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
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The input fields used to create or update an order.
	/// </summary>
	public class OrderInput
	{
		/// <summary>
		/// A new list of custom attributes for the order. Overwrites the existing custom attributes.
		/// </summary>
		[JsonProperty("customAttributes", NullValueHandling = NullValueHandling.Ignore)]
		public List<AttributeInput> CustomAttributes { get; set; }

		/// <summary>
		/// A new customer email address for the order. Overwrites the existing email address.
		/// </summary>
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
		public string Email { get; set; }

		/// <summary>
		/// The ID of the order to update.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public string ID { get; set; }

		/// <summary>
		/// Metafields attached to the order.
		/// </summary>
		[JsonProperty("metafields", NullValueHandling = NullValueHandling.Ignore)]
		public List<MetafieldInput> Metafields { get; set; }

		/// <summary>
		/// The new contents for the note associated with the order. Overwrites the existing note.
		/// </summary>
		[JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
		public string Note { get; set; }

		/// <summary>
		/// The new shipping address for the order. Overwrites the existing shipping address.
		/// </summary>
		[JsonProperty("shippingAddress", NullValueHandling = NullValueHandling.Ignore)]
		public MailingAddressInput ShippingAddress { get; set; }

		/// <summary>
		/// A new list of tags for the order. Overwrites the existing tags.
		/// </summary>
		[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Tags { get; set; }

		[JsonIgnore]
		public Guid? LocalID { get; set; }
	}
}
