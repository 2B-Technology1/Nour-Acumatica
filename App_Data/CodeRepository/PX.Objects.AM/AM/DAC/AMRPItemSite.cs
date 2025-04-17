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
using PX.Objects.CS;
using PX.Objects.IN;
using PX.TM;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table that stores replenishment values for an Inventory Item at a Warehouse. These values are used by Inventory Planning Regeneration (AM505000) (corresponding to the <see cref="MRPEngine"/> graph).
	/// </summary>
	[Serializable]
    [PXCacheName(AM.Messages.MRPInventory)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMRPItemSite : IBqlTable
    {
#if DEBUG
        //
        //  Developer Note: if adding new fields to this extension, add the same fields to PX.Objects.AM.Standalone.AMRPItemSite
        // 
#endif
        internal string DebuggerDisplay => $"InventoryID={InventoryID}, SiteID={SiteID}, SubItemID={SubItemID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMRPItemSite>.By<inventoryID, siteID, subItemID>
        {
            public static AMRPItemSite Find(PXGraph graph, int? inventoryID, int? siteID, int? subItemID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, inventoryID, siteID, subItemID, options);
        }

        public static class FK
        {
            public class ProductManager : CR.Standalone.EPEmployee.PK.ForeignKeyOf<AMRPItemSite>.By<productManagerID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPItemSite>.By<inventoryID> { }
            public class Site : IN.INSite.PK.ForeignKeyOf<AMRPItemSite>.By<siteID> { }
            public class PreferredVendorID : AP.Vendor.PK.ForeignKeyOf<AMRPDetailFP>.By<preferredVendorID> { }
			public class ReplenishmentSite : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMRPItemSite>.By<replenishmentSiteID> { }
		}

        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected Boolean? _Selected = false;
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual Boolean? Selected
        {
            get
            {
                return this._Selected;
            }
            set
            {
                this._Selected = value;
            }
        }
        #endregion
        #region InventoryID (key)
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(IsKey = true, Enabled = false)]
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
        #region SiteID (key)
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(IsKey = true, Enabled = false)]
        [PXDefault]
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
        #region SubItemID (key)
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(IsKey = true)]
        [PXDefault]
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
        #region LeadTime
        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

        protected Int32? _LeadTime;
        [PXDBInt]
        [PXUIField(DisplayName = "Lead Time")]
        [PXDefault(TypeCode.Int32, "0")]
        public Int32? LeadTime
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
        #region PreferredVendorID
        public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

        protected Int32? _PreferredVendorID;
        [VendorActive(DisplayName = "Preferred Vendor", Required = false, DescriptionField = typeof(Vendor.acctName))]
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
        #region ProductManagerID
        public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

        protected int? _ProductManagerID;
        [Owner(DisplayName = "Product Manager", Enabled = false)]
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
        #region QtyOnHand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected Decimal? _QtyOnHand;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. On Hand")]
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
        #region ReorderPoint
        public abstract class reorderPoint : PX.Data.BQL.BqlDecimal.Field<reorderPoint> { }

        protected Decimal? _ReorderPoint;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Reorder Point")]
        public virtual Decimal? ReorderPoint
        {
            get
            {
                return this._ReorderPoint;
            }
            set
            {
                this._ReorderPoint = value;
            }
        }
        #endregion
        #region ReplenishmentSource
        public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }

        protected string _ReplenishmentSource;

		/// <summary>
		/// Replenishment source
		/// </summary>
		[PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Replenishment Source")]
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
        #region SafetyStock
        public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }

        protected Decimal? _SafetyStock;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Safety Stock")]
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
        #region LotSize
        public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }

        private decimal? _lotSize;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Lot Size")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public Decimal? LotSize
        {
            get { return _lotSize; }
            set { _lotSize = value; }
        }

        #endregion
        #region MaxOrdQty
        public abstract class maxOrdQty : PX.Data.BQL.BqlDecimal.Field<maxOrdQty> { }

        private decimal? _MaxOrdQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Max Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public Decimal? MaxOrdQty
        {
            get { return _MaxOrdQty; }
            set { _MaxOrdQty = value; }
        }

        #endregion
        #region MinOrdQty
        public abstract class minOrdQty : PX.Data.BQL.BqlDecimal.Field<minOrdQty> { }

        private decimal? _MinOrdQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Min Order Qty")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public Decimal? MinOrdQty
        {
            get { return _MinOrdQty; }
            set { _MinOrdQty = value; }
        }
		#endregion
		#region ReplenishmentSiteID
		public abstract class replenishmentSiteID : PX.Data.BQL.BqlInt.Field<replenishmentSiteID> { }

		protected Int32? _ReplenishmentSiteID;
		[Site(DisplayName = "Replenishment Warehouse", ValidateValue = false)]
		public virtual Int32? ReplenishmentSiteID
		{
			get
			{
				return this._ReplenishmentSiteID;
			}
			set
			{
				this._ReplenishmentSiteID = value;
			}
		}
		#endregion
		#region TransferLeadTime
		public abstract class transferLeadTime : PX.Data.BQL.BqlInt.Field<transferLeadTime> { }

		[PXDBInt]
		[PXDefault(0)]
		public Int32? TransferLeadTime { get; set; }
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
        [PXUIField(DisplayName = "MRP Bucket Date", Enabled = false, Visible = false)]
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
		#endregion
		#region AMGroupWindow
		public abstract class aMGroupWindow : PX.Data.BQL.BqlInt.Field<aMGroupWindow> { }

		/// <summary>
		/// Days of Supply for the Inventory Planning
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Days of Supply")]
		[PXDefault(TypeCode.Int32, "0", PersistingCheck = PXPersistingCheck.Nothing)]
		public int? AMGroupWindow { get; set; }
		#endregion
	}
}
