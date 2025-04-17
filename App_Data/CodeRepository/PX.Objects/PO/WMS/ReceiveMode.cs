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
using System.Threading;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.BarcodeProcessing;

using PX.Objects.Common;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.IN.WMS;

namespace PX.Objects.PO.WMS
{
	using WMSBase = WarehouseManagementSystem<ReceivePutAway, ReceivePutAway.Host>;

	public partial class ReceivePutAway : WMSBase
	{
		public sealed class ReceiveMode : ScanMode
		{
			public const string Value = "RCPT";
			public class value : BqlString.Constant<value> { public value() : base(ReceiveMode.Value) { } }

			public ReceiveMode.Logic Body => Get<ReceiveMode.Logic>();

			public override string Code => Value;
			public override string Description => Msg.Description;

			#region State Machine
			protected override IEnumerable<ScanState<ReceivePutAway>> CreateStates()
			{
				yield return new ReceiptState();
				yield return new SpecifyWarehouseState();
				yield return new LocationState()
					.Intercept.IsStateSkippable.ByDisjoin(basis => basis.LocationID != null)
					.Intercept.IsStateSkippable.ByDisjoin(basis => basis.Get<ReceiveMode.Logic>().IsLocationUserInputRequired == false);
				yield return new InventoryItemState() { AlternateType = INPrimaryAlternateType.VPN }
					.Intercept.StatePrompt.ByOverride(
						(basis, base_StatePrompt) =>
						{
							if (basis.Remove == true || basis.PrevInventoryID == null || basis.Get<Logic>().CanReceiveOriginalLines)
								return base_StatePrompt();
							else
								return ConfirmState.Msg.ItemOrReceiptPrompt;
						})
					.Intercept.HandleAbsence.ByAppend(
						(basis, barcode) =>
						{
							if (basis.Get<Logic>().IsSingleLocation == false && basis.TryProcessBy<LocationState>(barcode, StateSubstitutionRule.KeepPositiveReports | StateSubstitutionRule.KeepApplication))
								return AbsenceHandling.Done;
							return AbsenceHandling.Skipped;
						})
					.Intercept.HandleAbsence.ByAppend(
						(basis, barcode) =>
						{
							if (basis.TryProcessBy<ReceiptState>(barcode, StateSubstitutionRule.KeepAbsenceHandling))
							{
								basis.Reset(fullReset: true);
								basis.SetScanState<ReceiptState>();
								if (basis.FindState<ReceiptState>().Process(barcode))
									return AbsenceHandling.Done;
							}
							return AbsenceHandling.Skipped;
						});
				yield return new LotSerialState()
					.Intercept.IsStateActive.ByConjoin(basis => basis.SelectedLotSerialClass.LotSerAssign == INLotSerAssign.WhenReceived)
					.Intercept.IsStateSkippable.ByDisjoin(basis => basis.Get<ReceiveMode.Logic>().IsLotSerialUserInputRequired == false);
				yield return new ExpireDateState()
					.Intercept.IsStateSkippable.ByDisjoin(basis => basis.Get<ReceiveMode.Logic>().With(m => m.IsLotSerialUserInputRequired == false || m.IsExpireDateUserInputRequired == false));
				yield return new ConfirmState();
			}

			protected override IEnumerable<ScanTransition<ReceivePutAway>> CreateTransitions()
			{
				if (!Body.IsSingleLocation && Body.PromptLocationForEveryLine)
				{
					return StateFlow(flow => flow
						.From<ReceiptState>()
						.NextTo<InventoryItemState>()
						.NextTo<LotSerialState>()
						.NextTo<ExpireDateState>()
						.NextTo<LocationState>());
				}
				else
				{
					return StateFlow(flow => flow
						.From<ReceiptState>()
						.NextTo<LocationState>() // read it only once
						.NextTo<InventoryItemState>()
						.NextTo<LotSerialState>()
						.NextTo<ExpireDateState>());
				}
			}

			protected override IEnumerable<ScanCommand<ReceivePutAway>> CreateCommands()
			{
				yield return new RemoveCommand();
				yield return new QtySupport.SetQtyCommand();
				yield return new ReleaseReceiptCommand();
				yield return new ReleaseReceiptAndCompletePOLinesCommand();
			}

			protected override IEnumerable<ScanQuestion<ReceivePutAway>> CreateQuestions()
			{
				yield return new ConfirmState.OverreceiveQuestion();
			}

			protected override IEnumerable<ScanRedirect<ReceivePutAway>> CreateRedirects() => AllWMSRedirects.CreateFor<ReceivePutAway>();

			protected override void ResetMode(bool fullReset)
			{
				base.ResetMode(fullReset);

				Clear<ReceiptState>(when: fullReset && !Basis.IsWithinReset);
				Clear<LocationState>(when: fullReset || Body.PromptLocationForEveryLine && Body.IsSingleLocation == false);
				Clear<InventoryItemState>(when: fullReset);
				Clear<LotSerialState>();
				Clear<ExpireDateState>();

				if (fullReset)
				{
					Basis.PONbr = null;
					Basis.PrevInventoryID = null;
				}
			}
			#endregion

			#region Logic
			public class Logic : ScanExtension
			{
				public virtual bool CanReceiveOriginalLines
				{
					get
					{
						if (Basis.Receipt?.WMSSingleOrder == true)
						{
							var newReceiptLines = Basis.Graph.transactions.Cache.Inserted.ToArray<POReceiptLine>();
							if (newReceiptLines.Length <= 1)
							{
								var poLineNbr = newReceiptLines.Length == 0
									? null
									: newReceiptLines[0].POLineNbr;
								POLineR anyLine =
									SelectFrom<POLineR>.
									Where<
										POLineR.orderType.IsEqual<POOrderType.regularOrder>.
										And<POLineR.orderNbr.IsEqual<@P.AsString>>.
										And<@P.AsInt.IsNull.Or<POLineR.lineNbr.IsNotEqual<@P.AsInt>>>.
										And<POLineR.orderQty.IsGreater<CS.decimal0>>.
										And<POLineR.orderQty.IsGreater<POLineR.receivedQty>>>.
									View.ReadOnly.SelectWindowed(Basis, 0, 1, Basis.Receipt.OrigPONbr, poLineNbr, poLineNbr);
								if (anyLine != null)
									return true;
							}
						}
						var splits = Received.SelectMain();
						return !splits.Any() || splits.Any(s => s.ReceivedQty < s.Qty || GetExtendedRestQty(s) > 0);
					}
				}

				public virtual bool IsUnboundSplit(POReceiptLineSplit s) => s.PONbr == null && Graph.Document.Current.ReceiptType != POReceiptType.TransferReceipt;

				#region Views
				public
					SelectFrom<POReceiptLineSplit>.
					InnerJoin<POReceiptLine>.On<POReceiptLineSplit.FK.ReceiptLine>.
					View Received;
				public virtual IEnumerable received() => Basis.SortedResult(Basis.GetSplits(Basis.Receipt, includeUnassigned: true, sortByBeingProcessed: false, RowProcessedPredicate));

