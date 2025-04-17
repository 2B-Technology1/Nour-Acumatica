﻿namespace Maintenance
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class UserCurrent : PX.Data.IBqlTable
	{
		#region UserID
		public abstract class userID : PX.Data.IBqlField
		{
		}
		protected Guid? _UserID;
		[PXDBField()]
		[PXUIField(DisplayName = "UserID")]
		public virtual Guid? UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}
		#endregion
	}
}
