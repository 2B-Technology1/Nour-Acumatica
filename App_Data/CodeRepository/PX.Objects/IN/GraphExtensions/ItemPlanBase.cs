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
using System.Linq;
using PX.Common;
using PX.Data;

namespace PX.Objects.IN.GraphExtensions
{
	public abstract class ItemPlanBase<TGraph, TItemPlanSource> : ItemPlanHelper<TGraph>
		where TGraph : PXGraph
		where TItemPlanSource : class, IItemPlanSource, IBqlTable, new()
	{
		#region Cache Helpers
		#region Item
		private PXCache<TItemPlanSource> _itemPlanSourceCache;
		public PXCache<TItemPlanSource> ItemPlanSourceCache => _itemPlanSourceCache ??= Base.Caches<TItemPlanSource>();
		#endregion
		#region INItemPlan
		private PXCache<INItemPlan> _planCache;
		public PXCache<INItemPlan> PlanCache => _planCache ??= Base.Caches<INItemPlan>();
		#endregion
		#endregion

		#region State

		protected long? _selfKeyToAbort;
		protected bool PersistedToAbortLocked
		{
			get
			{
				return PXContext.GetSlot<bool>($"{nameof(ItemPlanBase<TGraph, TItemPlanSource>)}.{nameof(PersistedToAbortLocked)}");
			}
			set
			{
				PXContext.SetSlot<bool>($"{nameof(ItemPlanBase<TGraph, TItemPlanSource>)}.{nameof(PersistedToAbortLocked)}", value);
			}
		}
		protected Dictionary<long?, long?> _persistedToAbort;
		protected Dictionary<long?, TItemPlanSource> _inserted;
		protected Dictionary<long?, TItemPlanSource> _updated;
		protected Dictionary<long?, List<INItemPlan>> _selfInserted;
		protected Dictionary<long?, List<INItemPlan>> _selfUpdated;
		#endregion

		#region Initialization

		public override void Initialize()
		{
			base.Initialize();

			_persistedToAbort = new Dictionary<long?, long?>();

			if (!Base.Views.RestorableCaches.Contains(typeof(INItemPlan)))
				Base.Views.RestorableCaches.Add(typeof(INItemPlan));

			Base.OnBeforeCommit += VerifyUnsavedPlans;
		}

		#endregion

		#region Event Handlers

		#region TItemPlanSource

		public virtual void _(Events.RowPersisting<TItemPlanSource> e)
		{
			if (e.Row.PlanID != null)
			{
				if (e.Row.PlanID < 0L)
				{
					bool isPlanFound = false;
					foreach (INItemPlan data in PlanCache.Inserted)
					{
						if (e.Row.PlanID == data.PlanID)
						{
							isPlanFound = true;
							try
							{
								PlanCache.PersistInserted(data);
							}
							catch (PXOuterException ex)
							{
								for (int i = 0; i < ex.InnerFields.Length; i++)
								{
									if (e.Cache.RaiseExceptionHandling(ex.InnerFields[i], e.Row, null, new PXSetPropertyKeepPreviousException(ex.InnerMessages[i])))
									{
										throw new PXRowPersistingException(ex.InnerFields[i], null, ex.InnerMessages[i]);
									}
								}
								return;
							}
							long id = Convert.ToInt64(PXDatabase.SelectIdentity());
							e.Row.PlanID = id;
							data.PlanID = id;
							PlanCache.Normalize();
							break;
						}
					}
					if (!isPlanFound && e.Cache.GetStatus(e.Row).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
						throw new PXException(Messages.InvalidPlan);
				}
				else
				{
					foreach (INItemPlan data in PlanCache.Updated)
					{
						if (e.Row.PlanID == data.PlanID)
						{
							PlanCache.PersistUpdated(data);
							break;
						}
					}
				}
			}
		}

		public virtual void _(Events.RowPersisted<TItemPlanSource> e)
		{
			if (e.TranStatus == PXTranStatus.Aborted)
			{
				if (e.Row.PlanID != null && _persistedToAbort.TryGetValue(e.Row.PlanID, out long? keyToAbort))
				{
					long? key = e.Row.PlanID;
					e.Row.PlanID = keyToAbort;
					foreach (INItemPlan data in PlanCache.Inserted)
					{
						if (key == data.PlanID)
						{
							try
							{
								PersistedToAbortLocked = true;
								PlanCache.RaiseRowPersisted(data, PXDBOperation.Insert, e.TranStatus, e.Args.Exception);
							}
							finally
							{
								PersistedToAbortLocked = false;
							}
							//the code below is most likely not needed because identity revert is performed by PXLongIdentity.RowPersisted which is now called above
							data.PlanID = keyToAbort;
							break;
						}
					}
				}
				else
				{
					foreach (INItemPlan data in PlanCache.Updated)
					{
						if (e.Row.PlanID == data.PlanID)
						{
							try
							{
								PersistedToAbortLocked = true;
								PlanCache.RaiseRowPersisted(data, PXDBOperation.Update, e.TranStatus, e.Args.Exception);
							}
							finally
							{
								PersistedToAbortLocked = false;
							}
							PlanCache.ResetPersisted(data);
						}
					}
				}
				PlanCache.Normalize();
			}
			else if (e.TranStatus == PXTranStatus.Completed)
			{
				foreach (INItemPlan data in PlanCache.Inserted)
				{
					if (e.Row.PlanID == data.PlanID)
					{
						PlanCache.RaiseRowPersisted(data, PXDBOperation.Insert, e.TranStatus, e.Args.Exception);
						PlanCache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(PlanCache, data, Base.TimeStamp);
						PlanCache.ResetPersisted(data);
					}
				}
				foreach (INItemPlan data in PlanCache.Updated)
				{
					if (e.Row.PlanID == data.PlanID)
					{
						PlanCache.RaiseRowPersisted(data, PXDBOperation.Update, e.TranStatus, e.Args.Exception);
						PlanCache.SetStatus(data, PXEntryStatus.Notchanged);
						PXTimeStampScope.PutPersisted(PlanCache, data, Base.TimeStamp);
						PlanCache.ResetPersisted(data);
					}
				}
				PlanCache.Normalize();
			}
		}

		#endregion

		#region INItemPlan

		public virtual void _(Events.RowPersisting<INItemPlan> e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.Row.IsTempLotSerial == true)
				{
					e.Cancel = true;
					return;
				}

				_selfKeyToAbort = e.Row.PlanID;
			}
		}

