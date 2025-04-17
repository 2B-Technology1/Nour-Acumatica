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
using PX.Objects.IN.Attributes;

namespace PX.Objects.CR
{
	public class PXRestrictorWithEraseAttribute : RestrictorWithParametersAttribute
	{
		#region ctor

		public PXRestrictorWithEraseAttribute(Type where, string message, params Type[] pars)
			: base(where, message, pars)
		{
			this.ShowWarning = true;
		}

		#endregion

		#region Events

		protected override PXException TryVerify(PXCache sender, PXFieldVerifyingEventArgs e, bool IsErrorValueRequired)
		{
			var ex = base.TryVerify(sender, e, IsErrorValueRequired);

			sender.AdjustUI(e.Row)
				.For(this.FieldName, attribute =>
				{
					// in case no error is still apllied to the field - try to add new error. If the error exists already - just skip
					if (attribute.ErrorLevel == PXErrorLevel.Undefined)
					{
						attribute.ExceptionHandling(sender, new PXExceptionHandlingEventArgs(e.Row, null, ex != null
							? new PXSetPropertyException(ex.MessageNoPrefix, PXErrorLevel.Error)
							: null));
					}
				});

			return ex;
		}

		#endregion
	}
}
