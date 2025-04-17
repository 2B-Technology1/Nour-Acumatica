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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PX.Commerce.Shopify.API.REST
{
	public class OrderRestDataProviderFactory : ISPRestDataProviderFactory<IOrderRestDataProvider>
	{
		public virtual IOrderRestDataProvider CreateInstance(IShopifyRestClient restClient) => new OrderRestDataProvider(restClient);
	}	

	public class OrderRestDataProvider : RestDataProviderBase, IParentRestDataProvider<OrderData>, IOrderRestDataProvider
	{
		protected override string GetListUrl { get; } = "orders.json";
		protected override string GetSingleUrl { get; } = "orders/{id}.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetMetafieldsUrl { get; } = "orders/{id}/metafields.json";
		private string GetCloseOrderUrl { get; } = "orders/{id}/close.json";
		private string GetCancelOrderUrl { get; } = "orders/{id}/cancel.json";
		private string GetReOpenOrderUrl { get; } = "orders/{id}/open.json";
		private string GetTransactionsUrl { get; } = "orders/{id}/transactions.json";
		private string GetSingleTransactionUrl { get; } = "orders/{parent_id}/transactions/{id}.json";
		private string GetCustomerUrl { get; } = "customers/{id}.json";
		private string GetOrderRiskUrl { get; } = "orders/{id}/risks.json";
		private string GetOrderRefundsurl { get; } = "orders/{id}/refunds.json";
		private string GetSingleRefundsUrl { get; } = "orders/{parent_id}/refunds/{id}.json";

		public OrderRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual OrderData Create(OrderData entity)
		{
			return base.Create<OrderData, OrderResponse>(entity);
		}

		public virtual OrderData Update(OrderData entity) => Update(entity, entity.Id.ToString());
		public virtual OrderData Update(OrderData entity, string id)
		{
			var segments = MakeUrlSegments(id);
			return base.Update<OrderData, OrderResponse>(entity, segments);
		}

		public virtual bool Delete(string id)
		{
			var segments = MakeUrlSegments(id);
			return Delete(segments);
		}

		public virtual IEnumerable<OrderData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			return GetCurrentList<OrderData, OrdersResponse>(out previousList, out nextList, filter);
		}

		public virtual IEnumerable<OrderData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll<OrderData, OrdersResponse>(filter, cancellationToken: cancellationToken);
		}

		public virtual OrderData GetByID(string id) => GetByID(id, false, false, false, false);

		public virtual OrderData GetByID(string id, bool includedMetafields = false, bool includedTransactions = false, bool includedCustomer = true, bool includedOrderRisk = false, CancellationToken cancellationToken = default)
		{
			var segments = MakeUrlSegments(id);
			var entity = base.GetByID<OrderData, OrderResponse>(segments);
			if (entity != null)
			{
				if (includedTransactions == true)
					entity.Transactions = GetOrderTransactions(id, cancellationToken);
				if (includedCustomer == true && entity.Customer != null && entity.Customer.Id > 0)
				{
					entity.Customer = GetOrderCustomer(entity.Customer.Id.ToString());
				}
				if (includedMetafields == true)
					entity.Metafields = GetMetafieldsById(id, cancellationToken);
				if (includedOrderRisk == true)
					entity.OrderRisks = GetOrderRisks(id, cancellationToken);
			}
			return entity;
		}

		public virtual List<MetafieldData> GetMetafieldsById(string id, CancellationToken cancellationToken = default)
		{
			var request = BuildRequest(GetMetafieldsUrl, nameof(GetMetafieldsById), MakeUrlSegments(id), null);
			return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request, cancellationToken).ToList();
		}

		public virtual OrderData CloseOrder(string orderId)
		{
			var request = BuildRequest(GetCloseOrderUrl, nameof(CloseOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public virtual OrderData ReopenOrder(string orderId)
		{
			var request = BuildRequest(GetReOpenOrderUrl, nameof(ReopenOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public virtual OrderData CancelOrder(string orderId)
		{
			var request = BuildRequest(GetCancelOrderUrl, nameof(CancelOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public virtual List<OrderTransaction> GetOrderTransactions(string orderId, CancellationToken cancellationToken = default)
		{
			var request = BuildRequest(GetTransactionsUrl, nameof(GetOrderTransactions), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.GetAll<OrderTransaction, OrderTransactionsResponse>(request, cancellationToken).ToList();
		}

		public virtual OrderTransaction GetOrderSingleTransaction(string orderId, string transactionId)
		{
			var request = BuildRequest(GetSingleTransactionUrl, nameof(GetOrderSingleTransaction), MakeUrlSegments(transactionId, orderId), null);
			return ShopifyRestClient.Get<OrderTransaction, OrderTransactionResponse>(request);
		}

		public virtual CustomerData GetOrderCustomer(string orderId)
		{
			var request = BuildRequest(GetCustomerUrl, nameof(GetOrderCustomer), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Get<CustomerData, CustomerResponse>(request);
		}

		public virtual List<OrderRisk> GetOrderRisks(string orderId, CancellationToken cancellationToken = default)
		{
			var request = BuildRequest(GetOrderRiskUrl, nameof(GetOrderRisks), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.GetAll<OrderRisk, OrderRisksResponse>(request, cancellationToken).ToList();
		}

		public virtual OrderTransaction PostPaymentToCapture(OrderTransaction entity, string orderId)
		{
			var request = BuildRequest(GetTransactionsUrl, nameof(PostPaymentToCapture), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderTransaction, OrderTransactionResponse>(request, entity);
		}
	}
}