				public
					SelectFrom<POReceiptLineSplit>.
					InnerJoin<POReceiptLine>.On<POReceiptLineSplit.FK.ReceiptLine>.
					View ReceivedNotZero;
				public virtual IEnumerable receivedNotZero() => Basis.SortedResult(Basis.GetSplits(Basis.Receipt, includeUnassigned: true, sortByBeingProcessed: false, RowProcessedPredicate).Where(s => ((POReceiptLineSplit)s).ReceivedQty > 0));

				private Func<POReceiptLineSplit, bool> RowProcessedPredicate
				{
					get
					{
						if (Basis.Receipt?.WMSSingleOrder == true)
							return (r => GetNormalRestQty(r) == 0);
						else
							return (r => r.ReceivedQty == r.Qty);
					}
				}
				#endregion

				#region Buttons
				public PXAction<ScanHeader> ReviewReceive;
				[PXButton(CommitChanges = true), PXUIField(DisplayName = "Review")]
				protected virtual IEnumerable reviewReceive(PXAdapter adapter) => adapter.Get();
				#endregion

				#region Event Handlers
				protected virtual void _(Events.RowSelected<ScanHeader> e)
				{
					ReviewReceive.SetVisible(Base.IsMobile && e.Row?.Mode == ReceiveMode.Value);
				}
				#endregion

				#region Configuration
				public bool PromptLocationForEveryLine => Basis.Setup.Current.RequestLocationForEachItemInReceive == true;
				public bool IsSingleLocation => UserSetup.For(Basis).SingleLocation == true;

				public bool IsLocationUserInputRequired => Basis.HasActive<LocationState>() && Basis.DefaultLocationID == null;

				public bool UseDefaultLotSerial => UserSetup.For(Basis).DefaultLotSerialNumber == true;
				public bool IsLotSerialUserInputRequired => Basis.HasActive<LotSerialState>() && (UseDefaultLotSerial == false || Basis.SelectedLotSerialClass.AutoNextNbr == false);

				public bool UseDefaultExpireDate => UserSetup.For(Basis).DefaultExpireDate == true;
				public bool IsExpireDateUserInputRequired => Basis.HasActive<ExpireDateState>() && (UseDefaultExpireDate == false || EnsureExpireDateDefault() == null);
				public DateTime? EnsureExpireDateDefault()
				{
					DateTime? expireDate = Basis.EnsureExpireDateDefault();
					if (expireDate != null)
						return expireDate;

					var sameLines = Received.SelectMain()
						.Where(s =>
							s.InventoryID == Basis.InventoryID &&
							string.Equals(s.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase));

					if (UseDefaultExpireDate == false)
						sameLines = sameLines.Where(s => s.ReceivedQty > 0);

					return sameLines.Select(s => s.ExpireDate).FirstOrDefault();
				}
				#endregion

				#region SplitQuantities
				public decimal GetOverallReceivedQty(POReceiptLineSplit split) => GetSplitQuantities(split).overallReceivedQty;
				public decimal GetNormalReceivedQty(POReceiptLineSplit split) => GetSplitQuantities(split).normalReceiptQty;
				public decimal GetNormalRestQty(POReceiptLineSplit split) => GetSplitQuantities(split).normalRestQty;
				public decimal GetExtendedReceivedQty(POReceiptLineSplit split) => GetSplitQuantities(split).extendedReceiptQty;
				public decimal GetExtendedRestQty(POReceiptLineSplit split) => GetSplitQuantities(split).extendedRestQty;

				protected (decimal overallReceivedQty, decimal normalReceiptQty, decimal normalRestQty, decimal extendedReceiptQty, decimal extendedRestQty) GetSplitQuantities(POReceiptLineSplit split)
				{
					var row = (PXResult<POReceiptLine, POLine>)
						SelectFrom<POReceiptLine>.
						LeftJoin<POLine>.On<POReceiptLine.FK.OrderLine>.
						Where<POReceiptLine.receiptType.IsEqual<@P.AsString.ASCII>.
							And<POReceiptLine.receiptNbr.IsEqual<@P.AsString>>.
							And<POReceiptLine.lineNbr.IsEqual<@P.AsInt>>>.
						View.Select(Basis, split.ReceiptType, split.ReceiptNbr, split.LineNbr);
					(var line, var poLine) = row;
					var inventoryItem = InventoryItem.PK.Find(Basis, line.InventoryID);
					var lotSerClass = INLotSerClass.PK.Find(Basis, inventoryItem?.LotSerClassID);
					bool isDirectReceiptLine = poLine == null || poLine.OrderNbr == null;

					decimal? ReceivedQty(POReceiptLineSplit rls, POReceiptLine rl) => rl.Released == true
						? rls.BaseQty * rls.InvtMult
						: rls.BaseReceivedQty;

					decimal overallReceivedQty = isDirectReceiptLine
						? SelectFrom<POReceiptLineSplit>.
							Where<POReceiptLineSplit.FK.ReceiptLine.SameAsCurrent>.
							View.SelectMultiBound(Basis, new[] { line })
							.AsEnumerable()
							.Sum(s => ReceivedQty(s, line) ?? 0)
						: SelectFrom<POReceiptLineSplit>.
							InnerJoin<POReceiptLine>.On<POReceiptLineSplit.FK.ReceiptLine>.
							Where<POReceiptLine.FK.OrderLine.SameAsCurrent>.
							View.SelectMultiBound(Basis, new[] { poLine })
							.AsEnumerable()
							.Sum(s => ReceivedQty(s, s.GetItem<POReceiptLine>()) ?? 0);

					if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && lotSerClass.LotSerAssign == INLotSerAssign.WhenReceived)
						return (overallReceivedQty, 1, 1 - split.ReceivedQty.Value, 1, 1 - split.ReceivedQty.Value);

					decimal normalReceiptQty = isDirectReceiptLine
						? line.BaseQty.Value
						: poLine.BaseOrderQty.Value;
					decimal restQty = PXDBQuantityAttribute.Round(Math.Max(0, normalReceiptQty - overallReceivedQty));

					decimal extendedReceiptQty = isDirectReceiptLine
						? 0
						: poLine.BaseOrderQty.Value * (poLine.RcptQtyMax.Value - 100) / 100m;
					decimal extendedRestQty = PXDBQuantityAttribute.Round(extendedReceiptQty + (normalReceiptQty - overallReceivedQty));

					return (overallReceivedQty, normalReceiptQty, restQty, extendedReceiptQty, extendedRestQty);
				}
				#endregion
			}
			#endregion

			#region States
			public sealed class ReceiptState : ReceivePutAway.ReceiptState
			{
				public bool OrderComesFirst { get; set; }
				public int LargeOrderLinesCount { get; set; } = 15;

				public Logic Body => Get<Logic>();

				protected override POReceipt GetByBarcode(string barcode)
				{
					POReceipt receipt =
						SelectFrom<POReceipt>.
						LeftJoin<Vendor>.On<POReceipt.vendorID.IsEqual<Vendor.bAccountID>>.SingleTableOnly.
						Where<
							POReceipt.receiptNbr.IsEqual<@P.AsString>.
							And<POReceipt.receiptType.IsIn<POReceiptType.poreceipt, POReceiptType.transferreceipt>>.
							And<
								Vendor.bAccountID.IsNull.
								Or<Match<Vendor, AccessInfo.userName.FromCurrent>>>>.
						OrderBy<Desc<TestIf<POReceipt.receiptType.IsEqual<POReceiptType.poreceipt>>>>. // receipts come first
						View.ReadOnly.Select(Basis, barcode);
					return receipt;
				}

