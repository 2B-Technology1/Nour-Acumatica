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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[JsonObject(Description = "Order Item Location")]
	[Description(ShopifyCaptions.OrderItemLocation)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class OrderItemLocation : BCAPIEntity
	{
		/// <summary>
		/// The first line of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("address1")]
		public string Address1 { get; set; }

		/// <summary>
		/// The second line of the address.
		/// </summary>
		[PIIData]
		[JsonProperty("address2")]
		public string Address2 { get; set; }

		/// <summary>
		/// The city the location is in.
		/// </summary>
		[JsonProperty("city")]
		public virtual string City { get; set; }

		/// <summary>
		/// The zip or postal code.
		/// </summary>
		[PIIData]
		[JsonProperty("zip")]
		public virtual string PostalCode { get; set; }

		/// <summary>
		/// The two-letter code (ISO 3166-1 alpha-2 format) corresponding to country the location is in.
		/// </summary>
		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		/// <summary>
		/// The two-letter code corresponding to province or state the location is in.
		/// </summary>
		[JsonProperty("province_code")]
		public string ProvinceCode { get; set; }

		/// <summary>
		/// The name of the location.
		/// </summary>
		[PIIData]
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The ID for the location address.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The ID of Location
		/// </summary>
		[JsonIgnore]
		public long? LocationId { get; set; }
	}
}
