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
using PX.Objects.AR;
using PX.Objects.CA.Descriptor;
using PX.Objects.CM;
using PX.Objects.CR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.PM.PMBillingRule;

namespace PX.Objects.CA
{
	public static class AddAPTransactionHelper
	{
		public static void TryToSetBranch(this APPayment payment, APPaymentEntry graph, ICADocSource parameters, BranchSource branchSource)
		{
			switch (branchSource)
			{
				case BranchSource.CustomerVendorLocation:
					var cashAccount = CashAccount.PK.Find(graph, parameters.CashAccountID);
					var location = Location.PK.Find(graph, parameters.BAccountID, parameters.LocationID);
					if (cashAccount.RestrictVisibilityWithBranch == true && cashAccount.BranchID != location.CBranchID)
					{
						break;
					}
					payment.BranchID = location.VBranchID;
					break;
				case BranchSource.AccessInfo:
					payment.BranchID = graph.Accessinfo.BranchID;
					break;
			}

			if (payment.BranchID == null)
			{
				payment.BranchID = graph.Accessinfo.BranchID;
			}
		}

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R2)]
		public static APPayment InitializeAPPayment(APPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, IList<ICADocAdjust> aAdjustments, bool aOnHold)
			=> InitializeAPPayment(graph, parameters, aCuryInfo, aAdjustments, aOnHold, BranchSource.AccessInfo);

		public static APPayment InitializeAPPayment(APPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, IList<ICADocAdjust> aAdjustments, bool aOnHold, BranchSource branchSource)
		{
			APPayment doc = new APPayment();

			if (!(parameters.DrCr == CADrCr.CACredit ^ Math.Sign(parameters.CuryOrigDocAmt.Value) > 0))
			{
				decimal? sumOfAdjustments = aAdjustments == null ? 0.0m : aAdjustments
					.Select(adj => new APAdjust { AdjgDocType = APDocType.Check, AdjdDocType = adj.AdjdDocType, CuryAdjgAmt = adj.CuryAdjgAmount })
					.Sum(adj => adj.AdjgBalSign * adj.CuryAdjgAmt);

				if (sumOfAdjustments == (parameters.CuryOrigDocAmt - ((parameters.ChargeDrCr == parameters.DrCr ? 1 : -1) * parameters.CuryChargeAmt ?? 0m)))
				{
					doc.DocType = APDocType.Check;
				}
				else
				{
					doc.DocType = APDocType.Prepayment;

					if (aAdjustments != null && aAdjustments.Any(adj => adj.AdjdDocType == APDocType.DebitAdj || adj.AdjdDocType == APDocType.Prepayment))
						throw new PXException(Messages.CanApplyPrepaymentToDrAdjOrPrepayment);
				}

			}
			else
			{
				doc.DocType = APDocType.Refund;
			}

			doc = PXCache<APPayment>.CreateCopy(graph.Document.Insert(doc));
			doc.TryToSetBranch(graph, parameters, branchSource);
			doc.VendorID = parameters.BAccountID;
			doc.VendorLocationID = parameters.LocationID;
			doc.CashAccountID = parameters.CashAccountID;
			doc.PaymentMethodID = parameters.PaymentMethodID;
			doc.AdjDate = parameters.MatchingPaymentDate;
			doc = PXCache<APPayment>.CreateCopy(graph.Document.Update(doc));
			doc.CuryID = parameters.CuryID;
			doc.CuryOrigDocAmt = Math.Abs(parameters.CuryOrigDocAmt ?? 0m) - (parameters.ChargeDrCr == CADrCr.Debit ? -1 : 1) * (parameters.CuryChargeAmt ?? 0m);
			doc.DocDesc = parameters.TranDesc;
			doc.Cleared = parameters.Cleared;
			if (doc.Cleared == true)
			{
				doc.ClearDate = parameters.ClearDate;
			}
			doc.PrintCheck = false;
			doc.Printed = true;
			doc.FinPeriodID = parameters.FinPeriodID;
			doc.AdjFinPeriodID = parameters.FinPeriodID;
			doc.ExtRefNbr = parameters.ExtRefNbr;
			doc.Hold = aOnHold;

			doc.CARefTranAccountID = parameters.CARefTranAccountID;
			doc.CARefTranID = parameters.CARefTranID;
			doc.CARefSplitLineNbr = parameters.CARefSplitLineNbr;

			doc.RefNoteID = parameters.NoteID;

			var sourceCache = graph.Caches[parameters.GetType()];
			PXNoteAttribute.CopyNoteAndFiles(sourceCache, parameters, graph.Document.Cache, doc);

			doc = PXCache<APPayment>.CreateCopy(graph.Document.Update(doc));

			return doc;
		}

		public static void InitializeCurrencyInfo(APPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, APPayment doc)
		{
			CurrencyInfo refInfo = aCuryInfo
					?? PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(graph, parameters.CuryInfoID);

			if (refInfo != null)
			{
				CM.Extensions.CurrencyInfo new_info = CM.Extensions.CurrencyInfo.GetEX(refInfo);
				new_info.CuryInfoID = graph.GetExtension<APPaymentEntry.MultiCurrency>().GetDefaultCurrencyInfo().CuryInfoID;
				graph.currencyinfo.Cache.Update(new_info);
			}
		}

		public static APAdjust InitializeAPAdjustment(APPaymentEntry graph, ICADocAdjust adjustment)
		{
			APAdjust result = null;

			if (adjustment.PaymentsByLinesAllowed == true)
			{
				foreach (APTran tran in
							PXSelect<APTran,
								Where<APTran.tranType, Equal<Current<CABankTranAdjustment.adjdDocType>>,
									And<APTran.refNbr, Equal<Current<CABankTranAdjustment.adjdRefNbr>>,
									And<APTran.curyTranBal, Greater<Zero>>>>>
							.SelectMultiBound(graph, new object[] { adjustment }))
				{
					APAdjust adjust = new APAdjust();
					adjust.AdjdRefNbr = adjustment.AdjdRefNbr;
					adjust.AdjdDocType = adjustment.AdjdDocType;
					adjust.AdjdLineNbr = tran.LineNbr;
					adjust = graph.Adjustments.Insert(adjust);

					result = adjust;
				}
			}
			else
			{
				APAdjust adjust = new APAdjust();
				adjust.AdjdDocType = adjustment.AdjdDocType;
				adjust.AdjdRefNbr = adjustment.AdjdRefNbr;
				adjust.AdjdLineNbr = 0;
				try
				{
					adjust = graph.Adjustments.Insert(adjust);
				}
				catch (PXFieldValueProcessingException e)
				{
					APAdjust existingAdjust = PXSelectReadonly<APAdjust, Where<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>, And<APAdjust.released, Equal<False>>>>>.Select(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
					APInvoice invoice = PXSelectReadonly<APInvoice, Where<APInvoice.refNbr, Equal<Required<APAdjust.adjdRefNbr>>, And<APInvoice.docType, Equal<Required<APAdjust.adjdDocType>>>>>.Select(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
					if (existingAdjust != null || invoice.Status == ARDocStatus.Closed)
						throw new PXException(Messages.CouldNotAddApplication, adjustment.AdjdRefNbr);
					else throw e;
				}
				adjust.AdjdCuryRate = adjustment.AdjdCuryRate;
				adjust = graph.Adjustments.Update(adjust);
				if (adjustment.CuryAdjgAmount.HasValue)
				{
					adjust.CuryAdjgAmt = adjustment.CuryAdjgAmount;
				}
				adjust.CuryAdjgDiscAmt = adjustment.CuryAdjgDiscAmt;
				adjust.CuryAdjgWhTaxAmt = adjustment.CuryAdjgWhTaxAmt;
				adjust = graph.Adjustments.Update(adjust);

				result = adjust;
			}

			return result;
		}
	}
}
