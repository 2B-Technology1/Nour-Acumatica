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
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.Update;
using PX.Objects.IN;
using PX.Data.BQL.Fluent;
using System.Linq;
using PX.Data.BQL;
using PX.Objects.Common.Bql;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing BOM Material Where Used Inquiry
    /// </summary>
    public class BOMWhereUsedInq : PXGraph<BOMWhereUsedInq>
    {
        public PXFilter<BomWhereUsedFilter> Filter;
        public PXCancel<BomWhereUsedFilter> Cancel;

        public PXSelect<BomWhereUsedDetail> BOMWhereUsedRecs;

        public BOMWhereUsedInq()
        {
            this.BOMWhereUsedRecs.Cache.AllowInsert = false;
            this.BOMWhereUsedRecs.Cache.AllowDelete = false;
            this.BOMWhereUsedRecs.Cache.AllowUpdate = false;
        }

        protected virtual IEnumerable bOMWhereUsedRecs()
        {
            return LoadAllData();
        }

        public virtual List<BomWhereUsedDetail> LoadAllData()
        {
            if(Filter.Current != null)
            {
                Filter.Current.Sequence = 0;
            }

            List<BomWhereUsedDetail> bomWhereUsedRecs = new List<BomWhereUsedDetail>();  

            if (Filter.Current.InventoryID == null && Filter.Current.ItemClassID == null)
            {
                return bomWhereUsedRecs;
            }

            PXSelectBase<BomWhereUsedDetail> slctdmatl = new PXSelectJoin<BomWhereUsedDetail,
                InnerJoin<InventoryItem, On<BomWhereUsedDetail.inventoryID, Equal<InventoryItem.inventoryID>>,
                InnerJoin<AMBomItem, On<BomWhereUsedDetail.bOMID, Equal<AMBomItem.bOMID>,
                    And<BomWhereUsedDetail.revisionID, Equal<AMBomItem.revisionID>>>,
                LeftJoin<INItemSite, On<BomWhereUsedDetail.inventoryID, Equal<INItemSite.inventoryID>,
                    And<INItemSite.siteID, Equal<IsNull<BomWhereUsedDetail.siteID, AMBomItem.siteID>>>>>>>,
                Where<BomWhereUsedDetail.expDate, IsNull, 
                    Or<BomWhereUsedDetail.expDate, GreaterEqual<Today>>>,
                OrderBy<
                    Asc<BomWhereUsedDetail.inventoryID, 
                    Asc<BomWhereUsedDetail.bOMID, 
                    Asc<BomWhereUsedDetail.revisionID>>>>>(this);

			if (Filter.Current.InventoryID != null)
            {
                slctdmatl.WhereAnd<Where<BomWhereUsedDetail.inventoryID, Equal<Current<BomWhereUsedFilter.inventoryID>>>>();
            }
            
            if (InventoryHelper.SubItemFeatureEnabled && Filter.Current.SubItemID != null)
            {
                slctdmatl.WhereAnd<Where<BomWhereUsedDetail.subItemID, Equal<Current<BomWhereUsedFilter.subItemID>>>>();
            }

            if (Filter.Current.SiteID != null)
            {
                slctdmatl.WhereAnd<Where<AMBomItem.siteID, Equal<Current<BomWhereUsedFilter.siteID>>>>();
            }

            if (Filter.Current.ItemClassID != null)
            {
                slctdmatl.WhereAnd<Where<InventoryItem.itemClassID, Equal<Current<BomWhereUsedFilter.itemClassID>>>>();
            }

			if (Filter.Current.ShowActiveOnly.GetValueOrDefault())
			{
				slctdmatl.WhereAnd<Where<AMBomItem.status, Equal<AMBomStatus.active>>>();
			}

			foreach (PXResult<BomWhereUsedDetail, InventoryItem, AMBomItem, INItemSite> result in slctdmatl.Select())
            {
                var matlItem = (BomWhereUsedDetail)result;
                var bomItem = (AMBomItem)result;
                var invtItem = (InventoryItem)result;
                var itemSite = (INItemSite) result;				

                if (string.IsNullOrWhiteSpace(matlItem?.BOMID) || string.IsNullOrWhiteSpace(bomItem?.BOMID) || invtItem?.InventoryID == null)
                {
                    continue;
                }

                var record = matlItem;
                record.Level = 1;
                record.ParentInventoryID = bomItem.InventoryID;
                record.ParentSubItemID = bomItem.SubItemID;
                record.QtyRequired = matlItem.QtyReq;
                record.Description = matlItem.Descr;
                record.ItemClassID = invtItem.ItemClassID;
                record.IsStockItem = invtItem.StkItem;
                record.Source = itemSite?.ReplenishmentSource;
				record.BOMStatus = bomItem.Status;
				record.EffStartDate = bomItem.EffStartDate;
				record.EffEndDate = bomItem.EffEndDate;
				record.BOMSiteID = bomItem.SiteID;

				var matlCury = AMBomMatlCury.PK.Find(this, matlItem.BOMID, matlItem.RevisionID, matlItem.OperationID, matlItem.LineID,
					Accessinfo.BaseCuryID);

				record.SiteID = matlCury?.SiteID;
				record.LocationID = matlCury?.LocationID;
				record.UnitCost = matlCury?.UnitCost;

                record.Sequence = Filter.Current.Sequence.GetValueOrDefault();

                var parentInvtItem = GetParentInventoryItem(record);
                if (parentInvtItem != null)
                {
                    record.ParentDescription = parentInvtItem.Descr;
                    record.ParentItemClassID = parentInvtItem.ItemClassID;
                }

				AddIfNotExist(bomWhereUsedRecs, record);

				if (Filter.Current.MultiLevel.GetValueOrDefault())
                {
                    try
                    {
                        LoadDataRecords(record, 2, bomWhereUsedRecs);
                    }
                    catch(Exception)
                    {
                        if (Filter.Current != null)
                        {
                            Filter.Current.MultiLevel = false;
                        }
                        throw;
                    }
                }
            }
            return bomWhereUsedRecs;
        }

        protected virtual InventoryItem GetParentInventoryItem(BomWhereUsedDetail bomWhereUsedDetail)
        {
            if (bomWhereUsedDetail?.ParentInventoryID == null)
            {
                throw new PXArgumentException(nameof(bomWhereUsedDetail));
            }

            return GetInventoryItem(bomWhereUsedDetail.ParentInventoryID);
        }

        protected virtual InventoryItem GetInventoryItem(int? inventoryID)
        {
            if (inventoryID.GetValueOrDefault() == 0)
            {
                return null;
            }

            var parentInvtItem = (InventoryItem)this.Caches<InventoryItem>().Locate(new InventoryItem() { InventoryID = inventoryID });

            if (parentInvtItem != null)
            {
                return parentInvtItem;
            }

            return PXSelect<InventoryItem,
                Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
        }

        /// <summary>
        /// Checks for recursive items found in the multi level where used process
        /// </summary>
        protected virtual void CheckRecursive(Dictionary<int, BomWhereUsedDetail> bomInventoryItems, AMBomItem bomItem, int level, BomWhereUsedDetail callingBomWhereUsedDetail)
        {
            int key = bomItem.InventoryID.GetValueOrDefault();
            if (bomInventoryItems.ContainsKey(key))
            {
                var value = bomInventoryItems[key];
                if ((InventoryHelper.SubItemFeatureEnabled && value.ParentSubItemID.GetValueOrDefault() != bomItem.SubItemID.GetValueOrDefault())
                    || value.Level >= level)
                {
                    //Currently allow same item when sub items enabled and different subitemid
                    return;
                }

                var item1 = GetInventoryItem(key);
                var item2 = GetInventoryItem(callingBomWhereUsedDetail.ParentInventoryID);
                var msg =
                    $"Recursive item found while processing BOM ID '{callingBomWhereUsedDetail.BOMID.TrimIfNotNullEmpty()}'";
                if (item1 != null && item2 != null)
                {
                    msg =
                        $"Recursive item '{item1.InventoryCD.TrimIfNotNullEmpty()}' found at level {level} as BOM material in BOM ID '{callingBomWhereUsedDetail.BOMID.TrimIfNotNullEmpty()}' of item '{item2.InventoryCD.TrimIfNotNullEmpty()}'";
                }

                throw new PXException(msg);
            }
        }

        public virtual void LoadDataRecords(BomWhereUsedDetail callingBomWhereUsedDetail, int level, List<BomWhereUsedDetail> bomWhereUsedRecs)
        {
            if (callingBomWhereUsedDetail?.InventoryID == null || callingBomWhereUsedDetail.ParentInventoryID == null)
            {
                throw new PXArgumentException(nameof(callingBomWhereUsedDetail));
            }

            LoadDataRecords(callingBomWhereUsedDetail, level, bomWhereUsedRecs, null);
        }

        public virtual void LoadDataRecords(BomWhereUsedDetail callingBomWhereUsedDetail, int level, List<BomWhereUsedDetail> bomWhereUsedRecs, Dictionary<int, BomWhereUsedDetail> bomInventoryItems )
        {
            if (Filter?.Current == null)
            {
                throw new PXArgumentException("Filter.Current");
            }

            if (callingBomWhereUsedDetail?.InventoryID == null || callingBomWhereUsedDetail.ParentInventoryID == null)
            {
                throw new PXArgumentException(nameof(callingBomWhereUsedDetail));
            }

            if(bomInventoryItems == null)
            {
                bomInventoryItems = new Dictionary<int, BomWhereUsedDetail>();
            }

            var levelBomInventoryItems = new Dictionary<int, BomWhereUsedDetail>();
            levelBomInventoryItems.AddRange(bomInventoryItems);

            if (!levelBomInventoryItems.ContainsKey(callingBomWhereUsedDetail.ParentInventoryID.GetValueOrDefault()))
            {
                levelBomInventoryItems.Add(callingBomWhereUsedDetail.ParentInventoryID.GetValueOrDefault(), callingBomWhereUsedDetail);
            }

            if (level >= LowLevel.MaxLowLevel)
            {
                throw new MaxLowLevelException(
                    $"Max level '{level}' reached while processing BOM {callingBomWhereUsedDetail.BOMID}") {Level = level};
            }

            PXSelectBase<BomWhereUsedDetail> slctdmatl = new PXSelectJoin<BomWhereUsedDetail, 
                InnerJoin<AMBomItem, On<BomWhereUsedDetail.bOMID, Equal<AMBomItem.bOMID>,
                    And<BomWhereUsedDetail.revisionID, Equal<AMBomItem.revisionID>>>,
                InnerJoin<InventoryItem, On<AMBomItem.inventoryID, Equal<InventoryItem.inventoryID>>>>,
                    Where<BomWhereUsedDetail.inventoryID, Equal<Required<BomWhereUsedDetail.inventoryID>>,
                        And<Where<BomWhereUsedDetail.expDate, IsNull, 
                            Or<BomWhereUsedDetail.expDate, GreaterEqual<Today>>>>>,
                OrderBy<
                    Asc<BomWhereUsedDetail.inventoryID, 
                    Asc<BomWhereUsedDetail.bOMID,
                    Asc<BomWhereUsedDetail.revisionID>>>>>(this);

            if (Filter.Current.SiteID != null)
            {
                slctdmatl.WhereAnd<Where<AMBomItem.siteID, Equal<Current<BomWhereUsedFilter.siteID>>>>();
            }

            foreach (PXResult<BomWhereUsedDetail, AMBomItem, InventoryItem> result in slctdmatl.Select(callingBomWhereUsedDetail.ParentInventoryID))
            {
                var matlItem = (BomWhereUsedDetail) result;
                var bomItem = (AMBomItem) result;
                var inventoryItem = (InventoryItem) result;

                if (matlItem?.InventoryID == null || bomItem?.InventoryID == null || inventoryItem?.InventoryID == null)
                {
                    continue;
                }

                CheckRecursive(levelBomInventoryItems, bomItem, level, callingBomWhereUsedDetail);

                var record = matlItem;

                record.InventoryID = callingBomWhereUsedDetail.InventoryID;
                record.SubItemID = callingBomWhereUsedDetail.SubItemID;
                record.Level = level;
                record.ParentInventoryID = bomItem.InventoryID;
                record.ParentSubItemID = bomItem.SubItemID;
                record.ParentDescription = inventoryItem.Descr;
                record.ParentItemClassID = inventoryItem.ItemClassID;
                record.QtyRequired = callingBomWhereUsedDetail.QtyRequired;
                record.BatchSize = callingBomWhereUsedDetail.BatchSize.GetValueOrDefault();
                record.UOM = callingBomWhereUsedDetail.UOM;
                record.ItemClassID = callingBomWhereUsedDetail.ItemClassID;
                record.IsStockItem = callingBomWhereUsedDetail.IsStockItem;
                record.Source = callingBomWhereUsedDetail.Source;
                record.SiteID = bomItem.SiteID;
                record.Description = callingBomWhereUsedDetail.Description;
                record.Sequence = Filter.Current.Sequence.GetValueOrDefault();
				record.BOMStatus = bomItem.Status;
				record.EffStartDate = bomItem.EffStartDate;
				record.EffEndDate = bomItem.EffEndDate;
				record.BOMSiteID = bomItem.SiteID;

				AddIfNotExist(bomWhereUsedRecs, record);

				if (Filter.Current.MultiLevel.GetValueOrDefault())
                {
                    try
                    {
                        LoadDataRecords(record, level + 1, bomWhereUsedRecs, levelBomInventoryItems);
                    }
                    catch (Exception e)
                    {
                        if (e is MaxLowLevelException && ((MaxLowLevelException)e).Level <= level + 1)
                        {
                            var item = GetInventoryItem(callingBomWhereUsedDetail.InventoryID);

                            var itemCD = item == null
                                ? Messages.GetLocal(Messages.RecordIDEqual, callingBomWhereUsedDetail.InventoryID.GetValueOrDefault())
                                : item.InventoryCD.TrimIfNotNullEmpty();

                            PXTrace.WriteInformation(Messages.GetLocal(Messages.ErrorProcessingMultLevelWhereUsedInqMatl,
                                itemCD, record.BOMID.TrimIfNotNullEmpty(), level + 1, e.Message));
                        }
                        throw;
                    }
                }
            }
        }

        public PXAction<BomWhereUsedFilter> ViewBOM;
        [PXUIField(DisplayName = "View BOM")]
        [PXLookupButton]
        protected virtual void viewBOM()
        {
            BOMMaint.Redirect(BOMWhereUsedRecs?.Current?.BOMID, BOMWhereUsedRecs?.Current?.RevisionID);
        }

		protected virtual void AddIfNotExist(List<BomWhereUsedDetail> bomWhereUsedRecs, BomWhereUsedDetail record)
		{
			if (!bomWhereUsedRecs.Any(a => a.BOMID == record.BOMID
											   && a.RevisionID == record.RevisionID
											   && a.InventoryID == record.InventoryID
											   && a.Level == record.Level
											   && a.OperationID == record.OperationID))
				bomWhereUsedRecs.Add(record);
		}
	}
    
    /// <summary>
    /// Where used inquiry filter
    /// </summary>
    [Serializable]
    [PXCacheName("BOM Where Used Filter")]
    public class BomWhereUsedFilter : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory]
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
        [SubItem(typeof(BomWhereUsedFilter.inventoryID))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [AMSite(DisplayName = "BOM Warehouse")]
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
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

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
        #region MultiLevel
        public abstract class multiLevel : PX.Data.BQL.BqlBool.Field<multiLevel> { }

        protected Boolean? _MultiLevel;
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Show Multilevel Results")]
        public virtual Boolean? MultiLevel
        {
            get
            {
                return this._MultiLevel;
            }
            set
            {
                this._MultiLevel = value;
            }
        }
        #endregion
        #region Sequence
        public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }

        protected Int32? _Sequence;
        /// <summary>
        /// Keep track of the records being inserted into the rows for display
        /// </summary>
        [PXInt]
        public virtual Int32? Sequence
        {
            get
            {
                this._Sequence = this._Sequence.GetValueOrDefault() + 1;
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
            }
        }
		#endregion
		#region ShowActiveOnly
		public abstract class showActiveOnly : PX.Data.BQL.BqlBool.Field<showActiveOnly> { }
		[PXBool()]
		[PXUIField(DisplayName = "Show Active BOMs Only", Visibility = PXUIVisibility.Visible)]
		[PXUnboundDefault(true)]
		public virtual Boolean? ShowActiveOnly { get; set; }
		#endregion
	}

	/// <summary>
	/// Where used inquiry detail
	/// </summary>
	[PXProjection(typeof(Select<AMBomMatl>), Persistent = false)]
    [Serializable]
    [PXCacheName("BOM Where Used Detail")]
    public class BomWhereUsedDetail : AMBomMatl
    {
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }

        protected Int32? _Level;
        [PXInt]
        [PXUIField(DisplayName = "Level")]
        public virtual Int32? Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
            }
        }
        #endregion
        #region ParentInventoryID
        public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

        protected Int32? _ParentInventoryID;
        [Inventory(DisplayName = "Parent Inventory ID", BqlField = typeof(AMBomMatl.inventoryID),
            Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Int32? ParentInventoryID
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
        [SubItem(DisplayName = "Parent Subitem", BqlField = typeof(AMBomMatl.subItemID))]
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
        #region QtyRequired
        public abstract class qtyRequired : PX.Data.BQL.BqlDecimal.Field<qtyRequired> { }

        protected decimal? _QtyRequired;
        [PXQuantity]
        [PXUIField(DisplayName = "Qty Required")]
        public virtual decimal? QtyRequired
        {
            get
            {
                return this._QtyRequired;
            }
            set
            {
                this._QtyRequired = value;
            }
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXInt]
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
        #region Source
        public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

        protected String _Source;
        [PXString]
        [INReplenishmentSource.List]
        public virtual String Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXString]
        [PXUIField(DisplayName = "Description")]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
        #region ParentDescription
        public abstract class parentDescription : PX.Data.BQL.BqlString.Field<parentDescription> { }

        protected String _ParentDescription;
        [PXString]
        [PXUIField(DisplayName = "Parent Desc.")]
        public virtual String ParentDescription
        {
            get
            {
                return this._ParentDescription;
            }
            set
            {
                this._ParentDescription = value;
            }
        }
        #endregion
        #region ParentItemClassID
        public abstract class parentItemClassID : PX.Data.BQL.BqlInt.Field<parentItemClassID> { }

        protected int? _ParentItemClassID;
        [PXInt]
        [PXUIField(DisplayName = "Parent Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ParentItemClassID
        {
            get
            {
                return this._ParentItemClassID;
            }
            set
            {
                this._ParentItemClassID = value;
            }
        }
        #endregion
        #region Sequence
        public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }

        protected Int32? _Sequence;
        /// <summary>
        /// Keep track of the records being inserted into the rows for display/sort order
        /// </summary>
        [PXInt(IsKey = true)]
        [PXUIField(DisplayName = "Sequence", Visible = false, Enabled = false)]
        public virtual Int32? Sequence
        {
            get
            {
                return this._Sequence;
            }
            set
            {
                this._Sequence = value;
            }
        }
        #endregion
        #region CompBOMID (override)
        [BomID(DisplayName = "Comp BOM ID")]
        [BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
            LeftJoin<InventoryItem, On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<AMBomItemActive.inventoryID, Equal<Current<AMBomMatl.inventoryID>>>>))]
        public override String CompBOMID
        {
            get
            {
                return this._CompBOMID;
            }
            set
            {
                this._CompBOMID = value;
            }
        }
		#endregion
		#region BOMStatus
		public abstract class bOMStatus : PX.Data.BQL.BqlString.Field<bOMStatus> { }
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Revision Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[AMBomStatus.List]
		public virtual string BOMStatus { get; set; }
		#endregion
		#region EffStartDate
		public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Revision Start Date")]
		public virtual DateTime? EffStartDate { get; set; }
		#endregion
		#region EffEndDate
		public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Revision End Date")]
		public virtual DateTime? EffEndDate { get; set; }
		#endregion
		#region BOMSiteID
		public abstract class bOMSiteID : PX.Data.BQL.BqlInt.Field<bOMSiteID> { }

		[PXInt]
		[PXUIField(DisplayName = "BOM Warehouse", Visibility = PXUIVisibility.Visible, FieldClass = SiteAttribute.DimensionName)]
		[PXDimensionSelector("INSITE", typeof(SearchFor<INSite.siteID>.In<SelectFrom<INSite>.
					Where<INSite.siteID.IsEqual<bOMSiteID.FromCurrent>>>), typeof(INSite.siteCD))]
		public virtual Int32? BOMSiteID { get; set; }
		#endregion
	}
}
