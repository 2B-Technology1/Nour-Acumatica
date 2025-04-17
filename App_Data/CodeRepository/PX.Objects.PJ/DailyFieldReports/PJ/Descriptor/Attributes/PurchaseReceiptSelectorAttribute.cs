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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public sealed class PurchaseReceiptSelectorAttribute : PXCustomSelectorAttribute
    {
        public PurchaseReceiptSelectorAttribute()
            : base(typeof(POReceipt.receiptNbr),
				typeof(POReceipt.receiptType),
				typeof(POReceipt.receiptNbr),
                typeof(POReceipt.status),
                typeof(POReceipt.receiptType),
                typeof(POReceipt.vendorID),
                typeof(POReceipt.vendorLocationID),
                typeof(POReceipt.receiptDate),
                typeof(POReceipt.orderQty))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedPurchaseReceiptNumbers = _Graph.GetExtension<DailyFieldReportEntryPurchaseReceiptExtension>()
                .PurchaseReceipts.SelectMain().Select(pr => pr.PurchaseReceiptId);
            return GetPurchaseReceipts().Where(pr => pr.ReceiptNbr.IsNotIn(linkedPurchaseReceiptNumbers));
        }

        private IEnumerable<POReceipt> GetPurchaseReceipts()
        {
            return SelectFrom<POReceipt>
                .LeftJoin<POReceiptLine>
                    .On<POReceiptLine.FK.Receipt>
                .Where<APSetup.requireSingleProjectPerDocument.FromCurrent.IsEqual<True>
                        .And<POReceipt.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                    .Or<APSetup.requireSingleProjectPerDocument.FromCurrent.IsEqual<False>>
                        .And<POReceiptLine.projectID.IsEqual<DailyFieldReport.projectId.FromCurrent>>>
                .AggregateTo<
					GroupBy<POReceipt.receiptType,
					GroupBy<POReceipt.receiptNbr>>>
                .View.Select(_Graph).FirstTableItems;
        }
    }
}
