using System;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20240918
{
  [Serializable]
  [PXCacheName("LoyaltyTierRewards")]
  public class LoyaltyTierReward : IBqlTable
  {
    #region LoyaltyTierID
    [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
    [PXDBDefault(typeof(LoyaltyTiers.loyaltyTierID))]
    [PXParent(typeof(SelectFrom<LoyaltyTiers>.
                        Where<LoyaltyTiers.loyaltyTierID.
                    IsEqual<LoyaltyTierReward.loyaltyTierID.FromCurrent>>))]
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Loyalty TierID")]
    public virtual int? LoyaltyTierID { get; set; }
    public abstract class loyaltyTierID : PX.Data.BQL.BqlInt.Field<loyaltyTierID> { }
    #endregion

    #region LoyaltyTierRewardID
    [PXDBInt(IsKey = true)]
        [PXSelector(
        typeof(Rewards.id), // DAC field to be used for selection
        typeof(Rewards.name), // Field to display in the selector
        typeof(Rewards.points), // Additional column to show in the dropdown
        typeof(Rewards.item),
        SubstituteKey = typeof(Rewards.name), 
        DescriptionField = typeof(Rewards.name)) // Field for the description tooltip
    ]

        [PXUIField(DisplayName = "Loyalty Reward")]
    public virtual int? LoyaltyTierRewardID { get; set; }
    public abstract class loyaltyTierRewardID : PX.Data.BQL.BqlInt.Field<loyaltyTierRewardID> { }
        #endregion

    #region LineNbr
    [PXDBInt(IsKey = true)]
    [PXUIField(DisplayName = "Line Nbr")]
    [PXDefault]
    [PXLineNbr(typeof(LoyaltyTiers.lineCntr))]
    public virtual int? LineNbr { get; set; }
    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
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