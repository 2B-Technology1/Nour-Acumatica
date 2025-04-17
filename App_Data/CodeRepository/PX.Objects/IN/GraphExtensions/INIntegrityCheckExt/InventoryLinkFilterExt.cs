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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.IN.GraphExtensions.INIntegrityCheckExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class InventoryLinkFilterExt : InventoryLinkFilterExtensionBase<INIntegrityCheck, INRecalculateInventoryFilter, INRecalculateInventoryFilter.inventoryID>
	{
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class descr : AttachedInventoryDescription<descr> { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[AnyInventory(typeof(
			SearchFor<InventoryItem.inventoryID>.
			Where<
				INRecalculateInventoryFilter.itemClassID.FromCurrent.IsNull.
				Or<InventoryItem.itemClassID.IsEqual<INRecalculateInventoryFilter.itemClassID.FromCurrent>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr), IsKey = true)]
		[PXRestrictor(
			typeof(Where<InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.inactive>>),
			Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
		protected override void _(Events.CacheAttached<InventoryLinkFilter.inventoryID> e) { }

		protected virtual void _(Events.RowUpdated<INRecalculateInventoryFilter> e)
		{
			if (e.Row.InventoryID != null && !e.Cache.ObjectsEqual<INRecalculateInventoryFilter.inventoryID>(e.OldRow, e.Row))
				SelectedItems.Cache.Clear();
		}

		protected virtual void _(Events.FieldUpdated<INRecalculateInventoryFilter, INRecalculateInventoryFilter.itemClassID> e)
		{
			if (!IsValidValue<INRecalculateInventoryFilter.inventoryID>(e.Row))
				e.Cache.SetValueExt<INRecalculateInventoryFilter.inventoryID>(e.Row, null);
		}

		protected virtual bool IsValidValue<TField>(object row)
			where TField : IBqlField
		{
			var table = BqlCommand.GetItemType<TField>();
			var cache = Base.Caches[table];
			var value = Base.Filter.Cache.GetValue<TField>(row);
			if (value == null)
				return true;

			try
			{
				cache.RaiseFieldVerifying<TField>(row, ref value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// Overrides <see cref="INIntegrityCheck.AppendFilter(BqlCommand, IList{object}, INRecalculateInventoryFilter)"/>
		[PXOverride]
		public BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, INRecalculateInventoryFilter filter,
			Func<BqlCommand, IList<object>, INRecalculateInventoryFilter, BqlCommand> base_AppendFilter)
		{
			cmd = base_AppendFilter(cmd, parameters, filter);

			var inventories = GetSelectedEntities(filter).ToArray();
			if (inventories.Length > 0)
			{
				cmd = cmd.WhereAnd<Where<InventoryItemCommon.inventoryID.IsIn<@P.AsInt>>>();
				parameters.Add(inventories);
			}

			return cmd;
		}
	}
}
