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
using System.ComponentModel;
using RestSharp;
using RestSharp.Extensions;

namespace PX.Commerce.BigCommerce.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class Filter : IFilter
    {
        protected const string RFC2822_DATE_FORMAT = "{0:ddd, dd MMM yyyy HH:mm:ss} GMT";
        protected const string ISO_DATE_FORMAT = "{0:yyyy-MM-ddTHH:mm:ss}";        
        
        [Description("limit")]
        public int? Limit { get; set; }

        [Description("page")]
        public int? Page { get; set; }

        public virtual void AddFilter(RestRequest request)
        {
           foreach (var propertyInfo in GetType().GetProperties())
            {
				DescriptionAttribute attr = propertyInfo.GetAttribute<DescriptionAttribute>();
                if (attr == null) continue;
                String key = attr.Description;
                Object value = propertyInfo.GetValue(this);
                if (value != null)
                {
                    if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        value = string.Format(ISO_DATE_FORMAT, value);
                    }
					request.AddParameter(key, value, ParameterType.GetOrPost);
                }
            }            
        }
    }
    
}
