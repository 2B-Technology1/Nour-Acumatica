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
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;

namespace PX.Objects.SO.GraphExtensions.ARReleaseProcessExt
{
	public class Correction : PXGraphExtension<IN.GraphExtensions.ARReleaseProcessExt.ProcessInventory, ARReleaseProcess>
	{
		/// <summary>
		/// Overrides <see cref="ARReleaseProcess.CloseInvoiceAndClearBalances(ARRegister, int?)"/>
		/// </summary>
		[PXOverride]
		public virtual void CloseInvoiceAndClearBalances(ARRegister ardoc, Action<ARRegister> baseMethod)
		{
			if (ardoc.IsUnderCorrection == true && Base.IsIntegrityCheck == false)
			{
				ardoc.Canceled = true;
				ARInvoice.Events
					.Select(ev=>ev.CancelDocument)
					.FireOn(Base, (ARInvoice)ardoc);

				PXDatabase.Update<ARTran>(
					new PXDataFieldAssign<ARTran.canceled>(PXDbType.Bit, true),
					new PXDataFieldRestrict<ARTran.tranType>(PXDbType.Char, ardoc.DocType),
					new PXDataFieldRestrict<ARTran.refNbr>(PXDbType.NVarChar, ardoc.RefNbr),
					new PXDataFieldRestrict<ARTran.canceled>(PXDbType.Bit, false));
			}

			baseMethod(ardoc);
		}

		/// <summary>
		/// Overrides <see cref="ARReleaseProcess.OpenInvoiceAndRecoverBalances(ARRegister)"/>
		/// </summary>
		[PXOverride]
		public virtual void OpenInvoiceAndRecoverBalances(ARRegister ardoc, Action<ARRegister> baseMethod)
		{
			if (ardoc.IsUnderCorrection == true && !Base.IsIntegrityCheck)
			{
				throw new PXException(Messages.OnlyCancelCreditMemoCanBeApplied, ardoc.RefNbr);
			}

			baseMethod(ardoc);
		}

		/// <summary>
		/// Overrides <see cref="IN.GraphExtensions.ARReleaseProcessExt.ProcessInventory.ProcessARTranInventory(ARTran, ARInvoice, JournalEntry)"/>
		/// </summary>
		[PXOverride]
		public virtual void ProcessARTranInventory(ARTran n, ARInvoice ardoc, JournalEntry je, Action<ARTran, ARInvoice, JournalEntry> baseMethod)
		{
			if (ardoc.IsCancellation == true)
			{
				if (Base.IsIntegrityCheck || n?.LineType == SOLineType.Discount) return;

				foreach (INTran intran in Base1.intranselect.View.SelectMultiBound(new object[] { n }))
				{
					intran.ARDocType = null;
					intran.ARRefNbr = null;
					intran.ARLineNbr = null;

					Base1.intranselect.Cache.MarkUpdated(intran, assertError: true);

					Base1.PostShippedNotInvoiced(intran, n, ardoc, je);
				}
			}
			else
			{
				baseMethod(n, ardoc, je);
			}
		}

		public delegate List<ARRegister> ReleaseInvoiceDelegate(
			JournalEntry je,
			ARRegister doc,
			PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res,
			List<PMRegister> pmDocs);

		/// <summary>
		/// Overrides <see cref="ARReleaseProcess.ReleaseInvoice"/>
		/// </summary>
		[PXOverride]
		public virtual List<ARRegister> ReleaseInvoice(
			JournalEntry je,
			ARRegister doc,
			PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res,
			List<PMRegister> pmDocs,
			ReleaseInvoiceDelegate baseMethod)
		{
			EnsureCanReleaseOrThrow(doc);

			// special handling for zero invoice correction
			ARInvoice ardoc = res;
			bool zeroCancellationInv = (ardoc.IsCancellation == true
				&& ardoc.CuryOrigDocAmt == 0m
				&& !string.IsNullOrEmpty(ardoc.OrigRefNbr));

			var ret = baseMethod(je, doc, res, pmDocs);

			if (zeroCancellationInv && ardoc.OpenDoc == false)
			{
				ARRegister origInvoice = PXSelect<ARRegister,
					Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>
					.Select(Base, ardoc.OrigDocType, ardoc.OrigRefNbr);

				if (origInvoice.IsUnderCorrection == true)
				{
					origInvoice.Canceled = true;
					origInvoice = Base.ARDocument.Update(origInvoice);

					PXDatabase.Update<ARTran>(
						new PXDataFieldAssign<ARTran.canceled>(PXDbType.Bit, true),
						new PXDataFieldRestrict<ARTran.tranType>(PXDbType.Char, origInvoice.DocType),
						new PXDataFieldRestrict<ARTran.refNbr>(PXDbType.NVarChar, origInvoice.RefNbr),
						new PXDataFieldRestrict<ARTran.canceled>(PXDbType.Bit, false));
				}
			}

			return ret;
		}

