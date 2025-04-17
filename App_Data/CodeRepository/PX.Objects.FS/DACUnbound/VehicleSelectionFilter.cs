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

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class VehicleSelectionFilter : IBqlTable
    {
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route Document ID", Enabled = false)]
        [PXSelector(typeof(FSRouteDocument.routeDocumentID), SubstituteKey = typeof(FSRouteDocument.refNbr))]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region ShowUnassignedVehicles
        public abstract class showUnassignedVehicles : PX.Data.BQL.BqlBool.Field<showUnassignedVehicles> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Show Available Vehicles for this Route only")]
        public virtual bool? ShowUnassignedVehicles { get; set; }
        #endregion
        #region VehicleTypeID
        public abstract class vehicleTypeID : PX.Data.BQL.BqlInt.Field<vehicleTypeID> { }

        [PXInt]
        public virtual int? VehicleTypeID { get; set; }
        #endregion
    }
}
