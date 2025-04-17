using System;
using Nour20240918;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace Nour20240918
{
    [Serializable]
    [PXCacheName("LoyaltyTierRule")]
    public class LoyaltyTierRule : IBqlTable
    {
        #region LoyaltyTierID
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(LoyaltyTiers.loyaltyTierID))]
        [PXParent(typeof(SelectFrom<LoyaltyTiers>.
            Where<LoyaltyTiers.loyaltyTierID
                .IsEqual<LoyaltyTierRule.loyaltyTierID.FromCurrent>>))]

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Loyalty Tier ID")]

        public virtual int? LoyaltyTierID { get; set; }
        public abstract class loyaltyTierID : PX.Data.BQL.BqlInt.Field<loyaltyTierID> { }
        #endregion

        #region LoyaltyRuleID
        [PXSelector(
            typeof(LoyaltyRules.loyaltyRuleID), // DAC field to be used for selection
            typeof(LoyaltyRules.loyaltyRuleCD), // Field to display in the selector
            SubstituteKey = typeof(LoyaltyRules.loyaltyRuleCD),
            DescriptionField = typeof(LoyaltyRules.loyaltyRuleCD)) // Field for the description tooltip
        ]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Loyalty Rule ID")]
        public virtual int? LoyaltyRuleID { get; set; }
        public abstract class loyaltyRuleID : PX.Data.BQL.BqlInt.Field<loyaltyRuleID> { }
        #endregion
        #region Logical Operator
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Logical Operator", Required = true)]
        [PXDefault("-")]
        [PXStringList(
            new string[] { "-", "AND", "OR" },
            new string[] { "-", "AND", "OR" }
        )]
        public virtual string LogicalOperator { get; set; }
        public abstract class logicalOperator : PX.Data.BQL.BqlString.Field<logicalOperator> { }
        #endregion

        #region LineNbr
        [PXDefault]
        [PXLineNbr(typeof(LoyaltyTiers.lineCntr))]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr")]
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