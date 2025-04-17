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

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class DailyFieldReportStatus
    {
        public const string Hold = "On Hold";
        public const string PendingApproval = "Pending Approval";
        public const string Rejected = "Rejected";
        public const string Completed = "Completed";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] AllowedValues =
            {
                Hold,
                PendingApproval,
                Rejected,
                Completed
            };

            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }

        public sealed class hold : BqlString.Constant<hold>
        {
            public hold()
                : base(Hold)
            {
            }
        }

        public sealed class pendingApproval : BqlString.Constant<pendingApproval>
        {
            public pendingApproval()
                : base(PendingApproval)
            {
            }
        }

        public sealed class rejected : BqlString.Constant<rejected>
        {
            public rejected()
                : base(Rejected)
            {
            }
        }

        public sealed class completed : BqlString.Constant<completed>
        {
            public completed()
                : base(Completed)
            {
            }
        }
    }
}