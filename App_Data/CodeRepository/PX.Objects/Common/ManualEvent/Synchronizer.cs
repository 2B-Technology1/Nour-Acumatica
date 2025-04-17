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
using System.Reflection;
using System.Runtime.CompilerServices;
using PX.Data;

namespace PX.Objects.Common
{
	public static partial class ManualEvent
	{
		private static readonly object StaticTarget = new object();

		private class Synchronizer<TModernDelegate, TOrigDelegate>
			where TModernDelegate : class
			where TOrigDelegate : class
		{
			private static readonly ConditionalWeakTable<PXGraph, ConditionalWeakTable<object, Dictionary<MethodInfo, TOrigDelegate>>> Storage =
				new ConditionalWeakTable<PXGraph, ConditionalWeakTable<object, Dictionary<MethodInfo, TOrigDelegate>>>();

			public static void Subscribe(PXGraph graph, TModernDelegate handler, Action<PXGraph, TOrigDelegate> subscription, Func<TModernDelegate, TOrigDelegate> wrapping)
			{
				TOrigDelegate origHandler = wrapping(handler);

				if (handler is Delegate del)
				{
					if (Storage.TryGetValue(graph, out ConditionalWeakTable<object, Dictionary<MethodInfo, TOrigDelegate>> targetMethods))
					{
						if (targetMethods.TryGetValue(del.Target ?? StaticTarget, out var methodsToDelegates))
						{
							methodsToDelegates[del.Method] = origHandler;
						}
						else
						{
							targetMethods.Add(del.Target ?? StaticTarget, new Dictionary<MethodInfo, TOrigDelegate> { [del.Method] = origHandler });
						}
					}
					else
					{
						targetMethods = new ConditionalWeakTable<object, Dictionary<MethodInfo, TOrigDelegate>>();
						targetMethods.Add(del.Target ?? StaticTarget, new Dictionary<MethodInfo, TOrigDelegate> { [del.Method] = origHandler });
						Storage.Add(graph, targetMethods);
					}
				}

				subscription(graph, origHandler);
			}

			public static void Unsubscribe(PXGraph graph, TModernDelegate handler, Action<PXGraph, TOrigDelegate> unsubscription)
			{
				if (handler is Delegate del)
					if (Storage.TryGetValue(graph, out var targetMethods))
						if (targetMethods.TryGetValue(del.Target ?? StaticTarget, out var methodToDelegates))
							if (methodToDelegates.TryGetValue(del.Method, out var origHandler))
								unsubscription(graph, origHandler);
			}
		}

		public static partial class Row<TTable>
			where TTable : class, IBqlTable, new()
		{ }

		public static partial class FieldOf<TTable>
			where TTable : class, IBqlTable, new()
		{ }

		public static partial class FieldOf<TTable, TField>
			where TTable : class, IBqlTable, new()
			where TField : IBqlField
		{ }
	}
}
