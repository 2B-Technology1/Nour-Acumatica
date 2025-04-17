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
using PX.Data;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public abstract class ProjectTaskValidationServiceBase
    {
        private readonly PXGraph graph;

        protected ProjectTaskValidationServiceBase()
        {
			graph = PXGraph.CreateInstance(typeof(PXGraph));
		}

        protected abstract bool IsTaskUsedInCostDocument(int? projectID, int? taskId);

        protected abstract bool IsTaskUsedInRevenueDocument(int? projectID, int? taskId);

        protected bool IsTaskUsed<TTable, TProjectField, TProjectTaskField>(int? projectID, int? projectTaskId)
            where TTable : class, IBqlTable, new()
            where TProjectField : IBqlField
			where TProjectTaskField : IBqlField
		{
            return new PXSelect<TTable,
                    Where<TProjectField, Equal<Required<TProjectField>>, And<TProjectTaskField, Equal<Required<TProjectTaskField>>>>>(graph)
                .Select(projectID, projectTaskId).FirstTableItems.Any();
        }

        protected bool IsTaskUsed<TTable, TProjectField, TProjectTaskField, TBudgetTypeField>(int? projectID, int? projectTaskId, string budgetType)
            where TTable : class, IBqlTable, new()
			where TProjectField : IBqlField
			where TProjectTaskField : IBqlField
            where TBudgetTypeField : IBqlField
        {
            return new PXSelect<TTable,
                    Where<TProjectField, Equal<Required<TProjectField>>, And<TProjectTaskField, Equal<Required<TProjectTaskField>>,
                        And<TBudgetTypeField, Equal<Required<TBudgetTypeField>>>>>>(graph)
                .Select(projectID, projectTaskId, budgetType).FirstTableItems.Any();
        }
    }
}
