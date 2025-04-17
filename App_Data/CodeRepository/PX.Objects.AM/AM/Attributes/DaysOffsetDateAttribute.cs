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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Date attribute to indicate an offset of days to set the min/max date values
    /// </summary>
    public sealed class DaysOffsetDateAttribute : PXDBDateAttribute
    {
        /// <summary>
        /// Min date offset in days
        /// </summary>
        private int? _MinOffsetValue;
        /// <summary>
        /// Max date offset in days
        /// </summary>
        private int? _MaxOffsetValue;


        /// <summary>
        /// Minimum date offset in days. The offset adds days to the current business date to define the minimum date value.
        /// If no offset is defined then the default min date value is used without an offset
        /// </summary>
        /// 
        /// <example>
        /// The example below shows a StartDate field that will not allow a date less that yesterday based on the current business date of today.
        /// 
        /// <code>
        /// [DaysOffsetDate(MinOffsetDays = "-1")]
        ///             public virtual DateTime? StartDate { get; set; }
        /// 
        /// </code>
        /// 
        /// </example>
        public string MinOffsetDays
        {
            get { return Convert.ToString(_MinOffsetValue); }
            set { _MinOffsetValue = ConvertToInt(value); }
        }

        /// <summary>
        /// Maximum date offset in days. The offset adds days to the current business date to define the maximum date value.
        /// If no offset is defined then the default max date value is used without an offset
        /// </summary>
        /// 
        /// <example>
        /// The example below shows a EndDate field that will not allow a date less that yesterday and greater than 30 days from now based on the current business date of today.
        /// 
        /// <code>
        /// [DaysOffsetDate(MinOffsetDays = "-1", MaxOffsetDays = "30")]
        ///             public virtual DateTime? EndDate { get; set; }
        /// 
        /// </code>
        /// 
        /// </example>
        public string MaxOffsetDays
        {
            get { return Convert.ToString(_MaxOffsetValue); }
            set { _MaxOffsetValue = ConvertToInt(value); }
        }

        private int? ConvertToInt(string stringValue)
        {
            int v;
            if (Int32.TryParse(stringValue, out v))
            {
                return v;
            }
            return null;
        }

        public override void CacheAttached(PXCache sender)
        {
            if (IsKey)
            {
                sender.Keys.Add(_FieldName);
            }

            DateTime businessDate = Common.Current.BusinessDate(sender.Graph);

            if (_MinValue == null)
            {
                if (_MinOffsetValue != null)
                {
                    _MinValue = businessDate.AddDays(_MinOffsetValue.GetValueOrDefault());
                }
                else
                {
                    _MinValue = Common.Dates.BeginOfTimeDate;
                }
            }
            if (_MaxValue == null)
            {
                if (_MaxOffsetValue != null)
                {
                    _MaxValue = businessDate.AddDays(_MaxOffsetValue.GetValueOrDefault());
                }
                else
                {
                    _MaxValue = Common.Dates.EndOfTimeDate;
                }
            }
        }
    }
}