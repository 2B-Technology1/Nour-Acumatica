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

using PX.Commerce.Core;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class OrderRefundsRestDataProviderFactory : IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrderRefund>>
	{
		public virtual IChildReadOnlyRestDataProvider<OrderRefund> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new OrderRefundsRestDataProvider(restClient);
		}
	}
	public class OrderRefundsRestDataProvider : RestDataProviderV3, IChildReadOnlyRestDataProvider<OrderRefund>
	{
		private const string id_string = "id";
		private const string parent_id_string = "parent_id";

		protected override string GetListUrl { get; } = "v3/orders/{parent_id}/payment_actions/refunds";
		protected override string GetSingleUrl => string.Empty; //Not implemented on Big Commerce

		public OrderRefundsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}


		public virtual OrderRefund GetByID(string id, string parentId)
		{
			foreach (OrderRefund refund in GetAll(parentId))
			{
				if (refund.Id == id.ToInt()) return refund;
			}
			return null;
		}

		public virtual IEnumerable<OrderRefund> GetAll(string externID, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(externID);
			return base.GetAll<OrderRefund, OrderRefundsList>(null, segments, cancellationToken);
		}
	}
}
