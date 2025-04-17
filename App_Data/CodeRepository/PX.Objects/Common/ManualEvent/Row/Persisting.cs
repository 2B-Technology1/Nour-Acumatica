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
using PX.Data;

namespace PX.Objects.Common
{
	public static partial class ManualEvent
	{
		public static partial class Row<TTable>
		{
			public static class Persisting
			{
				[System.Diagnostics.DebuggerStepThrough]
				public class Args
				{
					public PXCache Cache { get; }
					public PXRowPersistingEventArgs EventArgs { get; }

					public TTable Row => (TTable)EventArgs.Row;
					public PXDBOperation Operation => EventArgs.Operation;
					public bool Cancel
					{
						get => EventArgs.Cancel;
						set => EventArgs.Cancel = value;
					}

					public Args(PXCache cache, PXRowPersistingEventArgs args) => (Cache, EventArgs) = (cache, args);
					public Args(PXCache cache, PXDBOperation operation, TTable row)
						: this(cache, new PXRowPersistingEventArgs(operation, row)) { }

					public IDictionary<string, (object OldValue, object NewValue)> GetDifference()
					{
						switch (Operation.Command())
						{
							case PXDBOperation.Insert: return new Dictionary<string, (object, object)> { ["__RowExists__"] = (false, true) };
							case PXDBOperation.Update: return Cache.GetDifference((IBqlTable)Cache.GetOriginal(Row), Row);
							case PXDBOperation.Delete: return new Dictionary<string, (object, object)> { ["__RowExists__"] = (true, false) };
							default: return null;
						}
					}
				}

				public static void Subscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXRowPersisting>.Subscribe(
						graph,
						handler,
						(g, h) => g.RowPersisting.AddHandler<TTable>(h),
						h => Wrap(h));
				}
				public static void Unsubscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXRowPersisting>.Unsubscribe(
						graph,
						handler,
						(g, h) => g.RowPersisting.RemoveHandler<TTable>(h));
				}
				private static PXRowPersisting Wrap(Action<Args> handler) => (c, e) => handler(new Args(c, e));
			}
		}
	}
}
