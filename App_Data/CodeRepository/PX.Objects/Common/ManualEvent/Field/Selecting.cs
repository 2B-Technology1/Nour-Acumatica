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
using System.Diagnostics;
using PX.Data;

namespace PX.Objects.Common
{
	public static partial class ManualEvent
	{
		public static partial class FieldOf<TTable>
		{
			public static class Selecting
			{
				[DebuggerStepThrough]
				public class Args
				{
					public PXCache Cache { get; }
					public PXFieldSelectingEventArgs EventArgs { get; }

					public TTable Row => (TTable)EventArgs.Row;
					public bool ExternalCall => EventArgs.ExternalCall;
					public bool IsAltered
					{
						get => EventArgs.IsAltered;
						set => EventArgs.IsAltered = value;
					}
					public object ReturnValue
					{
						get => EventArgs.ReturnValue;
						set => EventArgs.ReturnValue = value;
					}
					public object ReturnState
					{
						get => EventArgs.ReturnState;
						set => EventArgs.ReturnState = value;
					}
					public bool Cancel
					{
						get => EventArgs.Cancel;
						set => EventArgs.Cancel = value;
					}

					public Args(PXCache cache, PXFieldSelectingEventArgs args) => (Cache, EventArgs) = (cache, args);
					public Args(PXCache cache, TTable row, object returnValue, bool isAltered, bool externalCall)
						: this(cache, new PXFieldSelectingEventArgs(row, returnValue, isAltered, externalCall)) { }
				}
				public static void Subscribe(PXGraph graph, string fieldName, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXFieldSelecting>.Subscribe(
						graph,
						handler,
						(g, h) => g.FieldSelecting.AddHandler(typeof(TTable), fieldName, h),
						h => Wrap(h));
				}
				public static void Unsubscribe(PXGraph graph, string fieldName, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXFieldSelecting>.Unsubscribe(
						graph,
						handler,
						(g, h) => g.FieldSelecting.RemoveHandler(typeof(TTable), fieldName, h));
				}
				private static PXFieldSelecting Wrap(Action<Args> handler) => (c, e) => handler(new Args(c, e));
			}
		}

		public static partial class FieldOf<TTable, TField>
		{
			public static class Selecting
			{
				[DebuggerStepThrough]
				public class Args : FieldOf<TTable>.Selecting.Args
				{
					public Args(PXCache cache, PXFieldSelectingEventArgs args) : base(cache, args) { }
					public Args(PXCache cache, TTable row, object returnValue, bool isAltered, bool externalCall) : base(cache, row, returnValue, isAltered, externalCall) { }
				}
				public static void Subscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXFieldSelecting>.Subscribe(
						graph,
						handler,
						(g, h) => g.FieldSelecting.AddHandler(typeof(TTable), typeof(TField).Name, h),
						h => Wrap(h));
				}
				public static void Unsubscribe(PXGraph graph, Action<Args> handler)
				{
					Synchronizer<Action<Args>, PXFieldSelecting>.Unsubscribe(
						graph,
						handler,
						(g, h) => g.FieldSelecting.RemoveHandler(typeof(TTable), typeof(TField).Name, h));
				}
				private static PXFieldSelecting Wrap(Action<Args> handler) => (c, e) => handler(new Args(c, e));
			}

		}
	}
}
