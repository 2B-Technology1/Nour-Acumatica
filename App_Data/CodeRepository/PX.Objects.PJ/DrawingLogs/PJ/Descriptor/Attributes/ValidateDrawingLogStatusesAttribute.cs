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
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.PM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateDrawingLogStatusesAttribute : ValidateDrawingLogBaseAttribute, IPXRowUpdatedSubscriber
    {
        public void RowUpdated(PXCache cache, PXRowUpdatedEventArgs args)
        {
            if (args.Row is DrawingLog drawingLog)
            {
                ValidateFields(drawingLog);
            }
        }

        protected override void ValidateDiscipline(DrawingLog drawingLog)
        {
            var discipline = DrawingLogDataProvider
                .GetDiscipline<DrawingLogDiscipline.drawingLogDisciplineId>(drawingLog.DisciplineId);
            if (discipline != null && !discipline.IsActive.GetValueOrDefault())
            {
                Cache.RaiseException<DrawingLog.disciplineId>(drawingLog,
                    DrawingLogMessages.DisciplineIsInactive, discipline.Name, PXErrorLevel.Warning);
            }
        }

        protected override void ValidateProjectTask(IProjectManagementDocumentBase drawingLog)
        {
            var projectTaskDataProvider = Cache.Graph.GetService<IProjectTaskDataProvider>();
            var projectTask = projectTaskDataProvider.GetProjectTask(Cache.Graph, drawingLog.ProjectId, drawingLog.ProjectTaskId);
            if (projectTask != null
                && projectTask.Status.IsNotIn(ProjectTaskStatus.Active, ProjectTaskStatus.Planned))
            {
                var message = CreateMessage<PMTask.status, PMTask>(projectTask, "Project Task");
                Cache.RaiseException<DrawingLog.projectTaskId>(
                    drawingLog, message, projectTask.TaskCD, PXErrorLevel.Warning);
            }
        }

        protected override void ValidateProject(IProjectManagementDocumentBase drawingLog)
        {
            var projectDataProvider = Cache.Graph.GetService<IProjectDataProvider>();
            var project = projectDataProvider.GetProject(Cache.Graph, drawingLog.ProjectId);
            if (project != null
                && project.Status.IsNotIn(ProjectStatus.Active, ProjectStatus.Planned))
            {
                var message = CreateMessage<PMProject.status, PMProject>(project, DrawingLogLabels.Project);
                Cache.RaiseException<DrawingLog.projectId>(
                    drawingLog, message, project.ContractCD, PXErrorLevel.Warning);
            }
        }

        private string CreateMessage<TStatusField, TCacheType>(object entity, string entityName)
            where TStatusField : IBqlField
            where TCacheType : IBqlTable, new()
        {
            var status =
                PXStringListAttribute.GetLocalizedLabel<TStatusField>(Cache.Graph.Caches[typeof(TCacheType)], entity);
            return string.Format(DrawingLogMessages.RecordHasStatus, entityName, status);
        }
    }
}
