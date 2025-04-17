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
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class EarningTaskStatusWarningAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (PREarningDetail)e.Row;
			PMTask task = PXSelect<PMTask>.Search<PMTask.projectID, PMTask.taskID>(sender.Graph, row.ProjectID, e.NewValue);
			if (task != null && task.Status != ProjectTaskStatus.Active)
			{
				var listAttribute = new ProjectTaskStatus.ListAttribute();
				string status = listAttribute.ValueLabelDic[task.Status];

				sender.RaiseExceptionHandling<PREarningDetail.projectTaskID>(e.Row, e.NewValue,
					new PXSetPropertyException(Messages.TaskStatusWarning, PXErrorLevel.Warning, status));
			}
		}
	}
}
