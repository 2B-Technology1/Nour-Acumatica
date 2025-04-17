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

namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A predicate which returns <c>true</c> whenever the value of its operand field
	/// does not correspond to a self-applying document, which has no balance, e.g. a 
	/// <see cref="ARDocType.cashSale">Cash Sale</see> or a 
	/// <see cref="ARDocType.cashReturn">Cash Return</see>.
	/// </summary>
	public class IsNotSelfApplying<TDocTypeField> : IBqlUnary
		where TDocTypeField : IBqlOperand
	{
		private IBqlCreator _where = new Where<
			TDocTypeField, NotEqual<ARDocType.cashSale>,
			And<TDocTypeField, NotEqual<ARDocType.cashReturn>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> _where.Verify(cache, item, pars, ref result, ref value);
	}
}
