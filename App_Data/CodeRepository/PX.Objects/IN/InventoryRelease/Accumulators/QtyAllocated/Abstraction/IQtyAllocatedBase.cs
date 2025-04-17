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

namespace PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated.Abstraction
{
	public interface IQtyAllocatedBase
	{
		bool? NegQty { get; }
		bool? InclQtyAvail { get; }

		bool? InclQtyFSSrvOrdPrepared { get; }
		bool? InclQtyFSSrvOrdBooked { get; }
		bool? InclQtyFSSrvOrdAllocated { get; }

		bool? InclQtySOReverse { get; }
		bool? InclQtySOBackOrdered { get; }
		bool? InclQtySOPrepared { get; }
		bool? InclQtySOBooked { get; }
		bool? InclQtySOShipped { get; }
		bool? InclQtySOShipping { get; }
		bool? InclQtyPOPrepared { get; }
		bool? InclQtyPOOrders { get; }
		bool? InclQtyFixedSOPO { get; }
		bool? InclQtyPOReceipts { get; }
		bool? InclQtyInTransit { get; }
		bool? InclQtyINIssues { get; }
		bool? InclQtyINReceipts { get; }
		bool? InclQtyPOFixedReceipt { get; }
		bool? InclQtyINAssemblySupply { get; }
		bool? InclQtyINAssemblyDemand { get; }
		bool? InclQtyProductionDemandPrepared { get; }
		bool? InclQtyProductionDemand { get; }
		bool? InclQtyProductionAllocated { get; }
		bool? InclQtyProductionSupplyPrepared { get; }
		bool? InclQtyProductionSupply { get; }
		decimal? QtyOnHand { get; set; }
		decimal? QtyAvail { get; set; }
		decimal? QtyHardAvail { get; set; }
		decimal? QtyActual { get; set; }
		decimal? QtyNotAvail { get; set; }
		decimal? QtyInTransit { get; set; }
	}
}
