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
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing batch release graph
    /// </summary>
    public class AMDocumentRelease : PXGraph<AMDocumentRelease>
    {
        public PXCancel<AMBatch> Cancel;
        [PXFilterable]
        public PXProcessingJoin<AMBatch,
			InnerJoin<Branch, On<AMBatch.branchID, Equal<Branch.branchID>>>,
			Where<AMBatch.released, Equal<False>, And<AMBatch.hold, Equal<False>,
				And<AMBatch.origBatNbr, IsNull,
				And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>>>>>> AMDocumentList;
        // Above should not include entries with source batch numbers. The source batch should be released to release the child batch numbers if any.

        public AMDocumentRelease()
        {
            AMDocumentList.SetProcessDelegate(
            delegate(List<AMBatch> list)
            {
                ReleaseDoc(list, true);
            });
        }

        public static void ReleaseDoc(List<AMBatch> list)
        {
            ReleaseDoc(list, false);
        }

        public static void ReleaseDoc(List<AMBatch> list, bool isMassProcess)
        {
            var failed = false;
            var rg = CreateInstance<AMReleaseProcess>();
            
            for (var i = 0; i < list.Count; i++)
            {
                var doc = list[i];
                try
                {
                    rg.Clear();

                    rg.ReleaseDocProc(doc);

                    if (isMassProcess)
                    {
                        PXProcessing<AMBatch>.SetInfo(i, PX.Data.ActionsMessages.RecordProcessed);
                    }
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXTraceHelper.WriteInformation($"Unable to release {AMDocType.GetDocTypeDesc(doc.DocType)} batch {doc.BatNbr}: {e.Message}");
                        PXProcessing<AMBatch>.SetError(i, e);
                        failed = true;
                    }
                    else if (list.Count == 1)
                    {
                        throw new PXOperationCompletedSingleErrorException(e);
                    }
                    else
                    {
                        PXTraceHelper.PxTraceException(e);

                        failed = true;
                    }
                }
            }

            if (failed)
            {
                throw new PXOperationCompletedException(PX.Data.ErrorMessages.SeveralItemsFailed);
            }
        }
    }
}
