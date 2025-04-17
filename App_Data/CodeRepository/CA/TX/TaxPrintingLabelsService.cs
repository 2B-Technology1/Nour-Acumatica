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
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.TX;

namespace PX.Objects.Localizations.CA.TX
{
	public class TaxPrintingLabelsService
    {
         private readonly PXGraph graph = PXGraph.CreateInstance<PXGraph>();

        public string GetSOTaxShortPrintingLabels(string orderType, string orderNbr, int? lineNbr)
        {
            var taxes = GetSOShortPrintingLabelsForLine(orderType, orderNbr, lineNbr);

            return GetShortPrintingLabels(taxes);
        }

        public string GetARTaxShortPrintingLabels(string tranType, string refNbr, int? lineNbr)
        {
            var taxes = GetARShortPrintingLabelsForLine(tranType, refNbr, lineNbr);

            return GetShortPrintingLabels(taxes);
        }

        private string GetShortPrintingLabels(PXResultset<Tax> taxes)
        {
            string shortTaxLabels = string.Empty;

            foreach (Tax tax in taxes)
            {
                if (tax != null)
                {
                    shortTaxLabels += tax.ShortPrintingLabel;
                }
            }

            return shortTaxLabels;
        }

        private PXResultset<Tax> GetSOShortPrintingLabelsForLine(string orderType, string orderNbr, int? lineNbr)
        {
            graph.Clear(PXClearOption.ClearQueriesOnly);

            var taxes = PXSelectReadonly2<Tax,
                LeftJoin<SOTax,
                    On<SOTax.taxID, Equal<Tax.taxID>>>,
                Where<SOTax.orderType, Equal<Required<SOTax.orderType>>,
                    And<SOTax.orderNbr, Equal<Required<SOTax.orderNbr>>,
                        And<SOTax.lineNbr, Equal<Required<SOTax.lineNbr>>>>>,
                OrderBy<Asc<Tax.printingSequence>>>
                .Select(graph, orderType, orderNbr, lineNbr);

            return taxes;
        }

        private PXResultset<Tax> GetARShortPrintingLabelsForLine(string tranType, string refNbr, int? lineNbr)
        {
            graph.Clear(PXClearOption.ClearQueriesOnly);

            var taxes = PXSelectReadonly2<Tax,
                    LeftJoin<ARTax,
                        On<ARTax.taxID, Equal<Tax.taxID>>>,
                    Where<ARTax.tranType, Equal<Required<ARTax.tranType>>,
                        And<ARTax.refNbr, Equal<Required<ARTax.refNbr>>,
                            And<ARTax.lineNbr, Equal<Required<ARTax.lineNbr>>>>>,
                    OrderBy<Asc<Tax.printingSequence>>>
                .Select(graph, tranType, refNbr, lineNbr);

            return taxes;
        }
    }
}
