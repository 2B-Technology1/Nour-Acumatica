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
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.Extensions;
using PX.Objects.IN;

namespace PX.Objects.PO.GraphExtensions.POOrderEntryExt
{
	using POOrderR = POOrderEntry.POOrderR;

	public class AffectedBlanketOrdersByOriginalOrders : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedBlanketOrdersByOriginalOrders, POOrderEntry, POOrderR, POOrderEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
		}

		#region Overrides

		protected override bool ClearAffectedCaches => false;
		protected override bool PersistInSameTransaction => true;

		protected override bool EntityIsAffected(POOrderR entity)
		{
			if (entity.OrderType != POOrderType.Blanket)
				return false;

			var cache = Base.poOrder.Cache;
			int? linesToCompleteCntrOldValue = (int?)cache.GetValueOriginal<POOrderR.linesToCompleteCntr>(entity);
			int? linesToCloseCntrOldValue = (int?)cache.GetValueOriginal<POOrderR.linesToCloseCntr>(entity);

			return
				(!Equals(linesToCompleteCntrOldValue, entity.LinesToCompleteCntr)
					|| !Equals(linesToCloseCntrOldValue, entity.LinesToCloseCntr)) 
				&& (linesToCompleteCntrOldValue == 0
					|| entity.LinesToCompleteCntr == 0
					|| linesToCloseCntrOldValue == 0
					|| entity.LinesToCloseCntr == 0);
		}

		protected override void ProcessAffectedEntity(POOrderEntry primaryGraph, POOrderR entity)
		{
			var order = POOrder.PK.Find(primaryGraph, entity.OrderType, entity.OrderNbr);
			primaryGraph.UpdateDocumentState(order);
		}

		protected override POOrderR ActualizeEntity(POOrderEntry primaryGraph, POOrderR entity)
			=> PXSelect<POOrderR,
				Where<POOrderR.orderType, Equal<Required<POOrderR.orderType>>,
					And<POOrderR.orderNbr, Equal<Required<POOrderR.orderNbr>>>>>
				.Select(primaryGraph, entity.OrderType, entity.OrderNbr);

		#endregion

		protected virtual bool IsBlanketChildRow(POLine row) => POOrderType.IsUseBlanket(row?.OrderType) && row?.POType == POOrderType.Blanket && row.PONbr != null && row.POLineNbr != null;

		protected virtual POLineR FindBlanketRow(PXCache rowCache, POLine normalRow)
			=> PXParentAttribute.SelectParent<POLineR>(rowCache, normalRow);

		#region POLine fields overriding

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUnboundFormula(
			typeof(Switch<
					Case<Where<POLine.cancelled.IsEqual<True>>, decimal0,
					Case<Where<POLine.completed.IsEqual<True>.And<POLine.lineType.IsNotEqual<POLineType.service>>>, POLine.baseReceivedQty>>,
					POLine.baseOrderQty>),
			typeof(SumCalc<POLineR.baseOrderedQty>))]
		protected virtual void _(Events.CacheAttached<POLine.completed> e) { }

		#endregion

		#region POLineR fields overriding

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<Case<Where<POLineR.completed, Equal<True>, Or<POLineR.cancelled, Equal<True>>>, decimal0>,
			Maximum<Sub<POLineR.orderQty, POLineR.orderedQty>, decimal0>>),
			typeof(SumCalc<POOrderR.openOrderQty>))]
		protected virtual void _(Events.CacheAttached<POLineR.openQty> e) { }

		#endregion

		#region POLineR events

		protected virtual void _(Events.RowDeleting<POLineR> e)
		{
			e.Cache.SetStatus(e.Row, PXEntryStatus.Notchanged); //This is important to prevent updating (by API) after this event .
			e.Cancel = true;
		}
		protected virtual void _(Events.RowInserting<POLineR> e)
		{
			e.Cancel = true;
		}

		#endregion

		#region POLine event handlers

		protected virtual void _(Events.FieldVerifying<POLine, POLine.orderQty> e)
		{
			if(e.Row.OrderType == POOrderType.Blanket
				&& (decimal?)e.NewValue < e.Row.OrderedQty)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(Messages.OrderLineQtyLessThanQuantityInChildOrders);
			}
		}

		protected virtual void _(Events.RowSelected<POLine> e)
		{
			if (IsBlanketChildRow(e.Row))
			{
				var blanketRow = FindBlanketRow(e.Cache, e.Row);
				if (blanketRow == null)
					throw new PXArgumentException(nameof(blanketRow));

				RaiseNormalOrderQtyExceedsWarning(e.Cache, e.Row, blanketRow);
			}
		}

		protected virtual void RaiseNormalOrderQtyExceedsWarning(PXCache normalRowCache, POLine normalRow, POLineR blanketRow)
		{
			var error = PXUIFieldAttribute.GetErrorOnly<POLine.orderQty>(normalRowCache, normalRow);
			if (!string.IsNullOrEmpty(error))
				return;

			if (normalRow.OrderQty > 0
				&& normalRow.Completed == false
				&& normalRow.Cancelled == false
				&& blanketRow.OrderedQty > blanketRow.OrderQty)
			{
				var ex = new PXSetPropertyException(Messages.OrderLineQtyExceedsQuantityInBlanketOrder, PXErrorLevel.Warning, normalRow.PONbr);
				normalRowCache.RaiseExceptionHandling<POLine.orderQty>(normalRow, normalRow.OrderQty, ex);
			}
			else
			{
				normalRowCache.RaiseExceptionHandling<POLine.orderQty>(normalRow, normalRow.OrderQty, null);
			}
		}

		protected virtual void _(Events.RowUpdated<POLine> e)
		{
			if (IsBlanketChildRow(e.Row))
			{
				if (!e.Cache.ObjectsEqual<POLine.completed, POLine.closed>(e.Row, e.OldRow))
				{
					var blanketRow = FindBlanketRow(e.Cache, e.Row);
					if (blanketRow == null)
						throw new PXArgumentException(nameof(blanketRow));

					var oldCompleted = blanketRow.Completed;
					var oldClosed = blanketRow.Closed;

					var blanketRowCache = Base.poLiner.Cache;
					blanketRow = CompleteBlanketRow(blanketRowCache, blanketRow, e.Row);
					blanketRow = CloseBlanketRow(blanketRowCache, blanketRow, e.Row);

					if (oldCompleted != blanketRow.Completed
						|| oldClosed != blanketRow.Closed)
					{
						blanketRowCache.Update(blanketRow);
						e.Cache.Current = e.Row;
					}
				}

				if (!e.Cache.ObjectsEqual<POLine.orderQty>(e.Row, e.OldRow))
				{
					var blanketRow = FindBlanketRow(e.Cache, e.Row);
					if (blanketRow == null)
						throw new PXArgumentException(nameof(blanketRow));

					RaiseNormalOrderQtyExceedsWarning(e.Cache, e.Row, blanketRow);
				}
			}
		}

		#endregion

		#region Complete/Close blanket rows

		/// the same logic for the <see cref="POLineUOpen"/> <see cref="AffectedPOOrdersByPOLineUOpen{TSelf, TGraph}.CompleteBlanketRow(PXCache, POLineUOpen, POLineUOpen, POLineUOpen)"/>
		protected virtual POLineR CompleteBlanketRow(PXCache blanketRowCache, POLineR blanketRow, POLine normalRow)
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
					if (POLineType.IsService(blanketRow.LineType))
						completed = blanketRow.BilledQty >= blanketRow.OrderQty * blanketRow.RcptQtyThreshold / 100;
					else
						completed = blanketRow.ReceivedQty >= blanketRow.OrderQty * blanketRow.RcptQtyThreshold / 100;
				}
				else
				{
					completed = blanketRow.BilledAmt >= (blanketRow.ExtCost + blanketRow.RetainageAmt) * blanketRow.RcptQtyThreshold / 100;
				}
				if (completed)
				{
					POLineR uncompletedChild = SelectFrom<POLineR>
						.Where<POLineR.FK.BlanketOrderLine.SameAsCurrent
							.And<POLineR.completed.IsEqual<False>>
							.And<POLineR.orderType.IsNotEqual<@P.AsString.ASCII>
								.Or<POLineR.orderNbr.IsNotEqual<@P.AsString>>
								.Or<POLineR.lineNbr.IsNotEqual<@P.AsInt>>>>
						.View
						.SelectSingleBound(Base, new[] { blanketRow }, normalRow.OrderType, normalRow.OrderNbr, normalRow.LineNbr);
					completed = uncompletedChild == null;
				}
			}

			blanketRow.Completed = completed;

			return blanketRow;
		}

		/// similar for the <see cref="POLineUOpen"/> <see cref="AffectedPOOrdersByPOLineUOpen{TSelf, TGraph}.CloseBlanketRow(PXCache, POLineUOpen, POLineUOpen, POLineUOpen)"/>
		protected virtual POLineR CloseBlanketRow(PXCache blanketRowCache, POLineR blanketRow, POLine normalRow)
		{
			bool closed;
			if (blanketRow.Completed == false)
			{
				closed = false;
			}
			else if (normalRow.Closed == false)
			{
				closed = false;
			}
			else
			{
				POLineR unclosedChild = SelectFrom<POLineR>
						.Where<POLineR.FK.BlanketOrderLine.SameAsCurrent
							.And<POLineR.closed.IsEqual<False>>
							.And<POLineR.orderType.IsNotEqual<@P.AsString.ASCII>
								.Or<POLineR.orderNbr.IsNotEqual<@P.AsString>>
								.Or<POLineR.lineNbr.IsNotEqual<@P.AsInt>>>>
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
