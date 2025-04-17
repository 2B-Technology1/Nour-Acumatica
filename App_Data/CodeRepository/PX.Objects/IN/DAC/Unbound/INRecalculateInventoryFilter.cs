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
using PX.Objects.GL;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INRecalculateInventoryFilter)]
	public partial class INRecalculateInventoryFilter : IBqlTable
	{
		#region SiteID
		[Site]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region ItemClassID
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class")]
		[PXDimensionSelector(INItemClass.Dimension,
			typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem.IsEqual<True>>>),
			typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
		public virtual int? ItemClassID { get; set; }
		public abstract class itemClassID : BqlInt.Field<itemClassID> { }
		#endregion

		#region InventoryID
		[AnyInventory(typeof(SelectFrom<InventoryItem>
			.Where<itemClassID.FromCurrent.IsNull.Or<InventoryItem.itemClassID.IsEqual<itemClassID.FromCurrent>>>
			.SearchFor<InventoryItem.inventoryID>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.inactive>>),
			Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region RebuildHistory
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Rebuild Item History Since:")]
		public virtual bool? RebuildHistory { get; set; }
		public abstract class rebuildHistory : BqlBool.Field<rebuildHistory> { }
		#endregion

		#region FinPeriodID
		[FinPeriodNonLockedSelector(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual string FinPeriodID { get; set; }
		public abstract class finPeriodID : BqlString.Field<finPeriodID> { }
		#endregion

		#region ReplanBackorders
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Replan Back Orders")]
		public virtual bool? ReplanBackorders { get; set; }
		public abstract class replanBackorders : BqlBool.Field<replanBackorders> { }
		#endregion

		#region ShowOnlyAllocatedItems
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Exclude Items Without Allotted Plan Types")]
		public virtual bool? ShowOnlyAllocatedItems { get; set; }
		public abstract class showOnlyAllocatedItems : BqlBool.Field<showOnlyAllocatedItems> { }
		#endregion
	}
}
