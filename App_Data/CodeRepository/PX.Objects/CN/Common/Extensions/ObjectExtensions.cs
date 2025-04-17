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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Extensions
{
	public static class ObjectExtensions
	{
		public static T[] CreateArray<T>(this T item)
		{
			return new[]
			{
				item
			};
		}

		public static T SingleOrNull<T>(this IEnumerable<T> enumerable)
		{
			var list = enumerable.ToList();
			return list.IsSingleElement()
				? list.Single()
				: default(dynamic);
		}

		public static T GetPropertyValue<T>(this object entity, string propertyName)
		{
			var value = entity.GetType().GetProperty(propertyName)
				?.GetValue(entity, null);
			return value == null
				? default
				: (T)value;
		}

		public static T Cast<T>(this object entity)
		{
			var serializeObject = JsonConvert.SerializeObject(entity);
			return JsonConvert.DeserializeObject<T>(serializeObject);
		}

		public static dynamic Cast(this object entity, Type resultType)
		{
			var serializeObject = JsonConvert.SerializeObject(entity);
			return JsonConvert.DeserializeObject(serializeObject, resultType);
		}
	}
}