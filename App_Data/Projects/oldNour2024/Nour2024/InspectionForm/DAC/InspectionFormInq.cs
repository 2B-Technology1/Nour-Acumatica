using System;
using Maintenance.MM;
using MyMaintaince;
using Nour20230821VTaxFieldsSolveError;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;


namespace Nour20220913V1
{
    [Serializable]
    [PXCacheName("InspectionFormInq")]
    [PXPrimaryGraph(typeof(InspectionFormEntry))]
    public class InspectionFormInq : IBqlTable
    {

        #region Master 
        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCC")]
        //[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Inspection Number", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<inspectionFormNbr>), typeof(inspectionFormNbr), typeof(status))]
        [AutoNumber(typeof(SetupForm.numberingID), typeof(createdDateTime))]
        public virtual string InspectionFormNbr {get; set;}
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> {}
        #endregion

        #region Status
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [PXDefault("O")]
        [PXStringList(
            new string[] { "J", "O", "C" }, new string[] { "Job Ordered", "Open", "Canceled" })]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region Inspection Type
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Type", Enabled = true)]
        [PXDefault("F")]
        [PXStringList(
            new string[] { "F", "S" }, new string[] { "Full Inspection", "Specific Inspection" })]
        public virtual string InspectionType { get; set; }
        public abstract class inspectionType : PX.Data.BQL.BqlString.Field<inspectionType> { }
        #endregion

