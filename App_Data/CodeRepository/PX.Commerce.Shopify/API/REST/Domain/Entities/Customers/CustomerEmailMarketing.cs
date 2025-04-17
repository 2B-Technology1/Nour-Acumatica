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

namespace PX.Commerce.Shopify.API.REST
{
	public class CustomerEmailMarketing
	{
		/// <summary>
		/// The current email marketing state for the customer.
		/// </summary>
		[JsonProperty("state")]
		public virtual string State { get; set; }

		/// <summary>
		/// The marketing subscription opt-in level, as described in the M3AAWG Sender Best Common Practices,
		/// that the customer gave when they consented to receive marketing material by email.
		/// </summary>
		[JsonProperty("opt_in_level")]
		public virtual string Opt_in_level { get; set; }

		/// <summary>
		/// The date and time when the customer consented to receive marketing material by email.
		/// If no date is provided, then the date and time when the consent information was sent is used.
		/// </summary>
		[JsonProperty("consent_updated_at")]
		public virtual string Consent_updated_at { get; set; }

	}
}
