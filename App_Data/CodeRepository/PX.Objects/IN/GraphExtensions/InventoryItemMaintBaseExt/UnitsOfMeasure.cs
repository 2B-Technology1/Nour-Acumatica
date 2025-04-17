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

using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.SO;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.GraphExtensions.InventoryItemMaintBaseExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UnitsOfMeasure :
		INUnitLotSerClassBase<
			InventoryItemMaintBase,
			INUnit.inventoryID,
			INUnitType.inventoryItem,
			InventoryItem,
			InventoryItem.inventoryID,
			InventoryItem.baseUnit,
			InventoryItem.salesUnit,
			InventoryItem.purchaseUnit,
			InventoryItem.lotSerClassID>
	{
		[PXDependToCache(typeof(InventoryItem))]
		public
			SelectFrom<INUnit>
			.Where<INUnit.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>
				.And<INUnit.toUnit.IsEqual<InventoryItem.baseUnit.AsOptional>>
				.And<INUnit.fromUnit.IsNotEqual<InventoryItem.baseUnit.AsOptional>>>
			.View
			itemunits;

		public override void Initialize()
		{
			base.Initialize();
			itemunits.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>();

			SOSetup soSetup = Base.sosetup.Current;

			PXUIFieldAttribute.SetVisible<INUnit.toUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<INUnit.toUnit>(itemunits.Cache, null, false);

			PXUIFieldAttribute.SetVisible<INUnit.sampleToUnit>(itemunits.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<INUnit.sampleToUnit>(itemunits.Cache, null, false);
			PXUIFieldAttribute.SetVisible<INUnit.priceAdjustmentMultiplier>(itemunits.Cache, null, soSetup?.UsePriceAdjustmentMultiplier == true);
		}

		#region Configuration

		protected override IEnumerable<INUnit> SelectOwnConversions(string baseUnit) =>
			itemunits.SelectMain(baseUnit, baseUnit);

		protected override IEnumerable<INUnit> SelectParentConversions(string baseUnit) =>
			SelectFrom<INUnit>
			.Where<INUnit.unitType.IsEqual<INUnitType.itemClass>
				.And<INUnit.itemClassID.IsEqual<InventoryItem.parentItemClassID.FromCurrent>>
				.And<INUnit.toUnit.IsEqual<InventoryItem.baseUnit.FromCurrent>>
				.And<INUnit.fromUnit.IsNotEqual<InventoryItem.baseUnit.FromCurrent>>>
			.View
			.Select(Base, baseUnit, baseUnit)
			.RowCast<INUnit>();

		protected override INUnit GetBaseUnit(int? parentID, string baseUnit) =>
			ResolveConversion(INUnitType.InventoryItem, baseUnit, baseUnit, parentID, 0);

		protected override INUnit CreateUnitCopy(int? parentID, INUnit unit)
		{
			INUnit itemunit = PXCache<INUnit>.CreateCopy(unit);
			itemunit.InventoryID = parentID;
			itemunit.ItemClassID = 0;
			itemunit.UnitType = INUnitType.InventoryItem;
			itemunit.RecordID = null;
			return itemunit;
		}

		protected override void InitBaseUnit(int? parentID, string newValue)
		{
			if (INUnit.UK.ByInventory.FindDirty(Base, parentID, newValue) == null)
			{
				var baseunit = ResolveConversion(INUnitType.InventoryItem, newValue, newValue, parentID, 0);

				UnitCache.Insert(baseunit);
			}
		}

		private void InsertConversion(int? parentID, string fromUnit, string toUnit, string oppositeTypeUnit, string oldFromValue)
		{
			if (string.IsNullOrEmpty(fromUnit))
				return;

			if (INUnit.UK.ByInventory.FindDirty(Base, parentID, fromUnit) == null)
			{
				var conv = INUnit.UK.ByGlobal.FindDirty(Base, fromUnit, toUnit);
				if (conv != null)
				{
					conv = PXCache<INUnit>.CreateCopy(conv);
					conv.UnitType = INUnitType.InventoryItem;
					conv.ItemClassID = 0;
					conv.InventoryID = parentID;
					conv.RecordID = null;
				}
				else
				{
					conv = ResolveConversion(INUnitType.InventoryItem, fromUnit, toUnit, parentID, 0);
				}
				UnitCache.Insert(conv);
			}

			//try to delete conversions added when changing base unit copied from item class
			//if purchaseunit is not equal to oldvalue -> delete it
			if (oldFromValue.IsNotIn(null, string.Empty, fromUnit, toUnit, oppositeTypeUnit))
			{
				INUnit oldConv = ResolveConversion(
					INUnitType.InventoryItem,
					oldFromValue,
					toUnit,
					parentID, 0);

				if (UnitCache.GetStatus(oldConv) == PXEntryStatus.Inserted)
				{
					UnitCache.Delete(oldConv);
				}
			}
		}

		protected override void ValidateUnitConversions(InventoryItem validatedItem)
		{
			if (validatedItem == null)
				return;

			using (PXDataRecord record = PXDatabase.SelectSingle<INUnit>(new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.InventoryItem),
				new PXDataFieldValue<INUnit.inventoryID>(validatedItem.InventoryID),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, 6, validatedItem.BaseUnit, PXComp.NE)))
			{
				if (record != null)
					throw new PXException(Messages.WrongInventoryItemToUnitValue, record.GetString(0), validatedItem.InventoryCD, validatedItem.BaseUnit);
			}

			IEnumerable<PXDataRecord> baseConversions = PXDatabase.SelectMulti<INUnit>(new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.InventoryItem),
				new PXDataFieldValue<INUnit.inventoryID>(validatedItem.InventoryID),
				new PXDataFieldValue<INUnit.fromUnit>(PXDbType.NVarChar, validatedItem.BaseUnit),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, validatedItem.BaseUnit));

			if (baseConversions.Count() != 1)
				throw new PXException(Messages.BaseConversionNotFound, validatedItem.BaseUnit, validatedItem.BaseUnit, validatedItem.InventoryCD);
		}
		#endregion

		#region DAC Overrides

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(InventoryItem.inventoryID))]
		protected virtual void _(Events.CacheAttached<INUnit.inventoryID> eventArgs) { }

		#endregion

		#region Event Handlers

		#region INUnit

		protected override void _(Events.RowSelected<INUnit> e)
		{
			PXFieldState state = (PXFieldState)e.Cache.GetStateExt<INUnit.unitMultDiv>(e.Row);
			if (state.Error == null || state.Error == PXMessages.Localize(Messages.FractionalUnitConversion, out string _))
			{
				base._(e);
			}
		}

		protected virtual void _(Events.RowInserting<INUnit> e)
		{
			if (e.Row != null && e.Row.ToUnit == null)
				e.Cancel = true;

			if (e.Row != null)
			{
				foreach (INUnit item in e.Cache.Deleted)
				{
					if (e.Cache.ObjectsEqual(item, e.Row))
					{
						//since this item (although previously was deleted) will eventually be updated restore tstamp and recordID fields:
						e.Row.RecordID = item.RecordID;
						e.Row.tstamp = item.tstamp;
						break;
					}
				}
			}
		}

		protected virtual void _(Events.RowInserted<INUnit> e)
		{
			if (e.Row.FromUnit != null && e.Row.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = INUnit.UK.ByGlobal.FindDirty(Base, e.Row.FromUnit, e.Row.FromUnit);
				if (global == null)
				{
					global = ResolveConversion(INUnitType.Global, e.Row.FromUnit, e.Row.FromUnit, 0, 0);

					e.Cache.RaiseRowInserting(global);

					e.Cache.SetStatus(global, PXEntryStatus.Inserted);
					e.Cache.ClearQueryCacheObsolete();
				}
			}
		}

		protected virtual void _(Events.RowDeleted<INUnit> e)
		{
			if (e.Row.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = ResolveConversion(INUnitType.Global, e.Row.FromUnit, e.Row.FromUnit, 0, 0);

				if (e.Cache.GetStatus(global) == PXEntryStatus.Inserted)
				{
					e.Cache.SetStatus(global, PXEntryStatus.InsertedDeleted);
					e.Cache.ClearQueryCacheObsolete();
				}
			}
		}

		#region InventoryItem
		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.salesUnit> e) =>
			InsertConversion(
				e.Row.InventoryID,
				e.Row.SalesUnit,
				e.Row.BaseUnit,
				e.Row.PurchaseUnit,
				(string)e.OldValue);

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.purchaseUnit> e) =>
			InsertConversion(
				e.Row.InventoryID,
				e.Row.PurchaseUnit,
				e.Row.BaseUnit,
				e.Row.SalesUnit,
				(string)e.OldValue);

		#endregion

		#endregion

		#region InventoryItem

		protected virtual void _(Events.RowDeleted<InventoryItem> e)
		{
			if (e.Row == null) return;

			// deleting only inventory-specific uoms
			foreach (INUnit unit in
				SelectFrom<INUnit>.
				Where<
					INUnit.unitType.IsEqual<INUnitType.inventoryItem>.
					And<INUnit.inventoryID.IsEqual<@P.AsInt>>>.
				View.Select(Base, e.Row.InventoryID))
			{
				itemunits.Delete(unit);
			}
		}

		#endregion

		#endregion
	}
}
