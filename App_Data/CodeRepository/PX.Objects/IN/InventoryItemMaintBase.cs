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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.IN.GraphExtensions.InventoryItemMaintBaseExt;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.SM;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.IN
{
	public abstract class InventoryItemMaintBase : PXGraph<InventoryItemMaintBase, InventoryItem>
	{
		public abstract bool IsStockItemFlag { get; }

		public virtual bool DefaultSiteFromItemClass { get; set; } = true;

		#region Public members
		public bool doResetDefaultsOnItemClassChange;
		#endregion

		#region Initialization
		public InventoryItemMaintBase()
		{
			// PXSetupOptional initialization.
			INSetup record = insetup.Current;
			SOSetup soSetup = sosetup.Current;
			CommonSetup commonSetup = commonsetup.Current;

			PXDBDefaultAttribute.SetDefaultForInsert<INItemXRef.inventoryID>(itemxrefrecords.Cache, null, true);

			PXUIFieldAttribute.SetVisible<INComponent.amtOption>(Components.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.aSC606>() != true);
		}
		#endregion

		#region Cache Attached
		#region INItemClass
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[INParentItemClass]
		protected virtual void _(Events.CacheAttached<INItemClass.parentItemClassID> e) { }
		#endregion
		#region POVendorInventory
		[LocationID(typeof(Where<Location.bAccountID.IsEqual<POVendorInventory.vendorID.FromCurrent>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		[PXFormula(typeof(Selector<POVendorInventory.vendorID, Vendor.defLocationID>))]
		[PXParent(typeof(
			SelectFrom<Location>.
			Where<
				Location.bAccountID.IsEqual<POVendorInventory.vendorID.FromCurrent>.
				And<Location.locationID.IsEqual<POVendorInventory.vendorLocationID.FromCurrent>>>))]
		protected virtual void _(Events.CacheAttached<POVendorInventory.vendorLocationID> e) { }
		#endregion
		#region INComponent
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(DRDeferredCode.deferredCodeID))]
		[PXRestrictor(typeof(Where<
			DRDeferredCode.multiDeliverableArrangement.IsNotEqual<True>.
			And<DRDeferredCode.accountType.IsEqual<DeferredAccountType.income>>>), DR.Messages.ComponentsCantUseMDA)]
		protected virtual void _(Events.CacheAttached<INComponent.deferredCode> e) { }
		#endregion
		#region INItemXRef
		[PXParent(typeof(INItemXRef.FK.InventoryItem))]
		[Inventory(Filterable = true, DirtyRead = true, Enabled = false, IsKey = true)]
		[PXDBDefault(typeof(InventoryItem.inventoryID), DefaultForInsert = true, DefaultForUpdate = false)]
		protected virtual void _(Events.CacheAttached<INItemXRef.inventoryID> e) { }
		#endregion
		#region INItemCategory
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(INCategory.categoryID), DescriptionField = typeof(INCategory.description))]
		[PXUIField(DisplayName = "Category ID")]
		protected virtual void _(Events.CacheAttached<INItemCategory.categoryID> e) { }
		#endregion
		#endregion

		#region Selects
		[PXViewName(Messages.InventoryItem)]
		public
			SelectFrom<InventoryItem>.
			Where<
				InventoryItem.itemStatus.IsNotEqual<InventoryItemStatus.unknown>.
				And<InventoryItem.isTemplate.IsEqual<False>>.
				And<MatchUser>>.
			View Item;

		public SelectFrom<InventoryItemCurySettings>.
			Where<InventoryItemCurySettings.inventoryID.IsEqual<InventoryItem.inventoryID.AsOptional>.
				And<InventoryItemCurySettings.curyID.IsEqual<AccessInfo.baseCuryID.AsOptional>>>.View ItemCurySettings;

		public virtual IEnumerable itemCurySettings()
		{
			InventoryItemCurySettings curyrecord =
				(InventoryItemCurySettings)new PXView(this, false, ItemCurySettings.View.BqlSelect).SelectSingle(PXView.Parameters);
			if (curyrecord == null && PXView.Parameters.Length != 0
				&& PXView.Parameters[0] is int inventoryID && Item.Current?.InventoryID == inventoryID)
			{
				bool itemCurySettingsIsDirty = ItemCurySettings.Cache.IsDirty;
				bool itemIsDirty = Item.Cache.IsDirty;
				curyrecord = (InventoryItemCurySettings)ItemCurySettings.Cache.Insert();
				ItemCurySettings.Cache.IsDirty = itemCurySettingsIsDirty;
				Item.Cache.IsDirty = itemIsDirty;
			}
			yield return curyrecord;
		}

		public SelectFrom<InventoryItemCurySettings>.
			Where<InventoryItemCurySettings.inventoryID.IsEqual<@P.AsInt>>.View AllItemCurySettings;

		[PXCopyPasteHiddenView]
		public INSubItemSegmentValueList SegmentValues;

		[PXCopyPasteHiddenFields(typeof(InventoryItem.body), typeof(InventoryItem.imageUrl))]
		public
			SelectFrom<InventoryItem>.
			Where<InventoryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.
			View ItemSettings;

		public
			SelectFrom<INComponent>.
			Where<INComponent.FK.InventoryItem.SameAsCurrent>.
			View Components;

		[PXDependToCache(typeof(InventoryItem))]
		public
			SelectFrom<INCategory>.
			OrderBy<INCategory.sortOrder.Asc>.
			View Categories;
		protected virtual IEnumerable categories([PXInt] int? categoryID) => GetCategories(categoryID);

		public
			SelectFrom<ARSalesPrice>.
			View SalesPrice;

		public PXSetupOptional<INSetup> insetup;
		public PXSetupOptional<SOSetup> sosetup;
		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<GL.Company> Company;

		public
			SelectFrom<INItemClass>.
			Where<INItemClass.itemClassID.IsEqual<InventoryItem.itemClassID.AsOptional>>.
			View ItemClass;

		public
			POVendorInventorySelect<POVendorInventory,
			InnerJoin<Vendor, On<Vendor.bAccountID.IsEqual<POVendorInventory.vendorID>>,
			LeftJoin<CRLocation, On<
				CRLocation.bAccountID.IsEqual<POVendorInventory.vendorID>.
				And<CRLocation.locationID.IsEqual<POVendorInventory.vendorLocationID>>>>>,
			Where<POVendorInventory.inventoryID, Equal<Current<InventoryItem.inventoryID>>,
				And<Where<Vendor.baseCuryID, Equal<Current<AccessInfo.baseCuryID>>,
					Or<Vendor.baseCuryID, IsNull>>>>,
			InventoryItem> VendorItems;

		public
			SelectFrom<INItemXRef>.
			Where<INItemXRef.FK.InventoryItem.SameAsCurrent>.
			View itemxrefrecords;

		public CRAttributeList<InventoryItem> Answers;

		public
			SelectFrom<INItemCategory>.
			InnerJoin<INCategory>.On<INItemCategory.FK.Category>.
			Where<INItemCategory.FK.InventoryItem.SameAsCurrent>.
			View Category;

		[PXDependToCache(typeof(InventoryItem))]
		public
			SelectFrom<CacheEntityItem>.
			Where<CacheEntityItem.path.IsEqual<CacheEntityItem.path>>.
			OrderBy<CacheEntityItem.number.Asc>.
			View EntityItems;
		protected IEnumerable entityItems(string parent) => GetEntityItems(parent);

		public
			SelectFrom<ARPriceWorksheetDetail>.
			View arpriceworksheetdetails;

		public
			SelectFrom<DiscountItem>.
			View discountitems;

		public
			SelectFrom<PM.PMItemRate>.
			View pmitemrates;

		public
			SelectFrom<INKitSpecHdr>.
			View kitheaders;

		public
			SelectFrom<INKitSpecStkDet>.
			View kitspecs;

		public
			SelectFrom<INKitSpecNonStkDet>.
			View kitnonstockdet;

		public
			SelectFrom<BAccount>.
			View dummy_BAccount;

		public
			SelectFrom<Vendor>.
			View dummy_Vendor;
		#endregion

		#region Actions
		public PXChangeID<InventoryItem, InventoryItem.inventoryCD> ChangeID;

		public PXAction<InventoryItem> updateCost;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Update Cost", MapEnableRights = PXCacheRights.Update)]
		protected virtual IEnumerable UpdateCost(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter a)
		{
			foreach (InventoryItem e in new PXCancel<InventoryItem>(this, "Cancel").Press(a))
			{
				if (Item.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					if (InventoryItem.UK.Find(this, e.InventoryCD) is InventoryItem duplicate)
					{
						Item.Cache.RaiseExceptionHandling<InventoryItem.inventoryCD>(e, e.InventoryCD,
							new PXSetPropertyException(
								duplicate.IsTemplate == true ? Messages.TemplateItemExists :
								duplicate.StkItem == true ? Messages.StockItemExists : Messages.NonStockItemExists));
					}
				}
				yield return e;
			}
		}

		public PXAction<InventoryItem> viewSalesPrices;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Sales Prices", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable ViewSalesPrices(PXAdapter adapter)
		{
			if (Item.Current != null)
			{
				ARSalesPriceMaint graph = PXGraph.CreateInstance<ARSalesPriceMaint>();
				graph.Filter.Current.InventoryID = Item.Current.InventoryID;
				throw new PXRedirectRequiredException(graph, "Sales Prices")
				{
					Mode = PXBaseRedirectException.WindowMode.New
				};
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> viewVendorPrices;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Vendor Prices", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable ViewVendorPrices(PXAdapter adapter)
		{
			if (Item.Current != null)
			{
				APVendorPriceMaint graph = PXGraph.CreateInstance<APVendorPriceMaint>();
				graph.Filter.Current.InventoryID = Item.Current.InventoryID;
				throw new PXRedirectRequiredException(graph, "Vendor Prices")
				{
					Mode = PXBaseRedirectException.WindowMode.New
				};
			}
			return adapter.Get();
		}

		public PXAction<InventoryItem> viewRestrictionGroups;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Manage Restriction Groups", MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (Item.Current != null)
			{
				INAccessDetailByItem graph = CreateInstance<INAccessDetailByItem>();
				graph.Item.Current = graph.Item.Search<InventoryItem.inventoryCD>(Item.Current.InventoryCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}
		#endregion

		#region Event handlers
		#region InventoryItem
		protected virtual void _(Events.RowInserted<InventoryItem> e)
		{
			if (e.Row.InventoryCD != null && e.Row.IsConversionMode != true)
			{
				using (new ReadOnlyScope(ItemCurySettings.Cache))
					SetDefaultSiteID(e.Row);
			}
		}

		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			if (e.Row == null)
				return;

			if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
			{
				PXUIFieldAttribute.SetWarning<InventoryItem.weightUOM>(e.Cache, e.Row, string.IsNullOrEmpty(commonsetup.Current.WeightUOM) ? Messages.BaseCompanyUomIsNotDefined : null);
				PXUIFieldAttribute.SetWarning<InventoryItem.volumeUOM>(e.Cache, e.Row, string.IsNullOrEmpty(commonsetup.Current.VolumeUOM) ? Messages.BaseCompanyUomIsNotDefined : null);
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				PXUIFieldAttribute.SetVisible<InventoryItem.totalPercentage>(e.Cache, null, false);
			}

			//Multiple Components are not supported for CashReceipt Deferred Revenue:
			DRDeferredCode dc = SelectFrom<DRDeferredCode>.Where<DRDeferredCode.deferredCodeID.IsEqual<InventoryItem.deferredCode.FromCurrent>>.View.Select(this);
			PXUIFieldAttribute.SetEnabled<POVendorInventory.isDefault>(VendorItems.Cache, null, true);

			SetDefaultTermControlsState(e.Cache, e.Row);

			//Initial State for Components:
			Components.Cache.AllowDelete = false;
			Components.Cache.AllowInsert = false;
			Components.Cache.AllowUpdate = false;

			if (e.Row.IsSplitted == true)
			{
				Components.Cache.AllowDelete = true;
				Components.Cache.AllowInsert = true;
				Components.Cache.AllowUpdate = true;
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification: virtual field]
				e.Row.TotalPercentage = SumComponentsPercentage();
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(e.Cache, e.Row, true);
			}
			else
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification: virtual field]
				e.Row.TotalPercentage = 100;
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification: virtual field]
				e.Row.UseParentSubID = false;
				PXUIFieldAttribute.SetEnabled<InventoryItem.useParentSubID>(e.Cache, e.Row, false);
			}

			InventoryHelper.CheckZeroDefaultTerm<InventoryItem.deferredCode, InventoryItem.defaultTerm>(e.Cache, e.Row);

			e.Cache.AdjustUI()
				.For<InventoryItem.itemClassID>(a => a.Enabled = e.Row.TemplateItemID == null)
				.SameFor<InventoryItem.baseUnit>()
				.SameFor<InventoryItem.decimalBaseUnit>()
				.SameFor<InventoryItem.itemType>()
				.SameFor<InventoryItem.taxCategoryID>();
		}

		protected virtual void _(Events.RowPersisting<InventoryItem> e)
		{
			if (e.Operation.Command() == PXDBOperation.Update && e.Row.KitItem != true && (bool?)Item.Cache.GetValueOriginal<InventoryItem.kitItem>(e.Row) == true)
			{
				INKitSpecHdr kitSpec =
					SelectFrom<INKitSpecHdr>.
					Where<INKitSpecHdr.kitInventoryID.IsEqual<@P.AsInt>>.
					View.ReadOnly.SelectWindowed(this, 0, 1, e.Row.InventoryID);

				if (kitSpec != null)
					if (e.Cache.RaiseExceptionHandling<InventoryItem.kitItem>(e.Row, e.Row.KitItem, new PXSetPropertyException<InventoryItem.kitItem>(Messages.KitSpecificationExists)))
						throw new PXRowPersistingException(nameof(InventoryItem.kitItem), e.Row.KitItem, Messages.KitSpecificationExists);
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.deferredCode> e)
		{
			UpdateSplittedFromDeferralCode(e.Cache, e.Row);
			SetDefaultTerm(e.Cache, e.Row, typeof(InventoryItem.deferredCode), typeof(InventoryItem.defaultTerm), typeof(InventoryItem.defaultTermUOM));
		}

		public static void UpdateSplittedFromDeferralCode(PXCache cache, InventoryItem item)
		{
			if (item == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(cache, item);
			cache.SetValueExt<InventoryItem.isSplitted>(item, code != null && code.MultiDeliverableArrangement == true);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> e)
		{
			if (e.Row != null && e.Row.ItemClassID < 0)
			{
				INItemClass ic = ItemClass.Select();
				e.Row.ParentItemClassID = ic?.ParentItemClassID;
			}
			else if (e.Row != null)
			{
				e.Row.ParentItemClassID = e.Row.ItemClassID;
			}

			if (e.Row != null && e.Row.ItemClassID != null && e.ExternalCall)
				Answers.Cache.Clear();

			if (Item.Current?.IsConversionMode == true)
			{
				if (GetCurySettings(e.Row.InventoryID)?.DfltSiteID == null)
					SetDefaultSiteID(e.Row, false);

				if (e.Row.DeferredCode == null)
					e.Cache.SetDefaultExt<InventoryItem.deferredCode>(e.Row);

				e.Cache.SetDefaultExt<InventoryItem.postClassID>(e.Row);

				if (e.Row.MarkupPct.IsIn(null, 0m))
					e.Cache.SetDefaultExt<InventoryItem.markupPct>(e.Row);

				if (e.Row.MinGrossProfitPct.IsIn(null, 0m))
					e.Cache.SetDefaultExt<InventoryItem.minGrossProfitPct>(e.Row);

				if (e.Row.TaxCategoryID == null)
					e.Cache.SetDefaultExt<InventoryItem.taxCategoryID>(e.Row);

				if (e.Row.PriceClassID == null)
					e.Cache.SetDefaultExt<InventoryItem.priceClassID>(e.Row);

				INItemClass ic = ItemClass.Select();

				if (e.Row.PriceWorkgroupID == null)
					e.Cache.SetValue<InventoryItem.priceWorkgroupID>(e.Row, ic?.PriceWorkgroupID);

				if (e.Row.PriceManagerID == null)
					e.Cache.SetValue<InventoryItem.priceManagerID>(e.Row, ic?.PriceManagerID);

				if (e.Row.UndershipThreshold.IsIn(null, 100m))
					e.Cache.SetDefaultExt<InventoryItem.undershipThreshold>(e.Row);

				if (e.Row.OvershipThreshold.IsIn(null, 100m))
					e.Cache.SetDefaultExt<InventoryItem.overshipThreshold>(e.Row);
			}
			else if (doResetDefaultsOnItemClassChange)
			{
				ResetConversionsSettings(e.Cache, e.Row);

				if (PXAccess.FeatureInstalled<FeaturesSet.inventory>())
				{
					SetDefaultSiteID(e.Row);
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
				{
					e.Cache.SetDefaultExt<InventoryItem.deferredCode>(e.Row);
					e.Cache.SetDefaultExt<InventoryItem.postClassID>(e.Row);
					e.Cache.SetDefaultExt<InventoryItem.markupPct>(e.Row);
					e.Cache.SetDefaultExt<InventoryItem.minGrossProfitPct>(e.Row);
				}

				e.Cache.SetDefaultExt<InventoryItem.exportToExternal>(e.Row);

				e.Cache.SetDefaultExt<InventoryItem.taxCategoryID>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.itemType>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.priceClassID>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.priceWorkgroupID>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.priceManagerID>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.undershipThreshold>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.overshipThreshold>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.commodityCodeType>(e.Row);
				e.Cache.SetDefaultExt<InventoryItem.hSTariffCode>(e.Row);

				INItemClass ic = ItemClass.Select();
				if (ic != null)
				{
					e.Cache.SetValue<InventoryItem.priceWorkgroupID>(e.Row, ic.PriceWorkgroupID);
					e.Cache.SetValue<InventoryItem.priceManagerID>(e.Row, ic.PriceManagerID);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<InventoryItem, InventoryItem.itemClassID> e)
		{
			doResetDefaultsOnItemClassChange = false;
			INItemClass ic = ItemClass.Select(e.NewValue);

			if (ic != null)
			{
				doResetDefaultsOnItemClassChange = true;
				if (e.ExternalCall && e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted && e.Row?.IsConversionMode != true)
					if (Item.Ask(AR.Messages.Warning, Messages.ItemClassChangeWarning, MessageButtons.YesNo) == WebDialogResult.No)
						doResetDefaultsOnItemClassChange = false;
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.isSplitted> e)
		{
			if (e.Row != null)
			{
				if (e.Row.IsSplitted == false)
				{
					foreach (INComponent c in Components.Select())
						Components.Delete(c);

					e.Row.TotalPercentage = 100;
				}
				else
					e.Row.TotalPercentage = 0;
			}
		}
		#endregion
		#region INItemXRef
		protected virtual void _(Events.RowSelected<INItemXRef> e)
		{
			if (e.Row == null) return;
			
			VerifyXRefUOMExists(e.Cache, e.Row);

			if (Item.Current != null && e.Row.AlternateID != null && !PXDimensionAttribute.MatchMask<InventoryItem.inventoryCD>(Item.Cache, e.Row.AlternateID))
				e.Cache.RaiseExceptionHandling<INItemXRef.alternateID>(e.Row, e.Row.AlternateID, new PXSetPropertyException(Messages.AlternateIDDoesNotCorrelateWithCurrentSegmentRules, PXErrorLevel.Warning));

			PXUIFieldAttribute.SetEnabled<INItemXRef.bAccountID>(e.Cache, e.Row, e.Row.AlternateType.IsIn(INAlternateType.CPN, INAlternateType.VPN));
			PXUIFieldAttribute.SetEnabled<INItemXRef.alternateType>(e.Cache, e.Row, (e.Row.BAccountID == null || e.Row.BAccountID == 0) && e.Row.AlternateID == null);
		}

		private void VerifyXRefUOMExists(PXCache sender, INItemXRef row)
		{
			sender.RaiseExceptionHandling<INItemXRef.uOM>(
				row,
				row.UOM,
				row.UOM.IsIn(UnitsOfMeasureExt.itemunits.Select().RowCast<INUnit>().Select(u => u.FromUnit).Concat(new[] { null, Item.Current.BaseUnit }))
				? null
				: new PXSetPropertyException(Messages.UOMAssignedToAltIDIsNotDefined, PXErrorLevel.Warning));
		}

		protected virtual void _(Events.FieldVerifying<INItemXRef, INItemXRef.alternateID> e)
		{
			if (e.Row == null || Item.Current == null || e.NewValue == null)
				return;

			e.NewValue = ((String)e.NewValue).Trim();
			if ((String)e.NewValue == String.Empty)
				e.NewValue = null;
		}

		protected virtual void _(Events.FieldDefaulting<INItemXRef, INItemXRef.inventoryID> e)
		{
			if (Item.Current != null)
			{
				e.NewValue = Item.Current.InventoryID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldVerifying<INItemXRef, INItemXRef.inventoryID> e) => e.Cancel = true;

		protected virtual void _(Events.RowPersisting<INItemXRef> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update)
				&& e.Row.AlternateType.IsNotIn(INAlternateType.VPN, INAlternateType.CPN))
			{
				e.Row.BAccountID = 0;
				e.Cache.Normalize();
			}
		}

		protected virtual void _(Events.FieldVerifying<INItemXRef, INItemXRef.bAccountID> e)
		{
			if (e.Row.AlternateType.IsNotIn(INAlternateType.VPN, INAlternateType.CPN))
				e.Cancel = true;
		}

		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2022R2)]
		protected virtual void _(Events.FieldUpdating<INItemXRef, INItemXRef.bAccountID> e)
		{ }
		#endregion
		#region INComponent
		protected virtual void _(Events.FieldDefaulting<INComponent, INComponent.percentage> e)
		{
			if (e.Row != null && e.Row.AmtOption == INAmountOption.Percentage)
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Justification: virtual field]
				e.Row.Percentage = GetRemainingPercentage();
			}
		}

		protected virtual void _(Events.FieldUpdated<INComponent, INComponent.componentID> e)
		{
			if (e.Row != null)
			{
				InventoryItem item = InventoryItem.PK.FindDirty(this, e.Row.ComponentID);
				var deferralCode = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(Item.Cache, item);
				bool useDeferralFromItem = deferralCode != null && deferralCode.MultiDeliverableArrangement != true;

				if (item != null)
				{
					e.Row.SalesAcctID = item.SalesAcctID;
					e.Row.SalesSubID = item.SalesSubID;
					e.Row.UOM = item.SalesUnit;
					e.Row.DeferredCode = useDeferralFromItem ? item.DeferredCode : null;
					e.Row.DefaultTerm = item.DefaultTerm;
					e.Row.DefaultTermUOM = item.DefaultTermUOM;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<INComponent, INComponent.amtOption> e)
		{
			if (e.Row != null)
			{
				if (e.Row.AmtOption == INAmountOption.Percentage)
				{
					e.Row.FixedAmt = null;
					e.Row.Percentage = GetRemainingPercentage();
				}
				else
				{
					e.Row.Percentage = 0;
				}

				if (e.Row.AmtOption == INAmountOption.Residual)
				{
					e.Cache.SetValueExt<INComponent.deferredCode>(e.Row, null);
					e.Cache.SetDefaultExt<INComponent.fixedAmt>(e.Row);
					e.Cache.SetDefaultExt<INComponent.percentage>(e.Row);
				}
			}
		}

		protected virtual void _(Events.RowSelected<INComponent> e)
		{
			SetComponentControlsState(e.Cache, e.Row);
			InventoryHelper.CheckZeroDefaultTerm<INComponent.deferredCode, INComponent.defaultTerm>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<INComponent, INComponent.deferredCode> e)
		{
			SetDefaultTerm(e.Cache, e.Row, typeof(INComponent.deferredCode), typeof(INComponent.defaultTerm), typeof(INComponent.defaultTermUOM));
		}

		public static void SetDefaultTerm(PXCache cache, object row, Type deferralCode, Type defaultTerm, Type defaultTermUOM)
		{
			if (row == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select(cache, row, deferralCode.Name);

			if (code == null || DeferredMethodType.RequiresTerms(code) == false)
			{
				cache.SetDefaultExt(row, defaultTerm.Name);
				cache.SetDefaultExt(row, defaultTermUOM.Name);
			}
		}

		public static void SetComponentControlsState(PXCache cache, INComponent component)
		{
			bool disabledFixedAmtAndPercentage = false;

			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				disabledFixedAmtAndPercentage = true;
				PXUIFieldAttribute.SetEnabled<INComponent.amtOption>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<INComponent.fixedAmt>(cache, null, false);
				PXUIFieldAttribute.SetEnabled<INComponent.percentage>(cache, null, false);
				PXDefaultAttribute.SetPersistingCheck<INComponent.amtOption>(cache, null, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INComponent.fixedAmt>(cache, null, PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<INComponent.percentage>(cache, null, PXPersistingCheck.Nothing);

				PXUIFieldAttribute.SetVisible<INComponent.amtOption>(cache, null, false);
				PXUIFieldAttribute.SetVisible<INComponent.fixedAmt>(cache, null, false);
				PXUIFieldAttribute.SetVisible<INComponent.percentage>(cache, null, false);
			}

			if (component == null)
				return;

			bool isResidual = false;
			bool isResidualEnabled = true;
			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				isResidual = component.AmtOptionASC606 == INAmountOption.Residual;
			}
			else
			{
				isResidual = component.AmtOption == INAmountOption.Residual;
				isResidualEnabled = !isResidual;
			}

			PXDefaultAttribute.SetPersistingCheck<INComponent.deferredCode>(cache, component, isResidual ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);

			PXUIFieldAttribute.SetEnabled<INComponent.deferredCode>(cache, component, isResidualEnabled);
			PXUIFieldAttribute.SetEnabled<INComponent.fixedAmt>(cache, component, disabledFixedAmtAndPercentage == false && component.AmtOption == INAmountOption.FixedAmt);
			PXUIFieldAttribute.SetEnabled<INComponent.percentage>(cache, component, disabledFixedAmtAndPercentage == false && component.AmtOption == INAmountOption.Percentage);

			var code = (DRDeferredCode)PXSelectorAttribute.Select<INComponent.deferredCode>(cache, component);
			bool enableTerms = DeferredMethodType.RequiresTerms(code) && isResidualEnabled;
			PXUIFieldAttribute.SetEnabled<INComponent.defaultTerm>(cache, component, enableTerms);
			PXUIFieldAttribute.SetEnabled<INComponent.defaultTermUOM>(cache, component, enableTerms);
			PXUIFieldAttribute.SetEnabled<INComponent.overrideDefaultTerm>(cache, component, enableTerms);
		}
		#endregion
		#region INUnit
		protected virtual void _(Events.RowSelected<INUnit> e)
		{
			PXFieldState state = (PXFieldState)e.Cache.GetStateExt<INUnit.fromUnit>(e.Row);
			if (Item.Current != null && e.Row.ToUnit == Item.Current.BaseUnit && (state.Error == null || state.Error == PXMessages.Localize(Messages.BaseUnitNotSmallest, out string _) || state.ErrorLevel == PXErrorLevel.RowInfo))
			{
				if (e.Row.UnitMultDiv == MultDiv.Multiply && e.Row.UnitRate < 1 || e.Row.UnitMultDiv == MultDiv.Divide && e.Row.UnitRate > 1)
					e.Cache.RaiseExceptionHandling<INUnit.fromUnit>(e.Row, e.Row.FromUnit, new PXSetPropertyException(Messages.BaseUnitNotSmallest, PXErrorLevel.RowWarning));
				else
					e.Cache.RaiseExceptionHandling<INUnit.fromUnit>(e.Row, e.Row.FromUnit, null);
			}
		}

		public virtual decimal? GetUnitRate(PXCache sender, INUnit unit, int? itemClassID)
		{
			decimal? unitRate = unit != null ? unit.UnitRate ?? 1m : 1m;
			INUnit existingUnit =
				SelectFrom<INUnit>.
				Where<
					INUnit.unitType.IsNotEqual<INUnitType.inventoryItem>.
					And<INUnit.fromUnit.IsEqual<INUnit.fromUnit.FromCurrent>>.
					And<INUnit.toUnit.IsEqual<INUnit.toUnit.FromCurrent>>.
					And<INUnit.unitMultDiv.IsEqual<INUnit.unitMultDiv.FromCurrent>>.
					And<
						INUnit.itemClassID.IsEqual<@P.AsInt>.
						Or<INUnit.unitType.IsEqual<INUnitType.global>>>>.
				OrderBy<INUnit.unitType.Asc>.
				View.SelectSingleBound(sender.Graph, new object[] { unit }, itemClassID);
			if (existingUnit != null)
				unitRate = existingUnit.UnitRate;

			return unitRate;
		}

		public UnitsOfMeasure UnitsOfMeasureExt => FindImplementation<UnitsOfMeasure>();

		protected virtual void _(Events.RowUpdated<INUnit> e)
		{
			if (e.Row != null && e.Row.FromUnit != null && !UnitsOfMeasureExt.itemunits.Cache.ObjectsEqual<INUnit.fromUnit>(e.Row, e.OldRow) && Item.Current != null)
				e.Row.UnitRate = GetUnitRate(e.Cache, e.Row, Item.Current.ItemClassID);

			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}

		protected virtual void _(Events.RowInserted<INUnit> e)
		{
			if (e.Row != null && e.Row.FromUnit != null && Item.Current != null)
				e.Row.UnitRate = GetUnitRate(e.Cache, e.Row, Item.Current.ItemClassID);

			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}

		protected virtual void _(Events.RowDeleted<INUnit> e)
		{
			foreach (var row in itemxrefrecords.Select())
				VerifyXRefUOMExists(itemxrefrecords.Cache, row);
		}
		#endregion
		#region InventoryItemCurySettings
		protected virtual void _(Events.FieldDefaulting<InventoryItemCurySettings, InventoryItemCurySettings.preferredVendorID> e)
		{
			foreach(InventoryItemCurySettings curySettings in AllItemCurySettings.Select(e.Row?.InventoryID))
			{
				if (e.Row != curySettings && curySettings?.PreferredVendorID != null)
				{
					var vendor = Vendor.PK.Find(this, curySettings.PreferredVendorID);
					if (vendor != null && vendor.BaseCuryID == null)
						e.NewValue = curySettings.PreferredVendorID;

					return;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<InventoryItemCurySettings, InventoryItemCurySettings.preferredVendorLocationID> e)
		{
			foreach (InventoryItemCurySettings curySettings in AllItemCurySettings.Select(e.Row?.InventoryID))
			{
				if (e.Row != curySettings && curySettings?.PreferredVendorID != null)
				{
					var vendor = Vendor.PK.Find(this, curySettings.PreferredVendorID);
					if (vendor != null && vendor.BaseCuryID == null)
						e.NewValue = curySettings.PreferredVendorLocationID;

					return;
				}
			}
		}
		#endregion // InventoryItemCurySettings
		#region POVendorInventory
		protected virtual void _(Events.FieldDefaulting<POVendorInventory, POVendorInventory.isDefault> e)
		{
			if ((POVendorInventory)VendorItems.SelectWindowed(0, 1, e.Row.InventoryID) == null)
			{
				e.NewValue = true;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowDeleted<POVendorInventory> e)
		{
			if (Item.Cache.GetStatus(Item.Current).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
				return;

			var curySettingsRows = AllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
			var vendor = Vendor.PK.Find(this, e.Row.VendorID);
			if (vendor == null)
				return;

			foreach (var upd in curySettingsRows)
			{
				object isdefault = e.Cache.GetValueExt<POVendorInventory.isDefault>(e.Row);

				if (isdefault is PXFieldState state)
					isdefault = state.Value;

				bool theSameCurrency = string.Equals(upd.CuryID, vendor.BaseCuryID, StringComparison.OrdinalIgnoreCase) ||
					vendor.BaseCuryID == null;

				if (upd != null && (bool?)isdefault == true && theSameCurrency)
				{
					upd.PreferredVendorID = null;
					upd.PreferredVendorLocationID = null;
					ItemCurySettings.Update(upd);
				}
			}
		}

		protected virtual void _(Events.RowInserted<POVendorInventory> e)
		{
			if (e.Row.VendorID != null && e.Row.IsDefault == true && (!IsStockItemFlag || e.Row.SubItemID != null))
			{
				GetCurySettings(e.Row.InventoryID);
				var curySettingsRows = AllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
				var newVendor = Vendor.PK.Find(this, e.Row.VendorID);

				foreach (var upd in curySettingsRows)
				{
					var vendor = Vendor.PK.Find(this, upd.PreferredVendorID);

					bool theSameCurrency = newVendor.BaseCuryID == null ||
						string.Equals(upd.CuryID, newVendor.BaseCuryID, StringComparison.OrdinalIgnoreCase);

					if (theSameCurrency)
					{
						upd.PreferredVendorID = e.Row.IsDefault == true ? e.Row.VendorID : null;
						upd.PreferredVendorLocationID = e.Row.IsDefault == true ? e.Row.VendorLocationID : null;
						ItemCurySettings.Update(upd);
					}
					else if (vendor != null && vendor.BaseCuryID == null)
					{
						upd.PreferredVendorID = null;
						upd.PreferredVendorLocationID = null;
						ItemCurySettings.Update(upd);
					}
				}
			}
		}

		protected virtual void _(Events.RowUpdated<POVendorInventory> e)
		{
			if (e.OldRow == null || e.Row == null || (IsStockItemFlag && e.Row.SubItemID == null))
				return;

			GetCurySettings(e.Row.InventoryID);
			var curySettingsRows = AllItemCurySettings.Select(e.Row.InventoryID).RowCast<InventoryItemCurySettings>();
			var newVendor = Vendor.PK.Find(this, e.Row.VendorID);

			bool updated = false;

			foreach (var upd in curySettingsRows)
			{
				var vendor = Vendor.PK.Find(this, upd.PreferredVendorID);

				bool theSameCurrency = newVendor == null || newVendor.BaseCuryID == null ||
					string.Equals(upd.CuryID, newVendor.BaseCuryID, StringComparison.OrdinalIgnoreCase);

				if (theSameCurrency)
				{
					if ((e.Row.IsDefault == true && (upd.PreferredVendorID != e.Row.VendorID || upd.PreferredVendorLocationID != e.Row.VendorLocationID) ||
						(e.Row.IsDefault != true && upd.PreferredVendorID == e.Row.VendorID && upd.PreferredVendorLocationID == e.Row.VendorLocationID)))
					{
						upd.PreferredVendorID = e.Row.IsDefault == true ? e.Row.VendorID : null;
						upd.PreferredVendorLocationID = e.Row.IsDefault == true ? e.Row.VendorLocationID : null;
						ItemCurySettings.Update(upd);
						updated = true;
					}
				}
				else if (vendor != null && vendor.BaseCuryID == null)
				{
					upd.PreferredVendorID = null;
					upd.PreferredVendorLocationID = null;
					ItemCurySettings.Update(upd);
				}
			}

			if (e.Row.IsDefault == true && updated)
			{
				foreach (POVendorInventory vendorInventory in VendorItems.Select())
				{
					if (vendorInventory.RecordID != e.Row.RecordID && vendorInventory.IsDefault == true)
						VendorItems.Cache.SetValue<POVendorInventory.isDefault>(vendorInventory, false);
				}
				VendorItems.Cache.ClearQueryCacheObsolete();
				VendorItems.View.RequestRefresh();
			}
		}
		#endregion
		#endregion

		public override void Persist()
		{
			using (PXTransactionScope tscope = new PXTransactionScope())
			{
				INItemClass itemClassOnTheFly = null;
				if (Item.Current != null && Item.Current.ItemClassID < 0)
				{
					itemClassOnTheFly = ItemClass.Select();
					var itemClassGraph = PXGraph.CreateInstance<INItemClassMaint>();
					var itemClassCopy = (INItemClass)itemClassGraph.itemclass.Cache.CreateCopy(itemClassOnTheFly);
					itemClassCopy = itemClassGraph.itemclass.Insert(itemClassCopy);
					itemClassGraph.Actions.PressSave();

					foreach (var row in ItemClass.Cache.Inserted)
					{
						ItemClass.Cache.SetStatus(row, PXEntryStatus.Held);
					}
					Item.Current.ItemClassID = itemClassGraph.itemclass.Current.ItemClassID;
				}
				try
				{
					base.Persist();
				}
				catch
				{
					if (itemClassOnTheFly != null)
					{
						Item.Current.ItemClassID = itemClassOnTheFly.ItemClassID;
						ItemClass.Cache.SetStatus(itemClassOnTheFly, PXEntryStatus.Inserted);
					}
					throw;
				}
				ItemClass.Cache.Clear();

				tscope.Complete();
			}
		}

		protected decimal GetRemainingPercentage() => Math.Max(0, 100 - SumComponentsPercentage());

		protected decimal SumComponentsPercentage() => Components.Select().RowCast<INComponent>().Where(c => c.AmtOption == INAmountOption.Percentage).Sum(c => c.Percentage ?? 0);

		public bool AlwaysFromBaseCurrency
		{
			get
			{
				bool alwaysFromBase = false;

				ARSetup arsetup = PXSelect<ARSetup>.Select(this);
				if (arsetup != null)
					alwaysFromBase = arsetup.AlwaysFromBaseCury == true;

				return alwaysFromBase;
			}
		}
		public string SalesPriceUpdateUnit => SalesPriceUpdateUnitType.BaseUnit;

		protected virtual void ResetConversionsSettings(PXCache cache, InventoryItem item)
		{
			//sales and purchase units must be cleared not to be added to item unit conversions on base unit change.
			cache.SetValueExt<InventoryItem.baseUnit>(item, null);
			cache.SetValue<InventoryItem.salesUnit>(item, null);
			cache.SetValue<InventoryItem.purchaseUnit>(item, null);

			cache.SetDefaultExt<InventoryItem.baseUnit>(item);
			cache.SetDefaultExt<InventoryItem.salesUnit>(item);
			cache.SetDefaultExt<InventoryItem.purchaseUnit>(item);

			cache.SetDefaultExt<InventoryItem.decimalBaseUnit>(item);
			cache.SetDefaultExt<InventoryItem.decimalSalesUnit>(item);
			cache.SetDefaultExt<InventoryItem.decimalPurchaseUnit>(item);
		}

		protected virtual void SetDefaultSiteID(InventoryItem item, bool allCurrencies = true)
		{
			if (!DefaultSiteFromItemClass || item == null)
				return;

			var itemClassCurySettingsRows = SelectFrom<INItemClassCurySettings>
				.Where<INItemClassCurySettings.itemClassID.IsEqual<@P.AsInt>
					.And<@P.AsBool.IsEqual<True>.Or<INItemClassCurySettings.curyID.IsEqual<AccessInfo.baseCuryID.FromCurrent>>>>
				.View.ReadOnly.Select(this, item.ParentItemClassID, allCurrencies)
				.RowCast<INItemClassCurySettings>().ToList();

			if (!itemClassCurySettingsRows.Any(i => string.Equals(i.CuryID, Accessinfo.BaseCuryID, StringComparison.OrdinalIgnoreCase)) && Accessinfo.BaseCuryID != null)
			{
				itemClassCurySettingsRows.Add(new INItemClassCurySettings() { CuryID = Accessinfo.BaseCuryID } );
			}

			IEnumerable<InventoryItemCurySettings> itemCurySettingsRows;

			if (allCurrencies)
			{
				itemCurySettingsRows = AllItemCurySettings.Select(item.InventoryID).RowCast<InventoryItemCurySettings>();
			}
			else
			{
				itemCurySettingsRows = new InventoryItemCurySettings[] { GetCurySettings(item.InventoryID) };
			}

			foreach (var itemClassCurySettings in itemClassCurySettingsRows)
			{
				var itemCurySettings = itemCurySettingsRows.FirstOrDefault(
					i => string.Equals(i.CuryID, itemClassCurySettings.CuryID, StringComparison.OrdinalIgnoreCase));

				if (itemCurySettings == null)
				{
					itemCurySettings = ItemCurySettings.Insert(
						new InventoryItemCurySettings() { InventoryID = item.InventoryID, CuryID = itemClassCurySettings.CuryID });
				}

				if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
				{
					itemCurySettings.DfltSiteID = itemClassCurySettings.DfltSiteID;

					try
					{
						ItemCurySettings.Update(itemCurySettings);
					}
					catch (PXSetPropertyException exception)
					{
						ItemCurySettings.Cache.RaiseExceptionHandling<InventoryItemCurySettings.dfltSiteID>
							(itemCurySettings, itemClassCurySettings.DfltSiteID, exception);
					}
				}
				else
				{
					// Inserting INItemSite record.
					ItemCurySettings.Cache.RaiseFieldUpdated<InventoryItemCurySettings.dfltSiteID>(itemCurySettings, null);
				}
			}

			if (allCurrencies)
			{
				var itemCurySettingsToDelete = itemCurySettingsRows.Where(i => !itemClassCurySettingsRows.Any(
					c => string.Equals(c.CuryID, i.CuryID, StringComparison.OrdinalIgnoreCase)));

				foreach (var itemCurySettings in itemCurySettingsToDelete)
				{
					itemCurySettings.DfltSiteID = null;
					ItemCurySettings.Update(itemCurySettings);
				}
			}
		}

		public virtual InventoryItemCurySettings GetCurySettings(int? inventoryID, string curyID = null)
		{
			if (curyID == null)
				curyID = Accessinfo.BaseCuryID;

			return ItemCurySettings.SelectSingle(inventoryID, curyID) ??
				ItemCurySettings.Insert(new InventoryItemCurySettings() { InventoryID = inventoryID, CuryID = curyID });
		}

		public static void CheckSameTermOnAllComponents(PXCache compCache, IEnumerable<INComponent> components)
		{
			var componentsWithOverrideDefaultTerm = components.Where(c => c.OverrideDefaultTerm == true);
			int count = componentsWithOverrideDefaultTerm.GroupBy(c => new { c.OverrideDefaultTerm, c.DefaultTerm, c.DefaultTermUOM }).Count();

			if (count > 1)
			{
				INComponent firstComponent = componentsWithOverrideDefaultTerm.First();
				compCache.RaiseExceptionHandling<INComponent.defaultTerm>(firstComponent, firstComponent.DefaultTerm,
								new PXSetPropertyException<INComponent.defaultTerm>(DR.Messages.DefaultTermMustBeTheSameForAllComponents));
					throw new PXRowPersistingException(typeof(INComponent.defaultTerm).Name, firstComponent.DefaultTerm, DR.Messages.DefaultTermMustBeTheSameForAllComponents);
			}
		}

		public static void VerifyComponentPercentages(PXCache itemCache, InventoryItem item, IEnumerable<INComponent> components)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
				return;

			var hasResiduals = components.Any(c => c.AmtOption == INAmountOption.Residual);

			if (item.TotalPercentage != 100 && hasResiduals == false)
			{
				if (itemCache.RaiseExceptionHandling<InventoryItem.totalPercentage>(item, item.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeHundred)))
					throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, item.TotalPercentage, Messages.SumOfAllComponentsMustBeHundred);
			}
			else if (item.TotalPercentage >= 100 && hasResiduals == true)
			{
				if (itemCache.RaiseExceptionHandling<InventoryItem.totalPercentage>(item, item.TotalPercentage, new PXSetPropertyException(Messages.SumOfAllComponentsMustBeLessHundredWithResiduals)))
					throw new PXRowPersistingException(typeof(InventoryItem.totalPercentage).Name, item.TotalPercentage, Messages.SumOfAllComponentsMustBeLessHundredWithResiduals);
			}
		}

		public static void VerifyOnlyOneResidualComponent(PXCache compCache, IEnumerable<INComponent> components)
		{
			bool manyResidual = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.aSC606>())
			{
				var residualComponents = components.Where(c => c.AmtOptionASC606 == INAmountOptionASC606.Residual);
				manyResidual = residualComponents.Count() > 1;

				if (manyResidual)
				{
					compCache.RaiseExceptionHandling<INComponent.amtOptionASC606>(residualComponents.First(), residualComponents.First().AmtOptionASC606,
						new PXSetPropertyException(Messages.OnlyOneResidualComponentAllowed));
					throw new PXRowPersistingException(typeof(INComponent.amtOptionASC606).Name, residualComponents.First().AmtOptionASC606, Messages.OnlyOneResidualComponentAllowed);
				}
			}
			else
			{
				var residualComponents = components.Where(c => c.AmtOption == INAmountOption.Residual);
				manyResidual = residualComponents.Count() > 1;

				if (manyResidual)
				{
					compCache.RaiseExceptionHandling<INComponent.amtOption>(residualComponents.First(), residualComponents.First().AmtOption,
						new PXSetPropertyException(Messages.OnlyOneResidualComponentAllowed));
					throw new PXRowPersistingException(typeof(INComponent.amtOption).Name, residualComponents.First().AmtOption, Messages.OnlyOneResidualComponentAllowed);
				}
			}

		}

		public static void SetDefaultTermControlsState(PXCache cache, InventoryItem item)
		{
			if (item == null)
				return;

			var code = (DRDeferredCode)PXSelectorAttribute.Select<InventoryItem.deferredCode>(cache, item);
			bool enableTerms = DeferredMethodType.RequiresTerms(code);
			PXUIFieldAttribute.SetEnabled<InventoryItem.defaultTerm>(cache, item, enableTerms);
			PXUIFieldAttribute.SetEnabled<InventoryItem.defaultTermUOM>(cache, item, enableTerms);
		}

		private IEnumerable GetEntityItems(String parent)
		{
			string screenID = IsStockItemFlag ? "IN202500" : "IN202000";

			PXSiteMapNode siteMap = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
			if (siteMap != null)
				foreach (var entry in EMailSourceHelper.TemplateEntity(this, parent, null, siteMap.GraphType, true))
					yield return entry;
		}

		private IEnumerable<INCategory> GetCategories(int? categoryID)
		{
			if (categoryID == null)
				yield return new INCategory
				{
					CategoryID = 0,
					Description = PXSiteMap.RootNode.Title
				};

			foreach (INCategory item in
				SelectFrom<INCategory>.
				Where<
					INCategory.parentID.IsEqual<@P.AsInt>.
					And<INCategory.description.IsNotEqual<Empty>>.
					And<INCategory.description.IsNotNull>>.
				OrderBy<INCategory.sortOrder.Asc>.
				View.Select(this, categoryID))
			{
				yield return item;
			}
		}
		
		public static class ActionCategories
		{
			public const string PricesCategoryID = "Prices Category";

			[PXLocalizable]
			public static class DisplayNames
			{
				public const string Prices = "Prices";
			}
		}

		public class DefaultKitRevisionID : PX.Objects.Common.PXFieldAttachedTo<InventoryItem>.By<InventoryItemMaintBase>.AsString.Named<DefaultKitRevisionID>
		{
			public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>();
			public override string GetValue(InventoryItem Row) => Row.With(GetLatestKitRevision).With(kit => kit.RevisionID);
			protected virtual INKitSpecHdr GetLatestKitRevision(InventoryItem item) =>
				SelectFrom<INKitSpecHdr>.
				Where<
					INKitSpecHdr.kitInventoryID.IsEqual<@P.AsInt>.
					And<INKitSpecHdr.isActive.IsEqual<True>>>.
				OrderBy<INKitSpecHdr.lastModifiedDateTime.Desc>.
				View.ReadOnly.Select(Base, item.InventoryID);
		}
	}
}
