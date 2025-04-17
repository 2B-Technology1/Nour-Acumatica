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

using PX.Common;
using PX.Data;
using PX.BarcodeProcessing;

using PX.Objects.Common;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	using WMSBase = WarehouseManagementSystem<PickPackShip, PickPackShip.Host>;

	public class PaperlessWaveBatchPicking : WaveBatchPicking.ScanExtension
	{
		public static bool IsActive() => IsActiveBase() && PaperlessPicking.IsActive();

		#region Decoration
		public virtual void InjectPaperlessWaveMode(WaveBatchPicking.WavePickMode wavePick)
		{
			wavePick
				.Intercept.CreateCommands.ByAppend(basis => new PickPackShip.ScanCommand[]
				{
					new PaperlessPicking.TakeNextPickListCommand(),
					new PaperlessPicking.ConfirmPickListAndTakeNextCommand(),
					new PaperlessPicking.ConfirmLineQtyCommand()
				})
				.Intercept.CreateStates.ByAppend(basis => new ScanState<PickPackShip>[]
				{
					new PaperlessPicking.WarehouseState(),
					new PaperlessPicking.NearestLocationState()
				})
				.Intercept.CreateTransitions.ByReplace(basis =>
				{
					return basis.StateFlow(flow => flow
						.ForkBy(b => b.Remove != true)
						.PositiveBranch(directFlow => directFlow
							.From<WaveBatchPicking.WavePickMode.PickListState>()
							.NextTo<ToteSupport.AssignToteState>()
							.NextTo<WMSBase.LocationState>()
							.NextTo<WMSBase.InventoryItemState>()
							.NextTo<WMSBase.LotSerialState>()
							.NextTo<WMSBase.ExpireDateState>()
							.NextTo<ToteSupport.SelectToteState>()
							.NextTo<WaveBatchPicking.WavePickMode.ConfirmToteState>())
						.NegativeBranch(removeFlow => removeFlow
							.From<WaveBatchPicking.WavePickMode.PickListState>()
							.NextTo<WMSBase.InventoryItemState>()
							.NextTo<WMSBase.LotSerialState>()
							.NextTo<WMSBase.LocationState>()));
				}, RelativeInject.CloserToBase)
				.Intercept.ResetMode.ByReplace((basis, fullReset) =>
				{
					basis.Clear<WaveBatchPicking.WavePickMode.PickListState>(when: fullReset && !basis.IsWithinReset);
					basis.Clear<WMSBase.LocationState>(when: fullReset);
					basis.Clear<WMSBase.InventoryItemState>(when: fullReset);
					basis.Clear<WMSBase.LotSerialState>();
					basis.Clear<WMSBase.ExpireDateState>();
					basis.Clear<ToteSupport.AssignToteState>();
					basis.Clear<ToteSupport.SelectToteState>();
				}, RelativeInject.CloserToBase);
		}

		public virtual void InjectPaperlessBatchMode(WaveBatchPicking.BatchPickMode batchPick)
		{
			batchPick
				.Intercept.CreateCommands.ByAppend(basis => new PickPackShip.ScanCommand[]
				{
					new PaperlessPicking.TakeNextPickListCommand(),
					new PaperlessPicking.ConfirmPickListAndTakeNextCommand(),
					new PaperlessPicking.ConfirmLineQtyCommand()
				})
				.Intercept.CreateStates.ByAppend(basis => new ScanState<PickPackShip>[]
				{
					new PaperlessPicking.WarehouseState(),
					new PaperlessPicking.NearestLocationState()
				})
				.Intercept.CreateTransitions.ByReplace(basis =>
				{
					return basis.StateFlow(flow => flow
						.ForkBy(b => b.Remove != true)
						.PositiveBranch(directFlow => directFlow
							.ForkBy(b => b.Get<PPSCartSupport>().With(cs => cs.IsCartRequired()))
							.PositiveBranch(directCartFlow => directCartFlow
								.From<WaveBatchPicking.BatchPickMode.PickListState>()
								.NextTo<PPSCartSupport.CartState>()
								.NextTo<WMSBase.LocationState>()
								.NextTo<WMSBase.InventoryItemState>()
								.NextTo<WMSBase.LotSerialState>()
								.NextTo<WMSBase.ExpireDateState>())
							.NegativeBranch(directNoCartFlow => directNoCartFlow
								.From<WaveBatchPicking.BatchPickMode.PickListState>()
								.NextTo<WMSBase.LocationState>()
								.NextTo<WMSBase.InventoryItemState>()
								.NextTo<WMSBase.LotSerialState>()
								.NextTo<WMSBase.ExpireDateState>()))
						.NegativeBranch(removeFlow => removeFlow
							.From<WaveBatchPicking.BatchPickMode.PickListState>()
							.NextTo<WMSBase.InventoryItemState>()
							.NextTo<WMSBase.LotSerialState>()
							.NextTo<WMSBase.LocationState>()));
				}, RelativeInject.CloserToBase)
				.Intercept.ResetMode.ByReplace((basis, fullReset) =>
				{
					basis.Clear<WaveBatchPicking.BatchPickMode.PickListState>(when: fullReset && !basis.IsWithinReset);
					basis.Clear<PPSCartSupport.CartState>(when: fullReset && !basis.IsWithinReset);
					basis.Clear<PickPackShip.LocationState>(when: fullReset);
					basis.Clear<PickPackShip.InventoryItemState>(when: fullReset);
					basis.Clear<PickPackShip.LotSerialState>();
					basis.Clear<PickPackShip.ExpireDateState>();
				}, RelativeInject.CloserToBase);
		}
		#endregion

		#region Overrides
		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanMode"/>
		[PXOverride]
		public virtual ScanMode<PickPackShip> DecorateScanMode(ScanMode<PickPackShip> original, Func<ScanMode<PickPackShip>, ScanMode<PickPackShip>> base_DecorateScanMode)
		{
			var mode = base_DecorateScanMode(original);

			if (mode is WaveBatchPicking.WavePickMode wavePick)
				InjectPaperlessWaveMode(wavePick);
			else if (mode is WaveBatchPicking.BatchPickMode batchPick)
				InjectPaperlessBatchMode(batchPick);

			return mode;
		}

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
		[PXOverride]
		public virtual ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original, Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
		{
			var ppBasis = Basis.Get<PaperlessPicking>();
			var state = base_DecorateScanState(original);

			if (WaveBatchPicking.MatchMode(state.ModeCode))
			{
				if (state is WorksheetPicking.PickListState pickList)
					ppBasis.InjectPickListPaperless(pickList);
				else if (state is WMSBase.LocationState locState)
					ppBasis.InjectNavigationOnLocation(locState);
				else if (state is WMSBase.InventoryItemState itemState)
					ppBasis.InjectNavigationOnItem(itemState);
				else if (state is WMSBase.LotSerialState lsState)
					ppBasis.InjectNavigationOnLotSerial(lsState);
			}

			return state;
		}

		/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanCommand"/>
		[PXOverride]
		public virtual ScanCommand<PickPackShip> DecorateScanCommand(ScanCommand<PickPackShip> original, Func<ScanCommand<PickPackShip>, ScanCommand<PickPackShip>> base_DecorateScanCommand)
		{
			var ppBasis = Basis.Get<PaperlessPicking>();
			var command = base_DecorateScanCommand(original);

			if (WaveBatchPicking.MatchMode(command.ModeCode))
			{
				if (command is WMSBase.RemoveCommand remove)
					ppBasis.InjectRemoveClearLocationAndInventory(remove);
				else if (command is WorksheetPicking.ConfirmPickListCommand confirm)
					ppBasis.InjectConfirmPickListSuppressionOnCanPick(confirm);
			}

			return command;
		}

		/// Overrides <see cref="PickPackShip.InjectLocationSkippingOnPromptLocationForEveryLineOption"/>
		[PXOverride]
		public virtual void InjectLocationSkippingOnPromptLocationForEveryLineOption(WMSBase.LocationState locationState, Action<WMSBase.LocationState> base_InjectLocationSkippingOnPromptLocationForEveryLineOption)
		{
			if (WaveBatchPicking.MatchMode(locationState.ModeCode))
			{
				/// suppress changes applied in <see cref="WaveBatchPicking.DecorateScanState(ScanState{PickPackShip}, Func{ScanState{PickPackShip}, ScanState{PickPackShip}})"/>
			}
			else
			{
				base_InjectLocationSkippingOnPromptLocationForEveryLineOption(locationState);
			}
		}

		/// Overrides <see cref="PickPackShip.InjectItemAbsenceHandlingByLocation"/>
		[PXOverride]
		public virtual void InjectItemAbsenceHandlingByLocation(WMSBase.InventoryItemState inventoryState, Action<WMSBase.InventoryItemState> base_InjectItemAbsenceHandlingByLocation)
		{
			if (WaveBatchPicking.MatchMode(inventoryState.ModeCode))
			{
				/// suppress changes applied in <see cref="WaveBatchPicking.DecorateScanState(ScanState{PickPackShip}, Func{ScanState{PickPackShip}, ScanState{PickPackShip}})"/>
			}
			else
			{
				base_InjectItemAbsenceHandlingByLocation(inventoryState);
			}
		}

		/// Overrides <see cref="WorksheetPicking.GetSelectedPickListEntry()"/>
		[PXOverride]
		public SOPickerListEntry GetSelectedPickListEntry(
			Func<SOPickerListEntry> base_GetSelectedPickListEntry)
		{
			if (WaveBatchPicking.MatchMode(Basis.CurrentMode.Code))
			{
				var ppBasis = Basis.Get<PaperlessPicking>();
				return Basis.Remove == true
					? ppBasis.GetSplitForRemoval()
					: ppBasis.GetWantedSplitForIncrease();
			}
			else
				return base_GetSelectedPickListEntry();
		}

		/// Overrides <see cref="WaveBatchPicking.ConfirmState.Logic"/>
		public class AlterWaveBatchPickingConfirmStateLogic : WaveBatchPicking.ScanExtension<WaveBatchPicking.ConfirmState.Logic>
		{
			public static bool IsActive() => PaperlessWaveBatchPicking.IsActive();

			/// Overrides <see cref="WaveBatchPicking.ConfirmState.Logic.Confirm"/>
			[PXOverride]
			public FlowStatus Confirm(Func<FlowStatus> baseIgnored)
			{
				Basis.Get<PaperlessPicking>().EnsureLocationFromLastVisited();

				SOPickerListEntry pickedSplit = WSBasis.GetSelectedPickListEntry();

				var confirmResult = WSBasis.ConfirmSplit(pickedSplit, Sign.MinusIf(Basis.Remove == true) * Basis.BaseQty);
				if (confirmResult.IsError != false)
					return confirmResult;

				WSBasis.ReportSplitConfirmed(pickedSplit);

				Basis.Get<PaperlessPicking.ConfirmState.Logic>().VisitSplit(pickedSplit);

				return FlowStatus.Ok.WithDispatchNext;
			}
		}

		/// Overrides <see cref="WorksheetPicking.ConfirmPickListCommand.Logic"/>
		public class AlterConfirmPickListCommandLogic : WorksheetPicking.ScanExtension<WorksheetPicking.ConfirmPickListCommand.Logic>
		{
			public static bool IsActive() => PaperlessWaveBatchPicking.IsActive();

			/// Overrides <see cref="WorksheetPicking.ConfirmPickListCommand.Logic.ConfigureOnSuccessAction(ScanLongRunAwaiter{PickPackShip, SOPicker}.IResultProcessor)"/>
			[PXOverride]
			public virtual void ConfigureOnSuccessAction(ScanLongRunAwaiter<PickPackShip, SOPicker>.IResultProcessor onSuccess, Action<ScanLongRunAwaiter<PickPackShip, SOPicker>.IResultProcessor> base_ConfigureOnSuccessAction)
			{
				base_ConfigureOnSuccessAction(onSuccess);
				if (Base1.CurrentMode is WaveBatchPicking.BatchPickMode)
				{
					var confirmAndNextCommand = Base1.CurrentMode.Commands.OfType<PaperlessPicking.ConfirmPickListAndTakeNextCommand>().FirstOrDefault();
					if (confirmAndNextCommand != null)
					{
						var preSortLocationScan = Base1.Logs.Select().RowCast<ScanLog>().Select(log => log.HeaderStateBefore).SkipWhile(h => h.InitialScanState == WaveBatchPicking.BatchPickMode.SortingLocationState.Value).FirstOrDefault();
						if (preSortLocationScan != null && preSortLocationScan.Barcode.Substring(1) == confirmAndNextCommand.Code)
							onSuccess.Do((basis, picker) => basis.CurrentMode.Commands.OfType<PaperlessPicking.TakeNextPickListCommand>().First().Execute());
					}
				}
			}
		}

		/// Overrides <see cref="ToteSupport"/>
		public class AlterToteSupport : WorksheetPicking.ScanExtension<ToteSupport>
		{
			public static bool IsActive() => PaperlessWaveBatchPicking.IsActive();

			#region Overrides
			/// Overrides <see cref="ToteSupport.NoEmptyTotes"/>
			[PXOverride]
			public bool get_NoEmptyTotes(
				Func<bool> base_NoEmptyTotes)
			{
				if (Basis.CurrentMode is WaveBatchPicking.WavePickMode)
				{
					var ppBasis = Basis.Get<PaperlessPicking>();
					if (ppBasis.GetWantedSplit() is SOPickerListEntry split && split.ToteID != INTote.UnassignedToteID)
					{
						return WSBasis.PickListOfPicker
							.SearchAll<Asc<SOPickerListEntry.toteID>>(new object[] { split.ToteID })
							.RowCast<SOPickerListEntry>()
							.Any(s => s.PickedQty > 0 || s.ForceCompleted == true);
					}
				}

				return base_NoEmptyTotes();
			}

			/// Overrides <see cref="ToteSupport.GetShipmentToAddToteTo"/>
			[PXOverride]
			public string GetShipmentToAddToteTo(
				Func<string> base_GetShipmentToAddToteTo)
			{
				if (Basis.CurrentMode is WaveBatchPicking.WavePickMode)
					return Basis.Get<PaperlessPicking>().GetWantedSplit()?.ShipmentNbr;
				else
					return base_GetShipmentToAddToteTo();
			}

			/// Overrides <see cref="ToteSupport.IsToteSelectionNeeded"/>
			[PXOverride]
			public bool get_IsToteSelectionNeeded(
				Func<bool> base_IsToteSelectionNeeded)
			{
				if (Basis.CurrentMode is WaveBatchPicking.WavePickMode)
					return WSBasis.PickListOfPicker.SelectMain().Where(s => s.PickedQty > 0 && s.ForceCompleted != true).GroupBy(s => s.ToteID).Count() > 1;
				else
					return base_IsToteSelectionNeeded();
			}

			/// Overrides <see cref="ToteSupport.MoveSplitRestQtyToAnotherTote(SOPickerListEntry, int?)"/>
			[PXOverride]
			public SOPickerListEntry MoveSplitRestQtyToAnotherTote(SOPickerListEntry split, int? toteID,
				Func<SOPickerListEntry, int?, SOPickerListEntry> base_MoveSplitRestQtyToAnotherTote)
			{
				split = base_MoveSplitRestQtyToAnotherTote(split, toteID);

				if (Basis.CurrentMode is WaveBatchPicking.WavePickMode)
				{
					// set of lines might be changed by the original method, so we need to get the wanted one once again
					var ppBasis = Basis.Get<PaperlessPicking>();
					ppBasis.WantedLineNbr = ppBasis.GetNextWantedLineNbr();
				}

				return split;
			}

			/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
			[PXOverride]
			public ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original,
				Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
			{
				var state = base_DecorateScanState(original);

				if (state.ModeCode == WaveBatchPicking.WavePickMode.Value && state is ToteSupport.SelectToteState selectTote)
					InjectSelectToteValidateAgainstWantedLine(selectTote);

				return state;
			}

			/// Overrides <see cref="WaveBatchPicking.WavePickMode.AlterToteSupport"/>
			public class SuppressWorksheetToteSupportChanges : WorksheetPicking.ScanExtension<WaveBatchPicking.WavePickMode.AlterToteSupport>
			{
				public static bool IsActive() => PaperlessWaveBatchPicking.IsActive();

				/// Overrides <see cref="WaveBatchPicking.WavePickMode.AlterToteSupport.IsSuppressed"/>
				[PXOverride]
				public bool get_IsSuppressed(Func<bool> base_IsSuppressed) => true;
			}
			#endregion

			#region Decorations
			public virtual void InjectSelectToteValidateAgainstWantedLine(ToteSupport.SelectToteState selectTote)
			{
				/// Intercepts <see cref="ToteSupport.SelectToteState.Validate"/>
				selectTote.Intercept.Validate.ByAppend((basis, tote) =>
				{
					var ppBasis = basis.Get<PaperlessPicking>();
					if (basis.Remove == false && ppBasis.GetWantedSplit() is SOPickerListEntry wantedLine)
					{
						if (wantedLine.ToteID == tote.ToteID)
							return Validation.Ok;

						var properTotesIDs = ppBasis.WSBasis.ShipmentsOfPicker
							.SearchAll<Asc<SOPickerToShipmentLink.shipmentNbr>>(new object[] { wantedLine.ShipmentNbr })
							.RowCast<SOPickerToShipmentLink>()
							.Select(link => link.ToteID)
							.ToHashSet();

						if (properTotesIDs.Contains(tote.ToteID) == false)
							return Validation.Fail(ToteSupport.Msg.MissingToteAssignedToShipment,
								tote.ToteCD,
								wantedLine.ShipmentNbr,
								properTotesIDs
									.Select(tid => INTote.PK.Find(basis, basis.SiteID, tid).ToteCD)
									.With(tcds => string.Join(", ", tcds)));
					}

					return Validation.Ok;
				});
			}
			#endregion
		}

		/// Overrides <see cref="PaperlessPicking"/>
		public class AlterPaperlessPicking : PaperlessPicking.ScanExtension
		{
			public static bool IsActive() => PaperlessWaveBatchPicking.IsActive();

			/// Overrides <see cref="PaperlessPicking.EnsureShipmentUserLinkForPaperlessPick"/>
			[PXOverride]
			public void EnsureShipmentUserLinkForPaperlessPick(Action base_EnsureShipmentUserLinkForPaperlessPick)
			{
				if (Basis.CurrentMode is WaveBatchPicking.WavePickMode || Basis.CurrentMode is WaveBatchPicking.BatchPickMode)
				{
					// suppress work logs for wave and batch paperless picking
				}
				else
					base_EnsureShipmentUserLinkForPaperlessPick();
			}
		}
		#endregion
	}
}
