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
using PX.Concurrency;
using PX.Data;
using PX.Data.EP;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.SM;

namespace PX.Objects.SM
{
	public class AccessExt : PXGraphExtension<Access>
	{
		[InjectDependency]
		private IMailSendProvider MailSendProvider { get; set; }

		[PXOverride]
		public void SendUserNotification(int? accountId, Notification notification, Action<int?, Notification> del)
		{
			if (accountId == null || notification == null)
				return;

			var graph = Base.CloneGraphState();
			graph.UserList.Current = Base.UserList.Current;
			Base.LongOperationManager.StartOperation(Base, ct =>
			{
				var gen = TemplateNotificationGenerator.Create(graph, graph.UserList.Current, notification);
				gen.MailAccountId = accountId;
				gen.To            = graph.UserList.Current.Email;
				gen.LinkToEntity  = true;
				gen.Body          = gen.Body.Replace("((UserList.Password))", graph.UserList.Current.Password);

				ct.ThrowIfCancellationRequested();
				var activities = gen.Send();
				var provider = graph.GetExtension<AccessExt>().MailSendProvider;
				foreach (SMEmail email in gen.CastToSMEmail(activities))
				{
					ct.ThrowIfCancellationRequested();
					provider.SendMessage(email);
				}
			});

		}
	}
}
