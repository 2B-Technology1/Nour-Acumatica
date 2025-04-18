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

using CommonServiceLocator;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common.Scopes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.PO;

namespace PX.Objects.PM
{
	public class ChangeOrderEntry : PXGraph<ChangeOrderEntry, PMChangeOrder>, PXImportAttribute.IPXPrepareItems, IGraphWithInitialization
	{
		public class MultiCurrency : ProjectBudgetMultiCurrency<ChangeOrderEntry>
		{
			protected override PXSelectBase[] GetChildren() => new PXSelectBase[]
			{
				Base.Budget
			};
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ChangeOrderEntry_ActivityDetailsExt : PMActivityDetailsExt<ChangeOrderEntry, PMChangeOrder, PMChangeOrder.noteID>
		{
			public override Type GetBAccountIDCommand() => typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMChangeOrder.customerID>>>>);

			public override Type GetEmailMessageTarget() => typeof(Select2<Contact,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>, And<Customer.defContactID, Equal<Contact.contactID>>>>,
				Where<Customer.bAccountID, Equal<Current<PMChangeOrder.customerID>>>>);
		}

		public const string ChangeOrderReport = "PM643000";
		public const string ChangeOrderNotificationCD = "CHANGE ORDER";

		#region Inner DACs
		/// <summary>
		/// Contains reversing change order data.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		[PXCacheName(Messages.ReversingChangeOrder)]
		[Serializable]
		public partial class ReversingChangeOrder: PMChangeOrder
		{
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

			public new abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		}
		#endregion

