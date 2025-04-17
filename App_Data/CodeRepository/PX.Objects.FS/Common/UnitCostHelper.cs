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
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    internal static class UnitCostHelper
    {
        public class UnitCostPair
        {
            public decimal? unitCost;
            public decimal? curyUnitCost;

            public UnitCostPair(decimal? unitCost, decimal? curyUnitCost)
            {
                this.unitCost = unitCost;
                this.curyUnitCost = curyUnitCost;
            }
        }

        public static UnitCostPair CalculateCuryUnitCost<unitCostField, inventoryIDField, uomField>(PXCache cache, object row, bool raiseUnitCostDefaulting, decimal? unitCost)
            where unitCostField : IBqlField
            where inventoryIDField : IBqlField
            where uomField : IBqlField
        {
            decimal curyUnitCost = 0m;

            if (raiseUnitCostDefaulting == true)
            {
                object unitCostObj;
                cache.RaiseFieldDefaulting<unitCostField>(row, out unitCostObj);
                unitCost = (decimal?)unitCostObj;
            }

            if (unitCost != null && unitCost != 0m)
            {
                decimal valueConvertedToBase = INUnitAttribute.ConvertToBase<inventoryIDField, uomField>(cache, row, unitCost.Value, INPrecision.NOROUND);

                IPXCurrencyHelper currencyHelper = cache.Graph.FindImplementation<IPXCurrencyHelper>();

                if (currencyHelper != null)
                {
                    valueConvertedToBase = currencyHelper.GetDefaultCurrencyInfo().CuryConvCury(unitCost.Value);
                }
                else
                {
                    CM.PXDBCurrencyAttribute.CuryConvCury(cache, row, valueConvertedToBase, out valueConvertedToBase, true);
                }

                curyUnitCost = Math.Round(valueConvertedToBase, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
            }

            return new UnitCostPair(curyUnitCost, unitCost);
        }
    }
}
