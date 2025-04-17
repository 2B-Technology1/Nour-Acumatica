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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR.AUF
{
	public abstract class CalculationItem : AufRecord
	{
		protected CalculationItem(AufRecordType recordType, DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd) : base(recordType)
		{
			CheckDate = checkDate;
			PimID = pimID;
			State = state;
			TotalWagesAndTips = 0m;
			TaxableWagesAndTips = 0m;
			TaxableTips = 0m;
			WithholdingAmount = 0m;
			Hours = 0m;
			Days = (periodEnd - periodStart).Value.Days + 1;
			Weeks = ((decimal)((periodEnd - periodStart).Value.Days) + 1) / 7;
			PeriodStart = periodStart;
			PeriodEnd = periodEnd;
		}

		protected CalculationItem(AufRecordType recordType, DateTime checkDate, int pimID, string state, DateTime? periodStart, DateTime? periodEnd, bool? paymentPeriodAlreadyExist) : base(recordType)
		{
			CheckDate = checkDate;
			PimID = pimID;
			State = state;
			TotalWagesAndTips = 0m;
			TaxableWagesAndTips = 0m;
			TaxableTips = 0m;
			WithholdingAmount = 0m;
			Hours = 0m;
			Days = paymentPeriodAlreadyExist.Value ? 0m : (periodEnd - periodStart).Value.Days + 1;
			Weeks = paymentPeriodAlreadyExist.Value ? 0m : ((decimal)((periodEnd - periodStart).Value.Days) + 1) / 7;
			PeriodStart = periodStart;
			PeriodEnd = periodEnd;
		}

		public virtual string State { get; set; }
		public virtual DateTime CheckDate { get; set; }
		public virtual int PimID { get; set; }
		public virtual decimal? TotalWagesAndTips { get; set; }
		public virtual decimal? TaxableWagesAndTips { get; set; }
		public virtual decimal? TaxableTips { get; set; }
		public virtual decimal? WithholdingAmount { get; set; }
		public virtual decimal? Rate { get; set; }
		public virtual decimal? Hours { get; set; }
		public virtual decimal? Days { get; set; }
		public virtual decimal? Weeks { get; set; }
		public virtual DateTime? PeriodStart { get; set; }
		public virtual DateTime? PeriodEnd { get; set; }
	}
}
