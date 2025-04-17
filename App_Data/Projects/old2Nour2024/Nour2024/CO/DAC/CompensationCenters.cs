﻿namespace Maintenance.CO
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class CompensationCenters : PX.Data.IBqlTable
	{
		#region CenterID
		public abstract class centerID : PX.Data.IBqlField
		{
		}
		protected int? _CenterID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? CenterID
		{
			get
			{
				return this._CenterID;
			}
			set
			{
				this._CenterID = value;
			}
		}
		#endregion
		#region CenterCD
		public abstract class centerCD : PX.Data.IBqlField
		{
		}
		protected string _CenterCD;
		[PXDBString(50,IsKey=true ,IsUnicode = true)]
		[PXUIField(DisplayName = "CenterCD")]
		public virtual string CenterCD
		{
			get
			{
				return this._CenterCD;
			}
			set
			{
				this._CenterCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected string _Descr;
		[PXDBString(300, IsUnicode = true)]
		[PXUIField(DisplayName = "Descr")]
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
