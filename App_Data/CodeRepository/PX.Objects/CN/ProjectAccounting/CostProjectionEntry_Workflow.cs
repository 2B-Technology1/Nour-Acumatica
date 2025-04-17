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

namespace PX.Objects.CN.ProjectAccounting
{
	using static BoundedTo<CostProjectionEntry, PMCostProjection>;

	public partial class CostProjectionEntry_Workflow : PXGraphExtension<CostProjectionEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<CostProjectionEntry, PMCostProjection>());

		protected static void Configure(WorkflowContext<CostProjectionEntry, PMCostProjection> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var otherCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Other,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Other));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMCostProjection.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<CostProjectionStatus.onHold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.refreshBudget, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.createRevision, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.removeHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));

									});
							});
							fss.Add<CostProjectionStatus.open>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.createRevision, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.hold);
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									});
							});
							fss.Add<CostProjectionStatus.released>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.createRevision, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<CostProjectionStatus.onHold>(ts =>
							{
								ts.Add(t => t
									.To<CostProjectionStatus.open>()
									.IsTriggeredOn(g => g.removeHold));
							});
							transitions.AddGroupFrom<CostProjectionStatus.open>(ts =>
							{
								ts.Add(t => t
									.To<CostProjectionStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<CostProjectionStatus.released>()
									.IsTriggeredOn(g => g.release));
							});
							transitions.AddGroupFrom<CostProjectionStatus.released>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.removeHold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMCostProjection.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMCostProjection.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.refreshBudget, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.createRevision, c => c
							.InFolder(otherCategory));
						actions.Add(g => g.costProjectionReport, c => c
							.InFolder(FolderType.ReportsFolder));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(otherCategory);
						categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(otherCategory));
					}));
		}
	}

	public class CostProjectionEntry_Workflow_CbApi_Adapter : PXGraphExtension<CostProjectionEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<PMCostProjection>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is PMCostProjection row
					&& e.OldRow is PMCostProjection oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<PMCostProjection>(RowUpdated);

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
