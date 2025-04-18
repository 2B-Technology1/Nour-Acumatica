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
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CountryData
	{
		/// <summary>
		/// The two-letter country code (ISO 3166-1 alpha-2 format).
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// The ID for the country.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The ID for the shipping zone that the country belongs to.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		public long? ShippingZoneId { get; set; }

		/// <summary>
		/// The full name of the country.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The sub-regions of a country, such as its provinces or states.
		/// </summary>
		[JsonProperty("provinces")]
		public List<ProvinceData> Provinces { get; set; }

		/// <summary>
		/// The national sales tax rate applied to orders made by customers from that country.
		/// </summary>
		[JsonProperty("tax")]
		public decimal TaxRate { get; set; }

		/// <summary>
		/// The name of the tax for this country.
		/// </summary>
		[JsonProperty("tax_name")]
		public string TaxCode { get; set; }
	}

	public class ProvinceData
	{
		/// <summary>
		/// The standard abbreviation for the province.
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// The ID for the country that the province belongs to.
		/// </summary>
		[JsonProperty("country_id")]
		public long? CountryId { get; set; }

		/// <summary>
		/// The ID for the province.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The full name of the province.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The ID for the shipping zone that the province belongs to.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		public long? ShippingZoneId { get; set; }

		/// <summary>
		/// The sales tax rate to be applied to orders made by customers from this province.
		/// </summary>
		[JsonProperty("tax")]
		public decimal TaxRate { get; set; }

		/// <summary>
		/// The name of the tax for this province.
		/// </summary>
		[JsonProperty("tax_name")]
		public string TaxCode { get; set; }

		/// <summary>
		/// The standard abbreviation for the province.
		/// </summary>
		[JsonProperty("tax_type")]
		public TaxType? TaxType { get; set; }

		/// <summary>
		/// The province's tax in percent format.
		/// </summary>
		[JsonProperty("tax_percentage")]
		public decimal TaxPercentage { get; set; }
	}
}
