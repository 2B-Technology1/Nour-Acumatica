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
using PX.Data;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.PM;
using PmMessages = PX.Objects.PM.Messages;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType
{
    public class ActiveProjectTaskWithTypeAttribute : ProjectTaskWithTypeAttribute
    {
        public ActiveProjectTaskWithTypeAttribute(Type projectField)
            : base(projectField)
        {
            _Attributes.Add(new PXRestrictorAttribute(typeof(Where<PMTask.isCompleted, NotEqual<True>>),
                PmMessages.ProjectTaskIsCompleted));
            _Attributes.Add(new PXRestrictorAttribute(typeof(Where<PMTask.isCancelled, NotEqual<True>>),
                PmMessages.ProjectTaskIsCanceled));
            _Attributes.Add(new PXRestrictorAttribute(typeof(Where<PMTask.status, NotEqual<ProjectTaskStatus.planned>>),
                PmMessages.InactiveTask, typeof(PMTask.taskCD)));
            _Attributes.Add(new PXRestrictorAttribute(
                typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
                ProjectAccountingMessages.TaskTypeIsNotAvailable));
        }

        protected override Type GetSearchType(Type projectId)
        {
            return BqlCommand.Compose(typeof(Search<,>), typeof(PMTask.taskID),
                typeof(Where<,,>), typeof(PMTask.projectID), typeof(Equal<>), typeof(Optional<>), projectId,
                typeof(And<PMTask.type, NotEqual<ProjectTaskType.revenue>,
                    And<PMTask.status, Equal<ProjectTaskStatus.active>>>));
        }

        protected override PXSelectBase<PMTask> GetRequiredDefaultProjectTaskQuery(PXGraph graph)
        {
            var query = base.GetRequiredDefaultProjectTaskQuery(graph);
            query.WhereAnd<Where<PMTask.status, Equal<ProjectTaskStatus.active>>>();
            return query;
        }
    }
}
