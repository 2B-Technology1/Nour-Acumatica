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
    public class CTPProcessActionOptions
    {
        /// <summary>
        /// Run CTP Process
        /// </summary>
        public const string RunCTP = "R";

        /// <summary>
        /// Accept CTP 
        /// </summary>
        public const string Accept = "A";

        /// <summary>
        /// Reject CTP 
        /// </summary>
        public const string Reject = "X";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string RunCTP => Messages.GetLocal(Messages.RunCTP);
            public static string Accept => Messages.GetLocal(Messages.Accept);
            public static string Reject => Messages.GetLocal(Messages.Reject);
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[]{RunCTP, Accept, Reject}
                , new string[]{ Messages.RunCTP, Messages.Accept, Messages.Reject}) { }
        }

        /// <summary>
        /// Run CTP Process
        /// </summary>
        public class runCTP : PX.Data.BQL.BqlString.Constant<runCTP>
        {
            public runCTP() : base(RunCTP) { ;}
        }

        /// <summary>
        /// Accept CTP
        /// </summary>
        public class accept : PX.Data.BQL.BqlString.Constant<accept>
        {
            public accept() : base(Accept) { ;}
        }

        /// <summary>
        /// Reject CTP
        /// </summary>
        public class reject : PX.Data.BQL.BqlString.Constant<reject>
        {
            public reject() : base(Reject) {; }
        }
    }
}