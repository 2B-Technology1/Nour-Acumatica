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

using PX.Data;
using System;

namespace PX.Objects.EP
{
	public static class CacheHelper
	{
		public static object GetValue<Field>(PXGraph graph, object data)
			where Field : IBqlField
		{
			return GetValue(graph, data, typeof(Field));
		}

		public static object GetValue(PXGraph graph, object data, Type field)
		{
			return graph.Caches[BqlCommand.GetItemType(field)].GetValue(data, field.Name);
		}

		public static object GetCurrentValue(PXGraph graph, Type type)
		{
			PXCache cache = graph.Caches[BqlCommand.GetItemType(type)];
			return cache?.GetValue(cache.Current, type.Name);
		}

		public static object GetCurrentRecord(PXGraph graph, Type fieldType)
		{
			PXCache cache = graph.Caches[fieldType];
			return cache?.Current;
		}
	}
}
