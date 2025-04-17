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
using PX.Objects.CN.ProjectAccounting.PM.GraphExtensions;

namespace PX.Objects.PM
{
	using static BoundedTo<ProgressWorksheetEntry, PMProgressWorksheet>;

	public partial class ProgressWorksheetEntry_Workflow : PXGraphExtension<ProgressWorksheetEntry>
	{
		public static bool IsActive() => true;

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<ProgressWorksheetEntry, PMProgressWorksheet>());

		protected static void Configure(WorkflowContext<ProgressWorksheetEntry, PMProgressWorksheet> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var correctionsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Corrections,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Corrections));
			var printingAndEmailingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.PrintingAndEmailing));
			var otherCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Other,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Other));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PMProgressWorksheet.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<ProgressWorksheetStatus.onHold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.removeHold, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									});
							});
							fss.Add<ProgressWorksheetStatus.open>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.hold);
									})
									.WithEventHandlers(handlers =>
										handlers.Add(g => g.OnRelease));
							});
							fss.Add<ProgressWorksheetStatus.closed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.reverse);
										actions.Add(g => g.correct);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<ProgressWorksheetStatus.onHold>(ts =>
							{
								ts.Add(t => t
									.To<ProgressWorksheetStatus.open>()
									.IsTriggeredOn(g => g.removeHold));
							});
							transitions.AddGroupFrom<ProgressWorksheetStatus.open>(ts =>
							{
								ts.Add(t => t
									.To<ProgressWorksheetStatus.onHold>()
									.IsTriggeredOn(g => g.hold));
								ts.Add(t => t
									.To<ProgressWorksheetStatus.closed>()
									.IsTriggeredOn(g => g.release));
								ts.Add(t => t
									.To<ProgressWorksheetStatus.closed>()
									.IsTriggeredOn(g => g.OnRelease));
							});
							transitions.AddGroupFrom<ProgressWorksheetStatus.closed>(ts =>
							{
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.removeHold, c => c
							.InFolder(processingCategory)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
							.WithFieldAssignments(fa => fa.Add<PMProgressWorksheet.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<PMProgressWorksheet.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory));
						actions.Add(g => g.reverse, c => c
							.InFolder(correctionsCategory));
						actions.Add(g => g.correct, c => c
							.InFolder(correctionsCategory));
						actions.Add(g => g.print, c => c
							.InFolder(printingAndEmailingCategory));
					})
					.WithHandlers(handlers =>
					{
						handlers.Add(handler => handler
							.WithTargetOf<PMProgressWorksheet>()
							.OfEntityEvent<PMProgressWorksheet.Events>(e => e.Release)
							.Is(g => g.OnRelease)
							.UsesTargetAsPrimaryEntity());
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(correctionsCategory);
						categories.Add(printingAndEmailingCategory);
						categories.Add(otherCategory);
						categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(otherCategory));
					}));
		}
	}

	public class ProgressWorksheetEntry_Workflow_CbApi_Adapter : PXGraphExtension<ProgressWorksheetEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<PMProgressWorksheet>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is PMProgressWorksheet row
					&& e.OldRow is PMProgressWorksheet oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<PMProgressWorksheet>(RowUpdated);

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
