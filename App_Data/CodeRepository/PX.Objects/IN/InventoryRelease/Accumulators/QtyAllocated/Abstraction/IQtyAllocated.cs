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
	public interface IQtyAllocated : IQtyAllocatedBase
	{
		decimal? QtyINIssues { get; set; }
		decimal? QtyINReceipts { get; set; }
		decimal? QtyPOPrepared { get; set; }
		decimal? QtyPOOrders { get; set; }
		decimal? QtyPOReceipts { get; set; }

		decimal? QtyFSSrvOrdPrepared { get; set; }
		decimal? QtyFSSrvOrdBooked { get; set; }
		decimal? QtyFSSrvOrdAllocated { get; set; }

		decimal? QtySOBackOrdered { get; set; }
		decimal? QtySOPrepared { get; set; }
		decimal? QtySOBooked { get; set; }
		decimal? QtySOShipped { get; set; }
		decimal? QtySOShipping { get; set; }
		decimal? QtyINAssemblySupply { get; set; }
		decimal? QtyINAssemblyDemand { get; set; }
		decimal? QtyInTransitToProduction { get; set; }
		decimal? QtyProductionSupplyPrepared { get; set; }
		decimal? QtyProductionSupply { get; set; }
		decimal? QtyPOFixedProductionPrepared { get; set; }
		decimal? QtyPOFixedProductionOrders { get; set; }
		decimal? QtyProductionDemandPrepared { get; set; }
		decimal? QtyProductionDemand { get; set; }
		decimal? QtyProductionAllocated { get; set; }
		decimal? QtySOFixedProduction { get; set; }
		decimal? QtyProdFixedPurchase { get; set; }
		decimal? QtyProdFixedProduction { get; set; }
		decimal? QtyProdFixedProdOrdersPrepared { get; set; }
		decimal? QtyProdFixedProdOrders { get; set; }
		decimal? QtyProdFixedSalesOrdersPrepared { get; set; }
		decimal? QtyProdFixedSalesOrders { get; set; }
		decimal? QtyINReplaned { get; set; }

		decimal? QtyFixedFSSrvOrd { get; set; }
		decimal? QtyPOFixedFSSrvOrd { get; set; }
		decimal? QtyPOFixedFSSrvOrdPrepared { get; set; }
		decimal? QtyPOFixedFSSrvOrdReceipts { get; set; }

		decimal? QtySOFixed { get; set; }
		decimal? QtyPOFixedOrders { get; set; }
		decimal? QtyPOFixedPrepared { get; set; }
		decimal? QtyPOFixedReceipts { get; set; }
		decimal? QtySODropShip { get; set; }
		decimal? QtyPODropShipOrders { get; set; }
		decimal? QtyPODropShipPrepared { get; set; }
		decimal? QtyPODropShipReceipts { get; set; }
		decimal? QtyInTransitToSO { get; set; }
	}
}
