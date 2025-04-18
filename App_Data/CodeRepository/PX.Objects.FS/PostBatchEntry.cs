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

namespace PX.Objects.FS
{
    public class PostBatchEntry : PXGraph<PostBatchEntry, FSPostBatch>
    {
        public PXSelect<FSPostBatch> PostBatchRecords;

        public virtual FSPostBatch InitFSPostBatch(int? billingCycleID, DateTime? invoiceDate, string postTo, DateTime? upToDate, string invoicePeriodID)
        {
            FSPostBatch fsPostBatchRow = new FSPostBatch();

            fsPostBatchRow.QtyDoc = 0;
            fsPostBatchRow.BillingCycleID = billingCycleID;
            fsPostBatchRow.InvoiceDate = new DateTime(invoiceDate.Value.Year, invoiceDate.Value.Month, invoiceDate.Value.Day, 0, 0, 0);
            fsPostBatchRow.PostTo = postTo;
            fsPostBatchRow.UpToDate = upToDate.HasValue == true ? new DateTime(upToDate.Value.Year, upToDate.Value.Month, upToDate.Value.Day, 0, 0, 0) : upToDate;
            fsPostBatchRow.CutOffDate = null;
            fsPostBatchRow.FinPeriodID = invoicePeriodID;
            fsPostBatchRow.Status = FSPostBatch.status.Temporary;

            return fsPostBatchRow;
        }

        public virtual FSPostBatch CreatePostingBatch(int? billingCycleID, DateTime? upToDate, DateTime? invoiceDate, string invoiceFinPeriodID, string postTo)
        {
            FSPostBatch fsPostBatchRow = InitFSPostBatch(billingCycleID, invoiceDate, postTo, upToDate, invoiceFinPeriodID);

            PostBatchRecords.Current = PostBatchRecords.Insert(fsPostBatchRow);
            Save.Press();

            return PostBatchRecords.Current;
        }

        public virtual void CompletePostingBatch(FSPostBatch fsPostBatchRow, int documentsQty)
        {
            fsPostBatchRow.QtyDoc = documentsQty;
            fsPostBatchRow.Status = FSPostBatch.status.Completed;

            PostBatchRecords.Update(fsPostBatchRow);
            Save.Press();
        }

        public virtual IInvoiceGraph CreateInvoiceGraph(string targetScreen)
        {
            return InvoiceHelper.CreateInvoiceGraph(targetScreen);
        }

        public virtual bool AreAppointmentsPostedInSO(PXGraph graph, int? sOID)
        {
            return InvoiceHelper.AreAppointmentsPostedInSO(graph, sOID);
        }

        public virtual void DeletePostingBatch(FSPostBatch fsPostBatchRow)
        {
            if (fsPostBatchRow.BatchID < 0)
            {
                return;
            }

            PostBatchRecords.Current = PostBatchRecords.Search<FSPostBatch.batchID>(fsPostBatchRow.BatchID);

            if (PostBatchRecords.Current == null || PostBatchRecords.Current.BatchID != fsPostBatchRow.BatchID)
            {
                return;
            }

            IInvoiceGraph invoiceGraph = CreateInvoiceGraph(fsPostBatchRow.PostTo);

            using (var ts = new PXTransactionScope())
            {
                PXResultset<FSCreatedDoc> fsCreatedDocSet = PXSelect<FSCreatedDoc,
                                                            Where<
                                                                FSCreatedDoc.batchID, Equal<Required<FSCreatedDoc.batchID>>>>
                                                            .Select(invoiceGraph.GetGraph(), fsPostBatchRow.BatchID);

                foreach (FSCreatedDoc fsCreatedDocRow in fsCreatedDocSet)
                {
                    if (fsCreatedDocRow.PostTo != fsPostBatchRow.PostTo)
                    {
                        throw new PXException(TX.Error.DOCUMENT_MODULE_DIFERENT_T0_BATCH_MODULE, fsCreatedDocRow.PostTo, fsPostBatchRow.PostTo);
                    }

                    invoiceGraph.DeleteDocument(fsCreatedDocRow);
                }

                ts.Complete();
            }
        }
    }
}
