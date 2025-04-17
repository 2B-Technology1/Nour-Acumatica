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

namespace PX.Objects.AP
{
	public static class APRegisterClassExtensions
	{
		/// <summary>
		/// Returns an enumerable string array, which is included all 
		/// possible original document types for voiding document.
		/// </summary>
		public static IEnumerable<string> PossibleOriginalDocumentTypes(this APRegister voidcheck)
		{
			switch (voidcheck.DocType)
			{
				case APDocType.VoidCheck:
                case APDocType.VoidRefund:
				case APDocType.VoidQuickCheck:
                    return APPaymentType.GetVoidedAPDocType(voidcheck.DocType);

				default:
					return new[] { voidcheck.DocType };
			}
		}

		/// <summary>
		/// Indicates whether the record is an original Retainage document
		/// with <see cref="APRegister.RetainageApply"/> flag equal to true
		/// </summary>
		public static bool IsOriginalRetainageDocument(this APRegister doc)
		{
			return doc.RetainageApply == true;
		}

		/// <summary>
		/// Indicates whether the record is a child Retainage document
		/// with <see cref="APRegister.IsRetainageDocument"/> flag equal to true
		/// and existing reference on the original Retainage document.
		/// </summary>
		public static bool IsChildRetainageDocument(this APRegister doc)
		{
			return
				doc.IsRetainageDocument == true &&
				!string.IsNullOrEmpty(doc.OrigDocType) &&
				!string.IsNullOrEmpty(doc.OrigRefNbr);
		}

		/// <summary>
		/// Indicates whether the record has zero document balance and zero lines balances
		/// depending on <see cref="APRegister.PaymentsByLinesAllowed"/> flag value.
		/// </summary>
		public static bool HasZeroBalance<TDocBal, TLineBal>(this APRegister document, PXGraph graph)
			where TDocBal : IBqlField
			where TLineBal : IBqlField
		{
			PXCache cache = graph.Caches[typeof(APRegister)];
			return (cache.GetValue<TDocBal>(document) as decimal? ?? 0m) == 0m && (document.PaymentsByLinesAllowed != true ||
				// Select + AsEnumerable here to check Any() on merged cache.
				!PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
						And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
						And<TLineBal, NotEqual<decimal0>>>>>
					.Select(graph, document.DocType, document.RefNbr).AsEnumerable().Any());
		}
	}
}
