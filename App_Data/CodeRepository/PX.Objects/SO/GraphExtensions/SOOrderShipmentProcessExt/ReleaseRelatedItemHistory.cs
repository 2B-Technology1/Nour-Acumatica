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
using PX.Objects.CS;
using PX.Objects.IN.RelatedItems;
using PX.Objects.SO.GraphExtensions.SOInvoiceEntryExt;
using PX.Objects.SO.GraphExtensions.SOOrderEntryExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO.GraphExtensions.SOOrderShipmentProcessExt
{
    public class ReleaseRelatedItemHistory: ReleaseRelatedItemHistory<SOOrderShipmentProcess>
    {
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.relatedItems>();

        /// <summary>
        /// Overrides <see cref="SOOrderShipmentProcess.OnInvoiceReleased(ARRegister, List{PXResult{SOOrderShipment, SOOrder}})"/>
        /// </summary>
        [PXOverride]
        public virtual void OnInvoiceReleased(ARRegister ardoc, List<PXResult<SOOrderShipment, SOOrder>> orderShipments, Action<ARRegister, List<PXResult<SOOrderShipment, SOOrder>>> baseImpl)
        {
            baseImpl(ardoc, orderShipments);

            if (PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>())
                ReleaseRelatedItemHistoryFromInvoice(ardoc);

            if (orderShipments.Any())
                ReleaseRelatedItemHistoryFromOrder(ardoc);

            if (Base.IsDirty)
                Base.Save.Press();
        }
    }
}
