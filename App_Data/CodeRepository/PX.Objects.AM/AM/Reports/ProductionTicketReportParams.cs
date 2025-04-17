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

using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.AM.Reports
{
    /// <summary>
    /// Manufacturing Production Ticket Report helper class for calling the AM625000 report.
    /// </summary>
    public class ProductionTicketReportParams
    {
        public const string ReportID = "AM625000";
        public const string ReportName = "Production Ticket";
        
        public static Dictionary<string, string> FromProdItem(AMProdItem amProdItem)
        {
            return BuildReportParameters(amProdItem, Parameters.OrderType, Parameters.Production_Nbr);
        }

        protected static Dictionary<string, string>  BuildReportParameters(AMProdItem amProdItem, string orderTypeKey, string prodOrdIdKey)
        {
            var parametersDictionary = new Dictionary<string, string>();

            if (amProdItem == null)
            {
                return parametersDictionary;
            }

            if (!string.IsNullOrWhiteSpace(amProdItem.OrderType))
            {
                parametersDictionary[orderTypeKey] = amProdItem.OrderType;
            }

            if (!string.IsNullOrWhiteSpace(amProdItem.ProdOrdID))
            {
                parametersDictionary[prodOrdIdKey] = amProdItem.ProdOrdID;
            }

            return parametersDictionary;
        }

        public static PXReportRequiredException CreateMultipleOrdersReportException(List<AMProdItem> prodItems)
        {
            return CreateMultipleOrdersReportException(prodItems, ReportID);
        }

        public static PXReportRequiredException CreateMultipleOrdersReportException(List<AMProdItem> prodItems, string reportId)
        {
            if (prodItems == null || prodItems.Count == 0)
            {
                return null;
            }

            PXReportRequiredException reportEx = null;

            foreach (var prodItem in prodItems)
            {
                reportEx = CreateMultipleOrdersReportException(reportEx, prodItem, reportId);
            }

            return reportEx;
        }

        public static PXReportRequiredException CreateMultipleOrdersReportException(PXReportRequiredException reportEx, AMProdItem prodItem, string reportId)
        {
            return string.IsNullOrWhiteSpace(prodItem?.ProdOrdID)
                ? reportEx
                : PXReportRequiredException.CombineReport(reportEx, reportId,
                    BuildReportParameters(prodItem, Fields.OrderType, Fields.ProductionNbr));
        }

        /// <summary>
        /// Report parameters
        /// </summary>
        public static class Parameters
        {
            public const string OrderType = "OrderType";
            public const string Production_Nbr = "Production_Nbr";
        }

        /// <summary>
        /// Fields within the report used for filtering.
        /// (Note: to use a field it must be in the report as a ViewerField)
        /// </summary>
        public static class Fields
        {
            public static readonly string OrderType = $"{nameof(AMProdItem)}.{nameof(AMProdItem.OrderType)}";
            public static readonly string ProductionNbr = $"{nameof(AMProdItem)}.{nameof(AMProdItem.ProdOrdID)}";
        }
    }
}
