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
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Data;
using PX.Web.UI;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Utilities
{
    public class PriorityIcon<TPriorityId> : BqlFormulaEvaluator<TPriorityId>
        where TPriorityId : IBqlField
    {
        public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> entityTypes)
        {
            var priorityId = (int?) entityTypes[typeof(TPriorityId)];
            var projectManagementClassPriority = ProjectManagementClassPriorityService
                .GetProjectManagementClassPriority(cache.Graph, priorityId);
            return projectManagementClassPriority?.IsHighestPriority == true
                ? Sprite.Control.GetFullUrl(Sprite.Control.PriorityHigh)
                : string.Empty;
        }
    }
}