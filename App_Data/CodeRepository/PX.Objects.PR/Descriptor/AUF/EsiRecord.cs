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

using PX.Payroll.Data;
using System;

namespace PX.Objects.PR.AUF
{
	public class EsiRecord : CalculationItem
	{
		public EsiRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) :
			base(AufRecordType.Esi, checkDate, pimID, state, periodStart, periodEnd) { }

		public EsiRecord(DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd, bool? paymentPeriodAlreadyExist) :
			base(AufRecordType.Esi, checkDate, pimID, state, periodStart, periodEnd, paymentPeriodAlreadyExist)
		{ }

		public override string ToString()
		{
			bool isPuertoRico = State == LocationConstants.PuertoRicoStateAbbr;

			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				TotalWagesAndTips,
				TaxableWagesAndTips,
				TaxableTips,
				WithholdingAmount,
				Hours,
				Days,
				Weeks,
				PeriodStart,
				PeriodEnd,
				isPuertoRico ? Commissions : new decimal?(),
				isPuertoRico ? Allowances : new decimal?(),
				AccountNumber
			};

			return FormatLine(lineData);
		}

		public virtual decimal? Commissions { get; set; }
		public virtual decimal? Allowances { get; set; }
		public virtual string AccountNumber { get; set; }
	}
}
