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

using CommonServiceLocator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.GL.Overrides.PostGraph;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CM
{
	[TableAndChartDashboardType]
	public class RevalueGLAccounts : RevalueAcountsBase<RevaluedGLHistory>
	{
		public PXCancel<RevalueFilter> Cancel;
		public PXFilter<RevalueFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<RevaluedGLHistory, RevalueFilter, Where<boolTrue, Equal<boolTrue>>, OrderBy<Asc<RevaluedGLHistory.ledgerID, Asc<RevaluedGLHistory.accountID, Asc<RevaluedGLHistory.subID>>>>> GLAccountList;
		public PXSelect<CurrencyInfo> currencyinfo;
		public PXSetup<GLSetup> glsetup;
		public PXSetup<CMSetup> cmsetup;
		public PXSetup<Company> company;

		public RevalueGLAccounts()
		{
			var curCMSetup = cmsetup.Current;
			var curGLSetup = glsetup.Current;
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() != true)
				throw new Exception(Messages.MultiCurrencyNotActivated);
			GLAccountList.SetProcessCaption(Messages.Revalue);
			GLAccountList.SetProcessAllVisible(false);

			PXUIFieldAttribute.SetEnabled<RevaluedGLHistory.finPtdRevalued>(GLAccountList.Cache, null, true);
		}

		protected virtual void _(Events.RowSelected<RevaluedGLHistory> e)
		{
			RevaluedGLHistory history = e.Row;
			if (history == null) return;

			Account account = Account.PK.Find(this, history.AccountID);
			PXUIFieldAttribute.SetEnabled<RevaluedGLHistory.selected>(e.Cache, history, account.Active == true);
			e.Cache.RaiseExceptionHandling<RevaluedGLHistory.selected>(history, null, account.Active == true ? null : new PXSetPropertyException(Messages.AccountInactive, PXErrorLevel.RowWarning, account.AccountCD));
		}

		protected virtual void RevalueFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RevalueFilter filter = (RevalueFilter)e.Row;
			if (filter != null)
			{
				GLAccountList.SetProcessDelegate(delegate (List<RevaluedGLHistory> list)
				{
					var graph = CreateInstance<RevalueGLAccounts>();
					graph.Revalue(filter, list);
				}
				);

				VerifyCurrencyEffectiveDate(sender, filter);
			}
		}

		protected virtual void RevalueFilter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
		}

		protected virtual void RevalueFilter_OrgBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RevalueFilter filter = (RevalueFilter)e.Row;
			if (filter != null)
			{
				filter.CuryID = null;
			}
			GLAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_FinPeriodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RevalueFilter.curyEffDate>(e.Row);

			if (((RevalueFilter)e.Row).CuryEffDate != null)
			{
				((RevalueFilter)e.Row).CuryEffDate = ((DateTime)((RevalueFilter)e.Row).CuryEffDate).AddDays(-1);
			}
			GLAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryEffDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLAccountList.Cache.Clear();
		}

		protected virtual void RevalueFilter_TotalRevalued_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				decimal val = 0m;
				foreach (RevaluedGLHistory res in GLAccountList.Cache.Updated)
				{
					if ((bool)res.Selected)
					{
					    decimal sign = AccountRules.IsDEALAccount(res.AccountType) ? 1.0m : -1.0m;
                        val += sign * (decimal)res.FinPtdRevalued;
                    }
				}

				if (val == 0)
				{
					TimeSpan timespan;
					Exception ex;
					var status = PXLongOperation.GetStatus(UID, out timespan, out ex);
					if (status == PXLongRunStatus.Completed)
					{
						Filter.Cache.RaiseExceptionHandling<RevalueFilter.totalRevalued>(e.Row, val,
							new PXSetPropertyException(Messages.NoRevaluationEntryWasMade, PXErrorLevel.Warning));
					}
				}
				else
				{
					e.ReturnValue = val;
				}
			}
		}

		public virtual IEnumerable glaccountlist()
		{
			foreach (Tuple<string, int?, int?> branchPeriodFillter in PXSelectJoin<Branch,
				   LeftJoin<FinPeriod,
					   On<Branch.organizationID, Equal<FinPeriod.organizationID>,
														And<Where<Branch.branchID, InsideBranchesOf<Current2<RevalueFilter.orgBAccountID>>,
					   Or<Current2<RevalueFilter.orgBAccountID>, IsNull,
						   And<Not<FeatureInstalled<FeaturesSet.multipleBaseCurrencies>>>>>>>>,
				   Where2<Where<FinPeriod.masterFinPeriodID, Equal<Current<RevalueFilter.finPeriodID>>,
																And<Current2<RevalueFilter.orgBAccountID>, IsNull>>,
				   Or<Where<FinPeriod.finPeriodID, Equal<Current<RevalueFilter.finPeriodID>>
				   >>>>.Select(this)
				   .Select(_ => Tuple.Create(_.GetItem<FinPeriod>().FinPeriodID, _.GetItem<Branch>().BranchID, _.GetItem<Branch>().LedgerID)))
			{
				foreach (Tuple<GLHistory, Account> historyAndAccount in PXSelectJoin<GLHistoryByPeriod,
					LeftJoin<Account, On<Account.accountID, Equal<GLHistoryByPeriod.accountID>>,
					LeftJoin<GLHistory, On<GLHistory.ledgerID, Equal<GLHistoryByPeriod.ledgerID>,
						And<GLHistory.branchID, Equal<GLHistoryByPeriod.branchID>,
						And<GLHistory.accountID, Equal<GLHistoryByPeriod.accountID>,
						And<GLHistory.subID, Equal<GLHistoryByPeriod.subID>,
						And<GLHistory.finPeriodID, Equal<GLHistoryByPeriod.lastActivityPeriod>>>>>>>>,
					Where<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>,
					And<GLHistoryByPeriod.branchID, Equal<Required<GLHistoryByPeriod.branchID>>,
					And<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
					And<Account.curyID, Equal<Current<RevalueFilter.curyID>>,
					And<Where<GLHistory.curyFinYtdBalance, NotEqual<decimal0>,
							Or<GLHistory.finYtdBalance, NotEqual<decimal0>
					>>>>>>>>
					.Select(this, branchPeriodFillter.Item1, branchPeriodFillter.Item2, branchPeriodFillter.Item3)
					.Select(_ => Tuple.Create(_.GetItem<GLHistory>(), _.GetItem<Account>())))
				{
					Account account = historyAndAccount.Item2;

					RevaluedGLHistory hist = new RevaluedGLHistory();
					PXCache<GLHistory>.RestoreCopy(hist, historyAndAccount.Item1);
					hist.AccountType = account.Type;
				RevaluedGLHistory existing;

				if ((existing = GLAccountList.Locate(hist)) != null)
				{
					yield return existing;
					continue;
				}
				else
				{
					GLAccountList.Cache.SetStatus(hist, PXEntryStatus.Held);
				}

					if (string.IsNullOrEmpty(hist.CuryRateTypeID = account.RevalCuryRateTypeId))
				{
					hist.CuryRateTypeID = cmsetup.Current.GLRateTypeReval;
				}

				if (string.IsNullOrEmpty(hist.CuryRateTypeID))
				{
					GLAccountList.Cache.RaiseExceptionHandling<RevaluedGLHistory.curyRateTypeID>(hist, null, new PXSetPropertyException(Messages.RateTypeNotFound));
				}
				else
				{
					CurrencyRate curyrate = Filter.Current.OrganizationBaseCuryID == null ?
						PXSelect<CurrencyRate,
						Where<CurrencyRate.fromCuryID, Equal<Current<RevalueFilter.curyID>>,
						And<CurrencyRate.toCuryID, Equal<Current<Company.baseCuryID>>,
						And<CurrencyRate.curyRateType, Equal<Required<Account.revalCuryRateTypeId>>,
						And<CurrencyRate.curyEffDate, LessEqual<Current<RevalueFilter.curyEffDate>>>>>>,
						OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(this, hist.CuryRateTypeID)
						:
						PXSelect<CurrencyRate,
						Where<CurrencyRate.fromCuryID, Equal<Current<RevalueFilter.curyID>>,
						And<CurrencyRate.toCuryID, Equal<Current<RevalueFilter.organizationBaseCuryID>>,
						And<CurrencyRate.curyRateType, Equal<Required<Account.revalCuryRateTypeId>>,
						And<CurrencyRate.curyEffDate, LessEqual<Current<RevalueFilter.curyEffDate>>>>>>,
						OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(this, hist.CuryRateTypeID);

					if (curyrate == null || curyrate.CuryMultDiv == null)
					{
						hist.CuryMultDiv = "M";
						hist.CuryRate = 1m;
						hist.RateReciprocal = 1m;
						hist.CuryEffDate = Filter.Current.CuryEffDate;
						GLAccountList.Cache.RaiseExceptionHandling<RevaluedGLHistory.curyRate>(hist, 1m, new PXSetPropertyException(Messages.RateNotFound, PXErrorLevel.RowWarning));
					}
					else
					{
						hist.CuryRate = curyrate.CuryRate;
						hist.RateReciprocal = curyrate.RateReciprocal;
						hist.CuryEffDate = curyrate.CuryEffDate;
						hist.CuryMultDiv = curyrate.CuryMultDiv;
					}

					CurrencyInfo info = new CurrencyInfo();
					info.BaseCuryID = Filter.Current.OrganizationBaseCuryID == null ? company.Current.BaseCuryID : Filter.Current.OrganizationBaseCuryID;
					info.CuryID = hist.CuryID;
					info.CuryMultDiv = hist.CuryMultDiv;
					info.CuryRate = hist.CuryRate;

						PXCurrencyAttribute.CuryConvBase(currencyinfo.Cache, info, (decimal)hist.CuryFinYtdBalance, out decimal baseval);
					hist.FinYtdRevalued = baseval;
					hist.FinPtdRevalued = hist.FinYtdRevalued - hist.FinYtdBalance;
						hist.LastRevaluedFinPeriodID = PXSelect<GLHistory,
							Where<GLHistory.finPtdRevalued, NotEqual<decimal0>,
							And<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>,
							And<GLHistory.branchID, Equal<Required<GLHistory.branchID>>,
							And<GLHistory.accountID, Equal<Required<GLHistoryByPeriod.accountID>>,
								And<GLHistory.subID, Equal<Required<GLHistoryByPeriod.subID>>>>>>>>
							.Select(this, hist.LedgerID, hist.BranchID, hist.AccountID, hist.SubID)
							.Select(_ => _.GetItem<GLHistory>().FinPeriodID).Max();
				}

				yield return hist;
			}
		}
		}

		public void Revalue(RevalueFilter filter, List<RevaluedGLHistory> list)
		{
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			PostGraph pg = PXGraph.CreateInstance<PostGraph>();
			PXCache basecache = je.Caches[typeof(AcctHist)];
			je.Views.Caches.Add(typeof(AcctHist));

            string extRefNbrNumbering = je.CMSetup.Current.ExtRefNbrNumberingID;
            if (string.IsNullOrEmpty(extRefNbrNumbering) == false)
            {
                RevaluationRefNbrHelper helper = new RevaluationRefNbrHelper(extRefNbrNumbering);
				helper.Subscribe(je);
            }

			DocumentList<Batch> created = new DocumentList<Batch>(je);

			Currency currency = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(je, filter.CuryID);

			bool hasErrors = false;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (RevaluedGLHistory hist in list)
				{
                    PXProcessing<RevaluedGLHistory>.SetCurrentItem(hist);
					if (hist.FinPtdRevalued == 0m)
					{
                        PXProcessing<RevaluedGLHistory>.SetProcessed();
						continue;
					}

					PXAccess.MasterCollection.Organization org = PXAccess.GetOrganizationByBAccountID(filter.OrgBAccountID);
					string FinPeriod = (filter.OrgBAccountID == null || (org != null && org.IsGroup)) ?
						FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(hist.BranchID), filter.FinPeriodID)
						.Result
						.FinPeriodID
						: filter.FinPeriodID;

					ProcessingResult result = CheckFinPeriod(FinPeriod, hist.BranchID);
					if (!result.IsSuccess)
					{
						hasErrors = true;
						continue;
					}

					if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
					{
						je.Save.Press();

						if (created.Find(je.BatchModule.Current) == null)
						{
							created.Add(je.BatchModule.Current);
						}
					}

					Batch cmbatch = created.Find<Batch.branchID>(hist.BranchID) ?? new Batch();
					if (cmbatch.BatchNbr == null)
					{
						je.Clear();

						CurrencyInfo info = new CurrencyInfo();
						info.CuryID = hist.CuryID;
						info.CuryEffDate = hist.CuryEffDate;
						info.BaseCalc = false;
						info = je.currencyinfo.Insert(info) ?? info;

						cmbatch = new Batch();
						cmbatch.BranchID = hist.BranchID;
						cmbatch.Module = "CM";
						cmbatch.Status = "U";
						cmbatch.AutoReverse = false;
						cmbatch.Released = true;
						cmbatch.Hold = false;
						cmbatch.DateEntered = filter.CuryEffDate;
						cmbatch.FinPeriodID = FinPeriod;
						cmbatch.CuryID = hist.CuryID;
						cmbatch.CuryInfoID = info.CuryInfoID;
						cmbatch.DebitTotal = 0m;
						cmbatch.CreditTotal = 0m;
						cmbatch.Description = filter.Description;
						je.BatchModule.Insert(cmbatch);

						CurrencyInfo b_info = je.currencyinfo.Select();
						if (b_info != null)
						{
							b_info.CuryID = hist.CuryID;
							b_info.CuryEffDate = hist.CuryEffDate;
							b_info.CuryRateTypeID = hist.CuryRateTypeID;
							b_info.CuryRate = hist.CuryRate;
							b_info.RecipRate = hist.RateReciprocal;
							b_info.CuryMultDiv = hist.CuryMultDiv;
							je.currencyinfo.Update(b_info);
						}
					}
					else
					{
						if (!je.BatchModule.Cache.ObjectsEqual(je.BatchModule.Current, cmbatch))
						{
							je.Clear();
						}

						je.BatchModule.Current = je.BatchModule.Search<Batch.batchNbr>(cmbatch.BatchNbr, cmbatch.Module);
					}

					{
						GLTran tran = new GLTran();
						tran.SummPost = false;
						tran.AccountID = hist.AccountID;
						tran.SubID = hist.SubID;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						if (hist.AccountType == AccountType.Asset || hist.AccountType == AccountType.Expense)
						{
							tran.DebitAmt = (hist.FinPtdRevalued < 0m) ? 0m : hist.FinPtdRevalued;
							tran.CreditAmt = (hist.FinPtdRevalued < 0m) ? -1m * hist.FinPtdRevalued : 0m;
						}
						else
						{
							tran.DebitAmt = (hist.FinPtdRevalued < 0m) ? -1m * hist.FinPtdRevalued : 0m;
							tran.CreditAmt = (hist.FinPtdRevalued < 0m) ? 0m : hist.FinPtdRevalued;
						}

						tran.TranType = "REV";
						tran.TranClass = hist.AccountType;
						tran.RefNbr = string.Empty;
						tran.TranDesc = filter.Description;
						tran.FinPeriodID = FinPeriod;
						tran.TranDate = filter.CuryEffDate;
						tran.CuryInfoID = null;
						tran.Released = true;
						tran.ReferenceID = null;
						tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

						je.GLTranModuleBatNbr.Insert(tran);
					}

					foreach (GLTran tran in je.GLTranModuleBatNbr.SearchAll<Asc<GLTran.tranClass>>(new object[] { "G" }))
					{
						je.GLTranModuleBatNbr.Delete(tran);
					}

					{
						GLTran tran = new GLTran();
						tran.SummPost = true;
						tran.ZeroPost = false;
						tran.CuryDebitAmt = 0m;
						tran.CuryCreditAmt = 0m;

						if (je.BatchModule.Current.DebitTotal > je.BatchModule.Current.CreditTotal)
						{
							tran.AccountID = currency.RevalGainAcctID;
                            tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.revalGainSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = 0m;
							tran.CreditAmt = (je.BatchModule.Current.DebitTotal - je.BatchModule.Current.CreditTotal);
						}
						else
						{
							tran.AccountID = currency.RevalLossAcctID;
                            tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.revalLossSubID>(je, hist.BranchID, currency);
							tran.DebitAmt = (je.BatchModule.Current.CreditTotal - je.BatchModule.Current.DebitTotal);
							tran.CreditAmt = 0m;
						}

						tran.TranType = "REV";
						tran.TranClass = GLTran.tranClass.UnrealizedAndRevaluationGOL;
						tran.RefNbr = string.Empty;
						tran.TranDesc = filter.Description;
						tran.Released = true;
						tran.ReferenceID = null;

						je.GLTranModuleBatNbr.Insert(tran);
					}

					{
						AcctHist accthist = new AcctHist();
						accthist.BranchID = hist.BranchID;
						accthist.LedgerID = hist.LedgerID;
						accthist.AccountID = hist.AccountID;
						accthist.SubID = hist.SubID;
						accthist.FinPeriodID = filter.FinPeriodID;
						accthist.CuryID = hist.CuryID;
						accthist.BalanceType = "A";

						accthist = (AcctHist)basecache.Insert(accthist);
						accthist.FinPtdRevalued += hist.FinPtdRevalued;
					}

					PXProcessing<RevaluedGLHistory>.SetProcessed();
				}

				if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
				{
					je.Save.Press();

					if (created.Find(je.BatchModule.Current) == null)
					{
						created.Add(je.BatchModule.Current);
					}
				}

				ts.Complete();
			}

			CMSetup cmsetup = PXSelect<CMSetup>.Select(je);

			for (int i = 0; i < created.Count; i++)
			{
				if (cmsetup.AutoPostOption == true)
				{
					pg.Clear();
					pg.PostBatchProc(created[i]);
				}
			}

			if (hasErrors)
			{
				//Clean current to prevent set exception to the last item
				PXProcessing<RevaluedGLHistory>.SetCurrentItem(null);
				throw new PXException(ErrorMessages.SeveralItemsFailed);
			}

			if (created.Count > 0)
			{
                je.BatchModule.Current = created[created.Count - 1];
                throw new PXRedirectRequiredException(je, "Preview");
            }

            decimal val = 0m;
            foreach (RevaluedGLHistory res in GLAccountList.Cache.Updated)
            {
                if ((bool)res.Selected)
                {
                    decimal sign = AccountRules.IsDEALAccount(res.AccountType) ? 1.0m : -1.0m;
                    val += sign * (decimal)res.FinPtdRevalued;
                }
            }

            if (val == 0)
            {
                throw new PXOperationCompletedWithWarningException(Messages.NoRevaluationEntryWasMade);
            }
		}
	}

	/// <summary>
	/// The DAC represents a view into the base <see cref="GLHistory"/> entity and provides information required for
	/// the GL Accounts Revaluation process (see <see cref="RevalueGLAccounts"/>).
	/// Records of this type are used to select and display data from <see cref="GLHistory"/> prior to revaluation process (see <see cref="RevalueGLAccounts.glaccountlist"/>).
	/// The revaluation process itself is also performed based on these records and makes adjustments and generates transactions
	/// from them (see <see cref="RevalueGLAccounts.Revalue(RevalueFilter, List{RevaluedGLHistory})")/>.
	/// </summary>
	[Serializable]
	[PXBreakInheritance()]
	public partial class RevaluedGLHistory : GLHistory
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;
		[PXString(6, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency Rate Type")]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		protected Decimal? _CuryRate = 1m;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Currency Rate")]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region RateReciprocal
		public abstract class rateReciprocal : PX.Data.BQL.BqlDecimal.Field<rateReciprocal> { }
		protected Decimal? _RateReciprocal;

		[PXDecimal(8)]
		public virtual Decimal? RateReciprocal
		{
			get
			{
				return this._RateReciprocal;
			}
			set
			{
				this._RateReciprocal = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;

		[PXDate()]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		protected String _CuryMultDiv = "M";
		[PXString(1, IsFixed = true)]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region AccountType
		public abstract class accountType : PX.Data.BQL.BqlString.Field<accountType> { }
		protected string _AccountType;
		[PXString(1)]
		[AccountType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual string AccountType
		{
			get
			{
				return this._AccountType;
			}
			set
			{
				this._AccountType = value;
			}
		}
		#endregion
		#region LedgerID
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXUIField(TabOrder = 0)]
		[Branch(IsKey = true, IsDetail = true)]
		public override Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		[Account(IsKey = true, DescriptionField = typeof(Account.description))]
		public override Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		[SubAccount(IsKey = true, DescriptionField = typeof(Sub.description))]
		public override Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		[PXDBCury(typeof(RevaluedGLHistory.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Foreign Currency Balance", Enabled = false)]
		public override Decimal? CuryFinYtdBalance
		{
			get
			{
				return this._CuryFinYtdBalance;
			}
			set
			{
				this._CuryFinYtdBalance = value;
			}
		}
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Original Balance", Enabled = false)]
		public override Decimal? FinYtdBalance
		{
			get
			{
				return this._FinYtdBalance;
			}
			set
			{
				this._FinYtdBalance = value;
			}
		}
		#endregion
		#region FinYtdRevalued
		public abstract class finYtdRevalued : PX.Data.BQL.BqlDecimal.Field<finYtdRevalued> { }
		protected Decimal? _FinYtdRevalued;
		[PXBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Revalued Balance", Enabled = false)]
		[PXFormula(typeof(Add<RevaluedGLHistory.finYtdBalance, RevaluedGLHistory.finPtdRevalued>))]
		public virtual Decimal? FinYtdRevalued
		{
			get
			{
				return this._FinYtdRevalued;
			}
			set
			{
				this._FinYtdRevalued = value;
			}
		}
		#endregion
		#region FinPtdRevalued
		public new abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Difference", Enabled = true)]
		public override Decimal? FinPtdRevalued
		{
			get
			{
				return this._FinPtdRevalued;
			}
			set
			{
				this._FinPtdRevalued = value;
			}
		}
		#endregion
		#region LastRevaluedFinPeriodID
		public abstract class lastRevaluedFinPeriodID : PX.Data.BQL.BqlString.Field<lastRevaluedFinPeriodID> { }

		[PXUIField(DisplayName = "Last Revaluation Period")]
		[FinPeriodID(IsDBField = false)]
		public virtual String LastRevaluedFinPeriodID { get; set; }
		#endregion
	}
}