				protected override Validation Validate(POReceipt receipt)
				{
					Body.SiteID = receipt.SiteID;
					if (Body.IsNewReceipt == false)
					{
						if (receipt.Released != false || receipt.Hold == true && receipt.WMSSingleOrder != true)
							return Validation.Fail(Msg.InvalidStatus, receipt.ReceiptNbr, Basis.SightOf<POReceipt.status>(receipt));

						if (receipt.ReceiptType.IsNotIn(POReceiptType.POReceipt, POReceiptType.TransferReceipt))
							return Validation.Fail(Msg.InvalidType, receipt.ReceiptNbr, Basis.SightOf<POReceipt.receiptType>(receipt));

						if (receipt.POType == POOrderType.DropShip)
							return Validation.Fail(Msg.InvalidOrderType, receipt.ReceiptNbr, Basis.SightOf<POReceipt.pOType>(receipt));

						if (Basis.HasSingleSiteInLines(receipt, out int? singleSiteID) == false)
							return Validation.Fail(Msg.MultiSites, receipt.ReceiptNbr);
						else if (receipt.SiteID == null)
							Body.SiteID = singleSiteID;

						if (Basis.HasNonStockKit(receipt))
							Validation.Fail(Msg.HasNonStockKit, receipt.ReceiptNbr);
					}
					return Validation.Ok;
				}

				protected override void Apply(POReceipt receipt)
				{
					int? defaultLocationID = Basis.Setup.Current.DefaultReceivingLocation == true
						? INSite.PK.Find(Basis, Body.SiteID)?.ReceiptLocationID
						: null;

					Basis.SiteID = Body.SiteID;
					Basis.LocationID = Basis.DefaultLocationID = defaultLocationID;
					base.Apply(receipt);
				}

				protected override void ClearState()
				{
					Basis.SiteID = null;
					Basis.DefaultLocationID = null;
					base.ClearState();
				}

				protected override void ReportSuccess(POReceipt receipt) => Basis.ReportInfo(Body.IsNewReceipt ? Msg.ReadyNew : Msg.Ready, receipt.ReceiptNbr);

				#region Logic
				public class Logic : ScanExtension
				{
					public bool IsNewReceipt { get; private set; } = false;
					public int? SiteID { get; set; } = null;

					public virtual POReceipt GetReceiptByBarcode(string barcode) => (POReceipt)PXSelectorAttribute.Select<WMSScanHeader.refNbr>(Basis.HeaderView.Cache, Basis.Header, barcode);

					public virtual bool TryGetReceiptByOrder(string barcode, out POReceipt receipt)
					{
						receipt = null;

						POOrder order = POOrder.PK.Find(Basis, POOrderType.RegularOrder, barcode);
						if (order == null)
							return false;

						if (!string.IsNullOrEmpty(Basis.PONbr) && !Basis.PONbr.Equals(order.OrderNbr, StringComparison.OrdinalIgnoreCase)
							|| !string.IsNullOrEmpty(Basis.RefNbr) && Basis.Receipt?.Released == true)
						{
							Basis.ReceiptType = null;
							Basis.RefNbr = null;
							Basis.PONbr = null;
							Basis.SiteID = null;
						}

						if (order.Status != POOrderStatus.Open)
						{
							Basis.ReportError(Msg.POOrderInvalid, order.OrderNbr);
							return false;
						}

						int nonStockKitLinesCount =
							SelectFrom<POLine>.
							InnerJoin<InventoryItem>.On<POLine.FK.InventoryItem>.
							Where<
								POLine.FK.Order.SameAsCurrent.
								And<InventoryItem.stkItem.IsEqual<False>>.
								And<InventoryItem.kitItem.IsEqual<True>>>.
							AggregateTo<Count>.
							View.SelectSingleBound(Basis, new[] { order }).RowCount ?? 0;
						if (nonStockKitLinesCount != 0)
						{
							Basis.ReportError(Msg.POOrderHasNonStocKit, order.OrderNbr);
							return false;
						}

						var poSplitsGrouped =
							SelectFrom<POLine>.
							Where<POLine.FK.Order.SameAsCurrent.And<POLine.siteID.IsNotNull>>.
							AggregateTo<GroupBy<POLine.siteID>>.
							View.SelectMultiBound(Basis, new[] { order });
						if (poSplitsGrouped.Count == 1)
							SiteID = ((POLine)poSplitsGrouped).SiteID;
						else
						{
							SiteID = Basis.SiteID;
							if (SiteID == null)
								Basis.SiteID = SiteID = Basis.DefaultSiteID;

							if (SiteID == null)
							{
								Basis.PONbr = order.OrderNbr;

								Basis.SetScanState<SpecifyWarehouseState>();

								return true;
							}
						}

						receipt =
							SelectFrom<POReceipt>.
							InnerJoin<POOrderReceipt>.On<POOrderReceipt.FK.Receipt>.
							LeftJoin<Vendor>.On<POReceipt.FK.Vendor>.SingleTableOnly.
							Where<
								POOrderReceipt.FK.Order.SameAsCurrent.
								And<POReceipt.released.IsEqual<False>>.
								And<Vendor.bAccountID.IsNull.Or<Match<Vendor, AccessInfo.userName.FromCurrent>>>.
								And<POReceipt.wMSSingleOrder.IsEqual<False>.Or<POReceipt.createdByID.IsEqual<AccessInfo.userID.FromCurrent>>>.
								And<
									POReceipt.siteID.IsEqual<@P.AsInt>.
									Or<Not<Exists<
										SelectFrom<POReceiptLineSplit>.
										Where<
											POReceiptLineSplit.FK.Receipt.
											And<POReceiptLineSplit.siteID.IsNotNull>.
											And<POReceiptLineSplit.siteID.IsNotEqual<@P.AsInt>>>>>>>>.
							OrderBy<POReceipt.wMSSingleOrder.Desc>.
							View.SelectSingleBound(Basis, new[] { order }, SiteID, SiteID);

						if (receipt == null)
						{
							try
							{
								Basis.PONbr = order.OrderNbr;

								int linesCount =
									SelectFrom<POLine>.
									Where<POLine.FK.Order.SameAsCurrent>.
									AggregateTo<Count>.
									View.SelectMultiBound(Basis, new[] { order }).RowCount ?? 0;

								receipt = CreateNewReceiptForOrder(order, linesCount > Basis.FindState<ReceiptState>().LargeOrderLinesCount);
							}
							catch (Exception e)
							{
								Basis.PONbr = null;
								HandleReceiptCreationError(order, e);
								return true;
							}
						}
						return true;
					}