		protected virtual void _(Events.RowPersisted<INItemPlan> e)
		{
			PlanForSupplyRowPersisted(e.Row, e.Operation, e.TranStatus);
			PlanForItemRowPersisted(e.Row, e.Operation, e.TranStatus);
			if (e.TranStatus == PXTranStatus.Open)
			{
				_selfKeyToAbort = null;
			}
		}

		#endregion

		#endregion

		#region OnBeforeCommit

		protected virtual void VerifyUnsavedPlans(PXGraph graph)
		{
			if (graph.Caches<INItemPlan>()
				.Inserted
				.Cast<INItemPlan>()
				.Any(p => p.IsTempLotSerial == true))
			{
				throw new PXException(Messages.InvalidPlan);
			}
		}

		#endregion

		#region Sync from persisted INItemPlan.PlanID to other DACs

		protected virtual void PlanForItemRowPersisted(INItemPlan plan, PXDBOperation operation, PXTranStatus tranStatus)
		{
			if (operation == PXDBOperation.Insert)
			{
				if (tranStatus == PXTranStatus.Open && _selfKeyToAbort != null)
				{
					if (!_persistedToAbort.ContainsKey(plan.PlanID))
					{
						_persistedToAbort.Add(plan.PlanID, _selfKeyToAbort);
					}

					_inserted ??= ItemPlanSourceCache.Inserted.Cast<TItemPlanSource>()
						.Where(item => item.PlanID != null)
						.ToDictionary(item => item.PlanID);

					if (_inserted.TryGetValue(_selfKeyToAbort, out TItemPlanSource row))
					{
						row.PlanID = plan.PlanID;
					}

					_updated ??= ItemPlanSourceCache.Updated.Cast<TItemPlanSource>()
						.Where(item => item.PlanID != null)
						.ToDictionary(item => item.PlanID);

					if (_updated.TryGetValue(_selfKeyToAbort, out row))
					{
						row.PlanID = plan.PlanID;
					}
				}

				//this code needs to be executed only in case INItemPlan cache was persisted before TItemPlanSource
				if (tranStatus == PXTranStatus.Aborted && PersistedToAbortLocked == false)
				{
					foreach (TItemPlanSource item in ItemPlanSourceCache.Inserted.Concat_(ItemPlanSourceCache.Updated))
					{
						if (item.PlanID != null && _persistedToAbort.TryGetValue(item.PlanID, out long? selfKeyToAbort))
						{
							item.PlanID = selfKeyToAbort;
						}
					}
				}

				if (tranStatus.IsIn(PXTranStatus.Completed, PXTranStatus.Aborted))
				{
					_inserted = null;
					_updated = null;
				}
			}
		}

		protected virtual void PlanForSupplyRowPersisted(INItemPlan plan, PXDBOperation operation, PXTranStatus tranStatus)
		{
			if ((operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				List<INItemPlan> references;
				if (tranStatus == PXTranStatus.Open && _selfKeyToAbort != null)
				{
					if (_selfInserted == null)
					{
						_selfInserted = new Dictionary<long?, List<INItemPlan>>();
						foreach (INItemPlan item in PlanCache.Inserted)
						{
							if (item.SupplyPlanID != null)
							{
								if (!_selfInserted.TryGetValue(item.SupplyPlanID, out references))
								{
									_selfInserted[item.SupplyPlanID] = references = new List<INItemPlan>();
								}
								references.Add(item);
							}
						}
					}

					if (_selfInserted.TryGetValue(_selfKeyToAbort, out references))
					{
						foreach (INItemPlan item in references)
						{
							item.SupplyPlanID = plan.PlanID;
						}
					}

					if (_selfUpdated == null)
					{
						_selfUpdated = new Dictionary<long?, List<INItemPlan>>();
						foreach (INItemPlan item in PlanCache.Updated)
						{
							if (item.SupplyPlanID != null)
							{
								if (!_selfUpdated.TryGetValue(item.SupplyPlanID, out references))
								{
									_selfUpdated[item.SupplyPlanID] = references = new List<INItemPlan>();
								}
								references.Add(item);
							}
						}
					}

					if (_selfUpdated.TryGetValue(_selfKeyToAbort, out references))
					{
						foreach (INItemPlan item in references)
						{
							item.SupplyPlanID = plan.PlanID;
						}
					}
				}
				else if (tranStatus == PXTranStatus.Aborted)
				{
					foreach (INItemPlan poplan in PlanCache.Inserted.Concat_(PlanCache.Updated))
					{
						if (poplan.SupplyPlanID != null && _persistedToAbort.TryGetValue(poplan.SupplyPlanID, out long? selfKeyToAbort))
						{
							poplan.SupplyPlanID = selfKeyToAbort;
						}
					}
				}
			}

			if (tranStatus.IsIn(PXTranStatus.Completed, PXTranStatus.Aborted))
			{
				_selfInserted = null;
				_selfUpdated = null;
			}
		}

		#endregion
	}
}
