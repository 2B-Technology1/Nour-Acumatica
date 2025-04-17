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
	/// The type of line on the order that the discount is applicable on. Valid values:
	/// line_item: The discount applies to line items.
	/// shipping_line: The discount applies to shipping lines.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountTargetType
	{
		/// <summary>
		/// line_item: The discount applies to line items.
		/// </summary>
		[EnumMember(Value = "line_item")]
		LineItem = 0,

		/// <summary>
		/// shipping_line: The discount applies to shipping lines.
		/// </summary>
		[EnumMember(Value = "shipping_line")]
		ShippingLine = 1
	}
}
