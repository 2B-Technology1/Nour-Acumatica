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
	/// Provides functionality for performing CRUD tasks with customers in an external system.
	/// </summary>
	public interface ICustomerGQLDataProvider
	{
		/// <summary>
		/// Create a new customer in the store.
		/// </summary>
		/// <param name="customerInput"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created customer.</returns>
		public Task<CustomerDataGQL> CreateCustomerAsync(CustomerInput customerInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update a customer in the store.
		/// </summary>
		/// <param name="customerInput"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The updated customer.</returns>
		public Task<CustomerDataGQL> UpdateCustomerAsync(CustomerInput customerInput, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update the default address of the customer in the store.
		/// </summary>
		/// <param name="customerId">The id of the customer</param>
		/// <param name="addressId">The new default address id</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The customer.</returns>
		public Task<CustomerDataGQL> UpdateCustomerDefaultAddressAsync(string customerId, string addressId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete a customer in the store.
		/// </summary>
		/// <param name="customerId">The id of the customer</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The deleted customer ID</returns>
		public Task<string> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get customers
		/// </summary>
		/// <param name="filterString">Specify the query conditions</param>
		/// <param name="includedSubFields"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <param name="specifiedFieldsOnly">Specify the fields</param>
		/// <returns>The customer list</returns>
		public Task<IEnumerable<CustomerDataGQL>> GetCustomersAsync(string filterString = null, string sortKeyFieldName = OrderSortKeys.UpdatedAt, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get the customer by ID
		/// </summary>
		/// <param name="id">The id of the customer</param>
		/// <param name="includedSubFields"></param>
		/// <param name="includedMetaFields"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The customer</returns>
		public Task<CustomerDataGQL> GetCustomerByIDAsync(string id, bool includedSubFields = true, bool includedMetaFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Get customer addresses
		/// </summary>
		/// <param name="id">The id of the customer</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The address list</returns>
		public Task<IEnumerable<MailingAddress>> GetCustomerAddressesAsync(string id, CancellationToken cancellationToken = default);
	}
}
