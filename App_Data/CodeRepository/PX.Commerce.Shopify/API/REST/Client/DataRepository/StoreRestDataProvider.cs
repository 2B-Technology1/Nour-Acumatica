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

namespace PX.Commerce.Shopify.API.REST
{
	public class StoreRestDataProviderFactory : ISPRestDataProviderFactory<IStoreRestDataProvider>
	{
		public virtual IStoreRestDataProvider CreateInstance(IShopifyRestClient restClient) => new StoreRestDataProvider(restClient);
	}

	public class StoreRestDataProvider : RestDataProviderBase, IStoreRestDataProvider
	{
		public StoreRestDataProvider(IShopifyRestClient restClient)
		{
			ShopifyRestClient = restClient;
		}
		protected override string GetListUrl => throw new NotImplementedException();

		protected override string GetSingleUrl => "shop.json";


		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetShippingZoneUrl => "shipping_zones.json";

		public virtual StoreData Get()
		{
			var request = BuildRequest(GetSingleUrl, nameof(this.Get));
			StoreData data = ShopifyRestClient.Get<StoreData, StoreResponse>(request, out var headers);
			var responseTime = headers.FirstOrDefault(x => string.Equals(x.Name, "Date", StringComparison.OrdinalIgnoreCase));
			if (responseTime != null && responseTime.Value != null && DateTime.TryParse(responseTime.Value.ToString(), out var serverTime))
			{
				data.ResponseTime = serverTime;
			}

			var paraLimit = headers.FirstOrDefault(x => string.Equals(x.Name, "X-Shopify-Shop-Api-Call-Limit", StringComparison.OrdinalIgnoreCase));
			if (paraLimit != null && paraLimit.Value != null)
			{
				var numStr = paraLimit.Value.ToString().Split('/');
				if (numStr != null && numStr.Length == 2)
				{
					data.ApiCapacity = int.Parse(numStr[1]);
					data.ApiAvailable = data.ApiCapacity - int.Parse(numStr[0]);
				}
			}

			var apiVersion = headers.FirstOrDefault(x => string.Equals(x.Name, "X-Shopify-API-Version", StringComparison.OrdinalIgnoreCase));
			if (apiVersion != null && apiVersion.Value != null)
			{
				data.ApiVersion = apiVersion.Value.ToString();
			}
			return data;
		}

		public virtual IEnumerable<ShippingZoneData> GetShippingZones(CancellationToken cancellationToken = default)
		{
			var request = BuildRequest(GetShippingZoneUrl, nameof(GetShippingZones));
			return ShopifyRestClient.GetAll<ShippingZoneData, ShippingZonesResponse>(request, cancellationToken);
		}
	}
}
