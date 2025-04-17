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
using PX.Objects.CM.Extensions;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.TX;

namespace PX.Objects.Extensions.SalesTax
{
    public static class TaxExtensionHelper
    {
        public static void AdjustMinMaxTaxableAmt(
                  this PXGraph graph,
                   TaxRev taxrev,
                   ref decimal curyTaxableAmt,
                   ref decimal taxableAmt)
        {
            CurrencyInfo currencyInfo = graph.FindImplementation<IPXCurrencyHelper>().GetDefaultCurrencyInfo();
            taxableAmt = currencyInfo.CuryConvBase(curyTaxableAmt);

            if (taxrev.TaxableMin != 0.0m)
            {
                if (taxableAmt < taxrev.TaxableMin)
                {
                    curyTaxableAmt = 0.0m;
                    taxableAmt = 0.0m;
                }
            }

            if (taxrev.TaxableMax != 0.0m)
            {
                if (taxableAmt > taxrev.TaxableMax)
                {
                    curyTaxableAmt = currencyInfo.CuryConvCury((decimal)taxrev.TaxableMax);
                    taxableAmt = (decimal)taxrev.TaxableMax;
                }
            }
        }

        public static void SetExpenseAmountsForDeductibleVAT(this TaxDetail taxdet, TaxRev taxrev, decimal CuryTaxAmt, CurrencyInfo currencyInfo)
        {
            taxdet.CuryExpenseAmt = currencyInfo.RoundCury(CuryTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100));
            taxdet.ExpenseAmt = currencyInfo.CuryConvBase(taxdet.CuryExpenseAmt.Value);
        }
    }
}
