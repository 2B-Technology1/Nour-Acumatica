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
using PX.Api;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.IN;
using PX.DbServices.QueryObjectModel;

namespace PX.Objects.SO
{
	public class ManageSalesAllocations : PXGraph<ManageSalesAllocations>, PXImportAttribute.IPXPrepareItems, PXImportAttribute.IPXProcess
	{
		private const string MultiSelectSeparator = ",";

		#region Actions

		public PXCancel<SalesAllocationsFilter> Cancel;

		#endregion

		#region Views

		public PXFilter<SalesAllocationsFilter> Filter;

		[PXFilterable]
		public AllocationsProcessing Allocations;

		public class AllocationsProcessing : PXFilteredProcessing<SalesAllocation, SalesAllocationsFilter>
		{
			public Func<SalesAllocation, bool> SkipProcessing;

			public AllocationsProcessing(PXGraph graph) : base(graph) { }

			public AllocationsProcessing(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override bool startPendingProcess(List<SalesAllocation> items)
			{
				if (SkipProcessing != null)//exclude only when ProcessAll
					items = items.Where(x => !SkipProcessing(x)).ToList();

				return base.startPendingProcess(items);
			}
		}

		#endregion

		#region Initialization

		public ManageSalesAllocations()
		{
			{
				var setup = new PXSetup<SOSetup>(this).Current;
			}

			Allocations.SuppressUpdate = true;
			Allocations.Cache.DisableReadItem = true;

			PXUIFieldAttribute.SetEnabled<SalesAllocation.qtyToAllocate>(Allocations.Cache, null);
			PXUIFieldAttribute.SetEnabled<SalesAllocation.qtyToDeallocate>(Allocations.Cache, null);

			SetOrderTypeList();
		}

		#endregion

		#region Allocations loading

		protected virtual IEnumerable allocations()
		{
			var filter = Filter.Current;
			if (!AllowProcess(filter))
				return Array<SalesAllocation>.Empty;

			var maxRows = PXView.MaximumRows;
			if (maxRows == 1)
			{
				var allocation = Allocations.Cache.CreateInstance<SalesAllocation>(PXView.SortColumns, PXView.Searches);
				allocation = (SalesAllocation)Allocations.Cache.Locate(allocation);
				if (allocation != null)
					return new List<SalesAllocation> { allocation };
			}
			else
			{
				Allocations.Cache.UnHoldRows(Allocations.Cache.Cached.OfType<SalesAllocation>());
			}
			var isAllocation = filter.Action == SalesAllocationsFilter.action.AllocateSalesOrders;
			var viewSettings = ViewSettings.FromCurrentContext();
			viewSettings.FieldsMap
				.Add(nameof(SalesAllocation.inventoryID), nameof(SalesAllocation.inventoryCD));

			if (isAllocation)
				viewSettings.SelectFromFirstPage();

			var rows = LoadAllocations(filter, viewSettings).ToList();
			PXView.StartRow = 0;

			if (rows.Any() && maxRows != 1)
			{
				if(isAllocation)
					CalculateQtyToAllocate(rows);
				else
					CalculateQtyToDeallocate(rows);
			}

			rows = isAllocation
				? viewSettings.GetCurrentPageRange(rows)
				: rows;

			Allocations.Cache.HoldRows(rows.RowCast<SalesAllocation>());

			var result = new PXDelegateResult
			{
				IsResultFiltered = true,
				IsResultSorted = true,
				IsResultTruncated = true
			};
			result.AddRange(rows);
			return result;
		}

		protected virtual IEnumerable<PXResult<SalesAllocation>> LoadAllocations(SalesAllocationsFilter filter, ViewSettings viewSettings)
		{
			var parameters = new List<object>();
			var query = CreateQuery(filter, parameters);
			parameters.AddRange(viewSettings.Parameters);
			viewSettings.Parameters = parameters;

			viewSettings.ApplySortingFrom(query);

			foreach(object row in query.View.Select(viewSettings))
			{
				var result = row as PXResult<SalesAllocation>;
				if (result == null)
					result = new PXResult<SalesAllocation>((SalesAllocation)row);
				yield return result;
			}
		}

