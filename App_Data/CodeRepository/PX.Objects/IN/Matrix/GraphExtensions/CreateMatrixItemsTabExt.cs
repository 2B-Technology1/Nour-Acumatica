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
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Unbound;
using PX.Objects.IN.Matrix.Graphs;
using PX.Objects.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.IN.Matrix.Utility;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.Attributes;
using PX.Common;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class CreateMatrixItemsTabExt : CreateMatrixItemsExt<TemplateInventoryItemMaint, InventoryItem>
	{
		public override void Initialize()
		{
			Base.Views.Caches.Remove(typeof(AdditionalAttributes));
			Base.Views.Caches.Remove(typeof(EntryMatrix));
			Base.Views.Caches.Remove(typeof(MatrixInventoryItem));

			base.Initialize();
		}

		public PXAction<InventoryItem> createUpdate;
		[PXUIField(DisplayName = "Confirmation", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public override IEnumerable CreateUpdate(PXAdapter adapter)
		{
			Base.Save.Press();
			return base.CreateUpdate(adapter);
		}

		protected override int? GetTemplateID()
		{
			return Base.Item.Current?.InventoryID;
		}

		protected override InventoryItem GetTemplateItem()
		{
			return Base.Item.Current;
		}

		protected override void GetGenerationRules(CreateMatrixItemsHelper helper, out List<INMatrixGenerationRule> idGenerationRules, out List<INMatrixGenerationRule> descrGenerationRules)
		{
			var extension = Base.GetExtension<TemplateGenerationRulesExt>();

			idGenerationRules = extension.IDGenerationRules.SelectMain().Select(s => (INMatrixGenerationRule)s).ToList();
			descrGenerationRules = extension.DescriptionGenerationRules.SelectMain().Select(s => (INMatrixGenerationRule)s).ToList();
		}

		protected override CSAttribute[] GetAdditionalAttributes()
		{
			var item = GetTemplateItem();

			var attributes = new PXSelectReadonly2<CSAttribute,
				InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<CSAttribute.attributeID, NotEqual<Current2<EntryHeader.colAttributeID>>,
					And<CSAttribute.attributeID, NotEqual<Current2<EntryHeader.rowAttributeID>>>>>>>>>(Base)
				.SelectMain(item?.ParentItemClassID, item?.NoteID);

			var answers = Base.Answers.SelectMain().ToDictionary(a => a.AttributeID);

			return attributes.Where(a => !answers.TryGetValue(a.AttributeID, out CSAnswers answer) || answer.IsActive != false).ToArray();
		}

		protected override void SetFilter(int? templateID, string columnAttributeID, string rowAttributeID, string[] additionalAttributes)
		{
			Base.Item.Current = Base.Item.Search<InventoryItem.inventoryID>(templateID);

			base.SetFilter(templateID, columnAttributeID, rowAttributeID, additionalAttributes);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.itemClassID> eventArgs)
		{
			RecalcAttributesGrid();
		}

		protected virtual void _(Events.RowPersisted<InventoryItem> eventArgs)
		{
			if (eventArgs.Row?.IsTemplate == true && eventArgs.TranStatus == PXTranStatus.Completed)
			{
				Header.Current.TemplateItemID = eventArgs.Row.InventoryID;
			}
		}

		protected virtual void _(Events.RowSelected<EntryHeader> eventArgs)
		{
			PXUIFieldAttribute.SetEnabled<EntryHeader.rowAttributeID>(eventArgs.Cache, eventArgs.Row, Base.Item.Current?.DefaultRowMatrixAttributeID != null);
			PXUIFieldAttribute.SetEnabled<EntryHeader.colAttributeID>(eventArgs.Cache, eventArgs.Row, Base.Item.Current?.DefaultColumnMatrixAttributeID != null);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultColumnMatrixAttributeID> eventArgs)
		{
			Header.SetValueExt<EntryHeader.colAttributeID>(Header.Current, eventArgs.NewValue);
		}

		protected virtual void _(Events.FieldUpdated<InventoryItem, InventoryItem.defaultRowMatrixAttributeID> eventArgs)
		{
			Header.SetValueExt<EntryHeader.rowAttributeID>(Header.Current, eventArgs.NewValue);
		}

		protected virtual void _(Events.RowSelected<InventoryItem> eventArgs)
		{
			if (eventArgs.Row?.IsTemplate == true && Header.Current != null &&
				Header.Current.TemplateItemID != eventArgs.Row.InventoryID)
			{
				Header.Current.TemplateItemID = eventArgs.Row?.InventoryID;
				RecalcAttributesGrid();
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXDefault(typeof(InventoryItem.defaultColumnMatrixAttributeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[MatrixAttributeSelector(typeof(Search2<CSAnswers.attributeID,
			InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID.IsEqual<CSAnswers.attributeID>>>,
			Where<CSAnswers.refNoteID, Equal<Current<InventoryItem.noteID>>,
				And<CSAnswers.isActive, Equal<True>,
				And<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.parentItemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>>>),
			typeof(EntryHeader.rowAttributeID), true, typeof(CSAttributeGroup.attributeID),
			DescriptionField = typeof(CSAttributeGroup.description), DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<EntryHeader.colAttributeID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXDefault(typeof(InventoryItem.defaultRowMatrixAttributeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[MatrixAttributeSelector(typeof(Search2<CSAnswers.attributeID,
			InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID.IsEqual<CSAnswers.attributeID>>>,
			Where<CSAnswers.refNoteID, Equal<Current<InventoryItem.noteID>>,
				And<CSAnswers.isActive, Equal<True>,
				And<CSAttributeGroup.isActive, Equal<True>,
				And<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.parentItemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>>>>>>>>),
			typeof(EntryHeader.colAttributeID), true, typeof(CSAttributeGroup.attributeID),
			DescriptionField = typeof(CSAttributeGroup.description), DirtyRead = true)]
		protected virtual void _(Events.CacheAttached<EntryHeader.rowAttributeID> eventArgs)
		{
		}
	}
}
