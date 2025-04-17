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
using PX.Common;
using PX.Data;

namespace PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction
{
	using static PXDataFieldAssign.AssignBehavior;

	public class StatusAccumulatorAttribute : PXAccumulatorAttribute
	{
		protected Dictionary<object, bool> _persisted;
		protected PXAccumulatorCollection _columns;
		protected bool _InternalCall = false;

		protected virtual TAccumulator Aggregate<TAccumulator>(PXCache cache, TAccumulator a, TAccumulator b)
			where TAccumulator : class, IBqlTable, new()
		{
			TAccumulator ret = (TAccumulator)cache.CreateCopy(a);

			foreach (KeyValuePair<string, PXAccumulatorItem> column in _columns)
			{
				if (column.Value.CurrentUpdateBehavior == Summarize)
				{
					object aVal = cache.GetValue(a, column.Key);
					object bVal = cache.GetValue(b, column.Key);
					object retVal = null;

					if (aVal is decimal)
						retVal = (decimal)aVal + (decimal)bVal;

					if (aVal is double)
						retVal = (double)aVal + (double)bVal;

					if (aVal is long)
						retVal = (long)aVal + (long)bVal;

					if (aVal is int)
						retVal = (int)aVal + (int)bVal;

					if (aVal is short)
						retVal = (short)aVal + (short)bVal;

					cache.SetValue(ret, column.Key, retVal);
				}
				else if (column.Value.CurrentUpdateBehavior == Replace)
				{
					object retVal = cache.GetValue(b, column.Key);
					cache.SetValue(ret, column.Key, retVal);
				}
				else if (column.Value.CurrentUpdateBehavior == Initialize)
				{
					object aVal = cache.GetValue(a, column.Key);
					object bVal = cache.GetValue(b, column.Key);
					object retVal = aVal ?? bVal;
					cache.SetValue(ret, column.Key, retVal);
				}
			}

			return ret;
		}

		public virtual void ResetPersisted(object row)
		{
			if (_persisted != null && _persisted.ContainsKey(row))
				_persisted.Remove(row);
		}

		public override object Insert(PXCache cache, object row)
		{
			object copy = cache.CreateCopy(row);

			PXAccumulatorCollection columns = new PXAccumulatorCollection(cache, row);

			_InternalCall = true;
			PrepareInsert(cache, row, columns);

			foreach (KeyValuePair<string, PXAccumulatorItem> column in columns)
				if (column.Value.CurrentUpdateBehavior == Summarize)
					cache.SetValue(copy, column.Key, null);

			object item = base.Insert(cache, copy);

			if (item != null && _persisted.ContainsKey(item))
			{
				foreach (string field in cache.Fields)
				{
					if (cache.GetValue(copy, field) == null)
					{
						if (cache.RaiseFieldDefaulting(field, copy, out object newvalue))
							cache.RaiseFieldUpdating(field, copy, ref newvalue);

						cache.SetValue(copy, field, newvalue);
					}
				}
				return copy;
			}
			return item;
		}

		public override void CacheAttached(PXCache cache)
		{
			base.CacheAttached(cache);
			_persisted = new Dictionary<object, bool>();
			cache.Graph.RowPersisted.AddHandler(cache.GetItemType(), RowPersisted);
			cache.Graph.OnClear += Graph_OnClear;
		}

		private void Graph_OnClear(PXGraph graph, PXClearOption option)
		{
			_persisted = new Dictionary<object, bool>();
		}

		protected override bool PrepareInsert(PXCache cache, object row, PXAccumulatorCollection columns)
		{
			_columns = columns;

			if (!base.PrepareInsert(cache, row, columns))
				return false;

			if (!cache.IsKeysFilled(row))
				return false;

			foreach (string field in cache.Fields)
			{
				if (cache.Keys.IndexOf(field) < 0 && field.StartsWith("Usr", StringComparison.InvariantCultureIgnoreCase))
				{
					object val = cache.GetValue(row, field);
					columns.Update(field, val, val != null ? Replace : Initialize);
				}
			}

			return true;
		}

		public override bool PersistInserted(PXCache cache, object row)
		{
			_InternalCall = false;
			if (!base.PersistInserted(cache, row))
				return false;

			_persisted.Add(row, true);
			return true;
		}

		public virtual void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Insert && e.TranStatus.IsIn(PXTranStatus.Completed, PXTranStatus.Aborted))
				if (_persisted.ContainsKey(e.Row))
					_persisted.Remove(e.Row);
		}

		public virtual bool IsZero(IStatus a) => a.IsZero();
	}
}
