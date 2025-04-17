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
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.CM.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.Common.Abstractions;
using PX.Objects.IN;

namespace PX.Objects.CN.JointChecks
{ 
	public class APReleaseProcessJointCheck : PXGraphExtension<APReleaseProcess>
	{
		public PXSelectJoin<JointPayeePayment,
			InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>,
			InnerJoin<APInvoice, On<APInvoice.docType, Equal<JointPayeePayment.invoiceDocType>,
				And<APInvoice.refNbr, Equal<JointPayeePayment.invoiceRefNbr>>>,
			InnerJoin<CM.CurrencyInfo, On<CM.CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>>>,
			Where<JointPayeePayment.paymentDocType, Equal<Current<APPayment.docType>>,
			And<JointPayeePayment.paymentRefNbr, Equal<Current<APPayment.refNbr>>,
			And<JointPayeePayment.isVoided, NotEqual<True>>>>> JointPayment;

		public PXSelect<JointPayee> JointPayees;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.construction>();
		}

		public delegate List<APRegister> ReleaseDocProcDelegate(
			JournalEntry journalEntry, APRegister document, bool isPreBooking, out List<INRegister> documents);

		[PXOverride]
		public virtual List<APRegister> ReleaseDocProc(JournalEntry journalEntry, APRegister document,
			bool isPreBooking, out List<INRegister> documents, ReleaseDocProcDelegate baseHandler)
		{
			ValidateInvoice(document);
			return baseHandler(journalEntry, document, isPreBooking, out documents);
		}

		private void ValidateInvoice(IDocumentKey document)
		{
			if (document.DocType != APDocType.Invoice) return; //Other type of documents cannot have IsJointPayees == true for now

			APInvoice invoice = APInvoice.PK.Find(Base, document.DocType, document.RefNbr);

			if (invoice.IsJointPayees == true)
			{
				//Negative amount is not allowed If Payment By line
				if (invoice.PaymentsByLinesAllowed == true
					&& Base.APTran_TranType_RefNbr.Select(invoice.DocType, invoice.RefNbr).RowCast<APTran>().Any(_ => _.CuryLineAmt < 0m))
				{
					throw new PXException(JointCheckMessages.NegativeLinesAreNotAllowed);
				}
			}
		}

		[PXOverride]
		public virtual void ProcessPayment(JournalEntry je, APRegister doc,
			PXResult<APPayment, CurrencyInfo, Currency, Vendor, CashAccount> res,
			Action<JournalEntry, APRegister, PXResult<APPayment, CurrencyInfo, Currency, Vendor, CashAccount>> baseMethod)
		{
			baseMethod(je, doc, res);

			Dictionary<int, decimal> amountPaidByPayee = new Dictionary<int, decimal>();
			foreach (PXResult<JointPayeePayment, JointPayee, APInvoice, CM.CurrencyInfo> rs in JointPayment.Select())
			{
				JointPayeePayment payment = (JointPayeePayment)rs;
				JointPayee payee = (JointPayee)rs;
				APInvoice bill = (APInvoice)rs;
				CM.CurrencyInfo curyInfo = (CM.CurrencyInfo) rs;

				decimal curyval;
				if (bill.CuryID == doc.CuryID)
				{
					curyval = payment.CuryJointAmountToPay.GetValueOrDefault();
				}
				else
				{
					CM.PXDBCurrencyAttribute.CuryConvCury(Base.Caches[typeof(APInvoice)], curyInfo, payment.JointAmountToPay.GetValueOrDefault(), out curyval);
				}

				if (amountPaidByPayee.ContainsKey(payee.JointPayeeId.Value))
				{
					amountPaidByPayee[payee.JointPayeeId.Value] += curyval;
				}
				else
				{
					amountPaidByPayee.Add(payee.JointPayeeId.Value, curyval);
				}
			}

			foreach(KeyValuePair<int, decimal> kv in amountPaidByPayee)
			{
				AppendAmountPaid(kv.Key, kv.Value);
			}

			if (doc.DocType == APDocType.Prepayment)
				ProcessPrepaymentWithJointBills(doc);

			if (doc.DocType == APDocType.VoidCheck)
				ProcessVoidCheck(doc);
		}

		private void AppendAmountPaid(int? jointPayeeID, decimal curyJointAmountPaid)
		{
			JointPayee payee = JointPayee.PK.Find(Base, jointPayeeID);
			if (payee != null)
			{
				payee.CuryJointAmountPaid = payee.CuryJointAmountPaid.GetValueOrDefault() + curyJointAmountPaid;
				JointPayees.Update(payee);
			}
		}

