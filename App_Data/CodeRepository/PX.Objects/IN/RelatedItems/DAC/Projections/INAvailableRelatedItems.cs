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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Bql;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.RelatedItems
{
    [PXProjection(typeof(
        SelectFrom<INRelatedInventory>
            .InnerJoin<InventoryItem>
                .On<INRelatedInventory.FK.RelatedInventoryItem
                .And<INRelatedInventory.isActive.IsEqual<True>>
                .And<InventoryItem.isTemplate.IsNotEqual<True>>
                .And<InventoryItem.itemStatus.IsNotIn<InventoryItemStatus.unknown, InventoryItemStatus.inactive, InventoryItemStatus.markedForDeletion, InventoryItemStatus.noSales>>
                .And<CurrentMatch<InventoryItem, AccessInfo.userName>>>
            .LeftJoin<INSiteStatusByCostCenter>
                .On<INSiteStatusByCostCenter.inventoryID.IsEqual<INRelatedInventory.relatedInventoryID>
                .And<INSiteStatusByCostCenter.siteID.IsNotEqual<SiteAttribute.transitSiteID>
				.And<INSiteStatusByCostCenter.costCenterID.IsEqual<CostCenter.freeStock>>>>
            .LeftJoin<INSubItem>
                .On<INSiteStatusByCostCenter.FK.SubItem>
            .LeftJoin<INSite>
                .On<INSiteStatusByCostCenter.FK.Site>
            .Where<
                Brackets<INSubItem.subItemID.IsNull.Or<CurrentMatch<INSubItem, AccessInfo.userName>>>
                .And<INSite.siteID.IsNull.Or<CurrentMatch<INSite, AccessInfo.userName>>>>),
        Persistent = false)]
    [PXHidden]
    public class INAvailableRelatedItems : IBqlTable
    {
        #region OriginalInventoryID
        [PXDBInt(BqlField = typeof(INRelatedInventory.inventoryID))]
        public int? OriginalInventoryID { get; set; }
        public abstract class originalInventoryID : BqlInt.Field<originalInventoryID> { }
        #endregion

        #region InventoryID 
        [PXDBInt(BqlField = typeof(INRelatedInventory.relatedInventoryID))]
        public int? InventoryID { get; set; }
        public abstract class relatedInventoryID : BqlInt.Field<relatedInventoryID> { }
        #endregion

        #region StkItem 
        [PXDBBool(BqlField = typeof(InventoryItem.stkItem))]
        public virtual bool? StkItem { get; set; }
        public abstract class stkItem : BqlBool.Field<stkItem> { }
        #endregion

        #region EffectiveDate
        [PXDBDate(BqlField = typeof(INRelatedInventory.effectiveDate))]
        public DateTime? EffectiveDate { get; set; }
        public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
        #endregion

        #region ExpirationDate
        [PXDBDate(BqlField = typeof(INRelatedInventory.expirationDate))]
        public DateTime? ExpirationDate { get; set; }
        public abstract class expirationDate : BqlDateTime.Field<expirationDate> { }
        #endregion

        #region SubItemID
        [PXDBInt(BqlField = typeof(INSiteStatusByCostCenter.subItemID))]
        public virtual int? SubItemID { get; set; }
        public abstract class subItemID : BqlInt.Field<subItemID> { }
        #endregion

        #region SiteID
        [PXDBInt(BqlField = typeof(INSiteStatusByCostCenter.siteID))]
        public virtual int? SiteID { get; set; }
        public abstract class siteID : BqlInt.Field<siteID> { }
        #endregion

        #region BranchID
        [PXDBInt(BqlField = typeof(INSite.branchID))]
        public virtual int? BranchID { get; set; }
        public abstract class branchID : BqlInt.Field<branchID> { }
        #endregion

        #region QtyAvail
        public abstract class qtyAvail : BqlDecimal.Field<qtyAvail> { }
        [PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
        public virtual decimal? QtyAvail { get; set; }
        #endregion

        #region Relation
        [PXDBString(5, IsFixed = true, BqlField = typeof(INRelatedInventory.relation))]
        public string Relation { get; set; }
        public abstract class relation : BqlString.Field<relation> { }
        #endregion

        #region Required
        [PXDBBool(BqlField = typeof(INRelatedInventory.required))]
        public bool? Required { get; set; }
        public abstract class required : BqlBool.Field<required> { }
        #endregion
    }
}
