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
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// A collection of monetary values in their respective currencies.
	/// Typically used in the context of multi-currency pricing and transactions, when an amount in the shop's currency is converted to the customer's currency of choice (the presentment currency).
	/// </summary>
	[CommerceDescription(ShopifyCaptions.PriceSet)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MoneyBag
	{
		/// <summary>
		/// Amount in presentment currency.
		/// </summary>
		[JsonProperty("presentmentMoney")]
		[GraphQLField("presentmentMoney", GraphQLConstants.DataType.Object, typeof(Money))]
		[CommerceDescription(ShopifyCaptions.PresentmentMoney, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public Money PresentmentMoney { get; set; }

		/// <summary>
		/// Amount in shop currency.
		/// </summary>
		[JsonProperty("shopMoney")]
		[GraphQLField("shopMoney", GraphQLConstants.DataType.Object, typeof(Money))]
		[CommerceDescription(ShopifyCaptions.ShopMoney, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public Money ShopMoney { get; set; }
	}
}
