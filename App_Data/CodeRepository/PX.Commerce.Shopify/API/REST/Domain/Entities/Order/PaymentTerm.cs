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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Shopify.API.REST;

namespace PX.Commerce.Shopify.API
{
	[JsonObject(Description = "payment_terms")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

	public class PaymentTerm : BCAPIEntity
	{
		/// <summary>
		/// The amount that is owed according to the payment terms
		/// </summary>
		[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// The presentment currency for the payment
		/// </summary>
		[JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }

		/// <summary>
		/// The number of days between the invoice date and due date that is defined in the selected payment terms template.
		/// </summary>
		[JsonProperty("due_in_days", NullValueHandling = NullValueHandling.Ignore)]
		public int? DueInDays { get; set; }

		/// <summary>
		/// The name of the selected payment terms template for the order.
		/// </summary>
		[JsonProperty("payment_terms_name", NullValueHandling = NullValueHandling.Ignore)]
		public string PaymentTermsName { get; set; }

		/// <summary>
		/// The type of selected payment terms template for the order.
		/// </summary>
		[JsonProperty("payment_terms_type", NullValueHandling = NullValueHandling.Ignore)]
		public string PaymentTermsType { get; set; }

		/// <summary>
		/// An array of schedules associated to the payment terms.
		/// </summary>
		[JsonProperty("payment_schedules", NullValueHandling = NullValueHandling.Ignore)]
		public List<PaymentSchedule> PaymentSchedules { get; set; }
	}
}
