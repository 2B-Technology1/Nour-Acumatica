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

using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class ProjectIssueMaintExtension : CreatedFromDailyFieldReportExtension<ProjectIssueMaint>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportProjectIssue>
            .Where<DailyFieldReportProjectIssue.projectIssueId.IsEqual<ProjectIssue.projectIssueId.FromCurrent>>
            .View DailyFieldReportProjectIssues;

        protected override string EntityName => DailyFieldReportEntityNames.ProjectIssue;

        protected override DailyFieldReportRelatedDocumentMapping GetDailyFieldReportMapping()
        {
            return new DailyFieldReportRelatedDocumentMapping(typeof(ProjectIssue))
            {
                ReferenceId = typeof(ProjectIssue.projectIssueId)
            };
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
            return new PXSelectExtension<DailyFieldReportRelation>(DailyFieldReportProjectIssues);
        }

        protected override DailyFieldReportRelation CreateDailyFieldReportRelation(int? dailyFieldReportId)
        {
            return new DailyFieldReportRelation
            {
                RelationId = Documents.Current.ReferenceId,
                DailyFieldReportId = dailyFieldReportId
            };
        }
    }
}