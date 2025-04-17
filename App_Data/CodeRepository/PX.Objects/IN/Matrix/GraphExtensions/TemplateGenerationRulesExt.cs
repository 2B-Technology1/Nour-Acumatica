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
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Attributes;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC;
using PX.Objects.IN.Matrix.DAC.Projections;
using PX.Objects.IN.Matrix.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class TemplateGenerationRulesExt : GenerationRulesExt<
		TemplateInventoryItemMaint,
		InventoryItem,
		InventoryItem.inventoryID,
		INMatrixGenerationRule.parentType.templateItem,
		InventoryItem.sampleID,
		InventoryItem.sampleDescription>
	{
		#region Event handlers
		#region CSAnswers
		protected virtual void _(Events.RowUpdated<CSAnswers> e)
		{
			if (e.Row.IsActive != true && e.OldRow.IsActive == true)
			{
				foreach (IDGenerationRule rule in IDGenerationRules.Select())
				{
					if (rule.AttributeID == e.Row.AttributeID)
					{
						rule.AttributeID = null;
						IDGenerationRules.Update(rule);
					}
				}

				foreach (DescriptionGenerationRule rule in DescriptionGenerationRules.Select())
				{
					if (rule.AttributeID == e.Row.AttributeID)
					{
						rule.AttributeID = null;
						DescriptionGenerationRules.Update(rule);
					}
				}
			}
		}
		#endregion // CSAnswers
		#endregion // Event handlers


		#region Cache Attached

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[DefaultConditional(typeof(Search<INItemClass.defaultRowMatrixAttributeID,
			Where<INItemClass.itemClassID, Equal<Current<InventoryItem.parentItemClassID>>>>), typeof(InventoryItem.isTemplate), true)]
		[PXUIRequired(typeof(Where<InventoryItem.defaultRowMatrixAttributeID,
			NotEqual<MatrixAttributeSelectorAttribute.dummyAttributeName>>))]
		protected virtual void _(Events.CacheAttached<InventoryItem.defaultRowMatrixAttributeID> eventArgs)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[DefaultConditional(typeof(Search<INItemClass.defaultColumnMatrixAttributeID,
			Where<INItemClass.itemClassID, Equal<Current<InventoryItem.parentItemClassID>>>>), typeof(InventoryItem.isTemplate), true)]
		[PXUIRequired(typeof(Where<InventoryItem.defaultColumnMatrixAttributeID,
			NotEqual<MatrixAttributeSelectorAttribute.dummyAttributeName>>))]
		protected virtual void _(Events.CacheAttached<InventoryItem.defaultColumnMatrixAttributeID> eventArgs)
		{
		}

		#endregion // Cache Attached

		#region Overrides

		protected override string[] GetAttributeIDs()
			=> Base.Answers.SelectMain().Select(a => a.AttributeID).ToArray();

		protected override InventoryItem GetTemplate()
			=> Base.ItemSettings.Current;

		[PXOverride]
		public virtual void ResetDefaultsOnItemClassChange(InventoryItem row, Action<InventoryItem> baseMethod)
		{
			baseMethod?.Invoke(row);

			var cache = Base.Item.Cache;

			InsertDefaultAnswers();
			cache.SetDefaultExt<InventoryItem.defaultColumnMatrixAttributeID>(row);
			cache.SetDefaultExt<InventoryItem.defaultRowMatrixAttributeID>(row);

			var classIdRules =
				SelectFrom<IDGenerationRule>
				.Where<IDGenerationRule.parentType.IsEqual<INMatrixGenerationRule.parentType.itemClass>
					.And<IDGenerationRule.parentID.IsEqual<InventoryItem.itemClassID.FromCurrent>>>
				.OrderBy<Asc<IDGenerationRule.sortOrder>>
				.View
				.Select(Base)
				.RowCast<IDGenerationRule>();

			IDGenerationRules.SelectMain().ForEach(rule =>
				IDGenerationRules.Delete(rule));

			classIdRules.ForEach(classIdRule =>
			{
				var newRule = PropertyTransfer.Transfer(classIdRule, new IDGenerationRule());
				newRule.ParentID = row.InventoryID;
				newRule.ParentType = INMatrixGenerationRule.parentType.TemplateItem;
				newRule.LineNbr = null;

				IDGenerationRules.Insert(newRule);
			});

			var classDescriptionRules =
				SelectFrom<DescriptionGenerationRule>
				.Where<DescriptionGenerationRule.parentType.IsEqual<INMatrixGenerationRule.parentType.itemClass>
					.And<DescriptionGenerationRule.parentID.IsEqual<InventoryItem.itemClassID.FromCurrent>>>
				.OrderBy<Asc<DescriptionGenerationRule.sortOrder>>
				.View
				.Select(Base)
				.RowCast<DescriptionGenerationRule>();

			DescriptionGenerationRules.SelectMain().ForEach(rule =>
				DescriptionGenerationRules.Delete(rule));

			classDescriptionRules.ForEach(classIdRule =>
			{
				var newRule = PropertyTransfer.Transfer(classIdRule, new DescriptionGenerationRule());
				newRule.ParentID = row.InventoryID;
				newRule.ParentType = INMatrixGenerationRule.parentType.TemplateItem;
				newRule.LineNbr = null;

				DescriptionGenerationRules.Insert(newRule);
			});
		}

		protected virtual void InsertDefaultAnswers()
		{
			Base.Answers.SelectMain();
		}

		protected override int? GetAttributeLength(INMatrixGenerationRule row)
		{
			var helper = Base.GetCreateMatrixItemsHelper();

			return (row.SegmentType == INMatrixGenerationRule.segmentType.AttributeValue) ?
				helper.GetAttributeValue(row, Base.Item.Current, null).Length :
				helper.GetAttributeCaption(row, Base.Item.Current, null, null).Length;
		}

		#endregion // Overrides
	}
}
