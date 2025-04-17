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

using System.Collections.Generic;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	/// <summary>A class that holds the responses returned by the processing center.</summary>
	public class APIResponse
	{
		/// <summary>
		/// Must be <tt>true</tt> if the request was completed without any errors and <tt>false</tt> otherwise.
		/// </summary>
		public bool isSucess = false;
		/// <summary>
		/// Contains the error messages received from the processing center.
		/// </summary>
		public Dictionary<string, string> Messages;
		/// <summary>Specifies the error source.</summary>
		public CCError.CCErrorSource ErrorSource = CCError.CCErrorSource.None;

		/// <exclude/>
		public APIResponse()
		{
			Messages = new Dictionary<string, string>();
		}
	}
}
