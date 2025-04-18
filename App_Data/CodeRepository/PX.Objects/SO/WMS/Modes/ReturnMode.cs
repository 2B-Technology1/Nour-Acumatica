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

using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	using WMSBase = WarehouseManagementSystem<PickPackShip, PickPackShip.Host>;

	public partial class PickPackShip : WMSBase
	{
		public sealed class ReturnMode : ScanMode
		{
			public const string Value = "CRTN";
			public class value : BqlString.Constant<value> { public value() : base(ReturnMode.Value) { } }

			public override string Code => Value;
			public override string Description => Msg.Description;

			protected override bool IsModeActive() => Basis.Setup.Current.ShowReturningTab == true;

			#region State Machine
			protected override IEnumerable<ScanState<PickPackShip>> CreateStates()
			{
				yield return new ShipmentState();
				yield return new LocationState();
				yield return new InventoryItemState() { AlternateType = INPrimaryAlternateType.CPN, IsForIssue = false };
				yield return new LotSerialState();
				yield return new ConfirmState();
				yield return new CommandOrShipmentOnlyState();
			}

			protected override IEnumerable<ScanTransition<PickPackShip>> CreateTransitions()
			{
				return StateFlow(flow => flow
					.From<ShipmentState>()
					.NextTo<InventoryItemState>()
					.NextTo<LotSerialState>()
					.NextTo<LocationState>());
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
					View Returned;
				protected virtual IEnumerable returned()
				{
					var delegateResult = new PXDelegateResult { IsResultSorted = true };
					delegateResult.AddRange(Basis.GetSplits(Basis.RefNbr, includeUnassigned: true, s => s.PickedQty >= s.Qty));
					return delegateResult;
				}
				#endregion

				#region Buttons
				public PXAction<ScanHeader> ReviewReturn;
				[PXButton, PXUIField(DisplayName = "Review")]
				protected virtual IEnumerable reviewReturn(PXAdapter adapter) => adapter.Get();
				#endregion

				#region Event Handlers
				protected virtual void _(Events.RowSelected<ScanHeader> e) => ReviewReturn.SetVisible(Base.IsMobile && e.Row?.Mode == ReturnMode.Value);
				#endregion

				#region Overrides
				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
				[PXOverride]
				public virtual ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original, Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
				{
					var state = base_DecorateScanState(original);

					if (state.ModeCode == ReturnMode.Value)
					{
						PXSelectBase<SOShipLineSplit> viewSelector(PickPackShip basis) => basis.Get<Logic>().Returned;

						if (state is LocationState locationState)
						{
							Basis.InjectLocationDeactivationOnDefaultLocationOption(locationState);
							Basis.InjectLocationSkippingOnPromptLocationForEveryLineOption(locationState);
						}
						else if (state is InventoryItemState itemState)
						{
							Basis.InjectItemPresenceValidation(itemState, viewSelector);
						}
						else if (state is LotSerialState lsState)
						{
							Basis.InjectLotSerialPresenceValidation(lsState, viewSelector);
						}
					}

					return state;
				}

				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.OnBeforeFullClear"/>
				[PXOverride]
				public void OnBeforeFullClear(Action base_OnBeforeFullClear)
				{
					base_OnBeforeFullClear();

					if (Basis.CurrentMode is ReturnMode && Basis.RefNbr != null)
						if (Graph.WorkLogExt.SuspendFor(Basis.RefNbr, Graph.Accessinfo.UserID, SOShipmentProcessedByUser.jobType.Pick))
							Graph.WorkLogExt.PersistWorkLog();
				}
				#endregion

				public virtual bool ShowReturnTab(ScanHeader row) => Basis.Setup.Current.ShowReturningTab == true && row.Mode == ReturnMode.Value;
				public virtual bool CanReturn => Returned.SelectMain().Any(s => s.PickedQty < s.Qty);
			}
			#endregion

			#region States
			public new sealed class ShipmentState : PickPackShip.ShipmentState
			{
				protected override AbsenceHandling.Of<SOShipment> HandleAbsence(string barcode)
				{
					SOShipment byCustomerRefNbr =
						SelectFrom<SOShipment>.
						InnerJoin<INSite>.On<SOShipment.FK.Site>.
						InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.Shipment>.
						InnerJoin<SOOrder>.On<SOOrderShipment.FK.Order>.
						LeftJoin<Customer>.On<SOShipment.customerID.IsEqual<Customer.bAccountID>>.SingleTableOnly.
						Where<
							Match<INSite, AccessInfo.userName.FromCurrent>.
							And<SOShipment.status.IsEqual<SOShipmentStatus.open>>.
							And<SOShipment.operation.IsEqual<SOOperation.receipt>>.
							And<SOOrder.customerRefNbr.IsEqual<@P.AsString>>.
							And<
								SOShipment.siteID.IsEqual<WMSScanHeader.siteID.FromCurrent>.
								Or<WMSScanHeader.siteID.FromCurrent.NoDefault.IsNull>>.
							And<
								Customer.bAccountID.IsNull.
								Or<Match<Customer, AccessInfo.userName.FromCurrent>>>>.
						View.Select(Basis, barcode);

					if (byCustomerRefNbr != null)
						return AbsenceHandling.ReplaceWith(byCustomerRefNbr);

					return base.HandleAbsence(barcode);
				}

				protected override Validation Validate(SOShipment shipment)
				{
					if (shipment.Operation != SOOperation.Receipt)
						return Validation.Fail(Msg.InvalidOperation, shipment.ShipmentNbr, Basis.SightOf<SOShipment.operation>(shipment));

					if (shipment.Status != SOShipmentStatus.Open)
						return Validation.Fail(Msg.InvalidStatus, shipment.ShipmentNbr, Basis.SightOf<SOShipment.status>(shipment));

					return Validation.Ok;
				}

				protected override void ReportSuccess(SOShipment shipment) => Basis.ReportInfo(Msg.Ready, shipment.ShipmentNbr);

				protected override void SetNextState()
				{
					if (Basis.Remove == false && Basis.Get<ReturnMode.Logic>().CanReturn == false)
						Basis.SetScanState(BuiltinScanStates.Command, ReturnMode.Msg.Completed, Basis.RefNbr);
					else
						base.SetNextState();
				}

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : PickPackShip.ShipmentState.Msg
				{
					public new const string Ready = "The {0} shipment is loaded and ready to be returned.";
					public const string InvalidStatus = "The {0} shipment cannot be returned because it has the {1} status.";
					public const string InvalidOperation = "The {0} shipment cannot be returned because it has the {1} operation.";
				}
				#endregion
			}

			public sealed class ConfirmState : ConfirmationState
			{
				public override string Prompt => Basis.Localize(Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

				protected override FlowStatus PerformConfirmation() => Basis.Get<Logic>().Confirm();

				#region Logic
				public class Logic : ScanExtension
				{
					protected ReturnMode.Logic ModeLogic { get; private set; }
					public override void Initialize() => ModeLogic = Basis.Get<ReturnMode.Logic>();

					public virtual FlowStatus Confirm()
					{
						var returnedSplit = ModeLogic.Returned
							.SelectMain()
							.Where(r => IsSelectedSplit(r))
							.OrderByDescending(split => split.IsUnassigned == false && Basis.Remove == true ? split.PickedQty > 0 : split.Qty > split.PickedQty)
							.OrderByDescending(split => Basis.Remove == true ? split.PickedQty > 0 : split.Qty > split.PickedQty)
							.ThenByDescending(split => string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
							.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
							.ThenByDescending(split => (split.Qty > split.PickedQty || Basis.Remove == true) && split.PickedQty > 0)
							.ThenByDescending(split => Sign.MinusIf(Basis.Remove == true) * (split.Qty - split.PickedQty))
							.FirstOrDefault();
						if (returnedSplit == null)
							return FlowStatus.Fail(Basis.Remove == true ? Msg.NothingToRemove : Msg.NothingToReturn).WithModeReset;

						decimal qty = Basis.BaseQty;
						decimal threshold = Graph.GetQtyThreshold(returnedSplit);

						if (qty != 0)
						{
							bool splitUpdated = false;

							if (Basis.Remove == true)
							{
								if (returnedSplit.PickedQty - qty < 0)
									return FlowStatus.Fail(Msg.Underreturning);
							}
							else
							{
								if (returnedSplit.PickedQty + qty > returnedSplit.Qty * threshold)
									return FlowStatus.Fail(Msg.Overreturning);

								if (!string.Equals(returnedSplit.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase) && Basis.IsEnterableLotSerial(isForIssue: false))
								{
									if (!SetLotSerialNbrAndQty(returnedSplit, qty))
										return FlowStatus.Fail(Msg.Overreturning);
									splitUpdated = true;
								}
							}

							if (!splitUpdated)
							{
								Basis.EnsureAssignedSplitEditing(returnedSplit);

								if (Basis.Remove == false && Basis.LocationID != null)
									returnedSplit.LocationID = Basis.LocationID;

								returnedSplit.PickedQty += Basis.Remove == true ? -qty : qty;

								ModeLogic.Returned.Update(returnedSplit);
							}
						}

						EnsureShipmentUserLinkForReturn();

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
							string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase);
					}

					public virtual bool SetLotSerialNbrAndQty(SOShipLineSplit pickedSplit, decimal qty)
					{
						PXSelectBase<SOShipLineSplit> splitsView = ModeLogic.Returned;
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
									if (Basis.Remove == false)
										pickedSplit.LocationID = Basis.LocationID;
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
									if (Basis.Remove == false)
										pickedSplit.LocationID = Basis.LocationID;
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
								if (Basis.Remove == false)
									pickedSplit.LocationID = Basis.LocationID;

								pickedSplit.LotSerialNbr = Basis.LotSerialNbr;

								if (Basis.SelectedLotSerialClass.LotSerTrackExpiration == true && Basis.ExpireDate != null)
									pickedSplit.ExpireDate = Basis.ExpireDate; // not sure about that - do we really enter expire date on returns?

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
								newSplit.LotSerialNbr = Basis.LotSerialNbr;
								if (pickedSplit.Qty - qty > 0 || pickedSplit.IsUnassigned == true)
								{
									newSplit.Qty = qty;
									newSplit.PickedQty = qty;
									newSplit.PackedQty = 0;
									newSplit.IsUnassigned = false;
									newSplit.PlanID = null;
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

					public virtual void EnsureShipmentUserLinkForReturn()
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
				public abstract class Msg
				{
					public const string Prompt = "Confirm returning {0} x {1} {2}.";

					public const string NothingToReturn = "No items to return.";
					public const string NothingToRemove = "No items to remove from the shipment.";

					public const string Overreturning = "The returned quantity cannot be greater than the quantity in the shipment line.";
					public const string Underreturning = "The returned quantity cannot become negative.";

					public const string InventoryAdded = "{0} x {1} {2} has been added to the return.";
					public const string InventoryRemoved = "{0} x {1} {2} has been removed from the return.";
				}
				#endregion
			}
			#endregion

			#region Redirect
			public sealed class RedirectFrom<TForeignBasis> : WMSBase.RedirectFrom<TForeignBasis>.SetMode<ReturnMode>
				where TForeignBasis : PXGraphExtension, IBarcodeDrivenStateMachine
			{
				public override string Code => "SORETURN";
				public override string DisplayName => Msg.DisplayName;

				public override bool IsPossible
				{
					get
					{
						bool wmsFulfillment = PXAccess.FeatureInstalled<CS.FeaturesSet.wMSFulfillment>();
						var ppsSetup = SOPickPackShipSetup.PK.Find(Basis.Graph, Basis.Graph.Accessinfo.BranchID);
						return wmsFulfillment && ppsSetup?.ShowReturningTab == true;
					}
				}

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string DisplayName = "SO Return";
				}
				#endregion
			}
			#endregion

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ScanMode.Msg
			{
				public const string Description = "Return";
				public const string Completed = "The {0} shipment has been returned.";
			}
			#endregion

			#region Attached Fields
			[PXUIField(Visible = false)]
			public class ShowReturn : FieldAttached.To<ScanHeader>.AsBool.Named<ShowReturn>
			{
				public override bool? GetValue(ScanHeader row) => Base.WMS.Get<ReturnMode.Logic>().ShowReturnTab(row) == true;
			}
			#endregion
		}
	}
}
