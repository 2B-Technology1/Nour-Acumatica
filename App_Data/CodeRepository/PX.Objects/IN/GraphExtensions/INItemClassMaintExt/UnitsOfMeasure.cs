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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.IN.GraphExtensions.INItemClassMaintExt
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UnitsOfMeasure :
		INUnitLotSerClassBase<
			INItemClassMaint,
			INUnit.itemClassID,
			INUnitType.itemClass,
			INItemClass,
			INItemClass.itemClassID,
			INItemClass.baseUnit,
			INItemClass.salesUnit,
			INItemClass.purchaseUnit,
			INItemClass.lotSerClassID>
	{
		[PXDependToCache(typeof(INItemClass))]
		public
			SelectFrom<INUnit>
			.Where<INUnit.itemClassID.IsEqual<INItemClass.itemClassID.AsOptional>
				.And<INUnit.toUnit.IsEqual<INItemClass.baseUnit.AsOptional>>
				.And<INUnit.fromUnit.IsNotEqual<INItemClass.baseUnit.AsOptional>>>
			.View
			classunits;

		public override void Initialize()
		{
			base.Initialize();
			classunits.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>();
		}

		#region Configuration

		protected override IEnumerable<INUnit> SelectOwnConversions(string baseUnit) =>
			classunits.SelectMain(ParentCurrent.ItemClassID, baseUnit, baseUnit);

		protected override IEnumerable<INUnit> SelectParentConversions(string baseUnit) =>
			SelectFrom<INUnit>
			.Where<INUnit.unitType.IsEqual<INUnitType.global>
				.And<INUnit.toUnit.IsEqual<@P.AsString>>
				.And<INUnit.fromUnit.IsNotEqual<@P.AsString>>>
			.View
			.Select(Base, baseUnit, baseUnit)
			.RowCast<INUnit>();

		protected override INUnit GetBaseUnit(int? parentID, string baseUnit) =>
			ResolveConversion(INUnitType.ItemClass, baseUnit, baseUnit, 0, parentID);

		protected override INUnit CreateUnitCopy(int? parentID, INUnit unit)
		{
			INUnit classunit = PXCache<INUnit>.CreateCopy(unit);
			classunit.ItemClassID = null;
			classunit.UnitType = null;
			classunit.RecordID = null;
			return classunit;
		}

		protected override void InitBaseUnit(int? parentID, string newValue) => InsertConversion(parentID, newValue, newValue);

		private void InsertConversion(int? parentID, string fromUnit, string toUnit)
		{
			if (string.IsNullOrEmpty(fromUnit))
				return;

			if (INUnit.UK.ByItemClass.FindDirty(Base, parentID, fromUnit) == null)
			{
				var conv = ResolveConversion(INUnitType.ItemClass, fromUnit, toUnit, 0, parentID);

				UnitCache.Insert(conv);
			}
		}

		protected override void ValidateUnitConversions(INItemClass validatedItem)
		{
			if (validatedItem == null)
				return;

			using (PXDataRecord record = PXDatabase.SelectSingle<INUnit>(
				new PXDataField<INUnit.toUnit>(),
				new PXDataFieldValue<INUnit.unitType>(INUnitType.ItemClass),
				new PXDataFieldValue<INUnit.itemClassID>(validatedItem.ItemClassID),
				new PXDataFieldValue<INUnit.toUnit>(PXDbType.NVarChar, 6, validatedItem.BaseUnit, PXComp.NE)))
			{
				if (record != null)
					throw new PXException(Messages.WrongItemClassToUnitValue, record.GetString(0), validatedItem.ItemClassCD, validatedItem.BaseUnit);
			}
		}
		#endregion

		#region DAC Overrides

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXDBDefault(typeof(INItemClass.itemClassID))]
		protected virtual void _(Events.CacheAttached<INUnit.itemClassID> eventArgs) { }

		#endregion

		#region Event Handlers

		protected virtual void _(Events.FieldUpdated<INItemClass, INItemClass.salesUnit> e) =>
			InsertConversion(
				e.Row.ItemClassID,
				e.Row.SalesUnit,
				e.Row.BaseUnit);

		protected virtual void _(Events.FieldUpdated<INItemClass, INItemClass.purchaseUnit> e) =>
			InsertConversion(
				e.Row.ItemClassID,
				e.Row.PurchaseUnit,
				e.Row.BaseUnit);
		#endregion

	}
}
