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

using System.Collections;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.Standalone;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Extensions.PaymentTransaction;

namespace PX.Objects.AR.GraphExtensions
{
	public class ARCashSaleEntryPaymentTransaction : PaymentTransactionGraph<ARCashSaleEntry, ARCashSale>
	{
		public static bool IsActive() => PXAccess.FeatureInstalled<CS.FeaturesSet.integratedCardProcessing>();

		public PXSelect<ExternalTransaction> externalTran;

		protected override PaymentTransactionDetailMapping GetPaymentTransactionMapping()
		{
			return new PaymentTransactionDetailMapping(typeof(CCProcTran));
		}

		protected override ExternalTransactionDetailMapping GetExternalTransactionMapping()
		{
			return new ExternalTransactionDetailMapping(typeof(ExternalTransaction));
		}

		protected override PaymentMapping GetPaymentMapping()
		{
			return new PaymentMapping(typeof(ARCashSale));
		}

		protected override void MapViews(ARCashSaleEntry graph)
		{
			this.PaymentTransaction = new PXSelectExtension<PaymentTransactionDetail>(Base.ccProcTran);
			this.ExternalTransaction = new PXSelectExtension<ExternalTransactionDetail>(Base.ExternalTran);
		}

		protected override void BeforeVoidPayment(ARCashSale doc)
		{
			base.BeforeVoidPayment(doc);
			ReleaseDoc = doc.VoidAppl == true && doc.Released == false && this.ARSetup.Current.IntegratedCCProcessing == true;
		}

		protected override void BeforeCapturePayment(ARCashSale doc)
		{
			base.BeforeCapturePayment(doc);
			ReleaseDoc = doc.Released == false && ARSetup.Current.IntegratedCCProcessing == true;
		}

		protected override void BeforeCreditPayment(ARCashSale doc)
		{
			base.BeforeCreditPayment(doc);
			ReleaseDoc = doc.Released == false && ARSetup.Current.IntegratedCCProcessing == true;
		}

		protected override AfterProcessingManager GetAfterProcessingManager(ARCashSaleEntry graph)
		{
			var manager = GetARCashSaleAfterProcessingManager();
			manager.Graph = graph;
			return manager;
		}

		protected override AfterProcessingManager GetAfterProcessingManager()
		{
			return GetARCashSaleAfterProcessingManager();
		}

		private ARCashSaleAfterProcessingManager GetARCashSaleAfterProcessingManager()
		{
			return new ARCashSaleAfterProcessingManager() { ReleaseDoc = true };
		}

