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
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Linq;

namespace PX.Objects.PO
{
	[Serializable]
	public static class POItemCostManager
	{
		public class ItemCost
		{
			public ItemCost(InventoryItem item, decimal cost)
				: this(item, null, null, cost, cost, true)
			{
			}
			public ItemCost(InventoryItem item, string uom, string curyID, decimal cost, bool convertCury)
				: this(item, uom, curyID, cost, cost, convertCury)
			{
			}
			public ItemCost(InventoryItem item, string uom, string curyID, decimal cost, decimal baseCost, bool convertCury)
			{
				this.Item = item;
				this.uom = uom;
				this.CuryID = curyID;
				this.Cost = cost;
				this.BaseCost = baseCost;
				this.ConvertCury = convertCury;
			}

			public string UOM
			{
				get
				{
					return this.Item != null ? this.uom ?? this.Item.BaseUnit : null;
				}
			}

			private readonly string uom;

			public readonly InventoryItem Item;
			public readonly string CuryID;
			public readonly decimal Cost;
			public readonly decimal BaseCost;
			public readonly bool ConvertCury;

			public decimal Convert<InventoryIDField>(PXGraph graph, object inventoryRow, CM.Extensions.CurrencyInfo currencyInfo, string uom)
				where InventoryIDField : IBqlField
			{
				ItemCost price = this;
				if (price == null || price.Cost == 0 || price.Item == null || inventoryRow == null)
					return 0;

				decimal result = (ConvertCury && currencyInfo != null) ? currencyInfo.CuryConvCuryRaw(price.BaseCost) : price.BaseCost;

				if (price.UOM != uom && !string.IsNullOrEmpty(uom))
				{
					if (inventoryRow == null) return 0;

					PXCache invCache = graph.Caches[inventoryRow.GetType()];
					decimal baseUOM =
						price.UOM != price.Item.BaseUnit ?
						INUnitAttribute.ConvertFromBase<InventoryIDField>(invCache, inventoryRow, price.UOM, result, INPrecision.UNITCOST) :
						result;

					result =
						uom != price.Item.BaseUnit ?
						INUnitAttribute.ConvertToBase<InventoryIDField>(invCache, inventoryRow, uom, baseUOM, INPrecision.UNITCOST) :
						baseUOM;
				}

				return result;
			}
		}

		private static ItemCost FetchStdCost(PXGraph graph, InventoryItem item, string baseCuryID, DateTime? docDate)
		{
			if (item.StkItem == false || item.ValMethod == INValMethod.Standard)
			{
				InventoryItemCurySettings curySettings =
					InventoryItemCurySettings.PK.Find(graph, item.InventoryID, baseCuryID ?? graph.Accessinfo.BaseCuryID);

				if (curySettings == null) return null;

				if (!docDate.HasValue || (curySettings.StdCostDate.HasValue && curySettings.StdCostDate.Value <= docDate.Value))
				{
					return new ItemCost(item, curySettings.StdCost.Value);
				}
			}
			return null;
		}

		private static ItemCost FetchSiteLastCost(PXGraph graph, InventoryItem item, int? siteID, string baseCuryID)
		{
			INItemStats itemStats;
			if (siteID != null)
			{
				itemStats = PXSelect<INItemStats,
					 Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>,
					And<INItemStats.siteID, Equal<Required<INItemStats.siteID>>>>>
					.Select(graph, item.InventoryID, siteID);
			}
			else
			{
				itemStats = PXSelectJoin<INItemStats,
					InnerJoin<INSite, On<INItemStats.FK.Site>>,
					Where<INItemStats.inventoryID, Equal<Required<INItemStats.inventoryID>>,
						And<INSite.baseCuryID, Equal<Required<INSite.baseCuryID>>>>,
					OrderBy<Desc<INItemStats.lastCostDate>>>
					.Select(graph, item.InventoryID, baseCuryID);
			}

			if (itemStats?.LastCost != null)
				return new ItemCost(item, itemStats.LastCost.Value);

			return null;
		}

