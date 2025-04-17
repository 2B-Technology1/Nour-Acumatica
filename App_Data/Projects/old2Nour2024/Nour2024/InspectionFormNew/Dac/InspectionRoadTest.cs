using System;
using Nour20220913V1;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionRoadTest")]
  public class InspectionRoadTest : IBqlTable
  {


        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Form Nbr")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(InspectionFormMaster.inspectionFormNbr))]
        [PXParent(typeof(SelectFrom<InspectionFormMaster>.
                              Where<InspectionFormMaster.inspectionFormNbr.
                            IsEqual<InspectionRoadTest.inspectionFormNbr.FromCurrent>>))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion


        #region Road Test tab
        #region EngineStartup
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "œÊ«—… «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "IDLE œÊ«—… «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "⁄„· Ê ”«—⁄ «·„Õ—ﬂ ”·”")]
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
        [PXUIField(DisplayName = "√’Ê«  «·„Õ—ﬂ")]
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
        [PXUIField(DisplayName = "⁄„· «·› Ì”")]
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
        [PXUIField(DisplayName = " ÊÃÌ… ”·” ÊœﬁÌﬁ")]
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
        [PXUIField(DisplayName = "‰Ÿ«„ «· ⁄·Ìﬁ")]
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
        [PXUIField(DisplayName = "√’Ê«  ‰Ÿ«„ «· ⁄·Ìﬁ")]
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
        [PXUIField(DisplayName = "⁄„· «·„”«⁄œÌ‰")]
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
        [PXUIField(DisplayName = "ABS ⁄„· «·›—«„·")]
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
        [PXUIField(DisplayName = "«·⁄œ«œ«   ⁄„·")]
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
        [PXUIField(DisplayName = "√œ«¡ «· ﬂÌÌ›")]
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
        [PXUIField(DisplayName = "‰Ÿ«„ «· œ›∆…")]
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




        //--------------------------------------------------
        //#region EngineStartup
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Engine Startup")]
        //public virtual string EngineStartup { get; set; }
        //public abstract class engineStartup : PX.Data.BQL.BqlString.Field<engineStartup> { }
        //#endregion

        //#region EngineStartupIDLE
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Engine Startup IDLE")]
        //public virtual string EngineStartupIDLE { get; set; }
        //public abstract class engineStartupIDLE : PX.Data.BQL.BqlString.Field<engineStartupIDLE> { }
        //#endregion

        //#region SmoothEngineAcceleration
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Smooth Engine Acceleration")]
        //public virtual string SmoothEngineAcceleration { get; set; }
        //public abstract class smoothEngineAcceleration : PX.Data.BQL.BqlString.Field<smoothEngineAcceleration> { }
        //#endregion

        //#region EngineSounds
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Engine Sounds")]
        //public virtual string EngineSounds { get; set; }
        //public abstract class engineSounds : PX.Data.BQL.BqlString.Field<engineSounds> { }
        //#endregion

        //#region GearBoxPerformance
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Gear Box Performance")]
        //public virtual string GearBoxPerformance { get; set; }
        //public abstract class gearBoxPerformance : PX.Data.BQL.BqlString.Field<gearBoxPerformance> { }
        //#endregion

        //#region SmoothPowerSteering
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Smooth Power Steering")]
        //public virtual string SmoothPowerSteering { get; set; }
        //public abstract class smoothPowerSteering : PX.Data.BQL.BqlString.Field<smoothPowerSteering> { }
        //#endregion

        //#region SuspentionSystem
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Suspention System")]
        //public virtual string SuspentionSystem { get; set; }
        //public abstract class suspentionSystem : PX.Data.BQL.BqlString.Field<suspentionSystem> { }
        //#endregion

        //#region SuspentionSystemSound
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Suspention System Sound")]
        //public virtual string SuspentionSystemSound { get; set; }
        //public abstract class suspentionSystemSound : PX.Data.BQL.BqlString.Field<suspentionSystemSound> { }
        //#endregion

        //#region ShockAbsorberPerformance
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Shock Absorber Performance")]
        //public virtual string ShockAbsorberPerformance { get; set; }
        //public abstract class shockAbsorberPerformance : PX.Data.BQL.BqlString.Field<shockAbsorberPerformance> { }
        //#endregion

        //#region ABSPerformance
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "ABSPerformance")]
        //public virtual string ABSPerformance { get; set; }
        //public abstract class aBSPerformance : PX.Data.BQL.BqlString.Field<aBSPerformance> { }
        //#endregion

        //#region DriverDashBoardPerformance
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Driver Dash Board Performance")]
        //public virtual string DriverDashBoardPerformance { get; set; }
        //public abstract class driverDashBoardPerformance : PX.Data.BQL.BqlString.Field<driverDashBoardPerformance> { }
        //#endregion

        //#region AirCondPerformance
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Air Cond Performance")]
        //public virtual string AirCondPerformance { get; set; }
        //public abstract class airCondPerformance : PX.Data.BQL.BqlString.Field<airCondPerformance> { }
        //#endregion

        //#region HeatingSys
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Heating Sys")]
        //public virtual string HeatingSys { get; set; }
        //public abstract class heatingSys : PX.Data.BQL.BqlString.Field<heatingSys> { }
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