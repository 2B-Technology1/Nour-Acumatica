﻿namespace MyMaintaince
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using PX.Data;
	using PX.SM;
	using PX.Objects;
	using PX.Objects.CS;
	using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	public class JobOrderNonStockItems : PX.Data.IBqlTable
	{
        #region JobOrdrID
        public abstract class jobOrdrID : PX.Data.IBqlField
        {
        }
        protected string _JobOrdrID;
        [PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(JobOrder.jobOrdrID))]
        [PXUIField(DisplayName = "jobOrdr ID")]
        [PXParent(typeof(Select<JobOrder, Where<JobOrder.jobOrdrID, Equal<Current<JobOrderNonStockItems.jobOrdrID>>>>))]
        public virtual string JobOrdrID
        {
            get
            {
                return this._JobOrdrID;
            }
            set
            {
                this._JobOrdrID = value;
            }
        }
        #endregion
        #region StockItemID  means just "Line ID" not the InventoryID
        public abstract class stockItemID : PX.Data.IBqlField
        {
        }
        protected int? _StockItemID;
        [PXDBIdentity(IsKey = true)]
        public virtual int? StockItemID
        {
            get
            {
                return this._StockItemID;
            }
            set
            {
                this._StockItemID = value;
            }
        }
        #endregion
        //Poo : Code And Name Changed To InventoryCode and InventoryName
        #region Code >> InventoryCode
        public abstract class inventoryCode : PX.Data.IBqlField
        {
        }
        protected int? _InventoryCode;
        [NonStockItem(DisplayName = "Inventory ID")]
        [PXUIField(DisplayName = "Inventory ID")]
        [PXDefault()]
        //poo
        /**
        [PXSelector(typeof(Search<InventoryItem.inventoryID,Where<InventoryItem.stkItem,Equal<True>>>)
                   ,new Type[]{
                      typeof(InventoryItem.inventoryCD),
                      typeof(InventoryItem.descr),
                      typeof(InventoryItem.itemType),
                      typeof(InventoryItem.itemClassID),
                      typeof(InventoryItem.itemStatus),
                      typeof(InventoryItem.baseUnit),
                      typeof(InventoryItem.salesUnit),
                      typeof(InventoryItem.purchaseUnit),
                   }
                   , DescriptionField = typeof(InventoryItem.descr)
                   , SubstituteKey = typeof(InventoryItem.inventoryCD))]
		
         * */
        public virtual int? InventoryCode
        {
            get
            {
                return this._InventoryCode;
            }
            set
            {
                this._InventoryCode = value;
            }
        }
        #endregion
        #region Name >> InventoryName
        public abstract class inventoryName : PX.Data.IBqlField
        {
        }
        protected string _InventoryName;
        [PXDBString(100, IsUnicode = true)]
        [PXDefault("")]
        [PXUIField(DisplayName = "Inventory Desc")]
        public virtual string InventoryName
        {
            get
            {
                return this._InventoryName;
            }
            set
            {
                this._InventoryName = value;
            }
        }
        #endregion
        #region Time
        public abstract class time : PX.Data.IBqlField
        {
        }
        protected float? _Time;
        [PXDBFloat(2)]
        [PXDefault((float)0)]
        [PXUIField(DisplayName = "Time")]
        public virtual float? Time
        {
            get
            {
                return this._Time;
            }
            set
            {
                this._Time = value;
            }
        }
        #endregion
        #region NonStkPrice
        public abstract class nonStkPrice : PX.Data.IBqlField
        {
        }
        protected decimal? _NonStkPrice;
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Price")]
        public virtual decimal? NonStkPrice
        {
            get
            {
                return this._NonStkPrice;
            }
            set
            {
                this._NonStkPrice = value;
            }
        }
        #endregion
        #region LinePrice
        public abstract class linePrice : PX.Data.IBqlField
        {
        }
        protected decimal? _LinePrice;
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Line Total", IsReadOnly = true)]
        public virtual decimal? LinePrice
        {
            get
            {
                return this._LinePrice;
            }
            set
            {
                this._LinePrice = value;
            }
        }
        #endregion
        #region OpId
        public abstract class opId : PX.Data.IBqlField
        {
        }
        protected int? _OpId;
        [PXDBInt()]
        [PXUIField(DisplayName = "OpId", Visible = false,IsReadOnly = true)]
        public virtual int? OpId
        {
            get
            {
                return this._OpId;
            }
            set
            {
                this._OpId = value;
            }
        }
        #endregion

        #region Virtual Fields Unbounded DAC Fields
        #region OperationName
        public abstract class operationname : PX.Data.IBqlField
        {
        }
        [PXString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Operation Name",IsReadOnly=true)]
        public virtual string OperationName { get; set; }
        #endregion
        #endregion

    }
}
