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

namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCPaymentTransaction
	{
		int? TransactionID { get; set; }
		int? TranNbr { get; set; }
		int? PMInstanceID { get; set; }
		string ProcessingCenterID { get; set; }
		string DocType { get; set; }
		string RefNbr { get; set; }
		int? RefTranNbr { get; set; }
		string OrigDocType { get; set; }
		string OrigRefNbr { get; set; }
		string PCTranNumber { get; set; }
		string AuthNumber { get; set; }
		string TranType { get; set; }
		string ProcStatus { get; set; }
		string TranStatus { get; set; }
		DateTime? ExpirationDate { get; set; }
		decimal? Amount { get; set; }
	}
}
