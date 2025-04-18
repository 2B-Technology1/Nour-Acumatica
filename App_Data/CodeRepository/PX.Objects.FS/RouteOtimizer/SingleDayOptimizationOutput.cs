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
    public class SingleDayOptimizationOutput
    {
        //Unique request ID.
        //Can be used with the "export" resource to get a link to
        //a map showing the optimized routes.
        public string reqID { get; set; }

        //Can be QUEUED, PROCESSING or COMPLETED.
        //QUEUED and PROCESSING statuses only apply to asynchronous requests.
        public string status { get; set; }

        // Duration of the optimization process in seconds.
        public int elapsedSec { get; set; }

        //List of computed routes, one for each used vehicle.
        public List<Route> routes { get; set; }

        //List of OutWaypoint objects defining the waypoints that, 
        //with the given constraints, could not be reached/serviced.
        public List<OutputWaypoint> unreachedWaypoints { get; set; }

        //List of OutWaypoint objects defining the waypoints that cannot be served in any feasible solution.
        //A waypoint belongs to this list if no vehicle can serve it, even if it is the only waypoint served by the vehicles.
        //Each element represents the name of a waypoint as provided in the input(or an empty string if not provided).
        public List<OutputWaypoint> unreachableWaypoints { get; set; }

        //List of OutVehicle objects defining the unneeded vehicles.
        public List<OuputVehicle> unneededVehicles { get; set; }

        //List of warnings defined by a numeric code and a descriptive message.Warnings are issued when, for
        //example, the optimization request completes successfully but some geodesic distances are used
        //instead of actual driving durations because one or more coordinates cannot be linked to the road network.
        public List<object> warnings { get; set; }
    }

}
