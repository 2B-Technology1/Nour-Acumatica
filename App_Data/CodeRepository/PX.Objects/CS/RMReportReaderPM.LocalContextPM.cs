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

using PX.Common;
using PX.CS;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.CS
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public partial class RMReportReaderPM : PXGraphExtension<RMReportReaderGL, RMReportReader>
    {
		// Local context are structs to reduce memory allocations count when the code is executed sequentially

		/// <summary>
		/// A local PM reports' context used during iteration over tasks.
		/// </summary>
		private readonly struct TaskIterationContext
		{
			public SharedContextPM SharedContext { get; }

			public PMAccountGroup CurrentAccountGroup { get; }

			public int AccountGroupIndex { get; }

			public NestedDictionary<string, int, (int ProjectID, int InventoryID), Dictionary<string, PMHistory>> AccountGroupDict { get; }

			public TaskIterationContext(SharedContextPM sharedContext, PMAccountGroup currentAccountGroup, int accountGroupIndex,
										NestedDictionary<string, int, (int ProjectID, int InventoryID), Dictionary<string, PMHistory>> accountGroupDict)
			{
				SharedContext = sharedContext;
				CurrentAccountGroup = currentAccountGroup;
				AccountGroupIndex = accountGroupIndex;
				AccountGroupDict = accountGroupDict;
			}

			public void TaskIterationNoClosures(int taskIndex) => RMReportReaderPM.TaskIteration(this, taskIndex);
		}


		/// <summary>
		/// A local PM reports' context used during iteration over cost codes.
		/// </summary>
		private readonly struct CostCodeIterationContextPM
		{
			public SharedContextPM SharedContext { get; }

			public PMAccountGroup CurrentAccountGroup { get; }

			public int AccountGroupIndex { get; }

			public PMTask CurrentTask { get; }

			public int TaskIndex { get; }

			public PMProject CurrentProject { get; }

			public int ProjectIndex { get; }

			public NestedDictionary<int, (int ProjectID, int InventoryID), Dictionary<string, PMHistory>> TaskDict { get; }

			public CostCodeIterationContextPM(SharedContextPM sharedContext, PMAccountGroup currentAccountGroup, int accountGroupIndex, PMTask currentTask, int taskIndex,
											  PMProject currentProject, int projectIndex,
											  NestedDictionary<int, (int ProjectID, int InventoryID), Dictionary<string, PMHistory>> taskDict)
			{
				SharedContext = sharedContext;

				CurrentAccountGroup = currentAccountGroup;
				AccountGroupIndex = accountGroupIndex;

				CurrentTask = currentTask;
				TaskIndex = taskIndex;

				CurrentProject = currentProject;
				ProjectIndex = projectIndex;

				TaskDict = taskDict;
			}

			public void CostCodeIterationNoClosures(int costCodeIndex) => RMReportReaderPM.CostCodeIteration(this, costCodeIndex);
		}


		/// <summary>
		/// A local PM reports' context used during iteration over inventory items.
		/// </summary>
		private readonly struct InventoryItemIterationContextPM
		{
			public SharedContextPM SharedContext { get; }

			public PMAccountGroup CurrentAccountGroup { get; }

			public int AccountGroupIndex { get; }

			public PMTask CurrentTask { get; }

			public int TaskIndex { get; }

			public PMProject CurrentProject { get; }

			public int ProjectIndex { get; }

			public PMCostCode CurrentCostCode { get; }

			public Dictionary<(int ProjectID, int InventoryID), Dictionary<string, PMHistory>> CostDict { get; }

			public InventoryItemIterationContextPM(in CostCodeIterationContextPM costCodeIterationContext, PMCostCode currentCostCode,
												   Dictionary<(int ProjectID, int InventoryID), Dictionary<string, PMHistory>> costDict)
			{
				SharedContext = costCodeIterationContext.SharedContext;

				CurrentAccountGroup = costCodeIterationContext.CurrentAccountGroup;
				AccountGroupIndex = costCodeIterationContext.AccountGroupIndex;

				CurrentTask = costCodeIterationContext.CurrentTask;
				TaskIndex = costCodeIterationContext.TaskIndex;

				CurrentProject = costCodeIterationContext.CurrentProject;
				ProjectIndex = costCodeIterationContext.ProjectIndex;

				CurrentCostCode = currentCostCode;
				CostDict = costDict;
			}

			public void InvItemIterationNoClosures(int itemIndex) => RMReportReaderPM.InvItemIteration(this, itemIndex);
		}
	}
}
