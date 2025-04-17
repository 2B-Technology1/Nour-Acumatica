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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	public abstract class MatrixImportBase<TGraph> : PXGraphExtension<TGraph>
		where TGraph : InventoryItemMaintBase, new()
	{
		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			if (!(e.Row is InventoryItem item) || item.IsTemplate == true) return;

			PXUIFieldAttribute.SetEnabled<InventoryItem.templateItemID>(e.Cache, item, Base.IsImport);

			if (item.TemplateItemID != null)
				PXUIFieldAttribute.SetEnabled<InventoryItem.kitItem>(e.Cache, item, true);
		}

		protected void _(Events.FieldSelecting<CSAnswers, CSAnswers.value> e, PXFieldSelecting baseMethod)
		{
			// only call base method when one exists
			baseMethod?.Invoke(e.Cache, e.Args);

			if (!(e.Row is CSAnswers row)) return;
			if (!(e.ReturnState is PXFieldState state)) return;
			if (!(Base.Caches[typeof(InventoryItem)].Current is InventoryItem item) || item.TemplateItemID == null || item.IsTemplate == true)
				return;

			string category = (string)(e.Cache.GetValueExt<CSAnswers.attributeCategory>(row) as PXFieldState)?.Value;
			if (category == CSAttributeGroup.attributeCategory.Variant && Base.IsImport)
			{
				var inactiveAttribute = SelectFrom<CSAnswers>.InnerJoin<InventoryItem>.On<CSAnswers.refNoteID.IsEqual<InventoryItem.noteID>>
					.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>
					.And<CSAnswers.isActive.IsEqual<False>
					.And<CSAnswers.attributeID.IsEqual<@P.AsString>>>>
					.View.Select(Base, item.TemplateItemID, row.AttributeID).RowCast<CSAnswers>().FirstOrDefault();

				state.Enabled = inactiveAttribute == null;
			}
		}

		protected virtual void _(Events.FieldVerifying<InventoryItem.templateItemID> e)
		{
			if (!(e.Row is InventoryItem currentItem)) return;
			if (currentItem.IsTemplate == true || currentItem.ItemClassID == null || e.NewValue == null || (int)e.NewValue == 0) return;

			InventoryItem templateItem = SelectFrom<InventoryItem>.
					Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(Base, e.NewValue).FirstOrDefault();

			if (templateItem == null) return;
			// there should not be any template item with a different item class to be selected here
			if (templateItem.ItemClassID == null || templateItem.ItemClassID != currentItem.ItemClassID) return;

			List<string> fieldsWithDifferentValues = new List<string>();

			if (!e.Cache.ObjectsEqual<InventoryItem.baseUnit>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.baseUnit>(e.Cache));

			if (!e.Cache.ObjectsEqual<InventoryItem.salesUnit>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.salesUnit>(e.Cache));

			if (!e.Cache.ObjectsEqual<InventoryItem.purchaseUnit>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.purchaseUnit>(e.Cache));

			if (!e.Cache.ObjectsEqual<InventoryItem.valMethod>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.valMethod>(e.Cache));

			if (!e.Cache.ObjectsEqual<InventoryItem.taxCategoryID>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.taxCategoryID>(e.Cache));

			if (!e.Cache.ObjectsEqual<InventoryItem.itemType>(currentItem, templateItem))
				fieldsWithDifferentValues.Add(PXUIFieldAttribute.GetDisplayName<InventoryItem.itemType>(e.Cache));

			if (fieldsWithDifferentValues.Count > 0)
			{
				if (Base.Item.Ask(BCObjectsMessages.AskAllowOverwritingFieldsHeader,
					PXMessages.LocalizeFormat(BCObjectsMessages.AskAllowOverwritingFieldsContent, currentItem.InventoryCD?.Trim(), templateItem.InventoryCD?.Trim(), string.Join(", ", fieldsWithDifferentValues)),
					MessageButtons.OKCancel) != WebDialogResult.OK)
				{
					e.Cancel = true;
					return;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem.templateItemID> e)
		{
			if (!(e.Row is InventoryItem currentItem)) return;
			if (currentItem.IsTemplate == true || e.NewValue == null || (int)e.NewValue == 0) return;

			InventoryItem templateItem = SelectFrom<InventoryItem>.
					Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(Base, e.NewValue).FirstOrDefault();

			currentItem.ItemType = templateItem.ItemType;
			currentItem.BaseUnit = templateItem.BaseUnit;
			currentItem.DecimalBaseUnit = templateItem.DecimalBaseUnit;
			currentItem.SalesUnit = templateItem.SalesUnit;
			currentItem.DecimalSalesUnit = templateItem.DecimalSalesUnit;
			currentItem.PurchaseUnit = templateItem.PurchaseUnit;
			currentItem.DecimalPurchaseUnit = templateItem.DecimalPurchaseUnit;
			currentItem.ValMethod = templateItem.ValMethod;
			currentItem.TaxCategoryID = templateItem.TaxCategoryID;
		}

		protected virtual void _(Events.RowPersisting<InventoryItem> e)
		{
			if (!(e.Row is InventoryItem item) || item.TemplateItemID == null || item.IsTemplate == true) return;
			bool isDeleted = e.Cache.GetStatus(e.Row) == PXEntryStatus.Deleted;
			if (isDeleted) return;

			var answerCaches = Base.Caches[typeof(CSAnswers)];

			// retrieve the list of inactive attributes of template item to exclude from validations
			var inactiveAttributes = SelectFrom<CSAnswers>.InnerJoin<InventoryItem>.On<CSAnswers.refNoteID.IsEqual<InventoryItem.noteID>>
				.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>
				.And<CSAnswers.isActive.IsEqual<False>>>
				.View.Select(Base, item.TemplateItemID).RowCast<CSAnswers>().ToList();

			foreach (CSAttributeGroup ag in SelectFrom<CSAttributeGroup>
				.InnerJoin<CSAttribute>.On<CSAttributeGroup.attributeID.IsEqual<CSAttribute.attributeID>>
				.Where<CSAttributeGroup.attributeCategory.IsEqual<@P.AsString>
				.And<CSAttributeGroup.entityClassID.IsEqual<@P.AsString>>>.View
				.Select(Base, CSAttributeGroup.attributeCategory.Variant, item.ItemClassID.ToString()))
			{
				CSAnswers an = Base.Answers.Select().RowCast<CSAnswers>().Where(res => (res.AttributeID == ag.AttributeID))?.FirstOrDefault();

				// skip validation if attribute is inactive in template item
				if (an?.AttributeID != null && inactiveAttributes.Any(x => x.AttributeID == an.AttributeID))
					continue;

				var attributeValues = SelectFrom<CSAttributeDetail>
						.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>
						.View.Select(Base, ag.AttributeID).RowCast<CSAttributeDetail>();

				// if the attribute was created with no values defined in it
				if (attributeValues.Count() == 0)
					throw new PXException(BCObjectsMessages.AttributeHasNoDefinedValues, ag?.AttributeID);

				// if the attribute value field is empty
				if (an != null && string.IsNullOrWhiteSpace(an?.Value))
				{
					answerCaches.RaiseExceptionHandling<CSAnswers.value>(an, an?.Value, new PXSetPropertyException(BCObjectsMessages.AttributeValueCannotBeEmpty, ag?.AttributeID));
					throw new PXException(BCObjectsMessages.AttributeValueCannotBeEmpty, ag?.AttributeID);
				}
			}

			foreach (InventoryItem it in PXSelectReadonly<InventoryItem,
				Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>,
				And<InventoryItem.isTemplate, Equal<False>,
				And<InventoryItem.templateItemID, Equal<Required<InventoryItem.templateItemID>>,
				And<InventoryItem.inventoryID, NotEqual<Required<InventoryItem.inventoryID>>,
				And<Match<Current<AccessInfo.userName>>>>>>>>
				.Select(Base, item.TemplateItemID, item.InventoryID))
			{
				bool sameAttrValues = true;
				foreach (CSAnswers csa in SelectFrom<CSAnswers>.InnerJoin<CSAttributeGroup>
					.On<CSAnswers.attributeID.IsEqual<CSAttributeGroup.attributeID>>
					.Where<CSAnswers.refNoteID.IsEqual<@P.AsGuid>
					.And<CSAttributeGroup.attributeCategory.IsEqual<CSAttributeGroup.attributeCategory.variant>>>.View.ReadOnly.Select(Base, it.NoteID))
				{
					if (csa.Value != Base.Answers.Select().RowCast<CSAnswers>().Where(res => (res.AttributeID == csa.AttributeID))?.FirstOrDefault()?.Value)
					{
						sameAttrValues = false;
						break;
					}
				}

				if (sameAttrValues == true)
					throw new PXException(BCObjectsMessages.InvItemHasSameAttrValues, item.InventoryCD.Trim(), it.InventoryCD.Trim());
			}
		}
	}

	public class InventoryItemMatrixImport : MatrixImportBase<InventoryItemMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }
	}

	public class NonStockItemMatrixImport : MatrixImportBase<NonStockItemMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }
	}
}
