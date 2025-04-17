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

using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using System.Diagnostics;

namespace PX.Commerce.BigCommerce
{
	/// <summary>
	/// Holds details about the price with values from different tables.
	/// </summary>
	[DebuggerDisplay("Name={InventoryItem?.InventoryCD.Trim()}, PriceId={SalesPrice?.RecordID} Price={SalesPrice?.SalesPrice} {SalesPrice?.CuryID} Bqty={SalesPrice?.BreakQty}, PriceCode={SalesPrice?.CustPriceClassID},TemplateID={InventoryItem.TemplateItemID}")]
	public class PriceListDetailsDTO
	{
		public PriceListDetailsDTO()
		{ }

		public void Init(InventoryItem inventoryItem,
						 ARSalesPrice salesPrice,
						 ARPriceClass priceClass,
						 BCSyncStatus priceSyncStatus,
						 BCSyncDetail priceSyncDetails,
						 BCSyncStatusForInventoryItem inventorySyncStatus,
						 BCSyncStatusForCustomerPriceClass customerClassSyncStatus,
						 BCSyncStatusForVariantInventoryItem syncStatusForVariantItem,
						 BCSyncDetailForVariantInventoryItem syncDetailForVariantItem)
		{
			SalesPrice = salesPrice;
			InventoryItem = inventoryItem;
			PriceSyncStatus = priceSyncStatus;
			PriceSyncDetails = priceSyncDetails;
			InventorySyncStatus = inventorySyncStatus;
			CustomerClassSyncStatus = customerClassSyncStatus;
			PriceClass = priceClass;
			VariantInventorySyncStatus = syncStatusForVariantItem;
			VariantInventorySyncDetail = syncDetailForVariantItem;
		}


		/// <summary>
		/// Price details. The entity can be empty (null) in the case of a deleted price.
		/// </summary>
		public ARSalesPrice SalesPrice { get; set; }

		/// <summary>
		/// InventoryItem to which the price applies
		/// </summary>
		public InventoryItem InventoryItem { get; set; }
		/// <summary>
		/// Warehouse related to the price/
		/// </summary>
		//public INSite Warehouse { get; set; }
		/// <summary>
		/// BCSyncStatus for the base price or the price list
		/// </summary>
		public BCSyncStatus PriceSyncStatus { get; set; }

		/// <summary>
		/// Every price synced as part of the price list will have a BCSyncDetail record.
		/// </summary>
		public BCSyncDetail PriceSyncDetails { get; set; }
		/// <summary>
		/// BCSyncStatus for the inventory item
		/// </summary>
		public BCSyncStatus InventorySyncStatus { get; set; }
		/// <summary>
		/// BCSyncStatus for the variant inventory item in case of a variant
		/// </summary>
		public BCSyncStatus VariantInventorySyncStatus { get; set; }
		/// <summary>
		/// BCSyncDetail for the variant inventory item in case of a variant
		/// </summary>
		public BCSyncDetail VariantInventorySyncDetail { get; protected set; }
		/// <summary>
		/// BCSyncStatus for the customer price class entity
		/// </summary>
		public BCSyncStatus CustomerClassSyncStatus { get; set; }
		/// <summary>
		/// Details about the price class
		/// </summary>
		public ARPriceClass PriceClass { get; set; }
	}
}
