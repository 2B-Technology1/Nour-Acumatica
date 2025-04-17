using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.SM;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.EP;
using System.Globalization;
using PX.Objects.GL;
using static PX.Data.PXQuickProcess;
using Nour20220913V1;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using Nour2024.Helpers;
using Nour2024.Models;
using Newtonsoft.Json;

namespace MyMaintaince
{
    public class JobOrderMaint : PXGraph<JobOrderMaint, JobOrder>
    {
        #region Views
        public PXSetup<SetUp> AutoNumSetup;
        public PXSelect<JobOrder> jobOreder;
        public PXSelect<OperationJOB, Where<OperationJOB.jobOrderID, Equal<Current<JobOrder.jobOrdrID>>>> operationJob;
        public PXSelect<OPenTime, Where<OPenTime.jobOrderID, Equal<Current<JobOrder.jobOrdrID>>>> openTime;
        public PXSelect<Workers, Where<Workers.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>> workers;
        public PXSelect<RelatedCase, Where<RelatedCase.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>> relatedCase;
        //public PXSelect<ARInvoice, Where<ARInvoice.usrJobOrdID, Equal<Current<JobOrder.jobOrdrID>>>> arInvoice;//poo
        public PXSelect<ARInvoice, Where<ARRegisterExt.usrJobOrdID, Equal<Current<JobOrder.jobOrdrID>>>> arInvoice;//poo
        public PXSelect<JobFeadBack, Where<JobFeadBack.joberderID, Equal<Current<JobOrder.jobOrdrID>>>> jobFeedBack;
        public PXSelect<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>> items;
        public PXSelect<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>> nonStockItems;
        public PXSelect<INTran, Where<INTranExt.usrOrdID, Equal<Current<JobOrder.jobOrdrID>>>> IssuedItems;
        public PXSelect<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>>> site;
        public PXSelect<INSiteStatus, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>> siteOp;
        public PXSelect<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>> oper;
        public PXSelect<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>> chas;
        public PXSelect<JobOrder, Where<JobOrder.jobOrdrID, Equal<Current<JobOrder.jobOrdrID>>>> inspection;
        #endregion
        protected bool IsSmsSent = false;
        //public PXSelect<JobOrder, Where<JobOrder.itemsID, Equal<Current<JobOrder.itemsID>>,And<JobOrder.jobOrdrID,NotEqual<Current<JobOrder.jobOrdrID>>>>> HistoryJobs;

        public JobOrderMaint()
        {
            // SetUp setup = AutoNumSetup.Current;

            ActionsMenu.AddMenuAction(StartJobOrder);
            ActionsMenu.AddMenuAction(StopJobOrder);
            ActionsMenu.AddMenuAction(CompleteJobOrder);
            ActionsMenu.AddMenuAction(Restart);
            ActionsMenu.AddMenuAction(PrepareInvoice);
            ActionsMenu.AddMenuAction(ReOpen);
            ActionsMenu.MenuAutoOpen = true;

            PrintMenu.AddMenuAction(Print);
            PrintMenu.AddMenuAction(PrintHistory);
            PrintMenu.AddMenuAction(PrintPerformInvoice);
            PrintMenu.AddMenuAction(PrintOpenJob);
            PrintMenu.MenuAutoOpen = true;


            //PXUIFieldAttribute.SetEnabled(inspection.Cache, null, false);
            //ExitMenu.AddMenuAction(ExitPermision);
            //ExitMenu.MenuAutoOpen = true;

        }
        protected virtual void _(Events.RowPersisted<JobOrder> e)
        {
            var row = e.Row;
            if (row == null) return;
            bool IsSavingForFirtTime = (e.Operation == PXDBOperation.Insert && !IsSmsSent);
            if (IsSavingForFirtTime == true)
            {
                string RefNbr = row.JobOrdrID;
                string message = $"عميلنا العزيز تم فتح أمر شغل تحت رقم: {RefNbr} و يمكنكم المتابعة و الاستفسار من خلال الأتصال علي 19943";
                Customer customer = SelectFrom<Customer>.Where<Customer.acctCD.IsEqual<@P.AsString>>.View.Select(this, row.Customer);
                Contact contact = SelectFrom<Contact>.Where<Contact.bAccountID.IsEqual<@P.AsInt>>.View.Select(this, customer.BAccountID);
                string phone = contact?.Phone1;
                if (!String.IsNullOrEmpty(phone) && RefNbr != "<NEW>")
                {
                    string response = SmsSender.SendMessage(message, phone);
                    IsSmsSent = true;
                    salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);

                    if (responseRoot != null)
                    {
                        if (responseRoot.code == 0)
                        {
                            string responseMessage = responseRoot.message;
                            string smsid = responseRoot.smsid;
                        }

                    }
                }

            }

        }


        ////public PXAction<JobOrder> Test;
        ////[PXUIField(DisplayName = "Test")]
        ////[PXButton(CommitChanges = true)]
        ////protected void test()
        ////{
        ////    SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<JobOrder.branchesss>>>>.Select(this, this.jobOreder.Current.Branchesss);
        ////    if (result != null)
        ////    {
        ////        string lastNumber = result.ReceiptLastRefNbr;
        ////        this.jobOreder.Current.Descrption = lastNumber;
        ////        char[] symbols = lastNumber.ToCharArray();
        ////        for (int i = symbols.Length - 1; i >= 0; i--)
        ////        {
        ////            if (!char.IsDigit(symbols[i]))
        ////                break;
        ////            if (symbols[i] < '9')//a0001
        ////            {
        ////                symbols[i]++;
        ////                break;
        ////            }
        ////            symbols[i] = '0';
        ////        }

        ////        this.jobOreder.Current.JobOrdrID = new string(symbols);
        ////        throw new PXException("ok");
        ////        //this.jobOreder.Current.JobOrdrID = (lastNumber) + 1;
        ////        this.ProviderUpdate<SetUp>(new PXDataFieldAssign("receiptLastRefNbr", this.jobOreder.Current.JobOrdrID), new PXDataFieldRestrict("branchCD", this.jobOreder.Current.Branchesss));// .Accessinfo.BranchID));
        ////        //throw new PXException(this.jobOreder.Current.JobOrdrID);
        ////        // this.jobOreder.Cache.Clear();
        ////    }


        //}
        //public PXAction<JobOrder> ExitMenu;
        //[PXButton]
        //[PXUIField(DisplayName = "Exit")]
        //protected virtual void exitMenu()
        //{
        //}


        // #region Events Attached To Fields

