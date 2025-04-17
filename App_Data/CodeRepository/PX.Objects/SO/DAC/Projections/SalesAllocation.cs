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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	/// <exclude/>
	[PXCacheName(Messages.SalesAllocation)]
	[PXProjection(typeof(
		SelectFrom<SOLine>
			.InnerJoin<SOOrder>
				.On<SOLine.FK.Order>
			.InnerJoin<SOOrderType>
				.On<SOOrder.FK.OrderType
				.And<SOOrderType.behavior.IsIn<SOBehavior.bL, SOBehavior.sO, SOBehavior.tR, SOBehavior.rM>>>
			.InnerJoin<SOOrderTypeOperation>
				.On<SOOrderTypeOperation.FK.OrderType
				.And<SOOrderTypeOperation.operation.IsEqual<SOOperation.issue>>
				.And<SOOrderTypeOperation.active.IsEqual<True>>>
			.LeftJoin<Customer>
				.On<SOOrder.FK.Customer>
			.InnerJoin<InventoryItem>
				.On<SOLine.FK.InventoryItem
				.And<InventoryItem.stkItem.IsEqual<True>>
				.And<CurrentMatch<InventoryItem, AccessInfo.userName>>>
			.InnerJoin<SOLineSiteAllocation>
				.On<SOLineSiteAllocation.FK.OrderLine
				.And<SOLineSiteAllocation.siteID.IsEqual<SalesAllocationsFilter.siteID.FromCurrent.Value>>>
		.Where<SOLine.isSpecialOrder.IsNotEqual<True>
			.And<SOOrderType.behavior.IsEqual<SOBehavior.tR>.Or<Customer.bAccountID.IsNotNull.And<CurrentMatch<Customer, AccessInfo.userName>>>>
			.And<SOLine.pOCreate.IsNotEqual<True>.Or<SOLine.pOSource.IsEqual<INReplenishmentSource.purchaseToOrder>>>
			.And<SOLine.completed.IsNotEqual<True>>>
		), Persistent = false)]
	public class SalesAllocation : IBqlTable 
	{
		#region Keys
		public static class FK
		{
			public class SiteStatusByCostCenterShort : INSiteStatusByCostCenterShort.PK.ForeignKeyOf<SalesAllocation>.By<inventoryID, subItemID, lineSiteID, costCenterID> { }
		}
		#endregion

		#region Selected
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		public abstract class selected : BqlBool.Field<selected> { }
		#endregion

		#region Key fields

		#region OrderType
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		[PXUIField(DisplayName = "Order Type")]
		[PXSelector(typeof(SelectFrom<SOOrderType>.SearchFor<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual string OrderType { get; set; }
		public abstract class orderType : BqlString.Field<orderType> { }
		#endregion

		#region OrderNbr
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
		[PXSelector(typeof(SelectFrom<SOOrder>
			.Where<SOOrder.orderType.IsEqual<orderType.FromCurrent>>
			.SearchFor<SOOrder.orderNbr>))]
		[PXUIField(DisplayName = "Order Nbr.")]
		public virtual string OrderNbr { get; set; }
		public abstract class orderNbr : BqlString.Field<orderNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
		[PXUIField(DisplayName = "SO Line Nbr.", Visible = false)]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }
		#endregion

		#endregion

		#region SOLine fields

		#region LineSiteID
		[Site(BqlField = typeof(SOLine.siteID), Visible = false)]
		public virtual int? LineSiteID { get; set; }
		public abstract class lineSiteID : BqlInt.Field<lineSiteID> { }
		#endregion

		#region InventoryID
		[StockItem(BqlField = typeof(SOLine.inventoryID))]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion

		#region SubItemID
		[PXDBInt(BqlField = typeof(SOLine.subItemID))]
		public virtual int? SubItemID { get; set; }
		public abstract class subItemID : BqlInt.Field<subItemID> { }
		#endregion

		#region CostCenterID
		[PXDBInt(BqlField = typeof(SOLine.costCenterID))]
		public virtual int? CostCenterID { get; set; }
		public abstract class costCenterID : BqlInt.Field<costCenterID> { }
		#endregion

		#region LineDesc
		[PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
		[PXUIField(DisplayName = "Description")]
		public virtual string TranDesc { get; set; }
		public abstract class tranDesc : BqlString.Field<tranDesc> { }
		#endregion

		#region UOM
		[INUnit(typeof(SOLine.inventoryID), BqlField = typeof(SOLine.uOM), Visible = false)]
		public virtual string UOM { get; set; }
		public abstract class uOM : BqlString.Field<uOM> { }
		#endregion

		#region LineQty
		[PXDBQuantity(BqlField = typeof(SOLine.orderQty))]
		[PXUIField(DisplayName = "Line Qty.", Visible = false)]
		public virtual decimal? LineQty { get; set; }
		public abstract class lineQty : BqlDecimal.Field<lineQty> { }
		#endregion

		#region BaseLineQty
		[PXDBQuantity(BqlField = typeof(SOLine.baseOrderQty))]
		[PXUIField(DisplayName = "Base Qty.", Visible = false)]
		public virtual decimal? BaseLineQty { get; set; }
		public abstract class baseLineQty : BqlDecimal.Field<baseLineQty> { }
		#endregion

		#region ShipComplete
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
		[SOShipComplete.List]
		[PXUIField(DisplayName = "Line Shipping Rule")]
		public virtual string ShipComplete { get; set; }
		public abstract class shipComplete : BqlString.Field<shipComplete> { }
		#endregion

		#region RequestDate
		[PXDBDate(BqlField = typeof(SOLine.requestDate))]
		[PXUIField(DisplayName = "Line Requested On")]
		public virtual DateTime? RequestDate { get; set; }
		public abstract class requestDate : BqlDateTime.Field<requestDate> { }
		#endregion

		#region ShipDate
		[PXDBDate(BqlField = typeof(SOLine.shipDate))]
		[PXUIField(DisplayName = "Line Ship On")]
		public virtual DateTime? ShipDate { get; set; }
		public abstract class shipDate : BqlDateTime.Field<shipDate> { }
		#endregion

		#region CuryInfoID
		[PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
		public abstract class curyInfoID : BqlLong.Field<curyInfoID> { }
		public virtual long? CuryInfoID { get; set; }
		#endregion

		#region CuryLineAmt
		[CM.PXDBCurrency(typeof(curyInfoID), typeof(lineAmt), BqlField = typeof(SOLine.curyLineAmt))]
		[PXUIField(DisplayName = "Line Amount", Visible = false)]
		public virtual decimal? CuryLineAmt { get; set; }
		public abstract class curyLineAmt : BqlDecimal.Field<curyLineAmt> { }
		#endregion

		#region LineAmt
		[PXDBDecimal(4, BqlField = typeof(SOLine.lineAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? LineAmt { get; set; }
		public abstract class lineAmt : BqlDecimal.Field<lineAmt> { }
		#endregion

		#endregion

		#region SOLineSiteAllocation fields

		#region SplitSiteID
		[Site(DisplayName = "Alloc. Warehouse", BqlField = typeof(SOLineSiteAllocation.siteID), Visible = false)]
		public virtual int? SplitSiteID { get; set; }
		public abstract class splitSiteID : BqlInt.Field<splitSiteID> { }
		#endregion

		#region QtyAllocated
		[PXDBQuantity(BqlField = typeof(SOLineSiteAllocation.qtyAllocated))]
		[PXUIField(DisplayName = "Qty. Allocated")]		
		public virtual decimal? QtyAllocated { get; set; }
		public abstract class qtyAllocated : BqlDecimal.Field<qtyAllocated> { }
		#endregion

		#region QtyUnallocated
		[PXDBQuantity(BqlField = typeof(SOLineSiteAllocation.qtyUnallocated))]
		[PXUIField(DisplayName = "Qty. Unallocated")]
		public virtual decimal? QtyUnallocated { get; set; }
		public abstract class qtyUnallocated : BqlDecimal.Field<qtyUnallocated> { }
		#endregion

		#region LotSerialQtyAllocated
		[PXDBQuantity(BqlField = typeof(SOLineSiteAllocation.lotSerialQtyAllocated))]
		public virtual decimal? LotSerialQtyAllocated { get; set; }
		public abstract class lotSerialQtyAllocated : BqlDecimal.Field<lotSerialQtyAllocated> { }
		#endregion

		#endregion

		#region InventoryItem fields

		#region BaseUOM
		[INUnit(typeof(SOLine.inventoryID), DisplayName = "Base UOM", BqlField = typeof(InventoryItem.baseUnit), Visible = false)]
		public virtual string BaseUOM { get; set; }
		public abstract class baseUOM : BqlString.Field<baseUOM> { }
		#endregion

		#region InventoryCD
		[PXDBString(IsUnicode = true, BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual string InventoryCD { get; set; }
		public abstract class inventoryCD : BqlString.Field<inventoryCD> { }
		#endregion

		#endregion

		#region SOOrder fields

		#region OrderPriority
		[PXDBShort(BqlField = typeof(SOOrder.priority))]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual short? OrderPriority { get; set; }
		public abstract class orderPriority : BqlShort.Field<orderPriority> { }
		#endregion

		#region OrderStatus
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOOrder.status))]
		[PXUIField(DisplayName = "Order Status")]
		[SOOrderStatus.List]
		public virtual string OrderStatus { get; set; }
		public abstract class orderStatus : BqlString.Field<orderStatus> { }
		#endregion

		#region OrderHold
		[PXDBBool(BqlField = typeof(SOOrder.hold))]
		public virtual bool? OrderHold { get; set; }
		public abstract class orderHold : BqlBool.Field<orderHold> { }
		#endregion

		#region OrderDesc
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(SOOrder.orderDesc))]
		[PXUIField(DisplayName = "Order Description", Visible = false)]
		public virtual string OrderDesc { get; set; }
		public abstract class orderDesc : BqlString.Field<orderDesc> { }
		#endregion

		#region CustomerID		
		[CustomerActive(BqlField = typeof(SOOrder.customerID), DescriptionField = null)]
		public virtual int? CustomerID { get; set; }
		public abstract class customerID : BqlInt.Field<customerID> { }
		#endregion

		#region OrderDate
		[PXDBDate(BqlField = typeof(SOOrder.orderDate))]
		[PXUIField(DisplayName = "Order Date")]
		public virtual DateTime? OrderDate { get; set; }
		public abstract class orderDate : BqlDateTime.Field<orderDate> { }
		#endregion

		#region CancelDate
		[PXDBDate(BqlField = typeof(SOOrder.cancelDate))]
		[PXUIField(DisplayName = "Cancel By")]
		public virtual DateTime? CancelDate { get; set; }
		public abstract class cancelDate : BqlDateTime.Field<cancelDate> { }
		#endregion

		#region SalesPersonID
		[SalesPerson(BqlField = typeof(SOOrder.salesPersonID), Visible = false)]
		public virtual int? SalesPersonID { get; set; }
		public abstract class salesPersonID : BqlInt.Field<salesPersonID> { }
		#endregion

		#region CuryID
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(SOOrder.curyID))]
		[PXUIField(DisplayName = "Currency", Visible = false)]
		public virtual string CuryID { get; set; }
		public abstract class curyID : BqlString.Field<curyID> { }
		#endregion

		#region OrderCreatedOn
		[PXDBDateAndTime(BqlField = typeof(SOOrder.createdDateTime))]
		public virtual DateTime? OrderCreatedOn { get; set; }
		public abstract class orderCreatedOn : BqlDateTime.Field<orderCreatedOn> { }
		#endregion

		#region CuryOrderTotal
		[CM.PXDBCurrency(typeof(curyInfoID), typeof(SOOrder.orderTotal), BqlField = typeof(SOOrder.curyOrderTotal))]
		[PXUIField(DisplayName = "Order Total", Visible = false)]
		public virtual decimal? CuryOrderTotal { get; set; }
		public abstract class curyOrderTotal : BqlDecimal.Field<curyOrderTotal> { }
		#endregion

		#region OrderTotal
		[PXDBDecimal(4, BqlField = typeof(SOOrder.orderTotal))]
		public virtual decimal? OrderTotal { get; set; }
		public abstract class orderTotal : BqlDecimal.Field<orderTotal> { }
		#endregion

		#endregion

		#region Customer fields

		#region CustomerName		
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Customer.acctName))]
		[PXUIField(DisplayName = "Customer Name", Visible = false)]
		public virtual string CustomerName { get; set; }
		public abstract class customerName : BqlString.Field<customerName> { }
		#endregion

		#region CustomerClassID
		[PXDBString(10, IsUnicode = true, BqlField = typeof(Customer.customerClassID))]
		[PXSelector(typeof(SelectFrom<CustomerClass>
			.SearchFor<CustomerClass.customerClassID>),
			DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class", Visible = false)]
		public virtual string CustomerClassID { get; set; }
		public abstract class customerClassID : BqlString.Field<customerClassID> { }
		#endregion

		#endregion

		#region QtyHardAvail
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available for Shipping")]
		[PXUIVisible(typeof(SalesAllocationsFilter.action.FromCurrent.IsNotEqual<SalesAllocationsFilter.action.deallocateSalesOrders>))]
		public virtual decimal? QtyHardAvail { get; set; }
		public abstract class qtyHardAvail : BqlDecimal.Field<qtyHardAvail> { }
		#endregion

		#region QtyToAllocate
		[PXQuantity(MinValue = 0)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. to Allocate")]
		[PXUIVisible(typeof(SalesAllocationsFilter.action.FromCurrent.IsNotEqual<SalesAllocationsFilter.action.deallocateSalesOrders>))]
		[PXUIEnabled(typeof(SalesAllocationsFilter.action.FromCurrent.IsNotEqual<SalesAllocationsFilter.action.deallocateSalesOrders>))]
		public virtual decimal? QtyToAllocate { get; set; }
		public abstract class qtyToAllocate : BqlDecimal.Field<qtyToAllocate> { }
		#endregion

		#region QtyToDeallocate
		[PXQuantity(MinValue = 0)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. to Deallocate")]
		[PXUIVisible(typeof(SalesAllocationsFilter.action.FromCurrent.IsEqual<SalesAllocationsFilter.action.deallocateSalesOrders>))]
		[PXUIEnabled(typeof(SalesAllocationsFilter.action.FromCurrent.IsEqual<SalesAllocationsFilter.action.deallocateSalesOrders>))]
		public virtual decimal? QtyToDeallocate { get; set; }
		public abstract class qtyToDeallocate : BqlDecimal.Field<qtyToDeallocate> { }
		#endregion

		#region BufferedQty
		[PXQuantity(MinValue = 0)]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BufferedQty { get; set; }
		public abstract class bufferedQty : BqlDecimal.Field<bufferedQty> { }
		#endregion

		#region BufferedTime
		[PXDateAndTime]
		public virtual DateTime? BufferedTime { get; set; }
		public abstract class bufferedTime : BqlDateTime.Field<bufferedTime> { }
		#endregion

		#region IsExtraAllocation
		[PXBool]
		[PXUnboundDefault(false)]
		public virtual bool? IsExtraAllocation { get; set; }
		public abstract class isExtraAllocation : BqlBool.Field<isExtraAllocation> { }
		#endregion

		#region NoteID
		[PXNote(BqlField = typeof(SOLine.noteID))]
		public virtual Guid? NoteID { get; set; }
		public abstract class noteID : BqlGuid.Field<noteID> { }
		#endregion
	}
}
