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

using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Common;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public abstract class BCOrderBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : OrderProcessorBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected InventoryItem refundItem;
		protected Lazy<InventoryItem> giftCertificateItem;
		protected BCRestClient client;
		protected IOrderRestDataProvider orderDataProvider;
		protected IChildReadOnlyRestDataProvider<OrderRefund> orderRefundsRestDataProvider;
		protected IChildRestDataProvider<OrdersProductData> orderProductsRestDataProvider;
		protected IChildReadOnlyRestDataProvider<OrdersTaxData> orderTaxesRestDataProvider;
		protected IChildReadOnlyRestDataProvider<OrdersTransactionData> orderTransactionsRestDataProvider;
		protected IChildRestDataProvider<OrdersCouponData> orderCouponsRestDataProvider;

		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IOrderRestDataProvider> orderDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrderRefund>> orderRefundsRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<OrdersProductData>> orderProductsRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<OrdersTaxData>> orderTaxesRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrdersTransactionData>> orderTransactionsRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IChildRestDataProvider<OrdersCouponData>> orderCouponsRestDataProviderFactory { get; set; }
		#endregion

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			orderDataProvider = orderDataProviderFactory.CreateInstance(client);
			orderCouponsRestDataProvider = orderCouponsRestDataProviderFactory.CreateInstance(client);
			orderProductsRestDataProvider = orderProductsRestDataProviderFactory.CreateInstance(client);
			orderRefundsRestDataProvider = orderRefundsRestDataProviderFactory.CreateInstance(client);
			orderTaxesRestDataProvider = orderTaxesRestDataProviderFactory.CreateInstance(client);
			orderTransactionsRestDataProvider = orderTransactionsRestDataProviderFactory.CreateInstance(client);

			var bindingExt = GetBindingExt<BCBindingExt>();
			refundItem = bindingExt?.RefundAmountItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)this, bindingExt?.RefundAmountItemID) : throw new PXException(BigCommerceMessages.NoRefundItem);
			giftCertificateItem = new Lazy<InventoryItem>(delegate { return InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID); }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public virtual SalesOrderDetail InsertRefundAmountItem(decimal amount, StringValue branch)
		{
			decimal quantity = amount < 0m ? -1m : 1m;

			SalesOrderDetail detail = new SalesOrderDetail();
			detail.InventoryID = refundItem.InventoryCD?.TrimEnd().ValueField();
			detail.OrderQty = quantity.ValueField();
			detail.UOM = refundItem.BaseUnit.ValueField();
			detail.Branch = branch;
			//Unit price is not allowed to be negative. We take the absolute value and use -1 for quantity instead.
			detail.UnitPrice = Math.Abs(amount).ValueField();
			detail.ManualPrice = true.ValueField();
			detail.ReasonCode = GetBindingExt<BCBindingExt>()?.ReasonCode?.ValueField();
			return detail;

		}

		public virtual decimal CalculateTaxableRefundAmount(OrdersProductData originalOrderProduct, IEnumerable<RefundedItem> shippingRefundItems, int quantity, string lineItemType)
		{
			var discountPerItem = (originalOrderProduct?.AppliedDiscounts?.Sum(x => x.DiscountAmount) ?? 0) > 0 ? originalOrderProduct?.AppliedDiscounts?.Sum(x => x.DiscountAmount) / originalOrderProduct?.Quantity : 0;
			decimal taxableAmountForRefundItem = (((originalOrderProduct?.PriceExcludingTax ?? 0) - discountPerItem) * quantity) ?? 0;
			decimal excluderefundAmount = taxableAmountForRefundItem;
			if (lineItemType.Equals(BCConstants.Shipping, StringComparison.InvariantCultureIgnoreCase))
			{
				excluderefundAmount += shippingRefundItems?.Sum(x => x.RequestedAmount ?? 0) ?? 0;
			}

			return excluderefundAmount;
		}

		public virtual string GetInventoryCDForGiftWrap(out string uom)
		{
			var currentBindingExt = GetBindingExt<BCBindingExt>();
			InventoryItem inventory = currentBindingExt?.GiftWrappingItemID != null ? PX.Objects.IN.InventoryItem.PK.Find(this, currentBindingExt?.GiftWrappingItemID) : null;
			if (inventory?.InventoryCD == null)
				throw new PXException(BigCommerceMessages.NoGiftWrapItem);
			uom = inventory?.BaseUnit?.Trim();
			return inventory?.InventoryCD?.Trim();
		}
	}
}
