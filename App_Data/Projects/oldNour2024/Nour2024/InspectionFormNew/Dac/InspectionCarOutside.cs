using System;
using Nour20220913V1;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionCarOutside")]
  public class InspectionCarOutside : IBqlTable
  {
   

        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Form Nbr")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(InspectionFormMaster.inspectionFormNbr))]
        [PXParent(typeof(SelectFrom<InspectionFormMaster>.
                              Where<InspectionFormMaster.inspectionFormNbr.
                            IsEqual<InspectionCarOutside.inspectionFormNbr.FromCurrent>>))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion






        #region Car outside tab
        #region CarGlass
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "“Ã«Ã «·”Ì«—…")]
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
        [PXUIField(DisplayName = "„”«Õ«  «·“Ã«Ã")]
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
        [PXUIField(DisplayName = "«·„—«Ì«  «·Ã«‰»Ì…")]
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
        [PXUIField(DisplayName = "«·„—«Ì«  «·ﬂÂ—»«∆Ì…")]
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
        [PXUIField(DisplayName = "«·√‰Ê«— «·√„«„Ì…")]
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
        [PXUIField(DisplayName = "«·√‰Ê«— «·Œ·›Ì…")]
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
        [PXUIField(DisplayName = "√‰Ê«— «·√‘«—« ")]
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








        //-----------------------------------------

        //#region CarGlass
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Car Glass")]
        //public virtual string CarGlass { get; set; }
        //public abstract class carGlass : PX.Data.BQL.BqlString.Field<carGlass> { }
        //#endregion

        //#region GlassWipers
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Glass Wipers")]
        //public virtual string GlassWipers { get; set; }
        //public abstract class glassWipers : PX.Data.BQL.BqlString.Field<glassWipers> { }
        //#endregion

        //#region SideMirrors
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Side Mirrors")]
        //public virtual string SideMirrors { get; set; }
        //public abstract class sideMirrors : PX.Data.BQL.BqlString.Field<sideMirrors> { }
        //#endregion

        //#region ElectricalMirrors
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Electrical Mirrors")]
        //public virtual string ElectricalMirrors { get; set; }
        //public abstract class electricalMirrors : PX.Data.BQL.BqlString.Field<electricalMirrors> { }
        //#endregion

        //#region HeadLamps
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Head Lamps")]
        //public virtual string HeadLamps { get; set; }
        //public abstract class headLamps : PX.Data.BQL.BqlString.Field<headLamps> { }
        //#endregion

        //#region RearLamps
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Rear Lamps")]
        //public virtual string RearLamps { get; set; }
        //public abstract class rearLamps : PX.Data.BQL.BqlString.Field<rearLamps> { }
        //#endregion

        //#region SignalLamps
        //[PXDBString(1, IsUnicode = true, InputMask = "")]
        //[PXUIField(DisplayName = "Signal Lamps")]
        //public virtual string SignalLamps { get; set; }
        //public abstract class signalLamps : PX.Data.BQL.BqlString.Field<signalLamps> { }
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