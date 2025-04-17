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
using System.Linq;

namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the payment defined
	/// by its key fields (document type and reference number) has an unreleased 
	/// void payment. This may be needed to exclude such payments from processing
	/// to prevent creating unnecessary applications, see e.g. the
	/// <see cref="ARAutoApplyPayments"/> graph.
	/// </summary>
	public class HasUnreleasedVoidPayment<TDocTypeField, TRefNbrField> : IBqlUnary
		where TDocTypeField : IBqlOperand
		where TRefNbrField : IBqlOperand
    {
		private readonly IBqlCreator exists = new Exists<Select<
			ARRegisterAlias2,
				Where<
					ARRegisterAlias2.docType, Equal<Switch<Case<Where<TDocTypeField, Equal<ARDocType.refund>>, ARDocType.voidRefund>, ARDocType.voidPayment>>,
					And<ARRegisterAlias2.docType, NotEqual<TDocTypeField>,
					And<ARRegisterAlias2.refNbr, Equal<TRefNbrField>,
					And<ARRegisterAlias2.released, NotEqual<True>>>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);

		private string GetFieldName<T>()
		{
			if (typeof(IBqlField).IsAssignableFrom(typeof(T)))
			{
				return typeof(T).Name;
			}
			else if (typeof(IBqlParameter).IsAssignableFrom(typeof(T)))
			{
				return (Activator.CreateInstance<T>() as IBqlParameter).GetReferencedType().Name;
			}
			else
				return null;
		}

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
			string docType = cache.GetValue(item, GetFieldName<TDocTypeField>()) as string;
			string refNbr = cache.GetValue(item, GetFieldName<TRefNbrField>()) as string;

			value = result = Select(cache.Graph, ARPaymentType.GetVoidingARDocType(docType), docType, refNbr) != null;
        }

        public static bool Verify(PXGraph graph, ARRegister payment)
        {
            bool? result = null;
            object value = null;

            new HasUnreleasedVoidPayment<ARRegister.docType, ARRegister.refNbr>().Verify(
                graph.Caches[payment.GetType()],
                payment,
                new List<object>(0),
                ref result,
                ref value);

            return result == true;
        }

		public static ARRegister Select(PXGraph graph, ARRegister payment)
		{
			if (payment == null || payment.RefNbr == null || payment.DocType == null)
			{
				return null;
			}
			return Select(graph, ARPaymentType.GetVoidingARDocType(payment.DocType), payment.DocType, payment.RefNbr);
		}

		public static ARRegister Select(PXGraph graph, string voidDocType, string docType, string refNbr)
            => PXSelect<
                ARRegisterAlias2,
                Where<
                    ARRegisterAlias2.docType, Equal<Required<ARRegister.docType>>,
                    And<ARRegisterAlias2.docType, NotEqual<Required<ARRegister.docType>>,
                    And<ARRegisterAlias2.refNbr, Equal<Required<ARRegister.refNbr>>,
                    And<ARRegisterAlias2.released, NotEqual<True>>>>>>
                .SelectWindowed(graph, 0, 1, voidDocType, docType, refNbr)
                .RowCast<ARRegisterAlias2>()
                .FirstOrDefault();
    }
}
