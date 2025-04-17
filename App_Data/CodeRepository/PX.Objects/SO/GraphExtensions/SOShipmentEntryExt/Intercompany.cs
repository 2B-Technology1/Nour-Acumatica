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
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.PO;

namespace PX.Objects.SO.GraphExtensions.SOShipmentEntryExt
{
	public class Intercompany : PXGraphExtension<SOShipmentEntry>
	{
		public static bool IsActive()
			=> PXAccess.FeatureInstalled<FeaturesSet.interBranch>()
			&& PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();

		public PXAction<SOShipment> generatePOReceipt;
		[PXUIField(DisplayName = "Generate PO Receipt", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable GeneratePOReceipt(PXAdapter adapter)
		{
			Base.Save.Press();

			SOShipment shipment = Base.Document.Current;
			List<PXResult<SOShipLine, SOLine>> shipLines =
				SelectFrom<SOShipLine>
					.InnerJoin<SOLine>.On<SOShipLine.FK.OrderLine>
					.Where<SOShipLine.FK.Shipment.SameAsCurrent>
					.View.Select(Base).AsEnumerable()
					.Cast<PXResult<SOShipLine, SOLine>>()
					.ToList();

			PXLongOperation.StartOperation(Base, () =>
			{
				var baseGraph = PXGraph.CreateInstance<SOShipmentEntry>();
				var ext = baseGraph.GetExtension<Intercompany>();
				ext.GenerateIntercompanyPOReceipt(shipment, shipLines, null, null);
			});

			yield return shipment;
		}

		public virtual POReceipt GenerateIntercompanyPOReceipt(
			SOShipment shipment,
			List<PXResult<SOShipLine, SOLine>> shipLines,
			bool? holdValue,
			DateTime? receiptDate)
		{
			if (!string.IsNullOrEmpty(shipment.IntercompanyPOReceiptNbr)
				|| shipment.ShipmentType != SOShipmentType.Issue
				|| shipment.Operation != SOOperation.Issue)
			{
				throw new PXInvalidOperationException();
			}
			SOOrder so = SelectFrom<SOOrder>
				.InnerJoin<SOOrderShipment>.On<SOOrderShipment.FK.Order>
				.Where<SOOrderShipment.FK.Shipment.SameAsCurrent>
				.View.SelectSingleBound(Base, new[] { shipment });
			Branch vendorBranch = Branch.PK.Find(Base, so.BranchID);
			Vendor vendor = Vendor.PK.Find(Base, vendorBranch?.BAccountID);
			if (vendor == null)
			{
				throw new PXException(Messages.BranchIsNotExtendedToVendor, vendorBranch?.BranchCD.TrimEnd());
			}
			var customerBranch = PXAccess.GetBranchByBAccountID(so.CustomerID);

			POOrder po = null;
			if (so.Behavior == SOBehavior.SO)
			{
				po = POOrder.PK.Find(Base, so.IntercompanyPOType, so.IntercompanyPONbr);
			}
			else if (so.Behavior == SOBehavior.RM)
			{
				po = SelectFrom<POOrder>
					.InnerJoin<POOrderReceipt>
						.On<POOrderReceipt.FK.Order>
					.Where<
						POOrderReceipt.receiptType.IsEqual<POReceiptType.poreturn>
						.And<POOrderReceipt.receiptNbr.IsEqual<@P.AsString>>>
					.View.ReadOnly
					.Select(Base, so.IntercompanyPOReturnNbr);
			}

			if (po?.Cancelled == true)
			{
				throw new PXException(Messages.POCancelledPRCannotBeCreated, po.OrderNbr);
			}

			var graph = PXGraph.CreateInstance<POReceiptEntry>();

			CurrencyInfo origCuryInfo = graph.MultiCurrencyExt.GetCurrencyInfo(po?.CuryInfoID ?? so.CuryInfoID);
			CurrencyInfo curyInfo = graph.MultiCurrencyExt.CloneCurrencyInfo(origCuryInfo);

			var doc = new POReceipt
			{
				ReceiptType = POReceiptType.POReceipt,
				ReceiptDate = receiptDate,
				BranchID = customerBranch.BranchID,
				CuryID = curyInfo.CuryID,
				CuryInfoID = curyInfo.CuryInfoID
			};
			doc = PXCache<POReceipt>.CreateCopy(graph.Document.Insert(doc));
			doc.VendorID = vendor.BAccountID;
			doc = PXCache<POReceipt>.CreateCopy(graph.Document.Update(doc));
			doc.BranchID = customerBranch.BranchID;
			doc = PXCache<POReceipt>.CreateCopy(graph.Document.Update(doc));
			curyInfo = graph.MultiCurrencyExt.GetCurrencyInfo(doc.CuryInfoID);

			curyInfo.CuryID = origCuryInfo.CuryID;
			if (string.Equals(origCuryInfo.BaseCuryID, curyInfo.BaseCuryID, StringComparison.OrdinalIgnoreCase))
			{
				curyInfo.CuryRateTypeID = origCuryInfo.CuryRateTypeID;
			}
			curyInfo = graph.MultiCurrencyExt.currencyinfo.Update(curyInfo);

			doc.CuryID = curyInfo.CuryID;
			doc.ProjectID = po?.ProjectID ?? so.ProjectID;
			doc.AutoCreateInvoice = false;
			doc.InvoiceNbr = shipment.ShipmentNbr;
			doc.IntercompanyShipmentNbr = shipment.ShipmentNbr;
			doc = graph.Document.Update(doc);

			foreach (PXResult<SOShipLine, SOLine> shipLine in shipLines)
			{
				GeneratePOReceiptLine(graph, po, shipLine, so, shipLine);
			}

			if (graph.Document.Current != null)
			{
				doc = PXCache<POReceipt>.CreateCopy(graph.Document.Current);
				doc.ControlQty = doc.OrderQty;
				graph.Document.Update(doc);
			}

			var uniquenessChecker = new UniquenessChecker<
				SelectFrom<POReceipt>
				.Where<POReceipt.FK.IntercompanyShipment.SameAsCurrent>>(shipment);
			graph.OnBeforeCommit += uniquenessChecker.OnBeforeCommitImpl;
			try
			{
				graph.Save.Press();

				if (holdValue != null)
				{
					if (holdValue == true)
						graph.putOnHold.Press();
					else
						graph.releaseFromHold.Press();
				}
			}
			finally
			{
				graph.OnBeforeCommit -= uniquenessChecker.OnBeforeCommitImpl;
			}

			return graph.Document.Current;
		}

		protected virtual POReceiptLine GeneratePOReceiptLine(
			POReceiptEntry graph,
			POOrder po,
			SOShipLine shipLine,
			SOOrder so,
			SOLine soLine)
		{
			POReceiptLine line;
			POLine poLine = null;
			if (po != null)
			{
				if (so.Behavior == SOBehavior.SO)
				{
					poLine = POLine.PK.Find(graph, po?.OrderType, po?.OrderNbr, soLine.IntercompanyPOLineNbr);
				}
				else if (so.Behavior == SOBehavior.RM)
				{
					SOLine receiptSOLine = SOLine.FK.OriginalOrderLine.FindParent(graph, soLine);
					if (receiptSOLine != null)
					{
						poLine = SelectFrom<POLine>
							.InnerJoin<POReceiptLine>.On<POReceiptLine.FK.OrderLine>
							.Where<POReceiptLine.receiptType.IsEqual<POReceiptType.poreturn>
								.And<POReceiptLine.receiptNbr.IsEqual<@P.AsString>>
								.And<POReceiptLine.lineNbr.IsEqual<@P.AsInt>>>
							.View.ReadOnly
							.Select(graph, so.IntercompanyPOReturnNbr, receiptSOLine.IntercompanyPOLineNbr);
					}
				}
			}
			if (poLine != null)
			{
				line = graph.AddPOLine(poLine, true);
				line.IsLSEntryBlocked = true;
				graph.AddPOOrderReceipt(poLine.OrderType, poLine.OrderNbr);
			}
			else
			{
				line = new POReceiptLine
				{
					IsStockItem = shipLine.IsStockItem,
					InventoryID = shipLine.InventoryID,
					SubItemID = shipLine.SubItemID,
					IsLSEntryBlocked = true,
				};
				line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Insert(line));
				line.ProjectID = soLine.ProjectID;
				line.TaskID = soLine.TaskID;
				line.CostCodeID = soLine.CostCodeID;
				line = graph.transactions.Update(line);
			}
			line = PXCache<POReceiptLine>.CreateCopy(line);
			line.UOM = shipLine.UOM;
			line.TranDesc = shipLine.TranDesc;
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));
			line.Qty = shipLine.ShippedQty;
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));

			CurrencyInfo curyInfo = graph.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();

			line.CuryUnitCost = soLine.CuryUnitPrice;
			line.ManualPrice = true;
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));
			decimal? ratio = soLine.BaseQty == 0m ? 1m : shipLine.BaseQty / soLine.BaseQty;
			line.CuryExtCost = curyInfo.RoundCury((soLine.CuryExtPrice * ratio) ?? 0m);
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));
			line.DiscPct = soLine.DiscPct;
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));
			line.CuryDiscAmt = curyInfo.RoundCury((soLine.CuryDiscAmt * ratio) ?? 0m);
			line = PXCache<POReceiptLine>.CreateCopy(graph.transactions.Update(line));

			line.IsLSEntryBlocked = false;
			line.IntercompanyShipmentLineNbr = shipLine.LineNbr;
			line = graph.transactions.Update(line);
			graph.transactions.Cache.SetDefaultExt<POReceiptLine.locationID>(line);

			if (line.IsStockItem == true)
			{
				List<SOShipLineSplit> shipLineSplits = SelectFrom<SOShipLineSplit>
					.Where<SOShipLineSplit.FK.ShipmentLine.SameAsCurrent>
					.View.SelectMultiBound(graph, new[] { shipLine })
					.RowCast<SOShipLineSplit>().ToList();
				foreach (SOShipLineSplit shipLineSplit in shipLineSplits)
				{
					var split = new POReceiptLineSplit
					{
						InventoryID = shipLineSplit.InventoryID,
						SubItemID = shipLineSplit.SubItemID,
						LotSerialNbr = shipLineSplit.LotSerialNbr,
						ExpireDate = shipLineSplit.ExpireDate,
					};
					split = PXCache<POReceiptLineSplit>.CreateCopy(graph.splits.Insert(split));
					split.Qty = shipLineSplit.Qty;
					split = graph.splits.Update(split);
				}
			}

			return line;
		}

		/// Overrides <see cref="SOShipmentEntry.CreateShipment(CreateShipmentArgs)"/>
		[PXOverride]
		public virtual void CreateShipment(CreateShipmentArgs args, Action<CreateShipmentArgs> baseMethod)
		{
			if (Base.Document.Current != null && !args.MassProcess)
			{
				var shipment = Base.Document.Current;
				bool isIntercompanyIssue = shipment.ShipmentType == SOShipmentType.Issue && shipment.IsIntercompany == true;
				if (isIntercompanyIssue)
				{
					var anotherOrder = Base.OrderListSimple.Select()
						.RowCast<SOOrderShipment>()
						.Where(os => os.OrderType != args.Order.OrderType || os.OrderNbr != args.Order.OrderNbr)
						.FirstOrDefault();

					if (anotherOrder != null)
					{
						throw new PXException(Messages.IntercompanyShipmentShouldContainSingleOrder);
					}
				}
			}

			baseMethod(args);
		}

		protected virtual void _(Events.RowSelecting<SOShipment> eventArgs)
		{
			if (eventArgs.Row == null
				|| eventArgs.Row.ShipmentType != SOShipmentType.Issue
				|| eventArgs.Row.Operation != SOOperation.Issue)
				return;

			if (eventArgs.Row.IsIntercompany == true)
			{
				using (new PXReadBranchRestrictedScope())
				{
					// Acuminator disable once PX1042 DatabaseQueriesInRowSelecting [false positive]
					POReceipt intercompanyPOReceipt =
						SelectFrom<POReceipt>
							.Where<POReceipt.FK.IntercompanyShipment.SameAsCurrent>
						.View.SelectSingleBound(Base, new[] { eventArgs.Row });
					eventArgs.Row.IntercompanyPOReceiptNbr = intercompanyPOReceipt?.ReceiptNbr;
				}
			}
		}

		protected virtual void _(Events.RowSelected<SOShipment> eventArgs)
		{
			if (eventArgs.Row == null)
				return;

			bool isIntercompanyIssue =
				eventArgs.Row.ShipmentType == SOShipmentType.Issue
				&& eventArgs.Row.IsIntercompany == true;
			eventArgs.Cache.Adjust<PXUIFieldAttribute>(eventArgs.Row)
				.For<SOShipment.intercompanyPOReceiptNbr>(a =>
				{
					a.Visible = isIntercompanyIssue;
					a.Enabled = false;
				})
				.For<SOShipment.excludeFromIntercompanyProc>(a =>
				{
					a.Visible = isIntercompanyIssue;
					a.Enabled = true;
				});

			if (isIntercompanyIssue)
			{
				eventArgs.Cache.AllowUpdate = true; // needed to enable ExcludeFromIntercompanyProc
				PXUIFieldAttribute.SetEnabled<SOShipment.shipmentNbr>(eventArgs.Cache, eventArgs.Row);
			}
		}
	}
}
