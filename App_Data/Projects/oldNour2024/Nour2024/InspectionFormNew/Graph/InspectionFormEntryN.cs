using MyMaintaince;
using Nour20220913V1;
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
    public class InspectionFormEntryN : PXGraph<InspectionFormEntryN, InspectionFormMaster>
    {

        #region All Views

        public PXSelect<InspectionFormMaster> inspectionFormView;
        public PXSelect<JobOrder, Where<JobOrder.inspectionNbr, Equal<Required<JobOrder.inspectionNbr>>>> inspecJob;
        //public PXSelect<InspectionJobOrder, Where<InspectionJobOrder.inspectionNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> inspJobOrder;
        public PXSelectJoin<Customer, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Required<ItemCustomers.itemsID>>>> customer;
        public PXSetup<SetupForm> AutoNumberSetup;

        #region Tab Views
        //public PXSelect<InspectionMalfunctionCheck, Where<InspectionMalfunctionCheck.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> malfunCheckView;
        //public PXSelect<InspectionCarOutside, Where<InspectionCarOutside.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> carOutsideView;
        //public PXSelect<InspectionRoadTest, Where<InspectionRoadTest.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> roadTestView;
        //public PXSelect<InspectionSuspensionSystem, Where<InspectionSuspensionSystem.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> suspensionSysView;
        //public PXSelect<InspectionMotor, Where<InspectionMotor.inspectionFormNbr, Equal<Current<InspectionFormMaster.inspectionFormNbr>>>> motorView;

        public SelectFrom<InspectionSuspensionSystem>.
    LeftJoin<InspectionFormMaster>.On<InspectionFormMaster.inspectionFormNbr.IsEqual<InspectionSuspensionSystem.inspectionFormNbr>>.
    Where<InspectionSuspensionSystem.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View
    suspensionSysView;

        public SelectFrom<InspectionRoadTest>.
    LeftJoin<InspectionFormMaster>.On<InspectionFormMaster.inspectionFormNbr.IsEqual<InspectionRoadTest.inspectionFormNbr>>.
    Where<InspectionRoadTest.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View
    roadTestView;

        public SelectFrom<InspectionMotor>.
    LeftJoin<InspectionFormMaster>.On<InspectionFormMaster.inspectionFormNbr.IsEqual<InspectionMotor.inspectionFormNbr>>.
    Where<InspectionMotor.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View
    motorView;

        public SelectFrom<InspectionCarOutside>.
    LeftJoin<InspectionFormMaster>.On<InspectionFormMaster.inspectionFormNbr.IsEqual<InspectionCarOutside.inspectionFormNbr>>.
    Where<InspectionCarOutside.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View
    carOutsideView;

        public SelectFrom<InspectionMalfunctionCheck>.
    LeftJoin<InspectionFormMaster>.On<InspectionFormMaster.inspectionFormNbr.IsEqual<InspectionMalfunctionCheck.inspectionFormNbr>>.
    Where<InspectionMalfunctionCheck.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View
    malfunCheckView;


        //public PXSelect<InspectionMalfunctionCheck> malfunCheckView;
        //public PXSelect<InspectionCarOutside> carOutsideView;
        //public PXSelect<InspectionRoadTest> roadTestView;
        //public PXSelect<InspectionSuspensionSystem> suspensionSysView;
        //public PXSelect<InspectionMotor> motorView;
        #endregion

        #endregion




        #region constructor
        public InspectionFormEntryN()
        {
            SetupForm setup = AutoNumberSetup.Current;
        }
        #endregion

        #region Resetting the customer when changing the vehicle

        //protected virtual void _(Events.RowPersisted<InspectionFormMaster> e)
        //{
        //    var row = e.Row;
        //    if (row.Status == "J" && row.JobOrderID != null)
        //    {

        //        throw new PXRedirectRequiredException(graph, true, null);
        //    }
        //}
        protected void InspectionFormMaster_Vehicle_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
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
            //inspectionFormView.View.RequestRefresh();


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
            //}

            //bool isMalfunction = row.InspectionClass == "MC";
            //bool isCarOutside = row.InspectionClass == "CO";
            //bool isRoadTest = row.InspectionClass == "RT";
            //bool isSuspensionSystem = row.InspectionClass == "SS";
            //bool isMotor = row.InspectionClass == "M";

            //malfunCheckView.Cache.AllowSelect = isMalfunction;
            //carOutsideView.Cache.AllowSelect = isCarOutside;
            //roadTestView.Cache.AllowSelect = isRoadTest;
            //motorView.Cache.AllowSelect = isMotor;
            //suspensionSysView.Cache.AllowSelect = isSuspensionSystem;


            #region tabs

            if (row.InspectionType == "S")
            {
                if (row.InspectionClass == "MC")
                {
                    malfunCheckView.Cache.AllowSelect = true;
                    carOutsideView.Cache.AllowSelect = false;
                    roadTestView.Cache.AllowSelect = false;
                    motorView.Cache.AllowSelect = false;
                    suspensionSysView.Cache.AllowSelect = false;
                }
                else if (row.InspectionClass == "CO")
                {
                    malfunCheckView.Cache.AllowSelect = false;
                    carOutsideView.Cache.AllowSelect = true;
                    roadTestView.Cache.AllowSelect = false;
                    motorView.Cache.AllowSelect = false;
                    suspensionSysView.Cache.AllowSelect = false;
                }
                else if (row.InspectionClass == "RT")
                {
                    malfunCheckView.Cache.AllowSelect = false;
                    carOutsideView.Cache.AllowSelect = false;
                    roadTestView.Cache.AllowSelect = true;
                    motorView.Cache.AllowSelect = false;
                    suspensionSysView.Cache.AllowSelect = false;
                }
                else if (row.InspectionClass == "SS")
                {
                    malfunCheckView.Cache.AllowSelect = false;
                    carOutsideView.Cache.AllowSelect = false;
                    roadTestView.Cache.AllowSelect = false;
                    motorView.Cache.AllowSelect = false;
                    suspensionSysView.Cache.AllowSelect = true;
                }
                else if (row.InspectionClass == "M")
                {
                    malfunCheckView.Cache.AllowSelect = false;
                    carOutsideView.Cache.AllowSelect = false;
                    roadTestView.Cache.AllowSelect = false;
                    motorView.Cache.AllowSelect = true;
                    suspensionSysView.Cache.AllowSelect = false;
                }

            }
            else
            {

                malfunCheckView.Cache.AllowSelect = true;
                carOutsideView.Cache.AllowSelect = true;
                roadTestView.Cache.AllowSelect = true;
                motorView.Cache.AllowSelect = true;
                suspensionSysView.Cache.AllowSelect = true;
            }

            #endregion 


            #region last 

            /*
            
            if (row.InspectionClass != "MC" && row.InspectionType != "F")
            {
                malfunCheckView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionMalfunctionCheck.carControlSysTest>(cache, row, false);
                PXUIFieldAttribute.SetVisible<InspectionMalfunctionCheck.carControlSysTest>(malfunCheckView.Cache, malfunCheckView.Current, false);
                malfunCheckView.Cache.AllowSelect = false;
            }
            else
            {
                PXUIFieldAttribute.SetVisible<InspectionMalfunctionCheck.carControlSysTest>(cache, row, true);
                PXUIFieldAttribute.SetVisible<InspectionMalfunctionCheck.carControlSysTest>(malfunCheckView.Cache, malfunCheckView.Current, true);
                malfunCheckView.Cache.AllowSelect = true;
            }

            if (row.InspectionClass != "SS" && row.InspectionType != "F")
            {
                suspensionSysView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.angles>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.arms>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.bars>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.brakeConnections>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.brakeDrums>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.calipers>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.couplings>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.exhaustSystem>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.fourByFourSys>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.generalMaster>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.handBrake>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.headBrakeLining>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.petrolConnections>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.powerPump>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.rearBrakeLining>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.shockAbsorber>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.steeringBox>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.teeshBalance>(suspensionSysView.Cache, suspensionSysView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.zippers>(suspensionSysView.Cache, suspensionSysView.Current, false);
                suspensionSysView.Cache.AllowSelect = false;


            }
            else
            {
                suspensionSysView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.angles>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.arms>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.bars>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.brakeConnections>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.brakeDrums>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.calipers>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.couplings>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.exhaustSystem>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.fourByFourSys>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.generalMaster>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.handBrake>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.headBrakeLining>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.petrolConnections>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.powerPump>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.rearBrakeLining>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.shockAbsorber>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.steeringBox>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.teeshBalance>(suspensionSysView.Cache, suspensionSysView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionSuspensionSystem.zippers>(suspensionSysView.Cache, suspensionSysView.Current, true);
            }

            if (row.InspectionClass != "RT" && row.InspectionType != "F")
            {
                roadTestView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.aBSPerformance>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.airCondPerformance>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.driverDashBoardPerformance>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineSounds>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineStartup>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineStartupIDLE>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.gearBoxPerformance>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.heatingSys>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.shockAbsorberPerformance>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.smoothEngineAcceleration>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.smoothPowerSteering>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.suspentionSystem>(roadTestView.Cache, roadTestView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.suspentionSystemSound>(roadTestView.Cache, roadTestView.Current, false);
                roadTestView.Cache.AllowSelect = false;
            }
            else
            {
                roadTestView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.aBSPerformance>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.airCondPerformance>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.driverDashBoardPerformance>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineSounds>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineStartup>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.engineStartupIDLE>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.gearBoxPerformance>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.heatingSys>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.shockAbsorberPerformance>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.smoothEngineAcceleration>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.smoothPowerSteering>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.suspentionSystem>(roadTestView.Cache, roadTestView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionRoadTest.suspentionSystemSound>(roadTestView.Cache, roadTestView.Current, true);
            }


            if (row.InspectionClass != "CO" && row.InspectionType != "F")
            {
                carOutsideView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.carGlass>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.electricalMirrors>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.glassWipers>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.headLamps>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.rearLamps>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.sideMirrors>(carOutsideView.Cache, carOutsideView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.signalLamps>(carOutsideView.Cache, carOutsideView.Current, false);
                carOutsideView.Cache.AllowSelect = false;
            }
            else
            {

                carOutsideView.Cache.AllowSelect = true;
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.carGlass>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.electricalMirrors>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.glassWipers>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.headLamps>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.rearLamps>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.sideMirrors>(carOutsideView.Cache, carOutsideView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionCarOutside.signalLamps>(carOutsideView.Cache, carOutsideView.Current, true);
            }


            if (row.InspectionClass != "M" && row.InspectionType != "F")
            {

                motorView.Cache.AllowSelect = true;

                PXUIFieldAttribute.SetVisible<InspectionMotor.liquids>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engineOil>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingLiq>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.brakeLiq>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.gearBoxLiq>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.powerSteeringLiq>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.wiperWater>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.airCondFreon>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engFluidLeakage>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.connectionsHosesCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.beltCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.electricalConnectionsCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilInAirAunt>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilInCoolingLiq>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.mechanicalBelt>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engineBases>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.radiatorCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.radiatorCover>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingFans>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.waterPump>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingWaterBox>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.airCondFilter>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.petrolPumpSound>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.petrolFilterCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilFilterCond>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.march>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.sparkPlugsAndMobines>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.battery>(motorView.Cache, motorView.Current, false);
                PXUIFieldAttribute.SetVisible<InspectionMotor.dynamo>(motorView.Cache, motorView.Current, false);

                motorView.Cache.AllowSelect = false;

            }
            else
            {
                motorView.Cache.AllowSelect = true;

                PXUIFieldAttribute.SetVisible<InspectionMotor.liquids>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engineOil>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingLiq>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.brakeLiq>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.gearBoxLiq>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.powerSteeringLiq>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.wiperWater>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.airCondFreon>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engFluidLeakage>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.connectionsHosesCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.beltCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.electricalConnectionsCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilInAirAunt>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilInCoolingLiq>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.mechanicalBelt>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.engineBases>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.radiatorCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.radiatorCover>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingFans>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.waterPump>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.coolingWaterBox>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.airCondFilter>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.petrolPumpSound>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.petrolFilterCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.oilFilterCond>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.march>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.sparkPlugsAndMobines>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.battery>(motorView.Cache, motorView.Current, true);
                PXUIFieldAttribute.SetVisible<InspectionMotor.dynamo>(motorView.Cache, motorView.Current, true);

            }
            //currentInspectionForm.View.RequestRefresh();
            //inspectionForm.View.RequestRefresh();
            */
            #endregion


            if (row.Status == "C" || row.Status == "J")
            {
                PXUIFieldAttribute.SetEnabled(inspectionFormView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(carOutsideView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(inspecJob.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(motorView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(roadTestView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(suspensionSysView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled(malfunCheckView.Cache, null, false);
                PXUIFieldAttribute.SetEnabled<InspectionFormMaster.inspectionFormNbr>(inspectionFormView.Cache, null, true);
                CancelForm.SetEnabled(false);
                ConvertToJobOrder.SetEnabled(false);
                //jobOrder.Cache.Clear();
                //JobOrder job = jobOrder.SelectSingle();
                //row.JobOrderID = job.JobOrdrID;
            }
            else
            {
                PXUIFieldAttribute.SetEnabled(inspectionFormView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(carOutsideView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(inspecJob.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(motorView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(roadTestView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(suspensionSysView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled(malfunCheckView.Cache, null, true);
                PXUIFieldAttribute.SetEnabled<InspectionFormMaster.status>(inspectionFormView.Cache, null, false);
                CancelForm.SetEnabled(true);
                ConvertToJobOrder.SetEnabled(true);
                // row.JobOrderID = null;
            }

            #region last2
            /*

            if (carOutsideView.Current is null)
            {
                InspectionCarOutside outSide = SelectFrom<InspectionCarOutside>
                    .Where<InspectionCarOutside.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View.Select(this);
                if( outSide == null )
                {
                    outSide = new InspectionCarOutside();
                }

                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                carOutsideView.Update(outSide);
                //carOutsideView.Cache.IsDirty = false;
            }

            if (motorView.Current is null)
            {
                InspectionMotor mot = SelectFrom<InspectionMotor>
                    .Where<InspectionMotor.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View.Select(this);

                if (mot == null)
                {
                    mot = new InspectionMotor();
                }
                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                motorView.Update(mot);
                //motorView.Cache.IsDirty = false;
            }
            if (roadTestView.Current is null)
            {
                InspectionRoadTest road = SelectFrom<InspectionRoadTest>
                    .Where<InspectionRoadTest.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View.Select(this);
                if(road == null)
                {
                    road = new InspectionRoadTest();
                }
                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                roadTestView.Update(road);
                //roadTestView.Cache.IsDirty = false;
            }
            if (suspensionSysView.Current is null)
            {
                InspectionSuspensionSystem suspen = SelectFrom<InspectionSuspensionSystem>
                    .Where<InspectionSuspensionSystem.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View.Select(this);

                if (suspen == null)
                {
                 suspen = new InspectionSuspensionSystem();
                }

                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                suspensionSysView.Update(suspen);
                //suspensionSysView.Cache.IsDirty = false;
            }
            if (malfunCheckView.Current is null)
            {
                InspectionMotor mot = SelectFrom<InspectionMotor>
    .Where<InspectionMotor.inspectionFormNbr.IsEqual<InspectionFormMaster.inspectionFormNbr.FromCurrent>>.View.Select(this);


                InspectionMalfunctionCheck malCheck = new InspectionMalfunctionCheck();
                //malCheck.CarControlSysTest = "N";
                // Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers [Justification]
                malfunCheckView.Update(malCheck);
                //malfunCheckView.Cache.IsDirty = false;
            }

            */
            #endregion
        }


        #endregion

        //// ------------
        ////DataControls["CstPXDropDown11"].Value == MC  
        ////DataControls["CstPXDropDown265"].Value != M  
        ///
        ///
        //// ------------






        #region Buttons



        public PXAction<InspectionFormMaster> SaveMobile;
        [PXUIField(DisplayName = "SaveMobile", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        protected IEnumerable saveMobile(PXAdapter adapter)
        {
            this.Actions.PressSave();
            return adapter.Get();


        }


        #region Convert To Job Order

        public PXAction<InspectionFormMaster> ConvertToJobOrder;
        [PXUIField(DisplayName = "Convert To Job Order", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        protected virtual IEnumerable convertToJobOrder(PXAdapter adapter)
        {

            JobOrderMaint graph = PXGraph.CreateInstance<JobOrderMaint>();
            InspectionFormMaster form = (InspectionFormMaster)inspectionFormView.Cache.Current;
            if (!(form.InspectionFormNbr is null))
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

                form.Status = "J";
                inspectionFormView.Update(form);
                this.Actions.PressSave();

                this.ProviderUpdate<InspectionFormMaster>(new PXDataFieldAssign("Status", "J"),
                        new PXDataFieldRestrict("InspectionFormNbr", form.InspectionFormNbr));
            }
            else
            {
                throw new PXException("Please Save The Form First.");
            }

            return adapter.Get();
            //throw new PXRedirectRequiredException(graph, true, null);

        }

        #endregion

        #region Open Report

        public PXAction<InspectionFormMaster> OpenReport;
       // private JobOrderMaint graph;

        [PXUIField(DisplayName = "Print Inspection Form")]
        [PXButton(CommitChanges = true)]
        protected void openReport()
        {
            InspectionFormMaster currentInspectionForm = this.inspectionFormView.Current;

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

        #region Cancel Form

        public PXAction<InspectionFormMaster> CancelForm;
        [PXUIField(DisplayName = "Cancel", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable cancelForm(PXAdapter adapter)
        {

            InspectionFormMaster form = (InspectionFormMaster)inspectionFormView.Cache.Current;

            if (form.InspectionFormNbr is null)
                throw new PXException("Cant Cancel Unsaved Form.");

            form.Status = "C";
            this.ProviderUpdate<InspectionFormMaster>(new PXDataFieldAssign("Status", "C"),
                new PXDataFieldRestrict("InspectionFormNbr", inspectionFormView.Current.InspectionFormNbr));
            return adapter.Get();
        }

        #endregion

        #endregion



    }






}