					protected virtual POReceipt CreateNewReceiptForOrder(POOrder order, bool createEmpty)
					{
						var receipt = createEmpty
							? Base.CreateEmptyReceiptFrom(order)
							: Basis.Graph.CreateReceiptFrom(order);

						receipt.OrigPONbr = order.OrderNbr;
						receipt.WMSSingleOrder = true;
						receipt.SiteID = SiteID;
						receipt.AutoCreateInvoice = false;
						receipt.Hold = true; // we can create the receipt only on Hold
						receipt.Status = POReceiptStatus.Hold;
						receipt = Basis.Graph.Document.Current = Basis.Graph.Document.Update(receipt);

						foreach (POReceiptLineSplit rLine in Basis.Graph.splits.Cache.Inserted)
						{
							INLotSerClass lsClass = Basis.GetLotSerialClassOf(InventoryItem.PK.Find(Basis, rLine.InventoryID));
							if (lsClass != null && lsClass.LotSerTrack.IsIn(INLotSerTrack.LotNumbered, INLotSerTrack.SerialNumbered) && lsClass.LotSerTrackExpiration == true && lsClass.AutoNextNbr == true && rLine.ExpireDate == null)
							{
								DateTime? expireDate =
									SelectFrom<INItemLotSerial>.
									Where<INItemLotSerial.inventoryID.IsEqual<POReceiptLineSplit.inventoryID.FromCurrent>>.
									OrderBy<
										Desc<True.When<INItemLotSerial.lotSerialNbr.IsEqual<POReceiptLineSplit.lotSerialNbr.FromCurrent>>.Else<False>>,
										Desc<INItemLotSerial.expireDate>>.
									View.SelectSingleBound(Basis, new[] { rLine })
									.RowCast<INItemLotSerial>()
									.FirstOrDefault()?
									.ExpireDate ?? Basis.Graph.Accessinfo.BusinessDate;
								Basis.Graph.splits.Cache.SetValueExt<POReceiptLineSplit.expireDate>(rLine, expireDate);
							}

							rLine.Qty = 0;
							Basis.Graph.splits.Cache.Update(rLine);
						}

						Basis.SaveChanges();

						IsNewReceipt = true;
						return receipt;
					}

					protected virtual void HandleReceiptCreationError(POOrder order, Exception exception)
					{
						PXTrace.WriteError(exception);

						string errorMsg = Basis.Localize(Msg.POOrderUnableToCreateReceipt, order.OrderNbr) + Environment.NewLine + exception.Message;
						if (exception is PXOuterException outerEx)
						{
							if (outerEx.InnerMessages.Length > 0)
								errorMsg += Environment.NewLine + string.Join(Environment.NewLine, outerEx.InnerMessages);
							else if (outerEx.Row != null)
								errorMsg += Environment.NewLine + string.Join(Environment.NewLine, PXUIFieldAttribute.GetErrors(Basis.Graph.Caches[outerEx.Row.GetType()], outerEx.Row).Select(kvp => kvp.Value));
						}

						Basis.Graph.Clear();
						Basis.ReportError(errorMsg);
					}

					/// Overrides <see cref="BarcodeDrivenStateMachine{TSelf, TGraph}.DecorateScanState(ScanState{TSelf})"/>
					[PXOverride]
					public ScanState<ReceivePutAway> DecorateScanState(ScanState<ReceivePutAway> original,
						Func<ScanState<ReceivePutAway>, ScanState<ReceivePutAway>> base_DecorateScanState)
					{
						var state = base_DecorateScanState(original);

						if (state is ReceiveMode.ReceiptState receipt)
							InjectReceiptFetchByOrder(receipt);

						return state;
					}

					public virtual void InjectReceiptFetchByOrder(ReceiptState receiptState)
					{
						bool isSecondTry = false;

						receiptState
							.Intercept.GetByBarcode.ByOverride(
								(basis, barcode, base_GetReceiptByBarcode) =>
								{
									if (!isSecondTry && receiptState.OrderComesFirst)
										return null; // on the first try forces HandleAbsence, that is placed below, to work
									else
										return base_GetReceiptByBarcode(barcode);
								})
							.Intercept.HandleAbsence.ByAppend(
								(basis, barcode) =>
								{
									if (isSecondTry)
										return AbsenceHandling.Skipped; // this handler should work only on first try

									if (basis.Get<Logic>().TryGetReceiptByOrder(barcode, out var receipt))
										return AbsenceHandling.ReplaceWith(receipt);

									if (receiptState.OrderComesFirst)
									{
										isSecondTry = true;

										try
										{
											// the original receipt search was not called previously - lets try it now
											if (basis.TryProcessBy<ReceiptState>(barcode, StateSubstitutionRule.KeepAll))
												return AbsenceHandling.Done;
										}
										finally
										{
											isSecondTry = false;
										}
									}

									return AbsenceHandling.Skipped;
								});
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : ReceivePutAway.ReceiptState.Msg
				{
					public const string ReadyNew = "New receipt has been created and ready for processing.";

					public const string POOrderInvalid = "Status of the {0} order is not valid for processing.";
					public const string POOrderMultiSites = "All items in the {0} order must refer to the same warehouse.";
					public const string POOrderHasNonStocKit = "The {0} order cannot be processed because it contains a non-stock kit item.";
					public const string POOrderUnableToCreateReceipt = "Cannot create a purchase receipt for the {0} purchase order. Create a purchase receipt manually.";
				}
				#endregion
			}

			public sealed class SpecifyWarehouseState : WarehouseState
			{
				protected override bool UseDefaultWarehouse => false;

				protected override void SetNextState()
				{
					Basis.SetScanState<ReceiptState>();
					if (Basis.CurrentState.Code == ReceiptState.Value)
						Basis.CurrentState.Process(Basis.PONbr);
				}
			}

			public sealed class ConfirmState : ConfirmationState
			{
				public override string Prompt => Basis.Localize(Msg.Prompt, Basis.SightOf<WMSScanHeader.inventoryID>(), Basis.Qty, Basis.UOM);

				protected override FlowStatus PerformConfirmation() => Get<Logic>().Confirm();

				public class OverreceiveQuestion : ScanQuestion
				{
					public override string Code => "OVERRECEIVE";

					protected override string GetPrompt() => Basis.Localize(Msg.Prompt, Basis.BaseExcessQty, Basis.SelectedInventoryItem.BaseUnit);

					protected override void Confirm()
					{
						Basis.ForceInsertLine = true;
						if (Basis.CurrentState is ConfirmState confirmState)
							confirmState.Confirm();
					}

					protected override void Reject() { }

					#region Messages
					[PXLocalizable]
					public abstract class Msg
					{
						public const string Prompt = "The following quantity of the item will be added to a receipt line that is not linked to a purchase order: {0} {1}. To confirm, click OK.";
					}
					#endregion
				}

				#region Logic
				public class Logic : ScanExtension
				{
					public ReceiveMode.Logic Mode { get; private set; }
					public override void Initialize() => Mode = Basis.Get<ReceiveMode.Logic>();

					public virtual FlowStatus Confirm()
					{
						if (!CanReceive(out var error))
							return error;

						if (Basis.Remove == false)
							return ProcessAdd();
						else
							return ProcessRemove();
					}

