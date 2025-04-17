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

using CommonServiceLocator;
using PX.Common;
using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.CN.Common.Extensions
{
    public static class GraphExtensions
    {
        public static void RedirectToEntity(this PXGraph graph, object row, PXRedirectHelper.WindowMode windowMode)
        {
            if (row != null)
            {
                PXRedirectHelper.TryRedirect(graph, row, windowMode);
            }
        }

        public static TService GetService<TService>(this PXGraph graph)
        {
			return ((Func<PXGraph, TService>)ServiceLocator.Current
                .GetService(typeof(Func<PXGraph, TService>)))(graph);
        }

		public static bool HasErrors(this PXGraph graph)
		{
			var attributes = graph.Views.Caches
				.Where(key => graph.Caches.ContainsKey(key))
				.SelectMany(key => graph.Caches[key].GetAttributes(null));
			return attributes.Any(a =>
				a is IPXInterfaceField field && field.ErrorLevel.IsIn(PXErrorLevel.Error, PXErrorLevel.RowError));
		}
    }
}