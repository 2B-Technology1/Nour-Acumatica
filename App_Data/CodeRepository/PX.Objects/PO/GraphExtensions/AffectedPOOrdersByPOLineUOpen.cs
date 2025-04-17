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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Extensions;
using PX.Objects.IN;
using System.Collections.Generic;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.PO.GraphExtensions
{
	public abstract class AffectedPOOrdersByPOLineUOpen<TSelf, TGraph> : ProcessAffectedEntitiesInPrimaryGraphBase<TSelf, TGraph, POOrder, POOrderEntry>
		where TGraph : PXGraph
		where TSelf : AffectedPOOrdersByPOLineUOpen<TSelf, TGraph>
	{
		#region Overrides

		protected override bool PersistInSameTransaction => true;

		protected override bool EntityIsAffected(POOrder entity)
		{
			var cache = Base.Caches<POOrder>();
			int? linesToCloseCntrOldValue = (int?)cache.GetValueOriginal<POOrder.linesToCloseCntr>(entity),
				linesToCompleteCntrOldValue = (int?)cache.GetValueOriginal<POOrder.linesToCompleteCntr>(entity);
			return
				(!Equals(linesToCloseCntrOldValue, entity.LinesToCloseCntr)
				|| !Equals(linesToCompleteCntrOldValue, entity.LinesToCompleteCntr))
				&& (linesToCloseCntrOldValue == 0 || linesToCompleteCntrOldValue == 0
				|| entity.LinesToCloseCntr == 0 || entity.LinesToCompleteCntr == 0);
		}

		protected override IEnumerable<POOrder> GetAffectedEntities()
			=> base.GetAffectedEntities().BeginWith(x => x.OrderType != POOrderType.Blanket);

		protected override void ProcessAffectedEntity(POOrderEntry primaryGraph, POOrder entity)
		{
			primaryGraph.UpdateDocumentState(entity);
		}

		protected override POOrder ActualizeEntity(POOrderEntry primaryGraph, POOrder entity)
			=> PXSelect<POOrder,
				Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
					And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
				.Select(primaryGraph, entity.OrderType, entity.OrderNbr);

		#endregion

		#region POLineUOpen fields overriding

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXParent(typeof(SelectFrom<POLineUOpen>
			.Where<POLineUOpen.orderType.IsEqual<POOrderType.blanket>
				.And<POLineUOpen.orderType.IsEqual<POLineUOpen.pOType.FromCurrent>>
				.And<POLineUOpen.orderNbr.IsEqual<POLineUOpen.pONbr.FromCurrent>>
				.And<POLineUOpen.lineNbr.IsEqual<POLineUOpen.pOLineNbr.FromCurrent>>>))]
		protected virtual void _(Events.CacheAttached<POLineUOpen.pOLineNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(
			typeof(Switch<
					Case<Where<POLineUOpen.cancelled.IsEqual<True>>, decimal0,
					Case<Where<POLineUOpen.completed.IsEqual<True>.And<POLineUOpen.lineType.IsNotEqual<POLineType.service>>>, POLineUOpen.baseReceivedQty>>,
					POLineUOpen.baseOrderQty>),
			typeof(SumCalc<POLineUOpen.baseOrderedQty>))]
		protected virtual void _(Events.CacheAttached<POLineUOpen.completed> e) { }

		#endregion

		#region Update blanket order line

		protected virtual void _(Events.RowUpdated<POLineUOpen> e)
		{
			if (e.Row?.POType != POOrderType.Blanket)
				return;

			var blanketRow = UpdateBlanketRow(e.Cache, e.Row, e.OldRow, false);
			if (blanketRow != null)
				e.Cache.Current = e.Row;
		}

		protected virtual POLineUOpen UpdateBlanketRow(PXCache cache, POLineUOpen normalRow, POLineUOpen normalOldRow, bool hardCheck)
		{
			if (!hardCheck && cache.ObjectsEqual<POLineUOpen.completed, POLineUOpen.closed>(normalRow, normalOldRow))
				return null;

			var blanketRow = FindBlanketRow(cache, normalRow);
			if (blanketRow == null)
				throw new PXArgumentException(nameof(blanketRow));

			bool? oldCompleted = blanketRow.Completed;
			bool? oldClosed = blanketRow.Closed;

			if (hardCheck
				|| normalRow.Completed != normalOldRow.Completed)
			{
				blanketRow = CompleteBlanketRow(cache, blanketRow, normalRow, normalOldRow);
			}

			if (hardCheck
				|| oldCompleted != blanketRow.Completed
				|| normalRow.Closed != normalOldRow.Closed)
			{
				blanketRow = CloseBlanketRow(cache, blanketRow, normalRow, normalOldRow);
			}

			if (oldCompleted != blanketRow.Completed || oldClosed != blanketRow.Closed)
			{
				return (POLineUOpen)cache.Update(blanketRow);
			}

			return null;
		}

		protected POLineUOpen FindBlanketRow(PXCache rowCache, POLineUOpen normalRow)
			=> PXParentAttribute.SelectParent<POLineUOpen>(rowCache, normalRow);

		protected virtual POLineUOpen CompleteBlanketRow(PXCache cache, POLineUOpen blanketRow, POLineUOpen normalRow, POLineUOpen normalOldRow)
		{
			bool completed;
			if (normalRow.Completed == false)
			{
				completed = false;
			}
			else
			{
				if (blanketRow.CompletePOLine == CompletePOLineTypes.Quantity)
				{
					if(POLineType.IsService(blanketRow.LineType))
						completed = blanketRow.BaseBilledQty >= blanketRow.BaseOrderQty * blanketRow.RcptQtyThreshold / 100;
					else
						completed = blanketRow.BaseReceivedQty >= blanketRow.BaseOrderQty * blanketRow.RcptQtyThreshold / 100;
				}
				else
				{
					completed = blanketRow.BilledAmt >= (blanketRow.ExtCost + blanketRow.RetainageAmt) * blanketRow.RcptQtyThreshold / 100;
				}
				if (completed)
				{
					POLineUOpen uncompletedChild = SelectFrom<POLineUOpen>
						.Where<POLineUOpen.FK.BlanketOrderLine.SameAsCurrent
							.And<POLineUOpen.completed.IsEqual<False>>
							.And<POLineUOpen.orderType.IsNotEqual<@P.AsString.ASCII>
								.Or<POLineUOpen.orderNbr.IsNotEqual<@P.AsString>>
								.Or<POLineUOpen.lineNbr.IsNotEqual<@P.AsInt>>>
							>
						.View
						.SelectSingleBound(Base, new[] { blanketRow }, normalRow.OrderType, normalRow.OrderNbr, normalRow.LineNbr);
					completed = uncompletedChild == null;
				}
			}

			blanketRow.Completed = completed;

			return blanketRow;
		}

		protected virtual POLineUOpen CloseBlanketRow(PXCache cache, POLineUOpen blanketRow, POLineUOpen normalRow, POLineUOpen normalOldRow)
		{
			bool closed;
			if (blanketRow.Completed == false)
			{
				closed = false;
			}
			else if(normalRow.Closed == false)
			{
				closed = false;
			}
			else
			{
				POLineUOpen unclosedChild = SelectFrom<POLineUOpen>
					.Where<POLineUOpen.FK.BlanketOrderLine.SameAsCurrent
						.And<POLineUOpen.closed.IsEqual<False>>
						.And<POLineUOpen.orderType.IsNotEqual<@P.AsString.ASCII>
							.Or<POLineUOpen.orderNbr.IsNotEqual<@P.AsString>>
							.Or<POLineUOpen.lineNbr.IsNotEqual<@P.AsInt>>>
						>
					.View
					.SelectSingleBound(Base, new[] { blanketRow }, normalRow.OrderType, normalRow.OrderNbr, normalRow.LineNbr);
				closed = unclosedChild == null;
			}

			blanketRow.Closed = closed;

			return blanketRow;
		}

		#endregion
	}
}
