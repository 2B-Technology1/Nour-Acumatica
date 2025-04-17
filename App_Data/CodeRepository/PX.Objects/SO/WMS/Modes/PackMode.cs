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

using PX.SM;
using PX.Common;
using PX.BarcodeProcessing;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.SO.WMS
{
	using WMSBase = WarehouseManagementSystem<PickPackShip, PickPackShip.Host>;

	public partial class PickPackShip : WMSBase
	{
		public sealed class PackMode : ScanMode
		{
			public const string Value = "PACK";
			public class value : BqlString.Constant<value> { public value() : base(PackMode.Value) { } }

			public override string Code => Value;
			public override string Description => Msg.Description;

			protected override bool IsModeActive() => Basis.Setup.Current.ShowPackTab == true;

			#region State Machine
			protected override IEnumerable<ScanState<PickPackShip>> CreateStates()
			{
				yield return new ShipmentState();
				yield return new BoxState();
				yield return new InventoryItemState() { AlternateType = INPrimaryAlternateType.CPN, IsForIssue = true, IsForTransfer = Basis.IsTransfer, SuppressModuleItemStatusCheck = true };
				yield return new LotSerialState();
				yield return new ConfirmState();
				yield return new CommandOrShipmentOnlyState();

				yield return new BoxConfirming.StartState();
				yield return new BoxConfirming.WeightState();
				yield return new BoxConfirming.DimensionsState();
				yield return new BoxConfirming.CompleteState();

				if (!Basis.HasPick)
				{
					yield return new LocationState();
					yield return new ExpireDateState() { IsForIssue = true, IsForTransfer = Basis.IsTransfer };
				}
			}

			protected override IEnumerable<ScanTransition<PickPackShip>> CreateTransitions()
			{
				var itemFlow = StateFlow(flow => flow
					.ForkBy(basis => basis.HasPick)
					.PositiveBranch(separatePicking => separatePicking
						.From<ShipmentState>()
						.NextTo<BoxState>()
						.NextTo<InventoryItemState>()
						.NextTo<LotSerialState>())
					.NegativeBranch(packOnly => packOnly
						.From<ShipmentState>()
						.NextTo<BoxState>()
						.NextTo<LocationState>()
						.NextTo<InventoryItemState>()
						.NextTo<LotSerialState>()
						.NextTo<ExpireDateState>()));
				var boxFlow = StateFlow(flow => flow
					.From<BoxConfirming.StartState>()
					.NextTo<BoxConfirming.WeightState>()
					.NextTo<BoxConfirming.DimensionsState>()
					.NextTo<BoxConfirming.CompleteState>());

				return itemFlow.Concat(boxFlow);
			}

			protected override IEnumerable<ScanCommand<PickPackShip>> CreateCommands()
			{
				yield return new RemoveCommand();
				yield return new QtySupport.SetQtyCommand();
				yield return new ConfirmPackageCommand();
				yield return new ConfirmShipmentCommand();
			}

			protected override IEnumerable<ScanQuestion<PickPackShip>> CreateQuestions()
			{
				yield return new BoxConfirming.WeightState.SkipQuestion();
				yield return new BoxConfirming.WeightState.SkipScalesQuestion();
				yield return new BoxConfirming.DimensionsState.SkipQuestion();
			}

			protected override IEnumerable<ScanRedirect<PickPackShip>> CreateRedirects() => AllWMSRedirects.CreateFor<PickPackShip>();

			protected override void ResetMode(bool fullReset)
			{
				base.ResetMode(fullReset);

				Clear<ShipmentState>(when: fullReset && !Basis.IsWithinReset);
				Clear<BoxState>(when: fullReset);
				Clear<InventoryItemState>(when: fullReset);
				Clear<LotSerialState>();

				Clear<BoxConfirming.WeightState>();
				Clear<BoxConfirming.DimensionsState>();

				if (fullReset)
					Get<Logic>().PackageLineNbrUI = null;

				if (!Basis.HasPick)
				{
					Clear<LocationState>(when: fullReset || Basis.PromptLocationForEveryLine);
					Clear<ExpireDateState>();
				}
			}
			#endregion

			#region Logic
			public class Logic : ScanExtension
			{
				#region State
				public PackScanHeader PackHeader => Basis.Header.Get<PackScanHeader>() ?? new PackScanHeader();
				public ValueSetter<ScanHeader>.Ext<PackScanHeader> PackSetter => Basis.HeaderSetter.With<PackScanHeader>();

				#region PackageLineNbr
				public int? PackageLineNbr
				{
					get => PackHeader.PackageLineNbr;
					set => PackSetter.Set(h => h.PackageLineNbr, value);
				}
				#endregion
				#region PackageLineNbrUI
				public int? PackageLineNbrUI
				{
					get => PackHeader.PackageLineNbrUI;
					set => PackSetter.Set(h => h.PackageLineNbrUI, value);
				}
				#endregion
				#endregion

				#region Views
				public
					SelectFrom<SOShipLineSplit>.
					InnerJoin<SOShipLine>.On<SOShipLineSplit.FK.ShipmentLine>.
					OrderBy<
						SOShipLineSplit.shipmentNbr.Asc,
						SOShipLineSplit.isUnassigned.Desc,
						SOShipLineSplit.lineNbr.Asc>.
					View PickedForPack;
				protected virtual IEnumerable pickedForPack()
				{
					var delegateResult = new PXDelegateResult { IsResultSorted = true };
					delegateResult.AddRange(Basis.GetSplits(Basis.RefNbr, includeUnassigned: true, s => s.PackedQty >= s.Qty));
					return delegateResult;
				}

				public SelectFrom<SOShipLineSplit>.View Packed;
				protected virtual IEnumerable packed()
				{
					return Basis.Header == null
						? Enumerable.Empty<PXResult<SOShipLineSplit, SOShipLine>>() :
						from link in Graph.PackageDetailExt.PackageDetailSplit.SelectMain(Basis.RefNbr, PackageLineNbrUI)
						from split in PickedForPack.Select().Cast<PXResult<SOShipLineSplit, SOShipLine>>()
						where
							((SOShipLineSplit)split).ShipmentNbr == link.ShipmentNbr &&
							((SOShipLineSplit)split).LineNbr == link.ShipmentLineNbr &&
							((SOShipLineSplit)split).SplitLineNbr == link.ShipmentSplitLineNbr
						select split;
				}

				public
					SelectFrom<SOPackageDetailEx>.
					Where<
						SOPackageDetailEx.shipmentNbr.IsEqual<WMSScanHeader.refNbr.FromCurrent>.
						And<SOPackageDetailEx.lineNbr.IsEqual<PackScanHeader.packageLineNbrUI.FromCurrent.NoDefault>>>.
					View ShownPackage;
				#endregion

				#region Buttons
				public PXAction<ScanHeader> ReviewPack;
				[PXButton, PXUIField(DisplayName = "Review")]
				protected virtual IEnumerable reviewPack(PXAdapter adapter)
				{
					PackageLineNbrUI = null;
					return adapter.Get();
				}
				#endregion

				#region Event Handlers
				protected virtual void _(Events.RowSelected<ScanHeader> e)
				{
					if (e.Row == null)
						return;

					new[] {
						Base.Packages.Cache,
						Base.PackageDetailExt.PackageDetailSplit.Cache
					}
					.Modify(c => c.AllowInsert = c.AllowUpdate = c.AllowDelete = !Basis.DocumentIsConfirmed)
					.Consume();

					ReviewPack.SetVisible(Base.IsMobile && e.Row.Mode == PackMode.Value);
				}
				#endregion

				#region Decorations
				public virtual void InjectExpireDateForPackDeactivationOnAlreadyEnteredLot(ExpireDateState expireDateState)
				{
					expireDateState.Intercept.IsStateActive.ByConjoin(basis =>
						basis.SelectedLotSerialClass?.LotSerAssign == INLotSerAssign.WhenUsed &&
						basis.Get<PackMode.Logic>().PickedForPack.SelectMain().Any(t =>
							t.IsUnassigned == true ||
							t.LotSerialNbr == basis.LotSerialNbr && t.PackedQty == 0));
				}

				public virtual void InjectItemAbsenceHandlingByBox(InventoryItemState itemState)
				{
					itemState.Intercept.HandleAbsence.ByAppend((basis, barcode) =>
						basis.Get<PackMode.Logic>().TryAutoConfirmCurrentPackageAndLoadNext(barcode) == false
							? AbsenceHandling.Skipped
							: AbsenceHandling.Done);
				}

				public virtual void InjectItemPromptForPackageConfirm(InventoryItemState itemState)
				{
					itemState.Intercept.StatePrompt.ByOverride((basis, base_StatePrompt) =>
						basis.Get<PackMode.Logic>().With(mode =>
							basis.Remove != true && mode.CanConfirmPackage
								? basis.HasActive<LocationState>()
									? Msg.BoxConfirmOrContinuePromptNoPick
									: Msg.BoxConfirmOrContinuePrompt
								: null)
						?? base_StatePrompt());
				}
				#endregion

				#region Overrides
				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState"/>
				[PXOverride]
				public virtual ScanState<PickPackShip> DecorateScanState(ScanState<PickPackShip> original, Func<ScanState<PickPackShip>, ScanState<PickPackShip>> base_DecorateScanState)
				{
					var state = base_DecorateScanState(original);

					if (state.ModeCode == PackMode.Value)
					{
						PXSelectBase<SOShipLineSplit> viewSelector(PickPackShip basis) => basis.Get<Logic>().PickedForPack;

						if (state is LocationState locationState)
						{
							Basis.InjectLocationDeactivationOnDefaultLocationOption(locationState);
							Basis.InjectLocationSkippingOnPromptLocationForEveryLineOption(locationState);
							Basis.InjectLocationPresenceValidation(locationState, viewSelector);
						}
						else if (state is InventoryItemState itemState)
						{
							if (!Basis.HasPick)
								Basis.InjectItemAbsenceHandlingByLocation(itemState);

							InjectItemPromptForPackageConfirm(itemState);
							InjectItemAbsenceHandlingByBox(itemState);
							Basis.InjectItemPresenceValidation(itemState, viewSelector);
						}
						else if (state is LotSerialState lsState)
						{
							Basis.InjectLotSerialPresenceValidation(lsState, viewSelector);
							Basis.InjectLotSerialDeactivationOnDefaultLotSerialOption(lsState, isEntranceAllowed: !Basis.HasPick);
						}
						else if (state is ExpireDateState expireDateState)
						{
							InjectExpireDateForPackDeactivationOnAlreadyEnteredLot(expireDateState);
						}
					}

					return state;
				}

				/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.OnBeforeFullClear"/>
				[PXOverride]
				public void OnBeforeFullClear(Action base_OnBeforeFullClear)
				{
					base_OnBeforeFullClear();

					if (Basis.CurrentMode is PackMode && Basis.RefNbr != null)
						if (Graph.WorkLogExt.SuspendFor(Basis.RefNbr, Graph.Accessinfo.UserID, Basis.HasPick ? SOShipmentProcessedByUser.jobType.Pack : SOShipmentProcessedByUser.jobType.PackOnly))
							Graph.WorkLogExt.PersistWorkLog();
				}

				[Obsolete]
				public virtual string GetCommandOrShipmentOnlyPrompt(Func<string> base_GetCommandOrShipmentOnlyPrompt)
					=> base_GetCommandOrShipmentOnlyPrompt();

				// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
				public class AlterCommandOrShipmentOnlyStatePrompt : ScanExtension<CommandOrShipmentOnlyState.Logic>
				{
					/// Overrides <see cref="CommandOrShipmentOnlyState.Logic.GetPromptForCommandOrShipmentOnly"/>
					[PXOverride]
					public virtual string GetPromptForCommandOrShipmentOnly(Func<string> base_GetPromptForCommandOrShipmentOnly)
					{
						if (Basis.CurrentMode is PackMode && Basis.Get<Logic>() is Logic mode && mode.CanConfirmPackage)
							return PackMode.Msg.BoxConfirmPrompt;

						return base_GetPromptForCommandOrShipmentOnly();
					}
				}

                #endregion

                public virtual bool ShowPackTab(ScanHeader row) => Basis.HasPack && row.Mode == PackMode.Value;
				public virtual bool CanPack => Basis.HasPick
					? PickedForPack.SelectMain().Any(s => s.PackedQty < s.PickedQty)
					: PickedForPack.SelectMain().Any(s => s.PackedQty < s.Qty);

				public virtual bool CanConfirmPackage => Basis.RefNbr != null && HasConfirmableBoxes && !HasSingleAutoPackage(Basis.RefNbr, out var _) && PackageLineNbr != null && SelectedPackage != null && !IsPackageEmpty(SelectedPackage);

				public virtual bool IsPackageEmpty(SOPackageDetailEx package) => Graph.PackageDetailExt.PackageDetailSplit.Select(package.ShipmentNbr, package.LineNbr).Count == 0;

				public SOPackageDetailEx SelectedPackage => Graph.Packages.Search<SOPackageDetailEx.lineNbr>(PackageLineNbr);

				public virtual bool HasConfirmableBoxes => Graph.Packages.SelectMain().Any(p => p.Confirmed == false && !IsPackageEmpty(p));
				public virtual bool HasSingleAutoPackage(string shipmentNbr, out SOPackageDetailEx package)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.autoPackaging>())
					{
						var packages =
							SelectFrom<SOPackageDetailEx>.
							Where<SOPackageDetailEx.shipmentNbr.IsEqual<@P.AsString>>.
							View.Select(Basis, shipmentNbr)
							.RowCast<SOPackageDetailEx>()
							.ToArray();
						if (packages.Length == 1 && packages[0].PackageType == SOPackageType.Auto)
						{
							package = packages[0];
							return true;
						}
						else if (packages.Any(p => p.PackageType == SOPackageType.Auto))
							throw new PXInvalidOperationException(Msg.CannotBePacked, shipmentNbr);
					}

					package = null;
					return false;
				}

				public virtual bool? TryAutoConfirmCurrentPackageAndLoadNext(string boxBarcode)
				{
					if (Basis.Remove == false)
					{
						CSBox box = CSBox.PK.Find(Basis, boxBarcode);
						if (box != null)
						{
							if (!Basis.Get<BoxConfirming.CompleteState.Logic>().TryAutoConfirm())
								return null;

							if (Basis.TryProcessBy<BoxState>(boxBarcode, StateSubstitutionRule.KeepPositiveReports | StateSubstitutionRule.KeepApplication | StateSubstitutionRule.KeepStateChange))
								return true;
						}
					}

					return false;
				}
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

					var splits = Basis.GetSplits(shipment.ShipmentNbr, includeUnassigned: true).RowCast<SOShipLineSplit>().AsEnumerable();
					if (Basis.HasPick)
					{
						if (splits.All(s => s.PickedQty == 0))
							return Validation.Fail(Msg.ShouldBePickedFirst, shipment.ShipmentNbr);
					}

					if (Basis.HasNonStockLinesWithEmptyLocation(shipment, out Validation error))
						return error;

					Get<PackMode.Logic>().HasSingleAutoPackage(shipment.ShipmentNbr, out var _);

					return Validation.Ok;
				}

				protected override void ReportSuccess(SOShipment shipment) => Basis.ReportInfo(Msg.Ready, shipment.ShipmentNbr);

				protected override void SetNextState()
				{
					var mode = Basis.Get<PackMode.Logic>();
					if (Basis.Remove == true || mode.CanPack || mode.HasConfirmableBoxes)
						base.SetNextState();
					else
						Basis.SetScanState(BuiltinScanStates.Command, PackMode.Msg.Completed, Basis.RefNbr);
				}

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : PickPackShip.ShipmentState.Msg
				{
					public new const string Ready = "The {0} shipment is loaded and ready to be packed.";
					public const string InvalidStatus = "The {0} shipment cannot be packed because it has the {1} status.";
					public const string InvalidOperation = "The {0} shipment cannot be packed because it has the {1} operation.";
					public const string ShouldBePickedFirst = "The {0} shipment cannot be packed because the items have not been picked.";
				}
				#endregion
			}

			public sealed class BoxState : EntityState<CSBox>
			{
				public const string Value = "BOX";
				public class value : BqlString.Constant<value> { public value() : base(BoxState.Value) { } }

				public PackMode.Logic Mode => Get<PackMode.Logic>();

				public override string Code => Value;
				protected override string StatePrompt => Msg.Prompt;

				protected override bool IsStateSkippable() => base.IsStateSkippable() || Mode.PackageLineNbr != null;

				protected override void OnTakingOver()
				{
					if (Mode.HasSingleAutoPackage(Basis.RefNbr, out SOPackageDetailEx package)) // skip single auto package input
					{
						Mode.PackageLineNbr = package.LineNbr;
						Mode.PackageLineNbrUI = package.LineNbr;
						Basis.Graph.Packages.Current = package;
						MoveToNextState();
					}
				}

				protected override CSBox GetByBarcode(string barcode) => CSBox.PK.Find(Basis, barcode);

				protected override void Apply(CSBox box)
				{
					SOPackageDetailEx package = Basis.Graph.Packages.SelectMain().FirstOrDefault(p => string.Equals(p.BoxID.Trim(), box.BoxID.Trim(), StringComparison.OrdinalIgnoreCase) && p.Confirmed == false);
					if (package == null)
					{
						package = (SOPackageDetailEx)Basis.Graph.Packages.Cache.CreateInstance();
						package.BoxID = box.BoxID;
						package.ShipmentNbr = Basis.RefNbr;
						package = Basis.Graph.Packages.Insert(package);
						Basis.Save.Press();
					}

					Mode.PackageLineNbr = package.LineNbr;
					Mode.PackageLineNbrUI = package.LineNbr;
					Basis.Graph.Packages.Current = package;
				}

				protected override void ClearState()
				{
					Mode.PackageLineNbr = null;
					Basis.Graph.Packages.Current = null;
				}

				protected override void ReportSuccess(CSBox entity) => Basis.ReportInfo(Msg.Ready, entity.BoxID);
				protected override void ReportMissing(string barcode) => Basis.ReportError(Msg.Missing, barcode);

				protected override void SetNextState()
				{
					if (Basis.Remove != true && !Mode.CanPack)
					{
						Basis.SetScanState(BuiltinScanStates.Command);

						if (!Mode.CanConfirmPackage)
							Basis.ReportInfo(PackMode.Msg.Completed, Basis.RefNbr);
					}
					else
					{
						base.SetNextState();
					}
				}

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string Prompt = "Scan the box barcode.";
					public const string Ready = "The {0} box is selected.";
					public const string Missing = "The {0} box is not found in the database.";
				}
				#endregion
			}

			public static class BoxConfirming
			{
				public sealed class StartState : MediatorState
				{
					public const string Value = "BCS";
					public class value : BqlString.Constant<value> { public value() : base(StartState.Value) { } }

					public override string Code => Value;

					protected override bool IsStateActive() => Get<PackMode.Logic>().CanConfirmPackage;

					protected override void Apply()
					{
						Basis.Clear<WeightState>();
						Basis.Clear<DimensionsState>();
					}

					protected override void SetNextState()
					{
						if (Get<PackMode.Logic>().HasSingleAutoPackage(Basis.RefNbr, out var _)) // package is already fully prepared
							Basis.SetScanState(BuiltinScanStates.Command, PackMode.Msg.Completed, Basis.RefNbr);
						else
							base.SetNextState();
					}
				}

				public sealed class WeightState : EntityState<decimal?>
				{
					public const string Value = "BWGT";
					public class value : BqlString.Constant<value> { public value() : base(WeightState.Value) { } }

					public Logic This => Get<Logic>();
					public PackMode.Logic Mode => Get<PackMode.Logic>();

					#region Flow
					public override string Code => Value;
					protected override string StatePrompt => Msg.Prompt;

					protected override void OnTakingOver()
					{
						if (This.TryPrepareWeightAndSkipInputFor(Mode.SelectedPackage))
							MoveToNextState();
					}

					protected override void OnDismissing() => Basis.RevokeQuestion<SkipQuestion>();

					protected override decimal? GetByBarcode(string barcode) => decimal.TryParse(barcode, out decimal value) ? value : (decimal?)null;

					protected override void ReportMissing(string barcode) => Basis.ReportError(Msg.BadFormat);

					protected override Validation Validate(decimal? value)
					{
						if (Basis.HasFault(value, base.Validate, out var fault))
							return fault;

						var package = Mode.SelectedPackage;

						if (Basis.IsValid<SOPackageDetailEx.weight, SOPackageDetailEx>(package, value.Value, out string error) == false)
							return Validation.Fail(error);

						return Validation.Ok;
					}

					protected override void Apply(decimal? value) => This.Weight = value.Value;

					protected override void ClearState() => (This.Weight, This.LastWeighingTime) = (null, null);

					protected override void ReportSuccess(decimal? value) => Basis.ReportInfo(Msg.Success, value.Value, Mode.SelectedPackage.WeightUOM);
					#endregion

					#region SkipQuestion
					public sealed class SkipQuestion : ScanQuestion
					{
						public Logic State => Get<Logic>();

						public override string Code => "SKIPWEIGHT";

						protected override string GetPrompt() => Msg.Prompt;

						protected override void Confirm()
						{
							if (State.TryUsePreparedWeightFor(State.Target.SelectedPackage, explicitConfirmation: true) && Basis.CurrentState is WeightState)
								Basis.DispatchNext();
						}

						protected override void Reject() { }

						#region Messages
						[PXLocalizable]
						public abstract class Msg
						{
							public const string Prompt = "To skip the weighing, click OK.";
						}
						#endregion
					}
					#endregion

					#region SkipScalesQuestion
					public sealed class SkipScalesQuestion : ScanQuestion
					{
						public Logic State => Get<Logic>();

						public override string Code => "SKIPSCALES";

						protected override string GetPrompt() => Msg.Prompt;

						protected override void Confirm()
						{
							var package = State.Target.SelectedPackage;

							if (State.SelectedScales.LastModifiedDateTime == State.LastWeighingTime)
							{
								// user wants not to use scale
								if (State.TryUsePreparedWeightFor(package) && Basis.CurrentState is WeightState)
									Basis.DispatchNext();
							}
							else
							{
								// user tried to use scales again
								if (State.ProcessScales(package) && Basis.CurrentState is WeightState)
									Basis.DispatchNext();
							}
						}

						protected override void Reject() { }

						#region Messages
						[PXLocalizable]
						public abstract class Msg
						{
							public const string Prompt = "Put the package on the scale and click OK. To skip the weighing, click OK without using the scale.";
						}
						#endregion
					}
					#endregion

					#region Logic
					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class Logic : ScanExtension<PackMode.Logic>
					{
						public PXSetupOptional<CommonSetup> CommonSetupUOM;

						public virtual double ScaleWeightValiditySeconds => 30;

						public SMScale SelectedScales
						{
							get
							{
								Guid? scaleDeviceID = UserSetup.For(Graph).ScaleDeviceID;
								Graph.Caches<SMScale>().ClearQueryCache();
								return SMScale.PK.Find(Basis, scaleDeviceID);
							}
						}

						#region Weight
						public decimal? Weight
						{
							get => Target.PackHeader.Weight;
							set => Target.PackSetter.Set(h => h.Weight, value);
						}
						#endregion
						#region LastWeighingTime
						public DateTime? LastWeighingTime
						{
							get => Target.PackHeader.LastWeighingTime;
							set => Target.PackSetter.Set(h => h.LastWeighingTime, value);
						}
						#endregion

						public virtual bool TryUsePreparedWeightFor(SOPackageDetailEx package, bool explicitConfirmation = false)
						{
							if (!explicitConfirmation && Basis.Setup.Current.ConfirmEachPackageWeight == true)
								return false; // explicit user input required

							if (!CanSkipInputFor(package))
							{
								Basis.ReportError(Msg.NoSkip);
								Basis.RevokeQuestion<SkipQuestion>();
								Basis.RevokeQuestion<SkipScalesQuestion>();
								return false;
							}
							else if (Weight != null && Basis.HasFault(Weight, w => Basis.TryValidate(w).By<WeightState>(), out var fault))
							{
								Basis.ReportError(fault.Message, fault.MessageArgs);
								Basis.RevokeQuestion<SkipQuestion>();
								Basis.RevokeQuestion<SkipScalesQuestion>();
								return false;
							}
							else
							{
								return true;
							}
						}

						public virtual bool TryPrepareWeightAndSkipInputFor(SOPackageDetailEx package)
						{
							Weight = package.Weight == 0
								? AutoCalculateBoxWeightBasedOnItems(package)
								: package.Weight.Value;

							if (UserSetup.For(Basis).UseScale == true && !ProcessScales(package))
								return false;

							Basis.ReportInfo(Msg.CalculatedWeight, package.BoxID, Weight, package.WeightUOM);

							if (Basis.Setup.Current.ConfirmEachPackageWeight == true)
							{
								if (CanSkipInputFor(package))
									AskToSkipFor(package);

								return false;
							}

							return CanSkipInputFor(package);
						}

						protected virtual bool CanSkipInputFor(SOPackageDetailEx package) => Weight.IsNotIn(null, 0) || package.Weight.IsNotIn(null, 0);

						protected virtual void AskToSkipFor(SOPackageDetailEx package)
						{
							if (Basis.HasFault(Weight, w => Basis.TryValidate(w).By<WeightState>(), out var _))
								Basis.Warn<SkipQuestion>(Msg.CalculatedWeight, package.BoxID, Weight, package.WeightUOM);
							else
								Basis.Ask<SkipQuestion>(Msg.CalculatedWeight, package.BoxID, Weight, package.WeightUOM);
						}

						protected virtual decimal AutoCalculateBoxWeightBasedOnItems(SOPackageDetailEx package)
						{
							decimal calculatedWeight = CSBox.PK.Find(Basis, package.BoxID)?.BoxWeight ?? 0m;
							SOShipLineSplitPackage[] links = Graph.PackageDetailExt.PackageDetailSplit.SelectMain(package.ShipmentNbr, package.LineNbr);
							foreach (var link in links)
							{
								var inventory = InventoryItem.PK.Find(Basis, link.InventoryID);
								calculatedWeight += (inventory.BaseWeight ?? 0) * (link.BasePackedQty ?? 0);
							}

							return Math.Round(calculatedWeight, 4);
						}

						public virtual bool ProcessScales(SOPackageDetailEx package)
						{
							SMScale scale = SelectedScales;

							if (scale == null)
							{
								Basis.ReportError(Msg.ScaleMissing, "");
								return false;
							}

							DateTime dbNow = GetServerTime();
							LastWeighingTime = scale.LastModifiedDateTime.Value;

							if (scale.LastModifiedDateTime.Value.AddHours(1) < dbNow)
							{
								Basis.ReportError(Msg.ScaleDisconnected, scale.ScaleID);
								return false;
							}

							if (scale.LastWeight.GetValueOrDefault() == 0)
							{
								Basis.Warn<SkipScalesQuestion>(Msg.ScaleNoBox, scale.ScaleID);
								return false;
							}

							if (scale.LastModifiedDateTime.Value.AddSeconds(ScaleWeightValiditySeconds) < dbNow)
							{
								Basis.ReportError(Msg.ScaleTimeout, scale.ScaleID, ScaleWeightValiditySeconds);
								return false;
							}

							var scaleConversion = scale.GetExtension<SM.SMScaleWeightConversion>();
							if (scaleConversion?.CompanyUOM == null)
							{
								Basis.ReportError(IN.Messages.BaseCompanyUomIsNotDefined);
								return false;
							}

							if (scaleConversion?.CompanyLastWeight == null)
							{
								Basis.ReportError(IN.Messages.MissingGlobalUnitConversion, scale.UOM, scaleConversion.CompanyUOM);
								return false;
							}

							Weight = scaleConversion.CompanyLastWeight ?? 0;
							return true;
						}

						protected virtual DateTime GetServerTime()
						{
							PXDatabase.SelectDate(out var _, out DateTime dbNow);
							dbNow = PXTimeZoneInfo.ConvertTimeFromUtc(dbNow, LocaleInfo.GetTimeZone());
							return dbNow;
						}

						/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanCommand(ScanCommand{TSelf})"/>
						[PXOverride]
						public ScanCommand<PickPackShip> DecorateScanCommand(ScanCommand<PickPackShip> original,
							Func<ScanCommand<PickPackShip>, ScanCommand<PickPackShip>> base_DecorateScanCommand)
						{
							var command = base_DecorateScanCommand(original);

							if (command is ConfirmPackageCommand confirmPackage)
								confirmPackage.Intercept.IsEnabled.ByConjoin(basis => !(basis.CurrentState is WeightState));

							return command;
						}
					}
					#endregion

					#region AlterComplete
					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class AlterComplete : ScanExtension<CompleteState.Logic>
					{
						/// Overrides <see cref="CompleteState.Logic.ApplyChanges(SOPackageDetailEx)"/>
						[PXOverride]
						public void ApplyChanges(SOPackageDetailEx package,
							Action<SOPackageDetailEx> base_ApplyChanges)
						{
							base_ApplyChanges(package);

							var state = Basis.Get<Logic>();

							if (state.Weight.IsNotIn(null, 0m))
								package.Weight = Math.Round(state.Weight.Value, 4);
						}

						/// Overrides <see cref="CompleteState.Logic.TryForwardProcessing"/>
						[PXOverride]
						public bool TryForwardProcessing(Func<bool> base_TryForwardProcessing)
						{
							if (Basis.CurrentState is WeightState)
								if (TryForward() == false)
									return false;

							if (base_TryForwardProcessing() == false)
								return false;

							if (Basis.CurrentState is WeightState)
								if (TryForward() == false)
									return false;

							return true;
						}

						/// Overrides <see cref="CompleteState.Logic.ClearStates"/>
						[PXOverride]
						public void ClearStates(Action base_ClearStates)
						{
							base_ClearStates();
							Basis.Clear<WeightState>();
						}

						protected virtual bool TryForward()
						{
							var state = Basis.Get<Logic>();

							if (state.TryUsePreparedWeightFor(state.Target.SelectedPackage) == false)
								return false;

							Basis.DispatchNext();
							return true;
						}
					}
					#endregion

					#region Messages
					[PXLocalizable]
					public abstract class Msg
					{
						public const string Prompt = "Enter the actual total weight of the package.";
						public const string BadFormat = "The quantity format does not fit the locale settings.";
						public const string Success = "Once the package is confirmed, it will have the following weight: {0} {1}.";

						public const string CalculatedWeight = "The {0} package is ready to be confirmed. The calculated weight is {1} {2}.";
						public const string NoSkip = "The package does not have a predefined weight.";

						public const string ScaleMissing = "The {0} scale is not found in the database.";
						public const string ScaleDisconnected = "No information from the {0} scales. Check connection of the scales.";
						public const string ScaleTimeout = "Measurement on the {0} scale is more than {1} seconds old. Remove the package from the scale and weigh it again.";
						public const string ScaleNoBox = "No information from the {0} scales. Make sure that items are on the scales.";
					}
					#endregion
				}

				public sealed class DimensionsState : EntityState<(decimal L, decimal W, decimal H)?>
				{
					public const string Value = "BDIM";
					public class value : BqlString.Constant<value> { public value() : base(DimensionsState.Value) { } }

					public Logic This => Get<Logic>();
					public PackMode.Logic Mode => Get<PackMode.Logic>();

					#region Flow
					public override string Code => Value;
					protected override string StatePrompt => Msg.Prompt;

					protected override bool IsStateSkippable() =>
						Basis.Setup.Current.ConfirmEachPackageDimensions == false ||
						Mode.SelectedPackage == null ||
						CSBox.PK.Find(Basis, Mode.SelectedPackage.BoxID)?.AllowOverrideDimension != true;

					protected override void OnTakingOver()
					{
						if (This.TryPrepareDimensionsAndSkipInputFor(Mode.SelectedPackage))
							MoveToNextState();
					}
					protected override void OnDismissing() => Basis.RevokeQuestion<SkipQuestion>();

					protected override (decimal L, decimal W, decimal H)? GetByBarcode(string barcode)
					{
						string[] dimensions = barcode.Trim().Split(' ');

						if (dimensions.Length < 3)
							return null;

						(string lengthStr, string widthStr, string heightStr) = dimensions;

						if (decimal.TryParse(lengthStr, out decimal lenght) &&
							decimal.TryParse(widthStr, out decimal width) &&
							decimal.TryParse(heightStr, out decimal height))
							return (lenght, width, height);

						return null;
					}

					protected override void ReportMissing(string barcode) => Basis.ReportError(Msg.BadFormat);

					protected override Validation Validate((decimal L, decimal W, decimal H)? value)
					{
						if (Basis.HasFault(value, base.Validate, out var fault))
							return fault;

						var package = Mode.SelectedPackage;

						if (Basis.IsValid<SOPackageDetailEx.length, SOPackageDetailEx>(package, value.Value.L, out string errorLength) == false)
							return Validation.Fail(errorLength);

						if (Basis.IsValid<SOPackageDetailEx.width, SOPackageDetailEx>(package, value.Value.W, out string errorWidth) == false)
							return Validation.Fail(errorWidth);

						if (Basis.IsValid<SOPackageDetailEx.height, SOPackageDetailEx>(package, value.Value.H, out string errorHeight) == false)
							return Validation.Fail(errorHeight);

						return Validation.Ok;
					}

					protected override void Apply((decimal L, decimal W, decimal H)? value) => This.Dimensions = value.Value;

					protected override void ClearState() => This.Dimensions = null;

					protected override void ReportSuccess((decimal L, decimal W, decimal H)? value)
						=> Basis.ReportInfo(Msg.Success, value.Value.L, value.Value.W, value.Value.H, Mode.SelectedPackage.LinearUOM);
					#endregion

					#region SkipQuestion
					public sealed class SkipQuestion : ScanQuestion
					{
						public Logic State => Get<Logic>();

						public override string Code => "SKIPDIMENSIONS";

						protected override string GetPrompt() => Msg.Prompt;

						protected override void Confirm()
						{
							if (State.TryUsePreparedDimensionsFor(State.Target.SelectedPackage, explicitConfirmation: true) && Basis.CurrentState is DimensionsState)
								Basis.DispatchNext();
						}

						protected override void Reject() { }

						#region Messages
						[PXLocalizable]
						public abstract class Msg
						{
							public const string Prompt = "To use the default dimensions, click OK.";
						}
						#endregion
					}
					#endregion

					#region Logic
					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class Logic : ScanExtension<PackMode.Logic>
					{
						#region Dimensions
						public (decimal L, decimal W, decimal H)? Dimensions
						{
							get
							{
								if (((decimal?)null).IsIn(Target.PackHeader.Length, Target.PackHeader.Width, Target.PackHeader.Height))
									return null;

								return
								(
									Target.PackHeader.Length.Value,
									Target.PackHeader.Width.Value,
									Target.PackHeader.Height.Value
								);
							}
							set
							{
								if (value == null)
								{
									Target.PackSetter.Set(h => h.Length, null);
									Target.PackSetter.Set(h => h.Width, null);
									Target.PackSetter.Set(h => h.Height, null);
								}
								else
								{
									Target.PackSetter.Set(h => h.Length, value.Value.L);
									Target.PackSetter.Set(h => h.Width, value.Value.W);
									Target.PackSetter.Set(h => h.Height, value.Value.H);
								}
							}
						}
						#endregion

						public virtual bool TryUsePreparedDimensionsFor(SOPackageDetailEx package, bool explicitConfirmation = false)
						{
							if (!explicitConfirmation && Basis.Setup.Current.ConfirmEachPackageDimensions == true)
								return false; // explicit user input required

							if (!CanSkipInputFor(package))
							{
								Basis.ReportError(Msg.NoSkip);
								Basis.RevokeQuestion<SkipQuestion>();
								return false;
							}
							else if (Dimensions != null && Basis.HasFault(Dimensions, dims => Basis.TryValidate(dims).By<DimensionsState>(), out var fault))
							{
								Basis.ReportError(fault.Message, fault.MessageArgs);
								Basis.RevokeQuestion<SkipQuestion>();
								return false;
							}
							else
							{
								return true;
							}
						}

						public virtual bool TryPrepareDimensionsAndSkipInputFor(SOPackageDetailEx package)
						{
							Dimensions = (package.Length ?? 0, package.Width ?? 0, package.Height ?? 0);

							Basis.ReportInfo(Msg.CalculatedDimensions, package.BoxID, Dimensions.Value.L, Dimensions.Value.W, Dimensions.Value.H, package.LinearUOM);

							if (Basis.Setup.Current.ConfirmEachPackageDimensions == true)
							{
								if (CanSkipInputFor(package))
									AskToSkipFor(package);

								return false;
							}

							return CanSkipInputFor(package);
						}

						protected virtual bool CanSkipInputFor(SOPackageDetailEx package) =>
							Dimensions.IsNotIn(null, (0m, 0m, 0m)) ||
							package.Length.IsNotIn(null, 0) &&
							package.Width.IsNotIn(null, 0) &&
							package.Height.IsNotIn(null, 0);

						protected virtual void AskToSkipFor(SOPackageDetailEx package)
						{
							if (Basis.HasFault(Dimensions, dims => Basis.TryValidate(dims).By<DimensionsState>(), out var _))
								Basis.Warn<SkipQuestion>(Msg.CalculatedDimensions, package.BoxID, Dimensions.Value.L, Dimensions.Value.W, Dimensions.Value.H, package.LinearUOM);
							else
								Basis.Ask<SkipQuestion>(Msg.CalculatedDimensions, package.BoxID, Dimensions.Value.L, Dimensions.Value.W, Dimensions.Value.H, package.LinearUOM);
						}

						/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanCommand(ScanCommand{TSelf})"/>
						[PXOverride]
						public ScanCommand<PickPackShip> DecorateScanCommand(ScanCommand<PickPackShip> original,
							Func<ScanCommand<PickPackShip>, ScanCommand<PickPackShip>> base_DecorateScanCommand)
						{
							var command = base_DecorateScanCommand(original);

							if (command is ConfirmPackageCommand confirmPackage)
								confirmPackage.Intercept.IsEnabled.ByConjoin(basis => !(basis.CurrentState is DimensionsState));

							return command;
						}
					}

					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					[PXUIField(DisplayName = Msg.PackageDimensionsCombined)]
					public class PackageDimensionsCombined : FieldAttached.To<SOPackageDetailEx>.AsString.Named<PackageDimensionsCombined>
					{
						public override string GetValue(SOPackageDetailEx Row) => $"{Row.Length} x {Row.Width} x {Row.Height} {Row.LinearUOM}";
					}
					#endregion

					#region AlterComplete
					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class AlterComplete : ScanExtension<CompleteState.Logic>
					{
						/// Overrides <see cref="CompleteState.Logic.ApplyChanges(SOPackageDetailEx)"/>
						[PXOverride]
						public void ApplyChanges(SOPackageDetailEx package,
							Action<SOPackageDetailEx> base_ApplyChanges)
						{
							base_ApplyChanges(package);

							var state = Basis.Get<Logic>();

							if (state.Dimensions.IsNotIn(null, (0m, 0m, 0m)))
							{
								package.Length = Math.Round(state.Dimensions.Value.L, 4);
								package.Width = Math.Round(state.Dimensions.Value.W, 4);
								package.Height = Math.Round(state.Dimensions.Value.H, 4);
							}
						}

						/// Overrides <see cref="CompleteState.Logic.TryForwardProcessing"/>
						[PXOverride]
						public bool TryForwardProcessing(Func<bool> base_TryForwardProcessing)
						{
							if (Basis.CurrentState is DimensionsState)
								if (TryForward() == false)
									return false;

							if (base_TryForwardProcessing() == false)
								return false;

							if (Basis.CurrentState is DimensionsState)
								if (TryForward() == false)
									return false;

							return true;
						}

						/// Overrides <see cref="CompleteState.Logic.ClearStates"/>
						[PXOverride]
						public void ClearStates(Action base_ClearStates)
						{
							base_ClearStates();
							Basis.Clear<DimensionsState>();
						}

						protected virtual bool TryForward()
						{
							var state = Basis.Get<Logic>();

							if (state.TryUsePreparedDimensionsFor(state.Target.SelectedPackage) == false)
								return false;

							Basis.DispatchNext();
							return true;
						}
					}
					#endregion

					#region Messages
					[PXLocalizable]
					public abstract class Msg
					{
						public const string Prompt = "Enter the actual length, width, and height of the package. Use a space as a separator.";
						public const string BadFormat = "The entered data has an incorrect format. The data input should contain three numeric dimensions separated by a space. Example: 31.2 20 13.5";
						public const string Success = "Once the package is confirmed, it will have the following dimensions: {0} x {1} x {2} {3}.";
						public const string NoSkip = "The package does not have predefined dimensions.";
						public const string CalculatedDimensions = "The {0} package is ready to be confirmed. It has the following default dimensions: {1} x {2} x {3} {4}.";
						public const string PackageDimensionsCombined = "Dimensions (L x W x H)";
					}
					#endregion
				}

				public sealed class CompleteState : MediatorState
				{
					public const string Value = "BCC";
					public class value : BqlString.Constant<value> { public value() : base(CompleteState.Value) { } }

					public override string Code => Value;

					protected override void Apply() => Get<Logic>().Call(state => state.SettleAndConfirmPackage(state.Target.SelectedPackage));

					protected override void SetNextState() => Basis.SetDefaultState();

					#region Logic
					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class Logic : ScanExtension<PackMode.Logic>
					{
						public virtual bool TryAutoConfirm()
						{
							if (!Basis.HasActive<StartState>())
								return true;

							Basis.SetScanState<StartState>();

							if (Target.SelectedPackage.Confirmed == true)
								return true;

							if (TryForwardProcessing() == false)
								return false;

							SettleAndConfirmPackage(Target.SelectedPackage);

							return true;
						}

						public virtual void SettleAndConfirmPackage(SOPackageDetailEx package)
						{
							ApplyChanges(package);

							package.Confirmed = true;
							Graph.Packages.Update(package);
							Basis.Save.Press();

							ClearStates();
							Basis.Reset(fullReset: false);
							Basis.ReportInfo(Msg.Success);
						}

						protected virtual bool TryForwardProcessing() => true;
						protected virtual void ApplyChanges(SOPackageDetailEx package) { }
						protected virtual void ClearStates() => Basis.Clear<BoxState>();
					}
					#endregion

					#region Messages
					[PXLocalizable]
					public abstract class Msg
					{
						public const string Success = "The package has been confirmed.";
					}
					#endregion
				}
			}

			public sealed class ConfirmState : ConfirmationState
			{
				public sealed override string Prompt => Basis.Localize(Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

				protected sealed override FlowStatus PerformConfirmation() => Get<Logic>().Confirm();

				#region Logic
				public class Logic : ScanExtension
				{
					protected PackMode.Logic Mode { get; private set; }
					public override void Initialize() => Mode = Basis.Get<PackMode.Logic>();

					public virtual FlowStatus Confirm()
					{
						bool locationIsRequired = Basis.HasActive<LocationState>();

						var nothingToDoError = FlowStatus.Fail(Basis.Remove == true ? Msg.NothingToRemove : Msg.NothingToPack);

						if (Mode.PackageLineNbr == null)
							return nothingToDoError;

						var packageDetail = Mode.SelectedPackage;

						if (Basis.InventoryID == null || Basis.Qty == 0)
							return nothingToDoError;

						void KeepPackageSelection() => Mode.PackageLineNbr = packageDetail.LineNbr;

						var packedSplits = Mode.PickedForPack.SelectMain().Where(r =>
							r.InventoryID == Basis.InventoryID &&
							r.SubItemID == Basis.SubItemID &&
							(r.IsUnassigned == true || r.HasGeneratedLotSerialNbr == true || string.Equals(r.LotSerialNbr, Basis.LotSerialNbr ?? r.LotSerialNbr, StringComparison.OrdinalIgnoreCase)) &&
							locationIsRequired.Implies(r.LocationID == (Basis.LocationID ?? r.LocationID)) &&
							(Basis.Remove == true ? r.PackedQty > 0 : TargetQty(r) > r.PackedQty));

						if (Basis.HasPick == false && Basis.Shipment?.PickedViaWorksheet == false)
							packedSplits = packedSplits
							.OrderByDescending(split => split.IsUnassigned == false && split.HasGeneratedLotSerialNbr == false)
							.OrderByDescending(split => Basis.Remove == true ? split.PickedQty > 0 : split.Qty > split.PickedQty)
							.ThenByDescending(split => string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
							.ThenByDescending(split => string.IsNullOrEmpty(split.LotSerialNbr))
							.ThenByDescending(split => (split.Qty > split.PickedQty || Basis.Remove == true) && split.PickedQty > 0)
							.ThenByDescending(split => Sign.MinusIf(Basis.Remove == true) * (split.Qty - split.PickedQty));
						if (packedSplits.Any() == false)
							return nothingToDoError.WithModeReset.WithPostAction(KeepPackageSelection);

						decimal qty = Basis.BaseQty;
						string inventoryCD = Basis.SightOf<WMSScanHeader.inventoryID>();

						if (Basis.Remove == true ? packedSplits.Sum(s => s.PackedQty) - qty < 0 : packedSplits.Sum(s => TargetQty(s) - s.PackedQty) < qty)
							return FlowStatus.Fail(Basis.Remove == true ? Msg.BoxCanNotUnpack : Msg.BoxCanNotPack, inventoryCD, Basis.Qty, Basis.UOM);

						decimal unassignedQty = Sign.MinusIf(Basis.Remove == true) * qty;
						foreach (var packedSplit in packedSplits)
						{
							decimal currentQty = Basis.Remove == true
								? -Math.Min(packedSplit.PackedQty.Value, -unassignedQty)
								: +Math.Min(TargetQty(packedSplit).Value - packedSplit.PackedQty.Value, unassignedQty);

							if (PackSplit(packedSplit, packageDetail, currentQty) == false)
								return FlowStatus.Fail(Basis.Remove == true ? Msg.BoxCanNotUnpack : Msg.BoxCanNotPack, inventoryCD, Basis.Qty, Basis.UOM);

							unassignedQty -= currentQty;
							if (unassignedQty == 0)
								break;
						}

						if (Mode.IsPackageEmpty(packageDetail))
						{
							Basis.Graph.Packages.Delete(packageDetail);
							Basis.Clear<BoxState>();
							Mode.PackageLineNbrUI = null;
						}
						else
						{
							Mode.PackageLineNbrUI = Mode.PackageLineNbr;
						}

						EnsureShipmentUserLinkForPack();

						Basis.ReportInfo(
							Basis.Remove == true ? Msg.InventoryRemoved : Msg.InventoryAdded,
							Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

						return FlowStatus.Ok.WithDispatchNext;
					}

					public virtual decimal? TargetQty(SOShipLineSplit split) => Basis.HasPick ? split.PickedQty : split.Qty * Basis.Graph.GetQtyThreshold(split);

					protected virtual bool IsSelectedSplit(SOShipLineSplit split)
					{
						return
							split.InventoryID == Basis.InventoryID &&
							split.SubItemID == Basis.SubItemID &&
							split.SiteID == Basis.SiteID &&
							split.LocationID == (Basis.LocationID ?? split.LocationID) &&
							string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase);
					}

					public virtual bool PackSplit(SOShipLineSplit split, SOPackageDetailEx package, decimal qty)
					{
						if (Basis.HasPick)
							Basis.EnsureAssignedSplitEditing(split);
						else if (split.IsUnassigned == true)
						{
							var existingSplit = Mode.PickedForPack.SelectMain().FirstOrDefault(s =>
								s.LineNbr == split.LineNbr &&
								s.IsUnassigned == false &&
								string.Equals(s.LotSerialNbr, Basis.LotSerialNbr ?? s.LotSerialNbr, StringComparison.OrdinalIgnoreCase) &&
								IsSelectedSplit(s));

							if (existingSplit == null)
							{
								var newSplit = PXCache<SOShipLineSplit>.CreateCopy(split);
								newSplit.SplitLineNbr = null;
								newSplit.LotSerialNbr = Basis.LotSerialNbr;
								newSplit.ExpireDate = Basis.ExpireDate;
								newSplit.Qty = qty;
								newSplit.PickedQty = qty;
								newSplit.PackedQty = 0;
								newSplit.IsUnassigned = false;
								newSplit.PlanID = null;

								split = Mode.PickedForPack.Insert(newSplit);
							}
							else
							{
								existingSplit.Qty += qty;
								existingSplit.ExpireDate = Basis.ExpireDate;
								split = Mode.PickedForPack.Update(existingSplit);
							}
						}
						else if (split.HasGeneratedLotSerialNbr == true)
						{
							var donorSplit = PXCache<SOShipLineSplit>.CreateCopy(split);
							if (donorSplit.Qty == qty)
							{
								Mode.PickedForPack.Delete(donorSplit);
							}
							else
							{
								donorSplit.Qty -= qty;
								donorSplit.PickedQty -= Math.Min(qty, donorSplit.PickedQty.Value);
								Mode.PickedForPack.Update(donorSplit);
							}

							var existingSplit = Mode.PickedForPack.SelectMain().FirstOrDefault(s =>
								s.LineNbr == split.LineNbr &&
								s.HasGeneratedLotSerialNbr == false &&
								string.Equals(s.LotSerialNbr, Basis.LotSerialNbr ?? s.LotSerialNbr, StringComparison.OrdinalIgnoreCase) &&
								IsSelectedSplit(s));

							if (existingSplit == null)
							{
								var newSplit = PXCache<SOShipLineSplit>.CreateCopy(split);
								newSplit.SplitLineNbr = null;
								newSplit.LotSerialNbr = Basis.LotSerialNbr;
								if (Basis.ExpireDate != null)
									newSplit.ExpireDate = Basis.ExpireDate;
								newSplit.Qty = qty;
								newSplit.PickedQty = qty;
								newSplit.PackedQty = 0;
								newSplit.PlanID = null;

								split = Mode.PickedForPack.Insert(newSplit);

								split.HasGeneratedLotSerialNbr = false;
								split = Mode.PickedForPack.Update(split);
							}
							else
							{
								existingSplit.Qty += qty;
								existingSplit.PickedQty += qty;
								if (Basis.ExpireDate != null)
									existingSplit.ExpireDate = Basis.ExpireDate;
								split = Mode.PickedForPack.Update(existingSplit);
							}
						}

						SOShipLineSplitPackage link = Graph.PackageDetailExt.PackageDetailSplit
							.SelectMain(package.ShipmentNbr, package.LineNbr)
							.FirstOrDefault(t =>
								t.ShipmentNbr == split.ShipmentNbr
								&& t.ShipmentLineNbr == split.LineNbr
								&& t.ShipmentSplitLineNbr == split.SplitLineNbr);

						if (qty < 0)
						{
							if (link == null || link.PackedQty + qty < 0)
								return false;

							if (!Basis.HasPick && Basis.IsEnterableLotSerial(isForIssue: true, isForTransfer: Basis.IsTransfer))
							{
								if (Basis.SelectedLotSerialClass.AutoNextNbr == true)
								{
									if (split.PackedQty + qty == 0)
									{
										split.HasGeneratedLotSerialNbr = true;
										split = Mode.PickedForPack.Update(split);
									}
								}
								else
								{
								split.Qty += qty;
								if (split.Qty == 0)
									split = Mode.PickedForPack.Delete(split);
								else
									split = Mode.PickedForPack.Update(split);
							}
							}

							if (link.PackedQty + qty > 0)
							{
								link.PackedQty += qty;
								Graph.PackageDetailExt.PackageDetailSplit.Update(link);
							}
							else if (link.PackedQty + qty == 0)
							{
								Graph.PackageDetailExt.PackageDetailSplit.Delete(link);
							}

							package.Confirmed = false;
							Graph.Packages.Update(package);
						}
						else
						{
							if (link == null)
							{
								link = (SOShipLineSplitPackage)Base.PackageDetailExt.PackageDetailSplit.Cache.CreateInstance();

								PXFieldVerifying ver = (c, a) => a.Cancel = true;
								Graph.FieldVerifying.AddHandler<SOShipLineSplitPackage.shipmentSplitLineNbr>(ver);
								link.ShipmentSplitLineNbr = split.SplitLineNbr;
								link.PackedQty = qty;
								link = Graph.PackageDetailExt.PackageDetailSplit.Insert(link);
								Graph.FieldVerifying.RemoveHandler<SOShipLineSplitPackage.shipmentSplitLineNbr>(ver);

								link.ShipmentNbr = split.ShipmentNbr;
								link.ShipmentLineNbr = split.LineNbr;
								link.PackageLineNbr = package.LineNbr;
								link.InventoryID = split.InventoryID;
								link.SubItemID = split.SubItemID;
								link.LotSerialNbr = split.LotSerialNbr;
								link.UOM = split.UOM;

								link = Graph.PackageDetailExt.PackageDetailSplit.Update(link);
							}
							else
							{
								link.PackedQty += qty;
								Graph.PackageDetailExt.PackageDetailSplit.Update(link);
							}
						}

						return true;
					}

					public virtual void EnsureShipmentUserLinkForPack()
					{
						Graph.WorkLogExt.EnsureFor(
							Basis.RefNbr,
							Graph.Accessinfo.UserID,
							Basis.HasPick == true
								? SOShipmentProcessedByUser.jobType.Pack
								: SOShipmentProcessedByUser.jobType.PackOnly);
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string Prompt = "Confirm packing {0} x {1} {2}.";

					public const string NothingToPack = "No items to pack.";
					public const string NothingToRemove = "No items to remove from the shipment.";

					public const string InventoryAdded = "{0} x {1} {2} has been added to the shipment.";
					public const string InventoryRemoved = "{0} x {1} {2} has been removed from the shipment.";

					public const string BoxCanNotPack = "Cannot pack {1} {2} of {0}.";
					public const string BoxCanNotUnpack = "Cannot unpack {1} {2} of {0}.";
				}
				#endregion
			}
			#endregion

			#region Commands
			public sealed class ConfirmPackageCommand : ScanCommand
			{
				public PackMode.Logic Mode => Get<PackMode.Logic>();

				public override string Code => "PACKAGE*CONFIRM";
				public override string ButtonName => "scanConfirmPackage";
				public override string DisplayName => Msg.DisplayName;
				protected override bool IsEnabled => Mode.CanConfirmPackage;
				protected override bool Process()
				{
					Basis.SetScanState<BoxConfirming.StartState>();
					return true;
				}

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string DisplayName = "Confirm Package";
				}
				#endregion
			}

			public sealed class RemoveCommand : WMSBase.RemoveCommand
			{
				protected override bool IsEnabled => base.IsEnabled && Get<PackMode.Logic>().HasConfirmableBoxes;
			}
			#endregion

			#region Redirect
			public sealed class RedirectFrom<TForeignBasis> : WMSBase.RedirectFrom<TForeignBasis>.SetMode<PackMode>
				where TForeignBasis : PXGraphExtension, IBarcodeDrivenStateMachine
			{
				public override string Code => PackMode.Value;
				public override string DisplayName => Msg.Description;

				private string RefNbr { get; set; }

				public override bool IsPossible
				{
					get
					{
						bool wmsFulfillment = PXAccess.FeatureInstalled<CS.FeaturesSet.wMSFulfillment>();
						var ppsSetup = SOPickPackShipSetup.PK.Find(Basis.Graph, Basis.Graph.Accessinfo.BranchID);
						return wmsFulfillment && ppsSetup?.ShowPackTab == true;
					}
				}

				protected override bool PrepareRedirect()
				{
					if (Basis is PickPackShip pps && pps.RefNbr != null && pps.DocumentIsConfirmed == false)
					{
						if (pps.FindMode<PackMode>().TryValidate(pps.Shipment).By<ShipmentState>() is Validation valid && valid.IsError == true)
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
				public const string Description = "Pack";
				public const string PackedQtyPerBox = "Packed Qty.";

				public const string Completed = "The {0} shipment is packed.";
				public const string CannotBePacked = "The {0} shipment cannot be processed in the Pack mode because the shipment has two or more packages assigned.";

				public const string BoxConfirmPrompt = "Confirm the package.";
				public const string BoxConfirmOrContinuePrompt = "Confirm package or scan the next item.";
				public const string BoxConfirmOrContinuePromptNoPick = "Confirm package, or scan the next item or the next location.";
			}
			#endregion

			#region Attached Fields
			[PXUIField(Visible = false)]
			public class ShowPack : FieldAttached.To<ScanHeader>.AsBool.Named<ShowPack>
			{
				public override bool? GetValue(ScanHeader row) => Base.WMS.Get<PackMode.Logic>().ShowPackTab(row) == true;
			}

			[PXUIField(DisplayName = Msg.PackedQtyPerBox)]
			public class PackedQtyPerBox : FieldAttached.To<SOShipLineSplit>.AsDecimal.Named<PackedQtyPerBox>
			{
				public override decimal? GetValue(SOShipLineSplit row)
				{
					SOShipLineSplitPackage package = Base.PackageDetailExt.PackageDetailSplit
						.SelectMain(Base.WMS.RefNbr, Base.WMS.Get<PackMode.Logic>().PackageLineNbrUI)
						.FirstOrDefault(t => t.ShipmentSplitLineNbr == row.SplitLineNbr);
					return package?.PackedQty ?? 0m;
				}
			}
			#endregion
		}
	}

	/// <exclude/>
	public sealed class PackScanHeader : PXCacheExtension<WMSScanHeader, QtyScanHeader, ScanHeader>
	{
		#region PackageLineNbr
		[PXInt]
		[PXFormula(typeof(Null.When<WMSScanHeader.refNbr.IsNull>.Else<packageLineNbr>))]
		public int? PackageLineNbr { get; set; }
		public abstract class packageLineNbr : BqlInt.Field<packageLineNbr> { }
		#endregion
		#region PackageLineNbrUI
		[PXInt]
		[PXUIField(DisplayName = "Package")]
		[PXSelector(
			typeof(SearchFor<SOPackageDetailEx.lineNbr>.Where<SOPackageDetailEx.shipmentNbr.IsEqual<WMSScanHeader.refNbr.FromCurrent>>),
			typeof(BqlFields.FilledWith<
				SOPackageDetailEx.confirmed,
				SOPackageDetailEx.lineNbr,
				SOPackageDetailEx.boxID,
				SOPackageDetailEx.boxDescription,
				SOPackageDetailEx.weight,
				SOPackageDetailEx.maxWeight,
				SOPackageDetailEx.weightUOM,
				SOPackageDetailEx.carrierBox,
				SOPackageDetailEx.length,
				SOPackageDetailEx.width,
				SOPackageDetailEx.height>),
			DescriptionField = typeof(SOPackageDetailEx.boxID), DirtyRead = true, SuppressUnconditionalSelect = true)]
		[PXFormula(typeof(packageLineNbr.When<packageLineNbr.IsNotNull>.Else<packageLineNbrUI>))]
		public int? PackageLineNbrUI { get; set; }
		public abstract class packageLineNbrUI : BqlInt.Field<packageLineNbrUI> { }
		#endregion

		#region Weight
		[PXDecimal(6)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public decimal? Weight { get; set; }
		public abstract class weight : BqlDecimal.Field<weight> { }
		#endregion
		#region LastWeighingTime
		[PXDate]
		public DateTime? LastWeighingTime { get; set; }
		public abstract class lastWeighingTime : BqlDateTime.Field<lastWeighingTime> { }
		#endregion

		#region Length
		[PXDecimal(6)]
		public decimal? Length { get; set; }
		public abstract class length : BqlDecimal.Field<length> { }
		#endregion
		#region Width
		[PXDecimal(6)]
		public decimal? Width { get; set; }
		public abstract class width : BqlDecimal.Field<width> { }
		#endregion
		#region Height
		[PXDecimal(6)]
		public decimal? Height { get; set; }
		public abstract class height : BqlDecimal.Field<height> { }
		#endregion
	}
}
