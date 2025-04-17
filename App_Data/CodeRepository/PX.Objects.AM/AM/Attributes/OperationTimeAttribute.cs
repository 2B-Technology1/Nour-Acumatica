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
using System;

namespace PX.Objects.AM.Attributes
{
	/// <summary>
	/// Operation time span attribute for correctly displaying the operation time format based on setup
	/// </summary>
	public class OperationDBTimeAttribute : PXDBTimeSpanLongAttribute
	{
		int? FormatValue { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			using (PXDataRecord record = PXDatabase.SelectSingle<AMBSetup>(
				new PXDataField<AMBSetup.operationTimeFormat>()))
			{
				FormatValue = record?.GetInt32(0);
				Format = GetFormat(FormatValue);
			}
		}

		protected virtual TimeSpanFormatType GetFormat(int? format)
		{
			return AMTimeFormatAttribute.GetFormat(format);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var retval = e.ReturnValue;
			var timeinminutes = Convert.ToInt32(retval);
			bool maxtimeexceeded = false;

			switch (FormatValue)
			{
				case AMTimeFormatAttribute.TimeSpanFormat.DaysHoursMinutes:
				case AMTimeFormatAttribute.TimeSpanFormat.DaysHoursMinutesCompact:
					if (timeinminutes > 1439999)
					{
						maxtimeexceeded = true;
					}
					break;
				case AMTimeFormatAttribute.TimeSpanFormat.LongHoursMinutes:
					if (timeinminutes > 599999)
					{
						maxtimeexceeded = true;
					}
					break;
				case AMTimeFormatAttribute.TimeSpanFormat.ShortHoursMinutes:
				case AMTimeFormatAttribute.TimeSpanFormat.ShortHoursMinutesCompact:
					if (timeinminutes > 1439)
					{
						maxtimeexceeded = true;
					}
					break;
			}	


			base.FieldSelecting(sender, e);

			if (maxtimeexceeded)
			{
				var timespan = TimeSpan.FromMinutes(timeinminutes);
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, typeof(string), error: String.Format(Messages.ExceedsOperationTimeFormat,
					string.Format("{0:D2}d:{1:D2}h:{2:D2}m",
					timespan.Days,
					timespan.Hours,
					timespan.Minutes
				)), errorLevel: PXErrorLevel.Warning);
			}

		}

	}



	/// <summary>
	/// Operation time span attribute for correctly displaying the operation time format based on setup
	/// (For unbound/non DB fields)
	/// </summary>
	public class OperationNonDBTimeAttribute : PXTimeSpanLongAttribute
    {
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Format = GetFormat(((AMBSetup)PXSelect<AMBSetup>.Select(sender.Graph))?.OperationTimeFormat);
        }

        protected virtual TimeSpanFormatType GetFormat(int? format)
        {
            return AMTimeFormatAttribute.GetFormat(format);
        }
    }
}
