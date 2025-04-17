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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.FS.RouteOtimizer
{
    public class Route
    {
        //The OutVehicle object representing the vehicle associated with this route.
        public OuputVehicle vehicle { get; set; }

        //Ordered list of route steps.Route steps include one origin, one or more waypoints, 
        //zero or one destination(route ends at the last visited waypoint if no destination is
        //specified for the vehicle).
        public List<RouteStep> steps { get; set; }

        //Cost and time statistics related to this route
        public RouteStats routeStats { get; set; }

        //Total cost of the route: sum of total driving cost, total
        //mileage cost, total idle time cost, total break time cost,
        //total service time cost and total enter/exit cluster cost.
        public double cost { get; set; }
    }
}
