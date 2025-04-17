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
using PX.Data.SQLTree;
using PX.Objects.AR.Standalone;
using System;
using System.Collections.Generic;

namespace PX.Objects.AR.BQL
{
	class HasApplicationToUnreleasedCM<TDocTypeField, TRefNbrField> : IBqlUnary
		where TDocTypeField : IBqlOperand
		where TRefNbrField : IBqlOperand
	{
		public readonly static BqlCommand Select = new Select2<
				ARAdjust,
				InnerJoin<ARRegisterAlias,
					On<ARAdjust.adjdDocType, Equal<ARRegisterAlias.docType>,
						And<ARAdjust.adjdRefNbr, Equal<ARRegisterAlias.refNbr>>>>,
				Where<
					ARAdjust.adjgDocType, Equal<TDocTypeField>,
					And<ARAdjust.adjgRefNbr, Equal<TRefNbrField>,
					And<ARAdjust.adjdDocType, Equal<ARDocType.creditMemo>,
					And<ARRegisterAlias.released, NotEqual<True>>>>>>();

		private IBqlCreator exist => (IBqlCreator)Activator.CreateInstance(typeof(Exists<>).MakeGenericType(Select.GetType()));

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return exist.AppendExpression(ref exp, graph, info, selection);
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			exist.Verify(cache, item, pars, ref result, ref value);
		}
	}
}
