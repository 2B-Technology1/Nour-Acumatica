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
using System.Threading.Tasks;

using PX.Common;
using PX.CS;
using PX.Data;
using PX.Objects.GL;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class RMReportReaderGL : PXGraphExtension<RMReportReader>
	{
		/// <summary>
		/// A shared GL reports' context used during parallel calculations of the cell value.
		/// </summary>
		private class SharedContextGL
		{
			private readonly object _locker = new object();
			private decimal _totalAmount = 0m;

			public decimal TotalAmount => _totalAmount;

			public RMReportReaderGL This { get; }

			public RMDataSource DataSource { get; }

			public RMDataSourceGL DataSourceGL { get; }

			public ARmDataSet DataSet { get; }

			public bool Drilldown { get; }

			public Dictionary<GLHistoryKeyTuple, PXResult<ArmGLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>> DrilldownData { get; }

			public GLSetup GLSetup { get; }

			public List<object[]> SplitReturn { get; }

			public List<Account> Accounts { get; }

			public List<Sub> Subs { get; }

			public List<Branch> Branches { get; }

			public bool ParallelizeAccounts { get; }

			public bool ParallelizeSubs { get; }

			public bool ParallelizeBranches { get; }

			public ParallelOptions ParallelOptions { get; }

			public SharedContextGL(RMReportReaderGL @this, bool drillDown, RMDataSource ds, RMDataSourceGL dsGL, ARmDataSet dataSet,
								   List<Account> accounts, List<Sub> subs, List<Branch> branchList, List<object[]> splitret)
			{
				This = @this;
				Drilldown = drillDown;
				DataSource = ds;
				DataSourceGL = dsGL;
				DataSet = dataSet;
				Accounts = accounts;
				Subs = subs;
				Branches = branchList;
				SplitReturn = splitret;

				const int useAllAvailableProcessorsForParallelCalculation = -1;
				var (workerThreadsCount, useAllProcessors) = This.Base.GetThreadsCountForCellValueParallelCalculation();
				int maxDegreeOfParallelism =
					useAllProcessors
						? useAllAvailableProcessorsForParallelCalculation
						: workerThreadsCount;

				const bool forceNonParallel = false;    //set to true for debugging in single thread

				if (forceNonParallel)
				{
					workerThreadsCount = maxDegreeOfParallelism = 1;
				}

				ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

				if (Drilldown)
				{
					DrilldownData = new Dictionary<GLHistoryKeyTuple, PXResult<ArmGLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>>();
					GLSetup = PXSelect<GLSetup>.Select(This.Base);
				}

				(ParallelizeAccounts, ParallelizeSubs, ParallelizeBranches) = DetermineParallelizationOptions(workerThreadsCount);
			}

			/// <summary>
			/// Determine parallelization options considering that we want no more than one nested parallel loop with good parallelization on worker threads.
			/// </summary>
			/// <exception cref="InvalidOperationException">Thrown when the number of worker threads is less than 1.</exception>
			/// <param name="workerThreadsCount">Number of worker threads.</param>
			/// <returns>
			/// Parallelization options.
			/// </returns>
			private (bool ParallelizeAccounts, bool ParallelizeSubs, bool ParallelizeBranches) DetermineParallelizationOptions(int workerThreadsCount)
			{
				if (workerThreadsCount <= 0)
					throw new InvalidOperationException("The number of worker threads cannot be less than one");

				if (WebConfig.ParallelizeAllDimensionsInArmReports)
					return (ParallelizeAccounts: true, ParallelizeSubs: true, ParallelizeBranches: true);
				else if (workerThreadsCount == 1)
					return (ParallelizeAccounts: false, ParallelizeSubs: false, ParallelizeBranches: false);

				bool accountsAreWellParallelized = Accounts.Count >= workerThreadsCount;

				if (accountsAreWellParallelized)
					return (ParallelizeAccounts: true, ParallelizeSubs: false, ParallelizeBranches: false);

				bool subsAreWellParallelized = Subs.Count >= workerThreadsCount;

				if (subsAreWellParallelized)
					return (ParallelizeAccounts: false, ParallelizeSubs: true, ParallelizeBranches: false);

				bool branchesAreWellParallelized = Branches.Count >= workerThreadsCount;

				if (branchesAreWellParallelized)
					return (ParallelizeAccounts: false, ParallelizeSubs: false, ParallelizeBranches: true);

				bool parallelizeAccounts = Accounts.Count > 1;
				bool parallelizeSubs = Subs.Count > 1 && !parallelizeAccounts;
				bool parallelizeBranches = Branches.Count > 1 && !parallelizeAccounts && !parallelizeSubs;

				return (parallelizeAccounts, parallelizeSubs, parallelizeBranches);
			}

			public void AccountIterationNoClosures(int accountIndex)
			{
				RMReportReaderGL.AccountIteration(this, accountIndex);
			}

			public void AddToTotalAmount(in decimal amount)
			{
				lock (_locker)
				{
					_totalAmount += amount;
				}
			}
		}
	}
}
