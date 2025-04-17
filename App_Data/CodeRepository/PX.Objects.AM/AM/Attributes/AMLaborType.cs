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
    /// Manufacturing Labor Types
    /// </summary>
    public class AMLaborType
    {
        /// <summary>
        /// Manufacturing Direct Labor
        /// </summary>
        public const string Direct = "D";
        /// <summary>
        /// Manufacturing Indirect Labor
        /// </summary>
        public const string Indirect = "I";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            /// <summary>
            /// Direct Labor
            /// </summary>
            public static string Direct => Messages.GetLocal(Messages.Direct);

            /// <summary>
            /// Indirect Labor
            /// </summary>
            public static string Indirect => Messages.GetLocal(Messages.Indirect);
        }

        /// <summary>
        /// Manufacturing Direct Labor
        /// </summary>
        public class direct : PX.Data.BQL.BqlString.Constant<direct>
        {
            public direct() : base(Direct){ ;}
        }
        /// <summary>
        /// Manufacturing Indirect Labor
        /// </summary>
        public class indirect : PX.Data.BQL.BqlString.Constant<indirect>
        {
            public indirect() : base(Indirect){ ;}
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        Direct, 
                        Indirect}, 
                    new string[] {
                        Messages.Direct,
                        Messages.Indirect})
            { }
        }
    }
}