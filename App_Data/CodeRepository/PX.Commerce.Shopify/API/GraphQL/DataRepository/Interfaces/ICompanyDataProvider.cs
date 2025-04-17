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
	/// Provides functionality for performing CRUD tasks with companies in an external system.
	/// </summary>
	public interface ICompanyGQLDataProvider
	{
		/// <summary>
		/// Creates a new company in the store.
		/// </summary>
		/// <param name="company">Object describing the attributes of the company.</param>
		/// <param name="contact">Objects describing the attributes of the company contact.</param>
		/// <param name="location">Objects describing the attributes of the company location.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created company.</returns>
		public Task<CompanyDataGQL> CreateCompanyAsync(CompanyDataGQL company, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates a company in the store with the specified ID.
		/// </summary>
		/// <param name="companyId">The ID of the company to update.</param>
		/// <param name="company">Object describing the attributes of the company.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The updated company.</returns>
		public Task<CompanyDataGQL> UpdateCompanyAsync(string companyId, CompanyDataGQL company, CancellationToken cancellationToken = default);

		/// <summary>
		/// Delete a company
		/// </summary>
		/// <param name="companyId">The company ID</param>
		/// <param name="cancellationToken"></param>
		/// <returns>The deleted company ID</returns>
		public Task<string> DeleteCompanyAsync(string companyId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a company contact in the store for the company with the specified ID.
		/// </summary>
		/// <param name="companyId">The Id of the company to add the contact to.</param>
		/// <param name="contact">Object describing the attributes of the contact</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created company contact.</returns>
		public Task<CompanyContactDataGQL> CreateCompanyContactAsync(string companyId, CompanyContactDataGQL contact, CancellationToken cancellationToken = default);

		/// <summary>
		/// Assign Customer As a Company Contact
		/// </summary>
		/// <param name="companyId">The Id of the company to add the contact to.</param>
		/// <param name="customerId">The Id of the customer</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<CompanyContactDataGQL> AssignCustomerAsContactAsync(string companyId, string customerId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Assign a contact or customer as the main contact of a company
		/// </summary>
		/// <param name="companyId">The Id of the company to add the contact to.</param>
		/// <param name="companyContactId">The Id of the contact</param>
		/// <param name="isCustomer">Determines whether the contact is an existing customer or a company contact</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<CompanyDataGQL> AssignCompanyMainContactAsync(string companyId, string companyContactId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Revoke assignRoles for the specified location
		/// </summary>
		/// <param name="companyContactId">The Id of the company contact</param>
		/// <param name="revokeRoleAssignmentIds">The RoleAssignmentId list that need to revoke from current company contact</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<IEnumerable<string>> RevokeCompanyContactRolesAsync(string companyContactId, List<string> revokeRoleAssignmentIds, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates a company contact in the store with the specified id.
		/// </summary>
		/// <param name="companyContactId">The Id of the company contact to update.</param>
		/// <param name="contact">Object describing the attributes of the contact</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created company contact.</returns>
		public Task<CompanyContactDataGQL> UpdateCompanyContactAsync(string companyContactId, CompanyContactDataGQL contact, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a company location in the store for the company with the specified ID.
		/// </summary>
		/// <param name="companyId">The Id of the company to add the location to.</param>
		/// <param name="location">Object describing the attributes of the location</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The created company location.</returns>
		public Task<CompanyLocationDataGQL> CreateCompanyLocationAsync(string companyId, CompanyLocationDataGQL location, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates a company location in the store with the specified id.
		/// </summary>
		/// <param name="companyLocationId">The Id of the company location to update.</param>
		/// <param name="location">Object describing the attributes of the location.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The updated company location.</returns>
		public Task<CompanyLocationDataGQL> UpdateCompanyLocationAsync(string companyLocationId, CompanyLocationDataGQL location, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update the address for the specified Location
		/// </summary>
		/// <param name="companyLocationId">The Id of the company location</param>
		/// <param name="addressData">The company address data</param>
		/// <param name="addressTypes">The address type, available value: BILLING or SHIPPING</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The updated company address data.</returns>
		public Task<IEnumerable<CompanyAddressDataGQL>> UpdateCompanyLocationAddressAsync(string companyLocationId, CompanyAddressDataGQL addressData, string[] addressTypes, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create new roles for specified company contact
		/// </summary>
		/// <param name="companyContactId">company contact id</param>
		/// <param name="assignRoles">The roles list for company locations</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The list of CompanyContactRoleAssignmentData</returns>
		public Task<IEnumerable<CompanyContactRoleAssignmentDataGQL>> AssignCompanyContactRolesAsync(string companyContactId, List<CompanyContactRoleAssign> assignRoles, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets all companies in the store.
		/// </summary>
		/// <param name="filterString">A string which provides filter criteria for the query.</param>
		/// <param name="includedSubFields"></param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <param name="specifiedFieldsOnly"></param>
		/// <returns>An IEnumerable of the retrieved companies.</returns>
		public Task<IEnumerable<CompanyDataGQL>> GetCompaniesAsync(string filterString = null, bool includedSubFields = false, CancellationToken cancellationToken = default, params string[] specifiedFieldsOnly);

		/// <summary>
		/// Gets the company with the specified ID.
		/// </summary>
		/// <param name="id">The ID of the company to retrieve.</param>
		/// <param name="cancellationToken">Cancellation token for the operation.</param>
		/// <returns>The retrieved company.</returns>
		public Task<CompanyDataGQL> GetCompanyByIDAsync(string id, bool withContacts = false, bool withLocations = false, CancellationToken cancellationToken = default);

		public Task<IEnumerable<CompanyContactDataGQL>> GetCompanyContactsByCompanyIDAsync(string id, int contactCount, CancellationToken cancellationToken = default);

		public Task<IEnumerable<CompanyContactRoleAssignmentDataGQL>> GetContactRoleAssignmentsByContactIDAsync(string id, CancellationToken cancellationToken = default);

		public Task<IEnumerable<CompanyLocationDataGQL>> GetCompanyLocationsByCompanyIDAsync(string id, int locationCount, CancellationToken cancellationToken = default);

		public Task<IEnumerable<CompanyContactRoleDataGQL>> GetCompanyContactRolesByCompanyIDAsync(string id, int contactCount, CancellationToken cancellationToken = default);

		public Task<IEnumerable<CustomerDataGQL>> GetCompanyCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);
	}
}
