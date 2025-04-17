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

using PX.Data.WorkflowAPI;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
	using static PX.Objects.AM.AMProdItem;
	using State = ProductionOrderStatus;

	public class ProdMaint_Workflow : ProdMaintBase_Workflow<ProdMaint>
	{
		[PXWorkflowDependsOnType(typeof(AMPSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProdMaint, AMProdItem>());

		protected static void Configure(WorkflowContext<ProdMaint, AMProdItem> context)
		{
			ConfigureCommon(context);
			var conditions = context.Conditions.GetPack<Conditions>();
			context.UpdateScreenConfigurationFor(screen =>
				screen.UpdateDefaultFlow(flow => flow
					.WithFlowStates(fss => {
						fss.Update<State.planned>(FlowState =>
						{
							return FlowState.WithActions(action =>
							{
								action.Add(g => g.release, a => a.IsDuplicatedInToolbar());
								action.Add(g => g.calculatePlanCost);
								action.Add(g => g.createLinkedOrders);								
							});
						});
						fss.Update<State.hold>(FlowState =>
						{
							return FlowState.WithActions(action =>
							{
								action.Add(g => g.calculatePlanCost);
								action.Add(g => g.createLinkedOrders);
							});
						});
						fss.Update<State.released>(FlowState =>
						{
							return FlowState.WithActions(action =>
							{
								action.Add(g => g.plan);
								action.Add(g => g.releaseMaterial, a => a.IsDuplicatedInToolbar());
								action.Add(g => g.disassemble, a => a.IsDuplicatedInToolbar());
								action.Add(g => g.createMove, a => a.IsDuplicatedInToolbar());
								action.Add(g => g.cancelorder);
								action.Add(g => g.calculatePlanCost);
								action.Add(g => g.createLinkedOrders);
							});
						});
						fss.Update<State.inProcess>(FlowState =>
						{
							return FlowState
								.WithActions(action =>
								{
									action.Add(g => g.releaseMaterial, a => a.IsDuplicatedInToolbar());
									action.Add(g => g.createMove, a => a.IsDuplicatedInToolbar());
									action.Add(g => g.completeorder);
									action.Add(g => g.cancelorder);
									action.Add(g => g.calculatePlanCost);
									action.Add(g => g.createLinkedOrders);
									action.Add(g => g.disassemble, a => a.IsDuplicatedInToolbar());
								});
						});
						fss.Update<State.completed>(FlowState =>
						{
							return FlowState
								.WithActions(action =>
								{
									action.Add(g => g.closeorder);
									action.Add(g => g.lockOrder, a => a.IsDuplicatedInToolbar());
								});
						});
						fss.Update<State.locked>(FlowState =>
						{
							return FlowState
								.WithActions(action =>
								{
									action.Add(g => g.unlockOrder);
									action.Add(g => g.closeorder);
								});
						});
					})
					.WithTransitions(transitions =>
					{
						transitions.UpdateGroupFrom<State.planned>(ts =>
						{
							ts.Add(t => t
								.To<State.released>()
								.IsTriggeredOn(g => g.release)
								.When(conditions.IsReleased));
						});
						transitions.UpdateGroupFrom<State.released>(ts =>
						{
							ts.Add(t => t
								.To<State.planned>()
								.IsTriggeredOn(g => g.plan)
								.When(conditions.IsPlanned));

							ts.Add(t => t
								.To<State.cancel>()
								.IsTriggeredOn(g => g.cancelorder)
								.When(conditions.IsCancelled));

							ts.Add(t => t
								.To<State.inProcess>()
								.IsTriggeredOn(g => g.createMove)
								.When(conditions.IsInProcess));
						});
						transitions.UpdateGroupFrom<State.inProcess>(ts =>
						{
							ts.Add(t => t
								.To<State.cancel>()
								.IsTriggeredOn(g => g.cancelorder)
								.When(conditions.IsCancelled));

							ts.Add(t => t
								.To<State.completed>()
								.IsTriggeredOn(g => g.completeorder)
								.When(conditions.IsCompleted));
						});
						transitions.UpdateGroupFrom<State.locked>(ts =>
						{
							ts.Add(t => t
								.To<State.completed>()
								.IsTriggeredOn(g => g.unlockOrder)
								.When(conditions.IsCompleted));
						});
						transitions.AddGroupFrom<State.completed>(ts =>
						{
							ts.Add(t => t
								.To<State.locked>()
								.IsTriggeredOn(g => g.lockOrder)
								.When(conditions.IsLocked));
						});
					}))
				.WithActions(actions =>
				{
					actions.Add(g => g.plan, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithPersistOptions(ActionPersistOptions.NoPersist)
					.WithFieldAssignments(fas =>
					{
						fas.Add<isOpen>(f => f.SetFromValue(false));
						fas.Add<released>(f => f.SetFromValue(false));
					}));
					actions.Add(g => g.release, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithPersistOptions(ActionPersistOptions.NoPersist)
					.WithFieldAssignments(fas =>
					{
						fas.Add<isOpen>(f => f.SetFromValue(true));
						fas.Add<released>(f => f.SetFromValue(true));
					})
					.IsDisabledWhen(conditions.IsRebuildProductionBOM || conditions.IsEmptyOrder
						|| !(conditions.IsRegularOrderType || conditions.IsDisassembleOrderType)));

					actions.Add(g => g.completeorder, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithFieldAssignments(fas =>
					{
					})
					.IsDisabledWhen(conditions.IsEmptyOrder || !conditions.IsInProcess));

					actions.Add(g => g.cancelorder, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithPersistOptions(ActionPersistOptions.NoPersist)
					.WithFieldAssignments(fas =>
					{
						fas.Add<isOpen>(f => f.SetFromValue(false));
					})
					.IsDisabledWhen(conditions.IsEmptyOrder || !(conditions.IsReleased || conditions.IsInProcess)));

					actions.Add(g => g.lockOrder, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithPersistOptions(ActionPersistOptions.NoPersist)
					.WithFieldAssignments(fas =>
					{
						fas.Add<isOpen>(f => f.SetFromValue(false));
						fas.Add<locked>(f => f.SetFromValue(true));
					})
					.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsLocked || !conditions.IsCompleted || conditions.IsClosed)
					.IsHiddenWhen(!conditions.IsLockWorkflowEnabled));

					actions.Add(g => g.unlockOrder, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithFieldAssignments(fas =>
					{
						fas.Add<isOpen>(f => f.SetFromValue(true));
						fas.Add<locked>(f => f.SetFromValue(false));
					})
					.IsDisabledWhen(conditions.IsEmptyOrder || !conditions.IsLocked)
					.IsHiddenWhen(!conditions.IsLockWorkflowEnabled));

					actions.Add(g => g.closeorder, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Processing))
					.WithFieldAssignments(fas =>
					{
					})
					.IsDisabledWhen(conditions.IsEmptyOrder || (!conditions.IsLocked && conditions.IsLockWorkflowEnabled)
					|| (!conditions.IsCompleted && !conditions.IsLockWorkflowEnabled)));

					actions.Add(g => g.releaseMaterial, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Transaction))
					.IsDisabledWhen(conditions.IsEmptyOrder || !conditions.IsOpen || conditions.IsCompleted || !conditions.IsRegularOrderType));

					actions.Add(g => g.createLaborTransaction, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Transaction))
					.IsDisabledWhen(!conditions.IsOpen || conditions.IsCompleted || !conditions.IsRegularOrderType || conditions.IsEmptyOrder));

					actions.Add(g => g.createMove, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Transaction))
					.IsDisabledWhen(!conditions.IsOpen || conditions.IsCompleted || !conditions.IsRegularOrderType || conditions.IsEmptyOrder));

					actions.Add(g => g.ProductionDetails, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.CustomOther)));

					actions.Add(g => g.calculatePlanCost, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.CustomOther))
					.IsDisabledWhen(conditions.IsEmptyOrder || !(conditions.IsPlanned || conditions.IsOnHold)));

					actions.Add(g => g.createLinkedOrders, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.CustomOther))
					.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsCompleted || !conditions.IsRegularOrderType));

					actions.Add(g => g.disassemble, a => a.WithCategory(context.Categories.Get(ActionCategoryNames.Transaction))
					.IsDisabledWhen(!conditions.IsDisassembleOrderType || !conditions.IsOpen || conditions.IsEmptyOrder));

					actions.Add(g => g.printProdTicket, a => a.WithCategory(PredefinedCategory.Reports)
					.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsLocked || conditions.IsClosed));
					actions.Add(g => g.TransactionsByProductionOrderInq, a => a.WithCategory(PredefinedCategory.Inquiries)
					.PlaceAfterInCategory(g => g.InventoryAllocationDetailInq)
					.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsClosed));
					actions.Add(g => g.ViewSchedule, a => a.WithCategory(PredefinedCategory.Inquiries)
					.PlaceAfterInCategory(g => g.AttributesInq)
					.IsDisabledWhen(conditions.IsEmptyOrder));

					actions.Add(g => g.RoughCutSchedule, a => a.WithCategory(context.Categories.Get("Scheduling"))
					.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsCancelled || conditions.IsCompleted
					|| conditions.IsLocked || conditions.IsClosed));
					actions.Add(g => g.RoughCutFirm, a => a.WithCategory(context.Categories.Get("Scheduling"))
					.IsDisabledWhen(conditions.IsCancelled || conditions.IsCompleted || conditions.IsLocked || conditions.IsClosed));
					actions.Add(g => g.RoughCutUndoFirm, a => a.WithCategory(context.Categories.Get("Scheduling"))
					.IsDisabledWhen(conditions.IsCancelled || conditions.IsCompleted || conditions.IsLocked || conditions.IsClosed));

					actions.Update(g => g.AttributesInq, a => a.PlaceAfterInCategory(g => g.TransactionsByProductionOrderInq));
					actions.Update(g => g.LateAssignmentEntry, a => a.PlaceAfterInCategory(g => g.createMove));
					actions.Update(g => g.disassemble, a => a.PlaceAfterInCategory(g => g.LateAssignmentEntry));

					#region Side Panels

					actions.AddNew("ShowProdOrdDetail", a => a
						.DisplayName("Production Order Details")
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen<ProdDetail>()
							.WithIcon("visibility")
							.WithAssignments(ass =>
							{
								ass.Add(nameof(AMProdItem.OrderType), e => e.SetFromField<AMProdItem.orderType>());
								ass.Add(nameof(AMProdItem.ProdOrdID), e => e.SetFromField<AMProdItem.prodOrdID>());
							})));

					actions.AddNew("ShowCriticalMaterials", a => a
						.DisplayName("Critical Materials")
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen<CriticalMaterialsInq>()
							.WithIcon("check_circle")
							.WithAssignments(ass =>
							{
								ass.Add(nameof(AMProdItem.OrderType), e => e.SetFromField<AMProdItem.orderType>());
								ass.Add(nameof(AMProdItem.ProdOrdID), e => e.SetFromField<AMProdItem.prodOrdID>());
							})));

					actions.AddNew("ShowDetailInquiry", a => a
						.DisplayName("MRP Results by Item")
						.IsHiddenWhen(!conditions.IsMRPEnabled)
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen<MRPDetail>()
							.WithIcon("compliance")
							.WithAssignments(ass =>
							{
								ass.Add(nameof(AMProdItem.InventoryID), e => e.SetFromField<AMProdItem.inventoryID>());
								ass.Add(nameof(AMProdItem.SiteID), e => e.SetFromField<AMProdItem.siteID>());
							})));

					actions.AddNew("ShowOperationInformation", a => a
						.DisplayName("Production Order Analysis")
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen("AM0042DB")
							.WithIcon("dashboard")
							.WithAssignments(ass =>
							{
								ass.Add("ProductionOrderType", e => e.SetFromField<AMProdItem.orderType>());
								ass.Add("ProductionNbr", e => e.SetFromField<AMProdItem.prodOrdID>());
							})));

					actions.AddNew("ShowLinkedSupply", a => a
						.DisplayName("Production Order Supply Documents")
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen("AM0026SP")
							.WithIcon("flow")
							.WithAssignments(ass =>
							{
								ass.Add("OrderType", e => e.SetFromField<AMProdItem.orderType>());
								ass.Add("ProdID", e => e.SetFromField<AMProdItem.prodOrdID>());
							})));
					
					actions.AddNew("ShowProjectTask", a => a
						.DisplayName("Project Task")
						.IsHiddenWhen(!conditions.HasProjectTask || !conditions.IsProjectEnabled)
						.IsSidePanelScreen(sp => sp
							.NavigateToScreen<PM.ProjectTaskEntry>()
							.WithIcon("project")
							.WithAssignments(ass =>
							{
								ass.Add("ProjectID", e => e.SetFromField<AMProdItem.projectID>());
								ass.Add("TaskCD", e => e.SetFromField<AMProdItem.taskID>());
							})));

					#endregion
				}));
		}
    }
}
