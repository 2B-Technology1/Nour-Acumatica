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
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN.DAC.Unbound;

namespace PX.Objects.IN.GraphExtensions.MassConvertStockNonStockExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class InventoryLinkFilterExt : InventoryLinkFilterExtensionBase<MassConvertStockNonStock, MassConvertStockNonStockFilter, MassConvertStockNonStockFilter.inventoryID>
	{
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class descr : AttachedInventoryDescription<descr> { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[AnyInventory(typeof(
			Search<InventoryItem.inventoryID,
			Where<
				InventoryItem.stkItem, Equal<Current<MassConvertStockNonStockFilter.stkItem>>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.isTemplate, Equal<False>,
				And<InventoryItem.templateItemID, IsNull,
				And<InventoryItem.kitItem, Equal<False>,
				And2<Where<
					InventoryItem.stkItem, Equal<True>,
					Or<InventoryItem.nonStockReceipt, Equal<True>,
					And<InventoryItem.nonStockShip, Equal<True>,
					And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>>,
				And<MatchUser>>>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			DisplayName = "Inventory ID",
			IsKey = true)]
		protected override void _(Events.CacheAttached<InventoryLinkFilter.inventoryID> e) { }

		protected virtual void _(Events.RowUpdated<MassConvertStockNonStockFilter> e)
		{
			if (!e.Cache.ObjectsEqual<MassConvertStockNonStockFilter.stkItem, MassConvertStockNonStockFilter.inventoryID>(e.Row, e.OldRow))
			{
				SelectedItems.Cache.Clear();
				SelectedItems.Cache.ClearQueryCache();
			}
		}

		/// Overrides <see cref="MassConvertStockNonStock.AppendFilter(BqlCommand, IList{object}, MassConvertStockNonStockFilter)"/>
		[PXOverride]
		public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, MassConvertStockNonStockFilter filter,
			Func<BqlCommand, IList<object>, MassConvertStockNonStockFilter, BqlCommand> base_AppendFilter)
		{
			cmd = base_AppendFilter(cmd, parameters, filter);

			var inventories = GetSelectedEntities(filter).ToArray();
			if (inventories.Length > 0)
			{
				cmd = cmd.WhereAnd<Where<InventoryItem.inventoryID.IsIn<@P.AsInt>>>();
				parameters.Add(inventories);
			}

			return cmd;
		}
	}
}
