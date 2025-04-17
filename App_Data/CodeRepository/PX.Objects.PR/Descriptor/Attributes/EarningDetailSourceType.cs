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
	public class EarningDetailSourceType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { TimeActivity, QuickPay, SalesCommission },
				new string[] { Messages.TimeActivity, Messages.QuickPay, Messages.SalesCommission })
			{ }
		}

		public class timeActivity : BqlString.Constant<timeActivity>
		{
			public timeActivity() : base(TimeActivity) { }
		}

		public const string TimeActivity = "TMA";
		public const string QuickPay = "QPY";
		public const string SalesCommission = "SPC"; //Sales Person Commission
	}
}