		protected virtual PXSelectBase<SalesAllocation> CreateQuery(SalesAllocationsFilter filter, List<object> parameters)
		{
			PXSelectBase<SalesAllocation> query = null;
			switch (filter.Action)
			{
				case SalesAllocationsFilter.action.AllocateSalesOrders:
					query = CreateAllocateQuery(filter, parameters);
					break;
				case SalesAllocationsFilter.action.DeallocateSalesOrders:
					query = CreateDeallocateQuery(filter, parameters);
					break;
			}
			return query;
		}

		protected virtual PXSelectBase<SalesAllocation> CreateBaseQuery(SalesAllocationsFilter filter, List<object> parameters)
		{
			void push(object value) => parameters.Add(value);

			var query = new SelectFrom<SalesAllocation>
				.OrderBy<SalesAllocation.orderPriority.Asc>
				.View(this);

			SOSetup soSetup = SelectFrom<SOSetup>.View.ReadOnly.Select(this);
			if (soSetup?.OrderRequestApproval == true)
			{
				var orderTypes = SOOrderTypeApproval.GetOrderTypes();
				if (orderTypes.Length > 0)
				{
					query.WhereAnd<Where<SalesAllocation.orderHold.IsEqual<True>
						.Or<SalesAllocation.orderType.IsNotIn<@P.AsString.ASCII>>>>();
					push(orderTypes);
				}
			}

			if (filter.SelectBy != null)
			{
				void addFilter<TDate>()
					where TDate : BqlDateTime.Field<TDate>
				{
					if (filter.StartDate != null)
					{
						query.WhereAnd<Where<Use<TDate>.AsDateTime.IsGreaterEqual<@P.AsDateTime>>>();
						push(filter.StartDate);
					}
					query.WhereAnd<Where<Use<TDate>.AsDateTime.IsLessEqual<@P.AsDateTime>>>();
					push(filter.EndDate);
				}

				switch (filter.SelectBy)
				{
					case SalesAllocationsFilter.selectBy.OrderDate:
						addFilter<SalesAllocation.orderDate>();
						break;
					case SalesAllocationsFilter.selectBy.LineRequestedOn:
						addFilter<SalesAllocation.requestDate>();
						break;
					case SalesAllocationsFilter.selectBy.LineShipOn:
						addFilter<SalesAllocation.shipDate>();
						break;
					case SalesAllocationsFilter.selectBy.CancelBy:
						addFilter<SalesAllocation.cancelDate>();
						break;
				}
			}

			if (!string.IsNullOrEmpty(filter.OrderType))
			{
				var orderTypes = filter.OrderType.Split(new[] { MultiSelectSeparator }, StringSplitOptions.RemoveEmptyEntries);
				query.WhereAnd<Where<SalesAllocation.orderType.IsIn<@P.AsString.ASCII>>>();
				push(orderTypes);
			}

			if (!string.IsNullOrEmpty(filter.OrderStatus))
			{
				var orderStatuses = filter.OrderStatus.Split(new[] { MultiSelectSeparator }, StringSplitOptions.RemoveEmptyEntries);
				query.WhereAnd<Where<SalesAllocation.orderStatus.IsIn<@P.AsString.ASCII>>>();
				push(orderStatuses);
			}

			if (filter.OrderNbr != null)
			{
				query.WhereAnd<Where<SalesAllocation.orderNbr.IsEqual<@P.AsString>>>();
				push(filter.OrderNbr);
			}

			if (filter.Priority != null)
			{
				query.WhereAnd<Where<SalesAllocation.orderPriority.IsEqual<@P.AsInt>>>();
				push(filter.Priority);
			}

			if (filter.CustomerClassID != null)
			{
				query.WhereAnd<Where<SalesAllocation.customerClassID.IsEqual<@P.AsString>>>();
				push(filter.CustomerClassID);
			}

			if (filter.CustomerID != null)
			{
				query.WhereAnd<Where<SalesAllocation.customerID.IsEqual<@P.AsInt>>>();
				push(filter.CustomerID);
			}

			int[] inventories = filter.InventoryID != null
				? new[] { filter.InventoryID.Value }
				: SelectedItems.SelectMain().Select(l => l.InventoryID.Value).ToArray();

			if (inventories.Length > 0)
			{
				query.WhereAnd<Where<SalesAllocation.inventoryID.IsIn<@P.AsInt>>>();
				parameters.Add(inventories);
			}

			return query;
		}

