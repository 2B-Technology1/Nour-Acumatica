using System;
using PX.Data;
using PX.Data.BQL;

namespace Nour20240918
{
  [Serializable]
  [PXCacheName("LoyaltyTiers")]
  public class LoyaltyTiers : IBqlTable
  {
    #region LoyalityTierID
    [PXUIField(DisplayName = "TierID")]
    [PXSelector(
        typeof(loyaltyTierID), // DAC field to be used for selection
        typeof(loyaltyTierCD), // Field to display in the selector
        typeof(description), // Additional column to show in the dropdown
        DescriptionField = typeof(description),
        SubstituteKey = typeof(loyaltyTierCD)) // Field for the description tooltip
    ]
    [PXDBIdentity(IsKey = true)]
    public virtual int? LoyaltyTierID { get; set; }
    public abstract class loyaltyTierID : PX.Data.BQL.BqlInt.Field<loyaltyTierID> { }
        #endregion

    #region LoyaltyTierCD    
    [PXDBString(15, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Tier Name", Required = true)]
    [PXDefault]
    public virtual string LoyaltyTierCD { get; set; }
    public abstract class loyaltyTierCD : PX.Data.BQL.BqlString.Field<loyaltyTierCD> { }
    #endregion

    #region Description
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Description")]
    public virtual string Description { get; set; }
    public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        #endregion

    #region LineCntr
    [PXDBInt()]
    [PXDefault(0)]
    [PXUIField(DisplayName = "Line Cntr")]
    public virtual int? LineCntr { get; set; }
    public abstract class lineCntr : BqlInt.Field<lineCntr> { }
    #endregion

    #region SubscribtionExpirationTimeInMonths
    [PXDBInt()]
    [PXUIField(DisplayName = "Subscribtion Expiration Time In Months")]
    public virtual int? SubscribtionExpirationTimeInMonths { get; set; }
    public abstract class subscribtionExpirationTimeInMonths : PX.Data.BQL.BqlInt.Field<subscribtionExpirationTimeInMonths> { }
    #endregion

    #region PoinsExpirationTimeInMonths
    [PXDBInt()]
    [PXUIField(DisplayName = "Poins Expiration Time In Months")]
    public virtual int? PoinsExpirationTimeInMonths { get; set; }
    public abstract class poinsExpirationTimeInMonths : PX.Data.BQL.BqlInt.Field<poinsExpirationTimeInMonths> { }
    #endregion

    #region EarnedPointsPercent
    [PXDBInt()]
    [PXUIField(DisplayName = "Earned Points Percent")]
    public virtual int? EarnedPointsPercent { get; set; }
    public abstract class earnedPointsPercent : PX.Data.BQL.BqlInt.Field<earnedPointsPercent> { }
    #endregion

    #region MaximumPointsEarnedInOrder
    [PXDBInt()]
    [PXUIField(DisplayName = "Maximum Points Earned In Order")]
    public virtual int? MaximumPointsEarnedInOrder { get; set; }
    public abstract class maximumPointsEarnedInOrder : PX.Data.BQL.BqlInt.Field<maximumPointsEarnedInOrder> { }
    #endregion

    #region ActivationTimeInDays
    [PXDBInt()]
    [PXUIField(DisplayName = "Activation Time In Days")]
    public virtual int? ActivationTimeInDays { get; set; }
    public abstract class activationTimeInDays : PX.Data.BQL.BqlInt.Field<activationTimeInDays> { }
    #endregion

    #region MinimumPointsEarnedInOrder
    [PXDBInt()]
    [PXUIField(DisplayName = "Minimum Points Earned In Order")]
    public virtual int? MinimumPointsEarnedInOrder { get; set; }
    public abstract class minimumPointsEarnedInOrder : PX.Data.BQL.BqlInt.Field<minimumPointsEarnedInOrder> { }
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
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region NoteID
    [PXNote()]
    public virtual Guid? NoteID { get; set; }
    public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
    #endregion

    }
}