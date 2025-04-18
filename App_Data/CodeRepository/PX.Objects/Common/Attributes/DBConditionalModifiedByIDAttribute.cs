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

using PX.Data;
using System;

namespace PX.Objects.Common.Attributes
{
	public class DBConditionalModifiedByIDAttribute : PXDBGuidAttribute, IPXRowPersistedSubscriber
	{
		protected Type _valueField;
		protected object _expectedValue;

		/// <summary>
		/// Initializes a new instance of the DBConditionalModifiedByAttribute attribute.
		///	If the new value is equal to the expected value, then this field will have the identifier value of current user.
		/// </summary>
		/// <param name="valueField">The reference to a field in same DAC. Cannot be null.</param>
		/// <param name="expectedValue">Expected value for "valueField".</param>
		public DBConditionalModifiedByIDAttribute(Type valueField, object expectedValue)
		{
			_valueField = valueField ?? throw new PXArgumentException(nameof(valueField));
			_expectedValue = expectedValue;
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			if (e.Row == null) return;

			bool insert = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert;
			bool update = (e.Operation & PXDBOperation.Command) == PXDBOperation.Update;

			if (insert || update)
			{
				object currentValue = sender.GetValue(e.Row, _valueField.Name);

				if (Equals(currentValue, _expectedValue))
				{
					if (e.Value == null)
					{
						e.DataType = PXDbType.UniqueIdentifier;
						e.DataValue = PXAccess.GetTrueUserID();
					}
					else
						e.ExcludeFromInsertUpdate();
				}
				else
					e.DataValue = null;
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			bool insert = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert;
			bool update = (e.Operation & PXDBOperation.Command) == PXDBOperation.Update;

			if ((insert || update) && e.TranStatus == PXTranStatus.Completed)
			{
				object currentValue = sender.GetValue(e.Row, _valueField.Name);

				if (Equals(currentValue, _expectedValue))
				{
					if (sender.GetValue(e.Row, _FieldOrdinal) == null)
						sender.SetValue(e.Row, _FieldOrdinal, PXAccess.GetTrueUserID());
				}
				else
					sender.SetValue(e.Row, _FieldOrdinal, null);
			}
		}
	}
}
