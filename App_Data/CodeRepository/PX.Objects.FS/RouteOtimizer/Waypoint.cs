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
    public class Waypoint
    {
        //If provided, this name string can be used to identify this waypoint in the output.
        public string name { get; set; }

        //Defines waypoint latitude/longitude coordinates.
        public RouteLocation location { get; set; }

        //Defines the size of the delivery in terms of load unit types(i.e.: consumed vehicle capacity).
        //Mutually exclusive with pickupMap(each waypoint is either a delivery or a pickup, not both).
        public Capacity deliveryMap { get; set; }

        //The default amount of time in seconds which a vehicle is expected to be 
        //stationary at this waypoint (i.e.the time required for the servicing to take place).
        // 0 where not specified
        public int serviceTimeSec { get; set; }

        //Defines the priority of this waypoint where 0 is the lowest possible priority.
        //Note: Priority is only taken into account when all Waypoints cannot be serviced by the given Vehicles:
        //under such circumstances Waypoints with a higherpriority are preferred over lower-priority ones.
        public int priority { get; set; }


        //Define tag-based vehicle inclusion constraints: 
        //only vehicles defining all tags in this list(AND-criteria) are allowed to service this waypoint.
        public List<string> tagsIncludeAnd { get; set; }

        //Define tag-based vehicle inclusion constraints: 
        //all vehicles defining one or more tags in this list(ORcriteria) are allowed to service this waypoint.
        public List<string> tagsIncludeOr { get; set; }

        //Define tag-based vehicle exclusion constraints: 
        //all vehicles defining one or more tags in this list are not allowed to service this waypoint.
        public List<string> tagsExclude { get; set; }


        //If provided, defines an array of arrival time windowconstraints for this waypoint.
        //Each time window defines a time interval when the waypoint can start to be serviced.This interval does
        //not include the service time(e.g.: if service time is 30 minutes and the time window is from 10:00 to
        //11:00, then it is ok for the vehicle to arrive and start servicing the waypoint at 10:59 and complete the
        //service 30 minutes later at 11.29). If the time window must include the service time, just subtract
        //the service time from the end time of the time window(e.g.: if service time is 30 minutes and the
        //time window is from 10:00 to 11:00 and must include the service time, then change the time
        //window to be from 10:00 to 10:30). If not specified, this waypoint will always be
        //available to be serviced.
        public List<TimeWindow> timeWindows { get; set; }
    }
}
