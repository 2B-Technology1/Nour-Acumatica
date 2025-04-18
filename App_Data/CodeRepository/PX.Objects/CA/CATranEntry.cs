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
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.TM;
using PX.CS.Contracts.Interfaces;
using PX.Data.WorkflowAPI;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.TaxProvider;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Common;
using PX.Objects.CR.Extensions;

namespace PX.Objects.CA
{
	public class CATranEntry : PXGraph<CATranEntry, CAAdj>, PX.Objects.GL.IVoucherEntry
	{
		private bool IsReverseContext { get; set; }

		internal class ReverseContext : IDisposable
		{
			private readonly CATranEntry _graph;

			public ReverseContext(CATranEntry graph)
			{
				_graph = graph;
				_graph.IsReverseContext = true;
			}

			public void Dispose()
			{
				_graph.IsReverseContext = false;
			}
		}

		public PXAction DeleteButton
		{
			get
			{
				return this.Delete;
			}
		}
		
		#region Cache Attached Events
		[Account(
			typeof(CASplit.branchID),
			typeof(Search<Account.accountID,
				Where2<Where<Account.curyID, Equal<Current<CAAdj.curyID>>, Or<Account.curyID, IsNull>>,
                    And<Match<Current<AccessInfo.userName>>>>>),
			DisplayName = "Offset Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description), 
			CacheGlobal = false,
			AvoidControlAccounts = true)]
		[PXDefault()]
		protected virtual void CASplit_AccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		
		public PXCache dailycache
		{
			get
			{
				return this.Caches[typeof(CADailySummary)];
			}
		}

		public PXCache catrancache
		{
			get
			{
				return this.Caches[typeof(CATran)];
			}
		}

		public PXCache gltrancache
		{
			get
			{
				return this.Caches[typeof(GLTran)];
			}
		}

