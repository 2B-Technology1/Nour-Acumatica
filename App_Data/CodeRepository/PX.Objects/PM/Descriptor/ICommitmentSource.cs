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

using CommonServiceLocator;
using PX.Data;
using PX.Objects.CA.Descriptor;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
    interface ICommitmentSource 
	{ 
		Guid? CommitmentID { get; }
		int? ExpenseAcctID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		int? InventoryID { get; }
		int? CostCodeID { get; }
		int? BranchID { get; }
		string UOM { get; }
		long? CuryInfoID { get; }
		decimal? OrigExtCost { get; }
		decimal? OrigOrderQty { get; }
		decimal? CuryExtCost { get; }
		decimal? ExtCost { get; }
		decimal? OrderQty { get; }
		decimal? CuryRetainageAmt { get; }
		decimal? CompletedQty { get; }
		decimal? ReceivedQty { get; }
		decimal? BilledQty { get; }
		decimal? CuryBilledAmt { get; }
		decimal? BilledAmt { get; }
		decimal? RetainageAmt { get; }
		bool? Completed { get; }
		bool? Closed { get; }
		bool? Cancelled { get; }
		string CompletePOLine { get; }
	}

}
