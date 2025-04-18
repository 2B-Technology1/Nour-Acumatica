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
using PX.Api;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AP
{
	[Serializable()]
	[PXHidden]
	[PXCacheName(Messages.APVendorRefNbr)]
	public partial class APVendorRefNbr : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<APVendorRefNbr>.By<masterID, detailID>
		{
			public static APVendorRefNbr Find(PXGraph graph, Guid? masterID, Int32? detailID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, masterID, detailID, options);
		}
		public static class FK
		{
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<APVendorRefNbr>.By<vendorID> { }
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[PXDBInt()]
		[PXDefault()]
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
        #region VendorDocumentID
        public abstract class vendorDocumentID : PX.Data.BQL.BqlString.Field<vendorDocumentID> { }
		protected String _VendorDocumentID;
		[PXDBString(64, IsUnicode = true)]
		[PXDefault()]
		public virtual String VendorDocumentID
        {
			get
			{
				return this._VendorDocumentID;
			}
			set
			{
				this._VendorDocumentID = value;
			}
		}
		#endregion
		#region MasterID
		public abstract class masterID : PX.Data.BQL.BqlGuid.Field<masterID> { }
		protected Guid? _MasterID;
		[PXDBGuid(IsKey = true)]
		public virtual Guid? MasterID
        {
			get
			{
				return this._MasterID;
			}
			set
			{
				this._MasterID = value;
			}
		}
		#endregion
		#region DetailID
		public abstract class detailID : PX.Data.BQL.BqlInt.Field<detailID> { }
		protected Int32? _DetailId;
		[PXDBInt(IsKey = true)]
		public virtual Int32? DetailID
		{
			get
			{
				return this._DetailId;
			}
			set
			{
				this._DetailId = value;
			}
		}
        #endregion

        #region IsProcessed
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
        public abstract class isProcessed : PX.Data.BQL.BqlBool.Field<isProcessed> { }

        protected bool? _IsProcessed;
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2020R2)]
        [PXBool]
        public virtual bool? IsProcessed
        {
            get
            {
                return _IsProcessed;
            }
            set
            {
                _IsProcessed = value;
            }
        }
        #endregion

        #region SiblingID
        public abstract class siblingID : PX.Data.BQL.BqlGuid.Field<siblingID> { }
        protected Guid? _SiblingID;
        [PXDBGuid()]
        [PXDefault()]
        public virtual Guid? SiblingID
        {
            get
            {
                return this._SiblingID;
            }
            set
            {
                this._SiblingID = value;
            }
        }
        #endregion

        #region IsChecked
        public abstract class isChecked : PX.Data.BQL.BqlBool.Field<isChecked> { }

        protected bool? _IsChecked;
        [PXBool]
        public virtual bool? IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                _IsChecked = value;
            }
        }
        #endregion

        #region IsIgnored
        public abstract class isIgnored : PX.Data.BQL.BqlBool.Field<isIgnored> { }

        protected bool? _IsIgnored;
        [PXBool]
        [PXDBCalced(typeof(False), typeof(bool))]
        public virtual bool? IsIgnored
        {
            get
            {
                return _IsIgnored;
            }
            set
            {
                _IsIgnored = value;
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

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
