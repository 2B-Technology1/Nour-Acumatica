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

namespace PX.Objects.IN.AffectedAvailability
{
	[PXHidden]
	[PXProjection(typeof(Select4<INSiteStatusByCostCenter,
		Aggregate<GroupBy<INSiteStatusByCostCenter.inventoryID, GroupBy<INSiteStatusByCostCenter.siteID,
			Sum<INSiteStatusByCostCenter.qtyOnHand, Sum<INSiteStatusByCostCenter.qtyHardAvail>>>>>>))]
	public class INSiteStatusGroup : PX.Data.IBqlTable
	{
		#region InventoryID
		[Inventory(BqlField = typeof(INSiteStatusByCostCenter.inventoryID), IsKey = true)]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		[Site(BqlField = typeof(INSiteStatusByCostCenter.siteID), IsKey = true)]
		public virtual Int32? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region QtyOnHand
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		public virtual Decimal? QtyOnHand { get; set; }
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region QtyHardAvail
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyHardAvail))]
		public virtual Decimal? QtyHardAvail { get; set; }
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		#endregion
	}
}
