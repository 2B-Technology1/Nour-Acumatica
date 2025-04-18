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
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.IN;
using static PX.Objects.AR.ARSalesPriceMaint;

namespace PX.Objects.AR
{
	public class ARSalesPriceMaintTemplateItemExtension : PXGraphExtension<ARSalesPriceMaint>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.matrixItem>();

		public static int? GetTemplateInventoryID(PXCache sender, int? inventoryID)
		{
			return InventoryItem.PK.Find(sender.Graph, inventoryID)?.TemplateItemID;
		}

		public delegate SalesPriceItem FindSalesPriceOrig(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date, bool isFairValue, string taxCalcMode);
		[PXOverride]
		public virtual SalesPriceItem FindSalesPrice(PXCache sender, string custPriceClass, int? customerID, int? inventoryID, int? siteID, string baseCuryID, string curyID, decimal? quantity, string UOM, DateTime date, bool isFairValue, string taxCalcMode, FindSalesPriceOrig baseMethod)
		{
			var priceListExists = RecordExistsSlot<ARSalesPrice, ARSalesPrice.recordID>.IsRowsExists();
			if (!priceListExists)
				return isFairValue ? null : Base.SelectDefaultItemPrice(sender, inventoryID, baseCuryID);

			var salesPriceSelect =
				new SalesPriceSelectWithTemplateItem(sender, inventoryID, UOM, (decimal)quantity, isFairValue, taxCalcMode)
				{
					CustomerID = customerID,
					CustPriceClass = custPriceClass,
					CuryID = curyID,
					SiteID = siteID,
					Date = date,
					TaxCalcMode = taxCalcMode
				};

			SalesPriceForCurrentUOMWithTemplateItem priceForCurrentUOM = salesPriceSelect.ForCurrentUOM();
			SalesPriceForBaseUOMWithTemplateItem priceForBaseUOM = salesPriceSelect.ForBaseUOM();
			SalesPriceForSalesUOMWithTemplateItem priceForSalesUOM = salesPriceSelect.ForSalesUOM();

			return priceForCurrentUOM.SelectCustomerPrice()
				?? priceForBaseUOM.SelectCustomerPrice()
				?? priceForCurrentUOM.SelectBasePrice()
				?? priceForBaseUOM.SelectBasePrice()
				?? (isFairValue ? null : Base.SelectDefaultItemPrice(sender, inventoryID, baseCuryID))
				?? (isFairValue ? null : Base.SelectDefaultItemPrice(sender, GetTemplateInventoryID(sender, inventoryID), baseCuryID))
				?? priceForSalesUOM.SelectCustomerPrice()
				?? priceForSalesUOM.SelectBasePrice();
		}

		internal class SalesPriceSelectWithTemplateItem : SalesPriceSelect
		{
			public SalesPriceSelectWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty, bool isFairValue) : base(cache, inventoryID, uom, qty, isFairValue)
			{
			}

			public SalesPriceSelectWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty, bool isFairValue, string taxCalcMode) : base(cache, inventoryID, uom, qty, isFairValue, taxCalcMode)
			{
			}

			#region Factories
			public new SalesPriceForCurrentUOMWithTemplateItem ForCurrentUOM() => new SalesPriceForCurrentUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue, TaxCalcMode = TaxCalcMode };
			public new SalesPriceForBaseUOMWithTemplateItem ForBaseUOM() => new SalesPriceForBaseUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue, TaxCalcMode = TaxCalcMode };
			public new SalesPriceForSalesUOMWithTemplateItem ForSalesUOM() => new SalesPriceForSalesUOMWithTemplateItem(Cache, InventoryID, UOM, Qty) { CustomerID = CustomerID, CustPriceClass = CustPriceClass, CuryID = CuryID, SiteID = SiteID, Date = Date, IsFairValue = IsFairValue, TaxCalcMode = TaxCalcMode };
			#endregion
		}

		internal class SalesPriceForCurrentUOMWithTemplateItem : SalesPriceForCurrentUOM
		{
			public SalesPriceForCurrentUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.Join<InnerJoin<InventoryItem,
						On<InventoryItem.inventoryID, Equal<ARSalesPrice.inventoryID>>>>();

				SelectCommand.OrderByNew<OrderBy<
							Asc<ARSalesPrice.priceType,
							Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}

		internal class SalesPriceForBaseUOMWithTemplateItem : SalesPriceForBaseUOM
		{
			public SalesPriceForBaseUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.OrderByNew<OrderBy<
							Asc<ARSalesPrice.priceType,
							Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}

		internal class SalesPriceForSalesUOMWithTemplateItem : SalesPriceForSalesUOM
		{
			public SalesPriceForSalesUOMWithTemplateItem(PXCache cache, int? inventoryID, string uom, decimal qty) : base(cache, inventoryID, uom, qty)
			{
				SelectCommand.OrderByNew<OrderBy<
							Asc<ARSalesPrice.priceType,
							Desc<ARSalesPrice.isPromotionalPrice,
							Desc<ARSalesPrice.siteID,
							Asc<InventoryItem.isTemplate,
							Desc <ARSalesPrice.breakQty>>>>>>>();
			}

			public override int?[] GetInventoryIDs(PXCache sender, int? inventoryID)
			{
				return new int?[] { inventoryID, GetTemplateInventoryID(sender, inventoryID) };
			}
		}
	}
}
