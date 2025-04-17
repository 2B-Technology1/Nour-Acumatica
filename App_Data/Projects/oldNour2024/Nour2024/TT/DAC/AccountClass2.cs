﻿namespace Maintenance
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class AccountClass2 : PX.Data.IBqlTable
	{
		#region AccountClassID
		public abstract class accountClassID : PX.Data.IBqlField
		{
		}
		protected string _AccountClassID;
		[PXDBString(15, IsUnicode = true,IsKey=true)]
		[PXUIField(DisplayName = "AccountClassID")]
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected string _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
	}
}
