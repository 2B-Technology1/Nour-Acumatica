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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.Common.Bql;
using PX.Objects.CR;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	using static SOOrderStatus;

	/// <exclude/>
	[PXCacheName(Messages.SalesAllocationsFilter)]
	public class SalesAllocationsFilter: IBqlTable
	{
		#region Action
		[action.List]
		[PXDBString(7)]
		[PXUIField(DisplayName = "Action", Required = true)]
		[PXDefault(action.None)]
		public virtual string Action { get; set; }
		public abstract class action : BqlString.Field<action>
		{
			public const string None = "None";
			public const string AllocateSalesOrders = "Alloc";
			public const string DeallocateSalesOrders = "Dealloc";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					(None, IN.Turnover.Messages.NonePlaceholder),
					(AllocateSalesOrders, Messages.AllocateSalesOrders),
					(DeallocateSalesOrders, Messages.DeallocateSalesOrders))
				{ }
			}

			public class allocateSalesOrders : BqlString.Constant<allocateSalesOrders> { public allocateSalesOrders() : base(AllocateSalesOrders) { } }
			public class deallocateSalesOrders : BqlString.Constant<deallocateSalesOrders> { public deallocateSalesOrders() : base(DeallocateSalesOrders) { } }
		}
		#endregion

		#region SelectBy
		[selectBy.List]
		[PXDBString(11)]
		[PXUIField(DisplayName = "Select By", Required = true)]
		[PXDefault(selectBy.LineShipOn)]
		public virtual string SelectBy { get; set; }
		public abstract class selectBy : BqlString.Field<selectBy>
		{
			public const string OrderDate = "OrderDate";
			public const string CancelBy = "CancelBy";
			public const string LineRequestedOn = "RequestedOn";
			public const string LineShipOn = "ShipOn";

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute() : base(
					(OrderDate, Messages.OrderDate),
					(LineRequestedOn, Messages.LineRequestedOn),
					(LineShipOn, Messages.LineShipOn),
					(CancelBy, Messages.CancelBy))
				{ }
			}

			public class orderDate : BqlString.Constant<orderDate> { public orderDate() : base(OrderDate) { } }
			public class cancelBy : BqlString.Constant<cancelBy> { public cancelBy() : base(CancelBy) { } }
			public class lineRequestedOn : BqlString.Constant<lineRequestedOn> { public lineRequestedOn() : base(LineRequestedOn) { } }
			public class lineShipOn : BqlString.Constant<lineShipOn> { public lineShipOn() : base(LineShipOn) { } }
		}
		#endregion

		#region StartDate
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		public abstract class startDate : BqlDateTime.Field<startDate> { }
		#endregion

		#region EndDate
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "End Date")]
		public virtual DateTime? EndDate { get; set; }
		public abstract class endDate : BqlDateTime.Field<endDate> { }
		#endregion

		#region SiteID
		[Site(Required = true)]
		public virtual int? SiteID { get; set; }
		public abstract class siteID : BqlInt.Field<siteID> { }
		#endregion

		#region OrderType
		[PXDBString]
		[PXStringList(MultiSelect = true)]
		[PXUIField(DisplayName = "Order Type")]
		public virtual string OrderType { get; set; }
		public abstract class orderType : BqlString.Field<orderType>
		{
			public class behaviorList : SetOf.Strings.FilledWith<
				SOBehavior.bL,
				SOBehavior.sO,
				SOBehavior.tR,
				SOBehavior.rM>
			{ }
		}
		#endregion

		#region OrderStatus
		[PXDBString]
		[orderStatus.ListAttribute.WithExpired]
		[PXUIField(DisplayName = "Order Status")]
		public virtual string OrderStatus { get; set; }
		public abstract class orderStatus : BqlString.Field<orderStatus>
		{
			public class ListAttribute : PXStringListAttribute
			{
				private static (string, string)[] BaseValues = new (string, string)[]
				{
					(Hold, Messages.Hold),
					(CreditHold, Messages.CreditHold),
					(AwaitingPayment, Messages.AwaitingPayment),
					(PendingProcessing, Messages.PendingProcessing),
					(Open, Messages.Open),
					(BackOrder, Messages.BackOrder)
				};

				public ListAttribute() : base(BaseValues)
				{
					MultiSelect = true;
				}

				protected ListAttribute(params (string, string)[] additionalValues)
					: base(BaseValues.Concat(additionalValues).ToArray())
				{
					MultiSelect = true;
				}

				public class WithExpiredAttribute: ListAttribute
				{
					public WithExpiredAttribute() : base((Expired, Messages.Expired))
					{ }
				}
			}

			public class list : SetOf.Strings.FilledWith<
				hold,
				creditHold,
				awaitingPayment,
				pendingProcessing,
				open,
				backOrder>
			{
				public class withExpired : SetOf.Strings.FilledWith<
					hold,
					creditHold,
					awaitingPayment,
					pendingProcessing,
					open,
					backOrder,
					expired>
				{ }
			}
		}
		#endregion

		#region Priority
		[PXDBShort]
		[PXUIField(DisplayName = "Order Priority")]
		public virtual short? Priority { get; set; }
		public abstract class priority : BqlShort.Field<priority> { }
		#endregion

		#region OrderNbr
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXSelector(typeof(SelectFrom<SOOrder>
			.InnerJoin<SOOrderType>
				.On<SOOrderType.orderType.IsEqual<SOOrder.orderType>
				.And<SOOrderType.behavior.IsInSequence<orderType.behaviorList>>
				.And<SOOrderType.active.IsEqual<True>>>
			.Where<SOOrder.status.IsInSequence<AllowedValues<orderStatus>>
				.And<priority.FromCurrent.IsNull.Or<SOOrder.priority.IsEqual<priority.FromCurrent>>>
				.And<orderType.FromCurrent.IsNull.Or<SOOrder.orderType.IsInSequence<CurrentSelectedValues<orderType>>>>
				.And<orderStatus.FromCurrent.IsNull.Or<SOOrder.status.IsInSequence<CurrentSelectedValues<orderStatus>>>>
				.And<customerID.FromCurrent.IsNull.Or<SOOrder.customerID.IsEqual<customerID.FromCurrent>>>
				.And<Exists<SelectFrom<SOOrderTypeOperation>//because of error when inner join SOOrderTypeOperation
					.Where<SOOrderTypeOperation.orderType.IsEqual<SOOrderType.orderType>
						.And<SOOrderTypeOperation.operation.IsEqual<SOOperation.issue>>
						.And<SOOrderTypeOperation.active.IsEqual<True>>>>>>
			.SearchFor<SOOrder.orderNbr>))]
		[PXFormula(typeof(Default<action>))]
		[PXFormula(typeof(Default<priority, orderType, orderStatus, customerID>))]
		public virtual string OrderNbr { get; set; }
		public abstract class orderNbr : BqlString.Field<orderNbr> { }
		#endregion

		#region CustomerClassID
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(SelectFrom<CustomerClass>
			.SearchFor<CustomerClass.customerClassID>),
			DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class")]
		public virtual string CustomerClassID { get; set; }
		public abstract class customerClassID : BqlString.Field<customerClassID> { }
		#endregion

		#region CustomerID		
		[CustomerActive(
			typeof(Search<BAccountR.bAccountID,
				Where<customerClassID.FromCurrent.IsNull
					.Or<Customer.customerClassID.IsEqual<customerClassID.FromCurrent>>>>))]
		[PXFormula(typeof(Default<customerClassID>))]
		public virtual int? CustomerID { get; set; }
		public abstract class customerID : BqlInt.Field<customerID> { }
		#endregion

		#region InventoryID
		[StockItem]
		[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>),
			IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		public virtual int? InventoryID { get; set; }
		public abstract class inventoryID : BqlInt.Field<inventoryID> { }
		#endregion
	}
}
