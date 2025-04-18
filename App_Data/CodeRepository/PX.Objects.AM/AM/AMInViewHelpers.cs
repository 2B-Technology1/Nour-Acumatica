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
using System;
using System.Linq;

namespace PX.Objects.AM
{
    public class AMInViewGraphCacheContainer<TType>
        where TType : IBqlTable, new()
    {
        public PXCache Cache { get; }
        public TType Current { get; }

        private AMInViewGraphCacheContainer(PXCache cache)
        {
            Cache = cache;
            Current = (TType)cache.Current;
        }

        public static AMInViewGraphCacheContainer<TType> Create(PXCache cache)
        {
            if (cache == null)
                return null;
            return new AMInViewGraphCacheContainer<TType>(cache);
        }
    }

    public class AMInViewSelectReadOnly<TTable, TWhere> : AMInViewSelect<TTable, TWhere>
        where TTable : class, IBqlTable, new()
        where TWhere : IBqlWhere, new()
    {
        public AMInViewSelectReadOnly(PXGraph graph) : base(graph)
        {
            View.IsReadOnly = true;
        }
    }

    public class AMInViewSelect<TTable, TWhere> : PXSelect<TTable, TWhere>
        where TTable : class, IBqlTable, new()
        where TWhere : IBqlWhere, new()
    {

        public AMInViewSelect(PXGraph graph) : base(graph)
        {
            graph.Caches[typeof(TTable)] = Cache;
            graph.Views[CreateViewName()] = View;
            graph.Views.Caches.Add(typeof(TTable));
        }

        private string CreateViewName()
        {
            return String.Format("{0}${1}", typeof(TTable).Name, View.ToString());
        }
    }

    public static class AMInViewHelper
    {
        public static void InitializeViews<TTable>(this PXSelect<TTable> baseView, PXGraph graph)
            where TTable : class, IBqlTable, new()
        {
            foreach (var dataView in baseView
                                    .GetType()
                                    .GetFields()
                                    .Where(se =>
                                        se.FieldType.
                                            IsSubclassOf(typeof(PXSelectBase))))
            {
                dataView.SetValue(baseView,
                    Activator.CreateInstance(dataView.FieldType,
                                             graph));
            }
        }

        public static AMInViewGraphCacheContainer<TType> GetCacheCurrent<TType>(this PXGraph graph)
            where TType : IBqlTable, new()
        {
            var cache = graph.Caches[typeof(TType)];
            if (cache == null)
                return null;
            return AMInViewGraphCacheContainer<TType>.Create(cache);
        }
    }
}
