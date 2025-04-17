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
using PX.Objects.CC.PaymentProcessing.Helpers;
using PX.Objects.CS;
using PX.Objects.SO;
using System.Linq;
using PX.Objects.AR;
using PX.Objects.Common;

namespace PX.Objects.CC.GraphExtensions
{
	public class ARPaymentEntryPayLink : PXGraphExtension<ARPaymentEntry>
	{
		public bool DoNotSetNeedSync { get; set; }

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.acumaticaPayments>();
		}

		private void _(Events.RowPersisted<SOAdjust> e)
		{
			if (DoNotSetNeedSync) return;

			if (!(e.TranStatus == PXTranStatus.Open &&
				(e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update || e.Operation == PXDBOperation.Delete))) return;
			
			var adj = e.Row;
			var order = Base.Caches[typeof(SOOrder)].Cached.RowCast<SOOrder>()
				.Where(i => i.OrderNbr == adj.AdjdOrderNbr && i.OrderType == adj.AdjdOrderType).FirstOrDefault();

			if (order == null) return;
				
			var orderExt = Base.Caches[typeof(SOOrder)].GetExtension<SOOrderPayLink>(order);
			if (orderExt?.PayLinkID == null) return;
				
			CCPayLink link = PXSelect<CCPayLink, Where<CCPayLink.payLinkID, Equal<Required<CCPayLink.payLinkID>>>>.Select(Base, orderExt.PayLinkID);
			if (link.Amount != order.CuryUnpaidBalance && link.NeedSync == false)
			{
				var payment = Base.Document.Current;
				SetSyncFlagIfNeeded(payment, link);
				if (link.NeedSync == true)
				{
					if (e.TranStatus == PXTranStatus.Open)
					{
						Base.Caches[typeof(CCPayLink)].PersistUpdated(link);
					}
				}
			}
		}

		private void SetSyncFlagIfNeeded(ARPayment payment, CCPayLink payLink)
		{
			if (!PayLinkHelper.PayLinkOpen(payLink)) return;

			if (payment?.CCActualExternalTransactionID != null)
			{
				var extTranExists = new PXSelect<ExternalTransaction,
					Where<ExternalTransaction.transactionID, Equal<Required<ExternalTransaction.transactionID>>,
					And<ExternalTransaction.payLinkID, Equal<Required<ExternalTransaction.payLinkID>>>>>(Base)
					.Any(payment.CCActualExternalTransactionID, payLink.PayLinkID);

				if (!extTranExists)
				{
					payLink.NeedSync = true;
					Base.Caches[typeof(CCPayLink)].Update(payLink);
				}
			}
			else
			{
				payLink.NeedSync = true;
				Base.Caches[typeof(CCPayLink)].Update(payLink);
			}
		}
	}
}
