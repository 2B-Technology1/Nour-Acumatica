using System;
using PX.Data;
using PX.Objects.AR;

namespace Nour20241217
{
  [Serializable]
  [PXCacheName("CustomerLoyalty")]
  public class CustomerLoyalty : IBqlTable
  {


        #region CustomerID
        [PXDBInt(IsKey = true)]
      //  [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Customer ID")]
        [PXSelector(
            typeof(Search<Customer.bAccountID>),
            typeof(Customer.acctCD),
            typeof(Customer.acctName),
            SubstituteKey = typeof(Customer.acctCD),
            DescriptionField = typeof(Customer.acctName)
        )]
        public virtual int? CustomerID { get; set; }
    public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
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

    #region Tstamp
    [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region LineCntr
    [PXDBInt()]
    [PXUIField(DisplayName = "Line Cntr")]
    public virtual int? LineCntr { get; set; }
    public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
    #endregion
  }
}