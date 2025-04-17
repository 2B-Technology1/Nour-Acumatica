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
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.PhysicalInventory
{
	public class PICollision
	{
		private const int c_itemsLoggingLimit = 50;

		public ICollection<int> InventoryItemIds { get; set; }
		public ICollection<int> LocationIds { get; set; }
		public string PIID { get; set; }
		public bool AllInventory { get; set; }
		public bool AllLocations { get; set; }

		public void Notify(PXGraph graph, string siteCD)
		{
			if (AllInventory && AllLocations)
			{
				PXTrace.WriteError(PXMessages.Localize(Messages.PIFullCollisionDetails), PIID, siteCD);
				return;
			}

			if (AllLocations)
			{
				PXTrace.WriteError(
					PXMessages.Localize(Messages.PIAllLocationsCollisionDetails),
					PIID,
					siteCD,
					string.Join(", ", ReadIdsToString(graph, InventoryItemIds, ReadInventoryCds)));

				return;
			}

			if (AllInventory)
			{
				PXTrace.WriteError(
					PXMessages.Localize(Messages.PIAllInventoryCollisionDetails),
					PIID,
					siteCD,
					string.Join(", ", ReadIdsToString(graph, LocationIds, ReadLocationCds)));

				return;
			}

			PXTrace.WriteError(
				PXMessages.Localize(Messages.PICollisionDetails),
				PIID,
				siteCD,
				string.Join(", ", ReadIdsToString(graph, InventoryItemIds, ReadInventoryCds)),
				string.Join(", ", ReadIdsToString(graph, LocationIds, ReadLocationCds)));
		}

		protected IEnumerable<string> ReadInventoryCds(PXGraph graph, IEnumerable<int> ids)
		{
			var query = new PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, In<Required<InventoryItem.inventoryID>>>>(graph);

			using(new PXFieldScope(
				query.View,
				typeof(InventoryItem.inventoryID),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.stkItem)))
			{
				return query.Select(ids.ToArray())
					.RowCast<InventoryItem>()
					.Select(i => i.InventoryCD);
			}
		}

		protected IEnumerable<string> ReadLocationCds(PXGraph graph, IEnumerable<int> ids)
		{
			var query = new PXSelect<INLocation,
					Where<INLocation.locationID, In<Required<INLocation.locationID>>>>(graph);

			using(new PXFieldScope(
				query.View,
				typeof(INLocation.locationID),
				typeof(INLocation.siteID),
				typeof(INLocation.locationCD)))
			{
				return query.Select(ids.ToArray())
					.RowCast<INLocation>()
					.Select(i => i.LocationCD);
			}
		}

		protected string ReadIdsToString(PXGraph graph, ICollection<int> ids, Func<PXGraph, IEnumerable<int>, IEnumerable<string>> readFunc)
		{
			var idsString = string.Join(", ", readFunc(graph, ids.Take(c_itemsLoggingLimit)));

			if (ids.Count > c_itemsLoggingLimit)
				return idsString + $", and {ids.Count-c_itemsLoggingLimit} more...";

			return idsString;
		}
	}
}