		public static int? FetchLocation(PXGraph graph, int? vendorID, int? itemID, int? subItemID, int? siteID)
		{
			BAccountR company = PXSelectJoin<BAccountR, 
				InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, 
				Where<BAccountR.bAccountID, Equal<Required<BAccountR.bAccountID>>>>.Select(graph, vendorID);
			if (company != null)
			{
				return company.DefLocationID;
			}

			Vendor vendor = PXSelectReadonly<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(graph, vendorID);

			var record =
						PXSelectJoin<INItemSiteSettings,
								LeftJoin<POVendorInventory,
									On<POVendorInventory.inventoryID, Equal<INItemSiteSettings.inventoryID>,
									And<POVendorInventory.active, Equal<boolTrue>,
									And<POVendorInventory.vendorID, Equal<Required<Vendor.bAccountID>>,
									And<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
											Or<POVendorInventory.subItemID, Equal<INItemSiteSettings.defaultSubItemID>,
											Or<POVendorInventory.subItemID, IsNull,
											Or<Where<Required<POVendorInventory.subItemID>, IsNull,
												 And<POVendorInventory.subItemID, Equal<True>>>>>>>>>>>>,
							Where<INItemSiteSettings.inventoryID, Equal<Required<INItemSiteSettings.inventoryID>>,
								And<INItemSiteSettings.siteID, Equal<Required<INItemSiteSettings.siteID>>>>>
								.Select(graph, vendorID, subItemID, subItemID, itemID, siteID).ToArray()
								.Select(r => new { Item = r.GetItem<POVendorInventory>(), Site = r.GetItem<INItemSiteSettings>() })
								.Where(r => r.Item != null && r.Site != null)
								.OrderBy(r => r.Item.LastPrice)
								.ThenByDescending(r => r.Item.SubItemID == r.Site.DefaultSubItemID)
								.ThenByDescending(r => r.Item.VendorLocationID != null)
								.ThenByDescending(r => r.Item.IsDefault == true)
								.ThenByDescending(r => r.Item.VendorLocationID == vendor?.DefLocationID)
								.FirstOrDefault();

			if (record == null)
				return null;
			if (record.Item.VendorLocationID != null)
				return record.Item.VendorLocationID;
			if (record.Site.PreferredVendorID == vendorID)
				return record.Site.PreferredVendorLocationID ?? vendor?.DefLocationID;
			if (vendor != null && vendor.BAccountID == vendorID)
				return vendor.DefLocationID;
			return null;
		}
		public static CM.Extensions.CurrencyInfo FetchCuryInfo<CuryInfoIDField>(PXGraph graph, object row)
			where CuryInfoIDField : IBqlField
		{
			PXCache cache = graph.Caches[row.GetType()];
			if (cache.GetValue<CuryInfoIDField>(row) == null)
			{
				cache.SetDefaultExt<CuryInfoIDField>(row);
			}
			return CM.TemporaryHelpers.MultiCurrencyCalculator.GetCurrencyInfo<CuryInfoIDField>(graph, row);
		}

		public static decimal? Fetch<InventoryIDField, CuryInfoIDField>(PXGraph graph, object row, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, int? inventoryID, int? subItemID, int? siteID, string uom, bool onlyVendor = false)
			where InventoryIDField : IBqlField
			where CuryInfoIDField : IBqlField
		{
			CM.Extensions.CurrencyInfo info = FetchCuryInfo<CuryInfoIDField>(graph, row);
			ItemCost price = Fetch(graph, vendorID, vendorLocationID, docDate, curyID, info?.BaseCuryID, inventoryID, subItemID, siteID, uom, onlyVendor);
			return price.Convert<InventoryIDField>(graph, row, info, uom);
		}