					protected virtual bool CanReceive(out FlowStatus error)
					{
						if (Basis.InventoryID == null)
						{
							error = FlowStatus.Fail(InventoryItemState.Msg.NotSet).WithModeReset;
							return false;
						}

						if (Mode.IsLocationUserInputRequired && Basis.LocationID == null)
						{
							error = FlowStatus.Fail(LocationState.Msg.NotSet).WithModeReset;
							return false;
						}

						if (Mode.IsLotSerialUserInputRequired && Basis.LotSerialNbr == null)
						{
							error = FlowStatus.Fail(LotSerialState.Msg.NotSet).WithModeReset;
							return false;
						}

						error = FlowStatus.Ok;
						return true;
					}

					protected virtual FlowStatus ProcessAdd()
					{
						decimal qty = Basis.BaseQty;

						if (Mode.IsLotSerialUserInputRequired && Basis.ReceiveBySingleItem && qty != 1)
							return FlowStatus.Fail(InventoryItemState.Msg.SerialItemNotComplexQty);

						if (Basis.CurrentMode.HasActive<ExpireDateState>() && Mode.UseDefaultExpireDate == false && Basis.ExpireDate == null)
							return FlowStatus.Fail(ExpireDateState.Msg.NotSet).WithModeReset;

						if (Basis.LocationID == null && Basis.DefaultLocationID != null)
							Basis.LocationID = Basis.DefaultLocationID;

						if (!Basis.EnsureLocationPrimaryItem(Basis.InventoryID, Basis.LocationID))
							return FlowStatus.Fail(IN.Messages.NotPrimaryLocation);

						var itemSplits = Mode.Received.SelectMain()
							.Where(s =>
								s.InventoryID == Basis.InventoryID &&
								s.SubItemID == Basis.SubItemID);

						if (Basis.Receipt.WMSSingleOrder == true && Basis.Receipt.OrigPONbr != null && itemSplits.Any() == false)
						{
							Basis.Graph.AddPurchaseOrder(POOrder.PK.Find(Basis, POOrderType.RegularOrder, Basis.Receipt.OrigPONbr), Basis.InventoryID, Basis.SubItemID);
							itemSplits = Mode.Received.Cache.Inserted.Cast<POReceiptLineSplit>();
						}

						itemSplits = itemSplits
							.OrderByDescending(s => string.Equals(s.LotSerialNbr, Basis.LotSerialNbr ?? s.LotSerialNbr, StringComparison.OrdinalIgnoreCase))
							.ThenByDescending(s => s.LocationID == (Basis.LocationID ?? s.LocationID))
							.ToArray();

						if (Mode.IsLotSerialUserInputRequired && Basis.ReceiveBySingleItem && itemSplits.Any(s => string.Equals(s.LotSerialNbr, Basis.LotSerialNbr ?? s.LotSerialNbr, StringComparison.OrdinalIgnoreCase) && s.ReceivedQty == 1))
							return FlowStatus.Fail(InventoryItemState.Msg.SerialItemNotComplexQty).WithModeReset;

						var spreader = GetQuantitySpreader(qty, itemSplits);

						Basis.ForceInsertLine = Basis.ForceInsertLine == true && Basis.PrevInventoryID == Basis.InventoryID;
						if (spreader.NewLineIsNeeded && Basis.ForceInsertLine == false)
						{
							Basis.BaseExcessQty = spreader.BaseExcessQty;
							Basis.PrevInventoryID = Basis.InventoryID;
							return FlowStatus.Warn<OverreceiveQuestion>(Msg.QtyExceedsReceipt, Basis.RefNbr, Basis.SelectedInventoryItem.InventoryCD);
						}

						spreader.Spread();

						Basis.PrevInventoryID = Basis.InventoryID;
						Basis.BaseExcessQty = 0;

						Basis.ReportInfo(Msg.InventoryAdded, Basis.SelectedInventoryItem.InventoryCD, Basis.Qty, Basis.UOM);

						return FlowStatus.Ok.WithDispatchNext;
					}

					protected virtual FlowStatus ProcessRemove()
					{
						if (Basis.LocationID == null && Basis.DefaultLocationID != null)
							Basis.LocationID = Basis.DefaultLocationID;

						var allSplits = Mode.Received.SelectMain()
							.Where(r =>
								r.InventoryID == Basis.InventoryID &&
								r.SubItemID == Basis.SubItemID)
							.ToArray();

						bool IsDeductable(POReceiptLineSplit r) =>
							string.Equals(r.LotSerialNbr, Basis.LotSerialNbr ?? r.LotSerialNbr, StringComparison.OrdinalIgnoreCase) &&
							r.LocationID == (Basis.LocationID ?? r.LocationID);

						var deductableSplits = allSplits.Where(IsDeductable).ToArray();
						if (deductableSplits.Any() == false)
							return FlowStatus.Fail(Msg.NothingToRemove).WithModeReset;

						decimal qty = Basis.BaseQty;

						if (deductableSplits.Sum(s => s.ReceivedQty.Value) - qty < 0)
							return FlowStatus.Fail(Msg.Underreceiving);

						if (Basis.ReceiveBySingleItem)
						{
							var serialSplit = deductableSplits.Reverse().First(s => s.ReceivedQty != 0);

							if (Mode.IsUnboundSplit(serialSplit))
								Mode.Received.Delete(serialSplit);
							else
							{
								serialSplit.ReceivedQty = 0;
								Mode.Received.Update(serialSplit);
							}
						}
						else
						{
							decimal undeductedQty = qty;

							var splitGroups = allSplits.Reverse().GroupBy(s => s.LineNbr).ToDictionary(g => g.Key, g => g.OrderByDescending(IsDeductable).ToArray());
							foreach (var group in splitGroups.OrderByDescending(kvp => kvp.Key))
							{
								decimal groupUndeductedQty = Math.Min(group.Value.Where(IsDeductable).Sum(s => s.ReceivedQty.Value), undeductedQty);
								if (groupUndeductedQty == 0)
									continue;

								var removedSplits = new HashSet<int>();

								bool isQtyRearranged = group.Value.Any(Mode.IsUnboundSplit);

								decimal groupCurrentQty = groupUndeductedQty;
								decimal rearrangeQty = 0;
								for (int i = 0; i < group.Value.Length; i++)
								{
									var split = group.Value[i];
									if (IsDeductable(split) == false)
										break;

									decimal currentQty = Math.Min(split.ReceivedQty.Value, groupCurrentQty);
									if (i == group.Value.LastIndex() && !Mode.IsUnboundSplit(split))
									{
										split.Qty += rearrangeQty;
										split.ReceivedQty -= currentQty;
										Mode.Received.Update(split);
										isQtyRearranged = true;
										break;
									}
									else if (currentQty < split.ReceivedQty)
									{
										split.ReceivedQty -= currentQty;
										Mode.Received.Update(split);
									}
									else if (Mode.IsUnboundSplit(split))
									{
										Basis.Graph.transactions.Delete(Basis.Graph.transactions.Search<POReceiptLine.lineNbr>(split.LineNbr));
									}
									else
									{
										removedSplits.Add(split.SplitLineNbr.Value);
										rearrangeQty += split.Qty.Value;
										Mode.Received.Delete(split);
									}

									groupCurrentQty -= currentQty;
									if (groupCurrentQty == 0)
										break;
								}

								if (isQtyRearranged == false)
								{
									var donorSplit = group.Value.FirstOrDefault(s => IsDeductable(s) == false) ?? group.Value.First(s => s.SplitLineNbr.Value.IsNotIn(removedSplits));
									donorSplit.Qty += rearrangeQty;
									Mode.Received.Update(donorSplit);
								}

								undeductedQty -= groupUndeductedQty;
								if (undeductedQty == 0)
									break;
							}
						}

						Basis.ReportInfo(Msg.InventoryRemoved, Basis.SelectedInventoryItem.InventoryCD, Basis.Qty, Basis.UOM);

						return FlowStatus.Ok.WithDispatchNext;
					}

