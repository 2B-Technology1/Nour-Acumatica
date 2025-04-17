using System;
using Nour20240918;
using PX.Data;
using PX.Data.BQL.Fluent;
using static Nour20240918.LoyaltyTierCalculation;
using static Nour2025.Loyalty.Helpers.LoyaltyHelper;

namespace Nour20240918
{
    [Serializable]
    [PXCacheName("LoyaltyTierCalculation")]
    public class LoyaltyTierCalculation : IBqlTable
    {
        #region LoyaltyTierID
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXDBDefault(typeof(LoyaltyTiers.loyaltyTierID))]
        [PXParent(typeof(SelectFrom<LoyaltyTiers>.
            Where<LoyaltyTiers.loyaltyTierID
                .IsEqual<LoyaltyTierCalculation.loyaltyTierID.FromCurrent>>))]

        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Loyalty Tier ID")]

        public virtual int? LoyaltyTierID { get; set; }
        public abstract class loyaltyTierID : PX.Data.BQL.BqlInt.Field<loyaltyTierID> { }
        #endregion


        #region LineNbr
        [PXDefault]
        [PXLineNbr(typeof(LoyaltyTiers.lineCntr))]
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr")]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region PointsFactor
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Points Factor")]
        [PXDefault]
        public virtual decimal? PointsFactor { get; set; }
        public abstract class pointsFactor : PX.Data.BQL.BqlInt.Field<pointsFactor> { }
        #endregion

        #region CalculationField
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Calculation Field")]
        [PXDefault]
        [PXStringList(
            new string[] { CalculationFields.Profit, CalculationFields.TotalSales },
            new string[] { CalculationFields.Profit, CalculationFields.TotalSales }
        )]
        public virtual string CalculationField { get; set; }
        public abstract class calculationField : PX.Data.BQL.BqlString.Field<calculationField> { }
        #endregion

        #region Arithmatic Operator
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Operator", Required = true)]
        [PXDefault("N/A")]
        [PXStringList(
            new string[] { "+", "-", "N/A" },
            new string[] { "+", "-", "N/A" }
        )]
        public virtual string ArithmaticOperator { get; set; }
        public abstract class arithmaticOperator : PX.Data.BQL.BqlString.Field<arithmaticOperator> { }
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