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
using PX.Common;
using PX.Data;

namespace PX.Objects.CR
{
	public class EntityIDSelectorAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber
	{
		protected static string ViewNamePrefix => "_ENTITYID_SELECTOR_";
		protected static string ViewSearchPrefix => "_ENTITYID_SEARCHSELECTOR_";
		protected string DescriptionFieldPostfix => "_Description";

		protected Type _typeBqlField { get; set; }
		protected string _typeFieldName { get; set; }
		protected string _descriptionFieldName { get; set; }

		protected PXGraph Graph;

		public EntityIDSelectorAttribute(Type typeBqlField)
		{
			_typeBqlField = typeBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			Graph = sender.Graph;

			_typeFieldName = sender.GetField(_typeBqlField);

			_descriptionFieldName = _FieldName + DescriptionFieldPostfix;
			sender.Fields.Add(_descriptionFieldName);
			Graph.FieldSelecting.AddHandler(sender.GetItemType(), _descriptionFieldName, _Description_FieldSelecting);
		}

		public virtual void _Description_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string info = GetDescription(sender, e.Row);

			var displayName = PXUIFieldAttribute.GetDisplayName(sender, this.FieldName) ?? _descriptionFieldName;

			e.ReturnState = PXFieldState.CreateInstance(info, typeof(string),
				fieldName: _descriptionFieldName,
				displayName: displayName,
				enabled: false,
				visible: !string.IsNullOrEmpty(info),
				visibility: PXUIVisibility.Visible);
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!(e.NewValue is string))
				return;

			var guid = Guid.TryParse((string)e.NewValue, out var val)
				? val as Guid?
				: null;

			if (guid != null)
			{
				e.NewValue = guid;
			}
			else
			{
				Type itemType = (e.Row ?? sender.Current)
					.With(row => sender.GetValue(row, _typeFieldName) as string)
					.With(typeName => System.Web.Compilation.PXBuildManager.GetType(typeName, false));

				PXCache itemCache = Graph.Caches[itemType];

				var keysNames = itemCache.GetAttributes(null)
					.OfType<PXDBFieldAttribute>()
					.Where(_ => _.IsKey)
					.Select(_ => _.FieldName)
					.ToArray();

				string[] keysValues;

				if (keysNames.Length > 1)
				{
					keysValues = ((string)e.NewValue)
						.Split(',')
						.Select(_ => _.Trim())
						.ToArray();
				}
				else
				{
					var noteAtt = EntityHelper.GetNoteAttribute(itemType);
					keysNames = new[] { noteAtt.DescriptionField.Name };
					keysValues = new[] { (string)e.NewValue };
				}

				if (keysValues.Length != keysNames.Length)
					return;

				var state = (PXFieldState)sender.GetStateExt(e.Row, _FieldName);

				PXView view = Graph.Views[state.ViewName];

				int startRow = 0;
				int totalRow = 0;
				var result = view.Select(null, null, keysValues, keysNames, new[] { true }, null, ref startRow, 1, ref totalRow);

				if (result.Count == 0)
					return;

				var valueField = EntityHelper.GetNoteField(itemType);

				if (result[0] is PXResult)
				{
					var item = ((PXResult)result[0])[itemType];

					if (item != null)
					{
						e.NewValue = itemCache.GetValue(((PXResult)result[0])[itemType], valueField);
					}
					else
					{
						// Let's guess the [0] element of PXResult is the required one
						item = ((PXResult)result[0])[0];
						itemType = item.GetType();
						itemCache = Graph.Caches[itemType];
						e.NewValue = itemCache.GetValue(item, valueField);
					}
				}
				else
					e.NewValue = itemCache.GetValue(result?[0], valueField);
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var itemType = (e.Row ?? sender.Current)
				.With(row => sender.GetValue(row, _typeFieldName) as string)
				.With(typeName => System.Web.Compilation.PXBuildManager.GetType(typeName, false));

			if (itemType != null)
			{
				var itemCache = Graph.Caches[itemType];
				var noteAtt = EntityHelper.GetNoteAttribute(itemType, false);
				foreach (var extType in itemCache.GetExtensionTypes().Reverse())
				{

					var extNoteAtt = EntityHelper.GetNoteAttribute(extType);
					if (extNoteAtt != null && extNoteAtt.ShowInReferenceSelector == true)
					{
						noteAtt = extNoteAtt;
						break;
					}
				}

				CreateSelectorView(Graph, itemType, noteAtt, out var viewName, out var fieldList, out var headerList);

				if (noteAtt.FieldList != null && noteAtt.FieldList.Length > 0)
				{
					fieldList = new string[noteAtt.FieldList.Length];
					for (int i = 0; i < noteAtt.FieldList.Length; i++)
					{
						fieldList[i] = noteAtt.FieldList[i].Name;
					}

					headerList = null;
				}

				fieldList = fieldList ?? new EntityHelper(Graph).GetFieldList(itemType);

				headerList = headerList ?? GetFieldDisplayNames(itemCache, fieldList);

				var keys = itemCache.Keys.ToArray();
				var valueField = EntityHelper.GetNoteField(itemType);
				var textField = noteAtt.DescriptionField.With(df => df.Name) ?? keys.Last();

				var fieldState = PXFieldState.CreateInstance(e.ReturnState, null,
					fieldName: _FieldName,
					viewName: viewName,
					fieldList: fieldList,
					headerList: headerList);

				fieldState.ValueField = valueField;
				fieldState.DescriptionName = textField;
				fieldState.SelectorMode = PXSelectorMode.NoAutocomplete;

				// for cb it should be guid
				if (Graph.IsContractBasedAPI && !Graph.IsImport)
					return;

				e.ReturnState = fieldState;
			}
			else
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null,
					fieldName: _FieldName,
					enabled: e.Row == null,
					viewName: Graph.PrimaryView);

