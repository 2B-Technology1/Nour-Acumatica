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

using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.AP
{
	public class IsPOLinkedAPBill : IBqlUnary
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = result = PXSelect<
				APTran,
				Where<
					APTran.tranType, Equal<Current<APInvoice.docType>>,
					And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
					And<Where<
						APTran.pOLineNbr, IsNotNull,
						Or<APTran.pONbr, IsNotNull,
						Or<APTran.receiptLineNbr, IsNotNull,
						Or<APTran.receiptNbr, IsNotNull>>>>>>>>
				.SelectSingleBound(cache.Graph, new[] { item }).AsEnumerable().Any();
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) 
		{
			return true;
		}

		public static bool Ensure(PXCache cache, APInvoice bill)
		{
			bool? result = null;
			object value = null;

			new IsPOLinkedAPBill().Verify(cache, bill, new List<object>(), ref result, ref value);
			return result == true;
		}
	}
}
