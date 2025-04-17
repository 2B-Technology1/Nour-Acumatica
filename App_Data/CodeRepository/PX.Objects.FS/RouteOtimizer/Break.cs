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
    public class Break
    {
        //Minimum allowed break start time in seconds since midnight (00:00).
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int startTimeSec { get; set; }

        //Maximum allowed break stop time in seconds since midnight(00:00).
        //Must be >= startTimeSec.
        //The allowed range can span a maximum of a 7-days period[0 – 604800].
        public int stopTimeSec { get; set; }

        //Break duration in seconds.
        //The break can be scheduled to start at any time between startTimeSec and stopTimeSec, and lasts for durationSec.
        //Example: startTimeSec = 43200 (12:00), stopTimeSec = 45000 (12:30) 
        //and durationSec = 3600(1 hour) means: schedule a 1-hour break starting at any time between 12:00 and 12:30.
        public int durationSec { get; set; }
    }
}
