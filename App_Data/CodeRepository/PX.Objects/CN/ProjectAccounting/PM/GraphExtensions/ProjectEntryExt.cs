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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }


        public PXAction<PMProject> aia;
        [PXUIField(DisplayName = "AIA Report")]
        [PXButton(DisplayOnMainToolbar = false, VisibleOnProcessingResults = false)]
        protected virtual IEnumerable Aia(PXAdapter adapter)
        {
            if (Base.Invoices.Current != null && !string.IsNullOrEmpty(Base.Invoices.Current.ProformaRefNbr))
            {
                ProformaEntry entry = PXGraph.CreateInstance<ProformaEntry>();
                ProformaEntryExt ext = entry.GetExtension<ProformaEntryExt>();
                entry.Document.Current = PXSelect<PMProforma, Where<PMProforma.refNbr, 
                    Equal<Current<PMBillingRecord.proformaRefNbr>>,
                    And<PMProforma.corrected, NotEqual<True>>>>.Select(Base);
                ext.aia.Press();
            }

            return adapter.Get();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMRevenueBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.cost>>>>>))]
        protected virtual void _(Events.CacheAttached<PMRevenueBudget.projectTaskID> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMCostBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>))]
        protected virtual void _(Events.CacheAttached<PMCostBudget.projectTaskID> e)
        {
        }

		protected virtual void _(Events.RowPersisting<PMProject> e)
		{
			Dictionary<int, PMTask> tasks = Base.Tasks.Select().RowCast<PMTask>().ToDictionary(task => task.TaskID.Value);

			foreach (PMCostBudget budget in Base.CostBudget.Cache.Inserted)
			{
				PMTask task;
				if (budget.ProjectTaskID != null && tasks.TryGetValue(budget.ProjectTaskID.Value, out task) && task.Type == ProjectTaskType.Revenue)
				{
					Base.CostBudget.Cache.RaiseException<PMCostBudget.projectTaskID>(budget, ProjectAccountingMessages.CostTaskTypeIsNotValid, task.TaskCD);
				}
			}

			foreach (PMCostBudget budget in Base.CostBudget.Cache.Updated)
			{
				PMTask task;
				if (budget.ProjectTaskID != null && tasks.TryGetValue(budget.ProjectTaskID.Value, out task) && task.Type == ProjectTaskType.Revenue)
				{
					Base.CostBudget.Cache.RaiseException<PMCostBudget.projectTaskID>(budget, ProjectAccountingMessages.CostTaskTypeIsNotValid, task.TaskCD);
				}
			}

			foreach (PMRevenueBudget budget in Base.RevenueBudget.Cache.Inserted)
			{
				PMTask task;
				if (budget.ProjectTaskID != null && tasks.TryGetValue(budget.ProjectTaskID.Value, out task) && task.Type == ProjectTaskType.Cost)
				{
					Base.RevenueBudget.Cache.RaiseException<PMRevenueBudget.projectTaskID>(budget, ProjectAccountingMessages.RevenueTaskTypeIsNotValid, task.TaskCD);
				}
			}

			foreach (PMRevenueBudget budget in Base.RevenueBudget.Cache.Updated)
			{
				PMTask task;
				if (budget.ProjectTaskID != null && tasks.TryGetValue(budget.ProjectTaskID.Value, out task) && task.Type == ProjectTaskType.Cost)
				{
					Base.RevenueBudget.Cache.RaiseException<PMRevenueBudget.projectTaskID>(budget, ProjectAccountingMessages.RevenueTaskTypeIsNotValid, task.TaskCD);
				}
			}
		}

        public PXAction<PMProject> costProjection;
        [PXUIField(DisplayName = "Cost Projection", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable CostProjection(PXAdapter adapter)
        {
            if (Base.Project.Current != null)
            {
                CostProjectionEntry graph = PXGraph.CreateInstance<CostProjectionEntry>();
                var select = new PXSelect<PMCostProjection, Where<PMCostProjection.projectID, Equal<Current<PMProject.contractID>>>, OrderBy<Desc<PMCostProjection.date>>>(Base);
                PMCostProjection exists = select.Select();
                if (exists != null)
                {
                    graph.Document.Current = exists;
                }
                else
                {
                    graph.Document.Insert();
                    graph.Document.Current.ProjectID = Base.Project.Current.ContractID;
                    graph.Document.Cache.IsDirty = false;
                }

                throw new PXRedirectRequiredException(graph, "CostProjection");
            }
            return adapter.Get();
        }

        protected virtual void _(Events.RowPersisting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskTypeUsageService = new ProjectTaskTypeUsageInConstructionValidationService();
                projectTaskTypeUsageService.ValidateProjectTaskType(args.Cache, projectTask);
            }
        }

		[Obsolete]
        protected virtual void _(Events.RowDeleting<PMTask> args)
        {
        }

        protected virtual void _(Events.RowSelected<PMBillingRecord> e)
        {
            if (e.Row != null && !string.IsNullOrEmpty(e.Row.ProformaRefNbr) && IsAIAOutdated(e.Row.ProjectID, e.Row.ProformaRefNbr))
            {
                PXUIFieldAttribute.SetWarning(e.Cache, e.Row, nameof(PMBillingRecord.ProformaRefNbr), PX.Objects.PM.Messages.AIAIsOutdated);
            }
        }

        

        protected int? outdatedForProject;
        protected HashSet<string> outdatedAIA;
        protected virtual bool IsAIAOutdated(int? projectID, string proformaRefNbr)
        {
            if (outdatedForProject != null && outdatedForProject != projectID)
            {
                outdatedForProject = null;
                outdatedAIA = null;
            }

            if (outdatedAIA == null)
            {
                var select = new PXSelect<PMProforma,
                    Where<PMProforma.projectID, Equal<Required<PMProject.contractID>>,
                    And<PMProforma.isAIAOutdated, Equal<True>>>>(Base);

                outdatedForProject = projectID;
                outdatedAIA = new HashSet<string>();
                foreach (PMProforma proforma in select.Select(projectID))
                {
                    outdatedAIA.Add(proforma.RefNbr);
                }
            }

            return outdatedAIA.Contains(proformaRefNbr);
        }

        protected virtual void _(Events.RowInserted<PMCostBudget> e)
        {
            if (Base.IsCopyPaste)
            {
                e.Cache.SetValue<PMCostBudget.costProjectionCompletedPct>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionCostAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionCostToComplete>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionQtyAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.costProjectionQtyToComplete>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.curyCostProjectionCostAtCompletion>(e.Row, 0m);
                e.Cache.SetValue<PMCostBudget.curyCostProjectionCostToComplete>(e.Row, 0m);
            }
        }
    }

    public class ProjectEntryExt_Workflow : PXGraphExtension<ProjectEntry_Workflow, ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public sealed override void Configure(PXScreenConfiguration config) =>
            Configure(config.GetScreenConfigurationContext<ProjectEntry, PMProject>());

        protected static void Configure(WorkflowContext<ProjectEntry, PMProject> context)
        {
            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen
                    .WithActions(actions =>
                    {
                        actions.Add<ProjectEntryExt>(g => g.costProjection,
                            c => c.InFolder(context.Categories.Get(ToolbarCategory.ActionCategoryNames.BudgetOperations)));
                    });
            });
        }
    }
}
