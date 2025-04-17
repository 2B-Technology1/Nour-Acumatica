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
    [PXDynamicButton(new string[] { PasteLineCommand, ResetOrderCommand },
        new string[] { AM.Messages.PasteLine, AM.Messages.ResetLines },
        TranslationKeyType = typeof(AM.Messages))]
    public class AMOrderedMatlSelect<Primary, Table, Where, OrderBy> : PXOrderedSelectBase<Primary, Table>
        where Primary : class, IBqlTable, new()
        where Table : class, IBqlTable, ISortOrder, new()
        where Where : IBqlWhere, new()
        where OrderBy : IBqlOrderBy, new()
    {
        public AMOrderedMatlSelect(PXGraph graph)
        {
            _Graph = graph;
            Initialize();

            View = new PXView(graph, false, new Select<Table, Where, OrderBy>());
        }
        public AMOrderedMatlSelect(PXGraph graph, Delegate handler)
        {
            _Graph = graph;
            Initialize();

            View = new PXView(graph, false, new Select<Table, Where, OrderBy>(), handler);
        }

        public override void Initialize()
        {
            base.Initialize();
            // Replacing "Reset Order" to "Reset Lines" ...
            _Graph.Actions[ResetOrderCommand].SetCaption(Messages.GetLocal(Messages.ResetLines));
            RenumberTailOnDelete = false;
        }

        public override void RenumberAll()
        {
            foreach (Table line in Select())
            {
                Cache.SetValue(line, nameof(ISortOrder.SortOrder), line.LineNbr);
                Cache.MarkUpdated(line);
                Cache.IsDirty = true;
            }
        }
    }
}