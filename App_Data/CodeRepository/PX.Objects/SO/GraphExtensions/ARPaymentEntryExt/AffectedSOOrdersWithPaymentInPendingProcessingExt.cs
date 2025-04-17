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
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.Extensions;
using PX.Objects.CS;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
    public class AffectedSOOrdersWithPaymentInPendingProcessingExt : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedSOOrdersWithPaymentInPendingProcessingExt, ARPaymentEntry, SOOrder, SOOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.distributionModule>();
        }

        private PXCache<SOOrder> orders => Base.Caches<SOOrder>();
        protected override bool PersistInSameTransaction => false;
        private readonly IDictionary<(string orderType, string orderNbr), int?> _oldPaymentsNeedValidationCntrValues = new Dictionary<(string orderType, string orderNbr), int?>();

        protected override bool EntityIsAffected(SOOrder order)
        {
            int? oldPaymentsNeedValidationCntr = (int?)orders.GetValueOriginal<SOOrder.paymentsNeedValidationCntr>(order);
            if (!Equals(oldPaymentsNeedValidationCntr, order.PaymentsNeedValidationCntr))
            {
                _oldPaymentsNeedValidationCntrValues[(order.OrderType, order.OrderNbr)] = oldPaymentsNeedValidationCntr;
                return true;
            }
            else
                return false;
        }

        protected override void ProcessAffectedEntity(SOOrderEntry orderEntry, SOOrder order)
        {
            int? oldPaymentsNeedValidationCntr = _oldPaymentsNeedValidationCntrValues[(order.OrderType, order.OrderNbr)];
            if (order.PaymentsNeedValidationCntr == 0 && oldPaymentsNeedValidationCntr != null)
                SOOrder.Events.Select(e => e.LostLastPaymentInPendingProcessing).FireOn(orderEntry, order);
            else if (oldPaymentsNeedValidationCntr < order.PaymentsNeedValidationCntr)
                SOOrder.Events.Select(e => e.ObtainedPaymentInPendingProcessing).FireOn(orderEntry, order);
        }

        protected override SOOrder ActualizeEntity(SOOrderEntry orderEntry, SOOrder order)
			=> orderEntry.Document.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderType);
    }
}
