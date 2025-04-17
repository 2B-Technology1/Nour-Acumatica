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
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class PaymentBatchStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { ReadyForExport, WaitingPaycheckCalculation, Paid, Closed },
					new string[] { Messages.ReadyForPrint, Messages.WaitingPaycheckCalculation, Messages.Paid, Messages.Closed }
				)
			{ }
		}

		public const string ReadyForExport = "RFE";
		public const string WaitingPaycheckCalculation = "WPC";
		public const string Paid = "PAD";
		public const string Closed = "CLD";

		public static string GetStatus(IEnumerable<PRPayment> results)
		{
			string status = ReadyForExport;
			if (results.Any(x => x.Released == true))
			{
				status = Closed;
			}
			else if (!results.Any(x => x.Paid == false))
			{
				status = Paid;
			}
			//Adjustments don't need to be calculated
			else if (results.Any(x => x.Calculated == false && x.DocType != PayrollType.Adjustment))
			{
				status = WaitingPaycheckCalculation;
			}

			return status;
		}

		public static string GetStatus(PRPayment payment)
		{
			return GetStatus(new List<PRPayment>() { payment });
		}
	}
}
