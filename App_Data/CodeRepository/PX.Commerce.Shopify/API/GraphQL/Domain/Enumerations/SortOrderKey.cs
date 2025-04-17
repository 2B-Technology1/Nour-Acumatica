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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// https://shopify.dev/docs/api/admin-graphql/2023-04/enums/OrderSortKeys
	/// Valid values for sorting orders.
	/// </summary>
	public static class OrderSortKeys
	{
		/// <summary>
		/// Sort by the created_at value.
		/// </summary>
		public const string CreatedAt = "CREATED_AT";

		/// <summary>
		/// Sort by the customer_name value.
		/// </summary>
		public const string CustomerName = "CUSTOMER_NAME";

		/// <summary>
		/// Sort orders by their shipping address country and city.
		/// </summary>
		public const string Destination = "DESTINATION";

		/// <summary>
		/// Sort by the financial_status value.
		/// </summary>
		public const string FinancialStatus = "FINANCIAL_STATUS";

		/// <summary>
		/// Sort by the fulfillment_status value.
		/// </summary>
		public const string FulfillmentStatus = "FULFILLMENT_STATUS";

		/// <summary>
		/// Sort by the id value.
		/// </summary>
		public const string Id = "ID";

		/// <summary>
		/// Sort by the order_number value.
		/// </summary>
		public const string OrderNumber = "ORDER_NUMBER";

		/// <summary>
		/// Sort by the processed_at value.
		/// </summary>
		public const string ProcessedAt = "PROCESSED_AT";

		/// <summary>
		/// Sort by relevance to the search terms when the query parameter is specified on the connection. 
		/// Don't use this sort key when no search query is specified.
		/// </summary>
		public const string Relevance = "RELEVANCE";

		/// <summary>
		/// Sort by the total_price value.
		/// </summary>
		public const string TotalPrice = "TOTAL_PRICE";

		/// <summary>
		/// Sort by the updated_at value.
		/// </summary>
		public const string UpdatedAt = "UPDATED_AT";
	}

}