				((PXFieldState)e.ReturnState).ValueField = "noteID";
				((PXFieldState)e.ReturnState).DescriptionName = _descriptionFieldName;
				((PXFieldState)e.ReturnState).SelectorMode = PXSelectorMode.NoAutocomplete;
			}
		}

		protected virtual string GetDescription(PXCache sender, object row)
		{
			var noteId = (Guid?)sender.GetValue(row, _FieldOrdinal);
			var typeName = sender.GetValue(row, _typeFieldName) as string;

			var type = typeName != null
				? System.Web.Compilation.PXBuildManager.GetType(typeName, false)
				: null;

			return noteId.HasValue && noteId.Value != Guid.Empty
				? EntityHelper.GetEntityDescription(Graph, new EntityHelper(Graph).GetEntityRow(type, noteId, false))
				: null;
		}

		protected virtual void CreateSelectorView(PXGraph graph, Type itemType, PXNoteAttribute noteAtt, out string viewName, out string[] fieldList, out string[] headerList)
		{
			viewName = null;
			fieldList = null;
			headerList = null;

			var cache = graph.Caches[typeof(BAccount)];
			cache = graph.Caches[typeof(AR.Customer)];
			cache = graph.Caches[typeof(AP.Vendor)];
			cache = graph.Caches[typeof(EP.EPEmployee)];

			PXFieldState state;

			if (typeof(IBqlField).IsAssignableFrom(noteAtt.Selector)
				&& (state = AddFieldView(graph, noteAtt.Selector)) != null)
			{
				viewName = state.ViewName;
				fieldList = state.FieldList;
				headerList = state.HeaderList;
			}
			if (typeof(IBqlSearch).IsAssignableFrom(noteAtt.Selector))
				viewName = AddSelectorView(graph, noteAtt.Selector);

			if (viewName == null)
				viewName = AddView(graph, itemType);
		}

		private string[] GetFieldDisplayNames(PXCache itemCache, string[] fieldList)
		{
			var result = new string[fieldList.Length];

			for (int i = 0; i < fieldList.Length; i++)
			{
				var field = fieldList[i];
				var fs = itemCache.GetStateExt(null, field) as PXFieldState;

				if (fs != null && !string.IsNullOrEmpty(fs.DisplayName))
					result[i] = fs.DisplayName;
				else
					result[i] = field;
			}

			return result;
		}

		protected static PXFieldState AddFieldView(PXGraph graph, Type selectorField)
		{
			var table = BqlCommand.GetItemType(selectorField);
			var cache = graph.Caches[table];
			var field = cache.GetField(selectorField);

			return cache.GetStateExt(null, field) as PXFieldState;
		}

		protected static string GetSelectorViewName(Type nameType) => ViewSearchPrefix + nameType?.DeclaringType?.FullName + "." + nameType?.Name;

		protected static string AddSelectorView(PXGraph graph, Type search)
		{
			Type nameType = search.GenericTypeArguments[0];
			var viewName = GetSelectorViewName(nameType);

			if (!graph.Views.ContainsKey(viewName))
			{
				var command = BqlCommand.CreateInstance(search);
				var newView = new PXView(graph, true, command);
				graph.Views.Add(viewName, newView);
			}
			else
			{
				var command = BqlCommand.CreateInstance(search);
				var newView = new PXView(graph, true, command);
			}

			return viewName;
		}

		protected static string AddView(PXGraph graph, Type itemType)
		{
			var viewName = ViewNamePrefix + itemType.GetLongName();

			if (graph.Views.ContainsKey(viewName))
				return viewName;

			var command = BqlCommand.CreateInstance(typeof(Select<>), itemType);
			var newView = new PXView(graph, true, command);

			graph.Views.Add(viewName, newView);

			return viewName;
		}
	}
}