		protected virtual PXSelectBase<SalesAllocation> CreateAllocateQuery(SalesAllocationsFilter filter, List<object> parameters)
		{
			var query = CreateBaseQuery(filter, parameters);

			query.Join<InnerJoin<INSiteStatusByCostCenterShort,
					On<SalesAllocation.FK.SiteStatusByCostCenterShort>>>();

			query.WhereAnd<Where<
				SalesAllocation.qtyUnallocated.IsGreater<CS.decimal0>
				.And<INSiteStatusByCostCenterShort.qtyHardAvail.IsGreater<CS.decimal0>>
				.And<SalesAllocation.lineSiteID.IsEqual<@P.AsInt>>>>();
			parameters.Add(filter.SiteID);

			if (string.IsNullOrEmpty(filter.OrderStatus))
				query.WhereAnd<Where<SalesAllocation.orderStatus.IsInSequence<SalesAllocationsFilter.orderStatus.list>>>();

			AddAllocateSorting(query, filter);

			return query;
		}

		protected virtual PXSelectBase<SalesAllocation> CreateDeallocateQuery(SalesAllocationsFilter filter, List<object> parameters)
		{
			var query = CreateBaseQuery(filter, parameters);

			query.WhereAnd<Where<
				SalesAllocation.qtyAllocated.IsGreater<CS.decimal0>>>();

			if (string.IsNullOrEmpty(filter.OrderStatus))
				query.WhereAnd<Where<SalesAllocation.orderStatus.IsInSequence<SalesAllocationsFilter.orderStatus.list.withExpired>>>();

			AddDeallocateSorting(query, filter);

			return query;
		}

		protected virtual void AddAllocateSorting(PXSelectBase<SalesAllocation> query, SalesAllocationsFilter filter)
		{
			var sortFields = new List<IBqlSortColumn>();
			void addSort<TSortField>() where TSortField : IBqlSortColumn, new()
				=> sortFields.Add(new TSortField());

			void sort<TField>(bool asc = true) where TField : IBqlField
			{
				if (asc)
					addSort<Asc<TField>>();
				else
					addSort<Desc<TField>>();
			}

			sort<SalesAllocation.orderPriority>();

			if (filter.SelectBy != null)
			{
				switch (filter.SelectBy)
				{
					case SalesAllocationsFilter.selectBy.OrderDate:
						sort<SalesAllocation.orderDate>();
						break;
					case SalesAllocationsFilter.selectBy.LineRequestedOn:
						sort<SalesAllocation.requestDate>();
						break;
					case SalesAllocationsFilter.selectBy.LineShipOn:
						sort<SalesAllocation.shipDate>();
						break;
					case SalesAllocationsFilter.selectBy.CancelBy:
						sort<SalesAllocation.cancelDate>();
						break;
				}
			}

			sort<SalesAllocation.orderCreatedOn>();
			sort<SalesAllocation.orderType>();
			sort<SalesAllocation.orderNbr>();
			sort<SalesAllocation.inventoryCD>();
			sort<SalesAllocation.lineNbr>();

			query.OrderByNew(sortFields);
		}