					protected virtual QuantitySpreader GetQuantitySpreader(decimal qty, IEnumerable<POReceiptLineSplit> itemSplits)
					{
						var spreader = Basis.Get<QuantitySpreader>();
						spreader.Prepare(qty, itemSplits);
						return spreader;
					}

					// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
					public class QuantitySpreader : ScanExtension
					{
						protected ReceivePutAway.ReceiveMode.Logic Mode { get; set; }

						protected decimal RestQty { get; set; }

						protected IEnumerable<POReceiptLineSplit> ExactSplits { get; set; }
						protected IEnumerable<POReceiptLineSplit> DonorSplits { get; set; }
						protected IEnumerable<POReceiptLineSplit> UnassignedSplits { get; set; }
						protected List<POReceiptLineSplit> AcceptorSplits { get; set; }

						public bool NewLineIsNeeded => BaseExcessQty > 0;
						public decimal BaseExcessQty { get; protected set; }
						protected decimal GetExtendedRestQty(POReceiptLineSplit split) => Mode.GetExtendedRestQty(split);

						public virtual void Prepare(decimal qtyToSpread, IEnumerable<POReceiptLineSplit> itemSplits)
						{
							Mode = Basis.Get<ReceivePutAway.ReceiveMode.Logic>();

							RestQty = qtyToSpread;

							(UnassignedSplits, itemSplits) = itemSplits.DisuniteBy(s => s.IsUnassigned == true);

							DonorSplits = itemSplits
								.Where(s =>
									Mode.IsUnboundSplit(s) ||
									s.Qty - s.ReceivedQty > 0 ||
									GetExtendedRestQty(s) > 0)
								.ToArray();

							bool inputLocation = Mode.IsLocationUserInputRequired;
							bool inputLotSerial = Mode.IsLotSerialUserInputRequired;

							ExactSplits = DonorSplits
								.Where(s =>
									inputLocation.Implies(s.LocationID == Basis.LocationID || s.ReceivedQty == 0) &&
									inputLotSerial.Implies(string.Equals(s.LotSerialNbr, Basis.LotSerialNbr, StringComparison.OrdinalIgnoreCase) || s.ReceivedQty == 0))
								.ToArray();

							AcceptorSplits = new List<POReceiptLineSplit>();

							decimal donorQty = DonorSplits.Concat(UnassignedSplits).GroupBy(s => new { s.LineNbr, s.PONbr }).Sum(g => GetExtendedRestQty(g.First()));
							BaseExcessQty = Math.Max(0, RestQty - donorQty);
						}

						public virtual void Spread()
						{
							if (RestQty > 0)
								FulfillUnassigned();

							if (RestQty > 0)
								FulfillDirectly();

							if (RestQty > 0)
								FulfillFromDonor();

							if (RestQty > 0)
								FulfillDirectlyWithThresholds();

							if (RestQty > 0)
								FulfillFromDonorWithThresholds();

							if (RestQty > 0 && NewLineIsNeeded)
								Overreceive();
						}

						protected virtual void SetSplitInfo(ILSMaster split, bool isAcceptor = false, bool forceSetExpireDate = false)
						{
							if (isAcceptor)
							{
								if (Basis.HasActive<LocationState>())
									split.LocationID = null;
								if (Mode.UseDefaultLotSerial == false)
									split.LotSerialNbr = null;
								if (Mode.UseDefaultExpireDate == false)
									split.ExpireDate = null;
							}

							forceSetExpireDate |= isAcceptor;
							forceSetExpireDate |= !string.Equals(split.LotSerialNbr, Basis.LotSerialNbr ?? split.LotSerialNbr, StringComparison.OrdinalIgnoreCase);

							if (Basis.HasActive<LocationState>() && Basis.LocationID != null)
								split.LocationID = Basis.LocationID;

							if (Mode.IsLotSerialUserInputRequired && Basis.LotSerialNbr != null)
								split.LotSerialNbr = Basis.LotSerialNbr;

							DateTime? expireDate = forceSetExpireDate ? Basis.ExpireDate : Mode.EnsureExpireDateDefault();
							if (Basis.HasActive<ExpireDateState>() && (Mode.UseDefaultExpireDate == false || forceSetExpireDate) && expireDate != null)
								split.ExpireDate = expireDate;
						}

						protected virtual void FulfillUnassigned()
						{
							foreach (var split in UnassignedSplits)
							{
								if (RestQty == 0) break;

								decimal currentQty = Math.Min(split.Qty.Value, RestQty);

								if (currentQty > 0)
								{
									Graph.transactions.Current = Graph.transactions.Search<POReceiptLine.lineNbr>(split.LineNbr);
									POReceiptLineSplit matchingAssignedSplit = Graph.splits.Search<POReceiptLineSplit.locationID, POReceiptLineSplit.lotSerialNbr>(Basis.LocationID, Basis.LotSerialNbr);

									if (matchingAssignedSplit == null)
									{
										matchingAssignedSplit = PXCache<POReceiptLineSplit>.CreateCopy(split);

										matchingAssignedSplit.IsUnassigned = false;
										matchingAssignedSplit.SplitLineNbr = null;
										matchingAssignedSplit.PlanID = null;

										matchingAssignedSplit.Qty = currentQty;
										matchingAssignedSplit.ReceivedQty = currentQty;

										SetSplitInfo(matchingAssignedSplit, isAcceptor: true);
										AcceptorSplits.Add(Mode.Received.Insert(matchingAssignedSplit));
									}
									else
									{
										matchingAssignedSplit.Qty += currentQty;
										matchingAssignedSplit.ReceivedQty += currentQty;
										Mode.Received.Update(matchingAssignedSplit);
									}

									RestQty -= currentQty;
								}
							}
						}

						protected virtual void FulfillDirectly()
						{
							foreach (var split in ExactSplits)
							{
								if (RestQty == 0) break;

								decimal currentQty = split.Qty.Value - split.ReceivedQty.Value;
								currentQty = Math.Min(currentQty, RestQty);

								if (currentQty > 0)
								{
									Graph.transactions.Current = Graph.transactions.Search<POReceiptLine.lineNbr>(split.LineNbr);
									split.ReceivedQty += currentQty;
									SetSplitInfo(split);
									Mode.Received.Update(split);

									RestQty -= currentQty;
								}
							}
						}

