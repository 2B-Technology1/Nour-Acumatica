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


namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Provides functionality for instantiating implementations of ISPGraphQLDataProvider.
	/// </summary>
	/// <typeparam name="TDataProvider">The implementation of <see cref="ISPGraphQLDataProvider"/> to instantiate.</typeparam>
	public interface ISPGraphQLDataProviderFactory<out TDataProvider> where TDataProvider : SPGraphQLDataProvider
	{
		/// <summary>
		///	Creates an instance of <typeparamref name="TDataProvider"/> with <paramref name="graphQLAPIService"/> and returns it.
		/// </summary>
		/// <param name="graphQLAPIService">The <see cref="ISPGraphQLAPIService"/> the instance of
		/// <typeparamref name="TDataProvider"/> will use.</param>
		/// <returns>A reference to the created <see cref="SPGraphQLDataProvider"/>.</returns>
		TDataProvider GetProvider(IGraphQLAPIClient graphQLAPIService);
	}
}
