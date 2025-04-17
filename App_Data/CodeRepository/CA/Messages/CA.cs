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

namespace PX.Objects.Localizations.CA.Messages
{
    [PX.Common.PXLocalizable]
    public static class CA
    {
        public const string
            MailingIdNotFound = "The mailing ID ({0}) is not defined or is not active. Specify the mailing on the Mailing & Printing tab of the Accounts Payable Preferences (AP101000) form.";

        public const string
            EmailStatus = "Emails sent: {0}, Emails not sent: {1}. See the error messages for more information.";

        /// {0} Vendor name.
        /// {1} Name of MailingId to use (should be PAYMENTNOTICE).
        public const string
            CheckMailingOnVendor = "The settings of the vendor {0} are incorrect. On the Vendors (AP303000) form, verify that on the Mailing Settings tab, in the Mailings table, the Active check box is selected for the Mailing ID {1}.";

        /// {0} System error message received from exception.
        public const string
            EmailFailed = "The email message has not been sent.  See the following error message for more information: {0}.";

        public const string
            EmailSent = "The email has been sent.";
    }
}
