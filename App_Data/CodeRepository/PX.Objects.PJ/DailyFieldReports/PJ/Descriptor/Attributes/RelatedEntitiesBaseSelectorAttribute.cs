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
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class RelatedEntitiesBaseSelectorAttribute : PXCustomSelectorAttribute
    {
        protected IEnumerable<string> RelatedEntitiesIds;

        protected RelatedEntitiesBaseSelectorAttribute(Type type, params Type[] fieldList)
            : base(type, fieldList)
        {
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            if (args.NewValue == null || !ValidateValue || _BypassFieldVerifying.Value)
            {
                return;
            }
            if (RelatedEntitiesIds.All(id => id != args.NewValue.ToString()))
            {
                throw new PXSetPropertyException(SharedMessages.CannotBeFound, $"[{_FieldName}]");
            }
        }

        protected IEnumerable<TTable> GetRelatedEntities<TTable, TProjectField>()
            where TTable : class, IBqlTable, new()
            where TProjectField : BqlInt.Field<TProjectField>
        {
            return SelectFrom<TTable>
                .Where<BqlInt.Field<TProjectField>.IsEqual<DailyFieldReport.projectId.FromCurrent>>.View
                .Select(_Graph).FirstTableItems;
        }

        protected IEnumerable<TTable> GetRelatedEntities<TTable>()
            where TTable : class, IBqlTable, new()
        {
            return SelectFrom<TTable>.View.Select(_Graph).FirstTableItems;
        }
    }
}
