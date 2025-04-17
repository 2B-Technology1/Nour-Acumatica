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
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public class DeductionMaxFrequencyType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { NoMaximum, PerPayPeriod, PerCalendarYear },
				new string[] { Messages.NoMaximum, Messages.PerPayPeriod, Messages.PerCalendarYear })
			{ }
		}

		public class noMaximum : PX.Data.BQL.BqlString.Constant<noMaximum>
		{
			public noMaximum()
				: base(NoMaximum)
			{
			}
		}

		public static PRBenefitMaximumFrequency ToEnum(string value)
		{
			PRBenefitMaximumFrequency enumValue = PRBenefitMaximumFrequency.NoMaximum;

			if (value == PerPayPeriod)
			{
				enumValue = PRBenefitMaximumFrequency.PerPayPeriod;
			}
			else if (value == PerCalendarYear)
			{
				enumValue = PRBenefitMaximumFrequency.PerCalendarYear;
			}

			return enumValue;
		}

		public const string NoMaximum = "NOM";
		public const string PerPayPeriod = "PAY";
		public const string PerCalendarYear = "CAL";
	}
}
