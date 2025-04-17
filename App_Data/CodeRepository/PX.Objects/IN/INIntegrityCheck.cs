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

using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN.GraphExtensions;
using PX.Objects.IN.InventoryRelease;
using PX.Objects.IN.InventoryRelease.Accumulators.ItemHistory;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction;
using PX.Objects.IN.InventoryRelease.Accumulators.Statistics.ItemCustomer;

using POReceiptLineSplit = PX.Objects.PO.POReceiptLineSplit;
using SOShipLineSplit = PX.Objects.SO.Table.SOShipLineSplit;
using PX.Objects.IN.InventoryRelease.DAC;

namespace PX.Objects.IN
{
	#region Helper DACs
	[PXHidden]
	public partial class ReadOnlyLocationStatusByCostCenter : INLocationStatusByCostCenter
	{
		#region InventoryID
		public new abstract class inventoryID : BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : BqlInt.Field<subItemID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public new abstract class siteID : BqlInt.Field<siteID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public new abstract class locationID : BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? LocationID
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public new abstract class costCenterID : BqlInt.Field<costCenterID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}

	[Serializable]
	[PXHidden]
    public partial class INItemSiteSummary : INItemSite 
    {
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
    }

	#endregion

	[PX.Objects.GL.TableAndChartDashboardType]
	public class INIntegrityCheck : PXGraph<INIntegrityCheck>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ItemPlanHelper : ItemPlanHelper<INIntegrityCheck>
		{
		}
		public ItemPlanHelper ItemPlanHelperExt => FindImplementation<ItemPlanHelper>();

		#region Actions
		public PXCancel<INRecalculateInventoryFilter> Cancel;
		#endregion

		#region Views		
		public PXFilter<INRecalculateInventoryFilter> Filter;
		[PXFilterable]
        public PXFilteredProcessingJoin<InventoryItemCommon,
			INRecalculateInventoryFilter,
            LeftJoin<INSiteStatusSummary,
				On<INSiteStatusSummary.inventoryID, Equal<InventoryItemCommon.inventoryID>,
				And<INSiteStatusSummary.siteID, Equal<Current<INRecalculateInventoryFilter.siteID>>>>>,
			Where<InventoryItemCommon.itemStatus.IsNotIn<InventoryItemStatus.unknown, InventoryItemStatus.inactive>
				.And<InventoryItemCommon.isTemplate.IsEqual<False>>>,
            OrderBy<Asc<InventoryItemCommon.inventoryCD>>>
			INItemList;
		public PXSetup<INSetup> insetup;
		public PXSelect<INItemSite> itemsite;
		public PXSelect<SiteStatusByCostCenter> sitestatusbycostcenter;
		public PXSelect<LocationStatusByCostCenter> locationstatusbycostcenter;
		public PXSelect<LotSerialStatusByCostCenter> lotserialstatusbycostcenter;
        public PXSelect<ItemLotSerial> itemlotserial;
		public PXSelect<SiteLotSerial> sitelotserial;
		public PXSelect<INItemPlan> initemplan;

		public PXSelect<ItemSiteHist> itemsitehist;
		public PXSelect<ItemSiteHistByCostCenterD> itemsitehistbycostcenterd;
        public PXSelect<ItemSiteHistDay> itemsitehistday;
        public PXSelect<ItemCostHist> itemcosthist;
		public PXSelect<ItemSalesHistD> itemsalehistd;
		public PXSelect<ItemCustSalesStats> itemcustsalesstats;
		public PXSelect<ItemCustDropShipStats> itemcustdropshipstats;
		#endregion

		public INIntegrityCheck()
		{
			INSetup record = insetup.Current;

			INItemList.SuppressUpdate = true;
			INItemList.SetProcessCaption(Messages.Process);
			INItemList.SetProcessAllCaption(Messages.ProcessAll);

			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.refNbr>(this.Caches[typeof(INTranSplit)], null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<INTranSplit.tranDate>(this.Caches[typeof(INTranSplit)], null, false);
		}

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);

