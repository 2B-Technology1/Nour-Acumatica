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
using PX.Objects.CN.ProjectAccounting.Descriptor;

namespace PX.Objects.PJ.DrawingLogs.Descriptor
{
    [PXLocalizable]
    public static class DrawingLogLabels
    {
        public const string Project = "Project";
        public const string DrawingLogId = "Drawing Log ID";
        public const string OriginalDrawingId = "Original Drawing";
        public const string Discipline = "Discipline";
        public const string Status = "Status";

        [PXLocalizable]
        public static class DisciplinesOrderedSelect
        {
            public const string PasteLine = "PasteLine";
            public const string ResetOrder = "ResetOrder";
        }
    }
}