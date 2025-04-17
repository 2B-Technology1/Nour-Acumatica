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
using PX.Objects.CS;

namespace PX.Objects.SO
{
	/// <summary>
	/// Special projection for SOShipLineSplit records.
	/// It returns both assigned and unassigned records in the scope of reports or generic inquiries,
	/// but only one type of records depending on the passed parameter in the scope of other graphs.
	/// </summary>
	public class SOShipLineSplitProjectionAttribute : PXProjectionAttribute
	{
		protected bool _isUnassignedValue;
        protected Type _customselect;

		public SOShipLineSplitProjectionAttribute(Type select, Type unassignedType, bool isUnassignedValue)
			: base(select)
		{
            _isUnassignedValue = isUnassignedValue;

            Type[] args = select.GetGenericArguments();
            // TODO: rewrite with BqlTemplate
            _customselect = _isUnassignedValue ?
                BqlCommand.Compose(
                    typeof(Select<,>),
                    args[0],
                    typeof(Where<,>),
                    unassignedType,
                    typeof(Equal<True>)) :
                BqlCommand.Compose(
                    typeof(Select<,>),
                    args[0],
                    typeof(Where<,>),
                    unassignedType,
                    typeof(Equal<False>));

            Persistent = true;
		}

		protected override Type GetSelect(PXCache sender)
		{
            if (sender.Graph.GetType() == typeof(PXGraph)   //report mode
                || sender.Graph.GetType() == typeof(PXGenericInqGrph))
            {
                return base.GetSelect(sender);
            }
            else
            {
                return _customselect;
            }
		}
	}
}
