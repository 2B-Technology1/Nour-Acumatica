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
using PX.Objects.CR;

namespace PX.Objects.FS
{
  [System.SerializableAttribute]
  public class RouteAppointmentGPSLocationFilter : PX.Data.IBqlTable
  {
        #region ServiceID
        public abstract class serviceID : PX.Data.BQL.BqlInt.Field<serviceID> { }
  
        [PXInt]
        [ServiceAttribute(DisplayName = "Service")]
        public virtual int? ServiceID { get; set; }
        #endregion
        #region DateFrom
        public abstract class dateFrom : PX.Data.BQL.BqlDateTime.Field<dateFrom> { }
  
        [PXDate]
        [PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DateFrom { get; set; }
        #endregion
        #region DateTo
        public abstract class dateTo : PX.Data.BQL.BqlDateTime.Field<dateTo> { }
  
        [PXDate]
        [PXUIField(DisplayName = "To Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DateTo { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorBAccountTypeCustomerOrCombined]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

        [PXDBInt]
        [PXFormula(typeof(Default<RouteAppointmentGPSLocationFilter.customerID>))]
        [PXSelector(typeof(Search<Location.locationID,
                        Where<
                            Location.bAccountID, Equal<Current<RouteAppointmentGPSLocationFilter.customerID>>>>), 
        SubstituteKey = typeof(Location.locationCD), DescriptionField = typeof(Location.descr))]
        [PXUIField(DisplayName = "Customer Location")]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #region RouteID
        public abstract class routeID : PX.Data.BQL.BqlInt.Field<routeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Route")]
        [FSSelectorRouteID]
        public virtual int? RouteID { get; set; }
        #endregion
        #region RouteDocumentID
        public abstract class routeDocumentID : PX.Data.BQL.BqlInt.Field<routeDocumentID> { }

        [PXInt]
        [PXFormula(typeof(Default<RouteAppointmentGPSLocationFilter.routeID>))]
        [PXSelector(typeof(Search<FSRouteDocument.routeDocumentID,
                        Where<
                            FSRouteDocument.routeID, Equal<Current<RouteAppointmentGPSLocationFilter.routeID>>>>), SubstituteKey = typeof(FSRouteDocument.refNbr))]
        [PXUIField(DisplayName = "Route Nbr.")]
        public virtual int? RouteDocumentID { get; set; }
        #endregion
        #region LoadData
        public abstract class loadData : PX.Data.BQL.BqlBool.Field<loadData> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Generate", Visible = false)]
        public virtual bool? LoadData { get; set; }
        #endregion
    }
}