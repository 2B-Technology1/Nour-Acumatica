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

using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.GraphExtensions;
using PX.Objects.IN.Matrix.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class MatrixAttributeSelectorAttribute : PXSelectorAttribute, IPXFieldUpdatedSubscriber, IPXRowPersistingSubscriber
	{
		public const string DummyAttributeName = "~MX~DUMMY~";

		public class dummyAttributeName : PX.Data.BQL.BqlString.Constant<dummyAttributeName>
		{
			public dummyAttributeName() : base(DummyAttributeName) { }
		}

		public const string DummyAttributeValue = nameof(CSAnswers.Value);

		protected Type _secondField;
		protected bool _allowTheSameValue;

		public MatrixAttributeSelectorAttribute(Type type, Type secondField, bool allowTheSameValue, params Type[] fieldList)
			: base(type, fieldList)
		{
			_secondField = secondField;
			_allowTheSameValue = allowTheSameValue;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue as string == DummyAttributeName)
				return;

			if (e.Row != null && e.NewValue == null)
			{
				string secondFieldValue = sender.GetValue(e.Row, _secondField.Name) as string;

				if (secondFieldValue.IsNotIn(null, DummyAttributeName))
				{
					var values = SelectAll(sender, _FieldName, e.Row);
					bool isListEmpty = IsValueListEmpty(values);

					if (isListEmpty)
					{
						e.NewValue = DummyAttributeName;
						return;
					}
				}
			}

			base.FieldVerifying(sender, e);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.ReturnValue as string == DummyAttributeName)
				e.ReturnValue = null;
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs args)
		{
			if (args.Row == null)
				return;

			string fieldOldValue = (string)args.OldValue;
			string fieldValue = (string)sender.GetValue(args.Row, _FieldName);
			string secondFieldValue = (string)sender.GetValue(args.Row, _secondField.Name);

			ReplaceSecondFieldValueWithOldValueIfCurrentValueIsTheSame(sender, args.Row, fieldOldValue, fieldValue, ref secondFieldValue);
			SetDummyAttribute(sender, args.Row, fieldValue, secondFieldValue);
		}

		protected virtual void ReplaceSecondFieldValueWithOldValueIfCurrentValueIsTheSame(
			PXCache sender, object row, string fieldOldValue, string fieldValue, ref string secondFieldValue)
		{
			if (!string.Equals(fieldOldValue, fieldValue, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(secondFieldValue, fieldValue, StringComparison.OrdinalIgnoreCase) && secondFieldValue != null)
			{
				sender.SetValueExt(row, _secondField.Name, fieldOldValue);
				secondFieldValue = fieldOldValue;
			}
		}

		protected virtual void SetDummyAttribute(PXCache cache, object row, string fieldValue, string secondFieldValue)
		{
			if (fieldValue.IsNotIn(null, DummyAttributeName) && secondFieldValue == null)
			{
				var secondFieldValues = SelectAll(cache, _secondField.Name, row);
				bool isSecondFieldListEmpty = IsValueListEmpty(secondFieldValues);

				if (isSecondFieldListEmpty)
					cache.SetValueExt(row, _secondField.Name, DummyAttributeName);
			}
			else if (fieldValue == null && secondFieldValue == DummyAttributeName)
			{
				cache.SetValueExt(row, _secondField.Name, null);
			}
		}

		protected virtual bool IsValueListEmpty(List<object> values)
			=> (values.Count <= 0 && !_allowTheSameValue) ||
				(values.Count <= 1 && _allowTheSameValue);

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			bool insert = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert;
			bool update = (e.Operation & PXDBOperation.Command) == PXDBOperation.Update;

			if (!insert && !update)
				return;

			string fieldValue = sender.GetValue(e.Row, _FieldName) as string;
			if (fieldValue == DummyAttributeName)
			{
				var values = SelectAll(sender, _FieldName, e.Row);

				if (!IsValueListEmpty(values))
				{
					if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
					}
				}
			}
		}

		public virtual void RefreshDummyValue(PXCache cache, object row)
		{
			string fieldValue = cache.GetValue(row, _FieldName) as string;
			string secondFieldValue = cache.GetValue(row, _secondField.Name) as string;

			if (fieldValue == DummyAttributeName)
			{
				var values = SelectAll(cache, _FieldName, row);
				bool isListEmpty = IsValueListEmpty(values);

				if (!isListEmpty)
					cache.SetValueExt(row, _FieldName, null);
			}
			else if (fieldValue == null && secondFieldValue.IsNotIn(DummyAttributeName, null))
			{
				var values = SelectAll(cache, _FieldName, row);
				bool isListEmpty = IsValueListEmpty(values);

				if (isListEmpty)
					cache.SetValueExt(row, _FieldName, DummyAttributeName);
			}
		}
	}
}
