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

using PX.Commerce.Shopify.API.GraphQL;

namespace PX.Commerce.Shopify
{

	/// <summary>
	/// Holds the value, type and category of a metafield instance.
	/// </summary>
	public class MetafieldValue
	{

		public MetafieldValue(string value, MetafieldCategory category, ShopifyMetafieldType type, string originalShopifyType)
		{
			Value = value;
			Type = type;
			Category = category;
			OriginalShopifyType = originalShopifyType;
		}
		public string Value { get; private set; }
		private ShopifyMetafieldType Type { get; set; }
		public MetafieldCategory Category { get; private set; }
		public string OriginalShopifyType { get; private set; }

		public string GetShopifyType()
		{

			if (Type == ShopifyMetafieldType.NotSupportedShopifyType)
				return this.OriginalShopifyType;

			if ((Category == MetafieldCategory.List || Category == MetafieldCategory.jSonList) && Type != ShopifyMetafieldType.multi_line_text_field)
				return $"list.{Type.ToString()}";


			return Type.ToString();
		}
	}

}
