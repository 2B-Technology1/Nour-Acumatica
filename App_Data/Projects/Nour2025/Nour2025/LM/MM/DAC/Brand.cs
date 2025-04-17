﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Brand : PX.Data.IBqlTable
	{
		#region BrandID
		public abstract class brandID : PX.Data.IBqlField
		{
		}
		protected int? _BrandID;
		[PXDBIdentity()]
		[PXUIField(DisplayName="Brand Nbr")]
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
		#region code
		public abstract class Code : PX.Data.IBqlField
		{
		}
		protected string _code;
        [PXDBString(50, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaa")]
		[PXDefault("")]
		[PXSelector(typeof(Brand.Code),
                    new Type[] { typeof(Brand.Code),typeof(Brand.name) }
                    , DescriptionField = typeof(Brand.name))]
        [PXUIField(DisplayName = "Code")]
		public virtual string code
		{
			get
			{
				return this._code;
			}
			set
			{
				this._code = value;
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
