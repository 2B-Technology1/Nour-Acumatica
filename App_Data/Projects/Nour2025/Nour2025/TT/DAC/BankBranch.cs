﻿namespace Maintenance
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class BankBranch : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected string _BranchID;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "BranchID")]
		public virtual string BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region BranchName
		public abstract class branchName : PX.Data.IBqlField
		{
		}
		protected string _BranchName;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "BranchName")]
		public virtual string BranchName
		{
			get
			{
				return this._BranchName;
			}
			set
			{
				this._BranchName = value;
			}
		}
		#endregion
	}
}
