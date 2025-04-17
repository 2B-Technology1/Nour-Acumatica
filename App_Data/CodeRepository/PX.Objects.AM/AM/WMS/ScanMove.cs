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

using PX.BarcodeProcessing;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AM.Attributes;
using PX.Objects.IN;
using PX.Objects.IN.WMS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM
{
	using WMSBase = ScanProductionBase<ScanMove, ScanMove.Host, AMDocType.move>;

	public class ScanMove : WMSBase
	{
		public override bool UseDefaultWarehouse => UserSetup.For(Graph).DefaultWarehouse == true;
		protected override bool UseQtyCorrectection => Setup.Current.UseDefaultQtyInMove != true;
		protected bool PromptLocationForEveryLine => Setup.Current.RequestLocationForEachItemInMove == true;
		protected bool UseRemainingQty => Setup.Current.UseRemainingQtyInMove == true;
		public override bool ExplicitConfirmation => Setup.Current.ExplicitLineConfirmation == true;
		protected override bool CanOverrideQty => (!DocumentLoaded || NotReleasedAndHasLines) && (SelectedLotSerialClass?.LotSerTrack == INLotSerTrack.SerialNumbered).Implies(SelectedLotSerialClass?.LotSerAssign == INLotSerAssign.WhenUsed);

		public class Host : MoveEntry { }

		public new class QtySupport : WMSBase.QtySupport { }
		public new class UserSetup : WMSBase.UserSetup { }

		#region Overrides
		protected override IEnumerable<ScanMode<ScanMove>> CreateScanModes() { yield return new MoveMode();  }

		protected virtual void _(Events.RowInserting<AMMTranAttribute> e)
		{
			e.Cancel = true;
		}

		#endregion
		public sealed class MoveMode : ScanMode
		{
			public const string Value = "MOVE";
			public class value : BqlString.Constant<value> { public value() : base(MoveMode.Value) { } }
			public override string Code => Value;
			public override string Description => Msg.Description;

			protected override IEnumerable<ScanState<ScanMove>> CreateStates()
			{
				yield return new OrderTypeState().Intercept.IsStateActive.ByConjoin(basis => basis.UseDefaultOrderType() == false);
				yield return new ProdOrdState();
				yield return new OperationState();
				yield return new WarehouseState();
				yield return new LocationState().Intercept.IsStateActive.ByConjoin(basis => basis.PromptLocationForEveryLine == true || basis.LocationID == null);
				yield return new LotSerialState().Intercept.IsStateActive.ByConjoin(basis => basis.IsLastOperation() && basis.IsLotSerialRequired());
				yield return new ExpireDateState().Intercept.IsStateActive.ByConjoin(basis => basis.IsLastOperation() && basis.IsExpirationDateRequired());
				yield return new ConfirmState();
			}

			protected override IEnumerable<ScanTransition<ScanMove>> CreateTransitions()
			{
				return StateFlow(flow => flow
					.From<OrderTypeState>()
					.NextTo<ProdOrdState>()
					.NextTo<OperationState>()
					.NextTo<WarehouseState>()
					.NextTo<LocationState>()
					.NextTo<LotSerialState>()
					.NextTo<ExpireDateState>()
					.NextTo<ConfirmState>()
				);
			}

			protected override IEnumerable<ScanCommand<ScanMove>> CreateCommands()
			{
				return new ScanCommand<ScanMove>[]
				{
					new RemoveCommand(),
					new QtySupport.SetQtyCommand(),
					new ReleaseCommand()
				};
			}

			protected override IEnumerable<ScanRedirect<ScanMove>> CreateRedirects() => AllWMSRedirects.CreateFor<ScanMove>();

			protected override void ResetMode(bool fullReset = false)
			{
				Clear<OrderTypeState>();
				Clear<ProdOrdState>();
				Clear<OperationState>();
				Clear<WarehouseState>();
				Clear<LocationState>(when: fullReset || Basis.PromptLocationForEveryLine);
				Clear<InventoryItemState>(when: fullReset);
				Clear<LotSerialState>();
				Clear<ExpireDateState>();
			}


			#region States

			public sealed class ConfirmState : ConfirmationState
			{
				public override string Prompt => Basis.Localize(Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

				protected override FlowStatus PerformConfirmation() => Get<Logic>().Confirm();

				#region Logic
				public class Logic : ScanExtension
				{
					public virtual FlowStatus Confirm()
					{
						if (!CanConfirm(out var error))
							return error;

						return Basis.Remove == true
							? ConfirmRemove()
							: ConfirmAdd();
					}

					protected virtual FlowStatus ConfirmAdd()
					{
						var lsClass = Basis.SelectedLotSerialClass;

						bool newDocument = Basis.Batch == null;
						if (newDocument)
						{
							Basis.BatchView.Insert();
							Basis.BatchView.Current.NoteID = Basis.NoteID;
						}

						AMMTran existTransaction = FindMoveRow();

						decimal? newQty = Basis.Qty;

						if (existTransaction != null)
						{
							newQty += existTransaction.Qty;

							if (Basis.CurrentMode.HasActive<LotSerialState>() &&
								lsClass.LotSerTrack == INLotSerTrack.SerialNumbered &&
								newQty != 1) // TODO: use base qty
							{
								return FlowStatus.Fail(Msg.SerialItemNotComplexQty);
							}

							Basis.Details.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
							existTransaction = Basis.Details.Update(existTransaction);
						}
						else
						{
							existTransaction = Basis.Details.Insert();
							Basis.Details.Cache.SetValueExt<AMMTran.orderType>(existTransaction, Basis.OrderType);
							Basis.Details.Cache.SetValueExt<AMMTran.prodOrdID>(existTransaction, Basis.ProdOrdID);
							Basis.Details.Cache.SetValueExt<AMMTran.operationID>(existTransaction, Basis.OperationID);
							existTransaction = Basis.Details.Update(existTransaction);
							Basis.Details.Cache.SetValueExt<AMMTran.siteID>(existTransaction, Basis.SiteID);
							Basis.Details.Cache.SetValueExt<AMMTran.locationID>(existTransaction, Basis.LocationID);
							Basis.Details.Cache.SetValueExt<AMMTran.uOM>(existTransaction, Basis.UOM);
							Basis.Details.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
							Basis.Details.Cache.SetValueExt<AMMTran.lotSerialNbr>(existTransaction, Basis.LotSerialNbr);
							if (lsClass.LotSerTrackExpiration == true && Basis.ExpireDate != null)
								Basis.Details.Cache.SetValueExt<AMMTran.expireDate>(existTransaction, Basis.ExpireDate);
							existTransaction = Basis.Details.Update(existTransaction);

							if (HasErrors(existTransaction, out var error))
							{
								Base.transactions.Delete(existTransaction);
								return error;
							}

						}

						if (!string.IsNullOrEmpty(Basis.LotSerialNbr))
						{
							foreach (AMMTranSplit split in Basis.Graph.splits.Select())
							{
								Basis.Graph.splits.Cache.SetValueExt<AMMTranSplit.expireDate>(split, Basis.ExpireDate ?? existTransaction.ExpireDate);
								Basis.Graph.splits.Cache.SetValueExt<AMMTranSplit.lotSerialNbr>(split, Basis.LotSerialNbr);
								Basis.Graph.splits.Update(split);
							}
						}

						Basis.ReportInfo(
							Msg.InventoryAdded,
							Basis.SightOf<WMSScanHeader.inventoryID>(),
							Basis.Qty,
							Basis.UOM);

						if (Basis.BatchView.Cache.GetStatus(Basis.BatchView.Current) == PXEntryStatus.Inserted)
							return FlowStatus.Ok.WithDispatchNext.WithSaveSkip;
						else
							return FlowStatus.Ok.WithDispatchNext;

					}

					protected virtual bool HasErrors(AMMTran tran, out FlowStatus error)
					{
						error = FlowStatus.Ok;
						return false;
					}


					protected virtual FlowStatus ConfirmRemove()
					{
						AMMTran existTransaction = FindMoveRow();

						if (existTransaction == null)
							return FlowStatus.Fail(Msg.LineMissing, Basis.SelectedInventoryItem.InventoryCD);

						if (existTransaction.Qty == Basis.Qty)
						{
							Basis.Details.Delete(existTransaction);
						}
						else
						{
							var newQty = existTransaction.Qty - Basis.Qty;

							if (!Basis.IsValid<AMMTran.qty, AMMTran>(existTransaction, newQty, out string error))
								return FlowStatus.Fail(error);

							Basis.Details.Cache.SetValueExt<AMMTran.qty>(existTransaction, newQty);
							Basis.Details.Update(existTransaction);
						}

						Basis.ReportInfo(
							Msg.InventoryRemoved,
							Basis.SightOf<WMSScanHeader.inventoryID>(),
							Basis.Qty,
							Basis.UOM);

						if (Basis.BatchView.Cache.GetStatus(Basis.BatchView.Current) == PXEntryStatus.Inserted)
							return FlowStatus.Ok.WithDispatchNext.WithSaveSkip;
						else
							return FlowStatus.Ok.WithDispatchNext;
					}

					protected virtual AMMTran FindMoveRow()
					{
						var existTransactions = Basis.Details.SelectMain().Where(t =>
							t.OrderType == Basis.OrderType &&
							t.ProdOrdID == Basis.ProdOrdID &&
							t.OperationID == Basis.OperationID &&
							t.InventoryID == Basis.InventoryID &&
							t.SiteID == Basis.SiteID &&
							t.LocationID == (Basis.LocationID ?? t.LocationID));

						AMMTran existTransaction = null;

						if (Basis.CurrentMode.HasActive<LotSerialState>())
						{
							foreach (var tran in existTransactions)
							{
								Basis.Details.Current = tran;
								if (Basis.Graph.splits.SelectMain().Any(t => string.Equals(t.LotSerialNbr ?? "", Basis.LotSerialNbr ?? "", System.StringComparison.OrdinalIgnoreCase)))
								{
									existTransaction = tran;
									break;
								}
							}
						}
						else
						{
							existTransaction = existTransactions.FirstOrDefault();
						}

						return existTransaction;
					}

					protected virtual bool CanConfirm(out FlowStatus error)
					{
						if(Basis.Batch?.Released == true)
						{
							error = FlowStatus.Fail(PX.Objects.IN.Messages.Document_Status_Invalid);
							return false;
						}
						if(!Basis.InventoryID.HasValue)
						{
							error = FlowStatus.Fail(Msg.InventoryNotSet);
							return false;
						}
						if (Basis.SelectedLotSerialClass.LotSerTrack == INLotSerTrack.SerialNumbered && Basis.Qty != 1)
						{
							error = FlowStatus.Fail(Msg.SerialItemNotComplexQty);
							return false;
						}
						error = FlowStatus.Ok;
						return true;
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public new abstract class Msg
				{
					public const string Prompt = "Confirm movement of Item {0} x {1} {2}.";
					public const string InventoryNotSet = "The item is not selected.";
					public const string SerialItemNotComplexQty = "Serialized items can be processed only with the base UOM and the 1.00 quantity.";
					public const string LineMissing = "The {0} item is not found in the batch.";
					public const string InventoryRemoved = "{0} x {1} {2} has been removed from the batch.";
					public const string InventoryAdded = "{0} x {1} {2} has been added.";
				}
				#endregion
			}

			public sealed new class OperationState : OperationStateBase<ScanMove>
			{
				protected override string OrderType { get => Basis.OrderType; set => Basis.OrderType = value; }
				protected override string ProdOrdID { get => Basis.ProdOrdID; set => Basis.ProdOrdID = value; }
				protected override int? OperationID { get => Basis.OperationID; set => Basis.OperationID = value; }
				protected bool IsLastOperation { get => Basis.LastOperationID != null && Basis.LastOperationID == Basis.OperationID; }
				protected bool HasDefaultedQty { get; set; }
				protected bool HasDefaultedLocationID { get; set; }
				protected decimal DefaultQty => 1m;

				protected override void Apply(AMProdOper oper)
				{
					base.Apply(oper);

					if (Basis.UseRemainingQty && (Basis.Qty == DefaultQty || Basis.UseQtyCorrectection))
					{
						Basis.Qty = oper?.QtyRemaining ?? DefaultQty;
						HasDefaultedQty = true;
					}

					if (IsLastOperation != true)
					{
						var prodItem = AMProdItem.PK.Find(Basis.Graph, OrderType, ProdOrdID);
						if (prodItem != null)
						{
							Basis.SiteID = prodItem.SiteID;
							if (!Basis.PromptLocationForEveryLine)
							{
								Basis.LocationID = prodItem.LocationID;
								HasDefaultedLocationID = true;
							}
						}
					}
				}

				protected override void ClearState()
				{
					base.ClearState();
					if (HasDefaultedQty)
					{
						Basis.Qty = DefaultQty;
						HasDefaultedQty = false;
					}
					if (HasDefaultedLocationID)
					{
						Basis.LocationID = null;
						HasDefaultedLocationID = false;
					}
				}
			}
			#endregion

			#region Commands
			public new sealed class ReleaseCommand : WMSBase.ReleaseCommand
			{
				protected override string DocumentReleasing => Msg.DocumentReleasing;
				protected override string DocumentIsReleased => Msg.DocumentIsReleased;
				protected override string DocumentReleaseFailed => Msg.DocumentReleaseFailed;

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : WMSBase.ReleaseCommand.Msg
				{
					public const string DocumentReleasing = "The {0} move is being released.";
					public const string DocumentIsReleased = "The move transaction has been successfully released.";
					public const string DocumentReleaseFailed = "The move release failed.";
				}
				#endregion
			}
			#endregion

			#region Messages
			[PXLocalizable]
			public new abstract class Msg
			{
				public const string Description = "Scan Move";
			}
			#endregion
		}
	}

}