		protected virtual void AddDeallocateSorting(PXSelectBase<SalesAllocation> query, SalesAllocationsFilter filter)
		{
			var sortFields = new List<IBqlSortColumn>();
			void addSort<TSortField>() where TSortField : IBqlSortColumn, new()
				=> sortFields.Add(new TSortField());

			void sort<TField>(bool asc = true) where TField : IBqlField
			{
				if (asc)
					addSort<Asc<TField>>();
				else
					addSort<Desc<TField>>();
			}

			sort<SalesAllocation.orderPriority>(false);

			if (filter.SelectBy != null)
			{
				switch (filter.SelectBy)
				{
					case SalesAllocationsFilter.selectBy.OrderDate:
						sort<SalesAllocation.orderDate>(false);
						break;
					case SalesAllocationsFilter.selectBy.LineRequestedOn:
						sort<SalesAllocation.requestDate>(false);
						break;
					case SalesAllocationsFilter.selectBy.LineShipOn:
						sort<SalesAllocation.shipDate>(false);
						break;
					case SalesAllocationsFilter.selectBy.CancelBy:
						sort<SalesAllocation.cancelDate>(false);
						break;
				}
			}

			sort<SalesAllocation.orderCreatedOn>(false);
			sort<SalesAllocation.orderType>();
			sort<SalesAllocation.orderNbr>();
			sort<SalesAllocation.inventoryCD>();
			sort<SalesAllocation.lineNbr>();

			query.OrderByNew(sortFields);
		}

		#endregion

		#region Event handlers

		#region SalesAllocationsFilter event handlers

		protected virtual void _(Events.RowSelected<SalesAllocationsFilter> e)
		{
			var filter = e.Row;
			if (filter == null)
				return;

			SetOrderStatusList(filter);

			SetFieldsVisibility(filter);

			var allowProcess = AllowProcess(filter);

			Allocations.SetProcessEnabled(allowProcess);
			Allocations.SetProcessAllEnabled(allowProcess);

			if (allowProcess)
				SetProcessDelegate(filter);
		}

		protected virtual void _(Events.RowUpdated<SalesAllocationsFilter> e)
		{
			if (!e.Cache.ObjectsEqual<SalesAllocationsFilter.action, SalesAllocationsFilter.siteID>(e.OldRow, e.Row))
				ResetAllocations();

			if (!e.Cache.ObjectsEqual<SalesAllocationsFilter.action>(e.OldRow, e.Row))
				NormalizeOrderStatusValue(e.Row);
		}

		#endregion

		#region SalesAllocation event handlers

