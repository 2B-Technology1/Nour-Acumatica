namespace Maintenance
{
    using System;
    using PX.Data;
    using PX.Objects.AP;
    using PX.Objects.AR;
    using PX.Objects.CA;
    using PX.Objects.CS;

    [System.SerializableAttribute()]
    public class AutoSequence : PX.Data.IBqlTable
    {
        #region Type
        public abstract class type : PX.Data.IBqlField
        {
        }
        protected string _Type;
        [PXDBString(50, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Type")]
        [PXStringList(new string[]{ARPaymentType.Payment,
                                   ARPaymentType.Prepayment,
                                   ARPaymentType.Refund,
                                   ARPaymentType.VoidPayment,
                                   APPaymentType.Check,
                                   APPaymentType.VoidCheck,
                                   "Transaction"},

                      new string[]{"Payment",
                                   "Prepayment",
                                   "Refund",
                                   "Void Payment",
                                   "Check",
                                   "Void Check",
                                   "Transaction"})]
        public virtual string Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }
        #endregion
        #region CashAccountID
        public abstract class cashAccountID : PX.Data.IBqlField
        {
        }
        protected int? _CashAccountID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Cash Account")]
        [PXSelector(typeof(Search<CashAccount.cashAccountID>),
                        new Type[]
                {
                    typeof(CashAccount.cashAccountCD),
                    typeof(CashAccount.descr),
                    typeof(CashAccount.curyID),
                },
                        SubstituteKey = typeof(CashAccount.cashAccountCD))]
        public virtual int? CashAccountID
        {
            get
            {
                return this._CashAccountID;
            }
            set
            {
                this._CashAccountID = value;
            }
        }
        #endregion
        #region Sequence
        public abstract class sequence : PX.Data.IBqlField
        {
        }
        protected string _Sequence;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Sequence")]
        [PXSelector(typeof(Numbering.numberingID))]
        public virtual string Sequence
        {
            get
            {
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
            }
        }
        #endregion
        #region Vendor
        public abstract class vendor : PX.Data.IBqlField
        {
        }
        protected bool? _Vendor;
        [PXDBBool(IsKey = true)]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Vendor")]
        public virtual bool? Vendor
        {
            get
            {
                return this._Vendor;
            }
            set
            {
                this._Vendor = value;
            }
        }
        #endregion
        #region EntryType
        public abstract class entryType : PX.Data.IBqlField
        {
        }
        protected string _EntryType;
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDefault("NONE")]
        [PXUIField(DisplayName = "Entry Type")]
        [PXSelector(typeof(Search2<CAEntryType.entryTypeId, InnerJoin<CashAccountETDetail, On<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>
            // , Where<CashAccountETDetail.accountID, Equal<Current<AutoSequence.cashAccountID>>>
            >))]
        public virtual string EntryType
        {
            get
            {
                return this._EntryType;
            }
            set
            {
                this._EntryType = value;
            }
        }
        #endregion
    }
}