						protected virtual void FulfillFromDonor()
						{
							foreach (var donorSplitGroup in DonorSplits.Except(ExactSplits).GroupBy(s => s.LineNbr))
							{
								decimal acceptorQty = 0;
								POReceiptLineSplit acceptorSplit = null;
								foreach (var donorSplit in donorSplitGroup)
								{
									if (RestQty == 0) break;

									decimal currentQty = donorSplit.Qty.Value - donorSplit.ReceivedQty.Value;
									currentQty = Math.Min(currentQty, RestQty);

									if (currentQty > 0)
									{
										Graph.transactions.Current = Graph.transactions.Search<POReceiptLine.lineNbr>(donorSplit.LineNbr);
										donorSplit.Qty -= currentQty;
										Mode.Received.Update(donorSplit);

										if (acceptorSplit == null)
											acceptorSplit = PXCache<POReceiptLineSplit>.CreateCopy(donorSplit);

										RestQty -= currentQty;
										acceptorQty += currentQty;
									}
								}

								if (acceptorSplit != null)
								{
									acceptorSplit.SplitLineNbr = null;
									acceptorSplit.PlanID = null;

									acceptorSplit.Qty = acceptorQty;
									acceptorSplit.ReceivedQty = acceptorQty;

									SetSplitInfo(acceptorSplit, isAcceptor: true);
									AcceptorSplits.Add(Mode.Received.Insert(acceptorSplit));
								}
							}
						}

						protected virtual void FulfillDirectlyWithThresholds()
						{
							foreach (var exactSplitGroup in ExactSplits.Concat(AcceptorSplits).GroupBy(s => new { s.LineNbr, s.PONbr }).OrderBy(g => g.Key.PONbr == null))
							{
								if (RestQty == 0) break;

								decimal restGroupQty = exactSplitGroup.Key.PONbr == null ? RestQty : Math.Min(RestQty, GetExtendedRestQty(exactSplitGroup.First()));
								foreach (var split in exactSplitGroup.Where(s => Basis.ReceiveBySingleItem.Implies(s.ReceivedQty == 0)))
								{
									if (restGroupQty == 0) break;

									decimal currentQty = Basis.ReceiveBySingleItem ? 1 : restGroupQty;

									if (currentQty > 0)
									{
										Graph.transactions.Current = Graph.transactions.Search<POReceiptLine.lineNbr>(split.LineNbr);
										split.ReceivedQty += currentQty;
										if (exactSplitGroup.Key.PONbr == null)
											split.Qty += currentQty;
										SetSplitInfo(split);
										Mode.Received.Update(split);

										RestQty -= currentQty;
										restGroupQty -= currentQty;
									}
								}
							}
						}

						protected virtual void FulfillFromDonorWithThresholds()
						{
							foreach (var donorSplitGroup in
								DonorSplits.Except(ExactSplits.Concat(AcceptorSplits))
								.Where(s => s.Qty.Value - s.ReceivedQty.Value > 0 || GetExtendedRestQty(s) > 0)
								.GroupBy(s => new { s.LineNbr, s.PONbr })
								.Where(g => g.Key.PONbr != null || Graph.Document.Current.ReceiptType == POReceiptType.TransferReceipt))
							{
								if (RestQty == 0) break;

								decimal currentGroupQty = GetExtendedRestQty(donorSplitGroup.First());
								currentGroupQty = Math.Min(currentGroupQty, RestQty);

								if (currentGroupQty > 0)
								{
									Graph.transactions.Current = Graph.transactions.Search<POReceiptLine.lineNbr>(donorSplitGroup.Key.LineNbr);

									decimal acceptorQty = 0;
									decimal restGroupQty = currentGroupQty;
									foreach (var donorSplit in donorSplitGroup)
									{
										if (restGroupQty == 0) break;

										decimal currentQty = donorSplit.Qty.Value - donorSplit.ReceivedQty.Value;
										currentQty = Math.Min(currentQty, restGroupQty);

										if (currentQty > 0)
										{
											donorSplit.Qty -= currentQty;
											Mode.Received.Update(donorSplit);

											restGroupQty -= currentQty;
											acceptorQty += currentQty;
										}

										if (restGroupQty > 0)
										{
											decimal extraQty = GetExtendedRestQty(donorSplit);
											extraQty = Math.Min(extraQty, restGroupQty);

											if (extraQty > 0)
											{
												restGroupQty -= extraQty;
												acceptorQty += extraQty;
											}
										}
									}

									if (acceptorQty > 0)
									{
										var acceptorSplit = PXCache<POReceiptLineSplit>.CreateCopy(donorSplitGroup.First());
										acceptorSplit.SplitLineNbr = null;
										acceptorSplit.PlanID = null;
										acceptorSplit.Qty = acceptorQty;
										acceptorSplit.ReceivedQty = acceptorQty;

										SetSplitInfo(acceptorSplit, isAcceptor: true);
										Mode.Received.Insert(acceptorSplit);

										RestQty -= acceptorQty;
									}
								}
							}
						}

						protected virtual void Overreceive()
						{
							PXSelectBase<POReceiptLine> lines = Graph.transactions;

							POReceiptLine newLine = lines.With(_ => _.Insert() ?? _.Insert());
							lines.SetValueExt<POReceiptLine.inventoryID>(newLine, Basis.InventoryID);
							lines.SetValueExt<POReceiptLine.subItemID>(newLine, Basis.SubItemID);
							lines.SetValueExt<POReceiptLine.siteID>(newLine, Basis.SiteID);
							lines.SetValueExt<POReceiptLine.uOM>(newLine, Basis.SelectedInventoryItem.BaseUnit);
							newLine = lines.Update(newLine);

							newLine.Qty = RestQty;
							newLine = lines.Update(newLine);

							SetSplitInfo(newLine, forceSetExpireDate: true);
							newLine = lines.Update(newLine);

							foreach (POReceiptLineSplit split in Graph.splits.Select())
							{
								split.ReceivedQty = split.Qty;
								Mode.Received.Update(split);
							}
						}
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string Prompt = "Confirm receiving {0} x {1} {2}.";
					public const string QtyExceedsReceipt = "The scanned quantity exceeds the quantity in the {0} PO receipt for the {1} item.";
					public const string NothingToRemove = "No items to remove from the receipt.";
					public const string Overreceiving = "The received quantity cannot be greater than the expected quantity.";
					public const string Underreceiving = "The received quantity cannot become negative.";
					public const string InventoryAdded = "{0} x {1} {2} has been added to the receipt.";
					public const string InventoryRemoved = "{0} x {1} {2} has been removed from the receipt.";
					public const string ItemOrReceiptPrompt = "Scan the barcode of the item or the next receipt number.";
				}
				#endregion
			}
			#endregion

			#region Commands
			public sealed class ReleaseReceiptCommand : ScanCommand
			{
				public override string Code => "RELEASE";
				public override string ButtonName => "scanReleaseReceipt";
				public override string DisplayName => Msg.DisplayName;
				protected override bool IsEnabled => Basis.DocumentIsEditable;

				protected override bool Process()
				{
					Get<Logic>().ReleaseReceipt(false);
					return true;
				}

