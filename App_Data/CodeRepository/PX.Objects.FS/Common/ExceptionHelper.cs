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
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public static class ExceptionHelper
    {
        public static Exception GetExceptionWithContextMessage(string contextMessage, Exception ex)
        {
            if (ex is PXLockViolationException && ex.Message.Contains(typeof(FSPostRegister).Name))
            {
                return new PXException(contextMessage + Environment.NewLine + TX.Error.DUPLICATING_POSTING_DOCUMENT, ex);
            }
            else if (ex is PXOuterException)
            {
                return GetPXOuterExceptionWithContextMessage(contextMessage, (PXOuterException)ex);
            }
            else if (ex is PXException)
            {
                return new PXException(contextMessage + Environment.NewLine + ex.Message, ex);
            }
            else
            {
                return new Exception(contextMessage + Environment.NewLine + ex.Message, ex);
            }
        }

        private static PXOuterException GetPXOuterExceptionWithContextMessage(string contextMessage, PXOuterException ex)
        {
            Dictionary<string, string> innerExceptions = new Dictionary<string, string>();

            for (int i = 0; i < ex.InnerFields.Length; i++)
            {
                innerExceptions.Add(ex.InnerFields[i], ex.InnerMessages[i]);
            }

            return new PXOuterException(innerExceptions, ex.GraphType, ex.Row, contextMessage + Environment.NewLine + ex.Message);
        }
    }
}
