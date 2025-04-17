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
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using PX.Objects.GL;

namespace PX.Objects.AM
{
    public class ReleaseOrd : PXGraph<ReleaseOrd>
    {
        public PXCancel<AMBatch> Cancel;
        [PXFilterable]
        public PXProcessingJoin<AMProdItem,
			InnerJoin<Branch, On<AMProdItem.branchID, Equal<Branch.branchID>>>,
            Where<AMProdItem.released, Equal<False>,
				And<AMProdItem.lastOperationID, IsNotNull,
                And<AMProdItem.hold, Equal<False>,
				And<Branch.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>,
                And<Where<AMProdItem.function, Equal<OrderTypeFunction.regular>,
                    Or<AMProdItem.function, Equal<OrderTypeFunction.disassemble>>>>>>>>> PlannedOrds;

        public PXSetup<AMPSetup> ampsetup;

        public ReleaseOrd()
        {
            PlannedOrds.SetProcessDelegate(delegate (List<AMProdItem> list)
            {
                ReleaseDoc(list, true);
            });
        }

        public static void ReleaseDoc(List<AMProdItem> list, bool isMassProcess)
        {
			if (list == null)
			{
				return;
			}

			var prodMaint = CreateInstance<ProdMaint>();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					var prodItem = list[i];
				prodMaint.Clear();
				prodMaint.ProdMaintRecords.Current = prodItem;
				prodMaint.release.Press();

					PXProcessing<AMProdItem>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					PXProcessing<AMProdItem>.SetError(i, e.Message);
				}
			}
		}
    }
}
