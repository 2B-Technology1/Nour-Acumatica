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

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	/// <summary>
	/// Allowed obligation statuses
	/// </summary>
	public class ObligationStatus
	{
		/// <summary>
		/// List attribute of the obligation status.
		/// </summary>
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Fulfilled, Open },
				new string[] { Messages.Fulfilled, Messages.Open })
			{; }
		}

		public const string Fulfilled = "F";
		public const string Open = "O";

		/// <summary>
		/// Fulfilled obligation status
		/// </summary>
		public class fulfilled : PX.Data.BQL.BqlString.Constant<fulfilled>
		{
			public fulfilled() : base(Fulfilled) {; }
		}

		/// <summary>
		/// Open obligation status
		/// </summary>
		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) {; }
		}
	}
}
