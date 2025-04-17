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
using PX.Objects.PJ.DailyFieldReports.PJ.CacheExtensions.PJ;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Project Issue")]
    public class DailyFieldReportProjectIssue : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportProjectIssueId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBInt]
        [PXSelector(typeof(SearchFor<ProjectIssue.projectIssueId>
            .Where<ProjectIssue.projectIssueId
                .IsNotInSubselect<SearchFor<projectIssueId>
                    .Where<dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>>
                .And<ProjectIssue.projectId.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                .Or<ProjectIssueExt.dailyFieldReportId.FromCurrent.IsNotNull>>),
            SubstituteKey = typeof(ProjectIssue.projectIssueCd))]
        [PXParent(typeof(SelectFrom<ProjectIssue>
            .Where<ProjectIssue.projectIssueId.IsEqual<projectIssueId.FromCurrent>>))]
        [PXUIField(DisplayName = "Project Issue ID", Required = true)]
        public virtual int? ProjectIssueId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportProjectIssueId : BqlInt.Field<dailyFieldReportProjectIssueId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class projectIssueId : BqlInt.Field<projectIssueId>
        {
        }
    }
}