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
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.CM.Extensions
{
	internal class APPaymentBalanceCalculator : AbstractPaymentBalanceCalculator<APAdjust,APTran>
	{
		public APPaymentBalanceCalculator(PXSelectBase<CM.CurrencyInfo> curyInfoSelect) : this(new CM.CuryHelper(curyInfoSelect))
		{
		}

		public APPaymentBalanceCalculator(IPXCurrencyHelper curyhelper) : base(curyhelper)
		{
		}

		protected override void AfterBalanceCalculatedBeforeBalanceAjusted<T>(APAdjust adj, T voucher, bool DiscOnDiscDate, APTran tran)
		{
			adj.CuryOrigDocAmt = tran?.CuryOrigTranAmt ?? voucher.CuryOrigDocAmt;
			adj.OrigDocAmt = tran?.OrigTranAmt ?? voucher.OrigDocAmt;

			if (DiscOnDiscDate) PaymentEntry.CalcDiscount(adj.AdjgDocDate, voucher, adj);

			base.AfterBalanceCalculatedBeforeBalanceAjusted(adj, voucher, DiscOnDiscDate, tran);
		}

		protected override bool ShouldRgolBeResetInZero(APAdjust adj) =>
			(adj.AdjgDocType == APDocType.Check || adj.AdjgDocType == APDocType.VoidCheck || adj.AdjgDocType == APDocType.Prepayment)
			&& adj.AdjdDocType == APDocType.Prepayment;
	}
}
