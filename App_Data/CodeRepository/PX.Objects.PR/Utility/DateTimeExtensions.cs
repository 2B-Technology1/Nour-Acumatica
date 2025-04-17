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

namespace PX.Objects.PR
{
	public static class PRDateTime
	{
		/// <summary>
		/// See https://stackoverflow.com/a/32034722/2528023
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static int GetQuarter(this DateTime date)
		{
			return (date.Month + 2) / 3;
		}

		public static int[] GetQuarterMonths(this DateTime date)
		{
			var quarter = GetQuarter(date);
			return new int[] { quarter * 3 - 2, quarter * 3 - 1, quarter * 3 };
		}
	}
}
