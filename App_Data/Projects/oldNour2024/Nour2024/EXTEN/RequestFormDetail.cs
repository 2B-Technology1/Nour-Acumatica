using System;
using PX.Data;
using PX.Objects.IN;
using PX.Data.BQL;

namespace NourSc202007071
{
    [Serializable]
    public class RequestFormDetail : IBqlTable
    {
        #region statusDocType
        public abstract class Doctype : PX.Data.IBqlField
        {
            //Constant declaration
            public const string SparPart = "Spare Parts";
            public const string Warranty = "Warranty";
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { SparPart, Warranty },
                        new string[] { SparPart, Warranty })
                {; }
            }
            public class wararanty :BqlString.Constant<wararanty>
            {
                public wararanty() : base(Warranty) {; }
            }
            public class sparePart :BqlString.Constant<sparePart>
            {
                public sparePart() : base(SparPart) {; }
            }
        }
        #endregion

        #region RequstDetID
        public abstract class requstDetID : PX.Data.IBqlField
        {
        }
        protected int? _RequstDetID;
        [PXDBIdentity(IsKey = true)]
        public virtual int? RequstDetID
        {
            get
            {
                return this._RequstDetID;
            }
            set
            {
                this._RequstDetID = value;
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected string _RefNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXDefault(typeof(RequestForm.refNbr))]
        [PXParent(typeof(
        Select<RequestForm,
        Where<RequestForm.refNbr,
        Equal<Current<RequestFormDetail.refNbr>>>>))]
        public virtual string RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.IBqlField
        {
        }
        protected int? _InventoryID;
        [StockItem(DisplayName = "InventoryID")]
        public virtual int? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region ItemDesc
        public abstract class itemDesc : PX.Data.IBqlField
        {
        }
        protected string _ItemDesc;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<RequestFormDetail.inventoryID>>>>))]
        public virtual string ItemDesc
        {
            get
            {
                return this._ItemDesc;
            }
            set
            {
                this._ItemDesc = value;
            }
        }
        #endregion
        #region ItemClass
        public abstract class itemClass : PX.Data.IBqlField
        {
        }
        protected string _ItemClass;
        [PXDBString(50, IsUnicode = true)]
        [PXDefault(typeof(Search<InventoryItem.postClassID, Where<InventoryItem.inventoryID, Equal<Current<RequestFormDetail.inventoryID>>>>))]
        public virtual string ItemClass
        {
            get
            {
                return this._ItemClass;
            }
            set
            {
                this._ItemClass = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.IBqlField
        {
        }
        protected int? _Qty;
        [PXDBInt()]
        [PXDefault(0)]
        public virtual int? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region ItemType
        public abstract class itemType : PX.Data.IBqlField
        {
        }
        protected string _ItemType;
        [PXDBString(50, IsUnicode = true)]
        [Doctype.List()]
        [PXDefault("Warranty")]
        public virtual string ItemType
        {
            get
            {
                return this._ItemType;
            }
            set
            {
                this._ItemType = value;
            }
        }
        #endregion

        #region PartNbr
        public abstract class partNbr : PX.Data.IBqlField
        {
        }
        protected string _PartNbr;
        [PXDBString(1000, IsUnicode = true)]
        [PXUIField(DisplayName = "Part Nbr")]
        public virtual string PartNbr
        {
            get
            {
                return this._PartNbr;
            }
            set
            {
                this._PartNbr = value;
            }
        }
        #endregion

    }
}