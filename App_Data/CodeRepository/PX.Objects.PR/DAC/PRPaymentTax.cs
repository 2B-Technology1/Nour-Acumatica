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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the tax information about a specific paycheck. The information will be displayed on the Paychecks and Adjustments (PR302000) form.
	/// </summary>
	[PXCacheName(Messages.PRPaymentTax)]
	[Serializable]
	public class PRPaymentTax : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRPaymentTax>.By<docType, refNbr, taxID>
		{
			public static PRPaymentTax Find(PXGraph graph, string docType, string refNbr, int? taxID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, docType, refNbr, taxID, options);
		}

		public static class FK
		{
			public class Payment : PRPayment.PK.ForeignKeyOf<PRPaymentTax>.By<docType, refNbr> { }
			public class TaxCode : PRTaxCode.PK.ForeignKeyOf<PRPaymentTax>.By<taxID> { }
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<PRPaymentTax.docType>>, And<PRPayment.refNbr, Equal<Current<PRPaymentTax.refNbr>>>>>))]
		public String RefNbr { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Code")]
		[PXSelector(
			typeof(PRTaxCode.taxID),
			DescriptionField = typeof(PRTaxCode.description),
			SubstituteKey = typeof(PRTaxCode.taxCD))]
		[PXUIEnabled(typeof(Where<taxID.IsNull>))]
		public int? TaxID { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategory : PX.Data.BQL.BqlString.Field<taxCategory> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax Category", Enabled = false)]
		[TaxCategory.List]
		[PXFormula(typeof(Selector<PRPaymentTax.taxID, PRTaxCode.taxCategory>))]
		public string TaxCategory { get; set; }
		#endregion
		#region TaxAmount
		public abstract class taxAmount : PX.Data.BQL.BqlDecimal.Field<taxAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Tax Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(taxAmount.When<taxCategory.IsEqual<TaxCategory.employeeWithholding>>.Else<decimal0>), typeof(SumCalc<PRPayment.taxAmount>))]
		[PXUnboundFormula(typeof(taxAmount.When<taxCategory.IsEqual<TaxCategory.employerTax>>.Else<decimal0>), typeof(SumCalc<PRPayment.employerTaxAmount>))]
		public Decimal? TaxAmount { get; set; }
		#endregion
		#region YtdAmount
		public abstract class ytdAmount : PX.Data.BQL.BqlDecimal.Field<ytdAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "YTD Amount", Enabled = false)]
		[YtdAmountDefault]
		public Decimal? YtdAmount { get; set; }
		#endregion
		#region WageBaseAmount
		public abstract class wageBaseAmount : PX.Data.BQL.BqlDecimal.Field<wageBaseAmount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Taxable Wages", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? WageBaseAmount { get; set; }
		#endregion
		#region WageBaseHours
		public abstract class wageBaseHours : PX.Data.BQL.BqlDecimal.Field<wageBaseHours> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Taxable Hours")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? WageBaseHours { get; set; }
		#endregion
		#region WageBaseGrossAmt
		public abstract class wageBaseGrossAmt : PX.Data.BQL.BqlDecimal.Field<wageBaseGrossAmt> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Taxable Gross")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public Decimal? WageBaseGrossAmt { get; set; }
		#endregion
		#region AdjustedGrossAmount
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Adjusted Gross Amount")]
		public virtual decimal? AdjustedGrossAmount { get; set; }
		public abstract class adjustedGrossAmount : PX.Data.BQL.BqlDecimal.Field<adjustedGrossAmount> { }
		#endregion
		#region ExemptionAmount
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Exemption Amount")]
		public virtual decimal? ExemptionAmount { get; set; }
		public abstract class exemptionAmount : PX.Data.BQL.BqlDecimal.Field<exemptionAmount> { }
		#endregion

		#region PaymentCountryID
		[PXString(2)]
		[PXUnboundDefault(typeof(Parent<PRPayment.countryID>))]
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

	public class YtdAmountDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), nameof(PRPaymentTax.taxID), (cache, e) =>
			{
				PRPaymentTax row = e.Row as PRPaymentTax;
				if (GetYtdAmount(sender, row, out decimal newValue))
				{
					cache.SetValue<PRPaymentTax.ytdAmount>(row, newValue);
				}
			});
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PRPaymentTax row = e.Row as PRPaymentTax;
			e.Cancel = GetYtdAmount(sender, row, out decimal newValue);
			e.NewValue = newValue;
		}

		private bool GetYtdAmount(PXCache sender, PRPaymentTax row, out decimal newValue)
		{
			newValue = 0m;
			if (row?.RefNbr == null || row?.DocType == null || row?.TaxID == null)
			{
				return false;
			}

			PRPayment payment = PXParentAttribute.SelectParent<PRPayment>(sender, row);
			if (payment.TransactionDate == null)
			{
				return false;
			}

			newValue = new YtdRecordQuery(sender.Graph).SelectSingle(payment.TransactionDate.Value.Year.ToString(), payment.EmployeeID, row.TaxID)?.Amount ?? 0m;
			return true;
		}

		private class YtdRecordQuery : SelectFrom<PRYtdTaxes>
			.Where<PRYtdTaxes.year.IsEqual<P.AsString>
				.And<PRYtdTaxes.employeeID.IsEqual<P.AsInt>>
				.And<PRYtdTaxes.taxID.IsEqual<P.AsInt>>>.View
		{
			public YtdRecordQuery(PXGraph graph) : base(graph) { }
		}
	}
}
