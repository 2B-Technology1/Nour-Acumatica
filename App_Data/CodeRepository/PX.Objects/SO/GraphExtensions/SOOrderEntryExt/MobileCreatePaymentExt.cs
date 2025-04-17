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
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.CS;
using PX.Objects.Extensions.PaymentTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOOrderEntryExt
{
    public class MobileCreatePaymentExt : PXGraphExtension<CreatePaymentExt, SOOrderEntry>
    {
        #region Buttons

        public PXAction<SOOrder> mobileCreatePayment;
		[PXUIField(DisplayName = "Create Payment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Payment", DisplayOnMainToolbar = false, OnClosingPopup = PXSpecialButtonType.Cancel)]
		protected virtual void MobileCreatePayment()
		{
			if (Base.Document.Current != null)
			{
				Base1.CheckTermsInstallmentType();

				Base.Save.Press();

				PXGraph target;
				MobileCreatePaymentProc(Base.Document.Current, out target);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

		public PXAction<SOOrder> mobileCreatePrepayment;
		[PXUIField(DisplayName = "Create Prepayment", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.AddNew, Tooltip = "Create Prepayment", DisplayOnMainToolbar = false, OnClosingPopup = PXSpecialButtonType.Cancel)]
		protected virtual void MobileCreatePrepayment()
		{
			if (Base.Document.Current != null)
			{
				Base1.CheckTermsInstallmentType();

				Base.Save.Press();

				PXGraph target;
				MobileCreatePaymentProc(Base.Document.Current, out target, ARPaymentType.Prepayment);

				throw new PXPopupRedirectException(target, "New Payment", true);
			}
		}

        #endregion // Buttons

        #region SOOrder events

		protected virtual void _(Events.RowSelected<SOOrder> eventArgs)
        {
			if (eventArgs.Row == null)
				return;

			var orderType = Base.soordertype.Current;
			bool paymentsEnabled = (orderType.CanHavePayments == true) && Base.IsAddingPaymentsAllowed(eventArgs.Row, Base.soordertype.Current);
			mobileCreatePayment.SetEnabled(paymentsEnabled && eventArgs.Cache.GetStatus(eventArgs.Row) != PXEntryStatus.Inserted);
			mobileCreatePrepayment.SetEnabled(paymentsEnabled && eventArgs.Cache.GetStatus(eventArgs.Row) != PXEntryStatus.Inserted);
		}

        #endregion // SOOrder events

        #region Methods

		public virtual void MobileCreatePaymentProc(SOOrder order, out PXGraph target, string paymentType = ARPaymentType.Payment)
		{
			ARPaymentEntry docgraph = PXGraph.CreateInstance<ARPaymentEntry>();
			target = docgraph;

			docgraph.Clear();
			ARPayment payment = new ARPayment()
			{
				DocType = paymentType,				
			};
			docgraph.arsetup.Current.HoldEntry = false;

			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, true);
			payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
			AROpenPeriodAttribute.SetThrowErrorExternal<ARPayment.adjFinPeriodID>(docgraph.Document.Cache, false);

			payment.CustomerID = order.CustomerID;
			payment.CustomerLocationID = order.CustomerLocationID;
			payment.PaymentMethodID = order.PaymentMethodID;
			payment.PMInstanceID = order.PMInstanceID;
			payment.CuryOrigDocAmt = 0m;
			payment.DocDesc = order.OrderDesc;
			payment.CashAccountID = order.CashAccountID;
			payment = docgraph.Document.Update(payment);

			MobileInsertSOAdjustments(order, docgraph, payment);

			if (payment.CuryOrigDocAmt == 0m)
			{
				payment.CuryOrigDocAmt = payment.CurySOApplAmt;
				payment = docgraph.Document.Update(payment);
			}
		}

		protected virtual void MobileInsertSOAdjustments(SOOrder order, ARPaymentEntry docgraph, ARPayment payment)
		{
			SOAdjust adj = new SOAdjust()
			{
				AdjdOrderType = order.OrderType,
				AdjdOrderNbr = order.OrderNbr
			};

			try
			{
				docgraph.GetOrdersToApplyTabExtension(true).SOAdjustments.Insert(adj);
			}
			catch (PXSetPropertyException)
			{
				payment.CuryOrigDocAmt = 0m;
			}
		}

		#endregion Methods
	}
}
