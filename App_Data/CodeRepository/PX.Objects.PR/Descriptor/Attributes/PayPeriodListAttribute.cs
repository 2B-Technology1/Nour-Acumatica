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
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PayPeriodType
	{
		public const string SemiMonth = "SM";
		public const string Month = FinPeriodType.Month;
		public const string BiMonth = FinPeriodType.BiMonth;
		public const string Quarter = FinPeriodType.Quarter;
		public const string Week = FinPeriodType.Week;
		public const string BiWeek = FinPeriodType.BiWeek;
		public const string CustomPeriodsNumber = FinPeriodType.CustomPeriodsNumber;

		public const int ROENumRecordsWeek = 53;
		public const int ROENumRecordsBiWeek = 27;
		public const int ROENumRecordsSemiMonth = 25;
		public const int ROENumRecordsMonth = 13;


		public class ROEListAttribute : PXStringListAttribute
		{
			public ROEListAttribute()
				: base(
				new string[] { Week, BiWeek, SemiMonth, Month, CustomPeriodsNumber },
				new string[] { Messages.Weekly, Messages.Biweekly, Messages.Semimonthly, Messages.Monthly, Messages.ThirteenPerYear }
				)
			{ }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SemiMonth, Month, BiMonth, Quarter, Week, BiWeek, CustomPeriodsNumber },
				new string[] { Messages.Semimonthly, Messages.Monthly, Messages.BiMonthly, Messages.Quarterly, Messages.Weekly, Messages.Biweekly, GL.Messages.PT_CustomPeriodsNumber }
				)
			{ }
		}

		public class week : PX.Data.BQL.BqlString.Constant<week>
		{
			public week() : base(Week)
			{
			}
		}

		public class biWeek : PX.Data.BQL.BqlString.Constant<biWeek>
		{
			public biWeek() : base(BiWeek)
			{
			}
		}

		public static FiscalPeriodSetupCreator.FPType GetFPType(string aFinPeriodType)
		{
			switch (aFinPeriodType)
			{
				case SemiMonth:
					return FiscalPeriodSetupCreator.FPType.Custom;
				default:
					return FinPeriodType.GetFPType(aFinPeriodType);
			}
		}

		public static bool IsSemiMonth(string aFinPeriodType)
		{
			return aFinPeriodType == SemiMonth;
		}

		public static bool IsCustom(PRPayGroupYearSetup periodSetup)
		{
			return periodSetup.FPType == FiscalPeriodSetupCreator.FPType.Custom && !IsSemiMonth(periodSetup.PeriodType);
		}

		public static int GetROENumberOfPeriods(string periodType)
		{
			switch (periodType)
			{
				case Week: return ROENumRecordsWeek;
				case BiWeek: return ROENumRecordsBiWeek;
				case SemiMonth: return ROENumRecordsSemiMonth;
				case Month:	return ROENumRecordsMonth;
				default: return ROENumRecordsWeek;
			}
		}
	}
}
