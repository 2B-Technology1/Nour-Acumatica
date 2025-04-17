using System;
using MyMaintaince;
using Nour20220913V1;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20231012VSolveUSDNew
{
  [Serializable]
  [PXCacheName("InspectionMalfunctionCheck")]
  public class InspectionMalfunctionCheck : IBqlTable
  {


        #region InspectionFormNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Inspection Form Nbr")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(InspectionFormMaster.inspectionFormNbr))]
        [PXParent(typeof(SelectFrom<InspectionFormMaster>.
                              Where<InspectionFormMaster.inspectionFormNbr.
                            IsEqual<InspectionMalfunctionCheck.inspectionFormNbr.FromCurrent>>))]
        public virtual string InspectionFormNbr { get; set; }
        public abstract class inspectionFormNbr : PX.Data.BQL.BqlString.Field<inspectionFormNbr> { }
        #endregion


        #region CarControlSysTest
        [PXDBString(1, InputMask = "")]
        [PXUIField(DisplayName = "√Œ »«— ‰Ÿ«„  Õﬂ„ «·”Ì«—…")]
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









    //    //---------------------------------------------
    //    #region CarControlSysTest
    //    [PXDBString(1, IsFixed = true, InputMask = "")]
    //[PXUIField(DisplayName = "Car Control Sys Test")]
    //public virtual string CarControlSysTest { get; set; }
    //public abstract class carControlSysTest : PX.Data.BQL.BqlString.Field<carControlSysTest> { }
    //    #endregion







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