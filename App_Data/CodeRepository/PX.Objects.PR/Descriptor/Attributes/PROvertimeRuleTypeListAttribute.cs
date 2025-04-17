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
using PX.Data.BQL;

namespace PX.Objects.PR
{
	public class PROvertimeRuleType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Daily, Weekly, Consecutive },
				new string[] { Messages.DailyRule, Messages.WeeklyRule, Messages.ConsecutiveRule })
			{ }
		}

		public class daily : BqlString.Constant<daily>
		{
			public daily() : base(Daily) { }
		}

		public class weekly : BqlString.Constant<weekly>
		{
			public weekly() : base(Weekly) { }
		}

		public class consecutive : BqlString.Constant<consecutive>
		{
			public consecutive() : base(Consecutive) { }
		}

		public const string Daily = "DAY";
		public const string Weekly = "WEK";
		public const string Consecutive = "CON";
	}
}
