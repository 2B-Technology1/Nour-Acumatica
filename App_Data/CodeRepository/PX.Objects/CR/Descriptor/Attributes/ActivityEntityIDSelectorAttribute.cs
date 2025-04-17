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
using System.Collections.Generic;
using System.Web.Compilation;
using PX.Common;
using PX.Data;
using PX.Data.SQLTree;
using static PX.SM.EMailAccount;

namespace PX.Objects.CR
{
	public class ActivityEntityIDSelectorAttribute : EntityIDSelectorAttribute, IPXFieldUpdatedSubscriber, IPXRowPersistingSubscriber //, IPXRowSelectingSubscriber
	{
		#region State

		protected readonly Type _contactIdBqlField;
		protected string _contactIdFieldName { get; set; }

		protected readonly Type _baccountIdBqlField;
		protected string _baccountIdFieldName { get; set; }

		protected PXView RelatedEntity { get; set; }
		protected EntityHelper EntityHelperInstance { get; set; }

		#endregion

		#region ctor

		public ActivityEntityIDSelectorAttribute(Type typeBqlField, Type contactIdBqlField, Type baccountIdBqlField)
			: base(typeBqlField)
		{
			_contactIdBqlField = contactIdBqlField;
			_baccountIdBqlField = baccountIdBqlField;
		}

		#endregion

		#region Events

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			Graph = sender.Graph;

			_contactIdFieldName = sender.GetField(_contactIdBqlField);
			_baccountIdFieldName = sender.GetField(_baccountIdBqlField);

			Graph.FieldUpdated.AddHandler(sender.GetItemType(), _typeBqlField.Name, _RelatedEntityType_FieldUpdated);
			EntityHelperInstance = new EntityHelper(Graph);
		}

		public virtual void _RelatedEntityType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row;

			if (row == null)
				return;

			sender.SetValue(row, this.FieldName, null);
		}

		public override void _Description_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			Guid? noteId = (Guid?)sender.GetValue(e.Row ?? sender.Current, _FieldOrdinal);

			string info = noteId.HasValue && noteId.Value != Guid.Empty
				? EntityHelperInstance.GetEntityDescription((Guid)noteId, sender.GetItemType())
				: null;

			var isNotEmpty = !string.IsNullOrEmpty(info);

			e.ReturnState = PXFieldState.CreateInstance(info, typeof(string),
				fieldName: _descriptionFieldName,
				displayName: _descriptionFieldName,
				errorLevel: PXErrorLevel.Undefined,
				enabled: false,
				visible: isNotEmpty,
				visibility: PXUIVisibility.Visible
			);
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row;

			if (row == null)
				return;

			var typeName = sender.GetValue(row, _typeFieldName) as string;

			if (typeName == null)
				return;

			var rowType = GetRelatedEntityType(typeName);

			var cache = Graph.Caches[rowType];
			var entity = EntityHelperInstance.GetEntityRow(rowType, sender.GetValue(row, FieldName) as Guid?);
			var noteField = EntityHelper.GetNoteField(rowType);

			Graph.EnsureCachePersistence<Note>();

			PXNoteAttribute.GetNoteID(cache, entity, noteField);

			Graph.Caches[typeof(Note)].Persist(PXDBOperation.Insert);
			Graph.Caches[typeof(Note)].Persisted(false);
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null
				|| object.Equals(sender.GetValue(e.Row, this._FieldName), e.OldValue))
				return;

			FillRefNoteIDType(sender, e.Row);

			// We assume that ContacID and BAccountID may be filled with needed empty values, and should not be redefaulted (because it's a heavy operation)
			// The assumption is that pending value may exist in unattended mode only if set explicitly by human being 
			if (sender.Graph.UnattendedMode
				&& (sender.GetValuePending(e.Row, _contactIdFieldName) == PXCache.NotSetValue
					|| sender.GetValuePending(e.Row, _baccountIdFieldName) == PXCache.NotSetValue)
				)
				return;

			// Acuminator disable once PX1045 PXGraphCreateInstanceInEventHandlers [Legacy]
			FillContactAndBAccount(sender, e.Row);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (e.ReturnState is PXFieldState state)
			{
				state.Enabled = (!sender.Graph.IsMobile && sender.GetValue(e.Row, _typeFieldName) != null) || sender.Graph.IsContractBasedAPI;
			}
		}

		#endregion

		#region Methods

		protected Type GetRelatedEntityType(string typeName) => PXBuildManager.GetType(typeName, false);

		protected virtual Type GetRelatedEntity(object row)
		{
			if (row == null)
				return null;

			var typeName = Graph.Caches[this.BqlTable].GetValue(row, _typeFieldName) as string;

			if (typeName == null)
			{
				if (!(Graph.Caches[this.BqlTable].GetValue(row, this.FieldName) is Guid refNoteID))
					return null;

				// Get from Note
				return EntityHelperInstance.GetEntityRowType(refNoteID, true);
			}
			else
			{
				return GetRelatedEntityType(typeName);
			}
		}

		public virtual void FillRefNoteIDType(PXCache sender, object row)
		{
			if (row == null)
				return;

			var refNoteTypeString = Graph.Caches[this.BqlTable].GetValue(row, _typeFieldName) as string;

			if (refNoteTypeString != null)
				return;

			refNoteTypeString = GetRelatedEntity(row)?.FullName;

			Graph.Caches[this.BqlTable].SetValue(row, _typeFieldName, refNoteTypeString);
		}

		public virtual void FillContactAndBAccount(PXCache sender, object row)
		{
			if (row == null)
				return;

			var refNoteTypeString = Graph.Caches[this.BqlTable].GetValue(row, _typeFieldName) as string;

			if (refNoteTypeString == null)
				return;

			Type refNoteType = PXBuildManager.GetType(refNoteTypeString, false);

			var refNoteID = Graph.Caches[this.BqlTable].GetValue(row, this.FieldName) as Guid?;

			EntityHelper helper = new EntityHelper(Graph);
			var related = helper.GetEntityRow(refNoteType, refNoteID);
			if (related == null)
				return;

			var graphType = helper.GetPrimaryGraphType(refNoteType, related, true);
			if (graphType == null)
				return;

			var copy = sender.CreateCopy(row);
			var graph = PXGraph.CreateInstance(graphType);

			var noteType = EntityHelper.GetNoteType(refNoteType);
			var view = new PXView(Graph, false, BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<,>), refNoteType, typeof(Where<,>), noteType, typeof(Equal<>), typeof(Required<>), noteType)));
			graph.Caches[refNoteType].Current = view.SelectSingle(refNoteID);
			if (graph.Caches[refNoteType].Current == null)
				return;

			graph.Caches[refNoteType].SetStatus(graph.Caches[refNoteType].Current, PXEntryStatus.Inserted);
			var cache = graph.Caches[sender.GetItemType()];

			cache.SetDefaultExt(copy, _contactIdFieldName);
			cache.SetDefaultExt(copy, _baccountIdFieldName);

			sender.SetValue(row, _contactIdFieldName, sender.GetValue(copy, _contactIdFieldName));
			sender.SetValue(row, _baccountIdFieldName, sender.GetValue(copy, _baccountIdFieldName));
		}

		#endregion
	}
}
