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
	public abstract class PaymentItem : AufRecord
	{
		protected PaymentItem(AufRecordType recordType, DateTime checkDate, int pimID) : base(recordType)
		{
			CheckDate = checkDate;
			PimID = pimID;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				State,
				CheckDate,
				PimID,
				PaymentAmount,
				PaymentDate
			};

			return FormatLine(lineData);
		}

		public virtual string State { get; set; }
		public virtual DateTime CheckDate { get; set; }
		public virtual int PimID { get; set; }
		public virtual decimal? PaymentAmount { get; set; }
		public virtual DateTime? PaymentDate { get; set; }
	}
}
