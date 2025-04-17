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
using System;
using System.Runtime.Serialization;

namespace PX.Objects.Localizations.GB.HMRC.Exceptions
{
	/// <summary>
	/// Exception returned by the VAT API when sending or viewing the return or retrieve obligations. 
	/// </summary>
	public class VATAPIException : Exception
	{
		public string Code { get; set; }

		public VATAPIException() { }

		public VATAPIException(string code, string message = null) : base(message)
		{
			Code = code;
		}
		protected VATAPIException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}
}
