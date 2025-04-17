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

namespace PX.Objects.SO
{
	public class ExternalTaxRecalculationScope : IDisposable
	{
		protected bool _disposed = false;

		public class Context
		{
			public int ReferenceCounter { get; set; }
		}

		public ExternalTaxRecalculationScope()
		{
			Context currentContext = PXContext.GetSlot<Context>();
			if (currentContext == null)
			{
				currentContext = new Context();
				PXContext.SetSlot(currentContext);
			}
			currentContext.ReferenceCounter++;
		}

		public void Dispose()
		{
			if (_disposed)
				throw new PXObjectDisposedException();

			_disposed = true;

			Context currentContext = PXContext.GetSlot<Context>();
			currentContext.ReferenceCounter--;

			if (currentContext.ReferenceCounter == 0)
				PXContext.SetSlot<Context>(null);
		}

		public static bool IsScoped()
		{
			var currentContext = PXContext.GetSlot<Context>();
			return currentContext?.ReferenceCounter > 0;
		}
	}
}
