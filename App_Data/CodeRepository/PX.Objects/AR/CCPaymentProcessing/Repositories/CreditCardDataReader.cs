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
using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class CreditCardDataReader : ICreditCardDataReader
	{
		private PXGraph _graph;
		private int? _pminstanceID;

		public CreditCardDataReader(PXGraph graph, int? pminstanceID)
		{
			if (graph == null)
			{
				throw new ArgumentNullException(nameof(graph));
			}
		
			_graph = graph;
			_pminstanceID = pminstanceID;
		}

		void ICreditCardDataReader.ReadData(Dictionary<string, string> aData)
		{
			PXResultset<CustomerPaymentMethodDetail> PMDselect = null;

			PMDselect = PXSelectJoin<CustomerPaymentMethodDetail,
					InnerJoin<PaymentMethodDetail, On<CustomerPaymentMethodDetail.detailID, Equal<PaymentMethodDetail.detailID>,
						And<CustomerPaymentMethodDetail.paymentMethodID, Equal<PaymentMethodDetail.paymentMethodID>>>>,
					Where<CustomerPaymentMethodDetail.pMInstanceID, Equal<Required<CustomerPaymentMethodDetail.pMInstanceID>>>>
				.Select(_graph, _pminstanceID);

			if (PMDselect != null)
			{
				foreach (PXResult<CustomerPaymentMethodDetail, PaymentMethodDetail> dets in PMDselect)
				{
					PaymentMethodDetail pmd = dets;
					CustomerPaymentMethodDetail cpmd = dets;
					if (pmd.IsCCProcessingID == true)
					{
						aData[CreditCardAttributes.CCPID] = cpmd.Value;
					}
					if (pmd.IsIdentifier == true)
					{
						aData[CreditCardAttributes.CardNumber] = cpmd.Value;
					}
					if (pmd.IsExpirationDate == true)
					{
						aData[CreditCardAttributes.ExpirationDate] = cpmd.Value;
					}
					if (pmd.IsCVV == true)
					{
						aData[CreditCardAttributes.CVV] = cpmd.Value;
					}
					if (pmd.IsOwnerName == true)
					{
						aData[CreditCardAttributes.NameOnCard] = cpmd.Value;
					}
				}
			}
		}

		string ICreditCardDataReader.Key_CardNumber => CreditCardAttributes.CardNumber;

		string ICreditCardDataReader.Key_CardExpiryDate => CreditCardAttributes.ExpirationDate;

		string ICreditCardDataReader.Key_CardCVV => CreditCardAttributes.CVV;

		string ICreditCardDataReader.Key_PMCCProcessingID => CreditCardAttributes.CCPID;
	}
}
