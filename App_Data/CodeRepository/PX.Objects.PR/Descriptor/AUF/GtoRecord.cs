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
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class GtoRecord : AufRecord
	{
		public GtoRecord(DateTime checkDate) : base(AufRecordType.Gto)
		{
			CheckDate = checkDate;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				AufConstants.UnusedField,
				CheckDate,
				GrossPay,
				NetPay,
				AufConstants.UnusedField,
				SSWages,
				SSLiability,
				MedicareWages,
				MedicareLiability,
				FederalWages,
				FederalWHLiability,
				TaxableFutaWages,
				FutaLiability,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				EarnedIncomeCredit,
				SSTips,
				TotalFutaWages,
				PeriodStart,
				PeriodEnd,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.ManualInput, // 940 Deposit
				AufConstants.ManualInput, // 941 Deposit
				AufConstants.ManualInput, // 943 Deposit
				AufConstants.ManualInput, // 945 Deposit
				SSEmployerMatch,
				MedicareEmployerMatch,
				AdditionalMedicareTax,
				AdditionalMedicareWages
			};

			StringBuilder builder = new StringBuilder(FormatLine(lineData));
			CsiList?.ForEach(csi => builder.Append(csi.ToString()));
			CliList?.ForEach(cli => builder.Append(cli.ToString()));
			CspList?.ForEach(csp => builder.Append(csp.ToString()));
			ClpList?.ForEach(clp => builder.Append(clp.ToString()));
			return builder.ToString();
		}

		#region Data
		public virtual DateTime CheckDate { get; set; }
		public virtual decimal? GrossPay { get; set; }
		public virtual decimal? NetPay { get; set; }
		public virtual decimal? SSWages { get; set; }
		public virtual decimal? SSLiability { get; set; }
		public virtual decimal? MedicareWages { get; set; }
		public virtual decimal? MedicareLiability { get; set; }
		public virtual decimal? FederalWages { get; set; }
		public virtual decimal? FederalWHLiability { get; set; }
		public virtual decimal? TaxableFutaWages { get; set; }
		public virtual decimal? FutaLiability { get; set; }
		public virtual decimal? SSTips { get; set; }
		public virtual decimal? TotalFutaWages { get; set; }
		public virtual decimal? EarnedIncomeCredit { get; set; }
		public virtual DateTime? PeriodStart { get; set; }
		public virtual DateTime? PeriodEnd { get; set; }
		public virtual decimal? SSEmployerMatch { get; set; }
		public virtual decimal? MedicareEmployerMatch { get; set; }
		public virtual decimal? AdditionalMedicareTax { get; set; }
		public virtual decimal? AdditionalMedicareWages { get; set; }
		#endregion Data

		#region Children records
		public List<CsiRecord> CsiList { private get; set; }
		public List<CliRecord> CliList { private get; set; }
		public List<CspRecord> CspList { private get; set; }
		public List<ClpRecord> ClpList { private get; set; }
		#endregion
	}
}
