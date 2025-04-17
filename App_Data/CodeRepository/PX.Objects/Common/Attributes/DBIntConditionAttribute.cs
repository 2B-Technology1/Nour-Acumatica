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
	public class DBIntConditionAttribute : PXDBIntAttribute
	{
		protected Type _valueField;
		protected string _databaseField;
		protected object _expectedValue;

		/// <summary>
		/// Initializes a new instance of the DBIntConditionAttribute attribute.
		///	If the new value is equal to the expected value, then this field will save to database.
		/// </summary>
		/// <param name="valueField">The reference to a field in same DAC. Cannot be null.</param>
		/// <param name="expectedValue">Expected value for "valueField".</param>
		/// <param name="databaseField">A value of property will save to this field.</param>
		public DBIntConditionAttribute(Type valueField, object expectedValue, Type databaseField)
		{
			_valueField = valueField ?? throw new PXArgumentException(nameof(valueField));
			_expectedValue = expectedValue;

			if (databaseField == null)
				throw new PXArgumentException(nameof(databaseField));
			_databaseField = char.ToUpper(databaseField.Name[0]) + databaseField.Name.Substring(1);
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			base.CommandPreparing(sender, e);

			e.BqlTable = _BqlTable;
			var table = e.Table == null ? _BqlTable : e.Table;
			e.Expr = new Data.SQLTree.Column(_databaseField, new Data.SQLTree.SimpleTable(table), e.DataType);

			PXDBOperation command = e.Operation.Command();
			if (command == PXDBOperation.Update || command == PXDBOperation.Insert)
			{
				object currentValue = sender.GetValue(e.Row, _valueField.Name);

				if (!Equals(currentValue, _expectedValue))
				{
					e.ExcludeFromInsertUpdate();
				}
			}
		}
	}
}
