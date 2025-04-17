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
    /// Generic list of selectable time unit types
    /// </summary>
    public class TimeUnits
    {
        /// <summary>
        /// Units in Days
        /// </summary>
        public const int Days = 0;
        /// <summary>
        /// Units in Hours
        /// </summary>
        public const int Hours = 1;

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Days => Messages.GetLocal(Messages.Days);
            public static string Hours => Messages.GetLocal(Messages.Hours);
        }

        /// <summary>
        /// Time units for Lead Time
        /// </summary>
        public class LeadTimeListAttribute : PXIntListAttribute
        {
            public LeadTimeListAttribute()
                : base(new int[]
            {
                Days,
                Hours
            }, new string[]
            {
                Messages.Days,
                Messages.Hours
            })
                {
                }
        }

        public class days : PX.Data.BQL.BqlInt.Constant<days>
        {
            public days()
                : base(Days) { }
        }

        public class hours : PX.Data.BQL.BqlInt.Constant<hours>
        {
            public hours()
                : base(Hours) { }
        }

    }
}