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

namespace PX.Objects.FS
{
    public class RunningSelectingScope<DAC> : IDisposable
        where DAC : IBqlTable
    {
        protected List<string> _GraphList;
        protected string _MyGraphSelecting;
        protected readonly RunningSelectingScope<DAC> _Previous;

        public RunningSelectingScope(PXGraph myGraph)
        {
            _MyGraphSelecting = myGraph.GetType().FullName;

            _Previous = PXContext.GetSlot<RunningSelectingScope<DAC>>();
            if (_Previous == null)
            {
                _GraphList = new List<string>();
            }
            else
            {
                _GraphList = new List<string>(_Previous._GraphList);
            }

            _GraphList.Add(_MyGraphSelecting);

            PXContext.SetSlot<RunningSelectingScope<DAC>>(this);
        }
        public void Dispose()
        {
            PXContext.SetSlot<RunningSelectingScope<DAC>>(_Previous);
        }
        public static bool IsRunningSelecting(PXGraph graph)
        {
            RunningSelectingScope<DAC> scope = PXContext.GetSlot<RunningSelectingScope<DAC>>();
            if (scope == null)
            {
                return false;
            }
            else
            {
                return scope._GraphList.Exists(e => e == graph.GetType().FullName);
            }
        }
    }
}
