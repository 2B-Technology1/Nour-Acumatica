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

namespace PX.Objects.AP.InvoiceRecognition.Feedback
{
	internal class TableContext
	{
		public short Page { get; }
		public short Table { get; }

		private SortedDictionary<short, short> _rowByDetailRow { get; } = new SortedDictionary<short, short>();
		private Dictionary<string, List<short>> _columnByDetailColumn { get; } = new Dictionary<string, List<short>>(StringComparer.OrdinalIgnoreCase);
		private List<ColumnSelected> _columnSelected { get; } = new List<ColumnSelected>();
		private List<ColumnUnbound> _columnUnbound { get; } = new List<ColumnUnbound>();

		public IEnumerable<KeyValuePair<short, short>> RowByDetailRow => _rowByDetailRow;
		public IEnumerable<ColumnSelected> ColumnSelected => _columnSelected;
		public IEnumerable<ColumnUnbound> ColumnUnbound => _columnUnbound;

		public TableContext(short page, short table)
		{
			Page = page;
			Table = table;
		}

		public void RegisterColumnSelected(string detailColumn, List<short> columns)
		{
			if (_columnByDetailColumn.TryGetValue(detailColumn, out var existingColumns))
			{
				var appendColumn = columns.Count > existingColumns.Count;
				if (!appendColumn)
				{
					return;
				}

				_columnByDetailColumn[detailColumn] = columns;
				_columnSelected.Add(new ColumnSelected(detailColumn, columns));
				return;
			}

			_columnByDetailColumn.Add(detailColumn, columns);
			_columnSelected.Add(new ColumnSelected(detailColumn, columns));
		}

		public void RegisterColumnUnbound(string detailColumn)
		{
			if (_columnByDetailColumn.ContainsKey(detailColumn))
			{
				_columnByDetailColumn.Remove(detailColumn);
			}

			_columnUnbound.Add(new ColumnUnbound(detailColumn));
		}

		public void RegisterRowBound(short detailRow, short row)
		{
			_rowByDetailRow[detailRow] = row;
		}

		public bool CanBeBounded(CellBound cellBound)
		{
			if (cellBound.Page != Page || cellBound.Table != Table)
			{
				return false;
			}

			if (_rowByDetailRow.TryGetValue(cellBound.DetailRow, out var rowIndex) && rowIndex != cellBound.Row)
			{
				return false;
			}

			if (_columnByDetailColumn.TryGetValue(cellBound.DetailColumn, out var existingColumns))
			{
				var unboundColumn = cellBound.Columns.Count == 1 && cellBound.Columns[0] == -1;
				if (unboundColumn)
				{
					return true;
				}

				var appendColumn = existingColumns.TrueForAll(c => cellBound.Columns.Contains(c));
				if (appendColumn)
				{
					return true;
				}

				var knownColumns = cellBound.Columns.TrueForAll(c => existingColumns.Contains(c));
				if (knownColumns)
				{
					return true;
				}

				return false;
			}

			return true;
		}
	}
}
