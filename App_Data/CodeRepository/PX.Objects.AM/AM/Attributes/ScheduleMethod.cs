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
    /// Scheduling method parameters for controlling of scheduling start/end dates
    /// </summary>
    public class ScheduleMethod
    {
        /// <summary>
        /// Finish On = "F"
        ///     Backward schedule of an order
        /// </summary>
        public const string FinishOn = "F";
        /// <summary>
        /// Start On = "S"
        ///     Forward schedule of an order
        /// </summary>
        public const string StartOn = "S"; 
        /// <summary>
        /// User Dates = "U"
        ///     Allows users to select specific start/end dates
        /// </summary>
        public const string UserDates = "U";

        /// <summary>
        /// Description/labels for identifiers
        /// </summary>
        public static class Desc
        {
            public static string FinishOn => Messages.GetLocal(Messages.FinishOn);
            public static string StartOn => Messages.GetLocal(Messages.StartOn);
            public static string UserDates => Messages.GetLocal(Messages.UserDates);
        }

        public class finishOn : PX.Data.BQL.BqlString.Constant<finishOn>
        {
            public finishOn() : base(FinishOn) { }
        }
        public class startOn : PX.Data.BQL.BqlString.Constant<startOn>
        {
            public startOn() : base(StartOn) { }
        }
        public class userDates : PX.Data.BQL.BqlString.Constant<userDates>
        {
            public userDates() : base(UserDates) { }
        }

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { 
                        StartOn, 
                        FinishOn, 
                        UserDates },
                    new string[] { 
                        Messages.StartOn,
                        Messages.FinishOn,
                        Messages.UserDates }) { }
        }
    }
}