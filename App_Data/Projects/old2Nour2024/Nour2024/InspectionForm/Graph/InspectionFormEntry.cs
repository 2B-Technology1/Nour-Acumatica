using MyMaintaince;
using Nour20230821VTaxFieldsSolveError;
using Nour20231012VSolveUSDNew;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nour20220913V1
{
    public class InspectionFormEntry : PXGraph<InspectionFormEntry, InspectionFormInq>
    {
        JobOrderMaint graph;

        #region Views

        public PXSelect<InspectionFormInq> inspectionForm;
        public PXSelect<InspectionFormInq, Where<InspectionFormInq.inspectionFormNbr, Equal<Current<InspectionFormInq.inspectionFormNbr>>>> currentInspectionForm;
        public PXSelect<InspectionFormInq, Where<InspectionFormInq.inspectionFormNbr, Equal<Current<InspectionFormInq.inspectionFormNbr>>>> carOutside;
        public PXSelectJoin<Customer, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Required<ItemCustomers.itemsID>>>> customer;
        public PXSetup<SetupForm> AutoNumberSetup;

        public PXSelect<JobOrder, Where<JobOrder.inspectionNbr, Equal<Required<JobOrder.inspectionNbr>>>> inspecJob;
        #endregion


        //public PXSelect<InspectionMalfunctionCheck, Where<InspectionMalfunctionCheck.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> malfunCheckView;
        //public PXSelect<InspectionCarOutside, Where<InspectionCarOutside.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> carOutsideView;
        //public PXSelect<InspectionRoadTest, Where<InspectionRoadTest.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> roadTestView;
        //public PXSelect<InspectionSuspensionSystem, Where<InspectionSuspensionSystem.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> suspensionSysView;
        //public PXSelect<InspectionMotor, Where<InspectionMotor.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> motorView;

        #region constructor
        public InspectionFormEntry()
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

            var row = (InspectionFormInq)e.Row;

            var vehicleCusts = customer.Select(row.Vehicle);

            if(vehicleCusts.Count > 1)
            {
                row.Customer = null;
            }else if (vehicleCusts.Count == 1)
            {
                foreach(Customer cust in vehicleCusts)
                {

                    row.Customer = cust.AcctCD;
                }
            }


        }

        #endregion

        //-------------------------Arafa

        #region Event Handlers


        public void InspectionFormInq_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (InspectionFormInq)e.Row;


            //var rowCurrent = currentInspectionForm.Current;
            //currentInspectionForm.View.RequestRefresh();
            //var testCache = currentInspectionForm.Cache;


            if (row.InspectionType == "F")
            {
                PXUIFieldAttribute.SetVisible<InspectionFormInq.inspectionClass>(cache, row, false);

            }
            else
            {
                PXUIFieldAttribute.SetVisible<InspectionFormInq.inspectionClass>(cache, row, true);
            }


            //----------------------Tab-----------------

            if (row.InspectionClass == "MC")
            {

                //currentInspectionForm.AllowSelect = false;
                //currentInspectionForm.Cache.AllowSelect = false;
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.carControlSysTest>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.carControlSysTest>(testCache, rowCurrent, PXUIVisibility.Invisible);
            }
            else if (row.InspectionClass == "CO")
            {

                carOutside.AllowSelect = false;
                carOutside.AllowInsert = false;
                carOutside.AllowDelete = false;
                carOutside.AllowUpdate = false;
                //carOutside.Cache.AllowSelect = false;
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.carGlass>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.electricalMirrors>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.glassWipers>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.headLamps>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.rearLamps>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.signalLamps>(testCache, rowCurrent, false);
                //PXUIFieldAttribute.SetVisible<InspectionFormInq.sideMirrors>(testCache, rowCurrent, false);

                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.carGlass>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.electricalMirrors>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.glassWipers>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.headLamps>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.rearLamps>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.signalLamps>(testCache, rowCurrent, PXUIVisibility.Invisible);
                //PXUIFieldAttribute.SetVisibility<InspectionFormInq.sideMirrors>(testCache, rowCurrent, PXUIVisibility.Invisible);
            }

            //currentInspectionForm.View.Cache.RaiseRowSelected(row);

            //if (row.InspectionClass != "SS" && row.InspectionType != "F")
            //{
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.angles>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.arms>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.bars>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.brakeConnections>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.brakeDrums>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.calipers>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.couplings>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.exhaustSystem>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.fourByFourSys>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.generalMaster>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.handBrake>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.headBrakeLining>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.petrolConnections>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.powerPump>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.rearBrakeLining>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.shockAbsorber>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.steeringBox>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.teeshBalance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.zippers>(cache, row, false);
            //}

            //if (row.InspectionClass != "RT" && row.InspectionType != "F")
            //{
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.aBSPerformance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.airCondPerformance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.driverDashBoardPerformance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.engineSounds>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.engineStartup>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.engineStartupIDLE>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.gearBoxPerformance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.heatingSys>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.shockAbsorberPerformance>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.smoothEngineAcceleration>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.smoothPowerSteering>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.suspentionSystem>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.suspentionSystemSound>(cache, row, false);
            //}

            //if (row.InspectionClass != "M" && row.InspectionType != "F")
            //{
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.carGlass>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.electricalMirrors>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.glassWipers>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.headLamps>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.rearLamps>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.sideMirrors>(cache, row, false);
            //    //PXUIFieldAttribute.SetVisible<InspectionFormInq.signalLamps>(cache, row, false);
            //}

            //if (row.InspectionClass != "CO" && row.InspectionType != "F")
            //{
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.carGlass>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.electricalMirrors>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.glassWipers>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.headLamps>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.rearLamps>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.sideMirrors>(cache, row, false);
            //    PXUIFieldAttribute.SetVisible<InspectionFormInq.signalLamps>(cache, row, false);
            //}


            //currentInspectionForm.View.RequestRefresh();
            //inspectionForm.View.RequestRefresh();
        }



        #endregion




        #region Buttons

        #region Convert To Job Order

        public PXAction<InspectionFormInq> ConvertToJobOrder;
        [PXUIField(DisplayName = "Convert To Job Order", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        protected virtual IEnumerable convertToJobOrder(PXAdapter adapter)
        {

            graph = PXGraph.CreateInstance<JobOrderMaint>();
            InspectionFormInq form = (InspectionFormInq)inspectionForm.Cache.Current;
            if (!(form.InspectionFormNbr is null) && form.InspectionFormNbr != " <NEW>")
            {
                JobOrder job = new JobOrder();

                job.ItemsID = form.Vehicle;

                job.InspectionNbr = form.InspectionFormNbr;
                
                job.Customer = form.Customer;
                job.AssignedToDateTime = form.Date;
                job.JobOrdrID = "<NEW>";
                job.Branchesss = form.Branches;
                job.BranchID = Accessinfo.BranchID;
                job.KM = form.KM;
           
                graph.jobOreder.Insert(job);
                graph.Actions.PressSave();
              
                form.JobOrderID = graph.jobOreder.Current.JobOrdrID;

               
            }
            else
            {
                throw new PXException("Please Save The Form First.");
            }

            return adapter.Get();

        }

        #endregion

        #region Cancel Form

        public PXAction<InspectionFormInq> CancelForm;
        [PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable cancelForm(PXAdapter adapter)
        {

            InspectionFormInq form = (InspectionFormInq)inspectionForm.Cache.Current;

            if (form.InspectionFormNbr is null || form.InspectionFormNbr == " <NEW>")
                throw new PXException("Cant Cancel Unsaved Form.");
            return adapter.Get();
        }



        #endregion

        #region Open Report

        public PXAction<InspectionFormInq> OpenReport;
        [PXUIField(DisplayName = "Print Inspection Form")]
        [PXButton(CommitChanges = true)]
        protected void openReport()
        {
            InspectionFormInq currentInspectionForm = this.inspectionForm.Current;

            if (currentInspectionForm != null)
            {
                if (currentInspectionForm.InspectionFormNbr != null)
                {
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters["IN"] = currentInspectionForm.InspectionFormNbr;
                    throw new PXReportRequiredException(parameters, "NM000001", PXBaseRedirectException.WindowMode.New, null);
                }
                else
                {
                    throw new PXException("There is no data in the form to show");
                }
            }
            else
            {
                throw new PXException("There is no data in the form to show");
            }
        }

        #endregion

        #endregion

    }
}
