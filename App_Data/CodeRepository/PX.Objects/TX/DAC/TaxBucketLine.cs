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

namespace PX.Objects.TX
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// A detail of <see cref="TaxBucket"/>. It implements the many-to-many relationship
	/// between <see cref="TaxReportLine"/> and <see cref="TaxBucket"/>.
	/// The records of this type are also created for the report lines that are detailed by tax zones.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TaxBucketLine)]
	public partial class TaxBucketLine : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<TaxBucketLine>.By<vendorID, bucketID, taxReportRevisionID, lineNbr>
		{
			public static TaxBucketLine Find(PXGraph graph, int? vendorID, int? bucketID, int? taxReportRevisionID, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, vendorID, bucketID, taxReportRevisionID, lineNbr, options);
		}
		public static class FK
		{
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<TaxBucketLine>.By<vendorID> { }
			public class TaxBucket : TX.TaxBucket.PK.ForeignKeyOf<TaxBucketLine>.By<vendorID, bucketID> { }
			public class ReportLine : TX.TaxReportLine.PK.ForeignKeyOf<TaxBucketLine>.By<vendorID, taxReportRevisionID, lineNbr> { }
			
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;

		/// <summary>
		/// The foreign key to <see cref="PX.Objects.AP.Vendor">. It specifies a tax agency to which the report line belongs.
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(TaxBucketMaster.vendorID))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region BucketID
		public abstract class bucketID : PX.Data.BQL.BqlInt.Field<bucketID> { }
		protected Int32? _BucketID;

		/// <summary>
		/// The foreign key to the master record (<see cref="TaxBucket"/>).
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(TaxBucketMaster.bucketID))]
		public virtual Int32? BucketID
		{
			get
			{
				return this._BucketID;
			}
			set
			{
				this._BucketID = value;
			}
		}
		#endregion
		#region TaxReportRevisionID
		public abstract class taxReportRevisionID : PX.Data.BQL.BqlInt.Field<taxReportRevisionID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(TaxBucketMaster.taxReportRevisionID))]
		[PXUIField(DisplayName = "Report Version", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? TaxReportRevisionID { get; set; }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// The reference to the report line (<see cref="TaxReportLine"/>), which is included in the reporting group (<see cref="TaxBucket"/>).
		/// The field is a part of the primary key.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName="Report Line", Visibility=PXUIVisibility.Visible)]
		[TaxReportLineSelector(
			typeof(Search<TaxReportLine.lineNbr, 
					Where<TaxReportLine.vendorID, Equal<Current<TaxBucketLine.vendorID>>, 
						And<TaxReportLine.taxReportRevisionID, Equal<Current<TaxBucketLine.taxReportRevisionID>>,
						And<TaxReportLine.tempLineNbr, IsNull>>>>),
				typeof(TaxReportLine.sortOrder), typeof(TaxReportLine.descr),
				typeof(TaxReportLine.reportLineNbr), typeof(TaxReportLine.bucketSum), typeof(TaxReportLine.taxReportRevisionID),
			SubstituteKey = typeof(TaxReportLine.sortOrder))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedByID()]
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
