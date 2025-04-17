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

using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public sealed class ChangeOrderSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public ChangeOrderSelectorAttribute()
            : base(typeof(PMChangeOrder.refNbr),
                typeof(PMChangeOrder.refNbr),
                typeof(PMChangeOrder.status),
                typeof(PMChangeOrder.projectNbr),
                typeof(PMChangeOrder.date),
                typeof(PMChangeOrder.completionDate),
                typeof(PMChangeOrder.description))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedChangeOrderNumbers = _Graph.GetExtension<DailyFieldReportEntryChangeOrderExtension>()
                .ChangeOrders.SelectMain().Select(co => co.ChangeOrderId);
            return GetRelatedEntities<PMChangeOrder, PMChangeOrder.projectID>()
                .Where(co => co.RefNbr.IsNotIn(linkedChangeOrderNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            RelatedEntitiesIds = GetRelatedEntities<PMChangeOrder>().Select(co => co.RefNbr);
            base.FieldVerifying(cache, args);
        }
    }
}
