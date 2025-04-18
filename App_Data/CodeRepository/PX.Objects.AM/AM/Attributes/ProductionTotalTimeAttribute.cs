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
    /// Production total time span attribute for correctly displaying the production total time format based on setup
    /// </summary>
    public class ProductionTotalTimeDBAttribute : PXDBTimeSpanLongAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Format = GetFormat(((AMBSetup) PXSelect<AMBSetup>.Select(sender.Graph))?.ProductionTimeFormat);
        }

        protected virtual TimeSpanFormatType GetFormat(int? format)
        {
            return AMTimeFormatAttribute.GetFormat(format);
        }
    }

    /// <summary>
    /// Production total time span attribute for correctly displaying the production total time format based on setup
    /// </summary>
    public class ProductionTotalTimeNonDBAttribute : PXTimeSpanLongAttribute
    {
        public ProductionTotalTimeNonDBAttribute()
        {
            // Work around provided by Acumatica DEV support case 063186 (confirmed defect as of 17.200.0223)
            // Only non db version of TimeSpanLongAttribute is having the issues.            
            base._inputMasks = new string[] { ActionsMessages.TimeSpanMaskDHM, Messages.DaysHoursMinutesCompact, ActionsMessages.TimeSpanLongHM, ActionsMessages.TimeSpanHM, Messages.ShortHoursMinutesCompact };
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Format = GetFormat(((AMBSetup)PXSelect<AMBSetup>.Select(sender.Graph))?.ProductionTimeFormat);
        }

        protected virtual TimeSpanFormatType GetFormat(int? format)
        {
            return AMTimeFormatAttribute.GetFormat(format);
        }
    }
}
