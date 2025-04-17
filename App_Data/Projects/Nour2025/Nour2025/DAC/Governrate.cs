﻿namespace MyMaintaince.LM
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class Governrate : PX.Data.IBqlTable
	{
		#region GovID
		public abstract class govID : PX.Data.IBqlField
		{
		}
		protected int? _GovID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? GovID
		{
			get
			{
				return this._GovID;
			}
			set
			{
				this._GovID = value;
			}
		}
		#endregion
		#region GovCD
		public abstract class govCD : PX.Data.IBqlField
		{
		}
		protected string _GovCD;
		[PXDBString(50,IsKey=true ,IsUnicode = true)]
		[PXUIField(DisplayName = "Gov ID")]
        [PXSelector(typeof(Governrate.govCD))]
		public virtual string GovCD
		{
			get
			{
				return this._GovCD;
			}
			set
			{
				this._GovCD = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected string _Descr;
		[PXDBString(50, IsUnicode = true)]
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
