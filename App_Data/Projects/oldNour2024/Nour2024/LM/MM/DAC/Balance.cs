﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	public class Balance : PX.Data.IBqlTable
	{
		#region BalanceID
		public abstract class balanceID : PX.Data.IBqlField
		{
		}
		protected int? _BalanceID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? BalanceID
		{
			get
			{
				return this._BalanceID;
			}
			set
			{
				this._BalanceID = value;
			}
		}
		#endregion
		#region JoborderID
		public abstract class joborderID : PX.Data.IBqlField
		{
		}
		protected string _JoborderID;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
		public virtual string JoborderID
		{
			get
			{
				return this._JoborderID;
			}
			set
			{
				this._JoborderID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected int? _InventoryID;
		[PXDefault]
		[StockItem(DisplayName="inventory ID")]
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
		#region Name
		public abstract class name : PX.Data.IBqlField
		{
		}
		protected string _Name;
		[PXDBString(200, IsUnicode = true)]
		public virtual string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.IBqlField
		{
		}
		protected float? _Qty;
		[PXDBFloat(4)]
		public virtual float? Qty
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
	}
}
