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
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public static class TreeWFStageHelper
    {
        public class TreeWFStageView : PXSelectOrderBy<FSWFStage, OrderBy<Asc<FSWFStage.sortOrder>>>
        {
            public TreeWFStageView(PXGraph graph) : base(graph)
            {
            }

            public TreeWFStageView(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }

        public static IEnumerable treeWFStages(PXGraph graph, string srvOrdType, int? wFStageID)
        {
            if (wFStageID == null)
            {
                wFStageID = 0;
            }

            PXResultset<FSWFStage> fsWFStageSet = PXSelectJoin<FSWFStage,
                                                  InnerJoin<FSSrvOrdType,
                                                  On<
                                                      FSSrvOrdType.srvOrdTypeID, Equal<FSWFStage.wFID>>>,
                                                  Where<
                                                      FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>,
                                                      And<FSWFStage.parentWFStageID, Equal<Required<FSWFStage.parentWFStageID>>>>,
                                                  OrderBy<
                                                      Asc<FSWFStage.sortOrder>>>
                                                  .Select(graph, srvOrdType, wFStageID);

            foreach (FSWFStage fsWFStageRow in fsWFStageSet)
            {
                yield return fsWFStageRow;
            }
        }
    }
}
