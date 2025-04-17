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
    public sealed class ChangeRequestSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public ChangeRequestSelectorAttribute()
            : base(typeof(PMChangeRequest.refNbr),
                typeof(PMChangeRequest.refNbr),
                typeof(PMChangeRequest.date),
                typeof(PMChangeRequest.extRefNbr),
                typeof(PMChangeRequest.description),
                typeof(PMChangeRequest.costTotal),
                typeof(PMChangeRequest.lineTotal),
                typeof(PMChangeRequest.markupTotal),
                typeof(PMChangeRequest.priceTotal))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedChangeRequestNumbers = _Graph.GetExtension<DailyFieldReportEntryChangeRequestExtension>()
                .ChangeRequests.SelectMain().Select(cr => cr.ChangeRequestId);
            return GetRelatedEntities<PMChangeRequest, PMChangeRequest.projectID>()
                .Where(cr => cr.RefNbr.IsNotIn(linkedChangeRequestNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            RelatedEntitiesIds = GetRelatedEntities<PMChangeRequest>().Select(cr => cr.RefNbr);
            base.FieldVerifying(cache, args);
        }
    }
}