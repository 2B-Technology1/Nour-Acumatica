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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.TM;
using System.Text;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Root Inventory Planning table for Inventory Planning display (planning recommendations)
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName("Inventory Planning Detail")]
	public class AMRPDetail : IBqlTable
	{
        internal string DebuggerDisplay => $"[{RecordID}] InventoryID={InventoryID}, SiteID={SiteID}, BaseQty={BaseQty}, SDFlag={SDFlag}, Type={Type}";

        #region Keys

        public class PK : PrimaryKeyOf<AMRPDetail>.By<recordID>
        {
            public static AMRPDetail Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
        }

        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMRPDetail>.By<bOMID, bOMRevisionID> { }
            public class Vendor : AP.Vendor.PK.ForeignKeyOf<AMRPDetail>.By<preferredVendorID> { }
            public class ProductManager : CR.Standalone.EPEmployee.PK.ForeignKeyOf<AMRPDetail>.By<productManagerID> { }
            public class DetailFP : AMRPDetailFP.PK.ForeignKeyOf<AMRPDetail>.By<detailFPRecordID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPDetail>.By<inventoryID> { }
            public class Site : IN.INSite.PK.ForeignKeyOf<AMRPDetail>.By<siteID> { }
            public class ParentInventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPDetail>.By<parentInventoryID> { }
            public class ProductInventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPDetail>.By<productInventoryID> { }
        }

        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region RecordID (key)
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected int? _RecordID;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Record ID", Enabled = false, Visible = false)]
        public virtual int? RecordID
        {
            get
            {
                return this._RecordID;
            }
            set
            {
                this._RecordID = value;
            }
        }
        #endregion
        #region ActionDate
        public abstract class actionDate : PX.Data.BQL.BqlDateTime.Field<actionDate> { }

        protected DateTime? _ActionDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Action Date", Enabled = false)]
        public virtual DateTime? ActionDate
        {
            get
            {
                return this._ActionDate;
            }
            set
            {
                this._ActionDate = value;
            }
        }
        #endregion
        #region ActionLeadTime
        public abstract class actionLeadTime : PX.Data.BQL.BqlInt.Field<actionLeadTime> { }

        protected int? _ActionLeadTime;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Action Lead Time", Enabled = false, Visible = false)]
        public virtual int? ActionLeadTime
        {
            get
            {
                return this._ActionLeadTime;
            }
            set
            {
                this._ActionLeadTime = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
		protected string _BOMID;

		/// <summary>
		/// BOM ID - ID of Bill of materials record
		/// </summary>
		[BomID(Enabled = false, Visible = false, FieldClass = nameof(FeaturesSet.ManufacturingMRP))]
        public virtual string BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
	    #region BOMRevisionID
	    public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }

	    protected string _BOMRevisionID;

		/// <summary>
		/// Bill of material Revision ID
		/// </summary>
		[RevisionIDField(DisplayName = "BOM Revision", Enabled = false, Visible = false, FieldClass = nameof(FeaturesSet.ManufacturingMRP))]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMRPDetail.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate), ValidateValue = false)]
        public virtual string BOMRevisionID
        {
	        get
	        {
	            return this._BOMRevisionID;
	        }
	        set
	        {
	            this._BOMRevisionID = value;
	        }
	    }
	    #endregion
        #region BOMLevel
        public abstract class bOMLevel : PX.Data.BQL.BqlInt.Field<bOMLevel> { }

        protected int? _BOMLevel;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "BOM Level", Enabled = false, Visible = false)]
        public virtual int? BOMLevel
        {
            get
            {
                return this._BOMLevel;
            }
            set
            {
                this._BOMLevel = value;
            }
        }
        #endregion
        #region DetailFPRecordID
        public abstract class detailFPRecordID : PX.Data.BQL.BqlInt.Field<detailFPRecordID> { }

        protected int? _DetailFPRecordID;
        [PXDBInt]
        [PXDefault]
        [PXUIField(DisplayName = "Detail FP ID", Enabled = false, Visible = false)]
        public virtual int? DetailFPRecordID
        {
            get
            {
                return this._DetailFPRecordID;
            }
            set
            {
                this._DetailFPRecordID = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected int? _InventoryID;
        [Inventory(Enabled = false)]
        [PXDefault]
        public virtual int? InventoryID
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
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(AMRPDetail.inventoryID))]
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
        #region IsSub
        public abstract class isSub : PX.Data.BQL.BqlBool.Field<isSub> { }

        protected bool? _IsSub;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Sub", Enabled = false, Visible = false)]
        public virtual bool? IsSub
        {
            get
            {
                return this._IsSub;
            }
            set
            {
                this._IsSub = value;
            }
        }
        #endregion
        #region ParentInventoryID
        public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

        protected int? _ParentInventoryID;
        [Inventory(DisplayName = "Parent Inventory ID", Enabled = false, Visible = false)]
        public virtual int? ParentInventoryID
        {
            get
            {
                return this._ParentInventoryID;
            }
            set
            {
                this._ParentInventoryID = value;
            }
        }
        #endregion
        #region ParentSubItemID

        public abstract class parentSubItemID : PX.Data.BQL.BqlInt.Field<parentSubItemID> { }

        protected Int32? _ParentSubItemID;
        [SubItem(typeof(AMRPDetail.parentInventoryID), DisplayName = "Parent Subitem")]
        public virtual Int32? ParentSubItemID
        {
            get
            {
                return this._ParentSubItemID;
            }
            set
            {
                this._ParentSubItemID = value;
            }
        }
        #endregion
        #region ProductInventoryID
        public abstract class productInventoryID : PX.Data.BQL.BqlInt.Field<productInventoryID> { }

        protected int? _ProductInventoryID;
        [Inventory(DisplayName = "Product Inventory ID", Enabled = false, Visible = false)]
        public virtual int? ProductInventoryID
        {
            get
            {
                return this._ProductInventoryID;
            }
            set
            {
                this._ProductInventoryID = value;
            }
        }
        #endregion
        #region ProductSubItemID

        public abstract class productSubItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _ProductSubItemID;
        [SubItem(typeof(AMRPDetail.productInventoryID), DisplayName = "Product Subitem")]
        public virtual Int32? ProductSubItemID
        {
            get
            {
                return this._ProductSubItemID;
            }
            set
            {
                this._ProductSubItemID = value;
            }
        }
        #endregion
        #region PromiseDate
        public abstract class promiseDate : PX.Data.BQL.BqlDateTime.Field<promiseDate> { }

        protected DateTime? _PromiseDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Promise Date", Enabled = false)]
        public virtual DateTime? PromiseDate
        {
            get
            {
                return this._PromiseDate;
            }
            set
            {
                this._PromiseDate = value;
            }
        }
        #endregion
        #region RefType
	    public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

	    protected string _RefType;
        /// <summary>
        /// Plan type related to the RefOrdertype and RefOrderNbr
        /// </summary>
	    [PXDBString]
	    [PXUIField(DisplayName = "Reference Type", Visible = false, Enabled = false)]
	    [MRPPlanningType.List]
	    public virtual string RefType
	    {
	        get
	        {
	            return this._RefType;
	        }
	        set
	        {
	            this._RefType = value;
	        }
	    }
        #endregion
        #region SDFlag
        public abstract class sDFlag : PX.Data.BQL.BqlString.Field<sDFlag> { }

        protected string _SDFlag;
        [PXDBString(1, IsUnicode = true, IsFixed = true)]
        [PXDefault(MRPSDFlag.Unknown)]
        [PXUIField(DisplayName = "SD Flag", Visible = false, Enabled = false)]
        [MRPSDFlag.List]
        public virtual string SDFlag
        {
            get
            {
                return this._SDFlag;
            }
            set
            {
                this._SDFlag = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;
        [Site(Enabled = false)]
        [PXDefault]
        public virtual int? SiteID
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		protected int? _BranchID;
		[Branch(IsDetail = false, Enabled = false, Visible = false)]
		public virtual int? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
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
        [PXDefault(INReplenishmentSource.None, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Source")]
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
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected decimal? _BaseQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty", Enabled = false)]
        public virtual decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
        #region BaseUOM
        public abstract class baseUOM : PX.Data.BQL.BqlString.Field<baseUOM> { }

        protected string _BaseUOM;
        [INUnit(typeof(AMRPDetail.inventoryID), DisplayName = "Base UOM", Enabled = false, Visible = false)]
        [PXDefault]
        public virtual string BaseUOM
        {
            get
            {
                return this._BaseUOM;
            }
            set
            {
                this._BaseUOM = value;
            }
        }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected string _Type;

		/// <summary>
		/// Type
		/// </summary>
		[PXDefault(MRPPlanningType.Unknown)]
        [PXDBString]
        [PXUIField(DisplayName = "Type")]
        [MRPPlanningType.List]
        public virtual string Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }
        #endregion
        #region ProductManagerID
        public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

        protected int? _ProductManagerID;
        [Owner(DisplayName = "Product Manager ID", Visible = false, Enabled = false)]
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
        #region PreferredVendorID
        public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

        protected Int32? _PreferredVendorID;
        [VendorNonEmployeeActive(typeof(Search2<Vendor.bAccountID,
                    LeftJoin<POVendorInventory, On<Vendor.bAccountID, Equal<POVendorInventory.vendorID>>>,
                    Where<Vendor.type, NotEqual<BAccountType.employeeType>,
                        And<POVendorInventory.inventoryID, Equal<Current<AMRPDetail.inventoryID>>>>>),
            Visibility = PXUIVisibility.SelectorVisible,
            DescriptionField = typeof(Vendor.acctName),
            CacheGlobal = true,
            Filterable = true,
            Required = false,
            DisplayName = "Preferred Vendor ID")]
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
        #region Processed
        public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }

        protected bool? _Processed;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Processed")]
        public virtual bool? Processed
        {
            get
            {
                return _Processed;
            }
            set
            {
                _Processed = value;
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
        [PXUIField(DisplayName = "Inventory Planning Date", Enabled = false, Visible = false)]
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
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [RefNbrField(typeof(AMRPDetail.refNoteID), Enabled = false, DisplayName = "Related Document")]
        public virtual String RefNbr { get; set; }
        #endregion
        #region ParentRefNbr
        public abstract class parentRefNbr : PX.Data.BQL.BqlString.Field<parentRefNbr> { }

        [RefNbrField(typeof(AMRPDetail.parentRefNoteID), Enabled = false, DisplayName = "Related Parent Document")]
        public virtual String ParentRefNbr { get; set; }
        #endregion
        #region ProductRefNbr
        public abstract class productRefNbr : PX.Data.BQL.BqlString.Field<productRefNbr> { }

        [RefNbrField(typeof(AMRPDetail.productRefNoteID), Enabled = false, DisplayName = "Related Product Document")]
        public virtual String ProductRefNbr { get; set; }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        [PXDBGuid]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region ParentRefNoteID
        public abstract class parentRefNoteID : PX.Data.BQL.BqlGuid.Field<parentRefNoteID> { }

        protected Guid? _ParentRefNoteID;
        [PXUIField(DisplayName = "Related Parent Document ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        [PXDBGuid]
        public virtual Guid? ParentRefNoteID
        {
            get
            {
                return this._ParentRefNoteID;
            }
            set
            {
                this._ParentRefNoteID = value;
            }
        }
        #endregion
        #region ProductRefNoteID
        public abstract class productRefNoteID : PX.Data.BQL.BqlGuid.Field<productRefNoteID> { }

        protected Guid? _ProductRefNoteID;
        [PXUIField(DisplayName = "Related Product Document ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        [PXDBGuid]
        public virtual Guid? ProductRefNoteID
        {
            get
            {
                return this._ProductRefNoteID;
            }
            set
            {
                this._ProductRefNoteID = value;
            }
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID>
        {
        }
        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
		#endregion
		#region TransferSiteID
		public abstract class transferSiteID : PX.Data.BQL.BqlInt.Field<transferSiteID> { }

		protected Int32? _TransferSiteID;

		/// <summary>
		/// Transfer Warehouse ID
		/// </summary>
		[Site(DisplayName = "Transfer Warehouse", ValidateValue = false, FieldClass = nameof(FeaturesSet.Warehouse))]
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
	}
}
