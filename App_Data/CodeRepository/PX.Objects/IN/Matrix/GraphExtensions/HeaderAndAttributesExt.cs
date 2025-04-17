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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.Common;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.Attributes;
using PX.Objects.IN.Matrix.DAC.Unbound;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public abstract class HeaderAndAttributesExt<Graph, MainItemType> : PXGraphExtension<Graph>
			where Graph : PXGraph
			where MainItemType : class, IBqlTable, new()
	{
		public PXFilter<EntryHeader> Header;
		public PXFilter<AdditionalAttributes> AdditionalAttributes;

		public virtual bool ShowDisabledValue => true;

		public override void Initialize()
		{
			base.Initialize();
			AddNeededFields();

			Base.Views.Caches.Remove(typeof(EntryHeader));
			Base.Views.Caches.Remove(typeof(AdditionalAttributes));
		}

		protected virtual void AddNeededFields()
		{
			for (int attributeIndex = 0; attributeIndex < AdditionalAttributes.Current.Values?.Length; attributeIndex++)
				AddFieldToAttributeGrid(AdditionalAttributes.Cache, attributeIndex);
		}

		protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.templateItemID> eventArgs)
		{
			RecalcAttributesGrid();
		}

		protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.colAttributeID> eventArgs)
			=> RecalcAttributesGrid();

		protected virtual void _(Events.FieldUpdated<EntryHeader, EntryHeader.rowAttributeID> eventArgs)
			=> RecalcAttributesGrid();

		protected virtual void _(Events.RowUpdated<CSAnswers> e)
		{
			if (e.Row.IsActive != true && e.OldRow.IsActive == true)
			{
				if (Header.Current.ColAttributeID == e.Row.AttributeID)
				{
					Header.Current.ColAttributeID = null;
					Header.UpdateCurrent();
				}

				if (Header.Current.RowAttributeID == e.Row.AttributeID)
				{
					Header.Current.RowAttributeID = null;
					Header.UpdateCurrent();
				}
			}

			if (e.Row.IsActive != e.OldRow.IsActive)
			{
				Header.Cache.AdjustReadonly<MatrixAttributeSelectorAttribute>().ForAllFields(
					a => a.RefreshDummyValue(Header.Cache, Header.Current));

				RecalcAttributesGrid();
			}
		}

		protected virtual void RecalcAttributesGrid()
		{
			CSAttribute[] attributes = GetAdditionalAttributes();

			AdditionalAttributes.Current.Values = new string[attributes.Length];
			AdditionalAttributes.Current.Descriptions = new string[attributes.Length];
			AdditionalAttributes.Current.AttributeIdentifiers = new string[attributes.Length];
			AdditionalAttributes.Current.AttributeDisplayNames = new string[attributes.Length];
			AdditionalAttributes.Current.ViewNames = new string[attributes.Length];

			if (attributes.Length > 0)
			{
				for (int attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
				{
					CSAttribute attribute = attributes[attributeIndex];
					AdditionalAttributes.Current.AttributeIdentifiers[attributeIndex] = attribute.AttributeID;
					AdditionalAttributes.Current.AttributeDisplayNames[attributeIndex] = attribute.Description;
				}

				for (int attributeIndex = 0; attributeIndex < AdditionalAttributes.Current.Values.Length; attributeIndex++)
					AddFieldToAttributeGrid(AdditionalAttributes.Cache, attributeIndex);
			}

			AdditionalAttributes.View.RequestRefresh();
		}

		protected virtual InventoryItem GetTemplateItem()
			=> InventoryItem.PK.Find(Base, Header.Current.TemplateItemID);

		protected virtual CSAttribute[] GetAdditionalAttributes()
		{
			var item = GetTemplateItem();

			return new PXSelectReadonly2<CSAttribute,
				InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.isActive, Equal<True>,
					And<CSAttributeGroup.entityClassID, Equal<Required<InventoryItem.itemClassID>>,
					And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
					And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
					And<CSAttribute.attributeID, NotEqual<Current2<EntryHeader.colAttributeID>>,
					And<CSAttribute.attributeID, NotEqual<Current2<EntryHeader.rowAttributeID>>,
					And<NotExists<Select<CSAnswers,
						Where<CSAnswers.isActive.IsEqual<False>.
							And<CSAnswers.attributeID.IsEqual<CSAttribute.attributeID>>.
							And<CSAnswers.refNoteID.IsEqual<@P.AsGuid>>>>>>>>>>>>>(Base)
					.SelectMain(item?.ParentItemClassID, item?.NoteID);
		}

		protected virtual void AddFieldToAttributeGrid(PXCache cache, int attributeNumber)
		{
			string fieldName = $"AttributeValue{attributeNumber}";

			FillAttributeViewName(attributeNumber);

			if (!cache.Fields.Contains(fieldName))
			{
				cache.Fields.Add(fieldName);

				Base.FieldSelecting.AddHandler(
					cache.GetItemType(),
					fieldName,
					(s, e) => AttributeValueFieldSelecting(attributeNumber, e, fieldName));

				Base.FieldUpdating.AddHandler(
					cache.GetItemType(),
					fieldName,
					(s, e) => AttributeValueFieldUpdating(attributeNumber, e));
			}
		}

		protected virtual void FillAttributeViewName(int attributeNumber)
		{
			if (attributeNumber < AdditionalAttributes.Current.ViewNames?.Length)
			{
				string attributeID = AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber];
				string viewNameAttribute = $"attr{ attributeID }";

				var selectorAttribute = new MatrixAttributeValueSelectorAttribute(attributeNumber, ShowDisabledValue) { FieldName = viewNameAttribute };
				selectorAttribute.CacheAttached(AdditionalAttributes.Cache);

				var args = new PXFieldSelectingEventArgs(null, null, true, false);
				selectorAttribute.FieldSelecting(AdditionalAttributes.Cache, args);

				PXFieldState state = (PXFieldState)args.ReturnState;
				AdditionalAttributes.Current.ViewNames[attributeNumber] = state.ViewName;
			}
		}

		protected virtual void AttributeValueFieldSelecting(int attributeNumber, PXFieldSelectingEventArgs e, string fieldName)
		{
			PXFieldState state = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, 1, null, null, null, fieldName);
			e.ReturnState = state;

			if (attributeNumber < AdditionalAttributes.Current.ViewNames?.Length)
			{
				e.ReturnValue = GetAttributeValue(e.Row, attributeNumber);
				state.DisplayName = AdditionalAttributes.Current.AttributeDisplayNames[attributeNumber];
				state.Visible = true;
				state.Visibility = PXUIVisibility.Visible;
				state.Enabled = true;
				state.ViewName = AdditionalAttributes.Current.ViewNames[attributeNumber];
				state.DescriptionName = nameof(CSAttributeDetail.description);
				state.FieldList = new string[] { nameof(CSAttributeDetail.valueID), nameof(CSAttributeDetail.description) };
				var attributeDetailCache = Base.Caches<CSAttributeDetail>();
				state.HeaderList = new string[] { PXUIFieldAttribute.GetDisplayName<CSAttributeDetail.valueID>(attributeDetailCache),
							PXUIFieldAttribute.GetDisplayName<CSAttributeDetail.description>(attributeDetailCache) };
			}
			else
			{
				state.Value = null;
				e.ReturnValue = null;
				state.DisplayName = null;
				state.Visible = false;
				state.Visibility = PXUIVisibility.Invisible;
				state.Enabled = false;
				state.ViewName = null;
			}
		}

		protected virtual string GetAttributeValue(object row, int attributeNumber)
		{
			string returnValue = AdditionalAttributes.Current.Descriptions[attributeNumber];
			if (string.IsNullOrEmpty(returnValue))
				returnValue = AdditionalAttributes.Current.Values[attributeNumber];
			return returnValue;
		}

		protected virtual void AttributeValueFieldUpdating(int attributeNumber, PXFieldUpdatingEventArgs e)
		{
			AdditionalAttributes row = e.Row as AdditionalAttributes;
			if (row == null)
				return;

			string newValue = e.NewValue as string;

			if (attributeNumber < row.Values?.Length && row.Descriptions[attributeNumber] != newValue)
			{
				var attributeDetailSelect = new PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>,
						And<CSAttributeDetail.valueID, Equal<Required<CSAttributeDetail.valueID>>>>>(Base);

				if (!ShowDisabledValue)
					attributeDetailSelect.WhereAnd<Where<CSAttributeDetail.disabled, NotEqual<True>>>();

				CSAttributeDetail attributeDetail = attributeDetailSelect.Select(
					AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				if (attributeDetail == null)
					throw new RowNotFoundException(Base.Caches<CSAttributeDetail>(), AdditionalAttributes.Current.AttributeIdentifiers[attributeNumber], newValue);

				row.Values[attributeNumber] = attributeDetail.ValueID;
				row.Descriptions[attributeNumber] = attributeDetail.Description;
			}
		}

		protected virtual bool AllAdditionalAttributesArePopulated() => AdditionalAttributes.Current?.Values?.Any(v => string.IsNullOrEmpty(v)) == false;
	}
}
