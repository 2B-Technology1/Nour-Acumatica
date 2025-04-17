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
	/// The CompanyContactRoleAssign  fields to use when creating or updating a ContactRoleAssign .
	/// </summary>
	public class CompanyContactRoleAssign
	{
		/// <summary>
		/// A unique externally-supplied identifier for the company contact role.
		/// </summary>
		[JsonProperty("companyContactRoleId")]
		public string CompanyContactRoleId { get; set; }

		/// <summary>
		/// The unique externally-supplied identifier of the company location.
		/// </summary>
		[JsonProperty("companyLocationId")]
		public string CompanyLocationId { get; set; }

		[JsonIgnore]
		public string RoleAssignmentId { get; set; }

	}
}
