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
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PM
{
	using static BoundedTo<ChangeOrderEntry, PMChangeOrder>;

	public partial class ChangeOrderEntry_Workflow : PXGraphExtension<ChangeOrderEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ChangeOrderEntry, PMChangeOrder>());

		protected static void Configure(WorkflowContext<ChangeOrderEntry, PMChangeOrder> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var correctionsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Corrections,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Corrections));
			var printingAndEmailingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.PrintingAndEmailing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMCostProjection.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ChangeOrderStatus.onHold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.removeHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.send);
										actions.Add(g => g.coCancel);
									});
							});
							fss.Add<ChangeOrderStatus.open>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.hold);
										actions.Add(g => g.send);
										actions.Add(g => g.coCancel);
									});
							});
							fss.Add<ChangeOrderStatus.closed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.send);
										actions.Add(g => g.reverse);
									});
							});
							fss.Add<ChangeOrderStatus.canceled>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold);
										actions.Add(g => g.send);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ChangeOrderStatus.onHold>(ts =>
							{
								ts.Add(t => t
									.To<ChangeOrderStatus.open>()
									.IsTriggeredOn(g => g.removeHold));
								ts.Add(t => t
									.To<ChangeOrderStatus.canceled>()
									.IsTriggeredOn(g => g.coCancel));
							});
							transitions.AddGroupFrom<ChangeOrderStatus.open>(ts =>
							{
								ts.Add(t => t
									.To<ChangeOrderStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<ChangeOrderStatus.closed>()
									.IsTriggeredOn(g => g.release));
								ts.Add(t => t
									.To<ChangeOrderStatus.canceled>()
									.IsTriggeredOn(g => g.coCancel));
							});
							transitions.AddGroupFrom<ChangeOrderStatus.canceled>(ts =>
							{
								ts.Add(t => t
									   .To<ChangeOrderStatus.onHold>()
									   .IsTriggeredOn(g => g.hold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.removeHold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMChangeOrder.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMChangeOrder.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.coCancel, c => c
							 .InFolder(processingCategory));
						actions.Add(g => g.send, c => c
							.InFolder(printingAndEmailingCategory));
						actions.Add(g => g.reverse, c => c
							.InFolder(correctionsCategory));
						actions.Add(g => g.coReport, c => c
							.InFolder(printingAndEmailingCategory));

						if (PXAccess.FeatureInstalled<FeaturesSet.changeRequest>() == true)
						{
							actions.AddNew("ChangeRequests",
								config => config.IsSidePanelScreen(
									sidePanelAction =>
										sidePanelAction.NavigateToScreen("PM3085PL")
											.WithIcon("event")
											.WithAssignments(containerFiller =>
											{
												containerFiller.Add("PMChangeRequest_changeOrderNbr", c => c.SetFromField<PMChangeOrder.refNbr>());
											}
											))
								.DisplayName("Change Requests"));
						}
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(correctionsCategory);
						categories.Add(printingAndEmailingCategory);
					}));
		}
	}

	public class ChangeOrderEntry_Workflow_CbApi_Adapter : PXGraphExtension<ChangeOrderEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<PMChangeOrder>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is PMChangeOrder row
					&& e.OldRow is PMChangeOrder oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<PMChangeOrder>(RowUpdated);

					Base.OnAfterPersist += InvokeTransition;
					void InvokeTransition(PXGraph obj)
					{
						obj.OnAfterPersist -= InvokeTransition;
						(newHold ? Base.hold : Base.removeHold).PressImpl(internalCall: true);
					}
				}
			}
		}
	}
}
