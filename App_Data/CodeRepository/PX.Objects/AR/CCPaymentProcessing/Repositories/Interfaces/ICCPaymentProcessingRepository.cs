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
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CC;
using System.Collections.Generic;
namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public interface ICCPaymentProcessingRepository
	{
		PXGraph Graph { get; }
		CCProcTran InsertCCProcTran(CCProcTran transaction);
		CCProcTran UpdateCCProcTran(CCProcTran transaction);
		CCProcTran InsertOrUpdateTransaction(CCProcTran procTran);
		CCProcTran InsertOrUpdateTransaction(CCProcTran procTran, ExternalTransaction extTran);
		CCProcTran InsertTransaction(CCProcTran procTran, ExternalTransaction extTran);
		CCProcTran UpdateTransaction(CCProcTran procTran, ExternalTransaction extTran);
		ExternalTransaction UpdateExternalTransaction(ExternalTransaction extTran);
		IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId);
		ExternalTransaction FindCapturedExternalTransaction(int? pMInstanceID, string refTranNbr);
		ExternalTransaction FindCapturedExternalTransaction(string procCenterId, string tranNbr);
		IEnumerable<ExternalTransaction> GetExternalTransactionsByPayLinkID(int? payLinkId);
		Tuple<ExternalTransaction, ARPayment> GetExternalTransactionWithPayment(string tranNbr, string procCenterId);
		Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(string procCenter, string custProfileId, string paymentProfileId);
		Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(int? pmInstanceId);
		CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod);
		CustomerPaymentMethodDetail GetCustomerPaymentMethodDetail(int? pMInstanceId, string detailID);
		CustomerProcessingCenterID GetCustomerProcessingCenterByAccountAndProcCenterIDs(int? baccountId, string procCenterId);
		CCProcessingCenterBranch GetProcessingCenterBranchByBranchAndProcCenterIDs(int? branchId, string procCenterId);
		void DeletePaymentMethodDetail(CustomerPaymentMethodDetail detail);
		CCProcTran FindVerifyingCCProcTran(int? pMInstanceID);
		CCProcessingCenter GetCCProcessingCenter(string processingCenterID);
		CCProcessingCenter FindProcessingCenter(int? pMInstanceID, string aCuryId);
		CCProcessingCenter GetProcessingCenterByID(string procCenterId);
		PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID);
		PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID);
		PaymentMethod GetPaymentMethod(string paymentMehtod);
		void Save();
	}
}
