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
using PX.Objects.PO;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class CommitmentDataProvider
	{
		public static POOrder GetCommitment(PXGraph graph, string orderNumber, string orderType)
		{
			return SelectFrom<POOrder>
				.Where<POOrder.orderNbr.IsEqual<P.AsString>.And<POOrder.orderType.IsEqual<P.AsString>>>.View
				.Select(graph, orderNumber, orderType);
		}

		public static IEnumerable<POLine> GetCommitmentLines(PXGraph graph, string orderNumber, string orderType,
			int? projectId)
		{
			return SelectFrom<POLine>
				.Where<POLine.orderNbr.IsEqual<P.AsString>
					.And<POLine.orderType.IsEqual<P.AsString>>
					.And<POLine.projectID.IsEqual<P.AsInt>>>.View
				.Select(graph, orderNumber, orderType, projectId).FirstTableItems;
		}
	}
}