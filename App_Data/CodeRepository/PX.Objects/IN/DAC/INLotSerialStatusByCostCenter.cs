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
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INLotSerialStatusByCostCenter)]
	[PXProjection(typeof(Select2<INLotSerialStatusByCostCenter, InnerJoin<INItemLotSerial, On<FK.ItemLotSerial>>>), Persistent = false)]
	public class INLotSerialStatusByCostCenter : IBqlTable, IStatus, ILotSerial
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLotSerialStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID, lotSerialNbr, costCenterID>
		{
			public static INLotSerialStatusByCostCenter Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? locationID, string lotSerialNbr, int? costCenterID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, subItemID, siteID, locationID, lotSerialNbr, costCenterID, options);
		}
		public static class FK
		{
			public class Location : INLocation.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<locationID> { }
			public class LocationStatus : INLocationStatus.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<inventoryID, subItemID, siteID, locationID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<subItemID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<inventoryID> { }
			public class ItemLotSerial : INItemLotSerial.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<inventoryID, lotSerialNbr> { }
			public class Site : INSite.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<siteID> { }
			public class CostCenter : INCostCenter.PK.ForeignKeyOf<INLotSerialStatusByCostCenter>.By<costCenterID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[StockItem(IsKey = true)]
		[PXDefault]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true)]
		[PXDefault]
		public virtual int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		[Site(IsKey = true)]
		[PXDefault]
		public virtual int? SiteID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : Data.BQL.BqlInt.Field<locationID> { }
		protected int? _LocationID;
		[Location(typeof(siteID), IsKey = true)]
		[PXDefault]
		public virtual int? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : Data.BQL.BqlString.Field<lotSerialNbr>
		{
			public const int Length = 100;
		}
		[PXDefault]
		[LotSerialNbr(IsKey = true)]
		public virtual string LotSerialNbr
		{
			get;
			set;
		}
		#endregion
		#region CostCenterID
		public abstract class costCenterID : Data.BQL.BqlInt.Field<costCenterID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion

		#region ExpireDate
		public abstract class expireDate : Data.BQL.BqlDateTime.Field<expireDate> { }
		[PXDBDate(BqlField = typeof(INItemLotSerial.expireDate))]
		[PXUIField(DisplayName = "Expiry Date")]
		public virtual DateTime? ExpireDate
		{
			get;
			set;
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : Data.BQL.BqlDateTime.Field<receiptDate> { }
		[PXDBDate]
		[PXDefault]
		public virtual DateTime? ReceiptDate
		{
			get;
			set;
		}
		#endregion
		#region LotSerTrack
		public abstract class lotSerTrack : Data.BQL.BqlString.Field<lotSerTrack> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		public virtual string LotSerTrack
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdBooked
		public abstract class qtyFSSrvOrdBooked : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdBooked
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdAllocated
		public abstract class qtyFSSrvOrdAllocated : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdAllocated
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdPrepared
		public abstract class qtyFSSrvOrdPrepared : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual decimal? QtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : Data.BQL.BqlDecimal.Field<qtyAvail> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Hard Available")]
		public virtual decimal? QtyHardAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : Data.BQL.BqlDecimal.Field<qtyActual> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available for Issue")]
		public virtual decimal? QtyActual
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyInTransit
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransitToSO
		public abstract class qtyInTransitToSO : Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOBackOrdered
		{
			get;
			set;
		}
		#endregion
		#region QtySOPrepared
		public abstract class qtySOPrepared : Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOBooked
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOShipped
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOShipping
		{
			get;
			set;
		}
		#endregion
		#region QtyINIssues
		public abstract class qtyINIssues : Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Issues")]
		public virtual decimal? QtyINIssues
		{
			get;
			set;
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Inventory Receipts")]
		public virtual decimal? QtyINReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Demanded by Kit Assembly")]
		public virtual decimal? QtyINAssemblyDemand
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Kit Assembly")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty In Transit to Production")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Production Supply Prepared")]
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
		/// Specifies the quantity On Production Supply.
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production Supply")]
		public virtual decimal? QtyProductionSupply
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedProductionPrepared
		public abstract class qtyPOFixedProductionPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
		protected decimal? _QtyPOFixedProductionPrepared;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Purchase for Prod. Prepared. 
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Purchase for Prod. Prepared")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Purchase for Production")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production Demand Prepared")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production Demand")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production Allocated")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On SO to Production")]
		public virtual decimal? QtySOFixedProduction
		{
			get;
			set;
		}
		#endregion

		#region QtyFixedFSSrvOrd
		public abstract class qtyFixedFSSrvOrd : Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyFixedFSSrvOrd
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrd
		public abstract class qtyPOFixedFSSrvOrd : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedFSSrvOrd
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrdPrepared
		public abstract class qtyPOFixedFSSrvOrdPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedFSSrvOrdReceipts
		public abstract class qtyPOFixedFSSrvOrdReceipts : Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production to Purchase", Enabled = false)]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production to Production", Enabled = false)]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production for Prod. Prepared", Enabled = false)]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production for Production", Enabled = false)]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
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
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty On Production for SO", Enabled = false)]
		public virtual decimal? QtyProdFixedSalesOrders
		{
			get;
			set;
		}
		#endregion
		#region QtySOFixed
		public abstract class qtySOFixed : Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySOFixed
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPOFixedReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtySODropShip
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? QtyPODropShipReceipts
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.BothFromGraphAndRecord)]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R2)]
	[PXHidden]
	public partial class INLotSerialStatusByCostCenterAlias : INLotSerialStatusByCostCenter
	{
		#region InventoryID
		public new abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		public new abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region SiteID
		public new abstract class siteID : Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		public new abstract class locationID : Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region QtyAvail
		public new abstract class qtyAvail : Data.BQL.BqlDecimal.Field<qtyAvail> { }
		#endregion

		#region QtyAvailNotNull
		[PXQuantity]
		[PXFormula(typeof(IsNull<qtyAvail, decimal0>))]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual decimal? QtyAvailNotNull { get; set; }
		public abstract class qtyAvailNotNull : PX.Data.BQL.BqlDecimal.Field<qtyAvailNotNull> { }
		#endregion
	}

	//Copy of original but with location restriction removed. Made for transitline support
	[PXHidden]
	public class INLotSerialStatusInTransit : INLotSerialStatusByCostCenter
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		#endregion
		#region SubItemID
		public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		public override int? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region QtyAvail
		public new abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		#endregion
		#region QtyNotAvail
		public new abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		#endregion
		#region QtyExpired
		public new abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		#endregion
		#region QtyHardAvail
		public new abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		#endregion
		#region QtyInTransit
		public new abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		#endregion
		#region QtyInTransitToSO
		public new abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
		#endregion
		#region QtyPOPrepared
		public new abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		#endregion
		#region QtyPOOrders
		public new abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		#endregion
		#region QtyPOReceipts
		public new abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		#endregion
		#region QtySOBackOrdered
		public new abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		#endregion
		#region QtySOPrepared
		public new abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		#endregion
		#region QtySOBooked
		public new abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		#endregion
		#region QtySOShipped
		public new abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		#endregion
		#region QtySOShipping
		public new abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		#endregion
		#region QtyINIssues
		public new abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		#endregion
		#region QtyINReceipts
		public new abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		#endregion
		#region QtyINAssemblyDemand
		public new abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		#endregion
		#region QtyINAssemblySupply
		public new abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		#endregion
		#region QtyInTransitToProduction
		public new abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
		#endregion
		#region QtyProductionSupplyPrepared
		public new abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
		#endregion
		#region QtyProductionSupply
		public new abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
		#endregion
		#region QtyPOFixedProductionPrepared
		public new abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
		#endregion
		#region QtyPOFixedProductionOrders
		public new abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
		#endregion
		#region QtyProductionDemandPrepared
		public new abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
		#endregion
		#region QtyProductionDemand
		public new abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
		#endregion
		#region QtyProductionAllocated
		public new abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
		#endregion
		#region QtySOFixedProduction
		public new abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
		#endregion
		#region QtyProdFixedPurchase
		public new abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
		#endregion
		#region QtyProdFixedProduction
		public new abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
		#endregion
		#region QtyProdFixedProdOrdersPrepared
		public new abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
		#endregion
		#region QtyProdFixedProdOrders
		public new abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
		#endregion
		#region QtyProdFixedSalesOrdersPrepared
		public new abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
		#endregion
		#region QtyProdFixedSalesOrders
		public new abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
		#endregion
		#region QtySOFixed
		public new abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		#endregion
		#region QtyPOFixedOrders
		public new abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		#endregion
		#region QtyPOFixedPrepared
		public new abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		#endregion
		#region QtyPOFixedReceipts
		public new abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		#endregion
		#region QtySODropShip
		public new abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		#endregion
		#region QtyPODropShipOrders
		public new abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		#endregion
		#region QtyPODropShipPrepared
		public new abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		#endregion
		#region QtyPODropShipReceipts
		public new abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		#endregion
		#region ExpireDate
		public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region ReceiptDate
		public new abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		#endregion
		#region LotSerTrack
		public new abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }
		#endregion
		#region tstamp
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}
