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
using PX.Data.BQL.Fluent;
using PX.Objects.IN.Turnover;

namespace PX.Objects.IN.GraphExtensions.TurnoverEnqExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class InventoryLinkFilterExt : InventoryLinkFilterExtensionBase<TurnoverEnq, INTurnoverEnqFilter, INTurnoverEnqFilter.inventoryID>
	{
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class descr : AttachedInventoryDescription<descr> { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[AnyInventory(typeof(
			SearchFor<InventoryItem.inventoryID>.
			Where<
				Match<AccessInfo.userName.FromCurrent>.
				And<InventoryItem.stkItem.IsEqual<True>>.
				And<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.markedForDeletion>>.
				And<
					INTurnoverEnqFilter.itemClassID.FromCurrent.IsNull.
					Or<InventoryItem.itemClassID.IsEqual<INTurnoverEnqFilter.itemClassID.FromCurrent>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			DisplayName = "Inventory ID",
			IsKey = true)]
		protected override void _(Events.CacheAttached<InventoryLinkFilter.inventoryID> e) { }

		protected virtual void _(Events.RowUpdated<INTurnoverEnqFilter> e)
		{
			if (e.Row.InventoryID != null && !e.Cache.ObjectsEqual<INTurnoverEnqFilter.inventoryID>(e.OldRow, e.Row))
			{
				SelectedItems.Cache.Clear();
				SelectedItems.Cache.ClearQueryCache();
			}
		}

		[PXButton, PXUIField(DisplayName = "List")]
		public override void selectItems()
		{
			// Acuminator disable once PX1091 StackOverflowExceptionInBaseActionHandlerInvocation false alert
			base.selectItems();
			Base.FindTurnoverCalc(Base.Filter.Current);
		}

		/// Overrides <see cref="TurnoverEnq.AppendFilter(BqlCommand, IList{object}, INTurnoverEnqFilter)"/>
		[PXOverride]
		public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, INTurnoverEnqFilter filter,
			Func<BqlCommand, IList<object>, INTurnoverEnqFilter, BqlCommand> base_AppendFilter)
		{
			cmd = base_AppendFilter(cmd, parameters, filter);

			var inventories = GetSelectedEntities(filter).ToArray();
			if (inventories.Length > 0)
			{
				cmd = cmd.WhereAnd<Where<TurnoverCalcItem.inventoryID.IsIn<@P.AsInt>>>();
				parameters.Add(inventories);
			}

			return cmd;
		}
	}
}
