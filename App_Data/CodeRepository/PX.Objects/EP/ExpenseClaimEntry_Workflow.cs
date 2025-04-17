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
	public partial class ExpenseClaimEntry_Workflow : PXGraphExtension<ExpenseClaimEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ExpenseClaimEntry, EPExpenseClaim>());
		protected static void Configure(WorkflowContext<ExpenseClaimEntry, EPExpenseClaim> context)
		{
			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));
			var printingAndEmailingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.PrintingAndEmailing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<EPExpenseClaim.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<EPExpenseClaimStatus.holdStatus>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.submit, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.expenseClaimPrint, c => c.IsDuplicatedInToolbar());
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnSubmit);
										handlers.Add(g => g.OnUpdateStatus);
									});
							});
							fss.Add<EPExpenseClaimStatus.approvedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.edit);
										actions.Add(g => g.expenseClaimPrint, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<EPExpenseClaimStatus.releasedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.expenseClaimPrint, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<EPExpenseClaimStatus.holdStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPExpenseClaimStatus.approvedStatus>()
									.IsTriggeredOn(g => g.submit));
								ts.Add(t => t
									.To<EPExpenseClaimStatus.approvedStatus>()
									.IsTriggeredOn(g => g.OnSubmit));
							});
							transitions.AddGroupFrom<EPExpenseClaimStatus.approvedStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPExpenseClaimStatus.releasedStatus>()
									.IsTriggeredOn(g => g.release));
								ts.Add(t => t
									.To<EPExpenseClaimStatus.holdStatus>()
									.IsTriggeredOn(g => g.edit));
							});
							transitions.AddGroupFrom<EPExpenseClaimStatus.releasedStatus>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.submit, c => c
							.InFolder(processingCategory));
							//.WithFieldAssignments(fa => fa.Add<EPExpenseClaim.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.edit, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<EPExpenseClaim.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.expenseClaimPrint, c => c
							.InFolder(printingAndEmailingCategory));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<EPExpenseClaim>()
							.OfEntityEvent<EPExpenseClaim.Events>(e => e.Submit)
							.Is(g => g.OnSubmit)
							.UsesTargetAsPrimaryEntity()
							.WithFieldAssignments(fa => fa.Add<EPExpenseClaim.hold>(f => f.SetFromValue(false))));
						handlers.Add(handler => handler
							.WithTargetOf<EPExpenseClaim>()
							.OfEntityEvent<EPExpenseClaim.Events>(e => e.UpdateStatus)
							.Is(g => g.OnUpdateStatus)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(printingAndEmailingCategory);
					}));
		}
		#region Update Workflow Status
		public class PXUpdateStatus : PXSelect<EPExpenseClaim>
		{
			public PXUpdateStatus(PXGraph graph)
				: base(graph)
			{
				graph.Initialized += g => g.RowUpdated.AddHandler<EPExpenseClaim>((PXCache sender, PXRowUpdatedEventArgs e) =>
				{
					if (!sender.ObjectsEqual<EPExpenseClaim.rejected>(e.Row, e.OldRow))
					{
						EPExpenseClaim.Events.Select(ev => ev.UpdateStatus).FireOn(g, (EPExpenseClaim)e.Row);
					}
				});
			}
		}
		public PXUpdateStatus updateStatus;
		#endregion
	}
}
