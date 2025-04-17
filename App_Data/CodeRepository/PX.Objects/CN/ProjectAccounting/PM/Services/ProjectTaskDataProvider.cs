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

using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public class ProjectTaskDataProvider : IProjectTaskDataProvider
    {
        public PMTask GetProjectTask(PXGraph graph, int? projectID, int? projectTaskId)
        {
            return PMTask.PK.FindDirty(graph, projectID, projectTaskId);
        }

        public IEnumerable<PMTask> GetProjectTasks(PXGraph graph, int? projectId)
        {
            return SelectFrom<PMTask>.Where<PMTask.projectID.IsEqual<P.AsInt>>.View.Select(graph, projectId)
                .FirstTableItems;
        }

        public IEnumerable<PMTask> GetProjectTasks<TTaskType>(PXGraph graph, int? projectId)
            where TTaskType : BqlString.Constant<TTaskType>, new()
        {
            return SelectFrom<PMTask>
                .Where<PMTask.projectID.IsEqual<P.AsInt>
                    .And<PMTask.type.IsEqual<TTaskType>>>.View.Select(graph, projectId).FirstTableItems;
        }
    }
}
