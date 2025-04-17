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
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The return type of the priceListFixedPricesUpdate  mutation.
	/// </summary>
	[GraphQLObject(MutationName = "priceListFixedPricesUpdate")]
	public class PriceListFixedPricesUpdatePayload : MutationPayload
	{
		/// <summary>
		/// A list of deleted variant IDs for prices.
		/// </summary>
		[JsonProperty("deletedFixedPriceVariantIds")]
		[GraphQLField("deletedFixedPriceVariantIds", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public List<string> DeletedFixedPriceVariantIds { get; set; }

		/// <summary>
		/// The price list for which the fixed prices were modified.
		/// </summary>
		[JsonProperty("priceList")]
		[GraphQLField("priceList", GraphQLConstants.DataType.Object, typeof(PriceListGQL))]
		public PriceListGQL PriceList { get; set; }

		/// <summary>
		/// The prices that were added to the price list.
		/// </summary>
		[JsonProperty("pricesAdded")]
		[GraphQLField("pricesAdded", GraphQLConstants.DataType.Object, typeof(PriceListPriceGQL))]
		public List<PriceListPriceGQL> PricesAdded { get; set; }

		public class Arguments
		{
			[GraphQLArgument("priceListId", GraphQLConstants.ScalarType.ID, false)]
			public abstract class PriceListId { }

			[GraphQLArgument("pricesToAdd", "[PriceListPriceInput!]", false)]
			public abstract class PricesToAdd { }

			[GraphQLArgument("variantIdsToDelete", "[ID!]", false)]
			public abstract class VariantIdsToDelete { }
		}
	}
}
