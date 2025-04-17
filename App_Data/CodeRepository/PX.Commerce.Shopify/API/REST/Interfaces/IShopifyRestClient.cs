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
using System.Threading.Tasks;
using PX.Commerce.Core;
using RestSharp;

namespace PX.Commerce.Shopify.API.REST
{
	public interface IShopifyRestClient
	{
		/// <summary>
		/// Post data to Shopify
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="request"></param>
		/// <param name="obj">The data obj that posts to Shopify API, it should be a T object</param>
		/// <param name="usingTRasBodyObj">True : system will auto convert T data obj to TR response obj first, and then posts to Shopify API; False : system will post T data obj to Shopify API directly. Please follow the Shopify API documents to determine this value, default is true.</param>
		/// <returns>The response data from Shopify API, it is a TR object</returns>
		T Post<T, TR>(RestRequest request, T obj, bool usingTRasBodyObj = true) where T : class, new() where TR : class, IEntityResponse<T>, new();

		/// <summary>
		/// Update data to Shopify
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="request"></param>
		/// <param name="obj">The data obj that udpates to Shopify API, it should be a T object</param>
		/// <param name="usingTRasBodyObj">True : system will auto convert T data obj to TR response obj first, and then posts to Shopify API; False : system will post T data obj to Shopify API directly. Please follow the Shopify API documents to determine this value, default is true.</param>
		/// <returns>The response data from Shopify API, it is a TR object</returns>
		T Put<T, TR>(RestRequest request, T obj, bool usingTRasBodyObj = true) where T : class, new() where TR : class, IEntityResponse<T>, new();
		bool Delete(RestRequest request);
		T Get<T, TR>(RestRequest request) where T : class, new() where TR : class, IEntityResponse<T>, new();
		T Get<T, TR>(RestRequest request, out IList<HeaderParameter> responseHeader) where T : class, new() where TR : class, IEntityResponse<T>, new();
		IEnumerable<T> GetCurrentList<T, TR>(RestRequest request, out string previousList, out string nextList) where T : class, new() where TR : class, IEntitiesResponse<T>, new();
		IEnumerable<T> GetAll<T, TR>(RestRequest request, CancellationToken cancellationToken = default) where T : class, new() where TR : class, IEntitiesResponse<T>, new();
		RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null);
		bool Post(RestRequest request);
		Serilog.ILogger Logger { set; get; }
	}
}
