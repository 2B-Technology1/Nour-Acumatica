﻿namespace MyMaintaince.SN
{
	using System;
	using PX.Data;
    using PX.Objects.IN;
	
	[System.SerializableAttribute()]
	public class InventoryWarehouseSerials : PX.Data.IBqlTable
	{
		#region ID
		public abstract class iD : PX.Data.IBqlField
		{
		}
		protected int? _ID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? ID
		{
			get
			{
				return this._ID;
			}
			set
			{
				this._ID = value;
			}
		}
		#endregion
		#region Warehouse
		public abstract class warehouse : PX.Data.IBqlField
		{
		}
		protected int? _Warehouse;
		[PXDBInt()]
		[PXUIField(DisplayName = "Warehouse")]
        [PXSelector(typeof(INSite.siteID)
            , new Type[]{
                      typeof(INSite.siteCD),
                      typeof(INSite.descr) 
                    }
            , SubstituteKey = typeof(INSite.siteCD)
            , DescriptionField = typeof(INSite.descr))]  
		public virtual int? Warehouse
		{
			get
			{
				return this._Warehouse;
			}
			set
			{
				this._Warehouse = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected string _TranType;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "TranType")]
        [PXStringList(new String[] { "I", "R" }, new String[] { "Issue", "Receipt" })]
		public virtual string TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region LastRefNbr
		public abstract class lastRefNbr : PX.Data.IBqlField
		{
		}
		protected int? _LastRefNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "LastRefNbr")]
		public virtual int? LastRefNbr
		{
			get
			{
				return this._LastRefNbr;
			}
			set
			{
				this._LastRefNbr = value;
			}
		}
		#endregion
	}
}
