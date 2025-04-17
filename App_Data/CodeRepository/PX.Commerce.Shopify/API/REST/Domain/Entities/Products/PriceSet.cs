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
	[CommerceDescription(ShopifyCaptions.PriceSet)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PriceSet
	{
		[JsonProperty("shop_money", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.ShopMoney, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public PresentmentPrice ShopMoney { get; set; }

		[JsonProperty("presentment_money", NullValueHandling = NullValueHandling.Ignore)]
		[CommerceDescription(ShopifyCaptions.PresentmentMoney, FieldFilterStatus.Skipped, FieldMappingStatus.Import)]
		public PresentmentPrice PresentmentMoney { get; set; }
	}
}
