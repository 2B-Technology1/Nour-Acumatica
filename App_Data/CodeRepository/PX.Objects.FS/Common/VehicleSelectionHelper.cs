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
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class VehicleSelectionHelper
    {
        #region Selects
        public class VehicleRecords_View : PXSelectReadonly2<FSVehicle,
                                           LeftJoin<FSServiceVehicleType,
                                           On<
                                               FSServiceVehicleType.vehicleTypeID, Equal<FSVehicle.vehicleTypeID>>>,
                                           Where<
                                               FSVehicle.isVehicle, Equal<True>,
                                               And<FSVehicle.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>>,
                                           OrderBy<
                                               Asc<FSServiceVehicleType.priorityPreference>>>
        {
            public VehicleRecords_View(PXGraph graph) : base(graph)
            {
            }

            public VehicleRecords_View(PXGraph graph, Delegate handler) : base(graph, handler)
            {
            }
        }
        #endregion

        public static IEnumerable VehicleRecordsDelegate(PXGraph graph, SharedClasses.RouteSelected_view routeSelected, PXFilter<VehicleSelectionFilter> filter)
        {
            if (routeSelected.Current == null)
            {
                yield break;
            }

            List<object> args = new List<object>();

            PXSelectBase<FSVehicle> commandFilter = new PXSelectJoinGroupBy<FSVehicle,
                                                        LeftJoin<FSRouteDocument,
                                                        On<
                                                            FSRouteDocument.vehicleID, Equal<FSVehicle.SMequipmentID>,
                                                            And<FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>>>>,
                                                        Where<
                                                            FSVehicle.isVehicle, Equal<True>>,
                                                        Aggregate<
                                                            GroupBy<
                                                                FSVehicle.SMequipmentID>>,
                                                        OrderBy<
                                                            Asc<FSServiceVehicleType.priorityPreference>>>(graph);

            args.Add(routeSelected.Current.Date);

            if (filter.Current.ShowUnassignedVehicles == true)
            {
                commandFilter.WhereAnd<Where<FSRouteDocument.routeID, IsNull>>();
            }

            var list = commandFilter.Select(args.ToArray());

            foreach (PXResult<FSVehicle, FSRouteDocument> bqlResult in list)
            {
                FSVehicle fsEquipmentRow = (FSVehicle)bqlResult;
                FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)bqlResult;

                if (fsRouteDocumentRow != null && fsRouteDocumentRow.RouteID != null)
                {
                    fsEquipmentRow.MemUnassignedVehicle = true;
                }

                yield return bqlResult;
            }
        }
    }
}
