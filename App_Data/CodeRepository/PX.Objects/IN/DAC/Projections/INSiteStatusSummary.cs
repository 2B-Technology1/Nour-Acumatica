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
using PX.Objects.CS;
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN
{
	[INSiteStatusSummaryProjection]
	[PXCacheName(Messages.INSiteStatusSummary)]
	public class INSiteStatusSummary : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteStatusSummary>.By<inventoryID, siteID>
		{
			public static INSiteStatusSummary Find(PXGraph graph, int? inventoryID, int? siteID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, siteID, options);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteStatusSummary>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INSiteStatusSummary>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INSiteStatusSummary>.By<inventoryID, siteID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.inventoryID))]
		[PXDefault]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		/// <exclude />
		[Site(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.siteID))]
		[PXDefault]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		/// <exclude />
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		/// <exclude />
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyNotAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Not Available")]
		public virtual Decimal? QtyNotAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : Data.BQL.BqlDecimal.Field<qtyAvail> { }
		/// <exclude />
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get;
			set;
		}
		#endregion
	}
}
