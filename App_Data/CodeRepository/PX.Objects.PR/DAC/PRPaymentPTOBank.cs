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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using System;
using System.Diagnostics;

namespace PX.Objects.PR
{
	/// <summary>
	/// Includes detailed information about the paid time off (PTO) usage and balance data
	/// associated to the current pay check.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.PRPaymentPTOBank)]
	[DebuggerDisplay("{GetType().Name,nq}: DocType = {DocType,nq}, RefNbr = {RefNbr,nq}, BankID = {BankID,nq}, EffectiveStartDate = {EffectiveStartDate,nq}")]
	public class PRPaymentPTOBank : IBqlTable, PTOHelper.IPTOHistory
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PRPaymentPTOBank>.By<docType, refNbr, bankID, effectiveStartDate>
		{
			public static PRPaymentPTOBank Find(PXGraph graph, string docType, string refNbr, string bankID, DateTime? effectiveStartDate, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, bankID, effectiveStartDate, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Payment
			/// </summary>
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentPTOBank>.By<docType, refNbr> { }
			/// <summary>
			/// PTO Bank
			/// </summary>
			public class PTOBank : PRPTOBank.PK.ForeignKeyOf<PRPaymentPTOBank>.By<bankID> { }
			/// <summary>
			/// Disbursing Earning Type
			/// </summary>
			public class DisbursingEarningType : EPEarningType.PK.ForeignKeyOf<PRPaymentPTOBank>.By<earningTypeCD> { }
		}
		#endregion

		#region DocType
		/// <summary>
		/// The document type of the current paycheck.
		/// The field is included in <see cref="FK.Payment"/>
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRPayment.docType" /> field.
		/// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		[PayrollType.List]
		public string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		/// <summary>
		/// The reference number of the current paycheck.
		/// The field is included in <see cref="FK.Payment"/>
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRPayment.refNbr" /> field.
		/// </value>
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, 
			Where<PRPayment.docType, Equal<Current<PRPaymentPTOBank.docType>>,
			And<PRPayment.refNbr, Equal<Current<PRPaymentPTOBank.refNbr>>>>>))]
		public String RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region BankID
		/// <summary>
		/// The unique identifier of the bank.
		/// The field is included in <see cref="FK.PTOBank"/>
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRPTOBank.bankID" /> field.
		/// </value>
		[PXDBString(3, IsKey = true, IsUnicode = true, InputMask = ">CCC")]
		[PXUIField(DisplayName = "PTO Bank", Enabled = false)]
		[PXSelector(typeof(SearchFor<PRPTOBank.bankID>), DescriptionField = typeof(PRPTOBank.description))]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region EffectiveStartDate
		/// <summary>
		/// The start date when the settings of the PTO bank are applied to the employee.
		/// </summary>
		[PXDBDate(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? EffectiveStartDate { get; set; }
		public abstract class effectiveStartDate : PX.Data.BQL.BqlDateTime.Field<effectiveStartDate> { }
		#endregion

		#region EffectiveEndDate
		/// <summary>
		/// The end date when the settings of the PTO bank are applied to the employee.
		/// </summary>
		[PXDBDate(PreserveTime = true, UseTimeZone = false)]
		[PXUIField(DisplayName = "Effective End Date")]
		public virtual DateTime? EffectiveEndDate { get; set; }
		public abstract class effectiveEndDate : PX.Data.BQL.BqlDateTime.Field<effectiveEndDate> { }
		#endregion

		#region EarningTypeCD
		/// <summary>
		/// An earning type code to be used to disburse it on a paycheck.
		/// The field is included in <see cref="FK.DisbursingEarningType"/>.
		/// </summary>
		[PXString(EPEarningType.typeCD.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Disbursing Earning Type", Visible = false)]
		[PXFormula(typeof(Selector<PRPaymentPTOBank.bankID, PRPTOBank.earningTypeCD>))]
		public virtual string EarningTypeCD { get; set; }
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		#endregion

		#region IsActive
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the bank accrues hours.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		#endregion

		#region IsCertifiedJob
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that only hours worked on certified project should be included in the paid time off calculation.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Applies to Certified Job Only")]
		[PXDefault(false)]
		[PXUIEnabled(typeof(PRPaymentPTOBank.isActive))]
		public virtual bool? IsCertifiedJob { get; set; }
		public abstract class isCertifiedJob : PX.Data.BQL.BqlBool.Field<isCertifiedJob> { }
		#endregion

		#region AccrualMethod
		/// <summary>
		/// The method of PTO hours accrual, which can be one of the following: Percentage or Total Hours per Year.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PTOAccrualMethod.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Accrual Method", Enabled = false)]
		[PTOAccrualMethod.List]
		public virtual string AccrualMethod { get; set; }
		public abstract class accrualMethod : PX.Data.BQL.BqlString.Field<accrualMethod> { }
		#endregion

		#region AccrualRate
		/// <summary>
		/// An accrual rate to be used to accumulate hours.
		/// </summary>
		[PXDBDecimal(6, MinValue = 0)]
		[PXUIField(DisplayName = "Accrual %")]
		[PXUIEnabled(typeof(Where<PRPaymentPTOBank.isActive.IsEqual<True>.And<accrualMethod.IsEqual<PTOAccrualMethod.percentage>>>))]
		[ShowValueWhen(typeof(Where<accrualMethod.IsEqual<PTOAccrualMethod.percentage>>))]
		[PXDefault(TypeCode.Decimal, "0")]
		[PXDependsOnFields(typeof(accrualMethod))]
		public virtual Decimal? AccrualRate { get; set; }
		public abstract class accrualRate : PX.Data.BQL.BqlDecimal.Field<accrualRate> { }
		#endregion

		#region HoursPerYear
		/// <summary>
		/// The number of hours that the employee may accrue throughout the year.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Hours per Year")]
		[PXUIEnabled(typeof(Where<PRPaymentPTOBank.isActive.IsEqual<True>.And<accrualMethod.IsEqual<PTOAccrualMethod.totalHoursPerYear>>>))]
		[ShowValueWhen(typeof(Where<accrualMethod.IsEqual<PTOAccrualMethod.totalHoursPerYear>>))]
		[PXDefault(TypeCode.Decimal, "0")]
		[PXDependsOnFields(typeof(accrualMethod))]
		public virtual Decimal? HoursPerYear { get; set; }
		public abstract class hoursPerYear : PX.Data.BQL.BqlDecimal.Field<hoursPerYear> { }
		#endregion

		#region AccrualLimit
		/// <summary>
		/// The upper limit for the bank. Once the hours accumulated in the bank reach the limit,
		/// the system stops accruing the hours.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Balance Limit", Enabled = false)]
		public virtual Decimal? AccrualLimit
		{
			get => _AccrualLimit != 0 ? _AccrualLimit : null;
			set => _AccrualLimit = value;
		}
		private decimal? _AccrualLimit;
		public abstract class accrualLimit : PX.Data.BQL.BqlDecimal.Field<accrualLimit> { }
		#endregion

		#region AccumulatedAmount
		/// <summary>
		/// The number of hours accrued for the bank.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Accrued Hours", Enabled = false)]
		public virtual Decimal? AccumulatedAmount { get; set; }
		public abstract class accumulatedAmount : PX.Data.BQL.BqlDecimal.Field<accumulatedAmount> { }
		#endregion

		#region AccumulatedMoney
		/// <summary>
		/// The amount of money accrued for the bank.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Accrued Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction, Equal<True>>))]
		public virtual Decimal? AccumulatedMoney { get; set; }
		public abstract class accumulatedMoney : PX.Data.BQL.BqlDecimal.Field<accumulatedMoney> { }
		#endregion

		#region UsedAmount
		/// <summary>
		/// The number of hours currently used by the employee.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Used Hours", Enabled = false)]
		public virtual Decimal? UsedAmount { get; set; }
		public abstract class usedAmount : PX.Data.BQL.BqlDecimal.Field<usedAmount> { }
		#endregion

		#region UsedMoney
		/// <summary>
		/// The amount of money currently used by the employee.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Used Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction, Equal<True>>))]
		public virtual Decimal? UsedMoney { get; set; }
		public abstract class usedMoney : PX.Data.BQL.BqlDecimal.Field<usedMoney> { }
		#endregion

		#region AvailableAmount
		/// <summary>
		/// The number of hours accrued for the employee through all released paychecks.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Available Hours", Enabled = false)]
		public virtual Decimal? AvailableAmount { get; set; }
		public abstract class availableAmount : PX.Data.BQL.BqlDecimal.Field<availableAmount> { }
		#endregion

		#region AvailableMoney
		/// <summary>
		/// The amount of money accrued for the employee through all released paychecks.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Available Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction, Equal<True>>))]
		public virtual Decimal? AvailableMoney { get; set; }
		public abstract class availableMoney : PX.Data.BQL.BqlDecimal.Field<availableMoney> { }
		#endregion

		#region AccruingHours
		/// <summary>
		/// The number of hours accrued during the current period.
		/// </summary>
		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? AccruingHours { get; set; }
		public abstract class accruingHours : PX.Data.BQL.BqlDecimal.Field<accruingHours> { }
		#endregion

		#region AccruingDays
		/// <summary>
		/// The number of days accrued during the current period.
		/// </summary>
		[PXDBInt]
		public virtual int? AccruingDays { get; set; }
		public abstract class accruingDays : PX.Data.BQL.BqlInt.Field<accruingDays> { }
		#endregion

		#region DaysInPeriod
		/// <summary>
		/// The amount of days during the current period.
		/// </summary>
		[PXDBInt]
		public virtual int? DaysInPeriod { get; set; }
		public abstract class daysInPeriod : PX.Data.BQL.BqlInt.Field<daysInPeriod> { }
		#endregion

		#region EffectiveCoefficient
		/// <summary>
		/// The effective coefficient which is calculated by dividing the number of days accrued by
		/// the amount of days during the current period.
		/// </summary>
		[PXDecimal]
		[PXDependsOnFields(typeof(daysInPeriod), typeof(accruingDays))]
		public virtual decimal? EffectiveCoefficient => DaysInPeriod.HasValue ? AccruingDays / (decimal)DaysInPeriod.Value : null;
		public abstract class effectiveCoefficient : PX.Data.BQL.BqlDecimal.Field<effectiveCoefficient> { }
		#endregion

		#region AccrualAmount
		/// <summary>
		/// The number of hours accrued for the bank during the current period.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Paycheck Accrual Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? AccrualAmount { get; set; }
		public abstract class accrualAmount : PX.Data.BQL.BqlDecimal.Field<accrualAmount> { }
		#endregion

		#region AccrualMoney
		/// <summary>
		/// The amount of money accrued for the bank during the current period.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Paycheck Accrual Amount", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? AccrualMoney { get; set; }
		public abstract class accrualMoney : PX.Data.BQL.BqlDecimal.Field<accrualMoney> { }
		#endregion

		#region DisbursementAmount
		/// <summary>
		/// The total of the disbursement hours and paid carryover hours.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Disbursement Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? DisbursementAmount { get; set; }
		public abstract class disbursementAmount : PX.Data.BQL.BqlDecimal.Field<disbursementAmount> { }
		#endregion

		#region DisbursementMoney
		/// <summary>
		/// The amount of disbursement money and paid carryover money.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Disbursement Amount", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? DisbursementMoney { get; set; }
		public abstract class disbursementMoney : PX.Data.BQL.BqlDecimal.Field<disbursementMoney> { }
		#endregion

		#region ProcessedFrontLoading
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the PTO has already processed front loading.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Front Loading", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedFrontLoading { get; set; }
		public abstract class processedFrontLoading : PX.Data.BQL.BqlBool.Field<processedFrontLoading> { }
		#endregion

		#region FrontLoadingAmount
		/// <summary>
		/// The number of hours the system adds to the bank each year on a date specified in the Start Date box.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Front Loading Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PTOProcessedFlag(typeof(processedFrontLoading))]
		public virtual Decimal? FrontLoadingAmount { get; set; }
		public abstract class frontLoadingAmount : PX.Data.BQL.BqlDecimal.Field<frontLoadingAmount> { }
		#endregion

		#region ProcessedCarryover
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the PTO has already processed carryover.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Carryover", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedCarryover { get; set; }
		public abstract class processedCarryover : PX.Data.BQL.BqlBool.Field<processedCarryover> { }
		#endregion

		#region CarryoverAmount
		/// <summary>
		/// The number of hours the system carries over to the following year.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Carryover Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PTOProcessedFlag(typeof(processedCarryover))]
		public virtual Decimal? CarryoverAmount { get; set; }
		public abstract class carryoverAmount : PX.Data.BQL.BqlDecimal.Field<carryoverAmount> { }
		#endregion

		#region CarryoverMoney
		/// <summary>
		/// The amount of money the system carries over to the following year.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Carryover Amount", FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		public virtual Decimal? CarryoverMoney { get; set; }
		public abstract class carryoverMoney : PX.Data.BQL.BqlDecimal.Field<carryoverMoney> { }
		#endregion

		#region ProcessedPaidCarryover
		/// <summary>
		/// Indicates (if set to <see langword="true" />) that the PTO has already processed paid carryover.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Processed Paid Carryover", Visible = false)]
		[PXDefault(false)]
		public virtual bool? ProcessedPaidCarryover { get; set; }
		public abstract class processedPaidCarryover : PX.Data.BQL.BqlBool.Field<processedPaidCarryover> { }
		#endregion

		#region PaidCarryoverAmount
		/// <summary>
		/// Hours automatically disbursed by the system according to the Paid after a period of time setting in the PTO Banks screen.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Paid Carryover Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIVisible(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PXUIEnabled(typeof(Where<Parent<PRPayment.docType>, Equal<PayrollType.adjustment>>))]
		[PTOProcessedFlag(typeof(processedPaidCarryover))]
		public virtual Decimal? PaidCarryoverAmount { get; set; }
		public abstract class paidCarryoverAmount : PX.Data.BQL.BqlDecimal.Field<paidCarryoverAmount> { }
		#endregion

		#region TotalAccrual
		/// <summary>
		/// The number of hours accrued for the bank.
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "Paycheck Total Accrual Hours", Enabled = false)]
		[PXFormula(typeof(Add<Add<PRPaymentPTOBank.accrualAmount, PRPaymentPTOBank.frontLoadingAmount>, PRPaymentPTOBank.carryoverAmount>))]
		public virtual Decimal? TotalAccrual { get; set; }
		public abstract class totalAccrual : PX.Data.BQL.BqlDecimal.Field<totalAccrual> { }
		#endregion

		#region TotalAccrualMoney
		/// <summary>
		/// The amount of money accrued for the bank during the current year.
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "Total Accrual Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXFormula(typeof(Add<accrualMoney, carryoverMoney>))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction, Equal<True>>))]
		public virtual Decimal? TotalAccrualMoney { get; set; }
		public abstract class totalAccrualMoney : PX.Data.BQL.BqlDecimal.Field<totalAccrualMoney> { }
		#endregion

		#region TotalDisbursement
		/// <summary>
		/// The total of the disbursement hours, paid carryover hours and settlement discard amount.
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "Paycheck Disbursed Hours", Enabled = false)]
		[PXFormula(typeof(Add<Add<PRPaymentPTOBank.disbursementAmount, PRPaymentPTOBank.paidCarryoverAmount>, PRPaymentPTOBank.settlementDiscardAmount>))]
		public virtual Decimal? TotalDisbursement { get; set; }
		public abstract class totalDisbursement : PX.Data.BQL.BqlDecimal.Field<totalDisbursement> { }
		#endregion

		#region TotalDisbursementMoney
		/// <summary>
		/// The total amount of money of the disbursement.
		/// </summary>
		[PXDecimal]
		[PXUIField(DisplayName = "Total Disbursement Amount", Enabled = false, FieldClass = nameof(FeaturesSet.PayrollCAN))]
		[PXUnboundDefault(typeof(disbursementMoney.FromCurrent))]
		[HideValueIfDisabled(false, typeof(Where<createFinancialTransaction, Equal<True>>))]
		public virtual Decimal? TotalDisbursementMoney { get; set; }
		public abstract class totalDisbursementMoney : PX.Data.BQL.BqlDecimal.Field<totalDisbursementMoney> { }
		#endregion

		#region CreateFinancialTransaction
		/// <summary>
		/// Enable the money calculation and the creation of general ledger transaction for paid time off on Paychecks and Adjustments.
		/// </summary>
		[PXBool]
		[PXUnboundDefault(typeof(Selector<bankID, PRPTOBank.createFinancialTransaction>))]
		public virtual bool? CreateFinancialTransaction { get; set; }
		public abstract class createFinancialTransaction : PX.Data.BQL.BqlBool.Field<createFinancialTransaction> { }
		#endregion

		#region NbrOfPayPeriods
		/// <summary>
		/// Number of pay periods.
		/// </summary>
		[PXShort]
		[PXDBScalar(typeof(Search2<PRPayGroupYear.finPeriods,
			InnerJoin<PRPayment, On<PRPayment.payGroupID, Equal<PRPayGroupYear.payGroupID>,
				And<PRPayGroupYear.year, Equal<DatePart<DatePart.year, PRPayment.transactionDate>>>>>,
			Where<PRPayment.docType, Equal<docType>, And<PRPayment.refNbr, Equal<refNbr>>>>))]
		public virtual short? NbrOfPayPeriods { get; set; }
		public abstract class nbrOfPayPeriods : PX.Data.BQL.BqlShort.Field<nbrOfPayPeriods> { }
		#endregion NbrOfPayPeriods

		#region CalculationFormula
		/// <summary>
		/// The calculation formula for the accrual of PTO hours, which depends on the specified accrual method and the resulting amount.
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Paycheck Accrual Calculation", Enabled = false)]
		[PXDependsOnFields(typeof(accrualMethod), typeof(accrualRate), typeof(accruingHours), typeof(frontLoadingAmount), typeof(carryoverAmount), typeof(totalAccrual), typeof(hoursPerYear),
			typeof(nbrOfPayPeriods), typeof(effectiveCoefficient), typeof(accruingDays), typeof(daysInPeriod))]
		public virtual string CalculationFormula
		{
			get
			{
				if (AccrualMethod == PTOAccrualMethod.Percentage)
				{
					decimal rate = AccrualRate / 100 ?? 0;
					decimal calculation = (rate * AccruingHours) + FrontLoadingAmount + CarryoverAmount ?? 0;
					if (Math.Round(calculation, 2) == TotalAccrual)
					{
						return $"({rate:0.00} * {AccruingHours:0.00}) + {FrontLoadingAmount:0.00} + {CarryoverAmount:0.00} = {TotalAccrual:0.00}";
					}
				}
				else if (AccrualMethod == PTOAccrualMethod.TotalHoursPerYear && NbrOfPayPeriods > 0)
				{
					decimal calculation = (HoursPerYear / NbrOfPayPeriods * EffectiveCoefficient) + FrontLoadingAmount + CarryoverAmount ?? 0;
					if (Math.Round(calculation, 2) == TotalAccrual)
					{
						return $"{HoursPerYear:0.00} / {NbrOfPayPeriods} * {AccruingDays} / {DaysInPeriod} + {FrontLoadingAmount:0.00} + {CarryoverAmount:0.00} = {TotalAccrual:0.00}";
					}
				}

				return Messages.Unknown;
			}

			set { }
		}
		public abstract class calculationFormula : PX.Data.BQL.BqlString.Field<calculationFormula> { }
		#endregion

		#region SettlementDiscardAmount
		/// <summary>
		/// Paid time off hours that will not be paid as part of an employee settlement.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Settlement Discard Amount", Enabled = false, Visible = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? SettlementDiscardAmount { get; set; }
		public abstract class settlementDiscardAmount : PX.Data.BQL.BqlDecimal.Field<settlementDiscardAmount> { }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}

	public class PTOProcessedFlagAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _ProcessedFlagField;

		public PTOProcessedFlagAttribute(Type processedFlagField)
		{
			_ProcessedFlagField = processedFlagField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (!sender.Graph.IsImport)
			{
				return;
			}

			decimal? value = sender.GetValue(e.Row, _FieldName) as decimal?;
			if (value != null && value != 0)
			{
				sender.SetValue(e.Row, _ProcessedFlagField.Name, true);
			}
		}
	}
}
