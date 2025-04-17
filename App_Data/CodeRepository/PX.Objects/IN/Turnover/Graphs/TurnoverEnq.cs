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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN.GraphExtensions.TurnoverEnqExt;
using PX.Objects.SO.Interfaces;
using static PX.Data.PXImportAttribute;

namespace PX.Objects.IN.Turnover
{
	[TableAndChartDashboardType]
	public class TurnoverEnq : PXGraph<TurnoverEnq>, IPXPrepareItems, IPXProcess
	{
		public InventoryLinkFilterExt InventoryLinkFilterExt => FindImplementation<InventoryLinkFilterExt>();

		public override bool IsDirty => false;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Actions
		public PXAction<INTurnoverEnqFilter> refresh;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable Refresh(PXAdapter adapter)
		{
			FindTurnoverCalc(Filter.Current);
			return adapter.Get();
		}

		public PXCancel<INTurnoverEnqFilter> Cancel;

		public PXAction<INTurnoverEnqFilter> previousPeriod;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			var filter = Filter.Current;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, false);
			FinPeriod prevFromPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.FromPeriodID, looped: true);
			FinPeriod prevToPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.ToPeriodID, looped: true);
			if (prevFromPeriod != null && prevToPeriod != null)
			{
				filter.FromPeriodID = prevFromPeriod.FinPeriodID;
				filter.ToPeriodID = prevToPeriod.FinPeriodID;
				FindTurnoverCalc(filter);
			}

			return adapter.Get();
		}

		public PXAction<INTurnoverEnqFilter> nextPeriod;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			var filter = Filter.Current;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, false);
			FinPeriod nextFromPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.FromPeriodID, looped: true);
			FinPeriod nextToPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.ToPeriodID, looped: true);
			if (nextFromPeriod != null && nextToPeriod != null)
			{
				filter.FromPeriodID = nextFromPeriod.FinPeriodID;
				filter.ToPeriodID = nextToPeriod.FinPeriodID;
				FindTurnoverCalc(filter);
			}
			return adapter.Get();
		}

		public PXAction<INTurnoverEnqFilter> calculateTurnover;

		[PXUIField(DisplayName = "Calculate Turnover", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CalculateTurnover(PXAdapter adapter)
		{
			var clone = this.Clone();

			PXLongOperation.StartOperation(this, () =>
			{
				PXLongOperation.SetCustomInfo(clone);

				var filter = clone.Filter.Current;
				var filteredInventories = clone.InventoryLinkFilterExt.SelectedItems
					.SelectMain()
					.Select(l => l.InventoryID)
					.ToArray();

				var branches = clone.GetBranches(filter);

				if (!branches.Any())
					return;

				if (filter.SiteID != null
					|| filter.ItemClassID != null
					|| filter.InventoryID != null
					|| filteredInventories.Any())
				{
					INTurnoverCalc fullTurnoverCalc = SelectFrom<INTurnoverCalc>
						.Where<INTurnoverCalc.branchID.IsIn<@P.AsInt>
						.And<INTurnoverCalc.fromPeriodID.IsEqual<@P.AsString.ASCII>>
						.And<INTurnoverCalc.toPeriodID.IsEqual<@P.AsString.ASCII>>
						.And<INTurnoverCalc.isFullCalc.IsEqual<True>>>
						.View.ReadOnly.SelectWindowed(clone, 0, 1, branches, filter.FromPeriodID, filter.ToPeriodID);
					if (fullTurnoverCalc != null)
					{
						if (filter.BranchID != null)
							throw new PXException(Messages.CannotReplaceFullTurnoverOfBranch);
						throw new PXException(Messages.CannotReplaceFullTurnoverOfCompany);
					}
				}

				var graph = CreateInstance<TurnoverCalcMaint>();

				foreach (var branchID in branches)
				{
					var calcArgs = new TurnoverCalculationArgs
					{
						BranchID = branchID,
						FromPeriodID = filter.FromPeriodID,
						ToPeriodID = filter.ToPeriodID,

						SiteID = filter.SiteID,
						ItemClassID = filter.ItemClassID,
						Inventories = filter.InventoryID != null
							? new[] { filter.InventoryID }
							: filteredInventories
					};

					if (!calcArgs.IsFullCalc)
					{
						INTurnoverCalc existingCalc = graph.Calc.Select(calcArgs.BranchID, calcArgs.FromPeriodID, calcArgs.ToPeriodID);
						if (existingCalc != null && existingCalc.IsFullCalc == true)
							throw new PXException(Messages.CannotReplaceFullTurnoverOfBranch);
					}

					graph.Calculate(calcArgs);
				}

				clone.FindTurnoverCalc(clone.Filter.Current);
			});

			return adapter.Get();
		}

		public PXAction<INTurnoverEnqFilter> manageTurnoverHistory;

		[PXUIField(DisplayName = "Manage Turnover History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ManageTurnoverHistory(PXAdapter adapter)
		{
			var filter = Filter.Current;

			var manageGraph = CreateInstance<ManageTurnover>();

			manageGraph.Filter.SetValueExt<INTurnoverCalcFilter.action>(manageGraph.Filter.Current, INTurnoverCalcFilter.action.Calculate);

			if (filter.OrgBAccountID != null)
				manageGraph.Filter.SetValueExt<INTurnoverCalcFilter.orgBAccountID>(manageGraph.Filter.Current, filter.OrgBAccountID);

			if (filter.FromPeriodID != null)
				manageGraph.Filter.Cache.SetValue<INTurnoverCalcFilter.fromPeriodID>(manageGraph.Filter.Current, filter.FromPeriodID);

			if (filter.ToPeriodID != null)
				manageGraph.Filter.Cache.SetValue<INTurnoverCalcFilter.toPeriodID>(manageGraph.Filter.Current, filter.ToPeriodID);

			var ex = new PXRedirectRequiredException(manageGraph, true, Messages.INTurnoverCalc)
			{
				Mode = PXBaseRedirectException.WindowMode.New
			};
			throw ex;
		}

		#endregion

		#region Views
		public PXFilter<INTurnoverEnqFilter> Filter;

		[PXFilterable]
		public SelectFrom<TurnoverCalcItem>
			.Where<TurnoverCalcItem.branchID.Is<Inside<INTurnoverEnqFilter.orgBAccountID.FromCurrent>>
				.And<TurnoverCalcItem.fromPeriodID.IsEqual<INTurnoverEnqFilter.fromPeriodID.FromCurrent>>
				.And<TurnoverCalcItem.toPeriodID.IsEqual<INTurnoverEnqFilter.toPeriodID.FromCurrent>>>
			.OrderBy<
				Asc<TurnoverCalcItem.inventoryCD>,
				Asc<TurnoverCalcItem.siteCD>>
			.View.ReadOnly
			TurnoverCalcItems;

		protected virtual IEnumerable turnoverCalcItems()
		{
			var filter = Filter.Current;
			if (filter.IsSuitableCalcsFound != true)
				return Array<TurnoverCalcItem>.Empty;

			var cmd = TurnoverCalcItems.View.BqlSelect;
			var parameters = new List<object>();
			cmd = AppendFilter(cmd, parameters, filter);

			var view = new PXView(this, TurnoverCalcItems.View.IsReadOnly, cmd);

			var startRow = PXView.StartRow;
			int totalRows = 0;
			var result = view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;

			return result;
		}

		public PXSetup<INSetup> insetup;

		#endregion

		#region Initialization
		public TurnoverEnq()
		{
			INSetup record = insetup.Current;
		}
		#endregion

		#region Event handlers

		#region INTurnoverEnqFilter event handlers

		protected virtual void _(Events.RowSelected<INTurnoverEnqFilter> e)
		{
			if (e.Row == null)
				return;

			SetFilterWarnings(e.Row);
			SetCostColumnsDisplayName(e.Row);
		}

		protected virtual void SetFilterWarnings(INTurnoverEnqFilter filter)
		{
			var filterCache = Filter.Cache;

			var organization = PXAccess.GetOrganizationByBAccountID(filter.OrgBAccountID);
			PXSetPropertyException commonWarning = null;
			if (filter.IsPartialSuitableCalcs == true)
			{
				if (filter.BranchID != null)
					commonWarning = new PXSetPropertyException(Messages.BranchTurnoverHasNotBeenCalculated, PXErrorLevel.Warning,
						filterCache.GetStateExt<INTurnoverEnqFilter.fromPeriodID>(filter), filterCache.GetStateExt<INTurnoverEnqFilter.toPeriodID>(filter));
				else if (organization.IsGroup != true)
					commonWarning = new PXSetPropertyException(Messages.CompanyTurnoversHasNotBeenCalculated, PXErrorLevel.Warning,
						filterCache.GetStateExt<INTurnoverEnqFilter.fromPeriodID>(filter), filterCache.GetStateExt<INTurnoverEnqFilter.toPeriodID>(filter));
				else
					commonWarning = new PXSetPropertyException(Messages.GroupTurnoversHasNotBeenCalculated, PXErrorLevel.Warning,
						filterCache.GetStateExt<INTurnoverEnqFilter.fromPeriodID>(filter), filterCache.GetStateExt<INTurnoverEnqFilter.toPeriodID>(filter));
			}
			else if (filter.IsMixedSuitableCalcs == true)
			{
				if (organization?.IsGroup != true)
					commonWarning = new PXSetPropertyException(Messages.CompanyTurnoversHaveBeenFiltered, PXErrorLevel.Warning, filterCache.GetStateExt<INTurnoverEnqFilter.orgBAccountID>(filter));
				else
					commonWarning = new PXSetPropertyException(Messages.GroupTurnoversHaveBeenFiltered, PXErrorLevel.Warning, filterCache.GetStateExt<INTurnoverEnqFilter.orgBAccountID>(filter));
			}
			filterCache.RaiseExceptionHandling<INTurnoverEnqFilter.toPeriodID>(filter, filter.ToPeriodID, commonWarning);

			filterCache.RaiseExceptionHandling<INTurnoverEnqFilter.siteID>(filter, filter.SiteID, filter.SuitableCalcsSiteID != null
				? new PXSetPropertyException(Messages.IsSiteCalc, PXErrorLevel.Warning, filterCache.GetStateExt<INTurnoverEnqFilter.suitableCalcsSiteID>(filter))
				: null);
			filterCache.RaiseExceptionHandling<INTurnoverEnqFilter.itemClassID>(filter, filter.ItemClassID, filter.SuitableCalcsItemClassID != null
				? new PXSetPropertyException(Messages.IsItemClassCalc, PXErrorLevel.Warning, filterCache.GetStateExt<INTurnoverEnqFilter.suitableCalcsItemClassID>(filter))
				: null);

			if (filter.SuitableCalcsInventoryID != null)
				filterCache.RaiseExceptionHandling<INTurnoverEnqFilter.inventoryID>(filter, filter.InventoryID,
					new PXSetPropertyException(Messages.IsInventoryCalc, PXErrorLevel.Warning, filterCache.GetStateExt<INTurnoverEnqFilter.suitableCalcsInventoryID>(filter)));
			else
				filterCache.RaiseExceptionHandling<INTurnoverEnqFilter.inventoryID>(filter, filter.InventoryID, filter.IsInventoryListCalc == true
					? new PXSetPropertyException(Messages.IsInventoryListCalc, PXErrorLevel.Warning)
					: null);
		}

		protected virtual void SetCostColumnsDisplayName(INTurnoverEnqFilter filter)
		{
			var currency = GetCurrency(filter);

			if (string.IsNullOrEmpty(currency?.CurySymbol))
			{
				PXUIFieldAttribute.SetDisplayName(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.begCost), Messages.BeginningInventory);
				PXUIFieldAttribute.SetDisplayName(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.ytdCost), Messages.EndingInventory);
				PXUIFieldAttribute.SetDisplayName(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.avgCost), Messages.AverageInventory);
				PXUIFieldAttribute.SetDisplayName(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.soldCost), Messages.CostOfGoodsSold);
				PXUIFieldAttribute.SetDisplayName(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.costRatio), Messages.TurnoverRatio);
			}
			else
			{
				PXUIFieldAttribute.SetDisplayNameLocalized(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.begCost), PXMessages.LocalizeFormatNoPrefix(Messages.BeginningInventoryCurrency, currency.CurySymbol));
				PXUIFieldAttribute.SetDisplayNameLocalized(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.ytdCost), PXMessages.LocalizeFormatNoPrefix(Messages.EndingInventoryCurrency, currency.CurySymbol));
				PXUIFieldAttribute.SetDisplayNameLocalized(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.avgCost), PXMessages.LocalizeFormatNoPrefix(Messages.AverageInventoryCurrency, currency.CurySymbol));
				PXUIFieldAttribute.SetDisplayNameLocalized(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.soldCost), PXMessages.LocalizeFormatNoPrefix(Messages.CostOfGoodsSoldCurrency, currency.CurySymbol));
				PXUIFieldAttribute.SetDisplayNameLocalized(TurnoverCalcItems.Cache, nameof(TurnoverCalcItem.costRatio), PXMessages.LocalizeFormatNoPrefix(Messages.TurnoverRatioCurrency, currency.CurySymbol));
			}
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverEnqFilter, INTurnoverEnqFilter.fromPeriodID> e)
		{
			if (e.Row.ToPeriodID != null && string.CompareOrdinal(e.Row.FromPeriodID, e.Row.ToPeriodID) > 0)
			{
				e.Cache.SetValue<INTurnoverEnqFilter.toPeriodID>(e.Row, e.Row.FromPeriodID);
			}
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverEnqFilter, INTurnoverEnqFilter.toPeriodID> e)
		{
			if (e.Row.FromPeriodID != null && string.CompareOrdinal(e.Row.FromPeriodID, e.Row.ToPeriodID) > 0)
			{
				e.Cache.SetValue<INTurnoverEnqFilter.fromPeriodID>(e.Row, e.Row.ToPeriodID);
			}
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverEnqFilter, INTurnoverEnqFilter.orgBAccountID> e)
		{
			if (!IsValidValue<INTurnoverEnqFilter.siteID>(e.Row))
				e.Cache.SetValueExt<INTurnoverEnqFilter.siteID>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverEnqFilter, INTurnoverEnqFilter.itemClassID> e)
		{
			if (!IsValidValue<INTurnoverEnqFilter.inventoryID>(e.Row))
				e.Cache.SetValueExt<INTurnoverEnqFilter.inventoryID>(e.Row, null);
		}

		#endregion

		#endregion

		public virtual void FindTurnoverCalc(INTurnoverEnqFilter filter)
		{
			void found(bool isCalcFound)
			{
				filter.IsSuitableCalcsFound = isCalcFound;
				filter.IsPartialSuitableCalcs = false;
				filter.IsMixedSuitableCalcs = false;
				filter.SuitableCalcsSiteID = null;
				filter.SuitableCalcsItemClassID = null;
				filter.SuitableCalcsInventoryID = null;
				filter.IsInventoryListCalc = false;
			};

			if (filter.OrgBAccountID == null
				|| filter.FromPeriodID == null
				|| filter.ToPeriodID == null)
			{
				found(false);
				return;
			}
			var branches = GetBranches(filter);
			if (!branches.Any())
			{
				found(false);
				return;
			}

			var turnoverCalcs = new SelectFrom<Branch>
					.LeftJoin<INTurnoverCalc>
						.On<INTurnoverCalc.branchID.IsEqual<Branch.branchID>
						.And<INTurnoverCalc.fromPeriodID.IsEqual<@P.AsString.ASCII>>
						.And<INTurnoverCalc.toPeriodID.IsEqual<@P.AsString.ASCII>>>
					.Where<Branch.branchID.IsIn<@P.AsInt>>
					.View.ReadOnly(this)
					.Select<INTurnoverCalc>(filter.FromPeriodID, filter.ToPeriodID, branches);

			if (turnoverCalcs.Any(x => x.BranchID == null))
			{
				found(false);
				filter.IsPartialSuitableCalcs = true;
				return;
			}

			var inventories = filter.InventoryID != null
				? new[] { filter.InventoryID }
				: InventoryLinkFilterExt.SelectedItems.SelectMain().Select(x => x.InventoryID).ToArray();

			if (filter.SiteID == null
				&& filter.ItemClassID == null
				&& inventories.Length == 0)
			{
				if (turnoverCalcs.All(x => x.IsFullCalc == true))
				{
					found(true);
				}
				else if (turnoverCalcs.Any(x => x.IsFullCalc == true) && turnoverCalcs.Any(x => x.IsFullCalc == false))
				{
					found(false);
					filter.IsMixedSuitableCalcs = true;
				}
				else
				{
					var groups = turnoverCalcs
						.GroupBy(x => (x.SiteID, x.ItemClassID, x.InventoryID, x.IsInventoryListCalc))
						.ToList();
					if (groups.Count > 1)
					{
						found(false);
						filter.IsMixedSuitableCalcs = true;
					}
					else
					{
						var groupKey = groups[0].Key;

						if (groupKey.IsInventoryListCalc == true && !IsInventoryListMatch(filter, turnoverCalcs.Select(x => x.BranchID).ToArray()))
						{
							found(false);
							filter.IsMixedSuitableCalcs = true;
						}
						else
						{
							found(true);

							filter.SuitableCalcsSiteID = groupKey.SiteID;
							filter.SuitableCalcsItemClassID = groupKey.ItemClassID;
							filter.SuitableCalcsInventoryID = groupKey.InventoryID;
							filter.IsInventoryListCalc = groupKey.IsInventoryListCalc == true;
						}
					}
				}
			}
			else
			{
				var individualCalcs = turnoverCalcs.Where(x => x.IsFullCalc == false).ToArray();

				if (!individualCalcs.Any())
				{
					found(true);
				}
				else
				{
					var groups = individualCalcs
						.GroupBy(x => (x.SiteID, x.ItemClassID, x.InventoryID, x.IsInventoryListCalc))
						.ToList();
					if (groups.Count > 1)
					{
						found(false);
						filter.IsMixedSuitableCalcs = true;
					}
					else
					{
						var groupKey = groups[0].Key;

						int?[] groupInventories = null;
						if (groupKey.IsInventoryListCalc == true && !IsInventoryListMatch(filter, turnoverCalcs.Select(x => x.BranchID).ToArray(), out groupInventories))
						{
							found(false);
							filter.IsMixedSuitableCalcs = true;
						}
						else
						{
							found(true);

							if (groupKey.SiteID != null && filter.SiteID != groupKey.SiteID)
								filter.SuitableCalcsSiteID = groupKey.SiteID;

							if (groupKey.ItemClassID != null && filter.ItemClassID != groupKey.ItemClassID)
								filter.SuitableCalcsItemClassID = groupKey.ItemClassID;

							if (groupKey.InventoryID != null && (inventories.Length != 1 || inventories[0] != groupKey.InventoryID))
								filter.SuitableCalcsInventoryID = groupKey.InventoryID;

							if (groupKey.IsInventoryListCalc == true && groupInventories != null)
							{
								if (inventories.Length == 0 || inventories.Except(groupInventories).Any())
									filter.IsInventoryListCalc = true;
							}
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowUpdated<INTurnoverEnqFilter> e)
		{
			FindTurnoverCalc(e.Row);
		}

		protected virtual bool IsInventoryListMatch(INTurnoverEnqFilter filter, int?[] branches)
		{
			if (branches.Length == 1)
				return true;

			return IsInventoryListMatch(filter, branches, out var _);
		}

		protected virtual bool IsInventoryListMatch(INTurnoverEnqFilter filter, int?[] branches, out int?[] inventories)
		{
			var inventoriesQuery = new
				SelectFrom<INTurnoverCalcItem>
					.Where<INTurnoverCalcItem.branchID.IsEqual<@P.AsInt>
					.And<INTurnoverCalcItem.fromPeriodID.IsEqual<@P.AsString.ASCII>>
					.And<INTurnoverCalcItem.toPeriodID.IsEqual<@P.AsString.ASCII>>>
				.View.ReadOnly(this);

			using (new PXFieldScope(inventoriesQuery.View, typeof(INTurnoverCalcItem.inventoryID)))
			{
				inventories = inventoriesQuery
					.SelectMain(
						branches[0],
						filter.FromPeriodID, filter.ToPeriodID)
					.Select(x => x.InventoryID)
					.ToArray();
			}

			branches = branches.Skip(1).ToArray();
			if (branches.Length == 1)
				return true;

			return IsInventoryListMatch(filter, branches, inventories);
		}

		protected virtual bool IsInventoryListMatch(INTurnoverEnqFilter filter, int?[] branches, int?[] inventories)
		{
			var filteredItemsQuery = new
				SelectFrom<INTurnoverCalcItem>
					.Where<INTurnoverCalcItem.branchID.IsIn<@P.AsInt>
					.And<INTurnoverCalcItem.fromPeriodID.IsEqual<@P.AsString.ASCII>>
					.And<INTurnoverCalcItem.toPeriodID.IsEqual<@P.AsString.ASCII>>
					.And<INTurnoverCalcItem.inventoryID.IsIn<@P.AsInt>>>
				.AggregateTo<
					GroupBy<INTurnoverCalcItem.branchID>,
					GroupBy<INTurnoverCalcItem.fromPeriodID>,
					GroupBy<INTurnoverCalcItem.toPeriodID>,
					Count<INTurnoverCalcItem.inventoryID>>
				.View.ReadOnly(this);
			PXResult<INTurnoverCalcItem>[] filteredItems;
			using (new PXFieldScope(filteredItemsQuery.View, typeof(INTurnoverCalcItem.branchID), typeof(INTurnoverCalcItem.fromPeriodID), typeof(INTurnoverCalcItem.toPeriodID)))
			{
				// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters Correct
				filteredItems = filteredItemsQuery
					.Select(
						branches,
						filter.FromPeriodID, filter.ToPeriodID,
						inventories)
					.ToArray();
			}
			if (filteredItems.Any(x => x.RowCount != inventories.Length))
				return false;

			return true;
		}

		protected virtual int[] GetBranches(INTurnoverEnqFilter filter)
		{
			int[] availBranches;
			if (filter.BranchID != null)
				availBranches = new[] { filter.BranchID.Value };
			else
			{
				var organizationID = filter.OrganizationID != null
					? filter.OrganizationID
					: PXAccess.GetOrganizationByBAccountID(filter.OrgBAccountID)?.OrganizationID;

				availBranches = PXAccess.GetChildBranchIDs(organizationID);
			}
			if (filter.SiteID == null)
				return availBranches;

			var site = INSite.PK.Find(this, filter.SiteID);
			if (site?.BranchID != null && availBranches.Contains(site.BranchID.Value))
				return new[] { site.BranchID.Value };

			return Array<int>.Empty;
		}

		protected virtual CurrencyList GetCurrency(INTurnoverEnqFilter filter)
		{
			var org = filter.OrganizationID != null
				? PXAccess.GetOrganizationByID(filter.OrganizationID)
				: PXAccess.GetOrganizationByBAccountID(filter.OrgBAccountID);

			var currency = CurrencyList.PK.Find(this, org?.BaseCuryID);
			return currency;
		}

		protected virtual bool IsValidValue<TField>(object row)
			where TField : IBqlField
		{
			var table = BqlCommand.GetItemType<TField>();
			var cache = Caches[table];
			var value = Filter.Cache.GetValue<TField>(row);
			if (value == null)
				return true;

			try
			{
				cache.RaiseFieldVerifying<TField>(row, ref value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public virtual BqlCommand AppendFilter(BqlCommand cmd, IList<object> parameters, INTurnoverEnqFilter filter)
		{
			if (filter.SiteID != null)
			{
				cmd = cmd.WhereAnd<Where<TurnoverCalcItem.siteID.IsIn<@P.AsInt>>>();
				parameters.Add(filter.SiteID);
			}

			if (filter.ItemClassID != null)
			{
				cmd = cmd.WhereAnd<Where<TurnoverCalcItem.itemClassID.IsIn<@P.AsInt>>>();
				parameters.Add(filter.ItemClassID);
			}
			return cmd;
		}

		#region PXImportAttribute.IPXPrepareItems and PXImportAttribute.IPXProcess implementations
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values) => true;

		public virtual bool RowImporting(string viewName, object row) => row == null;

		public virtual bool RowImported(string viewName, object row, object oldRow) => oldRow == null;

		public virtual void PrepareItems(string viewName, IEnumerable items) { }

		public virtual void ImportDone(PXImportAttribute.ImportMode.Value mode) { }
		#endregion
	}
}
