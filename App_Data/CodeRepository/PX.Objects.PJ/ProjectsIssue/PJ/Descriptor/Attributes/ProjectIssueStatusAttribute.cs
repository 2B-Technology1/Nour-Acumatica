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

namespace PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes
{
    public class ProjectIssueStatusAttribute : PXStringListAttribute
    {
        public const string Open = "O";
        public const string Closed = "C";
        public const string ConvertedToRfi = "R";
        public const string ConvertedToChangeRequest = "Q";

        public ProjectIssueStatusAttribute()
            : base(new[]
            {
                Open,
                Closed,
                ConvertedToRfi,
                ConvertedToChangeRequest
            }, new[]
            {
                "Open",
                "Closed",
                "Converted to RFI",
                "Converted to CR"
            })
        {
        }

        public sealed class convertedToChangeRequest : BqlString.Constant<convertedToChangeRequest>
        {
            public convertedToChangeRequest()
                : base(ConvertedToChangeRequest)
            {
            }
        }

        public sealed class convertedToRfi : BqlString.Constant<convertedToRfi>
        {
            public convertedToRfi()
                : base(ConvertedToRfi)
            {
            }
        }

        public sealed class open : BqlString.Constant<open>
        {
            public open()
                : base(Open)
            {
            }
        }

        public sealed class closed : BqlString.Constant<closed>
        {
            public closed()
                : base(Closed)
            {
            }
        }
    }
}
