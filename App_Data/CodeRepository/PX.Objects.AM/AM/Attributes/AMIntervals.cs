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
    /// MRP Bucket Units
    /// </summary>
    public class AMIntervals
    {
        /// <summary>
        /// Days
        /// </summary>
        public const int Day = 1;
        /// <summary>
        /// Weeks
        /// </summary>
        public const int Week = 2;
        /// <summary>
        /// Months
        /// </summary>
        public const int Month = 3;
        /// <summary>
        /// Years
        /// </summary>
        public const int Year = 4;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            /// <summary>
            /// Days
            /// </summary>
            public static string Day => PX.Objects.IN.Messages.Day;

            /// <summary>
            /// Weeks
            /// </summary>
            public static string Week => PX.Objects.IN.Messages.Week;

            /// <summary>
            /// Months
            /// </summary>
            public static string Month => PX.Objects.IN.Messages.Month;

            /// <summary>
            /// Years
            /// </summary>
            public static string Year => PX.Objects.IN.Messages.YearConst;
        }


        /// <summary>
        /// Days
        /// </summary>
        public class day : PX.Data.BQL.BqlInt.Constant<day>
        {
            public day() : base(Day) { }
        }
        /// <summary>
        /// Weeks
        /// </summary>
        public class week : PX.Data.BQL.BqlInt.Constant<week>
        {
            public week() : base(Week) { }
        }
        /// <summary>
        /// Months
        /// </summary>
        public class month : PX.Data.BQL.BqlInt.Constant<month>
        {
            public month() : base(Month) { }
        }
        /// <summary>
        /// Years
        /// </summary>
        public class year : PX.Data.BQL.BqlInt.Constant<year>
        {
            public year() : base(Year) { }
        }

        /// <summary>
        /// List for MRP Bucket Units
        /// </summary>
        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Day, Week, Month, Year },
                new string[] { IN.Messages.Day, IN.Messages.Week, IN.Messages.Month, IN.Messages.YearConst })
            { }
        }
    }
}