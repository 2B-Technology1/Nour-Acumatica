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
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
	using static BoundedTo<DailyFieldReportEntry, DailyFieldReport>;

	public partial class DailyFieldReportEntry_Workflow : PXGraphExtension<DailyFieldReportEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<DailyFieldReportEntry, DailyFieldReport>());

		protected static void Configure(WorkflowContext<DailyFieldReportEntry, DailyFieldReport> context)
		{
			var processingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.Processing));
			var printingAndEmailingCategory = context.Categories.CreateNew(PX.Objects.PM.ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(PX.Objects.PM.ToolbarCategory.ActionCategory.PrintingAndEmailing));

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<DailyFieldReport.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<DailyFieldReportStatus.hold>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.complete, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									});
							});
							fss.Add<DailyFieldReportStatus.completed>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.hold, c => c.IsDuplicatedInToolbar());
										actions.Add(g => g.Print, c => c.IsDuplicatedInToolbar());
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<DailyFieldReportStatus.hold>(ts =>
							{
								ts.Add(t => t
									.To<DailyFieldReportStatus.completed>()
									.IsTriggeredOn(g => g.complete));
							});
							transitions.AddGroupFrom<DailyFieldReportStatus.completed>(ts =>
							{
								ts.Add(t => t
									.To<DailyFieldReportStatus.hold>()
									.IsTriggeredOn(g => g.hold));
							});
						}))
					.WithActions(actions =>
					{
						actions.Add(g => g.complete, c => c
							.InFolder(processingCategory)
							.WithPersistOptions(ActionPersistOptions.PersistBeforeAction)
							.WithFieldAssignments(fa => fa.Add<DailyFieldReport.hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.hold, c => c
							.InFolder(processingCategory)
							.WithFieldAssignments(fa => fa.Add<DailyFieldReport.hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.Print, c => c
							.InFolder(printingAndEmailingCategory));
					})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(printingAndEmailingCategory);
					}));
		}
	}

	public class DailyFieldReportEntry_Workflow_CbApi_Adapter : PXGraphExtension<DailyFieldReportEntry>
	{
		public static bool IsActive() => true;

		public override void Initialize()
		{
			base.Initialize();
			if (!Base.IsContractBasedAPI && !Base.IsImport)
				return;

			Base.RowUpdated.AddHandler<DailyFieldReport>(RowUpdated);

			void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				if (e.Row is DailyFieldReport row
					&& e.OldRow is DailyFieldReport oldRow
					&& row.Hold is bool newHold
					&& oldRow.Hold is bool oldHold
					&& newHold != oldHold)
				{
					// change it only by transition
					row.Hold = oldHold;

					Base.RowUpdated.RemoveHandler<DailyFieldReport>(RowUpdated);

					Base.OnAfterPersist += InvokeTransition;
					void InvokeTransition(PXGraph obj)
					{
						obj.OnAfterPersist -= InvokeTransition;
						(newHold ? Base.hold : Base.complete).PressImpl(internalCall: true);
					}
				}
			}
		}
	}
}
