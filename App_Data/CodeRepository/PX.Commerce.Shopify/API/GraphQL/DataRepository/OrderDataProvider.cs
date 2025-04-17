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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;
using PX.Common;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <inheritdoc />
	public class OrderGQLDataProviderFactory : ISPGraphQLDataProviderFactory<OrderGQLDataProvider>
	{
		/// <inheritdoc />
		public OrderGQLDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService)
		{
			return new OrderGQLDataProvider(graphQLAPIService);
		}
	}

	/// <summary>
	/// Performs data operations with companies through Shopify's GraphQL API
	/// </summary>
	public class OrderGQLDataProvider : SPGraphQLDataProvider, IOrderGQLDataProvider
	{
		private const int DefaultDelayTime = 300;
		private const int MaxAttemptApiCall = 5;

		/// <summary>
		/// Creates a new instance of the OrderGraphQLDataProvider that uses the specified GraphQLAPIService.
		/// </summary>
		/// <param name="graphQLAPIClient">The GraphQLAPIService to use to make requests.</param>
		public OrderGQLDataProvider(IGraphQLAPIClient graphQLAPIClient) : base(graphQLAPIClient)
		{
		}


		/// <inheritdoc />
		public virtual async Task<DraftOrderDataGQL> CreateDraftOrderAsync(DraftOrderInput draftOrderInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(DraftOrderCreatePayload.Arguments.Input), draftOrderInput},
				{ typeof(QueryArgument.First<DraftOrderLineItemDataGQL>), draftOrderInput.LineItems.Count > DefaultPageSize ? draftOrderInput.LineItems.Count : DefaultPageSize}
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 4
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(DraftOrderCreatePayload), GraphQLQueryType.Mutation, variables, true, true);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.DraftOrderCreate?.UserErrors);

			return response?.DraftOrderCreate?.DraftOrder;
		}

		/// <inheritdoc />
		public virtual async Task<DraftOrderDataGQL> UpdateDraftOrderAsync(string draftOrderId, DraftOrderInput draftOrderInput, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(DraftOrderUpdatePayload.Arguments.Id), draftOrderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.DraftOrder)},
				{ typeof(DraftOrderUpdatePayload.Arguments.Input), draftOrderInput},
				{ typeof(QueryArgument.First<DraftOrderLineItemDataGQL>), draftOrderInput.LineItems.Count > DefaultPageSize ? draftOrderInput.LineItems.Count : DefaultPageSize}
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 4
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(DraftOrderUpdatePayload), GraphQLQueryType.Mutation, variables, true, true);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.DraftOrderUpdate?.UserErrors);

			return response?.DraftOrderUpdate?.DraftOrder;
		}

		/// <inheritdoc />
		public virtual async Task<string> DeleteDraftOrderAsync(string draftOrderId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(DraftOrderDeletePayload.Arguments.Input), new DraftOrderDeleteInput(){Id = draftOrderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.DraftOrder)} }
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(DraftOrderDeletePayload), GraphQLQueryType.Mutation, variables, false);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.DraftOrderDelete?.UserErrors);

			return response?.DraftOrderDelete?.DeletedId;
		}

		/// <inheritdoc />
		public virtual async Task<DraftOrderDataGQL> CompleteDraftOrderAsync(string draftOrderId, bool? readyToComplete = true, int? lineItemQty = DefaultPageSize, bool paymentPending = false, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(DraftOrderCompletePayload.Arguments.Id), draftOrderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.DraftOrder)},
				{ typeof(DraftOrderCompletePayload.Arguments.PaymentPending), paymentPending },
				{ typeof(QueryArgument.First<OrderLineItemGQL>), lineItemQty ?? DefaultPageSize }
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 4
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(DraftOrderCompletePayload), GraphQLQueryType.Mutation, variables, true, true,
				"DraftOrder.Order", nameof(OrderDataGQL.LineItems));

			if (readyToComplete != true)
				await Task.Delay(DefaultDelayTime);
			int attemptComplete = 1;
			OrderMutation response = null;

			do
			{
				attemptComplete++;
				response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
				//If Shopify returns the draftOrder object and error at the same time, it may not be ready to complete order, need to wait and re-try
				//this code is used for avoiding this error: This order has not finished calculating, please try again later
				if (response?.DraftOrderComplete?.UserErrors?.Length > 0)
				{
					await Task.Delay(DefaultDelayTime);
				}
				else
					return response?.DraftOrderComplete?.DraftOrder;

			} while (attemptComplete < MaxAttemptApiCall);

			CheckIfHaveErrors(response?.DraftOrderComplete?.UserErrors);

			return response?.DraftOrderComplete?.DraftOrder;
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> CloseOrderAsync(string orderId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(OrderClosePayload.Arguments.Input), new OrderCloseInput(){Id = orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)} }
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderClosePayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.OrderClose?.UserErrors);

			return response?.OrderClose?.Order;
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> OpenOrderAsync(string orderId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(OrderOpenPayload.Arguments.Input), new OrderOpenInput(){Id = orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)} }
			};
			var querybuilder = new GraphQLQueryBuilder();
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderOpenPayload), GraphQLQueryType.Mutation, variables, true);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.OrderOpen?.UserErrors);

			return response?.OrderOpen?.Order;
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> UpdateOrderAsync(OrderInput orderInput, int? lineItemQty = DefaultPageSize, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(OrderUpdatePayload.Arguments.Input), orderInput},
				{ typeof(QueryArgument.First<OrderLineItemGQL>), lineItemQty ?? DefaultPageSize}
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3,
				ConnectionPathDepth = 3,
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderUpdatePayload), GraphQLQueryType.Mutation, variables, true, true);

			var response = await MutationAsync<OrderMutation>(queryInfo, cancellationToken);
			CheckIfHaveErrors(response?.OrderUpdate?.UserErrors);

			return response?.OrderUpdate?.Order;
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> GetOrderByIDAsync(string orderId, bool withSubFields = false, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderDataGQL>), orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)}
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Node, variables, withSubFields);

			return await GetSingleAsync<OrderDataGQL, OrderResponse> (queryInfo, cancellationToken);
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> GetOrderWithDetailsAsync(string orderId, bool withLineItems = false, bool withDiscounts = false, bool withMetafields = false, bool withRefundDetails = false, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderDataGQL>), orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)}
			};
			if (withLineItems)
			{
				variables.Add(typeof(QueryArgument.First<OrderLineItemGQL>), DefaultPageSize);
				variables.Add(typeof(QueryArgument.After<OrderLineItemGQL>), null);
			}
			if (withDiscounts)
			{
				variables.Add(typeof(QueryArgument.First<DiscountApplicationGQL>), DefaultPageSize);
				variables.Add(typeof(QueryArgument.After<DiscountApplicationGQL>), null);
			}

			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3,
				ConnectionPathDepth = 4,
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Node, variables, true, withLineItems || withDiscounts, specifiedFieldsOnly);

			var orderResult = await GetSingleAsync<OrderDataGQL, OrderLineItemGQL, OrderResponse>(queryInfo, nameof(OrderDataGQL.LineItems), default);
			if (orderResult == null) return orderResult;

			//The order query is too big if includes all query objects, it may exceed the cost points, so separate to 2 queries.
			if (withMetafields)
			{
				orderResult.MetafieldNodes = (await GetOrderMetafieldsAsync(orderId))?.MetafieldNodes;
			}

			if(withRefundDetails && orderResult.Refunds?.Count() > 0)
			{
				List<OrderRefundGQL> orderRefunds = new List<OrderRefundGQL>();
				foreach (OrderRefundGQL refund in orderResult.Refunds)
				{
					var resulOrderRefund = await GetOrderRefundDetailsAsync(refund.Id);
					orderRefunds.Add(resulOrderRefund);
				}
				orderResult.Refunds = orderRefunds;
			}

			return orderResult;
		}

		/// <inheritdoc />
		public virtual async Task<OrderDataGQL> GetOrderMetafieldsAsync(string orderId, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderDataGQL>), orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)},
				{ typeof(QueryArgument.First<MetafieldGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<MetafieldGQL>), null}
			};

			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(OrderDataGQL.MetafieldNodes));

			return await GetSingleAsync<OrderDataGQL, MetafieldGQL, OrderResponse>(queryInfo, nameof(OrderDataGQL.MetafieldNodes), default);
		}

		/// <inheritdoc />
		public virtual async Task<IEnumerable<OrderTransactionGQL>> GetOrderTransactionsAsync(string orderId, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderDataGQL>), orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)}
			};

			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3,
				AllowRecursiveObject = true,
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Node, variables, true, false, nameof(OrderDataGQL.Transactions));

			var order = await GetSingleAsync<OrderDataGQL, OrderResponse>(queryInfo, default);
			return order.Transactions;
		}

		/// <inheritdoc />
		public virtual async Task<IEnumerable<FulfillmentOrderGQL>> GetOrderFulfillmentOrdersAsync(string orderId)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderDataGQL>), orderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Order)},
				{ typeof(QueryArgument.First<FulfillmentOrderGQL>), 3},
				{ typeof(QueryArgument.After<FulfillmentOrderGQL>), null},
				{ typeof(QueryArgument.First<FulfillmentOrderLineItemGQL>), DefaultPageSize},
				{ typeof(QueryArgument.After<FulfillmentOrderLineItemGQL>), null}
			};

			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3,
				ConnectionPathDepth = 3,
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Node, variables, true, true, nameof(OrderDataGQL.FulfillmentOrders), "FulfillmentOrders.FulfillmentLineItems", "FulfillmentLineItems");

			return (await GetSingleAsync<OrderDataGQL, FulfillmentOrderGQL, OrderResponse>(queryInfo, nameof(OrderDataGQL.FulfillmentOrders), default))?.FulfillmentOrders?.Nodes;
		}

		/// <inheritdoc />
		public virtual async Task<OrderRefundGQL> GetOrderRefundDetailsAsync(string orderRefundId)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<OrderRefundGQL>), orderRefundId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.Refund)},
				{ typeof(QueryArgument.First<OrderRefundLineItemGQL>), 3},
				{ typeof(QueryArgument.After<OrderRefundLineItemGQL>), null},
				{ typeof(QueryArgument.First<OrderTransactionGQL>), 3},
				{ typeof(QueryArgument.After<OrderTransactionGQL>), null}
				//OrderAdjustmentGQL only exists in the unstable API version
				//{ typeof(QueryArgument.First<OrderAdjustmentGQL>), 3},
				//{ typeof(QueryArgument.After<OrderAdjustmentGQL>), null}
			};

			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3,
				ConnectionPathDepth = 3,
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderRefundGQL), GraphQLQueryType.Node, variables, true, true);

			return await GetSingleWithSubConnectionsAsync<OrderRefundGQL, OrderRefundResponse>(queryInfo, default, nameof(OrderRefundGQL.RefundLineItems), nameof(OrderRefundGQL.Transactions));
		}

		/// <inheritdoc />
		public virtual async Task<DraftOrderDataGQL> GetDraftOrderByIDAsync(string draftOrderId, int lineItemNum, CancellationToken cancellationToken = default)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.ID<DraftOrderDataGQL>), draftOrderId.ConvertIdToGid(ShopifyGraphQLConstants.Objects.DraftOrder)},
				{ typeof(QueryArgument.First<DraftOrderLineItemDataGQL>), lineItemNum > DefaultPageSize ? lineItemNum : DefaultPageSize }
			};
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(DraftOrderDataGQL), GraphQLQueryType.Node, variables, true, true);

			return await GetSingleAsync<DraftOrderDataGQL, DraftOrderResponse>(queryInfo, cancellationToken);
		}

		/// <inheritdoc />
		public virtual async Task<IEnumerable<OrderDataGQL>> GetOrdersAsync(string filterString = null, string sortKeyFieldName = OrderSortKeys.UpdatedAt, bool withSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly)
		{
			var variables = new Dictionary<Type, object>
			{
				{ typeof(QueryArgument.First<OrderDataGQL>), withSubFields ? DefaultFetchBulkSizeWithSubfields : DefaultFetchBulkSize},
				{ typeof(QueryArgument.After<OrderDataGQL>), null},
				{ typeof(OrderDataGQL.Arguments.SortKey), sortKeyFieldName}
			};
			if(string.IsNullOrEmpty(filterString) == false)
			{
				variables[typeof(QueryArgument.Query<OrderDataGQL>)] = filterString;
			}
			var querybuilder = new GraphQLQueryBuilder
			{
				ObjectPathDepth = 3
			};
			GraphQLQueryInfo queryInfo = querybuilder.GetQueryResult(typeof(OrderDataGQL), GraphQLQueryType.Connection, variables, withSubFields, false, specifiedFieldsOnly);

			var response = await GetAllAsync<OrderDataGQL, OrdersResponseData, OrdersResponse>(queryInfo, cancellationToken);
			response.ForEach(order => order.Transactions?.ForEach(transaction => transaction.OrderId = order.Id));
			return response;
		}
	}
}
