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
	/// target_selection: The lines on the order, of the type defined by target_type, that the discount is allocated over. Valid values:
	/// all: The discount is allocated onto all lines,
	/// entitled: The discount is allocated only onto lines it is entitled for.
	/// explicit: The discount is allocated onto explicitly selected lines.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum DiscountTargetSelection
	{
		/// <summary>
		/// all: The discount is allocated onto all lines
		/// </summary>
		[EnumMember(Value = "all")]
		All = 0,

		/// <summary>
		/// entitled: The discount is allocated only onto lines it is entitled for.
		/// </summary>
		[EnumMember(Value = "entitled")]
		Entitled = 1,

		/// <summary>
		/// explicit: The discount is allocated onto explicitly selected lines.
		/// </summary>
		[EnumMember(Value = "explicit")]
		Explicit = 2
	}
}
