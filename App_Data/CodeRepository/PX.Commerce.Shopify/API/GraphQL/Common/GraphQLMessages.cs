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

using PX.Common;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Contains constants describing messages used by the GraphQL client.
	/// </summary>
	[PXLocalizable]
	public class GraphQLMessages
	{
		public const string AggregateExceptionMessage = "At least one GraphQL error has occurred. {0}";
		public const string ExceptionMessage = "Error: {0}";
		public const string QueryWithoutAgruments = "The query for fetching multiple records must include either the first, or the last argument.";
		public const string QueryAgrumentWithoutValue = "At least one of the query arguments is missing a required value.";
		public const string MutationWithoutAgruments = "No arguments have been found in the mutation {0}.";
		public const string NoQueryFields = "No GraphQL query fields have been found in the query object {0}.";

		//GraphQL Client error messages
		public const string GraphQLClientEmptyEndpoint = "The endpoint used to initialize the GraphQLAPIClient cannot be null or empty.";
		public const string GraphQLClientInvalidEndpointFormat = "The endpoint format for the GraphQLAPIClient is invalid.";
		public const string GraphQLCostExceed = "The total cost of your queries cannot exceed 1,000 points.";

	}
}
