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
using System.Text;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class UrlSegments
    {
        private readonly Dictionary<string, string> _urlSegments = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            _urlSegments.Add(key, value);
        }

        public void Delete(string key)
        {
            if (_urlSegments.ContainsKey(key))
            {
                _urlSegments.Remove(key);
            }
        }

        public  Dictionary<string, string> GetUrlSegments()
        {
            return _urlSegments;
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(KeyValuePair<string, string> pair in _urlSegments)
			{
				sb.Append(pair.Key);
				sb.Append("=");
				sb.Append(pair.Value);
				sb.Append(";");
			}
			return sb.ToString();
		}
	}
}
