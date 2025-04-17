﻿namespace MyMaintaince
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class JobTime : PX.Data.IBqlTable
	{
		#region code
		public abstract class Code : PX.Data.IBqlField
		{
		}
		protected string _code;
		[PXDBString(50, IsKey = true, IsUnicode = true,InputMask=">aaaaaaaaaaaaaaaaaaaaaa")]
		[PXDefault()]
        [PXSelector(typeof(JobTime.Code)
                    ,new Type[]{
                      typeof(JobTime.Code),
                      typeof(JobTime.name)
                    }
                    , DescriptionField = typeof(JobTime.name))]
        [PXUIField(DisplayName="Code")]
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
