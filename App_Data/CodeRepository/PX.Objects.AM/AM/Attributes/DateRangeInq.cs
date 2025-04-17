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
    public class DateRangeInq
    {
        public const string Daily = "D";
        public const string Weekly = "W";
        public const string Biweekly = "B";
        public const string Monthly = "M";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string Daily => Messages.GetLocal(Messages.Daily);
            public static string Weekly => Messages.GetLocal(Messages.Weekly);
            public static string Biweekly => Messages.GetLocal(Messages.BiWeekly);
            public static string Monthly => Messages.GetLocal(Messages.Monthly);
        }


        public class daily : PX.Data.BQL.BqlString.Constant<daily>
        {
            public daily() : base(Daily) {; }
        }

        public class weekly : PX.Data.BQL.BqlString.Constant<weekly>
        {
            public weekly() : base(Weekly) {; }
        }

        public class biweekly : PX.Data.BQL.BqlString.Constant<biweekly>
        {
            public biweekly() : base(Biweekly) {; }
        }

        public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
        {
            public monthly() : base(Monthly) {; }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Daily, Weekly, Biweekly, Monthly },
                    new string[] { Messages.Daily, Messages.Weekly, Messages.BiWeekly, Messages.Monthly }) { }
        }
    }
}