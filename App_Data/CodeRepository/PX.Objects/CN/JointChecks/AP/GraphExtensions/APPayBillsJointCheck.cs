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
using PX.Objects.CM.Extensions;
using PX.Objects.CN.Compliance;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CR;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CN.JointChecks
{
	public class ApPayBillsJointCheckExt : PXGraphExtension<APPayBills>
	{
		[PXCopyPasteHiddenView]
		public PXSetup<LienWaiverSetup> lienWaiverSetup;

		public PXSelectJoin<APInvoice,
			LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
				And<APTran.tranType, Equal<APInvoice.docType>,
				And<APTran.refNbr, Equal<APInvoice.refNbr>,
				And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>,
			InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
			LeftJoin<JointPayeePerDoc, On<JointPayeePerDoc.aPDocType, Equal<APInvoice.docType>,
				And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.refNbr>>>,
			LeftJoin<JointPayeePerLine, On<JointPayeePerLine.aPDocType, Equal<APInvoice.docType>,
				And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.refNbr>,
				And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>>>>>>>>,
			Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
				And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>> StoredResults;

		protected Dictionary<string, PayBillData> data;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.construction>();
		}

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search2<
			APInvoice.refNbr,
			InnerJoin<BAccount,
				On<BAccount.bAccountID, Equal<APInvoice.vendorID>,
				And<Where<
					BAccount.vStatus, Equal<VendorStatus.active>,
					Or<BAccount.vStatus, Equal<VendorStatus.oneTime>>>>>,
			LeftJoin<APAdjust,
				On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
				And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
				And<APAdjust.released, Equal<False>,
				And<Where<
					APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>,
					Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>,
			LeftJoin<APPayment,
				On<APPayment.docType, Equal<APInvoice.docType>,
				And<APPayment.refNbr, Equal<APInvoice.refNbr>,
				And<APPayment.docType, Equal<APDocType.prepayment>>>>>>>,
			Where<
				APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
				And2<Where<
					APInvoice.released, Equal<True>,
					Or<APInvoice.prebooked, Equal<True>>>,
				And<APInvoice.openDoc, Equal<True>,
				And2<Where<APAdjust.adjgRefNbr, IsNull, Or<APInvoice.isJointPayees, Equal<True>>>,
				And<APPayment.refNbr, IsNull>>>>>>),
			Filterable = true)]
		protected virtual void _(Events.CacheAttached<APAdjust.adjdRefNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Joint Payees", Visible = false, FieldClass = nameof(FeaturesSet.Construction))]
		protected virtual void _(Events.CacheAttached<APInvoice.isJointPayees> e)
		{
		}

		Dictionary<int, BalanceAmount> jointPayeeBalance;
		Dictionary<string, BalanceAmount> billBalance;
		[PXOverride]
		public virtual IEnumerable apdocumentlist(Func<IEnumerable> baseMethod)
		{
			data = new Dictionary<string, PayBillData>();
			IEnumerable resultset = baseMethod();

			jointPayeeBalance = GetJointPayeesBalance();
			billBalance = new Dictionary<string, BalanceAmount>();

			foreach (PXResult<APAdjust, APInvoice, APTran> res in resultset)
			{
				APAdjust adjust = (APAdjust)res;
				APInvoice doc = (APInvoice)res;
				APTran tran = (APTran)res;

				if (doc.IsJointPayees == true)
				{
					decimal balance = doc.PaymentsByLinesAllowed == true ? tran.CuryTranBal.GetValueOrDefault() : doc.CuryDocBal.GetValueOrDefault();
					decimal balanceInBase = doc.PaymentsByLinesAllowed == true ? tran.TranBal.GetValueOrDefault() : doc.DocBal.GetValueOrDefault();

					string key = GetKeyForBillBalance(adjust);
					if (!billBalance.ContainsKey(key))
					{
						billBalance.Add(key, new BalanceAmount(balance, balanceInBase));
					}

					CalculateJointAmountToPay(adjust, doc);
				}
			}

			return resultset;
		}

		private string GetKeyForBillBalance(APAdjust adjust)
		{
			return string.Format("{0}.{1}.{2}", adjust.AdjdDocType, adjust.AdjdRefNbr, adjust.AdjdLineNbr);
		}

		private Dictionary<int, BalanceAmount> GetJointPayeesBalance()
		{
			Dictionary<int, BalanceAmount> result = new Dictionary<int, BalanceAmount>();
			
			foreach(PayBillData item in data.Values)
			{
				foreach(JointPayee jp in item.JointPayees)
				{
					if (!result.ContainsKey(jp.JointPayeeId.Value))
					{
						result.Add(jp.JointPayeeId.Value, new BalanceAmount(jp.CuryJointBalance.GetValueOrDefault(), jp.JointBalance.GetValueOrDefault()));
					}
				}
			}

			return result;
		}

		private class BalanceAmount
		{
			public decimal Amount { get; set; }
			public decimal AmountInBase { get; set; }

			public BalanceAmount(decimal amount, decimal amountInBase)
			{
				Amount = amount;
				AmountInBase = amountInBase;
			}
		}

		[PXOverride]
		public virtual BqlCommand ComposeBQLCommandForAPDocumentListSelect(Func<BqlCommand> baseMethod)
		{
			var cmd = baseMethod.Invoke();

			cmd = BqlCommand.AppendJoin<LeftJoin<JointPayeePerDoc, On<Where2<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.docType>,
				And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.refNbr>,
				And<APInvoice.isRetainageDocument, Equal<False>>>>,
				Or<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.origDocType>,
				And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.origRefNbr>,
				And<APInvoice.isRetainageDocument, Equal<True>>>>>>>>>(cmd);

			cmd = BqlCommand.AppendJoin<LeftJoin<JointPayeePerLine, On<Where2<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.docType>,
				And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.refNbr>,
				And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
				And<APInvoice.isRetainageDocument, Equal<False>>>>>,
				Or<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.origDocType>,
				And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.origRefNbr>,
				And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
				And<APInvoice.isRetainageDocument, Equal<True>>>>>>>>>>(cmd);

			if (!string.IsNullOrEmpty(Base.Filter.Current.DocType) &&
				!string.IsNullOrEmpty(Base.Filter.Current.RefNbr))
			{
				cmd = cmd.WhereAnd(typeof(Where2<Where<APInvoice.docType, Equal<Current<PayBillsFilter.docType>>,
					And<APInvoice.refNbr, Equal<Current<PayBillsFilter.refNbr>>>>,
					Or<Where<APInvoice.origDocType, Equal<Current<PayBillsFilter.docType>>,
					And<APInvoice.origRefNbr, Equal<Current<PayBillsFilter.refNbr>>>>>>));
			}

			cmd = cmd.WhereAnd(typeof(Where2<Where<
				APInvoice.isJointPayees, Equal<False>,
				Or<APInvoice.isRetainageDocument, Equal<True>>>,
				Or<Where<
					JointPayeePerDoc.jointPayeeId, IsNotNull,
					Or<JointPayeePerLine.jointPayeeId, IsNotNull>>>>));

			return cmd;
		}

		[PXOverride]
		public virtual void StoreResultset(PXResult res, string docType, string refNbr, int? lineNbr,
			Action<PXResult, string, string, int?> baseMethod)
		{
			APInvoice doc = res.GetItem<APInvoice>();
			APTran tran = res.GetItem<APTran>();
			CurrencyInfo cury = res.GetItem<CurrencyInfo>();
			JointPayeePerDoc jppd = res.GetItem<JointPayeePerDoc>();
			JointPayeePerLine jppl = res.GetItem<JointPayeePerLine>();

			StoredResults.View.StoreResult(new List<object> { new PXResult<APInvoice, APTran, CurrencyInfo, JointPayeePerDoc, JointPayeePerLine>(doc, tran, cury, jppd, jppl) },
			PXQueryParameters.ExplicitParameters(lineNbr, docType, refNbr));

			//Store in memory to be used in the context of delegate apdocumentlist
			string key = string.Format("{0}.{1}.{2}", doc.DocType, doc.RefNbr, doc.PaymentsByLinesAllowed == true ? tran.LineNbr : 0);
			PayBillData record;
			if (!data.TryGetValue(key, out record))
			{
				record = new PayBillData(doc, tran, cury);
				data.Add(key, record);
			}

			if (doc.PaymentsByLinesAllowed == true)
			{
				if (jppl.JointPayeeId != null)
					record.JointPayees.Add(jppl);
			}
			else
			{
				if (jppd.JointPayeeId != null)
					record.JointPayees.Add(jppd);
			}
		}

		[PXOverride]
		public virtual APAdjust InitRecord(PXResult res, Func<PXResult, APAdjust> baseMethod)
		{
			APAdjust adjust = baseMethod(res);
			APInvoice invoice = res.GetItem<APInvoice>();
			
			if (invoice != null && invoice.IsJointPayees == true)
			{
				int? payeeID;
				if (invoice.PaymentsByLinesAllowed == true)
				{
					JointPayeePerLine jp = res.GetItem<JointPayeePerLine>();
					payeeID = jp.JointPayeeId;
				}
				else
				{
					JointPayeePerDoc jp = res.GetItem<JointPayeePerDoc>();
					payeeID = jp.JointPayeeId;
				}

				adjust.AdjNbr = payeeID;
				adjust.JointPayeeID = payeeID;
			}

			return adjust;
		}

		[PXOverride]
		public virtual bool SetSuggestedAmounts(APAdjust adj, APTran tran, APInvoice invoice,
			Func<APAdjust, APTran, APInvoice, bool> baseMethod)
		{
			bool result = baseMethod(adj, tran, invoice);

			var data = SelectPayBillData(adj.AdjdDocType, adj.AdjdRefNbr, adj.AdjdLineNbr);

			if (data != null && data.Invoice?.IsJointPayees == true)
			{
				ApAdjustExt rowExt = PXCache<APAdjust>.GetExtension<ApAdjustExt>(adj);
				if (rowExt != null)
				{
					rowExt.JointPayeeExternalName = data.GetJointPayeeExternalName(adj.AdjNbr);
					var jp = data.GetJointPayee(adj.AdjNbr);

					if (jp != null && jp.IsMainPayee != true)
					{
						if (data.Invoice.CuryID == Base.Filter.Current.CuryID)
						{
							rowExt.CuryJointAmountOwed = jp.CuryJointAmountOwed.GetValueOrDefault();
							rowExt.JointAmountOwed = jp.JointAmountOwed.GetValueOrDefault();
							rowExt.CuryJointBalance = jp.CuryJointBalance.GetValueOrDefault();
							rowExt.JointBalance = jp.JointBalance.GetValueOrDefault();
						}
						else
						{
							rowExt.JointAmountOwed = jp.JointAmountOwed.GetValueOrDefault();
							rowExt.JointBalance = jp.JointBalance.GetValueOrDefault();

							decimal curyval;
							CM.PXDBCurrencyAttribute.CuryConvCury<PayBillsFilter.curyInfoID>(Base.APDocumentList.Cache, rowExt, rowExt.JointAmountOwed.GetValueOrDefault(), out curyval);
							rowExt.CuryJointAmountOwed = curyval;

							CM.PXDBCurrencyAttribute.CuryConvCury<PayBillsFilter.curyInfoID>(Base.APDocumentList.Cache, rowExt, rowExt.JointBalance.GetValueOrDefault(), out curyval);
							rowExt.CuryJointBalance = curyval;
						}
						adj.CuryDiscBal = 0;
						adj.DiscBal = 0;
						adj.CuryAdjgDiscAmt = 0;
						adj.AdjDiscAmt = 0;
					}

					result = true;
				}
			}

			return result;
		}
		
		[PXOverride]
		public virtual APPayment PaymentPostProcessing(APPaymentEntry pe, APPayment payment, Vendor vendor,
			Func<APPaymentEntry, APPayment, Vendor, APPayment> baseMethod)
		{
			var result = baseMethod(pe, payment, vendor);

			var peExt = pe.GetExtension<APPaymentEntryLienWaiver>();

			if (result.Hold != true)
			{
				if (lienWaiverSetup.Current.ShouldStopPayments == true &&
					peExt.ContainsOutstandingLienWavers(payment))
				{
					payment.Hold = true;
					pe.Document.Update(payment);
				}

				if (result.Hold != true)
				{
					peExt.GenerateLienWaivers();
				}
				
				pe.Save.Press();
			}

			return result;
		}

		[PXOverride]
		public virtual APPaymentEntry CreatePaymentEntry(Func<APPaymentEntry> baseMethod)
		{
			APPaymentEntry pe = baseMethod();
			var peExt = pe.GetExtension<APPaymentEntryLienWaiver>();
			if (peExt != null)
			{
				peExt.IsPreparePaymentsMassProcessing = true;
			}
			
			var peExtJointCheck = pe.GetExtension<APPaymentEntryJointCheck>();
			if (peExtJointCheck != null)
			{
				peExtJointCheck.IsPreparePaymentsMassProcessing = true;
			}
			
			return pe;
		}

		private bool skipAmountToPayVerification = false;

		private void CalculateJointAmountToPay(APAdjust adj, APInvoice doc)
		{
			if (adj.JointPayeeID == null)
				return;

			var data = SelectPayBillData(adj.AdjdDocType, adj.AdjdRefNbr, adj.AdjdLineNbr);
			if (data != null)
			{
				string key = GetKeyForBillBalance(adj);

				if (data.Invoice.CuryID == Base.Filter.Current.CuryID)
				{
					decimal balance = billBalance[key].Amount;
					decimal cashDicount = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.CuryCashDiscBal.GetValueOrDefault() : data.Invoice.CuryDiscBal.GetValueOrDefault();
					decimal available = Math.Max(0, balance - cashDicount);
					decimal toPay = GetAmountToPay(adj.JointPayeeID, data, available).GetValueOrDefault();

					adj.CuryAdjgAmt = toPay;

					jointPayeeBalance[adj.JointPayeeID.Value].Amount -= toPay;
					billBalance[key].Amount -= toPay;
				}
				else
				{
					decimal balance = billBalance[key].AmountInBase;
					decimal cashDicount = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.CashDiscBal.GetValueOrDefault() : data.Invoice.DiscBal.GetValueOrDefault();
					decimal available = Math.Max(0, balance - cashDicount);
					decimal toPay = GetAmountToPayInBase(adj.JointPayeeID, data, available).GetValueOrDefault();

					adj.AdjAmt = toPay;

					decimal curyval;
					CM.PXDBCurrencyAttribute.CuryConvCury<PayBillsFilter.curyInfoID>(Base.Filter.Cache, Base.Filter.Current, adj.AdjAmt.GetValueOrDefault(), out curyval);
					adj.CuryAdjgAmt = curyval;

					jointPayeeBalance[adj.JointPayeeID.Value].AmountInBase -= toPay;
					billBalance[key].AmountInBase -= toPay;
				}

				try
				{
					skipAmountToPayVerification = true;
					adj = Base.APDocumentList.Update(adj);
				}
				finally
				{
					skipAmountToPayVerification = false;
				}

				if (data.GetJointPayee(adj.JointPayeeID).IsMainPayee == true)
				{
					if (data.Invoice.CuryID == Base.Filter.Current.CuryID)
					{
						adj.CuryAdjgDiscAmt = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.CuryCashDiscBal.GetValueOrDefault() : data.Invoice.CuryDiscBal.GetValueOrDefault();
					}
					else
					{
						decimal cashDicount = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.CashDiscBal.GetValueOrDefault() : data.Invoice.DiscBal.GetValueOrDefault();

						decimal curyval;
						CM.PXDBCurrencyAttribute.CuryConvCury<PayBillsFilter.curyInfoID>(Base.Filter.Cache, Base.Filter.Current, cashDicount, out curyval);
						adj.CuryAdjgDiscAmt = curyval;
					}

					adj = Base.APDocumentList.Update(adj);
				}
			}
		}

		private decimal? GetAmountToPay(int? jointPayeeID, PayBillData data, decimal available)
		{
			//Joint Payees:
			foreach (JointPayee jp in data.JointPayees.Where(payee => payee.IsMainPayee != true).OrderBy(payee => payee.JointPayeeId.GetValueOrDefault()))
			{
				decimal curyJointBalance = jointPayeeBalance[jp.JointPayeeId.Value].Amount;

				if (jp.JointPayeeId == jointPayeeID)
				{
					if (available > curyJointBalance)
					{
						return curyJointBalance;
					}
					else
					{
						return available;
					}
				}
				else
				{
					available = Math.Max(0, available - curyJointBalance);
				}
			}

			if (available <= 0)
				return 0;

			//Main Vendor:
			JointPayee mainVendor = data.JointPayees.Where(payee => payee.IsMainPayee == true).SingleOrDefault();
			if (mainVendor != null && mainVendor.JointPayeeId == jointPayeeID)
			{
				decimal curyJointBalance = jointPayeeBalance[mainVendor.JointPayeeId.Value].Amount;
				if (available > curyJointBalance)
				{
					return curyJointBalance;
				}
				else
				{
					return available;
				}
			}

			return 0;
		}

		private decimal? GetAmountToPayInBase(int? jointPayeeID, PayBillData data, decimal available)
		{
			//Joint Payees:
			foreach (JointPayee jp in data.JointPayees.Where(payee => payee.IsMainPayee != true).OrderBy(payee => payee.JointPayeeId.GetValueOrDefault()))
			{
				decimal jointBalance = jointPayeeBalance[jp.JointPayeeId.Value].AmountInBase;

				if (jp.JointPayeeId == jointPayeeID)
				{
					if (available > jointBalance)
					{
						return jointBalance;
					}
					else
					{
						return available;
					}
				}
				else
				{
					available = Math.Max(0, available - jointBalance);
				}
			}

			if (available <= 0)
				return 0;

			//Main Vendor:
			JointPayee mainVendor = data.JointPayees.Where(payee => payee.IsMainPayee == true).SingleOrDefault();
			if (mainVendor != null && mainVendor.JointPayeeId == jointPayeeID)
			{
				decimal jointBalance = jointPayeeBalance[mainVendor.JointPayeeId.Value].AmountInBase;
				if (available > jointBalance)
				{
					return jointBalance;
				}
				else
				{
					return available;
				}
			}

			return 0;
		}

		protected virtual void _(Events.FieldVerifying<APAdjust, APAdjust.curyAdjgAmt> e)
		{
			if (skipAmountToPayVerification)
				return;

			PayBillData data = SelectPayBillData(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.AdjdLineNbr);
			if (data != null && data.Invoice.IsJointPayees == true)
			{
				ApAdjustExt ext = e.Cache.GetExtension<ApAdjustExt>(e.Row);
				if (ext != null)
				{
					if (data.Invoice.CuryID == Base.Filter.Current.CuryID)
					{
						decimal? jointBalance;
						JointPayee payee = data.GetJointPayee(e.Row.JointPayeeID);
						if ( payee.IsMainPayee == true )
						{
							jointBalance = payee.CuryJointBalance - e.Row.CuryAdjgDiscAmt.GetValueOrDefault();
						}
						else
						{
							jointBalance = ext.CuryJointBalance;
						}

						if (jointBalance < (decimal?)e.NewValue)
						{
							throw new PXSetPropertyException(PX.Objects.AP.Messages.Entry_LE, jointBalance);
						}

						decimal siblingsTotal = GetSiblingsAmountPay(e.Row);

						decimal balance = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.CuryTranBal.GetValueOrDefault() : data.Invoice.CuryDocBal.GetValueOrDefault();
						balance = balance - e.Row.CuryAdjgDiscAmt.GetValueOrDefault();

						if (siblingsTotal + ((decimal?)e.NewValue).GetValueOrDefault() > balance)
						{
							decimal maxValue = balance - siblingsTotal;
							throw new PXSetPropertyException(JointCheckMessages.OutOfBalance, maxValue.ToString("n2"));
						}
					}
					else
					{
						decimal? jointBalance;
						JointPayee payee = data.GetJointPayee(e.Row.JointPayeeID);
						if (payee.IsMainPayee == true)
						{
							jointBalance = payee.JointBalance;
						}
						else
						{
							jointBalance = ext.JointBalance;
						}

						if (jointBalance < (decimal?)e.NewValue)
						{
							throw new PXSetPropertyException(PX.Objects.AP.Messages.Entry_LE, jointBalance);
						}

						decimal siblingsTotal = GetSiblingsAmountPayInBase(e.Row);

						decimal balance = data.Invoice.PaymentsByLinesAllowed == true ? data.Tran.TranBal.GetValueOrDefault() : data.Invoice.DocBal.GetValueOrDefault();
						balance = balance - e.Row.CuryAdjgDiscAmt.GetValueOrDefault();

						if (siblingsTotal + ((decimal?)e.NewValue).GetValueOrDefault() > balance)
						{
							decimal maxValue = balance - siblingsTotal;
							throw new PXSetPropertyException(JointCheckMessages.OutOfBalance, maxValue.ToString("n2"));
						}
					}
				}
			}
		}

		private decimal GetSiblingsAmountPay(APAdjust adjust)
		{
			decimal result = 0;
			foreach (APAdjust item in Base.APDocumentList.Cache.Inserted)
			{
				if (item.AdjdDocType == adjust.AdjdDocType &&
					item.AdjdRefNbr == adjust.AdjdRefNbr &&
					item.AdjdLineNbr == adjust.AdjdLineNbr &&
					item.AdjNbr != adjust.AdjNbr)
				{
					result += item.CuryAdjgAmt.GetValueOrDefault();
				}
			}

			return result;
		}

		private decimal GetSiblingsAmountPayInBase(APAdjust adjust)
		{
			decimal result = 0;
			foreach (APAdjust item in Base.APDocumentList.Cache.Inserted)
			{
				if (item.AdjdDocType == adjust.AdjdDocType &&
					item.AdjdRefNbr == adjust.AdjdRefNbr &&
					item.AdjdLineNbr == adjust.AdjdLineNbr &&
					item.AdjNbr != adjust.AdjNbr)
				{
					result += item.AdjAmt.GetValueOrDefault();
				}
			}

			return result;
		}

		protected virtual void _(Events.RowSelected<APAdjust> e)
		{
			if (e.Row != null &&
				lienWaiverSetup.Current.ShouldWarnOnBillEntry == true)
			{
				PayBillData data = SelectPayBillData(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.AdjdLineNbr);

				if (data != null && ContainsOutstandingLienWaversByVendor(data.Tran.ProjectID, e.Row.VendorID))
				{
					e.Cache.RaiseExceptionHandling<APAdjust.vendorID>(e.Row, e.Row.VendorID,
						new PXSetPropertyException<APAdjust.vendorID>(Compliance.Descriptor.ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver, PXErrorLevel.Warning));
				}

				if (data != null && data.Invoice.IsJointPayees == true)
				{
					var jp = data.GetJointPayee(e.Row.AdjNbr);
					if (jp != null && jp.JointPayeeInternalId != null &&
						ContainsOutstandingLienWaversByJointPayee(data.Tran.ProjectID, jp.JointPayeeInternalId))
					{
						e.Cache.RaiseExceptionHandling<ApAdjustExt.jointPayeeExternalName>(e.Row, jp.JointPayeeExternalName,
						new PXSetPropertyException<ApAdjustExt.jointPayeeExternalName>(Compliance.Descriptor.ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver, PXErrorLevel.Warning));
					}
				}
			}

		}

		int? lienWaiverDocType = null;

		private int? LienWaiverDocType
		{
			get
			{
				if (lienWaiverDocType == null)
				{
					lienWaiverDocType = GetLienWaiverDocumentType();
				}

				return lienWaiverDocType;
			}
		}

		private int? GetLienWaiverDocumentType()
		{
			var select = new PXSelect<ComplianceAttributeType,
				Where<ComplianceAttributeType.type, Equal<Required<ComplianceAttributeType.type>>>>(Base);

			ComplianceAttributeType type = select.SelectSingle(ComplianceDocumentType.LienWaiver);

			return type?.ComplianceAttributeTypeID;
		}

		private bool ContainsOutstandingLienWaversByVendor(int? projectID, int? vendorID)
		{
			var select = new PXSelectReadonly<ComplianceDocument,
				Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
				And<ComplianceDocument.projectID, Equal<Required<ComplianceDocument.projectID>>,
				And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
				And<ComplianceDocument.received, NotEqual<True>>>>>>>(Base);

			ComplianceDocument outstandingLV = select.SelectWindowed(0, 1, LienWaiverDocType, vendorID, projectID, Base.Accessinfo.BusinessDate);

			if (outstandingLV != null)
			{
				return true;
			}

			return false;
		}

		private bool ContainsOutstandingLienWaversByJointPayee(int? projectID, int? vendorID)
		{
			var select = new PXSelectReadonly<ComplianceDocument,
				Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
				And<ComplianceDocument.jointVendorInternalId, Equal<Required<ComplianceDocument.jointVendorInternalId>>,
				And<ComplianceDocument.projectID, Equal<Required<ComplianceDocument.projectID>>,
				And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
				And<ComplianceDocument.received, NotEqual<True>>>>>>>(Base);

			ComplianceDocument outstandingLV = select.SelectWindowed(0, 1, LienWaiverDocType, vendorID, projectID, Base.Accessinfo.BusinessDate);

			if (outstandingLV != null)
			{
				return true;
			}

			return false;
		}

		protected PayBillData SelectPayBillData(string docType, string refNbr, int? lineNbr)
		{
			PayBillData result = null;
			//In the context of apdocumentlist delegate use the in memory instance of cache and fallback to stored cache on callbacks.
			if (this.data != null)
			{
				string key = string.Format("{0}.{1}.{2}", docType, refNbr, lineNbr.GetValueOrDefault());

				data.TryGetValue(key, out result);
			}
			else
			{
				//TODO: NEED TO INVESTIGATE - On callback the select is goind to the database,
				//and not using the pre-stored cache as expected -> StoredResults.Select(lineNbr, docType, refNbr).
				//for now using the workaround:
				var select = new PXSelectJoin<APInvoice,
					LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
						And<APTran.tranType, Equal<APInvoice.docType>,
						And<APTran.refNbr, Equal<APInvoice.refNbr>,
						And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>,
					InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>,
					LeftJoin<JointPayeePerDoc, On<Where2<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.docType>,
						And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.refNbr>,
						And<APInvoice.isRetainageDocument, Equal<False>>>>,
						Or<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.origDocType>,
						And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.origRefNbr>,
						And<APInvoice.isRetainageDocument, Equal<True>>>>>>>,
					LeftJoin<JointPayeePerLine, On<Where2<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.docType>,
						And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.refNbr>,
						And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
						And<APInvoice.isRetainageDocument, Equal<False>>>>>,
						Or<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.origDocType>,
						And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.origRefNbr>,
						And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
						And<APInvoice.isRetainageDocument, Equal<True>>>>>>>>>>>>,
					Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
						And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>(Base);

				foreach (PXResult<APInvoice, APTran, CurrencyInfo, JointPayeePerDoc, JointPayeePerLine> res in
					select.Select(lineNbr, docType, refNbr))
				{
					CurrencyInfo info = res;
					APInvoice invoice = res;
					APTran tran = res;
					JointPayeePerDoc perDoc = res;
					JointPayeePerLine perLine = res;

					if (result == null)
						result = new PayBillData(invoice, tran, info);

					if (invoice.PaymentsByLinesAllowed == true)
					{
						if (perLine.JointPayeeId != null)
							result.JointPayees.Add(perLine);
					}
					else
					{
						if (perDoc.JointPayeeId != null)
							result.JointPayees.Add(perDoc);
					}
				}
			}

			return result;
		}

		public class PayBillData
		{
			public APInvoice Invoice { get; private set; }
			public APTran Tran { get; private set; }
			public CurrencyInfo CuryInfo { get; private set; }

			public List<JointPayee> JointPayees { get; private set; }

			public PayBillData(APInvoice invoice, APTran tran, CurrencyInfo curyInfo)
			{
				Invoice = invoice;
				Tran = tran;
				CuryInfo = curyInfo;
				JointPayees = new List<JointPayee>();
			}

			public string GetJointPayeeExternalName(int? payeeId)
			{
				JointPayeePerLine perLine = JointPayees.Where(j => j.JointPayeeId == payeeId).SingleOrDefault() as JointPayeePerLine;
				if (perLine != null)
				{
					return perLine.JointPayeeExternalName ?? perLine.JointVendorName;
				}

				JointPayeePerDoc perDoc = JointPayees.Where(j => j.JointPayeeId == payeeId).SingleOrDefault() as JointPayeePerDoc;
				if (perDoc != null)
				{
					return perDoc.JointPayeeExternalName ?? perDoc.JointVendorName;
				}

				return null;
			}

			public JointPayee GetJointPayee(int? payeeId)
			{
				return JointPayees.Where(j => j.JointPayeeId == payeeId).SingleOrDefault();
			}
		}
	}
}