        #region Inspection Class
        [PXDBString(2, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Class", Enabled = true)]
        [PXDefault("MC")]
        [PXStringList(
            new string[] { "MC", "SS", "RT", "M", "CO" }, new string[] { "Malfunction Check", "Suspension System", "Road Test", "Motor", "Car Outside" })]
        public virtual string InspectionClass { get; set; }
        public abstract class inspectionClass : PX.Data.BQL.BqlString.Field<inspectionClass> { }
        #endregion

        #region Date
        [PXDBDate()]
        [PXUIField(DisplayName = "Date")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? Date { get; set; }
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
        #endregion

        #region JobOrderID
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Job Order ID", Enabled = false)]
        [PXSelector(
            typeof(Search<JobOrder.jobOrdrID, Where<JobOrder.inspectionNbr, Equal<Current<inspectionFormNbr>>>>))]
        public virtual string JobOrderID { get; set; }
        public abstract class jobOrderID : PX.Data.BQL.BqlString.Field<jobOrderID> { }
        #endregion

        #region Branches

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Branch")]
        [PXSelector(typeof(Search<SetUp.branchCD, Where<SetUp.autoNumbering, Equal<True>>>))]
        [PXDefault()]
        public virtual string Branches { get; set;   }
        public abstract class branches : PX.Data.BQL.BqlString.Field<branches>{ }
        #endregion

        #region Customer
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Customer")]
        [PXDefault()]
        [PXSelector(typeof(Search2<Customer.acctCD, InnerJoin<ItemCustomers, On<ItemCustomers.customerID, Equal<Customer.bAccountID>>>, Where<ItemCustomers.itemsID, Equal<Current<vehicle>>>>)
                   , new Type[]
                   {
                       typeof(Customer.acctCD),
                       typeof(Customer.acctName)
                   }
                   , DescriptionField = typeof(Customer.acctName)
                   , SubstituteKey = typeof(Customer.acctCD))]
        public virtual string Customer { get; set; }
        public abstract class customer : PX.Data.BQL.BqlString.Field<customer> { }
        #endregion

        #region Vehicle
        [PXDBInt()]
        [PXDefault()]
        [PXUIField(DisplayName = "Vehicle")]
        [PXSelector(typeof(Search<Items2.itemsID>)
                    , new Type[] {
                        typeof(Items2.code),
                        typeof(Items2.name),
                        //typeof(Items.customer),
                        typeof(Items2.brandID),
                        typeof(Items2.modelID),
                        typeof(Items2.purchesDate),
                        typeof(Items2.lincensePlat),
                        typeof(Items2.mgfDate),
                        typeof(Items2.gurarantYear),
                    }
                    , DescriptionField = typeof(Items2.name)
                    , SubstituteKey = typeof(Items2.code))]
        public virtual int? Vehicle { get; set; }
        public abstract class vehicle : PX.Data.BQL.BqlInt.Field<vehicle> { }
        #endregion

        #region KM
     
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Kilometer")]
        public virtual string KM { get; set;}
        public abstract class kM : PX.Data.BQL.BqlString.Field<kM>
        {
        }
        #endregion

        #region Phone
        [PXDBString(20, InputMask = "")]
        [PXUIField(DisplayName = "Phone Number")]
        public virtual string Phone { get; set; }
        public abstract class phone : PX.Data.BQL.BqlString.Field<phone> { }
        #endregion

        #region Comment
        [PXDBString(InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Notes")]
        public virtual string Comment { get; set; }
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        #endregion

        #endregion

        #region Car outside tab
        #region CarGlass
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "زجاج السيارة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string CarGlass { get; set; }
        public abstract class carGlass : PX.Data.BQL.BqlString.Field<carGlass> { }
        #endregion

        #region GlassWipers
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "مساحات الزجاج")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string GlassWipers { get; set; }
        public abstract class glassWipers : PX.Data.BQL.BqlString.Field<glassWipers> { }
        #endregion

        #region SideMirrors
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "المرايات الجانبية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SideMirrors { get; set; }
        public abstract class sideMirrors : PX.Data.BQL.BqlString.Field<sideMirrors> { }
        #endregion

        #region ElectricalMirrors
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "المرايات الكهربائية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ElectricalMirrors { get; set; }
        public abstract class electricalMirrors : PX.Data.BQL.BqlString.Field<electricalMirrors> { }
        #endregion

        #region HeadLamps
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الأنوار الأمامية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string HeadLamps { get; set; }
        public abstract class headLamps : PX.Data.BQL.BqlString.Field<headLamps> { }
        #endregion

        #region RearLamps
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الأنوار الخلفية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string RearLamps { get; set; }
        public abstract class rearLamps : PX.Data.BQL.BqlString.Field<rearLamps> { }
        #endregion

        #region SignalLamps
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أنوار الأشارات")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SignalLamps { get; set; }
        public abstract class signalLamps : PX.Data.BQL.BqlString.Field<signalLamps> { }
        #endregion
        #endregion

        #region Motor tab
        #region Liquids
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "السوائل")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Liquids { get; set; }
        public abstract class liquids : PX.Data.BQL.BqlString.Field<liquids> { }
        #endregion

        #region EngineOil
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "زيت المحرك / الفلتر")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngineOil { get; set; }
        public abstract class engineOil : PX.Data.BQL.BqlString.Field<engineOil> { }
        #endregion

        #region CoolingLiq
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "سائل التبريد")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string CoolingLiq { get; set; }
        public abstract class coolingLiq : PX.Data.BQL.BqlString.Field<coolingLiq> { }
        #endregion

        #region BrakeLiq
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "سائل الفرامل")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string BrakeLiq { get; set; }
        public abstract class brakeLiq : PX.Data.BQL.BqlString.Field<brakeLiq> { }
        #endregion

        #region GearBoxLiq
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "زيت الفتيس")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string GearBoxLiq { get; set; }
        public abstract class gearBoxLiq : PX.Data.BQL.BqlString.Field<gearBoxLiq> { }
        #endregion

        #region PowerSteeringLiq
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "سائل نظام التوجية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string PowerSteeringLiq { get; set; }
        public abstract class powerSteeringLiq : PX.Data.BQL.BqlString.Field<powerSteeringLiq> { }
        #endregion

