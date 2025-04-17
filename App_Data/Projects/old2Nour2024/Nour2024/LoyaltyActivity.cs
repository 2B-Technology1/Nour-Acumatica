using System;
using PX.Data;
using static PX.Data.BQL.BqlPlaceholder;

namespace Nour20241217
{
  [Serializable]
  [PXCacheName("LoyaltyActivity")]
  public class LoyaltyActivity : IBqlTable
  {
    #region LoyaltyActivityID
    [PXDBIdentity]
    public virtual int? LoyaltyActivityID { get; set; }
    public abstract class loyaltyActivityID : PX.Data.BQL.BqlInt.Field<loyaltyActivityID> { }
    #endregion

    #region CustomerID
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Customer ID")]
    public virtual int? CustomerID { get; set; }
    public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
    #endregion

    #region TransactionID
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Transaction ID")]
    public virtual string TransactionID { get; set; }
    public abstract class transactionID : PX.Data.BQL.BqlString.Field<transactionID> { }
    #endregion

    #region TransactionDate
    [PXDBDate()]
    [PXUIField(DisplayName = "Transaction Date")]
    public virtual DateTime? TransactionDate { get; set; }
    public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate> { }
    #endregion

    #region TransactionType
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Transaction Type")]
    public virtual string TransactionType { get; set; }
    public abstract class transactionType : PX.Data.BQL.BqlString.Field<transactionType> { }
    #endregion

    #region OrderInvoiceReference
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Order Invoice Reference")]
    public virtual string OrderInvoiceReference { get; set; }
    public abstract class orderInvoiceReference : PX.Data.BQL.BqlString.Field<orderInvoiceReference> { }
    #endregion  

    #region PointsEarned
    [PXDBInt()]
    [PXUIField(DisplayName = "Points Earned")]
    public virtual int? PointsEarned { get; set; }
    public abstract class pointsEarned : PX.Data.BQL.BqlInt.Field<pointsEarned> { }
        #endregion

    #region ConsumedPoints
    [PXDBInt()]
    [PXUIField(DisplayName = "Points Earned")]
    public virtual int? ConsumedPoints { get; set; }
    public abstract class consumedPoints : PX.Data.BQL.BqlInt.Field<consumedPoints> { }
    #endregion

    #region RemainingPoints
    [PXDBInt()]
    [PXUIField(DisplayName = "Points Earned")]
    public virtual int? RemainingPoints { get; set; }
    public abstract class remainingPoints : PX.Data.BQL.BqlInt.Field<remainingPoints> { }
    #endregion


    #region Type
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXStringList(
        new string[] { "Greeting Points","Referal Points", "Earned Points" }, 
        new string[] { "Greeting Points", "Referal Points", "Earned Points" }
        )]
    [PXUIField(DisplayName = "Status")]
    public virtual string Type { get; set; }
    public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
    #endregion



    #region ExpirationDate
    [PXDBDate()]
    [PXUIField(DisplayName = "Expiration Date")]
    public virtual DateTime? ExpirationDate { get; set; }
    public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
    #endregion

    #region ActivationDate
    [PXDBDate()]
    [PXUIField(DisplayName = "Activation Date")]
    public virtual DateTime? ActivationDate { get; set; }
    public abstract class activationDate : PX.Data.BQL.BqlDateTime.Field<activationDate> { }
    #endregion

    #region Status
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Status")]
    [PXStringList(new string[] { "Active", "Redeemed", "InActive","Expired" }, 
                  new string[] { "Active", "Redeemed", "InActive", "Expired"}
                  )]

    public virtual string Status { get; set; }
    public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
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

    #region LineNbr
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Line Nbr")]
    public virtual int? LineNbr { get; set; }
    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
    #endregion
  }
}