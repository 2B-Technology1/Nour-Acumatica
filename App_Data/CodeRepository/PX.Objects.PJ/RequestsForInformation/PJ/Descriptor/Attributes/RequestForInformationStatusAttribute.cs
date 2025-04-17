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

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationStatusAttribute : PXStringListAttribute
    {
        public const string NewStatus = "N";
        public const string OpenStatus = "O";
        public const string ClosedStatus = "C";
        public const string NewStatusLabel = "New";
        public const string OpenStatusLabel = "Open";
        public const string ClosedStatusLabel = "Closed";

        public RequestForInformationStatusAttribute()
            : base(new[]
            {
                NewStatus,
                OpenStatus,
                ClosedStatus
            }, new[]
            {
                NewStatusLabel,
                OpenStatusLabel,
                ClosedStatusLabel
            })
        {
        }

        public sealed class openStatus : BqlString.Constant<openStatus>
        {
            public openStatus()
                : base(OpenStatus)
            {
            }
        }

        public sealed class newStatus : BqlString.Constant<newStatus>
        {
            public newStatus()
                : base(NewStatus)
            {
            }
        }

        public sealed class closedStatus : BqlString.Constant<closedStatus>
        {
            public closedStatus()
                : base(ClosedStatus)
            {
            }
        }
    }
}
