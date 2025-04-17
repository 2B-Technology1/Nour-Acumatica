using System;
using Nour20220913V1;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionSuspensionSystem")]
  public class InspectionSuspensionSystem : IBqlTable
  {


        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Form Nbr")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(InspectionFormMaster.inspectionFormNbr))]
        [PXParent(typeof(SelectFrom<InspectionFormMaster>.
                              Where<InspectionFormMaster.inspectionFormNbr.
                            IsEqual<InspectionSuspensionSystem.inspectionFormNbr.FromCurrent>>))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion




        #region InspectionSuspensionSystem
        #region ShockAbsorber
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "«·„”«⁄œÌ‰")]
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
        [PXUIField(DisplayName = "«·“Ê«Ì«")]
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
        [PXUIField(DisplayName = "ÿ·„»… «·»«Ê—")]
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
        [PXUIField(DisplayName = "ﬂ«·Ì»— / «·„«” — «·›—⁄Ï")]
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
        [PXUIField(DisplayName = "«· Ì· «·√„«„Ï")]
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
        [PXUIField(DisplayName = "«· Ì· «·Œ·›Ï")]
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
        [PXUIField(DisplayName = "«·ÿ‰«»Ì—")]
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
        [PXUIField(DisplayName = " Ê’Ì·«  «·›—«„·")]
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
        [PXUIField(DisplayName = "›—«„· «·Ìœ")]
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
        [PXUIField(DisplayName = "«·„«” — «·⁄„Ê„Ï")]
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
        [PXUIField(DisplayName = "Ê’·«  «·ÊﬁÊœ")]
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
        [PXUIField(DisplayName = "‰Ÿ«„ «·⁄«œ„")]
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
        [PXUIField(DisplayName = "«·ﬂ»«∆‰ / ’·Ì»… «·ﬂ—œ«‰")]
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
        [PXUIField(DisplayName = "‰Ÿ«„ «·œ›⁄ «·—»«⁄Ï / «·ﬂ«—Ê‰…")]
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
        [PXUIField(DisplayName = "⁄·»… «·œ—Ìﬂ”ÌÊ‰")]
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
        [PXUIField(DisplayName = "«·„ﬁ’« ")]
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
        [PXUIField(DisplayName = "«·»«—« ")]
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
        [PXUIField(DisplayName = "√ Ì«‘ «·„Ì“«‰")]
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
        [PXUIField(DisplayName = "«·”Ê” ")]
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




        //-----------------------------------------------

        //#region ShockAbsorber
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Shock Absorber")]
        //public virtual string ShockAbsorber { get; set; }
        //public abstract class shockAbsorber : PX.Data.BQL.BqlString.Field<shockAbsorber> { }
        //#endregion

        //#region Angles
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Angles")]
        //public virtual string Angles { get; set; }
        //public abstract class angles : PX.Data.BQL.BqlString.Field<angles> { }
        //#endregion

        //#region PowerPump
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Power Pump")]
        //public virtual string PowerPump { get; set; }
        //public abstract class powerPump : PX.Data.BQL.BqlString.Field<powerPump> { }
        //#endregion

        //#region Calipers
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Calipers")]
        //public virtual string Calipers { get; set; }
        //public abstract class calipers : PX.Data.BQL.BqlString.Field<calipers> { }
        //#endregion

        //#region HeadBrakeLining
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Head Brake Lining")]
        //public virtual string HeadBrakeLining { get; set; }
        //public abstract class headBrakeLining : PX.Data.BQL.BqlString.Field<headBrakeLining> { }
        //#endregion

        //#region RearBrakeLining
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Rear Brake Lining")]
        //public virtual string RearBrakeLining { get; set; }
        //public abstract class rearBrakeLining : PX.Data.BQL.BqlString.Field<rearBrakeLining> { }
        //#endregion

        //#region BrakeDrums
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Brake Drums")]
        //public virtual string BrakeDrums { get; set; }
        //public abstract class brakeDrums : PX.Data.BQL.BqlString.Field<brakeDrums> { }
        //#endregion

        //#region BrakeConnections
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Brake Connections")]
        //public virtual string BrakeConnections { get; set; }
        //public abstract class brakeConnections : PX.Data.BQL.BqlString.Field<brakeConnections> { }
        //#endregion

        //#region HandBrake
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Hand Brake")]
        //public virtual string HandBrake { get; set; }
        //public abstract class handBrake : PX.Data.BQL.BqlString.Field<handBrake> { }
        //#endregion

        //#region GeneralMaster
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "General Master")]
        //public virtual string GeneralMaster { get; set; }
        //public abstract class generalMaster : PX.Data.BQL.BqlString.Field<generalMaster> { }
        //#endregion

        //#region PetrolConnections
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Petrol Connections")]
        //public virtual string PetrolConnections { get; set; }
        //public abstract class petrolConnections : PX.Data.BQL.BqlString.Field<petrolConnections> { }
        //#endregion

        //#region ExhaustSystem
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Exhaust System")]
        //public virtual string ExhaustSystem { get; set; }
        //public abstract class exhaustSystem : PX.Data.BQL.BqlString.Field<exhaustSystem> { }
        //#endregion

        //#region Couplings
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Couplings")]
        //public virtual string Couplings { get; set; }
        //public abstract class couplings : PX.Data.BQL.BqlString.Field<couplings> { }
        //#endregion

        //#region FourByFourSys
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Four By Four Sys")]
        //public virtual string FourByFourSys { get; set; }
        //public abstract class fourByFourSys : PX.Data.BQL.BqlString.Field<fourByFourSys> { }
        //#endregion

        //#region SteeringBox
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Steering Box")]
        //public virtual string SteeringBox { get; set; }
        //public abstract class steeringBox : PX.Data.BQL.BqlString.Field<steeringBox> { }
        //#endregion

        //#region Arms
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Arms")]
        //public virtual string Arms { get; set; }
        //public abstract class arms : PX.Data.BQL.BqlString.Field<arms> { }
        //#endregion

        //#region Bars
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Bars")]
        //public virtual string Bars { get; set; }
        //public abstract class bars : PX.Data.BQL.BqlString.Field<bars> { }
        //#endregion

        //#region TeeshBalance
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Teesh Balance")]
        //public virtual string TeeshBalance { get; set; }
        //public abstract class teeshBalance : PX.Data.BQL.BqlString.Field<teeshBalance> { }
        //#endregion

        //#region Zippers
        //[PXDBString(1, IsFixed = true, InputMask = "")]
        //[PXUIField(DisplayName = "Zippers")]
        //public virtual string Zippers { get; set; }
        //public abstract class zippers : PX.Data.BQL.BqlString.Field<zippers> { }
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