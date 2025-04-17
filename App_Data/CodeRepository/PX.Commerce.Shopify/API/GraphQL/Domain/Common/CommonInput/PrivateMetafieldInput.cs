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
	/// The input fields for a private metafield.
	/// </summary>
	public class PrivateMetafieldInput
	{
		/// <summary>
		/// The resource that owns the metafield. If the field is blank, then the Shop resource owns the metafield.
		/// </summary>
		[JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
		public string Owner { get; set; }

		/// <summary>
		/// The key of the private metafield.
		/// </summary>
		[JsonProperty("key")]
		public string Key { get; set; }

		/// <summary>
		/// The namespace of the private metafield.
		/// </summary>
		[JsonProperty("namespace")]
		public string Namespace { get; set; }

		/// <summary>
		/// The value of a metafield.
		/// </summary>
		[JsonProperty("valueInput")]
		public PrivateMetafieldValueInput ValueInput { get; set; }
	}

	/// <summary>
	/// The value input contains the value and value type of the private metafield.
	/// </summary>
	public class PrivateMetafieldValueInput
	{
		/// <summary>
		/// The value of a private metafield.
		/// </summary>
		[JsonProperty("value")]
		public string Value { get; set; }

		/// <summary>
		/// Represents the private metafield value type.
		/// </summary>
		[JsonProperty("valueType")]
		public PrivateMetafieldValueType ValueType { get; set; }
	}
}
