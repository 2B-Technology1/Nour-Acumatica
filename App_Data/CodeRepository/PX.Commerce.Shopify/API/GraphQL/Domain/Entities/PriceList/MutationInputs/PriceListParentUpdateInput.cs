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
	/// The input fields to create a price list adjustment.
	/// </summary>
	public class PriceListParentUpdateInput
	{
		public PriceListParentUpdateInput()
		{
			Adjustment = new PriceListAdjustmentInput { Type = PriceListAdjustmentTypeGQL.PercentageDecrease, Value = 0 };
			Settings = new PriceListAdjustmentSettingsInput { CompareAtMode = PriceListCompareAtModeGQL.Nullify };
		}
		/// <summary>
		/// The relative adjustments to other prices.
		/// </summary>
		[JsonProperty("adjustment")]
		public PriceListAdjustmentInput Adjustment { get; set; }

		/// <summary>
		/// The input fields to set a price list's adjustment settings.
		/// </summary>
		[JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
		public PriceListAdjustmentSettingsInput Settings { get; set; }

	}
}
