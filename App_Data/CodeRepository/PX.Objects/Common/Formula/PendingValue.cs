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
using PX.Common;
using PX.Data;
using PX.Data.SQLTree;
using static PX.Data.BqlCommand;

namespace PX.Objects.Common
{
	public class PendingValue<Field> : BqlFormula, IBqlCreator, IBqlOperand
		where Field : IBqlField
	{

		protected bool IsExternalCall;

		/// <exclude />
		public virtual bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return true;
		}

		/// <exclude/>
		public virtual void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			object row;
			BqlFormula.ItemContainer container = item as BqlFormula.ItemContainer;
			if (container != null)
			{
				IsExternalCall = container.IsExternalCall;
			}

			if (container != null && !(container.PendingValue == null || container.PendingValue == PXCache.NotSetValue))
				value = container.PendingValue;
			else
			{
				row = ItemContainer.Unwrap(item);
				value = cache.GetValuePending<Field>(row);
			}
			if (value == null && cache.Graph.IsCopyPasteContext)
				value = new object();

		}
	}

	[PXInternalUseOnly]
	public class IsPending : IBqlComparison
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = (value != null && value != PXCache.NotSetValue);
		}

		public virtual bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, Selection selection)
		{
			return true;
		}
	}

	[PXInternalUseOnly]
	public class IsNotPending : IBqlComparison
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = (value == null || value == PXCache.NotSetValue);
		}

		public virtual bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, Selection selection)
		{
			return true;
		}
	}
}
