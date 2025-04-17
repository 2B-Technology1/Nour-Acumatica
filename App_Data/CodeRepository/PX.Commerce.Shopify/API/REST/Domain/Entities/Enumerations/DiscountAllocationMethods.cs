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
	/// allocation_method: The method by which the discount application value has been allocated to entitled lines. Valid values:
	/// across: The value is spread across all entitled lines.
	/// each: The value is applied onto every entitled line.
	/// one: The value is applied onto a single line.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountAllocationMethods
	{
		/// <summary>
		/// across: The value is spread across all entitled lines.
		/// </summary>
		[EnumMember(Value = "across")]
		Across = 0,

		/// <summary>
		/// each: The value is applied onto every entitled line.
		/// </summary>
		[EnumMember(Value = "each")]
		Each = 1,

		/// <summary>
		/// one: The value is applied onto a single line.
		/// </summary>
		[EnumMember(Value = "one")]
		One = 2
	}
}
