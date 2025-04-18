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
using System.Collections.Generic;
using System.ComponentModel;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldResponse : IEntityResponse<MetafieldData>
	{
		[JsonProperty("metafield")]
		public MetafieldData Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class MetafieldsResponse : IEntitiesResponse<MetafieldData>
	{
		[JsonProperty("metafields")]
		public IEnumerable<MetafieldData> Data { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[Description("Metafield")]
	public class MetafieldData
    {
		/// <summary>
		/// The date and time (ISO 8601 format) when the metafield was created.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
		public virtual string DateCreatedAt { get; set; }

		/// <summary>
		/// The date and time (ISO 8601 format) when the metafield was last updated.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public virtual string DateModifiedAt { get; set; }

		/// <summary>
		/// The unique ID of the metafield.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public virtual long? Id { get; set; }

		/// <summary>
		/// A description of the information that the metafield contains.
		/// </summary>
		[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.MetaDescription)]
		public virtual string Description { get; set; }

		/// <summary>
		/// The name of the metafield. Maximum length: 30 characters.
		/// </summary>
		[JsonProperty("key")]
		[Description(ShopifyCaptions.MetaKeywords)]
		public virtual string Key { get; set; }

		/// <summary>
		/// A container for a set of metafields. You need to define a custom namespace for your metafields to distinguish them from the metafields used by other apps. Maximum length: 20 characters.
		/// </summary>
		[JsonProperty("namespace", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.MetaNamespace)]
		public virtual string Namespace { get; set; }

		/// <summary>
		/// The unique ID of the resource that the metafield is attached to.
		/// </summary>
		[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
		public virtual long? OwnerId { get; set; }

		/// <summary>
		/// The type of resource that the metafield is attached to.
		/// </summary>
		[JsonProperty("owner_resource", NullValueHandling = NullValueHandling.Ignore)]
		public virtual string OwnerResource { get; set; }

		/// <summary>
		/// The information to be stored as metadata.
		/// </summary>
		[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.Value)]
		public virtual string Value { get; set; }

		/// <summary>
		/// The metafield's information type.
		/// </summary>
		[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
		public virtual string Type { get; set; }
	}
}
