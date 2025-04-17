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
using System.Linq;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType
{
    public class ProjectTaskWithTypeAttribute : ProjectTaskAttribute
    {
        private readonly Type projectField;

        public ProjectTaskWithTypeAttribute(Type projectField)
            : base(projectField)
        {
            this.projectField = projectField ?? throw new ArgumentNullException(nameof(projectField));
            var dimensionSelectorAttribute = CreateDimensionSelectorAttribute(projectField);
            _Attributes.Add(dimensionSelectorAttribute);
            _SelAttrIndex = _Attributes.Count - 1;
            Filterable = true;
        }

        public bool NeedsPrefilling
        {
            get;
            set;
        } = true;

        protected override void OnProjectUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            var projectFieldName = cache.GetField(projectField);
            var projectId = cache.GetValue(args.Row, projectFieldName) as int?;
            if (DoesRequiredDefaultProjectTaskExist(cache.Graph, projectId) && NeedsPrefilling)
            {
                base.OnProjectUpdated(cache, args);
            }
            else
            {
                cache.SetValue(args.Row, _FieldName, null);
            }
        }

        protected virtual Type GetSearchType(Type projectId)
        {
            return BqlCommand.Compose(typeof(Search<,>), typeof(PMTask.taskID),
                typeof(Where<,,>), typeof(PMTask.projectID), typeof(Equal<>), typeof(Optional<>), projectId,
                typeof(And<PMTask.type, NotEqual<ProjectTaskType.revenue>>));
        }

        protected virtual PXSelectBase<PMTask> GetRequiredDefaultProjectTaskQuery(PXGraph graph)
        {
            return new PXSelect<PMTask,
                Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                    And<PMTask.isDefault, Equal<True>,
                    And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>(graph);
        }

        private bool DoesRequiredDefaultProjectTaskExist(PXGraph graph, int? projectId)
        {
            return GetRequiredDefaultProjectTaskQuery(graph).Select(projectId).FirstTableItems.Any();
        }

        private PXDimensionSelectorAttribute CreateDimensionSelectorAttribute(Type projectId)
        {
            var searchType = GetSearchType(projectId);
            return new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(PMTask.taskCD),
                typeof(PMTask.taskCD), typeof(PMTask.type), typeof(PMTask.description), typeof(PMTask.status))
            {
                DescriptionField = typeof(PMTask.description),
                ValidComboRequired = true
            };
        }
    }
}