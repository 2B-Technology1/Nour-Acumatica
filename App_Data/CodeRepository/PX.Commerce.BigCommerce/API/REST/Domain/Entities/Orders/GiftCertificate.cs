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
	public class GiftCertificate
    {
        [JsonProperty("code")]
		[CommerceDescription(BigCommerceCaptions.Code, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public string Code { get; set; }

        [JsonProperty("original_balance")]
		[CommerceDescription(BigCommerceCaptions.OriginalBalance, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public double OriginalBalance { get; set; }

        [JsonProperty("starting_balance")]
		[CommerceDescription(BigCommerceCaptions.StartingBalance, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public double StartingBalance { get; set; }

        [JsonProperty("remaining_balance")]
		[CommerceDescription(BigCommerceCaptions.RemainingBalance, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public double RemainingBalance { get; set; }

        [JsonProperty("status")]
		[CommerceDescription(BigCommerceCaptions.GiftCertificateStatus, FieldFilterStatus.Skipped, FieldMappingStatus.ImportAndExport)]
		public GiftCertificateStatus Status { get; set; }
    }
}
