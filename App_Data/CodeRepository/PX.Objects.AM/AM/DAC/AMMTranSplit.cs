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
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
	/// <summary>
	/// Line details for a production transaction.
	/// Parent: <see cref="AMMTran"/>
	/// </summary>
	/// <remarks>
	/// The records of this type are created and edited on the following forms:
	/// <list type="bullet">
	/// <item><description>Move (AM302000) (corresponding to the <see cref="MoveEntry"/> graph)</description></item>
	/// <item><description>Labor (AM301000) (corresponding to the <see cref="LaborEntry"/> graph)</description></item>
	/// <item><description>Materials (AM300000) (corresponding to the <see cref="MaterialEntry"/> graph)</description></item>
	/// </list>
	/// </remarks>
	[PXCacheName(Messages.AMTransactionSplit)]
    [System.Diagnostics.DebuggerDisplay("DocType = {DocType}, BatNbr = {BatNbr}, LineNbr = {LineNbr}, SplitLineNbr = {SplitLineNbr}")]
    [Serializable]
    public class AMMTranSplit : IBqlTable, ILSDetail, IAMBatch
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMMTranSplit>.By<docType, batNbr, lineNbr, splitLineNbr>
        {
            public static AMMTranSplit Find(PXGraph graph, string docType, string batNbr, int? lineNbr, int? splitLineNbr, PKFindOptions options = PKFindOptions.None) =>
                FindBy(graph, docType, batNbr, lineNbr, splitLineNbr, options);
        }

        public static class FK
        {
            public class Batch : AMBatch.PK.ForeignKeyOf<AMMTranSplit>.By<docType, batNbr> { }
            public class Tran : AMMTran.PK.ForeignKeyOf<AMMTranSplit>.By<docType, batNbr, lineNbr> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMMTranSplit>.By<inventoryID> { }
            public class SubItem : INSubItem.PK.ForeignKeyOf<AMMTranSplit>.By<subItemID> { }
            public class Site : INSite.PK.ForeignKeyOf<AMMTranSplit>.By<siteID> { }
            public class Location : INLocation.PK.ForeignKeyOf<AMMTranSplit>.By<locationID> { }
            public class LotSerialStatus : PX.Objects.IN.INLotSerialStatus.PK.ForeignKeyOf<AMMTranSplit>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr> { }
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
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(AMMTran.tranType))]
		public virtual String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
        #region DocType (key)

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault(typeof(AMMTran.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr (key)

        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(AMBatch.batNbr))]
        [PXParent(typeof(Select<AMMTran, Where<AMMTran.docType, Equal<Current<AMMTranSplit.docType>>, And<AMMTran.batNbr, Equal<Current<AMMTranSplit.batNbr>>, And<AMMTran.lineNbr, Equal<Current<AMMTranSplit.lineNbr>>>>>>))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr (key)

        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(AMMTran.lineNbr))]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region SplitLineNbr (key)

        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(AMMTran.lotSerCntr))]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region TranDate

        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMMTran.tranDate))]
		[PXUIField(DisplayName = "Date")]

		public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region InventoryID

        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory(Visible = false, Enabled = false)]
        [PXDefault(typeof(AMMTran.inventoryID))]
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
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(
            typeof(AMMTranSplit.inventoryID),
            typeof(LeftJoin<INSiteStatusByCostCenter,
                On<INSiteStatusByCostCenter.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatusByCostCenter.inventoryID, Equal<Optional<AMMTranSplit.inventoryID>>,
                And<INSiteStatusByCostCenter.siteID, Equal<Optional<AMMTranSplit.siteID>>,
				And<INSiteStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>>>>>>))]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMMTranSplit.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
        [PXFormula(typeof(Default<AMMTranSplit.inventoryID>))]
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
        #region CostSubItemID

        public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }

        protected Int32? _CostSubItemID;
        [PXInt]
        public virtual Int32? CostSubItemID
        {
            get
            {
                return this._CostSubItemID;
            }
            set
            {
                this._CostSubItemID = value;
            }
        }
        #endregion
        #region CostSiteID

        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }

        protected Int32? _CostSiteID;
        [PXInt]
        public virtual Int32? CostSiteID
        {
            get
            {
                return this._CostSiteID;
            }
            set
            {
                this._CostSiteID = value;
            }
        }
        #endregion
        #region SiteID

        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site]
        [PXDefault(typeof(AMMTran.siteID))]
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
        #region LocationID

        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(AMMTranSplit.inventoryID), typeof(AMMTranSplit.subItemID), typeof(AMMTranSplit.siteID), false, true, typeof(AMMTran.isScrap), typeof(AMMTran))]
        [PXDefault]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMMTranSplit.inventoryID), typeof(AMMTranSplit.subItemID), 
            typeof(AMMTranSplit.locationID), typeof(AMMTran.lotSerialNbr), FieldClass = "LotSerial")]
        public virtual String LotSerialNbr
        {
            get
            {
                return this._LotSerialNbr;
            }
            set
            {
                this._LotSerialNbr = value;
            }
        }
        #endregion
        #region LotSerClassID

        public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        protected String _LotSerClassID;
        [PXString(10, IsUnicode = true)]
        public virtual String LotSerClassID
        {
            get
            {
                return this._LotSerClassID;
            }
            set
            {
                this._LotSerClassID = value;
            }
        }
        #endregion
        #region AssignedNbr

        public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr>{ }

        protected String _AssignedNbr;
        [PXString(30, IsUnicode = true)]
        public virtual String AssignedNbr
        {
            get
            {
                return this._AssignedNbr;
            }
            set
            {
                this._AssignedNbr = value;
            }
        }
        #endregion
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMMTranSplit.inventoryID))]
        public virtual DateTime? ExpireDate
        {
            get
            {
                return this._ExpireDate;
            }
            set
            {
                this._ExpireDate = value;
            }
        }
        #endregion
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXDefault(typeof(AMMTran.invtMult))]
        public virtual Int16? InvtMult
        {
            get
            {
                return this._InvtMult;
            }
            set
            {
                this._InvtMult = value;
            }
        }
        #endregion
        #region Released

        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool]
        [PXDefault(false)]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region UOM

        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit(typeof(AMMTranSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(typeof(AMMTran.uOM))]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region Qty

        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMMTranSplit.uOM), typeof(AMMTranSplit.baseQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity")]
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
        #region BaseQty

        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        public virtual Decimal? BaseQty
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
        #region PlanID

        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        protected Int64? _PlanID;
        [PXDBLong]
        public virtual Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
        #region OrigSource

        public abstract class origSource : PX.Data.BQL.BqlString.Field<origSource> { }

        protected String _OrigSource;
        [PXDBString(2, IsUnicode = true)]
        public virtual String OrigSource
        {
            get
            {
                return this._OrigSource;
            }
            set
            {
                this._OrigSource = value;
            }
        }
        #endregion
        #region OrigBatNbr

        public abstract class origBatNbr : PX.Data.BQL.BqlString.Field<origBatNbr> { }

        protected String _OrigBatNbr;
        [PXDBString(15, IsUnicode = true)]
        public virtual String OrigBatNbr
        {
            get
            {
                return this._OrigBatNbr;
            }
            set
            {
                this._OrigBatNbr = value;
            }
        }
        #endregion
        #region OrigLineNbr

        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }

        protected Int32? _OrigLineNbr;
        [PXDBInt]
        public virtual Int32? OrigLineNbr
        {
            get
            {
                return this._OrigLineNbr;
            }
            set
            {
                this._OrigLineNbr = value;
            }
        }
        #endregion
        #region OrigSplitLineNbr

        public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }

        protected Int32? _OrigSplitLineNbr;
        [PXDBInt]
        public virtual Int32? OrigSplitLineNbr
        {
            get
            {
                return this._OrigSplitLineNbr;
            }
            set
            {
                this._OrigSplitLineNbr = value;
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
        #region Is Stock Item
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

        protected bool? _IsStockItem;
        [PXDBBool]
        [PXUIField(DisplayName = "Stock Item")]
        [PXFormula(typeof(Selector<AMMTranSplit.inventoryID, InventoryItem.stkItem>))]
        public virtual bool? IsStockItem
        {
            get
            {
                return this._IsStockItem;
            }
            set
            {
                this._IsStockItem = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        [PXInt]
        [PXUIField(DisplayName = "Project", Visible = false, Enabled = false)]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        [PXInt]
        [PXUIField(DisplayName = "Task", Visible = false, Enabled = false)]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region ParentLotSerialNbr
        public abstract class parentLotSerialNbr : PX.Data.BQL.BqlString.Field<parentLotSerialNbr> { }

        protected String _ParentLotSerialNbr;
        [ParentLotSerialNbr(typeof(AMMTran.orderType), typeof(AMMTran.prodOrdID))]
        public virtual String ParentLotSerialNbr
        {
            get
            {
                return this._ParentLotSerialNbr;
            }
            set
            {
                this._ParentLotSerialNbr = value;
            }
        }
        #endregion

        #region Methods

        public static implicit operator INCostSubItemXRef(AMMTranSplit item)
        {
            INCostSubItemXRef ret = new INCostSubItemXRef();
            ret.SubItemID = item.SubItemID;
            ret.CostSubItemID = item.CostSubItemID;

            return ret;
        }

        public static implicit operator INCostSite(AMMTranSplit item)
        {
            INCostSite ret = new INCostSite();
            ret.CostSiteID = item.CostSiteID;

            return ret;
        }

        public static implicit operator INCostStatus(AMMTranSplit item)
        {
            INCostStatus ret = new INCostStatus();
            ret.InventoryID = item.InventoryID;
            ret.CostSubItemID = item.CostSubItemID;
            ret.CostSiteID = item.CostSiteID;
            ret.LayerType = INLayerType.Normal;

            return ret;
        }
        #endregion

		bool? ILSMaster.IsIntercompany => false;
    }

	[PXHidden]
	[Serializable]
	[PXProjection(typeof(Select2<AMMTranSplit,
		InnerJoin<AMMTran, On<AMMTran.docType, Equal<AMMTranSplit.docType>,
			And<AMMTran.batNbr, Equal<AMMTranSplit.batNbr>,
				And<AMMTran.lineNbr, Equal<AMMTranSplit.lineNbr>>>>>,
		Where<AMMTranSplit.lotSerialNbr, NotEqual<Empty>,
			And<AMMTranSplit.lotSerialNbr, IsNotNull>>>), Persistent = false)]
	public class AMMTranSplitProdMatlLotSerial : IBqlTable
	{ 
		
        #region DocType (key)

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMMTranSplit.docType))]
        public virtual String DocType { get; set; }
        #endregion
        #region BatNbr (key)

        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMMTranSplit.batNbr))]
        public virtual String BatNbr { get; set; }
        #endregion
        #region LineNbr (key)

        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.lineNbr))]
        public virtual Int32? LineNbr { get; set; }
        #endregion
        #region SplitLineNbr (key)

        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.splitLineNbr))]
        public virtual Int32? SplitLineNbr { get; set; }
        #endregion
		#region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        [PXDBString(3, IsFixed = true, BqlField = typeof(AMMTranSplit.tranType))]
		public virtual String TranType { get; set; }

        #endregion
		#region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        [PXDBShort(BqlField = typeof(AMMTranSplit.invtMult))]
        public virtual Int16? InvtMult { get; set; }

        #endregion
		#region Qty

        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		[PXDBQuantity(BqlField = typeof(AMMTranSplit.qty))]
        [PXUIField(DisplayName = "Quantity")]
        public virtual Decimal? Qty { get; set; }
        #endregion
        #region BaseQty

        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        [PXDBQuantity(BqlField = typeof(AMMTranSplit.baseQty))]
        public virtual Decimal? BaseQty { get; set; }
        #endregion
		#region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        [PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode = true, BqlField = typeof(AMMTranSplit.lotSerialNbr))]
		[PXUIField(DisplayName = "Lot/Serial Nbr.")]
        public virtual String LotSerialNbr { get; set; }
        #endregion
		#region ParentLotSerialNbr
        public abstract class parentLotSerialNbr : PX.Data.BQL.BqlString.Field<parentLotSerialNbr> { }

        [PXDBString(INLotSerialStatusByCostCenter.lotSerialNbr.Length, IsUnicode =true, BqlField = typeof(AMMTranSplit.parentLotSerialNbr))]
		[PXUIField(DisplayName = "Parent Lot/Serial Nbr.")]
        public virtual String ParentLotSerialNbr { get; set; }
        #endregion

		#region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        [AMOrderTypeField(BqlField = typeof(AMMTran.orderType))]
        public virtual String OrderType { get; set; }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        [ProductionNbr(BqlField = typeof(AMMTran.prodOrdID))]
        public virtual String ProdOrdID { get; set; }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        [OperationIDField(BqlField = typeof(AMMTran.operationID))]
        public virtual int? OperationID { get; set; }
        #endregion
        #region MatlLineId
        public abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

        [PXDBInt(BqlField = typeof(AMMTran.matlLineId))]
        public virtual Int32? MatlLineId { get; set; }
        #endregion
		#region TranInvtMult

		public abstract class tranInvtMult : PX.Data.BQL.BqlShort.Field<tranInvtMult> { }

		[PXDBShort(BqlField = typeof(AMMTran.invtMult))]
		public virtual Int16? TranInvtMult { get; set; }

		#endregion
	}
}
