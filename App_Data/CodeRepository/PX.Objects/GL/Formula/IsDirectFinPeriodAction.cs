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
using System;
using System.Collections.Generic;

using static PX.Objects.GL.FinPeriodStatusProcess;

namespace PX.Objects.GL.Formula
{
	public class IsDirectFinPeriodAction<Action> : BqlFormulaEvaluator<Action>, IBqlOperand
		where Action : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			string action = (string)parameters[typeof(Action)];
			return action == null ||
				action == FinPeriodStatusProcessParameters.action.Undefined ||
				FinPeriodStatusProcessParameters.action.GetDirection(action) == FinPeriodStatusProcessParameters.action.Direction.Direct;
		}
	}
}
