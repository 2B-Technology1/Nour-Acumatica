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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.IN
{
	[PXHidden]
	public class INItemSiteHistByCostCenterD : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemSiteHistByCostCenterD>.By<siteID, inventoryID, subItemID, costCenterID, sDate>
		{
			public static INItemSiteHistByCostCenterD Find(PXGraph graph, int? siteID, int? inventoryID, int? subItemID, int? costCenterID, DateTime? sDate)
				=> FindBy(graph, siteID, inventoryID, subItemID, costCenterID, sDate);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemSiteHistByCostCenterD>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemSiteHistByCostCenterD>.By<siteID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemSiteHistByCostCenterD>.By<subItemID> { }
			public class ItemSiteReplenishment : INItemSiteReplenishment.PK.ForeignKeyOf<INItemSiteHistByCostCenterD>.By<inventoryID, siteID, subItemID> { }
		}
		#endregion
		#region SiteID
		public abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		[Site(IsKey = true)]
		[PXDefault]
		public virtual int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[StockItem(IsKey = true)]
		[PXDefault]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true)]
		[PXDefault]
		public virtual int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion
		#region SDate
		public abstract class sDate : Data.BQL.BqlDateTime.Field<sDate> { }
		[PXDBDate(IsKey = true)]
		[PXDefault]
		public virtual DateTime? SDate
		{
			get;
			set;
		}
		#endregion

		#region SYear
		public abstract class sYear : Data.BQL.BqlInt.Field<sYear> { }
		[PXDBInt]
		public virtual int? SYear
		{
			get;
			set;
		}
		#endregion
		#region SMonth
		public abstract class sMonth : Data.BQL.BqlInt.Field<sMonth> { }
		[PXDBInt]
		public virtual int? SMonth
		{
			get;
			set;
		}
		#endregion
		#region SQuater
		public abstract class sQuater : Data.BQL.BqlInt.Field<sQuater> { }
		[PXDBInt]
		public virtual int? SQuater
		{
			get;
			set;
		}
		#endregion
		#region SDay
		public abstract class sDay : Data.BQL.BqlInt.Field<sDay> { }
		[PXDBInt]
		public virtual int? SDay
		{
			get;
			set;
		}
		#endregion
		#region SDayOfWeek
		public abstract class sDayOfWeek : Data.BQL.BqlInt.Field<sDayOfWeek> { }
		[PXDBInt]
		public virtual int? SDayOfWeek
		{
			get;
			set;
		}
		#endregion
		#region QtyReceived
		public abstract class qtyReceived : Data.BQL.BqlDecimal.Field<qtyReceived> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Received")]
		public virtual decimal? QtyReceived
		{
			get;
			set;
		}
		#endregion
		#region QtyIssued
		public abstract class qtyIssued : Data.BQL.BqlDecimal.Field<qtyIssued> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Issued")]
		public virtual decimal? QtyIssued
		{
			get;
			set;
		}
		#endregion
		#region QtySales
		public abstract class qtySales : Data.BQL.BqlDecimal.Field<qtySales> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Sales")]
		public virtual decimal? QtySales
		{
			get;
			set;
		}
		#endregion
		#region QtyCreditMemos
		public abstract class qtyCreditMemos : Data.BQL.BqlDecimal.Field<qtyCreditMemos> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Memos")]
		public virtual decimal? QtyCreditMemos
		{
			get;
			set;
		}
		#endregion
		#region QtyDropShipSales
		public abstract class qtyDropShipSales : Data.BQL.BqlDecimal.Field<qtyDropShipSales> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop Ship Sales")]
		public virtual decimal? QtyDropShipSales
		{
			get;
			set;
		}
		#endregion
		#region QtyTransferIn
		public abstract class qtyTransferIn : Data.BQL.BqlDecimal.Field<qtyTransferIn> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer In")]
		public virtual decimal? QtyTransferIn
		{
			get;
			set;
		}
		#endregion
		#region QtyTransferOut
		public abstract class qtyTransferOut : Data.BQL.BqlDecimal.Field<qtyTransferOut> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transfer Out")]
		public virtual decimal? QtyTransferOut
		{
			get;
			set;
		}
		#endregion
		#region QtyAssemblyIn
		public abstract class qtyAssemblyIn : Data.BQL.BqlDecimal.Field<qtyAssemblyIn> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly In")]
		public virtual decimal? QtyAssemblyIn
		{
			get;
			set;
		}
		#endregion
		#region QtyAssemblyOut
		public abstract class qtyAssemblyOut : Data.BQL.BqlDecimal.Field<qtyAssemblyOut> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Assembly Out")]
		public virtual decimal? QtyAssemblyOut
		{
			get;
			set;
		}
		#endregion
		#region QtyAdjusted
		public abstract class qtyAdjusted : Data.BQL.BqlDecimal.Field<qtyAdjusted> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Adjusted")]
		public virtual decimal? QtyAdjusted
		{
			get;
			set;
		}
		#endregion
		#region BegQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beginning Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? BegQty
		{
			get;
			set;
		}
		public abstract class begQty : Data.BQL.BqlDecimal.Field<begQty> { }
		#endregion
		#region EndQty
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? EndQty
		{
			get;
			set;
		}
		public abstract class endQty : Data.BQL.BqlDecimal.Field<endQty> { }
		#endregion
		#region QtyDebit
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? QtyDebit
		{
			get;
			set;
		}
		public abstract class qtyDebit : Data.BQL.BqlDecimal.Field<qtyDebit> { }
		#endregion
		#region QtyCredit
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Qty.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? QtyCredit
		{
			get;
			set;
		}
		public abstract class qtyCredit : Data.BQL.BqlDecimal.Field<qtyCredit> { }
		#endregion
		#region CostDebit
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CostDebit
		{
			get;
			set;
		}
		public abstract class costDebit : Data.BQL.BqlDecimal.Field<costDebit> { }
		#endregion
		#region CostCredit
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CostCredit
		{
			get;
			set;
		}
		public abstract class costCredit : Data.BQL.BqlDecimal.Field<costCredit> { }
		#endregion
		#region EndCost
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? EndCost
		{
			get;
			set;
		}
		public abstract class endCost : PX.Data.BQL.BqlDecimal.Field<endCost> { }
		#endregion
		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(RecordComesFirst = true)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
