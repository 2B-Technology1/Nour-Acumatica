/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;

namespace PX.Objects.AM
{
	/// <summary>
	/// Maintenance table for machines tied to a work center on the Work Centers (AM207000) form (corresponding to the <see cref="WCMaint"/> graph).
	/// Parent: <see cref="AMWC"/>, <see cref="AMMach"/>
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.WcMachines)]
	public class AMWCMach : IBqlTable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMWCMach>.By<machID, wcID>
		{
			public static AMWCMach Find(PXGraph graph, string machID, string wcID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, machID, wcID, options);
		}

		public static class FK
		{
			public class Machine : AMMach.PK.ForeignKeyOf<AMWCMach>.By<machID> { }
			public class WorkCenter : AMWC.PK.ForeignKeyOf<AMWCMach>.By<wcID> { }
		}
		#endregion

		#region MachID
		public abstract class machID : PX.Data.BQL.BqlString.Field<machID> { }

		protected String _MachID;
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Machine ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AMMach.machID>))]
		[PXForeignReference(typeof(Field<machID>.IsRelatedTo<AMMach.machID>))]
		public virtual String MachID
		{
			get
			{
				return this._MachID;
			}
			set
			{
				this._MachID = value;
			}
		}
		#endregion
		#region wcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
		[WorkCenterIDField(IsKey = true, Visible = false, Enabled = false)]
		[PXDBDefault(typeof(AMWC.wcID))]
		[PXParent(typeof(Select<AMWC, Where<AMWC.wcID, Equal<Current<AMWCMach.wcID>>>>))]
		public virtual String WcID
		{
			get
			{
				return this._WcID;
			}
			set
			{
				this._WcID = value;
			}
		}
		#endregion
		#region MachAcctID
		public abstract class machAcctID : PX.Data.BQL.BqlInt.Field<machAcctID> { }

		protected Int32? _MachAcctId;
		[Account(DisplayName = "Machine Account")]
		public virtual Int32? MachAcctID
		{
			get
			{
				return this._MachAcctId;
			}
			set
			{
				this._MachAcctId = value;
			}
		}
		#endregion
		#region MachSubID
		public abstract class machSubID : PX.Data.BQL.BqlInt.Field<machSubID> { }

		protected Int32? _MachSubId;
		[SubAccount(typeof(AMWCMach.machAcctID), DisplayName = "Machine Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? MachSubID
		{
			get
			{
				return this._MachSubId;
			}
			set
			{
				this._MachSubId = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region StdCost
		[Obsolete("This field is obsolete and will be removed in later Acumatica versions. Use AMWCMachCury.stdCost")]
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

		protected Decimal? _StdCost;
		[Obsolete("This field is obsolete and will be removed in later Acumatica versions. Use AMWCMachCury.StdCost")]
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Standard Cost")]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region MachineOverride
		/// <summary>
		/// Indicates if the machine record will be used (false value) or should the workcenter machine record be used as overriding values (true value)
		/// </summary>
		public abstract class machineOverride : PX.Data.BQL.BqlBool.Field<machineOverride> { }

		protected Boolean? _MachineOverride;
		/// <summary>
		/// Indicates if the machine record will be used (false value) or should the workcenter machine record be used as overriding values (true value)
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Machine Override")]
		public virtual Boolean? MachineOverride
		{
			get
			{
				return this._MachineOverride;
			}
			set
			{
				this._MachineOverride = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// Projection of the AMWCCurySettings table that contains cost information for a machine on a work center.
	/// </summary>
	[Serializable]
	[PXCacheName("AMWCMachCury")]
	[PXProjection(typeof(SelectFrom<AMWCCurySettings>
	.Where<AMWCCurySettings.detailID.IsNotEqual<DetailType.wc>>), Persistent = true)]
	public class AMWCMachCury : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<AMWCMachCury>.By<detailID, wcID, curyID>
		{
			public static AMWCMachCury Find(PXGraph graph, string wcID, string detailID, string curyID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, wcID, detailID, curyID, options);
		}
		public static class FK
		{
			public class WCMach : AMWCMach.PK.ForeignKeyOf<AMWCMachCury>.By<detailID, wcID> { }
		}
		#endregion


		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
		[WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Enabled = false, BqlField = typeof(AMWCCurySettings.wcID))]
		[PXDBDefault(typeof(AMWCMach.wcID))]
		[PXParent(typeof(FK.WCMach))]
		public virtual String WcID
		{
			get
			{
				return this._WcID;
			}
			set
			{
				this._WcID = value;
			}
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlString.Field<detailID> { }

		protected String _DetailID;
		[PXDBString(30, IsKey = true, BqlField = typeof(AMWCCurySettings.detailID))]
		[PXUIField(DisplayName = "Detail", Enabled = false)]
		[PXDBDefault(typeof(AMWCMach.machID))]
		public virtual String DetailID
		{
			get
			{
				return this._DetailID;
			}
			set
			{
				this._DetailID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		protected String _CuryID;
		[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(AMWCCurySettings.curyID))]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
		[PXSelector(typeof(Search<CurrencyList.curyID>))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

		protected Decimal? _StdCost;
		[PXDBPriceCost(BqlField = typeof(AMWCCurySettings.stdCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Standard Cost")]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp(BqlField = typeof(AMWCCurySettings.Tstamp))]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID(BqlField = typeof(AMWCCurySettings.createdByID))]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(AMWCCurySettings.createdByScreenID))]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(AMWCCurySettings.createdDateTime))]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(AMWCCurySettings.lastModifiedByID))]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(AMWCCurySettings.lastModifiedByScreenID))]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(AMWCCurySettings.lastModifiedDateTime))]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}
}
