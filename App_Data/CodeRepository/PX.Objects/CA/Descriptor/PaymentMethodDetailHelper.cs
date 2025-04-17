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
using PX.Objects.AP;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CA
{
	public class PaymentMethodDetailHelper
	{
		public static void VendorDetailValueFieldSelecting(PXGraph graph, Events.FieldSelecting<VendorPaymentMethodDetail.detailValue> e)
		{
			var row = (VendorPaymentMethodDetail)e.Row;

			if (row == null)
			{
				return;
			}

			PaymentMethodDetail paymentMethodDetail = PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Required<VendorPaymentMethodDetail.paymentMethodID>>,
								And<PaymentMethodDetail.detailID, Equal<Required<VendorPaymentMethodDetail.detailID>>,
									And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
												Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>.Select(graph, row.PaymentMethodID, row.DetailID);

			if (paymentMethodDetail == null)
			{
				return;
			}

			switch ((PaymentMethodDetailType?)paymentMethodDetail.ControlType)
			{
				case PaymentMethodDetailType.Text:
					return;

				case PaymentMethodDetailType.AccountType:
					List<string> allowedValues = new List<string>();
					List<string> allowedLabels = new List<string>();

					allowedValues.Add(((int)ACHPlugInBase.TransactionCode.CheckingAccount).ToString());
					allowedLabels.Add(CA.Messages.CheckingAccount);
					allowedValues.Add(((int)ACHPlugInBase.TransactionCode.SavingAccount).ToString());
					allowedLabels.Add(CA.Messages.SavingAccount);

					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 10,
						true, "DetailValue", false, -1, null, allowedValues.ToArray(), allowedLabels.ToArray(),
						true, allowedValues.First());

					return;
			}
		}
	}
}
