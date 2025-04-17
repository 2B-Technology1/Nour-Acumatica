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
using System;

namespace PX.Objects.CR
{
	public class PXReverseRelationAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		#region Events

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRRelation;

			if (row?.TargetNoteID == null)
				return;

			var existing = CRRelation.UK.Find(sender.Graph,
				refNoteID: row.RefNoteID,
				targetNoteID: row.TargetNoteID,
				role: row.Role);

			if (existing != null && existing.RelationID != row.RelationID)
			{
				sender.RaiseExceptionHandling<CRRelation.targetNoteID>(row, row.TargetNoteID, new PXSetPropertyException(Messages.RelationAlreadyExists, PXErrorLevel.RowError));
			}
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			var row = e.Row as CRRelation;

			if (row == null)
				return;

			if (e.TranStatus != PXTranStatus.Open)
				return;

			var dbRow = sender.GetOriginal(row) as CRRelation;
			var wasBidirectional = IsBidirectionalRole(dbRow);
			var isBidirectional = IsBidirectionalRole(row);

			if (!wasBidirectional && !isBidirectional)
				return;

			switch (e.Operation.Command())
			{
				case PXDBOperation.Insert:
				{
					if (!isBidirectional)
						return;

					var inversedRelation = Invert(sender, row);

					InsertRelationInDB(sender, inversedRelation);

					break;
				}

				case PXDBOperation.Update:
				{
					if (wasBidirectional && !isBidirectional)
						goto case PXDBOperation.Delete;

					if (dbRow == null || !IsSignificantlyChanged(sender, row, dbRow))
						return;

					var inversedRelation = SearchForInversedRelation(sender.Graph, dbRow);

					if (inversedRelation == null)
						return;

					var relationID = inversedRelation.RelationID;

					sender.RestoreCopy(inversedRelation, Invert(sender, row));

					inversedRelation.RelationID = relationID;

					UpdateRelationInDB(sender, inversedRelation);

					break;
				}

				case PXDBOperation.Delete:
				{
					if (dbRow == null)
						return;

					var inversedRelation = SearchForInversedRelation(sender.Graph, dbRow);

					if (inversedRelation == null)
						return;

					DeleteRelationInDB(sender, inversedRelation);

					break;
				}
			}
		}

		#endregion

		#region Methods

		public virtual CRRelation Invert(PXCache sender, CRRelation relation)
		{
			if (relation == null)
				return null;

			var result = sender.CreateCopy(relation) as CRRelation;

			if (result == null)
				return null;

			result.RelationID = null;

			result.RefNoteID = relation.TargetNoteID;
			result.RefEntityType = relation.TargetType;
			result.TargetNoteID = relation.RefNoteID;
			result.TargetType = relation.RefEntityType;
			result.Role = GetOppositeRole(relation);

			sender.RaiseFieldDefaulting<CRRelation.entityID>(result, out var newEntityID);
			sender.SetValue<CRRelation.entityID>(result, newEntityID);

			sender.RaiseFieldDefaulting<CRRelation.contactID>(result, out var newContactID);
			sender.SetValue<CRRelation.contactID>(result, newContactID);

			return result;
		}

		protected virtual void InsertRelationInDB(PXCache sender, CRRelation relation)
		{
			PreventRecursionCall.Execute(() =>
			{
				var insertedRecord = sender.Insert(relation);

				if (insertedRecord == null)
					return;

				sender.PersistInserted(insertedRecord);
			});
		}

		protected virtual void UpdateRelationInDB(PXCache sender, CRRelation relation)
		{
			PreventRecursionCall.Execute(() =>
			{
				var updatededRecord = sender.Update(relation);

				if (updatededRecord == null)
					return;

				sender.PersistUpdated(updatededRecord);
			});
		}

		protected virtual void DeleteRelationInDB(PXCache sender, CRRelation relation)
		{
			PreventRecursionCall.Execute(() =>
			{
				var deletedRecord = sender.Delete(relation);

				if (deletedRecord == null)
					return;

				sender.PersistDeleted(deletedRecord);
			});
		}

		public virtual bool IsSignificantlyChanged(PXCache sender, CRRelation row, CRRelation oldRow)
		{
			if (row == null || oldRow == null)
				return true;

			return !sender.ObjectsEqual<CRRelation.refEntityType>(row, oldRow)
					|| !sender.ObjectsEqual<CRRelation.refNoteID>(row, oldRow)
					|| !sender.ObjectsEqual<CRRelation.targetType>(row, oldRow)
					|| !sender.ObjectsEqual<CRRelation.targetNoteID>(row, oldRow);
		}

		public static bool IsBidirectionalRole(CRRelation relation)
		{
			switch (relation.Role)
			{
				case CRRoleTypeList.Parent:
				case CRRoleTypeList.Child:
				case CRRoleTypeList.Derivative:
				case CRRoleTypeList.Source:
					return true;
				default:
					return false;
			}
		}

		public static string GetOppositeRole(CRRelation relation)
		{
			switch (relation.Role)
			{
				case CRRoleTypeList.Parent:
					return CRRoleTypeList.Child;
				case CRRoleTypeList.Child:
					return CRRoleTypeList.Parent;
				case CRRoleTypeList.Derivative:
					return CRRoleTypeList.Source;
				case CRRoleTypeList.Source:
					return CRRoleTypeList.Derivative;
				default:
					return relation.Role;
			}
		}

		public static CRRelation SearchForInversedRelation(PXGraph graph, CRRelation relation)
		{
			return CRRelation.UK.Find(graph,
				refNoteID: relation.TargetNoteID,
				targetNoteID: relation.RefNoteID,
				role: GetOppositeRole(relation));
		}

		#endregion
	}
}
