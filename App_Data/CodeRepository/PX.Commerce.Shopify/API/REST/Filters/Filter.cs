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
using System.Linq;
using System.Runtime.Serialization;
using RestSharp;
using RestSharp.Extensions;

namespace PX.Commerce.Shopify.API.REST
{
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public abstract class FilterBase : IFilter
    {
        protected const string ISO_8601_DATE_FORMAT = "{0:yyyy-MM-ddTHH:mm:sszzz}";

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
                        value = string.Format(ISO_8601_DATE_FORMAT, value);
                    }
					else if(value is Enum)
					{
						var memInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();
						var memAttr = memInfo?.GetAttribute<EnumMemberAttribute>();
						if(memAttr != null)
						{
							value = memAttr.Value;
						}
					}
					request.AddParameter(key, value, ParameterType.GetOrPost);
				}
            }            
        }
    }

	public class FilterWithFields : FilterBase, IFilterWithFields
	{
		/// <summary>
		/// Show only certain fields, specified by a comma-separated list of field names.
		/// </summary>
		[Description("fields")]
		public string Fields { get; set; }
	}

	public class FilterWithDateTimeAndLimit : FilterBase, IFilterWithDateTime, IFilterWithLimit
	{
		/// <summary>
		/// The maximum number of results to show.
		/// (default: 50, maximum: 250)
		/// </summary>
		[Description("limit")]
		public int? Limit { get; set; }
		/// <summary>
		/// Show customers created after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_min")]
		public DateTime? CreatedAtMin { get; set; }

		/// <summary>
		/// Show customers created before a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("created_at_max")]
		public DateTime? CreatedAtMax { get; set; }

		/// <summary>
		/// Show customers last updated after a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("updated_at_min")]
		public DateTime? UpdatedAtMin { get; set; }

		/// <summary>
		/// Show customers last updated before a specified date.
		///(format: 2014-04-25T16:15:47-04:00)
		/// </summary>
		[Description("updated_at_max")]
		public DateTime? UpdatedAtMax { get; set; }
	}

}
