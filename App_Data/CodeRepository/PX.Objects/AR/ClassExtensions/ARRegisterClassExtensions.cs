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
using PX.Objects.CS;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AR
{
	public static class ARRegisterClassExtensions
	{
		/// <summary>
		/// Returns an enumerable string array, which is included all 
		/// possible original document types for voiding document.
		/// </summary>
		public static IEnumerable<string> PossibleOriginalDocumentTypes(this ARRegister voidpayment)
		{
			switch (voidpayment.DocType)
			{
				case ARDocType.CashReturn:
					return new[] { ARDocType.CashSale };

                case ARDocType.VoidRefund:
                case ARDocType.VoidPayment:
					return ARPaymentType.GetVoidedARDocType(voidpayment.DocType);

                default:
					return new[] { voidpayment.DocType };
			}
		}

		/// <summary>
		/// Indicates whether the record is an original Retainage document
		/// with <see cref="ARRegister.RetainageApply"/> flag equal to true.
		/// </summary>
		public static bool IsOriginalRetainageDocument(this ARRegister doc)
		{
			return doc.RetainageApply == true;
		}

		/// <summary>
		/// Indicates whether the record is a child Retainage document
		/// with <see cref="ARRegister.IsRetainageDocument"/> flag equal to true
		/// and existing reference on the original Retainage document.
		/// </summary>
		public static bool IsChildRetainageDocument(this ARRegister doc)
		{
			return
				doc.IsRetainageDocument == true &&
				!string.IsNullOrEmpty(doc.OrigDocType) &&
				!string.IsNullOrEmpty(doc.OrigRefNbr);
		}

		/// <summary>
		/// Indicates whether the record is an Prepayment Invoice document.
		/// </summary>
		public static bool IsPrepaymentInvoiceDocument(this ARRegister doc)
		{
			return doc.DocType == ARDocType.PrepaymentInvoice;
		}

		/// <summary>
		/// Indicates whether the record is a reverse Prepayment Invoice document
		/// by Credit Memo.
		/// </summary>
		public static bool IsPrepaymentInvoiceDocumentReverse(this ARRegister doc)
		{
			return doc.DocType == ARDocType.CreditMemo
				&& doc.OrigDocType == ARDocType.PrepaymentInvoice;
		}

		/// <summary>
		/// Indicates whether the record has zero document balance and zero lines balances
		/// depending on <see cref="ARRegister.PaymentsByLinesAllowed"/> flag value.
		/// </summary>
		public static bool HasZeroBalance<TDocBal, TLineBal>(this ARRegister document, PXGraph graph)
			where TDocBal : IBqlField
			where TLineBal : IBqlField
		{
			PXCache cache = graph.Caches[typeof(ARRegister)];
			if((cache.GetValue<TDocBal>(document) as decimal? ?? 0m) != 0m)
				return false;
				
			if(document.PaymentsByLinesAllowed == true)
			{
				PXCache tranCache = graph.Caches[typeof(ARTran)];
				foreach(ARTran tran in PXSelect<ARTran,
							Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
								And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>
							.Select(graph, document.DocType, document.RefNbr))
				{
					if((tranCache.GetValue<TLineBal>(tran) as decimal? ?? 0m) != 0m)
						return false;
				}
			}

			return true;
		}
	}
}
