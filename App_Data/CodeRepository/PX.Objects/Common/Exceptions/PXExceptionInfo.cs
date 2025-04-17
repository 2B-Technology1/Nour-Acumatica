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

namespace PX.Objects.Common.Exceptions
{
	public class PXExceptionInfo
	{
		public string MessageFormat { get; }

		public object[] MessageArguments { get; set; }

		public PXErrorLevel? ErrorLevel { get; set; }

		public string Css { get; set; }

		public PXExceptionInfo(string messageFormat, params object[] messageArgs)
		{
			MessageFormat = messageFormat;
			MessageArguments = messageArgs ?? Array.Empty<object>();
		}

		public PXExceptionInfo(PXErrorLevel errorLevel, string messageFormat, params object[] messageArgs)
			: this(messageFormat, messageArgs)
		{
			ErrorLevel = errorLevel;
		}

		public PXSetPropertyException ToSetPropertyException()
        {
			var errorLevel = ErrorLevel ?? PXErrorLevel.Warning;
			if (string.IsNullOrEmpty(Css))
				return new PXSetPropertyException(MessageFormat, errorLevel, MessageArguments);
			return new PXSetPropertyException($"|css={Css}|{PXMessages.LocalizeFormatNoPrefix(MessageFormat, MessageArguments)}", errorLevel);
		}
	}
}
