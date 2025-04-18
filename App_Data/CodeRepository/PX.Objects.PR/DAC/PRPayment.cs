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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.Common.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information about the paycheck of a specific employee. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// It is the main document in the Payroll functional area.
	/// </summary>
	[PXCacheName(Messages.PRPayment)]
	[Serializable]
	[PXPrimaryGraph(typeof(PRPayChecksAndAdjustments))]
	[PXProjection(typeof(SelectFrom<PRPayment>
		.Where<MatchWithBranch<PRPayment.branchID>
			.And<MatchWithPayGroup<PRPayment.payGroupID>>
			.And<MatchPRCountry<PRPayment.countryID>>>),
		Persistent = true)]
	public class PRPayment : IBqlTable, IEmployeeType
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPayment>.By<docType, refNbr>
		{
			public static PRPayment Find(PXGraph graph, string docType, string refNbr, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, options);
		}

		public static class FK
		{
			public class PayGroup : PRPayGroup.PK.ForeignKeyOf<PRPayment>.By<payGroupID> { }
			public class PayPeriod : PRPayGroupPeriod.PK.ForeignKeyOf<PRPayment>.By<payGroupID, finPeriodID> { }
			public class Branch : GL.Branch.PK.ForeignKeyOf<PRPayment>.By<branchID> { }
			public class PaymentMethod : CA.PaymentMethod.PK.ForeignKeyOf<PRPayment>.By<paymentMethodID> { }
			public class CashAccount : CA.CashAccount.PK.ForeignKeyOf<PRPayment>.By<cashAccountID> { }
			public class Employee : PREmployee.PK.ForeignKeyOf<PRPayment>.By<employeeID> { }
			public class PayrollBatch : PRBatch.PK.ForeignKeyOf<PRPayment>.By<payBatchNbr> { }
			//public class GLBatch : GL.Batch.PK.ForeignKeyOf<PRPayment>.By<BatchModule.modulePR, batchNbr> { }
			public class CurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<PRPayment>.By<curyInfoID> { }
			public class CashAccountTransaction : CATran.PK.ForeignKeyOf<PRPayment>.By<cashAccountID, caTranID> { }
			public class Country : CS.Country.PK.ForeignKeyOf<PRPayment>.By<countryID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(PayrollType.Regular)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PayrollType.List]
		[PXFieldDescription]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// [key] Reference number of the document.
		/// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(SelectFrom<PRPayment>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>
			.Where<PRPayment.docType.IsEqual<PRPayment.docType.FromCurrent>>
			.SearchFor<PRPayment.refNbr>),
			typeof(docType), typeof(refNbr), typeof(employeeID), typeof(EPEmployee.acctName), typeof(status), typeof(extRefNbr),
			typeof(payGroupID), typeof(payPeriodID), typeof(startDate), typeof(endDate), typeof(transactionDate))]
		[PayrollType.Numbering]
		[PXFieldDescription]
		public String RefNbr { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PaymentStatus.NeedCalculation)]
		[PaymentStatus.List]
		[SetStatus]
		[PXDependsOnFields(typeof(PRPayment.closed), typeof(PRPayment.released), typeof(PRPayment.hold),
			typeof(PRPayment.voided), typeof(PRPayment.hasUpdatedGL), typeof(PRPayment.calculated),
			typeof(PRPayment.paid), typeof(PRPayment.liabilityPartiallyPaid), typeof(PRPayment.netAmount))]
		public virtual string Status { get; set; }
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		/// <summary>
		/// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<PRSetup.holdEntry>))]
		public virtual Boolean? Hold { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Visible = false)]
		public bool? Released { get; set; }
		#endregion
		#region ReleasedToVerify
		public abstract class releasedToVerify : PX.Data.BQL.BqlBool.Field<releasedToVerify> { }
		[PXDBRestrictionBool(typeof(released))]
		public bool? ReleasedToVerify { get; set; }
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Voided", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public Boolean? Voided { get; set; }
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Closed")]
		[PXDefault(false)]
		public virtual Boolean? Closed { get; set; }
		#endregion
		#region LiabilityPartiallyPaid
		public abstract class liabilityPartiallyPaid : PX.Data.BQL.BqlBool.Field<liabilityPartiallyPaid> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Liability Partially Paid")]
		[PXDefault(false)]
		public virtual Boolean? LiabilityPartiallyPaid { get; set; }
		#endregion
		#region Paid
		public abstract class paid : PX.Data.BQL.BqlBool.Field<paid> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Paid")]
		[PXDefault(false)]
		public virtual Boolean? Paid { get; set; }
		#endregion
		#region Calculated
		public abstract class calculated : PX.Data.BQL.BqlBool.Field<calculated> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Calculated")]
		[PXDefault(false)]
		public virtual Boolean? Calculated { get; set; }
		#endregion
		#region HasUpdatedGL
		public abstract class hasUpdatedGL : PX.Data.BQL.BqlBool.Field<hasUpdatedGL> { }
		[PXDBBool]
		[PXUIField(Visible = false)]
		[PXDefault(typeof(PRSetup.updateGL))]
		public virtual bool? HasUpdatedGL { get; set; }
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		//TODO: Numbering [PaymentRef(typeof(APPayment.cashAccountID), typeof(APPayment.paymentMethodID), typeof(APPayment.stubCntr), typeof(APPayment.updateNextNumber))]
		public virtual string ExtRefNbr { get; set; }
		#endregion
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Group", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<payBatchNbr.IsNull>))]
		[PXDefault]
		[PXSelector(typeof(SearchFor<PRPayGroup.payGroupID>.Where<MatchWithPayGroup<PRPayGroup.payGroupID>>), DescriptionField = typeof(PRPayGroup.description))]
		[PXForeignReference(typeof(FK.PayGroup))]
		public string PayGroupID { get; set; }
		#endregion
		#region IsWeekOrBiWeekPeriod
		public abstract class isWeeklyOrBiWeeklyPeriod : PX.Data.BQL.BqlBool.Field<isWeeklyOrBiWeeklyPeriod> { }
		[PXBool]
		[PXFormula(typeof(Default<payGroupID>))]
		[PXUnboundDefault(typeof(SearchFor<PRPayGroupYearSetup.isWeeklyOrBiWeeklyPeriod>.Where<PRPayGroupYearSetup.payGroupID.IsEqual<payGroupID.FromCurrent>>))]
		[PXUIField(DisplayName = "Is Weekly or Biweekly Period", Visible = false)]
		public bool? IsWeeklyOrBiWeeklyPeriod { get; set; }
		#endregion
		#region PayPeriodID
		public abstract class payPeriodID : PX.Data.BQL.BqlString.Field<payPeriodID> { }
		[PXUIField(DisplayName = "Pay Period", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<payBatchNbr.IsNull>))]
		[PRPayGroupPeriodID(typeof(payGroupID), typeof(startDate), null, typeof(endDate), typeof(transactionDate), false,
			typeof(Where<payBatchNbr.IsNull.And<docType.IsEqual<PayrollType.special>.Or<docType.IsEqual<PayrollType.final>>>>), true)]
		public string PayPeriodID { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period Start", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period End", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? EndDate { get; set; }
		#endregion
		#region TransactionDate
		public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? TransactionDate { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXFormula(typeof(Default<employeeID>))]
		[Branch(typeof(SelectFrom<BranchWithAddress>
				.InnerJoin<EPEmployee>.On<Branch.bAccountID.IsEqual<EPEmployee.parentBAccountID>>
				.Where<EPEmployee.bAccountID.IsEqual<employeeID.FromCurrent>>
				.SearchFor<BranchWithAddress.branchID>),
			typeof(SearchFor<BranchWithAddress.branchID>),
			IsDetail = false,
			Visibility = PXUIVisibility.SelectorVisible)]
		[PXRestrictor(typeof(Where<countryID.FromCurrent.IsNull.Or<BranchWithAddress.addressCountryID.IsEqual<countryID.FromCurrent>>>),
			Messages.PaymentBranchCountryNotMatching, new[] { typeof(countryID), typeof(BranchWithAddress.addressCountryID) })]
		public int? BranchID { get; set; }
		#endregion
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		[PXInt]
		[PXFormula(typeof(Default<branchID>))]
		[PXUnboundDefault(typeof(SearchFor<Branch.organizationID>.Where<Branch.branchID.IsEqual<branchID.FromCurrent>>))]
		public virtual int? OrganizationID { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXUIField(DisplayName = "Posting Period")]
		[PROpenPeriod(typeof(PRPayment.transactionDate), typeof(PRPayment.organizationID))]
		public string FinPeriodID { get; set; }
		#endregion FinPeriodID
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(SearchFor<PaymentMethod.paymentMethodID>.Where<PRxPaymentMethod.useForPR.IsEqual<True>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXFormula(typeof(Default<PRPayment.employeeID>))]
		[PXDefault(typeof(SearchFor<PREmployee.paymentMethodID>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>))]
		public virtual string PaymentMethodID { get; set; }
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector(typeof(Search2<CashAccount.cashAccountID,
			InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
			Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PRPayment.paymentMethodID>>, And<PRxPaymentMethodAccount.useForPR, Equal<True>>>>),
			SubstituteKey = typeof(CashAccount.cashAccountCD),
			DescriptionField = typeof(CashAccount.descr))]
		[PXFormula(typeof(Default<PRPayment.paymentMethodID>))]
		[PXDefault(typeof(SearchFor<PREmployee.cashAccountID>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>))]
		public virtual int? CashAccountID { get; set; }
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(128, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string DocDesc { get; set; }
		#endregion
		#region ChkVoidType
		public abstract class chkVoidType : PX.Data.BQL.BqlString.Field<chkVoidType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Void Reason", Enabled = false)]
		[CheckVoidType.List]
		public virtual string ChkVoidType { get; set; }
		#endregion
		#region ChkCreateNew
		public abstract class chkCreateNew : PX.Data.BQL.BqlBool.Field<chkCreateNew> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create New Payment from Voided Payment")]
		public virtual bool? ChkCreateNew { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[EmployeeActiveInPayGroup]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>.And<payBatchNbr.IsNull>>))]
		[PXForeignReference(typeof(Field<PRPayment.employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		public virtual int? EmployeeID { get; set; }
		#endregion
		#region EmpType
		public abstract class empType : PX.Data.BQL.BqlString.Field<empType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Employee Type")]
		[PXFormula(typeof(Default<employeeID>))]
		[PXDefault]
		[EmployeeType.List]
		public virtual string EmpType { get; set; }
		#endregion
		#region RegularAmount
		public abstract class regularAmount : PX.Data.BQL.BqlDecimal.Field<regularAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Regular Amount to Be Paid")]
		public virtual Decimal? RegularAmount { get; set; }
		#endregion
		#region ManualRegularAmount
		public abstract class manualRegularAmount : PX.Data.BQL.BqlBool.Field<manualRegularAmount> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Manual Amount")]
		[PXDefault(false)]
		public virtual bool? ManualRegularAmount { get; set; }
		#endregion
		#region PayBatchNbr
		public abstract class payBatchNbr : PX.Data.BQL.BqlString.Field<payBatchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Batch Nbr.")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<payBatchNbr>>>>))]
		public virtual string PayBatchNbr { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.modulePR>>>))]
		public virtual string BatchNbr { get; set; }
		#endregion
		#region TotalEarnings
		public abstract class totalEarnings : PX.Data.BQL.BqlDecimal.Field<totalEarnings> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(Visible = false)]
		public virtual decimal? TotalEarnings { get; set; }
		#endregion
		#region GrossAmount
		public abstract class grossAmount : PX.Data.BQL.BqlDecimal.Field<grossAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Gross Pay", Enabled = false)]
		[PXFormula(typeof(totalEarnings.Add<payableBenefitAmount>))]
		public virtual decimal? GrossAmount { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Deductions", Enabled = false)]
		public virtual decimal? DedAmount { get; set; }
		#endregion
		#region TaxAmount
		public abstract class taxAmount : PX.Data.BQL.BqlDecimal.Field<taxAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Taxes", Enabled = false)]
		public virtual decimal? TaxAmount { get; set; }
		#endregion
		#region NetAmount
		public abstract class netAmount : PX.Data.BQL.BqlDecimal.Field<netAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Net Pay", Enabled = false)]
		[PXFormula(typeof(Sub<Sub<PRPayment.grossAmount, PRPayment.dedAmount>, PRPayment.taxAmount>))]
		public virtual decimal? NetAmount { get; set; }
		#endregion
		//TODO: Review multi-currency support in payroll
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(ModuleCode = BatchModule.PR)]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion
		#region PayableBenefitAmount 
		public abstract class payableBenefitAmount : PX.Data.BQL.BqlDecimal.Field<payableBenefitAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(Visible = false)]
		public virtual decimal? PayableBenefitAmount { get; set; }
		#endregion
		#region BenefitAmount 
		public abstract class benefitAmount : PX.Data.BQL.BqlDecimal.Field<benefitAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Benefits", Enabled = false)]
		public virtual decimal? BenefitAmount { get; set; }
		#endregion
		#region EmployerTaxAmount 
		public abstract class employerTaxAmount : PX.Data.BQL.BqlDecimal.Field<employerTaxAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Employer Tax", Enabled = false)]
		public virtual decimal? EmployerTaxAmount { get; set; }
		#endregion
		#region CATranID
		public abstract class caTranID : PX.Data.BQL.BqlLong.Field<caTranID> { }
		[PXDBLong]
		[PRPaymentCashTranID]
		public virtual Int64? CATranID { get; set; }
		#endregion
		#region DetailLinesCount
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Nbr. of Detail Lines")]
		public virtual int? DetailLinesCount { get; set; }
		public abstract class detailLinesCount : PX.Data.BQL.BqlInt.Field<detailLinesCount> { }
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		/// <summary>
		/// Type of the original (source) document.
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PayrollType.List]
		[PXUIField(DisplayName = "Original Doc. Type")]
		public virtual String OrigDocType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		/// <summary>
		/// Reference number of the original (source) document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Original Document")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		/// <summary>
		/// Read-only field indicating whether the document is of debit or credit type.
		/// The value of this field is based solely on the <see cref="DocType"/> field.
		/// </summary>
		[PXString(1, IsFixed = true)]
		public string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return PayrollType.DrCr(DocType);
			}
			set
			{
			}
		}
		#endregion
		#region AverageRate
		public abstract class averageRate : PX.Data.BQL.BqlDecimal.Field<averageRate> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Average Rate", Enabled = false)]
		[PXUIVisible(typeof(Where<empType.IsNotEqual<EmployeeType.salariedExempt>.And<empType.IsNotEqual<EmployeeType.salariedNonExempt>>>))]
		[PXFormula(typeof(Switch<Case<Where<PRPayment.totalHours.IsGreater<decimal0>>,
			PRPayment.grossAmount.Divide<PRPayment.totalHours>>,
			decimal0>))]
		public virtual decimal? AverageRate { get; set; }
		#endregion
		#region TotalHours
		public abstract class totalHours : PX.Data.BQL.BqlDecimal.Field<totalHours> { }
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Hours", Enabled = false)]
		public virtual decimal? TotalHours { get; set; }
		#endregion
		#region ExemptFromOvertimeRules
		public abstract class exemptFromOvertimeRules : PX.Data.BQL.BqlBool.Field<exemptFromOvertimeRules> { }
		[PXBool]
		[PXUIField(DisplayName = "Exempt from Overtime Rules", Visible = false)]
		[PXFormula(typeof(Selector<employeeID, PREmployee.exemptFromOvertimeRules>))]
		public bool? ExemptFromOvertimeRules { get; set; }
		#endregion
		#region ApplyOvertimeRules
		public abstract class applyOvertimeRules : PX.Data.BQL.BqlBool.Field<applyOvertimeRules> { }
		[PXDBBool]
		[PXDefault(typeof(Switch<Case<Where<exemptFromOvertimeRules, Equal<True>, Or<Parent<PRBatch.applyOvertimeRules>, Equal<False>>>, False>, True>))]
		[PXUIField(DisplayName = "Apply Overtime Rules for the Document")]
		[PXUIEnabled(typeof(Where<exemptFromOvertimeRules.IsEqual<False>.And<released.IsNotEqual<True>.And<paid.IsNotEqual<True>>>>))]
		[PXFormula(typeof(Default<exemptFromOvertimeRules, payBatchNbr>))]
		public virtual bool? ApplyOvertimeRules { get; set; }
		#endregion
		#region PaymentDocAndRef
		public abstract class paymentDocAndRef : Data.BQL.BqlString.Field<paymentDocAndRef> { }
		[PXUnboundDefault]
		[DocTypeAndRefNbrDisplayName(typeof(docType), typeof(refNbr))]
		[PXUIField(DisplayName = "Paycheck Ref")]
		public string PaymentDocAndRef { get; set; }
		#endregion
		#region IsPrintChecksPaymentMethod
		public abstract class isPrintChecksPaymentMethod : PX.Data.BQL.BqlBool.Field<isPrintChecksPaymentMethod> { }
		[PXBool]
		[PXUIField(DisplayName = "Print Checks Payment Method", Visible = false)]
		[PXFormula(typeof(Selector<PRPayment.paymentMethodID, PRxPaymentMethod.prPrintChecks>))]
		public virtual Boolean? IsPrintChecksPaymentMethod { get; set; }
		#endregion
		#region NetAmountToWords
		public abstract class netAmountToWords : PX.Data.BQL.BqlString.Field<netAmountToWords> { }
		[ToWords(typeof(PRPayment.netAmount))]
		public virtual string NetAmountToWords { get; set; }
		#endregion
		#region LaborCostSplitType
		public abstract class laborCostSplitType : PX.Data.BQL.BqlString.Field<laborCostSplitType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[CostAssignmentType.List]
		public virtual string LaborCostSplitType { get; set; }
		#endregion
		#region PTOCostSplitType
		public abstract class ptoCostSplitType : PX.Data.BQL.BqlString.Field<ptoCostSplitType> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(Visible = false)]
		public virtual string PTOCostSplitType { get; set; }
		#endregion
		#region AutoPayCarryover
		public abstract class autoPayCarryover : PX.Data.BQL.BqlBool.Field<autoPayCarryover> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Pay Carryover if Applicable")]
		[PXDefault(true)]
		public virtual Boolean? AutoPayCarryover { get; set; }
		#endregion
		#region PaymentBatchNbr
		public abstract class paymentBatchNbr : PX.Data.BQL.BqlString.Field<paymentBatchNbr> { }
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Batch Nbr.", Enabled = true)]
		[PXDBScalar(typeof(SelectFrom<CABatch>
			.InnerJoin<CABatchDetail>
				.On<CABatch.batchNbr.IsEqual<CABatchDetail.batchNbr>>
			.Where<CABatchDetail.origDocType.IsEqual<PRPayment.docType>
				.And<CABatchDetail.origRefNbr.IsEqual<PRPayment.refNbr>>
				.And<CABatchDetail.origModule.IsEqual<BatchModule.modulePR>>>.SearchFor<CABatch.batchNbr>))]
		public virtual string PaymentBatchNbr { get; set; }
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXFormula(typeof(Default<branchID>))]
		[PXDBString(2, IsFixed = true)]
		[PRCountry]
		[PXUIField(DisplayName = "Payment country", Enabled = false)]
		[PXDefault(typeof(SelectFrom<BranchWithAddress>
			.Where<BranchWithAddress.branchID.IsEqual<PRPayment.branchID.FromCurrent>>
			.SearchFor<BranchWithAddress.addressCountryID>))]
		public virtual string CountryID { get; set; }
		#endregion
		#region TerminationReason
		public abstract class terminationReason : PX.Data.BQL.BqlString.Field<terminationReason> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Termination Reason", Required = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVisible(typeof(PRPayment.docType.FromCurrent.IsEqual<PayrollType.final>))]
		[PXUIRequiredIfVisible]
		[EPTermReason.List]
		public virtual string TerminationReason { get; set; }
		#endregion
		#region ShowROETab
		public abstract class showROETab : PX.Data.BQL.BqlBool.Field<showROETab> { }
		[PXBool]
		[PXUIField(Visible = false)]
		public virtual bool? ShowROETab
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return DocType == PayrollType.Final && PXAccess.FeatureInstalled<FeaturesSet.payrollCAN>();
			}
		}
		#endregion

		#region IsRehirable
		[PXDBBool]
		[PXUIField(DisplayName = "Eligible for Rehire")]
		[PXUIVisible(typeof(PRPayment.docType.FromCurrent.IsEqual<PayrollType.final>))]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsRehirable { get; set; }
		public abstract class isRehirable : PX.Data.BQL.BqlBool.Field<isRehirable> { }
		#endregion

		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Termination Date")]
		[PXDefault(typeof(PRPayment.endDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIVisible(typeof(PRPayment.docType.FromCurrent.IsEqual<PayrollType.final>))]
		[PXUIRequiredIfVisible]
		public virtual DateTime? TerminationDate { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.PR, Messages.SearchableTitlePRPayment, new Type[] { typeof(docType), typeof(refNbr) },
			new Type[] { typeof(refNbr), typeof(payGroupID), typeof(payPeriodID), typeof(PREmployee.acctName) },
			NumberFields = new Type[] { typeof(refNbr) },
			Line1Format = "{1}{2}{3}{4}{5:d}", Line1Fields = new Type[] { typeof(employeeID), typeof(refNbr), typeof(payGroupID), typeof(payPeriodID), typeof(PREmployee.acctName), typeof(transactionDate) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(docDesc) },
			SelectForFastIndexing = typeof(Select2<PRPayment, InnerJoin<PREmployee, On<PREmployee.bAccountID, Equal<PRPayment.employeeID>>>>)
		)]
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}

	[Obsolete("This class is not supported since version 2022 R1.")]
	public class PaymentEmployeeInCountryAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PREmployee employee = PXSelectorAttribute.Select(sender, e.Row, _FieldName) as PREmployee;
			if (employee != null)
			{
				sender.SetValue<PRPayment.countryID>(e.Row, employee.CountryID);
			}
		}
	}
}
