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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN.Matrix.Utility;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class ApplyToMatrixItemsExt : PXGraphExtension<ItemsGridExt, Graphs.TemplateInventoryItemMaint>
	{
		public PXAction<InventoryItem> applyToItems;
		[PXUIField(DisplayName = "Update Matrix Items", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXProcessButton(IsLockedOnToolbar = true)]
		public virtual IEnumerable ApplyToItems(PXAdapter adapter)
		{
			Base.Save.Press();

			InventoryItem templateItem = Base.Item.Current;
			var childrenItems = Base1.MatrixItems.SelectMain();

			if (templateItem.UpdateOnlySelected == true)
				childrenItems = childrenItems.Where(item => item.Selected == true).ToArray();

			var graph = (templateItem.StkItem == true)
				? (InventoryItemMaintBase)PXGraph.CreateInstance<InventoryItemMaint>()
				: PXGraph.CreateInstance<NonStockItemMaint>();
			graph.DefaultSiteFromItemClass = false;
			var helper = this.GetHelper(graph);

			PXLongOperation.StartOperation(Base, delegate ()
			{
				helper.CreateUpdateMatrixItems(graph, templateItem, childrenItems, false);
			});

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			applyToItems.SetEnabled(e.Cache.GetStatus(e.Row).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Updated));
		}

		protected virtual CreateMatrixItemsHelper GetHelper(PXGraph graph)
		{
			return new CreateMatrixItemsHelper(graph);
		}
	}
}
