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
using PX.Objects.PM;
using System;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.Attributes
{
    [PXRestrictor(typeof(Where<PMTask.isCancelled, Equal<False>>), PX.Objects.PM.Messages.ProjectTaskIsCanceled, typeof(PMTask.taskCD))]
    [PXRestrictor(typeof(Where<PMTask.isCompleted, Equal<False>>), PX.Objects.PM.Messages.ProjectTaskIsCompleted, typeof(PMTask.taskCD))]
    [PXRestrictor(typeof(Where<PMTaskExt.visibleInPROD, Equal<True>>), PX.Objects.PM.Messages.ProjectTaskAttributeNotSupport, typeof(PMTask.taskCD))]
    public class ActiveOrInPlanningProjectTaskForProdAttribute : ActiveOrInPlanningProjectTaskAttribute
    {
        public ActiveOrInPlanningProjectTaskForProdAttribute(Type projectID) : base(projectID)
        {
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var graph = sender.Graph;
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
