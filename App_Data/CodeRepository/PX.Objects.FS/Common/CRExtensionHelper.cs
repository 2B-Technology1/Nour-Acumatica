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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    public static class CRExtensionHelper
    {
		public static void LaunchEmployeeBoard(PXGraph graph, string refNbr, string srvOrdType)
		{
			if (refNbr == null)
			{
				return;
			}

			ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

			graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
							.Search<FSServiceOrder.refNbr>(refNbr, srvOrdType);

			graphServiceOrder.OpenEmployeeBoard();
		}

		public static void LaunchServiceOrderScreen(PXGraph graph, string refNbr, string srvOrdType)
		{
			if (refNbr == null)
			{
				return;
			}

			ServiceOrderEntry graphServiceOrder = PXGraph.CreateInstance<ServiceOrderEntry>();

			graphServiceOrder.ServiceOrderRecords.Current = graphServiceOrder.ServiceOrderRecords
							.Search<FSServiceOrder.refNbr>(refNbr, srvOrdType);

			throw new PXRedirectRequiredException(graphServiceOrder, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

        public static FSSrvOrdType GetServiceOrderType(PXGraph graph, string srvOrdType)
        {
            if (string.IsNullOrEmpty(srvOrdType))
            {
                return null;
            }

            return PXSelect<FSSrvOrdType,
                   Where<
                       FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                   .Select(graph, srvOrdType);
        }

        public static int? GetSalesPersonID(PXGraph graph, int? ownerID)
        {
            EPEmployee epeEmployeeRow = PXSelect<EPEmployee,
                                        Where<
                                            EPEmployee.defContactID, Equal<Required<EPEmployee.defContactID>>>>
                                        .Select(graph, ownerID);

            if (epeEmployeeRow != null)
            {
                return epeEmployeeRow.SalesPersonID;
            }

            return null;
        }

        public static FSServiceOrder InitNewServiceOrder(string srvOrdType, string sourceType)
        {
            FSServiceOrder fsServiceOrderRow = new FSServiceOrder();
            fsServiceOrderRow.SrvOrdType = srvOrdType;
            fsServiceOrderRow.SourceType = sourceType;

            return fsServiceOrderRow;
        }

        public static FSServiceOrder GetRelatedServiceOrder(PXGraph graph, PXCache chache, IBqlTable crTable, string refNbr, string srvOrdType)
        {
            FSServiceOrder fsServiceOrderRow = null;

            if (refNbr != null && chache.GetStatus(crTable) != PXEntryStatus.Inserted)
            {
                fsServiceOrderRow = PXSelect<FSServiceOrder,
                                    Where<
                                        FSServiceOrder.refNbr, Equal<Required<FSServiceOrder.refNbr>>,
										And<FSServiceOrder.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>>>>
                                    .Select(graph, refNbr, srvOrdType);
            }

            return fsServiceOrderRow;
        }
    }
}
