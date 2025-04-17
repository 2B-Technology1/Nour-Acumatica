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
using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// Represents an event handler for the RowUpdated event that subscribes as late as possible.
	/// </summary>
	public abstract class LateRowUpdatedAttribute : PXEventSubscriberAttribute
	{
		public Type TargetTable { get; set; }

		public override void CacheAttached(PXCache cache)
		{
			base.CacheAttached(cache);

			cache.Graph.Initialized -= LateSubscription;
			cache.Graph.Initialized += LateSubscription;
		}

		private void LateSubscription(PXGraph graph)
		{
			graph.RowUpdated.RemoveHandler(TargetTable ?? BqlTable, LateRowUpdated);
			graph.RowUpdated.AddHandler(TargetTable ?? BqlTable, LateRowUpdated);
		}

		protected abstract void LateRowUpdated(PXCache cache, PXRowUpdatedEventArgs args);
	}
}