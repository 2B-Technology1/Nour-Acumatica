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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN.DAC;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class ConvertStockToNonStockExtBase<TConvertFromGraph, TConvertToGraph> : PXGraphExtension<TConvertFromGraph>
		where TConvertFromGraph : InventoryItemMaintBase, new()
		where TConvertToGraph : InventoryItemMaintBase, new()
	{
		protected const int MaxNumberOfDocuments = 200;

		protected HashSet<InventoryItem> _persistingConvertedItems;

		public override void Initialize()
		{
			base.Initialize();

			Base.EnsureCachePersistence<INConversionHistory>();
			Base.OnBeforeCommit += (graph) => _persistingConvertedItems?.ForEach(OnBeforeCommitVerifyNoTransactionsCreated);
			Base.OnClear += (graph, opt) => _persistingConvertedItems?.Clear();
		}

		public PXAction<InventoryItem> convert;
		[PXButton(CommitChanges = true), PXUIField(MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable Convert(PXAdapter adapter)
		{
			if (Base.Item.Current.InventoryID >= 0)
			{
				Base.Save.Press();

				int? inventoryID = Base.Item.Current.InventoryID;

				PXLongOperation.StartOperation(Base, () =>
				{
					TConvertFromGraph newGraph = PXGraph.CreateInstance<TConvertFromGraph>();
					var ext = newGraph.FindImplementation<ConvertStockToNonStockExtBase<TConvertFromGraph, TConvertToGraph>>();
					ext.VerifyAndConvert(inventoryID);
				});

				return adapter.Get();
			}
			else
			{
				var newGraph = PXGraph.CreateInstance<TConvertToGraph>();
				newGraph.Item.Current = ConvertInventoryItem(newGraph, Base.Item.Current);

				throw new PXRedirectRequiredException(newGraph, null);
			}
		}

		protected virtual void VerifyAndConvert(int? inventoryID)
		{
			DateTime startedDateTime = PXTimeZoneInfo.UtcNow;
			Base.Item.Current = Base.Item.Search<InventoryItem.inventoryID>(inventoryID);

			var errors = new List<string>();
			int numberOfErrors = Verify(Base.Item.Current, errors);
			if (numberOfErrors == 1 && errors.Count == 1)
			{
				throw new PXException(errors.First());
			}
			else
			{
				numberOfErrors += VerifyINItemPlan(Base.Item.Current, errors);
				if (numberOfErrors == 1 && errors.Count == 1)
				{
					throw new PXException(errors.First());
				}
				else if (numberOfErrors != 0)
				{
					throw new PXException(Messages.NotProcessedDocumentsSeeTrace);
				}
			}

			var newGraph = PXGraph.CreateInstance<TConvertToGraph>();
			newGraph.Item.Current = ConvertInventoryItem(newGraph, Base.Item.Current);

			newGraph.Caches<INConversionHistory>().Insert(new INConversionHistory()
			{
				InventoryID = inventoryID,
				IsStockItem = !(bool)Base.Item.Current.StkItem,
				StartedDateTime = startedDateTime
			});

			throw new PXRedirectRequiredException(newGraph, null);
		}

		#region Verification

		protected virtual int Verify(InventoryItem item, List<string> errors)
		{
			int numberOfErrors = 0;

			numberOfErrors += VerifyInventoryItem(item, errors);
			numberOfErrors += VerifyINRegister(item, errors);
			numberOfErrors += VerifyPOOrder(item, errors);
			numberOfErrors += VerifyPOReceipt(item, errors);
			numberOfErrors += VerifyLandedCost(item, errors);
			numberOfErrors += VerifyAPTran(item, errors);
			numberOfErrors += VerifySOOrder(item, errors);
			numberOfErrors += VerifySOShipment(item, errors);
			numberOfErrors += VerifyARTran(item, errors);

			return numberOfErrors;
		}

		protected virtual int VerifyInventoryItem(InventoryItem item, List<string> errors)
		{
			if (item.KitItem == true)
				throw new PXException(Messages.CannotConvertKit, item.InventoryCD.Trim());

			if (item.TemplateItemID != null || item.IsTemplate == true)
				throw new PXException(Messages.CannotConvertMatrix, item.InventoryCD.Trim());

			return 0;
		}

		protected virtual int VerifyINRegister(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<INTran>
				.Where<INTran.released.IsNotEqual<True>
					.And<INTran.inventoryID.IsEqual<@P.AsInt>>>
				.AggregateTo<GroupBy<INTran.docType>, GroupBy<INTran.refNbr>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INTran>().ToArray();

			if (documents.Any())
			{
				foreach (var documentsGroupedByDocType in documents.GroupBy(d => GetINRegisterMessage(d.DocType)))
				{
					string error = PXLocalizer.LocalizeFormat(documentsGroupedByDocType.Key, item.InventoryCD.Trim(),
						string.Join(", ", documentsGroupedByDocType.Select(d => d.RefNbr)));

					PXTrace.WriteError(error);
					errors.Add(error);
				}
			}

			return documents.Length;
		}

		protected virtual string GetINRegisterMessage(string docType)
		{
			switch (docType)
			{
				case INDocType.Receipt:
					return Messages.CannotConvertInventoryReceipts;
				case INDocType.Issue:
					return Messages.CannotConvertInventoryIssues;
				case INDocType.Transfer:
					return Messages.CannotConvertInventoryTrasfers;
				case INDocType.Adjustment:
					return Messages.CannotConvertInventoryAdjustments;
				case INDocType.Production:
				case INDocType.Disassembly:
					return Messages.CannotConvertInventoryKits;
				default:
					throw new NotImplementedException(docType);
			}
		}

		protected virtual int VerifyPOOrder(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<POOrder>
				.Where<POOrder.cancelled.IsNotEqual<True>
					.And<POOrder.hold.IsNotEqual<False>
						.Or<POOrder.approved.IsNotEqual<True>>
						.Or<POOrder.linesToCompleteCntr.IsNotEqual<decimal0>>>
					.And<Exists<Select<POLine, Where<POLine.inventoryID.IsEqual<@P.AsInt>
						.And<POLine.FK.Order>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<POOrder>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertPOOrders,
					item.InventoryCD.Trim(), string.Join(", ", documents.Select(c => c.OrderNbr)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyPOReceipt(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<POReceiptLine>
				.Where<POReceiptLine.receiptType.IsNotEqual<POReceiptType.transferreceipt>
					.And<POReceiptLine.inventoryID.IsEqual<@P.AsInt>>
					.And<POReceiptLine.unbilledQty.IsNotEqual<decimal0>
						.Or<POReceiptLine.released.IsNotEqual<True>>>>
				.AggregateTo<GroupBy<POReceiptLine.receiptType>, GroupBy<POReceiptLine.receiptNbr>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<POReceiptLine>().ToArray();

			if (documents.Any())
			{
				foreach (var documentsGroupedByReleased in documents.GroupBy(d => d.Released))
				{
					string message = (documentsGroupedByReleased.Key != true) ?
						Messages.CannotConvertNotReleasedPOReceipts : Messages.CannotConvertNotBilledPOReceipts;

					string error = PXLocalizer.LocalizeFormat(message, item.InventoryCD.Trim(),
						string.Join(", ", documentsGroupedByReleased.Select(d => d.ReceiptNbr)));

					PXTrace.WriteError(error);
					errors.Add(error);
				}
			}

			return documents.Length;
		}

		protected virtual int VerifyLandedCost(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<POLandedCostDoc>
				.Where<POLandedCostDoc.released.IsNotEqual<True>
					.And<Exists<Select<POLandedCostReceiptLine, Where<POLandedCostReceiptLine.inventoryID.IsEqual<@P.AsInt>
					.And<POLandedCostReceiptLine.FK.LandedCostDocument>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<POLandedCostDoc>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertLandedCosts,
					item.InventoryCD.Trim(), string.Join(", ", documents.Select(c => c.RefNbr)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyAPTran(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<APRegister>
				.Where<APRegister.released.IsNotEqual<True>
					.And<APRegister.docType.IsIn<@P.AsString.ASCII>>
					.And<Exists<Select<APTran, Where<APTran.FK.Document
						.And<APTran.released.IsNotEqual<True>>
						.And<APTran.inventoryID.IsEqual<@P.AsInt>>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, GetAPTranTypes().ToArray(), item.InventoryID)
				.RowCast<APRegister>().ToArray();

			if (documents.Any())
			{
				foreach (var documentsGroupedByDocType in documents.GroupBy(d => GetAPTranMessage(d.DocType)))
				{
					string error = PXLocalizer.LocalizeFormat(documentsGroupedByDocType.Key, item.InventoryCD.Trim(),
						string.Join(", ", documentsGroupedByDocType.Select(d => d.RefNbr)));

					PXTrace.WriteError(error);
					errors.Add(error);
				}
			}

			return documents.Length;
		}

		protected virtual List<string> GetAPTranTypes()
		{
			return new List<string> { APDocType.Invoice, APDocType.DebitAdj, APDocType.Prepayment };
		}

		protected virtual string GetAPTranMessage(string apDocType)
		{
			switch (apDocType)
			{
				case APDocType.Invoice:
					return Messages.CannotConvertAPBills;
				case APDocType.DebitAdj:
					return Messages.CannotConvertAPDebitAdjustments;
				case APDocType.Prepayment:
					return Messages.CannotConvertAPPrepaymentRequests;
				default:
					throw new NotImplementedException(apDocType);
			}
		}

		protected virtual int VerifySOOrder(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<SOOrder>
				.Where<SOOrder.cancelled.IsNotEqual<True>
					.And<SOOrder.completed.IsNotEqual<True>>
					.And<Exists<Select<SOLine, Where<SOLine.inventoryID.IsEqual<@P.AsInt>.And<SOLine.FK.Order>>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<SOOrder>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertSOOrders, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(c => c.OrderNbr)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifySOShipment(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<SOShipment>
				.Where<Exists<
					Select2<SOOrderShipment,
					InnerJoin<SOShipLine, On<SOShipLine.FK.OrderShipment>>,
					Where<SOShipLine.inventoryID.IsEqual<@P.AsInt>
						.And<SOOrderShipment.FK.Shipment>
						.And<SOOrderShipment.createINDoc.IsEqual<True>>
						.And<SOOrderShipment.invtRefNbr.IsNull>>>>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<SOShipment>().ToArray();

			if (documents.Any())
			{
				string error = PXLocalizer.LocalizeFormat(Messages.CannotConvertSOShipments, item.InventoryCD.Trim(),
					string.Join(", ", documents.Select(c => c.ShipmentNbr)));

				PXTrace.WriteError(error);
				errors.Add(error);
			}

			return documents.Length;
		}

		protected virtual int VerifyARTran(InventoryItem item, List<string> errors)
		{
			var documents = SelectFrom<ARTran>
				.Where<ARTran.tranType.IsIn<@P.AsString.ASCII>
					.And<ARTran.released.IsNotEqual<True>>
					.And<ARTran.inventoryID.IsEqual<@P.AsInt>>>
				.AggregateTo<GroupBy<ARTran.tranType>, GroupBy<ARTran.refNbr>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, GetARTranTypes().ToArray(), item.InventoryID)
				.RowCast<ARTran>().ToArray();

			if (documents.Any())
			{
				foreach (var documentsGroupedByDocType in documents.GroupBy(d => GetARTranMessage(d.TranType)))
				{
					string error = PXLocalizer.LocalizeFormat(documentsGroupedByDocType.Key, item.InventoryCD.Trim(),
						string.Join(", ", documentsGroupedByDocType.Select(c => c.RefNbr)));

					PXTrace.WriteError(error);
					errors.Add(error);
				}
			}

			return documents.Length;
		}

		protected virtual List<string> GetARTranTypes()
		{
			return new List<string> { ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.CreditMemo,
				ARDocType.CashSale, ARDocType.CashReturn };
		}

		protected virtual string GetARTranMessage(string apDocType)
		{
			switch (apDocType)
			{
				case ARDocType.Invoice:
				case ARDocType.DebitMemo:
				case ARDocType.CreditMemo:
					return Messages.CannotConvertInvoices;
				case ARDocType.CashSale:
				case ARDocType.CashReturn:
					return Messages.CannotConvertCashSale;
				default:
					throw new NotImplementedException(apDocType);
			}
		}

		protected virtual int VerifyINItemPlan(InventoryItem item, List<string> errors)
		{
			var plans = SelectFrom<INItemPlan>
				.Where<INItemPlan.inventoryID.IsEqual<@P.AsInt>
					.And<INItemPlan.planType.IsNotEqual<INPlanConstants.plan90>>>
				.AggregateTo<GroupBy<INItemPlan.planType>>
				.View.ReadOnly.SelectWindowed(Base, 0, MaxNumberOfDocuments, item.InventoryID)
				.RowCast<INItemPlan>().ToArray();

			if (plans.Any())
			{
				foreach (var groupedPlan in plans.GroupBy(p => GetINItemPlanMessage(p)))
				{
					string error = PXLocalizer.LocalizeFormat(groupedPlan.Key, item.InventoryCD.Trim(),
							string.Join(", ", groupedPlan.Select(g => GetINItemPlanName(g.PlanType)).Distinct()));

					PXTrace.WriteError(error);
					errors.Add(error);
				}
			}

			return plans.Length;
		}

		protected virtual string GetINItemPlanMessage(INItemPlan itemPlan)
		{
			bool isReplenishment = (itemPlan.PlanType == INPlanConstants.Plan90);

			if (isReplenishment)
			{
				if (itemPlan.FixedSource?.IsIn(INReplenishmentSource.TransferToPurchase, INReplenishmentSource.Transfer) == true)
				{
					return Messages.CannotConvertTransferRequests;
				}
				else
				{
					return Messages.CannotConvertPurchaseRequests;
				}
			}

			return Messages.CannotConvertINItemPlans;
		}

		protected virtual string GetINItemPlanName(string planType)
		{
			var planTypeField = INPlanConstants.ToInclQtyField(planType);
			if (planTypeField != null)
			{
				var cache = Base.Caches<INPlanType>();
				object val = cache.GetStateExt(null, planTypeField.Name);
				if (val is PXIntState state && !string.IsNullOrEmpty(state.DisplayName))
					return state.DisplayName;
			}

			return INPlanType.PK.Find(Base, planType)?.Descr ?? planType;
		}

		protected virtual void VerifySingleINTran(InventoryItem item)
		{
			INTran tran = SelectFrom<INTran>
				.Where<INTran.released.IsNotEqual<True>
					.And<INTran.inventoryID.IsEqual<@P.AsInt>>>
				.View.ReadOnly.Select(Base, item.InventoryID);
			if (tran != null)
			{
				throw new PXException(GetINRegisterMessage(tran.DocType), item?.InventoryCD.Trim(), tran.RefNbr);
			}
		}

		protected virtual void VerifySingleINItemPlan(InventoryItem item)
		{
			INItemPlan plan = SelectFrom<INItemPlan>
				.Where<INItemPlan.inventoryID.IsEqual<@P.AsInt>>
				.View.ReadOnly.Select(Base, item.InventoryID);
			if (plan != null)
			{
				throw new PXException(Messages.CannotConvertINItemPlans, item?.InventoryCD.Trim(), GetINItemPlanName(plan.PlanType));
			}
		}

		protected virtual void OnBeforeCommitVerifyNoTransactionsCreated(InventoryItem item)
		{
			VerifySingleINTran(item);
			VerifySingleINItemPlan(item);
		}

		#endregion // Verification

		#region Convert

		protected virtual InventoryItem ConvertInventoryItem(TConvertToGraph graph, InventoryItem source)
		{
			DeleteRelatedRecords(graph, source.InventoryID);

			PXFieldDefaulting cancelDefaulting = (c, e) => e.Cancel = true;
			graph.FieldDefaulting.AddHandler<InventoryItem.itemClassID>(cancelDefaulting);
			graph.FieldDefaulting.AddHandler<InventoryItem.parentItemClassID>(cancelDefaulting);
			graph.FieldDefaulting.AddHandler<InventoryItem.postClassID>(cancelDefaulting);

			try
			{
				var newItem = graph.Item.Insert(new InventoryItem() { IsConversionMode = true });
				graph.Item.Cache.Clear();

				newItem = ConvertMainFields(graph, source, newItem);
				newItem = ConvertPriceManagement(graph, source, newItem);
				newItem = ConvertPackaging(graph, source, newItem);
				newItem = ConvertCommerceData(graph, source, newItem);
				newItem = ConvertDeferral(graph, source, newItem);
				newItem = ConvertCurySettings(graph, source, newItem);

				newItem = graph.Item.Update(newItem);

				PXDBLocalizableStringAttribute.CopyTranslations<InventoryItem.descr, InventoryItem.descr>(graph, newItem, source);
				PXDBLocalizableStringAttribute.CopyTranslations<InventoryItem.body, InventoryItem.body>(graph, newItem, source);

				ConvertVendorInventory(graph, source, newItem);

				return newItem;
			}
			finally
			{
				graph.FieldDefaulting.RemoveHandler<InventoryItem.itemClassID>(cancelDefaulting);
				graph.FieldDefaulting.RemoveHandler<InventoryItem.parentItemClassID>(cancelDefaulting);
				graph.FieldDefaulting.RemoveHandler<InventoryItem.postClassID>(cancelDefaulting);
			}
		}

		protected virtual InventoryItem ConvertMainFields(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			newItem.NoteID = source.NoteID;
			newItem.InventoryID = source.InventoryID;
			newItem.IsConversionMode = (source.InventoryID >= 0);
			newItem.IsConverted = newItem.IsConversionMode;
			newItem.StkItem = !(bool)source.StkItem;

			newItem.InventoryCD = source.InventoryCD;
			newItem.ItemStatus = source.ItemStatus;
			newItem.Descr = source.Descr;
			newItem.ProductWorkgroupID = source.ProductWorkgroupID;
			newItem.ProductManagerID = source.ProductManagerID;
			newItem.KitItem = source.KitItem;
			newItem.TaxCategoryID = source.TaxCategoryID;
			newItem.Body = source.Body;

			newItem.BaseUnit = source.BaseUnit;
			newItem.PurchaseUnit = source.PurchaseUnit;
			newItem.SalesUnit = source.SalesUnit;
			newItem.DecimalBaseUnit = source.DecimalBaseUnit;
			newItem.DecimalPurchaseUnit = source.DecimalPurchaseUnit;
			newItem.DecimalSalesUnit = source.DecimalSalesUnit;

			newItem.tstamp = source.tstamp;

			return newItem;
		}

		protected virtual void DeleteRelatedRecords(TConvertToGraph graph, int? inventoryID)
		{
		}

		protected virtual InventoryItem ConvertPriceManagement(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			newItem.PriceClassID = source.PriceClassID;
			newItem.PriceWorkgroupID = source.PriceWorkgroupID;
			newItem.PriceManagerID = source.PriceManagerID;
			newItem.Commisionable = source.Commisionable;
			newItem.MinGrossProfitPct = source.MinGrossProfitPct;
			newItem.MarkupPct = source.MarkupPct;

			return newItem;
		}

		protected virtual InventoryItem ConvertPackaging(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			newItem.BaseItemWeight = source.BaseItemWeight;
			newItem.WeightUOM = source.WeightUOM;
			newItem.BaseItemVolume = source.BaseItemVolume;
			newItem.VolumeUOM = source.VolumeUOM;
			newItem.UndershipThreshold = source.UndershipThreshold;
			newItem.OvershipThreshold = source.OvershipThreshold;

			return newItem;
		}

		protected virtual InventoryItem ConvertCommerceData(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			newItem.ExportToExternal = source.ExportToExternal;
			newItem.Visibility = source.Visibility;
			newItem.Availability = source.Availability;
			newItem.NotAvailMode = source.NotAvailMode;

			return newItem;
		}

		protected virtual InventoryItem ConvertDeferral(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			graph.Item.SetValueExt<InventoryItem.deferredCode>(newItem, source.DeferredCode);
			newItem.UseParentSubID = source.UseParentSubID;

			return newItem;
		}

		protected virtual InventoryItem ConvertCurySettings(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			newItem.DfltSiteID = source.DfltSiteID;
			newItem.RecPrice = source.RecPrice;
			newItem.BasePrice = source.BasePrice;
			newItem.PendingStdCost = source.PendingStdCost;
			newItem.PendingStdCostDate = source.PendingStdCostDate;
			newItem.StdCost = source.StdCost;
			newItem.StdCostDate = source.StdCostDate;
			newItem.LastStdCost = source.LastStdCost;

			newItem = graph.Item.Update(newItem);

			foreach (InventoryItemCurySettings curySettings in graph.AllItemCurySettings.Select(newItem.InventoryID))
			{
				var siteID = curySettings.DfltSiteID;
				var receiptLocationID = curySettings.DfltReceiptLocationID;
				var shipLocationID = curySettings.DfltShipLocationID;

				curySettings.DfltSiteID = null;
				curySettings.DfltReceiptLocationID = null;
				curySettings.DfltShipLocationID = null;
				graph.AllItemCurySettings.Update(curySettings);

				curySettings.DfltSiteID = siteID;
				graph.AllItemCurySettings.Update(curySettings);
			}

			return newItem;
		}

		protected virtual void ConvertVendorInventory(TConvertToGraph graph, InventoryItem source, InventoryItem newItem)
		{
			foreach (var vendorInventory in SelectFrom<POVendorInventory>
											.Where<POVendorInventory.inventoryID.IsEqual<@P.AsInt>>
											.View.Select(graph, newItem.InventoryID))
			{
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.subItemID>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.addLeadTimeDays>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.minOrdFreq>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.minOrdFreq>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.minOrdQty>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.maxOrdQty>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.lotSize>(vendorInventory);
				graph.VendorItems.Cache.SetDefaultExt<POVendorInventory.eRQ>(vendorInventory);

				graph.VendorItems.Update(vendorInventory);
			}
		}

		#endregion // Convert

		#region Events

		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			if (e.Row == null)
				return;

			if (PXUIFieldAttribute.GetErrorOnly<InventoryItem.itemClassID>(e.Cache, e.Row) == null)
			{
				PXUIFieldAttribute.SetWarning<InventoryItem.itemClassID>(e.Cache, e.Row,
					e.Row.IsConversionMode == true && Base.Answers.Cache.Deleted.Any_() ? Messages.DeletedAttributesOnConvert : null);
			}
		}

		protected virtual void _(Events.RowSelected<CSAnswers> e)
		{
			if (e.Row == null)
				return;

			var itemClass = Base.Item.Current?.ParentItemClassID;
			if (itemClass != null)
			{
				var attributes = CRAttribute.EntityAttributes(typeof(InventoryItem), itemClass.ToString());
				if (attributes.FirstOrDefault()?.ID == e.Row.AttributeID)
				{
					string error = (Base.Item.Current?.IsConversionMode == true && Base.Answers.Cache.Deleted.Any_()) ?
						Messages.DeletedAttributesOnConvert : null;

					PXUIFieldAttribute.SetError(e.Cache, e.Row, nameof(CSAnswers.attributeID), error, null, false, PXErrorLevel.RowWarning);
				}
			}
		}
		
		protected virtual void _(Events.RowPersisting<InventoryItem> e)
		{
			if (e.Row.IsConversionMode == true && e.Operation.Command() == PXDBOperation.Update)
			{
				if (_persistingConvertedItems == null)
					_persistingConvertedItems = new HashSet<InventoryItem>(e.Cache.GetComparer());
				_persistingConvertedItems.Add(e.Row);
			}
		}

		protected virtual void _(Events.RowPersisted<InventoryItem> e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
				e.Row.IsConversionMode = false;
		}

		#endregion Events
	}
}
