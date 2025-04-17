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
	/// Represents a purchasing entity. Can either be a customer or a purchasing company.
	/// </summary>
	public class PurchasingEntityInput
	{
		/// <summary>
		/// Unit of measurement for value.
		/// </summary>
		[JsonProperty("customerId", NullValueHandling = NullValueHandling.Ignore)]
		public string CustomerId { get; set; }

		/// <summary>
		/// Represents a purchasing company. Null if there is a customer.
		/// </summary>
		[JsonProperty("purchasingCompany", NullValueHandling = NullValueHandling.Ignore)]
		public PurchasingCompanyInput PurchasingCompany { get; set; }
	}

	/// <summary>
	/// Represents a purchasing company, which is a combination of company, company contact, and company location.
	/// </summary>
	public class PurchasingCompanyInput
	{
		/// <summary>
		/// ID of the company contact.
		/// </summary>
		[JsonProperty("companyContactId")]
		public string CompanyContactId { get; set; }

		/// <summary>
		/// ID of the company.
		/// </summary>
		[JsonProperty("companyId")]
		public string CompanyId { get; set; }

		/// <summary>
		/// ID of the company location.
		/// </summary>
		[JsonProperty("companyLocationId")]
		public string CompanyLocationId { get; set; }
	}
}