		public CATranEntry()
		{
			CASetup setup = casetup.Current;
			OpenPeriodAttribute.SetValidatePeriod<CAAdj.finPeriodID>(CAAdjRecords.Cache, null, PeriodValidation.DefaultSelectUpdate);
			PXUIFieldAttribute.SetDisplayName<Account.description>(Caches[typeof(Account)], Messages.AccountDescription);
			this.FieldSelecting.AddHandler<CAAdj.tranID_CATran_batchNbr>(CAAdj_TranID_CATran_BatchNbr_FieldSelecting);
			PXUIFieldAttribute.SetVisible<CASplit.projectID>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));
			PXUIFieldAttribute.SetVisible<CASplit.taskID>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));
			PXUIFieldAttribute.SetVisible<CASplit.nonBillable>(CASplitRecords.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.CA));

			Approval.Cache.AllowUpdate = false;
            Approval.Cache.AllowInsert = false;
            Approval.Cache.AllowDelete = false;
            
			CAExpenseHelper.InitBackwardEditorHandlers(this);
        }

		#region Extensions

		public class CATranEntryDocumentExtension : DocumentWithLinesGraphExtension<CATranEntry>
		{
			#region Mapping

			public override void Initialize()
			{
				base.Initialize();

				Documents = new PXSelectExtension<Document>(Base.CAAdjRecords);
				Lines = new PXSelectExtension<DocumentLine>(Base.CASplitRecords);
			}

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(CAAdj))
				{
					HeaderTranPeriodID = typeof(CAAdj.tranPeriodID),
					HeaderDocDate = typeof(CAAdj.tranDate)
				};
			}

			protected override DocumentLineMapping GetDocumentLineMapping()
			{
				return new DocumentLineMapping(typeof(CASplit));
			}

			#endregion

			protected override bool ShouldUpdateLinesOnDocumentUpdated(Events.RowUpdated<Document> e)
			{
				return base.ShouldUpdateLinesOnDocumentUpdated(e)
					   || !e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow);
			}

			protected override void ProcessLineOnDocumentUpdated(Events.RowUpdated<Document> e,
			    DocumentLine line)
			{
				base.ProcessLineOnDocumentUpdated(e, line);

				if (!e.Cache.ObjectsEqual<Document.headerDocDate>(e.Row, e.OldRow))
				{
					Lines.Cache.SetDefaultExt<DocumentLine.tranDate>(line);
				}
			}
		}

		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class CATranEntry_ActivityDetailsExt : ActivityDetailsExt<CATranEntry, CAAdj, CAAdj.noteID>
		{
			public PXAction<CAAdj> ViewActivities;
			[PXButton(CommitChanges = true), PXUIField(DisplayName = "Activities", MapEnableRights = PXCacheRights.Select)]
			public virtual IEnumerable viewActivities(PXAdapter adapter)
			{
				if (Base.CAAdjRecords.Current != null)
				{
					return this.ViewAllActivities.Press(adapter);
				}

				return adapter.Get();
			}
		}

		#endregion

		public TaxZone TAXZONE
		{
			get
			{
				return taxzone.Select();
			}
		}
		#region Worflow Buttons
		public PXInitializeState<CAAdj> initializeState;
		
		public PXAction<CAAdj> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<CAAdj> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();

		public PXWorkflowEventHandler<CAAdj> OnReleaseDocument;
		public PXWorkflowEventHandler<CAAdj> OnUpdateStatus;
		#endregion
		
		#region Buttons
		
		public PXAction<CAAdj> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			Save.Press();

			CAAdj adj = CAAdjRecords.Current;
			List<CARegister> registerList = new List<CARegister>();
			registerList.Add(CATrxRelease.CARegister(adj));
			PXLongOperation.StartOperation(this, () => CATrxRelease.GroupRelease(registerList, false));
			List<CAAdj> ret = new List<CAAdj>();
			ret.Add(adj);
			return ret;
		}
		protected bool reversingContext;
		public PXAction<CAAdj> Reverse;
		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable reverse(PXAdapter adapter)
		{
			CAAdj current = CAAdjRecords.Current;
			if (current?.Released != true)
				return adapter.Get();

			if (!AskUserApprovalToReverse(current))
			{
				return adapter.Get();
			}

			using (new ReverseContext(this))
			{
				return ReverseTransaction(current);
			}
		}

		private IEnumerable ReverseTransaction(CAAdj transaction)
		{
			CAAdj adj = (CAAdj)CAAdjRecords.Cache.CreateCopy(CAAdjRecords.Current);
			SetAdjFields(transaction, adj);
			SetCleared(adj);
			List<Tuple<CASplit, CASplit>> splits = new List<Tuple<CASplit, CASplit>>();
			foreach (CASplit split in CASplitRecords.Select())
			{
				CASplit newSplit = (CASplit)CASplitRecords.Cache.CreateCopy(split);
				newSplit.AdjRefNbr = null;
				newSplit.CuryInfoID = null;
				newSplit.NoteID = null;
				newSplit.CuryTranAmt *= -1;
				newSplit.CuryUnitPrice *= -1;
				newSplit.CuryTaxAmt *= -1;
				newSplit.CuryTaxableAmt *= -1;
				newSplit.TranAmt *= -1;
				newSplit.UnitPrice *= -1;
				newSplit.TaxAmt *= -1;
				newSplit.TaxableAmt *= -1;
				newSplit.AccountID = split.AccountID;
				newSplit.SubID = split.SubID;
				splits.Add(new Tuple<CASplit, CASplit>(split, newSplit));
			}
			List<CATaxTran> taxes = new List<CATaxTran>();
			foreach (CATaxTran taxTran in Taxes.Select())
			{
				CATaxTran newTaxTran = new CATaxTran();
				newTaxTran.AccountID = taxTran.AccountID;
				newTaxTran.BranchID = taxTran.BranchID;
				newTaxTran.FinPeriodID = taxTran.FinPeriodID;
				newTaxTran.SubID = taxTran.SubID;
				newTaxTran.TaxBucketID = taxTran.TaxBucketID;
				newTaxTran.TaxID = taxTran.TaxID;
				newTaxTran.TaxType = taxTran.TaxType;
				newTaxTran.TaxZoneID = taxTran.TaxZoneID;
				newTaxTran.TranDate = taxTran.TranDate;
				newTaxTran.VendorID = taxTran.VendorID;
				newTaxTran.CuryID = taxTran.CuryID;
				newTaxTran.Description = taxTran.Description;
				newTaxTran.NonDeductibleTaxRate = taxTran.NonDeductibleTaxRate;
				newTaxTran.TaxRate = taxTran.TaxRate;
				newTaxTran.CuryTaxableAmt = -taxTran.CuryTaxableAmt;
				newTaxTran.CuryExemptedAmt = -taxTran.CuryExemptedAmt;
				newTaxTran.CuryTaxAmt = -taxTran.CuryTaxAmt;
				newTaxTran.CuryTaxAmtSumm = -taxTran.CuryTaxAmtSumm;
				newTaxTran.CuryExpenseAmt = -taxTran.CuryExpenseAmt;
				newTaxTran.TaxableAmt = -taxTran.TaxableAmt;
				newTaxTran.ExemptedAmt = -taxTran.ExemptedAmt;
				newTaxTran.TaxAmt = -taxTran.TaxAmt;
				newTaxTran.ExpenseAmt = -taxTran.ExpenseAmt;

				taxes.Add(newTaxTran);
			}

			finperiod.Cache.Current = finperiod.View.SelectSingleBound(new object[] { adj });
			FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<Batch.finPeriodID, Batch.branchID>(CAAdjRecords.Cache, adj, finperiod);

			Clear();
			reversingContext = true;
			CAAdj insertedAdj = CAAdjRecords.Insert(adj);
			PXNoteAttribute.CopyNoteAndFiles(CAAdjRecords.Cache, transaction, CAAdjRecords.Cache, insertedAdj);
			foreach (Tuple<CASplit, CASplit> pair in splits)
			{
				CASplit newSplit = pair.Item2;

				InventoryItem item = InventoryItem.PK.Find(this, newSplit.InventoryID);
				if (item?.IsConverted == true && item.StkItem == true)
					newSplit.InventoryID = null;

				newSplit = CASplitRecords.Insert(newSplit);
				PXNoteAttribute.CopyNoteAndFiles(CASplitRecords.Cache, pair.Item1, CASplitRecords.Cache, newSplit);
			}
			reversingContext = false;
			foreach (CATaxTran newTaxTran in taxes)
			{
				Taxes.Insert(newTaxTran);
			}
			//We should reenter totals depending on taxes as TaxAttribute does not recalculate them if externalCall==false
			CAAdjRecords.Cache.SetValue<CAAdj.taxRoundDiff>(insertedAdj, adj.TaxRoundDiff);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTaxRoundDiff>(insertedAdj, adj.CuryTaxRoundDiff);
			CAAdjRecords.Cache.SetValue<CAAdj.taxTotal>(insertedAdj, adj.TaxAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTaxTotal>(insertedAdj, adj.CuryTaxAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.tranAmt>(insertedAdj, adj.TranAmt);
			CAAdjRecords.Cache.SetValue<CAAdj.curyTranAmt>(insertedAdj, adj.CuryTranAmt);
			insertedAdj = CAAdjRecords.Update(insertedAdj);

			FinPeriodUtils.CopyPeriods<CAAdj, CAAdj.finPeriodID, CAAdj.tranPeriodID>(CAAdjRecords.Cache, transaction, insertedAdj);


			return new List<CAAdj> { insertedAdj };
		}

		protected virtual void SetAdjFields(CAAdj current, CAAdj adj)
		{
			adj.AdjRefNbr = null;
			adj.Status = null;
			adj.Approved = null;
			adj.Hold = null;
			adj.Released = null;
			adj.TranID = null;
			adj.NoteID = null;
			adj.CurySplitTotal = null;
			adj.CuryVatExemptTotal = null;
			adj.CuryVatTaxableTotal = null;
			adj.SplitTotal = null;
			adj.VatExemptTotal = null;
			adj.VatTaxableTotal = null;
			adj.CuryTaxRoundDiff *= -1;
			adj.CuryControlAmt *= -1;
			adj.CuryTaxAmt *= -1;
			adj.CuryTaxTotal *= -1;
			adj.CuryTranAmt *= -1;
			adj.TaxRoundDiff *= -1;
			adj.TaxAmt *= -1;
			adj.ControlAmt *= -1;
			adj.TaxTotal *= -1;
			adj.TranAmt *= -1;
			adj.EmployeeID = null;
			adj.OrigAdjTranType = current.AdjTranType;
			adj.OrigAdjRefNbr = current.AdjRefNbr;
			adj.ReverseCount = null;
			adj.DepositAsBatch = current.DepositAsBatch;
			adj.Deposited = false;
			adj.DepositDate = null;
			adj.DepositNbr = null;
			adj.Cleared = false;
			adj.ClearDate = null;
		}

		protected virtual void SetCleared(CAAdj adj)
		{
			if (cashAccount.Current.Reconcile != true)
			{
				adj.Cleared = true;
				adj.ClearDate = adj.TranDate;
			}
		}

		public PXAction<CAAdj> caReversingTransactions;
		[PXUIField(DisplayName = "CA Reversing Transactions", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CAReversingTransactions(PXAdapter adapter)
		{
			if (CAAdjRecords.Current != null)
			{
				var reportParams = new Dictionary<string, string>();
				reportParams["TranType"] = CAAdjRecords.Current.AdjTranType;
				reportParams["RefNbr"] = CAAdjRecords.Current.AdjRefNbr;

				throw new PXReportRequiredException(reportParams, "CA659000", "CA Reversing Transactions");
			}
			return adapter.Get();
		}
		#endregion

		#region Selects
		[PXViewName(Messages.CashTransactions)]
		[PXCopyPasteHiddenFields(typeof(CAAdj.cleared), typeof(CAAdj.clearDate), typeof(CAAdj.depositAsBatch), typeof(CAAdj.depositAfter),
									typeof(CAAdj.depositDate), typeof(CAAdj.deposited), typeof(CAAdj.depositType), typeof(CAAdj.depositNbr))]
		public PXSelect<CAAdj, Where<CAAdj.draft, Equal<False>>> CAAdjRecords;
		[PXCopyPasteHiddenFields(typeof(CAAdj.cleared), typeof(CAAdj.clearDate), typeof(CAAdj.depositAsBatch), typeof(CAAdj.depositAfter),
									typeof(CAAdj.depositDate), typeof(CAAdj.deposited), typeof(CAAdj.depositType), typeof(CAAdj.depositNbr))]
		public PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>> CurrentDocument;
		[PXImport(typeof(CAAdj))]
		[PXViewName(Messages.CASplit)]
		public PXSelect<CASplit, Where<CASplit.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>,
															 And<CASplit.adjTranType, Equal<Current<CAAdj.adjTranType>>>>> CASplitRecords;
		public PXSelect<CATax, Where<CATax.adjTranType, Equal<Current<CAAdj.adjTranType>>, And<CATax.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>>, OrderBy<Asc<CATax.adjTranType, Asc<CATax.adjRefNbr, Asc<CATax.taxID>>>>> Tax_Rows;
		public PXSelectJoin<CATaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, Where<CATaxTran.module, Equal<BatchModule.moduleCA>, And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>, And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>>>>> Taxes;

		// We should use read only view here
		// to prevent cache merge because it
		// used only as a shared BQL query.
		// 
		public PXSelectReadonly2<CATaxTran, 
			LeftJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, 
			Where<CATaxTran.module, Equal<BatchModule.moduleCA>, 
				And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>, 
				And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>, 
			And<Tax.taxType, Equal<CSTaxType.use>>>>>> UseTaxes;

		public PXSetup<CashAccount, Where<CashAccount.cashAccountID, Equal<Current<CAAdj.cashAccountID>>>> cashAccount;
		public PXSelect<CASetupApproval, Where<Current<CAAdj.adjTranType>, Equal<CATranType.cAAdjustment>>> SetupApproval;
        [PXViewName(Messages.Approval)]
		public EPApprovalAutomation<CAAdj, CAAdj.approved, CAAdj.rejected, CAAdj.hold, CASetupApproval> Approval;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CAAdj.curyInfoID>>>> currencyinfo;
		public PXSelect<GLVoucher, Where<True, Equal<False>>> Voucher;

		public PXSetup<OrganizationFinPeriod,
							Where<OrganizationFinPeriod.finPeriodID, Equal<Current<CAAdj.finPeriodID>>,
									And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<CAAdj.branchID>>>>>
							finperiod;

		public PXSetup<CASetup> casetup;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<CATran> catran;
		public PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Current<CAAdj.taxZoneID>>>> taxzone;

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CAExpense> caExpense;
		#endregion

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		#region EPApproval Cahce Attached
		[PXDefault(typeof(CAAdj.tranDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(Search<CREmployee.defContactID, Where<CREmployee.bAccountID.IsEqual<CAAdj.employeeID.FromCurrent>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(CAAdj.tranDesc), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_Descr_CacheAttached(PXCache sender)
		{
		}

		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(CAAdj.curyTranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_CuryTotalAmount_CacheAttached(PXCache sender)
		{
		}

		[PXDefault(typeof(CAAdj.tranAmt), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPApproval_TotalAmount_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Functions
		public bool AskUserApprovalToReverse(CAAdj origDoc)
		{
			string localizedMsg;

			if (GetReversingCAAdj(this, origDoc.AdjTranType, origDoc.AdjRefNbr).Count() >= 1)
			{
				localizedMsg = PXMessages.LocalizeNoPrefix(Messages.ReversingTransactionExists);
				return CAAdjRecords.View.Ask(localizedMsg, MessageButtons.YesNo) == WebDialogResult.Yes;
			}

			return true;
		}

		public static IEnumerable<CAAdj> GetReversingCAAdj(PXGraph graph, string tranType, string refNbr)
		{
			var reversingAdj = PXSelectReadonly<CAAdj,
										Where<CAAdj.origAdjTranType, Equal<Required<CAAdj.origAdjTranType>>,
											And<CAAdj.origAdjRefNbr, Equal<Required<CAAdj.origAdjRefNbr>>>>>
										.Select(graph, tranType, refNbr)
										.RowCast<CAAdj>();

			return reversingAdj;
		}

		public bool IsApprovalRequired(CAAdj doc, PXCache cache)
		{
			var isApprovalInstalled = PXAccess.FeatureInstalled<FeaturesSet.approvalWorkflow>();
			var areMapsAssigned = Approval.GetAssignedMaps(doc, cache).Any();
			return isApprovalInstalled && areMapsAssigned;
		}
		#endregion

		#region CATran Envents
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleCA>>>))]
		public virtual void CATran_BatchNbr_CacheAttached(PXCache sender)
		{
		}

		protected virtual void CATran_BatchNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void CATran_ReferenceID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region CAAdj Events
		protected virtual void CAAdj_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			if (casetup.Current.RequireControlTotal != true)
			{
				sender.SetValue<CAAdj.curyControlAmt>(adj, adj.CuryTranAmt);
			}
			else
			{
				if (adj.Hold != true)
					if (adj.CuryControlAmt != adj.CuryTranAmt)
					{
						sender.RaiseExceptionHandling<CAAdj.curyControlAmt>(adj, adj.CuryControlAmt, new PXSetPropertyException(Messages.DocumentOutOfBalance));
					}
					else
					{
						sender.RaiseExceptionHandling<CAAdj.curyControlAmt>(adj, adj.CuryControlAmt, null);
					}
			}

			bool checkControlTaxTotal = casetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

			if (adj.CuryTaxTotal != adj.CuryTaxAmt && adj.Hold != true && checkControlTaxTotal)
			{
				sender.RaiseExceptionHandling<CAAdj.curyTaxAmt>(adj, adj.CuryTaxAmt, new PXSetPropertyException(AP.Messages.TaxTotalAmountDoesntMatch));
			}
			else
			{
				if (checkControlTaxTotal)
				{
					sender.RaiseExceptionHandling<CAAdj.curyTaxAmt>(adj, null, null);
				}
				else
				{
					sender.SetValueExt<CAAdj.curyTaxAmt>(adj, adj.CuryTaxTotal != null && adj.CuryTaxTotal != 0 ? adj.CuryTaxTotal : 0m);
				}
			}

			sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, null, null);

			if (adj.Hold != true && adj.Released != true && adj.TaxRoundDiff != 0)
			{
				if (checkControlTaxTotal)
				{
					if (Math.Abs(adj.TaxRoundDiff.Value) > Math.Abs(CM.CurrencyCollection.GetCurrency(currencyinfo.Current.BaseCuryID).RoundingLimit.Value))
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(AP.Messages.RoundingAmountTooBig, currencyinfo.Current.BaseCuryID, PXDBQuantityAttribute.Round(adj.TaxRoundDiff),
								PXDBQuantityAttribute.Round(CM.CurrencyCollection.GetCurrency(currencyinfo.Current.BaseCuryID).RoundingLimit)));
					}
				}
				else
				{
					if (!PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(AP.Messages.CannotEditTaxAmtWOFeature));
					}
					else
					{
						sender.RaiseExceptionHandling<CAAdj.curyTaxRoundDiff>(adj, adj.CuryTaxRoundDiff,
							new PXSetPropertyException(Messages.CannotEditTaxAmtWOCASetup));
					}
				}
			}
		}

		public override void InitCacheMapping(Dictionary<Type, Type> map)
		{
			base.InitCacheMapping(map);
			map.Add(typeof(AP.Vendor), typeof(AP.Vendor));
		}

		protected virtual void CAAdj_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj == null) return;

			bool dontApprove = !IsApprovalRequired(adj, sender);
			if (adj.DontApprove != dontApprove)
			{
				sender.SetValueExt<CAAdj.dontApprove>(adj, dontApprove);
			}

			if (taxzone.Current != null && adj.TaxZoneID != taxzone.Current.TaxZoneID)
			{
				taxzone.Current = null;
			}
			bool requiredFieldsFilled = adj.CashAccountID != null && adj.EntryTypeID != null;
			bool adjNotReleased = adj.Released != true;
			bool manuallyApproved = Approval.Any() && adj.Approved == true;
			bool adjNotReleasedAndNotApproved = adjNotReleased & !manuallyApproved;

			sender.AllowDelete = adjNotReleasedAndNotApproved;

			CASplitRecords.Cache.AllowInsert = adjNotReleasedAndNotApproved && requiredFieldsFilled;
			CASplitRecords.Cache.AllowUpdate = adjNotReleasedAndNotApproved;
			CASplitRecords.Cache.AllowDelete = adjNotReleasedAndNotApproved;

			PXUIFieldAttribute.SetEnabled(sender, adj, false);
			PXUIFieldAttribute.SetEnabled<CAAdj.adjRefNbr>(sender, adj, true);
			
			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CAAdj.cashAccountID>(sender, adj);
			bool requireControlTotal = (bool)casetup.Current.RequireControlTotal;
			bool clearEnabled = (adj.Released != true) && (cashaccount != null) && (cashaccount.Reconcile == true);
            bool hasNoDetailRecords = !this.CASplitRecords.Any();
            
			if (adjNotReleasedAndNotApproved)
			{
				PXUIFieldAttribute.SetEnabled<CAAdj.hold>(sender, adj, true);
				PXUIFieldAttribute.SetEnabled<CAAdj.cashAccountID>(sender, adj, hasNoDetailRecords);
				PXUIFieldAttribute.SetEnabled<CAAdj.entryTypeID>(sender, adj, hasNoDetailRecords);
                PXUIFieldAttribute.SetEnabled<CAAdj.extRefNbr>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.tranDate>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.finPeriodID>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.tranDesc>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.taxZoneID>(sender, adj);
				PXUIFieldAttribute.SetEnabled<CAAdj.curyControlAmt>(sender, adj, requireControlTotal);
				PXUIFieldAttribute.SetEnabled<CAAdj.cleared>(sender, adj, clearEnabled);
				PXUIFieldAttribute.SetEnabled<CAAdj.clearDate>(sender, adj, clearEnabled && (adj.Cleared == true));
				CAEntryType entryType = PXSelect<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(this, adj.EntryTypeID);
				bool isReclassyPaymentEntry = (entryType != null && entryType.UseToReclassifyPayments == true);
				PXUIFieldAttribute.SetEnabled<CASplit.inventoryID>(this.CASplitRecords.Cache, null, !isReclassyPaymentEntry);
			}
			
			PXUIFieldAttribute.SetVisible<CAAdj.curyControlAmt>(sender, null, requireControlTotal);
			PXUIFieldAttribute.SetVisible<CAAdj.curyID>(sender, adj, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetRequired<CAAdj.curyControlAmt>(sender, requireControlTotal);

            bool isReclassification = adj.PaymentsReclassification == true;
            PXUIFieldAttribute.SetVisible<CASplit.cashAccountID>(this.CASplitRecords.Cache, null, isReclassification);
			PXUIFieldAttribute.SetEnabled<CASplit.cashAccountID>(this.CASplitRecords.Cache, null, isReclassification);
            PXUIFieldAttribute.SetRequired<CAAdj.extRefNbr>(sender, casetup.Current.RequireExtRefNbr == true);

			bool RequireTaxControlTotal = PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>() &&
			                              (casetup.Current.RequireControlTaxTotal == true);

			PXUIFieldAttribute.SetVisible<CAAdj.curyTaxAmt>(sender, adj, RequireTaxControlTotal);
			PXUIFieldAttribute.SetEnabled<CAAdj.curyTaxAmt>(sender, adj, RequireTaxControlTotal);
			PXUIFieldAttribute.SetRequired<CAAdj.curyTaxAmt>(sender, RequireTaxControlTotal);

			PXUIFieldAttribute.SetEnabled<CAAdj.taxCalcMode>(sender, adj, adj.Released != true);

			bool showRoundingDiff = adj.CuryTaxRoundDiff != 0;
			PXUIFieldAttribute.SetVisible<CAAdj.curyTaxRoundDiff>(sender, adj, showRoundingDiff);

			if (UseTaxes.Select().Count != 0)
			{
				sender.RaiseExceptionHandling<CAAdj.curyTaxTotal>(adj, adj.CuryTaxTotal, new PXSetPropertyException(TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}

			PXUIFieldAttribute.SetVisible<CAAdj.usesManualVAT>(sender, adj, adj.UsesManualVAT == true);
			Taxes.Cache.AllowDelete = Taxes.Cache.AllowDelete && adj.UsesManualVAT != true;
			Taxes.Cache.AllowInsert = Taxes.Cache.AllowInsert && adj.UsesManualVAT != true;
			Taxes.Cache.AllowUpdate = Taxes.Cache.AllowUpdate && adj.UsesManualVAT != true;

			PXUIFieldAttribute.SetEnabled<CAAdj.depositAfter>(sender, adj, false);
			PXUIFieldAttribute.SetRequired<CAAdj.depositAfter>(sender, adj.DepositAsBatch == true);
			PXPersistingCheck depositAfterPersistCheck = adj.DepositAsBatch == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<CAAdj.depositAfter>(sender, adj, depositAfterPersistCheck);

			PXUIFieldAttribute.SetEnabled<CAAdj.depositDate>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CAAdj.depositAsBatch>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CAAdj.deposited>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CAAdj.depositNbr>(sender, null, false);

			CashAccount cashAccount = this.cashAccount.Current;
			bool isClearingAccount = (cashAccount != null && cashAccount.CashAccountID == adj.CashAccountID && cashAccount.ClearingAccount == true);
			bool isDeposited = string.IsNullOrEmpty(adj.DepositNbr) == false && string.IsNullOrEmpty(adj.DepositType) == false;
			bool enableDepositEdit = !isDeposited && cashAccount != null && (isClearingAccount || adj.DepositAsBatch != isClearingAccount);

			if (enableDepositEdit)
			{
				var exc = adj.DepositAsBatch != isClearingAccount ? new PXSetPropertyException(AR.Messages.DocsDepositAsBatchSettingDoesNotMatchClearingAccountFlag, PXErrorLevel.Warning)
																  : null;

				sender.RaiseExceptionHandling<CAAdj.depositAsBatch>(adj, adj.DepositAsBatch, exc);
			}

			PXUIFieldAttribute.SetEnabled<CAAdj.depositAsBatch>(sender, adj, enableDepositEdit);
			PXUIFieldAttribute.SetEnabled<CAAdj.depositAfter>(sender, adj, !isDeposited && isClearingAccount && adj.DepositAsBatch == true);

			PXUIFieldAttribute.SetVisible<CAAdj.reverseCount>(sender, adj, adj.ReverseCount > 0);
			PXUIFieldAttribute.SetVisible<CAAdj.origAdjRefNbr>(sender, adj, adj.OrigAdjRefNbr != null);
		}

		protected virtual void CAAdj_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj == null) return;

			if (adj.Released == true && adj.ReverseCount == null)
			{
				using (new PXConnectionScope())
				{
					adj.ReverseCount = GetReversingCAAdj(this, adj.AdjTranType, adj.AdjRefNbr).Count();
				}
			}
		}

		protected virtual void CAAdj_EntryTypeId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
		    if (adj != null)
		    {
		        CAEntryType entryType = PXSelect<CAEntryType,
		            Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.
		            Select(this, adj.EntryTypeID);
		        if (entryType != null)
		        {
                    adj.DrCr = entryType.DrCr;
                    adj.PaymentsReclassification = entryType.UseToReclassifyPayments == true;
                    if (entryType.UseToReclassifyPayments == true && adj.CashAccountID.HasValue)
                    {
                        CashAccount availableAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, NotEqual<Required<CashAccount.cashAccountID>>,
                            And<CashAccount.curyID, Equal<Required<CashAccount.curyID>>>>>.SelectWindowed(sender.Graph, 0, 1, adj.CashAccountID, adj.CuryID);
                        if (availableAccount == null)
                        {
                            sender.RaiseExceptionHandling<CAAdj.entryTypeID>(adj, null, new PXSetPropertyException(Messages.EntryTypeRequiresCashAccountButNoOneIsConfigured, PXErrorLevel.Warning, adj.CuryID));
                        }
                    }		            
		        }
				sender.SetDefaultExt<CAAdj.taxZoneID>(adj);
				sender.SetDefaultExt<CAAdj.taxCalcMode>(adj);
		    }
		}
		protected virtual void CAAdj_Hold_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = casetup.Current.HoldEntry == true;
		}
		protected virtual void CAAdj_Status_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = casetup.Current.HoldEntry == true
										? CATransferStatus.Hold
										: CATransferStatus.Balanced;
		}
		protected virtual void CAAdj_Cleared_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = e.Row as CAAdj;
			if (adj.Cleared == true)
			{
				if (adj.ClearDate == null)
				{
					adj.ClearDate = adj.TranDate;
				}
			}
			else
			{
				adj.ClearDate = null;
			}
		}

		protected virtual void CAAdj_TranDesc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj cAAdj = (CAAdj)e.Row;
			if (cAAdj?.Released != false) return;

			foreach (CATaxTran cATaxTran in Taxes.Select())
			{
				cATaxTran.Description = cAAdj.DocDesc;
				Taxes.Cache.Update(cATaxTran);
			}
		}

		protected virtual void CAAdj_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			adj.Cleared = false;
			adj.ClearDate = null;
			if (adj.CashAccountID == null)
			{
				return;
			}
			if (cashAccount.Current == null || cashAccount.Current.CashAccountID != adj.CashAccountID)
			{
				cashAccount.Current = (CashAccount)PXSelectorAttribute.Select<CAAdj.cashAccountID>(sender, adj);
			}
			SetCleared(adj);
			sender.SetDefaultExt<CAAdj.entryTypeID>(e.Row);
			sender.SetDefaultExt<CAAdj.depositAsBatch>(e.Row);
			sender.SetDefaultExt<CAAdj.depositAfter>(e.Row);
		}

		protected virtual void CAAdj_DepositAsBatch_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<CAAdj.depositAfter>(e.Row);
		}

		protected virtual void CAAdj_DepositAfter_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CAAdj row = (CAAdj)e.Row;
			if (row.DepositAsBatch == true)
			{
				e.NewValue = row.TranDate;
				e.Cancel = true;
			}
		}

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		protected virtual void CAAdj_AdjDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void _(Events.FieldUpdated<CAAdj.tranDate> e)
		{
			CAAdj adj = (CAAdj)e.Row;
			if (adj.Released == false)
			{
				e.Cache.SetDefaultExt<CAAdj.depositAfter>(e.Row);
			}
		}

		protected virtual void CAAdj_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CAAdj adj = (CAAdj)e.Row;

			PXPersistingCheck depositAfterPersistCheck = adj.DepositAsBatch == true ? PXPersistingCheck.NullOrBlank
																									: PXPersistingCheck.Nothing;
			PXDefaultAttribute.SetPersistingCheck<CAAdj.depositAfter>(sender, adj, depositAfterPersistCheck);

			PXDefaultAttribute.SetPersistingCheck<CAAdj.extRefNbr>(sender, adj,
				casetup.Current.RequireExtRefNbr == true ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) && adj.Status == CATransferStatus.Released)
			{
				e.Cancel = true;
				throw new PXException(Messages.ReleasedDocCanNotBeDel);
			}
		}

		protected virtual void CAAdj_Rejected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CAAdj doc = (CAAdj)e.Row;

			if (doc.Rejected == true)
			{
				doc.Approved = false;
				doc.Hold = false;
				doc.Status = CATransferStatus.Rejected;
				cache.RaiseFieldUpdated<CAAdj.hold>(e.Row, null);
			}
		}

		protected virtual void CAAdj_TranID_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || e.IsAltered)
			{
				string ViewName = null;
				PXCache cache = sender.Graph.Caches[typeof(CATran)];
				PXFieldState state = cache.GetStateExt<CATran.batchNbr>(null) as PXFieldState;

				if (state != null)
				{
					ViewName = state.ViewName;
				}

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null,
                    PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}

		[CM.Extensions.PXDBCurrency(typeof(CATran.curyInfoID), typeof(CATran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void CATran_CuryTranAmt_CacheAttached(PXCache sender) { }

		#endregion

		#region CASplit Events

		protected virtual void CASplit_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsReverseContext)
			{
				return;
			}

			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (adj == null || adj.EntryTypeID == null || split == null)
                return;

            e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).AccountID;
            e.Cancel = e.NewValue != null; 
		}

		protected virtual void CASplit_AccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CATranDetailHelper.OnAccountIdFieldUpdatedEvent(cache, e);

			CASplit caSplit = (CASplit)e.Row;

			if (caSplit.InventoryID == null)
				cache.SetDefaultExt<CASplit.taxCategoryID>(e.Row);

			if (caSplit.ProjectID == null || caSplit.ProjectID == PM.ProjectDefaultAttribute.NonProject())
			{
				cache.SetDefaultExt<CASplit.projectID>(e.Row);
			}
		}

		protected virtual void CASplit_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (IsReverseContext)
			{
				return;
			}

			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (adj == null || adj.EntryTypeID == null || split == null)
                return;

		    e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).SubID;
		    e.Cancel = e.NewValue != null; 
		}

        protected virtual void CASplit_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
			if (IsReverseContext)
			{
				return;
			}

			CAAdj adj = CAAdjRecords.Current;
            CASplit split = e.Row as CASplit;

            if (adj == null || adj.EntryTypeID == null || split == null)
				return;

	        e.NewValue = GetDefaultAccountValues(this, adj.CashAccountID, adj.EntryTypeID).BranchID;
			e.Cancel = e.NewValue != null; 

        }

        private CASplit GetDefaultAccountValues(PXGraph graph, int? cashAccountID, string entryTypeID)
        {
            return CATranDetailHelper.CreateCATransactionDetailWithDefaultAccountValues<CASplit>(graph, cashAccountID, entryTypeID);
        }

		protected virtual void CASplit_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldDefaultingEvent(sender, e);

		}

		protected virtual void CASplit_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
            CATranDetailHelper.OnCashAccountIdFieldUpdatedEvent(sender, e);
		}

		protected virtual void CASplit_TranDesc_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

            if (adj?.EntryTypeID == null)
                return;

				CAEntryType entryType = PXSelect<CAEntryType,
                                       Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>
                                    .Select(this, adj.EntryTypeID);

				if (entryType != null)
				{
					e.NewValue = entryType.Descr;
				}
			}

		protected virtual void CASplit_CuryTranAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;

			if (split == null)
                return;

			if (casetup.Current.RequireControlTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
			{
				CAAdj adj = CAAdjRecords.Current;

				if (adj == null)
                    return;

				decimal? newVal = 0;

				if (String.IsNullOrEmpty(split.TaxCategoryID))
				{
					sender.SetDefaultExt<CASplit.taxCategoryID>(split);
				}

				newVal = TaxAttribute.CalcResidualAmt(sender, split, adj.TaxZoneID, split.TaxCategoryID, adj.TranDate.Value,
					adj.TaxCalcMode, adj.CuryControlAmt.Value, adj.CurySplitTotal.Value, adj.CuryTaxTotal.Value);
				e.NewValue = Math.Sign(newVal.Value) == Math.Sign(adj.CuryControlAmt.Value) ? newVal : 0;
				e.Cancel = true;
			}
		}

		protected virtual void CASplit_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;

			if (reversingContext || split == null)
				return;

			if (TaxAttribute.GetTaxCalc<CASplit.taxCategoryID>(sender, split) != TaxCalc.Calc ||
				split.InventoryID != null)
				return;

			Account account = null;
			if (split.AccountID != null)
			{
				account = PXSelect<Account,
								Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, split.AccountID);
			}

			if (account?.TaxCategoryID != null)
			{
				e.NewValue = account.TaxCategoryID;
			}
			else if (taxzone.Current != null &&
				!string.IsNullOrEmpty(taxzone.Current.DfltTaxCategoryID))
			{
				e.NewValue = taxzone.Current.DfltTaxCategoryID;
			}
			else
			{
				e.NewValue = split.TaxCategoryID;
			}
		}

		protected virtual void CASplit_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CASplit row = e.Row as CASplit;

			if (reversingContext || row == null)
                return;

			CAAdj doc = CAAdjRecords.Current;

			if (!this.IsCopyPasteContext && casetup.Current.RequireControlTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>()
				&& row.CuryTranAmt.HasValue && row.CuryTranAmt.Value != 0 && row.Qty == (decimal)1.0 && doc.TaxCalcMode == TaxCalculationMode.Net)
			{
				PXResultset<CATax> taxes = PXSelect<CATax,
                                              Where<CATax.adjTranType, Equal<Required<CATax.adjTranType>>,
                                                And<CATax.adjRefNbr, Equal<Required<CATax.adjRefNbr>>,
					                            And<CATax.lineNbr, Equal<Required<CATax.lineNbr>>>>>>
                                          .Select(this, row.AdjTranType, row.AdjRefNbr, row.LineNbr);
				decimal curyTaxSum = 0;

				foreach (CATax tax in taxes)
				{
					curyTaxSum += tax.CuryTaxAmt.Value;
				}

				decimal? taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(sender, row, doc.TaxZoneID,
                                                                                 row.TaxCategoryID, doc.TranDate.Value,
                                                                                 aCuryTotal: row.CuryTranAmt.Value + curyTaxSum,
                                                                                 aSalesOrPurchaseSwitch: false,
                                                                                 enforceType: GLTaxAttribute.TaxCalcLevelEnforcing.EnforceCalcOnItemAmount);
				sender.SetValueExt<CASplit.curyTranAmt>(row, taxableAmount);
			}
		}

		protected virtual void CASplit_InventoryId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			CAAdj adj = CAAdjRecords.Current;

			if (split != null && split.InventoryID != null)
			{
				InventoryItem invItem = PXSelect<InventoryItem,
					Where<InventoryItem.inventoryID, Equal<Required<CASplit.inventoryID>>>>.
					Select(this, split.InventoryID);

				bool isEnableForImport = split.AccountID == null;
				bool isUIContext = !(IsImportFromExcel == true || IsContractBasedAPI == true);

				if (invItem != null && adj != null && (isEnableForImport || isUIContext) && !IsReverseContext)
				{
					if (adj.DrCr == CADrCr.CADebit)
					{
						split.AccountID = invItem.SalesAcctID;
						split.SubID = invItem.SalesSubID;
					}
					else
					{
						split.AccountID = invItem.COGSAcctID;
						split.SubID = invItem.COGSSubID;
					}
				}
			}

			sender.SetDefaultExt<CASplit.taxCategoryID>(split);
			sender.SetDefaultExt<CASplit.uOM>(split);
		}

		protected virtual void CASplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CASplit row = (CASplit)e.Row;

			if (row == null)
				return;

		    bool isReclassification = this.CAAdjRecords.Current.PaymentsReclassification == true;

            PXUIFieldAttribute.SetEnabled<CASplit.accountID>(sender, row, !isReclassification);
            PXUIFieldAttribute.SetEnabled<CASplit.subID>(sender, row, !isReclassification);
            PXUIFieldAttribute.SetEnabled<CASplit.branchID>(sender, row, !isReclassification);
		}
        
		protected virtual void CASplit_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CATranDetailHelper.OnCATranDetailRowUpdatingEvent(sender, e);
			if (CATranDetailHelper.VerifyOffsetCashAccount(sender, e.NewRow as CASplit, CAAdjRecords.Current?.CashAccountID))
			{
				e.Cancel = true;
			}
			sender.SetValueExt<CASplit.curyTranAmt>(e.NewRow, (e.NewRow as CASplit).CuryTranAmt);
		}

		protected virtual void CASplit_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CATranDetailHelper.VerifyOffsetCashAccount(sender, e.Row as CASplit, CAAdjRecords.Current?.CashAccountID);
			sender.SetValueExt<CASplit.curyTranAmt>(e.Row, (e.Row as CASplit).CuryTranAmt);
		}

		protected virtual void CASplit_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (e.Row == null) return;
			TaxAttribute.Calculate<CASplit.taxCategoryID>(sender, e);
		}

		protected virtual void CASplit_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null) return;

			var row = (CASplit)e.Row;
			if (IsImport && row != null && row.AccountID == null)
			{
				sender.SetDefaultExt<CASplit.accountID>(row);
			}
			if (IsImport && row != null && row.SubID == null)
			{
				sender.SetDefaultExt<CASplit.subID>(row);
			}
		}

		protected virtual void CASplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CASplit tran = e.Row as CASplit;
			object projectID = tran.ProjectID;
			try
			{
				sender.RaiseFieldVerifying<CASplit.projectID>(tran, ref projectID);
			}
			catch (PXSetPropertyException exc)
			{
				sender.RaiseExceptionHandling<CASplit.projectID>(tran, projectID, exc);
			}
		}

		protected virtual void CASplit_Qty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CASplit split = e.Row as CASplit;
			e.NewValue = (decimal)1.0;
		}
	
		#endregion

		#region CATaxTran Events
		protected virtual void CATaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			bool usesManualVAT = this.CurrentDocument.Current != null && this.CurrentDocument.Current.UsesManualVAT == true;
			PXUIFieldAttribute.SetEnabled<CATaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted && !usesManualVAT);
		}
		protected virtual void CATaxTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (reversingContext) e.Cancel = true;
			PXParentAttribute.SetParent(sender, e.Row, typeof(CAAdj), this.CAAdjRecords.Current);
		}

		protected virtual void CATaxTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Taxes.View.RequestRefresh();
		}

		protected virtual void CATaxTran_TaxType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.TranTaxType;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.TranTaxType;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CATaxTran_AccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxAcctID;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxAcctID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CATaxTran_SubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null)
			{
				if (CAAdjRecords.Current.DrCr == CADrCr.CACredit)
				{
					AP.PurchaseTax tax = PXSelect<AP.PurchaseTax, Where<AP.PurchaseTax.taxID, Equal<Required<AP.PurchaseTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxSubID;
						e.Cancel = true;
					}
				}
				else
				{
					AR.SalesTax tax = PXSelect<AR.SalesTax, Where<AR.SalesTax.taxID, Equal<Required<AR.SalesTax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
					if (tax != null)
					{
						e.NewValue = tax.HistTaxSubID;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void CATaxTran_TaxBucketID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null && CAAdjRecords.Current != null && CAAdjRecords.Current.DrCr == CADrCr.CACredit)
			{
				Tax tax = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(sender.Graph, ((CATaxTran)e.Row).TaxID);
				if (tax?.IsExternal == true)
				{
					e.NewValue = 0;
					e.Cancel = true;
				}
			}
		}
		#endregion

		#region External Tax Provider

		public virtual bool IsExternalTax(string taxZoneID)
				{
					return false;
				}

		public virtual CAAdj CalculateExternalTax(CAAdj invoice)
			{
			return invoice;
		}
		#endregion

	}
}
