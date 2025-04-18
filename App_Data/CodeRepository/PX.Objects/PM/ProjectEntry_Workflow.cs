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
using PX.Data.WorkflowAPI;
using PX.Objects.AR;

namespace PX.Objects.PM
{
	using static BoundedTo<ProjectEntry, PMProject>;

	public partial class ProjectEntry_Workflow : PXGraphExtension<ProjectEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProjectEntry, PMProject>());

		protected static void Configure(WorkflowContext<ProjectEntry, PMProject> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var billingAndAllocationsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.BillingAndAllocations,
				category => category.DisplayName(ToolbarCategory.ActionCategory.BillingAndAllocations));
			var budgetOperationsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.BudgetOperations,
				category => category.DisplayName(ToolbarCategory.ActionCategory.BudgetOperations));
			var changeManagementCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.ChangeManagement,
				category => category.DisplayName(ToolbarCategory.ActionCategory.ChangeManagement));
			var commitmentsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Commitments,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Commitments));
			var otherCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Other,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Other));

			var suspend = context.ActionDefinitions
				.CreateExisting<ProjectEntry_Workflow>(g => g.suspend, a => a
					.InFolder(processingCategory)
					.WithFieldAssignments(fa => fa.Add<PMProject.isActive>(e => e.SetFromValue(false))));

			var cancelProject = context.ActionDefinitions
				.CreateExisting<ProjectEntry_Workflow>(g => g.cancelProject, a => a
					.InFolder(processingCategory)
					.WithFieldAssignments(fa =>
					{
						fa.Add<PMProject.isActive>(e => e.SetFromValue(false));
						fa.Add<PMProject.isCancelled>(e => e.SetFromValue(true));
					}));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMProject.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProjectStatus.planned>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.createChangeOrder);
										actions.Add(g => g.lockBudget);
										actions.Add(g => g.unlockBudget);
										actions.Add(g => g.lockCommitments);
										actions.Add(g => g.unlockCommitments);
										actions.Add(g => g.validateAddresses);
										actions.Add(g => g.validateBalance);
										actions.Add(g => g.createTemplate);
										actions.Add(g => g.runAllocation);
										actions.Add(g => g.autoBudget);
									});
							});
							fss.Add<ProjectStatus.active>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.bill, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.complete, c => c.IsDuplicatedInToolbar());
										actions.Add(cancelProject);
										actions.Add(suspend);
										actions.Add(g => g.createChangeOrder);
										actions.Add(g => g.lockBudget);
										actions.Add(g => g.unlockBudget);
										actions.Add(g => g.lockCommitments);
										actions.Add(g => g.unlockCommitments);
										actions.Add(g => g.validateAddresses);
										actions.Add(g => g.validateBalance);
										actions.Add(g => g.createTemplate);
										actions.Add(g => g.runAllocation);
										actions.Add(g => g.autoBudget);
									});
							});
							fss.Add<ProjectStatus.completed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.bill, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.validateBalance);
										actions.Add(g => g.createTemplate);
										actions.Add(g => g.runAllocation);
									})
									.WithFieldStates(fs => 
									{
										fs.AddField<PMProject.customerID>(f => f.IsDisabled());
										fs.AddField<PMProject.startDate>(f => f.IsDisabled());
										fs.AddField<PMProject.expireDate>(f => f.IsDisabled());
									});
							});
							fss.Add<ProjectStatus.cancelled>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar());
									})
									.WithFieldStates(fs => fs.AddField<PMProject.customerID>(f => f.IsDisabled()));
							});
							fss.Add<ProjectStatus.suspended>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.activate, c => c.IsDuplicatedInToolbar());
									})
									.WithFieldStates(fs => fs.AddField<PMProject.customerID>(f => f.IsDisabled()));
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ProjectStatus.planned>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(g => g.activate)
									.WithFieldAssignments(fa =>
									{
										fa.Add<PMProject.hold>(f => f.SetFromValue(false));
										fa.Add<PMProject.isActive>(e => e.SetFromValue(true));
									}));
							});
							transitions.AddGroupFrom<ProjectStatus.active>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.completed>()
									.IsTriggeredOn(g => g.complete));
								ts.Add(t => t
									.To<ProjectStatus.cancelled>()
									.IsTriggeredOn(cancelProject));
								ts.Add(t => t
									.To<ProjectStatus.suspended>()
									.IsTriggeredOn(suspend));
							});
							transitions.AddGroupFrom<ProjectStatus.completed>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(g => g.activate)
									.WithFieldAssignments(fa => fa.Add<PMProject.isActive>(e => e.SetFromValue(true))));
							});
							transitions.AddGroupFrom<ProjectStatus.cancelled>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(g => g.activate)
									.WithFieldAssignments(fa => fa.Add<PMProject.isActive>(e => e.SetFromValue(true))));
							});
							transitions.AddGroupFrom<ProjectStatus.suspended>(ts =>
							{
								ts.Add(t => t
									.To<ProjectStatus.active>()
									.IsTriggeredOn(g => g.activate)
									.WithFieldAssignments(fa => fa.Add<PMProject.isActive>(e => e.SetFromValue(true))));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.activate, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa =>
							{
								fa.Add<PMProject.hold>(f => f.SetFromValue(false));
								fa.Add<PMProject.isCompleted>(e => e.SetFromValue(false));
								fa.Add<PMProject.isCancelled>(e => e.SetFromValue(false));
							}));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMProject.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.complete, c => c
							.InFolder(processingCategory));
						//.WithFieldAssignments(fa => fa.Add<PMProject.isCompleted>(e => e.SetFromValue(true)));
						actions.Add(suspend);
						actions.Add(cancelProject);
						actions.Add(g => g.bill, c => c
							.InFolder(billingAndAllocationsCategory));
						actions.Add(g => g.lockCommitments, c => c
							.InFolder(commitmentsCategory));
						actions.Add(g => g.unlockCommitments, c => c
							.InFolder(commitmentsCategory));
						actions.Add(g => g.copyProject, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.createTemplate, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.ChangeID, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.validateAddresses, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.createChangeOrder, c => c
							.InFolder(changeManagementCategory));
						actions.Add(g => g.runAllocation, c => c
							.InFolder(billingAndAllocationsCategory));
						actions.Add(g => g.validateBalance, c => c
							.InFolder(budgetOperationsCategory));
						actions.Add(g => g.autoBudget, c => c
							.InFolder(budgetOperationsCategory));
						actions.Add(g => g.lockBudget, c => c
							.InFolder(budgetOperationsCategory));
						actions.Add(g => g.unlockBudget, c => c
							.InFolder(budgetOperationsCategory));
						actions.Add(g => g.laborCostRates, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.forecast, c => c
							.InFolder(budgetOperationsCategory));
						actions.Add(g => g.currencyRates, c => c
							.InFolder(FolderType.ReportsFolder));
						actions.Add(g => g.projectBalanceReport, c => c
							.InFolder(FolderType.ReportsFolder));

						actions.AddNew("Customers",
							config => config.IsSidePanelScreen(
								sidePanelAction =>
									sidePanelAction.NavigateToScreen<CustomerMaint>()
										.WithIcon("person")
										.WithAssignments(containerFiller =>
										{
											containerFiller.Add<Customer.acctCD>(c => c.SetFromField<PMProject.customerID>());
										}
										))
							.DisplayName("Customers"));
						actions.AddNew("ProjectTransactionDetails",
							config => config.IsSidePanelScreen(
								sidePanelAction =>
									sidePanelAction.NavigateToScreen<TransactionInquiry>()
										.WithIcon("account_details")
										.WithAssignments(containerFiller =>
										{
											containerFiller.Add<TransactionInquiry.TranFilter.projectID>(c => c.SetFromField<PMProject.contractID>());
										}
										))
							.DisplayName("Project Transaction Details"));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(billingAndAllocationsCategory);
						categories.Add(budgetOperationsCategory);
						categories.Add(changeManagementCategory);
						categories.Add(commitmentsCategory);
						categories.Add(otherCategory);
						categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(otherCategory));
					}));
		}

		public PXAction<PMProject> cancelProject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Cancel Project")]
		protected virtual IEnumerable CancelProject(PXAdapter adapter) => adapter.Get();

		public PXAction<PMProject> suspend;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Suspend Project")]
		protected virtual IEnumerable Suspend(PXAdapter adapter) => adapter.Get();
	}

	public class ProjectEntry_Workflow_CbApi_Adapter : PXGraphExtension<ProjectEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<PMProject>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is PMProject row
					&& e.OldRow is PMProject oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<PMProject>(RowUpdated);

					Base.OnAfterPersist += InvokeTransition;
					void InvokeTransition(PXGraph obj)
					{
						obj.OnAfterPersist -= InvokeTransition;
						(newHold ? Base.hold : Base.activate).PressImpl(internalCall: true);
					}
				}
			}
		}
	}
}