		protected virtual void _(Events.RowSelected<SalesAllocation> e)
		{
			if (e.Row == null)
				return;

			if (Filter.Current.Action == SalesAllocationsFilter.action.AllocateSalesOrders)
			{
				var error = PXUIFieldAttribute.GetErrorOnly<SalesAllocation.qtyToAllocate>(Allocations.Cache, e.Row);
				if (string.IsNullOrEmpty(error))
					ShowAllocationError(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<SalesAllocation, SalesAllocation.qtyToAllocate> e)
		{
			if (e.ExternalCall)
			{
				if(e.Row.Selected != true)
					e.Cache.SetValueExt<SalesAllocation.selected>(e.Row, true);
			}
		}

		protected virtual void _(Events.FieldUpdated<SalesAllocation, SalesAllocation.qtyToDeallocate> e)
		{
			if (e.ExternalCall)
			{
				if (e.Row.Selected != true)
					e.Cache.SetValueExt<SalesAllocation.selected>(e.Row, true);
			}
		}

		protected virtual void _(Events.RowUpdated<SalesAllocation> e)
		{
			if (e.ExternalCall)
			{
				if (e.OldRow.Selected == true && e.Row.Selected != true)
				{
					e.Cache.SetStatus(e.Row, PXEntryStatus.Held);

					if (Filter.Current.Action == SalesAllocationsFilter.action.AllocateSalesOrders)
						Allocations.View.RequestRefresh();
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<SalesAllocation, SalesAllocation.qtyToDeallocate> e)
		{
			if (e.ExternalCall)
			{
				var newValue = (decimal?)e.NewValue;
				var ex = GetDeallocationError(e.Row, newValue);
				if (ex != null)
				{
					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs ErrorValue is not DAC field
					ex.ErrorValue = e.NewValue;
					throw ex;
				}
				if (decimal.Remainder(newValue ?? 0, 1m) > 0m)
				{
					var inventoryItem = InventoryItem.PK.Find(this, e.Row.InventoryID);
					var lotSerClass = INLotSerClass.PK.Find(this, inventoryItem?.LotSerClassID);
					if (lotSerClass?.LotSerTrack == INLotSerTrack.SerialNumbered)
						e.NewValue = Math.Truncate(newValue ?? 0);
				}
			}
		}

		#endregion

		#endregion

		#region Another implementations

		protected virtual bool AllowProcess(SalesAllocationsFilter filter)
			=> filter != null
				&& filter.Action != null
				&& filter.Action != SalesAllocationsFilter.action.None
				&& filter.SelectBy != null
				&& filter.EndDate != null
				&& filter.SiteID != null;

		protected virtual void CalculateQtyToAllocate(List<PXResult<SalesAllocation>> rows)
		{
			foreach (var row in rows)
			{
				var allocation = (SalesAllocation)row;
				var status = row.GetItem<INSiteStatusByCostCenterShort>();
				if (status != null)
					allocation.QtyHardAvail = status.QtyHardAvail;
			}
		}

		protected virtual void CalculateQtyToDeallocate(List<PXResult<SalesAllocation>> rows)
		{
			foreach (SalesAllocation row in rows)
			{
				if(row.Selected != true)
					row.QtyToDeallocate = row.QtyAllocated;
			}
		}

		protected virtual bool ShowAllocationError(SalesAllocation row)
		{
			var ex = GetAllocationError(row);
			Allocations.Cache.RaiseExceptionHandling<SalesAllocation.qtyToAllocate>(row, row.QtyToAllocate, ex);
			return ex != null;
		}

		protected virtual PXSetPropertyException GetAllocationError(SalesAllocation row) => GetAllocationError(row, row.QtyToAllocate);

		protected virtual PXSetPropertyException GetAllocationError(SalesAllocation row, decimal? qtyToAllocate)
		{
			if (qtyToAllocate < 0)
				return new PXSetPropertyException(CS.Messages.FieldShouldNotBeNegative,
					PXUIFieldAttribute.GetDisplayName<SalesAllocation.qtyToAllocate>(Allocations.Cache));

			if (qtyToAllocate > row.QtyUnallocated)
				return new PXSetPropertyException(Messages.QtyToAllocateGreaterQtyUnallocated);

			return null;
		}

		protected virtual PXSetPropertyException GetDeallocationError(SalesAllocation row, decimal? qtyToDeallocate)
		{
			if (qtyToDeallocate < 0)
				return new PXSetPropertyException(CS.Messages.FieldShouldNotBeNegative,
					PXUIFieldAttribute.GetDisplayName<SalesAllocation.qtyToDeallocate>(Allocations.Cache));

			if (qtyToDeallocate > row.QtyAllocated)
				return new PXSetPropertyException(Messages.QtyToDeallocateGreaterQtyAllocated);

			if (row.OrderStatus == SOOrderStatus.Expired && qtyToDeallocate < row.QtyAllocated)
				return new PXSetPropertyException(Messages.CantEditQtyToDeallocateForExpiredOrderLine);

			if (qtyToDeallocate < row.QtyAllocated)
			{
				var unassignedLotQty = row.QtyAllocated - row.LotSerialQtyAllocated;
				if (unassignedLotQty < qtyToDeallocate)
					throw new PXSetPropertyException(Messages.CantDeallocateLotSerialNumber);
			}

			return null;
		}

		protected virtual void ResetAllocations()
		{
			Allocations.Cache.Clear();
			Allocations.Cache.ClearQueryCache();
		}

		protected virtual void SetProcessDelegate(SalesAllocationsFilter filter)
		{
			PXProcessingBase<SalesAllocation>.ProcessListDelegate handler = null;
			Func<SalesAllocation, bool> skipProcessingHandler = null;
			switch (filter.Action)
			{
				case SalesAllocationsFilter.action.AllocateSalesOrders:
					handler = (list) => CreateInstance<SalesAllocationProcess>().AllocateOrders(list);
					skipProcessingHandler = (s) => s.QtyToAllocate == 0;
					break;
				case SalesAllocationsFilter.action.DeallocateSalesOrders:
					handler = (list) => CreateInstance<SalesAllocationProcess>().DeallocateOrders(list);
					skipProcessingHandler = (s) => s.QtyToDeallocate == 0;
					break;
			}
			if (handler != null)
				Allocations.SetProcessDelegate(handler);

			if (skipProcessingHandler != null)
				Allocations.SkipProcessing = skipProcessingHandler;
		}

		protected virtual void SetFieldsVisibility(SalesAllocationsFilter filter)
		{
			bool isDeallocation = filter.Action == SalesAllocationsFilter.action.DeallocateSalesOrders;

			PXUIFieldAttribute.SetVisible<SalesAllocation.qtyAllocated>(Allocations.Cache, null, isDeallocation);
			PXUIFieldAttribute.SetVisible<SalesAllocation.qtyUnallocated>(Allocations.Cache, null, !isDeallocation);
		}

		protected virtual void SetOrderStatusList(SalesAllocationsFilter filter)
		{
			bool isDeallocation = filter?.Action == SalesAllocationsFilter.action.DeallocateSalesOrders;
			var stringlist = isDeallocation
				? new SalesAllocationsFilter.orderStatus.ListAttribute.WithExpiredAttribute()
				: new SalesAllocationsFilter.orderStatus.ListAttribute();

			PXStringListAttribute.SetList<SalesAllocationsFilter.orderStatus>(Filter.Cache, filter, stringlist);
		}

		protected virtual void SetOrderTypeList()
		{
			string[] orderTypes = AvailableOrderTypes.GetOrderTypes();
			var orderTypeValues = orderTypes
				.Select(ot =>
				{
					var orderType = SOOrderType.PK.Find(this, ot);
					if (string.IsNullOrEmpty(orderType?.Descr))
						return (ot, ot);
					return (ot, $"{ot} - {orderType.Descr}");
				})
				.ToArray();

			PXStringListAttribute.SetLocalizable<SalesAllocationsFilter.orderType>(Filter.Cache, null, false);
			PXStringListAttribute.SetList<SalesAllocationsFilter.orderType>(Filter.Cache, null, orderTypeValues);
		}

		protected virtual void NormalizeOrderStatusValue(SalesAllocationsFilter filter)
		{
			if (string.IsNullOrEmpty(filter?.OrderStatus))
				return;

			var stringListAttribute = Filter.Cache.GetAttributes<SalesAllocationsFilter.orderStatus>(filter).OfType<PXStringListAttribute>().FirstOrDefault();
			if(stringListAttribute != null)
			{
				var availableStatuses = stringListAttribute.GetAllowedValues(Filter.Cache);
				var currentStatuses = filter.OrderStatus.Split(new []{ MultiSelectSeparator }, StringSplitOptions.RemoveEmptyEntries).ToList();
				if (currentStatuses.RemoveAll(x => !availableStatuses.Contains(x)) > 0)
					filter.OrderStatus = string.Join(MultiSelectSeparator, currentStatuses);
			}
		}

		#endregion

		private class AvailableOrderTypes : IPrefetchable
		{
			public static string[] GetOrderTypes() => PXDatabase.GetSlot<AvailableOrderTypes>(nameof(AvailableOrderTypes), typeof(SOOrderType), typeof(SOOrderTypeOperation))._orderTypes;

			private string[] _orderTypes;

			void IPrefetchable.Prefetch()
			{
				var orderTypes = new HashSet<string>();
				foreach (PXDataRecord rec in PXDatabase.SelectMulti<SOOrderType>(
					Yaql.join<SOOrderTypeOperation>(
						Yaql.column<SOOrderTypeOperation.orderType>().eq(Yaql.column<SOOrderType.orderType>())
						.and(Yaql.column<SOOrderTypeOperation.operation>().eq(SOOperation.Issue))
						.and(Yaql.column<SOOrderTypeOperation.active>().eq(Yaql.@true))
						.and(Yaql.column<SOOrderType.active>().eq(Yaql.@true))
						.and(Yaql.column<SOOrderType.behavior>().isIn(SalesAllocationsFilter.orderType.behaviorList.Set)),
						YaqlJoinType.INNER),
						new PXAliasedDataField<SOOrderType.orderType>()))
				{
					var orderType = rec.GetString(0);
					if (!string.IsNullOrEmpty(orderType))
						orderTypes.Add(orderType);
				}
				_orderTypes = orderTypes.ToArray();
			}
		}

		#region Inventories import

		public PXAction<SalesAllocationsFilter> selectItems;

		[PXButton, PXUIField(DisplayName = "List")]
		public virtual void SelectItems() => SelectedItems.AskExt();

		[PXVirtualDAC]
		[PXImport]
		public PXSelect<InventoryLinkFilter> SelectedItems;

		public IEnumerable selectedItems()
		{
			return SelectedItems.Cache
				.Inserted.Concat_(SelectedItems.Cache.Updated)
				.RowCast<InventoryLinkFilter>()
				.ToArray();
		}

		protected virtual void _(Events.FieldUpdated<SalesAllocation, SalesAllocation.inventoryID> e)
		{
			if (e.NewValue != null && SelectedItems.Select().Any())
			{
				SelectedItems.Cache.Clear();
			}
		}

		protected virtual void _(Events.RowInserted<InventoryLinkFilter> e) => Filter.SetValueExt<SalesAllocationsFilter.inventoryID>(Filter.Current, null);

		protected virtual void _(Events.RowUpdated<InventoryLinkFilter> e) => Filter.SetValueExt<SalesAllocationsFilter.inventoryID>(Filter.Current, null);

		private int excelRowNumber = 2;
		private bool importHasError = false;

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (!viewName.Equals(nameof(SelectedItems), StringComparison.InvariantCultureIgnoreCase))
				return true;

			try
			{
				ImportInventoryItems(values);
			}
			catch (Exception e)
			{
				PXTrace.WriteError(Messages.RowError, excelRowNumber, e.Message);
				importHasError = true;
			}
			finally
			{
				excelRowNumber++;
			}
			return false;
		}

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode)
		{
			excelRowNumber = 0;
			if (importHasError)
			{
				importHasError = false;
				throw new Exception(Messages.ImportHasError);
			}
		}

		public virtual bool RowImporting(string viewName, object row) => row == null;

		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		protected virtual void ImportInventoryItems(IDictionary values)
		{
			PXCache itemCache = SelectedItems.Cache;
			InventoryLinkFilter item = (InventoryLinkFilter)itemCache.CreateInstance();
			if (values.Contains(nameof(InventoryLinkFilter.inventoryID)))
			{
				try
				{
					object value = values[nameof(InventoryLinkFilter.inventoryID)];
					itemCache.SetValueExt<InventoryLinkFilter.inventoryID>(item, value);
					itemCache.Update(item);
				}
				catch (Exception e)
				{
					PXTrace.WriteError(Messages.RowError, excelRowNumber, e.Message);
				}
			}
		}

		/// <exclude/>
		[PXCacheName(IN.Messages.InventoryLinkFilter)]
		public class InventoryLinkFilter : IBqlTable
		{
			#region InventoryID
			[StockItem(IsKey = true)]
			[PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>>),
				IN.Messages.InventoryItemIsInStatus, typeof(InventoryItem.itemStatus), ShowWarning = true)]
			public virtual int? InventoryID { get; set; }
			public abstract class inventoryID : BqlInt.Field<inventoryID> { }
			#endregion

			#region Descr
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			[PXUIField(DisplayName = "Description", Enabled = false)]
			public class descr : PXFieldAttachedTo<InventoryLinkFilter>.By<ManageSalesAllocations>.AsString.Named<descr>
			{
				public override string GetValue(InventoryLinkFilter Row) => InventoryItem.PK.Find(Base, Row?.InventoryID)?.Descr;
			}
			#endregion
		}

		#endregion
	}
}
