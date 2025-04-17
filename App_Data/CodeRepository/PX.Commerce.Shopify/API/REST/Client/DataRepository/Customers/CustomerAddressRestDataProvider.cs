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
using System.Threading;

namespace PX.Commerce.Shopify.API.REST
{
	public class CustomerAddressRestDataProviderFactory : ISPRestDataProviderFactory<IChildRestDataProvider<CustomerAddressData>>
	{
		public virtual IChildRestDataProvider<CustomerAddressData> CreateInstance(IShopifyRestClient restClient) => new CustomerAddressRestDataProvider(restClient);
	}

	public class CustomerAddressRestDataProvider : RestDataProviderBase, IChildRestDataProvider<CustomerAddressData>
	{
		protected override string GetListUrl { get; } = "customers/{parent_id}/addresses.json";
		protected override string GetSingleUrl { get; } = "customers/{parent_id}/addresses/{id}.json";
		protected override string GetSearchUrl => throw new NotImplementedException();

		public CustomerAddressRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual CustomerAddressData Create(CustomerAddressData entity, string customerId)
		{
			var segments = MakeParentUrlSegments(customerId);
			return Create<CustomerAddressData, CustomerAddressResponse>(entity, segments);
		}

		public virtual CustomerAddressData Update(CustomerAddressData entity, string customerId, string addressId)
		{
			var segments = MakeUrlSegments(addressId, customerId);
			return Update<CustomerAddressData, CustomerAddressResponse>(entity, segments);
		}

		public virtual bool Delete(string customerId, string addressId)
		{
			var segments = MakeUrlSegments(addressId, customerId);
			return Delete(segments);
		}

		public virtual IEnumerable<CustomerAddressData> GetAll(string customerId, IFilter filter = null, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(customerId);
			return GetAll<CustomerAddressData, CustomerAddressesResponse>(filter, segments, cancellationToken);
		}

		public virtual CustomerAddressData GetByID(string customerId, string addressId)
		{
			var segments = MakeUrlSegments(addressId, customerId);
			return GetByID<CustomerAddressData, CustomerAddressResponse>(segments);
		}

		public virtual IEnumerable<CustomerAddressData> GetAllWithoutParent(IFilter filter = null)
		{
			throw new NotImplementedException();
		}
	}
}
