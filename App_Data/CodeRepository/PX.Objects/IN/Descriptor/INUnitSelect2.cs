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

using System;
using System.Collections.Generic;

namespace PX.Objects.IN
{
	[Obsolete(Common.Messages.MethodIsObsoleteRemoveInLaterAcumaticaVersions)]
	public class INUnitSelect2<Table, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.itemClassID, Equal<Optional<itemClassID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where itemClassID : IBqlField
		where salesUnit : IBqlField
		where purchaseUnit : IBqlField
		where baseUnit : IBqlField
		where lotSerClass : IBqlField
	{
		#region State
		protected PXCache TopCache;
		#endregion

		#region Ctor
		public INUnitSelect2(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(itemClassID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);

			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.itemClassID>(INUnit_ItemClassID_FieldVerifying);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
			graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);
			graph.RowPersisted.AddHandler<INUnit>(INUnit_RowPersisted);

			if (this.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
			{
				graph.FieldVerifying.AddHandler<INUnit.fromUnit>(INUnit_FromUnit_FieldVerifying);
			}
			else
			{
				graph.ExceptionHandling.AddHandler<salesUnit>((sender, e) => { e.Cancel = true; });
				graph.ExceptionHandling.AddHandler<purchaseUnit>((sender, e) => { e.Cancel = true; });
			}
		}
		#endregion

		#region Implementation
		protected object TopGetValue<Field>(object data)
			where Field : IBqlField
		{
			if (BqlCommand.GetItemType(typeof(Field)) == TopCache.GetItemType() || TopCache.GetItemType().IsAssignableFrom(BqlCommand.GetItemType(typeof(Field))))
			{
				return this.TopCache.GetValue<Field>(data);
			}
			else
			{
				PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				return cache.GetValue<Field>(cache.Current);
			}
		}

		protected DataType TopGetValue<Field, DataType>(object data)
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>(data);
		}

		protected object TopGetValue<Field>()
			where Field : IBqlField
		{
			return TopGetValue<Field>(this.TopCache.Current);
		}

		protected DataType TopGetValue<Field, DataType>()
			where Field : IBqlField
		{
			return (DataType)TopGetValue<Field>();
		}

		protected virtual void SalesUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void SalesUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversionIfNotExists<salesUnit>(sender, e.Row);
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversionIfNotExists<purchaseUnit>(sender, e.Row);
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private void InsertConversionIfNotExists<TFromUnit>(PXCache sender, object row) where TFromUnit : IBqlField
		{
			string fromUnit = TopGetValue<TFromUnit, string>(row);
			if (string.IsNullOrEmpty(fromUnit))
				return;
			int? itemClassID = TopGetValue<itemClassID, int?>(row);
			INUnit conv = INUnit.UK.ByItemClass.FindDirty(sender.Graph, itemClassID, fromUnit);
			if (conv == null)
			{
				conv = ResolveItemClassConversion(itemClassID, TopGetValue<baseUnit, string>(row), fromUnit);

				this.Cache.Insert(conv);
			}
		}

