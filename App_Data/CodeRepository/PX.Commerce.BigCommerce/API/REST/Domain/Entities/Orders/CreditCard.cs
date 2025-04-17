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
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class CreditCard
    {
        [JsonProperty("card_type")]
		[CommerceDescription(BigCommerceCaptions.CardType, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string CardType { get; set; }

        [JsonProperty("card_iin")]
		[CommerceDescription(BigCommerceCaptions.CardIin, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string CardIin { get; set; }

        [JsonProperty("card_last4")]
		[CommerceDescription(BigCommerceCaptions.CardLast4, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string CardLast4 { get; set; }

		[JsonProperty("card_expiry_month")]
		[CommerceDescription(BigCommerceCaptions.CardExpiryMonth, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public int CardExpiryMonth { get; set; }

		[JsonProperty("card_expiry_year")]
		[CommerceDescription(BigCommerceCaptions.CardExpiryYear, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public int CardExpiryYear { get; set; }
	}
}
