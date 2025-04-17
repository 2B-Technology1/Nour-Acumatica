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

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Selector allowing a view to be ordered.
    /// </summary>
    public class OrderedSelect<Table, Where, OrderBy> : PXSelectBase<Table>
            where Table : class, IBqlTable, new()
            where Where : IBqlWhere, new()
            where OrderBy : IBqlOrderBy, new()
    {
        public OrderedSelect(PXGraph graph)
        {
            _Graph = graph;
            View = new PXView(graph, false, new Select<Table, Where, OrderBy>());
        }

        public OrderedSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            View = new PXView(graph, false, new Select<Table, Where, OrderBy>(), handler);
        }
    }
}
