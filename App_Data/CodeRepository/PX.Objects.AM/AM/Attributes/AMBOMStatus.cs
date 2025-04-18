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
    public class AMBomStatus
    {
        /// <summary>
        /// Hold
        /// </summary>
        public const string Hold = "H"; // Old value: 0
        /// <summary>
        /// Active
        /// </summary>
        public const string Active = "A"; // Old value: 
        /// <summary>
        /// Archived
        /// </summary>
        public const string Archived = "V"; // Old value: 
        /// <summary>
        /// PendingApproval
        /// </summary>
        public const string PendingApproval = "P"; // Old value: 
        /// <summary>
        /// Rejected
        /// </summary>
        public const string Rejected = "R"; // Old value: 

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Active => Messages.GetLocal(Messages.Active);
            public static string Archived => Messages.GetLocal(Messages.Archived);
            public static string PendingApproval => Messages.GetLocal(PX.Objects.EP.Messages.Balanced);
            public static string Rejected => Messages.GetLocal(PX.Objects.EP.Messages.Rejected);
        }

        /// <summary>
        /// Get the list label of the attribute value
        /// </summary>
        public static string GetListDescription(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[value];
            }
            catch
            {
                return Messages.Unknown;
            }
        }

        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { }
        }

        public class active : PX.Data.BQL.BqlString.Constant<active>
        {
            public active() : base(Active) { }
        }

        public class archived : PX.Data.BQL.BqlString.Constant<archived>
        {
            public archived() : base(Archived) { }
        }

        public class pendingApproval : PX.Data.BQL.BqlString.Constant<pendingApproval>
        {
            public pendingApproval() : base(PendingApproval) { }
        }

        public class rejected : PX.Data.BQL.BqlString.Constant<rejected>
        {
            public rejected() : base(Rejected) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Hold, Active, Archived, PendingApproval, Rejected },
                new string[] { Messages.Hold, Messages.Active, Messages.Archived, PX.Objects.EP.Messages.Balanced, PX.Objects.EP.Messages.Rejected })
            { }
        }
    }
}