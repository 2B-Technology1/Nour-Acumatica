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

using Newtonsoft.Json;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PX.Commerce.Shopify.API.REST
{
	public class CustomerRestDataProviderFactory : ISPRestDataProviderFactory<ICustomerRestDataProvider<CustomerData>>
	{
		public virtual ICustomerRestDataProvider<CustomerData> CreateInstance(IShopifyRestClient restClient) => new CustomerRestDataProvider(restClient);
	}

	public class CustomerRestDataProvider : RestDataProviderBase, ICustomerRestDataProvider<CustomerData>
	{
		protected override string GetListUrl { get; } = "customers.json";
		protected override string GetSingleUrl { get; } = "customers/{id}.json";
		protected override string GetSearchUrl { get; } = "customers/search.json";
		private string GetAccountActivationUrl { get; } = "customers/{id}/account_activation_url.json";
		private string GetSendInviteUrl { get; } = "customers/{id}/send_invite.json";
		private string GetMetafieldsUrl { get; } = "customers/{id}/metafields.json";
		private string GetAddressesUrl { get; } = "customers/{id}/addresses.json";

		public CustomerRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual CustomerData Create(CustomerData entity)
		{
			return base.Create<CustomerData, CustomerResponse>(entity);
		}

		public virtual CustomerData Update(CustomerData entity) => Update(entity, entity.Id.ToString());
		public virtual CustomerData Update(CustomerData entity, string customerId)
		{
			var segments = MakeUrlSegments(customerId);
			return base.Update<CustomerData, CustomerResponse>(entity, segments);
		}

		public virtual bool Delete(CustomerData entity, string customerId) => Delete(customerId);

		public virtual bool Delete(string customerId)
		{
			var segments = MakeUrlSegments(customerId);
			return Delete(segments);
		}

		public virtual IEnumerable<CustomerData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll<CustomerData, CustomersResponse>(filter, cancellationToken: cancellationToken);
		}

		public virtual CustomerData GetByID(string id) => GetByID(id, true, false);

		public CustomerData GetByID(string customerId, bool includedMetafields = true, bool includeAllAddresses = false, CancellationToken cancellationToken = default)
		{
			var segments = MakeUrlSegments(customerId);
			var entity = base.GetByID<CustomerData, CustomerResponse>(segments);
			if (entity != null && includedMetafields == true)
			{
				entity.Metafields = GetMetafieldsById(customerId, cancellationToken);
			}
			if (entity != null && includeAllAddresses == true)
			{
				entity.Addresses = GetAddressesById(customerId, cancellationToken);
			}
			return entity;
		}

		public virtual IEnumerable<CustomerData> GetByQuery(string fieldName, string value, bool includedMetafields = false, CancellationToken cancellationToken = default)
		{
			var url = GetSearchUrl;
			var property = typeof(CustomerData).GetProperty(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
			if (property != null)
			{
				var attr = property.GetAttribute<JsonPropertyAttribute>();
				if (attr == null) throw new KeyNotFoundException();
				String key = attr.PropertyName;
				url += $"?query={attr.PropertyName}:{value}";
			}
			else
				throw new KeyNotFoundException();
			var request = BuildRequest(url, nameof(this.GetByQuery), null, null);
			foreach (var result in ShopifyRestClient.GetAll<CustomerData, CustomersResponse>(request, cancellationToken))
			{
				if (includedMetafields == true && result != null)
				{
					result.Metafields = GetMetafieldsById(result.Id.ToString(), cancellationToken);
				}
				yield return result;

			}
		}

		public virtual bool ActivateAccount(string customerId)
		{
			var request = BuildRequest(GetAccountActivationUrl, nameof(this.ActivateAccount), MakeUrlSegments(customerId), null);
			return ShopifyRestClient.Post(request);
		}

		public virtual List<MetafieldData> GetMetafieldsById(string id, CancellationToken cancellationToken = default)
		{
			var request = BuildRequest(GetMetafieldsUrl, nameof(GetMetafieldsById), MakeUrlSegments(id), null);
			return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request, cancellationToken ).ToList();
		}


		public List<CustomerAddressData> GetAddressesById(string id, CancellationToken cancellationToken = default)
		{
			var segments = MakeUrlSegments(id);
			var request = BuildRequest(GetAddressesUrl, nameof(GetAddressesById), segments, null);
			return ShopifyRestClient.GetAll<CustomerAddressData, CustomerAddressesResponse>(request, cancellationToken).ToList();
		}
	}
}
