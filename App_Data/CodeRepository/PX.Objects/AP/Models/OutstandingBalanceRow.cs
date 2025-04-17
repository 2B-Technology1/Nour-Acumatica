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

using PX.Objects.Common;
using System;


namespace PX.Objects.AP
{
	class OutstandingBalanceRow : IAdjustmentStub
	{
		public string StubNbr { get; set; }
		public int? CashAccountID { get; set; }
		public string PaymentMethodID { get; set; }
		public bool Persistent => false;
		public decimal? CuryOutstandingBalance { get; set; }
		public DateTime? OutstandingBalanceDate { get; set; }
		public decimal? CuryAdjgAmt => 0;
		public decimal? CuryAdjgDiscAmt => 0m;
		public string AdjdDocType => null;
		public bool? IsRequest => null;
	}
}
