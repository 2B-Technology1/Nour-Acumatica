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
	/// HTTP status: 201 (Created)
	/// </summary>
	[System.SerializableAttribute()]
	public class VaTreturnResponse
	{
		/// <summary>
		/// The time that the message was processed in the system.
		/// </summary>		///
		public string processingDate { get; set; }

		/// <summary>
		/// Unique number that represents the form bundle.
		/// The system stores VAT Return data in forms, which are held in a unique form bundle.
		/// Must conform to the regular expression ^[0-9]{12}$
		/// </summary>
		public string paymentIndicator { get; set; }

		/// <summary>
		/// Is DD if the netVatDue value is a debit and HMRC holds a Direct Debit Instruction for the client.
		/// Is BANK if the netVatDue value is a credit and HMRC holds the client's bank data.
		/// Otherwise not present.
		/// Limited to the following possible values:
		/// DD
		/// BANK
		/// </summary>
		public string formBundleNumber { get; set; }

		/// <summary>
		/// The charge reference number is returned, only if the netVatDue value is a debit.
		/// Between 1 and 16 characters.
		/// </summary>
		public string chargeRefNumber { get; set; }

	}
}
