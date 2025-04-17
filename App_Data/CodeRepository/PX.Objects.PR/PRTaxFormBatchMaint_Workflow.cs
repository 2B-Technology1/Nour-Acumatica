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

using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PR
{
	using static BoundedTo<PRTaxFormBatchMaint, PRTaxFormBatch>;
	using static PX.Objects.PR.PRTaxFormBatch;

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class PRTaxFormBatchMaint_Workflow : PXGraphExtension<PRTaxFormBatchMaint>
	{
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<PRTaxFormBatchMaint, PRTaxFormBatch>());

		public class Conditions : Condition.Pack
		{
			public Condition NonePublished =>
				GetOrCreate(c => c.FromBql<numberOfPublishedEmployees.IsEqual<int0>>());

			public Condition AllPublished =>
				GetOrCreate(c => c.FromBql<numberOfEmployees.IsEqual<numberOfPublishedEmployees>>());
		}

		protected static void Configure(WorkflowContext<PRTaxFormBatchMaint, PRTaxFormBatch> context)
		{
			var processingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Processing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Processing));
			var downloadAndPrintingCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.PrintingAndEmailing,
				category => category.DisplayName(ToolbarCategory.ActionCategory.PrintingAndEmailing));
			var correctionsCategory = context.Categories.CreateNew(ToolbarCategory.ActionCategoryNames.Corrections,
				category => category.DisplayName(ToolbarCategory.ActionCategory.Corrections));

			var conditions = context.Conditions.GetPack<Conditions>();

			context.AddScreenConfigurationFor(screen =>
				screen
					.StateIdentifierIs<PRTaxFormBatch.status>()
					.AddDefaultFlow(flow => flow
						.WithFlowStates(fss =>
						{
							fss.Add<TaxFormBatchStatus.nonePublished>(flowState =>
							{
								return flowState
									.IsInitial()
									.WithActions(actions =>
									{
										actions.Add(g => g.PublishAll, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.DownloadXml);
										actions.Add(g => g.PrintAll);
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnTaxFormPublished);
									});
							});
							fss.Add<TaxFormBatchStatus.somePublished>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.DownloadXml, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.PrintAll, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.PublishAll);
										actions.Add(g => g.UnpublishAll);
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnTaxFormPublished);
										handlers.Add(g => g.OnTaxFormUnpublished);
									});
							});
							fss.Add<TaxFormBatchStatus.allPublished>(flowState =>
							{
								return flowState
									.WithActions(actions =>
									{
										actions.Add(g => g.DownloadXml, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.PrintAll, c => c.IsDuplicatedInToolbar().WithConnotation(ActionConnotation.Success));
										actions.Add(g => g.UnpublishAll);
									})
									.WithEventHandlers(handlers =>
									{
										handlers.Add(g => g.OnTaxFormUnpublished);
									});
							});
						})
						.WithTransitions(transitions =>
						{
							transitions.AddGroupFrom<TaxFormBatchStatus.nonePublished>(ts =>
							{
								ts.Add(t => t
									.To<TaxFormBatchStatus.allPublished>()
									.IsTriggeredOn(g => g.PublishAll));
								ts.Add(t => t
									.To<TaxFormBatchStatus.allPublished>()
									.IsTriggeredOn(g => g.OnTaxFormPublished)
									.When(conditions.AllPublished));
								ts.Add(t => t
									.To<TaxFormBatchStatus.somePublished>()
									.IsTriggeredOn(g => g.OnTaxFormPublished)
									.When(!conditions.AllPublished));
							});
							transitions.AddGroupFrom<TaxFormBatchStatus.somePublished>(ts =>
							{
								ts.Add(t => t
									.To<TaxFormBatchStatus.allPublished>()
									.IsTriggeredOn(g => g.PublishAll));
								ts.Add(t => t
									.To<TaxFormBatchStatus.nonePublished>()
									.IsTriggeredOn(g => g.UnpublishAll));
								ts.Add(t => t
									.To<TaxFormBatchStatus.allPublished>()
									.IsTriggeredOn(g => g.OnTaxFormPublished)
									.When(conditions.AllPublished));
								ts.Add(t => t
									.To<TaxFormBatchStatus.nonePublished>()
									.IsTriggeredOn(g => g.OnTaxFormUnpublished)
									.When(conditions.NonePublished));
							});
							transitions.AddGroupFrom<TaxFormBatchStatus.allPublished>(ts =>
							{
								ts.Add(t => t
									.To<TaxFormBatchStatus.nonePublished>()
									.IsTriggeredOn(g => g.UnpublishAll));
								ts.Add(t => t
									.To<TaxFormBatchStatus.nonePublished>()
									.IsTriggeredOn(g => g.OnTaxFormUnpublished)
									.When(conditions.NonePublished));
								ts.Add(t => t
									.To<TaxFormBatchStatus.somePublished>()
									.IsTriggeredOn(g => g.OnTaxFormUnpublished)
									.When(!conditions.NonePublished));
							});
						}))
						.WithActions(actions =>
						{
							actions.Add(g => g.DownloadXml, c => c.WithCategory(downloadAndPrintingCategory));
							actions.Add(g => g.PrintAll, c => c.WithCategory(downloadAndPrintingCategory));
							actions.Add(g => g.PublishAll, c => c.WithCategory(processingCategory));
							actions.Add(g => g.UnpublishAll, c => c.WithCategory(correctionsCategory));
						})
						.WithHandlers(handlers =>
						{
							handlers.Add(handler => handler
								.WithTargetOf<PRTaxFormBatch>()
								.OfEntityEvent<PRTaxFormBatch.Events>(e => e.PublishTaxForm)
								.Is(g => g.OnTaxFormPublished)
								.UsesTargetAsPrimaryEntity());
							handlers.Add(handler => handler
								.WithTargetOf<PRTaxFormBatch>()
								.OfEntityEvent<PRTaxFormBatch.Events>(e => e.UnPublishTaxForm)
								.Is(g => g.OnTaxFormUnpublished)
								.UsesTargetAsPrimaryEntity());
						})
					.WithCategories(categories =>
					{
						categories.Add(processingCategory);
						categories.Add(downloadAndPrintingCategory);
						categories.Add(correctionsCategory);
					}));
		}
	}
}
