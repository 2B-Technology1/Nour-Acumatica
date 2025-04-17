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
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;

namespace PX.Objects.CR.Threading
{
	[PXInternalUseOnly]
	public static class TaskHelper
	{
		public static T RunSynchronously<T>(Func<Task<T>> factory)
		{
			try
			{
				if (PXLongOperation.IsLongOperationContext())
					return factory().Result;

				var key = Guid.NewGuid().ToString();
				T result = default;
				Exception ex = null;

				PXLongOperation.StartOperation(key, () =>
				{
					try
					{
						result = factory().Result;
					}
					catch (Exception e)
					{
						ex = e;
					}
				});

				PXLongOperation.WaitCompletion(key);
				PXLongOperation.ClearStatus(key);

				if (ex != null)
					ExceptionDispatchInfo.Capture(ex).Throw();

				return result;
			}
			catch (AggregateException ae)
			{
				RethrowInnerPXException(ae);
				// hack: never get here, just for compiler
				return default;
			}
		}

		public static void RunSynchronously(Func<Task> factory)
		{
			RunSynchronously(() => factory().ContinueWith(t => true));
		}

		private static void RethrowInnerPXException(AggregateException ae)
		{
			Exception exception = ae;
			if (ae.InnerException is PXException pxe)
				exception = pxe;
			ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}
}
