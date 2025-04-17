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

namespace PX.Objects.PJ.PhotoLogs.Descriptor
{
    [PXLocalizable]
    public class PhotoLogMessages
    {
        public const string PhotoLogs = "photo logs";

        public const string AnotherPhotoHasAlreadyBeenMarkedAsTheMainPhoto =
            "Another photo has already been marked as the main photo in the photo log. Would you like to mark the current photo as the main photo instead?";

        public const string NoRecordsWereSelected = "Select one or multiple photo logs.";

        public const string NoPhotosAddedToPhotoLog = "At least one photo should be added to a photo log.";

        public const string NoPhotosInSelectedPhotoLogs = "There are no photos in the selected photo logs.";
    }
}
