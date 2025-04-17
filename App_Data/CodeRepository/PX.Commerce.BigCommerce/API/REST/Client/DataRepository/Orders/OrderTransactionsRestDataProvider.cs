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
	public class OrderTransactionsRestDataProviderFactory : IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrdersTransactionData>>
	{
		public virtual IChildReadOnlyRestDataProvider<OrdersTransactionData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new OrderTransactionsRestDataProvider(restClient);
		}
	}

	public class OrderTransactionsRestDataProvider : RestDataProviderV3, IChildReadOnlyRestDataProvider<OrdersTransactionData>
    {
		private const string id_string = "id";
		private const string parent_id_string = "parent_id";

		protected override string GetListUrl { get; } = "v3/orders/{parent_id}/transactions";
        protected override string GetSingleUrl => string.Empty; //Not implemented on Big Commerce

		public OrderTransactionsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public OrdersTransactionData GetByID(string id, string parentId)
        {
			foreach(OrdersTransactionData tran in GetAll(parentId))
			{
				if (tran.Id == id.ToLong()) return tran;
			}
			return null;
        }

		public virtual IEnumerable<OrdersTransactionData> GetAll(string parentId, CancellationToken cancellationToken = default)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetAll<OrdersTransactionData, OrderTransactionsOptionList>(null, segments, cancellationToken);
        }
    }
}
