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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR.CustomerStatements;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions;
using PX.Objects.Common.Aging;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{

	[PXHidden]
	public partial class StatementCycleProcessBO : PXGraph<StatementCycleProcessBO>
	{
		public StatementCycleProcessBO()
		{
			ARSetup setup = ARSetup.Current;
		}

		#region Internal Types Definition
		[PXProjection(typeof(Select5<
			ARBalances,
			InnerJoin<Customer,
				On<Customer.bAccountID, Equal<ARBalances.customerID>>>,
			Where<ARBalances.lastDocDate, IsNull,
				Or<
					Where<ARBalances.statementRequired, Equal<True>,
					Or<ARBalances.lastDocDate, Greater<Customer.statementLastDate>>>>>,
			Aggregate<
				GroupBy<ARBalances.customerID>>>)
		, Persistent = false)]
		public partial class CustomerWithActiveBalance : PX.Data.IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(IsKey = true, BqlField = typeof(ARBalances.customerID))]
			public virtual int? CustomerID { get; set; }
			#endregion
		}
		#endregion

		#region Public Views
		public PXSetup<ARSetup> ARSetup;

		public PXSelect<
			ARStatementCycle,
			Where<
				ARStatementCycle.statementCycleId, Equal<Required<ARStatementCycle.statementCycleId>>>>
			CyclesList;

		public PXSelect<ARRegister> Register;
		#endregion

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region External Processing Functions
		public static void ProcessCycles(StatementCycleProcessBO graph, ARStatementCycle aCycle)
		{
			if (aCycle.NextStmtDate == null)
			{
				return;
			}

			graph.Clear();

			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
			DateTime statementDate = aCycle.NextStmtDate ?? graph.Accessinfo.BusinessDate.Value;

			PXProcessing<ARStatementCycle>.SetCurrentItem(aCycle);

			graph.GenerateStatement(cycle, statementDate);
		}

		public static void RegenerateLastStatement(StatementCycleProcessBO graph, ARStatementCycle aCycle)
		{
			graph.Clear();
			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
			if (cycle.LastStmtDate != null)
			{
				DateTime stmtDate = (DateTime)cycle.LastStmtDate;
				graph.RegenerateLastStatement(cycle, stmtDate);
			}
		}
		public static void DeleteLastStatement(StatementCycleProcessBO graph, ARStatementCycle aCycle)
		{
			graph.Clear();
			ARStatementCycle cycle = graph.CyclesList.Select(aCycle.StatementCycleId);
			if (cycle.LastStmtDate != null)
			{
				DateTime stmtDate = (DateTime)cycle.LastStmtDate;
				graph.DeleteLastStatementExec(cycle, stmtDate);
			}
		}

		private static IEnumerable<Customer> GetStatementCustomers(PXGraph graph, IEnumerable<Customer> customers)
		{
			List<Customer> result = new List<Customer>();

			foreach (int? customerID in customers
				.Select(customer => customer.StatementCustomerID)
				.Distinct())
			{
				IEnumerable<Customer> customersThatConsolidateToThisOne = PXSelect<
					Customer,
					Where<
						Customer.statementCustomerID, Equal<Required<Customer.statementCustomerID>>>>
					.Select(graph, customerID)
					.RowCast<Customer>();

				result.AddRange(customersThatConsolidateToThisOne);
			}

			return result;
		}

		private class CustomerIDComparer : IEqualityComparer<Customer>
		{
			public bool Equals(Customer x, Customer y)
			{
				return x.BAccountID == y.BAccountID;
			}

			public int GetHashCode(Customer obj)
			{
				return obj.BAccountID.GetHashCode();
			}
		}

		/// <summary>
		/// After parent-child relationship is broken and a user wants to regenerate last statement
		/// for a customer that was in the family earlier, this method ensures that customers that 
		/// "were family" at the moment of previous statement generation will get into the family 
		/// for regeneration.
		/// </summary>
		/// <param name="customerFamily">
		/// The customer family collected on the basis of parent-child links between customers.
		/// </param>
		private static ICollection<Customer> PrependFamilyCustomersFromPreviousStatement(
			PXGraph graph,
			IEnumerable<Customer> customerFamily,
			DateTime previousStatementDate)
		{
			int[] customerIDs = customerFamily
				.Select(customer => customer.BAccountID.Value)
				.ToArray();

			IBqlSelect previousCustomersBase = new SelectFrom<Customer>
				// Collect customers from previous statements' ARStatement.statementCustomerID.
				.LeftJoin<ARStatement>
					.On<Customer.bAccountID.IsEqual<ARStatement.customerID>
						.And<ARStatement.statementDate.IsEqual<@P.AsDateTime.UTC>>
						.And<ARStatement.statementCustomerID.IsIn<@P.AsInt>>>
				.Where<ARStatement.statementDate.IsNotNull
					.Or<ARStatementDetail.statementDate.IsNotNull>>();

			CustomerIDComparer customerIDComparer = new CustomerIDComparer();

			// Also collect customers from previous statement applications, because
			// e.g. on second re-generation the statement records have already lost
			// the information about the family.
			BqlCommand previousCustomersBaseAdjg = previousCustomersBase.AddNewJoin(typeof(LeftJoin<ARStatementDetail,
				On<ARStatementDetail.statementDate.IsEqual<@P.AsDateTime.UTC>
					.And<Customer.bAccountID.IsEqual<ARStatementDetail.customerID>>
					.And<ARStatementDetail.customerID.IsIn<@P.AsInt>>>>));

			return previousCustomersBaseAdjg
				.CreateView(graph)
				.SelectMulti(
					previousStatementDate,
					customerIDs,
					previousStatementDate,
					customerIDs)
				.RowCast<Customer>()
			.Concat(customerFamily)
				.Distinct(customerIDComparer)
			.ToArray();
		}

		public static void RegenerateStatements(StatementCycleProcessBO graph, ARStatementCycle cycle, IEnumerable<Customer> customers)
		{
			RegenerateStatementsExplicitStmtDate(graph, cycle, customers, null);
		}

		public static void RegenerateStatementsExplicitStmtDate(StatementCycleProcessBO graph, ARStatementCycle cycle, IEnumerable<Customer> customers, DateTime? explicitStmtDate)
		{
			graph.Clear();

			if (cycle.LastStmtDate != null || explicitStmtDate != null)
			{
				DateTime stmtDate = explicitStmtDate == null ? (DateTime)cycle.LastStmtDate : explicitStmtDate.GetValueOrDefault();

				StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

				IEnumerable<IEnumerable<Customer>> customerFamilies = GetStatementCustomers(graph, customers)
					.GroupBy(customer => customer.StatementCustomerID)
					.Select(family => PrependFamilyCustomersFromPreviousStatement(graph, family, stmtDate))
					.OrderByDescending(family => family.Count);

				HashSet<int> processedCustomers = new HashSet<int>();

				foreach (IEnumerable<Customer> extendedFamily in customerFamilies)
				{
					// The trick with excluding processed customers is required when 
					// re-generating statements for customers that were a family during 
					// previous cycle, but are not now. The process of prepending previous
					// family customers may result multiple size families containing 
					// the same customers. We will only process the largest one, thus
					// avoiding lock violations.
					// -
					if (extendedFamily.Any(customer => processedCustomers.Contains(customer.BAccountID.Value)))
					{
						continue;
					}

					graph.GenerateStatementForCustomerFamily(
						statementGraph,
						cycle,
						extendedFamily,
						stmtDate,
						true,
						false);

					processedCustomers.UnionWith(extendedFamily.Select(customer => customer.BAccountID.Value));
				}
			}
		}

		public static void GenerateOnDemandStatement(
			StatementCycleProcessBO graph,
			ARStatementCycle statementCycle,
			Customer customer,
			DateTime statementDate)
		{
			graph.Clear();

			StatementCreateBO persistGraph = PXGraph.CreateInstance<StatementCreateBO>();

			IEnumerable<Customer> customerFamily = GetStatementCustomers(graph, customer.AsSingleEnumerable());

			graph.GenerateStatementForCustomerFamily(persistGraph, statementCycle, customerFamily, statementDate, true, true);
		}
		#endregion

		#region Internal Processing functions

		protected virtual void GenerateStatement(ARStatementCycle cycle, DateTime statementDate)
		{
			bool hasInactiveBranches = PXSelect<Branch, Where<Branch.active, Equal<False>>>.Select(this).Any();
			Dictionary<int, HashSet<string>> inactiveBranchesOfCustomers = new Dictionary<int, HashSet<string>>();

			#region check inactive branches
			if (hasInactiveBranches)
			{
				PXSelectBase<Branch> viewInactiveBranchesOfCustomers =
					new PXSelectJoinGroupBy<Branch,
						InnerJoin<ARStatement, On<ARStatement.branchID, Equal<Branch.branchID>>>,
						Where<Branch.active, Equal<False>,
							And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
							And<ARStatement.statementCycleId, Equal<Required<ARStatement.statementCycleId>>>>>,
						Aggregate<
							GroupBy<ARStatement.customerID,
							GroupBy<Branch.branchID,
							GroupBy<Branch.branchCD>>>>
						>(this);

				using (new PXFieldScope(viewInactiveBranchesOfCustomers.View, typeof(ARStatement.customerID), typeof(Branch.branchCD)))
				{
					int customerID;
					string branchCD;
					HashSet<string> setInactiveBranchesOfCustomers;
					foreach (PXResult<Branch, ARStatement> ba in viewInactiveBranchesOfCustomers.Select(cycle.LastStmtDate, cycle.StatementCycleId))
					{
						customerID = ((ARStatement)ba).CustomerID.Value;
						branchCD = ((Branch)ba).BranchCD;
						if (!inactiveBranchesOfCustomers.TryGetValue(customerID, out setInactiveBranchesOfCustomers))
						{
							setInactiveBranchesOfCustomers = new HashSet<string>();
							inactiveBranchesOfCustomers.Add(customerID, setInactiveBranchesOfCustomers);
						}
						if (!setInactiveBranchesOfCustomers.Contains(branchCD))
						{
							setInactiveBranchesOfCustomers.Add(branchCD);
						}
					}
				}
			}
			#endregion

			StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

			IEnumerable<IEnumerable<Customer>> customerFamilies = CollectCustomerFamiliesForCycleProcessing(cycle, statementDate);

			ICollection<string> customersWithOnDemandStatementsOnDate = new List<string>();
			ICollection<string> customersWithExistingStatementsOnDate = new List<string>();
			HashSet<string> inactiveBranches = new HashSet<string>();

			foreach (IEnumerable<Customer> customerFamily in customerFamilies)
			{
				// If at least one of the customers in the family has on-demand statements
				// on the specified date, the statements will be re-generated for the whole
				// family.
				// -
				bool familyHasOnDemandStatementsOnDate = new PXSelect<
					ARStatement,
					Where<
						ARStatement.customerID, In<Required<ARStatement.customerID>>,
						And<ARStatement.onDemand, Equal<True>,
						And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>>>>>(this)
					.Any(
						customerFamily.Select(customer => customer.BAccountID).ToArray(),
						statementDate);

				if (familyHasOnDemandStatementsOnDate)
				{
					customersWithOnDemandStatementsOnDate.Add(
						customerFamily.First().AcctCD.Trim());
				}

				IEnumerable<Customer> excludedCustomers =
					customerFamily.Where(customer => customer.StatementLastDate >= statementDate);

				PX.Objects.Common.Extensions.CollectionExtensions.AddRange(customersWithExistingStatementsOnDate,
					excludedCustomers.Select(customer => customer.AcctCD));

				IEnumerable<Customer> neededCustomerFamily = customerFamily.Except(excludedCustomers);

				if (neededCustomerFamily.Count() > 0)
				{
					#region check inactive branches
					if (hasInactiveBranches)
					{
						foreach (Customer c in neededCustomerFamily)
						{
							if (inactiveBranchesOfCustomers.TryGetValue(c.BAccountID.Value, out HashSet<string> setInactiveBranches))
							{
								if (setInactiveBranches.Count > 0)
								{
									PXTrace.WriteWarning(AR.Messages.DocumentsOfFollowingCustomersForInactiveBranchesHaveBeenExcludedFromPreparedStatements,
										c.AcctCD,
										string.Join(", ", setInactiveBranches));
									inactiveBranches.AddRange(setInactiveBranches);
								}
							}
						}
					}
					#endregion

					GenerateStatementForCustomerFamily(
						statementGraph,
						cycle,
							neededCustomerFamily,
						statementDate,
						clearExisting: familyHasOnDemandStatementsOnDate,
						isOnDemand: false);
				}
			}

			if (customersWithOnDemandStatementsOnDate.Any())
			{
				PXTrace.WriteWarning(
					Messages.ExistingOnDemandStatementsForCustomersOverwritten,
					string.Join(", ", customersWithOnDemandStatementsOnDate));
			}

			if (customersWithExistingStatementsOnDate.Any())
			{
				PXTrace.WriteWarning(
					Messages.CustomersExcludedBecauseStatementsAlreadyExistForDate,
					string.Join(", ", customersWithExistingStatementsOnDate));

				PXProcessing<ARStatementCycle>.SetWarning(Messages.CustomersExcludedFromStatementGeneration);
			}

			if (inactiveBranches.Count > 0)
			{
				PXProcessing<ARStatementCycle>.SetWarning(
					string.Format(AR.Messages.DocumentsOfInactiveBranchHaveBeenExcludedFromPreparedStatements,
					string.Join(",", inactiveBranches))
				);
			}

			UpdateStatementCycleLastStatementDate(
				cycle,
				statementDate);
		}

		protected virtual void RegenerateLastStatement(ARStatementCycle cycle, DateTime statementDate)
		{
			StatementCreateBO statementGraph = CreateInstance<StatementCreateBO>();

			IEnumerable<IEnumerable<Customer>> customerFamilies = CollectCustomerFamiliesForCycleProcessing(cycle, statementDate)
				// Customers who have been transferred from another statement cycle may have
				// last statement date later than the current statement date. They will be
				// excluded from processing.
				// -
				.Where(family => !family.Any(customer => customer.StatementLastDate > statementDate))
				.Select(family => PrependFamilyCustomersFromPreviousStatement(statementGraph, family, statementDate))
				.OrderByDescending(family => family.Count);

			HashSet<int> processedCustomers = new HashSet<int>();

			foreach (IEnumerable<Customer> extendedFamily in customerFamilies)
			{
				// The trick with excluding processed customers is required when 
				// re-generating statements for customers that were a family during 
				// previous cycle, but are not now. The process of prepending previous
				// family customers may result multiple size families containing 
				// the same customers. We will only process the largest one, thus
				// avoiding lock violations.
				// -
				if (extendedFamily.Any(customer => processedCustomers.Contains(customer.BAccountID.Value)))
				{
					continue;
				}

				GenerateStatementForCustomerFamily(
					statementGraph,
					cycle,
					extendedFamily,
					statementDate,
					clearExisting: true,
					isOnDemand: false);

				processedCustomers.UnionWith(extendedFamily.Select(customer => customer.BAccountID.Value));
			}

			UpdateStatementCycleLastStatementDate(
				cycle,
				statementDate);
		}

		protected virtual void DeleteLastStatementExec(ARStatementCycle cycle, DateTime deleteStatementDate)
		{
			StatementCreateBO pGraph = CreateInstance<StatementCreateBO>();
			IEnumerable<IEnumerable<Customer>> customerFamilies = CollectCustomerFamiliesForCycleDeleting(cycle, deleteStatementDate)
				.Select(family => PrependFamilyCustomersFromPreviousStatement(pGraph, family, deleteStatementDate))
				.OrderByDescending(family => family.Count);
			HashSet<int> processedCustomers = new HashSet<int>();
			var prevStatementDate = GetPreviousStatementDate(cycle);

			var allCustomers = new List<int>();

			foreach (IEnumerable<Customer> extendedFamily in customerFamilies)
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (Customer curCustomer in extendedFamily)
					{
						// if deleting statement is not last for customer let's skip this customer
						if (curCustomer.StatementLastDate != null && curCustomer.StatementLastDate.Value > deleteStatementDate) continue;

						// ARStatement: delete all the records with statement date = last statement date(Selected_Statement_Cycle)
						// and On Demand = FALSE for the statement customers with statement cycle = Selected_Statement_Cycle
						// and last statement date <= statement date being deleted

						// ARBalances: set StatementRequired = 1 if the statement was deleted for the customer and branch;
						// set StatementRequired = 1 for parent customer and branch as well.

						foreach (ARStatement ars in pGraph.CustomerStatementForDelete.Select(curCustomer.BAccountID, deleteStatementDate))
						{
							allCustomers.Add(ars.CustomerID.Value);
							PXUpdate<
								Set<ARBalances.statementRequired, True>,
								ARBalances,
								Where<
									ARBalances.customerID, Equal<Required<ARBalances.customerID>>,
									And<ARBalances.branchID, Equal<Required<ARBalances.branchID>>>>>
							.Update(pGraph, ars.CustomerID, ars.BranchID);
							if (curCustomer.ConsolidateStatements == true && curCustomer.ParentBAccountID != null)
							{
								allCustomers.Add(curCustomer.ParentBAccountID.Value);
								PXUpdate<
									Set<ARBalances.statementRequired, True>,
									ARBalances,
									Where<
										ARBalances.customerID, Equal<Required<ARBalances.customerID>>,
										And<ARBalances.branchID, Equal<Required<ARBalances.branchID>>>>>
								.Update(pGraph, curCustomer.ParentBAccountID, ars.BranchID);
							}
							pGraph.CustomerStatementForDelete.Delete(ars);

						}

						pGraph.Actions.PressSave();

						// ARRegister: set Statement Date = NULL where statement date = last statement date(Selected_Statement_Cycle)
						// for the customers with((statement cycle = Selected_Statement_Cycle and parent account is null)
						// or(parent statement cycle = Selected_Statement_Cycle and Consolidate Statements = TRUE and parent account is not null))
						// and last statement date <= statement date being deleted
						PXUpdate<
							Set<ARRegister.statementDate, Null>,
							ARRegister,
							Where<
								ARRegister.statementDate, Equal<Required<ARRegister.statementDate>>,
								And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>
						.Update(pGraph, deleteStatementDate, curCustomer.BAccountID);

						// ARAdjust: set Statement Date = NULL where statement date = last statement date(Selected_Statement_Cycle)
						// for the customers with((statement cycle = Selected_Statement_Cycle and parent account is null)
						// or(parent statement cycle = Selected_Statement_Cycle and Consolidate Statements = TRUE and parent account is not null))
						// and last statement date <= statement date being deleted

						PXUpdate<
							Set<ARAdjust.statementDate, Null>,
							ARAdjust,
							Where<
								ARAdjust.statementDate, Equal<Required<ARAdjust.statementDate>>,
								And<ARAdjust.customerID, Equal<Required<ARAdjust.customerID>>>>>
						.Update(pGraph, deleteStatementDate, curCustomer.BAccountID);

						// ARTranPost:
						//
						// set Statement Date = NULL where statement date = last statement date (Selected_Statement_Cycle)
						// for the statement customers with (statement cycle = Selected_Statement_Cycle and statement type = BBF
						// and last statement date <= statement date being deleted);
						//
						// set Statement Date = max(statement date) from ARStatementDetail for the document
						// where statement date < last statement date(Selected_Statement_Cycle) and On Demand = FALSE
						// whose statement customers have(statement cycle = Selected_Statement_Cycle and
						// statement type = Open Item and last statement date <= statement date being deleted).

						if (curCustomer.StatementType == ARStatementType.BalanceBroughtForward)
							// for BBF, if ARTranPost.StatementDate = deleted_statement _date: set ARTranPost.StatementDate = NULL
							PXUpdate<
								Set<ARTranPost.statementDate, Null>,
								ARTranPost,
								Where<
									ARTranPost.statementDate, Equal<Required<ARTranPost.statementDate>>,
									And<ARTranPost.docDate, Greater<Required<ARTranPost.docDate>>,
									And<ARTranPost.customerID, Equal<Required<ARTranPost.customerID>>>>>>
							.Update(pGraph, deleteStatementDate, prevStatementDate ?? DateTime.MinValue, curCustomer.BAccountID);

						if (curCustomer.StatementType == ARStatementType.OpenItem)
						{
							ARStatementDetail[] maxDates =
								SelectFrom<ARStatementDetail>
								.InnerJoin<Customer>.On<ARStatementDetail.customerID.IsEqual<Customer.bAccountID>>
								.LeftJoin<ARStatement>.On<
									ARStatement.branchID.IsEqual<ARStatementDetail.branchID>
									.And<ARStatement.customerID.IsEqual<ARStatementDetail.customerID>
									.And<ARStatement.curyID.IsEqual<ARStatementDetail.curyID>
									.And<ARStatement.statementDate.IsEqual<ARStatementDetail.statementDate>>>>>
								.Where<
									Customer.statementType.IsEqual<@P.AsString>
									.And<ARStatement.onDemand.IsEqual<False>>
									.And<Customer.bAccountID.IsEqual<@P.AsInt>>>
								.AggregateTo<
									GroupBy<ARStatementDetail.refNbr>,
									GroupBy<ARStatementDetail.docType>,
									Max<ARStatement.statementDate>>
								.View.Select(
									pGraph,
									ARStatementType.OpenItem,
									curCustomer.BAccountID)
								.RowCast<ARStatementDetail>().ToArray();
							foreach (ARStatementDetail item in maxDates)
							{
								PXUpdate<
									Set<ARTranPost.statementDate, Required<ARTranPost.statementDate>>,
									ARTranPost,
									Where<
										ARTranPost.customerID, Equal<Required<ARTranPost.customerID>>,
										And<ARTranPost.type, Equal<Required<ARTranPost.type>>,
										And<ARTranPost.refNbr, Equal<Required<ARTranPost.refNbr>>,
										And<ARTranPost.docType, Equal<Required<ARTranPost.docType>>>>>>>
								.Update(
									pGraph,
									item.StatementDate,
									curCustomer.BAccountID,
									ARTranPost.type.Origin,
									item.RefNbr,
									item.DocType);
							}
							// if there is no arstatementdetail for refNbr+docType+customerID
							PXUpdate<
								Set<ARTranPost.statementDate, Null>,
								ARTranPost,
								Where<
									ARTranPost.customerID, Equal<Required<ARTranPost.customerID>>,
									And<ARTranPost.statementDate, Equal<Required<ARTranPost.statementDate>>>>>
							.Update(
								pGraph,
								curCustomer.BAccountID,
								deleteStatementDate);

						}
					}

					ts.Complete();
				}

			}

			// Customer: set last statement date to max(statement date) from ARStatement
			// where On Demand = FALSE for the customers with statement cycle = Selected_Statement_Cycle
			// and old last statement date <= statement date being deleted

			allCustomers = allCustomers.Distinct().ToList();

			Dictionary<int?, DateTime?> maxCustomerDates =
				SelectFrom<ARStatement>
				.Where<
					ARStatement.customerID.IsIn<@P.AsInt>
					.And<ARStatement.onDemand.IsEqual<False>>>
				.AggregateTo<
					GroupBy<ARStatement.customerID>,
					Max<ARStatement.statementDate>>
				.View
				.Select(pGraph, allCustomers)
				.RowCast<ARStatement>()
				.ToDictionary(_ => _.CustomerID, _ => _.StatementDate);

			foreach (int? item in allCustomers)
			{
				DateTime? lastStmtDate = null;
				maxCustomerDates.TryGetValue(item, out lastStmtDate);
				PXUpdate<
					Set<Customer.statementLastDate, Required<Customer.statementLastDate>>,
					Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
				.Update(pGraph, lastStmtDate, item);
			}

			cycle.LastStmtDate = prevStatementDate;
			CyclesList.Update(cycle);
			Actions.PressSave();

		}

		protected virtual IEnumerable<ICollection<Customer>> CollectCustomerFamiliesForCycleProcessing(
			ARStatementCycle cycle,
			DateTime statementDate)
		{
			ICollection<ICollection<Customer>> customerFamilies = new List<ICollection<Customer>>();

			PXSelectBase<Customer> selectCustomer = new PXSelectJoin<
				Customer,
					InnerJoin<CustomerMaster,
						On<CustomerMaster.bAccountID, Equal<Customer.statementCustomerID>>,
					LeftJoin<CustomerWithActiveBalance,
						On<CustomerWithActiveBalance.customerID, Equal<Customer.bAccountID>>>>,
				Where<
						CustomerMaster.statementCycleId, Equal<Required<Customer.statementCycleId>>,
							And<Where2<Where<Required<ARStatementCycle.printEmptyStatements>, Equal<True>
													, Or<CustomerWithActiveBalance.customerID, IsNotNull>>
										, Or<Exists<Select<ARStatement,
												Where<ARStatement.statementCycleId, Equal<CustomerMaster.statementCycleId>,
													And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
													And<ARStatement.customerID, Equal<Customer.bAccountID>,
													And<ARStatement.onDemand, Equal<True>>>>>>>>>>>,
				OrderBy<
					Asc<Customer.statementCustomerID>>>
				(this);

			List<Customer> currentFamily = new List<Customer>();

			foreach (Customer customer in selectCustomer.Select(cycle.StatementCycleId, cycle.PrintEmptyStatements, statementDate))
			{
				if (!currentFamily.Any() || currentFamily.First().StatementCustomerID == customer.StatementCustomerID)
				{
					currentFamily.Add(customer);
				}
				else
				{
					customerFamilies.Add(currentFamily);
					currentFamily = new List<Customer> { customer };
				}
			}

			if (currentFamily.Any())
			{
				customerFamilies.Add(currentFamily);
			}

			return customerFamilies;
		}

		protected virtual IEnumerable<ICollection<Customer>> CollectCustomerFamiliesForCycleDeleting(
			ARStatementCycle cycle, DateTime statementDate)
		{
			ICollection<ICollection<Customer>> customerFamilies = new List<ICollection<Customer>>();

			var allCustomers =
				SelectFrom<Customer>
					.LeftJoin<CustomerMaster>
						.On<Customer.parentBAccountID.IsEqual<CustomerMaster.bAccountID>>
				.Where<
					Brackets<
						Customer.consolidateStatements.IsEqual<False>>
						.And<Customer.statementCycleId.IsEqual<@P.AsString>>
					.Or<
					Brackets<
						CustomerMaster.statementCycleId.IsEqual<@P.AsString>
						.And<Customer.consolidateStatements.IsEqual<True>>>>>
				.View
				.Select(this, cycle.StatementCycleId, cycle.StatementCycleId)
				.RowCast<Customer>().ToArray();

			List<Customer> currentFamily = new List<Customer>();

			foreach (Customer customer in allCustomers)
			{
				if (!currentFamily.Any() || currentFamily.First().StatementCustomerID == customer.StatementCustomerID)
				{
					currentFamily.Add(customer);
				}
				else
				{
					customerFamilies.Add(currentFamily);
					currentFamily = new List<Customer> { customer };
				}
			}

			if (currentFamily.Any())
			{
				customerFamilies.Add(currentFamily);
			}

			return customerFamilies;
		}

		/// <summary>
		/// Checks if all customers belonging to the specified statement cyclehave their 
		/// statement date updated (i.e. they have been properly processed by the
		/// <see cref="GenerateCustomerStatement(StatementCreateBO, ARStatementCycle, Customer, DateTime, 
		/// IDictionary{ARStatementKey, ARStatement}, IDictionary{ARStatementKey, ARStatement}, bool)"/> 
		/// function). If so, updates the <see cref="ARStatementCycle.LastStmtDate">last statement date</see> 
		/// of the statement cycle.
		/// </summary>
		protected virtual void UpdateStatementCycleLastStatementDate(ARStatementCycle cycle, DateTime statementDate, bool isOnDemand = false)
		{
			if (isOnDemand) return;

			// The INNER JOIN by the parent customer is required
			// because the parent customer's statement cycle ID value
			// determines the statement cycle for the whole family,
			// regardless of the children's values of that field.
			// -
			bool allCustomersProcessed = PXSelectReadonly2<
				Customer,
					InnerJoin<CustomerMaster,
					On<CustomerMaster.bAccountID, Equal<Customer.statementCustomerID>>,
				LeftJoin<CustomerWithActiveBalance,
					On<CustomerWithActiveBalance.customerID, Equal<Customer.bAccountID>>>>,
				Where<
					CustomerMaster.statementCycleId, Equal<Required<Customer.statementCycleId>>,
					And2<
						Where<Customer.statementLastDate, IsNull,
							Or<Customer.statementLastDate, Less<Required<Customer.statementLastDate>>>>,
						And<
						Where<Required<ARStatementCycle.printEmptyStatements>, Equal<True>,
							Or<CustomerWithActiveBalance.customerID, IsNotNull>>>>>>
				.SelectWindowed(this, 0, 1, cycle.StatementCycleId, statementDate, cycle.PrintEmptyStatements)
				.IsEmpty();

			// The statement cycle's last statement date is updated
			// only when all customers from the cycle have been
			// properly processed.
			// -
			if (allCustomersProcessed)
			{
				cycle.LastStmtDate = statementDate;

				CyclesList.Update(cycle);
				Actions.PressSave();
			}
		}

		/// <summary>
		/// Deletes the existing statements for the family on the specified date,
		/// depending on the settings for the current statement generation.
		/// </summary>
		/// <returns>
		/// Trace information about the deleted statements, which is required 
		/// to pass print / email counts to regenerated statements.
		/// </returns>
		protected virtual IDictionary<ARStatementKey, ARStatement> DeleteExistingStatements(
			IEnumerable<Customer> customerFamily,
			DateTime statementDate,
			bool clearExisting,
			bool isOnDemand)
		{
			IEnumerable<ARStatement> deletedCustomerStatements = Enumerable.Empty<ARStatement>();
			HashSet<int> deletedParentCustomerIDs = new HashSet<int>();

			foreach (Customer customer in customerFamily)
			{
				if (clearExisting || customer.StatementLastDate == statementDate)
				{
					if (isOnDemand)
					{
						EnsureNoRegularStatementExists(customer.BAccountID, statementDate);
					}

					deletedCustomerStatements = deletedCustomerStatements
						.Concat(DeleteCustomerStatement(customer, statementDate, isOnDemand));

					if (customer.BAccountID != customer.StatementCustomerID
						&& customer.StatementCustomerID != null
						&& deletedParentCustomerIDs.Contains((int)customer.StatementCustomerID) == false)
					{
						deletedCustomerStatements = deletedCustomerStatements
							.Concat(DeleteCustomerStatement(new Customer { BAccountID = customer.StatementCustomerID }, statementDate, isOnDemand));

						deletedParentCustomerIDs.Add((int)customer.StatementCustomerID);
					}

				}
			}

			return deletedCustomerStatements.ToDictionary(statement => new ARStatementKey(statement));
		}

		protected virtual void ForceBeginningBalanceToPreviousStatementEndBalance(
			IDictionary<ARStatementKey, ARStatement> familyStatements,
			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> familyStatementDetails)
		{
			foreach (KeyValuePair<ARStatementKey, ARStatement> pair in familyStatements)
			{
				ARStatementKey statementKey = pair.Key;
				ARStatement statement = pair.Value;

				ARStatement previousStatement = GetPreviousStatement(statement.BranchID, statement.CustomerID, statement.CuryID);

				if (previousStatement != null)
				{
					statement.BegBalance = previousStatement.EndBalance;
					statement.CuryBegBalance = previousStatement.CuryEndBalance;
				}

				ApplyFIFORule(statement, ARSetup.Current.AgeCredits == true);

				// Adding detail record with aged balances
				if (!familyStatementDetails.TryGetValue(statementKey, out ICollection<ARStatementDetail> statementDetails)
					|| statementDetails == null)
				{
					statementDetails = new List<ARStatementDetail>();
					familyStatementDetails[statementKey] = statementDetails;
				}

				statementDetails.Add(new ARStatementDetail
				{
					BranchID = statementKey.BranchID,
					CustomerID = statementKey.CustomerID,
					CuryID = statementKey.CurrencyID,
					StatementDate = statementKey.StatementDate,
					DocType = string.Empty,
					RefNbr = string.Empty,
					RefNoteID = Guid.NewGuid(),

					TranPostID = 0,
					DocBalance = 0,
					CuryDocBalance = 0,

					StatementType = statement.StatementType,
					BegBalance = statement.BegBalance,
					CuryBegBalance = statement.CuryBegBalance,

					AgeBalance00 = statement.AgeBalance00,
					AgeBalance01 = statement.AgeBalance01,
					AgeBalance02 = statement.AgeBalance02,
					AgeBalance03 = statement.AgeBalance03,
					AgeBalance04 = statement.AgeBalance04,

					CuryAgeBalance00 = statement.CuryAgeBalance00,
					CuryAgeBalance01 = statement.CuryAgeBalance01,
					CuryAgeBalance02 = statement.CuryAgeBalance02,
					CuryAgeBalance03 = statement.CuryAgeBalance03,
					CuryAgeBalance04 = statement.CuryAgeBalance04,
				});
			}
		}

		/// <summary>
		/// If <see cref="ARStatementCycle.PrintEmptyStatements"/> is set to
		/// <c>false</c>, marks all empty open item statements and empty 
		/// zero-balance BBF statements as "do not print" and "do not email".
		/// </summary>
		protected virtual void MarkEmptyStatementsForPrintingAndEmailing(
			ARStatementCycle statementCycle,
			IEnumerable<ARStatement> statements,
			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> statementDetails)
		{
			if (statementCycle.PrintEmptyStatements == true) return;

			foreach (ARStatement statement in statements)
			{
				ARStatementKey statementKey = new ARStatementKey(statement);

				if (ARStatementProcess.IsEmptyStatement(statement, statementDetails.GetValueOrEmpty(statementKey)))
				{
					statement.DontEmail = true;
					statement.DontPrint = true;
				}
			}
		}

		/// <param name="isOnDemand">
		/// If set to <c>true</c>, indicates that the statements to be persisted
		/// are on-demand statements, so that <see cref="Customer.statementLastDate"/>, 
		/// <see cref="ARRegister.statementDate"/>, and <see cref="ARAdjust.statementDate"/>
		/// will not be updated.
		/// </param>
		protected static bool PersistStatement(
			StatementCreateBO statementPersistGraph,
			IEnumerable<ARStatement> statements,
			IEnumerable<ARStatementDetail> statementDetails)
		{
			statementPersistGraph.Clear();

			foreach (ARStatement statement in statements)
			{
				statementPersistGraph.Statement.Insert(statement);
			}

			foreach (ARStatementDetail statementDetail in statementDetails)
			{
				statementPersistGraph.StatementDetail.Insert(statementDetail);
			}

			statementPersistGraph.Actions.PressSave();

			return true;
		}

		protected static void UpdateCustomersLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime statementDate,
			int customerID) =>
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<
						Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>,
						And<Where<
							Override.Customer.statementLastDate, IsNull,
							Or<Override.Customer.statementLastDate, Less<Required<Override.Customer.statementLastDate>>>>>>>
				.Update(
					statementPersistGraph,
					statementDate,
					customerID,
					statementDate);

		protected static void UpdateARBalanceStatementNotRequired(
			PXGraph statementPersistGraph,
			ARStatement statement,
			DateTime statementDate) =>
			PXUpdate<
				Set<ARBalances.statementRequired, False>,
				ARBalances,
				Where<ARBalances.customerID, Equal<Required<ARBalances.customerID>>,
					And<ARBalances.branchID, Equal<Required<ARBalances.branchID>>,
					And<ARBalances.lastDocDate, LessEqual<Required<ARBalances.lastDocDate>>>>>>
			.Update(
				statementPersistGraph,
				statement.CustomerID.Value,
				statement.BranchID.Value,
				statementDate);


		/// <summary>
		/// Used for open item statement processing. Updates all documents of the customers that do not
		/// yet have an <see cref="ARRegister.StatementDate"/>, regardless of whether the document
		/// has got into statement. This is done so that if the customer switches to BBF statement
		/// type, the documents that didn't get into BBF statements don't suddenly appear in the
		/// new statements.
		/// </summary>
		protected static void UpdateDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID) =>
			PXUpdate<
				Set<ARRegister.statementDate, Required<ARRegister.statementDate>>,
				ARRegister,
				Where<
					ARRegister.statementDate, IsNull,
					And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

		/// <summary>
		/// Used for balance brought forward statements. Updates <see cref="ARRegister.StatementDate"/>
		/// for documents corresponding to <see cref="ARStatementDetail"/> records of a given statement,
		/// so that these documents are not included into future BBF statements.
		/// </summary>
		protected static void UpdateDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			ARStatement statement) =>
				PXUpdateJoin<
					Set<ARRegister.statementDate, Required<ARRegister.statementDate>>,
					ARRegister,
						InnerJoin<ARStatementDetail,
							On<ARRegister.docType, Equal<ARStatementDetail.docType>,
							And<ARRegister.refNbr, Equal<ARStatementDetail.refNbr>>>>,
					Where<
						ARRegister.statementDate, IsNull,
						And<ARStatementDetail.branchID, Equal<Required<ARStatementDetail.branchID>>,
						And<ARStatementDetail.curyID, Equal<Required<ARStatementDetail.curyID>>,
						And<ARStatementDetail.customerID, Equal<Required<ARStatementDetail.customerID>>,
						And<ARStatementDetail.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>>
				.Update(
					statementPersistGraph,
					statement.StatementDate,
					statement.BranchID,
					statement.CuryID,
					statement.CustomerID,
					statement.StatementDate);

		/// <summary>
		/// Used for statement re-generation, when the old statement objects are deleted.
		/// In order for the documents to re-appear in the new statement, their statement
		/// date needs to be reset to <c>null</c>.
		/// </summary>
		/// <remarks>
		/// It is insufficient to just update documents for which matching statement details 
		/// exist, because in case of switching from Open Item to BBF, some documents could have
		/// been closed at the moment of Open Item processing, and no <see cref="ARStatementDetail"/> 
		/// was created for them.
		/// </remarks>
		protected static void ResetDocumentsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID)
		{
			PXUpdate<
				Set<ARRegister.statementDate, Null>,
				ARRegister,
				Where<
					ARRegister.statementDate, Equal<Required<ARRegister.statementDate>>,
					And<ARRegister.docDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARRegister.customerID, Equal<Required<ARRegister.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

			PXUpdate<
				Set<ARTranPost.statementDate, Null>,
				ARTranPost,
				Where<
					ARTranPost.statementDate, Equal<Required<ARTranPost.statementDate>>,
					And<ARTranPost.docDate, LessEqual<Required<ARTranPost.docDate>>,
					And<ARTranPost.customerID, Equal<Required<ARTranPost.customerID>>>>>>
					.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);
		}

		/// <summary>
		/// Used for open item statements. Updates <see cref="ARAdjust.StatementDate"/> for all relevant
		/// applications of the customer so that they don't suddenly show up in customer statement when
		/// the user switches the customer to BBF.
		/// </summary>
		protected static void UpdateApplicationsLastStatementDate(
			PXGraph statementPersistGraph,
			DateTime? statementDate,
			int? customerID) =>
			PXUpdate<
				Set<ARAdjust.statementDate, Required<ARAdjust.statementDate>>,
				ARAdjust,
				Where<
					ARAdjust.statementDate, IsNull,
					And<ARAdjust.adjgDocDate, LessEqual<Required<ARRegister.docDate>>,
					And<ARAdjust.customerID, Equal<Required<ARAdjust.customerID>>>>>>
			.Update(
				statementPersistGraph,
				statementDate,
				statementDate,
				customerID);

		/// <summary>
		/// Used for balance brought forward statements. Updates <see cref="ARAdjust.StatementDate"/>
		/// for applications corresponding to <see cref="ARStatementDetail"/> records of a given statement,
		/// so that these applications are not included into future statements.
		/// </summary>
		protected static void UpdateApplicationsLastStatementDate(
			PXGraph statementPersistGraph,
			ARStatement statement) =>
				PXUpdateJoin<
					Set<ARAdjust.statementDate, Required<ARAdjust.statementDate>>,
					ARAdjust,
						InnerJoin<ARStatementDetail,
							On<ARAdjust.noteID, Equal<ARStatementDetail.refNoteID>>>,
					Where<
						ARAdjust.statementDate, IsNull,
						And<ARStatementDetail.branchID, Equal<Required<ARStatementDetail.branchID>>,
						And<ARStatementDetail.curyID, Equal<Required<ARStatementDetail.curyID>>,
						And<ARStatementDetail.customerID, Equal<Required<ARStatementDetail.customerID>>,
						And<ARStatementDetail.statementDate, Equal<Required<ARStatementDetail.statementDate>>>>>>>>
				.Update(
					statementPersistGraph,
					statement.StatementDate,
					statement.BranchID,
					statement.CuryID,
					statement.CustomerID,
					statement.StatementDate);

		#endregion

		#region Utility Functions
		protected static void Recalculate(ARStatement aDest)
		{
			if (aDest.StatementType == ARStatementType.BalanceBroughtForward)
			{
				aDest.CuryEndBalance = aDest.CuryAgeBalance00 + aDest.CuryAgeBalance01 + aDest.CuryAgeBalance02 + aDest.CuryAgeBalance03 + aDest.CuryAgeBalance04;
				aDest.EndBalance = aDest.AgeBalance00 + aDest.AgeBalance01 + aDest.AgeBalance02 + aDest.AgeBalance03 + aDest.AgeBalance04;
			}
		}

		protected static void ApplyFIFORule(ARStatement aDest, bool aAgeCredits)
		{
			//Apply Extra payment in the correct sequence - first to oldest, then - to closer debts
			//We assume, that allpayments are already applyed to oldest -this function propagates them up.
			if (!aAgeCredits)
			{
				if (aDest.AgeBalance04 < 0)//|| (aDest.AgeDays03 == null)) //Extra payments
				{
					aDest.AgeBalance03 += aDest.AgeBalance04;
					aDest.AgeBalance04 = Decimal.Zero;
					aDest.CuryAgeBalance03 += aDest.CuryAgeBalance04;
					aDest.CuryAgeBalance04 = Decimal.Zero;

				}
				if (aDest.AgeBalance03 < 0)//|| (aDest.AgeDays02 == null))
				{
					aDest.AgeBalance02 += aDest.AgeBalance03;
					aDest.AgeBalance03 = Decimal.Zero;
					aDest.CuryAgeBalance02 += aDest.CuryAgeBalance03;
					aDest.CuryAgeBalance03 = Decimal.Zero;
				}
				if (aDest.AgeBalance02 < 0)//|| (aDest.AgeDays01 == null))
				{
					aDest.AgeBalance01 += aDest.AgeBalance02;
					aDest.AgeBalance02 = Decimal.Zero;
					aDest.CuryAgeBalance01 += aDest.CuryAgeBalance02;
					aDest.CuryAgeBalance02 = Decimal.Zero;
				}
				if (aDest.AgeBalance01 < 0)
				{
					aDest.AgeBalance00 += aDest.AgeBalance01;
					aDest.AgeBalance01 = Decimal.Zero;
					aDest.CuryAgeBalance00 += aDest.CuryAgeBalance01;
					aDest.CuryAgeBalance01 = Decimal.Zero;
				}
			}
		}
		#endregion

		#region Statements Generation

		protected virtual void GenerateStatementForCustomerFamily(
			StatementCreateBO persistGraph,
			ARStatementCycle statementCycle,
			IEnumerable<Customer> customerFamily,
			DateTime statementDate,
			bool clearExisting,
			bool isOnDemand)
		{
			IDictionary<ARStatementKey, ARStatement> familyStatements =
				new Dictionary<ARStatementKey, ARStatement>();

			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> familyStatementDetails =
				new Dictionary<ARStatementKey, ICollection<ARStatementDetail>>();

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				IDictionary<ARStatementKey, ARStatement> deletedFamilyStatementsTrace = DeleteExistingStatements(
					customerFamily,
					statementDate,
					clearExisting,
					isOnDemand);

				foreach (Customer customer in customerFamily)
				{
					GenerateCustomerStatement(
						statementCycle,
						customer,
						statementDate,
						familyStatements,
						familyStatementDetails,
						deletedFamilyStatementsTrace,
						isOnDemand);
				}

				ForceBeginningBalanceToPreviousStatementEndBalance(familyStatements, familyStatementDetails);

				MarkEmptyStatementsForPrintingAndEmailing(
					statementCycle,
					familyStatements.Values,
					familyStatementDetails);

				PersistStatement(
					persistGraph,
					familyStatements.Values,
					familyStatementDetails.Values.SelectMany(sequence => sequence));

				if (!isOnDemand)
				{
					foreach (Customer customer in customerFamily)
					{
						UpdateCustomersLastStatementDate(
							persistGraph,
							statementDate,
							customer.BAccountID.Value);

						if (GetStatementType(customer) == ARStatementType.OpenItem)
						{
							UpdateDocumentsLastStatementDate(persistGraph, statementDate, customer.BAccountID);
							UpdateApplicationsLastStatementDate(persistGraph, statementDate, customer.BAccountID);
						}
					}

					foreach (ARStatement statement in familyStatements.Values)
					{
						UpdateDocumentsLastStatementDate(persistGraph, statement);
						UpdateApplicationsLastStatementDate(persistGraph, statement);

						ARStatementKey statementKey = new ARStatementKey(statement);

						IEnumerable<ARStatementDetail> familyDetailsForAnyCuryID = familyStatementDetails.Where
							(detail => detail.Key.BranchID == statement.BranchID
							&& detail.Key.CustomerID == statement.CustomerID
							&& detail.Key.StatementDate == statement.StatementDate)
							.SelectMany(kvp => kvp.Value);

						if (ARStatementProcess.IsEmptyStatement(
							statement,
							familyDetailsForAnyCuryID))
						{
							UpdateARBalanceStatementNotRequired(persistGraph, statement, statementDate);
						}
					}
				}

				ts.Complete();
			}
		}

		private DateTime? GetPreviousStatementDate(ARStatementCycle cycle)
		{
			if (cycle.LastStmtDate == null) return null;

			ARStatement q = SelectFrom<ARStatement>
			.Where<
				ARStatement.statementCycleId.IsEqual<@P.AsString>
				.And<ARStatement.statementDate.IsLess<@P.AsDateTime.UTC>
				.And<ARStatement.onDemand.IsEqual<False>>>>
			.OrderBy<ARStatement.statementDate.Desc>
			.View
			.Select(this, cycle.StatementCycleId, cycle.LastStmtDate);
			return q?.StatementDate;
		}

		private string GetStatementType(Customer customer)
			=> customer.StatementChild == true
			? PXSelect<
					Customer,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(this, customer.StatementCustomerID)
					.RowCast<Customer>()
					.FirstOrDefault()
					?.StatementType
			: customer.StatementType;

		/// <summary>
		/// Returns the last statement generated for the specified customer,
		/// branch, and currency. Excludes on-demand statements.
		/// In case no last statement is present, returns <c>null</c>.
		/// </summary>
		private ARStatement GetPreviousStatement(int? branchID, int? customerID, string currencyID) =>
			PXSelectJoin<ARStatement,
				InnerJoin<Branch,
					On<Branch.branchID, Equal<ARStatement.branchID>, And<Branch.active, Equal<True>>>>,
				Where<
					ARStatement.branchID, Equal<Required<ARStatement.branchID>>,
					And<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.curyID, Equal<Required<ARStatement.curyID>>,
					And<ARStatement.onDemand, Equal<False>>>>>,
				OrderBy<
					Desc<ARStatement.statementDate>>>
			.SelectWindowed(this, 0, 1, branchID, customerID, currencyID);

		protected virtual void GenerateCustomerStatement(
			ARStatementCycle statementCycle,
			Customer customer,
			DateTime statementDate,
			IDictionary<ARStatementKey, ARStatement> familyStatements,
			IDictionary<ARStatementKey, ICollection<ARStatementDetail>> familyStatementDetails,
			IDictionary<ARStatementKey, ARStatement> deletedFamilyStatementsTrace,
			bool isOnDemand)
		{
			ICollection<ARRegister> customerDocumentsToAge = new List<ARRegister>();

			foreach (ARTranPostStatement arTranPostData in GetDataForStatement(customer, statementDate))
			{
				if (arTranPostData.ARRegisterHasBalance())
				{
					customerDocumentsToAge.Add(arTranPostData.ARRegister);
				}

				ARStatement statement = GetOrAddStatementForDocument(
					familyStatements,
					deletedFamilyStatementsTrace,
					arTranPostData,
					customer,
					statementCycle,
					statementDate,
					isOnDemand);

				if (arTranPostData.ShouldBeConvertedToStatementDetail())
				{
					arTranPostData.AdjustStatementEndBalance(statement);

					ICollection<ARStatementDetail> statementDetails = familyStatementDetails.GetOrAdd(
						new ARStatementKey(statement),
						() => new List<ARStatementDetail>());

					statementDetails.Add(CombineStatementDetailCustomizable(statement, arTranPostData));
					PXUpdate<Set<ARTranPost.statementDate, Required<ARTranPost.statementDate>>, ARTranPost, Where<ARTranPost.iD, Equal<Required<ARTranPost.iD>>>>
						.Update(this, statement.StatementDate, arTranPostData.ARTranPost.ID);
				}
			}

			AccumulateAgeBalancesIntoStatements(
				statementCycle,
				statementDate,
				customerDocumentsToAge,
				familyStatements,
				ARSetup.Current.AgeCredits == true);

			PXSelectBase<ARStatement> selectAllPreviousByCustomer =
				new PXSelectJoin<ARStatement,
				InnerJoin<Branch,
					On<Branch.branchID, Equal<ARStatement.branchID>, And<Branch.active, Equal<True>>>,
				InnerJoin<ARBalances,
					On<ARBalances.branchID, Equal<ARStatement.branchID>, And<ARBalances.customerID, Equal<ARStatement.customerID>>>>>,
				Where<ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.onDemand, Equal<False>,
					And<ARBalances.statementRequired, Equal<True>>>>,
				OrderBy<
					Asc<ARStatement.curyID,
					Desc<ARStatement.statementDate>>>>
				(this);

			// Merge with previous statements - is needed for Balance Brought Forward.
			// -
			IDictionary<ARStatementKey, DateTime> lastStatementDates = new Dictionary<ARStatementKey, DateTime>();

			foreach (ARStatement statement in selectAllPreviousByCustomer.Select(customer.BAccountID))
			{
				ARStatementKey statementKey = new ARStatementKey(
					statement.BranchID.Value,
					statement.CuryID,
					customer.BAccountID.Value,
					statementDate);

				if (lastStatementDates.ContainsKey(statementKey)
					&& lastStatementDates[statementKey] > statement.StatementDate)
				{
					continue;
				}

				ARStatement header = GetOrAddStatement(
					familyStatements,
					statementKey,
					statementCycle,
					customer,
					isOnDemand);

				header.BegBalance = statement.EndBalance;
				header.CuryBegBalance = statement.CuryEndBalance;

				Recalculate(header);

				lastStatementDates[statementKey] = statement.StatementDate.Value;
			}

			if (isOnDemand && familyStatements.Values.Count == 0)
			{
				ARStatementKey statementKey = new ARStatementKey(
					Accessinfo.BranchID.Value,
					customer.CuryID ??
						PXSelect<Branch, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
							.Select(this, Accessinfo.BranchID.Value)
							.RowCast<Branch>().FirstOrDefault<Branch>().BaseCuryID,
					customer.BAccountID.Value,
					statementDate);

				ARStatement header = GetOrAddStatement(
					familyStatements,
					statementKey,
					statementCycle,
					customer,
					isOnDemand);
			}
		}

		private ARStatement GetOrAddStatementForDocument(
			IDictionary<ARStatementKey, ARStatement> familyStatements,
			IDictionary<ARStatementKey, ARStatement> deletedStatementsTrace,
			ARTranPostStatement document,
			Customer customer,
			ARStatementCycle statementCycle,
			DateTime statementDate,
			bool isOnDemand)
		{
			if (document.ARTranPost.Type == ARTranPost.type.RGOL) return null;

			ARStatementKey statementKey = document.GetARStatementKey(customer, statementDate);

			ARStatement statement = GetOrAddStatement(
				familyStatements,
				statementKey,
				statementCycle,
				customer,
				isOnDemand);

			SetPreviousStatementInfo(statement, deletedStatementsTrace);

			// Ensure the existence of statement object for the parent customer
			// in case the parent customer does not have his own relevant 
			// documents to be processed.
			// -
			if (customer.BAccountID != customer.StatementCustomerID)
			{
				ARStatementKey parentStatementKey = statementKey.CopyForAnotherCustomer(customer.StatementCustomerID.Value);

				ARStatement statementForParent = GetOrAddStatement(
					familyStatements,
					parentStatementKey,
					statementCycle,
					customer,
					isOnDemand);

				SetPreviousStatementInfo(statementForParent, deletedStatementsTrace);
			}

			return statement;
		}

		private static ARTranPostStatement[] ParseDBStatementsData(string statementType, PXResultset<ARTranPostGL> response)
		{
			switch (statementType)
			{
				case ARStatementType.OpenItem:
					{
						return response.Select(_ => new ARTranPostOpenItem(_)).ToArray();
					}
				case ARStatementType.BalanceBroughtForward:
					{
						return response.Select(_ => new ARTranPostBBF(_)).ToArray();

					}
				default: throw new PXInvalidOperationException(Messages.UnknownStatementType);
			}
		}

		private IEnumerable<ARTranPostStatement> GetDataForStatement(Customer customer, DateTime statementDate)
		{
			PXSelectBase<ARTranPostGL> artranPostView = new PXSelectJoin<ARTranPostGL,
				InnerJoin<Branch, On<Branch.branchID, Equal<ARTranPostGL.branchID>, And<Branch.active, Equal<True>>>,
				LeftJoin<ARRegister, On<ARTranPostGL.docType, Equal<ARRegister.docType>, And<ARTranPostGL.refNbr, Equal<ARRegister.refNbr>>>,
				LeftJoin<ARRegister2, On<ARTranPostGL.sourceDocType, Equal<ARRegister2.docType>, And<ARTranPostGL.sourceRefNbr, Equal<ARRegister2.refNbr>>>
				>>>,
				Where<
					ARRegister.customerID, Equal<Required<ARTranPostGL.customerID>>,
					And<ARTranPostGL.docDate, LessEqual<Required<ARTranPostGL.docDate>>>>
					>(this);

			if (customer.StatementType == ARStatementType.OpenItem)
			{
				artranPostView.WhereAnd<Where<
					// For Open Item statements, we exclude documents that are
					// definitely closed by the statement calculation date.
					// -
					ARRegister.closedTranPeriodID, IsNull,
					Or<ARRegister.closedDate, Greater<Required<ARRegister.closedDate>>>>>();
			}
			else
			{
				artranPostView.WhereAnd<Where<
					// For BBF statements, we should fetch even closed documents
					// if they are not yet reported.
					// -
					ARRegister.closedTranPeriodID, IsNull,
					Or<ARRegister.closedDate, GreaterEqual<Required<ARRegister.closedDate>>,
					Or<ARRegister.statementDate, IsNull,
					Or<ARTranPostGL.type.IsNotEqual<ARTranPost.type.origin>.And<ARTranPostGL.statementDate.IsNull>>>>>>();
			}

			object[] queryParameters = new object[]
			{
				customer.BAccountID,
				statementDate,
				customer.StatementType == ARStatementType.OpenItem
				? statementDate
				: FinPeriodRepository.GetFinPeriodByDate(statementDate, FinPeriod.organizationID.MasterValue).StartDate
			};

			ARTranPostStatement[] result = ParseDBStatementsData(customer.StatementType, artranPostView.Select(queryParameters));

			//TODO: replace this kludge with something. Required to "adjust" balance for current date. Probably whole documents should be assmbled...

			Dictionary<DocumentKey, ARTranPostStatement[]> postGroups = result.GroupBy(_ => new DocumentKey(_)).ToDictionary(_ => _.Key, _ => _.ToArray());
			foreach (ARTranPostStatement[] postsGroup in postGroups.Values)
			{
				decimal CuryBalance = postsGroup.Sum(_ => _.ARTranPost.CuryBalanceAmt.Value);
				decimal Balance = postsGroup.Sum(_ => _.ARTranPost.BalanceAmt.Value);

				foreach (ARTranPostStatement postLinked in postsGroup.Where(_ => _.ARRegister.RefNbr != null))
				{
					postLinked.ARRegister.CuryDocBal = postLinked.ARRegister.SignBalance * CuryBalance;
					postLinked.ARRegister.DocBal = postLinked.ARRegister.SignBalance * Balance;
				}
			}

			return result;
		}

		private void SetPreviousStatementInfo(ARStatement statement, IDictionary<ARStatementKey, ARStatement> statementsTrace)
		{
			if (statement.Processed == true) return;

			ARStatement previousStatement = GetPreviousStatement(
				statement.BranchID,
				statement.CustomerID,
				statement.CuryID);

			if (previousStatement != null)
			{
				statement.PrevStatementDate = previousStatement.StatementDate;
			}

			if (statementsTrace.TryGetValue(new ARStatementKey(statement), out ARStatement deletedStatementTrace))
			{
				statement.PrevPrintedCnt = deletedStatementTrace.PrevPrintedCnt;
				statement.PrevEmailedCnt = deletedStatementTrace.PrevEmailedCnt;
			}

			statement.Processed = true;
		}

		protected virtual ARStatement GetOrAddStatement(
			IDictionary<ARStatementKey, ARStatement> statementsDictionary,
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer customer,
			bool isOnDemand)
			=> statementsDictionary.GetOrAdd(statementKey, () =>
			{
				ARStatement statement = CombineStatementCustomizable(
					statementKey,
					statementCycle,
					customer,
					isOnDemand);

				using (new PXLocaleScope(statement.LocaleName))
				{
					FillBucketDescriptions(statement, statementCycle);
				}

				return statement;
			});

		#region Statement Entities Creation Utility Functions

		/// <param name="familyCustomer">
		/// Any customer from the family. The method operates under
		/// assumption that all customers in the family share their
		/// statement parameters, such as statement type, printing
		/// flags etc.
		/// </param>
		public static ARStatement CombineStatement(
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer familyCustomer,
			bool isOnDemand)
		{
			ARStatement result = new ARStatement();

			SetStatementAgeDaysToZero(result);
			SetStatementAgeBalancesToZero(result);

			result.BranchID = statementKey.BranchID;
			result.CuryID = statementKey.CurrencyID;
			result.CustomerID = statementKey.CustomerID;
			result.StatementDate = statementKey.StatementDate;
			result.StatementCycleId = statementCycle.StatementCycleId;
			result.StatementCustomerID = familyCustomer.StatementCustomerID;
			result.StatementType = familyCustomer.StatementType;
			result.DontPrint = familyCustomer.PrintStatements != true;
			result.DontEmail = familyCustomer.SendStatementByEmail != true;
			result.OnDemand = isOnDemand;
			result.AgeDays00 = 0;
			result.AgeDays01 = statementCycle.AgeDays00;
			result.AgeDays02 = statementCycle.AgeDays01;
			result.AgeDays03 = statementCycle.AgeDays02;
			result.LocaleName = familyCustomer.LocaleName ?? (System.Globalization.CultureInfo.CurrentCulture.Name);

			return result;
		}

		protected static void SetStatementAgeDaysToZero(ARStatement statement)
		{
			statement.AgeDays00 =
			statement.AgeDays01 =
			statement.AgeDays02 =
			statement.AgeDays03 = 0;
		}

		protected static void SetStatementAgeBalancesToZero(ARStatement statement)
		{
			statement.AgeBalance00 =
			statement.AgeBalance01 =
			statement.AgeBalance02 =
			statement.AgeBalance03 =
			statement.AgeBalance04 =
			statement.CuryAgeBalance00 =
			statement.CuryAgeBalance01 =
			statement.CuryAgeBalance02 =
			statement.CuryAgeBalance03 =
			statement.CuryAgeBalance04 =
			statement.BegBalance =
			statement.EndBalance =
			statement.CuryBegBalance =
			statement.CuryEndBalance = decimal.Zero;
		}

		/// <summary>
		/// Creates a new <see cref="ARStatementDetail"/> record using the information
		/// from the given customer statement and document records.
		/// </summary>
		/// <param name="statement">The statement to which the created detail belongs.</param>
		/// <param name="document">The document to which the created detail corresponds.</param>
		protected static ARStatementDetail CombineStatementDetail(ARStatement statement, ARTranPostStatement document)
			=> new ARStatementDetail
			{
				DocType = document.ARTranPost.DocType,
				RefNbr = document.ARTranPost.RefNbr,
				BranchID = document.ARTranPost.BranchID,
				DocBalance = document.ARRegister.DocBal ?? document.ARTranPost.BalanceAmt,
				CuryDocBalance = document.ARRegister.CuryDocBal ?? document.ARTranPost.CuryBalanceAmt,
				IsOpen = document.ARRegister?.OpenDoc,
				CustomerID = statement.CustomerID,
				CuryID = statement.CuryID,
				StatementDate = statement.StatementDate,
				RefNoteID = document.ARTranPost.RefNoteID,
				TranPostID = document.ARTranPost.ID
			};

		protected virtual ARStatement CombineStatementCustomizable(
			ARStatementKey statementKey,
			ARStatementCycle statementCycle,
			Customer familyCustomer,
			bool isOnDemand)
			=> CombineStatement(statementKey, statementCycle, familyCustomer, isOnDemand);

		protected virtual ARStatementDetail CombineStatementDetailCustomizable(
			ARStatement statement,
			ARTranPostStatement document)
			=> CombineStatementDetail(statement, document);

		#endregion

		protected virtual void FillBucketDescriptions(
			ARStatement statement,
			ARStatementCycle statementCycle)
		{
			DateTime statementDate = statement.StatementDate.Value;

			if (statementCycle.UseFinPeriodForAging == true)
			{
				IList<string> bucketDescriptions = AgingEngine
					.GetPeriodAgingBucketDescriptions(
						FinPeriodRepository,
						statementDate,
						AgingDirection.Backwards,
						5)
					.ToArray();

				statement.AgeBucketCurrentDescription = bucketDescriptions[0];
				statement.AgeBucket01Description = bucketDescriptions[1];
				statement.AgeBucket02Description = bucketDescriptions[2];
				statement.AgeBucket03Description = bucketDescriptions[3];
				statement.AgeBucket04Description = bucketDescriptions[4];
			}
			else
			{
				IList<string> bucketDescriptions = AgingEngine
					.GetDayAgingBucketDescriptions(
						AgingDirection.Backwards,
						new int[]
						{
							statement.AgeDays00 ?? 0,
							statement.AgeDays01 ?? 0,
							statement.AgeDays02 ?? 0,
							statement.AgeDays03 ?? 0,
						},
						false)
					.ToArray();

				PXCache statementCycleCache = this.Caches[typeof(ARStatementCycle)];

				// Take custom bucket descriptions if they are specified in the
				// statement cycle. Otherwise, use the calculated descriptions.
				// -
				statement.AgeBucketCurrentDescription =
					GetBucketDescription<ARStatementCycle.ageMsgCurrent>(statementCycleCache, statementCycle) ?? bucketDescriptions[0];

				statement.AgeBucket01Description =
					GetBucketDescription<ARStatementCycle.ageMsg00>(statementCycleCache, statementCycle) ?? bucketDescriptions[1];

				statement.AgeBucket02Description =
					GetBucketDescription<ARStatementCycle.ageMsg01>(statementCycleCache, statementCycle) ?? bucketDescriptions[2];

				statement.AgeBucket03Description =
					GetBucketDescription<ARStatementCycle.ageMsg02>(statementCycleCache, statementCycle) ?? bucketDescriptions[3];

				statement.AgeBucket04Description =
					GetBucketDescription<ARStatementCycle.ageMsg03>(statementCycleCache, statementCycle) ?? bucketDescriptions[4];
			}
		}

		private string GetBucketDescription<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			object value = cache.GetValueExt<Field>(data);
			return (value is PXFieldState ? ((PXFieldState)value).Value : value) as string;
		}

		/// <summary>
		/// Fills the age balances of relevant statements from the provided statement
		/// dictionary based on the information of invoices and payments open on the
		/// statement date.
		/// </summary>
		protected virtual void AccumulateAgeBalancesIntoStatements(
			ARStatementCycle statementCycle,
			DateTime statementDate,
			IEnumerable<ARRegister> openDocuments,
			IDictionary<ARStatementKey, ARStatement> statements,
			bool ageCredits)
		{
			foreach (ARRegister document in openDocuments)
			{
				ARStatementKey statementKey = new ARStatementKey(
					document.BranchID.Value,
					document.CuryID,
					document.CustomerID.Value,
					statementDate);

				if (statements.ContainsKey(statementKey)
					&& document.DocType != ARDocType.CashSale
					&& document.DocType != ARDocType.CashReturn)
				{
					AccumulateAgeBalances(
						this,
						statementCycle,
						statements[statementKey],
						document,
						ageCredits);
				}
			}
		}

		protected static void AccumulateAgeBalances(
			PXGraph graph,
			ARStatementCycle statementCycle,
			ARStatement statement,
			ARRegister document,
			bool ageCredits)
		{
			// Small Credit WO is a type of invoice but it must be processed as a payment.
			// -
			if (document.Payable == true && document?.DocType != ARDocType.SmallCreditWO
				|| document.DocType == ARDocType.Refund
				|| document.DocType == ARDocType.VoidRefund
				|| ageCredits)
			{
				DateTime statementDate = statement.StatementDate.Value;

				DateTime dateForAging =
					statementCycle.AgeBasedOn == AgeBasedOnType.DueDate && document.Payable == true
						? document.DueDate.Value
						: document.DocDate.Value;

				int bucketNumber = statementCycle.UseFinPeriodForAging == true
					? AgingEngine.AgeByPeriods(statementDate, dateForAging, graph.GetService<IFinPeriodRepository>(), AgingDirection.Backwards, 5)
					: AgingEngine.AgeByDays(statementDate, dateForAging, AgingDirection.Backwards, new int[]
					{
						statement.AgeDays00 ?? 0,
						statement.AgeDays01 ?? 0,
						statement.AgeDays02 ?? 0,
						statement.AgeDays03 ?? 0,
					});


				int balanceSign = (document.Paying == true
					&& document.DocType != ARDocType.Refund
					&& document.DocType != ARDocType.VoidRefund)
					? -1
					: 1;

				decimal docBal = balanceSign * document.DocBal.Value;
				decimal curyDocBal = balanceSign * document.CuryDocBal.Value;

				switch (bucketNumber)
				{
					case 0:
						statement.AgeBalance00 = (statement.AgeBalance00 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance00 = (statement.CuryAgeBalance00 ?? decimal.Zero) + curyDocBal;
						break;
					case 1:
						statement.AgeBalance01 = (statement.AgeBalance01 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance01 = (statement.CuryAgeBalance01 ?? decimal.Zero) + curyDocBal;
						break;
					case 2:
						statement.AgeBalance02 = (statement.AgeBalance02 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance02 = (statement.CuryAgeBalance02 ?? decimal.Zero) + curyDocBal;
						break;
					case 3:
						statement.AgeBalance03 = (statement.AgeBalance03 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance03 = (statement.CuryAgeBalance03 ?? decimal.Zero) + curyDocBal;
						break;
					case 4:
						statement.AgeBalance04 = (statement.AgeBalance04 ?? decimal.Zero) + docBal;
						statement.CuryAgeBalance04 = (statement.CuryAgeBalance04 ?? decimal.Zero) + curyDocBal;
						break;
					default:
						throw new PXException(Messages.ImpossibleToAgeDocumentUnexpectedBucketNumber);
				}
			}
			else
			{
				// Payments or small credit write-offs, in case when credits are not aged.
				// After completion we must apply residual payments to previous buckets.
				// -
				statement.AgeBalance04 = statement.AgeBalance04 - document.DocBal;
				statement.CuryAgeBalance04 = statement.CuryAgeBalance04 - document.CuryDocBal;
			}
		}

		/// <param name="isOnDemand">
		/// If set to <c>true</c>, indicates that the existing statement should be deleted
		/// so that a new on-demand statement will be generated on that date.
		/// </param>
		protected virtual IEnumerable<ARStatement> DeleteCustomerStatement(
			Customer customer,
			DateTime statementDate,
			bool isOnDemand)
		{
			StatementCreateBO persistGraph = PXGraph.CreateInstance<StatementCreateBO>();

			List<ARStatement> deletedStatementsTrace = new List<ARStatement>();

			int deletedCount = 0;

			foreach (ARStatement statement in persistGraph.CustomerStatement.Select(customer.BAccountID, statementDate))
			{
				deletedStatementsTrace.Add(StatementTrace(statement));
				persistGraph.CustomerStatement.Delete(statement);

				++deletedCount;
			}

			if (deletedCount == 0 && !isOnDemand)
			{
				DateTime? statementLastDate = persistGraph.FindLastCstmStatementDate(customer.BAccountID, statementDate);
				PXUpdate<
					Set<Override.Customer.statementLastDate, Required<Override.Customer.statementLastDate>>,
					Override.Customer,
					Where<Override.Customer.bAccountID, Equal<Required<Override.Customer.bAccountID>>>>
				.Update(this, statementLastDate, customer.BAccountID);
			}

			ResetDocumentsLastStatementDate(persistGraph, statementDate, customer.BAccountID);

			persistGraph.Actions.PressSave();

			return deletedStatementsTrace;
		}

		private void EnsureNoRegularStatementExists(int? customerID, DateTime statementDate)
		{
			PXSelectBase<ARStatement> nonOnDemandStatements = new PXSelect<
				ARStatement,
				Where<
					ARStatement.customerID, Equal<Required<ARStatement.customerID>>,
					And<ARStatement.statementDate, Equal<Required<ARStatement.statementDate>>,
					And<ARStatement.onDemand, NotEqual<True>>>>>(this);

			if (nonOnDemandStatements.Any(customerID, statementDate))
			{
				throw new PXException(Messages.StatementCoveringDateAlreadyExistsForCustomer);
			}
		}

		protected static ARStatement StatementTrace(ARStatement statement)
		{
			var trace = new ARStatement
			{
				BranchID = statement.BranchID,
				CuryID = statement.CuryID,
				CustomerID = statement.CustomerID,
				StatementDate = statement.StatementDate,
				PrevPrintedCnt = statement.PrevPrintedCnt,
				PrevEmailedCnt = statement.PrevEmailedCnt
			};

			if (statement.Printed == true)
				trace.PrevPrintedCnt++;

			if (statement.Emailed == true)
				trace.PrevEmailedCnt++;

			return trace;
		}
		#endregion
	}
}
