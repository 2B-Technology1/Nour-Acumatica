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

namespace PX.Objects.EP
{
	public class ShowValueWhenAttribute : PXBaseConditionAttribute, IPXFieldSelectingSubscriber, IPXRowPersistingSubscriber
	{
		private bool _ClearOnPersisting = false;

		public ShowValueWhenAttribute(Type conditionType, bool clearOnPersisting = false)
			: base(conditionType)
		{
			_ClearOnPersisting = clearOnPersisting;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null || _Condition == null)
			{
				return;
			}

			if (!GetConditionResult(sender, e.Row, Condition))
			{
				e.ReturnValue = null;
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_ClearOnPersisting && !GetConditionResult(sender, e.Row, Condition))
			{
				sender.SetValue(e.Row, _FieldName, null);
			}
		}
	}
}
