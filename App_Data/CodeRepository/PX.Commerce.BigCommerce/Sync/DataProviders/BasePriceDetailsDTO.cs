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
	[DebuggerDisplay("Name={InventoryItem.InventoryCD}, Price={SalesPrice.SalesPrice.Value} {SalesPrice.CuryID} Bqty={SalesPrice.BreakQty}, PriceCode={SalesPrice.CustPriceClassID.ValueField()},TemplateID={InventoryItem.TemplateItemID}")]
	public class BasePriceDetailsDTO
	{
		public BasePriceDetailsDTO()
		{ }

		public void Init(ARSalesPrice salePrice,
							InventoryItem inventoryItem,
							INSite warehouse,							
							BCSyncStatus priceSyncStatus,
							BCSyncDetail priceSyncDetails,
							BCSyncStatusForInventoryItem inventorySyncStatus,
							BCSyncStatusForVariantInventoryItem variantInventorySyncStatus,
							BCSyncDetailForVariantInventoryItem variantInventorySyncDetail)
		{
			SalesPrice = salePrice;
			InventoryItem = inventoryItem;
			Warehouse = warehouse;
			PriceSyncStatus = priceSyncStatus;
			PriceSyncDetails = priceSyncDetails;
			InventorySyncStatus = inventorySyncStatus;
			VariantInventorySyncStatus = variantInventorySyncStatus;
			VariantInventorySyncDetail = variantInventorySyncDetail;
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
		public INSite Warehouse { get; set; }
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
	
	}
}
