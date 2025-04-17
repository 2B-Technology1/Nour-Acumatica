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
using System.Linq;
using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// The class allows to emulate the behavior of unique index via <see cref="PXGraph.OnBeforeCommit"/> event.
	/// </summary>
	/// <typeparam name="TSelect">The query which must return a unique record.</typeparam>
	public class UniquenessChecker<TSelect>
		where TSelect : BqlCommand, new()
	{
		IBqlTable _binding;

		public UniquenessChecker(IBqlTable binding)
		{
			_binding = binding;
		}

		public virtual void OnBeforeCommitImpl(PXGraph graph)
		{
			BqlCommand command = new TSelect();
			List<object> result = new PXView(graph, true, command).SelectMultiBound(new[] { _binding });
			if (result.Count > 1)
			{
				var cache = graph.Caches[_binding.GetType()];
				var keys = cache.Keys.Select(f => cache.GetValue(_binding, f)).ToArray();
				throw new PXLockViolationException(_binding.GetType(), PXDBOperation.Update, keys);
			}
		}
	}
}
