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
using PX.Common;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class PmQuoteMaintExtension : PXGraphExtension<PMQuoteMaint>
    {
        [InjectDependency]
        public IProjectTaskDataProvider ProjectTaskDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual void AddingTasksToProject(PMQuote quote, ProjectEntry projectEntry,
            Dictionary<string, int> taskMap, Action<PMQuote, ProjectEntry, Dictionary<string, int>> baseHandler)
        {
            baseHandler(quote, projectEntry, taskMap);
            var quoteTasks = Base.Tasks.Select().FirstTableItems;
            var projectTasks = projectEntry.Tasks.Select().FirstTableItems;
            projectTasks.ForEach(t => CopyTaskType(t, quoteTasks));
        }

        [PXOverride]
        public virtual void RedefaultTasksFromTemplate(PMQuote quote, Action<PMQuote> baseHandler)
        {
            Base.DeleteAllTasks();

            var tasks = ProjectTaskDataProvider.GetProjectTasks(Base, quote.TemplateID);

            foreach (var task in tasks.Where(t => t.AutoIncludeInPrj == true))
            {
                Base.InsertNewTaskWithProjectTask(quote, task, (qt, pt) => { qt.Type = pt.Type; });
            }
        }

        private static void CopyTaskType(PMTask task, IEnumerable<PMQuoteTask> quoteTasks)
        {
            var relatedQuoteTask = quoteTasks.SingleOrDefault(qt => qt.TaskCD == task.TaskCD);
            task.Type = relatedQuoteTask?.Type ?? ProjectTaskType.CostRevenue;
        }
    }
}
