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
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Data;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.DataView
{
    [PXDynamicButton(
        new[]
        {
            DrawingLogLabels.DisciplinesOrderedSelect.PasteLine,
            DrawingLogLabels.DisciplinesOrderedSelect.ResetOrder
        },
        new[]
        {
            DrawingLogLabels.DisciplinesOrderedSelect.PasteLine,
            DrawingLogLabels.DisciplinesOrderedSelect.ResetOrder
        })]
    public sealed class DisciplinesOrderedSelect<TPrimary, TEntity, TOrderBy> : PXOrderedSelectBase<TPrimary, TEntity>
        where TPrimary : class, IBqlTable, new()
        where TEntity : class, IBqlTable, ISortOrder, new()
        where TOrderBy : IBqlOrderBy, new()
    {
        public DisciplinesOrderedSelect(PXGraph graph)
        {
            _Graph = graph;
            Initialize();
            View = new PXView(graph, false, new Select3<TEntity, TOrderBy>());
        }

        public DisciplinesOrderedSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            Initialize();
            View = new PXView(graph, false, new Select3<TEntity, TOrderBy>(), handler);
        }
    }
}