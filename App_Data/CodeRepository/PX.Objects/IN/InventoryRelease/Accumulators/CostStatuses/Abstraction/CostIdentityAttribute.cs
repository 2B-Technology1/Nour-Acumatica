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

namespace PX.Objects.IN.InventoryRelease.Accumulators.CostStatuses.Abstraction
{
	public class CostIdentityAttribute : PXDBLongIdentityAttribute
	{
		protected new long? _KeyToAbort = null;
		protected Type[] _ChildTypes = null;

		public CostIdentityAttribute(params Type[] ChildTypes)
			: base()
		{
			_ChildTypes = ChildTypes;
		}

		public override void RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					_KeyToAbort = (long?)cache.GetValue(e.Row, _FieldOrdinal);
					base.RowPersisted(cache, e);
					ConfirmKeyForChildren(cache, e.Row, isNewRow: e.Operation.Command() == PXDBOperation.Insert);
				}
				else if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
				{
					RollbackKeyForChildren(cache, e.Row);
					_KeyToAbort = null;
					base.RowPersisted(cache, e);
				}
			}
		}

		private void ConfirmKeyForChildren(PXCache cache, object row, bool isNewRow)
		{
			if (_KeyToAbort < 0)
			{
				long? newKey = isNewRow
					? Convert.ToInt64(PXDatabase.SelectIdentity())
					: SelectAccumIdentity(cache, row);

				ChangeKeyForChildren(cache, from: _KeyToAbort, to: newKey);
			}
		}

		private long? SelectAccumIdentity(PXCache cache, object row)
		{
			var fields = new List<PXDataField> { new PXDataField(_FieldName) };

			foreach (string key in cache.Keys)
				fields.Add(new PXDataFieldValue(key, cache.GetValue(row, key)));

			using (PXDataRecord UpdatedRow = PXDatabase.SelectSingle(cache.BqlTable, fields.ToArray()))
				if (UpdatedRow != null)
					return UpdatedRow.GetInt64(0);

			return null;
		}

		private void RollbackKeyForChildren(PXCache cache, object row)
		{
			ChangeKeyForChildren(cache, from: (long?)cache.GetValue(row, _FieldOrdinal), to: _KeyToAbort);
		}

		private void ChangeKeyForChildren(PXCache cache, long? from, long? to)
		{
			foreach (Type childKeyField in _ChildTypes)
			{
				PXCache childCache = cache.Graph.Caches[BqlCommand.GetItemType(childKeyField)];
				foreach (object item in childCache.Inserted)
					if ((long?)childCache.GetValue(item, childKeyField.Name) == from)
						childCache.SetValue(item, childKeyField.Name, to);
			}
		}
	}
}
