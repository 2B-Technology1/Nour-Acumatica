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
	/// Whether the product is published to the Point of Sale channel. Valid values:
	/// web: The product is published to the Online Store channel but not published to the Point of Sale channel.
	/// global: The product is published to both the Online Store channel and the Point of Sale channel.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
    public enum PublishedScope
    {
		/// <summary>
		/// web: The product is published to the Online Store channel but not published to the Point of Sale channel.
		/// </summary>
		[EnumMember(Value = "web")]
		Web = 0,

		/// <summary>
		/// global: The product is published to both the Online Store channel and the Point of Sale channel.
		/// </summary>
		[EnumMember(Value = "global")]
		Global = 1
    }
}
