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
using System.Text;

using CommonServiceLocator;

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.DependencyInjection;
using PX.Data.WorkflowAPI;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.IN;
using PX.Objects.TX;

using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	[Serializable]
	public class ProformaEntry : PXGraph<ProformaEntry, PMProforma>, IGraphWithInitialization
	{
		#region Extensions

		public class MultiCurrency : MultiCurrencyGraph<ProformaEntry, PMProforma>
		{
			protected override string Module => BatchModule.PM;

			protected override CurySourceMapping GetCurySourceMapping()
			{
				return new CurySourceMapping(typeof(Customer));
			}

			protected override CurySource CurrentSourceSelect()
			{
				CurySource curySource = base.CurrentSourceSelect();
				// old handler was using allowOverrideRate as AllowOverrideCury
				if (curySource != null) curySource.AllowOverrideCury = curySource.AllowOverrideRate;
				return curySource;
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(PMProforma))
				{
					BAccountID = typeof(PMProforma.customerID),
					DocumentDate = typeof(PMProforma.invoiceDate),
					CuryID = typeof(PMProforma.curyID)
				};
			}

			protected override bool AllowOverrideCury() => Base.CanEditDocument(Base.Document.Current);

			protected override PXSelectBase[] GetChildren() => new PXSelectBase[]
			{
				Base.Document,
				Base.ProgressiveLines,
				Base.TransactionLines,
				Base.Overflow,
				Base.Taxes,
				Base.Tax_Rows
			};

			protected override PXSelectBase[] GetTrackedExceptChildren() => new PXSelectBase[] { Base.dummyRevenueBudget };

			protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyID> e)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
				{
					if (string.IsNullOrEmpty(Base.Project.Current?.BillingCuryID)) return;

					e.NewValue = Base.Project.Current.BillingCuryID;
					e.Cancel = true;
				}
				else
					// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers
					base._(e);
			}

			protected override void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.curyEffDate> e)
			{
				if (Base.Document.Cache.Current == null) return;

				e.NewValue = ((PMProforma)Base.Document.Cache.Current).InvoiceDate;
				e.Cancel = true;
			}

			protected virtual void _(Events.RowInserting<PMProformaTransactLine> e)
			{
				recalculateRowBaseValues(e.Cache, e.Row, TrackedItems[Base.TransactionLines.Cache.GetItemType()]);
			}

			protected virtual void _(Events.RowInserting<PMProformaProgressLine> e)
			{
				recalculateRowBaseValues(e.Cache, e.Row, TrackedItems[Base.ProgressiveLines.Cache.GetItemType()]);
			}

			protected override void _(Events.FieldVerifying<Document, Document.curyID> e)
			{
				ThrowIfCuryIDCannotBeChangedDueTo((string)e.NewValue);
				// Acuminator disable once PX1044 ChangesInPXCacheInEventHandlers
				base._(e);
			}

			private void ThrowIfCuryIDCannotBeChangedDueTo(string newValue)
			{
				if (Base.Project.Current == null) return;
				if (Base.Project.Current.BillingCuryID == newValue) return;

				throw new PXSetPropertyException(Messages.BillingCurrencyCannotBeChanged, Base.Project.Current.BillingCuryID)
				{
					ErrorValue = newValue
				};
			}
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class ProformaEntry_ActivityDetailsExt : PMActivityDetailsExt<ProformaEntry, PMProforma, PMProforma.noteID>
		{
			public override Type GetBAccountIDCommand() => typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<PMProforma.customerID>>>>);

			public override Type GetEmailMessageTarget() => typeof(Select2<Contact,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<Contact.bAccountID>, And<Customer.defContactID, Equal<Contact.contactID>>>>,
				Where<Customer.bAccountID, Equal<Current<PMProforma.customerID>>>>);
		}

		#endregion

		public const string ProformaInvoiceReport = "PM642000";
		public const string ProformaNotificationCD = "PROFORMA";
		public readonly ProformaTotalsCounter.AmountBaseKey PayByLineOffKey = new ProformaTotalsCounter.AmountBaseKey(0, CostCodeAttribute.DefaultCostCode.GetValueOrDefault(), PMInventorySelectorAttribute.EmptyInventoryID, 0);
		public readonly ProformaTotalsCounter TotalsCounter = new ProformaTotalsCounter();

		#region DAC Overrides

		[PXDefault(BAccountType.CustomerType)]
		protected virtual void _(Events.CacheAttached<BAccountR.type> e)
		{
		}
				
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Actual Amount", Enabled = false, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMRevenueBudget.curyActualAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = Messages.DraftInvoiceAmount, Enabled = false, Visible = false)]
		protected virtual void _(Events.CacheAttached<PMRevenueBudget.curyInvoicedAmount> e) { }

		#region EPApproval Cache Attached - Approvals Fields
		[PXDBDate()]
		[PXDefault(typeof(PMProforma.invoiceDate), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.docDate> e)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(PMProforma.customerID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.bAccountID> e)
		{
		}

		[PXDBString(60, IsUnicode = true)]
		[PXDefault(typeof(PMProforma.description), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.descr> e)
		{
		}
		
		
		[PXDBLong]
		[CurrencyInfo(typeof(PMProforma.curyInfoID))]
		protected virtual void _(Events.CacheAttached<EPApproval.curyInfoID> e)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMProforma.curyDocTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.curyTotalAmount> e)
		{
		}

		[PXDBDecimal]
		[PXDefault(typeof(PMProforma.docTotal), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void _(Events.CacheAttached<EPApproval.totalAmount> e)
		{
		}

		#endregion

		#region PMTran
		[Account(null, typeof(Search2<Account.accountID,
			LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Current<PMTran.accountGroupID>>>>,
			Where<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>, And<Account.accountGroupID, Equal<Current<PMTran.accountGroupID>>,
			Or<PMAccountGroup.type, Equal<PMAccountType.offBalance>,
			Or<PMAccountGroup.groupID, IsNull>>>>>), DisplayName = "Debit Account", Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTran.accountID> e) { }

		[SubAccount(typeof(PMTran.accountID), DisplayName = "Debit Subaccount", Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTran.subID> e) { }

		[Account(DisplayName = "Credit Account", Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTran.offsetAccountID> e) { }

		[SubAccount(typeof(PMTran.offsetAccountID), DisplayName = "Credit Subaccount", Visible = false)]
		protected virtual void _(Events.CacheAttached<PMTran.offsetSubID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Amount")]
		protected virtual void _(Events.CacheAttached<PMTran.projectCuryAmount> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Currency", FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		protected virtual void _(Events.CacheAttached<PMTran.projectCuryID> e) { }
		#endregion

		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(PMProforma.taxZoneID))]
		[PXUIFieldAttribute(DisplayName = "Customer Tax Zone", Enabled = false)]
		protected virtual void _(Events.CacheAttached<PMTaxTran.taxZoneID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(TaxCategory.taxCategoryID.Length, IsUnicode = true)]
		protected virtual void _(Events.CacheAttached<PMProformaLine.taxCategoryID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMProject.billAddressID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt]
		protected virtual void _(Events.CacheAttached<PMProject.billContactID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Currency Rate for Budget", IsReadOnly = true, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		protected virtual void _(Events.CacheAttached<PMProject.curyID> e) { }
		#endregion

		[PXViewName(Messages.Proforma)]
		public SelectFrom<PMProforma>
			.LeftJoin<PMProject>
				.On<PMProject.contractID.IsEqual<PMProforma.projectID>>
			.Where<
				Brackets<PMProject.contractID.IsNull.Or<MatchUserFor<PMProject>>>
				.And<PMProforma.corrected.IsNotEqual<True>>>
			.View Document;
		public PXFilter<PMProformaOverflow> Overflow;
		public PXSelect<PMProforma, Where<PMProforma.refNbr, Equal<Current<PMProforma.refNbr>>, And<PMProforma.revisionID, Equal<Current<PMProforma.revisionID>>>>> DocumentSettings;
		public ProgressLineSelect ProgressiveLines;
		public TransactLineSelect TransactionLines;
		public PXSetup<ARSetup> arSetup;
		public PXSetup<Branch>.Where<Branch.branchID.IsEqual<PMProforma.branchID.AsOptional>> branch;

		public PXSelect<PMProformaRevision, Where<PMProformaRevision.refNbr, Equal<Current<PMProforma.refNbr>>, And<PMProformaRevision.revisionID, NotEqual<Current<PMProforma.revisionID>>>>, OrderBy<Asc<PMProformaRevision.revisionID>>> Revisions;

		public PXOrderedSelect<PMProforma, PMProformaTransactLine,
			Where<PMProformaTransactLine.refNbr, Equal<Current<PMProforma.refNbr>>,
			And<PMProformaTransactLine.revisionID, Equal<Current<PMProforma.revisionID>>,
			And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>>>>,
			OrderBy<Asc<PMProformaTransactLine.sortOrder, Asc<PMProformaTransactLine.lineNbr>>>> Trans;

		[InjectDependency]
		protected ILicenseLimitsService _licenseLimits { get; set; }
		[InjectDependency]
		private ICurrentUserInformationProvider _currentUserInformationProvider { get; set; }

		[InjectDependency]
		public IProjectMultiCurrency MultiCurrencyService { get; set; }

		public virtual IEnumerable transactionLines()
		{
			if (!IsLimitsEnabled())
			{
				return Trans.Select();
			}
			else
			{
				var select = new PXOrderedSelect<PMProforma, PMProformaTransactLine,
					Where<PMProformaTransactLine.refNbr, Equal<Current<PMProforma.refNbr>>,
					And<PMProformaTransactLine.revisionID, Equal<Current<PMProforma.revisionID>>,
					And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>>>>,
					OrderBy<Asc<PMProformaTransactLine.sortOrder, Asc<PMProformaTransactLine.lineNbr>>>>(this);

				List<PMProformaTransactLine> result = new List<PMProformaTransactLine>();

				var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);
				var revenueBudget = selectRevenueBudget.Select(Project.Current.ContractID);

				Dictionary<BudgetKeyTuple, Tuple<decimal, decimal>> maxLimits = new Dictionary<BudgetKeyTuple, Tuple<decimal, decimal>>();//in Project currency
				Dictionary<BudgetKeyTuple, decimal> currentDocumentAmounts = new Dictionary<BudgetKeyTuple, decimal>();//in Project currency
				var resultset = select.Select();
				foreach (PMProformaTransactLine line in resultset)
				{
					if (line.IsPrepayment == true)
						continue;

					BudgetKeyTuple key = new BudgetKeyTuple(line.ProjectID.GetValueOrDefault(), line.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(line).GetValueOrDefault(),
						Project.Current.BudgetLevel == BudgetLevels.Item ? line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) : PMInventorySelectorAttribute.EmptyInventoryID,
						Project.Current.BudgetLevel == BudgetLevels.Task ? CostCodeAttribute.GetDefaultCostCode() : line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
					decimal invoicedAmount;
					if (currentDocumentAmounts.TryGetValue(key, out invoicedAmount))
					{
						currentDocumentAmounts[key] = invoicedAmount + GetAmountInProjectCurrency(line.CuryLineTotal);
					}
					else
					{
						currentDocumentAmounts[key] = GetAmountInProjectCurrency(line.CuryLineTotal);
					}
				}

				foreach (PMRevenueBudget budget in revenueBudget)
				{
					if (budget.LimitAmount != true)
						continue;

					BudgetKeyTuple key = BudgetKeyTuple.Create(budget);

					//Total invoiced amount including sum of all transactions in current document.
					//invoicedAmountIncludingCurrentDocument is in ProjectCurrency (i.e. = Base if MC is OFF; = Doc.CuryID if MC is ON)
					decimal invoicedAmountIncludingCurrentDocument = GetCuryActualAmountWithTaxes(budget) + budget.CuryInvoicedAmount.GetValueOrDefault() + CalculatePendingInvoicedAmount(budget.ProjectID, budget.ProjectTaskID, budget.AccountGroupID, budget.InventoryID, budget.CostCodeID);
					currentDocumentAmounts.TryGetValue(key, out decimal invoicedAmountCurrentDoc);

					decimal previouslyInvoicedTotal = invoicedAmountIncludingCurrentDocument - invoicedAmountCurrentDoc;

					Tuple<decimal, decimal> bucket = new Tuple<decimal, decimal>(budget.CuryMaxAmount.GetValueOrDefault(), Math.Max(0, budget.CuryMaxAmount.GetValueOrDefault() - previouslyInvoicedTotal));
					maxLimits.Add(key, bucket);
				}

				Dictionary<int, decimal> negativeAdjustmentPool = new Dictionary<int, decimal>();
				decimal overflowTotal = 0;

				foreach (PMProformaTransactLine line in resultset)
				{
					if (line.IsPrepayment == true)
						continue;

					BudgetKeyTuple key = new BudgetKeyTuple(line.ProjectID.GetValueOrDefault(), line.TaskID.GetValueOrDefault(), GetProjectedAccountGroup(line).GetValueOrDefault(),
						(Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) ? line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID) : PMInventorySelectorAttribute.EmptyInventoryID,
						Project.Current.BudgetLevel == BudgetLevels.Task ? CostCodeAttribute.GetDefaultCostCode() : line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));

					Tuple<decimal, decimal> bucket;
					if (maxLimits.TryGetValue(key, out bucket))
					{
						line.CuryMaxAmount = GetAmountInBillingCurrency(bucket.Item1);

						decimal adj = 0;//In doc currency
						negativeAdjustmentPool.TryGetValue(line.TaskID.Value, out adj);//In DocCury

						line.CuryAvailableAmount = Math.Max(0, GetAmountInBillingCurrency(bucket.Item2 + adj));

						if (GetAmountInProjectCurrency(line.CuryLineTotal) > 0)
						{
							decimal remainder = line.CuryAvailableAmount.GetValueOrDefault() - line.CuryLineTotal.GetValueOrDefault();
							if (remainder >= adj)
							{
								line.CuryOverflowAmount = 0;
								maxLimits[key] = new Tuple<decimal, decimal>(bucket.Item1, GetAmountInProjectCurrency(remainder - adj));
							}
							else
							{
								line.CuryOverflowAmount = remainder > 0 ? 0 : -remainder;
								maxLimits[key] = new Tuple<decimal, decimal>(bucket.Item1, GetAmountInProjectCurrency(remainder - adj));
								overflowTotal += line.CuryOverflowAmount.Value;
							}
						}
						else
						{
							if (negativeAdjustmentPool.ContainsKey(line.TaskID.Value))
							{

								negativeAdjustmentPool[line.TaskID.Value] += -(line.CuryLineTotal.GetValueOrDefault());
							}
							else
							{
								negativeAdjustmentPool[line.TaskID.Value] = -(line.CuryLineTotal.GetValueOrDefault());
							}
						}
					}

					result.Add(line);
				}

				SetOverflowTotal(overflowTotal);

				return result;
			}
		}

		private void SetOverflowTotal(decimal overflowTotal)
		{
			Overflow.Current.CuryOverflowTotal = overflowTotal;
			if (overflowTotal == 0m) Overflow.Current.OverflowTotal = overflowTotal;
			else
			{
				CurrencyInfo currencyInfo = GetExtension<MultiCurrency>().GetDefaultCurrencyInfo();
				Overflow.Current.OverflowTotal = currencyInfo.CuryConvBase(overflowTotal);
			}
			Overflow.View.RequestRefresh();
		}

		public PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Current<PMProformaTransactLine.refNbr>>, And<PMTran.proformaLineNbr, Equal<Current<PMProformaTransactLine.lineNbr>>>>> Details;
		public PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Current<PMProformaTransactLine.refNbr>>>> AllReferencedTransactions;

		public PXSelect<PMTran> Unbilled;
		public virtual IEnumerable unbilled()
		{
			List<PMTran> billingBase = new List<PMTran>();
			if (Document.Current == null)
				return billingBase;

			PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();

			PMProject project = PMProject.PK.Find(engine, Document.Current.ProjectID);
			if (project != null)
			{
				List<PMTask> tasks = engine.SelectBillableTasks(project);

				DateTime cuttoffDate = Document.Current.InvoiceDate.Value.AddDays(engine.IncludeTodaysTransactions ? 1 : 0);
				engine.PreSelectTasksTransactions(Document.Current.ProjectID, tasks, cuttoffDate); //billingRules dictionary also filled.

				
				foreach (PMTask task in tasks)
				{
					List<PMBillingRule> rulesList;
					if (engine.billingRules.TryGetValue(task.BillingID, out rulesList))
					{
						foreach (PMBillingRule rule in rulesList)
						{
							if (rule.Type == PMBillingType.Transaction)
							{
								billingBase.AddRange(engine.SelectBillingBase(task.ProjectID, task.TaskID, rule.AccountGroupID, rule.IncludeNonBillable == true));
							}
						}
					}
				}

				HashSet<long> unbilledBase = new HashSet<long>();
				foreach (PMTran tran in billingBase)
				{
					unbilledBase.Add(tran.TranID.Value);
					PMTran located = Unbilled.Locate(tran);
					if (located != null && (located.Billed == true || located.ExcludedFromBilling == true))
					{
						tran.Selected = true;
					}
				}

				foreach (PMTran tran in Unbilled.Cache.Updated)
				{
					if (tran.Billed != true && tran.ExcludedFromBilling != true && !unbilledBase.Contains(tran.TranID.Value))
					{
						billingBase.Add(tran);
					}
				}
			}
			return billingBase;
		}

		public PXSelect<PMBudgetAccum> Budget;
		public PXSelect<PMBillingRecord> BillingRecord;
		public PXSelect<ARInvoice> Invoices;
		[PXViewName(PM.Messages.Project)]
		public PXSetup<PMProject>.Where<PMProject.contractID.IsEqual<PMProforma.projectID.FromCurrent>> Project;
		[PXViewName(AR.Messages.Customer)]
		public PXSetup<Customer>.Where<Customer.bAccountID.IsEqual<PMProforma.customerID.AsOptional>> Customer;
		public PXSetup<Location>.Where<Location.bAccountID.IsEqual<PMProforma.customerID.FromCurrent>.And<Location.locationID.IsEqual<PMProforma.locationID.AsOptional>>> Location;

		[PXViewName(Messages.Approval)]
		public EPApprovalAutomation<PMProforma, PMProforma.approved, PMProforma.rejected, PMProforma.hold, PMSetupProformaApproval> Approval;

		public PXSelect<PMAddress, Where<PMAddress.addressID, Equal<Current<PMProforma.billAddressID>>>> Billing_Address;
		public PXSelect<PMContact, Where<PMContact.contactID, Equal<Current<PMProforma.billContactID>>>> Billing_Contact;

		[PXViewName(Messages.PMAddress)]
		public PXSelect<PMShippingAddress, Where<PMShippingAddress.addressID, Equal<Current<PMProforma.shipAddressID>>>> Shipping_Address;
		[PXViewName(Messages.PMContact)]
		public PXSelect<PMShippingContact, Where<PMShippingContact.contactID, Equal<Current<PMProforma.shipContactID>>>> Shipping_Contact;

		[PXCopyPasteHiddenView]
		public PXSelect<PMTax, Where<PMTax.refNbr, Equal<Current<PMProforma.refNbr>>, And<PMTax.revisionID, Equal<Current<PMProforma.revisionID>>>>, OrderBy<Asc<PMTax.refNbr, Asc<PMTax.taxID>>>> Tax_Rows;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<PMTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<PMTaxTran.taxID>>>,
			Where<PMTaxTran.refNbr, Equal<Current<PMProforma.refNbr>>,
				And<PMTaxTran.revisionID, Equal<Current<PMProforma.revisionID>>>>> Taxes;

		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> Company;
		public PXSetup<ARSetup> ARSetup;
		public PXSetup<TaxZone>.Where<TaxZone.taxZoneID.IsEqual<PMProforma.taxZoneID.FromCurrent>> taxzone;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMUnbilledDailySummaryAccum> UnbilledSummary;

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMRevenueBudget> dummyRevenueBudget; //for cache attached to rename fields for joinned table in ProgressiveLines view.

		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<BAccountR> dummyAccountR;

		[PXViewName(CR.Messages.MainContact)]
		public PXSelect<Contact> DefaultCompanyContact;
		protected virtual IEnumerable defaultCompanyContact()
		{
			return OrganizationMaint.GetDefaultContactForCurrentOrganization(this);
		}

		[InjectDependency]
		protected IFinPeriodUtils FinPeriodUtils { get; set; }

		[InjectDependency]
		protected IFinPeriodRepository FinPeriodRepository { get; set; }

		public Dictionary<int, List<PMTran>> cachedReferencedTransactions;

		public bool SuppressRowSeleted
		{
			get;
			set;
		}
				
		public ProformaEntry()
		{
			Setup.Cache.Clear();
			OpenPeriodAttribute.SetValidatePeriod<PMProforma.finPeriodID>(Document.Cache, null, PeriodValidation.DefaultSelectUpdate);
			uploadFromBudget.SetVisible(IsMigrationMode());
			CopyPaste.SetVisible(false);
			Insert.SetVisible(IsMigrationMode());

			var pmAddressCache = Caches[typeof(PMAddress)];
			var pmContactCache = Caches[typeof(PMContact)];
			var pmShippingAddressCache = Caches[typeof(PMShippingAddress)];
			var pmShippingContactCache = Caches[typeof(PMShippingContact)];
		}

		public bool IsMigrationMode()
		{
			return Setup.Current.MigrationMode == true;
		}

		protected virtual void BeforeCommitHandler(PXGraph e)
		{
			var check = _licenseLimits.GetCheckerDelegate<PMProforma>(new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(PMProformaLine), (graph) =>
			{
				return new PXDataFieldValue[]
				{
							new PXDataFieldValue<PMProformaLine.refNbr>(((ProformaEntry)graph).Document.Current?.RefNbr)

				};
			}));

			try
			{
				check.Invoke(e);
			}
			catch (PXException)
			{
				throw new PXException(Messages.LicenseProgressBillingAndTimeAndMaterial);
			}

		}

		void IGraphWithInitialization.Initialize()
		{
			if (_licenseLimits != null)
			{
				OnBeforeCommit += BeforeCommitHandler;

			}
		}

		#region Actions
		public PXAction<PMProforma> release;
		[PXUIField(DisplayName = GL.Messages.Release)]
		[PXProcessButton]
		public IEnumerable Release(PXAdapter adapter)
		{
			RecalculateExternalTaxesSync = true;
			this.Save.Press();

			PXLongOperation.StartOperation(this, delegate () {

				ProformaEntry pe = PXGraph.CreateInstance<ProformaEntry>();
				pe.Document.Current = Document.Current;
				pe.RecalculateExternalTaxesSync = true;
				pe.ReleaseDocument(Document.Current);

			});
			return adapter.Get();
		}

		public PXAction<PMProforma> proformaReport;
		[PXUIField(DisplayName = "Print", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable ProformaReport(PXAdapter adapter)
		{
			OpenReport(ProformaInvoiceReport, Document.Current);

			return adapter.Get();
		}

		public virtual void OpenReport(string reportID, PMProforma doc)
		{
			if (doc != null)
			{
				string specificReportID = new NotificationUtility(this).SearchProjectReport(reportID, Project.Current.ContractID, Project.Current.DefaultBranchID);

				throw new PXReportRequiredException(new Dictionary<string, string>
				{
					["RefNbr"] = doc.RefNbr
				}, specificReportID, specificReportID);
			}
		}

		public PXAction<PMProforma> send;
		[PXUIField(DisplayName = "Email", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable Send(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				PXLongOperation.StartOperation(this, delegate () {
					ProformaEntry pe = PXGraph.CreateInstance<ProformaEntry>();
					pe.Document.Current = Document.Current;
					pe.SendReport(ProformaNotificationCD, Document.Current, adapter.MassProcess);
				});
			}

			return adapter.Get();
		}

		public virtual void SendReport(string notificationCD, PMProforma doc, bool massProcess = false)
		{
			if (doc != null)
			{
				Dictionary<string, string> mailParams = new Dictionary<string, string>();
				mailParams["RefNbr"] = Document.Current.RefNbr;
				
				using (var ts = new PXTransactionScope())
				{
					this.GetExtension<ProformaEntry_ActivityDetailsExt>().SendNotification(PMNotificationSource.Project, notificationCD, doc.BranchID, mailParams, massProcess);
					this.Save.Press();

					ts.Complete();
				}
			}
		}
				
		public PXAction<PMProforma> autoApplyPrepayments;
		[PXUIField(DisplayName = "Apply Available Prepaid Amounts")]
		[PXProcessButton]
		public IEnumerable AutoApplyPrepayments(PXAdapter adapter)
		{
			ApplyPrepayment(Document.Current);

			yield return Document.Current;
		}

		public PXAction<PMProforma> viewTranDocument;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTranDocument(PXAdapter adapter)
		{
			RegisterEntry graph = CreateInstance<RegisterEntry>();
			graph.Document.Current = graph.Document.Search<PMRegister.refNbr>(Details.Current.RefNbr, Details.Current.TranType);
			throw new PXRedirectRequiredException(graph, "PMTransactions") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> uploadUnbilled;
		[PXUIField(DisplayName = "Upload Unbilled Transactions")]
		[PXButton]
		public IEnumerable UploadUnbilled(PXAdapter adapter)
		{			
			if(Unbilled.View.AskExt() == WebDialogResult.OK)
			{
				AppendUnbilled();
			}

			return adapter.Get();
		}

		public PXAction<PMProforma> appendSelected;
		[PXUIField(DisplayName = "Upload")]
		[PXButton]
		public IEnumerable AppendSelected(PXAdapter adapter)
		{
			AppendUnbilled();

			return adapter.Get();
		}

		public PXAction<PMProforma> viewProgressLineTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProgressLineTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current< PMProformaProgressLine.projectID>>, And<PMTask.taskID, Equal<Current<PMProformaProgressLine.taskID>>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> viewTransactLineTask;
		[PXUIField(DisplayName = Messages.ViewTask, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTransactLineTask(PXAdapter adapter)
		{
			ProjectTaskEntry graph = CreateInstance<ProjectTaskEntry>();
			graph.Task.Current = PXSelect<PMTask, Where<PMTask.projectID, Equal<Current<PMProformaTransactLine.projectID>>, And<PMTask.taskID, Equal<Current<PMProformaTransactLine.taskID>>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewTask) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		public PXAction<PMProforma> viewProgressLineInventory;
		[PXUIField(DisplayName = "View Inventory Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProgressLineInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMProformaProgressLine.inventoryID>>>>.Select(this);
			if (item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMProforma> viewTransactLineInventory;
		[PXUIField(DisplayName = "View Inventory Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTransactLineInventory(PXAdapter adapter)
		{
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<PMProformaTransactLine.inventoryID>>>>.Select(this);
			if (item != null && item.ItemStatus != InventoryItemStatus.Unknown)
			{
				if (item.StkItem == true)
				{
					InventoryItemMaint graph = CreateInstance<InventoryItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else
				{
					NonStockItemMaint graph = CreateInstance<NonStockItemMaint>();
					graph.Item.Current = item;
					throw new PXRedirectRequiredException(graph, true, "View Inventory Item") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXAction<PMProforma> viewVendor;
		[PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewVendor(PXAdapter adapter)
		{
			Vendor vendor = PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<PMProformaTransactLine.vendorID>>>>.Select(this);

			if (vendor != null)
			{
				VendorMaint graph = CreateInstance<VendorMaint>();
				graph.BAccount.Current = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<PMProformaTransactLine.vendorID>>>>.Select(this);
				throw new PXRedirectRequiredException(graph, true, "View Vendor") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<PMProforma> uploadFromBudget;
		[PXUIField(DisplayName = "Load Lines")]
		[PXProcessButton]
		public IEnumerable UploadFromBudget(PXAdapter adapter)
		{
			if (Document.Current != null && Document.Current.ProjectID != null)
			{
				var select = new PXSelectJoin<PMRevenueBudget,
					InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMRevenueBudget.accountGroupID>>>,
					Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
					And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

				var existingLines = new Dictionary<string, PMProformaProgressLine>();

				foreach (PMProformaProgressLine line in ProgressiveLines.Select())
				{
					existingLines[GetProformaLineKey(line)] = line;
				}

				PMBillEngine billEngine = PXGraph.CreateInstance<PMBillEngine>();
				Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, Project.Current.CustomerID);

				foreach (PXResult<PMRevenueBudget, PMAccountGroup> res in select.Select(Document.Current.ProjectID))
				{
					PMRevenueBudget line = (PMRevenueBudget)res;
					PMAccountGroup ag = (PMAccountGroup)res;

					PMTask task = PMTask.PK.FindDirty(this, line.ProjectID, line.TaskID);

					PMBillingRule rule = SelectFrom<PMBillingRule>
						.Where<PMBillingRule.billingID.IsEqual<P.AsString>
							.And<PMBillingRule.type.IsEqual<PMBillingType.budget>>>
						.View
						.Select(this, task?.BillingID);

					if (rule == null)
						continue;

					PMProformaProgressLine proformaLine = new PMProformaProgressLine();
					proformaLine.Type = PMProformaLineType.Progressive;
					proformaLine.Description = line.Description;
					proformaLine.BillableQty = 0;
					proformaLine.Qty = proformaLine.BillableQty;
					proformaLine.UOM = line.UOM;
					proformaLine.ProjectID = line.ProjectID;
					proformaLine.TaskID = line.ProjectTaskID;
					proformaLine.AccountGroupID = line.AccountGroupID;
					proformaLine.CostCodeID = line.CostCodeID;
					proformaLine.InventoryID = line.InventoryID;
					proformaLine.TaxCategoryID = line.TaxCategoryID;
					proformaLine.AccountID = billEngine.CalculateTargetSalesAccountID(rule, Project.Current, task, null, proformaLine, customer);
					string subCD = billEngine.CalculateTargetSalesSubaccountCD(rule, Project.Current, task, null, null, null, line.InventoryID, customer);
					proformaLine.BranchID = billEngine.CalculateTargetBranchID(rule, Project.Current, task, null, customer, null);
					proformaLine.RetainagePct = line.RetainagePct;
					proformaLine.CuryBillableAmount = proformaLine.CuryBillableAmount;
					proformaLine.CuryAmount = proformaLine.CuryBillableAmount;
					proformaLine.ProgressBillingBase = line.ProgressBillingBase;

					if (existingLines.TryGetValue(GetProformaLineKey(proformaLine), out var existingLine))
					{
						if (adapter.ImportFlag)
						{
							// No override existing lines in import scenarios
							continue;
						}
						else
						{
							ProgressiveLines.Delete(existingLine);
						}
					}
					proformaLine = ProgressiveLines.Insert(proformaLine);
					if (proformaLine != null && subCD != null)
						ProgressiveLines.Cache.SetValueExt<PMProformaProgressLine.subID>(proformaLine, subCD);
				}
			}

			return adapter.Get();
		}

		private static string GetProformaLineKey(PMProformaLine line)
			=> $"{line.TaskID}.{line.AccountGroupID}.{line.CostCodeID}.{line.InventoryID}";

		public PXAction<PMProforma> removeHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold")]
		protected virtual IEnumerable RemoveHold(PXAdapter adapter) 
		{
			if (Document.Current != null)
			{
				ValidateLimitsOnUnhold(Document.Current);
				Document.Current.Hold = false;
				Document.Update(Document.Current);
			}
			return adapter.Get(); 
		}

		public PXAction<PMProforma> hold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold")]
		protected virtual IEnumerable Hold(PXAdapter adapter) => adapter.Get();

		#endregion

		#region Entity Event Handlers

		public PXWorkflowEventHandler<PMProforma> OnRelease;

		#endregion

		#region Event Handlers


		protected virtual void _(Events.RowDeleted<PMProforma> e)
		{
			PMProformaRevision last = GetLastRevision();
			if (e.Row != null && last == null)
			{
				PMBillingRecord record = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>.Select(this, e.Row.RefNbr);
				BillingRecord.Delete(record);

				var selectAllTransactions = new PXSelect<PMTran, Where<PMTran.proformaRefNbr, Equal<Required<PMTran.proformaRefNbr>>>>(this);
				foreach (PMTran tran in selectAllTransactions.Select(e.Row.RefNbr))
				{
					Unbill(tran);
				}
			}

			//restore total retained & Billing Record references
			if (last != null)
			{
				var select = new PXSelect<PMProformaLine,
					Where<PMProformaLine.refNbr, Equal<Required<PMProforma.refNbr>>,
					And<PMProformaLine.revisionID, Equal<Required<PMProforma.revisionID>>>>>(this);

				foreach (PMProformaLine line in select.Select(last.RefNbr, last.RevisionID))
				{
					AddToTotalRetained(line);
				}

				PMBillingRecord record = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>.Select(this, e.Row.RefNbr);
				record.ARDocType = last.ARInvoiceDocType;
				record.ARRefNbr = last.ARInvoiceRefNbr;
				BillingRecord.Update(record);
			}

			var arDocType = e.Row?.ARInvoiceDocType;
			var arRefNbr = e.Row?.ARInvoiceRefNbr;
			if (!string.IsNullOrEmpty(arDocType) && !string.IsNullOrEmpty(arRefNbr))
			{
				UpdateInvoice(arDocType, arRefNbr, false);
			}
		}

		protected virtual void _(Events.RowInserted<PMProformaTransactLine> e)
		{
			Document.Cache.SetValue<PMProforma.enableTransactional>(Document.Current, true);

			AddToInvoiced(e.Row);
			AddToDraftRetained(e.Row);
			AddToTotalRetained(e.Row);
			SubtractPerpaymentRemainder(e.Row);
		}
				
		protected virtual void _(Events.RowInserted<PMProformaProgressLine> e)
		{
			Document.Cache.SetValue<PMProforma.enableProgressive>(Document.Current, true);
			AddToInvoiced(e.Row);
			AddToDraftRetained(e.Row);
			AddToTotalRetained(e.Row);
			SubtractPerpaymentRemainder(e.Row);
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.curyPrepaidAmount> e)
		{
			if (e.Row != null)
			{
				decimal? newAmount = (decimal?)e.NewValue;

				if (newAmount != null && e.Row.CuryPrepaidAmount > 0 && e.Row.CuryPrepaidAmount < newAmount)
				{
					e.NewValue = e.Row.CuryPrepaidAmount;
					e.Cache.RaiseExceptionHandling<PMProformaTransactLine.curyPrepaidAmount>(e.Row, e.Row.CuryPrepaidAmount, new PXSetPropertyException<PMProformaTransactLine.curyPrepaidAmount>(Messages.PrepaidAmountDecreased, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.curyPrepaidAmount> e)
		{
			if (e.Row != null)
			{
				decimal? newAmount = (decimal?)e.NewValue;

				if (newAmount != null && e.Row.CuryPrepaidAmount > 0 && e.Row.CuryPrepaidAmount < newAmount)
				{
					e.NewValue = e.Row.CuryPrepaidAmount;
					e.Cache.RaiseExceptionHandling<PMProformaProgressLine.curyPrepaidAmount>(e.Row, e.Row.CuryPrepaidAmount, new PXSetPropertyException<PMProformaProgressLine.curyPrepaidAmount>(Messages.PrepaidAmountDecreased, PXErrorLevel.Warning));
				}
			}
		}
				
		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.curyLineTotal> e)
		{
			if (e.Row.CuryLineTotal + e.Row.CuryPrepaidAmount < e.Row.CuryBillableAmount)
			{
				if (e.Row.Option == PMProformaLine.option.BillNow && !IsAdjustment(e.Row))
				{
					e.Cache.SetValue<PMProformaTransactLine.option>(e.Row, null);
					e.Cache.SetValuePending<PMProformaTransactLine.option>(e.Row, null);
				}
			}
		}
				
		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.completedPct> e)
		{
			PMRevenueBudget budget = SelectRevenueBudget(e.Row);
			if (budget != null)
			{
				if (e.Row.ProgressBillingBase == ProgressBillingBase.Amount)
				{
					decimal pendingInvoiceAmount = GetAmountInBillingCurrency(CalculatePendingInvoicedAmount(e.Row));
					decimal billableAmount = budget.CuryRevisedAmount.GetValueOrDefault() * e.Row.CompletedPct.GetValueOrDefault() / 100m;
					decimal invoicedAmount = GetCuryActualAmountWithTaxes(budget) + budget.CuryInvoicedAmount.GetValueOrDefault() + pendingInvoiceAmount - e.Row.CuryLineTotal.GetValueOrDefault() - GetLastInvoicedBeforeCorrection(e.Row);
					decimal unbilledAmount = Math.Max(0, billableAmount - invoicedAmount);

					ProgressiveLines.SetValueExt<PMProformaProgressLine.curyAmount>(e.Row, unbilledAmount - e.Row.CuryMaterialStoredAmount.GetValueOrDefault());
				}
				else if(e.Row.ProgressBillingBase == ProgressBillingBase.Quantity)
				{
					INUnitAttribute.TryConvertGlobalUnits(this, budget.UOM, e.Row.UOM, budget.RevisedQty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal qty);

					ProgressiveLines.SetValueExt<PMProformaProgressLine.qty>(e.Row, qty * (e.Row.CompletedPct.GetValueOrDefault() / 100m) - e.Row.PreviouslyInvoicedQty.GetValueOrDefault());
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.qty> e)
		{
			if (e.Row != null)
			{
				decimal amount = e.Row.Qty.GetValueOrDefault() * e.Row.CuryUnitPrice.GetValueOrDefault();
				ProgressiveLines.SetValueExt<PMProformaProgressLine.curyAmount>(e.Row, amount);
				ProgressiveLines.SetValueExt<PMProformaProgressLine.curyLineTotal>(e.Row, amount + e.Row.CuryMaterialStoredAmount);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.curyRetainage> e)
		{
			if (e.Row.CuryLineTotal > 0)
				e.Row.RetainagePct = Math.Min(100m, 100m * Math.Abs(e.Row.CuryRetainage.GetValueOrDefault() / e.Row.CuryLineTotal.GetValueOrDefault()));
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.accountID> e)
		{
			e.Cache.SetDefaultExt<PMProformaTransactLine.accountGroupID>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.retainagePct> e)
		{
			PMProject proj = PMProject.PK.Find(this, e.Row.ProjectID);
			if (proj != null)
			{
				e.NewValue = proj.RetainagePct;
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMProformaProgressLine.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaProgressLine, PMProformaProgressLine.curyRetainage> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal val = (decimal)e.NewValue;
				if (val > 0 && val > e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
				else if (val < 0 && e.Row.CuryLineTotal < 0 && val < e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.curyRetainage> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal val = (decimal)e.NewValue;
				if (val > 0 && val > e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
				else if (val < 0 && e.Row.CuryLineTotal < 0 && val < e.Row.CuryLineTotal)
				{
					e.NewValue = e.Row.CuryLineTotal;
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.curyRetainage> e)
		{
			if (e.Row.CuryLineTotal > 0)
				e.Row.RetainagePct = Math.Min(100m, 100m * Math.Abs(e.Row.CuryRetainage.GetValueOrDefault() / e.Row.CuryLineTotal.Value));
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.retainagePct> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				decimal percent = (decimal)e.NewValue;
				if (percent < 0 || percent > 100)
				{
					throw new PXSetPropertyException<PMProformaTransactLine.retainagePct>(IN.Messages.PercentageValueShouldBeBetween0And100);
				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaProgressLine, PMProformaProgressLine.currentInvoicedPct> e)
		{
			PMRevenueBudget budget = SelectRevenueBudget(e.Row);
			if (budget != null)
			{
				if (e.Row.ProgressBillingBase == ProgressBillingBase.Amount)
				{
					decimal unbilledAmount = budget.CuryRevisedAmount.GetValueOrDefault() * e.Row.CurrentInvoicedPct.GetValueOrDefault() / 100m;
					decimal amt = GetAmountInBillingCurrency(unbilledAmount);
					ProgressiveLines.SetValueExt<PMProformaProgressLine.curyAmount>(e.Row, amt - e.Row.CuryMaterialStoredAmount.GetValueOrDefault());
				}
				else if (e.Row.ProgressBillingBase == ProgressBillingBase.Quantity)
				{
					INUnitAttribute.TryConvertGlobalUnits(this, budget.UOM, e.Row.UOM, budget.RevisedQty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal qty);

					ProgressiveLines.SetValueExt<PMProformaProgressLine.qty>(e.Row, qty * (e.Row.CurrentInvoicedPct.GetValueOrDefault() / 100m));
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaProgressLine, PMProformaProgressLine.inventoryID> e)
		{
			if (e.Row != null && e.Row.InventoryID == null)
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaProgressLine, PMProformaProgressLine.costCodeID> e)
		{
			if (e.Row != null && e.Row.CostCodeID == null)
			{
				e.NewValue = CostCodeAttribute.DefaultCostCode;
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaProgressLine, PMProformaProgressLine.completedPct> e)
		{
			if (e.Row != null)
			{
				PMRevenueBudget budget = SelectRevenueBudget(e.Row);
				if (budget != null)
				{
					decimal result = 0.0m;
					if (e.Row.ProgressBillingBase == ProgressBillingBase.Amount)
					{
						if (budget.CuryRevisedAmount.GetValueOrDefault() != 0.0m)
						{
							decimal curyLineTotal = TotalsCounter.GetAmountBaseTotals(this, Document.Current.RefNbr, e.Row).CuryLineTotal;
							decimal invoicedAmount = GetAmountInProjectCurrency(e.Row.CuryLineTotal) + GetAmountInProjectCurrency(curyLineTotal);

							result = 100m * invoicedAmount / budget.CuryRevisedAmount.Value;
						}

						result = Math.Round(result, PMProformaProgressLine.completedPct.Precision);
					}
					else if(e.Row.ProgressBillingBase == ProgressBillingBase.Quantity)
					{
						decimal qtyTotal = TotalsCounter.GetQuantityBaseTotals(this, Document.Current.RefNbr, e.Row).QuantityTotal + e.Row.Qty.GetValueOrDefault();

						if(budget.RevisedQty.GetValueOrDefault() != 0.0m &&
							INUnitAttribute.TryConvertGlobalUnits(this, e.Row.UOM, budget.UOM, qtyTotal, INPrecision.QUANTITY, out qtyTotal))
						{
							result = 100.0m * qtyTotal / budget.RevisedQty.Value;
						}
					}

					PXFieldState fieldState = PXDecimalState.CreateInstance(result, PMProformaProgressLine.completedPct.Precision, nameof(PMProformaProgressLine.CompletedPct), false, 0, Decimal.MinValue, Decimal.MaxValue);
					e.ReturnState = fieldState;
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaProgressLine, PMProformaProgressLine.currentInvoicedPct> e)
		{
			if (e.Row != null)
			{
				PMRevenueBudget budget = SelectRevenueBudget(e.Row);
				if (budget != null)
				{
					
					decimal result = 0;

					if (e.Row.ProgressBillingBase == ProgressBillingBase.Amount)
					{
						if (budget.CuryRevisedAmount.GetValueOrDefault() != 0)
						{
							decimal invoicedAmount = GetAmountInProjectCurrency(e.Row.CuryLineTotal);
							result = Math.Round(100m * invoicedAmount / budget.CuryRevisedAmount.Value, PMProformaProgressLine.completedPct.Precision);
						}
					}
					else if(e.Row.ProgressBillingBase == ProgressBillingBase.Quantity)
					{
						decimal qtyToInvoice = 0.0m;

						if (budget.RevisedQty.GetValueOrDefault() != 0 &&
							INUnitAttribute.TryConvertGlobalUnits(this, e.Row.UOM, budget.UOM, e.Row.Qty.GetValueOrDefault(), INPrecision.QUANTITY, out qtyToInvoice))
						{
							result = Math.Round(100m * qtyToInvoice / budget.RevisedQty.Value, PMProformaProgressLine.completedPct.Precision);
						}
					}

					PXFieldState fieldState = PXDecimalState.CreateInstance(result, PMProformaProgressLine.completedPct.Precision, nameof(PMProformaProgressLine.CurrentInvoicedPct), false, 0, Decimal.MinValue, Decimal.MaxValue);
					e.ReturnState = fieldState;
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row != null)
			{
				string status = (string)e.ReturnValue;

				KeyValuePair<List<string>, List<string>> result = new KeyValuePair<List<string>, List<string>>(new List<string>(), new List<string>());

				if (status == PMProformaLine.option.Writeoff || status == PMProformaLine.option.BillNow)
				{
					result.Key.Add(PMProformaLine.option.BillNow);
					result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_BillNow));
					result.Key.Add(PMProformaLine.option.Writeoff);
					result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
				}
				else
				{
					if (e.Row.CuryLineTotal + e.Row.CuryPrepaidAmount < e.Row.CuryBillableAmount && e.Row.CuryLineTotal >= 0)
					{
						result.Key.Add(PMProformaLine.option.HoldRemainder);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_HoldRemainder));
						result.Key.Add(PMProformaLine.option.WriteOffRemainder);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_WriteOffRemainder));
						result.Key.Add(PMProformaLine.option.Writeoff);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
					}
					else
					{
						result.Key.Add(PMProformaLine.option.BillNow);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_BillNow));
						result.Key.Add(PMProformaLine.option.Writeoff);
						result.Value.Add(PXMessages.LocalizeNoPrefix(Messages.Option_Writeoff));
					}
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, typeof(PMProformaTransactLine.option).Name, false, 1, null, result.Key.ToArray(), result.Value.ToArray(), true, null);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row != null && e.NewValue != null)
			{
				if (e.NewValue.ToString() == PMProformaLine.option.HoldRemainder)
				{
					Dictionary<int, List<PMTran>> pmtranByProformalLineNbr = GetReferencedTransactions();
					List<PMTran> list;
					if (pmtranByProformalLineNbr.TryGetValue(e.Row.LineNbr.Value, out list))
					{
						bool containAllocation = false;
						foreach (PMTran item in list)
						{
							containAllocation = !string.IsNullOrEmpty(item.AllocationID);

							if (containAllocation)
								break;
						}

						if (containAllocation && list.Count > 1)
						{
							throw new PXSetPropertyException(Messages.GroupedAllocationsBillLater);
						}
					}

				}
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.option> e)
		{
			if (e.Row.Option == PMProformaLine.option.Writeoff)
			{
				e.Cache.SetValueExt<PMProformaTransactLine.curyPrepaidAmount>(e.Row, 0m);
				e.Cache.SetValueExt<PMProformaTransactLine.curyLineTotal>(e.Row, 0m);
				e.Cache.SetValueExt<PMProformaTransactLine.qty>(e.Row, 0m);
			}
			else if (e.Row.Option == PMProformaLine.option.BillNow && GetAmountInProjectCurrency(e.Row.CuryLineTotal) >= 0)
			{
				e.Cache.SetValueExt<PMProformaTransactLine.curyLineTotal>(e.Row, e.Row.CuryBillableAmount);
				e.Cache.SetValueExt<PMProformaTransactLine.qty>(e.Row, e.Row.BillableQty);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.accountID> e)
		{
			bool isLocationIntercompanySalesAccountDefaulting =
				Customer.Current?.IsBranch == true
				&& IsAdjustment(e.Row)
				&& arSetup.Current.IntercompanySalesAccountDefault == APAcctSubDefault.MaskLocation;

			if (e.Row != null 
				&& (e.Row.InventoryID == null 
					|| e.Row.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID
					|| isLocationIntercompanySalesAccountDefaulting) 
				&& Location.Current != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(TransactionLines.Cache, e.Row, Location.Current.CSalesAcctID) as Account;
				if (revenueAccount != null && revenueAccount.AccountGroupID != null)
				{
					e.NewValue = Location.Current.CSalesAcctID;
				}
				if (e.NewValue != null || isLocationIntercompanySalesAccountDefaulting)
				{
					e.Cancel = true;
				}
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.inventoryID> e)
		{
			if (e.Row != null && e.Row.InventoryID == null && IsAdjustment(e.Row) )
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProformaTransactLine, PMProformaTransactLine.taxCategoryID> e)
		{
			if (Project.Current != null)
			{
				if (e.Row.InventoryID != null && e.Row.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<PMProformaTransactLine.inventoryID>(e.Cache, e.Row);
					if (item != null && item.TaxCategoryID != null)
					{
						e.NewValue = item.TaxCategoryID;
					}
				}

				if (e.NewValue == null)
				{
					PMTask task = PMTask.PK.FindDirty(this, e.Row.ProjectID, e.Row.TaskID);
					if (task != null && task.TaxCategoryID != null)
					{
						e.NewValue = task.TaxCategoryID;
					}
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProformaTransactLine, PMProformaTransactLine.taskID> e)
		{
			if (IsAdjustment(e.Row))
			{
				PMTask task = PMTask.PK.FindDirty(this, e.Row.ProjectID, (int?)e.NewValue);
				if (task != null)
				{
					if (task.IsCompleted == true)
					{
						var ex = new PXTaskIsCompletedException(task.ProjectID, task.TaskID);
						ex.ErrorValue = task.TaskCD;
						throw ex;
					}
				}
			}
		}


		protected virtual void _(Events.RowDeleted<PMProformaProgressLine> e)
		{
			bool documentDeleted = false;
			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Deleted)
			{
				documentDeleted = true;
			}

			SubtractFromInvoiced(e.Row);
			SubtractFromDraftRetained(e.Row);
			SubtractFromTotalRetained(e.Row);
			
			SubtractPerpaymentRemainder(e.Row, -1);

			if (e.Row.IsPrepayment != true)
			{
				SubtractValuesToInvoice(e.Row, -e.Row.CuryBillableAmount, -e.Row.Qty); //Restoring AmountToInvoice and QtyToInvoice
			}
			
			if (!documentDeleted && !RecalculatingContractRetainage)
			{
				RecalculateRetainageOnDocument(Project.Current);
			}
		}

		private ISet<int> GetUserBranches()
		{
			return new HashSet<int>(_currentUserInformationProvider.GetActiveBranches().Select(b => b.Id));
		}

		protected virtual void _(Events.RowDeleting<PMProforma> e)
		{
			ISet<int> userBranches = GetUserBranches();

			using (new PXReadBranchRestrictedScope())
			{
				var lineBranches = new HashSet<int>(
					SelectFrom<PMProformaLine>
						.Where<PMProformaLine.refNbr.IsEqual<PMProforma.refNbr.FromCurrent>
							.And<PMProformaLine.revisionID.IsEqual<PMProforma.revisionID.FromCurrent>>>
						.View
						.Select(this)
						.Select(line => ((PMProformaLine)line).BranchID)
						.Where(branchID => branchID.HasValue)
						.Select(branchID => branchID.Value));

				//checks line branch is deleted or not
				foreach (var lineBranch in lineBranches)
				{
					Branch branch = Branch.PK.Find(this, lineBranch);
					if (branch == null)
					{
						userBranches.Add(lineBranch);
					}
				}

				if (!userBranches.IsSupersetOf(lineBranches))
				{		
					throw new PXOperationCompletedSingleErrorException(Messages.ProformaDeletingRestriction);
				}
			}
		}

		protected virtual void _(Events.RowDeleting<PMProformaTransactLine> e)
		{
			ISet<int> userBranches = GetUserBranches();

			using (new PXReadBranchRestrictedScope())
			{
				PMProformaTransactLine proformaLine = e.Row;
				var transactionBranches = new HashSet<int>(
					SelectFrom<PMTran>
						.Where
							<PMTran.proformaRefNbr.IsEqual<@P.AsString>.And
							<PMTran.proformaLineNbr.IsEqual<@P.AsInt>>>
						.View
						.Select(this, proformaLine.RefNbr, proformaLine.LineNbr)
						.Select(line => ((PMTran)line).BranchID)
						.Where(branchID => branchID.HasValue)
						.Select(branchID => branchID.Value));

				//checks line branch is deleted or not
				foreach (var lineBranch in transactionBranches)
				{
					Branch branch = Branch.PK.Find(this, lineBranch);
					if (branch == null)
					{
						userBranches.Add(lineBranch);
					}
				}

				if (!userBranches.IsSupersetOf(transactionBranches))
				{
					throw new PXOperationCompletedSingleErrorException(Messages.ProformaLineDeletingRestriction);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMProformaTransactLine> e)
		{
			bool referencesAlreadyDeleted = false; //for the entire document.

			if (Document.Current != null && Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Deleted)
			{
				referencesAlreadyDeleted = true;
			}

			if (!referencesAlreadyDeleted)
			{
				var selectReferencedTransactions = new PXSelect<PMTran,
					Where<PMTran.proformaRefNbr, Equal<Required<PMTran.proformaRefNbr>>,
					And<PMTran.proformaLineNbr, Equal<Required<PMTran.proformaLineNbr>>>>>(this);

				foreach (PMTran tran in selectReferencedTransactions.Select(e.Row.RefNbr, e.Row.LineNbr))
				{
					Unbill(tran);
				}
			}

			cachedReferencedTransactions = null;
			SubtractFromInvoiced(e.Row);
			SubtractFromDraftRetained(e.Row);
			SubtractFromTotalRetained(e.Row);
			SubtractPerpaymentRemainder(e.Row, -1);
		}

		protected virtual void _(Events.FieldSelecting<PMTran, PMTran.projectCuryID> e)
		{
			if (Project.Current != null)
				e.ReturnValue = Project.Current.CuryID;
		}

		protected virtual void _(Events.RowDeleted<PMTran> e)
		{
			PMProformaTransactLine key = new PMProformaTransactLine();
			key.RefNbr = Document.Current.RefNbr;
			key.RevisionID = Document.Current.RevisionID;
			key.LineNbr = e.Row.ProformaLineNbr;

			PMProformaTransactLine line = TransactionLines.Locate(key);

			if (line != null)
			{
				line.CuryBillableAmount -= e.Row.ProjectCuryInvoicedAmount.GetValueOrDefault();
				line.CuryAmount -= e.Row.ProjectCuryInvoicedAmount.GetValueOrDefault();

				line.BillableQty -= e.Row.InvoicedQty.GetValueOrDefault();
				line.Qty -= e.Row.InvoicedQty.GetValueOrDefault();

				TransactionLines.Update(line);
			}

			e.Cache.SetStatus(e.Row, PXEntryStatus.Updated);

			e.Row.Billed = false;
			e.Row.BilledDate = null;
			e.Row.BillingID = null;
			e.Row.ProformaRefNbr = null;
			e.Row.ProformaLineNbr = null;
		}

		#region CurrencyInfo events

		protected virtual void _(Events.FieldDefaulting<CurrencyInfo, CurrencyInfo.baseCuryID> e)
		{
			var branchID = Accessinfo.BranchID;
			if (Document.Current != null && Document.Current.BranchID != null)
			{
				branchID = Document.Current.BranchID;
			}

			e.NewValue = GetBaseCurency(branchID);
			e.Cancel = true;
		}

		protected virtual string GetBaseCurency(int? branchID)
		{			
			return ServiceLocator.Current.GetInstance<Func<PXGraph, CM.Extensions.IPXCurrencyService>>()(this).BaseCuryID(branchID);
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			Action<PXCache, PMProformaLine> syncBudgets = (cache, tran) =>
			{
				decimal newLineTotal = 0;
				decimal newPrepaidAmount = 0;
				if (e.Row.CuryRate != null)
				{
					newLineTotal = e.Row.CuryConvBase(tran.CuryLineTotal.GetValueOrDefault());
					newPrepaidAmount = e.Row.CuryConvBase(tran.CuryPrepaidAmount.GetValueOrDefault());
				}
				var newTran = cache.CreateCopy(tran) as PMProformaLine;
				newTran.LineTotal = newLineTotal;
				newTran.PrepaidAmount = newPrepaidAmount;

				decimal oldLineTotal = 0;
				decimal oldPrepaidAmount = 0;
				if (e.OldRow.CuryRate != null)
				{
					oldLineTotal = e.OldRow.CuryConvBase(tran.CuryLineTotal.GetValueOrDefault());
					oldPrepaidAmount = e.OldRow.CuryConvBase(tran.CuryPrepaidAmount.GetValueOrDefault());
				}
				var oldTran = cache.CreateCopy(tran) as PMProformaLine;
				oldTran.LineTotal = oldLineTotal;
				oldTran.PrepaidAmount = oldPrepaidAmount;

				SyncBudgets(cache, newTran, oldTran);
			};

			foreach (PMProformaLine tran in TransactionLines.Select())
				syncBudgets(TransactionLines.Cache, tran);

			foreach (PMProformaLine tran in ProgressiveLines.Select())
				syncBudgets(ProgressiveLines.Cache, tran);
		}

		#endregion

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.taskID> e)
		{
			e.Cache.SetDefaultExt<PMProformaTransactLine.taxCategoryID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProformaTransactLine, PMProformaTransactLine.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMProformaTransactLine.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMProformaTransactLine.taxCategoryID>(e.Row);
			e.Cache.SetDefaultExt<PMProformaTransactLine.accountID>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.locationID> e)
		{
			e.Cache.SetDefaultExt<PMProforma.branchID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.workgroupID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.ownerID>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.externalTaxExemptionNumber>(e.Row);
			e.Cache.SetDefaultExt<PMProforma.avalaraCustomerUsageType>(e.Row);

			ARShippingAddressAttribute.DefaultRecord<PMProforma.shipAddressID>(e.Cache, e.Row);
			ARShippingContactAttribute.DefaultRecord<PMProforma.shipContactID>(e.Cache, e.Row);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.projectID> e)
		{
			if (e.Row == null)
				return;

			Project.Cache.Clear();
			e.Cache.SetValueExt<PMProforma.customerID>(e.Row, Project.Current?.CustomerID);
			e.Cache.SetValueExt<PMProforma.curyID>(e.Row, Project.Current?.BillingCuryID);

			Customer.Cache.Clear();
			var locationId = Project.Current?.LocationID
				?? Customer.Current?.DefLocationID;
			e.Cache.SetValueExt<PMProforma.locationID>(e.Row, locationId);
			e.Cache.SetValueExt<PMProforma.billAddressID>(e.Row, Project.Current?.BillAddressID);
			e.Cache.SetValueExt<PMProforma.billContactID>(e.Row, Project.Current?.BillContactID);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.aRInvoiceDocType> e)
		{
			if (e.Row == null)
				return;

			UpdateInvoice((string)e.OldValue, e.Row.ARInvoiceRefNbr,
				(string)e.NewValue, null);
			e.Cache.SetValue<PMProforma.aRInvoiceRefNbr>(e.Row, null);
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.aRInvoiceRefNbr> e)
		{
			if (e.Row == null)
				return;

			var matchingRecords = SelectFrom<PMBillingRecord>
				.Where<PMBillingRecord.proformaRefNbr.IsEqual<P.AsString>>
				.View.Select(this, e.Row.RefNbr);

			foreach (PMBillingRecord record in matchingRecords)
			{
				record.ARDocType = e.Row.ARInvoiceDocType;
				record.ARRefNbr = e.Row.ARInvoiceRefNbr;
				BillingRecord.Update(record);
			}

			UpdateInvoice(e.Row.ARInvoiceDocType, (string) e.OldValue,
				e.Row.ARInvoiceDocType, (string) e.NewValue);
		}

		private void UpdateInvoice(string oldType, string oldNbr, string newType, string newNbr)
		{
			if (!string.IsNullOrEmpty(oldType) && !string.IsNullOrEmpty(oldNbr))
			{
				UpdateInvoice(oldType, oldNbr, false);
			}

			if (!string.IsNullOrEmpty(newType) && !string.IsNullOrEmpty(newNbr))
			{
				UpdateInvoice(newType, newNbr, true);
			}
		}

		private void UpdateInvoice(string arDocType, string arRefNbr, bool proformaExists)
		{
			var currentInvoice = ARInvoice.PK.Find(this, arDocType, arRefNbr);
			if (currentInvoice == null)
				return;

			currentInvoice.ProformaExists = proformaExists;
			Invoices.Update(currentInvoice);
		}

		protected virtual void _(Events.RowPersisting<PMProforma> e)
		{
			if (e.Row == null || e.Operation == PXDBOperation.Delete)
				return;

			if (e.Row.FinPeriodID == null)
			{
				e.Cache.RaiseExceptionHandling<PMProforma.finPeriodID>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMProforma.finPeriodID)}]"));
			}
		}

		protected virtual void _(Events.RowPersisting<PMProformaProgressLine> e)
		{
			if (e.Row == null || e.Operation == PXDBOperation.Delete)
				return;

			if (IsMigrationMode() && e.Row.ProgressBillingBase == null)
			{
				e.Cache.RaiseExceptionHandling<PMProformaProgressLine.progressBillingBase>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(PMProformaProgressLine.progressBillingBase)}]"));
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProforma, PMProforma.branchID> e)
		{
			if (e.Row != null)
			{
				Location customerLocation = Location.View.SelectSingleBound(new object[] { e.Row }) as Location;
				if (customerLocation != null && customerLocation.CBranchID != null)
				{
					e.NewValue = customerLocation.CBranchID;
				}
				else
				{
					e.NewValue = Accessinfo.BranchID;
				}
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProforma, PMProforma.isMigratedRecord> e)
		{
			if (e.Row != null)
			{
				e.NewValue = IsMigrationMode();
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMProforma, PMProforma.taxZoneID> e)
		{
			if (e.Row != null)
			{
				e.NewValue = GetDefaultTaxZone(e.Row);
			}
		}

		public virtual string GetDefaultTaxZone(PMProforma row)
		{
			string result = null;
			if (row != null)
			{
				Location customerLocation = Location.View.SelectSingleBound(new object[] { row }) as Location;
				if (customerLocation != null)
				{
					if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
					{
						result = customerLocation.CTaxZoneID;
					}
					else
					{
						BAccount companyAccount = PXSelectJoin<BAccountR, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
						if (companyAccount != null)
						{
							Location companyLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, companyAccount.BAccountID, companyAccount.DefLocationID);
							if (companyLocation != null)
								result = companyLocation.VTaxZoneID;
						}
					}
				}
			}

			return result;
		}

		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (SuppressRowSeleted)
				return;

			if (Project.Current != null)
			{
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.costCodeID>(ProgressiveLines.Cache, null, Project.Current.BudgetLevel == BudgetLevels.CostCode);
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, IsMigrationMode() || !CostCodeAttribute.UseCostCode() && Project.Current.BudgetLevel != BudgetLevels.Task);
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.retainagePct>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract);
				PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.retainagePct>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyRetainage>(ProgressiveLines.Cache, null, Project.Current.RetainageMode != RetainageModes.Contract);

				PXUIFieldAttribute.SetVisible<PMProformaLine.curyAllocatedRetainedAmount>(ProgressiveLines.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);
				PXUIFieldAttribute.SetVisibility<PMProformaLine.curyAllocatedRetainedAmount>(ProgressiveLines.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract ? PXUIVisibility.Visible : PXUIVisibility.Invisible);

				PXUIFieldAttribute.SetVisible<PMProforma.curyAllocatedRetainedTotal>(e.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);
				PXUIFieldAttribute.SetVisible<PMProforma.retainagePct>(e.Cache, null, Project.Current.RetainageMode == RetainageModes.Contract);

			}
			else
			{
				PXUIFieldAttribute.SetVisible<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, true);
			}

			//Migration Mode:
			PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.taskID>(TransactionLines.Cache, null, IsMigrationMode());
			PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.inventoryID>(TransactionLines.Cache, null, IsMigrationMode());
			PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.accountGroupID>(ProgressiveLines.Cache, null, IsMigrationMode());
			PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.taskID>(ProgressiveLines.Cache, null, IsMigrationMode());
			PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, IsMigrationMode());
			ProgressiveLines.Cache.AllowInsert = IsMigrationMode();

			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyMaxAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyAvailableAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyOverflowAmount>(TransactionLines.Cache, null, IsLimitsEnabled());
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyMaxAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyAvailableAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyOverflowAmount>(ProgressiveLines.Cache, null, IsLimitsEnabled() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			
			PXUIFieldAttribute.SetVisible<PMProforma.curyID>(e.Cache, e.Row, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.inventoryID>(ProgressiveLines.Cache, null, !CostCodeAttribute.UseCostCode() ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaProgressLine.curyPrepaidAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyPrepaidAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PMProformaProgressLine.curyPrepaidAmount>(ProgressiveLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyPrepaidAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisibility<PMProformaTransactLine.curyAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisible<PMProformaTransactLine.curyAmount>(TransactionLines.Cache, null, Project.Current?.PrepaymentEnabled == true);
			PXUIFieldAttribute.SetVisible<PMProforma.externalTaxExemptionNumber>(e.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>());
			PXUIFieldAttribute.SetVisible<PMProforma.avalaraCustomerUsageType>(e.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>());

			PXUIFieldAttribute.SetRequired<PMProformaProgressLine.progressBillingBase>(ProgressiveLines.Cache, IsMigrationMode());

			autoApplyPrepayments.SetEnabled(Project.Current?.PrepaymentEnabled == true);
			Revisions.Cache.AllowSelect = PXAccess.FeatureInstalled<FeaturesSet.construction>();

			if (e.Row != null)
			{
				var isMigratedRow = e.Row.IsMigratedRecord == true;
				PXUIFieldAttribute.SetVisible<PMProforma.isMigratedRecord>(e.Cache, e.Row, isMigratedRow);
				uploadFromBudget.SetEnabled(e.Row.Hold == true && isMigratedRow);
				PXUIFieldAttribute.SetEnabled<PMProforma.aRInvoiceDocType>(Document.Cache, e.Row, IsMigrationMode() && isMigratedRow);
				PXUIFieldAttribute.SetEnabled<PMProforma.aRInvoiceRefNbr>(Document.Cache, e.Row, IsMigrationMode() && isMigratedRow);

				var isProjectIDEnabled = IsMigrationMode()
					&& isMigratedRow
					&& e.Row.Hold == true
					&& ProgressiveLines.Select().Count == 0;
				PXUIFieldAttribute.SetEnabled<PMProforma.projectID>(Document.Cache, e.Row, isProjectIDEnabled);

				uploadUnbilled.SetEnabled(e.Row.Hold == true);
				bool isEditable = CanEditDocument(e.Row);

				Document.Cache.AllowDelete = isEditable;
				ProgressiveLines.Cache.AllowUpdate = e.Row.Hold == true;
				ProgressiveLines.Cache.AllowDelete = e.Row.Hold == true;
				TransactionLines.Cache.AllowInsert = e.Row.Hold == true;
				TransactionLines.Cache.AllowUpdate = e.Row.Hold == true;
				TransactionLines.Cache.AllowDelete = e.Row.Hold == true;
				Details.Cache.AllowDelete = e.Row.Hold == true;
				Billing_Address.Cache.AllowUpdate = isEditable;
				Billing_Contact.Cache.AllowUpdate = isEditable;
				Shipping_Address.Cache.AllowUpdate = isEditable;
				Shipping_Contact.Cache.AllowUpdate = isEditable;
				
				PXUIFieldAttribute.SetEnabled<PMProforma.invoiceDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.invoiceNbr>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.finPeriodID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.description>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.curyID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.branchID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.taxZoneID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.termsID>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.dueDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.discDate>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.externalTaxExemptionNumber>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.avalaraCustomerUsageType>(e.Cache, e.Row, isEditable);
				PXUIFieldAttribute.SetEnabled<PMProforma.locationID>(e.Cache, e.Row, e.Row.Hold == true);
								
				PXUIFieldAttribute.SetDisplayName<PMRevenueBudget.curyRevisedAmount>(Caches[typeof(PMRevenueBudget)], BillingInAnotherCurrency ? Messages.RevisedBudgetedAmountInProjectCurrency : Messages.RevisedBudgetedAmount);
				PXUIFieldAttribute.SetDisplayName<PMRevenueBudget.curyInvoicedAmount>(Caches[typeof(PMRevenueBudget)], BillingInAnotherCurrency ? Messages.DraftInvoiceAmountInProjectCurrency : Messages.DraftInvoiceAmount);
				PXUIFieldAttribute.SetDisplayName<PMRevenueBudget.curyActualAmount>(Caches[typeof(PMRevenueBudget)], BillingInAnotherCurrency ? Messages.ActualAmountInProjectCurrency : Messages.ActualAmount);
			}
		}

		public virtual bool CanEditDocument(PMProforma doc)
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
				if (doc.Rejected == true)
					return false;

				if (doc.Approved == true)
				{
					//document is either approved or no approval is not required.
					if (PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>() && Setup.Current.ProformaApprovalMapID != null)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				else 
				{
					return false;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaOverflow> e)
		{
			PXUIFieldAttribute.SetVisible<PMProformaOverflow.curyOverflowTotal>(e.Cache, null, IsLimitsEnabled());

			if (e.Row.CuryOverflowTotal > 0)
			{
				PXUIFieldAttribute.SetWarning<PMProformaOverflow.curyOverflowTotal>(e.Cache, e.Row, Messages.OverlimitHint);
			}
			else
			{
				PXUIFieldAttribute.SetError<PMProformaOverflow.curyOverflowTotal>(e.Cache, e.Row, null);
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaProgressLine> e)
		{
			if (e.Row != null)
			{
				string billingBase = e.Row.ProgressBillingBase;

				decimal curyPreviouslyInvoiced = 0.0m;
				decimal previouslyInvoiced = 0.0m;

				if(e.Row.TaskID != null
					&& billingBase == ProgressBillingBase.Amount)
				{
					var totals = TotalsCounter.GetAmountBaseTotals(this, Document.Current.RefNbr, e.Row);
					curyPreviouslyInvoiced = totals.CuryLineTotal;
					previouslyInvoiced = totals.LineTotal;
				}
				else if(e.Row.TaskID != null
					&& billingBase == ProgressBillingBase.Quantity)
				{
					var totals = TotalsCounter.GetQuantityBaseTotals(this, Document.Current.RefNbr, e.Row);
					curyPreviouslyInvoiced = totals.CuryLineTotal;
					previouslyInvoiced = totals.LineTotal;

					// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Initializing non-db field]
					e.Row.PreviouslyInvoicedQty = totals.QuantityTotal;

					PMRevenueBudget budget = SelectRevenueBudget(e.Row);

					if (budget != null)
					{
						if (INUnitAttribute.TryConvertGlobalUnits(this, budget.UOM, e.Row.UOM, budget.ActualQty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal qty))
					{
						// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Initializing non-db field]
						e.Row.ActualQty = qty;
					}
					else
					{
						string message = PXMessages.LocalizeFormatNoPrefix(Messages.ActualQtyUomConversionNotFound, budget.UOM, e.Row.UOM);
						PXUIFieldAttribute.SetWarning<PMProformaProgressLine.actualQty>(e.Cache, e.Row, message);
					}
				}
				}

				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Initializing non-db field]
				e.Row.CuryPreviouslyInvoiced = curyPreviouslyInvoiced;
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [Initializing non-db field]
				e.Row.PreviouslyInvoiced = previouslyInvoiced;

				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.qty>(e.Cache, e.Row, billingBase == ProgressBillingBase.Quantity);

				bool isAmountBase = billingBase == ProgressBillingBase.Amount;
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyAmount>(e.Cache, e.Row, isAmountBase);
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyLineTotal>(e.Cache, e.Row, isAmountBase);
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.curyUnitPrice>(e.Cache, e.Row, false);
				PXUIFieldAttribute.SetEnabled<PMProformaProgressLine.progressBillingBase>(e.Cache, e.Row, IsMigrationMode());
			}
		}

		protected virtual void _(Events.RowSelected<PMProformaTransactLine> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.curyPrepaidAmount>(e.Cache, e.Row, IsPrepaidAmountEnabled(e.Row));
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.option>(e.Cache, e.Row, e.Row.IsPrepayment != true);
				Details.AllowSelect = e.Row.IsPrepayment != true;

				bool adjustment = IsAdjustment(e.Row);

				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.taskID>(e.Cache, e.Row, adjustment);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.inventoryID>(e.Cache, e.Row, adjustment);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.option>(e.Cache, e.Row, !adjustment);

				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.curyLineTotal>(e.Cache, e.Row, e.Row.Option != PMProformaLine.option.Writeoff);
				PXUIFieldAttribute.SetEnabled<PMProformaTransactLine.qty>(e.Cache, e.Row, e.Row.Option != PMProformaLine.option.Writeoff);

			}
		}

		protected virtual void _(Events.RowUpdated<PMProformaTransactLine> e)
		{
			SyncBudgets(e.Cache, e.Row, e.OldRow);
			
			if (e.Row.CuryMaxAmount != null && GetAmountInProjectCurrency(e.Row.CuryLineTotal) != GetAmountInProjectCurrency(e.OldRow.CuryLineTotal))
			{
				TransactionLines.View.RequestRefresh();
			}
		}

		protected virtual void _(Events.RowUpdated<PMProformaProgressLine> e)
		{
			SyncBudgets(e.Cache, e.Row, e.OldRow);

			if (!RecalculatingContractRetainage && e.Row.CuryLineTotal != e.OldRow.CuryLineTotal)
				RecalculateRetainageOnDocument(Project.Current);
		}

		private void SyncBudgets(PXCache cache, PMProformaLine row, PMProformaLine oldRow)
		{
			Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(cache, row, row.AccountID) as Account;
			Account oldRevenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(cache, oldRow, oldRow.AccountID) as Account;

			if (oldRevenueAccount != null)
			{
				SubtractFromInvoiced(oldRow, oldRevenueAccount.AccountGroupID);
				SubtractFromDraftRetained(oldRow, oldRevenueAccount.AccountGroupID);
				SubtractFromTotalRetained(oldRow, oldRevenueAccount.AccountGroupID);
			}

			if (revenueAccount != null)
			{
				AddToInvoiced(row, revenueAccount.AccountGroupID);
				AddToDraftRetained(row, revenueAccount.AccountGroupID);
				AddToTotalRetained(row, revenueAccount.AccountGroupID);
			}

			if (row.CuryPrepaidAmount != oldRow.CuryPrepaidAmount || row.PrepaidAmount != oldRow.PrepaidAmount)
			{
				SubtractPerpaymentRemainder(oldRow, -1);
				SubtractPerpaymentRemainder(row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.branchID> e)
		{
			e.Cache.SetDefaultExt<PMProforma.taxZoneID>(e.Row);

			foreach (PMTaxTran taxTran in Taxes.Select())
			{
				if (Taxes.Cache.GetStatus(taxTran) == PXEntryStatus.Notchanged)
				{
					Taxes.Cache.SetStatus(taxTran, PXEntryStatus.Updated);
				}
			}
		}
		
		#region PMTaxTran Events
		protected virtual void _(Events.FieldDefaulting<PMTaxTran, PMTaxTran.taxZoneID> e)
		{
			if (Document.Current != null)
			{
				e.NewValue = Document.Current.TaxZoneID;
				e.Cancel = true;
			}
		}

		protected virtual void _(Events.RowSelected<PMTaxTran> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(e.Cache, e.Row, e.Cache.GetStatus(e.Row) == PXEntryStatus.Inserted);
		}

		protected virtual void _(Events.RowInserting<PMTaxTran> e)
		{
			PXParentAttribute.SetParent(e.Cache, e.Row, typeof(PMProforma), this.Document.Current);
		}

		protected virtual void _(Events.ExceptionHandling<PMTaxTran, PMTaxTran.taxZoneID> e)
		{
			Exception ex = e.Exception as PXSetPropertyException;
			if (ex != null)
			{
				Document.Cache.RaiseExceptionHandling<PMProforma.taxZoneID>(Document.Current, null, ex);
			}
		}
		#endregion

		protected virtual void _(Events.RowPersisted<PMBudgetAccum> e)
		{
			//to fix discrepancy on transactions persisting
			this.Caches[typeof(PMRevenueBudget)].Clear();
		}

		protected virtual void PMProforma_InvoiceDate_FieldUpdated(Events.FieldUpdated<PMProforma.invoiceDate> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMProforma.finPeriodID>(e.Row);
			}
		}

		protected virtual void PMProforma_BranchID_FieldUpdated(Events.FieldUpdated<PMProforma.branchID> e)
		{
			if (e.Row != null)
			{
				e.Cache.SetDefaultExt<PMProforma.finPeriodID>(e.Row);
			}
		}

		#endregion

		public override void Clear()
		{
			cachedReferencedTransactions = null;
			base.Clear();
		}
		
		protected virtual void ValidateLimitsOnUnhold(PMProforma row)
		{
			if (Overflow.Current.CuryOverflowTotal > 0 && Setup.Current.OverLimitErrorLevel == OverLimitValidationOption.Error)
			{
				throw new PXRowPersistingException(typeof(PMProformaOverflow.overflowTotal).Name, null, Messages.Overlimit);
			}
		}

		public virtual void ReleaseDocument(PMProforma doc)
		{
			if (doc == null)
				throw new ArgumentNullException();

			if (doc.Released == true)
				throw new PXException(EP.Messages.AlreadyReleased);

			if (IsMigrationMode() && doc.IsMigratedRecord == false)
				throw new PXException(Messages.DeactivateMigrationModeToReleaseProforma);

			if (!IsMigrationMode() && doc.IsMigratedRecord == true)
				throw new PXException(Messages.ActivateMigrationModeToReleaseProforma);

			// Don't create AR invoice automatically in migration mode.
			if (IsMigrationMode())
			{
				foreach (PMProformaProgressLine line in ProgressiveLines.Select())
				{
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceDocType>(line, doc.ARInvoiceDocType);
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceRefNbr>(line, doc.ARInvoiceRefNbr);
					ProgressiveLines.Cache.SetValue<PMProformaProgressLine.released>(line, true);
					ProgressiveLines.Cache.MarkUpdated(line, assertError: true);
				}

				doc.Released = true;
				Document.Update(doc);
				Save.Press();
			}
			else
			{
				CheckMigrationMode();

				ValidatePrecedingBeforeRelease(doc);
				ValidatePrecedingInvoicesBeforeRelease(doc);
				ValidateBranchBeforeRelease(doc);

				PMProject project = (PMProject)Project.View.SelectSingleBound(new object[] { doc });
				PMRegister reversalDoc = null;
				ARInvoice invoice = null;
				ARInvoice creditMemo = null;

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					creditMemo = ProcessRevision();

					RegisterEntry pmEntry = PXGraph.CreateInstance<RegisterEntry>();
					pmEntry.Clear();
					pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					reversalDoc = (PMRegister)pmEntry.Document.Cache.Insert();
					reversalDoc.OrigDocType = PMOrigDocType.AllocationReversal;
					reversalDoc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversalOnARInvoiceGeneration);
					pmEntry.Document.Current = reversalDoc;

					PMBillEngine engine = PXGraph.CreateInstance<PMBillEngine>();

					ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					invoiceEntry.Clear();
					invoiceEntry.ARSetup.Current.RequireControlTotal = false;
					invoiceEntry.FieldVerifying.AddHandler<ARTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARInvoice.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					invoiceEntry.FieldVerifying.AddHandler<ARTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

					invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
					invoiceEntry.RowPersisted.AddHandler<ARInvoice>(delegate (PXCache sender, PXRowPersistedEventArgs e)
					{
						if (e.TranStatus == PXTranStatus.Open)
						{
							var row = (ARInvoice)e.Row;
							doc.ARInvoiceDocType = row.DocType;
							doc.ARInvoiceRefNbr = row.RefNbr;
							reversalDoc.OrigNoteID = row.NoteID;
						}
					});

					int mult = doc.DocTotal >= 0 ? 1 : -1;
					invoice.DocType = mult == 1 ? ARDocType.Invoice : ARDocType.CreditMemo;

					invoice.ProjectID = doc.ProjectID;
					invoice = invoiceEntry.Document.Insert(invoice);

					invoice.CustomerID = doc.CustomerID;
					invoice.DocDate = doc.InvoiceDate;
					invoice.DocDesc = doc.Description;
					invoice.DueDate = doc.DueDate;
					invoice.DiscDate = doc.DiscDate;
					invoice.TermsID = doc.TermsID;
					invoice.TaxZoneID = doc.TaxZoneID;
					invoice.CuryID = doc.CuryID;
					invoice.CuryInfoID = doc.CuryInfoID;
					invoice.CustomerLocationID = doc.LocationID;
					invoice.FinPeriodID = doc.FinPeriodID;
					invoice.BranchID = doc.BranchID;
					invoice.ProformaExists = true;
					invoice.InvoiceNbr = doc.InvoiceNbr;
					if (doc.RetainageTotal != 0m)
					{
						invoice.RetainageApply = true;
					}

					if (Project.Current.RetainageMode != RetainageModes.Normal)
					{
						invoice.PaymentsByLinesAllowed = true;
					}

					invoice = invoiceEntry.Document.Update(invoice);

					invoice.TaxCalcMode = TX.TaxCalculationMode.TaxSetting;
					invoice.ExternalTaxExemptionNumber = doc.ExternalTaxExemptionNumber;
					invoice.AvalaraCustomerUsageType = doc.AvalaraCustomerUsageType;

					if (!string.IsNullOrEmpty(doc.FinPeriodID))
					{
						invoiceEntry.Document.Cache.SetValue<ARInvoice.finPeriodID>(invoice, doc.FinPeriodID);
					}

					invoiceEntry.currencyinfo.Current = invoiceEntry.currencyinfo.Select();

					PMAddress billAddressPM = (PMAddress)PXSelect<PMAddress, Where<PMAddress.addressID, Equal<Required<PMProforma.billAddressID>>>>.Select(this, doc.BillAddressID);
					if (billAddressPM != null && billAddressPM.IsDefaultAddress != true)
					{
						ARAddress addressAR = invoiceEntry.Billing_Address.Select();
						invoiceEntry.Billing_Address.Cache.SetValueExt<ARAddress.overrideAddress>(addressAR, true);
						CopyPMAddressToARInvoice(invoiceEntry, billAddressPM, invoiceEntry.Billing_Address.Current);
					}

					PMShippingAddress shipAddressPM = (PMShippingAddress)PXSelect<PMShippingAddress,
						Where<PMShippingAddress.addressID, Equal<Required<PMProforma.shipAddressID>>>>.Select(this, doc.ShipAddressID);
					if (shipAddressPM != null && shipAddressPM.IsDefaultAddress != true)
					{
						ARShippingAddress shipAddressAR = invoiceEntry.Shipping_Address.Select();
						invoiceEntry.Shipping_Address.Cache.SetValueExt<ARShippingAddress.overrideAddress>(shipAddressAR, true);
						CopyPMAddressToARInvoice(invoiceEntry, shipAddressPM, invoiceEntry.Shipping_Address.Current);
					}

					PMContact billContactPM = (PMContact)PXSelect<PMContact, Where<PMContact.contactID, Equal<Required<PMProforma.billContactID>>>>.Select(this, doc.BillContactID);
					if (billContactPM != null && billContactPM.IsDefaultContact != true)
					{
						ARContact contactAR = invoiceEntry.Billing_Contact.Select();
						invoiceEntry.Billing_Contact.Cache.SetValueExt<ARContact.overrideContact>(contactAR, true);
						CopyPMContactToARInvoice(invoiceEntry, billContactPM, invoiceEntry.Billing_Contact.Current);
					}

					PMShippingContact shipContactPM = (PMShippingContact)PXSelect<PMShippingContact,
						Where<PMShippingContact.contactID, Equal<Required<PMProforma.shipContactID>>>>.Select(this, doc.ShipContactID);
					if (shipContactPM != null && shipContactPM.IsDefaultContact != true)
					{
						ARShippingContact shipContactAR = invoiceEntry.Shipping_Contact.Select();
						invoiceEntry.Shipping_Contact.Cache.SetValueExt<ARShippingContact.overrideContact>(shipContactAR, true);
						CopyPMContactToARInvoice(invoiceEntry, shipContactPM, invoiceEntry.Shipping_Contact.Current);
					}

					if (string.IsNullOrEmpty(doc.TaxZoneID))
					{
						TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.NoCalc);
					}
					else
					{
						if (!RecalculateTaxesOnRelease())
							TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);
					}


					ARTran artran = null;
					List<PMProformaProgressLine> processedProgressiveLines = new List<PMProformaProgressLine>();
					foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.View.SelectMultiBound(new[] { doc }))
					{
						PMProformaProgressLine line = (PMProformaProgressLine)res;
						artran = InsertTransaction(invoiceEntry, line, mult);
						PXNoteAttribute.CopyNoteAndFiles(ProgressiveLines.Cache, line, invoiceEntry.Transactions.Cache, artran);
						line.ARInvoiceLineNbr = artran.LineNbr;
						ProgressiveLines.Update(line);
						processedProgressiveLines.Add(line);
					}

					Dictionary<int, List<PMTran>> pmtranByProformalLineNbr = GetReferencedTransactions();

					List<PMProformaTransactLine> processedTransactionLines = new List<PMProformaTransactLine>();
					foreach (PMProformaTransactLine line in TransactionLines.View.SelectMultiBound(new[] { doc }).RowCast<PMProformaTransactLine>())
					{
						if (line.Option != PMProformaLine.option.Writeoff)
						{
							artran = InsertTransaction(invoiceEntry, line, mult);
							PXNoteAttribute.CopyNoteAndFiles(TransactionLines.Cache, line, invoiceEntry.Transactions.Cache, artran);

							TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceLineNbr>(line, artran.LineNbr);
							TransactionLines.Cache.MarkUpdated(line, assertError: true);
							processedTransactionLines.Add(line);
						}
						else
						{
							List<PMTran> list;
							if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list))
							{
								foreach (PMTran tran in list)
								{
									if (string.IsNullOrEmpty(tran.AllocationID))
									{
										//direct cost transaction
										PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, tran);
										AllReferencedTransactions.Update(tran);
									}
								}
							}
						}

						List<PMTran> list2;
						if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list2))
						{
							foreach (PMTran original in list2)
							{
								if (original != null && original.Reverse == PMReverse.OnInvoiceGeneration)
								{
									foreach (PMTran tran in engine.ReverseTran(original))
									{
										tran.Date = doc.InvoiceDate;
										tran.FinPeriodID = null;
										pmEntry.InsertTransactionWithManuallyChangedCurrencyInfo(tran);
									}
								}
							}
						}
					}

					TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(invoiceEntry.Transactions.Cache, null, PX.Objects.TX.TaxCalc.ManualCalc);
					if (artran != null)
						invoiceEntry.Transactions.Cache.RaiseRowUpdated(artran, artran);

					if (!RecalculateTaxesOnRelease() && !IsExternalTax(Document.Current.TaxZoneID))
					{
						var curyRetainedTaxAmtSums = Tax_Rows.Select().RowCast<PMTax>()
							.GroupBy(x => x.TaxID)
							.ToDictionary(x => x.Key, x => x.Sum(i => i.CuryRetainedTaxAmt.GetValueOrDefault()));

						List<Tuple<ARTaxTran, PMTaxTran>> manualTaxes = new List<Tuple<ARTaxTran, PMTaxTran>>();
						foreach (PMTaxTran tax in Taxes.Select())
						{
							ARTaxTran new_artax = new ARTaxTran { TaxID = tax.TaxID };
							new_artax = invoiceEntry.Taxes.Insert(new_artax);
							manualTaxes.Add(new Tuple<ARTaxTran, PMTaxTran>(new_artax, tax));
						}

						foreach (Tuple<ARTaxTran, PMTaxTran> manualTax in manualTaxes)
						{
							if (manualTax.Item1 != null)
							{
								manualTax.Item1.TaxRate = manualTax.Item2.TaxRate;
								manualTax.Item1.CuryTaxableAmt = mult * manualTax.Item2.CuryTaxableAmt;
								manualTax.Item1.CuryExemptedAmt = mult * manualTax.Item2.CuryExemptedAmt;
								manualTax.Item1.CuryTaxAmt = mult * manualTax.Item2.CuryTaxAmt;
								manualTax.Item1.CuryRetainedTaxableAmt = mult * manualTax.Item2.CuryRetainedTaxableAmt.GetValueOrDefault();
								manualTax.Item1.CuryRetainedTaxAmt = mult * manualTax.Item2.CuryRetainedTaxAmt.GetValueOrDefault();

								if (curyRetainedTaxAmtSums.TryGetValue(manualTax.Item2.TaxID, out decimal curyRetainedTaxAmtSum))
								{
									manualTax.Item1.CuryRetainedTaxAmtSumm = mult * curyRetainedTaxAmtSum;
								}

								invoiceEntry.Taxes.Update(manualTax.Item1);
							}
						}
					}

					invoice.Hold = ARSetup.Current.HoldEntry == true;
					invoice = invoiceEntry.Document.Update(invoice);

					invoiceEntry.Save.Press();
					doc.Released = true;
					Document.Update(doc);

					PMBillingRecord billingRecord = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMProforma.refNbr>>>>.Select(this, doc.RefNbr);
					if (billingRecord != null)
					{
						billingRecord.ARDocType = doc.ARInvoiceDocType;
						billingRecord.ARRefNbr = doc.ARInvoiceRefNbr;

						BillingRecord.Update(billingRecord);
					}

					foreach (PMProformaProgressLine line in processedProgressiveLines)
					{
						ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceDocType>(line, doc.ARInvoiceDocType);
						ProgressiveLines.Cache.SetValue<PMProformaProgressLine.aRInvoiceRefNbr>(line, doc.ARInvoiceRefNbr);
						ProgressiveLines.Cache.SetValue<PMProformaProgressLine.released>(line, true);
						ProgressiveLines.Cache.MarkUpdated(line, assertError: true);
					}

					foreach (PMProformaTransactLine line in processedTransactionLines)
					{
						TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceDocType>(line, doc.ARInvoiceDocType);
						TransactionLines.Cache.SetValue<PMProformaTransactLine.aRInvoiceRefNbr>(line, doc.ARInvoiceRefNbr);
						TransactionLines.Cache.SetValue<PMProformaTransactLine.released>(line, true);
						TransactionLines.Cache.MarkUpdated(line, assertError: true);

						List<PMTran> list;
						if (pmtranByProformalLineNbr.TryGetValue(line.LineNbr.Value, out list))
						{
							foreach (PMTran tran in list)
							{
								tran.ARTranType = line.ARInvoiceDocType;
								tran.ARRefNbr = line.ARInvoiceRefNbr;
								tran.RefLineNbr = line.ARInvoiceLineNbr;
								AllReferencedTransactions.Update(tran);
							}
						}
					}

					if (pmEntry.Transactions.Cache.IsDirty)
						pmEntry.Save.Press();

					Save.Press();
					ts.Complete();
					invoice = (Invoices.Locate(invoice) as ARInvoice) ?? invoice;
				}

				if (project.AutomaticReleaseAR == true)
				{
					try
					{
						List<ARRegister> list = new List<ARRegister>();
						if (creditMemo != null)
						{
							list.Add(creditMemo);
						}
						list.Add(invoice);
						ARDocumentRelease.ReleaseDoc(list, false);
					}
					catch (Exception ex)
					{
						throw new PXException(Messages.AutoReleaseAROnProrofmaReleaseFailed, ex);
					}
				}
			}
		}

		protected virtual PMProformaRevision GetLastRevision()
		{
			var list = new List<PMProformaRevision>(Revisions.Select().RowCast<PMProformaRevision>());
			return list.LastOrDefault();
		}

		protected virtual ARInvoice ProcessRevision()
		{
			PMProformaRevision last = GetLastRevision();
			if (last == null)
				return null;

			ARInvoice doc = PXSelect<ARInvoice,
				Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
				And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, last.ARInvoiceDocType, last.ARInvoiceRefNbr);

			if (IsReversingDocumentAlreadyExists(doc))
				return null;

			if (doc.Released != true)
			{
				throw new PXException(Messages.ArDocShouldBereleasedBeforeCorrection, last.ARInvoiceRefNbr);
			}

			ARInvoiceEntry invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
			//
			invoiceEntry.Document.Search<ARInvoice.refNbr>(last.ARInvoiceRefNbr, last.ARInvoiceDocType);
			var finPeriod = FinPeriodRepository.GetByID(Document.Current.FinPeriodID, PXAccess.GetParentOrganizationID(Document.Current.BranchID));
			var canPostRes = FinPeriodUtils.CanPostToPeriod(finPeriod, typeof(GL.FinPeriods.TableDefinition.FinPeriod.aRClosed));

			if (canPostRes.HasWarningOrError && canPostRes.MaxErrorLevel >= PXErrorLevel.Error)
			{
				OpenPeriodAttribute.SetValidatePeriod<ARInvoice.finPeriodID>(invoiceEntry.Document.Cache, null, PeriodValidation.Nothing);

				invoiceEntry.Document.Cache.Adjust<AROpenPeriodAttribute>().For<ARInvoice.finPeriodID>(attr =>	
				{
					attr.ValidatePeriod = PeriodValidation.Nothing;
					attr.RedefaultOnDateChanged = false;
				});

				ReverseCurrentDocument(invoiceEntry);
				invoiceEntry.Document.Current.DocDate = Document.Current.InvoiceDate;
				invoiceEntry.Document.Current.FinPeriodID = Document.Current.FinPeriodID;
				invoiceEntry.Document.Update(invoiceEntry.Document.Current);
			}
			else
			{
				ReverseCurrentDocument(invoiceEntry);
				invoiceEntry.Document.Current.DocDesc = string.Format(Messages.CorrectionReversal, Document.Current.RefNbr);
			}
			invoiceEntry.Save.Press();

			last.ReversedARInvoiceDocType = invoiceEntry.Document.Current.DocType;
			last.ReversedARInvoiceRefNbr = invoiceEntry.Document.Current.RefNbr;

			Revisions.Update(last);

			return invoiceEntry.Document.Current;
		}

		private void ReverseCurrentDocument(ARInvoiceEntry invoiceEntry)
        {
			ReverseInvoiceArgs reverseArgs = new ReverseInvoiceArgs();
			reverseArgs.DateOption = ReverseInvoiceArgs.CopyOption.Override;
			reverseArgs.DocumentDate = Document.Current.InvoiceDate;
			reverseArgs.DocumentFinPeriodID = Document.Current.FinPeriodID;

			PXAdapter adapter = new PXAdapter(new PXView.Dummy(invoiceEntry, invoiceEntry.Document.View.BqlSelect,
														new List<object>() { invoiceEntry.Document.Current }));

			if (invoiceEntry.Document.Current.PaymentsByLinesAllowed == true)
			{
				reverseArgs.ApplyToOriginalDocument = false;
				invoiceEntry.ReverseDocumentAndApplyToReversalIfNeeded(adapter, reverseArgs);
			}
            else
            {
				reverseArgs.ApplyToOriginalDocument = true;
				invoiceEntry.ReverseDocumentAndApplyToReversalIfNeeded(adapter, reverseArgs);
			}
		}

		protected virtual bool IsReversingDocumentAlreadyExists(ARInvoice origDoc)
			=> GetReversingDocument(origDoc.DocType, origDoc.RefNbr) != null;

		public virtual ARRegister GetReversingDocument(string originalDocType, string originalRefNbr)
		{
			string reversingDocType = GetReversingDocType(originalDocType);
			ARRegister reversingDoc = PXSelect<ARRegister,
				Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
					And<ARRegister.origDocType, Equal<Required<ARRegister.origDocType>>,
					And<ARRegister.origRefNbr, Equal<Required<ARRegister.origRefNbr>>,
					And<ARRegister.isRetainageDocument, NotEqual<True>>>>>,
				OrderBy<Desc<ARRegister.createdDateTime>>>
				.SelectSingleBound(this, null,
					reversingDocType, originalDocType, originalRefNbr);

			return reversingDoc;
		}

		// TODO: Use GetReversingDocType from ARInvoiceEntry.
		// TODO: Why virtual and not static.
		public virtual string GetReversingDocType(string docType)
		{
			switch (docType)
			{
				case ARDocType.Invoice:
				case ARDocType.DebitMemo:
					docType = ARDocType.CreditMemo;
					break;
				case ARDocType.CreditMemo:
					docType = ARDocType.DebitMemo;
					break;
			}

			return docType;
		}

		public override void Persist()
		{
			BranchAttribute.VerifyFieldInPXCache<PMProformaProgressLine, PMProformaProgressLine.branchID>(this, ProgressiveLines.Select());
			BranchAttribute.VerifyFieldInPXCache<PMProformaTransactLine, PMProformaTransactLine.branchID>(this, TransactionLines.Select());

			PMProforma deletedDoc = null;
			foreach (PMProforma doc in Document.Cache.Deleted)
			{
				deletedDoc = doc;
			}

			RollbackRevision(deletedDoc);

			base.Persist();
		}

		protected virtual void RollbackRevision(PMProforma deletedDoc)
		{
			if (deletedDoc != null)
			{
				PMProforma corrected = PXSelect<PMProforma,
						Where<PMProforma.refNbr, Equal<Required<PMProforma.refNbr>>,
						And<PMProforma.corrected, Equal<True>>>,
						OrderBy<Desc<PMProforma.revisionID>>>.SelectWindowed(this, 0, 1, deletedDoc.RefNbr);
				if (corrected != null)
				{
					Document.Cache.SetValue<PMProforma.corrected>(corrected, false);
					Document.Cache.MarkUpdated(corrected, assertError: true);

					foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.View.SelectMultiBound(new[] { corrected }))
					{
						PMProformaProgressLine line = (PMProformaProgressLine)res;
						ProgressiveLines.Cache.SetValue<PMProformaProgressLine.corrected>(line, false);
						ProgressiveLines.Cache.MarkUpdated(line, assertError: true);
					}

					foreach (PMProformaTransactLine line in TransactionLines.View.SelectMultiBound(new[] { corrected }).RowCast<PMProformaTransactLine>())
					{
						TransactionLines.Cache.SetValue<PMProformaTransactLine.corrected>(line, false);
						TransactionLines.Cache.MarkUpdated(line, assertError: true);
					}
				}
			}
		}

		protected virtual void CopyPMAddressToARInvoice(ARInvoiceEntry invoiceEntry, PMAddress addressPM, ARAddress addressAR)
		{
			addressAR.BAccountAddressID = addressPM.BAccountAddressID;
			addressAR.BAccountID = addressPM.BAccountID;
			addressAR.RevisionID = addressPM.RevisionID;
			addressAR.IsDefaultAddress = addressPM.IsDefaultAddress;
			addressAR.AddressLine1 = addressPM.AddressLine1;
			addressAR.AddressLine2 = addressPM.AddressLine2;
			addressAR.AddressLine3 = addressPM.AddressLine3;
			addressAR.City = addressPM.City;
			addressAR.State = addressPM.State;
			addressAR.PostalCode = addressPM.PostalCode;
			addressAR.CountryID = addressPM.CountryID;
			addressAR.IsValidated = addressPM.IsValidated;
		}

		protected virtual void CopyPMContactToARInvoice(ARInvoiceEntry invoiceEntry, PMContact contactPM, ARContact contactAR)
		{
			contactAR.BAccountContactID = contactPM.BAccountContactID;
			contactAR.BAccountID = contactPM.BAccountID;
			contactAR.RevisionID = contactPM.RevisionID;
			contactAR.IsDefaultContact = contactPM.IsDefaultContact;
			contactAR.FullName = contactPM.FullName;
			contactAR.Attention = contactPM.Attention;
			contactAR.Salutation = contactPM.Salutation;
			contactAR.Title = contactPM.Title;
			contactAR.Phone1 = contactPM.Phone1;
			contactAR.Phone1Type = contactPM.Phone1Type;
			contactAR.Phone2 = contactPM.Phone2;
			contactAR.Phone2Type = contactPM.Phone2Type;
			contactAR.Phone3 = contactPM.Phone3;
			contactAR.Phone3Type = contactPM.Phone3Type;
			contactAR.Fax = contactPM.Fax;
			contactAR.FaxType = contactPM.FaxType;
			contactAR.Email = contactPM.Email;
		}
	
		public virtual ARTran InsertTransaction(ARInvoiceEntry invoiceEntry, PMProformaLine line, int mult)
		{
			var tran = new ARTran();
			tran.InventoryID = line.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : line.InventoryID;
			tran.BranchID = line.BranchID;
			tran.TranDesc = line.Description;
			tran.ProjectID = line.ProjectID;
			tran.TaskID = line.TaskID;
			tran.CostCodeID = line.CostCodeID;
			tran.ExpenseDate = line.Date;
			tran.AccountID = line.AccountID;
			tran.SubID = line.SubID;
			tran.TaxCategoryID = line.TaxCategoryID;
			tran.UOM = line.UOM;
			tran.Qty = line.Qty * mult;
			tran.CuryUnitPrice = line.CuryUnitPrice;
			tran.CuryExtPrice = line.CuryLineTotal * mult;
			tran.FreezeManualDisc = true;
			tran.ManualPrice = true;
			tran.PMDeltaOption = line.Option == PMProformaLine.option.HoldRemainder ? line.Option : PMProformaLine.option.WriteOffRemainder;
			tran.DeferredCode = line.DefCode;
			tran.RetainagePct = line.RetainagePct;
			tran.RetainageAmt = line.Retainage * mult;
			tran.CuryRetainageAmt = line.CuryRetainage * mult;
			
			tran = invoiceEntry.Transactions.Insert(tran);

			bool updateRequired = false;
			if (!string.IsNullOrEmpty(line.TaxCategoryID) && line.TaxCategoryID != tran.TaxCategoryID)
			{
				tran.TaxCategoryID = line.TaxCategoryID;
				updateRequired = true;
			}

			//if CuryExtPrice is passed as zero InvoiceEntry will recalculate it as Qty x UnitPrice.
			//so we need to correct it explicitly:
			if (line.CuryLineTotal == 0)
			{
				tran.CuryExtPrice = 0;
				updateRequired = true;				
			}

			if (updateRequired)
			{
				tran = invoiceEntry.Transactions.Update(tran);
			}

			return tran;
		}

		protected virtual void AddToInvoiced(PMProformaLine line)
		{
			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			
			AddToInvoiced(line, projectedRevenueAccountGroupID);
		}

		protected virtual void SubtractFromInvoiced(PMProformaLine line)
		{
			if (line.LineTotal == 0 && line.Qty == 0)
				return;

			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			AddToInvoiced(line, projectedRevenueAccountGroupID, -1);
		}

		protected virtual void SubtractFromInvoiced(PMProformaLine line, int? revenueAccountGroup)
		{
			AddToInvoiced(line, revenueAccountGroup, -1);
		}

		private decimal GetBaseValueForBudget(PMProject project, decimal curyValue)
		{
			if (project.CuryID == project.BaseCuryID) return curyValue;
			else
			{
				CurrencyInfo currencyInfo = GetExtension<MultiCurrency>().GetCurrencyInfo(project.CuryInfoID);
				return currencyInfo.CuryConvBase(curyValue);
			}
		}

		protected virtual void AddToInvoiced(PMProformaLine line, int? revenueAccountGroup, int mult = 1)
		{
			if (revenueAccountGroup != null && line.ProjectID != null && line.TaskID != null )
			{
				decimal curyAmount = mult * GetAmountInProjectCurrency(line.CuryLineTotal);
				PMBudgetAccum invoiced = GetTargetBudget(revenueAccountGroup, line);
				invoiced = Budget.Insert(invoiced);

				INUnitAttribute.TryConvertGlobalUnits(this, line.UOM, invoiced.UOM, line.Qty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal qty);


				PMProject project = PMProject.PK.Find(this, line.ProjectID);

				invoiced.CuryInvoicedAmount += curyAmount;
				invoiced.InvoicedAmount += GetBaseValueForBudget(project, curyAmount);
				invoiced.InvoicedQty += mult * qty;

				if (line.IsPrepayment == true)
				{
					invoiced.CuryPrepaymentInvoiced += curyAmount;
					invoiced.PrepaymentInvoiced += GetBaseValueForBudget(project, curyAmount);
				}
			}
		}

		protected virtual void SubtractFromDraftRetained(PMProformaLine line)
		{
			if (line.LineTotal == 0)
				return;

			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			SubtractFromDraftRetained(line, projectedRevenueAccountGroupID);
		}

		protected virtual void SubtractFromDraftRetained(PMProformaLine line, int? revenueAccountGroup)
		{
			AddToDraftRetained(line, revenueAccountGroup, -1);
		}

		protected virtual void AddToDraftRetained(PMProformaLine line)
		{
			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			AddToDraftRetained(line, projectedRevenueAccountGroupID);
		}

		protected virtual void AddToDraftRetained(PMProformaLine line, int? revenueAccountGroup, int mult = 1)
		{
			if (revenueAccountGroup != null && line.ProjectID != null && line.TaskID != null)
			{
				decimal curyAmount = mult * GetAmountInProjectCurrency(line.CuryRetainage);

				PMBudgetAccum invoiced = GetTargetBudget(revenueAccountGroup, line);
				invoiced = Budget.Insert(invoiced);
				invoiced.CuryDraftRetainedAmount += curyAmount;
				PMProject project = PMProject.PK.Find(this, line.ProjectID);
				invoiced.DraftRetainedAmount += GetBaseValueForBudget(project, curyAmount);
			}
		}

		public virtual void SubtractFromTotalRetained(PMProformaLine line)
		{
			if (line.LineTotal == 0)
				return;

			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			SubtractFromTotalRetained(line, projectedRevenueAccountGroupID);
		}

		protected virtual void SubtractFromTotalRetained(PMProformaLine line, int? revenueAccountGroup)
		{
			AddToTotalRetained(line, revenueAccountGroup, -1);
		}

		public virtual void AddToTotalRetained(PMProformaLine line)
		{
			int? projectedRevenueAccountGroupID = line.AccountGroupID;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}

			AddToTotalRetained(line, projectedRevenueAccountGroupID);
		}

		protected virtual void AddToTotalRetained(PMProformaLine line, int? revenueAccountGroup, int mult = 1)
		{
			if (revenueAccountGroup != null && line.ProjectID != null && line.TaskID != null)
			{
				decimal curyAmount = mult * GetAmountInProjectCurrency(line.CuryRetainage);
				PMBudgetAccum invoiced = GetTargetBudget(revenueAccountGroup, line);
				invoiced = Budget.Insert(invoiced);
				invoiced.CuryTotalRetainedAmount += curyAmount;

				PMProject project = PMProject.PK.Find(this, line.ProjectID);
				invoiced.TotalRetainedAmount += GetBaseValueForBudget(project, curyAmount);
			}
		}


		protected virtual PMBudgetAccum GetTargetBudget(int? accountGroupID, PMProformaLine line)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(this, accountGroupID);
			PMProject project = PMProject.PK.Find(this, line.ProjectID);

			BudgetService budgetService = new BudgetService(this);
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
				CuryInfoID = project.CuryInfoID,
				ProgressBillingBase = budget.ProgressBillingBase
			};
		}

		public virtual void SubtractPerpaymentRemainder(PMProformaLine line, int mult = 1)
		{
			int? projectedRevenueAccountGroupID = null;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			else
			{
				projectedRevenueAccountGroupID = line.AccountGroupID;
			}

			if (projectedRevenueAccountGroupID != null && line.ProjectID != null && line.TaskID != null)
			{
				decimal curyAmount = mult * GetAmountInProjectCurrency(line.CuryPrepaidAmount);

				PMBudgetAccum invoiced = GetTargetBudget(projectedRevenueAccountGroupID, line);
				invoiced = Budget.Insert(invoiced);
				invoiced.CuryPrepaymentAvailable -= curyAmount;
				PMProject project = PMProject.PK.Find(this, line.ProjectID);
				invoiced.PrepaymentAvailable -= GetBaseValueForBudget(project, curyAmount);
			}
		}

		public virtual void SubtractValuesToInvoice(PMProformaLine line, decimal? value, decimal? qty) 
		{
			int? projectedRevenueAccountGroupID = null;
			if (line.Type == PMProformaLineType.Transaction)
			{
				projectedRevenueAccountGroupID = GetProjectedAccountGroup((PMProformaTransactLine)line);
			}
			else
			{
				projectedRevenueAccountGroupID = line.AccountGroupID;
			}

			if (projectedRevenueAccountGroupID != null && line.ProjectID != null && line.TaskID != null)
			{
				decimal amount = GetAmountInProjectCurrency(value);

				PMBudgetAccum invoiced = GetTargetBudget(projectedRevenueAccountGroupID, line);
				invoiced = Budget.Insert(invoiced);
				invoiced.CuryAmountToInvoice -= amount;


				PMProject project = PMProject.PK.Find(this, line.ProjectID);
				invoiced.AmountToInvoice -= GetBaseValueForBudget(project, amount);
				if (invoiced.ProgressBillingBase == ProgressBillingBase.Quantity)
				{
					INUnitAttribute.TryConvertGlobalUnits(this, line.UOM, invoiced.UOM, qty.GetValueOrDefault(), INPrecision.QUANTITY, out decimal convertedQty);
					invoiced.QtyToInvoice -= convertedQty;
				}
			}
		}

		public virtual bool IsLimitsEnabled()
		{
			if (Project.Current == null)
				return false;

			return Project.Current.LimitsEnabled == true;
		}

		public virtual bool IsAdjustment(PMProformaTransactLine line)
		{
			if (line == null)
				return false;

			if (line.LineNbr == null)
				return true;

			//billing process do not create adjustment lines.
			if (this.UnattendedMode == true)
				return false;

			var references = GetReferencedTransactions();
			return !references.ContainsKey(line.LineNbr.Value);
		}

		public virtual Dictionary<int, List<PMTran>> GetReferencedTransactions()
		{
			if (cachedReferencedTransactions == null)
			{
				cachedReferencedTransactions = new Dictionary<int, List<PMTran>>();

				foreach (PMTran tran in AllReferencedTransactions.View.SelectMultiBound(new[] { Document.Current }).RowCast<PMTran>())
				{
					List<PMTran> list;
					if (!cachedReferencedTransactions.TryGetValue(tran.ProformaLineNbr.Value, out list))
					{
						list = new List<PMTran>();
						cachedReferencedTransactions.Add(tran.ProformaLineNbr.Value, list);
					}

					list.Add(tran);
				}
			}

			return cachedReferencedTransactions;
		}

		public bool IsAllocated(PMProformaTransactLine row)
		{
			return false;
		}

		public virtual int? GetProjectedAccountGroup(PMProformaTransactLine line)
		{
			int? projectedRevenueAccountGroupID = null;
			int? projectedRevenueAccount = line.AccountID;
		
			if (projectedRevenueAccount != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<PMProformaTransactLine.accountID>(TransactionLines.Cache, line, projectedRevenueAccount) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID == null)
						throw new PXException(PM.Messages.RevenueAccountIsNotMappedToAccountGroup, revenueAccount.AccountCD);

					projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}

			return projectedRevenueAccountGroupID;
		}

		public virtual PMRevenueBudget SelectRevenueBudget(PMProformaProgressLine row)
		{			
			PMRevenueBudget budget = PMRevenueBudget.PK.Find(this, row.ProjectID, row.TaskID, row.AccountGroupID, row.CostCodeID, row.InventoryID);

			return budget;
		}

		/// <summary>
		/// Returns Pending InvoicedAmount calculated for current document in Project Currency.
		/// </summary>
		public virtual decimal CalculatePendingInvoicedAmount(PMProformaProgressLine row)
		{
			return CalculatePendingInvoicedAmount(row.ProjectID, row.TaskID, row.AccountGroupID, row.InventoryID, row.CostCodeID);
		}

		/// <summary>
		/// Returns Pending InvoicedAmount calculated for current document in Project Currency.
		/// </summary>
		public virtual decimal CalculatePendingInvoicedAmount(int? projectID, int? taskID, int? accountGroupID, int? inventoryID, int? costCodeID)
		{
			decimal result = 0;

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				if (accum.ProjectID == projectID && accum.ProjectTaskID == taskID && accum.AccountGroupID == accountGroupID && accum.InventoryID == inventoryID && accum.CostCodeID == costCodeID)
				{
					result += accum.CuryInvoicedAmount.GetValueOrDefault();
				}
			}

			return result;
		}

		/// <summary>
		/// Returns Prevously invoiced Amount in Project Currency
		/// </summary>
		public virtual Dictionary<BudgetKeyTuple, decimal> CalculatePreviouslyInvoicedAmounts(PMProforma document)
		{
			Dictionary<BudgetKeyTuple, decimal> previouslyInvoiced = new Dictionary<BudgetKeyTuple, decimal>();
			
			var select = new PXSelect<PMProformaProgressLine,
					Where<PMProformaProgressLine.type, Equal<PMProformaLineType.progressive>,
					And<PMProformaProgressLine.projectID, Equal<Required<PMProforma.projectID>>,
					And<PMProformaProgressLine.refNbr, GreaterEqual<Required<PMProformaProgressLine.refNbr>>,
					And<PMProformaProgressLine.corrected, NotEqual<True>>>>>>(this);

			var currentAndFutureProgressiveLines = select.Select(document.ProjectID, document.RefNbr);

			var select2 = new PXSelect<PMProformaTransactLine,
					Where<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>,
					And<PMProformaProgressLine.projectID, Equal<Required<PMProforma.projectID>>,
					And<PMProformaTransactLine.refNbr, GreaterEqual<Required<PMProformaTransactLine.refNbr>>,
					And<PMProformaTransactLine.corrected, NotEqual<True>>>>>>(this);

			var currentAndFutureTransactionLines = select2.Select(document.ProjectID, document.RefNbr);

			var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);
			var revenueBudget = selectRevenueBudget.Select(document.ProjectID);

			foreach (PMRevenueBudget budget in revenueBudget)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(budget);
				var previouslyInvoicedAmount = GetCuryActualAmountWithTaxes(budget) + budget.CuryInvoicedAmount.GetValueOrDefault();
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] += previouslyInvoicedAmount;
				else
					previouslyInvoiced[key] = previouslyInvoicedAmount;
			}

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(accum);
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] += accum.CuryInvoicedAmount.GetValueOrDefault();
				else
					previouslyInvoiced[key] = accum.CuryInvoicedAmount.GetValueOrDefault();
			}

			foreach (PMProformaProgressLine line in currentAndFutureProgressiveLines)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (previouslyInvoiced.ContainsKey(key))
					previouslyInvoiced[key] -= GetAmountInProjectCurrency(line.CuryLineTotal);
			}

			foreach (PMProformaTransactLine line in currentAndFutureTransactionLines)
			{
				BudgetKeyTuple key = GetBudgetKey(line);
				if (previouslyInvoiced.ContainsKey(key))
				{
					previouslyInvoiced[key] -= GetAmountInProjectCurrency(line.CuryLineTotal);
				}
				else 
				{
					//Will be executed in case BudgetLevel=Item and given item is not budgeted.
					BudgetKeyTuple naKey = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					if (previouslyInvoiced.ContainsKey(naKey))
					{
						previouslyInvoiced[naKey] -= GetAmountInProjectCurrency(line.CuryLineTotal);
					}
				}
			}

			return previouslyInvoiced;
		}
				
		private decimal GetAmountInProjectCurrency(decimal? value)
		{
			return MultiCurrencyService.GetValueInProjectCurrency(this, Project.Current, Document.Current.CuryID, Document.Current.InvoiceDate, value);
		}

		private decimal GetAmountInBillingCurrency(decimal? value)
		{
			return MultiCurrencyService.GetValueInBillingCurrency(this, Project.Current, GetExtension<MultiCurrency>().GetDefaultCurrencyInfo(), value);
		}

		private decimal GetLastInvoicedBeforeCorrection(PMProformaProgressLine row)
		{
			if (row.RevisionID.GetValueOrDefault() < 2)
				return 0;

			var select = new PXSelect<PMProformaProgressLine,
				Where<PMProformaProgressLine.refNbr, Equal<Required<PMProformaProgressLine.refNbr>>,
				And<PMProformaProgressLine.lineNbr, Equal<Required<PMProformaProgressLine.lineNbr>>,
				And<PMProformaProgressLine.corrected, Equal<True>>>>,
				OrderBy<Desc<PMProformaProgressLine.revisionID>>>(this);

			PMProformaProgressLine lastCorrectedLine = select.SelectSingle(row.RefNbr, row.LineNbr);
			if (lastCorrectedLine != null)
			{
				return lastCorrectedLine.CuryLineTotal.GetValueOrDefault();
			}

			return 0;
		}

		public virtual bool IsPrepaidAmountEnabled(PMProformaLine line)
		{
			return line.IsPrepayment != true;
		}

		public virtual void ApplyPrepayment(PMProforma doc)
		{
			var select = new PXSelect<PMRevenueBudget, Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
				And<PMRevenueBudget.curyPrepaymentAvailable, Greater<decimal0>>>>(this);

			Dictionary<BudgetKeyTuple, decimal> remainders = new Dictionary<BudgetKeyTuple, decimal>(); // project currency

			foreach (PMRevenueBudget budget in select.Select(doc.ProjectID))
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(budget);
				if (remainders.ContainsKey(key))
					remainders[key] += budget.CuryPrepaymentAvailable.GetValueOrDefault();
				else
					remainders[key] = budget.CuryPrepaymentAvailable.GetValueOrDefault();
			}

			foreach (PMBudgetAccum accum in Budget.Cache.Inserted)
			{
				BudgetKeyTuple key = BudgetKeyTuple.Create(accum);
				if (remainders.ContainsKey(key))
					remainders[key] += accum.CuryPrepaymentAvailable.GetValueOrDefault(); //not  saved -ve values.
			}


			foreach (PMProformaProgressLine line in ProgressiveLines.Select())
			{
				if (line.IsPrepayment == true)
					continue;

				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.InventoryID, CostCodeAttribute.DefaultCostCode.GetValueOrDefault());
				}


				if ((Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					if (!remainders.ContainsKey(key))
					{
						key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					}
				}
				
				decimal remainder = 0;
				if (remainders.TryGetValue(key, out remainder) && remainder > 0 && line.CuryAmount > 0 && line.CuryPrepaidAmount == 0)
				{
					decimal curyRemainder = GetAmountInBillingCurrency(remainder);

					line.CuryPrepaidAmount = Math.Min(curyRemainder, line.CuryAmount.Value);
					ProgressiveLines.Update(line);

					remainders[key] -= GetAmountInProjectCurrency(line.CuryPrepaidAmount);
				}
			}

			foreach (PMProformaTransactLine line in TransactionLines.Select())
			{
				if (line.IsPrepayment == true)
					continue;

				BudgetKeyTuple key = BudgetKeyTuple.Create(line);
				if (Project.Current.BudgetLevel == BudgetLevels.Task)
				{
					key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, key.InventoryID, CostCodeAttribute.DefaultCostCode.GetValueOrDefault());
				}

				if ((Project.Current.BudgetLevel == BudgetLevels.Item || Project.Current.BudgetLevel == BudgetLevels.Detail) && line.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					if (!remainders.ContainsKey(key))
					{
						key = new BudgetKeyTuple(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, PMInventorySelectorAttribute.EmptyInventoryID, key.CostCodeID);
					}
				}
								
				decimal remainder = 0;
				if (remainders.TryGetValue(key, out remainder) && remainder > 0 && line.CuryAmount > 0 && line.CuryPrepaidAmount.GetValueOrDefault() == 0) 
				{
					decimal curyRemainder = GetAmountInBillingCurrency(remainder);
					line.CuryPrepaidAmount = Math.Min(curyRemainder, line.CuryAmount.GetValueOrDefault());
					TransactionLines.Update(line);

					remainders[key] -= GetAmountInProjectCurrency(line.CuryPrepaidAmount);
				}
			}
		}

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public virtual void ValidatePrecedingBeforeRelease(PMProforma doc)
		{
			var selectUnreleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.proformaRefNbr, Greater<PMBillingRecordEx.proformaRefNbr>,
				And<PMBillingRecordEx.aRRefNbr, IsNull>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>>(this);

			var resultset = selectUnreleased.Select(doc.ProjectID, doc.RefNbr);
			if (resultset.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				foreach (PXResult<PMBillingRecord, PMBillingRecordEx> res in resultset)
				{
					PMBillingRecordEx unreleased = (PMBillingRecordEx)res;
					sb.AppendFormat("{0},", unreleased.ProformaRefNbr);
				}

				string list = sb.ToString().TrimEnd(',');

				throw new PXException(Messages.UnreleasedProforma, list);
			}
		}

		public virtual void ValidatePrecedingInvoicesBeforeRelease(PMProforma doc)
		{
			var selectUnreleased = new PXSelectJoin<PMBillingRecord,
				InnerJoin<PMBillingRecordEx, On<PMBillingRecord.projectID, Equal<PMBillingRecordEx.projectID>,
				And<PMBillingRecord.billingTag, Equal<PMBillingRecordEx.billingTag>,
				And<PMBillingRecord.proformaRefNbr, Greater<PMBillingRecordEx.proformaRefNbr>,
				And<PMBillingRecordEx.aRRefNbr, IsNotNull>>>>,
				InnerJoin<ARRegister, On<ARRegister.docType, Equal<PMBillingRecordEx.aRDocType>, And<ARRegister.refNbr, Equal<PMBillingRecordEx.aRRefNbr>>>>>,
				Where<PMBillingRecord.projectID, Equal<Required<PMBillingRecord.projectID>>,
				And<PMBillingRecord.proformaRefNbr, Equal<Required<PMBillingRecord.proformaRefNbr>>>>,
				OrderBy<Desc<PMBillingRecordEx.recordID>>>(this);

			var resultset = selectUnreleased.SelectWindowed(0, 1, doc.ProjectID, doc.RefNbr);
			if (resultset.Count > 0)
			{
				ARRegister register = PXResult.Unwrap<ARRegister>(resultset[0]);
				if (register != null && register.Released != true)
					throw new PXException(Messages.UnreleasedPreviousInvoice, register.DocType, register.RefNbr);
			}
		}

		public virtual void ValidateBranchBeforeRelease(PMProforma doc)
		{
			Branch branch =  Branch.PK.Find(this, doc.BranchID);

			if (branch == null)
			{
				using (new PXReadDeletedScope())
				{
					branch = Branch.PK.Find(this, doc.BranchID);
					if (branch != null)
						throw new PXException(Messages.ProformaCannotRealeaseWithDeletedBranch, branch.BranchCD.Trim());
				}
			}			
		}

		public virtual void AppendUnbilled()
		{
			if (Document.Current == null)
				return;

			PMBillingRecord billingRecord = PXSelect<PMBillingRecord, Where<PMBillingRecord.proformaRefNbr, Equal<Required<PMProforma.refNbr>>>>.Select(this, Document.Current.RefNbr);
			string tag = billingRecord?.BillingTag ?? "P";

			DateTime invoiceDate = Document.Current.InvoiceDate.Value;

			ProformaAppender engine = PXGraph.CreateInstance<ProformaAppender>();
			engine.SetProformaEntry(this);
			List<PMTask> tasks = engine.SelectBillableTasks(Project.Current);
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, Document.Current.CustomerID);
			
			DateTime cuttoffDate = invoiceDate.AddDays(engine.IncludeTodaysTransactions ? 1 : 0);
			engine.PreSelectTasksTransactions(Document.Current.ProjectID, tasks, cuttoffDate); //billingRules dictionary also filled.

			HashSet<string> distinctRateTables = new HashSet<string>();
			foreach (PMTask task in tasks)
			{
				if (!string.IsNullOrEmpty(task.RateTableID))
					distinctRateTables.Add(task.RateTableID);
			}
			HashSet<string> distinctRateTypes = new HashSet<string>();
			foreach (List<PMBillingRule> ruleList in engine.billingRules.Values)
			{
				foreach (PMBillingRule rule in ruleList)
				{
					if (!string.IsNullOrEmpty(rule.RateTypeID))
						distinctRateTypes.Add(rule.RateTypeID);
				}
			}

			engine.InitRateEngine(distinctRateTables.ToList(), distinctRateTypes.ToList());

			List<PMTran> billingBase = new List<PMTran>();
			List<PMBillEngine.BillingData> billingData = new List<PMBillEngine.BillingData>();
			Dictionary<int, decimal> availableQty = new Dictionary<int, decimal>();
			Dictionary<int, PMRecurringItem> billingItems = new Dictionary<int, PMRecurringItem>();

			foreach (PMTask task in tasks)
			{
				if (task.WipAccountGroupID != null)
					continue;
				
				List<PMBillingRule> rulesList;
				if (engine.billingRules.TryGetValue(task.BillingID, out rulesList))
				{
					foreach (PMBillingRule rule in rulesList)
					{
						if (rule.Type == PMBillingType.Transaction)
						{
							billingData.AddRange(engine.BillTask(Project.Current, customer, task, rule, invoiceDate, availableQty, billingItems, true));
						}
					}
				}
			}

			engine.InsertTransactionsInProforma(Project.Current, billingData);

			foreach (PMBillEngine.BillingData data in billingData)
			{
				foreach (PMTran orig in data.Transactions)
				{
					orig.Billed = true;
					orig.BilledDate = invoiceDate;
					Unbilled.Update(orig);
					PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, orig);
				}
			}
		}

		/// <summary>
		/// If false during the release of proforma documet taxes are copied as is; otherwise taxes are recalculated automaticaly on the ARInvoice.
		/// Default value is false.
		/// </summary>
		public virtual bool RecalculateTaxesOnRelease()
		{
			if (Document.Current != null && Customer.Current != null)
			{
				return Customer.Current.PaymentsByLinesAllowed == true;
			}

			return false;
		}

		public virtual BudgetKeyTuple GetBudgetKey(PMProformaTransactLine line)
		{
			int? accountGroupID = GetProjectedAccountGroup(line);
			int inventoryID = line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);

			if (Project.Current.BudgetLevel != BudgetLevels.Item)
				inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;

			BudgetKeyTuple defualtKey = BudgetKeyTuple.Create(line);
			BudgetKeyTuple key = new BudgetKeyTuple(defualtKey.ProjectID, defualtKey.ProjectTaskID, accountGroupID.GetValueOrDefault(), inventoryID, defualtKey.CostCodeID);

			return key;
		}

		#region Retainage

		protected virtual void RecalculateRetainageOnDocument(PMProject project)
		{
			if (project?.RetainageMode == RetainageModes.Contract)
			{
				PMProjectRevenueTotal budget = PXSelectReadonly<PMProjectRevenueTotal,
				Where<PMProjectRevenueTotal.projectID, Equal<Required<PMProjectRevenueTotal.projectID>>>>.Select(this, project.ContractID);
				
				decimal totalRetainageOnInvoice = GetTotalRetainageOnInvoice(project.RetainagePct.GetValueOrDefault());
				decimal totalRetainageUptoDate = GetTotalRetainageUptoDate(budget);
				decimal contractAmount = GetContractAmount(project, budget);

				RecalculateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice);

				decimal totalInvoiceUptoDate = GetTotalInvoiced(budget);
				decimal retainageToAllocate = GetTotalRetainageUptoDate(budget, true);
				decimal retainageReleased = GetBilledRetainageToDateTotal(Document.Current.InvoiceDate);
				ReAllocateContractRetainage(project, totalInvoiceUptoDate, retainageToAllocate - retainageReleased);

				ProgressiveLines.View.RequestRefresh();
			}
		}

		/// <summary>
		/// Returns Total Retainage accumulated upto date excluding current document in project currency
		/// </summary>
		/// 
		private decimal GetTotalRetainageUptoDate(PMProjectRevenueTotal budget)
		{
			return GetTotalRetainageUptoDate(budget, false);
		}

		/// <summary>
		/// Returns Total Retainage accumulated upto date in project currency
		/// </summary>
		private decimal GetTotalRetainageUptoDate(PMProjectRevenueTotal budget, bool includeCurrentDocument)
		{
			decimal totalRetainageUptoDate = budget.CuryTotalRetainedAmount.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				totalRetainageUptoDate += item.CuryTotalRetainedAmount.GetValueOrDefault();
			}

			if (!includeCurrentDocument)
			{
				foreach (PMProformaProgressLine line in ProgressiveLines.Select())
				{
					totalRetainageUptoDate -= GetAmountInProjectCurrency(line.CuryRetainage.GetValueOrDefault());
				}
			}

			return totalRetainageUptoDate;
		}
				
		public virtual void RecalculateRetainage()
		{
			if (Project.Current == null)
				return;
			if (Document.Current == null)
				return;
			if (Document.Current.Released == true)
				return;
					
			if (Project.Current.RetainageMode == RetainageModes.Contract)
			{
				PMProjectRevenueTotal budget = PXSelectReadonly<PMProjectRevenueTotal,
				Where<PMProjectRevenueTotal.projectID, Equal<Required<PMProjectRevenueTotal.projectID>>>>.Select(this, Project.Current.ContractID);

				RecalculateContractRetainage(Project.Current, budget);
				ReAllocateContractRetainage(Project.Current, budget);
			}
			else if (Project.Current.RetainageMode == RetainageModes.Line)
			{
				RecalculateLineRetainage(Project.Current);
			}
		}

		protected bool RecalculatingContractRetainage = false;
		protected virtual void RecalculateContractRetainage(PMProject project, PMProjectRevenueTotal budget)
		{
			
			decimal totalRetainageOnInvoice = GetAmountInProjectCurrency(GetTotalRetainageOnInvoice(project.RetainagePct.GetValueOrDefault()));
			decimal totalRetainageUptoDate = budget.CuryTotalRetainedAmount.GetValueOrDefault();
			decimal contractAmount = GetContractAmount(project, budget);


			RecalculateContractRetainage(project, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice);
		}

		protected virtual void RecalculateContractRetainage(PMProject project, decimal totalRetainageUptoDate, decimal contractAmount, decimal totalRetainageOnInvoice)
		{
			RecalculatingContractRetainage = true;
			try
			{
				decimal roundingOverflow = 0;
				
				foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
				{
					PMProformaProgressLine line = (PMProformaProgressLine)res;
					line = (PMProformaProgressLine) ProgressiveLines.Cache.CreateCopy(line);//Use Copy instance since we are calling Update in the context on RowUpdated (outer caller).
					Tuple<decimal, decimal> retaiangeOnLine = CalculateContractRetainageOnLine(project, line, totalRetainageUptoDate, contractAmount, totalRetainageOnInvoice, roundingOverflow);
					line.CuryRetainage = GetAmountInBillingCurrency(retaiangeOnLine.Item1);
					roundingOverflow = retaiangeOnLine.Item2;
					line = ProgressiveLines.Update(line);
				}
			}
			finally
			{
				RecalculatingContractRetainage = false;
			}
		}

		/// <summary>
		/// effectiveRetainage in project currency
		/// </summary>
		protected virtual Tuple<decimal, decimal> CalculateContractRetainageOnLine(PMProject project, PMProformaProgressLine line, decimal totalRetainageUptoDate, decimal contractAmount, decimal totalRetainageOnInvoice, decimal roundingOverflow)
		{
			decimal result = 0;
			
			decimal totalRetainageCap = decimal.Round(contractAmount * 0.01m * project.RetainagePct.GetValueOrDefault() * project.RetainageMaxPct.GetValueOrDefault() * 0.01m, 2);

			decimal lineRetainage = GetAmountInProjectCurrency(decimal.Round(line.CuryLineTotal.GetValueOrDefault() * project.RetainagePct.GetValueOrDefault() * 0.01m, 2));
			
			if (totalRetainageOnInvoice <= 0 || totalRetainageOnInvoice + totalRetainageUptoDate <= totalRetainageCap)
			{
				//within limits.
				result = lineRetainage;
			}
			else
			{
				decimal overLimitRetainage = totalRetainageOnInvoice + totalRetainageUptoDate - totalRetainageCap;
				decimal ratio = lineRetainage / totalRetainageOnInvoice;
				decimal decrease = overLimitRetainage * ratio;

				decimal effectiveRetainage = Math.Max(0, decimal.Round(lineRetainage - decrease + roundingOverflow, 2));
				roundingOverflow = (lineRetainage - decrease + roundingOverflow) - effectiveRetainage;
				
				result = effectiveRetainage;
			}

			return new Tuple<decimal, decimal>(result, roundingOverflow);
		}

		private decimal GetTotalRetainageOnInvoice(decimal retainagePct)
		{
			decimal totalRetainageOnInvoice = 0;

			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				PMProformaProgressLine line = (PMProformaProgressLine)res;

				decimal lineRetainage = line.CuryLineTotal.GetValueOrDefault() * retainagePct * 0.01m;

				if (lineRetainage > 0)
				{
					totalRetainageOnInvoice += lineRetainage;
				}
			}

			return totalRetainageOnInvoice;
		}

		protected virtual void ReAllocateContractRetainage(PMProject project, PMProjectRevenueTotal budget)
		{
			decimal totalInvoiceUptoDate = GetTotalInvoiced(budget);//project currency
			decimal retainageToAllocate = GetTotalRetainageUptoDate(budget, true);//project currency
			decimal retainageReleased = GetBilledRetainageToDateTotal(Document.Current.InvoiceDate);//document currency

			ReAllocateContractRetainage(project, totalInvoiceUptoDate, retainageToAllocate - GetAmountInProjectCurrency(retainageReleased));
		}

		protected virtual void ReAllocateContractRetainage(PMProject project, decimal totalInvoiceUptoDate, decimal retainageToAllocate)
		{
			decimal roundingOverflow = 0;//project currency
			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				PMProformaProgressLine line = (PMProformaProgressLine)res;
				PMRevenueBudget revenue = (PMRevenueBudget)res;

				decimal invoicedLineTotal = GetInvoicedAmount(revenue);//project currency
				decimal allocateRaw = roundingOverflow + retainageToAllocate * invoicedLineTotal / totalInvoiceUptoDate;
				decimal allocate = decimal.Round(allocateRaw, 2);
				roundingOverflow = allocateRaw - allocate;
				ProgressiveLines.Cache.SetValue<PMProformaLine.curyAllocatedRetainedAmount>(line, GetAmountInBillingCurrency(allocate));
			}

			Document.Cache.SetValue<PMProforma.curyAllocatedRetainedTotal>(Document.Current, GetAmountInBillingCurrency(retainageToAllocate));
		}

		private decimal GetInvoicedAmount(PMRevenueBudget budget)
		{
			decimal invoicedTotal = budget.CuryInvoicedAmount.GetValueOrDefault()
							+ GetCuryActualAmountWithTaxes(budget)
							+ budget.CuryAmountToInvoice.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				if (item.ProjectTaskID == budget.ProjectTaskID &&
					item.AccountGroupID == budget.AccountGroupID &&
					item.InventoryID == budget.InventoryID &&
					item.CostCodeID == budget.CostCodeID)
				{
					invoicedTotal += item.CuryInvoicedAmount.GetValueOrDefault() + item.CuryAmountToInvoice.GetValueOrDefault();
				}
			}

			return invoicedTotal;
		}

		protected virtual decimal GetCuryActualAmountWithTaxes(PMRevenueBudget row)
			=> row.CuryActualAmount.GetValueOrDefault()
			 + row.CuryInclTaxAmount.GetValueOrDefault();

		protected virtual decimal GetCuryActualAmountWithTaxes(PMProjectRevenueTotal row)
			=> row.CuryActualAmount.GetValueOrDefault()
			 + row.CuryInclTaxAmount.GetValueOrDefault();

		private decimal GetTotalInvoiced(PMProjectRevenueTotal budget)
		{
			decimal invoicedTotal = GetCuryActualAmountWithTaxes(budget) + budget.CuryInvoicedAmount.GetValueOrDefault();

			foreach (PMBudgetAccum item in Budget.Cache.Inserted)
			{
				invoicedTotal += item.CuryInvoicedAmount.GetValueOrDefault();
			}

			return invoicedTotal;
		}
		
		protected virtual void RecalculateLineRetainage(PMProject project)
		{
			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in ProgressiveLines.Select())
			{
				PMProformaProgressLine line = (PMProformaProgressLine)res;
				PMRevenueBudget revenue = (PMRevenueBudget)res;

				decimal lineRetainage = line.CuryLineTotal.GetValueOrDefault() * line.RetainagePct.GetValueOrDefault() * 0.01m;
				decimal maxLineRetainage = GetLineAmount(project, revenue) * revenue.RetainagePct.GetValueOrDefault() * 0.01m * revenue.RetainageMaxPct.GetValueOrDefault() * 0.01m;

				//TODO MC support

				if (lineRetainage + revenue.CuryTotalRetainedAmount.GetValueOrDefault() <= maxLineRetainage)
				{
					line.CuryRetainage = lineRetainage;
				}
				else
				{
					line.CuryRetainage = maxLineRetainage - revenue.CuryTotalRetainedAmount.GetValueOrDefault();
				}

				ProgressiveLines.Update(line);
			}
		}

		private decimal GetBilledRetainageToDateTotal(DateTime? cutoffDate)
        {
			decimal total = 0;
			foreach ( decimal val in GetBilledRetainageToDate(cutoffDate).Values)
            {
				total += val;
            }

			return total;
        }

		public virtual Dictionary<ProformaTotalsCounter.AmountBaseKey, decimal> GetBilledRetainageToDate(DateTime? cutoffDate)
		{
			var billedByTask = new Dictionary<ProformaTotalsCounter.AmountBaseKey, decimal>();

			var selectBilledRetainageByLine = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARInvoice, On<ARTran.tranType, Equal<ARInvoice.docType>, And<ARTran.refNbr, Equal<ARInvoice.refNbr>>>,
				InnerJoin<RetainageOriginalARTran, On<ARTran.origDocType, Equal<RetainageOriginalARTran.tranType>, And<ARTran.origRefNbr, Equal<RetainageOriginalARTran.refNbr>, And<ARTran.origLineNbr, Equal<RetainageOriginalARTran.lineNbr>>>>,
				InnerJoin<Account, On<RetainageOriginalARTran.accountID, Equal<Account.accountID>>>>>,
				Where<ARInvoice.isRetainageDocument, Equal<True>,
				And<ARInvoice.released, Equal<True>,
				And<ARInvoice.paymentsByLinesAllowed, Equal<True>,
				And<ARInvoice.docDate, LessEqual<Required<ARInvoice.docDate>>,
				And<ARInvoice.projectID, Equal<Current<PMProforma.projectID>>>>>>>,
				Aggregate<GroupBy<ARTran.tranType, GroupBy<ARTran.taskID, GroupBy<ARTran.costCodeID, GroupBy<ARTran.inventoryID, GroupBy<Account.accountGroupID, Sum<ARTran.curyTranAmt>>>>>>>>(this);

			foreach (PXResult<ARTran, ARInvoice, RetainageOriginalARTran, Account> res in selectBilledRetainageByLine.Select(cutoffDate))
			{
				ARTran tran = (ARTran)res;
				Account account = (Account)res;
				var key = new ProformaTotalsCounter.AmountBaseKey(tran.TaskID.GetValueOrDefault(), tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.DefaultCostCode.GetValueOrDefault()), tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID), account.AccountGroupID.GetValueOrDefault());

				decimal amount = tran.CuryTranAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);
				if (billedByTask.ContainsKey(key))
				{
					billedByTask[key] += amount;
				}
				else
				{
					billedByTask.Add(key, amount);
				}
			}

			var selectBilledRetainage = new PXSelectGroupBy<ARInvoice,
				Where<ARInvoice.isRetainageDocument, Equal<True>,
				And<ARInvoice.released, Equal<True>,
				And<ARInvoice.paymentsByLinesAllowed, Equal<False>,
				And<ARInvoice.docDate, LessEqual<Required<ARInvoice.docDate>>,
				And<ARInvoice.projectID, Equal<Current<PMProforma.projectID>>>>>>>,
				Aggregate<GroupBy<ARInvoice.docType, Sum<ARInvoice.curyLineTotal>>>>(this);

			decimal? total = null;
			foreach (ARInvoice sum in selectBilledRetainage.Select(cutoffDate))
			{
				total = total.GetValueOrDefault() + sum.CuryLineTotal.GetValueOrDefault() * ARDocType.SignAmount(sum.DocType).GetValueOrDefault(1);
			}

			if (total != null && total.GetValueOrDefault() != 0)
			{
				billedByTask.Add(PayByLineOffKey, total.Value);
			}

			return billedByTask;
		}

		protected virtual decimal GetContractAmount(PMProject project, PMProjectRevenueTotal budget)
		{
			if (project.IncludeCO == true)
			{
				return budget.CuryRevisedAmount.GetValueOrDefault();
			}
			else
			{
				return budget.CuryAmount.GetValueOrDefault();
			}
		}

		protected virtual decimal GetLineAmount(PMProject project, PMBudget budget)
		{
			if (project.IncludeCO == true)
			{
				return budget.CuryRevisedAmount.GetValueOrDefault();
			}
			else
			{
				return budget.CuryAmount.GetValueOrDefault();
			}
		}

		#endregion

		private bool BillingInAnotherCurrency
		{
			get
			{
				if (Document.Current != null && Document.Current.ProjectID != null)
				{
					if (Project.Current.CuryID != Project.Current.BillingCuryID)
					{
						return true;
					}
				}

				return false;
			}
		}

		public virtual void CheckMigrationMode()
		{
			if (ARSetup.Current.MigrationMode == true)
			{
				throw new PXException(Messages.ActiveMigrationMode);
			}
		}

		private void Unbill(PMTran tran)
		{
			tran.Billed = false;
			tran.BilledDate = null;
			tran.BillingID = null;
			tran.ProformaRefNbr = null;
			tran.ProformaLineNbr = null;        
			tran.Selected = false;

            PX.Objects.PM.RegisterReleaseProcess.AddToUnbilledSummary(this, tran);
            this.Details.Update(tran);
		}

		#region External Tax Provider


		public bool RecalculateExternalTaxesSync { get; set; }

		public virtual bool IsExternalTax(string taxZoneID)
		{
					return false;
			}

		public virtual PMProforma CalculateExternalTax(PMProforma doc)
		{
			return doc;
		}
		#endregion

		#region Local Types
		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public class PMProformaOverflow : PX.Data.IBqlTable
		{			
			#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

			[PXDBLong()]
			[CurrencyInfo(typeof(PMProforma.curyInfoID))]
			public virtual Int64? CuryInfoID
			{
				get;
				set;
			}
			#endregion
			
			#region CuryOverflowTotal
			public abstract class curyOverflowTotal : PX.Data.BQL.BqlDecimal.Field<curyOverflowTotal> { }
			[PXCurrency(typeof(curyInfoID), typeof(overflowTotal), BaseCalc = false)]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Over-Limit Total", Enabled = false, Visible = false)]
			public virtual Decimal? CuryOverflowTotal
			{
				get; set;
			}
			#endregion
			#region OverflowTotal
			public abstract class overflowTotal : PX.Data.BQL.BqlDecimal.Field<overflowTotal> { }
			[PXBaseCury]
			[PXUnboundDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Overflow Total in Base Currency", Enabled = false, Visible = false)]
			public virtual Decimal? OverflowTotal
			{
				get; set;
			}
			#endregion
		}

		public class ProformaTotalsCounter
		{
			[System.Diagnostics.DebuggerDisplay("{TaskID}.{CostCodeID}.{InventoryID}.{AccountGroupID}")]
			[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
			public class AmountBaseKey
			{
				public readonly int TaskID;
				public readonly int InventoryID;
				public readonly int CostCodeID;
				public readonly int AccountGroupID;

				public AmountBaseKey(int taskID, int costCodeID, int inventoryID, int accountGroupID)
				{
					TaskID = taskID;
					InventoryID = inventoryID;
					CostCodeID = costCodeID;
					AccountGroupID = accountGroupID;
				}

				public AmountBaseKey(PMProformaLine line) :
					this(line.TaskID.GetValueOrDefault(),
						line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()),
						line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID),
						line.AccountGroupID.GetValueOrDefault())
				{ }

				public override int GetHashCode()
				{
					unchecked // Overflow is fine, just wrap
					{
						int hash = 17;
						hash = hash * 23 + TaskID.GetHashCode();
						hash = hash * 23 + InventoryID.GetHashCode();
						hash = hash * 23 + CostCodeID.GetHashCode();
						hash = hash * 23 + AccountGroupID.GetHashCode();
						return hash;
					}
				}

				public override bool Equals(object obj)
				{
					if(obj is AmountBaseKey otherKey)
					{
						return object.ReferenceEquals(this, otherKey) ||
							(otherKey.AccountGroupID == AccountGroupID &&
							otherKey.TaskID == TaskID &&
							otherKey.CostCodeID == CostCodeID &&
							otherKey.InventoryID == InventoryID);
					}
					return false;
				}
			}

			public struct AmountBaseTotals
			{
				public AmountBaseKey Key { get; set; }
				public decimal CuryRetainage { get; set; }
				public decimal Retainage { get; set; }
				public decimal CuryLineTotal { get; set; }
				public decimal LineTotal { get; set; }
			}

			[System.Diagnostics.DebuggerDisplay("{TaskID}.{CostCodeID}.{InventoryID}.{AccountGroupID}.{UOM}")]
			[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
			public class QuantityBaseKey : AmountBaseKey
			{
				public readonly string UOM;

				public QuantityBaseKey(PMProformaLine line) : base(line)
				{
					UOM = line.UOM ?? string.Empty;
				}

				public QuantityBaseKey(int taskID, int costCodeID, int inventoryID, int accountGroupID, string uom) :
					base(taskID, costCodeID, inventoryID, accountGroupID)
				{
					UOM = uom ?? string.Empty;
				}

				public override int GetHashCode()
				{
					unchecked // Overflow is fine, just wrap
					{
						int hash = base.GetHashCode() * 23 + UOM.GetHashCode();
						return hash;
					}
				}

				public override bool Equals(object obj)
				{
					if (obj is QuantityBaseKey otherKey)
					{
						return base.Equals(otherKey) && otherKey.UOM == UOM;
					}
					return false;
				}
			}

			public struct QuantityBaseTotals
			{
				public QuantityBaseKey Key { get; set; }
				public decimal CuryRetainage { get; set; }
				public decimal Retainage { get; set; }
				public decimal CuryLineTotal { get; set; }
				public decimal LineTotal { get; set; }
				public decimal QuantityTotal { get; set; }
			}

			private Dictionary<AmountBaseKey, AmountBaseTotals> AmtBaseTotals = null;
			private Dictionary<QuantityBaseKey, QuantityBaseTotals> QtyBaseTotals = null;
			private string LastTotalsKey = null;

			public QuantityBaseTotals GetQuantityBaseTotals(PXGraph graph, string proformaRefNbr, PMProformaProgressLine progressLine)
			{
				if (QtyBaseTotals == null || proformaRefNbr != LastTotalsKey)
				{
					LastTotalsKey = proformaRefNbr;

					PXSelectBase<PMProformaLine> totalsPreviousSelect = new PXSelectJoinGroupBy<PMProformaLine,
							InnerJoin<PMProforma, On<PMProformaLine.refNbr, Equal<PMProforma.refNbr>,
								And<PMProformaLine.revisionID, Equal<PMProforma.revisionID>,
								And<PMProforma.curyID, Equal<Current<PMProforma.curyID>>>>>>,
							Where<PMProformaLine.refNbr, Less<Current<PMProforma.refNbr>>,
							And<PMProformaLine.projectID, Equal<Current<PMProforma.projectID>>,
							And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
							And<PMProformaLine.corrected, NotEqual<True>>>>>,
							Aggregate<GroupBy<PMProformaLine.taskID,
							GroupBy<PMProformaLine.costCodeID,
							GroupBy<PMProformaLine.inventoryID,
							GroupBy<PMProformaLine.accountGroupID,
							GroupBy<PMProformaLine.uOM,
							Sum<PMProformaLine.curyRetainage,
							Sum<PMProformaLine.retainage,
							Sum<PMProformaLine.curyLineTotal,
							Sum<PMProformaLine.lineTotal,
							Sum<PMProformaLine.qty>>>>>>>>>>>>(graph);

					QtyBaseTotals = totalsPreviousSelect
						.Select()
						.RowCast<PMProformaLine>()
						.Select(line =>
						{
							return new QuantityBaseTotals()
							{
								Key = new QuantityBaseKey(line),
								CuryRetainage = line.CuryRetainage.GetValueOrDefault(),
								Retainage = line.Retainage.GetValueOrDefault(),
								CuryLineTotal = line.CuryLineTotal.GetValueOrDefault(),
								LineTotal = line.LineTotal.GetValueOrDefault(),
								QuantityTotal = line.Qty.GetValueOrDefault(),
							};
						})
						.ToDictionary(t => t.Key);
				}

				QtyBaseTotals.TryGetValue(new QuantityBaseKey(progressLine), out QuantityBaseTotals value);
				return value;
			}

			public virtual AmountBaseTotals GetAmountBaseTotals(PXGraph graph, string proformaRefNbr, PMProformaProgressLine progressLine)
			{
				if (AmtBaseTotals == null || proformaRefNbr != LastTotalsKey)
				{
					LastTotalsKey = proformaRefNbr;
					AmtBaseTotals = new Dictionary<AmountBaseKey, AmountBaseTotals>();

					PXSelectBase<PMProformaLine> totalsPreviousSelect = new PXSelectJoinGroupBy<PMProformaLine,
						InnerJoin<PMProforma, On<PMProformaLine.refNbr, Equal<PMProforma.refNbr>,
							And<PMProformaLine.revisionID, Equal<PMProforma.revisionID>,
							And<PMProforma.curyID, Equal<Current<PMProforma.curyID>>>>>>,
						Where<PMProformaLine.refNbr, Less<Current<PMProforma.refNbr>>,
						And<PMProformaLine.projectID, Equal<Current<PMProforma.projectID>>,
						And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
						And<PMProformaLine.corrected, NotEqual<True>>>>>,
						Aggregate<GroupBy<PMProformaLine.taskID,
						GroupBy<PMProformaLine.costCodeID,
						GroupBy<PMProformaLine.inventoryID,
						GroupBy<PMProformaLine.accountGroupID,
						Sum<PMProformaLine.curyRetainage,
						Sum<PMProformaLine.retainage,
						Sum<PMProformaLine.curyLineTotal,
						Sum<PMProformaLine.lineTotal,
						Sum<PMProformaLine.qty>>>>>>>>>>>(graph);

					foreach (PMProformaLine line in totalsPreviousSelect.Select())
					{
						AmountBaseTotals totals = new AmountBaseTotals();
						totals.Key = new AmountBaseKey(line);
						totals.CuryRetainage = line.CuryRetainage.GetValueOrDefault();
						totals.Retainage = line.Retainage.GetValueOrDefault();
						totals.CuryLineTotal = line.CuryLineTotal.GetValueOrDefault();
						totals.LineTotal = line.LineTotal.GetValueOrDefault();

						AmtBaseTotals.Add(totals.Key, totals);
					}
				}

				AmtBaseTotals.TryGetValue(new AmountBaseKey(progressLine), out AmountBaseTotals value);
				return value;
			}
		}

		[PXHidden]
		public class RetainageOriginalARTran : ARTran
		{
			public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		}
		#endregion
	}

	[PXDynamicButton(new string[] { ProgressivePasteLineCommand, ProgressiveResetOrderCommand },
					 new string[] { PX.Data.ActionsMessages.PasteLine, PX.Data.ActionsMessages.ResetOrder },
					 TranslationKeyType = typeof(PX.Objects.Common.Messages))]
	public class ProgressLineSelect : PXOrderedSelect<PMProforma, PMProformaProgressLine,
			LeftJoin<PMRevenueBudget, On<PMProformaProgressLine.projectID, Equal<PMRevenueBudget.projectID>,
				And<PMProformaProgressLine.taskID, Equal<PMRevenueBudget.projectTaskID>,
				And<PMProformaProgressLine.accountGroupID, Equal<PMRevenueBudget.accountGroupID>,
				And<PMProformaProgressLine.inventoryID, Equal<PMRevenueBudget.inventoryID>,
				And<PMProformaProgressLine.costCodeID, Equal<PMRevenueBudget.costCodeID>>>>>>>,
			Where<PMProformaProgressLine.refNbr, Equal<Current<PMProforma.refNbr>>,
				And<PMProformaProgressLine.revisionID, Equal<Current<PMProforma.revisionID>>,
				And<PMProformaProgressLine.type, Equal<PMProformaLineType.progressive>>>>, OrderBy<Asc<PMProformaProgressLine.sortOrder, Asc<PMProformaProgressLine.lineNbr>>>>
	{
		public ProgressLineSelect(PXGraph graph) : base(graph) { }
		public ProgressLineSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public const string ProgressivePasteLineCommand = "ProgressPasteLine";
		public const string ProgressiveResetOrderCommand = "ProgressResetOrder";

		protected override void AddActions(PXGraph graph)
		{
			AddAction(graph, ProgressivePasteLineCommand, PX.Data.ActionsMessages.PasteLine, PasteLine);
			AddAction(graph, ProgressiveResetOrderCommand, PX.Data.ActionsMessages.ResetOrder, ResetOrder);
		}
	}

	[PXDynamicButton(new string[] { TransactPasteLineCommand, TransactResetOrderCommand },
					 new string[] { PX.Data.ActionsMessages.PasteLine, PX.Data.ActionsMessages.ResetOrder },
					 TranslationKeyType = typeof(PX.Objects.Common.Messages))]
	public class TransactLineSelect : PXOrderedSelect<PMProforma, PMProformaTransactLine,
			Where<PMProformaTransactLine.refNbr, Equal<Current<PMProforma.refNbr>>,
			And<PMProformaTransactLine.revisionID, Equal<Current<PMProforma.revisionID>>,
			And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>>>>,
			OrderBy<Asc<PMProformaTransactLine.sortOrder, Asc<PMProformaTransactLine.lineNbr>>>>
	{
		public TransactLineSelect(PXGraph graph) : base(graph) { }
		public TransactLineSelect(PXGraph graph, Delegate handler) : base(graph, handler) { }

		public const string TransactPasteLineCommand = "TransactPasteLine";
		public const string TransactResetOrderCommand = "TransactResetOrder";

		protected override void AddActions(PXGraph graph)
		{
			AddAction(graph, TransactPasteLineCommand, PX.Data.ActionsMessages.PasteLine, PasteLine);
			AddAction(graph, TransactResetOrderCommand, PX.Data.ActionsMessages.ResetOrder, ResetOrder);
		}
	}

	public abstract class PMActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		: ActivityDetailsExt<TGraph, TPrimaryEntity, TPrimaryEntity_NoteID>
		where TGraph : PXGraph, new()
		where TPrimaryEntity : class, IBqlTable, INotable, new()
		where TPrimaryEntity_NoteID : IBqlField, IImplement<IBqlCastableTo<IBqlGuid>>
	{
		public override object GetBAccountRow(string sourceType, CRPMTimeActivity activity)
		{
			object sourceRow = Base.Caches[typeof(TPrimaryEntity)].Current;

			if (sourceRow != null && sourceType == PMNotificationSource.Project)
			{
				int? projectID = (int?)Base.Caches[typeof(TPrimaryEntity)].GetValue(sourceRow, nameof(PMProforma.ProjectID));
				PMProject rec = PMProject.PK.Find(Base, projectID);

				if (rec != null && rec.NonProject != true && rec.BaseType == CT.CTPRType.Project)
				{
					return rec;
				}
			}

			return base.GetBAccountRow(sourceType, activity);
		}

		public virtual bool IsProjectSourceActive(int? projectID, string notificationCD)
		{
			var select = new PXSelectJoin<NotificationSource,
				InnerJoin<NotificationSetup, On<NotificationSource.setupID, Equal<NotificationSetup.setupID>>,
				InnerJoin<PMProject, On<PMProject.noteID, Equal<NotificationSource.refNoteID>>>>,
				Where<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
				And<PMProject.contractID, Equal<Required<PMProject.contractID>>,
				And<NotificationSource.active, Equal<True>>>>>(Base);

			NotificationSource source = select.SelectSingle(notificationCD, projectID);

			return source != null;
		}

		public virtual string ProjectInvoiceReportActive(int? projectID)
		{
			var select = new PXSelectJoin<NotificationSource,
				InnerJoin<NotificationSetup, On<NotificationSource.setupID, Equal<NotificationSetup.setupID>>,
				InnerJoin<PMProject, On<PMProject.noteID, Equal<NotificationSource.refNoteID>>>>,
				Where<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>,
				And<PMProject.contractID, Equal<Required<PMProject.contractID>>,
				And<NotificationSource.active, Equal<True>>>>>(Base);

			NotificationSource source = select.SelectSingle("INVOICE", projectID);

			if (source != null)
				return source.ReportID;
			else
				return null;
		}

		public virtual string GetDefaultProjectInvoiceReport()
		{
			return "PM641000";
		}
	}

	public class ProformaAppender : PMBillEngine
	{
		public void SetProformaEntry(ProformaEntry proformaEntry)
		{
			this.proformaEntry = proformaEntry;
		}

		public override List<PMTran> SelectBillingBase(int? projectID, int? taskID, int? accountGroupID, bool includeNonBillable)
		{
			List<PMTran> list = new List<PMTran>();
			
			foreach (PMTran tran in proformaEntry.Unbilled.Cache.Updated)
			{
				if (tran.Selected == true && tran.Billed != true && tran.ExcludedFromBilling != true && tran.TaskID == taskID && tran.AccountGroupID == accountGroupID)
				{
					list.Add(tran);
				}
			}

			return list;
		}

		public void InitRateEngine(IList<string> distinctRateTables, IList<string> distinctRateTypes)
		{
			rateEngine = CreateRateEngineV2(distinctRateTables, distinctRateTypes);
		}
	}
}
