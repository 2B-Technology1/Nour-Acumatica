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
using PX.Objects.IN;
using System;

namespace PX.Objects.CR.Descriptor.Attributes
{
	public class PXDBOpportunityProductQuantityAttribute : PXDBQuantityAttribute
	{
		public PXDBOpportunityProductQuantityAttribute(Type keyField, Type resultField, InventoryUnitType decimalVerifyUnits)
			: base(keyField, resultField, decimalVerifyUnits)
		{
		}

		protected override decimal? CalcResultValue(PXCache sender, QtyConversionArgs e)
		{
			decimal? resultval = null;
			if (_ResultField != null)
			{
				if (e.NewValue != null)
				{
					bool handled = false;
					if (this._HandleEmptyKey)
					{
						if (string.IsNullOrEmpty(GetFromUnit(sender, e.Row)))
						{
							resultval = (decimal)e.NewValue;
							handled = true;
						}
					}
					if (!handled)
					{
						if ((decimal)e.NewValue == 0)
						{
							resultval = 0m;
						}
						else
						{
							ConversionInfo convInfo = ReadConversionInfo(sender, e.Row);
							if (convInfo?.Conversion != null)
							{
								resultval = ConvertValue(sender, e.Row, (decimal)e.NewValue, convInfo.Conversion);
								var exception = VerifyForDecimalValue(sender, convInfo.Inventory, e.Row, (decimal)e.NewValue, resultval);
								if (exception?.ErrorLevel == PXErrorLevel.Error && e.ThrowNotDecimalUnitException && !exception.IsLazyThrow)
									throw exception;
							}
							else if (convInfo?.Inventory.InventoryID == null)
							{
								resultval = (decimal)e.NewValue;
							}
							else
							{
								if (!e.ExternalCall)
									throw new PXUnitConversionException(GetFromUnit(sender, e.Row));
							}
						}
					}
				}
			}
			return resultval;
		}
	}
}