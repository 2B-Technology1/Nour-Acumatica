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

using RestSharp;
using RestSharp.Authenticators;
using System.Linq;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class Autentificator : IAuthenticator
    {
        private readonly string _xAuthClient;
        private readonly string _xAuthTocken;

        public Autentificator(string xAuthClient, string xAuthTocken)
        {
            _xAuthClient = xAuthClient;
            _xAuthTocken = xAuthTocken;
        }

        public ValueTask Authenticate(RestClient client, RestRequest request)
        {
            if(!request.Parameters.Any(x => x.Name == "X-Auth-Client"))
                request.AddHeader("X-Auth-Client", _xAuthClient);
            if (!request.Parameters.Any(x => x.Name == "X-Auth-Token"))
                request.AddHeader("X-Auth-Token", _xAuthTocken);
			return new ValueTask();
        }
    }
}
