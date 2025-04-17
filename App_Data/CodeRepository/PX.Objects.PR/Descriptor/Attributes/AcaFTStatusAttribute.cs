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
	public class AcaFTStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { FullTime, PartTime },
				new string[]
				{
					Messages.FullTime,
					Messages.PartTime,
				})
			{
			}
		}

		public class fullTime : PX.Data.BQL.BqlString.Constant<fullTime>
		{
			public fullTime() : base(FullTime) { }
		}

		public class partTime : PX.Data.BQL.BqlString.Constant<partTime>
		{
			public partTime() : base(PartTime) { }
		}

		public const string FullTime = "FTM";
		public const string PartTime = "PTM";

		public const int FullTimeMonthlyHourThreshold = 130;
		public const int FullTimeWeeklyHourThreshold = 30;
		public const int PartTimeMaxHoursImputed = 120;
		public const int FteHours = 120;
	}
}
