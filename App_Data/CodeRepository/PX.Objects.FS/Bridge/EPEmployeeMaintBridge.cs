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
using PX.Objects.EP;

namespace PX.Objects.FS
{
    public class EPEmployeeMaintBridge : PXGraph<EPEmployeeMaintBridge, EPEmployeeFSRouteEmployee>
    {
        #region Selects
        public PXSelect<EPEmployeeFSRouteEmployee> EPEmployeeFSRouteEmployeeRecords;
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<EPEmployeeFSRouteEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPEmployeeFSRouteEmployee epEmployeeFSRouteEmployeeRow = e.Row;

            var graphEmployeeMaint = PXGraph.CreateInstance<EmployeeMaint>();

            if (epEmployeeFSRouteEmployeeRow.BAccountID != null)
            {
                graphEmployeeMaint.CurrentEmployee.Current = PXSelect<EPEmployee, 
                                                             Where<
                                                                EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>
                                                             .Select(e.Cache.Graph, epEmployeeFSRouteEmployeeRow.BAccountID);
            }

            throw new PXRedirectRequiredException(graphEmployeeMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
    }
}
