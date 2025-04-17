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

using PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.Repositories;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCCustomerInformationManagerGraph : PXGraph<CCCustomerInformationManagerGraph>
	{
		public virtual void GetOrCreatePaymentProfile(PXGraph graph, ICCPaymentProfileAdapter paymentProfileAdapter, ICCPaymentProfileDetailAdapter profileDetailAdapter)
		{
			CCCustomerInformationManager.GetOrCreatePaymentProfile(graph, paymentProfileAdapter, profileDetailAdapter);
		}

		public virtual void GetCreatePaymentProfileForm(PXGraph graph, ICCPaymentProfileAdapter ccPaymentProfileAdapter)
		{
			CCCustomerInformationManager.GetCreatePaymentProfileForm(graph, ccPaymentProfileAdapter);
		}

		public virtual void GetManagePaymentProfileForm(PXGraph graph, ICCPaymentProfile paymentProfile)
		{
			CCCustomerInformationManager.GetManagePaymentProfileForm(graph,paymentProfile);
		}

		public virtual void GetNewPaymentProfiles(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.GetNewPaymentProfiles(graph, payment, paymentDetail);
		}

		public virtual void GetPaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.GetPaymentProfile(graph, payment, paymentDetail);
		}

		public virtual PXResultset<CustomerPaymentMethodDetail> GetAllCustomersCardsInProcCenter(PXGraph graph, int? BAccountID, string CCProcessingCenterID)
		{
			return CCCustomerInformationManager.GetAllCustomersCardsInProcCenter(graph, BAccountID, CCProcessingCenterID);
		}

		public virtual void DeletePaymentProfile(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail)
		{
			CCCustomerInformationManager.DeletePaymentProfile(graph, payment, paymentDetail);
		}

		public virtual TranProfile GetOrCreatePaymentProfileByTran(PXGraph graph, ICCPaymentProfileAdapter payment, string tranId)
		{
			return CCCustomerInformationManager.GetOrCreatePaymentProfileByTran(graph, payment, tranId);
		}

		public virtual void ProcessProfileFormResponse(PXGraph graph, ICCPaymentProfileAdapter payment, ICCPaymentProfileDetailAdapter paymentDetail, string response)
		{
			CCCustomerInformationManager.ProcessProfileFormResponse(graph, payment, paymentDetail, response);
		}
	}
}