		#region DAC Overrides

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMChangeOrder.date), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(PMChangeOrder.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.bAccountID> e)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMChangeOrder.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.descr> e)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeOrder.revenueTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.curyTotalAmount> e)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMChangeOrder.revenueTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.totalAmount> e)
		{
		}

		#endregion

		#region PMChangeOrderRevenueBudget

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Quantity", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.previouslyApprovedQty> e){}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.previouslyApprovedAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Quantity", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.committedCOQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.committedCOAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Other Draft CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.otherDraftRevisedAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Total Potentially Revised Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderRevenueBudget.totalPotentialRevisedAmount> e) { }

		#endregion

		#region PMChangeOrderCostBudget

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Quantity", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.previouslyApprovedQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Previously Approved CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.previouslyApprovedAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Quantity", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.committedCOQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Current Committed CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.committedCOAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Other Draft CO Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.otherDraftRevisedAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Total Potentially Revised Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderCostBudget.totalPotentialRevisedAmount> e) { }

		#endregion

		#region PMChangeOrderLine

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Potentially Revised Quantity", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderLine.potentialRevisedQty> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Potentially Revised Amount", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMChangeOrderLine.potentialRevisedAmount> e) { }

		#endregion

		#region POLine

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		protected virtual void _(Events.CacheAttached<POLine.orderNbr> e) { }
				
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.")]
		protected virtual void _(Events.CacheAttached<POLine.lineNbr> e) { }

		#endregion

		#region PMChangeOrder

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<PMChangeOrder.classID, PMChangeOrderClass.isRevenueBudgetEnabled>))]
		protected virtual void _(Events.CacheAttached<PMChangeOrder.isRevenueVisible> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Selector<PMChangeOrder.classID, PMChangeOrderClass.isCostBudgetEnabled>))]
		protected virtual void _(Events.CacheAttached<PMChangeOrder.isCostVisible> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Switch<Case<Where<Current<PMSetup.costCommitmentTracking>, Equal<True>>, Selector<PMChangeOrder.classID, PMChangeOrderClass.isPurchaseOrderEnabled>>, False>))]
		protected virtual void _(Events.CacheAttached<PMChangeOrder.isDetailsVisible> e) { }

		#endregion

		#endregion

		[PXCopyPasteHiddenFields(typeof(PMChangeOrder.projectNbr))]
		[PXViewName(Messages.ChangeOrder)]
		public SelectFrom<PMChangeOrder>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<PMChangeOrder.projectID>>
			.Where<PMProject.contractID.IsNull
				.Or<MatchUserFor<PMProject>>>
			.View Document;

		public PXSelect<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> DocumentSettings;
		public PXSelect<PMChangeOrder, Where<PMChangeOrder.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> VisibilitySettings;

		[PXCopyPasteHiddenView]
		[PXViewName(PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMChangeOrder.projectID.FromCurrent>> Project;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMChangeOrder.projectID>>>> ProjectProperties;

		[PXCopyPasteHiddenView]
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer>.Where<Customer.bAccountID.IsEqual<PMChangeOrder.customerID.AsOptional>> Customer;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }

		[InjectDependency]
		public IUnitRateService RateService { get; set; }

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderCostBudget, 
			LeftJoin<PMBudget, On<PMBudget.projectID, Equal<PMChangeOrderCostBudget.projectID>,
				And<PMBudget.projectTaskID, Equal<PMChangeOrderCostBudget.projectTaskID>,
				And<PMBudget.accountGroupID, Equal<PMChangeOrderCostBudget.accountGroupID>,
				And<PMBudget.inventoryID, Equal<PMChangeOrderCostBudget.inventoryID>,
				And<PMBudget.costCodeID, Equal<PMChangeOrderCostBudget.costCodeID>>>>>>>,
			Where<PMChangeOrderCostBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderCostBudget.type, Equal<GL.AccountType.expense>>>> CostBudget;
				
		public virtual IEnumerable costBudget()
		{
			List<PXResult<PMChangeOrderCostBudget, PMBudget>> result = new List<PXResult<PMChangeOrderCostBudget, PMBudget>>();
 
			var select = new PXSelect<PMChangeOrderCostBudget,
			Where<PMChangeOrderCostBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderCostBudget.type, Equal<GL.AccountType.expense>>>>(this);

			foreach (PMChangeOrderCostBudget record in select.Select())
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalCostBudget(BudgetKeyTuple.Create(record)) : null;
				if (budget == null) budget = new PMBudget();

				result.Add(new PXResult<PMChangeOrderCostBudget, PMBudget>(record, budget));
			}

			return result;
		}

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderRevenueBudget,
			LeftJoin<PMBudget, On<PMBudget.projectID, Equal<PMChangeOrderRevenueBudget.projectID>,
				And<PMBudget.projectTaskID, Equal<PMChangeOrderRevenueBudget.projectTaskID>,
				And<PMBudget.accountGroupID, Equal<PMChangeOrderRevenueBudget.accountGroupID>,
				And<PMBudget.inventoryID, Equal<PMChangeOrderRevenueBudget.inventoryID>,
				And<PMBudget.costCodeID, Equal<PMChangeOrderRevenueBudget.costCodeID>>>>>>>,
			Where<PMChangeOrderRevenueBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>> RevenueBudget;

		public virtual IEnumerable revenueBudget()
		{
			List<PXResult<PMChangeOrderRevenueBudget, PMBudget>> result = new List<PXResult<PMChangeOrderRevenueBudget, PMBudget>>();

			var select = new PXSelect<PMChangeOrderRevenueBudget,
			Where<PMChangeOrderRevenueBudget.refNbr, Equal<Current<PMChangeOrder.refNbr>>,
			And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

			foreach (PMChangeOrderRevenueBudget record in select.Select())
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(record)) : null;
				if (budget == null) budget = new PMBudget();

				result.Add(new PXResult<PMChangeOrderRevenueBudget, PMBudget>(record, budget));
			}

			return result;
		}

		[PXCopyPasteHiddenView]
		public PXSelect<PMCostBudget> AvailableCostBudget;
		public virtual IEnumerable availableCostBudget()
		{
			HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
			foreach (PXResult<PMChangeOrderCostBudget, PMBudget> res in costBudget())
			{
				existing.Add(BudgetKeyTuple.Create((PMChangeOrderCostBudget)res));
			}

			foreach (PMBudget budget in GetCostBudget() )
			{
				if (budget.Type != GL.AccountType.Expense)
					continue;

				if (existing.Contains(BudgetKeyTuple.Create(budget)))
					budget.Selected = true;

				yield return budget;
			}
		}

		[PXCopyPasteHiddenView]
		public PXSelect<PMRevenueBudget> AvailableRevenueBudget;
		public virtual IEnumerable availableRevenueBudget()
		{
			HashSet<BudgetKeyTuple> existing = new HashSet<BudgetKeyTuple>();
			foreach (PXResult<PMChangeOrderRevenueBudget, PMBudget> res in revenueBudget())
			{
				existing.Add(BudgetKeyTuple.Create((PMChangeOrderRevenueBudget)res));
			}

			foreach (PMBudget budget in GetRevenueBudget())
			{
				if (budget.Type != GL.AccountType.Income)
					continue;

				if (existing.Contains(BudgetKeyTuple.Create(budget)))
					budget.Selected = true;

				yield return budget;
			}
		}

		[PXImport(typeof(PMChangeOrder))]
		[PXFilterable]
		public PXSelectJoin<PMChangeOrderLine,
			LeftJoin<POLinePM, On<POLinePM.orderType, Equal<PMChangeOrderLine.pOOrderType>, 
				And<POLinePM.orderNbr, Equal<PMChangeOrderLine.pOOrderNbr>, 
				And<POLinePM.lineNbr, Equal<PMChangeOrderLine.pOLineNbr>>>>>,
			Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>>> Details;

		protected Dictionary<POLineKey, POLinePM> polines;
		public IEnumerable details()
		{
			List<PXResult<PMChangeOrderLine, POLinePM>> result = new List<PXResult<PMChangeOrderLine, POLinePM>>(200);

			var select = new PXSelectJoin<PMChangeOrderLine,
				LeftJoin<POLinePM, On<POLinePM.orderType, Equal<PMChangeOrderLine.pOOrderType>,
				And<POLinePM.orderNbr, Equal<PMChangeOrderLine.pOOrderNbr>,
				And<POLinePM.lineNbr, Equal<PMChangeOrderLine.pOLineNbr>>>>>,
				Where<PMChangeOrderLine.refNbr, Equal<Current<PMChangeOrder.refNbr>>>>(this);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			if (polines == null || IsCacheUpdateRequired())
			{
				polines = new Dictionary<POLineKey, POLinePM>();
			}


			foreach (PXResult<PMChangeOrderLine, POLinePM> res in select.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				PMChangeOrderLine line = res;
				POLinePM poLine = res;
				if (IsValidKey(line) && poLine.LineNbr == null)
				{
					poLine = GetPOLine(line);
					if (poLine == null)
						poLine = res;
				}
				
				if (poLine.LineNbr != null)
				{
					POLineKey key = GetKey(poLine);
					if (!polines.ContainsKey(key))
					{
						polines.Add(key, poLine);
					}
				}

				result.Add(new PXResult<PMChangeOrderLine, POLinePM>(line, poLine));
			}
			PXView.StartRow = 0;

			return result;
		}

		public PXFilter<POLineFilter> AvailablePOLineFilter;

		[PXCopyPasteHiddenView]
		public PXSelect<POLinePM> AvailablePOLines;

		public IEnumerable availablePOLines()
		{
			List<POLinePM> result = new List<POLinePM>(200);

			var select = new PXSelect<POLinePM,
			Where2<Where<Current<POLineFilter.vendorID>, IsNull, Or<POLinePM.vendorID, Equal<Current<POLineFilter.vendorID>>>>,
			And2<Where<Current<POLineFilter.pOOrderNbr>, IsNull, Or<POLinePM.orderNbr, Equal<Current<POLineFilter.pOOrderNbr>>>>,
			And2<Where<Current<POLineFilter.projectTaskID>, IsNull, Or<POLinePM.taskID, Equal<Current<POLineFilter.projectTaskID>>>>,
			And2<Where<Current<POLineFilter.inventoryID>, IsNull, Or<POLinePM.inventoryID, Equal<Current<POLineFilter.inventoryID>>>>,
			And2<Where<Current<POLineFilter.costCodeFrom>, IsNull, Or<POLinePM.costCodeCD, GreaterEqual<Current<POLineFilter.costCodeFrom>>>>,
			And2<Where<Current<POLineFilter.costCodeTo>, IsNull, Or<POLinePM.costCodeCD, LessEqual<Current<POLineFilter.costCodeTo>>>>,
			And<POLinePM.projectID, Equal<Current<PMChangeOrder.projectID>>,
			And2<Where<POLinePM.cancelled, NotEqual<True>, Or<Current<POLineFilter.includeNonOpen>, Equal<True>>>, 
			And<Where<POLinePM.completed, NotEqual<True>, Or<Current<POLineFilter.includeNonOpen>, Equal<True>>>>>>>>>>>>>(this);
			
			int startRow = PXView.StartRow;
			int totalRows = 0;

			if (polines == null || IsCacheUpdateRequired())
			{
				polines = new Dictionary<POLineKey, POLinePM>();
			}

			result.AddRange(select.View.Select(
				PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows).RowCast<POLinePM>());
			PXView.StartRow = 0;

			return result;
		}
		[PXViewName(Messages.ChangeOrderClass)]
		public PXSetup<PMChangeOrderClass>.Where<PMChangeOrderClass.classID.IsEqual<PMChangeOrder.classID.AsOptional>> ChangeOrderClass;

		[PXViewName(CR.Messages.Answers)]
		public CRAttributeList<PMChangeOrder> Answers;

		[PXCopyPasteHiddenView]
		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<PMChangeOrder, PMChangeOrder.approved, PMChangeOrder.rejected, PMChangeOrder.hold, PMSetupChangeOrderApproval> Approval;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> Company;

		public PXSetup<APSetup> apSetup;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<PMBudgetAccum> Budget;

		[PXHidden]
		public PXSelect<PMForecastHistoryAccum> ForecastHistory;

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<POOrder> Order;


		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<POLine> dummyPOLine; //Added for the sake of Cache_Attached.

		[PXHidden]
		[PXCopyPasteHiddenView]
		public PXSelect<Vendor> dummyVendor;

		[PXCopyPasteHiddenView]
		public SelectFrom<ReversingChangeOrder>
					.Where<ReversingChangeOrder.origRefNbr.IsEqual<PMChangeOrder.refNbr.FromCurrent>>
					.View ReversingChangeOrders;

		[PXRemoveBaseAttribute(typeof(PXUIFieldAttribute))]
		[PXUIField(DisplayName = "Commitment Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		[POOrderType.RPSList]
		protected virtual void _(Events.CacheAttached<POLine.orderType> e)
		{
		}

		public ChangeOrderEntry()
		{
			APSetup apsetup = apSetup.Current;
			AvailablePOLines.AllowInsert = false;
			AvailablePOLines.AllowDelete = false;
		}

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

		protected virtual void BeforeCommitHandler(PXGraph e)
		{
			var check1 = _licenseLimits.GetCheckerDelegate<PMChangeOrder>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMChangeOrderBudget), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMChangeOrderBudget.refNbr>(((ChangeOrderEntry)graph).Document.Current?.RefNbr)
				};
			}));

			try
			{
				check1.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseCostBudgetAndRevenueBudget);
			}

			var check2 = _licenseLimits.GetCheckerDelegate<PMChangeOrder>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMChangeOrderLine), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMChangeOrderLine.refNbr>(((ChangeOrderEntry)graph).Document.Current?.RefNbr)
				};
			}));

			try
			{
				check2.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseCommitments);
			}
		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += BeforeCommitHandler;
			}
			OnBeforeCommit += SaveChangesToTaskMadeAfterAccumulatorPersited;
		}
		protected virtual void SaveChangesToTaskMadeAfterAccumulatorPersited(PXGraph e)
		{
			Persist(typeof(PMTask), PXDBOperation.Update);
		}

		protected ProjectBalance balanceCalculator;

		public virtual ProjectBalance BalanceCalculator
		{
			get
			{
				if (balanceCalculator == null)
				{
					balanceCalculator = new ProjectBalance(this);
				}

				return balanceCalculator;
			}
		}

		#region Actions
		public PXAction<PMChangeOrder> release;
		[PXUIField(DisplayName = GL.Messages.Release)]
		[PXProcessButton]
		public IEnumerable Release(PXAdapter adapter)
		{
			List<PMChangeOrder> list = new List<PMChangeOrder>();

			foreach (PXResult<PMChangeOrder, PMProject> item in adapter.Get())
			{
				PMChangeOrder order = item;
				list.Add(order);
			}

			Save.Press();

			PXLongOperation.StartOperation(this, delegate () {

				ChangeOrderEntry graph = PXGraph.CreateInstance<ChangeOrderEntry>();
				graph.Document.Current = Document.Current;
				using (new ChangeOrderReleaseScope())
				{
					graph.ReleaseDocument(Document.Current);
				}
			});

			return list;
		}

		public PXAction<PMChangeOrder> reverse;
		[PXUIField(DisplayName = Messages.Reverse)]
		[PXProcessButton]
		public IEnumerable Reverse(PXAdapter adapter)
		{
			ReverseDocument();

			return new PMChangeOrder[] { Document.Current };
		}

		public PXAction<PMChangeOrder> approve;
		[PXUIField(DisplayName = "Approve", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public IEnumerable Approve(PXAdapter adapter)
		{
			List<PMChangeOrder> list = new List<PMChangeOrder>();

			foreach (PXResult<PMChangeOrder, PMProject> res in adapter.Get())
			{
				PMChangeOrder item = res;
				list.Add(item);
			}

			if (IsDirty)
				Save.Press();

			foreach (PMChangeOrder item in list)
			{
				try
				{
				if (item.Approved == true)
					continue;

				if (Setup.Current.ChangeOrderApprovalMapID != null)
				{
					if (!Approval.Approve(item))
						throw new PXSetPropertyException(Common.Messages.NotApprover);
					item.Approved = Approval.IsApproved(item);
					if (item.Approved == true)
						item.Status = ChangeOrderStatus.Open;
				}
				else
				{
					item.Approved = true;
					item.Status = ChangeOrderStatus.Open;
				}

				Document.Update(item);

				Save.Press();
				}
				catch (ReasonRejectedException) { }

				yield return item;
			}
		}		

		public PXAction<PMChangeOrder> reject;
		[PXUIField(DisplayName = "Reject", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable Reject(PXAdapter adapter)
		{
			IncreaseDraftBucket(-1);
			Save.Press();
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> coReport;
		[PXUIField(DisplayName = "Print", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable COReport(PXAdapter adapter)
		{
			OpenReport(ChangeOrderReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMChangeOrder doc)
		{
			if (doc != null && Document.Cache.GetStatus(doc) != PXEntryStatus.Inserted)
			{
				string specificReportID = new NotificationUtility(this).SearchProjectReport(reportID, Project.Current.ContractID, Project.Current.DefaultBranchID);

				throw new PXReportRequiredException(new Dictionary<string, string>
				{
					["RefNbr"] = doc.RefNbr
				}, specificReportID, specificReportID);
			}
		}

		public PXAction<PMChangeOrder> send;
		[PXUIField(DisplayName = "Email", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Send(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				Save.Press();

				var graph = CreateInstance<ChangeOrderEntry>();
				graph.Document.Current = Document.Current;

				var changeOrderRefNumber = Document.Current.RefNbr;
				var massProcess = adapter.MassProcess;
				var defaultBranchId = Project.Current.DefaultBranchID;

				PXLongOperation.StartOperation(this, delegate ()
				{
					SendReport(graph, changeOrderRefNumber, ChangeOrderNotificationCD, defaultBranchId, massProcess);
				});
			}

			return adapter.Get();
		}

		public virtual void SendReport(string notificationCD, PMChangeOrder doc, bool massProcess = false)
		{
			if (doc != null)
			{
				if (Document.Current != doc)
				{
					Document.Current = doc;
				}

				SendReport(this, doc.RefNbr, notificationCD, Project.Current.DefaultBranchID, massProcess);
			}
		}

		public static void SendReport(ChangeOrderEntry graph, string changeOrderRefNbr, string notificationCD, int? defaultBranchId, bool massProcess)
		{
			try
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>
				{
					["RefNbr"] = changeOrderRefNbr
				};

				using (var ts = new PXTransactionScope())
				{
					graph.GetExtension<ChangeOrderEntry_ActivityDetailsExt>()
						.SendNotification(PMNotificationSource.Project, notificationCD, defaultBranchId, mailParams, massProcess);

					graph.Save.Press();

					ts.Complete();
				}
			}
			catch (CR.Descriptor.Exceptions.EmailFromReportCannotBeCreatedException ex)
			{
				throw new PXException(ex,
						Messages.ChangeOrderReportEmailingError,
						changeOrderRefNbr,
						ex.ReportId);
			}
		}

		public PXAction<PMChangeOrder> addCostBudget;
		[PXUIField(DisplayName = "Select Budget Lines")]
		[PXButton]
		public IEnumerable AddCostBudget(PXAdapter adapter)
		{
			if (AvailableCostBudget.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedCostBudget();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedCostBudget;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedCostBudget(PXAdapter adapter)
		{
			AddSelectedCostBudget();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> addRevenueBudget;
		[PXUIField(DisplayName = "Select Budget Lines")]
		[PXButton]
		public IEnumerable AddRevenueBudget(PXAdapter adapter)
		{
			if (AvailableRevenueBudget.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedRevenueBudget();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedRevenueBudget;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedRevenueBudget(PXAdapter adapter)
		{
			AddSelectedRevenueBudget();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> addPOLines;
		[PXUIField(DisplayName = "Select Commitments")]
		[PXButton]
		public IEnumerable AddPOLines(PXAdapter adapter)
		{
			if (AvailablePOLines.View.AskExt(
				(graph, view) =>
				{
					AvailablePOLines.Cache.Clear();
					AvailablePOLines.View.Clear();
					AvailablePOLines.Cache.ClearQueryCacheObsolete();
				}, true) == WebDialogResult.OK)
			{
				AddSelectedPOLines();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> appendSelectedPOLines;
		[PXUIField(DisplayName = "Add Lines")]
		[PXButton]
		public IEnumerable AppendSelectedPOLines(PXAdapter adapter)
		{
			AddSelectedPOLines();

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCommitments;
		[PXUIField(DisplayName = Messages.ViewCommitments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public IEnumerable ViewCommitments(PXAdapter adapter)
		{
			if (Details.Current != null && !string.IsNullOrEmpty(Details.Current.POOrderNbr))
			{
				POOrderEntry target = PXGraph.CreateInstance<POOrderEntry>();
				target.Document.Current = PXSelect<POOrder, Where<POOrder.orderType, Equal<Current<PMChangeOrderLine.pOOrderType>>,
					And<POOrder.orderNbr, Equal<Current<PMChangeOrderLine.pOOrderNbr>>>>>.Select(this);

				throw new PXRedirectRequiredException(target, true, Messages.ViewCommitments) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewChangeOrder;
		[PXUIField(DisplayName = Messages.ViewChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]

		public virtual IEnumerable ViewChangeOrder(PXAdapter adapter)
		{
			return ViewChangeOrderActionImplementation(adapter, Document.Current?.OrigRefNbr, Messages.ViewChangeOrder);
		}

		public PXAction<PMChangeOrder> viewReversingChangeOrders;
		[PXUIField(DisplayName = Messages.ViewReversingChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]

		public virtual IEnumerable ViewReversingChangeOrders(PXAdapter adapter)
		{
			var reveresingOrderRefs = GetReversingOrderRefs();

			if (reveresingOrderRefs.Length == 0)
				return adapter.Get();

			if (reveresingOrderRefs.Length == 1)
			{
				return ViewChangeOrderActionImplementation(adapter, reveresingOrderRefs[0], Messages.ViewReversingChangeOrder);
			}
			else
			{
				ReversingChangeOrders.AskExt();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCurrentReversingChangeOrder;
		[PXUIField(DisplayName = Messages.ViewReversingChangeOrder, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]

		public virtual IEnumerable ViewCurrentReversingChangeOrder(PXAdapter adapter)
		{
			if (ReversingChangeOrders.Current != null)
			{
				return ViewChangeOrderActionImplementation(adapter, ReversingChangeOrders.Current.RefNbr, Messages.ViewReversingChangeOrder);
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewRevenueBudgetTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRevenueBudgetTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current< PMChangeOrderRevenueBudget .projectID>>, And<PMTask.taskID, Equal<Current<PMChangeOrderRevenueBudget.projectTaskID>>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewCostBudgetTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCostBudgetTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMChangeOrderCostBudget.projectID>>, And<PMTask.taskID, Equal<Current<PMChangeOrderCostBudget.projectTaskID>>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewCommitmentTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCommitmentTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMChangeOrderLine.projectID>>, And<PMTask.taskID, Equal<Current<PMChangeOrderLine.taskID>>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMChangeOrder> viewRevenueBudgetInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRevenueBudgetInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderRevenueBudget.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCostBudgetInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCostBudgetInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderCostBudget.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> viewCommitmentInventory;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewCommitmentInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMChangeOrderLine.inventoryID>>>>.Select(this);
			if (item != null && item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = graph.Item.Search<InventoryItem.inventoryID>(item.InventoryID);
					throw new PXRedirectRequiredException(graph, "Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMChangeOrder> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter)
		{
			if (Document.Current.Status == ChangeOrderStatus.Rejected || Document.Current.Status == ChangeOrderStatus.Canceled)
			{
				IncreaseDraftBucket(1);
				Save.Press();
			}

			return adapter.Get();
		}

		public PXAction<PMChangeOrder> removeHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable RemoveHold(PXAdapter adapter) => adapter.Get();

		public PXAction<PMChangeOrder> coCancel;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Cancel")]
		protected virtual IEnumerable COCancel(PXAdapter adapter)
		{
			if (Document.Current.Status != ChangeOrderStatus.Rejected)
			{
				IncreaseDraftBucket(-1);
			}

			Document.Current.Hold = false;
			Document.Current.Approved = false;
			Document.Current.Rejected = false;
			Document.Update(Document.Current);

			return adapter.Get();
		}
		#endregion

		#region Event handlers

		#region PMChangeOrder
		protected virtual void _(Events.RowSelected<PMChangeOrder> e)
		{
			Details.Cache.AllowSelect = Setup.Current.CostCommitmentTracking == true;

			string budgetLevelCost = BudgetLevels.Detail;
			string budgetLevelRevenue = BudgetLevels.Detail;
			if (Project.Current != null)
			{
				budgetLevelCost = Project.Current.CostBudgetLevel;
				budgetLevelRevenue = Project.Current.BudgetLevel;
			}

			if (e.Row != null)
			{
				bool isOrderInReversalProcess = e.Row.ReverseStatus != ChangeOrderReverseStatus.None;
				PXUIFieldAttribute.SetVisible<PMChangeOrder.reverseStatus>(e.Cache, e.Row, isOrderInReversalProcess);
				PXUIFieldAttribute.SetVisible<PMChangeOrder.origRefNbr>(e.Cache, e.Row, isOrderInReversalProcess);
				PXUIFieldAttribute.SetVisible<PMChangeOrder.reversingRefNbr>(e.Cache, e.Row, isOrderInReversalProcess);

				if (!IsCopyPasteContext && !IsImport && !IsExport)
                {
                    PXUIFieldAttribute.SetVisible<PMChangeOrderRevenueBudget.inventoryID>(RevenueBudget.Cache, null, budgetLevelRevenue == BudgetLevels.Item || budgetLevelRevenue == BudgetLevels.Detail);
                    PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.inventoryID>(CostBudget.Cache, null, budgetLevelCost == BudgetLevels.Item || budgetLevelCost == BudgetLevels.Detail);
                    PXUIFieldAttribute.SetVisible<PMChangeOrderRevenueBudget.costCodeID>(RevenueBudget.Cache, null, budgetLevelRevenue == BudgetLevels.CostCode || budgetLevelRevenue == BudgetLevels.Detail);
                    PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.costCodeID>(CostBudget.Cache, null, budgetLevelCost == BudgetLevels.CostCode || budgetLevelCost == BudgetLevels.Detail);
                }

                PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedInvoicedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedInvoicedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedOpenAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedOpenQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedReceivedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.committedCOQty> (Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMBudget.curyCommittedCOAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.committedCOQty>(CostBudget.Cache, null, Setup.Current.CostCommitmentTracking == true);
				PXUIFieldAttribute.SetVisible<PMChangeOrderCostBudget.committedCOAmount>(CostBudget.Cache, null, Setup.Current.CostCommitmentTracking == true);

				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedInvoicedAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedInvoicedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.curyCommittedOpenAmount>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedOpenQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetVisibility<PMBudget.committedReceivedQty>(Caches[typeof(PMBudget)], null, Setup.Current.CostCommitmentTracking == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				bool isEditable = CanEditDocument(e.Row);

				Document.Cache.AllowDelete = isEditable;

				addRevenueBudget.SetEnabled(isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true);
				RevenueBudget.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;
				RevenueBudget.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;
				RevenueBudget.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsRevenueBudgetEnabled == true;

				addCostBudget.SetEnabled(isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true);
				CostBudget.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;
				CostBudget.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;
				CostBudget.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsCostBudgetEnabled == true;

				addPOLines.SetEnabled(isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true);
				Details.Cache.AllowInsert = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				Details.Cache.AllowUpdate = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				Details.Cache.AllowDelete = isEditable && ChangeOrderClass.Current?.IsPurchaseOrderEnabled == true;
				
				Answers.Cache.AllowInsert = isEditable;
				Answers.Cache.AllowUpdate = isEditable;
				Answers.Cache.AllowDelete = isEditable;

				if (!this.IsContractBasedAPI)
				{
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.classID>(e.Cache, e.Row, isEditable);
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.projectID>(e.Cache, e.Row, isEditable && IsProjectEnabled());
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.description>(e.Cache, e.Row, isEditable);
					PXUIFieldAttribute.SetEnabled<PMChangeOrder.completionDate>(e.Cache, e.Row, isEditable);
				}
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.extRefNbr>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.projectNbr>(e.Cache, e.Row, isEditable && e.Row.IsRevenueVisible == true && e.Row.ProjectNbr != Messages.NotAvailable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.date>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.delayDays>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMChangeOrder.text>(e.Cache, e.Row, isEditable);

				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.pOOrderType>(Details.Cache, null, true);
				PXUIFieldAttribute.SetVisible<PMChangeOrderLine.pOOrderType>(Details.Cache, null, true);
			}
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrder> e)
		{
			PMChangeOrder deletedOrder = e.Row;

			PMChangeOrderClass orderClass = PMChangeOrderClass.PK.Find(this, deletedOrder.ClassID);
			if (orderClass != null && orderClass.IncrementsProjectNumber == true)
			{
				if (Project.Current != null && Project.Current.LastChangeOrderNumber == deletedOrder.ProjectNbr &&
					string.IsNullOrEmpty(Project.Current.LastChangeOrderNumber) == false)
				{
					Project.Current.LastChangeOrderNumber = NumberHelper.DecreaseNumber(Project.Current.LastChangeOrderNumber, 1);
					ProjectProperties.Update(Project.Current);
				}
			}
			UpdateOriginalReverseStatus(e.Cache, deletedOrder);
		}

		private void UpdateOriginalReverseStatus(PXCache cache, PMChangeOrder order)
		{
			if (order.ReverseStatus == ChangeOrderReverseStatus.Reversal)
			{
				PMChangeOrder originalOrder = SelectFrom<PMChangeOrder>
					.Where<PMChangeOrder.refNbr.IsEqual<PMChangeOrder.origRefNbr.FromCurrent>>
					.View
					.Select(this);

				// Has reversing orders, except current.
				bool originalHasReversing = SelectFrom<PMChangeOrder>
					.Where
						<PMChangeOrder.origRefNbr.IsEqual<@P.AsString>.And
						<PMChangeOrder.refNbr.IsNotEqual<@P.AsString>>>
					.View
					.Select(this, originalOrder.RefNbr, order.RefNbr)
					.Count > 0;

				if (originalHasReversing)
				{
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.Reversed;
				}
				else if (!string.IsNullOrWhiteSpace(originalOrder.OrigRefNbr))
				{
					// If original order has his own original.
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.Reversal;
				}
				else
				{
					originalOrder.ReverseStatus = ChangeOrderReverseStatus.None;
				}

				cache.Update(originalOrder);

				// Without this action, TimeStamp uses the value from the row that is being deleted,
				// which leads to the error "Data was updated by another operation".
				PXDBTimestampAttribute timestampAttribute = cache
					.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(PMChangeOrder.tstamp))
					.First();

				timestampAttribute.VerifyTimestamp = VerifyTimestampOptions.FromRecord;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{			
			PMChangeOrderClass orderClass = PMChangeOrderClass.PK.Find(this, e.Row.ClassID);
			if (orderClass != null && e.Row.ProjectID != null && orderClass.IncrementsProjectNumber == true)
			{
				PMProject project = GetProject(e.Row.ProjectID);
				string lastNumber = string.IsNullOrEmpty(project?.LastChangeOrderNumber) ? "0000" : project.LastChangeOrderNumber;

				if (!char.IsDigit(lastNumber[lastNumber.Length - 1]))
				{
					lastNumber = string.Format("{0}0000", lastNumber);
				}

				e.NewValue = NumberHelper.IncreaseNumber(lastNumber, 1);
			}
			else
			{
				e.NewValue = Messages.NotAvailable;
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{
			string val = (string)e.NewValue;

			if (val.Equals(Messages.NotAvailable, StringComparison.InvariantCultureIgnoreCase))
				return;

			var selectDuplicate = new PXSelect<PMChangeOrder, Where<PMChangeOrder.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrder.projectNbr, Equal<Required<PMChangeOrder.projectNbr>>,
				And<PMChangeOrder.reverseStatus, NotEqual<ChangeOrderReverseStatus.reversed>>>>>(this);

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				selectDuplicate.WhereAnd<Where<PMChangeOrder.refNbr, NotEqual<Current<PMChangeOrder.refNbr>>>>();
			}

			PMChangeOrder duplicate = selectDuplicate.Select(e.NewValue);

			if (duplicate != null && duplicate != e.Row && duplicate.RefNbr != e.Row.RefNbr)
			{
				throw new PXSetPropertyException(Messages.DuplicateChangeOrderNumber, duplicate.RefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.projectNbr> e)
		{
			PMChangeOrderClass orderClass = PMChangeOrderClass.PK.Find(this, e.Row.ClassID);
			if (orderClass != null && e.Row.ProjectID != null && orderClass.IncrementsProjectNumber == true)
			{
			PMProject project = GetProject(e.Row.ProjectID);
			if (project != null && e.Row.ProjectNbr != Messages.NotAvailable)
			{
					if (string.IsNullOrEmpty(project.LastChangeOrderNumber) == true ||
						(string.IsNullOrEmpty(e.Row.ProjectNbr) == false &&
							NumberHelper.GetTextPrefix(e.Row.ProjectNbr) == NumberHelper.GetTextPrefix(project.LastChangeOrderNumber) &&
							NumberHelper.GetNumericValue(e.Row.ProjectNbr) >= NumberHelper.GetNumericValue(project.LastChangeOrderNumber)))
				{
					project.LastChangeOrderNumber = e.Row.ProjectNbr;
					ProjectProperties.Update(project);
				}
			}
		}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.projectID> e)
		{
			PMProject project = PMProject.PK.Find(this, (int)e.NewValue);
			if (project != null)
			{
				if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled || project.Status == ProjectStatus.Suspended)
				{
					var listAttribute = new ProjectStatus.ListAttribute();
					string status = listAttribute.ValueLabelDic[project.Status];
					var ex = new PXSetPropertyException(Messages.CreateCOProjectStatusError, PXErrorLevel.Error, status);
					ex.ErrorValue = project.ContractCD;
					throw ex;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.projectID> e)
		{
			PMChangeOrderClass orderClass = PMChangeOrderClass.PK.Find(this, e.Row.ClassID);
			if (orderClass != null && orderClass.IncrementsProjectNumber == true)
			{
				PMProject oldProject = GetProject((int?)e.OldValue);
				if (oldProject != null && oldProject.LastChangeOrderNumber == e.Row.ProjectNbr &&
					string.IsNullOrEmpty(Project.Current.LastChangeOrderNumber) == false)
				{
					oldProject.LastChangeOrderNumber = NumberHelper.DecreaseNumber(oldProject.LastChangeOrderNumber, 1);
					ProjectProperties.Update(oldProject);
				}
				e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
			}
		}

		protected virtual void _(Events.FieldSelecting<PMChangeOrder.reversingRefNbr> e)
		{
			if (e.Row == null)
				return;

			var reversingOrderRefs = GetReversingOrderRefs();

			if (reversingOrderRefs.Length == 0)
				return;

			if (reversingOrderRefs.Length == 1)
			{
				e.ReturnValue = reversingOrderRefs[0];
			}
			else
			{
				e.ReturnValue = "<LIST>";
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrder, PMChangeOrder.classID> e)
		{
			PMProject project = GetProject(e.Row.ProjectID);

			PMChangeOrderClass oldClass = PMChangeOrderClass.PK.Find(this, (string)e.OldValue);
			PMChangeOrderClass newClass = PMChangeOrderClass.PK.Find(this, e.Row.ClassID);

			if (oldClass != null && newClass != null && oldClass.IncrementsProjectNumber != newClass.IncrementsProjectNumber)
			{
				if (project != null && oldClass.IncrementsProjectNumber == true && project.LastChangeOrderNumber == e.Row.ProjectNbr &&
					string.IsNullOrEmpty(Project.Current.LastChangeOrderNumber) == false)
				{
					project.LastChangeOrderNumber = NumberHelper.DecreaseNumber(Project.Current.LastChangeOrderNumber, 1);
					ProjectProperties.Update(project);
				}

				e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
			}
			else if (oldClass == null && newClass != null)
			{
				e.Cache.SetDefaultExt<PMChangeOrder.projectNbr>(e.Row);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.classID> e)
		{
			if (!string.IsNullOrEmpty(e.Row.ClassID))
			{
				PMChangeOrderClass newClass = PMChangeOrderClass.PK.Find(this, (string) e.NewValue);
				if (newClass != null)
				{
					if (newClass.IsRevenueBudgetEnabled != true)
					{
						if (RevenueBudget.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsRevenueBudget);
						}
					}

					if (newClass.IsCostBudgetEnabled != true)
					{
						if (CostBudget.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsCostBudget);
						}
					}

					if (newClass.IsPurchaseOrderEnabled != true)
					{
						if (Details.Select().Count > 0)
						{
							throw new PXSetPropertyException<PMChangeOrder.classID>(Messages.ChangeOrderContainsDetails);
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrder, PMChangeOrder.date> e)
		{
			if (e.NewValue != null)
			{
				Func<PXGraph, IFinPeriodRepository> factoryFunc = (Func<PXGraph, IFinPeriodRepository>)ServiceLocator.Current.GetService(typeof(Func<PXGraph, IFinPeriodRepository>));
				IFinPeriodRepository service = factoryFunc(this);

				DateTime? date = (DateTime?)e.NewValue;
				string strDate = string.Empty;
				if (date != null)
				{
					strDate = date.Value.ToShortDateString();
				}
				if (service != null)
				{
					try
					{						
						var finperiod = service.GetFinPeriodByDate(date, PXAccess.GetParentOrganizationID(Accessinfo.BranchID));

						if (finperiod == null)
						{
							throw new PXSetPropertyException(Messages.ChnageOrderInvalidDate, strDate);
						}
					}
					catch (PXException ex)
					{
						throw new PXSetPropertyException(ex, PXErrorLevel.Error, Messages.ChnageOrderInvalidDate, strDate);
					}
				}
			}
		}
		#endregion

		#region PMChangeOrderCostBudget
		protected virtual void _(Events.RowSelected<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
			PMBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;

			PXUIFieldAttribute.SetEnabled<PMChangeOrderCostBudget.uOM>(e.Cache, e.Row, budget == null);
        }

		protected virtual void _(Events.RowInserting<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowInserted<PMChangeOrderCostBudget> e)
		{
			IncreaseDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}
				
		protected virtual void _(Events.RowUpdating<PMChangeOrderCostBudget> e)
		{
			InitCostBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowUpdated<PMChangeOrderCostBudget> e)
		{
			IncreaseDraftBucket(e.OldRow, -1);
			IncreaseDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrderCostBudget> e)
		{
			IncreaseDraftBucket(e.Row, -1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.accountGroupID> e)
		{
			if (e.Row == null) return;
			if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderCostBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
				if (item != null)
				{
					Account account = PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item) as Account;
					if (account != null && account.AccountGroupID != null)
					{
						e.NewValue = account.AccountGroupID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.uOM> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderCostBudget, PMChangeOrderCostBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderCostBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.uOM> e)
		{
			PMCostBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.UOM;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.rate> e)
		{
			PMCostBudget budget = IsValidKey(e.Row) ? GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.CuryUnitRate;
			}
            else
            {
				PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
				e.NewValue = RateService.CalculateUnitCost(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, null, Document.Current?.Date, project?.CuryInfoID);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderCostBudget, PMChangeOrderCostBudget.description> e)
		{
			if (e.Row == null || Project.Current == null) return;

			PMCostBudget budget = GetOriginalCostBudget(BudgetKeyTuple.Create(e.Row));
					if (budget != null)
					{
						e.NewValue = budget.Description; 
					}
					else
					{
				if (Project.Current.CostBudgetLevel == BudgetLevels.CostCode)
				{
					if (e.Row.CostCodeID != null)
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
				else if (Project.Current.CostBudgetLevel == BudgetLevels.Task)
				{
					if (e.Row.ProjectTaskID != null)
					{
						PMTask projectTask = PXSelectorAttribute.Select<PMChangeOrderCostBudget.projectTaskID>(e.Cache, e.Row) as PMTask;
						if (projectTask != null)
						{
							e.NewValue = projectTask.Description;
						}
					}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		}
		#endregion

		#region PMChangeOrderRevenueBudget
		protected virtual void _(Events.RowSelected<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			PXUIFieldAttribute.SetEnabled<PMChangeOrderRevenueBudget.uOM>(e.Cache, e.Row, budget == null);
		}

		protected virtual void _(Events.RowInserting<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowInserted<PMChangeOrderRevenueBudget> e)
		{
			IncreaseDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}
		
		protected virtual void _(Events.RowUpdating<PMChangeOrderRevenueBudget> e)
		{
			InitRevenueBudgetFields(e.Row);
		}

		protected virtual void _(Events.RowDeleted<PMChangeOrderRevenueBudget> e)
		{
			IncreaseDraftBucket(e.Row, -1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.RowUpdated<PMChangeOrderRevenueBudget> e)
		{
			IncreaseDraftBucket(e.OldRow, -1);
			IncreaseDraftBucket(e.Row, 1);
			RemoveObsoleteLines();
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.costCodeID> e)
		{
			if (Project.Current != null)
			{
				if (Project.Current.BudgetLevel != BudgetLevels.CostCode)
				{
					e.NewValue = CostCodeAttribute.GetDefaultCostCode();
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.inventoryID> e)
		{
			e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.accountGroupID> e)
		{
			if (e.Row == null ) return;

			var select = new PXSelect<PMAccountGroup, Where<PMAccountGroup.type, Equal<GL.AccountType.income>>>(this);

			var resultset = select.SelectWindowed(0, 2);

			if (resultset.Count == 1)
			{
				e.NewValue = ((PMAccountGroup)resultset).GroupID;
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderRevenueBudget.inventoryID>(e.Cache, e.Row) as InventoryItem;
					if (item != null)
					{
						Account account = PXSelectorAttribute.Select<InventoryItem.salesAcctID>(Caches[typeof(InventoryItem)], item) as Account;
						if (account != null && account.AccountGroupID != null)
						{
							e.NewValue = account.AccountGroupID;
						}
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.projectTaskID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.accountGroupID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.uOM> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.rate>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderRevenueBudget.description>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.uOM> e)
		{
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.UOM;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.rate> e)
		{
			PMRevenueBudget budget = IsValidKey(e.Row) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row)) : null;
			if (budget != null)
			{
				e.NewValue = budget.CuryUnitRate;
			}
            else
            {
				PMProject project = PMProject.PK.Find(this, e.Row.ProjectID);
				e.NewValue = RateService.CalculateUnitPrice(e.Cache, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.InventoryID, e.Row.UOM, null, Document.Current?.Date, project?.CuryInfoID);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderRevenueBudget, PMChangeOrderRevenueBudget.description> e)
		{
			if (e.Row == null || Project.Current == null) return;

			PMRevenueBudget budget = GetOriginalRevenueBudget(BudgetKeyTuple.Create(e.Row));
					if (budget != null)
					{
						e.NewValue = budget.Description;
					}
					else
					{
				if (Project.Current.BudgetLevel == BudgetLevels.CostCode)
				{
					if (e.Row.CostCodeID != null)
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
				else if (Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					if (e.Row.ProjectTaskID != null)
					{
						PMTask projectTask = PXSelectorAttribute.Select<PMChangeOrderRevenueBudget.projectTaskID>(e.Cache, e.Row) as PMTask;
						if (projectTask != null)
						{
							e.NewValue = projectTask.Description;
						}
					}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
		}
		#endregion

		#region PMChangeOrderLine
		protected virtual void _(Events.RowUpdated<PMChangeOrderLine> e)
		{
			bool isReopen = false;

			if (e.Row.POOrderNbr != e.OldRow.POOrderNbr)
			{
				if (!string.IsNullOrEmpty(e.OldRow.POOrderNbr))
				{
					// POOrderNbr was changed from one value to another
					e.Row.POLineNbr = null;
					e.Row.VendorID = null;
					e.Row.CuryID = null;
					e.Row.AccountID = null;
				}

				POOrderPM poOrder = PXSelect<POOrderPM, Where<POOrderPM.orderType, Equal<Required<POOrderPM.orderType>>,
					And<POOrderPM.orderNbr, Equal<Required<POOrderPM.orderNbr>>>>>.Select(this, e.Row.POOrderType, e.Row.POOrderNbr);

				if (poOrder != null)
				{
					e.Row.VendorID = poOrder.VendorID;
					e.Row.CuryID = poOrder.CuryID;
					e.Row.AccountID = DefaultAccountID(e.Row);
				}

			}
			else if (e.Row.POLineNbr != e.OldRow.POLineNbr)
			{
				if (IsValidKey(e.Row))
				{
					POLinePM poLine = GetPOLine(e.Row);
					if (poLine != null)
					{
						e.Row.TaskID = poLine.TaskID;
						e.Row.UOM = poLine.UOM;
						e.Row.VendorID = poLine.VendorID;
						e.Row.CostCodeID = poLine.CostCodeID;
						e.Row.InventoryID = poLine.InventoryID;
						e.Row.UnitCost = poLine.CuryUnitCost;
						e.Row.CuryID = poLine.CuryID;
						e.Row.AccountID = poLine.ExpenseAcctID;

						e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);

						isReopen = poLine.Completed == true || poLine.Cancelled == true;
					}
				}
			}

			if (IsValidKey(e.Row))
			{
				e.Row.LineType = isReopen ? ChangeOrderLineType.Reopen : ChangeOrderLineType.Update;
			}
			else if (!string.IsNullOrEmpty(e.Row.POOrderNbr))
			{
				e.Row.LineType = ChangeOrderLineType.NewLine;
			}
			else
			{
				e.Row.LineType = ChangeOrderLineType.NewDocument;
			}
		}

		protected virtual void _(Events.RowSelected<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);

			if (e.Row != null)
			{
				bool referencesPOLine = e.Row.POLineNbr != null;
				bool referencesPOOrder = !string.IsNullOrEmpty(e.Row.POOrderNbr);

				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.taskID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.inventoryID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.costCodeID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.vendorID>(e.Cache, e.Row, !referencesPOOrder);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.curyID>(e.Cache, e.Row, !referencesPOOrder);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.uOM>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.accountID>(e.Cache, e.Row, !referencesPOLine);
				PXUIFieldAttribute.SetEnabled<PMChangeOrderLine.taxCategoryID>(e.Cache, e.Row, !referencesPOLine);
			}
		}

		protected virtual void _(Events.RowInserting<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);
		}

		protected virtual void _(Events.RowUpdating<PMChangeOrderLine> e)
		{
			InitDetailLineFields(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.accountID> e)
		{
			int? defaultValue = DefaultAccountID(e.Row);
			if (defaultValue != null)
			{
				e.NewValue = defaultValue;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.vendorID> e)
		{
			if (e.Row.AccountID == null
				|| PXAccess.FeatureInstalled<FeaturesSet.interBranch>()
				&& apSetup.Current.IntercompanyExpenseAccountDefault == APAcctSubDefault.MaskLocation
				&& Vendor.PK.Find(this, (int?)e.NewValue)?.IsBranch == true)
			{
				e.Cache.SetDefaultExt<PMChangeOrderLine.accountID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderLine.accountID>(e.Row);
			e.Cache.SetDefaultExt<PMChangeOrderLine.taxCategoryID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.costCodeID> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.accountID> e)
		{
			if (e.Row.AccountID != null)
				e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOOrderNbr> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.pOLineNbr> e)
		{
			e.Cache.SetDefaultExt<PMChangeOrderLine.description>(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderLine, PMChangeOrderLine.qty> e)
		{
			decimal? newValue = (decimal?)e.NewValue;
			if (newValue < 0)
			{
				if (!IsValidKey(e.Row))
				{
					throw new PXSetPropertyException(Messages.NewCommitmentQtyIsNegative);
				}
				else
				{
					POLinePM poLine = GetPOLine(e.Row);
					if (poLine != null && poLine.CalcOpenQty < Math.Abs(newValue.Value))
					{
						throw new PXSetPropertyException(Messages.CommitmentQtyCannotbeDecreased);
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderLine, PMChangeOrderLine.amount> e)
		{
			if (e.NewValue == null) return;
			decimal newValue = ((decimal?)e.NewValue).Value;

				if (!IsValidKey(e.Row))
				{
				if (newValue < 0 && e.Row.Qty != 0)
				{
					throw new PXSetPropertyException(Messages.CommitmentAmtIsNegative);
				}
				}
				else
				{
					POLinePM poLine = GetPOLine(e.Row);
				if (poLine != null)
				{
					var currentPoLineAmt = poLine.CuryLineAmt.Value;
					var newPoLineAmt = poLine.CuryLineAmt.Value + newValue;
					if (Math.Sign(newPoLineAmt) == Math.Sign(currentPoLineAmt) &&
						Math.Abs(newPoLineAmt) < Math.Abs(currentPoLineAmt) &&
						Math.Abs(poLine.CuryUnbilledAmt.GetValueOrDefault()) < Math.Abs(newValue))
					{
						string message = newPoLineAmt > 0 ?
							Messages.PositiveCommitmentAmtCannotBeDecreased :
							Messages.NegativeCommitmentAmtCannotBeDecreased;
						throw new PXSetPropertyException(message);
					}
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.amount> e)
		{
			e.Cache.SetValueExt<PMChangeOrderLine.amountInProjectCury>(e.Row, GetAmountInProjectCurrency(e.Row.CuryID, e.Row.Amount));
		}

		protected virtual void _(Events.FieldUpdated<PMChangeOrderLine, PMChangeOrderLine.curyID> e)
		{
			e.Cache.SetValueExt<PMChangeOrderLine.amountInProjectCury>(e.Row, GetAmountInProjectCurrency(e.Row.CuryID, e.Row.Amount));
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.description> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				if (e.Row.CostCodeID != null)
				{
					PMCostBudget budget = null;
					if (e.Row.TaskID != null && e.Row.AccountID != null)
					{
						BudgetKeyTuple key = new BudgetKeyTuple(e.Row.ProjectID.GetValueOrDefault(), e.Row.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(e.Row).GetValueOrDefault(), e.Row.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID), e.Row.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
						budget = GetOriginalCostBudget(key);
					}

					if (budget != null)
					{
						e.NewValue = budget.Description;
					}
					else
					{
						PMCostCode costCode = GetCostCode(e.Row.CostCodeID);
						if (costCode != null)
						{
							e.NewValue = costCode.Description;
						}
					}
				}
			}
			else
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = GetInventoryItem(e.Row.InventoryID);
					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMChangeOrderLine, PMChangeOrderLine.taxCategoryID> e)
		{
			if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				InventoryItem item = GetInventoryItem(e.Row.InventoryID);
				if (item != null && item.TaxCategoryID != null)
				{
					e.NewValue = item.TaxCategoryID;
				}
			}
		}
		#endregion

		protected virtual void _(Events.FieldUpdated<POLineFilter, POLineFilter.vendorID> e)
		{
			if (e.Row.VendorID != null)
			{
				e.Row.POOrderNbr = null;
			}
		}

		#endregion

		public override void Persist()
		{
			ResetToStandardIfLastReferenceWasDeleted();
			SetToChangeOrderIfReferenceAdded();

			//Clear lookup grids (updated only due to Selected field being changed); otherwise it will messup Budget Acumulator:
			AvailableCostBudget.Cache.Clear();
			AvailableRevenueBudget.Cache.Clear();

			base.Persist();
		}

		private void SetToChangeOrderIfReferenceAdded()
        {	
			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.POLineNbr != null)
				{
					SetPOOrderBehavior(line.POOrderType, line.POOrderNbr, POBehavior.ChangeOrder);
				}
			}
		}

		private void ResetToStandardIfLastReferenceWasDeleted()
        {
			string refNbr = null;
			HashSet<POLineKey> deletedRefs = new HashSet<POLineKey>();
			foreach (PMChangeOrderLine line in Details.Cache.Deleted)
					{
				if (line.POOrderType != null && line.POOrderNbr != null)
				{
					POLineKey key = new POLineKey(line.POOrderType, line.POOrderNbr, 0);
					deletedRefs.Add(key);
					refNbr = line.RefNbr;
				}
			}

			var selectOtherReferences = new PXSelect<PMChangeOrderLine,
				Where<PMChangeOrderLine.pOOrderType, Equal<Required<PMChangeOrderLine.pOOrderType>>,
				And<PMChangeOrderLine.pOOrderNbr, Equal<Required<PMChangeOrderLine.pOOrderNbr>>,
				And<PMChangeOrderLine.refNbr, NotEqual<Required<PMChangeOrderLine.refNbr>>>>>>(this);
			foreach (POLineKey deleted in deletedRefs)
			{
				PMChangeOrderLine otherRef = selectOtherReferences.SelectWindowed(0, 1, deleted.OrderType, deleted.OrderNbr, refNbr);
				if (otherRef == null)
				{
					SetPOOrderBehavior(deleted.OrderType, deleted.OrderNbr, POBehavior.Standard);
					}
				}
			}

		private void SetPOOrderBehavior(string orderType, string orderNbr, string behavior)
        {
			var selectOrder = new PXSelect<POOrder, Where<POOrder.orderType, Equal<Required<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>(this);

			POOrder order = selectOrder.Select(orderType, orderNbr);
			if (order != null && order.Behavior != null && order.Behavior != behavior)
			{
				order.Behavior = behavior;
				Order.Update(order);
			}
		}

		protected string draftChangeOrderBudgetStatsKey;
		protected Dictionary<BudgetKeyTuple, decimal> draftChangeOrderBudgetStats;
		public virtual Dictionary<BudgetKeyTuple, decimal> BuildBudgetStatsOnDraftChangeOrders()
		{
			Dictionary<BudgetKeyTuple, decimal> drafts = new Dictionary<BudgetKeyTuple, decimal>();

			var select = new PXSelectGroupBy<PMChangeOrderBudget, Where<PMChangeOrderBudget.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrderBudget.released, Equal<False>, 
				And<PMChangeOrderBudget.refNbr, NotEqual<Current<PMChangeOrder.refNbr>>>>>,
				Aggregate<GroupBy<PMChangeOrderBudget.projectID,
				GroupBy<PMChangeOrderBudget.projectTaskID,
				GroupBy<PMChangeOrderBudget.accountGroupID,
				GroupBy<PMChangeOrderBudget.inventoryID,
				GroupBy<PMChangeOrderBudget.costCodeID,
				Sum<PMChangeOrderBudget.amount>>>>>>>>(this);

			using (new PXFieldScope(select.View, typeof(PMChangeOrderBudget.projectID), typeof(PMChangeOrderBudget.projectTaskID)
				, typeof(PMChangeOrderBudget.accountGroupID), typeof(PMChangeOrderBudget.inventoryID), typeof(PMChangeOrderBudget.costCodeID)
				, typeof(PMChangeOrderBudget.amount), typeof(PMChangeOrderBudget.amount)))
			{
				foreach(PMChangeOrderBudget record in select.Select())
				{
					drafts.Add(BudgetKeyTuple.Create(record), record.Amount.GetValueOrDefault());
				}
			}

			return drafts;
		}

		public virtual decimal GetDraftChangeOrderBudgetAmount(PMChangeOrderBudget record)
		{
			if (!IsValidKey(record))
				return 0;


			if (draftChangeOrderBudgetStats == null || draftChangeOrderBudgetStatsKey != record.RefNbr)
			{
				draftChangeOrderBudgetStats = BuildBudgetStatsOnDraftChangeOrders();
				draftChangeOrderBudgetStatsKey = record.RefNbr;
			}
			
			decimal result = 0;
			draftChangeOrderBudgetStats.TryGetValue(BudgetKeyTuple.Create(record), out result);

			return result;
		}

		
		public virtual int? DefaultAccountID(PMChangeOrderLine line)
		{
			Vendor GetVendor() => PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Select(this, line.VendorID);

			int? accountID = null;
			Vendor vendor = null;

			if ((line?.LineType == ChangeOrderLineType.NewDocument 
					|| line?.LineType == ChangeOrderLineType.NewLine)
				&& line.InventoryID != null 
				&& line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID
				&& apSetup.Current.IntercompanyExpenseAccountDefault == APAcctSubDefault.MaskLocation
				&& (vendor = GetVendor()) != null
				&& vendor.IsBranch == true)
			{
				Location location = null;
				if (line?.LineType == ChangeOrderLineType.NewLine && !string.IsNullOrEmpty(line.POOrderNbr))
				{
					location = SelectFrom<Location>
						.InnerJoin<POOrder>
							.On<Location.locationID.IsEqual<POOrder.vendorLocationID>>
						.Where<POOrder.orderType.IsEqual<@P.AsString>
							.And<POOrder.orderNbr.IsEqual<@P.AsString>>>
						.View
						.SelectSingleBound(this, null, line.POOrderType, line.POOrderNbr);
				}
				if (line?.LineType == ChangeOrderLineType.NewDocument)
				{ 
					location = SelectFrom<Location>
						.Where<Location.locationID.IsEqual<@P.AsInt>>
						.View
						.SelectSingleBound(this, null, vendor.DefLocationID);
				}

				if (location != null)
				{
					if (PXSelectorAttribute.Select<Location.vExpenseAcctID>(Caches[typeof(Location)], location) is Account expenseAccount 
						&& expenseAccount.AccountGroupID != null)
					{
						accountID = expenseAccount.AccountID;
					}
				}
			}
			else
			{
				if (line.InventoryID != null && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelectorAttribute.Select<PMChangeOrderLine.inventoryID>(Details.Cache, line) as InventoryItem;
					if (item != null)
					{
						Account expenseAccount = PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Caches[typeof(InventoryItem)], item) as Account;
						if (expenseAccount != null && expenseAccount.AccountGroupID != null)
						{
							accountID = expenseAccount.AccountID;
						}
					}
				}

				if (accountID == null && line.VendorID != null)
				{
					vendor = vendor ?? GetVendor();
					if (vendor != null)
					{
						Location location = PXSelect<Location, Where<Location.locationID, Equal<Required<Location.locationID>>>>.Select(this, vendor.DefLocationID);
						if (location != null)
						{
							Account expenseAccount = PXSelectorAttribute.Select<Location.vExpenseAcctID>(Caches[typeof(Location)], location) as Account;
							if (expenseAccount != null && expenseAccount.AccountGroupID != null)
							{
								accountID = expenseAccount.AccountID;
							}
						}
					}
				}
			}
			return accountID;
		}

		private POLineKey GetKey(POLinePM record)
		{
			return new POLineKey(record.OrderType, record.OrderNbr, record.LineNbr.Value);
		}

		private POLineKey GetKey(PMChangeOrderLine record)
		{
			return new POLineKey(record.POOrderType, record.POOrderNbr, record.POLineNbr.Value);
		}

		private bool IsValidKey(PMChangeOrderBudget record)
		{
			if (record == null)
				return false;

			if (record.CostCodeID == null)
				return false;

			if (record.InventoryID == null)
				return false;

			if (record.AccountGroupID == null)
				return false;

			if (record.TaskID == null)
				return false;

			if (record.ProjectID == null)
				return false;

			return true;

		}

		private bool IsValidKey(PMChangeOrderLine record)
		{
			if(record == null)
				return false;

			if (record.POLineNbr == null)
				return false;

			if (string.IsNullOrEmpty(record.POOrderNbr))
				return false;

			if (string.IsNullOrEmpty(record.POOrderType))
				return false;

			return true;
		}
		
		protected Dictionary<BudgetKeyTuple, PMCostBudget> costBudgets;
		protected Dictionary<BudgetKeyTuple, PMChangeOrderBudget> previousTotals;
		public virtual Dictionary<BudgetKeyTuple, PMCostBudget> BuildCostBudgetLookup()
		{
			Dictionary<BudgetKeyTuple, PMCostBudget> result = new Dictionary<BudgetKeyTuple, PMCostBudget>();

			var select = new PXSelectReadonly<PMCostBudget, Where<PMCostBudget.projectID, Equal<Current<PMChangeOrder.projectID>>, And<PMCostBudget.type, Equal<GL.AccountType.expense>>>>(this);

			foreach(PMCostBudget record in select.Select())
			{
				var costCode = GetCostCode(record.CostCodeID);
				if (costCode == null || costCode.IsActive == true)
				{
					result.Add(BudgetKeyTuple.Create(record), record);
				}
			}

			return result;
		}

		protected Dictionary<BudgetKeyTuple, PMRevenueBudget> revenueBudgets;
		public virtual Dictionary<BudgetKeyTuple, PMRevenueBudget> BuildRevenueBudgetLookup()
		{
			Dictionary<BudgetKeyTuple, PMRevenueBudget> result = new Dictionary<BudgetKeyTuple, PMRevenueBudget>();

			var select = new PXSelectReadonly<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Current<PMChangeOrder.projectID>>, And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

			foreach (PMRevenueBudget record in select.Select())
			{
				var costCode = GetCostCode(record.CostCodeID);
				if (costCode == null || costCode.IsActive == true)
				{
					result.Add(BudgetKeyTuple.Create(record), record);
				}
			}

			return result;
		}
				
		public virtual Dictionary<BudgetKeyTuple, PMChangeOrderBudget> BuildPreviousTotals()
		{
			Dictionary<BudgetKeyTuple, PMChangeOrderBudget> result = new Dictionary<BudgetKeyTuple, PMChangeOrderBudget>();

			var select = new PXSelectJoinGroupBy<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderBudget.refNbr>>>,
				Where<PMChangeOrderBudget.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<PMChangeOrder.released, Equal<True>,
				And<PMChangeOrder.reverseStatus, Equal<ChangeOrderReverseStatus.none>>>>,
				Aggregate<GroupBy<PMChangeOrderBudget.projectID,
				GroupBy<PMChangeOrderBudget.projectTaskID,
				GroupBy<PMChangeOrderBudget.accountGroupID,
				GroupBy<PMChangeOrderBudget.inventoryID,
				GroupBy<PMChangeOrderBudget.costCodeID,
				Sum<PMChangeOrderBudget.qty,
				Sum<PMChangeOrderBudget.amount>>>>>>>>>(this);

			List<object> parameters = new List<object>();
			if (Document.Current != null)
			{
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted)
				{
					foreach (PMChangeOrder updated in Document.Cache.Updated)
					{
						if (updated.ReverseStatus == ChangeOrderReverseStatus.Reversed)
						{
							select.WhereAnd<Where<PMChangeOrderBudget.refNbr, NotEqual<Required<PMChangeOrderBudget.refNbr>>>>();
							parameters.Add(updated.RefNbr);
						}
					}
				}
				else
				{
					select.WhereAnd<Where<PMChangeOrderBudget.refNbr, Less<Current<PMChangeOrder.refNbr>>>>();
				}
			}


			foreach (PMChangeOrderBudget record in select.Select(parameters.ToArray()))
			{
				result.Add(BudgetKeyTuple.Create(record), record);
			}

			return result;
		}

		public virtual POLinePM GetPOLine(PMChangeOrderLine line)
		{
			POLinePM result = null;

			if (IsValidKey(line))
			{
				POLineKey key = GetKey(line);

				if (polines != null)
				{
					polines.TryGetValue(key, out result);
				}
				else
				{
					polines = new Dictionary<POLineKey, POLinePM>();
				}

				if (result == null)
				{
					result = PXSelect<POLinePM, Where<POLinePM.orderType, Equal<Required<POLinePM.orderType>>,
						And<POLinePM.orderNbr, Equal<Required<POLinePM.orderNbr>>,
						And<POLinePM.lineNbr, Equal<Required<POLinePM.lineNbr>>>>>>.Select(this, key.OrderType, key.OrderNbr, key.LineNbr);

					if (result != null)
					{
						polines.Add(key, result);
					}
				}
			}			

			return result;
		}

		public virtual PMCostBudget GetOriginalCostBudget(BudgetKeyTuple record)
		{
			if (costBudgets == null || IsCacheUpdateRequired())
			{
				costBudgets = BuildCostBudgetLookup();
			}

			PMCostBudget result = null;
			costBudgets.TryGetValue(record, out result);

			return result;
		}

		public virtual PMRevenueBudget GetOriginalRevenueBudget(BudgetKeyTuple record)
		{
			if (revenueBudgets == null || IsCacheUpdateRequired())
			{
				revenueBudgets = BuildRevenueBudgetLookup();
			}

			PMRevenueBudget result = null;
			revenueBudgets.TryGetValue(record, out result);

			return result;
		}

		public virtual PMChangeOrderBudget GetPreviousTotals(BudgetKeyTuple record)
		{
			if (previousTotals == null || IsCacheUpdateRequired())
			{
				previousTotals = BuildPreviousTotals();
			}

			PMChangeOrderBudget result = null;
			previousTotals.TryGetValue(record, out result);

			return result;
		}

		public virtual ICollection<PMCostBudget> GetCostBudget()
		{
			if (costBudgets == null || IsCacheUpdateRequired())
			{
				costBudgets = BuildCostBudgetLookup();
			}

			return costBudgets.Values;
		}

		public virtual ICollection<PMRevenueBudget> GetRevenueBudget()
		{
			if (revenueBudgets == null || IsCacheUpdateRequired())
			{
				revenueBudgets = BuildRevenueBudgetLookup();
			}

			return revenueBudgets.Values;
		}

		public virtual void ReleaseDocument(PMChangeOrder doc)
		{
			if (doc.Released == true)
				return;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
			PMProject project = PMProject.PK.Find(this, doc.ProjectID);
			if (project != null)
			{
				if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled || project.Status == ProjectStatus.Suspended)
				{
					var listAttribute = new ProjectStatus.ListAttribute();
					string status = listAttribute.ValueLabelDic[project.Status];
					throw new PXException(Messages.ReleaseCOProjectStatusError, status);
				}
			}

			ValidateOrdersTotal(doc);

			foreach (PMChangeOrderCostBudget costBudget in CostBudget.Select())
			{
				ApplyChangeOrderBudget(costBudget, doc);
				CostBudget.Cache.SetValue<PMChangeOrderCostBudget.released>(costBudget, true);
				CostBudget.Cache.MarkUpdated(costBudget, assertError: true);
			}

			foreach (PMChangeOrderRevenueBudget revenueBudget in RevenueBudget.Select())
			{
				ApplyChangeOrderBudget(revenueBudget, doc);
				RevenueBudget.Cache.SetValue<PMChangeOrderRevenueBudget.released>(revenueBudget, true);
				RevenueBudget.Cache.MarkUpdated(revenueBudget, assertError: true);
			}

			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = res;

				Details.Cache.SetValue<PMChangeOrderLine.released>(line, true);
				Details.Cache.MarkUpdated(line, assertError: true);
			}

			ReleaseLineChanges();

			doc.Released = true;
			Document.Update(doc);
			Save.Press();

				ts.Complete();
			}
		}
				
		public virtual void ValidateOrdersTotal(PMChangeOrder doc)
		{
			var ordersTotal = new Dictionary<Tuple<int?, string, string>, decimal>();

			var orderSelect = new SelectFrom<POOrder>
				.Where<POOrder.orderType.IsEqual<@P.AsString>
					.And<POOrder.orderNbr.IsEqual<@P.AsString>>>.View(this);

			var otherCOLines = new SelectFrom<PMChangeOrderLine>
				.InnerJoin<PMChangeOrder>.On<PMChangeOrderLine.refNbr.IsEqual<PMChangeOrder.refNbr>>
				.Where<PMChangeOrderLine.pOOrderType.IsEqual<@P.AsString>
					.And<PMChangeOrderLine.pOOrderNbr.IsEqual<@P.AsString>>
					.And<PMChangeOrder.status.IsEqual<ChangeOrderStatus.onHold>
						.Or<PMChangeOrder.status.IsEqual<ChangeOrderStatus.pendingApproval>>
						.Or<PMChangeOrder.status.IsEqual<ChangeOrderStatus.open>>>
					.And<PMChangeOrderLine.refNbr.IsNotEqual<@P.AsString>>>
				.AggregateTo<Sum<PMChangeOrderLine.amount>>.View(this);

			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = res;
				var orderKey = new Tuple<int?, string, string>(line.VendorID, line.POOrderType, line.POOrderNbr ?? string.Empty);
				if (!ordersTotal.ContainsKey(orderKey))
				{
					if (line.LineType == ChangeOrderLineType.NewDocument)
					{
						ordersTotal[orderKey] = 0;
					}
					else
					{
						using (new PXFieldScope(orderSelect.View, typeof(POOrder.orderType), typeof(POOrder.orderNbr), typeof(POOrder.curyOrderTotal)))
						{
							POOrder order = orderSelect.SelectSingle(line.POOrderType, line.POOrderNbr);
							ordersTotal[orderKey] = order.CuryOrderTotal ?? 0;
						}
						ordersTotal[orderKey] += otherCOLines.SelectSingle(line.POOrderType, line.POOrderNbr, doc.RefNbr).Amount ?? 0m;
					}
				}
				ordersTotal[orderKey] += line.Amount ?? 0;
			}

			foreach (var order in ordersTotal)
			{
				if (order.Value < 0)
				{
					if (order.Key.Item3 == string.Empty)
					{
						throw new PXException(Messages.NewCommitmentTotalIsNegative);
					}
					else
					{
						throw new PXException(order.Key.Item2 == POOrderType.RegularSubcontract ? Messages.CommitmentTotalIsNegativeSubcontract : Messages.CommitmentTotalIsNegativePurchaseOrder, order.Key.Item3);
					}
				}
			}
		}

		public virtual void ReverseDocument()
		{
			if (Document.Current == null)
				return;

			PMChangeOrder source = (PMChangeOrder) Document.Cache.CreateCopy(Document.Current);
			source.ReverseStatus = ChangeOrderReverseStatus.Reversed;
			Document.Update(source);
			
			List<PMChangeOrderRevenueBudget> revenueBudget = new List<PMChangeOrderRevenueBudget>();
			foreach(PMChangeOrderRevenueBudget budget in RevenueBudget.Select())
			{
				PMChangeOrderRevenueBudget sourceRevenueBudget = (PMChangeOrderRevenueBudget) RevenueBudget.Cache.CreateCopy(budget);
				revenueBudget.Add(sourceRevenueBudget);
			}

			List<PMChangeOrderCostBudget> costBudget = new List<PMChangeOrderCostBudget>();
			foreach (PMChangeOrderCostBudget budget in CostBudget.Select())
			{
				PMChangeOrderCostBudget sourceCostBudget = (PMChangeOrderCostBudget)CostBudget.Cache.CreateCopy(budget);
				costBudget.Add(sourceCostBudget);
			}

			List<PXResult<PMChangeOrderLine, POLinePM>> lines = new List<PXResult<PMChangeOrderLine, POLinePM>>();
			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = (PMChangeOrderLine)res;
				POLinePM poLine = (POLinePM)res;

				PMChangeOrderLine sourceLine = (PMChangeOrderLine)Details.Cache.CreateCopy(line);
				lines.Add(new PXResult<PMChangeOrderLine, POLinePM>(sourceLine, poLine));
			}

			source.OrigRefNbr = source.RefNbr;
			source.RefNbr = null;
			source.ExtRefNbr = null;
			source.ProjectNbr = Messages.NotAvailable; ;
			source.Released = false;
			source.ReverseStatus = ChangeOrderReverseStatus.Reversal;
			source.Hold = true;
			source.Approved = false;
			source.Status = ChangeOrderStatus.OnHold;
			source.LineCntr = 0;
			source.CommitmentTotal = 0;
			source.RevenueTotal = 0;
			source.CostTotal = 0;
			source.NoteID = Guid.NewGuid();

			PMChangeOrder target = Document.Insert(source);

			foreach (CSAnswers answer in Answers.Select())
			{
				CSAnswers dstanswer =
					Answers.Insert(new CSAnswers()
					{
						RefNoteID = target.NoteID,
						AttributeID = answer.AttributeID
					});
				if (dstanswer != null)
					dstanswer.Value = answer.Value;
			}

			foreach (PMChangeOrderRevenueBudget budget in revenueBudget)
			{
				budget.RefNbr = target.RefNbr;
				budget.Released = false;
				budget.Amount = -budget.Amount.GetValueOrDefault();
				budget.Qty = -budget.Qty.GetValueOrDefault();
				budget.NoteID = null;
				budget.IsDisabled = false;
				RevenueBudget.Insert(budget);
			}

			foreach (PMChangeOrderCostBudget budget in costBudget)
			{
				budget.RefNbr = target.RefNbr;
				budget.Released = false;
				budget.Amount = -budget.Amount.GetValueOrDefault();
				budget.Qty = -budget.Qty.GetValueOrDefault();
				budget.NoteID = null;
				budget.IsDisabled = false;
				CostBudget.Insert(budget);
			}

			foreach (PXResult<PMChangeOrderLine, POLinePM> res in lines)
			{
				PMChangeOrderLine line = (PMChangeOrderLine)res;
				POLinePM poLine = (POLinePM)res;
				line.LineType = ChangeOrderLineType.Update;
				line.RefNbr = target.RefNbr;
				line.LineNbr = null;
				line.Released = false;
				line.Amount = -Math.Min( line.Amount.GetValueOrDefault(), poLine.CalcCuryOpenAmt.GetValueOrDefault() );
				line.Qty = - Math.Min( line.Qty.GetValueOrDefault(), poLine.CalcOpenQty.GetValueOrDefault() );
				line.AmountInProjectCury = null;
				line.RetainageAmt = null;
				line.RetainageAmtInProjectCury = null;
				line.NoteID = null;
				Details.Insert(line);
			}
		}

		public virtual void ReleaseLineChanges()
		{
			//Existing orders:
			Dictionary<string, POOrder> orders = new Dictionary<string, POOrder>();
			Dictionary<string, List<PMChangeOrderLine>> updated = new Dictionary<string, List<PMChangeOrderLine>>();
			Dictionary<string, List<PMChangeOrderLine>> added = new Dictionary<string, List<PMChangeOrderLine>>();

			//New orders:
			SortedList<string, POOrder> newOrders = new SortedList<string, POOrder>();
			SortedList<string, List<PMChangeOrderLine>> newLines = new SortedList<string, List<PMChangeOrderLine>>();
			
			foreach (PXResult<PMChangeOrderLine, POLinePM> res in Details.Select())
			{
				PMChangeOrderLine line = res;
				POLinePM poLine = res;

				POOrder order = CreatePOOrderFromChangedLine(line);
				string key = CreatePOOrderKey(order);

				if (string.IsNullOrEmpty(order.OrderNbr))
				{					
					if (!newOrders.ContainsKey(key))
					{
						newOrders.Add(key, order);
						newLines.Add(key, new List<PMChangeOrderLine>());
					}

					newLines[key].Add(line);
				}
				else
				{
					if (!orders.ContainsKey(key))
					{
						orders.Add(key, order);
						updated.Add(key, new List<PMChangeOrderLine>());
						added.Add(key, new List<PMChangeOrderLine>());
					}

					if (poLine.LineNbr != null)
					{
						updated[key].Add(line);
					}
					else
					{
						added[key].Add(line);
					}
				}
			}

			foreach(KeyValuePair<string, POOrder> kv in orders)
			{
				ModifyExistingOrder(kv.Value, updated[kv.Key], added[kv.Key]);
			}

			foreach (KeyValuePair<string, POOrder> kv in newOrders)
			{
				POOrder newOrder = CreateNewOrder(kv.Value, newLines[kv.Key]);

				foreach (PMChangeOrderLine line in newLines[kv.Key])
				{
					SetReferences(line, newOrder);
				}
			}
		}
		
		public virtual void ModifyExistingOrder(POOrder order, List<PMChangeOrderLine> updated, List<PMChangeOrderLine> added)
		{
			POOrderEntry target = CreateTarget(order);
			target.Document.Current = target.Document.Search<POOrder.orderNbr>(order.OrderNbr, order.OrderType);
			target.GetExtension<POOrderEntryExt>().SkipProjectLockCommitmentsVerification = true;
			
			if (updated.Count > 0)
				target.Transactions.Select().Consume();

			target.FieldUpdated.RemoveHandler(typeof(POOrder), typeof(POOrder.cancelled).Name, target.POOrder_Cancelled_FieldUpdated);

			target.Document.Current.LockCommitment = true;

			POSetup poSetup = target.POSetup.Current;
			poSetup.RequireOrderControlTotal = false;
			poSetup.RequireBlanketControlTotal = false;
			poSetup.RequireDropShipControlTotal = false;
			poSetup.RequireProjectDropShipControlTotal = false;

			target.Document.Update(target.Document.Current);

			var orderLines = new List<POLine>(updated.Count + added.Count);

			foreach (PMChangeOrderLine line in updated)
			{
				orderLines.Add(ModifyExistsingLineInOrder(target, line));
			}

			foreach (PMChangeOrderLine line in added)
			{
				orderLines.Add(AddNewLineToOrder(target, line));
			}

			bool hasNonZeroRetainageValues = orderLines.Any(x => x.RetainagePct.GetValueOrDefault() != 0);

			if (hasNonZeroRetainageValues)
			{
				target.Document.Current.RetainageApply = true;
			}

			POOrder.Events.Select(ev => ev.ReleaseChangeOrder).FireOn(target, target.Document.Current);
			target.Save.Press();
		}

		protected virtual POLine ModifyExistsingLineInOrder(POOrderEntry target, PMChangeOrderLine line)
		{
			POLine key = new POLine() { OrderType = line.POOrderType, OrderNbr = line.POOrderNbr, LineNbr = line.POLineNbr };
			POLine poLine = (POLine)target.Transactions.Cache.Locate(key);

			if (poLine.OrigExtCost == null)
			{
				poLine.OrigOrderQty = poLine.OrderQty;
				poLine.OrigExtCost = poLine.CuryLineAmt;
			}

			decimal curyLineAmt = poLine.CuryLineAmt.GetValueOrDefault() + line.Amount.GetValueOrDefault();

			bool wasCancelledOrCompleted = poLine.Cancelled == true || poLine.Completed == true;
			poLine.ManualPrice = true;
			poLine.Cancelled = false;
			poLine.Completed = false;

			poLine = target.Transactions.Update(poLine);

			if (poLine.OrderQty + line.Qty.GetValueOrDefault() == 0 && poLine.OrderQty > 0)
			{
				poLine.Cancelled = true;
			}
			else
			{
				poLine.OrderQty += line.Qty.GetValueOrDefault();
			}

			poLine.CuryUnitCost = line.UnitCost;

			poLine = target.Transactions.Update(poLine);

			poLine.CuryLineAmt = curyLineAmt;

			poLine = target.Transactions.Update(poLine);

			if (wasCancelledOrCompleted && poLine.Cancelled != true && poLine.Completed != true)
			{
				POOrder.Events.Select(ev => ev.LinesReopened).FireOn(target, target.Document.Current);
			}

			return poLine;
		}

		protected virtual POLine AddNewLineToOrder(POOrderEntry target, PMChangeOrderLine line)
		{
			POLine poLine = CreatePOLineFromChangeOrderLine(line);
			decimal curyLineAmt = poLine.CuryLineAmt.GetValueOrDefault();
			poLine.CuryLineAmt = null;
			poLine = target.Transactions.Insert(poLine);

			Details.Cache.SetValue<PMChangeOrderLine.pOLineNbr>(line, poLine.LineNbr);
			poLine.CuryLineAmt = curyLineAmt;

			return target.Transactions.Update(poLine);
		}

		public virtual POOrder CreateNewOrder(POOrder order, List<PMChangeOrderLine> added)
		{
			POOrderEntry target = CreateTarget(order);
			target.GetExtension<POOrderEntryExt>().SkipProjectLockCommitmentsVerification = true;

			var newOrder = target.Document.Insert(order);			
			target.Document.SetValueExt<POOrder.curyID>(newOrder, order.CuryID);

			bool hasNonZeroRetainageValues = false;

			foreach (PMChangeOrderLine line in added)
			{
				POLine source = CreatePOLineFromChangeOrderLine(line);
				source.CuryUnitCost = null;
				source.CuryLineAmt = null;

				POLine poline = target.Transactions.Insert(source);
				poline.CuryUnitCost = line.UnitCost;
				poline.CuryLineAmt = line.Amount;
				if (line.Amount < 0)
				{
					poline.CuryDiscAmt = 0;
					poline.DiscAmt = 0;
					poline.DiscPct = 0;
				}

				if (line.RetainageAmt.GetValueOrDefault() != 0)
				{
					hasNonZeroRetainageValues = true;
				}

				target.Transactions.Update(poline);

				Details.Cache.SetValue<PMChangeOrderLine.pOLineNbr>(line, poline.LineNbr);
			}

			if (hasNonZeroRetainageValues)
			{
				target.Document.Current.RetainageApply = true;
			}

			if (target.GetRequireControlTotal(newOrder.OrderType) && newOrder.CuryControlTotal != newOrder.CuryOrderTotal)
			{
				target.Document.SetValueExt<POOrder.curyControlTotal>(newOrder, newOrder.CuryOrderTotal);
			}

			newOrder.LockCommitment = true;
			newOrder.Approved = true;

			if (newOrder.Hold == true)
				target.releaseFromHold.Press();
			else
				target.Save.Press();

			return newOrder;
		}

		public virtual void SetReferences(PMChangeOrderLine line, POOrder newOrder)
		{
			Details.Cache.SetValue<PMChangeOrderLine.pOOrderType>(line, newOrder.OrderType);
			Details.Cache.SetValue<PMChangeOrderLine.pOOrderNbr>(line, newOrder.OrderNbr);
		}

		public virtual string CreatePOOrderKey(POOrder order)
		{
			return string.Format("{0}.{1}.{2}.{3}", order.VendorID, order.OrderType, order.OrderNbr, order.CuryID);
		}

		public virtual POOrder CreatePOOrderFromChangedLine(PMChangeOrderLine line)
		{
			POOrder order = new POOrder();
			order.OrderType = line.POOrderType;
			order.OrderNbr = line.POOrderNbr;
			order.VendorID = line.VendorID;
			order.PayToVendorID = line.VendorID;
			order.Behavior = POBehavior.ChangeOrder;
			order.OrderDesc = PXMessages.LocalizeFormatNoPrefix(Messages.ChangeOrderPrefix, line.RefNbr);
			order.ProjectID = line.ProjectID;
			order.CuryID = line.CuryID;
			return order;
		}

		public virtual POLine CreatePOLineFromChangeOrderLine(PMChangeOrderLine line)
		{
			POLine poLine = new POLine();
			poLine.InventoryID = line.InventoryID;
			poLine.SubItemID = line.SubItemID;
			poLine.TranDesc = line.Description;
			poLine.UOM = line.UOM;
			poLine.OrigOrderQty = 0m;
			poLine.OrigExtCost = 0m;
			poLine.OrderQty = line.Qty;
			poLine.CuryUnitCost = line.UnitCost;
			poLine.CuryLineAmt = line.Amount;
			poLine.RetainagePct = line.RetainagePct;
			poLine.CuryRetainageAmt = line.RetainageAmt;
			poLine.ExpenseAcctID = line.AccountID;
			poLine.ProjectID = line.ProjectID;
			poLine.TaskID = line.TaskID;
			poLine.CostCodeID = line.CostCodeID;
			poLine.TranDesc = line.Description;
			poLine.ManualPrice = true;
			poLine.TaxCategoryID = line.TaxCategoryID;
			return poLine;
		}

		public virtual int? GetProjectedAccountGroup(PMChangeOrderLine line)
		{
			Account revenueAccount = PXSelectorAttribute.Select<PMChangeOrderLine.accountID>(Details.Cache, line, line.AccountID) as Account;
			if (revenueAccount != null)
			{
				return revenueAccount.AccountGroupID;
			}
			return null;
		}

		public virtual POOrderEntry CreateTarget(POOrder order)
		{
			if (order.OrderType == POOrderType.RegularSubcontract)
				return PXGraph.CreateInstance<CN.Subcontracts.SC.Graphs.SubcontractEntry>();
			return PXGraph.CreateInstance<POOrderEntry>();
		}
				
		public virtual void AddSelectedCostBudget()
		{
			foreach( PMCostBudget budget in AvailableCostBudget.Cache.Updated )
			{
				if (budget.Type != GL.AccountType.Expense || budget.Selected != true) continue;

				PMChangeOrderCostBudget key = new PMChangeOrderCostBudget() { ProjectID = budget.ProjectID, ProjectTaskID = budget.ProjectTaskID, AccountGroupID = budget.AccountGroupID, InventoryID = budget.InventoryID, CostCodeID = budget.CostCodeID };

				if (CostBudget.Locate(key) == null)
				{
					CostBudget.Insert(key);
				}
			}
		}

		public virtual void AddSelectedRevenueBudget()
		{
			foreach (PMRevenueBudget budget in AvailableRevenueBudget.Cache.Updated)
			{
				if (budget.Type != GL.AccountType.Income || budget.Selected != true) continue;

				PMChangeOrderRevenueBudget key = new PMChangeOrderRevenueBudget() { ProjectID = budget.ProjectID, ProjectTaskID = budget.ProjectTaskID, AccountGroupID = budget.AccountGroupID, InventoryID = budget.InventoryID, CostCodeID = budget.CostCodeID };

				if (RevenueBudget.Locate(key) == null)
				{
					RevenueBudget.Insert(key);
				}
			}
		}

		public virtual void AddSelectedPOLines()
		{
			HashSet<POLineKey> existing = new HashSet<POLineKey>();

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.POLineNbr != null)
				{
					existing.Add(new POLineKey(line.POOrderType, line.POOrderNbr, line.POLineNbr.Value));
				}
			}
						
			foreach (POLinePM selected in AvailablePOLines.Cache.Updated)
			{
				if (selected.Selected != true)
					continue;

				POLineKey key = new POLineKey(selected.OrderType, selected.OrderNbr, selected.LineNbr.Value);

				if (existing.Contains(key))
					continue;
				
				Details.Insert(CreateChangeOrderLine(selected));
			}
		}

		public virtual PMChangeOrderLine CreateChangeOrderLine(POLinePM poLine)
		{
			PMChangeOrderLine line = new PMChangeOrderLine();
			line.POOrderType = poLine.OrderType;
			line.POOrderNbr = poLine.OrderNbr;
			line.POLineNbr = poLine.LineNbr;
			line.TaskID = poLine.TaskID;
			line.UOM = poLine.UOM;
			line.UnitCost = poLine.CuryUnitCost;
			line.VendorID = poLine.VendorID;
			line.CostCodeID = poLine.CostCodeID;
			line.CuryID = poLine.CuryID;
			line.AccountID = poLine.ExpenseAcctID;
			line.LineType = poLine.Completed == true || poLine.Cancelled == true ? ChangeOrderLineType.Reopen : ChangeOrderLineType.Update;
			line.InventoryID = poLine.InventoryID;
			line.SubItemID = poLine.SubItemID;
			line.TaxCategoryID = poLine.TaxCategoryID;
			return line;
		}

		public virtual void InitCostBudgetFields(PMChangeOrderCostBudget record)
		{
			if (record != null)
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalCostBudget(BudgetKeyTuple.Create(record)) : null;
				InitBudgetFields(record, budget);
			}
		}

		public virtual void InitRevenueBudgetFields(PMChangeOrderRevenueBudget record)
		{
			if (record != null)
			{
				PMBudget budget = IsValidKey(record) ? GetOriginalRevenueBudget(BudgetKeyTuple.Create(record)) : null;
				InitBudgetFields(record, budget);
			}
		}

		public virtual void InitBudgetFields(PMChangeOrderBudget record, PMBudget budget)
		{
			if (record != null)
			{
				var recordKey = BudgetKeyTuple.Create(record);

				record.OtherDraftRevisedAmount = GetDraftChangeOrderBudgetAmount(record);
				record.RevisedAmount = record.Amount.GetValueOrDefault();
				
				if (IsValidKey(record))
					record.CommittedCOAmount = GetCurrentCommittedCOAmount(recordKey);

				PMChangeOrderBudget previousTotals = GetPreviousTotals(recordKey);

				if (previousTotals != null)
				{
					record.PreviouslyApprovedAmount = previousTotals.Amount.GetValueOrDefault();
					record.PreviouslyApprovedQty = previousTotals.Qty.GetValueOrDefault();
				}
				else
				{
					record.PreviouslyApprovedAmount = 0;
					record.PreviouslyApprovedQty = 0;
				}

				if (budget != null)
				{
					record.RevisedAmount = budget.CuryAmount.GetValueOrDefault() + record.PreviouslyApprovedAmount.GetValueOrDefault() + record.Amount.GetValueOrDefault();
					record.RevisedQty = budget.Qty.GetValueOrDefault() + record.PreviouslyApprovedQty.GetValueOrDefault() + record.Qty.GetValueOrDefault();

					if (IsValidKey(record))
						record.CommittedCOQty = GetCurrentCommittedCOQty(BudgetKeyTuple.Create(budget), budget);

					record.TotalPotentialRevisedAmount = budget.CuryRevisedAmount.GetValueOrDefault() + record.OtherDraftRevisedAmount.GetValueOrDefault();

					if (record.Released != true)
					{
						record.TotalPotentialRevisedAmount += record.Amount.GetValueOrDefault();
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(record.UOM) )
					{
						record.RevisedQty = record.Qty;
						if (IsValidKey(record))
							record.CommittedCOQty = GetCurrentCommittedCOQty(recordKey, record);
					}
				}
			}
		}

		public virtual decimal GetCurrentCommittedCOAmount(BudgetKeyTuple key)
		{
			decimal amount = 0;

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) == key.CostCodeID && line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) == key.InventoryID && line.TaskID == key.ProjectTaskID && GetProjectedAccountGroup(line) == key.AccountGroupID)
				{
					amount += line.AmountInProjectCury.GetValueOrDefault();
				}
			}

			return amount;
		}

		public virtual decimal GetCurrentCommittedCOQty(BudgetKeyTuple key, IQuantify budget)
		{
			decimal qty = 0;

			foreach (PMChangeOrderLine line in Details.Select())
			{
				if (line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()) == key.CostCodeID && line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) == key.InventoryID && line.TaskID == key.ProjectTaskID && GetProjectedAccountGroup(line) == key.AccountGroupID)
				{
					var rollupQty = BalanceCalculator.CalculateRollupQty(line, budget);
					if (rollupQty != 0)
					{
						qty += rollupQty;
					}
				}
			}

			return qty;
		}
		
		public virtual void InitDetailLineFields(PMChangeOrderLine line)
		{
			if (line != null)
			{
				if (line.Released != true)
				{
					line.PotentialRevisedAmount = line.Amount.GetValueOrDefault();
					line.PotentialRevisedQty = line.Qty.GetValueOrDefault();
				}
				else
				{
					line.PotentialRevisedAmount = 0;
					line.PotentialRevisedQty = 0;
				}

				POLinePM poLine = GetPOLine(line);
				if (poLine != null)
				{
					line.PotentialRevisedAmount += poLine.CuryLineAmt.GetValueOrDefault();
					line.PotentialRevisedQty += poLine.OrderQty.GetValueOrDefault();
				}
			}
		}

		public virtual bool PrepaymentVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.PrepaymentEnabled == true;
			}
			return false;
		}

		public virtual bool LimitsVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.LimitsEnabled == true;
			}
			return false;
		}

		public virtual bool ProductivityVisible()
		{
			if (Project.Current != null)
			{
				return Project.Current.BudgetMetricsEnabled == true;
			}
			return false;
		}

		public virtual bool CanEditDocument(PMChangeOrder doc)
		{
			if (doc == null)
				return true;

			if (doc.Released == true)
				return false;

			if (doc.Hold == true)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public decimal GetAmountInProjectCurrency(string fromCuryID, decimal? value)
		{
			return MultiCurrencyService.GetValueInProjectCurrency(this, Project.Current, fromCuryID, Document.Current.Date, value);
		}

		public virtual bool IsProjectEnabled()
		{
			if (CostBudget.Cache.IsInsertedUpdatedDeleted)
				return false;
			if (RevenueBudget.Cache.IsInsertedUpdatedDeleted)
				return false;
			if (Details.Cache.IsInsertedUpdatedDeleted)
				return false;

			if (CostBudget.Select().Count > 0)
				return false;
			
			if (RevenueBudget.Select().Count > 0)
				return false;

			if (Details.Select().Count > 0)
				return false;

			return true;
		}

		public virtual void IncreaseDraftBucket(int mult)
		{
			foreach (PMChangeOrderCostBudget costBudget in CostBudget.Select())
			{
				IncreaseDraftBucket(costBudget, mult);
			}

			foreach (PMChangeOrderRevenueBudget revenueBudget in RevenueBudget.Select())
			{
				IncreaseDraftBucket(revenueBudget, mult);
			}
		}

		public virtual void IncreaseDraftBucket(PMChangeOrderBudget row, int mult)
		{
			if (row.ProjectID == null) return;
			if (row.ProjectTaskID == null) return;
			if (row.AccountGroupID == null) return;
			if (row.InventoryID == null) return;
			if (row.CostCodeID == null) return;

			PMAccountGroup ag = PMAccountGroup.PK.Find(this, row.AccountGroupID);
			
			BudgetService budgetService = new BudgetService(this);
			Lite.PMBudget target = budgetService.SelectProjectBalance(row, ag, Project.Current, out bool isExisting);

			PMBudgetAccum budget = Budget.Insert(new PMBudgetAccum
			{
				ProjectID = row.ProjectID,
				ProjectTaskID = row.ProjectTaskID,
				AccountGroupID = row.AccountGroupID,
				InventoryID = target.InventoryID,
				CostCodeID = target.CostCodeID,
				CuryInfoID = Project.Current.CuryInfoID
			});

			budget.UOM = target.UOM;
			budget.Description = row.Description;
			budget.Type = target.Type;
			budget.CuryUnitRate = row.Rate;
			budget.RetainagePct = Project.Current.RetainagePct;

			decimal rollupQty = BalanceCalculator.CalculateRollupQty(row, budget);

			budget.CuryDraftChangeOrderAmount += mult * row.Amount.GetValueOrDefault();
			budget.DraftChangeOrderQty += mult * rollupQty;

			if (Document.Current != null)
			{
				FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(Document.Current.Date, FinPeriod.organizationID.MasterValue);

				if (finPeriod != null)
				{
					PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
					forecast.ProjectID = target.ProjectID;
					forecast.ProjectTaskID = target.ProjectTaskID;
					forecast.AccountGroupID = target.AccountGroupID;
					forecast.InventoryID = target.InventoryID;
					forecast.CostCodeID = target.CostCodeID;
					forecast.PeriodID = finPeriod.FinPeriodID;

					forecast = ForecastHistory.Insert(forecast);

					forecast.DraftChangeOrderQty += mult * rollupQty;
					forecast.CuryDraftChangeOrderAmount += mult * row.Amount.GetValueOrDefault();
				}
			}
		}

		public virtual void RemoveObsoleteLines()
		{
			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.CuryDraftChangeOrderAmount.GetValueOrDefault() == 0 && item.DraftChangeOrderQty.GetValueOrDefault() == 0)
				{
					Budget.Cache.Remove(item);
				}
			}

			foreach (PMForecastHistoryAccum item in ForecastHistory.Cache.Inserted)
			{
				if (item.CuryDraftChangeOrderAmount.GetValueOrDefault() == 0 && item.DraftChangeOrderQty.GetValueOrDefault() == 0)
				{
					ForecastHistory.Cache.Remove(item);
				}
			}
		}

		public virtual void ApplyChangeOrderBudget(PMChangeOrderBudget row, PMChangeOrder order)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(this, row.AccountGroupID);
			
			Func<decimal, (decimal, decimal)> calcQuantityDelta = null;

			BudgetService budgetService = new BudgetService(this);
			Lite.PMBudget target = budgetService.SelectProjectBalance(row, ag, Project.Current, out bool isExisting);

			PMBudgetAccum budget = new PMBudgetAccum
			{
				ProjectID = target.ProjectID,
				ProjectTaskID = target.ProjectTaskID,
				AccountGroupID = target.AccountGroupID,
				InventoryID = target.InventoryID,
				CostCodeID = target.CostCodeID,
				UOM = target.UOM,
				Description = target.Description,
				Type = target.Type,
				CuryInfoID = Project.Current.CuryInfoID
			};

			decimal amountToInvoiceDelta = 0;
			if (!isExisting)
			{
				budget.CuryUnitRate = row.Rate;
				budget.UOM = row.UOM ?? target.UOM;
				budget.Description = row.Description ?? target.Description;
			}
			else
			{
				//The code block below is used to calculate/update the PendingInvoiceAmount based on 
				//current Completed % and CO. The following code works under assumption that there is no
				//concurrent modification of the underline budget line.
				//This pattern is to be reviewed

				if (budget.Type == GL.AccountType.Income)
				{
					PMRevenueBudget revenue = GetOriginalRevenueBudget(BudgetKeyTuple.Create(target));
					if (revenue != null)
					{
						if (revenue.ProgressBillingBase == ProgressBillingBase.Amount)
						{
							decimal pendingCuryRevisedAmount = revenue.CuryRevisedAmount.GetValueOrDefault() + row.Amount.GetValueOrDefault();
							decimal invoicedOrPendingPrepayment = revenue.CuryActualAmount.GetValueOrDefault() + revenue.CuryInclTaxAmount.GetValueOrDefault() + revenue.CuryInvoicedAmount.GetValueOrDefault() + (revenue.CuryPrepaymentAmount.GetValueOrDefault() - revenue.CuryPrepaymentInvoiced.GetValueOrDefault());
							decimal amountToInvoice = (pendingCuryRevisedAmount * revenue.CompletedPct.GetValueOrDefault() / 100m) - invoicedOrPendingPrepayment;

							amountToInvoiceDelta = amountToInvoice - revenue.CuryAmountToInvoice.GetValueOrDefault();
						}
						else if(revenue.ProgressBillingBase == ProgressBillingBase.Quantity)
						{
							calcQuantityDelta = (rollupQty) =>
							{
								decimal qtyDelta = (revenue.CompletedPct.GetValueOrDefault() / 100.0m * rollupQty) - (revenue.InvoicedQty.GetValueOrDefault() + revenue.ActualQty.GetValueOrDefault());
								qtyDelta = decimal.Round(qtyDelta, CommonSetupDecPl.Qty);
								decimal amtDelta = qtyDelta * revenue.CuryUnitRate.GetValueOrDefault();
								return (qtyDelta, amtDelta);
							};
						}
					}
				}
			}

			budget = Budget.Insert(budget);
			decimal rollupQty = BalanceCalculator.CalculateRollupQty(row, budget);

			if(calcQuantityDelta != null)
			{
				var delta = calcQuantityDelta.Invoke(rollupQty);
				budget.QtyToInvoice += delta.Item1;
				amountToInvoiceDelta = delta.Item2;
			}

			budget.DraftChangeOrderQty -= rollupQty;
			budget.ChangeOrderQty += rollupQty;
			budget.RevisedQty += rollupQty;
			budget.CuryDraftChangeOrderAmount -= row.Amount.GetValueOrDefault();
			budget.CuryChangeOrderAmount += row.Amount.GetValueOrDefault();
			budget.CuryRevisedAmount += row.Amount.GetValueOrDefault();
			budget.CuryAmountToInvoice += amountToInvoiceDelta;
			

			FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(order.Date, FinPeriod.organizationID.MasterValue);

			if (finPeriod != null)
			{
				PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
				forecast.ProjectID = target.ProjectID;
				forecast.ProjectTaskID = target.ProjectTaskID;
				forecast.AccountGroupID = target.AccountGroupID;
				forecast.InventoryID = target.InventoryID;
				forecast.CostCodeID = target.CostCodeID;
				forecast.PeriodID = finPeriod.FinPeriodID;

				forecast = ForecastHistory.Insert(forecast);

				forecast.DraftChangeOrderQty -= rollupQty;
				forecast.CuryDraftChangeOrderAmount -= row.Amount.GetValueOrDefault();
				forecast.ChangeOrderQty += rollupQty;
				forecast.CuryChangeOrderAmount += row.Amount.GetValueOrDefault();
			}
		}

		private PMProject GetProject(int? projectID)
		{
			PMProject project = PMProject.PK.Find(this, projectID);
			return project;
		}

		private PMCostCode GetCostCode(int? costCodeID)
		{
			PMCostCode costCode = PMCostCode.PK.Find(this, costCodeID);
			return costCode;
		}

		private InventoryItem GetInventoryItem(int? inventoryID)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);
			return item;
		}

		private string LastRefNbr = string.Empty;
		/// <summary>
		/// You must invalidate local cache, when document key changes (next, previous, last, first actions).
		/// </summary>
		private bool IsCacheUpdateRequired()
		{
			if (Document.Current == null)
				return false;

			string currentRefNbr = Document.Current.RefNbr;
			bool isRequired = currentRefNbr != LastRefNbr;
			if (isRequired)
			{
				ClearLocalCache();
				LastRefNbr = currentRefNbr;				
			}
			return isRequired;
		}

		public override void Clear()
		{
			base.Clear();
			ClearLocalCache();
		}

		private void ClearLocalCache()
		{
			costBudgets = null;
			revenueBudgets = null;
			polines = null;
			previousTotals = null;
		}

		private IEnumerable ViewChangeOrderActionImplementation(PXAdapter adapter, string refNbr, string message)
		{
			if ( string.IsNullOrWhiteSpace(refNbr))
				return adapter.Get();

			ChangeOrderEntry target = PXGraph.CreateInstance<ChangeOrderEntry>();
			target.Clear(PXClearOption.ClearAll);
			target.SelectTimeStamp();
			target.Document.Current = PMChangeOrder.PK.Find(this, refNbr);
			throw new PXRedirectRequiredException(target, true, message) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		private string[] GetReversingOrderRefs()
			=> ReversingChangeOrders.Select()
						   	  .RowCast<ReversingChangeOrder>()
							  .Select(x => x.RefNbr)
				              .ToArray();

		#region Local Types

		[PXHidden]
		[Serializable]
		public class POLineFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[POVendor()]
			public virtual Int32? VendorID
			{
				get;
				set;
			}
			#endregion
			#region VendorRefNbr
			public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr> { }
			
			[PXDBString(40)]
			[PXUIField(DisplayName = "Vendor Ref.", Enabled = false)]
			public virtual String VendorRefNbr
			{
				get;
				set;
			}
			#endregion
			#region POOrderNbr
			public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }

			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "PO Nbr.")]
			[PXSelector(typeof(Search4<POLine.orderNbr, Where<POLine.orderType, In3<POOrderType.regularOrder, POOrderType.projectDropShip>,
				And<POLine.projectID, Equal<Current<PMChangeOrder.projectID>>,
				And<POLine.cancelled, Equal<False>,
				And<POLine.completed, Equal<False>,
				And<Where<Current<vendorID>, IsNull, Or<POLine.vendorID, Equal<Current<vendorID>>>>>>>>>,
				Aggregate<GroupBy<POLine.orderType, GroupBy<POLine.orderNbr, GroupBy<POLine.vendorID>>>>>),
				typeof(POLine.orderType), typeof(POLine.orderNbr), typeof(POLine.vendorID))]
			public virtual String POOrderNbr
			{
				get;
				set;
			}
			#endregion
			#region CostCodeFrom
			public abstract class costCodeFrom : PX.Data.BQL.BqlString.Field<costCodeFrom> { }

			[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(Search<PMCostCode.costCodeCD>))]
			[PXDBString(IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Cost Code From", FieldClass = CostCodeAttribute.COSTCODE)]
			public virtual String CostCodeFrom
			{
				get;
				set;
			}
			#endregion
			#region CostCodeTo
			public abstract class costCodeTo : PX.Data.BQL.BqlString.Field<costCodeTo> { }

			[PXDimensionSelector(PMCostCode.costCodeCD.DimensionName, typeof(PMCostCode.costCodeCD))]
			[PXDBString(IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Cost Code To", FieldClass = CostCodeAttribute.COSTCODE)]
			public virtual String CostCodeTo
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[Inventory(Filterable = true)]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region Include Non-open Commitments 
			public abstract class includeNonOpen : PX.Data.BQL.BqlBool.Field<includeNonOpen> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Include Non-Open Commitments")]
			public virtual Boolean? IncludeNonOpen
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		[Serializable]
		public class CostBudgetFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true, DirtyRead = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region GroupByTask
			public abstract class groupByTask : PX.Data.BQL.BqlBool.Field<groupByTask> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Group by Task")]
			public virtual Boolean? GroupByTask
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		[Serializable]
		public class RevenueBudgetFilter : IBqlTable
		{
			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			protected Int32? _ProjectTaskID;
			[ProjectTask(typeof(PMProject.contractID), AlwaysEnabled = true, DirtyRead = true)]
			public virtual Int32? ProjectTaskID
			{
				get
				{
					return this._ProjectTaskID;
				}
				set
				{
					this._ProjectTaskID = value;
				}
			}
			#endregion
			#region GroupByTask
			public abstract class groupByTask : PX.Data.BQL.BqlBool.Field<groupByTask> { }
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Group by Task")]
			public virtual Boolean? GroupByTask
			{
				get;
				set;
			}
			#endregion
		}

		public struct POLineKey
		{
			public readonly string OrderType;
			public readonly string OrderNbr;
			public readonly int LineNbr;
			
			public POLineKey(string orderType, string orderNbr, int lineNbr)
			{
				OrderType = orderType;
				OrderNbr = orderNbr;
				LineNbr = lineNbr;
			}

			public override int GetHashCode()
			{
				unchecked // Overflow is fine, just wrap
				{
					int hash = 17;
					hash = hash * 23 + OrderType.GetHashCode();
					hash = hash * 23 + OrderNbr.GetHashCode();
					hash = hash * 23 + LineNbr.GetHashCode();
					return hash;
				}
			}
			
		}

		#endregion

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(RevenueBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMRevenueBudget.AccountGroupID)))
				{
					//Import file could be missing the AccountGroupID field and hence the Default value could be set by the DefaultEventHandler

					object keyVal = keys[nameof(PMRevenueBudget.AccountGroupID)];

					if (keyVal is int)
					{
						PMAccountGroup accountGroup = PMAccountGroup.PK.Find(this, (int?)keyVal);
						if (accountGroup != null)
						{
							return accountGroup.Type == GL.AccountType.Income;
						}
					}
					else
					{
						accountGroupCD = (string)keys[nameof(PMRevenueBudget.AccountGroupID)];
					}
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.Type == GL.AccountType.Income;
					}
				}
				else
				{
					return true;
				}
				return false;
			}
			else if (viewName == nameof(CostBudget))
			{
				string accountGroupCD = null;
				if (keys.Contains(nameof(PMCostBudget.AccountGroupID)))
				{
					accountGroupCD = (string)keys[nameof(PMCostBudget.AccountGroupID)];
				}

				if (!string.IsNullOrEmpty(accountGroupCD))
				{
					PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupCD, Equal<Required<PMAccountGroup.groupCD>>>>.Select(this, accountGroupCD);
					if (accountGroup != null)
					{
						return accountGroup.IsExpense == true;
					}
				}
				else
				{
					return true;
				}
				return false;
			}			
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion
	}

	public class ChangeOrderReleaseScope: FlaggedModeScopeBase<ChangeOrderReleaseScope>
	{
		public ChangeOrderReleaseScope() : base() {}
	}
}
