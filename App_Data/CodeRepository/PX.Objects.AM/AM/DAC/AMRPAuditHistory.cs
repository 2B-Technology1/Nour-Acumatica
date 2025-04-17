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
using PX.Data;

namespace PX.Objects.AM
{
	/// <summary>
	/// Result of processing Regenerate Inventory Planning (AM50500) and the information captured in the grid of this screen from <see cref="AMRPAuditTable"/>.
	/// </summary>
	[Serializable]
	[PXCacheName("Inventory Planning Audit History")]
	public class AMRPAuditHistory : IBqlTable
	{
		#region Recno
		public abstract class recno : PX.Data.BQL.BqlInt.Field<recno> { }

		protected Int32? _Recno;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false, Visible = false)]
		public virtual Int32? Recno
		{
			get
			{
				return this._Recno;
			}
			set
			{
				this._Recno = value;
			}
		}
		#endregion
		#region MsgText
		public abstract class msgText : PX.Data.BQL.BqlString.Field<msgText> { }

		protected String _MsgText;
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Message", Enabled = false)]
		public virtual String MsgText
		{
			get
			{
				return this._MsgText;
			}
			set
			{
				this._MsgText = value;
			}
		}
		#endregion
		#region MsgType
		public abstract class msgType : PX.Data.BQL.BqlInt.Field<msgType> { }

		protected int? _MsgType;
		[PXDBInt]
		[PXDefault(AMRPAuditTable.MsgTypes.Default)]
		[AMRPAuditTable.MsgTypes.List]
		[PXUIField(DisplayName = "Message Type", Enabled = false, Visible = false)]
		public virtual int? MsgType
		{
			get
			{
				return this._MsgType;
			}
			set
			{
				this._MsgType = value;
			}
		}
		#endregion
		#region ProcessID

		public abstract class processID : PX.Data.BQL.BqlGuid.Field<processID> { }

		protected Guid? _ProcessID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Process ID", Enabled = false, Visible = false)]
		public virtual Guid? ProcessID
		{
			get
			{
				return this._ProcessID;
			}
			set
			{
				this._ProcessID = value;
			}
		}
		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		protected Guid? _CreatedByID;
		[PXDBCreatedByID(Visible = false)]
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
		[PXUIField(DisplayName = "Created By (Screen ID)", Enabled = false, Visible = false)]
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
		[PXDBDateAndTime(UseTimeZone = true, DisplayNameDate = "Created At (Date)", DisplayNameTime = "Created At (Time)")]
		[PXUIField(DisplayName = "Created At", Visible = true, Enabled = false)]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp()]
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
	}
}
