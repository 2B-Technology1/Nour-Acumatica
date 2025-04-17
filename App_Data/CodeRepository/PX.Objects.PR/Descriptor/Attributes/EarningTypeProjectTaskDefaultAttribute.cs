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
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public class EarningTypeProjectTaskDefaultAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _ProjectField;
		private Type _ProjectTaskField;

		public EarningTypeProjectTaskDefaultAttribute(Type projectField, Type projectTaskField)
		{
			_ProjectField = projectField;
			_ProjectTaskField = projectTaskField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPEarningType earningType = PXSelectorAttribute.Select(sender, e.Row, _FieldName) as EPEarningType;
			if (earningType?.ProjectID != null)
			{
				sender.SetValue(e.Row, _ProjectField.Name, earningType.ProjectID);
				sender.SetValue(e.Row, _ProjectTaskField.Name, earningType.TaskID);
			}
		}
	}
}
