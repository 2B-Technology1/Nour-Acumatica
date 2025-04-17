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
	/// The return type of the customerUpdateDefaultAddress mutation.
	/// </summary>
	[GraphQLObject(MutationName = "customerUpdateDefaultAddress")]
	public class CustomerUpdateDefaultAddressPayload : MutationPayload
	{
		/// <summary>
		/// The updated customer.
		/// </summary>
		[JsonProperty("customer")]
		[GraphQLField("customer ", GraphQLConstants.DataType.Object, typeof(CustomerDataGQL))]
		public CustomerDataGQL Customer { get; set; }

		public class Arguments
		{
			[GraphQLArgument("addressId", GraphQLConstants.ScalarType.ID, false)]
			public abstract class AddressId { }

			[GraphQLArgument("customerId", GraphQLConstants.ScalarType.ID, false)]
			public abstract class CustomerId { }
		}
	}
}