		protected virtual void EnsureCanReleaseOrThrow(ARRegister doc)
		{
			if (doc.DocType == ARDocType.CreditMemo && doc.IsCancellation != true)
			{
				ARAdjust2 cancellationAdjust = SelectFrom<ARAdjust2>.
					InnerJoin<ARAdjust>.
					On<ARAdjust.adjdRefNbr.IsEqual<ARAdjust2.adjdRefNbr>.
						And<ARAdjust.adjdDocType.IsEqual<ARAdjust2.adjdDocType>>>.
					InnerJoin<ARRegister>.
					On<ARRegister.refNbr.IsEqual<ARAdjust2.adjgRefNbr>.
						And<ARRegister.docType.IsEqual<ARAdjust2.adjgDocType>>>.
					Where<ARAdjust.adjgDocType.IsEqual<ARRegister.docType.FromCurrent>.
						And<ARAdjust.adjgRefNbr.IsEqual<ARRegister.refNbr.FromCurrent>>.
						And<ARAdjust.voided.IsNotEqual<True>>.
						And<ARAdjust2.adjgRefNbr.IsNotEqual<ARRegister.refNbr.FromCurrent>.
							Or<ARAdjust2.adjgDocType.IsNotEqual<ARRegister.docType.FromCurrent>>>.
						And<ARAdjust2.voided.IsNotEqual<True>>.
						And<ARRegister.docType.IsEqual<ARDocType.creditMemo>>.
						And<ARRegister.isCancellation.IsEqual<True>>>
					.View.ReadOnly.SelectSingleBound(Base, new[] { doc });

				if (cancellationAdjust != null)
				{
					throw new PXException(Messages.CantCreateApplicationToInvoiceUnderCorrection, cancellationAdjust.AdjdRefNbr);
				}
			}
			else if (doc.IsCancellation == true && doc.OrigRefNbr != null && doc.OrigDocType != null)
			{
				var arAdjustGroups = SelectFrom<ARAdjust>.
										Where<ARAdjust.adjdDocType.IsEqual<ARRegister.origDocType.FromCurrent>.
											And<ARAdjust.adjdRefNbr.IsEqual<ARRegister.origRefNbr.FromCurrent>>.
											And<ARAdjust.adjgDocType.IsNotEqual<ARRegister.docType.FromCurrent>.
												Or<ARAdjust.adjgRefNbr.IsNotEqual<ARRegister.refNbr.FromCurrent>>>.
											And<ARAdjust.voided.IsNotEqual<True>>>.
										Aggregate<To<GroupBy<ARAdjust.adjgDocType>,
											GroupBy<ARAdjust.adjgRefNbr>,
											GroupBy<ARAdjust.released>,
											Sum<ARAdjust.curyAdjdAmt>>>
										.View.ReadOnly.SelectMultiBound(Base, new[] { doc })
										.RowCast<ARAdjust>().ToList();

				if (arAdjustGroups.Any(a => a.Released == false))
				{
					throw new PXException(Messages.CantCancelInvoiceWithUnreleasedApplications);
				}

				var nonReversedCreditMemo = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m && a.AdjgDocType == ARDocType.CreditMemo);
				if (nonReversedCreditMemo != null)
				{
					throw new PXException(Messages.CantCancelInvoiceWithCM, nonReversedCreditMemo.AdjdRefNbr, nonReversedCreditMemo.AdjgRefNbr);
				}

				var nonReversedApplication = arAdjustGroups.FirstOrDefault(a => a.CuryAdjdAmt != 0m);
				if (nonReversedApplication != null)
				{
					throw new PXException(Messages.CantCancelInvoiceWithPayment, nonReversedApplication.AdjdRefNbr, nonReversedApplication.AdjgRefNbr);
				}
			}
		}
	}
}
