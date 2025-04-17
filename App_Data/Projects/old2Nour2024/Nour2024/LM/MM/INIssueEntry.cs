using System;
using PX.Data;
using PX.Objects.IN;
using IN = PX.Objects.IN;
using System.Collections;
using PX.Objects.CM;
using PX.Objects.CS;
using CS = PX.Objects.CS;
using PX.Objects.PM;
using PM = PX.Objects.PM;
using PX.Objects.GL;
using GL = PX.Objects.GL;
using PX.Objects.AR;
using System.Collections.Generic;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace MyMaintaince
{

    public class INIssueEntry : PXGraph<INIssueEntry, INRegister>
    {
        public PXSelect<INRegister, Where<INRegister.docType, Equal<INDocType.issue>>> issue;
        public PXSelect<INRegister, Where<INRegister.docType, Equal<Current<INRegister.docType>>, And<INRegister.refNbr, Equal<Current<INRegister.refNbr>>>>> CurrentDocument;
        [PXImport(typeof(INRegister))]
        public PXSelect<INTran, Where<INTran.docType, Equal<Current<INRegister.docType>>, And<INTran.refNbr, Equal<Current<INRegister.refNbr>>>>> transactions;

        [PXCopyPasteHiddenView()]
        public PXSelect<INTranSplit, Where<INTranSplit.tranType, Equal<Current<INTran.tranType>>, And<INTranSplit.refNbr, Equal<Current<INTran.refNbr>>, And<INTranSplit.lineNbr, Equal<Current<INTran.lineNbr>>>>>> splits;
        public PXSetup<INSetup> insetup;
        //public LSINTran lsselect;




        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<INTran.inventoryID>>>>))]
        [INUnit(typeof(INTran.inventoryID))]
        public virtual void INTran_UOM_CacheAttached(PXCache sender)
        {
        }
        //[PXString(2)]
        //[PXFormula(typeof(Parent<INRegister.origModule>))]
        //public virtual void INTran_OrigModule_CacheAttached(PXCache sender)
        //{
        //}
        [PXString(2)]
        [PXFormula(typeof(Parent<INRegister.origModule>))]
        public virtual void INTranSplit_OrigModule_CacheAttached(PXCache sender)
        {
        }

        //[IN.LocationAvail(typeof(INTran.inventoryID),
        //    typeof(INTran.subItemID),
        //    typeof(INTran.siteID),
        //    typeof(Where<INTran.tranType,
        //                    Equal<INTranType.invoice>,
        //                Or<INTran.tranType,
        //                    Equal<INTranType.debitMemo>,
        //                Or<INTran.origModule,
        //                    NotEqual<GL.BatchModule.modulePO>,
        //                And<INTran.tranType,
        //                    Equal<INTranType.issue>>>>>),
        //    typeof(Where<INTran.tranType,
        //                    Equal<INTranType.receipt>,
        //                 Or<INTran.tranType,
        //                    Equal<INTranType.return_>,
        //                 Or<INTran.tranType,
        //                    Equal<INTranType.creditMemo>,
        //                 Or<INTran.origModule,
        //                    Equal<GL.BatchModule.modulePO>,
        //                 And<INTran.tranType,
        //                    Equal<INTranType.issue>>>>>>),
        //    typeof(Where<INTran.tranType,
        //                    Equal<INTranType.transfer>,
        //                 And<INTran.invtMult,
        //                    Equal<short1>,
        //                 Or<INTran.tranType,
        //                    Equal<INTranType.transfer>,
        //                 And<INTran.invtMult,
        //                    Equal<shortMinus1>>>>>))]
        //public virtual void INTran_LocationID_CacheAttached(PXCache sender)
        //{
        //}
        


        //[IN.LocationAvail(typeof(INTranSplit.inventoryID),
        //    typeof(INTranSplit.subItemID),
        //       typeof(costCenterID),

        //    typeof(INTranSplit.siteID),
        //    typeof(Where<INTranSplit.tranType,
        //                    Equal<INTranType.invoice>,
        //                 Or<INTranSplit.tranType,
        //                    Equal<INTranType.debitMemo>,
        //                 Or<INTranSplit.origModule,
        //                    NotEqual<GL.BatchModule.modulePO>,
        //                 And<INTranSplit.tranType,
        //                    Equal<INTranType.issue>>>>>),
        //    typeof(Where<INTranSplit.tranType,
        //                    Equal<INTranType.receipt>,
        //                 Or<INTranSplit.tranType,
        //                    Equal<INTranType.return_>,
        //                 Or<INTranSplit.tranType,
        //                    Equal<INTranType.creditMemo>,
        //                 Or<INTranSplit.origModule,
        //                    Equal<GL.BatchModule.modulePO>,
        //                 And<INTranSplit.tranType,
        //                    Equal<INTranType.issue>>>>>>),
        //    typeof(Where<INTranSplit.tranType,
        //                    Equal<INTranType.transfer>,
        //                 And<INTranSplit.invtMult,
        //                    Equal<short1>,
        //                 Or<INTranSplit.tranType,
        //                    Equal<INTranType.transfer>,
        //                 And<INTranSplit.invtMult,
        //                    Equal<shortMinus1>>>>>))]
        //[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        //public virtual void INTranSplit_LocationID_CacheAttached(PXCache sender)
        //{
        //}

       

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[ItemPlanBaseExt(typeof(INRegister.noteID), typeof(INRegister.hold), typeof(INRegister.transferType))]
        //protected virtual void INTranSplit_PlanID_CacheAttached(PXCache sender)
        //{
        //}

        #region CacheAttached


        [PXDefault("")]
        [PXDBString(50, IsFixed = false, IsUnicode = true, IsKey = false)]
        [PXUIField(DisplayName = "Job Order ID")]
        //using MyMaintaince;
        [PXSelector(typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.Status, Equal<JobOrderStatus.started>>>)
                               , new Type[] { typeof(JobOrder.jobOrdrID), typeof(JobOrder.customer) })]
        protected void INRegister_UsrOrderID_CacheAttached(PXCache sender)
        {
        }
        protected virtual void INRegister_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            INRegister InR = (INRegister)this.issue.Current;

            
            if (InR != null)
            {
                INRegisterExt inreg = PXCache<INRegister>.GetExtension<INRegisterExt>(InR);

                if (!String.IsNullOrEmpty(inreg.UsrOrderID + ""))
            {
                PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);
                Jo.Cache.ClearQueryCache();
                Customer job = Jo.Select(inreg.UsrOrderID);
                sender.SetValueExt<INRegisterExt.customer>(e.Row, job.AcctName);

                PXSelectBase<Items> Itm = new PXSelectJoin<Items, InnerJoin<JobOrder, On<JobOrder.itemsID, Equal<Items.itemsID>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);
                Itm.Cache.ClearQueryCache();
                Items it = Itm.Select(inreg.UsrOrderID);
                sender.SetValueExt<INRegisterExt.code>(e.Row, it.Code);
               

            }
        }
        }
        protected virtual void INRegister_UsrOrderID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {

                INRegister header = (INRegister)this.issue.Current;

                INRegisterExt inreg = PXCache<INRegister>.GetExtension<INRegisterExt>(header);

                if (!String.IsNullOrEmpty(inreg.UsrOrderID + ""))
                {
                    PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);
                    Jo.Cache.ClearQueryCache();
                    Customer job = Jo.Select(inreg.UsrOrderID);
                    sender.SetValueExt<INRegisterExt.customer>(e.Row, job.AcctName);

                    PXSelectBase<Items> Itm = new PXSelectJoin<Items, InnerJoin<JobOrder, On<JobOrder.itemsID, Equal<Items.itemsID>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);
                    Itm.Cache.ClearQueryCache();
                    Items it = Itm.Select(inreg.UsrOrderID);
                    sender.SetValueExt<INRegisterExt.code>(e.Row, it.Code);

                }
            }

        }
        [PXDBString(50, IsKey = false, IsFixed = false, IsUnicode = true)]
        [PXUIField(DisplayName = "Job Order ID")]
        [PXDBDefault(typeof(INRegisterExt.usrOrderID))]
        protected void INTran_UsrOrdID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [PXDBInt()]
        [PXUIField(DisplayName = "Inventory ID",Required=true)]
        [PXSelector(
                    //typeof(Search2<JobOrderStockItems.inventoryCode, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<JobOrderStockItems.inventoryCode>>>, Where<JobOrderStockItems.jobOrdrID, Equal<Current<INTranExt.usrOrdID>>>>)
                    //typeof(Search2<InventoryItem.inventoryID, InnerJoin<JobOrderStockItems, On<JobOrderStockItems.inventoryCode, Equal<InventoryItem.inventoryID>>>, Where<JobOrderStockItems.jobOrdrID, Equal<Current<INTranExt.usrOrdID>>>>)
                    typeof(Search2<InventoryItem.inventoryID, InnerJoin<Balance, On<Balance.inventoryID, Equal<InventoryItem.inventoryID>>>, Where<Balance.joborderID, Equal<Current<INTranExt.usrOrdID>>>>)
                    ,new Type[]{
                        typeof(InventoryItem.inventoryCD),
                        typeof(InventoryItem.descr),
                        typeof(Balance.qty)
                    }
                    , SubstituteKey = typeof(InventoryItem.inventoryCD)
                    , DescriptionField = typeof(InventoryItem.descr))]
        public virtual void INTran_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion 


        public PXAction<INRegister> viewBatch;
        [PXUIField(DisplayName = "Review Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable ViewBatch(PXAdapter adapter)
        {
            if (issue.Current != null && !String.IsNullOrEmpty(issue.Current.BatchNbr))
            {
                GL.JournalEntry graph = PXGraph.CreateInstance<GL.JournalEntry>();
                graph.BatchModule.Current = graph.BatchModule.Search<GL.Batch.batchNbr>(issue.Current.BatchNbr, "IN");
                throw new PXRedirectRequiredException(graph, "Current batch record");
            }
            return adapter.Get();
        }

        
        public PXAction<INRegister> release;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
           
            INRegister header = (INRegister)this.issue.Current;
          
            INRegisterExt headerExt = PXCache<INRegister>.GetExtension<INRegisterExt>(header);
            PXCache cache = issue.Cache;

            #region original Code




            PXDatabase.Delete<INTranSplit>(
                new PXDataFieldRestrict<INTranSplit.refNbr>(transactions.Current.RefNbr),
                new PXDataFieldRestrict<INTranSplit.docType>(transactions.Current.DocType)
                );

            int counter = 1;
            foreach (INTran tran in transactions.Select())
            {
                PXDatabase.Update<INTran>(
                    new PXDataFieldAssign("invtMult", INTranType.InvtMult(tran.TranType)),
                    
                    new PXDataFieldRestrict<INTran.refNbr>(tran.RefNbr),
                    new PXDataFieldRestrict<INTran.lineNbr>(tran.LineNbr),
                    new PXDataFieldRestrict<INTran.docType>(tran.DocType)
                    );



                PXDatabase.Insert<INTranSplit>(
                new PXDataFieldAssign("DocType", tran.DocType)
               , new PXDataFieldAssign("TranType", tran.TranType)
               , new PXDataFieldAssign("RefNbr", tran.RefNbr)
               , new PXDataFieldAssign("LineNbr", tran.LineNbr)
               , new PXDataFieldAssign("InvtMult", INTranType.InvtMult(tran.TranType))
               , new PXDataFieldAssign("SplitLineNbr", counter)
               , new PXDataFieldAssign("TranDate", tran.TranDate)
               , new PXDataFieldAssign("InventoryID", tran.InventoryID)
               , new PXDataFieldAssign("SubItemID", tran.SubItemID)
               , new PXDataFieldAssign("SiteID", tran.SiteID)
               , new PXDataFieldAssign("LocationID", tran.LocationID)
               , new PXDataFieldAssign("LotSerialNbr", tran.LotSerialNbr)
               , new PXDataFieldAssign("ExpireDate", tran.ExpireDate)
               , new PXDataFieldAssign("Qty", tran.Qty)
               , new PXDataFieldAssign("Released", tran.Released)
               , new PXDataFieldAssign("UOM", tran.UOM)
               , new PXDataFieldAssign("POLineType", tran.POLineType)
               , new PXDataFieldAssign("SOLineType", tran.SOLineType)
               , new PXDataFieldAssign("ToSiteID", tran.ToSiteID)
               , new PXDataFieldAssign("ToLocationID", tran.ToLocationID)


               , new PXDataFieldAssign("TransferType", CurrentDocument.Current.TransferType)
               , new PXDataFieldAssign("CostSubItemID", tran.SubItemID)
               , new PXDataFieldAssign("CostSiteID", tran.SiteID)
               , new PXDataFieldAssign("TotalQty", CurrentDocument.Current.TotalQty)
               , new PXDataFieldAssign("TotalCost", CurrentDocument.Current.TotalCost)
               //, new PXDataFieldAssign("AdditionalCost",CurrentDocument.Current.)
               //, new PXDataFieldAssign("ShipmentNbr", tran.shipm),
               //, new PXDataFieldAssign("ShipmentLineNbr", tran.ship),
               //, new PXDataFieldAssign("ShipmentLineSplitNbr", tran.ship),
               , new PXDataFieldAssign("CreatedByID", Accessinfo.UserID)
               , new PXDataFieldAssign("CreatedByScreenID", "MM303000")
               , new PXDataFieldAssign("CreatedDateTime", DateTime.Now)

               , new PXDataFieldAssign("LastModifiedByID", Accessinfo.UserID)

               , new PXDataFieldAssign("LastModifiedByScreenID", "MM303000")

               , new PXDataFieldAssign("LastModifiedDateTime", DateTime.Now)

               , new PXDataFieldAssign("BaseQty", tran.BaseQty)

               , new PXDataFieldAssign("MaxTransferBaseQty", tran.MaxTransferBaseQty)

               , new PXDataFieldAssign("OrigPlanType", tran.OrigPlanType)

               //, new PXDataFieldAssign("IsFixedInTransit", tran.is),

               //, new PXDataFieldAssign("PlanID", CurrentDocument.Current.p)

               , new PXDataFieldAssign("ReleasedDateTime", DateTime.Now)

               , new PXDataFieldAssign("IsIntercompany", tran.IsIntercompany)
         ,
                new PXDataFieldAssign("OrigModule", "IN")
                                      //new PXDataFieldRestrict<ARRegister.refNbr>(doc.RefNbr), new PXDataFieldRestrict<ARRegister.docType>(doc.DocType)
                                      );

                counter += 2;
            }




            List<PX.Objects.IN.INRegister> list = new List<PX.Objects.IN.INRegister>();
            foreach (PX.Objects.IN.INRegister indoc in adapter.Get<PX.Objects.IN.INRegister>())
            {
                if (indoc.Hold == false && indoc.Released == false)
                {
                    cache.Update(indoc);
                    list.Add(indoc);
                }
            }
            if (list.Count == 0)
            {
                throw new PXException(PX.Objects.IN.Messages.Document_Status_Invalid);
            }
            Save.Press();
            PXLongOperation.StartOperation(this, delegate() { INDocumentRelease.ReleaseDoc(list, false); });
            //Save.Press();
            //PXSelectBase<INRegister> reg = new PXSelectReadonly<INRegister, Where<INRegister.refNbr, Equal<Required<INRegister.refNbr>>>>(this);
            //reg.Cache.ClearQueryCache();
            //INRegister x = reg.Select(header.RefNbr);
            //if (x.Released == true)
            //{
 
            //}
            #endregion
            
                //#region poo :  update the Qty on the balances
           
                //foreach (INTran inTran in this.transactions.Select())
                //{
                //    //if issue , increase the qty to the balance
                //    if (x.Status == "R")
                //    {
                //        if (inTran.TranType == INTranType.Issue)
                //        {

                //            PXSelectBase<Balance> resultBalance = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
                //            resultBalance.Cache.ClearQueryCache();
                //            //Balance balanceRow = resultBalance.Select(header.UsrOrderID, inTran.InventoryID);
                //            Balance balanceRow = resultBalance.Select(headerExt.UsrOrderID, inTran.InventoryID);
                //            float? balanceQty = balanceRow.Qty;

                //            PXSelectBase<JobOrderStockItems> JobOrderStockItemsResult = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
                //            JobOrderStockItemsResult.Cache.ClearQueryCache();
                //            //JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(header.UsrOrderID, inTran.InventoryID);
                //            JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(headerExt.UsrOrderID, inTran.InventoryID);
                //            float? jobOrderStockItemsQty = jobOrderStockItemsResultRow.Quantity;

                //            decimal? maxItemQtySupposedToBeIssued = (decimal?)(jobOrderStockItemsQty - balanceQty);
                //            decimal? newBalanceQty = (decimal?)balanceQty - inTran.Qty;
                //            //this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", newBalanceQty), new PXDataFieldRestrict("joborderID", header.UsrOrderID), new PXDataFieldRestrict("inventoryID", inTran.InventoryID));
                //            this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", newBalanceQty), new PXDataFieldRestrict("joborderID", headerExt.UsrOrderID), new PXDataFieldRestrict("inventoryID", inTran.InventoryID));
                //        }//if return ,decrease the qty from the balance
                //        else if (inTran.TranType == INTranType.Return)
                //        {
                //            //PXSelectBase<Balance> resultBalance = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
                //            //resultBalance.Cache.ClearQueryCache();
                //            ////Balance balanceRow = resultBalance.Select(header.UsrOrderID, inTran.InventoryID);
                //            //Balance balanceRow = resultBalance.Select(headerExt.UsrOrderID, inTran.InventoryID);
                //            //float? balanceQty = balanceRow.Qty;

                //            //PXSelectBase<JobOrderStockItems> JobOrderStockItemsResult = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
                //            //JobOrderStockItemsResult.Cache.ClearQueryCache();
                //            ////JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(header.UsrOrderID, inTran.InventoryID);
                //            //JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(headerExt.UsrOrderID, inTran.InventoryID);
                //            //float? jobOrderStockItemsQty = jobOrderStockItemsResultRow.Quantity;

                //            //decimal? maxItemQtyCanBeReturned = (decimal?)(jobOrderStockItemsQty - balanceQty);
                //            //decimal? newBalanceQty = (decimal?)balanceQty + inTran.Qty;
                //            ////this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", newBalanceQty), new PXDataFieldRestrict("JoborderID", header.UsrOrderID), new PXDataFieldRestrict("InventoryID", inTran.InventoryID));
                //            //this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", newBalanceQty), new PXDataFieldRestrict("JoborderID", headerExt.UsrOrderID), new PXDataFieldRestrict("InventoryID", inTran.InventoryID));
                //        }
                //    }


                //}
                //#endregion
                return list;
        }

        #region MyButtons (MMK)
        public PXAction<INRegister> Print;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Issue", MapEnableRights = PXCacheRights.Select)]
        protected void print()
        {

            INRegister reg = this.CurrentDocument.Current;
            if (reg != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["RefNbr"] = reg.RefNbr;
                parameters["DocType"] = reg.DocType;
                throw new PXReportRequiredException(parameters, "NO700016", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }
        public PXAction<INRegister> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        public PXAction<INRegister> iNEdit;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.INEditDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable INEdit(PXAdapter adapter)
        {
            if (issue.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["DocType"] = issue.Current.DocType;
                parameters["RefNbr"] = issue.Current.RefNbr;
                parameters["PeriodTo"] = null;
                parameters["PeriodFrom"] = null;
                throw new PXReportRequiredException(parameters, "IN611000", PX.Objects.IN.Messages.INEditDetails);
            }
            return adapter.Get();
        }

        public PXAction<INRegister> inventorySummary;
        [PXUIField(DisplayName = "Inventory Summary", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable InventorySummary(PXAdapter adapter)
        {
            PXCache tCache = transactions.Cache;
            INTran line = transactions.Current;
            if (line == null) return adapter.Get();

            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<INTran.inventoryID>(tCache, line);
            if (item != null && item.StkItem == true)
            {
                INSubItem sbitem = (INSubItem)PXSelectorAttribute.Select<INTran.subItemID>(tCache, line);
                InventorySummaryEnq.Redirect(item.InventoryID,
                                             ((sbitem != null) ? sbitem.SubItemCD : null),
                                             line.SiteID,
                                             line.LocationID);
            }
            return adapter.Get();
        }

        public PXAction<INRegister> iNRegisterDetails;
        [PXUIField(DisplayName = PX.Objects.IN.Messages.INRegisterDetails, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual IEnumerable INRegisterDetails(PXAdapter adapter)
        {
            if (issue.Current != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["PeriodID"] = (string)issue.GetValueExt<INRegister.finPeriodID>(issue.Current);
                parameters["DocType"] = issue.Current.DocType;
                parameters["RefNbr"] = issue.Current.RefNbr;
                throw new PXReportRequiredException(parameters, "IN614000", PX.Objects.IN.Messages.INRegisterDetails);
            }
            return adapter.Get();
        }

        #region SiteStatus Lookup
        public PXFilter<INSiteStatusFilter> sitestatusfilter;
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public INSiteStatusLookup<INSiteStatusSelected, INSiteStatusFilter> sitestatus;

        //public PXAction<INRegister> addInvBySite;
        //[PXUIField(DisplayName = "Add Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        //[PXLookupButton]
        //public virtual IEnumerable AddInvBySite(PXAdapter adapter)
        //{
        //    sitestatusfilter.Cache.Clear();
        //    if (sitestatus.AskExt() == WebDialogResult.OK)
        //    {
        //        return AddInvSelBySite(adapter);
        //    }
        //    sitestatusfilter.Cache.Clear();
        //    sitestatus.Cache.Clear();
        //    return adapter.Get();
        //}

        public PXAction<INRegister> addInvSelBySite;
        [PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable AddInvSelBySite(PXAdapter adapter)
        {
            foreach (INSiteStatusSelected line in sitestatus.Cache.Cached)
            {
                if (line.Selected == true && line.QtySelected > 0)
                {
                    INTran newline = PXCache<INTran>.CreateCopy(this.transactions.Insert(new INTran()));
                    newline.SiteID = line.SiteID;
                    newline.InventoryID = line.InventoryID;
                    newline.SubItemID = line.SubItemID;
                    newline.UOM = line.BaseUnit;
                    newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
                    newline.LocationID = line.LocationID;
                    newline = PXCache<INTran>.CreateCopy(transactions.Update(newline));
                    newline.Qty = line.QtySelected;
                    transactions.Update(newline);
                }
            }
            sitestatus.Cache.Clear();
            return adapter.Get();
        }

        public PXAction<INRegister> addInvFromJobOrder;
        [PXUIField(DisplayName = "Add Job Order Stock Item ", Visible = false)]
        [PXButton]
        public virtual IEnumerable AddInvFromJobOrder(PXAdapter adapter)
        {
           Persist();
           PXLongOperation.StartOperation((PXGraph)this, delegate
           {
               INRegister row = this.issue.Current as INRegister;
               INRegisterExt rowExt = PXCache<INRegister>.GetExtension<INRegisterExt>(row);
               JournalEntry Jo = PXGraph.CreateInstance<JournalEntry>();
               JournalEntry_Extension JOext = Jo.GetExtension<JournalEntry_Extension>();
               PXLongOperation.WaitCompletion(JOext);

               
               INIssueEntry invoice = PXGraph<INIssueEntry>.CreateInstance<INIssueEntry>();
               ArrayList tran = new ArrayList();
               INTran Tran = new INTran();
               Tran.InventoryID = 20619;
               Tran.Qty = 1;
               Tran.TranType = "III";
               invoice.transactions.Insert(Tran);
               invoice.transactions.Current = Tran;
               invoice.Actions.PressSave();
               invoice.Persist();
           });

           //    Dictionary<int?, float?> itemDictionary = new Dictionary<int?, float?>();

           //    #region Query All Stock Quantites and Prices
           //    PXSelectBase<Balance> Stk = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>>>(this);
           //    Stk.Cache.ClearQueryCache();
           //    PXResultset<Balance> resultStk = Stk.Select(rowExt.UsrOrderID);
           //    foreach (Balance StkItem in resultStk)
           //    {
           //        if (StkItem.Qty > 0)
           //        {
           //            itemDictionary.Add(StkItem.InventoryID, StkItem.Qty);
           //        }
           //    }
           //    #endregion
           //    //create Issues lines for the stock 
           //    foreach (int? itm in itemDictionary.Keys)
           //    {
           //        InventoryItem Inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, itm);
           //        INTran Tran = new INTran();
           //        Tran.InventoryID = itm;
           //        Tran.Qty = Convert.ToDecimal(itemDictionary[itm]);
           //        Tran.TranType="III";
           //        tran.Add(Tran);
           //    }
                            
           //    foreach (INTran record in tran)
           //    {
           //        if (record.InventoryID.HasValue)
           //        {
                      
           //            INTran newline = new INTran();
                       
           //            newline.InventoryID = record.InventoryID;
                      
           //            newline.Qty = record.Qty;
                       
           //            newline.TranType = record.TranType;
                      
           //            transactions.Insert(newline);
           //            invoice.Persist();
           //            invoice.Actions.PressSave();
                                            
           //        }
           //    }

              
           //});


           return adapter.Get();
       }
          

       
       

        #region poo : Job Customizations >> Check if this Inventory Belongs To this Job Order 
        /**
        protected virtual void INTran_inventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INRegister header = (INRegister)this.issue.Current;
            if (header.UsrRtrn == true)
            {
                PXSelectBase<INTran> INTran = new PXSelect<INTran, Where<INTran.usrOrdID, Equal<Required<INTran.usrOrdID>>, And<INTran.inventoryID, Equal<Required<INTran.inventoryID>>>>>(this);
                PXResultset<INTran> result = INTran.Select(this.issue.Current.UsrOrderID, e.NewValue);
                if (result == null)
                {
                    throw new PXSetPropertyException(" this inventory ID not issued in this job order.");
                }
            }

        }
        **/
        #endregion
        
        protected virtual void INSiteStatusFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            INSiteStatusFilter row = (INSiteStatusFilter)e.Row;
            if (row != null && issue.Current != null)
                row.SiteID = issue.Current.SiteID;
        }
        #endregion

        public INIssueEntry()
        {
            INSetup record = insetup.Current;

            PXStringListAttribute.SetList<INTran.tranType>(transactions.Cache, null, new INTranType.IssueListAttribute().AllowedValues, new INTranType.IssueListAttribute().AllowedLabels);
            //PXDimensionSelectorAttribute.SetValidCombo<INTran.subItemID>(transactions.Cache, true);
            //PXDimensionSelectorAttribute.SetValidCombo<INTranSplit.subItemID>(splits.Cache, true);

            //this.FieldDefaulting.AddHandler<IN.Overrides.INDocumentRelease.SiteStatus.negAvailQty>((sender, e) =>
            //{
            //    if (!e.Cancel)
            //        e.NewValue = true;
            //    e.Cancel = true;
            //});
        }

        protected virtual void INRegister_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = INDocType.Issue;
        }

        protected virtual void INRegister_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (((INRegister)e.Row).DocType == INDocType.Undefined)
            {
                e.Cancel = true;
            }
        }


        protected virtual void INTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            //INRegister row = this.issue.Current as INRegister;
            //INRegisterExt rowExt = PXCache<INRegister>.GetExtension<INRegisterExt>(row);

            //INTran tranRow = e.Row as INTran;
            //INTranExt tranRowExt = PXCache<INTran>.GetExtension<INTranExt>(tranRow);
            //tranRowExt.UsrOrdID = rowExt.UsrOrderID;
            //sender.SetValue<INTranExt.usrOrdID>(row, rowExt.UsrOrderID);
            //if (row.RefNbr != null)
            //{
            //    if (rowExt.UsrRtrn == true)
            //    {
            //        string x = "RET";
            //        tranRow.TranType = x;
            //        sender.SetValue<INTran.tranType>(row, x);
            //    }

            //}
            
        }
        

        protected virtual void INRegister_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (insetup.Current.RequireControlTotal == false)
            {
                if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalAmount) == false)
                {
                    sender.SetValue<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).TotalAmount);
                }
                else
                {
                    sender.SetValue<INRegister.controlAmount>(e.Row, 0m);
                }

                if (PXCurrencyAttribute.IsNullOrEmpty(((INRegister)e.Row).TotalQty) == false)
                {
                    sender.SetValue<INRegister.controlQty>(e.Row, ((INRegister)e.Row).TotalQty);
                }
                else
                {
                    sender.SetValue<INRegister.controlQty>(e.Row, 0m);
                }
            }

            if (((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false)
            {
                if ((bool)insetup.Current.RequireControlTotal)
                {
                    if (((INRegister)e.Row).TotalAmount != ((INRegister)e.Row).ControlAmount)
                    {
                        sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, new PXSetPropertyException(PX.Objects.IN.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<INRegister.controlAmount>(e.Row, ((INRegister)e.Row).ControlAmount, null);
                    }

                    if (((INRegister)e.Row).TotalQty != ((INRegister)e.Row).ControlQty)
                    {
                        sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, new PXSetPropertyException(PX.Objects.IN.Messages.DocumentOutOfBalance));
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<INRegister.controlQty>(e.Row, ((INRegister)e.Row).ControlQty, null);
                    }
                }
            }
        }
        
        protected virtual void INRegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            
            if (e.Row == null)
            {
                return;
            }
            var inReg = (INRegister)e.Row;
            if(inReg.Status == null)
            {
                inReg.Status = "B";
            }

            release.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
            iNEdit.SetEnabled(e.Row != null && ((INRegister)e.Row).Hold == false && ((INRegister)e.Row).Released == false);
            iNRegisterDetails.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == true);
            //addInvBySite.SetEnabled(e.Row != null && ((INRegister)e.Row).Released == false);

            PXUIFieldAttribute.SetEnabled(sender, e.Row, ((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
            PXUIFieldAttribute.SetEnabled<INRegister.refNbr>(sender, e.Row, true);
            PXUIFieldAttribute.SetEnabled<INRegister.totalQty>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<INRegister.totalAmount>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<INRegister.totalCost>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<INRegister.status>(sender, e.Row, false);

            sender.AllowInsert = true;
            sender.AllowUpdate = (((INRegister)e.Row).Released == false);
            sender.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

            //lsselect.AllowInsert = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);
            //lsselect.AllowUpdate = (((INRegister)e.Row).Released == false);
            //lsselect.AllowDelete = (((INRegister)e.Row).Released == false && ((INRegister)e.Row).OrigModule == GL.BatchModule.IN);

            PXUIFieldAttribute.SetVisible<INRegister.controlQty>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
            PXUIFieldAttribute.SetVisible<INRegister.controlAmount>(sender, e.Row, (bool)insetup.Current.RequireControlTotal);
            PXUIFieldAttribute.SetVisible<INTran.projectID>(transactions.Cache, null, IsPMVisible);
            PXUIFieldAttribute.SetVisible<INTran.taskID>(transactions.Cache, null, IsPMVisible);
            PXUIFieldAttribute.SetVisible<INRegister.totalCost>(sender, e.Row, ((INRegister)e.Row).Released == true);
        }
        
        protected virtual void INTran_DocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = INDocType.Issue;
        }

        protected virtual void INTran_TranType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = INTranType.Issue;
        }

        protected virtual void INTran_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = INTranType.InvtMult(((INTran)e.Row).TranType);
        }

        protected virtual void INTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            sender.SetDefaultExt<INTran.uOM>(e.Row);
            sender.SetDefaultExt<INTran.tranDesc>(e.Row);
            INTran row = e.Row as INTran;
            INItemClass itemClass = PXSelectJoin<INItemClass, InnerJoin<InventoryItem, On<INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>,
                                        Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
            INItemClassExt INItemClassExt = PXCache<INItemClass>.GetExtension<INItemClassExt>(itemClass);
            if (itemClass != null)
                sender.SetValue<INTran.reasonCode>(e.Row, INItemClassExt.UsrReasonCode);
           
        }

        protected virtual void INTran_UOM_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitPrice(sender, e);
            DefaultUnitCost(sender, e);
        }

        protected virtual void INTran_SiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DefaultUnitPrice(sender, e);
            DefaultUnitCost(sender, e);
        }

        protected virtual void INTran_SOOrderNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void INTran_SOShipmentNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void INTran_ReasonCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            INTran row = e.Row as INTran;
            if (row != null)
            {
                ReasonCode reasoncd = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Optional<ReasonCode.reasonCodeID>>>>.Select(this, e.NewValue);

                if (reasoncd != null && row.ProjectID != null && !ProjectDefaultAttribute.IsProject(this, row.ProjectID))
                {
                    PX.Objects.GL.Account account = PXSelect<PX.Objects.GL.Account, Where<PX.Objects.GL.Account.accountID, Equal<Required<PX.Objects.GL.Account.accountID>>>>.Select(this, reasoncd.AccountID);
                    if (account != null && account.AccountGroupID == null)
                    {
                        sender.RaiseExceptionHandling<INTran.reasonCode>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, PXErrorLevel.Warning, account.AccountCD));
                    }
                }

                e.Cancel = (reasoncd != null) &&
                    (row.TranType != INTranType.Issue && row.TranType != INTranType.Receipt && reasoncd.Usage == ReasonCodeUsages.Sales || reasoncd.Usage == row.DocType);
            }
        }

        protected virtual void INTran_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
            {
                e.Cancel = true;
            }
        }

        protected virtual void INTranSplit_LocationID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
            {
                e.Cancel = true;
            }
        }

        protected virtual void INTran_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
            {
                e.Cancel = true;
            }
        }

        protected virtual void INTranSplit_LotSerialNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (issue.Current != null && issue.Current.OrigModule != GL.BatchModule.IN)
            {
                e.Cancel = true;
            }
        }

        protected virtual void INTranSplit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            //for cluster only. SelectQueries sometimes does not contain all the needed records after failed Save operation
            if (e.TranStatus == PXTranStatus.Aborted && PX.Common.WebConfig.IsClusterEnabled)
            {
                sender.ClearQueryCache();
            }
        }

        protected virtual void INTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<INTran.unitCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);
                PXUIFieldAttribute.SetEnabled<INTran.tranCost>(sender, e.Row, ((INTran)e.Row).InvtMult == 1);
            }
        }

        protected virtual void INTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            INTran row = (INTran)e.Row;
            if (row.Qty <= 0)
            {
                throw new PXSetPropertyException("Can not issue a Qty Zero or less");
            }
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                if (!string.IsNullOrEmpty(row.SOShipmentNbr))
                {
                    if (PXDBQuantityAttribute.Round((decimal)(row.Qty + row.OrigQty)) > 0m)
                    {
                        sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_LE, -row.OrigQty));
                    }
                    else if (PXDBQuantityAttribute.Round((decimal)(row.Qty + row.OrigQty)) < 0m)
                    {
                        sender.RaiseExceptionHandling<INTran.qty>(row, row.Qty, new PXSetPropertyException(CS.Messages.Entry_GE, -row.OrigQty));
                    }
                }
            }

            if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                CheckForSingleLocation(sender, row);
                CheckSplitsForSameTask(sender, row);
                CheckLocationTaskRule(sender, row);
            }





        }

        protected virtual void INRegister_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            #region poo :  Qty Validations without any update to the balances
            INRegister header = (INRegister)e.Row;
            INRegisterExt headerExt = PXCache<INRegister>.GetExtension<INRegisterExt>(header);

            foreach (INTran inTran in this.transactions.Select())
            {
                //if issue , increase the qty to the balance
                if (inTran.TranType == INTranType.Issue)
                {
                    PXSelectBase<Balance> resultBalance = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
                    resultBalance.Cache.ClearQueryCache();
                    //Balance balanceRow = resultBalance.Select(header.UsrOrderID, inTran.InventoryID);
                    Balance balanceRow = resultBalance.Select(headerExt.UsrOrderID, inTran.InventoryID);
                    float? balanceQty = balanceRow.Qty;

                    PXSelectBase<JobOrderStockItems> JobOrderStockItemsResult = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
                    JobOrderStockItemsResult.Cache.ClearQueryCache();
                    //JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(header.UsrOrderID, inTran.InventoryID);
                    JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(headerExt.UsrOrderID, inTran.InventoryID);
                    float? jobOrderStockItemsQty = jobOrderStockItemsResultRow.Quantity;
                    string PartName = jobOrderStockItemsResultRow.InventoryName;
                    //the max Qty We Can Issue
                    //decimal? maxItemQtySupposedToBeIssued = (decimal?)(jobOrderStockItemsQty - balanceQty);
                    decimal? maxItemQtySupposedToBeIssued = (decimal?)(balanceQty);

                    if (balanceQty == 0)
                    {
                        throw new PXSetPropertyException("The Whole Qty for this item Has been issued , please do not try to issue it again");
                    }
                    else
                    {
                        if (inTran.Qty > maxItemQtySupposedToBeIssued)
                        {
                            throw new PXSetPropertyException("Can not issue a Qty bigger than the balance 'Reminder' of specified for this item in the Job Order Part Name" + PartName);
                        }
                        else
                        {
                            // do nothing 
                        }
                    }

                    /**
                        //we commented this as we will not make any updates in here but below
                        //decimal? qty = (decimal?)resulti.Qty - inTran.Qty;
                        //this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", qty), new PXDataFieldRestrict("JoborderID", header.UsrOrderID), new PXDataFieldRestrict("InventoryID", inTran.InventoryID));
                    **/
                }//if return ,decrease the qty from the balance
                else if (inTran.TranType == INTranType.Return)
                {
                    PXSelectBase<Balance> resultBalance = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
                    resultBalance.Cache.ClearQueryCache();
                    //Balance balanceRow = resultBalance.Select(header.UsrOrderID, inTran.InventoryID);
                    Balance balanceRow = resultBalance.Select(headerExt.UsrOrderID, inTran.InventoryID);
                    float? balanceQty = balanceRow.Qty;

                    PXSelectBase<JobOrderStockItems> JobOrderStockItemsResult = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
                    JobOrderStockItemsResult.Cache.ClearQueryCache();
                    //JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(header.UsrOrderID, inTran.InventoryID);
                    JobOrderStockItems jobOrderStockItemsResultRow = JobOrderStockItemsResult.Select(headerExt.UsrOrderID, inTran.InventoryID);
                    float? jobOrderStockItemsQty = jobOrderStockItemsResultRow.Quantity;

                    decimal? maxItemQtyCanBeReturned = (decimal?)(jobOrderStockItemsQty - balanceQty);

                    if (balanceQty == jobOrderStockItemsQty)
                    {
                        throw new PXSetPropertyException("There is nothing issued form the Qty specified for this item on the specified job order , please do not try to return it");
                    }
                    else
                    {
                        if (inTran.Qty > maxItemQtyCanBeReturned)
                        {
                            throw new PXSetPropertyException("Can not return a Qty bigger than the already issued for this item in the Job Order");
                        }
                        else
                        {
                            // do nothing 
                        }
                    }
                }


            }
            #endregion
        }
        protected virtual void INTran_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            INTran row = e.Row as INTran;
            if (row == null) return;

            if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
            {
                if (row.LocationID != null)
                {
                    PXResultset<INLocation> result = PXSelectJoin<INLocation,
                        LeftJoin<PMProject, On<PMProject.contractID, Equal<INLocation.projectID>>>,
                        Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
                        And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(sender.Graph, row.SiteID, row.LocationID);

                    foreach (PXResult<INLocation, PMProject> res in result)
                    {
                        PMProject project = (PMProject)res;
                        if (project != null && project.ContractCD != null && project.VisibleInIN == true)
                        {
                            e.NewValue = project.ContractCD;
                            return;
                        }
                    }
                }
            }
        }

        protected virtual void INTran_TaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            INTran row = e.Row as INTran;
            if (row == null) return;

            if (PM.ProjectAttribute.IsPMVisible(BatchModule.IN))
            {
                if (row.LocationID != null)
                {
                    PXResultset<INLocation> result = PXSelectJoin<INLocation,
                        LeftJoin<PMTask, On<PMTask.projectID, Equal<INLocation.projectID>, And<PMTask.taskID, Equal<INLocation.taskID>>>>,
                        Where<INLocation.siteID, Equal<Required<INLocation.siteID>>,
                        And<INLocation.locationID, Equal<Required<INLocation.locationID>>>>>.Select(sender.Graph, row.SiteID, row.LocationID);

                    foreach (PXResult<INLocation, PMTask> res in result)
                    {
                        PMTask task = (PMTask)res;
                        if (task != null && task.TaskCD != null && task.VisibleInIN == true)
                        {
                            e.NewValue = task.TaskCD;
                            return;
                        }
                    }
                }
            }
        }

        protected virtual void INTran_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            INTran row = e.Row as INTran;
            if (row == null) return;

            sender.SetDefaultExt<INTran.projectID>(e.Row); //will set pending value for TaskID to null if project is changed. This is the desired behavior for all other screens.
            if (sender.GetValuePending<INTran.taskID>(e.Row) == null) //To redefault the TaskID in currecnt screen - set the Pending value from NULL to NOTSET
                sender.SetValuePending<INTran.taskID>(e.Row, PXCache.NotSetValue);
            sender.SetDefaultExt<INTran.taskID>(e.Row);
        }

        protected virtual void DefaultUnitPrice(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object UnitPrice;
            sender.RaiseFieldDefaulting<INTran.unitPrice>(e.Row, out UnitPrice);

            if (UnitPrice != null && (decimal)UnitPrice != 0m)
            {
                decimal? unitprice = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitPrice, INPrecision.UNITCOST);
                sender.SetValueExt<INTran.unitPrice>(e.Row, unitprice);
            }
        }

        protected virtual void DefaultUnitCost(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            object UnitCost;
            sender.RaiseFieldDefaulting<INTran.unitCost>(e.Row, out UnitCost);

            if (UnitCost != null && (decimal)UnitCost != 0m)
            {
                decimal? unitcost = INUnitAttribute.ConvertToBase<INTran.inventoryID>(sender, e.Row, ((INTran)e.Row).UOM, (decimal)UnitCost, INPrecision.UNITCOST);
                sender.SetValueExt<INTran.unitCost>(e.Row, unitcost);
            }
        }

        protected virtual bool IsPMVisible
        {
            get
            {
                PM.PMSetup setup = PXSelect<PM.PMSetup>.Select(this);
                if (setup == null)
                {
                    return false;
                }
                else
                {
                    if (setup.IsActive != true)
                        return false;
                    else
                        return setup.VisibleInIN == true;
                }
            }
        }

        protected virtual void CheckLocationTaskRule(PXCache sender, INTran row)
        {
            if (row.TaskID != null)
            {
                INLocation selectedLocation = (INLocation)PXSelectorAttribute.Select(sender, row, sender.GetField(typeof(INTran.locationID)));

                if (selectedLocation != null && selectedLocation.TaskID != row.TaskID && (selectedLocation.TaskID != null || selectedLocation.ProjectID != null))
                {
                    sender.RaiseExceptionHandling<INTran.locationID>(row, selectedLocation.LocationCD,
                        new PXSetPropertyException(IN.Messages.LocationIsMappedToAnotherTask, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void CheckForSingleLocation(PXCache sender, INTran row)
        {
            if (row.TaskID != null && row.LocationID == null)
            {
                InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, row, sender.GetField(typeof(INTran.inventoryID)));
                if (item != null && item.StkItem == true && row.LocationID == null)
                {
                    sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(PX.Objects.IN.Messages.RequireSingleLocation));
                }
            }
        }

        protected virtual void CheckSplitsForSameTask(PXCache sender, INTran row)
        {
            if (row.HasMixedProjectTasks == true)
            {
                sender.RaiseExceptionHandling<INTran.locationID>(row, null, new PXSetPropertyException(PX.Objects.IN.Messages.MixedProjectsInSplits));
            }
        }
    }

}
