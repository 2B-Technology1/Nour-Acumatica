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
using System.Text;
using System.Linq;
using PX.Data;
using System.Collections;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common.Logging;
using PX.Objects.Common.Tools;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.GL.ConsolidationImport;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;
using PX.Reports;

namespace PX.Objects.GL.Consolidation
{
	

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ConsolSourceDataMaint : PXGraph<ConsolSourceDataMaint>
	{
		[Serializable]
		[PXHidden]
		public class ConsolRecordsFilter : IBqlTable
		{
			#region LedgerCD
			public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
			[PXString]
			[PXSelector(typeof(Ledger.ledgerCD), DescriptionField = typeof(Ledger.descr))]
			[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string LedgerCD { get; set; }
			#endregion
			#region BranchCD
			public abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
			[PXString]
			[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string BranchCD { get; set; }
			#endregion
		}

		public PXFilter<ConsolRecordsFilter> Filter;
		public PXCancel<ConsolRecordsFilter> Cancel;

		public PXSelectOrderBy<GLConsolData,
			OrderBy<Asc<GLConsolData.finPeriodID>>> ConsolRecords;

		protected PXSelect<Segment,
			Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>> SubaccountSegmentsView;

		public PXSetup<GLSetup> GLSetup;

		protected virtual IExportSubaccountMapper CreateExportSubaccountMapper()
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				var segments = SubaccountSegmentsView.Select().RowCast<Segment>().ToArray();

				var subaccountSegmentValuesView = new PXSelect<SegmentValue,
					Where<SegmentValue.dimensionID, Equal<SubAccountAttribute.dimensionName>>>(this);

				var segmentValues = subaccountSegmentValuesView.Select().RowCast<SegmentValue>();

				return new ExportSubaccountMapper(segments, segmentValues);
			}
			else
			{
				return new SubOffExportSubaccountMapper();
			}
		}

		protected virtual IEnumerable consolRecords()
		{
			var ledgerCD = Filter.Current.LedgerCD;
			var companyOrBranchCD = Filter.Current.BranchCD;

			if (string.IsNullOrEmpty(Filter.Current.LedgerCD))
			{
				return new List<GLConsolData>();
			}

			Ledger ledger = PXSelect<Ledger,
				Where<Ledger.consolAllowed, Equal<True>,
				And<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>>>>.Select(this, ledgerCD);

			if (ledger == null)
			{
				throw new PXException(Messages.CantFindConsolidationLedger, ledgerCD);
			}

			Organization org = OrganizationMaint.FindOrganizationByCD(this, companyOrBranchCD);
			if (org != null)
			{
				if (org.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					org = null; // use BranchCD = OrganizationCD in this particular case
				}
			}
			else
			{
				Branch branch = BranchMaint.FindBranchByCD(this, companyOrBranchCD);

				if (!string.IsNullOrEmpty(companyOrBranchCD) && branch == null)
				{
					throw new PXException(Messages.CantFindConsolidationBranch, companyOrBranchCD);
				}

				if (branch != null && ledger.BalanceType != LedgerBalanceType.Report)
				{
					Organization organization = OrganizationMaint.FindOrganizationByID(this, branch.OrganizationID);

					if (organization.OrganizationType == OrganizationTypes.WithBranchesNotBalancing)
					{
						throw new PXException(Messages.BranchCannotBeConsolidated, companyOrBranchCD);
					}
				}
			}

			var exportSubaccountMapper = CreateExportSubaccountMapper();

			var noSegmentsToExport = false;
			if (PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				noSegmentsToExport = SubaccountSegmentsView.Select()
					.RowCast<Segment>()
					.All(segment => segment.ConsolNumChar <= 0);
			}

			ConsolRecords.Cache.Clear();

			PXSelectBase<GLHistory> cmd = new PXSelectJoin<GLHistory,
				InnerJoin<Account, On<Account.accountID, Equal<GLHistory.accountID>>,
				InnerJoin<Sub, On<Sub.subID, Equal<GLHistory.subID>>,
				InnerJoin<Ledger, On<Ledger.ledgerID, Equal<GLHistory.ledgerID>>,
				InnerJoin<Branch, On<Branch.branchID, Equal<GLHistory.branchID>>>>>>,
				Where<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>,
				And<GLHistory.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>,
				OrderBy<Asc<GLHistory.finPeriodID, Asc<Account.accountCD, Asc<Sub.subCD>>>>>(this);

			if (!string.IsNullOrEmpty(companyOrBranchCD))
			{
				if (org != null)
				{
					cmd.Join<InnerJoin<Organization, On<Organization.organizationID, Equal<Branch.organizationID>>>>();
					cmd.WhereAnd<Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>();
				}
				else
				{
					cmd.WhereAnd<Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>();
				}
			}

			foreach (PXResult<GLHistory, Account, Sub> result in cmd.Select(ledgerCD, companyOrBranchCD))
			{
				GLHistory history = result;
				Account account = result;
				Sub sub = result;

				string accountCD = account.GLConsolAccountCD;
				string subCD = exportSubaccountMapper.GetMappedSubaccountCD(sub);

				if (accountCD != null && accountCD.TrimEnd() != ""
					&& (subCD != null && subCD.TrimEnd() != "" || noSegmentsToExport))
				{
					GLConsolData consolData = new GLConsolData();
					consolData.MappedValue = subCD;
					consolData.AccountCD = accountCD;
					consolData.FinPeriodID = history.FinPeriodID;
					consolData = ConsolRecords.Locate(consolData);
					if (consolData != null)
					{
						consolData.ConsolAmtDebit += history.TranPtdDebit;
						consolData.ConsolAmtCredit += history.TranPtdCredit;
					}
					else
					{
						consolData = new GLConsolData();
						consolData.MappedValue = subCD;
						consolData.MappedValueLength = subCD.Length;
						consolData.AccountCD = accountCD;
						consolData.FinPeriodID = history.FinPeriodID;
						consolData.ConsolAmtDebit = history.TranPtdDebit;
						consolData.ConsolAmtCredit = history.TranPtdCredit;
						ConsolRecords.Insert(consolData);
					}
				}
			}

			return ConsolRecords.Cache.Inserted;
		}

		public ConsolSourceDataMaint()
		{
			GLSetup setup = GLSetup.Current;

			ConsolRecords.Cache.AllowDelete = false;
			ConsolRecords.Cache.AllowInsert = false;
			ConsolRecords.Cache.AllowUpdate = false;

			SubaccountSegmentsView =
				new PXSelect<Segment, Where<Segment.dimensionID, Equal<SubAccountAttribute.dimensionName>>>(this);
		}
	}
}
