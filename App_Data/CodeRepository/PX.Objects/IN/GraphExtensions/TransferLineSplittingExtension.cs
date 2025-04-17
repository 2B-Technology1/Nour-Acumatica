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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class TransferLineSplittingExtension<TGraph, TSplittingExt, TPrimary, TLine, TSplit> : PXGraphExtension<TSplittingExt, TGraph>
		where TGraph : PXGraph
		where TSplittingExt : LineSplittingExtension<TGraph, TPrimary, TLine, TSplit>
		where TPrimary : class, IBqlTable, new()
		where TLine : class, IBqlTable, ILSPrimary, ILSTransferPrimary, new()
		where TSplit : class, IBqlTable, ILSDetail, new()
	{
		protected TSplittingExt LineSplittingExt => Base1;
		protected PXCache<TSplit> SplitCache => LineSplittingExt.SplitCache;
		protected PXCache<TLine> LineCache => LineSplittingExt.LineCache;

		#region Event Handlers
		#region TLine

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.SubscribeForLineEvents"/>
		/// </summary>
		[PXOverride]
		public virtual void SubscribeForLineEvents(Action baseMethod)
		{
			baseMethod();

			if (LineLotSerialNbrField != null)
				Base.FieldVerifying.AddHandler(typeof(TLine), LineLotSerialNbrField.Name, LineEventHandlerLotSerialNbr);
		}

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.EventHandlerQty(ManualEvent.FieldOf{TLine}.Verifying.Args{decimal?})"/>
		/// </summary>
		[PXOverride]
		public virtual void EventHandlerQty(ManualEvent.FieldOf<TLine>.Verifying.Args<decimal?> e,
			Action<ManualEvent.FieldOf<TLine>.Verifying.Args<decimal?>> baseMethod)
		{
			baseMethod(e);

			if (!Base1.IsTransferReceipt(e.Row) || !e.NewValue.HasValue || Base1.SuppressedMode)
				return;

			if (!string.IsNullOrEmpty(e.Row.LotSerialNbr))
			{
				VerifyQtyInTransit(e.Row, e.NewValue.Value, e.Row.LotSerialNbr);
			}
			else
			{
				VerifyMaxLineQty(e.Row, e.NewValue.Value, persisting: false);
			}
		}

		protected virtual void LineEventHandlerLotSerialNbr(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			TLine row = (TLine)e.Row;
			string newValue = (string)e.NewValue;

			if (Base1.SuppressedMode || !Base1.IsTransferReceipt(row) || string.IsNullOrEmpty(newValue))
				return;

			VerifyQtyInTransit(row, row.Qty ?? 0m, lotSerialNbr: newValue);
		}

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.EventHandler(ManualEvent.Row{TLine}.Persisting.Args)"/>
		/// </summary>
		[PXOverride]
		public virtual void EventHandler(ManualEvent.Row<TLine>.Persisting.Args e,
			Action<ManualEvent.Row<TLine>.Persisting.Args> baseMethod)
		{
			if (Base1.IsTransferReceipt(e.Row) && e.Operation.Command() != PXDBOperation.Delete)
			{
				VerifyMaxLineQty(e.Row, e.Row.Qty ?? 0m, persisting: true);
				VerifyUnassignedQty(e.Row);
			}

			baseMethod(e);
		}

		#endregion
		#region TSplit

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.SubscribeForSplitEvents"/>
		/// </summary>
		[PXOverride]
		public virtual void SubscribeForSplitEvents(Action baseMethod)
		{
			baseMethod();

			if (SplitLotSerialNbrField != null)
				Base.FieldVerifying.AddHandler(typeof(TSplit), SplitLotSerialNbrField.Name, SplitEventHandlerLotSerialNbr);
		}

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.EventHandlerQty(ManualEvent.FieldOf{TSplit}.Verifying.Args{decimal?})"/>
		/// </summary>
		[PXOverride]
		public virtual void EventHandlerQty(ManualEvent.FieldOf<TSplit>.Verifying.Args<decimal?> e,
			Action<ManualEvent.FieldOf<TSplit>.Verifying.Args<decimal?>> baseMethod)
		{
			baseMethod(e);

			if (Base1.SuppressedMode || !Base1.IsTransferReceipt(e.Row) || string.IsNullOrEmpty(e.Row.LotSerialNbr) || !e.NewValue.HasValue)
				return;

			VerifyQtyInTransit(e.Row, e.NewValue.Value, e.Row.LotSerialNbr);
		}

		protected virtual void SplitEventHandlerLotSerialNbr(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			TSplit row = (TSplit)e.Row;
			string newValue = (string)e.NewValue;

			if (Base1.SuppressedMode || !Base1.IsTransferReceipt(row) || string.IsNullOrEmpty(newValue))
				return;

			VerifyQtyInTransit(row, row.Qty ?? 0m, lotSerialNbr: newValue);
		}

		#endregion
		#endregion

		#region Create/Truncate/Update/Issue Numbers

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.IssueLotSerials(TLine, decimal, PXCache, PXCache, PXResult{InventoryItem, INLotSerClass})"/>
		/// </summary>
		[PXOverride]
		public virtual (decimal, TSplit) IssueLotSerials(TLine line, decimal deltaBaseQty, PXCache statusCache, PXCache statusAccumCache, PXResult<InventoryItem, INLotSerClass> item,
			Func<TLine, decimal, PXCache, PXCache, PXResult<InventoryItem, INLotSerClass>, (decimal, TSplit)> baseMethod)
		{
			if (Base1.IsTransferReceipt(line))
				return (deltaBaseQty, LineSplittingExt.LineToSplit(line));

			return baseMethod(line, deltaBaseQty, statusCache, statusAccumCache, item);
		}

		#endregion

		#region Utility Helpers

		/// <summary>
		/// Overrides <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.IsPrimaryFieldsUpdated(TLine, TLine)"/>
		/// </summary>
		[PXOverride]
		public virtual bool IsPrimaryFieldsUpdated(TLine line, TLine oldLine, Func<TLine, TLine, bool> baseMethod)
		{
			if (!Base1.IsTransferReceipt(line))
				return baseMethod(line, oldLine);

			return line.SubItemID != oldLine.SubItemID ||
				line.SiteID != oldLine.SiteID;
		}

		protected virtual void VerifyUnassignedQty(TLine line)
		{
			if (line.UnassignedQty > 0m)
				throw new PXRowPersistingException(LineQtyField.Name, line.Qty, Messages.BinLotSerialNotAssigned);
		}

		protected virtual void VerifyMaxLineQty(TLine line, decimal qty, bool persisting)
		{
			decimal max = INUnitAttribute.ConvertFromBase(LineCache, line.InventoryID, line.UOM, line.MaxTransferBaseQty ?? 0m, INPrecision.QUANTITY);
			if (qty > max && line.MaxTransferBaseQty.HasValue)
			{
				if (persisting)
					throw new PXRowPersistingException(LineQtyField.Name, qty, CS.Messages.Entry_LE, max);

				LineCache.RaiseExceptionHandling(LineQtyField.Name, line, qty, new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, max));
			}
		}

		protected virtual void VerifyQtyInTransit(TLine line, decimal qty, string lotSerialNbr)
		{
			INTransitLineLotSerialStatus transitStatus = GetTransitLotSerialStatus(line, line.SubItemID, lotSerialNbr);

			decimal baseQty = INUnitAttribute.ConvertToBase(LineCache, line.InventoryID, line.UOM, qty, INPrecision.QUANTITY);
			if (transitStatus == null || transitStatus.QtyAvail < baseQty)
			{
				var item = InventoryItem.PK.Find(Base, line.InventoryID);
				throw new PXSetPropertyException(Messages.StatusCheck_QtyTransitLotSerialNegative, item.InventoryCD, lotSerialNbr);
			}
		}

		protected virtual void VerifyQtyInTransit(TSplit split, decimal qty, string lotSerialNbr)
		{
			TLine line = (TLine)PXParentAttribute.SelectParent(SplitCache, split, typeof(TLine));
			INTransitLineLotSerialStatus transitStatus = GetTransitLotSerialStatus(line, split.SubItemID, lotSerialNbr);

			decimal otherSplitsQty = PXParentAttribute.SelectSiblings(SplitCache, split, typeof(TLine))
				.Cast<TSplit>()
				.Where(s => s.SplitLineNbr != split.SplitLineNbr && string.Equals(s.LotSerialNbr, lotSerialNbr, StringComparison.OrdinalIgnoreCase))
				.Sum(s => s.Qty ?? 0m);

			if (transitStatus == null || transitStatus.QtyAvail < qty + otherSplitsQty)
			{
				var item = InventoryItem.PK.Find(Base, split.InventoryID);
				throw new PXSetPropertyException(Messages.StatusCheck_QtyTransitLotSerialNegative, item.InventoryCD, lotSerialNbr);
			}
		}

		protected virtual INTransitLineLotSerialStatus GetTransitLotSerialStatus(TLine line, int? subItemID, string lotSerialNbr)
		{
			if (line == null)
				return null;

			return new SelectFrom<INTransitLineLotSerialStatus>
				.Where<INTransitLineLotSerialStatus.inventoryID.IsEqual<@P.AsInt>
					.And<INTransitLineLotSerialStatus.subItemID.IsEqual<@P.AsInt>>
					.And<INTransitLineLotSerialStatus.transferNbr.IsEqual<@P.AsString>>
					.And<INTransitLineLotSerialStatus.transferLineNbr.IsEqual<@P.AsInt>>
					.And<INTransitLineLotSerialStatus.lotSerialNbr.IsEqual<@P.AsString>>>
				.View(Base)
				.SelectWindowed(0, 1, line.InventoryID, subItemID, line.OrigRefNbr, line.OrigLineNbr, lotSerialNbr);
		}

		#endregion

		#region Protected methods of LineSplittingExtension
		/// Uses <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.SplitLotSerialNbrField"/>
		[PXProtectedAccess]
		protected abstract Type SplitLotSerialNbrField { get; }

		/// Uses <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.LineLotSerialNbrField"/>
		[PXProtectedAccess]
		protected abstract Type LineLotSerialNbrField { get; }

		/// Uses <see cref="LineSplittingExtension{TGraph, TPrimary, TLine, TSplit}.LineQtyField"/>
		[PXProtectedAccess]
		protected abstract Type LineQtyField { get; }
		#endregion
	}
}
