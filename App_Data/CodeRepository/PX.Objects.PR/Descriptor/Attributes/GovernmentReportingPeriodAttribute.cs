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

namespace PX.Objects.PR
{
	public class GovernmentReportingPeriod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { Annual, Quarterly, Monthly, DateRange },
				new string[]
				{
					Messages.Annual,
					Messages.Quarterly,
					Messages.Monthly,
					Messages.DateRange
				})
			{
			}
		}

		public class annual : PX.Data.BQL.BqlString.Constant<annual>
		{
			public annual() : base(Annual) { }
		}

		public class quarterly : PX.Data.BQL.BqlString.Constant<quarterly>
		{
			public quarterly() : base(Quarterly) { }
		}

		public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
		{
			public monthly() : base(Monthly) { }
		}

		public class dateRange : PX.Data.BQL.BqlString.Constant<dateRange>
		{
			public dateRange() : base(DateRange) { }
		}

		public static string FromAatrixData(string aatrixReportingPeriod)
		{
			switch (aatrixReportingPeriod)
			{
				case "Annual":
					return Annual;
				case "Quarterly":
					return Quarterly;
				case "Monthly":
					return Monthly;
			}
			return DateRange;
		}

		public const string Annual = "ANN";
		public const string Quarterly = "QTR";
		public const string Monthly = "MTH";
		public const string DateRange = "DTR";
	}

	public class Quarter
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new int[] { 1, 2, 3, 4 },
				new string[] 
				{
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 1),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 2),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 3),
					PXMessages.LocalizeFormatNoPrefix(GL.Messages.QuarterDescr, 4)
				})
			{
			}
		}
	}

	public class Month
	{
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() : base(
				new int[]
				{
					_Jan, _Feb, _Mar, _Apr, _May, _Jun, _Jul, _Aug, _Sep, _Oct, _Nov, _Dec
				},
				new string[]
				{
					Messages.January,
					Messages.February,
					Messages.March,
					Messages.April,
					Messages.May,
					Messages.June,
					Messages.July,
					Messages.August,
					Messages.September,
					Messages.October,
					Messages.November,
					Messages.December
				})
			{
			}

			private static int _Jan = 1;
			private static int _Feb = 2;
			private static int _Mar = 3;
			private static int _Apr = 4;
			private static int _May = 5;
			private static int _Jun = 6;
			private static int _Jul = 7;
			private static int _Aug = 8;
			private static int _Sep = 9;
			private static int _Oct = 10;
			private static int _Nov = 11;
			private static int _Dec = 12;
		}
	}
}
