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
	using static BoundedTo<TimeCardMaint, EPTimeCard>;

	public partial class TimeCardMaint_ApprovalWorkflow : PXGraphExtension<TimeCardMaint_Workflow, TimeCardMaint>
	{
		private class TimeCardSetupApproval : IPrefetchable
		{
			public static bool IsActive => PXDatabase.GetSlot<TimeCardSetupApproval>(nameof(TimeCardSetupApproval), typeof(EPSetup)).RequestApproval;

			private bool RequestApproval;

			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord epSetup = PXDatabase.SelectSingle<EPSetup>(new PXDataField<EPSetup.timeCardAssignmentMapID>()))
				{
					if (epSetup != null)
						RequestApproval = epSetup.GetInt32(0).HasValue;
				}
			}
		}

		protected static bool ApprovalIsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.approvalWorkflow>() && TimeCardSetupApproval.IsActive;

		[PXWorkflowDependsOnType(typeof(EPSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<TimeCardMaint, EPTimeCard>());

		protected static void Configure(WorkflowContext<TimeCardMaint, EPTimeCard> context)
		{
			var approvalCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Approval,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Approval));

			Condition Bql<T>() where T : IBqlUnary, new() => context.Conditions.FromBql<T>();
			var conditions = new
			{
				IsRejected
					= Bql<EPTimeCard.isRejected.IsEqual<True>>(),
				IsApproved
					= Bql<EPTimeCard.isApproved.IsEqual<True>>(),
				IsNotApproved
					= Bql<EPTimeCard.isApproved.IsEqual<False>>(),
				IsApprovalDisabled
					= ApprovalIsActive()
						? Bql<True.IsEqual<False>>()
						: Bql<EPTimeCard.status.IsNotIn<EPTimeCardStatusAttribute.openStatus, EPTimeCardStatusAttribute.rejectedStatus>>(),
			}.AutoNameConditions();

			var approve = context.ActionDefinitions
				.CreateExisting<TimeCardMaint_ApprovalWorkflow>(g => g.approve, a => a
					.InFolder(approvalCategory, g => g.release)
					.PlaceAfter(g => g.release)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<EPTimeCard.isApproved>(e => e.SetFromValue(true))));

			var reject = context.ActionDefinitions
				.CreateExisting<TimeCardMaint_ApprovalWorkflow>(g => g.reject, a => a
					.InFolder(approvalCategory, approve)
					.PlaceAfter(approve)
					.IsHiddenWhen(conditions.IsApprovalDisabled)
					.WithFieldAssignments(fa => fa.Add<EPTimeCard.isRejected>(e => e.SetFromValue(true))));

			var reassign = context.ActionDefinitions
				.CreateExisting(nameof(TimeCardMaint.Approval.ReassignApproval), a => a
					.WithCategory(approvalCategory)
					.PlaceAfter(reject)
					.IsHiddenWhen(conditions.IsApprovalDisabled));

			context.UpdateScreenConfigurationFor(screen =>
				screen
					.UpdateDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<EPTimeCardStatusAttribute.openStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(approve, c => c.IsDuplicatedInToolbar());
										actions.Add(reject, c => c.IsDuplicatedInToolbar());
										actions.Add(reassign);
										actions.Add(g => g.edit);
									});
							});
							fss.Add<EPTimeCardStatusAttribute.rejectedStatus>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.edit, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.UpdateGroupFrom<EPTimeCardStatusAttribute.holdStatus>(ts =>
							{
								ts.Update(t => t
									.To<EPTimeCardStatusAttribute.approvedStatus>()
									.IsTriggeredOn(g => g.submit), t => t
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.openStatus>()
									.IsTriggeredOn(g => g.submit)
									.When(conditions.IsNotApproved));
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.rejectedStatus>()
									.IsTriggeredOn(g => g.OnUpdateStatus)
									.When(conditions.IsRejected));
							});
							transitions.AddGroupFrom<EPTimeCardStatusAttribute.openStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.approvedStatus>()
									.IsTriggeredOn(approve)
									.When(conditions.IsApproved));
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.rejectedStatus>()
									.IsTriggeredOn(reject)
									.When(conditions.IsRejected));
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.holdStatus>()
									.IsTriggeredOn(g => g.edit));
							});
							transitions.AddGroupFrom<EPTimeCardStatusAttribute.rejectedStatus>(ts =>
							{
								ts.Add(t => t
									.To<EPTimeCardStatusAttribute.holdStatus>()
									.IsTriggeredOn(g => g.edit));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(approve);
						actions.Add(reject);
						actions.Add(reassign);
						actions.Update(
							g => g.edit,
							a => a.WithFieldAssignments(fa =>
							{
								fa.Add<EPTimeCard.isApproved>(f => f.SetFromValue(false));
								fa.Add<EPTimeCard.isRejected>(f => f.SetFromValue(false));
							}));
					})
					.WithCategories(categories =>
					{
						categories.Add(approvalCategory);
						categories.Update(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Approval, category => category.PlaceAfter(context.Categories.Get(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Corrections)));
					}));
		}

		public PXAction<EPTimeCard> approve;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Approve")]
		protected virtual IEnumerable Approve(PXAdapter adapter) => adapter.Get();

		public PXAction<EPTimeCard> reject;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Reject")]
		protected virtual IEnumerable Reject(PXAdapter adapter) => adapter.Get();
	}
}