			this.Caches.AddCacheMapping(typeof(INSiteStatusByCostCenter), typeof(INSiteStatusByCostCenter));
			this.Caches.AddCacheMapping(typeof(SiteStatusByCostCenter), typeof(SiteStatusByCostCenter));
		}

		#region Items loading

		protected virtual PXView CreateItemsLoadView(INRecalculateInventoryFilter filter, List<object> parameters)
		{
			if (filter?.SiteID == null)
				return null;

			var cmd = INItemList.View.BqlSelect;
			cmd = AppendFilter(cmd, parameters, filter);
			var view = new PXView(this, INItemList.View.IsReadOnly, cmd);

			return view;
		}

		protected IEnumerable initemlist()
		{
			var parameters = new List<object>();
			var view = CreateItemsLoadView(Filter.Current, parameters);
			if (view == null)
				return Array<object>.Empty;

			var startRow = PXView.StartRow;
			int totalRows = 0;

			var result = view.Select(PXView.Currents, parameters.ToArray(),
					PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					ref startRow, PXView.MaximumRows, ref totalRows);

			PXView.StartRow = 0;

			return result;
		}

		public virtual BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, INRecalculateInventoryFilter filter)
		{
			if (filter.ItemClassID != null)
			{
				cmd = cmd.WhereAnd<Where<InventoryItemCommon.itemClassID.IsEqual<@P.AsInt>>>();
				parameters.Add(filter.ItemClassID);
			}

			if (filter.ShowOnlyAllocatedItems == true)
			{
				cmd = cmd.WhereAnd<Where<Exists<
					SelectFrom<INItemPlan>
						.Where<INItemPlan.siteID.IsEqual<@P.AsInt>
						.And<INItemPlan.inventoryID.IsEqual<InventoryItemCommon.inventoryID>>
						.And<INItemPlan.planQty.IsNotEqual<decimal0>>>>>>();
				parameters.Add(filter.SiteID);
			}

			return cmd;
		}

		#endregion

		#region Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<LocationStatusByCostCenter.negQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<LotSerialStatusByCostCenter.negActualQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<SiteLotSerial.negQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<SiteLotSerial.negActualQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<SiteStatusByCostCenter.negQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUnboundDefault(true)]
		protected virtual void _(Events.CacheAttached<SiteStatusByCostCenter.negAvailQty> e) { }

		#endregion

		#region Event handlers

		protected virtual void _(Events.RowSelected<INRecalculateInventoryFilter> e)
		{
			if (e.Row == null) return;

			INRecalculateInventoryFilter filter = e.Row;

            INItemList.SetProcessDelegate<INIntegrityCheck>(delegate(INIntegrityCheck graph, InventoryItemCommon item)
			{
				graph.Clear(PXClearOption.PreserveTimeStamp);
				graph.IntegrityCheckProc(new INItemSiteSummary { InventoryID = item.InventoryID, SiteID = filter?.SiteID }, filter?.RebuildHistory == true ? filter.FinPeriodID : null, filter?.ReplanBackorders == true);
			});
			PXUIFieldAttribute.SetEnabled<INRecalculateInventoryFilter.finPeriodID>(e.Cache, null, filter.RebuildHistory == true);
		}
		#endregion

		public TNode UpdateAllocatedQuantities<TNode>(INItemPlan plan, INPlanType plantype, bool InclQtyAvail)
			where TNode : class, IQtyAllocatedBase
		{
			INPlanType targettype = ItemPlanHelperExt.GetTargetPlanType<TNode>(plan, plantype);
			return ItemPlanHelperExt.UpdateAllocatedQuantitiesBase<TNode>(plan, targettype, InclQtyAvail);
		}

		public virtual void IntegrityCheckProc(INItemSiteSummary itemsite, string minPeriod, bool replanBackorders)
		{
			using (var ts = new PXTransactionScope())
			{
				DeleteOrphanedItemPlans(itemsite);

				CreateItemPlansForTransit(itemsite);

				DeleteLotSerialStatusForNotTrackedItems(itemsite);

				ClearSiteStatusAllocatedQuantities(itemsite);
				ClearLocationStatusAllocatedQuantities(itemsite);
				ClearLotSerialStatusAllocatedQuantities(itemsite);

				PopulateSiteAvailQtyByLocationStatus(itemsite);

				UpdateAllocatedQuantitiesWithExistingPlans(itemsite);

				ReplanBackOrders(replanBackorders);

				PersistCaches();

				RebuildItemHistory(minPeriod, itemsite);

				DeleteZeroStatusRecords(itemsite);

				ts.Complete();
			}

			OnCachePersisted();
		}

		protected virtual void PersistCaches()
        {
			Caches[typeof(INTranSplit)].Persist(PXDBOperation.Update);

			sitestatusbycostcenter.Cache.Persist(PXDBOperation.Insert);
			sitestatusbycostcenter.Cache.Persist(PXDBOperation.Update);

			locationstatusbycostcenter.Cache.Persist(PXDBOperation.Insert);
			locationstatusbycostcenter.Cache.Persist(PXDBOperation.Update);

			lotserialstatusbycostcenter.Cache.Persist(PXDBOperation.Insert);
			lotserialstatusbycostcenter.Cache.Persist(PXDBOperation.Update);

			itemlotserial.Cache.Persist(PXDBOperation.Insert);
			itemlotserial.Cache.Persist(PXDBOperation.Update);

			sitelotserial.Cache.Persist(PXDBOperation.Insert);
			sitelotserial.Cache.Persist(PXDBOperation.Update);
		}

		protected virtual void OnCachePersisted()
        {
			initemplan.Cache.Persisted(false);
			Caches[typeof(INTranSplit)].Persisted(false);
			sitestatusbycostcenter.Cache.Persisted(false);
			locationstatusbycostcenter.Cache.Persisted(false);
			lotserialstatusbycostcenter.Cache.Persisted(false);
			itemlotserial.Cache.Persisted(false);
			sitelotserial.Cache.Persisted(false);

			itemcosthist.Cache.Persisted(false);
			itemsitehist.Cache.Persisted(false);
			itemsitehistbycostcenterd.Cache.Persisted(false);
			itemsitehistday.Cache.Persisted(false);
			itemsalehistd.Cache.Persisted(false);
			itemcustsalesstats.Cache.Persisted(false);
			itemcustdropshipstats.Cache.Persisted(false);
		}

		protected virtual void DeleteOrphanedItemPlans(INItemSiteSummary itemsite)
		{
			DeleteItemPlansWithoutParentDocument(itemsite);

			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				InnerJoin<INRegister, On<INRegister.noteID, Equal<INItemPlan.refNoteID>>>,
				Where<INRegister.released, Equal<boolTrue>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d22.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<PO.POReceipt, On<PO.POReceipt.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<POReceiptLineSplit, On<POReceiptLineSplit.receiptNbr, Equal<PO.POReceipt.receiptNbr>
				, And<POReceiptLineSplit.receiptType, Equal<PO.POReceipt.receiptType>
				, And<POReceiptLineSplit.planID, Equal<INItemPlan.planID>>>>>>,
				Where<POReceiptLineSplit.receiptNbr, IsNull,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
				And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d32.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<SOOrder, On<SOOrder.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOOrder.orderType>
				, And<SOLineSplit.orderNbr, Equal<SOOrder.orderNbr>
				, And<SOLineSplit.planID, Equal<INItemPlan.planID>>>>>>,
				Where<SOLineSplit.orderNbr, IsNull,
					And<INItemPlan.planType, NotEqual<INPlanConstants.plan64>,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d33.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
			InnerJoin<SOShipment, On<SOShipment.noteID, Equal<INItemPlan.refNoteID>>,
			LeftJoin<SOShipLineSplit, On<SOShipLineSplit.shipmentNbr, Equal<SOShipment.shipmentNbr>
				, And<SOShipLineSplit.planID, Equal<INItemPlan.planID>>>>>,
				Where<SOShipLineSplit.shipmentNbr, IsNull,
				And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
				And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}

			//d128.
			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				LeftJoin<INItemPlanSupply, On<INItemPlanSupply.planID, Equal<INItemPlan.supplyPlanID>>>,
				Where<INItemPlanSupply.planID, IsNull,
					And<INItemPlan.supplyPlanID, IsNotNull,
					And<INItemPlan.planType, Equal<INPlanConstants.plan94>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}
		}

		protected virtual void DeleteItemPlansWithoutParentDocument(INItemSiteSummary itemsite)
		{
			Type[] knownNoteFields = GetParentDocumentsNoteFields();

			var docTypeNames = new List<string>();

			foreach (var noteField in knownNoteFields)
			{
				Type docType = BqlCommand.GetItemType(noteField);

				var command = BqlTemplate.OfCommand<Select2<INItemPlan,
					LeftJoin<BqlPlaceholder.E, On<BqlPlaceholder.N, Equal<INItemPlan.refNoteID>>>,
					Where<INItemPlan.refEntityType, Equal<Required<INItemPlan.refEntityType>>,
						And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
						And<BqlPlaceholder.N, IsNull>>>>>>
					.Replace<BqlPlaceholder.E>(docType)
					.Replace<BqlPlaceholder.N>(noteField)
					.ToCommand();

				var view = new PXView(this, true, command);

				string docTypeName = docType.FullName;

				foreach (PXResult<INItemPlan> row in view.SelectMultiBound(new object[] { itemsite }, new object[] { docTypeName }))
				{
					INItemPlan p = row;
					PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
				}

				docTypeNames.Add(docTypeName);
			}

			foreach (INItemPlan p in PXSelectReadonly2<INItemPlan,
				LeftJoin<Note, On<Note.noteID, Equal<INItemPlan.refNoteID>>>,
				Where<INItemPlan.refEntityType, NotIn<Required<INItemPlan.refEntityType>>,
					And<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<Note.noteID, IsNull>>>>>
				.SelectMultiBound(this, new object[] { itemsite }, new object[] { docTypeNames.ToArray() }))
			{
				PXDatabase.Delete<INItemPlan>(new PXDataFieldRestrict<INItemPlan.planID>(PXDbType.BigInt, 8, p.PlanID, PXComp.EQ));
			}
		}

		public virtual Type[] GetParentDocumentsNoteFields()
		{
			List<Type> knownNoteFields = new List<Type>() {
				typeof(SOOrder.noteID),
				typeof(SOShipment.noteID),
				typeof(PO.POOrder.noteID),
				typeof(PO.POReceipt.noteID),
				typeof(INRegister.noteID),
				typeof(INTransitLine.noteID),
			};

			if (PXAccess.FeatureInstalled<FeaturesSet.replenishment>())
				knownNoteFields.Add(typeof(INReplenishmentOrder.noteID));

			if (PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>())
				knownNoteFields.Add(typeof(INKitRegister.noteID));

			return knownNoteFields.ToArray();
		}

		protected virtual void CreateItemPlansForTransit(INItemSiteSummary itemsite)
		{
			var transferGraph = CreateInstance<INTransferEntry>();
			foreach (PXResult<INLocationStatusInTransit, INTransitLine> res in PXSelectJoin<INLocationStatusInTransit,
					InnerJoin<INTransitLine, On<INTransitLine.costSiteID, Equal<INLocationStatusInTransit.locationID>>,
					LeftJoin<INItemPlan, On<INItemPlan.refNoteID, Equal<INTransitLine.noteID>>>>,
					Where<INLocationStatusInTransit.qtyOnHand, Greater<decimal0>,
						And<INLocationStatusInTransit.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INTransitLine.toSiteID, Equal<Current<INItemSiteSummary.siteID>>,
						And<INItemPlan.planID, IsNull>>>>>
					.SelectMultiBound(transferGraph, new object[] { itemsite }))
			{
				INItemPlan plan;
				var locst = (INLocationStatusInTransit)res;
				var tl = (INTransitLine)res;

				foreach (TransitLotSerialStatusByCostCenter tlss in
					PXSelect<TransitLotSerialStatusByCostCenter,
					Where<TransitLotSerialStatusByCostCenter.locationID, Equal<Current<INLocationStatusInTransit.locationID>>,
						And<TransitLotSerialStatusByCostCenter.inventoryID, Equal<Current<INLocationStatusInTransit.inventoryID>>,
						And<TransitLotSerialStatusByCostCenter.subItemID, Equal<Current<INLocationStatusInTransit.subItemID>>,
						And<TransitLotSerialStatusByCostCenter.costCenterID, Equal<Current<INLocationStatusInTransit.costCenterID>>,
						And<TransitLotSerialStatusByCostCenter.qtyOnHand, Greater<decimal0>>>>>>>
					.SelectMultiBound(transferGraph, new object[] { locst }))
				{
					plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].CreateInstance();
					plan.PlanType = tl.SOShipmentNbr == null ? INPlanConstants.Plan42 : INPlanConstants.Plan44;
					plan.InventoryID = tlss.InventoryID;
					plan.SubItemID = tlss.SubItemID ?? locst.SubItemID;
					plan.LotSerialNbr = tlss.LotSerialNbr;
					plan.CostCenterID = tlss.CostCenterID;
					plan.SiteID = tl.ToSiteID;
					plan.LocationID = tl.ToLocationID;
					plan.FixedSource = INReplenishmentSource.Purchased;
					plan.PlanDate = tl.CreatedDateTime;
					plan.Reverse = false;
					plan.Hold = false;
					plan.PlanQty = tlss.QtyOnHand;
					locst.QtyOnHand -= tlss.QtyOnHand;
					plan.RefNoteID = tl.NoteID;
					plan.RefEntityType = typeof(INTransitLine).FullName;
					plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].Insert(plan);
				}

				if (locst.QtyOnHand <= 0m)
					continue;
				plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].CreateInstance();
				plan.PlanType = tl.SOShipmentNbr == null ? INPlanConstants.Plan42 : INPlanConstants.Plan44;
				plan.InventoryID = locst.InventoryID;
				plan.SubItemID = locst.SubItemID;
				plan.SiteID = tl.ToSiteID;
				plan.LocationID = tl.ToLocationID;
				plan.CostCenterID = locst.CostCenterID;
				plan.FixedSource = INReplenishmentSource.Purchased;
				plan.PlanDate = tl.CreatedDateTime;
				plan.Reverse = false;
				plan.Hold = false;
				plan.PlanQty = locst.QtyOnHand;
				plan.RefNoteID = tl.NoteID;
				plan.RefEntityType = typeof(INTransitLine).FullName;
				plan = (INItemPlan)transferGraph.Caches[typeof(INItemPlan)].Insert(plan);
			}
			transferGraph.Save.Press();
		}

		protected virtual void DeleteLotSerialStatusForNotTrackedItems(INItemSiteSummary itemsite)
		{
			//Deleting records from INLotSerialStatus, INItemLotSerial, INSiteLotSerial if item is not Lot/Serial tracked any more
			InventoryItem notTrackedItem = PXSelectReadonly2<InventoryItem,
				InnerJoin<INLotSerClass,
					On2<InventoryItem.FK.LotSerialClass, And<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.notNumbered>>>,
				InnerJoin<INLotSerialStatusByCostCenter, On<INLotSerialStatusByCostCenter.FK.InventoryItem>>>,
				Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.SelectWindowed(this, 0, 1, itemsite.InventoryID);
			if (notTrackedItem != null && !InventoryItemMaint.IsQtyStillPresent(this, itemsite.InventoryID))
			{
				DeleteLotSerialStatusForNotTrackedItemsByItem(itemsite.InventoryID);
			}
		}

		protected virtual void DeleteLotSerialStatusForNotTrackedItemsByItem(int? inventoryID)
		{
			PXDatabase.Delete<INLotSerialStatusByCostCenter>(
				new PXDataFieldRestrict<INLotSerialStatusByCostCenter.inventoryID>(PXDbType.Int, 4, inventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INLotSerialStatusByCostCenter.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
			);

			PXDatabase.Delete<INItemLotSerial>(
				new PXDataFieldRestrict<INItemLotSerial.inventoryID>(PXDbType.Int, 4, inventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemLotSerial.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
			);

			PXDatabase.Delete<INSiteLotSerial>(
				new PXDataFieldRestrict<INSiteLotSerial.inventoryID>(PXDbType.Int, 4, inventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INSiteLotSerial.qtyOnHand>(PXDbType.Decimal, 4, 0m, PXComp.EQ)
			);
		}

		protected virtual void ClearSiteStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INSiteStatusByCostCenter>(
				AssignAllDBDecimalFieldsToZeroCommand(sitestatusbycostcenter.Cache,
					excludeFields: new string[]
					{
						nameof(INSiteStatusByCostCenter.qtyOnHand)
					})
				.Append(new PXDataFieldRestrict<INSiteStatusByCostCenter.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INSiteStatusByCostCenter.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void ClearLocationStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INLocationStatusByCostCenter>(
				AssignAllDBDecimalFieldsToZeroCommand(locationstatusbycostcenter.Cache,
					excludeFields: new string[]
					{
						nameof(INLocationStatusByCostCenter.qtyOnHand),
						nameof(INLocationStatusByCostCenter.qtyAvail),
						nameof(INLocationStatusByCostCenter.qtyHardAvail),
						nameof(INLocationStatusByCostCenter.qtyActual)
					})
				.Append(new PXDataFieldAssign<INLocationStatusByCostCenter.qtyAvail>(PXDbType.DirectExpression, nameof(INLocationStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLocationStatusByCostCenter.qtyHardAvail>(PXDbType.DirectExpression, nameof(INLocationStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLocationStatusByCostCenter.qtyActual>(PXDbType.DirectExpression, nameof(INLocationStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INLocationStatusByCostCenter.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INLocationStatusByCostCenter.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void ClearLotSerialStatusAllocatedQuantities(INItemSiteSummary itemsite)
		{
			PXDatabase.Update<INLotSerialStatusByCostCenter>(
				AssignAllDBDecimalFieldsToZeroCommand(lotserialstatusbycostcenter.Cache,
					excludeFields: new string[]
					{
						nameof(INLotSerialStatusByCostCenter.qtyOnHand),
						nameof(INLotSerialStatusByCostCenter.qtyAvail),
						nameof(INLotSerialStatusByCostCenter.qtyHardAvail),
						nameof(INLotSerialStatusByCostCenter.qtyActual)
					})
				.Append(new PXDataFieldAssign<INLotSerialStatusByCostCenter.qtyAvail>(PXDbType.DirectExpression, nameof(INLotSerialStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLotSerialStatusByCostCenter.qtyHardAvail>(PXDbType.DirectExpression, nameof(INLotSerialStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldAssign<INLotSerialStatusByCostCenter.qtyActual>(PXDbType.DirectExpression, nameof(INLotSerialStatusByCostCenter.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INLotSerialStatusByCostCenter.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INLotSerialStatusByCostCenter.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());

			PXDatabase.Update<INItemLotSerial>(
				AssignAllDBDecimalFieldsToZeroCommand(itemlotserial.Cache,
					excludeFields: new string[]
					{
						nameof(INItemLotSerial.qtyOnHand),
						nameof(INItemLotSerial.qtyAvail),
						nameof(INItemLotSerial.qtyHardAvail),
						nameof(INItemLotSerial.qtyActual),
						nameof(INItemLotSerial.qtyOrig)
					})
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyAvail>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyHardAvail>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INItemLotSerial.qtyActual>(PXDbType.DirectExpression, nameof(INItemLotSerial.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INItemLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.ToArray());

			PXDatabase.Update<INSiteLotSerial>(
				AssignAllDBDecimalFieldsToZeroCommand(sitelotserial.Cache,
					excludeFields: new string[]
					{
						nameof(INSiteLotSerial.qtyOnHand),
						nameof(INSiteLotSerial.qtyAvail),
						nameof(INSiteLotSerial.qtyHardAvail),
						nameof(INSiteLotSerial.qtyActual)
					})
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyAvail>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyHardAvail>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldAssign<INSiteLotSerial.qtyActual>(PXDbType.DirectExpression, nameof(INSiteLotSerial.QtyOnHand)))
				.Append(new PXDataFieldRestrict<INSiteLotSerial.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INSiteLotSerial.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.ToArray());
		}

		protected virtual void PopulateSiteAvailQtyByLocationStatus(INItemSiteSummary itemsite)
		{
			foreach (PXResult<ReadOnlyLocationStatusByCostCenter, INLocation> res in PXSelectJoinGroupBy<ReadOnlyLocationStatusByCostCenter,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<ReadOnlyLocationStatusByCostCenter.locationID>>>,
				Where<ReadOnlyLocationStatusByCostCenter.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<ReadOnlyLocationStatusByCostCenter.siteID, Equal<Current<INItemSiteSummary.siteID>>>>,
				Aggregate<GroupBy<ReadOnlyLocationStatusByCostCenter.inventoryID,
					GroupBy<ReadOnlyLocationStatusByCostCenter.siteID,
					GroupBy<ReadOnlyLocationStatusByCostCenter.subItemID,
					GroupBy<ReadOnlyLocationStatusByCostCenter.costCenterID,
					GroupBy<INLocation.inclQtyAvail,
					Sum<ReadOnlyLocationStatusByCostCenter.qtyOnHand>>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				ReadOnlyLocationStatusByCostCenter locStatus = res;
				var status = new SiteStatusByCostCenter
				{
					InventoryID = locStatus.InventoryID,
					SubItemID = locStatus.SubItemID,
					SiteID = locStatus.SiteID,
					CostCenterID = locStatus.CostCenterID,
				};
				status = sitestatusbycostcenter.Insert(status);

				if (((INLocation)res).InclQtyAvail == true)
				{
					status.QtyAvail += locStatus.QtyOnHand;
					status.QtyHardAvail += locStatus.QtyOnHand;
					status.QtyActual += locStatus.QtyOnHand;
				}
				else
				{
					status.QtyNotAvail += locStatus.QtyOnHand;
				}
			}

			foreach (PXResult<ReadOnlyLotSerialStatus, INLocation> res in PXSelectJoinGroupBy<ReadOnlyLotSerialStatus,
				InnerJoin<INLocation, On<INLocation.locationID, Equal<ReadOnlyLotSerialStatus.locationID>>>,
				Where<ReadOnlyLotSerialStatus.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<ReadOnlyLotSerialStatus.siteID, Equal<Current<INItemSiteSummary.siteID>>>>,
				Aggregate<GroupBy<ReadOnlyLotSerialStatus.inventoryID,
					GroupBy<ReadOnlyLotSerialStatus.siteID,
					GroupBy<ReadOnlyLotSerialStatus.lotSerialNbr,
					GroupBy<INLocation.inclQtyAvail,
					Sum<ReadOnlyLotSerialStatus.qtyOnHand>>>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				ReadOnlyLotSerialStatus locStatus = res;
				var status = new SiteLotSerial
				{
					InventoryID = locStatus.InventoryID,
					SiteID = locStatus.SiteID,
					LotSerialNbr = locStatus.LotSerialNbr
				};
				status = sitelotserial.Insert(status);

				if (((INLocation)res).InclQtyAvail == true)
				{
					status.QtyAvailOnSite += locStatus.QtyOnHand;
				}
				else
				{
					status.QtyNotAvail += locStatus.QtyOnHand;
				}
			}
		}

		protected virtual void UpdateAllocatedQuantitiesWithExistingPlans(INItemSiteSummary itemsite)
		{
			foreach (PXResult<INItemPlan, InventoryItem> res in PXSelectJoin<INItemPlan,
				InnerJoin<InventoryItem, On<INItemPlan.FK.InventoryItem>>,
				Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INItemPlan.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<InventoryItem.stkItem, Equal<boolTrue>>>>>
				.SelectMultiBound(this, new object[] { itemsite }))
            {
                INItemPlan plan = (INItemPlan)res;
                INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);

				UpdateAllocatedQuantitiesWithPlans(itemsite, plan, plantype);
            }

            //Updating cross-site ItemLotSerial
            foreach (INItemPlan plan in PXSelect<INItemPlan,
					Where<INItemPlan.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
						And<INItemPlan.lotSerialNbr, NotEqual<StringEmpty>,
						And<INItemPlan.lotSerialNbr, IsNotNull>>>>
					.SelectMultiBound(this, new object[] { itemsite }))
			{
				INPlanType plantype = INPlanType.PK.Find(this, plan.PlanType);

				if (plan.InventoryID != null &&
					plan.SubItemID != null &&
					plan.SiteID != null)
				{
					if (plan.LocationID != null)
					{
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
					}
					else
					{
						UpdateAllocatedQuantities<ItemLotSerial>(plan, plantype, true);
					}
				}
			}
		}

		protected virtual void UpdateAllocatedQuantitiesWithPlans(INItemSiteSummary itemsite, INItemPlan plan, INPlanType plantype)
        {
            if (plan.InventoryID != null &&
                plan.SubItemID != null &&
                plan.SiteID != null)
            {
                if (plan.LocationID != null)
                {
					LocationStatusByCostCenter itemByCostCenter = UpdateAllocatedQuantities<LocationStatusByCostCenter>(plan, plantype, true);
					UpdateAllocatedQuantities<SiteStatusByCostCenter>(plan, plantype, (bool)itemByCostCenter.InclQtyAvail);
                    if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                    {
						UpdateAllocatedQuantities<LotSerialStatusByCostCenter>(plan, plantype, true);
                        UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, (bool)itemByCostCenter.InclQtyAvail);
                    }
                }
                else
                {
					UpdateAllocatedQuantities<SiteStatusByCostCenter>(plan, plantype, true);
                    if (!string.IsNullOrEmpty(plan.LotSerialNbr))
                    {
                        //TODO: check if LotSerialNbr was allocated on OrigPlanType
                        UpdateAllocatedQuantities<SiteLotSerial>(plan, plantype, true);
                    }
                }
            }
        }

        protected virtual void ReplanBackOrders(bool replanBackorders)
		{
			if (!replanBackorders) return;

			INReleaseProcess.ReplanBackOrders(this);
			initemplan.Cache.Persist(PXDBOperation.Insert);
			initemplan.Cache.Persist(PXDBOperation.Update);
		}

		protected virtual void RebuildItemHistory(string minPeriod, INItemSiteSummary itemsite)
		{
			if (minPeriod == null)
				return;

			MasterFinPeriod period =
				PXSelect<MasterFinPeriod,
					Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>
					.SelectWindowed(this, 0, 1, minPeriod);
			if (period == null) return;
			DateTime startDate = (DateTime)period.StartDate;

			PXDatabase.Delete<INItemCostHist>(
				new PXDataFieldRestrict<INItemCostHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCostHist.costSiteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCostHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));

			PXDatabase.Update<INItemSalesHistD>(
				AssignAllDBDecimalFieldsToZeroCommand(itemsalehistd.Cache,
					excludeFields: new string[]
					{
						nameof(INItemSalesHistD.qtyPlanSales),
						nameof(INItemSalesHistD.qtyLostSales)
					})
				.Append(new PXDataFieldRestrict<INItemSalesHistD.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSalesHistD.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSalesHistD.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE))
				.ToArray());

			PXDatabase.Update<INItemCustSalesStats>(
				new PXDataFieldAssign<INItemCustSalesStats.lastQty>(PXDbType.Decimal, null),
				new PXDataFieldAssign<INItemCustSalesStats.lastDate>(PXDbType.DateTime, null),
				new PXDataFieldAssign<INItemCustSalesStats.lastUnitPrice>(PXDbType.Decimal, null),
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.lastDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			PXDatabase.Update<INItemCustSalesStats>(
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastQty>(PXDbType.Decimal, null),
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, null),
				new PXDataFieldAssign<INItemCustSalesStats.dropShipLastUnitPrice>(PXDbType.Decimal, null),
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			PXDatabase.Delete<INItemCustSalesStats>(
				new PXDataFieldRestrict<INItemCustSalesStats.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemCustSalesStats.lastDate>(PXDbType.DateTime, 8, startDate, PXComp.ISNULL),
				new PXDataFieldRestrict<INItemCustSalesStats.dropShipLastDate>(PXDbType.DateTime, 8, startDate, PXComp.ISNULL));

			foreach (INLocation loc in PXSelectJoinGroupBy<INLocation,
				InnerJoin<INItemCostHist, On<INItemCostHist.costSiteID, Equal<INLocation.locationID>>>,
				Where<INLocation.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INItemCostHist.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>>>,
				Aggregate<GroupBy<INLocation.locationID>>>
				.SelectMultiBound(this, new object[] { itemsite }))
			{
				PXDatabase.Delete<INItemCostHist>(
					new PXDataFieldRestrict<INItemCostHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
					new PXDataFieldRestrict<INItemCostHist.costSiteID>(PXDbType.Int, 4, loc.LocationID, PXComp.EQ),
					new PXDataFieldRestrict<INItemCostHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));
			}

			PXDatabase.Delete<INItemSiteHist>(
				new PXDataFieldRestrict<INItemSiteHist.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHist.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHist.finPeriodID>(PXDbType.Char, 6, minPeriod, PXComp.GE));

			PXDatabase.Update<INItemSiteHistByCostCenterD>(
				AssignAllDBDecimalFieldsToZeroCommand(itemsitehistbycostcenterd.Cache)
				.Append(new PXDataFieldRestrict<INItemSiteHistByCostCenterD.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSiteHistByCostCenterD.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ))
				.Append(new PXDataFieldRestrict<INItemSiteHistByCostCenterD.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE))
				.ToArray());

			PXDatabase.Delete<INItemSiteHistDay>(
				new PXDataFieldRestrict<INItemSiteHistDay.inventoryID>(PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHistDay.siteID>(PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				new PXDataFieldRestrict<INItemSiteHistDay.sDate>(PXDbType.DateTime, 8, startDate, PXComp.GE));

			INTran prev_tran = null;
			var splitsQuery = new PXSelectReadonly2<INTran,
				LeftJoin<INTranSplit,
					On<INTranSplit.FK.Tran>>,
				Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INTran.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>,
					And<INTran.released, Equal<boolTrue>>>>>,
				OrderBy<Asc<INTran.tranType, Asc<INTran.refNbr, Asc<INTran.lineNbr>>>>>(this);

			foreach (PXResult<INTran, INTranSplit> res in splitsQuery.View.SelectMultiBound(new object[] { itemsite }, minPeriod))
			{
				INTran tran = res;
				INTranSplit split = res;

				if (!Caches[typeof(INTran)].ObjectsEqual(prev_tran, tran))
				{
					INReleaseProcess.UpdateSalesHistD(this, tran);
					INReleaseProcess.UpdateCustSalesStats(this, tran);

					prev_tran = tran;
				}

				if ((split.BaseQty ?? 0) != 0m)
				{
					INReleaseProcess.UpdateSiteHist(this, res, split);
					INReleaseProcess.UpdateSiteHistByCostCenterD(this, res, split);
					INReleaseProcess.UpdateSiteHistDay(this, res, split);
				}
			}

			var tranCostsQuery = new
				PXSelectReadonly2<INTran,
				InnerJoin<INTranCost,
					On<INTranCost.FK.Tran>>,
				Where<INTran.inventoryID, Equal<Current<INItemSiteSummary.inventoryID>>,
					And<INTran.siteID, Equal<Current<INItemSiteSummary.siteID>>,
					And<INTranCost.finPeriodID, GreaterEqual<Required<INTran.finPeriodID>>,
					And<INTran.released, Equal<boolTrue>,
					And<INTranCost.costSiteID, NotEqual<SiteAttribute.transitSiteID>>>>>>>(this);

			foreach (PXResult<INTran, INTranCost> res in tranCostsQuery.View.SelectMultiBound(new object[] { itemsite }, minPeriod))
			{
				INReleaseProcess.UpdateCostHist(this, res, res);
				INReleaseProcess.UpdateSiteHistByCostCenterDCost(this, res, res);
			}

			itemcosthist.Cache.Persist(PXDBOperation.Insert);
			itemcosthist.Cache.Persist(PXDBOperation.Update);

			itemsitehist.Cache.Persist(PXDBOperation.Insert);
			itemsitehist.Cache.Persist(PXDBOperation.Update);

			itemsitehistbycostcenterd.Cache.Persist(PXDBOperation.Insert);
			itemsitehistbycostcenterd.Cache.Persist(PXDBOperation.Update);

			itemsitehistday.Cache.Persist(PXDBOperation.Insert);
			itemsitehistday.Cache.Persist(PXDBOperation.Update);

			itemsalehistd.Cache.Persist(PXDBOperation.Insert);
			itemsalehistd.Cache.Persist(PXDBOperation.Update);

			itemcustsalesstats.Cache.Persist(PXDBOperation.Insert);
			itemcustsalesstats.Cache.Persist(PXDBOperation.Update);

			itemcustdropshipstats.Cache.Persist(PXDBOperation.Insert);
			itemcustdropshipstats.Cache.Persist(PXDBOperation.Update);
		}

		public virtual IEnumerable<PXDataFieldParam> AssignAllDBDecimalFieldsToZeroCommand(PXCache cache, params string[] excludeFields)
		{
			return cache.GetAllDBDecimalFields(excludeFields)
				.Select(f => new PXDataFieldAssign(f, PXDbType.Decimal, decimal.Zero));
		}

		public virtual void DeleteZeroStatusRecords(INItemSiteSummary itemsite)
		{
			var restrictions = new List<PXDataFieldRestrict>
			{
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.InventoryID), PXDbType.Int, 4, itemsite.InventoryID, PXComp.EQ),
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.SiteID), PXDbType.Int, 4, itemsite.SiteID, PXComp.EQ),
				// just for reliability as it may cause very sensitive data loss
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.QtyOnHand), PXDbType.Decimal, decimal.Zero),
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.QtyAvail), PXDbType.Decimal, decimal.Zero),
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.QtyHardAvail), PXDbType.Decimal, decimal.Zero),
				new PXDataFieldRestrict(nameof(INLocationStatusByCostCenter.QtyActual), PXDbType.Decimal, decimal.Zero),
			};
			restrictions.AddRange(
				locationstatusbycostcenter.Cache.GetAllDBDecimalFields()
				.Select(f => new PXDataFieldRestrict(f, PXDbType.Decimal, decimal.Zero)));
			PXDatabase.Delete<INLocationStatusByCostCenter>(restrictions.ToArray());
		}

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values) => true;

		public virtual bool RowImporting(string viewName, object row) => row == null;

		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode) { }
		#endregion
	}
}
