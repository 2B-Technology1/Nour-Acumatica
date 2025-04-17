﻿namespace MyMaintaince.LM
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	public class TrafficUnits : PX.Data.IBqlTable
	{
		#region GovID
		public abstract class govID : PX.Data.IBqlField
		{
		}
		protected int? _GovID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Governrate.govID))]
        [PXParent(typeof(Select<Governrate, Where<Governrate.govID, Equal<Current<TrafficUnits.govID>>>>))]
		[PXUIField(DisplayName = "GovID")]
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
		#region TrafficUnitID
		public abstract class trafficUnitID : PX.Data.IBqlField
		{
		}
		protected int? _TrafficUnitID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false)]
		public virtual int? TrafficUnitID
		{
			get
			{
				return this._TrafficUnitID;
			}
			set
			{
				this._TrafficUnitID = value;
			}
		}
		#endregion
		#region TrafficUnitCD
		public abstract class trafficUnitCD : PX.Data.IBqlField
		{
		}
		protected string _TrafficUnitCD;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "TrafficUnit ID")]
		public virtual string TrafficUnitCD
		{
			get
			{
				return this._TrafficUnitCD;
			}
			set
			{
				this._TrafficUnitCD = value;
			}
		}
		#endregion
		#region TRDescr
		public abstract class tRDescr : PX.Data.IBqlField
		{
		}
		protected string _TRDescr;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string TRDescr
		{
			get
			{
				return this._TRDescr;
			}
			set
			{
				this._TRDescr = value;
			}
		}
		#endregion
	}
}
