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
using System;

namespace PX.Objects.IN
{
	[PXProjection(typeof(Select4<INSiteStatusByCostCenter,
		Aggregate<
			Max<INSiteStatusByCostCenter.lastModifiedDateTime,
			Sum<INSiteStatusByCostCenter.qtyAvail,
			GroupBy<INSiteStatusByCostCenter.inventoryID>>>>>))]
	[PXCacheName("Sum of Inventory Qtys by InventoryID with LastModifiedDateTime", PXDacType.History)]
	public class INSiteStatusQtyAggregated : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[Inventory(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.inventoryID))]
		public virtual Int32? InventoryID { get; set; }
		#endregion

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		[PXDBDecimal(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBDate(BqlField = typeof(INSiteStatusByCostCenter.lastModifiedDateTime))]
		[PXUIField(DisplayName = "Last Modified Date Time")]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
	}
}
