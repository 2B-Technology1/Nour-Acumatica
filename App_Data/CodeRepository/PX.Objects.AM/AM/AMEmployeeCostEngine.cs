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
using PX.Data;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.AM
{
    public class AMEmployeeCostEngine : EmployeeCostEngine
    {
        public AMEmployeeCostEngine(PXGraph graph) : base(graph)
        {
        }

        public virtual EmployeeCostEngine.Rate GetEmployeeRate(int? projectID, int? projectTaskID, int? employeeId, DateTime? date)
        {
            // When non-project cost code the task will be null, so lets only check a project when the task contains a value.
            var useProjectId = projectTaskID == null ? null : projectID;
			
			return base.GetEmployeeRate(null, useProjectId, projectTaskID, null, null, employeeId, date, graph.Accessinfo.BaseCuryID);
		}

        public virtual decimal? GetEmployeeHourlyRate(int? projectID, int? projectTaskID, int? employeeId, DateTime? date)
        {
            return GetEmployeeRate(projectID, projectTaskID, employeeId, date)?.HourlyRate;
        }

	}
}
