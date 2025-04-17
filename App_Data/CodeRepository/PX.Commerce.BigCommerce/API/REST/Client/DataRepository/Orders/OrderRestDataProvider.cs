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

using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class OrderRestDataProviderFactory : IBCRestDataProviderFactory<IOrderRestDataProvider>
	{
		public virtual IOrderRestDataProvider CreateInstance(IBigCommerceRestClient restClient) => new OrderRestDataProvider(restClient);
	}

	public class OrderRestDataProvider : RestDataProviderV2, IOrderRestDataProvider
	{
        protected override string GetListUrl { get; } = "v2/orders";
        protected override string GetSingleUrl { get; } = "v2/orders/{id}";

		public OrderRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;

		}

		public OrderData Create(OrderData order)
		{
			var newOrder = Create<OrderData>(order);
			return newOrder;
		}

		public virtual OrderData Update(OrderData order, string id)
		{
			var segments = MakeUrlSegments(id);
			var updated = Update(order, segments);
			return updated;
		}

		public virtual OrderStatus Update(OrderStatus order, string id)
		{
			var segments = MakeUrlSegments(id);
			var updated = Update(order, segments);
			return updated;
		}

        public bool Delete(string id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return base.Delete(segments);
        }

		public virtual bool Delete(string id, OrderData order)
		{
			return Delete(id);
		}

		public virtual List<OrderData> Get(IFilter filter = null)
		{
			return base.Get<OrderData>(filter);
        }

		public virtual IEnumerable<OrderData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return base.GetAll<OrderData>(filter, cancellationToken: cancellationToken);
        }

		public virtual OrderData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
            var orderData = GetByID<OrderData>(segments);

			return orderData;
        }
	}
}
