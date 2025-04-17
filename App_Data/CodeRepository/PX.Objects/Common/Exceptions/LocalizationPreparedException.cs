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
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.Exceptions
{
	/// <summary>
	/// Exception that saves message and arguments to be localized in wrapping by <see cref="PXException"/> or it's children.
	/// It helps to skips double localization if you need to throw exception with args,
	/// and then catch it and throw new <see cref="PXException"/> with same message.
	/// </summary>
	/// <example>
	/// try
	/// {
	///   throw new LocalizationPreparedException(Messages.Message, arg1, arg2);
	/// }
	/// catch (LocalizationPreparedException e)
	/// {
	///   throw new PXException(e.Format, e.Args);
	/// }
	///
	/// </example>
	public class LocalizationPreparedException : Exception
	{
		public LocalizationPreparedException(string format, params object[] args)
			: base()
		{
			Format = format;
			Args = args;
		}

		public LocalizationPreparedException(Exception innerException, string format, params object[] args)
			: base(null, innerException)
		{
			Format = format;
			Args = args;
		}

		public string Format { get; }
		public object[] Args { get; }
		public override string Message => string.Format(Format, Args);
	}
}
