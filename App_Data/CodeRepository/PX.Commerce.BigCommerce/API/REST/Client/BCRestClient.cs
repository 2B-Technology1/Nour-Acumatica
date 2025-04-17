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

using PX.Common;
using PX.Commerce.Core;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Net;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class BCRestClient : BCRestClientBase, IBigCommerceRestClient
	{
		private ISerializer _serializer;
		public BCRestClient(IRestSerializer serializer, IRestOptions options, Serilog.ILogger logger) : base(serializer, options, logger)
		{
			_serializer = serializer.Serializer;
		}

		#region API version 2
		public T Post<T>(RestRequest request, T obj)
			where T : class, new()
		{
			request.Method = Method.Post;
			request.AddBody(obj);
			var response = Execute<T>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new Exception($"Cannot insert {obj.GetType().Name}");
			}

			throw new RestException(response);
		}

		public T Put<T>(RestRequest request, T obj)
			where T : class, new()
		{
			request.Method = Method.Put;
			request.AddBody(obj);

			var response = Execute<T>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new Exception($"Cannot update {obj.GetType().Name}");
			}

			throw new RestException(response);
		}

		public bool Delete(RestRequest request)
		{
			request.Method = Method.Delete;
			var response = Execute(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				return true;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public T Get<T>(RestRequest request) 
			where T : class, new()
		{
			request.Method = Method.Get;
			var response = Execute<T>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;
				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(Options.BaseUrl, request, response);

			if (response.StatusCode == HttpStatusCode.InternalServerError && string.IsNullOrEmpty(response.Content))
			{
				throw new Exception(BigCommerceMessages.InternalServerError);
			}
			throw new RestException(response);
		}
		#endregion

		#region API version 3
		public TE Post<T, TE>(RestRequest request, T entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.Post;
			request.AddJsonBody(_serializer.Serialize(entity));
			RestResponse<TE> response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Post<T, TE>(RestRequest request, List<T> entities)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.Post;
			request.AddJsonBody(_serializer.Serialize(entities));
			RestResponse<TE> response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is IEnumerable<BCAPIEntity>) (result?.Data as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Post<T, TE>(RestRequest request, TE entity) 
			where T : class, new() 
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.Post;
			request.AddJsonBody(_serializer.Serialize(entity.Data));
			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE Put<T, TE>(RestRequest request, T entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.Put;
			request.AddJsonBody(_serializer.Serialize(entity));

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Put<T, TE>(RestRequest request, List<T> entities)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.Put;
			request.AddJsonBody(_serializer.Serialize(entities));

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is IEnumerable<BCAPIEntity>) (result?.Data as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Put<T, TE>(RestRequest request, TE entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.Put;
			request.AddJsonBody(_serializer.Serialize(entity));

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE PutList<T, TE>(RestRequest request, TE entity)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.Put;
			request.AddJsonBody(_serializer.Serialize(entity.Data));

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE Get<T, TE>(RestRequest request)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.Get;
			var response = Execute<TE>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE GetList<T, TE>(RestRequest request) 
			where T : class, new() 
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.Get;
			var response = Execute<TE>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(Options.BaseUrl, request, response);
			throw new RestException(response);
		}
		#endregion
	}
}
