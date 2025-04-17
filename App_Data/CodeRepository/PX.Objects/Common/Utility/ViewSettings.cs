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

namespace PX.Objects.Common
{
	public class ViewSettings
	{
		public static ViewSettings FromCurrentContext()
			=> new ViewSettings
			{
				Currents = PXView.Currents.ToList(),
				Parameters = PXView.Parameters.ToList(),
				Searches = PXView.Searches.ToList(),
				SortColumns = PXView.SortColumns.ToList(),
				Descendings = PXView.Descendings.ToList(),
				Filters = PXView.Filters,
				StartRow = PXView.StartRow,
				ReverseOrder = PXView.ReverseOrder,
				MaximumRows = PXView.MaximumRows
			};

		public ViewSettings()
		{
			FieldsMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		}

		private int? _previousMaximumRows;
		private int? _previousStartRow;

		public List<object> Currents { get; set; }

		public List<object> Parameters { get; set; }

		public List<object> Searches { get; set; }

		public List<string> SortColumns { get; set; }

		public List<bool> Descendings { get; set; }

		public PXView.PXFilterRowCollection Filters { get; set; }

		public bool ReverseOrder { get; set; }

		public int MaximumRows { get; set; }

		public int StartRow { get; set; }

		public Dictionary<string, string> FieldsMap { get; set; }

		public void SelectFromFirstPage()
		{
			_previousMaximumRows = MaximumRows;
			_previousStartRow = StartRow;

			if (ReverseOrder)
				MaximumRows = 0;
			else
				MaximumRows = StartRow + MaximumRows;

			StartRow = 0;
		}

		public List<TRow> GetCurrentPageRange<TRow>(List<TRow> rows)
		{
			if (_previousMaximumRows == null || _previousStartRow == null || rows.Count == 0)
				return rows;

			int startIndex;
			int count;
			if (!ReverseOrder)
			{
				startIndex = _previousStartRow ?? 0;
				if (startIndex > rows.Count)
					startIndex = rows.Count;
				if (startIndex < 0)
					startIndex = 0;

				count = rows.Count - startIndex;
			}
			else
			{
				startIndex = rows.Count + (_previousStartRow ?? 0);
				var endIndex = startIndex + (_previousMaximumRows ?? 0);
				if (startIndex < 0)
					startIndex = 0;
				if (endIndex < 0)
					endIndex = 0;
				count = endIndex - startIndex;
			}

			rows = rows.GetRange(startIndex, count);

			return rows;
		}
	}

	public static class ViewSettingsUtils
	{
		public static bool ApplySortingFrom<TEntity>(this ViewSettings viewSettings, PXSelectBase<TEntity> query)
			where TEntity : class, IBqlTable, new()
		{
			IBqlSortColumn[] sortColumns = query.View.BqlSelect.GetSortColumns();
			var cache = query.View.Graph.Caches<TEntity>();

			return viewSettings.ApplySorting(cache, sortColumns);
		}

		public static bool ApplySorting(this ViewSettings viewSettings, PXCache cache, IBqlSortColumn[] querySorting)
		{
			if (!viewSettings.SortColumns.Any())
				return false;

			List<string> customColumns;
			if (cache.Keys.Count == 0 || viewSettings.SortColumns.Count < cache.Keys.Count)
				customColumns = new List<string>(viewSettings.SortColumns);
			else
				customColumns = viewSettings.SortColumns.Take(viewSettings.SortColumns.Count - cache.Keys.Count).ToList();

			var queryFields = querySorting.Select(
				sort =>
				new
				{
					Type = sort.GetReferencedType(),
					Name = cache.GetField(sort.GetReferencedType()),
					Descending = sort.IsDescending
				})
				.ToList();

			void removeQueryField(string name)
			{
				var index = queryFields.FindIndex(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
				if (index >= 0)
					queryFields.RemoveAt(index);
			}

			for (var i = 0; i < customColumns.Count; i++)
			{
				var column = customColumns[i];
				removeQueryField(column);

				if (viewSettings.FieldsMap != null && viewSettings.FieldsMap.TryGetValue(column, out var mappedField))
				{
					//custom sorting by InventoryID while the query already contains a sorting by InventoryCD
					var searchValue = viewSettings.Searches[i];
					if(searchValue == null)
						viewSettings.SortColumns[i] = mappedField;

					removeQueryField(mappedField);
				}
			}

			var customIndex = customColumns.Count;
			foreach (var field in queryFields)
			{
				insert(customIndex++, field.Name, field.Descending);
			}

			return true;

			void insert(int index, string field, bool desc)
			{
				viewSettings.Searches.Insert(index, null);
				viewSettings.SortColumns.Insert(index, field);
				viewSettings.Descendings.Insert(index, desc);
			};
		}

		public static List<object> Select(this PXView view, ViewSettings viewSettings)
		{
			var startRow = viewSettings.StartRow;
			int totalRows = 0;

			var rows = view.Select(
					viewSettings.Currents.ToArray(),
					viewSettings.Parameters.ToArray(),
					viewSettings.Searches.ToArray(),
					viewSettings.SortColumns.ToArray(),
					viewSettings.Descendings.ToArray(),
					viewSettings.Filters,
					ref startRow,
					viewSettings.MaximumRows,
					ref totalRows);

			return rows;
		}

		public static List<TEntity> Select<TEntity>(this PXView view, ViewSettings viewSettings)
			where TEntity : class, IBqlTable, new()
		{
			var startRow = viewSettings.StartRow;
			int totalRows = 0;

			var rows = view.Select(
					viewSettings.Currents.ToArray(),
					viewSettings.Parameters.ToArray(),
					viewSettings.Searches.ToArray(),
					viewSettings.SortColumns.ToArray(),
					viewSettings.Descendings.ToArray(),
					viewSettings.Filters,
					ref startRow,
					viewSettings.MaximumRows,
					ref totalRows)
				.RowCast<TEntity>()
				.ToList();

			return rows;
		}
	}
}
