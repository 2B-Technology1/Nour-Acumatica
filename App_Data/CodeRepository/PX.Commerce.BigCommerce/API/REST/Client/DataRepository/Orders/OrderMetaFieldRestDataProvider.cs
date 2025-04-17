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
using System.Linq;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class OrderMetaFieldRestDataProviderFactory : IBCRestDataProviderFactory<IOrderMetaFieldRestDataProvider>
	{
		public virtual IOrderMetaFieldRestDataProvider CreateInstance(IBigCommerceRestClient restClient) => new OrderMetaFieldRestDataProvider(restClient);
	}

	public class OrderMetaFieldRestDataProvider : RestDataProviderV3, IOrderMetaFieldRestDataProvider
	{
		protected override string GetListUrl { get; } = "/v3/orders/{parent_id}/metafields";

		protected override string GetSingleUrl { get; } = "/v3/orders/{parent_id}/metafields/{id}";

		public OrderMetaFieldRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public virtual OrdersMetaFieldData Create(OrdersMetaFieldData entity, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			return base.Create(entity, segments);
		}

		public virtual IEnumerable<OrdersMetaFieldData> GetAll(IFilter filter, string parentId, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(parentId);

			return base.GetAll<OrdersMetaFieldData, OrdersMetaFieldList>(filter, segments, cancellationToken);
		}

		public virtual OrdersMetaFieldData Update(OrdersMetaFieldData entity, string id, string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return base.Update(entity, segments);
		}
	}
}
