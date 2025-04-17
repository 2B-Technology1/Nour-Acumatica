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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ProjectDefinition.Workflow;
using PX.Data.WorkflowAPI;
using PX.Objects.Common.Extensions;
using PX.Objects.CR.Workflows;
using PX.Objects.CS;

namespace PX.Objects.CA
{
	using State = CABatchStatus;
	using static CABatch;
	using static BoundedTo<CABatchEntry, CABatch>;

	public partial class CABatchEntry_Workflow : PXGraphExtension<CABatchEntry>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<CABatchEntry, CABatch>());
		public class Conditions : Condition.Pack
		{
			public Condition IsOnHold => GetOrCreate(c => c.FromBql<
				hold.IsEqual<True>
			>());

			public Condition IsReleased => GetOrCreate(c => c.FromBql<
				released.IsEqual<True>
			>());

			public Condition IsExported => GetOrCreate(c => c.FromBql<
				exported.IsEqual<True>
			>());

			public Condition SkipExport => GetOrCreate(c => c.FromBql<
				skipExport.IsEqual<True>
			>());

			public Condition IsCanceled => GetOrCreate(c => c.FromBql<
				canceled.IsEqual<True>
			>());

			public Condition IsVoided => GetOrCreate(c => c.FromBql <
				voided.IsEqual<True>
			>());
		}
		protected static void Configure(WorkflowContext<CABatchEntry, CABatch> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			#region Categories
			var processingCategory = context.Categories.CreateNew(ActionCategoryNames.Processing,
				category => category.DisplayName(ActionCategory.Processing));
			var correctionsCategory = context.Categories.CreateNew(ActionCategoryNames.Corrections,
				category => category.DisplayName(ActionCategory.Corrections));
			#endregion

			const string initialState = "_";

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<status>()
					.AddDefaultFlow(flow =>
						flow
						.WithFlowStates(fss =>
						{
							fss.Add(initialState, flowState => flowState.IsInitial(g => g.initializeState));
							fss.Add<State.hold>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.releaseFromHold, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.addPayments);

									});
							});
							fss.Add<State.balanced>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.release, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.export, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.putOnHold);
										actions.Add(g => g.addPayments);

									});
							});

							fss.Add<State.exported>(flowState =>

							{
								return flowState
								.WithActions(actions =>

								{
									actions.Add(g => g.release, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
									actions.Add(g => g.cancelBatch);
									actions.Add(g => g.setBalanced);
									actions.Add(g => g.export);

								});

							});

							fss.Add<State.released>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.export, a => a.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.voidBatch);
									});
							});
							fss.Add<State.canceled>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{

									});
							});

							fss.Add<State.voided>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{

									});
							});
						}
						)
					.WithTransitions(transitions =>
					{
						transitions.AddGroupFrom(initialState, ts =>
						{
							ts.Add(t => t.To<State.hold>()
								.IsTriggeredOn(g => g.initializeState)
								.When(conditions.IsOnHold));
						});
						transitions.AddGroupFrom<State.hold>(ts =>
						{
							ts.Add(t => t.To<State.balanced>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));

							ts.Add(t => t.To<State.exported>()
								.IsTriggeredOn(g => g.releaseFromHold)
								.When(conditions.IsExported)
								.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));
						});
						transitions.AddGroupFrom<State.balanced>(ts =>
						{
							ts.Add(t => t.To<State.hold>().IsTriggeredOn(g => g.putOnHold).WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(true))));
							ts.Add(t => t.To<State.exported>().IsTriggeredOn(g => g.export).WithFieldAssignments(fas => fas.Add<exported>(f => f.SetFromValue(true))));
							ts.Add(t => t.To<State.released>().IsTriggeredOn(g => g.release));
							
						});
						transitions.AddGroupFrom<State.exported>(ts =>
						{
							ts.Add(t => t.To<State.balanced>().IsTriggeredOn(g => g.setBalanced).WithFieldAssignments(fas => fas.Add<exported>(f => f.SetFromValue(false))));
							ts.Add(t => t.To<State.canceled>().IsTriggeredOn(g => g.cancelBatch));

						});
						transitions.AddGroupFrom<State.released>(ts =>
						{
							ts.Add(t => t.To<State.voided>().IsTriggeredOn(g => g.voidBatch));

						});
					}
					))
					.WithActions(actions =>
					{
						actions.Add(g => g.initializeState, a => a.IsHiddenAlways());
						actions.Add(g => g.releaseFromHold, c => c
							.InFolder(processingCategory)
							.WithPersistOptions(ActionPersistOptions.NoPersist)
							.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(false))));
						actions.Add(g => g.setBalanced, c => c
							.InFolder(correctionsCategory)
							.WithPersistOptions(ActionPersistOptions.NoPersist)
							.WithFieldAssignments(fas => fas.Add<exported>(f => f.SetFromValue(false))));
						actions.Add(g => g.putOnHold, c => c
							.InFolder(processingCategory)
							.WithPersistOptions(ActionPersistOptions.NoPersist)
							.WithFieldAssignments(fas => fas.Add<hold>(f => f.SetFromValue(true))));
						actions.Add(g => g.export, c => c
							.InFolder(processingCategory)
							.IsDisabledWhen(conditions.SkipExport && !conditions.IsReleased && !conditions.IsCanceled && !conditions.IsVoided));
						actions.Add(g => g.release, c => c
							.InFolder(processingCategory)
							.IsDisabledWhen(!conditions.SkipExport && !conditions.IsExported && !conditions.IsCanceled && !conditions.IsVoided));
						actions.Add(g => g.cancelBatch, c => c
							.InFolder(correctionsCategory)
							.IsDisabledWhen(conditions.SkipExport));
						actions.Add(g => g.voidBatch, c => c
							.InFolder(correctionsCategory));

					})
			);
		}

		public static class ActionCategoryNames
		{
			public const string Processing = "Processing";
			public const string Corrections = "Corrections";
		}

		public static class ActionCategory
		{
			public const string Processing = "Processing";
			public const string Corrections = "Corrections";
		}
	}
}
