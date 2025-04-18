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
	public partial class EquipmentTimeCardMaint_Workflow : PXGraphExtension<EquipmentTimeCardMaint>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<EquipmentTimeCardMaint, EPEquipmentTimeCard>());
		protected static void Configure(WorkflowContext<EquipmentTimeCardMaint, EPEquipmentTimeCard> context)
		{
			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<EPEquipmentTimeCard.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<EPEquipmentTimeCardStatusAttribute.onHold>(flowState =>
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
							fss.Add<EPEquipmentTimeCardStatusAttribute.approved>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.edit);
									});
							});
							fss.Add<EPEquipmentTimeCardStatusAttribute.released>(flowState =>
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
							transitions.AddGroupFrom<EPEquipmentTimeCardStatusAttribute.onHold>(ts =>
							{
								ts.Add(t => t
									.To<EPEquipmentTimeCardStatusAttribute.approved>()
									.IsTriggeredOn(g => g.submit));
							});
							transitions.AddGroupFrom<EPEquipmentTimeCardStatusAttribute.approved>(ts =>
							{
								ts.Add(t => t
									.To<EPEquipmentTimeCardStatusAttribute.released>()
									.IsTriggeredOn(g => g.release));
								ts.Add(t => t
									.To<EPEquipmentTimeCardStatusAttribute.onHold>()
									.IsTriggeredOn(g => g.edit));
							});
							transitions.AddGroupFrom<EPEquipmentTimeCardStatusAttribute.released>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.submit, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<EPEquipmentTimeCard.isHold>(f => f.SetFromValue(false))));
						actions.Add(g => g.edit, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<EPEquipmentTimeCard.isHold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.correct, c => c
							.InFolder(FolderType.ActionsFolder));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<EPEquipmentTimeCard>()
							.OfEntityEvent<EPEquipmentTimeCard.Events>(e => e.UpdateStatus)
							.Is(g => g.OnUpdateStatus)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
					}));
		}
		#region Update Workflow Status
		public class PXUpdateStatus : PXSelect<EPEquipmentTimeCard>
		{
			public PXUpdateStatus(PXGraph graph)
				: base(graph)
			{
				graph.Initialized += g => g.RowUpdated.AddHandler<EPEquipmentTimeCard>((PXCache sender, PXRowUpdatedEventArgs e) =>
				{
					if (!sender.ObjectsEqual<EPEquipmentTimeCard.isRejected>(e.Row, e.OldRow))
					{
						EPEquipmentTimeCard.Events.Select(ev => ev.UpdateStatus).FireOn(g, (EPEquipmentTimeCard)e.Row);
					}
				});
			}
		}
		public PXUpdateStatus updateStatus;
		#endregion
	}
}
