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
using System.Diagnostics;

namespace PX.Objects.PM
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMAllocationSourceTran)]
	[DebuggerDisplay("Rate={Rate} Qty={Qty} Amount={Amount} Description={Description}")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMAllocationSourceTran : PX.Data.IBqlTable
	{
		#region AllocationID
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID> { }
		protected String _AllocationID;
		[PXDBString(PMAllocation.allocationID.Length, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "AllocationID")]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region StepID
		public abstract class stepID : PX.Data.BQL.BqlInt.Field<stepID> { }
		protected Int32? _StepID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? StepID
		{
			get
			{
				return this._StepID;
			}
			set
			{
				this._StepID = value;
			}
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID> { }
		protected Int64? _TranID;
		[PXDBLong(IsKey = true)]
		[PXDBChildIdentity(typeof(PMTran.tranID))]
		public virtual Int64? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region Rate
		public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
		protected decimal? _Rate;
		[PXDBDecimal(19)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Rate
		{
			get
			{
				return this._Rate;
			}
			set
			{
				this._Rate = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBDecimal(19)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		protected Decimal? _Amount;
		[PXDBDecimal(19)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? Amount
		{
			get
			{
				return this._Amount;
			}
			set
			{
				this._Amount = value;
			}
		}
		#endregion

		#region System Columns
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
		[PXDBCreatedByScreenID()]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedByScreenID()]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime()]
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
		#endregion
	}
}
