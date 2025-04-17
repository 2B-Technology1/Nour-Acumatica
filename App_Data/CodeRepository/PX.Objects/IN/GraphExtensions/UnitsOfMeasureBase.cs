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
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class UnitsOfMeasureBase<TGraph, TUnitID, TUnitType, TParent, TParentID, TParentBaseUnit, TParentSalesUnit, TParentPurchaseUnit>
		: PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TUnitID : class, IBqlField
		where TUnitType : class, IConstant, new()
		where TParent : class, IBqlTable, new()
		where TParentID : class, IBqlField
		where TParentBaseUnit : class, IBqlField
		where TParentSalesUnit : class, IBqlField
		where TParentPurchaseUnit : class, IBqlField
	{
		#region Caches

		protected PXCache<TParent> ParentCache => Base.Caches<TParent>();
		protected PXCache<INUnit> UnitCache => Base.Caches<INUnit>();

		public TParent ParentCurrent => (TParent)ParentCache.Current;

		#endregion

		#region Configuration
		protected abstract IEnumerable<INUnit> SelectOwnConversions(string baseUnit);
		protected abstract IEnumerable<INUnit> SelectParentConversions(string baseUnit);
		protected abstract INUnit GetBaseUnit(int? parentID, string baseUnit);
		protected abstract INUnit CreateUnitCopy(int? parentID, INUnit unit);
		protected abstract void InitBaseUnit(int? parentID, string newValue);
		#endregion

		#region DAC Overrides

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		protected virtual void _(Events.CacheAttached<INUnit.unitType> eventArgs) { }

		#endregion

		#region Event Handlers

		#region INUnit

		protected virtual void _(Events.FieldDefaulting<INUnit, INUnit.toUnit> e)
		{
			if (ParentCurrent != null)
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Legacy]
				e.Row.SampleToUnit = (string)ParentCache.GetValue<TParentBaseUnit>(ParentCurrent);
				e.NewValue = e.Row.SampleToUnit;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<INUnit, INUnit.unitType> e)
		{
			if (ParentCurrent != null)
			{
				e.NewValue = GetUnitType();
			}
		}
		
		protected virtual void _(Events.FieldVerifying<INUnit, INUnit.unitRate> e)
		{
			if ((decimal?)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
			}
		}

		protected virtual void _(Events.FieldVerifying<INUnit, INUnit.fromUnit> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				if (e.Row == null || !e.ExternalCall)
					return;
				var newValue = (string)e.NewValue;
				if (!string.IsNullOrEmpty(newValue) && newValue == (string)ParentCache.GetValue<TParentBaseUnit>(ParentCurrent))
					throw new PXSetPropertyException(Messages.FromUnitCouldNotBeEqualBaseUnit);
			}
		}

		#endregion

		#region TParent

		protected virtual void _(Events.RowInserted<TParent> e)
		{
			if (string.IsNullOrEmpty((string)ParentCache.GetValue<TParentBaseUnit>(e.Row)) == false)
			{
				using (new ReadOnlyScope(UnitCache))
				{
					e.Cache.RaiseFieldUpdated<TParentBaseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<TParent> e)
		{
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				e.Cache.RaiseFieldUpdated<TParentBaseUnit>(e.Row, ParentCache.GetValue<TParentBaseUnit>(e.Row));
			}
		}

		protected virtual void _(Events.FieldVerifying<TParent, TParentSalesUnit> e) => e.Cancel = true;

		protected virtual void _(Events.FieldVerifying<TParent, TParentPurchaseUnit> e) => e.Cancel = true;

		protected virtual void _(Events.FieldVerifying<TParent, TParentBaseUnit> e) => e.Cancel = true;

		protected virtual void _(Events.FieldUpdated<TParent, TParentBaseUnit> e)
		{
			var parentID = (int?)ParentCache.GetValue<TParentID>(e.Row);
			var newValue = (string)ParentCache.GetValue<TParentBaseUnit>(e.Row);
			var oldValue = (string)e.OldValue;
			if (string.Equals(oldValue, newValue) == false)
			{
				if (string.IsNullOrEmpty(oldValue) == false)
				{
					INUnit baseunit = GetBaseUnit(parentID, oldValue);

					UnitCache.Delete(baseunit);

					foreach (INUnit oldunits in SelectOwnConversions(oldValue))
					{
						UnitCache.Delete(oldunits);
					}
				}

				if (string.IsNullOrEmpty(newValue) == false)
				{
					foreach (INUnit unit in SelectParentConversions(newValue))
					{
						INUnit classunit = CreateUnitCopy(parentID, unit);

						UnitCache.Insert(classunit);
					}
				}
			}

			if (string.IsNullOrEmpty(newValue) == false)
			{
				InitBaseUnit(parentID, newValue);

				e.Cache.RaiseFieldUpdated<TParentSalesUnit>(e.Row, (string)ParentCache.GetValue<TParentSalesUnit>(e.Row));
				e.Cache.RaiseFieldUpdated<TParentPurchaseUnit>(e.Row, (string)ParentCache.GetValue<TParentPurchaseUnit>(e.Row));
			}
		}

		protected virtual void _(Events.ExceptionHandling<TParent, TParentSalesUnit> e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.ExceptionHandling<TParent, TParentPurchaseUnit> e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				e.Cancel = true;
			}
		}

		#endregion

		#endregion

		#region Implementation

		private object GetUnitType() => new TUnitType().Value;

		protected INUnit ResolveConversion(short unitType, string fromUnit, string toUnit, int? inventoryID, int? itemClassID) =>
			new INUnit
			{
				UnitType = unitType,
				ItemClassID = itemClassID,
				InventoryID = inventoryID,
				FromUnit = fromUnit,
				ToUnit = toUnit,
				UnitRate = 1m,
				PriceAdjustmentMultiplier = 1m,
				UnitMultDiv = MultDiv.Multiply
			};
		#endregion
	}
}
