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
using PX.Objects.IN;
using PX.TM;
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.AM
{
	/// <summary>
	/// The table with the exception messages when running the Inventory Planning Regeneration (AM505000) report (corresponding to the <see cref="MRPEngine"/> graph).
	/// </summary>
	[Serializable]
	[PXCacheName(AM.Messages.MRPExceptions)]
    [PXPrimaryGraph(typeof(MRPExcept))]
    public class AMRPExceptions : IBqlTable
	{
        #region Keys

        public class PK : PrimaryKeyOf<AMRPExceptions>.By<recordID>
        {
            public static AMRPExceptions Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
        }

        public static class FK
        {
            public class ProductManager : CR.Standalone.EPEmployee.PK.ForeignKeyOf<AMRPExceptions>.By<productManagerID> { }
            public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<AMRPExceptions>.By<inventoryID> { }
            public class Site : IN.INSite.PK.ForeignKeyOf<AMRPExceptions>.By<siteID> { }
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected int? _InventoryID;
		[Inventory(Enabled=false)]
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMRPExceptions.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMRPExceptions.inventoryID))]
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
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected string _Type;

		/// <summary>
		/// Type
		/// </summary>
		[PXDBString]
        [PXDefault]
        [MRPExceptionType.List]
        [PXUIField(DisplayName = "Type", Enabled = false)]
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
        #region RefType
        public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

        protected string _RefType;

		/// <summary>
		/// Reference type
		/// </summary>
		[PXDBString]
        [PXUIField(DisplayName = "Ref Type", Enabled = false)]
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
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [RefNbrField(typeof(refNoteID), Enabled = false, DisplayName = "Related Document")]
        public virtual String RefNbr { get; set; }
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
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
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
		#region PromiseDate
        public abstract class promiseDate : PX.Data.BQL.BqlDateTime.Field<promiseDate> { }

        protected DateTime? _PromiseDate;
		[PXDBDate]
		[PXDefault]
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
        #region RequiredDate
        public abstract class requiredDate : PX.Data.BQL.BqlDateTime.Field<requiredDate> { }

        protected DateTime? _RequiredDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Required Date")]
        public virtual DateTime? RequiredDate
        {
            get
            {
                return this._RequiredDate;
            }
            set
            {
                this._RequiredDate = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Enabled = false)]
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
		#region SupplyQty
		public abstract class supplyQty : PX.Data.BQL.BqlDecimal.Field<supplyQty> { }

        protected Decimal? _SupplyQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Supply Qty", Enabled = false)]
        public virtual Decimal? SupplyQty
        {
            get
            {
                return this._SupplyQty;
            }
            set
            {
                this._SupplyQty = value;
            }
        }
        #endregion
        #region SupplySiteID
        public abstract class supplySiteID : PX.Data.BQL.BqlInt.Field<supplySiteID> { }

        protected Int32? _SupplySiteID;

		/// <summary>
		/// Supply Warehouse
		/// </summary>
		[Site(DisplayName = "Supply Warehouse", FieldClass = nameof(FeaturesSet.Warehouse))]
        public virtual Int32? SupplySiteID
        {
            get
            {
                return this._SupplySiteID;
            }
            set
            {
                this._SupplySiteID = value;
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
	    #region tstamp
	    public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

	    protected byte[] _tstamp;
	    [PXDBTimestamp()]
	    public virtual byte[] tstamp
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
    }
}
