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

namespace PX.Objects.FS
{
    public class LocationHelper
    {
        public static void OpenCustomerLocation(PXGraph graph, int? soID)
        {
            FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                               Where<
                                                    FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                               .Select(graph, soID);

            CustomerLocationMaint graphCustomerLocationMaint = PXGraph.CreateInstance<CustomerLocationMaint>();

            BAccount bAccountRow =
                      PXSelect<BAccount,
                      Where<
                          BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                      .Select(graph, fsServiceOrderRow.CustomerID);

            graphCustomerLocationMaint.Location.Current = graphCustomerLocationMaint.Location.Search<Location.locationID>
                                                            (fsServiceOrderRow.LocationID, bAccountRow.AcctCD);

            throw new PXRedirectRequiredException(graphCustomerLocationMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
    }
}
