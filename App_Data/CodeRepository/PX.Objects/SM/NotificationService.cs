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

using PX.Common;
using PX.Data;
using PX.SM;

namespace PX.Objects.SM
{
	public class NotificationService : INotificationService
	{
		private PXGraph _graph;

		public INotification Read(string notificationCD)
		{
			PXSelect<Notification,
					Where<Notification.notificationID, Equal<Required<Notification.notificationID>>>>.
				Clear(Graph);
			_graph.Caches[typeof(Notification)].Clear();//if it fails replace PXSelect with PXSelectReadonly
			return notificationCD.With(_ => (Notification)PXSelect<Notification,
					Where<Notification.notificationID, Equal<Required<Notification.notificationID>>>>.
				Select(Graph, _));
		}

		private PXGraph Graph
		{
			get { return _graph ?? (_graph = new PXGraph()); }
		}
	}
}
