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
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public delegate void OnDocumentHeaderInsertedDelegate(PXGraph graph, IBqlTable row);

    public delegate void OnTransactionInsertedDelegate(PXGraph graph, IBqlTable row);

    public delegate void BeforeSaveDelegate(PXGraph graph);

    public delegate void AfterCreateInvoiceDelegate(PXGraph graph, FSCreatedDoc fsCreatedDocRow);

    public interface IInvoiceProcessGraph
    {
        OnDocumentHeaderInsertedDelegate OnDocumentHeaderInserted { get; set; }

        OnTransactionInsertedDelegate OnTransactionInserted { get; set; }

        BeforeSaveDelegate BeforeSave { get; set; }

        AfterCreateInvoiceDelegate AfterCreateInvoice { get; set; }

        void Clear(PXClearOption option);

        PXGraph GetGraph();

        List<DocLineExt> GetInvoiceLines(Guid currentProcessID, int billingCycleID, string groupKey, bool getOnlyTotal, out decimal? invoiceTotal, string postTo);
        void UpdateSourcePostDoc(ServiceOrderEntry soGraph, AppointmentEntry apptGraph, PXCache<FSPostDet> cacheFSPostDet, FSPostBatch fsPostBatchRow, FSPostDoc fsPostDocRow);
    }
}
