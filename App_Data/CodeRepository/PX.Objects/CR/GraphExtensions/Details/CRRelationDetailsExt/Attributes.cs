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
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Compilation;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR.Extensions.Cache;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	/// <exclude/>
	public class StatusFieldAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		protected readonly Type _entityTypeField;

		protected string _entityTypeFieldName { get; set; }

		public StatusFieldAttribute(Type entityTypeField)
		{
			_entityTypeField = entityTypeField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_entityTypeFieldName = sender.GetField(_entityTypeField);
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
				return;

			var entityTypeName = sender.GetValue(e.Row, _entityTypeFieldName) as string;

			if (entityTypeName == null)
				return;

			var entityTypeCache = sender.Graph.Caches[GetRelatedEntityType(entityTypeName)];

			var field = entityTypeCache.GetField_WithAttribute<CRRelationDetail>();

			if (field.FieldName == null)
				return;

			if (field.Attribute.StatusFieldName == null)
				return;

			var stateOfTargetRecord = sender.Graph.Caches[GetRelatedEntityType(entityTypeName)].GetStateExt(null, field.Attribute.StatusFieldName);

			e.ReturnState = stateOfTargetRecord;

			if (e.ReturnState is PXFieldState state)
			{
				state.Enabled = false;
			}

			e.ReturnValue = sender.GetValue(e.Row, this._FieldName);
		}

		protected Type GetRelatedEntityType(string typeName) => PXBuildManager.GetType(typeName, false);
	}

	/// <exclude/>
	public class CRRelationDetail : PXEventSubscriberAttribute
	{
		protected readonly Type StatusField;
		protected readonly Type DescriptionField;
		protected readonly Type OwnerField;

		public string StatusFieldName;
		public string DescriptionFieldName;
		public string OwnerFieldName;

		public CRRelationDetail(Type statusField, Type descriptionField, Type ownerField)
		{
			StatusField = statusField;
			DescriptionField = descriptionField;
			OwnerField = ownerField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (StatusField != null)
				StatusFieldName = sender.GetField(StatusField);

			if (DescriptionField != null)
				DescriptionFieldName = sender.GetField(DescriptionField);

			if (OwnerField != null)
				OwnerFieldName = sender.GetField(OwnerField);
		}

		public (string, string, int?) GetValues(PXCache cache, object entity)
		{
			(string, string, int?) result = (null, null, null);

			if (this.StatusFieldName != null)
				result.Item1 = cache.GetValue(entity, this.StatusFieldName) as string;

			if (this.DescriptionFieldName != null)
				result.Item2 = cache.GetValue(entity, this.DescriptionFieldName) as string;

			if (this.OwnerFieldName != null)
				result.Item3 = cache.GetValue(entity, this.OwnerFieldName) as int?;

			return result;
		}
	}
}
