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

namespace PX.Objects.PR.AUF
{
	public class GenRecord : AufRecord
	{
		public GenRecord(DateTime checkDate, bool isRegularPaycheck) : base(AufRecordType.Gen)
		{
			CheckDate = checkDate;
			IsRegularPaycheck = isRegularPaycheck;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				CheckDate,
				GrossPay,
				AufConstants.UnusedField,
				NetPay,
				SSWages,
				SSWithheld,
				MedicareWages,
				MedicareWithheld,
				FederalWages,
				FederalWithheld,
				TaxableFutaWages,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				EarnedIncomeCredit,
				SSTips,
				FutaLiability,
				TotalFutaWages,
				PeriodStart,
				PeriodEnd,
				SSEmployerMatch,
				MedicareEmployerMatch,
				AdditionalMedicareTax,
				AdditionalMedicareWages
			};

			StringBuilder builder = new StringBuilder(FormatLine(lineData));
			EsiList?.ForEach(esi => builder.Append(esi.ToString()));
			EliList?.ForEach(eli => builder.Append(eli.ToString()));

			return builder.ToString();
		}

		#region Data
		public virtual DateTime CheckDate { get; set; }
		public virtual bool IsRegularPaycheck { get; set; }
		public virtual decimal? GrossPay { get; set; }
		public virtual decimal? NetPay { get; set; }
		public virtual decimal? SSWages { get; set; }
		public virtual decimal? SSWithheld { get; set; }
		public virtual decimal? MedicareWages { get; set; }
		public virtual decimal? MedicareWithheld { get; set; }
		public virtual decimal? FederalWages { get; set; }
		public virtual decimal? FederalWithheld { get; set; }
		public virtual decimal? TaxableFutaWages { get; set; }
		public virtual decimal? EarnedIncomeCredit { get; set; }
		public virtual decimal? SSTips { get; set; }
		public virtual decimal? FutaLiability { get; set; }
		public virtual decimal? TotalFutaWages { get; set; }
		public virtual DateTime? PeriodStart { get; set; }
		public virtual DateTime? PeriodEnd { get; set; }
		public virtual decimal? SSEmployerMatch { get; set; }
		public virtual decimal? MedicareEmployerMatch { get; set; }
		public virtual decimal? AdditionalMedicareTax { get; set; }
		public virtual decimal? AdditionalMedicareWages { get; set; }
		public virtual decimal? StateWithheld { get; set; }
		public virtual decimal? SuiWithheld { get; set; }
		public virtual decimal? SdiWithheld { get; set; }
		public virtual string CheckNumber { get; set; }
		#endregion Data

		#region Children records
		public List<EsiRecord> EsiList { get; set; }
		public List<EliRecord> EliList { get; set; }
		public List<EjwRecord> EjwList { get; set; }
		#endregion
	}
}
