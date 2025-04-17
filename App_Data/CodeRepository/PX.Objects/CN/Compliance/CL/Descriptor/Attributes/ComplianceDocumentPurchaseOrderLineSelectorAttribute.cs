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
using System.Collections;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    /// <summary>
    /// Needed as DirtyRead for the PXSelectorAttribute with type = Search2&lt;Joins&gt; didn`t read cached values.
    /// </summary>
    public class ComplianceDocumentPurchaseOrderLineSelectorAttribute : PXCustomSelectorAttribute
    {
        private static readonly Type[] Fields =
        {
            typeof(POLine.lineNbr),
            typeof(POLine.branchID),
            typeof(POLine.inventoryID),
            typeof(POLine.lineType),
            typeof(POLine.tranDesc),
            typeof(POLine.orderQty),
            typeof(POLine.curyUnitCost)
        };

        public ComplianceDocumentPurchaseOrderLineSelectorAttribute()
            : base(typeof(POLine.lineNbr), Fields)
        {
        }

        public IEnumerable GetRecords()
        {
            if (_Graph.Caches[typeof(ComplianceDocument)].Current is ComplianceDocument cache)
            {
                var cachedReferences = _Graph.Caches[typeof(ComplianceDocumentReference)].Cached
                    .Select<ComplianceDocumentReference>();
                var referenceForPurchaseOrder = cachedReferences
                    .Single(reference => reference.ComplianceDocumentReferenceId == cache.PurchaseOrder);
                return GetPurchaseOrdersLines(referenceForPurchaseOrder);
            }
            return Enumerable.Empty<POLine>();
        }

        private PXResultset<POLine> GetPurchaseOrdersLines(ComplianceDocumentReference reference)
        {
            return new PXSelect<POLine,
                    Where<POLine.orderNbr, Equal<Required<POLine.orderNbr>>,
                        And<POLine.orderType, Equal<Required<POLine.orderType>>>>>(_Graph)
                .Select(reference.ReferenceNumber, reference.Type);
        }
    }
}