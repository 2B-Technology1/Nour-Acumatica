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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class Store
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("secure_url")]
        public string SecureUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("admin_email")]
        public string AdminEmail { get; set; }

        [JsonProperty("order_email")]
        public string OrderEmail { get; set; }

        [JsonProperty("timezone")]
        public StoreTimezone Timezone { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currency_symbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("decimal_separator")]
        public string DecimalSeparator { get; set; }

        [JsonProperty("thousands_separator")]
        public string ThousandsSeparator { get; set; }

        [JsonProperty("decimal_places")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("currency_symbol_location")]
        public string CurrencySymbolLocation { get; set; }

        [JsonProperty("weight_units")]
        public string WeightUnits { get; set; }

        [JsonProperty("dimension_units")]
        public string DimensionUnits { get; set; }

        [JsonProperty("dimension_decimal_places")]
        public string DimensionDecimalPlaces { get; set; }

        [JsonProperty("dimension_decimal_token")]
        public string DimensionDecimalToken { get; set; }

        [JsonProperty("dimension_thousands_token")]
        public string DimensionThousandsToken { get; set; }

        [JsonProperty("plan_name")]
        public string PlanName { get; set; }

        [JsonProperty("plan_level")]
        public string PlanLevel { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }

		[JsonIgnore]
		//Logo can be array if no logo on site and as a object if logo there. Ignoring
		//[JsonProperty("logo")] 
		public StoreLogo Logo { get; set; }

        [JsonProperty("is_price_entered_with_tax")]
        public bool IsPriceEnteredWithTax { get; set; }

        [JsonProperty("active_comparison_modules")]
        public object[] ActiveComparisonModules { get; set; }
    }
}
