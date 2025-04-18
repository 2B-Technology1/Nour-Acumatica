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
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PR.AUF;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using EPEmployee = PX.Objects.EP.EPEmployee;

namespace PX.Objects.PR
{
	[PXHidden]
	public class PRAufGenerationEngine : PXGraph<PRAufGenerationEngine>
	{
		private Dictionary<string, SymmetryToAatrixTaxMapping> _TaxIDMapping;
		private Dictionary<string, int> _WCMapping;
		private List<PimRecord> _PayrollItemMapping;
		private List<CjiRecord> _JobItemMapping;
		private List<string> _PaymentPeriods = new List<string>();

		protected virtual byte[] GenerateAufInternal(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			using (var prTypeCache = new PRTypeSelectorBaseAttribute.PRTypeSelectorCache(new Type[] { typeof(PRWage), typeof(PRBenefit) }))
			{
				SetCurrents(orgBAccountID, reportToGenerate);

				_WCMapping = new Dictionary<string, int>();

				DatRecord datRecord = CreateDateRecord();

				// First create reference items that are used by company or employee records
				_TaxIDMapping = MapTaxes();
				_PayrollItemMapping = CreatePimRecords(datRecord).ToList();
				if (ShouldGenerateCertifiedRecords())
				{
					_JobItemMapping = CreateCompanyJobRecords(datRecord).ToList();
				}

				List<EmpRecord> empList = CreateEmpRecords(datRecord).ToList();

				AufFormatter formatter = new AufFormatter()
				{
					Dat = datRecord,
					PimList = _PayrollItemMapping.OrderBy(x => x.PimID).ToList(),
					Cmp = CreateCompanyRecord(datRecord, empList),
					EmpList = empList
				};

				return formatter.GenerateAufFile();
			}
		}

		static public void GenerateAuf(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			GenerateAufOperationInfo customInfo = PXLongOperation.GetCustomInfo() as GenerateAufOperationInfo;
			try
			{
				PRAufGenerationEngine instance = PXGraph.CreateInstance<PRAufGenerationEngine>();
				customInfo.AufContent = instance.GenerateAufInternal(orgBAccountID, reportToGenerate);
				
				// Don't complete the long operation until Aatrix user has been granted permission
				customInfo.PermissionGrantedEvent.WaitOne();
			}
			catch (Exception e)
			{
				customInfo.Error = e;
			}
		}

		#region Views
		public SelectFrom<PRGovernmentReport>.View Report;

		public SelectFrom<BAccountR>
			.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View OrgAccount;

		public SelectFrom<Address>
			.Where<Address.addressID.IsEqual<BAccountR.defAddressID.FromCurrent>>.View OrgAddress;

		public SelectFrom<Contact>
			.Where<Contact.contactID.IsEqual<BAccountR.defContactID.FromCurrent>>.View OrgContact;

		public SelectFrom<Country>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<BAccountR.defAddressID.FromCurrent>>
			.Where<Country.countryID.IsEqual<Address.countryID>>.View OrgCountry;

		public SelectFrom<EPEmployee>
			.InnerJoin<Branch>.On<Branch.bAccountID.IsEqual<EPEmployee.parentBAccountID>>
			.Where<Where<Branch.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>.View TotalEmployees;

		public SelectFrom<PRCompanyTaxAttribute>
			.Where<PRCompanyTaxAttribute.aatrixMapping.IsEqual<P.AsInt>
				.And<MatchPRCountry<PRCompanyTaxAttribute.countryID>>>.View CompanyTaxAttribute;

		public SelectFrom<EPEmployee>
			.InnerJoin<Contact>.On<Contact.contactID.IsEqual<EPEmployee.defContactID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<EPEmployee.defAddressID>>
			.LeftJoin<Country>.On<Country.countryID.IsEqual<Address.countryID>>
			.LeftJoin<PREmployeeDirectDeposit>.On<PREmployeeDirectDeposit.bAccountID.IsEqual<EPEmployee.bAccountID>>
			.LeftJoin<PRPayment>.On<PRPayment.employeeID.IsEqual<EPEmployee.bAccountID>>
			.LeftJoin<PRAcaEmployeeMonthlyInformation>.On<PRAcaEmployeeMonthlyInformation.employeeID.IsEqual<EPEmployee.bAccountID>>
			.Where<Brackets<PRPayment.transactionDate.IsGreaterEqual<P.AsDateTime.UTC>
				.And<PRPayment.transactionDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<PRGovernmentReport.reportType.FromCurrent.IsNotEqual<certifiedReportType>>
				.Or<PRPayment.startDate.IsGreaterEqual<P.AsDateTime.UTC>
					.And<PRPayment.endDate.IsLessEqual<P.AsDateTime.UTC>>
					.And<PRGovernmentReport.reportType.FromCurrent.IsEqual<certifiedReportType>>>>
				.And<PRPayment.released.IsEqual<True>>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>
				.And<PRPayment.grossAmount.IsNotEqual<decimal0>
					.Or<PRPayment.dedAmount.IsNotEqual<decimal0>>
					.Or<PRPayment.taxAmount.IsNotEqual<decimal0>>
					.Or<PRPayment.benefitAmount.IsNotEqual<decimal0>>
					.Or<PRPayment.employerTaxAmount.IsNotEqual<decimal0>>>
				.Or<P.AsBool.IsEqual<True>
					.And<PRAcaEmployeeMonthlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>>
					.And<PRAcaEmployeeMonthlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>>
			.AggregateTo<GroupBy<EPEmployee.bAccountID>>.View PeriodEmployees;

		public SelectFrom<PREmployee>
			.InnerJoin<PREmployeeClass>.On<PREmployeeClass.employeeClassID.IsEqual<PREmployee.employeeClassID>>
			.InnerJoin<PRPayGroup>.On<PRPayGroup.payGroupID.IsEqual<PREmployee.payGroupID>>
			.InnerJoin<PRPayGroupYearSetup>.On<PRPayGroupYearSetup.payGroupID.IsEqual<PRPayGroup.payGroupID>>
			.Where<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View PayrollEmployee;

		public SelectFrom<EPPosition>
			.InnerJoin<EPEmployeePosition>.On<EPEmployeePosition.positionID.IsEqual<EPPosition.positionID>>
			.Where<EPEmployeePosition.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>
				.And<EPEmployeePosition.startDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<Where<EPEmployeePosition.endDate.IsNull
					.Or<EPEmployeePosition.endDate.IsGreater<P.AsDateTime.UTC>>>>>
			.OrderBy<EPEmployeePosition.startDate.Desc>.View EmployeeTitles;

		public SelectFrom<PMWorkCode>
			.InnerJoin<PREmployee>.On<PREmployee.workCodeID.IsEqual<PMWorkCode.workCodeID>>
			.Where<PREmployee.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>>.View EmployeeWorkCode;

		public SelectFrom<PREmployeeEarning>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREmployeeEarning.typeCD>>
			.Where<PREmployeeEarning.bAccountID.IsEqual<EPEmployee.bAccountID.FromCurrent>
				.And<PREmployeeEarning.isActive.IsEqual<True>>
				.And<PREmployeeEarning.startDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<Where<PREmployeeEarning.endDate.IsNull
					.Or<PREmployeeEarning.endDate.IsGreater<P.AsDateTime.UTC>>>>
				.And<EPEarningType.isOvertime.IsEqual<False>>
				.And<PREarningType.isPiecework.IsEqual<False>>
				.And<PREarningType.isAmountBased.IsEqual<False>>
				.And<PREarningType.wageTypeCD.IsEqual<P.AsInt>>>
			.OrderBy<PREmployeeEarning.startDate.Desc>.View EmployeeEarningRates;

		public SelectFrom<PREmployeeAttribute>
			.RightJoin<PRCompanyTaxAttribute>.On<PRCompanyTaxAttribute.settingName.IsEqual<PREmployeeAttribute.settingName>
				.And<PREmployeeAttribute.bAccountID.IsEqual<EPEmployee.bAccountID.AsOptional>
					.Or<PREmployeeAttribute.bAccountID.IsNull>>>
			.Where<MatchPRCountry<PRCompanyTaxAttribute.countryID>>.View EmployeeAttributes;

		public SelectFrom<PREmployeeTaxAttribute>
			.RightJoin<PRTaxCodeAttribute>.On<PRTaxCodeAttribute.settingName.IsEqual<PREmployeeTaxAttribute.settingName>
				.And<PRTaxCodeAttribute.typeName.IsEqual<PREmployeeTaxAttribute.typeName>>
				.And<PRTaxCodeAttribute.taxID.IsEqual<PREmployeeTaxAttribute.taxID>>
				.And<PREmployeeTaxAttribute.bAccountID.IsEqual<EPEmployee.bAccountID.AsOptional>
					.Or<PREmployeeTaxAttribute.bAccountID.IsNull>>>.View EmployeeTaxAttributes;

		public SelectFrom<PMProject>
			.LeftJoin<PMAddress>.On<PMAddress.addressID.IsEqual<PMProject.siteAddressID>>
			.LeftJoin<PREarningDetail>.On<PREarningDetail.projectID.IsEqual<PMProject.contractID>>
			.LeftJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PREarningDetail.paymentRefNbr>
				.And<PRPayment.docType.IsEqual<PREarningDetail.paymentDocType>>>
			.LeftJoin<PREmployee>.On<PREmployee.bAccountID.IsEqual<PRPayment.employeeID>>
			.Where<Brackets<PRPayment.startDate.IsGreaterEqual<P.AsDateTime.UTC>
					.And<PRPayment.endDate.IsLessEqual<P.AsDateTime.UTC>>
					.And<PRPayment.released.IsEqual<True>>
					.And<PRPayment.voided.IsEqual<False>>
					.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
					.And<PREmployee.exemptFromCertifiedReporting.IsNotEqual<True>>
					.And<PREarningDetail.certifiedJob.IsEqual<True>>
					.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>
					.Or<PMProjectExtension.fileEmptyCertifiedReport.IsEqual<True>
						.And<PMProject.isActive.IsEqual<True>>
						.And<PMProject.nonProject.IsEqual<False>>
						.And<PMProject.certifiedJob.IsEqual<True>>>>
				.And<PMProject.baseType.IsEqual<CT.CTPRType.project>>>
			.AggregateTo<GroupBy<PMProject.contractID>>.View PeriodProjects;

		public SelectFrom<Customer>
			.LeftJoin<Address>.On<Customer.FK.Address>
			.Where<Customer.bAccountID.IsEqual<PMProject.customerID.FromCurrent>>.View Customers;

		public SelectFrom<PMRevenueBudget>
			.Where<PMRevenueBudget.projectID.IsEqual<PMProject.contractID.FromCurrent>
				.And<PMRevenueBudget.type.IsEqual<GL.AccountType.income>>>
			.AggregateTo<GroupBy<PMRevenueBudget.projectID, Sum<PMRevenueBudget.curyActualAmount>>>.View ProjectTotalBudget;

		public SelectFrom<PMTask>
			.Where<PMTask.projectID.IsEqual<PMProject.contractID.FromCurrent>>.View ProjectTasks;

		public PeriodPaymentQuery PeriodPayments;

		public EmployeePaymentQuery EmployeePayments;

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.LeftJoin<PMProject>.On<PMProject.contractID.IsEqual<PREarningDetail.projectID>>
			.LeftJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<PREarningDetail.labourItemID>>
			.Where<PREarningDetail.paymentRefNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PREarningDetail.paymentDocType.IsEqual<PRPayment.docType.FromCurrent>>
				.And<Where<PMProject.baseType.IsEqual<CT.CTPRType.project>
					.Or<PMProject.baseType.IsEqual<CT.CTPRType.projectTemplate>>
					.Or<PMProject.baseType.IsNull>>>>.View EmployeePaycheckHourlyEarnings;

