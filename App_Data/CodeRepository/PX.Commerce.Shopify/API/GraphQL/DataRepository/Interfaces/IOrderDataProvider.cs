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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Provides functionality for performing CRUD tasks with orders in an external system.
	/// </summary>
	public interface IOrderGQLDataProvider
	{
		/// <summary>
		/// Creates a new draftOrder in the store.
		/// </summary>
		/// <param name="draftOrderInput">Object describing the attributes of the draftOrder input.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created draftOrder.</returns>
		public Task<DraftOrderDataGQL> CreateDraftOrderAsync(DraftOrderInput draftOrderInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a new draftOrder in the store.
		/// </summary>
		/// <param name="draftOrderId">The ID of the draftOrder to update.</param>
		/// <param name="draftOrderInput">Object describing the attributes of the draftOrder input.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created draftOrder.</returns>
		public Task<DraftOrderDataGQL> UpdateDraftOrderAsync(string draftOrderId, DraftOrderInput draftOrderInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete a draftOrder in the store with the specified ID.
		/// </summary>
		/// <param name="draftOrderId">The ID of the draftOrder to delete.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The deleted draftOrderId.</returns>
		public Task<string> DeleteDraftOrderAsync(string draftOrderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Complete a draftOrder to order
		/// </summary>
		/// <param name="draftOrderId">The ID of the draftOrder to complete.</param>
		/// <param name="readyToComplete">Whether the Draft Order is ready and can be completed</param>
		/// <param name="paymentPending">Set the payment status</param>
		/// <param name="lineItemQty">The order line item qty</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The draftOrder with created order.</returns>
		public Task<DraftOrderDataGQL> CompleteDraftOrderAsync(string draftOrderId, bool? readyToComplete = true, int? lineItemQty = 100, bool paymentPending = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Close the order
		/// </summary>
		/// <param name="orderId">The order ID</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The closed order</returns>
		public Task<OrderDataGQL> CloseOrderAsync(string orderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Open the order
		/// </summary>
		/// <param name="orderId">The order ID</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The opened order</returns>
		public Task<OrderDataGQL> OpenOrderAsync(string orderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update the order
		/// </summary>
		/// <param name="orderId">The order ID</param>
		/// <param name="lineItemQty">The order line item qty</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The updated order</returns>
		public Task<OrderDataGQL> UpdateOrderAsync(OrderInput orderInput, int? lineItemQty = 100, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets the Order with the specified ID.
		/// </summary>
		/// <param name="orderId">The ID of the order to retrieve.</param>
		/// <param name="withSubFields"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The retrieved order data.</returns>
		public Task<OrderDataGQL> GetOrderByIDAsync(string orderId, bool withSubFields = false, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets the Order details with the specified ID
		/// </summary>
		/// <param name="orderId">The ID of the order to retrieve.</param>
		/// <param name="withLineItems"></param>
		/// <param name="withDiscounts"></param>
		/// <param name="withMetafields"></param>
		/// <param name="withRefundDetails"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns>The retrieved order data.</returns>
		public Task<OrderDataGQL> GetOrderWithDetailsAsync(string orderId, bool withLineItems = false, bool withDiscounts = false, bool withMetafields = false, bool withRefundDetails = false, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get the metafields of the specified order
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns></returns>
		public Task<OrderDataGQL> GetOrderMetafieldsAsync(string orderId, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get the Transactions of the specified order
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public Task<IEnumerable<OrderTransactionGQL>> GetOrderTransactionsAsync(string orderId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get the FulfillmentOrders of the specified order
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public Task<IEnumerable<FulfillmentOrderGQL>> GetOrderFulfillmentOrdersAsync(string orderId);

		/// <summary>
		/// Get specified order refund details
		/// </summary>
		/// <param name="orderRefundId"></param>
		/// <returns></returns>
		public Task<OrderRefundGQL> GetOrderRefundDetailsAsync(string orderRefundId);

		/// <summary>
		/// Gets the draft order with the specified ID.
		/// </summary>
		/// <param name="draftOrderId">The ID of the order to retrieve.</param>
		/// <param name="lineItemNum">The line items should be fetched with draft order</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The retrieved draft order data.</returns>
		public Task<DraftOrderDataGQL> GetDraftOrderByIDAsync(string draftOrderId, int lineItemNum, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get orders with or without filter
		/// </summary>
		/// <param name="filterString">filter string</param>
		/// <param name="sortKeyFieldName">The sort order key</param>
		/// <param name="withSubFields">Whether include sub objects</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns>The retrieved order data</returns>
		public Task<IEnumerable<OrderDataGQL>> GetOrdersAsync(string filterString = null, string sortKeyFieldName = OrderSortKeys.UpdatedAt, bool withSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);
	}
}