		public static ItemCost Fetch(PXGraph graph, int? vendorID, int? vendorLocationID, DateTime? docDate, string curyID, string baseCuryID, int? inventoryID, int? subItemID, int? siteID, string uom, bool onlyVendor = false)
		{
			PXSelectBase<InventoryItem> vendorCostSelect = 
				new PXSelectReadonly2<InventoryItem,
					LeftJoin<INItemCost, On<INItemCost.inventoryID, Equal<InventoryItem.inventoryID>,
						And<INItemCost.curyID, Equal<Required<INItemCost.curyID>>>>,
				LeftJoin<POVendorInventory, On<
					POVendorInventory.inventoryID, Equal<InventoryItem.inventoryID>,
					And<POVendorInventory.active, Equal<True>,
								And<POVendorInventory.vendorID, Equal<Required<Vendor.bAccountID>>,
								And<POVendorInventory.curyID, Equal<Required<POVendorInventory.curyID>>,
					And2<Where<POVendorInventory.subItemID, Equal<Required<POVendorInventory.subItemID>>,
											Or<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>,
											Or<POVendorInventory.subItemID, IsNull,
											Or<Where<Required<POVendorInventory.subItemID>, IsNull,
											     And<POVendorInventory.subItemID, Equal<True>>>>>>>,
							 And2<Where<POVendorInventory.purchaseUnit, Equal<Required<POVendorInventory.purchaseUnit>>>,
							 And<Where<POVendorInventory.vendorLocationID, Equal<Required<POVendorInventory.vendorLocationID>>,
											Or<POVendorInventory.vendorLocationID, IsNull>>>>>>>>>>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>, 
					OrderBy<
					Asc<Switch<Case<Where<POVendorInventory.purchaseUnit, Equal<InventoryItem.purchaseUnit>>, True>, False>,
					Asc<Switch<Case<Where<POVendorInventory.subItemID, Equal<InventoryItem.defaultSubItemID>>, True>, False>,
					Asc<Switch<Case<Where<POVendorInventory.vendorLocationID, IsNull>, True>, False>,
					Asc<InventoryItem.inventoryCD>>>>>>(graph);

			Func<string, PXResult<InventoryItem, INItemCost, POVendorInventory>> selectVendorCostByUOM =
				uomParam => vendorCostSelect
					.Select(baseCuryID, vendorID, curyID, subItemID, subItemID, uomParam, vendorLocationID, inventoryID).AsEnumerable()
					.FirstOrDefault(r => r.GetItem<POVendorInventory>() != null)
					as PXResult<InventoryItem, INItemCost, POVendorInventory>;

			var vendorCostRow = selectVendorCostByUOM(uom);
			var item = vendorCostRow.GetItem<InventoryItem>();

			Func<POVendorInventory, ItemCost> fetchVendorLastCost =
				vendorPrice => vendorPrice.LastPrice != null && vendorPrice.LastPrice != 0m
					? new ItemCost(item, vendorPrice.PurchaseUnit, curyID, vendorPrice.LastPrice.Value, false)
					: null;

			return fetchVendorLastCost(vendorCostRow.GetItem<POVendorInventory>())
				?? fetchVendorLastCost(selectVendorCostByUOM(item.BaseUnit).GetItem<POVendorInventory>())
				?? (onlyVendor ? null : FetchStdCost(graph, item, baseCuryID, docDate))
				?? (onlyVendor ? null : FetchSiteLastCost(graph, item, siteID, baseCuryID))
				?? new ItemCost(item, vendorCostRow.GetItem<INItemCost>()?.LastCost ?? 0);
		}

		public static void Update(PXGraph graph, int? vendorID, int? vendorLocationID, string curyID, int? inventoryID, int? subItemID, string uom, decimal curyCost)
		{
			if (curyCost <= 0 || string.IsNullOrEmpty(uom) ||
				vendorID == null ||
				vendorLocationID == null) return;
			
			PXCache cache = graph.Caches[typeof(POVendorInventoryPriceUpdate)];

			foreach (PXResult<InventoryItem, Vendor, Company> r in
				PXSelectReadonly2<InventoryItem,
					LeftJoinSingleTable<Vendor,
								 On<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>,
					CrossJoin<Company>>,
					Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.
					Select(graph, vendorID, inventoryID))
			{
				InventoryItem item = r;
				Vendor vendor = r;
				Company company = r;
				if (item.InventoryID == null || vendor.BAccountID == null || 
					(item.StkItem == true && subItemID == null)) continue;
				INSetup setup = PXSetupOptional<INSetup>.Select(graph);

                int? savedSubItemID = item.StkItem == true ? subItemID : null;

				POVendorInventoryPriceUpdate vendorPrice = (POVendorInventoryPriceUpdate)cache.CreateInstance();
				vendorPrice.InventoryID = inventoryID;
				vendorPrice.SubItemID = savedSubItemID;
				vendorPrice.VendorID = vendorID;
				vendorPrice.VendorLocationID = vendorLocationID;
				vendorPrice.PurchaseUnit = uom;
				vendorPrice = (POVendorInventoryPriceUpdate)cache.Insert(vendorPrice);
				if (item.StkItem != true) vendorPrice.SubItemID = savedSubItemID;
				vendorPrice.CuryID = curyID;
				cache.Normalize();
				vendorPrice.Active = true;
				vendorPrice.LastPrice = curyCost;
			}
		}

		public static Decimal ConvertUOM(PXGraph graph, InventoryItem item, string uom, decimal cost, string destinationUOM)
		{
			if (item == null) return 0;
			if (destinationUOM == uom) return cost;

			PXCache cache = graph.Caches[typeof(InventoryItem)];

			ConvertUOM(cache, item.InventoryID, uom, item.BaseUnit, true, ref cost);
			ConvertUOM(cache, item.InventoryID, item.BaseUnit, destinationUOM, false, ref cost);

			return cost;
		}

