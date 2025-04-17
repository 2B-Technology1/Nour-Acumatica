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
using PX.Objects.GL;
using PX.Objects.CM;
using System;
using CMExt = PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.CC.PaymentProcessing.Helpers;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Common;

namespace PX.Objects.CC.GraphExtensions
{
	public class ARReleaseProcessPayLink : PXGraphExtension<ARReleaseProcess>
	{

		const string slotKey = "ARReleaseProcessDoNotSyncFlag";

		public delegate Tuple<ARAdjust, CMExt.CurrencyInfo> ProcessAdjustmentsDelegate(JournalEntry je, PXResultset<ARAdjust> adjustments, ARRegister paymentRegister, ARPayment payment,
			Customer paymentCustomer, CurrencyInfo new_info, CMExt.Currency paycury);

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		public static void ActivateDoNotSyncFlag()
		{
			PXContext.SetSlot(slotKey, true);
		}

		public static void ClearDoNotSyncFlag()
		{
			PXContext.ClearSlot(slotKey);
		}

		[PXOverride]
		public virtual Tuple<ARAdjust, CMExt.CurrencyInfo> ProcessAdjustments(JournalEntry je, PXResultset<ARAdjust> adjustments, ARRegister paymentRegister,
			ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, CMExt.Currency paycury, ProcessAdjustmentsDelegate baseMethod)
		{
			var ret = baseMethod(je, adjustments, paymentRegister, payment, paymentCustomer, new_info, paycury);

			foreach (PXResult<ARAdjust, CMExt.CurrencyInfo, CMExt.Currency, ARRegister, ARInvoice, ARPayment, ARTran> item in adjustments)
			{
				ARInvoice invoice = item;
				var invoiceExt = Base.Caches[typeof(ARInvoice)].GetExtension<ARInvoicePayLink>(invoice);
				if (invoiceExt.PayLinkID != null)
				{
					CCPayLink link = PXSelect<CCPayLink, Where<CCPayLink.payLinkID, Equal<Required<CCPayLink.payLinkID>>>>.Select(Base, invoiceExt.PayLinkID);
					if (link.Amount != invoice.CuryDocBal && link.NeedSync == false)
					{
						ARAdjust adjust = item;
						SetSyncFlagIfNeeded(payment, adjust, link);
					}
				}
			}
			return ret;
		}

		public delegate void PersistDelegate();
		[PXOverride]
		public virtual void Persist(PersistDelegate baseMethod)
		{
			var arDoc = Base.ARDocument.Current;
			var soAdjust = Base.soAdjust.Current;
			var soOrder = Base.soOrder.Current;

			if (soAdjust != null && arDoc != null && soOrder != null
				&& soAdjust.AdjgRefNbr == arDoc.RefNbr && soAdjust.AdjdOrderNbr == soOrder.OrderNbr)
			{
				var orderExt = Base.Caches[typeof(SO.SOOrder)].GetExtension<SOOrderPayLink>(soOrder);
				if (orderExt.PayLinkID != null)
				{
					CCPayLink link = PXSelect<CCPayLink, Where<CCPayLink.payLinkID, Equal<Required<CCPayLink.payLinkID>>>>.Select(Base, orderExt.PayLinkID);
					if (link.Amount != soOrder.CuryUnpaidBalance && link.NeedSync == false)
					{
						ARPayment arPayment = Base.ARPayment_DocType_RefNbr.Select(arDoc.DocType, arDoc.RefNbr, arDoc.CustomerID);
						SetSyncFlagIfNeeded(arPayment, link);
					}
				}
			}

			Base.Caches[typeof(CCPayLink)].Persist(PXDBOperation.Update);
			baseMethod();
		}

		private void SetSyncFlagIfNeeded(ARPayment payment, CCPayLink payLink)
		{
			if (!PayLinkHelper.PayLinkOpen(payLink)) return;
			
			if (payment?.CCActualExternalTransactionID != null)
			{
				var extTranExists = new PXSelect<ExternalTransaction,
					Where<ExternalTransaction.transactionID, Equal<Required<ExternalTransaction.transactionID>>,
					And<ExternalTransaction.payLinkID, Equal<Required<ExternalTransaction.payLinkID>>>>>(Base)
					.Any(payment.CCActualExternalTransactionID, payLink.PayLinkID);

				if (!extTranExists)
				{
					payLink.NeedSync = true;
					Base.Caches[typeof(CCPayLink)].Update(payLink);
				}
			}
			else
			{
				payLink.NeedSync = true;
				Base.Caches[typeof(CCPayLink)].Update(payLink);
			}
		}

		private void SetSyncFlagIfNeeded(ARPayment payment, ARAdjust adjust, CCPayLink payLink)
		{
			var doNotSyncFlag = GetDoNotSyncFlag();
			if (doNotSyncFlag) return;

			if (!PayLinkHelper.PayLinkOpen(payLink)) return;

			var cashDiscAmt = adjust.CuryAdjdPPDAmt;
			var woAmt = adjust.CuryAdjdWOAmt;

			if (cashDiscAmt != 0 || woAmt != 0)
			{
				payLink.NeedSync = true;
				Base.Caches[typeof(CCPayLink)].Update(payLink);
			}
			else
			{
				SetSyncFlagIfNeeded(payment, payLink);
			}
		}

		private static bool GetDoNotSyncFlag()
		{
			return PXContext.GetSlot<bool>(slotKey);
		}
	}
}
