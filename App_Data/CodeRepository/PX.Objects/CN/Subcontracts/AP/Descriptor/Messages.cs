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

using PX.Common;

namespace PX.Objects.CN.Subcontracts.AP.Descriptor
{
    [PXLocalizable(Prefix)]
    public class Messages
    {
        public const string AddSubcontract = "Add Subcontract";
        public const string AddSubcontractLine = "Add Subcontract Line";
        public const string ViewSubcontract = "View Subcontract";
        public const string ViewPoOrder = "View PO Order";
        public const string SubcontractViewName = "Subcontract";

        public const string FailedToAddSubcontractLinesError =
			"One subcontract line or multiple subcontract lines cannot be added to the bill. See Trace Log for details.";

        public const string AutoApplyRetainageCheckBox = "The Apply Retainage check box is selected automatically " +
            "because you have added one or more lines with a retainage from the purchase order or subcontract.";

        private const string Prefix = "AP Error";

        public static class LinkLineFilterMode
        {
            public const string PurchaseOrderOrSubcontract = "Purchase Order / Subcontract";
        }

        public static class Subcontract
        {
            public const string SubcontractNumber = "Subcontract Nbr.";
            public const string SubcontractTotal = "Subcontract Total";
            public const string SubcontractBilledQty = "Total Billed Qty.";
            public const string SubcontractBilledTotal = "Total Billed Amount";
            public const string Project = "Project";
            public const string SubcontractLine = "Subcontract Line";
            public const string SubcontractDate = "Date";
        }

        public static class FieldClass
        {
            public const string Distribution = "DISTR";
        }
    }
}
