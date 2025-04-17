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
using static PX.Objects.TX.CSTaxCalcType;

namespace PX.Objects.CA
{
	public static class AddARTransactionHelper
	{
		public static void TryToSetBranch(this ARPayment payment, ARPaymentEntry graph, ICADocSource parameters, BranchSource branchSource)
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
					payment.BranchID = location.CBranchID;
					break;
				case BranchSource.AccessInfo:
					payment.BranchID = graph.Accessinfo.BranchID;
					break;
			}

			if(payment.BranchID == null)
			{
				payment.BranchID = graph.Accessinfo.BranchID;
			}
		}

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R2)]
		public static ARPayment InitializeARPayment(ARPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, bool aOnHold)
			=> InitializeARPayment(graph, parameters, aCuryInfo, aOnHold, BranchSource.AccessInfo);

		public static ARPayment InitializeARPayment(ARPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, bool aOnHold, BranchSource branchSource)
		{
			ARPayment doc = new ARPayment();

			if (!(parameters.DrCr == CADrCr.CACredit ^ Math.Sign(parameters.CuryOrigDocAmt.Value) > 0))
			{
				doc.DocType = ARDocType.Refund;
			}
			else
			{
				doc.DocType = ARDocType.Payment;
			}
			doc = PXCache<ARPayment>.CreateCopy(graph.Document.Insert(doc));
			doc.TryToSetBranch(graph, parameters, branchSource);
			doc.CustomerID = parameters.BAccountID;
			doc.CustomerLocationID = parameters.LocationID;
			doc.PaymentMethodID = parameters.PaymentMethodID;
			doc.PMInstanceID = parameters.PMInstanceID;
			doc.CashAccountID = parameters.CashAccountID;
			doc.CuryOrigDocAmt = Math.Abs(parameters.CuryOrigDocAmt.Value - (parameters.ChargeDrCr == parameters.DrCr ? 1 : -1) * (parameters.CuryChargeAmt ?? 0m));
			doc.DocDesc = parameters.TranDesc;
			doc.Cleared = parameters.Cleared;
			if (doc.Cleared == true)
			{
				doc.ClearDate = parameters.ClearDate;
			}
			doc.AdjDate = parameters.MatchingPaymentDate;
			doc.FinPeriodID = parameters.FinPeriodID;
			doc.AdjFinPeriodID = parameters.FinPeriodID;
			doc.ExtRefNbr = parameters.ExtRefNbr;
			doc.CARefTranAccountID = parameters.CARefTranAccountID;
			doc.CARefTranID = parameters.CARefTranID;
			doc.CARefSplitLineNbr = parameters.CARefSplitLineNbr;
			doc.Hold = aOnHold;
			if (aCuryInfo == null)
				doc.CuryID = parameters.CuryID;
			doc.RefNoteID = parameters.NoteID;
			doc = PXCache<ARPayment>.CreateCopy(graph.Document.Update(doc));

			var sourceCache = graph.Caches[parameters.GetType()];
			PXNoteAttribute.CopyNoteAndFiles(sourceCache, parameters, graph.Document.Cache, doc);
			doc = PXCache<ARPayment>.CreateCopy(graph.Document.Update(doc));

			return doc;
		}

		public static void InitializeCurrencyInfo(ARPaymentEntry graph, ICADocSource parameters, CurrencyInfo aCuryInfo, ARPayment doc)
		{
			CurrencyInfo refInfo = aCuryInfo
							?? PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(graph, parameters.CuryInfoID);

			if (refInfo != null)
			{
				foreach (CM.Extensions.CurrencyInfo info in graph.currencyinfo.Select())
				{
					PXCache<CM.Extensions.CurrencyInfo>.RestoreCopy(info, CM.Extensions.CurrencyInfo.GetEX(refInfo));
					info.CuryInfoID = doc.CuryInfoID;
					doc.CuryID = info.CuryID;
				}
			}
		}

		public static decimal InitializeARAdjustment(ARPaymentEntry graph, ARAdjust adjustment, decimal curyAppliedAmt)
		{
			ARAdjust adjust = new ARAdjust();
			adjust.AdjdDocType = adjustment.AdjdDocType;
			adjust.AdjdRefNbr = adjustment.AdjdRefNbr;
			adjust.AdjdLineNbr = adjustment.AdjdLineNbr;
			try
			{
				adjust = graph.Adjustments.Insert(adjust);
			}
			catch (PXFieldValueProcessingException e)
			{
				ARAdjust existingAdjust = PXSelectReadonly<ARAdjust, Where<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.released, Equal<False>>>>>.Select(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
				ARInvoice invoice = PXSelectReadonly<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARInvoice.docType, Equal<Required<ARAdjust.adjdDocType>>>>>.Select(graph, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
				if (existingAdjust != null || invoice.Status == ARDocStatus.Closed)
					throw new PXException(Messages.CouldNotAddApplication, adjustment.AdjdRefNbr);
				else throw e;
			}

			adjust.AdjdCuryRate = adjustment.AdjdCuryRate;
			adjust = graph.Adjustments.Update(adjust);
			if (adjust.CuryAdjgAmt.HasValue)
			{
				adjust.CuryAdjgAmt = adjustment.CuryAdjgAmt ?? adjust.CuryAdjgAmt ?? 0m;
				curyAppliedAmt += (adjust.CuryAdjgAmt ?? Decimal.Zero);
			}
			adjust.WriteOffReasonCode = adjustment.WriteOffReasonCode ?? adjust.WriteOffReasonCode;
			adjust.WOBal = adjustment.WOBal ?? adjust.WOBal;
			adjust.AdjWOAmt = adjustment.AdjWOAmt ?? adjust.AdjWOAmt;
			adjust.CuryAdjdWOAmt = adjustment.CuryAdjdWOAmt ?? adjust.CuryAdjdWOAmt;
			adjust.CuryAdjgWOAmt = adjustment.CuryAdjgWOAmt ?? adjust.CuryAdjgWOAmt;
			adjust.CuryWOBal = adjustment.CuryWOBal ?? adjust.CuryWOBal;
			adjust.CuryAdjgDiscAmt = adjustment.CuryAdjgDiscAmt ?? adjust.CuryAdjgDiscAmt ?? 0m;
			adjust.CuryAdjgWOAmt = adjustment.CuryAdjgWOAmt ?? adjust.CuryAdjgWOAmt ?? 0m;
			graph.Adjustments.Update(adjust);
			return curyAppliedAmt;
		}
	}
}
