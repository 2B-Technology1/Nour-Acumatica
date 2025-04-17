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
	/// Represents the query arguments for GraphQL query data
	/// In each query, the argument may be used in different level query object, you need to specify the query object type in each argument.
	/// </summary>
	public class QueryArgument
	{
		[GraphQLArgument("id", GraphQLConstants.ScalarType.ID, false)]
		public abstract class ID<T> { }

		[GraphQLArgument("after", GraphQLConstants.ScalarType.String)]
		public abstract class After<T> { }

		[GraphQLArgument("before", GraphQLConstants.ScalarType.String)]
		public abstract class Before<T> { }

		[GraphQLArgument("first", GraphQLConstants.ScalarType.Int, false, true)]
		public abstract class First<T> { }

		[GraphQLArgument("last", GraphQLConstants.ScalarType.Int, false, true)]
		public abstract class Last<T> { }

		[GraphQLArgument("reverse", GraphQLConstants.ScalarType.Boolean)]
		public abstract class Reverse<T> { }

		[GraphQLArgument("query", GraphQLConstants.ScalarType.String)]
		public abstract class Query<T> { }

		[GraphQLArgument("savedSearchId", GraphQLConstants.ScalarType.ID)]
		public abstract class SavedSearchId<T> { }

		[GraphQLArgument("namespace", GraphQLConstants.ScalarType.String)]
		public abstract class Namespace<T> { }

		[GraphQLArgument("ownerType", "MetafieldOwnerType", false)]
		public abstract class OwnerType<T> { }
	}
}
