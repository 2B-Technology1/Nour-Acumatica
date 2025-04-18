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

using System.Linq;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Web.UI;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Revisions
{
    public class DrawingLogEntryRevisionsExtension : DrawingLogRevisionsBaseExtension<DrawingLogEntry>
    {
        public PXAction<DrawingLog> NewRevisionSketch;

        public PXDelete<DrawingLog> Delete;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        public new static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXUIField(DisplayName = "Create Revision",
            MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(CommitChanges = true, Category = PX.Objects.PM.Messages.Processing, DisplayOnMainToolbar = true)]
        public virtual void newRevisionSketch()
        {
            Base.Persist();
            var graph = PXGraph.CreateInstance<DrawingLogEntry>();
            var drawingLog = CreateDrawingLogWithNewRevisionSketch(Base.DrawingLog.Current);
            graph.DrawingLog.Insert(drawingLog);
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        [PXDeleteButton(ImageKey = Sprite.Main.Remove, Tooltip = ActionsMessages.Delete, ConfirmationType = PXConfirmationType.Unspecified)]
        [PXUIField]
        public virtual void delete()
        {
            CheckRevisionsAvailability(Base.DrawingLog.Current.NoteID);
            if (IsDeleteConfirmed())
            {
                Base.Delete.Press();
            }
        }

        public override void _(Events.RowSelected<DrawingLog> args)
        {
            NewRevisionSketch.SetEnabled(IsNewRevisionSketchActionAvailable());
            base._(args);
        }

        public void _(Events.RowPersisted<DrawingLog> args)
        {
            var drawingLog = args.Row;
            if (IsNewRevisionDrawingLog(drawingLog))
            {
                UpdateRevisionDrawingLogFields(args.Cache, drawingLog);
                InsertOtherRevisionsRelations(drawingLog);
                InsertOriginalDrawingRelation(drawingLog);
                Base.Caches<DrawingLogRevision>().Persist(PXDBOperation.Insert);
                Base.Caches<DrawingLogRevision>().Clear();
            }
        }

        protected override WebDialogResult ShowConfirmationDialog(string message)
        {
            return Base.DrawingLog.Ask(SharedMessages.Warning, message, MessageButtons.OKCancel);
        }

        private bool IsDeleteConfirmed()
        {
			if(Base.IsMobile)
			{
				return true;
			}

            return Base.DrawingLog.Ask(SharedMessages.Warning, DrawingLogMessages.DrawingLogWillBeDeleted,
                MessageButtons.OKCancel).IsPositive();
        }

        private void InsertOriginalDrawingRelation(DrawingLog drawingLog)
        {
            var rootDrawingLog = DrawingLogDataProvider.GetDrawingLogs<DrawingLog.noteID>(drawingLog.OriginalDrawingId)
                .SingleOrDefault();
            InsertRevisionRelation(rootDrawingLog, drawingLog);
        }

        private void InsertOtherRevisionsRelations(DrawingLog revision)
        {
            var otherRevisions = DrawingLogDataProvider.GetDrawingLogs<DrawingLog.originalDrawingId>(
                revision.OriginalDrawingId).Except(revision);
            foreach (var otherRevision in otherRevisions)
            {
                InsertRevisionRelation(otherRevision, revision);
                InsertRevisionRelation(revision, otherRevision);
            }
        }

        private void InsertRevisionRelation(DrawingLog otherRevision, DrawingLog revision)
        {
            var revisionRelation = CreateRevision(otherRevision.DrawingLogId, revision.DrawingLogId);
            Base.Caches<DrawingLogRevision>().Insert(revisionRelation);
        }

        private static DrawingLogRevision CreateRevision(int? originalDrawingLogId, int? drawingLogId)
        {
            return new DrawingLogRevision
            {
                DrawingLogId = originalDrawingLogId,
                DrawingLogRevisionId = drawingLogId
            };
        }

        private void UpdateRevisionDrawingLogFields(PXCache cache, DrawingLog drawingLog)
        {
            var originalDrawingLog = DrawingLogDataProvider
                .GetDrawingLogs<DrawingLog.noteID>(drawingLog.OriginalDrawingId).SingleOrDefault();
            UpdateFields(originalDrawingLog, drawingLog);
            cache.Update(drawingLog);
        }

        private bool IsNewRevisionSketchActionAvailable()
        {
            var drawingLog = Base.DrawingLog.Current;
            if (!Base.IsDrawingLogSaved(drawingLog))
            {
                return false;
            }
            var project = ProjectDataProvider.GetProject(Base, drawingLog.ProjectId);
            var projectTask = ProjectTaskDataProvider.GetProjectTask(Base, drawingLog.ProjectId, drawingLog.ProjectTaskId);
            var discipline = DrawingLogDataProvider
                .GetDiscipline<DrawingLogDiscipline.drawingLogDisciplineId>(drawingLog.DisciplineId);
            return IsNewRevisionSketchActionAvailable(project, projectTask, discipline);
        }

        private static bool IsNewRevisionSketchActionAvailable(PMProject project, PMTask projectTask,
            DrawingLogDiscipline discipline)
        {
            return project?.Status.IsIn(ProjectStatus.Active, ProjectStatus.Planned) == true
                && (projectTask == null ||
                    projectTask.Status.IsIn(ProjectTaskStatus.Active, ProjectTaskStatus.Planned))
                && discipline?.IsActive == true;
        }

        private DrawingLog CreateDrawingLogWithNewRevisionSketch(DrawingLog drawingLog)
        {
            var newDrawingLog = PXCache<DrawingLog>.CreateCopy(drawingLog);
            newDrawingLog.ReceivedDate = null;
            newDrawingLog.DrawingDate = null;
            newDrawingLog.DrawingLogId = null;
            newDrawingLog.DrawingLogCd = null;
            newDrawingLog.NoteID = null;
            newDrawingLog.Revision = null;
            newDrawingLog.Sketch = null;
            newDrawingLog.OriginalDrawingId = drawingLog.OriginalDrawingId ?? drawingLog.NoteID;
            return newDrawingLog;
        }

        private bool IsNewRevisionDrawingLog(DrawingLog drawingLog)
        {
            return !Base.IsDrawingLogSaved(drawingLog) && drawingLog.OriginalDrawingId != null;
        }
    }
}
