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
using PX.Data;

namespace PX.Objects.IN.DAC.Unbound
{
	[PXHidden]
	public class LotSerialStatusAggregate : LocationStatusAggregate.ITable
	{
		#region InventoryID
		public new abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public new abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public new abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? LocationID
		{
			get;
			set;
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : Data.BQL.BqlString.Field<lotSerialNbr> { }
		[PXString(100, IsUnicode = true, IsKey = true)]
		public virtual String LotSerialNbr
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public abstract class costCenterID : PX.Data.BQL.BqlInt.Field<costCenterID> { }
		[PXInt(IsKey = true)]
		public virtual Int32? CostCenterID
		{
			get;
			set;
		}
		#endregion

		#region CostLayerType
		public abstract class costLayerType : Data.BQL.BqlString.Field<costLayerType> { }
		[PXString(1)]
		public virtual string CostLayerType
		{
			get;
			set;
		}
		#endregion
		#region SiteCD
		public abstract class siteCD : Data.BQL.BqlString.Field<siteCD> { }
		[PXString(IsUnicode = true)]
		public virtual String SiteCD
		{
			get;
			set;
		}
		#endregion
		#region SubItemCD
		public abstract class subItemCD : Data.BQL.BqlString.Field<subItemCD> { }
		[PXString(IsUnicode = true)]
		public virtual String SubItemCD
		{
			get;
			set;
		}
		#endregion
		#region LocationCD
		public abstract class locationCD : Data.BQL.BqlString.Field<locationCD> { }
		[PXDBString(IsUnicode = true)]
		public virtual String LocationCD
		{
			get;
			set;
		}
		#endregion

		#region ExpireDate
		public new abstract class expireDate : Data.BQL.BqlDateTime.Field<expireDate> { }
		[PXDate]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
		#region BaseUnit
		public abstract class baseUnit : Data.BQL.BqlString.Field<baseUnit> { }
		[PXString(6, IsUnicode = true)]
		public virtual String BaseUnit
		{
			get;
			set;
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : Data.BQL.BqlDecimal.Field<qtyAvail> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyNotAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyExpired
		public abstract class qtyExpired : Data.BQL.BqlDecimal.Field<qtyExpired> { }
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyExpired
		{
			get;
			set;
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyHardAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : Data.BQL.BqlDecimal.Field<qtyActual> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyActual
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyInTransit
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransitToSO
		public abstract class qtyInTransitToSO : Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyInTransitToSO
		{
			get;
			set;
		}
		#endregion
		#region QtyINReplaned
		public decimal? QtyINReplaned
		{
			get { return 0m; }
			set { }
		}
		#endregion
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOReceipts
		{
			get;
			set;
		}
		#endregion

		#region QtyFSSrvOrdBooked
		public abstract class qtyFSSrvOrdBooked : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyFSSrvOrdBooked
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdAllocated
		public abstract class qtyFSSrvOrdAllocated : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyFSSrvOrdAllocated
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdPrepared
		public abstract class qtyFSSrvOrdPrepared : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion

		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOBackOrdered
		{
			get;
			set;
		}
		#endregion
		#region QtySOPrepared
		public abstract class qtySOPrepared : Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOBooked
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOShipped
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOShipping
		{
			get;
			set;
		}
		#endregion
		#region QtyINIssues
		public abstract class qtyINIssues : Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyINIssues
		{
			get;
			set;
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyINReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyINAssemblyDemand
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyINAssemblySupply
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransitToProduction
		public abstract class qtyInTransitToProduction : Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity In Transit to Production.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyInTransitToProduction
		{
			get;
			set;
		}
		#endregion
		#region QtyProductionSupplyPrepared
		public abstract class qtyProductionSupplyPrepared : Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity Production Supply Prepared.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProductionSupplyPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyProductionSupply
		public abstract class qtyProductionSupply : Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity Production Supply.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProductionSupply
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedProductionPrepared
		public abstract class qtyPOFixedProductionPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Purchase for Prod. Prepared.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedProductionPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedProductionOrders
		public abstract class qtyPOFixedProductionOrders : Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Purchase for Production.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedProductionOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyProductionDemandPrepared
		public abstract class qtyProductionDemandPrepared : Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production Demand Prepared.
		/// </summary>
		[PXDecimal()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProductionDemandPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyProductionDemand
		public abstract class qtyProductionDemand : Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production Demand.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProductionDemand
		{
			get;
			set;
		}
		#endregion
		#region QtyProductionAllocated
		public abstract class qtyProductionAllocated : Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production Allocated.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProductionAllocated
		{
			get;
			set;
		}
		#endregion
		#region QtySOFixedProduction
		public abstract class qtySOFixedProduction : Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On SO to Production.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOFixedProduction
		{
			get;
			set;
		}
		#endregion

		#region QtyFixedFSSrvOrd
		public abstract class qtyFixedFSSrvOrd : Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyFixedFSSrvOrd
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrd
		public abstract class qtyPOFixedFSSrvOrd : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedFSSrvOrd
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrdPrepared
		public abstract class qtyPOFixedFSSrvOrdPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrdReceipts
		public abstract class qtyPOFixedFSSrvOrdReceipts : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedFSSrvOrdReceipts
		{
			get;
			set;
		}
		#endregion

		#region QtyProdFixedPurchase
		// M9
		public abstract class qtyProdFixedPurchase : Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production to Purchase.
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedPurchase
		{
			get;
			set;
		}
		#endregion
		#region QtyProdFixedProduction
		// MA
		public abstract class qtyProdFixedProduction : Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production to Production
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedProduction
		{
			get;
			set;
		}
		#endregion
		#region QtyProdFixedProdOrdersPrepared
		// MB
		public abstract class qtyProdFixedProdOrdersPrepared : Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for Prod. Prepared
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedProdOrdersPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyProdFixedProdOrders
		// MC
		public abstract class qtyProdFixedProdOrders : Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for Production
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedProdOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyProdFixedSalesOrdersPrepared
		// MD
		public abstract class qtyProdFixedSalesOrdersPrepared : Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for SO Prepared
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedSalesOrdersPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyProdFixedSalesOrders
		// ME
		public abstract class qtyProdFixedSalesOrders : Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for SO
		/// </summary>
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyProdFixedSalesOrders
		{
			get;
			set;
		}
		#endregion
		#region QtySOFixed
		public abstract class qtySOFixed : Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySOFixed
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPOFixedReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtySODropShip
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPODropShipOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPODropShipPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? QtyPODropShipReceipts
		{
			get;
			set;
		}
		#endregion

		#region TotalCost
		public abstract class totalCost : Data.BQL.BqlDecimal.Field<totalCost> { }
		[PXDecimal]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? TotalCost
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		[PXPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? UnitCost
		{
			get;
			set;
		}
		#endregion
	}
}
