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
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using static PX.Objects.PM.ProjectEntry;
using PX.Data.WorkflowAPI;

namespace PX.Objects.AM.GraphExtensions
{
    public class ProjectEntryAMExtension : PXGraphExtension<ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public PXFilter<CreateProdFilter> CreateProductionOrderFilter;

        [PXHidden]
        public PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID, Equal<Numbering.numberingID>>>,
                    Where<AMOrderType.orderType, Equal<Current<AMOrderType.orderType>>>> ProductionNumbering;

        public PXSelect<AMProdItem, Where<AMProdItem.projectID, Equal<Current<PMProject.contractID>>>> ProjectProdOrders;

        public PXSelectJoin<AMEstimateItem, LeftJoin<AMEstimateReference, On<AMEstimateReference.estimateID, Equal<AMEstimateItem.estimateID>>>,
            Where<AMEstimateReference.projectID, Equal<Current<PMProject.contractID>>>> ProjectEstimates;

        public PXAction<PMProject> productionSummary;
        [PXUIField(DisplayName = "Production Summary", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected virtual IEnumerable ProductionSummary(PXAdapter adapter)
        {
            if (Base?.Project?.Current?.ContractID == null)
            {
                return adapter.Get();
            }

            var productionSummaryGi = new GIProductionSummary();
            productionSummaryGi.SetNoStatus();
            productionSummaryGi.SetProjectFilter(Base.Project.Current.ContractID);
            productionSummaryGi.CallGenericInquiry();

            return adapter.Get();
        }

        public PXAction<PMProject> createProdOrder;
        [PXUIField(DisplayName = "Create Prod Order", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable CreateProdOrder(PXAdapter adapter)
        {
            CreateProductionOrderFilter.Current.ProjectID = Base?.Project?.Current?.ContractID;

            if (CurrentProductionNumbering != null && !CurrentProductionNumbering.UserNumbering.GetValueOrDefault())
            {
                CreateProductionOrderFilter.Current.ProdOrdID = CurrentProductionNumbering.NewSymbol;
            }
            if (CreateProductionOrderFilter.AskExt() == WebDialogResult.OK)
            {
				if (CreateProductionOrderFilter.Current.TaskID == null)
				{
					throw new PXException(PM.Messages.TaskIdEmptyError);
				}

                CreateProductionOrder(CreateProductionOrderFilter.Current);
                return adapter.Get();
            }
            else
            {
                return adapter.Get();
            }

        }

        protected Numbering CurrentProductionNumbering
        {
            get
            {

                ProductionNumbering.Current = PXSelectJoin<Numbering, LeftJoin<AMOrderType, On<AMOrderType.prodNumberingID,
                        Equal<Numbering.numberingID>>>, Where<AMOrderType.orderType, Equal<Required<AMOrderType.orderType>>>>
                        .Select(Base, CreateProductionOrderFilter.Current.OrderType);
                return ProductionNumbering.Current;
            }
        }

        private void CreateProductionOrder(CreateProdFilter filter)
        {
            var prodMaintGraph = PXGraph.CreateInstance<ProdMaint>();
            prodMaintGraph.Clear();
            prodMaintGraph.IsImport = true;

            AMProdItem amproditem = prodMaintGraph.ProdMaintRecords.Insert(new AMProdItem
            {
                OrderType = filter.OrderType,
                ProdOrdID = filter.ProdOrdID,
                SiteID = filter.SiteID,
                LocationID = filter.LocationID
            });
            amproditem.InventoryID = filter.InventoryID;

            amproditem = PXCache<AMProdItem>.CreateCopy(amproditem);
            PXCache cache = prodMaintGraph.Caches[typeof(AMProdItem)];
            prodMaintGraph.SetAccountDefaults(cache);
            amproditem = (AMProdItem)cache.Current;
            amproditem.DetailSource = ProductionDetailSource.BOM;
            amproditem.BuildProductionBom = true;
            amproditem.Reschedule = true;
            amproditem.SubItemID = filter.SubItemID;
            amproditem.ProdDate = filter.ProdDate;
            amproditem.ProjectID = filter.ProjectID;
            amproditem.TaskID = filter.TaskID;
            amproditem.CostCodeID = filter.CostCodeID;
            amproditem.QtytoProd = filter.QtytoProd;
            amproditem.UpdateProject = true;
            amproditem = PXCache<AMProdItem>.CreateCopy(prodMaintGraph.ProdMaintRecords.Update(amproditem));
            prodMaintGraph.Actions.PressSave();

        }

        public PXAction<AMProdItem> viewProdOrder;
        [PXUIField(DisplayName = "View Prod Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
        public virtual IEnumerable ViewProdOrder(PXAdapter adapter)
        {
            if (ProjectProdOrders.Current != null && !string.IsNullOrEmpty(ProjectProdOrders.Current.ProdOrdID))
            {
                ProdMaint target = PXGraph.CreateInstance<ProdMaint>();
                target.ProdItemSelected.Current = ProjectProdOrders.Current;
                throw new PXRedirectRequiredException(target, true, "ViewProdOrder") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        protected virtual void CreateProdFilter_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CreateProdFilter row = (CreateProdFilter)e.Row;
            if (row != null && row.InventoryID != null)
            {
                InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, row.InventoryID);
                if (item != null)
                {
                    row.SiteID = item.DfltSiteID;
                    row.LocationID = item.DfltReceiptLocationID;
                }
            }
        }

        [PXOverride]
        public virtual void DefaultFromTemplate(PMProject prj, int? templateID, DefaultFromTemplateSettings settings, Action<PMProject, int?, DefaultFromTemplateSettings> method)
        {
            method?.Invoke(prj, templateID, settings);
            PMProject templ = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, templateID);
            if (templ == null)
            {
                return;
            }
            var templext = PXCache<PMProject>.GetExtension<PMProjectExt>(templ);
            var prjext = PXCache<PMProject>.GetExtension<PMProjectExt>(prj);
            if (prjext == null)
            {
                return;
            }
            prjext.VisibleInPROD = templext == null ? false : templext.VisibleInPROD;
        }

        [PXOverride]
        public virtual PMTask CopyTask(PMTask task, int ProjectID, DefaultFromTemplateSettings settings, Func<PMTask, int, DefaultFromTemplateSettings, PMTask> method)
        {
            var dst = method?.Invoke(task, ProjectID, settings);
            var templext = PXCache<PMTask>.GetExtension<PMTaskExt>(task);
            var dstext = PXCache<PMTask>.GetExtension<PMTaskExt>(dst);
            if (dstext == null)
            {
                return null;
            }
            dstext.VisibleInPROD = templext == null ? false : templext.VisibleInPROD;

            return dst;
        }

        public virtual void PMProject_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row != null)
            {
                var setupExt = PXCache<PMSetup>.GetExtension<PMSetupExt>(Base.Setup.Current);
                if (setupExt != null)
                    PXUIFieldAttribute.SetEnabled<PMProjectExt.visibleInPROD>(cache, e.Row, setupExt.VisibleInPROD == true);
            }

        }
    }

    public class ProjectEntryAMExtension_Workflow : PXGraphExtension<ProjectEntry_Workflow, ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturing>();
        }

        public sealed override void Configure(PXScreenConfiguration config) =>
            Configure(config.GetScreenConfigurationContext<ProjectEntry, PMProject>());
        protected static void Configure(WorkflowContext<ProjectEntry, PMProject> context)
        {
            context.UpdateScreenConfigurationFor(screen =>
            {
                return screen
                    .WithActions(actions =>
                    {
                        actions.Add<ProjectEntryAMExtension>(g => g.productionSummary,
                            c => c.WithCategory(context.Categories.Get(ToolbarCategory.ActionCategoryNames.Other)));
                    });
            });
        }
    }
}
