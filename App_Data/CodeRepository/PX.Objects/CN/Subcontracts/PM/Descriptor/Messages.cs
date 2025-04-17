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

namespace PX.Objects.CN.Subcontracts.PM.Descriptor
{
    [PXLocalizable(Prefix)]
    public static class Messages
    {
        public const string Subcontract = "Subcontract";

        private const string Prefix = "PM Error";

        public static class PmTask
        {
            public const string TaskTypeIsNotAvailable = "Task Type is not valid";
        }

        public static class PmCommitment
        {
            public const string RelatedDocumentType = "Related Document Type";
            public const string PurchaseOrderType = "POOrder";
            public const string SalesOrderType = "SOOrder";
            public const string SubcontractType = "Subcontract";
            public const string PurchaseOrderLabel = "Purchase Order";
            public const string SalesOrderLabel = "Sales Order";
            public const string SubcontractLabel = "Subcontract";
        }

        public static class ChangeOrders
        {
            public const string CommitmentNbr = "Commitment Nbr.";
            public const string CommitmentLineNbr = "Commitment Line Nbr.";
        }

        public static class PmChangeOrderLine
        {
            public const string CommitmentType = "Commitment Type";
        }

        public const string CreateSubcontract = "Create Subcontract";
	}
}