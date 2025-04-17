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
using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.FA
{
	[Obsolete(Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
	public class CanBeDepreciated<TFieldDepreciated, TFieldUnderConstruction> : IBqlWhere
			where TFieldDepreciated : PX.Data.BQL.BqlBool.Field<TFieldDepreciated>
			where TFieldUnderConstruction : PX.Data.BQL.BqlBool.Field<TFieldUnderConstruction>
	{
		private readonly IBqlCreator whereEqualNotNull;

		private readonly Type cacheType;

		public CanBeDepreciated()
		{
			cacheType = BqlCommand.GetItemType(typeof(TFieldDepreciated));
			whereEqualNotNull = new Where<TFieldDepreciated, Equal<True>, And<TFieldUnderConstruction, NotEqual<True>>>();

			Type cacheTypeUnderConstruction = BqlCommand.GetItemType(typeof(TFieldUnderConstruction));
			if (cacheType != cacheTypeUnderConstruction)
			{
				throw new PXArgumentException();
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return whereEqualNotNull.AppendExpression(ref exp, graph, info, selection);
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			object row = item;

			if (!cacheType.IsAssignableFrom(item.GetType()))
			{
				PXCache parentcache = cache.Graph.Caches[cacheType];
				row = parentcache.Current;
			}

			whereEqualNotNull.Verify(cache, row, pars, ref result, ref value);
		}
	}
}
