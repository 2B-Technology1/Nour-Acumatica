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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LocationStatusAggregate = PX.Objects.IN.DAC.Unbound.LocationStatusAggregate;
using LotSerialStatusAggregate = PX.Objects.IN.DAC.Unbound.LotSerialStatusAggregate;
using SiteStatusAggregate = PX.Objects.IN.DAC.Unbound.SiteStatusAggregate;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.IN.InventoryRelease.Accumulators.QtyAllocated;
using PX.Objects.IN.DAC.Projections;
using PX.Objects.IN.GraphExtensions;

namespace PX.Objects.IN
{
	#region Filter

	public partial class InventorySummaryEnqFilter : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			Required = true)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[SubItemRawExt(typeof(InventorySummaryEnqFilter.inventoryID), DisplayName = "Subitem")]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region SubItemCD Wildcard
		public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubItemCDWildcard
		{
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(DescriptionField = typeof(INSite.descr), DisplayName = "Warehouse")]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(InventorySummaryEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
		public virtual Int32? LocationID
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

		#region ExpandByLotSerialNbr
		public abstract class expandByLotSerialNbr : PX.Data.BQL.BqlBool.Field<expandByLotSerialNbr> { }
		protected bool? _ExpandByLotSerialNbr;
		[PXDBBool()]
		[PXDefault()]
		[PXUIField(DisplayName = "Expand by Lot/Serial Number", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ExpandByLotSerialNbr
		{
			get
			{
				return this._ExpandByLotSerialNbr;
			}
			set
			{
				this._ExpandByLotSerialNbr = value;
			}
		}
		#endregion

		#region ExpandByCostLayerType
		/// <exclude />
		public abstract class expandByCostLayerType : Data.BQL.BqlBool.Field<expandByCostLayerType> { }
		/// <exclude />
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Expand by Cost Layer Type", Visibility = PXUIVisibility.Visible)]
		public virtual bool? ExpandByCostLayerType
		{
			get;
			set;
		}
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : IBqlField { }
		[OrganizationTree(FieldClass = nameof(FeaturesSet.MultipleBaseCurrencies), Required = true)]
		public int? OrgBAccountID
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

	#region ResultSet

	public partial class InventorySummaryEnquiryResult : PX.Data.IBqlTable, IStatus, ICostStatus
	{
		public const int TotalLocationID = -1;
		public const int EmptyLocationID = -3;

		public InventorySummaryEnquiryResult() { }

		#region GridLineNbr
		public abstract class gridLineNbr : PX.Data.BQL.BqlInt.Field<gridLineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual Int32? GridLineNbr { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID", Visible = false)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Warehouse")]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[Location(typeof(siteID), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
		public virtual Int32? LocationID
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
		#region CostLayerType
		/// <exclude />
		public abstract class costLayerType : Data.BQL.BqlString.Field<costLayerType> { }
		/// <exclude />
		[PXDBString(1)]
		[CostLayerType.List]
		[PXUIField(DisplayName = "Cost Layer Type", FieldClass = FeaturesSet.inventory.CostLayerType)]
		public virtual string CostLayerType
		{
			get;
			set;
		}
		#endregion

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
		#region QtyHardAvail
		public abstract class qtyHardAvail : PX.Data.BQL.BqlDecimal.Field<qtyHardAvail> { }
		protected Decimal? _QtyHardAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available for Shipment", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyHardAvail
		{
			get
			{
				return this._QtyHardAvail;
			}
			set
			{
				this._QtyHardAvail = value;
			}
		}
		#endregion
		#region QtyActual
		public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }
		protected decimal? _QtyActual;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Available for Issue", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual decimal? QtyActual
		{
			get { return _QtyActual; }
			set { _QtyActual = value; }
		}
		#endregion
		#region QtyNotAvail
		public abstract class qtyNotAvail : PX.Data.BQL.BqlDecimal.Field<qtyNotAvail> { }
		protected Decimal? _QtyNotAvail;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Not Available", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyNotAvail
		{
			get
			{
				return this._QtyNotAvail;
			}
			set
			{
				this._QtyNotAvail = value;
			}
		}
		#endregion

		#region QtyFSSrvOrdPrepared
		public abstract class qtyFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdPrepared> { }
		protected Decimal? _QtyFSSrvOrdPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "FS Prepared", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
		public virtual Decimal? QtyFSSrvOrdPrepared
		{
			get
			{
				return this._QtyFSSrvOrdPrepared;
			}
			set
			{
				this._QtyFSSrvOrdPrepared = value;
			}
		}
		#endregion
		#region QtyFSSrvOrdBooked
		public abstract class qtyFSSrvOrdBooked : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdBooked> { }
		protected Decimal? _QtyFSSrvOrdBooked;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "FS Booked", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
		public virtual Decimal? QtyFSSrvOrdBooked
		{
			get
			{
				return this._QtyFSSrvOrdBooked;
			}
			set
			{
				this._QtyFSSrvOrdBooked = value;
			}
		}
		#endregion
		#region QtyFSSrvOrdAllocated
		public abstract class qtyFSSrvOrdAllocated : PX.Data.BQL.BqlDecimal.Field<qtyFSSrvOrdAllocated> { }
		protected Decimal? _QtyFSSrvOrdAllocated;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "FS Allocated", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "SERVICEMANAGEMENT")]
		public virtual Decimal? QtyFSSrvOrdAllocated
		{
			get
			{
				return this._QtyFSSrvOrdAllocated;
			}
			set
			{
				this._QtyFSSrvOrdAllocated = value;
			}
		}
		#endregion

		#region QtySOPrepared
		public abstract class qtySOPrepared : PX.Data.BQL.BqlDecimal.Field<qtySOPrepared> { }
		protected Decimal? _QtySOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Prepared", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtySOPrepared
		{
			get
			{
				return this._QtySOPrepared;
			}
			set
			{
				this._QtySOPrepared = value;
			}
		}
		#endregion
		#region QtySOBooked
		public abstract class qtySOBooked : PX.Data.BQL.BqlDecimal.Field<qtySOBooked> { }
		protected Decimal? _QtySOBooked;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Booked", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtySOBooked
		{
			get
			{
				return this._QtySOBooked;
			}
			set
			{
				this._QtySOBooked = value;
			}
		}
		#endregion
		#region QtySOShipped
		public abstract class qtySOShipped : PX.Data.BQL.BqlDecimal.Field<qtySOShipped> { }
		protected Decimal? _QtySOShipped;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Shipped", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtySOShipped
		{
			get
			{
				return this._QtySOShipped;
			}
			set
			{
				this._QtySOShipped = value;
			}
		}
		#endregion
		#region QtySOShipping
		public abstract class qtySOShipping : PX.Data.BQL.BqlDecimal.Field<qtySOShipping> { }
		protected Decimal? _QtySOShipping;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Allocated", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtySOShipping
		{
			get
			{
				return this._QtySOShipping;
			}
			set
			{
				this._QtySOShipping = value;
			}
		}
		#endregion
		#region QtySOBackOrdered
		public abstract class qtySOBackOrdered : PX.Data.BQL.BqlDecimal.Field<qtySOBackOrdered> { }
		protected Decimal? _QtySOBackOrdered;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO Back Ordered", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? QtySOBackOrdered
		{
			get
			{
				return this._QtySOBackOrdered;
			}
			set
			{
				this._QtySOBackOrdered = value;
			}
		}
		#endregion
		#region QtyINAssemblyDemand
		public abstract class qtyINAssemblyDemand : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblyDemand> { }
		protected Decimal? _QtyINAssemblyDemand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In Assembly Demand", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
		public virtual Decimal? QtyINAssemblyDemand
		{
			get
			{
				return this._QtyINAssemblyDemand;
			}
			set
			{
				this._QtyINAssemblyDemand = value;
			}
		}
		#endregion

		#region QtyINIssues
		public abstract class qtyINIssues : PX.Data.BQL.BqlDecimal.Field<qtyINIssues> { }
		protected Decimal? _QtyINIssues;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Issues", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyINIssues
		{
			get
			{
				return this._QtyINIssues;
			}
			set
			{
				this._QtyINIssues = value;
			}
		}
		#endregion
		#region QtyINReceipts
		public abstract class qtyINReceipts : PX.Data.BQL.BqlDecimal.Field<qtyINReceipts> { }
		protected Decimal? _QtyINReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "IN Receipts", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyINReceipts
		{
			get
			{
				return this._QtyINReceipts;
			}
			set
			{
				this._QtyINReceipts = value;
			}
		}
		#endregion
		#region QtyInTransit
		public abstract class qtyInTransit : PX.Data.BQL.BqlDecimal.Field<qtyInTransit> { }
		protected Decimal? _QtyInTransit;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In-Transit", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyInTransit
		{
			get
			{
				return this._QtyInTransit;
			}
			set
			{
				this._QtyInTransit = value;
			}
		}
		#endregion
		#region QtyInTransitToSO
		public abstract class qtyInTransitToSO : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToSO> { }
		protected Decimal? _QtyInTransitToSO;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In-Transit to SO", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyInTransitToSO
		{
			get
			{
				return this._QtyInTransitToSO;
			}
			set
			{
				this._QtyInTransitToSO = value;
			}
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
		public abstract class qtyPOPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOPrepared> { }
		protected Decimal? _QtyPOPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase Prepared", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyPOPrepared
		{
			get
			{
				return this._QtyPOPrepared;
			}
			set
			{
				this._QtyPOPrepared = value;
			}
		}
		#endregion
		#region QtyPOOrders
		public abstract class qtyPOOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOOrders> { }
		protected Decimal? _QtyPOOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase Orders", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyPOOrders
		{
			get
			{
				return this._QtyPOOrders;
			}
			set
			{
				this._QtyPOOrders = value;
			}
		}
		#endregion
		#region QtyPOReceipts
		public abstract class qtyPOReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOReceipts> { }
		protected Decimal? _QtyPOReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "PO Receipts", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Decimal? QtyPOReceipts
		{
			get
			{
				return this._QtyPOReceipts;
			}
			set
			{
				this._QtyPOReceipts = value;
			}
		}
		#endregion

		#region QtyFixedFSSrvOrd
		public abstract class qtyFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyFixedFSSrvOrd> { }
		protected decimal? _QtyFixedFSSrvOrd;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "FS to Purchase", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyFixedFSSrvOrd
		{
			get
			{
				return this._QtyFixedFSSrvOrd;
			}
			set
			{
				this._QtyFixedFSSrvOrd = value;
			}
		}
		#endregion
		#region QtyPOFixedFSSrvOrd
		public abstract class qtyPOFixedFSSrvOrd : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrd> { }
		protected decimal? _QtyPOFixedFSSrvOrd;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for FS", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyPOFixedFSSrvOrd
		{
			get
			{
				return this._QtyPOFixedFSSrvOrd;
			}
			set
			{
				this._QtyPOFixedFSSrvOrd = value;
			}
		}
		#endregion
		#region QtyPOFixedFSSrvOrdPrepared
		public abstract class qtyPOFixedFSSrvOrdPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdPrepared> { }
		protected decimal? _QtyPOFixedFSSrvOrdPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for FS Prepared", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyPOFixedFSSrvOrdPrepared
		{
			get
			{
				return this._QtyPOFixedFSSrvOrdPrepared;
			}
			set
			{
				this._QtyPOFixedFSSrvOrdPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedFSSrvOrdReceipts
		public abstract class qtyPOFixedFSSrvOrdReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedFSSrvOrdReceipts> { }
		protected decimal? _QtyPOFixedFSSrvOrdReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Receipts for FS", FieldClass = "SERVICEMANAGEMENT")]
		public virtual decimal? QtyPOFixedFSSrvOrdReceipts
		{
			get
			{
				return this._QtyPOFixedFSSrvOrdReceipts;
			}
			set
			{
				this._QtyPOFixedFSSrvOrdReceipts = value;
			}
		}
		#endregion

		#region QtySOFixed
		public abstract class qtySOFixed : PX.Data.BQL.BqlDecimal.Field<qtySOFixed> { }
		protected decimal? _QtySOFixed;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO to Purchase", Visible = false)]
		public virtual decimal? QtySOFixed
		{
			get
			{
				return this._QtySOFixed;
			}
			set
			{
				this._QtySOFixed = value;
			}
		}
		#endregion
		#region QtyPOFixedOrders
		public abstract class qtyPOFixedOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedOrders> { }
		protected decimal? _QtyPOFixedOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for SO", Visible = false)]
		public virtual decimal? QtyPOFixedOrders
		{
			get
			{
				return this._QtyPOFixedOrders;
			}
			set
			{
				this._QtyPOFixedOrders = value;
			}
		}
		#endregion
		#region QtyPOFixedPrepared
		public abstract class qtyPOFixedPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedPrepared> { }
		protected decimal? _QtyPOFixedPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for SO Prepared", Visible = false)]
		public virtual decimal? QtyPOFixedPrepared
		{
			get
			{
				return this._QtyPOFixedPrepared;
			}
			set
			{
				this._QtyPOFixedPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedReceipts
		public abstract class qtyPOFixedReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedReceipts> { }
		protected decimal? _QtyPOFixedReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Receipts for SO", Visible = false)]
		public virtual decimal? QtyPOFixedReceipts
		{
			get
			{
				return this._QtyPOFixedReceipts;
			}
			set
			{
				this._QtyPOFixedReceipts = value;
			}
		}
		#endregion
		#region QtySODropShip
		public abstract class qtySODropShip : PX.Data.BQL.BqlDecimal.Field<qtySODropShip> { }
		protected decimal? _QtySODropShip;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO to Drop-Ship", Visible = false)]
		public virtual decimal? QtySODropShip
		{
			get
			{
				return this._QtySODropShip;
			}
			set
			{
				this._QtySODropShip = value;
			}
		}
		#endregion
		#region QtyPODropShipOrders
		public abstract class qtyPODropShipOrders : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipOrders> { }
		protected decimal? _QtyPODropShipOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO", Visible = false)]
		public virtual decimal? QtyPODropShipOrders
		{
			get
			{
				return this._QtyPODropShipOrders;
			}
			set
			{
				this._QtyPODropShipOrders = value;
			}
		}
		#endregion
		#region QtyPODropShipPrepared
		public abstract class qtyPODropShipPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipPrepared> { }
		protected decimal? _QtyPODropShipPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO Prepared", Visible = false)]
		public virtual decimal? QtyPODropShipPrepared
		{
			get
			{
				return this._QtyPODropShipPrepared;
			}
			set
			{
				this._QtyPODropShipPrepared = value;
			}
		}
		#endregion
		#region QtyPODropShipReceipts
		public abstract class qtyPODropShipReceipts : PX.Data.BQL.BqlDecimal.Field<qtyPODropShipReceipts> { }
		protected decimal? _QtyPODropShipReceipts;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Drop-Ship for SO Receipts", Visible = false)]
		public virtual decimal? QtyPODropShipReceipts
		{
			get
			{
				return this._QtyPODropShipReceipts;
			}
			set
			{
				this._QtyPODropShipReceipts = value;
			}
		}
		#endregion
		#region QtyINAssemblySupply
		public abstract class qtyINAssemblySupply : PX.Data.BQL.BqlDecimal.Field<qtyINAssemblySupply> { }
		protected Decimal? _QtyINAssemblySupply;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "In Assembly Supply", Visibility = PXUIVisibility.Visible, Enabled = false, Visible = false)]
		public virtual Decimal? QtyINAssemblySupply
		{
			get
			{
				return this._QtyINAssemblySupply;
			}
			set
			{
				this._QtyINAssemblySupply = value;
			}
		}
		#endregion

		#region QtyInTransitToProduction
		public abstract class qtyInTransitToProduction : PX.Data.BQL.BqlDecimal.Field<qtyInTransitToProduction> { }
		protected Decimal? _QtyInTransitToProduction;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Transit to Production")]
		public virtual Decimal? QtyInTransitToProduction
		{
			get
			{
				return this._QtyInTransitToProduction;
			}
			set
			{
				this._QtyInTransitToProduction = value;
			}
		}
		#endregion
		#region QtyProductionSupplyPrepared
		public abstract class qtyProductionSupplyPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupplyPrepared> { }
		protected Decimal? _QtyProductionSupplyPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production Supply Prepared", FieldClass = "MANUFACTURING")]

		public virtual Decimal? QtyProductionSupplyPrepared
		{
			get
			{
				return this._QtyProductionSupplyPrepared;
			}
			set
			{
				this._QtyProductionSupplyPrepared = value;
			}
		}
		#endregion
		#region QtyProductionSupply
		public abstract class qtyProductionSupply : PX.Data.BQL.BqlDecimal.Field<qtyProductionSupply> { }
		protected Decimal? _QtyProductionSupply;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production Supply", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProductionSupply
		{
			get
			{
				return this._QtyProductionSupply;
			}
			set
			{
				this._QtyProductionSupply = value;
			}
		}
		#endregion
		#region QtyPOFixedProductionPrepared
		public abstract class qtyPOFixedProductionPrepared : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionPrepared> { }
		protected Decimal? _QtyPOFixedProductionPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for Prod. Prepared", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyPOFixedProductionPrepared
		{
			get
			{
				return this._QtyPOFixedProductionPrepared;
			}
			set
			{
				this._QtyPOFixedProductionPrepared = value;
			}
		}
		#endregion
		#region QtyPOFixedProductionOrders
		public abstract class qtyPOFixedProductionOrders : PX.Data.BQL.BqlDecimal.Field<qtyPOFixedProductionOrders> { }
		protected Decimal? _QtyPOFixedProductionOrders;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Purchase for Production", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyPOFixedProductionOrders
		{
			get
			{
				return this._QtyPOFixedProductionOrders;
			}
			set
			{
				this._QtyPOFixedProductionOrders = value;
			}
		}
		#endregion
		#region QtyProductionDemandPrepared
		public abstract class qtyProductionDemandPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemandPrepared> { }
		protected Decimal? _QtyProductionDemandPrepared;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production Demand Prepared", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProductionDemandPrepared
		{
			get
			{
				return this._QtyProductionDemandPrepared;
			}
			set
			{
				this._QtyProductionDemandPrepared = value;
			}
		}
		#endregion
		#region QtyProductionDemand
		public abstract class qtyProductionDemand : PX.Data.BQL.BqlDecimal.Field<qtyProductionDemand> { }
		protected Decimal? _QtyProductionDemand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production Demand", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProductionDemand
		{
			get
			{
				return this._QtyProductionDemand;
			}
			set
			{
				this._QtyProductionDemand = value;
			}
		}
		#endregion
		#region QtyProductionAllocated
		public abstract class qtyProductionAllocated : PX.Data.BQL.BqlDecimal.Field<qtyProductionAllocated> { }
		protected Decimal? _QtyProductionAllocated;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production Allocated", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProductionAllocated
		{
			get
			{
				return this._QtyProductionAllocated;
			}
			set
			{
				this._QtyProductionAllocated = value;
			}
		}
		#endregion
		#region QtySOFixedProduction
		public abstract class qtySOFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtySOFixedProduction> { }
		protected Decimal? _QtySOFixedProduction;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "SO to Production", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtySOFixedProduction
		{
			get
			{
				return this._QtySOFixedProduction;
			}
			set
			{
				this._QtySOFixedProduction = value;
			}
		}
		#endregion
		#region QtyProdFixedPurchase
		// M9
		public abstract class qtyProdFixedPurchase : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedPurchase> { }
		protected Decimal? _QtyProdFixedPurchase;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production to Purchase.  
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production to Purchase", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedPurchase
		{
			get
			{
				return this._QtyProdFixedPurchase;
			}
			set
			{
				this._QtyProdFixedPurchase = value;
			}
		}
		#endregion
		#region QtyProdFixedProduction
		// MA
		public abstract class qtyProdFixedProduction : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProduction> { }
		protected Decimal? _QtyProdFixedProduction;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production to Production
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production to Production", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedProduction
		{
			get
			{
				return this._QtyProdFixedProduction;
			}
			set
			{
				this._QtyProdFixedProduction = value;
			}
		}
		#endregion
		#region QtyProdFixedProdOrdersPrepared
		// MB
		public abstract class qtyProdFixedProdOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrdersPrepared> { }
		protected Decimal? _QtyProdFixedProdOrdersPrepared;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for Prod. Prepared
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production for Prod. Prepared", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedProdOrdersPrepared
		{
			get
			{
				return this._QtyProdFixedProdOrdersPrepared;
			}
			set
			{
				this._QtyProdFixedProdOrdersPrepared = value;
			}
		}
		#endregion
		#region QtyProdFixedProdOrders
		// MC
		public abstract class qtyProdFixedProdOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedProdOrders> { }
		protected Decimal? _QtyProdFixedProdOrders;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for Production
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production for Production", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedProdOrders
		{
			get
			{
				return this._QtyProdFixedProdOrders;
			}
			set
			{
				this._QtyProdFixedProdOrders = value;
			}
		}
		#endregion
		#region QtyProdFixedSalesOrdersPrepared
		// MD
		public abstract class qtyProdFixedSalesOrdersPrepared : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrdersPrepared> { }
		protected Decimal? _QtyProdFixedSalesOrdersPrepared;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for SO Prepared
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production for SO Prepared", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedSalesOrdersPrepared
		{
			get
			{
				return this._QtyProdFixedSalesOrdersPrepared;
			}
			set
			{
				this._QtyProdFixedSalesOrdersPrepared = value;
			}
		}
		#endregion
		#region QtyProdFixedSalesOrders
		// ME
		public abstract class qtyProdFixedSalesOrders : PX.Data.BQL.BqlDecimal.Field<qtyProdFixedSalesOrders> { }
		protected Decimal? _QtyProdFixedSalesOrders;
		/// <summary>
		/// Production / Manufacturing 
		/// Specifies the quantity On Production for SO
		/// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Production for SO", FieldClass = "MANUFACTURING")]
		public virtual Decimal? QtyProdFixedSalesOrders
		{
			get
			{
				return this._QtyProdFixedSalesOrders;
			}
			set
			{
				this._QtyProdFixedSalesOrders = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "On Hand", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion

		#region QtyExpired
		public abstract class qtyExpired : PX.Data.BQL.BqlDecimal.Field<qtyExpired> { }
		protected Decimal? _QtyExpired;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Expired", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? QtyExpired
		{
			get
			{
				return this._QtyExpired;
			}
			set
			{
				this._QtyExpired = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible)]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region Qty		
		protected Decimal? _Qty;
		[PXQuantity()]
		public virtual Decimal? Qty
		{
			[PXDependsOnFields(typeof(qtyOnHand), typeof(qtyInTransit))]
			get
			{
				return this.QtyOnHand + this.QtyInTransit;
			}
		}
		#endregion

		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Unit Cost")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion

		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Total Cost")]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Lot/Serial Number", Visible = false)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion

		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Visible = false)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion
		#region IsTotal
		public abstract class isTotal : PX.Data.BQL.BqlBool.Field<isTotal>
		{
		}
		[PXBool()]
		public virtual bool? IsTotal
		{
			get;
			set;
		}
		#endregion

		/// <exclude />
		public string ControlTimeStamp { get; set; }
	}

	#endregion

	[TableAndChartDashboardType]
	public class InventorySummaryEnq : PXGraph<InventorySummaryEnq>
	{
		public class ItemPlanHelper : ItemPlanHelper<InventorySummaryEnq>
		{
		}
		public ItemPlanHelper ItemPlanHelperExt => FindImplementation<ItemPlanHelper>();

		public PXCancel<InventorySummaryEnqFilter> Cancel;
		public PXAction<InventorySummaryEnqFilter> viewAllocDet;
		public PXFilter<InventorySummaryEnqFilter> Filter;
		[PXFilterable]
		public PXSelect<InventorySummaryEnquiryResult> ISERecords; // ISE = Inventory Summary Enquiry
		protected virtual IEnumerable iSERecords()
		{
			string controlTimeStamp = ControlTimeStamp;
			if (PXView.MaximumRows == 1 && PXView.Searches != null && PXView.Searches.Length == 1)
			{
				InventorySummaryEnquiryResult other = new InventorySummaryEnquiryResult();
				other.GridLineNbr = (int?)PXView.Searches[0];
				other = (InventorySummaryEnquiryResult)ISERecords.Cache.Locate(other);
				if (other != null && other.ControlTimeStamp == controlTimeStamp)
				{
					PXDelegateResult oneRowResult = new PXDelegateResult();
					oneRowResult.Add(other);
					oneRowResult.IsResultFiltered = true;
					oneRowResult.IsResultSorted = true;
					oneRowResult.IsResultTruncated = true;

					return oneRowResult;
				}
			}
			int lineIndex = 0;

			if (!ISERecords.Cache.Cached.Any_() || ISERecords.Cache.Cached.RowCast<InventorySummaryEnquiryResult>().First().ControlTimeStamp != controlTimeStamp)
			{
				ISERecords.Cache.Clear();
				foreach (var item in iSERecordsFetch())
				{
					item.GridLineNbr = ++lineIndex;
					ISERecords.Cache.Hold(item);
				}
			}
			else
				lineIndex = (int)ISERecords.Cache.Cached.Count();

			if (!IsImport && !IsExport && !IsContractBasedAPI)
			{
				var resultset = ISERecords.Cache.Cached.RowCast<InventorySummaryEnquiryResult>();
				var total = CalculateSummaryTotal(resultset);
				total.GridLineNbr = ++lineIndex;

				return SortSummaryResult(resultset, total);
			}
			else
			{
				return ISERecords.Cache.Cached;
			}
		}

		public PXSetupOptional<CommonSetup> commonsetup;

		private Lazy<string> _locationDisplayName;


		public InventorySummaryEnq()
		{
			ISERecords.Cache.AllowInsert = false;
			ISERecords.Cache.AllowDelete = false;
			ISERecords.Cache.AllowUpdate = false;

			CommonSetup record = commonsetup.Current;

			_locationDisplayName = new Lazy<string>(GetLocationDisplayName);

			PXUIFieldAttribute.SetVisible<InventorySummaryEnqFilter.expandByCostLayerType>(Filter.Cache, null,
				PXAccess.FeatureInstalled<FeaturesSet.specialOrders>() || PXAccess.FeatureInstalled<FeaturesSet.materialManagement>());
		}

		protected virtual void _(Events.FieldDefaulting<InventorySummaryEnqFilter.locationID> e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<InventorySummaryEnquiryResult.locationID> e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void _(Events.RowSelected<InventorySummaryEnqFilter> e)
		{
			if (e.Row == null) return;

			ISERecords.Cache.AdjustUI()
				.For<InventorySummaryEnquiryResult.lotSerialNbr>(a => a.Visible = (e.Row.ExpandByLotSerialNbr == true))
				.SameFor<InventorySummaryEnquiryResult.expireDate>()
				.For<InventorySummaryEnquiryResult.costLayerType>(a => a.Visible = (e.Row.ExpandByCostLayerType == true));
		}

		protected virtual void _(Events.RowInserted<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();
		protected virtual void _(Events.RowUpdated<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();
		protected virtual void _(Events.RowDeleted<InventorySummaryEnqFilter> e) => ISERecords.Cache.Clear();

		protected virtual void AppendCostLocationLayerJoin(PXSelectBase<INLocationStatusByCostCenter> cmd)
		{
		}

		protected virtual void AppendCostLotSerialLayerJoin(PXSelectBase<INLotSerialStatusByCostCenter> cmd)
		{
		}

		protected virtual void AppendFilters<T>(PXSelectBase<T> cmd, InventorySummaryEnqFilter filter)
			where T : class, IBqlTable, new()
		{
			if (filter.InventoryID != null)
			{
				cmd.WhereAnd<Where<InventoryItem.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD))
			{
				cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventorySummaryEnqFilter.subItemCDWildcard>>>>();
			}

			if (filter.SiteID != null)
			{
				cmd.WhereAnd<Where<INSite.siteID, Equal<Current<InventorySummaryEnqFilter.siteID>>>>();
			}

			if (PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>())
			{
				cmd.WhereAnd<Where<INSite.branchID, InsideBranchesOf<Current<InventorySummaryEnqFilter.orgBAccountID>>>>();
			}

			if (typeof(T).IsNotIn(typeof(INSiteStatusByCostCenter), typeof(INItemPlan)) && filter.LocationID != null)
			{
				cmd.WhereAnd<Where<INLocation.locationID, Equal<Current<InventorySummaryEnqFilter.locationID>>>>();
			}
		}

		protected virtual void _(Events.FieldSelecting<InventorySummaryEnquiryResult.locationID> e)
		{
			string locationName = null;

			switch (e.ReturnValue)
			{
				case null:
					locationName = Messages.Unassigned;
					break;
				case InventorySummaryEnquiryResult.TotalLocationID:
					locationName = Messages.TotalLocation;
					break;
			}

			if (locationName != null)
			{
				e.ReturnState = PXFieldState.CreateInstance(PXMessages.LocalizeNoPrefix(locationName), typeof(string), false, null, null, null, null, null,
					nameof(InventorySummaryEnquiryResult.locationID), null, _locationDisplayName.Value, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
				e.Cancel = true;
			}
		}

		private string GetLocationDisplayName()
		{
			var displayName = PXUIFieldAttribute.GetDisplayName<InventorySummaryEnquiryResult.locationID>(ISERecords.Cache);
			if (displayName != null) displayName = PXMessages.LocalizeNoPrefix(displayName);

			return displayName;
		}

		protected virtual List<Type> GetCostTables()
		{
			return new List<Type>(new Type[] { typeof(INLocationCostStatus) });
		}

		protected virtual IEnumerable<InventorySummaryEnquiryResult> iSERecordsFetch()
		{
			try
			{
				return iSERecordsFetchImpl();
			}
			finally
			{
				this.Caches<SiteStatusAggregate>().Clear();
				this.Caches<LocationStatusAggregate>().Clear();
				this.Caches<LotSerialStatusAggregate>().Clear();
			}
		}

		private IEnumerable<InventorySummaryEnquiryResult> iSERecordsFetchImpl()
		{
			InventorySummaryEnqFilter filter = Filter.Current;
			string controlTimeStamp = ControlTimeStamp;

			PXSelectBase<INLotSerialStatusByCostCenter> cmd_lss = new PXSelectReadonly2<INLotSerialStatusByCostCenter,
				InnerJoin<INLocation,
					On<INLotSerialStatusByCostCenter.FK.Location>,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INLotSerialStatusByCostCenter.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INLotSerialStatusByCostCenter.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INLotSerialStatusByCostCenter.FK.SubItem>,
				LeftJoin<INCostCenter,
					On<INLotSerialStatusByCostCenter.FK.CostCenter>,
				LeftJoin<INLotSerClass,
					On<InventoryItem.FK.LotSerialClass>,
				LeftJoin<INLocationCostStatus,
					On<INLocationCostStatus.inventoryID, Equal<INLotSerialStatusByCostCenter.inventoryID>,
						And<INLocationCostStatus.subItemID, Equal<INLotSerialStatusByCostCenter.subItemID>,
						And<INLocationCostStatus.locationID, Equal<INLotSerialStatusByCostCenter.locationID>,
						And<INLotSerialStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>,
						And<INLocationCostStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>,
                        And<Where<InventoryItem.valMethod, NotEqual<INValMethod.specific>,
                        Or<Current<InventorySummaryEnqFilter.expandByLotSerialNbr>, Equal<True>>>>>>>>>>>>>>>>,
				Where<INLotSerialStatusByCostCenter.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>,
					And<Where<Current<InventorySummaryEnqFilter.expandByLotSerialNbr>, Equal<True>,
						And<INLotSerClass.lotSerAssign, Equal<INLotSerAssign.whenReceived>,
						And<INLotSerClass.lotSerTrack, NotEqual<INLotSerTrack.notNumbered>,
						Or<InventoryItem.valMethod, Equal<INValMethod.specific>,
						Or<INLotSerClass.lotSerTrackExpiration, Equal<True>,
						And<INLotSerClass.lotSerTrack, NotEqual<INLotSerTrack.notNumbered>>>>>>>>>>(this);

			AppendCostLotSerialLayerJoin(cmd_lss);

			AppendFilters<INLotSerialStatusByCostCenter>(cmd_lss, filter);

			var cmdLotSerialCostStatus = new PXSelectReadonly<INLotSerialCostStatusByCostLayerType,
				Where<INLotSerialCostStatusByCostLayerType.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>,
					And<INLotSerialCostStatusByCostLayerType.lotSerialNbr, IsNotNull>>>(this);
			var cmdSiteCostStatus = new PXSelectReadonly<INSiteCostStatusByCostLayerType,
				Where<INSiteCostStatusByCostLayerType.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>(this);

			if (filter.SiteID != null)
			{
				cmdLotSerialCostStatus.WhereAnd<Where<INLotSerialCostStatusByCostLayerType.siteID, Equal<Current<InventorySummaryEnqFilter.siteID>>>>();
				cmdSiteCostStatus.WhereAnd<Where<INSiteCostStatusByCostLayerType.siteID, Equal<Current<InventorySummaryEnqFilter.siteID>>>>();
			}

			if (filter.InventoryID != null)
			{
				foreach (INLotSerialCostStatusByCostLayerType res in cmdLotSerialCostStatus.Select())
					cmdLotSerialCostStatus.Cache.SetStatus(res, PXEntryStatus.Notchanged);
				foreach (INSiteCostStatusByCostLayerType res in cmdSiteCostStatus.Select())
					cmdSiteCostStatus.Cache.SetStatus(res, PXEntryStatus.Notchanged);
			}

			List<Type> fieldScope = new List<Type>(new Type[] {
				typeof(INLotSerialStatusByCostCenter),
				typeof(INSite.siteCD),
				typeof(INSubItem.subItemCD),
				typeof(INCostCenter.costLayerType),
				typeof(INLocation.locationCD),
				typeof(INLocation.inclQtyAvail),
				typeof(InventoryItem.baseUnit)
			});
			fieldScope.AddRange(GetCostTables());

			using (new PXFieldScope(cmd_lss.View, fieldScope.ToArray()))
			{
				foreach (PXResult<INLotSerialStatusByCostCenter, INLocation, InventoryItem, INSite, INSubItem, INCostCenter> res in cmd_lss.Select())
				{
					INLotSerialStatusByCostCenter lss_rec = res;
					INLocation loc_rec = res;
					InventoryItem item_rec = res;
					INCostCenter costCenter = res;

					LotSerialStatusAggregate ret = LocateOrInsertLotSerialAggregate(lss_rec, out bool newRecord);
					ret.Add(lss_rec);

					if (newRecord)
					{
						ret.CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal;
						ret.SubItemCD = ((INSubItem)res).SubItemCD;
						ret.SiteCD = ((INSite)res).SiteCD;
						ret.LocationCD = ((INLocation)res).LocationCD;
						ret.ExpireDate = lss_rec.ExpireDate;
						ret.BaseUnit = item_rec.BaseUnit;
					}

					ICostStatus costStatus = cmdLotSerialCostStatus.Locate(
						new INLotSerialCostStatusByCostLayerType()
						{
							InventoryID = lss_rec.InventoryID,
							SubItemID = lss_rec.SubItemID,
							SiteID = lss_rec.SiteID,
							LotSerialNbr = lss_rec.LotSerialNbr,
							CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal,
						});
					if (costStatus?.TotalCost == null)
					{
						costStatus = PXResult.Unwrap<INLocationCostStatus>(res);
					}
					if (costStatus?.TotalCost == null)
					{
						costStatus = cmdSiteCostStatus.Locate(new INSiteCostStatusByCostLayerType
						{
							InventoryID = lss_rec.InventoryID,
							SubItemID = lss_rec.SubItemID,
							SiteID = lss_rec.SiteID,
							CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal,
						});
					}
					ret.TotalCost += CalculateUnitCost(costStatus, false) * lss_rec.QtyOnHand;

					if (loc_rec.InclQtyAvail == false)
					{
						ret.QtyNotAvail = ret.QtyAvail;
						ret.QtyAvail = 0m;
						ret.QtyHardAvail = 0m;
						ret.QtyActual = 0m;
					}
					else
					{
						ret.QtyNotAvail = 0m;
					}

					if (lss_rec.ExpireDate != null && DateTime.Compare((DateTime)this.Accessinfo.BusinessDate, (DateTime)lss_rec.ExpireDate) > 0)
					{
						ret.QtyExpired = lss_rec.QtyOnHand;
					}

					if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
					{
						var aggregate = LocateOrInsertLocationAggregate(ret, true, out bool _);
						aggregate.Add(ret);
					}
					else
					{
						var aggregate = LocateOrInsertSiteAggregate(ret, true, out bool _);
						aggregate.Add(ret);
					}

					ret.QtyAvail -= ret.QtyExpired;
                    if (loc_rec.InclQtyAvail == false && ret.QtyAvail < 0) ret.QtyAvail = 0;
                }
			}

			PXSelectBase<INLocationStatusByCostCenter> cmd_ls = new PXSelectReadonly2<INLocationStatusByCostCenter,
				InnerJoin<INLocation,
					On<INLocationStatusByCostCenter.FK.Location>,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INLocationStatusByCostCenter.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INLocationStatusByCostCenter.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INLocationStatusByCostCenter.FK.SubItem>,
				LeftJoin<INCostCenter,
					On<INLocationStatusByCostCenter.FK.CostCenter>,
				LeftJoin<INLocationCostStatus,
					On<INLocationCostStatus.inventoryID, Equal<INLocationStatusByCostCenter.inventoryID>,
						And<INLocationCostStatus.subItemID, Equal<INLocationStatusByCostCenter.subItemID>,
						And<INLocationCostStatus.locationID, Equal<INLocationStatusByCostCenter.locationID>,
						And<INLocationStatusByCostCenter.costCenterID, Equal<CostCenter.freeStock>,
						And<INLocationCostStatus.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>>>>>>>>>>,
				Where<INLocationStatusByCostCenter.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>(this);

			AppendCostLocationLayerJoin(cmd_ls);
			AppendFilters<INLocationStatusByCostCenter>(cmd_ls, filter);

			foreach (PXResult<INLocationStatusByCostCenter, INLocation, InventoryItem, INSite, INSubItem, INCostCenter> res in cmd_ls.Select())
			{
				INLocationStatusByCostCenter ls_rec = res;
				INLocation loc_rec = res;
				InventoryItem item_rec = res;
				INCostCenter costCenter = res;

				var ret = new LocationStatusAggregate
				{
					InventoryID = ls_rec.InventoryID,
					SubItemID = ls_rec.SubItemID,
					SiteID = ls_rec.SiteID,
					LocationID = ls_rec.LocationID,
					CostCenterID = ls_rec.CostCenterID,
				};
				ret = LocateOrInsertLocationAggregate(ret, false, out bool newRecord);
				ret.Add(ls_rec);

				if (newRecord)
				{
					ret.CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal;
					ret.SubItemCD = ((INSubItem)res).SubItemCD;
					ret.SiteCD = ((INSite)res).SiteCD;
					ret.LocationCD = ((INLocation)res).LocationCD;
					ret.BaseUnit = item_rec.BaseUnit;
				}
				ICostStatus costStatus = PXResult.Unwrap<INLocationCostStatus>(res);
				if (costStatus?.TotalCost == null)
				{
					costStatus = cmdSiteCostStatus.Locate(new INSiteCostStatusByCostLayerType
					{
						InventoryID = ls_rec.InventoryID,
						SubItemID = ls_rec.SubItemID,
						SiteID = ls_rec.SiteID,
						CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal,
					});
				}
				ret.TotalCost += CalculateUnitCost(costStatus, false) * ls_rec.QtyOnHand;

				if (loc_rec.InclQtyAvail == false)
				{
					ret.QtyNotAvail = ret.QtyAvail;
					ret.QtyAvail = 0m;
					ret.QtyHardAvail = 0m;
					ret.QtyActual = 0m;
				}
				else
				{
					ret.QtyNotAvail = 0m;
				}

				var siteAggregate = new Lazy<SiteStatusAggregate>(() =>
				{
					return LocateOrInsertSiteAggregate(ret, true, out bool _);
				});
				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
					siteAggregate.Value.Add(ret);
				else
				{
					bool skipCostAndQty = (item_rec.ValMethod == INValMethod.Specific);

					if (!skipCostAndQty)
					{
						var lotSerClass = INLotSerClass.PK.Find(this, item_rec.LotSerClassID);

						skipCostAndQty = lotSerClass?.LotSerTrack != INLotSerTrack.NotNumbered &&
							(lotSerClass?.LotSerTrackExpiration == true ||
								(filter.ExpandByLotSerialNbr == true && lotSerClass?.LotSerAssign == INLotSerAssign.WhenReceived));
					}

					if (skipCostAndQty)
					{
						//TotalCost and QtyOnHand already were accumulated when INLotSerialStatus processing
					}
					else
					{
						siteAggregate.Value.QtyOnHand += ret.QtyOnHand;
						siteAggregate.Value.TotalCost += ret.TotalCost;
					}
				}

				var excludeAggregate = LocateOrInsertLocationAggregate(ret, true, out bool _);
				if (filter.ExpandByLotSerialNbr == true)
				{
					ret = ret.Subtract(excludeAggregate);
				}
				else
				{
					if (excludeAggregate.TotalCost != 0m)
					{
						ret.TotalCost = excludeAggregate.TotalCost;
						ret.UnitCost = CalculateUnitCost(excludeAggregate, true);
					}
					ret.QtyExpired += excludeAggregate.QtyExpired;
					ret.QtyAvail -= excludeAggregate.QtyExpired;
                    if (loc_rec.InclQtyAvail == false && ret.QtyAvail < 0) ret.QtyAvail = 0;
                }
			}

			if (filter.ExpandByLotSerialNbr == true)
			{
				var shippingPlanTypes = PXSelectReadonly<INPlanType, Where<INPlanType.inclQtySOShipping, Equal<True>>>.SelectMultiBound(this, null).AsEnumerable();
				PXSelectBase<INItemPlan> cmd_plans = new PXSelectReadonly2<INItemPlan,
					InnerJoin<InventoryItem,
						On2<INItemPlan.FK.InventoryItem,
						And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
					InnerJoin<INSite,
						On2<INItemPlan.FK.Site,
						And<Match<INSite, Current<AccessInfo.userName>>>>,
					InnerJoin<INSubItem,
						On<INItemPlan.FK.SubItem>,
					InnerJoin<INPlanType,
						On<INItemPlan.FK.PlanType>>>>>,
					Where<
						INPlanType.inclQtySOShipping, Equal<decimal1>,
						And<INItemPlan.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>>(this);

				AppendFilters<INItemPlan>(cmd_plans, filter);

				var lstPlans = new List<PXResult<INItemPlan, InventoryItem, INSite, INSubItem>>();

				foreach (PXResult<INItemPlan, InventoryItem, INSite, INSubItem> res in cmd_plans.Select())
				{
					lstPlans.Add(res);
				}

				for (int i = 0; i < lstPlans.Count; i++)
				{
					INItemPlan plan_rec = lstPlans[i];

					if (shippingPlanTypes.Any(x => ((INPlanType)x).PlanType == plan_rec.OrigPlanType))
					{
						for (int j = 0; j < lstPlans.Count; j++)
						{
							INItemPlan origplan_rec = lstPlans[j];

							if (origplan_rec.PlanID == plan_rec.OrigPlanID
								||
								(origplan_rec.RefNoteID == plan_rec.OrigNoteID &&
								origplan_rec.PlanType == plan_rec.OrigPlanType &&
								origplan_rec.Reverse == plan_rec.Reverse &&
								origplan_rec.SubItemID == plan_rec.SubItemID &&
								origplan_rec.SiteID == plan_rec.SiteID &&
								origplan_rec.LocationID == null &&
								string.Equals(origplan_rec.LotSerialNbr, plan_rec.LotSerialNbr, StringComparison.OrdinalIgnoreCase)))
							{
								origplan_rec.PlanQty -= plan_rec.PlanQty;
								plan_rec.PlanQty = 0m;
								break;
							}
						}
					}
				}

				foreach (PXResult<INItemPlan, InventoryItem, INSite, INSubItem> res in lstPlans)
				{
					INItemPlan plan_rec = res;
					InventoryItem item_rec = res;

					if (plan_rec.PlanQty == 0m || String.IsNullOrEmpty(plan_rec.LotSerialNbr) || plan_rec.LocationID != null)
						continue;

					var item = InventoryItem.PK.Find(this, plan_rec.InventoryID);
					var subItem = INSubItem.PK.Find(this, plan_rec.SubItemID);
					var site = INSite.PK.Find(this, plan_rec.SiteID);
					var itemLotSerial = INItemLotSerial.PK.Find(this, plan_rec.InventoryID, plan_rec.LotSerialNbr);
					var costCenter = INCostCenter.PK.Find(this, plan_rec.CostCenterID);
					var siteStatusUpdCache = this.Caches<SiteStatusByCostCenter>();
					var siteStatusUpd = (SiteStatusByCostCenter)siteStatusUpdCache.Insert(new SiteStatusByCostCenter
					{
						SiteID = plan_rec.SiteID,
						InventoryID = plan_rec.InventoryID,
						SubItemID = plan_rec.SubItemID,
						CostCenterID = plan_rec.CostCenterID,
					});
					ItemPlanHelperExt.UpdateAllocatedQuantitiesBase<SiteStatusByCostCenter>(
						siteStatusUpd,
						plan_rec,
						shippingPlanTypes.First(x => ((INPlanType)x).PlanType == plan_rec.PlanType),
						siteStatusUpd.InclQtyAvail.GetValueOrDefault(),
						plan_rec.Hold,
						plan_rec.RefEntityType);

					var lotSerialCache = this.Caches<LotSerialStatusAggregate>();
					var lotSerialRec = new LotSerialStatusAggregate
					{
						SiteID = plan_rec.SiteID,
						SiteCD = site?.SiteCD,
						InventoryID = plan_rec.InventoryID,
						BaseUnit = item?.BaseUnit,
						SubItemID = plan_rec.SubItemID,
						SubItemCD = subItem?.SubItemCD,
						LocationID = InventorySummaryEnquiryResult.EmptyLocationID,
						LotSerialNbr = plan_rec.LotSerialNbr,
						ExpireDate = itemLotSerial?.ExpireDate,
						CostCenterID = plan_rec.CostCenterID,
						CostLayerType = costCenter?.CostLayerType ?? CostLayerType.Normal,
					};
					lotSerialRec = (LotSerialStatusAggregate)(lotSerialCache.Locate(lotSerialRec) ?? lotSerialCache.Insert(lotSerialRec));
					lotSerialRec.Add(siteStatusUpd);

					var siteCache = this.Caches<SiteStatusAggregate>();
					var siteRec = new SiteStatusAggregate
					{
						SiteID = plan_rec.SiteID,
						SiteCD = site?.SiteCD,
						InventoryID = plan_rec.InventoryID,
						BaseUnit = item?.BaseUnit,
						SubItemID = plan_rec.SubItemID,
						SubItemCD = subItem?.SubItemCD,
						CostCenterID = plan_rec.CostCenterID,
						CostLayerType = costCenter?.CostLayerType ?? CostLayerType.Normal,
					};
					siteRec = LocateOrInsertSiteAggregate(siteRec, false, out bool _);
					siteRec.Subtract(siteStatusUpd);

					siteStatusUpdCache.Clear();
				}
			}

			PXSelectBase<INSiteStatusByCostCenter> cmd_ss = new PXSelectReadonly2<INSiteStatusByCostCenter,
				InnerJoin<InventoryItem,
					On<InventoryItem.inventoryID, Equal<INSiteStatusByCostCenter.inventoryID>,
					And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
				InnerJoin<INSite,
					On2<INSiteStatusByCostCenter.FK.Site,
					And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
					And<Match<INSite, Current<AccessInfo.userName>>>>>,
				InnerJoin<INSubItem,
					On<INSiteStatusByCostCenter.FK.SubItem>,
				LeftJoin<INCostCenter, On<INSiteStatusByCostCenter.FK.CostCenter>>>>>,
				Where<INSiteStatusByCostCenter.inventoryID, Equal<Current<InventorySummaryEnqFilter.inventoryID>>>>(this);

			AppendFilters<INSiteStatusByCostCenter>(cmd_ss, filter);

			foreach (PXResult<INSiteStatusByCostCenter, InventoryItem, INSite, INSubItem, INCostCenter> res in cmd_ss.Select())
			{
				INSiteStatusByCostCenter ss_rec = res;
				InventoryItem item_rec = res;
				INCostCenter costCenter = res;

				var ret = new SiteStatusAggregate
				{
					InventoryID = ss_rec.InventoryID,
					SubItemID = ss_rec.SubItemID,
					SiteID = ss_rec.SiteID,
					CostCenterID = ss_rec.CostCenterID,
				};
				ret = LocateOrInsertSiteAggregate(ret, false, out bool _);
				ret.Add(ss_rec);

				ret.CostLayerType = costCenter.CostLayerType ?? CostLayerType.Normal;
				ret.SubItemCD = ((INSubItem)res).SubItemCD;
				ret.SiteCD = ((INSite)res).SiteCD;
				ret.BaseUnit = item_rec.BaseUnit;
				ret.TotalCost = 0m;
				ret.QtyExpired = 0m;

				var excludeAggregate = LocateOrInsertSiteAggregate(ret, true, out bool _);
				if (PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() || filter.ExpandByLotSerialNbr == true)
				{
					ret = ret.Subtract(excludeAggregate);
					ret.TotalCost = 0m;
				}
				else
				{
					if (excludeAggregate.TotalCost != 0m)
					{
						ret.TotalCost = excludeAggregate.TotalCost;
						ret.UnitCost = CalculateUnitCost(excludeAggregate, true);
					}

					ret.QtyExpired += excludeAggregate.QtyExpired;
					ret.QtyAvail -= excludeAggregate.QtyExpired;
				}
			}

			return Array<SiteStatusAggregate.ITable>.Empty
				.Concat(
					AggregateRecords(
						this.Caches<LotSerialStatusAggregate>().Inserted.Cast<LotSerialStatusAggregate>()
							.Where(s => filter.ExpandByLotSerialNbr == true && !s.IsZero()),
						r => new { r.InventoryID, r.SubItemID, r.SiteID, r.LocationID, r.LotSerialNbr, CostLayerType = GetCostLayerTypeForAggregate(r) }))
				.Concat(
					AggregateRecords(
						this.Caches<LocationStatusAggregate>().Inserted.Cast<LocationStatusAggregate>()
							.Where(s => PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() && s.ExcludeRecord != true && !s.IsZero()),
						r => new { r.InventoryID, r.SubItemID, r.SiteID, r.LocationID, CostLayerType = GetCostLayerTypeForAggregate(r) }))
				.Concat(
					AggregateRecords(
						this.Caches<SiteStatusAggregate>().Inserted.Cast<SiteStatusAggregate>()
							.Where(s => (!PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>() || filter.LocationID == null)
								&& s.ExcludeRecord != true && !s.IsZero()),
						r => new { r.InventoryID, r.SubItemID, r.SiteID, CostLayerType = GetCostLayerTypeForAggregate(r) }))
				.OrderBy(s => s.CostLayerType)
				.ThenBy(s => s.SubItemCD)
				.ThenBy(s => s.SiteCD)
				.ThenBy(s => (s as LocationStatusAggregate.ITable)?.LocationCD)
				.ThenBy(s => (s as LotSerialStatusAggregate)?.LotSerialNbr)
				.Select(s => ConvertToResult(s, controlTimeStamp));
		}

		private string GetCostLayerTypeForAggregate(SiteStatusAggregate.ITable t)
			=> Filter.Current.ExpandByCostLayerType == true ? t.CostLayerType : CostLayerType.Normal;

		private IEnumerable<T> AggregateRecords<T, TKey>(IEnumerable<T> enumerable, Func<T, TKey> groupSel)
			where T : class, SiteStatusAggregate.ITable, new()
		{
			foreach (var group in enumerable.GroupBy(groupSel))
			{
				T aggregate = null;
				foreach (T rec in group)
				{
					if (aggregate == null)
					{
						aggregate = (T)Caches[typeof(T)].CreateCopy(rec);
					}
					else
					{
						aggregate.Add(rec);
					}
				}
				aggregate.UnitCost = CalculateUnitCost(aggregate, true);
				aggregate.TotalCost = PXDBCurrencyAttribute.BaseRound(this, aggregate.TotalCost ?? 0m);
				yield return aggregate;
			}
		}

		private InventorySummaryEnquiryResult ConvertToResult(SiteStatusAggregate.ITable s, string controlTimeStamp)
		{
			var ret = new InventorySummaryEnquiryResult
			{
				InventoryID = s.InventoryID,
				SubItemID = s.SubItemID,
				SiteID = s.SiteID,
				CostLayerType = s.CostLayerType,

				BaseUnit = s.BaseUnit,
				UnitCost = s.UnitCost,
				TotalCost = s.TotalCost,

				ControlTimeStamp = controlTimeStamp,
			};
			ret.OverrideBy(s);

			if (s is LotSerialStatusAggregate lotSerialStatus)
			{
				ret.LocationID = (lotSerialStatus.LocationID == InventorySummaryEnquiryResult.EmptyLocationID) ? null : lotSerialStatus.LocationID;
				ret.LotSerialNbr = lotSerialStatus.LotSerialNbr;
				ret.ExpireDate = lotSerialStatus.ExpireDate;
			}
			else if (s is LocationStatusAggregate locationStatus)
			{
				ret.LocationID = locationStatus.LocationID;
			}

			return ret;
		}

		private decimal CalculateUnitCost(ICostStatus st, bool round)
		{
			decimal unitCost = ((st?.QtyOnHand ?? 0m) != 0m ? st.TotalCost / st.QtyOnHand : 0m) ?? 0m;
			return round ? Math.Round(unitCost, (int)commonsetup.Current.DecPlPrcCst, MidpointRounding.AwayFromZero)
				: unitCost;
		}

		protected virtual IEnumerable SortSummaryResult(IEnumerable<InventorySummaryEnquiryResult> resultset,
			InventorySummaryEnquiryResult total)
		{
			var delegateResult = new PXDelegateResult() { IsResultSorted = true };

			var sortedResultset = PXView.Sort(resultset).RowCast<InventorySummaryEnquiryResult>();

			if (resultset.Any())
			{
				if (!PXView.ReverseOrder)
				{
					delegateResult.AddRange(sortedResultset);
					delegateResult.Add(total);
				}
				else
				{
					delegateResult.Add(total);
					delegateResult.AddRange(sortedResultset);
				}
			}

			return delegateResult;
		}

		protected virtual InventorySummaryEnquiryResult CalculateSummaryTotal(IEnumerable<InventorySummaryEnquiryResult> resultset)
		{
			InventorySummaryEnquiryResult total = resultset.CalculateSumTotal(ISERecords.Cache);
			total.IsTotal = true;
			total.LocationID = InventorySummaryEnquiryResult.TotalLocationID;
			total.SiteID = null;
			total.UnitCost = null;
			return total;
		}

		private SiteStatusAggregate LocateOrInsertSiteAggregate(SiteStatusAggregate.ITable ret, bool exclude, out bool inserted)
		{
			var aggregate = new SiteStatusAggregate
			{
				InventoryID = ret.InventoryID,
				SubItemID = ret.SubItemID,
				SiteID = ret.SiteID,
				CostCenterID = ret.CostCenterID,
				ExcludeRecord = exclude,
			};
			PXCache cache = this.Caches<SiteStatusAggregate>();
			var located = (SiteStatusAggregate)cache.Locate(aggregate);
			if (located != null)
			{
				inserted = false;
				return located;
			}
			else
			{
				inserted = true;
				return (SiteStatusAggregate)cache.Insert(aggregate);
			}
		}

		private LocationStatusAggregate LocateOrInsertLocationAggregate(LocationStatusAggregate.ITable ret, bool exclude, out bool inserted)
		{
			var aggregate = new LocationStatusAggregate
			{
				InventoryID = ret.InventoryID,
				SubItemID = ret.SubItemID,
				SiteID = ret.SiteID,
				LocationID = ret.LocationID,
				CostCenterID = ret.CostCenterID,
				ExcludeRecord = exclude,
			};
			PXCache cache = this.Caches<LocationStatusAggregate>();
			var located = (LocationStatusAggregate)cache.Locate(aggregate);
			if (located != null)
			{
				inserted = false;
				return located;
			}
			else
			{
				inserted = true;
				return (LocationStatusAggregate)cache.Insert(aggregate);
			}
		}

		private LotSerialStatusAggregate LocateOrInsertLotSerialAggregate(INLotSerialStatusByCostCenter st, out bool inserted)
		{
			var aggregate = new LotSerialStatusAggregate
			{
				InventoryID = st.InventoryID,
				SubItemID = st.SubItemID,
				SiteID = st.SiteID,
				LocationID = st.LocationID,
				LotSerialNbr = st.LotSerialNbr,
				CostCenterID = st.CostCenterID,
			};
			PXCache cache = this.Caches<LotSerialStatusAggregate>();
			var located = (LotSerialStatusAggregate)cache.Locate(aggregate);
			if (located != null)
			{
				inserted = false;
				return located;
			}
			else
			{
				inserted = true;
				return (LotSerialStatusAggregate)cache.Insert(aggregate);
			}
		}

		public override bool IsDirty => false;


		[PXUIField(DisplayName = "")]
		[PXEditDetailButton]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.ISERecords.Current != null)
			{
				object subItem =
						this.ISERecords.Cache.GetValueExt<InventorySummaryEnquiryResult.subItemID>
					(this.ISERecords.Current);

				if (subItem is PXSegmentedState)
					subItem = ((PXSegmentedState)subItem).Value;

				InventoryAllocDetEnq.Redirect(
					this.ISERecords.Current.InventoryID,
					subItem != null ? (string)subItem : null,
					this.Filter.Current.ExpandByLotSerialNbr == true ?
					this.ISERecords.Current.LotSerialNbr : null,
					this.ISERecords.Current.SiteID,
					this.ISERecords.Current.LocationID);
			}
			return a.Get();
		}

		public PXAction<InventorySummaryEnqFilter> viewItem;
		[PXButton(DisplayOnMainToolbar = false)]
		[PXUIField]
		protected virtual IEnumerable ViewItem(PXAdapter a)
		{
			if (this.ISERecords.Current != null)
				InventoryItemMaint.Redirect(this.ISERecords.Current.InventoryID, true);
			return a.Get();
		}

		public static void Redirect(int? inventoryID, string subItemCD, int? siteID, int? locationID)
		{
			Redirect(inventoryID, subItemCD, siteID, locationID, true);
		}

		public static void Redirect(int? inventoryID, string subItemCD, int? siteID, int? locationID, bool newWindow)
		{
			InventorySummaryEnq graph = PXGraph.CreateInstance<InventorySummaryEnq>();

			InventoryItem item = InventoryItem.PK.Find(graph, inventoryID);
			if (item?.IsConverted == true && item.StkItem != true)
				throw new PXException(Messages.ItemHasBeenConvertedToNonStock, item.InventoryCD.Trim());

			graph.Filter.Current.InventoryID = inventoryID;
			graph.Filter.Current.SubItemCD = subItemCD;
			graph.Filter.Current.SiteID = siteID;
			graph.Filter.Current.LocationID = locationID;

			if (newWindow)
				throw new PXRedirectRequiredException(graph, true, Messages.InventorySummary) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			else
				throw new PXRedirectRequiredException(graph, Messages.InventorySummary);
		}

		#region Aging strategy
		private bool timestampSelected = false;
		private String ControlTimeStamp
		{
			get
			{
				if (!timestampSelected)
				{
					PXDatabase.SelectTimeStamp();
					timestampSelected = true;
				}
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					PXContext.SetSlot<Definition>(
					defs = PXDatabase.GetSlot<Definition>(nameof(InventorySummaryEnq) + "$ControlTimeStampDefinition",
						new Type[] /// <see cref="iSERecordsFetch"/> for proper tables
						{
							typeof(InventoryItem),
							typeof(INSubItem),
							typeof(INItemPlan),

							typeof(INSite),
							typeof(INSiteStatusByCostCenter),
							typeof(CommonSetup),

							typeof(INLocation),
							typeof(INLocationStatusByCostCenter),

							typeof(INLotSerClass),
							typeof(INLotSerialStatusByCostCenter),
							typeof(INItemLotSerial),
							typeof(INCostStatus),
							typeof(INCostSubItemXRef),
						})
					);
				}
				return defs.TimeStamp;
			}
		}
		public class Definition : IPrefetchable
		{
			public String TimeStamp { get; private set; }

			public void Prefetch()
			{
				TimeStamp = System.Text.Encoding.Default.GetString(PXDatabase.Provider.SelectTimeStamp());
			}
		}
		#endregion
	}
}
