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
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
	public class Correction : ARAdjustCorrectionExtension<ARPaymentEntry>
	{
		public delegate void CreatePaymentDelegate(ARInvoice ardoc, CurrencyInfo info, DateTime? paymentDate, string aFinPeriod, bool overrideDesc);
		[PXOverride]
		public virtual void CreatePayment(ARInvoice ardoc, CurrencyInfo info, DateTime? paymentDate, string aFinPeriod, bool overrideDesc,
			CreatePaymentDelegate baseMethod)
		{
			if (ardoc.IsUnderCorrection == true)
			{
				throw new PXException(Messages.CantCreateApplicationToInvoiceUnderCorrection, ardoc.RefNbr);
			}

			baseMethod(ardoc, info, paymentDate, aFinPeriod, overrideDesc);
		}

		[PXOverride]
		public virtual void ReverseApplicationProc(ARAdjust application, ARPayment payment, Action<ARAdjust, ARPayment> baseMethod)
		{
			if (payment?.IsCancellation == true)
			{
				throw new PXException(Messages.CantReverseCancellationApplication, application.AdjgRefNbr, application.AdjdRefNbr);
			}

			baseMethod(application, payment);
		}
	}
}
