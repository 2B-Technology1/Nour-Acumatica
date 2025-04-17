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
	/// Contains queries for mutations
	/// </summary>
	public class CompanyMutation
	{
		/// <summary>
		/// The payload for the CompanyCreate mutation.
		/// </summary>
		[JsonProperty("companyCreate")]
		public CompanyCreatePayload CompanyCreate { get; set; }

		/// <summary>
		/// The payload for the CompanyDelete mutation.
		/// </summary>
		[JsonProperty("companyDelete")]
		public CompanyDeletePayload CompanyDelete { get; set; }

		/// <summary>
		/// The payload for the CompanyUpdate mutation.
		/// </summary>
		[JsonProperty("companyUpdate")]
		public CompanyUpdatePayload CompanyUpdate { get; set; }

		/// <summary>
		/// The payload for the CompanyContactCreate mutation.
		/// </summary>
		[JsonProperty("companyContactCreate")]
		public CompanyContactCreatePayload CompanyContactCreate { get; set; }

		/// <summary>
		/// The payload for the CompanyContactUpdate mutation.
		/// </summary>
		[JsonProperty("companyContactUpdate")]
		public CompanyContactCreatePayload CompanyContactUpdate { get; set; }

		/// <summary>
		/// The payload for the CompanyLocationCreate mutation.
		/// </summary>
		[JsonProperty("companyLocationCreate")]
		public CompanyLocationCreatePayload CompanyLocationCreate { get; set; }

		/// <summary>
		/// The payload for the CompanyLocationUpdate mutation.
		/// </summary>
		[JsonProperty("companyLocationUpdate")]
		public CompanyLocationUpdatePayload CompanyLocationUpdate { get; set; }

		/// <summary>
		/// The payload for the CompanyLocationUpdate mutation.
		/// </summary>
		[JsonProperty("companyLocationAssignAddress")]
		public CompanyLocationAssignAddressPayload CompanyLocationAssignAddress { get; set; }

		/// <summary>
		/// The payload for the CompanyContactAssignRoles mutation.
		/// </summary>
		[JsonProperty("companyContactAssignRoles")]
		public CompanyContactAssignRolesPayload CompanyContactAssignRolesCreate { get; set; }

		/// <summary>
		/// The payload for the CompanyContactRevokeRoles mutation.
		/// </summary>
		[JsonProperty("companyContactRevokeRoles")]
		public CompanyContactRevokeRolesPayload CompanyContactRevokeRoles { get; set; }

		/// <summary>
		/// The payload for the companyAssignCustomerAsContact mutation.
		/// </summary>
		[JsonProperty("companyAssignCustomerAsContact")]
		public CompanyAssignCustomerAsContactPayload CompanyAssignCustomerAsContact { get; set; }

		/// <summary>
		/// The payload for the companyAssignCustomerAsContact mutation.
		/// </summary>
		[JsonProperty("companyAssignMainContact")]
		public CompanyAssignMainContactPayload CompanyAssignMainContact { get; set; }
	}
}
