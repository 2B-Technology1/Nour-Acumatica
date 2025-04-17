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
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.SO.DAC.Projections
{
	[PXProjection(typeof(Select2<SOLine,
		InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOLine.orderType>, And<SOOrder.orderNbr, Equal<SOLine.orderNbr>>>,
		InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>>>,
		Where<SOOrderType.requireShipping, Equal<True>, And<SOOrderType.aRDocType, NotEqual<ARDocType.noUpdate>,
			And2<Where<SOOrder.isLegacyMiscBilling, Equal<True>,
					And<Sub<SOLine.baseOrderQty, SOLine.baseShippedQty>, Greater<decimal0>,
				Or<SOOrder.isLegacyMiscBilling, Equal<False>,
					And<Where<SOLine.operation, Equal<SOLine.defaultOperation>,
						And<SOLine.unbilledQty, Greater<decimal0>,
					Or<SOLine.operation, NotEqual<SOLine.defaultOperation>,
						And<SOLine.unbilledQty, Less<decimal0>>>>>>>>>,
			And<SOLine.pOCreate, Equal<False>, And<SOLine.completed, Equal<False>>>>>>,
		OrderBy<Desc<SOLine.orderNbr>>>),
		Persistent = false)]
	[PXCacheName(Messages.SOLine)]
	[Serializable]
	public class SOLineForDirectInvoice : IBqlTable
	{
		#region Selected
		public abstract class selected : Data.BQL.BqlBool.Field<selected>
		{
		}
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get;
			set;
		}
		#endregion
		#region OrderType
		public abstract class orderType : Data.BQL.BqlString.Field<orderType>
		{
		}
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>))]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : Data.BQL.BqlString.Field<orderNbr>
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(SOLine.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		[PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Enabled = false)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region CustomerID
		public abstract class customerID : Data.BQL.BqlInt.Field<customerID>
		{
		}
		[Customer(Enabled = false, BqlField = typeof(SOOrder.customerID))]
		public virtual int? CustomerID
		{
			get;
			set;
		}
		#endregion
		#region Operation
		public abstract class operation : Data.BQL.BqlString.Field<operation>
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.operation))]
		[PXUIField(DisplayName = "Operation", Enabled = false)]
		[SOOperation.List]
		public virtual string Operation
		{
			get;
			set;
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : Data.BQL.BqlDateTime.Field<shipDate>
		{
		}
		[PXDBDate(BqlField = typeof(SOLine.shipDate))]
		[PXUIField(DisplayName = "Ship On", Enabled = false)]
		public virtual DateTime? ShipDate
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		[SOLineInventoryItem(Enabled = false, BqlField = typeof(SOLine.inventoryID))]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region UOM
		public abstract class uOM : Data.BQL.BqlString.Field<uOM>
		{
		}
		[INUnit(typeof(SOLine.inventoryID), DisplayName = "UOM", Enabled = false, BqlField = typeof(SOLine.uOM))]
		public virtual string UOM
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : Data.BQL.BqlDecimal.Field<orderQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(SOLine.orderQty))]
		[PXUIField(DisplayName = "Order Qty.", Enabled = false)]
		public virtual decimal? OrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : Data.BQL.BqlDecimal.Field<baseOrderQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(SOLine.baseOrderQty))]
		public virtual decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : Data.BQL.BqlDecimal.Field<shippedQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(SOLine.shippedQty))]
		[PXUIField(DisplayName = "Qty. on Shipments", Enabled = false)]
		public virtual decimal? ShippedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : Data.BQL.BqlDecimal.Field<baseShippedQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(SOLine.baseShippedQty))]
		public virtual decimal? BaseShippedQty
		{
			get;
			set;
		}
		#endregion
		#region Completed
		public abstract class completed : Data.BQL.BqlBool.Field<completed>
		{
		}
		[PXDBBool(BqlField = typeof(SOLine.completed))]
		[PXUIField(DisplayName = "Completed", Enabled = false)]
		public virtual bool? Completed
		{
			get;
			set;
		}
		#endregion
	}
}
