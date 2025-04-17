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
using PX.Objects.Common.Attributes;
using PX.Objects.GL;
using System;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class TemplateInventorySubAccountAttribute : SubAccountAttribute
	{
		protected bool _expectedStockItemValue;
		protected Type _databaseField;

		public TemplateInventorySubAccountAttribute(bool expectedStockItemValue, Type databaseField, Type accountType)
			: base(accountType)
		{
			_expectedStockItemValue = expectedStockItemValue;
			_databaseField = databaseField;

			if (_DBAttrIndex != -1)
				_Attributes[_DBAttrIndex] = new DBIntConditionAttribute(typeof(InventoryItem.stkItem), expectedStockItemValue, databaseField);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsCurrentStockItemValueEqualsExpectedValue(e.Row as InventoryItem))
			{
				if (string.Compare(FieldName, _databaseField.Name, true) != 0)
				{
					var valueOfCurrentField = sender.GetValue(e.Row, FieldName);
					sender.SetValue(e.Row, _databaseField.Name, valueOfCurrentField);
				}

				base.RowPersisting(sender, e);
			}
		}

		protected virtual bool IsCurrentStockItemValueEqualsExpectedValue(InventoryItem currentRow)
			=> currentRow?.StkItem == _expectedStockItemValue;
	}
}
