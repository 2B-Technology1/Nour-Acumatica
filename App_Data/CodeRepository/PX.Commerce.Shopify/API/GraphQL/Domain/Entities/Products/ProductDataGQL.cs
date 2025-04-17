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

using System.Linq;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.GraphQL
{
	[GraphQLObject(NodeName = "product", ConnectionName = "products")]
	public class ProductDataGQL: BCAPIEntity, INode
	{
		[JsonProperty("id")]
		[GraphQLField("id", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string Id { get; set; }

		[JsonProperty("isGiftCard")]
		[GraphQLField("isGiftCard", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.Boolean)]
		public bool IsGiftCard { get; set; }

		[JsonIgnore]
		public string IdNumber => Id?.Split('/').LastOrDefault();
	}
}
