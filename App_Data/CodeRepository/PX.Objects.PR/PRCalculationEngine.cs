/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PR.Utility;
using PX.Payroll.Data;
using PX.Payroll.Data.Vertex;
using PX.Payroll.Proxy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PX.Objects.PR
{
	[PXHidden]
	public partial class PRCalculationEngine : PXGraph<PRCalculationEngine>
	{
		#region Members

		public PaymentCalculationInfoCollection PaymentsToProcess { get; private set; }

		private List<PRPayment> _PaymentList;
		private bool _IsMassProcess;
		private CalculationResultInfo<PRPayChecksAndAdjustments, PRPaymentDeduct> _CalculationErrors;

		private Lazy<PRCalculationEngineUtils> _CalculationUtils = new Lazy<PRCalculationEngineUtils>(() => PXGraph.CreateInstance<PRCalculationEngineUtils>());

		#endregion

		#region DataView

		public PXSelect<PRPayment> Payments;

		public SelectFrom<PREarningDetail>.
			InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>.
			InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>.
			InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>.
			Where<PREarningDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
				And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>.
				And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.
			OrderBy<PREarningDetail.date.Asc, PREarningDetail.sortingRecordID.Asc, PREarningDetail.rate.Asc>.View PaymentEarningDetails;

		public SelectFrom<PRPaymentOvertimeRule>.
			InnerJoin<PROvertimeRule>.
				On<PRPaymentOvertimeRule.overtimeRuleID.IsEqual<PROvertimeRule.overtimeRuleID>>.
			InnerJoin<EPEarningType>
				.On<EPEarningType.typeCD.IsEqual<PROvertimeRule.disbursingTypeCD>>.
			Where<PRPaymentOvertimeRule.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
				And<PRPaymentOvertimeRule.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>.
				And<PRPaymentOvertimeRule.isActive.IsEqual<True>>.
				And<PROvertimeRule.isActive.IsEqual<True>>>.
			OrderBy<PROvertimeRule.overtimeMultiplier.Desc, PROvertimeRule.overtimeThreshold.Asc>.View OvertimeRulesForCalculation;

		public SelectFrom<Address>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
			.Where<Address.addressID.IsEqual<EPEmployee.defAddressID>>.View CurrentEmployeeResidenceAddress;

		public SelectFrom<Contact>
			.InnerJoin<EPEmployee>.On<EPEmployee.defContactID.IsEqual<Contact.contactID>>
			.Where<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View CurrentEmployeeContactInfo;

		public PXSelectJoin<PRPaymentEarning,
				InnerJoin<EPEarningType,
					On<PRPaymentEarning.typeCD,
						Equal<EPEarningType.typeCD>>,
				InnerJoin<PRLocation,
					On<PRPaymentEarning.locationID,
						Equal<PRLocation.locationID>>,
				InnerJoin<Address,
					On<PRLocation.addressID,
						Equal<Address.addressID>>>>>,
				Where<PRPaymentEarning.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentEarning.refNbr,
						Equal<Current<PRPayment.refNbr>>>>> Earnings;

		public SelectFrom<PRTaxCode>
			.LeftJoin<PREarningTypeDetail>.On<PRTaxCode.taxID.IsEqual<PREarningTypeDetail.taxID>
				.And<PREarningTypeDetail.typecd.IsEqual<P.AsString>>>
			.Where<PRTaxCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>.View EarningTaxList;

		public SelectFrom<PRPaymentDeduct>
				.InnerJoin<PRDeductCode>.On<PRPaymentDeduct.codeID.IsEqual<PRDeductCode.codeID>>
				.LeftJoin<PREmployeeDeduct>.On<PREmployeeDeduct.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>
					.And<PREmployeeDeduct.codeID.IsEqual<PRPaymentDeduct.codeID>>>
				.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
					.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
					.And<PRPaymentDeduct.saveOverride.IsEqual<True>
						.Or<PRPaymentDeduct.source.IsNotEqual<PaymentDeductionSourceAttribute.employeeSetting>>
						.Or<PREmployeeDeduct.codeID.IsNull>
						.Or<PREmployeeDeduct.startDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>
							.And<PREmployeeDeduct.endDate.IsGreaterEqual<PRPayment.transactionDate.FromCurrent>
								.Or<PREmployeeDeduct.endDate.IsNull>>>
						.Or<PRPaymentDeduct.isActive.IsEqual<True>
						.Or<PRDeductCode.affectsTaxes.IsEqual<True>
							.And<PRPaymentDeduct.ytdAmount.IsNotEqual<decimal0>
								.Or<PRPaymentDeduct.employerYtdAmount.IsNotEqual<decimal0>>>>>>>.View Deductions;

		public SelectFrom<PRPaymentDeduct>
			.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View AllPaymentDeductions;

		public SelectFrom<PRDeductionDetail>.
			Where<PRDeductionDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
			And<PRDeductionDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
			And<PRDeductionDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View DeductionDetails;

		public SelectFrom<PRBenefitDetail>.
			Where<PRBenefitDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>.
			And<PRBenefitDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>.
			And<PRBenefitDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View BenefitDetails;

		public SelectFrom<PREmployeeTax>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PREmployeeTax.taxID>>
			.Where<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PREmployeeTax.isActive.IsEqual<True>>
				.And<PRTaxCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>>.View TaxSettings;

		public SelectFrom<PRYtdTaxes>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRYtdTaxes.taxID>>
			.Where<PRYtdTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRYtdTaxes.year.IsEqual<P.AsString>>
				.And<PRTaxCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>>.View EmployeeYTDTaxes;

		public SelectFrom<PRTaxCodeAttribute>
			.LeftJoin<PREmployeeTaxAttribute>.On<PREmployeeTaxAttribute.taxID.IsEqual<PRTaxCodeAttribute.taxID>
				.And<PREmployeeTaxAttribute.settingName.IsEqual<PRTaxCodeAttribute.settingName>>
				.And<PREmployeeTaxAttribute.typeName.IsEqual<PRTaxCodeAttribute.typeName>
					.Or<PRTaxCodeAttribute.typeName.IsNull>>
				.And<PREmployeeTaxAttribute.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>>
			.InnerJoin<PREmployeeTax>.On<PREmployeeTax.taxID.IsEqual<PRTaxCodeAttribute.taxID>>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRTaxCodeAttribute.taxID>>
			.Where<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRTaxCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>>.View TaxSettingAttributes;

		public SelectFrom<PRCompanyTaxAttribute>
			.InnerJoin<PREmployeeTax>.On<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PREmployeeTax.taxID>
				.And<PRTaxCode.taxState.IsEqual<PRCompanyTaxAttribute.state>>
				.Or<PRCompanyTaxAttribute.state.IsEqual<BQLLocationConstants.FederalUS>>
				.Or<PRCompanyTaxAttribute.state.IsEqual<BQLLocationConstants.FederalCAN>>>
			.LeftJoin<PREmployeeAttribute>.On<PREmployeeAttribute.settingName.IsEqual<PRCompanyTaxAttribute.settingName>
				.And<PREmployeeAttribute.typeName.IsEqual<PRCompanyTaxAttribute.typeName>
					.Or<PRCompanyTaxAttribute.typeName.IsNull>>
				.And<PREmployeeAttribute.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>>
			.Where<PRCompanyTaxAttribute.countryID.IsEqual<PRPayment.countryID.FromCurrent>>
			.AggregateTo<GroupBy<PRCompanyTaxAttribute.settingName>>.View EmployeeAttributes;

		public PXSelect<PRPaymentTax,
				Where<PRPaymentTax.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentTax.refNbr,
						Equal<Current<PRPayment.refNbr>>>>> PaymentTaxes;

		public PXSelect<PRPaymentTaxSplit,
				Where<PRPaymentTaxSplit.docType,
						Equal<Current<PRPayment.docType>>,
					And<PRPaymentTaxSplit.refNbr,
						Equal<Current<PRPayment.refNbr>>,
				And<PRPaymentTaxSplit.taxID,
					Equal<Current<PRPaymentTax.taxID>>>>>> PaymentTaxesSplit;

		public SelectFrom<PRPaymentTaxApplicableAmounts>
			.Where<PRPaymentTaxApplicableAmounts.FK.Payment.SameAsCurrent>.View PaymentTaxApplicableAmounts;

		public SelectFrom<PRYtdTaxes>
			.Where<PRYtdTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdTaxes.taxID.IsEqual<P.AsInt>
			.And<PRYtdTaxes.year.IsEqual<P.AsString>>>>.View YTDTaxes;

		public SelectFrom<PRPeriodTaxes>
			.Where<PRPeriodTaxes.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRPeriodTaxes.taxID.IsEqual<P.AsInt>>
			.And<PRPeriodTaxes.year.IsEqual<P.AsString>>>.View PeriodTaxes;

		public SelectFrom<PRPeriodTaxApplicableAmounts>
			.Where<PRPeriodTaxApplicableAmounts.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRPeriodTaxApplicableAmounts.taxID.IsEqual<P.AsInt>>
				.And<PRPeriodTaxApplicableAmounts.year.IsEqual<P.AsString>>>.View PeriodTaxApplicableAmounts;

		public SelectFrom<PRTaxDetail>
			.Where<PRTaxDetail.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRTaxDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRTaxDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>.View TaxDetails;

		public SelectFrom<PRYtdDeductions>
			.Where<PRYtdDeductions.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
			.And<PRYtdDeductions.codeID.IsEqual<P.AsInt>
			.And<PRYtdDeductions.year.IsEqual<P.AsString>>>>.View YtdDeduction;

		public SelectFrom<PRPayGroupPeriod>
			.InnerJoin<PRPayGroupYear>.On<PRPayGroupYear.payGroupID.IsEqual<PRPayGroupPeriod.payGroupID>
				.And<PRPayGroupYear.year.IsEqual<PRPayGroupPeriod.finYear>>>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayment.payGroupID.FromCurrent>
				.And<PRPayGroupPeriod.finPeriodID.IsEqual<PRPayment.payPeriodID.FromCurrent>>>.View PayPeriod;

		public SelectFrom<PRPayGroupPeriod>
			.Where<PRPayGroupPeriod.payGroupID.IsEqual<P.AsString>
				.And<PRPayGroupPeriod.finYear.IsEqual<P.AsString>>
				.And<PRPayGroupPeriod.transactionDate.IsLess<P.AsDateTime.UTC>>>
			.OrderBy<PRPayGroupPeriod.transactionDate.Desc>.View PreviousPayPeriod;

		public SelectFrom<PRPayGroupYear>
			.Where<PRPayGroupYear.payGroupID.IsEqual<PRPayment.payGroupID.FromCurrent>
				.And<PRPayGroupYear.year.IsEqual<P.AsString>>>.View PayrollYear;

		public FringeBenefitApplicableEarningsQuery FringeBenefitApplicableEarnings;

		public SelectFrom<PRProjectFringeBenefitRateReducingDeduct>
			.Where<PRProjectFringeBenefitRateReducingDeduct.projectID.IsEqual<P.AsInt>
				.And<PRProjectFringeBenefitRateReducingDeduct.isActive.IsEqual<True>>>.View FringeBenefitRateReducingDeductions;

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.projectID.IsEqual<P.AsInt>>
				.And<PREarningDetail.projectTaskID.IsEqual<P.AsInt>>
				.And<PREarningDetail.labourItemID.IsEqual<P.AsInt>>
				.And<PREarningDetail.certifiedJob.IsEqual<True>>>
			.AggregateTo<Sum<PREarningDetail.hours>>.View ProjectHours;

		public SelectFrom<PREarningDetail>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.certifiedJob.IsEqual<True>>>
			.AggregateTo<Sum<PREarningDetail.hours>>.View AllProjectHours;

		public SelectFrom<PRSetup>.View PayrollPreferences;

		public ProjectDeductionQuery ProjectDeductions;

		public UnionDeductionQuery UnionDeductions;

		public SelectFrom<PREarningDetail>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.state.IsEqual<Address.state>
				.And<PRDeductCode.isWorkersCompensation.IsEqual<True>>
				.And<PRDeductCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.workCodeID.IsNotNull>
				.And<PREarningType.isWCCCalculation.IsEqual<True>>>
			.OrderBy<PREarningDetail.date.Asc, PREarningDetail.isOvertime.Asc>.View WorkCodeEarnings;

		public SelectFrom<PRPaymentWCPremium>
			.Where<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View WCPremiums;

		public SelectFrom<PRPaymentWCPremium>
			.InnerJoin<PRDeductCode>.On<PRPaymentWCPremium.FK.DeductionCode>
			.InnerJoin<PRPayment>.On<PRPaymentWCPremium.FK.Payment>
			.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRPayment.released.IsEqual<True>>
				.And<PRPaymentWCPremium.contribType.IsEqual<ContributionTypeListAttribute.bothDeductionAndContribution>
					.Or<PRPaymentWCPremium.contribType.IsEqual<P.AsString>>>>.View YtdWCPremiums;

		public SelectFrom<PRWorkCompensationMaximumInsurableWage>
			.InnerJoin<PRDeductCode>.On<PRWorkCompensationMaximumInsurableWage.FK.DeductionCode>
			.Where<PRWorkCompensationMaximumInsurableWage.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>.View WCMaximumInsurableWages;

		public SelectFrom<PRPaymentProjectPackageDeduct>
			.Where<PRPaymentProjectPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentProjectPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View ProjectPackageDeductions;

		public SelectFrom<PRPaymentUnionPackageDeduct>
			.Where<PRPaymentUnionPackageDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentUnionPackageDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View UnionPackageDeductions;

		public SelectFrom<PREarningDetail>
			.Where<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<True>
					.Or<PREarningDetail.isPayingCarryover.IsEqual<True>>>>.View CalculatedEarnings;

		public SelectFrom<PRDirectDepositSplit>
			.Where<PRDirectDepositSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRDirectDepositSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View DirectDepositSplits;

		public SelectFrom<CABatch>
			.Where<CABatch.batchNbr.IsEqual<PRPayment.paymentBatchNbr.FromCurrent>>.View PaymentBatch;

		public SelectFrom<CABatchDetail>
			.Where<CABatchDetail.origDocType.IsEqual<PRPayment.docType.FromCurrent>
				.And<CABatchDetail.origRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<CABatchDetail.origModule.IsEqual<BatchModule.modulePR>>>.View PaymentBatchDetails;

		public SelectFrom<PREarningDetail>
			.InnerJoin<PRPaymentEarning>.On<PRPaymentEarning.refNbr.IsEqual<PREarningDetail.paymentRefNbr>
				.And<PRPaymentEarning.docType.IsEqual<PREarningDetail.paymentDocType>>
				.And<PRPaymentEarning.locationID.IsEqual<PREarningDetail.locationID>>
				.And<PRPaymentEarning.typeCD.IsEqual<PREarningDetail.typeCD>>>
			.InnerJoin<PRLocation>.On<PRLocation.locationID.IsEqual<PREarningDetail.locationID>>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<True>>>
			.AggregateTo<GroupBy<PREarningDetail.typeCD>, GroupBy<PREarningDetail.locationID>, Sum<PREarningDetail.amount>, Sum<PREarningDetail.hours>>.View FringeBenefitCalculatedEarnings;

		public SelectFrom<PRPayment>
			.Where<PRPayment.employeeID.IsEqual<PRPayment.employeeID.FromCurrent>
				.And<PRPayment.transactionDate.IsLess<PRPayment.transactionDate.FromCurrent>>
				.And<PRPayment.released.IsNotEqual<True>>>
			.OrderBy<PRPayment.transactionDate.Asc>.View OlderUnreleasedPayments;

		public SelectFrom<PMLaborCostRate>
			.Where<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.certified>
				.And<PMLaborCostRate.projectID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.inventoryID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.effectiveDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<PMLaborCostRate.taskID.IsEqual<P.AsInt>
					.Or<PMLaborCostRate.taskID.IsNull>>>
			.OrderBy<PMLaborCostRate.taskID.Desc, PMLaborCostRate.effectiveDate.Desc>.View PrevailingWage;

		public SelectFrom<PRPaymentFringeBenefit>
			.Where<PRPaymentFringeBenefit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentFringeBenefits;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.Where<PRPaymentFringeBenefitDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeBenefitDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeBenefitDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeBenefitsDecreasingRate;

		public SelectFrom<PRPaymentFringeEarningDecreasingRate>
			.Where<PRPaymentFringeEarningDecreasingRate.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentFringeEarningDecreasingRate.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectID.IsEqual<PRPaymentFringeBenefit.projectID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.laborItemID.IsEqual<PRPaymentFringeBenefit.laborItemID.FromCurrent>>
				.And<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsEqual<PRPaymentFringeBenefit.projectTaskID.FromCurrent>
					.Or<PRPaymentFringeEarningDecreasingRate.projectTaskID.IsNull
						.And<PRPaymentFringeBenefit.projectTaskID.FromCurrent.IsNull>>>>.View PaymentFringeEarningsDecreasingRate;

		public SelectFrom<PRPaymentPTOBank>
			.Where<PRPaymentPTOBank.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentPTOBank.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentPTOBanks;

		public PTOHelper.PTOBankSelect.View PTOBanks;

		// To enable the defaulting of account/sub fields in the DAC
		public PXSelect<PTODisbursementEarningDetail> DummyPTODisbursementEarningDetail;
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PaymentRegularAmount(nameof(PaymentEarningDetails))]
		protected virtual void _(Events.CacheAttached<PRPayment.regularAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseAccountAttribute), nameof(BenExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(BenExpenseSubAccountAttribute), nameof(BenExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRBenefitDetail.expenseSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseAccountAttribute), nameof(TaxExpenseAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseAccountID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxExpenseSubAccountAttribute), nameof(TaxExpenseSubAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.expenseSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(TaxLiabilityAccountAttribute), nameof(TaxLiabilityAccountAttribute.CheckIfEmpty), false)]
		public void _(Events.CacheAttached<PRTaxDetail.liabilityAccountID> e) { }
		#endregion CacheAttached

		#region Events
		public void _(Events.FieldUpdated<PRPaymentTax.taxID> e)
		{
			var row = e.Row as PRPaymentTax;
			if (row == null)
			{
				return;
			}

			UpdateTax(row);
		}
		#endregion Events

		#region Static Accessors

		public static void Run(List<PRPayment> payments, bool isMassProcess = true)
		{
			var calculationEngine = PXGraph.CreateInstance<PRCalculationEngine>();
			calculationEngine._IsMassProcess = isMassProcess;
			calculationEngine._PaymentList = payments;

			try
			{
				calculationEngine.Run();
				PRValidatePaycheckTotals validationGraph = CreateInstance<PRValidatePaycheckTotals>();
				foreach (PRPayment payment in calculationEngine.PaymentsToProcess.GetAllPayments())
				{
					validationGraph.ValidateTotals(payment, true);
				}
			}
			catch (Exception ex)
			{
				if (isMassProcess)
				{
					throw new CalculationEngineException(calculationEngine.Payments.Current, ex);
				}
				else
				{
					throw;
				}
			}
		}

		public static void CreateTaxDetail(PXGraph graph, PRTaxCode taxCode, PRPaymentTax paymentTax, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateTaxDetail(graph, taxCode, paymentTax, earnings, out TaxEarningDetailsSplits _);
		}

		public static void CreateDeductionDetail(PXGraph graph, PXCache deductionDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateDeductionDetail(graph, deductionDetailViewCache, deduction, earnings);
		}

		public static void CreateBenefitDetail(PXGraph graph, PXCache benefitDetailViewCache, PRPaymentDeduct deduction, IEnumerable<PREarningDetail> earnings)
		{
			PXGraph.CreateInstance<PRCalculationEngineUtils>().CreateBenefitDetail(graph, benefitDetailViewCache, deduction, earnings);
		}

		#endregion

		#region Run

		protected virtual void Run()
		{
			PayrollPreferences.Current = PayrollPreferences.SelectSingle();
			_CalculationErrors = new CalculationResultInfo<PRPayChecksAndAdjustments, PRPaymentDeduct>();
			PXLongOperation.SetCustomInfo(_CalculationErrors);

			DeleteEmptyEarnings();
			DeleteCalculatedEarningLines(ref _PaymentList);
			AddMissingLocationCodes();
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				ResetPTOInfo();
			}
			PaymentsToProcess = ValidateInputs();

			UpdateYTDValues();
			InsertPaidCarryoverEarnings();
			CalculatePaymentOvertimeRules();
			RecordContributionPayableBenefits();
			CalculatePTO();
			AfterCalculationPTOProcess();
			List<PRPayrollCalculation> calculations = CalculatePayroll().ToList();
			SavePayrollCalculations(calculations);
			CalculatePostTaxBenefits(calculations);
			CalculateFringeBenefitRates(calculations);
			SetDirectDepositSplit(calculations);
			SetCalculatedStatus(calculations);
		}

		#endregion

		#region Prepare Data
		protected virtual void DeleteEmptyEarnings()
		{
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				List<PREarningDetail> earningDetails = PaymentEarningDetails.Select().FirstTableItems.ToList();
				foreach (PREarningDetail zeroEarning in earningDetails.Where(x => x.Hours == 0 && x.Amount == 0))
				{
					if ((zeroEarning.SourceNoteID == null || zeroEarning.SourceType != EarningDetailSourceType.TimeActivity) && earningDetails.All(x => x.BaseOvertimeRecordID != zeroEarning.RecordID))
					{
						PaymentEarningDetails.Delete(zeroEarning);
					}
				}

				PRPayChecksAndAdjustments.DeleteEmptySummaryEarnings(Earnings.View, PaymentEarningDetails.Cache);
			}
		}

		protected virtual void DeleteCalculatedEarningLines(ref List<PRPayment> paymentList)
		{
			List<PRPayment> updatedPayments = new List<PRPayment>();
			foreach (PRPayment payment in paymentList)
			{
				Payments.Current = payment;
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, false);
				PRPayChecksAndAdjustments.RevertPaymentOvertimeCalculation(this, payment, PaymentEarningDetails.View);
				CalculatedEarnings.Select().ForEach(x => CalculatedEarnings.Delete(x));
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, true);
				updatedPayments.Add(Payments.Current);
			}

			Actions.PressSave();
			paymentList = updatedPayments;
		}

		protected virtual void AddMissingLocationCodes()
		{
			IEnumerable<Address> addressesWithoutLocationCode = new List<Address>();
			TaxLocationHelpers.AddressEqualityComparer comparer = new TaxLocationHelpers.AddressEqualityComparer();
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				addressesWithoutLocationCode = addressesWithoutLocationCode.Union(
					Earnings.Select().ToList()
						.Select(x => (Address)x[typeof(Address)])
						.Where(x => string.IsNullOrWhiteSpace(x.TaxLocationCode))
						.Distinct(x => x.AddressID),
					comparer);

				PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as PREmployee;
				Address employeeAddress = (Address)PXSelectorAttribute.Select<EPEmployee.defAddressID>(Caches[typeof(EPEmployee)], employee);
				if (!TaxLocationHelpers.ValidateTaxLocationCode(employeeAddress.TaxLocationCode, payment.CountryID))
				{
					addressesWithoutLocationCode = addressesWithoutLocationCode.Union(new List<Address> { employeeAddress }, comparer);
				}
			}

			if (addressesWithoutLocationCode.Any())
			{
				try
				{
					TaxLocationHelpers.UpdateAddressLocationCodes(addressesWithoutLocationCode.ToList());
				}
				catch { } // If location update fails, error will be thrown from ValidateInputs()
			}
		}

		//ToDo: Review, which additional checks might be necessary here, AC-146235
		protected virtual PaymentCalculationInfoCollection ValidateInputs()
		{
			HashSet<string> activeEarningTypes = new HashSet<string>(
				SelectFrom<EPEarningType>.
				Where<EPEarningType.isActive.IsEqual<True>>.View.
				Select(this).FirstTableItems.Select(item => item.TypeCD));

			var validPayments = new PaymentCalculationInfoCollection(this);
			foreach (PRPayment payment in _PaymentList)
			{
				Payments.Current = payment;
				string errorMessage = null;

				PRPayment olderUnreleasedPayment = OlderUnreleasedPayments.Select().FirstOrDefault();
				if (olderUnreleasedPayment != null)
				{
					errorMessage = PXMessages.LocalizeFormat(Messages.CannotCalculateBecauseOlderPaymentIsUnreleased, olderUnreleasedPayment.PaymentDocAndRef);
				}
				else
				{
					PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as PREmployee;

					if (payment.CountryID == LocationConstants.CanadaCountryCode && CurrentEmployeeContactInfo.SelectSingle().DateOfBirth == null)
					{
						throw new PXException(Messages.MandatoryDOB);
					}

					IEnumerable<PREmployeeClassWorkLocation> employeeClassWorkLocations = null;
					IEnumerable<PREmployeeWorkLocation> employeeWorkLocations = null;
					if (employee.LocationUseDflt == true)
					{
						employeeClassWorkLocations = SelectFrom<PREmployeeClassWorkLocation>
							.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<P.AsString>>.View.Select(this, employee.EmployeeClassID).FirstTableItems;
					}
					else
					{
						employeeWorkLocations = SelectFrom<PREmployeeWorkLocation>
							.Where<PREmployeeWorkLocation.employeeID.IsEqual<P.AsInt>>.View.Select(this, employee.BAccountID).FirstTableItems;
					}

					foreach (PXResult<PREarningDetail, PRLocation, Address> result in PaymentEarningDetails.Select())
					{
						PREarningDetail earningDetail = (PREarningDetail)result;
						PRLocation workLocation = (PRLocation)result;
						Address locationAddress = (Address)result;

						if (earningDetail.Amount < 0)
						{
							errorMessage = Messages.CantCalculateNegative;
							break;
						}

						if (earningDetail.Date == null || earningDetail.Date < payment.StartDate || earningDetail.Date > payment.EndDate)
						{
							errorMessage = DateInPeriodAttribute.GetIncorrectDateMessage(payment.StartDate.GetValueOrDefault(), payment.EndDate.GetValueOrDefault());
							break;
						}

						if (!activeEarningTypes.Contains(earningDetail.TypeCD))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.EarningDetailsWithInactiveEarningTypes, earningDetail.TypeCD);
							break;
						}

						if (string.IsNullOrWhiteSpace(locationAddress.TaxLocationCode))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.AddressNotRecognized, workLocation.LocationCD);
							break;
						}

						if (employee.LocationUseDflt == true && !employeeClassWorkLocations.Any(x => x.LocationID == workLocation.LocationID))
						{
							errorMessage = PXMessages.LocalizeFormat(Messages.LocationNotSetInEmployeeClass, workLocation.LocationCD, employee.EmployeeClassID);
							break;
						}

						if (employee.LocationUseDflt != true && !employeeWorkLocations.Any(x => x.LocationID == workLocation.LocationID))
						{
							errorMessage = Messages.PaycheckEarningLocationNotAssignedToEmployee;
							break;
						}
					}

					if (errorMessage == null && SelectFrom<PRPaymentDeduct>
							.Where<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>
								.And<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
								.And<PRPaymentDeduct.isActive.IsEqual<True>>
								.And<PRPaymentDeduct.saveOverride.IsEqual<True>>
								.And<PRPaymentDeduct.dedAmount.IsLess<decimal0>
									.Or<PRPaymentDeduct.cntAmount.IsLess<decimal0>>>>.View.Select(this).Any())
					{

						errorMessage = Messages.CantCalculateNegative;
					}

					if (errorMessage == null)
					{
						Address employeeAddress = (Address)PXSelectorAttribute.Select<EPEmployee.defAddressID>(Caches[typeof(EPEmployee)], employee);
						if (string.IsNullOrWhiteSpace(employeeAddress.TaxLocationCode))
						{
							errorMessage = Messages.EmployeeAddressNotRecognized;
						}
					}

					if (errorMessage == null &&
						new SelectFrom<PREmployeeTax>.Where<PREmployeeTax.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View(this).SelectSingle() == null)
					{
						errorMessage = Messages.EmployeeHasNoTaxes;
					}
				}

				if (errorMessage != null)
				{
					throw new PXException(errorMessage);
				}
				else
				{
					validPayments.Add(payment);
				}
			}
			return validPayments;
		}

		protected virtual void UpdateYTDValues()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;

				Earnings.Select().ForEach(x =>
				{
					PRPayChecksAndAdjustments.UpdateSummaryEarning(this, payment, x);
					Earnings.Update(x);
				});
				AllPaymentDeductions.Select().ForEach(x =>
				{
					PRPayChecksAndAdjustments.UpdateSummaryDeductions(this, payment, x);
					AllPaymentDeductions.Update(x);
				});
				PaymentTaxes.Select().ForEach(x => PaymentTaxes.Cache.SetDefaultExt<PRPaymentTax.ytdAmount>(x));
			}

			Actions.PressSave();
		}

		protected IEnumerable<PRPayrollCalculation> CalculatePayroll()
		{
			List<PRPayrollCalculation> calculations = new List<PRPayrollCalculation>();
			foreach (IGrouping<string, PRPayment> paymentsByCountry in PaymentsToProcess.GetAllPayments().GroupBy(x => x.CountryID))
			{
				if (paymentsByCountry.Key == LocationConstants.CanadaCountryCode)
				{
					calculations.AddRange(CalculateCanadaPayroll(paymentsByCountry));
				}
				else if (paymentsByCountry.Key == LocationConstants.USCountryCode)
				{
					calculations.AddRange(CalculateUSPayroll(paymentsByCountry));
				}
				else
				{
					throw new PXException(Messages.CannotCalculateCountry, paymentsByCountry.Key);
				}
			}

			return calculations;
		}

		protected virtual T CreatePayrollBase<T>(PRPayment payment)
			where T : PRPayrollBase, new()
		{
			PXResult<PRPayGroupPeriod, PRPayGroupYear> result = (PXResult<PRPayGroupPeriod, PRPayGroupYear>)PayPeriod.Select();
			PRPayGroupPeriod period = (PRPayGroupPeriod)result;
			PRPayGroupYear year = (PRPayGroupYear)result;

			PXResultset<PRPayGroupPeriod> yearPeriods = SelectFrom<PRPayGroupPeriod>
				.Where<PRPayGroupPeriod.payGroupID.IsEqual<PRPayGroupYear.payGroupID.FromCurrent>
					.And<PRPayGroupPeriod.finYear.IsEqual<PRPayGroupYear.year.FromCurrent>>>
				.OrderBy<PRPayGroupPeriod.transactionDate.Asc, PRPayGroupPeriod.startDate.Asc>.View.SelectMultiBound(this, new object[] { year });

			// Since periods shifted from other years have an alpha value for the PeriodNbr field, order all periods in year and find the
			// position in the resulting list.
			int periodNbr = yearPeriods.FirstTableItems.Select(x => x.FinPeriodID).ToList().IndexOf(period?.FinPeriodID) + 1;

			if (period?.FinPeriodID == null || year?.FinPeriods == null || periodNbr == 0)
			{
				throw new PXException(Messages.InvalidPayPeriod);
			}

			var payroll = new T()
			{
				ReferenceNbr = payment.PaymentDocAndRef,
				PayDate = payment.TransactionDate.GetValueOrDefault(),
				PayPeriodNumber = (short)periodNbr,
				PayPeriodsPerYear = year.FinPeriods.Value
			};

			return payroll;
		}

		protected virtual void GetAggregateRecords<T>(
			List<T> ytdRecords,
			out List<T> periodToDateRecords,
			out List<T> wtdRecords,
			out List<T> mtdRecords,
			out List<T> qtdRecords,
			out List<T> previoudPeriodRecords)
			where T : IAggregatePaycheckData
		{
			PRPayGroupYear payYear = PayrollYear.SelectSingle(Payments.Current.TransactionDate.Value.Year.ToString());
			PRPayGroupPeriod period = PayPeriod.SelectSingle();
			PRPayGroupPeriod previousPeriod = PreviousPayPeriod.SelectSingle(period?.PayGroupID, period?.FinYear, period?.TransactionDate);
			DayOfWeek payWeekStart = payYear.StartDate.Value.DayOfWeek == DayOfWeek.Saturday ? DayOfWeek.Sunday : payYear.StartDate.Value.DayOfWeek + 1;
			int weekOfYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(Payments.Current.TransactionDate.Value, CalendarWeekRule.FirstDay, payWeekStart);
			int[] quarterMonths = PRDateTime.GetQuarterMonths(Payments.Current.TransactionDate.Value);

			periodToDateRecords = ytdRecords.Where(x => x.PeriodNbr == period?.PeriodNbrAsInt).ToList();
			wtdRecords = ytdRecords.Where(x => x.Week == weekOfYear).ToList();
			mtdRecords = ytdRecords.Where(x => x.Month == Payments.Current.TransactionDate.Value.Month).ToList();
			qtdRecords = ytdRecords.Join(quarterMonths, result => result.Month, month => month, (result, month) => result).ToList();
			previoudPeriodRecords = ytdRecords.Where(x => x.PeriodNbr == previousPeriod?.PeriodNbrAsInt).ToList();
		}

		#region Settings (common Canada/US)
		protected virtual Dictionary<int?, List<ISettingDefinition>> CreateTaxSettingDictionary()
		{
			Dictionary<int?, List<ISettingDefinition>> settings = new Dictionary<int?, List<ISettingDefinition>>();

			foreach (IGrouping<int?, PXResult<PRTaxCodeAttribute, PREmployeeTaxAttribute>> grouping in TaxSettingAttributes
				.Select()
				.Select(x => (PXResult<PRTaxCodeAttribute, PREmployeeTaxAttribute>)x)
				.GroupBy(x => ((PRTaxCodeAttribute)x).TaxID))
			{
				List<ISettingDefinition> settingsForTax = new List<ISettingDefinition>();
				foreach (PXResult<PRTaxCodeAttribute, PREmployeeTaxAttribute> result in grouping)
				{
					PRTaxCodeAttribute companyTaxSettingAttribute = result;
					PREmployeeTaxAttribute employeeTaxSettingAttribute = result;

					ISettingDefinition setting = employeeTaxSettingAttribute?.TaxID == null || employeeTaxSettingAttribute?.UseDefault == true || companyTaxSettingAttribute.AllowOverride != true ?
						(ISettingDefinition)companyTaxSettingAttribute :
						(ISettingDefinition)employeeTaxSettingAttribute;
					if (string.IsNullOrEmpty(setting?.Value) && companyTaxSettingAttribute.Required == true)
					{
						PRTaxCode taxCode = SelectFrom<PRTaxCode>.View.Search<PRTaxCode.taxID>(this, companyTaxSettingAttribute.TaxID);
						StringBuilder sb = new StringBuilder(PXMessages.LocalizeFormatNoPrefix(Messages.RequiredTaxSettingNullInCalculateTraceFormat, companyTaxSettingAttribute.SettingName, taxCode?.TaxCD));
						sb.AppendLine();
						sb.AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.EmployeeTaxSettingValueTraceFormat, employeeTaxSettingAttribute?.Value ?? "null"));
						sb.AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.CompanyTaxSettingValueTraceFormat, companyTaxSettingAttribute.Value ?? "null"));
						PXTrace.WriteError(sb.ToString());
						throw new PXException(Messages.RequiredTaxSettingNullInCalculate);
					}

					settingsForTax.Add(setting);
				}

				settings[grouping.Key] = settingsForTax;
			}

			return settings;
		}

		protected virtual IEnumerable<ISettingDefinition> GetEmployeeSettings()
		{
			HashSet<string> employeeTaxStates = TaxSettings.Select<PRTaxCode>().Select(x => x.TaxState).ToHashSet();

			foreach (PXResult<PRCompanyTaxAttribute, PREmployeeTax, PRTaxCode, PREmployeeAttribute> result in EmployeeAttributes.Select()
				.Select(x => (PXResult<PRCompanyTaxAttribute, PREmployeeTax, PRTaxCode, PREmployeeAttribute>)x)
				// Force immediate query evaluation because the following Where doesn't play nice with PX.Data.SQLTree.SQLinqExecutor
				.ToList()
				.Where(x => ((PRCompanyTaxAttribute)x).State == LocationConstants.USFederalStateCode || ((PRCompanyTaxAttribute)x).State == LocationConstants.CanadaFederalStateCode || employeeTaxStates.Contains(((PRCompanyTaxAttribute)x).State)))
			{
				PRCompanyTaxAttribute companyAttribute = result;
				PREmployeeAttribute employeeAttribute = result;

				ISettingDefinition setting = employeeAttribute?.BAccountID == null || employeeAttribute?.UseDefault == true || companyAttribute.AllowOverride != true ?
					(ISettingDefinition)companyAttribute :
					(ISettingDefinition)employeeAttribute;
				if (string.IsNullOrEmpty(setting?.Value) && companyAttribute.Required == true)
				{
					StringBuilder sb = new StringBuilder(PXMessages.LocalizeFormatNoPrefix(Messages.RequiredEmployeeSettingNullInCalculateTraceFormat, companyAttribute.SettingName, companyAttribute.State));
					sb.AppendLine();
					sb.AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.EmployeeTaxSettingValueTraceFormat, employeeAttribute?.Value ?? "null"));
					sb.AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.CompanyTaxSettingValueTraceFormat, companyAttribute.Value ?? "null"));
					PXTrace.WriteError(sb.ToString());
					throw new PXException(Messages.RequiredTaxSettingNullInCalculate);
				}

				yield return setting;
			}
		}
		#endregion Settings (common Canada/US)

		#region Wages (common Canada/US)
		protected virtual IEnumerable<PRWage> PrepareWageData()
		{
			List<PXResult<PREarningTypeDetail, PRTaxCode>> earningTypeTaxabilityRecords = SelectFrom<PREarningTypeDetail>
				.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PREarningTypeDetail.taxID>>.View
				.Select(this)
				.Select(x => (PXResult<PREarningTypeDetail, PRTaxCode>)x)
				.ToList();
			foreach (PXResult<PRPaymentEarning, EPEarningType, PRLocation, Address> earning in Earnings.Select())
			{
				PRWage wage = CreateWage((PRPaymentEarning)earning, (EPEarningType)earning, (Address)earning, earningTypeTaxabilityRecords);
				if (wage != null)
				{
					yield return wage;
				}
			}
		}

		protected virtual PRWage CreateWage(PRPaymentEarning paymentEarning, EPEarningType earningType, Address address, List<PXResult<PREarningTypeDetail, PRTaxCode>> earningTypeTaxabilityRecords)
		{
			if (Payments.Current.CountryID == LocationConstants.CanadaCountryCode)
			{
				VertexWage wage = CreateWage<VertexWage, PREarningType.wageTypeCDCAN>(paymentEarning, earningType, address) as VertexWage;
				PREarningType prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(earningType);
				wage.IsSupplemental = prEarningType.IsSupplementalCAN.GetValueOrDefault();
				foreach (PXResult<PREarningTypeDetail, PRTaxCode> result in earningTypeTaxabilityRecords.Where(x => ((PREarningTypeDetail)x).TypeCD == earningType.TypeCD))
				{
					PREarningTypeDetail taxability = result;
					PRTaxCode taxCode = result;
					wage.Taxability.Add(new WageTypeTaxability()
					{
						TaxUniqueCode = taxCode.TaxUniqueCode,
						CompensationType = (CompensationType)taxability.Taxability.GetValueOrDefault()
					});
				}

				return wage;
			}
			else
			{
				return CreateWage<PRWage, PREarningType.wageTypeCD>(paymentEarning, earningType, address) as PRWage;
			}
		}

		protected virtual PRWage CreateWage<TWageDataType, TWageTypeField>(PRPaymentEarning paymentEarning, EPEarningType earningType, Address address)
			where TWageDataType : PRWage, new()
			where TWageTypeField : IBqlField
		{
			if (address.CountryID != Payments.Current.CountryID)
			{
				return null;
			}

			PXCache prEarningTypeCache = Caches[typeof(PREarningType)];
			string includeType = prEarningTypeCache.GetValue<PREarningType.includeType>(earningType)?.ToString();
			int wageType = (prEarningTypeCache.GetValue<TWageTypeField>(earningType) as int?).GetValueOrDefault();

			PRWage wage = includeType == SubjectToTaxes.PerTaxEngine || Payments.Current.CountryID != LocationConstants.USCountryCode
					   ? (PRWage)new TWageDataType()
					   : CreateCustomWage(earningType.TypeCD, includeType);

			wage.Name = paymentEarning.TypeCD;
			wage.LocationCode = address.TaxLocationCode;
			wage.TaxMunicipalCode = address.TaxMunicipalCode;
			wage.TaxSchoolDistrictCode = address.TaxSchoolCode;
			wage.WageType = wageType;
			wage.IsCommission = PRSetupMaint.GetEarningTypeFromSetup<PRSetup.commissionType>(this) == earningType.TypeCD;
			wage.Hours = paymentEarning.Hours.GetValueOrDefault();
			wage.Amount = paymentEarning.Amount.GetValueOrDefault();
			wage.MTDAmount = paymentEarning?.MTDAmount ?? 0;
			wage.QTDAmount = paymentEarning?.QTDAmount ?? 0;
			wage.YTDAmount = paymentEarning?.YTDAmount ?? 0;

			PaymentsToProcess[Payments.Current].GrossWage += wage.Amount;

			return wage;
		}

		protected virtual PRCustomWage CreateCustomWage(string typeCD, string includeType)
		{
			var customWage = new PRCustomWage();
			customWage.NotSubjectToTaxCalculationMethod = SubjectToTaxes.Get(includeType);

			if (customWage.NotSubjectToTaxCalculationMethod == PRCustomItemCalculationMethod.FromList)
			{
				customWage.NotSubjectToTaxUniqueTaxIDs = GetWageTaxList(typeCD, includeType == SubjectToTaxes.AllButList).ToArray();
			}

			return customWage;
		}
		#endregion Wages (common Canada/US)

		#region Taxable deductions/benefits (common Canada/US)
		protected virtual IEnumerable<PRBenefit> PrepareTaxableBenefitData(bool firstPass)
		{
			List<PXResult<PRDeductCodeDetail, PRTaxCode>> taxabilityRecords = SelectFrom<PRDeductCodeDetail>
				.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRDeductCodeDetail.taxID>>.View
				.Select(this)
				.Select(x => (PXResult<PRDeductCodeDetail, PRTaxCode>)x)
				.ToList();

			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> deduction in GetDeductions(true))
			{
				PRDeductCode deductCode = (PRDeductCode)deduction;
				PRBenefit benefit;
				if (Payments.Current.CountryID == LocationConstants.CanadaCountryCode)
				{
					benefit = CreateBenefit<VertexDeduction, PRDeductCode.benefitTypeCDCAN>(deduction, deductCode, deduction, firstPass);
					VertexDeduction vertexDeduction = (VertexDeduction)benefit;
					foreach (PXResult<PRDeductCodeDetail, PRTaxCode> result in taxabilityRecords.Where(x => ((PRDeductCodeDetail)x).CodeID == deductCode.CodeID))
					{
						PRDeductCodeDetail taxability = result;
						PRTaxCode taxCode = result;
						vertexDeduction.ContribType = deductCode.ContribType;
						vertexDeduction.DeductionTaxability[taxCode.TaxUniqueCode] = taxability.IsDeductionPreTax.GetValueOrDefault();
						vertexDeduction.BenefitTaxability[taxCode.TaxUniqueCode] = taxability.IsBenefitTaxable.GetValueOrDefault();
					}
				}
				else
				{
					benefit = CreateBenefit<PRBenefit, PRDeductCode.benefitTypeCD>(deduction, deductCode, deduction, firstPass);
				}

				if (benefit == null)
				{
					continue;
				}

				DedBenAmount nominal = new DedBenAmount()
				{
					DeductionAmount = benefit.Amount,
					BenefitAmount = benefit.EmployerAmount
				};
				// Calculate proportion of benefit that comes from Employee/Benefit setting, project, union and WC so that
				// we can map calculation results later.
				PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID] = nominal;

				yield return benefit;
			}
		}

		protected virtual PRBenefit CreateBenefit<TBenefitDataType, TBenefitTypeField>(
			PRPaymentDeduct paymentDeduction,
			PRDeductCode deductionCode,
			PREmployeeDeduct employeeDeduction,
			bool firstPass)
			where TBenefitDataType : PRBenefit, new()
			where TBenefitTypeField : IBqlField
		{
			if (paymentDeduction.Source == DeductionSourceListAttribute.CertifiedProject
				&& Payments.Current.CountryID != LocationConstants.USCountryCode)
			{
				return null;
			}

			bool hasEmployeeDedOverride = employeeDeduction != null && employeeDeduction.DedUseDflt == false;
			bool hasEmployeeCntOverride = employeeDeduction != null && employeeDeduction.CntUseDflt == false;
			bool usePaymentDeductAmounts = paymentDeduction.SaveOverride == true || !firstPass;

			PRBenefit benefit = deductionCode.IncludeType == SubjectToTaxes.PerTaxEngine || Payments.Current.CountryID != LocationConstants.USCountryCode
				? (PRBenefit)new TBenefitDataType()
				: CreateCustomBenefit(deductionCode.CodeID, deductionCode.IncludeType);

			benefit.CodeCD = deductionCode.CodeCD;
			benefit.BenefitType = (Caches[typeof(PRDeductCode)].GetValue<TBenefitTypeField>(deductionCode) as int?).GetValueOrDefault();
			benefit.AllowSupplementalElection = deductionCode.AllowSupplementalElection == true;
			benefit.ProrateUsingStateWages = true;
			benefit.YTDAmount = paymentDeduction.YtdAmount.GetValueOrDefault();
			benefit.EmployerYTDAmount = paymentDeduction.EmployerYtdAmount.GetValueOrDefault();

			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductionCode, employeeDeduction);
			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductionCode, employeeDeduction);
			benefit.Limits = null;
			if (!usePaymentDeductAmounts &&
				(dedMaximumFrequency != DeductionMaxFrequencyType.NoMaximum ||
				cntMaximumFrequency != DeductionMaxFrequencyType.NoMaximum))
			{
				benefit.Limits = new PRBenefitLimit()
				{
					MaximumFrequency = DeductionMaxFrequencyType.ToEnum(dedMaximumFrequency),
					MaximumAmount = hasEmployeeDedOverride ? employeeDeduction.DedMaxAmount : deductionCode.DedMaxAmount,
					YtdAmount = paymentDeduction.YtdAmount.GetValueOrDefault(),
					EmployerMaximumFrequency = DeductionMaxFrequencyType.ToEnum(cntMaximumFrequency),
					EmployerMaximumAmount = hasEmployeeCntOverride ? employeeDeduction.CntMaxAmount : deductionCode.CntMaxAmount,
					EmployerYtdAmount = paymentDeduction.EmployerYtdAmount.GetValueOrDefault()
				};
			}

			var transactionDate = Payments.Current.TransactionDate;
			if (paymentDeduction.IsActive != true || (paymentDeduction.SaveOverride != true && (employeeDeduction.IsActive != true || (employeeDeduction.StartDate > transactionDate || (employeeDeduction.EndDate < transactionDate && employeeDeduction.EndDate.HasValue)))
				&& employeeDeduction.StartDate.HasValue))
			{
				benefit.Amount = 0;
				benefit.EmployerAmount = 0;
				return benefit;
			}
			else if (usePaymentDeductAmounts)
			{
				benefit.Amount = paymentDeduction.DedAmount.GetValueOrDefault();
				benefit.EmployerAmount = paymentDeduction.CntAmount.GetValueOrDefault();
				return benefit;
			}
			else
			{
				decimal? employeeAmount = null;
				decimal? employerAmount = null;

				switch (paymentDeduction.Source)
				{
					case PaymentDeductionSourceAttribute.EmployeeSettings:
						if (deductionCode.DedCalcType == DedCntCalculationMethod.FixedAmount)
						{
							employeeAmount = hasEmployeeDedOverride ? employeeDeduction.DedAmount : deductionCode.DedAmount;
						}
						else if (deductionCode.DedCalcType == DedCntCalculationMethod.AmountPerHour)
						{
							employeeAmount = (hasEmployeeDedOverride ? employeeDeduction.DedAmount : deductionCode.DedAmount) * GetDedBenApplicableHours(deductionCode, ContributionType.EmployeeDeduction);
						}
						else
						{
							employeeAmount = (hasEmployeeDedOverride ? employeeDeduction.DedPercent : deductionCode.DedPercent) * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployeeDeduction) / 100;
						}

						if (deductionCode.CntCalcType == DedCntCalculationMethod.FixedAmount)
						{
							employerAmount = hasEmployeeCntOverride ? employeeDeduction.CntAmount : deductionCode.CntAmount;
						}
						else if (deductionCode.CntCalcType == DedCntCalculationMethod.AmountPerHour)
						{
							employerAmount = (hasEmployeeCntOverride ? employeeDeduction.CntAmount : deductionCode.CntAmount) * GetDedBenApplicableHours(deductionCode, ContributionType.EmployerContribution);
						}
						else
						{
							employerAmount = (hasEmployeeCntOverride ? employeeDeduction.CntPercent : deductionCode.CntPercent) * GetDedBenApplicableAmount(deductionCode, ContributionType.EmployerContribution) / 100;
						}
						break;
					case PaymentDeductionSourceAttribute.CertifiedProject:
						HashSet<(int?, int?, int?)> projectPackageDeductionsApplied = new HashSet<(int?, int?, int?)>();

						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>> resultGroup in ProjectDeductions.Select(deductionCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitProjectPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType, deductionCode, this);
							switch (deductionCode.DedCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (package.DeductionAmount > 0 && !projectPackageDeductionsApplied.Contains((deductionCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.DeductionAmount = package.DeductionAmount;
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
									if (package.DeductionRate > 0)
									{
										calculation.DeductionAmount = package.DeductionRate * calculation.TotalAmountForDed / 100;
									}
									break;
								case DedCntCalculationMethod.AmountPerHour:
									if (package.DeductionAmount > 0)
									{
										calculation.DeductionAmount = package.DeductionAmount * calculation.TotalHoursForDed;
									}
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							switch (deductionCode.CntCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (package.BenefitAmount > 0 && !projectPackageDeductionsApplied.Contains((deductionCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.BenefitAmount = package.BenefitAmount;
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
									if (package.BenefitRate > 0)
									{
										calculation.BenefitAmount = package.BenefitRate * calculation.TotalAmountForBen / 100;
									}
									break;
								case DedCntCalculationMethod.AmountPerHour:
									if (package.BenefitAmount > 0)
									{
										calculation.BenefitAmount = package.BenefitAmount * calculation.TotalHoursForBen;
									}
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							RecordProjectPackageNominalAmounts(package, calculation);
							employeeAmount = employeeAmount.GetValueOrDefault() + calculation.DeductionAmount;
							employerAmount = employerAmount.GetValueOrDefault() + calculation.BenefitAmount;
							projectPackageDeductionsApplied.Add((deductionCode.CodeID, package.ProjectID, package.LaborItemID));
						}
						break;
					case PaymentDeductionSourceAttribute.Union:
						HashSet<(int?, string, int?)> unionPackageDeductionsApplied = new HashSet<(int?, string, int?)>();

						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>> resultGroup in UnionDeductions.Select(deductionCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitUnionPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType, deductionCode, this);
							if (deductionCode.ContribType != ContributionType.EmployerContribution)
							{
								switch (deductionCode.DedCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (package.DeductionAmount > 0 && !unionPackageDeductionsApplied.Contains((deductionCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.DeductionAmount = package.DeductionAmount;
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
										if (package.DeductionRate > 0)
										{
											calculation.DeductionAmount = package.DeductionRate * calculation.TotalAmountForDed / 100;
										}
										break;
									case DedCntCalculationMethod.AmountPerHour:
										if (package.DeductionAmount > 0)
										{
											calculation.DeductionAmount = package.DeductionAmount * calculation.TotalHoursForDed;

											if (deductionCode.DedApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarningsWithOTMult
												|| deductionCode.DedApplicableEarnings == DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithOTMult && earningType.IsOvertime == true)
											{
												calculation.DeductionAmount *= earningType.OvertimeMultiplier.GetValueOrDefault();
											}
										}
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							if (deductionCode.ContribType != ContributionType.EmployeeDeduction)
							{
								switch (deductionCode.CntCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (package.BenefitAmount > 0 && !unionPackageDeductionsApplied.Contains((deductionCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.BenefitAmount = package.BenefitAmount;
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
										if (package.BenefitRate > 0)
										{
											calculation.BenefitAmount = package.BenefitRate * calculation.TotalAmountForBen / 100;
										}
										break;
									case DedCntCalculationMethod.AmountPerHour:
										if (package.BenefitAmount > 0)
										{
											calculation.BenefitAmount = package.BenefitAmount * calculation.TotalHoursForBen;

											if (deductionCode.CntApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarningsWithOTMult
												|| deductionCode.CntApplicableEarnings == DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithOTMult && earningType.IsOvertime == true)
											{
												calculation.BenefitAmount *= earningType.OvertimeMultiplier.GetValueOrDefault();
											}
										}
										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							RecordUnionPackageNominalAmounts(package, calculation);
							employeeAmount = employeeAmount.GetValueOrDefault() + calculation.DeductionAmount;
							employerAmount = employerAmount.GetValueOrDefault() + calculation.BenefitAmount;
							unionPackageDeductionsApplied.Add((deductionCode.CodeID, package.UnionID, package.LaborItemID));
						}
						break;
				}

				employeeAmount = employeeAmount.HasValue ? Math.Max(employeeAmount.Value, 0m) : employeeAmount;
				employerAmount = employerAmount.HasValue ? Math.Max(employerAmount.Value, 0m) : employerAmount;
				benefit.Amount = employeeAmount.GetValueOrDefault();
				benefit.EmployerAmount = employerAmount.GetValueOrDefault();

				return benefit;
			}
		}
		#endregion Taxable deductions/benefits (common Canada/US)

		#region US tax calculation

		protected virtual IEnumerable<PRPayrollCalculation> CalculateUSPayroll(IEnumerable<PRPayment> payments)
		{
			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				foreach (PRPayment payment in payments)
				{
					Payments.Current = payment;
					var payrollBase = CreatePayrollBase<PRPayrollBase>(payment);

					List<PRWage> wages = PrepareWageData().ToList();

					var benefits = PrepareTaxableBenefitData(true);
					var refNbr = payrollAssemblyScope
									.Proxy
									.AddPayroll(
										payrollBase,
										wages,
										benefits.ToList());

					PopulateTaxSettingData(refNbr, payrollAssemblyScope);
				}

				return payrollAssemblyScope.Proxy.CalculatePayroll(LocationConstants.USCountryCode);
			}
		}

		#region Earnings / Wage

		protected virtual IEnumerable<string> GetWageTaxList(string typeCD, bool useReturnList)
		{
			if (useReturnList)
				EarningTaxList.WhereAnd<Where<PREarningTypeDetail.typecd, IsNotNull>>();
			else
				EarningTaxList.WhereAnd<Where<PREarningTypeDetail.typecd, IsNull>>();

			foreach (PRTaxCode taxCode in EarningTaxList.Select(typeCD))
			{
				yield return taxCode.TaxUniqueCode;
			}
		}

		#endregion

		#region Deductions / Benefit

		protected virtual PRCustomBenefit CreateCustomBenefit(int? codeID, string includeType)
		{
			var customBenefit = new PRCustomBenefit();
			customBenefit.PreTaxCalculationMethod = SubjectToTaxes.Get(includeType);

			if (customBenefit.PreTaxCalculationMethod == PRCustomItemCalculationMethod.FromList)
			{
				customBenefit.PreTaxUniqueTaxIDs = GetBenefitTaxList(codeID, includeType == SubjectToTaxes.AllButList).ToArray();
			}

			return customBenefit;
		}

		protected virtual IEnumerable<string> GetBenefitTaxList(int? codeID, bool useReturnList)
		{
			var deductionTaxList = new SelectFrom<PRTaxCode>
				.LeftJoin<PRDeductCodeDetail>.On<PRTaxCode.taxID.IsEqual<PRDeductCodeDetail.taxID>
					.And<PRDeductCodeDetail.codeID.IsEqual<P.AsInt>>>
				.Where<PRTaxCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>.View(this);

			if (useReturnList)
			{
				deductionTaxList.WhereAnd<Where<PRDeductCodeDetail.codeID.IsNotNull>>();
			}
			else
			{
				deductionTaxList.WhereAnd<Where<PRDeductCodeDetail.codeID.IsNull>>();
			}

			foreach (PRTaxCode taxCode in deductionTaxList.Select(codeID))
			{
				yield return taxCode.TaxUniqueCode;
			}
		}

		#endregion

		#region Tax Settings

		protected virtual void PopulateTaxSettingData(string referenceNbr, PXPayrollAssemblyScope<PayrollCalculationProxy> payrollAssemblyScope)
		{
			Dictionary<int?, List<ISettingDefinition>> taxSettingDictionary = CreateTaxSettingDictionary();
			foreach (PXResult<PREmployeeTax, PRTaxCode> taxEmployeeCompanySetting in TaxSettings.Select())
			{
				var employeeTaxSetting = (PREmployeeTax)taxEmployeeCompanySetting;
				var companyTaxSetting = (PRTaxCode)taxEmployeeCompanySetting;

				PRYtdTaxes ytdRecord = YTDTaxes.SelectSingle(employeeTaxSetting.TaxID, Payments.Current.TransactionDate.Value.Year);
				IEnumerable<PRPeriodTaxes> ytdPeriodTaxes = PeriodTaxes.Select(employeeTaxSetting.TaxID, Payments.Current.TransactionDate.Value.Year).Select(x => (PRPeriodTaxes)x);
				GetAggregateRecords(
					ytdPeriodTaxes.ToList(),
					out List<PRPeriodTaxes> periodTaxes,
					out List<PRPeriodTaxes> wtdTaxes,
					out List<PRPeriodTaxes> mtdTaxes,
					out List<PRPeriodTaxes> qtdTaxes,
					out List<PRPeriodTaxes> _);

				decimal periodAmount = periodTaxes.Sum(x => x?.Amount ?? 0);
				decimal wtdAmount = wtdTaxes.Sum(x => x?.Amount ?? 0);
				decimal mtdAmount = mtdTaxes.Sum(x => x.Amount ?? 0);
				decimal qtdAmount = qtdTaxes.Sum(result => result.Amount ?? 0);

				IEnumerable<PRYtdEarnings> companyYtdEarnings = new SelectFrom<PRYtdEarnings>
					.Where<PRYtdEarnings.year.IsEqual<P.AsString>>.View(this).Select(Payments.Current.TransactionDate.Value.Year.ToString()).FirstTableItems;
				int[] quarterMonths = PRDateTime.GetQuarterMonths(Payments.Current.TransactionDate.Value);
				decimal compagnyWagesQtd = companyYtdEarnings.Join(quarterMonths, result => result.Month, month => month, (result, month) => result).Sum(result => result.Amount ?? 0);
				decimal compagnyWagesYtd = companyYtdEarnings.Sum(x => x.Amount ?? 0);

				payrollAssemblyScope.Proxy.AddTaxSetting(
					referenceNbr,
					companyTaxSetting.TaxUniqueCode,
					periodAmount,
					wtdAmount,
					mtdAmount,
					qtdAmount,
					ytdRecord?.Amount ?? 0m,
					ytdRecord?.TaxableWages ?? 0m,
					ytdRecord?.MostRecentWH ?? 0m,
					compagnyWagesQtd,
					compagnyWagesYtd,
					companyTaxSetting,
					taxSettingDictionary[employeeTaxSetting.TaxID].ToDictionary(k => k.SettingName, v => v.Value));
			}

			SetEmployeeSettings(referenceNbr, payrollAssemblyScope);
		}

		protected virtual void SetEmployeeSettings(string referenceNbr, PXPayrollAssemblyScope<PayrollCalculationProxy> payrollAssemblyScope)
		{
			// Reflective mapping is by type/state, so group attributes by those two fields and create one reflective mapper for each grouping. 
			foreach (var attributeGroup in GetEmployeeSettings()
				.GroupBy(x => new { x.State, x.TypeName }))
			{
				payrollAssemblyScope.Proxy.AddEmployeeSetting(
					referenceNbr,
					new PREmployeeSettingMapper(attributeGroup.First()),
					attributeGroup.ToDictionary(k => k.SettingName, v => v.Value));
			}

			payrollAssemblyScope.Proxy.SetEmployeeResidenceLocationCode(
				referenceNbr,
				CurrentEmployeeResidenceAddress.SelectSingle()?.TaxLocationCode);
		}

		#endregion

		#endregion US tax calculation

		#region Canada tax calculation

		protected virtual IEnumerable<VertexPayrollCalculation> CalculateCanadaPayroll(IEnumerable<PRPayment> payments)
		{
			List<VertexPayroll> payrolls = new List<VertexPayroll>();
			foreach (PRPayment payment in payments)
			{
				Payments.Current = payment;
				VertexPayroll payroll = CreatePayrollBase<VertexPayroll>(payment);

				PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, payment) as PREmployee;
				Contact employeeContact = PXSelectorAttribute.Select<PREmployee.defContactID>(Caches[typeof(PREmployee)], employee) as Contact;
				Address residenceAddress = PXSelectorAttribute.Select<PREmployee.defAddressID>(Caches[typeof(PREmployee)], employee) as Address;
				PRLocation primaryWorkLocation;
				if (employee.LocationUseDflt == true)
				{
					primaryWorkLocation = new SelectFrom<PRLocation>
						.InnerJoin<PREmployeeClassWorkLocation>.On<PREmployeeClassWorkLocation.locationID.IsEqual<PRLocation.locationID>>
						.Where<PREmployeeClassWorkLocation.employeeClassID.IsEqual<P.AsString>
							.And<PREmployeeClassWorkLocation.isDefault.IsEqual<True>>>.View(this).SelectSingle(employee.EmployeeClassID);
				}
				else
				{
					primaryWorkLocation = new SelectFrom<PRLocation>
						.InnerJoin<PREmployeeWorkLocation>.On<PREmployeeWorkLocation.locationID.IsEqual<PRLocation.locationID>>
						.Where<PREmployeeWorkLocation.employeeID.IsEqual<P.AsInt>
							.And<PREmployeeWorkLocation.isDefault.IsEqual<True>>>.View(this).SelectSingle(employee.BAccountID);
				}

				Address primaryWorkAddress = PXSelectorAttribute.Select<PRLocation.addressID>(Caches[typeof(PRLocation)], primaryWorkLocation) as Address;

				payroll.EmployeeID = employee.AcctCD;
				payroll.EmployeeDateOfBirth = employeeContact.DateOfBirth.GetValueOrDefault();
				payroll.EmployeeResidence = residenceAddress != null ? new GeoCodeParser(residenceAddress.TaxLocationCode) : null;
				payroll.PrimaryWorkLocation = primaryWorkAddress != null ? new GeoCodeParser(primaryWorkAddress.TaxLocationCode) : null;
				payroll.Settings = CreateTaxSettingDictionary().Values.SelectMany(x => x)
					.Union(GetEmployeeSettings())
					.Select(x => new TaxSetting(x))
					.ToList();
				payroll.Wages = PrepareWageData().Select(x => (VertexWage)x).ToList();
				payroll.Deductions = PrepareTaxableBenefitData(true).Select(x => (VertexDeduction)x).ToList();
				payroll.TaxAggregates = PrepareTaxAggregateData().ToList();
				payroll.WageAggregates = PrepareWageAggregateData().ToList();
				payrolls.Add(payroll);
			}

			return new PRWebServiceRestClient().Calculate(LocationConstants.CanadaCountryCode, payrolls);
		}

		protected virtual IEnumerable<VertexTaxAggregate> PrepareTaxAggregateData()
		{
			foreach (PXResult<PREmployeeTax, PRTaxCode> result in TaxSettings.Select())
			{
				PREmployeeTax employeeTax = result;
				PRTaxCode taxCode = result;

				List<PRPeriodTaxes> ytdPeriodTaxes = PeriodTaxes.Select(employeeTax.TaxID, Payments.Current.TransactionDate.Value.Year)
					.Select(x => (PRPeriodTaxes)x)
					.ToList();
				GetAggregateRecords(
					ytdPeriodTaxes,
					out List<PRPeriodTaxes> periodTaxes,
					out List<PRPeriodTaxes> _,
					out List<PRPeriodTaxes> mtdTaxes,
					out List<PRPeriodTaxes> qtdTaxes,
					out List<PRPeriodTaxes> previousPeriodTaxes);

				VertexTaxAggregate taxAggregate = new VertexTaxAggregate()
				{
					UniqueTaxID = taxCode.TaxUniqueCode,
					AggregateAmounts = new Dictionary<Aggregate.AggregateType, AggregateTaxAmounts>()
				};

				taxAggregate.AggregateAmounts[Aggregate.AggregateType.PeriodToDate] = AggregateTaxAmounts(periodTaxes);
				taxAggregate.AggregateAmounts[Aggregate.AggregateType.MTD] = AggregateTaxAmounts(mtdTaxes);
				taxAggregate.AggregateAmounts[Aggregate.AggregateType.QTD] = AggregateTaxAmounts(qtdTaxes);
				taxAggregate.AggregateAmounts[Aggregate.AggregateType.YTD] = AggregateTaxAmounts(ytdPeriodTaxes);
				taxAggregate.AggregateAmounts[Aggregate.AggregateType.Previous] = AggregateTaxAmounts(previousPeriodTaxes);

				yield return taxAggregate;
			}
		}

		protected virtual AggregateTaxAmounts AggregateTaxAmounts(List<PRPeriodTaxes> recordsInAggregate)
		{
			return new AggregateTaxAmounts()
			{
				TaxAmount = recordsInAggregate.Sum(x => x.Amount ?? 0),
				AdjustedGrossAmount = recordsInAggregate.Sum(x => x.AdjustedGrossAmount ?? 0),
				ExemptionAmount = recordsInAggregate.Sum(x => x.ExemptionAmount ?? 0)
			};
		}

		protected virtual IEnumerable<VertexWageAggregate> PrepareWageAggregateData()
		{
			foreach (PXResult<PREmployeeTax, PRTaxCode> result in TaxSettings.Select())
			{
				PREmployeeTax employeeTax = result;
				PRTaxCode taxCode = result;

				foreach (IGrouping<(int? wageTypeID, bool? isSupplemental), PRPeriodTaxApplicableAmounts> resultGroup in PeriodTaxApplicableAmounts
					.Select(employeeTax.TaxID, Payments.Current.TransactionDate.Value.Year)
					.FirstTableItems
					.GroupBy(x => (x.WageTypeID, x.IsSupplemental)))
				{
					GetAggregateRecords(
						resultGroup.ToList(),
						out List<PRPeriodTaxApplicableAmounts> periodRecords,
						out List<PRPeriodTaxApplicableAmounts> _,
						out List<PRPeriodTaxApplicableAmounts> mtdRecords,
						out List<PRPeriodTaxApplicableAmounts> qtdRecords,
						out List<PRPeriodTaxApplicableAmounts> previousPeriodRecords);

					VertexWageAggregate wageAggregate = new VertexWageAggregate()
					{
						UniqueTaxID = taxCode.TaxUniqueCode,
						WageTypeID = resultGroup.Key.wageTypeID.GetValueOrDefault(),
						IsSupplemental = resultGroup.Key.isSupplemental.GetValueOrDefault(),
						AggregateAmounts = new Dictionary<Aggregate.AggregateType, decimal>()
					};

					wageAggregate.AggregateAmounts[Aggregate.AggregateType.PeriodToDate] = periodRecords.Sum(x => x.AmountAllowed.GetValueOrDefault());
					wageAggregate.AggregateAmounts[Aggregate.AggregateType.MTD] = mtdRecords.Sum(x => x.AmountAllowed.GetValueOrDefault());
					wageAggregate.AggregateAmounts[Aggregate.AggregateType.QTD] = qtdRecords.Sum(x => x.AmountAllowed.GetValueOrDefault());
					wageAggregate.AggregateAmounts[Aggregate.AggregateType.YTD] = resultGroup.Sum(x => x.AmountAllowed.GetValueOrDefault());
					wageAggregate.AggregateAmounts[Aggregate.AggregateType.Previous] = previousPeriodRecords.Sum(x => x.AmountAllowed.GetValueOrDefault());

					yield return wageAggregate;
				}
			}
		}

		#endregion Canada tax calculation

		#endregion Prepare Data

		#region Calculations

		protected virtual void SavePayrollCalculations(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;

				SaveTaxCalculations(payrollCalculation);
				SaveTaxableBenefitCalculations(payrollCalculation);

				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			this.Actions.PressSave();
		}

		protected virtual void SetDirectDepositSplit(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				PRPayment foundPayment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				PRPayment payment = SelectFrom<PRPayment>
									.Where<PRPayment.docType.IsEqual<P.AsString>
									.And<PRPayment.refNbr.IsEqual<P.AsString>>>
									.View.Select(this, foundPayment.DocType, foundPayment.RefNbr);

				SetDirectDepositSplit(payment);
				PaymentsToProcess.UpdatePayment(payment);
			}

			Actions.PressSave();
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				PRCABatch batch = SelectFrom<PRCABatch>.Where<PRCABatch.batchNbr.IsEqual<P.AsString>>.View.Select(this, payment.PaymentBatchNbr).TopFirst;
				PRCABatchUpdate.RecalculatePaymentBatchTotal(batch);
			}
		}

		public virtual void SetDirectDepositSplit(PRPayment payment)
		{
			Payments.Current = payment;
			PaymentBatch.Current = PaymentBatch.SelectSingle();
			DirectDepositSplits.Select().ForEach(x => DirectDepositSplits.Delete(x));
			PaymentBatchDetails.Select().ForEach(x => PaymentBatchDetails.Delete(x));
			if (payment.NetAmount > 0)
			{
				var paymentMethod = (PaymentMethod)PXSelectorAttribute.Select<PRPayment.paymentMethodID>(Payments.Cache, payment);
				var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();
				if (paymentMethodExt.PRPrintChecks == false)
				{
					PRDirectDepositSplit remainderRow = null;
					bool anyBankAccounts = false;
					decimal total = 0m;
					foreach (PREmployeeDirectDeposit employeeDDRow in SelectFrom<PREmployeeDirectDeposit>
						.Where<PREmployeeDirectDeposit.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>
						.OrderBy<Asc<PREmployeeDirectDeposit.sortOrder>>.View.Select(this))
					{
						anyBankAccounts = true;
						var split = new PRDirectDepositSplit();
						split.LineNbr = employeeDDRow.LineNbr;

						//US informations
						split.BankAcctNbr = employeeDDRow.BankAcctNbr;
						split.BankRoutingNbr = employeeDDRow.BankRoutingNbr;
						split.BankAcctType = employeeDDRow.BankAcctType;
						split.BankName = employeeDDRow.BankName;

						//CAN informations
						split.FinInstNbrCan = employeeDDRow.FinInstNbrCan;
						split.BankTransitNbrCan = employeeDDRow.BankTransitNbrCan;
						split.BankAcctNbrCan = employeeDDRow.BankAcctNbrCan;
						split.BeneficiaryName = employeeDDRow.BeneficiaryName;

						if (employeeDDRow.Amount != null)
						{
							split.Amount = employeeDDRow.Amount;
						}
						else if (employeeDDRow.Percent != null)
						{
							split.Amount = (employeeDDRow.Percent / 100) * payment.NetAmount;
						}

						if (total + split.Amount > payment.NetAmount)
						{
							split.Amount = payment.NetAmount - total;
						}

						split = DirectDepositSplits.Insert(split);
						total += split.Amount ?? 0m;

						if (employeeDDRow.GetsRemainder == true)
						{
							remainderRow = split;
						}

						if (PaymentBatch.Current != null)
						{
							var detail = new CABatchDetail();
							detail.OrigModule = BatchModule.PR;
							detail.OrigDocType = Payments.Current.DocType;
							detail.OrigRefNbr = Payments.Current.RefNbr;
							detail.OrigLineNbr = split.LineNbr;
							detail = PaymentBatchDetails.Insert(detail);
						}
					}

					if (!anyBankAccounts)
					{
						throw new PXException(Messages.NoBankAccountForDirectDeposit);
					}

					var remainingAmount = payment.NetAmount - total;
					if (remainingAmount > 0 && remainderRow != null)
					{
						remainderRow.Amount += remainingAmount;
						remainderRow = DirectDepositSplits.Update(remainderRow);
					}
				}
			}
		}

		public virtual decimal GetDirectDepositSum(PRPayment payment)
		{
			Payments.Current = payment;
			decimal directDepositSum = DirectDepositSplits.Select().FirstTableItems.Sum(x => x.Amount.GetValueOrDefault());

			return directDepositSum;
		}

		protected virtual void SaveTaxCalculations(PRPayrollCalculation payrollCalculation)
		{
			var taxSettings = TaxSettings.Select()
				.Select(x => (PXResult<PREmployeeTax, PRTaxCode>)x)
				.ToDictionary(k => ((PRTaxCode)k).TaxUniqueCode, v => (PRTaxCode)v);

			foreach (var taxCalculation in payrollCalculation.TaxCalculations)
			{
				CreatePaymentTax(taxCalculation, taxSettings);
			}

			// Re-insert taxes that were not calculated in current paycheck but have YTD
			HashSet<int?> taxesInPaycheck = PaymentTaxes.Select().FirstTableItems.Select(x => x.TaxID).ToHashSet();
			foreach (PRYtdTaxes ytdTax in EmployeeYTDTaxes.Select(Payments.Current.TransactionDate.Value.Year).FirstTableItems
				.Where(x => !taxesInPaycheck.Contains(x.TaxID)))
			{
				var summary = new PRPaymentTax();
				summary.TaxID = ytdTax.TaxID;
				PaymentTaxes.Insert(summary);
			}

			if (payrollCalculation is VertexPayrollCalculation vertexPayrollCalculation)
			{
				Dictionary<string, int?> taxUniqueIDs = SelectFrom<PRTaxCode>.View.Select(this).FirstTableItems.ToDictionary(k => k.TaxUniqueCode, v => v.TaxID);

				foreach (KeyValuePair<VertexTaxApplicableWageAmountKey, decimal> kvp in vertexPayrollCalculation.ApplicableWageAmounts)
				{
					if (taxUniqueIDs.TryGetValue(kvp.Key.UniqueTaxID, out int? taxID))
					{
						PRPaymentTaxApplicableAmounts applicableAmountRecord = new PRPaymentTaxApplicableAmounts()
						{
							DocType = Payments.Current.DocType,
							RefNbr = Payments.Current.RefNbr,
							TaxID = taxID,
							WageTypeID = kvp.Key.WageTypeID,
							IsSupplemental = kvp.Key.IsSupplemental,
							AmountAllowed = kvp.Value
						};
						PaymentTaxApplicableAmounts.Insert(applicableAmountRecord);
					}
				}
			}
		}

		protected virtual void CreatePaymentTax(
			PRTaxCalculation taxCalculation,
			Dictionary<string, PRTaxCode> taxSettings)
		{
			PRTaxCode taxSetting;
			if (taxSettings.TryGetValue(taxCalculation.TaxCode, out taxSetting))
			{
				PRPaymentTax paymentTax = new SelectFrom<PRPaymentTax>
					.Where<PRPaymentTax.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PRPaymentTax.taxID.IsEqual<P.AsInt>>>.View(this).SelectSingle(taxSetting.TaxID);
				if (paymentTax == null)
				{
					paymentTax = PaymentTaxes.Insert(new PRPaymentTax { TaxID = taxSetting.TaxID });
				}
				PaymentTaxes.Current = paymentTax;

				paymentTax.TaxAmount = taxCalculation.Amount;
				paymentTax.WageBaseGrossAmt = taxCalculation.GrossSubjectWageAmount;
				paymentTax.WageBaseHours = taxCalculation.SubjectHours;
				paymentTax.TaxCategory = taxSetting.TaxCategory;
				if (taxCalculation is VertexTaxCalculation vertexCalculation)
				{
					paymentTax.AdjustedGrossAmount = vertexCalculation.AdjustedGrossAmount;
					paymentTax.ExemptionAmount = vertexCalculation.ExemptionAmount;
				}

				PaymentTaxes.Update(paymentTax);

				Dictionary<int?, PRPaymentTaxSplit> paymentTaxSplits = SelectFrom<PRPaymentTaxSplit>
					.Where<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
						.And<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
						.And<PRPaymentTaxSplit.taxID.IsEqual<PRPaymentTax.taxID.FromCurrent>>>.View.Select(this).FirstTableItems.ToDictionary(k => k.WageType, v => v);
				UpdatePaymentTaxSplits(taxSetting.TaxID, taxCalculation.Details, paymentTaxSplits);
				_CalculationUtils.Value.CreateTaxDetail(this, taxSetting, paymentTax, PaymentEarningDetails.Select().FirstTableItems, out TaxEarningDetailsSplits splitTaxAmounts);

				PaymentsToProcess[Payments.Current].TaxesSplitByEarning[paymentTax.TaxID] = splitTaxAmounts;

				if (paymentTax.TaxCategory == TaxCategory.EmployeeWithholding && paymentTax.TaxAmount != 0)
				{
					PaymentsToProcess[Payments.Current].TaxAmount += paymentTax.TaxAmount.Value;
				}
			}
			else
			{
				throw new PXException(Messages.TaxCodeNotSetUp, taxCalculation.Description);
			}
		}

		protected virtual void UpdatePaymentTaxSplits(int? taxID, IEnumerable<PRTaxCalculationDetail> taxCalculationDetails, Dictionary<int?, PRPaymentTaxSplit> taxSplits)
		{
			foreach (IGrouping<int, PRTaxCalculationDetail> split in taxCalculationDetails.GroupBy(x => (int)x.WageType))
			{
				PRPaymentTaxSplit record;
				if (!taxSplits.TryGetValue(split.Key, out record))
				{
					record = PaymentTaxesSplit.Insert(new PRPaymentTaxSplit());
					record.WageType = split.Key;
					record.WageBaseAmount = 0m;
					record.TaxID = taxID;
				}

				record.WageBaseAmount = record.WageBaseAmount.GetValueOrDefault() + split.Sum(x => x.SubjectWageAmount);
				if (record.WageBaseAmount > 0)
				{
					PaymentTaxesSplit.Update(record);
					taxSplits[split.Key] = record;
				}
			}
		}

		protected virtual void SaveTaxableBenefitCalculations(PRPayrollCalculation payrollCalculation)
		{
			foreach (PRBenefitCalculation benefitCalculation in payrollCalculation.BenefitCalculations)
			{
				PaymentsToProcess[Payments.Current].NonWCDeductionAmount += benefitCalculation.CalculatedAmount;

				PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result = GetDeductions(null, benefitCalculation.CodeCD).FirstOrDefault();
				if (result != null)
				{
					PRPaymentDeduct paymentDeduct = result;
					if (paymentDeduct.IsActive == true)
					{
						paymentDeduct.DedAmount = benefitCalculation.CalculatedAmount;
						paymentDeduct.CntAmount = benefitCalculation.EmployerCalculatedAmount;

						ValidateBenefitsFromTaxEngine(paymentDeduct, result, result, benefitCalculation);
						Deductions.Update(result);

						List<PREarningDetail> paymentEarnings = PaymentEarningDetails.Select().FirstTableItems.ToList();
						PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning[paymentDeduct.CodeID] =
							_CalculationUtils.Value.SplitDedBenAmountsPerEarning(this, paymentDeduct, paymentEarnings);
					}
				}
			}
		}

		protected virtual void ValidateBenefitsFromTaxEngine(PRPaymentDeduct paymentDeduct, PRDeductCode deductCode, PREmployeeDeduct employeeDeduct, PRBenefitCalculation calculationResult)
		{
			PREmployee employee = SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View
				.SelectSingleBound(this, new object[] { Payments.Current });
			decimal minNetIncome = employee?.NetPayMin ?? 0m;
			if (PaymentsToProcess[Payments.Current].NetIncomeAccumulator < minNetIncome && Payments.Current.DocType != PayrollType.Adjustment)
			{
				_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionCausesNetPayBelowMin);
			}

			if (Math.Abs(PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID].DeductionAmount.GetValueOrDefault() - calculationResult.CalculatedAmount) > 0.01m)
			{
				_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
					paymentDeduct,
					paymentDeduct.DedAmount,
					Messages.DeductionAdjustedByTaxEngine);
			}

			bool hasEmployeeDedOverride = employeeDeduct != null && employeeDeduct.DedUseDflt == false;
			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductCode, employeeDeduct);
			if (dedMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
			{
				decimal? maximumAmount = hasEmployeeDedOverride ? employeeDeduct.DedMaxAmount : deductCode.DedMaxAmount;
				decimal? newYtd = paymentDeduct.YtdAmount.GetValueOrDefault() + calculationResult.CalculatedAmount;
				if (newYtd >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
						paymentDeduct,
						paymentDeduct.DedAmount,
						Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, newYtd > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
			else if (dedMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod)
			{
				decimal? maximumAmount = hasEmployeeDedOverride ? employeeDeduct.DedMaxAmount : deductCode.DedMaxAmount;
				if (calculationResult.CalculatedAmount >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
						paymentDeduct,
						paymentDeduct.DedAmount,
						Messages.DeductionMaxLimitExceededWarn, Messages.PerPayPeriod, calculationResult.CalculatedAmount > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}

			if (Math.Abs(PaymentsToProcess[Payments.Current].NominalTaxableDedBenAmounts[deductCode.CodeID].BenefitAmount.GetValueOrDefault() - calculationResult.EmployerCalculatedAmount) > 0.01m)
			{
				_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
					paymentDeduct,
					paymentDeduct.CntAmount,
					Messages.BenefitAdjustedByTaxEngine);
			}

			bool hasEmployeeCntOverride = employeeDeduct != null && employeeDeduct.CntUseDflt == false;
			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductCode, employeeDeduct);
			if (cntMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
			{
				decimal? maximumAmount = hasEmployeeCntOverride ? employeeDeduct.CntMaxAmount : deductCode.CntMaxAmount;
				decimal? newYtd = paymentDeduct.EmployerYtdAmount.GetValueOrDefault() + calculationResult.EmployerCalculatedAmount;
				if (newYtd >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
						paymentDeduct,
						paymentDeduct.CntAmount,
						Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, newYtd > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
			else if (cntMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod)
			{
				decimal? maximumAmount = hasEmployeeCntOverride ? employeeDeduct.CntMaxAmount : deductCode.CntMaxAmount;
				if (calculationResult.EmployerCalculatedAmount >= maximumAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
						paymentDeduct,
						paymentDeduct.CntAmount,
						Messages.BenefitMaxLimitExceededWarn, Messages.PerPayPeriod, calculationResult.EmployerCalculatedAmount > maximumAmount ? Messages.Exceeded : Messages.Reached);
				}
			}
		}

		protected virtual void CalculatePostTaxBenefits(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				CalculatePostTaxBenefitsForCurrent();
				CreateDeductionDetails();
				CreateBenefitDetails();
				if (Payments.Current.CountryID == LocationConstants.USCountryCode)
				{
					InsertProjectPackageDetails();
				}
				InsertUnionPackageDetails();
				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			this.Actions.PressSave();
		}

		protected virtual void CalculatePostTaxBenefitsForCurrent()
		{
			PREmployee employee = SelectFrom<PREmployee>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>.View
				.SelectSingleBound(this, new object[] { Payments.Current });
			decimal minNetIncome = employee?.NetPayMin ?? 0m;
			string deductionSplitMethod = employee?.DedSplitType;
			decimal maxPercentOfNetForGarnishments = employee?.GrnMaxPctNet ?? 100m;

			CalculateProjectUnionPostTaxBenefits(minNetIncome, deductionSplitMethod);
			CalculateWorkersCompensation(minNetIncome);

			decimal garnishmentTotal = 0m;
			SortedDictionary<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>> deductionSequences = SeparateDeductionsBySequence();

			// Calculate deductions by sequence, respecting minNetIncome and maxPercentOfNetForGarnishments
			var nominalAmounts = new Dictionary<int, DedBenAmount>();
			foreach (KeyValuePair<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>> sequence in deductionSequences)
			{
				// First pass to determine whether garnishments exceed limits
				decimal sequenceGarnishmentTotal = 0m;
				int garnishmentsInSequence = 0;
				foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result in sequence.Value)
				{
					PRDeductCode deductCode = result;
					PREmployeeDeduct employeeDeduct = result;
					PRPaymentDeduct paymentDeduct = result;

					DedBenAmount nominal = CalculateRegularBenefitAmount(deductCode, employeeDeduct, paymentDeduct);
					nominalAmounts[deductCode.CodeID.Value] = nominal;

					if (deductCode.IsGarnishment == true)
					{
						sequenceGarnishmentTotal += nominal.DeductionAmount.GetValueOrDefault();
						garnishmentsInSequence++;
					}
				}

				// Second pass to adjust garnishments for max percent of net pay
				decimal sequenceDeductionTotal = 0m;
				bool limitGarnishments = garnishmentTotal + sequenceGarnishmentTotal > PaymentsToProcess[Payments.Current].NetIncomeForGarnishmentCalc * maxPercentOfNetForGarnishments / 100;
				decimal garnishmentAmountAllowed = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeForGarnishmentCalc * maxPercentOfNetForGarnishments / 100 - garnishmentTotal, 0m);
				decimal garnishmentAmountRemaining = garnishmentAmountAllowed;
				foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result in sequence.Value
					.OrderBy(x => nominalAmounts[((PRDeductCode)x).CodeID.Value].DeductionAmount.GetValueOrDefault()))
				{
					PRDeductCode deductCode = result;
					PRPaymentDeduct paymentDeduct = result;

					decimal adjustedAmount = nominalAmounts[deductCode.CodeID.Value].DeductionAmount.GetValueOrDefault();
					if (deductCode.IsGarnishment == true)
					{
						if (limitGarnishments && adjustedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
						{
							adjustedAmount = AdjustBenefitAmountForSequenceSplit(
								adjustedAmount,
								garnishmentAmountAllowed,
								garnishmentAmountRemaining,
								sequenceGarnishmentTotal,
								garnishmentsInSequence,
								paymentDeduct,
								deductionSplitMethod,
								Messages.GarnishmentCausesPercentOfNetAboveMax,
								Messages.GarnishmentAdjustedForPercentOfNetMax,
								true);

							garnishmentAmountRemaining -= adjustedAmount;
						}
						garnishmentsInSequence--;
					}

					nominalAmounts[deductCode.CodeID.Value].DeductionAmount = adjustedAmount;
					sequenceDeductionTotal += adjustedAmount;
				}

				// Third pass to adjust amounts for net pay minimum and record adjusted amounts
				bool limitDeductions = PaymentsToProcess[Payments.Current].NetIncomeAccumulator - sequenceDeductionTotal <= minNetIncome;
				decimal deductionAmountAllowed = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - minNetIncome, 0m);
				decimal deductionAmountRemaining = deductionAmountAllowed;
				int deductionsInSequence = sequence.Value.Count;
				foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result in sequence.Value
					.OrderBy(x => nominalAmounts[((PRDeductCode)x).CodeID.Value].DeductionAmount.GetValueOrDefault()))
				{
					PRDeductCode deductCode = result;
					PRPaymentDeduct paymentDeduct = result;

					decimal adjustedAmount = nominalAmounts[deductCode.CodeID.Value].DeductionAmount.GetValueOrDefault();
					if (limitDeductions && adjustedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
					{
						adjustedAmount = AdjustBenefitAmountForSequenceSplit(
							adjustedAmount,
							deductionAmountAllowed,
							deductionAmountRemaining,
							sequenceDeductionTotal,
							deductionsInSequence,
							paymentDeduct,
							deductionSplitMethod,
							Messages.DeductionCausesNetPayBelowMin,
							Messages.DeductionAdjustedForNetPayMin,
							false);

						deductionAmountRemaining -= adjustedAmount;
					}

					deductionsInSequence--;
					paymentDeduct.DedAmount = adjustedAmount;
					PaymentsToProcess[Payments.Current].NonWCDeductionAmount += adjustedAmount;
					if (deductCode.IsGarnishment == true)
					{
						garnishmentTotal += adjustedAmount;
					}

					paymentDeduct.CntAmount = nominalAmounts[deductCode.CodeID.Value].BenefitAmount;
				}
			}

			// All calculations are done, perform update.
			foreach (var sequence in deductionSequences)
			{
				foreach (PRPaymentDeduct deduction in sequence.Value)
				{
					Deductions.Update(deduction);
				}
			}
		}

		protected virtual void CalculateProjectUnionPostTaxBenefits(decimal minNetIncome, string deductionSplitMethod)
		{
			IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> dedResults = GetDeductions(false)
				.Where(x => ((PRPaymentDeduct)x).Source != PaymentDeductionSourceAttribute.EmployeeSettings);
			dedResults.ForEach(x => MarkDeductionAsModified(x));
			decimal deductionTotal = 0m;

			// First pass to determine whether deductions exceed net pay minimum limit
			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> result in dedResults)
			{
				var paymentDeduct = (PRPaymentDeduct)result;
				var deductCode = (PRDeductCode)result;
				var employeeDeduct = (PREmployeeDeduct)result;
				DedBenAmount nominalAmounts = CalculateRegularBenefitAmount(
						deductCode,
						employeeDeduct,
						paymentDeduct);
				paymentDeduct.DedAmount = nominalAmounts.DeductionAmount;
				paymentDeduct.CntAmount = nominalAmounts.BenefitAmount;

				deductionTotal += paymentDeduct.DedAmount ?? 0m;
			}

			// Second pass to adjust deduction amounts for net pay minimum and record amounts
			bool limitDeductions = PaymentsToProcess[Payments.Current].NetIncomeAccumulator - deductionTotal <= minNetIncome;
			decimal deductionAmountAllowed = Math.Max(PaymentsToProcess[Payments.Current].NetIncomeAccumulator - minNetIncome, 0m);
			decimal deductionAmountRemaining = deductionAmountAllowed;
			var amountsPerCode = new Dictionary<int, (decimal deductionAmount, decimal contributionAmount)>();
			int numberOfDeductions = dedResults.Count();
			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> dedResult in dedResults.OrderBy(x => ((PRPaymentDeduct)x).DedAmount.GetValueOrDefault()))
			{
				PRDeductCode deductCode = (PRDeductCode)dedResult;
				PRPaymentDeduct paymentDeduct = (PRPaymentDeduct)dedResult;

				if (limitDeductions && paymentDeduct.DedAmount > 0 && Payments.Current.DocType != PayrollType.Adjustment)
				{
					paymentDeduct.DedAmount = AdjustBenefitAmountForSequenceSplit(
						paymentDeduct.DedAmount.Value,
						deductionAmountAllowed,
						deductionAmountRemaining,
						deductionTotal,
						numberOfDeductions,
						paymentDeduct,
						deductionSplitMethod,
						Messages.DeductionCausesNetPayBelowMin,
						Messages.DeductionAdjustedForNetPayMin,
						true);

					deductionAmountRemaining -= paymentDeduct.DedAmount.GetValueOrDefault();
				}

				numberOfDeductions--;
				PaymentsToProcess[Payments.Current].NonWCDeductionAmount += paymentDeduct.DedAmount ?? 0m;
				Deductions.Update(paymentDeduct);
			}
		}

		protected virtual DedBenAmount CalculateRegularBenefitAmount(
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{

			DedBenAmount calculatedAmounts = CalculateRegularBenefitNominalAmount(deductCode, employeeDeduct, paymentDeduct);
			return AdjustSingleBenefitAmount(calculatedAmounts, deductCode, employeeDeduct, paymentDeduct);
		}

		protected virtual DedBenAmount CalculateRegularBenefitNominalAmount(
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{
			decimal deductionCalculatedAmount = 0m;
			decimal contributionCalculatedAmount = 0m;
			if (paymentDeduct.SaveOverride == true)
			{
				deductionCalculatedAmount = paymentDeduct.DedAmount ?? 0m;
				contributionCalculatedAmount = paymentDeduct.CntAmount ?? 0m;
			}
			else if (paymentDeduct.Source == PaymentDeductionSourceAttribute.EmployeeSettings)
			{
				if (deductCode.ContribType == ContributionType.BothDeductionAndContribution || deductCode.ContribType == ContributionType.EmployeeDeduction)
				{
					decimal? deductionAmount = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
								employeeDeduct.DedAmount :
								deductCode.DedAmount;
					decimal? deductionPercent = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
						employeeDeduct.DedPercent :
						deductCode.DedPercent;
					switch (deductCode.DedCalcType)
					{
						case DedCntCalculationMethod.FixedAmount:
							if (deductionAmount == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, Messages.FixedAmount);
							}

							deductionCalculatedAmount = (decimal)deductionAmount;
							break;
						case DedCntCalculationMethod.AmountPerHour:
							if (deductionAmount == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, Messages.AmountPerHour);
							}

							deductionCalculatedAmount = (decimal)deductionAmount * GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction);
							break;
						case DedCntCalculationMethod.PercentOfGross:
						case DedCntCalculationMethod.PercentOfCustom:
						case DedCntCalculationMethod.PercentOfNet:
							if (deductionPercent == null)
							{
								throw new PXException(Messages.DedCalculationErrorFormat, new DedCntCalculationMethod.ListAttribute().ValueLabelDic[deductCode.DedCalcType]);
							}

							deductionCalculatedAmount = (decimal)deductionPercent * GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction) / 100;
							break;
					}
				}

				if (deductCode.ContribType == ContributionType.BothDeductionAndContribution || deductCode.ContribType == ContributionType.EmployerContribution)
				{
					decimal? contributionAmount = employeeDeduct != null && employeeDeduct.CntUseDflt == false ?
								employeeDeduct.CntAmount :
								deductCode.CntAmount;
					decimal? contributionPercent = employeeDeduct != null && employeeDeduct.CntUseDflt == false ?
						employeeDeduct.CntPercent :
						deductCode.CntPercent;
					switch (deductCode.CntCalcType)
					{
						case DedCntCalculationMethod.FixedAmount:
							if (contributionAmount == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, Messages.FixedAmount);
							}

							contributionCalculatedAmount = (decimal)contributionAmount;
							break;
						case DedCntCalculationMethod.AmountPerHour:
							if (contributionAmount == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, Messages.AmountPerHour);
							}

							contributionCalculatedAmount = (decimal)contributionAmount * GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution);
							break;
						case DedCntCalculationMethod.PercentOfGross:
						case DedCntCalculationMethod.PercentOfCustom:
						case DedCntCalculationMethod.PercentOfNet:
							if (contributionPercent == null)
							{
								throw new PXException(Messages.CntCalculationErrorFormat, new DedCntCalculationMethod.ListAttribute().ValueLabelDic[deductCode.CntCalcType]);
							}

							contributionCalculatedAmount = (decimal)contributionPercent * GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution) / 100;
							break;
					}
				}
			}
			else
			{
				switch (paymentDeduct.Source)
				{
					case PaymentDeductionSourceAttribute.CertifiedProject:
						HashSet<(int?, int?, int?)> projectPackageDeductionsApplied = new HashSet<(int?, int?, int?)>();
						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>> resultGroup in ProjectDeductions.Select(deductCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitProjectPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitProjectPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitProjectPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType, deductCode, this);
							switch (deductCode.DedCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (!projectPackageDeductionsApplied.Contains((deductCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.DeductionAmount = package.DeductionAmount.GetValueOrDefault();
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									calculation.DeductionAmount = (package.DeductionRate * calculation.TotalAmountForDed / 100).GetValueOrDefault();
									break;
								case DedCntCalculationMethod.AmountPerHour:
									calculation.DeductionAmount = (package.DeductionAmount * calculation.TotalHoursForDed).GetValueOrDefault();
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							switch (deductCode.CntCalcType)
							{
								case DedCntCalculationMethod.FixedAmount:
									if (!projectPackageDeductionsApplied.Contains((deductCode.CodeID, package.ProjectID, package.LaborItemID)))
									{
										calculation.BenefitAmount = package.BenefitAmount.GetValueOrDefault();
									}
									break;
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									calculation.BenefitAmount = (package.BenefitRate * calculation.TotalAmountForBen / 100).GetValueOrDefault();
									break;
								case DedCntCalculationMethod.AmountPerHour:
									calculation.BenefitAmount = (package.BenefitAmount * calculation.TotalHoursForBen).GetValueOrDefault();
									break;
								default:
									throw new PXException(Messages.PercentOfNetInCertifiedProject);
							}

							RecordProjectPackageNominalAmounts(package, calculation);
							deductionCalculatedAmount += calculation.DeductionAmount.GetValueOrDefault();
							contributionCalculatedAmount += calculation.BenefitAmount.GetValueOrDefault();
							projectPackageDeductionsApplied.Add((deductCode.CodeID, package.ProjectID, package.LaborItemID));
						}
						break;
					case PaymentDeductionSourceAttribute.Union:
						HashSet<(int?, string, int?)> unionPackageDeductionsApplied = new HashSet<(int?, string, int?)>();
						foreach (IGrouping<int?, PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>> resultGroup in UnionDeductions.Select(deductCode.CodeID)
							.Select(x => (PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType>)x)
							.GroupBy(x => ((PREarningDetail)x).RecordID))
						{
							PXResult<PREarningDetail, PRDeductionAndBenefitUnionPackage, PRDeductCode, EPEarningType> result = resultGroup.OrderByDescending(x => ((PRDeductionAndBenefitUnionPackage)x).EffectiveDate).First();
							PREarningDetail earning = result;
							PRDeductionAndBenefitUnionPackage package = result;
							EPEarningType earningType = result;

							PackageDedBenCalculation calculation = new PackageDedBenCalculation(earning, earningType, deductCode, this);
							if (deductCode.ContribType != ContributionType.EmployerContribution)
							{
								switch (deductCode.DedCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (!unionPackageDeductionsApplied.Contains((deductCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.DeductionAmount = package.DeductionAmount.GetValueOrDefault();
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										calculation.DeductionAmount = (package.DeductionRate * calculation.TotalAmountForDed / 100).GetValueOrDefault();
										break;
									case DedCntCalculationMethod.AmountPerHour:
										calculation.DeductionAmount = (package.DeductionAmount * calculation.TotalHoursForDed).GetValueOrDefault();

										if (deductCode.DedApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarningsWithOTMult
											|| deductCode.DedApplicableEarnings == DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithOTMult && earningType.IsOvertime == true)
										{
											calculation.DeductionAmount *= earningType.OvertimeMultiplier.GetValueOrDefault();
										}

										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							if (deductCode.ContribType != ContributionType.EmployeeDeduction)
							{
								switch (deductCode.CntCalcType)
								{
									case DedCntCalculationMethod.FixedAmount:
										if (!unionPackageDeductionsApplied.Contains((deductCode.CodeID, package.UnionID, package.LaborItemID)))
										{
											calculation.BenefitAmount = package.BenefitAmount.GetValueOrDefault();
										}
										break;
									case DedCntCalculationMethod.PercentOfGross:
									case DedCntCalculationMethod.PercentOfCustom:
										calculation.BenefitAmount = (package.BenefitRate * calculation.TotalAmountForBen / 100).GetValueOrDefault();
										break;
									case DedCntCalculationMethod.AmountPerHour:
										calculation.BenefitAmount = (package.BenefitAmount * calculation.TotalHoursForBen).GetValueOrDefault();

										if (deductCode.CntApplicableEarnings == DedBenApplicableEarningsAttribute.TotalEarningsWithOTMult
											|| deductCode.CntApplicableEarnings == DedBenApplicableEarningsAttribute.RegularAndOTEarningsWithOTMult && earningType.IsOvertime == true)
										{
											calculation.BenefitAmount *= earningType.OvertimeMultiplier.GetValueOrDefault();
										}

										break;
									default:
										throw new PXException(Messages.PercentOfNetInUnion);
								}
							}

							RecordUnionPackageNominalAmounts(package, calculation);
							deductionCalculatedAmount += calculation.DeductionAmount.GetValueOrDefault();
							contributionCalculatedAmount += calculation.BenefitAmount.GetValueOrDefault();
							unionPackageDeductionsApplied.Add((deductCode.CodeID, package.UnionID, package.LaborItemID));
						}
						break;
				}
			}

			return new DedBenAmount
			{
				DeductionAmount = Math.Max(deductionCalculatedAmount, 0),
				BenefitAmount = Math.Max(contributionCalculatedAmount, 0)
			};
		}

		protected virtual DedBenAmount AdjustSingleBenefitAmount(
			DedBenAmount calculatedAmounts,
			PRDeductCode deductCode,
			PREmployeeDeduct employeeDeduct,
			PRPaymentDeduct paymentDeduct)
		{
			// Lazy load
			PRYtdDeductions ytdTally = null;

			string dedMaximumFrequency = GetDedMaxFreqTypeValue(deductCode, employeeDeduct);
			if (dedMaximumFrequency != DeductionMaxFrequencyType.NoMaximum)
			{
				decimal? maxAmount = employeeDeduct != null && employeeDeduct.DedUseDflt == false ?
					employeeDeduct.DedMaxAmount :
					deductCode.DedMaxAmount;
				if (maxAmount != null)
				{
					if (dedMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod && calculatedAmounts.DeductionAmount > maxAmount)
					{
						if (paymentDeduct.SaveOverride == true)
						{
							// Leave deduction amount as is but generate a warning that period maximum has been exceeded
							_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
								paymentDeduct,
								paymentDeduct.DedAmount,
								Messages.DeductionMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Exceeded);
						}
						else
						{
							// Adjust deduction amount to respect maximum and generate a warning
							calculatedAmounts.DeductionAmount = Math.Max(maxAmount.Value, 0m);
							_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
								paymentDeduct,
								paymentDeduct.DedAmount,
								Messages.DeductionMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Reached);
						}
					}
					else if (dedMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
					{
						decimal ytdAmount = 0m;
						ytdTally = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
						if (ytdTally != null)
						{
							ytdAmount = ytdTally.Amount ?? 0m;
						}

						if (calculatedAmounts.DeductionAmount + ytdAmount > maxAmount)
						{
							if (paymentDeduct.SaveOverride == true)
							{
								// Leave deduction amount as is but generate a warning that annual maximum has been exceeded
								_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Exceeded);
							}
							else
							{
								// Adjust deduction amount to respect maximum and generate a warning
								calculatedAmounts.DeductionAmount = Math.Max(maxAmount.Value - ytdAmount, 0m);
								_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
									paymentDeduct,
									paymentDeduct.DedAmount,
									Messages.DeductionMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Reached);
							}
						}
					}
				}
			}

			if (deductCode.IsGarnishment == true && employeeDeduct?.GarnOrigAmount > 0 &&
				calculatedAmounts.DeductionAmount > employeeDeduct.GarnOrigAmount - (employeeDeduct.GarnPaidAmount ?? 0m))
			{
				calculatedAmounts.DeductionAmount = Math.Max(employeeDeduct.GarnOrigAmount.Value - (employeeDeduct.GarnPaidAmount ?? 0m), 0m);
			}

			string cntMaximumFrequency = GetCntMaxFreqTypeValue(deductCode, employeeDeduct);
			if (cntMaximumFrequency != DeductionMaxFrequencyType.NoMaximum)
			{
				decimal? maxAmount = employeeDeduct != null && employeeDeduct.CntUseDflt != null && !employeeDeduct.CntUseDflt.Value ?
					employeeDeduct.CntMaxAmount :
					deductCode.CntMaxAmount;
				if (maxAmount != null)
				{
					if (cntMaximumFrequency == DeductionMaxFrequencyType.PerPayPeriod && calculatedAmounts.BenefitAmount > maxAmount)
					{
						if (paymentDeduct.SaveOverride == true)
						{
							// Leave contribution amount as is but generate a warning that period maximum has been exceeded
							_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
								paymentDeduct,
								paymentDeduct.CntAmount,
								Messages.BenefitMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Exceeded);
						}
						else
						{
							// Adjust contribution amount to respect maximum and generate a warning
							calculatedAmounts.BenefitAmount = Math.Max(maxAmount.Value, 0m);
							_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
								paymentDeduct,
								paymentDeduct.CntAmount,
								Messages.BenefitMaxLimitExceededWarn, Messages.PerPayPeriod, Messages.Reached);
						}
					}
					else if (cntMaximumFrequency == DeductionMaxFrequencyType.PerCalendarYear)
					{
						decimal ytdAmount = 0m;
						if (ytdTally == null)
						{
							ytdTally = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
						}

						if (ytdTally != null)
						{
							ytdAmount = ytdTally.EmployerAmount ?? 0m;
						}

						if (calculatedAmounts.BenefitAmount + ytdAmount > maxAmount)
						{
							if (paymentDeduct.SaveOverride == true)
							{
								// Leave contribution amount as is but generate a warning that annual maximum has been exceeded
								_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
									paymentDeduct,
									paymentDeduct.CntAmount,
									Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Exceeded);
							}
							else
							{
								// Adjust contribution amount to respect maximum and generate a warning
								calculatedAmounts.BenefitAmount = Math.Max(maxAmount.Value - ytdAmount, 0m);
								_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
									paymentDeduct,
									paymentDeduct.CntAmount,
									Messages.BenefitMaxLimitExceededWarn, Messages.PerCalendarYear, Messages.Reached);
							}
						}
					}
				}
			}

			return calculatedAmounts;
		}

		protected virtual SortedDictionary<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>> SeparateDeductionsBySequence()
		{
			var deductionSequences = new SortedDictionary<int, List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>>();

			foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> deduction in GetDeductions(false))
			{
				PRPaymentDeduct paymentDeduct = deduction;
				if (paymentDeduct.Source == PaymentDeductionSourceAttribute.EmployeeSettings)
				{
					var employeeDeduct = (PREmployeeDeduct)deduction;
					int sequenceNumber = employeeDeduct != null && employeeDeduct.Sequence != null ?
						(int)employeeDeduct.Sequence :
						int.MaxValue;
					if (!deductionSequences.ContainsKey(sequenceNumber))
					{
						deductionSequences[sequenceNumber] = new List<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>>();
					}

					deductionSequences[sequenceNumber].Add(deduction);

					MarkDeductionAsModified(deduction);
				}
			}

			return deductionSequences;
		}

		protected virtual void MarkDeductionAsModified(PRPaymentDeduct row)
		{
			// Since PRPaymentDeduct are updated in a different loop than they are Selected, if their status
			// in the cache is Notchanged, PXCache can try to reload its internal Originals collection with the updated
			// record value. This would lead to OldRow and NewRow being equal when the records are updated in the cache,
			// which negatively impacts the RowUpdated events. Since the PRPaymentDeduct are going to be modified in the
			// calculation process, we mark them as Modified here to make sure their entry in the Originals collection
			// in not overwritten.
			if (Deductions.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
			{
				Deductions.Cache.SetStatus(row, PXEntryStatus.Modified);
			}
		}

		protected virtual decimal AdjustBenefitAmountForSequenceSplit(
			decimal initialAmount,
			decimal amountAllowedInSequence,
			decimal amountRemainingInSequence,
			decimal sequenceTotal,
			int numberInSequence,
			PRPaymentDeduct deduction,
			string splitMethod,
			string errorMessage,
			string warningMessage,
			bool allowAdjustGarnishment = true)
		{
			decimal adjustedAmount = 0m;
			if (deduction.SaveOverride == true || (deduction.IsGarnishment == true && !allowAdjustGarnishment))
			{
				// Overridden deduction amount can't be adjusted => throw error
				// Garnishment causes net pay to ge below minimum => throw error
				deduction.DedAmount = initialAmount;
				_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
					deduction,
					deduction.DedAmount,
					errorMessage);
			}
			else
			{
				// Adjust deduction amount so that Minimum net pay is respected and generate a warning
				if (splitMethod == DeductionSplitType.Even && numberInSequence != 0)
				{
					adjustedAmount = Math.Min(amountRemainingInSequence / numberInSequence, initialAmount);
				}
				else if (sequenceTotal != 0)
				{
					adjustedAmount = Math.Min(initialAmount / sequenceTotal * amountAllowedInSequence, amountRemainingInSequence);
				}

				adjustedAmount = Math.Round(adjustedAmount, 2, MidpointRounding.AwayFromZero);

				if (adjustedAmount != initialAmount)
				{
					_CalculationErrors.AddWarning<PRPaymentDeduct.dedAmount>(
						deduction,
						adjustedAmount,
						warningMessage);
				}
			}

			return adjustedAmount;
		}

		protected virtual void CreateDeductionDetails()
		{
			foreach (PRPaymentDeduct deduction in GetDeductions().Select(x => (PRPaymentDeduct)x))
			{
				_CalculationUtils.Value.CreateDeductionDetail(this, DeductionDetails.Cache, deduction, PaymentEarningDetails.Select().FirstTableItems.ToList());
			}
		}

		protected virtual void CreateBenefitDetails()
		{
			foreach (PRPaymentDeduct deduction in GetDeductions().Select(x => (PRPaymentDeduct)x))
			{
				_CalculationUtils.Value.CreateBenefitDetail(this, BenefitDetails.Cache, deduction, PaymentEarningDetails.Select().FirstTableItems.ToList());
			}
		}

		protected virtual void CalculateFringeBenefitRates(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (PRPayrollCalculation payrollCalculation in payrollCalculations)
			{
				PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				Payments.Current = payment;
				if (payment.CountryID != LocationConstants.USCountryCode)
				{
					continue;
				}

				PaymentsToProcess[Payments.Current].FringeRateReducingBenefits = new Dictionary<FringeBenefitDecreasingRateKey, PRPaymentFringeBenefitDecreasingRate>();
				PaymentsToProcess[Payments.Current].FringeRateReducingEarnings = new Dictionary<FringeEarningDecreasingRateKey, PRPaymentFringeEarningDecreasingRate>();

				List<FringeSourceEarning> fringeRates = new List<FringeSourceEarning>();
				foreach (IGrouping<int?, PXResult<PREarningDetail, PRProjectFringeBenefitRate, PMProject, EPEarningType>> result in FringeBenefitApplicableEarnings.Select()
					.Select(x => (PXResult<PREarningDetail, PRProjectFringeBenefitRate, PMProject, EPEarningType>)x)
					.Where(x => ((PREarningDetail)x).CertifiedJob == true)
						// It's important to keep ordering in the GroupBy result. PX.Data.SQL's implementation of Enumerable.GroupBy doesn't preserve order,
						// while System.Collections.Generic's implementation does preserve order.
						// Force immediate query evaluation by calling ToList so that System.Collections.Generic's GroupBy is called.
						.ToList()
						.GroupBy(x => ((PREarningDetail)x).RecordID))
				{
					PREarningDetail earning = result.First();
					PRProjectFringeBenefitRate fringeBenefit = result.First();
					PMProject project = result.First();
					PMProjectExtension projectExt = project.GetExtension<PMProjectExtension>();
					EPEarningType earningType = result.First();

					if (earning.Hours > 0)
					{
						decimal earningTypeOTMultiplier = earningType.OvertimeMultiplier.GetValueOrDefault(1);
						bool applyOTMultiplierToFringeRate = projectExt.ApplyOTMultiplierToFringeRate == true;
						decimal overtimeMultiplier = GetOvertimeMultiplier(earning, applyOTMultiplierToFringeRate, earningTypeOTMultiplier);

						decimal fringeRate = fringeBenefit.Rate.GetValueOrDefault() * overtimeMultiplier - GetFringeReducingRate(earning, project);
						fringeRates.Add(new FringeSourceEarning(earning, project, fringeRate, fringeBenefit.Rate.GetValueOrDefault(), overtimeMultiplier));
					}
				}

				PaymentsToProcess[payment].FringeAmountsPerProject = SplitFringeRates(fringeRates);
				PaymentsToProcess[payment].FringeRates = fringeRates;

				PaymentsToProcess[Payments.Current].FringeRateReducingBenefits.Values.ForEach(x => PaymentFringeBenefitsDecreasingRate.Insert(x));
				PaymentsToProcess[Payments.Current].FringeRateReducingEarnings.Values.ForEach(x => PaymentFringeEarningsDecreasingRate.Insert(x));

				PaymentsToProcess.UpdatePayment(Payments.Current);
			}

			Actions.PressSave();
			AdjustFringePayoutBenefits();
			ApplyFringeRates();
			Actions.PressSave();
			CalculateTaxesOnFringeEarnings(payrollCalculations);
			Actions.PressSave();
		}

		/// <summary>
		/// Extracted public virtual method to allow for customization projects to calculate the OT multiplier
		/// </summary>
		public virtual decimal GetOvertimeMultiplier(PREarningDetail earningDetail, bool applyOTMultiplierToFringeRate, decimal earningTypeOvertimeMultiplier)
		{
			decimal overtimeMultiplier = 1;
			if (applyOTMultiplierToFringeRate == true)
			{
				overtimeMultiplier = earningTypeOvertimeMultiplier;
			}

			return overtimeMultiplier;
		}

		protected virtual decimal GetFringeReducingRate(PREarningDetail earning, PMProject project)
		{
			decimal reducingRate = GetFringeReducingRateFromBenefits(earning, project.ContractID)
				+ GetFringeReducingRateFromExcessWage(earning, project);

			// reducingRate is used to calculate PRPaymentFringeBenefit.CalculatedFringeRate, which is PXDecimal with default precision (2)
			return Math.Round(reducingRate, 2, MidpointRounding.AwayFromZero);
		}

		protected virtual decimal GetFringeReducingRateFromBenefits(PREarningDetail earning, int? projectID)
		{
			decimal reducingRate = 0m;
			decimal allProjectHours = AllProjectHours.SelectSingle().Hours ?? 0m;
			var paymentDeducts = GetDeductions().ToArray();
			var paymentUnionPackageDeducts = UnionPackageDeductions.Select().FirstTableItems.ToArray();

			foreach (PRProjectFringeBenefitRateReducingDeduct reducingDeduct in FringeBenefitRateReducingDeductions.Select(projectID))
			{
				decimal applicableHours = GetFringeApplicableHours(reducingDeduct.AnnualizationException == true);
				if (applicableHours == 0)
				{
					continue;
				}

				if (PayrollPreferences.Current.UseBenefitRateFromUnionInCertProject == true && earning.UnionID != null)
				{
					bool commonDeductCodeInUnionAndProject = SelectFrom<PRDeductionAndBenefitUnionPackage>
						.Where<PRDeductionAndBenefitUnionPackage.unionID.IsEqual<P.AsString>
						.And<PRDeductionAndBenefitUnionPackage.deductionAndBenefitCodeID.IsEqual<P.AsInt>>>.View
						.Select(this, earning.UnionID, reducingDeduct.DeductCodeID).Any();

					if (!commonDeductCodeInUnionAndProject)
					{
						continue;
					}
				}

				PRDeductCode deductCode = paymentDeducts.FirstOrDefault(x => ((PRPaymentDeduct)x).CodeID == reducingDeduct.DeductCodeID);
				decimal benefitAmount = 0;

				if (PayrollPreferences.Current.UseBenefitRateFromUnionInCertProject == true && deductCode?.IsUnion == true)
				{
					PRPaymentUnionPackageDeduct paymentUnionPackageDeduct = paymentUnionPackageDeducts.FirstOrDefault(x => x.DeductCodeID == reducingDeduct.DeductCodeID && x.LaborItemID == earning.LabourItemID);

					//if we don't find a specific union row with same deduction code and labor item, take the union row that has the same deduction code 
					if (paymentUnionPackageDeduct == null)
					{
						paymentUnionPackageDeduct = paymentUnionPackageDeducts.FirstOrDefault(x => x.DeductCodeID == reducingDeduct.DeductCodeID && x.LaborItemID == null);
					}

					benefitAmount = paymentUnionPackageDeduct?.BenefitAmount ?? 0;
					applicableHours = paymentUnionPackageDeduct?.WageBaseHours ?? 0;
				}
				else
				{
					benefitAmount = ((PRPaymentDeduct)paymentDeducts.FirstOrDefault(x => ((PRPaymentDeduct)x).CodeID == reducingDeduct.DeductCodeID))?.CntAmount ?? 0;
				}

				if (benefitAmount == 0)
				{
					continue;
				}
				reducingRate += benefitAmount / applicableHours;

				FringeBenefitDecreasingRateKey key = new FringeBenefitDecreasingRateKey(earning.ProjectID, reducingDeduct.DeductCodeID, earning.LabourItemID, earning.ProjectTaskID);
				if (!PaymentsToProcess[Payments.Current].FringeRateReducingBenefits.ContainsKey(key))
				{
					PaymentsToProcess[Payments.Current].FringeRateReducingBenefits[key] = new PRPaymentFringeBenefitDecreasingRate()
					{
						ProjectID = earning.ProjectID,
						DeductCodeID = reducingDeduct.DeductCodeID,
						LaborItemID = earning.LabourItemID,
						ProjectTaskID = earning.ProjectTaskID,
						ApplicableHours = reducingDeduct.AnnualizationException == true && allProjectHours != 0 ? allProjectHours : applicableHours,
						Amount = benefitAmount
					};
				}
			}

			return reducingRate;
		}

		protected virtual decimal GetFringeReducingRateFromExcessWage(PREarningDetail earning, PMProject project)
		{
			Dictionary<string, WagesAbovePrevailing> excessWages = new Dictionary<string, WagesAbovePrevailing>();
			foreach (PXResult<PREarningDetail, PRLocation, Address, EPEarningType> result in PaymentEarningDetails.Select().ToList()
				.Select(x => (PXResult<PREarningDetail, PRLocation, Address, EPEarningType>)x)
				.Where(x => ((PREarningDetail)x).IsFringeRateEarning != true &&
					((PREarningDetail)x).ProjectID == earning.ProjectID &&
					((PREarningDetail)x).ProjectTaskID == earning.ProjectTaskID &&
					((PREarningDetail)x).LabourItemID == earning.LabourItemID &&
					((PREarningDetail)x).CertifiedJob == true &&
					((EPEarningType)x).OvertimeMultiplier != 0))
			{
				PREarningDetail excessWageEarning = result;
				EPEarningType excessWageEarningType = result;
				decimal? prevailingRate = PrevailingWage.SelectSingle(excessWageEarning.ProjectID, excessWageEarning.LabourItemID, excessWageEarning.Date, excessWageEarning.ProjectTaskID)?.WageRate;
				if (prevailingRate.HasValue)
				{
					prevailingRate *= excessWageEarningType.OvertimeMultiplier.GetValueOrDefault(1);
					prevailingRate = Math.Round(prevailingRate.GetValueOrDefault(), 2, MidpointRounding.AwayFromZero);
					if (excessWageEarning.Rate > prevailingRate)
					{
						if (!excessWages.ContainsKey(excessWageEarningType.TypeCD))
						{
							excessWages[excessWageEarningType.TypeCD] = new WagesAbovePrevailing();
						}
						excessWages[excessWageEarningType.TypeCD].Add(prevailingRate, excessWageEarning.Amount, excessWageEarning.Hours);
					}
				}
			}

			decimal applicableHours = GetFringeEarningApplicableHours(
				project.ContractID,
				earning.ProjectTaskID,
				earning.LabourItemID,
				Caches[typeof(PMProject)].GetExtension<PMProjectExtension>(project).WageAbovePrevailingAnnualizationException == true);
			if (applicableHours == 0)
			{
				return 0;
			}

			foreach (KeyValuePair<string, WagesAbovePrevailing> kvp in excessWages)
			{
				FringeEarningDecreasingRateKey key = new FringeEarningDecreasingRateKey(earning.ProjectID, kvp.Key, earning.LabourItemID, earning.ProjectTaskID, true);
				if (!PaymentsToProcess[Payments.Current].FringeRateReducingEarnings.ContainsKey(key))
				{
					PaymentsToProcess[Payments.Current].FringeRateReducingEarnings[key] = new PRPaymentFringeEarningDecreasingRate()
					{
						ProjectID = earning.ProjectID,
						EarningTypeCD = kvp.Key,
						LaborItemID = earning.LabourItemID,
						ProjectTaskID = earning.ProjectTaskID,
						ApplicableHours = applicableHours,
						Amount = kvp.Value.ExcessWageAmount,
						ActualPayRate = kvp.Value.EffectivePayRate,
						PrevailingWage = kvp.Value.EffectivePrevailingRate
					};
				}
			}

			return excessWages.Values.Sum(x => x.ExcessWageAmount) / applicableHours;
		}

		protected virtual Dictionary<int?, FringeAmountInfo> SplitFringeRates(
			List<FringeSourceEarning> fringeRates)
		{
			Dictionary<int?, FringeAmountInfo> projectFringeAmounts = new Dictionary<int?, FringeAmountInfo>();

			foreach (IGrouping<int?, FringeSourceEarning> projectGroup in fringeRates.GroupBy(x => x.Project.ContractID))
			{
				decimal totalProjectFringeAmount = projectGroup.Sum(x => x.CalculatedFringeAmount);
				int? destinationBenefitCodeID = projectGroup.First().Project.GetExtension<PMProjectExtension>().BenefitCodeReceivingFringeRate;
				if (destinationBenefitCodeID != null)
				{
					decimal projectFringeAmountAsBenefit = totalProjectFringeAmount;

					IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> deductions = GetDeductions(codeID: destinationBenefitCodeID);
					PRDeductCode deductCode = deductions.FirstOrDefault() ??
						new SelectFrom<PRDeductCode>.Where<PRDeductCode.codeID.IsEqual<P.AsInt>>.View(this).SelectSingle(destinationBenefitCodeID);

					PRYtdDeductions ytd = YtdDeduction.SelectSingle(deductCode.CodeID, Payments.Current.TransactionDate.Value.Year.ToString());
					decimal? benefitOnPaycheck = deductions.Sum(x => ((PRPaymentDeduct)x).CntAmount.GetValueOrDefault());
					decimal? benefitYtd = benefitOnPaycheck + (ytd?.EmployerAmount ?? 0);

					string cntMaxFreqType = ((PREmployeeDeduct)deductions.FirstOrDefault())?.CntMaxFreqType ?? deductCode.CntMaxFreqType;
					decimal? cntMaxAmount = ((PREmployeeDeduct)deductions.FirstOrDefault())?.CntMaxAmount ?? deductCode.CntMaxAmount;
					if (cntMaxFreqType == DeductionMaxFrequencyType.PerPayPeriod && benefitOnPaycheck + projectFringeAmountAsBenefit > cntMaxAmount)
					{
						projectFringeAmountAsBenefit = (cntMaxAmount - benefitOnPaycheck).GetValueOrDefault();
					}
					else if (cntMaxFreqType == DeductionMaxFrequencyType.PerCalendarYear && benefitYtd + projectFringeAmountAsBenefit > cntMaxAmount)
					{
						projectFringeAmountAsBenefit = (cntMaxAmount - benefitYtd).GetValueOrDefault();
					}

					if (projectFringeAmountAsBenefit > 0)
					{
						PRPaymentDeduct projectPaymentDeduct = deductions.FirstOrDefault(x => ((PRPaymentDeduct)x).Source == PaymentDeductionSourceAttribute.CertifiedProject) ??
							new PRPaymentDeduct()
							{
								DocType = Payments.Current.DocType,
								RefNbr = Payments.Current.RefNbr,
								CodeID = deductCode.CodeID,
								Source = PaymentDeductionSourceAttribute.CertifiedProject,
								CntAmount = 0
							};
						projectPaymentDeduct = Deductions.Locate(projectPaymentDeduct) ?? projectPaymentDeduct;
						projectPaymentDeduct.IsActive = true;
						projectPaymentDeduct.CntAmount += projectFringeAmountAsBenefit;
						Deductions.Update(projectPaymentDeduct);
						projectFringeAmounts[projectGroup.Key] = new FringeAmountInfo(deductCode, totalProjectFringeAmount, projectFringeAmountAsBenefit);
						continue;
					}
				}
				projectFringeAmounts[projectGroup.Key] = new FringeAmountInfo(null, totalProjectFringeAmount, 0);
			}

			return projectFringeAmounts;
		}

		protected virtual decimal GetFringeApplicableHours(bool annualizationException)
		{
			if (annualizationException)
			{
				return AllProjectHours.SelectSingle().Hours ?? 0m;
			}

			return GetApplicableHours();
		}

		protected virtual decimal GetFringeEarningApplicableHours(int? projectID, int? projectTaskID, int? laborItemID, bool annualizationException)
		{
			if (annualizationException)
			{
				return ProjectHours.SelectSingle(projectID, projectTaskID, laborItemID).Hours ?? 0m;
			}

			return GetApplicableHours();
		}

		protected virtual decimal GetApplicableHours()
		{
			PREmployee employee = PXSelectorAttribute.Select<PRPayment.employeeID>(Payments.Cache, Payments.Current) as PREmployee;
			if (employee.StdWeeksPerYear == null || employee.StdWeeksPerYear == 0 || Payments.Current.StartDate == null || Payments.Current.EndDate == null)
			{
				return 0m;
			}

			decimal hoursPerYear;
			if (employee.OverrideHoursPerYearForCertified == true && employee.HoursPerYearForCertified != null)
			{
				hoursPerYear = (decimal)employee.HoursPerYearForCertified;
			}
			else
			{
				hoursPerYear = AnnualBaseForCertifiedAttribute.GetHoursPerYear(Caches[typeof(PREmployee)], employee);
			}

			decimal weeksInPaycheck = (decimal)((Payments.Current.EndDate.Value.Date - Payments.Current.StartDate.Value.Date).TotalDays + 1d) / 7m;
			return hoursPerYear / employee.StdWeeksPerYear.Value * weeksInPaycheck;
		}

		protected virtual void ApplyFringeRates()
		{
			foreach (PaymentCalculationInfo paymentInfo in PaymentsToProcess.Where(x => x.FringeAmountsPerProject.Any()))
			{
				Payments.Current = paymentInfo.Payment;

				Dictionary<PaymentFringeBenefitKey, PRPaymentFringeBenefit> paymentFringeBenefits = PaymentFringeBenefits.Select().FirstTableItems
					.Where(x => x.DocType == paymentInfo.Payment.DocType && x.RefNbr == paymentInfo.Payment.RefNbr)
					.ToDictionary(k => new PaymentFringeBenefitKey(k.ProjectID, k.LaborItemID, k.ProjectTaskID), v => v);

				Dictionary<int?, FringeAmountInfo> fringeAmountsPerProject = paymentInfo.FringeAmountsPerProject;
				List<FringeSourceEarning> fringeRates = paymentInfo.FringeRates;

				HashSet<int?> deductCodesWithFringeAdded = new HashSet<int?>();
				foreach (IGrouping<int?, FringeSourceEarning> projectGroup in fringeRates.GroupBy(x => x.Project.ContractID))
				{
					if (fringeAmountsPerProject.ContainsKey(projectGroup.Key))
					{
						decimal totalFringeAmount = fringeAmountsPerProject[projectGroup.Key].TotalProjectFringeAmount;
						decimal fringeAmountAsBenefit = fringeAmountsPerProject[projectGroup.Key].ProjectFringeAmountAsBenefit;
						decimal fringeAmountAsEarning = totalFringeAmount - fringeAmountAsBenefit;

						if (fringeAmountAsBenefit > 0)
						{
							deductCodesWithFringeAdded.Add(fringeAmountsPerProject[projectGroup.Key].DeductCode.CodeID);
						}

						if (fringeAmountAsEarning > 0 && totalFringeAmount != 0)
						{
							decimal amountToSubstract = 0;
							foreach (FringeSourceEarning sourceEarningInfo in projectGroup.Where(x => x.CalculatedFringeAmount != 0).OrderBy(x => x.CalculatedFringeAmount))
							{
								decimal adjustedFringeAmount = sourceEarningInfo.CalculatedFringeAmount - amountToSubstract;
								if (adjustedFringeAmount < 0)
								{
									amountToSubstract -= sourceEarningInfo.CalculatedFringeAmount;
								}
								else
								{
									decimal adjustedFringeRate = adjustedFringeAmount / sourceEarningInfo.Earning.Hours.Value;
									InsertFringeRate(sourceEarningInfo.Earning, adjustedFringeRate * fringeAmountAsEarning / totalFringeAmount);
									amountToSubstract = 0;
								}
							}
						}

						decimal totalFringeAmountInBenefitAssigned = 0;
						foreach (var fringeBenefitGrouping in projectGroup.GroupBy(x => new { x.Earning.LabourItemID, x.Earning.ProjectTaskID }))
						{
							int? projectID = projectGroup.Key;
							int? laborItemID = fringeBenefitGrouping.Key.LabourItemID;
							int? projectTaskID = fringeBenefitGrouping.Key.ProjectTaskID;

							PaymentFringeBenefitKey key = new PaymentFringeBenefitKey(projectID, laborItemID, projectTaskID);
							if (!paymentFringeBenefits.TryGetValue(key, out PRPaymentFringeBenefit fringeBenefit))
							{
								fringeBenefit = PaymentFringeBenefits.Insert(new PRPaymentFringeBenefit()
								{
									ProjectID = projectID,
									LaborItemID = laborItemID,
									ProjectTaskID = projectTaskID
								});
							}
							fringeBenefit.ApplicableHours = fringeBenefitGrouping.Where(x => x.Earning.CertifiedJob == true).Sum(x => x.Earning.Hours.Value);
							fringeBenefit.ProjectHours = projectGroup.Sum(x => x.Earning.Hours.Value);
							fringeBenefit.FringeRate = GetNominalFringeRate(fringeBenefitGrouping);
							fringeBenefit.PaidFringeAmount = Math.Max(fringeBenefitGrouping.Sum(x => x.CalculatedFringeAmount), 0m);
							decimal unroundedFringeAmountInBenefit = totalFringeAmount == 0 ?
								0 : fringeAmountAsBenefit * fringeBenefit.PaidFringeAmount.GetValueOrDefault() / totalFringeAmount;
							fringeBenefit.FringeAmountInBenefit = Math.Round(unroundedFringeAmountInBenefit, 2, MidpointRounding.AwayFromZero);
							totalFringeAmountInBenefitAssigned += fringeBenefit.FringeAmountInBenefit.Value;
							PaymentFringeBenefits.Update(fringeBenefit);
							paymentFringeBenefits[key] = fringeBenefit;
						}

						// Handle rounding for FringeAmountInBenefit
						if (totalFringeAmountInBenefitAssigned != fringeAmountAsBenefit)
						{
							PRPaymentFringeBenefit recordWithHighestAmount =
								paymentFringeBenefits.Where(x => x.Key.ProjectID == projectGroup.Key).OrderByDescending(x => x.Value.FringeAmountInBenefit).First().Value;
							recordWithHighestAmount.FringeAmountInBenefit += fringeAmountAsBenefit - totalFringeAmountInBenefitAssigned;
						}
					}
				}

				Actions.PressSave();
				BenefitDetails.Select().FirstTableItems.Where(x => deductCodesWithFringeAdded.Contains(x.CodeID)).ForEach(x => BenefitDetails.Delete(x));
				deductCodesWithFringeAdded.ForEach(deductCodeID =>
					_CalculationUtils.Value.CreateBenefitDetail(
						this,
						BenefitDetails.Cache,
						GetDeductions(codeID: deductCodeID).FirstOrDefault(),
						PaymentEarningDetails.Select().FirstTableItems.ToList()));
			}
		}

		protected virtual decimal GetNominalFringeRate(IEnumerable<FringeSourceEarning> fringeBenefitGrouping)
		{
			decimal totalHours = fringeBenefitGrouping.Sum(x => x.Earning.Hours.GetValueOrDefault());
			if (totalHours == 0)
			{
				return 0;
			}

			return Math.Round(fringeBenefitGrouping.Sum(x => x.Earning.Hours.GetValueOrDefault() * x.SetupFringeRate * x.OvertimeMultiplier) / totalHours, 2, MidpointRounding.AwayFromZero);
		}

		protected virtual void InsertFringeRate(PREarningDetail originalEarning, decimal fringeRate)
		{
			PXCache cache = Caches[typeof(PREarningDetail)];
			PREarningDetail fringeEarning = EarningDetailHelper.CreateEarningDetailCopy(cache, originalEarning, true);
			fringeEarning.Rate = fringeRate;
			fringeEarning.ManualRate = false;
			fringeEarning.UnitType = UnitType.Hour;
			cache.SetDefaultExt<PREarningDetail.amount>(fringeEarning);

			cache.Update(fringeEarning);
		}

		protected virtual void AdjustFringePayoutBenefits()
		{
			List<PRPayment> paymentsToCalculate = PaymentsToProcess
				.Where(x => x.FringeAmountsPerProject.Values.Any(y => y.DeductCode?.AffectsTaxes == true && y.ProjectFringeAmountAsBenefit > 0)
					&& x.Payment.CountryID == LocationConstants.USCountryCode)
				.Select(x => x.Payment).ToList();

			if (!paymentsToCalculate.Any())
			{
				return;
			}

			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				foreach (PRPayment payment in paymentsToCalculate)
				{
					Payments.Current = payment;
					string refNbr = payrollAssemblyScope.Proxy.AddPayroll(CreatePayrollBase<PRPayrollBase>(payment), new List<PRWage>(), PrepareTaxableBenefitData(false).ToList());
					payrollAssemblyScope.Proxy.SetEmployeeResidenceLocationCode(refNbr, CurrentEmployeeResidenceAddress.SelectSingle()?.TaxLocationCode);
				}

				foreach (var payrollCalculation in payrollAssemblyScope.Proxy.CalculatePayroll(LocationConstants.USCountryCode))
				{
					PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
					Payments.Current = payment;
					IEnumerable<PRPaymentDeduct> paymentDeductions = AllPaymentDeductions.Select().FirstTableItems;
					foreach (PRBenefitCalculation benefitCalculation in payrollCalculation.BenefitCalculations)
					{
						decimal benefitMissing = benefitCalculation.EmployerOriginalAmount - benefitCalculation.EmployerCalculatedAmount;
						if (benefitMissing <= 0)
						{
							continue;
						}

						IEnumerable<KeyValuePair<int?, FringeAmountInfo>> applicableFringeBenefits =
							PaymentsToProcess[payrollCalculation.ReferenceNbr].FringeAmountsPerProject.Where(x => x.Value.DeductCode?.CodeCD == benefitCalculation.CodeCD);
						if (!applicableFringeBenefits.Any())
						{
							continue;
						}

						int? codeID = applicableFringeBenefits.First().Value.DeductCode.CodeID;
						decimal totalFringeAmountForBenefit = applicableFringeBenefits.Sum(x => x.Value.ProjectFringeAmountAsBenefit);
						if (totalFringeAmountForBenefit != 0)
						{
							Dictionary<int?, FringeAmountInfo> newAmounts = new Dictionary<int?, FringeAmountInfo>();
							foreach (KeyValuePair<int?, FringeAmountInfo> kvp in applicableFringeBenefits)
							{
								decimal adjustedBenefitAmount = kvp.Value.ProjectFringeAmountAsBenefit - benefitMissing * kvp.Value.ProjectFringeAmountAsBenefit / totalFringeAmountForBenefit;
								newAmounts[kvp.Key] = new FringeAmountInfo(kvp.Value.DeductCode, kvp.Value.TotalProjectFringeAmount, adjustedBenefitAmount);
							}
							newAmounts.ForEach(kvp => PaymentsToProcess[payrollCalculation.ReferenceNbr].FringeAmountsPerProject[kvp.Key] = kvp.Value);
						}

						PRPaymentDeduct adjustedPaymentDeduct = paymentDeductions.First(x => x.CodeID == codeID && x.Source == PaymentDeductionSourceAttribute.CertifiedProject);
						adjustedPaymentDeduct.CntAmount -= benefitMissing;
						_CalculationErrors.AddWarning<PRPaymentDeduct.cntAmount>(
							adjustedPaymentDeduct,
							adjustedPaymentDeduct.CntAmount,
							Messages.BenefitAdjustedByTaxEngine);
						AllPaymentDeductions.Update(adjustedPaymentDeduct);
					}
					PaymentsToProcess.UpdatePayment(Payments.Current);
				}
			}
		}

		protected virtual void CalculateTaxesOnFringeEarnings(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollCalculationProxy>())
			{
				bool calculateNeeded = false;
				foreach (var payrollCalculation in payrollCalculations)
				{
					PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
					Payments.Current = payment;

					if (FringeBenefitCalculatedEarnings.Select().Any() && payment.CountryID == LocationConstants.USCountryCode)
					{
						PaymentTaxes.Select().ForEach(x => PaymentTaxes.Delete(x));
						calculateNeeded = true;
						string refNbr = payrollAssemblyScope.Proxy.AddPayroll(CreatePayrollBase<PRPayrollBase>(payment), PrepareWageData().ToList(), PrepareTaxableBenefitData(false).ToList());
						PopulateTaxSettingData(refNbr, payrollAssemblyScope);
					}
				}

				if (calculateNeeded)
				{
					PaymentTaxesSplit.Cache.Persist(PXDBOperation.Delete);
					PaymentTaxesSplit.Cache.Clear();

					foreach (var payrollCalculation in payrollAssemblyScope.Proxy.CalculatePayroll(LocationConstants.USCountryCode))
					{
						Payments.Current = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
						SaveTaxCalculations(payrollCalculation);
						PaymentsToProcess.UpdatePayment(Payments.Current);
					}
				}
			}
		}

		protected virtual void CalculateWorkersCompensation(decimal minNetIncome)
		{
			List<PRPaymentDeduct> wcDeductions = ZeroOutWCDeductions().ToList();
			Dictionary<WCPremiumKey, PRWorkCompensationMaximumInsurableWage> maximumInsurableWages = GetEffectiveWCMaximumInsurableWages();
			Dictionary<WCPremiumKey, decimal> ytdDeductionWCWageBase = GetYtdWCWageBase(ContributionType.EmployeeDeduction, maximumInsurableWages);
			Dictionary<WCPremiumKey, decimal> ytdBenefitWCWageBase = GetYtdWCWageBase(ContributionType.EmployerContribution, maximumInsurableWages);

			Dictionary<WCPremiumKey, WCPremiumDetails> premiumDetails = new Dictionary<WCPremiumKey, WCPremiumDetails>();
			foreach (PXResult<PREarningDetail, PRLocation, Address, EPEarningType, PRDeductCode> result in WorkCodeEarnings.Select())
			{
				PREarningDetail earningDetail = result;
				PRDeductCode deductCode = result;
				string state = ((Address)result).State;
				bool isOvertime = ((EPEarningType)result).IsOvertime == true;

				WCPremiumDetails calculationDetails = new WCPremiumDetails();
				calculationDetails.ApplicableRegularEarningAmountForDed = isOvertime ? 0m : GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction, earningDetail);
				calculationDetails.ApplicableRegularEarningAmountForBen = isOvertime ? 0m : GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution, earningDetail);
				calculationDetails.ApplicableRegularEarningHoursForDed = isOvertime ? 0m : GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction, earningDetail);
				calculationDetails.ApplicableRegularEarningHoursForBen = isOvertime ? 0m : GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution, earningDetail);
				calculationDetails.ApplicableOvertimeEarningAmountForDed = isOvertime ? GetDedBenApplicableAmount(deductCode, ContributionType.EmployeeDeduction, earningDetail) : 0m;
				calculationDetails.ApplicableOvertimeEarningAmountForBen = isOvertime ? GetDedBenApplicableAmount(deductCode, ContributionType.EmployerContribution, earningDetail) : 0m;
				calculationDetails.ApplicableOvertimeEarningHoursForDed = isOvertime ? GetDedBenApplicableHours(deductCode, ContributionType.EmployeeDeduction, earningDetail) : 0m;
				calculationDetails.ApplicableOvertimeEarningHoursForBen = isOvertime ? GetDedBenApplicableHours(deductCode, ContributionType.EmployerContribution, earningDetail) : 0m;

				WCPremiumKey key = new WCPremiumKey(earningDetail.WorkCodeID, state, earningDetail.BranchID);
				AdjustWCApplicableAmountsForMaximumInsurableWage(maximumInsurableWages, ytdDeductionWCWageBase, ytdBenefitWCWageBase, calculationDetails, key);

				if (premiumDetails.ContainsKey(key))
				{
					calculationDetails.Add(premiumDetails[key]);
				}
				premiumDetails[key] = calculationDetails;
			}

			PXResultset<PRDeductCode> wcDeductCodes = SelectFrom<PRDeductCode>
				.Where<PRDeductCode.isWorkersCompensation.IsEqual<True>
					.And<PRDeductCode.isActive.IsEqual<True>>
					.And<PRDeductCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>>.View.Select(this);
			foreach (KeyValuePair<WCPremiumKey, WCPremiumDetails> kvp in premiumDetails)
			{
				PRDeductCode deductCode = wcDeductCodes.FirstOrDefault(x => ((PRDeductCode)x).State == kvp.Key.State);
				if (deductCode != null)
				{
					PRWorkCompensationBenefitRate rate = GetApplicableWCRate(this, kvp.Key.WorkCodeID, deductCode.CodeID, kvp.Key.BranchID);
					if (rate != null)
					{
						decimal benefitAmount = 0m;
						decimal deductionAmount = 0m;
						switch (deductCode.CntCalcType)
						{
							case DedCntCalculationMethod.PercentOfGross:
							case DedCntCalculationMethod.PercentOfCustom:
								benefitAmount = rate.Rate.GetValueOrDefault() * kvp.Value.ApplicableTotalEarningAmountForBen / 100;
								break;
							case DedCntCalculationMethod.AmountPerHour:
								benefitAmount = rate.Rate.GetValueOrDefault() * kvp.Value.ApplicableTotalEarningHoursForBen;
								break;
						}

						if (deductCode.ContribType != ContributionType.EmployerContribution)
						{
							switch (deductCode.DedCalcType)
							{
								case DedCntCalculationMethod.PercentOfGross:
								case DedCntCalculationMethod.PercentOfCustom:
									deductionAmount = rate.DeductionRate.GetValueOrDefault() * kvp.Value.ApplicableTotalEarningAmountForDed / 100;
									break;
								case DedCntCalculationMethod.AmountPerHour:
									deductionAmount = rate.DeductionRate.GetValueOrDefault() * kvp.Value.ApplicableTotalEarningHoursForDed;
									break;
							}
						}

						deductionAmount = Math.Round(deductionAmount, 2, MidpointRounding.AwayFromZero);
						benefitAmount = Math.Round(benefitAmount, 2, MidpointRounding.AwayFromZero);
						List<PRPaymentWCPremium> premiums = new List<PRPaymentWCPremium>();
						if (deductCode.ContribType == ContributionType.BothDeductionAndContribution && kvp.Value.SameApplicableForDedAndBen)
						{
							premiums.Add(new PRPaymentWCPremium()
							{
								WorkCodeID = kvp.Key.WorkCodeID,
								DeductCodeID = deductCode.CodeID,
								BranchID = kvp.Key.BranchID,
								DeductionRate = rate.DeductionRate,
								Rate = rate.Rate,
								RegularWageBaseHours = kvp.Value.ApplicableRegularEarningHoursForBen,
								OvertimeWageBaseHours = kvp.Value.ApplicableOvertimeEarningHoursForBen,
								RegularWageBaseAmount = kvp.Value.ApplicableRegularEarningAmountForBen,
								OvertimeWageBaseAmount = kvp.Value.ApplicableOvertimeEarningAmountForBen,
								DeductionAmount = deductionAmount,
								Amount = benefitAmount,
								ContribType = ContributionType.BothDeductionAndContribution
							});
						}
						else
						{
							if (deductCode.ContribType != ContributionType.EmployerContribution && (deductionAmount != 0 || kvp.Value.HasAnyApplicableForDed))
							{
								premiums.Add(new PRPaymentWCPremium()
								{
									WorkCodeID = kvp.Key.WorkCodeID,
									DeductCodeID = deductCode.CodeID,
									BranchID = kvp.Key.BranchID,
									DeductionRate = rate.DeductionRate,
									RegularWageBaseHours = kvp.Value.ApplicableRegularEarningHoursForDed,
									OvertimeWageBaseHours = kvp.Value.ApplicableOvertimeEarningHoursForDed,
									RegularWageBaseAmount = kvp.Value.ApplicableRegularEarningAmountForDed,
									OvertimeWageBaseAmount = kvp.Value.ApplicableOvertimeEarningAmountForDed,
									DeductionAmount = deductionAmount,
									Amount = 0,
									ContribType = ContributionType.EmployeeDeduction
								});
							}

							if (deductCode.ContribType != ContributionType.EmployeeDeduction && (benefitAmount != 0 || kvp.Value.HasAnyApplicableForBen))
							{
								premiums.Add(new PRPaymentWCPremium()
								{
									WorkCodeID = kvp.Key.WorkCodeID,
									DeductCodeID = deductCode.CodeID,
									BranchID = kvp.Key.BranchID,
									Rate = rate.Rate,
									RegularWageBaseHours = kvp.Value.ApplicableRegularEarningHoursForBen,
									OvertimeWageBaseHours = kvp.Value.ApplicableOvertimeEarningHoursForBen,
									RegularWageBaseAmount = kvp.Value.ApplicableRegularEarningAmountForBen,
									OvertimeWageBaseAmount = kvp.Value.ApplicableOvertimeEarningAmountForBen,
									DeductionAmount = 0,
									Amount = benefitAmount,
									ContribType = ContributionType.EmployerContribution
								});
							}
						}

						deductionAmount = 0;
						benefitAmount = 0;
						foreach (PRPaymentWCPremium premium in premiums)
						{
							PRPaymentWCPremium inserted = WCPremiums.Insert(premium);
							deductionAmount += inserted.DeductionAmount.GetValueOrDefault();
							benefitAmount += inserted.Amount.GetValueOrDefault();
						}
						PaymentsToProcess[Payments.Current].WCDeductionAmount += deductionAmount;

						PRPaymentDeduct deduction = wcDeductions.FirstOrDefault(x => x.CodeID == deductCode.CodeID);
						if (deduction == null)
						{
							deduction = new PRPaymentDeduct()
							{
								CodeID = deductCode.CodeID,
								Source = PaymentDeductionSourceAttribute.WorkCode,
								DedAmount = 0,
								CntAmount = 0
							};

							wcDeductions.Add(deduction);
						}
						else if (!string.IsNullOrEmpty(deduction.DocType) && !string.IsNullOrEmpty(deduction.RefNbr))
						{
							MarkDeductionAsModified(deduction);
						}

						deduction.IsActive = true;
						deduction.DedAmount += deductionAmount;
						deduction.CntAmount += benefitAmount;

						if (deductionAmount > 0 && PaymentsToProcess[Payments.Current].NetIncomeAccumulator < minNetIncome)
						{
							// WC deduction amount can't be adjusted => throw error
							_CalculationErrors.AddError<PRPaymentDeduct.dedAmount>(
								deduction,
								deduction.DedAmount,
								Messages.DeductionCausesNetPayBelowMin);
						}
					}
				}
			}

			foreach (PRPaymentDeduct deduction in wcDeductions)
			{
				Deductions.Update(deduction);
			}
		}

		protected virtual IEnumerable<PRPaymentDeduct> ZeroOutWCDeductions()
		{
			foreach (PRPaymentDeduct deduct in SelectFrom<PRPaymentDeduct>
				.Where<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
					.And<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>
					.And<PRPaymentDeduct.source.IsEqual<PaymentDeductionSourceAttribute.workCode>>>.View.Select(this))
			{
				deduct.DedAmount = 0m;
				deduct.CntAmount = 0m;
				yield return deduct;
			}
		}

		protected virtual void AdjustWCApplicableAmountsForMaximumInsurableWage(
			Dictionary<WCPremiumKey, PRWorkCompensationMaximumInsurableWage> maximumInsurableWages,
			Dictionary<WCPremiumKey, decimal> ytdDeductionWCWageBase,
			Dictionary<WCPremiumKey, decimal> ytdBenefitWCWageBase,
			WCPremiumDetails calculationDetails,
			WCPremiumKey key)
		{
			key.BranchID = null;
			if (!maximumInsurableWages.TryGetValue(key, out PRWorkCompensationMaximumInsurableWage maximumInsurableWage))
			{
				return;
			}

			decimal maximumInsurableWageAmount = maximumInsurableWage.MaximumInsurableWage.GetValueOrDefault();

			if (calculationDetails.ApplicableRegularEarningAmountForDed > 0)
			{
				AdjustAmountForMaximumInsurableWage(
					maximumInsurableWageAmount,
					ytdDeductionWCWageBase,
					key,
					ref calculationDetails.ApplicableRegularEarningAmountForDed,
					ref calculationDetails.ApplicableRegularEarningHoursForDed);
			}
			else if (calculationDetails.ApplicableOvertimeEarningAmountForDed > 0)
			{
				AdjustAmountForMaximumInsurableWage(
					maximumInsurableWageAmount,
					ytdDeductionWCWageBase,
					key,
					ref calculationDetails.ApplicableOvertimeEarningAmountForDed,
					ref calculationDetails.ApplicableOvertimeEarningHoursForDed);
			}

			if (calculationDetails.ApplicableRegularEarningAmountForBen > 0)
			{
				AdjustAmountForMaximumInsurableWage(
					maximumInsurableWageAmount,
					ytdBenefitWCWageBase,
					key,
					ref calculationDetails.ApplicableRegularEarningAmountForBen,
					ref calculationDetails.ApplicableRegularEarningHoursForBen);
			}
			else if (calculationDetails.ApplicableOvertimeEarningAmountForBen > 0)
			{
				AdjustAmountForMaximumInsurableWage(
					maximumInsurableWageAmount,
					ytdBenefitWCWageBase,
					key,
					ref calculationDetails.ApplicableOvertimeEarningAmountForBen,
					ref calculationDetails.ApplicableOvertimeEarningHoursForBen);
			}
		}

		protected virtual void AdjustAmountForMaximumInsurableWage(
			decimal maximumInsurableWage,
			Dictionary<WCPremiumKey, decimal> ytdWages,
			WCPremiumKey key,
			ref decimal applicableEarningAmount,
			ref decimal applicableHours)
		{
			decimal ytdWageBase = 0;
			if (ytdWages.ContainsKey(key))
			{
				ytdWageBase = ytdWages[key];
			}

			decimal originalAmount = applicableEarningAmount;
			applicableEarningAmount = Math.Min(applicableEarningAmount, maximumInsurableWage - ytdWageBase);
			applicableEarningAmount = Math.Max(applicableEarningAmount, 0m);
			applicableHours = PRUtils.Round(applicableEarningAmount / originalAmount * applicableHours);

			ytdWages[key] = applicableEarningAmount + ytdWageBase;
		}

		protected virtual Dictionary<WCPremiumKey, PRWorkCompensationMaximumInsurableWage> GetEffectiveWCMaximumInsurableWages()
		{
			var effectives = new Dictionary<WCPremiumKey, PRWorkCompensationMaximumInsurableWage>();
			foreach (PXResult<PRWorkCompensationMaximumInsurableWage, PRDeductCode> record in WCMaximumInsurableWages.Select())
			{
				PRWorkCompensationMaximumInsurableWage maxInsurableWage = record;
				PRDeductCode deductCode = record;
				WCPremiumKey key = new WCPremiumKey(maxInsurableWage.WorkCodeID, deductCode.State, null);
				if (!effectives.TryGetValue(key, out PRWorkCompensationMaximumInsurableWage existingMaxInsurableWage)
					|| maxInsurableWage.EffectiveDate > existingMaxInsurableWage.EffectiveDate)
				{
					effectives[key] = maxInsurableWage;
				}
			}

			return effectives;
		}

		protected static PRWorkCompensationBenefitRate GetApplicableWCRate(PXGraph graph, string workCodeID, int? deductCodeID, int? branchID)
		{
			List<PRWorkCompensationBenefitRate> rates = SelectFrom<PRWorkCompensationBenefitRate>
				.InnerJoin<PMWorkCode>.On<PMWorkCode.workCodeID.IsEqual<PRWorkCompensationBenefitRate.workCodeID>>
				.Where<PRWorkCompensationBenefitRate.workCodeID.IsEqual<P.AsString>
					.And<PRWorkCompensationBenefitRate.deductCodeID.IsEqual<P.AsInt>>
					.And<PMWorkCode.isActive.IsEqual<True>>
					.And<PRxPMWorkCode.countryID.IsEqual<PRPayment.countryID.FromCurrent>>
					.And<PRWorkCompensationBenefitRate.effectiveDate.IsLessEqual<PRPayment.transactionDate.FromCurrent>>>
				.OrderBy<PRWorkCompensationBenefitRate.effectiveDate.Desc>.View.Select(graph, workCodeID, deductCodeID).FirstTableItems.ToList();
			return rates.FirstOrDefault(x => x.BranchID == branchID) ?? rates.FirstOrDefault(x => x.BranchID == null);
		}

		protected virtual void SetCalculatedStatus(IEnumerable<PRPayrollCalculation> payrollCalculations)
		{
			foreach (var payrollCalculation in payrollCalculations)
			{
				PRPayment payment = PaymentsToProcess[payrollCalculation.ReferenceNbr].Payment;
				payment.Calculated = !_CalculationErrors.HasChildError(Deductions.Cache, Payments.Cache, payment);
				PaymentsToProcess.UpdatePayment(payment);
				Payments.Update(payment);
			}

			this.Actions.PressSave();
		}

		protected virtual void RecordContributionPayableBenefits()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				foreach (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> deduction in GetDeductions(contributesToGrossCalculation: true))
				{
					PRDeductCode deductCode = deduction;
					PRPaymentDeduct paymentDeduct = deduction;
					PREmployeeDeduct employeeDeduct = deduction;

					DedBenAmount calculatedAmount = CalculateRegularBenefitAmount(deductCode, employeeDeduct, paymentDeduct);
					PaymentsToProcess[payment].PayableBenefitContributingAmount += calculatedAmount.BenefitAmount.GetValueOrDefault();
				}

				// These two dictionaries were filled with incorrect values as a side-effect of CalculateRegularBenefitAmount()
				// We need to clear them here, they will be filled again with correct values in CalculateProjectUnionPostTaxBenefits()
				// or CreateBenefit()
				PaymentsToProcess[payment].NominalUnionPackageAmounts.Clear();
				PaymentsToProcess[payment].NominalProjectPackageAmounts.Clear();
			}
		}

		#endregion Calculations

		#region Overtime Rules Calculation

		protected virtual void CalculatePaymentOvertimeRules()
		{
			PRPayBatchEntry payBatchEntryGraph = CreateInstance<PRPayBatchEntry>();
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, false);

				Dictionary<int?, PREarningDetail> originalEarningDetails = PaymentEarningDetails
					.Select()
					.FirstTableItems
					.ToDictionary(item => item.RecordID, item => PXCache<PREarningDetail>.CreateCopy(item));

				foreach (PXResult<PRPaymentOvertimeRule, PROvertimeRule, EPEarningType> rule in OvertimeRulesForCalculation.Select())
				{
					PROvertimeRule overtimeRule = rule;
					Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails = GetPossibleOvertimeEarningDetails(overtimeRule);

					List<OvertimeDistribution> overtimeDistributions = new List<OvertimeDistribution>();
					if (overtimeRule.RuleType == PROvertimeRuleType.Daily)
					{
						overtimeDistributions = ProcessDailyOvertimeRule(overtimeRule, possibleOvertimeEarningDetails, originalEarningDetails);
					}
					else if (overtimeRule.RuleType == PROvertimeRuleType.Weekly)
					{
						overtimeDistributions = ProcessWeeklyOvertimeRule(overtimeRule, possibleOvertimeEarningDetails, originalEarningDetails);
					}
					else if (overtimeRule.RuleType == PROvertimeRuleType.Consecutive)
					{
						overtimeDistributions = ProcessConsecutiveOvertimeRule(overtimeRule, possibleOvertimeEarningDetails, originalEarningDetails);
					}

					SaveOvertimeCalculation(overtimeDistributions);
				}

				RegularAmountAttribute.EnforceEarningDetailUpdate<PRPayment.regularAmount>(Payments.Cache, Payments.Current, true);
				PaymentsToProcess.UpdatePayment(Payments.Current);

				payBatchEntryGraph.UpdatePayrollBatch(payment.PayBatchNbr, payment.EmployeeID, false);
			}
		}

		public virtual Dictionary<DateTime, List<PREarningDetail>> GetPossibleOvertimeEarningDetails(PROvertimeRule overtimeRule)
		{
			var baseEarningDetailRecords = new Dictionary<DateTime, List<PREarningDetail>>();

			string[] regularTypesForOvertime = SelectFrom<PRRegularTypeForOvertime>
				.InnerJoin<EPEarningType>.On<PRRegularTypeForOvertime.FK.RegularEarningType>
				.Where<PRRegularTypeForOvertime.overtimeTypeCD.IsEqual<P.AsString>
					.And<EPEarningType.isOvertime.IsNotEqual<True>>
					.And<PREarningType.isAmountBased.IsNotEqual<True>>
					.And<PREarningType.isPTO.IsNotEqual<True>>>
				.View.Select(this, overtimeRule.DisbursingTypeCD).FirstTableItems
				.Select(item => item.RegularTypeCD).ToArray();

			foreach (PXResult<PREarningDetail, PRLocation, Address> record in PaymentEarningDetails.Select())
			{
				Address address = record;
				PREarningDetail earningDetail = record;

				if (earningDetail.BaseOvertimeRecordID != null || earningDetail.IsOvertime == true)
					continue;

				if (earningDetail.IsAmountBased == true)
					continue;

				if (overtimeRule.RuleType == PROvertimeRuleType.Daily && overtimeRule.WeekDay != null &&
					(DayOfWeek)overtimeRule.WeekDay != earningDetail.Date.GetValueOrDefault().DayOfWeek)
					continue;

				if (!regularTypesForOvertime.Contains(earningDetail.TypeCD))
					continue;

				if (!string.IsNullOrWhiteSpace(overtimeRule.State) && !string.Equals(address.State, overtimeRule.State))
					continue;

				if (overtimeRule.UnionID != null && !string.Equals(earningDetail.UnionID, overtimeRule.UnionID))
					continue;

				if (overtimeRule.ProjectID != null && earningDetail.ProjectID != overtimeRule.ProjectID)
					continue;

				if (earningDetail.Date == null)
					continue;

				DateTime earningDetailDay = earningDetail.Date.Value;
				if (!baseEarningDetailRecords.ContainsKey(earningDetailDay))
					baseEarningDetailRecords[earningDetailDay] = new List<PREarningDetail>();

				baseEarningDetailRecords[earningDetailDay].Add(earningDetail);
			}

			return baseEarningDetailRecords;
		}

		public virtual List<OvertimeDistribution> ProcessDailyOvertimeRule(
			PROvertimeRule overtimeRule,
			Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails,
			Dictionary<int?, PREarningDetail> originalEarningDetails)
		{
			List<OvertimeDistribution> result = new List<OvertimeDistribution>();
			foreach (var dailyEarningDetails in possibleOvertimeEarningDetails)
			{
				List<PREarningDetail> originalDailyEarningDetails =
					dailyEarningDetails.Value.Select(item => originalEarningDetails[item.RecordID]).ToList();

				decimal averageRegularRate = CalculateAverageRegularRate(originalDailyEarningDetails);

				result.AddRange(CheckOvertimeHours(dailyEarningDetails.Value, overtimeRule, averageRegularRate));
			}

			return result;
		}

		public virtual List<OvertimeDistribution> ProcessWeeklyOvertimeRule(
			PROvertimeRule overtimeRule,
			Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails,
			Dictionary<int?, PREarningDetail> originalEarningDetails)
		{
			if (!TryGetWeeklyBasedPeriods(possibleOvertimeEarningDetails, out DateTime currentPeriodStartDate, out DateTime lastEarningDetailDate))
			{
				return new List<OvertimeDistribution>();
			}

			List<OvertimeDistribution> result = new List<OvertimeDistribution>();

			while (currentPeriodStartDate <= lastEarningDetailDate)
			{
				DateTime nextWeekStartDate = currentPeriodStartDate.AddDays(7);

				List<PREarningDetail> weeklyEarningDetails =
					possibleOvertimeEarningDetails.Where(item => item.Key >= currentPeriodStartDate && item.Key < nextWeekStartDate)
					.SelectMany(item => item.Value).ToList();

				List<PREarningDetail> originalWeeklyEarningDetails =
					weeklyEarningDetails.Select(item => originalEarningDetails[item.RecordID]).ToList();

				decimal averageRegularRate = CalculateAverageRegularRate(originalWeeklyEarningDetails);

				result.AddRange(CheckOvertimeHours(weeklyEarningDetails, overtimeRule, averageRegularRate));

				currentPeriodStartDate = nextWeekStartDate;
			}

			return result;
		}

		public virtual List<OvertimeDistribution> ProcessConsecutiveOvertimeRule(
			PROvertimeRule overtimeRule,
			Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails,
			Dictionary<int?, PREarningDetail> originalEarningDetails)
		{
			if (!TryGetWeeklyBasedPeriods(possibleOvertimeEarningDetails, out DateTime currentPeriodStartDate, out DateTime lastEarningDetailDate) ||
				overtimeRule.NumberOfConsecutiveDays == null)
			{
				return new List<OvertimeDistribution>();
			}

			List<OvertimeDistribution> result = new List<OvertimeDistribution>();

			while (currentPeriodStartDate <= lastEarningDetailDate)
			{
				DateTime nextWeekStartDate = currentPeriodStartDate.AddDays(7);

				int numberOfConsecutiveDaysWorked = 0;
				DateTime dateToApplyConsecutiveOvertimeRule = DateTime.MinValue;

				List<PREarningDetail> originalWeeklyEarningDetails = possibleOvertimeEarningDetails
					.Where(item => item.Key >= currentPeriodStartDate && item.Key < nextWeekStartDate)
					.SelectMany(item => item.Value)
					.Select(item => originalEarningDetails[item.RecordID])
					.ToList();

				decimal averageRegularRate = CalculateAverageRegularRate(originalWeeklyEarningDetails);

				for (int i = 0; i < 7; i++)
				{
					DateTime currentDate = currentPeriodStartDate.AddDays(i);
					if (possibleOvertimeEarningDetails.ContainsKey(currentDate))
					{
						numberOfConsecutiveDaysWorked++;
						if (overtimeRule.NumberOfConsecutiveDays == numberOfConsecutiveDaysWorked)
						{
							dateToApplyConsecutiveOvertimeRule = currentDate;
							break;
						}
					}
					else
					{
						numberOfConsecutiveDaysWorked = 0;
					}
				}

				if (dateToApplyConsecutiveOvertimeRule != DateTime.MinValue)
				{
					List<PREarningDetail> consecutiveEarningDetails = possibleOvertimeEarningDetails[dateToApplyConsecutiveOvertimeRule];

					result.AddRange(CheckOvertimeHours(consecutiveEarningDetails, overtimeRule, averageRegularRate));
				}

				currentPeriodStartDate = nextWeekStartDate;
			}

			return result;
		}

		public virtual bool TryGetWeeklyBasedPeriods(Dictionary<DateTime, List<PREarningDetail>> possibleOvertimeEarningDetails, out DateTime periodStartDate, out DateTime periodEndDate)
		{
			periodStartDate = DateTime.MinValue;
			periodEndDate = DateTime.MinValue;

			if (!possibleOvertimeEarningDetails.Any() || Payments.Current.StartDate == null)
			{
				return false;
			}

			PRPayGroupYearSetup payGroupYearSetup = SelectFrom<PRPayGroupYearSetup>
				.Where<PRPayGroupYearSetup.payGroupID.IsEqual<P.AsString>>
				.View.Select(this, Payments.Current.PayGroupID);

			if (payGroupYearSetup.EndYearDayOfWeek == null)
			{
				return false;
			}

			DateTime firstEarningDetailDate = possibleOvertimeEarningDetails.Keys.Min();
			periodStartDate = Payments.Current.StartDate.Value;
			DayOfWeek overtimePeriodStartDay =
				OneBasedDayOfWeek.GetZeroBasedDayOfWeek(payGroupYearSetup.EndYearDayOfWeek.Value);

			if (periodStartDate > firstEarningDetailDate)
			{
				periodStartDate = firstEarningDetailDate;
			}

			while (periodStartDate.DayOfWeek != overtimePeriodStartDay)
			{
				periodStartDate = periodStartDate.AddDays(-1);
			}

			periodEndDate = possibleOvertimeEarningDetails.Keys.Max();

			return true;
		}

		public virtual decimal CalculateAverageRegularRate(List<PREarningDetail> earningDetails)
		{
			decimal totalHours = earningDetails.Sum(item => item.Hours.GetValueOrDefault());
			decimal totalAmount = earningDetails.Sum(item => item.Amount.GetValueOrDefault());

			return totalHours > 0 ? totalAmount / totalHours : 0m;
		}

		public virtual List<OvertimeDistribution> CheckOvertimeHours(List<PREarningDetail> earningDetails, PROvertimeRule overtimeRule, decimal averageRegularRate)
		{
			decimal totalHours = earningDetails.Sum(item => item.Hours.GetValueOrDefault());
			decimal overtimeHours = totalHours - overtimeRule.OvertimeThreshold.GetValueOrDefault();
			
			if (overtimeHours <= 0 || totalHours == 0)
			{
				return new List<OvertimeDistribution>();
			}

			if (earningDetails.Count == 1)
			{
				OvertimeDistribution overtimeCalculation = ApplyOvertimeCalculation(earningDetails[0], overtimeRule, overtimeHours, averageRegularRate);
				return new List<OvertimeDistribution> { overtimeCalculation };
			}

			/*
			A tricky algorithm starts here.
			For example, we have the "Weekly" overtime rule with 40 hours threshold.
			And there are nine "regular" earning detail records with three different "regular types": "RG1", "RG2", and "RG3".
			RG1, 7 hours
			RG1, 7 hours
			RG1, 6 hours
			RG2, 7 hours
			RG2, 7 hours
			RG2, 6 hours
			RG3, 7 hours
			RG3, 7 hours
			RG3, 6 hours
			Total regular hours = 60. Overtime hours = 60 (regular hours) - 40 (overtime threshold) = 20.

			First, we need to group the "Regular" earning details by Earning Type.
			RG1: total 20 regular hours (7 + 7 + 6)
			RG2: total 20 regular hours (7 + 7 + 6)
			RG3: total 20 regular hours (7 + 7 + 6)
			And then, we calculate the overtime share for the entire earning type.
			Overtime part (out of all hours) is calculated as: 20 (overtime hours) / 60 (total hours) = 0.33(3)
			It means that one third of all regular hours should be moved to overtime.

			Prorated Overtime Hours for Earning Type RG1 = 20 (regular hours) * 0.33(3) (overtime part) = 6.67 hours.
			Prorated Overtime Hours for Earning Type RG2 = 20 (regular hours) * 0.33(3) (overtime part) = 6.67 hours.
			Prorated Overtime Hours for Earning Type RG3 = 20 (regular hours) * 0.33(3) (overtime part) = 6.67 hours, but reduced to 6.66 hours.
			Prorated Overtime Hours are rounded to 2 digits because it is a precision of "PREarningDetail.Hours" and "PRPaymentEarning.Hours" fields.
			The last record either gets the remainder or is reduced to ensure that the sum of all overtime hours (6.67 + 6.67 + 6.66) is still equal to 20.

			Then we split overtime hours between earning details of each earning type.
			6.67 overtime hours related to RG1 earning type should be split proportionally among three earning detail records
			Overtime part (for regular earning details of RG1 earning type) is calculated as = 6.67 (overtime hours) / 20 (total hours) = 0.3335
			RG1, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG1, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG1, 6 regular hours. Overtime Hours = 6 * 0.3335 = 2.00 => but increased to 2.01
			The last record either gets the remainder or is reduced to ensure that the sum of all overtime hours (2.33 + 2.33 + 2.01) is still equal to 6.67.

			The same is done for earning details of RG2 earning type.
			RG2, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG2, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG2, 6 regular hours. Overtime Hours = 6 * 0.3335 = 2.00 => but increased to 2.01
			The last record either gets the remainder or is reduced to ensure that the sum of all overtime hours (2.33 + 2.33 + 2.01) is still equal to 6.67.

			The same is done for earning details of RG3 earning type.
			RG3, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG3, 7 regular hours. Overtime Hours = 7 * 0.3335 = 2.33
			RG3, 6 regular hours. Overtime Hours = 6 * 0.3335 = 2.00
			The last record was not modified since overtime share for the RG3 type is 6.66 = (2.33 + 2.33 + 2.00).
			*/

			List<OvertimeDistribution> result = new List<OvertimeDistribution>();

			var earningDetailsByEarningTypes = earningDetails
				.GroupBy(ed => ed.TypeCD)
				.Select(edGroup =>
					new
					{
						EarningDetails = edGroup.ToArray(),
						EarningTypeHours = edGroup.Sum(ed => ed.Hours.GetValueOrDefault())
					})
				.Where(record => record.EarningTypeHours > 0)
				.ToArray();

			int earningTypeGroupIndex = 0;
			decimal earningTypeOvertimePart = overtimeHours / totalHours;
			decimal earningTypeOvertimeHoursToSplit = overtimeHours;

			foreach (var earningDetailsByEarningType in earningDetailsByEarningTypes)
			{
				earningTypeGroupIndex++;

				ProratedOvertimeHoursItem proratedEarningTypeOvertimeHoursItem = GetProratedOvertimeHours(
					earningTypeGroupIndex,
					earningDetailsByEarningTypes.Length,
					earningDetailsByEarningType.EarningTypeHours,
					earningTypeOvertimePart,
					earningTypeOvertimeHoursToSplit);

				earningTypeOvertimeHoursToSplit = proratedEarningTypeOvertimeHoursItem.HoursToSplit;

				int earningDetailIndex = 0;
				decimal overtimePart = proratedEarningTypeOvertimeHoursItem.HoursValue / earningDetailsByEarningType.EarningTypeHours;
				decimal overtimeHoursToSplit = proratedEarningTypeOvertimeHoursItem.HoursValue;

				foreach (PREarningDetail baseEarningDetail in earningDetailsByEarningType.EarningDetails)
				{
					earningDetailIndex++;
					ProratedOvertimeHoursItem proratedOvertimeHoursItem = GetProratedOvertimeHours(
						earningDetailIndex,
						earningDetailsByEarningType.EarningDetails.Length,
						baseEarningDetail.Hours.GetValueOrDefault(),
						overtimePart, overtimeHoursToSplit);

					overtimeHoursToSplit = proratedOvertimeHoursItem.HoursToSplit;
					result.Add(ApplyOvertimeCalculation(baseEarningDetail, overtimeRule, proratedOvertimeHoursItem.HoursValue, averageRegularRate));
				}
			}

			return result;
		}

		protected virtual ProratedOvertimeHoursItem GetProratedOvertimeHours(int recordIndex, int numberOfRecords, decimal regularHours, decimal overtimePart, decimal overtimeHoursToSplit)
		{
			decimal proratedOvertimeHours = Math.Round(regularHours * overtimePart, 2, MidpointRounding.AwayFromZero);

			if (recordIndex == numberOfRecords || proratedOvertimeHours > overtimeHoursToSplit)
			{
				proratedOvertimeHours = overtimeHoursToSplit;
			}
			overtimeHoursToSplit -= proratedOvertimeHours;

			return new ProratedOvertimeHoursItem(proratedOvertimeHours, overtimeHoursToSplit);
		}

		protected virtual OvertimeDistribution ApplyOvertimeCalculation(PREarningDetail baseEarningDetail, PROvertimeRule overtimeRule, decimal overtimeHours, decimal averageRegularRate)
		{
			PREarningDetail overtimeEarningDetailRecord = PXCache<PREarningDetail>.CreateCopy(baseEarningDetail);

			baseEarningDetail = PXCache<PREarningDetail>.CreateCopy(baseEarningDetail);
			baseEarningDetail.Hours = baseEarningDetail.Hours.GetValueOrDefault() - overtimeHours;

			overtimeEarningDetailRecord.RecordID = null;
			overtimeEarningDetailRecord.IsOvertime = null;
			overtimeEarningDetailRecord.Amount = null;
			overtimeEarningDetailRecord.TypeCD = overtimeRule.DisbursingTypeCD;
			overtimeEarningDetailRecord.Hours = overtimeHours;
			overtimeEarningDetailRecord.Units = null;
			overtimeEarningDetailRecord.UnitType = UnitType.Hour;
			overtimeEarningDetailRecord.ManualRate = false;
			overtimeEarningDetailRecord.Rate = averageRegularRate * overtimeRule.OvertimeMultiplier;
			overtimeEarningDetailRecord.IsRegularRate = false;
			overtimeEarningDetailRecord.BaseOvertimeRecordID = baseEarningDetail.RecordID;
			overtimeEarningDetailRecord.SourceNoteID = null;

			return new OvertimeDistribution(baseEarningDetail, overtimeEarningDetailRecord);
		}

		protected virtual void SaveOvertimeCalculation(List<OvertimeDistribution> overtimeDistributions)
		{
			using (PXTransactionScope transactionScope = new PXTransactionScope())
			{
				foreach (OvertimeDistribution overtimeDistribution in overtimeDistributions)
				{
					PaymentEarningDetails.Update(overtimeDistribution.BaseEarningDetail);

					PREarningDetail overtimeEarningDetailRecord = overtimeDistribution.OvertimeEarningDetail;

					// Get the list of null fields that will be defaulted by Insert, but should keep their null value in the overtime record.
					PXCache cache = Caches[typeof(PREarningDetail)];
					List<string> nulledFields = new List<string>();
					foreach (string field in cache.Fields)
					{
						if (!cache.Keys.Contains(field) && cache.GetValue(overtimeEarningDetailRecord, field) == null)
						{
							if (cache.GetAttributesOfType<PXDefaultAttribute>(overtimeEarningDetailRecord, field).Any(x => x.PersistingCheck == PXPersistingCheck.Nothing))
							{
								nulledFields.Add(field);
							}
						}
					}

					overtimeEarningDetailRecord = PaymentEarningDetails.Insert(overtimeEarningDetailRecord);
					foreach (string nulledField in nulledFields)
					{
						cache.SetValue(overtimeEarningDetailRecord, nulledField, null);
					}
					PaymentEarningDetails.Update(overtimeEarningDetailRecord);
				}

				transactionScope.Complete(this);
				Actions.PressSave();
			}
		}

		#endregion Overtime Calculation Rules

		#region PTO calculation
		protected virtual void InsertPaidCarryoverEarnings()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				InsertPaidCarryoverEarningsProc();
				PaymentsToProcess.UpdatePayment(Payments.Current);
			}
		}

		protected virtual void CalculatePTO()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				CalculateAmountPerYearPTO();
				CalculateFrontLoading();
				PaymentEarningDetails.Select().ForEach(earning => CalculatePTO(earning));
				CalculateCarryover();

				if (Payments.Current.DocType == PayrollType.Final)
				{
				ProcessPTOSettlement(Payments.Current);
				}

				PaymentsToProcess.UpdatePayment(Payments.Current);
			}
		}

		protected virtual void AfterCalculationPTOProcess()
		{
			foreach (PRPayment payment in PaymentsToProcess.GetAllPayments())
			{
				Payments.Current = payment;
				ProcessSamePaycheckCarryover();
				InsertSettlementEarnings(payment);
			}
		}

		/// <summary>
		/// Check if the current paycheck might accrue hours in the first PTO Year that needs to be paid in the 2nd PTO Year if the payment overlaps 2 PTO years for that bank ID.
		/// </summary>
		protected virtual void ProcessSamePaycheckCarryover()
		{
			IEnumerable<PRPaymentPTOBank> paymentPTOBanks = PaymentPTOBanks.Select().FirstTableItems;
			IEnumerable<PREarningDetail> detailPayingCarryover = PaymentEarningDetails.Select().FirstTableItems.Where(x => x.IsPayingCarryover == true);
			foreach (PREarningDetail detail in detailPayingCarryover)
			{
				IEnumerable<PRPaymentPTOBank> possibleDisbursingBanks = paymentPTOBanks.Where(x => x.EarningTypeCD == detail.TypeCD);
				PRPaymentPTOBank carryoverPayingBank = PTOHelper.GetEffectivePaymentBank(possibleDisbursingBanks, detail.Date.Value, possibleDisbursingBanks.First().BankID);
				if (carryoverPayingBank == null)
				{
					return;
				}

				IPTOBank bankSettings = PTOHelper.GetBankSettings(this, carryoverPayingBank.BankID, Payments.Current.EmployeeID.Value, carryoverPayingBank.EffectiveStartDate.Value);

				//If there aren't duplicates bankIDs or paid after 12 month, we don't have to check carryover in other banks in this paycheck.
				bool spansTwoPTOYears = PTOHelper.SpansTwoPTOYears(bankSettings.StartDate.Value, Payments.Current.StartDate.Value, Payments.Current.EndDate.Value);
				if (spansTwoPTOYears && paymentPTOBanks.Count(x => x.BankID == carryoverPayingBank.BankID) > 1 && bankSettings.CarryoverPayMonthLimit.GetValueOrDefault() < 12)
				{
					PRPaymentPTOBank carriedOverBank = PTOHelper.GetLastEffectiveBank(possibleDisbursingBanks.Where(x => x.EffectiveStartDate < carryoverPayingBank.EffectiveStartDate), carryoverPayingBank.BankID);
					IPTOBank carriedOverBankSettings = PTOHelper.GetBankSettings(this, carriedOverBank.BankID, Payments.Current.EmployeeID.Value, carriedOverBank.EffectiveStartDate.Value);
					PTOHelper.PTOYearSummary prePaymentYearSummary = PTOHelper.GetPTOYearSummary(this, carriedOverBank.EffectiveStartDate.Value, Payments.Current.EmployeeID.Value, carriedOverBankSettings);
					PTOHelper.PTOYearSummary postPaymentYearSummary = GetPTOYearAndPaymentSummary(this, carriedOverBank.EffectiveStartDate.Value, Payments.Current.EmployeeID.Value, carriedOverBankSettings);

					decimal carryoverDiff = postPaymentYearSummary.AccrualAmount.GetValueOrDefault() - prePaymentYearSummary.AccrualAmount.GetValueOrDefault();
					detail.Hours += carryoverDiff;
					PaymentEarningDetails.Update(detail);
					if (carryoverPayingBank.AccrualMethod == PTOAccrualMethod.Percentage)
					{
						CalculatePercentagePTO(carryoverPayingBank, detail, bankSettings, out decimal accrualHours, out decimal _);
						carryoverPayingBank.AccrualAmount = accrualHours;
					}

					carryoverPayingBank.PaidCarryoverAmount += carryoverDiff;
					carryoverPayingBank.ProcessedCarryover = true;
				}

				if (detail.Hours == 0)
				{
					PaymentEarningDetails.Delete(detail);
					carryoverPayingBank.ProcessedPaidCarryover = false;
				}

				PaymentPTOBanks.Update(carryoverPayingBank);
			}
		}

		protected virtual void ProcessCarryover(IPTOBank sourceBank, PRPaymentPTOBank effectiveBank, DateTime targetDate)
		{
			PTOHelper.PTOYearSummary yearSummary = GetPTOYearAndPaymentSummary(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank);

			if (yearSummary.ProcessedCarryover == false)
			{
				effectiveBank.CarryoverAmount = PTOHelper.CalculateHoursToCarryover(this, Payments.Current.EmployeeID, Payments.Current, sourceBank, yearSummary.StartDate, yearSummary.EndDate);
				effectiveBank.CarryoverMoney = PTOHelper.CalculateMoneyToCarryover(this, Payments.Current.EmployeeID, Payments.Current, sourceBank, yearSummary.StartDate, yearSummary.EndDate);
				effectiveBank.ProcessedCarryover = true;
			}
		}

		protected virtual void ProcessFrontLoading(IPTOBank sourceBank, PRPaymentPTOBank effectiveBank, DateTime targetDate)
		{
			PTOHelper.PTOYearSummary yearSummary = GetPTOYearAndPaymentSummary(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank);

			//When the user adds a new PTO bank in the Employee Payroll Settings, and we are processing it at this point, check if a PTO bank with the same effective date has already processed front loading
			var results = PTOHelper.EmployeePTOHistory.Select(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank.PTOYearStartDate.Value, sourceBank.BankID);
			var history = results.Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).Where(x => ((PRPaymentPTOBank)x).EffectiveStartDate == effectiveBank.EffectiveStartDate).ToList();
			yearSummary.ProcessedFrontLoading = history.Any(x => ((PRPaymentPTOBank)x).ProcessedFrontLoading == true && ((PRPayment)x).Voided == false && ((PRPayment)x).DocType != PayrollType.VoidCheck);

			if (yearSummary.ProcessedFrontLoading == false)
			{
				effectiveBank.FrontLoadingAmount = sourceBank.FrontLoadingAmount.GetValueOrDefault();
				effectiveBank.ProcessedFrontLoading = true;
			}
		}

		protected virtual void RegularizeAccrualAmount(IPTOBank sourceBank, PRPaymentPTOBank effectiveBank, DateTime targetDate)
		{
			PTOHelper.PTOYearSummary updatedYearSummary = GetPTOYearAndPaymentSummary(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank);
			decimal carryoverAmount;
			if (sourceBank.CarryoverType == CarryoverType.Total)
			{
				carryoverAmount = updatedYearSummary.CarryoverAmount.GetValueOrDefault();
			}
			else
			{
				carryoverAmount = PTOHelper.CalculateHoursToCarryover(this, null, Payments.Current, sourceBank, updatedYearSummary.StartDate, updatedYearSummary.EndDate);
			}
			decimal balance = carryoverAmount + updatedYearSummary.FrontLoadingAmount.GetValueOrDefault() + updatedYearSummary.AccrualAmount.GetValueOrDefault() - updatedYearSummary.TotalDecreasedHours;

			if (sourceBank.AccrualLimit != null)
			{
				if (balance <= sourceBank.AccrualLimit && effectiveBank.AccrualAmount.GetValueOrDefault() > sourceBank.AccrualLimit)
				{
					balance += effectiveBank.DisbursementAmount.GetValueOrDefault() + effectiveBank.PaidCarryoverAmount.GetValueOrDefault();
				}

				if (balance > sourceBank.AccrualLimit)
			{
				decimal diff = balance - sourceBank.AccrualLimit.GetValueOrDefault();
					decimal limitedAccrualHours = Math.Max(effectiveBank.AccrualAmount.GetValueOrDefault() - diff, 0m);
				decimal limitedAccrualFactor = effectiveBank.AccrualAmount != 0 ? limitedAccrualHours / effectiveBank.AccrualAmount.Value : 0;
				effectiveBank.AccrualAmount = limitedAccrualHours;
				effectiveBank.AccrualMoney = Math.Round(effectiveBank.AccrualMoney.GetValueOrDefault() * limitedAccrualFactor, 2, MidpointRounding.AwayFromZero);
			}
		}
		}

		protected virtual void CalculateCarryover()
		{
			IEnumerable<PRPaymentPTOBank> ptoBanks = PaymentPTOBanks.Select().FirstTableItems.Where(x => x.IsActive == true);
			foreach (PRPaymentPTOBank effectiveBank in ptoBanks)
			{
				IPTOBank sourceBank = PTOHelper.GetBankSettings(this, effectiveBank.BankID, Payments.Current.EmployeeID.Value, effectiveBank.EffectiveStartDate.Value);
				if (sourceBank?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, effectiveBank.BankID);
				}

				if (Payments.Current.DocType == PayrollType.Regular)
				{
					this.ProcessCarryover(sourceBank, effectiveBank, effectiveBank.EffectiveStartDate.Value);
				}

				PaymentPTOBanks.Update(effectiveBank);
			}
		}

		protected virtual void CalculateFrontLoading()
		{
			IEnumerable<PRPaymentPTOBank> ptoBanks = PaymentPTOBanks.Select().FirstTableItems.Where(bank => bank.IsActive == true);
			foreach (PRPaymentPTOBank effectiveBank in ptoBanks)
			{
				IPTOBank sourceBank = PTOHelper.GetBankSettings(this, effectiveBank.BankID, Payments.Current.EmployeeID.Value, effectiveBank.EffectiveStartDate.Value);
				if (sourceBank?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, effectiveBank.BankID);
				}

				if (Payments.Current.DocType == PayrollType.Regular)
				{
					this.ProcessFrontLoading(sourceBank, effectiveBank, effectiveBank.EffectiveStartDate.Value);
				}

				PaymentPTOBanks.Update(effectiveBank);
			}
		}

		protected virtual void CalculatePTO(PREarningDetail earningDetail)
		{
			foreach (IGrouping<string, PRPaymentPTOBank> bankGroup in PaymentPTOBanks.Select().FirstTableItems.GroupBy(x => x.BankID))
			{
				PRPayment currentPayment = Payments.Current;
				bool currentPaymentOnSameYear = currentPayment.StartDate.Value.Year == currentPayment.EndDate.Value.Year;
				bool bankGroupIsUnique = bankGroup.Count() == 1;

				PRPaymentPTOBank effectiveBank = bankGroupIsUnique ? bankGroup.OrderBy(x => x.EffectiveStartDate).LastOrDefault() : (currentPaymentOnSameYear ?
					bankGroup.Where(x => (x.EffectiveStartDate.Value <= earningDetail.Date.Value && x.EffectiveEndDate.Value >= earningDetail.Date.Value)).OrderBy(x => x.EffectiveStartDate).FirstOrDefault() :
					bankGroup.Single(x => x.EffectiveStartDate.Value.Year == earningDetail.Date.Value.Year));

				if ((effectiveBank.IsCertifiedJob == true && effectiveBank.IsCertifiedJob != earningDetail.CertifiedJob) || earningDetail.Date.Value < effectiveBank.EffectiveStartDate)
				{
					continue;
				}


				IPTOBank sourceBank = PTOHelper.GetBankSettings(this, effectiveBank.BankID, Payments.Current.EmployeeID.Value, earningDetail.Date.Value);
				if (sourceBank?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, effectiveBank.BankID);
				}

				if (effectiveBank.IsActive == true)
				{
					PTOHelper.PTOYearSummary yearSummary = GetPTOYearAndPaymentSummary(this, earningDetail.Date.Value, Payments.Current.EmployeeID.Value, sourceBank);
					effectiveBank.AccruingHours += earningDetail.Hours.GetValueOrDefault();
					CalculatePercentagePTO(effectiveBank, earningDetail, sourceBank, out decimal accrualHours, out decimal accrualMoney);
					effectiveBank.AccrualAmount += accrualHours;
					effectiveBank.AccrualMoney += accrualMoney;

					if (Payments.Current.DocType == PayrollType.Regular)
					{
						//Process Paid Carryover
						if (earningDetail.IsPayingCarryover == true && earningDetail.TypeCD == effectiveBank.EarningTypeCD)
						{
							effectiveBank.PaidCarryoverAmount += earningDetail.Hours.GetValueOrDefault();
						}
					}
				}

				//Calculate Disbursement Amount
				if (earningDetail.TypeCD == effectiveBank.EarningTypeCD && earningDetail.IsPayingCarryover == false)
				{
					effectiveBank.DisbursementAmount += earningDetail.Hours.GetValueOrDefault();
					effectiveBank.DisbursementMoney += earningDetail.Amount.GetValueOrDefault();

					PTOHelper.PTOYearSummary updatedYearSummary = GetPTOYearAndPaymentSummary(this, earningDetail.Date.Value, Payments.Current.EmployeeID.Value, sourceBank);
					decimal carryoverAmount = PTOHelper.CalculateHoursToCarryover(this, Payments.Current.EmployeeID, Payments.Current, sourceBank, updatedYearSummary.StartDate, updatedYearSummary.EndDate);
					decimal carryoverMoney = PTOHelper.CalculateMoneyToCarryover(this, Payments.Current.EmployeeID, Payments.Current, sourceBank, updatedYearSummary.StartDate, updatedYearSummary.EndDate);

					decimal newAccrualHours = effectiveBank.AccrualAmount.GetValueOrDefault() + carryoverAmount + updatedYearSummary.FrontLoadingAmount.GetValueOrDefault();
					decimal newAccrualMoney = effectiveBank.AccrualMoney.GetValueOrDefault() + carryoverMoney;
					decimal newDisbursementHours = effectiveBank.DisbursementAmount.GetValueOrDefault() + effectiveBank.PaidCarryoverAmount.GetValueOrDefault(); ;
					decimal newDisbursementMoney = effectiveBank.DisbursementMoney.GetValueOrDefault();

					PTOHelper.PTOHistoricalAmounts history = PTOHelper.GetPTOHistory(this, Payments.Current.TransactionDate.GetValueOrDefault(), Payments.Current.EmployeeID.GetValueOrDefault(), sourceBank);
					decimal effectiveAvailableHours = history.AvailableHours + newAccrualHours - effectiveBank.DisbursementAmount.GetValueOrDefault();
					decimal effectiveAvailableMoney = history.AvailableMoney + newAccrualMoney - effectiveBank.DisbursementMoney.GetValueOrDefault();

					CanadianPTOCalculationEngineExt canadianPTOExt = CanadianPTOCalculationEngineExt.IsActive() ? GetExtension<CanadianPTOCalculationEngineExt>() : null;
					canadianPTOExt?.AdjustPTODisbursementRate(sourceBank, earningDetail, effectiveAvailableHours, effectiveAvailableMoney);

					if (sourceBank.DisburseFromCarryover == true && newDisbursementHours > history.AvailableHours + carryoverAmount)
					{
						throw new PXException(Messages.NotEnoughLastYearCarryover, effectiveBank.BankID);
					}
					else if (sourceBank.DisburseFromCarryover == true && sourceBank.CreateFinancialTransaction == true
						&& newDisbursementMoney > history.AvailableMoney + effectiveBank.CarryoverMoney)
					{
						throw new PXException(Messages.NotEnoughLastYearCarryoverMoney, effectiveBank.BankID);
					}
					else if (sourceBank.AllowNegativeBalance == false && effectiveAvailableHours < 0)
					{
						throw new PXException(Messages.NotEnoughPTOAvailable, effectiveBank.EarningTypeCD, effectiveBank.BankID);
					}
					else if (sourceBank.AllowNegativeBalance == false && sourceBank.CreateFinancialTransaction == true
						&& newDisbursementMoney > effectiveAvailableMoney)
					{
						throw new PXException(Messages.NotEnoughPTOMoneyAvailable, effectiveBank.EarningTypeCD, effectiveBank.BankID);
					}

					canadianPTOExt?.AdjustPTODisbursementAcctAndSub(sourceBank, earningDetail, effectiveAvailableMoney, newDisbursementMoney);
				}

				if ((Payments.Current.DocType == PayrollType.Regular || Payments.Current.DocType == PayrollType.Special) && effectiveBank.IsActive == true)
				{
					this.RegularizeAccrualAmount(sourceBank, effectiveBank, earningDetail.Date.Value);
				}

				PaymentPTOBanks.Update(effectiveBank);
			}
		}

		protected virtual void CalculateAmountPerYearPTO()
		{
			if (Payments.Current.DocType == PayrollType.Regular)
			{
				int nbrOfDaysInPeriod = (Payments.Current.EndDate - Payments.Current.StartDate).Value.Days + 1;
				IEnumerable<PRPaymentPTOBank> ptoBanks = PaymentPTOBanks.Select().FirstTableItems.Where(x => x.AccrualMethod == PTOAccrualMethod.TotalHoursPerYear && x.IsActive == true);
				foreach (PRPaymentPTOBank bank in ptoBanks)
				{
					DateTime? bankEffectiveStartForPayment = bank.EffectiveStartDate.Value.Year > Payments.Current.StartDate.Value.Year ?
						new DateTime(bank.EffectiveStartDate.Value.Year, 1, 1) : (bank.EffectiveStartDate < Payments.Current.StartDate ? Payments.Current.StartDate : bank.EffectiveStartDate);
					DateTime? bankEffectiveEndForPayment = bank?.EffectiveEndDate?.Date < Payments.Current.EndDate.Value.Date ?
						bank.EffectiveEndDate : Payments.Current.EndDate;
					bank.AccruingDays = ((bankEffectiveEndForPayment - bankEffectiveStartForPayment).Value.Days + 1);
					bank.DaysInPeriod = nbrOfDaysInPeriod;

					bank.AccrualAmount += (bank.HoursPerYear / bank.NbrOfPayPeriods * bank.EffectiveCoefficient) ?? 0;

					IPTOBank sourceBank = PTOHelper.GetBankSettings(this, bank.BankID, Payments.Current.EmployeeID.Value, bank.EffectiveStartDate.Value);
					if (sourceBank?.StartDate == null)
					{
						throw new PXException(Messages.BankNotFound, bank.BankID);
					}
					this.RegularizeAccrualAmount(sourceBank, bank, bank.EffectiveStartDate.Value);

					PaymentPTOBanks.Update(bank);
				}
			}
		}

		protected virtual void CalculatePercentagePTO(PRPaymentPTOBank paymentBank, PREarningDetail earningDetail, IPTOBank sourceBank, out decimal accrualHours, out decimal accrualMoney)
		{
			accrualHours = 0;
			accrualMoney = 0;
			var earningType = (EPEarningType)PREarningTypeSelectorAttribute.Select<PREarningDetail.typeCD>(PaymentEarningDetails.Cache, earningDetail);
			PREarningType prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(earningType);
			decimal accuralPct = paymentBank.AccrualRate.GetValueOrDefault() / 100;
			if (prEarningType?.AccruePTO == true && paymentBank.AccrualMethod == PTOAccrualMethod.Percentage && earningDetail.IsPayingSettlement == false)
			{
				accrualHours = (accuralPct * earningDetail.Hours) ?? 0;
				if (sourceBank.CreateFinancialTransaction != true || sourceBank.DisbursingType != PTODisbursingType.AverageRate)
				{
					accrualMoney = earningDetail.Amount.GetValueOrDefault() * accuralPct;
				}
				else
				{
					// If the earning is a PTO disbursement with Average Rate disbursing type,
					// the accrual is calculated with the default rate, not the actual rate, to avoid
					// circular logic.
					PREarningDetail copy = PaymentEarningDetails.Cache.CreateCopy(earningDetail) as PREarningDetail;
					copy.ManualRate = false;
					PayRateAttribute.SetRate(PaymentEarningDetails.Cache, copy);
					accrualMoney = copy.Amount.GetValueOrDefault() * accuralPct;
				}

				if (!PaymentsToProcess[Payments.Current].PTOAccrualMoneySplitByEarning.ContainsKey(paymentBank.BankID))
				{
					PaymentsToProcess[Payments.Current].PTOAccrualMoneySplitByEarning[paymentBank.BankID] = new PTOAccrualSplits();
				}
				PaymentsToProcess[Payments.Current].PTOAccrualMoneySplitByEarning[paymentBank.BankID][earningDetail.RecordID] = accrualMoney;
			}
		}

		protected virtual void ResetPTOInfo()
		{
			//Delete lines that were paying PTO for settlement
			PaymentEarningDetails.Select().FirstTableItems
				.Where(x => x.IsPayingSettlement == true)
				.ForEach(x => PaymentEarningDetails.Delete(x));

			IEnumerable<PRPaymentPTOBank> banks = PaymentPTOBanks.Select().FirstTableItems;
			IEnumerable<PRPaymentPTOBank> lastEffectiveBanks = PTOHelper.GetLastEffectiveBanks(banks);
			foreach (PRPaymentPTOBank bankToReset in banks)
			{
				bankToReset.AccrualAmount = 0;
				bankToReset.AccrualMoney = 0;
				bankToReset.DisbursementAmount = 0;
				bankToReset.DisbursementMoney = 0;
				bankToReset.CarryoverAmount = 0;
				bankToReset.CarryoverMoney = 0;
				bankToReset.FrontLoadingAmount = 0;
				bankToReset.PaidCarryoverAmount = 0;
				bankToReset.SettlementDiscardAmount = 0;
				bankToReset.AccruingHours = 0;
				bankToReset.ProcessedFrontLoading = false;
				bankToReset.ProcessedCarryover = false;
				bankToReset.ProcessedPaidCarryover = false;
				bankToReset.AccumulatedAmount = null;
				bankToReset.AccumulatedMoney = null;
				bankToReset.UsedAmount = null;
				bankToReset.UsedMoney = null;
				bankToReset.AvailableAmount = null;
				bankToReset.AvailableMoney = null;

				IPTOBank bankInfo = PTOHelper.GetBankSettings(this, bankToReset.BankID, Payments.Current.EmployeeID.Value, Payments.Current.EndDate.Value);
				if (bankInfo?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, bankToReset.BankID);
				}
				bankToReset.AccrualLimit = bankInfo.AccrualLimit;

				if (lastEffectiveBanks.Contains(bankToReset))
				{
					PTOHelper.GetPTOBankYear(Payments.Current.EndDate.GetValueOrDefault(), bankInfo.StartDate.GetValueOrDefault(), out DateTime yearStartDate, out DateTime yearEndDate);
					PTOHelper.PTOHistoricalAmounts history = PTOHelper.GetPTOHistory(this, Payments.Current.EndDate.Value, Payments.Current.EmployeeID.Value, bankInfo);

					bankToReset.AccumulatedAmount = history.AccumulatedHours;
					bankToReset.AccumulatedMoney = history.AccumulatedMoney;
					bankToReset.UsedAmount = history.UsedHours;
					bankToReset.UsedMoney = history.UsedMoney;
					bankToReset.AvailableAmount = history.AvailableHours;
					bankToReset.AvailableMoney = history.AvailableMoney;
				}

				PaymentPTOBanks.Update(bankToReset);
			}
		}

		protected virtual void AdjustPTODisbursementRateAndAcctSub(IPTOBank bank, PREarningDetail row, decimal availableHours, decimal availableMoney)
		{
			if (Payments.Current.CountryID != LocationConstants.CanadaCountryCode || bank.CreateFinancialTransaction != true)
			{
				return;
			}

			if (bank.DisbursingType == PTODisbursingType.AverageRate)
			{
				row.Rate = availableHours <= 0 || availableMoney <= 0 ? 0 : availableMoney / availableHours;
			}

			PTODisbursementEarningDetail disbursementEarningDetail = new PTODisbursementEarningDetail() { PTOBankID = bank.BankID };
			Caches[typeof(PTODisbursementEarningDetail)].SetDefaultExt<PTODisbursementEarningDetail.liabilityAccountID>(disbursementEarningDetail);
			Caches[typeof(PTODisbursementEarningDetail)].SetDefaultExt<PTODisbursementEarningDetail.liabilitySubID>(disbursementEarningDetail);

			row.AccountID = disbursementEarningDetail.LiabilityAccountID;
			row.SubID = disbursementEarningDetail.LiabilitySubID;
			PaymentEarningDetails.Update(row);
		}

		protected virtual void InsertPaidCarryoverEarningsProc()
		{
			foreach (var row in PaymentPTOBanks.Select().FirstTableItems.Where(x => x.ProcessedPaidCarryover == true))
			{
				row.ProcessedPaidCarryover = false;
				PaymentPTOBanks.Update(row);
			}

			if (Payments.Current.AutoPayCarryover == false)
			{
				return;
			}

			foreach (IPTOBank bank in PTOHelper.GetEmployeeBanks(this, Payments.Current))
			{
				PTOHelper.PTOYearSummary ptoYearSummary = PTOHelper.GetPTOYearSummary(this, Payments.Current.EndDate.Value, Payments.Current.EmployeeID.Value, bank);
				DateTime dateToBePaid = ptoYearSummary.StartDate.AddMonths(bank.CarryoverPayMonthLimit.GetValueOrDefault());
				if (ptoYearSummary.ProcessedPaidCarryover == false
					&& Payments.Current.DocType == PayrollType.Regular
					&& bank.IsActive == true
					&& bank.CarryoverType == CarryoverType.PaidOnTimeLimit
					&& (bank.CarryoverPayMonthLimit.GetValueOrDefault() == 12
						|| dateToBePaid <= Payments.Current.EndDate.Value))
				{
					PRPTOBank ptoBank = PXSelectorAttribute.Select(Caches[bank.GetType()], bank, nameof(bank.BankID)) as PRPTOBank;
					bool isFirstBankProcessOfPTOYear = PTOHelper.IsFirstBankProcessOfPTOYear(this, ptoYearSummary.StartDate, Payments.Current.EmployeeID.Value, bank, Payments.Current);

					decimal carryoverAmount = ptoYearSummary.CarryoverAmount.GetValueOrDefault();
					//For 12 months limit, look at last year instead
					if (bank.CarryoverPayMonthLimit.GetValueOrDefault() == 12)
					{
						ptoYearSummary = PTOHelper.GetPTOYearSummary(this, Payments.Current.EndDate.Value.AddYears(-1), Payments.Current.EmployeeID.Value, bank);
						carryoverAmount = ptoYearSummary.CarryoverAmount.GetValueOrDefault();
					}
					//For first paycheck, calculate what carryover would be
					else if (isFirstBankProcessOfPTOYear)
					{
						carryoverAmount = PTOHelper.CalculateHoursToCarryover(this, Payments.Current.EmployeeID, Payments.Current, bank, ptoYearSummary.StartDate, ptoYearSummary.EndDate);
					}

					decimal usedOnPaycheck = PaymentEarningDetails.Select().FirstTableItems.Where(x => x.TypeCD == ptoBank.EarningTypeCD).Sum(x => x.Hours.GetValueOrDefault());
					decimal carryoverLeftover = carryoverAmount - ptoYearSummary.TotalDecreasedHours - usedOnPaycheck;
					if (carryoverLeftover <= 0)
					{
						return;
					}

					if (dateToBePaid < Payments.Current.StartDate || dateToBePaid > Payments.Current.EndDate)
					{
						dateToBePaid = Payments.Current.EndDate.Value;
					}
					var detail = new PREarningDetail();
					detail.EmployeeID = Payments.Current.EmployeeID;
					detail.PaymentDocType = Payments.Current.DocType;
					detail.PaymentRefNbr = Payments.Current.RefNbr;
					detail.BatchNbr = Payments.Current.PayBatchNbr;
					detail.TypeCD = ptoBank.EarningTypeCD;
					detail.Hours = carryoverLeftover;
					detail.Date = dateToBePaid;
					detail.IsPayingCarryover = true;
					detail.IsRegularRate = false;
					detail = PaymentEarningDetails.Insert(detail);

					PRPaymentPTOBank row = PTOHelper.GetEffectivePaymentBank(PaymentPTOBanks.Select().FirstTableItems, dateToBePaid, bank.BankID);
					if (row != null)
					{
						row.ProcessedPaidCarryover = true;
						PaymentPTOBanks.Update(row);
					}
				}
			}
		}

		public PTOHelper.PTOYearSummary GetPTOYearAndPaymentSummary(PXGraph graph, DateTime targetDate, int employeeID, IPTOBank bank)
		{
			PTOHelper.PTOYearSummary yearSummary = PTOHelper.GetPTOYearSummary(graph, targetDate, employeeID, bank);
			var history = new List<PRPaymentPTOBank>();
			foreach (PRPaymentPTOBank paymentBank in PaymentPTOBanks.Select().Where(x => ((PRPaymentPTOBank)x).BankID == bank.BankID))
			{
				if (yearSummary.StartDate <= paymentBank.EffectiveStartDate && paymentBank.EffectiveStartDate < yearSummary.EndDate)
				{
					history.Add(paymentBank);
				}
			}

			yearSummary.AccrualAmount += history.Sum(x => x.AccrualAmount.GetValueOrDefault());
			yearSummary.DisbursementAmount += history.Sum(x => x.DisbursementAmount.GetValueOrDefault());
			yearSummary.FrontLoadingAmount += history.Sum(x => x.FrontLoadingAmount.GetValueOrDefault());
			yearSummary.CarryoverAmount += history.Sum(x => x.CarryoverAmount.GetValueOrDefault());
			yearSummary.PaidCarryoverAmount += history.Sum(x => x.PaidCarryoverAmount.GetValueOrDefault());
			yearSummary.SettlementDiscardAmount += history.Sum(x => x.SettlementDiscardAmount.GetValueOrDefault());

			yearSummary.ProcessedFrontLoading |= history.Any(x => x.ProcessedFrontLoading == true);
			yearSummary.ProcessedCarryover |= history.Any(x => x.ProcessedCarryover == true);
			yearSummary.ProcessedPaidCarryover |= history.Any(x => x.ProcessedPaidCarryover == true);

			return yearSummary;
		}


		public void InsertSettlementEarnings(PRPayment payment)
		{
			if (Payments.Current.DocType == PayrollType.Final)
			{
				foreach (var paymentBank in PTOHelper.GetLastEffectiveBanks(PaymentPTOBanks.Select().Select(x => (PRPaymentPTOBank)x)))
				{
					var ptoBank = PRPTOBank.PK.Find(this, paymentBank.BankID);
					if (ptoBank.SettlementBalanceType == SettlementBalanceType.Pay)
					{
						var bankSettings = PTOHelper.GetBankSettings(this, paymentBank.BankID, Payments.Current.EmployeeID.Value, paymentBank.EffectiveStartDate.Value);
						var hours = GetPTOYearAndPaymentSummary(this, paymentBank.EffectiveStartDate.Value, payment.EmployeeID.Value, bankSettings).BalanceHours;
						if (hours == 0)
						{
							continue;
						}

						var detail = new PREarningDetail();
						detail.EmployeeID = Payments.Current.EmployeeID;
						detail.PaymentDocType = Payments.Current.DocType;
						detail.PaymentRefNbr = Payments.Current.RefNbr;
						detail.BatchNbr = Payments.Current.PayBatchNbr;
						detail.TypeCD = paymentBank.EarningTypeCD;
						detail.Hours = hours;
						detail.Date = Payments.Current.TransactionDate < Payments.Current.EndDate ? Payments.Current.TransactionDate : Payments.Current.EndDate;
						detail.IsPayingSettlement = true;
						detail.IsRegularRate = false;
						detail = PaymentEarningDetails.Insert(detail);

						paymentBank.DisbursementAmount += hours;
						PaymentPTOBanks.Update(paymentBank);
					}
				}
			}
		}

		#endregion

		#region Helpers
		/// <summary>
		/// Recalculates taxes YTD amounts according to current document date
		/// </summary>
		/// <param name="row"></param>
		protected virtual void UpdateTax(PRPaymentTax row)
		{
			var result = (PRYtdTaxes)YTDTaxes.Select(row.TaxID, Payments.Current.TransactionDate.Value.Year);
			row.YtdAmount = result?.Amount ?? 0;
		}

		protected virtual IEnumerable<PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>> GetDeductions(
			bool? taxable = null,
			string codeCD = null,
			int? codeID = null,
			bool? contributesToGrossCalculation = null)
		{
			foreach (var group in Deductions.Select()
				.ToList()
				.Select(x => (PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct>)x)
				.Where(x => (taxable == null || ((PRDeductCode)x).AffectsTaxes == taxable) &&
					(codeCD == null || ((PRDeductCode)x).CodeCD == codeCD) &&
					(codeID == null || ((PRDeductCode)x).CodeID == codeID) &&
					(contributesToGrossCalculation == null || ((PRDeductCode)x).ContributesToGrossCalculation == contributesToGrossCalculation))
				.GroupBy(x => new { ((PRDeductCode)x).CodeID, ((PRPaymentDeduct)x).Source }))
			{
				if (group.Key.Source == DeductionSourceListAttribute.EmployeeSettings)
				{
					PXResult<PRPaymentDeduct, PRDeductCode, PREmployeeDeduct> groupResult = group.First();
					PREmployeeDeduct employeeDeduct = groupResult;

					if (employeeDeduct.CodeID != null)
					{
						var transactionDate = Payments.Current.TransactionDate;
						var result = group.Where(x => ((PREmployeeDeduct)x).IsActive == true && transactionDate >= ((PREmployeeDeduct)x).StartDate &&
							(((PREmployeeDeduct)x).EndDate == null || ((PREmployeeDeduct)x).EndDate >= transactionDate)).OrderByDescending(x => ((PREmployeeDeduct)x).StartDate).FirstOrDefault();

						if (result != null)
						{
							yield return result;
						}
						else
						{
							if (taxable == true)
							{
								yield return group.OrderByDescending(x => ((PREmployeeDeduct)x).StartDate).First();
							}
						}
					}
					else
					{
						yield return group.OrderByDescending(x => ((PREmployeeDeduct)x).StartDate).First();
					}
				}
				else
				{
				yield return group.OrderByDescending(x => ((PREmployeeDeduct)x).StartDate).First();
			}
		}
		}

		protected string GetDedMaxFreqTypeValue(PRDeductCode deductionCode, PREmployeeDeduct employeeDeduction)
		{
			return employeeDeduction != null && employeeDeduction.DedUseDflt == false ? employeeDeduction.DedMaxFreqType : deductionCode.DedMaxFreqType;
		}

		protected string GetCntMaxFreqTypeValue(PRDeductCode deductionCode, PREmployeeDeduct employeeDeduction)
		{
			return employeeDeduction != null && employeeDeduction.CntUseDflt == false ? employeeDeduction.CntMaxFreqType : deductionCode.CntMaxFreqType;
		}

		protected virtual void RecordProjectPackageNominalAmounts(PRDeductionAndBenefitProjectPackage package, PackageDedBenCalculation calculation)
		{
			var packageKey = new ProjectDedBenPackageKey(package.ProjectID, package.DeductionAndBenefitCodeID, package.LaborItemID);
			if (!PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.ContainsKey(packageKey))
			{
				PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts[packageKey] = calculation;
			}
			else
			{
				PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts[packageKey].Add(calculation);
			}
		}

		protected virtual void RecordUnionPackageNominalAmounts(PRDeductionAndBenefitUnionPackage package, PackageDedBenCalculation calculation)
		{
			var packageKey = new UnionDedBenPackageKey(package.UnionID, package.DeductionAndBenefitCodeID, package.LaborItemID);
			if (!PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.ContainsKey(packageKey))
			{
				PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts[packageKey] = calculation;
			}
			else
			{
				PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts[packageKey].Add(calculation);
			}
		}

		protected virtual void InsertProjectPackageDetails()
		{
			if (PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.Any())
			{
				foreach (PRPaymentDeduct paymentDeduct in Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.CertifiedProject))
				{
					IEnumerable<KeyValuePair<ProjectDedBenPackageKey, PackageDedBenCalculation>> nominals =
						PaymentsToProcess[Payments.Current].NominalProjectPackageAmounts.Where(x => x.Key.DeductCodeID == paymentDeduct.CodeID);
					decimal totalNominalDeductionAmount = nominals.Sum(x => x.Value.DeductionAmount.GetValueOrDefault());
					decimal totalActualDeductionAmount = paymentDeduct.DedAmount.GetValueOrDefault();
					decimal deductionFactor = totalNominalDeductionAmount > 0 ? totalActualDeductionAmount / totalNominalDeductionAmount : 0m;
					decimal totalNominalBenefitAmount = nominals.Sum(x => x.Value.BenefitAmount.GetValueOrDefault());
					decimal totalActualBenefitAmount = paymentDeduct.CntAmount.GetValueOrDefault();
					decimal benefitFactor = totalNominalBenefitAmount > 0 ? totalActualBenefitAmount / totalNominalBenefitAmount : 0m;

					foreach (KeyValuePair<ProjectDedBenPackageKey, PackageDedBenCalculation> kvp in nominals)
					{
						if (kvp.Value.DeductionAmount != 0 && deductionFactor != 0 ||
							kvp.Value.BenefitAmount != 0 && benefitFactor != 0)
						{
							List<PRPaymentProjectPackageDeduct> details = new List<PRPaymentProjectPackageDeduct>();
							if (paymentDeduct.ContribType == ContributionType.BothDeductionAndContribution && kvp.Value.SameApplicableForDedAndBen)
							{
								details.Add(new PRPaymentProjectPackageDeduct()
								{
									ProjectID = kvp.Key.ProjectID,
									LaborItemID = kvp.Key.LaborItemID,
									DeductCodeID = kvp.Key.DeductCodeID,
									RegularWageBaseHours = kvp.Value.RegularHoursForDed,
									OvertimeWageBaseHours = kvp.Value.OvertimeHoursForDed,
									RegularWageBaseAmount = kvp.Value.RegularHoursAmountForDed,
									OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForDed,
									DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
									BenefitAmount = kvp.Value.BenefitAmount * benefitFactor,
									ContribType = ContributionType.BothDeductionAndContribution
								});
							}
							else
							{
								if (paymentDeduct.ContribType != ContributionType.EmployerContribution && kvp.Value.HasAnyApplicableForDed)
								{
									details.Add(new PRPaymentProjectPackageDeduct()
									{
										ProjectID = kvp.Key.ProjectID,
										LaborItemID = kvp.Key.LaborItemID,
										DeductCodeID = kvp.Key.DeductCodeID,
										RegularWageBaseHours = kvp.Value.RegularHoursForDed,
										OvertimeWageBaseHours = kvp.Value.OvertimeHoursForDed,
										RegularWageBaseAmount = kvp.Value.RegularHoursAmountForDed,
										OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForDed,
										DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
										BenefitAmount = 0,
										ContribType = ContributionType.EmployeeDeduction
									});
								}

								if (paymentDeduct.ContribType != ContributionType.EmployeeDeduction && kvp.Value.HasAnyApplicableForBen)
								{
									details.Add(new PRPaymentProjectPackageDeduct()
									{
										ProjectID = kvp.Key.ProjectID,
										LaborItemID = kvp.Key.LaborItemID,
										DeductCodeID = kvp.Key.DeductCodeID,
										RegularWageBaseHours = kvp.Value.RegularHoursForBen,
										OvertimeWageBaseHours = kvp.Value.OvertimeHoursForBen,
										RegularWageBaseAmount = kvp.Value.RegularHoursAmountForBen,
										OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForBen,
										DeductionAmount = 0,
										BenefitAmount = kvp.Value.BenefitAmount * benefitFactor,
										ContribType = ContributionType.EmployerContribution
									});
								}
							}

							details.ForEach(detail => ProjectPackageDeductions.Insert(detail));
						}
					}
				}
			}
		}

		protected virtual void InsertUnionPackageDetails()
		{
			if (PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.Any())
			{
				foreach (PRPaymentDeduct paymentDeduct in Deductions.Select().FirstTableItems.Where(x => x.Source == PaymentDeductionSourceAttribute.Union))
				{
					IEnumerable<KeyValuePair<UnionDedBenPackageKey, PackageDedBenCalculation>> nominals =
						PaymentsToProcess[Payments.Current].NominalUnionPackageAmounts.Where(x => x.Key.DeductCodeID == paymentDeduct.CodeID);
					decimal totalNominalDeductionAmount = nominals.Sum(x => x.Value.DeductionAmount.GetValueOrDefault());
					decimal totalActualDeductionAmount = paymentDeduct.DedAmount.GetValueOrDefault();
					decimal deductionFactor = totalNominalDeductionAmount > 0 ? totalActualDeductionAmount / totalNominalDeductionAmount : 0m;
					decimal totalNominalBenefitAmount = nominals.Sum(x => x.Value.BenefitAmount.GetValueOrDefault());
					decimal totalActualBenefitAmount = paymentDeduct.CntAmount.GetValueOrDefault();
					decimal benefitFactor = totalNominalBenefitAmount > 0 ? totalActualBenefitAmount / totalNominalBenefitAmount : 0m;

					foreach (KeyValuePair<UnionDedBenPackageKey, PackageDedBenCalculation> kvp in nominals)
					{
						if (kvp.Value.DeductionAmount != 0 && deductionFactor != 0 ||
							kvp.Value.BenefitAmount != 0 && benefitFactor != 0)
						{
							List<PRPaymentUnionPackageDeduct> details = new List<PRPaymentUnionPackageDeduct>();
							if (paymentDeduct.ContribType == ContributionType.BothDeductionAndContribution && kvp.Value.SameApplicableForDedAndBen)
							{
								details.Add(new PRPaymentUnionPackageDeduct()
								{
									UnionID = kvp.Key.UnionID,
									LaborItemID = kvp.Key.LaborItemID,
									DeductCodeID = kvp.Key.DeductCodeID,
									RegularWageBaseHours = kvp.Value.RegularHoursForDed,
									OvertimeWageBaseHours = kvp.Value.OvertimeHoursForDed,
									RegularWageBaseAmount = kvp.Value.RegularHoursAmountForDed,
									OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForDed,
									DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
									BenefitAmount = kvp.Value.BenefitAmount * benefitFactor,
									ContribType = ContributionType.BothDeductionAndContribution
								});
							}
							else
							{
								if (paymentDeduct.ContribType != ContributionType.EmployerContribution && kvp.Value.HasAnyApplicableForDed)
								{
									details.Add(new PRPaymentUnionPackageDeduct()
									{
										UnionID = kvp.Key.UnionID,
										LaborItemID = kvp.Key.LaborItemID,
										DeductCodeID = kvp.Key.DeductCodeID,
										RegularWageBaseHours = kvp.Value.RegularHoursForDed,
										OvertimeWageBaseHours = kvp.Value.OvertimeHoursForDed,
										RegularWageBaseAmount = kvp.Value.RegularHoursAmountForDed,
										OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForDed,
										DeductionAmount = kvp.Value.DeductionAmount * deductionFactor,
										BenefitAmount = 0,
										ContribType = ContributionType.EmployeeDeduction
									});
								}

								if (paymentDeduct.ContribType != ContributionType.EmployeeDeduction && kvp.Value.HasAnyApplicableForBen)
								{
									details.Add(new PRPaymentUnionPackageDeduct()
									{
										UnionID = kvp.Key.UnionID,
										LaborItemID = kvp.Key.LaborItemID,
										DeductCodeID = kvp.Key.DeductCodeID,
										RegularWageBaseHours = kvp.Value.RegularHoursForBen,
										OvertimeWageBaseHours = kvp.Value.OvertimeHoursForBen,
										RegularWageBaseAmount = kvp.Value.RegularHoursAmountForBen,
										OvertimeWageBaseAmount = kvp.Value.OvertimeHoursAmountForBen,
										DeductionAmount = 0,
										BenefitAmount = kvp.Value.BenefitAmount * benefitFactor,
										ContribType = ContributionType.EmployerContribution
									});
								}
							}

							details.ForEach(detail => UnionPackageDeductions.Insert(detail));
						}
					}
				}
			}
		}

		protected virtual void ProcessPTOSettlement(PRPayment payment)
		{
			var employeePTOBanks = PTOHelper.GetEmployeeBanks(this, payment);
			foreach (IPTOBank ptoBankSettings in employeePTOBanks)
			{
				var ptoBank = PRPTOBank.PK.Find(this, ptoBankSettings.BankID);
				switch (ptoBank.SettlementBalanceType)
				{
					case SettlementBalanceType.Discard:
						ProcessSettlementPTODiscard(payment, ptoBankSettings);
						break;
					case SettlementBalanceType.Pay:
					case SettlementBalanceType.Keep:
						break;
				}
			}
		}

		protected virtual void ProcessSettlementPTODiscard(PRPayment payment, IPTOBank ptoBankSettings)
		{
			var paymentPTOBank = PTOHelper.GetLastEffectiveBanks(PaymentPTOBanks.Select().Select(x => (PRPaymentPTOBank)x)).Single(x => x.BankID == ptoBankSettings.BankID);
			paymentPTOBank.SettlementDiscardAmount = GetPTOYearAndPaymentSummary(this, paymentPTOBank.EffectiveStartDate.Value, payment.EmployeeID.Value, ptoBankSettings).BalanceHours;
			PaymentPTOBanks.Update(paymentPTOBank);
		}

		private decimal GetDedBenApplicableAmount(PRDeductCode deductCode, string contributionType)
		{
			return _CalculationUtils.Value.GetDedBenApplicableAmount(
				this,
				deductCode,
				contributionType,
				PaymentEarningDetails.Select().FirstTableItems,
				PaymentsToProcess[Payments.Current].TaxesSplitByEarning,
				PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning);
		}

		private decimal GetDedBenApplicableAmount(PRDeductCode deductCode, string contributionType, PREarningDetail earning)
		{
			decimal totalEarningsInPaycheck = Payments.Current.TotalEarnings.GetValueOrDefault();
			decimal payableBenefitContributionPortion = 0;
			if (totalEarningsInPaycheck != 0)
			{
				// The pro-rated portion of payable benefits contributing to gross calculation
				payableBenefitContributionPortion =
					PaymentsToProcess[Payments.Current].PayableBenefitContributingAmount * earning.Amount.GetValueOrDefault() / totalEarningsInPaycheck;
			}

			return payableBenefitContributionPortion + _CalculationUtils.Value.GetDedBenApplicableAmount(
				this,
				deductCode,
				contributionType,
				earning,
				PaymentsToProcess[Payments.Current].TaxesSplitByEarning,
				PaymentsToProcess[Payments.Current].TaxableDeductionsAndBenefitsSplitByEarning);
		}

		private decimal GetDedBenApplicableHours(PRDeductCode deductCode, string contributionType)
		{
			return _CalculationUtils.Value.GetDedBenApplicableHours(
				this,
				deductCode,
				contributionType,
				PaymentEarningDetails.Select().FirstTableItems);
		}

		private decimal GetDedBenApplicableHours(PRDeductCode deductCode, string contributionType, PREarningDetail earning)
		{
			return _CalculationUtils.Value.GetDedBenApplicableHours(
				this,
				deductCode,
				contributionType,
				earning);
		}

		private bool IsInTheMaxInsurableWageDateInterval(DateTime? wccDate, DateTime? paymentTransactionDate)
		{
			DateTime? currentDate = Payments.Current.TransactionDate;

			if (currentDate.Value.DayOfYear < wccDate.Value.DayOfYear)
			{
				return wccDate.Value.AddYears(currentDate.Value.Year - wccDate.Value.Year - 1) <= paymentTransactionDate && paymentTransactionDate <= wccDate.Value.AddYears(currentDate.Value.Year - wccDate.Value.Year);
			}

			return wccDate.Value.AddYears(currentDate.Value.Year - wccDate.Value.Year) <= paymentTransactionDate && paymentTransactionDate <= wccDate.Value.AddYears(currentDate.Value.Year - wccDate.Value.Year + 1);
		}

		private Dictionary<WCPremiumKey, decimal> GetYtdWCWageBase(string contributionType, Dictionary<WCPremiumKey, PRWorkCompensationMaximumInsurableWage> maximumInsurableWages)
		{
			return YtdWCPremiums.Select(contributionType)
				.Select(x => (PXResult<PRPaymentWCPremium, PRDeductCode, PRPayment>)x)
				.Where(x => maximumInsurableWages.ContainsKey(new WCPremiumKey(((PRPaymentWCPremium)x).WorkCodeID, ((PRDeductCode)x).State, null)))
				.GroupBy(x => new { WorkCodeID = ((PRPaymentWCPremium)x).WorkCodeID, State = ((PRDeductCode)x).State })
				.ToDictionary(k => new WCPremiumKey(k.Key.WorkCodeID, k.Key.State, null),
							  v => v.Where(x => IsInTheMaxInsurableWageDateInterval(maximumInsurableWages[new WCPremiumKey(v.Key.WorkCodeID, v.Key.State, null)].EffectiveDate, ((PRPayment)x).TransactionDate) == true)
									.Sum(x => ((PRPaymentWCPremium)x).WageBaseAmount.GetValueOrDefault()));
		}
		#endregion Helpers

		#region Avoid breaking changes in 2023R1
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		protected virtual void CalculateCarryoverAndFrontLoading()
		{
			IEnumerable<PRPaymentPTOBank> ptoBanks = PaymentPTOBanks.Select().FirstTableItems.Where(x => x.IsActive == true);
			foreach (PRPaymentPTOBank effectiveBank in ptoBanks)
			{
				IPTOBank sourceBank = PTOHelper.GetBankSettings(this, effectiveBank.BankID, Payments.Current.EmployeeID.Value, effectiveBank.EffectiveStartDate.Value);
				if (sourceBank?.StartDate == null)
				{
					throw new PXException(Messages.BankNotFound, effectiveBank.BankID);
				}

				if (Payments.Current.DocType == PayrollType.Regular)
				{
					this.ProcessCarryOverAndFrontLoading(sourceBank, effectiveBank, effectiveBank.EffectiveStartDate.Value);
				}

				PaymentPTOBanks.Update(effectiveBank);
			}
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R1)]
		protected virtual void ProcessCarryOverAndFrontLoading(IPTOBank sourceBank, PRPaymentPTOBank effectiveBank, DateTime targetDate)
		{
			PTOHelper.PTOYearSummary yearSummary = GetPTOYearAndPaymentSummary(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank);

			//When the user adds a new PTO bank in the Employee Payroll Settings, and we are processing it at this point, check if a PTO bank with the same effective date has already processed front loading
			var results = PTOHelper.EmployeePTOHistory.Select(this, targetDate, Payments.Current.EmployeeID.Value, sourceBank.PTOYearStartDate.Value, sourceBank.BankID);
			var history = results.Select(x => (PXResult<PRPaymentPTOBank, PRPayment>)x).Where(x => ((PRPaymentPTOBank)x).EffectiveStartDate == effectiveBank.EffectiveStartDate).ToList();
			yearSummary.ProcessedFrontLoading = history.Any(x => ((PRPaymentPTOBank)x).ProcessedFrontLoading == true && ((PRPayment)x).Voided == false && ((PRPayment)x).DocType != PayrollType.VoidCheck);

			if (yearSummary.ProcessedCarryover == false)
			{
				effectiveBank.CarryoverAmount = PTOHelper.CalculateHoursToCarryover(this, Payments.Current.EmployeeID, Payments.Current, sourceBank, yearSummary.StartDate, yearSummary.EndDate);

			}
		}
		#endregion
	}
}
