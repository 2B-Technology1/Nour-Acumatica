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
using PX.Objects.PM;

namespace PX.Objects.CN.Common.Services.DataProviders
{
    public class BudgetedCostCodeDataProvider
    {
		public static IEnumerable<PMBudgetedCostCode> GetBudgetedCostCodes(PXGraph graph, int? projectId, int? projectTaskId)
		{
			return SelectFrom<PMBudgetedCostCode>
				.Where<PMBudgetedCostCode.projectID.IsEqual<P.AsInt>
					.And<PMBudgetedCostCode.projectTaskID.IsEqual<P.AsInt>>>.View
				.Select(graph, projectId, projectTaskId).FirstTableItems;
		}
	}
}