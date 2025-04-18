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
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC.Unbound;
using static PX.Objects.IN.Matrix.DAC.INMatrixGenerationRule.parentType;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class ItemsGridExt : PXGraphExtension<Graphs.TemplateInventoryItemMaint>
	{
		[PXVirtual]
		[PXCacheName(Messages.InventoryItemWithAttributeValuesDAC)]
		[PXBreakInheritance]
		public class MatrixInventoryItem : Matrix.DAC.Unbound.MatrixInventoryItem
		{
			#region InventoryCD
			public new abstract class inventoryCD : Data.BQL.BqlString.Field<inventoryCD> { }
			[PXDefault]
			[InventoryRaw(DisplayName = "Inventory ID", IsKey = true)]
			public override string InventoryCD
			{
				get => base.InventoryCD;
				set => base.InventoryCD = value;
			}
			#endregion
			#region InventoryID
			public new abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
			[AnyInventory(Visible = true, DisplayName = "Inventory ID")]
			public override int? InventoryID
			{
				get => base.InventoryID;
				set => base.InventoryID = value;
			}
			#endregion
			#region Descr
			public abstract new class descr : PX.Data.BQL.BqlString.Field<descr> { }
			/// <summary>
			/// The description of the Inventory Item.
			/// </summary>
			[DBMatrixLocalizableDescription(Common.Constants.TranDescLength, IsUnicode = true, CopyTranslationsToInventoryItem = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			[PX.Data.EP.PXFieldDescription]
			public override String Descr
			{
				get => base.Descr;
				set => base.Descr = value;
			}
			#endregion
			#region LastCost
			/// <summary>
			/// The cost assigned to the item before the current cost was set.
			/// </summary>
			[PXPriceCost]
			[PXUIField(DisplayName = "Last Cost", Enabled = false)]
			public virtual Decimal? LastCost { get; set; }
			public abstract class lastCost : BqlDecimal.Field<lastCost> { }
			#endregion
		}

		public PXFilter<TemplateAttributes> ItemAttributes;

		[PXVirtualDAC]
		public PXSelect<MatrixInventoryItem> MatrixItems;

		public PXAction<InventoryItem> viewMatrixItem;
		[PXUIField(DisplayName = "View Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewMatrixItem(PXAdapter adapter)
		{
			if (MatrixItems.Current != null)
			{
				InventoryItem item = InventoryItem.PK.Find(Base, MatrixItems.Current.InventoryID);
				if (item != null)
				{
					PXRedirectHelper.TryRedirect(Base.Caches[typeof(InventoryItem)], item, "View Item", PXRedirectHelper.WindowMode.NewWindow);
				}
			}
			return adapter.Get();
		}

		public override void Initialize()
		{
			base.Initialize();

			var enabledFields = new HashSet<string>(GetEditableChildFields(), StringComparer.OrdinalIgnoreCase);
			enabledFields.AddRange(GetEditableCurrencyChildFields());
			enabledFields.Add(nameof(InventoryItem.Selected));

			MatrixItems.Cache.GetAttributesOfType<PXUIFieldAttribute>(null, null).
				ForEach(a => a.Enabled = enabledFields.Contains(a.FieldName));

			for (int attributeIndex = 0; attributeIndex < ItemAttributes.Current.AttributeIdentifiers?.Length; attributeIndex++)
				AddAttributeColumn(attributeIndex);

			Base.Views.Caches.Remove(typeof(TemplateAttributes));
			Base.Views.Caches.Remove(typeof(MatrixInventoryItem));
		}

		protected virtual void _(Events.RowSelected<InventoryItem> eventArgs)
		{
			if (ItemAttributes.Current.TemplateItemID != eventArgs.Row?.InventoryID)
			{
				ItemAttributes.Current.TemplateItemID = eventArgs.Row?.InventoryID;
				RecalcAttributeColumns();
			}
		}

		protected virtual void _(Events.RowUpdated<CSAnswers> e)
		{
			if (e.Row.IsActive != e.OldRow.IsActive)
				RecalcAttributeColumns();
		}

		protected virtual void RecalcAttributeColumns()
		{
			var attributes = new PXSelectReadonly2<CSAttribute,
				InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>(Base)
				.SelectMain(Base.ItemSettings.Current?.ParentItemClassID);

			var answers = Base.Answers.SelectMain().ToDictionary(a => a.AttributeID);

			attributes = attributes.Where(a => !answers.TryGetValue(a.AttributeID, out CSAnswers answer) || answer.IsActive != false).ToArray();

			ItemAttributes.Current.AttributeIdentifiers = new string[attributes.Length];

			for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
			{
				ItemAttributes.Current.AttributeIdentifiers[attributeIndex] = attributes[attributeIndex].AttributeID;
				AddAttributeColumn(attributeIndex);
			}

			MatrixItems.View.RequestRefresh();
		}

		protected virtual void AddAttributeColumn(int attributeNumber)
		{
			string fieldName = $"AttributeValue{attributeNumber}";

			if (!MatrixItems.Cache.Fields.Contains(fieldName))
			{
				MatrixItems.Cache.Fields.Add(fieldName);
				
				Base.FieldSelecting.AddHandler(
					MatrixItems.Cache.GetItemType(),
					fieldName,
					(s, e) => AttributeValueFieldSelecting(attributeNumber, e, fieldName));
			}
		}

		protected virtual void AttributeValueFieldSelecting(int attributeNumber, PXFieldSelectingEventArgs e, string fieldName)
		{
			var row = e.Row as MatrixInventoryItem;
			PXStringState state = (PXStringState)PXStringState.CreateInstance(e.ReturnState, null, true, fieldName, false, null, null, null, null, null, null);
			state.Enabled = false;
			e.ReturnState = state;

			if (attributeNumber < ItemAttributes.Current.AttributeIdentifiers?.Length)
			{
				e.ReturnValue = 0 <= attributeNumber && attributeNumber < row?.AttributeValues?.Length ? row.AttributeValues[attributeNumber] : null;
				state.Visible = true;
				state.Visibility = PXUIVisibility.Visible;

				if (CRAttribute.Attributes.TryGetValue(ItemAttributes.Current.AttributeIdentifiers[attributeNumber],
					out CRAttribute.Attribute attribute))
				{
					state.DisplayName = attribute.Description;
					state.AllowedValues = attribute.Values.Select(v => v.ValueID).ToArray();
					state.AllowedLabels = attribute.Values.Select(v => v.Description).ToArray();
				}
			}
			else
			{
				state.Value = null;
				e.ReturnValue = null;
				state.DisplayName = null;
				state.Visible = false;
				state.Visibility = PXUIVisibility.Invisible;
			}
		}

		public virtual IEnumerable matrixItems()
		{
			var result = new List<MatrixInventoryItem>();

			PXResultset<CSAnswers> itemsWithAttributes = new PXSelectReadonly2<CSAnswers,
					InnerJoin<InventoryItem, On<CSAnswers.refNoteID, Equal<InventoryItem.noteID>>,
					InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAnswers.attributeID>>,
					LeftJoin<CSAttributeDetail, On<CSAttributeDetail.attributeID, Equal<CSAttributeGroup.attributeID>,
						And<CSAttributeDetail.valueID, Equal<CSAnswers.value>>>,
					LeftJoin<InventoryItemCurySettings, On<InventoryItemCurySettings.curyID, Equal<Current<AccessInfo.baseCuryID>>,
						And<InventoryItemCurySettings.FK.Inventory>>,
					LeftJoin<INItemCost, On<INItemCost.curyID, Equal<Current<AccessInfo.baseCuryID>>,
						And<INItemCost.FK.InventoryItem>>>>>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<InventoryItem.templateItemID, Equal<Current<InventoryItem.inventoryID>>,
					And<CSAnswers.attributeID, In<Required<CSAnswers.attributeID>>>>>>>>,
				OrderBy<Asc<InventoryItem.inventoryCD, Asc<CSAnswers.attributeID>>>>(Base)
				.Select(Base.ItemSettings.Current?.ItemClassID, ItemAttributes.Current.AttributeIdentifiers);

			string lastInventoryCD = null;
			MatrixInventoryItem item = null;
			List<string> attributes = ItemAttributes.Current.AttributeIdentifiers?.ToList();

			foreach (PXResult<CSAnswers, InventoryItem, CSAttributeGroup, CSAttributeDetail, InventoryItemCurySettings, INItemCost> itemWithAttribute in itemsWithAttributes)
			{
				CSAnswers answer = itemWithAttribute;
				InventoryItem inventoryItem = itemWithAttribute;
				CSAttributeDetail detail = itemWithAttribute;
				InventoryItemCurySettings curySettings = itemWithAttribute;
				INItemCost inItemCost = itemWithAttribute;

				if (lastInventoryCD != inventoryItem.InventoryCD)
				{
					if (item != null)
						result.Add(item);

					lastInventoryCD = inventoryItem.InventoryCD;
					item = PropertyTransfer.Transfer(inventoryItem, new MatrixInventoryItem());
					item.AttributeValues = new string[ItemAttributes.Current.AttributeIdentifiers.Length];
					item.AttributeIDs = attributes.ToArray();
					item.BasePrice = curySettings?.BasePrice ?? 0m;
					item.DfltSiteID = curySettings?.DfltSiteID;
					item.LastCost = inItemCost?.LastCost ?? 0m;
				}

				int attributeIndex = attributes?.IndexOf(answer.AttributeID) ?? -1;
				if (attributeIndex >= 0)
					item.AttributeValues[attributeIndex] = answer.Value;
			}

			if (item != null)
				result.Add(item);

			foreach (var row in result)
			{
				var rowFromCache = (MatrixInventoryItem)MatrixItems.Cache.Locate(row);
				if (rowFromCache == null ||
					MatrixItems.Cache.GetStatus(rowFromCache).IsNotIn(PXEntryStatus.Updated, PXEntryStatus.Inserted))
				{
					MatrixItems.Cache.MarkUpdated(row, assertError: true);
				}
				UpdateNonEditableFields(rowFromCache, row);
			}

			MatrixItems.View.RequestRefresh();

			return MatrixItems.Cache.Cached;
		}

		private void UpdateNonEditableFields(MatrixInventoryItem rowFromCache, MatrixInventoryItem rowFromDb)
		{
			if (rowFromCache == null || rowFromDb == null)
				return;

			var fieldsToUpdate = MatrixItems.Cache.BqlFields.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase).
				Except(
					GetEditableChildFields().
					Concat(GetEditableCurrencyChildFields()).
					Concat(new[] { nameof(rowFromCache.Selected) }), StringComparer.OrdinalIgnoreCase);

			foreach (var fieldName in fieldsToUpdate)
			{
				MatrixItems.Cache.SetValue(rowFromCache, fieldName, MatrixItems.Cache.GetValue(rowFromDb, fieldName));
			}
		}

		public PXAction<InventoryItem> deleteItems;
		[PXUIField(DisplayName = "Delete", MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete, Enabled = true)]
		[PXProcessButton]
		public virtual IEnumerable DeleteItems(PXAdapter adapter)
		{
			Base.Save.Press();

			var selectedItems = MatrixItems.SelectMain().Where(i => i.Selected == true).ToArray();

			PXLongOperation.StartOperation(Base, delegate ()
			{
				InventoryItemMaint stockGraph = null;
				NonStockItemMaint nonStockGraph = null;
				InventoryItemMaintBase graph = null;

				foreach (var item in selectedItems)
				{
					if (item.StkItem == true)
					{
						if (stockGraph == null)
							stockGraph = PXGraph.CreateInstance<InventoryItemMaint>();
						graph = stockGraph;
					}
					else
					{
						if (nonStockGraph == null)
							nonStockGraph = PXGraph.CreateInstance<NonStockItemMaint>();
						graph = nonStockGraph;
					}

					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					graph.Delete.Press();
				}
			});


			return adapter.Get();
		}

		protected virtual IEnumerable<string> GetEditableChildFields()
		{
			return new string[]
			{
				nameof(InventoryItem.descr),
			};
		}

		protected virtual IEnumerable<string> GetEditableCurrencyChildFields()
		{
			return new string[]
			{
				nameof(InventoryItemCurySettings.recPrice),
				nameof(InventoryItemCurySettings.basePrice)
			};
		}

		protected virtual void ChildItemUpdated(Events.RowUpdated<MatrixInventoryItem> eventArgs)
		{
			if (eventArgs.Row == null || eventArgs.OldRow == null)
				return;

			UpdateInventoryItemByChildItem(eventArgs.Cache, eventArgs.OldRow, eventArgs.Row);
			UpdateInventoryItemCurySettingsByChildItem(eventArgs.Cache, eventArgs.OldRow, eventArgs.Row);
		}

		protected virtual void UpdateInventoryItemByChildItem(PXCache cache, MatrixInventoryItem oldRow, MatrixInventoryItem row)
		{
			bool updated = false;
			InventoryItem item = null;

			foreach (var field in GetEditableChildFields())
			{
				var oldValue = cache.GetValue(oldRow, field);
				var value = cache.GetValue(row, field);

				if (!object.Equals(oldValue, value))
				{
					if (item == null)
					{
						item = InventoryItem.PK.FindDirty(Base, row.InventoryID);

						if (item == null || item.TemplateItemID != Base.Item.Current?.InventoryID)
							throw new Common.Exceptions.RowNotFoundException(Base.Item.Cache, row.InventoryID);
					}

					Base.Item.Cache.SetValue(item, field, value);
					updated = true;
				}
			}

			if (updated)
			{
				PXDBLocalizableStringAttribute.CopyTranslations
					<MatrixInventoryItem.descr, InventoryItem.descr>(Base, row, item);

				Base.UpdateChild(item);
			}
		}

		protected virtual void UpdateInventoryItemCurySettingsByChildItem(PXCache cache, MatrixInventoryItem oldRow, MatrixInventoryItem row)
		{
			bool updated = false;
			InventoryItemCurySettings curySettings = Base.GetCurySettings(row.InventoryID);

			foreach (var field in GetEditableCurrencyChildFields())
			{
				var oldValue = cache.GetValue(oldRow, field);
				var value = cache.GetValue(row, field);

				if (!object.Equals(oldValue, value))
				{
					Base.ItemCurySettings.Cache.SetValue(curySettings, field, value);
					updated = true;
				}
			}

			if (updated)
				Base.UpdateChildCurySettings(curySettings);
		}
	}
}
