using MyMaintaince;
using Nour20230821VTaxFieldsSolveError;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.CN.Subcontracts.PO.DAC.PurchaseOrderTypeFilter;

namespace Nour20231012VSolveUSDNew
{
    public class InspectionFormEntryNew : PXGraph<InspectionFormEntryNew, InspectionFormMaster>
    {

        #region All Views

        public PXSelect<InspectionFormMaster> inspectionFormView;
        public PXSelect<JobOrder, Where<JobOrder.inspectionNbr, Equal<Required<JobOrder.inspectionNbr>>>> inspecJob;
        //public PXSelect<InspectionJobOrder, Where<InspectionJobOrder.inspectionNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> inspJobOrder;
        public PXSelectJoin<Customer, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Required<ItemCustomers.itemsID>>>> customer;
        public PXSetup<SetupForm> AutoNumberSetup;

        #region Tab Views
        public PXSelect<InspectionCarOutside, Where<InspectionCarOutside.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> carOutsideView;
        public PXSelect<InspectionMotor, Where<InspectionMotor.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> motorView;
        public PXSelect<InspectionRoadTest, Where<InspectionRoadTest.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> roadTestView;
        public PXSelect<InspectionSuspensionSystem, Where<InspectionSuspensionSystem.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> suspensionSysView;
        public PXSelect<InspectionMalfunctionCheck, Where<InspectionMalfunctionCheck.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> malfunCheckView;
        #endregion

        #endregion

        #region Views oreginal

        //public PXSelect<InspectionFormInq> inspectionForm;
        //public PXSelect<InspectionFormInq, Where<InspectionFormInq.inspectionFormNbr, Equal<Current<InspectionFormInq.inspectionFormNbr>>>> currentInspectionForm;
        //public PXSelectJoin<Customer, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Required<ItemCustomers.itemsID>>>> customer;
        //public PXSetup<SetupForm> AutoNumberSetup;

        //public PXSelect<JobOrder, Where/*<JobOrder.inspectionNbr, Equal<Required<JobOrder.inspectionNbr>>>> inspecJob;*/
        #endregion




        #region constructor
        public InspectionFormEntryNew()
        {
            SetupForm setup = AutoNumberSetup.Current;
        }
        #endregion

        #region Resetting the customer when changing the vehicle

        //protected virtual void _(Events.RowPersisted<InspectionFormInq> e)
        //{
        //    var row = e.Row;
        //    if(row.Status=="J" && row.JobOrderID != null)
        //    {

        //        throw new PXRedirectRequiredException(graph, true, null);
        //    }
        //}
        protected void InspectionFormInq_Vehicle_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            var row = (InspectionFormMaster)e.Row;

            var vehicleCusts = customer.Select(row.Vehicle);

            if (vehicleCusts.Count > 1)
            {
                row.Customer = null;
            }
            else if (vehicleCusts.Count == 1)
            {
                foreach (Customer cust in vehicleCusts)
                {

                    row.Customer = cust.AcctCD;
                }
            }


        }

        #endregion



        #region Event Handlers
        protected void InspectionFormMaster_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (InspectionFormMaster)e.Row;
            if (row.InspectionType == "F")
            {
                PXUIFieldAttribute.SetVisible<InspectionFormMaster.inspectionClass>(cache, row, false);

            }
            else
            {
                PXUIFieldAttribute.SetVisible<InspectionFormMaster.inspectionClass>(cache, row, true);
            }


            malfunCheckView.Cache.AllowSelect = true;
            carOutsideView.Cache.AllowSelect = true;
            roadTestView.Cache.AllowSelect = true;
            suspensionSysView.Cache.AllowSelect = true;
            motorView.Cache.AllowSelect = true;



            //    //----------------------Tab-----------------

            //    if (row.InspectionClass != "MC" && row.InspectionType != "F")
            //    {
            //        malfunCheckView.Cache.AllowSelect = false;
            //    }

            //    if (row.InspectionClass != "CO" && row.InspectionType != "F")
            //    { 
            //        carOutsideView.Cache.AllowSelect = false;  
            //    }

