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

using System.Collections.Generic;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Linq;
using PX.Objects.AM.Attributes;
using PX.Common;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.SO;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.IN.InventoryRelease;

namespace PX.Objects.AM.GraphExtensions
{
    public class INReleaseProcessAMExtension : PXGraphExtension<INReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

		public class AMProdMatlSplitPlanSyncOnly : IN.GraphExtensions.ItemPlanSyncOnly<INReleaseProcess, AMProdMatlSplit>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
		}

        public PXSelect<AMProdMatlSplit> ProdMatlSplits;
		public PXSelect<AMProdItem> ProdItems;

        //Purpose of extension is to copy InventoryItemExt values over to the new INItemSiteExt record as defaults similar to Acumatica's DefaultItemSiteByitem call
        public virtual void INItemSite_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            INItemSite inItemSite = (INItemSite)e.Row;
            if (inItemSite == null || !inItemSite.InventoryID.HasValue || !inItemSite.SiteID.HasValue)
            {
                return;
            }

            InventoryItem inventoryItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, inItemSite.InventoryID);

            if (inventoryItem != null)
            {
                AM.InventoryHelper.DefaultItemSiteManufacturing(Base, inventoryItem, inItemSite);
                Base.initemsite.Cache.IsDirty = true;
            }
        }

        public delegate void ProcessLinkedAllocationDelegate(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list, string poReceiptType, string poReceiptNbr);

        [PXOverride]
        public virtual void ProcessLinkedAllocation(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list, string poReceiptType, string poReceiptNbr, ProcessLinkedAllocationDelegate del)
        {
            var remainingList = ProcessMaterialAllocation(list, poReceiptType, poReceiptNbr);
            // We will process the list for those entries related to production material. All others will be returned as passed to the base call.
            //  Note: this includes production move (receipt) linked to sales order. The base call works for us to update the allocation records.
            del?.Invoke(remainingList, poReceiptType, poReceiptNbr);

            UpdateSalesOrderLines(remainingList);
        }

        /// <summary>
        /// After base ProcessLinkedAllocation we need to update the sales orders further for production
        /// </summary>
        /// <param name="list"></param>
        protected virtual void UpdateSalesOrderLines(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list)
        {
            UpdateAllocatedSalesLines(list);
            UpdateParentSalesLine(list);
        }

        private void UpdateParentSalesLine(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list)
        {
            if (list == null || list.Count < 1 || (Base.Caches<SOLineSplit>()?.Inserted?.Count() ?? 0) == 0)
            {
                return;
            }
            // Updated should be the parent line where we need to syne the INItemPlan changes as the base call from Acumatica only updates the plan but not the solinesplit
            foreach (SOLineSplit soLineUpdated in Base.Caches<SOLineSplit>().Updated)
            {
                if (soLineUpdated?.PlanID == null)
                {
                    continue;
                }
                var inItemPlan = LocateItemPlan(soLineUpdated, list);
                if (inItemPlan?.PlanID == null || inItemPlan.PlanType != INPlanConstants.PlanM8 || inItemPlan.PlanQty > 0)
                {
                    continue;
                }

				var demandPlanID = inItemPlan.PlanID;
                        Base.Caches<INItemPlan>().Delete(inItemPlan);

				var prodItem = ProdItems.Cache.LocateElse((AMProdItem)SelectFrom<AMProdItem>.Where<AMProdItem.demandPlanID.IsEqual<@P.AsLong>>.View.Select(Base, demandPlanID));
				if(prodItem != null)
				{
					prodItem.DemandPlanID = null;
					ProdItems.Update(prodItem);
				}

				soLineUpdated.Completed = true;
				soLineUpdated.PlanID = null;
                Base.Caches<SOLineSplit>().Update(soLineUpdated);
            }
        }

        private void UpdateAllocatedSalesLines(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list)
        {
            if (list == null || list.Count < 1 || (Base.Caches<SOLineSplit>()?.Inserted?.Count() ?? 0) == 0)
            {
                return;
            }

            // Inserted should be the child allocation records
            foreach (SOLineSplit soLineInserted in Base.Caches<SOLineSplit>().Inserted)
            {
                if (soLineInserted?.PlanID == null || soLineInserted.RefNoteID != null)
                {
                    continue;
                }

                var parentSplit = (SOLineSplit)Base.Caches<SOLineSplit>().Locate(
                    new SOLineSplit
                    {
                        OrderType = soLineInserted.OrderType,
                        OrderNbr = soLineInserted.OrderNbr,
                        LineNbr = soLineInserted.LineNbr,
                        SplitLineNbr = soLineInserted.ParentSplitLineNbr
                    });

                if (parentSplit?.PlanID == null)
                {
                    continue;
                }

                var parentItemPlan = LocateItemPlan(parentSplit, list);
                if (parentItemPlan?.PlanID == null || parentItemPlan.PlanType != INPlanConstants.PlanM8)
                {
                    continue;
                }

                var inItemPlan = LocateItemPlan(soLineInserted, list);
                if (inItemPlan?.PlanID == null)
                {
                    continue;
                }

				if (!inItemPlan.RefEntityType.Equals(typeof(AMBatch).FullName))
				{
					continue;
				}

                soLineInserted.AMProdCreate = false;
                var soLineInsertedExt = soLineInserted.GetExtension<SOLineSplitExt>();
                if(soLineInsertedExt != null)
                {
                    soLineInsertedExt.AMOrderType = null;
                    soLineInsertedExt.AMProdOrdID = null;
                    soLineInsertedExt.AMProdQtyComplete = 0m;
                    soLineInsertedExt.AMProdBaseQtyComplete = 0m;
                    soLineInsertedExt.AMProdStatusID = ProductionOrderStatus.Completed; // using for status for other logic
                }

                // Setting AMBatch references
                soLineInserted.RefNoteID = inItemPlan.RefNoteID;

                //if the prod order was over completed, reduce the split qty to what was required
                if (soLineInserted.Qty > soLineInserted.UnreceivedQty)
                {
                    soLineInserted.Qty = soLineInserted.UnreceivedQty;
                    inItemPlan.PlanQty = soLineInserted.UnreceivedQty;
                }

                if (Base.Caches<SOLineSplit>().Update(soLineInserted) == null)
                {
                    continue;
                }

                // Need the plan record to show the SOOrder NoteID to correctly create the shipment and open the order from allocation details
                inItemPlan.RefNoteID = parentItemPlan.RefNoteID;
                inItemPlan.RefEntityType = parentItemPlan.RefEntityType;
                // PO allocation process doesn't list the customer but the parent line does so why not list it?
                inItemPlan.BAccountID = parentItemPlan.BAccountID;
                Base.Caches<INItemPlan>().Update(inItemPlan);
            }
        }

        protected INItemPlan LocateItemPlan(SOLineSplit soLineSplit, List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list)
        {
            if(soLineSplit?.PlanID == null)
            {
                return null;
            }

            var locatedItemPlan = (INItemPlan)Base.Caches<INItemPlan>().Locate(new INItemPlan
            {
                PlanID = soLineSplit.PlanID
            });

            if(locatedItemPlan?.PlanID != null)
            {
                return locatedItemPlan;
            }

            if (list != null)
            {
                foreach (PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand> result in list)
                {
                    var itemPlan = (INItemPlan)result;
                    var demandItemPlan = (INItemPlanDemand)result;

                    if (itemPlan?.PlanID == soLineSplit.PlanID)
                    {
                        return itemPlan;
                    }

                    if (demandItemPlan?.PlanID == soLineSplit.PlanID)
                    {
                        return demandItemPlan;
                    }
                } 
            }

            return PXSelect<INItemPlan,
                    Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                .Select(Base, soLineSplit.PlanID);
        }

        protected virtual List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> ProcessMaterialAllocation(List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>> list, string poReceiptType, string poReceiptNbr)
        {
            var processList = new List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>>();
            if (list == null)
            {
                return null;
            }

            var unprocessedList = new List<PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand>>();
            foreach (PXResult<INItemPlan, INTranSplit, INTran, INPlanType, INItemPlanDemand> result in list)
            {
                var demandPlanItem = (INItemPlanDemand) result;
                var origPlanItem = (INItemPlan)result;
#if DEBUG
                AMDebug.TraceWriteMethodName($"PlanID = {((INItemPlan)result)?.PlanID}; PlanType = {((INItemPlan)result)?.PlanType}; INTranSplit Keys = {((INTranSplit)result)?.DocType}:{((INTranSplit)result)?.RefNbr}:{((INTranSplit)result)?.LineNbr}:{((INTranSplit)result)?.SplitLineNbr}; Demand PlanID = {demandPlanItem?.PlanID}; Demand PlanType = {demandPlanItem?.PlanType}");
#endif
                if (IsMaterialPlanType(demandPlanItem?.PlanType) || IsMaterialPlanType(origPlanItem?.PlanType))
                {
                    processList.Add(result);

                    //Process as linked to production material
                    continue;
                }

                // return to process in base acumatica, unless the siteid changed on the production order
				if(demandPlanItem?.PlanType != INPlanConstants.PlanM8 ||
					(demandPlanItem?.PlanType == INPlanConstants.PlanM8 && demandPlanItem?.SiteID == origPlanItem?.SiteID))
                unprocessedList.Add(result);
            }

            ProdDetail.ProcessPOReceipt(Base, processList, poReceiptType, poReceiptNbr);

            return unprocessedList;
        }

        private bool IsMaterialPlanType(string planType)
        {
            return !string.IsNullOrWhiteSpace(planType)
                   && (planType == INPlanConstants.PlanM5 ||
                       planType == INPlanConstants.PlanM6 ||
                       planType == INPlanConstants.PlanM9 ||
                       planType == INPlanConstants.PlanMA);
        }

        [PXOverride]
        public virtual void WriteGLCosts(JournalEntry je, INTranCost trancost, INTran intran, InventoryItem item, INSite site, INPostClass postclass, ReasonCode reasoncode, INLocation location,
           Action<JournalEntry, INTranCost, INTran, InventoryItem, INSite, INPostClass, ReasonCode, INLocation> method)
        {
            method?.Invoke(je, trancost, intran, item, site, postclass, reasoncode, location);

            INTranExt inTranExt = intran?.GetExtension<INTranExt>();
            if (!ProjectHelper.IsProjectFeatureEnabled() || inTranExt?.AMProdOrdID == null || intran.DocType != INDocType.Issue)
            {
                return;
            }

            AMProdItem amproditem = PXSelect<AMProdItem, Where<AMProdItem.orderType, Equal<Required<AMProdItem.prodOrdID>>,
                And<AMProdItem.prodOrdID, Equal<Required<AMProdItem.prodOrdID>>>>>.Select(Base, inTranExt.AMOrderType, inTranExt.AMProdOrdID);

            if(amproditem.ProjectID != null && amproditem.UpdateProject == true)
            {
                GLTran matlInvtran = je.GLTranModuleBatNbr.Select().FirstTableItems.FirstOrDefault(x =>
                    x.AccountID == intran.InvtAcctID
                    && x.InventoryID == intran.InventoryID && x.ProjectID != ProjectDefaultAttribute.NonProject());
                if (matlInvtran == null)
                {
                    return;
                }

                matlInvtran.ProjectID = ProjectDefaultAttribute.NonProject();
                matlInvtran.TaskID = null;
                matlInvtran.CostCodeID = null;
                je.GLTranModuleBatNbr.Update(matlInvtran);
            }
        }
    }
}
