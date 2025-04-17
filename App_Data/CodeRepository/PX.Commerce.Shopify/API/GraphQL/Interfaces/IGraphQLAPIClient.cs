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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Provides functionality for executing GraphQL queries to an external GraphQL API.
	/// </summary>
	/// <typeparam name="TQueryRoot">The type to deserialize responses to.</typeparam>
	public interface IGraphQLAPIClient
	{
		/// <summary>
		/// Prepares and sends the query to the GraphQL API and returns the results or an exception.
		/// </summary>
		/// <param name="query">The query to send.</param>
		/// <param name="queryCost">The cost point of the query</param>
		/// <param name="variables">Variables to include in the request</param>
		/// <param name="cancellationToken">Cancellation token for the task.</param>
		/// <returns>A  containing the response as an instance of <typeparamref name="TQueryRoot"/>.</returns>
		Task<TQueryRoot> ExecuteAsync<TQueryRoot>(string query, int queryCost, Dictionary<string, object> variables,
			CancellationToken cancellationToken = default);
	}
}