				#region Logic
				public class Logic : ScanExtension
				{
					public virtual void ReleaseReceipt(bool completePOLines)
					{
						bool anyChanges = Basis.ApplyLinesQtyChanges(completePOLines);
						Basis.SaveChanges();

						var userSetup = UserSetup.For(Basis);
						bool printLabels = userSetup.PrintInventoryLabelsAutomatically == true;
						string printLabelsReportID = printLabels ? userSetup.InventoryLabelsReportID : null;
						bool printReceipt = anyChanges && userSetup.PrintPurchaseReceiptAutomatically == true;

						Basis
						.AwaitFor<POReceipt>((basis, doc, ct) => ReleaseReceiptImpl(doc, printLabelsReportID, printReceipt, ct))
						.WithDescription(Msg.InProcess, Basis.RefNbr)
						.ActualizeDataBy((basis, doc) => POReceipt.PK.Find(basis, doc))
						.OnSuccess(x => x.Say(Msg.Success).ChangeStateTo<ReceiptState>())
						.OnFail(x => x.Say(Msg.Fail))
						.BeginAwait(Basis.Receipt);
					}

					private static async System.Threading.Tasks.Task ReleaseReceiptImpl(POReceipt receipt, string printLabelsReportID, bool printReceipt, CancellationToken cancellationToken)
					{
						if (receipt.Hold == true)
						{
							var receiptGraph = PXGraph.CreateInstance<POReceiptEntry>();
							receiptGraph.Document.Current = receiptGraph.Document.Search<POReceipt.receiptNbr>(receipt.ReceiptNbr, receipt.ReceiptType);
							receiptGraph.releaseFromHold.Press();
							receipt = receiptGraph.Document.Current;
						}

						POReleaseReceipt.ReleaseDoc(new List<POReceipt>() { receipt }, false);

						if (PXAccess.FeatureInstalled<CS.FeaturesSet.deviceHub>())
						{
							var inGraph = Lazy.By(() => PXGraph.CreateInstance<INReceiptEntry>());

							if (!string.IsNullOrEmpty(printLabelsReportID))
							{
								string inventoryRefNbr = POReceipt.PK.Find(inGraph.Value, receipt)?.InvtRefNbr;
								if (inventoryRefNbr != null)
								{
									var reportParameters = new Dictionary<string, string>()
									{
										[nameof(INRegister.RefNbr)] = inventoryRefNbr
									};

									await DeviceHubTools.PrintReportViaDeviceHub<CR.BAccount>(inGraph.Value, printLabelsReportID, reportParameters, INNotificationSource.None, null, cancellationToken);
								}
							}

							if (printReceipt)
							{
								var reportParameters = new Dictionary<string, string>()
								{
									[nameof(POReceipt.ReceiptType)] = receipt.ReceiptType,
									[nameof(POReceipt.ReceiptNbr)] = receipt.ReceiptNbr
								};

								Vendor vendor = Vendor.PK.Find(inGraph.Value, receipt.VendorID);
								await DeviceHubTools.PrintReportViaDeviceHub(inGraph.Value, "PO646000", reportParameters, PONotificationSource.Vendor, vendor, cancellationToken);
							}
						}
					}
				}
				#endregion

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string DisplayName = "Release Receipt";
					public const string InProcess = "The {0} receipt is being released.";
					public const string Success = "The receipt has been successfully released.";
					public const string Fail = "The receipt release failed.";
				}
				#endregion
			}

			public sealed class ReleaseReceiptAndCompletePOLinesCommand : ScanCommand
			{
				public override string Code => "COMPLETE*POLINES";
				public override string ButtonName => "scanCompletePOLines";
				public override string DisplayName => Msg.DisplayName;
				protected override bool IsEnabled => Basis.DocumentIsEditable;

				protected override bool Process()
				{
					Get<ReleaseReceiptCommand.Logic>().ReleaseReceipt(true);
					return true;
				}

				#region Messages
				[PXLocalizable]
				public new abstract class Msg : ReleaseReceiptCommand.Msg
				{
					public new const string DisplayName = "Complete PO Lines";
				}
				#endregion
			}
			#endregion

			#region Redirect
			public sealed class RedirectFrom<TForeignBasis> : WMSBase.RedirectFrom<TForeignBasis>.SetMode<ReceiveMode>
				where TForeignBasis : PXGraphExtension, IBarcodeDrivenStateMachine
			{
				public override string Code => "RECEIVE";
				public override string DisplayName => Msg.DisplayName;

				private string RefNbr { get; set; }

				public override bool IsPossible
				{
					get
					{
						bool wmsReceiving = PXAccess.FeatureInstalled<CS.FeaturesSet.wMSReceiving>();
						var rpSetup = POReceivePutAwaySetup.PK.Find(Basis.Graph, Basis.Graph.Accessinfo.BranchID);
						return wmsReceiving && rpSetup?.ShowReceivingTab != false;
					}
				}

				protected override bool PrepareRedirect()
				{
					if (Basis is ReceivePutAway rpa && rpa.RefNbr != null)
					{
						if (rpa.FindMode<ReceiveMode>().TryValidate(rpa.Receipt).By<ReceiptState>() is Validation valid && valid.IsError == true)
						{
							rpa.ReportError(valid.Message, valid.MessageArgs);
							return false;
						}
						else
							RefNbr = rpa.RefNbr;
					}

					return true;
				}

				protected override void CompleteRedirect()
				{
					if (Basis is ReceivePutAway rpa && rpa.CurrentMode.Code != ReturnMode.Value && this.RefNbr != null)
						if (rpa.TryProcessBy(ReceivePutAway.ReceiptState.Value, RefNbr, StateSubstitutionRule.KeepAll & ~StateSubstitutionRule.KeepPositiveReports))
						{
							rpa.SetDefaultState();
							RefNbr = null;
						}
				}

				#region Messages
				[PXLocalizable]
				public abstract class Msg
				{
					public const string DisplayName = "PO Receive";
				}
				#endregion
			}
			#endregion

			#region Messages
			[PXLocalizable]
			public new abstract class Msg : ScanMode.Msg
			{
				public const string Description = "Receive";
				public const string Completed = "The {0} receipt is received.";
				public const string RestQty = "Remaining Qty.";
			}
			#endregion

			#region Attached Fields
			[PXUIField(DisplayName = Msg.RestQty)]
			public class RestQty : FieldAttached.To<POReceiptLineSplit>.AsDecimal.Named<RestQty>
			{
				public override decimal? GetValue(POReceiptLineSplit row) => Base.WMS.Header.Mode == ReceiveMode.Value ? Base.WMS.Get<ReceiveMode.Logic>().GetNormalRestQty(row) : 0;
				protected override bool? Visible => Base.WMS.With(wms => wms.Header.Mode == ReceiveMode.Value);
			}

			[PXUIField(Visible = false)]
			public class ShowReceive : FieldAttached.To<ScanHeader>.AsBool.Named<ShowReceive>
			{
				public override bool? GetValue(ScanHeader row) => Base.WMS.Setup.Current.ShowReceivingTab == true && row.Mode == ReceiveMode.Value;
			}
			#endregion
		}
	}
}
