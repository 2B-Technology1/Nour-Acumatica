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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;

namespace PX.Objects.AR.CCPaymentProcessing
{
	public class CCTransactionsProcessor : ICCTransactionsProcessor
	{
		private ICCPaymentProcessor _processingClass;

		protected CCTransactionsProcessor(ICCPaymentProcessor processingClass)
		{
			_processingClass = processingClass;
		}

		public static ICCTransactionsProcessor GetCCTransactionsProcessor()
		{
			return new CCTransactionsProcessor(CCPaymentProcessing.GetCCPaymentProcessing());
		}

		public void ProcessAuthorize(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Authorize(doc, false); });
		}

		public void ProcessAuthorizeCapture(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Authorize(doc, true); });
		}

		public void ProcessPriorAuthorizedCapture(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Capture(doc, tran.TransactionID); });
		}

		public void ProcessVoid(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.Void(doc, tran.TransactionID); });
		}

		public void ProcessVoidOrCredit(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.VoidOrCredit(doc, tran.TransactionID); });
		}

		public void ProcessCredit(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => {
				TranOperationResult opRes = null;
				if (tran.TransactionID.HasValue)
				{
					opRes = _processingClass.Credit(doc, tran.TransactionID.Value);
				}
				else
				{
					opRes = _processingClass.Credit(doc, tran.TranNumber, tran.ProcessingCenterID);
				}
				return opRes;
			});
		}

		public void ProcessCaptureOnly(ICCPayment doc, IExternalTransaction tran)
		{
			CheckInput(doc, tran);
			Process(() => { return _processingClass.CaptureOnly(doc, tran.AuthNumber); });
		}

		private void CheckInput(ICCPayment doc, IExternalTransaction tran)
		{
			if (doc == null)
			{
				throw new ArgumentNullException(nameof(doc));
			}
		}

		private void Process(Func<TranOperationResult> func)
		{
			TranOperationResult res = func();
			if (!res.Success)
			{
				throw new PXException(Messages.ERR_CCTransactionProcessingFailed, res.TransactionId);
			}
		}
	}
}
