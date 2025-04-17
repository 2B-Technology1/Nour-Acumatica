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
using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.AM.Attributes
{
    public sealed class MathCeiling<Operand> : BqlFunction, IBqlOperand, IBqlCreator
        where Operand : IBqlOperand
    {
        private IBqlCreator _operand;

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            value = null;
            if (!getValue<Operand>(ref _operand, cache, item, pars, ref result, out value) || value == null)
            {
                return;
            }
            value = calculateValue(value);
        }

        internal static object calculateValue(object value1)
        {
            return Math.Ceiling(Convert.ToDecimal(value1));
        }

        public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
        {
            bool status = true;
            if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
            {
                info.Fields?.Add(typeof(Operand));
                return true;
            }

            if (_operand == null)
            {
                _operand = Activator.CreateInstance<Operand>() as IBqlCreator;
            }
            if (_operand == null)
            {
                throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
            }

            _operand.AppendExpression(ref exp, graph, info, selection);
            return status;
        }
    }
}
