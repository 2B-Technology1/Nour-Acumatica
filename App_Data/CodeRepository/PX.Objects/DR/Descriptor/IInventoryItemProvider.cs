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

using System.Collections.Generic;

using PX.Objects.IN;

namespace PX.Objects.DR.Descriptor
{
	public struct InventoryItemComponentInfo
	{
		public InventoryItem Item { get; set; }
		public INComponent Component { get; set; }
		public DRDeferredCode DeferralCode { get; set; }
	}

	public interface IInventoryItemProvider
	{
		/// <summary>
		/// Given an inventory item ID and the required component allocation method, 
		/// retrieves all inventory item components matching this method, along
		/// with the corresponding deferral codes in the form of 
		/// <see cref="InventoryItemComponentInfo"/>. If the allocation method does
		/// not support deferral codes, them <see cref="InventoryItemComponentInfo.DeferralCode"/>
		/// will be <c>null</c>.
		/// </summary>
		IEnumerable<InventoryItemComponentInfo> GetInventoryItemComponents(int? inventoryItemID, string allocationMethod);

		/// <summary>
		/// Given an inventory item component, returns the corresponding 
		/// substitute natural key - component name that as specified by
		/// <see cref="InventoryItem.InventoryCD"/>.
		/// </summary>
		string GetComponentName(INComponent component);

		/// <summary>
		/// Returns the InventoryItem item by Id
		/// </summary>
		InventoryItem GetInventoryItemByID(int? inventoryItemID);
	}
}
