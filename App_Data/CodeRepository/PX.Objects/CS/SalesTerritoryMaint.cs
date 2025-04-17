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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CS
{
	public class SalesTerritoryMaint : PXGraph<SalesTerritoryMaint, SalesTerritory>, ICaptionable
	{
		#region Selects / Views
		public PXSelect<SalesTerritory> SalesTerritory;

		[PXHidden]
		public SelectFrom<Country>
					.OrderBy<Country.selected.Desc, Country.countryID.Asc>
				.View Countries;

		[PXHidden]
		public SelectFrom<State>
						.Where<State.countryID.IsEqual<SalesTerritory.countryID.FromCurrent>>
					.OrderBy<State.selected.Desc, State.countryID.Asc>
				.View CountryStates;

		[PXHidden]
		public SelectFrom<State>
						.Where<int0.IsEqual<int1>>
				.View EmptyView;

		#endregion

		#region Ctors

		public string Caption()
		{
			SalesTerritory currentItem = this.SalesTerritory.Current;
			if (currentItem == null) return "";

			if (currentItem.SalesTerritoryID != null && currentItem.Name != null)
				return $"{currentItem.SalesTerritoryID} - {currentItem.Name}";
			return "";
		}
		#endregion

		#region Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(IIf2<
					Where<
						Country.salesTerritoryID, IsNotNull,
						And<Country.salesTerritoryID, Equal<Current<SalesTerritory.salesTerritoryID>>>>, True, False>))]
		public virtual void _(Events.CacheAttached<Country.selected> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(IIf2<
						Where<
							State.salesTerritoryID, IsNotNull,
							And<State.salesTerritoryID, Equal<Current<SalesTerritory.salesTerritoryID>>>>, True, False>))]
		public virtual void _(Events.CacheAttached<State.selected> e)
		{
		}

		public virtual void _(Events.RowSelected<SalesTerritory> e)
		{
			if (e.Row == null) return;

			switch (e.Row.SalesTerritoryType)
			{
				case SalesTerritoryTypeAttribute.ByState:
					Countries.AllowSelect = false;
					CountryStates.AllowSelect =
						CountryStates.AllowUpdate = true;
					CountryStates.AllowDelete =
						CountryStates.AllowInsert = false;
					EmptyView.AllowSelect = false;
					break;
				case SalesTerritoryTypeAttribute.ByCountry:
					Countries.AllowSelect =
						Countries.AllowUpdate = true;
					Countries.AllowDelete =
						Countries.AllowInsert = false;
					CountryStates.AllowSelect = false;
					EmptyView.AllowSelect = false;
					break;
				default:
					Countries.AllowSelect = false;
					CountryStates.AllowSelect = false;
					EmptyView.AllowSelect = true;
					EmptyView.AllowInsert =
						EmptyView.AllowDelete =
						EmptyView.AllowUpdate = false;
					break;
			}
		}

		public virtual void _(Events.FieldUpdated<SalesTerritory.salesTerritoryType> e)
		{
			if (e.Row == null) return;
			SalesTerritory row = e.Row as SalesTerritory;

			if (SalesTerritoryTypeAttribute.ByState.Equals(e.OldValue))
			{
				foreach (State item in SelectFrom<State>
										.Where<State.salesTerritoryID
											.IsEqual<@P.AsString>>
									.View
									.Select(this, row.SalesTerritoryID))
				{
					item.Selected = false;
					this.Caches[typeof(State)].Update(item);
				}
			}
			else if (SalesTerritoryTypeAttribute.ByCountry.Equals(e.OldValue))
			{
				foreach (Country item in SelectFrom<Country>
										.Where<Country.salesTerritoryID
											.IsEqual<@P.AsString>>
									.View
									.Select(this, row.SalesTerritoryID))
				{
					item.Selected = false;
					this.Caches[typeof(Country)].Update(item);
				}
			}
			row.CountryID = null;
		}

		public virtual void _(Events.FieldUpdated<SalesTerritory, SalesTerritory.countryID> e)
		{
			if (e.Row == null) return;

			if (e.OldValue != null)
			{
				foreach (State item in SelectFrom<State>
										.Where<
												State.salesTerritoryID.IsEqual<@P.AsString>
											.And<State.countryID.IsEqual<@P.AsString>>>
									.View
									.Select(this, new object[] { e.Row.SalesTerritoryID, e.OldValue }))
				{
					item.Selected = false;
					this.Caches[typeof(State)].Update(item);
				}
			}
		}

		protected virtual void _(Events.RowSelected<Country> e)
		{
			e.Cache
				.AdjustUI(e.Row)
				.ForAllFields(ui => ui.Enabled = false)
				.For<Country.selected>(ui => ui.Enabled = true);
		}

		public virtual void _(Events.FieldUpdated<Country, Country.selected> e)
		{
			if (e.Row == null) return;

			e.Row.SalesTerritoryID =
				true.Equals(e.NewValue)
					? SalesTerritory.Current?.SalesTerritoryID
					: null;
		}

		protected virtual void _(Events.RowSelected<State> e)
		{
			e.Cache
				.AdjustUI(e.Row)
				.ForAllFields(ui => ui.Enabled = false)
				.For<State.selected>(ui => ui.Enabled = true);
		}

		public virtual void _(Events.FieldUpdated<State, State.selected> e)
		{
			if (e.Row == null) return;

			e.Row.SalesTerritoryID =
				true.Equals(e.NewValue)
					? SalesTerritory.Current?.SalesTerritoryID
					: null;
		}

		#endregion

		#region Methods
		public override void Persist()
		{
			if ((this.Caches[typeof(State)].IsInsertedUpdatedDeleted
				|| this.Caches[typeof(Country)].IsInsertedUpdatedDeleted)
				&& IsSalesTerritoryChnaged())
			{
				if (this.SalesTerritory.View.Ask(
							this.SalesTerritory.Current,
							Confirmations.Confirmation,
							Confirmations.SalesTerritoriesUpdated,
							MessageButtons.YesNoCancel,
							new Dictionary<WebDialogResult, string>
							{
								[WebDialogResult.Yes] = "Update",
								[WebDialogResult.No] = "Skip",
								[WebDialogResult.Cancel] = "Cancel",
							}, MessageIcon.None).IsIn(WebDialogResult.Yes, WebDialogResult.No)
					)
				{
					bool skip = this.SalesTerritory.View.Answer == WebDialogResult.No;

					if (skip)
					{
						if (SalesTerritory.Current?.SalesTerritoryType == SalesTerritoryTypeAttribute.ByState)
						{
							this
								.Caches[typeof(State)]
								.Updated
								.OfType<State>()
								.Where(v =>
									(this.Caches[typeof(State)].GetOriginal(v) as State)
										?.SalesTerritoryID != null
										&& v.SalesTerritoryID == SalesTerritory.Current?.SalesTerritoryID)
								.ForEach(v =>
									{
										this.Caches[typeof(State)].Remove(v);
									});
						}
						else if (SalesTerritory.Current?.SalesTerritoryType == SalesTerritoryTypeAttribute.ByCountry)
						{
							this
								.Caches[typeof(Country)]
								.Updated
								.OfType<Country>()
								.Where(v =>
									(this.Caches[typeof(Country)].GetOriginal(v) as Country)
										?.SalesTerritoryID != null
										&& v.SalesTerritoryID == SalesTerritory.Current?.SalesTerritoryID)
								.ForEach(v =>
								{
									this.Caches[typeof(Country)].Remove(v);
								});
						}
					}
				}
				else
				{
					this.SalesTerritory.View.Answer = WebDialogResult.None;
					return;
				}
			}
			base.Persist();
		}

		protected virtual bool IsSalesTerritoryChnaged()
		{
			return
				this
					.Caches[typeof(State)]
					.Updated
					.OfType<State>()
					.Any(v =>
						v?.SalesTerritoryID != null
						&&
						(this.Caches[typeof(State)].GetOriginal(v) as State)?.SalesTerritoryID != null)
				||
				this
					.Caches[typeof(Country)]
					.Updated
					.OfType<Country>()
					.Any(v =>
						v?.SalesTerritoryID != null
						&&
						(this.Caches[typeof(Country)].GetOriginal(v) as Country)?.SalesTerritoryID != null);
		}

		#endregion
	}
}

