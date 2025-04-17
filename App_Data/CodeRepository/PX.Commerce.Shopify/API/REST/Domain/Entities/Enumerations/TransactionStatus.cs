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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The status of the transaction. Valid values: pending, failure, success, and error.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TransactionStatus
	{
		/// <summary>
		/// pending: The transaction is pending.
		/// </summary>
		[EnumMember(Value = "pending")]
		Pending,

		/// <summary>
		/// failure: The transaction request failed.
		/// </summary>
		[EnumMember(Value = "failure")]
		Failure,

		/// <summary>
		/// success: The transaction was successful.
		/// </summary>
		[EnumMember(Value = "success")]
		Success,

		/// <summary>
		/// Awaiting a response.
		/// </summary>
		[EnumMember(Value = "awaiting_response")]
		AwaitingResponse,

		/// <summary>
		/// There was an error while processing the transaction.
		/// </summary>
		[EnumMember(Value = "error")]
		Error,

		/// <summary>
		/// The transaction status is unknown.
		/// </summary>
		[EnumMember(Value = "unknown")]
		Unknown

	}
}
