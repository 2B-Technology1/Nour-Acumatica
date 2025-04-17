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

using System.Collections.Generic;
using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.CR
{
	public class Editable<Field> : IBqlCreator, IBqlOperand
		where Field : IBqlField
	{
		#region IBqlCreator Members

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return false;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			PXCache c = cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
			object row = null;
			if (c.GetItemType().IsAssignableFrom(cache.GetItemType()))
			{
				row = BqlFormula.ItemContainer.Unwrap(item);
			}

			var state = c.GetStateExt<Field>(row) as PXFieldState;

			value = state == null || (state.Enabled && state.Visible && !state.IsReadOnly);
		}

		#endregion
	}
}
