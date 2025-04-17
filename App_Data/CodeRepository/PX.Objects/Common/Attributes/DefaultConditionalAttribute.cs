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
	public class DefaultConditionalAttribute : PXDefaultAttribute
	{
		protected Type _valueField;
		protected object[] _expectedValues;

		/// <summary>
		/// Initializes a new instance of the DefaultConditionalAttribute attribute.
		///	If the new value is equal to the expected value, then this field will be verified.
		/// </summary>
		/// <param name="valueField">The reference to a field in same DAC. Cannot be null.</param>
		/// <param name="expectedValues">Expected value for "valueField".</param>
		public DefaultConditionalAttribute(Type valueField, params object[] expectedValues)
		{
			_valueField = valueField ?? throw new PXArgumentException(nameof(valueField));
			_expectedValues = expectedValues ?? throw new PXArgumentException(nameof(expectedValues));
		}

		/// <summary>
		/// Initializes a new instance of the DefaultConditionalAttribute attribute.
		///	If the new value is equal to the expected value, then this field will be verified.
		/// </summary>
		/// <param name="sourceType">The value will be passed to PXDefaultAttribute constructor as sourceType parameter.</param>
		/// <param name="valueField">The reference to a field in same DAC. Cannot be null.</param>
		/// <param name="expectedValues">Expected value for "valueField".</param>
		public DefaultConditionalAttribute(Type sourceType, Type valueField, params object[] expectedValues)
			: base(sourceType)
		{
			_valueField = valueField ?? throw new PXArgumentException(nameof(valueField));
			_expectedValues = expectedValues ?? throw new PXArgumentException(nameof(expectedValues));
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object currentValue = sender.GetValue(e.Row, _valueField.Name);

			foreach (var expectedValue in _expectedValues)
			{
				if (Equals(currentValue, expectedValue))
				{
					base.RowPersisting(sender, e);
					break;
				}
			}
		}
	}
}
