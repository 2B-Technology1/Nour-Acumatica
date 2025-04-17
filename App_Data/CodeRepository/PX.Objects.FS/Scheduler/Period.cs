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

using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the Period time for the generation of the Time Slots.
    /// </summary>
    public class Period
    {
        /// <summary>
        /// Gets the beginning date for the Time Slot generation.
        /// </summary>
        public DateTime Start { get; private set; }

        /// <summary>
        /// Gets the end date for the Time Slot generation.
        /// </summary>
        public DateTime? End { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Period class which validates if the start Period time > end Period time.
        /// </summary>
        public Period(DateTime start, DateTime? end)
        {
            this.Start = start.Date;
            this.End = end;

            if (this.End != null && this.Start > this.End.Value)
            {
                throw new ArgumentException(PX.Data.PXMessages.LocalizeFormatNoPrefix(PX.Objects.FS.TX.Error.END_DATE_LESSER_THAN_START_DATE));
            }
        }
    }
}
