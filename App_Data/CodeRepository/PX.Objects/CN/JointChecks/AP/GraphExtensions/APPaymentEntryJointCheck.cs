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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CN.JointChecks
{

	public class APPaymentEntryJointCheck : PXGraphExtension<APPaymentEntry>
	{
		#region DAC Overrides
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APPayment.docType))]
		protected virtual void _(Events.CacheAttached<JointPayeePayment.paymentDocType> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBDefault(typeof(APPayment.refNbr))]
		protected virtual void _(Events.CacheAttached<JointPayeePayment.paymentRefNbr> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APInvoiceType.AdjdRefNbr(typeof(Search5<APAdjust.APInvoice.refNbr,
			LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APAdjust.APInvoice.docType>,
				And<APAdjust.adjdRefNbr, Equal<APAdjust.APInvoice.refNbr>,
				And<APAdjust.released, Equal<False>,
				And<Where<APAdjust.adjgDocType, NotEqual<Current<APPayment.docType>>,
					Or<APAdjust.adjgRefNbr, NotEqual<Current<APPayment.refNbr>>>>>>>>,
			LeftJoin<APAdjust2, On<APAdjust2.adjgDocType, Equal<APAdjust.APInvoice.docType>,
				And<APAdjust2.adjgRefNbr, Equal<APAdjust.APInvoice.refNbr>,
				And<APAdjust2.released, Equal<False>,
				And<APAdjust2.voided, Equal<False>>>>>,
			LeftJoin<APPayment, On<APPayment.docType, Equal<APAdjust.APInvoice.docType>,
				And<APPayment.refNbr, Equal<APAdjust.APInvoice.refNbr>,
				And<Where<APPayment.docType, Equal<APDocType.prepayment>,
					Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>>,
			Where<APAdjust.APInvoice.vendorID, Equal<Optional<APPayment.vendorID>>,
				And<APAdjust.APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
				And2<Where<APAdjust.APInvoice.released, Equal<True>,
					Or<APAdjust.APInvoice.prebooked, Equal<True>>>,
				And<APAdjust.APInvoice.openDoc, Equal<True>,
				And<APAdjust.APInvoice.hold, Equal<False>,
				And2<Where<APAdjust.adjgRefNbr, IsNull, Or<APAdjust.APInvoice.isJointPayees, Equal<True>>>,
				And<APAdjust2.adjdRefNbr, IsNull,
				And2<Where<APPayment.refNbr, IsNull,
					And<Current<APPayment.docType>, NotEqual<APDocType.refund>,
					Or<APPayment.refNbr, IsNotNull,
					And<Current<APPayment.docType>, Equal<APDocType.refund>,
					Or<APPayment.docType, Equal<APDocType.debitAdj>,
					And<Current<APPayment.docType>, Equal<APDocType.check>,
					Or<APPayment.docType, Equal<APDocType.debitAdj>,
					And<Current<APPayment.docType>, Equal<APDocType.voidCheck>>>>>>>>>,
				And2<Where<APAdjust.APInvoice.docDate, LessEqual<Current<APPayment.adjDate>>,
					And<APAdjust.APInvoice.tranPeriodID, LessEqual<Current<APPayment.adjTranPeriodID>>,
					Or<Current<APPayment.adjTranPeriodID>, IsNull,
					Or<Current<APPayment.docType>, Equal<APDocType.check>,
					And<Current<APSetup.earlyChecks>, Equal<True>,
					Or<Current<APPayment.docType>, Equal<APDocType.voidCheck>,
					And<Current<APSetup.earlyChecks>, Equal<True>,
					Or<Current<APPayment.docType>, Equal<APDocType.prepayment>,
					And<Current<APSetup.earlyChecks>, Equal<True>>>>>>>>>>,
				And2<Where<
					Current<APSetup.migrationMode>, NotEqual<True>,
					Or<APAdjust.APInvoice.isMigratedRecord, Equal<Current<APRegister.isMigratedRecord>>>>,
				And<Where<APAdjust.APInvoice.pendingPPD, NotEqual<True>,
					Or<Current<APRegister.pendingPPD>, Equal<True>>>>>>>>>>>>>>,
				Aggregate<GroupBy<APAdjust.APInvoice.docType, GroupBy<APAdjust.APInvoice.refNbr>>>>),
			Filterable = true)]
		protected virtual void _(Events.CacheAttached<APAdjust.adjdRefNbr> e)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(typeof(Switch<Case<Where<Selector<APAdjust.adjdRefNbr, APAdjust.APInvoice.paymentsByLinesAllowed>, NotEqual<True>>, int0>, Null>))]
		[APInvoiceType.AdjdLineNbr]
		protected virtual void _(Events.CacheAttached<APAdjust.adjdLineNbr> e)
		{
		}

		#endregion

		public bool IsPreparePaymentsMassProcessing { get; set; }

		#region Views/Selects

		public PXSelect<JointPayeeDisplay,
			Where<JointPayeeDisplay.jointPayeeId, Equal<Current<APPayment.jointPayeeID>>>> SelectedJointPayee;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<JointPayeePayment,
			InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>,
			Where<JointPayeePayment.paymentDocType, Equal<Current<APPayment.docType>>,
			And<JointPayeePayment.paymentRefNbr, Equal<Current<APPayment.refNbr>>,
			And<JointPayee.isMainPayee, Equal<False>>>>> JointPayments;

		
		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocumentPaymentReference> LinkToPayments;
				
		public PXFilter<JointPayeeFilter> BillWithJointPayeeFilter;

		[PXCopyPasteHiddenView]
		[PXVirtualDAC]
		public PXSelect<JointPayeeRecord, Where<JointPayeeRecord.curyBalance, NotEqual<Zero>>,
			OrderBy<Desc<JointPayeeRecord.refNbr,
				Asc<JointPayeeRecord.lineNbr,
				Asc<JointPayeeRecord.jointPayeeID>>>>> BillWithJointPayee;
		public virtual IEnumerable billWithJointPayee()
		{
			List<JointPayeeRecord> list = new List<JointPayeeRecord>(202);

			bool found = false;
			foreach (JointPayeeRecord item in BillWithJointPayee.Cache.Cached)
			{
				PXEntryStatus status = BillWithJointPayee.Cache.GetStatus(item);
				if (status == PXEntryStatus.Inserted)
				{
					found = true;
					list.Add(item);
				}
			}

			if (!found)
			{
					BqlCommand cmd = new Select2<APInvoice,
						LeftJoin<APTran, On<APInvoice.paymentsByLinesAllowed, Equal<True>,
									 And<APTran.tranType, Equal<APInvoice.docType>,
									 And<APTran.refNbr, Equal<APInvoice.refNbr>,
									 And<APTran.curyTranBal, Greater<decimal0>>>>>,
						LeftJoin<JointPayeePerDoc, On<Where2<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.docType>,
							  And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.refNbr>,
							  And<APInvoice.isRetainageDocument, Equal<False>,
							  And<JointPayeePerDoc.isMainPayee, Equal<False>>>>>,
							  Or<Where<JointPayeePerDoc.aPDocType, Equal<APInvoice.origDocType>,
							  And<JointPayeePerDoc.aPRefNbr, Equal<APInvoice.origRefNbr>,
							  And<APInvoice.isRetainageDocument, Equal<True>,
							  And<JointPayeePerDoc.isMainPayee, Equal<False>>>>>>>>,
						LeftJoin<JointPayeePerLine, On<Where2<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.docType>,
							  And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.refNbr>,
							  And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
							  And<APInvoice.isRetainageDocument, Equal<False>,
							  And<JointPayeePerLine.isMainPayee, Equal<False>>>>>>,
							Or<Where<JointPayeePerLine.aPDocType, Equal<APInvoice.origDocType>,
							And<JointPayeePerLine.aPRefNbr, Equal<APInvoice.origRefNbr>,
							And<JointPayeePerLine.aPLineNbr, Equal<APTran.lineNbr>,
							And<APInvoice.isRetainageDocument, Equal<True>,
							And<JointPayeePerLine.isMainPayee, Equal<False>>>>>>>>>>>>,
						Where<APInvoice.vendorID, Equal<Current<APPayment.vendorID>>,
						And<APInvoice.docType, Equal<Current<JointPayeeFilter.docType>>,
						And<APInvoice.curyDocBal, Greater<decimal0>,
						And<APInvoice.isJointPayees, Equal<True>,
						And2<Where<APInvoice.released, Equal<True>,
						Or<APInvoice.prebooked, Equal<True>>>,
						And<Where<Current<JointPayeeFilter.refNbr>, IsNull,
							Or<Current<JointPayeeFilter.refNbr>, Equal<APInvoice.refNbr>>>>>>>>>>();

				PXView view = new PXView(Base, false, cmd);
				var startRow = PXView.StartRow;
				int totalRows = 0;

				var resultset = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
				PXView.StartRow = 0;

				foreach (PXResult<APInvoice, APTran, JointPayeePerDoc, JointPayeePerLine> res in resultset)
				{
					APInvoice doc = (APInvoice)res;
					APTran tran = (APTran)res;
					JointPayeePerDoc perDoc = (JointPayeePerDoc)res;
					JointPayeePerLine perLine = (JointPayeePerLine)res;

					JointPayeeRecord record = new JointPayeeRecord();
					record.DocType = doc.DocType;
					record.RefNbr = doc.RefNbr;

					if (doc.PaymentsByLinesAllowed == true)
					{
						record.JointPayeeID = perLine.JointPayeeId;
						record.LineNbr = tran.LineNbr;
						record.Name = perLine.JointPayeeExternalName ?? perLine.JointVendorName;
						record.CuryJointAmountOwed = perLine.CuryJointAmountOwed;
						record.JointAmountOwed = perLine.JointAmountOwed;
						record.CuryJointAmountPaid = perLine.CuryJointAmountPaid;
						record.JointAmountPaid = perLine.JointAmountPaid;
						record.CuryBalance = record.CuryJointAmountOwed - record.CuryJointAmountPaid;
						record.Balance = record.JointAmountOwed - record.JointAmountPaid;
					}
					else
					{
						record.JointPayeeID = perDoc.JointPayeeId;
						record.LineNbr = 0;
						record.Name = perDoc.JointPayeeExternalName ?? perDoc.JointVendorName;
						record.CuryJointAmountOwed = perDoc.CuryJointAmountOwed;
						record.JointAmountOwed = perDoc.JointAmountOwed;
						record.CuryJointAmountPaid = perDoc.CuryJointAmountPaid;
						record.JointAmountPaid = perDoc.JointAmountPaid;
						record.CuryBalance = record.CuryJointAmountOwed - record.CuryJointAmountPaid;
						record.Balance = record.JointAmountOwed - record.JointAmountPaid;
					}

					var existing = BillWithJointPayee.Locate(record);

					if (existing == null)
					{
						list.Add(BillWithJointPayee.Insert(record));
					}
				}

			}

			#region Eliminate voided from the list
			HashSet<string> voidedBills = new HashSet<string>();
			foreach (APAdjust adjust in Base.Adjustments.Select())
			{
				if (adjust.Voided == true)
				{
					voidedBills.Add(string.Format("{0}.{1}.{2}", adjust.AdjdDocType, adjust.AdjdRefNbr, adjust.AdjdLineNbr));
				}
			}

			List<JointPayeeRecord> toRemove = new List<JointPayeeRecord>();
			foreach (JointPayeeRecord record in list)
			{
				string key = string.Format("{0}.{1}.{2}", record.DocType, record.RefNbr, record.LineNbr);
				if (voidedBills.Contains(key))
				{
					toRemove.Add(record);
				}
			}

			foreach (JointPayeeRecord record in toRemove)
			{
				list.Remove(record);
			} 
			#endregion

			return list;
		}

		[PXCopyPasteHiddenView]
		public PXSelect<ComplianceDocument> Compliance;
		#endregion


		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.construction>();
		}

		public PXAction<APPayment> addJointPayee;
		[PXUIField(DisplayName = "Add Joint Payee")]
		[PXButton]
		public IEnumerable AddJointPayee(PXAdapter adapter)
		{
			if (BillWithJointPayee.View.AskExt() == WebDialogResult.OK)
			{
				AddSelectedJointPayees();
			}

			return adapter.Get();
		}

		protected virtual void AddSelectedJointPayees()
		{
			HashSet<string> existing = new HashSet<string>();

			foreach (APAdjust line in Base.Adjustments.Select())
			{
				string key = string.Format("{0}.{1}.{2}.{3}", line.AdjdDocType, line.AdjdRefNbr, line.AdjdLineNbr.GetValueOrDefault(), line.JointPayeeID.GetValueOrDefault());
				existing.Add(key);
			}

			foreach (JointPayeeRecord selected in BillWithJointPayee.Cache.Cached)
			{
				if (selected.Selected != true)
					continue;

				string key = string.Format("{0}.{1}.{2}.{3}", selected.DocType, selected.RefNbr, selected.LineNbr.GetValueOrDefault(), selected.JointPayeeID.GetValueOrDefault());

				if (existing.Contains(key))
					continue;
								
				if (Base.Document.Current.JointPayeeID == null)
				{
					Base.Document.Current.JointPayeeID = selected.JointPayeeID;
					Base.Document.UpdateCurrent();
				}				

				APAdjust adjust = new APAdjust();
				adjust.AdjdDocType = selected.DocType;
				adjust.AdjdRefNbr = selected.RefNbr;
				adjust.AdjdLineNbr = selected.LineNbr;
				adjust.JointPayeeID = selected.JointPayeeID;

				adjust = Base.Adjustments.Insert(adjust);
				if (adjust != null)
				{
					JointPayee jp = JointPayee.PK.Find(Base, adjust.JointPayeeID);
					if (jp != null)
					{
						decimal jointBalance = jp.CuryJointBalance.GetValueOrDefault();
						if (adjust.AdjdCuryID != Base.Document.Current.CuryID)
						{
							CM.PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury<APAdjust.adjgCuryInfoID>(Base.Adjustments.Cache, adjust, jp.JointBalance.GetValueOrDefault(), out jointBalance);
						}
						adjust.CuryAdjgAmt = Math.Min(adjust.CuryAdjgAmt.GetValueOrDefault(), jointBalance);
						adjust.CuryAdjgDiscAmt = 0;
						Base.Adjustments.Update(adjust);
					}
				}
			}
		}


		[PXOverride]
		public virtual void InitAdjustmentData(APAdjust adj, APRegister invoice, APTran tran,
			Action<APAdjust, APRegister, APTran> baseMethod)
		{
			if (adj.JointPayeeID == null)
			{
				APInvoice doc = invoice as APInvoice;
				if (doc != null && doc.IsJointPayees == true)
				{
					if (doc.PaymentsByLinesAllowed == true)
					{
						var select = new PXSelect<JointPayee,
							Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
							And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
							And<JointPayee.aPLineNbr, Equal<Required<JointPayee.aPLineNbr>>,
							And<JointPayee.isMainPayee, Equal<True>>>>>>(Base);
						JointPayee mainPayee = select.Select(invoice.DocType, invoice.RefNbr, tran.LineNbr);
						if (mainPayee != null)
						{
							adj.JointPayeeID = mainPayee.JointPayeeId;
						}
					}
					else
					{
						var select = new PXSelect<JointPayee,
							Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
							And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
							And<JointPayee.isMainPayee, Equal<True>>>>>(Base);
						JointPayee mainPayee = select.Select(invoice.DocType, invoice.RefNbr);
						if (mainPayee != null)
						{
							adj.JointPayeeID = mainPayee.JointPayeeId;
						}
					}
				}
			}
		}

		[PXOverride]
		public virtual decimal CalculateApplicationAmount(APAdjust adj, Func<APAdjust, decimal> baseMethod)
		{
			decimal applAmt = baseMethod(adj);

			if (Base.Document.Current.DocType == APDocType.DebitAdj)
			{
				applAmt = Math.Min(GetDebitAdjustmentMaxBalance(adj).GetValueOrDefault(applAmt), applAmt);
			}
			else
			{				
				if (adj.JointPayeeID != null)
				{
					JointPayee payee = JointPayee.PK.Find(Base, adj.JointPayeeID);
					applAmt = Math.Max(0, Math.Min(applAmt, payee.CuryJointBalance.GetValueOrDefault() - adj.CuryDiscBal.GetValueOrDefault()));
				}
			}

			return applAmt;
		}

		protected virtual void _(Events.RowSelected<APPayment> e)
		{
			if (e.Row != null)
			{
				//if (IsInReversingApplicationStateWithJointPayee())
				//{
				//	Base.Adjustments.Cache.AllowInsert = false;
				//}
								
				addJointPayee.SetEnabled(e.Row.VendorID != null &&
					e.Row.DocType == APDocType.Check &&
					e.Row.OpenDoc == true);
			}
		}
		protected virtual void _(Events.RowUpdated<APPayment> e)
		{
			if (IsPreparePaymentsMassProcessing)
				return;

			if (e.Row != null && e.Row.JointPayeeID != null)
			{
				APPayment row = (APPayment)e.Row;
				decimal jointPayeeBalance;
				if (row.OpenDoc == true && row.Hold != true && IsJointPayeeBalanceExceeds(row, out jointPayeeBalance))
				{
					e.Cache.RaiseExceptionHandling<APPayment.curyOrigDocAmt>(row, row.CuryOrigDocAmt, new PXSetPropertyException(JointCheckMessages.BalanceExceeds, Math.Round(jointPayeeBalance, IN.CommonSetupDecPl.PrcCst), PXErrorLevel.Error));
				}
			}
		}

		protected virtual void _(Events.RowPersisting<APPayment> e)
		{
			if (IsPreparePaymentsMassProcessing)
				return;

			if (e.Row != null && e.Row.JointPayeeID != null)
			{
				APPayment row = (APPayment)e.Row;
				decimal jointPayeeBalance;
				if (row.OpenDoc == true && row.Hold != true && IsJointPayeeBalanceExceeds(row, out jointPayeeBalance))
				{
					throw new PXRowPersistingException(typeof(APPayment.curyOrigDocAmt).Name, row.CuryOrigDocAmt, JointCheckMessages.BalanceExceeds, Math.Round(jointPayeeBalance, IN.CommonSetupDecPl.PrcCst));
				}
			}
		}

		private bool IsJointPayeeBalanceExceeds(APPayment row, out decimal jointPayeeBalance)
		{
			var adjustments = new SelectFrom<APAdjust>
			.LeftJoin<APInvoice>.On<APInvoice.refNbr.IsEqual<APAdjust.adjdRefNbr>
				.And<APInvoice.docType.IsEqual<APAdjust.adjdDocType>>>
			.InnerJoin<JointPayee>.On<JointPayee.jointPayeeId.IsEqual<APAdjust.jointPayeeID>>
			.Where<APAdjust.adjgDocType.IsEqual<APPayment.docType.FromCurrent>
			.And<APAdjust.adjgRefNbr.IsEqual<APPayment.refNbr.FromCurrent>>
			.And<APAdjust.released.IsNotEqual<True>>
			.And<APInvoice.isJointPayees.IsEqual<True>>
			.And<APInvoice.isRetainageDocument.IsNotEqual<True>>>
			.AggregateTo<Sum<JointPayee.jointBalance>>.View(this.Base);

			using (new PXFieldScope(adjustments.View, typeof(JointPayee.jointBalance)))
			{
				JointPayee jointPayee = adjustments.Select().RowCast<JointPayee>().FirstOrDefault();
				jointPayeeBalance = 0;
				if (jointPayee != null && jointPayee.JointBalance != null)
				{
					jointPayeeBalance = jointPayee.JointBalance.GetValueOrDefault();
					if (Base.Adjustments.Current != null && Base.Adjustments.Current.AdjdCuryID != Base.Document.Current.CuryID)
					{
						CM.PXCurrencyAttribute.PXCurrencyHelper.CuryConvCury<APAdjust.adjgCuryInfoID>(Base.Adjustments.Cache, Base.Adjustments, jointPayee.JointBalance.GetValueOrDefault(), out jointPayeeBalance);
					}

					if (jointPayee != null && row.CuryOrigDocAmt > jointPayeeBalance)
						return true;
				}
			}
			return false;			
		}

		private bool IsInReversingApplicationStateWithJointPayee()
		{
			foreach ( JointPayeePayment payment in JointPayments.Cache.Updated)
			{
				if (payment.JointPayeeId != null && payment.IsVoided == true)
				{
					return true;
				}
			}

			return false;
		}

		protected virtual void _(Events.RowUpdated<JointPayeeFilter> e)
		{
			BillWithJointPayee.Cache.Clear();

		}

		protected virtual void _(Events.RowPersisting<JointPayeeRecord> e)
		{
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldVerifying<JointPayeeRecord, JointPayeeRecord.selected> e)
		{
			HashSet<JointPayeeRecordKey> alreadyAdded = new HashSet<JointPayeeRecordKey>();

			foreach (APAdjust adjust in Base.Adjustments.Select())
			{
				if (adjust.AdjdDocType == e.Row.DocType &&
					adjust.AdjdRefNbr == e.Row.RefNbr &&
					adjust.AdjdLineNbr == e.Row.LineNbr &&
					adjust.JointPayeeID == e.Row.JointPayeeID
					)
					continue;

				if (adjust.Voided == true)
					continue;

				alreadyAdded.Add(new JointPayeeRecordKey(adjust.AdjdDocType, adjust.AdjdRefNbr, adjust.AdjdLineNbr.GetValueOrDefault(), adjust.JointPayeeID.GetValueOrDefault()));
			}

			foreach (JointPayeeRecord record in BillWithJointPayee.Cache.Cached)
			{
				if (record.DocType == e.Row.DocType &&
					record.RefNbr == e.Row.RefNbr &&
					record.LineNbr == e.Row.LineNbr &&
					record.JointPayeeID == e.Row.JointPayeeID
					)
					continue;

				if (record.Selected == true)
					alreadyAdded.Add(new JointPayeeRecordKey(record.DocType, record.RefNbr, record.LineNbr.GetValueOrDefault(), record.JointPayeeID.GetValueOrDefault()));
			}

			foreach (JointPayeeRecordKey record in alreadyAdded)
			{
				if (record.JointPayeeID == e.Row.JointPayeeID)
					continue;

				JointPayee jp = JointPayee.PK.Find(Base, record.JointPayeeID);

				if (jp != null)
				{
					if (jp.JointPayeeInternalId == null)
					{
						var ex = new PXSetPropertyException(JointCheckMessages.SingleLineWithExternal, PXErrorLevel.RowError);
						ex.ErrorValue = false;
						throw ex;
					}
					else
					{
						JointPayee newJP = JointPayee.PK.Find(Base, e.Row.JointPayeeID);

						if (newJP != null)
						{
							if (newJP.JointPayeeInternalId == null)
							{
								Vendor selectedInternalVendor = Vendor.PK.Find(Base, jp.JointPayeeInternalId);

								var ex = new PXSetPropertyException(JointCheckMessages.LineWithExternalCannotBeSelected, PXErrorLevel.RowError, selectedInternalVendor.AcctName);
								ex.ErrorValue = false;
								throw ex;
							}
							else if (newJP.JointPayeeInternalId != jp.JointPayeeInternalId)
							{
								var ex = new PXSetPropertyException(JointCheckMessages.LineWithDifferentInternal, PXErrorLevel.RowError);
								ex.ErrorValue = false;
								throw ex;
							}
						}
					}
				}
			}
		}

		protected virtual void _(Events.RowInserted<APAdjust> e)
		{
			if (e.Row.AdjdDocType != APDocType.Invoice)
				return;

			if (jointPayeePayment != null)
			{
				//Processing
				e.Cache.SetValue<APAdjust.jointPayeeID>(e.Row, jointPayeePayment.JointPayeeId);
				jointPayeePayment.CuryJointAmountToPay += e.Row.CuryAdjgAmt.GetValueOrDefault();
				JointPayments.Update(jointPayeePayment);
			}
			else
			{
				if (!Base.AutoPaymentApp && e.Row.JointPayeeID != null)
				{
					JointPayeePayment jpp = GetJointPayment(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.AdjdLineNbr);
					if (jpp == null)
					{
						JointPayeePayment jointPayeePayment = JointPayments.Insert();
						jointPayeePayment.JointPayeeId = e.Row.JointPayeeID;
						jointPayeePayment.InvoiceDocType = e.Row.AdjdDocType;
						jointPayeePayment.InvoiceRefNbr = e.Row.AdjdRefNbr;
						jointPayeePayment.AdjustmentNumber = e.Row.AdjdLineNbr.GetValueOrDefault();
						jointPayeePayment.CuryJointAmountToPay = e.Row.CuryAdjgAmt;
						jointPayeePayment.JointAmountToPay = e.Row.AdjAmt;
						JointPayments.Insert(jointPayeePayment);
					}
				}
			}
		}

		protected virtual void _(Events.RowUpdated<APAdjust> e)
		{
			if (e.Row.AdjdDocType != APDocType.Invoice)
				return;

			if (e.Row.CuryAdjgAmt != e.OldRow.CuryAdjgAmt && e.Row.Voided != true)
			{
				if (jointPayeePayment != null)
				{
					//Processing prepare payments
					jointPayeePayment.CuryJointAmountToPay -= e.OldRow.CuryAdjgAmt.GetValueOrDefault();
					jointPayeePayment.CuryJointAmountToPay += e.Row.CuryAdjgAmt.GetValueOrDefault();
					JointPayments.Update(jointPayeePayment);
				}
				else
				{
					//Check and Payments UI
					JointPayeePayment jpp = GetJointPayment(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.AdjdLineNbr);
					if (jpp != null)
					{
						jpp.CuryJointAmountToPay -= e.OldRow.CuryAdjgAmt.GetValueOrDefault();
						jpp.CuryJointAmountToPay += e.Row.CuryAdjgAmt.GetValueOrDefault();
						JointPayments.Update(jpp);
					}
				}
			}

			if (Base.AutoPaymentApp)
			{
				if (e.Row.Voided == true)
				{
					JointPayeePayment jpp = GetJointPayment(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.VoidAdjNbr);
					if (jpp != null)
					{
						jpp.IsVoided = true;
						JointPayments.Update(jpp);

						JointPayeePayment jointPayeePayment = JointPayments.Insert();
						jointPayeePayment.JointPayeeId = e.Row.JointPayeeID;
						jointPayeePayment.InvoiceDocType = e.Row.AdjdDocType;
						jointPayeePayment.InvoiceRefNbr = e.Row.AdjdRefNbr;
						jointPayeePayment.AdjustmentNumber = e.Row.AdjdLineNbr.GetValueOrDefault();
						jointPayeePayment.CuryJointAmountToPay = e.Row.CuryAdjgAmt;
						jointPayeePayment.JointAmountToPay = e.Row.AdjAmt;
						JointPayments.Update(jointPayeePayment);
					}

					ClearJointPayeeOnDocument();
				}
			}
		}

		private JointPayeePayment GetJointPayment(string docType, string refNbr, int? lineNbr)
		{
			var select = new PXSelect<JointPayeePayment,
						Where<JointPayeePayment.paymentDocType, Equal<Current<APPayment.docType>>,
						And<JointPayeePayment.paymentRefNbr, Equal<Current<APPayment.refNbr>>,
						And<JointPayeePayment.invoiceDocType, Equal<Required<JointPayeePayment.invoiceDocType>>,
						And<JointPayeePayment.invoiceRefNbr, Equal<Required<JointPayeePayment.invoiceRefNbr>>,
						And<JointPayeePayment.adjustmentNumber, Equal<Required<JointPayeePayment.adjustmentNumber>>,
						And<JointPayeePayment.isVoided, NotEqual<True>>>>>>>>(Base);

			return select.Select(docType, refNbr, lineNbr.GetValueOrDefault());
		}

		protected virtual void _(Events.RowDeleted<APAdjust> e)
		{
			if (e.Row.AdjdDocType != APDocType.Invoice)
				return;

			if (jointPayeePayment != null)
			{
				//Processing prepare payments
				jointPayeePayment.CuryJointAmountToPay -= e.Row.CuryAdjgAmt;
				JointPayments.Update(jointPayeePayment);
			}
			else
			{
				//Check and Payments UI
				
				JointPayeePayment jpp = GetJointPayment(e.Row.AdjdDocType, e.Row.AdjdRefNbr, e.Row.AdjdLineNbr);
				if (jpp != null)
				{
					jpp.CuryJointAmountToPay -= e.Row.CuryAdjgAmt.GetValueOrDefault();
					JointPayments.Update(jpp);
				}
			}

			ResetJointPayeeOnLastLineDeleted();
		}

		protected virtual void _(Events.FieldVerifying<APAdjust, APAdjust.curyAdjgAmt> e)
		{
			if (e.Row.JointPayeeID != null)
			{
				JointPayee jp = JointPayee.PK.Find(Base, e.Row.JointPayeeID);
				if (e.Row.AdjdCuryID == Base.Document.Current.CuryID)
				{
					if (jp != null && (decimal?)e.NewValue > jp.CuryJointBalance)
					{
						throw new PXSetPropertyException(PX.Objects.AP.Messages.Entry_LE, jp.CuryJointBalance.Value.ToString("n2"));
					}
				}
				else
				{
					if (jp != null && (decimal?)e.NewValue > jp.JointBalance)
					{
						throw new PXSetPropertyException(PX.Objects.AP.Messages.Entry_LE, jp.JointBalance.Value.ToString("n2"));
					}
				}
			}
			else
			{
				if (Base.Document.Current.DocType == APDocType.DebitAdj)
				{
					ValidateDebitAdjustment(e.Row, (decimal?) e.NewValue);
				}
			}
		}

		private void ValidateDebitAdjustment(APAdjust row, decimal? newValue)
		{
			decimal? maxValue = GetDebitAdjustmentMaxBalance(row);
			if (maxValue != null && newValue > maxValue)
			{
				throw new PXSetPropertyException(JointCheckMessages.AmountPaidExceedsVendorBalanceForDebitAdjustment, maxValue.Value.ToString("n2"));
			}
		}

		private decimal? GetDebitAdjustmentMaxBalance(APAdjust row)
		{
			var selectMainVendor = new PXSelect<JointPayee,
						Where<JointPayee.aPDocType, Equal<Required<JointPayee.aPDocType>>,
						And<JointPayee.aPRefNbr, Equal<Required<JointPayee.aPRefNbr>>,
						And<JointPayee.isMainPayee, Equal<True>>>>>(Base);

			JointPayee mainVendor = selectMainVendor.Select(row.AdjdDocType, row.AdjdRefNbr);
			if (mainVendor != null)
			{
				return mainVendor.CuryJointBalance.GetValueOrDefault();
			}

			return null;
		}

		private void ClearJointPayeeOnDocument()
		{
			bool containsDetails = false;
			foreach (APAdjust adjust in Base.Adjustments.Select())
			{
				if (adjust.Voided != true)
				{
					containsDetails = true;
					break;
				}
			}

			if (!containsDetails)
			{
				Base.Document.Current.JointPayeeID = null;
				Base.Document.UpdateCurrent();
			}
		}

		private void ResetJointPayeeOnLastLineDeleted()
		{
			if (Base.Document.Current != null && Base.Document.Cache.GetStatus(Base.Document.Current) != PXEntryStatus.Deleted)
			{
				if (Base.Adjustments.Select().Count == 0)
				{
					Base.Document.Current.JointPayeeID = null;
					Base.Document.UpdateCurrent();
				}
			}
		}

		JointPayeePayment jointPayeePayment;

		[PXOverride]
		public virtual void Segregate(APAdjust adj, CurrencyInfo info, bool? onHold,
			Action<APAdjust, CurrencyInfo, bool?> baseMethod)
		{
			int? payeeId = adj.AdjNbr;
			
			if (Base.IsDirty)
			{
				Base.Save.Press();
			}

			APInvoice apdoc = Base.APInvoice_VendorID_DocType_RefNbr.Select(adj.AdjdLineNbr, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr);

			JointPayee jointPayee = JointPayee.PK.Find(Base, payeeId);

			if (payeeId != null && adj.SeparateCheck != true && jointPayee?.IsMainPayee != true)
			{
				string key;
				if (jointPayee.JointPayeeInternalId != null)
				{
					key = string.Format("V_{0}", jointPayee.JointPayeeInternalId);
				}
				else
				{
					key = string.Format("JP_{0}", jointPayee.JointPayeeId);
				}

				APPayment payment = Base.created.Find<APPayment.vendorID, APPayment.vendorLocationID, APPayment.hiddenKey>(apdoc.VendorID, apdoc.PayLocationID, key);
				if (payment != null )
				{
					Base.Document.Current = Base.Document.Search<APPayment.refNbr>(payment.RefNbr, payment.DocType);
					if (Base.createdInfo.TryGetValue(Base.Document.Current.CuryInfoID.Value, out var paymentInfo))
					{
						Base.FindImplementation<APPaymentEntry.MultiCurrency>()?.StoreResult(paymentInfo);
					}
				}
				else
				{
					baseMethod(adj, info, onHold);
				}

				Base.Document.Current.HiddenKey = key;
			}
			else
			{
				baseMethod(adj, info, onHold);
			}

			if (adj.AdjdDocType == APDocType.Invoice)
			{
				jointPayeePayment = JointPayments.Insert();
				jointPayeePayment.JointPayeeId = payeeId;
				jointPayeePayment.InvoiceDocType = adj.AdjdDocType;
				jointPayeePayment.InvoiceRefNbr = adj.AdjdRefNbr;
				jointPayeePayment.AdjustmentNumber = adj.AdjdLineNbr.GetValueOrDefault();

				if (Base.Document.Current.JointPayeeID == null && jointPayee != null && jointPayee.IsMainPayee != true)
				{
					Base.Document.Current.JointPayeeID = payeeId;
				}
			}
		}

		[PXOverride]
		public virtual APPayment FindOrCreatePayment(APInvoice apdoc, APAdjust adj,
			Func<APInvoice, APAdjust, APPayment> baseMethod)
		{
			int? payeeId = adj.AdjNbr;
			JointPayee jointPayee = JointPayee.PK.Find(Base, payeeId);
			if (payeeId != null && adj.SeparateCheck != true && jointPayee?.IsMainPayee != true)
			{
				string hiddenKey = string.Format("{0}", jointPayee.JointPayeeInternalId ?? jointPayee.JointPayeeId);

				return Base.created.Find<APPayment.vendorID, APPayment.vendorLocationID, APPayment.hiddenKey>(apdoc.VendorID, apdoc.PayLocationID, hiddenKey) ??
				new APPayment();
			}

			APPayment result = baseMethod(apdoc, adj);
			if (jointPayee != null && jointPayee.IsMainPayee != true)
			{
				Base.Document.Cache.SetValue<APPayment.jointPayeeID>(result, jointPayee.JointPayeeId);
			}

			return result;
		}

		[PXOverride]
		public virtual void Persist(Action baseMethod)
		{
			AddLinksToCompliance();

			baseMethod();
		}

		protected virtual void AddLinksToCompliance()
		{
			HashSet<string> processed = new HashSet<string>();
			if (Base.Document.Current != null)
			{
				foreach (var res in Base.Adjustments.Select())
				{
					if (res is PXResult<APAdjust, APInvoice, APTran> apDoc)
					{
						APInvoice bill = (APInvoice)apDoc;
						string key = string.Format("{0}.{1}", bill.DocType, bill.RefNbr);
						if (!processed.Contains(key))
						{
							processed.Add(key);
							LinkComplianceToPayment(bill, Base.Document.Current);
						}
					}
				}
			}
		}

		public virtual void LinkComplianceToPayment(APInvoice apdoc, APPayment payment)
		{
			var select = new PXSelectJoin<ComplianceDocumentReference,
				InnerJoin<ComplianceDocument,
				On<ComplianceDocument.billID, Equal<ComplianceDocumentReference.complianceDocumentReferenceId>>>,
				Where<ComplianceDocumentReference.refNoteId, Equal<Required<ComplianceDocumentReference.refNoteId>>,
				And<ComplianceDocument.linkToPayment, Equal<True>,
				And<ComplianceDocument.apCheckId, IsNull>>>>(Base);

			foreach (PXResult<ComplianceDocumentReference, ComplianceDocument> res in select.Select(apdoc.NoteID))
			{
				ComplianceDocument cd = (ComplianceDocument)res;

				ComplianceDocumentPaymentReference docRef = new ComplianceDocumentPaymentReference();
				docRef.ComplianceDocumentReferenceId = Guid.NewGuid();
				LinkToPayments.Insert(docRef);

				cd.ApCheckID = docRef.ComplianceDocumentReferenceId;
				cd.CheckNumber = payment.ExtRefNbr;
				Compliance.Update(cd);
			}
		}

		[PXHidden]
		public class JointPayeeFilter : IBqlTable
		{
			#region DocType
			
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			
			[PXDBString(3, IsFixed = true, InputMask = "")]
			[PXDefault(APDocType.Invoice)]
			[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
			[APInvoiceType.AdjdList()]
			public virtual String DocType
			{
				get;
				set;
			}

			#endregion

			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			
			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Reference Nbr.")]
			[PXSelector(typeof(Search<APInvoice.refNbr, Where<APInvoice.docType, Equal<Current<docType>>,
				And<APInvoice.vendorID, Equal<Current<APPayment.vendorID>>,
				And<APInvoice.isJointPayees, Equal<True>>>>>))]
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
		}

		[PXHidden]
		public class JointPayeeRecord : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
			{
			}
			
			[PXBool]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get;
				set;
			}
			#endregion


			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

			[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(APInvoice.docType))]
			[PXUIField(DisplayName = "Doc Type")]
			public virtual String DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

			[PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(APInvoice.refNbr))]
			[PXUIField(DisplayName = "Reference Nbr.")]
			public virtual String RefNbr
			{
				get;
				set;
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

			[PXDBInt(IsKey = true, BqlField = typeof(APTran.lineNbr))]
			[PXUIField(DisplayName = "Bill Line Nbr.", FieldClass = nameof(FeaturesSet.PaymentsByLines))]
			public virtual int? LineNbr
			{
				get;
				set;
			}
			#endregion
			#region JointPayeeID
			public abstract class jointPayeeID : PX.Data.BQL.BqlInt.Field<jointPayeeID> { }

			[PXInt(IsKey = true)]
			public virtual int? JointPayeeID
			{
				get;
				set;
			}
			#endregion

			#region Name
			public abstract class name : PX.Data.BQL.BqlString.Field<name> { }

			[PXString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Joint Payee Name")]
			public virtual String Name
			{
				get;
				set;
			}
			#endregion
			#region CuryJointAmountOwed
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBCurrency(typeof(APPayment.curyInfoID), typeof(jointAmountOwed))]
			[PXUIField(DisplayName = "Joint Amount Owed")]
			public virtual decimal? CuryJointAmountOwed
			{
				get;
				set;
			}
			public abstract class curyJointAmountOwed : BqlDecimal.Field<curyJointAmountOwed>
			{
			}
			#endregion

			#region JointAmountOwed
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBBaseCury]
			public virtual decimal? JointAmountOwed
			{
				get;
				set;
			}
			public abstract class jointAmountOwed : BqlDecimal.Field<jointAmountOwed>
			{
			}
			#endregion

			#region CuryJointAmountPaid
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBCurrency(typeof(APPayment.curyInfoID), typeof(jointAmountPaid))]
			[PXUIField(DisplayName = "Joint Amount Paid", IsReadOnly = true)]
			public virtual decimal? CuryJointAmountPaid
			{
				get;
				set;
			}
			public abstract class curyJointAmountPaid : BqlDecimal.Field<curyJointAmountPaid>
			{
			}
			#endregion

			#region JointAmountPaid
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBBaseCury]
			public virtual decimal? JointAmountPaid
			{
				get;
				set;
			}
			public abstract class jointAmountPaid : BqlDecimal.Field<jointAmountPaid>
			{
			}
			#endregion
			#region CuryBalance
			public abstract class curyBalance : PX.Data.BQL.BqlDecimal.Field<curyBalance> { }
			
			[PXCurrency(typeof(APPayment.curyInfoID), typeof(balance), BaseCalc = false)]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Joint Balance", Visibility = PXUIVisibility.Visible)]
			public virtual decimal? CuryBalance
			{
				get;
				set;
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }
		
			[PXDecimal(2)]
			[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual decimal? Balance
			{
				get;
				set;
			}
			#endregion
		}

		[System.Diagnostics.DebuggerDisplay("{ProjectID}.{OrderNbr}")]
		public class JointPayeeRecordKey
		{
			public readonly string DocType;
			public readonly string RefNbr;
			public readonly int LineNbr;
			public readonly int JointPayeeID;
			
			public JointPayeeRecordKey(string docType, string refNbr, int lineNbr, int jointPayeeID)
			{
				DocType = docType;
				RefNbr = refNbr;
				LineNbr = lineNbr;
				JointPayeeID = jointPayeeID;
			}

			public override int GetHashCode()
			{
				unchecked // Overflow is fine, just wrap
				{
					int hash = 17;
					hash = hash * 23 + DocType.GetHashCode();
					hash = hash * 23 + RefNbr.GetHashCode();
					hash = hash * 23 + LineNbr.GetHashCode();
					hash = hash * 23 + JointPayeeID.GetHashCode();
					return hash;
				}
			}

			
		}



	}
}
