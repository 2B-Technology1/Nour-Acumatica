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
	/// The company contact attributes to use when creating a company or updating a company contact.
	/// </summary>
	public class CompanyContactInput
	{
		/// <summary>
		/// The unique email address of the company contact.
		/// </summary>
		[JsonProperty("email")]
		public string Email { get; set; }

		/// <summary>
		/// The company contact's first name.
		/// </summary>
		[JsonProperty("firstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// The company contact's last name.
		/// </summary>
		[JsonProperty("lastName")]
		public string LastName { get; set; }

		/// <summary>
		/// The phone number of the company contact.
		/// </summary>
		[JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
		public string Phone { get; set; }

		/// <summary>
		/// The title of the company contact.
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }
	}
}
