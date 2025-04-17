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

namespace PX.Objects.CR
{
	/// <summary>
	/// View extension that is used for overriding the sort order.
	/// </summary>
	/// <typeparam name="TBqlField">The field that is used for sorting with the highest priority (cannot be overridden)</typeparam>
	public class PXPredefinedOrderedView<TBqlField> : PXView
		where TBqlField : IBqlField
	{
		public PXPredefinedOrderedView(PXGraph graph, bool isReadOnly, BqlCommand select)
			: base(graph, isReadOnly, select)
		{
		}

		public PXPredefinedOrderedView(PXGraph graph, bool isReadOnly, BqlCommand select, Delegate handler)
			: base(graph, isReadOnly, select, handler)
		{
		}

		public bool IsCompare { get; set; }
		protected override int Compare(object a, object b, compareDelegate[] comparisons)
		{
			IsCompare = true;
			int compare = base.Compare(a, b, comparisons);
			IsCompare = false;
			return compare;
		}

		public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (searches != null && searches.Where(x => x != null).Any())
			{
				return base.Select(currents, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
			}

			List<object> newSearches = new List<object>();
			List<string> newSorts = new List<string>();
			List<bool> newDescs = new List<bool>();

			newSorts.Add(typeof(TBqlField).Name);
			newDescs.Add(false);
			newSearches.Add(null);

			if (searches != null)
			{
				newSearches.AddRange(searches);
			}
			if (sortcolumns != null)
			{
				newSorts.AddRange(sortcolumns);
			}
			if (descendings != null)
			{
				newDescs.AddRange(descendings);
			}

			return base.Select(currents, parameters, newSearches.ToArray(), newSorts.ToArray(), newDescs.ToArray(), filters, ref startRow, maximumRows, ref totalRows);
		}
	}
}
