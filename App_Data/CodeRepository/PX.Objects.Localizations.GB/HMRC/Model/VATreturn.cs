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

namespace PX.Objects.Localizations.GB.HMRC.Model
{
	/// <summary>
	/// Submit VAT return for period
	/// </summary>
	[System.SerializableAttribute()]
	public class VATreturn
	{
		/// <summary>
		/// The ID code for the period that this obligation belongs to. 
		/// The format is a string of four alphanumeric characters. Occasionally the format includes the # symbol.
		/// For example: 18AD, 18A1, #001
		/// </summary>
		public string periodKey { get; set; }

		/// <summary>
		/// VAT due on sales and other outputs. 
		/// This corresponds to box 1 on the VAT Return form. 
		/// The value must be between -9999999999999.99 and 9999999999999.99.
		/// </summary>
		public decimal vatDueSales { get; set; }

		/// <summary>
		/// VAT due on acquisitions from other EC Member States. 
		/// This corresponds to box 2 on the VAT Return form. 
		/// The value must be between -9999999999999.99 and 9999999999999.99.
		/// </summary>
		public decimal vatDueAcquisitions { get; set; }

		/// <summary>
		/// Total VAT due (the sum of vatDueSales and vatDueAcquisitions). 
		/// This corresponds to box 3 on the VAT Return form. 
		/// The value must be between -9999999999999.99 and 9999999999999.99.
		/// </summary>
		public decimal totalVatDue { get; set; }

		/// <summary>
		/// VAT reclaimed on purchases and other inputs (including acquisitions from the EC). 
		/// This corresponds to box 4 on the VAT Return form. 
		/// The value must be between -9999999999999.99 and 9999999999999.99.
		/// </summary>
		public decimal vatReclaimedCurrPeriod { get; set; }

		/// <summary>
		/// The difference between totalVatDue and vatReclaimedCurrPeriod. 
		/// This corresponds to box 5 on the VAT Return form. 
		/// The value must be between 0.00 and 99999999999.99
		/// </summary>
		public decimal netVatDue { get; set; }

		/// <summary>
		/// Total value of sales and all other outputs excluding any VAT. 
		/// This corresponds to box 6 on the VAT Return form. 
		/// The value must be between -9999999999999 and 9999999999999.
		/// </summary>
		public decimal totalValueSalesExVAT { get; set; }

		/// <summary>
		/// Total value of purchases and all other inputs excluding any VAT (including exempt purchases). 
		/// This corresponds to box 7 on the VAT Return form. 
		/// The value must be between -9999999999999 and 9999999999999.
		/// </summary>
		public decimal totalValuePurchasesExVAT { get; set; }

		/// <summary>
		/// Total value of all supplies of goods and related costs, excluding any VAT, to other EC member states. 
		/// This corresponds to box 8 on the VAT Return form. 
		/// The value must be between -9999999999999 and 9999999999999.
		/// </summary>
		public decimal totalValueGoodsSuppliedExVAT { get; set; }

		/// <summary>
		/// Total value of acquisitions of goods and related costs excluding any VAT, from other EC member states. 
		/// This corresponds to box 9 on the VAT Return form. 
		/// The value must be between -9999999999999 and 9999999999999.
		/// </summary>
		public decimal totalAcquisitionsExVAT { get; set; }

		/// <summary>
		/// Declaration that the user has finalised their VAT return.
		/// For example: true
		/// </summary>
		public bool? finalised { get; set; }
	}
}
