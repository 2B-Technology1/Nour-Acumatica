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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA
{
	public class CalcDeprProcess : PXGraph<CalcDeprProcess>
	{
		public PXCancel<BalanceFilter> Cancel;
		public PXFilter<BalanceFilter> Filter;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<BalanceFilter> ViewAsset;
		public PXAction<BalanceFilter> ViewBook;
		public PXAction<BalanceFilter> ViewClass;

		[PXFilterable]
		public PXFilteredProcessingJoinOrderBy<FABookBalance, BalanceFilter,
				InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
				InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>,
				LeftJoin<Account, On<Account.accountID, Equal<FixedAsset.fAAccountID>>>>>,
				OrderBy<Asc<FABookBalance.assetID, Asc<FABookBalance.bookID>>>> Balances;

		public PXSetup<Company> company;
		public PXSetup<FASetup> fasetup;

		public CalcDeprProcess()
		{
			object setup = fasetup.Current;
		}

		#region CacheAttached
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID>),
			SubstituteKey = typeof(FixedAsset.assetCD), CacheGlobal = true, DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Fixed Asset", Enabled = false)]
		public virtual void FABookBalance_AssetID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributesAttribute(Method = MergeMethod.Append)]
		[PXSelector(typeof(FABook.bookID),
			SubstituteKey = typeof(FABook.bookCode),
			DescriptionField = typeof(FABook.description))]
		protected virtual void FABookBalance_BookID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		protected virtual IEnumerable balances()
		{
			BalanceFilter filter = Filter.Current;

			PXView view = new Select2<FABookBalance,
					InnerJoin<FixedAsset, On<FixedAsset.assetID, Equal<FABookBalance.assetID>>,
					InnerJoin<FADetails, On<FADetails.assetID, Equal<FABookBalance.assetID>>,
					LeftJoin<Account, On<Account.accountID, Equal<FixedAsset.fAAccountID>>>>>,
					Where<FABookBalance.depreciate, Equal<True>,
						And<FABookBalance.status, Equal<FixedAssetStatus.active>,
						And<FADetails.status, Equal<FixedAssetStatus.active>,
						And<FixedAsset.underConstruction, NotEqual<True>>>>>,
					OrderBy<Asc<FABookBalance.assetID, Asc<FABookBalance.bookID>>>>()
				.CreateView(this, mergeCache: PXView.MaximumRows > 1);

			if (filter.BookID != null)
			{
				view.WhereAnd<Where<FABookBalance.bookID, Equal<Current<BalanceFilter.bookID>>>>();
			}
			if (filter.ClassID != null)
			{
				view.WhereAnd<Where<FixedAsset.classID, Equal<Current<BalanceFilter.classID>>>>();
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>() || filter.OrgBAccountID != null)
			{
				view.WhereAnd<Where<FixedAsset.branchID, Inside<Current<BalanceFilter.orgBAccountID>>>>();
			}
			if (!string.IsNullOrEmpty(filter.PeriodID))
			{
				view.WhereAnd<Where<FABookBalance.currDeprPeriod, LessEqual<Current<BalanceFilter.periodID>>>>();
			}
			if (filter.ParentAssetID != null)
			{
				view.WhereAnd<Where<FixedAsset.parentAssetID, Equal<Current<BalanceFilter.parentAssetID>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<PXFilterRow> newFilters = new List<PXFilterRow>();
			foreach (PXFilterRow f in PXView.Filters)
			{
				if (f.DataField.ToLower() == "status")
				{
					f.DataField = "FADetails__Status";
				}
				newFilters.Add(f);
			}
			List<object> list = view.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, newFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		protected virtual void FABookBalance_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FABookBalance bal = (FABookBalance)e.Row;
			if (bal == null || PXLongOperation.Exists(UID)) return;

			bool hasError = false;
			try
			{
				CheckAcceleratedDepreciationStraightLine(this, bal);
			}
			catch (PXException exc)
			{
				hasError = true;
				if (Filter.Current.Action == BalanceFilter.action.Depreciate)
				{
					PXUIFieldAttribute.SetEnabled<FABookBalance.selected>(sender, bal, false);
					sender.RaiseExceptionHandling<FABookBalance.selected>(bal, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowError));
				}
				else
				{
					sender.RaiseExceptionHandling<FABookBalance.selected>(bal, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
				}

			}

			if (!hasError)
			{
				try
				{
					AssetProcess.CheckIfAssetIsUnderConstruction(this, bal.AssetID);
					AssetProcess.CheckUnreleasedTransactions(this, bal.AssetID);
				}
				catch (PXException exc)
				{
					PXUIFieldAttribute.SetEnabled<FABookBalance.selected>(sender, bal, false);
					sender.RaiseExceptionHandling<FABookBalance.selected>(bal, null, new PXSetPropertyException(exc.MessageNoNumber, PXErrorLevel.RowWarning));
				}
			}
		}

		private static IEnumerable<FABookBalance> GetProcessableRecords(IEnumerable<FABookBalance> list)
		{
			PXGraph graph = CreateInstance<PXGraph>();
			return list.Where(balance => !AssetProcess.UnreleasedTransactionsExistsForAsset(graph, balance.AssetID));
		}

		private void SetProcessDelegate()
		{
			BalanceFilter filter = Filter.Current;
			bool depreciate = filter.Action == BalanceFilter.action.Depreciate;

			Balances.ParallelProcessingOptions = settings => {
				settings.IsEnabled = true;
				settings.AutoBatchSize = true;
				settings.BatchSize = 500;
			};
			
			Balances.SetProcessDelegate(delegate(List<FABookBalance> list)
			{
				IEnumerable<FABookBalance> balances = GetProcessableRecords(list);

				bool success = depreciate
					? AssetProcess.DepreciateAsset(balances, null, filter.PeriodID, true)
					: AssetProcess.CalculateAsset(balances, filter.PeriodID);
				if (!success)
				{
					throw new PXOperationCompletedWithErrorException();
				}
			});

			bool canDepreciate = !string.IsNullOrEmpty(filter.PeriodID) && fasetup.Current.UpdateGL == true;
			PXUIFieldAttribute.SetEnabled<BalanceFilter.action>(Filter.Cache, filter, canDepreciate);
			if (!canDepreciate)
			{
				filter.Action = BalanceFilter.action.Calculate;
			}
		}

		protected virtual void BalanceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			SetProcessDelegate();
		}

		[PXSuppressActionValidation]
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable actionsFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable viewAsset(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				AssetMaint graph = CreateInstance<AssetMaint>();
				graph.CurrentAsset.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.assetID>>>>.Select(this);
				if (graph.CurrentAsset.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewAsset") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewBook, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewBook(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				BookMaint graph = CreateInstance<BookMaint>();
				graph.Book.Current = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.Select(this);
				if (graph.Book.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewBook") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewClass, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewClass(PXAdapter adapter)
		{
			if (Balances.Current != null)
			{
				AssetClassMaint graph = CreateInstance<AssetClassMaint>();
				graph.CurrentAssetClass.Current = PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FABookBalance.classID>>>>.Select(this);
				if (graph.CurrentAssetClass.Current != null)
				{
					throw new PXRedirectRequiredException(graph, true, "ViewClass") { Mode = PXBaseRedirectException.WindowMode.Same };
				}
			}
			return adapter.Get();
		}

		protected virtual void CheckAcceleratedDepreciationStraightLine(PXGraph graph, FABookBalance bookBalance)
		{
			PXResult<FABookBalance, FixedAsset, FAClass, FADepreciationMethod> result
			= (PXResult<FABookBalance, FixedAsset, FAClass, FADepreciationMethod>)
				SelectFrom<FABookBalance>
				.LeftJoin<FixedAsset>
					.On<FABookBalance.assetID.IsEqual<FixedAsset.assetID>>
				.LeftJoin<FAClass>
					.On<FABookBalance.classID.IsEqual<FAClass.assetID>>
				.InnerJoin<FADepreciationMethod>
					.On<FABookBalance.depreciationMethodID.IsEqual<FADepreciationMethod.methodID>>
				.Where<FABookBalance.assetID.IsEqual<FABookBalance.assetID.FromCurrent>.
					And<FABookBalance.bookID.IsEqual<FABookBalance.bookID.FromCurrent>>>
				.View.SelectSingleBound(graph, new object[] { bookBalance });

			FixedAsset asset = result;
			FAClass assetClass = result;
			FADepreciationMethod method = result;

			if (method.DepreciationMethod == FADepreciationMethod.depreciationMethod.StraightLine
				&& (bookBalance.AveragingConvention == FAAveragingConvention.FullDay || bookBalance.AveragingConvention == FAAveragingConvention.FullPeriod)
				&& assetClass.AcceleratedDepreciation == true
				&& SelectFrom<FABookHistory>
					.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
						.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
						.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>
						.And<FABookHistory.finPeriodID.IsNotEqual<P.AsString>>>
					.OrderBy<FABookHistory.finPeriodID.Asc>
					.View
					.Select(graph, bookBalance.AssetID, bookBalance.BookID, bookBalance.DeprFromPeriod).Any())
			{
				throw new PXException(Messages.AcceleratedDepreciationCalculationIssueForSLMethods, assetClass.AssetCD, asset.AssetCD);
			}
		}
	}

	[Serializable]
	public partial class BalanceFilter : ProcessAssetFilter
	{
		#region OrganizationID
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(Where<FeatureInstalled<FeaturesSet.multipleCalendarsSupport>>))]
		public override int? OrganizationID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(
			organizationFieldType: typeof(BalanceFilter.organizationID),
			onlyActive: true,
			featureFieldType: typeof(FeaturesSet.multipleCalendarsSupport),
			IsDetail = false, 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public override int? BranchID
		{
			get;
			set;
		}
		#endregion
		
		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXUIField(DisplayName = "To Period")]
		[FABookPeriodOpenInGLSelector(
			dateType: typeof(AccessInfo.businessDate),
			organizationSourceType: typeof(BalanceFilter.organizationID),
			branchSourceType: typeof(BalanceFilter.branchID),
			bookSourceType: typeof(BalanceFilter.bookID),
			isBookRequired: false)]
		public virtual string PeriodID
		{
			get;
			set;
		}
		#endregion
		#region Action
		public abstract class action : PX.Data.BQL.BqlString.Field<action>
		{
			#region List
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
				new[] { Calculate, Depreciate },
				new[] { Messages.Calculate, Messages.Depreciate })
				{ }
			}

			public const string Calculate = "C";
			public const string Depreciate = "D";

			public class calculate : PX.Data.BQL.BqlString.Constant<calculate>
			{
				public calculate() : base(Calculate) { }
			}
			public class depreciate : PX.Data.BQL.BqlString.Constant<depreciate>
			{
				public depreciate() : base(Depreciate) { }
			}
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BalanceFilter.action.Calculate)]
		[BalanceFilter.action.List]
		[PXUIField(DisplayName = "Action", Required = true)]
		public virtual string Action { get; set; }
		#endregion
	}
}
