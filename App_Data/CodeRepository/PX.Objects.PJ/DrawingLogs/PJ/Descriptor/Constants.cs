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

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor
{
    public class Constants
    {
        public const string DrawingLogsZipFileName = "Drawing Logs.zip";
        public const string DrawingLogClassId = "DRAWINGLOGS";
        public const string DisciplineNameField = "Name";
        public const string DisciplineSortOrderField = "SortOrder";
        public const string DrawingLogCdSearchPattern = "DL-";

        public static string DisciplineViewName = string.Concat("_Cache#PX.Objects.PJ.DrawingLogs.PJ.DAC",
            ".DrawingLog_DisciplineId_PX.Objects.PJ.DrawingLogs.PJ.DAC.DrawingLogDiscipline+drawingLogDisciplineId_");

        public static string DisciplineFilterViewName = string.Concat("_Cache#PX.Objects.PJ.DrawingLogs.PJ.DAC",
            ".DrawingLogFilter_DisciplineId_PX.Objects.PJ.DrawingLogs.PJ.DAC.DrawingLogDiscipline+drawingLogDisciplineId_");
    }
}
