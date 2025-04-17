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

namespace PX.Objects.FS
{
    public class ServiceManagementSetup
    {
        /// <summary>
        /// Returns FSSetup record.
        /// </summary>
        public static FSSetup GetServiceManagementSetup(PXGraph graph)
        {
            return PXSelect<FSSetup>.Select(graph);
        }

        /// <summary>
        /// Returns FSRouteSetup record.
        /// </summary>
        public static FSRouteSetup GetServiceManagementRouteSetup(PXGraph graph)
        {
            return PXSelect<FSRouteSetup>.Select(graph);
        }

        /// <summary>
        /// Return if ManageRooms is active or inactive on the Service Management.
        /// </summary>
        public static bool IsRoomManagementActive(PXGraph graph, FSSetup fsSetupRow = null)
        {
            if (fsSetupRow == null)
            {
                fsSetupRow = GetServiceManagementSetup(graph);
            }

            if (fsSetupRow != null) 
            {
                return (bool)fsSetupRow.ManageRooms;
            }

            return false;
        }
    }
}
