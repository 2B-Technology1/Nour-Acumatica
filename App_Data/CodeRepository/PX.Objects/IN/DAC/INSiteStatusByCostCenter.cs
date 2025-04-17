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
using PX.Objects.IN.Attributes;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INSiteStatusByCostCenter)]
	public partial class INSiteStatusByCostCenter : IBqlTable, IStatus
	{
		#region Keys
		public class PK : PrimaryKeyOf<INSiteStatusByCostCenter>.By<inventoryID, subItemID, siteID, costCenterID>
		{
			public static INSiteStatusByCostCenter Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? costCenterID, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, inventoryID, subItemID, siteID, costCenterID, options);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INSiteStatusByCostCenter>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INSiteStatusByCostCenter>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INSiteStatusByCostCenter>.By<siteID> { }
			public class ItemSite : INItemSite.PK.ForeignKeyOf<INSiteStatusByCostCenter>.By<inventoryID, siteID> { }
			public class CostCenter : INCostCenter.PK.ForeignKeyOf<INSiteStatusByCostCenter>.By<costCenterID> { }
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<INSiteStatusByCostCenter,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<Where<qtyOnHand, NotEqual<decimal0>, Or<qtyAvail, NotEqual<decimal0>>>>>>>
			{
			}
		}
		[Inventory(IsKey = true)]
		[PXDefault]
		[ConvertedInventoryItem(true)]
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

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		[PXExistance()]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active
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
		#region QtyNotAvail
		public abstract class qtyNotAvail : Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Not Available")]
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
		[PXUIField(DisplayName = "Qty. In-Transit")]
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
		[PXUIField(DisplayName = "Qty. In Transit to SO")]
		public virtual decimal? QtyInTransitToSO
		{
			get;
			set;
		}
		#endregion
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
		public virtual decimal? QtyPOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Purchase Orders")]
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
		[PXUIField(DisplayName = "Qty. Purchase Receipts")]
		public virtual decimal? QtyPOReceipts
		{
			get;
			set;
		}
		#endregion

		#region QtyFSSrvOrdBooked
		public abstract class qtyFSSrvOrdBooked : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. FS Booked", FieldClass = "SERVICEMANAGEMENT")]
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
		[PXUIField(DisplayName = "Qty. FS Allocated", FieldClass = "SERVICEMANAGEMENT")]
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
		[PXUIField(DisplayName = "Qty. FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion

		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Backordered")]
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
		[PXUIField(DisplayName = "Qty. SO Prepared")]
		public virtual decimal? QtySOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. SO Booked")]
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
		[PXUIField(DisplayName = "Qty. SO Shipped")]
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
		[PXUIField(DisplayName = "Qty. SO Shipping")]
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
		/// Specifies the quantity Production Supply.
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
		[PXUIField(DisplayName = "Qty On Production for SO Prepared", Enabled = false)]
		public virtual decimal? QtyProdFixedSalesOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyINReplaned
		public abstract class qtyINReplaned : Data.BQL.BqlDecimal.Field<qtyINReplaned> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Replaned")]
		public virtual decimal? QtyINReplaned
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
}
