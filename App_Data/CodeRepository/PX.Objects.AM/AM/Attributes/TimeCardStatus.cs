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
    public class TimeCardStatus
    {
        public const int Unprocessed = 0;
        public const int Processed = 1;
        public const int Skipped = 2;

        public class Desc
        {
            public static string Unprocessed => Messages.GetLocal(Messages.Unprocessed);
            public static string Processed => Messages.GetLocal(Messages.Processed);
            public static string Skipped => Messages.GetLocal(Messages.Skipped);
        }

        public class unprocessed : PX.Data.BQL.BqlInt.Constant<unprocessed>
        {
            public unprocessed() : base(Unprocessed) { }
        }

        public class processed : PX.Data.BQL.BqlInt.Constant<processed>
        {
            public processed() : base(Processed) { }
        }

        public class skipped : PX.Data.BQL.BqlInt.Constant<skipped>
        {
            public skipped() : base(Skipped) { }
        }


        /// <summary>
        /// UI list for production time card status
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Unprocessed, Processed, Skipped },
                new string[] { Messages.Unprocessed, Messages.Processed, Messages.Skipped})
            { }
        }
    }
}