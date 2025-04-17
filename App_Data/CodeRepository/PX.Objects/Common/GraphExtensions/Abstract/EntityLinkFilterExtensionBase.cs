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
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
	public abstract class EntityLinkFilterExtensionBase<TGraph, TGraphFilter, TGraphFilterEntityID, TEntity, TEntityID, TDataType> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
		where TGraphFilter : class, IBqlTable, new()
		where TGraphFilterEntityID : class, IBqlField
		where TEntity : class, IBqlTable, new()
		where TEntityID : class, IBqlField
	{
		protected abstract string EntityViewName { get; }

		#region Event Handlers
		protected abstract void _(Events.CacheAttached<TEntityID> e);

		protected virtual void _(Events.RowInserted<TEntity> e)
		{
			var cache = e.Cache.Graph.Caches<TGraphFilter>();
			cache.SetValueExt<TGraphFilterEntityID>(cache.Current, null);
		}

		protected virtual void _(Events.RowUpdated<TEntity> e)
		{
			var cache = e.Cache.Graph.Caches<TGraphFilter>();
			cache.SetValueExt<TGraphFilterEntityID>(cache.Current, null);
		}

		protected virtual void _(Events.FieldSelecting<TGraphFilter, TGraphFilterEntityID> e)
		{
			if (GetEntities().Any())
				e.ReturnValue = PXMessages.LocalizeNoPrefix(IN.Messages.ListPlaceholder);
		}

		protected virtual void _(Events.FieldUpdated<TGraphFilter, TGraphFilterEntityID> e)
		{
			if (e.Cache.GetValue<TGraphFilterEntityID>(e.Row) != null)
				Base.Caches<TEntity>().Clear();
		}
		#endregion

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations
		private int excelRowNumber = 2;
		private bool importHasError = false;

		// Acuminator disable once PX1096 PXOverrideSignatureMismatch [The method is guaranteed by the TGraph restrictions]
		/// Overrides <see cref="PXImportAttribute.IPXPrepareItems.PrepareImportRow(string, IDictionary, IDictionary)"/>
		[PXOverride]
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values,
			Func<string, IDictionary, IDictionary, bool> base_PrepareImportRow)
		{
			bool baseResult = base_PrepareImportRow(viewName, keys, values);

			try
			{
				if (viewName.Equals(EntityViewName, StringComparison.InvariantCultureIgnoreCase))
					ImportEntity(values);
			}
			catch (Exception e)
			{
				PXTrace.WriteError(SO.Messages.RowError, excelRowNumber, e.Message);
				importHasError = true;
			}
			finally
			{
				excelRowNumber++;
			}

			if (viewName == EntityViewName)
				return false;

			return baseResult;
		}

		protected virtual void ImportEntity(IDictionary values)
		{
			PXCache entityCache = Base.Caches<TEntity>();
			TEntity entity = (TEntity)entityCache.CreateInstance();

			if (values.Contains(typeof(TEntityID).Name))
			{
				try
				{
					object value = values[typeof(TEntityID).Name];
					entityCache.SetValueExt<TEntityID>(entity, value);
					entityCache.Update(entity);
				}
				catch (Exception e)
				{
					PXTrace.WriteError(SO.Messages.RowError, excelRowNumber, e.Message);
				}
			}
		}

		// Acuminator disable once PX1096 PXOverrideSignatureMismatch [The method is guaranteed by the TGraph restrictions]
		/// Overrides <see cref="PXImportAttribute.IPXProcess.ImportDone(PXImportAttribute.ImportMode.Value)"/>
		[PXOverride]
		public void ImportDone(PXImportAttribute.ImportMode.Value mode,
			Action<PXImportAttribute.ImportMode.Value> base_ImportDone)
		{
			base_ImportDone(mode);

			excelRowNumber = 0;

			if (importHasError)
			{
				importHasError = false;
				throw new Exception(SO.Messages.ImportHasError);
			}
		}
		#endregion

		protected virtual IEnumerable<TEntity> GetEntities()
		{
			var entityCache = Base.Caches<TEntity>();
			return entityCache.Cached.Cast<TEntity>().Where(t => entityCache.GetStatus(t).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)).ToArray();
		}

		protected virtual IEnumerable<TDataType> GetSelectedEntities(TGraphFilter filter)
		{
			var entityCache = Base.Caches<TEntity>();

			TDataType singleEntityID = (TDataType)Base.Caches<TGraphFilter>().GetValue<TGraphFilterEntityID>(filter);
			TDataType[] entityIDs = singleEntityID != null
				? new[] { singleEntityID }
				: GetEntities().Select(e => (TDataType)entityCache.GetValue<TEntityID>(e)).ToArray();

			return entityIDs;
		}
	}
}
