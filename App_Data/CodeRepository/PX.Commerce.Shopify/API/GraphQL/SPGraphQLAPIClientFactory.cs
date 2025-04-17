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
using System.Collections.Concurrent;
using System.Net.Http;
using Newtonsoft.Json;
using PX.Common;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// Factory for Shopify GraphQL API Clients.
	/// </summary>
	public class SPGraphQLAPIClientFactory : ISPGraphQLAPIClientFactory
	{
		private static readonly string APIVersion = SPHelper.GetAPIDefaultVersion();

		/// <summary>
		/// Returns a Shopify GraphQL API client for the specified store.
		/// </summary>
		/// <param name="binding">The binding of the store to return the binding for.</param>
		/// <param name="apiVersion">The specified API version</param>
		/// <returns>The GraphQL API Client for the specified store.</returns>
		public SPGraphQLAPIClient GetClient(BCBindingShopify binding, string apiVersion = null)
		{
			if(binding == null) throw new ArgumentNullException(nameof(binding));

			var endpoint = string.Format(ShopifyGraphQLConstants.Endpoint, binding.ShopifyApiBaseUrl, apiVersion ?? APIVersion);

			return new SPGraphQLAPIClient(endpoint, binding.ShopifyAccessToken, new JsonSerializerSettings()
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
				Formatting = Formatting.Indented,
				ContractResolver = new Core.REST.GetOnlyContractResolver(),
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
		}
	}
}
