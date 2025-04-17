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

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Objects.AM
{
	/// <summary>
	/// The maintenance table for transferring lead times between warehouses when the Manufacturing feature is enabled.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
	[Serializable]
	[PXCacheName("Warehouse Transfer")]
	public class AMSiteTransfer : IBqlTable
	{
		internal string DebuggerDisplay => $"[{SiteID}:{TransferSiteID}] TransferLeadTime = {TransferLeadTime}";

		public class PK : PrimaryKeyOf<AMSiteTransfer>.By<siteID, transferSiteID>
		{
			public static AMSiteTransfer Find(PXGraph graph, int? siteID, int? transferSiteID, PKFindOptions options = PKFindOptions.None)
			=> FindBy(graph, siteID, transferSiteID, options);
		}

		public static class FK
		{
			public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMSiteTransfer>.By<siteID> { }
			public class TransferSite : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMSiteTransfer>.By<transferSiteID> { }
		}

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected Int32? _SiteID;
		[PXDBDefault(typeof(INSite.siteID))]
		[PXParent(typeof(Select<INSite,
		Where<INSite.siteID, Equal<Current<AMSiteTransfer.siteID>>>>))]
		[Site(IsKey = true, DescriptionField = typeof(INSite.descr), Enabled = false)]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region TransferSiteID
		public abstract class transferSiteID : PX.Data.BQL.BqlInt.Field<transferSiteID> { }

		protected Int32? _TransferSiteID;
		[PXDefault]
		[Site(IsKey = true, DisplayName = "Replenishment Warehouse", DescriptionField = typeof(INSite.descr))]
		public virtual Int32? TransferSiteID
		{
			get
			{
				return this._TransferSiteID;
			}
			set
			{
				this._TransferSiteID = value;
			}
		}
		#endregion
		#region TransferLeadTime
		public abstract class transferLeadTime : PX.Data.BQL.BqlInt.Field<transferLeadTime> { }

		protected Int32? _TransferLeadTime;
		[PXDefault(0)]
		[PXUIField(DisplayName = "Transfer Lead Time (Days)")]
		[PXDBInt(MinValue = 0)]
		public virtual Int32? TransferLeadTime
		{
			get
			{
				return this._TransferLeadTime;
			}
			set
			{
				this._TransferLeadTime = value;
			}
		}
		#endregion
		#region tstamp

		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


		protected Byte[] _tstamp;

		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get { return this._tstamp; }
			set { this._tstamp = value; }
		}

		#endregion
		#region CreatedByID

		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


		protected Guid? _CreatedByID;

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get { return this._CreatedByID; }
			set { this._CreatedByID = value; }
		}

		#endregion
		#region CreatedByScreenID

		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


		protected String _CreatedByScreenID;

		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get { return this._CreatedByScreenID; }
			set { this._CreatedByScreenID = value; }
		}

		#endregion
		#region CreatedDateTime

		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


		protected DateTime? _CreatedDateTime;

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get { return this._CreatedDateTime; }
			set { this._CreatedDateTime = value; }
		}

		#endregion
		#region LastModifiedByID

		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


		protected Guid? _LastModifiedByID;

		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get { return this._LastModifiedByID; }
			set { this._LastModifiedByID = value; }
		}

		#endregion
		#region LastModifiedByScreenID

		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


		protected String _LastModifiedByScreenID;

		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get { return this._LastModifiedByScreenID; }
			set { this._LastModifiedByScreenID = value; }
		}

		#endregion
		#region LastModifiedDateTime

		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


		protected DateTime? _LastModifiedDateTime;

		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get { return this._LastModifiedDateTime; }
			set { this._LastModifiedDateTime = value; }
		}

		#endregion
	}
}
