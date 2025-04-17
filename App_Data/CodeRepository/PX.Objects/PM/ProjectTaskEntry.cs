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

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PM
{
	public class ProjectTaskEntry : PXGraph<ProjectTaskEntry, PMTask>
	{
		#region Extensions
		
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ProjectTaskEntry_ActivityDetailsExt_Actions : ActivityDetailsExt_Inversed_Actions<ProjectTaskEntry_ActivityDetailsExt, ProjectTaskEntry, PMTask> { }

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ProjectTaskEntry_ActivityDetailsExt : ActivityDetailsExt_Inversed<ProjectTaskEntry, PMTask>
		{
			#region Initialization

			public override Type GetLinkConditionClause() => typeof(Where<PMCRActivity.projectTaskID, Equal<Current<PMTask.taskID>>>);
			public override Type GetBAccountIDCommand() => typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMTask.customerID>>>>);

			public override string GetCustomMailTo()
			{
				PMProject current = Base.Project.Select();
				if (current == null)
					return null;

				Contact customerContact = PXSelectJoin<
						Contact,
					InnerJoin<BAccount,
						On<BAccount.defContactID, Equal<Contact.contactID>>>,
					Where<
						BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
					.Select(Base, current.CustomerID);

				if (!string.IsNullOrWhiteSpace(customerContact?.EMail))
					return PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(customerContact.EMail, customerContact.DisplayName);
				
				return null;
			}

			public override void CreateTimeActivity(PXGraph targetGraph, int classID, string activityType)
			{
				base.CreateTimeActivity(targetGraph, classID, activityType);

				PXCache timeCache = targetGraph.Caches[typeof(PMTimeActivity)];

				if (timeCache == null)
					return;

				PMTimeActivity timeActivity = (PMTimeActivity)timeCache.Current;
				if (timeActivity == null)
					return;

				bool withTimeTracking = classID != CRActivityClass.Task && classID != CRActivityClass.Event;

				timeCache.SetValue<PMTimeActivity.trackTime>(timeActivity, withTimeTracking);
				timeCache.SetValueExt<PMTimeActivity.projectID>(timeActivity, ((PMTask)Base.Caches[typeof(PMTask)].Current)?.ProjectID);
				timeCache.SetValueExt<PMTimeActivity.projectTaskID>(timeActivity, ((PMTask)Base.Caches[typeof(PMTask)].Current)?.TaskID);
			}

			#endregion

			#region Events

			protected virtual void _(Events.RowSelected<PMTask> e)
			{
				if (e.Row == null)
					return;

				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(Base);
				bool userCanAddActivity = true;

				if (project != null && project.RestrictToEmployeeList == true)
				{
					var select = new PXSelectJoin<
							EPEmployeeContract,
						InnerJoin<EPEmployee,
							On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
						Where<
							EPEmployeeContract.contractID, Equal<Current<PMTask.projectID>>,
							And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>(Base);

					EPEmployeeContract record = select.SelectSingle();
					userCanAddActivity = record != null;
				}

				Activities.AllowInsert = userCanAddActivity;
			}

			#endregion
		}

		#endregion

		#region DAC Attributes Override

		#region PMTask

		[Project(typeof(Where<PMProject.nonProject, NotEqual<True>, And<PMProject.baseType, Equal<CT.CTPRType.project>>>), DisplayName = "Project ID", IsKey = true)]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
		[PXDefault]
		protected virtual void _(Events.CacheAttached<PMTask.projectID> e) { }


		[PXDimensionSelector(ProjectTaskAttribute.DimensionName,
			typeof(Search<PMTask.taskCD, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>>>),
			typeof(PMTask.taskCD),
			typeof(PMTask.taskCD), typeof(PMTask.locationID), typeof(PMTask.description), typeof(PMTask.status), DescriptionField = typeof(PMTask.description))]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<PMTask.taskCD> e) { }

		#endregion

		#region CRCampaign

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.AccountedCampaign, Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void _(Events.CacheAttached<CRCampaign.campaignID> e) { }

		#endregion

		#region PMBudget

		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<PMTask, Where<PMTask.projectID, Equal<Current<PMBudget.projectID>>, And<PMTask.taskID, Equal<Current<PMBudget.projectTaskID>>>>>))]
		protected virtual void _(Events.CacheAttached<PMBudget.projectTaskID> e) { }

		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXParent(typeof(Select<PMTask, Where<PMTask.taskID, Equal<Current<PMForecastDetail.projectTaskID>>>>))]
		protected virtual void _(Events.CacheAttached<PMForecastDetail.projectTaskID> e) { }
		#endregion

		#region Views/Selects

		public SelectFrom<PMTask>
					.LeftJoin<PMProject>.On<PMProject.contractID.IsEqual<PMTask.projectID>>
					.Where<
						PMProject.nonProject.IsEqual<False>
						.And<PMProject.baseType.IsEqual<CT.CTPRType.project>>
						.And<PMTask.projectID.IsNull.Or<MatchUserFor<PMProject>>>>
					.View Task;

		[PXViewName(Messages.ProjectTask)]
		public PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>, And<PMTask.taskID, Equal<Current<PMTask.taskID>>>>> TaskProperties;

		public PXSelect<PMRecurringItem,
			Where<PMRecurringItem.projectID, Equal<Current<PMTask.projectID>>,
			And<PMRecurringItem.taskID, Equal<Current<PMTask.taskID>>>>> BillingItems;

		public PXSelect<PMBudget, Where<PMBudget.projectID, Equal<Current<PMTask.projectID>>, And<PMBudget.projectTaskID, Equal<Current<PMTask.taskID>>>>> TaskBudgets;

		[PXViewName(Messages.TaskAnswers)]
		public CRAttributeList<PMTask> Answers;

		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> CompanySetup;
		[PXViewName(Messages.Project)]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>> Project;

		[PXReadOnlyView]
		public PXSelect<CRCampaign, Where<CRCampaign.projectID, Equal<Current<PMTask.projectID>>, And<CRCampaign.projectTaskID, Equal<Current<PMTask.taskID>>>>> TaskCampaign;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMForecastDetail> ForecastDetails;

		#endregion


		public ProjectTaskEntry()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}
		}

		#region

		public PXAction<PMTask> activate;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Activate")]
		protected virtual IEnumerable Activate(PXAdapter adapter)
		{
			if (Task.Current != null)
			{
				if (Task.Current.StartDate == null)
				{
					Task.Current.StartDate = Accessinfo.BusinessDate;
					Task.Update(Task.Current);
				}
			}
			return adapter.Get();
		}

		public PXAction<PMTask> complete;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Complete")]
		protected virtual IEnumerable Complete(PXAdapter adapter)
		{
			if (Task.Current != null)
			{
				Task.Current.EndDate = Accessinfo.BusinessDate;
				Task.Current.CompletedPercent = Math.Max(100, Task.Current.CompletedPercent.GetValueOrDefault());
				Task.Update(Task.Current);
			}
			return adapter.Get();
		}

		public PXAction<PMTask> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMTask> cancelTask;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Cancel")]
		protected virtual IEnumerable CancelTask(PXAdapter adapter) => adapter.Get();

		#endregion

		#region Event Handlers

		protected virtual void PMTask_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<PMTask.visibleInGL>(sender, row, Setup.Current.VisibleInGL == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInAP>(sender, row, Setup.Current.VisibleInAP == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInAR>(sender, row, Setup.Current.VisibleInAR == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInSO>(sender, row, Setup.Current.VisibleInSO == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInPO>(sender, row, Setup.Current.VisibleInPO == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInTA>(sender, row, Setup.Current.VisibleInTA == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInEA>(sender, row, Setup.Current.VisibleInEA == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInIN>(sender, row, Setup.Current.VisibleInIN == true);
			PXUIFieldAttribute.SetEnabled<PMTask.visibleInCA>(sender, row, Setup.Current.VisibleInCA == true);

			PMProject project = Project.Select();
			if (project == null) return;

			string status = GetStatusFromFlags(row);
			bool projectEditable = ProjectEntry.IsProjectEditable(project);
			PXUIFieldAttribute.SetEnabled<PMTask.description>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.rateTableID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.allocationID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.billingID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.billingOption>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.completedPercent>(sender, row, row.CompletedPctMethod == PMCompletedPctMethod.Manual && status != ProjectTaskStatus.Planned && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.taxCategoryID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.approverID>(sender, row, projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.startDate>(sender, row, (status == ProjectTaskStatus.Planned || status == ProjectTaskStatus.Active) && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.endDate>(sender, row, status != ProjectTaskStatus.Completed && projectEditable);
			PXUIFieldAttribute.SetEnabled<PMTask.plannedStartDate>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.plannedEndDate>(sender, row, status == ProjectTaskStatus.Planned);
			PXUIFieldAttribute.SetEnabled<PMTask.isDefault>(sender, row, projectEditable);

			activate.SetEnabled(projectEditable);
			hold.SetEnabled(projectEditable);
			complete.SetEnabled(projectEditable);
			cancelTask.SetEnabled(projectEditable);
		}

		protected virtual void PMTask_IsActive_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null && e.NewValue != null && ((bool)e.NewValue) == true)
			{
				PMProject project = Project.Select();
				if (project != null)
				{
					if (project.IsActive == false)
					{
						sender.RaiseExceptionHandling<PMTask.status>(e.Row, e.NewValue, new PXSetPropertyException(Warnings.ProjectIsNotActive, PXErrorLevel.Warning));
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTask, PMTask.isDefault> e)
		{
			if (e.Row.IsDefault == true)
			{
				var select = new PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>>>(this);
				foreach (PMTask task in select.Select(e.Row.ProjectID))
				{
					if (task.IsDefault == true && task.TaskID != e.Row.TaskID)
					{
						Task.Cache.SetValue<PMTask.isDefault>(task, false);
						Task.Cache.SmartSetStatus(task, PXEntryStatus.Updated);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTask, PMTask.completedPctMethod> e)
		{
			OnTaskCompletedPctMethodUpdated(this, e, Task.Cache);
			Task.Cache.MarkUpdated(e.Row, assertError: true);
		}

		public static void OnTaskCompletedPctMethodUpdated(PXGraph graph, Events.FieldUpdated<PMTask, PMTask.completedPctMethod> e, PXCache cache = null)
		{
			if (e.Row.CompletedPctMethod != PMCompletedPctMethod.Manual)
			{
				decimal? completedPercent = PMTaskCompletedAttribute.CalculateTaskCompletionPercentage(graph, e.Row);
				(cache ?? e.Cache).SetValue<PMTask.completedPercent>(e.Row, completedPercent);
			}
		}

		protected virtual void PMTask_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row == null)
				return;

			if (row.IsActive == true && row.IsCancelled == false)
			{
				throw new PXException(Messages.OnlyPlannedCanbeDeleted);
			}

			//validate that all child records can be deleted:

			PMTran tran = PXSelect<PMTran, Where<PMTran.projectID, Equal<Required<PMTask.projectID>>, And<PMTran.taskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
			if (tran != null)
			{
				throw new PXException(Messages.HasTranData);
			}

			PMTimeActivity activity = PXSelect<PMTimeActivity, Where<PMTimeActivity.projectID, Equal<Required<PMTask.projectID>>, And<PMTimeActivity.projectTaskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
			if (activity != null)
			{
				throw new PXException(Messages.HasActivityData);
			}

			EP.EPTimeCardItem timeCardItem = PXSelect<EP.EPTimeCardItem, Where<EP.EPTimeCardItem.projectID, Equal<Required<PMTask.projectID>>, And<EP.EPTimeCardItem.taskID, Equal<Required<PMTask.taskID>>>>>.SelectWindowed(this, 0, 1, row.ProjectID, row.TaskID);
			if (timeCardItem != null)
			{
				throw new PXException(Messages.HasTimeCardItemData);
			}
		}



		protected virtual void PMTask_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(this);
			if (row != null && project != null)
			{
				row.CustomerID = project.CustomerID;
				row.BillingID = project.BillingID;
				row.AllocationID = project.AllocationID;
				row.DefaultSalesAccountID = project.DefaultSalesAccountID;
				row.DefaultSalesSubID = project.DefaultSalesSubID;
				row.DefaultExpenseAccountID = project.DefaultExpenseAccountID;
				row.DefaultExpenseSubID = project.DefaultExpenseSubID;
			}
		}

		protected virtual void PMTask_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null)
			{
				sender.SetDefaultExt<PMTask.visibleInAP>(row);
				sender.SetDefaultExt<PMTask.visibleInAR>(row);
				sender.SetDefaultExt<PMTask.visibleInCA>(row);
				sender.SetDefaultExt<PMTask.visibleInCR>(row);
				sender.SetDefaultExt<PMTask.visibleInTA>(row);
				sender.SetDefaultExt<PMTask.visibleInEA>(row);
				sender.SetDefaultExt<PMTask.visibleInGL>(row);
				sender.SetDefaultExt<PMTask.visibleInIN>(row);
				sender.SetDefaultExt<PMTask.visibleInPO>(row);
				sender.SetDefaultExt<PMTask.visibleInSO>(row);
				sender.SetDefaultExt<PMTask.customerID>(row);
				sender.SetDefaultExt<PMTask.locationID>(row);
				sender.SetDefaultExt<PMTask.rateTableID>(row);
			}
		}
		protected virtual void _(Events.FieldUpdated<PMRecurringItem, PMRecurringItem.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMRecurringItem.description>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.amount>(e.Row);
		}


		protected virtual void _(Events.FieldDefaulting<PMRecurringItem, PMRecurringItem.amount> e)
		{
			if (e.Row == null) return;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
			if (item != null)
			{
				e.NewValue = item.BasePrice;
			}
		}

		protected virtual void _(Events.RowSelected<PMRecurringItem> e)
		{
			if (e.Row != null && Task.Current != null)
			{
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.included>(e.Cache, e.Row, Task.Current.IsActive != true);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.accountID>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.subID>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
				PXUIFieldAttribute.SetEnabled<PMRecurringItem.subMask>(e.Cache, e.Row, e.Row.AccountSource != PMAccountSource.None);
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDateAndTime]
		[PXUIField(DisplayName = "Start Date")]
		[PXFormula(typeof(IsNull<Current<CRActivity.startDate>, Current<PMCRActivity.date>>))]
		protected virtual void _(Events.CacheAttached<PMCRActivity.startDate> e)
		{
			EPSetup setup = null;
			try
			{
				setup = Caches[typeof(EPSetup)]?.Current as EPSetup ?? new PXSetupSelect<EPSetup>(this).SelectSingle();
			}
			catch {/* SKIP */}

			var dateTimeAttribute = PXDateAndTimeAttribute.GetAttribute(e.Cache, null, nameof(PMCRActivity.startDate));
			dateTimeAttribute.InputMask = dateTimeAttribute.DisplayMask = setup?.RequireTimes == true ? "g" : "d";
		}
		#endregion

		public virtual string GetStatusFromFlags(PMTask task)
		{
			if (task == null)
				return ProjectTaskStatus.Planned;

			if (task.IsCancelled == true)
				return ProjectTaskStatus.Canceled;

			if (task.IsCompleted == true)
				return ProjectTaskStatus.Completed;

			if (task.IsActive == true)
				return ProjectTaskStatus.Active;

			return ProjectTaskStatus.Planned;
		}

		public virtual void SetFieldStateByStatus(PMTask task, string status)
		{

		}
	}
}
