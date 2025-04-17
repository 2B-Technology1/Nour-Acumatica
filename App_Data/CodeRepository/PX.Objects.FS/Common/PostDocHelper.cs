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
using PX.Objects.CS;

namespace PX.Objects.FS
{
	public static class DocGenerationHelper
    {
        public static void ValidatePostBatchStatus<TDocument>(PXGraph graph, PXDBOperation dbOperation, string postTo, string createdDocType, string createdRefNbr)
            where TDocument : class, IBqlTable, new()
        {
            if (dbOperation == PXDBOperation.Update
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_APPOINTMENT)
                && graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.INVOICE_BY_SERVICE_ORDER)
            )
            {
                FSCreatedDoc fsCreatedDocRow = PXSelectJoin<FSCreatedDoc,
                                                InnerJoin<FSPostBatch, On<FSCreatedDoc.batchID, Equal<FSPostBatch.batchID>>>,
                                                Where<
                                                    FSPostBatch.status, Equal<FSPostBatch.status.temporary>,
                                                    And<FSPostBatch.postTo, Equal<Required<FSPostBatch.postTo>>,
                                                    And<FSCreatedDoc.createdDocType, Equal<Required<FSCreatedDoc.createdDocType>>,
                                                    And<FSCreatedDoc.createdRefNbr, Equal<Required<FSCreatedDoc.createdRefNbr>>>>>>>
                                             .Select(graph, postTo, createdDocType, createdRefNbr);

                if (fsCreatedDocRow != null)
                {
                    PXProcessing<TDocument>.SetError(TX.Error.CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY);
                    throw new PXException(TX.Error.CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY);
                }
            }
		}

		public static void ValidateAutoNumbering(PXGraph graph, string numberingID)
		{
			Numbering numbering = null;

			if (numberingID != null)
			{
				numbering = PXSelect<Numbering,
							Where<
								Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>
							.Select(graph, numberingID);
			}

			if (numbering == null)
			{
				throw new PXSetPropertyException(CS.Messages.NumberingIDNull);
			}

			if (numbering.UserNumbering == true)
			{
				throw new PXSetPropertyException(CS.Messages.CantManualNumber, numbering.NumberingID);
			}
		}
	}
}
