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
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The valid discount types that can be applied to a draft order.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DraftOrderAppliedDiscountType
	{
		/// <summary>
		/// A fixed amount in the store's currency.
		/// </summary>
		[EnumMember(Value = "FIXED_AMOUNT")]
		FixedAmount = 0,

		/// <summary>
		/// percentage: Applies a discount of amount as a percentage of the order total.
		/// </summary>
		[EnumMember(Value = "PERCENTAGE")]
		Percentage = 1,
	}
}
