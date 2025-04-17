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
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.Localizations.CA.AR
{
	/// <summary>
	/// A cache extension for printing of an AR invoice. The extension provides a way to calculate the document total for printing with Canadian tax calculation.
	/// </summary>
	public sealed class ARInvoiceExt : PXCacheExtension<PX.Objects.AR.ARInvoice>
    {
        #region IsActive

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
        }

        #endregion

        #region CuryDocTotalWithoutTax

        public abstract class printCuryDocTotalWithoutTax : BqlDecimal.Field<printCuryDocTotalWithoutTax> { }

        /// <summary>
        /// This field is for calculating the correct document total without taxes and without the
        /// cash discount.  It is for printing only.
        ///
        /// CuryDocBal has the right amount but only until the invoice is released or until payments are
        /// applied so it cannot be used reliably.  When it's a cash sale, CuryDocBal's value drops to
        /// zero soon as the invoice is released.
        ///
        /// Instead of using CuryDocBal we compute the amount we need by using the following logic:
        ///
        ///     CuryOrigDocAmt - CuryTaxTotal
        ///
        ///     if cash sale or cash return and cash discount is not zero
        ///         + CuryOrigDiscAmt
        ///
        /// </summary>
        [PXDecimal(2)]
        public decimal? PrintCuryDocTotalWithoutTax
        {
            [PXDependsOnFields(
                typeof(ARInvoice.docType),
                typeof(ARInvoice.curyOrigDocAmt),
                typeof(ARInvoice.curyTaxTotal),
                typeof(ARInvoice.curyOrigDiscAmt))]
            get
            {
                decimal? result = Base.CuryOrigDocAmt - Base.CuryTaxTotal;

                if ((Base.DocType == ARDocType.CashSale || Base.DocType == ARDocType.CashReturn) &&
                    Base.CuryOrigDiscAmt.HasValue && Base.CuryOrigDiscAmt.Value != 0m)
                {
                    result += Base.CuryOrigDiscAmt;
                }

                return result;
            }
        }

        #endregion
    }
}
