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


using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Contains mutation for draftOrder, order
	/// </summary>
	public class OrderMutation
	{
		/// <summary>
		/// The payload for the DraftOrderCreate mutation.
		/// </summary>
		[JsonProperty("draftOrderCreate")]
		public DraftOrderCreatePayload DraftOrderCreate { get; set; }

		/// <summary>
		/// The payload for the DraftOrderUpdate mutation.
		/// </summary>
		[JsonProperty("draftOrderUpdate")]
		public DraftOrderUpdatePayload DraftOrderUpdate { get; set; }

		/// <summary>
		/// The payload for the draftOrderDelete mutation.
		/// </summary>
		[JsonProperty("draftOrderDelete")]
		public DraftOrderDeletePayload DraftOrderDelete { get; set; }

		/// <summary>
		/// The payload for the draftOrderComplete mutation.
		/// </summary>
		[JsonProperty("draftOrderComplete")]
		public DraftOrderCompletePayload DraftOrderComplete { get; set; }

		/// <summary>
		/// The payload for the OrderClose mutation.
		/// </summary>
		[JsonProperty("orderClose")]
		public OrderClosePayload OrderClose { get; set; }

		/// <summary>
		/// The payload for the orderOpen mutation.
		/// </summary>
		[JsonProperty("orderOpen")]
		public OrderOpenPayload OrderOpen { get; set; }

		/// <summary>
		/// The payload for the orderUpdate mutation.
		/// </summary>
		[JsonProperty("orderUpdate")]
		public OrderUpdatePayload OrderUpdate { get; set; }
	}
}
