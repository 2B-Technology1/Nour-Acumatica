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
    public class VehicleMaintBridge : PXGraph<VehicleMaintBridge, FSVehicle>
    {
        #region Selects
        public PXSelect<FSVehicle,
               Where<
                   FSVehicle.isVehicle, Equal<True>>> VehicleRecords;
        #endregion

        #region Event Handlers

        protected virtual void _(Events.RowSelected<FSVehicle> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSVehicle fsVehicleRow = e.Row;

            VehicleMaint graphVehicleMaint = PXGraph.CreateInstance<VehicleMaint>();

            if (fsVehicleRow.SMEquipmentID != null)
            {
                graphVehicleMaint.EPEquipmentRecords.Current = PXSelectJoin<EPEquipment,
                                                               InnerJoin<FSEquipment,
                                                                    On<
                                                                        FSEquipment.sourceID, Equal<EPEquipment.equipmentID>,
                                                                        And<FSEquipment.sourceType, Equal<FSEquipment.sourceType.Vehicle>>>>,
                                                               Where<
                                                                   FSEquipment.SMequipmentID, Equal<Required<FSEquipment.SMequipmentID>>>>
                                                               .Select(graphVehicleMaint, fsVehicleRow.SMEquipmentID);
            }

            throw new PXRedirectRequiredException(graphVehicleMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
    }
}
