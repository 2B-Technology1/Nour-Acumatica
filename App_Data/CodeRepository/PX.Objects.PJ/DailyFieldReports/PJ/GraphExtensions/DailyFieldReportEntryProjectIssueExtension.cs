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
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.CacheExtensions.PJ;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryProjectIssueExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.ProjectIssues)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportProjectIssue>
            .LeftJoin<ProjectIssue>
                .On<DailyFieldReportProjectIssue.projectIssueId.IsEqual<ProjectIssue.projectIssueId>>
            .Where<DailyFieldReportProjectIssue.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View ProjectIssues;

        public PXAction<DailyFieldReport> CreateProjectIssue;

        public PXAction<DailyFieldReport> ViewProjectIssue;

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.ProjectIssue, ViewNames.ProjectIssues);

        [PXButton]
        [PXUIField]
        public virtual void viewProjectIssue()
        {
            var projectIssueMaint = PXGraph.CreateInstance<ProjectIssueMaint>();
            projectIssueMaint.ProjectIssue.Current = projectIssueMaint.ProjectIssue
                .Search<ProjectIssue.projectIssueId>(ProjectIssues.Current.ProjectIssueId);
            PXRedirectHelper.TryRedirect(projectIssueMaint, PXRedirectHelper.WindowMode.NewWindow);
        }

        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        [PXUIField(DisplayName = "Create New Project Issue")]
        public virtual void createProjectIssue()
        {
            Base.Actions.PressSave();
            var projectIssueMaint = PXGraph.CreateInstance<ProjectIssueMaint>();
            InsertProjectIssue(projectIssueMaint);
            PXRedirectHelper.TryRedirect(projectIssueMaint, PXRedirectHelper.WindowMode.NewWindow);
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isActionAvailable = IsCreationActionAvailable(dailyFieldReport);
                CreateProjectIssue.SetEnabled(isActionAvailable);
            }
        }

        public virtual void _(Events.FieldUpdated<DailyFieldReportProjectIssue,
            DailyFieldReportProjectIssue.projectIssueId> args)
        {
            var projectIssueIsDuplicated = Base.IsMobile || args.OldValue != null
                ? ProjectIssuesViewHasAtLeastTwoSameProjectIssues(args.Row)
                : ProjectIssuesViewHasSameProjectIssues(args.Row);
            if (projectIssueIsDuplicated)
            {
                RaiseDailyFieldReportException(args.Cache, args.Row);
            }
        }

        public virtual void _(Events.RowPersisting<DailyFieldReportProjectIssue> args)
        {
            if (ProjectIssuesViewHasAtLeastTwoSameProjectIssues(args.Row))
            {
                RaiseDailyFieldReportException(args.Cache, args.Row);
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportProjectIssue))
            {
                RelationId = typeof(DailyFieldReportProjectIssue.projectIssueId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(ProjectIssues);
        }

        private void InsertProjectIssue(ProjectIssueMaint projectIssueMaint)
        {
            var projectIssue = projectIssueMaint.ProjectIssue.Insert();
            var dailyFieldReport = Base.DailyFieldReport.Current;
            projectIssue.ProjectId = dailyFieldReport.ProjectId;
            projectIssue.CreationDate = dailyFieldReport.Date;
            projectIssueMaint.ProjectIssue.Cache.SetValueExt<ProjectIssueExt.dailyFieldReportId>(projectIssue,
                dailyFieldReport.DailyFieldReportId);
        }

        private void RaiseDailyFieldReportException(PXCache cache,
            DailyFieldReportProjectIssue dailyFieldReportProjectIssue)
        {
            var projectIssue = GetProjectIssue(dailyFieldReportProjectIssue.ProjectIssueId);
            if (projectIssue == null)
            {
                return;
            }
            var message = string.Format(DailyFieldReportMessages.EntityCannotBeSelectedTwice, Name.Entity);
            cache.RaiseException<DailyFieldReportProjectIssue.projectIssueId>(dailyFieldReportProjectIssue,
                message, projectIssue.ProjectIssueCd);
        }

        private bool ProjectIssuesViewHasAtLeastTwoSameProjectIssues(
            DailyFieldReportProjectIssue dailyFieldReportProjectIssue)
        {
            return ProjectIssues.Cache.Cached.Cast<DailyFieldReportProjectIssue>()
                .Where(pi => pi.ProjectIssueId == dailyFieldReportProjectIssue.ProjectIssueId
                    && pi.DailyFieldReportId == dailyFieldReportProjectIssue.DailyFieldReportId).HasAtLeastTwoItems();
        }

        private bool ProjectIssuesViewHasSameProjectIssues(DailyFieldReportProjectIssue dailyFieldReportProjectIssue)
        {
            return ProjectIssues.SelectMain().Any(pi => pi.ProjectIssueId == dailyFieldReportProjectIssue.ProjectIssueId
                && pi.DailyFieldReportProjectIssueId != dailyFieldReportProjectIssue.DailyFieldReportProjectIssueId);
        }

        private ProjectIssue GetProjectIssue(int? projectIssueId)
        {
            return SelectFrom<ProjectIssue>
                .Where<ProjectIssue.projectIssueId.IsEqual<P.AsInt>>
                .View.Select(Base, projectIssueId);
        }
    }
}