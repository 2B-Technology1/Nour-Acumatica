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

namespace PX.Objects.PJ.RequestsForInformation.Descriptor
{
    public static class RequestForInformationConstants
    {
        public static class Print
        {
            public const string RequestForInformationId = "RFIID";
            public const string EmailId = "EmailId";
        }

        public static class EmailTemplate
        {
	        public const string NotificationName = "E-Mail Request For Information (v2)";

	        public const int IndexNotFound = -1;

	        public const string ResponseNoteTag =
		        "<p class=\"richp\">To respond to the RFI, reply to this email without changing the subject.</p>";

	        public const string RecipientNotesTag =
		        "<p class=\"richp\" style=\"text-align: left;\">{0}</p><p class=\"richp\"><br></p>";

	        public const string LogoTag =
		        "<p class=\"richp\" style=\"text-align: left;\"><img src=\"{0}\" objtype=\"file\" data-convert=\"view\" embedded=\"true\" title=\"{1}\"><br></p>";
        }
	}
}