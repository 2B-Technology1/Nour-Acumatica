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
using PX.Objects.PM;

namespace PX.Objects.PM.ChangeRequest
{
	using static BoundedTo<ChangeRequestEntry, PMChangeRequest>;

	public partial class ChangeRequestEntry_Workflow : PXGraphExtension<ChangeRequestEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ChangeRequestEntry, PMChangeRequest>());

		protected static void Configure(WorkflowContext<ChangeRequestEntry, PMChangeRequest> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsHoldDisabled
					= Bql<PMChangeRequest.changeOrderNbr.IsNotNull.Or<PMChangeRequest.costChangeOrderNbr.IsNotNull>>(),
				IsCancelDisabled
					= Bql<PMChangeRequest.costChangeOrderNbr.IsNotNull>(),
				IsCloseDisabled
					= Bql<PMChangeRequest.costChangeOrderReleased.IsNotEqual<True>>()
			}.AutoNameConditions();

			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var printingAndEmailingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.PrintingAndEmailing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMChangeRequest.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ChangeRequestStatus.onHold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.removeHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.send);
										actions.Add(g => g.crCancel);
									});
							});
							fss.Add<ChangeRequestStatus.open>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold);
										actions.Add(g => g.createChangeOrder, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.send);
										actions.Add(g => g.close, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.crCancel);
									})
									.WithEventHandlers(handlers =>
										handlers.Add(g => g.OnClose));
							});
							fss.Add<ChangeRequestStatus.closed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.send);
									})
									.WithEventHandlers(handlers =>
										handlers.Add(g => g.OnOpen));
							});
							fss.Add<ChangeRequestStatus.cancel>(flowState =>
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
							transitions.AddGroupFrom<ChangeRequestStatus.onHold>(ts =>
							{
								ts.Add(t => t
									.To<ChangeRequestStatus.open>()
									.IsTriggeredOn(g => g.removeHold));
								ts.Add(t => t
									.To<ChangeRequestStatus.cancel>()
									.IsTriggeredOn(g => g.crCancel));
							});
							transitions.AddGroupFrom<ChangeRequestStatus.open>(ts =>
							{
								ts.Add(t => t
									.To<ChangeRequestStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<ChangeRequestStatus.closed>()
									.IsTriggeredOn(g => g.OnClose));
								ts.Add(t => t
									 .To<ChangeRequestStatus.closed>()
									 .IsTriggeredOn(g => g.close));
								ts.Add(t => t
									.To<ChangeRequestStatus.cancel>()
									.IsTriggeredOn(g => g.crCancel));
							});
							transitions.AddGroupFrom<ChangeRequestStatus.closed>(ts =>
							{
								ts.Add(t => t
									.To<ChangeRequestStatus.open>()
									.IsTriggeredOn(g => g.OnOpen));
							});
							transitions.AddGroupFrom<ChangeRequestStatus.cancel>(ts =>
							{
								ts.Add(t => t
									.To<ChangeRequestStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.removeHold, c => c
							.InFolder(processingCategory));
						//.WithFieldAssignments(fa => fa.Add<PMChangeRequest.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.IsDisabledWhen(conditions.IsHoldDisabled)
							.WithFieldAssignments(fa => fa.Add<PMChangeRequest.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.createChangeOrder, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.send, c => c
							.InFolder(printingAndEmailingCategory));
						actions.Add(g => g.crReport, c => c
							.InFolder(printingAndEmailingCategory));
						actions.Add(g => g.close, c => c.InFolder(processingCategory).IsDisabledWhen(conditions.IsCloseDisabled));
						actions.Add(g => g.crCancel, c => c.InFolder(processingCategory).IsDisabledWhen(conditions.IsCancelDisabled));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<PMChangeRequest>()
							.OfEntityEvent<PMChangeRequest.Events>(e => e.Open)
							.Is(g => g.OnOpen)
							.UsesTargetAsPrimaryEntity());
						handlers.Add(handler => handler
							.WithTargetOf<PMChangeRequest>()
							.OfEntityEvent<PMChangeRequest.Events>(e => e.Close)
							.Is(g => g.OnClose)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(printingAndEmailingCategory);
					}));
		}
	}

	public class ChangeRequestEntry_Workflow_CbApi_Adapter : PXGraphExtension<ChangeRequestEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<PMChangeRequest>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is PMChangeRequest row
					&& e.OldRow is PMChangeRequest oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<PMChangeRequest>(RowUpdated);

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
