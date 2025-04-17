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
using PX.Objects.GL;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing Document Status
    /// </summary>
    public class DocStatus
    {
        public const string Balanced = BatchStatus.Balanced;
        public const string Hold = BatchStatus.Hold;
        public const string Released = BatchStatus.Released;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Balanced => Messages.GetLocal(Messages.Balanced);
            public static string Hold => Messages.GetLocal(Messages.Hold);
            public static string Released => Messages.GetLocal(Messages.Released);
        }

        //BQL constants declaration
        public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
        {
            public balanced() : base(Balanced) { ;}
        }
        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { ;}
        }
        public class released : PX.Data.BQL.BqlString.Constant<released>
        {
            public released() : base(Released) { ;}
        }

        #pragma warning disable
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        Balanced, 
                        Hold, 
                        Released},
                    new string[] {
                        Messages.Balanced,
                        Messages.Hold,
                        Messages.Released}) { }
        }
    }
}