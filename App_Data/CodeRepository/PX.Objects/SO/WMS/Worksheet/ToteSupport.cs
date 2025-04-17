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
using System.Collections.Generic;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.BarcodeProcessing;

using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	public class ToteSupport : WorksheetPicking.ScanExtension
	{
		public static bool IsActive() => IsActiveBase();

		#region State
		public ToteScanHeader ToteHeader => Basis.Header.Get<ToteScanHeader>() ?? new ToteScanHeader();
		public ValueSetter<ScanHeader>.Ext<ToteScanHeader> ToteSetter => Basis.HeaderSetter.With<ToteScanHeader>();

		#region AddNewTote
		public bool? AddNewTote
		{
			get => ToteHeader.AddNewTote;
			set => ToteSetter.Set(h => h.AddNewTote, value);
		}
		#endregion
		#region ToteID
		public int? ToteID
		{
			get => ToteHeader.ToteID;
			set => ToteSetter.Set(h => h.ToteID, value);
		}
		#endregion
		#region PreparedForPackToteIDs
		public ISet<int> PreparedForPackToteIDs => ToteHeader.PreparedForPackToteIDs;
		#endregion
		#endregion

		#region Logic
		public string ShipmentJustAssignedWithTote { get; set; }

		public virtual bool AllowMultipleTotesPerShipment => Basis.Setup.Current.AllowMultipleTotesPerShipment == true;
		public virtual bool CanAddNewTote => AllowMultipleTotesPerShipment && WSBasis.CanWSPick && NoEmptyTotes && !HasAnotherToAssign;
		public virtual bool NoEmptyTotes => WSBasis.PickListOfPicker.SelectMain().GroupBy(s => s.ToteID).All(tote => tote.Any(s => s.PickedQty > 0 || s.ForceCompleted == true));
		public virtual bool IsToteSelectionNeeded => false;

		public virtual bool HasAnotherToAssign => NextShipmentWithoutTote != null;
		public virtual SOPickerToShipmentLink NextShipmentWithoutTote => WSBasis.ShipmentsOfPicker.SelectMain().FirstOrDefault(s => s.ToteID == INTote.UnassignedToteID);

		public virtual bool HasAnotherToPrepare => TotesToPrepareForPack.Length - PreparedForPackToteIDs.Count > 0;
		public virtual INTote[] TotesToPrepareForPack =>
			SelectFrom<INTote>.
			InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Tote>.
			Where<
				SOPickerToShipmentLink.shipmentNbr.IsEqual<SOShipment.shipmentNbr.FromCurrent>>.
			View
			.Select(Basis)
			.RowCast<INTote>()
			.ToArray();

		public virtual string GetShipmentToAddToteTo() => Basis.RefNbr;

		public virtual bool TryAssignTotesFromCart(string barcode)
		{
			INCart cart =
				SelectFrom<INCart>.
				InnerJoin<INSite>.On<INCart.FK.Site>.
				Where<
					INCart.siteID.IsEqual<WMSScanHeader.siteID.FromCurrent>.
					And<INCart.cartCD.IsEqual<@P.AsString>>.
					And<Match<INSite, AccessInfo.userName.FromCurrent>>>.
				View.Select(Basis, barcode);
			if (cart == null)
				return false;

			var shipmentsOfPicker = WSBasis.ShipmentsOfPicker.SelectMain();
			if (shipmentsOfPicker.Any(s => s.ToteID != INTote.UnassignedToteID))
			{
				Basis.Reporter.Error(Msg.ToteAlreadyAssignedCannotAssignCart, cart.CartCD);
				return true;
			}

			bool cartIsBusy =
				SelectFrom<SOPickerToShipmentLink>.
				InnerJoin<SOPickingWorksheet>.On<SOPickerToShipmentLink.FK.Worksheet>.
				InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
				InnerJoin<INCart>.On<INTote.FK.Cart>.
				InnerJoin<SOShipment>.On<SOPickerToShipmentLink.FK.Shipment>.
				Where<
					SOPickingWorksheet.worksheetType.IsEqual<SOPickingWorksheet.worksheetType.wave>.
					And<SOShipment.confirmed.IsEqual<False>>.
					And<INCart.siteID.IsEqual<@P.AsInt>>.
					And<INCart.cartID.IsEqual<@P.AsInt>>>.
				View.ReadOnly.Select(Basis, cart.SiteID, cart.CartID).Any();
			if (cartIsBusy)
			{
				Basis.Reporter.Error(PPSCartSupport.CartState.Msg.IsOccupied, cart.CartCD);
				return true;
			}

			var totes = INTote.FK.Cart.SelectChildren(Basis, cart).Where(t => t.Active == true).ToArray();
			if (shipmentsOfPicker.Length > totes.Length)
			{
				Basis.Reporter.Error(Msg.TotesAreNotEnoughInCart, cart.CartCD);
				return true;
			}

			foreach (var (link, tote) in shipmentsOfPicker.Zip(totes, (link, tote) => (link, tote)))
				AssignTote(link, tote);

			WSBasis.Picker.Current.CartID = cart.CartID;
			WSBasis.Picker.UpdateCurrent();

			Basis.SaveChanges();

			if (Basis.Get<PPSCartSupport>() is PPSCartSupport cartSup)
				cartSup.CartID = cart.CartID;

			Basis.DispatchNext(Msg.TotesFromCartAreAssigned, shipmentsOfPicker.Length, cart.CartCD);
			return true;
		}

		public virtual void AssignTote(INTote tote)
		{
			if (AddNewTote == true)
			{
				if (AllowMultipleTotesPerShipment == false)
					throw new InvalidOperationException();

				ShipmentJustAssignedWithTote = GetShipmentToAddToteTo();

				if (ShipmentJustAssignedWithTote == null)
					throw new InvalidOperationException();

				var shipmentLink = new SOPickerToShipmentLink
				{
					WorksheetNbr = WSBasis.WorksheetNbr,
					PickerNbr = WSBasis.PickerNbr,
					ShipmentNbr = ShipmentJustAssignedWithTote,
					SiteID = Basis.SiteID,
				};

				AssignTote(shipmentLink, tote);

				AddNewTote = false;
			}
			else
			{
				var shipmentLink = NextShipmentWithoutTote;
				if (shipmentLink == null)
					throw new InvalidOperationException();

				ShipmentJustAssignedWithTote = shipmentLink.ShipmentNbr;

				AssignTote(shipmentLink, tote);
				WSBasis.AssignUser(startPicking: false);
			}

			Basis.SaveChanges();
		}

		public virtual void AssignTote(SOPickerToShipmentLink link, INTote tote)
		{
			if (WSBasis.ShipmentsOfPicker.Locate(link) is SOPickerToShipmentLink existingLink)
			{
				var newLink = PXCache<SOPickerToShipmentLink>.CreateCopy(existingLink);
				newLink.ToteID = tote.ToteID;
				WSBasis.ShipmentsOfPicker.Delete(existingLink);
				WSBasis.ShipmentsOfPicker.Insert(newLink);
			}
			else
			{
				link.ToteID = tote.ToteID;
				WSBasis.ShipmentsOfPicker.Insert(link);
			}

			var shipmentSplits = WSBasis.PickListOfPicker.SearchAll<Asc<SOPickerListEntry.shipmentNbr>>(new[] { link.ShipmentNbr });
			foreach (SOPickerListEntry split in shipmentSplits)
				if (split.PickedQty < split.Qty)
					MoveSplitRestQtyToAnotherTote(split, tote.ToteID);
		}

		public virtual SOPickerListEntry EnsureSplitQtyInTote(SOPickerListEntry pickedSplit, decimal deltaQty)
		{
			if (pickedSplit != null && ToteID != null && deltaQty > 0)
			{
				if (pickedSplit.ToteID != ToteID && pickedSplit.PickedQty + deltaQty <= pickedSplit.Qty)
				{ // selected split has a wrong tote, but it can accept the whole qty
					pickedSplit = MoveSplitRestQtyToAnotherTote(pickedSplit, ToteID);
				}
				else if (pickedSplit.ToteID == ToteID && pickedSplit.PickedQty + deltaQty > pickedSplit.Qty)
				{ // selected split has the right tote, but it cannot accept the whole qty
					if (TryBorrowMissingQtyFromSimilarSplitInAnotherTote(pickedSplit, qtyToBorrow: deltaQty - (pickedSplit.Qty.Value - pickedSplit.PickedQty.Value)))
						pickedSplit = WSBasis.PickListOfPicker.Locate(pickedSplit);
				}
			}

			return pickedSplit;
		}

		public virtual bool TryRemoveTote(INTote tote)
		{
			SOPickerToShipmentLink link = WSBasis.ShipmentsOfPicker.Search<SOPickerToShipmentLink.toteID>(tote.ToteID);
			return TryRemoveToteOf(link);
		}

		public virtual bool TryRemoveToteOf(SOPickerToShipmentLink link)
		{
			SOPickerToShipmentLink previousLink = WSBasis.ShipmentsOfPicker
				.SearchAll<Asc<SOPickerToShipmentLink.shipmentNbr>>(new[] { link.ShipmentNbr })
				.AsEnumerable().RowCast<SOPickerToShipmentLink>()
				.Where(l => l.ToteID != link.ToteID)
				.OrderByDescending(l => l.LastModifiedDateTime)
				.FirstOrDefault();

			if (previousLink == null)
				return false;

			var currentToteEntries = WSBasis.PickListOfPicker
				.SearchAll<Asc<SOPickerListEntry.shipmentNbr, Asc<SOPickerListEntry.toteID>>>(new object[] { link.ShipmentNbr, link.ToteID })
				.AsEnumerable().RowCast<SOPickerListEntry>()
				.ToArray();
			var previousToteEntries = WSBasis.PickListOfPicker
				.SearchAll<Asc<SOPickerListEntry.shipmentNbr, Asc<SOPickerListEntry.toteID>>>(new object[] { previousLink.ShipmentNbr, previousLink.ToteID })
				.AsEnumerable().RowCast<SOPickerListEntry>()
				.Where(r => r.ForceCompleted != true)
				.ToArray();

			foreach (var entry in currentToteEntries)
			{
				if (entry.PickedQty != 0)
					throw new InvalidOperationException();

				var acceptorEntry = previousToteEntries.FirstOrDefault(ps =>
					WSBasis.AreSplitsSimilar(ps, entry) &&
					ps.IsUnassigned == entry.IsUnassigned &&
					ps.HasGeneratedLotSerialNbr == entry.HasGeneratedLotSerialNbr);

				if (acceptorEntry != null)
				{
					acceptorEntry.Qty += entry.Qty;

					WSBasis.PickListOfPicker.Delete(entry);
					WSBasis.PickListOfPicker.Update(acceptorEntry);
				}
				else
				{
					entry.ToteID = previousLink.ToteID;
					WSBasis.PickListOfPicker.Update(entry);
				}
			}

			WSBasis.ShipmentsOfPicker.Delete(link);

			return true;
		}

		public virtual INTote GetProperTote() => GetToteForPickListEntry(WSBasis.GetSelectedPickListEntry(), certain: false);

		public virtual INTote GetToteForPickListEntry(SOPickerListEntry entry, bool certain = false)
		{
			if (entry == null)
				return null;

			if (certain == true)
				return INTote.PK.Find(Basis, entry.SiteID, entry.ToteID);

			var totesOfShipment =
				SelectFrom<INTote>.
				InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Tote>.
				Where<
					SOPickerToShipmentLink.worksheetNbr.IsEqual<SOPickerListEntry.worksheetNbr.FromCurrent>.
					And<SOPickerToShipmentLink.pickerNbr.IsEqual<SOPickerListEntry.pickerNbr.FromCurrent>>.
					And<SOPickerToShipmentLink.shipmentNbr.IsEqual<SOPickerListEntry.shipmentNbr.FromCurrent>>>.
				View.SelectMultiBound(Basis, new object[] { entry });

			if (totesOfShipment.Count == 1)
				return (INTote)totesOfShipment;
			else
				return null; // there are multiple possible totes
		}

		public virtual SOPickerListEntry MoveSplitRestQtyToAnotherTote(SOPickerListEntry split, int? toteID)
		{
			SOPickerToShipmentLink selectedToteShipment = WSBasis.ShipmentsOfPicker.Search<SOPickerToShipmentLink.toteID>(toteID);
			if (selectedToteShipment == null || selectedToteShipment.ShipmentNbr != split.ShipmentNbr)
			{
				// the tote is not assigned to the pick list, or it is already assigned to another shipment.
				// nothing to do, the further code will produce an error
				return split;
			}
			else if (split.PickedQty == 0)
			{
				// move not started split to a new tote, because we suppose that the tote was added exactly for this
				split.ToteID = toteID;
				return WSBasis.PickListOfPicker.Update(split);
			}
			else
			{
				// split started split in two parts: already picked qty and not started qty
				var pickedSplit = PXCache<SOPickerListEntry>.CreateCopy(split);
				pickedSplit.EntryNbr = null;
				pickedSplit.Qty = pickedSplit.PickedQty;

				var restSplit = PXCache<SOPickerListEntry>.CreateCopy(split);
				restSplit.EntryNbr = null;
				restSplit.Qty = split.Qty - split.PickedQty;
				restSplit.PickedQty = 0m;
				restSplit.ToteID = toteID;

				WSBasis.PickListOfPicker.Delete(split);
				WSBasis.PickListOfPicker.Insert(pickedSplit);
				return WSBasis.PickListOfPicker.Insert(restSplit);
			}
		}

		public virtual bool TryBorrowMissingQtyFromSimilarSplitInAnotherTote(SOPickerListEntry split, decimal qtyToBorrow)
		{
			var anotherToteDonorSplit = WSBasis.PickListOfPicker
				.SelectMain()
				.Where(r =>
					WSBasis.AreSplitsSimilar(r, split) &&
					r.ToteID != split.ToteID &&
					r.Qty - r.PickedQty >= qtyToBorrow)
				.FirstOrDefault();

			if (anotherToteDonorSplit != null)
			{ // borrow the needed qty from similar splits with different totes
				if (anotherToteDonorSplit.PickedQty == 0 && qtyToBorrow == anotherToteDonorSplit.Qty)
				{
					WSBasis.PickListOfPicker.Delete(anotherToteDonorSplit);
				}
				else
				{
					anotherToteDonorSplit.Qty -= qtyToBorrow;
					WSBasis.PickListOfPicker.Update(anotherToteDonorSplit);
				}

				split.Qty += qtyToBorrow;
				split = WSBasis.PickListOfPicker.Update(split);

				return true;
			}

			return false;
		}
		#endregion

		#region States
		public abstract class ToteState : PickPackShip.EntityState<INTote>
		{
			public WorksheetPicking WSBasis => Get<WorksheetPicking>();
			public ToteSupport ToteBasis => Get<ToteSupport>();

			protected override INTote GetByBarcode(string barcode) => INTote.UK.Find(Basis, Basis.SiteID, barcode);

			protected override Validation Validate(INTote tote)
			{
				if (tote.Active == false)
					return Validation.Fail(Msg.Inactive, tote.ToteCD);

				return base.Validate(tote);
			}

			protected override void ReportMissing(string barcode) => Basis.Reporter.Error(Msg.Missing, barcode);
			protected override void ReportSuccess(INTote tote) => Basis.Reporter.Info(Msg.Ready, tote.ToteCD);

			#region Messages
			[PXLocalizable]
			public abstract class Msg
			{
				public const string Ready = "The {0} tote is selected.";
				public const string Missing = "The {0} tote is not found.";
				public const string Inactive = "The {0} tote is inactive.";
			}
			#endregion
		}

		public sealed class AssignToteState : ToteState
		{
			public const string Value = "ASST";
			public class value : BqlString.Constant<value> { public value() : base(AssignToteState.Value) { } }

			public override string Code => Value;
			protected override string StatePrompt => Basis.Localize(Msg.Prompt, ToteBasis.With(ts => ts.AddNewTote == true
				? ts.GetShipmentToAddToteTo()
				: ts.NextShipmentWithoutTote?.ShipmentNbr));

			protected override bool IsStateSkippable() => ToteBasis.With(ts => ts.AddNewTote != true && ts.NextShipmentWithoutTote == null);

			protected override AbsenceHandling.Of<INTote> HandleAbsence(string barcode)
			{
				if (ToteBasis.AddNewTote != true && ToteBasis.TryAssignTotesFromCart(barcode))
					return AbsenceHandling.Done;

				return base.HandleAbsence(barcode);
			}

			protected override Validation Validate(INTote tote)
			{
				if (Basis.HasFault(tote, base.Validate, out var fault))
					return fault;

				if (tote.AssignedCartID != null)
					return Validation.Fail(Msg.CannotBeUsedSeparatly, tote.ToteCD);

				bool toteIsBusy =
					SelectFrom<SOPickerToShipmentLink>.
					InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
					InnerJoin<SOShipment>.On<SOPickerToShipmentLink.FK.Shipment>.
					Where<
						INTote.siteID.IsEqual<@P.AsInt>.
						And<INTote.toteID.IsEqual<@P.AsInt>>.
						And<SOShipment.confirmed.IsEqual<False>>>.
					View.Select(Basis, tote.SiteID, tote.ToteID).Any();
				if (toteIsBusy)
					return Validation.Fail(Msg.Busy, tote.ToteCD);

				return Validation.Ok;
			}

			protected override void Apply(INTote tote) => ToteBasis.AssignTote(tote);

			protected override void ClearState() => ToteBasis.AddNewTote = false;

			protected override void ReportSuccess(INTote tote) => Basis.Reporter.Info(Msg.Ready, tote.ToteCD, ToteBasis.ShipmentJustAssignedWithTote);

			protected override void SetNextState()
			{
				if (ToteBasis.HasAnotherToAssign)
					Basis.SetScanState<AssignToteState>(); // set the same state to change the prompt message
				else
					base.SetNextState();
			}

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ToteState.Msg
			{
				public const string Prompt = "Scan the tote barcode for the {0} shipment.";
				public new const string Ready = "The {0} tote is selected for the {1} shipment.";

				public const string CannotBeUsedSeparatly = "The {0} tote cannot be used separately from the cart.";
				public const string Busy = "The {0} tote cannot be selected because it is already assigned to another shipment.";
			}
			#endregion
		}

		public sealed class SelectToteState : ToteState
		{
			public const string Value = "TOTE";
			public class value : BqlString.Constant<value> { public value() : base(SelectToteState.Value) { } }

			public override string Code => Value;
			protected override string StatePrompt => Basis.Remove != true ? Msg.PromptAdd : Msg.PromptRemove;

			protected override bool IsStateActive() => base.IsStateActive() && WSBasis.ShipmentsOfPicker.Select().Count > 1;
			protected override bool IsStateSkippable() => Basis.Remove != true && ToteBasis.GetProperTote() != null;

			protected override Validation Validate(INTote tote)
			{
				if (Basis.HasFault(tote, base.Validate, out var fault))
					return fault;

				SOPickerToShipmentLink selectedToteLink = WSBasis.ShipmentsOfPicker.Search<SOPickerToShipmentLink.toteID>(tote.ToteID);
				if (selectedToteLink == null)
					return Validation.Fail(Msg.MissingAssigned,
						tote.ToteCD,
						WSBasis.ShipmentsOfPicker.SelectMain()
							.Select(link => link.ToteID)
							.Select(tid => INTote.PK.Find(Basis, Basis.SiteID, tid).ToteCD)
							.With(tcds => string.Join(", ", tcds)));

				return Validation.Ok;
			}

			protected override void Apply(INTote tote) => ToteBasis.ToteID = tote.ToteID;
			protected override void ClearState() => ToteBasis.ToteID = null;

			protected override void SetNextState()
			{
				if (Basis.Remove == true)
					Basis.SetDefaultState();
				else
					base.SetNextState();
			}

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ToteState.Msg
			{
				public const string PromptAdd = "Scan the barcode of the tote to which you want to add the items.";
				public const string PromptRemove = "Scan the barcode of the tote from which you want to remove the items.";
				public const string MissingAssigned = "The {0} tote is not assigned to the current pick list. Available totes: {1}.";
			}
			#endregion
		}

		public sealed class PrepareToteForPackState : ToteState
		{
			public const string Value = "PRET";
			public class value : BqlString.Constant<value> { public value() : base(PrepareToteForPackState.Value) { } }

			public override string Code => Value;
			protected override string StatePrompt
				=> ToteBasis.With(ts => ts.Basis.Localize(Msg.Prompt, ts.TotesToPrepareForPack.FirstOrDefault(tote => ts.PreparedForPackToteIDs.Contains(tote.ToteID.Value) == false).ToteCD));

			protected override string StateInstructions
				=> ToteBasis.With(ts => ts.Basis.Localize(Msg.Instruction, ts.PreparedForPackToteIDs.Count, ts.TotesToPrepareForPack.Length, ts.Basis.RefNbr));

			protected override void OnTakingOver()
			{
				if (Basis.Setup.Current.AllowMultipleTotesPerShipment == false && ToteBasis.TotesToPrepareForPack.Length == 1)
				{
					ToteBasis.PreparedForPackToteIDs.Add(ToteBasis.TotesToPrepareForPack[0].ToteID.Value);
					MoveToNextState();
				}
			}

			protected override Validation Validate(INTote tote)
			{
				if (Basis.HasFault(tote, base.Validate, out var fault))
					return fault;

				if (ToteBasis.TotesToPrepareForPack.Select(t => t.ToteID).Contains(tote.ToteID) == false)
					return Validation.Fail(Msg.WrongTote, tote.ToteCD, Basis.RefNbr);

				if (ToteBasis.PreparedForPackToteIDs.Contains(tote.ToteID.Value) == true)
					return Validation.Fail(Msg.AlreadyPreparedTote, tote.ToteCD);

				return Validation.Ok;
			}

			protected override void Apply(INTote tote) => ToteBasis.PreparedForPackToteIDs.Add(tote.ToteID.Value);

			protected override void ClearState() => ToteBasis.PreparedForPackToteIDs.Clear();

			protected override void ReportSuccess(INTote tote)
				=> ToteBasis.Call(ts => ts.Basis.Reporter.Info(Msg.Ready, tote.ToteCD, ts.PreparedForPackToteIDs.Count, ts.TotesToPrepareForPack.Length, ts.Basis.RefNbr));

			protected override void SetNextState()
			{
				if (ToteBasis.HasAnotherToPrepare)
					Basis.SetScanState<PrepareToteForPackState>(); // set the same state to change the prompt message
				else
					Basis.SetDefaultState();
			}

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ToteState.Msg
			{
				public const string Prompt = "Scan the barcode of the {0} tote.";
				public const string Instruction = "{0} of {1} totes scanned for the {2} shipment.";
				public const string WrongTote = "The {0} tote is not assigned to the {1} shipment.";
				public const string AlreadyPreparedTote = "The {0} tote has already been scanned.";
			}
			#endregion
		}
		#endregion

		#region Commands
		public sealed class AddToteCommand : PickPackShip.ScanCommand
		{
			public override string Code => "ADD*TOTE";
			public override string ButtonName => "scanAddTote";
			public override string DisplayName => Msg.DisplayName;
			protected override bool IsEnabled => Basis.DocumentIsEditable && Basis.Get<ToteSupport>().With(ts => ts.AddNewTote != true && ts.CanAddNewTote);

			protected override bool Process()
			{
				Basis.Reset(fullReset: false);
				Basis.Get<ToteSupport>().AddNewTote = true;
				Basis.SetScanState<AssignToteState>();
				return true;
			}

			#region Messages
			[PXLocalizable]
			public abstract class Msg
			{
				public const string DisplayName = "Add Tote";
			}
			#endregion
		}
		#endregion

		#region Decoration
		public virtual void InjectRemoveMovesToRemoveFromTote(PickPackShip.RemoveCommand remove)
		{
			remove
				.Intercept.Process.ByOverride((basis, base_Process) =>
				{
					bool result = base_Process();

					if (basis.Get<ToteSupport>().IsToteSelectionNeeded)
						basis.SetScanState<SelectToteState>();

					return result;
				});
		}

		public virtual void InjectRemoveDisableWhenAssignTote(PickPackShip.RemoveCommand remove)
		{
			remove
				.Intercept.IsEnabled.ByConjoin(basis => !(basis.CurrentState is AssignToteState));
		}

		public virtual void InjectShipmentAbsenceHandlingByTote(PickPackShip.PackMode.ShipmentState packShipment)
		{
			int? initialToteID = null;
			packShipment
				.Intercept.StatePrompt.ByReplace(() => Msg.PromptShipmentOrTote)
				.Intercept.HandleAbsence.ByAppend((basis, barcode) =>
				{
					var shipmentInTote = (PXResult<SOShipment, SOPickerToShipmentLink, INTote>)
						SelectFrom<SOShipment>.
						InnerJoin<SOPickerToShipmentLink>.On<SOPickerToShipmentLink.FK.Shipment>.
						InnerJoin<INTote>.On<SOPickerToShipmentLink.FK.Tote>.
						InnerJoin<SOPicker>.On<SOPickerToShipmentLink.FK.Picker>.
						InnerJoin<SOPickingWorksheet>.On<SOPicker.FK.Worksheet>.
						Where<
							INTote.toteCD.IsEqual<@P.AsString>.
							And<SOPickingWorksheet.worksheetType.IsIn<
								SOPickingWorksheet.worksheetType.wave,
								SOPickingWorksheet.worksheetType.single>>.
							And<SOPicker.confirmed.IsEqual<True>>.
							And<SOShipment.picked.IsEqual<True>>.
							And<SOShipment.confirmed.IsEqual<False>>>.
						View.Select(basis, barcode);

					if (shipmentInTote != null)
					{
						initialToteID = shipmentInTote.GetItem<INTote>().ToteID;
						return AbsenceHandling.ReplaceWith(shipmentInTote.GetItem<SOShipment>());
					}

					return AbsenceHandling.Skipped;
				})
				.Intercept.Apply.ByAppend((basis, shipment) =>
				{
					var toteBasis = basis.Get<ToteSupport>();
					toteBasis.PreparedForPackToteIDs.Clear();
					if (initialToteID.HasValue)
						toteBasis.PreparedForPackToteIDs?.Add(initialToteID.Value);
				})
				.Intercept.SetNextState.ByOverride((basis, base_SetNextState) =>
				{
					if (basis.Get<ToteSupport>().HasAnotherToPrepare)
						basis.SetScanState<PrepareToteForPackState>();
					else
						base_SetNextState();
				}, RelativeInject.CloserToBase);
		}

		public virtual void InjectPackPrepareTotesState(PickPackShip.PackMode pack)
		{
			pack
				.Intercept.CreateStates.ByAppend(
					() => new[] { new PrepareToteForPackState() })
				.Intercept.ResetMode.ByAppend(
					(basis, fullReset) => basis.Clear<PrepareToteForPackState>(when: fullReset && !basis.IsWithinReset));
		}
		#endregion

		#region Overrides
		/// Overrides <see cref="WorksheetPicking.IsSelectedSplit(SOPickerListEntry)"/>
		[PXOverride]
		public bool IsSelectedSplit(SOPickerListEntry split,
			Func<SOPickerListEntry, bool> base_IsSelectedSplit)
		{
			return
				base_IsSelectedSplit(split) &&
				split.ToteID == (ToteID ?? split.ToteID);
		}

		/// Overrides <see cref="WorksheetPicking.ConfirmSplit(SOPickerListEntry, decimal)"/>
		[PXOverride]
		public FlowStatus ConfirmSplit(SOPickerListEntry pickedSplit, decimal deltaQty,
			Func<SOPickerListEntry, decimal, FlowStatus> base_ConfirmSplit)
		{
			pickedSplit = EnsureSplitQtyInTote(pickedSplit, deltaQty);
			if (pickedSplit != null && Basis.Remove == false && ToteID != null && pickedSplit.ToteID != ToteID)
			{
				var properTotesIDs = WSBasis.ShipmentsOfPicker
					.SearchAll<Asc<SOPickerToShipmentLink.shipmentNbr>>(new object[] { pickedSplit.ShipmentNbr })
					.RowCast<SOPickerToShipmentLink>()
					.Select(link => INTote.PK.Find(Basis, Basis.SiteID, link.ToteID).ToteCD)
					.With(tcds => string.Join(", ", tcds));

				return FlowStatus.Fail(Msg.MissingToteAssignedToShipment,
					INTote.PK.Find(Basis, Basis.SiteID, ToteID).ToteCD,
					pickedSplit.ShipmentNbr,
					properTotesIDs);
			}

			var result = base_ConfirmSplit(pickedSplit, deltaQty);

			if (result.IsError == false)
				if (deltaQty < 0 && pickedSplit.PickedQty == 0 && pickedSplit.ToteID.IsNotIn(null, INTote.UnassignedToteID))
					if (WSBasis.PickListOfPicker.SearchAll<Asc<SOPickerListEntry.toteID>>(new object[] { pickedSplit.ToteID }).AsEnumerable().RowCast<SOPickerListEntry>().All(s => s.PickedQty == 0))
						TryRemoveTote(INTote.PK.Find(Basis, Basis.SiteID, pickedSplit.ToteID));

			return result;
		}

		/// Overrides <see cref="WorksheetPicking.ReportSplitConfirmed(SOPickerListEntry)"/>
		[PXOverride]
		public void ReportSplitConfirmed(SOPickerListEntry pickedSplit,
			Action<SOPickerListEntry> base_ReportSplitConfirmed)
		{
			INTote targetTote = GetToteForPickListEntry(pickedSplit, certain: true);
			if (targetTote != null)
			{
				Basis.ReportInfo(
					Basis.Remove == true
						? Msg.InventoryRemovedFromTote
						: Msg.InventoryAddedToTote,
					Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM, targetTote.ToteCD);
			}
			else
			{
				base_ReportSplitConfirmed(pickedSplit);
			}
		}

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
		[PXOverride]
		public ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original,
			Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
		{
			var state = base_DecorateScanState(original);

			if (state is PickPackShip.PackMode.ShipmentState packShipment)
				InjectShipmentAbsenceHandlingByTote(packShipment);

			return state;
		}

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanMode"/>
		[PXOverride]
		public ScanMode<PickPackShip> DecorateScanMode(ScanMode<PickPackShip> original,
			Func<ScanMode<PickPackShip>, ScanMode<PickPackShip>> base_DecorateScanMode)
		{
			var mode = base_DecorateScanMode(original);

			if (mode is PickPackShip.PackMode pack)
				InjectPackPrepareTotesState(pack);

			return mode;
		}

		public class AlterCommandOrShipmentOnlyStatePromptError : PickPackShip.ScanExtension<PickPackShip.CommandOrShipmentOnlyState.Logic>
		{
			public static bool IsActive() => ToteSupport.IsActive();
			/// Overrides <see cref="PickPackShip.CommandOrShipmentOnlyState.Logic.GetPromptForCommandOrShipmentOnly"/>
			[PXOverride]
			public string GetPromptForCommandOrShipmentOnly(Func<string> base_GetPromptForCommandOrShipmentOnly)
			{
				return Msg.UseCommandOrShipmentOrToteToContinue;
			}

			/// Overrides <see cref="PickPackShip.CommandOrShipmentOnlyState.Logic.GetErrorForCommandOrShipmentOnly"/>
			[PXOverride]
			public string GetErrorForCommandOrShipmentOnly(Func<string> base_GetErrorForCommandOrShipmentOnly)
			{
				return Msg.OnlyCommandsAndShipmentsOrToteAreAllowed;
			}
		}
		#endregion

		#region Messages
		[PXLocalizable]
		public abstract class Msg
		{
			public const string ToteAlreadyAssignedCannotAssignCart = "Totes from the {0} cart cannot be auto assigned to the pick list because it already has manual assignments.";
			public const string TotesAreNotEnoughInCart = "There are not enough active totes in the {0} cart to assign them to all of the shipments of the pick list.";
			public const string TotesFromCartAreAssigned = "The {0} first totes from the {1} cart were automatically assigned to the shipments of the pick list.";
			public const string MissingToteAssignedToShipment = "The {0} tote is not assigned to the {1} shipment. Available totes: {2}.";

			public const string InventoryAddedToTote = "{0} x {1} {2} has been added to the {3} tote.";
			public const string InventoryRemovedFromTote = "{0} x {1} {2} has been removed from the {3} tote.";

			public const string UseCommandOrShipmentOrToteToContinue = "Use any command or scan the next shipment or tote number to continue.";
			public const string OnlyCommandsAndShipmentsOrToteAreAllowed = "Only commands, a shipment number, or a tote number can be used to continue.";
			public const string PromptShipmentOrTote = "Scan the shipment or tote number.";
		}
		#endregion
	}

	public sealed class ToteScanHeader : PXCacheExtension<WorksheetScanHeader, WMSScanHeader, QtyScanHeader, ScanHeader>
	{
		public static bool IsActive() => WorksheetPicking.IsActive();

		#region AddNewTote
		[PXBool]
		public bool? AddNewTote { get; set; }
		public abstract class addNewTote : BqlBool.Field<addNewTote> { }
		#endregion
		#region ToteID
		[PXInt]
		public int? ToteID { get; set; }
		public abstract class toteID : BqlInt.Field<toteID> { }
		#endregion
		#region PreparedForPackToteIDs
		// Acuminator disable once PX1032 MethodInvocationInDac [field initialization]
		public HashSet<int> PreparedForPackToteIDs { get; set; } = new HashSet<int>();
		public abstract class preparedForPackToteIDs : IBqlField { }
		#endregion
	}
}
