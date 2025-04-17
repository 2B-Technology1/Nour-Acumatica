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

using System;
using PX.Data;
using PX.Common;

namespace PX.Objects.GDPR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages

		public const string Prefix = "GDPR Error";

		public const string NoConsent = "Consent to the processing of personal data has not been given or has expired.";
		public const string IsConsented = "Consented to the Processing of Personal Data";
		public const string DateOfConsent = "Date of Consent";
		public const string ConsentExpires = "Consent Expires";
		public const string ConsentExpired = "The consent has expired.";
		public const string ConsentDateNull = "No consent date has been specified.";

		public const string NotPseudonymized = "Not Pseudonymized";
		public const string Pseudonymized = "Pseudonymized";
		public const string Erased = "Erased";

		public const string NavigateDeleted = "A deleted contact cannot be opened.";
		public const string OpenContact = "Open Contact";

		public const string Pseudonymize = "Pseudonymize";
		public const string PseudonymizeAll = "Pseudonymize All";

		public const string Erase = "Erase";
		public const string EraseAll = "Erase All";

		public const string Restore = "Restore";

		public const string ContactOPTypeForIndex = "Opportunity Contact {0}";
		
		#endregion
		
		public static string GetLocal(string message)
		{
			return PXLocalizer.Localize(message, typeof(Messages).FullName);
		}
	}
}
