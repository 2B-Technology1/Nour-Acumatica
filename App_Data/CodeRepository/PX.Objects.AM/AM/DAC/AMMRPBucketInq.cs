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
using PX.Objects.AP;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP Bucket Inquiry table for MRP Bucket Reporting and Inquiry
    /// </summary>
    [Serializable]
    [PXCacheName(AM.Messages.MRPBucketInq)]
    public class AMMRPBucketInq : IBqlTable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMMRPBucketInq>.By<bucketID, inventoryID, subItemID, siteID>
        {
            public static AMMRPBucketInq Find(PXGraph graph, string bucketID, int? inventoryID, int? subItemID, int? siteID, PKFindOptions options = PKFindOptions.None) 
                => FindBy(graph, bucketID, inventoryID, subItemID, siteID, options);
        }

        public static class FK
        {
            public class Bucket : AMMRPBucket.PK.ForeignKeyOf<AMMRPBucketInq>.By<bucketID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMMRPBucketInq>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMMRPBucketInq>.By<siteID> { }
        }

        #endregion

        #region BucketID (key)
        public abstract class bucketID : PX.Data.BQL.BqlString.Field<bucketID> { }

        protected String _BucketID;
        [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        [PXDefault]
        [PXUIField(DisplayName = "Bucket ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMMRPBucket.bucketID>))]
        [PXRestrictor(typeof(Where<AMMRPBucket.activeFlg, Equal<True>>), AM.Messages.BucketInvalid)]
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
        #region InventoryID (key)
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
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
        [SubItem(typeof(AMMRPBucketInq.inventoryID), IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
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
        [Site(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)] 
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
        #region Qty On Hand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected Decimal? _QtyOnHand;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. on Hand", Enabled = false)]
        public virtual Decimal? QtyOnHand
        {
            get
            {
                return this._QtyOnHand;
            }
            set
            {
                this._QtyOnHand = value;
            }
        }
        #endregion
        #region Product Manager ID
        public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

        protected int? _ProductManagerID;
        [PX.TM.Owner(DisplayName = "Product Mgr.", Enabled = false)]
        public virtual int? ProductManagerID
        {
            get
            {
                return this._ProductManagerID;
            }
            set
            {
                this._ProductManagerID = value;
            }
        }
        #endregion
        #region Safety Stock
        public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }

        protected Decimal? _SafetyStock;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Safety Stock", Enabled = false)]
        public virtual Decimal? SafetyStock
        {
            get
            {
                return this._SafetyStock;
            }
            set
            {
                this._SafetyStock = value;
            }
        }
        #endregion
        #region Replenishment Source
        public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }

        protected string _ReplenishmentSource;

		/// <summary>
		/// Replenishment source
		/// </summary>
		[PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Rep. Source", Enabled = false)]
        [INReplenishmentSource.List]
        public virtual string ReplenishmentSource
        {
            get
            {
                return this._ReplenishmentSource;
            }
            set
            {
                this._ReplenishmentSource = value;
            }
        }
        #endregion
        #region Preferred Vendor ID
        public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

        protected Int32? _PreferredVendorID;
        [Vendor(DisplayName = "Preferred Vendor ID", Enabled = false, Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName))]
        public virtual Int32? PreferredVendorID
        {
            get
            {
                return this._PreferredVendorID;
            }
            set
            {
                this._PreferredVendorID = value;
            }
        }
        #endregion
        #region Lead Time
        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

        protected Int32? _LeadTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Lead Time")]
        [PXDefault(TypeCode.Int32, "0")]
        public virtual Int32? LeadTime
        {
            get
            {
                return this._LeadTime;
            }
            set
            {
                this._LeadTime = value;
            }
        }
        #endregion

        #region System Fields
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
        #endregion
    }
}
