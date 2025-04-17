﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Cls : PX.Data.IBqlTable
	{
		#region ClassID
		public abstract class classID : PX.Data.IBqlField
		{
		}
		protected int? _ClassID;
		[PXDBIdentity()]
		public virtual int? ClassID
		{
			get
			{
				return this._ClassID;
			}
			set
			{
				this._ClassID = value;
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
        [PXSelector(typeof(Cls.Code)
                   ,new Type[] { typeof(Cls.Code), typeof(Cls.Name) }
                   , DescriptionField = typeof(Cls.Name)
                   )]
        [PXUIField(DisplayName = "Class ID")]
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
		#region name
		public abstract class Name : PX.Data.IBqlField
		{
		}
		protected string _name;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault("")]
        [PXUIField(DisplayName = "Class Name")]
		public virtual string name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		#endregion
	}
}
