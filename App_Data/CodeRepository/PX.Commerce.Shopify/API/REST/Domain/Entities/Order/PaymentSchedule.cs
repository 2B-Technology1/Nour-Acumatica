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

namespace PX.Commerce.Shopify.API.REST
{
	[JsonObject(Description = "payment_schedules")]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PaymentSchedule : BCAPIEntity
	{
		/// <summary>
		/// The amount that is owed according to the payment terms.
		/// </summary>
		[JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
		public decimal? Amount { get; set; }

		/// <summary>
		/// The presentment currency for the payment.
		/// </summary>
		[JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
		public string Currency { get; set; }

		/// <summary>
		/// The date and time when the payment terms were initiated.
		/// </summary>
		[JsonProperty("issued_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? IssuedAt { get; set; }

		/// <summary>
		/// The date and time when the payment is due.
		/// Calculated based on issued_at and due_in_days or a customized fixed date if the type is fixed.
		/// </summary>
		[JsonProperty("due_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? DueAt { get; set; }

		/// <summary>
		/// The date and time when the purchase is completed.
		/// Returns null initially and updates when the payment is captured.
		/// </summary>
		[JsonProperty("completed_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? CompletedAt { get; set; }

		/// <summary>
		/// The name of the payment method gateway.
		/// </summary>
		[JsonProperty("expected_payment_method", NullValueHandling = NullValueHandling.Ignore)]
		public string ExpectedPaymentMethod { get; set; }

	}
}
