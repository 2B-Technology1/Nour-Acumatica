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
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using System.Collections.Generic;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public abstract class DocumentSelectorProvider
    {
        protected readonly PXGraph Graph;
        protected readonly PXCache Cache;

		private readonly EntityHelper _entityHelper;
        private readonly string _fieldName;
		private readonly string[] _selectorFields;
        private readonly string[] _selectorHeaders;
        private readonly string _viewName;

        protected DocumentSelectorProvider(PXGraph graph, string fieldName)
        {
            Graph = graph;
            Cache = graph.Caches[SelectorType];

			_fieldName = fieldName;
			_entityHelper = new EntityHelper(graph);
			_selectorFields = GetSelectorFields();
            _selectorHeaders = GetSelectorHeaderNames();
            _viewName = GetViewName();
		}

		public abstract string DocumentType
        {
            get;
        }

        protected abstract Type SelectorType
        {
            get;
        }

        protected abstract Type SelectorQuery
        {
            get;
        }

        protected abstract Type[] SelectorFieldTypes
        {
            get;
        }

		protected virtual Type DescriptionFieldType => null;

		private string DescriptionFieldName => DescriptionFieldType?.Name;

		protected abstract Type SubstituteKeyType
		{
			get;
		}

		private string SubstituteKeyName => SubstituteKeyType?.Name;

		public abstract void NavigateToDocument(Guid? noteId);

        public void CreateViewIfRequired()
        {
            if (!Graph.Views.ContainsKey(_viewName))
            {
                Graph.Views.Add(_viewName, CreateView());
            }
        }

		public virtual void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (SubstituteKeyName == null)
			{
				return;
			}

			if (e.NewValue == null)
			{
				return;
			}

			if (Guid.TryParse(e.NewValue.ToString(), out _))
			{
				return;
			}

			var noteId = GetReferencedNoteIdBySubstituteKey(e.NewValue);

			if (noteId == null)
			{
				return;
			}

			e.NewValue = noteId;
		}

		public PXFieldState GetFieldState(RequestForInformationRelation requestForInformationRelation, object returnState)
        {
			var fieldState = CreateFieldState(returnState);
			fieldState.ValueField = SubstituteKeyName ?? EntityHelper.GetNoteField(SelectorType);
			fieldState.DescriptionName = DescriptionFieldName;
            fieldState.SelectorMode = PXSelectorMode.TextModeSearch;
            fieldState.Value = GetDocumentDescription(requestForInformationRelation.DocumentNoteId);
            return fieldState;
        }

        public void AddDescriptionFieldIfRequired()
        {
			if (DescriptionFieldName == null)
			{
				return;
			}

            if (!Cache.Fields.Contains(DescriptionFieldName))
            {
                Cache.Fields.Add(DescriptionFieldName);
                Graph.FieldSelecting.AddHandler(SelectorType, DescriptionFieldName,
                    SelectorEntity_Description_FieldSelecting);
            }
        }

        protected virtual string[] GetSelectorHeaderNames()
        {
            return _selectorFields.Select(x => PXUIFieldAttribute.GetDisplayName(Cache, x)).ToArray();
        }

        protected virtual string GetDocumentDescription(Guid? noteId)
        {
            return _entityHelper.GetEntityDescription(noteId, SelectorType);
        }

        private void SelectorEntity_Description_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            var description = GetDocumentDescription(_entityHelper.GetEntityNoteID(args.Row));
            args.ReturnState = PXFieldState.CreateInstance(description, null, null, null,
                null, null, null, null, DescriptionFieldName, null, null, null, PXErrorLevel.Undefined,
                false, false, null, PXUIVisibility.Invisible, _viewName);
        }

        private PXFieldState CreateFieldState(object returnState)
        {
            return PXFieldState.CreateInstance(returnState, null, null, null,
                null, null, null, null, _fieldName, DescriptionFieldName, null, null, PXErrorLevel.Undefined, null, true, null,
                PXUIVisibility.Undefined, _viewName, _selectorFields, _selectorHeaders);
        }

        private PXView CreateView()
        {
            var bqlCommand = BqlCommand.CreateInstance(SelectorQuery);
            return new PXView(Graph, true, bqlCommand);
        }

		private PXView CreateViewBySubstituteKey()
		{
			var bqlCommand = BqlCommand
				.CreateInstance(SelectorQuery)
				.WhereAnd(BqlCommand.Compose(typeof(Where<,>), SubstituteKeyType, typeof(Equal<>), typeof(Required<>), SubstituteKeyType));

			return new PXView(Graph, true, bqlCommand);
		}

		private Guid? GetReferencedNoteIdBySubstituteKey(object substituteKey)
		{
			if (substituteKey == null)
			{
				return null;
			}

			var noteIdFieldName = EntityHelper.GetNoteField(SelectorType);

			if (string.IsNullOrWhiteSpace(noteIdFieldName))
			{
				return null;
			}

			var refView = CreateViewBySubstituteKey();

			var parameters = new List<object>() { substituteKey };

			var (startRow, totalRows) = (PXView.StartRow, 0);

			var result = refView.Select(
							PXView.Currents,
							parameters.ToArray(),
							PXView.Searches,
							PXView.SortColumns,
							PXView.Descendings,
							PXView.Filters,
							ref startRow,
							PXView.MaximumRows,
							ref totalRows);

			if (result.Count != 1)
			{
				return null;
			}

			var referencedObject = result.Single();

			return (Guid?)refView.Cache.GetValue(referencedObject, noteIdFieldName);
		}

		private string GetViewName()
        {
            return GetType().FullName;
        }

        private string[] GetSelectorFields()
        {
            return SelectorFieldTypes.Select(x => Cache.GetField(x)).ToArray();
        }
    }
}
