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
	public static partial class ManualEvent
	{
		public static partial class Row<TTable>
		{
			public static class Persisted
			{
				[System.Diagnostics.DebuggerStepThrough]
				public class Args
				{
					public PXCache Cache { get; }
					public PXRowPersistedEventArgs EventArgs { get; }

					public TTable Row => (TTable)EventArgs.Row;
					public PXTranStatus TranStatus => EventArgs.TranStatus;
					public PXDBOperation Operation => EventArgs.Operation;
					public Exception Exception => EventArgs.Exception;

					public Args(PXCache cache, PXRowPersistedEventArgs args) => (Cache, EventArgs) = (cache, args);
					public Args(PXCache cache, TTable row, PXDBOperation operation, PXTranStatus tranStatus, Exception exception)
						: this(cache, new PXRowPersistedEventArgs(row, operation, tranStatus, exception)) { }
				}

				public static void Subscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXRowPersisted>.Subscribe(
						graph,
						handler,
						(g, h) => g.RowPersisted.AddHandler<TTable>(h),
						h => Wrap(h));
				}
				public static void Unsubscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXRowPersisted>.Unsubscribe(
						graph,
						handler,
						(g, h) => g.RowPersisted.RemoveHandler<TTable>(h));
				}
				private static PXRowPersisted Wrap(Action<Args> handler) => (c, e) => handler(new Args(c, e));
			}
		}
	}
}