        protected virtual void Workers_workerCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Workers row = e.Row as Workers;
            PXSelectBase<EPEmployee> emps = new PXSelectReadonly<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>(this);
            emps.Cache.ClearQueryCache();
            EPEmployee ep = emps.Select(row.WorkerCode);
            sender.SetValue<Workers.workerName>(row, ep.AcctName);

        }

        protected void InTran_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            INTran row = e.Row as INTran;

            if (row != null)
            {
                if (row.TranType == "RET")
                {
                    row.Qty = row.Qty * -1;
                }
            }

        }

        #region MyButtons (MMK)
        public PXAction<JobOrder> PrintMenu;
        [PXButton]
        [PXUIField(DisplayName = "Print Menu")]
        protected virtual void printMenu()
        {
        }
        public PXAction<JobOrder> Print;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Job Order")]
        protected void print()
        {
            JobOrder JoOrdr = this.jobOreder.Current;
            if (JoOrdr != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["OrderID"] = JoOrdr.JobOrdrID;
                if (JoOrdr.Branchesss == "ABO ROWASH")
                {
                    throw new PXReportRequiredException(parameters, "NO700006", PXBaseRedirectException.WindowMode.New, null);
                }
                else
                {
                    throw new PXReportRequiredException(parameters, "NO700007", PXBaseRedirectException.WindowMode.New, null);
                }
            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }
        public PXAction<JobOrder> PrintPerformInvoice;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Perform Invoice")]
        protected void printPerformInvoice()
        {
            JobOrder JoOrdr = this.jobOreder.Current;
            if (JoOrdr != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["JobOrderID"] = JoOrdr.JobOrdrID;
                throw new PXReportRequiredException(parameters, "NO700020", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }
        public PXAction<JobOrder> PrintHistory;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "print Visit History")]
        protected void printHistory()
        {
            JobOrder JoOrdr = this.jobOreder.Current;

            if (JoOrdr != null)
            {
                PXSelectBase<Items> itm = new PXSelectReadonly<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>>(this);
                itm.Cache.ClearQueryCache();
                Items itms = itm.Select(JoOrdr.ItemsID);
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["ItemsID"] = itms.Code;
                parameters["currentJO"] = JoOrdr.JobOrdrID;
                throw new PXReportRequiredException(parameters, "AN800001", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }
        public PXAction<JobOrder> PrintOpenJob;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "print Open Jobs")]
        protected void printOpenJob()
        {
            JobOrder JoOrdr = this.jobOreder.Current;

            if (JoOrdr != null)
            {
                PXSelectBase<Items> itm = new PXSelectReadonly<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>>(this);
                itm.Cache.ClearQueryCache();
                Items itms = itm.Select(JoOrdr.ItemsID);
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["ItemsID"] = itms.Code;
                parameters["currentJO"] = JoOrdr.JobOrdrID;
                throw new PXReportRequiredException(parameters, "AN800002", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }
        #endregion

        #region Buttons

        #region Exit Permision

        public PXAction<JobOrder> ExitPermision;
        [PXUIField(DisplayName = "Exit Permision", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton()]
        protected virtual IEnumerable exitPermision(PXAdapter adapter)
        {
            JobOrder joorder = this.jobOreder.Current;
            if (joorder.Price > 0)
            {
                throw new PXException("Total Price grater Than Zero");
            }
            PXSelectBase<JobOrderNonStockItems> nonStk = new PXSelectReadonly<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>>>(this);
            nonStk.Cache.ClearQueryCache();
            PXResultset<JobOrderNonStockItems> resultNonStk = nonStk.Select(joorder.JobOrdrID);
            float? Time = 0;
            foreach (JobOrderNonStockItems nonStkItem in resultNonStk)
            {
                Time = Time + nonStkItem.Time;
            }
            if (Time > 0)
            {
                throw new PXException("Service Time grater Than Zero");
            }

            PXSelectBase<JobOrderStockItems> Stk = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>>>(this);
            Stk.Cache.ClearQueryCache();
            PXResultset<JobOrderStockItems> resultStk = Stk.Select(joorder.JobOrdrID);
            float? Qty = 0;
            foreach (JobOrderStockItems StkItem in resultStk)
            {
                Qty = Qty + StkItem.Quantity;
            }
            if (Qty > 0)
            {
                throw new PXException("Stock Quantity grater Than Zero");
            }
            //Last Stage
            jobOreder.Cache.SetValue<JobOrder.Status>(jobOreder.Current, JobOrderStatus.Finished);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Finished), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.finishedDateTime>(jobOreder.Current, DateTime.Now);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("finishedDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.finishedByUserName>(jobOreder.Current, this.Accessinfo.UserName);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("finishedByUserName", this.Accessinfo.UserName), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));




            StartJobOrder.SetEnabled(false);
            StopJobOrder.SetEnabled(false);
            CompleteJobOrder.SetEnabled(false);
            Restart.SetEnabled(false);
            PrepareInvoice.SetEnabled(false);
            ReOpen.SetEnabled(true);
            AddOpenTime.SetEnabled(false);

            //close all view
            PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
            PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
            PXUIFieldAttribute.SetEnabled(jobFeedBack.Cache, null, false);
            PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
            PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);
            nonStockItems.AllowDelete = false;

            items.AllowDelete = false;
            workers.AllowDelete = false;
            this.Actions.PressSave();
            return adapter.Get();
        }

        #endregion

        #region Actions

        public PXAction<JobOrder> ActionsMenu;
        [PXButton]
        [PXUIField(DisplayName = "Actions")]
        protected virtual void actionsMenu()
        {
        }


        #endregion

        #region Start Job Order

        public PXAction<JobOrder> StartJobOrder;
        [PXUIField(DisplayName = "Start Job Order",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public IEnumerable startJobOrder(PXAdapter adapter)
        {

            StartJobOrder.SetEnabled(false);
            StopJobOrder.SetEnabled(true);
            CompleteJobOrder.SetEnabled(true);
            Restart.SetEnabled(false);
            PrepareInvoice.SetEnabled(false);
            ReOpen.SetEnabled(false);
            AddOpenTime.SetEnabled(true);

            if (!this.jobOreder.Current.status.Equals(JobOrderStatus.Finished) && !this.jobOreder.Current.status.Equals(JobOrderStatus.Started))
            {
                jobOreder.Cache.SetValue<JobOrder.Status>(jobOreder.Current, JobOrderStatus.Started);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Started), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.startDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("startDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.startByUserName>(jobOreder.Current, this.Accessinfo.UserName);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("startByUserName", this.Accessinfo.UserName), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                this.Actions.PressSave();
            }

            return adapter.Get();
        }

        #endregion

        #region Stop Job Order

        public PXAction<JobOrder> StopJobOrder;
        [PXUIField(DisplayName = "Stop Job Order",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public IEnumerable stopJobOrder(PXAdapter adapter)
        {
            Persist();
            JobOrder jobOrder = jobOreder.Current as JobOrder;
            if (String.IsNullOrEmpty(jobOrder.StopReason))
            {
                throw new PXException("Please Insert The Stop Reason First and then press save ");
            }
            CompleteJobOrder.SetEnabled(false);
            StartJobOrder.SetEnabled(true);
            StopJobOrder.SetEnabled(false);
            Restart.SetEnabled(false);
            PrepareInvoice.SetEnabled(false);
            ReOpen.SetEnabled(false);
            AddOpenTime.SetEnabled(false);
            if (!this.jobOreder.Current.status.Equals(JobOrderStatus.Finished))
            {
                jobOreder.Cache.SetValue<JobOrder.Status>(jobOreder.Current, JobOrderStatus.Stoped);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Stoped), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                this.Actions.PressSave();
            }

            return adapter.Get();
        }

        #endregion

        #region Complete Job Order

        public PXAction<JobOrder> CompleteJobOrder;
        [PXUIField(DisplayName = "Complete Job Order",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public IEnumerable completeJobOrder(PXAdapter adapter)
        {

            Persist();

            JobOrder jobOrder = jobOreder.Current as JobOrder;
            if (jobOrder != null)
            {
                //if ((bool)jobOrder.Complete == true)



                #region validate if all items on this job order issued
                PXSelectBase<Balance> jobOrderBalances = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>>>(this);
                jobOrderBalances.Cache.ClearQueryCache();
                //PXResultset<Balance> jobOrderBalancesset = jobOrderBalances.Select(header.UsrOrderID);
                PXResultset<Balance> jobOrderBalancesset = jobOrderBalances.Select(jobOrder.JobOrdrID);
                bool allItemsIssuedFlag = true;
                foreach (PXResult<Balance> record in jobOrderBalancesset)
                {
                    Balance row = (Balance)record;
                    if (row.Qty != 0)
                    {
                        allItemsIssuedFlag = false;
                        break;
                    }
                }
                #endregion

                #region validate that there is an open time for every non-stock item
                //get the outer join 



                PXSelectBase<JobOrderNonStockItems> jobOrderNonStockItems =
                    new PXSelectReadonly2<JobOrderNonStockItems,
                 LeftJoin<OPenTime, On<OPenTime.jobOrderID, Equal<JobOrderNonStockItems.jobOrdrID>,
                 And<OPenTime.inventoryCode,
                 Equal<JobOrderNonStockItems.inventoryCode>>>,
                 LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<JobOrderNonStockItems.inventoryCode>>>>,
                 Where<OPenTime.jobTimeID, IsNull, And<JobOrderNonStockItems.jobOrdrID,
                 Equal<Required<JobOrderNonStockItems.jobOrdrID>>, And<InventoryItem.body, IsNull>>>>(this);

                jobOrderBalances.Cache.ClearQueryCache();
                PXResultset<JobOrderNonStockItems> jobOrderNonStockItemsset = jobOrderNonStockItems.Select(jobOrder.JobOrdrID);

                bool allNonStkItemsHasOpenTimeFlag = true;

                if (jobOrderNonStockItemsset.Count > 0)
                {
                    allNonStkItemsHasOpenTimeFlag = false;
                }

                #endregion

                #region validate if all times is closed
                PXSelectBase<OPenTime> jobOrderOpenTime = new PXSelectReadonly<OPenTime, Where<OPenTime.jobOrderID, Equal<Required<OPenTime.jobOrderID>>>>(this);
                jobOrderOpenTime.Cache.ClearQueryCache();
                PXResultset<OPenTime> jobOrderOpenTimeset = jobOrderOpenTime.Select(jobOrder.JobOrdrID);
                bool allOpenTimesClosedFlag = true;
                foreach (PXResult<OPenTime> record in jobOrderOpenTimeset)
                {
                    OPenTime row = (OPenTime)record;
                    if (row.Status != OpenTimeStatus.Closed)
                    {
                        allOpenTimesClosedFlag = false;
                        break;
                    }
                }
                #endregion


                if (!allItemsIssuedFlag)
                {
                    throw new PXSetPropertyException("Please make sure all items have been issued first before completing the job order", PXErrorLevel.Error);
                }

                if (!allOpenTimesClosedFlag)
                {
                    throw new PXSetPropertyException("Please make sure all open times have been closed first before completing the job order", PXErrorLevel.Error);
                }

                if (!allNonStkItemsHasOpenTimeFlag)
                {
                    throw new PXSetPropertyException("Please make sure all Non Stock have been processed by opening time for every one before completing the job order", PXErrorLevel.Error);
                }

                if (allItemsIssuedFlag && allOpenTimesClosedFlag && allNonStkItemsHasOpenTimeFlag)
                {
                    //try
                    //{
                    this.jobOreder.Cache.SetValue<JobOrder.Status>(jobOrder, JobOrderStatus.Completed);
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Completed), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    this.Actions.PressSave();
                    //}
                    //catch
                    //{
                    //    throw new PXSetPropertyException("Please make data is Right before completing the job order", PXErrorLevel.Error);
                    //}
                }
                StartJobOrder.SetEnabled(false);
                StopJobOrder.SetEnabled(false);
                CompleteJobOrder.SetEnabled(false);
                Restart.SetEnabled(true);
                PrepareInvoice.SetEnabled(true);
                ReOpen.SetEnabled(false);
                AddOpenTime.SetEnabled(false);
                ExitPermision.SetVisible(true);
                //close all view
                PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(jobFeedBack.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);

            }

            return adapter.Get();
        }

        #endregion

        #region Add Open Time

        public PXAction<JobOrder> AddOpenTime;
        [PXUIField(DisplayName = "Add Open Time",
        MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton()]
        public IEnumerable addOpenTime(PXAdapter adapter)
        {

            JobOrder jobOrder = jobOreder.Current as JobOrder;
            if (jobOrder.status == "Open")
            {
                throw new PXException("Please Start The Job Order First ");
            }
            OPenTime op = new OPenTime();
            op.JobOrderID = this.jobOreder.Current.JobOrdrID;

            DateTime date = DateTime.Now;
            op.StartTime = date;

            //String hour;
            String min;
            /**
            if (date.Hour < 10)
            {
                hour = "0" + date.Hour;
            }
            else
            {
                hour = date.Hour.ToString();
            }
            **/
            if (date.Minute < 10)
            {
                min = "0" + date.Minute;
            }
            else
            {
                min = date.Minute.ToString();
            }

            if (date.Hour < 12)
            {
                if (date.Hour < 10)
                    op.StarTime = "0" + date.Hour + ":" + min + ":00 AM";
                else
                    op.StarTime = date.Hour + ":" + min + ":00 AM";
            }
            else
            {
                if ((date.Hour - 12) < 10)
                    op.StarTime = "0" + (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                else
                    op.StarTime = (date.Hour - 12).ToString() + ":" + min + ":00 PM";
            }


            OpenTimeMaint graph = PXGraph.CreateInstance<OpenTimeMaint>();
            graph.openTime.Insert(op);
            //graph.Persist();
            throw new PXPopupRedirectException(graph, null, true);

           // return adapter.Get();
        }

        #endregion

        #region View OpenTime

        public PXAction<JobOrder> ViewOpenTime;
        [PXButton]
        [PXUIField(DisplayName = "View OpenTime")]
        protected virtual void viewOpenTime()
        {
            OPenTime row = openTime.Current;
            OpenTimeMaint graph = PXGraph.CreateInstance<OpenTimeMaint>();
            graph.openTime.Current =
            graph.openTime.Search<OPenTime.openTimeID>(row.OpenTimeID);

            if (graph.openTime.Current != null)
            {
                throw new PXPopupRedirectException(graph, null, true);
            }
        }

        #endregion

        #region View invoice

        public PXAction<JobOrder> ViewInvoice;
        [PXButton]
        [PXUIField(DisplayName = "View invoice")]
        protected virtual void viewInvoice()
        {
            ARInvoice row = arInvoice.Current;
            ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();
            graph.Document.Current =
            graph.Document.Search<ARInvoice.refNbr>(row.RefNbr);

            if (graph.Document.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, null);
            }

        }

        #endregion

        #region Reopen Job Order

        public PXAction<JobOrder> Restart;
        [PXButton]
        [PXUIField(DisplayName = "Reopen Job Order")]
        protected virtual void restart()
        {
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Started), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            this.jobOreder.Current.status = JobOrderStatus.Started;
            jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
            this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            jobOreder.Cache.SetValue<JobOrder.Status>(this.jobOreder.Current, JobOrderStatus.Started);



            StartJobOrder.SetEnabled(false);
            StopJobOrder.SetEnabled(true);
            CompleteJobOrder.SetEnabled(true);
            Restart.SetEnabled(false);
            PrepareInvoice.SetEnabled(false);
            ReOpen.SetEnabled(false);
            AddOpenTime.SetEnabled(true);


            //close all view
            PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
            PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
            PXUIFieldAttribute.SetEnabled(jobFeedBack.Cache, null, true);
            PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
            PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);
            nonStockItems.AllowDelete = true;
            items.AllowDelete = true;
            workers.AllowDelete = true;

        }

        #endregion

        #region Prepare Invoice

        public PXAction<JobOrder> PrepareInvoice;
        [PXUIField(DisplayName = "Prepare Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton]
        protected virtual IEnumerable prepareInvoice(PXAdapter adapter)
        {
            Persist();
            PXLongOperation.StartOperation((PXGraph)this, delegate
            {

                JobOrder joorder = this.jobOreder.Current;
                Customer c = PXSelect<Customer, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>.Select(this, joorder.Customer);

                ARInvoiceEntry invoice = PXGraph.CreateInstance<ARInvoiceEntry>();



                invoice.Accessinfo.BranchID = this.jobOreder.Current.BranchID;
                ARInvoice header = new ARInvoice();
               //ARRegisterExt headerExt = PXCache<ARRegister>.GetExtension<ARRegisterExt>(header);


               ArrayList tran = new ArrayList();

               #region Query General Setup
               SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<SetUp.branchCD>>>>.Select(this, this.jobOreder.Current.Branchesss);

               #endregion

               #region Query All Non-Stock Quantites and Prices
               //invoice base on the standard time

               PXSelectBase<JobOrderNonStockItems> nonStk = new PXSelectReadonly<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>>>(this);
                nonStk.Cache.ClearQueryCache();
                PXResultset<JobOrderNonStockItems> resultNonStk = nonStk.Select(joorder.JobOrdrID);

                Dictionary<int?, float?> itemDictionary = new Dictionary<int?, float?>();
                foreach (JobOrderNonStockItems nonStkItem in resultNonStk)
                {

                    itemDictionary.Add(nonStkItem.InventoryCode, nonStkItem.Time);
                }


               //invoice based on the User Specified Time
               /**
               PXSelectBase<OPenTime> oPenTimes = new PXSelectReadonly<OPenTime, Where<OPenTime.jobOrderID, Equal<Required<OPenTime.jobOrderID>>>>(this);
               oPenTimes.Cache.ClearQueryCache();
               PXResultset<OPenTime> oPenTimesset = oPenTimes.Select(joorder.JobOrdrID);

               Dictionary<int?, float?> itemDictionary = new Dictionary<int?, float?>();
               foreach (OPenTime nonStkItem in oPenTimesset)
               {
                   itemDictionary.Add(nonStkItem.InventoryCode, float.Parse(nonStkItem.ManualDauration, CultureInfo.InvariantCulture.NumberFormat));
               }
               **/
               #endregion

               #region Query All Stock Quantites and Prices
               PXSelectBase<JobOrderStockItems> Stk = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>>>(this);
                Stk.Cache.ClearQueryCache();
                PXResultset<JobOrderStockItems> resultStk = Stk.Select(joorder.JobOrdrID);

                foreach (JobOrderStockItems StkItem in resultStk)
                {
                    if (StkItem.Quantity > 0)
                    {
                        itemDictionary.Add(StkItem.InventoryCode, StkItem.Quantity);
                    }

                }
               #endregion
               //create transaction lines for the stock and non stock 
               foreach (int? itm in itemDictionary.Keys)
                {
                    InventoryItem Inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, itm);
                    ARTran arTran = new ARTran();
                    arTran.InventoryID = itm;
                    arTran.Qty = Convert.ToDecimal(itemDictionary[itm]);
                    arTran.Commissionable = false;
                    arTran.SubID = result.SubAccount;
                   //arTran.SubID = Inv.SalesSubID;
                   tran.Add(arTran);
                }

                header.CustomerID = c.BAccountID;
                header.DocType = "INV";
                header.Hold = true;
               //header.DocDate =this.Base.Document.Current.Accessinfo.BusinessDate;
               //headerExt.UsrJobOrdID = jobOreder.Current.JobOrdrID;

               invoice.Document.Cache.RaiseFieldUpdated<ARInvoice.termsID>(header, null);
                invoice.Document.Cache.RaiseFieldUpdated<ARInvoice.customerID>(header, null);

                invoice.Document.Insert(header);

                foreach (ARTran record in tran)
                {
                    if (record.InventoryID.HasValue)
                    {
                        ARTran artran = invoice.Transactions.Insert(record);
                       //invoice.Transactions.Cache.RaiseFieldUpdated<ARTran.inventoryID>(record, null);
                       //invoice.Transactions.Cache.RaiseFieldUpdated<ARTran.taxCategoryID>(record, null);
                       //invoice.Transactions.Cache.RaiseFieldUpdated<ARTran.salesPersonID>(record, null);
                       //invoice.Transactions.Cache.RaiseFieldUpdated<ARTran.qty>(record, null);
                       //ARInvoiceEntry invoice = ARDataEntryGraph<ARInvoiceEntry, ARInvoice>.CreateInstance<ARInvoiceEntry>();
                       invoice.RecalculateDiscounts(invoice.Transactions.Cache, artran);
                        invoice.Transactions.Update(artran);
                    }
                }
                invoice.Persist();
                this.ProviderUpdate<ARRegister>(new PXDataFieldAssign("UsrJobOrdID", jobOreder.Current.JobOrdrID), new PXDataFieldRestrict("DocType", PXDbType.NVarChar, 50, invoice.Document.Current.DocType, PXComp.EQ), new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 50, invoice.Document.Current.RefNbr, PXComp.EQ));


               //Last Stage
               this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.Finished), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.finishedDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("finishedDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.finishedByUserName>(jobOreder.Current, this.Accessinfo.UserName);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("finishedByUserName", this.Accessinfo.UserName), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));




                StartJobOrder.SetEnabled(false);
                StopJobOrder.SetEnabled(false);
                CompleteJobOrder.SetEnabled(false);
                Restart.SetEnabled(false);
                PrepareInvoice.SetEnabled(false);
                ReOpen.SetEnabled(true);
                AddOpenTime.SetEnabled(false);

               //close all view
               PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(jobFeedBack.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);
                nonStockItems.AllowDelete = false;

                items.AllowDelete = false;
                workers.AllowDelete = false;


            });


            return adapter.Get();
        }

        #endregion

        #region Return Job Order

        public PXAction<JobOrder> ReOpen;
        [PXButton]
        [PXUIField(DisplayName = "Return Job Order")]
        protected virtual void reOpen()
        {
            if (this.jobOreder.Current.status == JobOrderStatus.Completed)
            {
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.ReOpen), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                this.jobOreder.Current.status = JobOrderStatus.ReOpen;
                jobOreder.Cache.SetValue<JobOrder.Status>(this.jobOreder.Current, JobOrderStatus.ReOpen);
                jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
            }
            else if (this.jobOreder.Current.status == JobOrderStatus.Finished)
            {


                PXSelectBase<ARInvoice> aRInvoices = new PXSelectReadonly<ARInvoice, Where<ARRegisterExt.usrJobOrdID, Equal<Required<JobOrder.jobOrdrID>>, And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>>>>(this);
                aRInvoices.Cache.ClearQueryCache();
                PXResultset<ARInvoice> invInvoices = aRInvoices.Select(this.jobOreder.Current.JobOrdrID, ARDocType.Invoice);
                if (invInvoices.Count > 0) //check if there is an invoices for this job order make sure it is returned 
                {
                    bool isThereInvoiceWithoutRetrun = false;
                    string firstInvoiceWithoutReturn = "";
                    foreach (PXResult<ARInvoice> rec in invInvoices)
                    {
                        ARInvoice record = (ARInvoice)rec;
                        PXSelectBase<ARInvoice> returnInvoices = new PXSelectReadonly<ARInvoice, Where<ARRegisterExt.usrJobOrdID, Equal<Required<JobOrder.jobOrdrID>>, And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.origRefNbr, Equal<Required<ARInvoice.origRefNbr>>>>>>(this);
                        returnInvoices.Cache.ClearQueryCache();
                        PXResultset<ARInvoice> crmInvoices = returnInvoices.Select(this.jobOreder.Current.JobOrdrID, ARDocType.CreditMemo, record.RefNbr);

                        if (crmInvoices.Count <= 0)
                        { // this means no return for already existed invoice
                            isThereInvoiceWithoutRetrun = true;
                            //this.jobOreder.Cache.RaiseExceptionHandling("ReOpen", this.jobOreder.Current, null, new PXSetPropertyException("Can not Reopen the order before returning the Invoice Nbr.: " + record.RefNbr + "", PXErrorLevel.Error));
                            firstInvoiceWithoutReturn = record.RefNbr;
                            break;
                        }
                        else
                        {
                            // do nothing
                        }
                    }

                    if (!isThereInvoiceWithoutRetrun)
                    {
                        this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.ReOpen), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                        this.jobOreder.Current.status = JobOrderStatus.ReOpen;
                        jobOreder.Cache.SetValue<JobOrder.Status>(this.jobOreder.Current, JobOrderStatus.ReOpen);
                        jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                        this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                        jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                        this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    }
                    else
                    {
                        throw new PXSetPropertyException("Can not Reopen the order before returning the Invoice Nbr.: " + firstInvoiceWithoutReturn + "", PXErrorLevel.Error);
                    }

                }
                else
                {//if there is no invoices for this job order just reopen it 
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("Status", JobOrderStatus.ReOpen), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    this.jobOreder.Current.status = JobOrderStatus.ReOpen;
                    jobOreder.Cache.SetValue<JobOrder.Status>(this.jobOreder.Current, JobOrderStatus.ReOpen);
                    jobOreder.Cache.SetValue<JobOrder.lastUpdateDateTime>(jobOreder.Current, DateTime.Now);
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateDateTime", DateTime.Now), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));
                    jobOreder.Cache.SetValue<JobOrder.lastUpdateByID>(jobOreder.Current, this.Accessinfo.UserID);
                    this.ProviderUpdate<JobOrder>(new PXDataFieldAssign("lastUpdateByID", this.Accessinfo.UserID), new PXDataFieldRestrict("jobOrdrID", PXDbType.NVarChar, 50, jobOreder.Current.JobOrdrID, PXComp.EQ));


                    StartJobOrder.SetEnabled(true);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);
                    AddOpenTime.SetEnabled(true);

                    //close all view
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(jobFeedBack.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);
                    nonStockItems.AllowDelete = true;
                    items.AllowDelete = true;
                    workers.AllowDelete = true;
                }





            }
        }

        #endregion

        #endregion

        protected decimal? CalcLinePrice(decimal? Price, decimal? qty)
        {
            return Price * qty;
        }

        #region Non Stock Events

        protected virtual void JobOrderNonStockItems_opId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {
                JobOrderNonStockItems jobOrder = e.Row as JobOrderNonStockItems;

                if (!String.IsNullOrEmpty(jobOrder.OpId + ""))
                {
                    int? Opid = (int?)jobOrder.OpId;
                    PXSelectBase<Operation> ChassisBase = new PXSelectReadonly<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Operation Chassis = ChassisBase.Select(Opid);
                    try
                    {
                        sender.SetValue<JobOrderNonStockItems.operationname>(jobOrder, Chassis.OperationName);
                    }
                    catch
                    { }

                }
            }

        }
        protected virtual void JobOrderNonStockItems_inventoryCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {
                JobOrderNonStockItems row = e.Row as JobOrderNonStockItems;
                #region Update Descr. Field
                try
                {
                    PXSelectBase<InventoryItem> invt = new PXSelectReadonly<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<JobOrderNonStockItems.inventoryCode>>>>(this);
                    invt.Cache.ClearQueryCache();
                    InventoryItem result = invt.Select(row.InventoryCode);
                    sender.SetValue<JobOrderNonStockItems.inventoryName>(row, result.Descr);
                }
                catch
                {
                    //throw new PXException("Update Descr");
                }
                #endregion
                try
                {
                    #region Update Price Field
                    //PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.expirationDate, IsNull>>>(this);
                    PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.expirationDate, IsNull, And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>>>>>(this);

                    salesPrices.Cache.ClearQueryCache();
                    ARSalesPrice resultSalesPrices = salesPrices.Select(row.InventoryCode);
                    if (resultSalesPrices != null)
                    {
                        sender.SetValue<JobOrderNonStockItems.nonStkPrice>(row, resultSalesPrices.SalesPrice);
                    }
                    else
                    {
                        sender.RaiseExceptionHandling<JobOrderNonStockItems.inventoryCode>(row, 0.0, new PXSetPropertyException("There is no Sales Prices For this Item!", PXErrorLevel.Warning));
                    }
                }
                catch
                {
                    //throw new PXException("Update Price");
                }
                #endregion

                // get standard qty
                PXSelectBase<JobTimeAttachedNonStocks> jobTimeAttachedNonStocks = new PXSelectReadonly<JobTimeAttachedNonStocks, Where<JobTimeAttachedNonStocks.nonStockID, Equal<Required<JobTimeAttachedNonStocks.nonStockID>>>>(this);
                jobTimeAttachedNonStocks.Cache.ClearQueryCache();
                PXResultset<JobTimeAttachedNonStocks> jobTimeAttachedNonStocksResult = jobTimeAttachedNonStocks.Select(row.InventoryCode);
                if (jobTimeAttachedNonStocksResult.Count <= 0)
                {
                    throw new PXException("this item is not defined in any job Time!");
                }
                else
                {
                    JobTimeAttachedNonStocks jobTimeAttachedNonStock = jobTimeAttachedNonStocksResult;
                    sender.SetValue<JobOrderNonStockItems.time>(row, jobTimeAttachedNonStock.StandardQty);

                }

                try
                {

                    if (row.NonStkPrice != null)
                    {
                        if (row.Time != null)
                        {

                            row.LinePrice = CalcLinePrice(row.NonStkPrice, Convert.ToDecimal(row.Time));
                        }
                    }
                }
                catch
                {

                }
            }
        }
        protected void JobOrderNonStockItems_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                try
                {
                    JobOrderNonStockItems jobOrder = e.Row as JobOrderNonStockItems;

                    if (!String.IsNullOrEmpty(jobOrder.OpId + ""))
                    {
                        int? Opid = (int?)jobOrder.OpId;
                        PXSelectBase<Operation> ChassisBase = new PXSelectReadonly<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>>(this);
                        ChassisBase.Cache.ClearQueryCache();
                        Operation Chassis = oper.Select(Opid);
                        try
                        {
                            sender.SetValue<JobOrderNonStockItems.operationname>(jobOrder, Chassis.OperationName);
                        }
                        catch
                        { }

                    }
                }
                catch { }
            }
        }
        protected virtual void JobOrderNonStockItems_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {

            JobOrder row = jobOreder.Current;
            PXSelectBase<ARInvoice> itm4 = new PXSelect<ARInvoice, Where<ARRegisterExt.usrJobOrdID, Equal<Required<ARRegisterExt.usrJobOrdID>>>>(this);
            PXResultset<ARInvoice> result4 = itm4.Select(row.JobOrdrID);
            if (result4.Count > 0)
            {
                throw new PXException("It is not Allow to delete this Service coz there is an Invoice related");
            }
        }
        protected virtual void JobOrderNonStockItems_NonStkPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            JobOrderNonStockItems line = (JobOrderNonStockItems)e.Row;
            if (line.NonStkPrice != null)
            {
                line.LinePrice = CalcLinePrice(line.NonStkPrice, Convert.ToDecimal(line.Time));
            }

        }
        protected virtual void JobOrderNonStockItems_Time_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            JobOrderNonStockItems line = (JobOrderNonStockItems)e.Row;
            if (line.Time != null)
            {
                line.LinePrice = CalcLinePrice(line.NonStkPrice, Convert.ToDecimal(line.Time));
            }
        }
        protected virtual void JobOrderNonStockItems_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            JobOrderNonStockItems line = (JobOrderNonStockItems)e.Row;
            JobOrder order = jobOreder.Current;

            bool isUpdated = false;

            if (line.LinePrice != null)
            {
                order.Price += line.LinePrice;
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }



        }
        protected virtual void JobOrderNonStockItems_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            JobOrderNonStockItems newline = (JobOrderNonStockItems)e.Row;
            JobOrderNonStockItems oldline = (JobOrderNonStockItems)e.OldRow;
            JobOrder order = jobOreder.Current;
            bool isUpdated = false;
            if (!sender.ObjectsEqual<JobOrderNonStockItems.linePrice>(newline, oldline))
            {
                if (oldline.LinePrice != null)
                {
                    order.Price -= oldline.LinePrice;
                }
                if (newline.LinePrice != null)
                {
                    order.Price += newline.LinePrice;
                }
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }
        }
        protected virtual void JobOrderNonStockItems_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            JobOrderNonStockItems line = (JobOrderNonStockItems)e.Row;
            JobOrder order = jobOreder.Current;
            PXEntryStatus orderStatus = jobOreder.Cache.GetStatus(order);
            bool isDeleted = orderStatus == PXEntryStatus.InsertedDeleted || orderStatus == PXEntryStatus.Deleted;
            if (isDeleted == true)
            {
                return;
            }
            bool isUpdated = false;

            if (line.LinePrice != null)
            {
                order.Price -= line.LinePrice;
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }

        }


        public virtual void JobOrderNonStockItems_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null)
            {
                JobOrderNonStockItems row = e.Row as JobOrderNonStockItems;
                PXSelectBase<JobOrderNonStockItems> jobOrdernonStockItems = new PXSelectReadonly<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>, And<JobOrderNonStockItems.inventoryCode, Equal<Required<JobOrderNonStockItems.inventoryCode>>>>>(this);
                jobOrdernonStockItems.Cache.ClearQueryCache();
                PXResultset<JobOrderNonStockItems> jobOrdernonStockItemsResult = jobOrdernonStockItems.Select(jobOreder.Current.JobOrdrID, row.InventoryCode);
                if (row.Time <= 0)
                {    //if Time zero or less stop and throw an exception
                    e.Cancel = true;
                    //throw new PXException("The Quantity Can not Be Zero or Less");
                    sender.RaiseExceptionHandling<JobOrderNonStockItems.time>(row, row.Time, new PXSetPropertyException("The Time Can not Be Zero or Less", PXErrorLevel.Error));
                }
            }


        }
        #endregion

        #region Stock Events

        protected virtual void JobOrderStockItems_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {

            //JobOrder row = jobOreder.Current;
            //PXSelectBase<INTran> itm3 = new PXSelect<INTran, Where<INTranExt.usrOrdID, Equal<Required<INTranExt.usrOrdID>>>>(this);
            //PXResultset<INTran> result3 = itm3.Select(row.JobOrdrID);
            //if (result3.Count > 0)
            //{
            //    throw new PXException("It is not Allow to delete Spare Parts coz there is Issues related");
            //    //e.Cancel = true;
            //    //return;
            //}  
        }
        protected virtual void JobOrderStockItems_inventoryCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            #region Update Description Field
            JobOrderStockItems row = e.Row as JobOrderStockItems;
            PXSelectBase<InventoryItem> invt = new PXSelectReadonly<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(this);
            invt.Cache.ClearQueryCache();
            InventoryItem result = invt.Select(row.InventoryCode);
            sender.SetValue<JobOrderStockItems.inventoryName>(row, result.Descr);
            #endregion

            #region Update Price Field
            PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.expirationDate, IsNull,
                And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>>>>>(this);

            salesPrices.Cache.ClearQueryCache();
            ARSalesPrice resultSalesPrices = salesPrices.Select(row.InventoryCode);
            if (resultSalesPrices != null)
            {
                sender.SetValue<JobOrderStockItems.stkPrice>(row, resultSalesPrices.SalesPrice);
            }
            else
            {
                sender.RaiseExceptionHandling<JobOrderNonStockItems.inventoryCode>(row, 0.0, new PXSetPropertyException("There is no Sales Prices For this Item!", PXErrorLevel.Warning));
            }
            #endregion

            #region Update Stock Qty OnHand Field
            if (e.Row != null)
            {


                if (!String.IsNullOrEmpty(row.InventoryCode + ""))
                {
                    int? InvId = (int?)row.InventoryCode;
                    PXSelectBase<INSiteStatus> ChassisBase = new PXSelectReadonly<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();


                    PXResultset<INSiteStatus> Chassis = ChassisBase.Select(InvId);

                    decimal? x = 0;
                    foreach (INSiteStatus nonStkItem in Chassis)
                    {

                        x = x + nonStkItem.QtyOnHand;
                    }

                    try
                    {
                        sender.SetValue<JobOrderStockItems.stkQty>(row, x);
                    }
                    catch
                    { }


                }
            }
            #endregion
            try
            {

                if (row.StkPrice != null)
                {
                    if (row.Quantity != null)
                    {

                        row.LinePrice = CalcLinePrice(row.StkPrice, Convert.ToDecimal(row.Quantity));
                    }
                }
            }
            catch
            {

            }

        }
        protected void JobOrderStockItems_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                JobOrderStockItems jobOrder = e.Row as JobOrderStockItems;

                if (!String.IsNullOrEmpty(jobOrder.OpId + ""))
                {
                    int? Opid = (int?)jobOrder.OpId;
                    var ChassisBase = new PXSelectReadonly<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Operation Chassis = ChassisBase.Select(Opid);
                    try
                    {
                        sender.SetValue<JobOrderStockItems.operationname>(jobOrder, Chassis.OperationName);
                    }
                    catch
                    { }

                }
            }
            #region Update Stock Qty OnHand Field
            if (e.Row != null)
            {
                JobOrderStockItems row = e.Row as JobOrderStockItems;

                if (!String.IsNullOrEmpty(row.InventoryCode + ""))
                {
                    int? InvId = (int?)row.InventoryCode;
                    PXSelectBase<INSiteStatus> ChassisBase = new PXSelectReadonly<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();


                    //PXResultset<INSiteStatus> Chassis = ChassisBase.Select(InvId);
                    var Chassis = site.Select(InvId);

                    decimal? x = 0;
                    foreach (INSiteStatus nonStkItem in Chassis)
                    {

                        x = x + nonStkItem.QtyOnHand;
                    }

                    try
                    {
                        sender.SetValue<JobOrderStockItems.stkQty>(row, x);
                    }
                    catch
                    { }


                }
            }
            #endregion
        }
        protected virtual void JobOrderStockItems_opId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {
                JobOrderStockItems jobOrder = e.Row as JobOrderStockItems;

                if (!String.IsNullOrEmpty(jobOrder.OpId + ""))
                {
                    int? Opid = (int?)jobOrder.OpId;
                    PXSelectBase<Operation> ChassisBase = new PXSelectReadonly<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Operation Chassis = ChassisBase.Select(Opid);
                    try
                    {
                        sender.SetValue<JobOrderStockItems.operationname>(jobOrder, Chassis.OperationName);
                    }
                    catch
                    { }

                }
            }

        }
        protected virtual void JobOrderStockItems_StkPrice_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            JobOrderStockItems line = (JobOrderStockItems)e.Row;
            if (line.StkPrice != null)
            {
                line.LinePrice = CalcLinePrice(line.StkPrice, Convert.ToDecimal(line.Quantity));
            }

        }
        protected virtual void JobOrderStockItems_Quantity_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            JobOrderStockItems line = (JobOrderStockItems)e.Row;
            if (line.Quantity != null)
            {
                line.LinePrice = CalcLinePrice(line.StkPrice, Convert.ToDecimal(line.Quantity));
            }
        }
        protected virtual void JobOrderStockItems_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {

            JobOrderStockItems line = (JobOrderStockItems)e.Row;
            JobOrder order = jobOreder.Current;
            bool isUpdated = false;
            object x = order.Price;
            if ((line.LinePrice != null) && (line.LinePrice > 0))
            {
                order.Price += line.LinePrice;
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }


        }
        protected virtual void JobOrderStockItems_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            JobOrderStockItems newline = (JobOrderStockItems)e.Row;
            JobOrderStockItems oldline = (JobOrderStockItems)e.OldRow;
            JobOrder order = jobOreder.Current;
            bool isUpdated = false;
            if (!sender.ObjectsEqual<JobOrderStockItems.linePrice>(newline, oldline))
            {
                if (oldline.LinePrice != null)
                {
                    order.Price -= oldline.LinePrice;
                }
                if (newline.LinePrice != null)
                {
                    order.Price += newline.LinePrice;
                }
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }
        }
        protected virtual void JobOrderStockItems_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            JobOrderStockItems line = (JobOrderStockItems)e.Row;
            JobOrder order = jobOreder.Current;
            PXEntryStatus orderStatus = jobOreder.Cache.GetStatus(order);
            bool isDeleted = orderStatus == PXEntryStatus.InsertedDeleted || orderStatus == PXEntryStatus.Deleted;
            if (isDeleted == true)
            {
                return;
            }
            bool isUpdated = false;

            if (line.LinePrice != null)
            {
                order.Price -= line.LinePrice;
                isUpdated = true;
            }
            if (isUpdated == true)
            {
                jobOreder.Update(order);
            }

        }
        public virtual void JobOrderStockItems_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            //if (e.Row != null)
            //{
            //    JobOrderStockItems row = e.Row as JobOrderStockItems;
            //    if (e.Operation == PXDBOperation.Insert)
            //    {

            //        //check if exists before in JobOrderStockItems itself dismiss the whole operation
            //        PXSelectBase<JobOrderStockItems> jobOrderStockItems = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
            //        jobOrderStockItems.Cache.ClearQueryCache();
            //        PXResultset<JobOrderStockItems> jobOrderStockItemsResult = jobOrderStockItems.Select(jobOreder.Current.JobOrdrID, row.InventoryCode);
            //        //JobOrderStockItems jobOrderStockItemsResult = PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>.Select(this, jobOreder.Current.JobOrdrID, row.InventoryCode);


            //        if (jobOrderStockItemsResult.Count > 0)
            //        {
            //            e.Cancel = true;
            //            //throw new PXException("This Item Exists Before please Upadate the Existing if you want extra Qty add it to the existing item");
            //            sender.RaiseExceptionHandling<JobOrderStockItems.inventoryCode>(row, row.InventoryCode, new PXSetPropertyException("This Item Exists Before please Upadate the Existing if you want extra Qty add it to the existing item", PXErrorLevel.Error));
            //        }
            //        else
            //        {//if does not exist 
            //            ///check that the Qty must be greater than 0 then add it to the Balance Table 
            //            if (row.Quantity > 0)
            //            {
            //                this.ProviderInsert<Balance>(new PXDataFieldAssign("joborderID", row.JobOrdrID), new PXDataFieldAssign("inventoryID", row.InventoryCode), new PXDataFieldAssign("name", row.InventoryName), new PXDataFieldAssign("Qty", row.Quantity));
            //                //disable the Inventory Code Field
            //                //PXUIFieldAttribute.SetEnabled<JobOrderStockItems.inventoryCode>(sender, row, false);
            //            }
            //            else
            //            {    //if quantity zero or less stop and throw an exception
            //                e.Cancel = true;
            //                //throw new PXException("The Quantity Can not Be Zero or Less");
            //                sender.RaiseExceptionHandling<JobOrderStockItems.quantity>(row, row.Quantity, new PXSetPropertyException("The Quantity Can not Be Zero or Less", PXErrorLevel.Error));
            //            }
            //        }


            //    }
            //    else if (e.Operation == PXDBOperation.Update)
            //    {
            //        // //to get the old value you have to query the DB 
            //        // PXSelectBase<JobOrderStockItems> jobOrderStockItems = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>>>(this);
            //        // jobOrderStockItems.Cache.ClearQueryCache();
            //        // PXResultset<JobOrderStockItems> result = jobOrderStockItems.Select(row.JobOrdrID);
            //        // JobOrderStockItems jobOrderStockItemOldRow = (JobOrderStockItems)result;
            //        // float? qtyOldValue = jobOrderStockItemOldRow.Quantity;
            //        // float? qtyNewValue = row.Quantity;
            //        //if (qtyNewValue > qtyOldValue)
            //        // { //if increased
            //        //     //update the balance Qty with the old balance qty + (difference between qtyNewValue and qtyOldValue) 
            //        //     PXSelectBase<Balance> balances = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
            //        //     balances.Cache.ClearQueryCache();
            //        //     PXResultset<Balance> balancesResult = balances.Select(row.JobOrdrID, row.InventoryCode);
            //        //     Balance balanceRow = (Balance)balancesResult;
            //        //     float? balanceOldQty = balanceRow.Qty;
            //        //     float? difference = qtyNewValue - qtyOldValue;
            //        //     float? balanceNewQty = balanceOldQty + difference;

            //        //     this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", balanceNewQty), new PXDataFieldRestrict("JoborderID", row.JobOrdrID), new PXDataFieldRestrict("InventoryID", row.InventoryCode));
            //        // }
            //        // else if (qtyNewValue < qtyOldValue)
            //        // { // if decreased 
            //        //     //check the Balance of this item if zero through exception as the whole qty is already issued 
            //        //     //else check if after decresing the balance qty the balance qty will go negative or not "the alerady issued qty is bigger than the new value the user want to update"
            //        //     //then throw an exception and dismiss
            //        //     ///else update the balance Qty with the old balance qty - (difference between qtyOldValue and qtyNewValue) 
            //        //     PXSelectBase<Balance> balances = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
            //        //     balances.Cache.ClearQueryCache();
            //        //     PXResultset<Balance> balancesResult = balances.Select(row.JobOrdrID, row.InventoryCode);
            //        //     Balance balanceRow = (Balance)balancesResult;
            //        //     if (balanceRow.Qty == 0)
            //        //     {
            //        //         e.Cancel = true;
            //        //         //throw new PXException("The Whole Item is Already Issued");
            //        //         sender.RaiseExceptionHandling<JobOrderStockItems.quantity>(row, row.Quantity, new PXSetPropertyException("The Whole Item is Already Issued", PXErrorLevel.Error));
            //        //     }
            //        //     else
            //        //     {
            //        //         float? balanceOldQty = balanceRow.Qty;
            //        //         float? difference = qtyOldValue - qtyNewValue;
            //        //         float? balanceNewQty = balanceOldQty - difference;
            //        //         if (balanceNewQty < 0)
            //        //         {
            //        //             e.Cancel = true;
            //        //             //throw new PXException("the alerady issued qty is bigger than the new value the user want to update , this will give negative balance qty"); 
            //        //             sender.RaiseExceptionHandling<JobOrderStockItems.quantity>(row, row.Quantity, new PXSetPropertyException("the alerady issued qty is bigger than the new value the user want to update , this will give negative balance qty", PXErrorLevel.Error));
            //        //         }
            //        //         else
            //        //         {
            //        //             this.ProviderUpdate<Balance>(new PXDataFieldAssign("Qty", balanceNewQty), new PXDataFieldRestrict("JoborderID", row.JobOrdrID), new PXDataFieldRestrict("InventoryID", row.InventoryCode));
            //        //         }
            //        //     }


            //        //}
            //        //else
            //        //{ // if does not changed
            //        //    //do nothing
            //        //}
            //    }
            //    else if (e.Operation == PXDBOperation.Delete)
            //    {

            //        //    //can delete only if StockItem Qty == balance Qty that means nothing issued
            //        //    PXSelectBase<Balance> balances = new PXSelectReadonly<Balance, Where<Balance.joborderID, Equal<Required<Balance.joborderID>>, And<Balance.inventoryID, Equal<Required<Balance.inventoryID>>>>>(this);
            //        //    balances.Cache.ClearQueryCache();
            //        //    PXResultset<Balance> balancesResult = balances.Select(row.JobOrdrID, row.InventoryCode);
            //        //    Balance balanceRow = (Balance)balancesResult;

            //        //    //to get the old value you have to query the DB 
            //        //    PXSelectBase<JobOrderStockItems> jobOrderStockItems = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>, And<JobOrderStockItems.inventoryCode, Equal<Required<JobOrderStockItems.inventoryCode>>>>>(this);
            //        //    jobOrderStockItems.Cache.ClearQueryCache();
            //        //    PXResultset<JobOrderStockItems> jobOrderStockItemsResult = jobOrderStockItems.Select(row.JobOrdrID, row.InventoryCode);
            //        //    JobOrderStockItems jobOrderStockItemOldRow = (JobOrderStockItems)jobOrderStockItemsResult;

            //        //    if (jobOrderStockItemOldRow.Quantity != balanceRow.Qty)
            //        //    {
            //        //        e.Cancel = true;
            //        //        //throw new PXException("There is already issued items");
            //        //        sender.RaiseExceptionHandling<JobOrderStockItems.quantity>(row, row.Quantity, new PXSetPropertyException("There is already issued items", PXErrorLevel.Error));
            //        //    }
            //        //    else
            //        //    {
            //        //        //continue deletion
            //        //        //and delete the attached Balance Row
            //        //        this.ProviderDelete<Balance>(new PXDataFieldRestrict("joborderID", row.JobOrdrID), new PXDataFieldRestrict("InventoryID", row.InventoryCode));
            //        //    }
            //    }

            //}
        }

        #endregion

        #region Operation Events
        protected virtual void OperationJOB_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {

            this.Actions.PressSave();
            JobOrder order = jobOreder.Current;
            jobOreder.Update(order);
            this.Actions.PressCancel();
            JobOrder jobOrder = jobOreder.Current as JobOrder;
            if (jobOrder != null)
            {

                PXSelectBase<JobOrderNonStockItems> jobOrderBalances = new PXSelectReadonly<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>>>(this);
                jobOrderBalances.Cache.ClearQueryCache();
                PXResultset<JobOrderNonStockItems> jobOrderBalancesset = jobOrderBalances.Select(jobOrder.JobOrdrID);

                order.Price = 0;
                foreach (PXResult<JobOrderNonStockItems> record in jobOrderBalancesset)
                {
                    JobOrderNonStockItems row = (JobOrderNonStockItems)record;
                    order.Price += row.LinePrice;
                }

                PXSelectBase<JobOrderStockItems> jobOrderstk = new PXSelectReadonly<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>>>(this);
                jobOrderBalances.Cache.ClearQueryCache();
                PXResultset<JobOrderStockItems> jobOrderst = jobOrderstk.Select(jobOrder.JobOrdrID);

                foreach (PXResult<JobOrderStockItems> record in jobOrderst)
                {
                    JobOrderStockItems row2 = (JobOrderStockItems)record;
                    order.Price += row2.LinePrice;
                }

            }
            jobOreder.Update(order);
        }
        protected virtual void OperationJOB_operationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            OperationJOB row = e.Row as OperationJOB;
            PXSelectBase<Operation> invt = new PXSelectReadonly<Operation, Where<Operation.oPerationID, Equal<Required<Operation.oPerationID>>>>(this);
            invt.Cache.ClearQueryCache();
            Operation result = invt.Select(row.OperationID);
            sender.SetValue<OperationJOB.operationName>(row, result.OperationName);
        }
        #endregion

        #region JobOrder Events

        protected virtual void JobOrder_branchesss_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {

                JobOrder row = e.Row as JobOrder;
                PXSelectBase<SetUp> setups = new PXSelectReadonly<SetUp, Where<SetUp.branchCD, Equal<Required<SetUp.branchCD>>>>(this);
                setups.Cache.ClearQueryCache();
                SetUp setup = setups.Select(row.Branchesss);
                AutoNumSetup.Current = setup;

            }
        }
        protected virtual void JobOrder_itemsID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {

            if (e.Row != null)
            {
                //JobOrder ord = this.jobOreder.Current;
                JobOrder jobOrder = e.Row as JobOrder;
                int? classID = jobOrder.ClassID;
                int? itemID = (int?)e.NewValue;

                PXSelectBase<JobOrder> jobOrd = new PXSelectReadonly<JobOrder, Where<JobOrder.classID, Equal<Required<JobOrder.classID>>, And<JobOrder.itemsID, Equal<Required<JobOrder.itemsID>>, And<JobOrder.Status, NotEqual<Required<JobOrder.Status>>>>>>(this);//,And<JobOrder.customer,Equal<Current<JobOrder.customer>>>,And<JobOrder.itemsID,Equal<Current<JobOrder.itemsID>>>>>(this);
                jobOrd.Cache.ClearQueryCache();
                PXResultset<JobOrder> result1 = jobOrd.Select(classID, itemID, JobOrderStatus.Finished);

                if (result1.Count > 0)
                {
                    throw new PXSetPropertyException("you have the same order on this chassis");
                }
            }


        }
        protected virtual void JobOrder_ParentNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            if (e.Row != null)
            {
                JobOrder jobOrder = e.Row as JobOrder;

                if (!String.IsNullOrEmpty(jobOrder.ItemsID + ""))
                {
                    int? itemID = (int?)jobOrder.ItemsID;
                    PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>>(this);
                    ChassisBase.Cache.ClearQueryCache();
                    Items Chassis = ChassisBase.Select(itemID);
                    //sender.SetValue<JobOrder.customer>(jobOrder, Chassis.Customer);
                    sender.SetValue<JobOrder.licensePlate>(jobOrder, Chassis.LincensePlat);
                    sender.SetValue<JobOrder.brandName>(jobOrder, Chassis.BrandID);
                    sender.SetValue<JobOrder.modelName>(jobOrder, Chassis.ModelID);
                }
                if (!String.IsNullOrEmpty(jobOrder.Customer + ""))
                {
                    string CustomerID = (string)jobOrder.Customer;
                    //PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);

                    PXSelectBase<Contact> cust = new PXSelectJoin<Contact, InnerJoin<Customer, On<Contact.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                    cust.Cache.ClearQueryCache();
                    Contact co = cust.Select(CustomerID);
                    sender.SetValue<JobOrder.email>(jobOrder, co.EMail);
                    //--------------------------------
                    PXSelectBase<Address> add = new PXSelectJoin<Address, LeftJoin<Customer, On<Address.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                    add.Cache.ClearQueryCache();
                    Address addre = add.Select(CustomerID);
                    sender.SetValue<JobOrder.address>(jobOrder, addre.AddressLine1);

                }
            }

        }
        protected virtual void JobOrder_classID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {

            if (e.Row != null)
            {
                JobOrder jobOrder = e.Row as JobOrder;

                if (!String.IsNullOrEmpty(jobOrder.ItemsID + ""))
                {

                    int? classID = jobOrder.ClassID;
                    int? itemID = (int?)e.NewValue;

                    PXSelectBase<JobOrder> jobOrd = new PXSelectReadonly<JobOrder, Where<JobOrder.classID, Equal<Required<JobOrder.classID>>, And<JobOrder.itemsID, Equal<Required<JobOrder.itemsID>>, And<JobOrder.Status, NotEqual<Required<JobOrder.Status>>>>>>(this);//,And<JobOrder.customer,Equal<Current<JobOrder.customer>>>,And<JobOrder.itemsID,Equal<Current<JobOrder.itemsID>>>>>(this);
                    jobOrd.Cache.ClearQueryCache();
                    PXResultset<JobOrder> result1 = jobOrd.Select(classID, itemID, JobOrderStatus.Finished);

                    if (result1.Count > 0)
                    {
                        throw new PXSetPropertyException("you have the same order on this chassis");
                    }

                }
            }


        }
        protected void JobOrder_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {

            JobOrder jobOrder = e.Row as JobOrder;
            //populating unbounded dac fields
            if (e.Row != null)
            {
                if (jobOrder.JobOrdrID.Contains("<NEW>"))
                {
                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);
                    nonStockItems.AllowDelete = true;
                    items.AllowDelete = true;
                    workers.AllowDelete = true;
                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);
                    AddOpenTime.SetEnabled(true);
                }

                if (!String.IsNullOrEmpty(jobOrder.ItemsID + ""))
                {
                    int? itemID = (int?)jobOrder.ItemsID;
                    if (itemID != null)
                    {
                        PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>>(this);
                        ChassisBase.Cache.ClearQueryCache();
                        Items Chassis = ChassisBase.Select(itemID);
                        //sender.SetValue<JobOrder.customer>(jobOrder, Chassis.Customer);
                        sender.SetValue<JobOrder.licensePlate>(jobOrder, Chassis.LincensePlat);
                        sender.SetValue<JobOrder.brandName>(jobOrder, Chassis.BrandID);
                        sender.SetValue<JobOrder.modelName>(jobOrder, Chassis.ModelID);
                    }
                    if (!String.IsNullOrEmpty(jobOrder.Customer + ""))
                    {
                        string CustomerID = (string)jobOrder.Customer;
                        //PXSelectBase<Customer> Jo = new PXSelectJoin<Customer, InnerJoin<JobOrder, On<JobOrder.customer, Equal<Customer.acctCD>>>, Where<JobOrder.jobOrdrID, Equal<Required<JobOrder.jobOrdrID>>>>(this);

                        PXSelectBase<Contact> cust = new PXSelectJoin<Contact, InnerJoin<Customer, On<Contact.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                        cust.Cache.ClearQueryCache();
                        Contact co = cust.Select(CustomerID);
                        sender.SetValue<JobOrder.email>(jobOrder, co.EMail);
                        //--------------------------------
                        PXSelectBase<Address> add = new PXSelectJoin<Address, LeftJoin<Customer, On<Address.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                        add.Cache.ClearQueryCache();
                        Address addre = add.Select(CustomerID);
                        sender.SetValue<JobOrder.address>(jobOrder, addre.AddressLine1);

                    }
                }
            }

            //enable and disable validations

            if (e.Row != null && !jobOrder.JobOrdrID.Contains("<NEW>"))
            {

                if (jobOrder.status == JobOrderStatus.Open || jobOrder.status == JobOrderStatus.ReOpen)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);
                    nonStockItems.AllowDelete = true;
                    items.AllowDelete = true;
                    workers.AllowDelete = true;
                    StartJobOrder.SetEnabled(true);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);
                    AddOpenTime.SetEnabled(true);

                }
                else if (jobOrder.status == JobOrderStatus.Started)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);
                    nonStockItems.AllowDelete = true;
                    items.AllowDelete = true;
                    workers.AllowDelete = true;

                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(true);
                    CompleteJobOrder.SetEnabled(true);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);
                    AddOpenTime.SetEnabled(true);

                }
                else if (jobOrder.status == JobOrderStatus.Completed)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);
                    nonStockItems.AllowDelete = false;
                    items.AllowDelete = false;
                    workers.AllowDelete = false;

                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(true);
                    PrepareInvoice.SetEnabled(true);
                    ReOpen.SetEnabled(false);
                    AddOpenTime.SetEnabled(false);
                    ExitPermision.SetVisible(true);

                }
                else if (jobOrder.status == JobOrderStatus.Finished)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, false);
                    
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);
                    nonStockItems.AllowDelete = false;
                    items.AllowDelete = false;
                    workers.AllowDelete = false;


                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    AddOpenTime.SetEnabled(false);
                    ReOpen.SetEnabled(true);

                }
                else {; }
            }
        }
        protected void JobOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {

            JobOrder jobOrder = e.Row as JobOrder;

            if ( !(jobOrder is null) && jobOrder.JobOrdrID.Contains("<NEW>"))
            {
                PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);

                StartJobOrder.SetEnabled(false);
                StopJobOrder.SetEnabled(false);
                CompleteJobOrder.SetEnabled(false);
                Restart.SetEnabled(false);
                PrepareInvoice.SetEnabled(false);
                ReOpen.SetEnabled(false);
            }

            //populating unbounded dac fields
            if (e.Row != null)
            {

                if (!String.IsNullOrEmpty(jobOrder.ItemsID + ""))
                {
                    int? itemID = (int?)jobOrder.ItemsID;
                    if (itemID != null)
                    {
                        try
                        {
                            PXSelectBase<Items> ChassisBase = new PXSelectReadonly<Items, Where<Items.itemsID, Equal<Required<Items.itemsID>>>>(this);
                            ChassisBase.Cache.ClearQueryCache();
                            //Items Chassis = ChassisBase.Select(itemID);
                            Items Chassis = chas.Select(itemID);
                            //sender.SetValue<JobOrder.customer>(jobOrder, Chassis.Customer);

                            sender.SetValue<JobOrder.licensePlate>(jobOrder, Chassis.LincensePlat);
                            sender.SetValue<JobOrder.brandName>(jobOrder, Chassis.BrandID);
                            sender.SetValue<JobOrder.modelName>(jobOrder, Chassis.ModelID);
                        }
                        catch
                        { }
                    }
                    if (!String.IsNullOrEmpty(jobOrder.Customer + ""))
                    {
                        string CustomerID = (string)jobOrder.Customer;
                        PXSelectBase<Contact> cust = new PXSelectJoin<Contact, InnerJoin<Customer, On<Contact.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                        cust.Cache.ClearQueryCache();
                        Contact co = cust.Select(CustomerID);
                        sender.SetValue<JobOrder.email>(jobOrder, co.EMail);
                        //--------------------------------
                        PXSelectBase<Address> add = new PXSelectJoin<Address, LeftJoin<Customer, On<Address.bAccountID, Equal<Customer.bAccountID>>>, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>(this);
                        add.Cache.ClearQueryCache();
                        Address addre = add.Select(CustomerID);
                        sender.SetValue<JobOrder.address>(jobOrder, addre.AddressLine1);

                    }
                }
            }

            //enable and disable validations

            if (e.Row != null && !jobOrder.JobOrdrID.Contains("<NEW>"))
            {

                if (jobOrder.status == JobOrderStatus.Open || jobOrder.status == JobOrderStatus.ReOpen)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);

                    StartJobOrder.SetEnabled(true);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);

                }
                else if (jobOrder.status == JobOrderStatus.Started)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, true);

                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(true);
                    CompleteJobOrder.SetEnabled(true);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(false);

                }
                else if (jobOrder.status == JobOrderStatus.Completed)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled<JobOrder.jobOrdrID>(sender, null, true);
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);

                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(true);
                    PrepareInvoice.SetEnabled(true);
                    ReOpen.SetEnabled(false);

                }
                else if (jobOrder.status == JobOrderStatus.Finished)
                {

                    PXUIFieldAttribute.SetEnabled(jobOreder.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled<JobOrder.jobOrdrID>(sender,null,true);                   
                    PXUIFieldAttribute.SetEnabled(operationJob.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(openTime.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(workers.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(items.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(nonStockItems.Cache, null, false);

                    StartJobOrder.SetEnabled(false);
                    StopJobOrder.SetEnabled(false);
                    CompleteJobOrder.SetEnabled(false);
                    Restart.SetEnabled(false);
                    PrepareInvoice.SetEnabled(false);
                    ReOpen.SetEnabled(true);

                }
                else {; }
            }
        }
        
        public virtual void JobOrder_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null)
            {
                if (e.Operation == PXDBOperation.Insert)
                {
                    if (this.jobOreder.Current.JobOrdrID == "<NEW>")
                    {
                        SetUp result = PXSelect<SetUp, Where<SetUp.branchCD, Equal<Required<JobOrder.branchesss>>>>.Select(this, this.jobOreder.Current.Branchesss);
                        if (result != null)
                        {
                            string lastNumber = result.ReceiptLastRefNbr;
                            char[] symbols = lastNumber.ToCharArray();
                            for (int i = symbols.Length - 1; i >= 0; i--)
                            {
                                if (!char.IsDigit(symbols[i]))
                                    break;
                                if (symbols[i] < '9')
                                {
                                    symbols[i]++;
                                    break;
                                }
                                symbols[i] = '0';
                            }

                            this.jobOreder.Current.JobOrdrID = new string(symbols);
                            this.ProviderUpdate<SetUp>(new PXDataFieldAssign("receiptLastRefNbr", this.jobOreder.Current.JobOrdrID), new PXDataFieldRestrict("branchCD", this.jobOreder.Current.Branchesss));
                        }
                        else
                        {
                            e.Cancel = true;
                            throw new PXException("Please Define auto Numbering For the Selected Branch !");
                        }

                    }
                    else
                    {
                    }

                }
            }
        }
        protected virtual void JobOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //JobOrder newline = (JobOrder)e.Row;
            //JobOrder oldline = (JobOrder)e.OldRow;
            //JobOrder order = jobOreder.Current;
            //bool isUpdated = false;
            //if (!sender.ObjectsEqual<JobOrderStockItems.linePrice>(newline, oldline))
            //{
            //    if (oldline.LinePrice != null)
            //    {
            //        order.Price -= oldline.LinePrice;
            //    }
            //    if (newline.LinePrice != null)
            //    {
            //        order.Price += newline.LinePrice;
            //    }
            //    isUpdated = true;
            //}
            //if (isUpdated == true)
            //{
            //    jobOreder.Update(order);
            //}
        }
        protected virtual void JobOrder_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {

            JobOrder row = jobOreder.Current;
            PXSelectBase<JobOrderNonStockItems> itm = new PXSelect<JobOrderNonStockItems, Where<JobOrderNonStockItems.jobOrdrID, Equal<Required<JobOrderNonStockItems.jobOrdrID>>>>(this);
            PXResultset<JobOrderNonStockItems> result = itm.Select(row.JobOrdrID);

            if (result.Count > 0)
            {
                throw new PXException("It is not Allow to delete this joborder coz there is some servise items related");
                //e.Cancel = true;
                //return;
            }
            else
            {
                PXSelectBase<JobOrderStockItems> itm2 = new PXSelect<JobOrderStockItems, Where<JobOrderStockItems.jobOrdrID, Equal<Required<JobOrderStockItems.jobOrdrID>>>>(this);
                PXResultset<JobOrderStockItems> result2 = itm2.Select(row.JobOrdrID);
                if (result2.Count > 0)
                {
                    throw new PXException("It is not Allow to delete this joborder coz there is some Spare Parts related");
                    //e.Cancel = true;
                    //return;
                }
                else
                {
                    PXSelectBase<INTran> itm3 = new PXSelect<INTran, Where<INTranExt.usrOrdID, Equal<Required<INTranExt.usrOrdID>>>>(this);
                    PXResultset<INTran> result3 = itm3.Select(row.JobOrdrID);
                    if (result3.Count > 0)
                    {
                        throw new PXException("It is not Allow to delete this joborder coz there is an Issues related");
                        //e.Cancel = true;
                        //return;
                    }
                    else
                    {
                        PXSelectBase<ARInvoice> itm4 = new PXSelect<ARInvoice, Where<ARRegisterExt.usrJobOrdID, Equal<Required<ARRegisterExt.usrJobOrdID>>>>(this);
                        PXResultset<ARInvoice> result4 = itm4.Select(row.JobOrdrID);
                        if (result4.Count > 0)
                        {
                            throw new PXException("It is not Allow to delete this joborder coz there is an Invoice related");
                            //e.Cancel = true;
                            //return;
                        }
                    }
                }
            }

        }

        #endregion

        #region Open Time Events

        protected void OPenTime_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {

            OPenTime row = e.Row as OPenTime;
            if (row != null && !String.IsNullOrEmpty(row.Variation))
            {
                if (double.Parse(row.Variation) < 0)
                {
                    sender.RaiseExceptionHandling<OPenTime.variation>(row, row.Variation, new PXSetPropertyException("This job take more than specified standard time !", PXErrorLevel.Warning));
                }
            }

        }
        //this is the start date
        protected virtual void OPenTime_StartTime_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            DateTime date = DateTime.Now;
            this.openTime.Current.StartTime = date;
        }
        //this is the start Time
        protected virtual void OPenTime_StarTime_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            DateTime date = DateTime.Now;
            String hour;
            String min;

            if (date.Hour < 10)
            {
                hour = "0" + date.Hour;
            }
            else
            {
                hour = date.Hour.ToString();
            }

            if (date.Minute < 10)
            {
                min = "0" + date.Minute;
            }
            else
            {
                min = date.Minute.ToString();
            }

            if (date.Hour < 12)
            {
                if (date.Hour < 10)
                    this.openTime.Current.StarTime = "0" + hour + ":" + min + ":00 AM";
                else
                    this.openTime.Current.StarTime = hour + ":" + min + ":00 AM";
            }
            else
            {
                if ((date.Hour - 12) < 10)
                    this.openTime.Current.StarTime = "0" + (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                else
                    this.openTime.Current.StarTime = (date.Hour - 12).ToString() + ":" + min + ":00 PM";
            }


        }
        protected virtual void OPenTime_clse_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row != null)
            {
                OPenTime row = e.Row as OPenTime;
                if (row.Clse == true)
                {
                    DateTime date = DateTime.Now;
                    this.openTime.Current.EndeTime = date;

                    String hour;
                    String min;

                    if (date.Hour < 10)
                    {
                        hour = "0" + date.Hour;
                    }
                    else
                    {
                        hour = date.Hour.ToString();
                    }

                    if (date.Minute < 10)
                    {
                        min = "0" + date.Minute;
                    }
                    else
                    {
                        min = date.Minute.ToString();
                    }

                    if (date.Hour < 12)
                    {
                        if (date.Hour < 10)
                            this.openTime.Current.EndTime = "0" + hour + ":" + min + ":00 AM";
                        else
                            this.openTime.Current.EndTime = hour + ":" + min + ":00 AM";

                    }
                    else
                    {
                        if ((date.Hour - 12) < 10)
                            this.openTime.Current.EndTime = "0" + (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                        else
                            this.openTime.Current.EndTime = (date.Hour - 12).ToString() + ":" + min + ":00 PM";
                    }



                    DateTime? startDate = openTime.Current.StartTime;
                    DateTime? endDate = openTime.Current.EndeTime;


                    String startTime = openTime.Current.StarTime;
                    String startTimeSuffix = startTime.Substring(startTime.Length - 2, 2);
                    String startTimeWithoutSuffix = startTime.Substring(0, startTime.Length - 3); // space + AM

                    DateTime startTimetime = DateTime.ParseExact(startTimeWithoutSuffix, "HH:mm:ss", CultureInfo.InvariantCulture);
                    int startHour = startTimetime.Hour;
                    if (startTimeSuffix.Equals("PM"))
                        startHour += 12;
                    int startMin = startTimetime.Minute;


                    String endTime = openTime.Current.EndTime;
                    String endTimeSuffix = endTime.Substring(endTime.Length - 2, 2);
                    String endTimeWithoutSuffix = endTime.Substring(0, endTime.Length - 3); // space + AM


                    DateTime endTimetime = DateTime.ParseExact(endTimeWithoutSuffix, "HH:mm:ss", CultureInfo.InvariantCulture);
                    int endHour = endTimetime.Hour;
                    if (endTimeSuffix.Equals("PM"))
                        endHour += 12;
                    int endtMin = endTimetime.Minute;


                    TimeSpan? daysDiff = endDate - startDate.Value;

                    int hourDiff = endHour - startHour;
                    int minDiff = endHour - startMin;

                    double hoursFromMin = (minDiff) / 60.0;

                    double total = hourDiff + hoursFromMin;
                    openTime.Current.Dauration = ((daysDiff.Value.Days * 24) + (total)).ToString();
                    openTime.Current.ManualDauration = ((daysDiff.Value.Days * 24) + (total)).ToString();


                    this.openTime.Current.Status = OpenTimeStatus.Closed;
                }
            }
        }
        protected virtual void OPenTime_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {


            //throw new PXException("It is not Allow to delete Any Open Time");

        }
        #endregion
        protected virtual void Workers_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {

            JobOrder row = jobOreder.Current;
            PXSelectBase<OPenTime> itm4 = new PXSelect<OPenTime, Where<OPenTime.jobOrderID, Equal<Required<OPenTime.jobOrderID>>>>(this);
            PXResultset<OPenTime> result4 = itm4.Select(row.JobOrdrID);
            if (result4.Count > 0)
            {
                throw new PXException("It is not Allow to delete this Technician coz there is an Open Time related");
            }

        }
        /* protected virtual void JobOrderNonStockItems_inventoryCode1_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
         {

             if (e.Row != null)
             {
                 JobOrderNonStockItems row = e.Row as JobOrderNonStockItems;
                 try
                 {
                     #region Update Price Field
                     //decimal? Total = 15; 
                     //PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.expirationDate, IsNull, And<ARSalesPrice.priceType, Equal<PriceTypes.basePrice>>>>>(this);
                     //salesPrices.Cache.ClearQueryCache();
                     //ARSalesPrice resultSalesPrices = salesPrices.Select(row.InventoryCode);


                     //PXResultset<ARSalesPrice> rss = PXSelect<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                     //And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>>>>.Select(this.Base, this.Base.Document.Current.OrderType, this.Base.Document.Current.OrderNbr);
                     string x = "001";
                     //PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>,
                     //And<ARSalesPrice.expirationDate, IsNull, And<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>
                     //, And<ARSalesPrice.priceCode, Equal<Required<ARSalesPrice.priceCode>>>>>>>(this);
                     PXResultset<ARSalesPrice> salesPrices = PXSelect<ARSalesPrice, Where<ARSalesPrice.inventoryID, 
                         Equal<Required<ARSalesPrice.inventoryID>>,
                     And<ARSalesPrice.expirationDate, IsNull, And<ARSalesPrice.priceType, Equal<PriceTypes.customerPriceClass>
                     , And<ARSalesPrice.custPriceClassID, Equal<Required<ARSalesPrice.custPriceClassID>>>>>>>.Select(this.Base, 0, 0);

                     PXResultset<SOLine> set = PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOLine.orderType>>, And<SOLine.orderNbr, Equal<Required<SOLine.orderNbr>>, And<SOLine.lineNbr, NotEqual<Required<SOLine.lineNbr>>>>>>.Select(this.Base, this.Base.Document.Current.OrderType, this.Base.Document.Current.OrderNbr, line.LineNbr);

                     salesPrices.Cache.ClearQueryCache();
                     ARSalesPrice resultSalesPrices = salesPrices.Select(row.InventoryCode,x);
                     if (resultSalesPrices != null)
                     {
                         sender.SetValue<JobOrderNonStockItems.nonStkPrice>(row, resultSalesPrices.SalesPrice);
                     }
                     else
                     {
                         sender.RaiseExceptionHandling<JobOrderNonStockItems.inventoryCode>(row, 0.0, new PXSetPropertyException("There is no Sales Prices For this Item!", PXErrorLevel.Warning));
                     }
                     //SOOrder order = this.Base.Document.Current;
                     //SOOrderExt orderExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(order);
                     ////PXSelectBase<Customer> res = new PXSelectReadonly<Customer,Where<Customer.bAccountID,Equal<Required<Customer.bAccountID>>>>(this);
                     ////res.Cache.ClearQueryCache();
                     ////Customer cus = res.Select(row.CustomerID);
                     //PXSelectBase<BAccount> cust = new PXSelectReadonly<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>(this);
                     //cust.Cache.ClearQueryCache();
                     //BAccount custt = cust.Select(order.CustomerID);
                     ////BAccountExtension custtExt = PXCache<BAccount>.GetExtension<BAccountExtension>(custt);
                     ////string pricecode = custt.UsrOriginalPriceClass;
                     //sender.SetValue<SOOrder.orderDesc>(row, custtExt.UsrOriginalPriceClass);
                     ////ARInvoice header = new ARInvoice();
                     //BAccountExt custtExt = PXCache<BAccount>.GetExtension<BAccountExt>(custt);
                     //string pricecode = custt.UsrOriginalPriceClass;
                     //ARSalesPrice resultSalesPrices = salesPrices.Select(row.InventoryCode);

                     //PXSelectBase<ARSalesPrice> salesPrices = new PXSelectReadonly<ARSalesPrice, Where<ARSalesPrice.inventoryID, Equal<Required<ARSalesPrice.inventoryID>>, And<ARSalesPrice.expirationDate, IsNull,
                     //    And<ARSalesPrice.priceCode, Equal<PriceTypes.customerPriceClass>>>>>(this);



                     if (resultSalesPrices != null)
                     {
                         sender.SetValue<JobOrderNonStockItems.nonStkPrice>(row, resultSalesPrices.SalesPrice);
                     }
                     else
                     {
                         throw new PXException(x.ToString());
                         sender.RaiseExceptionHandling<JobOrderNonStockItems.inventoryCode>(row, 0.0, new PXSetPropertyException("There is no Sales Prices For this Item!", PXErrorLevel.Warning));
                     }
                 }
                 catch
                 {
                     //throw new PXException("Update Price");
                 }
                     #endregion


             }
         }*/

    }
}

//#region New Events (Fields and Row) attached to the correct Design of JobOrderStockItems and specifically quantity


//public PXAction<PX.Objects.SO.SOOrder> PrintDeliveryNote;

//[PXButton(CommitChanges = true)]
//[PXUIField(DisplayName = "Print Delivery Note")]
//protected void printDeliveryNote()
//{
//    SOOrder order = this.Base.Document.Current;
//    if (order.OrderNbr != " <NEW>")
//    {
//        Dictionary<string, string> parameters = new Dictionary<string, string>();
//        parameters["OrderType"] = order.OrderType;
//        parameters["RefNbr"] = order.OrderNbr;
//        throw new PXReportRequiredException(parameters, "NO604000", PXBaseRedirectException.WindowMode.New, null);
//    }
//    else
//    {
//        throw new PXException(" No Data Found To print");
//    }
//}