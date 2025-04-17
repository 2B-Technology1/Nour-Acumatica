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
using System.Linq;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    public static class UomHelper
    {

#if DEBUG
        static UomHelper()
        {
            AMDebug.TraceWriteLine("[UomHelper] Decimal Precision: Qty = {0} ; Cost/Price = {1}", QtyDecimalPrecision, CostDecimalPrecision);
        }
#endif
        /// <summary>
        /// Quantity Decimal Precision value
        /// </summary>
        /// <returns></returns>
        public static int QtyDecimalPrecision => CommonSetupDecPl.Qty;

        /// <summary>
        /// Cost Decimal Precision value
        /// </summary>
        /// <returns></returns>
        public static int CostDecimalPrecision => CommonSetupDecPl.PrcCst;

        public static string FormatQty(decimal? value)
        {
            return FormatQty(value, GetPrecision(INPrecision.QUANTITY));
        }

		public static string FormatQty(decimal? value, int precision)
		{
			return value.GetValueOrDefault().ToString(PrecisionDisplayFormat(precision, '#'));
		}

		public static string FormatQtyFixed(decimal? value)
		{
			return FormatQtyFixed(value, GetPrecision(INPrecision.QUANTITY));
		}

		public static string FormatQtyFixed(decimal? value, int precision)
		{
			return value.GetValueOrDefault().ToString(PrecisionDisplayFormat(precision, '0'));
		}

		public static string FormatCost(decimal? value)
        {
            return FormatCost(value, GetPrecision(INPrecision.UNITCOST));
        }

		public static string FormatCost(decimal? value, int precision)
		{
			return value.GetValueOrDefault().ToString(PrecisionDisplayFormat(precision, '#'));
		}

		private static string PrecisionDisplayFormat(int precision, char c)
		{
			return precision <= 0 ? "0" : "0.0".PadRight(precision + 2, c);
		}

		private static int GetPrecision(INPrecision precision)
		{
			var p = 0;
			switch (precision)
			{
				case INPrecision.NOROUND:
					p = -1;
					break;
				case INPrecision.QUANTITY:
					p = QtyDecimalPrecision;
					break;
				case INPrecision.UNITCOST:
					p = CostDecimalPrecision;
					break;
				default:
					p = 6;
					break;
			}

			if (p > 9)
			{
				p = 9;
			}

			return p <= 0 ? 0 : p;
		}

        public static decimal Round(decimal value, int nbrOfDecimals)
        {
            return Math.Round(value, nbrOfDecimals, MidpointRounding.AwayFromZero);
        }

        public static decimal QuantityRound(decimal? value)
        {
            return PXDBQuantityAttribute.Round(value);
        }

        public static decimal PriceCostRound(decimal value)
        {
            return PXDBPriceCostAttribute.Round(value);
        }

        public static bool TryConvertFromToQty<InventoryIDField>(PXCache sender, object row, string fromUnit, string toUnit, Decimal convertingQty, out decimal? result) where InventoryIDField : IBqlField
        {
            return TryConvertFromTo<InventoryIDField>(sender, row, fromUnit, toUnit, convertingQty, out result, INPrecision.QUANTITY);
        }

        public static bool TryConvertFromToCost<InventoryIDField>(PXCache sender, object row, string fromUnit, string toUnit, Decimal convertingCost, out decimal? result) where InventoryIDField : IBqlField
        {
            // The naming or logic appears backwards in Acumatica. 
            //  Set your from as the to and to as the from if you want to go from a UOM to another UOM for cost.
            //  Cost seems backwards but Qty appears correct

            var fromUnitAsToUnitParameter = fromUnit;
            var toUnitAsFromUnitParameter = toUnit;

            return TryConvertFromTo<InventoryIDField>(sender, row, toUnitAsFromUnitParameter, fromUnitAsToUnitParameter, convertingCost, out result, INPrecision.UNITCOST);
        }

        private static bool TryConvertFromTo<InventoryIDField>(PXCache sender, object row, string fromUnit, string toUnit, Decimal convertingDecimal, out decimal? result, INPrecision precision) where InventoryIDField : IBqlField
        {
            result = null;

            try
            {
                result = INUnitAttribute.ConvertFromTo<InventoryIDField>(sender, row, fromUnit, toUnit, convertingDecimal, precision);
                return true;
            }
            catch (PXUnitConversionException)
            {
                return false;
            }
        }

        public static bool TryConvertToBaseCost<InventoryIDField>(PXCache sender, object row, string fromUnit, Decimal convertingCost, out decimal? result) where InventoryIDField : IBqlField
        {
            return TryConvertToBase<InventoryIDField>(sender, row, fromUnit, convertingCost, out result, INPrecision.UNITCOST);
        }

        public static bool TryConvertToBaseQty<InventoryIDField>(PXCache sender, object row, string fromUnit, Decimal convertingQty, out decimal? result) where InventoryIDField : IBqlField
        {
            return TryConvertToBase<InventoryIDField>(sender, row, fromUnit, convertingQty, out result, INPrecision.QUANTITY);
        }

		public static bool TryConvertToBaseNoRound<InventoryIDField>(PXCache sender, object row, string fromUnit, Decimal convertingQty, out decimal? result) where InventoryIDField : IBqlField
		{
			return TryConvertToBase<InventoryIDField>(sender, row, fromUnit, convertingQty, out result, INPrecision.NOROUND);
		}

		private static bool TryConvertToBase<InventoryIDField>(PXCache sender, object row, string fromUnit, Decimal convertingDecimal, out decimal? result, INPrecision precision) where InventoryIDField : IBqlField
        {
            result = null;

            try
            {
                result = INUnitAttribute.ConvertToBase<InventoryIDField>(sender, row, fromUnit, convertingDecimal, precision);

                return true;
            }
            catch (PXUnitConversionException)
            {
                return false;
            }
        }

        public static bool TryConvertFromBaseCost<InventoryIDField>(PXCache sender, object row, string toUnit, Decimal convertingCost, out decimal? result) where InventoryIDField : IBqlField
        {
            return TryConvertFromBase<InventoryIDField>(sender, row, toUnit, convertingCost, out result, INPrecision.UNITCOST);
        }

        public static bool TryConvertFromBaseQty<InventoryIDField>(PXCache sender, object row, string toUnit, Decimal convertingQty, out decimal? result) where InventoryIDField : IBqlField
        {
            return TryConvertFromBase<InventoryIDField>(sender, row, toUnit, convertingQty, out result, INPrecision.QUANTITY);
        }

        public static bool TryConvertFromBase<InventoryIDField>(PXCache sender, object row, string toUnit, Decimal convertingDecimal, out decimal? result, INPrecision precision) where InventoryIDField : IBqlField
        {
            result = null;

            try
            {
                result = INUnitAttribute.ConvertFromBase<InventoryIDField>(sender, row, toUnit, convertingDecimal, precision);

                return true;
            }
            catch (PXUnitConversionException)
            {
                return false;
            }
        }

        /// <summary>
        /// Does the provided cache, row, and UOM field have a valid UOM value
        /// </summary>
        public static bool HasValidUOM<UOMField>(PXCache cache, object data, string uomValue) where UOMField : IBqlField
        {
            // Copy from PXDBQuantityAttribute.ReadConversionInfo (internal virtual)

            var unitAttribute = cache.GetAttributesOfType<INUnitAttribute>(data, typeof(UOMField).Name).FirstOrDefault();

            var unit = unitAttribute?.ReadConversion(cache, data, uomValue);

            return unit != null;
        }
    }
}
