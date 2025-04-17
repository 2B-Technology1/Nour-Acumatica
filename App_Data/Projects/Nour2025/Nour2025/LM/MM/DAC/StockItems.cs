﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	using PX.SM;
	using PX.Objects;
	using PX.Objects.CS;
	using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	public class StockItems : PX.Data.IBqlTable
	{
        #region OperationId
        public abstract class operationId : PX.Data.IBqlField
        {
        }
        protected int? _OperationId;
        [PXDBInt()]
        [PXDBDefault(typeof(Operation.oPerationID))]
        [PXParent(typeof(
        Select<Operation,
        Where<Operation.oPerationID,
        Equal<Current<StockItems.operationId>>>>))]
        public virtual int? OperationId
        {
            get
            {
                return this._OperationId;
            }
            set
            {
                this._OperationId = value;
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
		[StockItem(DisplayName = "Inventory ID")]
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
        [PXUIField(DisplayName="Inventory Desc")]
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
		#region Quantity
		public abstract class quantity : PX.Data.IBqlField
		{
		}
		protected float? _Quantity;
		[PXDBFloat(4)]
		[PXDefault((float)0)]
        [PXUIField(DisplayName = "Qty")]
		public virtual float? Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				this._Quantity = value;
			}
		}
		#endregion
        //poo :not used
		#region OperationCode
		public abstract class operationCode : PX.Data.IBqlField
		{
		}
		protected string _OperationCode;
		[PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(Operation.operationCode))]
		public virtual string OperationCode
		{
			get
			{
				return this._OperationCode;
			}
			set
			{
				this._OperationCode = value;
			}
		}
		#endregion
	}
}
