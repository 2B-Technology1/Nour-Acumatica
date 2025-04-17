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
using PX.Data;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class OccupationalRatesFilter : IBqlTable
    {
        #region PeriodType
        public abstract class periodType : ListField_Period_Appointment
        {
        }

        [PXString(1, IsFixed = true)]
        [PXDefault(ID.PeriodType.WEEK)]
        [PXUIField(DisplayName = "Period")]
        [periodType.ListAtrribute]
        public virtual string PeriodType { get; set; }
        #endregion
        #region DateInRange
        public abstract class dateInRange : PX.Data.BQL.BqlDateTime.Field<dateInRange> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Date in Range")]
        public virtual DateTime? DateInRange { get; set; }
        #endregion
        #region DateBegin
        public abstract class dateBegin : PX.Data.BQL.BqlDateTime.Field<dateBegin> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Begin Date", Enabled = false, Visible = true)]
        public virtual DateTime? DateBegin { get; set; }
        #endregion
        #region DateEnd
        public abstract class dateEnd : PX.Data.BQL.BqlDateTime.Field<dateEnd> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "End Date", Enabled = false, Visible = true)]
        public virtual DateTime? DateEnd { get; set; }
        #endregion
    }
}
