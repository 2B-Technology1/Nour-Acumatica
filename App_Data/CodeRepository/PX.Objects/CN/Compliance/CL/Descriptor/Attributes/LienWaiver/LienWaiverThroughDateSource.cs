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
using PX.Data.BQL;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
    public class LienWaiverThroughDateSource
    {
        public const string BillDate = "AP Bill Date";
        public const string PostingPeriodEndDate = "Posting Period End Date";
        public const string PaymentDate = "AP Check Date";

        private static readonly string[] ThroughDateSources =
        {
            BillDate,
            PostingPeriodEndDate,
            PaymentDate
        };

		private static readonly string[] ThroughDateSourcesLabels =
{
			ComplianceLabels.LienWaiverSetup.LienWaiverThroughDateSource_BillDate,
			ComplianceLabels.LienWaiverSetup.LienWaiverThroughDateSource_PostingPeriodEndDate,
			ComplianceLabels.LienWaiverSetup.LienWaiverThroughDateSource_PaymentDate
		};

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(ThroughDateSources, ThroughDateSourcesLabels)
            {
            }
        }

        public sealed class billDate : BqlString.Constant<billDate>
        {
            public billDate()
                : base(BillDate)
            {
            }
        }

        public sealed class postingPeriodEndDate : BqlString.Constant<postingPeriodEndDate>
        {
            public postingPeriodEndDate()
                : base(PostingPeriodEndDate)
            {
            }
        }

        public sealed class paymentDate : BqlString.Constant<paymentDate>
        {
            public paymentDate()
                : base(PaymentDate)
            {
            }
        }
    }
}
