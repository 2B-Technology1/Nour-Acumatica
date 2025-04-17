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
using PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.CR;

namespace PX.Objects.AR.CCPaymentProcessing.Common
{
	public delegate DateTime? String2DateConverterFunc(string s);
	public class CCProcessingContext
	{
		public CCProcessingCenter processingCenter = null;
		public int? aPMInstanceID = null;
		public string PaymentMethodID = null;
		public int? aCustomerID = 0;
		public string aCustomerCD = null;
		public string PrefixForCustomerCD = null;
		public string aDocType = null;
		public string aRefNbr = null;
		public PXGraph callerGraph = null;
		public String2DateConverterFunc expirationDateConverter = null;
	}
}
