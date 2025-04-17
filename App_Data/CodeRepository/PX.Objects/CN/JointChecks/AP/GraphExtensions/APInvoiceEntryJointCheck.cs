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
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CS;
using PX.SM;

using CommonMessages = PX.Objects.Common.Messages;

namespace PX.Objects.CN.JointChecks
{
	public class APInvoiceEntryJointCheck : PXGraphExtension<APInvoiceEntry>
	{
		#region DAC overrides

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(APInvoice.docType))]
		protected virtual void _(Events.CacheAttached<JointPayee.aPDocType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APInvoice.refNbr))]
		[PXParent(typeof(Select<APInvoice, Where<APInvoice.docType, Equal<Current<JointPayee.aPDocType>>,
			And<APInvoice.refNbr, Equal<Current<JointPayee.aPRefNbr>>>>>))]
		protected virtual void _(Events.CacheAttached<JointPayee.aPRefNbr> e) { }

		#endregion

		[PXCopyPasteHiddenFields(typeof(JointPayee.curyJointBalance), typeof(JointPayee.curyJointAmountPaid))]
		[PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
			Where<Vendor.bAccountID, Equal<Current<JointPayee.jointPayeeInternalId>>>>))]
		public PXSelect<JointPayee,
			Where2<Where<JointPayee.aPDocType, Equal<Current<APInvoice.docType>>,
				And<JointPayee.aPRefNbr, Equal<Current<APInvoice.refNbr>>,
				And<Current<APInvoice.isRetainageDocument>, Equal<False>,
				And<JointPayee.isMainPayee, Equal<False>>>>>,
			Or<Where<JointPayee.aPDocType, Equal<Current<APInvoice.origDocType>>,
				And<JointPayee.aPRefNbr, Equal<Current<APInvoice.origRefNbr>>,
				And<Current<APInvoice.isRetainageDocument>, Equal<True>,
				And<JointPayee.isMainPayee, Equal<False>>>>>>>> JointPayees;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<JointPayeePayment,
			InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>,
				And<JointPayee.isMainPayee, NotEqual<True>>>,
			InnerJoin<APPayment, On<APPayment.docType, Equal<JointPayeePayment.paymentDocType>,
				And<APPayment.refNbr, Equal<JointPayeePayment.paymentRefNbr>>>>>,
			Where<JointPayeePayment.invoiceDocType, Equal<Current<APInvoice.docType>>,
				And<JointPayeePayment.invoiceRefNbr, Equal<Current<APInvoice.refNbr>>>>> JointAmountApplications;

		public PXSetup<LienWaiverSetup> lienWaiverSetup;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.construction>();
		}

		public PXAction<APInvoice> viewApPayment;
		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ViewApPayment(PXAdapter adapter)
		{
			JointPayeePayment jointPayeePayment = JointAmountApplications.Current;

			if (jointPayeePayment != null && !string.IsNullOrWhiteSpace(jointPayeePayment.PaymentRefNbr))
			{
				APPayment payment = APPayment.PK.Find(Base, jointPayeePayment.PaymentDocType, jointPayeePayment.PaymentRefNbr);

				if (payment != null)
				{
					APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();
					pe.Document.Current = payment;

					throw new PXRedirectRequiredException(pe, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}

			return adapter.Get();
		}

		[PXOverride]
		public IEnumerable PayInvoice(PXAdapter adapter,
			Func<PXAdapter, IEnumerable> baseMethod)
		{
			if (Base.Document.Current != null && IsJointPayee(Base.Document.Current))
			{
				var selectAllPayees = new PXSelect<JointPayee,
					Where2<Where<JointPayee.aPDocType, Equal<Current<APInvoice.docType>>,
						And<JointPayee.aPRefNbr, Equal<Current<APInvoice.refNbr>>,
						And<Current<APInvoice.isRetainageDocument>, Equal<False>>>>,
					Or<Where<JointPayee.aPDocType, Equal<Current<APInvoice.origDocType>>,
						And<JointPayee.aPRefNbr, Equal<Current<APInvoice.origRefNbr>>,
						And<Current<APInvoice.isRetainageDocument>, Equal<True>>>>>>>(Base);

				HashSet<int> jointPayees = new HashSet<int>();
				foreach ( JointPayee jp in selectAllPayees.Select())
				{
					jointPayees.Add(jp.JointPayeeId.Value);
				}

				var selectUreleasedAdjustment = new PXSelect<APAdjust,
					Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Current<APAdjust.adjdRefNbr>>,
					And<APAdjust.released, Equal<False>,
					And<APAdjust.jointPayeeID, IsNotNull>>>>,
					OrderBy<Asc<APAdjust.adjdRefNbr>>>(Base);

				foreach (APAdjust adjust in selectUreleasedAdjustment.Select())
				{
					jointPayees.Remove(adjust.JointPayeeID.Value);
				}

				if (jointPayees.Count == 0)
				{
					APAdjust topAdjustment = selectUreleasedAdjustment.SelectWindowed(0, 1);
					if (topAdjustment != null)
					{
						APPaymentEntry pe = PXGraph.CreateInstance<APPaymentEntry>();
						pe.Document.Current = pe.Document.Search<APPayment.refNbr>(topAdjustment.AdjgRefNbr, topAdjustment.AdjgDocType);
						throw new PXRedirectRequiredException(pe, "PayInvoice");
					}
				}

				APPayBills graph = PXGraph.CreateInstance<APPayBills>();
				graph.Clear();
				PayBillsFilter filter = (PayBillsFilter) graph.Filter.Cache.CreateCopy(graph.Filter.Current);
				filter.VendorID = Base.Document.Current.VendorID;
				filter.ShowPayInLessThan = false;
				filter.PayTypeID = Base.Document.Current.PayTypeID;
				filter.PayAccountID = Base.Document.Current.PayAccountID;
				filter.DocType = Base.Document.Current.DocType;
				filter.RefNbr = Base.Document.Current.RefNbr;
				graph.Filter.Update(filter);
				
				throw new PXRedirectRequiredException(graph, PX.Objects.AP.Messages.APPayBills);
			}
			else
			{
				return baseMethod(adapter);
			}
		}

		[PXOverride]
		public virtual void Persist(Action baseMethod)
		{
			if (IsJointPayee(Base.Document.Current))
			{
				if (Base.Document.Current.PaymentsByLinesAllowed == true)
				{
					RecalculateAmountOwedByLine();
				}
				else
				{
					RecalculateAmountOwed();
				}
			}
			baseMethod();
		}

		protected virtual void _(Events.FieldUpdating<APInvoice, APInvoice.isJointPayees> e)
		{
			if (Base.IsExport || Base.IsImport || Base.IsImportFromExcel)
				return;

			if ((bool?) e.NewValue == true || JointPayees.Select().Count == 0)
				return;

			var confirmationResult = Base.Document.Ask(
				CommonMessages.Warning,
				JointCheckMessages.ConfirmJointPayeesRemoval,
				MessageButtons.YesNo,
				MessageIcon.Warning);
			if (confirmationResult == WebDialogResult.Yes)
				return;

			e.Cancel = true;
			e.NewValue = true;
		}

		protected virtual void _(Events.FieldUpdated<APInvoice, APInvoice.isJointPayees> e)
		{
			if ((bool?) e.NewValue == true)
				return;

			// Fake joint payees for retainage.
			// They are actually for original invoice, not for retainage invoice.
			// They should not be deleted.
			if (e.Row.IsRetainageDocument == true)
				return;

			// Joint payees cannot be deleted
			// if they are already linked to payments.
			if (JointAmountApplications.Select().Count > 0)
				return;

			DeleteAllPayees();
		}

		public virtual void _(Events.FieldVerifying<APTran, APTran.curyLineAmt> e)
		{
			if (e.Row == null) return;

			if (PXAccess.FeatureInstalled<FeaturesSet.paymentsByLines>()
				&& Base.Document.Current?.PaymentsByLinesAllowed == true
				&& Base.Document.Current?.IsJointPayees == true
				&& (decimal?)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(JointCheckMessages.NegativeLinesAreNotAllowed);
			}
		}

		protected virtual void _(Events.FieldVerifying<JointPayee, JointPayee.curyJointAmountOwed> e)
		{
			if (e.Row.IsMainPayee != true)
			{
				if (Base.Document.Current.PaymentsByLinesAllowed == true)
				{
					ValidatePayByLine(e.Row, (decimal?)e.NewValue);
				}
				else
				{
					ValidatePayByDocument(e.Row, (decimal?)e.NewValue);
				}
			}
		}

		private void ValidatePayByDocument(JointPayee row, decimal? newValue)
		{
			decimal totalExcludingCurrent = 0;

			foreach (JointPayee jp in JointPayees.Select())
			{
				if (jp.JointPayeeId == row.JointPayeeId) continue;
				totalExcludingCurrent += jp.CuryJointBalance.GetValueOrDefault();
			}

			APInvoice bill = GetOriginalBill();
			
			Terms terms = Terms.PK.Find(Base, bill.TermsID);
			decimal cashDiscountPct = terms != null ? terms.DiscPercent.GetValueOrDefault() : 0;

			decimal availableUnreleasedRetainage = bill.CuryRetainageUnreleasedAmt.GetValueOrDefault()
				- bill.CuryRetainageUnreleasedAmt.GetValueOrDefault() * cashDiscountPct;

			decimal totalRetainageBalance = 0;

			if (bill.Released == true)
			{
				//retainage may exist only for released bill
				APInvoice retainageAggregate = GetRetainageAggregate(bill.DocType, bill.RefNbr);
				if (retainageAggregate != null)
				{
					totalRetainageBalance = retainageAggregate.CuryDocBal.GetValueOrDefault() - retainageAggregate.CuryDiscBal.GetValueOrDefault();
				}
			}

			decimal cashDiscount = bill.Released == true ? bill.CuryDiscBal.GetValueOrDefault() : bill.CuryOrigDiscAmt.GetValueOrDefault();
			decimal unreleasedPaymentAmountForMainVendor = GetUnreleasedPaymentTotalForMainVendor(row);

			decimal billedAmount = row.CuryJointAmountPaid.GetValueOrDefault() + GetUnreleasedPaymentTotalByJointPayee(row.JointPayeeId);
			decimal availableAmount = row.CuryJointAmountPaid.GetValueOrDefault() + bill.CuryDocBal.GetValueOrDefault()
				- unreleasedPaymentAmountForMainVendor
				- cashDiscount
				+ availableUnreleasedRetainage
				+ totalRetainageBalance
				- totalExcludingCurrent;

			if (newValue > availableAmount ||
				newValue < billedAmount
				)
			{
				throw new PXSetPropertyException(JointCheckMessages.JointAmountOwedIsIncorrect, billedAmount.ToString("n2"), availableAmount.ToString("n2"));
			}
		}

		private void ValidatePayByLine(JointPayee row, decimal? newValue)
		{
			decimal totalExcludingCurrent = 0;

			foreach (JointPayee jp in JointPayees.Select())
			{
				if (jp.APLineNbr == row.APLineNbr)
				{
					if (jp.JointPayeeId == row.JointPayeeId) continue;

					totalExcludingCurrent += jp.CuryJointBalance.GetValueOrDefault();
				}
			}

			APInvoice bill = GetOriginalBill();
			APTran line = APTran.PK.Find(Base, row.APDocType, row.APRefNbr, row.APLineNbr);

			if (line == null)
				return;

			decimal lineWeightRatio = 0;
			
			if (bill.CuryLineTotal.GetValueOrDefault() != 0)
			{
				lineWeightRatio = line.CuryTranAmt.GetValueOrDefault() / bill.CuryLineTotal.GetValueOrDefault();
			}

			decimal? lineBalance = line.Released == true ? line.CuryTranBal : line.CuryTranAmt;
			decimal? lineRetainageBalance = line.Released == true ? line.CuryRetainageBal : line.CuryRetainageAmt;
			decimal? lineCashDiscount = line.Released == true ? line.CuryCashDiscBal : lineWeightRatio * bill.CuryOrigDiscAmt;

			Terms terms = Terms.PK.Find(Base, bill.TermsID);
			decimal cashDiscountPct = terms != null ? terms.DiscPercent.GetValueOrDefault() : 0;
			decimal unreleasedPaymentAmountForMainVendor = GetUnreleasedPaymentTotalForMainVendor(row);

			decimal billedAmount = row.CuryJointAmountPaid.GetValueOrDefault() + GetUnreleasedPaymentTotalByJointPayee(row.JointPayeeId);

			decimal availableAmount = row.CuryJointAmountPaid.GetValueOrDefault() + lineBalance.GetValueOrDefault()
				- unreleasedPaymentAmountForMainVendor
				- lineCashDiscount.GetValueOrDefault()
				+ lineRetainageBalance.GetValueOrDefault()
				- totalExcludingCurrent;

			if (newValue > availableAmount ||
				newValue < billedAmount
				)
			{
				throw new PXSetPropertyException(JointCheckMessages.JointAmountOwedIsIncorrect, billedAmount.ToString("n2"), availableAmount.ToString("n2"));
			}
		}

		private APInvoice GetOriginalBill()
		{
			APInvoice bill = Base.Document.Current;
			if (bill.IsRetainageDocument == true)
			{
				bill = APInvoice.PK.Find(Base, bill.OrigDocType, bill.OrigRefNbr);
			}

			return bill;
		}

		private APInvoice GetRetainageAggregate(string docType, string refNbr)
		{
			var select = new PXSelectGroupBy<APInvoice, Where<APInvoice.isRetainageDocument, Equal<True>,
				And<APInvoice.origDocType, Equal<Required<APInvoice.docType>>,
				And<APInvoice.origRefNbr, Equal<Required<APInvoice.refNbr>>>>>,
				Aggregate<Sum<APInvoice.curyDocBal, Sum<APInvoice.curyDiscBal>>>>(Base);

			return select.Select(docType, refNbr);
		}

		/// <summary>
		/// Returns Sum of all unreleased payments for the current document and all related retainage bills.
		/// </summary>
		private decimal GetUnreleasedPaymentTotal()
		{
			decimal result = 0;

			if (Base.Document.Current.Released == true)
			{
				//payments may exist only for released bills.

				var selectPayments = new PXSelectGroupBy<APAdjust,
					Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>>>,
					Aggregate<Sum<APAdjust.curyAdjdAmt>>>(Base);

				APAdjust aggregate = selectPayments.Select();
				if (aggregate != null)
				{
					result = aggregate.CuryAdjdAmt.GetValueOrDefault();
				}

				var selectRetainagePayments = new PXSelectJoinGroupBy<APAdjust,
					InnerJoin<APRegister, On<APAdjust.adjdDocType, Equal<APRegister.docType>,
					And<APAdjust.adjdRefNbr, Equal<APRegister.refNbr>>>>,
					Where<APRegister.origDocType, Equal<Current<APInvoice.docType>>,
					And<APRegister.origRefNbr, Equal<Current<APInvoice.refNbr>>,
					And<APRegister.isRetainageDocument, Equal<True>,
					And<APAdjust.released, Equal<False>>>>>,
					Aggregate<Sum<APAdjust.curyAdjdAmt>>>(Base);

				aggregate = selectRetainagePayments.Select();
				if (aggregate != null)
				{
					result += aggregate.CuryAdjdAmt.GetValueOrDefault();
				}
			}

			return result;
		}

		private decimal GetUnreleasedPaymentTotal(int? lineNbr)
		{
			decimal result = 0;

			if (Base.Document.Current.Released == true)
			{
				//payments may exist only for released bills.

				var selectPayments = new PXSelectGroupBy<APAdjust,
					Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>,
					And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>,
					And<APAdjust.adjdLineNbr, Equal<Required<APAdjust.adjdLineNbr>>>>>,
					Aggregate<Sum<APAdjust.curyAdjdAmt>>>(Base);

				APAdjust aggregate = selectPayments.Select(lineNbr);
				if (aggregate != null)
				{
					result = aggregate.CuryAdjdAmt.GetValueOrDefault();
				}

				var selectRetainagePayments = new PXSelectJoinGroupBy<APAdjust,
					InnerJoin<APRegister, On<APAdjust.adjdDocType, Equal<APRegister.docType>,
					And<APAdjust.adjdRefNbr, Equal<APRegister.refNbr>,
					And<APAdjust.adjdLineNbr, Equal<Required<APAdjust.adjdLineNbr>>>>>>,
					Where<APRegister.origDocType, Equal<Current<APInvoice.docType>>,
					And<APRegister.origRefNbr, Equal<Current<APInvoice.refNbr>>,
					And<APRegister.isRetainageDocument, Equal<True>,
					And<APAdjust.released, Equal<False>>>>>,
					Aggregate<Sum<APAdjust.curyAdjdAmt>>>(Base);

				aggregate = selectRetainagePayments.Select(lineNbr);
				if (aggregate != null)
				{
					result += aggregate.CuryAdjdAmt.GetValueOrDefault();
				}
			}

			return result;
		}


		private decimal GetUnreleasedPaymentTotalByJointPayee(int? jointPayeeID)
		{
			var selectPayments = new PXSelectGroupBy<APAdjust,
				Where<APAdjust.jointPayeeID, Equal<Required<APAdjust.jointPayeeID>>,
				And<APAdjust.released, Equal<False>>>,
				Aggregate<Sum<APAdjust.curyAdjdAmt>>>(Base);

			APAdjust aggregate = selectPayments.Select(jointPayeeID);

			if (aggregate != null)
			{
				return aggregate.CuryAdjdAmt.GetValueOrDefault();
			}

			return 0;
		}

		private decimal GetUnreleasedPaymentTotalForMainVendor(JointPayee jointPayee)
		{
			APInvoice bill = Base.Document.Current;

			JointPayee mainPayee;

			if (bill.PaymentsByLinesAllowed == true)
			{
				mainPayee = new PXSelect<JointPayee,
					Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
					And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
					And<JointPayee.aPLineNbr, Equal<Required<JointPayee.aPLineNbr>>,
					And<JointPayee.isMainPayee, Equal<True>>>>>>(Base)
					.SelectSingle(jointPayee.APDocType, jointPayee.APRefNbr, jointPayee.APLineNbr);
			}
			else
			{
				mainPayee = new PXSelect<JointPayee,
					Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
					And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
					And<JointPayee.isMainPayee, Equal<True>>>>>(Base)
					.SelectSingle(jointPayee.APDocType, jointPayee.APRefNbr);
			}

			if (mainPayee != null)
			{
				return GetUnreleasedPaymentTotalByJointPayee(mainPayee.JointPayeeId);
			}

			return 0;
		}

		protected virtual void _(Events.RowSelected<APInvoice> e)
		{
			var invoice = e.Row;
			if (invoice == null)
				return;

			var isPayByLineEnabled = invoice.PaymentsByLinesAllowed == true;
			PXUIFieldAttribute.SetVisible<JointPayee.aPLineNbr>(JointPayees.Cache, null, isPayByLineEnabled);
			PXUIFieldAttribute.SetVisible<JointPayee.billLineAmount>(JointPayees.Cache, null, isPayByLineEnabled);

			PXUIFieldAttribute.SetEnabled<APInvoice.isJointPayees>(e.Cache, invoice,
				invoice.OpenDoc == true && JointAmountApplications.Select().Count == 0);

			if (lienWaiverSetup.Current.ShouldWarnOnBillEntry == true
				&& ContainsOutstandingLienWaversByVendor(invoice))
			{
				e.Cache.RaiseExceptionHandling<APInvoice.vendorID>(invoice, invoice.VendorID,
					new PXSetPropertyException<APInvoice.vendorID>(Compliance.Descriptor.ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver, PXErrorLevel.Warning));
			}
		}

		protected virtual void _(Events.RowSelected<JointPayee> e)
		{
			if (e.Row != null && e.Row.JointPayeeInternalId != null)
			{
				if (lienWaiverSetup.Current.ShouldWarnOnBillEntry == true &&
					ContainsOutstandingLienWaversByJointPayee(Base.Document.Current, e.Row.JointPayeeInternalId))
				{
					e.Cache.RaiseExceptionHandling<JointPayee.jointPayeeInternalId>(e.Row, e.Row.JointPayeeInternalId,
						new PXSetPropertyException<JointPayee.jointPayeeInternalId>(Compliance.Descriptor.ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void _(Events.RowDeleted<APTran> e)
		{
			if (Base.Document.Current?.IsRetainageDocument != true &&
				Base.Document.Current?.PaymentsByLinesAllowed == true)
			{
				foreach (JointPayee jp in JointPayees.Select())
				{
					if (jp.APLineNbr == e.Row.LineNbr)
						JointPayees.Delete(jp);
				}
			}
		}

		protected virtual void _(Events.RowPersisting<JointPayee> e)
		{
			if (e.Row != null)
			{
				if (e.Operation != PXDBOperation.Delete)
				{
					if (e.Row.APLineNbr == null &&
						Base.Document.Current?.PaymentsByLinesAllowed == true )
					{
						e.Cache.RaiseExceptionHandling<JointPayee.aPLineNbr>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, $"[{nameof(JointPayee.aPLineNbr)}]"));
					}
				}
			}
		}

		private void DeleteAllPayees()
		{
			foreach (JointPayee jp in JointPayees.Select())
			{
				JointPayees.Delete(jp);
			}
		}

		private bool IsJointPayee(APInvoice doc)
		{
			if (doc != null)
			{
				return doc.IsJointPayees == true;
			}

			return false;
		}

		private void RecalculateAmountOwed()
		{
			var selectMainPayee = new PXSelect<JointPayee,
				Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
				And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
				And<JointPayee.isMainPayee, Equal<True>>>>>(Base);


			APInvoice doc = Base.Document.Current;
			if (doc.IsRetainageDocument == true)
			{
				doc = APInvoice.PK.Find(Base, doc.OrigDocType, doc.OrigRefNbr);
			}

			decimal amountOwed = 0;
			
			foreach (JointPayee jp in JointPayees.Select())
			{
				amountOwed += jp.CuryJointAmountOwed.GetValueOrDefault();
			}

			decimal mainBalance = Math.Max(0, doc.CuryOrigDocAmtWithRetainageTotal.GetValueOrDefault() - amountOwed);

			JointPayee mainPayee = selectMainPayee.Select(doc.DocType, doc.RefNbr);

			if (mainPayee != null)
			{
				if (mainPayee.CuryJointAmountPaid == null || mainPayee.CuryJointBalance == null)
				{
					mainPayee.CuryJointAmountPaid = 0;
					mainPayee.CuryJointBalance = 0;
					mainPayee = JointPayees.Update(mainPayee);
				}
				
				mainPayee.CuryJointAmountOwed = mainBalance;
				JointPayees.Update(mainPayee);
			}
			else
			{
				mainPayee = new JointPayee();
				mainPayee.IsMainPayee = true;
				mainPayee.APDocType = doc.DocType;
				mainPayee.APRefNbr = doc.RefNbr;
				mainPayee.JointPayeeInternalId = Base.Document.Current.VendorID;
				mainPayee.CuryJointAmountOwed = mainBalance;
				mainPayee = JointPayees.Insert(mainPayee);
			}
		}

		private class JointPayment
		{
			public decimal Original;
			public decimal Retainage;
		}
		private void RecalculateAmountOwedByLine()
		{
			var selectMainPayee = new PXSelect<JointPayee,
				Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
				And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
				And<JointPayee.isMainPayee, Equal<True>>>>>(Base);

			Dictionary<int, decimal> amountOwed = new Dictionary<int, decimal>(); 

			foreach (JointPayee jp in JointPayees.Select())
			{
				if (!amountOwed.ContainsKey(jp.APLineNbr.GetValueOrDefault()))
				{
					amountOwed.Add(jp.APLineNbr.GetValueOrDefault(), 0);
				}

				amountOwed[jp.APLineNbr.GetValueOrDefault()] += jp.CuryJointAmountOwed.GetValueOrDefault();
			}

			Dictionary<int, JointPayee> mainPayeesByLine = new Dictionary<int, JointPayee>();

			APInvoice doc = Base.Document.Current;
			if (doc.IsRetainageDocument == true)
			{
				doc = APInvoice.PK.Find(Base, doc.OrigDocType, doc.OrigRefNbr);
			}

			foreach (JointPayee jp in selectMainPayee.Select(doc.DocType, doc.RefNbr))
			{
				mainPayeesByLine.Add(jp.APLineNbr.GetValueOrDefault(), jp);
			}

			foreach (APTran tran in Base.Transactions.View.SelectMultiBound(new object[] { doc }))
			{
				decimal jointAmountOwed = 0;
				amountOwed.TryGetValue(tran.LineNbr.Value, out jointAmountOwed);
				
				JointPayee mainPayee;
				if (mainPayeesByLine.TryGetValue(tran.LineNbr.Value, out mainPayee))
				{
					mainPayee.CuryJointAmountOwed = Math.Max(0, tran.CuryTranAmt.GetValueOrDefault() + tran.CuryRetainageAmt.GetValueOrDefault() - jointAmountOwed);
					JointPayees.Update(mainPayee);
				}
				else
				{
					mainPayee = new JointPayee();
					mainPayee.IsMainPayee = true;
					mainPayee.JointPayeeInternalId = tran.VendorID;
					mainPayee.APDocType = doc.DocType;
					mainPayee.APRefNbr = doc.RefNbr;
					mainPayee.APLineNbr = tran.LineNbr;
					mainPayee.CuryJointAmountOwed = Math.Max(0, tran.CuryTranAmt.GetValueOrDefault() + tran.CuryRetainageAmt.GetValueOrDefault() - jointAmountOwed);
					mainPayee = JointPayees.Insert(mainPayee);
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

		private bool ContainsOutstandingLienWaversByVendor(APInvoice invoice)
		{
			if (Base.apsetup.Current.RequireSingleProjectPerDocument == true)
			{
				//only one project.
				var select = new PXSelectReadonly<ComplianceDocument,
					Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.projectID, Equal<Required<ComplianceDocument.projectID>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>(Base);

				ComplianceDocument outstandingLV = select.SelectWindowed(0, 1, LienWaiverDocType, invoice.VendorID, invoice.ProjectID, Base.Accessinfo.BusinessDate);

				if (outstandingLV != null)
				{
					return true;
				}
			}
			else
			{
				//based on lines.
				string docType = invoice.IsRetainageDocument == true ? invoice.OrigDocType : invoice.DocType;
				string refNbr = invoice.IsRetainageDocument == true ? invoice.OrigRefNbr : invoice.RefNbr;

				var select = new PXSelectReadonly2<APTran,
				InnerJoin<ComplianceDocument, On<ComplianceDocument.projectID, Equal<APTran.projectID>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>,
				Where<APTran.tranType, Equal<Required<APTran.tranType>>,
				And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>(Base);

				APTran outstanding = select.SelectWindowed(0, 1, invoice.VendorID, LienWaiverDocType, Base.Accessinfo.BusinessDate, docType, refNbr);

				if (outstanding != null)
				{
					return true;
				}
			}

			return false;
		}

		private bool ContainsOutstandingLienWaversByJointPayee(APInvoice invoice, int? vendorID)
		{
			if (Base.apsetup.Current.RequireSingleProjectPerDocument == true)
			{
				//only one project.
				var select = new PXSelectReadonly<ComplianceDocument,
					Where<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.jointVendorInternalId, Equal<Required<ComplianceDocument.jointVendorInternalId>>,
					And<ComplianceDocument.projectID, Equal<Required<ComplianceDocument.projectID>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>>(Base);

				ComplianceDocument outstandingLV = select.SelectWindowed(0, 1, LienWaiverDocType, invoice.VendorID, vendorID, invoice.ProjectID, Base.Accessinfo.BusinessDate);

				if (outstandingLV != null)
				{
					return true;
				}
			}
			else
			{
				//based on lines.
				string docType = invoice.IsRetainageDocument == true ? invoice.OrigDocType : invoice.DocType;
				string refNbr = invoice.IsRetainageDocument == true ? invoice.OrigRefNbr : invoice.RefNbr;

				var select = new PXSelectReadonly2<APTran,
				InnerJoin<ComplianceDocument, On<ComplianceDocument.projectID, Equal<APTran.projectID>,
					And<ComplianceDocument.vendorID, Equal<Required<ComplianceDocument.vendorID>>,
					And<ComplianceDocument.jointVendorInternalId, Equal<Required<ComplianceDocument.jointVendorInternalId>>,
					And<ComplianceDocument.documentType, Equal<Required<ComplianceDocument.documentType>>,
					And<ComplianceDocument.throughDate, Less<Required<ComplianceDocument.throughDate>>,
					And<ComplianceDocument.received, NotEqual<True>>>>>>>>,
				Where<APTran.tranType, Equal<Required<APTran.tranType>>,
				And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>(Base);

				APTran outstanding = select.SelectWindowed(0, 1, invoice.VendorID, vendorID, LienWaiverDocType, Base.Accessinfo.BusinessDate, docType, refNbr);

				if (outstanding != null)
				{
					return true;
				}
			}

			return false;
		}
	}
}
