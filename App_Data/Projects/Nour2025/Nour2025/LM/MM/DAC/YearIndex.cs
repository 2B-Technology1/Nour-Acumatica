﻿namespace Maintenance.MM
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class YearIndex : PX.Data.IBqlTable
	{
		#region id
		public abstract class Id : PX.Data.IBqlField
		{
		}
		protected int? _id;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		#endregion
		#region YearID
		public abstract class yearID : PX.Data.IBqlField
		{
		}
		protected int? _YearID;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "YearID")]
		public virtual int? YearID
		{
			get
			{
				return this._YearID;
			}
			set
			{
				this._YearID = value;
			}
		}
		#endregion
	}
}
