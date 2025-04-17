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
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.Localizations.GB.BACS.AP
{
	/// <summary>
	/// The graph extension that adds a check when creating a batch from the processing form for the BACS Lloyds payment method.
	/// </summary>
	public class APPrintChecksExt : PXGraphExtension<APPrintChecks>
	{
		#region IsActive
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.uKLocalization>();
		}
		#endregion

		[PXOverride]
		public virtual void PrintPayments(List<APPayment> list, PrintChecksFilter filter,
			PaymentMethod paymentMethod, Action<List<APPayment>, PrintChecksFilter, PaymentMethod> del)
		{
			if (paymentMethod.DirectDepositFileFormat == LloydsDirectDepositType.Code)
			{
				DateTime? paymentDate = null;
				foreach (var payment in list)
				{
					if (paymentDate == null)
					{
						paymentDate = payment.DocDate;
					}
					else
					{
						if (paymentDate != payment.DocDate)
						{
							string fieldName =
								PXUIFieldAttribute.GetDisplayName<APPayment.docDate>(Base.APPaymentList.Cache);
							throw new PXException(Messages.BACSLloyds.DifferentBatchPaymentDates, fieldName);
						}
					}
				}
			}

			del(list, filter, paymentMethod);
		}
	}
}
