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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP Bucket Detail Inquiry table for MRP Bucket Reporting and Inquiry
    /// </summary>
    [Serializable]
    [PXCacheName(AM.Messages.MRPBucketDetailInq)]
    public class AMMRPBucketDetailInq : IBqlTable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMMRPBucketDetailInq>.By<bucketID, bucket, inventoryID, subItemID, siteID>
        {
            public static AMMRPBucketDetailInq Find(PXGraph graph, string bucketID, int? bucket, int? inventoryID, int? subItemID, int? siteID, PKFindOptions options = PKFindOptions.None) 
                => FindBy(graph, bucketID, bucket, inventoryID, subItemID, siteID, options);
        }

        public static class FK
        {
            public class Bucket : AMMRPBucket.PK.ForeignKeyOf<AMMRPBucketDetailInq>.By<bucketID> { }
            public class BucketInq : AMMRPBucketInq.PK.ForeignKeyOf<AMMRPBucketDetailInq>.By<bucketID, inventoryID, subItemID, siteID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMMRPBucketDetailInq>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMMRPBucketDetailInq>.By<siteID> { }
        }

        #endregion

        #region BucketID (key)
        public abstract class bucketID : PX.Data.BQL.BqlString.Field<bucketID> { }

        protected String _BucketID;
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Bucket ID", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMMRPBucketInq.bucketID))]
        public virtual String BucketID
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
        #region Bucket (key)
        public abstract class bucket : PX.Data.BQL.BqlString.Field<bucket> { }

        protected Int32? _Bucket;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Bucket", Visible = false, Enabled = false)]
        [PXDefault]
        [PXParent(typeof(Select<AMMRPBucketInq, Where<AMMRPBucketInq.bucketID, Equal<Current<AMMRPBucketDetailInq.bucketID>>,
            And<AMMRPBucketInq.inventoryID, Equal<Current<AMMRPBucketDetailInq.inventoryID>>,
            And<AMMRPBucketInq.subItemID, Equal<Current<AMMRPBucketDetailInq.subItemID>>,
            And<AMMRPBucketInq.siteID, Equal<Current<AMMRPBucketDetailInq.siteID>>>>>>>))]
        public virtual Int32? Bucket
        {
            get
            {
                return this._Bucket;
            }
            set
            {
                this._Bucket = value;
            }
        }
        #endregion
        #region Inventory ID (key)
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Inventory ID", Visible = false, Enabled = false)]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID (key)
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "SubItem", Visible = false, Enabled = false)]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID (key)
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Warehouse", Visible = false, Enabled = false)]
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
        #region From Date
        public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

        protected DateTime? _FromDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "From Date", Enabled = false)]
        public virtual DateTime? FromDate
        {
            get
            {
                return this._FromDate;
            }
            set
            {
                this._FromDate = value;
            }
        }
        #endregion
        #region To Date
        public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }

        protected DateTime? _ToDate;
        [PXDBDate]
        [PXDefault]
        [PXUIField(DisplayName = "To Date", Enabled = false, Visible = false)]
        public virtual DateTime? ToDate
        {
            get
            {
                return this._ToDate;
            }
            set
            {
                this._ToDate = value;
            }
        }
        #endregion
        #region Begin Qty
        public abstract class beginQty : PX.Data.BQL.BqlDecimal.Field<beginQty> { }

        protected Decimal? _BeginQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Begin Qty")]
        public virtual Decimal? BeginQty
        {
            get
            {
                return this._BeginQty;
            }
            set
            {
                this._BeginQty = value;
            }
        }
        #endregion
        #region Actual Supply
        public abstract class actualSupply : PX.Data.BQL.BqlDecimal.Field<actualSupply> { }

        protected Decimal? _ActualSupply;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Actual Supply")]
        public virtual Decimal? ActualSupply
        {
            get
            {
                return this._ActualSupply;
            }
            set
            {
                this._ActualSupply = value;
            }
        }
        #endregion
        #region Actual Demand
        public abstract class actualDemand : PX.Data.BQL.BqlDecimal.Field<actualDemand> { }

        protected Decimal? _ActualDemand;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Actual Demand")]
        public virtual Decimal? ActualDemand
        {
            get
            {
                return this._ActualDemand;
            }
            set
            {
                this._ActualDemand = value;
            }
        }
        #endregion
        #region Net Qty
        public abstract class netQty : PX.Data.BQL.BqlDecimal.Field<netQty> { }

        protected Decimal? _NetQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Net Qty")]
        public virtual Decimal? NetQty
        {
            get
            {
                return this._NetQty;
            }
            set
            {
                this._NetQty = value;
            }
        }
        #endregion
        #region Planned Supply
        public abstract class plannedSupply : PX.Data.BQL.BqlDecimal.Field<plannedSupply> { }

        protected Decimal? _PlannedSupply;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Planned Supply")]
        public virtual Decimal? PlannedSupply
        {
            get
            {
                return this._PlannedSupply;
            }
            set
            {
                this._PlannedSupply = value;
            }
        }
        #endregion
        #region Planned Demand
        public abstract class plannedDemand : PX.Data.BQL.BqlDecimal.Field<plannedDemand> { }

        protected Decimal? _PlannedDemand;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Planned Demand")]
        public virtual Decimal? PlannedDemand
        {
            get
            {
                return this._PlannedDemand;
            }
            set
            {
                this._PlannedDemand = value;
            }
        }
        #endregion
        #region End Qty
        public abstract class endQty : PX.Data.BQL.BqlDecimal.Field<endQty> { }

        protected Decimal? _EndQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "End Qty")]
        public virtual Decimal? EndQty
        {
            get
            {
                return this._EndQty;
            }
            set
            {
                this._EndQty = value;
            }
        }
        #endregion

        #region System Fields
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
        #endregion
    }
}
