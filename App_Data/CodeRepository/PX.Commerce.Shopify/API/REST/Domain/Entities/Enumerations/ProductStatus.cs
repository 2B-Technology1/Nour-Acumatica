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
	/// The status of the product. Valid values: active, archived,draft.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ProductStatus
	{
		/// <summary>
		/// active: The product is ready to sell and is available to customers on the online store, sales channels, and apps. 
		/// By default, existing products are set to active.
		/// </summary>
		[EnumMember(Value = "active")]
		Active = 0,

		/// <summary>
		/// archived: The product is no longer being sold and isn't available to customers on sales channels and apps.
		/// </summary>
		[EnumMember(Value = "archived")]
		Archived = 1,

		/// <summary>
		/// draft: The product isn't ready to sell and is unavailable to customers on sales channels and apps. 
		/// By default, duplicated and unarchived products are set to draft.
		/// </summary>
		[EnumMember(Value = "draft")]
		Draft = 2

	}
}
