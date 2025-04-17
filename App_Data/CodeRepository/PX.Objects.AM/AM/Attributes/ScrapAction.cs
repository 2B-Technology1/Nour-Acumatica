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
    /// Production Scrap Action 
    /// </summary>
    public class ScrapAction
    {
        /// <summary>
        /// None
        /// </summary>
        public const int NoAction = 0;
        /// <summary>
        /// Write Off
        /// </summary>
        public const int WriteOff = 1;
        /// <summary>
        /// Quarantine
        /// </summary>
        public const int Quarantine = 2;
        
        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// NoAction
            /// </summary>
            public static string NoAction => Messages.GetLocal(Messages.NoAction);

            /// <summary>
            /// Write Off
            /// </summary>
            public static string WriteOff => Messages.GetLocal(Messages.WriteOff);

            /// <summary>
            /// Warehouse
            /// </summary>
            public static string Quarantine => Messages.GetLocal(Messages.Quarantine);
        }


        /// <summary>
        /// NoAction
        /// </summary>
        public class noAction : PX.Data.BQL.BqlInt.Constant<noAction>
        {
            public noAction() : base(NoAction) {; }
        }
        /// <summary>
        /// Write Off
        /// </summary>
        public class writeOff : PX.Data.BQL.BqlInt.Constant<writeOff>
        {
            public writeOff() : base(WriteOff) {; }
        }
        /// <summary>
        /// Quarantine
        /// </summary>
        public class quarantine : PX.Data.BQL.BqlInt.Constant<quarantine>
        {
            public quarantine() : base(Quarantine) {; }
        }
        
        /// <summary>
        /// List for Production Scrap Action
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { NoAction, WriteOff, Quarantine },
                new string[] { Messages.NoAction, Messages.WriteOff, Messages.Quarantine })
            { }
        }
    }
}