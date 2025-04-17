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
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.Common.Extensions
{
	public static class CacheExtensions
	{
		public static object GetValue(this PXCache cache, object data, Type bqlField)
		{
			var fieldName = cache.GetField(bqlField);
			return cache.GetValue(data, fieldName);
		}

		public static void Enable(this PXCache cache, bool isEnabled)
		{
			cache.AllowUpdate = isEnabled;
			cache.AllowInsert = isEnabled;
			cache.AllowDelete = isEnabled;
		}

		public static IEnumerable<object> UpdateAll(this PXCache cache, IEnumerable<object> items)
		{
			items = items.ToList();
			foreach (var item in items)
			{
				cache.Update(item);
			}
			return items;
		}

		public static IEnumerable<object> InsertAll(this PXCache cache, IEnumerable<object> items)
		{
			items = items.ToList();
			foreach (var item in items)
			{
				cache.Insert(item);
			}
			return items;
		}

		public static IEnumerable<object> DeleteAll(this PXCache cache, IEnumerable<object> items)
		{
			items = items.ToList();
			foreach (var item in items)
			{
				cache.Delete(item);
			}
			return items;
		}

		public static void RaiseException<TField>(this PXCache cache, object row, string message,
			object newValue = null, PXErrorLevel errorLevel = PXErrorLevel.Error)
			where TField : IBqlField
		{
			var exception = new PXSetPropertyException(message, errorLevel);
			cache.RaiseExceptionHandling<TField>(row, newValue, exception);
		}

		public static void RaiseException(this PXCache cache, string fieldName, object row, string message,
			object newValue = null, PXErrorLevel errorLevel = PXErrorLevel.Error)
		{
			var exception = new PXSetPropertyException(message, errorLevel);
			cache.RaiseExceptionHandling(fieldName, row, newValue, exception);
		}

		public static void ClearFieldErrorIfExists<TField>(this PXCache cache, object row, string errorMessage)
			where TField : IBqlField
		{
			if (HasError<TField>(cache, row, errorMessage))
			{
				cache.ClearFieldErrors<TField>(row);
			}
		}

		public static bool HasError<TField>(this PXCache cache, object row, string errorMessage)
			where TField : IBqlField
		{
			var fieldName = cache.GetField(typeof(TField));
			return cache.GetAttributes(row, fieldName).OfType<IPXInterfaceField>()
				.Any(field => field.ErrorText == errorMessage);
		}

		public static bool GetEnabled<TField>(this PXCache cache, object data)
			where TField : IBqlField
		{
			return cache.GetAttributesOfType<PXUIFieldAttribute>(data, typeof(TField).Name)
				.Single(attribute => attribute != null).Enabled;
		}
	}
}