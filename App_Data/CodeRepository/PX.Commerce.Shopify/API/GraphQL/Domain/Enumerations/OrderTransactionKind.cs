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

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// https://shopify.dev/docs/api/admin-graphql/2023-04/enums/OrderTransactionKind
	/// </summary>
	public static class OrderTransactionTypeGQL
	{
		public const string Authorization = "AUTHORIZATION";
		public const string Capture = "CAPTURE";
		public const string Change = "CHANGE";
		public const string EmvAuthorization = "EMV_AUTHORIZATION";
		public const string Refund = "REFUND";
		public const string Sale = "SALE";
		public const string SuggestedRefund = "SUGGESTED_REFUND";
		public const string Void = "VOID";
	}
}

