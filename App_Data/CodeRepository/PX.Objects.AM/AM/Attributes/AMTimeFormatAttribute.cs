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
    public class AMTimeFormatAttribute
    {
        public static TimeSpanFormatType GetFormat(int? format)
        {
            if (format != null && typeof(TimeSpanFormatType).IsEnumDefined(format))
            {
                return (TimeSpanFormatType)format;
            }

            return TimeSpanFormatType.ShortHoursMinutes;
        }

        public class TimeSpanFormat
        {
            public const int DaysHoursMinutes = 0;
            public const int DaysHoursMinutesCompact = 1;
            public const int LongHoursMinutes = 2;
            public const int ShortHoursMinutes = 3;
            public const int ShortHoursMinutesCompact = 4;

            public class Desc
            {
                public static string DaysHoursMinutes => ActionsMessages.TimeSpanMaskDHM;

                public static string DaysHoursMinutesCompact => Messages.DaysHoursMinutesCompact;

                public static string LongHoursMinutes => ActionsMessages.TimeSpanLongHM;

                public static string ShortHoursMinutes => ActionsMessages.TimeSpanHM;

                public static string ShortHoursMinutesCompact => Messages.ShortHoursMinutesCompact;
            }

            public class daysHoursMinutes : PX.Data.BQL.BqlInt.Constant<daysHoursMinutes>
            {
                public daysHoursMinutes() : base(DaysHoursMinutes) {; }
            }

            public class daysHoursMinutesCompact : PX.Data.BQL.BqlInt.Constant<daysHoursMinutesCompact>
            {
                public daysHoursMinutesCompact() : base(DaysHoursMinutesCompact) {; }
            }

            public class longHoursMinutes : PX.Data.BQL.BqlInt.Constant<longHoursMinutes>
            {
                public longHoursMinutes() : base(LongHoursMinutes) {; }
            }

            public class shortHoursMinutes : PX.Data.BQL.BqlInt.Constant<shortHoursMinutes>
            {
                public shortHoursMinutes() : base(ShortHoursMinutes) {; }
            }

            public class shortHoursMinutesCompact : PX.Data.BQL.BqlInt.Constant<shortHoursMinutesCompact>
            {
                public shortHoursMinutesCompact() : base(ShortHoursMinutesCompact) {; }
            }

            /// <summary>
            /// List for Time Formatting
            /// </summary>
            public class ListAttribute : PXIntListAttribute
            {
                public ListAttribute()
                    : base(
                        new int[]
                        {
                            DaysHoursMinutes, DaysHoursMinutesCompact, LongHoursMinutes, ShortHoursMinutes,
                            ShortHoursMinutesCompact
                        },
                        new string[]
                        {
                            ActionsMessages.TimeSpanMaskDHM, Messages.DaysHoursMinutesCompact, ActionsMessages.TimeSpanLongHM,
                            ActionsMessages.TimeSpanHM, Messages.ShortHoursMinutesCompact
                        })
                { }
            }
        }
    }
}