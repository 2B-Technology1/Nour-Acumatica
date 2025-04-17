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
	public class OrderShipmentsRestDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<OrdersShipmentData>>
	{
		public virtual IChildRestDataProvider<OrdersShipmentData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new OrderShipmentsRestDataProvider(restClient);
		}
	}

	public class OrderShipmentsRestDataProvider : RestDataProviderV2, IChildRestDataProvider<OrdersShipmentData>
    {
        protected override string GetListUrl { get; }   = "v2/orders/{parent_id}/shipments";
        protected override string GetSingleUrl { get; } = "v2/orders/{parent_id}/shipments/{id}";

        public OrderShipmentsRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        public virtual IEnumerable<OrdersShipmentData> GetAll(string parentId, CancellationToken cancellationToken = default)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetAll<OrdersShipmentData>(null, segments, cancellationToken: cancellationToken);
        }

		public virtual OrdersShipmentData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<OrdersShipmentData>(segments);
        }

		/// <remarks>
		/// Presuming that a valid carrier code is used, a tracking link is generated if either shipping_provider or tracking_carrier is supplied alongside a tracking number.<br/>
		/// Providing only the tracking number will result in an unclickable text in the customer facing email.
		/// Acceptable values for shipping_provider include an empty string (""), auspost, canadapost, endicia, usps, fedex, royalmail, ups, upsready, upsonline, or shipperhq.
		/// Acceptable values for tracking_carrier include an empty string ("") or one of the valid <see href="https://github.com/bigcommerce/dev-docs/blob/master/assets/csv/tracking_carrier_values.csv">tracking-carrier values.</see>
		/// </remarks>
		public virtual OrdersShipmentData Create(OrdersShipmentData entity, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            return base.Create(entity, segments);
        }

		public virtual OrdersShipmentData Update(OrdersShipmentData entity, string id, string parentId)
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
