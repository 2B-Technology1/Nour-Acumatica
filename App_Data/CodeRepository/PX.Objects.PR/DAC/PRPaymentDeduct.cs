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
using PX.Payroll.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using System;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	/// <summary>
	/// Includes the settings of the deduction or benefit code which are used for the current payment.
	/// </summary>
	[PXCacheName(Messages.PRDeductionSummary)]
	[Serializable]
	public class PRPaymentDeduct : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPaymentDeduct>.By<docType, refNbr, codeID>
		{
			public static PRPaymentDeduct Find(PXGraph graph, string docType, string refNbr, int? codeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, codeID, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentDeduct>.By<docType, refNbr> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRPaymentDeduct>.By<codeID> { }
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		/// <summary>
		/// The type of the payment.
		/// The field is included in <see cref="FK.Payment"/>.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PayrollType.ListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		/// <summary>
		/// The unique identifier of the payment.
		/// The field is included in <see cref="FK.Payment"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<PRPaymentDeduct.docType>>, And<PRPayment.refNbr, Equal<Current<PRPaymentDeduct.refNbr>>>>>))]
		public String RefNbr { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		/// <summary>
		/// The unique identifier of the deduction or benefit.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Deduction Code")]
		[DeductionActiveSelector(null, typeof(paymentCountryID))]
		[PXDefault]
		[PXForeignReference(typeof(FK.DeductionCode))]
		public int? CodeID { get; set; }
		#endregion
		#region IsGarnishment
		public abstract class isGarnishment : PX.Data.BQL.BqlBool.Field<isGarnishment> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the code is used for a garnishment.
		/// </summary>
		[PXDBBool]
		[PXDefault]
		[PXUIField(DisplayName = "Garnishment", Enabled = false)]
		[PXFormula(typeof(Selector<PRPaymentDeduct.codeID, PRDeductCode.isGarnishment>))]
		public bool? IsGarnishment { get; set; }
		#endregion
		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		/// <summary>
		/// The type of a code that defines how the code affects employee earnings.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="ContributionTypeListAttribute"/>.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Contribution Type", Enabled = false)]
		[ContributionTypeList]
		[PXFormula(typeof(Selector<PRPaymentDeduct.codeID, PRDeductCode.contribType>))]
		public string ContribType { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		/// <summary>
		/// The amount deducted from net pay for the paycheck.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Deduction Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(dedAmount.When<isActive.IsEqual<True>>.Else<decimal0>),
			typeof(SumCalc<PRPayment.dedAmount>))]
		public Decimal? DedAmount { get; set; }
		#endregion
		#region CntAmount
		public abstract class cntAmount : PX.Data.BQL.BqlDecimal.Field<cntAmount> { }
		/// <summary>
		/// The amount that the employer is required to contribute, which is not deducted from the net pay but creates a liability for the employer.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Employer Contribution")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(cntAmount.When<isActive.IsEqual<True>>.Else<decimal0>),
			typeof(SumCalc<PRPayment.benefitAmount>))]
		[PXUnboundFormula(
			typeof(cntAmount.When<isActive.IsEqual<True>
				.And<Where<Selector<codeID, PRDeductCode.isPayableBenefit>, Equal<True>>>>
				.Else<decimal0>),
			typeof(SumCalc<PRPayment.payableBenefitAmount>))]
		public Decimal? CntAmount { get; set; }
		#endregion
		#region YtdAmount
		public abstract class ytdAmount : PX.Data.BQL.BqlDecimal.Field<ytdAmount> { }
		/// <summary>
		/// The year-to-date total deduction amount of this code for this employee, including the current paycheck.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "YTD Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? YtdAmount { get; set; }
		#endregion
		#region EmployerYtdAmount
		public abstract class employerYtdAmount : PX.Data.BQL.BqlDecimal.Field<employerYtdAmount> { }
		/// <summary>
		/// The year-to-date total employer contribution amount of this code for this employee, including the current paycheck.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "YTD Employer Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? EmployerYtdAmount { get; set; }
		#endregion
		#region WageBaseAmount
		public abstract class wageBaseAmount : PX.Data.BQL.BqlDecimal.Field<wageBaseAmount> { }
		/// <summary>
		/// The taxable income amount for the current paycheck.
		/// </summary>
		[PRCurrency]
		[PXUIField(DisplayName = "Wage Base Amount")]
		public Decimal? WageBaseAmount { get; set; }
		#endregion
		#region WageBaseHours
		public abstract class wageBaseHours : PX.Data.BQL.BqlDecimal.Field<wageBaseHours> { }
		/// <summary>
		/// The number of hours which generate taxable income for the current paycheck.
		/// </summary>
		[PXDBDecimal]
		[PXUIField(DisplayName = "Wage Base Hours")]
		public Decimal? WageBaseHours { get; set; }
		#endregion
		#region SaveOverride
		public abstract class saveOverride : PX.Data.BQL.BqlBool.Field<saveOverride> { }
		/// <summary>
		/// A boolean value that indicates (if set to <see langword="true" />) that the manually entered amounts will override the ones specified for the deduction or benefit code.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Save Override")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<PRPaymentDeduct.isActive.IsEqual<True>>))]
		[PXFormula(typeof(Switch<Case<Where<PRPaymentDeduct.isActive.IsEqual<False>>, False>, PRPaymentDeduct.saveOverride>))]
		public virtual bool? SaveOverride { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive: PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the code is available for use.
		/// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Source", Enabled = false)]
		[PaymentDeductionSource(typeof(codeID))]
		[PXFormula(typeof(Default<codeID>))]
		public virtual string Source { get; set; }
		#endregion

		#region NoFinancialTransaction
		public abstract class noFinancialTransaction : PX.Data.BQL.BqlBool.Field<noFinancialTransaction> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the deduction or benefit produces no financial transactions.
		/// </summary>
		[PXBool]
		[PXUnboundDefault(typeof(Selector<codeID, PRDeductCode.noFinancialTransaction>))]
		public bool? NoFinancialTransaction { get; set; }
		#endregion
		#region PaymentCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(IIf<
			Where<Current<PRPayment.countryID>, IsNotNull>, Current<PRPayment.countryID>,
			Selector<codeID, PRDeductCode.countryID>>))]
		public virtual string PaymentCountryID { get; set; }
		public abstract class paymentCountryID : PX.Data.BQL.BqlString.Field<paymentCountryID> { }
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
}
