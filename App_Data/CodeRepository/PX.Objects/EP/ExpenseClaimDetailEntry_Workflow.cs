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

namespace PX.Objects.EP
{
	using static BoundedTo<ExpenseClaimDetailEntry, EPExpenseClaimDetails>;

	public partial class ExpenseClaimDetailEntry_Workflow : PXGraphExtension<ExpenseClaimDetailEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ExpenseClaimDetailEntry, EPExpenseClaimDetails>());

		protected static void Configure(WorkflowContext<ExpenseClaimDetailEntry, EPExpenseClaimDetails> context)
		{
			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();

			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));

			var conditions = new
			{
				IsOnHold
					= Bql<EPExpenseClaimDetails.hold.IsEqual<True>>(),
				IsApproved
					= Bql<EPExpenseClaimDetails.approved.IsEqual<True>>(),
				IsHoldDisabled
					= Bql<EPExpenseClaimDetails.holdClaim.IsEqual<False>
						.Or<EPExpenseClaimDetails.rejected.IsEqual<False>.And<EPExpenseClaimDetails.bankTranDate.IsNotNull>>>()
			}.AutoNameConditions();

			const string initialState = "_";

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<EPExpenseClaimDetails.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add(initialState, flowState => flowState.IsInitial(g => g.initializeState));
							fss.Add<EPExpenseClaimDetailsStatus.holdStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.Submit, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									});
							});
							fss.Add<EPExpenseClaimDetailsStatus.approvedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold);
										actions.Add(g => g.Claim, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									});
							});
							fss.Add<EPExpenseClaimDetailsStatus.releasedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom(initialState, ts =>
							{
								ts.Add(t => t
									.To<EPExpenseClaimDetailsStatus.holdStatus>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsOnHold));
								ts.Add(t => t
									.To<EPExpenseClaimDetailsStatus.approvedStatus>()
									.IsTriggeredOn(g => g.initializeState)
									.When(conditions.IsApproved));
							});
							transitions.AddGroupFrom<EPExpenseClaimDetailsStatus.holdStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPExpenseClaimDetailsStatus.approvedStatus>()
									.IsTriggeredOn(g => g.Submit));
							});
							transitions.AddGroupFrom<EPExpenseClaimDetailsStatus.approvedStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.holdStatus>()
									.IsTriggeredOn(g => g.hold));
							});
							transitions.AddGroupFrom<EPExpenseClaimDetailsStatus.releasedStatus>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						
						actions.Add(g => g.Submit, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa =>
							{
								fa.Add<EPExpenseClaimDetails.hold>(e => e.SetFromValue(false));
							}));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.IsDisabledWhen(conditions.IsHoldDisabled)
							.WithFieldAssignments(fa =>
							{
								fa.Add<EPExpenseClaimDetails.hold>(e => e.SetFromValue(true));
							}));
						actions.Add(g => g.Claim, c => c
							.InFolder(processingCategory));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
					}));
		}
	}
}
