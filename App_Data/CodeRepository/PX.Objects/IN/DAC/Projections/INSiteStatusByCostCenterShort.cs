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
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	/// <summary>
	/// A shortened version of the <see cref="INSiteStatusByCostCenter"/> DAC which includes only commonly used fields with item quantities.
	/// </summary>
	[PXCacheName(Messages.INSiteStatusByCostCenterShort)]
	[PXProjection(typeof(SelectFrom<INSiteStatusByCostCenter>), Persistent = false)]
	public class INSiteStatusByCostCenterShort: IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteStatusByCostCenterShort>.By<inventoryID, subItemID, siteID, costCenterID>
		{
			public static INSiteStatusByCostCenterShort Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? costCenterID)
				=> FindBy(graph, inventoryID, subItemID, siteID, costCenterID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteStatusByCostCenterShort>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INSiteStatusByCostCenterShort>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INSiteStatusByCostCenterShort>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INSiteStatusByCostCenterShort>.By<inventoryID, siteID> { }
		}
		#endregion

		#region InventoryID
		/// <exclude/>
		[PXDBInt(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		/// <exclude/>
		[PXDBInt(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.subItemID))]
		public virtual int? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion
		#region SiteID
		/// <exclude/>
		[PXDBInt(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.siteID))]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion
		#region CostCenterID
		/// <exclude/>
		[PXDBInt(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.costCenterID))]
		[PXDefault]
		public virtual int? CostCenterID { get; set; }
		public abstract class costCenterID : BqlInt.Field<costCenterID> { }
		#endregion

		#region QtyOnHand
		/// <exclude/>
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		public virtual decimal? QtyOnHand { get; set; }
		public abstract class qtyOnHand : BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region QtyNotAvail
		/// <exclude/>
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyNotAvail))]
		public virtual decimal? QtyNotAvail { get; set; }
		public abstract class qtyNotAvail : BqlDecimal.Field<qtyNotAvail> { }
		#endregion
		#region QtyAvail
		/// <exclude/>
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		public virtual decimal? QtyAvail { get; set; }
		public abstract class qtyAvail : BqlDecimal.Field<qtyAvail> { }
		#endregion
		#region QtyHardAvail
		/// <exclude/>
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyHardAvail))]
		public virtual decimal? QtyHardAvail { get; set; }
		public abstract class qtyHardAvail : BqlDecimal.Field<qtyHardAvail> { }
		#endregion
	}
}
