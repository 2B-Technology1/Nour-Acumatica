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

using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public abstract class ValidateDrawingLogBaseAttribute : PXEventSubscriberAttribute
    {
        protected DrawingLogDataProvider DrawingLogDataProvider;
        protected PXCache Cache;

        public override void CacheAttached(PXCache cache)
        {
            DrawingLogDataProvider = new DrawingLogDataProvider(cache.Graph);
            Cache = cache;
        }

        protected void ValidateFields(DrawingLog drawingLog)
        {
            if (drawingLog.ProjectId.HasValue)
            {
                ValidateProject(drawingLog);
            }
            if (drawingLog.ProjectTaskId.HasValue)
            {
                ValidateProjectTask(drawingLog);
            }
            if (drawingLog.DisciplineId.HasValue)
            {
                ValidateDiscipline(drawingLog);
            }
        }

        protected abstract void ValidateDiscipline(DrawingLog drawingLog);

        protected abstract void ValidateProjectTask(IProjectManagementDocumentBase drawingLog);

        protected abstract void ValidateProject(IProjectManagementDocumentBase drawingLog);
    }
}