		public SelectFrom<PMLaborCostRate>
			.Where<PMLaborCostRate.type.IsEqual<PMLaborCostRateType.certified>
				.And<PMLaborCostRate.inventoryID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.projectID.IsEqual<P.AsInt>>
				.And<PMLaborCostRate.effectiveDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<PMLaborCostRate.taskID.IsEqual<P.AsInt>
					.Or<PMLaborCostRate.taskID.IsNull>>>
			.OrderBy<PMLaborCostRate.taskID.Desc, PMLaborCostRate.effectiveDate.Desc>.View PrevailingWage;

		public SelectFrom<PRTaxCode>
			.Where<MatchPRCountry<PRTaxCode.countryID>>.View TaxCodes;

		public SelectFrom<PRPaymentEarning>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PRPaymentEarning.typeCD>>
			.Where<PRPaymentEarning.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentEarning.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View PaymentEarnings;

		public SelectFrom<PRPaymentDeduct>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentDeduct.codeID>>
			.Where<PRPaymentDeduct.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>
				.And<PRPaymentDeduct.docType.IsEqual<PRPayment.docType.FromCurrent>>>.View PaymentDeductions;

		public SelectFrom<PRPaymentFringeBenefitDecreasingRate>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentFringeBenefitDecreasingRate.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentFringeBenefitDecreasingRate.docType>>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentFringeBenefitDecreasingRate.deductCodeID>>
			.LeftJoin<BAccount>.On<BAccount.bAccountID.IsEqual<PRDeductCode.bAccountID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.LeftJoin<Contact>.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.Where<PRPaymentFringeBenefitDecreasingRate.benefitRate.IsNotEqual<decimal0>
				.And<PRPayment.startDate.IsGreaterEqual<P.AsDateTime.UTC>>
				.And<PRPayment.endDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<PRPayment.released.IsEqual<True>>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>.View PeriodCertifiedBenefits;

		public SelectFrom<PRPaymentFringeBenefit>
			.InnerJoin<PRPayment>.On<PRPayment.refNbr.IsEqual<PRPaymentFringeBenefit.refNbr>
				.And<PRPayment.docType.IsEqual<PRPaymentFringeBenefit.docType>>>
			.InnerJoin<PMProject>.On<PMProject.contractID.IsEqual<PRPaymentFringeBenefit.projectID>>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PMProjectExtension.benefitCodeReceivingFringeRate>>
			.LeftJoin<BAccount>.On<BAccount.bAccountID.IsEqual<PRDeductCode.bAccountID>>
			.LeftJoin<Address>.On<Address.addressID.IsEqual<BAccount.defAddressID>>
			.LeftJoin<Contact>.On<Contact.contactID.IsEqual<BAccount.defContactID>>
			.Where<PRDeductCode.certifiedReportType.IsNotNull
				.And<PRPayment.startDate.IsGreaterEqual<P.AsDateTime.UTC>>
				.And<PRPayment.endDate.IsLessEqual<P.AsDateTime.UTC>>
				.And<PRPayment.released.IsEqual<True>>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>.View PeriodFringeDestinationBenefits;

		public SelectFrom<PRAcaCompanyYearlyInformation>
			.Where<PRAcaCompanyYearlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaCompanyYearlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaCompanyYearlyInformation;

		public SelectFrom<PRAcaCompanyMonthlyInformation>
			.Where<PRAcaCompanyMonthlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaCompanyMonthlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaCompanyMonthlyInformation;

		public SelectFrom<PRAcaAggregateGroupMember>
			.Where<PRAcaAggregateGroupMember.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaAggregateGroupMember.year.IsEqual<PRGovernmentReport.year.FromCurrent>>>.View AcaAggregateGroupMembers;

		public SelectFrom<PRAcaEmployeeMonthlyInformation>
			.Where<PRAcaEmployeeMonthlyInformation.orgBAccountID.IsEqual<BAccountR.bAccountID.FromCurrent>
				.And<PRAcaEmployeeMonthlyInformation.year.IsEqual<PRGovernmentReport.year.FromCurrent>>
				.And<PRAcaEmployeeMonthlyInformation.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>>>.View AcaEmployeeMonthlyInformation;

		public SelectFrom<PRDeductCode>
			.LeftJoin<PRAcaDeductCoverageInfo>.On<PRAcaDeductCoverageInfo.deductCodeID.IsEqual<PRDeductCode.codeID>>
			.Where<PRDeductCode.isActive.IsEqual<True>
				.And<PRAcaDeductCode.acaApplicable.IsEqual<True>>>.View AcaDeductions;

		public SelectFrom<PRPaymentTaxSplit>
			.InnerJoin<PRPaymentTax>.On<PRPaymentTaxSplit.FK.PaymentTax>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRPaymentTaxSplit.taxID>>
			.Where<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View PaymentTaxSplits;

		public SelectFrom<PRPaymentWCPremium>
			.InnerJoin<PRDeductCode>.On<PRDeductCode.codeID.IsEqual<PRPaymentWCPremium.deductCodeID>>
			.Where<PRPaymentWCPremium.docType.IsEqual<PRPayment.docType.FromCurrent>
				.And<PRPaymentWCPremium.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>
				.And<PRPaymentWCPremium.amount.IsNotEqual<decimal0>
					.Or<PRPaymentWCPremium.deductionAmount.IsNotEqual<decimal0>>>>.View PaymentWCPremiums;

		public SelectFrom<PRTaxDetail>
			.InnerJoin<APInvoice>.On<APInvoice.docType.IsEqual<PRTaxDetail.apInvoiceDocType>
				.And<APInvoice.refNbr.IsEqual<PRTaxDetail.apInvoiceRefNbr>>>
			.InnerJoin<APAdjust>.On<APAdjust.adjdDocType.IsEqual<APInvoice.docType>
				.And<APAdjust.adjdRefNbr.IsEqual<APInvoice.refNbr>>>
			.InnerJoin<APPayment>.On<APPayment.docType.IsEqual<APAdjust.adjgDocType>
				.And<APPayment.refNbr.IsEqual<APAdjust.adjgRefNbr>>>
			.InnerJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRTaxDetail.taxID>>
			.InnerJoin<PRPayment>.On<PRPayment.docType.IsEqual<PRTaxDetail.paymentDocType>
				.And<PRPayment.refNbr.IsEqual<PRTaxDetail.paymentRefNbr>>>
			.Where<PRPayment.transactionDate.IsEqual<P.AsDateTime.UTC>
				.And<APInvoice.released.IsEqual<True>>>
			.AggregateTo<GroupBy<PRTaxCode.taxID>, GroupBy<APPayment.adjDate>, Sum<PRTaxDetail.amount>>.View PaymentSettledTaxes;

		public PXSetup<PRSetup> PayrollPreferences;
		public class SetupValidation : PRSetupValidation<PRAufGenerationEngine> { }

		public SelectFrom<PREarningDetail>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<PREarningDetail.typeCD>>
			.Where<PREarningDetail.paymentDocType.IsEqual<P.AsString>
				.And<PREarningDetail.paymentRefNbr.IsEqual<P.AsString>>
				.And<PREarningDetail.projectID.IsEqual<P.AsInt>>
				.And<PREarningDetail.isFringeRateEarning.IsEqual<False>>>
			.AggregateTo<Sum<PREarningDetail.hours>>.View PaymentProjectHours;
		#endregion Views

		#region Helpers
		private void SetCurrents(int orgBAccountID, PRGovernmentReport reportToGenerate)
		{
			Report.Current = reportToGenerate;
			OrgAccount.Current = OrgAccount.SelectSingle(orgBAccountID);
		}

		protected virtual Dictionary<string, SymmetryToAatrixTaxMapping> MapTaxes()
		{
			List<PX.Payroll.Data.PRTaxType> configuredTaxes = new List<Payroll.Data.PRTaxType>();
			Dictionary<string, PRTaxCode> taxesByID = TaxCodes.Select().FirstTableItems.ToDictionary(k => k.TaxUniqueCode, v => v);
			foreach (PRTaxCode tax in taxesByID.Values)
			{
				configuredTaxes.Add(new PX.Payroll.Data.PRTaxType()
				{
					TypeName = tax.TypeName,
					UniqueTaxID = tax.TaxUniqueCode,
					TaxJurisdiction = TaxJurisdiction.GetTaxJurisdiction(tax.JurisdictionLevel),
					TaxCategory = TaxCategory.GetTaxCategory(tax.TaxCategory)
				});
			}

			var payrollClient = new PayrollTaxClient(Report.Current.CountryID);
			Dictionary<string, SymmetryToAatrixTaxMapping> mappings = payrollClient.GetAatrixTaxMapping(configuredTaxes);

			foreach (KeyValuePair<string, SymmetryToAatrixTaxMapping> kvp in mappings)
			{
				// If no Aatrix mapping exists for a local tax, map it to the generic local tax Aatrix ID
				if ((kvp.Value.TaxItemMappings == null || !kvp.Value.TaxItemMappings.Any())
					&& taxesByID[kvp.Key].JurisdictionLevel != TaxJurisdiction.Federal
					&& taxesByID[kvp.Key].JurisdictionLevel != TaxJurisdiction.State)
				{
					kvp.Value.TaxItemMappings = new List<(int aatrixID, bool isResidentTax)>() { (AufConstants.GenericLocalTaxMapping, false) };
				}
			}

			return mappings;
		}

		protected virtual DatRecord CreateDateRecord()
		{
			PRGovernmentReport reportToGenerate = Report.Current;
			DatRecord record = null;
			int? reportYear = !string.IsNullOrEmpty(reportToGenerate.Year) ? int.Parse(reportToGenerate.Year) : new int?();

			switch (reportToGenerate.ReportingPeriod)
			{
				case GovernmentReportingPeriod.Annual:
					record = new DatRecord(new DateTime(reportYear.Value, 1, 1), new DateTime(reportYear.Value, 12, 31));
					break;
				case GovernmentReportingPeriod.Quarterly:
					record = new DatRecord(
						new DateTime(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 1, 1),
						new DateTime(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 3, DateTime.DaysInMonth(reportYear.Value, (reportToGenerate.Quarter.Value - 1) * 3 + 3)))
					{
						Quarter = reportToGenerate.Quarter
					};
					break;
				case GovernmentReportingPeriod.Monthly:
					record = new DatRecord(
						new DateTime(reportYear.Value, reportToGenerate.Month.Value, 1),
						new DateTime(reportYear.Value, reportToGenerate.Month.Value, DateTime.DaysInMonth(reportYear.Value, reportToGenerate.Month.Value)))
					{
						Month = reportToGenerate.Month
					};
					break;
				case GovernmentReportingPeriod.DateRange:
					record = new DatRecord(reportToGenerate.DateFrom.Value, reportToGenerate.DateTo.Value);
					break;
			}

			return record;
		}

		protected virtual CmpRecord CreateCompanyRecord(DatRecord reportPeriod, IEnumerable<EmpRecord> empList)
		{
			Address address = OrgAddress.SelectSingle();
			Contact contact = OrgContact.SelectSingle();

			CmpRecord record = new CmpRecord()
			{
				Name = OrgAccount.Current.AcctName,
				AddressLine1 = address?.AddressLine1,
				AddressLine2 = address?.AddressLine2,
				City = address?.City,
				StateAbbr = address?.State,
				ZipCode = address?.PostalCode,
				Country = OrgCountry.SelectSingle()?.Description,
				CountryAbbr = address?.CountryID,
				NonUSPostalCode = address?.PostalCode,
				BranchName = OrgAccount.Current.AcctCD.TrimEnd(),
				TaxArea = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.TaxArea)?.Value,
				PhoneNumber = contact?.Phone1,
				PhoneExtension = contact?.Phone2,
				FaxNumber = contact?.Fax,
				IndustryCode = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.IndustryCode)?.Value,
				Ein = OrgAccount.Current.TaxRegistrationID,
				NumberOfEmployees = TotalEmployees.Select().Count,
				ContactEmail = contact?.EMail,
				NonUSState = address?.State,
				NonUSNationalID = OrgAccount.Current.TaxRegistrationID,
				KindOfEmployer = CompanyTaxAttribute.SelectSingle(AatrixField.CMP.KindOfEmployer)?.Value?.First(),

