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
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PX.Common;
using PX.CS;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Reports.ARm;

namespace PX.Objects.CS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class RMReportReaderPM : PXGraphExtension<RMReportReaderGL, RMReportReader>
    {
		/// <summary>
		/// A shared PM reports' context used during parallel calculations of the cell value.
		/// </summary>
		private class SharedContextPM
		{
			private static readonly string[] ExpandTypes = new string[]
			{
				ExpandType.AccountGroup,
				ExpandType.Project,
				ExpandType.ProjectTask,
				ExpandType.Inventory
			};

			private readonly object _locker = new object();
			private decimal _totalAmount = 0m;

			public decimal TotalAmount => _totalAmount;

			private readonly ImmutableArray<object> _splitLockers;

			public ImmutableArray<object> SplitLockers => _splitLockers;

			public RMReportReaderPM This { get; }

			public RMDataSource DataSource { get; }

			public RMDataSourceGL DataSourceGL { get; }

			public RMDataSourcePM DataSourcePM { get; }

			public ARmDataSet DataSet { get; }

			public bool Drilldown { get; }

			public bool Expansion { get; }

			public Dictionary<PMHistoryKeyTuple, PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>> DrilldownData { get; }

			public List<object[]> SplitReturn { get; }

			public List<PMAccountGroup> AccountGroups { get; }

			public List<PMTask> Tasks { get; }

			public List<PMCostCode> CostCodes { get; }

			public List<InventoryItem> Items { get; }

			public Dictionary<int?, (PMProject Project, int ProjectIndex)> ProjectsDict { get; }

			public bool ParallelizeAccountGroups { get; }

			public bool ParallelizeTasks { get; }

			public bool ParallelizeCostCodes { get; }

			public bool ParallelizeInvItems { get; }
			
			public ParallelOptions ParallelOptions { get; }

			public SharedContextPM(RMReportReaderPM @this, bool drillDown, RMDataSource ds, RMDataSourceGL dsGL, RMDataSourcePM dsPM,
								   ARmDataSet dataSet, List<PMAccountGroup> accountGroups, List<PMTask> tasks, List<PMCostCode> costCodes, List<InventoryItem> items,
								   List<PMProject> projects, List<object[]> splitret)
			{
				This = @this;
				Drilldown = drillDown;
				DataSource = ds;
				DataSourceGL = dsGL;
				DataSourcePM = dsPM;
				DataSet = dataSet;
				AccountGroups = accountGroups;
				Tasks = tasks;
				CostCodes = costCodes;
				Items = items;
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
					DrilldownData = new Dictionary<PMHistoryKeyTuple, PXResult<PMHistory, PMBudget, PMProject, PMTask, PMAccountGroup, InventoryItem, PMCostCode>>();
				}

				ProjectsDict = InitializeProjectDictionary(projects);

				(ParallelizeAccountGroups, ParallelizeTasks, ParallelizeCostCodes, ParallelizeInvItems) =
					DetermineParallelizationOptions(workerThreadsCount);

				if (SplitReturn != null && ExpandTypes.Contains(DataSource.Expand))
				{
					Expansion = true;
					var lockersArrayBuilder = ImmutableArray.CreateBuilder<object>(initialCapacity: SplitReturn.Count);

					for (int i = 0; i < SplitReturn.Count; ++i)
					{
						var lockerInArray = new object();
						lockersArrayBuilder.Add(lockerInArray);
					}

					_splitLockers = lockersArrayBuilder.ToImmutable();
				}
				else
				{
					Expansion = false;
					_splitLockers = ImmutableArray<object>.Empty;				
				}
			}

			private Dictionary<int?, (PMProject Project, int ProjectIndex)> InitializeProjectDictionary(List<PMProject> projects)
			{
				var projectsDict = new Dictionary<int?, (PMProject Project, int ProjectIndex)>(capacity: projects.Count);

				for (int i = 0; i < projects.Count; i++)
					projectsDict[projects[i].ContractID] = (Project: projects[i], ProjectIndex: i);

				return projectsDict;
			}

			/// <summary>
			/// Determine parallelization options considering that we want no more than one nested parallel loop with good parallelization on worker threads.
			/// </summary>
			/// <exception cref="InvalidOperationException">Thrown when the number of worker threads is less than 1.</exception>
			/// <param name="workerThreadsCount">Number of worker threads.</param>
			/// <returns>
			/// Parallelization options.
			/// </returns>
			private (bool ParallelizeAccountGroups, bool ParallelizeTasks, bool ParallelizeCostCodes, bool ParallelizeInvItems) DetermineParallelizationOptions(
																																	int workerThreadsCount)
			{
				if (workerThreadsCount <= 0)
					throw new InvalidOperationException("The number of worker threads cannot be less than one");

				if (WebConfig.ParallelizeAllDimensionsInArmReports)
					return (ParallelizeAccountGroups: true, ParallelizeTasks: true, ParallelizeCostCodes: true, ParallelizeInvItems: true);
				else if (workerThreadsCount == 1)
					return (ParallelizeAccountGroups: false, ParallelizeTasks: false, ParallelizeCostCodes: false, ParallelizeInvItems: false);

				bool accountGroupsAreWellParallelized = AccountGroups.Count >= workerThreadsCount;

				if (accountGroupsAreWellParallelized)
					return (ParallelizeAccountGroups: true, ParallelizeTasks: false, ParallelizeCostCodes: false, ParallelizeInvItems: false);

				bool tasksAreWellParallelized = Tasks.Count >= workerThreadsCount;

				if (tasksAreWellParallelized)
					return (ParallelizeAccountGroups: false, ParallelizeTasks: true, ParallelizeCostCodes: false, ParallelizeInvItems: false);

				bool costCodesAreWellParallelized = CostCodes.Count >= workerThreadsCount;

				if (costCodesAreWellParallelized)
					return (ParallelizeAccountGroups: false, ParallelizeTasks: false, ParallelizeCostCodes: true, ParallelizeInvItems: false);

				bool invItemsAreWellParallelized = Items.Count >= workerThreadsCount;

				if (invItemsAreWellParallelized)
					return (ParallelizeAccountGroups: false, ParallelizeTasks: false, ParallelizeCostCodes: false, ParallelizeInvItems: true);

				bool parallelizeAccountGroups = AccountGroups.Count > 1;
				bool parallelizeTasks = Tasks.Count > 1 && !parallelizeAccountGroups;
				bool parallelizeCostCodes = CostCodes.Count > 1 && !parallelizeAccountGroups && !parallelizeTasks;
				bool parallelizeInvItems = Items.Count > 1 && !parallelizeAccountGroups && !parallelizeTasks && !parallelizeCostCodes;

				return (parallelizeAccountGroups, parallelizeTasks, parallelizeCostCodes, parallelizeInvItems);
			}

			public void AccountGroupIterationNoClosures(int accountGroupIndex)
			{
				RMReportReaderPM.AccountGroupIteration(this, accountGroupIndex);
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
