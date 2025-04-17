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

using PX.Data;
using System;
using PX.Objects.CA;
using PX.Objects.AR.Repositories;
using PX.Objects.CA.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Collections.Generic;
using PX.Common;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CC;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CCPaymentProcessingRepository : ICCPaymentProcessingRepository
	{
		public PXGraph Graph
		{
			get; private set;
		}

		public bool NeedPersist { get; set; } = true;
		public bool KeepNewTranDeactivated { get; set; }
		private CCProcTranRepository _cctranRepository;
		private ExternalTransactionRepository _externalTran;
		private CustomerPaymentMethodRepository _cpmRepository;
		private CustomerPaymentMethodDetailRepository _cpmDetailRepository;
		private CCProcessingCenterRepository _processingCenterRepository;
		private CCProcessingCenterDetailRepository _processingCenterDetailRepository;

		public PXSelect<CCProcTran> CCProcTrans;
		public PXSelect<CustomerPaymentMethod> CustomerPaymentMethods;
		public PXSelect<CustomerPaymentMethodDetail> CustomerPaymentMethodDetails;

		public CCPaymentProcessingRepository(PXGraph graph)
		{
			Graph = graph ?? throw new ArgumentNullException(nameof(graph));
			InitializeRepositories(graph);
		}

		public static ICCPaymentProcessingRepository GetCCPaymentProcessingRepository()
		{
			CCPaymentHelperGraph newGraph = PXGraph.CreateInstance<CCPaymentHelperGraph>();
			CCPaymentProcessingRepository repository = new CCPaymentProcessingRepository(newGraph);
			return repository;
		}

		public CustomerProcessingCenterID GetCustomerProcessingCenterByAccountAndProcCenterIDs(int? bAccountId, string procCenterId)
		{
			return _cpmRepository.GetCustomerProcessingCenterByAccountAndProcCenterIDs(bAccountId, procCenterId);
		}

		public CCProcessingCenterBranch GetProcessingCenterBranchByBranchAndProcCenterIDs(int? branchId, string procCenterId)
		{
			return _processingCenterRepository.GetProcessingCenterBranchByBranchAndProcCenterIDs(branchId, procCenterId);
		}

		public CCProcTran GetCCProcTran(int? tranID)
		{
			return CCProcTran.PK.Find(Graph, tranID);
		}

		public CCProcTran InsertCCProcTran(CCProcTran transaction)
		{
			return _cctranRepository.InsertCCProcTran(transaction);
		}


		public CCProcTran InsertOrUpdateTransaction(CCProcTran procTran)
		{
			int extTranId = 0;
			ExternalTransaction extTransaction = null;

			if (procTran == null)
			{
				throw new ArgumentNullException(nameof(CCProcTran));
			}
			if (procTran.TransactionID.GetValueOrDefault() != 0)
			{
				extTranId = procTran.TransactionID.Value;
			}
			if (extTranId == 0)
			{
				extTransaction = new ExternalTransaction();
				procTran = InsertOrUpdateTransaction(procTran, extTransaction);
			}
			else
			{
				extTransaction = ExternalTransaction.PK.Find(Graph, procTran.TransactionID);
				if (extTransaction == null)
				{
					throw new Exception($"Could not find External transaction record by TransactionID = {procTran.TransactionID}");
				}
				procTran = InsertOrUpdateTransaction(procTran, extTransaction);
			}
			return procTran;
		}

		public CCProcTran InsertOrUpdateTransaction(CCProcTran procTran, ExternalTransaction extTransaction)
		{
			if (procTran == null)
			{
				throw new ArgumentNullException(nameof(CCProcTran));
			}
			if (extTransaction == null)
			{
				throw new ArgumentNullException(nameof(ExternalTransaction));
			}
			if (procTran.TransactionID != extTransaction.TransactionID)
			{
				throw new Exception($"External transaction record does not match CCProcTran.");
			}

			int extTranId = 0;
			if (procTran.TransactionID.GetValueOrDefault() != 0)
			{
				extTranId = procTran.TransactionID.Value;
			}

			if (extTranId == 0)
			{
				procTran = InsertTransaction(procTran, extTransaction);
			}
			else
			{
				procTran = UpdateTransaction(procTran, extTransaction);
			}
			return procTran;
		}

		public CCProcTran InsertTransaction(CCProcTran procTran, ExternalTransaction extTran)
		{
			if (procTran == null)
			{
				throw new ArgumentNullException(nameof(CCProcTran));
			}
			if (extTran == null)
			{
				throw new ArgumentNullException(nameof(ExternalTransaction));
			}
			using (var scope = new PXTransactionScope())
			{
				extTran.DocType = procTran.DocType;
				extTran.RefNbr = procTran.RefNbr;
				extTran.OrigDocType = procTran.OrigDocType;
				extTran.OrigRefNbr = procTran.OrigRefNbr;
				extTran.Amount = procTran.Amount;
				extTran.Direction = ExternalTransaction.TransactionDirection.Debet;
				extTran.PMInstanceID = procTran.PMInstanceID;
				extTran.ProcessingCenterID = procTran.ProcessingCenterID;
				extTran.CVVVerification = procTran.CVVVerificationStatus;
				extTran.ExpirationDate = procTran.ExpirationDate;
				if (procTran.TranType == CCTranTypeCode.Credit)
				{
					extTran.Direction = ExternalTransaction.TransactionDirection.Credit;
					if (procTran.RefTranNbr != null)
					{
						CCProcTran refProcTran = CCProcTran.PK.Find(Graph, procTran.RefTranNbr);
						extTran.ParentTranID = refProcTran.TransactionID;
					}
				}
				if (procTran.ProcStatus == CCProcStatus.Opened)
				{
					extTran.ProcStatus = ExtTransactionProcStatusCode.Unknown;
				}
				else
				{
					extTran.ProcStatus = ExtTransactionProcStatusCode.GetStatusByTranStatusTranType(procTran.TranStatus, procTran.TranType);
					extTran.Active = KeepNewTranDeactivated ? false : CCProcTranHelper.IsActiveTran(procTran);
					extTran.Completed = CCProcTranHelper.IsCompletedTran(procTran);
					extTran.TranNumber = procTran.PCTranNumber;
					extTran.AuthNumber = procTran.AuthNumber;
					extTran.ExpirationDate = procTran.ExpirationDate;
					extTran.LastActivityDate = procTran.EndTime;
				}
				extTran = _externalTran.InsertExternalTransaction(extTran);
				UpdateExternalTransactionForReAuth(extTran);
				procTran.TransactionID = extTran.TransactionID;
				procTran = _cctranRepository.InsertCCProcTran(procTran);
				if (NeedPersist)
				{
					Save();
				}
				scope.Complete();
				return procTran;
			}
		}

		public CCProcTran UpdateTransaction(CCProcTran procTran, ExternalTransaction extTran)
		{
			if (procTran == null)
			{
				throw new ArgumentNullException(nameof(CCProcTran));
			}
			if (extTran == null)
			{
				throw new ArgumentNullException(nameof(ExternalTransaction));
			}
			if (procTran.TransactionID.GetValueOrDefault() == 0)
			{
				throw new ArgumentNullException(nameof(procTran.TransactionID));
			}
			using (var scope = new PXTransactionScope())
			{
				if (procTran.ProcStatus != CCProcStatus.Opened)
				{
					if (procTran.AuthNumber != null)
					{
						extTran.AuthNumber = procTran.AuthNumber;
					}
					if (procTran.CVVVerificationStatus != null)
					{
						extTran.CVVVerification = procTran.CVVVerificationStatus;
					}
					if (procTran.PCTranNumber != null)
					{
						extTran.TranNumber = procTran.PCTranNumber;
					}
					if (procTran.Amount != null)
					{
						extTran.Amount = procTran.Amount;
					}
					if (procTran.TranType == CCTranTypeCode.Credit)
					{
						extTran.Direction = ExternalTransaction.TransactionDirection.Credit;
					}
					extTran.PMInstanceID = procTran.PMInstanceID;
					extTran.ProcessingCenterID = procTran.ProcessingCenterID;
					extTran.ProcStatus = ExtTransactionProcStatusCode.GetStatusByTranStatusTranType(procTran.TranStatus, procTran.TranType);
					extTran.LastActivityDate = procTran.EndTime;
					if (extTran.NeedSync == false)
					{
						extTran.Active = CCProcTranHelper.IsActiveTran(procTran);
					}
					else
					{
						extTran.SyncStatus = CCSyncStatusCode.None;
					}
					extTran.Completed = CCProcTranHelper.IsCompletedTran(procTran);
					if ((procTran.ProcStatus == CCProcStatus.Error && extTran.Active == false)
						|| !CCProcTranHelper.IsFailedTran(procTran))
					{
						extTran.ExpirationDate = procTran.ExpirationDate;
					}
					UpdateExternalTransactionForReAuth(extTran);
					_externalTran.UpdateExternalTransaction(extTran);
				}
				procTran = _cctranRepository.UpdateCCProcTran(procTran);
				if (NeedPersist)
				{
					Save();
				}
				scope.Complete();
				return procTran;
			}
		}

		private void UpdateExternalTransactionForReAuth(ExternalTransaction extTran)
		{
			var processingCenterPmntMethod = GetProcessingCenterPmntMethod(extTran);
			switch (extTran.ProcStatus)
			{
				case ExtTransactionProcStatusCode.AuthorizeSuccess:
					if (processingCenterPmntMethod?.FundHoldPeriod != null)
					{
						extTran.FundHoldExpDate =
							extTran.LastActivityDate?.AddDays(processingCenterPmntMethod.FundHoldPeriod.Value);
					}
					break;
				case ExtTransactionProcStatusCode.AuthorizeFail:
				case ExtTransactionProcStatusCode.AuthorizeDecline:
				case ExtTransactionProcStatusCode.VoidSuccess:
				case ExtTransactionProcStatusCode.CaptureSuccess:
					extTran.FundHoldExpDate = null;
					break;
			}
		}

		public ExternalTransaction UpdateExternalTransaction(ExternalTransaction extTran)
		{
			return _externalTran.UpdateExternalTransaction(extTran);
		}

		public CCProcTran UpdateCCProcTran(CCProcTran transaction)
		{
			return _cctranRepository.UpdateCCProcTran(transaction);
		}

		public CustomerPaymentMethod GetCustomerPaymentMethod(int? pMInstanceId)
		{
			return CustomerPaymentMethod.PK.Find(Graph, pMInstanceId);
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(string procCenter, string custProfileId, string paymentProfileId)
		{
			return _cpmRepository.GetCustomerPaymentMethodWithProfileDetail(procCenter, custProfileId, paymentProfileId);
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(int? pmInstanceId)
		{
			return _cpmRepository.GetCustomerPaymentMethodWithProfileDetail(pmInstanceId);
		}

		public CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod)
		{
			return _cpmRepository.UpdateCustomerPaymentMethod(paymentMethod);
		}

		public CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID)
		{
			return _cpmDetailRepository.GetCustomerPaymentMethodDetail(pMInstanceId, detailID);			
		}

		public void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail)
		{
			_cpmDetailRepository.DeletePaymentMethodDetail(detail);
		}

		private CCProcessingCenterPmntMethod GetProcessingCenterPmntMethod(ExternalTransaction extTran)
		{
			var query = new SelectFrom<ARPayment>
				.InnerJoin<PaymentMethod>
					.On<ARPayment.paymentMethodID.IsEqual<PaymentMethod.paymentMethodID>>
				.InnerJoin<CCProcessingCenterPmntMethod>
					.On<CCProcessingCenterPmntMethod.paymentMethodID.IsEqual<PaymentMethod.paymentMethodID>>
				.Where<ARPayment.docType.IsEqual<@P.AsString>
					.And<ARPayment.refNbr.IsEqual<@P.AsString>>
					.And<CCProcessingCenterPmntMethod.processingCenterID.IsEqual<P.AsString>>>
				.View(Graph);
			
			var result = query.Select(extTran.DocType, extTran.RefNbr, extTran.ProcessingCenterID);
			
			foreach (PXResult<ARPayment, PaymentMethod, CCProcessingCenterPmntMethod> item in result)
			{
				var processingCenterPmntMethod = (CCProcessingCenterPmntMethod) item;
				return processingCenterPmntMethod;
			}
			return null;
		}
		public void Save()
		{
			var action = Graph.Actions["Save"];
			if (action != null)
			{
				action.Press();
			}
			else
			{
				Graph.Actions.PressSave();
			}
		}

		public CCProcessingCenter GetCCProcessingCenter(string processingCenterID)
		{
			return CCProcessingCenter.PK.Find(Graph, processingCenterID);
		}

		public CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId)
		{
			return _processingCenterRepository.FindProcessingCenter(pMInstanceID, aCuryId);
		}

		public CCProcTran FindVerifyingCCProcTran(int? pMInstanceID)
		{
			return _cctranRepository.FindVerifyingCCProcTran(pMInstanceID);
		}

		public PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID)
		{
			return _cpmRepository.FindCustomerAndPaymentMethod(pMInstanceID);
		}

		public PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID)
		{
			return _processingCenterDetailRepository.FindAllProcessingCenterDetails(processingCenterID);
		}
		private void InitializeRepositories(PXGraph graph)
		{
			_cctranRepository = new CCProcTranRepository(graph);
			_cpmRepository = new CustomerPaymentMethodRepository(graph);
			_cpmDetailRepository = new CustomerPaymentMethodDetailRepository(graph);
			_processingCenterRepository = new CCProcessingCenterRepository(graph);
			_processingCenterDetailRepository = new CCProcessingCenterDetailRepository(graph);
			_externalTran = new ExternalTransactionRepository(graph);
		}

		public IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId)
		{
			return _cctranRepository.GetCCProcTranByTranID(transactionId);
		}

		public ExternalTransaction FindCapturedExternalTransaction(int? pMInstanceID, string refTranNbr)
		{
			return _externalTran.FindCapturedExternalTransaction(pMInstanceID, refTranNbr);
		}

		public IEnumerable<ExternalTransaction> GetExternalTransactionsByPayLinkID(int? payLinkId)
		{
			return _externalTran.GetExternalTransactionsByPayLinkID(payLinkId);
		}

		public ExternalTransaction FindCapturedExternalTransaction(string procCenterId, string tranNbr)
		{
			return _externalTran.FindCapturedExternalTransaction(procCenterId, tranNbr);
		}

		public Tuple<ExternalTransaction, ARPayment> GetExternalTransactionWithPayment(string tranNbr, string procCenterId)
		{
			return _externalTran.GetExternalTransactionWithPayment(tranNbr, procCenterId);
		}

		public PaymentMethod GetPaymentMethod(string paymentMehtodID)
		{
			return PaymentMethod.PK.Find(Graph, paymentMehtodID);
		}

		public CCProcessingCenter GetProcessingCenterByID(string procCenterId)
		{
			return _processingCenterRepository.GetProcessingCenterByID(procCenterId);
		}
	}

	public class CCPaymentHelperGraph : PXGraph<CCPaymentHelperGraph>
	{
		public PXSelect<ExternalTransaction> ExternalTrans;
		public PXSelect<CCProcTran> CCProcTrans;
		public PXSelect<PaymentMethod> PaymentMethod;
		public PXSelect<PaymentMethodDetail> PaymentMethodDet;
		public PXSelect<CustomerPaymentMethod> CustomerPaymentMethods;
		public PXSelect<CustomerProcessingCenterID> CustomerProcessingCenterID;
		public PXSelect<CustomerPaymentMethodDetail> CustomerPaymentMethodDetails;
		public PXSelect<CCPayLink> PayLink;

		[InjectDependency]
		public ICCDisplayMaskService CCDisplayMaskService { get; set; }

		protected virtual void CustomerPaymentMethodDetail_Value_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CustomerPaymentMethodDetail row = e.Row as CustomerPaymentMethodDetail;
			PaymentMethodDetail def = this.FindTemplate(row);
			if (def != null)
			{
				if (def.IsIdentifier ?? false)
				{
					string id = CustomerPaymentMethodMaint.IDObfuscator.GetMaskByID(this, row.Value, def.DisplayMask, CustomerPaymentMethodDetails.Current.PMInstanceID);

					if (this.CustomerPaymentMethods.Current.Descr != id)
					{
						CustomerPaymentMethod parent = this.CustomerPaymentMethods.Current;
						parent.Descr =  String.Format("{0}:{1}", parent.PaymentMethodID, id);
						parent.Descr = CustomerPaymentMethodMaint.FormatDescription(parent.CardType ?? CardType.OtherCode, id);
						this.CustomerPaymentMethods.Update(parent);
					}
				}
				if (def.IsExpirationDate ?? false)
				{
					CustomerPaymentMethod parent = this.CustomerPaymentMethods.Current;
					CustomerPaymentMethods.Cache.SetValueExt<CustomerPaymentMethod.expirationDate>(parent, CustomerPaymentMethodMaint.ParseExpiryDate(this, parent, row.Value));
					this.CustomerPaymentMethods.Update(parent);
				}
			}
		}

		protected virtual PaymentMethodDetail FindTemplate(CustomerPaymentMethodDetail aDet)
		{
			PaymentMethodDetail res = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<PaymentMethodDetail.paymentMethodID>>,
						And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>,
						And<PaymentMethodDetail.detailID, Equal<Required<PaymentMethodDetail.detailID>>>>>>.Select(this, aDet.PaymentMethodID, aDet.DetailID);
			return res;
		}
	}
}
