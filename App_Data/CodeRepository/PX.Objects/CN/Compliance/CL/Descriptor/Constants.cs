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

using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.Descriptor
{
    public class Constants
    {
        public const string ComplianceDocumentViewName = "ComplianceDocuments";
        public const string LienWaiverReportFileNamePattern = "{0}\\LW-{1}-{2}-{3:MM-dd-yyyy}{4}";
        public const string LienWaiverReportFileNameSearchPattern = "{0}\\LW-{1}-{2}";

        public class LienWaiverDocumentTypeValues
        {
			public const string All = "All";
			public const string ConditionalPartial = "Conditional Partial";
            public const string ConditionalFinal = "Conditional Final";
            public const string UnconditionalPartial = "Unconditional Partial";
            public const string UnconditionalFinal = "Unconditional Final";

			public class all : BqlString.Constant<all>
			{
				public all()
					: base(All)
				{
				}
			}
		}

		public class LienWaiverReportParameters
        {
            public const string ComplianceDocumentId = "ComplianceDocumentId";
            public const string DeviceHubComplianceDocumentId = "ComplianceDocument.ComplianceDocumentID";
            public const string IsJointCheck = "IsJointCheck";
        }

        public class ComplianceNotification
        {
            public const string LienWaiverNotificationSourceCd = "Vendor";
        }
    }
}
