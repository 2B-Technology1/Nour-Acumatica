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

namespace PX.Objects.PR
{
	public class SettlementBalanceType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Pay, Keep, Discard},
				new string[] { Messages.Pay, Messages.Keep, Messages.Discard})
			{ }
		}

		public class pay : PX.Data.BQL.BqlString.Constant<pay>
		{
			public pay() : base(Pay) { }
		}

		public class keep : PX.Data.BQL.BqlString.Constant<keep>
		{
			public keep() : base(Keep) { }
		}

		public class discard : PX.Data.BQL.BqlString.Constant<discard>
		{
			public discard() : base(Discard) { }
		}

		public const string Pay = "PAY";
		public const string Keep = "KEE";
		public const string Discard = "DIS";
	}
}
