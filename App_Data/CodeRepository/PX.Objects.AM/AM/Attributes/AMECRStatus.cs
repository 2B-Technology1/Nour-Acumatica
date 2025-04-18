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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Bill of Material Revision Status
    /// </summary>
    public class AMECRStatus
    {
        /// <summary>
        /// Hold
        /// </summary>
        public const string Hold = "H"; // Old value: 0
        /// <summary>
        /// Active
        /// </summary>
        public const string Active = "A"; // Old value: 1
        /// <summary>
        /// Archived
        /// </summary>
        public const string Approved = "V"; // Old value: 2
        /// <summary>
        /// PendingApproval
        /// </summary>
        public const string PendingApproval = "P"; // Old value: 3
        /// <summary>
        /// Rejected
        /// </summary>
        public const string Rejected = "R"; // Old value: 4
        /// <summary>
        /// Completed
        /// </summary>
        public const string Completed = "C"; // Old value: 5

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Active => Messages.GetLocal(Messages.Active);
            public static string Approved => Messages.GetLocal(PX.Objects.EP.Messages.Approved);
            public static string PendingApproval => Messages.GetLocal(PX.Objects.EP.Messages.PendingApproval);
            public static string Rejected => Messages.GetLocal(PX.Objects.EP.Messages.Rejected);
            public static string Completed => Messages.GetLocal(PX.Objects.EP.Messages.Completed);
        }

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { }
        }

        public class active : PX.Data.BQL.BqlString.Constant<active>
        {
            public active() : base(Active) { }
        }

        public class approved : PX.Data.BQL.BqlString.Constant<approved>
        {
            public approved() : base(Approved) { }
        }

        public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
        {
            public pendingApproval() : base(PendingApproval) { }
        }

        public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
        {
            public rejected() : base(Rejected) { }
        }
        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Hold, Active, Approved, PendingApproval, Rejected, Completed },
                new string[] { Messages.Hold, Messages.Active, EP.Messages.Approved, EP.Messages.PendingApproval, EP.Messages.Rejected, EP.Messages.Completed }) { }
        }
    }
}