        #region WiperWater
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "مياة المساحات")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string WiperWater { get; set; }
        public abstract class wiperWater : PX.Data.BQL.BqlString.Field<wiperWater> { }
        #endregion

        #region AirCondFreon
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "شحن فريون التكييف")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string AirCondFreon { get; set; }
        public abstract class airCondFreon : PX.Data.BQL.BqlString.Field<airCondFreon> { }
        #endregion

        #region EngFluidLeakage
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "تسريب سوائل المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngFluidLeakage { get; set; }
        public abstract class engFluidLeakage : PX.Data.BQL.BqlString.Field<engFluidLeakage> { }
        #endregion

        #region ConnectionsHosesCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة الوصلات والخراطيم")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ConnectionsHosesCond { get; set; }
        public abstract class connectionsHosesCond : PX.Data.BQL.BqlString.Field<connectionsHosesCond> { }
        #endregion

        #region BeltCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة السيور")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string BeltCond { get; set; }
        public abstract class beltCond : PX.Data.BQL.BqlString.Field<beltCond> { }
        #endregion

        #region ElectricalConnectionsCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة وصلات الكهرباء")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ElectricalConnectionsCond { get; set; }
        public abstract class electricalConnectionsCond : PX.Data.BQL.BqlString.Field<electricalConnectionsCond> { }
        #endregion

        #region OilInAirAunt
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "وجود زيت في عمة الهواء")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string OilInAirAunt { get; set; }
        public abstract class oilInAirAunt : PX.Data.BQL.BqlString.Field<oilInAirAunt> { }
        #endregion

        #region OilInCoolingLiq
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "وجود زيت في مياة التبريد")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string OilInCoolingLiq { get; set; }
        public abstract class oilInCoolingLiq : PX.Data.BQL.BqlString.Field<oilInCoolingLiq> { }
        #endregion

        #region MechanicalBelt
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "سير الميكانيكة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string MechanicalBelt { get; set; }
        public abstract class mechanicalBelt : PX.Data.BQL.BqlString.Field<mechanicalBelt> { }
        #endregion

        #region EngineBases
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "قواعد المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngineBases { get; set; }
        public abstract class engineBases : PX.Data.BQL.BqlString.Field<engineBases> { }
        #endregion

        #region RadiatorCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة الرادياتير")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string RadiatorCond { get; set; }
        public abstract class radiatorCond : PX.Data.BQL.BqlString.Field<radiatorCond> { }
        #endregion

        #region RadiatorCover
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "غطاء الرادياتير")]
        [PXDefault("N")]

        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string RadiatorCover { get; set; }
        public abstract class radiatorCover : PX.Data.BQL.BqlString.Field<radiatorCover> { }
        #endregion

        #region CoolingFans
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "مراوح التبريد")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string CoolingFans { get; set; }
        public abstract class coolingFans : PX.Data.BQL.BqlString.Field<coolingFans> { }
        #endregion

        #region WaterPump
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "طلمبة المياة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string WaterPump { get; set; }
        public abstract class waterPump : PX.Data.BQL.BqlString.Field<waterPump> { }
        #endregion

        #region CoolingWaterBox
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "علبة مياة التبريد")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string CoolingWaterBox { get; set; }
        public abstract class coolingWaterBox : PX.Data.BQL.BqlString.Field<coolingWaterBox> { }
        #endregion

        #region AirCondFilter
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "فلتر التكييف")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string AirCondFilter { get; set; }
        public abstract class airCondFilter : PX.Data.BQL.BqlString.Field<airCondFilter> { }
        #endregion

        #region PetrolPumpSound
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "صوت طلمبة البنزين")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string PetrolPumpSound { get; set; }
        public abstract class petrolPumpSound : PX.Data.BQL.BqlString.Field<petrolPumpSound> { }
        #endregion

        #region PetrolFilterCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة فلتر البنزين")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string PetrolFilterCond { get; set; }
        public abstract class petrolFilterCond : PX.Data.BQL.BqlString.Field<petrolFilterCond> { }
        #endregion

        #region OilFilterCond
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "حالة فلتر المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string OilFilterCond { get; set; }
        public abstract class oilFilterCond : PX.Data.BQL.BqlString.Field<oilFilterCond> { }
        #endregion

        #region March
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "المارش")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string March { get; set; }
        public abstract class march : PX.Data.BQL.BqlString.Field<march> { }
        #endregion

        #region SparkPlugsAndMobines
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "البوجيهات والموبينة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SparkPlugsAndMobines { get; set; }
        public abstract class sparkPlugsAndMobines : PX.Data.BQL.BqlString.Field<sparkPlugsAndMobines> { }
        #endregion

        #region Battery
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "البطارية")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Battery { get; set; }
        public abstract class battery : PX.Data.BQL.BqlString.Field<battery> { }
        #endregion

        #region Dynamo
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الدينامو")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Dynamo { get; set; }
        public abstract class dynamo : PX.Data.BQL.BqlString.Field<dynamo> { }
        #endregion

        #endregion

        #region Road Test tab
        #region EngineStartup
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "دوارة المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngineStartup { get; set; }
        public abstract class engineStartup : PX.Data.BQL.BqlString.Field<engineStartup> { }
        #endregion

        #region EngineStartupIDLE
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "IDLE دوارة المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngineStartupIDLE { get; set; }
        public abstract class engineStartupIDLE : PX.Data.BQL.BqlString.Field<engineStartupIDLE> { }
        #endregion

        #region SmoothEngineAcceleration
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "عمل وتسارع المحرك سلس")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SmoothEngineAcceleration { get; set; }
        public abstract class smoothEngineAcceleration : PX.Data.BQL.BqlString.Field<smoothEngineAcceleration> { }
        #endregion

        #region EngineSounds
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أصوات المحرك")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string EngineSounds { get; set; }
        public abstract class engineSounds : PX.Data.BQL.BqlString.Field<engineSounds> { }
        #endregion

        #region GearBoxPerformance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "عمل الفتيس")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string GearBoxPerformance { get; set; }
        public abstract class gearBoxPerformance : PX.Data.BQL.BqlString.Field<gearBoxPerformance> { }
        #endregion

        #region SmoothPowerSteering
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "توجية سلس ودقيق")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SmoothPowerSteering { get; set; }
        public abstract class smoothPowerSteering : PX.Data.BQL.BqlString.Field<smoothPowerSteering> { }
        #endregion

        #region SuspentionSystem
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "نظام التعليق")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SuspentionSystem { get; set; }
        public abstract class suspentionSystem : PX.Data.BQL.BqlString.Field<suspentionSystem> { }
        #endregion

        #region SuspentionSystemSound
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أصوات نظام التعليق")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SuspentionSystemSound { get; set; }
        public abstract class suspentionSystemSound : PX.Data.BQL.BqlString.Field<suspentionSystemSound> { }
        #endregion

        #region ShockAbsorberPerformance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "عمل المساعدين")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ShockAbsorberPerformance { get; set; }
        public abstract class shockAbsorberPerformance : PX.Data.BQL.BqlString.Field<shockAbsorberPerformance> { }
        #endregion

        #region ABSPerformance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "ABS عمل الفرامل")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ABSPerformance { get; set; }
        public abstract class aBSPerformance : PX.Data.BQL.BqlString.Field<aBSPerformance> { }
        #endregion

        #region DriverDashBoardPerformance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "العدادات تعمل")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string DriverDashBoardPerformance { get; set; }
        public abstract class driverDashBoardPerformance : PX.Data.BQL.BqlString.Field<driverDashBoardPerformance> { }
        #endregion

        #region AirCondPerformance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أداء التكييف")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string AirCondPerformance { get; set; }
        public abstract class airCondPerformance : PX.Data.BQL.BqlString.Field<airCondPerformance> { }
        #endregion

        #region HeatingSys
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "نظام التدفئة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string HeatingSys { get; set; }
        public abstract class heatingSys : PX.Data.BQL.BqlString.Field<heatingSys> { }
        #endregion
        #endregion

        //InspectionSuspensionSystem
        #region InspectionSuspensionSystem
        #region ShockAbsorber
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "المساعدين")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ShockAbsorber { get; set; }
        public abstract class shockAbsorber : PX.Data.BQL.BqlString.Field<shockAbsorber> { }
        #endregion

        #region Angles
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الزوايا")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Angles { get; set; }
        public abstract class angles : PX.Data.BQL.BqlString.Field<angles> { }
        #endregion

        #region PowerPump
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "طلمبة الباور")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string PowerPump { get; set; }
        public abstract class powerPump : PX.Data.BQL.BqlString.Field<powerPump> { }
        #endregion

        #region Calipers
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "كاليبر / الماستر الفرعى")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Calipers { get; set; }
        public abstract class calipers : PX.Data.BQL.BqlString.Field<calipers> { }
        #endregion

        #region HeadBrakeLining
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "التيل الأمامى")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string HeadBrakeLining { get; set; }
        public abstract class headBrakeLining : PX.Data.BQL.BqlString.Field<headBrakeLining> { }
        #endregion

        #region RearBrakeLining
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "التيل الخلفى")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string RearBrakeLining { get; set; }
        public abstract class rearBrakeLining : PX.Data.BQL.BqlString.Field<rearBrakeLining> { }
        #endregion

        #region BrakeDrums
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الطنابير")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string BrakeDrums { get; set; }
        public abstract class brakeDrums : PX.Data.BQL.BqlString.Field<brakeDrums> { }
        #endregion

        #region BrakeConnections
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "توصيلات الفرامل")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string BrakeConnections { get; set; }
        public abstract class brakeConnections : PX.Data.BQL.BqlString.Field<brakeConnections> { }
        #endregion

        #region HandBrake
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "فرامل اليد")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string HandBrake { get; set; }
        public abstract class handBrake : PX.Data.BQL.BqlString.Field<handBrake> { }
        #endregion

        #region GeneralMaster
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الماستر العمومى")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string GeneralMaster { get; set; }
        public abstract class generalMaster : PX.Data.BQL.BqlString.Field<generalMaster> { }
        #endregion

        #region PetrolConnections
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "وصلات الوقود")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string PetrolConnections { get; set; }
        public abstract class petrolConnections : PX.Data.BQL.BqlString.Field<petrolConnections> { }
        #endregion

        #region ExhaustSystem
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "نظام العادم")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string ExhaustSystem { get; set; }
        public abstract class exhaustSystem : PX.Data.BQL.BqlString.Field<exhaustSystem> { }
        #endregion

        #region Couplings
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "الكبائن / صليبة الكردان")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Couplings { get; set; }
        public abstract class couplings : PX.Data.BQL.BqlString.Field<couplings> { }
        #endregion

        #region FourByFourSys
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "نظام الدفع الرباعى / الكارونة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string FourByFourSys { get; set; }
        public abstract class fourByFourSys : PX.Data.BQL.BqlString.Field<fourByFourSys> { }
        #endregion

        #region SteeringBox
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "علبة الدريكسيون")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string SteeringBox { get; set; }
        public abstract class steeringBox : PX.Data.BQL.BqlString.Field<steeringBox> { }
        #endregion

        #region Arms
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "المقصات")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Arms { get; set; }
        public abstract class arms : PX.Data.BQL.BqlString.Field<arms> { }
        #endregion

        #region Bars
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "البارات")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Bars { get; set; }
        public abstract class bars : PX.Data.BQL.BqlString.Field<bars> { }
        #endregion

        #region TeeshBalance
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أتياش الميزان")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string TeeshBalance { get; set; }
        public abstract class teeshBalance : PX.Data.BQL.BqlString.Field<teeshBalance> { }
        #endregion

        #region Zippers
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "السوست")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string Zippers { get; set; }
        public abstract class zippers : PX.Data.BQL.BqlString.Field<zippers> { }
        #endregion

        #endregion

        //InspectionMalfunctionCheck
        #region MalfunctionCheck
        #region CarControlSysTest
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "أختبار نظام تحكم السيارة")]
        [PXDefault("N")]
        [PXStringList
            (
            new string[] { "G", "M", "B", "N" },
            new string[] { "Good", "Medium", "Bad", "N/A" }
            )
        ]
        public virtual string CarControlSysTest { get; set; }
        public abstract class carControlSysTest : PX.Data.BQL.BqlString.Field<carControlSysTest> { }
        #endregion
        #endregion

        //System Fields
        #region Tstamp
        [PXDBTimestamp()]
        [PXUIField(DisplayName = "Tstamp")]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
        #endregion

        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion

        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion

        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion

        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion

        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
    }
}