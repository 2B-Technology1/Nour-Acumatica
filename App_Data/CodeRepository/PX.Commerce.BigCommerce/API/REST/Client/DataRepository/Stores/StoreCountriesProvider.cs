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
	public class StoreCountriesProviderFactory : IBCRestDataProviderFactory<IRestDataReader<List<Countries>>>
	{
		public virtual IRestDataReader<List<Countries>> CreateInstance(IBigCommerceRestClient restClient) => new StoreCountriesProvider(restClient);
	}

	public class StoreCountriesProvider : IRestDataReader<List<Countries>>
    {
        private readonly IBigCommerceRestClient _restClient;

        public StoreCountriesProvider(IBigCommerceRestClient restClient)
        {
            _restClient = restClient;
        }

		public virtual List<Countries> Get()
        {
            const string resourceUrl = "v2/countries";
          
			var filter =  new Filter();
			var needGet = true;

			filter.Page = 1;
			filter.Limit = 250;
			
			List<Countries> Countries = new List<Countries>();
			while (needGet)
			{
				var request = _restClient.MakeRequest(resourceUrl);
				filter?.AddFilter(request);
				var country = _restClient.Get<List<Countries>>(request);
				Countries.AddRange(country);
				if(country.Count<250) needGet = false;
				filter.Page++;
			}
			
            return Countries;
        }
	}
}
