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
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
	using PX.Data.WorkflowAPI;
	using PX.Objects.AM.Attributes;
	using PX.Objects.AM.CacheExtensions;

	public class NonStockItemMaintAMExtension : PXGraphExtension<NonStockItemMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public sealed override void Configure(PXScreenConfiguration configuration) =>
            Configure(configuration.GetScreenConfigurationContext<NonStockItemMaint, InventoryItem>());
        protected static void Configure(WorkflowContext<NonStockItemMaint, InventoryItem> context)
        {
            context
                .UpdateScreenConfigurationFor(screen =>
                    screen.WithActions(actions =>
                        actions.Add<NonStockItemMaintAMExtension>(e => e.WhereUsedInq, a => a.WithCategory(PredefinedCategory.Inquiries))));
        }

        [PXOverride]
        public void Persist(Action del)
        {
            var estimateInventoryCdUpdateRequired = !Base.Item.Cache.IsCurrentRowInserted() &&
                                                    EstimateGraphHelper
                                                        .InventoryCDUpdateRequired<InventoryItem.inventoryCD>(
                                                            Base.Item.Cache);

            del?.Invoke();

            if (estimateInventoryCdUpdateRequired)
            {
                EstimateGraphHelper.UpdateEstimateInventoryCD(Base.Item.Current, Base);
            }
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [MaterialDefaultMarkFor.NonStockItemList]
		protected virtual void _(Events.CacheAttached<InventoryItemExt.aMDefaultMarkFor> e) { }

        public PXAction<InventoryItem> WhereUsedInq;
        [PXButton]
        [PXUIField(DisplayName = "BOM Where Used", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual System.Collections.IEnumerable whereUsedInq(PXAdapter adapter)
        {
            if (Base.Item.Current == null
                || Base.Item.Current.InventoryID.GetValueOrDefault() <= 0)
            {
                return adapter.Get();
            }

            var inqGraph = PXGraph.CreateInstance<BOMWhereUsedInq>();
            inqGraph.Filter.Current.InventoryID = Base.Item.Current.InventoryID;
            PXRedirectHelper.TryRedirect(inqGraph, PXRedirectHelper.WindowMode.NewWindow);

            return adapter.Get();
        }
    }
}
