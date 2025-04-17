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
using PX.Objects.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.Common.GraphExtensions
{
	public abstract class ForbidDuplicateDetailsExtension<TGraph, TDocument, TDetail>: PXGraphExtension<TGraph>
		where TGraph: PXGraph
		where TDocument: class, IBqlTable, new()
		where TDetail : class, IBqlTable, new()
	{
		private PXCache<TDocument> _documentCache;
		private PXCache<TDetail> _detailsCache;

		protected PXCache<TDocument> DocumentCache => _documentCache ??= Base.Caches<TDocument>();
		protected PXCache<TDetail> DetailsCache => _detailsCache ??= Base.Caches<TDetail>();

		#region Abstract methods

		protected abstract IEnumerable<Type> GetDetailUniqueFields();

		protected abstract void RaiseDuplicateError(TDetail duplicate);

		#endregion

		protected virtual bool ForbidDuplicates(TDocument document)
		{
			return document != null
				&& DetailsCache.IsDirty
				&& DocumentCache.GetStatus(document).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted);
		}

		#region Queries initialization

		protected virtual PXSelect<TDetail> InitializeDuplicatesLoadQuery(TDetail detail)
		{
			var query = new PXSelect<TDetail>(Base);
			var keyFields = DetailsCache.BqlKeys.ToList();

			foreach (var field in GetDetailUniqueFields())
			{
				if (keyFields.Contains(field))
					keyFields.Remove(field);

				var val = DetailsCache.GetValue(detail, field.Name);
				if (val == null)
					andIsNull(field);
				else
					andEqualCurrent(field);
			}

			Type excludeWhere = null;
			foreach(var keyField in keyFields)
			{
				var where = BqlCommand.Compose(
					typeof(Where<,>),
					keyField,
					typeof(NotEqual<>),
					typeof(Current<>), keyField);

				excludeWhere = excludeWhere == null
					? where
					: BqlCommand.Compose(typeof(Where2<,>), excludeWhere, typeof(Or<>), where);
			}

			if (excludeWhere != null)
				query.WhereAnd(excludeWhere);

			void andIsNull(Type field)
			{
				query.WhereAnd(BqlCommand.Compose(typeof(Where<,>), field, typeof(IsNull)));
			};

			void andEqualCurrent(Type field)
			{
				query.WhereAnd(BqlCommand.Compose(
					typeof(Where<,>),
					field,
					typeof(Equal<>),
					typeof(Current<>), field));
			}
			return query;
		}

		#endregion

		protected virtual TDetail[] LoadDetails(TDocument document)
		{
			return PXParentAttribute.SelectChildren(DetailsCache, document, typeof(TDocument))
				.OfType<TDetail>()
				.ToArray();
		}

		public virtual void CheckForDuplicates()
		{
			var document = (TDocument)DocumentCache.Current;
			if (!ForbidDuplicates(document))
				return;

			TDetail[] details;

			if (!DetailsCache.Deleted.Any_())
			{
				var inserted = DetailsCache.Inserted.OfType<TDetail>().ToArray();
				var updated = DetailsCache.Updated.OfType<TDetail>().ToArray();
				if (inserted.Any() && !updated.Any() && inserted.Length <= 5
					|| updated.Any() && !inserted.Any() && updated.Length == 1)
				{
					details = inserted.Concat(updated).ToArray();
					if (CheckForDuplicates(details))
						return;

					var docStatus = DocumentCache.GetStatus(document);

					if (docStatus != PXEntryStatus.Inserted)
					{
						foreach (var detail in details)
						{
							CheckForDuplicateOnDB(detail);
						}
					}
					return;
				}
			}

			details = LoadDetails(document);

			CheckForDuplicates(details);
		}

		public virtual bool CheckForDuplicates(TDetail[] details)
		{
			if (details.Count() <= 1)
				return false;

			IEqualityComparer<TDetail> duplicateComparer =
				new FieldSubsetEqualityComparer<TDetail>(
					DetailsCache, GetDetailUniqueFields());

			IEnumerable<TDetail> duplicates = details
				.GroupBy(detail => detail, duplicateComparer)
				.Where(duplicatesGroup => duplicatesGroup.HasAtLeastTwoItems())
				.Flatten();

			foreach (TDetail duplicate in duplicates)
			{
				RaiseDuplicateError(duplicate);
			}

			return duplicates.Any();
		}

		public virtual bool CheckForDuplicateOnDB(TDetail detail)
		{
			var duplicatesLoadQuery = InitializeDuplicatesLoadQuery(detail);

			var duplicate = (TDetail)duplicatesLoadQuery.View.SelectSingleBound(new[] { detail });

			if (duplicate != null)
			{
				RaiseDuplicateError(detail);
				return true;
			}
			return false;
		}
	}
}
