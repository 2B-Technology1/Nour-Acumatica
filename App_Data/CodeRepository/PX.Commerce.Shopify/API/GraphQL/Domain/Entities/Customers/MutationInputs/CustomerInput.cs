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
	/// Provides the fields and values to use when creating or updating a customer.
	/// </summary>
	public class CustomerInput
	{
		/// <summary>
		/// The addresses for a customer.
		/// </summary>
		[JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
		public List<MailingAddressInput> Addresses { get; set; }

		/// <summary>
		/// The unique email address of the customer.
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		/// <summary>
		/// The first name of the customer.
		/// </summary>
		[JsonProperty("firstName", NullValueHandling = NullValueHandling.Ignore)]
		public string FirstName { get; set; }

		///<inheritdoc/>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		/// <summary>
		/// The last name of the customer.
		/// </summary>
		[JsonProperty("lastName", NullValueHandling = NullValueHandling.Ignore)]
		public string LastName { get; set; }

		/// <summary>
		/// The customer's locale.
		/// </summary>
		[JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
		public string Locale { get; set; }

		/// <summary>
		/// The note of the customer.
		/// </summary>
		[JsonProperty("note")]
		public string Note { get; set; }

		/// <summary>
		/// The phone number.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// Additional metafields to associate to the customer.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		public List<MetafieldInput> Metafields { get; set; }

		/// <summary>
		/// A new list of tags for the customer. Overwrites the existing tags.
		/// </summary>
		[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Tags { get; set; }

		/// <summary>
		/// Whether the customer is exempt from paying taxes on their order.
		/// </summary>
		[JsonProperty("taxExempt", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TaxExempt { get; set; }

	}
}
