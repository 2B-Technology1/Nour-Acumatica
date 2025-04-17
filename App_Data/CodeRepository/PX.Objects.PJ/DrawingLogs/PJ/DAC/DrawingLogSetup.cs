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

using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.DAC;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.DAC
{
    [PXPrimaryGraph(typeof(DrawingLogsSetupMaint))]
    [PXCacheName(CacheNames.DrawingLogPreference)]
    public class DrawingLogSetup : BaseCache, IBqlTable
    {
        [PXDBString(10, IsUnicode = true)]
        [PXDefault("DRAWINGLOG")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Drawing Log Numbering Sequence")]
        public string DrawingLogNumberingSequenceId
        {
            get;
            set;
        }

        public abstract class drawingLogNumberingSequenceId : IBqlField
        {
        }
    }
}