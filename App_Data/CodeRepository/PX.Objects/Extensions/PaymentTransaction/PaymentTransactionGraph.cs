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
using System.Diagnostics;
using System.Linq;

using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.CA;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

using V2 = PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public abstract class PaymentTransactionGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
		where TGraph : PXGraph, new()
		where TPrimary : class, IBqlTable, new()
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public PXSelectExtension<ExternalTransactionDetail> ExternalTransaction;
		public PXSelectExtension<PaymentTransactionDetail> PaymentTransaction;
		public PXSelectExtension<Payment> PaymentDoc;
		public PXSetup<ARSetup> ARSetup;
		public PXFilter<InputPaymentInfo> InputPmtInfo;

		public bool ReleaseDoc { get; set; }
		public string TranHeldwarnMsg { get; set; } = AR.Messages.CCProcessingTranHeldWarning;
		public string SelectedProcessingCenter { get; protected set; }
		public string SelectedProcessingCenterType { get; protected set; }
		protected int? SelectedBAccount { get; set; }
		protected string SelectedPaymentMethod { get; set; }
		protected bool IsNeedSyncContext { get; set; }
		protected CCPaymentProcessing paymentProcessing;

		public PXAction<TPrimary> authorizeCCPayment;
		[PXUIField(DisplayName = "Authorize", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable AuthorizeCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeAuthorizePayment(doc);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry(this.Base);
				paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
				paymentEntry.AuthorizeCCpayment(pDoc, new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction));
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> captureCCPayment;
		[PXUIField(DisplayName = "Capture", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CaptureCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				RunPendingOperations(doc);
				BeforeCapturePayment(doc);
				CheckHeldForReviewTranStatus(pDoc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry(this.Base);
				paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
				paymentEntry.CaptureCCpayment(pDoc, tranAdapter);
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> voidCCPayment;
		[PXUIField(DisplayName = "Void Card Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable VoidCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				PXCache cache = this.Base.Caches[typeof(TPrimary)];
				bool prevAllowUpdateState = cache.AllowUpdate;
				cache.AllowUpdate = true;
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				RunPendingOperations(doc);
				BeforeVoidPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry(this.Base);
				paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
				paymentEntry.VoidCCPayment(pDoc, tranAdapter);
				cache.AllowUpdate = prevAllowUpdateState;
			}
			return list;
		}

		public PXAction<TPrimary> creditCCPayment;
		[PXUIField(DisplayName = "Refund Card Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CreditCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			AccessInfo info = this.Base.Accessinfo;
			PXTrace.WriteInformation($"{methodName} started.");
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeCreditPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				bool useTranRefunds = PaymentDoc.Current?.CCTransactionRefund == true;
				if (string.IsNullOrEmpty(pDoc.RefTranExtNbr) && useTranRefunds)
				{
					PaymentDoc.Cache.RaiseExceptionHandling<Payment.refTranExtNbr>(pDoc, pDoc.RefTranExtNbr, new PXSetPropertyException(AR.Messages.ERR_OrigTranNbrIsRequired));
					continue;
				}
				CCPaymentEntry paymentEntry = GetCCPaymentEntry(this.Base);
				paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
				paymentEntry.CreditCCPayment(pDoc, tranAdapter, SelectedProcessingCenter);
			}
			return list;
		}

		public PXAction<TPrimary> captureOnlyCCPayment;
		[PXUIField(DisplayName = "Record and Capture Preauthorization", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable CaptureOnlyCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			AccessInfo info = this.Base.Accessinfo;
			var parameters = InputPmtInfo.Current;
			if (parameters == null)
				return adapter.Get();
			if (string.IsNullOrEmpty(parameters.AuthNumber))
			{
				if (InputPmtInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.authNumber>(parameters,
					null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(InputPaymentInfo.authNumber)}]")))
					throw new PXRowPersistingException(typeof(InputPaymentInfo.authNumber).Name, null, ErrorMessages.FieldIsEmpty, nameof(InputPaymentInfo.authNumber));
				return adapter.Get();
			}
			List<TPrimary> list = new List<TPrimary>();
			foreach (TPrimary doc in adapter.Get<TPrimary>())
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				list.Add(doc);
				BeforeCaptureOnlyPayment(doc);
				var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
				CCPaymentEntry paymentEntry = GetCCPaymentEntry(this.Base);
				paymentEntry.AfterProcessingManager = GetAfterProcessingManager();
				paymentEntry.CaptureOnlyCCPayment(parameters, pDoc, tranAdapter);
			}
			return list;
		}

		public PXAction<TPrimary> validateCCPayment;
		[PXUIField(DisplayName = "Validate Card Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable ValidateCCPayment(PXAdapter adapter)
		{
			string methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			var list = adapter.Get<TPrimary>().ToList();
			PXLongOperation.StartOperation(Base, delegate
			{
				var processingGraph = GetProcessingGraph();
				foreach (TPrimary doc in list)
				{
					CheckProcCenterDisabled();
					var currDoc = SetCurrentDocument(processingGraph, doc);
					var ext = GetPaymentTransactionExt(processingGraph);
					ext.DoValidateCCPayment(currDoc);
				}
			});
			return list;
		}

		public PXAction<TPrimary> recordCCPayment;
		[PXUIField(DisplayName = "Record Card Payment", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		[ARMigrationModeDependentActionRestriction(
			restrictInMigrationMode: true,
			restrictForRegularDocumentInMigrationMode: true,
			restrictForUnreleasedMigratedDocumentInNormalMode: true)]
		public virtual IEnumerable RecordCCPayment(PXAdapter adapter)
		{
			var methodName = GetClassMethodName();
			PXTrace.WriteInformation($"{methodName} started.");
			AccessInfo info = this.Base.Accessinfo;
			List<TPrimary> list = adapter.Get<TPrimary>().ToList();
			InputPaymentInfo parameters = InputPmtInfo.Current;
			if (parameters == null)
				return list;

			bool validated = ValidateRecordedInfo(parameters);
			if (!validated)
				return list;

			InputPmtInfo.Cache.Clear();
			TPrimary doc = list.FirstOrDefault();
			if (doc != null)
			{
				CheckProcCenterDisabled();
				CheckDocumentUpdatedInDb(doc);
				ICCPayment pDoc = GetPaymentDoc(doc);
				PXTrace.WriteInformation($"{methodName}. RefNbr:{pDoc.RefNbr}; UserName:{info.UserName}");
				if (!CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(SelectedProcessingCenter),
					CCProcessingFeature.TransactionGetter))
				{
					throw new PXException(AR.Messages.ERR_RecordTranNotSupportedWithoutTranGetterFeature, SelectedProcessingCenter);
				}
				var details = GetTranDetails(parameters.PCTranNumber.Trim());
				ValidateTransactionData(pDoc, details);

				if (details.TranType == V2.CCTranType.Credit && details.RefTranID != null && pDoc.RefTranExtNbr == null)
				{
					var extDoc = PaymentDoc.Current;
					PaymentDoc.Cache.SetValue<Payment.refTranExtNbr>(extDoc, details.RefTranID);
				}

				if (pDoc.Released == false)
				{
					Base.Actions.PressSave();
				}

				PXLongOperation.StartOperation(Base, delegate ()
				{
					var processingGraph = GetProcessingGraph();
					var currDoc = SetCurrentDocument(processingGraph, doc);
					var ext = GetPaymentTransactionExt(processingGraph);
					CCPaymentEntry paymentEntry = new CCPaymentEntry(processingGraph);
					ext.CheckSaveCardOption(details);
					ext.RecordTransaction(currDoc, details, paymentEntry);
				});
			}
			return list;
		}

		public void CheckSaveCardOption(V2.TransactionData details)
		{
			if (NeedSaveCard() && details.CustomerId != null && details.PaymentId != null)
			{
				using (PXTransactionScope scope = new PXTransactionScope())
				{
					int? pmInstanceId = CreateProfileIfNeeded(details);
					if (pmInstanceId != PaymentTranExtConstants.NewPaymentProfile)
					{
					SetPmInstanceId(pmInstanceId);
					Base.Actions.PressSave();
					}
					scope.Complete();
				}
			}
		}

		protected virtual TPrimary DoValidateCCPayment(TPrimary doc)
		{
			ICCPayment pDoc = GetPaymentDoc(doc);
			IExternalTransaction tran = ExternalTranHelper.GetActiveTransaction(GetExtTrans());
			if (tran != null)
			{
				TranStatusChanged(pDoc, tran.TransactionID);
			}
			RestoreDocStateByTransactionIfNeeded(doc);
			return doc;
		}

		public virtual void RecordTransaction(TPrimary doc, V2.TransactionData details, CCPaymentEntry paymentEntry)
		{
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager(Base);
			ICCPayment pDoc = GetPaymentDoc(doc);
			CCTranType tranType = V2Converter.ConvertTranType(details.TranType.Value);
			var tranAdapter = new GenericExternalTransactionAdapter<ExternalTransactionDetail>(ExternalTransaction);
			TranRecordData recordData = FormatRecordData(details);
			recordData.RefExternalTranId = pDoc.RefTranExtNbr;
			RaiseBeforeAction(tranType, doc);

			if (DocIsPayment(pDoc))
			{
				if (tranType == CCTranType.AuthorizeAndCapture)
				{
					paymentEntry.RecordAuthCapture(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.PriorAuthorizedCapture)
				{
					paymentEntry.RecordPriorAuthCapture(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.AuthorizeOnly)
				{
					paymentEntry.RecordAuthorization(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.CaptureOnly)
				{
					paymentEntry.RecordCaptureOnly(pDoc, recordData, tranAdapter);
				}
				else if (tranType == CCTranType.Credit)
				{
					paymentEntry.RecordCCCredit(pDoc, recordData, tranAdapter);
				}
			}
			else if (pDoc.DocType.IsIn(ARDocType.Refund, ARDocType.VoidPayment) && tranType == CCTranType.Credit)
			{
				paymentEntry.RecordCCCredit(pDoc, recordData, tranAdapter);
			}
		}

		private TranRecordData FormatRecordData(V2.TransactionData info)
		{
			TranRecordData tranRecord = new TranRecordData();
			tranRecord.ExternalTranId = info.TranID;
			tranRecord.AuthCode = info.AuthCode;
			tranRecord.TransactionDate = PXTimeZoneInfo.ConvertTimeFromUtc(info.SubmitTime, LocaleInfo.GetTimeZone());
			tranRecord.ProcessingCenterId = SelectedProcessingCenter;
			tranRecord.TranStatus = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(info.TranStatus));
			tranRecord.CvvVerificationCode = CVVVerificationStatusCode.GetCCVCode(V2Converter.ConvertCvvStatus(info.CcvVerificationStatus));
			tranRecord.Imported = true;
			tranRecord.CardType = V2Converter.ConvertCardType(info.CardTypeCode);
			tranRecord.ProcCenterCardTypeCode = info.CardType;
			tranRecord.PayLinkExternalId = info.PayLinkExternalID;
			tranRecord.Tax = info.Tax;
			if (info.ExpireAfterDays.HasValue)
			{
				DateTime submitTimeInLocalFormat =
					PXTimeZoneInfo.ConvertTimeFromUtc(info.SubmitTime, LocaleInfo.GetTimeZone());
				DateTime expirationDate = submitTimeInLocalFormat.AddDays(info.ExpireAfterDays.Value);
				tranRecord.ExpirationDate = expirationDate;
			}
			return tranRecord;
		}

		protected virtual bool ValidateRecordedInfo(InputPaymentInfo info)
		{
			bool ret = true;

			if (string.IsNullOrEmpty(info.PCTranNumber))
			{
				var ex = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(InputPaymentInfo.PCTranNumber)}]");
				if (InputPmtInfo.Cache.RaiseExceptionHandling<InputPaymentInfo.pCTranNumber>(info, null, ex))
					throw ex;
				ret = false;
			}
			return ret;
		}

		protected V2.TransactionData GetTranDetails(string transactionId)
		{
			var details = GetPaymentProcessing().GetTransactionById(transactionId, SelectedProcessingCenter);
			return details;
		}

		protected virtual bool RunPendingOperations(TPrimary doc)
		{
			return true;
		}

		protected virtual bool TrySyncByStoredIds(TPrimary doc, IExternalTransaction extTran)
		{
			return false;
		}

		protected virtual void ValidateTransactionData(ICCPayment doc, V2.TransactionData tranData)
		{
			TranValidationHelper.CheckRecordedTranStatus(tranData);
			if (tranData.TranType == V2.CCTranType.Void)
			{
				throw new PXException(AR.Messages.ERR_IncorrectVoidTranType, tranData.TranID);
			}
			if (doc.DocType != ARDocType.Refund && tranData.TranType == V2.CCTranType.Credit)
			{
				throw new PXException(AR.Messages.ERR_IncorrectTranType, tranData.TranID);
			}
			if (doc.DocType == ARDocType.Refund && tranData.TranType != V2.CCTranType.Credit)
			{
				throw new PXException(AR.Messages.ERR_IncorrectTranType, tranData.TranID);
			}
			if (doc.DocType == ARDocType.Refund && tranData.RefTranID != null && doc.RefTranExtNbr != null
				&& tranData.RefTranID != doc.RefTranExtNbr)
			{
				throw new PXException(AR.Messages.ERR_RefundTranNotLinkedWithOrigTran, tranData.TranID, doc.RefTranExtNbr);
			}
			ValidateCustomerProfile(doc, tranData);

			var prms = new TranValidationHelper.AdditionalParams();
			prms.ProcessingCenter = SelectedProcessingCenter;
			prms.PMInstanceId = doc.PMInstanceID;
			prms.Repo = GetPaymentRepository();
			TranValidationHelper.CheckTranAlreadyRecorded(tranData, prms);

			if (tranData.TranUID != null)
			{
				TranValidationHelper.CheckTransactionByNoteId(tranData, prms);
			}

			if (PaymentDoc.Current.CuryDocBal != tranData.Amount)
			{
				throw new PXException(AR.Messages.ERR_IncorrectTranAmount, tranData.TranID);
			}

			var paymentMethod = PaymentMethod.PK.Find(Base, SelectedPaymentMethod);

			if (!((paymentMethod.PaymentType == PaymentMethodType.EFT && tranData.PaymentMethodType == V2.MeansOfPayment.EFT) ||
				(paymentMethod.PaymentType == PaymentMethodType.CreditCard && (tranData.PaymentMethodType == V2.MeansOfPayment.CreditCard || tranData.PaymentMethodType == null))))
			{
				throw new PXException(AR.Messages.ERR_IncorrectPaymentMethodType, paymentMethod.PaymentType, tranData.PaymentMethodType);
			}
		}

		protected void ValidateSharedTranSyncStatus(ICCPayment doc, IExternalTransaction extTran)
		{
			if (DocIsPayment(doc) && extTran.VoidDocType == ARDocType.Refund && extTran.NeedSync == true)
			{
				var docLabel = TranValidationHelper.GetDocumentName(extTran.VoidDocType);
				throw new PXException(AR.Messages.ERR_DocHasNeedSyncSharedTran, extTran.TranNumber, extTran.VoidRefNbr, docLabel);
			}
		}

		protected void ValidateCustomerProfile(ICCPayment doc, V2.TransactionData tranData)
		{
			var repo = GetPaymentRepository();
			var prms = new TranValidationHelper.AdditionalParams();
			prms.CustomerID = SelectedBAccount;
			prms.PMInstanceId = doc.PMInstanceID;
			prms.ProcessingCenter = SelectedProcessingCenter;
			prms.Repo = repo;
			TranValidationHelper.CheckPaymentProfile(tranData, prms);
		}

		protected string GetClassMethodName()
		{
			StackTrace sTrace = new StackTrace();
			string mName = sTrace.GetFrame(1).GetMethod().Name;
			string className = sTrace.GetFrame(1).GetMethod().ReflectedType.Name;
			int index = className.IndexOf('`');
			if (index >= 0)
			{
				className = className.Substring(0, index);
			}
			return className + "." + mName;
		}

		public virtual void clearCCInfo()
		{
			InputPaymentInfo filter = InputPmtInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
		}

		public virtual void initAuthCCInfo(PXGraph aGraph, string ViewName)
		{
			InputPaymentInfo filter = InputPmtInfo.Current;
			filter.PCTranNumber = filter.AuthNumber = null;
			PXUIFieldAttribute.SetVisible<InputPaymentInfo.pCTranNumber>(InputPmtInfo.Cache, filter, false);
		}

		protected virtual CCPaymentEntry GetCCPaymentEntry(PXGraph graph)
		{
			var paymentEntry = new CCPaymentEntry(graph);
			return paymentEntry;
		}

		public override void Initialize()
		{
			base.Initialize();
			MapViews(Base);
		}

		public virtual CCPaymentProcessing GetPaymentProcessing()
		{
			if (paymentProcessing == null)
			{
				paymentProcessing = new CCPaymentProcessing(Base);
			}
			return paymentProcessing;
		}

		public virtual ICCPaymentProcessingRepository GetPaymentRepository()
		{
			if (paymentProcessing == null)
			{
				paymentProcessing = new CCPaymentProcessing(Base);
			}
			return paymentProcessing.Repository;
		}

		protected virtual void CheckDocumentUpdatedInDb(TPrimary doc)
		{
			PXEntryStatus status = Base.Caches[typeof(TPrimary)].GetStatus(doc);
			if (status == PXEntryStatus.Notchanged)
			{
				EntityHelper entityHelper = new EntityHelper(this.Base);
				object[] keys = entityHelper.GetEntityKey(typeof(TPrimary), doc);
				object storedRow = entityHelper.GetEntityRow(typeof(TPrimary), keys);
				if (storedRow != null)
				{
					byte[] ts = Base.Caches[typeof(TPrimary)].GetValue(doc, "tstamp") as byte[];
					byte[] storedTs = Base.Caches[typeof(TPrimary)].GetValue(storedRow, "tstamp") as byte[];
					if (Base.SqlDialect.CompareTimestamps(ts, storedTs) < 0)
					{
						throw new PXException(ErrorMessages.RecordUpdatedByAnotherProcess, typeof(TPrimary).Name);
					}
				}
			}
		}

		protected virtual void CheckProcCenterDisabled()
		{
			CCPluginTypeHelper.ThrowIfProcCenterFeatureDisabled(SelectedProcessingCenterType);
		}

		protected virtual bool IsProcCenterDisabled(string procCenterType)
		{
			return CCPluginTypeHelper.IsProcCenterFeatureDisabled(procCenterType);
		}

		public virtual ICCPayment GetPaymentDoc(TPrimary doc)
		{
			ICCPayment pDoc = doc as ICCPayment;
			if (pDoc == null)
			{
				pDoc = PaymentDoc.View.SelectSingleBound(new object[] { doc }) as ICCPayment;
			}
			if (pDoc == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CCProcessingNotImplementedICCPayment);
			}
			return pDoc;
		}

		public void SyncProfile(TPrimary doc, V2.TransactionData tranData)
		{
			ExternalTransactionDetail storedTran = GetExtTranDetails().FirstOrDefault(i => i.TranNumber == tranData.TranID);
			if (storedTran == null) return;
			if (SelectedProcessingCenter != null && SelectedBAccount != null && storedTran.SaveProfile == true)
			{
				int? pmInstanceId = null;
				if (CheckAllowSavingCards())
				{
					ICCPayment pDoc = GetPaymentDoc(doc);
					if (tranData.CustomerId != null && tranData.PaymentId != null && pDoc.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
					{
						pmInstanceId = CreateProfileIfNeeded(tranData);
					}
					if (pmInstanceId != PaymentTranExtConstants.NewPaymentProfile && pDoc.Released == false)
					{
						SetPmInstanceId(pmInstanceId);
						storedTran.PMInstanceID = pmInstanceId;
						ExternalTransaction.Update(storedTran);
					}
				}
				storedTran.SaveProfile = false;
				ExternalTransaction.Update(storedTran);
			}
		}

		public int? CreateProfileIfNeeded(V2.TransactionData details)
		{
			string custId = details.CustomerId;
			string paymentId = details.PaymentId;
			var repo = GetPaymentRepository();
			var result = repo.GetCustomerPaymentMethodWithProfileDetail(SelectedProcessingCenter, custId, paymentId);
			int? pmInstanceId = PaymentTranExtConstants.NewPaymentProfile;

			if (result != null)
			{
				var cpm = result.Item1;
				if (cpm != null && cpm.IsActive == true && SelectedPaymentMethod == cpm.PaymentMethodID)
				{
				TranValidationHelper.CheckCustomer(details, SelectedBAccount, cpm);
				pmInstanceId = cpm.PMInstanceID;
			}
			}
			else
			{
				var tranProfile = new V2.TranProfile() { CustomerProfileId = custId, PaymentProfileId = paymentId };
				var creator = GetPaymentProfileCreator();
				creator.PrepeareCpmRecord();
				pmInstanceId = creator.CreatePaymentProfile(tranProfile);
				if (pmInstanceId != PaymentTranExtConstants.NewPaymentProfile)
				{
				creator.CreateCustomerProcessingCenterRecord(tranProfile);
				}
				creator.ClearCaches();
			}
			return pmInstanceId;
		}

		public bool TranStatusChanged(ICCPayment doc, int? tranId)
		{
			bool ret = false;
			ExternalTransactionDetail storedExtTran = GetExtTranDetails().Where(i => i.TransactionID == tranId).FirstOrDefault();

			if (SelectedProcessingCenter == null)
			{
				CustomerPaymentMethod cpm = CustomerPaymentMethod.PK.Find(Base, doc.PMInstanceID);
				SelectedProcessingCenter = cpm.CCProcessingCenterID;
				SelectedProcessingCenterType = GetProcessingCenterById(SelectedProcessingCenter)?.ProcessingTypeName;
			}

			if (storedExtTran != null && SelectedProcessingCenter != null)
			{
				bool supported = IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, false);
				if (supported)
				{
					V2.TransactionData tranData = GetPaymentProcessing().GetTransactionById(storedExtTran.TranNumber, SelectedProcessingCenter);
					string newProcStatus = GetProcessingStatus(tranData);
					if (storedExtTran.ProcStatus != newProcStatus)
					{
						UpdateSyncStatus(tranData, storedExtTran);
						if (tranData.TranType == V2.CCTranType.AuthorizeOnly)
						{
							RecordTran(doc, tranData, RecordAuth);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
						{
							RecordTran(doc, tranData, RecordCapture);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.AuthorizeAndCapture)
						{
							RecordTran(doc, tranData, RecordCapture);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.Void)
						{
							RecordTran(doc, tranData, RecordVoid);
							ret = true;
						}
						if (tranData.TranType == V2.CCTranType.Credit)
						{
							RecordTran(doc, tranData, RecordCredit);
							ret = true;
						}
					}
				}
			}
			return ret;
		}

		private void RaiseBeforeAction(CCTranType tranType, TPrimary doc)
		{
			if (tranType == CCTranType.AuthorizeOnly)
			{
				BeforeAuthorizePayment(doc);
			}
			else if (tranType == CCTranType.AuthorizeAndCapture || tranType == CCTranType.PriorAuthorizedCapture)
			{
				BeforeCapturePayment(doc);
			}
			else if (tranType == CCTranType.Credit)
			{
				BeforeCreditPayment(doc);
			}
			else if (tranType == CCTranType.CaptureOnly)
			{
				BeforeCaptureOnlyPayment(doc);
			}
		}

		private void RecordTran(ICCPayment doc, V2.TransactionData tranData, Action<ICCPayment, V2.TransactionData> action)
		{
			action(doc, tranData);
		}

		public void CheckHeldForReviewTranStatus(ICCPayment doc)
		{
			ExternalTransactionState state = GetActiveTransactionState();
			if (state.IsOpenForReview)
			{
				int? tranID = state.ExternalTransaction.TransactionID;
				bool changed = TranStatusChanged(doc, tranID);
				if (changed)
				{
					IExternalTransaction affectedTran = GetExtTrans().Where(i => i.TransactionID == tranID).FirstOrDefault();
					if (affectedTran != null && affectedTran.ProcStatus == ExtTransactionProcStatusCode.VoidSuccess ||
						affectedTran.Active == false)
					{	
						PaymentDoc.Cache.Clear();
						PaymentDoc.Cache.ClearQueryCache();
						throw new PXException(AR.Messages.CCProcessingAuthTranDeclined);
					}
				}
				else
				{
					if (IsFeatureSupported(SelectedProcessingCenter, CCProcessingFeature.TransactionGetter, true))
					{
						throw new PXException(TranHeldwarnMsg);
					}
					else
					{
						throw new PXException(AR.Messages.CCProcessingApprovalHoldingTranNotSupported, SelectedProcessingCenter);
					}
				}
			}
		}

		public ExternalTransactionState GetActiveTransactionState()
		{
			var trans = GetExtTrans();
			var ret = ExternalTranHelper.GetActiveTransactionState(Base, trans);
			return ret;
		}

		public string GetLastTransactionDescription()
		{
			string ret = null;

			var extTrans = GetExtTrans();
			IExternalTransaction targetTran = extTrans.FirstOrDefault();
			if (PaymentDoc.Current?.DocType == ARDocType.VoidPayment && PaymentDoc.Current?.CCActualExternalTransactionID != null)
			{
				targetTran = extTrans.Where(i => i.TransactionID == PaymentDoc.Current?.CCActualExternalTransactionID).FirstOrDefault();
			}
			if (targetTran != null && targetTran.SyncStatus == CCSyncStatusCode.Error && targetTran.Active == false)
			{
				int? actualTranId = PaymentDoc.Current?.CCActualExternalTransactionID;
				if (actualTranId != null)
				{
					var tranDetails = GetPaymentTranDetails();
					var lastProcTranForActual =  tranDetails.Where(i => i.TransactionID == actualTranId).FirstOrDefault();
					var lastProcTranForFailed = tranDetails.Where(i => i.TransactionID == targetTran.TransactionID).FirstOrDefault();
					if (lastProcTranForActual.TranNbr > lastProcTranForFailed.TranNbr)
					{
						targetTran = extTrans.Where(i => i.TransactionID == actualTranId).FirstOrDefault();
					}
				}
			}

			if (targetTran != null)
			{
				ret = GetTransactionStateDescription(targetTran);
			}
			return ret;
		}

		public virtual string GetTransactionStateDescription(IExternalTransaction targetTran)
		{
			return ExternalTranHelper.GetTransactionState(Base, targetTran).Description;
		}

		public bool NeedSaveCard()
		{
			return PaymentDoc?.Current.SaveCard == true
				&& CheckAllowSavingCards();
		}

		public IEnumerable<ICCPaymentTransaction> GetProcTrans()
		{
			foreach (ICCPaymentTransaction item in GetPaymentTranDetails())
			{
				yield return item;
			}
		}

		public IEnumerable<PaymentTransactionDetail> GetPaymentTranDetails()
		{
			if (PaymentTransaction == null)
				yield break;
			foreach (PaymentTransactionDetail item in PaymentTransaction.Select().RowCast<PaymentTransactionDetail>())
			{
				yield return item;
			}
		}

		public IEnumerable<ExternalTransactionDetail> GetExtTranDetails()
		{
			if (ExternalTransaction == null)
				yield break;
			foreach (ExternalTransactionDetail tran in ExternalTransaction.Select().RowCast<ExternalTransactionDetail>())
			{
				yield return tran;
			}
		}

		public IEnumerable<IExternalTransaction> GetExtTrans()
		{
			foreach (IExternalTransaction tran in GetExtTranDetails())
			{
				yield return tran;
			}
		}

		protected virtual void ValidateTran(TPrimary doc, V2.TransactionData tranData)
		{
			IExternalTransaction storedTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranData.TranID);
			if (storedTran == null) return;

			if (storedTran.NeedSync == true)
			{
				ICCPayment pDoc = GetPaymentDoc(doc);
				if (pDoc.DocType == ARDocType.Refund)
				{
					TranValidationHelper.CheckTranTypeForRefund(tranData);

					if (tranData.TranType == V2.CCTranType.Credit && tranData.RefTranID != null && pDoc.RefTranExtNbr != null
						&& tranData.RefTranID != pDoc.RefTranExtNbr)
					{
						throw new TranValidationHelper.TranValidationException(AR.Messages.ERR_RefundTranNotLinkedWithOrigTran,
							tranData.TranID, pDoc.RefTranExtNbr);
					}

					if (tranData.TranType == V2.CCTranType.Void 
						&& NeedMergeTransactionForRefund(pDoc, storedTran, tranData.TranType.Value)) 
					{
						var valParams = new TranValidationHelper.AdditionalParams();
						valParams.Repo = GetPaymentRepository();
						TranValidationHelper.CheckSharedTranIsSuitableForRefund(pDoc, tranData, valParams);
					}
				}
				if (DocIsPayment(pDoc))
				{
					string docTypeRefNbr = pDoc.DocType + pDoc.RefNbr;
					if (pDoc.Released == false && tranData.TranType == V2.CCTranType.Credit)
					{
						throw new TranValidationHelper.TranValidationException(AR.Messages.PaymentIsNotReleased, docTypeRefNbr);
					}
					var activeTranState = GetActiveTransactionState();
					TranValidationHelper.CheckActiveTransactionStateForPayment(pDoc, tranData, activeTranState);
					ValidateSharedTranSyncStatus(pDoc, storedTran);
				}
				string status = GetProcessingStatus(tranData);
				bool captureAfterAuth = storedTran.ProcStatus == ExtTransactionProcStatusCode.AuthorizeSuccess
					&& status == ExtTransactionProcStatusCode.CaptureSuccess;
				bool creditForPmt = status == ExtTransactionProcStatusCode.CreditSuccess
					&& DocIsPayment(pDoc);
				if ((status == storedTran.ProcStatus || captureAfterAuth || creditForPmt) && status != ExtTransactionProcStatusCode.VoidSuccess)
				{
					TranValidationHelper.CheckTranAmount(tranData, storedTran);
				}
				ValidateCustomerProfile(pDoc, tranData);
			}
		}

		protected virtual void UpdateNeedSyncDoc(TPrimary doc, V2.TransactionData tranData)
		{
			ExternalTransactionDetail storedTran = GetExtTranDetails().FirstOrDefault(i => i.TranNumber == tranData.TranID);
			if (storedTran == null) return;

			ICCPayment pDoc = GetPaymentDoc(doc);
			bool isNeedMergeTranForPmt = NeedMergeTransactionForPayment(pDoc, storedTran, tranData.TranType.Value);
			bool isNeedMergeTranForRef = NeedMergeTransactionForRefund(pDoc, storedTran, tranData.TranType.Value);
			
			bool isNeedMergeTran = isNeedMergeTranForPmt || isNeedMergeTranForRef;

			if (storedTran.NeedSync == true)
			{
				if (isNeedMergeTran)
				{
					PaymentTransactionDetail storedProcTran = GetPaymentTranDetails().First(i => i.TransactionID == storedTran.TransactionID);
					PaymentTransaction.Delete(storedProcTran);
					ExternalTransaction.Delete(storedTran);
				}
				else
				{
					storedTran.NeedSync = false;
					if (storedTran.ProcStatus == ExtTransactionProcStatusCode.VoidSuccess)
					{
						storedTran.Active = false;
					}
					if (storedTran.ProcStatus == ExtTransactionProcStatusCode.CreditSuccess)
					{
						storedTran.Active = true;
					}
					ExternalTransaction.Update(storedTran);
				}

				PersistChangesIfNeeded();

				pDoc = GetPaymentDoc(doc);
				string status = GetProcessingStatus(tranData);
				if (isNeedMergeTran)
				{
					TranRecordData recordData = FormatTranRecord(tranData);
					var res = GetPaymentRepository().GetExternalTransactionWithPayment(tranData.RefTranID, SelectedProcessingCenter);
					recordData.RefInnerTranId = res?.Item1.TransactionID;
					if (tranData.TranType == V2.CCTranType.Void)
					{
						if (pDoc.DocType == ARDocType.Refund)
						{
							recordData.AllowFillVoidRef = true;
						}
						RecordVoid(pDoc, recordData);
					}
					if (tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
					{
						RecordCapture(pDoc, V2.CCTranType.PriorAuthorizedCapture, recordData);
					}
				}
				else if(status == storedTran.ProcStatus)
				{
					RunCallbacks(doc, V2Converter.ConvertTranType(tranData.TranType.Value));
				}
				else
				{
					TranStatusChanged(pDoc, storedTran.TransactionID);
				}
			}
		}

		protected bool DocIsPayment(ICCPayment doc)
		{
			return doc.DocType == ARDocType.Payment || doc.DocType == ARDocType.Prepayment;
		}

		protected string GetProcessingStatus(V2.TransactionData tranData)
		{
			ProcessingStatus procStatus = CCProcessingHelper.GetProcessingStatusByTranData(tranData);
			string procStatusStr = ExtTransactionProcStatusCode.GetProcStatusStrByProcessingStatus(procStatus);
			return procStatusStr;
		}

		protected CCProcessingCenter GetProcessingCenterById(string id)
		{
			CCProcessingCenter procCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID, Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(Base, id);
			return procCenter;
		}

		protected virtual PaymentProfileCreator GetPaymentProfileCreator()
		{
			return new PaymentProfileCreator(PXGraph.CreateInstance<CCPaymentHelperGraph>(), SelectedPaymentMethod, SelectedProcessingCenter, SelectedBAccount);
		}

		protected virtual bool IsFeatureSupported(string procCenterId, CCProcessingFeature feature, bool throwOnError)
		{
			bool ret = CCProcessingFeatureHelper.IsFeatureSupported(GetProcessingCenterById(procCenterId), feature, throwOnError);
			return ret;
		}

		protected virtual void RecordAuth(ICCPayment doc, V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			if (tranData.ExpireAfterDays != null)
			{
				DateTime submitTimeInLocalFormat =
					PXTimeZoneInfo.ConvertTimeFromUtc(tranData.SubmitTime, LocaleInfo.GetTimeZone());
				DateTime expirationDate = submitTimeInLocalFormat.AddDays(tranData.ExpireAfterDays.Value);
				tranRecordData.ExpirationDate = expirationDate;
			}
			var paymentEntry = GetCCPaymentEntry(this.Base);
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager(this.Base);
			paymentEntry.RecordAuthorization(doc, tranRecordData);
		}

		private void RunAuthCallbacks(IBqlTable doc)
		{
			var afterProcessingMngr = GetAfterProcessingManager(this.Base);
			afterProcessingMngr.RunAuthorizeActions(doc, true);
			afterProcessingMngr.PersistData();
		}

		protected virtual void RecordVoid(ICCPayment doc, V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			tranRecordData.AuthCode = tranData.AuthCode;
			if (tranData.RefTranID != null)
			{
				var origTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranData.RefTranID && i.Active == true);
				if (origTran != null)
				{
					tranRecordData.RefInnerTranId = origTran.TransactionID;
				}
			}
			RecordVoid(doc, tranRecordData);
		}

		private void RunVoidCallbacks(IBqlTable doc)
		{
			var afterProcessingMngr = GetAfterProcessingManager(this.Base);
			afterProcessingMngr.RunVoidActions(doc, true);
			afterProcessingMngr.PersistData();
		}

		private bool NeedMergeTransactionForPayment(ICCPayment pmt, IExternalTransaction extTran, V2.CCTranType tranType)
		{
			bool isNeedMergeTranForPmt = extTran.Active == false && extTran.NeedSync == true
				&& (tranType == V2.CCTranType.PriorAuthorizedCapture || tranType == V2.CCTranType.Void)
				&& DocIsPayment(pmt);
			return isNeedMergeTranForPmt;
		}

		private bool NeedMergeTransactionForRefund(ICCPayment pmt, IExternalTransaction extTran, V2.CCTranType tranType)
		{
			bool isNeedMergeForRef = extTran.NeedSync == true && tranType == V2.CCTranType.Void
				&& extTran.VoidDocType == null && pmt.DocType == ARDocType.Refund;
			return isNeedMergeForRef;
		}

		protected virtual void RecordCapture(ICCPayment doc, V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			if (tranData.ExpireAfterDays != null)
			{
				DateTime submitTimeInLocalFormat =
					PXTimeZoneInfo.ConvertTimeFromUtc(tranData.SubmitTime, LocaleInfo.GetTimeZone());
				DateTime expirationDate = submitTimeInLocalFormat.AddDays(tranData.ExpireAfterDays.Value);
				tranRecordData.ExpirationDate = expirationDate;
			}
			if (tranData.RefTranID != null && tranData.TranType == V2.CCTranType.PriorAuthorizedCapture)
			{
				var origTran = GetExtTrans().FirstOrDefault(i => i.TranNumber == tranData.RefTranID && i.Active == true);
				if (origTran != null)
				{
					tranRecordData.RefInnerTranId = origTran.TransactionID;
				}
			}
			RecordCapture(doc, tranData.TranType.Value, tranRecordData);
		}

		protected virtual void RecordCredit(ICCPayment doc, V2.TransactionData tranData)
		{
			var paymentEntry = GetCCPaymentEntry(this.Base);
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager(this.Base);
			TranRecordData tranRecordData = FormatTranRecord(tranData);
			tranRecordData.TransactionDate = tranData.SubmitTime;
			tranRecordData.RefExternalTranId = tranData.RefTranID;
			paymentEntry.RecordCredit(doc, tranRecordData);
		}

		protected virtual void DeactivateNotFoundTran(ExternalTransactionDetail extTranDet)
		{
			string msg = PXMessages.LocalizeFormatNoPrefix(AR.Messages.ERR_CCTransactionCanNotBeFoundInProcessingCenter,
				extTranDet.TranNumber, extTranDet.ProcessingCenterID);
			UpdateSyncStatus(extTranDet, SyncStatus.Error, msg);
			DeactivateAndUpdateProcStatus(extTranDet);
		}

		protected virtual void DeactivateAndUpdateProcStatus(ExternalTransactionDetail extTranDet)
		{
			var state = ExternalTranHelper.GetTransactionState(Base, extTranDet);
			var errStatus = ExternalTranHelper.GetPossibleErrorStatusForTran(state);
			extTranDet.NeedSync = false;
			var pmt = PaymentDoc.Current;
			if (extTranDet.DocType == pmt.DocType && extTranDet.RefNbr == pmt.RefNbr)
			{
				extTranDet.Active = false;
			}
			extTranDet.ProcStatus = ExtTransactionProcStatusCode.GetProcStatusStrByProcessingStatus(errStatus);
			ExternalTransaction.Update(extTranDet);
			PaymentTransactionDetail procTran = GetPaymentTranDetails().First(i => i.TransactionID == extTranDet.TransactionID);
			CCTranType tranType = CCTranTypeCode.GetTranTypeByTranTypeStr(procTran.TranType);
			if (tranType != CCTranType.Unknown)
			{
				procTran.TranStatus = CCTranStatusCode.Error;
			}
			PaymentTransaction.Update(procTran);
		}

		protected virtual void UpdateSyncStatus(ExternalTransactionDetail extTranDet, SyncStatus syncStatus, string message)
		{
			string value = CCSyncStatusCode.GetSyncStatusStrBySyncStatus(syncStatus);
			extTranDet.SyncStatus = value;
			if (message != null)
			{
				if (!string.IsNullOrEmpty(extTranDet.SyncMessage))
				{
					if (!extTranDet.SyncMessage.Contains(message))
					{
						extTranDet.SyncMessage += ";" + message;
					}
				}
				else
				{
					extTranDet.SyncMessage = message;
				}
			}
			ExternalTransaction.Update(extTranDet);
		}

		protected virtual void UpdateSyncStatus(V2.TransactionData tranData, ExternalTransactionDetail extTranDet)
		{
			if (extTranDet.SyncStatus != CCSyncStatusCode.Warning && extTranDet.SyncStatus != CCSyncStatusCode.Success)
			{
				UpdateSyncStatus(extTranDet, SyncStatus.Success, null);
			}
		}

		protected virtual void RestoreDocStateByTransactionIfNeeded(TPrimary doc)
		{
			var payment = PaymentDoc.Current;
			if (payment == null) return;

			var manager = GetAfterProcessingManager(Base);
			if (manager != null && !manager.CheckDocStateConsistency(doc))
			{
				var extTrans = GetExtTrans();
				var procTrans = GetProcTrans();
				IExternalTransaction extTran = ExternalTranHelper.GetLastProcessedExtTran(extTrans, procTrans);

				var procTran = procTrans.Where(i => i.TransactionID == extTran.TransactionID 
					&& i.TranStatus != CCTranStatusCode.Error && i.TranStatus != CCTranStatusCode.Declined).FirstOrDefault();
				if (procTran != null)
				{
					var tranType = CCTranTypeCode.GetTranTypeByTranTypeStr(procTran.TranType);
					RunCallbacks(doc, tranType);
				}
			}
		}

		protected virtual void RunCallbacks(TPrimary doc, CCTranType tranType)
		{
			if (tranType == CCTranType.AuthorizeOnly)
			{
				RunAuthCallbacks(doc);
			}
			if (tranType == CCTranType.AuthorizeAndCapture || tranType == CCTranType.PriorAuthorizedCapture)
			{
				RunCaptureCallbacks(doc, tranType);
			}
			if (tranType == CCTranType.Credit)
			{
				RunCreditCallbacks(doc);
			}
			if (tranType == CCTranType.Void)
			{
				RunVoidCallbacks(doc);
			}
			if (tranType == CCTranType.Unknown)
			{
				RunUnknownCallbacks(doc);
			}
		}

		protected virtual void PersistChangesIfNeeded()
		{
			bool needPersist = false;
			var headerStatus = PaymentDoc.Cache.GetStatus(PaymentDoc.Current);
			if (headerStatus != PXEntryStatus.Notchanged)
			{
				needPersist = true;
			}
			else
			{
				bool modifiedTranExists = ExternalTransaction.Cache.Cached
					.RowCast<ExternalTransactionDetail>()
					.Any(i => ExternalTransaction.Cache.GetStatus(i) != PXEntryStatus.Notchanged);
				if (modifiedTranExists)
				{
					needPersist = true;
				}
			}
			if (needPersist)
			{
				Base.Actions["Save"].Press();
			}
		}

		private void RunUnknownCallbacks(IBqlTable doc)
		{
			var afterProcManager = GetAfterProcessingManager(this.Base);
			afterProcManager.RunUnknownActions(doc, true);
			afterProcManager.PersistData();
		}

		private void RunCreditCallbacks(IBqlTable doc)
		{
			var afterProcManager = GetAfterProcessingManager(this.Base);
			afterProcManager.RunCreditActions(doc, true);
			afterProcManager.PersistData();
		}

		private void RunCaptureCallbacks(IBqlTable doc, CCTranType tranType)
		{
			var afterProcessingMngr = GetAfterProcessingManager(this.Base);
			if (tranType == CCTranType.PriorAuthorizedCapture)
			{
				afterProcessingMngr.RunPriorAuthorizedCaptureActions(doc, true);
			}
			else
			{
				afterProcessingMngr.RunCaptureActions(doc, true);
			}
			afterProcessingMngr.PersistData();
		}

		private void RecordVoid(ICCPayment doc, TranRecordData tranRecordData)
		{
			var paymentEntry = GetCCPaymentEntry(this.Base);
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager(this.Base);
			paymentEntry.RecordVoid(doc, tranRecordData);
		}

		private void RecordCapture(ICCPayment doc, V2.CCTranType tranType, TranRecordData tranRecordData)
		{
			var paymentEntry = GetCCPaymentEntry(this.Base);
			paymentEntry.AfterProcessingManager = GetAfterProcessingManager(this.Base);
			if (tranType == V2.CCTranType.AuthorizeAndCapture)
			{
				paymentEntry.RecordAuthCapture(doc, tranRecordData);
			}
			else if (tranType == V2.CCTranType.PriorAuthorizedCapture)
			{
				paymentEntry.RecordPriorAuthCapture(doc, tranRecordData);
			}
			else
			{
				paymentEntry.RecordCaptureOnly(doc, tranRecordData);
			}
		}

		protected bool CheckAllowSavingCards()
		{
			CCProcessingCenter procCenter = GetProcessingCenterById(SelectedProcessingCenter);
			if (procCenter.AllowSaveProfile == false)
			{
				return false;
			}
			return true;
		}

		protected TranRecordData FormatTranRecord(V2.TransactionData tranData)
		{
			TranRecordData tranRecordData = new TranRecordData();
			tranRecordData.ExternalTranId = tranData.TranID;
			tranRecordData.Amount = tranData.Amount;
			tranRecordData.AuthCode = tranData.AuthCode;
			tranRecordData.ResponseCode = tranData.ResponseReasonCode.ToString();
			tranRecordData.ResponseText = tranData.ResponseReasonText;
			tranRecordData.ProcessingCenterId = SelectedProcessingCenter;
			tranRecordData.TransactionDate = PXTimeZoneInfo.ConvertTimeFromUtc(tranData.SubmitTime, LocaleInfo.GetTimeZone());
			tranRecordData.ValidateDoc = false;
			tranRecordData.TranStatus = CCTranStatusCode.GetCode(V2Converter.ConvertTranStatus(tranData.TranStatus));
			string cvvCode = CVVVerificationStatusCode.GetCCVCode(V2Converter.ConvertCardVerificationStatus(tranData.CcvVerificationStatus));
			tranRecordData.CvvVerificationCode = cvvCode;
			tranRecordData.TranUID = tranData.TranUID;
			tranRecordData.CardType = V2Converter.ConvertCardType(tranData.CardTypeCode);
			tranRecordData.ProcCenterCardTypeCode = tranData.CardType;
			tranRecordData.IsLocalValidation = tranData.IsLocalValidation;
			tranRecordData.Subtotal = tranData.SubtotalAmount;
			tranRecordData.Tax = tranData.Tax;
			return tranRecordData;
		}

		protected void ClearTransactionCaches()
		{
			PaymentTransaction.Cache.Clear();
			PaymentTransaction.Cache.ClearQueryCache();
			PaymentTransaction.View.Clear();
			ExternalTransaction.Cache.Clear();
			ExternalTransaction.Cache.ClearQueryCache();
			ExternalTransaction.View.Clear();
		}

		protected void SetPmInstanceId(int? pmInstanceId)
		{
			Payment doc = PaymentDoc.Current;
			int? saved = doc.CashAccountID;
			doc.PMInstanceID = pmInstanceId;
			doc = PaymentDoc.Update(doc);
			doc.CashAccountID = saved;
			PaymentDoc.Update(doc);
		}


		protected virtual void MapViews(TGraph graph)
		{

		}

		protected virtual void BeforeAuthorizePayment(TPrimary doc)
		{

		}

		protected virtual void BeforeCapturePayment(TPrimary doc)
		{

		}

		protected virtual void BeforeVoidPayment(TPrimary doc)
		{
			
		}

		protected virtual void BeforeCreditPayment(TPrimary doc)
		{

		}

		protected virtual void BeforeCaptureOnlyPayment(TPrimary doc)
		{

		}

		protected virtual void SetSyncLock(TPrimary doc)
		{

		}

		protected virtual void RemoveSyncLock(TPrimary doc)
		{

		}

		protected virtual bool LockExists(TPrimary doc)
		{
			return false;
		}

		protected virtual AfterProcessingManager GetAfterProcessingManager()
		{
			return null;
		}

		protected virtual AfterProcessingManager GetAfterProcessingManager(TGraph graph)
		{
			return null;
		}

		protected virtual void RowSelected(Events.RowSelected<TPrimary> e)
		{
		}

		protected virtual void RowSelected(Events.RowSelected<PaymentTransactionDetail> e)
		{
			e.Cache.AllowInsert = false;
			e.Cache.AllowUpdate = false;
			e.Cache.AllowDelete = false;
			PaymentTransactionDetail row = e?.Row;
			if (row != null)
			{
				string docType = this.PaymentDoc.Current?.DocType;
				IEnumerable<ExternalTransactionDetail> extTranDets = ExternalTransaction.Select().RowCast<ExternalTransactionDetail>();
				IEnumerable<PaymentTransactionDetail> storedTrans = null;
				PXSetPropertyException exception = null;

				if(row.TranStatus == CCTranStatusCode.HeldForReview
					&& extTranDets.Where(ii => ii.TransactionID == row.TransactionID && ii.Active == true 
					&& !ExternalTranHelper.IsExpired(ii)).Any())
				{
					storedTrans = PaymentTransaction.Select().RowCast<PaymentTransactionDetail>();
					var lastSuccessful = GetLastSuccessfulTran(storedTrans, row.TransactionID);
					if (lastSuccessful != null && lastSuccessful.TranNbr == row.TranNbr)
					{
						exception = new PXSetPropertyException(TranHeldwarnMsg, PXErrorLevel.RowWarning);
					}
				}

				if (extTranDets.Where(ii => ii.Active == true && ii.DocType == docType && !ExternalTranHelper.IsExpired(ii)).Count() > 1 
					&& extTranDets.Where(ii => ii.TransactionID == row.TransactionID && ii.Active == true
					&& !ExternalTranHelper.IsExpired(ii)).Any())
				{
					if (storedTrans == null)
					{ 
						storedTrans = PaymentTransaction.Select().RowCast<PaymentTransactionDetail>();
					}
					var lastSuccessful = GetLastSuccessfulTran(storedTrans, row.TransactionID);
					if (lastSuccessful != null && lastSuccessful.TranNbr == row.TranNbr)
					{
						exception = new PXSetPropertyException(AR.Messages.CCProcessingARPaymentMultipleActiveTranWarning, PXErrorLevel.RowWarning);
					}
				}

				ExternalTransactionDetail extTranDet = extTranDets.FirstOrDefault(i => i.TransactionID == row.TransactionID);
				if (extTranDet != null && (extTranDet.SyncStatus == CCSyncStatusCode.Error || extTranDet.SyncStatus == CCSyncStatusCode.Warning)
					&& extTranDet.SyncMessage != null)
				{
					if (storedTrans == null)
					{
						storedTrans = PaymentTransaction.Select().RowCast<PaymentTransactionDetail>();
					}
					int? lastTranNbr = storedTrans.Where(i => i.TransactionID == row.TransactionID).Max(i => i.TranNbr);
					if (lastTranNbr == row.TranNbr)
					{
						exception = new PXSetPropertyException(extTranDet.SyncMessage, PXErrorLevel.RowWarning);
					}
				}

				if (exception != null)
				{
					PaymentTransaction.Cache.RaiseExceptionHandling<CCProcTran.tranNbr>(row, row.TranNbr, exception);
				}
			}
		}

		private PaymentTransactionDetail GetLastSuccessfulTran(IEnumerable<PaymentTransactionDetail> storedExtTrans, int? extTranId)
		{
			var lastSuccessful = storedExtTrans.Where(i => i.TransactionID == extTranId &&
					(i.TranStatus == CCTranStatusCode.Approved || i.TranStatus == CCTranStatusCode.HeldForReview))
				.FirstOrDefault();
			return lastSuccessful;
		}

		protected virtual TGraph GetProcessingGraph()
		{
			return PXGraph.CreateInstance<TGraph>();
		}

		protected virtual TPrimary SetCurrentDocument(TGraph graph, TPrimary doc)
		{
			throw new NotImplementedException();
		}

		protected virtual PaymentTransactionGraph<TGraph, TPrimary> GetPaymentTransactionExt(TGraph graph)
		{
			throw new NotImplementedException();
		}

		protected virtual void RowPersisting(Events.RowPersisting<TPrimary> e)
		{
			
		}

		protected abstract ExternalTransactionDetailMapping GetExternalTransactionMapping();

		protected abstract PaymentTransactionDetailMapping GetPaymentTransactionMapping();

		protected abstract PaymentMapping GetPaymentMapping();

		protected class PaymentMapping : IBqlMapping
		{
			public Type PMInstanceID = typeof(Payment.pMInstanceID);
			public Type CashAccountID = typeof(Payment.cashAccountID);
			public Type ProcessingCenterID = typeof(Payment.processingCenterID);
			public Type CuryDocBal = typeof(Payment.curyDocBal);
			public Type CuryID = typeof(Payment.curyID);
			public Type DocType = typeof(Payment.docType);
			public Type RefNbr = typeof(Payment.refNbr);
			public Type OrigDocType = typeof(Payment.origDocType);
			public Type OrigRefNbr = typeof(Payment.origRefNbr);
			public Type RefTranExtNbr = typeof(Payment.refTranExtNbr);
			public Type Released = typeof(Payment.released);
			public Type SaveCard = typeof(Payment.saveCard);
			public Type CCTransactionRefund = typeof(Payment.cCTransactionRefund);
			public Type CCPaymentStateDescr = typeof(Payment.cCPaymentStateDescr);
			public Type CCActualExternalTransactionID = typeof(Payment.cCActualExternalTransactionID);
			
			public Type Table { get; private set; }
			public Type Extension => typeof(Payment);
			public PaymentMapping(Type table)
			{
				Table = table;
			}
		}

		protected class PaymentTransactionDetailMapping : IBqlMapping
		{
			public Type TranNbr = typeof(PaymentTransactionDetail.tranNbr);
			public Type TransactionID = typeof(PaymentTransactionDetail.transactionID);
			public Type PMInstanceID = typeof(PaymentTransactionDetail.pMInstanceID);
			public Type ProcessingCenterID = typeof(PaymentTransactionDetail.processingCenterID);
			public Type DocType = typeof(PaymentTransactionDetail.docType);
			public Type OrigDocType = typeof(PaymentTransactionDetail.origDocType);
			public Type OrigRefNbr = typeof(PaymentTransactionDetail.origRefNbr);
			public Type RefNbr = typeof(PaymentTransactionDetail.refNbr);
			public Type ExpirationDate = typeof(PaymentTransactionDetail.expirationDate);
			public Type ProcStatus = typeof(PaymentTransactionDetail.procStatus);
			public Type TranStatus = typeof(PaymentTransactionDetail.tranStatus);
			public Type TranType = typeof(PaymentTransactionDetail.tranType);
			public Type PCTranNumber = typeof(PaymentTransactionDetail.pCTranNumber);
			public Type AuthNumber = typeof(PaymentTransactionDetail.authNumber);
			public Type PCResponseReasonText = typeof(PaymentTransactionDetail.pCResponseReasonText);
			public Type Amount = typeof(PaymentTransactionDetail.amount);
			public Type Imported = typeof(PaymentTransactionDetail.imported);

			public Type Extension => typeof(PaymentTransactionDetail);
			public Type Table { get; private set; }

			public PaymentTransactionDetailMapping(Type table)
			{
				Table = table;
			}
		}

		protected class ExternalTransactionDetailMapping : IBqlMapping
		{
			public Type TransactionID = typeof(ExternalTransactionDetail.transactionID);
			public Type PMInstanceID = typeof(ExternalTransactionDetail.pMInstanceID);
			public Type DocType = typeof(ExternalTransactionDetail.docType);
			public Type RefNbr = typeof(ExternalTransactionDetail.refNbr);
			public Type OrigDocType = typeof(ExternalTransactionDetail.origDocType);
			public Type OrigRefNbr = typeof(ExternalTransactionDetail.origRefNbr);
			public Type VoidDocType = typeof(ExternalTransactionDetail.voidDocType);
			public Type VoidRefNbr = typeof(ExternalTransactionDetail.voidRefNbr);
			public Type TranNumber = typeof(ExternalTransactionDetail.tranNumber);
			public Type AuthNumber = typeof(ExternalTransactionDetail.authNumber);
			public Type Amount = typeof(ExternalTransactionDetail.amount);
			public Type ProcStatus = typeof(ExternalTransactionDetail.procStatus);
			public Type LastActivityDate = typeof(ExternalTransactionDetail.lastActivityDate);
			public Type Direction = typeof(ExternalTransactionDetail.direction);
			public Type Active = typeof(ExternalTransactionDetail.active);
			public Type Completed = typeof(ExternalTransactionDetail.completed);
			public Type ParentTranID = typeof(ExternalTransactionDetail.parentTranID);
			public Type ExpirationDate = typeof(ExternalTransactionDetail.expirationDate);
			public Type CVVVerification = typeof(ExternalTransactionDetail.cVVVerification);
			public Type NeedSync = typeof(ExternalTransactionDetail.needSync);
			public Type SaveProfile = typeof(ExternalTransactionDetail.saveProfile);
			public Type SyncStatus = typeof(ExternalTransactionDetail.syncStatus);
			public Type SyncMessage = typeof(ExternalTransactionDetail.syncMessage);
			public Type NoteID = typeof(ExternalTransactionDetail.noteID);
			public Type Extension => typeof(ExternalTransactionDetail);
			public Type Table { get; private set; }

			public ExternalTransactionDetailMapping(Type table)
			{
				Table = table;
			}
		}

		protected class InputPaymentInfoMapping : IBqlMapping
		{
			public Type Table { get; private set; }

			public Type Extension => typeof(InputPaymentInfo);

			public Type AuthNumber = typeof(InputPaymentInfo.authNumber);
			public Type PCTranNumber = typeof(InputPaymentInfo.pCTranNumber);

			public InputPaymentInfoMapping(Type table)
			{
				Table = table;
			}
		}

		public static void CheckForHeldForReviewStatusAfterProc(PXGraph graph, IBqlTable aTable, CCTranType procTran, bool success)
		{
			ICCPayment doc = aTable as ICCPayment;
			if (doc != null && success)
			{
				var paymentEntry = graph as ARPaymentEntry;
				var query = new PXSelect<ExternalTransaction, Where<ExternalTransaction.docType, Equal<Required<ExternalTransaction.docType>>,
					And<ExternalTransaction.refNbr, Equal<Required<ExternalTransaction.refNbr>>>>, OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
				var result = query.Select(doc.DocType, doc.RefNbr);
				ExternalTransactionState state = ExternalTranHelper.GetActiveTransactionState(graph, result.RowCast<ExternalTransaction>());
				if (state.IsOpenForReview)
				{
					throw new PXSetPropertyException(AR.Messages.CCProcessingTranHeldWarning, PXErrorLevel.RowWarning);
				}
			}
		}

		public static void ReleaseARDocument(IBqlTable aTable)
		{
			AR.ARRegister toProc = (AR.ARRegister)aTable;
			using (PXTimeStampScope scope = new PXTimeStampScope(null))
			{
				if (!(toProc.Released ?? false))
				{
					List<AR.ARRegister> list = new List<AR.ARRegister>(1);
					list.Add(toProc);
					ARDocumentRelease.ReleaseDoc(list, false);
				}
			}
		}

		protected bool IsActualFinPeriodClosedForBranch(int? branchId)
		{
			FinPeriod finPeriod = GetFinPeriod(branchId);

			if (finPeriod?.Status == FinPeriod.status.Open)
				return false;

			return true;
		}

		protected bool IsFinPeriodValid(int? branchId, bool? restrictAccessToClosedPeriods)
		{
			FinPeriod finPeriod = GetFinPeriod(branchId);

			if (finPeriod != null)
			{
				switch (finPeriod.Status)
				{
					case FinPeriod.status.Open:
						return true;
					case FinPeriod.status.Closed:
						if (restrictAccessToClosedPeriods != false)
							return false;
						else
							return true;
					case FinPeriod.status.Inactive:
					case FinPeriod.status.Locked:
					case null:
						return false;
				}
			}

			return false;
		}

		private FinPeriod GetFinPeriod(int? branchId)
		{
			var branch = PXAccess.GetBranch(branchId);

			if (branch != null)
			{
				var organizationId = branch.Organization.OrganizationID;

				return FinPeriodRepository.FindFinPeriodByDate(DateTime.Today, organizationId);
			}

			return null;
		}
	}
}
