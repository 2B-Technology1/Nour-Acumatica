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
	public class OrderCouponsRestDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<OrdersCouponData>>
	{
		public virtual IChildRestDataProvider<OrdersCouponData> CreateInstance(IBigCommerceRestClient restClient) => new OrderCouponsRestDataProvider(restClient);
	}

	public class OrderCouponsRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersCouponData>
    {
        protected override string GetListUrl { get; } = "v2/orders/{parent_id}/coupons";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/coupons/{id}";

        public OrderCouponsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

		public virtual IEnumerable<OrdersCouponData> GetAll(string parentId, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(parentId);
			return GetAll<OrdersCouponData>(null, segments, cancellationToken: cancellationToken);
		}

		public virtual OrdersCouponData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersCouponData>(segments);
        }

		public virtual OrdersCouponData Create(OrdersCouponData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

		public virtual OrdersCouponData Update(OrdersCouponData entity, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Update(entity, segments);
        }

		public virtual bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Delete(segments);
        }
    }
}
