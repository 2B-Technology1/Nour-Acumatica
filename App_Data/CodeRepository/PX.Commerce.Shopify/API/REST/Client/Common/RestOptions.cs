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

using System;

namespace PX.Commerce.Shopify.API.REST
{
    public interface IRestOptions
    {
        string BaseUri { get; set; }
        string ApiKey { get; set; }
        string ApiToken { get; set; }
        string SharedSecret { get; set; }
		int ApiCallLimit { get; set; }
	}

    public class RestOptions : IRestOptions
    {
        public string BaseUri { get; set; }
        public string ApiKey { get; set; }
        public string ApiToken { get; set; }
        public string SharedSecret { get; set; }
		public int ApiCallLimit { get; set; }
    }
}
