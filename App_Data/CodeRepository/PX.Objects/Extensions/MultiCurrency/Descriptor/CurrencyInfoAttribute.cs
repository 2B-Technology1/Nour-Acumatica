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

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Manages Foreign exchage data for the document or the document detail.
	/// When used for the detail useally a reference to the parent document is passed through ParentCuryInfoID in a constructor.
	/// </summary>
	/// <example>
	/// [CurrencyInfo]  - Document declaration
	/// [CurrencyInfo(typeof(ARRegister.curyInfoID))] - Detail declaration
	/// </example>
	public class CurrencyInfoAttribute : PXDBDefaultAttribute, IPXReportRequiredField, IPXDependsOnFields
	{
		#region State
		protected Dictionary<long, string> _Matches;
		private object _KeyToAbort;
		#endregion
		#region Public Properties
		public virtual bool PopulateParentCuryInfoID { get; set; }

		private bool _Required = false;
		public bool Required {
			get
			{
				return _Required;
			}
			set
			{
				_Required = value;
			}
		}

		#endregion
		#region Ctor
		public CurrencyInfoAttribute()
			: this(typeof(CurrencyInfo.curyInfoID))
		{
			_KeyToAbort = null;
		}

		protected override void EnsureIsRestriction(PXCache sender)
		{
			if (_IsRestriction.Value == null)
			{
				_IsRestriction.Value = true;
			}
		}

		public CurrencyInfoAttribute(Type ParentCuryInfoID)
			: base(ParentCuryInfoID)
		{
		}
		#endregion

		#region Implementation
		public virtual bool IsTopLevel
		{
			get
			{
				return _SourceType == null || typeof(CurrencyInfo).IsAssignableFrom(_SourceType);
			}
		}

		protected virtual void CurrencyInfo_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CurrencyInfo info = (CurrencyInfo)e.Row;
			if (info == null && _Required)
			{
				throw new PXSetPropertyException(Messages.CurrencyShouldBeSpecified);
			}
			long? baseCuryInfoID = CurrencyCollection.MatchBaseCuryInfoId(info);
			if (e.Operation == PXDBOperation.Insert)
			{
				if (baseCuryInfoID != null)
				{
					CurrencyInfo row = (CurrencyInfo)cache.CreateCopy(info);
					EnsureIsRestriction(cache);
					StorePersisted(cache, row);
					row.CuryInfoID = baseCuryInfoID;
					cache.SetStatus(row, PXEntryStatus.Notchanged);
					e.Cancel = true;
				}
				else
				{
					EnsureIsRestriction(cache);
					StorePersisted(cache, e.Row);
				}
			}
			if (info.CuryInfoID == CM.CurrencyCollection.GetBaseCurrency()?.CuryInfoID)
			{
				//Suppress all operations with shared CurrencyInfo
				e.Cancel = true;
			}
		}
		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			long? key = (long?)sender.GetValue(e.Row, FieldName);
			if (IsTopLevel && key != null)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				CurrencyInfo info = (CurrencyInfo)cache.Locate(new CurrencyInfo { CuryInfoID = key });
				PXEntryStatus status = PXEntryStatus.Notchanged;
				this.EnsureIsRestriction(sender);
				if (info != null && ((status = cache.GetStatus(info)) == PXEntryStatus.Inserted))
				{
					if (_IsRestriction.Persisted == null || !_IsRestriction.Persisted.ContainsKey(key))
						cache.PersistInserted(info);
				}
				else if (status == PXEntryStatus.Updated)
				{
					cache.PersistUpdated(info);
				}
				else if (status == PXEntryStatus.Deleted)
				{
					cache.PersistDeleted(info);
				}
			
			}
			if (!IsTopLevel && _IsRestriction.Persisted != null && key != null && _IsRestriction.Persisted.TryGetValue(key, out var parent))
			{
				_KeyToAbort = key;
				CurrencyInfo info = parent as CurrencyInfo;
				if (info == null) key = sender.Graph.Caches[parent.GetType()].GetValue(parent, _SourceField) as long?;
				else key = info.CuryInfoID;

				sender.SetValue(e.Row, _FieldOrdinal, key);
			}

			if (key == null || key < 0)
				base.RowPersisting(sender, e);
		}

		public override void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _SourceField ?? _FieldName);
			if (_IsRestriction.Persisted != null
				&& key != null
			    && _IsRestriction.Persisted.TryGetValue(key, out var persisted)
				&& persisted.GetType() == _SourceType)
					return;
			
			base.SourceRowPersisting(sender, e);
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			long? key = (long?)sender.GetValue(e.Row, _FieldOrdinal);

			if (e.TranStatus == PXTranStatus.Open && key < 1 && sender.GetStatus(e.Row).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
			{
				// Acuminator disable once PX1073 ExceptionsInRowPersisted [Exception throw in transaction scope]
				throw new PXException(Messages.CurrencyInfoNotSaved, sender.GetItemType().Name);
			}

			if (IsTopLevel && e.TranStatus != PXTranStatus.Open)
			{
				PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];
				cache.Persisted(e.TranStatus == PXTranStatus.Aborted);
			}

			if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort == null && e.Operation == PXDBOperation.Update)
			{
				if (_IsRestriction.Persisted != null
				&& _IsRestriction.Persisted.TryGetValue(key, out var persisted))
				{
					_KeyToAbort = (persisted as CurrencyInfo)?.CuryInfoID;
				}
			}

			if (e.TranStatus == PXTranStatus.Aborted && _KeyToAbort != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
			}
			else base.RowPersisted(sender, e);
		}

		protected virtual void RowSelectingCollectMatches(PXCache sender, PXRowSelectingEventArgs e)
		{
			long? id = (long?)sender.GetValue(e.Row, _FieldOrdinal);
			if (id != null)
			{
				string cury = (string)sender.GetValue(e.Row, "CuryID");
				if (!String.IsNullOrEmpty(cury))
				{
					_Matches[(long)id] = cury;
				}
			}
		}

		public ISet<Type> GetDependencies(PXCache cache)
		{
			var res = new HashSet<Type>();
			var field = cache.GetBqlField("CuryID");
			if (field != null) res.Add(field);
			return res;
		}
		private static Type GetPrimaryType(PXGraph graph)
		{
			foreach (DictionaryEntry action in graph.Actions)
			{
				try
				{
					Type primary;
					if ((primary = ((PXAction)action.Value).GetRowType()) != null)
						return primary;
				}
				catch (Exception)
				{
				}
			}
			return null;
		}
		#endregion

		#region Runtime
		public static CurrencyInfo GetCurrencyInfo<Field>(PXCache sender, object row)
			where Field : IBqlField
		{
			return GetCurrencyInfo(sender, typeof(Field), row);
		}
		public static CurrencyInfo GetCurrencyInfo(PXCache sender, Type field, object row)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(row, field.Name))
			{
				if (attr is CurrencyInfoAttribute curyAttr)
				{
					return curyAttr.GetCurrencyInfo(sender, row);
				}
			}
			return null;
		}
		protected virtual CurrencyInfo GetCurrencyInfo(PXCache sender, object row)
		{
			long? key = (long?)sender.GetValue(row, _FieldOrdinal);
			PXCache cache = sender.Graph.Caches[typeof(CurrencyInfo)];

			var result = new CurrencyInfo { CuryInfoID = key };
			object parent;
			if (_SourceType != null && _IsRestriction.Persisted != null && _IsRestriction.Persisted.TryGetValue(key, out parent))
			{
				long? ret = sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName) as long?;
				if (ret != null && ret.Value > 0L)
				{
					result = new CurrencyInfo {CuryInfoID = ret};
				}
			}

			result = (CurrencyInfo)cache.Locate(result);
			return result;
		}
		/// <exclude/>
		public static long? GetPersistedCuryInfoID(PXCache sender, long? curyInfoID)
		{
			if (curyInfoID == null || curyInfoID.Value > 0L) return curyInfoID;

			long[] founds = sender.GetAttributesReadonly(null)
				.OfType<CurrencyInfoAttribute>()
				.Select(cattr => cattr?.TryGetPersistedID(sender, curyInfoID))
				.Where(_ => _.HasValue)
				.Cast<long>()
				.ToArray();
			if (founds.Any()) return founds.Distinct().Single();
			else return curyInfoID;
		}

		private long? TryGetPersistedID(PXCache sender, long? curyInfoID)
		{
			if (_SourceType == null || _IsRestriction.Persisted == null) return null;
			if (!_IsRestriction.Persisted.TryGetValue(curyInfoID, out object parent)) return null;

			long? idOfPersistedCury = (parent as CurrencyInfo)?.CuryInfoID;
			if (idOfPersistedCury != null && idOfPersistedCury > 0L) return idOfPersistedCury;

			long? ret = sender.Graph.Caches[_SourceType].GetValue(parent, _SourceField ?? _FieldName) as long?;
			if (ret != null && ret.Value > 0L) return ret;
			else return null;
		}

		#endregion

		/// <summary>
		/// Verifies if field is not is one of which changes on changing Cury/Base view state
		/// </summary>
		/// <param name="fieldName">field name to verify</param>
		/// <returns></returns>
		internal static bool IsDifferenceEssential(string fieldName)
		{
			if (fieldName == nameof(Objects.Extensions.MultiCurrency.Document.Base)) return false;
			if (fieldName == nameof(AccessInfo.CuryViewState)) return false;
			if (fieldName == nameof(AP.APRegister.LastModifiedByScreenID)) return false;
			if (fieldName == nameof(CR.CROpportunity.RLastModifiedByScreenID)) return false;

			return true;
		}

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			sender.Graph.RowPersisting.AddHandler<CurrencyInfo>(CurrencyInfo_RowPersisting);

			base.CacheAttached(sender);
			if (sender.GetBqlField("CuryID") != null
				&& (_Matches = CurrencyInfo.CuryIDStringAttribute.GetMatchesDictionary(sender)) != null)
			{
				sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), RowSelectingCollectMatches);
			}

			Type sourceType = _SourceType == typeof(CurrencyInfo)
				? GetPrimaryType(sender.Graph)
				: _SourceType;
			if (sourceType != null)
			{
				Type cacheType = sender.Graph.Caches[sourceType].GetItemType();

				var parent = sender
					.GetAttributesReadonly(null)
					.OfType<PXParentAttribute>()
					.FirstOrDefault(p => p.ParentType == sourceType
					                     || p.ParentType == cacheType
					                     || sourceType.IsSubclassOf(p.ParentType));
				if (parent != null)
				{
					Type parentType = parent.ParentType;
					sender.Graph.FieldUpdated.AddHandler(parentType, _SourceField,
						(PXCache cache, PXFieldUpdatedEventArgs e) =>
						{
							long? newCuriInfoID = (long?)cache.GetValue(e.Row, _SourceField);
							long? oldCuriInfoID = (long?)e.OldValue;
							bool populateFromParent = PopulateParentCuryInfoID == true && oldCuriInfoID == null && newCuriInfoID != null;
							if (newCuriInfoID != oldCuriInfoID && (oldCuriInfoID != null || populateFromParent))
							{
								foreach (object item in PXParentAttribute.SelectSiblings(sender, null, parentType))
								{
									object updated = sender.Graph.Caches[item.GetType()].Locate(item) ?? item;
									long? curiInfoID = (long?)sender.GetValue(updated, FieldName);
									if (oldCuriInfoID != curiInfoID)
										continue;
									sender.SetValueExt(updated, _FieldName, newCuriInfoID);
									sender.MarkUpdated(updated);
								}
							}
						});
				}
			}

			//else
			//{
			//	throw new PXException("The PXParentAttribute is not defined on {0} with reference {1}",
			//	sender.GetItemType().Name,
			//	_SourceType);
			//}
			sender.Graph.OnAfterPersist += delegate (PXGraph graph)
			{
				PXCache cache = graph.Caches[typeof(CurrencyInfo)];
				foreach (CurrencyInfo info in cache.Inserted.ToArray<CurrencyInfo>())
				{
					if (info.BaseCuryID == info.CuryID && info.CuryInfoID < 0)
					{
						info.CuryInfoID = CurrencyCollection.GetCurrency(info.BaseCuryID).CuryInfoID;
						cache.SetStatus(info, PXEntryStatus.Notchanged);
					}
				}
			};
		}
		#endregion
	}
}
