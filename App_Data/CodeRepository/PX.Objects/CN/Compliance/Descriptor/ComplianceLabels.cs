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

namespace PX.Objects.CN.Compliance.Descriptor
{
    [PXLocalizable]
    public class ComplianceLabels
    {
        [PXLocalizable]
        public static class Subcontract
        {
            public const string SubcontractNumber = "Subcontract Nbr.";
            public const string SubcontractTotal = "Subcontract Total";
            public const string VendorReference = "Vendor Ref.";
            public const string Date = "Date";
            public const string Status = "Status";
            public const string Vendor = "Vendor";
            public const string VendorName = "Vendor Name";
            public const string Location = "Location";
            public const string Currency = "Currency";
        }

        [PXLocalizable]
        public static class LienWaiverSetup
        {
            public const string AutomaticallyGenerateLienWaivers = "Automatically Generate Lien Waivers";
            public const string GenerateWithoutCommitment = "Generate for AP Documents Not Linked to Commitments";
            public const string GenerateLienWaiversOn = "Generate Lien Waivers on";
            public const string ThroughDate = "Through Date";
            public const string GroupBy = "Calculate Amount By";
			public const string LienWaiverThroughDateSource_BillDate = "Bill Date";
			public const string LienWaiverThroughDateSource_PostingPeriodEndDate = "Posting Period End Date";
			public const string LienWaiverThroughDateSource_PaymentDate = "AP Payment Date";
			public const string CommitmentProjectTask = "Commitment, Project, Project Task";
			public const string CommitmentProject = "Commitment, Project";
			public const string ProjectTask = "Project, Project Task";
			public const string Project = "Project";
        }
    }
}
