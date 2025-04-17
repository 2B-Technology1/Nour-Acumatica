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

namespace PX.Commerce.BigCommerce.API.REST
{
	public class StoreStatesProviderFactory : IBCRestDataProviderFactory<IRestDataReader<List<States>>>
	{
		public virtual IRestDataReader<List<States>> CreateInstance(IBigCommerceRestClient restClient) => new StoreStatesProvider(restClient);
	}

	public class StoreStatesProvider : IRestDataReader<List<States>>
    {
        private readonly IBigCommerceRestClient _restClient;

        public StoreStatesProvider(IBigCommerceRestClient restClient)
        {
            _restClient = restClient;
        }


		public virtual List<States> Get()
        {
            const string resourceUrl = "v2/countries/states";
          
			var filter =  new Filter();
			var needGet = true;

			filter.Page = 1;
			filter.Limit = 250;
			
			List<States> States = new List<States>();
			while (needGet)
			{
				var request = _restClient.MakeRequest(resourceUrl);
				filter?.AddFilter(request);
				var state = _restClient.Get<List<States>>(request);
				States.AddRange(state);
				if(state.Count<250) needGet = false;
				filter.Page++;
			}
			
            return States;
        }
	}
}
