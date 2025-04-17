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
using System.Linq;

namespace PX.Objects.PR
{
	public class HideValueIfDisabledAttribute : PXUIEnabledAttribute
	{
		private bool? _Enable = null;
		private Type _ShowCondition = null;

		public HideValueIfDisabledAttribute(Type enableAndShowCondition) : base(enableAndShowCondition)
		{
			_ShowCondition = enableAndShowCondition;
		}

		public HideValueIfDisabledAttribute(bool enable, Type showCondition) : base(null)
		{
			_Enable = enable;
			_ShowCondition = showCondition;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			PXFieldState fieldState = e.ReturnState as PXFieldState;
			if (e.ReturnState == null || e.Row == null)
			{
				return;
			}

			if (_Enable != null)
			{
				fieldState.Enabled = _Enable.Value;
			}

			if (_ShowCondition != null && !ConditionEvaluator.GetResult(sender, e.Row, _ShowCondition))
			{
				e.ReturnValue = null;
			}
		}
	}
}
