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

namespace PX.Objects.IN.PhysicalInventory
{
	public class PIItemLocationInfo : IEquatable<PIItemLocationInfo>
	{
		public int InventoryID { get; set; }
		public int? SubItemID { get; set; }
		public int LocationID { get; set; }
		public PXResult QueryResult { get; set; }

		//Sorting columns
		public string InventoryCD { get; set; }
		public string SubItemCD { get; set; }
		public string LocationCD { get; set; }
		public string LotSerialNbr { get; set; }
		public string Description { get; set; }

		public bool Equals(PIItemLocationInfo other)
		{
			if (other == null)
				return false;

			return InventoryID == other.InventoryID && SubItemID == other.SubItemID && LocationID == other.LocationID;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != GetType())
				return false;

			return Equals(obj as PIItemLocationInfo);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 17;
				hashCode = hashCode * 23 + InventoryID;
				hashCode = hashCode * 23 + SubItemID ?? 0;
				hashCode = hashCode * 23 + LocationID;
				return hashCode;
			}
		}

		public static PIItemLocationInfo Create(PXResult result)
		{
			InventoryItem inventoryItem = result.GetItem<InventoryItem>();
			INSubItem subItem = result.GetItem<INSubItem>();
			INLocation location = result.GetItem<INLocation>();
			INLotSerialStatus lotSerialStatus = result.GetItem<INLotSerialStatus>();

			return new PIItemLocationInfo
			{
				InventoryID = (int)inventoryItem.InventoryID,
				SubItemID = subItem?.SubItemID,
				LocationID = (int)location.LocationID,
				QueryResult = result,

				InventoryCD = inventoryItem.InventoryCD,
				SubItemCD = subItem?.SubItemCD,
				LocationCD = location.LocationCD,
				LotSerialNbr = lotSerialStatus?.LotSerialNbr,
				Description = inventoryItem.Descr
			};
		}
	}
}
