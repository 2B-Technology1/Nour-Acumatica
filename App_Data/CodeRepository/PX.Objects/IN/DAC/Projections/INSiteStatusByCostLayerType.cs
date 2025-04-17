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

using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN.Attributes;
using System;

namespace PX.Objects.IN.DAC.Projections
{
	/// <exclude />
	[PXCacheName(Messages.INSiteStatusByCostLayerType)]
	[INSiteStatusByCostLayerTypeProjection]
	public class INSiteStatusByCostLayerType : IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[Inventory(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : Data.BQL.BqlInt.Field<subItemID> { }
		[SubItem(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.subItemID))]
		public virtual int? SubItemID
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[Site(IsKey = true, BqlField = typeof(INSiteStatusByCostCenter.siteID))]
		public virtual Int32? SiteID
		{
			get;
			set;
		}
		#endregion
		#region CostLayerType
		public abstract class costLayerType : PX.Data.BQL.BqlString.Field<costLayerType> { }
		[PXDBCalced(typeof(IsNull<INCostCenter.costLayerType, CostLayerType.normal>), typeof(string))]
		[PXString(1, IsKey = true)]
		[CostLayerType.List]
		[PXUIField(DisplayName = "Cost Layer Type", FieldClass = FeaturesSet.inventory.CostLayerType)]
		public virtual string CostLayerType
		{
			get;
			set;
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyOnHand))]
		public virtual Decimal? QtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyNotAvail))]
		[PXUIField(DisplayName = "Qty. Not Available")]
		public virtual decimal? QtyNotAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyAvail
		public abstract class qtyAvail : Data.BQL.BqlDecimal.Field<qtyAvail> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyAvail))]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual decimal? QtyAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyHardAvail))]
		[PXUIField(DisplayName = "Qty. Hard Available")]
		public virtual decimal? QtyHardAvail
		{
			get;
			set;
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : Data.BQL.BqlDecimal.Field<qtyActual> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyActual))]
		[PXUIField(DisplayName = "Qty. Available for Issue")]
		public virtual decimal? QtyActual
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyInTransit))]
		[PXUIField(DisplayName = "Qty. In-Transit")]
		public virtual decimal? QtyInTransit
		{
			get;
			set;
		}
		#endregion
		#region QtyInTransitToSO
		public abstract class qtyInTransitToSO : Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyInTransitToSO))]
		[PXUIField(DisplayName = "Qty. In Transit to SO")]
		public virtual decimal? QtyInTransitToSO
		{
			get;
			set;
		}
		#endregion
		#region QtyPOPrepared
		public abstract class qtyPOPrepared : Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOPrepared))]
		[PXUIField(DisplayName = "Qty. PO Prepared")]
		public virtual decimal? QtyPOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOOrders))]
		[PXUIField(DisplayName = "Qty. Purchase Orders")]
		public virtual decimal? QtyPOOrders
		{
			get;
			set;
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyPOReceipts))]
		[PXUIField(DisplayName = "Qty. Purchase Receipts")]
		public virtual decimal? QtyPOReceipts
		{
			get;
			set;
		}
		#endregion

		#region QtyFSSrvOrdBooked
		public abstract class qtyFSSrvOrdBooked : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdBooked))]
		[PXUIField(DisplayName = "Qty. FS Booked", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdBooked
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdAllocated
		public abstract class qtyFSSrvOrdAllocated : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdAllocated))]
		[PXUIField(DisplayName = "Qty. FS Allocated", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdAllocated
		{
			get;
			set;
		}
		#endregion
		#region QtyFSSrvOrdPrepared
		public abstract class qtyFSSrvOrdPrepared : Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyFSSrvOrdPrepared))]
		[PXUIField(DisplayName = "Qty. FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFSSrvOrdPrepared
		{
			get;
			set;
		}
		#endregion

		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOBackOrdered))]
		[PXUIField(DisplayName = "Qty. SO Backordered")]
		public virtual decimal? QtySOBackOrdered
		{
			get;
			set;
		}
		#endregion
		#region QtySOPrepared
		public abstract class qtySOPrepared : Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOPrepared))]
		[PXUIField(DisplayName = "Qty. SO Prepared")]
		public virtual decimal? QtySOPrepared
		{
			get;
			set;
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOBooked))]
		[PXUIField(DisplayName = "Qty. SO Booked")]
		public virtual decimal? QtySOBooked
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOShipped))]
		[PXUIField(DisplayName = "Qty. SO Shipped")]
		public virtual decimal? QtySOShipped
		{
			get;
			set;
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtySOShipping))]
		[PXUIField(DisplayName = "Qty. SO Shipping")]
		public virtual decimal? QtySOShipping
		{
			get;
			set;
		}
		#endregion
		#region QtyINIssues
		public abstract class qtyINIssues : Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINIssues))]
		[PXUIField(DisplayName = "Qty On Inventory Issues")]
		public virtual decimal? QtyINIssues
		{
			get;
			set;
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINReceipts))]
		[PXUIField(DisplayName = "Qty On Inventory Receipts")]
		public virtual decimal? QtyINReceipts
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINAssemblyDemand))]
		[PXUIField(DisplayName = "Qty Demanded by Kit Assembly")]
		public virtual decimal? QtyINAssemblyDemand
		{
			get;
			set;
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		[PXDBQuantity(BqlField = typeof(INSiteStatusByCostCenter.qtyINAssemblySupply))]
		[PXUIField(DisplayName = "Qty On Kit Assembly")]
		public virtual decimal? QtyINAssemblySupply
		{
			get;
			set;
		}
		#endregion
	}
}
