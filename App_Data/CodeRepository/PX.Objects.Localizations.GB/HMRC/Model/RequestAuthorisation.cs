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
	/// Authorisation
	/// </summary>
	[System.SerializableAttribute()]
	public class RequestAuthorisation
	{
		/// <summary>
		/// VAT registration number.
		/// A nine-digit number.
		/// For example: 123456789
		/// </summary>
		public string vrn { get; set; }

		/// <summary>
		/// Server token of Head of HTTP request
		/// Example: Authorization - Bearer 70fc461f8c82d2a322cfe87066ffb284
		/// </summary>
		public string serverToken { get; set; }
	}
}
