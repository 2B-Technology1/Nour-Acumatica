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

namespace PX.Objects.EP
{
	public partial class TimeCardMaint_Workflow : PXGraphExtension<TimeCardMaint>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<TimeCardMaint, EPTimeCard>());
		protected static void Configure(WorkflowContext<TimeCardMaint, EPTimeCard> context)
		{
			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));
			var correctionsCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Corrections,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Corrections));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<EPTimeCard.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<EPTimeCardStatusAttribute.holdStatus>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.submit, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<EPTimeCardStatusAttribute.approvedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.edit);
									});
							});
							fss.Add<EPTimeCardStatusAttribute.releasedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.correct);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<EPTimeCardStatusAttribute.holdStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.approvedStatus>()
									.IsTriggeredOn(g => g.submit));
							});
							transitions.AddGroupFrom<EPTimeCardStatusAttribute.approvedStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.releasedStatus>()
									.IsTriggeredOn(g => g.release));
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.holdStatus>()
									.IsTriggeredOn(g => g.edit));
							});
							transitions.AddGroupFrom<EPTimeCardStatusAttribute.releasedStatus>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.correct, c => c
							.InFolder(correctionsCategory));
						actions.Add(g => g.submit, c => c
							.InFolder(processingCategory));
							//.WithFieldAssignments(fa => fa.Add<EPTimeCard.isHold>(f => f.SetFromValue(false))));
						actions.Add(g => g.edit, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<EPTimeCard.isHold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<EPTimeCard>()
							.OfEntityEvent<EPTimeCard.Events>(e => e.UpdateStatus)
							.Is(g => g.OnUpdateStatus)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(correctionsCategory);
					}));
		}
		#region Update Workflow Status
		public class PXUpdateStatus : PXSelect<EPTimeCard>
		{
			public PXUpdateStatus(PXGraph graph)
				: base(graph)
			{
				graph.Initialized += g => g.RowUpdated.AddHandler<EPTimeCard>((PXCache sender, PXRowUpdatedEventArgs e) =>
				{
					if (!sender.ObjectsEqual<EPTimeCard.isRejected>(e.Row, e.OldRow))
					{
						EPTimeCard.Events.Select(ev => ev.UpdateStatus).FireOn(g, (EPTimeCard)e.Row);
					}
				});
			}
		}
		public PXUpdateStatus updateStatus;
		#endregion
	}
}
