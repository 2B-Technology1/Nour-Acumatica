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

using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.PhysicalInventory
{
	public class PILocks
	{
		protected string _piId;
		protected bool _isActive;
		protected bool _fullItemsLock;
		protected HashSet<int> _itemsCollection;
		protected bool _fullLocationsLock;
		protected HashSet<int> _locationsCollection;

		public string PIID => _piId;
		public bool IsActive => _isActive;

		public PILocks(string piId, IEnumerable<INPIStatusItem> itemLocks, IEnumerable<INPIStatusLoc> locationLocks)
		{
			_piId = piId;
			bool itemLocksActive, locationLocksActive;
			_itemsCollection = ParseLocks(itemLocks, r => r.InventoryID, out _fullItemsLock, out itemLocksActive);
			_locationsCollection = ParseLocks(locationLocks, r => r.LocationID, out _fullLocationsLock, out locationLocksActive);
			_isActive = itemLocksActive || locationLocksActive;
		}

		public virtual bool HasCollision(int inventoryID, int locationID)
		{
			bool itemIntersects = _fullItemsLock ^ _itemsCollection.Contains(inventoryID);
			bool locIntersects = _fullLocationsLock ^ _locationsCollection.Contains(locationID);
			return itemIntersects && locIntersects;
		}

		public virtual PICollision Intersect(
			bool fullItemsLock, ICollection<int> inventoryItems,
			bool fullLocationsLock, ICollection<int> locations)
		{
			bool fullInventoryIntersection;
			List<int> itemsIntersection = GetIntersection(_fullItemsLock, _itemsCollection, fullItemsLock, inventoryItems, out fullInventoryIntersection);

			if (!fullInventoryIntersection && !itemsIntersection.Any())
				return null;

			bool fullLocationsIntersection;
			List<int> locationsIntersection = GetIntersection(_fullLocationsLock, _locationsCollection, fullLocationsLock, locations, out fullLocationsIntersection);

			if (!fullLocationsIntersection && !locationsIntersection.Any())
				return null;

			return new PICollision
			{
				PIID = _piId,
				AllInventory = fullInventoryIntersection,
				AllLocations = fullLocationsIntersection,
				InventoryItemIds = itemsIntersection,
				LocationIds = locationsIntersection
			};
		}

		protected List<int> GetIntersection(
			bool fullLock1, ICollection<int> collection1,
			bool fullLock2, ICollection<int> collection2,
			out bool fullIntersection)
		{
			fullIntersection = false;
			if (fullLock1 == fullLock2)
			{
				if (fullLock1 == true)
				{
					fullIntersection = true;
					return new List<int>(0);
				}
				else
				{
					return collection1.Where(id => collection2.Contains(id)).ToList();
				}
			}
			else
			{
				var baseCollection = fullLock1 ? collection2 : collection1;
				var excludeCollection = fullLock1 ? collection1 : collection2;
				return baseCollection.Where(id => !excludeCollection.Contains(id)).ToList();
			}
		}

		protected HashSet<int> ParseLocks<TLock>(IEnumerable<TLock> items, Func<TLock, int?> idSelector, out bool fullLock, out bool isActive)
			where TLock : IPILock
		{
			bool emptyLockExists = items.Any(i => idSelector(i) == null);
			fullLock = emptyLockExists;
			isActive = items.Any(i => i.Active == true);
			return items.Where(i => i.Excluded == emptyLockExists && idSelector(i) != null).Select(i => (int)idSelector(i)).ToHashSet();
		}
	}
}
