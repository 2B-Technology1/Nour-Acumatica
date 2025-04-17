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
	/// https://shopify.dev/docs/api/admin-graphql/2023-04/enums/OrderDisplayFinancialStatus
	/// </summary>
	public class OrderFinancialStatusGQL
	{
		public const string Authorized = "AUTHORIZED";

		public const string Expired = "EXPIRED";

		public const string Paid = "PAID";

		public const string PartiallyPaid = "PARTIALLY_PAID";

		public const string PartiallyRefunded = "PARTIALLY_REFUNDED";

		public const string Pending = "PENDING";

		public const string Refunded = "REFUNDED";

		public const string Voided = "VOIDED";
    }
}

