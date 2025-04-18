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

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores different options as well as the sources of the account that apply to the Payroll functional area. The information will be displayed on the Payroll Preferences (PR101000) form.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PRSetup)]
	public partial class PRSetup : IBqlTable
	{
		#region Keys
		public static class FK
		{
			public class BatchNumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<batchNumberingID> { }
			public class PayrollBatchNumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<batchNumberingCD> { }
			public class TransactionNumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<tranNumberingCD> { }
			public class ROENumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<roeNumberingCD> { }
			public class BatchForSubmissionNumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<batchForSubmissionNumberingCD> { }
			public class PTOAdjustmentNumberingSequence : Numbering.PK.ForeignKeyOf<PRSetup>.By<ptoAdjustmentNumberingCD> { }
			public class RegularEarningType : EPEarningType.PK.ForeignKeyOf<PRSetup>.By<regularHoursType> { }
			public class HolidaysEarningType : EPEarningType.PK.ForeignKeyOf<PRSetup>.By<holidaysType> { }
			public class CommissionEarningType : EPEarningType.PK.ForeignKeyOf<PRSetup>.By<commissionType> { }
		}
		#endregion

		#region BatchNumberingID
		public abstract class batchNumberingID : IBqlField
		{
		}
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region BatchNumberingCD
		public abstract class batchNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Payroll Batch Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string BatchNumberingCD { get; set; }
		#endregion
		#region TranNumberingCD
		public abstract class tranNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Transaction Numbering Sequence")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string TranNumberingCD { get; set; }
		#endregion
		#region ROENumberingCD
		public abstract class roeNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "ROE Numbering Sequence", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string ROENumberingCD { get; set; }
		#endregion
		#region BatchForSubmissionNumberingCD
		public abstract class batchForSubmissionNumberingCD : PX.Data.IBqlField { }
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Batch for Submission Numbering Sequence", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string BatchForSubmissionNumberingCD { get; set; }
		#endregion
		#region PTOAdjustmentNumberingCD
		/// <summary>
		/// The identifier of the numbering sequence to be used for PTO adjustments.
		/// </summary>
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "PTO Adjustment Numbering Sequence", Visible = false)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		public virtual string PTOAdjustmentNumberingCD { get; set; }
		public abstract class ptoAdjustmentNumberingCD : PX.Data.IBqlField { }
		#endregion
		#region UpdateGL
		public abstract class updateGL : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Update GL")]
		[PXDefault(false)]
		public virtual bool? UpdateGL { get; set; }
		#endregion
		#region SummPost
		public abstract class summPost : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Summary on Updating GL")]
		public virtual bool? SummPost { get; set; }
		#endregion
		#region AutoPost
		public abstract class autoPost : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Post on Release")]
		public virtual bool? AutoPost { get; set; }
		#endregion
		#region DisableGLWarnings
		public abstract class disableGLWarnings : PX.Data.IBqlField { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Disable GL Account Warnings on Payment Release")]
		public virtual bool? DisableGLWarnings { get; set; }
		#endregion
		#region PayPeriodDateChangeAllowed
		public abstract class payPeriodDateChangeAllowed : PX.Data.BQL.BqlBool.Field<payPeriodDateChangeAllowed> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Allow Changing Pay Period Dates")]
		[PXDefault(false)]
		public virtual bool? PayPeriodDateChangeAllowed { get; set; }
		#endregion
		#region PayRateDecimalPlaces
		public abstract class payRateDecimalPlaces : PX.Data.IBqlField { }
		[PXDBShort(MinValue = 0, MaxValue = 6)]
		[PXDefault((short)2)]
		[PXUIField(DisplayName = "Pay Rate Decimal Places")]
		public virtual short? PayRateDecimalPlaces { get; set; }
		#endregion
		#region EarningsAcctDefault
		public abstract class earningsAcctDefault : PX.Data.BQL.BqlString.Field<earningsAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Earnings Account from")]
		[PREarningsAcctSubDefault.AcctList]
		[PXDefault(PREarningsAcctSubDefault.MaskEarningType)]
		public virtual String EarningsAcctDefault { get; set; }
		#endregion
		#region EarningsSubMask
		public abstract class earningsSubMask : PX.Data.BQL.BqlString.Field<earningsSubMask> { }
		[PXDefault]
		[PREarningsSubAccountMask(DisplayName = "Combine Earnings Sub. From")]
		public virtual String EarningsSubMask { get; set; }
		#endregion
		#region EarningsAlternateAcctDefault
		public abstract class earningsAlternateAcctDefault : PX.Data.BQL.BqlString.Field<earningsAlternateAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Fallback Account Source")]
		[PREarningsAcctSubDefault.AlternateList]
		[PXDefault(PREarningsAcctSubDefault.MaskEarningType)]
		[PXUIRequiredIfVisible]
		public virtual string EarningsAlternateAcctDefault { get; set; }
		#endregion
		#region EarningsAlternateSubMask
		public abstract class earningsAlternateSubMask : PX.Data.BQL.BqlString.Field<earningsAlternateSubMask> { }
		[PXDefault]
		[PXUIRequiredIfVisible]
		[PREarningsAlternateSubAccountMask(typeof(earningsSubMask), DisplayName = "Fallback Sub. Source")]
		public virtual string EarningsAlternateSubMask { get; set; }
		#endregion
		#region DeductLiabilityAcctDefault
		public abstract class deductLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<deductLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Deduction Liability Account from")]
		[PRDeductAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		public virtual String DeductLiabilityAcctDefault { get; set; }
		#endregion
		#region DeductLiabilitySubMask
		public abstract class deductLiabilitySubMask : PX.Data.BQL.BqlString.Field<deductLiabilitySubMask> { }
		[PXDefault]
		[PRDeductSubAccountMask(DisplayName = "Combine Deduction Liability Sub. From")]
		public virtual String DeductLiabilitySubMask { get; set; }
		#endregion
		#region BenefitExpenseAcctDefault
		public abstract class benefitExpenseAcctDefault : PX.Data.BQL.BqlString.Field<benefitExpenseAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Benefit Expense Account from")]
		[PRBenefitExpenseAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		[ExpenseAcctSubVerifier(typeof(benefitExpenseSubMask), PRBenefitExpenseAcctSubDefault.MaskEarningType, PRBenefitExpenseAcctSubDefault.MaskLaborItem)]
		public virtual String BenefitExpenseAcctDefault { get; set; }
		#endregion
		#region BenefitExpenseSubMask
		public abstract class benefitExpenseSubMask : PX.Data.BQL.BqlString.Field<benefitExpenseSubMask> { }
		[PXDefault]
		[PRBenefitExpenseSubAccountMask(DisplayName = "Combine Benefit Expense Sub. From")]
		public virtual String BenefitExpenseSubMask { get; set; }
		#endregion
		#region BenefitExpenseAlternateAcctDefault
		public abstract class benefitExpenseAlternateAcctDefault : PX.Data.BQL.BqlString.Field<benefitExpenseAlternateAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Fallback Account Source")]
		[PRBenefitExpenseAcctSubDefault.AlternateList]
		[PXDefault(PRBenefitExpenseAcctSubDefault.MaskEarningType)]
		[PXUIRequiredIfVisible]
		public virtual string BenefitExpenseAlternateAcctDefault { get; set; }
		#endregion
		#region BenefitExpenseAlternateSubMask
		public abstract class benefitExpenseAlternateSubMask : PX.Data.BQL.BqlString.Field<benefitExpenseAlternateSubMask> { }
		[PXDefault]
		[PRBenefitExpenseAlternateSubAccountMask(typeof(benefitExpenseSubMask), DisplayName = "Fallback Sub. Source")]
		[PXUIRequiredIfVisible]
		public virtual string BenefitExpenseAlternateSubMask { get; set; }
		#endregion
		#region BenefitLiabilityAcctDefault
		public abstract class benefitLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<benefitLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Benefit Liability Account from")]
		[PRDeductAcctSubDefault.AcctList]
		[PXDefault(PRDeductAcctSubDefault.MaskDeductionCode)]
		public virtual String BenefitLiabilityAcctDefault { get; set; }
		#endregion
		#region BenefitLiabilitySubMask
		public abstract class benefitLiabilitySubMask : PX.Data.BQL.BqlString.Field<benefitLiabilitySubMask> { }
		[PXDefault]
		[PRDeductSubAccountMask(DisplayName = "Combine Benefit Liability Sub. From")]
		public virtual String BenefitLiabilitySubMask { get; set; }
		#endregion
		#region TaxExpenseAcctDefault
		public abstract class taxExpenseAcctDefault : PX.Data.BQL.BqlString.Field<taxExpenseAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Tax Expense Account from")]
		[PRTaxExpenseAcctSubDefault.AcctList]
		[PXDefault(PRTaxAcctSubDefault.MaskTaxCode)]
		[ExpenseAcctSubVerifier(typeof(taxExpenseSubMask), PRTaxExpenseAcctSubDefault.MaskEarningType, PRTaxExpenseAcctSubDefault.MaskLaborItem)]
		public virtual String TaxExpenseAcctDefault { get; set; }
		#endregion
		#region TaxExpenseSubMask
		public abstract class taxExpenseSubMask : PX.Data.BQL.BqlString.Field<taxExpenseSubMask> { }
		[PXDefault]
		[PRTaxExpenseSubAccountMask(DisplayName = "Combine Tax Expense Sub. From")]
		public virtual String TaxExpenseSubMask { get; set; }
		#endregion
		#region TaxExpenseAlternateAcctDefault
		public abstract class taxExpenseAlternateAcctDefault : PX.Data.BQL.BqlString.Field<taxExpenseAlternateAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Fallback Account Source")]
		[PRTaxExpenseAcctSubDefault.AlternateList]
		[PXDefault(PRTaxExpenseAcctSubDefault.MaskEarningType)]
		[PXUIRequiredIfVisible]
		public virtual string TaxExpenseAlternateAcctDefault { get; set; }
		#endregion
		#region TaxExpenseAlternateSubMask
		public abstract class taxExpenseAlternateSubMask : PX.Data.BQL.BqlString.Field<taxExpenseAlternateSubMask> { }
		[PXDefault]
		[PRTaxExpenseSubAlternateAccountMask(typeof(taxExpenseSubMask), DisplayName = "Fallback Sub. Source")]
		[PXUIRequiredIfVisible]
		public virtual string TaxExpenseAlternateSubMask { get; set; }
		#endregion
		#region TaxLiabilityAcctDefault
		public abstract class taxLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<taxLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use Tax Liability Account from")]
		[PRTaxAcctSubDefault.AcctList]
		[PXDefault(PRTaxAcctSubDefault.MaskTaxCode)]
		public virtual String TaxLiabilityAcctDefault { get; set; }
		#endregion
		#region TaxLiabilitySubMask
		public abstract class taxLiabilitySubMask : PX.Data.BQL.BqlString.Field<taxLiabilitySubMask> { }
		[PXDefault]
		[PRTaxSubAccountMask(DisplayName = "Combine Tax Liability Sub. From")]
		public virtual String TaxLiabilitySubMask { get; set; }
		#endregion
		#region PTOExpenseAcctDefault
		public abstract class ptoExpenseAcctDefault : PX.Data.BQL.BqlString.Field<ptoExpenseAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use PTO Expense Account From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOExpenseAcctSubDefault.AcctList]
		[PXDefault(PRPTOAcctSubDefault.MaskPTOBank, PersistingCheck = PXPersistingCheck.Nothing)]
		[ExpenseAcctSubVerifier(typeof(ptoExpenseSubMask), PRPTOExpenseAcctSubDefault.MaskEarningType, PRPTOExpenseAcctSubDefault.MaskLaborItem)]
		public virtual String PTOExpenseAcctDefault { get; set; }
		#endregion
		#region PTOExpenseSubMask
		public abstract class ptoExpenseSubMask : PX.Data.BQL.BqlString.Field<ptoExpenseSubMask> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PRPTOExpenseSubAccountMask(DisplayName = "Combine PTO Expense Sub. From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.subAccount>>))]
		public virtual String PTOExpenseSubMask { get; set; }
		#endregion
		#region PTOExpenseAlternateAcctDefault
		public abstract class ptoExpenseAlternateAcctDefault : PX.Data.BQL.BqlString.Field<ptoExpenseAlternateAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Fallback PTO Account Source", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOExpenseAcctSubDefault.AlternateList]
		[PXDefault(PRPTOExpenseAcctSubDefault.MaskEarningType, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequiredIfVisible]
		[ExpenseAcctSubVerifier(typeof(ptoExpenseSubMask), PRPTOExpenseAcctSubDefault.MaskEarningType, PRPTOExpenseAcctSubDefault.MaskLaborItem)]
		public virtual string PTOExpenseAlternateAcctDefault { get; set; }
		#endregion
		#region PTOExpenseAlternateSubMask
		public abstract class ptoExpenseAlternateSubMask : PX.Data.BQL.BqlString.Field<ptoExpenseAlternateSubMask> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PRPTOExpenseSubAlternateAccountMask(typeof(ptoExpenseSubMask), DisplayName = "Fallback PTO Sub. Source", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUIRequiredIfVisible]
		public virtual string PTOExpenseAlternateSubMask { get; set; }
		#endregion
		#region PTOLiabilityAcctDefault
		public abstract class ptoLiabilityAcctDefault : PX.Data.BQL.BqlString.Field<ptoLiabilityAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use PTO Liability Account From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOAcctSubDefault.AcctList]
		[PXDefault(PRPTOAcctSubDefault.MaskPTOBank, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String PTOLiabilityAcctDefault { get; set; }
		#endregion
		#region PTOLiabilitySubMask
		public abstract class ptoLiabilitySubMask : PX.Data.BQL.BqlString.Field<ptoLiabilitySubMask> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PRPTOSubAccountMask(DisplayName = "Combine PTO Liability Sub. From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.subAccount>>))]
		public virtual String PTOLiabilitySubMask { get; set; }
		#endregion
		#region PTOAssetAcctDefault
		public abstract class ptoAssetAcctDefault : PX.Data.BQL.BqlString.Field<ptoAssetAcctDefault> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Use PTO Asset Account From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PRPTOAcctSubDefault.AcctList]
		[PXDefault(PRPTOAcctSubDefault.MaskPTOBank, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String PTOAssetAcctDefault { get; set; }
		#endregion
		#region PTOAssetSubMask
		public abstract class ptoAssetSubMask : PX.Data.BQL.BqlString.Field<ptoAssetSubMask> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PRPTOSubAccountMask(DisplayName = "Combine PTO Asset Sub. From", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.subAccount>>))]
		public virtual String PTOAssetSubMask { get; set; }
		#endregion
		#region SummarizeTimeCard
		public abstract class summarizeTimeCard : PX.Data.BQL.BqlBool.Field<summarizeTimeCard> { }
		[Obsolete]
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Summarize Time Card Data", Visible = false)]
		public virtual bool? SummarizeTimeCard { get; set; }
		#endregion
		#region RegularHoursType
		public abstract class regularHoursType : PX.Data.BQL.BqlString.Field<regularHoursType> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<EPEarningType.isOvertime.IsNotEqual<True>>.
				And<PREarningType.isPiecework.IsNotEqual<True>>.
				And<PREarningType.isAmountBased.IsNotEqual<True>>.
				And<PREarningType.isPTO.IsNotEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Regular Hours Earning Type for Quick Pay")]
		public virtual string RegularHoursType { get; set; }
		#endregion
		#region HolidaysType
		public abstract class holidaysType : PX.Data.BQL.BqlString.Field<holidaysType> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>.
			Where<EPEarningType.isActive.IsEqual<True>.
				And<PREarningType.isPTO.IsEqual<True>>>), 
			DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Holiday Earning Type for Quick Pay")]
		public virtual string HolidaysType { get; set; }
		#endregion
		#region CommissionType
		public abstract class commissionType : PX.Data.BQL.BqlString.Field<commissionType> { }
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXUIField(DisplayName = "Commission Earning Type")]
		[PXSelector(typeof(SelectFrom<EPEarningType>.
				Where<PREarningType.isAmountBased.IsEqual<True>>.
				OrderBy<EPEarningType.typeCD.Asc>.
				SearchFor<EPEarningType.typeCD>), 
			typeof(EPEarningType.typeCD), typeof(EPEarningType.description), 
			SelectorMode = PXSelectorMode.MaskAutocomplete, 
			DescriptionField = typeof(EPEarningType.description))]
		public virtual string CommissionType { get; set; }
		#endregion
		#region EnablePieceworkEarningType
		public abstract class enablePieceworkEarningType : PX.Data.BQL.BqlBool.Field<enablePieceworkEarningType> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Piecework as an Earning Type")]
		public virtual bool? EnablePieceworkEarningType { get; set; }
		#endregion
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Hold Paycheck on Entry")]
		[PXDefault(false)]
		public virtual bool? HoldEntry { get; set; }
		#endregion
		#region NoWeekendTransactionDate
		public abstract class noWeekendTransactionDate : PX.Data.BQL.BqlBool.Field<noWeekendTransactionDate> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Transaction Date Cannot Be on Weekend")]
		public virtual bool? NoWeekendTransactionDate { get; set; }
		#endregion
		#region HideEmployeeInfo
		public abstract class hideEmployeeInfo : PX.Data.BQL.BqlBool.Field<hideEmployeeInfo> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Hide Employee Name on Transactions")]
		[PXDefault(false)]
		public virtual bool? HideEmployeeInfo { get; set; }
		#endregion HideEmployeeInfo
		#region ProjectCostAssignment
		public abstract class projectCostAssignment : PX.Data.BQL.BqlString.Field<projectCostAssignment> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(ProjectCostAssignmentType.WageCostAssigned)]
		[PXUIField(DisplayName = "Project Cost Assignment")]
		[ProjectCostAssignmentType.List]
		public virtual string ProjectCostAssignment { get; set; }
		#endregion
		#region AutoReleaseOnPay
		public abstract class autoReleaseOnPay : PX.Data.BQL.BqlBool.Field<autoReleaseOnPay> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release on Payment")]
		public virtual bool? AutoReleaseOnPay { get; set; }
		#endregion
		#region TimePostingOption
		public abstract class timePostingOption : PX.Data.BQL.BqlString.Field<timePostingOption> { }
		[PXDBString(1, IsUnicode = false, IsFixed = true)]
		[PXDefault(EPPostOptions.DoNotPost)]
		[PRTimePostOptions.List]
		[PXUIField(DisplayName = "Time Posting Option", Required = true)]
		public virtual string TimePostingOption { get; set; }
		#endregion

		#region OffBalanceAccountGroupID
		public abstract class offBalanceAccountGroupID : PX.Data.BQL.BqlInt.Field<offBalanceAccountGroupID> { }
		[AccountGroup(typeof(Where<PMAccountGroup.type, Equal<PMAccountType.offBalance>>), DisplayName = "Off-Balance Account Group")]
		[PXUIVisible(typeof(Where<timePostingOption.IsEqual<EPPostOptions.postToOffBalance>.Or<timePostingOption.IsEqual<EPPostOptions.overridePMInPayroll>>>))]
		[PXFormula(typeof(Null.When<timePostingOption.IsNotEqual<EPPostOptions.postToOffBalance>.And<timePostingOption.IsNotEqual<EPPostOptions.overridePMInPayroll>>>.Else<Null>))]
		public virtual int? OffBalanceAccountGroupID { get; set; }
		#endregion
		#region UseBenefitRateFromUnionInCertProject
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that we will use the Benefit Rate from Union in Certified Project calculation.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Benefit Rate from Union in Certified Project Calculation", FieldClass = nameof(FeaturesSet.Construction))]
		public virtual bool? UseBenefitRateFromUnionInCertProject { get; set; }
		public abstract class useBenefitRateFromUnionInCertProject : PX.Data.BQL.BqlBool.Field<useBenefitRateFromUnionInCertProject> { }
		#endregion

		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion
	}
}
