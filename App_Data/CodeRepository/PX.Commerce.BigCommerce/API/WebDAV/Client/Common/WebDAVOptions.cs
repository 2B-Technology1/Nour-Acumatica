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

namespace PX.Commerce.BigCommerce.API.WebDAV
{
    public interface IWebDAVOptions
    {
        string ServerHttpsUri { get; set; }
        string ClientUser { get; set; }
        string ClientPassword { get; set; }
    }

    public class WebDAVOptions : IWebDAVOptions
    {
        public string ServerHttpsUri { get; set; }
        public string ClientUser { get; set; }
        public string ClientPassword { get; set; }

        public override string ToString()
        {
            return $"Server Https Url: {ServerHttpsUri},{Environment.NewLine}" +
                   $"ClientUser: {ClientUser},{Environment.NewLine}" +
                   $"ClientPassword: {ClientPassword}";
        }
    }
}
