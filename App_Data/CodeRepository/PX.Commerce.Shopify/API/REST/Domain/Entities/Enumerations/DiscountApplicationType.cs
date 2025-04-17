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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The discount application type. Valid values:
	/// automatic: The discount was applied automatically, such as by a Buy X Get Y automatic discount.
	/// discount_code: The discount was applied by a discount code.
	/// manual: The discount was manually applied by the merchant (for example, by using an app or creating a draft order).
	/// script: The discount was applied by a Shopify Script.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountApplicationType
	{
		/// <summary>
		/// automatic: The discount was applied automatically, such as by a Buy X Get Y automatic discount.
		/// </summary>
		[EnumMember(Value = "automatic")]
		Automatic = 0,

		/// <summary>
		/// discount_code: The discount was applied by a discount code.
		/// </summary>
		[EnumMember(Value = "discount_code")]
		DiscountCode = 1,

		/// <summary>
		/// manual: The discount was manually applied by the merchant (for example, by using an app or creating a draft order).
		/// </summary>
		[EnumMember(Value = "manual")]
		Manual = 2,

		/// <summary>
		/// script: The discount was applied by a Shopify Script.
		/// </summary>
		[EnumMember(Value = "script")]
		Script = 3
	}
}
