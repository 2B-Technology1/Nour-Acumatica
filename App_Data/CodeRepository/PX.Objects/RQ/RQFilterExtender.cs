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
using System.Text;
using PX.Data;

namespace PX.Objects.RQ
{
	public static class RQViewExtender
	{
		public static void WhereAndCurrent<Filter>(this PXView view, params string[] excluded)
			where Filter : IBqlTable
		{
			view.WhereAnd(WhereByType(typeof(Filter), view.Graph, view.BqlSelect, excluded));
		}

		public static void WhereAndCurrent<Filter>(this PXSelectBase select, params string[] excluded)
			where Filter : IBqlTable
		{
			select.View.WhereAndCurrent<Filter>(excluded);
		}

		private static Type WhereByType(Type Filter, PXGraph graph, BqlCommand command, string[] excluded)			
		{
			PXCache filter = graph.Caches[Filter];			

			Type where = typeof(Where<BQLConstants.BitOn, Equal<BQLConstants.BitOn>>);
			foreach (string field in filter.Fields.Where(field => (excluded == null || !excluded.Any(e => String.Compare(e, field, true) == 0)) && !filter.Fields.Contains(field + "Wildcard")))
			{
				foreach (Type table in command.GetTables())
				{
					PXCache cache = graph.Caches[table];
					bool find = false;
					if (cache.Fields.Contains(field))
					{
						Type sourceType = filter.GetBqlField(field);
						Type destinationType = cache.GetBqlField(field);
						if (sourceType != null && destinationType != null)
						{
							where = BqlCommand.Compose(
								typeof (Where2<,>),
								typeof (Where<,,>),
								typeof (Current<>), sourceType, typeof (IsNull),
								typeof (Or<,>), destinationType, typeof (Equal<>), typeof (Current<>), sourceType,
								typeof (And<>), where
								);
							find = true;
						}
					}
					string f;
					if (field.Length > 8 && field.EndsWith("Wildcard") &&
					    cache.Fields.Contains(f = field.Substring(0, field.Length - 8)))
					{
						Type like = filter.GetBqlField(field);
						Type dest = cache.GetBqlField(f);
						where = BqlCommand.Compose(
							typeof (Where2<,>),
							typeof (Where<,,>), typeof (Current<>), like, typeof (IsNull),
							typeof (Or<,>), dest, typeof (Like<>), typeof (Current<>), like,
							typeof (And<>), where
							);
						find = true;
					}
					if (find) break;
				}
			}
			return where;
		}
	}
}
