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
	public class StoreCurrencyDataProviderFactory : IBCRestDataProviderFactory<IRestDataReader<List<Currency>>>
	{
		public virtual IRestDataReader<List<Currency>> CreateInstance(IBigCommerceRestClient restClient) => new StoreCurrencyDataProvider(restClient);
	}

	public class StoreCurrencyDataProvider : IRestDataReader<List<Currency>>
	{
		private readonly IBigCommerceRestClient _restClient;

		public StoreCurrencyDataProvider(IBigCommerceRestClient restClient)
		{
			_restClient = restClient;
		}

		public virtual List<Currency> Get()
		{
			const string resourceUrl = "v2/currencies";
			var request = _restClient.MakeRequest(resourceUrl);
			var store = _restClient.Get<List<Currency>>(request);
			return store;
		}
	}
}
