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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// The return type of the catalogDelete mutation.
	/// </summary>
	[GraphQLObject(MutationName = "catalogDelete")]
	public class CatalogDeletePayload : MutationPayload
	{
		/// <summary>
		/// The deleted catalog ID.
		/// </summary>
		[JsonProperty("deletedId")]
		[GraphQLField("deletedId", GraphQLConstants.DataType.Scalar, GraphQLConstants.ScalarType.ID)]
		public string DeletedId { get; set; }

		public class Arguments
		{
			[GraphQLArgument("deleteDependentResources", GraphQLConstants.ScalarType.Boolean, true)]
			public abstract class DeleteDependentResources { }

			[GraphQLArgument("id", GraphQLConstants.ScalarType.ID, false)]
			public abstract class Id { }
		}
	}
}