		protected virtual void LotSerClass_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			INLotSerClass lotSerClass = INLotSerClass.PK.FindDirty(sender.Graph, (string)e.NewValue);
			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered)
			{
				foreach (INUnit unit in this.Select())
				{
					if (INUnitAttribute.IsFractional(unit))
					{
						this.Cache.MarkUpdated(unit, assertError: true);
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, ((INUnit)unit).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
					}
				}
			}
		}

		protected virtual void BaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var newValue = TopGetValue<baseUnit, string>(e.Row);
			if (string.Equals((string)e.OldValue, newValue) == false)
			{
				if (string.IsNullOrEmpty((string)e.OldValue) == false)
				{
					var itemClassId = TopGetValue<itemClassID, int?>(e.Row);
					INUnit baseunit = ResolveItemClassConversion(
						itemClassId,
						(string)e.OldValue,
						(string)e.OldValue);

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select(itemClassId, (string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				if (string.IsNullOrEmpty(newValue) == false)
				{
					foreach (INUnit globalunit in PXSelect<INUnit,
						Where<INUnit.unitType, Equal<INUnitType.global>,
							And<INUnit.toUnit, Equal<Required<baseUnit>>,
							And<INUnit.fromUnit, NotEqual<Required<baseUnit>>>>>>.Select(sender.Graph, newValue, newValue))
					{
						INUnit classunit = PXCache<INUnit>.CreateCopy(globalunit);
						classunit.ItemClassID = null;
						classunit.UnitType = null;
						classunit.RecordID = null;

						this.Cache.Insert(classunit);
					}
				}
			}

			if (string.IsNullOrEmpty(newValue) == false)
			{
				InsertConversionIfNotExists<baseUnit>(sender, e.Row);

				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit>(e.Row));
			}
		}

		protected virtual void Top_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (string.IsNullOrEmpty(TopGetValue<baseUnit, string>(e.Row)) == false)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(Cache))
				{
					sender.RaiseFieldUpdated<baseUnit>(e.Row, null);
				}
			}
		}

		protected virtual void Top_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit>(e.Row));
			}
		}

		protected virtual void INUnit_ToUnit_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				((INUnit)e.Row).SampleToUnit = TopGetValue<baseUnit, string>();
				e.NewValue = TopGetValue<baseUnit, string>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_ItemClassID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<itemClassID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.ItemClass;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitRate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			Decimal? conversion = (Decimal?)e.NewValue;
			if (conversion <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, "0");
			}
		}

		protected virtual void INUnit_FromUnit_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			var conversion = (INUnit)e.Row;
			if (conversion == null || !e.ExternalCall)
				return;
			if (!string.IsNullOrEmpty((string)e.NewValue) && (string)e.NewValue == TopGetValue<baseUnit, string>())
				throw new PXSetPropertyException(Messages.FromUnitCouldNotBeEqualBaseUnit);
		}

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INLotSerClass lotSerClass = ReadLotSerClass();

			if (lotSerClass != null && lotSerClass.LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, ((INUnit)e.Row).UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
			}
			else
			{
				sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, null, null);
			}
		}

		protected virtual void INUnit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var inUnit = (INUnit)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(INLotSerClass)];

				if (cache.Current != null && ((INLotSerClass)cache.Current).LotSerTrack == INLotSerTrack.SerialNumbered && INUnitAttribute.IsFractional((INUnit)e.Row))
				{
					sender.RaiseExceptionHandling<INUnit.unitMultDiv>(e.Row, inUnit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && inUnit.ItemClassID < 0 && TopCache.Current != null)
			{
				int? _KeyToAbort = TopGetValue<itemClassID, Int32?>();
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, inUnit.ItemClassID);
				}
				inUnit.ItemClassID = _KeyToAbort;
				sender.Normalize();
			}

			if ((e.Operation & PXDBOperation.Command).IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				if (inUnit.UnitType == INUnitType.InventoryItem && inUnit.InventoryID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.inventoryID>(sender));
				}

				if (inUnit.UnitType == INUnitType.ItemClass && inUnit.ItemClassID < 0)
				{
					throw new PXInvalidOperationException(CS.Messages.FieldShouldNotBeNegative, PXUIFieldAttribute.GetDisplayName<INUnit.itemClassID>(sender));
				}
			}
		}

		Dictionary<int?, int?> _persisted = new Dictionary<int?, int?>();

		protected virtual void INUnit_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				int? _KeyToAbort;
				if (_persisted.TryGetValue(((INUnit)e.Row).ItemClassID, out _KeyToAbort))
				{
					((INUnit)e.Row).ItemClassID = _KeyToAbort;
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return INLotSerClass.PK.FindDirty(_Graph, (string)cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}

		private INUnit ResolveItemClassConversion(int? itemClassID, string baseUnit, string fromUnit) => new INUnit
		{
			UnitType = INUnitType.ItemClass,
			ItemClassID = itemClassID,
			InventoryID = 0,
			FromUnit = fromUnit,
			ToUnit = baseUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion
	}
}
