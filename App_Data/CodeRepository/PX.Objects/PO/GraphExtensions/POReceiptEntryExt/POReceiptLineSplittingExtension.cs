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
using System.Collections;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.GraphExtensions;

using LotSerOptions = PX.Objects.IN.LSSelect.LotSerOptions;

namespace PX.Objects.PO.GraphExtensions.POReceiptEntryExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class POReceiptLineSplittingExtension : LineSplittingExtension<POReceiptEntry, POReceipt, POReceiptLine, POReceiptLineSplit>
	{
		#region Configuration
		protected override Type SplitsToDocumentCondition => typeof(POReceiptLineSplit.FK.Receipt.SameAsCurrent);

		protected override Type LineQtyField => typeof(POReceiptLine.receiptQty);

		public override POReceiptLineSplit LineToSplit(POReceiptLine item)
		{
			using (InvtMultModeScope(item))
			{
				POReceiptLineSplit ret = item;
				// baseQty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}
		#endregion
		#region Actions
		public override IEnumerable GenerateNumbers(PXAdapter adapter)
		{
			if (LineCurrent == null)
				return adapter.Get();

			if (!IsLSEntryEnabled(LineCurrent))
				return adapter.Get();

			return base.GenerateNumbers(adapter);
		}

		public override IEnumerable ShowSplits(PXAdapter adapter)
		{
			if (LineCurrent == null)
				return adapter.Get();

			if (!IsLSEntryEnabled(LineCurrent))
				throw new PXSetPropertyException(Messages.BinLotSerialEntryDisabled);

			return base.ShowSplits(adapter);
		}
		#endregion
		#region Event Handlers
		#region POReceiptLine
		protected override void SubscribeForLineEvents()
		{
			base.SubscribeForLineEvents();
			ManualEvent.FieldOf<POReceiptLine, POReceiptLine.receiptQty>.Updated.Subscribe<decimal?>(Base, EventHandler);
			ManualEvent.FieldOf<POReceiptLine, POReceiptLine.origOrderQty>.Selecting.Subscribe(Base, EventHandler);
			ManualEvent.FieldOf<POReceiptLine, POReceiptLine.openOrderQty>.Selecting.Subscribe(Base, EventHandler);
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<POReceiptLine, POReceiptLine.receiptQty>.Updated.Args<decimal?> e)
		{
			if (e.Row != null && e.Row.ReceiptQty != e.OldValue)
				e.Cache.RaiseFieldUpdated<POReceiptLine.baseReceiptQty>(e.Row, e.Row.BaseReceiptQty);
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<POReceiptLine, POReceiptLine.origOrderQty>.Selecting.Args e)
		{
			if (e.Row?.PONbr != null)
			{
				POLineR origLine = Base.GetReferencedPOLineR(e.Row.POType, e.Row.PONbr, e.Row.POLineNbr);
				if (origLine != null && e.Row.InventoryID == origLine.InventoryID)
				{
					if (string.Equals(e.Row.UOM, origLine.UOM) == false)
					{
						decimal baseOrderQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(e.Cache, e.Row, origLine.UOM, origLine.OrderQty.Value, INPrecision.QUANTITY);
						e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(e.Cache, e.Row, e.Row.UOM, baseOrderQty, INPrecision.QUANTITY);
					}
					else
					{
						e.ReturnValue = origLine.OrderQty;
					}
				}
			}

			if (e.Row?.OrigRefNbr != null)
			{
				INTran origLine =
					SelectFrom<INTran>.
					Where<
						INTran.tranType.IsEqual<INTranType.transfer>.
						And<INTran.refNbr.IsEqual<POReceiptLine.origRefNbr.FromCurrent>>.
						And<INTran.lineNbr.IsEqual<POReceiptLine.origLineNbr.FromCurrent>>.
						And<INTran.docType.IsEqual<POReceiptLine.origDocType.FromCurrent>>>.
					View.SelectSingleBound(Base, new object[] { e.Row });

				//is it needed at all? UOM conversion seems to be right thing to do. Also must it be origQty or origleftqty?
				if (origLine != null)
				{
					//if (string.Equals(row.UOM, origLine.UOM) == false)
					//{
					//    decimal baseOpenQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(e.Cache, e.Row, origLine.UOM, origLine.Qty.Value, INPrecision.QUANTITY);
					//    e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(e.Cache, e.Row, e.Row.UOM, baseOpenQty, INPrecision.QUANTITY);
					//}
					//else
					{
						e.ReturnValue = origLine.Qty;
					}
				}
			}

			var state = PXDecimalState.CreateInstance(
				e.ReturnState,
				precision: ((CommonSetup)Base.Caches<CommonSetup>().Current).DecPlQty,
				fieldName: nameof(POReceiptLine.OrigOrderQty),
				isKey: false,
				required: 0,
				minValue: decimal.MinValue,
				maxValue: decimal.MaxValue);
			state.DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OrigOrderQty);
			state.Enabled = false;
			e.ReturnState = state;
		}

		protected virtual void EventHandler(ManualEvent.FieldOf<POReceiptLine, POReceiptLine.openOrderQty>.Selecting.Args e)
		{
			if (e.Row?.PONbr != null)
			{
				POLineR origLine = Base.GetReferencedPOLineR(e.Row.POType, e.Row.PONbr, e.Row.POLineNbr);
				if (origLine != null && e.Row.InventoryID == origLine.InventoryID)
				{
					decimal? openQty;
					if (string.Equals(e.Row.UOM, origLine.UOM) == false)
					{
						decimal baseOpenQty = INUnitAttribute.ConvertToBase<POReceiptLine.inventoryID>(e.Cache, e.Row, origLine.UOM, origLine.OrderQty.Value - origLine.ReceivedQty.Value, INPrecision.QUANTITY);
						openQty = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(e.Cache, e.Row, e.Row.UOM, baseOpenQty, INPrecision.QUANTITY);
					}
					else
					{
						openQty = origLine.OrderQty - origLine.ReceivedQty;
					}
					e.ReturnValue = (openQty < 0m) ? 0m : openQty;
				}
			}

			if (e.Row?.OrigRefNbr != null)
			{
				INTransitLineStatus origLineStat =
					SelectFrom<INTransitLineStatus>.
					Where<
						INTransitLineStatus.transferNbr.IsEqual<POReceiptLine.origRefNbr.FromCurrent>.
						And<INTransitLineStatus.transferLineNbr.IsEqual<POReceiptLine.origLineNbr.FromCurrent>>>.
					View.SelectSingleBound(Base, new object[] { e.Row });

				if (origLineStat != null)
				{
					decimal baseOpenQty = origLineStat.QtyOnHand.Value - ((e.Row.Released ?? false) ? 0 : e.Row.BaseReceiptQty.GetValueOrDefault());
					e.ReturnValue = INUnitAttribute.ConvertFromBase<POReceiptLine.inventoryID>(e.Cache, e.Row, e.Row.UOM, baseOpenQty, INPrecision.QUANTITY);
				}
			}

			var state = PXDecimalState.CreateInstance(
				e.ReturnState,
				precision: ((CommonSetup)Base.Caches<CommonSetup>().Current).DecPlQty,
				fieldName: nameof(POReceiptLine.OpenOrderQty),
				isKey: false,
				required: 0,
				minValue: decimal.MinValue,
				maxValue: decimal.MaxValue);
			state.DisplayName = PXMessages.LocalizeNoPrefix(SO.Messages.OpenOrderQty);
			state.Enabled = false;
			e.ReturnState = state;
		}


		protected override void EventHandler(ManualEvent.Row<POReceiptLine>.Selected.Args e)
		{
			if (e.Row == null) return;

			bool lsEntryEnabled = IsLSEntryEnabled(e.Row) && e.Row.Released != true && !IsDropshipReturn();

			SplitCache.AllowInsert = lsEntryEnabled;
			SplitCache.AllowUpdate = lsEntryEnabled;
			SplitCache.AllowDelete = lsEntryEnabled;

			e.Cache.Adjust<POLotSerialNbrAttribute>(e.Row)
				.For<POReceiptLine.lotSerialNbr>(a => a.ForceDisable = !lsEntryEnabled);

			if(lsEntryEnabled && Math.Abs(e.Row.UnassignedQty ?? 0) >= 0.0000005m)
				RaiseUnassignedExceptionHandling(e.Row);
		}

		protected override void EventHandler(ManualEvent.Row<POReceiptLine>.Inserted.Args e)
		{
			if (IsLSEntryEnabled(e.Row))
			{
				base.EventHandler(e);
			}
			else
			{
				e.Cache.SetValue<POReceiptLine.locationID>(e.Row, null);
				e.Cache.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				e.Cache.SetValue<POReceiptLine.expireDate>(e.Row, null);
			}
		}

		protected override void EventHandler(ManualEvent.Row<POReceiptLine>.Updated.Args e)
		{
			if (IsLSEntryEnabled(e.Row) && (e.Row.LineType != POLineType.GoodsForProject || e.Row.ReceiptType != POReceiptType.POReturn))
			{
				using (ResolveNotDecimalUnitErrorRedirectorScope<POReceiptLineSplit.qty>(e.Row))
					base.EventHandler(e);
			}
			else
			{
				e.Cache.SetValue<POReceiptLine.locationID>(e.Row, null);
				e.Cache.SetValue<POReceiptLine.lotSerialNbr>(e.Row, null);
				e.Cache.SetValue<POReceiptLine.expireDate>(e.Row, null);

				if (e.Row != null && e.OldRow != null && e.Row.InventoryID != e.OldRow.InventoryID)
					base.RaiseRowDeleted(e.OldRow);
			}
		}

		protected override void EventHandler(ManualEvent.Row<POReceiptLine>.Deleted.Args e)
		{
			if (IsLSEntryEnabled(e.Row))
				base.EventHandler(e);
		}

		#endregion
		#region POReceiptLineSplit
		protected override void SubscribeForSplitEvents()
		{
			base.SubscribeForSplitEvents();
			ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.invtMult>.Defaulting.Subscribe<short?>(Base, EventHandler);
			ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.subItemID>.Defaulting.Subscribe<int?>(Base, EventHandler);
			ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.locationID>.Defaulting.Subscribe<int?>(Base, EventHandler);
			ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.lotSerialNbr>.Defaulting.Subscribe<string>(Base, EventHandler);
		}

		public virtual void EventHandler(ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.invtMult>.Defaulting.Args<short?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr))
			{
				using (InvtMultModeScope(LineCurrent))
				{
					e.NewValue = LineCurrent.InvtMult;
					e.Cancel = true;
				}
			}
		}

		public virtual void EventHandler(ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.subItemID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr))
			{
				e.NewValue = LineCurrent.SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void EventHandler(ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.locationID>.Defaulting.Args<int?> e)
		{
			if (LineCurrent != null && (e.Row == null || LineCurrent.LineNbr == e.Row.LineNbr))
			{
				e.NewValue = LineCurrent.LocationID;
				e.Cancel = true;
			}
		}

		public virtual void EventHandler(ManualEvent.FieldOf<POReceiptLineSplit, POReceiptLineSplit.lotSerialNbr>.Defaulting.Args<string> e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(e.Row.InventoryID);

			if (item != null)
			{
				if (e.Row.InvtMult == null)
					e.Cache.RaiseFieldDefaulting<POReceiptLineSplit.invtMult>(e.Row, out _);

				INLotSerTrack.Mode mode = GetTranTrackMode(e.Row, item);
				if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
				{
					ILotSerNumVal lotSerNum = ReadLotSerNumVal(item);
					foreach (POReceiptLineSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<POReceiptLineSplit>(e.Cache, item, lotSerNum, mode, 1m))
					{
						e.NewValue = lssplit.LotSerialNbr;
						e.Cancel = true;
					}
				}
				//otherwise default via attribute
			}
		}

		public override void EventHandlerQty(ManualEvent.FieldOf<POReceiptLineSplit>.Verifying.Args<decimal?> e)
		{
			if (IsTrackSerial(e.Row))
				base.EventHandlerQty(e);
			else
				e.NewValue = VerifySNQuantity(e.Cache, e.Row, e.NewValue, nameof(POReceiptLineSplit.qty));
		}

		public virtual void EventHandlerPOReceiptLineSplit(ManualEvent.Row<POReceiptLineSplit>.Persisting.Args e) // seems to be not used
		{
			if (e.Row != null && e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
				if (e.Row.BaseQty != 0m && e.Row.LocationID == null)
					ThrowFieldIsEmpty<POReceiptLineSplit.locationID>(e.Cache, e.Row);
		}
		#endregion
		#endregion

		public override POReceiptLine Clone(POReceiptLine item)
		{
			POReceiptLine copy = base.Clone(item);
			copy.POType = null;
			copy.PONbr = null;
			copy.POLineNbr = null;
			return copy;
		}

		public override bool IsTrackSerial(POReceiptLineSplit split)
		{
			INLotSerClass readLotSerialClass() => ReadInventoryItem(split.InventoryID)?.GetItem<INLotSerClass>();

			if (split.LineType == POLineType.GoodsForDropShip)
			{
				return readLotSerialClass()?.LotSerTrack == INLotSerTrack.SerialNumbered;
			}
			else if (Base.Document.Current?.ReceiptType == POReceiptType.POReturn
				&& readLotSerialClass()?.LotSerAssign == INLotSerAssign.WhenUsed)
			{
					return false;
			}
			else
			{
				return base.IsTrackSerial(split);
			}
		}

		protected override bool IsLotSerOptionsEnabled(LotSerOptions opt)
		{
			return base.IsLotSerOptionsEnabled(opt)
				&& Base.Document.Current?.Released != true
				&& !IsDropshipReturn();
		}

		public override void DefaultLotSerialNbr(POReceiptLineSplit row)
		{
			if (row.ReceiptType == POReceiptType.TransferReceipt)
				row.AssignedNbr = null;
			else
				base.DefaultLotSerialNbr(row);
		}

		protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
		{
			if (Base.Document.Current?.ReceiptType == POReceiptType.POReturn && lotSerClass?.LotSerAssign == INLotSerAssign.WhenUsed)
				return INLotSerTrack.Mode.None;
			else if (row is POReceiptLine line && line.LineType == POLineType.GoodsForDropShip
				&& lotSerClass != null && lotSerClass.LotSerTrack != null && lotSerClass.LotSerTrack != INLotSerTrack.NotNumbered)
			{
				return INLotSerTrack.Mode.Create;
			}

			return base.GetTranTrackMode(row, lotSerClass);
		}


		public virtual bool IsLSEntryEnabled(POReceiptLine line)
		{
			if (line != null && line.IsLSEntryBlocked == true)
				return false;

			if (line == null)
				return true;

			if (POLineType.IsStockNonDropShip(line.LineType))
				return true;

			if (line.LineType.IsIn(POLineType.GoodsForDropShip, POLineType.GoodsForProject))
			{
				(var _, var lsClass) = ReadInventoryItem(line.InventoryID);

				if (lsClass.RequiredForDropship == true)
					return true;
			}

			return false;
		}

		protected virtual bool IsDropshipReturn() => !string.IsNullOrEmpty(Base.Document.Current?.SOOrderNbr);


		protected override void AppendSerialStatusCmdWhere(PXSelectBase<INLotSerialStatusByCostCenter> cmd, POReceiptLine Row, INLotSerClass lotSerClass)
		{
			if (Row.SubItemID != null)
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.subItemID.IsEqual<INLotSerialStatusByCostCenter.subItemID.FromCurrent>>>();

			if (Row.LocationID != null)
				cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.locationID.IsEqual<INLotSerialStatusByCostCenter.locationID.FromCurrent>>>();
			else
				cmd.WhereAnd<Where<INLocation.receiptsValid.IsEqual<True>>>();

			if (lotSerClass.IsManualAssignRequired == true)
			{
				if (string.IsNullOrEmpty(Row.LotSerialNbr))
					cmd.WhereAnd<Where<True.IsEqual<False>>>();
				else
					cmd.WhereAnd<Where<INLotSerialStatusByCostCenter.lotSerialNbr.IsEqual<INLotSerialStatusByCostCenter.lotSerialNbr.FromCurrent>>>();
			}
		}

		protected override void RaiseUnassignedExceptionHandling(POReceiptLine line)
		{
			//redirect a warning from ReceiptQty to UnassignedQty
			LineCache.RaiseExceptionHandling<POReceiptLine.unassignedQty>(line, line.UnassignedQty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned, PXErrorLevel.Warning));
		}
	}
}
