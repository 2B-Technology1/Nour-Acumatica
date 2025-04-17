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

namespace PX.Objects.CR.Extensions
{
	public abstract class ActivityDetailsExt_Inversed_Actions<TActivityDetailsExt, TGraph, TPrimaryEntity>
		: ActivityDetailsExt_Actions<
			TActivityDetailsExt,
			TGraph,
			TPrimaryEntity,
			PMCRActivity,
			PMCRActivity.noteID>
		where TActivityDetailsExt : ActivityDetailsExt_Inversed<TGraph, TPrimaryEntity>, IActivityDetailsExt
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, INotable, new()
	{
	}

	public abstract class ActivityDetailsExt_Inversed<TGraph, TPrimaryEntity>
		: ActivityDetailsExt<
			TGraph,
			TPrimaryEntity,
			PMCRActivity,
			PMCRActivity.noteID>
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, INotable, new()
	{
		public override Type GetOrderByClause() => typeof(OrderBy<Desc<PMCRActivity.timeActivityCreatedDateTime>>);

		public override Type GetClassConditionClause() => typeof(Where<PMCRActivity.classID, IsNull, Or<PMCRActivity.classID, GreaterEqual<Zero>>>);

		public override Type GetPrivateConditionClause() => PXSiteMap.IsPortal
			? typeof(Where<PMCRActivity.isPrivate.IsNull.Or<PMCRActivity.isPrivate.IsEqual<False>>>)
			: null;
	}
}
