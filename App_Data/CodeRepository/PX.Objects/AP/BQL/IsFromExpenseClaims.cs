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

using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.AP.BQL
{
	/// <summary>
	/// A BQL predicate checking whether its operand (returning a 
	/// screen ID) corresponds to one of the Expense Claims screens.
	/// </summary>
	[Obsolete("This predicate is not used anymore and will be removed in Acumatica ERP 8.0. Use APRegister.OrigModule == 'EP' comparison instead.", false)]
	public class IsFromExpenseClaims<TScreenID> : IBqlUnary
		where TScreenID : IBqlOperand
	{
		public class screenExpenseClaims : PX.Data.BQL.BqlString.Constant<screenExpenseClaims>
		{
			public screenExpenseClaims() : base("EP301030")
			{ }
		}
		public class screenExpenseClaim : PX.Data.BQL.BqlString.Constant<screenExpenseClaim>
		{
			public screenExpenseClaim() : base("EP301000")
			{ }
		}
		public class screenReleaseExpenseClaims : PX.Data.BQL.BqlString.Constant<screenReleaseExpenseClaims>
		{
			public screenReleaseExpenseClaims() : base("EP501000")
			{ }
		}

		private IBqlUnary _where = new Where<
			TScreenID, Equal<screenExpenseClaim>,
			Or<TScreenID, Equal<screenExpenseClaims>,
			Or<TScreenID, Equal<screenReleaseExpenseClaims>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info,
			BqlCommand.Selection selection)
			=> _where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> _where.Verify(cache, item, pars, ref result, ref value);
	}
}
