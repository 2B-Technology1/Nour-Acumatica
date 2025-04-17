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
using System;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.Shopify.API.REST
{
    public class WebHookRestDataProvider : RestDataProviderBase, IParentRestDataProvider<WebHookData>
    {
        protected override string GetListUrl { get; } = "/webhooks.json";
        protected override string GetSingleUrl { get; } = "/webhooks/{id}.json";
		protected override string GetSearchUrl => throw new NotImplementedException();

		public WebHookRestDataProvider(IShopifyRestClient restClient) : base()
		{
            ShopifyRestClient = restClient;
		}

        #region IParentDataRestClient
        public virtual WebHookData Create(WebHookData entity)
        {
            var newwebhook = base.Create<WebHookData, WebHookResponse>(entity);
            return newwebhook;
        }

		public virtual WebHookData Update(WebHookData entity, string id)
        {
			var segments = MakeUrlSegments(id);
			return base.Update<WebHookData, WebHookResponse>(entity, segments);
		}

		public virtual bool Delete(string id)
        {
            var segments = MakeUrlSegments(id);
            return base.Delete(segments);
        }

		public virtual IEnumerable<WebHookData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
        {
            var allWebHooks = base.GetAll<WebHookData, WebHooksResponse>(filter, cancellationToken: cancellationToken);
            return allWebHooks;
        }

		public virtual WebHookData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            var webhook = GetByID<WebHookData, WebHookResponse>(segments);
            return webhook;
        }
		#endregion
	}
}
