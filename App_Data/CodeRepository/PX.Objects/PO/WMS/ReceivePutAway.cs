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
using System.Collections;
using System.Collections.Generic;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.Extensions;
using PX.Objects.AP;
using PX.BarcodeProcessing;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.PO.WMS
{
	using WMSBase = WarehouseManagementSystem<ReceivePutAway, ReceivePutAway.Host>;

	public partial class ReceivePutAway : WMSBase
	{
		public class Host : POReceiptEntry
		{
			public ReceivePutAway WMS => FindImplementation<ReceivePutAway>();
		}

		public new class QtySupport : WMSBase.QtySupport
		{
			// TODO: do we really need this method?
			protected override void OnPreviousHeaderRestored(ScanHeader headerBackup)
			{
				string transferRefNbr = headerBackup.Get<RPAScanHeader>().TransferRefNbr;
				if (!string.IsNullOrEmpty(transferRefNbr) && string.IsNullOrEmpty(Basis.TransferRefNbr))
					Basis.TransferRefNbr = transferRefNbr;
			}
		}
		public new class GS1Support : WMSBase.GS1Support { }

		public class UserSetup : PXUserSetup<UserSetup, Host, ScanHeader, POReceivePutAwayUserSetup, POReceivePutAwayUserSetup.userID> { }

		#region State
		public RPAScanHeader RPAHeader => Header.Get<RPAScanHeader>() ?? new RPAScanHeader();
		public ValueSetter<ScanHeader>.Ext<RPAScanHeader> RPASetter => HeaderSetter.With<RPAScanHeader>();

		#region DefaultLocationID
		public int? DefaultLocationID
		{
			get => RPAHeader.DefaultLocationID;
			set => RPASetter.Set(h => h.DefaultLocationID, value);
		}
		#endregion
		#region PutAwayToLocationID
		public int? PutAwayToLocationID
		{
			get => RPAHeader.PutAwayToLocationID;
			set => RPASetter.Set(h => h.PutAwayToLocationID, value);
		}
		#endregion
		#region Released
		public bool? Released => RPAHeader.Released;
		#endregion
		#region ForceInsertLine
		public bool? ForceInsertLine
		{
			get => RPAHeader.ForceInsertLine;
			set => RPASetter.Set(h => h.ForceInsertLine, value);
		}
		#endregion
		#region ExcessQty
		public decimal? BaseExcessQty
		{
			get => RPAHeader.BaseExcessQty;
			set => RPASetter.Set(h => h.BaseExcessQty, value);
		}
		#endregion
		#region PrevInventoryID
		public int? PrevInventoryID
		{
			get => RPAHeader.PrevInventoryID;
			set => RPASetter.Set(h => h.PrevInventoryID, value);
		}
		#endregion
		#region ReceiptType
		public string ReceiptType
		{
			get => RPAHeader.ReceiptType;
			set => RPASetter.Set(h => h.ReceiptType, value);
		}
		#endregion
		#region TransferRefNbr
		public String TransferRefNbr
		{
			get => RPAHeader.TransferRefNbr;
			set => RPASetter.Set(h => h.TransferRefNbr, value);
		}
		#endregion
		#region PONbr
		public String PONbr
		{
			get => RPAHeader.PONbr;
			set => RPASetter.Set(h => h.PONbr, value);
		}
		#endregion
		#region BaseQty
		public new decimal BaseQty => INUnitAttribute.ConvertToBase(Graph.transactions.Cache, InventoryID, UOM, Qty ?? 0, INPrecision.NOROUND);
		#endregion
		#endregion

		#region Configuration
		public override bool ExplicitConfirmation => Setup.Current.ExplicitLineConfirmation == true;

		protected override string DocumentIsNotEditableMessage => Msg.ReceiptIsNotEditable;
		public override bool DocumentIsEditable
		{
			get
			{
				if (Header.Mode == ReceiveMode.Value)
					return base.DocumentIsEditable && Receipt?.Released == false;
				if (Header.Mode == ReturnMode.Value)
					return base.DocumentIsEditable && Receipt?.Released == false;
				if (Header.Mode == PutAwayMode.Value)
					return base.DocumentIsEditable && Receipt?.Released == true;
				throw new ArgumentOutOfRangeException(nameof(Header.Mode));
			}
		}

		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQty != true;
		protected override bool CanOverrideQty => DocumentIsEditable && (SelectedLotSerialClass?.LotSerTrack == INLotSerTrack.SerialNumbered).Implies(SelectedLotSerialClass?.LotSerAssign == INLotSerAssign.WhenUsed);

		public bool ReceiveBySingleItem => SelectedLotSerialClass.LotSerTrack == INLotSerTrack.SerialNumbered && SelectedLotSerialClass.LotSerAssign == INLotSerAssign.WhenReceived;
		#endregion

		#region Views
		public
			PXSetupOptional<POReceivePutAwaySetup,
			Where<POReceivePutAwaySetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>>>
			Setup;
		#endregion

		#region Buttons
		public PXAction<ScanHeader> ViewOrder;
		[PXButton, PXUIField(DisplayName = "View Order")]
		protected virtual IEnumerable viewOrder(PXAdapter adapter)
		{
			POReceiptLineSplit currentSplit = (POReceiptLineSplit)Graph.Caches<POReceiptLineSplit>().Current;
			if (currentSplit == null)
				return adapter.Get();

			POReceiptLine currentLine =
				SelectFrom<POReceiptLine>.
				Where<
					POReceiptLine.receiptType.IsEqual<POReceiptLineSplit.receiptType.FromCurrent>.
					And<POReceiptLine.receiptNbr.IsEqual<POReceiptLineSplit.receiptNbr.FromCurrent>>.
					And<POReceiptLine.lineNbr.IsEqual<POReceiptLineSplit.lineNbr.FromCurrent>>>.
				View.SelectSingleBound(Graph, new[] { currentSplit });
			if (currentLine == null)
				return adapter.Get();

			var orderEntry = PXGraph.CreateInstance<POOrderEntry>();
			orderEntry.Document.Current = orderEntry.Document.Search<POOrder.orderType, POOrder.orderNbr>(currentLine.POType, currentLine.PONbr);
			throw new PXRedirectRequiredException(orderEntry, true, nameof(ViewOrder)) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}
		#endregion

		#region Event Handlers
		protected override void _(Events.RowSelected<ScanHeader> e)
		{
			base._(e);

			if (e.Row == null)
				return;

			var splitsCache = Graph.Caches<POReceiptLineSplit>();

			bool isNotReleased = POReceipt.PK.Find(Base, ReceiptType, RefNbr)?.Released == false;
			if (isNotReleased == false)
			{
				splitsCache.AdjustUI().ForAllFields(a => a.Enabled = false);

				splitsCache.AllowInsert =
				splitsCache.AllowUpdate =
				splitsCache.AllowDelete = false;
			}

			splitsCache.AdjustUI().For<POReceiptLineSplit.qty>(a => a.Visible = e.Row.Mode == PutAwayMode.Value || Receipt?.WMSSingleOrder != true);

			if (string.IsNullOrEmpty(ReceiptType) || String.IsNullOrEmpty(RefNbr))
			{
				if (String.IsNullOrEmpty(PONbr))
					Base.Document.Current = null;
			}
			else
			{
				Base.Document.Current = Released == true
					? POReceipt.PK.Find(Base, ReceiptType, RefNbr)
					: Base.Document.Search<POReceipt.receiptNbr>(RefNbr, ReceiptType);
			}
		}

		protected virtual void _(Events.FieldDefaulting<INRegister.docType> e) => e.NewValue = INDocType.Transfer; // for proper redirect to Transfer

		protected virtual void _(Events.ExceptionHandling<POReceiptLine, POReceiptLine.receiptQty> e)
		{
			foreach (var split in GetSplits(Receipt).Select(s => s.GetItem<POReceiptLineSplit>()).Where(r => r.LineNbr == e.Row.LineNbr))
			{
				if (redirectQtyErrorToReceivedQty)
					Graph.splits.Cache.RaiseExceptionHandling<POReceiptLineSplit.receivedQty>(split, split.ReceivedQty, e.Exception);
				else
					Graph.splits.Cache.RaiseExceptionHandling<POReceiptLineSplit.qty>(split, e.NewValue, e.Exception);
			}
		}
		private bool redirectQtyErrorToReceivedQty = false;

		protected virtual void _(Events.RowUpdated<POReceivePutAwayUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowInserted<POReceivePutAwayUserSetup> e) => e.Row.IsOverridden = !e.Row.SameAs(Setup.Current);
		protected virtual void _(Events.RowPersisting<POReceivePutAwayUserSetup> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
				if (e.Row.DefaultLotSerialNumber == true && e.Row.DefaultExpireDate == false)
				{
					// save is performed inside of a dialog box
					// that is going to be closed regardless the save success,
					// so we need to manually rollback the made changes
					e.Cache.Clear();
					e.Cache.ClearQueryCache();
					e.Cache.Current = null;

					throw new PXException(Msg.SetupLotSerialFlagsInconsistency,
						PXUIFieldAttribute.GetDisplayName<POReceivePutAwayUserSetup.defaultExpireDate>(e.Cache),
						PXUIFieldAttribute.GetDisplayName<POReceivePutAwayUserSetup.defaultLotSerialNumber>(e.Cache));
				}
		}
		protected virtual void _(Events.RowSelected<POReceivePutAwayUserSetup> e)
		{
			if (e.Row != null && e.Row.DefaultLotSerialNumber == true && e.Row.DefaultExpireDate == false)
			{
				e.Cache.RaiseExceptionHandling<POReceivePutAwayUserSetup.defaultExpireDate>(
					e.Row, e.Row.DefaultExpireDate,
					new PXException(Msg.SetupLotSerialFlagsInconsistency,
						PXUIFieldAttribute.GetDisplayName<POReceivePutAwayUserSetup.defaultExpireDate>(e.Cache),
						PXUIFieldAttribute.GetDisplayName<POReceivePutAwayUserSetup.defaultLotSerialNumber>(e.Cache)));
			}
			else
				e.Cache.RaiseExceptionHandling<POReceivePutAwayUserSetup.defaultExpireDate>(e.Row, e.Row?.DefaultExpireDate, null);
		}
		#endregion

		#region DAC overrides
		[PXCustomizeBaseAttribute(typeof(StockItemAttribute), nameof(StockItemAttribute.Visible), true)]
		[PXCustomizeBaseAttribute(typeof(StockItemAttribute), nameof(StockItemAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<POReceiptLineSplit.inventoryID> e) { }

		[PXCustomizeBaseAttribute(typeof(SiteAttribute), nameof(SiteAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<POReceiptLineSplit.siteID> e) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void _(Events.CacheAttached<POReceiptLineSplit.qty> e) { }


		[Common.Attributes.BorrowedNote(typeof(POReceipt), typeof(POReceiptEntry))]
		protected virtual void _(Events.CacheAttached<ScanHeader.noteID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Receipt Nbr.", Enabled = false)]
		[PXSelector(typeof(
			SelectFrom<POReceipt>.
			LeftJoin<Vendor>.On<POReceipt.vendorID.IsEqual<Vendor.bAccountID>>.SingleTableOnly.
			Where<
				POReceipt.receiptType.IsEqual<RPAScanHeader.receiptType.FromCurrent>.
				And<Brackets<Vendor.bAccountID.IsNull.
				Or<Match<Vendor, AccessInfo.userName.FromCurrent>>>>>.
			SearchFor<POReceipt.receiptNbr>))]
		protected virtual void _(Events.CacheAttached<WMSScanHeader.refNbr> e) { }
		#endregion

		#region Overrides
		/// Overrides <see cref="POReceiptEntry.Persist"/>
		[PXOverride]
		public virtual void Persist(Action base_Persist)
		{
			base_Persist();
			ReceiptType = Receipt?.ReceiptType;
			RefNbr = Receipt?.ReceiptNbr;
		}

		/// Overrides <see cref="POReceiptEntry.Copy(POReceiptLine, POLine, decimal, decimal)"/>
		[PXOverride]
		public virtual void Copy(POReceiptLine aDest, POLine aSrc, decimal aQtyAdj, decimal aBaseQtyAdj, Action<POReceiptLine, POLine, decimal, decimal> base_Copy)
		{
			base_Copy(aDest, aSrc, aQtyAdj, aBaseQtyAdj);

			if (SiteID != null && aSrc.SiteID != SiteID)
			{
				aDest.SiteID = SiteID;
				aDest.LocationID = null;
				aDest.ProjectID = null;
				aDest.TaskID = null;
			}
		}
		#endregion

		public POReceipt Receipt => POReceipt.PK.Find(Base, Base.CurrentDocument.Current);

		#region State Machine
		protected override ScanMode<ReceivePutAway> GetDefaultMode()
		{
			PX.SM.UserPreferences userPreferences =
				SelectFrom<PX.SM.UserPreferences>.
				Where<PX.SM.UserPreferences.userID.IsEqual<AccessInfo.userID.FromCurrent>>.
				View.Select(Base);
			var preferencesExt = userPreferences?.GetExtension<DefaultReceivePutAwayModeByUser>();

			var receiveMode = ScanModes.OfType<ReceiveMode>().FirstOrDefault();
			var returnMode = ScanModes.OfType<ReturnMode>().FirstOrDefault();
			var putAwayMode = ScanModes.OfType<PutAwayMode>().FirstOrDefault();

			return
				preferencesExt?.RPAMode == DefaultReceivePutAwayModeByUser.rPAMode.Receive && Setup.Current.ShowReceivingTab == true ? receiveMode :
				preferencesExt?.RPAMode == DefaultReceivePutAwayModeByUser.rPAMode.Return && Setup.Current.ShowReturningTab == true ? returnMode :
				preferencesExt?.RPAMode == DefaultReceivePutAwayModeByUser.rPAMode.PutAway && Setup.Current.ShowPutAwayTab == true ? putAwayMode :
				Setup.Current.ShowReceivingTab == true ? receiveMode :
				Setup.Current.ShowReturningTab == true ? returnMode :
				Setup.Current.ShowPutAwayTab == true ? putAwayMode :
				base.GetDefaultMode();
		}

		protected override IEnumerable<ScanMode<ReceivePutAway>> CreateScanModes()
		{
			yield return new ReceiveMode();
			yield return new ReturnMode();
			yield return new PutAwayMode();
		}
		#endregion

		#region States
		public abstract class ReceiptState : RefNbrState<POReceipt>
		{
			protected override string StatePrompt => Msg.Prompt;

			protected override POReceipt GetByBarcode(string barcode)
			{
				POReceipt receipt =
					SelectFrom<POReceipt>.
					LeftJoin<Vendor>.On<POReceipt.vendorID.IsEqual<Vendor.bAccountID>>.SingleTableOnly.
					Where<
						POReceipt.receiptNbr.IsEqual<@P.AsString>.
						And<
							Vendor.bAccountID.IsNull.
							Or<Match<Vendor, AccessInfo.userName.FromCurrent>>>>.
					View.ReadOnly.Select(Basis, barcode);
				return receipt;
			}

			protected override void Apply(POReceipt receipt)
			{
				Basis.ReceiptType = receipt.ReceiptType;
				Basis.RefNbr = receipt.ReceiptNbr;
				Basis.TranDate = receipt.ReceiptDate;
				Basis.NoteID = receipt.NoteID;

				Basis.Graph.Document.Current = receipt;
			}

			protected override void ClearState()
			{
				Basis.Graph.Document.Current = null;

				Basis.ReceiptType = null;
				Basis.RefNbr = null;
				Basis.TranDate = null;
				Basis.NoteID = null;
			}

			protected override void ReportMissing(string barcode) => Basis.ReportError(Msg.Missing, barcode);
			protected override void ReportSuccess(POReceipt receipt) => Basis.ReportInfo(Msg.Ready, receipt.ReceiptNbr);

			#region Messages
			[PXLocalizable]
			public abstract class Msg
			{
				public const string Prompt = "Scan the receipt number.";
				public const string Ready = "The {0} receipt is loaded and ready to be processed.";
				public const string Missing = "The {0} receipt is not found.";
				public const string InvalidStatus = "The {0} receipt cannot be processed because it has the {1} status.";
				public const string InvalidType = "The {0} receipt cannot be processed because it has the {1} type.";
				public const string InvalidOrderType = "The {0} receipt cannot be processed because it has the {1} order type.";
				public const string MultiSites = "The {0} receipt should have only one warehouse to be processed.";
				public const string HasNonStockKit = "The {0} receipt cannot be processed because it contains a non-stock kit item.";
			}
			#endregion
		}
		#endregion

		#region Modes' common methods
		public PXDelegateResult SortedResult(IEnumerable result)
		{
			var res = new PXDelegateResult();
			res.AddRange(result.Cast<object>());
			res.IsResultSorted = true;
			return res;
		}

		public virtual IEnumerable<PXResult<POReceiptLineSplit, POReceiptLine>> GetSplits(POReceipt receipt, bool includeUnassigned = false, bool sortByBeingProcessed = false, Func<POReceiptLineSplit, bool> processedSeparator = null)
		{
			if (receipt == null)
				return Array.Empty<PXResult<POReceiptLineSplit, POReceiptLine>>();

			var assignedOnly =
				SelectFrom<POReceiptLineSplit>.
				InnerJoin<POReceiptLine>.On<POReceiptLineSplit.FK.ReceiptLine>.
				Where<
					POReceiptLineSplit.receiptType.IsEqual<@P.AsString.ASCII>.
					And<POReceiptLineSplit.receiptNbr.IsEqual<@P.AsString>>>.
				View.Select(Graph, receipt.ReceiptType, receipt.ReceiptNbr)
				.AsEnumerable()
				.Cast<PXResult<POReceiptLineSplit, POReceiptLine>>();

			IEnumerable<PXResult<POReceiptLineSplit, POReceiptLine>> splits;
			if (includeUnassigned)
			{
				POReceiptLineSplit MakeAssigned(Unassigned.POReceiptLineSplit unassignedSplit) => PropertyTransfer.Transfer(unassignedSplit, new POReceiptLineSplit());

				var unassignedOnly =
					SelectFrom<Unassigned.POReceiptLineSplit>.
					InnerJoin<POReceiptLine>.On<Unassigned.POReceiptLineSplit.FK.ReceiptLine>.
					Where<
						Unassigned.POReceiptLineSplit.receiptType.IsEqual<@P.AsString.ASCII>.
						And<Unassigned.POReceiptLineSplit.receiptNbr.IsEqual<@P.AsString>>>.
					View.Select(Graph, receipt.ReceiptType, receipt.ReceiptNbr)
					.AsEnumerable()
					.Cast<PXResult<Unassigned.POReceiptLineSplit, POReceiptLine>>()
					.Select(r => new PXResult<POReceiptLineSplit, POReceiptLine>(MakeAssigned(r), r));

				splits = assignedOnly.Concat(unassignedOnly);
			}
			else
				splits = assignedOnly;

			var result = new List<PXResult<POReceiptLineSplit, POReceiptLine>>();
			if (sortByBeingProcessed)
			{
				processedSeparator ??= r => r.ReceivedQty == r.Qty;
				(var processed, var notProcessed) = splits.DisuniteBy(r => processedSeparator(r));

				result.AddRange(
					notProcessed
					.OrderBy(r => r.GetItem<POReceiptLineSplit>().IsUnassigned == false) // unassigned first
					.ThenBy(r => r.GetItem<POReceiptLineSplit>().InventoryID)
					.ThenBy(r => r.GetItem<POReceiptLineSplit>().LotSerialNbr));

				result.AddRange(
					processed
					.OrderByDescending(r => r.GetItem<POReceiptLineSplit>().InventoryID)
					.ThenByDescending(r => r.GetItem<POReceiptLineSplit>().LotSerialNbr));
			}
			else // keep the order similar to what it is in PO Order
			{
				result.AddRange(
					splits
					.OrderBy(r => r.GetItem<POReceiptLine>().ReceiptNbr)
					.ThenBy(r => r.GetItem<POReceiptLine>().PONbr == null) // expected lines first
					.ThenBy(r => r.GetItem<POReceiptLine>().LineNbr)
					.ThenBy(r => r.GetItem<POReceiptLineSplit>().SplitLineNbr));
			}

			return result;
		}

		protected virtual bool ApplyLinesQtyChanges(bool completePOLines)
		{
			PXSelectBase<POReceiptLine> lines = Graph.transactions;
			PXSelectBase<POReceiptLineSplit> splits = Graph.splits;

			bool anyChanges = false;

			foreach (POReceiptLine line in lines.Select())
			{
				lines.Current = line;

				decimal lineQty = 0;
				foreach (POReceiptLineSplit split in splits.Select())
				{
					splits.Current = split;

					anyChanges |= splits.Current.Qty != splits.Current.ReceivedQty;

					splits.Current.Qty = splits.Current.ReceivedQty;
					splits.UpdateCurrent();

					if (splits.Current.ReceivedQty == 0)
					{
						splits.DeleteCurrent();
						anyChanges = true;
					}
					else
						lineQty += splits.Current.ReceivedQty ?? 0;
				}

				lines.Current.Qty = INUnitAttribute.ConvertFromBase(lines.Cache, lines.Current.InventoryID, lines.Current.UOM, lineQty, INPrecision.NOROUND);
				lines.UpdateCurrent();
				if (completePOLines && lines.Current.Qty > 0)
					lines.Cache.SetValueExt<POReceiptLine.allowComplete>(line, true);

				if (lines.Current.Qty == 0)
				{
					lines.DeleteCurrent();
					anyChanges = true;
				}
			}

			return anyChanges;
		}

		protected virtual bool HasNonStockKit(POReceipt receipt)
		{
			var nonStockKitLines =
				SelectFrom<POReceiptLine>.
				InnerJoin<InventoryItem>.On<POReceiptLine.FK.InventoryItem>.
				Where<
					POReceiptLine.receiptType.IsEqual<@P.AsString.ASCII>.
					And<POReceiptLine.receiptNbr.IsEqual<@P.AsString>>.
					And<InventoryItem.stkItem.IsEqual<False>>.
					And<InventoryItem.kitItem.IsEqual<True>>>.
				View.Select(Graph, receipt.ReceiptType, receipt.ReceiptNbr);
			return nonStockKitLines.Count != 0;
		}

		protected virtual bool HasSingleSiteInLines(POReceipt receipt, out int? singleSiteID)
		{
			var linesGrouped =
				SelectFrom<POReceiptLine>.
				Where<
					POReceiptLine.receiptType.IsEqual<@P.AsString.ASCII>.
					And<POReceiptLine.receiptNbr.IsEqual<@P.AsString>>.
					And<POReceiptLine.siteID.IsNotNull>>.
				AggregateTo<GroupBy<POReceiptLine.siteID>>.
				View.Select(Graph, receipt.ReceiptType, receipt.ReceiptNbr);
			if (linesGrouped.Count == 0)
			{
				singleSiteID = receipt.SiteID;
				return singleSiteID != null;
			}
			else if (linesGrouped.Count == 1)
			{
				singleSiteID = ((POReceiptLine)linesGrouped).SiteID;
				return true;
			}
			else
			{
				singleSiteID = null;
				return false;
			}
		}

		public virtual bool EnsureLocationPrimaryItem(int? inventoryID, int? locationID)
		{
			var location = INLocation.PK.Find(this, locationID);
			if (location != null)
			{
				if (location.PrimaryItemValid == INPrimaryItemValid.PrimaryItemError && inventoryID != location.PrimaryItemID)
					return false;

				if (location.PrimaryItemValid == INPrimaryItemValid.PrimaryItemClassError)
				{
					var item = InventoryItem.PK.Find(this, inventoryID);
					if (item != null && item.ItemClassID != location.PrimaryItemClassID)
						return false;
				}
			}
			return true;
		}
		#endregion

		#region Attached Fields
		public static class FieldAttached
		{
			public abstract class To<TTable> : PXFieldAttachedTo<TTable>.By<Host>
				where TTable : class, IBqlTable, new()
			{ }

			[PXUIField(DisplayName = Msg.Fits)]
			public class Fits : FieldAttached.To<POReceiptLineSplit>.AsBool.Named<Fits>
			{
				public override bool? GetValue(POReceiptLineSplit row)
				{
					bool fits = true;
					if (Base.WMS.LocationID != null)
						fits &= Base.WMS.LocationID == row.LocationID;
					if (Base.WMS.InventoryID != null)
						fits &= Base.WMS.InventoryID == row.InventoryID && Base.WMS.SubItemID == row.SubItemID;
					if (Base.WMS.LotSerialNbr != null)
						fits &= string.Equals(Base.WMS.LotSerialNbr, row.LotSerialNbr, StringComparison.OrdinalIgnoreCase);
					return fits;
				}
			}

			[PXUIField(Visible = false)]
			public class ShowLog : FieldAttached.To<ScanHeader>.AsBool.Named<ShowLog>
			{
				public override bool? GetValue(ScanHeader row) => Base.WMS.Setup.Current.ShowScanLogTab == true;
			}
		}
		#endregion

		#region Messages
		[PXLocalizable]
		public new abstract class Msg : WMSBase.Msg
		{
			public const string ReceiptIsNotEditable = "The receipt became unavailable for editing. Contact your manager.";
			public const string SetupLotSerialFlagsInconsistency = "The {0} check box cannot be cleared if the {1} check box is selected.";
		}
		#endregion
	}

	/// <exclude/>
	public sealed class RPAScanHeader : PXCacheExtension<WMSScanHeader, QtyScanHeader, ScanHeader>
	{
		#region DefaultLocationID
		[Location(typeof(WMSScanHeader.siteID))]
		public int? DefaultLocationID { get; set; }
		public abstract class defaultLocationID : BqlInt.Field<defaultLocationID> { }
		#endregion
		#region PutAwayToLocationID
		[Location(typeof(WMSScanHeader.siteID))]
		public int? PutAwayToLocationID { get; set; }
		public abstract class putAwayToLocationID : BqlInt.Field<putAwayToLocationID> { }
		#endregion

		#region Released
		[PXBool]
		[PXUnboundDefault(false, typeof(POReceipt.released))]
		[PXFormula(typeof(Default<WMSScanHeader.refNbr>))]
		public bool? Released { get; set; }
		public abstract class released : BqlBool.Field<released> { }
		#endregion

		#region ForceInsertLine
		[PXBool, PXUnboundDefault(false)]
		public bool? ForceInsertLine { get; set; }
		public abstract class forceInsertLine : BqlBool.Field<forceInsertLine> { }
		#endregion
		#region PrevInventoryID
		[PXInt]
		public int? PrevInventoryID { get; set; }
		public abstract class prevInventoryID : BqlInt.Field<prevInventoryID> { }
		#endregion
		#region BaseExcessQty
		[PXDecimal(6)]
		public decimal? BaseExcessQty { get; set; }
		public abstract class baseExcessQty : BqlDecimal.Field<baseExcessQty> { }
		#endregion

		#region ReceiptType
		[PXDBString(2, IsFixed = true)]
		public string ReceiptType { get; set; }
		public abstract class receiptType : BqlString.Field<receiptType> { }//fill the value
		#endregion
		#region TransferRefNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Transfer Ref Nbr.", Enabled = false)]
		[PXSelector(typeof(
			SelectFrom<INRegister>.
			Where<INRegister.docType.IsEqual<INDocType.transfer>>.
			OrderBy<INRegister.refNbr.Desc>.
			SearchFor<INRegister.refNbr>),
			Filterable = true)]
		[PXUIVisible(typeof(Where<ScanHeader.mode.IsEqual<ReceivePutAway.PutAwayMode.value>>))]
		public String TransferRefNbr { get; set; }
		public abstract class transferRefNbr : BqlString.Field<transferRefNbr> { }
		#endregion
		#region PONbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "PO Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(
			SelectFrom<POOrder>.
			Where<POOrder.orderType.IsEqual<POOrderType.regularOrder>>.
			OrderBy<POOrder.orderNbr.Desc>.
			SearchFor<POOrder.orderNbr>),
			Filterable = true)]
		[PXUIVisible(typeof(Where<ScanHeader.mode.IsEqual<ReceivePutAway.ReceiveMode.value>>))]
		public String PONbr { get; set; }
		public abstract class pONbr : BqlString.Field<transferRefNbr> { }
		#endregion

		#region InventoryMultiplicator
		// Acuminator disable once PX1029 PXGraphUsageInDac [false positive]
		[PXShort]
		public short? InventoryMultiplicator
		{
			get => Base.Mode == PX.Objects.PO.WMS.ReceivePutAway.ReturnMode.Value
				? IN.InventoryMultiplicator.Decrease
				: IN.InventoryMultiplicator.Increase;
			set { }
		}
		public abstract class inventoryMultiplicator : BqlShort.Field<inventoryMultiplicator> { }
		#endregion
	}
}
