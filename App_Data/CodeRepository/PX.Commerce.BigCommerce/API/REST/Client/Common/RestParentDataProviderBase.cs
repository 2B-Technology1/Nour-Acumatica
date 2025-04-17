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

using PX.Commerce.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using static PX.Data.BQL.BqlPlaceholder;

namespace PX.Commerce.BigCommerce.API.REST
{
	public abstract class RestDataProviderBase
	{		
		protected const string ID_STRING = "id";
		protected const string PARENT_ID_STRING = "parent_id";
		protected const string OTHER_PARAM = "other_param";
		protected IBigCommerceRestClient _restClient;
	
		protected abstract string GetListUrl { get; }
		protected abstract string GetSingleUrl { get; }

		public RestDataProviderBase()
		{
		}

		public virtual T Create<T>(T entity, UrlSegments urlSegments = null)
			where T : class, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Creating new {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());

			return _restClient.Post(request, entity);
		}
		public virtual TE Create<T, TE>(T entity, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Creating new {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());

			TE result = _restClient.Post<T, TE>(request, entity);

			return result;

		}
		public virtual TE Create<T, TE>(List<T> entities, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Creating new {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entities);

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());

			TE result = _restClient.Post<T, TE>(request, entities);

			return result;
		}

		public virtual T Update<T>(T entity, UrlSegments urlSegments)
			where T : class, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Updating {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());

			T result = _restClient.Put(request, entity);

			return result;
		}

		public virtual TE Update<T, TE>(T entity, UrlSegments urlSegments)
			where T : class, new()
			where TE : class, IEntityResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Updating {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments?.GetUrlSegments());

			TE result = _restClient.Put<T, TE>(request, entity);

			return result;
		}
		public virtual TE Update<T, TE>(List<T> entities, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : class, IEntitiesResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Updating {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entities);

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments?.GetUrlSegments());

			return _restClient.Put<T, TE>(request, entities);
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual bool Delete(UrlSegments urlSegments)
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Deleting {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, this.GetType().ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()));

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());

			var result = _restClient.Delete(request);

			return result;
		}

		protected static UrlSegments MakeUrlSegments(string id)
		{
			var segments = new UrlSegments();
			segments.Add(ID_STRING, id);
			return segments;
		}

		protected static UrlSegments MakeParentUrlSegments(string parentId)
		{
			var segments = new UrlSegments();
			segments.Add(PARENT_ID_STRING, parentId);

			return segments;
		}


		protected static UrlSegments MakeUrlSegments(string id, string parentId)
		{
			var segments = new UrlSegments();
			segments.Add(PARENT_ID_STRING, parentId);
			segments.Add(ID_STRING, id);
			return segments;
		}
		protected static UrlSegments MakeUrlSegments(string id, string parentId, string param)
		{
			var segments = new UrlSegments();
			segments.Add(PARENT_ID_STRING, parentId);
			segments.Add(ID_STRING, id);
			segments.Add(OTHER_PARAM, param);
			return segments;
		}
	}

	public abstract class RestDataProviderV2 : RestDataProviderBase
	{
		public RestDataProviderV2() : base()
		{

		}

		protected virtual List<T> Get<T>(IFilter filter = null, UrlSegments urlSegments = null)
			where T : class, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Getting {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, this.GetType().ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()));

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());
			filter?.AddFilter(request);

			var entity = _restClient.Get<List<T>>(request);
			return entity;
		}

		public virtual IEnumerable<T> GetAll<T>(IFilter filter = null, UrlSegments urlSegments = null, CancellationToken cancellationToken = default)
			where T : class, new()
		{
			var localFilter = filter ?? new Filter();
			var needGet = true;

			localFilter.Page = 1;
			localFilter.Limit = 50;

			while (needGet)
			{
				cancellationToken.ThrowIfCancellationRequested();

				List<T> entities = Get<T>(localFilter, urlSegments);

				if (entities == null) yield break;
				foreach (T entity in entities)
				{
					yield return entity;
				}
				localFilter.Page++;
				needGet = localFilter.Limit == entities.Count;
			}
		}

		public virtual T GetByID<T>(UrlSegments urlSegments) where T : class, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Getting by ID {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, this.GetType().ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()));

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());

			var entity = _restClient.Get<T>(request);

			return entity;
		}
	}	

	public abstract class RestDataProviderV3 : RestDataProviderBase
	{
		protected const int DEFAULT_BATCH_SIZE = 10;
		protected virtual int BatchSize
		{
			get { return DEFAULT_BATCH_SIZE; }
		}
		public RestDataProviderV3() : base()
		{

		}

		protected virtual TE Get<T, TE>(IFilter filter = null, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Getting {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, this.GetType().ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()));

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());
			filter?.AddFilter(request);

			var response = _restClient.GetList<T, TE>(request);
			return response;
		}

		public virtual IEnumerable<T> GetAll<T, TE>(IFilter filter = null, UrlSegments urlSegments = null, CancellationToken cancellationToken = default)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			var localFilter = filter ?? new Filter();
			var needGet = true;

			localFilter.Page = 1;
			localFilter.Limit = 50;
			TE entity = default;
			while (needGet)
			{
				cancellationToken.ThrowIfCancellationRequested();

				entity = Get<T, TE>(localFilter, urlSegments);

				if (entity?.Data == null) yield break;
				foreach (T data in entity.Data)
				{
					yield return data;
				}

				needGet = localFilter.Page < entity?.Meta?.Pagination.TotalPages;
				localFilter.Page++;
			}
		}

		public virtual TE GetByID<T, TE>(UrlSegments urlSegments, IFilter filter = null)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Getting by ID {1} entity with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()));

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());
			if (filter != null)
				filter.AddFilter(request);

			TE result = _restClient.Get<T, TE>(request);

			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Returned By ID", BCCaptions.CommerceLogCaption),
				new BCLogTypeScope(GetType()), result);

			return result;
		}

		public virtual TE Create<T, TE>(TE entity, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger, 
				string.Format("{0}: BigCommerce REST API - Creating of ID {1} entry with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetListUrl, urlSegments?.GetUrlSegments());

			TE result = _restClient.Post<T, TE>(request, entity);

			return result;
		}

		public virtual TE Update<T, TE>(TE entity, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Updating {1} entry with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entity);

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());

			TE result = _restClient.Put<T, TE>(request, entity);

			return result;
		}

		public virtual TE UpdateAll<T, TE>(TE entities, UrlSegments urlSegments = null)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Updating {1} entry with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), entities);

			var request = _restClient.MakeRequest(GetSingleUrl, urlSegments.GetUrlSegments());

			return _restClient.PutList<T, TE>(request, entities);
		}
		public virtual void UpdateAll<T, TE>(TE entities, UrlSegments urlSegments, Action<ItemProcessCallback<T>> callback)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			TE batch = new TE();
			batch.Meta = entities.Meta;

			int index = 0;
			for (; index < entities.Data.Count; index++)
			{
				if (index % BatchSize == 0 && batch.Data.Count > 0)
				{
					UpdateBatch<T, TE>(batch, urlSegments, index - batch.Data.Count, callback);

					batch.Data.Clear();
				}
				batch.Data.Add(entities.Data[index]);
			}
			if (batch.Data.Count > 0)
			{
				UpdateBatch<T, TE>(batch, urlSegments, index - batch.Data.Count, callback);
			}
		}

		protected void UpdateBatch<T, TE>(TE batch, UrlSegments urlSegments, Int32 startIndex, Action<ItemProcessCallback<T>> callback)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			APIHelper.LogIntoProfiler(_restClient.Logger,
				string.Format("{0}: BigCommerce REST API - Batch Updating of {1} entry with parameters {2}", BCCaptions.CommerceLogCaption, typeof(T).ToString(), urlSegments?.ToString() ?? "none"),
				new BCLogTypeScope(GetType()), batch);

			while (true)
				try
				{
					RestRequest request = _restClient.MakeRequest(GetListUrl, urlSegments.GetUrlSegments());

					TE response = _restClient.PutList<T, TE>(request, batch);
					if (response == null) return;
					for (int i = 0; i < response.Data.Count; i++)
					{
						T item = response.Data[i];
						callback(new ItemProcessCallback<T>(startIndex + i, item));
					}
					break;
				}
				catch (RestException ex)
				{
					if (ex?.ResponceStatusCode == default(HttpStatusCode).ToString())
					{
						throw;
					}
					else
					{
						for (int i = 0; i < batch.Data.Count; i++)
						{
							callback(new ItemProcessCallback<T>(startIndex + i, ex, batch.Data));
						}
						break;
					}
				}
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual bool Delete(IFilter filter = null)
		{
			var request = _restClient.MakeRequest(GetSingleUrl);
			filter?.AddFilter(request);

			var response = _restClient.Delete(request);
			return response;
		}
	}
}
