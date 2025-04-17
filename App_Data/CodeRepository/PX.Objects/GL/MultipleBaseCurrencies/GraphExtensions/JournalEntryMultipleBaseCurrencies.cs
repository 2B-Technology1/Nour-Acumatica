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

using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Standalone;
using PX.Objects.Common;
using PX.Objects.Common.Bql;
using PX.Data.BQL;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	public sealed class JournalEntryMultipleBaseCurrencies : PXGraphExtension<JournalEntry>
	{

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[Branch(null, typeof(Search5<Branch.branchID,
						InnerJoin<Organization,
							On<Organization.organizationID, Equal<Branch.organizationID>>,
						InnerJoin<OrganizationLedgerLink,
							On<OrganizationLedgerLink.organizationID, Equal<Branch.organizationID>>,
						InnerJoin<Ledger,
							On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
						// currently selected ledger
						InnerJoin<LedgerAlias,
							On<LedgerAlias.ledgerID, Equal<Current<Batch.ledgerID>>,
								And<Where<
									// for reporting/statistical ledgers use the same ledger as selected for batch
									LedgerAlias.balanceType, In3<LedgerBalanceType.report, LedgerBalanceType.statistical>,
										And<LedgerAlias.ledgerID, Equal<Ledger.ledgerID>,
									// for actual ledgers allow to select branches from actual ledgers with the same base currency
									Or<LedgerAlias.balanceType, Equal<LedgerBalanceType.actual>,
										And<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
										And<Ledger.baseCuryID, Equal<LedgerAlias.baseCuryID>>>>>>>>>>>>,
						Where<MatchWithBranch<Branch.branchID>>,
						Aggregate<GroupBy<Branch.branchID>>>),
			addDefaultAttribute: false,
			useDefaulting: false,
			onlyActive: true)]
		[PXDefault(typeof(Batch.branchID))]
		public void _(Events.CacheAttached<GLTran.branchID> e) { }

		protected void _(Events.FieldVerifying<GLTran, GLTran.branchID> e)
		{
			if (e.Row == null || e.NewValue == null)
				return;

			Batch batch = Base.BatchModule?.Current;
			if (batch == null)
				return;

			Branch originatingBranch = PXSelectorAttribute.Select<Batch.branchID>(Base.BatchModule?.Cache, batch) as Branch;

			if (originatingBranch == null)
				return;

			Branch branch = GetBranchFromNewValue(e.NewValue);

			if (branch != null && branch.BaseCuryID != originatingBranch.BaseCuryID)
			{
				e.NewValue = branch.BranchCD;
				throw new PXSetPropertyException(Messages.BaseCurrencyDiffers, branch.BranchCD);
			}

			if (e.Row.Module == null)
			{
				try
				{
					JournalEntry.CheckBatchBranchHasLedger(Base.BatchModule?.Cache, batch);
				}
				catch (PXException x)
				{
					e.NewValue = branch?.BranchCD;
					throw new PXSetPropertyException(x.Message);
				}
			}
		}

		protected void _(Events.RowPersisting<GLTran> e)
		{
			if (e.Row == null)
				return;

			CurrencyInfo tranCurrencyInfo = CurrencyInfoAttribute.GetCurrencyInfo<GLTran.curyInfoID>(e.Cache, e.Row);
			CurrencyInfo batchCuryInfo = Base.currencyInfo;
			Ledger ledger = Ledger.PK.Find(Base, e.Row.LedgerID);

			if (e.Operation != PXDBOperation.Delete && (
					batchCuryInfo?.BaseCuryID != tranCurrencyInfo?.BaseCuryID
					|| ledger?.BaseCuryID != tranCurrencyInfo?.BaseCuryID))
			{
				throw new PXException(Messages.IncorrectBaseCurrency, ledger?.LedgerCD);
			}
		}

		protected void _(Events.RowPersisting<Batch> e)
		{
			if (e.Row == null)
				return;

			if (e.Operation != PXDBOperation.Delete)
			{
				CheckBatchAndLedgerBaseCurrency(e.Cache, (Batch)e.Row);
			}
		}

		protected void CheckBatchAndLedgerBaseCurrency(PXCache cache, Batch batch)
		{
			CurrencyInfo tranCurrencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
											.Select(cache.Graph, batch.CuryInfoID);
			Ledger ledger = Ledger.PK.Find(cache.Graph, batch.LedgerID) as Ledger;

			if (tranCurrencyInfo != null && ledger != null
				&& !tranCurrencyInfo.BaseCuryID.Equals(ledger.BaseCuryID))
			{
				throw new PXException(Messages.IncorrectBaseCurrency, ledger.LedgerCD);
			}
		}

		private Branch GetBranchFromNewValue(object newValue)
		{
			if (newValue is string branchCD)
			{
				return PXSelect<Branch, Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(Base, branchCD);
			}

			return PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(Base, newValue);
		}
	}
}
