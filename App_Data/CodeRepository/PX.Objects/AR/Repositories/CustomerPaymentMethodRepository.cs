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
using PX.Objects.AR;
using PX.Objects.CA;
using System.Collections.Generic;

namespace PX.Objects.AR.Repositories
{
	public class CustomerPaymentMethodRepository
	{
		protected readonly PXGraph _graph;

		public CustomerPaymentMethodRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CustomerPaymentMethod UpdateCustomerPaymentMethod(CustomerPaymentMethod paymentMethod)
		{
			return (CustomerPaymentMethod)_graph.Caches[typeof(CustomerPaymentMethod)].Update(paymentMethod);
		}

		public PXResult<CustomerPaymentMethod, Customer> FindCustomerAndPaymentMethod(int? pMInstanceID)
		{
			return (PXResult<CustomerPaymentMethod, Customer>)PXSelectJoin<CustomerPaymentMethod,
				InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>.Select(_graph, pMInstanceID);
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(string procCenter, string customerProfile, string paymentProfile)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
					And<CustomerPaymentMethod.customerCCPID, Equal<Required<CustomerPaymentMethod.customerCCPID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>,
				OrderBy<Desc<CustomerPaymentMethod.pMInstanceID>>>(_graph);

			PXResultset<CustomerPaymentMethod> result = query.Select(procCenter, customerProfile);
			foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in result)
			{
				CustomerPaymentMethodDetail det = (CustomerPaymentMethodDetail)item;
				if (det.Value == paymentProfile)
				{
					return new Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail>(item, det);
				}
			}
			return null;
		}

		public CustomerProcessingCenterID GetCustomerProcessingCenterByAccountAndProcCenterIDs(int? bAccountId, string procCenterId)
		{
			CustomerProcessingCenterID custProcCenterID = PXSelect<CustomerProcessingCenterID,
				Where<CustomerProcessingCenterID.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
				And<CustomerProcessingCenterID.cCProcessingCenterID, Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>>>,
			OrderBy<Desc<CustomerProcessingCenterID.createdDateTime>>>.Select(_graph, bAccountId, procCenterId);
			return custProcCenterID;
		}

		public Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail> GetCustomerPaymentMethodWithProfileDetail(int? pmInstanceId)
		{
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID, Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
					And<PaymentMethodDetail.isCCProcessingID, Equal<True>, And<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForARCards>>>>>(_graph);
			PXResultset<CustomerPaymentMethod> result = query.Select(pmInstanceId);
			foreach (PXResult<CustomerPaymentMethod, CustomerPaymentMethodDetail> item in result)
			{
				CustomerPaymentMethodDetail det = (CustomerPaymentMethodDetail)item;
				return new Tuple<CustomerPaymentMethod, CustomerPaymentMethodDetail>(item, det);
			}
			return null;
		}
	}
}
