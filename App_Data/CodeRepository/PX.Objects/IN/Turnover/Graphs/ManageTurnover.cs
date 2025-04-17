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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN.Turnover
{
	public class ManageTurnover : PXGraph<ManageTurnover>
	{
		#region Actions

		public PXAction<INTurnoverCalcFilter> refresh;

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Refresh)]
		public virtual IEnumerable Refresh(PXAdapter adapter)
		{
			TurnoverCalcs.Cache.Clear();
			TurnoverCalcs.Cache.ClearQueryCache();
			return adapter.Get();
		}

		public PXCancel<INTurnoverCalcFilter> Cancel;
		#endregion

		#region Views
		public PXFilter<INTurnoverCalcFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessing<
			INTurnoverCalc,
			INTurnoverCalcFilter,
			Where<INTurnoverCalc.branchID.Is<Inside<INTurnoverCalcFilter.orgBAccountID.FromCurrent>>
				.And<INTurnoverCalcFilter.fromPeriodID.FromCurrent.IsNull
					.Or<INTurnoverCalc.fromPeriodID.IsGreaterEqual<INTurnoverCalcFilter.fromPeriodID.FromCurrent>>>
				.And<INTurnoverCalcFilter.toPeriodID.FromCurrent.IsNull
					.Or<INTurnoverCalc.toPeriodID.IsLessEqual<INTurnoverCalcFilter.toPeriodID.FromCurrent>>>>>
			TurnoverCalcs;

		public PXSetup<INSetup> insetup;

		#endregion

		#region Initialization
		public ManageTurnover()
		{
			INSetup record = insetup.Current;
		}
		#endregion

		#region TurnoverCalcs view delegates

		protected virtual IEnumerable turnoverCalcs()
		{
			var filter = Filter.Current;
			if (filter.OrgBAccountID == null)
				return Array<INTurnoverCalc>.Empty;

			switch (filter.Action)
			{
				case INTurnoverCalcFilter.action.Delete:
					return null;
				case INTurnoverCalcFilter.action.Calculate:
					return calculateTurnoverCalcs(filter);
			}
			return Array<INTurnoverCalc>.Empty;
		}

		protected virtual IEnumerable<INTurnoverCalc> calculateTurnoverCalcs(INTurnoverCalcFilter filter)
		{
			if (filter.FromPeriodID == null && (filter.NumberOfPeriods ?? 0) <= 0
				|| filter.ToPeriodID == null
				|| filter.CalculateBy == null
				|| filter.CalculateBy == INTurnoverCalcFilter.calculateBy.None)
				return Array<INTurnoverCalc>.Empty;

			var calcCache = TurnoverCalcs.Cache;

			var calcs = calcCache.Inserted
				.RowCast<INTurnoverCalc>()
				.ToList();

			if (calcs.Any())
				return calcs;

			calcs = CreateTurnoverCalcRows(filter)
				.ToList();

			foreach (var calc in calcs)
			{
				calcCache.SetDefaultExt<INTurnoverCalc.noteID>(calc);
				calcCache.SetStatus(calc, PXEntryStatus.Inserted);
			}

			return calcs;
		}

		#endregion

		#region Event handlers

		#region INTurnoverCalcFilter event handlers

		protected virtual void _(Events.RowSelected<INTurnoverCalcFilter> e)
		{
			var filter = e.Row;
			if (filter == null)
				return;

			if (PXContext.GetSlot<PX.SM.AUSchedule>() == null)
				SetFieldsEnabled(filter);

			var allowTo = GetActionsAvailability(filter);

			TurnoverCalcs.SetProcessEnabled(allowTo.Calculate || allowTo.Delete);
			TurnoverCalcs.SetProcessAllEnabled(allowTo.Calculate || allowTo.Delete);

			if (allowTo.Calculate)
				TurnoverCalcs.SetProcessDelegate<TurnoverCalcMaint>((g, item) =>
				{
					g.Clear();
					g.Calculate(item);
				});
			else if (allowTo.Delete)
				TurnoverCalcs.SetProcessDelegate<TurnoverCalcMaint>((g, item) =>
				{
					g.Clear();
					g.Delete(item);
				});
		}

		protected virtual void _(Events.RowUpdated<INTurnoverCalcFilter> e)
		{
			if (!e.Cache.ObjectsEqual<
					INTurnoverCalcFilter.action,
					INTurnoverCalcFilter.orgBAccountID,
					INTurnoverCalcFilter.fromPeriodID,
					INTurnoverCalcFilter.toPeriodID,
					INTurnoverCalcFilter.calculateBy>(e.OldRow, e.Row))
			{
				TurnoverCalcs.Cache.Clear();
				TurnoverCalcs.Cache.ClearQueryCache();
			}
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverCalcFilter, INTurnoverCalcFilter.action> e)
		{
			e.Cache.SetDefaultExt<INTurnoverCalcFilter.calculateBy>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverCalcFilter, INTurnoverCalcFilter.fromPeriodID> e)
		{
			if (e.Row.ToPeriodID != null && string.CompareOrdinal(e.Row.FromPeriodID, e.Row.ToPeriodID) > 0)
			{
				e.Cache.SetValue<INTurnoverCalcFilter.toPeriodID>(e.Row, e.Row.FromPeriodID);
			}
		}

		protected virtual void _(Events.FieldUpdated<INTurnoverCalcFilter, INTurnoverCalcFilter.toPeriodID> e)
		{
			if (e.Row.FromPeriodID != null && string.CompareOrdinal(e.Row.FromPeriodID, e.Row.ToPeriodID) > 0)
			{
				e.Cache.SetValue<INTurnoverCalcFilter.fromPeriodID>(e.Row, e.Row.ToPeriodID);
			}
		}

		#endregion

		#region TurnoverCalc event handlers

		protected virtual void _(Events.FieldSelecting<INTurnoverCalc, INTurnoverCalc.inventoryID> e)
		{
			if (e.Row?.IsInventoryListCalc == true)
				e.ReturnValue = PXMessages.LocalizeNoPrefix(Messages.ListPlaceholder);
		}

		#endregion

		#endregion

		#region Helpers

		protected virtual INTurnoverCalc[] CreateTurnoverCalcRows(INTurnoverCalcFilter filter)
		{
			var calcs = new List<INTurnoverCalc>();
			var existingCalcs = new List<INTurnoverCalc>();

			var branches = GetBranches(filter);

			IEnumerable<INTurnoverCalc> createCalcs(string fromPeriodID, string toPeriodID)
			{
				foreach (var branchID in branches)
					yield return CreateTurnoverCalc(filter, branchID, fromPeriodID, toPeriodID);
			}

			MasterFinPeriod[] periods = null;
			string toPeriodID = filter.ToPeriodID;
			string fromPeriodID = filter.FromPeriodID;
			if (fromPeriodID == null)
			{
				periods = new SelectFrom<MasterFinPeriod>
						.Where<MasterFinPeriod.finPeriodID.IsLessEqual<@P.AsString.ASCII>>
						.OrderBy<Desc<MasterFinPeriod.finPeriodID>>
						.View.ReadOnly(this)
						.SelectWindowed(0, filter.NumberOfPeriods ?? 1, toPeriodID)
						.RowCast<MasterFinPeriod>()
						.OrderBy(x => x.FinPeriodID)
						.ToArray();
				fromPeriodID = periods.First().FinPeriodID;
			}

			var existingCalcsQuery = new SelectFrom<INTurnoverCalc>
				.Where<INTurnoverCalc.branchID.IsIn<@P.AsInt>
				.And<INTurnoverCalc.isFullCalc.IsEqual<True>>
				.And<INTurnoverCalc.fromPeriodID.IsEqual<@P.AsString.ASCII>>
				.And<INTurnoverCalc.toPeriodID.IsEqual<@P.AsString.ASCII>>>
				.View.ReadOnly(this);

			switch (filter.CalculateBy)
			{
				case INTurnoverCalcFilter.calculateBy.Range:
					calcs.AddRange(createCalcs(fromPeriodID, toPeriodID));
					existingCalcs.AddRange(existingCalcsQuery.SelectMain(branches, fromPeriodID, toPeriodID));
					break;
				case INTurnoverCalcFilter.calculateBy.Period:
					if (periods == null)
						periods = new SelectFrom<MasterFinPeriod>
							.Where<MasterFinPeriod.finPeriodID.IsGreaterEqual<@P.AsString.ASCII>
								.And<MasterFinPeriod.finPeriodID.IsLessEqual<@P.AsString.ASCII>>>
							.View.ReadOnly(this)
							.SelectMain(fromPeriodID, toPeriodID);
					foreach (var period in periods)
					{
						calcs.AddRange(createCalcs(period.FinPeriodID, period.FinPeriodID));
					}
					existingCalcs.AddRange(
						new SelectFrom<INTurnoverCalc>
							.Where<INTurnoverCalc.branchID.IsIn<@P.AsInt>
							.And<INTurnoverCalc.isFullCalc.IsEqual<True>>
							.And<INTurnoverCalc.fromPeriodID.IsEqual<INTurnoverCalc.toPeriodID>>
							.And<INTurnoverCalc.fromPeriodID.IsGreaterEqual<@P.AsString.ASCII>>
							.And<INTurnoverCalc.fromPeriodID.IsLessEqual<@P.AsString.ASCII>>>
						.View.ReadOnly(this)
						.SelectMain(branches, fromPeriodID, toPeriodID));
					break;
				case INTurnoverCalcFilter.calculateBy.Year:
					var periodYears = new SelectFrom<MasterFinPeriod>
						.Where<MasterFinPeriod.finPeriodID.IsGreaterEqual<@P.AsString.ASCII>
							.And<MasterFinPeriod.finPeriodID.IsLessEqual<@P.AsString.ASCII>>>
						.AggregateTo<
							GroupBy<MasterFinPeriod.finYear>,
							Max<MasterFinPeriod.finPeriodID>>
						.OrderBy<Asc<MasterFinPeriod.finYear>>
						.View.ReadOnly(this)
						.SelectMain(fromPeriodID, toPeriodID);
					string minPeriodID = fromPeriodID;
					foreach (var periodYear in periodYears)
					{
						minPeriodID = minPeriodID ?? FinPeriodUtils.GetFirstFinPeriodIDOfYear(new MasterFinYear { Year = periodYear.FinYear });
						calcs.AddRange(createCalcs(minPeriodID, periodYear.FinPeriodID));
						existingCalcs.AddRange(existingCalcsQuery.SelectMain(branches, minPeriodID, periodYear.FinPeriodID));
						minPeriodID = null;
					}
					break;
			}

			var existingCalcsDict = existingCalcs.ToDictionary(x => (x.BranchID, x.FromPeriodID, x.ToPeriodID));

			foreach (var calc in calcs)
			{
				if (existingCalcsDict.TryGetValue((calc.BranchID, calc.FromPeriodID, calc.ToPeriodID), out var existingCalc))
				{
					calc.CreatedDateTime = existingCalc.CreatedDateTime;
				}
			}

			return calcs.ToArray();
		}

		protected virtual int[] GetBranches(INTurnoverCalcFilter filter)
		{
			if (filter.BranchID != null)
				return new[] { filter.BranchID.Value };

			var organizationID = filter.OrganizationID != null
				? filter.OrganizationID
				: PXAccess.GetOrganizationByBAccountID(filter.OrgBAccountID)?.OrganizationID;

			return PXAccess.GetChildBranchIDs(organizationID);
		}

		protected virtual INTurnoverCalc CreateTurnoverCalc(INTurnoverCalcFilter filter, int? branchID, string fromPeriod = null, string toPeriod = null)
		{
			return new INTurnoverCalc
			{
				BranchID = branchID,
				FromPeriodID = fromPeriod ?? filter.FromPeriodID,
				ToPeriodID = toPeriod ?? filter.ToPeriodID
			};
		}

		protected (bool Calculate, bool Delete) GetActionsAvailability(INTurnoverCalcFilter filter)
		{
			bool allowCalc = filter.Action == INTurnoverCalcFilter.action.Calculate
				&& filter.OrgBAccountID != null
				&& !string.IsNullOrEmpty(filter.CalculateBy) && filter.CalculateBy != INTurnoverCalcFilter.calculateBy.None
				&& (!string.IsNullOrEmpty(filter.FromPeriodID) || filter.NumberOfPeriods > 0)
				&& !string.IsNullOrEmpty(filter.ToPeriodID);

			bool allowDelete = filter.Action == INTurnoverCalcFilter.action.Delete
				&& filter.OrgBAccountID != null;

			return (allowCalc, allowDelete);
		}

		protected virtual void SetFieldsEnabled(INTurnoverCalcFilter filter)
		{
			bool isCalcAction = filter.Action == INTurnoverCalcFilter.action.Calculate;
			bool isDeleteAction = filter.Action == INTurnoverCalcFilter.action.Delete;

			PXUIFieldAttribute.SetEnabled<INTurnoverCalcFilter.orgBAccountID>(Filter.Cache, filter, isCalcAction || isDeleteAction);

			Filter.Cache
				.Adjust<PXUIFieldAttribute>(filter)
				.For<INTurnoverCalcFilter.fromPeriodID>(fa =>
				{
					fa.Required = isCalcAction;
					fa.Enabled = isCalcAction || isDeleteAction;
				})
				.SameFor<INTurnoverCalcFilter.toPeriodID>();

			Filter.Cache
				.Adjust<PXUIFieldAttribute>(filter)
				.For<INTurnoverCalcFilter.calculateBy>(fa =>
				{
					fa.Required = isCalcAction;
					fa.Enabled = isCalcAction;
				});
		}

		#endregion
	}
}
