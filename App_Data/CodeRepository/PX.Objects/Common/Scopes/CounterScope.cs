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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.Common.Scopes
{
    public abstract class BaseCounterScope : IDisposable
    {
        private readonly string _scopeKey;

        protected BaseCounterScope(string scopeKey)
        {
            _scopeKey = scopeKey;
            Inc();
        }

        void IDisposable.Dispose() => Dec();

        protected virtual int Inc()
        {
            var cntr = GetCounter();
            return PXContext.SetSlot<int>(_scopeKey, cntr + 1);
        }

        protected virtual int Dec()
        {
            var cntr = GetCounter();
            if (cntr == 1)
            {
                PXContext.ClearSlot(_scopeKey);
                return 0;
            }
            else
                return PXContext.SetSlot<int>(_scopeKey, cntr - 1);
        }

        public int GetCounter() => GetCounter(_scopeKey);

        protected static int GetCounter(string scopeKey) => PXContext.GetSlot<int>(scopeKey);

        protected static bool IsEmpty(string scopeKey) => GetCounter(scopeKey) == 0;
    }

    public class CounterScope : BaseCounterScope
    {
        private static string ScopeKey(string scopePosfix = null) => string.IsNullOrEmpty(scopePosfix) ? nameof(CounterScope) : scopePosfix;
        private static string ScopeKey(PXGraph graph, string scopePosfix = null) => ScopeKey(scopePosfix) + "For" + graph.UID;

        public static new bool IsEmpty(string scopePosfix = null) => BaseCounterScope.IsEmpty(ScopeKey(scopePosfix));
        public static bool IsEmpty(PXGraph graph, string scopePosfix = null) => BaseCounterScope.IsEmpty(ScopeKey(graph, scopePosfix));

        public CounterScope(string scopePosfix = null) : base(ScopeKey(scopePosfix))
        { }

        public CounterScope(PXGraph graph, string scopePosfix = null) 
            : base(ScopeKey(graph, scopePosfix))
        { }
    }

    public abstract class CounterScope<TSelf> : CounterScope
        where TSelf : CounterScope<TSelf>
    {
        public CounterScope(PXGraph graph) : base(graph, typeof(TSelf).FullName) { }

        public static bool IsEmpty(PXGraph graph) => IsEmpty(graph, typeof(TSelf).FullName);

        public static bool Suppressed(PXGraph graph) => !IsEmpty(graph);
    }
}
