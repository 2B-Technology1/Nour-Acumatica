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

using PX.Commerce.Core.Model;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class OrderTaxesRestDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<OrdersTaxData>>
	{
		public virtual IChildRestDataProvider<OrdersTaxData> CreateInstance(IBigCommerceRestClient restClient) => new OrderTaxesRestDataProvider(restClient);
	}

	public class OrderTaxesRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersTaxData>
    {
        protected override string GetListUrl { get; } = "v2/orders/{parent_id}/taxes?details=true";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/taxes/{id}?details=true";

        public OrderTaxesRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public virtual IEnumerable<OrdersTaxData> GetAll(string parentId, CancellationToken cancellationToken = default)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetAll<OrdersTaxData>(null, segments, cancellationToken: cancellationToken);
        }

		public virtual OrdersTaxData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersTaxData>(segments);
        }

		public virtual OrdersTaxData Create(OrdersTaxData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return Create(entity, segments);
        }

		public virtual OrdersTaxData Update(OrdersTaxData entity, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Update(entity, segments);
        }

		public virtual bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }
    }
}