            //    if (row.InspectionClass != "RT" && row.InspectionType != "F")
            //    {
            //        carOutsideView.Cache.AllowSelect = false;
            //    }

            //    if (row.InspectionClass != "SS" && row.InspectionType != "F")
            //    {
            //        carOutsideView.Cache.AllowSelect = false;
            //    }

            //    if (row.InspectionClass != "M" && row.InspectionType != "F")
            //    {
            //        carOutsideView.Cache.AllowSelect = false;
            //    }

            //    inspectionFormView.View.RequestRefresh();
            //    malfunCheckView.View.RequestRefresh();
            //    carOutsideView.View.RequestRefresh();
            //    roadTestView.View.RequestRefresh();
            //    suspensionSysView.View.RequestRefresh();
            //    motorView.View.RequestRefresh();
            //}

            #endregion

            //// ------------
            ////DataControls["CstPXDropDown265"].Value == MC  
            ////DataControls["CstPXDropDown265"].Value != M  
            //// ------------




            #region Buttons

            //#region Convert To Job Order

            //public PXAction<InspectionFormMaster> ConvertToJobOrder;
            //[PXUIField(DisplayName = "Convert To Job Order", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
            //[PXButton(CommitChanges = true)]
            //protected virtual IEnumerable convertToJobOrder(PXAdapter adapter)
            //{

            //    graph = PXGraph.CreateInstance<JobOrderMaint>();
            //    InspectionFormMaster form = (InspectionFormMaster)inspectionFormView.Cache.Current;
            //    if (!(form.InspectionFormNbr is null) && form.InspectionFormNbr != " <NEW>")
            //    {
            //        JobOrder job = new JobOrder();

            //        job.ItemsID = form.Vehicle;

            //        job.InspectionNbr = form.InspectionFormNbr;

            //        job.Customer = form.Customer;
            //        job.AssignedToDateTime = form.Date;
            //        job.JobOrdrID = "<NEW>";
            //        job.Branchesss = form.Branches;
            //        job.BranchID = Accessinfo.BranchID;
            //        job.KM = form.KM;

            //        graph.jobOreder.Insert(job);
            //        graph.Actions.PressSave();

            //        form.JobOrderID = graph.jobOreder.Current.JobOrdrID;


            //    }
            //    else
            //    {
            //        throw new PXException("Please Save The Form First.");
            //    }

            //    return adapter.Get();

            //}

            //#endregion

            //#region Cancel Form

            //public PXAction<InspectionFormMaster> CancelForm;
            //[PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
            //[PXButton(CommitChanges = true)]
            //public virtual IEnumerable cancelForm(PXAdapter adapter)
            //{

            //    InspectionFormMaster form = (InspectionFormMaster)inspectionFormView.Cache.Current;

            //    if (form.InspectionFormNbr is null || form.InspectionFormNbr == " <NEW>")
            //        throw new PXException("Cant Cancel Unsaved Form.");
            //    return adapter.Get();
            //}



            //#endregion

            //#region Open Report

            //public PXAction<InspectionFormMaster> OpenReport;
            //private JobOrderMaint graph;

            //[PXUIField(DisplayName = "Print Inspection Form")]
            //[PXButton(CommitChanges = true)]
            //protected void openReport()
            //{
            //    InspectionFormMaster currentInspectionForm = this.inspectionFormView.Current;

            //    if (currentInspectionForm != null)
            //    {
            //        if (currentInspectionForm.InspectionFormNbr != null)
            //        {
            //            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //            parameters["IN"] = currentInspectionForm.InspectionFormNbr;
            //            throw new PXReportRequiredException(parameters, "NM000001", PXBaseRedirectException.WindowMode.New, null);
            //        }
            //        else
            //        {
            //            throw new PXException("There is no data in the form to show");
            //        }
            //    }
            //    else
            //    {
            //        throw new PXException("There is no data in the form to show");
            //    }
            //}

            //#endregion

            #endregion







        }
    }
}