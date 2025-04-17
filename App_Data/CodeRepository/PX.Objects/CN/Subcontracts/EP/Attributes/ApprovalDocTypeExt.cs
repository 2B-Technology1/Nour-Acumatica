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
using PX.Data;
using PX.Objects.CN.Subcontracts.EP.Descriptor;
using PX.Objects.CN.Subcontracts.PO.Extensions;
using PX.Objects.EP;

namespace PX.Objects.CN.Subcontracts.EP.Attributes
{
    public class ApprovalDocTypeExt : ApprovalDocType<EPApprovalProcess.EPOwned.entityType, EPApproval.sourceItemType>
    {
        public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
        {
            var epOwned = item as EPApprovalProcess.EPOwned;
            return epOwned.GetSubcontractEntity(cache.Graph) != null
                ? Constants.SubcontractDocumentType
                : base.Evaluate(cache, item, pars);
        }
    }
}