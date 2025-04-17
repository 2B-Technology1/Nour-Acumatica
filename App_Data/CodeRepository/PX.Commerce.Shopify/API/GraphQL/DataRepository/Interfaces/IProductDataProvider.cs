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
	/// Provides functionality for performing CRUD tasks with products in an external system.
	/// </summary>
	public interface IProductGQLDataProvider
	{
		/// <summary>
		/// Gets all product variants in the store.
		/// </summary>
		/// <param name="filterString">A string which provides filter criteria for the query.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>An IEnumerable of the retrieved companies.</returns>
		public Task<IEnumerable<ProductVariantGQL>> GetProductVariantsAsync(string filterString = null, CancellationToken cancellationToken = default);
	}
}
