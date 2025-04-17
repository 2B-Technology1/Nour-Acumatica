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
using PX.Objects.CT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
	public class PMTaskCompletedAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		public PMTaskCompletedAttribute()
		{
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null)
			{
				int? projectID = null;
				int? taskID = null;
				if (e.Row is PMBudget)
				{
					PMBudget row = e.Row as PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
				}
				else if (e.Row is Lite.PMBudget)
				{
					Lite.PMBudget row = e.Row as Lite.PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
				}
				if (projectID != null && taskID != null)
				{
					CalculateTaskCompleted(sender.Graph, projectID.Value, taskID.Value, false);
				}
			}
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Row != null)
			{
				int? projectID = null;
				int? taskID = null;
				if (e.Row is PMBudget)
				{
					PMBudget row = e.Row as PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
				}
				else if (e.Row is Lite.PMBudget)
				{
					Lite.PMBudget row = e.Row as Lite.PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
				}
				if (projectID != null && taskID != null)
				{
					if (!(e.Row is PMCostBudget))
					{
						sender.Graph.Caches<PMCostBudget>().ClearQueryCache();
						sender.Graph.Caches<PMCostBudget>().Clear();
						CalculateTaskCompleted(sender.Graph, projectID.Value, taskID.Value, true);
					}
				}
			}
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row != null)
			{
				int? projectID = null;
				int? taskID = null;
				bool? isProduction = null;
				if (e.Row is PMBudget)
				{
					PMBudget row = e.Row as PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
					isProduction = row.IsProduction;
				}
				else if (e.Row is Lite.PMBudget)
				{
					Lite.PMBudget row = e.Row as Lite.PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
					isProduction = row.IsProduction;
				}
				if (projectID != null && taskID != null && isProduction == true)
				{
					CalculateTaskCompleted(sender.Graph, projectID.Value, taskID.Value, false);
				}
			}
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row != null)
			{
				int? projectID = null;
				int? taskID = null;
				bool? isProduction = null;
				if (e.Row is PMBudget)
				{
					PMBudget row = e.Row as PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
					isProduction = row.IsProduction;
				}
				else if (e.Row is Lite.PMBudget)
				{
					Lite.PMBudget row = e.Row as Lite.PMBudget;
					projectID = row.ProjectID;
					taskID = row.ProjectTaskID;
					isProduction = row.IsProduction;
				}
				if (projectID != null && taskID != null && isProduction == true)
				{
					CalculateTaskCompleted(sender.Graph, projectID.Value, taskID.Value, false);
				}
			}
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.OldRow is PMBudget && e.Row is PMBudget)
			{
				PMBudget oldRow = (PMBudget)e.OldRow;
				PMBudget newRow = (PMBudget)e.Row;
				if (oldRow != null && newRow != null && newRow.ProjectID != null)
				{
					if (oldRow.ProjectTaskID != newRow.ProjectTaskID)
					{
						if (oldRow.ProjectTaskID != null)
						{
							CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, oldRow.ProjectTaskID.Value, false);
						}
						if (newRow.ProjectTaskID != null)
						{
							CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, newRow.ProjectTaskID.Value, false);
						}
					}
					else if (newRow.ProjectTaskID != null &&
							 (oldRow.IsProduction != newRow.IsProduction ||
							 oldRow.ActualQty != newRow.ActualQty ||
							 oldRow.RevisedQty != newRow.RevisedQty ||
							 oldRow.CuryActualAmount != newRow.CuryActualAmount ||
							 oldRow.CuryRevisedAmount != newRow.CuryRevisedAmount ||
							 oldRow.UOM != newRow.UOM ||
							 oldRow.AccountGroupID != newRow.AccountGroupID ||
							 oldRow.InventoryID != newRow.InventoryID))
					{
						CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, newRow.ProjectTaskID.Value, false);
					}
				}
			}
			else if (e.OldRow is Lite.PMBudget && e.Row is Lite.PMBudget)
			{
				Lite.PMBudget oldRow = (Lite.PMBudget)e.OldRow;
				Lite.PMBudget newRow = (Lite.PMBudget)e.Row;
				if (oldRow != null && newRow != null && newRow.ProjectID != null)
				{
					if (oldRow.ProjectTaskID != newRow.ProjectTaskID)
					{
						if (oldRow.ProjectTaskID != null)
						{
							CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, oldRow.ProjectTaskID.Value, false);
						}
						if (newRow.ProjectTaskID != null)
						{
							CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, newRow.ProjectTaskID.Value, false);
						}
					}
					else if (newRow.ProjectTaskID != null &&
							 (oldRow.IsProduction != newRow.IsProduction ||
							 oldRow.CuryActualAmount != newRow.CuryActualAmount ||
							 oldRow.CuryRevisedAmount != newRow.CuryRevisedAmount ||
							 oldRow.UOM != newRow.UOM ||
							 oldRow.AccountGroupID != newRow.AccountGroupID ||
							 oldRow.InventoryID != newRow.InventoryID))
					{
						CalculateTaskCompleted(sender.Graph, newRow.ProjectID.Value, newRow.ProjectTaskID.Value, false);
					}
				}
			}
		}

		protected void CalculateTaskCompleted(PXGraph graph, int projectID, int taskID, bool inPersisted)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>
				.Select(graph, projectID);
			if (project != null && project.BaseType == CTPRType.Project)
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>
					.Select(graph, projectID, taskID);
				if (task != null &&
					(task.CompletedPctMethod == PMCompletedPctMethod.ByAmount ||
						task.CompletedPctMethod == PMCompletedPctMethod.ByQuantity))
				{
					decimal completedPercent = CalculateTaskCompletionPercentage(graph, task);
					if (task.Status == ProjectTaskStatus.Completed)
					{
						completedPercent = Math.Max(100, completedPercent);
					}

					if (task.CompletedPercent != completedPercent)
					{
						if (inPersisted)
						{
							graph.Caches<PMTask>().SetValue<PMTask.completedPercent>(task, completedPercent);
							graph.Caches<PMTask>().SetStatus(task, PXEntryStatus.Updated);
						}
						else
						{
							task.CompletedPercent = completedPercent;
							graph.Caches<PMTask>().Update(task);
						}
					}
				}
			}
		}

		public static decimal CalculateTaskCompletionPercentage(PXGraph graph, PMTask task)
		{
			decimal result = 0;

			IEnumerable<PMCostBudget> budgets = PXSelect<PMCostBudget,
				Where<PMCostBudget.projectID, Equal<Required<PMTask.projectID>>,
				And<PMCostBudget.projectTaskID, Equal<Required<PMTask.taskID>>,
				And<PMCostBudget.isProduction, Equal<True>>>>>.Select(graph, task.ProjectID, task.TaskID).RowCast<PMCostBudget>();

			if (budgets != null)
			{
				double percentSum = 0;
				int recordCount = 0;
				decimal actualAmount = 0;
				decimal budgetedAmount = 0;
				foreach (IGrouping<string, PMCostBudget> item in budgets.GroupBy(c => GetGroupKey(c)))
				{
					if (task.CompletedPctMethod == PMCompletedPctMethod.ByQuantity)
					{
						decimal revisedQty = item.Sum(c => c.RevisedQty.GetValueOrDefault(0));
						if (revisedQty != 0)
						{
							recordCount++;
							percentSum += Convert.ToDouble(100 * item.Sum(c => c.ActualQty) / revisedQty);
						}
					}
					else if (task.CompletedPctMethod == PMCompletedPctMethod.ByAmount)
					{
						actualAmount += item.Sum(c => c.CuryActualAmount.GetValueOrDefault(0));
						budgetedAmount += item.Sum(c => c.CuryRevisedAmount.GetValueOrDefault(0));
					}
				}
				if (task.CompletedPctMethod == PMCompletedPctMethod.ByAmount)
				{
					result = budgetedAmount == 0 ? 0 : Convert.ToDecimal(100 * actualAmount / budgetedAmount);
				}
				else
				{
					result = recordCount == 0 ? 0 : Convert.ToDecimal(percentSum / recordCount);
				}
			}

			return result;
		}

		protected static string GetGroupKey(PMCostBudget budget)
		{
			string result = string.Empty;

			if (budget.AccountGroupID == null)
			{
				result += "null";
			}
			else
			{
				result += budget.AccountGroupID.ToString();
			}
			result += "_";

			if (budget.InventoryID == null)
			{
				result += "null";
			}
			else
			{
				result += budget.InventoryID.ToString();
			}
			result += "_" + budget.UOM;

			return result;
		}
	}
}
