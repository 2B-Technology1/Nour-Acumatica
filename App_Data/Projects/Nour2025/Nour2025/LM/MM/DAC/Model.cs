﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Model : PX.Data.IBqlTable
	{
        #region BrandID
        public abstract class brandID : PX.Data.IBqlField
        {
        }
        protected int? _BrandID;
        [PXDBInt()]
        [PXDBDefault(typeof(Brand.brandID))]
        [PXParent(typeof(
        Select<Brand,
        Where<Brand.brandID,
        Equal<Current<Model.brandID>>>>))]

        public virtual int? BrandID
        {
            get
            {
                return this._BrandID;
            }
            set
            {
                this._BrandID = value;
            }
        }
        #endregion
		#region ModelID
		public abstract class modelID : PX.Data.IBqlField
		{
		}
		protected int? _ModelID;
		[PXDBIdentity(IsKey = true)]
		public virtual int? ModelID
		{
			get
			{
				return this._ModelID;
			}
			set
			{
				this._ModelID = value;
			}
		}
		#endregion
		#region Code
		public abstract class code : PX.Data.IBqlField
		{
		}
		protected string _Code;
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Code")]
		public virtual string Code
		{
			get
			{
				return this._Code;
			}
			set
			{
				this._Code = value;
			}
		}
		#endregion
		#region Name
		public abstract class name : PX.Data.IBqlField
		{
		}
		protected string _Name;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Name")]
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
		
	}
}
