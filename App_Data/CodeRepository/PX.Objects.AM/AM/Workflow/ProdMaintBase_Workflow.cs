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
using PX.Data.WorkflowAPI;
using PX.Objects.AM.Attributes;
using PX.Objects.CS;

namespace PX.Objects.AM
{
	using State = ProductionOrderStatus;
	using static AMProdItem;

	public abstract class ProdMaintBase_Workflow<TGraph> : PXGraphExtension<TGraph>
		where TGraph : ProdMaintBase<TGraph>, new()
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();

		public class Conditions : BoundedTo<TGraph, AMProdItem>.Condition.Pack
		{
			public BoundedTo<TGraph, AMProdItem>.Condition IsOnHold => GetOrCreate(c => c.FromBql<
					hold.IsEqual<True>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsOffHold => GetOrCreate(c => c.FromBql<
					hold.IsEqual<False>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsPlanned => GetOrCreate(c => c.FromBql<
					hold.IsEqual<False>.And<released.IsEqual<False>>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsReleased => GetOrCreate(c => c.FromBql<
					hold.IsEqual<False>.And<released.IsEqual<True>>.And<hasTransactions.IsEqual<False>>
						.And<completed.IsEqual<False>>.And<canceled.IsEqual<False>>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsInProcess => GetOrCreate(c => c.FromBql <
					hold.IsEqual<False>.And<released.IsEqual<True>>.And<hasTransactions.IsEqual<True>>
						.And<completed.IsEqual<False>>.And<canceled.IsEqual<False>>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsCompleted => GetOrCreate(c => c.FromBql<
					completed.IsEqual<True>.And<closed.IsEqual<False>.And<locked.IsEqual<False>>>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsClosed => GetOrCreate(c => c.FromBql<
					closed.IsEqual<True>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsCancelled => GetOrCreate(c => c.FromBql<
					canceled.IsEqual<True>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsOpen => GetOrCreate(c => c.FromBql<
					isOpen.IsEqual<True>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsRebuildProductionBOM => GetOrCreate(c => c.FromBql<
					buildProductionBom.IsEqual<True>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsEmptyOrder => GetOrCreate(c => c.FromBql<
					lastOperationID.IsNull>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsDisassembleOrderType => GetOrCreate(c => c.FromBql<
					function.IsEqual<OrderTypeFunction.disassemble>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsPlanningOrderType => GetOrCreate(c => c.FromBql<
					function.IsEqual<OrderTypeFunction.planning>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsRegularOrderType => GetOrCreate(c => c.FromBql<
					function.IsEqual<OrderTypeFunction.regular>>());
			public BoundedTo<TGraph, AMProdItem>.Condition HasProjectTask => GetOrCreate(c => c.FromBql<
					projectID.IsNotNull.And<taskID.IsNotNull>>());
			public BoundedTo<TGraph, AMProdItem>.Condition HasBOMID => GetOrCreate(c => c.FromBql<
					bOMID.IsNotNull>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsLocked => GetOrCreate(c => c.FromBql<
					locked.IsEqual<True>.And<closed.IsEqual<False>>>());

			public BoundedTo<TGraph, AMProdItem>.Condition IsLockWorkflowEnabled => GetOrCreate(c =>
					IsLockWorkflowActive() ? c.FromBql<True.IsEqual<True>>() : c.FromBql<True.IsEqual<False>>());
			public BoundedTo<TGraph, AMProdItem>.Condition IsMRPEnabled => GetOrCreate(c =>
					PXAccess.FeatureInstalled<FeaturesSet.manufacturingMRP>()
					? c.FromBql<True.IsEqual<True>>()
					: c.FromBql<True.IsEqual<False>>()
			);
			public BoundedTo<TGraph, AMProdItem>.Condition IsProjectEnabled => GetOrCreate(c =>
					PXAccess.FeatureInstalled<FeaturesSet.projectModule>()
					? c.FromBql<True.IsEqual<True>>()
					: c.FromBql<True.IsEqual<False>>()
			);
		}

		protected static void ConfigureCommon(WorkflowContext<TGraph, AMProdItem> context)
		{
			var processingCategory = context.Categories.CreateNew(ActionCategoryNames.Processing,
				category => category.DisplayName(ActionCategory.Processing));

			var replenishmentCategory = context.Categories.CreateNew(ActionCategoryNames.Replenishment,
				category => category.DisplayName(ActionCategory.Replenishment));

			var customOtherCategory = context.Categories.CreateNew(ActionCategoryNames.CustomOther,
				category => category.DisplayName(ActionCategory.Other));

			var transactionCategory = context.Categories.CreateNew(ActionCategoryNames.Transaction,
				category => category.DisplayName(ActionCategory.Transaction));

			var materialsCategory = context.Categories.CreateNew(ActionCategoryNames.Materials,
				category => category.DisplayName(ActionCategory.Materials));

			var schedulingCategory = context.Categories.CreateNew(ActionCategoryNames.Scheduling,
				category => category.DisplayName(ActionCategory.Scheduling));

			const string initialState = "_";
			var conditions = context.Conditions.GetPack<Conditions>();

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					.StateIdentifierIs<statusID>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add(initialState, FlowState => FlowState.IsInitial(g => g.initializeState));
							fss.Add<State.planned>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.hold>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.released>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));										
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.inProcess>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.putOnHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.completed>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.CloseOrderWorkflow);
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.locked>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
										action.Add(g => g.CloseOrderWorkflow);
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.cancel>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<State.closed>(FlowState =>
							{
								return FlowState
									.WithActions(action =>
									{
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom(initialState, ts =>
							{
								ts.Add(t => t
									.To<State.planned>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsPlanned));

								ts.Add(t => t.
									To<State.hold>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsOnHold));

								ts.Add(t => t
									.To<State.released>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsReleased));

								ts.Add(t => t
									.To<State.inProcess>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsInProcess));
							});
							transitions.AddGroupFrom<State.planned>(ts =>
							{
								ts.Add(t => t
									.To<State.hold>()
									.IsTriggeredOn(g => g.putOnHold)
									.When(conditions.IsOnHold));
							});
							transitions.AddGroupFrom<State.hold>(ts =>
							{
								ts.Add(t => t
									.To<State.planned>()
									.IsTriggeredOn(g => g.releaseFromHold)
									.When(conditions.IsPlanned));
								ts.Add(t => t
									.To<State.inProcess>()
									.IsTriggeredOn(g => g.releaseFromHold)
									.When(conditions.IsInProcess)
									.WithFieldAssignments(fas => fas.Add<isOpen>(f => f.SetFromValue(true))));
								ts.Add(t => t
									.To<State.released>()
									.IsTriggeredOn(g => g.releaseFromHold)
									.When(conditions.IsReleased)
									.WithFieldAssignments(fas => fas.Add<isOpen>(f => f.SetFromValue(true))));
							});
							transitions.AddGroupFrom<State.released>(ts =>
							{
								ts.Add(t => t
									.To<State.hold>()
									.IsTriggeredOn(g => g.putOnHold)
								.When(conditions.IsOnHold));
								ts.Add(t => t
									.To<State.inProcess>()
									.IsTriggeredOn(g => g.OnUpdateStatus)
									.When(conditions.IsInProcess));
								ts.Add(t => t
									.To<State.completed>()
									.IsTriggeredOn(g => g.OnUpdateStatus)
									.When(conditions.IsCompleted));
							});
							transitions.AddGroupFrom<State.inProcess>(ts =>
							{
								ts.Add(t => t
									.To<State.hold>()
									.IsTriggeredOn(g => g.putOnHold)
									.When(conditions.IsOnHold)
									.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(true))));
								ts.Add(t => t
									.To<State.completed>()
									.IsTriggeredOn(g => g.OnUpdateStatus)
									.When(conditions.IsCompleted));
							});
							transitions.AddGroupFrom<State.completed>(ts =>
							{
								ts.Add(t => t
									.To<State.closed>()
									.IsTriggeredOn(g => g.CloseOrderWorkflow)
									.When(conditions.IsClosed)
									.WithFieldAssignments(fas => fas.Add<isOpen>(f => f.SetFromValue(false))));
								ts.Add(t => t
									.To<State.inProcess>()
									.IsTriggeredOn(g => g.OnUpdateStatus)
									.When(conditions.IsInProcess)
									.WithFieldAssignments(fas => fas.Add<isOpen>(f => f.SetFromValue(true))));

							});
							transitions.AddGroupFrom<State.locked>(ts =>
							{
								ts.Add(t => t
									.To<State.closed>()
									.IsTriggeredOn(g => g.CloseOrderWorkflow)
									.When(conditions.IsClosed));
							});
						})
					)
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(transactionCategory);
						categories.Add(replenishmentCategory);
						categories.Add(customOtherCategory);
						categories.Update(FolderType.InquiriesFolder, category => category.PlaceAfter(customOtherCategory));
						categories.Add(materialsCategory);
						categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(FolderType.InquiriesFolder));
						categories.Add(schedulingCategory);
					})
					.WithActions(actions =>
					{
						actions.Add(g => g.initializeState, a => a.IsHiddenAlways());
						actions.Add(g => g.putOnHold, a => a.WithCategory(processingCategory)
							.WithFieldAssignments(fas =>
							{
								fas.Add<hold>(f => f.SetFromValue(true));
								fas.Add<isOpen>(f => f.SetFromValue(false));

							}));
						actions.Add(g => g.releaseFromHold, a => a.WithCategory(processingCategory)
							.WithFieldAssignments(fas =>
							{
								fas.Add<hold>(f => f.SetFromValue(false));
							}));

						actions.Add(g => g.CreatePurchaseOrderInq, a => a.WithCategory(replenishmentCategory)
						.IsDisabledWhen(conditions.IsClosed || conditions.IsOnHold || conditions.IsEmptyOrder || conditions.IsCancelled || conditions.IsLocked));
						actions.Add(g => g.CreateProductionOrderInq, a => a.WithCategory(replenishmentCategory)
						.IsDisabledWhen(conditions.IsClosed || conditions.IsOnHold || conditions.IsEmptyOrder || conditions.IsCancelled || conditions.IsLocked));
						
						actions.Add(g => g.InventoryAllocationDetailInq, a => a.WithCategory(PredefinedCategory.Inquiries)
						.IsDisabledWhen(conditions.IsEmptyOrder));
						actions.Add(g => g.ProductionScheduleBoardRedirect, a => a.WithCategory(PredefinedCategory.Inquiries)
						.IsDisabledWhen(conditions.IsEmptyOrder || conditions.IsCompleted || conditions.IsClosed || conditions.IsCancelled || !conditions.IsRegularOrderType));

						actions.Add(g => g.AttributesInq, a => a.WithCategory(PredefinedCategory.Inquiries)
							.PlaceAfterInCategory(g => g.InventoryAllocationDetailInq)
							.IsDisabledWhen(conditions.IsEmptyOrder));

						actions.Add(g => g.CriticalMatl, a => a.WithCategory(ActionCategoryNames.Materials)
							.IsDisabledWhen(conditions.IsEmptyOrder));

						actions.Add(g => g.LateAssignmentEntry, a => a.WithCategory(ActionCategoryNames.Materials)
							.PlaceAfterInCategory(g => g.CriticalMatl)
							.IsDisabledWhen(conditions.IsEmptyOrder || !conditions.IsRegularOrderType || conditions.IsPlanned));
						actions.Add(g => g.SetMaterialsOpen, a => a.WithCategory(ActionCategoryNames.Materials)
							.IsDisabledWhen(!conditions.IsCompleted)
							.PlaceAfterInCategory(g => g.LateAssignmentEntry));

						actions.Add(g => g.SetMaterialsCompleted, a => a.WithCategory(ActionCategoryNames.Materials)
							.PlaceAfterInCategory(g => g.SetMaterialsOpen)
							.IsDisabledWhen(!conditions.IsCompleted));

						actions.Add(g => g.CloseOrderWorkflow, a => a.IsHiddenAlways());
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<AMProdItem>()
							.OfEntityEvent<AMProdItem.Events>(e => e.UpdateStatus)
							.Is(g => g.OnUpdateStatus)
							.UsesTargetAsPrimaryEntity());
					});
			});
		}

		public static class ActionCategoryNames
		{
			public const string Processing = "Processing";
			public const string Replenishment = "Replenishment";
			public const string CustomOther = "CustomOther";
			public const string Scheduling = "Scheduling";
			public const string Transaction = "Transactions";
			public const string Materials = "Materials";
		}

		public static class ActionCategory
		{
			public const string Processing = "Processing";
			public const string Replenishment = "Replenishment";
			public const string Other = "Other";
			public const string Scheduling = "Scheduling";
			public const string Transaction = "Transactions";
			public const string Materials = "Materials";
		}

		private class LockWorkflowSetup : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<LockWorkflowSetup>(nameof(LockWorkflowSetup), typeof(AMPSetup)).IsLockWorkflowEnabled;

			private bool IsLockWorkflowEnabled;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord setup = PXDatabase.SelectSingle<AMPSetup>(new PXDataField<AMPSetup.lockWorkflowEnabled>()))
				{
					if (setup == null)
					{
						return;
					}
					IsLockWorkflowEnabled = (bool?)setup.GetBoolean(0) ?? false;
				}
			}
		}

		private static bool IsLockWorkflowActive() => LockWorkflowSetup.IsActive;
	}
}
