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

using PX.Common;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.Descriptor
{
    [PXLocalizable]
    public static class RequestForInformationMessages
    {
        public const string AttachButtonTooltip = "Attach files from the related entity.";
        public const string RequestForInformationReportNamePattern = "RFI {0} ({1}).pdf";
        public const string OnlyActiveContactsAreAllowed = "Only Active Contacts are allowed.";
        public const string ContactBelongsToAnotherBusinessAccount = "Contact belongs to another business account.";
        public const string BusinessAccountRestrictionType = "Business Account is {0}.";
        public const string BusinessAccountRestrictionStatus = "Business Account is Inactive.";
        public const string RequestForInformationEmailDefaultSubject = "[RFI #{0}] {1}";

        public const string ClassChangeWillRemoveAttributes =
            "Changing the RFI class will remove all attribute values associated with the current class and " +
            "replace them with the attribute values of the new class. Continue?";

        public const string UnlinkDrawingLogsOnProjectChange =
            "Changing the Project unlinks the Drawing Log Documents associated with RFI where Project " +
            "no longer matches. Continue?";

        [PXLocalizable]
        public static class NotificationTemplate
        {
            private const string Yes = "Yes";
            private const string YesWithComma = "Yes,";
            private const string No = "No";

            public class yes : BqlString.Constant<yes>
            {
                public yes()
                    : base(Yes)
                {
                }
            }

            public class yesWithComma : BqlString.Constant<yesWithComma>
            {
                public yesWithComma()
                    : base(YesWithComma)
                {
                }
            }

            public class no : BqlString.Constant<no>
            {
                public no()
                    : base(No)
                {
                }
            }
        }
    }
}