		protected override void RowSelected(Events.RowSelected<ARCashSale> e)
		{
			base.RowSelected(e);
			ARCashSale doc = e.Row;
			if (doc == null)
				return;
			TranHeldwarnMsg = AR.Messages.CCProcessingARPaymentTranHeldWarning;
			PXCache cache = e.Cache;
			bool isDocTypePayment = IsDocTypePayment(doc);

			bool isPMInstanceRequired = false;
			if (!string.IsNullOrEmpty(doc.PaymentMethodID))
			{
				isPMInstanceRequired = Base.paymentmethod.Current?.IsAccountNumberRequired ?? false;
			}

			ExternalTransactionState tranState = GetActiveTransactionState();
			bool canAuthorize = CanAuthorize(doc, tranState, isDocTypePayment);
			bool canCapture = CanCapture(doc, tranState, isDocTypePayment);
			bool canVoid = doc.Hold == false && doc.DocType == ARDocType.CashReturn && (tranState.IsCaptured || tranState.IsPreAuthorized) ||
						   (tranState.IsPreAuthorized && isDocTypePayment);
			bool canCredit = doc.Hold == false && doc.DocType == ARDocType.CashReturn && !tranState.IsRefunded && (tranState.IsCaptured || tranState.IsPreAuthorized || string.IsNullOrEmpty(doc.OrigRefNbr));

			CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(Base, doc.ProcessingCenterID);
			bool canAuthorizeIfExtAuthOnly = procCenter?.IsExternalAuthorizationOnly == false;
			bool canCaptureIfExtAuthOnly = canAuthorizeIfExtAuthOnly || procCenter?.IsExternalAuthorizationOnly == true && tranState.IsActive == true;

			SelectedProcessingCenterType = procCenter?.ProcessingTypeName;
			bool enableCCProcess = EnableCCProcess(doc);

			this.authorizeCCPayment.SetEnabled(canAuthorize && canAuthorizeIfExtAuthOnly);
			this.captureCCPayment.SetEnabled(canCapture && canCaptureIfExtAuthOnly);
			this.voidCCPayment.SetEnabled(enableCCProcess && canVoid);
			this.creditCCPayment.SetEnabled(enableCCProcess && canCredit);
			doc.CCPaymentStateDescr = GetPaymentStateDescr(tranState);

			bool canValidate = CanValidate(doc);
			this.validateCCPayment.SetEnabled(canValidate);

			this.recordCCPayment.SetEnabled(false);
			this.recordCCPayment.SetVisible(false);
			this.captureOnlyCCPayment.SetEnabled(false);
			this.captureOnlyCCPayment.SetVisible(false);

			PXUIFieldAttribute.SetRequired<ARCashSale.extRefNbr>(cache, enableCCProcess || ARSetup.Current.RequireExtRef == true);
			PXUIFieldAttribute.SetVisible<ARCashSale.cCPaymentStateDescr>(cache, doc, enableCCProcess && doc.CCPaymentStateDescr != null);
			PXUIFieldAttribute.SetVisible<ARCashSale.refTranExtNbr>(cache, doc, ((doc.DocType == ARDocType.CashReturn) && enableCCProcess));
			PXUIFieldAttribute.SetRequired<ARPayment.pMInstanceID>(cache, isPMInstanceRequired);
			PXDefaultAttribute.SetPersistingCheck<ARPayment.pMInstanceID>(cache, doc, isPMInstanceRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			if (doc.Released == true || doc.Voided == true)
			{
				cache.AllowUpdate = enableCCProcess;
			}
			else if (enableCCProcess && (tranState.IsPreAuthorized || tranState.IsCaptured
				|| (doc.DocType == ARDocType.CashReturn && (tranState.IsRefunded || CheckLastProcessedTranIsVoided(doc)))))
			{
				PXUIFieldAttribute.SetEnabled(cache, doc, false);
				if (doc.Status != ARDocStatus.PendingApproval)
				{
					PXUIFieldAttribute.SetEnabled<ARCashSale.adjDate>(cache, doc, true);
					PXUIFieldAttribute.SetEnabled<ARCashSale.adjFinPeriodID>(cache, doc, true);
				}
				PXUIFieldAttribute.SetEnabled<ARCashSale.hold>(cache, doc, true);
				//calculate only on data entry, differences from the applications will be moved to RGOL upon closure
				PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDocBal>(cache, null, true);
				PXDBCurrencyAttribute.SetBaseCalc<ARCashSale.curyDiscBal>(cache, null, true);

				cache.AllowDelete = doc.DocType == ARDocType.CashReturn && !tranState.IsRefunded && !CheckLastProcessedTranIsVoided(doc);
				cache.AllowUpdate = true;
				Base.Transactions.Cache.AllowDelete = true;
				Base.Transactions.Cache.AllowUpdate = true;
				Base.Transactions.Cache.AllowInsert = doc.CustomerID != null && doc.CustomerLocationID != null;
				Base.release.SetEnabled(doc.Hold == false);
				Base.voidCheck.SetEnabled(false);
			}
			else
			{

				PXUIFieldAttribute.SetEnabled<ARCashSale.refTranExtNbr>(cache, doc, enableCCProcess && ((doc.DocType == ARDocType.CashReturn) && !tranState.IsRefunded));
				PXUIFieldAttribute.SetEnabled<ARPayment.pMInstanceID>(cache, doc, isPMInstanceRequired);
				cache.AllowDelete = !ExternalTranHelper.HasTransactions(Base.ExternalTran);
			}

			#region CCProcessing integrated with doc
			if (enableCCProcess && CCProcessingHelper.IntegratedProcessingActivated(ARSetup.Current))
			{
				if (doc.Released == false)
				{
					bool releaseActionEnabled = doc.Hold == false &&
												doc.OpenDoc == true &&
											   (doc.DocType == ARDocType.CashReturn ? tranState.IsRefunded : tranState.IsCaptured);

					Base.release.SetEnabled(releaseActionEnabled);
				}
			}
			#endregion

			PXUIFieldAttribute.SetEnabled<ARCashSale.docType>(cache, doc, true);
			PXUIFieldAttribute.SetEnabled<ARCashSale.refNbr>(cache, doc, true);
			ShowWarningIfActualFinPeriodClosed(e, doc);
			ShowWarningIfExternalAuthorizationOnly(e, doc);

			SetActionCaptions();

			EnableDisableFieldsAndActions(doc, tranState, cache);
		}

		protected virtual void EnableDisableFieldsAndActions(ARCashSale doc, ExternalTransactionState tranState, PXCache cache)
		{
			if (doc.Released == true || doc.IsCCPayment == false)
			{
				return;
			}
			
			bool isBalanced = doc.PendingProcessing == false && doc.Voided == false && doc.Hold == false;
			bool isHoldOrPendingProcessing = doc.Hold == true || doc.PendingProcessing == true;
			bool isHoldAndCaptured = doc.Hold == true && tranState.IsCaptured;
			switch (doc.DocType)
			{
				case ARDocType.CashSale:
					SetHeaderFields(cache, doc, (isHoldOrPendingProcessing && (!tranState.IsCaptured || tranState.IsPreAuthorized)) || isHoldAndCaptured);
					SetAdditionalHeaderFields(cache, doc, isHoldOrPendingProcessing && (!tranState.IsCaptured || tranState.IsPreAuthorized));
					SetDetailsTabFields(Base.Transactions.Cache, !(isHoldAndCaptured || isBalanced));
					SetFinancialTabFields(cache, doc, (isHoldOrPendingProcessing && (!tranState.IsCaptured || tranState.IsPreAuthorized)) || isHoldAndCaptured);

					PXUIFieldAttribute.SetEnabled<ARTran.qty>(Base.Transactions.Cache, null, !(isHoldAndCaptured || isBalanced));

					bool csTabPermissions = !(isHoldAndCaptured || isBalanced);
					SetTabPermissions(Base.Transactions, csTabPermissions, null, null, csTabPermissions);
					SetTabPermissions(Base.Taxes, csTabPermissions, null, csTabPermissions, csTabPermissions);
					SetTabPermissions(Base.salesPerTrans, !isBalanced, null, !isBalanced, !isBalanced);

					break;
				case ARDocType.CashReturn:
					bool isVoidedOrRefunded = tranState.IsRefunded || tranState.IsVoided || CheckLastProcessedTranIsVoided(doc);
					bool isVoidedOrRefundedOrCaptured = isVoidedOrRefunded || tranState.IsCaptured;
					bool isExistActiveApprovalMap = PXSelectReadonly<EP.EPAssignmentMap, Where<EP.EPAssignmentMap.entityType, Equal<EP.AssignmentMapType.AssignmentMapTypeARCashSale>>>.Select(Base).Count != 0 &&
						PXSelectReadonly<ARSetupApproval, Where<ARSetupApproval.docType, Equal<ARDocType.cashReturn>, And<ARSetupApproval.isActive, Equal<True>>>>.Select(Base).Count != 0;
					bool isPendingApproval = doc.Hold == false && doc.Approved == false && doc.DontApprove == false;
					bool isApprovedOrPending = isExistActiveApprovalMap && (isPendingApproval || doc.Approved == true);
					bool isPendingProcessingOrBalanced = doc.PendingProcessing == true || (doc.Voided == false && doc.Hold == false);
					bool isCreatedFromCS = !string.IsNullOrEmpty(doc.OrigRefNbr);

					// Header
					SetHeaderFields(cache, doc, !((isPendingProcessingOrBalanced && isApprovedOrPending) && doc.Hold == false || isVoidedOrRefunded && doc.Hold == false));
					SetAdditionalHeaderFields(cache, doc, !((isPendingProcessingOrBalanced && isApprovedOrPending) || (isCreatedFromCS && doc.Hold == true) || (isCreatedFromCS && doc.Hold == false) || isVoidedOrRefunded));
					PXUIFieldAttribute.SetEnabled<ARCashSale.pMInstanceID>(cache, doc, isPendingApproval && !isVoidedOrRefundedOrCaptured);

					// Details
					bool canEditDetails = !((isHoldOrPendingProcessing || doc.Voided == false) && (isVoidedOrRefunded || isExistActiveApprovalMap && (doc.Approved == true || tranState.IsCaptured)));
					SetDetailsTabFields(Base.Transactions.Cache, canEditDetails);
					SetTabPermissions(Base.Transactions, canEditDetails, null, canEditDetails, canEditDetails);
					PXUIFieldAttribute.SetEnabled<ARTran.qty>(Base.Transactions.Cache, null, !((doc.Hold == true || doc.PendingProcessing == true || doc.Voided == false) && isVoidedOrRefunded));

					// Financial
					SetFinancialTabFields(cache, doc, !((isHoldOrPendingProcessing || doc.Voided == false) && isVoidedOrRefunded && isHoldAndCaptured || (isBalanced && tranState.IsRefunded) || isApprovedOrPending));
					SetTaxTabFields(Base.Transactions.Cache, !((isHoldOrPendingProcessing || doc.Voided == false) && isVoidedOrRefunded));

					// Taxes
					bool crTabPermission = !((isHoldOrPendingProcessing || doc.Voided == false) && (isVoidedOrRefunded || isExistActiveApprovalMap && (doc.Approved == true || tranState.IsCaptured)));
					SetTabPermissions(Base.Taxes, crTabPermission, null, crTabPermission, crTabPermission);

					break;
			}			
		}

		protected virtual void SetHeaderFields(PXCache cache, ARCashSale doc, bool value)
		{
			PXUIFieldAttribute.SetEnabled<ARCashSale.projectID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.docDesc>(cache, doc, value);
		}

		protected virtual void SetAdditionalHeaderFields(PXCache cache, ARCashSale doc, bool value)
		{
			PXUIFieldAttribute.SetEnabled<ARCashSale.termsID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.extRefNbr>(cache, doc, value);
		}

		protected virtual void SetDetailsTabFields(PXCache cache, bool value)
		{
			PXUIFieldAttribute.SetEnabled<ARTran.curyUnitPrice>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.discPct>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.curyDiscAmt>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.curyExtPrice>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.manualDisc>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.taxCategoryID>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.inventoryID>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTran.uOM>(cache, null, value);
		}

		protected virtual void SetFinancialTabFields(PXCache cache, ARCashSale doc, bool value)
		{
			PXUIFieldAttribute.SetEnabled<ARCashSale.branchID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.aRAccountID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.aRSubID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.taxZoneID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.externalTaxExemptionNumber>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.avalaraCustomerUsageType>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.workgroupID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.ownerID>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.dontPrint>(cache, doc, value);
			PXUIFieldAttribute.SetEnabled<ARCashSale.dontEmail>(cache, doc, value);
		}

		protected virtual void SetTaxTabFields(PXCache cache, bool value)
		{
			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxID>(cache, null, value);
			PXUIFieldAttribute.SetEnabled<ARTaxTran.taxRate>(cache, null, value);
		}

		protected virtual void SetTabPermissions(PXSelectBase view, bool? allowInsert, bool? allowSelect, bool? allowUpdate, bool? allowDelete)
		{
			if (allowInsert.HasValue)
			{
				view.AllowInsert = allowInsert.Value;
			}
			if (allowSelect.HasValue)
			{
				view.AllowSelect = allowSelect.Value;
			}
			if (allowUpdate.HasValue)
			{
				view.AllowUpdate = allowUpdate.Value;
			}
			if (allowDelete.HasValue)
			{
				view.AllowDelete = allowDelete.Value;
			}
		}

		private void SetActionCaptions()
		{
			bool isEft = string.Equals(Base.paymentmethod.Current?.PaymentType, PaymentMethodType.EFT);
			this.voidCCPayment.SetCaption(isEft ? Messages.VoidEftPayment : Messages.VoidCardPayment);
			this.creditCCPayment.SetCaption(isEft ? Messages.RefundEftPayment : Messages.RefundCardPayment);
			this.validateCCPayment.SetCaption(isEft ? Messages.ValidateEftPayment : Messages.ValidateCardPayment);
		}

		private bool IsCCPaymentMethod(ARCashSale doc)
		{
			if (string.IsNullOrEmpty(doc.PaymentMethodID))
				return false;

			PaymentMethod paymentMethod = PaymentMethod.PK.Find(Base, doc.PaymentMethodID);
			return paymentMethod?.PaymentType == PaymentMethodType.CreditCard || paymentMethod?.PaymentType == PaymentMethodType.EFT;
		}

		protected virtual void ShowWarningIfActualFinPeriodClosed(Events.RowSelected<ARCashSale> e, ARCashSale doc)
		{
			bool isCCPaymentMethod = IsCCPaymentMethod(doc);
			if (isCCPaymentMethod && IsActualFinPeriodClosedForBranch(PXContext.GetBranchID()) &&
				string.IsNullOrEmpty(PXUIFieldAttribute.GetError<ARCashSale.paymentMethodID>(e.Cache, doc)))
			{
				e.Cache.RaiseExceptionHandling<ARCashSale.paymentMethodID>(doc, doc.PaymentMethodID,
					new PXSetPropertyException(Messages.CreditCardProcessingIsDisabled, PXErrorLevel.Warning,
					PXAccess.GetBranch(PXContext.GetBranchID()).Organization.OrganizationCD));
			}

			if (isCCPaymentMethod && IsActualFinPeriodClosedForBranch(doc.BranchID) &&
				string.IsNullOrEmpty(PXUIFieldAttribute.GetError<ARCashSale.paymentMethodID>(e.Cache, doc)))
			{
				e.Cache.RaiseExceptionHandling<ARCashSale.paymentMethodID>(doc, doc.PaymentMethodID,
					new PXSetPropertyException(Messages.CreditCardProcessingIsDisabled, PXErrorLevel.Warning,
					PXAccess.GetBranch(doc.BranchID).Organization.OrganizationCD));
			}
		}

		protected virtual void ShowWarningIfExternalAuthorizationOnly(Events.RowSelected<ARCashSale> e, ARCashSale doc)
		{
			ExternalTransactionState state = GetActiveTransactionState();
			CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(Base, doc.ProcessingCenterID);
			CustomerPaymentMethod cpm = CustomerPaymentMethod.PK.Find(Base, doc.PMInstanceID);

			bool IsExternalAuthorizationOnly = procCenter?.IsExternalAuthorizationOnly == true && !state.IsActive
												&& doc.Status == ARDocStatus.CCHold && doc.DocType == Standalone.ARCashSaleType.CashSale;

			UIState.RaiseOrHideErrorByErrorLevelPriority<ARCashSale.pMInstanceID>(e.Cache, e.Row, IsExternalAuthorizationOnly,
				Messages.CardAssociatedWithExternalAuthorizationOnlyProcessingCenter, PXErrorLevel.Warning, cpm?.Descr, procCenter?.ProcessingCenterID);
		}

		protected virtual void FieldUpdated(Events.FieldUpdated<ARCashSale.paymentMethodID> e)
		{
			PXCache cache = e.Cache;
			ARCashSale cashSale = e.Row as ARCashSale;
			if (cashSale == null) return;
			SetPendingProcessingIfNeeded(cache, cashSale);
		}

		public static bool IsDocTypeSuitableForCC(ARCashSale doc)
		{
			bool isDocTypeSuitableForCC = (doc.DocType == ARDocType.CashSale) || (doc.DocType == ARDocType.CashReturn);
			return isDocTypeSuitableForCC;
		}

		public static bool IsDocTypePayment(ARCashSale doc)
		{
			bool docTypePayment = doc.DocType == ARDocType.CashSale;
			return docTypePayment;
		}

		public bool EnableCCProcess(ARCashSale doc)
		{
			bool enableCCProcess = false;

			if (doc.IsMigratedRecord != true &&
				Base.paymentmethod.Current != null &&
				(Base.paymentmethod.Current.PaymentType == CA.PaymentMethodType.CreditCard ||
				Base.paymentmethod.Current.PaymentType == CA.PaymentMethodType.EFT))
			{
				enableCCProcess = IsDocTypeSuitableForCC(doc);
			}
			enableCCProcess &= !doc.Voided.Value;

			bool disabledProcCenter = IsProcCenterDisabled(SelectedProcessingCenterType);
			enableCCProcess &= !disabledProcCenter;

			return enableCCProcess &&
				IsFinPeriodValid(PXContext.GetBranchID(), Base.glsetup.Current.RestrictAccessToClosedPeriods) &&
				IsFinPeriodValid(doc.BranchID, Base.glsetup.Current.RestrictAccessToClosedPeriods);
		}

		private bool CanAuthorize(ARCashSale doc, ExternalTransactionState tranState, bool isDocTypePayment)
		{
			bool enableCCProcess = EnableCCProcess(doc);
			if (!enableCCProcess)
				return false;

			bool isEft = Base.paymentmethod.Current != null && Base.paymentmethod.Current.PaymentType == CA.PaymentMethodType.EFT;

			return doc.Hold == false && isDocTypePayment && !(tranState.IsPreAuthorized || tranState.IsCaptured) && doc.CuryDocBal > 0 && !isEft;
		}

		private bool CanCapture(ARCashSale doc, ExternalTransactionState tranState, bool isDocTypePayment)
		{
			bool enableCCProcess = EnableCCProcess(doc);
			if (!enableCCProcess)
				return false;
			
			return doc.Hold == false && isDocTypePayment && !tranState.IsCaptured && doc.CuryDocBal > 0;
		}

		public bool CanValidate(ARCashSale doc)
		{
			bool enableCCProcess = EnableCCProcess(doc);

			if (!enableCCProcess)
				return false;

			PXCache cache = Base.Document.Cache;
			bool isDocTypePayment = IsDocTypePayment(doc);
			ExternalTransactionState tranState = GetActiveTransactionState();
			bool canValidate = doc.Hold == false && isDocTypePayment && tranState.IsActive &&
				cache.GetStatus(doc) != PXEntryStatus.Inserted;

			if (!canValidate)
				return false;

			canValidate = CanCapture(doc, tranState, isDocTypePayment) || CanAuthorize(doc, tranState, isDocTypePayment) || tranState.IsOpenForReview
				|| ExternalTranHelper.HasImportedNeedSyncTran(Base, GetExtTrans())
				|| tranState.NeedSync || tranState.IsImportedUnknown || doc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile;

			if (canValidate && doc.DocType == ARDocType.Refund)
			{
				var sharedTranStatus = ExternalTranHelper.GetSharedTranStatus(Base, GetExtTrans().FirstOrDefault());
				if (sharedTranStatus == ExternalTranHelper.SharedTranStatus.ClearState
					|| sharedTranStatus == ExternalTranHelper.SharedTranStatus.Synchronized)
				{
					canValidate = false;
				}
			}

			if (!canValidate)
			{
				var manager = GetAfterProcessingManager(Base);
				canValidate = manager != null && !manager.CheckDocStateConsistency(doc);
			}

			canValidate = canValidate && GettingDetailsByTranSupported();

			return canValidate;
		}

		private string GetPaymentStateDescr(ExternalTransactionState state)
		{
			return GetLastTransactionDescription();
		}

		public override string GetTransactionStateDescription(IExternalTransaction targetTran)
		{
			var extTrans = GetExtTrans();
			foreach (var extTran in extTrans)
			{
				if (extTran.TransactionID == targetTran.ParentTranID)
				{
					if (extTran.ProcStatus == ExtTransactionProcStatusCode.VoidSuccess)
						return ExternalTranHelper.GetTransactionState(Base, extTran).Description;
				}
			}

			return base.GetTransactionStateDescription(targetTran);
		}

		private bool GettingDetailsByTranSupported()
		{
			CCProcessingCenter procCenter = Base.ProcessingCenter.SelectSingle();
			return CCProcessingFeatureHelper.IsFeatureSupported(procCenter, CCProcessingFeature.TransactionGetter, false);
		}

		protected void SetPendingProcessingIfNeeded(PXCache sender, ARCashSale document)
		{
			PaymentMethod pm = new PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>(Base)
				.SelectSingle(document.PaymentMethodID);
			bool pendingProc = false;
			if (CCProcessingHelper.PaymentMethodSupportsIntegratedProcessing(pm) && document.Released == false)
			{
				pendingProc = true;
			}
			sender.SetValue<ARRegister.pendingProcessing>(document, pendingProc);
		}

		protected override ARCashSale SetCurrentDocument(ARCashSaleEntry graph, ARCashSale doc)
		{
			var document = graph.Document;
			document.Current = document.Search<ARCashSale.refNbr>(doc.RefNbr, doc.DocType);
			return document.Current;
		}

		protected override PaymentTransactionGraph<ARCashSaleEntry, ARCashSale> GetPaymentTransactionExt(ARCashSaleEntry graph)
		{
			return graph.GetExtension<ARCashSaleEntryPaymentTransaction>();
		}

		private bool CheckLastProcessedTranIsVoided(ARCashSale cashSale)
		{
			var extTrans = GetExtTrans();
			var externalTran = ExternalTranHelper.GetLastProcessedExtTran(extTrans, GetProcTrans());
			
			bool ret = false;
			var transaction = extTrans.Where(i => i.TransactionID == externalTran.TransactionID).FirstOrDefault();
			if (transaction != null)
			{
				var state = ExternalTranHelper.GetTransactionState(Base, transaction);
				ret = state.IsVoided;
			}
			return ret;
		}

		public PXAction<ARCashSale> authorizeCCPayment;

		[PXUIField(DisplayName = "Authorize", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable AuthorizeCCPayment(PXAdapter adapter)
		{
			if (base.PaymentDoc.Current != null)
			{
				CalcTax(base.PaymentDoc.Current);
				return base.AuthorizeCCPayment(adapter);
			}
			return adapter.Get();
		}

		public PXAction<ARCashSale> captureCCPayment;

		[PXUIField(DisplayName = "Capture", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable CaptureCCPayment(PXAdapter adapter)
		{
			if (base.PaymentDoc.Current != null)
			{
				CalcTax(base.PaymentDoc.Current);
				return base.CaptureCCPayment(adapter);
			}
			return adapter.Get();
		}

		public virtual void CalcTax(Payment payment)
		{
			ARCashSale currentDocument = Base.CurrentDocument.Current;
			payment.Tax = currentDocument.CuryTaxTotal;
			payment.SubtotalAmount = currentDocument.CuryOrigDocAmt - payment.Tax;
		}

	}
}
