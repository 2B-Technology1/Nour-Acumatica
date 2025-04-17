using System;
using PX.Data;

namespace Nour20240918
{
  [Serializable]
  [PXCacheName("LoyaltyRules")]
  public class LoyaltyRules : IBqlTable
  {
    #region LoyaltyRuleID
    [PXDBIdentity(IsKey = true)]
    public virtual int? LoyaltyRuleID { get; set; }
    public abstract class loyaltyRuleID : PX.Data.BQL.BqlInt.Field<loyaltyRuleID> { }
    #endregion

    #region LoyaltyRuleCD
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Rule Name")]
    [PXDefault]
    public virtual string LoyaltyRuleCD { get; set; }
    public abstract class loyaltyRuleCD : PX.Data.BQL.BqlString.Field<loyaltyRuleCD> { }
    #endregion

    #region FieldName
    [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXStringList(
        new string[] { "Profit", "TotalSales" },
        new string[] { "Profit", "TotalSales" }
    )]
    [PXDefault]
    [PXUIField(DisplayName = "Field Name", Required = true)]
    public virtual string FieldName { get; set; }
    public abstract class fieldName : PX.Data.BQL.BqlString.Field<fieldName> { }
    #endregion

    #region Comparison Operator
    [PXDBString(15, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Comparison Operator", Required = true)]
    [PXDefault]
    [PXStringList(
        new string[] { "<", ">",">=", "<=" },
        new string[] { "<", ">",">=", "<=" }
    )]
        public virtual string ComparisonOperator { get; set; }
    public abstract class comparisonOperator : PX.Data.BQL.BqlString.Field<comparisonOperator> { }
    #endregion

    #region Value
    [PXDBInt()]
    [PXDefault]
    [PXUIField(DisplayName = "Value", Required = true)]
    public virtual int? Value { get; set; }
    public abstract class value : PX.Data.BQL.BqlInt.Field<value> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
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

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime()]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
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

    #region Tstamp
    [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region Noteid
    [PXNote()]
    public virtual Guid? Noteid { get; set; }
    public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
    #endregion
  }
}