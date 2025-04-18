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
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class ProjectBalanceValidation : PXGraph<ProjectBalanceValidation>
	{
		public PXCancel<PMValidationFilter> Cancel;
		public PXFilter<PMValidationFilter> Filter;
		public PXFilteredProcessing<PMProject, PMValidationFilter, Where<PMProject.baseType, Equal<CT.CTPRType.project>,
			And<PMProject.nonProject, Equal<False>,
			And2<Match<PMProject, Current<AccessInfo.userName>>,
			And<Where<PMProject.isActive, Equal<True>, Or<PMProject.isCompleted, Equal<True>>>>>>>> Items;
		public PXSetup<PMSetup> Setup;

		[PXViewName(Messages.Project)]
		public PXSelect<PMProject> Project;

		public ProjectBalanceValidation()
		{
			Items.SetSelected<PMProject.selected>();
			Items.SetProcessCaption(GL.Messages.Process);
			Items.SetProcessAllCaption(GL.Messages.ProcessAll);
			Items.SetProcessTooltip(Messages.RecalculateBalanceTooltip);
			Items.SetProcessAllTooltip(Messages.RecalculateBalanceTooltip);
		}

		public PXAction<PMValidationFilter> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			ProjectEntry graph = CreateInstance<ProjectEntry>();
			graph.Project.Current = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Current<PMProject.contractCD>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}


		protected virtual void _(Events.RowSelected<PMValidationFilter> e)
		{
			PMValidationFilter filter = Filter.Current;

			Items.SetProcessDelegate<ProjectBalanceValidationProcess>(
					delegate (ProjectBalanceValidationProcess graph, PMProject item)
					{
						graph.Clear(PXClearOption.PreserveTimeStamp);
						graph.RunProjectBalanceVerification(item, filter);
					});

			PXUIFieldAttribute.SetVisible<PMValidationFilter.rebuildCommitments>(Filter.Cache, null, Setup.Current == null || Setup.Current.CostCommitmentTracking.GetValueOrDefault());
		}
	}

	[Serializable]
	public class ProjectBalanceValidationProcess : PXGraph<ProjectBalanceValidationProcess>
	{
		#region DAC Overrides
		[POCommitment]
		[PXDBGuid]
		protected virtual void _(Events.CacheAttached<POLine.commitmentID> e) { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		protected virtual void _(Events.CacheAttached<POLine.orderType> e) { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXParent(typeof(Select<POOrder, Where<POOrder.orderType, Equal<Current<POLine.orderType>>, And<POOrder.orderNbr, Equal<Current<POLine.orderNbr>>>>>))]
		protected virtual void _(Events.CacheAttached<POLine.orderNbr> e) { }

		[PXDBDate()]
		protected virtual void _(Events.CacheAttached<POLine.orderDate> e) { }

		[PXDBInt]
		protected virtual void _(Events.CacheAttached<POLine.vendorID> e) { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		protected virtual void _(Events.CacheAttached<SOLine.orderType> e) { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine.orderNbr>>>>>))]
		protected virtual void _(Events.CacheAttached<SOLine.orderNbr> e) { }

		[PXDBDate()]
		protected virtual void _(Events.CacheAttached<SOLine.orderDate> e) { }

		[PXDefault]
		[PXDBInt] //NO Selector Validation
		protected virtual void _(Events.CacheAttached<PMCommitment.projectID> e) { }

		[PXDefault]
		[PXDBInt] //NO Selector Validation
		protected virtual void _(Events.CacheAttached<PMCommitment.projectTaskID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		protected virtual void _(Events.CacheAttached<PMBudgetAccum.projectTaskID> e) { }

		// AC-277504 (Disabling cost code verification)
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMCommitment.costCodeID> e) { }
		#endregion
				
		public class MultiCurrency : ProjectBudgetMultiCurrency<ProjectBalanceValidationProcess>
		{
			protected override PXSelectBase[] GetChildren() => new PXSelectBase[]
			{
				Base.Budget
			};
		}


		public PXSelect<PMBudgetAccum> Budget;
		public PXSelect<PMForecastHistoryAccum> ForecastHistory;
		public PXSelect<PMTaskTotal> TaskTotals;
		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;
		public PXSelect<PMHistoryAccum> History;
		public PXSelectJoin<POLine,
			InnerJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, And<POOrder.orderNbr, Equal<POLine.orderNbr>>>>,
			Where<POLine.projectID, Equal<Required<POLine.projectID>>>> polines;
		public PXSelect<PMCommitment, Where<PMCommitment.type, Equal<PMCommitmentType.externalType>, And<PMCommitment.projectID, Equal<Required<PMCommitment.projectID>>>>> ExternalCommitments;
		public PXSetup<PMSetup> Setup;
		public PXSelect<PMProject> Project;
		public PXSelect<PMBillingRecord> BillingRecords;
		public Dictionary<int, PMAccountGroup> AccountGroups;
		public Dictionary<int, Account> Accounts;


		private BudgetServiceMassUpdate budgetService;
		private IFinPeriodRepository finPeriodsRepo;
		public virtual IFinPeriodRepository FinPeriodRepository
		{
			get
			{
				if (finPeriodsRepo == null)
				{
					finPeriodsRepo = new FinPeriodRepository(this);
				}

				return finPeriodsRepo;
			}
		}

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		protected virtual void ClearBalance(PMProject project, PMValidationFilter options)
		{
			if (project != null && options != null)
			{
				PXDatabase.Delete<PMTaskTotal>(new PXDataFieldRestrict<PMTaskTotal.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
				PXDatabase.Delete<PMTaskAllocTotal>(new PXDataFieldRestrict<PMTaskAllocTotal.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
				PXDatabase.Delete<PMHistory>(new PXDataFieldRestrict<PMHistory.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));

				if (options.RebuildCommitments == true)
				{
					PXDatabase.Delete<PMCommitment>(new PXDataFieldRestrict<PMCommitment.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ),
					new PXDataFieldRestrict<PMCommitment.type>(PXDbType.Char, 1, PMCommitmentType.Internal, PXComp.EQ));
				}

				if (options.RecalculateUnbilledSummary == true)
				{
					PXDatabase.Delete<PMUnbilledDailySummary>(new PXDataFieldRestrict(typeof(PMUnbilledDailySummary.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ));
				}

				if (options.RecalculateChangeOrders == true && project.ChangeOrderWorkflow == true)
				{
					foreach (PMBudgetLiteEx record in budgetService.BudgetRecords)
					{
						List<PXDataFieldParam> list = BuildBudgetClearCommandWithChangeOrders(options, record);
						PXDatabase.Update<PMBudget>(list.ToArray());
					}
				}
				else
				{
					foreach (int accountGroupID in budgetService.GetUsedAccountGroups())
					{
						List<PXDataFieldParam> list = BuildBudgetClearCommand(options, project, accountGroupID);
						PXDatabase.Update<PMBudget>(list.ToArray());
					}
				}

				List<PXDataFieldParam> listForecast = BuildForecastClearCommand(options, project.ContractID);
				PXDatabase.Update<PMForecastHistory>(listForecast.ToArray());
			}
		}

		public virtual void RunProjectBalanceVerification(PMValidationFilter options)
		{
			RunProjectBalanceVerification(Project.Current, options);
		}

		public virtual void RunProjectBalanceVerification(PMProject project, PMValidationFilter options)
		{
			budgetService = new BudgetServiceMassUpdate(this, project);
			InitAccountGroup();
			InitAccounts();

			using (var ts = new PXTransactionScope())
			{
				ClearBalance(project, options);
				RecalculateBalance(project, options);
				PersistCaches();

				HandleProjectStatusCode(project, options);

				ts.Complete();
			}

			OnCachePersisted();
		}

		/// <summary>
		/// Recalculates the balances for all projects that may be affected when the Account type is modified.
		/// </summary>
		/// <param name="modifiedAccounts">List of modified Accounts.</param>
		public virtual void RebuildBalanceOnAccountTypeChange(IList<Account> modifiedAccounts)
		{
			if (modifiedAccounts != null && modifiedAccounts.Count > 0)
			{
				var select = new PXSelectJoinGroupBy<PMTran,
					InnerJoin<PMProject, On<PMTran.projectID, Equal<PMProject.contractID>,
						And<PMProject.nonProject, Equal<False>,
						And<PMProject.baseType, Equal<CT.CTPRType.project>>>>>,
					Where2<Where<PMTran.accountID, In<Required<PMTran.accountID>>,
						Or<PMTran.offsetAccountID, In<Required<PMTran.offsetAccountID>>>>,
					And<PMTran.released, Equal<True>>>,
					Aggregate<GroupBy<PMTran.projectID>>>(this);

				InitAccounts();

				List<int> ids = new List<int>(modifiedAccounts.Count);
				foreach (Account modified in modifiedAccounts)
				{
					Account applicable;
					if (Accounts.TryGetValue(modified.AccountID.Value, out applicable))
					{
						applicable.Type = modified.Type;
						ids.Add(modified.AccountID.Value);
					}
				}

				PMValidationFilter options = new PMValidationFilter();
				foreach (PXResult<PMTran, PMProject> res in select.Select(ids.ToArray(), ids.ToArray()))
				{
					PMProject project = (PMProject)res;
					RunProjectBalanceVerification(project, options);
				}
			}
		}
				

		protected virtual void PersistCaches()
		{
			Budget.Cache.Persist(PXDBOperation.Insert);
			Budget.Cache.Persist(PXDBOperation.Update);

			ForecastHistory.Cache.Persist(PXDBOperation.Insert);
			ForecastHistory.Cache.Persist(PXDBOperation.Update);

			TaskTotals.Cache.Persist(PXDBOperation.Insert);
			TaskTotals.Cache.Persist(PXDBOperation.Update);

			AllocationTotals.Cache.Persist(PXDBOperation.Insert);
			AllocationTotals.Cache.Persist(PXDBOperation.Update);

			History.Cache.Persist(PXDBOperation.Insert);
			History.Cache.Persist(PXDBOperation.Update);

			ExternalCommitments.Cache.Persist(PXDBOperation.Insert);
			ExternalCommitments.Cache.Persist(PXDBOperation.Update);
			ExternalCommitments.Cache.Persist(PXDBOperation.Delete);

			BillingRecords.Cache.Persist(PXDBOperation.Insert);
			polines.Cache.Persist(PXDBOperation.Update);

			this.Caches[typeof(PMUnbilledDailySummaryAccum)].Persist(PXDBOperation.Insert);
			this.Caches[typeof(PMUnbilledDailySummaryAccum)].Persist(PXDBOperation.Update);
		}

		protected virtual void OnCachePersisted()
		{
			Budget.Cache.Persisted(false);
			ForecastHistory.Cache.Persisted(false);
			TaskTotals.Cache.Persisted(false);
			AllocationTotals.Cache.Persisted(false);
			History.Cache.Persisted(false);
			ExternalCommitments.Cache.Persisted(false);
			BillingRecords.Cache.Persisted(false);
			polines.Cache.Persisted(false);
			this.Caches[typeof(PMUnbilledDailySummaryAccum)].Persisted(false);
		}

		public virtual void RecalculateBalance(PMProject project, PMValidationFilter options)
		{
			ProjectBalance pb = CreateProjectBalance();
			PXSelectBase<PMTran> select = null;

			if (options.RecalculateUnbilledSummary == true)
			{
				select = new PXSelectGroupBy<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>,
				Aggregate<GroupBy<PMTran.tranType,
				GroupBy<PMTran.branchID,
				GroupBy<PMTran.finPeriodID,
				GroupBy<PMTran.tranPeriodID,
				GroupBy<PMTran.projectID,
				GroupBy<PMTran.taskID,
				GroupBy<PMTran.inventoryID,
				GroupBy<PMTran.costCodeID,
				GroupBy<PMTran.date,
				GroupBy<PMTran.accountID,
				GroupBy<PMTran.accountGroupID,
				GroupBy<PMTran.offsetAccountID,
				GroupBy<PMTran.offsetAccountGroupID,
				GroupBy<PMTran.uOM,
				GroupBy<PMTran.released,
				GroupBy<PMTran.remainderOfTranID,
				GroupBy<PMTran.tranType,
				GroupBy<PMTran.origModule,
				GroupBy<PMTran.origTranType,
				Sum<PMTran.qty,
				Sum<PMTran.amount,
				Sum<PMTran.projectCuryAmount,
				Max<PMTran.billable,
				GroupBy<PMTran.billed,
				GroupBy<PMTran.excludedFromBilling>>>>>>>>>>>>>>>>>>>>>>>>>>>(this);
			}
			else
			{
				select = new PXSelectGroupBy<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>,
				Aggregate<GroupBy<PMTran.tranType,
				GroupBy<PMTran.branchID,
				GroupBy<PMTran.finPeriodID,
				GroupBy<PMTran.tranPeriodID,
				GroupBy<PMTran.projectID,
				GroupBy<PMTran.taskID,
				GroupBy<PMTran.inventoryID,
				GroupBy<PMTran.costCodeID,
				GroupBy<PMTran.accountID,
				GroupBy<PMTran.accountGroupID,
				GroupBy<PMTran.offsetAccountID,
				GroupBy<PMTran.offsetAccountGroupID,
				GroupBy<PMTran.uOM,
				GroupBy<PMTran.released,
				GroupBy<PMTran.remainderOfTranID,
				Sum<PMTran.qty,
				Sum<PMTran.amount,
				Sum<PMTran.projectCuryAmount>>>>>>>>>>>>>>>>>>>>(this);
			}

			using (new PXFieldScope(select.View
				, typeof(PMTran.tranID)
				, typeof(PMTran.tranType)
				, typeof(PMTran.branchID)
				, typeof(PMTran.finPeriodID)
				, typeof(PMTran.tranPeriodID)
				, typeof(PMTran.tranDate)
				, typeof(PMTran.date)
				, typeof(PMTran.projectID)
				, typeof(PMTran.taskID)
				, typeof(PMTran.inventoryID)
				, typeof(PMTran.costCodeID)
				, typeof(PMTran.accountID)
				, typeof(PMTran.accountGroupID)
				, typeof(PMTran.offsetAccountID)
				, typeof(PMTran.offsetAccountGroupID)
				, typeof(PMTran.uOM)
				, typeof(PMTran.released)
				, typeof(PMTran.remainderOfTranID)
				, typeof(PMTran.qty)
				, typeof(PMTran.amount)
				, typeof(PMTran.projectCuryAmount)
				, typeof(PMTran.billable)
				, typeof(PMTran.billed)
				, typeof(PMTran.excludedFromBilling)
				, typeof(PMTran.excludedFromBalance)
				, typeof(PMTran.origModule)
				, typeof(PMTran.origTranType)))
			{
				foreach (PMTran tran in select.Select(project.ContractID))
				{
					Account account = null;
					Account offsetAccount = null;
					PMAccountGroup accountGroup = null;
					PMAccountGroup offsetAccountGroup = null;

					#region Init Account and AccountGroups emulating BQL's LEFT JOIN

					if (!AccountGroups.TryGetValue(tran.AccountGroupID.Value, out accountGroup))
					{
						accountGroup = new PMAccountGroup();
					}

					if (tran.AccountID == null)
					{
						account = new Account();
					}
					else
					{
						if (!Accounts.TryGetValue(tran.AccountID.Value, out account))
						{
							account = new Account();
						}
					}

					if (tran.OffsetAccountID == null)
					{
						offsetAccount = new Account();
						offsetAccountGroup = new PMAccountGroup();
					}
					else
					{
						if (!Accounts.TryGetValue(tran.OffsetAccountID.Value, out offsetAccount))
						{
							offsetAccount = new Account();
							offsetAccountGroup = new PMAccountGroup();
						}
						else
						{
							if (!AccountGroups.TryGetValue(offsetAccount.AccountGroupID.Value, out offsetAccountGroup))
							{
								offsetAccountGroup = new PMAccountGroup();
							}
						}
					}
					#endregion

					RegisterReleaseProcess.AddToUnbilledSummary(this, tran);

					if (tran.ExcludedFromBalance == true)
						continue;

					try
					{
						ProcessTransaction(project, tran, account, accountGroup, offsetAccount, offsetAccountGroup, pb);
					}
					catch (IN.PXUnitConversionException ex)
					{
						IN.InventoryItem item = IN.InventoryItem.PK.Find(this, tran.InventoryID);
						string form = GetScreenName(item?.CreatedByScreenID);

						throw new PXException(ex, Messages.UnitConversionNotDefinedVerbose, tran.UOM, item?.BaseUnit, item?.InventoryCD, form);
					}
				}
			}

			RebuildAllocationTotals(project);

			if (options.RebuildCommitments == true)
			{
				ProcessCommitments(project);
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				RecalculateDraftInvoicesAmount(project, pb);
			}

			if (options.RecalculateChangeOrders == true && project.ChangeOrderWorkflow == true)
			{
				RecalculateChangeRequests(project, pb);
				RecalculateChangeOrders(project, pb);
			}

			InitCostCodeOnModifiedEntities();

			if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
			{
				ProcessRetainage(project, options);
			}

			if (Setup.Current.MigrationMode == true)
				RestoreBillingRecords(project);

			if (PXAccess.FeatureInstalled<FeaturesSet.construction>())
			{
				ProcessProgressWorksheets(pb, project, options);
			}

			ProcessInclusiveTaxes(project, options);
		}

		public virtual void ProcessTransaction(PMProject project, PMTran tran, Account acc, PMAccountGroup ag, Account offsetAcc, PMAccountGroup offsetAg, ProjectBalance pb)
		{
			IList<ProjectBalance.Result> balances = pb.Calculate(project, tran, acc, ag, offsetAcc, offsetAg);

			foreach (ProjectBalance.Result balance in balances)
			{
				if (balance.Status != null)
				{
					PMBudgetAccum ps = new PMBudgetAccum
					{
						ProjectID = balance.Status.ProjectID,
						ProjectTaskID = balance.Status.ProjectTaskID,
						AccountGroupID = balance.Status.AccountGroupID,
						InventoryID = balance.Status.InventoryID,
						CostCodeID = balance.Status.CostCodeID,
						UOM = balance.Status.UOM,
						IsProduction = balance.Status.IsProduction,
						Type = balance.Status.Type,
						Description = balance.Status.Description,
						CuryInfoID = balance.Status.CuryInfoID
					};

					ps = Budget.Insert(ps);

					ps.ActualQty += balance.Status.ActualQty.GetValueOrDefault();
					ps.CuryActualAmount += balance.Status.CuryActualAmount.GetValueOrDefault();
					ps.ActualAmount += balance.Status.ActualAmount.GetValueOrDefault();
					ps.ProgressBillingBase = null;
				}

				if (balance.ForecastHistory != null)
				{
					PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
					forecast.ProjectID = balance.ForecastHistory.ProjectID;
					forecast.ProjectTaskID = balance.ForecastHistory.ProjectTaskID;
					forecast.AccountGroupID = balance.ForecastHistory.AccountGroupID;
					forecast.InventoryID = balance.ForecastHistory.InventoryID;
					forecast.CostCodeID = balance.ForecastHistory.CostCodeID;
					forecast.PeriodID = balance.ForecastHistory.PeriodID;

					forecast = ForecastHistory.Insert(forecast);

					forecast.ActualQty += balance.ForecastHistory.ActualQty.GetValueOrDefault();
					forecast.CuryActualAmount += balance.ForecastHistory.CuryActualAmount.GetValueOrDefault();
					forecast.ActualAmount += balance.ForecastHistory.ActualAmount.GetValueOrDefault();
					forecast.CuryArAmount += balance.ForecastHistory.CuryArAmount.GetValueOrDefault();
				}

				if (balance.TaskTotal != null)
				{
					PMTaskTotal ta = new PMTaskTotal();
					ta.ProjectID = balance.TaskTotal.ProjectID;
					ta.TaskID = balance.TaskTotal.TaskID;

					ta = TaskTotals.Insert(ta);
					ta.CuryAsset += balance.TaskTotal.CuryAsset.GetValueOrDefault();
					ta.Asset += balance.TaskTotal.Asset.GetValueOrDefault();
					ta.CuryLiability += balance.TaskTotal.CuryLiability.GetValueOrDefault();
					ta.Liability += balance.TaskTotal.Liability.GetValueOrDefault();
					ta.CuryIncome += balance.TaskTotal.CuryIncome.GetValueOrDefault();
					ta.Income += balance.TaskTotal.Income.GetValueOrDefault();
					ta.CuryExpense += balance.TaskTotal.CuryExpense.GetValueOrDefault();
					ta.Expense += balance.TaskTotal.Expense.GetValueOrDefault();
				}


				foreach (PMHistory item in balance.History)
				{
					PMHistoryAccum hist = new PMHistoryAccum();
					hist.ProjectID = item.ProjectID;
					hist.ProjectTaskID = item.ProjectTaskID;
					hist.AccountGroupID = item.AccountGroupID;
					hist.InventoryID = item.InventoryID;
					hist.CostCodeID = item.CostCodeID;
					hist.PeriodID = item.PeriodID;
					hist.BranchID = item.BranchID;

					hist = History.Insert(hist);
					hist.FinPTDCuryAmount += item.FinPTDCuryAmount.GetValueOrDefault();
					hist.FinPTDAmount += item.FinPTDAmount.GetValueOrDefault();
					hist.FinYTDCuryAmount += item.FinYTDCuryAmount.GetValueOrDefault();
					hist.FinYTDAmount += item.FinYTDAmount.GetValueOrDefault();
					hist.FinPTDQty += item.FinPTDQty.GetValueOrDefault();
					hist.FinYTDQty += item.FinYTDQty.GetValueOrDefault();
					hist.TranPTDCuryAmount += item.TranPTDCuryAmount.GetValueOrDefault();
					hist.TranPTDAmount += item.TranPTDAmount.GetValueOrDefault();
					hist.TranYTDCuryAmount += item.TranYTDCuryAmount.GetValueOrDefault();
					hist.TranYTDAmount += item.TranYTDAmount.GetValueOrDefault();
					hist.TranPTDQty += item.TranPTDQty.GetValueOrDefault();
					hist.TranYTDQty += item.TranYTDQty.GetValueOrDefault();
				}
			}
		}

		public virtual void RebuildAllocationTotals(PMProject project)
		{
			PXSelectBase<PMTran> select = new PXSelectJoin<PMTran,
				LeftJoin<PMTranReversal, On<PMTranReversal.origTranID, Equal<PMTran.tranID>>>,
				Where<PMTran.origProjectID, Equal<Required<PMTran.origProjectID>>,
				And<PMTran.origTaskID, IsNotNull,
				And<PMTran.origAccountGroupID, IsNotNull,
				And<PMTranReversal.tranID, IsNull>>>>>(this);

			using (new PXFieldScope(select.View
				, typeof(PMTran.tranID)
				, typeof(PMTran.origProjectID)
				, typeof(PMTran.origTaskID)
				, typeof(PMTran.costCodeID)
				, typeof(PMTran.inventoryID)
				, typeof(PMTran.origAccountGroupID)
				, typeof(PMTran.qty)
				, typeof(PMTran.amount)))
			{
				foreach (PMTran tran in select.Select(project.ContractID))
				{
					PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
					tat.ProjectID = tran.OrigProjectID;
					tat.TaskID = tran.OrigTaskID;
					tat.AccountGroupID = tran.OrigAccountGroupID;
					tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
					tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

					tat = AllocationTotals.Insert(tat);
					tat.Amount += tran.Amount.GetValueOrDefault();
					tat.Quantity += tran.Qty.GetValueOrDefault();
				}
			}
		}

		public virtual void ProcessCommitments(PMProject project)
		{
			foreach (PXResult<POLine, POOrder> res in polines.Select(project.ContractID))
			{
				POLine poline = (POLine)res;
				POOrder order = (POOrder)res;
				PXParentAttribute.SetParent(polines.Cache, poline, typeof(POOrder), order);

				PMCommitmentAttribute.Sync(polines.Cache, poline);
			}
		}



		public virtual void RecalculateDraftInvoicesAmount(PMProject project, ProjectBalance pb)
		{
			var selectProforma = new PXSelectJoinGroupBy<PMProformaLine,
				InnerJoin<Account, On<PMProformaLine.accountID, Equal<Account.accountID>>,
				InnerJoin<PMProforma, On<PMProformaLine.refNbr, Equal<PMProforma.refNbr>, And<PMProformaLine.revisionID, Equal<PMProforma.revisionID>>>,
				InnerJoin<GL.Branch, On<PMProforma.branchID, Equal<GL.Branch.branchID>>>>>,
				Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.released, Equal<False>,
				And<PMProformaLine.corrected, Equal<False>,
				And<Account.accountGroupID, IsNotNull>>>>,
				Aggregate<GroupBy<PMProformaLine.projectID,
				GroupBy<PMProformaLine.taskID,
				GroupBy<PMProformaLine.accountID,
				GroupBy<PMProformaLine.inventoryID,
				GroupBy<PMProformaLine.costCodeID,
				GroupBy<PMProforma.branchID,
				GroupBy<PMProformaLine.uOM,
				Sum<PMProformaLine.curyLineTotal,
				Sum<PMProformaLine.lineTotal,
				Sum<PMProformaLine.qty>>>>>>>>>>>>(this);

			var selectInvoice = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>>,
				InnerJoin<ARRegister, On<ARTran.refNbr, Equal<ARRegister.refNbr>, And<ARTran.tranType, Equal<ARRegister.docType>>>,
				InnerJoin<GL.Branch, On<ARRegister.branchID, Equal<GL.Branch.branchID>>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<False>,
				And<Account.accountGroupID, IsNotNull,
				And<ARRegister.scheduled, Equal<False>,
				And<ARRegister.voided, Equal<False>>>>>>,
				Aggregate<GroupBy<ARTran.tranType,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.accountID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				GroupBy<ARRegister.branchID,
				GroupBy<ARTran.uOM,
				Sum<ARTran.curyExtPrice,
				Sum<ARTran.extPrice,
				Sum<ARTran.qty>>>>>>>>>>>>>(this);
						
			foreach (PXResult<PMProformaLine, Account, PMProforma, GL.Branch> res in selectProforma.Select(project.ContractID))
			{
				PMProformaLine line = (PMProformaLine)res;
				Account account = (Account)res;
				PMProforma doc = (PMProforma)res;
				GL.Branch branch = (GL.Branch)res;

				PMBudgetAccum invoiced = GetTargetBudget(project, account.AccountGroupID, line);
				if (invoiced != null)
				{
					invoiced = Budget.Insert(invoiced);
					decimal amount = MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.InvoiceDate, line.CuryLineTotal);
					IN.INUnitAttribute.TryConvertGlobalUnits(this, line.UOM, invoiced.UOM, line.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out decimal qty);
					invoiced.CuryInvoicedAmount += amount;
					invoiced.InvoicedQty += qty;
					invoiced.ProgressBillingBase = null;
					if (line.IsPrepayment == true)
					{
						invoiced.CuryPrepaymentInvoiced += amount;
					}
				}
			}

			foreach (PXResult<ARTran, Account, ARRegister, GL.Branch> res in selectInvoice.Select(project.ContractID))
			{
				ARTran line = (ARTran)res;
				Account account = (Account)res;
				ARRegister doc = (ARRegister)res;
				GL.Branch branch = (GL.Branch)res;

				PMBudgetAccum invoiced = GetTargetBudget(project, account.AccountGroupID, line);
				if (invoiced != null)
				{
					invoiced = Budget.Insert(invoiced);

					decimal? sign = ARDocType.SignAmount(line.TranType);
					invoiced.CuryInvoicedAmount += sign * MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.DocDate, line.CuryExtPrice);
					IN.INUnitAttribute.TryConvertGlobalUnits(this, line.UOM, invoiced.UOM, line.Qty.GetValueOrDefault(), IN.INPrecision.QUANTITY, out decimal qty);
					invoiced.InvoicedQty += sign * qty;
					invoiced.ProgressBillingBase = null;
				}
			}
		}

		public virtual void RecalculateChangeOrders(PMProject project, ProjectBalance projectBalance)
		{
			var select = new PXSelectJoin<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderBudget.refNbr>>>,
				Where<PMChangeOrderBudget.projectID, Equal<Required<PMChangeOrderBudget.projectID>>>>(this);

			foreach (PXResult<PMChangeOrderBudget, PMChangeOrder> res in select.Select(project.ContractID))
			{
				PMChangeOrderBudget change = (PMChangeOrderBudget)res;
				PMChangeOrder order = (PMChangeOrder)res;

				UpdateChangeBuckets(change, projectBalance, project, order.Date);
			}
		}

		public virtual void RecalculateChangeRequests(PMProject project, ProjectBalance projectBalance)
		{
			var select = new PXSelectJoin<PMChangeRequestLine,
				InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestLine.refNbr>>>,
				Where<PMChangeRequestLine.projectID, Equal<Required<PMChangeRequestLine.projectID>>,
				And<PMChangeRequest.released, Equal<False>,
				And<PMChangeRequest.approved, Equal<True>>>>>(this);

			foreach (PXResult<PMChangeRequestLine, PMChangeRequest> res in select.Select(project.ContractID))
			{
				PMChangeRequestLine change = (PMChangeRequestLine)res;
				PMChangeRequest request = (PMChangeRequest)res;

				PMChangeOrderBudget cost = new PMChangeOrderBudget();
				cost.ProjectID = change.ProjectID;
				cost.ProjectTaskID = change.CostTaskID;
				cost.AccountGroupID = change.CostAccountGroupID;
				cost.InventoryID = change.InventoryID;
				cost.CostCodeID = change.CostCodeID;
				cost.Qty = change.Qty;
				cost.Amount = change.ExtCost;
				cost.UOM = change.UOM;

				if (cost.TaskID != null && cost.AccountGroupID != null)
				{
					UpdateChangeBuckets(cost, projectBalance, project, request.Date);
				}

				PMChangeOrderBudget revenue = new PMChangeOrderBudget();
				revenue.ProjectID = change.ProjectID;
				revenue.ProjectTaskID = change.RevenueTaskID;
				revenue.AccountGroupID = change.RevenueAccountGroupID;
				revenue.InventoryID = change.InventoryID;
				revenue.CostCodeID = change.RevenueCodeID;
				revenue.Qty = change.Qty;
				revenue.Amount = change.LineAmount;
				revenue.UOM = change.UOM;

				if (revenue.TaskID != null && revenue.AccountGroupID != null)
				{
					UpdateChangeBuckets(revenue, projectBalance, project, request.Date);
				}
			}

			var selectMarkups = new PXSelectJoin<PMChangeRequestMarkup,
				InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestMarkup.refNbr>>>,
				Where<PMChangeRequest.projectID, Equal<Required<PMChangeRequest.projectID>>,
				And<PMChangeRequest.released, Equal<False>,
				And<PMChangeRequest.approved, Equal<True>>>>>(this);

			foreach (PXResult<PMChangeRequestMarkup, PMChangeRequest> res in selectMarkups.Select(project.ContractID))
			{
				PMChangeRequestMarkup markup = (PMChangeRequestMarkup)res;
				PMChangeRequest request = (PMChangeRequest)res;

				PMChangeOrderBudget revenue = new PMChangeOrderBudget();
				revenue.ProjectID = request.ProjectID;
				revenue.ProjectTaskID = markup.TaskID;
				revenue.AccountGroupID = markup.AccountGroupID;
				revenue.InventoryID = markup.InventoryID;
				revenue.CostCodeID = markup.CostCodeID;
				revenue.Amount = markup.MarkupAmount;

				if (revenue.TaskID != null && revenue.AccountGroupID != null)
				{
					UpdateChangeBuckets(revenue, projectBalance, project, request.Date);
				}
			}
		}

		protected virtual void UpdateChangeBuckets(PMChangeOrderBudget change, ProjectBalance projectBalance, PMProject project, DateTime? changeDate)
		{
			PMBudgetAccum budget = null;

			PMBudgetLite existing = budgetService.SelectProjectBalance(change, AccountGroups[change.AccountGroupID.Value], project, out bool isExisting);
			if (isExisting)
			{
				budget = Budget.Insert(new PMBudgetAccum
				{
					ProjectID = existing.ProjectID,
					ProjectTaskID = existing.ProjectTaskID,
					AccountGroupID = existing.AccountGroupID,
					InventoryID = existing.InventoryID,
					CostCodeID = existing.CostCodeID,
					UOM = existing.UOM,
					CuryInfoID = project.CuryInfoID
				});

				if (change.Released == true)
				{
					budget.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
					budget.CuryRevisedAmount += change.Amount.GetValueOrDefault();
				}
				else
				{
					budget.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
				}


				var rollupQty = projectBalance.CalculateRollupQty(change, existing);
				if (rollupQty != 0)
				{
					if (change.Released == true)
					{
						budget.ChangeOrderQty += change.Qty.GetValueOrDefault();
						budget.RevisedQty += change.Qty.GetValueOrDefault();
					}
					else
					{
						budget.DraftChangeOrderQty += change.Qty.GetValueOrDefault();
					}
				}
			}
			else
			{
				if (AccountGroups.TryGetValue(change.AccountGroupID.Value, out PMAccountGroup accountGroup))
				{
					budget = Budget.Insert(new PMBudgetAccum
					{
						ProjectID = existing.ProjectID,
						ProjectTaskID = existing.ProjectTaskID,
						AccountGroupID = existing.AccountGroupID,
						InventoryID = existing.InventoryID,
						CostCodeID = existing.CostCodeID,
						Type = existing.Type,
						Description = existing.Description,
						IsProduction = existing.IsProduction,
						CuryInfoID = project.CuryInfoID
					});
					if (change.Released == true)
					{
						budget.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
						budget.CuryRevisedAmount += change.Amount.GetValueOrDefault();
					}
					else
					{
						budget.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
					}
				}
			}

			if (budget != null)
			{
				FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(changeDate, FinPeriod.organizationID.MasterValue);

				if (finPeriod != null)
				{
					PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
					forecast.ProjectID = budget.ProjectID;
					forecast.ProjectTaskID = budget.ProjectTaskID;
					forecast.AccountGroupID = budget.AccountGroupID;
					forecast.InventoryID = budget.InventoryID;
					forecast.CostCodeID = budget.CostCodeID;
					forecast.PeriodID = finPeriod.FinPeriodID;

					forecast = ForecastHistory.Insert(forecast);
					if (change.Released == true)
					{
						forecast.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
						forecast.ChangeOrderQty += change.Qty.GetValueOrDefault();
					}
					else
					{
						forecast.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
						forecast.DraftChangeOrderQty += change.Qty.GetValueOrDefault();
					}
				}
				else
				{
					PXTrace.WriteError("Failed to find FinPeriodID for date {0}", changeDate);
				}
			}
		}

		public virtual void InitAccountGroup()
		{
			if (AccountGroups == null)
			{
				AccountGroups = new Dictionary<int, PMAccountGroup>();
				foreach (PMAccountGroup ag in PXSelect<PMAccountGroup>.Select(this))
				{
					AccountGroups.Add(ag.GroupID.Value, ag);
				}
			}
		}

		public virtual void InitAccounts()
		{
			if (Accounts == null)
			{
				Accounts = new Dictionary<int, Account>();
				foreach (Account account in PXSelect<Account, Where<Account.accountGroupID, IsNotNull>>.Select(this))
				{
					Accounts.Add(account.AccountID.Value, account);
				}
			}
		}

		public virtual void InitCostCodeOnModifiedEntities()
		{
			if (CostCodeAttribute.UseCostCode())
			{
				int? defaultCostCodeID = CostCodeAttribute.DefaultCostCode;
				foreach (POLine line in polines.Cache.Updated)
				{
					if (line.CostCodeID == null)
					{
						polines.Cache.SetValue<POLine.costCodeID>(line, defaultCostCodeID);
					}
				}
			}
		}

		public virtual string GetAccountGroupType(int? accountGroup)
		{
			PMAccountGroup ag = AccountGroups[accountGroup.Value];

			if (ag.Type == PMAccountType.OffBalance)
				return ag.IsExpense == true ? GL.AccountType.Expense : ag.Type;
			else
				return ag.Type;
		}

		public virtual ProjectBalance CreateProjectBalance()
		{
			return new ProjectBalance(this, budgetService, ServiceLocator.Current.GetInstance<IProjectSettingsManager>());
		}

		public List<PXDataFieldParam> BuildBudgetClearCommand(PMValidationFilter options, PMProject project, int? accountGroupID)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			list.Add(new PXDataFieldRestrict<PMBudget.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.accountGroupID>(PXDbType.Int, 4, accountGroupID, PXComp.EQ));

			list.Add(new PXDataFieldAssign<PMBudget.type>(PXDbType.Char, 1, GetAccountGroupType(accountGroupID)));
			list.Add(new PXDataFieldAssign<PMBudget.curyActualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualAmount>(PXDbType.Decimal, 0m));

			if (options.RecalculateInclusiveTaxes == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInclTaxAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.inclTaxAmount>(PXDbType.Decimal, 0m));
			}

			list.Add(new PXDataFieldAssign<PMBudget.actualQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.retainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyTotalRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.totalRetainedAmount>(PXDbType.Decimal, 0m));

			if (options.RebuildCommitments == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedReceivedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOrigAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigAmount>(PXDbType.Decimal, 0m));
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyPrepaymentInvoiced>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.prepaymentInvoiced>(PXDbType.Decimal, 0m));
			}

			return list;
		}

		public List<PXDataFieldParam> BuildBudgetClearCommandWithChangeOrders(PMValidationFilter options, PMBudgetLiteEx status)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			list.Add(new PXDataFieldRestrict<PMBudget.projectID>(PXDbType.Int, 4, status.ProjectID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.accountGroupID>(PXDbType.Int, 4, status.AccountGroupID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.projectTaskID>(PXDbType.Int, 4, status.ProjectTaskID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.inventoryID>(PXDbType.Int, 4, status.InventoryID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.costCodeID>(PXDbType.Int, 4, status.CostCodeID, PXComp.EQ));

			list.Add(new PXDataFieldAssign<PMBudget.type>(PXDbType.Char, 1, GetAccountGroupType(status.AccountGroupID)));
			list.Add(new PXDataFieldAssign<PMBudget.curyActualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualAmount>(PXDbType.Decimal, 0m));

			if (options.RecalculateInclusiveTaxes == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInclTaxAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.inclTaxAmount>(PXDbType.Decimal, 0m));
			}

			list.Add(new PXDataFieldAssign<PMBudget.actualQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.revisedQty>(PXDbType.Decimal, status.Qty));
			list.Add(new PXDataFieldAssign<PMBudget.curyRevisedAmount>(PXDbType.Decimal, status.CuryAmount));
			list.Add(new PXDataFieldAssign<PMBudget.revisedAmount>(PXDbType.Decimal, status.Amount));
			list.Add(new PXDataFieldAssign<PMBudget.draftChangeOrderQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.changeOrderQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.changeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.retainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyTotalRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.totalRetainedAmount>(PXDbType.Decimal, 0m));

			if (options.RebuildCommitments == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedReceivedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOrigAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigAmount>(PXDbType.Decimal, 0m));
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyPrepaymentInvoiced>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.prepaymentInvoiced>(PXDbType.Decimal, 0m));
			}

			return list;
		}

		public List<PXDataFieldParam> BuildForecastClearCommand(PMValidationFilter options, int? projectID)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			AddRestrictorsForcast(options, projectID, list);
			AddFieldAssignsForecast(options, list);

			return list;
		}

		public virtual void AddRestrictorsForcast(PMValidationFilter options, int? projectID, List<PXDataFieldParam> list)
		{
			list.Add(new PXDataFieldRestrict(typeof(PMForecastHistory.projectID).Name, PXDbType.Int, 4, projectID, PXComp.EQ));
			//list.Add(new PXDataFieldRestrict(typeof(PMForecastHistory.accountGroupID).Name, PXDbType.Int, 4, status.AccountGroupID, PXComp.EQ));
		}

		public virtual void AddFieldAssignsForecast(PMValidationFilter options, List<PXDataFieldParam> list)
		{
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyActualAmount).Name, PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.actualAmount).Name, PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyArAmount).Name, PXDbType.Decimal, 0m));

			if (options.RecalculateInclusiveTaxes == true)
			{
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyInclTaxAmount).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.inclTaxAmount).Name, PXDbType.Decimal, 0m));
			}

			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.actualQty).Name, PXDbType.Decimal, 0m));

			if (options.RecalculateChangeOrders == true)
			{
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.draftChangeOrderQty).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyDraftChangeOrderAmount).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.changeOrderQty).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyChangeOrderAmount).Name, PXDbType.Decimal, 0m));
			}
		}

		protected virtual void ProcessProgressWorksheets(ProjectBalance pb, PMProject project, PMValidationFilter options)
		{
			HashSet<BudgetKeyTuple> pwBudgetKeys = new HashSet<BudgetKeyTuple>();
			Dictionary<int, PMAccountGroup> addAccountGroups = new Dictionary<int, PMAccountGroup>();

			var select = new PXSelectJoin<PMProgressWorksheet,
				InnerJoin<PMProgressWorksheetLine, On<PMProgressWorksheet.refNbr, Equal<PMProgressWorksheetLine.refNbr>>>,
				Where<PMProgressWorksheet.projectID, Equal<Required<PMProgressWorksheet.projectID>>, And<PMProgressWorksheet.status, Equal<ProgressWorksheetStatus.closed>>>>(this);
			foreach (PXResult<PMProgressWorksheet, PMProgressWorksheetLine> line in select.Select(project.ContractID))
			{
				PMProgressWorksheetLine pwLine = line;
				BudgetKeyTuple budgetKey = BudgetKeyTuple.Create(pwLine);

				if (!pwBudgetKeys.Contains(budgetKey))
				{
					PMAccountGroup group;
					if (!AccountGroups.TryGetValue(pwLine.AccountGroupID.Value, out group) && !addAccountGroups.TryGetValue(pwLine.AccountGroupID.Value, out group))
					{
						group = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.SelectSingleBound(this, null, pwLine.AccountGroupID.Value);
						if (group != null)
						{
							addAccountGroups.Add(pwLine.AccountGroupID.Value, group);
						}
					}

					if (group != null)
					{
						bool isExisting;
						PMBudgetLite existing = budgetService.SelectProjectBalance(pwLine, group, project, out isExisting);
						if (isExisting == false)
						{
							PMBudgetAccum budget = Budget.Insert(new PMBudgetAccum
							{
								ProjectID = existing.ProjectID,
								ProjectTaskID = existing.ProjectTaskID,
								AccountGroupID = existing.AccountGroupID,
								InventoryID = existing.InventoryID,
								CostCodeID = existing.CostCodeID,
								Type = existing.Type,
								Description = existing.Description,
								UOM = existing.UOM,
								CuryInfoID = project.CuryInfoID
							});
							budget.ProductivityTracking = PMProductivityTrackingType.OnDemand;
						}
					}

					pwBudgetKeys.Add(budgetKey);
				}
			}
		}

		protected virtual void ProcessRetainage(PMProject project, PMValidationFilter options)
		{
			CurrencyInfo projectCurrencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, project.CuryInfoID);


			   //Retained Amount:
			   var selectReleasedNotByLines = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTran.tranType>, And<ARRegister.refNbr, Equal<ARTran.refNbr>>>,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<True>,
				And<ARRegister.paymentsByLinesAllowed, Equal<False>>>>,
				Aggregate<GroupBy<ARRegister.branchID,
				GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				GroupBy<ARTran.tranType,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, ARRegister, Account> res in selectReleasedNotByLines.Select(project.ContractID))
			{
				ARRegister doc = (ARRegister)res;
				ARTran tran = (ARTran)res;
				Account account = (Account)res;
								
				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.CuryRetainedAmount += MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.DocDate,
						tran.CuryRetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1));
				}
			}


			var selectReleasedByLines = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTran.tranType>, And<ARRegister.refNbr, Equal<ARTran.refNbr>>>,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<True>,
				And<ARRegister.paymentsByLinesAllowed, Equal<True>,
				And<ARRegister.isRetainageReversing, Equal<False>>>>>,
				Aggregate<GroupBy<ARRegister.branchID,
				GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				GroupBy<ARTran.tranType,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, ARRegister, Account> res in selectReleasedByLines.Select(project.ContractID))
			{
				ARRegister doc = (ARRegister)res;
				ARTran tran = (ARTran)res;
				Account account = (Account)res;

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.CuryRetainedAmount += MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.DocDate,
						tran.CuryRetainageBal.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1));
				}
			}

			//Draft Retained Amount:
			var selectUnreleased = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARTran.tranType>, And<ARRegister.refNbr, Equal<ARTran.refNbr>>>,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>, And<ARTran.released, Equal<False>>>,
				Aggregate<GroupBy<ARRegister.branchID,
				GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				GroupBy<ARTran.tranType,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, ARRegister, Account> res in selectUnreleased.Select(project.ContractID))
			{
				ARRegister doc = (ARRegister)res;
				ARTran tran = (ARTran)res;
				Account account = (Account)res;

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.CuryDraftRetainedAmount += MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.DocDate,
						tran.CuryRetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1));
				}
			}


			var selectProformaProgressive = new PXSelectJoinGroupBy<PMProformaLine,
			InnerJoin<PMProforma, On<PMProforma.refNbr, Equal<PMProformaLine.refNbr>, And<PMProforma.revisionID, Equal<PMProformaLine.revisionID>>>>,
			Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
				And<PMProformaLine.corrected, Equal<False>>>>,
			Aggregate<GroupBy<PMProforma.branchID,
			GroupBy<PMProformaLine.accountGroupID,
			GroupBy<PMProformaLine.projectID,
			GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.released,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyRetainage>>>>>>>>>>>(this);

			foreach (PXResult<PMProformaLine, PMProforma> res in selectProformaProgressive.Select(project.ContractID))
			{
				PMProformaLine tran = (PMProformaLine)res;
				PMProforma doc = (PMProforma)res;

				PMBudgetAccum retained = GetTargetBudget(project, tran.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					decimal retainedAmount = MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.InvoiceDate,
						tran.CuryRetainage.GetValueOrDefault());

					if (tran.Released != true)
						retained.CuryDraftRetainedAmount += retainedAmount;
					retained.CuryTotalRetainedAmount += retainedAmount;
				}
			}

			var selectProformaTransaction = new PXSelectJoinGroupBy<PMProformaLine,
			InnerJoin<PMProforma, On<PMProforma.refNbr, Equal<PMProformaLine.refNbr>, And<PMProforma.revisionID, Equal<PMProformaLine.revisionID>>>,
			InnerJoin<Account, On<PMProformaLine.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
			Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.transaction>,
				And<PMProformaLine.corrected, Equal<False>>>>,
			Aggregate<GroupBy<PMProforma.branchID,
			GroupBy<PMProformaLine.accountID,
			GroupBy<PMProformaLine.projectID,
			GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.released,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyRetainage>>>>>>>>>>>(this);

			foreach (PXResult<PMProformaLine, PMProforma, Account> res in selectProformaTransaction.Select(project.ContractID))
			{
				PMProformaLine tran = (PMProformaLine)res;
				PMProforma doc = (PMProforma)res;
				Account account = (Account)res;

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					decimal retainedAmount = MultiCurrencyService.GetValueInProjectCurrency(this, project, doc.CuryID, doc.InvoiceDate,
						tran.CuryRetainage.GetValueOrDefault());

					if (tran.Released != true)
						retained.CuryDraftRetainedAmount += retainedAmount;
					retained.CuryTotalRetainedAmount += retainedAmount;
				}
			}
		}

		public virtual (decimal? CuryAmount, decimal? Amount) GetInclusiveTaxAmount(PXGraph graph, ARTran tran)
		=> ProjectRevenueTaxAmountProvider.GetInclusiveTaxAmount(graph, tran);

		public virtual (decimal? CuryAmount, decimal? Amount) GetRetainedInclusiveTaxAmount(PXGraph graph, ARTran tran)
		=> ProjectRevenueTaxAmountProvider.GetRetainedInclusiveTaxAmount(graph, tran);

		protected virtual void ProcessInclusiveTaxes(PMProject project, PMValidationFilter options)
		{
			if (options?.RecalculateInclusiveTaxes != true)
				return;

			var arTrans = SelectFrom<ARTran>
				.InnerJoin<ARRegister>
					.On<ARRegister.docType.IsEqual<ARTran.tranType>
					.And<ARRegister.refNbr.IsEqual<ARTran.refNbr>>>
				.InnerJoin<Account>
					.On<ARTran.accountID.IsEqual<Account.accountID>
					.And<Account.accountGroupID.IsNotNull>>
				.Where<ARTran.projectID.IsEqual<@P.AsInt>
					.And<ARTran.released.IsEqual<True>>>
				.View.Select(this, project.ContractID);

			foreach (PXResult<ARTran, ARRegister, Account> res in arTrans)
			{
				ARRegister doc = (ARRegister)res;
				ARTran arTran = (ARTran)res;
				Account account = (Account)res;

				var sign = ARDocType.SignAmount(arTran.TranType);

				PMBudgetAccum budgetToUpdate = GetTargetBudget(project, account.AccountGroupID, arTran);

				if (budgetToUpdate == null)
					continue;

				budgetToUpdate = Budget.Insert(budgetToUpdate);

				var arTranInclTaxAmt = GetInclusiveTaxAmount(this, arTran);
				var arTranRetainedInclTaxAmt = GetRetainedInclusiveTaxAmount(this, arTran);

				var arTranCuryInclTaxTotalAmt = MultiCurrencyService.GetValueInProjectCurrency(
					this,
					project,
					doc.CuryID,
					doc.DocDate,
					arTranInclTaxAmt.CuryAmount + arTranRetainedInclTaxAmt.CuryAmount) * sign;

				var arTranInclTaxTotalAmt = (arTranInclTaxAmt.Amount + arTranRetainedInclTaxAmt.Amount) * sign;

				budgetToUpdate.CuryInclTaxAmount += arTranCuryInclTaxTotalAmt.GetValueOrDefault();
				budgetToUpdate.InclTaxAmount += arTranInclTaxTotalAmt.GetValueOrDefault();

				PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();

				forecast.ProjectID = budgetToUpdate.ProjectID;
				forecast.ProjectTaskID = budgetToUpdate.ProjectTaskID;
				forecast.AccountGroupID = budgetToUpdate.AccountGroupID;
				forecast.InventoryID = budgetToUpdate.InventoryID;
				forecast.CostCodeID = budgetToUpdate.CostCodeID;
				forecast.PeriodID = arTran.TranPeriodID;

				forecast = ForecastHistory.Insert(forecast);

				forecast.CuryInclTaxAmount += arTranCuryInclTaxTotalAmt.GetValueOrDefault();
				forecast.InclTaxAmount += arTranInclTaxTotalAmt.GetValueOrDefault();
			}
		}

		protected virtual void HandleProjectStatusCode(PMProject project, PMValidationFilter options)
		{
			if (StatusCodeHelper.CheckStatus(project.StatusCode, StatusCodes.InclusiveTaxesInRevenueBudgetIntroduced))
			{
				if (options.RecalculateInclusiveTaxes == true)
					ResetProjectStatusCode(project, StatusCodes.InclusiveTaxesInRevenueBudgetIntroduced);
			}
		}

		public static void ResetProjectStatusCode(PMProject project, StatusCodes statusCodeToReset)
		{
			PXDatabase.Update<Contract>(
				new PXDataFieldRestrict<PMProject.contractID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ),
				new PXDataFieldAssign<PMProject.statusCode>(PXDbType.Int, StatusCodeHelper.ResetStatus(project.StatusCode, statusCodeToReset)));
		}

		protected virtual void RestoreBillingRecords(PMProject project)
		{
			var selectBillingRecords = new PXSelect<PMBillingRecord,
			  Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>>,
			  OrderBy<Asc<PMBillingRecord.recordID>>>(this);

			HashSet<string> existingProformas = new HashSet<string>();

			foreach (PMBillingRecord record in selectBillingRecords.Select(project.ContractID))
			{
				existingProformas.Add(record.ProformaRefNbr);
			}

			var selectProformas = new PXSelect<PMProforma,
			  Where<PMProforma.projectID, Equal<Required<PMProforma.projectID>>,
			  And<PMProforma.corrected, Equal<False>>>,
			  OrderBy<Asc<PMProforma.refNbr>>>(this);

			foreach (PMProforma proforma in selectProformas.Select(project.ContractID))
			{
				if (!existingProformas.Contains(proforma.RefNbr))
				{
					project.BillingLineCntr = project.BillingLineCntr.GetValueOrDefault() + 1;
					Project.Update(project);

					PMBillingRecord billingRecord = (PMBillingRecord)BillingRecords.Cache.CreateInstance();
					billingRecord.ProjectID = project.ContractID;
					billingRecord.RecordID = project.BillingLineCntr;
					billingRecord.ProformaRefNbr = proforma.RefNbr;
					billingRecord.ARDocType = proforma.ARInvoiceDocType;
					billingRecord.ARRefNbr = proforma.ARInvoiceRefNbr;
					billingRecord.BillingTag = "P";
					billingRecord.Date = proforma.InvoiceDate;
					BillingRecords.Insert(billingRecord);
				}
			}

		}

		private string GetScreenName(string screenID)
		{
			string screenName = string.Empty;
			PXSiteMapNode node;

			if (!String.IsNullOrEmpty(screenID) && (node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID)) != null && !String.IsNullOrEmpty(node.Title))
			{
				screenName = string.Concat(node.Title, " ", screenID);
			}

			return screenName;
		}

		private PMBudgetAccum GetTargetBudget(PMProject project, int? accountGroupID, ARTran line)
		{
			if (AccountGroups.TryGetValue(accountGroupID.Value, out PMAccountGroup ag))
			{
				PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out bool isExisting);

				return new PMBudgetAccum
				{
					Type = budget.Type,
					ProjectID = budget.ProjectID,
					ProjectTaskID = budget.TaskID,
					AccountGroupID = budget.AccountGroupID,
					InventoryID = budget.InventoryID,
					CostCodeID = budget.CostCodeID,
					UOM = budget.UOM,
					Description = budget.Description,
					CuryInfoID = project.CuryInfoID
				};
			}
			else return null;
		}

		private PMBudgetAccum GetTargetBudget(PMProject project, int? accountGroupID, PMProformaLine line)
		{
			if (AccountGroups.TryGetValue(accountGroupID.Value, out PMAccountGroup ag))
			{
				PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out bool isExisting);

				return new PMBudgetAccum
				{
					Type = budget.Type,
					ProjectID = budget.ProjectID,
					ProjectTaskID = budget.TaskID,
					AccountGroupID = budget.AccountGroupID,
					InventoryID = budget.InventoryID,
					CostCodeID = budget.CostCodeID,
					UOM = budget.UOM,
					Description = budget.Description,
					CuryInfoID = project.CuryInfoID
				};
			}
			else return null;
		}

		[PXHidden]
		[Serializable]
		[PXBreakInheritance]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMBudgetLiteEx : PMBudgetLite
		{
			#region CuryAmount
			public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
			{
			}
			[PXDBDecimal]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Original Budgeted Amount")]
			public virtual Decimal? CuryAmount
			{
				get;
				set;
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
			{
			}
			[PXDBDecimal]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
			public virtual Decimal? Amount
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		[Serializable]
		[PXBreakInheritance]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class PMTranReversal : PMTran
		{
			public new abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID>
			{
			}

			public new abstract class origTranID : PX.Data.BQL.BqlLong.Field<origTranID>
			{
			}

			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
			{
			}
		}

		public class BudgetServiceMassUpdate : BudgetService
		{
			private int? projectID;
			private Dictionary<BudgetKeyTuple, PMBudgetLiteEx> Budget;
			private HashSet<int> accountGroups;

			public BudgetServiceMassUpdate(PXGraph graph, PMProject project) : base(graph)
			{
				this.projectID = project.ContractID;
			}

			protected override List<PMBudgetLite> SelectExistingBalances(int projectID, int taskID, int accountGroupID, int?[] costCodes, int?[] items)
			{
				if (Budget == null)
				{
					PreSelectProjectBudget();
				}

				List<PMBudgetLite> list = new List<PMBudgetLite>();
				foreach (int costCodeID in costCodes)
				{
					foreach (int inventoryID in items)
					{
						PMBudgetLiteEx result;
						if (Budget.TryGetValue(new BudgetKeyTuple(projectID, taskID, accountGroupID, inventoryID, costCodeID), out result))
						{
							list.Add(result);
						}
					}
				}

				return list;
			}

			private void PreSelectProjectBudget()
			{
				Budget = new Dictionary<BudgetKeyTuple, PMBudgetLiteEx>();
				accountGroups = new HashSet<int>();

				PXSelectBase<PMBudgetLiteEx> selectBudget = new PXSelect<PMBudgetLiteEx,
					Where<PMBudgetLiteEx.projectID, Equal<Required<PMBudgetLiteEx.projectID>>>>(graph);

				foreach (PMBudgetLiteEx budget in selectBudget.Select(projectID))
				{
					Budget.Add(BudgetKeyTuple.Create(budget), budget);
					accountGroups.Add(budget.AccountGroupID.Value);
				}
			}

			public ICollection<int> GetUsedAccountGroups()
			{
				if (Budget == null)
				{
					PreSelectProjectBudget();
				}

				return accountGroups;
			}

			public Dictionary<BudgetKeyTuple, PMBudgetLiteEx>.ValueCollection BudgetRecords
			{
				get
				{
					if (Budget == null)
					{
						PreSelectProjectBudget();
					}

					return Budget.Values;
				}
			}


		}
	}

	[PXPrimaryGraph(typeof(ProjectBalanceValidation))]
	[PXCacheName(Messages.RecalculateProjectBalances)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMValidationFilter : IBqlTable
	{
		#region RecalculateProjectBalances
		public abstract class recalculateProjectBalances : PX.Data.BQL.BqlBool.Field<recalculateProjectBalances> { }

		/// <summary>
		/// A check box that indicates (if selected) that balances will be recalculated even if extra check boxes are not selected after you click Process or Process All on the form toolbar.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Project Balances", IsReadOnly = true)]
		public virtual Boolean? RecalculateProjectBalances
		{
			get;
			set;
		}
		#endregion
		#region RecalculateUnbilledSummary
		public abstract class recalculateUnbilledSummary : PX.Data.BQL.BqlBool.Field<recalculateUnbilledSummary> { }
		protected Boolean? _RecalculateUnbilledSummary;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Unbilled Summary")]
		public virtual Boolean? RecalculateUnbilledSummary
		{
			get
			{
				return this._RecalculateUnbilledSummary;
			}
			set
			{
				this._RecalculateUnbilledSummary = value;
			}
		}
		#endregion
		#region RecalculateDraftInvoicesAmount
		public abstract class recalculateDraftInvoicesAmount : PX.Data.BQL.BqlBool.Field<recalculateDraftInvoicesAmount> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Draft Invoice Amount and Quantity")]
		public virtual Boolean? RecalculateDraftInvoicesAmount
		{
			get;
			set;
		}
		#endregion
		#region RebuildCommitments
		public abstract class rebuildCommitments : PX.Data.BQL.BqlBool.Field<rebuildCommitments> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Rebuild Commitments")]
		public virtual Boolean? RebuildCommitments
		{
			get;
			set;
		}
		#endregion
		#region RecalculateChangeOrders
		public abstract class recalculateChangeOrders : PX.Data.BQL.BqlBool.Field<recalculateChangeOrders> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Change Orders", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Boolean? RecalculateChangeOrders
		{
			get;
			set;
		}
		#endregion
		#region RecalculateInclusiveTaxes
		public abstract class recalculateInclusiveTaxes : PX.Data.BQL.BqlBool.Field<recalculateInclusiveTaxes> { }

		/// <summary>
		/// A Boolean value that indicates (if the value is <see langword="true" />) that inclusive taxes will be recalculated.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Recalculate Inclusive Taxes")]
		public virtual Boolean? RecalculateInclusiveTaxes
		{
			get;
			set;
		}
		#endregion
	}
}
