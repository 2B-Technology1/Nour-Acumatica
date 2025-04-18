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
using PX.BarcodeProcessing;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	using WMSBase = WarehouseManagementSystem<PickPackShip, PickPackShip.Host>;

	public partial class PickPackShip : WMSBase
	{
		public sealed class PickMode : ScanMode
		{
			public const string Value = "PICK";
			public class value : BqlString.Constant<value> { public value() : base(PickMode.Value) { } }

			public override string Code => Value;
			public override string Description => Msg.Description;

			protected override bool IsModeActive() => Basis.Setup.Current.ShowPickTab == true;

			#region State Machine
			protected override IEnumerable<ScanState<PickPackShip>> CreateStates()
			{
				yield return new ShipmentState();
				yield return new LocationState();
				yield return new InventoryItemState() { AlternateType = INPrimaryAlternateType.CPN, IsForIssue = true, IsForTransfer = Basis.IsTransfer, SuppressModuleItemStatusCheck = true };
				yield return new LotSerialState();
				yield return new ExpireDateState() { IsForIssue = true, IsForTransfer = Basis.IsTransfer };
				yield return new ConfirmState();
				yield return new CommandOrShipmentOnlyState();
			}

			protected override IEnumerable<ScanTransition<PickPackShip>> CreateTransitions()
			{
				return StateFlow(flow => flow
					.From<ShipmentState>()
					.NextTo<LocationState>()
					.NextTo<InventoryItemState>()
					.NextTo<LotSerialState>()
					.NextTo<ExpireDateState>());
			}

			protected override IEnumerable<ScanCommand<PickPackShip>> CreateCommands()
			{
				yield return new RemoveCommand();
				yield return new QtySupport.SetQtyCommand();
				yield return new ConfirmShipmentCommand();
			}

			protected override IEnumerable<ScanRedirect<PickPackShip>> CreateRedirects() => AllWMSRedirects.CreateFor<PickPackShip>();

			protected override void ResetMode(bool fullReset)
			{
				base.ResetMode(fullReset);

				Clear<ShipmentState>(when: fullReset && !Basis.IsWithinReset);
				Clear<LocationState>(when: fullReset || Basis.PromptLocationForEveryLine);
				Clear<InventoryItemState>(when: fullReset);
				Clear<LotSerialState>();
				Clear<ExpireDateState>();
			}
			#endregion

			#region Logic
			public class Logic : ScanExtension
			{
				#region Views
				public
					SelectFrom<SOShipLineSplit>.
					InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipmentLine>.
					OrderBy<
						SOShipLineSplit.shipmentNbr.Asc,
						SOShipLineSplit.isUnassigned.Desc,
						SOShipLineSplit.lineNbr.Asc>.
					View Picked;
				protected virtual IEnumerable picked()
				{
					var delegateResult = new PXDelegateResult { IsResultSorted = true };
					delegateResult.AddRange(Basis.GetSplits(Basis.RefNbr, includeUnassigned: true, s => s.PickedQty >= s.Qty));
					return delegateResult;
				}
				#endregion

				#region Buttons
				public PXAction<ScanHeader> ReviewPick;
				[PXButton, PXUIField(DisplayName = "Review")]
				protected virtual IEnumerable reviewPick(PXAdapter adapter) => adapter.Get();
				#endregion

				#region Event Handlers
				protected virtual void _(Events.RowSelected<ScanHeader> e) => ReviewPick.SetVisible(Base.IsMobile && e.Row?.Mode == PickMode.Value);
				#endregion

				#region Decorations
				public virtual void InjectExpireDateForPickDeactivationOnAlreadyEnteredLot(ExpireDateState expireDateState)
				{
					expireDateState.Intercept.IsStateActive.ByConjoin(basis =>
						basis.SelectedLotSerialClass?.LotSerIssueMethod != INLotSerIssueMethod.UserEnterable &&
						basis.Get<PickMode.Logic>().Picked.SelectMain().Any(t =>
							t.IsUnassigned == true ||
							string.Equals(t.LotSerialNbr, basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase) && t.PickedQty == 0));
				}
				#endregion

				#region Overrides
				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
				[PXOverride]
				public virtual ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original, Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
				{
					var state = base_DecorateScanState(original);

					if (state.ModeCode == PickMode.Value)
					{
						PXSelectBase<SOShipLineSplit> viewSelector(PickPackShip basis) => basis.Get<Logic>().Picked;

						if (state is LocationState locationState)
						{
							Basis.InjectLocationDeactivationOnDefaultLocationOption(locationState);
							Basis.InjectLocationSkippingOnPromptLocationForEveryLineOption(locationState);
							Basis.InjectLocationPresenceValidation(locationState, viewSelector);
						}
						else if (state is InventoryItemState itemState)
						{
							Basis.InjectItemAbsenceHandlingByLocation(itemState);
							Basis.InjectItemPresenceValidation(itemState, viewSelector);
						}
						else if (state is LotSerialState lsState)
						{
							Basis.InjectLotSerialPresenceValidation(lsState, viewSelector);
							Basis.InjectLotSerialDeactivationOnDefaultLotSerialOption(lsState, isEntranceAllowed: true);
						}
						else if (state is ExpireDateState expireDateState)
						{
							InjectExpireDateForPickDeactivationOnAlreadyEnteredLot(expireDateState);
						}
					}

					return state;
				}

				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.OnBeforeFullClear"/>
				[PXOverride]
				public void OnBeforeFullClear(Action base_OnBeforeFullClear)
				{
					base_OnBeforeFullClear();

					if (Basis.CurrentMode is PickMode && Basis.RefNbr != null)
						if (Graph.WorkLogExt.SuspendFor(Basis.RefNbr, Graph.Accessinfo.UserID, SOShipmentProcessedByUser.jobType.Pick))
							Graph.WorkLogExt.PersistWorkLog();
				}
				#endregion

				public virtual bool ShowPickTab(ScanHeader row) => Basis.HasPick && row.Mode == PickMode.Value;
				public virtual bool CanPick => Picked.SelectMain().Any(s => s.PickedQty < s.Qty);
			}
			#endregion

			#region States
			public new sealed class ShipmentState : PickPackShip.ShipmentState
			{
				protected override Validation Validate(SOShipment shipment)
				{
					if (shipment.Operation != SOOperation.Issue)
						return Validation.Fail(Msg.InvalidOperation, shipment.ShipmentNbr, Basis.SightOf<SOShipment.operation>(shipment));

					if (shipment.Status != SOShipmentStatus.Open)
						return Validation.Fail(Msg.InvalidStatus, shipment.ShipmentNbr, Basis.SightOf<SOShipment.status>(shipment));

					if(Basis.HasNonStockLinesWithEmptyLocation(shipment, out Validation error))
						return error;

					return Validation.Ok;
				}

				protected override void ReportSuccess(SOShipment shipment) => Basis.ReportInfo(Msg.Ready, shipment.ShipmentNbr);

				protected override void SetNextState()
				{
					if (Basis.Remove == false && Basis.Get<PickMode.Logic>().CanPick == false)
						Basis.SetScanState(BuiltinScanStates.Command, PickMode.Msg.Completed, Basis.RefNbr);
					else
						base.SetNextState();
				}

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : PickPackShip.ShipmentState.Msg
				{
					public new const string Ready = "The {0} shipment is loaded and ready to be picked.";
					public const string InvalidStatus = "The {0} shipment cannot be picked because it has the {1} status.";
					public const string InvalidOperation = "The {0} shipment cannot be picked because it has the {1} operation.";
				}
				#endregion
			}

			public sealed class ConfirmState : ConfirmationState
			{
				public override string Prompt => Basis.Localize(Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);
				protected override FlowStatus PerformConfirmation() => Get<Logic>().ConfirmPicked();

				#region Logic
				public class Logic : ScanExtension
				{
					protected PickMode.Logic Mode { get; private set; }
					public override void Initialize() => Mode = Basis.Get<PickMode.Logic>();

					public virtual FlowStatus ConfirmPicked()
					{
						SOShipLineSplit pickedSplit = GetPickedSplit();
						if (pickedSplit == null)
							return FlowStatus.Fail(Basis.Remove == true ? Msg.NothingToRemove : Msg.NothingToPick).WithModeReset;

						decimal qty = Basis.BaseQty;
						decimal threshold = Graph.GetQtyThreshold(pickedSplit);

						if (qty != 0)
						{
							bool splitUpdated = false;

							if (Basis.Remove == true)
							{
								if (pickedSplit.PickedQty - qty < 0)
									return FlowStatus.Fail(Msg.Underpicking);
								if (pickedSplit.PickedQty - qty < pickedSplit.PackedQty)
									return FlowStatus.Fail(Msg.UnderpickingByPack);
							}
							else
							{
								if (Basis.CurrentMode.HasActive<LotSerialState>()
									&& string.Equals(pickedSplit.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase)
									&& Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.SerialNumbered
									&& (pickedSplit.PickedQty + qty).IsNotIn(0, 1))
									return FlowStatus.Fail(InventoryItemState.Msg.SerialItemNotComplexQty);

								if (pickedSplit.PickedQty + qty > pickedSplit.Qty * threshold)
									return FlowStatus.Fail(Msg.Overpicking);

								if (!string.Equals(pickedSplit.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase)
									&& Basis.IsEnterableLotSerial(isForIssue: true, isForTransfer: Basis.IsTransfer))
								{
									if (!SetLotSerialNbrAndQty(pickedSplit, qty))
										return FlowStatus.Fail(Msg.Overpicking);
									splitUpdated = true;
								}
							}

							if (!splitUpdated)
							{
								Basis.EnsureAssignedSplitEditing(pickedSplit);

								pickedSplit.PickedQty += Basis.Remove == true ? -qty : qty;

								if (Basis.Remove == true && Basis.IsEnterableLotSerial(isForIssue: true, isForTransfer: Basis.IsTransfer))
								{
									if (pickedSplit.PickedQty + qty == pickedSplit.Qty)
										pickedSplit.Qty = pickedSplit.PickedQty;

									if (pickedSplit.Qty == 0)
										Mode.Picked.Delete(pickedSplit);
									else
										Mode.Picked.Update(pickedSplit);
								}
								else
									Mode.Picked.Update(pickedSplit);
							}
						}

						EnsureShipmentUserLinkForPick();

						Basis.ReportInfo(
							Basis.Remove == true ? Msg.InventoryRemoved : Msg.InventoryAdded,
							Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

						return FlowStatus.Ok.WithDispatchNext;
					}

					public virtual bool IsSelectedSplit(SOShipLineSplit split)
					{
						return
							split.InventoryID == Basis.InventoryID &&
							split.SubItemID == Basis.SubItemID &&
							split.SiteID == Basis.SiteID &&
							split.LocationID == (Basis.LocationID ?? split.LocationID) &&
							(
								string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase) ||
								Basis.Remove == false &&
								(
									Basis.SelectedLotSerialClass.LotSerAssign == INLotSerAssign.WhenUsed ||
									(
										Basis.SelectedLotSerialClass.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable &&
										split.PackedQty == 0
									)
								)
							);
					}

					public virtual bool SetLotSerialNbrAndQty(SOShipLineSplit pickedSplit, decimal qty)
					{
						PXSelectBase<SOShipLineSplit> splitsView = Mode.Picked;
						if (pickedSplit.PickedQty == 0 && pickedSplit.IsUnassigned == false)
						{
							if (Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.SerialNumbered && Basis.SelectedLotSerialClass.LotSerIssueMethod == INLotSerIssueMethod.UserEnterable)
							{
								SOShipLineSplit originalSplit =
									SelectFrom<SOShipLineSplit>.
									InnerJoin<SOLineSplit>.On<SOShipLineSplit.FK.OriginalLineSplit>.
									Where<
										SOShipLineSplit.shipmentNbr.IsEqual<WMSScanHeader.refNbr.FromCurrent>.
										And<SOLineSplit.lotSerialNbr.IsEqual<@P.AsString>>>.
									View.Select(Basis, Basis.LotSerialNbr);

								if (originalSplit == null)
								{
									pickedSplit.LotSerialNbr = Basis.LotSerialNbr;
									pickedSplit.PickedQty += qty;
									pickedSplit = splitsView.Update(pickedSplit);
								}
								else
								{
									if (string.Equals(originalSplit.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase)) return false;

									var tempOriginalSplit = PXCache<SOShipLineSplit>.CreateCopy(originalSplit);
									var tempPickedSplit = PXCache<SOShipLineSplit>.CreateCopy(pickedSplit);

									originalSplit.Qty = 0;
									originalSplit.LotSerialNbr = Basis.LotSerialNbr;
									originalSplit = splitsView.Update(originalSplit);
									originalSplit.Qty = tempOriginalSplit.Qty;
									originalSplit.PickedQty = tempPickedSplit.PickedQty + qty;
									originalSplit.ExpireDate = tempPickedSplit.ExpireDate;
									originalSplit = splitsView.Update(originalSplit);

									pickedSplit.Qty = 0;
									pickedSplit.LotSerialNbr = tempOriginalSplit.LotSerialNbr;
									pickedSplit = splitsView.Update(pickedSplit);
									pickedSplit.Qty = tempPickedSplit.Qty;
									pickedSplit.PickedQty = tempOriginalSplit.PickedQty;
									pickedSplit.ExpireDate = tempOriginalSplit.ExpireDate;
									pickedSplit = splitsView.Update(pickedSplit);
								}
							}
							else
							{
								pickedSplit.LotSerialNbr = Basis.LotSerialNbr;
								if (Basis.SelectedLotSerialClass.LotSerTrackExpiration == true && Basis.ExpireDate != null)
									pickedSplit.ExpireDate = Basis.ExpireDate; // TODO: use expire date of the same lot/serial in the shipment
								pickedSplit.PickedQty += qty;
								pickedSplit = splitsView.Update(pickedSplit);
							}
						}
						else
						{
							var existingSplit = pickedSplit.IsUnassigned == true || Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.LotNumbered
								? splitsView.SelectMain().FirstOrDefault(s =>
									s.LineNbr == pickedSplit.LineNbr &&
									s.IsUnassigned == false &&
									string.Equals(s.LotSerialNbr, Basis.LotSerialNbr ?? s.LotSerialNbr, StringComparison.OrdinalIgnoreCase) &&
									IsSelectedSplit(s))
								: null;

							bool suppressMode = pickedSplit.IsUnassigned == false && Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.LotNumbered;

							if (existingSplit != null)
							{
								existingSplit.PickedQty += qty;
								if (existingSplit.PickedQty > existingSplit.Qty)
									existingSplit.Qty = existingSplit.PickedQty;

								using (Graph.LineSplittingExt.SuppressedModeScope(suppressMode))
									existingSplit = splitsView.Update(existingSplit);
							}
							else
							{
								var newSplit = PXCache<SOShipLineSplit>.CreateCopy(pickedSplit);
								newSplit.SplitLineNbr = null;
								newSplit.PlanID = null;
								newSplit.LotSerialNbr = Basis.LotSerialNbr;
								if (pickedSplit.Qty - qty > 0 || pickedSplit.IsUnassigned == true)
								{
									newSplit.Qty = qty;
									newSplit.PickedQty = qty;
									newSplit.PackedQty = 0;
									newSplit.IsUnassigned = false;
								}
								else
								{
									newSplit.Qty = pickedSplit.Qty;
									newSplit.PickedQty = pickedSplit.PickedQty;
									newSplit.PackedQty = pickedSplit.PackedQty;
								}

								using (Graph.LineSplittingExt.SuppressedModeScope(suppressMode))
									newSplit = splitsView.Insert(newSplit);
							}

							if (pickedSplit.IsUnassigned == false) // Unassigned splits will be processed automatically
							{
								if (pickedSplit.Qty <= 0)
									pickedSplit = splitsView.Delete(pickedSplit);
								else
								{
									pickedSplit.Qty -= qty;
									pickedSplit = splitsView.Update(pickedSplit);
								}
							}
						}

						return true;
					}

					public virtual SOShipLineSplit GetPickedSplit()
					{
						var allFittingSplits = Mode.Picked.SelectMain().Where(IsSelectedSplit);
						if (Basis.Remove == true)
						{
							SOShipLineSplit splitToRemove = allFittingSplits
								.OrderByAccordanceTo(split => split.PickedQty > 0) // started
								.ThenByAccordanceTo(split => string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
								.ThenByAccordanceTo(split => split.PickedQty >= Basis.BaseQty) // can lose such much qty
								.ThenBy(split => split.Qty - split.PickedQty) // can lose less than others
								.FirstOrDefault();
							return splitToRemove;
						}
						else if (Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							SOShipLineSplit serialSplitToPick = allFittingSplits
								.OrderByAccordanceTo(split => string.Equals(split.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
								.ThenByAccordanceTo(split => split.PickedQty == 0)
								.ThenByAccordanceTo(split => split.IsUnassigned == true)
								.FirstOrDefault();
							return serialSplitToPick;
						}
						else
						{
							SOShipLineSplit splitToPick = allFittingSplits
								.OrderByAccordanceTo(split => split.Qty > split.PickedQty) // not finished
								.ThenByAccordanceTo(split => string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
								.ThenByAccordanceTo(split => string.IsNullOrEmpty(split.LotSerialNbr)) // unassigned first?
								.ThenByAccordanceTo(split => split.Qty >= split.PickedQty + Basis.BaseQty) // can accept such much qty
								.ThenByAccordanceTo(split => split.PickedQty > 0) // started
								.ThenByDescending(split => split.Qty - split.PickedQty) // can accept more than others
								.FirstOrDefault();
							return splitToPick;
						}
					}

					public virtual void EnsureShipmentUserLinkForPick()
					{
						Graph.WorkLogExt.EnsureFor(
							Basis.RefNbr,
							Graph.Accessinfo.UserID,
							SOShipmentProcessedByUser.jobType.Pick);
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public new abstract class Msg
				{
					public const string Prompt = "Confirm picking {0} x {1} {2}.";

					public const string NothingToPick = "No items to pick.";
					public const string NothingToRemove = "No items to remove from the shipment.";

					public const string Overpicking = "The picked quantity cannot be greater than the quantity in the shipment line.";
					public const string Underpicking = "The picked quantity cannot become negative.";
					public const string UnderpickingByPack = "The picked quantity cannot be less than the already packed quantity.";

					public const string InventoryAdded = "{0} x {1} {2} has been added to the shipment.";
					public const string InventoryRemoved = "{0} x {1} {2} has been removed from the shipment.";
				}
				#endregion
			}
			#endregion

			#region Redirect
			public sealed class RedirectFrom<TForeignBasis> : WMSBase.RedirectFrom<TForeignBasis>.SetMode<PickMode>
				where TForeignBasis : PXGraphExtension, IBarcodeDrivenStateMachine
			{
				public override string Code => PickMode.Value;
				public override string DisplayName => Msg.Description;

				private string RefNbr { get; set; }

				public override bool IsPossible
				{
					get
					{
						bool wmsFulfillment = PXAccess.FeatureInstalled<CS.FeaturesSet.wMSFulfillment>();
						var ppsSetup = SOPickPackShipSetup.PK.Find(Basis.Graph, Basis.Graph.Accessinfo.BranchID);
						return wmsFulfillment && ppsSetup?.ShowPickTab == true;
					}
				}

				protected override bool PrepareRedirect()
				{
					if (Basis is PickPackShip pps && pps.RefNbr != null && pps.DocumentIsConfirmed == false)
					{
						if (pps.FindMode<PickMode>().TryValidate(pps.Shipment).By<ShipmentState>() is Validation valid && valid.IsError == true)
						{
							pps.ReportError(valid.Message, valid.MessageArgs);
							return false;
						}
						else
							RefNbr = pps.RefNbr;
					}
					
					return true;
				}

				protected override void CompleteRedirect()
				{
					if (Basis is PickPackShip pps && pps.CurrentMode.Code != ReturnMode.Value && this.RefNbr != null)
						if (pps.TryProcessBy(PickPackShip.ShipmentState.Value, RefNbr, StateSubstitutionRule.KeepAll & ~StateSubstitutionRule.KeepPositiveReports))
						{
							pps.SetDefaultState();
							RefNbr = null;
						}
				}
			}
			#endregion

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ScanMode.Msg
			{
				public const string Description = "Pick";
				public const string Completed = "The {0} shipment is picked.";
			}
			#endregion

			#region Attached Fields
			[PXUIField(Visible = false)]
			public class ShowPick : FieldAttached.To<ScanHeader>.AsBool.Named<ShowPick>
			{
				public override bool? GetValue(ScanHeader row) => Base.WMS.Get<PickMode.Logic>().ShowPickTab(row) == true;
			}
			#endregion
		}
	}
}
