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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using static PX.Data.PXView;

namespace PX.Objects.Common
{
	public static class PXViewExtensions
	{
		public static IEnumerable SelectExternal(this PXView view, ref int startRow, ref int totalRows, object[] pars = null)
		{
			IEnumerable list = SelectWithExternalParameters(view, ref startRow, ref totalRows, pars);
			PXView.StartRow = 0;
			return list;
		}

		public static IEnumerable SelectExternal(this PXView view, object [] pars = null)
		{
			int startRow = 0;
			int totalRows = 0;

			IEnumerable list = SelectWithExternalParameters(view, ref startRow, ref totalRows, pars);
			return list;
		}

		public static IEnumerable SelectExternalWithPaging(this PXView view, object [] pars = null)
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;

			IEnumerable list = SelectWithExternalParameters(view, ref startRow, ref totalRows, pars);
			PXView.StartRow = 0;
			return list;
		}

		public static IEnumerable SelectWithinContext(this PXView view, object[] pars = null)
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;

			IEnumerable list = view.Select(
				PXView.Currents,
				pars ?? PXView.Parameters,
				PXView.Searches,
				PXView.SortColumns,
				PXView.Descendings,
				PXView.Filters,
				ref startRow,
				PXView.MaximumRows,
				ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		private static IEnumerable SelectWithExternalParameters(PXView view, ref int startRow, ref int totalRows, object [] pars = null)
		{
			IEnumerable list = view.Select(
				PXView.Currents,
				pars ?? PXView.Parameters,
				PXView.Searches,
				view.GetExternalSorts(),
				view.GetExternalDescendings(),
				view.GetExternalFilters(),
				ref startRow,
				PXView.MaximumRows,
				ref totalRows);
			return list;
		}

		public static List<PXSearchColumn> GetContextualExternalSearchColumns(this PXView view, IEnumerable<PXSearchColumn> contextualSorts = null)
		{
			string[] externalSortColumns = view.GetExternalSorts() ?? (new string[] { });
			bool[] externalDescendings = view.GetExternalDescendings() ?? (new bool[] { });
			HashSet<string> existingSortColumns = new HashSet<string>(externalSortColumns);
			return externalSortColumns
				.Zip(externalDescendings, (sortColumn, descending) => new PXSearchColumn(sortColumn, descending, null))
				.Concat((contextualSorts ?? SearchColumns).Where(searchColumn => !existingSortColumns.Contains(searchColumn.Column)))
				.ToList();
		}

		public static object[] GetSearches(this List<PXSearchColumn> searchColumns)
		{
			return searchColumns
				.Select(column => column.SearchValue)
				.ToArray();
		}

		public static string[] GetSortColumns(this List<PXSearchColumn> searchColumns)
		{
			return searchColumns
				.Select(column => column.Column)
				.ToArray();
		}

		public static bool[] GetDescendings(this List<PXSearchColumn> searchColumns)
		{
			return searchColumns
				.Select(column => column.Descending)
				.ToArray();
		}
	}
}
