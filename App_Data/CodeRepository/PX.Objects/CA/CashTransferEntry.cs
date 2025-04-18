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
using PX.Data.WorkflowAPI;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Bql;
using PX.Objects.TX;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.CA
{
	public class CashTransferEntry : PXGraph<CashTransferEntry, CATransfer>
	{
		#region Selects
		[PXCopyPasteHiddenFields(typeof(CATransfer.clearDateIn), typeof(CATransfer.clearDateOut), typeof(CATransfer.clearedIn), typeof(CATransfer.clearedOut))]
		public PXSelect<CATransfer> Transfer;
		[PXCopyPasteHiddenFields(typeof(CAExpense.refNbr), typeof(CAExpense.extRefNbr), typeof(CAExpense.adjRefNbr), typeof(CAExpense.curyInfoID))]
		public CAChargeSelect<CATransfer, CAExpense.tranDate, CAExpense.finPeriodID, CAExpense, CAExpense.entryTypeID, CAExpense.refNbr,
			Where<CAExpense.refNbr, Equal<Current<CATransfer.transferNbr>>>> Expenses;
		[PXCopyPasteHiddenView]
		public PXSelect<CAExpenseTax, Where<CAExpenseTax.tranType, Equal<CATranType.cATransferExp>, And<CAExpenseTax.refNbr, Equal<Current<CAExpense.refNbr>>, And<CAExpenseTax.lineNbr, Equal<Current<CAExpense.lineNbr>>>>>, 
			OrderBy<Asc<CAExpenseTax.tranType, Asc<CAExpenseTax.refNbr, Asc<CATax.taxID>>>>> ExpenseTaxes;
		[PXCopyPasteHiddenView]
		public PXSelectJoin<CAExpenseTaxTran,
			InnerJoin<Tax, On<Tax.taxID, Equal<CAExpenseTaxTran.taxID>>>,
			Where<CAExpenseTaxTran.refNbr, Equal<Current<CAExpense.refNbr>>,
				And<CAExpenseTaxTran.lineNbr, Equal<Current<CAExpense.lineNbr>>>>> ExpenseTaxTrans;
		[PXCopyPasteHiddenFields(typeof(CATransfer.clearDateIn), typeof(CATransfer.clearDateOut), typeof(CATransfer.clearedIn), typeof(CATransfer.clearedOut))]
		public PXSelect<CATran> caTran;
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CAAdj> caAdj;
		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		public PXSelect<CASplit> caSplit;
		public PXSetup<CASetup> CASetup;
		public PXSelect<CashAccount,
			Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>
			CashAccount;

		public PXSelectReadonly<OrganizationFinPeriod,
					Where<OrganizationFinPeriod.finPeriodID, Equal<Current<CATransfer.inPeriodID>>,
						And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<CATransfer.inBranchID>>>>>
					inFinPeriod;

		public PXSelectReadonly<OrganizationFinPeriod,
					Where<OrganizationFinPeriod.finPeriodID, Equal<Current<CATransfer.outPeriodID>>,
						And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<CATransfer.outBranchID>>>>>
					outFinPeriod;

		public PXSelect<Sub, Where<Sub.subID, Equal<Required<CashAccount.subID>>>> subaccount;
		public PXSelect<Account, Where<Account.accountID, Equal<Required<CashAccount.accountID>>>> account;
		#endregion
		#region Utils
		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		#endregion
		#region Functions
		public CashTransferEntry()
		{
			OpenPeriodAttribute.SetValidatePeriod<CATransfer.inPeriodID>(Transfer.Cache, null, PeriodValidation.DefaultSelectUpdate);
			OpenPeriodAttribute.SetValidatePeriod<CATransfer.outPeriodID>(Transfer.Cache, null, PeriodValidation.DefaultSelectUpdate);

			this.FieldSelecting.AddHandler<CATransfer.tranIDOut_CATran_batchNbr>(CATransfer_TranIDOut_CATran_BatchNbr_FieldSelecting);
			this.FieldSelecting.AddHandler<CATransfer.tranIDIn_CATran_batchNbr>(CATransfer_TranIDIn_CATran_BatchNbr_FieldSelecting);

			PXUIFieldAttribute.SetRequired<CATransfer.inExtRefNbr>(Transfer.Cache, CASetup.Current.RequireExtRefNbr == true);
			PXUIFieldAttribute.SetRequired<CATransfer.outExtRefNbr>(Transfer.Cache, CASetup.Current.RequireExtRefNbr == true);

			CAExpenseHelper.InitBackwardEditorHandlers(this);
				}
		#endregion

		#region CATransfer Events

		protected virtual void CATransfer_TranIDIn_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			=> TranID_CATran_BatchNbr_FieldSelectingHendler(sender, e);

		protected virtual void CATransfer_TranIDOut_CATran_BatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			=> TranID_CATran_BatchNbr_FieldSelectingHendler(sender, e);

		private static void TranID_CATran_BatchNbr_FieldSelectingHendler(PXCache sender, PXFieldSelectingEventArgs e)
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

				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, false, false, 0, 0, null, null, null, null, null, null, PXErrorLevel.Undefined, false, true, true, PXUIVisibility.Visible, ViewName, null, null);
			}
		}

		protected virtual void CATransfer_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;
			if (transfer == null) return;

			transfer.RGOLAmt = transfer.TranIn - transfer.TranOut;

			bool transferOnHold = (transfer.Hold == true);
			bool transferNotReleased = (transfer.Released != true);
			bool transferReleased = (transfer.Released == true);
			PXUIFieldAttribute.SetVisible<CATransfer.inGLBalance>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.outGLBalance>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.cashBalanceIn>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.cashBalanceOut>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.tranIDIn_CATran_batchNbr>(sender, transfer, transferReleased);
			PXUIFieldAttribute.SetVisible<CATransfer.tranIDOut_CATran_batchNbr>(sender, transfer, transferReleased);

			PXUIFieldAttribute.SetEnabled(sender, transfer, false);

			sender.AllowDelete = transferNotReleased;
			sender.AllowUpdate = transferNotReleased;
			Expenses.Cache.SetAllEditPermissions(transferNotReleased);

			CashAccount cashaccountOut = (CashAccount)PXSelectorAttribute.Select<CATransfer.outAccountID>(sender, e.Row);
			CashAccount cashaccountIn = (CashAccount)PXSelectorAttribute.Select<CATransfer.inAccountID>(sender, e.Row);

			bool clearEnabledIn = transferNotReleased && (cashaccountIn != null) && (cashaccountIn.Reconcile == true);
			bool clearEnabledOut = transferNotReleased && (cashaccountOut != null) && (cashaccountOut.Reconcile == true);

			PXUIFieldAttribute.SetEnabled<CATransfer.hold>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.transferNbr>(sender, transfer, true);
			PXUIFieldAttribute.SetEnabled<CATransfer.descr>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranIn>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.inAccountID>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.inDate>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.inExtRefNbr>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.curyTranOut>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outAccountID>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outDate>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.outExtRefNbr>(sender, transfer, transferNotReleased);
			PXUIFieldAttribute.SetEnabled<CATransfer.clearedOut>(sender, transfer, clearEnabledOut);
			PXUIFieldAttribute.SetEnabled<CATransfer.clearDateOut>(sender, transfer, clearEnabledOut && transfer.ClearedOut == true);
			PXUIFieldAttribute.SetEnabled<CATransfer.clearedIn>(sender, transfer, clearEnabledIn);
			PXUIFieldAttribute.SetEnabled<CATransfer.clearDateIn>(sender, transfer, clearEnabledIn && transfer.ClearedIn == true);

			ExpenseTaxTrans.AllowInsert = transferNotReleased;
			ExpenseTaxTrans.AllowUpdate = transferNotReleased;
			ExpenseTaxTrans.AllowDelete = transferNotReleased;

			UIState.RaiseOrHideErrorByErrorLevelPriority<CATransfer.inDate>(sender, transfer, transfer.Released != true && transfer.OutDate > transfer.InDate, Messages.EarlyInDate, PXErrorLevel.Warning);

			SetAdjRefNbrVisibility();

			PXUIFieldAttribute.SetVisible<CATransfer.reverseCount>(sender, transfer, transfer.ReverseCount > 0);
			PXUIFieldAttribute.SetVisible<CATransfer.origTransferNbr>(sender, transfer, transfer.OrigTransferNbr!= null);
		}

		protected virtual void CATransfer_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			CATransfer doc = e.Row as CATransfer;
			if (doc == null) return;

			// The UnattendedMode flag is used as a proxy for the graph not being in the UI scope. 
			// -
			// The key assumption under this hack is that reversing transactions count
			// is only displayed on the UI. Once this becomes false, this optimization
			// should be removed.
			// -
			if (doc.Released == true && doc.ReverseCount == null && !UnattendedMode)
			{
				using (new PXConnectionScope())
				{
					doc.ReverseCount = GetReversingTransfer(this, doc.TransferNbr).Count();
				}
			}
		}

		protected void CAExpense_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var expense = (CAExpense)e.Row;

			if (expense == null)
			{
				return;
			}

			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CAExpense.cashAccountID>>>>.Select(this, expense.CashAccountID);

			bool clearEnabled = expense.Released != true && cashAccount?.Reconcile == true;
			PXUIFieldAttribute.SetEnabled<CAExpense.cleared>(sender, expense, clearEnabled);

			PXFieldState state = sender.GetStateExt<CAExpense.tranDate>(expense) as PXFieldState;

			if (state != null && state.Value != null && state.ErrorLevel != PXErrorLevel.Error)
			{
				if (Transfer.Current.Released != true && Transfer.Current.OutDate > expense.TranDate)
				{
					sender.RaiseExceptionHandling<CAExpense.tranDate>(expense, expense.TranDate,
						new PXSetPropertyException(Messages.EarlyExpenseDate, PXErrorLevel.Warning));
				}
				else
				{
					sender.RaiseExceptionHandling<CAExpense.tranDate>(expense, expense.TranDate, null);
				}
			}
		}

		protected void CAExpense_RowInserted(PXCache sender,  PXRowInsertedEventArgs e)
		{
			var expense = (CAExpense)e.Row;

			if (expense.CashTranID == null)
			{
				expense.BatchNbr = null;
			}
		}

		protected void CAExpense_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var expense = (CAExpense)e.Row;

			PXDefaultAttribute.SetPersistingCheck<CAExpense.taxCategoryID>(sender, expense, string.IsNullOrEmpty(expense.TaxZoneID) ? PXPersistingCheck.Nothing : PXPersistingCheck.NullOrBlank);
		}

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), "DisplayName", "Tax Amount")]
		protected virtual void CAExpense_CuryTaxTotal_Cacheattached(PXCache sender) { }

		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void CAExpense_ExpenseNbr_Cacheattached(PXCache sender) { }

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private void SetAdjRefNbrVisibility()
		{
			CAAdj adj = PXSelect<CAAdj, Where<CAAdj.transferNbr, Equal<Current<CATransfer.transferNbr>>>>.Select(this);
			PXUIFieldAttribute.SetVisible<CAExpense.adjRefNbr>(Expenses.Cache, null, adj != null);
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(CashAccountAttribute), nameof(CashAccountAttribute.DisplayName), "Account")]
		protected virtual void CATransfer_OutAccountID_CacheAttached(PXCache sender) { }

		protected virtual void CATransfer_OutAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = e.Row as CATransfer;

			if(transfer.OutAccountID == null)
			{
				return;
			}

			CashAccount cashaccountOut = (CashAccount)PXSelectorAttribute.Select<CATransfer.outAccountID>(sender, e.Row);

			if (cashaccountOut?.Reconcile != true)
			{
				transfer.ClearedOut = true;
				transfer.ClearDateOut = transfer.OutDate;
			}
			else
			{
				transfer.ClearedOut = false;
				transfer.ClearDateOut = null;
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(CashAccountAttribute), nameof(CashAccountAttribute.DisplayName), "Account")]
		protected virtual void CATransfer_InAccountID_CacheAttached(PXCache sender) { }

		protected virtual void CATransfer_InAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = e.Row as CATransfer;

			if (transfer.InAccountID == null)
			{
				return;
			}

			CashAccount cashaccountIn = (CashAccount)PXSelectorAttribute.Select<CATransfer.inAccountID>(sender, transfer);

			if (cashaccountIn?.Reconcile != true)
			{
				transfer.ClearedIn = true;
				transfer.ClearDateIn = transfer.InDate;
			}
			else
			{
				transfer.ClearedIn = false;
				transfer.ClearDateIn = null;
			}
		}

		protected virtual void CAExpense_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CAExpense expense = e.Row as CAExpense;
			CashAccount cashaccount = (CashAccount)PXSelectorAttribute.Select<CAExpense.cashAccountID>(sender, e.Row);

			if (cashaccount?.Reconcile != true)
			{
				expense.Cleared = true;
				expense.ClearDate = expense.TranDate;
			}
			else
			{
				expense.Cleared = false;
				expense.ClearDate = null;
			}
		}

		protected virtual void CATransfer_InDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = e.Row as CATransfer;
			if (transfer.ClearedIn == true)
			{
				CashAccount cashaccountIn = (CashAccount)PXSelectorAttribute.Select<CATransfer.inAccountID>(sender, e.Row);
				if ((cashaccountIn != null) && (cashaccountIn.Reconcile != true))
				{
					transfer.ClearDateIn = transfer.InDate;
				}
			}
		}

		protected virtual void CATransfer_OutDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = e.Row as CATransfer;
			if (transfer.ClearedOut == true)
			{
				CashAccount cashaccountOut = (CashAccount)PXSelectorAttribute.Select<CATransfer.outAccountID>(sender, e.Row);
				if ((cashaccountOut != null) && (cashaccountOut.Reconcile != true))
				{
					transfer.ClearDateOut = transfer.OutDate;
				}
			}
			sender.SetValueExt<CATransfer.inDate>(transfer, transfer.OutDate);
		}

		protected virtual void CATransfer_OutExtRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;

			if (string.IsNullOrEmpty(transfer.InExtRefNbr))
			{
				transfer.InExtRefNbr = transfer.OutExtRefNbr;
			}
		}

		protected virtual void CATransfer_Descr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer cATransfer = (CATransfer)e.Row;
			if (cATransfer?.Released != false) return;

			foreach (CAExpenseTaxTran cAExpenseTaxTran in ExpenseTaxTrans.Select())
			{
				cAExpenseTaxTran.Description = cATransfer.Descr;
				ExpenseTaxTrans.Cache.Update(cAExpenseTaxTran);
			}
		}

		protected virtual void CATransfer_Hold_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

			CATransfer transfer = (CATransfer)e.Row;

			if ((bool)e.NewValue != true)
			{
				if (transfer.TranOut == 0)
				{
					cache.RaiseExceptionHandling<CATransfer.curyTranOut>(transfer, transfer.CuryTranOut, new PXSetPropertyException(Messages.CannotProceedFundTransfer, cache.GetValueExt<CATransfer.curyTranOut>(transfer)));
				}

				if (transfer.TranIn == 0m)
				{
					cache.RaiseExceptionHandling<CATransfer.curyTranIn>(transfer, transfer.CuryTranIn, new PXSetPropertyException(Messages.CannotProceedFundTransfer, cache.GetValueExt<CATransfer.curyTranIn>(transfer)));
				}
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Amount")]
		protected virtual void CATRansfer_CuryTranOut_CacheAttached(PXCache sender) { }

		protected virtual void CATransfer_CuryTranOut_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyTransferNonNegative(e.Row as CATransfer, e.NewValue as decimal?);
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Amount")]
		protected virtual void CATRansfer_CuryTranIn_CacheAttached(PXCache sender) { }

		protected virtual void CATransfer_CuryTranIn_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			VerifyTransferNonNegative(e.Row as CATransfer, e.NewValue as decimal?);
		}

		protected virtual void VerifyTransferNonNegative(CATransfer transfer, decimal? amount)
		{
			if (transfer != null && transfer.Released != true && amount < 0)
				throw new PXSetPropertyException(Messages.CantTransferNegativeAmount);
		}

		protected virtual void CATransfer_ClearedIn_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;
			if (transfer.ClearedIn == true)
			{
				if (transfer.ClearDateIn == null)
					transfer.ClearDateIn = transfer.InDate;
			}
			else
			{
				transfer.ClearDateIn = null;
			}
		}
		protected virtual void CATransfer_ClearedOut_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;
			if (transfer.ClearedOut == true)
			{
				if (transfer.ClearDateOut == null)
					transfer.ClearDateOut = transfer.OutDate;
			}
			else
			{
				transfer.ClearDateOut = null;
			}
		}

		protected virtual void CATransfer_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CATransfer transfer = (CATransfer)e.Row;
			bool requireExtRefNbr = CASetup.Current.RequireExtRefNbr == true;

			if (transfer.OutAccountID == null)
			{
				sender.RaiseExceptionHandling<CATransfer.outAccountID>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CATransfer.outAccountID)}]"));
			}

			PXDefaultAttribute.SetPersistingCheck<CATransfer.outExtRefNbr>(sender, transfer, requireExtRefNbr ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (transfer.InAccountID == null)
			{
				sender.RaiseExceptionHandling<CATransfer.inAccountID>(transfer, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(CATransfer.inAccountID)}]"));
			}

			PXDefaultAttribute.SetPersistingCheck<CATransfer.inExtRefNbr>(sender, transfer, requireExtRefNbr ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (transfer.OutAccountID == transfer.InAccountID)
			{
				sender.RaiseExceptionHandling<CATransfer.inAccountID>(transfer, null, new PXSetPropertyException(Messages.TransferInCAAreEquals));
			}
			if (transfer.Hold == false && (transfer.TranOut == null || transfer.TranOut == 0))
			{
				sender.RaiseExceptionHandling<CATransfer.curyTranOut>(transfer, transfer.CuryTranOut, new PXSetPropertyException(Messages.CannotProceedFundTransfer, sender.GetValueExt<CATransfer.curyTranOut>(transfer)));
			}
			if (transfer.Hold == false && (transfer.TranIn == null || transfer.TranIn == 0))
			{
				sender.RaiseExceptionHandling<CATransfer.curyTranIn>(transfer, transfer.CuryTranIn, new PXSetPropertyException(Messages.CannotProceedFundTransfer, sender.GetValueExt<CATransfer.curyTranIn>(transfer)));
			}
			}
		#endregion
		#region CATran Envents
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleCA>>>))]
		public virtual void CATran_BatchNbr_CacheAttached(PXCache sender)
		{
		}

		[CM.Extensions.PXDBCurrency(typeof(CATran.curyInfoID), typeof(CATran.tranAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		protected virtual void CATran_CuryTranAmt_CacheAttached(PXCache sender) { }
		#endregion
		#region CAExpenseTaxTran Envents
		protected virtual void _(Events.FieldUpdated<CAExpenseTaxTran, CAExpenseTaxTran.taxID> e)
		{
			if (!(e.Row is CAExpenseTaxTran caExpenseTaxTran))
				return;

			if (e.OldValue != null && e.OldValue != e.NewValue)
			{
				ExpenseTaxTrans.Cache.SetDefaultExt<CAExpenseTaxTran.accountID>(caExpenseTaxTran);
				ExpenseTaxTrans.Cache.SetDefaultExt<CAExpenseTaxTran.taxType>(caExpenseTaxTran);
				ExpenseTaxTrans.Cache.SetDefaultExt<CAExpenseTaxTran.taxBucketID>(caExpenseTaxTran);
			}
		}
		#endregion
		#region Worflow Buttons
		public PXInitializeState<CATransfer> initializeState;
		
		public PXAction<CATransfer> putOnHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable PutOnHold(PXAdapter adapter) => adapter.Get();

		public PXAction<CATransfer> releaseFromHold;
		[PXButton(CommitChanges = true), PXUIField(DisplayName = "Remove Hold", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable ReleaseFromHold(PXAdapter adapter) => adapter.Get();
		public PXWorkflowEventHandler<CATransfer> OnReleaseDocument;
		public PXWorkflowEventHandler<CATransfer> OnUpdateStatus;
		#endregion
		#region Buttons
		public PXAction<CATransfer> ViewDoc;
		[PXUIField(
			DisplayName = Messages.ViewExpense,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable viewDoc(PXAdapter adapter)
		{
			if (Expenses.Current.AdjRefNbr != null)
			{
				CATranEntry graph = PXGraph.CreateInstance<CATranEntry>();
				graph.Clear();
				Transfer.Cache.IsDirty = false;
				CAAdj adj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAExpense.adjRefNbr>>>>.Select(this, Expenses.Current.AdjRefNbr);
				graph.CAAdjRecords.Current = adj;
				throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public PXAction<CATransfer> Release;
		[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(CATransfer)];
			CATransfer transfer = Transfer.Current;

			Save.Press();

			CheckTransfer();
			CheckExpensesOnHold();

			List<CARegister> list = new List<CARegister>();
			CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIDIn>>>>.Select(this, transfer.TranIDIn);
			if (tran != null)
				list.Add(CATrxRelease.CARegister(transfer, tran));
			else
				throw new PXException(Messages.TransactionNotFound);

			tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIDOut>>>>.Select(this, transfer.TranIDOut);
			if (tran == null)
				throw new PXException(Messages.TransactionNotFound);

			PXLongOperation.StartOperation(this, delegate () { CATrxRelease.GroupRelease(list, false); });

			List<CATransfer> ret = new List<CATransfer>();
			ret.Add(transfer);
			return ret;
		}

		private void CheckTransfer()
		{
			CATransfer transfer = Transfer.Current;

			try
			{
				Transfer.Cache.VerifyFieldAndRaiseException<CATransfer.outAccountID>(transfer, throwError: true);
				Transfer.Cache.VerifyFieldAndRaiseException<CATransfer.inAccountID>(transfer, throwError: true);
			}
			catch
			{
				throw new PXException(Messages.TransferCannotBeReleased, transfer.TransferNbr);
			}
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private void CheckExpensesOnHold()
		{
			bool holdExpenses = false;

			var adjSet = new List<string>();
			foreach (CAExpense exp in Expenses.Select())
			{
				if (!string.IsNullOrEmpty(exp.AdjRefNbr))
				{
					adjSet.Add(exp.AdjRefNbr);
				}
			}

			if (adjSet.Count < 1)
			{
				return;
			}

			Cancel.Press();

			var adjs = PXSelectReadonly<CAAdj, Where<CAAdj.adjRefNbr, In<Required<CAExpense.adjRefNbr>>>>.Select(this, new object[] { adjSet.ToArray() });

			foreach (CAAdj adj in adjs)
			{
				if (adj.Hold == true)
				{
					var expense = (CAExpense)Expenses.Select().ToArray().First(m => ((CAExpense)m)?.AdjRefNbr == adj.AdjRefNbr);
					Expenses.Cache.RaiseExceptionHandling<CAExpense.adjRefNbr>(expense, expense.AdjRefNbr, new PXSetPropertyException(Messages.HoldExpense, PXErrorLevel.RowError));
					holdExpenses = true;
				}
			}

			if (holdExpenses)
			{
				PXGraph.ThrowWithoutRollback(
                    new PXException(Messages.HoldExpenses, Transfer.Current.TransferNbr));
			}
		}

		public PXAction<CATransfer> Reverse;
		[PXUIField(DisplayName = "Reverse", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable reverse(PXAdapter adapter)
		{
			List<CATransfer> result = new List<CATransfer>();
				result.Add(ReverseTransfer());
			return result;
		}

		private CATransfer ReverseTransfer()
		{
			CATransfer currentTransfer = Transfer.Current;

			if (!AskUserApprovalToReverse(currentTransfer))
			{
				return currentTransfer;
			}

			IEnumerable<CAExpense> currentExpenses = PXSelect<CAExpense, Where<CAExpense.refNbr, Equal<Required<CAExpense.refNbr>>>>
																	.Select(this, currentTransfer.RefNbr)
																	.RowCast<CAExpense>();
			List<CAExpenseTaxTran> taxes = CreateListOfNewTaxTrans(currentTransfer.RefNbr);

			var reverseTransfer = (CATransfer)Transfer.Cache.CreateCopy(currentTransfer);
			inFinPeriod.Cache.Current = inFinPeriod.View.SelectSingleBound(new object[] { reverseTransfer });
			FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<CATransfer.inPeriodID, CATransfer.inBranchID>(Transfer.Cache, reverseTransfer, inFinPeriod);
			outFinPeriod.Cache.Current = outFinPeriod.View.SelectSingleBound(new object[] { reverseTransfer });
			FinPeriodUtils.VerifyAndSetFirstOpenedFinPeriod<CATransfer.outPeriodID, CATransfer.outBranchID>(Transfer.Cache, reverseTransfer, outFinPeriod);

			Transfer.Cache.Clear();
			Expenses.Cache.Clear();

			SwapInOutFields(currentTransfer, reverseTransfer);
			SetOtherFields(reverseTransfer);

			reverseTransfer = Transfer.Insert(reverseTransfer);

			Expenses.Cache.SetDefaultExt<CATransfer.hold>(reverseTransfer);
			reverseTransfer.InDate = currentTransfer.OutDate;

			reverseTransfer = Transfer.Update(reverseTransfer);

			#region Reverse Expenses
			foreach (CAExpense expense in currentExpenses)
			{
				ReverseExpence(reverseTransfer, expense, taxes);
			}
			#endregion

			RemoveLinkOnAdj();

			FinPeriodUtils.CopyPeriods<CATransfer, CATransfer.inPeriodID, CATransfer.inTranPeriodID, CATransfer.outPeriodID, CATransfer.outTranPeriodID>(Transfer.Cache, currentTransfer, reverseTransfer);
			FinPeriodUtils.CopyPeriods<CATransfer, CATransfer.outPeriodID, CATransfer.outTranPeriodID, CATransfer.inPeriodID, CATransfer.inTranPeriodID>(Transfer.Cache, currentTransfer, reverseTransfer);

			return reverseTransfer;
		}

		protected virtual List<CAExpenseTaxTran> CreateListOfNewTaxTrans(string refNbr)
		{
			List<CAExpenseTaxTran> taxes = new List<CAExpenseTaxTran>();

			var origTaxTrans = PXSelectJoin<CAExpenseTaxTran, 
				InnerJoin<Tax, 
					On<Tax.taxID, Equal<CAExpenseTaxTran.taxID>>>,
				Where<CAExpenseTaxTran.refNbr, Equal<Required<CAExpense.refNbr>>>>.Select(this, refNbr);

			foreach (CAExpenseTaxTran taxTran in origTaxTrans)
			{
				CAExpenseTaxTran newTaxTran = new CAExpenseTaxTran();
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
				newTaxTran.LineNbr = taxTran.LineNbr;

				taxes.Add(newTaxTran);
			}

			return taxes;
		}

		protected virtual void ReverseExpence(CATransfer transfer, CAExpense expense, List<CAExpenseTaxTran> origTaxTrans)
		{
			ReverseCAExpenseEntry(transfer, expense);

			foreach (CAExpenseTaxTran taxTran in origTaxTrans.Where(m => m.LineNbr == expense.LineNbr))
			{
				ExpenseTaxTrans.Insert(taxTran);
			}
		}

		protected virtual void ReverseCAExpenseEntry(CATransfer transfer, CAExpense expense)
		{
			CAExpense reversedExpense;

			using (new CancelCAExpenseTaxCalcModeFieldDefaulting(this, expense))
			{
				reversedExpense = Expenses.ReverseCharge(expense, true);
				reversedExpense.CuryTaxableAmt = -expense.CuryTaxableAmt;
				reversedExpense.CuryTaxTotal = -expense.CuryTaxTotal;
				reversedExpense.FinPeriodID = transfer.OutPeriodID;
				reversedExpense.TranPeriodID = transfer.OutTranPeriodID;
				reversedExpense.TaxCalcMode = expense.TaxCalcMode;
				reversedExpense.TaxZoneID = expense.TaxZoneID;
				reversedExpense.TaxCategoryID = expense.TaxCategoryID;
				reversedExpense = Expenses.Insert(reversedExpense);
			}

			FinPeriodUtils.CopyPeriods<CAExpense, CAExpense.finPeriodID, CAExpense.tranPeriodID>(Expenses.Cache, expense, reversedExpense);
		}

		public bool AskUserApprovalToReverse(CATransfer origDoc)
		{
			string localizedMsg;

			if (GetReversingTransfer(this, origDoc.TransferNbr).Count() >= 1)
			{
				localizedMsg = PXMessages.LocalizeNoPrefix(Messages.ReversingTransactionExists);
				return Transfer.View.Ask(localizedMsg, MessageButtons.YesNo) == WebDialogResult.Yes;
			}

			return true;
		}

		public static IEnumerable<CATransfer> GetReversingTransfer(PXGraph graph, string refNbr)
		{
			var reversingTransfer = PXSelectReadonly<CATransfer,
									Where<CATransfer.origTransferNbr, Equal<Required< CATransfer.origTransferNbr>>>>
									.Select(graph, refNbr)
									.RowCast<CATransfer>();

			return reversingTransfer;
		}

		private void RemoveLinkOnAdj()
		{
			foreach (CAExpense item in Expenses.Select())
			{
				if (item.AdjRefNbr != null)
				{
					item.AdjRefNbr = null;
				}
			}
		}

		protected virtual void SwapInOutFields(CATransfer currentTransfer, CATransfer reverseTransfer)
		{
			reverseTransfer.OutAccountID = currentTransfer.InAccountID;
			reverseTransfer.OutCuryID = currentTransfer.InCuryID;
			reverseTransfer.CuryTranOut = currentTransfer.CuryTranIn;
			reverseTransfer.TranOut = currentTransfer.TranIn;
			reverseTransfer.OutDate = currentTransfer.InDate;
			reverseTransfer.OutPeriodID = currentTransfer.InPeriodID;
			reverseTransfer.OutTranPeriodID = currentTransfer.InTranPeriodID;
			reverseTransfer.TranIDOut = null;
			reverseTransfer.OutExtRefNbr = currentTransfer.InExtRefNbr;
			reverseTransfer.ClearedOut = currentTransfer.ClearedIn;
			reverseTransfer.ClearDateOut = currentTransfer.ClearDateIn;

			reverseTransfer.InAccountID = currentTransfer.OutAccountID;
			reverseTransfer.InCuryID = currentTransfer.OutCuryID;
			reverseTransfer.CuryTranIn = currentTransfer.CuryTranOut;
			reverseTransfer.TranIn = currentTransfer.TranOut;
			reverseTransfer.InPeriodID = currentTransfer.OutPeriodID;
			reverseTransfer.InTranPeriodID = currentTransfer.OutTranPeriodID;
			reverseTransfer.TranIDIn = null;
			reverseTransfer.InExtRefNbr = currentTransfer.OutExtRefNbr;
			reverseTransfer.ClearedIn = currentTransfer.ClearedOut;
			reverseTransfer.ClearDateIn = currentTransfer.ClearDateOut;

			reverseTransfer.OrigTransferNbr = currentTransfer.TransferNbr;
		}

		private void SetOtherFields(CATransfer reverseTransfer)
		{
			reverseTransfer.Descr = string.Format(Messages.ReversingTransferOfTransferNbr, reverseTransfer.TransferNbr);
			reverseTransfer.TransferNbr = null;
			reverseTransfer.Released = false;
			reverseTransfer.NoteID = null;
			reverseTransfer.ReverseCount = null;
			reverseTransfer.Hold = null;
			reverseTransfer.Status = null;
		}

		public PXAction<CATransfer> viewExpenseBatch;
		[PXUIField(DisplayName = "View Batch", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewExpenseBatch(PXAdapter adapter)
		{
			if (Expenses.Current != null)
			{
				string BatchNbr = (string)Expenses.GetValueExt<CAExpense.batchNbr>(Expenses.Current);
				if (BatchNbr != null)
				{
					RedirectToBatch(BatchNbr);
				}
			}
			return adapter.Get();
		}

		private void RedirectToBatch(string BatchNbr)
		{
			JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
			graph.Clear();
			graph.BatchModule.Current = PXSelect<Batch,
					Where<Batch.module, Equal<BatchModule.moduleCA>,
					And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
					.Select(this, BatchNbr);
			throw new PXRedirectRequiredException(null, graph: graph, windowMode: PXBaseRedirectException.WindowMode.NewWindow, message: "Batch Record");
		}

		public PXAction<CATransfer> caReversingTransfers;
		[PXUIField(DisplayName = "CA Reversing Transfers", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable CAReversingTransfers(PXAdapter adapter)
		{
			if (Transfer.Current != null)
			{
				var reportParams = new Dictionary<string, string>();
				reportParams["TransferNbr"] = Transfer.Current.TransferNbr;

				throw new PXReportRequiredException(reportParams, "CA659500", "CA Reversing Transfers");
			}
			return adapter.Get();
		}

		#endregion

		protected virtual void _(Events.RowPersisting<CATransfer> e)
		{
			e.Cache.VerifyFieldAndRaiseException<CATransfer.inAccountID>(e.Row);
		}

		#region Internal classes
		/// <summary>
		/// The defailting of the taxCalcMode field is overiden because we do not use the CashTransferEntryTax extension for reversing transfers.
		/// After it is overriten, the (newValue != (string)e.OldValue) condition always returns falls in the TaxBaseGraph.Document_TaxCalcMode_FieldUpdated handler
		/// </summary>
		public class CancelCAExpenseTaxCalcModeFieldDefaulting : IDisposable
		{
			private CashTransferEntry Graph { get; set; }
			private PXFieldDefaulting Event;

			public CancelCAExpenseTaxCalcModeFieldDefaulting(CashTransferEntry graph, CAExpense expense)
			{
				Graph = graph;
				Event = new PXFieldDefaulting((cache, e) => { e.NewValue = expense.TaxCalcMode; e.Cancel = true; });
				Graph.FieldDefaulting.AddHandler<CAExpense.taxCalcMode>(Event);
			}

			public void Dispose()
			{
				Graph.FieldDefaulting.RemoveHandler<CAExpense.taxCalcMode>(Event);
			}
		}
		#endregion
	}
}
