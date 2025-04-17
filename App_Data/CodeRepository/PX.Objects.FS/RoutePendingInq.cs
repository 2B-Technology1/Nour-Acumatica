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

namespace PX.Objects.FS
{
    public class RoutePendingInq : PXGraph<RoutePendingInq>
    {
        public RoutePendingInq()
            : base()
        {
            Routes.AllowUpdate = false;
        }
  
        #region DACFilter
        [Serializable]
        public partial class RouteWrkSheetFilter : IBqlTable
        {
            #region Date
		    public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

		    [PXDBDate]
		    [PXUIField(DisplayName = "Date")]
            [PXDefault]
		    public virtual DateTime? Date { get; set; }
		    #endregion
        }
        #endregion

        #region Filter+Select
        public PXFilter<RouteWrkSheetFilter> Filter;

        [PXFilterable]
        public PXSelectJoin<FSRouteDocument,
                InnerJoin<FSRoute,
                    On<FSRouteDocument.routeID, Equal<FSRoute.routeID>>>,
                Where2<
                    Where<
                        CurrentValue<RouteWrkSheetFilter.date>, IsNull,     
                    Or<
                        FSRouteDocument.date, Equal<CurrentValue<RouteWrkSheetFilter.date>>>>,
                    And<
                        Where<
                            FSRouteDocument.status, Equal<FSRouteDocument.status.Completed>,
                        Or<
                            FSRouteDocument.status, Equal<FSRouteDocument.status.Closed>>>>>,
                OrderBy<
                        Asc<FSRouteDocument.refNbr>>>
                Routes;
        #endregion

        #region Actions
        #region OpenRouteClosing
        public PXAction<RouteWrkSheetFilter> openRouteClosing;
        [PXUIField(DisplayName = "Open Route Closing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenRouteClosing()
        {
            if (Routes.Current != null)
            {
                RouteClosingMaint graphRouteClosingMaint = PXGraph.CreateInstance<RouteClosingMaint>();

                graphRouteClosingMaint.RouteRecords.Current = graphRouteClosingMaint.RouteRecords.Search<FSRouteDocument.refNbr>(Routes.Current.RefNbr);

                throw new PXRedirectRequiredException(graphRouteClosingMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #endregion
    }
}