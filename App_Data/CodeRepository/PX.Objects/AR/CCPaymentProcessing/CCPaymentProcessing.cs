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
using System.Net;
using System.Text;
using System.Linq;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.CA;
using PX.Objects.CM.Extensions;
using PX.Objects.CM.TemporaryHelpers;
using PX.Objects.Common;
using PX.Objects.Extensions.PaymentTransaction;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.CC;
using PX.Objects.SO;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCPaymentProcessing
	{
		#region Private Members		

		private ICCPaymentProcessingRepository _repository;

		public ICCPaymentProcessingRepository Repository => _repository;

		private Func<object, CCProcessingContext, ICardTransactionProcessingWrapper> _transactionProcessingWrapper;

		private Func<object, ICardProcessingReadersProvider, IHostedPaymentFormProcessingWrapper> _hostedPaymentFormProcessingWrapper;

		public Func<object, ICardProcessingReadersProvider, IHostedPaymentFormProcessingWrapper> HostedPaymnetFormProcessingWrapper
		{
			get
			{
				if (_hostedPaymentFormProcessingWrapper == null)
				{
					return (plugin, provider) => HostedFromProcessingWrapper.GetPaymentFormProcessingWrapper(plugin, provider, null);
				}
				return _hostedPaymentFormProcessingWrapper;
			}
			set
			{
				_hostedPaymentFormProcessingWrapper = value;
			}
		}

		#endregion
		#region Public Members
		public CCPaymentProcessing()
		{
			_repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public CCPaymentProcessing(PXGraph contextGraph)
		{
			if (contextGraph == null)
			{
				throw new ArgumentNullException(nameof(contextGraph));
			}
			_repository = new CCPaymentProcessingRepository(contextGraph);
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public CCPaymentProcessing(ICCPaymentProcessingRepository repo)
		{
			_repository = repo ?? throw new ArgumentNullException(nameof(repo));
			_transactionProcessingWrapper = (plugin, context) => CardTransactionProcessingWrapper.GetTransactionProcessingWrapper(plugin, new CardProcessingReadersProvider(context));
		}

		public static ICCPaymentProcessor GetCCPaymentProcessing()
		{
			return PXGraph.CreateInstance<CCPaymentProcessingGraph>();
		}
		#endregion

		#region Public Auxiliary Functions
		public virtual bool TestCredentials(PXGraph callerGraph, string processingCenterID)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID,
					Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(callerGraph, processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			APIResponse apiResponse = new APIResponse();
			processor.TestCredentials(apiResponse);
			ProcessAPIResponse(apiResponse);
			return apiResponse.isSucess;
		}

		public virtual void ValidateSettings(PXGraph callerGraph, string processingCenterID, PluginSettingDetail settingDetail)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID,
					Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(callerGraph, processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			CCError result = processor.ValidateSettings(settingDetail);
			if (result.source != CCError.CCErrorSource.None)
			{
				throw new PXSetPropertyException(result.ErrorMessage, PXErrorLevel.Error);
			}
		}

		public virtual IList<PluginSettingDetail> ExportSettings(PXGraph callerGraph, string processingCenterID)
		{
			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = callerGraph
			};
			CCProcessingCenter processingCenter = PXSelect<CCProcessingCenter,
				Where<CCProcessingCenter.processingCenterID,
					Equal<Required<CCProcessingCenter.processingCenterID>>>>.Select(callerGraph, processingCenterID);
			CCProcessingFeatureHelper.CheckProcessing(processingCenter, CCProcessingFeature.Base, context);
			if (context.processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			var processor = GetProcessingWrapper(context);
			List<PluginSettingDetail> processorSettings = new List<PluginSettingDetail>();
			processor.ExportSettings(processorSettings);
			return processorSettings;
		}
		#endregion
		#region Public Processing Functions

		public virtual TranOperationResult Authorize(ICCPayment payment, bool aCapture)
		{
			IsValidPmInstance(payment.PMInstanceID);
			(CustomerPaymentMethod pmInstance, Customer customer) = this.FindCpmAndCustomer(payment.PMInstanceID);
			CCProcessingCenter procCenter = _repository.FindProcessingCenter(pmInstance.PMInstanceID, null);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterIsNotSpecified, pmInstance.Descr);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingIsInactive, procCenter.ProcessingCenterID);
			}
			if (procCenter.IsExternalAuthorizationOnly == true)
			{
				throw new PXException(Messages.ERR_CCProcessingDoesNotSupportAuthorizeAction, procCenter.ProcessingCenterID);
			}

			string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

			CCTranType tranType = aCapture ? CCTranType.AuthorizeAndCapture : CCTranType.AuthorizeOnly;
			CCProcTran tran = new CCProcTran();
			tran.Copy(payment);
			CopyL2DataToProcTran(payment, tran);
			return this.DoTransaction(tranType, tran, null, customer.AcctCD, meansOfPayment);
		}

		public virtual TranOperationResult Capture(ICCPayment payment, int? transactionId)
		{
			ExternalTransaction extTran = ExternalTransaction.PK.Find(_repository.Graph, transactionId);
			IsValidPmInstance(payment.PMInstanceID);

			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionIsNotFound, transactionId);
			}
			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}
			if (ExternalTranHelper.IsExpired(extTran))
			{
				throw new PXException(Messages.ERR_CCAuthorizationTransactionHasExpired, transactionId);
			}

			(CCProcessingCenter procCenter, Customer customer) = GetProcCenterAndCustomer(payment, extTran);
			string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			CCProcTran procTran = new CCProcTran
			{
				PMInstanceID = payment.PMInstanceID,
				RefTranNbr = sTran.TranNbr,
				TransactionID = extTran.TransactionID,
				DocType = extTran.DocType,
				RefNbr = extTran.RefNbr,
				CuryID = payment.CuryID,
				Amount = payment.CuryDocBal,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr,
				ProcessingCenterID = procCenter.ProcessingCenterID
			};

			CopyL2DataToProcTran(payment, procTran);

			return this.DoTransaction(CCTranType.PriorAuthorizedCapture, procTran, extTran.TranNumber, customer.AcctCD, meansOfPayment);
		}

		public virtual TranOperationResult CaptureOnly(ICCPayment payment, string authNbr)
		{
			IsValidPmInstance(payment.PMInstanceID);
			(CustomerPaymentMethod pmInstance, Customer customer) = this.FindCpmAndCustomer(payment.PMInstanceID);
			if (string.IsNullOrEmpty(authNbr))
			{
				throw new PXException(Messages.ERR_CCExternalAuthorizationNumberIsRequiredForCaptureOnlyTrans, authNbr);
			}
			CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(_repository.Graph, pmInstance.CCProcessingCenterID);
			if (procCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotValid, pmInstance.CCProcessingCenterID, 0);
			}
			if (!(procCenter.IsActive ?? false))
			{
				throw new PXException(Messages.ERR_CCProcessingCenterUsedForAuthIsNotActive, procCenter, 0);
			}
			CCProcTran tran = new CCProcTran();
			tran.Copy(payment);
			CopyL2DataToProcTran(payment, tran);
			tran.ProcessingCenterID = procCenter.ProcessingCenterID;
			return this.DoTransaction(CCTranType.CaptureOnly, tran, authNbr, customer.AcctCD);
		}

		public virtual TranOperationResult Void(ICCPayment payment, int? transactionId)
		{
			ExternalTransaction extTran = ExternalTransaction.PK.Find(_repository.Graph, transactionId);
			IsValidPmInstance(payment.PMInstanceID);

			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToVoidIsNotFound, transactionId);
			}
			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeVoided(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeVoided, transactionId);
			}
			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}

			(CCProcessingCenter procCenter, Customer customer) = GetProcCenterAndCustomer(payment, extTran);
			string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = payment.PMInstanceID,
				RefTranNbr = sTran.TranNbr,
				TransactionID = extTran.TransactionID,
				DocType = payment.DocType,
				RefNbr = payment.RefNbr,
				CuryID = sTran.CuryID,
				Amount = extTran.Amount,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr,
				ProcessingCenterID = procCenter.ProcessingCenterID
			};

			if (extTran.DocType != payment.DocType)
			{
				extTran.VoidDocType = tran.DocType;
				extTran.VoidRefNbr = tran.RefNbr;
			}

			return this.DoTransaction(CCTranType.Void, tran, extTran.TranNumber, customer.AcctCD, meansOfPayment);
		}

		public virtual TranOperationResult VoidOrCredit(ICCPayment payment, int? transactionId)
		{
			TranOperationResult result = this.Void(payment, transactionId);
			if (!result.Success)
			{
				ExternalTransaction extTran = ExternalTransaction.PK.Find(_repository.Graph, transactionId);
				CCProcTran tran = GetSuccessfulCCProcTran(extTran);
				if (MayBeCredited(tran))
					result = this.Credit(payment, transactionId, null, null);
			}
			return result;
		}

		public virtual TranOperationResult Credit(ICCPayment payment, string extRefTranNbr, string procCenterId)
		{
			IsValidPmInstance(payment.PMInstanceID);
			ExternalTransaction refTran = null;

			if (extRefTranNbr != null)
			{
				if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
				{
					refTran = _repository.FindCapturedExternalTransaction(procCenterId, extRefTranNbr);
				}
				else
				{
					refTran = _repository.FindCapturedExternalTransaction(payment.PMInstanceID, extRefTranNbr);
				}
			}

			if (refTran != null)
			{
				return Credit(payment, refTran.TransactionID.Value);
			}
			else
			{
				if (extRefTranNbr == null && payment.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
				{
					CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
					if (procCenter?.AllowUnlinkedRefund == false)
					{
						CustomerPaymentMethod cpm = CustomerPaymentMethod.PK.Find(_repository.Graph, payment.PMInstanceID);
						throw new PXException(Messages.ERR_ProcCenterNotSupportedUnlinkedRefunds, cpm.Descr, procCenterId);
					}
				}

				Customer customer = GetCustomer(payment);
				CCProcTran tran = new CCProcTran();
				tran.Copy(payment);
				tran.RefTranNbr = null;
				tran.RefPCTranNumber = extRefTranNbr;
				if (tran.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
				{
					tran.ProcessingCenterID = procCenterId;
				}
				string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

				return this.DoTransaction(CCTranType.Credit, tran, extRefTranNbr, customer.AcctCD, meansOfPayment);
			}
		}

		public virtual TranOperationResult Credit(ICCPayment payment, int? transactionId)
		{
			IsValidPmInstance(payment.PMInstanceID);

			ExternalTransaction extTran = ExternalTransaction.PK.Find(_repository.Graph, transactionId);
			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, transactionId);
			}

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeCredited(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, sTran.TranType);
			}

			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}

			(CCProcessingCenter procCenter, Customer customer) = GetProcCenterAndCustomer(payment, extTran);
			string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

			CCProcTran tran = new CCProcTran();
			tran.Copy(payment);
			tran.RefTranNbr = sTran.TranNbr;
			tran.ProcessingCenterID = procCenter.ProcessingCenterID;

			if (!payment.CuryDocBal.HasValue)
			{
				tran.CuryID = sTran.CuryID;
				tran.Amount = sTran.Amount;
			}
			return this.DoTransaction(CCTranType.Credit, tran, extTran.TranNumber, customer.AcctCD, meansOfPayment);
		}

		public virtual TranOperationResult Credit(ICCPayment payment, int? transactionId, string curyId, decimal? amount)
		{
			IsValidPmInstance(payment.PMInstanceID);
			ExternalTransaction extTran = ExternalTransaction.PK.Find(_repository.Graph, transactionId);

			if (extTran == null)
			{
				throw new PXException(Messages.ERR_CCTransactionToCreditIsNotFound, transactionId);
			}

			CCProcTran sTran = GetSuccessfulCCProcTran(extTran);
			if (!MayBeCredited(sTran))
			{
				throw new PXException(Messages.ERR_CCProcessingTransactionMayNotBeCredited, sTran.TranType);
			}

			if (!extTran.Active.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingReferensedTransactionNotAuthorized, transactionId);
			}

			(CCProcessingCenter procCenter, Customer customer) = GetProcCenterAndCustomer(payment, extTran);
			string meansOfPayment = _repository.GetPaymentMethod(payment.PaymentMethodID)?.PaymentType;

			CCProcTran tran = new CCProcTran
			{
				PMInstanceID = payment.PMInstanceID,
				DocType = payment.DocType,
				RefNbr = payment.RefNbr,
				OrigDocType = extTran.OrigDocType,
				OrigRefNbr = extTran.OrigRefNbr,
				ProcessingCenterID = procCenter.ProcessingCenterID,
				RefTranNbr = sTran.TranNbr
			};
			if (amount.HasValue)
			{
				tran.CuryID = curyId;
				tran.Amount = amount;
			}
			else
			{
				tran.CuryID = sTran.CuryID;
				tran.Amount = sTran.Amount;
			}
			return this.DoTransaction(CCTranType.Credit, tran, extTran.TranNumber, customer.AcctCD, meansOfPayment);
		}

		private void ValidateRecordTran(ICCPayment payment, TranRecordData recordData)
		{
			CCProcessingCenter procCenter = null;
			if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
			{
				procCenter = CCProcessingCenter.PK.Find(_repository.Graph, recordData.ProcessingCenterId);
			}
			else
			{
				procCenter = _repository.FindProcessingCenter(payment.PMInstanceID, payment.CuryID);
			}
			if (procCenter == null || string.IsNullOrEmpty(procCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}

			if (recordData.ProcessingCenterId != null && payment.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile
				&& recordData.ProcessingCenterId != procCenter.ProcessingCenterID)
			{
				CustomerPaymentMethod cpm = CustomerPaymentMethod.PK.Find(_repository.Graph, payment.PMInstanceID);
				throw new PXException(Messages.ERR_IncorrectProcessingCenterForPmInstance, recordData.ProcessingCenterId, cpm?.Descr);
			}

			CheckProcCenterCashAccountCury(procCenter, payment.CuryID);
		}

		private void ValidateRelativelyRefTran(ICCPayment payment, TranRecordData recordData, CCProcTran refProcTran, CCProcTran historyTran)
		{
			string docType = GetTrimValue(refProcTran.DocType);
			string refNbr = GetTrimValue(refProcTran.RefNbr);
			string origDocType = GetTrimValue(refProcTran.OrigDocType);
			string origRefNbr = GetTrimValue(refProcTran.OrigRefNbr);
			string secondDocType = null;
			string secondRefNbr = null;
			if (historyTran != null)
			{
				secondDocType = GetTrimValue(historyTran.DocType);
				secondRefNbr = GetTrimValue(historyTran.RefNbr);
			}

			string pDocType = GetTrimValue(payment.DocType);
			string pRefNbr = GetTrimValue(payment.RefNbr);
			string pOrigDocType = GetTrimValue(payment.OrigDocType);
			string pOrigRefNbr = GetTrimValue(payment.OrigRefNbr);

			if (((docType != pDocType || refNbr != pRefNbr) && (secondDocType != pDocType || secondRefNbr != pRefNbr))
				|| (pOrigDocType != null && (origDocType != pOrigDocType || origRefNbr != pOrigRefNbr))
				|| (payment.PMInstanceID != refProcTran.PMInstanceID && refProcTran.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile))
			{
				var prms = GetValidationHelperParamsObj(payment, recordData);
				throw new PXException(TranValidationHelper.GenerateTranAlreadyRecordedErrMsg(recordData.ExternalTranId, refProcTran, prms));
			}
		}

		private TranValidationHelper.AdditionalParams GetValidationHelperParamsObj(ICCPayment payment, TranRecordData recordData)
		{
			var prms = new TranValidationHelper.AdditionalParams();
			prms.PMInstanceId = payment.PMInstanceID;
			prms.ProcessingCenter = recordData.ProcessingCenterId;
			prms.Repo = Repository;
			return prms;
		}

		private string GetTrimValue(string input)
		{
			return input != null ? input.Trim() : null;
		}

		private CCProcTran FormatCCProcTran(ICCPayment payment, TranRecordData recordData)
		{
			var result = (PXResult<ExternalTransaction, CCProcTran>)SelectFrom<ExternalTransaction>.InnerJoin<CCProcTran>
				.On<ExternalTransaction.transactionID.IsEqual<CCProcTran.transactionID>>
				.Where<ExternalTransaction.noteID.IsEqual<@P.AsGuid>
					.And<CCProcTran.procStatus.IsEqual<CCProcStatus.opened>>.Or<@P.AsBool.IsEqual<True>>>
				.OrderBy<CCProcTran.tranNbr.Desc>
				.View.Select(_repository.Graph, recordData.TranUID, recordData.IsLocalValidation);

			CCProcTran tran = new CCProcTran();
			if (result != null)
			{
				tran = (CCProcTran)result;
			}

			tran.PMInstanceID = payment.PMInstanceID;
			tran.OrigDocType = payment.OrigDocType;
			tran.DocType = payment.DocType;
			tran.TranStatus = recordData.TranStatus;
			tran.AuthNumber = recordData.AuthCode;
			tran.CuryID = payment.CuryID;
			tran.Amount = recordData.Amount ?? payment.CuryDocBal;
			tran.CVVVerificationStatus = recordData.CvvVerificationCode;
			tran.StartTime = recordData.TransactionDate;
			tran.EndTime = recordData.TransactionDate;
			tran.Imported = recordData.Imported;
			tran.PCResponseReasonCode = recordData.ResponseCode;
			tran.PCResponseReasonText = recordData.ResponseText;
			tran.PCTranNumber = recordData.ExternalTranId;
			tran.SubtotalAmount = recordData.Subtotal ?? 0;
			tran.Tax = recordData.Tax ?? 0;

			if (!recordData.NewDoc)
			{
				tran.OrigRefNbr = payment.OrigRefNbr;
				tran.RefNbr = payment.RefNbr;
			}
			return tran;
		}

		private void CheckProcCenterCashAccountCury(CCProcessingCenter procCenter, string curyId)
		{
			CashAccount cashAccount = CashAccount.PK.Find(_repository.Graph, procCenter.CashAccountID);
			if (cashAccount.CuryID != curyId)
			{
				throw new PXException(Messages.ProcessingCenterCurrencyDoesNotMatch, curyId, cashAccount.CuryID);
			}
		}

		private CCPayLink GetPayLinkByExternalId(string externalId, string procCenter)
		{
			return PXSelect<CCPayLink, Where<CCPayLink.processingCenterID, Equal<Required<CCPayLink.processingCenterID>>,
					And<CCPayLink.externalID, Equal<Required<CCPayLink.externalID>>>>>.Select(_repository.Graph, procCenter, externalId);
		}

		private Customer GetCustomerFromDoc(ICCPayment payment)
		{
			Customer ret = null;
			bool isArDoc = ARDocType.Values.Any(i => payment.DocType == i);
			if (isArDoc)
			{
				ret = new PXSelectJoin<Customer, InnerJoin<ARRegister, On<ARRegister.customerID, Equal<Customer.bAccountID>>>,
					Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>(_repository.Graph).SelectSingle(payment.DocType, payment.RefNbr);
			}
			return ret;
		}

		private Customer GetCustomer(ICCPayment payment)
		{
			Customer customer = null;
			if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
			{
				customer = GetCustomerFromDoc(payment);
			}
			else
			{
				customer = this.FindCpmAndCustomer(payment.PMInstanceID).Item2;
			}
			return customer;
		}

		private Tuple<CCProcessingCenter, Customer> GetProcCenterAndCustomer(ICCPayment payment, ExternalTransaction transaction)
		{
			CCProcessingCenter procCenter = null;
			Customer customer = null;
			string procCenterStr = null;
			if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
			{
				CCProcTran ccProcTran = GetSuccessfulCCProcTran(transaction);
				customer = GetCustomerFromDoc(payment);
				procCenterStr = ccProcTran.ProcessingCenterID;
				procCenter = CCProcessingCenter.PK.Find(_repository.Graph, ccProcTran.ProcessingCenterID);
				if (procCenter == null)
				{
					throw new PXException(Messages.ERR_CCProcessingCenterUsedInReferencedTranNotFound, procCenterStr, transaction.TransactionID);
				}
			}
			else
			{
				var tuple = this.FindCpmAndCustomer(payment.PMInstanceID);
				customer = tuple.Item2;
				CustomerPaymentMethod cpm = tuple.Item1;
				procCenterStr = cpm.CCProcessingCenterID;
				procCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterStr);
				if (procCenter == null)
				{
					throw new PXException(Messages.ERR_CCProcessingCenterIsNotSpecified, cpm.Descr);
				}
			}
			if (!procCenter.IsActive.GetValueOrDefault())
			{
				throw new PXException(Messages.ERR_CCProcessingIsInactive, procCenterStr);
			}
			return new Tuple<CCProcessingCenter, Customer>(procCenter, customer);
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordAuthorization(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			tran.ExpirationDate = recordData.ExpirationDate;
			int? innerTranId = null;
			if (tran.TransactionID == null)
			{
				ExternalTransaction extTran = new ExternalTransaction();
				PopulateExtTranFromTranRecordObj(extTran, recordData);
				innerTranId = this.RecordTransaction(CCTranType.AuthorizeOnly, tran, extTran);
			}
			else
			{
				UpdateExtTranCardTypeInfo(tran, recordData);
				innerTranId = this.RecordTransaction(CCTranType.AuthorizeOnly, tran);
			}
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordCapture(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			tran.ExpirationDate = recordData.ExpirationDate;

			if (tran.TransactionID == null)
			{
				ExternalTransaction extTran = new ExternalTransaction();
				PopulateExtTranFromTranRecordObj(extTran, recordData);
				innerTranId = this.RecordTransaction(CCTranType.AuthorizeAndCapture, tran, extTran);
			}
			else
			{
				UpdateExtTranCardTypeInfo(tran, recordData);
				innerTranId = this.RecordTransaction(CCTranType.AuthorizeAndCapture, tran);
			}
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordPriorAuthorizedCapture(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			int? innerTranId = null;
			ExternalTransaction extTran;
			if (tran.TransactionID == null)
			{
				extTran = new ExternalTransaction();
			}
			else
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
			}

			PopulateExtTranFromTranRecordObj(extTran, recordData);
			innerTranId = this.RecordTransaction(CCTranType.PriorAuthorizedCapture, tran, extTran);

			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordVoid(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			int? innerTranId = null;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			ExternalTransaction extTran;
			if (tran.TransactionID == null)
			{
				extTran = new ExternalTransaction();
			}
			else
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
			}
			if (recordData.AllowFillVoidRef && tran.TransactionID != null)
			{
				if (extTran.VoidDocType != null && (extTran.VoidDocType != payment.DocType || extTran.VoidRefNbr != payment.RefNbr))
				{
					var prms = GetValidationHelperParamsObj(payment, recordData);
					var exception = TranValidationHelper.GenerateTranAlreadyRecordedErrMsg(extTran.TranNumber, extTran.VoidDocType, extTran.RefNbr, prms);
					throw new PXException(exception);
				}
				extTran.VoidDocType = payment.DocType;
				extTran.VoidRefNbr = payment.RefNbr;
			}
			PopulateExtTranFromTranRecordObj(extTran, recordData);
			innerTranId = this.RecordTransaction(CCTranType.Void, tran, extTran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordCaptureOnly(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;

			CCProcTran tran = PrepeareRecord(payment, recordData);
			int? innerTranId = this.RecordTransaction(CCTranType.CaptureOnly, tran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		public virtual bool RecordUnknown(ICCPayment payment, TranRecordData recordData)
		{
			bool ret = false;
			CCProcTran tran = PrepeareRecord(payment, recordData);
			int? innerTranId = null;

			ExternalTransaction extTran;
			if (tran.TransactionID == null)
			{
				extTran = new ExternalTransaction();
			}
			else
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
			}

			if (recordData.AllowFillVoidRef && tran.TransactionID != null)
			{
				if (extTran.VoidDocType != null && (extTran.VoidDocType != payment.DocType || extTran.VoidRefNbr != payment.RefNbr))
				{
					var prms = GetValidationHelperParamsObj(payment, recordData);
					var exception = TranValidationHelper.GenerateTranAlreadyRecordedErrMsg(extTran.TranNumber, extTran.VoidDocType, extTran.RefNbr, prms);
					throw new PXException(exception);
				}
				extTran.VoidDocType = payment.DocType;
				extTran.VoidRefNbr = payment.RefNbr;
			}

			PopulateExtTranFromTranRecordObj(extTran, recordData);
			innerTranId = this.RecordTransaction(CCTranType.Unknown, tran, extTran);
			if (innerTranId != null)
			{
				recordData.InnerTranId = innerTranId;
				ret = true;
			}
			return ret;
		}

		/// <summary>
		/// After successful operation the property <see cref="TranRecordData.InnerTranId" /> stores <see cref="ExternalTransaction.TransactionID" /> value. 
		/// This value is not used in the client code, but could be processed by customizations.
		/// </summary>
		public virtual bool RecordCredit(ICCPayment payment, TranRecordData recordData)
		{
			ExternalTransaction origExtTran = null;
			if (!string.IsNullOrEmpty(recordData.RefExternalTranId))
			{
				if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
				{
					origExtTran = _repository.FindCapturedExternalTransaction(recordData.ProcessingCenterId, recordData.RefExternalTranId);
				}
				else
				{
					origExtTran = _repository.FindCapturedExternalTransaction(payment.PMInstanceID, recordData.RefExternalTranId);
				}
			}

			string refPCTranNumber = null;
			int? refTranNbr = null;
			if (origExtTran != null && (payment.PMInstanceID == origExtTran.PMInstanceID))
			{
				refTranNbr = _repository.GetCCProcTranByTranID(origExtTran.TransactionID).Where(i => i.TranType == CCTranTypeCode.AuthorizeAndCapture
					|| i.TranType == CCTranTypeCode.PriorAuthorizedCapture).Select(i => i.TranNbr).FirstOrDefault();
			}
			else
			{
				if (!string.IsNullOrEmpty(recordData.RefExternalTranId))
				{
					refPCTranNumber = recordData.RefExternalTranId;
				}
			}
			CCProcTran tran = PrepeareRecord(payment, recordData);
			tran.TranType = CCTranTypeCode.Credit;
			tran.ProcStatus = CCProcStatus.Finalized;
			tran.RefPCTranNumber = refPCTranNumber;
			if (tran.RefTranNbr == null)
			{
				tran.RefTranNbr = refTranNbr;
			}
			ExternalTransaction extTran = null;
			if (tran.TransactionID == null)
			{
				extTran = new ExternalTransaction();
				extTran.NoteID = recordData.TranUID;
			}
			else
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
				if (extTran.ParentTranID == null && origExtTran != null)
				{
					extTran.ParentTranID = origExtTran.TransactionID;
				}
				extTran = _repository.UpdateExternalTransaction(extTran);
			}
			extTran.NeedSync = recordData.NeedSync;
			extTran.ExtProfileId = recordData.ExtProfileId;
			SetCardTypeValue(extTran, recordData.CardType, recordData.ProcCenterCardTypeCode);
			tran = _repository.InsertOrUpdateTransaction(tran, extTran);
			recordData.InnerTranId = tran.TranNbr;
			return true;
		}

		public virtual CCProcTran PrepeareRecord(ICCPayment payment, TranRecordData recordData)
		{
			if (recordData.ValidateDoc)
			{
				IsValidPmInstance(payment.PMInstanceID);
				ValidateRecordTran(payment, recordData);
			}
			CCProcessingCenter procCenter;
			if (payment.PMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
			{
				procCenter = CCProcessingCenter.PK.Find(_repository.Graph, recordData.ProcessingCenterId);
			}
			else
			{
				procCenter = _repository.FindProcessingCenter(payment.PMInstanceID, payment.CuryID);
			}
			CCProcTran tran = FormatCCProcTran(payment, recordData);
			tran.ProcessingCenterID = procCenter.ProcessingCenterID;

			if (recordData.ExternalTranId != null)
			{
				var query = new PXSelectJoin<ExternalTransaction,
					InnerJoin<CCProcTran, On<CCProcTran.transactionID, Equal<ExternalTransaction.transactionID>>>,
						Where<CCProcTran.pCTranNumber, IsNotNull,
						And<Not<ExternalTransaction.syncStatus, Equal<CCSyncStatusCode.error>,
							And<ExternalTransaction.active, Equal<False>>>>>,
					OrderBy<Desc<CCProcTran.tranNbr>>>(_repository.Graph);

				PXResultset<ExternalTransaction> sel;
				if (recordData.RefInnerTranId != null)
				{
					query.WhereAnd<Where<ExternalTransaction.transactionID, Equal<Required<ExternalTransaction.transactionID>>>>();
					sel = query.Select(recordData.RefInnerTranId);
				}
				else
				{
					query.WhereAnd<Where<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>>>();
					sel = query.Select(recordData.ExternalTranId);
				}

				var trans = sel.RowCast<CCProcTran>();
				var extTrans = sel.RowCast<ExternalTransaction>();

				CCProcTran sTran = null;
				if (tran.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
				{
					sTran = trans.Where(i => i.PMInstanceID == tran.PMInstanceID).FirstOrDefault();
				}
				if (sTran == null)
				{
					sTran = trans.Where(i => i.ProcessingCenterID == tran.ProcessingCenterID).FirstOrDefault();
				}
				if (sTran != null && sTran.TranNbr != tran.TranNbr)
				{
					if (!recordData.AllowFillVoidRef)
					{
						var historyTran = GetProcTranFromOtherDocInHistory(sTran, extTrans, trans);
						ValidateRelativelyRefTran(payment, recordData, sTran, historyTran);
					}
					tran.RefTranNbr = sTran?.TranNbr;
				}
			}

			if (tran.RefTranNbr != null)
			{
				CCProcTran refProcTran = CCProcTran.PK.Find(_repository.Graph, tran.RefTranNbr);
				tran.TransactionID = refProcTran.TransactionID;
				tran.OrigDocType = refProcTran.OrigDocType;
				tran.OrigRefNbr = refProcTran.OrigRefNbr;
			}
			return tran;
		}

		private void CheckProcessingCenter(CCProcessingCenter procCenter)
		{
			CCPluginTypeHelper.GetPluginTypeWithCheck(procCenter);
		}
		#endregion

		#region Public Static Functions
		public static bool MayBeVoided(CCProcTran aOrigTran)
		{
			switch (aOrigTran.TranType)
			{
				case CCTranTypeCode.Authorize:
				case CCTranTypeCode.AuthorizeAndCapture:
				case CCTranTypeCode.PriorAuthorizedCapture:
				case CCTranTypeCode.CaptureOnly:
					return true;
			}
			return false;
		}

		public static bool MayBeCredited(CCProcTran aOrigTran)
		{
			switch (aOrigTran.TranType)
			{
				case CCTranTypeCode.AuthorizeAndCapture:
				case CCTranTypeCode.PriorAuthorizedCapture:
				case CCTranTypeCode.CaptureOnly:
					return true;
			}
			return false;
		}

		public static bool IsExpired(CustomerPaymentMethod aPMInstance)
		{
			return (aPMInstance.ExpirationDate.HasValue && aPMInstance.ExpirationDate.Value < DateTime.Now);
		}

		protected static void FillRecordedTran(CCProcTran tran, string aReasonText = Messages.ImportedExternalCCTransaction)
		{
			tran.PCResponseReasonText = aReasonText;
			tran.TranNbr = null;
			if (tran.StartTime == null)
			{
				tran.StartTime = tran.EndTime = PXTimeZoneInfo.Now;
			}
			tran.ExpirationDate = null;
			tran.TranStatus = CCTranStatusCode.Approved;
			tran.ProcStatus = CCProcStatus.Finalized;
		}

		#endregion

		#region Internal Processing Functions
		protected virtual void ProcessAPIResponse(APIResponse apiResponse)
		{
			if (!apiResponse.isSucess && apiResponse.ErrorSource != CCError.CCErrorSource.None)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, string> kvp in apiResponse.Messages)
				{
					stringBuilder.Append(kvp.Key);
					stringBuilder.Append(": ");
					stringBuilder.Append(kvp.Value.Trim(' '));
					if (stringBuilder.Length > 0)
					{
						if (stringBuilder[stringBuilder.Length - 1] != '.')
						{
							stringBuilder.Append(".");
						}
						stringBuilder.Append(" ");
					}
				}
				throw new PXException(Messages.CardProcessingError, CCError.GetDescription(apiResponse.ErrorSource), stringBuilder.ToString().TrimEnd(' '));
			}
		}

		protected virtual TranOperationResult DoTransaction(CCTranType aTranType, CCProcTran aTran, string origRefNbr, string customerCd, string meansOfPayment = "")
		{
			TranOperationResult ret = new TranOperationResult();
			CCProcessingCenter procCenter = GetAndCheckProcessingCenterFromTransaction(aTran);
			aTran = StartCreditCardTransaction(aTranType, aTran, procCenter);
			ExternalTransaction externalTransaction = ExternalTransaction.PK.Find(_repository.Graph, aTran.TransactionID);

			bool cvvVerified = false;
			bool needCvvVerification = isCvvVerificationRequired(aTranType);
			if (needCvvVerification)
			{
				bool isStored;
				CCProcTran verifyTran = this.findCVVVerifyingTran(aTran.PMInstanceID, out isStored);
				if (verifyTran != null)
				{
					cvvVerified = true;
					if (!isStored)
						this.UpdateCvvVerificationStatus(verifyTran);
				}
				if (!cvvVerified)
					aTran.CVVVerificationStatus = CVVVerificationStatusCode.RequiredButNotVerified;
			}
			ret.TransactionId = aTran.TransactionID.Value;

			TranProcessingInput inputData = new TranProcessingInput();
			Copy(inputData, aTran);
			if (!string.IsNullOrEmpty(customerCd))
			{
				inputData.CustomerCD = customerCd;
			}
			if (!string.IsNullOrEmpty(origRefNbr))
			{
				inputData.OrigRefNbr = origRefNbr;
			}

			if (needCvvVerification)
			{
				inputData.VerifyCVV = !cvvVerified;
			}
			if (externalTransaction != null)
			{
				inputData.TranUID = externalTransaction.NoteID;
			}
			inputData.MeansOfPayment = meansOfPayment;

			CCProcessingContext context = new CCProcessingContext()
			{
				callerGraph = _repository.Graph,
				processingCenter = procCenter,
				aCustomerCD = customerCd,
				aPMInstanceID = inputData.PMInstanceID,
				aDocType = inputData.DocType,
				aRefNbr = inputData.DocRefNbr,
			};
			var procWrapper = GetProcessingWrapper(context);
			TranProcessingResult result = new TranProcessingResult();
			bool hasError = false;
			try
			{
				result = procWrapper.DoTransaction(aTranType, inputData);
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction. PCTranNumber:{result.PCTranNumber}; PCResponseCode:{result.PCResponseCode}; PCResponseReasonCode:{result.PCResponseReasonCode}; PCResponseReasonText:{result.PCResponseReasonText}; ErrorText:{result.ErrorText}");
			}
			catch (V2.CCProcessingException procException)
			{
				int? reasonCode = null;
				if (procException.Reason == V2.CCProcessingException.ExceptionReason.TranDeclined)
				{
					result.TranStatus = CCTranStatus.Declined;
					reasonCode = procException.ReasonCode;
					hasError = false;
				}
				else
				{
					hasError = true;
				}
				result.ErrorSource = CCError.CCErrorSource.ProcessingCenter;
				string errorMessage = string.Empty;
				if (procException.Message.Equals(procException.InnerException?.Message)
					|| procException.InnerException == null)
				{
					errorMessage = procException.Message;
				}
				else
				{
					errorMessage = procException.Message + "; " + procException?.InnerException?.Message;
				}
				if (reasonCode != null)
				{
					result.PCResponseReasonCode = reasonCode.ToString();
				}
				result.ErrorText = errorMessage;
				result.PCResponseReasonText += errorMessage;
				result.CardType = CardType.GetCardTypeEnumByCode(externalTransaction.CardType);
				result.ProcCenterCardTypeCode = externalTransaction.ProcCenterCardTypeCode;

				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction.V2.CCProcessingException. ErrorSource:{result.ErrorSource}; ErrorText:{result.ErrorText}");
			}
			catch (WebException webExn)
			{
				hasError = true;
				result.ErrorSource = CCError.CCErrorSource.Network;
				result.ErrorText = webExn.Message;
				PXTrace.WriteInformation($"CCPaymentProcessing.DoTransaction.WebException. ErrorSource:{result.ErrorSource}; ErrorText:{result.ErrorText}");
			}
			catch (Exception exn)
			{
				hasError = true;
				result.ErrorSource = CCError.CCErrorSource.Internal;
				result.ErrorText = exn.Message;
				throw new PXException(Messages.ERR_CCPaymentProcessingInternalError, aTran.TranNbr, exn.Message);
			}
			finally
			{
				CCProcTran tran = this.EndTransaction(aTran.TranNbr.Value, result, (hasError ? CCProcStatus.Error : CCProcStatus.Finalized));

				if (!hasError)
				{
					this.ProcessTranResult(tran, result);
				}
			}
			ret.Success = result.Success;
			return ret;
		}

		public virtual CCProcTran StartCreditCardTransaction(CCTranType aTranType, CCProcTran ccProcTran, CCProcessingCenter procCenter)
		{
			ccProcTran.ProcessingCenterID = procCenter.ProcessingCenterID;
			ccProcTran.TranType = CCTranTypeCode.GetTypeCode(aTranType);
			ccProcTran.ProcStatus = CCProcStatus.Opened;
			ccProcTran.CVVVerificationStatus = CVVVerificationStatusCode.SkippedDueToPriorVerification;
			ccProcTran.PCTranNumber = String.Empty;

			ccProcTran = this.StartTransaction(ccProcTran, procCenter.OpenTranTimeout);

			return ccProcTran;
		}

		public virtual CCProcessingCenter GetAndCheckProcessingCenterFromTransaction(CCProcTran ccProcTran)
		{
			CCProcessingCenter procCenter = null;
			if (ccProcTran.ProcessingCenterID != null)
			{
				procCenter = CCProcessingCenter.PK.Find(_repository.Graph, ccProcTran.ProcessingCenterID);
			}
			else
			{
				procCenter = _repository.FindProcessingCenter(ccProcTran.PMInstanceID, ccProcTran.CuryID);
			}

			CheckProcessingCenter(procCenter);

			if (procCenter == null || string.IsNullOrEmpty(procCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}

			CheckProcCenterCashAccountCury(procCenter, ccProcTran.CuryID);
			return procCenter;
		}
		public virtual void ShowAcceptPaymentForm(V2.CCTranType tranType, ICCPayment paymentDoc, string procCenterId, int? bAccountId, Guid? TranUID)
		{
			CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
			CheckProcCenterCashAccountCury(procCenter, paymentDoc.CuryID);
			var context = new CCProcessingContext();
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
			context.callerGraph = _repository.Graph;
			context.aRefNbr = paymentDoc.RefNbr;
			context.aDocType = paymentDoc.DocType;
			context.aCustomerID = bAccountId;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			HostedPaymnetFormProcessingWrapper = (p, w) => { return HostedFromProcessingWrapper.GetPaymentFormProcessingWrapper(p, w, context); };
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(procCenterId, provider);
			var generator = new V2ProcessingInputGenerator(provider) { FillCardData = false, FillCustomerData = true, FillAdressData = true };
			V2.ProcessingInput v2Input = generator.GetProcessingInput(tranType, paymentDoc);
			v2Input.TranUID = TranUID;
			CopyL2DataToProcessingInput(paymentDoc, v2Input);
			wrapper.GetPaymentForm(v2Input);
		}

		public virtual HostedFormResponse ParsePaymentFormResponse(string response, string procCenterId)
		{
			CCProcessingContext context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(procCenterId, provider);
			return wrapper.ParsePaymentFormResponse(response);
		}

		public virtual V2.TransactionData GetTransactionById(string transactionId, string processingCenterId)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			V2.TransactionData ret = wrapper.GetTransaction(transactionId);
			return ret;
		}

		public virtual IEnumerable<V2.TransactionData> GetTransactionsByBatch(string batchId, string processingCenterId)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			IEnumerable<V2.TransactionData> ret = wrapper.GetTransactionsByBatch(batchId);
			return ret;
		}

		public virtual IEnumerable<V2.TransactionData> GetUnsettledTransactions(string processingCenterId, V2.TransactionSearchParams searchParams = null)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			IEnumerable<V2.TransactionData> ret = wrapper.GetUnsettledTransactions(searchParams);
			return ret;
		}

		public virtual PXPluginRedirectOptions PreparePaymentForm(ICCPayment paymentDoc, string processingCenterId, int? bAccountId, bool saveCard, V2.CCTranType tranType, Guid? tranUid, string meansOfPayment)
		{
			var context = new CCProcessingContext();
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			context.callerGraph = _repository.Graph;
			context.aRefNbr = paymentDoc.RefNbr;
			context.aDocType = paymentDoc.DocType;
			context.aCustomerID = bAccountId;

			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(processingCenterId, provider);

			var generator = new V2ProcessingInputGenerator(provider) { FillCardData = false, FillCustomerData = false, FillAdressData = true };
			V2.PaymentFormPrepareOptions v2Input = generator.GetPaymentFormPrepareOptions(tranType, paymentDoc);
			v2Input.SaveCard = saveCard;
			v2Input.TranUID = tranUid;
			v2Input.MeansOfPayment = meansOfPayment == PaymentMethodType.EFT
				? V2.MeansOfPayment.EFT
				: V2.MeansOfPayment.CreditCard;
			CopyL2DataToProcessingInput(paymentDoc, v2Input);
			return wrapper.PreparePaymentForm(v2Input);
		}

		public virtual V2.PaymentFormResponseProcessResult ProcessPaymentFormResponse(ICCPayment paymentDoc, string processingCenterId, int? bAccountId, string response)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			context.aRefNbr = paymentDoc.RefNbr;
			context.aDocType = paymentDoc.DocType;
			context.aCustomerID = bAccountId;
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			IHostedPaymentFormProcessingWrapper wrapper = GetHostedPaymentFormProcessingWrapper(processingCenterId, provider);

			var generator = new V2ProcessingInputGenerator(provider) { FillCardData = false, FillCustomerData = true, FillAdressData = true };
			V2.PaymentFormPrepareOptions v2Input = generator.GetPaymentFormPrepareOptions(V2.CCTranType.AuthorizeOnly, paymentDoc);
			CopyL2DataToProcessingInput(paymentDoc, v2Input);
			return wrapper.ProcessPaymentFormResponse(v2Input, response);
		}

		public virtual IEnumerable<V2.BatchData> GetSettledBatches(string processingCenterId, V2.BatchSearchParams batchSearchParams)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, processingCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			IEnumerable<V2.BatchData> ret = wrapper.GetSettledBatches(batchSearchParams);
			return ret;
		}

		public virtual V2.TransactionData FindTransaction(Guid guid, string procCenterId)
		{
			var context = new CCProcessingContext();
			context.callerGraph = _repository.Graph;
			context.processingCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
			ICardTransactionProcessingWrapper wrapper = GetProcessingWrapper(context);
			V2.TransactionData tranData = wrapper.FindTransaction(new V2.TransactionSearchParams() { TransactionGuid = guid });
			return tranData;
		}

		public virtual void FinalizeTransaction(int? tranId, string message)
		{
			CCProcTran tran = CCProcTran.PK.Find(_repository.Graph, tranId);
			if (tran != null)
			{
				tran.ProcStatus = CCProcStatus.Finalized;
				tran.TranStatus = CCTranStatusCode.Error;
				tran.PCResponseReasonText = message;
				tran.ErrorText = Messages.ERR_CCProcessingCenterNotContainsTranWithID; ;
				_repository.InsertOrUpdateTransaction(tran);
			}
		}

		protected virtual CCProcTran StartTransaction(CCProcTran aTran, int? aAutoExpTimeout)
		{
			aTran.TranNbr = null;
			aTran.StartTime = PXTimeZoneInfo.Now;
			if (aAutoExpTimeout.HasValue)
			{
				aTran.ExpirationDate = aTran.StartTime.Value.AddSeconds(aAutoExpTimeout.Value);
			}
			aTran = _repository.InsertOrUpdateTransaction(aTran);
			return aTran;
		}

		protected virtual CCProcTran EndTransaction(int aTranID, TranProcessingResult aRes, string aProcStatus)
		{
			CCProcTran tran = CCProcTran.PK.Find(_repository.Graph, aTranID);
			Copy(tran, aRes);
			tran.ProcStatus = aProcStatus;
			tran.EndTime = PXTimeZoneInfo.Now;
			if (aRes.ExpireAfterDays.HasValue)
				tran.ExpirationDate = tran.EndTime.Value.AddDays(aRes.ExpireAfterDays.Value);
			else
				tran.ExpirationDate = null;

			var extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
			SetCardTypeValue(extTran, aRes.CardType, aRes.ProcCenterCardTypeCode);

			tran = _repository.UpdateTransaction(tran, extTran);
			this.UpdateCvvVerificationStatus(tran);
			return tran;
		}

		protected virtual void ProcessTranResult(CCProcTran aTran, TranProcessingResult aResult)
		{

		}

		protected virtual void CopyL2DataToProcTran(ICCPayment doc, CCProcTran procTran)
		{
			Payment payment = ((IBqlTable)doc).GetExtension<Payment>();
			if (payment != null)
			{
				procTran.Tax = payment.Tax;
				procTran.SubtotalAmount = payment.SubtotalAmount;
			}			
		}

		protected virtual void CopyL2DataToProcessingInput(ICCPayment doc, V2.ProcessingInput processingInput)
		{
			Payment payment = ((IBqlTable)doc).GetExtension<Payment>();
			if (payment != null)
			{
				processingInput.Tax = payment.Tax;
				processingInput.SubtotalAmount = payment.SubtotalAmount;
			}
		}

		protected virtual int? RecordTransaction(CCTranType tranType, CCProcTran tran)
		{
			ExternalTransaction extTran = new ExternalTransaction();
			if (tran.TransactionID.GetValueOrDefault() > 0)
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, tran.TransactionID);
			}
			int? ret = RecordTransaction(tranType, tran, extTran);
			return ret;
		}

		protected virtual int? RecordTransaction(CCTranType tranType, CCProcTran tran, ExternalTransaction extTran)
		{
			//Add later - use ProcessCenter to fill ExpirationDate
			//aTran.ProcessingCenterID = aProcCenter.ProcessingCenterID;
			tran.TranType = CCTranTypeCode.GetTypeCode(tranType);
			if (string.IsNullOrEmpty(tran.CVVVerificationStatus))
			{
				tran.CVVVerificationStatus = CVVVerificationStatusCode.SkippedDueToPriorVerification;
				bool cvvVerified = false;
				bool needCvvVerification = isCvvVerificationRequired(tranType);
				if (needCvvVerification)
				{
					bool isStored;
					CCProcTran verifyTran = null;
					if (tran.PMInstanceID != PaymentTranExtConstants.NewPaymentProfile)
					{
						verifyTran = this.findCVVVerifyingTran(tran.PMInstanceID, out isStored);
					}
					if (verifyTran != null)
					{
						cvvVerified = true;
					}
					if (!cvvVerified)
						tran.CVVVerificationStatus = CVVVerificationStatusCode.RequiredButNotVerified;
				}
			}

			if (tran.StartTime == null)
			{
				tran.StartTime = tran.EndTime = PXTimeZoneInfo.Now;
			}

			if (tranType != CCTranType.AuthorizeOnly && !(tranType == CCTranType.AuthorizeAndCapture
				&& tran.TranStatus == CCTranStatusCode.HeldForReview))
			{
				tran.ExpirationDate = null;
			}
			if (tran.TranStatus == null)
			{
				tran.TranStatus = CCTranStatusCode.Approved;
			}
			tran.ProcStatus = CCProcStatus.Finalized;
			tran = _repository.InsertOrUpdateTransaction(tran, extTran);
			return tran.TransactionID;
		}

		private CCProcTran GetProcTranFromOtherDocInHistory(CCProcTran sTran, IEnumerable<ExternalTransaction> extTrans, IEnumerable<CCProcTran> procTrans)
		{
			string targetDocType = null;
			string targetRefNbr = null;
			CCProcTran secondTran = null;
			var sExtTran = extTrans.Where(i => i.TransactionID == sTran.TransactionID).FirstOrDefault();
			if (sTran.DocType == sExtTran.DocType && sTran.RefNbr == sExtTran.RefNbr && sExtTran.VoidDocType != null)
			{
				targetDocType = sExtTran.VoidDocType;
				targetRefNbr = sExtTran.VoidRefNbr;
			}
			else if (sTran.DocType == sExtTran.VoidDocType && sTran.RefNbr == sExtTran.VoidRefNbr)
			{
				targetDocType = sExtTran.DocType;
				targetRefNbr = sExtTran.RefNbr;
			}

			if (targetDocType != null)
			{
				secondTran = procTrans.Where(i => i.TransactionID == sExtTran.TransactionID
					&& i.DocType == targetDocType && i.RefNbr == targetRefNbr).FirstOrDefault();
			}
			return secondTran;
		}

		private void PopulateExtTranFromTranRecordObj(ExternalTransaction extTran, TranRecordData tranRecordData)
		{
			extTran.SaveProfile = tranRecordData.CreateProfile;
			extTran.NeedSync = tranRecordData.NeedSync;
			extTran.ExtProfileId = tranRecordData.ExtProfileId;
			
			SetCardTypeValue(extTran, tranRecordData.CardType, tranRecordData.ProcCenterCardTypeCode);

			if (tranRecordData.TranUID != null)
			{
				extTran.NoteID = tranRecordData.TranUID;
			}

			if (tranRecordData.PayLinkExternalId != null && extTran.PayLinkID == null)
			{
				var payLink = GetPayLinkByExternalId(tranRecordData.PayLinkExternalId, tranRecordData.ProcessingCenterId);
				if (payLink != null)
				{
					extTran.PayLinkID = payLink.PayLinkID;
				}
			}
		}

		private void UpdateExtTranCardTypeInfo(CCProcTran ccTran, TranRecordData recordData)
		{
			ExternalTransaction extTran = new ExternalTransaction();
			if (ccTran.TransactionID.GetValueOrDefault() > 0)
			{
				extTran = ExternalTransaction.PK.Find(_repository.Graph, ccTran.TransactionID);
			}
			SetCardTypeValue(extTran, recordData.CardType, recordData.ProcCenterCardTypeCode);
		}

		private void SetCardTypeValue(ExternalTransaction extTran, CCCardType cardType, string crocCenterCardTypeCode)
		{
			extTran.CardType = CardType.GetCardTypeCode(cardType);
			extTran.ProcCenterCardTypeCode = extTran.CardType == CardType.OtherCode ? crocCenterCardTypeCode : null;
		}
		#endregion

		#region Internal Reading Functions

		protected static bool isCvvVerificationRequired(CCTranType aType)
		{
			return (aType == CCTranType.AuthorizeOnly || aType == CCTranType.AuthorizeAndCapture || aType == CCTranType.Unknown);
		}

		protected virtual CCProcTran findCVVVerifyingTran(int? aPMInstanceID, out bool aIsStored)
		{
			CustomerPaymentMethod pmInstance = CustomerPaymentMethod.PK.Find(_repository.Graph, aPMInstanceID);
			if (pmInstance.CVVVerifyTran.HasValue)
			{
				aIsStored = true;
				return CCProcTran.PK.Find(_repository.Graph, pmInstance.CVVVerifyTran);
			}
			else
			{
				aIsStored = false;
				CCProcTran verifyingTran = _repository.FindVerifyingCCProcTran(aPMInstanceID);
				return verifyingTran;
			}
		}

		protected virtual Tuple<CustomerPaymentMethod, Customer> FindCpmAndCustomer(int? aPMInstanceID)
		{
			PXResult<CustomerPaymentMethod, Customer> res = _repository.FindCustomerAndPaymentMethod(aPMInstanceID);
			if (res != null)
			{
				CustomerPaymentMethod cpm = (CustomerPaymentMethod)res;
				Customer customer = (Customer)res;
				return new Tuple<CustomerPaymentMethod, Customer>(cpm, customer);
			}
			if (aPMInstanceID == PaymentTranExtConstants.NewPaymentProfile)
			{
				throw new PXException(Messages.CreditCardIsNotDefined);
			}
			else
			{
				throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, aPMInstanceID);
			}
		}

		protected virtual void UpdateCvvVerificationStatus(CCProcTran aTran)
		{
			if (aTran.TranStatus == CCTranStatusCode.Approved &&
				aTran.CVVVerificationStatus == CVVVerificationStatusCode.Matched &&
				(aTran.TranType == CCTranTypeCode.AuthorizeAndCapture || aTran.TranType == CCTranTypeCode.Authorize))
			{
				CustomerPaymentMethod pmInstance = CustomerPaymentMethod.PK.Find(_repository.Graph, aTran.PMInstanceID);
				if (!pmInstance.CVVVerifyTran.HasValue)
				{
					pmInstance.CVVVerifyTran = aTran.TranNbr;
					CustomerPaymentMethodDetail cvvDetail = _repository.GetCustomerPaymentMethodDetail(aTran.PMInstanceID, CreditCardAttributes.CVV);
					if (cvvDetail != null)
						_repository.DeletePaymentMethodDetail(cvvDetail);
					_repository.UpdateCustomerPaymentMethod(pmInstance);
					_repository.Save();
				}
			}
		}

		protected object GetProcessor(CCProcessingCenter processingCenter)
		{
			if (processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}

			object processor = null;

			try
			{
				processor = CCPluginTypeHelper.CreatePluginInstance(processingCenter);

			}
			catch (PXException)
			{
				throw;
			}
			catch
			{
				throw new PXException(Messages.ERR_ProcessingCenterTypeInstanceCreationFailed,
					processingCenter.ProcessingTypeName,
					processingCenter.ProcessingCenterID);
			}
			return processor;
		}

		protected ICardTransactionProcessingWrapper GetProcessingWrapper(CCProcessingContext context)
		{
			object processor = GetProcessor(context.processingCenter);
			return _transactionProcessingWrapper(processor, context);
		}

		protected IHostedPaymentFormProcessingWrapper GetHostedPaymentFormProcessingWrapper(string procCenterId, ICardProcessingReadersProvider provider)
		{
			CCProcessingCenter procCenter = CCProcessingCenter.PK.Find(_repository.Graph, procCenterId);
			object processor = GetProcessor(procCenter);

			return HostedPaymnetFormProcessingWrapper(processor, provider);
		}

		protected virtual void IsValidPmInstance(int? pmInstanceId)
		{
			if (pmInstanceId != PaymentTranExtConstants.NewPaymentProfile)
			{
				(CustomerPaymentMethod pmInstance, Customer customer) = this.FindCpmAndCustomer(pmInstanceId);
				if (pmInstance == null)
				{
					throw new PXException(Messages.CreditCardWithID_0_IsNotDefined, pmInstanceId);
				}
				if (pmInstance != null && pmInstance.IsActive == false)
				{
					throw new PXException(Messages.InactiveCreditCardMayNotBeProcessed, pmInstance.Descr);
				}
			}
		}

		private CCProcTran GetSuccessfulCCProcTran(ExternalTransaction extTran)
		{
			var procTran = ExternalTranHelper.LastSuccessfulCCProcTran(extTran, _repository) as CCProcTran;
			if (procTran == null)
			{
				throw new Exception("Could not get CCProcTran record by TransactionId.");
			}
			return procTran;
		}
		#endregion

		#region  Utility Funcions

		public static void Copy(TranProcessingInput aDst, CCProcTran aSrc)
		{
			aDst.TranID = aSrc.TranNbr.Value;
			aDst.PMInstanceID = aSrc.PMInstanceID;
			bool useOrigDoc = string.IsNullOrEmpty(aSrc.DocType);
			aDst.DocType = useOrigDoc ? aSrc.OrigDocType : aSrc.DocType;
			aDst.DocRefNbr = useOrigDoc ? aSrc.OrigRefNbr : aSrc.RefNbr;
			aDst.Amount = aSrc.Amount.Value;
			aDst.CuryID = aSrc.CuryID;
			aDst.SubtotalAmount = aSrc.SubtotalAmount;
			aDst.Tax = aSrc.Tax;
		}

		public static void Copy(CCProcTran aDst, TranProcessingResult aSrc)
		{
			aDst.PCTranNumber = aSrc.PCTranNumber;
			aDst.PCResponseCode = aSrc.PCResponseCode;
			aDst.PCResponseReasonCode = aSrc.PCResponseReasonCode;
			aDst.AuthNumber = aSrc.AuthorizationNbr;
			aDst.PCResponse = TrimIfLonger(aSrc.PCResponse, 2048);
			aDst.PCResponseReasonText = TrimIfLonger(aSrc.PCResponseReasonText, 512);
			aDst.CVVVerificationStatus = CVVVerificationStatusCode.GetCCVCode(aSrc.CcvVerificatonStatus);
			aDst.TranStatus = CCTranStatusCode.GetCode(aSrc.TranStatus);
			if (aSrc.ErrorSource != CCError.CCErrorSource.None)
			{
				aDst.ErrorSource = CCError.GetCode(aSrc.ErrorSource);
				aDst.ErrorText = TrimIfLonger(aSrc.ErrorText, 255);
				if (aSrc.ErrorSource != CCError.CCErrorSource.ProcessingCenter)
				{
					aDst.PCResponseReasonText = TrimIfLonger(aSrc.ErrorText, 512);
				}
			}
		}

		private static string TrimIfLonger(string source, int length)
		{
			if (source?.Length > length)
			{
				return source.Substring(0, length);
			}
			return source;
		}

		internal CCProcTran GetCCProcTran(ExternalTransaction externalTransaction) => GetSuccessfulCCProcTran(externalTransaction);

		#endregion
	}
}


