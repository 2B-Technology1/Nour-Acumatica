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
	public class INUnitSelect<Table, inventoryID, itemClassID, salesUnit, purchaseUnit, baseUnit, lotSerClass> : PXSelect<INUnit, Where<INUnit.inventoryID, Equal<Current<inventoryID>>, And<INUnit.toUnit, Equal<Optional<baseUnit>>, And<INUnit.fromUnit, NotEqual<Optional<baseUnit>>>>>>
		where Table : INUnit
		where inventoryID : IBqlField
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
		public INUnitSelect(PXGraph graph)
			: base(graph)
		{
			TopCache = this.Cache.Graph.Caches[BqlCommand.GetItemType(typeof(inventoryID))];

			graph.FieldVerifying.AddHandler<salesUnit>(SalesUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<purchaseUnit>(PurchaseUnit_FieldVerifying);
			graph.FieldVerifying.AddHandler<baseUnit>(BaseUnit_FieldVerifying);

			graph.FieldUpdated.AddHandler<salesUnit>(SalesUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<purchaseUnit>(PurchaseUnit_FieldUpdated);
			graph.FieldUpdated.AddHandler<baseUnit>(BaseUnit_FieldUpdated);
			graph.FieldVerifying.AddHandler<lotSerClass>(LotSerClass_FieldVerifying);
			graph.RowInserted.AddHandler(TopCache.GetItemType(), Top_RowInserted);
			graph.RowPersisting.AddHandler(TopCache.GetItemType(), Top_RowPersisting);

			graph.FieldDefaulting.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.inventoryID>(INUnit_InventoryID_FieldVerifying);
			graph.RowPersisting.AddHandler<INUnit>(INUnit_RowPersisting);
			graph.RowPersisted.AddHandler<INUnit>(INUnit_RowPersisted);
			graph.FieldDefaulting.AddHandler<INUnit.toUnit>(INUnit_ToUnit_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldDefaulting);
			graph.FieldVerifying.AddHandler<INUnit.unitType>(INUnit_UnitType_FieldVerifying);
			graph.FieldVerifying.AddHandler<INUnit.unitRate>(INUnit_UnitRate_FieldVerifying);
			graph.RowSelected.AddHandler<INUnit>(INUnit_RowSelected);
			graph.RowInserting.AddHandler<INUnit>(INUnit_RowInserting);
			graph.RowInserted.AddHandler<INUnit>(INUnit_RowInserted);
			graph.RowDeleted.AddHandler<INUnit>(INUnit_RowDeleted);

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
			InsertConversion<salesUnit>(sender, e.Row, (string)e.OldValue);
		}

		protected virtual void PurchaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void PurchaseUnit_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			InsertConversion<purchaseUnit>(sender, e.Row, (string)e.OldValue);
		}

		protected virtual void BaseUnit_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		private void InsertConversion<TFromUnit>(PXCache cache, object row, string oldFromValue) where TFromUnit : IBqlField
		{
			var fromUnit = TopGetValue<TFromUnit, string>(row);
			if (!string.IsNullOrEmpty(fromUnit))
			{
				var inventoryID = TopGetValue<inventoryID, int?>(row);
				INUnit conv = INUnit.UK.ByInventory.FindDirty(cache.Graph, inventoryID, fromUnit);
				if (conv == null)
				{
					var toUnit = TopGetValue<baseUnit, string>(row);
					if ((conv = INUnit.UK.ByGlobal.FindDirty(cache.Graph, fromUnit, toUnit)) != null)
					{
						conv = PXCache<INUnit>.CreateCopy(conv);
						conv.UnitType = INUnitType.InventoryItem;
						conv.ItemClassID = 0;
						conv.InventoryID = inventoryID;
						conv.RecordID = null;
					}
					else
					{
						conv = ResolveInventoryConversion(inventoryID, fromUnit, toUnit);
					}
					this.Cache.Insert(conv);
				}
			}

			//try to delete conversions added when changing base unit copied from item class
			//if purchaseunit is not equal to oldvalue -> delete it
			if (string.IsNullOrEmpty(oldFromValue) == false
				&& string.Equals(oldFromValue, TopGetValue<purchaseUnit, string>(row)) == false
				&& string.Equals(oldFromValue, TopGetValue<salesUnit, string>(row)) == false
				&& string.Equals(oldFromValue, TopGetValue<baseUnit, string>(row)) == false)
			{
				INUnit oldConv = ResolveInventoryConversion(
					TopGetValue<inventoryID, int?>(row),
					oldFromValue,
					TopGetValue<baseUnit, string>(row));

				if (this.Cache.GetStatus(oldConv) == PXEntryStatus.Inserted)
				{
					this.Cache.Delete(oldConv);
				}
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
						this.Cache.RaiseExceptionHandling<INUnit.unitMultDiv>(unit, unit.UnitMultDiv, new PXSetPropertyException(Messages.FractionalUnitConversion));
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
					INUnit baseunit = ResolveInventoryConversion(
						TopGetValue<inventoryID, int?>(e.Row),
						(string)e.OldValue,
						(string)e.OldValue);

					this.Cache.Delete(baseunit);

					foreach (INUnit oldunits in this.Select((string)e.OldValue, (string)e.OldValue))
					{
						this.Cache.Delete(oldunits);
					}
				}

				if (string.IsNullOrEmpty(newValue) == false)
				{
					foreach (INUnit classunit in PXSelect<INUnit,
						Where<INUnit.unitType, Equal<INUnitType.itemClass>,
						And<INUnit.itemClassID, Equal<Current<itemClassID>>,
							And<INUnit.toUnit, Equal<Required<baseUnit>>,
							And<INUnit.fromUnit, NotEqual<Required<baseUnit>>>>>>>.Select(sender.Graph, newValue, newValue))
					{
						INUnit itemunit = PXCache<INUnit>.CreateCopy(classunit);
						itemunit.InventoryID = TopGetValue<inventoryID, Int32?>(e.Row);
						itemunit.ItemClassID = 0;
						itemunit.UnitType = INUnitType.InventoryItem;
						itemunit.RecordID = null;

						this.Cache.Insert(itemunit);
					}
				}
			}

			if (string.IsNullOrEmpty(newValue) == false)
			{
				INUnit baseunit = INUnit.UK.ByInventory.FindDirty(
					sender.Graph,
					TopGetValue<inventoryID, int?>(e.Row),
					newValue);

				if (baseunit == null)
				{
					baseunit = ResolveInventoryConversion(
						TopGetValue<inventoryID, int?>(e.Row),
						newValue,
						newValue);

					this.Cache.Insert(baseunit);
				}

				sender.RaiseFieldUpdated<salesUnit>(e.Row, TopGetValue<salesUnit, string>(e.Row));
				sender.RaiseFieldUpdated<purchaseUnit>(e.Row, TopGetValue<purchaseUnit, string>(e.Row));
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
				sender.RaiseFieldUpdated<baseUnit>(e.Row, TopGetValue<baseUnit, string>(e.Row));
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

		protected virtual void INUnit_InventoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = TopGetValue<inventoryID>();
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
				e.Cancel = true;
			}
		}

		protected virtual void INUnit_UnitType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (this.TopCache.Current != null)
			{
				e.NewValue = INUnitType.InventoryItem;
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

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && inUnit.InventoryID < 0 && TopCache.Current != null)
			{
				int? _KeyToAbort = TopGetValue<inventoryID, Int32?>();
				if (!_persisted.ContainsKey(_KeyToAbort))
				{
					_persisted.Add(_KeyToAbort, inUnit.InventoryID);
				}
				inUnit.InventoryID = _KeyToAbort;
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
				if (_persisted.TryGetValue(((INUnit)e.Row).InventoryID, out _KeyToAbort))
				{
					((INUnit)e.Row).InventoryID = _KeyToAbort;
				}
			}
		}

		protected virtual void INUnit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXFieldState state = (PXFieldState)sender.GetStateExt<INUnit.unitMultDiv>(e.Row);
			if (state.Error == null || state.Error == PXMessages.Localize(Messages.FractionalUnitConversion, out string _))
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
		}

		protected virtual void INUnit_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;
			if (unit != null && unit.ToUnit == null)
				e.Cancel = true;

			if (unit != null)
			{
				foreach (INUnit item in sender.Deleted)
				{
					if (sender.ObjectsEqual(item, unit))
					{
						//since this item (although previously was deleted) will eventually be updated restore tstamp and recordID fields:
						unit.RecordID = item.RecordID;
						unit.tstamp = item.tstamp;
						break;
					}
				}
			}
		}

		protected virtual void INUnit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.FromUnit != null && unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = INUnit.UK.ByGlobal.FindDirty(sender.Graph, unit.FromUnit, unit.FromUnit);
				if (global == null)
				{
					global = ResolveGlobalConversion(unit.FromUnit);

					sender.RaiseRowInserting(global);

					sender.SetStatus(global, PXEntryStatus.Inserted);
					sender.ClearQueryCacheObsolete();
				}
			}
		}

		protected virtual void INUnit_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			INUnit unit = (INUnit)e.Row;

			if (unit.UnitType == INUnitType.InventoryItem)
			{
				INUnit global = ResolveGlobalConversion(unit.FromUnit);

				if (sender.GetStatus(global) == PXEntryStatus.Inserted)
				{
					sender.SetStatus(global, PXEntryStatus.InsertedDeleted);
					sender.ClearQueryCacheObsolete();
				}
			}
		}

		private INLotSerClass ReadLotSerClass()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(typeof(lotSerClass))];
			return INLotSerClass.PK.FindDirty(_Graph, (string)cache.GetValue(cache.Current, typeof(lotSerClass).Name));
		}

		private INUnit ResolveInventoryConversion(int? inventoryID, string fromUnit, string baseUnit) => new INUnit
		{
			UnitType = INUnitType.InventoryItem,
			ItemClassID = 0,
			InventoryID = inventoryID,
			FromUnit = fromUnit,
			ToUnit = baseUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		private INUnit ResolveGlobalConversion(string fromUnit) => new INUnit
		{
			UnitType = INUnitType.Global,
			ItemClassID = 0,
			InventoryID = 0,
			FromUnit = fromUnit,
			ToUnit = fromUnit,
			UnitRate = 1m,
			PriceAdjustmentMultiplier = 1m,
			UnitMultDiv = MultDiv.Multiply
		};

		#endregion
	}
}
