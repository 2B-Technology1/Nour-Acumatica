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

namespace PX.Commerce.BigCommerce.API.REST
{
    public class WebHookRestDataProvider : RestDataProviderV2, IParentRestDataProvider<WebHookData>
    {
        protected override string GetListUrl { get; } = "v2/hooks";
        protected override string GetSingleUrl { get; } = "v2/hooks/{id}";

        public WebHookRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region IParentDataRestClient
        public virtual WebHookData Create(WebHookData webhook)
        {
            var newwebhook = base.Create(webhook);
            return newwebhook;
        }

		public virtual WebHookData Update(WebHookData customer, string id)
        {
			throw new NotImplementedException();
		}

		public virtual bool Delete(string id)
		{
			var segments = MakeUrlSegments(id);
			return Delete(segments);
		}

		public virtual bool Delete(string id, WebHookData order)
        {
            return Delete(id);
        }

		public virtual IEnumerable<WebHookData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
        {
			return base.GetAll<WebHookData>(filter, cancellationToken: cancellationToken);
        }

		public virtual WebHookData GetByID(string webhookId)
        {
            var segments = MakeUrlSegments(webhookId);
            var customer = GetByID<WebHookData>(segments);
            return customer;
        }
		#endregion
	}
}
