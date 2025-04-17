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
	public class TaxFormBatchType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Original, Amendment, Cancellation },
				new string[] { Messages.Original, Messages.Amendment, Messages.Cancellation })
			{ }
		}

		public class original : PX.Data.BQL.BqlString.Constant<original>
		{
			public original() : base(Original) { }
		}

		public class amendment : PX.Data.BQL.BqlString.Constant<amendment>
		{
			public amendment() : base(Amendment) { }
		}

		public class cancellation : PX.Data.BQL.BqlString.Constant<cancellation>
		{
			public cancellation() : base(Cancellation) { }
		}

		public const string Original = "O";
		public const string Amendment = "A";
		public const string Cancellation = "C";

		public const string OriginalRL1 = "R";
		public const string AmendmentRL1 = Amendment;
		public const string CancellationRL1 = "D";

		public static string GetRL1TypeCode(string taxFormBatchType)
		{
			switch (taxFormBatchType)
			{
				case Original:
					return OriginalRL1;
				case Amendment:
					return AmendmentRL1;
				case Cancellation:
					return CancellationRL1;
			}

			throw new PXException(Messages.TaxFormBatchTypeIsNotSupported, taxFormBatchType);
		}
	}
}
