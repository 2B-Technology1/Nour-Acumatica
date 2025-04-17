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
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.SO;

namespace PX.Objects.AM.Attributes
{
    public class SOTaxAMExtensionAttribute : SOTaxAttribute
    {
        public SOTaxAMExtensionAttribute(Type ParentType, Type TaxType, Type TaxSumType)
            : base(ParentType, TaxType, TaxSumType)
        {
        }

		public SOTaxAMExtensionAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type TaxCalculationMode)
			: base(ParentType, TaxType, TaxSumType, TaxCalculationMode) { }

        protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal, decimal CuryTaxDiscountTotal)
        {
            base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

            decimal CuryEstimatetotal = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

            if (CuryEstimatetotal == 0)
            {
                return;
            }

            decimal CuryDocTotal = CuryEstimatetotal + (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m);
            ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (sender.Graph is SOOrderEntry)
            {
                sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), "AMCuryEstimateTotal", SOOrder_AMCuryEstimateTotal_FieldUpdated);
            }
        }

        protected virtual void SOOrder_AMCuryEstimateTotal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (SOOrder)e.Row;
            if (row == null)
            {
                return;
            }

            var rowStatus = sender.GetStatus(row);
            if (rowStatus == PXEntryStatus.Deleted || rowStatus == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            var oldValue = (decimal)(e.OldValue ?? 0m);
            var newValue = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

            var diff = newValue - oldValue;

            if (diff == 0)
            {
                return;
            }

            decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
            decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
            CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
        }
    }
}
