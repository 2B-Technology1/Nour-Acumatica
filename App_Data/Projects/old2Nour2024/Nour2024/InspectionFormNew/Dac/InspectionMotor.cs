using System;
using Nour20220913V1;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionMotor")]
  public class InspectionMotor : IBqlTable
  {


        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Form Nbr")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(InspectionFormMaster.inspectionFormNbr))]
        [PXParent(typeof(SelectFrom<InspectionFormMaster>.
                              Where<InspectionFormMaster.inspectionFormNbr.
                            IsEqual<InspectionMotor.inspectionFormNbr.FromCurrent>>))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion




        #region Motor tab
        #region Liquids
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "«·”Ê«∆·")]
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
        [PXUIField(DisplayName = "“Ì  «·„Õ—ﬂ / «·›· —")]
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
        [PXUIField(DisplayName = "”«∆· «· »—Ìœ")]
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
        [PXUIField(DisplayName = "”«∆· «·›—«„·")]
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
        [PXUIField(DisplayName = "“Ì  «·› Ì”")]
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
        [PXUIField(DisplayName = "”«∆· ‰Ÿ«„ «· ÊÃÌ…")]
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
        [PXUIField(DisplayName = "„Ì«… «·„”«Õ« ")]
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
        [PXUIField(DisplayName = "‘Õ‰ ›—ÌÊ‰ «· ﬂÌÌ›")]
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
        [PXUIField(DisplayName = " ”—Ì» ”Ê«∆· «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "Õ«·… «·Ê’·«  Ê«·Œ—«ÿÌ„")]
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
        [PXUIField(DisplayName = "Õ«·… «·”ÌÊ—")]
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
        [PXUIField(DisplayName = "Õ«·… Ê’·«  «·ﬂÂ—»«¡")]
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
        [PXUIField(DisplayName = "ÊÃÊœ “Ì  ›Ì ⁄„… «·ÂÊ«¡")]
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
        [PXUIField(DisplayName = "ÊÃÊœ “Ì  ›Ì „Ì«… «· »—Ìœ")]
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
        [PXUIField(DisplayName = "”Ì— «·„Ìﬂ«‰Ìﬂ…")]
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
        [PXUIField(DisplayName = "ﬁÊ«⁄œ «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "Õ«·… «·—«œÌ« Ì—")]
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
        [PXUIField(DisplayName = "€ÿ«¡ «·—«œÌ« Ì—")]
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
        [PXUIField(DisplayName = "„—«ÊÕ «· »—Ìœ")]
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
        [PXUIField(DisplayName = "ÿ·„»… «·„Ì«…")]
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
        [PXUIField(DisplayName = "⁄·»… „Ì«… «· »—Ìœ")]
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
        [PXUIField(DisplayName = "›· — «· ﬂÌÌ›")]
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
        [PXUIField(DisplayName = "’Ê  ÿ·„»… «·»‰“Ì‰")]
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
        [PXUIField(DisplayName = "Õ«·… ›· — «·»‰“Ì‰")]
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
        [PXUIField(DisplayName = "Õ«·… ›· — «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "«·„«—‘")]
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
        [PXUIField(DisplayName = "«·»ÊÃÌÂ«  Ê«·„Ê»Ì‰…")]
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
        [PXUIField(DisplayName = "«·»ÿ«—Ì…")]
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
        [PXUIField(DisplayName = "«·œÌ‰«„Ê")]
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





        //--------------------------------------------

        //#region Liquids
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Liquids")]
        //public virtual string Liquids { get; set; }
        //public abstract class liquids : PX.Data.BQL.BqlString.Field<liquids> { }
        //#endregion

        //#region EngineOil
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Engine Oil")]
        //public virtual string EngineOil { get; set; }
        //public abstract class engineOil : PX.Data.BQL.BqlString.Field<engineOil> { }
        //#endregion

        //#region CoolingLiq
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Cooling Liq")]
        //public virtual string CoolingLiq { get; set; }
        //public abstract class coolingLiq : PX.Data.BQL.BqlString.Field<coolingLiq> { }
        //#endregion

        //#region BrakeLiq
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Brake Liq")]
        //public virtual string BrakeLiq { get; set; }
        //public abstract class brakeLiq : PX.Data.BQL.BqlString.Field<brakeLiq> { }
        //#endregion

        //#region GearBoxLiq
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Gear Box Liq")]
        //public virtual string GearBoxLiq { get; set; }
        //public abstract class gearBoxLiq : PX.Data.BQL.BqlString.Field<gearBoxLiq> { }
        //#endregion

        //#region PowerSteeringLiq
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Power Steering Liq")]
        //public virtual string PowerSteeringLiq { get; set; }
        //public abstract class powerSteeringLiq : PX.Data.BQL.BqlString.Field<powerSteeringLiq> { }
        //#endregion

        //#region WiperWater
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Wiper Water")]
        //public virtual string WiperWater { get; set; }
        //public abstract class wiperWater : PX.Data.BQL.BqlString.Field<wiperWater> { }
        //#endregion

        //#region AirCondFreon
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Air Cond Freon")]
        //public virtual string AirCondFreon { get; set; }
        //public abstract class airCondFreon : PX.Data.BQL.BqlString.Field<airCondFreon> { }
        //#endregion

        //#region EngFluidLeakage
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Eng Fluid Leakage")]
        //public virtual string EngFluidLeakage { get; set; }
        //public abstract class engFluidLeakage : PX.Data.BQL.BqlString.Field<engFluidLeakage> { }
        //#endregion

        //#region ConnectionsHosesCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Connections Hoses Cond")]
        //public virtual string ConnectionsHosesCond { get; set; }
        //public abstract class connectionsHosesCond : PX.Data.BQL.BqlString.Field<connectionsHosesCond> { }
        //#endregion

        //#region BeltCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Belt Cond")]
        //public virtual string BeltCond { get; set; }
        //public abstract class beltCond : PX.Data.BQL.BqlString.Field<beltCond> { }
        //#endregion

        //#region ElectricalConnectionsCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Electrical Connections Cond")]
        //public virtual string ElectricalConnectionsCond { get; set; }
        //public abstract class electricalConnectionsCond : PX.Data.BQL.BqlString.Field<electricalConnectionsCond> { }
        //#endregion

        //#region OilInAirAunt
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Oil In Air Aunt")]
        //public virtual string OilInAirAunt { get; set; }
        //public abstract class oilInAirAunt : PX.Data.BQL.BqlString.Field<oilInAirAunt> { }
        //#endregion

        //#region OilInCoolingLiq
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Oil In Cooling Liq")]
        //public virtual string OilInCoolingLiq { get; set; }
        //public abstract class oilInCoolingLiq : PX.Data.BQL.BqlString.Field<oilInCoolingLiq> { }
        //#endregion

        //#region MechanicalBelt
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Mechanical Belt")]
        //public virtual string MechanicalBelt { get; set; }
        //public abstract class mechanicalBelt : PX.Data.BQL.BqlString.Field<mechanicalBelt> { }
        //#endregion

        //#region EngineBases
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Engine Bases")]
        //public virtual string EngineBases { get; set; }
        //public abstract class engineBases : PX.Data.BQL.BqlString.Field<engineBases> { }
        //#endregion

        //#region RadiatorCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Radiator Cond")]
        //public virtual string RadiatorCond { get; set; }
        //public abstract class radiatorCond : PX.Data.BQL.BqlString.Field<radiatorCond> { }
        //#endregion

        //#region RadiatorCover
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Radiator Cover")]
        //public virtual string RadiatorCover { get; set; }
        //public abstract class radiatorCover : PX.Data.BQL.BqlString.Field<radiatorCover> { }
        //#endregion

        //#region CoolingFans
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Cooling Fans")]
        //public virtual string CoolingFans { get; set; }
        //public abstract class coolingFans : PX.Data.BQL.BqlString.Field<coolingFans> { }
        //#endregion

        //#region WaterPump
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Water Pump")]
        //public virtual string WaterPump { get; set; }
        //public abstract class waterPump : PX.Data.BQL.BqlString.Field<waterPump> { }
        //#endregion

        //#region CoolingWaterBox
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Cooling Water Box")]
        //public virtual string CoolingWaterBox { get; set; }
        //public abstract class coolingWaterBox : PX.Data.BQL.BqlString.Field<coolingWaterBox> { }
        //#endregion

        //#region AirCondFilter
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Air Cond Filter")]
        //public virtual string AirCondFilter { get; set; }
        //public abstract class airCondFilter : PX.Data.BQL.BqlString.Field<airCondFilter> { }
        //#endregion

        //#region PetrolPumpSound
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Petrol Pump Sound")]
        //public virtual string PetrolPumpSound { get; set; }
        //public abstract class petrolPumpSound : PX.Data.BQL.BqlString.Field<petrolPumpSound> { }
        //#endregion

        //#region PetrolFilterCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Petrol Filter Cond")]
        //public virtual string PetrolFilterCond { get; set; }
        //public abstract class petrolFilterCond : PX.Data.BQL.BqlString.Field<petrolFilterCond> { }
        //#endregion

        //#region OilFilterCond
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Oil Filter Cond")]
        //public virtual string OilFilterCond { get; set; }
        //public abstract class oilFilterCond : PX.Data.BQL.BqlString.Field<oilFilterCond> { }
        //#endregion

        //#region March
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "March")]
        //public virtual string March { get; set; }
        //public abstract class march : PX.Data.BQL.BqlString.Field<march> { }
        //#endregion

        //#region SparkPlugsAndMobines
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Spark Plugs And Mobines")]
        //public virtual string SparkPlugsAndMobines { get; set; }
        //public abstract class sparkPlugsAndMobines : PX.Data.BQL.BqlString.Field<sparkPlugsAndMobines> { }
        //#endregion

        //#region Battery
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Battery")]
        //public virtual string Battery { get; set; }
        //public abstract class battery : PX.Data.BQL.BqlString.Field<battery> { }
        //#endregion

        //#region Dynamo
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Dynamo")]
        //public virtual string Dynamo { get; set; }
        //public abstract class dynamo : PX.Data.BQL.BqlString.Field<dynamo> { }
        //#endregion







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