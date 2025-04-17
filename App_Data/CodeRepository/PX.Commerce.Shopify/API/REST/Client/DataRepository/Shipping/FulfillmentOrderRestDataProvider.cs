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

namespace PX.Commerce.Shopify.API.REST
{
	public class FulfillmentOrderRestDataProviderFactory : ISPRestDataProviderFactory<IFulfillmentOrderRestDataProvider>
	{
		public virtual IFulfillmentOrderRestDataProvider CreateInstance(IShopifyRestClient restClient) => new FulfillmentOrderRestDataProvider(restClient);
	}
	public class FulfillmentOrderRestDataProvider : RestDataProviderBase, IChildRestDataProvider<FulfillmentOrder>, IFulfillmentOrderRestDataProvider
	{		
		protected override string GetListUrl => "orders/{id}/fulfillment_orders.json";
		protected override string GetSingleUrl => "fulfillment_orders/{id}.json";
		protected override string GetSearchUrl => throw new NotImplementedException();		
		private string GetReleaseOpenUrl { get; } = "fulfillment_orders/{id}/release_hold.json";
		private string GetOpenUrl { get; } = "fulfillment_orders/{id}/open.json";
		private string GetCloseUrl { get; } = "fulfillment_orders/{id}/close.json";
		private string GetHoldUrl { get; } = "fulfillment_orders/{id}/hold.json";
		private string GetMoveUrl { get; } = "fulfillment_orders/{id}/move.json";
		private string GetCancelUrl { get; } = "fulfillment_orders/{id}/cancel.json";

		public FulfillmentOrderRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual int Count(string parentId) => throw new NotImplementedException();

		public virtual FulfillmentOrder Create(FulfillmentOrder entity, string parentId) => throw new NotImplementedException();

		public virtual bool Delete(string parentId, string id) => throw new NotImplementedException();

		/// <summary>
		/// Retrieves a list of fulfillment orders for a specific order by order_id.
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public virtual IEnumerable<FulfillmentOrder> GetAll(string orderId, IFilter filter = null, CancellationToken cancellationToken = default)
		{
			var segments = MakeUrlSegments(orderId);
			return GetAll<FulfillmentOrder, FulfillmentOrdersResponse>(filter, segments, cancellationToken);
		}

		public virtual IEnumerable<FulfillmentOrder> GetAllWithoutParent(IFilter filter = null) => throw new NotImplementedException();

		/// <summary>
		/// Retrieves a specific fulfillment order.
		/// </summary>
		/// <param name="fulfillment_order_id"></param>
		/// <returns></returns>
		public virtual FulfillmentOrder GetByID(string fulfillmentOrderId, string parentID = null)
		{
			var segments = MakeUrlSegments(fulfillmentOrderId);
			return GetByID<FulfillmentOrder, FulfillmentOrderResponse>(segments);
		}

		public virtual IEnumerable<FulfillmentOrder> GetCurrentList(string orderID, out string previousList, out string nextList, IFilter filter = null)
		{
			var segments = MakeParentUrlSegments(orderID);
			return GetCurrentList<FulfillmentOrder, FulfillmentOrdersResponse>(out previousList, out nextList, filter, segments);
		}

		public virtual FulfillmentOrder Update(FulfillmentOrder entity, string parentId, string id) => throw new NotImplementedException();

		/// <summary>
		/// Releases the fulfillment hold on a fulfillment order and changes the status of the fulfillment order to OPEN or SCHEDULED.
		/// </summary>
		/// <param name="fulfillmentOrderId"></param>
		/// <returns></returns>
		public virtual FulfillmentOrder ReleaseOpenFulfillment(string fulfillmentOrderId)
		{
			var request = BuildRequest(GetReleaseOpenUrl, nameof(ReleaseOpenFulfillment), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, null, false);
		}

		/// <summary>
		/// Marks a scheduled fulfillment order as ready for fulfillment.
		/// This method allows merchants to work on a scheduled fulfillment order before its expected <i>fulfill_at</i> datetime.
		/// </summary>
		/// <param name="fulfillmentOrderId"></param>
		/// <returns></returns>
		public virtual FulfillmentOrder OpenFulfillmentOrder(string fulfillmentOrderId)
		{
			var request = BuildRequest(GetOpenUrl, nameof(OpenFulfillmentOrder), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, new FulfillmentOrder() { }, false);
		}

		public virtual FulfillmentOrder CancelFulfillmentOrder(string fulfillmentOrderId)
		{
			var request = BuildRequest(GetCancelUrl, nameof(CancelFulfillmentOrder), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, new FulfillmentOrder() { }, false);
		}

		/// <summary>
		/// Halts all fulfillment work on a fulfillment order with status OPEN and changes the status of the fulfillment order to ON_HOLD.
		/// </summary>
		/// <param name="fulfillmentOrderId"></param>
		/// <returns></returns>
		public virtual FulfillmentOrder HoldFulfillmentOrder(FulfillmentOrder fulfillmentOrder, string fulfillmentOrderId)
		{
			var request = BuildRequest(GetHoldUrl, nameof(HoldFulfillmentOrder), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, fulfillmentOrder, true);
		}

		/// <summary>
		/// Moves a fulfillment order from one merchant managed location to another merchant managed location.
		/// </summary>
		/// <param name="LocationId">The location to which the fulfillment order will be moved.</param>
		/// <param name="fulfillmentOrderId"></param>
		/// <returns></returns>
		public virtual FulfillmentOrder MoveFulfillmentOrder(long? locationId, string fulfillmentOrderId)
		{
			var request = BuildRequest(GetMoveUrl, nameof(MoveFulfillmentOrder), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, new FulfillmentOrder() { NewLocationId = locationId }, true);
		}

		/// <summary>
		/// Marks an in progress fulfillment order as incomplete, indicating the fulfillment service is unable to ship any remaining items and intends to close the fulfillment order.
		/// </summary>
		/// <param name="fulfillmentOrderId"></param>
		/// <param name="message">An optional reason for marking the fulfillment order as incomplete.</param>
		/// <returns></returns>
		public virtual FulfillmentOrder CloseFulfillmentOrder(string fulfillmentOrderId, string message = null)
		{
			var request = BuildRequest(GetCloseUrl, nameof(CloseFulfillmentOrder), MakeUrlSegments(fulfillmentOrderId), null);
			return ShopifyRestClient.Post<FulfillmentOrder, FulfillmentOrderResponse>(request, new FulfillmentOrder() { Message = message }, true);
		}
	}
}
