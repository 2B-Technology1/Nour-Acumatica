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
using PX.Data;
using PX.Data.BQL;
using PX.Objects.TX;
using Messages = PX.Objects.TX.Messages;

namespace PX.Objects.Extensions.PerUnitTax
{
	/// <summary>
	/// A set of post options for per unit tax amount during the release of the document.
	/// </summary>
	public class PerUnitTaxPostOptions
	{
		/// <summary>
		/// Post per-unit tax amount to the document line account.
		/// </summary>
		public const string LineAccount = "L";

		/// <summary>
		/// Post per-unit tax amount to the account specified in tax settings.
		/// </summary>
		public const string TaxAccount = "T";

		/// <summary>
		/// Post per-unit tax amount to the document line account.
		/// </summary>
		public class lineAccount : BqlString.Constant<lineAccount>
		{
			public lineAccount() : base(LineAccount) {; }
		}

		/// <summary>
		/// Post per-unit tax amount to the account specified in tax settings.
		/// </summary>
		public class taxAccount : BqlString.Constant<taxAccount>
		{
			public taxAccount() : base(TaxAccount) {; }
		}

		/// <summary>
		/// String list attribute with a list of per unit tax post options. 
		/// </summary>
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { LineAccount, TaxAccount },
				new string[] { Messages.PostPerUnitTaxToLineAccount, Messages.PostPerUnitTaxToTaxAccount })
			{ }
		}
	}
}
