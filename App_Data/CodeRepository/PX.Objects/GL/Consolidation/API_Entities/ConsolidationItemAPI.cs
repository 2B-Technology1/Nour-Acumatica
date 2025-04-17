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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.IO;

namespace PX.Objects.GL.Consolidation
{
	internal class ConsolidationItemAPITmp
	{
		public virtual ApiProperty<string> AccountCD { get; set; }
		public virtual ApiProperty<decimal?> ConsolAmtCredit { get; set; }
		public virtual ApiProperty<decimal?> ConsolAmtDebit { get; set; }
		public virtual ApiProperty<string> FinPeriodID { get; set; }
		public virtual ApiProperty<string> MappedValue { get; set; }
		public virtual ApiProperty<int?> MappedValueLength { get; set; }
		public ConsolidationItemAPI ToApiItem()
		{
			return new ConsolidationItemAPI()
			{
				AccountCD = AccountCD.value,
				ConsolAmtCredit = ConsolAmtCredit.value,
				ConsolAmtDebit = ConsolAmtDebit.value,
				FinPeriodID = string.IsNullOrEmpty(FinPeriodID.value) ? string.Empty : FinPeriodID.value.Substring(2, 4) + FinPeriodID.value.Substring(0, 2),
				MappedValue = MappedValue.value,
				MappedValueLength = MappedValueLength.value
			};

		}
	}
	internal class ConsolidationItemAPI
	{
		public virtual string AccountCD { get; set; }
		public virtual decimal? ConsolAmtCredit { get; set; }
		public virtual decimal? ConsolAmtDebit { get; set; }
		public virtual string FinPeriodID { get; set; }
		public virtual string MappedValue { get; set; }
		public virtual int? MappedValueLength { get; set; }
	}
}
