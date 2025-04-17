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
using PX.Data.BQL;
using System;

namespace PX.Objects.PR
{
	public class OneBasedDayOfWeek
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(GetOneBasedDaysOfWeek())
			{
			}

			private static Tuple<int, string>[] GetOneBasedDaysOfWeek()
			{
				const byte weekLength = 7;
				Tuple<int, string>[] result = new Tuple<int, string>[weekLength];
				for (byte i = 0; i < weekLength; i++)
				{
					DayOfWeek dayOfWeek = (DayOfWeek)i;
					int oneBasedDayOfWeek = GetOneBasedDayOfWeek(dayOfWeek);
					result[i] = new Tuple<int, string>(oneBasedDayOfWeek, dayOfWeek.ToString());
				}

				return result;
			}
		}

		public class saturday : BqlInt.Constant<saturday>
		{
			public saturday() : base(GetOneBasedDayOfWeek(DayOfWeek.Saturday)) { }
		}

		public static int GetOneBasedDayOfWeek(DayOfWeek zeroBasedDayOfWeek)
		{
			return (int)zeroBasedDayOfWeek + 1;
		}

		public static DayOfWeek GetZeroBasedDayOfWeek(int oneBasedDayOfWeek)
		{
			if (oneBasedDayOfWeek < 1 || oneBasedDayOfWeek > 7)
				throw new PXArgumentException(Messages.OneBasedDayOfWeekIncorrectValue, nameof(oneBasedDayOfWeek));

			return (DayOfWeek)(oneBasedDayOfWeek - 1);
		}
	}
}
