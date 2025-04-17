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
using PX.Commerce.Core;
using RestSharp;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IBigCommerceRestClient
    {
        T Post<T>(RestRequest request, T obj) where T : class, new();
        T Put<T>(RestRequest request, T obj) where T : class, new();
        bool Delete(RestRequest request);
        T Get<T>(RestRequest request) where T : class, new();   
		
        RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null);

		TE Post<T, TE>(RestRequest request, T entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Post<T, TE>(RestRequest request, TE entity) where T : class, new() where TE: IEntityResponse<T>, new();
		TE Post<T, TE>(RestRequest request, List<T> entities) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE Put<T, TE>(RestRequest request, T entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Put<T, TE>(RestRequest request, TE entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Put<T, TE>(RestRequest request, List<T> entities) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE PutList<T, TE>(RestRequest request, TE entity) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE Get<T, TE>(RestRequest request) where T : class, new() where TE: IEntityResponse<T>, new();
		TE GetList<T, TE>(RestRequest request) where T : class, new() where TE : IEntitiesResponse<T>, new();
		Serilog.ILogger Logger { set; get; }
	}
}
