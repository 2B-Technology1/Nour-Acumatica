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

namespace PX.Objects.Common.Attributes
{
	public class PXDBDefaultBankTaxZoneAttribute : PXDBDefaultAttribute
	{
		public PXDBDefaultBankTaxZoneAttribute(Type sourceType) :
			base(sourceType)
		{
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) && _SourceType != null)
			{
				EnsureIsRestriction(sender);
				if (_IsRestriction.Value == true)
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
						&& !_DoubleDefaultAttribute
						&& (key is string && ((string)key).StartsWith(" ", StringComparison.InvariantCultureIgnoreCase)
						|| key is int && ((int)key) < 0
						|| key is long && ((long)key) < 0))
					{
						sender.SetValue(e.Row, _FieldOrdinal, null);
					}
					if (_IsRestriction.Persisted != null && key != null)
					{
						object parent;
						if (_IsRestriction.Persisted.TryGetValue(key, out parent))
						{
							key = sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName);
							sender.SetValue(e.Row, _FieldOrdinal, key);
							if (key != null)
							{
								_IsRestriction.Persisted[key] = parent;
							}
						}
					}
				}
				else
				{
					sender.SetValue(e.Row, _FieldOrdinal, null);
					if (_Select != null)
					{
						PXView view = sender.Graph.TypedViews.GetView(_Select, false);
						List<object> source = view.SelectMultiBound(new object[] { e.Row });
						if (source != null && source.Count > 0)
						{
							object result = source[source.Count - 1];
							if (result is PXResult) result = ((PXResult)result)[0];
							sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[_SourceType].GetValue(result, _SourceField ?? _FieldName));
						}
					}
					// this block is overriden because of AC-210493. We need get a value not from the current record, but from the parent one
					else if (_SourceType != null)
					{
						var parentTran = PXParentAttribute.SelectParent(sender, e.Row, _SourceType);
						PXCache cache = sender.Graph.Caches[_SourceType];

						if (parentTran != null)
						{
							sender.SetValue(e.Row, _FieldOrdinal, cache.GetValue(parentTran, _SourceField ?? _FieldName));
						}
					}
				}
			}
			if (PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _DefaultForInsert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && _DefaultForUpdate) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, $"[{_FieldName}]"))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}
	}
}
