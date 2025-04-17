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

using System.Collections;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    /// <summary>
    /// Bill of Material Maintenance graph
    /// Main graph for managing a Bill of Material (BOM)
    /// </summary>
    public class BOMMaint : BOMBaseGraph<BOMMaint>
    {
		public new PXSave<AMBomItem> Save;
		public new PXRevisionableCancel<BOMMaint, AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Cancel;
		public new PXRevisionableInsert<AMBomItem> Insert;
		public new PXDelete<AMBomItem> Delete;
		public new PXRevisionableFirst<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> First;
		public new PXRevisionablePrevious<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Previous;
		public new PXRevisionableNext<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Next;
		public new PXRevisionableLast<AMBomItem, AMBomItem.bOMID, AMBomItem.revisionID> Last;

		public PXInitializeState<AMBomItem> initializeState;

		/// <summary>
		/// Redirect to this graph for the given BOM
		/// </summary>
		public static void Redirect(string bomId)
        {
            Redirect(bomId, null);
        }

        /// <summary>
        /// Redirect to this graph for the given BOM / effective date
        /// </summary>
        public static void Redirect(string bomId, string revisionId)
        {
            if (string.IsNullOrWhiteSpace(bomId))
            {
                return;
            }

            var graph = CreateInstance<BOMMaint>();

            var bomItem = revisionId == null
                ? PrimaryBomIDManager.GetNotArchivedRevisionBomItem(graph, bomId)
                : (AMBomItem) PXSelect<
                        AMBomItem,
                        Where<AMBomItem.bOMID, Equal<Required<AMBomItem.bOMID>>,
                            And<AMBomItem.revisionID, Equal<Required<AMBomItem.revisionID>>>>>
                    .SelectWindowed(graph, 0, 1, bomId, revisionId);

            if (bomItem == null)
            {
                PXTrace.WriteInformation($"No BOM record found for BOM ID {bomId}");
                return;
            }

            graph.Documents.Current = bomItem;
            if (graph.Documents.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, Messages.BOMMaint);
            }
        }

		public static void RedirectActive(string bomID)
		{
			if (string.IsNullOrWhiteSpace(bomID))
			{
				return;
			}

			var graphBOM = PXGraph.CreateInstance<BOMMaint>();

			var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(graphBOM, bomID);

			graphBOM.Documents.Current = bomItem;

			if (graphBOM.Documents.Current != null)
			{
				throw new PXRedirectRequiredException(graphBOM, true, Messages.BOMMaint);
			}
		}

		public static void RedirectToBOM(AMBomItem bomItem, BOMMaint graphBOM)
		{
			if (bomItem != null)
			{
				graphBOM.Documents.Current = bomItem;
				throw new PXRedirectRequiredException(graphBOM, true, Messages.BOMMaint) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
		}

		public static void RedirectToBOM(string bomID)
		{
			var	graphBOM = PXGraph.CreateInstance<BOMMaint>();

			if (bomID != null)
			{
				var bomItem = PrimaryBomIDManager.GetActiveRevisionBomItem(graphBOM, bomID);
				if (bomItem == null)
				{
					bomItem = PrimaryBomIDManager.GetRevisionBomItem(graphBOM, bomID);
				}
				RedirectToBOM(bomItem, graphBOM);
			}
		}

		public static void RedirectNew(int? inventoryID, int? siteID)
		{
			var graphBOM = PXGraph.CreateInstance<BOMMaint>();

			var bomItem = graphBOM.Documents.Insert(new AMBomItem { InventoryID = inventoryID, SiteID = siteID });
			RedirectToBOM(bomItem, graphBOM);
		}

		public class Conditions : BoundedTo<BOMMaint, AMBomItem>.Condition.Pack
		{
			public BoundedTo<BOMMaint, AMBomItem>.Condition IsECCEnabled => GetOrCreate(c =>
				PXAccess.FeatureInstalled<FeaturesSet.manufacturingECC>()
				? c.FromBql<True.IsEqual<True>>()
				: c.FromBql<True.IsEqual<False>>());
		}

		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<BOMMaint, AMBomItem>());
		protected static void Configure(WorkflowContext<BOMMaint, AMBomItem> context)
		{
			var conditions = context.Conditions.GetPack<Conditions>();
			var customOtherCategory = context.Categories.CreateNew("CustomOther",
				category => category.DisplayName("Other"));

			context.AddScreenConfigurationFor(screen =>
			{
				return screen
					  .WithCategories(categories =>
					  {
						  categories.Add(customOtherCategory);
						  categories.Update(FolderType.InquiriesFolder, category => category.PlaceAfter(customOtherCategory));
						  categories.Update(FolderType.ReportsFolder, category => category.PlaceAfter(FolderType.InquiriesFolder));
					  })
					.WithActions(actions =>
					{
						actions.Add(g => g.AMCopyBom, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.AMBomCostSettings, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.MakeDefaultBomAction, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.MakePlanningBomAction, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.Attributes, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.ArchiveBom, c => c.WithCategory(customOtherCategory));
						actions.Add(g => g.CreateECR, c => c.WithCategory(customOtherCategory));

						actions.Add(g => g.BOMCompare, a => a.WithCategory(PredefinedCategory.Inquiries));
						actions.Add(g => g.LaunchVisualBom, a => a.WithCategory(PredefinedCategory.Inquiries));
						actions.Add(g => g.LaunchEngineeringWorkbench, a => a.WithCategory(PredefinedCategory.Inquiries));
						actions.Add(g => g.ReportBOMSummary, a => a.WithCategory(PredefinedCategory.Reports));
						actions.Add(g => g.ReportMultiLevel, c => c.WithCategory(PredefinedCategory.Reports));

						#region Side Panels

						actions.AddNew("ShowBOMAttributes", a => a
							.DisplayName("BOM Attributes")
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen<BOMAttributeMaint>()
								.WithIcon("plus_square")
								.WithAssignments(ass =>
								{
									ass.Add(nameof(AMBomItem.BOMID), e => e.SetFromField<AMBomItem.bOMID>());
									ass.Add(nameof(AMBomItem.RevisionID), e => e.SetFromField<AMBomItem.revisionID>());
								})));

						actions.AddNew("ShowECR", a => a
							.DisplayName("Show Engineering Change Request")
							.IsHiddenWhen(!conditions.IsECCEnabled)
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen("AM2100PL")
								.WithIcon("control_point")
								.WithAssignments(ass =>
								{
									ass.Add("AMBomItem_bOMID", e => e.SetFromField<AMBomItem.bOMID>());
									ass.Add("AMBomItem_revisionID", e => e.SetFromField<AMBomItem.revisionID>());
								})));

						actions.AddNew("ShowECO", a => a
							.DisplayName("Show Engineering Change Order")
							.IsHiddenWhen(!conditions.IsECCEnabled)
							.IsSidePanelScreen(sp => sp
								.NavigateToScreen("AM2150PL")
								.WithIcon("check_circle")
								.WithAssignments(ass =>
								{
									ass.Add("AMBomItem_bOMID", e => e.SetFromField<AMBomItem.bOMID>());
									ass.Add("AMECOItem_bOMRevisionID", e => e.SetFromField<AMBomItem.revisionID>());
								})));

						#endregion
					});
			});
		}

		public PXAction<AMBomItem> LaunchVisualBom;
		[PXUIField(DisplayName = "Visual BOM", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable launchVisualBom(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				var graph = CreateInstance<VisualBOMMaint>();
				graph.Filter.Current.BOMID = Documents.Current.BOMID;
				graph.Filter.Current.RevisionID = Documents.Current.RevisionID;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.New);
			}

			return adapter.Get();
		}

		public PXAction<AMBomItem> LaunchEngineeringWorkbench;
		[PXUIField(DisplayName = "Engineering Workbench", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable launchEngineeringWorkbench(PXAdapter adapter)
		{
			if (Documents.Current != null)
			{
				EngineeringWorkbenchMaint.Redirect(Documents.Current);
			}

			return adapter.Get();
		}

		protected override void EnableButtons(bool enable)
		{
			base.EnableButtons(enable);
			LaunchVisualBom.SetEnabled(enable);
			LaunchEngineeringWorkbench.SetEnabled(enable);
		}
	}
}
