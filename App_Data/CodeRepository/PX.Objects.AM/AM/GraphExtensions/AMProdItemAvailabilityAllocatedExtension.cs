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
using PX.Objects.IN;
using PX.Objects.SO.GraphExtensions;

namespace PX.Objects.AM
{
	public abstract class AMProdItemAvailabilityAllocatedExtension<TGraph, TProdItemAvailExt> : ItemAvailabilityAllocatedExtension<TGraph, TProdItemAvailExt, AMProdItem, AMProdItemSplit>
		where TGraph : PXGraph
		where TProdItemAvailExt : AMProdItemAvailabilityExtension<TGraph>
	{
		protected override string GetStatusWithAllocated(AMProdItem line) => string.Empty;

		protected override Type LineQtyAvail => typeof(AMProdItem.lineQtyAvail);
		protected override Type LineQtyHardAvail => typeof(AMProdItem.lineQtyHardAvail);

		protected override AMProdItemSplit[] GetSplits(AMProdItem line) => Base.FindImplementation<AMProdItemLineSplittingExtension<TGraph>>().GetSplits(line);

		protected override AMProdItemSplit EnsurePlanType(AMProdItemSplit split)
		{
			if (split.PlanID != null && GetItemPlan(split.PlanID) is INItemPlan plan)
			{
				split = PXCache<AMProdItemSplit>.CreateCopy(split);
				//split.PlanType = plan.PlanType;
			}
			return split;
		}

		protected override Guid? DocumentNoteID => ((AMProdItem)Base.Caches<AMProdItem>().Current)?.NoteID;
	}
}