		private static void ConvertUOM(PXCache cache, int? inventoryID, string sourceUom, string destinationUom, bool viceVersa, ref decimal cost)
		{
			if (sourceUom != destinationUom && cost != 0)
			{
				INUnit conversion = INUnit.UK.ByInventory.FindDirty(cache.Graph, inventoryID, viceVersa ? sourceUom : destinationUom);
				if (conversion == null)
					throw new PXUnitConversionException();
				cost = INUnitAttribute.Convert(cache, conversion, cost, INPrecision.UNITCOST, viceVersa);
			}
		}

		[POVendorInventoryAccumulator]
        [Serializable]
		public partial class POVendorInventoryPriceUpdate : POVendorInventory
		{
			#region RecordID
			public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
			[PXDBIdentity()]
			public override Int32? RecordID
			{
				get
				{
					return this._RecordID;
				}
				set
				{
					this._RecordID = value;
				}
			}
			#endregion
			#region VendorID
			public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[VendorNonEmployeeActive(Visibility = PXUIVisibility.SelectorVisible,
				DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, DisplayName = "Vendor ID", IsKey = true)]
			public override Int32? VendorID
			{
				get
				{
					return this._VendorID;
				}
				set
				{
					this._VendorID = value;
				}
			}
			#endregion
			#region VendorLocationID
			public new abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
			[LocationID(typeof(Where<Location.bAccountID, Equal<Current<POVendorInventoryPriceUpdate.vendorID>>>),
				DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location", IsKey = true)]			
			public override Int32? VendorLocationID
			{
				get
				{
					return this._VendorLocationID;
				}
				set
				{
					this._VendorLocationID = value;
				}
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			[Inventory(Filterable = true, DirtyRead = true, IsKey = true)]			
			public override Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region SubItemID
			public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
			[SubItem(typeof(POVendorInventoryPriceUpdate.inventoryID), DisplayName = "Subitem", BqlField = typeof(POVendorInventory.subItemID), IsKey = true)]
			public override Int32? SubItemID
			{
				get
				{
					return this._SubItemID;
				}
				set
				{
					this._SubItemID = value;
				}
			}
			#endregion	
			#region PurchaseUnit
			public new abstract class purchaseUnit : PX.Data.BQL.BqlString.Field<purchaseUnit> { }
			[INUnit(typeof(POVendorInventory.inventoryID), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible, IsKey = true)]
			public override String PurchaseUnit
			{
				get
				{
					return this._PurchaseUnit;
				}
				set
				{
					this._PurchaseUnit = value;
				}
			}
			#endregion
			#region LastPrice
			public new abstract class lastPrice : PX.Data.BQL.BqlDecimal.Field<lastPrice> { }
			[PXDBPriceCost(BqlField = typeof(POVendorInventory.lastPrice))]
			[PXUIField(DisplayName = "Last Vendor Price", Enabled = false)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			public override Decimal? LastPrice
			{
				get
				{
					return this._LastPrice;
				}
				set
				{
					this._LastPrice = value;
				}
			}
			#endregion
			#region Active
			public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Active")]
			public override Boolean? Active
			{
				get
				{
					return this._Active;
				}
				set
				{
					this._Active = value;
				}
			}
			#endregion

			#region System Columns
			#region tstamp
			public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }			
			public override Byte[] tstamp
			{
				get
				{
					return this._tstamp;
				}
				set
				{
					this._tstamp = value;
				}
			}
			#endregion
			#endregion
		}
		public class POVendorInventoryAccumulatorAttribute : PXAccumulatorAttribute
		{
			public POVendorInventoryAccumulatorAttribute()
			{
				base._SingleRecord = true;
			}
			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))				
					return false;				

				POVendorInventoryPriceUpdate val = (POVendorInventoryPriceUpdate)row;
				columns.Update<POVendorInventoryPriceUpdate.curyID>(val.CuryID, PXDataFieldAssign.AssignBehavior.Replace);				
				columns.Update<POVendorInventoryPriceUpdate.lastPrice>(val.LastPrice, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<POVendorInventoryPriceUpdate.active>(val.Active, PXDataFieldAssign.AssignBehavior.Replace);				
				return true;
			}
		}
	}
}