				// Children records
				GtoList = CreateGeneralCompanyRecords(empList).OrderBy(x => x.CheckDate).ToList()
			};

			if (ShouldGenerateCertifiedRecords())
			{
				record.CjiList = _JobItemMapping.OrderBy(x => x.JobID).ToList();
				record.CfbList = CreateCompanyCertifiedBenefitRecords(reportPeriod).OrderBy(x => x.BenefitID).ToList();
			}

			if (ShouldGenerateAcaRecords())
			{
				record.Ale = CreateCompanyAleRecord();

				if (record.Ale != null)
				{
					if (record.Ale.IsAggregateGroupMember == true)
					{
						record.AggList = CreateCompanyAggRecords().ToList();
					}

					if (record.Ale.IsDesignatedGovernmentEntity == true)
					{
						record.Dge = CreateCompanyDgeRecord(record);
					}
				}
			}

			return record;
		}

		protected virtual AleRecord CreateCompanyAleRecord()
		{
			PRAcaCompanyYearlyInformation companyYear = AcaCompanyYearlyInformation.SelectSingle();
			if (companyYear != null)
			{
				string isDesignatedGovernmentEntity = CompanyTaxAttribute.SelectSingle(AatrixField.ALE.IsDesignatedGovernmentEntity)?.Value;

				bool offersMinimumCoverage = false;
				foreach (IGrouping<int, PXResult<PRDeductCode, PRAcaDeductCoverageInfo>> dedResult in AcaDeductions
					.Select()
					.Select(x => (PXResult<PRDeductCode, PRAcaDeductCoverageInfo>)x)
					.GroupBy(x => ((PRDeductCode)x).CodeID.Value))
				{
					if (AcaOfferOfCoverage.MeetsMinimumCoverageRequirement(dedResult.Select(x => (PRAcaDeductCoverageInfo)x)))
					{
						offersMinimumCoverage = true;
						break;
					}
				}

				AleRecord record = new AleRecord()
				{
					IsDesignatedGovernmentEntity = string.IsNullOrEmpty(isDesignatedGovernmentEntity) ? new bool?() : bool.Parse(isDesignatedGovernmentEntity),
					IsAggregateGroupMember = companyYear.IsPartOfAggregateGroup,
					IsSelfInsured = false,
					UsesCoeQualifyingOfferMethod = true,
					UsesCoe98PctMethod = true,
					MecIndicator = offersMinimumCoverage,
					IsAuthoritativeTransmittal = companyYear.IsAuthoritativeTransmittal
				};

				foreach (PRAcaCompanyMonthlyInformation companyMonth in AcaCompanyMonthlyInformation.Select())
				{
					int monthIndex = companyMonth.Month.Value - 1;

					if (companyMonth.SelfInsured == true)
					{
						record.IsSelfInsured = true;
					}

					if (companyMonth.CertificationOfEligibility != AcaCertificationOfEligibility.QualifyingOfferMethod)
					{
						record.UsesCoeQualifyingOfferMethod = false;
					}

					if (companyMonth.CertificationOfEligibility != AcaCertificationOfEligibility.NinetyEightPctMethod)
					{
						record.UsesCoe98PctMethod = false;
					}

					if (companyMonth.NumberOfFte != null)
					{
						record.FteCount[monthIndex] = companyMonth.NumberOfFte.Value;
					}

					if (companyMonth.NumberOfEmployees != null)
					{
						record.EmployeeCount[monthIndex] = companyMonth.NumberOfEmployees.Value;
					}
				}

				return record;
			}

			return null;
		}

		protected virtual DgeRecord CreateCompanyDgeRecord(CmpRecord cmp)
		{
			Contact contact = OrgContact.SelectSingle();

			DgeRecord record = new DgeRecord(cmp.Name, cmp.Ein)
			{
				AddressLine1 = cmp.AddressLine1,
				AddressLine2 = cmp.AddressLine2,
				City = cmp.City,
				StateAbbr = cmp.StateAbbr,
				ZipCode = cmp.ZipCode,
				NonUSState = cmp.NonUSState,
				NonUSPostalCode = cmp.NonUSPostalCode,
				Country = cmp.Country,
				CountryAbbr = cmp.CountryAbbr,
				ContactFirstName = contact?.FirstName,
				ContactPhoneNumber = contact?.Phone1,
				ContactPhoneExtension = contact?.Phone2,
				ContactMiddleName = contact?.MidName,
				ContactLastName = contact?.LastName
			};

			return record;
		}

		protected virtual IEnumerable<AggRecord> CreateCompanyAggRecords()
		{
			foreach (PRAcaAggregateGroupMember member in AcaAggregateGroupMembers.Select())
			{
				AggRecord record = new AggRecord(member.HighestMonthlyFteNumber)
				{
					MemberName = member.MemberCompanyName.TrimEnd(),
					MemberEin = member.MemberEin
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<EmpRecord> CreateEmpRecords(DatRecord reportPeriod)
		{
			bool shouldGenerateAcaRecords = ShouldGenerateAcaRecords();
			var periodEmployees = PeriodEmployees.Select(reportPeriod.FirstDate, reportPeriod.LastDate, reportPeriod.FirstDate, reportPeriod.LastDate, shouldGenerateAcaRecords).ToList();

			foreach (PXResult<EPEmployee, Contact, Address, Country, PREmployeeDirectDeposit> employeeResult 
				in periodEmployees)
			{
				EPEmployee employee = (EPEmployee)employeeResult;
				PeriodEmployees.Current = employee;

				Contact contactInfo = (Contact)employeeResult;
				Address address = (Address)employeeResult;
				Country country = (Country)employeeResult;
				PREmployeeDirectDeposit dd = (PREmployeeDirectDeposit)employeeResult;

				var payrollEmployeeResult = (PXResult<PREmployee, PREmployeeClass, PRPayGroup, PRPayGroupYearSetup>)PayrollEmployee.Select()?[0];
				PREmployee prEmployee = (PREmployee)payrollEmployeeResult;
				PREmployeeClass employeeClass = (PREmployeeClass)payrollEmployeeResult;
				PRPayGroupYearSetup payGroupSetup = (PRPayGroupYearSetup)payrollEmployeeResult;

				EmploymentDates employmentDates = EmploymentHistoryHelper.GetEmploymentDates(this, prEmployee.BAccountID, reportPeriod.LastDate);

				int? regularWageType = PRTypeSelectorAttribute.GetDefaultID<PRWage>(PRCountryAttribute.GetPayrollCountry());

				string title = EmployeeTitles.SelectSingle(reportPeriod.LastDate, reportPeriod.LastDate)?.Description;
				PMWorkCode workCode = EmployeeWorkCode.SelectSingle();
				InventoryItem laborItem = employee.LabourItemID != null ? PXSelectorAttribute.Select<EPEmployee.labourItemID>(PeriodEmployees.Cache, employee) as InventoryItem : null;

				List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes = EmployeeAttributes.Select().Select(x => (PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>)x).ToList();
				string isFemaleAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.IsFemale);
				string isDisableAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.IsDisabled);
				string federalExemptionsAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.FederalExemptions);
				string isFullTimeAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.IsFullTime);
				string hasHealthBenefitsAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.HasHealthBenefits);
				string isSeasonalAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.IsSeasonal);
				string isStatutoryEmployeeAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.IsStatutoryEmployee);
				string hasRetirementPlanAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.HasRetirementPlan);
				string hasThirdPartySickPayAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.HasThirdPartySickPay);
				string hasElectronicW2AsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.HasElectronicW2);
				string standardOccupationalClassification = GetAttributeValue(employeeAttributes, AatrixField.EMP.StandardOccupationalClassification);
				string eeoJobCategoryAsStr = GetAttributeValue(employeeAttributes, AatrixField.EMP.EEOJobCategory);

				bool exemptFromCertifiedReporting = prEmployee.ExemptFromCertifiedReporting == true;

				EmpRecord record = new EmpRecord(employee.AcctCD)
				{
					FirstName = contactInfo.FirstName,
					MiddleName = contactInfo.MidName,
					LastName = contactInfo.LastName,
					SocialSecurityNumber = GetAttributeValue(employeeAttributes, AatrixField.EMP.SocialSecurityNumber),
					AddressLine1 = address?.AddressLine1,
					City = address?.City,
					StateAbbr = address?.State,
					ZipCode = address?.PostalCode,
					Country = country?.Description,
					CountryAbbr = address?.CountryID,
					NonUSPostalCode = address?.PostalCode,
					IsFemale = string.IsNullOrEmpty(isFemaleAsStr) ? new bool?() : bool.Parse(isFemaleAsStr),
					IsDisabled = string.IsNullOrEmpty(isDisableAsStr) ? new bool?() : bool.Parse(isDisableAsStr),
					HireDate = employmentDates.InitialHireDate,
					FireDate = employmentDates.TerminationDateAuf,
					BirthDate = contactInfo.DateOfBirth,
					PayRate = EmployeeEarningRates.SelectSingle(reportPeriod.LastDate, reportPeriod.LastDate, regularWageType)?.PayRate.Value,
					FederalExemptions = string.IsNullOrEmpty(federalExemptionsAsStr) ? new int?() : int.Parse(federalExemptionsAsStr),
					IsHourlyPay = (prEmployee.EmpTypeUseDflt == true ? employeeClass.EmpType : prEmployee.EmpType) == EmployeeType.Hourly,
					IsFullTime = string.IsNullOrEmpty(isFullTimeAsStr) ? new bool?() : bool.Parse(isFullTimeAsStr),
					Title = title,
					StateOfHireAbbr = OrgAddress.SelectSingle()?.State,
					WorkType = laborItem?.Descr,
					HasHealthBenefits = string.IsNullOrEmpty(hasHealthBenefitsAsStr) ? new bool?() : bool.Parse(hasHealthBenefitsAsStr),
					PhoneNumber = contactInfo.Phone1,
					IsSeasonal = string.IsNullOrEmpty(isSeasonalAsStr) ? new bool?() : bool.Parse(isSeasonalAsStr),
					WorkersCompClass = workCode?.WorkCodeID,
					MaritalStatus = GetAttributeValue(employeeAttributes, AatrixField.EMP.MaritalStatus),
					EmployeeID = employee.AcctCD.TrimEnd(),
					IsStatutoryEmployee = string.IsNullOrEmpty(isStatutoryEmployeeAsStr) ? new bool?() : bool.Parse(isStatutoryEmployeeAsStr),
					HasRetirementPlan = string.IsNullOrEmpty(hasRetirementPlanAsStr) ? new bool?() : bool.Parse(hasRetirementPlanAsStr),
					HasThirdPartySickPay = string.IsNullOrEmpty(hasThirdPartySickPayAsStr) ? new bool?() : bool.Parse(hasThirdPartySickPayAsStr),
					HasDirectDeposit = dd.BankAcctNbr != null,
					AddressLine2 = address?.AddressLine2,
					Email = contactInfo.EMail,
					HasElectronicW2 = string.IsNullOrEmpty(hasElectronicW2AsStr) ? new bool?() : bool.Parse(hasElectronicW2AsStr),
					NonUSState = address?.State,
					RehireDate = employmentDates.RehireDateAuf,
					EmploymentCode = GetAttributeValue(employeeAttributes, AatrixField.EMP.EmploymentCode),
					StandardOccupationalClassification = standardOccupationalClassification,
					NonUSNationalID = null,
					CppExempt = false,
					EmploymentInsuranceExempt = false,
					ProvincialParentalInsurancePlanExempt = false,
					StateExemptions = employeeAttributes
						.Where(x => ((PRCompanyTaxAttribute)x).AatrixMapping == AatrixField.EMP.StateExemptions && ((PRCompanyTaxAttribute)x).State == Report.Current.State)
						.Sum(x => GetAttributeValue(x) == null ? 0 : int.Parse(GetAttributeValue(x))),
					Ethnicity = GetAttributeValue(employeeAttributes, AatrixField.EMP.Ethnicity),
					EEOEthnicity = GetAttributeValue(employeeAttributes, AatrixField.EMP.EEOEthnicity),
					EEOJobCategory = string.IsNullOrEmpty(eeoJobCategoryAsStr) ? new int?() : int.Parse(eeoJobCategoryAsStr),

					// Children records
					GenList = CreateGeneralEmployeeRecords(reportPeriod, payGroupSetup.PeriodType == PayPeriodType.Week, exemptFromCertifiedReporting, employeeAttributes).OrderBy(x => x.CheckDate).ToList()
				};

				if (ShouldGenerateCertifiedRecords() && !exemptFromCertifiedReporting)
				{
					record.EfbList = CreateEmployeeCertifiedBenefitRecords(reportPeriod).OrderBy(x => x.BenefitID).ToList();
				}

				if (shouldGenerateAcaRecords)
				{
					(record.Ecv, record.Eci) = CreateEmployeeAcaRecords(record);
				}

				record.EffList = employeeAttributes
					.Where(x => IsApplicableAatrixPredefinedField((PRCompanyTaxAttribute)x))
					.Select(x => new EffRecord(((PRCompanyTaxAttribute)x).AatrixMapping.Value, GetAttributeValue(x)))
					.ToList();
				record.EffList.AddRange(
					EmployeeTaxAttributes.Select()
					.Select(x => (PXResult<PREmployeeTaxAttribute, PRTaxCodeAttribute>)x)
					.ToList()
					.Where(x => IsApplicableAatrixPredefinedField((PRTaxCodeAttribute)x))
					.Select(x => new EffRecord(((PRTaxCodeAttribute)x).AatrixMapping.Value, GetAttributeValue(x))));

				yield return record;
			}
		}

		protected virtual IEnumerable<CjiRecord> CreateCompanyJobRecords(DatRecord reportPeriod)
		{
			foreach (PXResult<PMProject, PMAddress> projectResult
				in PeriodProjects.Select(reportPeriod.FirstDate, reportPeriod.LastDate))
			{
				PMProject project = projectResult;
				PMAddress projectAddress = projectResult;
				PeriodProjects.Current = project;

				PXResult<Customer, Address> result = Customers.Select().FirstOrDefault() as PXResult<Customer, Address>;
				Customer customer = result;
				Address customerAddress = result;

				decimal? completedPercent = ProjectTasks.Select().RowCast<PMTask>().ToList().Average(x => x.CompletedPercent);

				var record = new CjiRecord(project.ContractID.Value)
				{
					ProjectName = project.Description,
					ProjectNumber = project.ContractCD.TrimEnd(),
					ProjectAddress = ConcatAddress(projectAddress),
					AwardingContractor = customer?.AcctName,
					ACAddress = customerAddress != null ? ConcatAddress(customerAddress) : null,
					ACCity = customerAddress?.City,
					ACStateAbbr = customerAddress?.State,
					ACZipCode = customerAddress?.PostalCode,
					ContractAmount = ProjectTotalBudget.SelectSingle()?.CuryActualAmount,
					EstimatedCompletionDate = project.ExpireDate,
					EstimatedPercentComplete = completedPercent == null ? new int?() : (int)Math.Round(completedPercent.Value),
					TypeOfWork = project.Description,
					ProjectCity = projectAddress?.City,
					ProjectStateAbbr = projectAddress?.State,
					ProjectZipCode = projectAddress?.PostalCode
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<GenRecord> CreateGeneralEmployeeRecords(DatRecord reportPeriod, bool isWeeklyEmployee, bool exemptFromCertifiedReporting,
			List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes)
		{
			string FicaTaxUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Fica).Key;
			string ERFicaTaxUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.EmployerFica).Key;
			string MedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Medicare).Key;
			string ERMedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.EmployerMedicare).Key;
			string AdditionalMedicareUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.AdditionalMedicare).Key;
			string FitUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Fit).Key;
			string FutaUniqueID = _TaxIDMapping.FirstOrDefault(kvp => kvp.Value.Field == AatrixTax.Futa).Key;

			string[] docTypes = { PayrollType.Regular, PayrollType.Special, PayrollType.Adjustment, PayrollType.VoidCheck };
			var employeePayments = EmployeePayments
				.Select(reportPeriod.FirstDate, reportPeriod.LastDate, reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPayment, PRPaymentTax, PRPaymentTaxSplit, PRTaxCode>)x)
				.GroupBy(x => new { docType = ((PRPayment)x).DocType, refNbr = ((PRPayment)x).RefNbr })
				.OrderBy(x => Array.IndexOf(docTypes, x.Key.docType)).ToList();


			foreach (var paymentResult in employeePayments)
			{
				PRPayment payment = (PRPayment)paymentResult.First();
				EmployeePayments.Current = payment;
				List<PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> paymentTaxSplits = PaymentTaxSplits.Select().Select(x => (PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>)x).ToList();

				var record = new GenRecord(payment.TransactionDate.Value, payment.DocType == PayrollType.Regular)
				{
					GrossPay = payment.GrossAmount,
					NetPay = payment.NetAmount,
					SSWages = 0m,
					SSWithheld = 0m,
					MedicareWages = 0m,
					MedicareWithheld = 0m,
					FederalWages = 0m,
					FederalWithheld = 0m,
					TaxableFutaWages = 0m,
					SSTips = 0m,
					FutaLiability = 0m,
					TotalFutaWages = 0m,
					PeriodStart = payment.StartDate,
					PeriodEnd = payment.EndDate,
					SSEmployerMatch = 0m,
					MedicareEmployerMatch = 0m,
					AdditionalMedicareTax = 0m,
					AdditionalMedicareWages = 0m,
					StateWithheld = 0m,
					SuiWithheld = 0m,
					SdiWithheld = 0m,
					CheckNumber = payment.RefNbr,

					// Children records
					EsiList = CreateEsiRecords(employeeAttributes, paymentTaxSplits).OrderBy(x => x.PimID).ToList(),
					EliList = CreateTaxEliRecords(paymentTaxSplits).OrderBy(x => x.PimID).ToList()
				};

				foreach (IGrouping<int?, PXResult<PRPayment, PRPaymentTax, PRPaymentTaxSplit, PRTaxCode>> taxGroup in paymentResult
					.Where(x => ((PRTaxCode)x)?.TaxID != null)
					.GroupBy(x => ((PRTaxCode)x).TaxID))
				{
					PRTaxCode taxCode = taxGroup.First();
					PRPaymentTax paymentTax = taxGroup.First();

					if (taxCode.TaxUniqueCode == FicaTaxUniqueID)
					{
						record.SSWages += taxGroup.Select(x => (PRPaymentTaxSplit)x)
							.Where(x => !IsTipWage(x.WageType))
							.Sum(x => x.WageBaseAmount.GetValueOrDefault());
					}
					if (taxCode.TaxUniqueCode == FicaTaxUniqueID)
					{
						record.SSWithheld += paymentTax.TaxAmount;
					}
					if (taxCode.TaxUniqueCode == MedicareUniqueID)
					{
						record.MedicareWages += paymentTax.WageBaseAmount;
						record.MedicareWithheld += paymentTax.TaxAmount;
					}
					if (taxCode.TaxUniqueCode == FitUniqueID)
					{
						record.FederalWages += paymentTax.WageBaseAmount;
						record.FederalWithheld += paymentTax.TaxAmount;
					}
					if (taxCode.TaxUniqueCode == FutaUniqueID)
					{
						record.TaxableFutaWages += paymentTax.WageBaseAmount;
						record.FutaLiability += paymentTax.TaxAmount;
						record.TotalFutaWages += (paymentTax.WageBaseGrossAmt);
					}
					if (taxCode.TaxUniqueCode == FicaTaxUniqueID)
					{
						record.SSTips += taxGroup.Select(x => (PRPaymentTaxSplit)x)
							.Where(x => IsTipWage(x.WageType))
							.Sum(x => x.WageBaseAmount.GetValueOrDefault());
					}
					if (taxCode.TaxUniqueCode == ERFicaTaxUniqueID)
					{
						record.SSEmployerMatch += paymentTax.TaxAmount;
					}
					if (taxCode.TaxUniqueCode == ERMedicareUniqueID)
					{
						record.MedicareEmployerMatch += paymentTax.TaxAmount;
					}
					if (taxCode.TaxUniqueCode == AdditionalMedicareUniqueID)
					{
						record.AdditionalMedicareTax += paymentTax.TaxAmount;
						record.AdditionalMedicareWages += paymentTax.WageBaseAmount;
					}
					if (taxCode.TaxState == Report.Current.State || Report.Current.State == LocationConstants.USFederalStateCode)
					{
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sit).Select(y => y.Key).Contains(taxCode.TaxUniqueCode))
						{
							record.StateWithheld += paymentTax.TaxAmount;
						}
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sui).Select(y => y.Key).Contains(taxCode.TaxUniqueCode))
						{
							record.SuiWithheld += paymentTax.TaxAmount;
						}
						if (_TaxIDMapping.Where(kvp => kvp.Value.Field == AatrixTax.Sdi).Select(y => y.Key).Contains(taxCode.TaxUniqueCode))
						{
							record.SdiWithheld += paymentTax.TaxAmount;
						}
					}
				}

				if (isWeeklyEmployee && ShouldGenerateCertifiedRecords() && !exemptFromCertifiedReporting)
				{
					record.EjwList = CreateJobWeekRecords(record).OrderBy(x => x.JobID).ToList();
				}

				yield return record;
			}

			this._PaymentPeriods.Clear();
		}

		protected virtual IEnumerable<GtoRecord> CreateGeneralCompanyRecords(IEnumerable<EmpRecord> empList)
		{
			foreach (var paymentGroup in empList.SelectMany(x => x.GenList).GroupBy(x => new { x.PeriodStart, x.PeriodEnd, x.CheckDate }))
			{
				GtoRecord record = new GtoRecord(paymentGroup.Key.CheckDate)
				{
					GrossPay = 0m,
					NetPay = 0m,
					SSWages = 0m,
					SSLiability = 0m,
					MedicareWages = 0m,
					FederalWages = 0m,
					FederalWHLiability = 0m,
					TaxableFutaWages = 0m,
					FutaLiability = 0m,
					SSTips = 0m,
					TotalFutaWages = 0m,
					PeriodStart = paymentGroup.Key.PeriodStart,
					PeriodEnd = paymentGroup.Key.PeriodEnd,
					SSEmployerMatch = 0m,
					MedicareEmployerMatch = 0m,
					AdditionalMedicareTax = 0m,
					AdditionalMedicareWages = 0m,

					// Children records
					CsiList = CreateCsiRecords(empList, paymentGroup.Key.PeriodStart.Value, paymentGroup.Key.PeriodEnd.Value, paymentGroup.Key.CheckDate)
						.OrderBy(x => x.PimID)
						.ToList(),
					CliList = CreateCliRecords(empList, paymentGroup.Key.PeriodStart.Value, paymentGroup.Key.PeriodEnd.Value, paymentGroup.Key.CheckDate)
						.OrderBy(x => x.PimID)
						.ToList(),
					CspList = CreateCspList(paymentGroup.Key.CheckDate).ToList(),
					ClpList = CreateClpList(paymentGroup.Key.CheckDate).ToList()
				};

				foreach (GenRecord general in paymentGroup)
				{
					record.GrossPay += general.GrossPay;
					record.NetPay += general.NetPay;
					record.SSWages += general.SSWages;
					record.SSLiability += general.SSWithheld + general.SSEmployerMatch;
					record.MedicareWages += general.MedicareWages;
					record.FederalWages += general.FederalWages;
					record.FederalWHLiability += general.FederalWithheld;
					record.TaxableFutaWages += general.TaxableFutaWages;
					record.FutaLiability += general.FutaLiability;
					record.SSTips += general.SSTips;
					record.TotalFutaWages += general.TotalFutaWages;
					record.SSEmployerMatch += general.SSEmployerMatch;
					record.MedicareEmployerMatch += general.MedicareEmployerMatch;
					record.AdditionalMedicareTax += general.AdditionalMedicareTax;
					record.AdditionalMedicareWages += general.AdditionalMedicareWages;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<EjwRecord> CreateJobWeekRecords(GenRecord general)
		{
			Dictionary<CertifiedJobKey, EjwRecord> records = new Dictionary<CertifiedJobKey, EjwRecord>();

			IEnumerable<PXResult<PREarningDetail, EPEarningType, PMProject, InventoryItem>> earningResults = 
				EmployeePaycheckHourlyEarnings.Select().Select(x => (PXResult<PREarningDetail, EPEarningType, PMProject, InventoryItem>)x);

			decimal totalHoursDay1 = 0m;
			decimal totalHoursDay2 = 0m;
			decimal totalHoursDay3 = 0m;
			decimal totalHoursDay4 = 0m;
			decimal totalHoursDay5 = 0m;
			decimal totalHoursDay6 = 0m;
			decimal totalHoursDay7 = 0m;
			foreach (PREarningDetail earningDetail in earningResults)
			{
				if (earningDetail.Date.Value.Date == general.PeriodStart.Value.Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay1 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(1, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay2 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(2, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay3 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(3, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay4 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(4, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay5 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(5, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay6 += earningDetail.Hours.GetValueOrDefault();
				}
				else if (earningDetail.Date.Value.Date == (general.PeriodStart.Value + new TimeSpan(6, 0, 0, 0)).Date && earningDetail.IsFringeRateEarning != true)
				{
					totalHoursDay7 += earningDetail.Hours.GetValueOrDefault();
				}
			}

			int? firstJobID = null;

			foreach (PXResult<PREarningDetail, EPEarningType, PMProject, InventoryItem> earningResult in
				earningResults.Where(x => ((PREarningDetail)x).CertifiedJob == true && ((PMProject)x).ContractID != null && ((InventoryItem)x).InventoryID != null))
			{
				PREarningDetail earningDetail = earningResult;
				EPEarningType earningType = earningResult;
				PMProject project = earningResult;
				InventoryItem laborItem = earningResult;

				if (firstJobID == null)
				{
					firstJobID = project.ContractID.Value;
				}

				CjiRecord itemMap = _JobItemMapping.FirstOrDefault(x => x.JobID == project.ContractID);
				if (itemMap != null)
				{
					CertifiedJobKey key = new CertifiedJobKey(project.ContractID.Value, laborItem.InventoryID.Value);
					bool recordExists = records.TryGetValue(key, out EjwRecord record);
					if (!recordExists)
					{
						record = new EjwRecord(project.ContractID.Value, laborItem.InventoryID.Value)
						{
							WorkClassification = laborItem.Descr,
							JobGross = earningDetail.Amount,
							RegularHoursDay1 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, false),
							RegularHoursDay2 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, false),
							RegularHoursDay3 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, false),
							RegularHoursDay4 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, false),
							RegularHoursDay5 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, false),
							RegularHoursDay6 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, false),
							RegularHoursDay7 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, false),
							OvertimeHoursDay1 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, true),
							OvertimeHoursDay2 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, true),
							OvertimeHoursDay3 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, true),
							OvertimeHoursDay4 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, true),
							OvertimeHoursDay5 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, true),
							OvertimeHoursDay6 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, true),
							OvertimeHoursDay7 = GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, true),
							WorkClassificationCode = laborItem.InventoryCD,
							TotalHoursDay1 = totalHoursDay1,
							TotalHoursDay2 = totalHoursDay2,
							TotalHoursDay3 = totalHoursDay3,
							TotalHoursDay4 = totalHoursDay4,
							TotalHoursDay5 = totalHoursDay5,
							TotalHoursDay6 = totalHoursDay6,
							TotalHoursDay7 = totalHoursDay7,
							AdditionalPrevailingWage = project.ContractID.Value == firstJobID ? null : "X"
						};

						records[key] = record;
					}
					else
					{
						record.JobGross += earningDetail.Amount;
						record.RegularHoursDay1 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, false);
						record.RegularHoursDay2 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, false);
						record.RegularHoursDay3 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, false);
						record.RegularHoursDay4 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, false);
						record.RegularHoursDay5 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, false);
						record.RegularHoursDay6 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, false);
						record.RegularHoursDay7 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, false);
						record.OvertimeHoursDay1 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 0, true);
						record.OvertimeHoursDay2 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 1, true);
						record.OvertimeHoursDay3 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 2, true);
						record.OvertimeHoursDay4 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 3, true);
						record.OvertimeHoursDay5 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 4, true);
						record.OvertimeHoursDay6 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 5, true);
						record.OvertimeHoursDay7 += GetHoursForDay(earningType, earningDetail, general.PeriodStart.Value, 6, true);
					}
				}
			}

			foreach (EjwRecord record in records.Values)
			{
				IEnumerable<PXResult<PREarningDetail, EPEarningType, PMProject, InventoryItem>> jobEarnings = earningResults
					.Where(x => ((PREarningDetail)x).CertifiedJob == true && ((PMProject)x).ContractID == record.JobID && ((InventoryItem)x).InventoryCD == record.WorkClassificationCode);

				decimal jobTotalRegularHours = (record.RegularHoursDay1 + record.RegularHoursDay2 + record.RegularHoursDay3 + record.RegularHoursDay4 +
					record.RegularHoursDay5 + record.RegularHoursDay6 + record.RegularHoursDay7).Value;
				decimal jobTotalOvertimeHours = (record.OvertimeHoursDay1 + record.OvertimeHoursDay2 + record.OvertimeHoursDay3 + record.OvertimeHoursDay4 +
					record.OvertimeHoursDay5 + record.OvertimeHoursDay6 + record.OvertimeHoursDay7).Value;

				decimal jobTotalRegularWages = 0m;
				decimal jobTotalRegularFringe = 0m;
				decimal jobTotalOvertimeWages = 0m;
				decimal jobTotalOvertimeFringe = 0m;
				decimal overtimeMultiplier = 0m;
				foreach (PXResult<PREarningDetail, EPEarningType, PMProject, InventoryItem> jobEarning in jobEarnings)
				{
					PREarningDetail earningDetail = jobEarning;
					EPEarningType earningType = jobEarning;
					overtimeMultiplier = earningType.OvertimeMultiplier.HasValue ? earningType.OvertimeMultiplier.Value : 1.5m;

					if (earningType.IsOvertime != true)
					{
						if (earningDetail.IsFringeRateEarning == true)
						{
							jobTotalRegularFringe += earningDetail.Amount.GetValueOrDefault();
						}
						else
						{
							jobTotalRegularWages += earningDetail.Amount.GetValueOrDefault();
						}
					}
					else
					{
						if (earningDetail.IsFringeRateEarning == true)
					{
							jobTotalOvertimeFringe += earningDetail.Amount.GetValueOrDefault();
					}
						else
					{
							jobTotalOvertimeWages += earningDetail.Amount.GetValueOrDefault();
					}
				}
				}

				if (jobTotalOvertimeHours != 0)
				{
					record.OvertimeHourlyRate = jobTotalOvertimeWages / jobTotalOvertimeHours;
					record.CashFringeOvertimeHourlyRate = jobTotalOvertimeFringe / jobTotalOvertimeHours;
				}

				if (jobTotalRegularHours != 0)
				{
					record.RegularHourlyRate = jobTotalRegularWages / jobTotalRegularHours;
					record.CashFringeRegularHourlyRate = jobTotalRegularFringe / jobTotalRegularHours;
				}
				else
				{
					record.RegularHourlyRate = (record.OvertimeHourlyRate ?? 0) / overtimeMultiplier;
				}

				record.JobTotalRegularFringe = jobTotalRegularFringe;
				record.JobTotalRegularWages = jobTotalRegularWages;
				record.JobTotalOvertimeFringe = jobTotalOvertimeFringe;
				record.JobTotalOvertimeWages = jobTotalOvertimeWages;
				record.OvertimeMultiplier = overtimeMultiplier;

				// Prevailing
				record.IsPrevailingWage = false;
				bool allSameLabourItem = jobEarnings.GroupBy(x => ((PREarningDetail)x).LabourItemID).Count() == 1;
				bool allSameRate = jobEarnings.Where(x => ((EPEarningType)(PXResult<PREarningDetail, EPEarningType>)x).IsOvertime != true && ((PREarningDetail)x).IsFringeRateEarning != true).GroupBy(x => ((PREarningDetail)x).Rate).Count() == 1;
				if (allSameLabourItem && allSameRate)
				{
					PMLaborCostRate prevailingWage = PrevailingWage.SelectSingle(jobEarnings.Select(x => ((PREarningDetail)x).LabourItemID).First(), record.JobID, record.CheckDate, jobEarnings.Select(x => ((PREarningDetail)x).ProjectTaskID).First());
					record.IsPrevailingWage = prevailingWage?.WageRate == record.RegularHourlyRate;
				}

				record.OtherDeductions = general.GrossPay - general.NetPay -
						general.FederalWithheld - general.StateWithheld - general.SuiWithheld - general.SdiWithheld - general.SSWithheld - general.MedicareWithheld -
						general.EliList.Where(x => x.IsResidentTax).Sum(x => x.WithholdingAmount);
			}

			return records.Values;
		}

		protected virtual IEnumerable<PimRecord> CreatePimRecords(DatRecord reportPeriod)
		{
			var periodPayments = PeriodPayments
				.Select(reportPeriod.FirstDate, reportPeriod.LastDate, reportPeriod.FirstDate, reportPeriod.LastDate).ToList();

			return CreateEarningPimRecords(reportPeriod, periodPayments)
				.Union(CreateBenefitPimRecords(reportPeriod, periodPayments), new PimRecordComparer())
				.Union(CreateTaxPimRecords(reportPeriod, periodPayments), new PimRecordComparer());
		}

		protected virtual IEnumerable<PimRecord> CreateEarningPimRecords(DatRecord reportPeriod, List<PXResult<PRPayment>> periodPayments)
		{
			Dictionary<int, PRTypeMeta> wageReportingTypes = PRReportingTypeSelectorAttribute.GetAll<PRWage>(PRCountryAttribute.GetPayrollCountry()).ToDictionary(k => k.ID, v => v);
			HashSet<int> usedEarningReportingTypes = new HashSet<int>();
			foreach (PRPayment payment in periodPayments)
			{
				var paymentEarnings = PaymentEarnings.View.SelectMultiBound(new object[] { payment }).Select(x => (PXResult<PRPaymentEarning>)x).ToList();
				foreach (PRPaymentEarning paymentEarning in paymentEarnings)
				{
					EPEarningType epEarningType = PXSelectorAttribute.Select<PRPaymentEarning.typeCD>(PaymentEarnings.Cache, paymentEarning) as EPEarningType;
					if (epEarningType == null)
					{
						continue;
					}

					PREarningType prEarningType = PXCache<EPEarningType>.GetExtension<PREarningType>(epEarningType);
					if (prEarningType?.ReportType != null)
					{
						usedEarningReportingTypes.Add(prEarningType.ReportType.Value);
					}
				}
			}

			foreach (PRTypeMeta prType in usedEarningReportingTypes.Select(x => wageReportingTypes[x]).Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				PimRecord record = new PimRecord(prType.Name)
				{
					Description = prType.Description,
					AatrixTaxType = prType.ID
				};

				yield return record;
			}
		}

		protected virtual IEnumerable<PimRecord> CreateBenefitPimRecords(DatRecord reportPeriod, List<PXResult<PRPayment>> periodPayments)
		{
			Dictionary<int, PRTypeMeta> deductionReportingTypes = PRReportingTypeSelectorAttribute.GetAll<PRBenefit>(PRCountryAttribute.GetPayrollCountry()).ToDictionary(k => k.ID, v => v);
			HashSet<int> usedDeductionReportingTypes = new HashSet<int>();
			HashSet<string> statesWithWC = new HashSet<string>();
			foreach (PRPayment payment in periodPayments)
			{
				var paymentDeductions = PaymentDeductions.View.SelectMultiBound(new object[] { payment }).Select(x => (PXResult<PRPaymentDeduct>)x).ToList();
				foreach (PRPaymentDeduct paymentDeduction in paymentDeductions)
				{
					PRDeductCode deductCode = PXSelectorAttribute.Select<PRPaymentDeduct.codeID>(PaymentDeductions.Cache, paymentDeduction) as PRDeductCode;
					if (deductCode?.DedReportType != null)
					{
						usedDeductionReportingTypes.Add(deductCode.DedReportType.Value);
					}

					if (deductCode?.CntReportType != null)
					{
						usedDeductionReportingTypes.Add(deductCode.CntReportType.Value);
					}

					if (deductCode?.IsWorkersCompensation == true && !string.IsNullOrEmpty(deductCode?.State))
					{
						statesWithWC.Add(deductCode.State);
					}
				}
			}

			foreach (PRTypeMeta prType in usedDeductionReportingTypes.Select(x => deductionReportingTypes[x]).Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				PimRecord record = new PimRecord(prType.Name)
				{
					Description = prType.Description,
					AatrixTaxType = prType.ID
				};

				yield return record;
			}

			foreach (KeyValuePair<string, SymmetryToAatrixTaxMapping> kvp in _TaxIDMapping.Where(kvp => kvp.Value.IsWorkersCompensation && statesWithWC.Contains(kvp.Value.SubnationalEntity?.Abbr)))
			{
				int aatrixID = kvp.Value.TaxItemMappings.First().aatrixID;
				PimRecord record = new PimRecord(kvp.Key, aatrixID)
				{
					Description = PXMessages.LocalizeFormatNoPrefix(Messages.WorkersCompensationFormat, kvp.Value.SubnationalEntity.Abbr),
					AatrixTaxType = aatrixID
				};

				_WCMapping[kvp.Value.SubnationalEntity.Abbr] = record.PimID;

				yield return record;
			}
		}

		protected virtual IEnumerable<PimRecord> CreateTaxPimRecords(DatRecord reportPeriod, List<PXResult<PRPayment>> periodPayments)
		{
			foreach (IGrouping<int?, PXResult<PRPayment, PRPaymentTax, PRPaymentTaxSplit, PRTaxCode>> taxGroup in periodPayments
				.Select(x => (PXResult<PRPayment, PRPaymentTax, PRPaymentTaxSplit, PRTaxCode>)x)
				.Where(x => ((PRTaxCode)x).TaxID != null)
				.GroupBy(x => ((PRTaxCode)x).TaxID))
			{
				PRTaxCode tax = taxGroup.First();
				if (_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach (int aatrixTaxID in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings.Select(x => x.aatrixID).Except(_WCMapping.Values))
					{
						PimRecord record = aatrixTaxID == AufConstants.GenericLocalTaxMapping
							? new PimRecord(tax.TaxCD)
							: new PimRecord(tax.TaxCD, aatrixTaxID);
						record.Description = tax.Description;
						record.AatrixTaxType = aatrixTaxID;
						record.State = tax.TaxState;
						record.AccountNumber = tax.GovtRefNbr;

						yield return record;
					}
				}
			}
		}

		protected virtual IEnumerable<CsiRecord> CreateCsiRecords(IEnumerable<EmpRecord> empList, DateTime periodStart, DateTime periodEnd, DateTime checkDate)
		{
			foreach (IGrouping<int, EsiRecord> esiGroup in empList
				.SelectMany(x => x.GenList)
				.Where(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd && x.CheckDate == checkDate)
				.SelectMany(x => x.EsiList)
				.GroupBy(x => x.PimID))
			{
				CsiRecord record = new CsiRecord(checkDate, esiGroup.Key, esiGroup.First().State, periodStart, periodEnd);
				foreach (EsiRecord esi in esiGroup)
				{
					record.TotalWagesAndTips += esi.TotalWagesAndTips;
					record.TaxableWagesAndTips += esi.TaxableWagesAndTips;
					record.TaxableTips += esi.TaxableTips;
					record.WithholdingAmount += esi.WithholdingAmount;
					record.Hours += esi.Hours;
				}

				if (record.TaxableWagesAndTips.GetValueOrDefault() != 0)
				{
					record.Rate = record.WithholdingAmount / record.TaxableWagesAndTips * 100;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<CliRecord> CreateCliRecords(IEnumerable<EmpRecord> empList, DateTime periodStart, DateTime periodEnd, DateTime checkDate)
		{
			foreach (IGrouping<int, EliRecord> eliGroup in empList
				.SelectMany(x => x.GenList)
				.Where(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd && x.CheckDate == checkDate)
				.SelectMany(x => x.EliList)
				.GroupBy(x => x.PimID))
			{
				CliRecord record = new CliRecord(checkDate, eliGroup.Key, eliGroup.First().State, periodStart, periodEnd);
				foreach (EliRecord eli in eliGroup)
				{
					record.TotalWagesAndTips += eli.TotalWagesAndTips;
					record.TaxableWagesAndTips += eli.TaxableWagesAndTips;
					record.TaxableTips += eli.TaxableTips;
					record.WithholdingAmount += eli.WithholdingAmount;
					record.Hours += eli.Hours;
				}

				if (record.TaxableWagesAndTips.GetValueOrDefault() != 0)
				{
					record.Rate = record.WithholdingAmount / record.TaxableWagesAndTips * 100;
				}

				yield return record;
			}
		}

		protected virtual IEnumerable<EsiRecord> CreateEsiRecords(List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes, List<PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> paymentTaxSplits)
		{
			Dictionary<int, EsiRecord> esiRecords = new Dictionary<int, EsiRecord>();
			List<PXResult<PRPaymentEarning>> paymentEarnings = PaymentEarnings.Select().ToList();

			CreateEarningEsiRecords(ref esiRecords, employeeAttributes, paymentEarnings);
			CreateBenefitEsiRecords(ref esiRecords, employeeAttributes, paymentEarnings);
			CreateTaxEsiRecords(ref esiRecords, employeeAttributes, paymentTaxSplits);
			CreateWCEsiRecords(ref esiRecords, employeeAttributes);
			return esiRecords.Values;
		}

		protected virtual void CreateEarningEsiRecords(ref Dictionary<int, EsiRecord> records, List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes, List<PXResult<PRPaymentEarning>> paymentEarnings)
		{
			Dictionary<int, List<PXResult<PRPaymentEarning, EPEarningType>>> earningsByReportType = new Dictionary<int, List<PXResult<PRPaymentEarning, EPEarningType>>>();
			foreach (PXResult<PRPaymentEarning, EPEarningType> result in paymentEarnings)
			{
				PRPaymentEarning earning = (PRPaymentEarning)result;
				PREarningType earningType = ((EPEarningType)result).GetExtension<PREarningType>();

				if (earning.Amount.GetValueOrDefault() != 0 && earningType.ReportType.HasValue)
				{
					List<PXResult<PRPaymentEarning, EPEarningType>> earningsForReportType;
					if (!earningsByReportType.TryGetValue(earningType.ReportType.Value, out earningsForReportType))
					{
						earningsForReportType = new List<PXResult<PRPaymentEarning, EPEarningType>>();
						earningsByReportType[earningType.ReportType.Value] = earningsForReportType;
					}
					earningsForReportType.Add(result);
				}
			}

			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRWage>(PRCountryAttribute.GetPayrollCountry()).Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				List<PXResult<PRPaymentEarning, EPEarningType>> earningsForReportType;
				if (earningsByReportType.TryGetValue(prType.ID, out earningsForReportType))
				{
					int pimID = PimRecord.GetPimIDFromName(prType.Name);
					if (!records.TryGetValue(pimID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, pimID, null, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(GetAttributeValue(employeeAttributes, AatrixField.ESI.PuertoRicoTotalAllowances) ?? "0.0")
						};
						records[pimID] = record;
					}

					record.WithholdingAmount += earningsForReportType.Sum(x => ((PRPaymentEarning)x).Amount);
					foreach (PXResult<PRPaymentEarning, EPEarningType> paymentEarning in paymentEarnings)
					{
						record.TotalWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						record.TaxableWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						if (IsTipWage((EPEarningType)paymentEarning))
						{
							record.TaxableTips += ((PRPaymentEarning)paymentEarning).Amount;
						}
						record.Hours += ((PRPaymentEarning)paymentEarning).Hours;
						if (((PRPaymentEarning)paymentEarning).TypeCD == PRSetupMaint.GetEarningTypeFromSetup<PRSetup.commissionType>(this))
						{
							record.Commissions += ((PRPaymentEarning)paymentEarning).Amount;
						}
					}
				}
			}
		}

		protected virtual void CreateBenefitEsiRecords(ref Dictionary<int, EsiRecord> records, List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes, List<PXResult<PRPaymentEarning>> paymentEarnings)
		{
			Dictionary<int, decimal> applicableAmountsByBenefitType = new Dictionary<int, decimal>();
			var paymentDeductions = PaymentDeductions.Select().ToList();

			foreach (PXResult<PRPaymentDeduct, PRDeductCode> dedBen in paymentDeductions)
			{
				PRPaymentDeduct deduct = (PRPaymentDeduct)dedBen;
				PRDeductCode deductCode = (PRDeductCode)dedBen;

				if (deductCode.DedReportType.HasValue)
				{
					PX.Payroll.TaxCategory? reportTypeScope = PRReportingTypeSelectorAttribute.GetReportingTypeScope<PRBenefit>(deductCode.DedReportType.Value, PRCountryAttribute.GetPayrollCountry());
					if ((reportTypeScope == Payroll.TaxCategory.Employee || reportTypeScope == Payroll.TaxCategory.Any) && deduct.DedAmount.GetValueOrDefault() != 0)
					{
						if (applicableAmountsByBenefitType.ContainsKey(deductCode.DedReportType.Value))
						{
							applicableAmountsByBenefitType[deductCode.DedReportType.Value] += deduct.DedAmount.Value;
						}
						else
						{
							applicableAmountsByBenefitType[deductCode.DedReportType.Value] = deduct.DedAmount.Value;
						}
					}
				}

				if (deductCode.CntReportType.HasValue)
				{
					PX.Payroll.TaxCategory? reportTypeScope = PRReportingTypeSelectorAttribute.GetReportingTypeScope<PRBenefit>(deductCode.CntReportType.Value, PRCountryAttribute.GetPayrollCountry());
					if ((reportTypeScope == Payroll.TaxCategory.Employer || reportTypeScope == Payroll.TaxCategory.Any) && deduct.CntAmount.GetValueOrDefault() != 0)
					{
						if (applicableAmountsByBenefitType.ContainsKey(deductCode.CntReportType.Value))
						{
							applicableAmountsByBenefitType[deductCode.CntReportType.Value] += deduct.CntAmount.Value;
						}
						else
						{
							applicableAmountsByBenefitType[deductCode.CntReportType.Value] = deduct.CntAmount.Value;
						}
					}
				}
			}

			foreach (PRTypeMeta prType in PRReportingTypeSelectorAttribute.GetAll<PRBenefit>(PRCountryAttribute.GetPayrollCountry()).Where(x => x.AatrixMapping?.IsDirectMapping == true))
			{
				decimal dedBenAmount;
				if (applicableAmountsByBenefitType.TryGetValue(prType.ID, out dedBenAmount))
				{
					int pimID = PimRecord.GetPimIDFromName(prType.Name);
					if (!records.TryGetValue(pimID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, pimID, null, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(GetAttributeValue(employeeAttributes, AatrixField.ESI.PuertoRicoTotalAllowances) ?? "0.0")
						};
						records[pimID] = record;
					}

					record.WithholdingAmount += dedBenAmount;
					foreach (PXResult<PRPaymentEarning, EPEarningType> paymentEarning in paymentEarnings)
					{
						record.TotalWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						record.TaxableWagesAndTips += ((PRPaymentEarning)paymentEarning).Amount;
						if (IsTipWage((EPEarningType)paymentEarning))
						{
							record.TaxableTips += ((PRPaymentEarning)paymentEarning).Amount;
						}
						record.Hours += ((PRPaymentEarning)paymentEarning).Hours;
						if (((PRPaymentEarning)paymentEarning).TypeCD == PRSetupMaint.GetEarningTypeFromSetup<PRSetup.commissionType>(this))
						{
							record.Commissions += ((PRPaymentEarning)paymentEarning).Amount;
						}
					}
				}
			}
		}

		protected virtual void CreateTaxEsiRecords(ref Dictionary<int, EsiRecord> records, List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes,
			List<PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> paymentTaxSplits)
		{
			string payPeriodId = EmployeePayments.Current.PayPeriodID;
			bool paymentPeriodAlreadyTreated = this._PaymentPeriods.Contains(payPeriodId);
			if (!paymentPeriodAlreadyTreated)
			{
				this._PaymentPeriods.Add(payPeriodId);
			}

			
			foreach (IGrouping<int?, PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> taxGroup in paymentTaxSplits
				.GroupBy(x => ((PRTaxCode)x).TaxID))
			{
				PRPaymentTax paymentTax = taxGroup.First();
				PRTaxCode tax = taxGroup.First();

				if (tax.JurisdictionLevel == TaxJurisdiction.State && _TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach ((int aatrixTaxID, bool _) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!records.TryGetValue(aatrixTaxID, out EsiRecord record))
						{
							record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, aatrixTaxID, tax.TaxState, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate, paymentPeriodAlreadyTreated)
							{
								Allowances = decimal.Parse(GetAttributeValue(employeeAttributes, AatrixField.ESI.PuertoRicoTotalAllowances) ?? "0.0")
							};
							records[aatrixTaxID] = record;
						}

						record.TotalWagesAndTips += paymentTax.WageBaseGrossAmt;
						record.TaxableWagesAndTips += paymentTax.WageBaseAmount;
						record.TaxableTips += taxGroup.Select(x => (PRPaymentTaxSplit)x)
							.Where(x => IsTipWage(x.WageType))
							.Sum(x => x.WageBaseAmount.GetValueOrDefault());

						if (ShouldIncludeTaxAmountInWithholdingAmount(paymentTax))
						{
						record.WithholdingAmount += paymentTax.TaxAmount;
						}
						
						record.Hours += paymentTax.WageBaseHours;
					}
				}
			}
		}

		protected virtual void CreateWCEsiRecords(ref Dictionary<int, EsiRecord> records, List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes)
		{
			var paymentWCPremiums = PaymentWCPremiums.Select().ToList();
			foreach (PXResult<PRPaymentWCPremium, PRDeductCode> result in paymentWCPremiums)
			{
				PRPaymentWCPremium premium = result;
				PRDeductCode deductCode = result;

				if (_WCMapping.TryGetValue(deductCode.State, out int aatrixTaxID))
				{
					if (!records.TryGetValue(aatrixTaxID, out EsiRecord record))
					{
						record = new EsiRecord(EmployeePayments.Current.TransactionDate.Value, aatrixTaxID, deductCode.State, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate)
						{
							Allowances = decimal.Parse(GetAttributeValue(employeeAttributes, AatrixField.ESI.PuertoRicoTotalAllowances) ?? "0.0")
						};
						records[aatrixTaxID] = record;
					}

					record.TotalWagesAndTips += premium.WageBaseAmount;
					record.TaxableWagesAndTips += premium.WageBaseAmount;
					record.WithholdingAmount += premium.Amount.GetValueOrDefault() + premium.DeductionAmount.GetValueOrDefault();
					record.Hours += premium.WageBaseHours;
				}
			}
		}

		protected virtual IEnumerable<EliRecord> CreateTaxEliRecords(List<PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> paymentTaxSplits)
		{
			List<PXResult<PREmployeeTaxAttribute, PRTaxCodeAttribute>> employeeTaxAttributes = EmployeeTaxAttributes.Select().Select(x => (PXResult<PREmployeeTaxAttribute, PRTaxCodeAttribute>)x).ToList();

			Dictionary<int, EliRecord> records = new Dictionary<int, EliRecord>();
			foreach (IGrouping<int?, PXResult<PRPaymentTaxSplit, PRPaymentTax, PRTaxCode>> taxGroup in paymentTaxSplits
				.GroupBy(x => ((PRTaxCode)x).TaxID))
			{
				PRPaymentTax paymentTax = taxGroup.First();
				PRTaxCode tax = taxGroup.First();
				bool isResident = bool.Parse(GetAttributeValue(employeeTaxAttributes, AatrixField.ELI.IsResidentTax, tax.TaxID) ?? false.ToString());

				if (tax.JurisdictionLevel != TaxJurisdiction.State &&
					tax.JurisdictionLevel != TaxJurisdiction.Federal && 
					_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					bool residentSplitTax = TaxIsSplitByResidency(tax.TaxUniqueCode);
					foreach ((int aatrixTaxID, bool isResidentTax) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!residentSplitTax || isResident == isResidentTax)
						{
							int pimID = aatrixTaxID == AufConstants.GenericLocalTaxMapping ? PimRecord.GetPimIDFromName(tax.TaxCD) : aatrixTaxID;

							if (!records.TryGetValue(pimID, out EliRecord record))
							{
								record = new EliRecord(EmployeePayments.Current.TransactionDate.Value, pimID, tax.TaxState, EmployeePayments.Current.StartDate, EmployeePayments.Current.EndDate);
								record.IsResidentTax = isResidentTax;
								records[pimID] = record;
							}

							record.TotalWagesAndTips += paymentTax.WageBaseGrossAmt;
							record.TaxableWagesAndTips += paymentTax.WageBaseAmount;
							record.TaxableTips += taxGroup.Select(x => (PRPaymentTaxSplit)x)
								.Where(x => IsTipWage(x.WageType))
								.Sum(x => x.WageBaseAmount.GetValueOrDefault());
							record.WithholdingAmount += paymentTax.TaxAmount;
							record.Hours += paymentTax.WageBaseHours; 
						}
					}
				}
			}

			return records.Values;
		}

		protected virtual IEnumerable<CfbRecord> CreateCompanyCertifiedBenefitRecords(DatRecord reportPeriod)
		{
			HashSet<int?> reportedBenefits = new HashSet<int?>();
			foreach (IGrouping<int?, PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode, BAccount, Address, Contact>> resultGroup in
				PeriodCertifiedBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode, BAccount, Address, Contact>)x)
				.GroupBy(x => ((PRDeductCode)x).CodeID))
			{
				PRDeductCode benefit = resultGroup.First();
				CfbRecord record = CreateCfbRecordWithVendor(benefit, resultGroup.First(), resultGroup.First());
				reportedBenefits.Add(benefit.CodeID);
				yield return record;
			}

			foreach (IGrouping<int?, PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>> resultGroup in PeriodFringeDestinationBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>)x)
				.Where(x => !reportedBenefits.Contains(((PRDeductCode)x).CodeID))
				.GroupBy(x => ((PRDeductCode)x).CodeID))
			{
				PRDeductCode benefit = resultGroup.First();
				CfbRecord record = CreateCfbRecordWithVendor(benefit, resultGroup.First(), resultGroup.First());
				reportedBenefits.Add(benefit.CodeID);
				yield return record;
			}
		}

		protected virtual CfbRecord CreateCfbRecordWithVendor(PRDeductCode benefit, Address vendorAddress, Contact vendorContact)
		{
			return new CfbRecord(benefit.CodeCD, (char)benefit.CertifiedReportType)
				{
					VendorAddress = ConcatAddress(vendorAddress),
					VendorCity = vendorAddress?.City,
					VendorState = vendorAddress?.State,
					VendorZipCode = vendorAddress?.PostalCode,
					VendorPhone = vendorContact?.Phone1
				};
		}

		protected virtual IEnumerable<EfbRecord> CreateEmployeeCertifiedBenefitRecords(DatRecord reportPeriod)
		{
			foreach (var resultGroup in PeriodCertifiedBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
					.Select(x => (PXResult<PRPaymentFringeBenefitDecreasingRate, PRPayment, PRDeductCode>)x)
					.Where(x => ((PRPayment)x).EmployeeID == PeriodEmployees.Current.BAccountID)
					.GroupBy(x => new { ((PRDeductCode)x).CodeID }))
			{
				PRPaymentFringeBenefitDecreasingRate fringeBenefit = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				yield return new EfbRecord(deductCode.CodeCD)
				{
					HourlyRate = fringeBenefit.BenefitRate,
					FBFilter = fringeBenefit.ProjectID
				};
				}

			foreach (var resultGroup in PeriodFringeDestinationBenefits.Select(reportPeriod.FirstDate, reportPeriod.LastDate)
				.Select(x => (PXResult<PRPaymentFringeBenefit, PRPayment, PMProject, PRDeductCode, BAccount, Address, Contact>)x)
				.ToList()
				.Where(x => ((PRPayment)x).EmployeeID == PeriodEmployees.Current.BAccountID && ((PRPaymentFringeBenefit)x).FringeAmountInBenefit != 0 && ((PRPaymentFringeBenefit)x).ProjectHours != 0)
				.GroupBy(x => new { ((PRDeductCode)x).CodeID, ((PRPaymentFringeBenefit)x).ProjectID }))
			{
				PRPaymentFringeBenefit fringeBenefit = resultGroup.First();
				PRDeductCode deductCode = resultGroup.First();
				yield return new EfbRecord(deductCode.CodeCD)
				{
					HourlyRate = resultGroup.Sum(x => ((PRPaymentFringeBenefit)x).FringeAmountInBenefit) / fringeBenefit.ProjectHours,
					FBFilter = fringeBenefit.ProjectID
				};
			}
		}

		protected virtual IEnumerable<CspRecord> CreateCspList(DateTime checkDate)
		{
			Dictionary<int, CspRecord> records = new Dictionary<int, CspRecord>(); ;
			foreach (PXResult<PRTaxDetail, APInvoice, APAdjust, APPayment, PRTaxCode> result in PaymentSettledTaxes.Select(checkDate))
			{
				APPayment apPayment = (APPayment)result;
				PRTaxCode tax = (PRTaxCode)result;
				PRTaxDetail taxDetail = (PRTaxDetail)result;

				if (taxDetail.Amount.GetValueOrDefault() != 0 && tax.JurisdictionLevel == TaxJurisdiction.State && _TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					foreach ((int aatrixTaxID, bool _) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						CspRecord record;
						if (!records.TryGetValue(aatrixTaxID, out record))
						{
							record = new CspRecord(checkDate, aatrixTaxID)
							{
								State = tax.TaxState,
								PaymentAmount = 0m,
								PaymentDate = apPayment.AdjDate
							};

							records[aatrixTaxID] = record;
						}

						record.PaymentAmount += taxDetail.Amount;
					}
				}
			}

			return records.Values;
		}

		protected virtual IEnumerable<ClpRecord> CreateClpList(DateTime checkDate)
		{
			Dictionary<int, ClpRecord> records = new Dictionary<int, ClpRecord>(); ;
			foreach (PXResult<PRTaxDetail, APInvoice, APAdjust, APPayment, PRTaxCode, PRPayment> result in PaymentSettledTaxes.Select(checkDate))
			{
				APPayment apPayment = (APPayment)result;
				PRTaxCode tax = (PRTaxCode)result;
				PRTaxDetail taxDetail = (PRTaxDetail)result;
				PRPayment prPayment = (PRPayment)result;

				if (taxDetail.Amount.GetValueOrDefault() != 0 &&
					tax.JurisdictionLevel != TaxJurisdiction.State &&
					tax.JurisdictionLevel != TaxJurisdiction.Federal &&
					_TaxIDMapping[tax.TaxUniqueCode]?.TaxItemMappings != null)
				{
					bool residentSplitTax = TaxIsSplitByResidency(tax.TaxUniqueCode);
					bool isEmployeeResident = bool.Parse(GetAttributeValue(
						EmployeeTaxAttributes.Select(prPayment.EmployeeID).Select(x => (PXResult<PREmployeeTaxAttribute, PRTaxCodeAttribute>)x).ToList(),
						AatrixField.ELI.IsResidentTax,
						tax.TaxID) ?? false.ToString());
					foreach ((int aatrixTaxID, bool isResidentTax) in _TaxIDMapping[tax.TaxUniqueCode].TaxItemMappings)
					{
						if (!residentSplitTax || isEmployeeResident == isResidentTax)
						{
							ClpRecord record;
							if (!records.TryGetValue(aatrixTaxID, out record))
							{
								record = new ClpRecord(checkDate, aatrixTaxID)
								{
									State = tax.TaxState,
									PaymentAmount = 0m,
									PaymentDate = apPayment.AdjDate
								};

								records[aatrixTaxID] = record;
							}

							record.PaymentAmount += taxDetail.Amount; 
						}
					}
				}
			}

			return records.Values;
		}
		
		protected virtual (EcvRecord, EciRecord) CreateEmployeeAcaRecords(EmpRecord emp)
		{
			PXResultset<PRAcaEmployeeMonthlyInformation> employeeMonthlyRecords = AcaEmployeeMonthlyInformation.Select();
			if (employeeMonthlyRecords.Any())
			{
				List<PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>> employeeAttributes = EmployeeAttributes.Select().Select(x => (PXResult<PREmployeeAttribute, PRCompanyTaxAttribute>)x).ToList();

				string policyOriginCode = GetAttributeValue(employeeAttributes, AatrixField.ECV.AcaPolicyOrigin);
				EcvRecord ecv = new EcvRecord()
				{
					ElectronicOnly = bool.Parse(GetAttributeValue(employeeAttributes, AatrixField.ECV.ElectronicAcaForms) ?? "false"),
					PolicyOriginCode = policyOriginCode != null && policyOriginCode.Length > 0 ? policyOriginCode[0] : (char?)null,
					SelfInsuredEmployee = bool.Parse(GetAttributeValue(employeeAttributes, AatrixField.ECV.EmployeeSelfInsured) ?? "false")
				};

				EciRecord eci = new EciRecord(emp.EmployeeID)
				{
					SocialSecurityNumber = emp.SocialSecurityNumber,
					BirthDate = emp.BirthDate,
					FirstName = emp.FirstName,
					MiddleName = emp.MiddleName,
					LastName = emp.LastName,
					NameSuffix = emp.NameSuffix
				};

				ecv.PlanStartMonth = 0;
				foreach (PRAcaEmployeeMonthlyInformation employeeMonth in employeeMonthlyRecords)
				{
					int monthIndex = employeeMonth.Month.Value - 1;

					ecv.OfferOfCoverageCode[monthIndex] = employeeMonth.OfferOfCoverage;
					ecv.MinimumIndividualContribution[monthIndex] = employeeMonth.MinimumIndividualContribution;
					ecv.SafeHarborCode[monthIndex] = employeeMonth.Section4980H;
					eci.CoverageIndicator[monthIndex] = AcaOfferOfCoverage.MeetsMinimumCoverageRequirement(employeeMonth.OfferOfCoverage);
					if (eci.CoverageIndicator[monthIndex] == true && (ecv.PlanStartMonth == 0 || ecv.PlanStartMonth > employeeMonth.Month.Value))
					{
						ecv.PlanStartMonth = employeeMonth.Month.Value;
					}
				}

				return (ecv, eci);
			}

			return (null, null);
		}

		protected virtual string ConcatAddress(IAddressBase address)
		{
			if (address == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(address.AddressLine2))
			{
				return address.AddressLine1;
			}

			return address.AddressLine1 + ' ' + address.AddressLine2;
		}

		protected virtual decimal? GetHoursForDay(EPEarningType earningType, PREarningDetail earningDetail, DateTime periodStart, int dayOffset, bool overtime)
		{
			if (earningDetail.IsFringeRateEarning == true || earningDetail.Hours == null)
			{
				return 0m;
			}

			bool overtimeCondition;
			if (overtime)
			{
				overtimeCondition = earningType.IsOvertime == true;
			}
			else
			{
				overtimeCondition = earningType.IsOvertime != true;
			}

			return (overtimeCondition && earningDetail.Date.Value.Date == (periodStart + new TimeSpan(dayOffset, 0, 0, 0)).Date) ? earningDetail.Hours : 0m;
		}

		private bool TaxIsSplitByResidency(string uniqueTaxID)
		{
			List<(int _, bool isResidentTax)> aatrixMappings = _TaxIDMapping[uniqueTaxID]?.TaxItemMappings.ToList();
			return aatrixMappings != null && aatrixMappings.Count == 2 && aatrixMappings[0].isResidentTax != aatrixMappings[1].isResidentTax;
		}
		
		protected virtual bool ShouldGenerateAcaRecords()
		{
			return Report.Current.ReportType == PRGovernmentReport.AcaReportType && AcaCompanyYearlyInformation.SelectSingle() != null;
		}

		protected virtual bool ShouldGenerateCertifiedRecords()
		{
			return Report.Current.ReportType == PRGovernmentReport.CertifiedReportType;
		}

		protected virtual bool ShouldIncludeTaxAmountInWithholdingAmount(PRPaymentTax paymentTax)
		{
			return !(paymentTax.TaxCategory == TaxCategory.EmployerTax &&
				(Report.Current.ReportType == PRGovernmentReport.W2ReportType || Report.Current.ReportType == PRGovernmentReport.Federal941ReportType));
		}

		protected virtual string GetAttributeValue<TEmployee, TCompany>(List<PXResult<TEmployee, TCompany>> employeeAttributes, int aatrixMapping, bool matchState = false)
			where TEmployee : class, IPRSetting, IBqlTable, new()
			where TCompany : class, IPRSetting, IBqlTable, IStateSpecific, new()
		{
			PXResult<TEmployee, TCompany> attributeResult =
				employeeAttributes.FirstOrDefault(x => ((TCompany)x).AatrixMapping == aatrixMapping && (!matchState || ((TCompany)x).State == Report.Current.State));
			return attributeResult != null ? GetAttributeValue(attributeResult) : null;
		}

		protected virtual string GetAttributeValue(List<PXResult<PREmployeeTaxAttribute, PRTaxCodeAttribute>> employeeAttributes, int aatrixMapping, int? taxID)
		{
			return GetAttributeValue(employeeAttributes.Where(x => ((PRTaxCodeAttribute)x).TaxID == taxID).ToList(), aatrixMapping);
		}

		protected virtual string GetAttributeValue<TEmployee, TCompany>(PXResult<TEmployee, TCompany> attributeResult)
			where TEmployee : class, IPRSetting, IBqlTable, new()
			where TCompany : class, IPRSetting, IBqlTable, new()
		{
			TEmployee employeeAttribute = attributeResult;
			TCompany companyAttribute = attributeResult;
			return employeeAttribute?.UseDefault == false ? employeeAttribute?.Value : companyAttribute?.Value;
		}

		protected virtual bool IsApplicableAatrixPredefinedField<TSetting>(TSetting setting)
			where TSetting : IPRSetting, IStateSpecific
		{
			return setting.AatrixMapping >= WebserviceContants.FirstAatrixPredefinedField
				&& setting.AatrixMapping <= WebserviceContants.LastAatrixPredefinedField
				&& setting.State == Report.Current.State;
		}

		protected virtual bool IsTipWage(EPEarningType earningType)
		{
			return IsTipWage(earningType.GetExtension<PREarningType>().WageTypeCD);
		}

		protected virtual bool IsTipWage(int? wageType)
		{
			return PRTypeSelectorAttribute.GetAatrixMapping<PRWage>(
				wageType.GetValueOrDefault(),
				PRCountryAttribute.GetPayrollCountry()) == AatrixMiscInfo.IsTipWageType;
		}
		#endregion Helpers

		#region Helper classes
		public class PeriodPaymentQuery : SelectFrom<PRPayment>
			.LeftJoin<PRPaymentTax>.On<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr>
				.And<PRPaymentTax.docType.IsEqual<PRPayment.docType>>>
			.LeftJoin<PRPaymentTaxSplit>.On<PRPaymentTaxSplit.refNbr.IsEqual<PRPayment.refNbr>
				.And<PRPaymentTaxSplit.docType.IsEqual<PRPayment.docType>>
				.And<PRPaymentTaxSplit.taxID.IsEqual<PRPaymentTax.taxID>>>
			.LeftJoin<PRTaxCode>.On<PRTaxCode.taxID.IsEqual<PRPaymentTax.taxID>
				.And<MatchPRCountry<PRTaxCode.countryID>>>
			.Where<Brackets<PRPayment.transactionDate.IsGreaterEqual<P.AsDateTime.UTC>
				.And<PRPayment.transactionDate.IsLessEqual<P.AsDateTime.UTC>>
					.And<PRGovernmentReport.reportType.FromCurrent.IsNotEqual<certifiedReportType>>
					.Or<PRPayment.startDate.IsGreaterEqual<P.AsDateTime.UTC>
						.And<PRPayment.endDate.IsLessEqual<P.AsDateTime.UTC>>
						.And<PRGovernmentReport.reportType.FromCurrent.IsEqual<certifiedReportType>>>>
				.And<PRPayment.released.IsEqual<True>>
				.And<PRPayment.voided.IsEqual<False>>
				.And<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>
				.And<Where<PRPayment.branchID, Inside<BAccountR.bAccountID.FromCurrent>>>>
			.OrderBy<PRPayment.transactionDate.Asc>.View
		{
			public PeriodPaymentQuery(PXGraph graph) : base(graph) { }
		}

		public class EmployeePaymentQuery : PeriodPaymentQuery
		{
			public EmployeePaymentQuery(PXGraph graph) : base(graph)
			{
				WhereAnd(typeof(Where<PRPayment.employeeID.IsEqual<EPEmployee.bAccountID.FromCurrent>>));	
			}
		}
		#endregion

	}
}
