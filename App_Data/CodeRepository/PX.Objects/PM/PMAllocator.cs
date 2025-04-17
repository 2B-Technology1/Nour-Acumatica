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
using PX.Common.Parser;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
	/// <summary>
	/// Creates allocation transaction based on the allocation rules for the given project task(s).
	/// </summary>
	[Serializable]
	public class PMAllocator : PXGraph<PMAllocator>, IRateTable
	{
		public class MultiCurrency : PMTranMultiCurrencyPM<PMAllocator>
		{
			protected override CurySource CurrentSourceSelect()
			{
				return new CurySource();
			}

			protected override PXSelectBase[] GetChildren() => new PXSelectBase[]
			{
				Base.Transactions,
			};

			//Despite any other screens this screen always adds new PMTran lines with valid CurrencyInfo linked
			protected override void DocumentRowInserting<CuryInfoID, CuryID>(PXCache sender, object row)
			{
				long? id = (long?)sender.GetValue<CuryInfoID>(row);
				CurrencyInfo info = GetCurrencyInfo(id);
				if (info == null) base.DocumentRowInserting<CuryInfoID, CuryID>(sender, row);
				else sender.SetValue<CuryID>(row, info.CuryID);
			}
		}

		#region DAC Attributes override

		[PXDefault]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMTran.projectID> e) { }

		[PXSelector(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PMTran.projectID>>>>))]
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<PMTran.taskID> e) { }

		[PXDecimal]
		protected virtual void _(Events.CacheAttached<PMTran.tranCuryAmountCopy> e) { }

		#endregion

		#region Selects/Views
		
		public PXSelect<PMRegister> Document;
		public PXSelect<PMTran, Where<PMTran.tranType, Equal<Current<PMRegister.module>>, And<PMTran.refNbr, Equal<Current<PMRegister.refNbr>>>>> Transactions;
		public PXSelect<PMAllocationSourceTran> SourceTran;
		public PXSelect<PMAllocationAuditTran> AuditTran;
		public PXSelect<PMTaskAllocTotalAccum> TaskTotals;
		public PXSetupOptional<INSetup> Insetup;  //used in INUnit conversions and rounding. 
		public PXSetupOptional<CommonSetup> commonsetup;
		public PXSetup<CM.CMSetup> cmsetup;
		#endregion

		//stores allocated transactions for the step.
		public Dictionary<int, List<PMTranWithTrace>> stepResults;
		
		public Dictionary<int, Dictionary<int, List<PXResult<PMTran>>>> transactions; //Transactions stored by TaskID and then By AccountGroupID

		public RateEngineV2 rateEngine;

		public Dictionary<string, AllocationInfo> allocationInfo; //Cached set of allocation properties for a given AllocationID;
		public Dictionary<int, AccountGroup> accountGroups; //Cached set of AccountGroups;
		public Dictionary<string, List<PMRateDefinition>> rateDefinitions; //Cached set of RateDefinitions; 

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		/// <summary>
		/// Get or sets Restriction on Transactions that can be allocated. 
		/// If set only those transactions get allocated that have the Date greater or equal to FilterStartDate.
		/// </summary>
		public DateTime? FilterStartDate { get; set; }

        /// <summary>
        /// Get or sets Restriction on Transactions that can be allocated. 
        /// If set only those transactions get allocated that have the Date less then or equal to FilterEndDate.
        /// </summary>
        public DateTime? FilterEndDate { get; set; }

		/// <summary>
		/// This Date is used as source for PMTran.Date for newly created Allocation transaction (given that Allocation Rules is setup to use Allocation Date)
		/// </summary>
		public DateTime? PostingDate { get; set; }

		public PMAllocator()
		{
			Caches[typeof(PMAllocationDetail)].AllowInsert = false;
			Caches[typeof(PMAllocationDetail)].AllowUpdate = false;
			Caches[typeof(PMAllocationDetail)].AllowDelete = false;

			stepResults = new Dictionary<int, List<PMTranWithTrace>>();
			transactions = new Dictionary<int, Dictionary<int, List<PXResult<PMTran>>>>();
			allocationInfo = new Dictionary<string, AllocationInfo>();
			rateDefinitions = new Dictionary<string, List<PMRateDefinition>>();
		}

		/// <summary>
		/// Executes Allocation for the list of tasks.
		/// </summary>
		/// <param name="tasks"></param>
		public virtual void Execute(List<PMTask> tasks)
		{
			PreselectAccountGroups();
			if (PreSelectTasksTransactions(tasks))
			{
				foreach (PMTask task in tasks)
				{
					Execute(task, false);				
				}
			}
		}

		public virtual bool ExecuteContinueOnError(List<PMTask> tasks)
		{
			bool containsErrors = false;
			PreselectAccountGroups();
			if (PreSelectTasksTransactions(tasks))
			{
				foreach (PMTask task in tasks)
				{
					try
					{
						Execute(task, false);
					}
					catch (PXException ex)
					{
						PXTrace.WriteError(ex);
						containsErrors = true;
					}
				}
			}

			return containsErrors == false;
		}

		/// <summary>
		/// Executes Allocation for the given Task.
		/// </summary>
		/// <param name="task">Task</param>
		public virtual void Execute(PMTask task)
		{
			Execute(task, true);
		}

		public virtual void Execute(PMTask task, bool preselectTransactions)
		{
			stepResults.Clear();

			PreselectAccountGroups();

			if (preselectTransactions)
				PreSelectTaskTransactions(task);

			PMProject project = PMProject.PK.Find(this, task.ProjectID);
			if (project == null)
				throw new PXException(Messages.ProjectInTaskNotFound, task.TaskCD, task.ProjectID);

			foreach (PMAllocationDetail step in PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Required<PMAllocationDetail.allocationID>>>, OrderBy<Asc<PMAllocationDetail.stepID>>>.Select(this, task.AllocationID))
			{
				try
				{
					ProcessStep(task, project, step);
				}
				catch (PXException ex)
				{
					throw new PXException(ex, Messages.AllocationStepFailed, step.StepID, task.TaskCD?.Trim());
				}
			}

			if (stepResults.Count > 0)
			{
				foreach (KeyValuePair<int, List<PMTranWithTrace>> kv in stepResults)
				{
					foreach (PMTranWithTrace twt in kv.Value)
					{
						AddAuditTran(task.AllocationID, twt.Tran.TranID, twt.OriginalTrans);
					}
				}
			}
		}

		public virtual void PreselectAccountGroups()
		{
			if (accountGroups == null)
			{
				accountGroups = new Dictionary<int, AccountGroup>();
				PXSelectBase<PMAccountGroup> select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.isActive, Equal<True>>>(this);

				using (new PXFieldScope(select.View, new Type[] { typeof(PMAccountGroup.groupID), typeof(PMAccountGroup.groupCD) }))
				{
					foreach (PMAccountGroup ag in select.Select())
					{
						accountGroups.Add(ag.GroupID.Value, new AccountGroup(ag.GroupID.Value, ag.GroupCD));
					}
				}
			}
		}

		public virtual void PreSelectTaskTransactions(PMTask task)
		{
			if (task.AllocationID == null)
				return;

			transactions.Clear();
			Dictionary<int, List<PXResult<PMTran>>> transactionsByAccountGroup = new Dictionary<int, List<PXResult<PMTran>>>();
			HashSet<int> distinctAccountGroups = new HashSet<int>();
			HashSet<string> distinctRateTypes = new HashSet<string>();

			AllocationInfo info;
			if (!allocationInfo.TryGetValue(task.AllocationID, out info))
			{
				List<PMAllocationDetail> steps = new List<PMAllocationDetail>();
				info = new AllocationInfo(distinctAccountGroups, distinctRateTypes, steps);
				allocationInfo.Add(task.AllocationID, info);

				foreach (PMAllocationDetail step in PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Required<PMAllocationDetail.allocationID>>>,
						OrderBy<Asc<PMAllocationDetail.stepID>>>.Select(this, task.AllocationID))
				{
					if (step.Method == PMMethod.Budget)
						info.ContainsBudgetStep = true;
					foreach (int ag in GetAccountGroups(step))
					{
						distinctAccountGroups.Add(ag);
					}
					distinctRateTypes.Add(step.RateTypeID);
					steps.Add(step);
				}
			}
			else
			{
				distinctAccountGroups = info.AccountGroups;
				distinctRateTypes = info.RateTypes;
			}
			
			
			foreach (int distinctAccountGroup in distinctAccountGroups)
			{
				transactionsByAccountGroup.Add(distinctAccountGroup, new List<PXResult<PMTran>>(GetTranFromDatabase(task.ProjectID, task.TaskID, distinctAccountGroup)));
			}

			transactions.Add(task.TaskID.Value, transactionsByAccountGroup);

			rateEngine = CreateRateEngineV2( new string[1] {task.RateTableID}, distinctRateTypes.ToList());
		}

		public virtual bool PreSelectTasksTransactions(List<PMTask> tasks)
		{
			transactions.Clear();

			if (tasks.Count == 0) return false;

			bool tryToAllocate = false;
			
			HashSet<string> distinctAllocationRules = new HashSet<string>();
			HashSet<string> distinctRateTables = new HashSet<string>();
			HashSet<int> distinctProjects = new HashSet<int>();
			foreach (PMTask task in tasks)
			{
				if (task.AllocationID == null)
					continue;

				distinctAllocationRules.Add(task.AllocationID);
				transactions.Add(task.TaskID.Value, new Dictionary<int, List<PXResult<PMTran>>>());
				distinctRateTables.Add(task.RateTableID);
				distinctProjects.Add(task.ProjectID.Value);
			}

			HashSet<int> distinctAccountGroups = new HashSet<int>();
			HashSet<string> distinctRateTypes = new HashSet<string>(); 

			foreach(string allocationID in distinctAllocationRules)
			{
				AllocationInfo info;
				if (!allocationInfo.TryGetValue(allocationID, out info))
				{
					List<PMAllocationDetail> steps = new List<PMAllocationDetail>();
					info = new AllocationInfo(distinctAccountGroups, distinctRateTypes, steps);
					allocationInfo.Add(allocationID, info);

					foreach (PMAllocationDetail step in PXSelect<PMAllocationDetail, Where<PMAllocationDetail.allocationID, Equal<Required<PMAllocationDetail.allocationID>>>,
							OrderBy<Asc<PMAllocationDetail.stepID>>>.Select(this, allocationID))
					{
						if (step.Method == PMMethod.Budget)
							info.ContainsBudgetStep = true;
						foreach (int ag in GetAccountGroups(step))
						{
							distinctAccountGroups.Add(ag);
						}
						distinctRateTypes.Add(step.RateTypeID);
						steps.Add(step);
					}
				}
				else
				{
					distinctAccountGroups = info.AccountGroups;
					distinctRateTypes = info.RateTypes;
				}

				tryToAllocate = info.ContainsBudgetStep;
			}

			foreach (int distinctAccountGroup in distinctAccountGroups)
			{
				foreach (int projectID in distinctProjects)
				{
					foreach (PXResult<PMTran> tran in GetTranFromDatabase(projectID, distinctAccountGroup))
					{
						tryToAllocate = true;

						Dictionary<int, List<PXResult<PMTran>>> transactionsByAccountGroup;
						
						if (transactions.TryGetValue(((PMTran) tran).TaskID.Value, out transactionsByAccountGroup))
						{
							List<PXResult<PMTran>> trans;
							if (!transactionsByAccountGroup.TryGetValue(distinctAccountGroup, out trans))
							{
								trans = new List<PXResult<PMTran>>();
								transactionsByAccountGroup.Add(distinctAccountGroup, trans);
							}
							
							trans.Add(tran);
						}
					}
				}
			}

			if (!tryToAllocate)
				return false;

			rateEngine = CreateRateEngineV2(distinctRateTables.ToList(), distinctRateTypes.ToList());

			return true;
		}

		/// <summary>
		/// When overriding in customization return null in order for system to use RateEngineV1
		/// </summary>
		public virtual RateEngineV2 CreateRateEngineV2(IList<string> rateTables, IList<string> rateTypes)
		{
			return new RateEngineV2(this, rateTables, rateTypes);
		}

		/// <summary>
		/// Returns distinct account groups for the given step. 
		/// Method relies on pre-selected data stored in allocationInfo and accountGroups collections.
		/// Method do not query database.
		/// </summary>
		/// <param name="step">Allocation step</param>
		/// <returns></returns>
		public virtual HashSet<int> GetAccountGroups(PMAllocationDetail step)
		{
			HashSet<int> set = new HashSet<int>();
			if (step.SelectOption == PMSelectOption.Step)
			{
				foreach (PMAllocationDetail innerStep in GetSteps(step.AllocationID, step.RangeStart, step.RangeEnd))
				{
					foreach (int ag in GetAccountGroups(innerStep))
					{
						set.Add(ag);
					}
				}
			}
			else
			{
				foreach (int groupID in GetAccountGroupsRange(step))
					set.Add(groupID);
			}

			return set;
		}

		/// <summary>
		/// Returns distinct account groups for the Range defined in the given step. 
		/// Method relies on pre-selected data stored in accountGroups collection.
		/// Method do not query database.
		/// </summary>
		public virtual IList<int> GetAccountGroupsRange(PMAllocationDetail step)
		{
			List<int> list = new List<int>();

			if ((step.AccountGroupFrom != null && step.AccountGroupTo == null) ||
				 (step.AccountGroupFrom != null && step.AccountGroupTo == step.AccountGroupFrom))
			{
				list.Add(step.AccountGroupFrom.Value);
			}

			if (step.AccountGroupFrom == null && step.AccountGroupTo != null)
			{
				list.Add(step.AccountGroupFrom.Value);
			}

			if (step.AccountGroupTo != null && step.AccountGroupTo != step.AccountGroupFrom)
			{
				AccountGroup fromGroup;
				AccountGroup toGroup;

				if (!accountGroups.TryGetValue(step.AccountGroupFrom.Value, out fromGroup))
				{
					throw new PXException(Messages.AccountGroupInAllocationStepFromNotFound, step.AccountGroupFrom, step.StepID, step.AllocationID);
				}

				if (!accountGroups.TryGetValue(step.AccountGroupTo.Value, out toGroup))
				{
					throw new PXException(Messages.AccountGroupInAllocationStepToNotFound, step.AccountGroupTo, step.StepID, step.AllocationID);
				}

				foreach (AccountGroup ag in accountGroups.Values)
				{
					if (string.Compare(ag.GroupCD, fromGroup.GroupCD, StringComparison.InvariantCultureIgnoreCase) >= 0 && string.Compare(ag.GroupCD, toGroup.GroupCD, StringComparison.InvariantCultureIgnoreCase) <= 0)
					{
						list.Add(ag.GroupID);
					}
				}
			}

			return list;
		}

		/// <summary>
		/// Returns list of inner steps for the given Range.
		/// Method relies on pre-selected data stored in allocationInfo collection.
		/// Method do not query database.
		/// </summary>
		public virtual IList<PMAllocationDetail> GetSteps(string allocationID, int? rangeStart, int? rangeEnd)
		{
			List<PMAllocationDetail> list = new List<PMAllocationDetail>();
			foreach (PMAllocationDetail innerStep in allocationInfo[allocationID].Steps)
			{
				if (innerStep.StepID >= rangeStart && innerStep.StepID <= rangeEnd)
				{
					list.Add(innerStep);
				}
			}

			return list;
		}

		/// <summary>
		/// Returns RateDefinitions from Cached rateDefinitions collection or from database if not found.
		/// </summary>
		public virtual IList<PMRateDefinition> GetRateDefinitions(string rateTable)
		{
			List<PMRateDefinition> result;
			if (!string.IsNullOrEmpty(rateTable))
			{
				if (!rateDefinitions.TryGetValue(rateTable, out result))
				{
					PXSelectBase<PMRateDefinition> select = new PXSelect<PMRateDefinition,
				   Where<PMRateDefinition.rateTableID, Equal<Required<PMRateDefinition.rateTableID>>>,
				   OrderBy<Asc<PMRateDefinition.rateTypeID, Asc<PMRateDefinition.sequence>>>>(this);

					result = new List<PMRateDefinition>(select.Select(rateTable).RowCast<PMRateDefinition>());
					rateDefinitions.Add(rateTable, result);
				}
			}
			else
			{
				result = new List<PMRateDefinition>();
			}

			return result;
		} 

		public virtual bool ProcessStep(PMTask task, PMProject project, PMAllocationDetail step)
		{
			if (step.Post == true)
			{
				if (step.Method == PMMethod.Transaction)
				{
					List<PMTran> sourceList = Select(step, task.ProjectID, task.TaskID);
					if (sourceList.Count > 0)
					{
						List<PMTran> allocatedList = new List<PMTran>();
						Post(step, project, task, sourceList, allocatedList);
						AddSourceTrans(step, allocatedList);

						foreach (PMTran allocated in allocatedList)
						{
							allocated.Allocated = true;
							allocated.ExcludedFromAllocation = false;
							Transactions.Update(allocated);
						}
					}
				}
				else
				{
					List<PMTranWithTrace> allocated = ProcessBudgetStep(step, project, task);
					stepResults.Add(step.StepID.Value, allocated);

					return allocated.Count > 0;
				}
			}
			return false;
		}

		public virtual void Post(PMAllocationDetail step, PMProject project, PMTask task, List<PMTran> sourceList, List<PMTran> allocatedList)
		{
			AllocatedService allocService = new AllocatedService(this, task);

			List<PMTranWithTrace> allocated = null;
			if (step.FullDetail == true)
			{
				allocated = PostFullDetail(step, task, sourceList, allocatedList, allocService);
			}
			else
			{
				allocated = PostSummary(step, task, sourceList, allocatedList, allocService);
			}

			PostAllocatedTrans(step, project, allocated);
		}

		public virtual List<PMTranWithTrace> PostFullDetail(PMAllocationDetail step, PMTask task, List<PMTran> list, List<PMTran> allocatedList, AllocatedService allocService)
		{
			List<PMTranWithTrace> result = new List<PMTranWithTrace>(list.Count);

			foreach (PMTran original in list)
			{
				string note = null;
				Guid[] files = null;
				if (step.AllocateNonBillable == true || original.Billable == true)
				{
					original.Rate = GetRate(step, original, task.RateTableID);

					if (original.Rate == null)
					{
						//do not allocate option is selected.
						continue;
					}

					IList<PMTran> allocateCandidates = Transform(step, task, original, true, IsWipStep(step, task));

					foreach (PMTran allocateCandidate in allocateCandidates)
					{
						if (CanAllocate(step, allocateCandidate))
						{
							PMTranWithTrace item = new PMTranWithTrace(allocateCandidate, new List<long>(new long[] { original.TranID.Value }));
							note = PXNoteAttribute.GetNote(Transactions.Cache, original);
							files = PXNoteAttribute.GetFileNotes(Transactions.Cache, original);
							item.NoteText = note;
							item.Files = files;
							result.Add(item);

							allocatedList.Add(original);
						}
					}
				}
			}

			return result;
		}

		public bool IsWipStep(PMAllocationDetail step, PMTask task)
		{
			int? debitAccountGroupID = null;

			if (step.AccountGroupOrigin == PMOrigin.Change)
			{
				debitAccountGroupID = step.AccountGroupID;
			}
			else if (step.AccountGroupOrigin == PMOrigin.FromAccount)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, step.AccountID);
				if (account != null)
					debitAccountGroupID = account.AccountGroupID;
			}

			if (debitAccountGroupID != null && task.WipAccountGroupID != null)
				return debitAccountGroupID == task.WipAccountGroupID;
			
			return false;
		}

		public virtual List<PMTranWithTrace> PostSummary(PMAllocationDetail step, PMTask task, List<PMTran> fullList, List<PMTran> allocatedList, AllocatedService allocService)
		{
			List<Group> groups = BreakIntoGroups(step, fullList);
			List<PMTranWithTrace> result = new List<PMTranWithTrace>();

			foreach (Group group in groups)
			{
				PMDataNavigator navigator = new PMDataNavigator(this, group.List);
				List<long> originalTrans = new List<long>();
				
				decimal sumQty = 0;
				decimal sumBillableQty = 0;
				decimal sumAmt = 0;
				string lastDesc = null;
				DateTime? startDate = null;
				DateTime? endDate = null;
				foreach (PMTran tr in group.List)
				{
					if (startDate == null)
					{
						startDate = tr.StartDate;
					}
					else if (startDate > tr.StartDate)
					{
						startDate = tr.StartDate;
					}

					if (endDate == null)
					{
						endDate = tr.EndDate;
					}
					else if (endDate < tr.EndDate)
					{
						endDate = tr.EndDate;
					}

					decimal? qty = 0, billableQty = 0, amt = 0;
					string desc = null;

					tr.Rate = GetRate(step, tr, task.RateTableID);

					if (tr.Rate == null)
					{
						//do not allocate option is selected.
						break;//exit foreach; Do not process the second (credit) transaction if the debit was not allocated 
					}


					CalculateFormulas(navigator, step, tr, out qty, out billableQty, out amt, out desc);
					lastDesc = desc;

					decimal qtyInBase = qty ?? 0;
					if(tr.InventoryID != null && tr.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID && !string.IsNullOrEmpty(tr.UOM))
					{
						qtyInBase = ConvertQtyToBase(tr.InventoryID, tr.UOM, qty ?? 0);
					}


					originalTrans.Add(tr.TranID.Value);

					sumQty += qty.GetValueOrDefault();
					sumBillableQty += billableQty.GetValueOrDefault();
					sumAmt += amt.GetValueOrDefault();
				}

				if (group.List.Count > 0)
				{
					PMProject project = PMProject.PK.Find(this, task.ProjectID);
					IList<PMTran> allocs = Transform(step, task, group.List[0], false, IsWipStep(step, task));

					foreach (PMTran alloc in allocs)
					{
						alloc.Qty = alloc.IsInverted == true ? -sumQty : sumQty;
						alloc.BillableQty = alloc.IsInverted == true ? -sumBillableQty : sumBillableQty;
						alloc.StartDate = startDate;
						alloc.EndDate = endDate;

						decimal rawAmount;
						SetCalculatedAmount(project, alloc, alloc.IsInverted == true ? -CM.PXCurrencyAttribute.BaseRound(this, sumAmt) : CM.PXCurrencyAttribute.BaseRound(this, sumAmt), out rawAmount);

						if (alloc.BillableQty != 0)
						{
							decimal qty = PXDBQuantityAttribute.Round(alloc.BillableQty.Value);
							decimal unitRate = PXDBPriceCostAttribute.Round(rawAmount / qty);

							alloc.UnitRate = unitRate;
						}
						else if (alloc.Qty != 0)
						{
							decimal qty = PXDBQuantityAttribute.Round(alloc.Qty.Value);
							decimal unitRate = PXDBPriceCostAttribute.Round(rawAmount / qty);

							alloc.TranCuryUnitRate = unitRate;
							alloc.UnitRate = unitRate;
						}

						if (group.HasMixedInventory)
						{
							alloc.InventoryID = PMInventorySelectorAttribute.EmptyInventoryID; //mixed inventory in components
						}
						if (group.HasMixedUOM)
						{
							alloc.Qty = 0;
							alloc.BillableQty = 0;
							alloc.UOM = null;
							alloc.UnitRate = 0;
						}
						if (group.HasMixedBAccount)
						{
							alloc.BAccountID = null;
							alloc.LocationID = null;
						}

						if (group.HasMixedBAccountLoc)
						{
							alloc.LocationID = null;
						}
						if (lastDesc == null && group.HasMixedDescription)
						{
							alloc.Description = GetConcatenatedDescription(step, alloc, task);
						}
						else
						{
							alloc.Description = lastDesc;
						}

						if (CanAllocate(step, alloc))
						{
							PMTranWithTrace item = new PMTranWithTrace(alloc, originalTrans);
							List<Guid> files = new List<Guid>();
							foreach (PMTran original in group.List)
							{
								files.AddRange(PXNoteAttribute.GetFileNotes(Transactions.Cache, original));
								allocatedList.Add(original);
							}
							item.Files = files.ToArray();
							result.Add(item);
						}
					}
				}
			}

			return result;
		}
		
	   public virtual void PostAllocatedTrans(PMAllocationDetail step, PMProject project, List<PMTranWithTrace> allocated)
		{
			foreach (PMTranWithTrace twt in allocated)
			{
				if (Document.Current == null)
					AddAllocationDocument(project);

				twt.Tran = Transactions.Insert(twt.Tran); //TranID is initialized -1,-2,-3, etc.
				twt.Tran.BatchNbr = null;//allocation can be called from journal entry and BatchNbr will be defaulted.
				if (twt.NoteText != null)
					PXNoteAttribute.SetNote(Transactions.Cache, twt.Tran, twt.NoteText);
				if (twt.Files != null && twt.Files.Length > 0)
					PXNoteAttribute.SetFileNotes(Transactions.Cache, twt.Tran, twt.Files);

				if (twt.Tran.OrigAccountGroupID != null && twt.Tran.InventoryID != null)
				{
					PMTaskAllocTotalAccum ta = new PMTaskAllocTotalAccum();
					ta.ProjectID = twt.Tran.OrigProjectID;
					ta.TaskID = twt.Tran.OrigTaskID;
					ta.AccountGroupID = twt.Tran.OrigAccountGroupID;
					ta.InventoryID = twt.Tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
					ta.CostCodeID = twt.Tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
					if (ta.ProjectID == null)
						throw new PXException(Messages.ProjectIsNullAfterAllocation, step.StepID);

					if (ta.TaskID == null)
						throw new PXException(Messages.TaskIsNullAfterAllocation, step.StepID);

					if (ta.AccountGroupID == null)
						throw new PXException(Messages.AccountGroupIsNullAfterAllocation, step.StepID);
					
					ta = TaskTotals.Insert(ta);
					ta.Amount += twt.Tran.ProjectCuryAmount;
					ta.Quantity += twt.Tran.BillableQty;
				}
			}

			stepResults.Add(step.StepID.Value, allocated);
		}

		public virtual List<PMTranWithTrace> ProcessBudgetStep(PMAllocationDetail step, PMProject project, PMTask task)
		{
			List<PMTranWithTrace> tranAdded = new List<PMTranWithTrace>();
			decimal? taskCompletedPct = task.CompletedPercent;

			AllocatedService allocService = new AllocatedService(this, task);

			PXSelectBase<PMBudget> selectBudget = new PXSelectGroupBy<PMBudget,
				   Where<PMBudget.projectID, Equal<Required<PMBudget.projectID>>,
				   And<PMBudget.projectTaskID, Equal<Required<PMBudget.projectTaskID>>,
				   And<PMBudget.accountGroupID, Equal<Required<PMBudget.accountGroupID>>>>>,
				   Aggregate<
				   GroupBy<PMBudget.accountGroupID,
				   GroupBy<PMBudget.projectID,
				   GroupBy<PMBudget.projectTaskID,
				   GroupBy<PMBudget.inventoryID,
				   GroupBy<PMBudget.costCodeID,
				   Sum<PMBudget.curyAmount,
				   Sum<PMBudget.qty,
				   Sum<PMBudget.curyRevisedAmount,
				   Sum<PMBudget.revisedQty,
				   Sum<PMBudget.actualAmount,
				   Sum<PMBudget.actualQty>>>>>>>>>>>>>(this);

			List<PMAllocator.AllocData> data = new List<PMAllocator.AllocData>();
			Dictionary<int, decimal> uncapTotalAmtByItem = new Dictionary<int, decimal>();
			Dictionary<int, decimal> uncapTotalQtyByItem = new Dictionary<int, decimal>();
			decimal uncapTotal = 0;

			foreach (int groupID in GetAccountGroupsRange(step))
			{
				foreach (PMBudget budget in selectBudget.Select(task.ProjectID, task.TaskID, groupID))
				{
					if (taskCompletedPct >= 0 && budget.CuryRevisedAmount > 0)
					{
						PMAllocator.AllocData ad = new PMAllocator.AllocData(budget.AccountGroupID.Value, budget.InventoryID.Value, budget.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
						ad.Amount = PX.Objects.CM.PXCurrencyAttribute.BaseRound(this, budget.CuryRevisedAmount.Value * taskCompletedPct.Value * 0.01m);
						ad.Quantity = budget.RevisedQty.Value * taskCompletedPct.Value * 0.01m;
						ad.UOM = budget.UOM;
						data.Add(ad);

						uncapTotal += ad.Amount;

						if (uncapTotalAmtByItem.ContainsKey(budget.InventoryID.Value))
							uncapTotalAmtByItem[budget.InventoryID.Value] += ad.Amount;
						else
							uncapTotalAmtByItem.Add(budget.InventoryID.Value, ad.Amount);



						decimal qtyInBase = ad.Quantity;
						if (ad.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID && ad.UOM != null)
						{
							qtyInBase = ConvertQtyToBase(ad.InventoryID, ad.UOM, ad.Quantity);
						}

						if (uncapTotalQtyByItem.ContainsKey(budget.InventoryID.Value))
						{
							uncapTotalQtyByItem[budget.InventoryID.Value] += qtyInBase;
						}
						else
						{
							uncapTotalQtyByItem.Add(budget.InventoryID.Value, qtyInBase);
						}
					}
				}
			}
			
			foreach (PMAllocator.AllocData ad in data)
			{
				decimal coeff = 1;
								
				decimal unallocatedAmt = PX.Objects.CM.PXCurrencyAttribute.BaseRound(this, ad.Amount * coeff - allocService.GetAllocatedAmt(ad.AccountGroupID, ad.InventoryID, ad.CostCodeID));
				decimal quantity = 0;

				if (ad.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					quantity = ad.Quantity * coeff - allocService.GetAllocatedQty(ad.AccountGroupID, ad.InventoryID, ad.CostCodeID);
				}

				if (unallocatedAmt != 0)
				{
					tranAdded.AddRange(AllocateBudget(task, project, step, ad.AccountGroupID, ad.InventoryID, ad.CostCodeID, ad.UOM, unallocatedAmt, quantity));
				}
			}

			return tranAdded;
		}

		public virtual List<PMTranWithTrace> AllocateBudget(PMTask task, PMProject project, PMAllocationDetail step, int origAccountGroupID, int inventoryID, int costCodeID, string UOM, decimal amount, decimal quantity)
		{
			List<PMTranWithTrace> tranAdded = new List<PMTranWithTrace>();
			/*
				preconditions:
			 * ProjectID both for debit and credit is always = source.
			 * TaskID both for debit and credit is always = source.
			 * Single-sided transactions are allowed for UpdateGL == false.
			 * 
			*/

			bool additionalNonGLCreditTranIsRequired = false;
			int mult = 1;//sets sign for the transaction. since single-sided tran can only be debit, use mult=-1 to credit an account.

			if (Document.Current == null)
				AddAllocationDocument(project);

			PMTran tran = new PMTran();
			if (PostingDate != null)
			{
				tran.Date = PostingDate;
			}
			SetTransactionBranch(tran, step, project.DefaultBranchID, task.DefaultBranchID);

			tran = Transactions.Insert(tran);			
			tran.ProjectID = task.ProjectID;
			tran.TaskID = task.TaskID;
			tran.UOM = UOM;
			tran.BAccountID = task.CustomerID;
			tran.Billable = true;
			tran.UseBillableQty = true;
			tran.BillableQty = quantity;
			tran.InventoryID = inventoryID;
			tran.CostCodeID = costCodeID;
			tran.LocationID = task.LocationID;
			tran.Qty = tran.BillableQty;
			tran.ProjectCuryID = project.CuryID;
			tran.TranCuryID = project.CuryID;			
			tran.TranCuryAmount = amount;
			tran.ProjectCuryAmount = amount;

			
			if (PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
			{
				CM.CMSetup cmSetup = PXSelect<CM.CMSetup>.Select(this);

				CurrencyInfo projectCuryInfo = GetExtension<MultiCurrency>().currencyinfo.Insert(new CurrencyInfo
				{
					ModuleCode = GL.BatchModule.PM,
					BaseCuryID = project.CuryID,
					CuryID = project.CuryID,
					CuryRateTypeID = project.RateTypeID ?? cmSetup?.PMRateTypeDflt,
					CuryEffDate = tran.TranDate,
					CuryRate = 1,
					RecipRate = 1
				});

				tran.ProjectCuryInfoID = projectCuryInfo.CuryInfoID;
			}

			if (tran.Qty != null && tran.Qty != 0)
			{
				decimal unitRate = PXDBPriceCostAttribute.Round(tran.Amount.GetValueOrDefault() / tran.Qty.Value);

				tran.TranCuryUnitRate = unitRate;
				tran.UnitRate = unitRate;
			}

			if (project.CuryID == project.BaseCuryID)
			{
				tran.Amount = tran.TranCuryAmount;
				tran.UnitRate = tran.TranCuryUnitRate;
			}
			
			tran.AllocationID = step.AllocationID;
			tran.IsNonGL = step.UpdateGL == false;
			tran.AccountGroupID = origAccountGroupID;
			tran.Reverse = step.Reverse;
			tran.CreatedByCurrentAllocation = true;

			if (step.UpdateGL == true)
			{
				#region Debit/Credit

				if (step.AccountGroupOrigin == PMOrigin.Change)
				{
					tran.AccountGroupID = step.AccountGroupID;
				}
				else if (step.AccountGroupOrigin == PMOrigin.FromAccount)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, step.AccountID);
					if (account != null)
						tran.AccountGroupID = account.AccountGroupID;
				}

				if (step.ProjectOrigin == PMOrigin.Change)
				{
					tran.ProjectID = step.ProjectID;
				}
				else
				{
					tran.ProjectID = task.ProjectID;
				}

				if (step.TaskOrigin == PMOrigin.Change)
				{
					PMTask pmTask;
					if (step.ProjectID == null)
					{
						pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.TaskCD, tran.ProjectID);
						tran.TaskID = pmTask.TaskID;
					}
					else
					{
						tran.TaskID = step.TaskID;
					}
				}

				switch (step.AccountOrigin)
				{
					case PMOrigin.Change:
						tran.AccountID = step.AccountID;
						break;
				}

				tran.SubID = step.SubID ?? task.DefaultSalesSubID;

				if (step.OffsetAccountOrigin == PMOrigin.Change)
				{
					tran.OffsetAccountID = step.OffsetAccountID;
				}

				tran.OffsetSubID = step.OffsetSubID;

				if (tran.OffsetAccountID != null && tran.OffsetSubID == null)
				{
					tran.OffsetSubID = tran.SubID;
				}

				//Make Subaccount:

				object value = PMSubAccountMaskAttribute.MakeSub(this, step, tran, project, task);
				Transactions.Cache.RaiseFieldUpdating<PMTran.subID>(tran, ref value);
				tran.SubID = (int?)value;


				if (tran.OffsetAccountID != null)
				{
					object offsetValue = PMOffsetSubAccountMaskAttribute.MakeSub(this, step, tran, project, task);
					Transactions.Cache.RaiseFieldUpdating<PMTran.offsetSubID>(tran, ref offsetValue);
					tran.OffsetSubID = (int?)offsetValue;
				}

				#endregion
			}
			else
			{
				if (step.AccountGroupOrigin == PMOrigin.None)
				{
					//Single-sided transaction:

					//Take account group from Credit and change the sign of amount and qty.
					if (step.OffsetAccountGroupOrigin == PMOrigin.Change)
					{
						tran.AccountGroupID = step.OffsetAccountGroupID;
					}

					if (step.OffsetProjectOrigin == PMOrigin.Change)
					{
						tran.ProjectID = step.OffsetProjectID;
					}
					else
					{
						tran.ProjectID = task.ProjectID;
					}

					if (step.OffsetTaskOrigin == PMOrigin.Change)
					{
						PMTask pmTask;
						if (step.OffsetProjectID == null)
						{
							pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.OffsetTaskCD, tran.ProjectID);
							tran.TaskID = pmTask.TaskID;
						}
						else
						{
							tran.TaskID = step.OffsetTaskID;
						}
					}

					mult = -1;
				}
				else
				{
					if (step.AccountGroupOrigin == PMOrigin.Change)
					{
						tran.AccountGroupID = step.AccountGroupID;
					}

					if (step.ProjectOrigin == PMOrigin.Change)
					{
						tran.ProjectID = step.ProjectID;
					}
					else
					{
						tran.ProjectID = task.ProjectID;
					}

					if (step.TaskOrigin == PMOrigin.Change)
					{
						PMTask pmTask;
						if (step.ProjectID == null)
						{
							pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.TaskCD, tran.ProjectID);
							tran.TaskID = pmTask.TaskID;
						}
						else
						{
							tran.TaskID = step.TaskID;
						}
					}

					if (step.OffsetAccountGroupID != null)
					{
						additionalNonGLCreditTranIsRequired = true;
						tran.OffsetAccountGroupID = step.OffsetAccountGroupID;
					}
				}
			}

			tran.ExcludedFromAllocation = step.MarkAsNotAllocated != true;
			tran.OrigProjectID = task.ProjectID;
			tran.OrigTaskID = task.TaskID;
			tran.OrigAccountGroupID = origAccountGroupID;

			PMDataNavigator navigator = new PMDataNavigator(this, new List<PMTran>(new PMTran[1] { tran }));
			
			decimal? qty, billableQty, amt;
			string desc;
			try
			{
				IsBudgetAllocation = true;
				CalculateFormulas(navigator, step, tran, out qty, out billableQty, out amt, out desc);
				//!!!Amount and Qty should not be calculated by formula for budget allocation!!!  
			}
            finally
            {
				IsBudgetAllocation = false;
			}

			tran.Description = desc;
						
			try
			{
				tran = Transactions.Update(tran);
				tranAdded.Add(new PMTranWithTrace(tran, new List<long>()));
			}
			catch (PXFieldProcessingException ex)
			{
				if (ex.FieldName.Equals(Transactions.Cache.GetField(typeof(PMTran.locationID)), StringComparison.InvariantCultureIgnoreCase))
				{
					throw new PXException( PXMessages.LocalizeFormatNoPrefix(Messages.LocationNotFound, ex.Message));
				}
				else
				{
					throw new PXException(PXMessages.LocalizeFormatNoPrefix(Messages.GenericFieldErrorOnAllocation, ex.Message));
				}
			}

			PMTaskAllocTotalAccum ta = new PMTaskAllocTotalAccum();
			ta.ProjectID = tran.OrigProjectID;
			ta.TaskID = tran.OrigTaskID;
			ta.AccountGroupID = tran.OrigAccountGroupID;
			ta.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
			ta.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

			ta = TaskTotals.Insert(ta);
			ta.Amount += tran.ProjectCuryAmount;
			ta.Quantity += tran.BillableQty;

			//Apply sign correction 
			tran.Amount *= mult;
			tran.TranCuryAmount *= mult;
			tran.ProjectCuryAmount *= mult;
			tran.Qty *= mult;
			tran.BillableQty *= mult;

			if (additionalNonGLCreditTranIsRequired)
			{
				PMTran creditTran = PXCache<PMTran>.CreateCopy(tran);
				creditTran.AccountGroupID = step.OffsetAccountGroupID;
				
				if (step.OffsetProjectOrigin == PMOrigin.Change)
				{
					tran.ProjectID = step.OffsetProjectID;
				}

				if (step.OffsetTaskOrigin == PMOrigin.Change)
				{
					PMTask pmTask;
					if (step.OffsetProjectID == null)
					{
						pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.OffsetTaskCD, tran.ProjectID);
						tran.TaskID = pmTask.TaskID;
					}
					else
					{
						tran.TaskID = step.OffsetTaskID;
					}
				}

				creditTran.OrigProjectID = null;
				creditTran.OrigAccountGroupID = null;
				creditTran.TranID = null;
				creditTran.NoteID = null;

				creditTran.Qty = -creditTran.Qty;
				creditTran.BillableQty = -creditTran.BillableQty;
				creditTran.TranCuryAmount = -creditTran.TranCuryAmount;
				creditTran.TranCuryAmountCopy = null;
				creditTran.ProjectCuryAmount = -creditTran.ProjectCuryAmount;
				creditTran.Amount = -creditTran.Amount;
				
				creditTran.Billable = true;
				creditTran.UseBillableQty = true;
				creditTran.CreatedByCurrentAllocation = true;

				creditTran = Transactions.Insert(creditTran);
				tranAdded.Add(new PMTranWithTrace(creditTran, new List<long>()));
			}

			return tranAdded;
		}

		public virtual void SetTransactionBranch(PMTran tran, PMAllocationDetail step, int? ProjectBranchID, int? TaskBranchID)
		{
			if (step.Method == PMMethod.Budget && step.OffsetBranchOrigin == PMOrigin.Source)
			{
				tran.BranchID = TaskBranchID ?? ProjectBranchID ?? Accessinfo.BranchID;
			}
			else
			{
				tran.BranchID = step.TargetBranchID ?? Accessinfo.BranchID;
			}
		}

		public virtual void AddSourceTrans(PMAllocationDetail step, List<PMTran> sourceTrans)
		{
			foreach (PMTran sourceTran in sourceTrans)
			{
				PMAllocationSourceTran allocationTran = CreateAllocationTran(step.AllocationID, step.StepID, sourceTran);
				SourceTran.Insert(allocationTran);
			}
		}

		public virtual void AddAuditTran(string allocationID, long? tranID, List<long> sourceTrans)
		{
			foreach (long sourceTranID in sourceTrans)
			{
				PMAllocationAuditTran at = new PMAllocationAuditTran();
				at.AllocationID = allocationID;
				at.SourceTranID = sourceTranID;
				at.TranID = tranID;

				AuditTran.Insert(at);
			}
		}

		public virtual void AddAllocationDocument(PMProject project)
		{
			Document.Cache.Insert();
			Document.Current.OrigDocType = PMOrigDocType.Allocation;
			Document.Current.Description = PXMessages.LocalizeFormatNoPrefix(Messages.AllocationForProject, project.ContractCD);
			Document.Current.IsAllocation = true;
		}
						
		public virtual string GetConcatenatedDescription(PMAllocationDetail step, PMTran tran, PMTask task)
		{
			string result = "";
			if (step.GroupByDate == true)
				result = string.Format("{0}: ", tran.Date);
			if (step.GroupByItem == true)
			{
				Customer customer = null;
				if (task?.CustomerID != null)
				{
					customer = PXSelectReadonly<AR.Customer, Where<AR.Customer.bAccountID, Equal<Required<AR.Customer.bAccountID>>>>.Select(this, task.CustomerID);
				}

				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, tran.InventoryID);
				if (item != null)
				{
					using (new PXLocaleScope(customer?.LocaleName))
					{
						result += string.Format("{0} {1}", item.InventoryCD, this.Caches[typeof(InventoryItem)].GetValueExt<InventoryItem.descr>(item));
					}
				}
			}
			else if (step.GroupByEmployee == true)
			{
				BAccount employee = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.Select(this, tran.ResourceID);
				if (employee != null)
				{
					result += string.Format("{0}", employee.AcctCD);
				}
			}

			return result;
		}

		public virtual List<Group> BreakIntoGroups(PMAllocationDetail step, List<PMTran> fullList)
		{
			List<PMTran> list;
			if (step.AllocateNonBillable != true)
			{
				list = new List<PMTran>(fullList.Count);
				foreach (PMTran tran in fullList)
				{
					if (tran.Billable == true)
					{
						list.Add(tran);
					}
				}
			}
			else
			{
				list = fullList;
			}

			Grouping grouping = CreateGrouping(step);
			return grouping.BreakIntoGroups(list);
		}

		public virtual Grouping CreateGrouping(PMAllocationDetail step)
		{
			PMTranComparer comparer = new PMTranComparer(step.GroupByItem, step.GroupByVendor, step.GroupByDate, step.GroupByEmployee, step.AccountGroupOrigin == PMOrigin.Source || step.OffsetAccountGroupOrigin == PMOrigin.Source);

			return new Grouping(comparer);
		}

		public virtual bool CanAllocate(PMAllocationDetail step, PMTran tran)
		{
			bool result = false;

			if (step.AllocateZeroQty == true || tran.Qty != 0)
			{
				result = true;
			}

			if (step.AllocateZeroAmount == true || tran.Amount != 0)
			{
				result = true;
			}

			return result;
		}

		protected bool IsBudgetAllocation = false;
		public virtual object Evaluate(PMObjectType objectName, string fieldName, string attribute, PMTran row)
		{
			switch (objectName)
			{
				case PMObjectType.PMTran:
					return ConvertFromExtValue(this.Caches[typeof(PMTran)].GetValueExt(row, fieldName));
				case PMObjectType.PMBudget:
					PXSelectBase<PMBudget> selectBudget = new PXSelect<PMBudget,
						Where<PMBudget.accountGroupID, Equal<Required<PMBudget.accountGroupID>>,
						And<PMBudget.projectID, Equal<Required<PMBudget.projectID>>,
						And<PMBudget.projectTaskID, Equal<Required<PMBudget.projectTaskID>>,
						And<PMBudget.inventoryID, Equal<Required<PMBudget.inventoryID>>,
						And<PMBudget.costCodeID, Equal<Required<PMBudget.costCodeID>>>>>>>>(this);
					int? accountGroupID = row.AccountGroupID;
					if (IsBudgetAllocation && row.OrigAccountGroupID != null)
                    {
						accountGroupID = row.OrigAccountGroupID;
                    }

					PMProject projectData = PMProject.PK.Find(this, row.ProjectID);
					PMAccountGroup accountGroupData = PMAccountGroup.PK.Find(this, accountGroupID);
					bool isExisting;
					BudgetService budgetService = new BudgetService(this);
					PMBudgetLite budgetLite = budgetService.SelectProjectBalance(accountGroupData, projectData, row.TaskID, row.InventoryID, row.CostCodeID, out isExisting);

					PMBudget budget = selectBudget.Select(accountGroupID, row.ProjectID, row.TaskID, budgetLite.InventoryID, budgetLite.CostCodeID);
					if (budget != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(PMBudget)].GetValueExt(budget, fieldName));
					}
					break;
				case PMObjectType.PMProject:
					PMProject project = PMProject.PK.Find(this, row.ProjectID);
					if (project != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, project.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(PMProject)].GetValueExt(project, fieldName));
						}
					}
					break;
				case PMObjectType.PMTask:
					PMTask task = PMTask.PK.FindDirty(this, row.ProjectID, row.TaskID);
					if (task != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, task.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(PMTask)].GetValueExt(task, fieldName));
					}
					break;
				case PMObjectType.PMAccountGroup:
					PMAccountGroup accGroup = PMAccountGroup.PK.Find(this, row.AccountGroupID);
					if (accGroup != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, accGroup.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(PMAccountGroup)].GetValueExt(accGroup, fieldName));
					}
					break;
				case PMObjectType.EPEmployee:
					EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.ResourceID);
					if (employee != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, employee.NoteID);

						}
						else
							return ConvertFromExtValue(this.Caches[typeof(EPEmployee)].GetValueExt(employee, fieldName));
					}
					break;
				case PMObjectType.Customer:
					Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
					if (customer != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, customer.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(Customer)].GetValueExt(customer, fieldName));
					}
					break;
				case PMObjectType.Vendor:
					VendorR vendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Required<VendorR.bAccountID>>>>.Select(this, row.BAccountID);
					if (vendor != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, vendor.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(VendorR)].GetValueExt(vendor, fieldName));
					}
					break;
				case PMObjectType.InventoryItem:
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
					if (item != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, item.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(InventoryItem)].GetValueExt(item, fieldName));
					}
					break;
				default:
					break;
			}

			return null;
		}
				
		public virtual decimal? GetPrice(PMTran tran)
		{
			decimal? result = null;

			if (tran.InventoryID != null && tran.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;

				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, tran.ProjectID);
				PMTask projectTask = (PMTask)PXSelectorAttribute.Select(Caches[typeof(PMTran)], tran, "TaskID");
				CR.Location c = (CR.Location)PXSelectorAttribute.Select(Caches[typeof(PMTask)], projectTask, "LocationID");
				if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

				CurrencyInfo dummy = GetCurrencyInfo(project, tran.TranCuryID, tran.Date);
				bool alwaysFromBase = false;
				string taxCalcMode = c?.CTaxCalcMode ?? TX.TaxCalculationMode.TaxSetting;
				result = ARSalesPriceMaint.CalculateSalesPrice(Caches[typeof(PMTran)], customerPriceClass, projectTask.CustomerID, tran.InventoryID, dummy.GetCM(), tran.Qty, tran.UOM, tran.Date.Value, alwaysFromBase, taxCalcMode);

				if (alwaysFromBase != true && result == null)
				{
					result = ARSalesPriceMaint.CalculateSalesPrice(Caches[typeof(PMTran)], customerPriceClass, projectTask.CustomerID, tran.InventoryID, dummy.GetCM(), tran.Qty, tran.UOM, tran.Date.Value, true, taxCalcMode);
				}
			}

			return result;
		}

		public virtual CurrencyInfo GetCurrencyInfo(PMProject project, string curyID, DateTime? effectiveDate)
		{
			CurrencyInfo curyInfo = new CurrencyInfo
			{
				ModuleCode = GL.BatchModule.PM,
				BaseCuryID = project.BaseCuryID
			};

			if (curyID == project.BaseCuryID)
			{
				//Ex: Base=USD, TranCuryID = USD
				curyInfo.CuryID = project.BaseCuryID;
				curyInfo.CuryRate = 1;
				curyInfo.RecipRate = 1;
				curyInfo.CuryMultDiv = CuryMultDivType.Mult;
			}
			else if (project.CuryID == curyID && curyID != project.BaseCuryID)
			{
				//Ex: Base=USD, TranCuryID = EUR, ProjectCuryID = EUR
				//use the fxRate of the Project,
				curyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, project.CuryInfoID);
			}
			else
			{
				//Ex: Base=USD, TranCuryID = CAD, ProjectCuryID = USD/EUR
				CM.CMSetup cmsetup = PXSelect<CM.CMSetup>.Select(this);
				string rateType = project.RateTableID ?? cmsetup?.PMRateTypeDflt;

				if (effectiveDate == null)
					effectiveDate = Accessinfo.BusinessDate ?? DateTime.Now;

				curyInfo.CuryID = curyID;
				curyInfo.CuryRateTypeID = rateType;
				curyInfo.CuryEffDate = effectiveDate;
				curyInfo.SearchForNewRate(this)?.Populate(curyInfo);
			}

			return curyInfo;
		}

		public virtual object ConvertFromExtValue(object extValue)
		{
			PXFieldState fs = extValue as PXFieldState;
			if (fs != null)
				return fs.Value;
			else
			{
				return extValue;
			}
		}

		public virtual object EvaluateAttribute(string attribute, Guid? refNoteID)
		{
		    PXResultset<CSAnswers> res = PXSelectJoin<CSAnswers,
		        InnerJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>>>,
		        Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
		            And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(this, refNoteID, attribute);

			CSAnswers ans = null;
			CSAttribute attr = null;
			if (res.Count > 0)
			{
				ans = (CSAnswers)res[0][0];
				attr = (CSAttribute)res[0][1];
			}

			if (ans == null || ans.AttributeID == null)
			{
				//answer not found. if attribute exists return the default value.
				attr = PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>.Select(this, attribute);

				if (attr != null && attr.ControlType == CSAttribute.CheckBox)
                {
					return false;
                }
            }
			
			if (ans != null)
			{
				if (ans.Value != null)
					return ans.Value;
				else
				{
					if (attr != null && attr.ControlType == CSAttribute.CheckBox)
					{
						return false;
					}
				}
			}
				
			return string.Empty;
		}

		public virtual IList<PMTran> Transform(PMAllocationDetail step, PMTask task, PMTran original, bool calculateFormulas, bool isWipStep)
		{
			List<PMTran> list = new List<PMTran>();

			int? debitAccountGroup = GetDebitAccountGroupFromStep(step, original);
			int? creditAccountGroup = GetCreditAccountGroupFromStep(step, original);
			int? debitProjectID = GetDebitProjectFromStep(step, original);
			int? creditProjectID = GetCreditProjectFromStep(step, original);
			int? debitTaskID = GetDebitTaskFromStep(step, task, debitProjectID);
			int? creditTaskID = GetCreditTaskFromStep(step, task, creditProjectID);
			decimal? qty = null;
			decimal? billableQty = null;
			decimal? amt = null;
			string desc = null;

			if (debitAccountGroup == null)
			{
				PXTrace.WriteError(Messages.InvalidAllocationRule, step.StepID, task.TaskCD);
				throw new PXException(Messages.InvalidAllocationRule, step.StepID, task.TaskCD);
			}

			if (calculateFormulas)
			{
				PMDataNavigator navigator = new PMDataNavigator(this, new List<PMTran>(new PMTran[1] { original }));
				CalculateFormulas(navigator, step, original, out qty, out billableQty, out amt, out desc);
			}

			bool doubleEntry = false;

			if (step.UpdateGL != true)
			{
				doubleEntry = true;
			}
			else
			{
				creditProjectID = debitProjectID;
				creditTaskID = debitTaskID;
			}

			if (doubleEntry)
			{
				#region Debit Tran
				PMProject debitProject = PMProject.PK.Find(this, debitProjectID);
				PMTask debitTask = PMTask.PK.FindDirty(this, debitProjectID, debitTaskID);

				PMTran debitTran = CreateFromTemplate(step, original, isWipStep);
				debitTran.ExcludedFromAllocation = step.MarkAsNotAllocated != true;
				debitTran.AccountGroupID = debitAccountGroup;
				if (step.OffsetAccountGroupOrigin != PMOrigin.None)
					debitTran.OffsetAccountGroupID = creditAccountGroup;
				debitTran.ProjectID = debitProjectID;
				debitTran.TaskID = debitTaskID;
				debitTran.CreatedByCurrentAllocation = true;
				SetCurrencyInfoOnProjectChange(debitTran, original);

				if (CostCodeAttribute.UseCostCode())
				{
					if (original.CostCodeID == null)
					{
						debitTran.CostCodeID = CostCodeAttribute.DefaultCostCode;
					}
				}

				if (step.UpdateGL == true)
				{
					switch (step.AccountOrigin)
					{
						case PMOrigin.Change:
							debitTran.AccountID = step.AccountID;
							break;
						case PMOrigin.OtherSource:
							debitTran.AccountID = original.OffsetAccountID;
							break;
						default://Source
							debitTran.AccountID = original.AccountID;
							break;
					}

					//Make Subaccount:
					if (original.SubID != null)
					{
						PMSubAccountMaskAttribute.VerifyMask(step, original, debitProject, task);

						object value = PMSubAccountMaskAttribute.MakeSub(this, step, original, debitProject, task);

						Transactions.Cache.RaiseFieldUpdating<PMTran.subID>(debitTran, ref value);
						debitTran.SubID = (int?)value;
					}
					else
					{
						debitTran.SubID = step.SubID;
					}
				}

				if (calculateFormulas)
				{
					debitTran.Description = desc;
					debitTran.BillableQty = billableQty;
					debitTran.Qty = qty;

					decimal rawAmount;
					SetCalculatedAmount(debitProject, debitTran, amt.GetValueOrDefault(), out rawAmount);
					
					if (billableQty != null && billableQty != 0)
					{
						debitTran.Billable = true;
						debitTran.UseBillableQty = true;
						debitTran.UnitRate = rawAmount  / billableQty.Value;
						debitTran.TranCuryUnitRate = amt.GetValueOrDefault() / billableQty.Value;
					}					
				}
				list.Add(debitTran);
				#endregion

				#region Credit Tran

				if (creditAccountGroup != null)
				{
					PMProject creditProject = PMProject.PK.Find(this, creditProjectID);
					PMTask creditTask = PMTask.PK.FindDirty(this, creditProjectID, creditTaskID);

					PMTran creditTran = CreateFromTemplate(step, original, isWipStep);
					creditTran.ExcludedFromAllocation = step.MarkAsNotAllocated != true;
					creditTran.AccountGroupID = creditAccountGroup;
					creditTran.ProjectID = creditProjectID;
					creditTran.TaskID = creditTaskID;
					creditTran.IsInverted = true;
					creditTran.OrigAccountGroupID = null; //Skip recording this tran in PMTaskAllocTotal otherwise amount will always be 0.
					creditTran.IsCreditPair = true;//Caps should not be applied to this tran.
					creditTran.CreatedByCurrentAllocation = true;
					SetCurrencyInfoOnProjectChange(creditTran, original);

					if (CostCodeAttribute.UseCostCode())
					{
						if (original.CostCodeID == null)
						{
							creditTran.CostCodeID = CostCodeAttribute.DefaultCostCode;
						}
					}

					if (step.UpdateGL == true)
					{
						switch (step.OffsetAccountOrigin)
						{
							case PMOrigin.Change:
								creditTran.AccountID = step.OffsetAccountID;
								break;
							case PMOrigin.OtherSource:
								creditTran.AccountID = original.AccountID;
								break;
							default://Source
								creditTran.AccountID = original.OffsetAccountID;
								break;
						}

						//Make Subaccount:
						if (original.SubID != null)
						{
							PMOffsetSubAccountMaskAttribute.VerifyMaskReversed(step, original, creditProject, creditTask);

							object value = PMOffsetSubAccountMaskAttribute.MakeSubReversed(this, step, original, creditProject, creditTask);

							Transactions.Cache.RaiseFieldUpdating<PMTran.offsetSubID>(creditTran, ref value);
							creditTran.SubID = (int?)value;
						}
						else
						{
							creditTran.SubID = step.SubID;
						}
					}

					if (calculateFormulas)
					{
						creditTran.Description = desc;
						creditTran.BillableQty = -billableQty;
						creditTran.Qty = -qty;

						decimal rawAmount;
						SetCalculatedAmount(creditProject, creditTran, -amt.GetValueOrDefault(), out rawAmount);

						if (billableQty != null && billableQty != 0)
						{
							creditTran.Billable = true;
							creditTran.UseBillableQty = true;
							creditTran.UnitRate = rawAmount / billableQty.Value;
							creditTran.TranCuryUnitRate = -amt.GetValueOrDefault() / billableQty.Value;
						}
					}

					list.Add(creditTran);
				}
				#endregion
			}
			else
			{
				#region Debit/Credit Tran
				PMProject debitProject = PMProject.PK.Find(this, debitProjectID);
				PMTask debitTask = PMTask.PK.FindDirty(this, debitProjectID, debitTaskID);

				PMTran tran = CreateFromTemplate(step, original, isWipStep);
				tran.ExcludedFromAllocation = step.MarkAsNotAllocated != true;
				tran.AccountGroupID = debitAccountGroup;
				tran.ProjectID = debitProjectID;
				tran.TaskID = debitTaskID;
				tran.CreatedByCurrentAllocation = true;
				SetCurrencyInfoOnProjectChange(tran, original);

				if (CostCodeAttribute.UseCostCode())
				{
				
					if (original.CostCodeID == null)
					{
						tran.CostCodeID = CostCodeAttribute.DefaultCostCode;
					}
				}

				#region Set Debit/Credit Accounts and Subs
				if (step.UpdateGL == true)
				{
					switch (step.AccountOrigin)
					{
						case PMOrigin.Change:
							tran.AccountID = step.AccountID;
							tran.SubID = original.SubID ?? step.SubID;
							break;
						case PMOrigin.OtherSource:
							tran.AccountID = original.OffsetAccountID;
							tran.SubID = original.OffsetSubID;
							break;
						default://Source
							tran.AccountID = original.AccountID;
							tran.SubID = original.SubID;
							break;
					}

                    
					//Make Subaccount:
					
					if (step.AccountOrigin != PMOrigin.None)
					{
						PMSubAccountMaskAttribute.VerifyMask(step, tran, debitProject, task);

						object value = PMSubAccountMaskAttribute.MakeSub(this, step, tran, debitProject, task);

						Transactions.Cache.RaiseFieldUpdating<PMTran.subID>(tran, ref value);
						tran.SubID = (int?)value;
					}

					if (step.OffsetAccountOrigin == PMOrigin.Change)
					{
						tran.OffsetAccountID = step.OffsetAccountID;
						tran.OffsetSubID = original.OffsetSubID ?? (step.OffsetSubID ?? original.SubID);
					}
					else if (step.OffsetAccountOrigin == PMOrigin.OtherSource)
					{
                        if (original.AccountID == null)
                        {
							throw new PXException(Messages.OtherSourceIsEmpty, step.AllocationID, step.StepID, original.Description);
                        }

						tran.OffsetAccountID = original.AccountID;
						tran.OffsetSubID = original.SubID;
					}
					else
					{
						tran.OffsetAccountID = original.OffsetAccountID ?? original.AccountID;
						tran.OffsetSubID = original.OffsetSubID ?? original.SubID;
					}


					if (tran.OffsetAccountID != null)
					{
						PMOffsetSubAccountMaskAttribute.VerifyMask(step, tran, debitProject, task);

						object offsetValue = PMOffsetSubAccountMaskAttribute.MakeSub(this, step, tran, debitProject, task);

						Transactions.Cache.RaiseFieldUpdating<PMTran.offsetSubID>(tran, ref offsetValue);
						tran.OffsetSubID = (int?)offsetValue;
					}
				}
				#endregion

				if (calculateFormulas)
				{
					tran.Description = desc;
					tran.BillableQty = billableQty;
					tran.Qty = qty;

					decimal rawAmount;
					SetCalculatedAmount(debitProject, tran, amt.GetValueOrDefault(), out rawAmount);

					if (billableQty != null && billableQty != 0)
					{
						tran.Billable = true;
						tran.UseBillableQty = true;
						tran.UnitRate =  rawAmount / billableQty.Value;
						tran.TranCuryUnitRate = amt.GetValueOrDefault() / billableQty.Value ;
					}
				}
				list.Add(tran);
				#endregion
			}

			return list;
		}

		protected virtual void SetCurrencyInfoOnProjectChange(PMTran tran, PMTran original)
		{
			if (tran.ProjectID != original.ProjectID)
			{
				PMProject origProject = PMProject.PK.Find(this, original.ProjectID);
				PMProject project = PMProject.PK.Find(this, tran.ProjectID);

				string origProjectBaseCuryID = string.IsNullOrEmpty(origProject.BaseCuryID) ? Accessinfo.BaseCuryID : origProject.BaseCuryID;
				string projectBaseCuryID = string.IsNullOrEmpty(project.BaseCuryID) ? Accessinfo.BaseCuryID : project.BaseCuryID;
				string origProjectCuryID = string.IsNullOrEmpty(origProject.CuryID) ? Accessinfo.BaseCuryID : origProject.CuryID;
				string projectCuryID = string.IsNullOrEmpty(project.CuryID) ? Accessinfo.BaseCuryID : project.CuryID;

				if (origProjectBaseCuryID != projectBaseCuryID)
				{
					if (tran.TranCuryID == projectBaseCuryID)
					{
						var info = MultiCurrencyService.CreateDirectRate(this, projectBaseCuryID, tran.Date, GL.BatchModule.PM);
						tran.BaseCuryInfoID = info.CuryInfoID;
					}
					else
					{
						var info = MultiCurrencyService.CreateRate(this, tran.TranCuryID, projectBaseCuryID, tran.Date, project.RateTypeID, GL.BatchModule.PM);
						tran.BaseCuryInfoID = info.CuryInfoID;
					}
				}

				if (origProjectCuryID != projectCuryID)
				{
					if (projectCuryID == tran.TranCuryID)
					{
						var info = MultiCurrencyService.CreateDirectRate(this, tran.TranCuryID, tran.Date, GL.BatchModule.PM);
						tran.ProjectCuryInfoID = info.CuryInfoID;
					}
					else
					{
						var info = MultiCurrencyService.CreateRate(this, tran.TranCuryID, projectCuryID, tran.Date, project.RateTypeID, GL.BatchModule.PM);
						tran.ProjectCuryInfoID = info.CuryInfoID;
					}					
				}
			}
		}

		[Obsolete]
		protected virtual void VerifyMask(string mask, PMAllocationDetail step, PMProject project, PMTask task, int? tranSubID)
		{

		}

		protected virtual int? GetDebitAccountGroupFromStep(PMAllocationDetail step, PMTran original)
        {
			int? debitAccountGroup = null;

			if (step.AccountGroupOrigin == PMOrigin.Change)
			{
				debitAccountGroup = step.AccountGroupID;
			}
			else if (step.AccountGroupOrigin == PMOrigin.FromAccount)
			{
				if (step.AccountOrigin == PMOrigin.Source)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, original.AccountID);
					if (account != null)
						debitAccountGroup = account.AccountGroupID;
				}
				else if (step.AccountOrigin == PMOrigin.OtherSource)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, original.OffsetAccountID);
					if (account != null)
						debitAccountGroup = account.AccountGroupID;
				}
				else
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, step.AccountID);
					if (account != null)
						debitAccountGroup = account.AccountGroupID;
				}
			}
			else
			{
				if (step.AccountID != null)
				{
					PXTrace.WriteWarning("Step {0} is Debit Account Group configured as {1} but an Account is supplied.", step.StepID, step.AccountGroupOrigin);
				}

				debitAccountGroup = original.AccountGroupID;
			}

			return debitAccountGroup;
		}

		protected virtual int? GetCreditAccountGroupFromStep(PMAllocationDetail step, PMTran original)
		{
			int? creditAccountGroup = null;

			if (step.OffsetAccountGroupOrigin == PMOrigin.Change)
			{
				creditAccountGroup = step.OffsetAccountGroupID;
			}
			else if (step.OffsetAccountGroupOrigin == PMOrigin.FromAccount)
			{
				if (step.OffsetAccountOrigin == PMOrigin.Source)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, original.OffsetAccountID);
					if (account != null)
						creditAccountGroup = account.AccountGroupID;
				}
				else if (step.OffsetAccountOrigin == PMOrigin.OtherSource)
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, original.AccountID);
					if (account != null)
						creditAccountGroup = account.AccountGroupID;
				}
				else
				{
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, step.OffsetAccountID);
					if (account != null)
						creditAccountGroup = account.AccountGroupID;
				}
			}
			else if (step.OffsetAccountGroupOrigin == PMOrigin.Source)
			{
				if (step.OffsetAccountID != null)
				{
					PXTrace.WriteWarning("Step {0} is Credit Account Group configured as {1} but an Account is supplied.", step.StepID, step.OffsetAccountGroupOrigin);
				}

				creditAccountGroup = original.AccountGroupID;
			}

			return creditAccountGroup;
		}

		protected virtual int? GetDebitProjectFromStep(PMAllocationDetail step, PMTran original)
        {
			if (step.ProjectOrigin == PMOrigin.Change)
			{
				return step.ProjectID;
			}
			else
			{
				return original.ProjectID;
			}
		}

		protected virtual int? GetCreditProjectFromStep(PMAllocationDetail step, PMTran original)
		{
			if (step.OffsetProjectOrigin == PMOrigin.Change)
			{
				return step.OffsetProjectID;
			}
			else
			{
				return original.ProjectID;
			}
		}

		protected virtual int? GetDebitTaskFromStep(PMAllocationDetail step, PMTask task, int? debitProjectID)
		{
			if (ProjectDefaultAttribute.IsNonProject(debitProjectID))
				return null;

			int? debitTaskID = null;

			if (step.TaskOrigin == PMOrigin.Change)
			{
				PMTask pmTask;
				if (step.ProjectID == null)
				{
					pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.TaskCD, debitProjectID);
					debitTaskID = pmTask.TaskID;
				}
				else
				{
					debitTaskID = step.TaskID;
				}
			}
			else
			{
				PMTask debitTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(this, debitProjectID, task.TaskCD);

				if (debitTask == null)
				{
					PMProject debitProject = PMProject.PK.Find(this, debitProjectID);

					if (debitProject == null)
					{
						throw new PXException(Messages.DebitProjectNotFound, step.StepID);
					}

					throw new PXException(Messages.DebitTaskNotFound, step.StepID, step.AllocationID, task.TaskCD, debitProject.ContractCD);
				}

				debitTaskID = debitTask.TaskID;
			}

			return debitTaskID;
		}

		protected virtual int? GetCreditTaskFromStep(PMAllocationDetail step, PMTask task, int? creditProjectID)
        {
			if (ProjectDefaultAttribute.IsNonProject(creditProjectID))
				return null;

			int? creditTaskID = null;
			if (step.OffsetTaskOrigin == PMOrigin.Change)
			{
				PMTask pmTask;
				if (step.OffsetProjectID == null)
				{
					pmTask = PXSelect<PMTask, Where<PMTask.taskCD, Equal<Required<PMTask.taskCD>>, And<PMTask.projectID, Equal<Required<PMTask.projectID>>>>>.Select(this, step.OffsetTaskCD, creditProjectID);
					creditTaskID = pmTask.TaskID;
				}
				else
				{
					creditTaskID = step.OffsetTaskID;
				}
			}
			else
			{
				PMTask creditTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(this, creditProjectID, task.TaskCD);

				if (creditTask == null)
				{
					PMProject creditProject = PMProject.PK.Find(this, creditProjectID);

					if (creditProject == null)
					{
						throw new PXException(Messages.CreditProjectNotFound, step.StepID);
					}

					throw new PXException(Messages.CreditTaskNotFound, step.StepID, step.AllocationID, task.TaskCD, creditProject.ContractCD);
				}

				creditTaskID = creditTask.TaskID;
			}

			return creditTaskID;
		}

		protected virtual void SetCalculatedAmount(PMProject project, PMTran tran, decimal amt, out decimal rawAmount)
		{
			MultiCurrency multiCurrency = GetExtension<MultiCurrency>();
			CurrencyInfo baseCurrencyInfo = multiCurrency.GetCurrencyInfo(tran.BaseCuryInfoID);
			CurrencyInfo projectCurrencyInfo = multiCurrency.GetCurrencyInfo(tran.ProjectCuryInfoID);

			if ((tran.TranCuryID == project.CuryID) && (tran.TranCuryID == project.BaseCuryID))
			{
				rawAmount = amt;
				tran.TranCuryAmount = baseCurrencyInfo.RoundBase(amt);
				tran.ProjectCuryAmount = tran.TranCuryAmount;
				tran.Amount = tran.TranCuryAmount;
			}
			else if ((tran.TranCuryID != project.CuryID) && (tran.TranCuryID == project.BaseCuryID))
			{
				rawAmount = amt;
				tran.TranCuryAmount = baseCurrencyInfo.RoundBase(amt);
				tran.Amount = tran.TranCuryAmount;

				tran.ProjectCuryAmount = projectCurrencyInfo.CuryConvBase(tran.TranCuryAmount.GetValueOrDefault());
			}
			else if ((tran.TranCuryID == project.CuryID) && (tran.TranCuryID != project.BaseCuryID))
			{
				rawAmount = baseCurrencyInfo.CuryConvBase(amt);

				tran.TranCuryAmount = baseCurrencyInfo.RoundBase(amt);
				tran.ProjectCuryAmount = tran.TranCuryAmount;

				tran.Amount = baseCurrencyInfo.CuryConvBase(tran.TranCuryAmount.GetValueOrDefault());
			}
			else
			{
				rawAmount = baseCurrencyInfo.CuryConvBase(amt);
				tran.TranCuryAmount = baseCurrencyInfo.RoundBase(amt);

				tran.ProjectCuryAmount = projectCurrencyInfo.CuryConvBase(tran.TranCuryAmount.GetValueOrDefault());
				tran.Amount = baseCurrencyInfo.CuryConvBase(tran.TranCuryAmount.GetValueOrDefault());
			}
		}

		protected virtual void _(Events.FieldUpdated<PMTran, PMTran.offsetAccountID> e)
		{
			RegisterEntry.UpdateOffsetAccountId(this, e.Cache, e.Row);
		}

		public virtual PMTran CreateFromTemplate(PMAllocationDetail step, PMTran original, bool isWipStep)
		{
			PMTran tran = new PMTran();
			tran.BranchID = step.TargetBranchID ?? original.BranchID;
			tran.UOM = original.UOM;
			tran.BAccountID = original.BAccountID;
			tran.Billable = original.Billable;
			tran.BillableQty = original.BillableQty;
			tran.InventoryID = original.InventoryID;
			tran.LocationID = original.LocationID;
			tran.ResourceID = original.ResourceID;
			tran.CostCodeID = original.CostCodeID;
			tran.AllocationID = step.AllocationID;
			tran.TranCuryID = original.TranCuryID;
			tran.BaseCuryInfoID = original.BaseCuryInfoID;
			tran.ProjectCuryInfoID = original.ProjectCuryInfoID;

			if (!isWipStep)
            {
				tran.OrigAccountGroupID = original.AccountGroupID;
				tran.OrigProjectID = original.ProjectID;
				tran.OrigTaskID = original.TaskID;
            }
			tran.IsNonGL = step.UpdateGL == false;
			if (step.DateSource == PMDateSource.Transaction)
			{
				tran.Date = original.Date;
				if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
				{
					tran.FinPeriodID = FinPeriodRepository.GetFinPeriodByBranchAndMasterPeriodID(tran.BranchID, original.TranPeriodID);
				}
				else
				{
					tran.FinPeriodID = original.FinPeriodID;
				}
			}
			else if (PostingDate != null)
			{
				tran.Date = PostingDate;
				tran.FinPeriodID = null;
			}

			tran.StartDate = original.StartDate;
			tran.EndDate = original.EndDate;
			tran.OrigRefID = original.OrigRefID;
			tran.UseBillableQty = true;
			tran.Reverse = step.Reverse;

			return tran;
		}

		public virtual void CalculateFormulas(PMDataNavigator navigator, PMAllocationDetail step, PMTran tran, out decimal? qty, out decimal? billableQty, out decimal? amt, out string description)
		{
			qty = null;
			billableQty = null;
			amt = null;
			description = null;

			if (!string.IsNullOrEmpty(step.QtyFormula))
			{
				try
				{
					ExpressionNode qtyNode = PMExpressionParser.Parse(this, step.QtyFormula);
					qtyNode.Bind(navigator);
					object val = qtyNode.Eval(tran);
					if (val != null)
					{
						qty = Convert.ToDecimal(val);
					}
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcQtyFormula, step.AllocationID, step.StepID, step.QtyFormula, ex.Message);
				}
			}
			else
			{
				qty = tran.Qty;
			}

			if (!string.IsNullOrEmpty(step.BillableQtyFormula))
			{
				try
				{
					ExpressionNode billableQtyNode = PMExpressionParser.Parse(this, step.BillableQtyFormula);
					billableQtyNode.Bind(navigator);
					object val = billableQtyNode.Eval(tran);

					if (val != null)
					{
						billableQty = Convert.ToDecimal(val);
					}
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcBillQtyFormula, step.AllocationID, step.StepID, step.BillableQtyFormula, ex.Message);
				}
			}
			else
			{
				billableQty = tran.BillableQty;
			}

			if (!string.IsNullOrEmpty(step.AmountFormula))
			{
				try
				{
					ExpressionNode amtNode = PMExpressionParser.Parse(this, step.AmountFormula);
					amtNode.Bind(navigator);
					object val = amtNode.Eval(tran);
					if (val != null)
					{
						amt = Convert.ToDecimal(val);
					}
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcAmtFormula, step.AllocationID, step.StepID, step.AmountFormula, ex.Message);
				}
			}
			else
			{
				amt = tran.TranCuryAmount;
			}

			if (!string.IsNullOrEmpty(step.DescriptionFormula))
			{
				try
				{
					ExpressionNode descNode = PMExpressionParser.Parse(this, step.DescriptionFormula);
					descNode.Bind(navigator);
					object val = descNode.Eval(tran);
					if (val != null)
						description = val.ToString();
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcDescFormula, step.AllocationID, step.StepID, step.DescriptionFormula, ex.Message);
				}
			}
			else
			{
				description = tran.Description;
			}

		}
		
		/// <summary>
		/// Selects Source Transactions for the given step. 
		/// Method relies on pre-selected data stored in allocationInfo, accountGroups and transactions collections.
		/// Method do not query database.
		/// </summary>
		public virtual List<PMTran> Select(PMAllocationDetail step, int? projectID, int? taskID)
		{
			List<PMTran> list;

			if (step.Post == true)
			{
				if (stepResults.ContainsKey(step.StepID.Value))
				{
					list = new List<PMTran>(stepResults[step.StepID.Value].Count);
					foreach (PMTranWithTrace twt in stepResults[step.StepID.Value])
					{
						list.Add(twt.Tran);
					}

					return list;
				}
			}

			list = new List<PMTran>();
			if (step.SelectOption == PMSelectOption.Step)
			{
				foreach (PMAllocationDetail innerStep in GetSteps(step.AllocationID, step.RangeStart, step.RangeEnd))
				{
					list.AddRange(Select(innerStep, projectID, taskID));
				}
			}
			else
			{
				foreach (PMTran tran in GetTranByStep(step, projectID, taskID))
				{
					list.Add(tran);
				}
			}

			return list;
		}

		/// <summary>
		/// Selects Source Transactions for the given inner step (step with From/To AccountGroups). 
		/// Method relies on pre-selected data stored in allocationInfo, accountGroups and transactions collections.
		/// Method do not query database.
		/// </summary>
		public virtual List<PMTran> GetTranByStep(PMAllocationDetail step, int? projectID, int? taskID)
		{
			List<PMTran> result = new List<PMTran>();
			foreach (int groupID in GetAccountGroupsRange(step))
			{
					//get from memory (pre-selected/cached)
					Dictionary<int, List<PXResult<PMTran>>> transactionsByTask;
					if (transactions.TryGetValue(taskID.Value, out transactionsByTask))
					{
						List<PXResult<PMTran>> source;
						if (transactionsByTask.TryGetValue(groupID, out source))
						{
							foreach (PMTran tran in source)
							{
								if (step.SourceBranchID != null && step.SourceBranchID != tran.BranchID)
									continue;

							result.Add(tran);
						}
					}
				}
			}

			return result;
		}

		public virtual PXResultset<PMTran> GetTranFromDatabase(int? projectID, int? taskID, int groupID)
		{
			PXResultset<PMTran> resultset = null;

			PXSelectBase<PMTran> selectTrans = new PXSelectReadonly<PMTran,
				Where<PMTran.allocated, Equal<False>,
					And<PMTran.excludedFromAllocation, Equal<False>,
					And<PMTran.released, Equal<True>,
					And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
					And<PMTran.projectID, Equal<Required<PMTran.projectID>>,
					And<PMTran.taskID, Equal<Required<PMTran.taskID>>>>>>>>>(this);

			if (FilterStartDate != null && FilterEndDate != null)
			{
				if (FilterStartDate == FilterEndDate)
				{
					selectTrans.WhereAnd<Where<PMTran.date, Equal<Required<PMTran.date>>>>();
					resultset = selectTrans.Select(groupID, projectID, taskID, FilterStartDate);
				}
				else
				{
					selectTrans.WhereAnd<Where<PMTran.date, Between<Required<PMTran.date>, Required<PMTran.date>>>>();
					resultset = selectTrans.Select(groupID, projectID, taskID, FilterStartDate, FilterEndDate);
				}
			}
			else if (FilterStartDate != null)
			{
				selectTrans.WhereAnd<Where<PMTran.date, GreaterEqual<Required<PMTran.date>>>>();
				resultset = selectTrans.Select(groupID, projectID, taskID, FilterStartDate);
			}
			else if (FilterEndDate != null)
			{
				selectTrans.WhereAnd<Where<PMTran.date, LessEqual<Required<PMTran.date>>>>();
				resultset = selectTrans.Select(groupID, projectID, taskID, FilterEndDate);
			}
			else
			{
				resultset = selectTrans.Select(groupID, projectID, taskID);
			}

			return resultset;
		}

		public virtual PXResultset<PMTran> GetTranFromDatabase(int? projectID, int groupID)
		{
			PXResultset<PMTran> resultset = null;

			PXSelectBase<PMTran> selectTrans = new PXSelectReadonly<PMTran,
				Where<PMTran.allocated, Equal<False>,
					And<PMTran.excludedFromAllocation, Equal<False>,
					And<PMTran.released, Equal<True>,
					And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
					And<PMTran.projectID, Equal<Required<PMTran.projectID>>>>>>>>(this);

			if (FilterStartDate != null && FilterEndDate != null)
			{
				if (FilterStartDate == FilterEndDate)
				{
					selectTrans.WhereAnd<Where<PMTran.date, Equal<Required<PMTran.date>>>>();
					resultset = selectTrans.Select(groupID, projectID, FilterStartDate);
				}
				else
				{
					selectTrans.WhereAnd<Where<PMTran.date, Between<Required<PMTran.date>, Required<PMTran.date>>>>();
					resultset = selectTrans.Select(groupID, projectID, FilterStartDate, FilterEndDate);
				}
			}
			else if (FilterStartDate != null)
			{
				selectTrans.WhereAnd<Where<PMTran.date, GreaterEqual<Required<PMTran.date>>>>();
				resultset = selectTrans.Select(groupID, projectID, FilterStartDate);
			}
			else if (FilterEndDate != null)
			{
				selectTrans.WhereAnd<Where<PMTran.date, LessEqual<Required<PMTran.date>>>>();
				resultset = selectTrans.Select(groupID, projectID, FilterEndDate);
			}
			else
			{
				resultset = selectTrans.Select(groupID, projectID);
			}

			return resultset;
		}

		public virtual PMAllocationSourceTran CreateAllocationTran(string allocationID, int? stepID, PMTran tran)
		{
			PMAllocationSourceTran at = new PMAllocationSourceTran();
			at.AllocationID = allocationID;
			at.StepID = stepID;
			at.TranID = tran.TranID;
			at.Qty = tran.Qty;
			at.Rate = tran.Rate;
			at.Amount = tran.ProjectCuryAmount;

			return at;
		}

		public virtual decimal? GetRate(PMAllocationDetail step, PMTran tran, string rateTableID)
		{
			if (string.IsNullOrEmpty(step.RateTypeID))
			{
				switch (step.NoRateOption)
				{
					case PMNoRateOption.SetZero:
						return 0;
					case PMNoRateOption.RaiseError:
						throw new PXException(Messages.RateTypeNotDefinedForStep, step.StepID);
					case PMNoRateOption.DontAllocate:
						return null;
					default:
						return 1;
				}
			}

			decimal? rate = null;
			string trace = null;

			if (!string.IsNullOrEmpty(rateTableID))
			{
				if (rateEngine != null)
				{
					//use pre-selected rates
					rate = rateEngine.GetRate(rateTableID, step.RateTypeID, tran);
					trace = rateEngine.GetTrace(tran);
				}
				else
				{
					RateEngine engine = CreateRateEngine(step.RateTypeID, tran);

					rate = engine.GetRate(rateTableID);
					trace = engine.GetTrace();
				}
			}
						
			if (rate != null)
				return rate;
			else
			{
				switch (step.NoRateOption)
				{
					case PMNoRateOption.SetZero:
						return 0;
					case PMNoRateOption.RaiseError:
						PXTrace.WriteInformation(trace);
						PXTrace.WriteError(Messages.RateNotDefinedForStepAllocation, step.AllocationID, step.StepID);
						throw new PXException(Messages.RateNotDefinedForStepAllocation, step.AllocationID, step.StepID);
					case PMNoRateOption.DontAllocate:
						PXTrace.WriteInformation(trace);
						return null;
					default:
						return 1;
				}
			}

		}

		public decimal? ConvertAmountToCurrency(string fromCuryID, string toCuryID, string rateType, DateTime? effectiveDate, decimal? value)
		{
			if (string.IsNullOrEmpty(fromCuryID))
				throw new ArgumentNullException(nameof(fromCuryID), "From CuryID is null or an empty string.");

			if (string.IsNullOrEmpty(toCuryID))
				throw new ArgumentNullException(nameof(toCuryID), "To CuryID is null or an empty string.");

			if (string.IsNullOrEmpty(rateType))
				throw new ArgumentNullException(nameof(rateType), "RateType is null or an empty string.");

			if (effectiveDate == null)
				throw new ArgumentNullException(nameof(effectiveDate), "Effective Date is required.");

			if (value == null)
				return null;

			if (value.Value == 0m)
				return 0m;

			if (string.Equals(fromCuryID, toCuryID, StringComparison.InvariantCultureIgnoreCase))
			{
				return value.Value;
			}

			PX.Objects.CM.Extensions.IPXCurrencyService currencyService = ServiceLocator.Current.GetInstance<Func<PXGraph, PX.Objects.CM.Extensions.IPXCurrencyService>>()(this);
			var rate = currencyService.GetRate(fromCuryID, toCuryID, rateType, effectiveDate);
			if (rate == null)
			{
				throw new PXException(PM.Messages.CurrencyRateIsNotDefined, fromCuryID, toCuryID, rateType, effectiveDate);
			}
			else
			{
				return PMCommitmentAttribute.CuryConvCury(rate, value.GetValueOrDefault(), currencyService.CuryDecimalPlaces(toCuryID));
			}
		}

		public virtual RateEngine CreateRateEngine(string rateTypeID, PMTran tran)
		{
			return new RateEngine(this, rateTypeID, tran);
		}

		public virtual decimal ConvertQtyToBase(int? inventoryID, string UOM, decimal qty)
		{
			try
			{
				return INUnitAttribute.ConvertToBase(Transactions.Cache, inventoryID, UOM, qty, INPrecision.QUANTITY);
			}
			catch (PXException ex)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
						.Select(this, inventoryID);

				if (item != null)
				{
					PXTrace.WriteError("Failed to convert the Inventory Item '{0}' FROM '{1}' TO '{2}'. Error: {3}", item.InventoryCD, UOM, item.BaseUnit, ex.Message);
				}

				throw;
			}
		}

		public override void Clear()
		{
			base.Clear();
			MultiCurrencyService.Clear();
		}

		#region Local Types

		public class AllocData
		{
			public int AccountGroupID { get; private set; }
			public int InventoryID { get; private set; }
			public int CostCodeID { get; private set; }

			public decimal Amount { get; set; }
			public decimal Quantity { get; set; }
			public string UOM { get; set; }

			public AllocData(int accountGroupID, int inventoryID, int costCodeID)
			{
				this.AccountGroupID = accountGroupID;
				this.InventoryID = inventoryID;
				this.CostCodeID = costCodeID;
			}
		}

		public class PMBudgetStat
		{
			public int AccountGroupID { get; private set; }
			public int InventoryID { get; private set; }
			public int CostCodeID { get; private set; }
			public decimal? Amount { get; set; }
			public decimal? Quantity { get; set; }

			public PMBudgetStat(int accountGroupID, int inventoryID, int costCodeID)
			{
				this.AccountGroupID = accountGroupID;
				this.InventoryID = inventoryID;
				this.CostCodeID = costCodeID;
			}
		}
				
        [Serializable]
		public class AllocatedService
		{
			private Dictionary<string, PMBudgetStat> list = new Dictionary<string, PMBudgetStat>();

			public AllocatedService(PXGraph graph, PMTask task)
			{
				PXSelectBase<PMTaskAllocTotalEx> select = new PXSelectReadonly<PMTaskAllocTotalEx, Where<PMTaskAllocTotalEx.projectID, Equal<Required<PMTaskAllocTotalEx.projectID>>, And<PMTaskAllocTotalEx.taskID, Equal<Required<PMTaskAllocTotalEx.taskID>>>>>(graph);
				foreach (PMTaskAllocTotalEx tat in select.Select(task.ProjectID, task.TaskID))
				{
					PMBudgetStat bs = new PMBudgetStat(tat.AccountGroupID.Value, tat.InventoryID.Value, tat.CostCodeID.Value);
					bs.Amount = tat.Amount;
					bs.Quantity = tat.Quantity;

					list.Add(GetKey(tat.AccountGroupID.Value, tat.InventoryID.Value, tat.CostCodeID.Value), bs);
				}

				foreach (PMTaskAllocTotalAccum acum in graph.Caches[typeof(PMTaskAllocTotalAccum)].Inserted)
				{
					string key = GetKey(acum.AccountGroupID.Value, acum.InventoryID.Value, acum.CostCodeID.Value);

					if (list.ContainsKey(key) && acum.TaskID == task.TaskID)
					{
						list[key].Amount += acum.Amount;
						list[key].Quantity += acum.Quantity;
					}
				}
			}

			public decimal GetAllocatedAmt(int accountGroupID, int inventoryID, int costCodeID)
			{
				PMBudgetStat result;
				if (list.TryGetValue(GetKey(accountGroupID, inventoryID, costCodeID), out result))
					return result.Amount ?? 0;
				else
					return 0;
			}

			public decimal GetAllocatedAmt(int accountGroupID)
			{
				decimal amt = 0;
				foreach (PMBudgetStat record in list.Values)
				{
					if (record.AccountGroupID == accountGroupID)
						amt += record.Amount ?? 0;
				}

				return amt;
			}

			public decimal GetAllocatedQty(int accountGroupID, int inventoryID, int costCodeID)
			{
				PMBudgetStat result;
				if (list.TryGetValue(GetKey(accountGroupID, inventoryID, costCodeID), out result))
					return result.Quantity ?? 0;
				else
					return 0;
			}

			private static string GetKey(int accountGroupID, int inventoryID, int costCodeID)
			{
				return string.Format("{0}.{1}.{2}", accountGroupID, inventoryID, costCodeID);
			}

            [Serializable]
            [PXHidden]
			[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
			public class PMTaskAllocTotalEx : PMTaskAllocTotal
			{
				#region ProjectID
				public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
				#endregion
				#region TaskID
				public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
				#endregion
				#region AccountGroupID
				public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
				#endregion
				#region InventoryID
				public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
				#endregion
				#region CostCodeID
				public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
				#endregion
			}
		}
				
		public class PMTranWithTrace
		{
			public PMTran Tran;
			public Guid[] Files;
			public string NoteText;
			public List<long> OriginalTrans = new List<long>();

			public PMTranWithTrace(PMTran tran, long? originalTran)
			{
				this.Tran = tran;
				OriginalTrans.Add(originalTran.Value);
			}

			public PMTranWithTrace(PMTran tran, List<long> list)
			{
				this.Tran = tran;
				OriginalTrans.AddRange(list);
			}
		}

		public class PMDataNavigator : PX.Reports.Data.IDataNavigator
		{
			protected List<PMTran> list;
			protected IRateTable engine;

			public PMDataNavigator(IRateTable engine, List<PMTran> list)
			{
				this.engine = engine;
				this.list = list;
			}

			#region IDataNavigator Members

			public void Clear()
			{
			}

			public void Refresh()
			{
			}

			public object Current
			{
				get { throw new NotImplementedException(); }
			}

			public PX.Reports.Data.IDataNavigator GetChildNavigator(object record)
			{
				return null;
			}

			public object GetItem(object dataItem, string dataField)
			{
				throw new NotImplementedException();
			}

			public System.Collections.IList GetList()
			{
				return list;
			}

			public object GetValue(object dataItem, string dataField, ref string format, bool valueOnly = false)
			{
				PMNameNode nn = new PMNameNode(null, dataField, null);

				if (nn.IsAttribute)
					return engine.Evaluate(nn.ObjectName, null, nn.FieldName, (PMTran)dataItem);
				else
				{
					return engine.Evaluate(nn.ObjectName, nn.FieldName, null, (PMTran)dataItem);

				}
			}

			public bool MoveNext()
			{
				throw new NotImplementedException();
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			public PX.Reports.Data.ReportSelectArguments SelectArguments
			{
				get { throw new NotImplementedException(); }
			}

			public object this[string dataField]
			{
				get { throw new NotImplementedException(); }
			}

			public string CurrentlyProcessingParam
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public int[] GetFieldSegments(string field)
			{
				throw new NotImplementedException();
			}
			public object Clone()
			{
				return new PMDataNavigator(engine, list == null ? null : new List<PMTran>(list));
			}

			#endregion
		}

		public class AllocationInfo
		{
			public HashSet<int> AccountGroups { get; private set; }
			public HashSet<string> RateTypes { get; private set; }
			public List<PMAllocationDetail> Steps  { get; private set; }
			public bool ContainsBudgetStep { get; set; }

			public AllocationInfo(HashSet<int> accountGroups, HashSet<string> rateTypes, List<PMAllocationDetail> steps)
			{
				this.AccountGroups = accountGroups;
				this.RateTypes = rateTypes;
				this.Steps = steps;
	}

		}

		public class AccountGroup
		{
			public int GroupID { get; private set; }
			public string GroupCD { get; private set; }

			public AccountGroup(int id, string cd)
			{
				this.GroupID = id;
				this.GroupCD = cd;
			}
		}

		#endregion
	}
}
