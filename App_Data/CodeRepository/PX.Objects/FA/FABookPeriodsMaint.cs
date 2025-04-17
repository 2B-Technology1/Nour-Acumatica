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

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.SQLTree;

using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.DAC;
using PX.Objects.CS;
using PX.Objects.Common.Extensions;

namespace PX.Objects.FA
{
	public class FABookPeriodsMaint : PXGraph<FABookPeriodsMaint>
	{
		public PXFilter<FABookYear> BookYear;

		[PXFilterable]
		public SelectFrom<FABookPeriod>
			.Where<FABookPeriod.bookID.IsEqual<FABookYear.bookID.FromCurrent>
				.And<FABookPeriod.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>
				.And<FABookPeriod.finYear.IsEqual<FABookYear.year.FromCurrent>>>
			.OrderBy<
				Asc<FABookPeriod.periodNbr>>
			.View.ReadOnly BookPeriod;

		public enum LastFirstYear { Last, First };

		public enum PrevNextYear { Previous, Next, Equal };

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXDBIntAttribute), nameof(PXDBIntAttribute.IsKey), false)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Required), true)]
		protected void FABookYear_BookID_CacheAttached(PXCache sender)
		{ }

		[Organization(IsKey = false, ValidateValue = false)]
		protected void FABookYear_OrganizationID_CacheAttached(PXCache sender)
		{ }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXDBStringAttribute), nameof(PXDBStringAttribute.IsKey), false)]
		[PXSelector(typeof(Search<
			FABookYear.year,
			Where<FABookYear.organizationID, Equal<Current<FABookYear.organizationID>>,
				And<FABookYear.bookID, Equal<Current<FABookYear.bookID>>>>,
			OrderBy<
				Desc<FABookYear.year>>>))]
		protected void FABookYear_Year_CacheAttached(PXCache sender)
		{ }

		public PXAction<FABookYear> First;
		[PXUIField]
		[PXFirstButton]
		protected virtual IEnumerable first(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear bookYear = SelectSingleBookYear(LastFirstYear.First);

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		public PXAction<FABookYear> Previous;
		[PXUIField]
		[PXPreviousButton]
		protected virtual IEnumerable previous(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Previous, current.Year);

			if (bookYear == null)
			{
				bookYear = SelectSingleBookYear(LastFirstYear.Last);
			}

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		public PXAction<FABookYear> Next;
		[PXUIField]
		[PXNextButton]
		protected virtual IEnumerable next(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Next, current.Year);

			if (bookYear == null)
			{
				bookYear = SelectSingleBookYear(LastFirstYear.First);
			}

			BookYear.Cache.Clear();

			yield return bookYear;
		}


		public PXAction<FABookYear> Last;
		[PXUIField]
		[PXLastButton]
		protected virtual IEnumerable last(PXAdapter adapter)
		{
			if (!IsValidCurrent()) yield break;

			FABookYear bookYear = SelectSingleBookYear(LastFirstYear.Last);

			BookYear.Cache.Clear();

			yield return bookYear;
		}

		public PXAction<FABookYear> SynchronizeCalendarWithGL;
		[PXUIField(DisplayName = "Synchronize FA Calendar with GL", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(Category = CS.ActionCategories.Processing)]
		protected virtual IEnumerable synchronizeCalendarWithGL(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () =>
			{
				var graph = PXGraph.CreateInstance<FABookPeriodsMaint>();
				graph.CheckSyncNecessity();
				graph.CheckReleasedTransactionsExistence();
				graph.CheckUnreleasedTransactionsExistence();

				graph.SyncronizeCalendars();
			});

			return adapter.Get();
		}

		protected virtual void SyncronizeCalendars()
		{
			IFABookPeriodUtils bookPeriodUtils = this.GetService<IFABookPeriodUtils>();
			IFABookPeriodRepository bookPeriodRepository = this.GetService<IFABookPeriodRepository>();

			Dictionary<int, FABookPeriod> firstUnsyncedPeriodIDsByOrganization = SelectFrom<FABookPeriod>
				.InnerJoin<FABook>
					.On<FABookPeriod.bookID.IsEqual<FABook.bookID>
						.And<FABook.updateGL.IsEqual<True>>>
				.InnerJoin<FinPeriod>
					.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
						.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
				.Where<FABookPeriod.startDate.IsNotEqual<FinPeriod.startDate>
					.Or<FABookPeriod.endDate.IsNotEqual<FinPeriod.endDate>>>
				.AggregateTo<
					GroupBy<FABookPeriod.organizationID>,
					GroupBy<FABookPeriod.bookID>,
					Min<FABookPeriod.finYear>,
					Min<FABookPeriod.finPeriodID>>
				.View
				.Select(this)
				.RowCast<FABookPeriod>()
				.ToDictionary(period => (int)period.OrganizationID);


			#region Get last generated FABookYear
			FABookYear lastGeneratedFABookYear = SelectFrom<FABookYear>
				.Where<FABookYear.bookID.IsEqual<@P.AsInt>
					.And<FABookYear.organizationID.IsIn<@P.AsInt>>>
				.AggregateTo<Max<FABookYear.year>>
				.View
				.Select(
					this,
					firstUnsyncedPeriodIDsByOrganization.Values.FirstOrDefault()?.BookID,
					firstUnsyncedPeriodIDsByOrganization.Keys);
			#endregion

			List<(string, int[])> firstPeriodIDsToDeleteByOrganization = new List<(string, int[])>();

			using (PXTransactionScope transaction = new PXTransactionScope())
			{
				foreach (KeyValuePair<int, FABookPeriod> period in firstUnsyncedPeriodIDsByOrganization)
				{
					int organizationID = period.Key;
					int postingBookID = (int)period.Value.BookID;
					string firstUnsynchedPeriodID = period.Value.FinPeriodID;

					FABookPeriod lastSynchedPeriodID = SelectFrom<FABookPeriod>
						.InnerJoin<FinPeriod>
							.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
								.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
						.Where<FABookPeriod.bookID.IsEqual<@P.AsInt>
							.And<FABookPeriod.organizationID.IsEqual<@P.AsInt>>
							.And<FABookPeriod.startDate.IsEqual<FinPeriod.startDate>>
							.And<FABookPeriod.endDate.IsEqual<FinPeriod.endDate>>
							.And<FABookPeriod.finPeriodID.IsLess<@P.AsString>>>
						.AggregateTo<
							Max<FABookPeriod.finPeriodID>>
						.View
						.Select(this, postingBookID, organizationID, firstUnsynchedPeriodID);

					string firstPeriodIDToDelete = bookPeriodUtils.GetNextFABookPeriodID(lastSynchedPeriodID.FinPeriodID, postingBookID, organizationID);
					string firstYearToDelete = FinPeriodUtils.FiscalYear(firstPeriodIDToDelete);

					int[] affectedBranches = PXAccess.GetChildBranchIDs(organizationID);
					if (organizationID != FinPeriod.organizationID.MasterValue)
					{
						firstPeriodIDsToDeleteByOrganization.Add((firstPeriodIDToDelete, affectedBranches));
					}

					#region Delete the unsynchronized part of the FA calendar and the related objects
					PXDatabase.Delete<FABookPeriod>(
						new PXDataFieldRestrict<FABookPeriod.organizationID>(organizationID),
						new PXDataFieldRestrict<FABookPeriod.bookID>(postingBookID),
						new PXDataFieldRestrict<FABookPeriod.finPeriodID>(PXDbType.Unspecified, null, firstPeriodIDToDelete, PXComp.GE));

					PXDatabase.Delete<FABookYear>(
						new PXDataFieldRestrict<FABookYear.organizationID>(organizationID),
						new PXDataFieldRestrict<FABookYear.bookID>(postingBookID),
						new PXDataFieldRestrict<FABookYear.year>(PXDbType.Unspecified, null, firstYearToDelete, PXComp.GE));

					foreach (FixedAsset asset in SelectFrom<FixedAsset>
						.LeftJoin<FABookBalance>
							.On<FixedAsset.assetID.IsEqual<FABookBalance.assetID>>
						.Where<FABookBalance.updateGL.IsEqual<True>
							.And<FABookBalance.depreciate.IsEqual<True>>
							.And<FABookBalance.status.IsIn<FixedAssetStatus.active, FixedAssetStatus.suspended>>
							.And<FABookBalance.deprToPeriod.IsGreaterEqual<@P.AsString>>
							.And<FixedAsset.branchID.IsIn<@P.AsInt>>>
						.View
						.Select(this, firstUnsynchedPeriodID, PXAccess.GetChildBranchIDs(organizationID)))
					{
						PXDatabase.Delete<FABookHistory>(
							new PXDataFieldRestrict<FABookHistory.assetID>(asset.AssetID),
							new PXDataFieldRestrict<FABookHistory.bookID>(postingBookID),
							new PXDataFieldRestrict<FABookHistory.finPeriodID>(PXDbType.Unspecified, null, firstPeriodIDToDelete, PXComp.GE));
					}
					#endregion

					#region Copy the FA calendar from GL
					foreach(FinYear finYear in SelectFrom<FinYear>
						.LeftJoin<FABookYear>
							.On<FABookYear.year.IsEqual<FinYear.year>
								.And<FABookYear.organizationID.IsEqual<FinYear.organizationID>>
								.And<FABookYear.bookID.IsEqual<@P.AsInt>>>
						.Where<FinYear.organizationID.IsEqual<@P.AsInt>
							.And<FABookYear.year.IsNull>>
						.View
						.Select(this, postingBookID, organizationID))
					{
						PXDatabase.Insert<FABookYear>(
							new PXDataFieldAssign<FABookYear.bookID>(postingBookID),
							new PXDataFieldAssign<FABookYear.organizationID>(finYear.OrganizationID),
							new PXDataFieldAssign<FABookYear.year>(finYear.Year),
							new PXDataFieldAssign<FABookYear.startMasterFinPeriodID>(finYear.StartMasterFinPeriodID),
							new PXDataFieldAssign<FABookYear.finPeriods>(finYear.FinPeriods),
							new PXDataFieldAssign<FABookYear.startDate>(finYear.StartDate),
							new PXDataFieldAssign<FABookYear.endDate>(finYear.EndDate),
							new PXDataFieldAssign<FABookYear.createdByID>(finYear.CreatedByID),
							new PXDataFieldAssign<FABookYear.createdByScreenID>(finYear.CreatedByScreenID),
							new PXDataFieldAssign<FABookYear.createdDateTime>(finYear.CreatedDateTime),
							new PXDataFieldAssign<FABookYear.lastModifiedByID>(finYear.LastModifiedByID),
							new PXDataFieldAssign<FABookYear.lastModifiedByScreenID>(finYear.LastModifiedByScreenID),
							new PXDataFieldAssign<FABookYear.lastModifiedDateTime>(finYear.LastModifiedDateTime));
					}

					foreach (FinPeriod finPeriod in SelectFrom<FinPeriod>
						.LeftJoin<FABookPeriod>
							.On<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>
								.And<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>>
								.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>>
						.Where<FinPeriod.organizationID.IsEqual<@P.AsInt>
							.And<FABookPeriod.finPeriodID.IsNull>>
						.View
						.Select(this, postingBookID, organizationID))
					{
						PXDatabase.Insert<FABookPeriod>(
							new PXDataFieldAssign<FABookPeriod.bookID>(postingBookID),
							new PXDataFieldAssign<FABookPeriod.organizationID>(finPeriod.OrganizationID),
							new PXDataFieldAssign<FABookPeriod.finYear>(finPeriod.FinYear),
							new PXDataFieldAssign<FABookPeriod.finPeriodID>(finPeriod.FinPeriodID),
							new PXDataFieldAssign<FABookPeriod.masterFinPeriodID>(finPeriod.MasterFinPeriodID),
							new PXDataFieldAssign<FABookPeriod.startDate>(finPeriod.StartDate),
							new PXDataFieldAssign<FABookPeriod.endDate>(finPeriod.EndDate),
							new PXDataFieldAssign<FABookPeriod.descr>(finPeriod.Descr),
							new PXDataFieldAssign<FABookPeriod.dateLocked>(finPeriod.DateLocked),
							new PXDataFieldAssign<FABookPeriod.periodNbr>(finPeriod.PeriodNbr),
							new PXDataFieldAssign<FABookPeriod.closed>(finPeriod.Status == FinPeriod.status.Closed || finPeriod.Status == FinPeriod.status.Locked),
							new PXDataFieldAssign<FABookPeriod.createdByID>(finPeriod.CreatedByID),
							new PXDataFieldAssign<FABookPeriod.createdByScreenID>(finPeriod.CreatedByScreenID),
							new PXDataFieldAssign<FABookPeriod.createdDateTime>(finPeriod.CreatedDateTime),
							new PXDataFieldAssign<FABookPeriod.lastModifiedByID>(finPeriod.LastModifiedByID),
							new PXDataFieldAssign<FABookPeriod.lastModifiedByScreenID>(finPeriod.LastModifiedByScreenID),
							new PXDataFieldAssign<FABookPeriod.lastModifiedDateTime>(finPeriod.LastModifiedDateTime));
					}
					#endregion
				}
				transaction.Complete();
			}

			#region Generate the tail of the FA calendar
			string toYear = FinPeriodUtils.GetNextYearID(lastGeneratedFABookYear.Year);

			List<FAOrganizationBook> orgBooks = SelectFrom<FAOrganizationBook>
				.Where<FAOrganizationBook.updateGL.IsEqual<True>
					.And<FAOrganizationBook.rawOrganizationID.IsNull
						.Or<FAOrganizationBook.rawOrganizationID.IsIn<@P.AsInt>>>>
				.View
				.Select(this, firstUnsyncedPeriodIDsByOrganization.Keys)
				.RowCast<FAOrganizationBook>()
				.ToList();

			string fromYear = FinPeriodUtils.GetNextYearID(orgBooks.Min(b => b.LastCalendarYear));

			GenerationPeriods.GeneratePeriods(
				new BoundaryYears
				{
					FromYear = fromYear,
					ToYear = toYear
				},
				orgBooks);
			#endregion

			#region Recalculate the fixed assets parameters according the regenerated calendar
			foreach ((string, int[]) organizationData in firstPeriodIDsToDeleteByOrganization)
			{
				string firstPeriodIDToDelete = organizationData.Item1;
				int[] affectedBranches = organizationData.Item2;

				// Recalculate DeprToPeriod
				foreach (FABookBalance bookBalance in SelectFrom<FABookBalance>
					.LeftJoin<FixedAsset>
						.On<FABookBalance.assetID.IsEqual<FixedAsset.assetID>>
					.Where<FABookBalance.updateGL.IsEqual<True>
						.And<FABookBalance.depreciate.IsEqual<True>>
						.And<FABookBalance.status.IsIn<FixedAssetStatus.active, FixedAssetStatus.suspended>>
						.And<FABookBalance.deprToPeriod.IsGreaterEqual<@P.AsString>>
						.And<FixedAsset.branchID.IsIn<@P.AsInt>>>
					.View
					.ReadOnly
					.Select(
						this,
						firstPeriodIDToDelete,
						affectedBranches))
				{
					string origDeprToPeriod = bookBalance.DeprToPeriod;
					string newDeprToPeriod = (string)PXFormulaAttribute.Evaluate<FABookBalance.deprToPeriod>(
						this.Caches<FABookBalance>(),
						bookBalance);

					if (string.Compare(newDeprToPeriod, origDeprToPeriod) != 0)
					{
						PXDatabase.Update<FABookBalance>(
							new PXDataFieldAssign<FABookBalance.deprToPeriod>(newDeprToPeriod),
							new PXDataFieldRestrict<FABookBalance.assetID>(bookBalance.AssetID),
							new PXDataFieldRestrict<FABookBalance.bookID>(bookBalance.BookID));
					}
				}

				// Recalculate CurrDeprPeriod
				foreach (FABookBalance bookBalance in SelectFrom<FABookBalance>
					.LeftJoin<FixedAsset>
						.On<FABookBalance.assetID.IsEqual<FixedAsset.assetID>>
					.Where<FABookBalance.updateGL.IsEqual<True>
						.And<FABookBalance.depreciate.IsEqual<True>>
						.And<FABookBalance.status.IsIn<FixedAssetStatus.active, FixedAssetStatus.suspended>>
						.And<FABookBalance.lastDeprPeriod.IsNotNull>
						.And<FABookBalance.currDeprPeriod.IsGreaterEqual<@P.AsString>>
						.And<FixedAsset.branchID.IsIn<@P.AsInt>>>
					.View
					.ReadOnly
					.Select(
						this,
						firstPeriodIDToDelete,
						affectedBranches))
				{
					string origCurrDeprPeriod = bookBalance.CurrDeprPeriod;
					string newCurrDeprPeriod = bookPeriodUtils.GetNextFABookPeriodID(
						bookBalance.LastDeprPeriod,
						bookBalance.BookID,
						bookBalance.AssetID);

					if (string.Compare(newCurrDeprPeriod, origCurrDeprPeriod) != 0)
					{
						PXDatabase.Update<FABookBalance>(
						new PXDataFieldAssign<FABookBalance.currDeprPeriod>(newCurrDeprPeriod),
						new PXDataFieldRestrict<FABookBalance.assetID>(bookBalance.AssetID),
						new PXDataFieldRestrict<FABookBalance.bookID>(bookBalance.BookID));
					}
				}

				// Recalculate depreciation in the first synchronized period.
				foreach (FABookBalance bookBalance in SelectFrom<FABookBalance>
					.LeftJoin<FixedAsset>
						.On<FABookBalance.assetID.IsEqual<FixedAsset.assetID>>
					.LeftJoin<FABookHistory>
						.On<FABookHistory.assetID.IsEqual<FABookBalance.assetID>
							.And<FABookHistory.bookID.IsEqual<FABookBalance.bookID>>
							.And<FABookHistory.finPeriodID.IsEqual<FABookBalance.currDeprPeriod>>>
					.Where<FABookBalance.updateGL.IsEqual<True>
						.And<FABookBalance.depreciate.IsEqual<True>>
						.And<FABookBalance.status.IsIn<FixedAssetStatus.active, FixedAssetStatus.suspended>>
						.And<FABookBalance.deprToPeriod.IsGreaterEqual<@P.AsString>>
						.And<FABookHistory.finPeriodID.IsNull>
						.And<FixedAsset.branchID.IsIn<@P.AsInt>>>
					.View
					.ReadOnly
					.Select(
						this,
						firstPeriodIDToDelete,
						affectedBranches))
				{
					AssetProcess.CalculateAsset(bookBalance.SingleToListOrNull(), bookBalance.CurrDeprPeriod);
				}
			}
			#endregion
		}

		protected virtual void CheckSyncNecessity()
		{
			FABookPeriod unsynchronizedPeriod = SelectFrom<FABookPeriod>
				.InnerJoin<FABook>
					.On<FABookPeriod.bookID.IsEqual<FABook.bookID>
						.And<FABook.updateGL.IsEqual<True>>>
				.InnerJoin<FinPeriod>
					.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
						.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
				.Where<FABookPeriod.startDate.IsNotEqual<FinPeriod.startDate>
					.Or<FABookPeriod.endDate.IsNotEqual<FinPeriod.endDate>>>
				.View
				.ReadOnly
				.Select(this);

			if(unsynchronizedPeriod == null)
			{
				throw new PXException(Messages.FACalendarMatchGL);
			}
		}

		protected virtual void CheckReleasedTransactionsExistence()
		{
			FABookPeriod unsynchronizedPeriodWithTrans = SelectFrom<FABookPeriod>
				.InnerJoin<FABook>
					.On<FABookPeriod.bookID.IsEqual<FABook.bookID>
						.And<FABook.updateGL.IsEqual<True>>>
				.InnerJoin<FinPeriod>
					.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
						.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
				.InnerJoin<FATran>
					.On<FABookPeriod.bookID.IsEqual<FATran.bookID>
						.And<FABookPeriod.finPeriodID.IsEqual<FATran.finPeriodID>
						.And<FATran.released.IsEqual<True>>>>
				.Where<FABookPeriod.startDate.IsNotEqual<FinPeriod.startDate>
					.Or<FABookPeriod.endDate.IsNotEqual<FinPeriod.endDate>>>
				.View
				.ReadOnly
				.Select(this);

			if (unsynchronizedPeriodWithTrans != null)
			{
				throw new PXException(Messages.ReleasedTransactionsExistInUnsynchronizedPeriods);
			}
		}

		protected virtual void CheckUnreleasedTransactionsExistence()
		{
			FABookPeriod unsynchronizedPeriodWithTrans = SelectFrom<FABookPeriod>
				.InnerJoin<FABook>
					.On<FABookPeriod.bookID.IsEqual<FABook.bookID>
						.And<FABook.updateGL.IsEqual<True>>>
				.InnerJoin<FinPeriod>
					.On<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID>
						.And<FABookPeriod.finPeriodID.IsEqual<FinPeriod.finPeriodID>>>
				.InnerJoin<FATran>
					.On<FABookPeriod.bookID.IsEqual<FATran.bookID>
						.And<FABookPeriod.finPeriodID.IsEqual<FATran.finPeriodID>
						.And<FATran.released.IsNotEqual<True>>>>
				.Where<FABookPeriod.startDate.IsNotEqual<FinPeriod.startDate>
					.Or<FABookPeriod.endDate.IsNotEqual<FinPeriod.endDate>>>
				.View
				.ReadOnly
				.Select(this);

			if (unsynchronizedPeriodWithTrans != null)
			{
				throw new PXException(Messages.UnreleasedTransactionsExistInUnsynchronizedPeriods);
			}
		}

		protected bool IsValidCurrent()
		{
			if (BookYear.Cache.InternalCurrent == null) return false;

			FABookYear current = BookYear.Cache.Current as FABookYear;

			if (current.BookID == null || current.OrganizationID == null) return false;

			FABookYear existYear = SelectFrom<FABookYear>
				.Where<FABookYear.bookID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, current.BookID);

			if (existYear == null) return false;

			return true;
		}

		protected FABookYear SelectSingleBookYear(LastFirstYear lastFirstYear)
		{
			this.Caches[typeof(FABookYear)].ClearQueryCache();

			PXResultset<FABookYear> query = SelectFrom<FABookYear>
				.Where<
					FABookYear.bookID.IsEqual<FABookYear.bookID.FromCurrent>
					.And<FABookYear.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>>
				.View.ReadOnly.SelectSingleBound(this, null);

			if (lastFirstYear == LastFirstYear.First)
			{
				return query
					.OrderBy(row => ((FABookYear)row).Year)
					.First();
			}
			else
			{
				return query
					.OrderByDescending(row => ((FABookYear)row).Year)
					.First();
			}
		}

		protected FABookYear SelectSingleBookYear(PrevNextYear direction, string year)
		{
			this.Caches[typeof(FABookYear)].ClearQueryCache();

			PXResultset<FABookYear> query = SelectFrom<FABookYear>
				.Where<
					FABookYear.bookID.IsEqual<FABookYear.bookID.FromCurrent>
					.And<FABookYear.organizationID.IsEqual<FABookYear.organizationID.FromCurrent>>>
				.View.ReadOnly.Select(this);

			if (direction == PrevNextYear.Next)
			{
				return query
					.OrderBy(row => ((FABookYear)row).Year)
					.Where(row => String.Compare(((FABookYear)row).Year, year) > 0)
					.ReadOnly()
					.FirstOrDefault();
			}
			else if (direction == PrevNextYear.Previous)
			{
				return query
					.OrderByDescending(row => ((FABookYear)row).Year)
					.Where(row => String.Compare(((FABookYear)row).Year, year) < 0)
					.ReadOnly()
					.FirstOrDefault();
			}
			else
			{
				return query
					.Where(row => String.Compare(((FABookYear)row).Year, year) == 0)
					.ReadOnly()
					.FirstOrDefault();
			}
		}


		protected virtual void _(Events.RowUpdated<FABookYear> e)
		{
			e.Cache.IsDirty = false;
		}

		protected virtual void _(Events.RowSelected<FABookYear> e)
		{
			FABook book =
				SelectFrom<FABook>
					.Where<FABook.bookID.IsEqual<@P.AsInt>>
				.View.SelectSingleBound(this, null, e.Row.BookID);

			PXUIFieldAttribute.SetVisible<FABookYear.organizationID>(BookYear.Cache, null, (book != null && book.UpdateGL == true && PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>()));
		}

		protected IEnumerable bookYear()
		{
			if (BookYear.Cache.InternalCurrent == null)
			{
				FABookYear defaultYear =
					SelectFrom<FABookYear>
						.InnerJoin<FABook>.On<FABookYear.bookID.IsEqual<FABook.bookID>>
					.Where<FABookYear.organizationID.IsEqual<@P.AsInt>
						.And<FABookYear.startDate.IsLess<@P.AsDateTime>>
						.And<FABookYear.endDate.IsGreater<@P.AsDateTime>>>
					.OrderBy<
						Desc<FABook.updateGL>,
						Asc<FABookYear.year>>
					.View.SelectSingleBound(this, null, PXAccess.GetParentOrganizationID(Accessinfo.BranchID), Accessinfo.BusinessDate, Accessinfo.BusinessDate);

				return new object[] { defaultYear };
			}

			FABookYear currentFABookYear = BookYear.Cache.Current as FABookYear;

			FABook book =
				SelectFrom<FABook>
					.Where<FABook.bookID.IsEqual<@P.AsInt>>
				.View.SelectSingleBound(this, null, currentFABookYear.BookID);

			if (book == null) return null;

			if (book.UpdateGL == true && PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
			{
				if (currentFABookYear.OrganizationID == FinPeriod.organizationID.MasterValue)
				{
					currentFABookYear.OrganizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);
				}
			}
			else
			{
				currentFABookYear.OrganizationID = FinPeriod.organizationID.MasterValue;
			}

			FABookYear existingYear =
				SelectFrom<FABookYear>
					.Where<FABookYear.bookID.IsEqual<@P.AsInt>>
				.View.ReadOnly.SelectSingleBound(this, null, book.BookID);

			if (existingYear == null)
			{
				BookYear.Cache.RaiseExceptionHandling<FABookYear.year>(currentFABookYear, null,
											new PXSetPropertyException(Messages.MissingFACalendarInCurrentFinYear, PXErrorLevel.Warning));
				currentFABookYear.Year = null;
				return null;
			}
			else if (String.IsNullOrWhiteSpace(currentFABookYear.Year))
			{
				DateTime businessDate = Accessinfo.BusinessDate ?? DateTime.Now;
				currentFABookYear.Year = businessDate.Year.ToString();
			}

			FABookYear bookYear = SelectSingleBookYear(PrevNextYear.Equal, currentFABookYear.Year);

			if (bookYear == null)
			{
				BookYear.Cache.RaiseExceptionHandling<FABookYear.year>(currentFABookYear, null,
											new PXSetPropertyException(Messages.MissingFACalendarInCurrentFinYear, PXErrorLevel.Warning));
				currentFABookYear.Year = null;
				return null;
			}

			return new object[] { bookYear };
		}
	}
}
