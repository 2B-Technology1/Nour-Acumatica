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
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes
{
    public class ProjectTaskTypeValidationAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
    {
        private IProjectTaskDataProvider projectTaskDataProvider;

        public Type ProjectIdField
        {
            get;
            set;
        }

		public Type ProjectTaskIdField
		{
			get;
			set;
		}

		public string WrongProjectTaskType
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public void RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (cache.GetValue(args.Row, ProjectIdField) is int projectId &&
				cache.GetValue(args.Row, _FieldName) is int projectTaskId)
            {
                ValidateProjectTaskType(cache, args.Row, projectId, projectTaskId);
            }
        }

        private void ValidateProjectTaskType(PXCache cache, object row, int? projectId, int? projectTaskId)
        {
            projectTaskDataProvider = cache.Graph.GetService<IProjectTaskDataProvider>();
            var projectTask = projectTaskDataProvider.GetProjectTask(cache.Graph, projectId, projectTaskId);
            if (projectTask != null)
            {
                if (projectTask.Type == WrongProjectTaskType)
                {
                    cache.RaiseException(_FieldName, row, Message, projectTask.TaskCD);
                }
            }
        }
    }
}