		protected virtual void ProcessPrepaymentWithJointBills(APRegister doc)
		{
			var select = new PXSelectJoin<APAdjust,
				InnerJoin<APInvoice,
					On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
					And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				InnerJoin<CM.CurrencyInfo, On<CM.CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
				InnerJoin<JointPayee, On<JointPayee.aPDocType, Equal<APAdjust.adjdDocType>,
					And<JointPayee.aPDocType, Equal<APAdjust.adjdDocType>,
					And<JointPayee.aPRefNbr, Equal<APAdjust.adjdRefNbr>,
					And<JointPayee.aPLineNbr, Equal<APAdjust.adjdLineNbr>,
					And<JointPayee.isMainPayee, Equal<True>>>>>>>>>,
				Where<APInvoice.isJointPayees, Equal<True>,
				And<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
				And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>>>>>(Base);

			Dictionary<int, decimal> amountPaidByPayee = new Dictionary<int, decimal>();
			foreach (PXResult<APAdjust, APInvoice, CM.CurrencyInfo, JointPayee> res in select.Select(doc.DocType, doc.RefNbr))
			{
				APAdjust adjust = (APAdjust)res;
				JointPayee jp = (JointPayee)res;
				APInvoice bill = (APInvoice)res;
				CM.CurrencyInfo curyInfo = (CM.CurrencyInfo)res;

				JointPayeePayment payment = JointPayment.Insert();
				payment.JointPayeeId = jp.JointPayeeId;
				payment.InvoiceDocType = adjust.AdjdDocType;
				payment.InvoiceRefNbr = adjust.AdjdRefNbr;
				payment.InvoiceRefNbr = adjust.AdjdRefNbr;
				payment.PaymentDocType = adjust.AdjgDocType;
				payment.PaymentRefNbr = adjust.AdjgRefNbr;
				payment.AdjustmentNumber = adjust.AdjdLineNbr.GetValueOrDefault();

				decimal curyval;
				if (bill.CuryID == doc.CuryID)
				{
					curyval = adjust.CuryAdjdAmt.GetValueOrDefault();
				}
				else
				{
					CM.PXDBCurrencyAttribute.CuryConvCury(Base.Caches[typeof(APInvoice)], curyInfo, adjust.AdjAmt.GetValueOrDefault(), out curyval);
				}
				payment.CuryJointAmountToPay = curyval;

				if (amountPaidByPayee.ContainsKey(jp.JointPayeeId.Value))
				{
					amountPaidByPayee[jp.JointPayeeId.Value] += curyval;
				}
				else
				{
					amountPaidByPayee.Add(jp.JointPayeeId.Value, curyval);
				}
			}

			foreach (KeyValuePair<int, decimal> kv in amountPaidByPayee)
			{
				AppendAmountPaid(kv.Key, kv.Value);
			}
		}

		protected virtual void ProcessVoidCheck(APRegister doc)
		{
			var select = new PXSelectJoin<APAdjust,
				InnerJoin<APInvoice,
					On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
					And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>,
				InnerJoin<CM.CurrencyInfo, On<CM.CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
				InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<APAdjust.jointPayeeID>>>>>,
				Where<APInvoice.isJointPayees, Equal<True>,
				And<APAdjust.adjgDocType, Equal<Required<APAdjust.adjgDocType>>,
				And<APAdjust.adjgRefNbr, Equal<Required<APAdjust.adjgRefNbr>>>>>>(Base);

			Dictionary<int, decimal> amountPaidByPayee = new Dictionary<int, decimal>();
			foreach (PXResult<APAdjust, APInvoice, CM.CurrencyInfo, JointPayee> res in select.Select(doc.DocType, doc.RefNbr))
			{
				APAdjust adjust = (APAdjust)res;
				JointPayee jp = (JointPayee)res;
				APInvoice bill = (APInvoice)res;
				CM.CurrencyInfo curyInfo = (CM.CurrencyInfo)res;

				JointPayeePayment payment = JointPayment.Insert();
				payment.JointPayeeId = jp.JointPayeeId;
				payment.InvoiceDocType = adjust.AdjdDocType;
				payment.InvoiceRefNbr = adjust.AdjdRefNbr;
				payment.InvoiceRefNbr = adjust.AdjdRefNbr;
				payment.PaymentDocType = adjust.AdjgDocType;
				payment.PaymentRefNbr = adjust.AdjgRefNbr;
				payment.AdjustmentNumber = adjust.AdjdLineNbr.GetValueOrDefault();

				decimal curyval;
				if (bill.CuryID == doc.CuryID)
				{
					curyval = adjust.CuryAdjdAmt.GetValueOrDefault();
				}
				else
				{
					CM.PXDBCurrencyAttribute.CuryConvCury(Base.Caches[typeof(APInvoice)], curyInfo, adjust.AdjAmt.GetValueOrDefault(), out curyval);
				}
				payment.CuryJointAmountToPay = curyval;

				if (amountPaidByPayee.ContainsKey(jp.JointPayeeId.Value))
				{
					amountPaidByPayee[jp.JointPayeeId.Value] += curyval;
				}
				else
				{
					amountPaidByPayee.Add(jp.JointPayeeId.Value, curyval);
				}
			}

			foreach (KeyValuePair<int, decimal> kv in amountPaidByPayee)
			{
				AppendAmountPaid(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// Extends <see cref="APReleaseProcess.ExtensionsPersist"/> 
		/// </summary>
		[PXOverride]
		public virtual void ExtensionsPersist()
		{
			JointPayment.Cache.Persist(PXDBOperation.Insert);
			JointPayment.Cache.Persist(PXDBOperation.Update);
			JointPayees.Cache.Persist(PXDBOperation.Update);
		}

		/// <summary>
		/// Extends <see cref="APReleaseProcess.ExtensionsPersisted"/>
		/// </summary>
		[PXOverride]
		public virtual void ExtensionsPersisted()
		{
			JointPayment.Cache.Persisted(false);
			JointPayees.Cache.Persisted(false);
		}
	}
}
