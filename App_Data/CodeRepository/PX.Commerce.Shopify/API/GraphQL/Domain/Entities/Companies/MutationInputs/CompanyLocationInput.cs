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
	/// The company location fields to use when creating or updating a company location.
	/// </summary>
	public class CompanyLocationInput
	{
		/// <summary>
		/// The input fields to create or update the billing address for a company location.
		/// </summary>
		[JsonProperty("billingAddress", NullValueHandling = NullValueHandling.Ignore)]
		public CompanyAddressInput BillingAddress { get; set; }

		/// <summary>
		/// Whether the billing address is the same as the shipping address.
		/// If the value is true, then the input for billingAddress is ignored.
		/// </summary>
		[JsonProperty("billingSameAsShipping", NullValueHandling = NullValueHandling.Ignore)]
		public bool BillingSameAsShipping { get; set; }

		/// <summary>
		/// A unique externally-supplied identifier for the company location.
		/// </summary>
		[JsonProperty("externalId")]
		public string ExternalID { get; set; }

		/// <summary>
		/// The name of the company location.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The note of the company location.
		/// </summary>
		[JsonProperty("note")]
		public string Note { get; set; }

		/// <summary>
		/// The phone number of the company location.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The input fields to create or update the shipping address for a company location.
		/// </summary>
		[JsonProperty("shippingAddress", NullValueHandling = NullValueHandling.Ignore)]
		public CompanyAddressInput ShippingAddress { get; set; }

		/// <summary>
		/// The tax registration ID of the company location.
		/// </summary>
		[JsonProperty("taxRegistrationId", NullValueHandling = NullValueHandling.Ignore)]
		public string TaxRegistrationId { get; set; }

	}
}
