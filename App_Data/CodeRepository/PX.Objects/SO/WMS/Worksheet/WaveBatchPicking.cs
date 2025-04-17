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

using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	using WMSBase = WarehouseManagementSystem<PickPackShip, PickPackShip.Host>;

	public class WaveBatchPicking : WorksheetPicking.ScanExtension
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.wMSAdvancedPicking>();
		public static bool MatchMode(string mode) => mode.IsIn(WavePickMode.Value, BatchPickMode.Value);

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.CreateScanModes"/>
		[PXOverride]
		public virtual IEnumerable<ScanMode<PickPackShip>> CreateScanModes(Func<IEnumerable<ScanMode<PickPackShip>>> base_CreateScanModes)
		{
			foreach (var mode in base_CreateScanModes())
				yield return mode;

			yield return new WavePickMode();
			yield return new BatchPickMode();
		}

		public sealed class WavePickMode : PickPackShip.ScanMode
		{
			public const string Value = "WAVE";
			public class value : BqlString.Constant<value> { public value() : base(WavePickMode.Value) { } }

			public WaveBatchPicking WBBasis => Get<WaveBatchPicking>();

			public override string Code => Value;
			public override string Description => Msg.DisplayName;

			protected override bool IsModeActive() => Basis.HasPick;

			#region State Machine
			protected override IEnumerable<ScanState<PickPackShip>> CreateStates()
			{
				yield return new PickListState();
				yield return new ToteSupport.AssignToteState();
				yield return new WMSBase.LocationState();
				yield return new WMSBase.InventoryItemState() { AlternateType = INPrimaryAlternateType.CPN, IsForIssue = true, SuppressModuleItemStatusCheck = true };
				yield return new WMSBase.LotSerialState();
				yield return new WMSBase.ExpireDateState() { IsForIssue = true };
				yield return new ToteSupport.SelectToteState();
				yield return new ConfirmToteState();
				yield return new ConfirmState();

				// directly set state
				yield return new ShipmentToteState();
			}

			protected override IEnumerable<ScanTransition<PickPackShip>> CreateTransitions()
			{
				return StateFlow(flow => flow
					.ForkBy(b => b.Remove != true)
					.PositiveBranch(directFlow => directFlow
						.From<PickListState>()
						.NextTo<ToteSupport.AssignToteState>()
						.NextTo<WMSBase.LocationState>()
						.NextTo<WMSBase.InventoryItemState>()
						.NextTo<WMSBase.LotSerialState>()
						.NextTo<WMSBase.ExpireDateState>()
						.NextTo<ToteSupport.SelectToteState>()
						.NextTo<ConfirmToteState>())
					.NegativeBranch(removeFlow => removeFlow
						.From<PickListState>()
						.NextTo<WMSBase.LocationState>()
						.NextTo<WMSBase.InventoryItemState>()
						.NextTo<WMSBase.LotSerialState>()));
			}

			protected override IEnumerable<ScanCommand<PickPackShip>> CreateCommands()
			{
				yield return new WMSBase.RemoveCommand();
				yield return new WMSBase.QtySupport.SetQtyCommand();
				yield return new WorksheetPicking.ConfirmPickListCommand();
				yield return new ToteSupport.AddToteCommand();
			}

			protected override IEnumerable<ScanRedirect<PickPackShip>> CreateRedirects() => AllWMSRedirects.CreateFor<PickPackShip>();

			protected override void ResetMode(bool fullReset)
			{
				base.ResetMode(fullReset);
				Clear<PickListState>(when: fullReset && !Basis.IsWithinReset);
				Clear<WMSBase.LocationState>(when: fullReset || Basis.PromptLocationForEveryLine);
				Clear<WMSBase.InventoryItemState>(when: fullReset);
				Clear<WMSBase.LotSerialState>();
				Clear<WMSBase.ExpireDateState>();
				Clear<ToteSupport.SelectToteState>();
				Clear<ToteSupport.AssignToteState>();
				Clear<ShipmentToteState>();
			}
			#endregion

			#region States
			public sealed class PickListState : WorksheetPicking.PickListState
			{
				protected override string WorksheetType => SOPickingWorksheet.worksheetType.Wave;
			}

			public sealed class ConfirmToteState : ToteSupport.ToteState
			{
				public const string Value = "CNFT";
				public class value : BqlString.Constant<value> { public value() : base(ConfirmToteState.Value) { } }

				public override string Code => Value;
				protected override string StatePrompt => Basis.Localize(Msg.Prompt, ToteBasis.GetProperTote()?.ToteCD);

				protected override bool IsStateActive() => Basis.Get<Logic>().ConfirmToteForEveryLine;
				protected override bool IsStateSkippable() => ToteBasis.GetProperTote() == null;

				protected override Validation Validate(INTote tote)
				{
					if (Basis.HasFault(tote, base.Validate, out var fault))
						return fault;

					if (ToteBasis.GetProperTote().ToteID != tote.ToteID)
						return Validation.Fail(Msg.Mismatch, tote.ToteCD);

					return Validation.Ok;
				}

				public class Logic : WorksheetPicking.ScanExtension
				{
					public static bool IsActive() => IsActiveBase();

					public virtual bool ConfirmToteForEveryLine =>
						Basis.Setup.Current.ConfirmToteForEachItem == true &&
						Basis.Remove == false &&
						WSBasis.WorksheetNbr != null &&
						WSBasis.Worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Wave &&
						WSBasis.ShipmentsOfPicker.Select().Count > 1;
				}

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : ToteSupport.ToteState.Msg
				{
					public const string Prompt = "Scan the barcode of the {0} tote to confirm picking of the items.";
					public const string Mismatch = "Incorrect tote barcode ({0}) has been scanned.";
				}
				#endregion
			}

			public sealed class ShipmentToteState : ToteSupport.ToteState
			{
				public const string Value = "SHTO";
				public class value : BqlString.Constant<value> { public value() : base(ShipmentToteState.Value) { } }

				public override string Code => Value;
				protected override string StatePrompt => Basis.Localize(Msg.Prompt);

				protected override bool IsStateActive() => base.IsStateActive() && WSBasis.ShipmentsOfPicker.SelectMain().Distinct(link => link.ShipmentNbr).Count() > 1;
				protected override bool IsStateSkippable() => Basis.Get<AlterToteSupport>().ToteShipmentNbr != null;

				protected override Validation Validate(INTote tote)
				{
					if (Basis.HasFault(tote, base.Validate, out var fault))
						return fault;

					SOPickerToShipmentLink selectedToteLink = WSBasis.ShipmentsOfPicker.Search<SOPickerToShipmentLink.toteID>(tote.ToteID);
					if (selectedToteLink == null)
						return Validation.Fail(ToteSupport.SelectToteState.Msg.MissingAssigned,
							tote.ToteCD,
							WSBasis.ShipmentsOfPicker.SelectMain()
								.Select(link => link.ToteID)
								.Select(tid => INTote.PK.Find(Basis, Basis.SiteID, tid).ToteCD)
								.With(tcds => string.Join(", ", tcds)));

					var emptyTote = WSBasis.PickListOfPicker
						.SearchAll<Asc<SOPickerListEntry.shipmentNbr>>(new object[] { selectedToteLink.ShipmentNbr })
						.RowCast<SOPickerListEntry>()
						.GroupBy(s => s.ToteID)
						.FirstOrDefault(tote => tote.All(s => s.PickedQty == 0));
					if (emptyTote != null)
						return Validation.Fail(Msg.ThereIsNotStartedTote, selectedToteLink.ShipmentNbr, INTote.PK.Find(Basis, selectedToteLink.SiteID, emptyTote.Key).ToteCD);

					return Validation.Ok;
				}

				protected override void Apply(INTote tote) => Basis.Get<AlterToteSupport>().ToteShipmentNbr = ((SOPickerToShipmentLink)WSBasis.ShipmentsOfPicker.Search<SOPickerToShipmentLink.toteID>(tote.ToteID)).ShipmentNbr;
				protected override void ClearState() => Basis.Get<AlterToteSupport>().ToteShipmentNbr = null;

				protected override void SetNextState() => Basis.SetScanState<ToteSupport.AssignToteState>();

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : ToteSupport.ToteState.Msg
				{
					public const string Prompt = "Scan the barcode of the tote to which you want to add another one.";
					public const string ThereIsNotStartedTote = "The {0} shipment has an empty tote. Use the {1} tote instead of adding a new one.";
				}
				#endregion
			}
			#endregion

			/// Overrides <see cref="ToteSupport"/>
			public class AlterToteSupport : WorksheetPicking.ScanExtension<ToteSupport>
			{
				public static bool IsActive() => IsActiveBase();

				protected virtual bool IsSuppressed => false;

				#region Overrides
				/// Overrides <see cref="ToteSupport.CanAddNewTote"/>
				[PXOverride]
				public bool get_CanAddNewTote(
					Func<bool> base_CanAddNewTote)
				{
					if (Basis.CurrentMode is WavePickMode && !IsSuppressed)
						return Target.AllowMultipleTotesPerShipment && WSBasis.CanWSPick && !Target.HasAnotherToAssign;
					return
						base_CanAddNewTote();
				}

				/// Overrides <see cref="ToteSupport.GetShipmentToAddToteTo"/>
				[PXOverride]
				public string GetShipmentToAddToteTo(
					Func<string> base_GetShipmentToAddToteTo)
				{
					if (Basis.CurrentMode is WavePickMode && !IsSuppressed)
						return ToteShipmentNbr ?? WSBasis.ShipmentsOfPicker.Select().TopFirst?.ShipmentNbr;
					else
						return base_GetShipmentToAddToteTo();
				}

				/// Overrides <see cref="ToteSupport.IsToteSelectionNeeded"/>
				[PXOverride]
				public bool get_IsToteSelectionNeeded(
					Func<bool> base_IsToteSelectionNeeded)
				{
					if (Basis.CurrentMode is WavePickMode && !IsSuppressed)
						return true;
					else
						return base_IsToteSelectionNeeded();
				}

				/// Overrides <see cref="WorksheetPicking.GetSelectedPickListEntry"/>
				[PXOverride]
				public SOPickerListEntry GetSelectedPickListEntry(
					Func<SOPickerListEntry> base_GetSelectedPickListEntry)
				{
					var split = base_GetSelectedPickListEntry();

					if (Basis.CurrentMode is WavePickMode && !IsSuppressed && split == null && Target.ToteID != null)
					{
						int? originalToteID = Target.ToteID;
						try
						{
							Target.ToteID = null;

							var similarSplit = base_GetSelectedPickListEntry();
							if (similarSplit != null)
								return similarSplit;
						}
						finally
						{
							Target.ToteID = originalToteID;
						}
					}

					return split;
				}

				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
				[PXOverride]
				public ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original,
					Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
				{
					var state = base_DecorateScanState(original);

					if (state is ToteSupport.AssignToteState assignTote && assignTote.ModeCode == WavePickMode.Value && !IsSuppressed)
						InjectAssignToteTakeOverToShipmentTote(assignTote);

					return state;
				}

				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanCommand"/>
				[PXOverride]
				public ScanCommand<PickPackShip> DecorateScanCommand(ScanCommand<PickPackShip> original,
					Func<ScanCommand<PickPackShip>, ScanCommand<PickPackShip>> base_DecorateScanCommand)
				{
					var command = base_DecorateScanCommand(original);

					if (command is WMSBase.RemoveCommand remove && remove.ModeCode == WavePickMode.Value)
					{
						Target.InjectRemoveDisableWhenAssignTote(remove);
						Target.InjectRemoveMovesToRemoveFromTote(remove);
					}

					return command;
				}
				#endregion

				public virtual void InjectAssignToteTakeOverToShipmentTote(ToteSupport.AssignToteState assignTote)
				{
					assignTote.Intercept.OnTakingOver.ByOverride((basis, base_OnTakingOver) =>
					{
						if (basis.FindState<ShipmentToteState>() is ShipmentToteState st && st.IsActive && !st.IsSkippable && Target.AddNewTote == true)
							basis.SetScanState<ShipmentToteState>();
					});
				}

				#region State
				public ShipmentToteScanHeader ShipmentToteHeader => Basis.Header.Get<ShipmentToteScanHeader>() ?? new ShipmentToteScanHeader();
				public ValueSetter<ScanHeader>.Ext<ShipmentToteScanHeader> ShipmentToteSetter => Basis.HeaderSetter.With<ShipmentToteScanHeader>();

				#region ToteShipmentNbr
				public string ToteShipmentNbr
				{
					get => ShipmentToteHeader.ToteShipmentNbr;
					set => ShipmentToteSetter.Set(h => h.ToteShipmentNbr, value);
				}
				#endregion
				#endregion

				public sealed class ShipmentToteScanHeader : PXCacheExtension<WorksheetScanHeader, WMSScanHeader, QtyScanHeader, ScanHeader>
				{
					public static bool IsActive() => AlterToteSupport.IsActive();

					#region ToteShipmentNbr
					[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
					[PXSelector(typeof(SOShipment.shipmentNbr))]
					public string ToteShipmentNbr { get; set; }
					public abstract class toteShipmentNbr : BqlString.Field<toteShipmentNbr> { }
					#endregion
				}
			}

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : PickPackShip.ScanMode.Msg
			{
				public const string DisplayName = "Wave Pick";
			}
			#endregion
		}

		public sealed class BatchPickMode : PickPackShip.ScanMode
		{
			public const string Value = "BTCH";
			public class value : BqlString.Constant<value> { public value() : base(BatchPickMode.Value) { } }

			public WaveBatchPicking WBBasis => Get<WaveBatchPicking>();

			public override string Code => Value;
			public override string Description => Msg.DisplayName;

			protected override bool IsModeActive() => Basis.HasPick;

			#region State Machine
			protected override IEnumerable<ScanState<PickPackShip>> CreateStates()
			{
				yield return new PickListState();
				yield return new WMSBase.LocationState();
				yield return new WMSBase.InventoryItemState() { AlternateType = INPrimaryAlternateType.CPN, IsForIssue = true };
				yield return new WMSBase.LotSerialState();
				yield return new WMSBase.ExpireDateState() { IsForIssue = true };
				yield return new ConfirmState();

				if (Get<PPSCartSupport>() is PPSCartSupport cartSupport && cartSupport.IsCartRequired())
				{
					yield return new PPSCartSupport.CartState()
						.Intercept.Apply.ByAppend((basis, cart) =>
						{
							var wsBasis = basis.Get<WorksheetPicking>();
							if (wsBasis.Picker.Current != null)
							{
								wsBasis.Picker.Current.CartID = cart.CartID;
								wsBasis.Picker.UpdateCurrent();
							}
						});
				}

				// directly set state
				yield return new SortingLocationState();
			}

			protected override IEnumerable<ScanTransition<PickPackShip>> CreateTransitions()
			{
				var cartSupport = Get<PPSCartSupport>();
				if (cartSupport != null && cartSupport.IsCartRequired())
				{ // With Cart
					return StateFlow(flow => flow
						.From<PickListState>()
						.NextTo<PPSCartSupport.CartState>()
						.NextTo<PickPackShip.LocationState>()
						.NextTo<PickPackShip.InventoryItemState>()
						.NextTo<PickPackShip.LotSerialState>()
						.NextTo<PickPackShip.ExpireDateState>());
				}
				else
				{ // No Cart
					return StateFlow(flow => flow
						.From<PickListState>()
						.NextTo<PickPackShip.LocationState>()
						.NextTo<PickPackShip.InventoryItemState>()
						.NextTo<PickPackShip.LotSerialState>()
						.NextTo<PickPackShip.ExpireDateState>());
				}
			}

			protected override IEnumerable<ScanCommand<PickPackShip>> CreateCommands()
			{
				yield return new PickPackShip.RemoveCommand();
				yield return new PickPackShip.QtySupport.SetQtyCommand();
				yield return new WorksheetPicking.ConfirmPickListCommand();
			}

			protected override IEnumerable<ScanRedirect<PickPackShip>> CreateRedirects() => AllWMSRedirects.CreateFor<PickPackShip>();

			protected override void ResetMode(bool fullReset)
			{
				base.ResetMode(fullReset);
				Clear<PickListState>(when: fullReset && !Basis.IsWithinReset);
				Clear<PPSCartSupport.CartState>(when: fullReset && !Basis.IsWithinReset);
				Clear<PickPackShip.LocationState>(when: fullReset || Basis.PromptLocationForEveryLine);
				Clear<PickPackShip.InventoryItemState>(when: fullReset);
				Clear<PickPackShip.LotSerialState>();
				Clear<PickPackShip.ExpireDateState>();
			}
			#endregion

			#region States
			public sealed class PickListState : WorksheetPicking.PickListState
			{
				protected override string WorksheetType => SOPickingWorksheet.worksheetType.Batch;
			}

			public sealed class SortingLocationState : PickPackShip.EntityState<INLocation>
			{
				public const string Value = "SLOC";
				public class value : BqlString.Constant<value> { public value() : base(SortingLocationState.Value) { } }

				public override string Code => Value;
				protected override string StatePrompt => Msg.Prompt;

				protected override INLocation GetByBarcode(string barcode)
				{
					return
						SelectFrom<INLocation>.
						Where<
							INLocation.siteID.IsEqual<@P.AsInt>.
							And<INLocation.locationCD.IsEqual<@P.AsString>>>.
						View.Select(Basis, Basis.SiteID, barcode);
				}

				protected override Validation Validate(INLocation location)
				{
					if (location.Active != true)
						return Validation.Fail(IN.Messages.InactiveLocation, location.LocationCD);

					if (location.IsSorting != true)
						return Validation.Fail(Msg.NotSorting, location.LocationCD);

					return Validation.Ok;
				}

				protected override void Apply(INLocation location) => Basis.Get<WorksheetPicking.ConfirmPickListCommand.Logic>().ConfirmPickList(location.LocationID.Value);

				protected override void ReportSuccess(INLocation location) => Basis.Reporter.Info(Msg.Ready, location.LocationCD);
				protected override void ReportMissing(string barcode) => Basis.Reporter.Error(Msg.Missing, barcode, Basis.SightOf<WMSScanHeader.siteID>());

				protected override void SetNextState() { }

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string Prompt = "Scan the sorting location.";
					public const string Ready = "The {0} sorting location is selected.";
					public const string Missing = PickPackShip.LocationState.Msg.Missing;
					public const string NotSorting = "The {0} location cannot be selected because it is not a sorting location.";
				}
				#endregion
			}
			#endregion

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : PickPackShip.ScanMode.Msg
			{
				public const string DisplayName = "Batch Pick";
			}
			#endregion
		}

		#region Event Handlers
		protected virtual void _(Events.RowSelected<ScanHeader> e)
		{
			if (e.Row == null)
				return;

			WSBasis.PickListOfPicker.Cache.AdjustUI()
				.For<SOPickerListEntry.shipmentNbr>(a => a.Visible = WSBasis.Worksheet.Current?.WorksheetType == SOPickingWorksheet.worksheetType.Wave);
		}
		#endregion

		#region States
		public sealed class ConfirmState : PickPackShip.ConfirmationState
		{
			public override string Prompt => Basis.Localize(PickPackShip.PickMode.ConfirmState.Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);
			protected override FlowStatus PerformConfirmation() => Basis.Get<Logic>().Confirm();

			#region Logic
			public class Logic : ScanExtension
			{
				public static bool IsActive() => IsActiveBase();

				public virtual FlowStatus Confirm()
				{
					SOPickerListEntry pickedSplit = WSBasis.GetSelectedPickListEntry();

					var confirmResult = WSBasis.ConfirmSplit(pickedSplit, Sign.MinusIf(Basis.Remove == true) * Basis.BaseQty);
					if (confirmResult.IsError != false)
						return confirmResult;

					WSBasis.ReportSplitConfirmed(pickedSplit);

					return FlowStatus.Ok.WithDispatchNext;
				}
			}
			#endregion
		}
		#endregion

		#region Decoration
		public virtual void InjectPackLocationDeactivatedBasedOnShipmentSpecialPickType(WMSBase.LocationState locationState)
		{
			locationState.Intercept.IsStateActive.ByConjoin(basis =>
			{
				switch (basis.Get<WorksheetPicking>().ShipmentSpecialPickType)
				{
					case SOPickingWorksheet.worksheetType.Batch: return true;
					case SOPickingWorksheet.worksheetType.Wave: return false;
					case null: return true;
					default: throw new ArgumentOutOfRangeException();
				}
			});
		}
		#endregion

		#region Overrides
		/// Overrides <see cref="WorksheetPicking.IsWorksheetMode(string)"/>
		[PXOverride]
		public virtual bool IsWorksheetMode(string modeCode, Func<string, bool> base_IsWorksheetMode)
			=> base_IsWorksheetMode(modeCode) || modeCode.IsIn(WavePickMode.Value, BatchPickMode.Value);

		/// Overrides <see cref="WorksheetPicking.FindModeForWorksheet(SOPickingWorksheet)"/>
		[PXOverride]
		public virtual ScanMode<PickPackShip> FindModeForWorksheet(SOPickingWorksheet sheet, Func<SOPickingWorksheet, ScanMode<PickPackShip>> base_FindModeForWorksheet)
		{
			if (sheet.WorksheetType == SOPickingWorksheet.worksheetType.Wave)
				return Basis.FindMode<WavePickMode>();
			
			if (sheet.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
				return Basis.FindMode<BatchPickMode>();

			return base_FindModeForWorksheet(sheet);
		}

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
		[PXOverride]
		public virtual ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original, Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
		{
			var state = base_DecorateScanState(original);

			if (MatchMode(state.ModeCode))
			{
				if (state is WMSBase.LocationState locationState)
				{
					Basis.InjectLocationDeactivationOnDefaultLocationOption(locationState);
					Basis.InjectLocationSkippingOnPromptLocationForEveryLineOption(locationState);
				}
				else if (state is WMSBase.InventoryItemState itemState)
				{
					Basis.InjectItemAbsenceHandlingByLocation(itemState);
				}
			}
			else if (state.ModeCode == PickPackShip.PackMode.Value)
			{
				if (state is WMSBase.LocationState locationState)
				{
					InjectPackLocationDeactivatedBasedOnShipmentSpecialPickType(locationState);
				}
			}

			return state;
		}

		/// Overrides <see cref="WorksheetPicking.ConfirmPickListCommand.Logic"/>
		public class AlterConfirmPickListCommandLogic : WorksheetPicking.ScanExtension<WorksheetPicking.ConfirmPickListCommand.Logic>
		{
			public static bool IsActive() => WaveBatchPicking.IsActive();

			/// Overrides <see cref="WorksheetPicking.ConfirmPickListCommand.Logic.ConfirmPickList(int?)"/>
			[PXOverride]
			public virtual void ConfirmPickList(int? sortingLocationID, Action<int?> base_ConfirmPickList)
			{
				if (sortingLocationID == null && Base2.Worksheet.Current.WorksheetType == SOPickingWorksheet.worksheetType.Batch)
				{
					Base1.SetScanState<BatchPickMode.SortingLocationState>();
					return;
				}

				base_ConfirmPickList(sortingLocationID);
			}

			/// Overrides <see cref="WorksheetPicking.ConfirmPickListCommand.Logic.ReportConfirmationInPart"/>
			[PXOverride]
			public void ReportConfirmationInPart(Action base_ReportConfirmationInPart)
			{
				if (Basis.CurrentMode is BatchPickMode)
					Basis.ReportError(Msg.BatchCannotBeConfirmedInPart);
				else
					base_ReportConfirmationInPart();
			}

			[PXLocalizable]
			public abstract class Msg
			{
				public const string BatchCannotBeConfirmedInPart = "The batch pick list cannot be confirmed because it is not complete.";
			}
		}

		/// Overrides <see cref="PickPackShip.PackMode.ConfirmState.Logic"/>
		public class AlterPackConfirmLogic : PickPackShip.ScanExtension<PickPackShip.PackMode.ConfirmState.Logic>
		{
			public static bool IsActive() => WaveBatchPicking.IsActive();

			protected WorksheetPicking WSBasis => Basis.Get<WorksheetPicking>();

			/// Overrides <see cref="PickPackShip.PackMode.ConfirmState.Logic.TargetQty(SOShipLineSplit)"/>
			[PXOverride]
			public virtual decimal? TargetQty(SOShipLineSplit split, Func<SOShipLineSplit, decimal?> base_TargetQty)
			{
				switch (WSBasis.ShipmentSpecialPickType)
				{
					case SOPickingWorksheet.worksheetType.Batch: return split.PickedQty * Graph.GetQtyThreshold(split);
					case SOPickingWorksheet.worksheetType.Wave: return split.PickedQty;
					default: return base_TargetQty(split);
				}
			}
		}

		/// Overrides <see cref="PPSCartSupport"/>
		public class AlterCartSupport : PickPackShip.ScanExtension<PPSCartSupport>
		{
			public static bool IsActive() => WaveBatchPicking.IsActive() && PPSCartSupport.IsActive();

			/// Overrides <see cref="PPSCartSupport.IsCartRequired()"/>
			[PXOverride]
			public virtual bool IsCartRequired(Func<bool> base_IsCartRequired)
			{
				return base_IsCartRequired() ||
					Basis.Setup.Current.UseCartsForPick == true &&
					Basis.Header.Mode == BatchPickMode.Value;
			}
		}
		#endregion

		#region Extensibility
		public abstract class ScanExtension : PXGraphExtension<WaveBatchPicking, WorksheetPicking, PickPackShip, PickPackShip.Host>
		{
			protected static bool IsActiveBase() => WaveBatchPicking.IsActive();

			public PickPackShip.Host Graph => Base;
			public PickPackShip Basis => Base1;
			public WorksheetPicking WSBasis => Base2;
			public WaveBatchPicking WBBasis => Base3;
		}

		public abstract class ScanExtension<TTargetExtension> : PXGraphExtension<TTargetExtension, WaveBatchPicking, WorksheetPicking, PickPackShip, PickPackShip.Host>
			where TTargetExtension : PXGraphExtension<WaveBatchPicking, WorksheetPicking, PickPackShip, PickPackShip.Host>
		{
			protected static bool IsActiveBase() => WaveBatchPicking.IsActive();

			public PickPackShip.Host Graph => Base;
			public PickPackShip Basis => Base1;
			public WorksheetPicking WSBasis => Base2;
			public WaveBatchPicking WBBasis => Base3;
			public TTargetExtension Target => Base4;
		}
		#endregion
	}
}
