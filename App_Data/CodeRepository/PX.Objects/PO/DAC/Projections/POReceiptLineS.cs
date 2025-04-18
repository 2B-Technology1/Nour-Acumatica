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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	/// <summary>
	/// Read-only class for selector
	/// </summary>
	[PXProjection(typeof(Select2<POReceiptLine,
		LeftJoin<POLine, On<POLine.orderType, Equal<POReceiptLine.pOType>, And<POLine.orderNbr, Equal<POReceiptLine.pONbr>, And<POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
		LeftJoin<POOrder, On<POOrder.orderType, Equal<POReceiptLine.pOType>, And<POOrder.orderNbr, Equal<POReceiptLine.pONbr>>>,
		LeftJoin<POReceipt, On<POReceiptLine.FK.Receipt>>>>>),
		Persistent = false)]
	[PXCacheName(Messages.POReceiptLine)]
	[Serializable]
	public partial class POReceiptLineS : IBqlTable, IAPTranSource, ISortOrder
	{
		#region Keys
		public class PK : PrimaryKeyOf<POReceiptLineS>.By<receiptType, receiptNbr, lineNbr>
		{
			public static POReceiptLineS Find(PXGraph graph, string receiptType, string receiptNbr, int? lineNbr, PKFindOptions options = PKFindOptions.None) => FindBy(graph, receiptType, receiptNbr, lineNbr, options);
		}
		public static class FK
		{
			public class Branch : GL.Branch.PK.ForeignKeyOf<POReceiptLineS>.By<branchID> { }
			public class Receipt : POReceipt.PK.ForeignKeyOf<POReceiptLineS>.By<receiptType, receiptNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POReceiptLineS>.By<inventoryID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<POReceiptLineS>.By<vendorID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<POReceiptLineS>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<POReceiptLineS>.By<siteID> { }
			public class Currency : CM.Currency.PK.ForeignKeyOf<POReceiptLineS>.By<curyID> { }
			public class OrderCurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<POReceiptLineS>.By<orderCuryInfoID> { }
			public class ReceiptCurrencyInfo : CM.CurrencyInfo.PK.ForeignKeyOf<POReceiptLineS>.By<receiptCuryInfoID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<POReceiptLineS>.By<taxCategoryID> { }
			public class ExpenseAccount : GL.Account.PK.ForeignKeyOf<POReceiptLineS>.By<expenseAcctID> { }
			public class ExpenseSubaccount : Sub.PK.ForeignKeyOf<POReceiptLineS>.By<expenseSubID> { }
			public class AccrualAccount : GL.Account.PK.ForeignKeyOf<POReceiptLineS>.By<pOAccrualAcctID> { }
			public class AccrualSubaccount : Sub.PK.ForeignKeyOf<POReceiptLineS>.By<pOAccrualSubID> { }
			public class Tax : TX.Tax.PK.ForeignKeyOf<POReceiptLineS>.By<taxID> { }
			public class Project : PM.PMProject.PK.ForeignKeyOf<POReceiptLineS>.By<projectID> { }
			public class Task : PM.PMTask.PK.ForeignKeyOf<POReceiptLineS>.By<projectID, taskID> { }
			public class CostCode : PM.PMCostCode.PK.ForeignKeyOf<POReceiptLineS>.By<costCodeID> { }
			public class AccrualStatus : POAccrualStatus.PK.ForeignKeyOf<POReceiptLineS>.By<pOAccrualRefNoteID, pOAccrualLineNbr, pOAccrualType> { }
			public class Order : POOrder.PK.ForeignKeyOf<POReceiptLineS>.By<pOType, pONbr> { }
			public class OrderLine : POLine.PK.ForeignKeyOf<POReceiptLineS>.By<pOType, pONbr, pOLineNbr> { }
			public class Discount : APDiscount.PK.ForeignKeyOf<POReceiptLineS>.By<discountID, vendorID> { }
			public class DiscountSequence : VendorDiscountSequence.PK.ForeignKeyOf<POReceiptLineS>.By<vendorID, discountID, discountSequenceID> { }

			//todo public class UnitOfMeasure : INUnit.UK.ByInventory.ForeignKeyOf<POReceiptLineS>.By<inventoryID, uOM> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
		}
		protected Int32? _BranchID;
		[Branch(BqlField = typeof(POReceiptLine.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		[PXDBString(2, IsFixed = true, BqlField = typeof(POLine.orderType))]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXUIField(DisplayName = "Receipt Nbr.")]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, IsKey = true, BqlField = typeof(POReceiptLine.receiptType))]
		[PXUIField(DisplayName = "Receipt Type")]
		public virtual String ReceiptType
		{
			get
			{
				return this._ReceiptType;
			}
			set
			{
				this._ReceiptType = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
		{
		}
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(POReceiptLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.lineType))]
		[POLineType.List()]
		[PXUIField(DisplayName = "Line Type")]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
		[PXDBBool(BqlField = typeof(POReceiptLine.isStockItem))]
		public virtual bool? IsStockItem
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
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
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost>
		{
		}
		[PXDBBool(BqlField = typeof(POReceiptLine.accrueCost))]
		public virtual bool? AccrueCost
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}
		protected Int32? _VendorID;
		[Vendor(BqlField = typeof(POReceiptLine.vendorID), Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate>
		{
		}

		protected DateTime? _ReceiptDate;
		[PXDBDate(BqlField = typeof(POReceiptLine.receiptDate))]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
		{
		}
		protected Int32? _SubItemID;
		[SubItem(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.subItemID))]
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
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		protected Int32? _SiteID;
		[IN.SiteAvail(typeof(POReceiptLineS.inventoryID), typeof(POReceiptLineS.subItemID), typeof(POReceiptLineS.costCenterID), BqlField = typeof(POReceiptLine.siteID))]
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
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;
		[INUnit(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.uOM))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region ReceiptQty
		public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty>
		{
		}

		protected Decimal? _ReceiptQty;
		[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseReceiptQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.receiptQty))]
		[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReceiptQty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}

		#endregion
		#region BaseReceiptQty
		public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty>
		{
		}

		protected Decimal? _BaseReceiptQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseReceiptQty))]
		public virtual Decimal? BaseReceiptQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Order Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region ReturnInventoryCostMode
		public abstract class returnInventoryCostMode : PX.Data.BQL.BqlString.Field<returnInventoryCostMode> { }

		/// <inheritdoc cref="POReceipt.ReturnInventoryCostMode"/>
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later [Justification: false alert, see ATR-741]
		[PXDBString(1, IsFixed = true, BqlField = typeof(POReceipt.returnInventoryCostMode))]
		[PXUIField(DisplayName = "Cost of Inventory Return From", Visibility = PXUIVisibility.Visible)]
		public virtual String ReturnInventoryCostMode
		{
			get;
			set;
		}
		#endregion
		#region OrderCuryInfoID
		public abstract class orderCuryInfoID : PX.Data.BQL.BqlLong.Field<orderCuryInfoID>
		{
		}
		protected Int64? _OrderCuryInfoID;
		[PXDBLong(BqlField = typeof(POLine.curyInfoID))]
		public virtual Int64? OrderCuryInfoID
		{
			get
			{
				return this._OrderCuryInfoID;
			}
			set
			{
				this._OrderCuryInfoID = value;
			}
		}
		#endregion
		#region ReceiptCuryInfoID
		public abstract class receiptCuryInfoID : PX.Data.BQL.BqlLong.Field<receiptCuryInfoID>
		{
		}
		protected Int64? _ReceiptCuryInfoID;
		[PXDBLong(BqlField = typeof(POReceiptLine.curyInfoID))]
		public virtual Int64? ReceiptCuryInfoID
		{
			get
			{
				return this._ReceiptCuryInfoID;
			}
			set
			{
				this._ReceiptCuryInfoID = value;
			}
		}
		#endregion
		#region CuryOrderUnitCost
		public abstract class curyOrderUnitCost : PX.Data.BQL.BqlDecimal.Field<curyOrderUnitCost>
		{
		}
		protected Decimal? _CuryOrderUnitCost;

		[PXDBDecimal(6, BqlField = typeof(POLine.curyUnitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryOrderUnitCost
		{
			get
			{
				return this._CuryOrderUnitCost;
			}
			set
			{
				this._CuryOrderUnitCost = value;
			}
		}
		#endregion
		#region OrderUnitCost
		public abstract class orderUnitCost : PX.Data.BQL.BqlDecimal.Field<orderUnitCost>
		{
		}
		protected Decimal? _OrderUnitCost;

		[PXDBDecimal(6, BqlField = typeof(POLine.unitCost))]
		public virtual Decimal? OrderUnitCost
		{
			get
			{
				return this._OrderUnitCost;
			}
			set
			{
				this._OrderUnitCost = value;
			}
		}
		#endregion
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost>
		{
		}
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(POReceiptLineS.orderCuryInfoID), typeof(POReceiptLineS.extCost), BqlField = typeof(POLine.curyExtCost))]
		[PXUIField(DisplayName = "Order Line Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryExtCost
		{
			get
			{
				return this._CuryExtCost;
			}
			set
			{
				this._CuryExtCost = value;
			}
		}
		#endregion
		#region ExtCost
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost>
		{
		}
		protected Decimal? _ExtCost;
		[PXDBBaseCury(BqlField = typeof(POLine.extCost))]
		public virtual Decimal? ExtCost
		{
			get
			{
				return this._ExtCost;
			}
			set
			{
				this._ExtCost = value;
			}
		}
		#endregion
		#region CuryOrderLineAmt
		public abstract class curyOrderLineAmt : PX.Data.BQL.BqlDecimal.Field<curyOrderLineAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.orderCuryInfoID), typeof(POReceiptLineS.orderLineAmt), BqlField = typeof(POLine.curyLineAmt))]
		public virtual decimal? CuryOrderLineAmt
		{
			get;
			set;
		}
		#endregion
		#region OrderLineAmt
		public abstract class orderLineAmt : PX.Data.BQL.BqlDecimal.Field<orderLineAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.lineAmt))]
		public virtual decimal? OrderLineAmt
		{
			get;
			set;
		}
		#endregion
		#region OrderDiscPct
		public abstract class orderDiscPct : PX.Data.BQL.BqlDecimal.Field<orderDiscPct>
		{
		}
		protected Decimal? _OrderDiscPct;
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(POLine.discPct))]
		[PXUIField(DisplayName = "Discount Percent")]
		public virtual Decimal? OrderDiscPct
		{
			get
			{
				return this._OrderDiscPct;
			}
			set
			{
				this._OrderDiscPct = value;
			}
		}
		#endregion
		#region CompletePOLine
		public abstract class completePOLine : PX.Data.BQL.BqlString.Field<completePOLine> { }
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLine.completePOLine))]
		public virtual String CompletePOLine
		{
			get;
			set;
		}
		#endregion

		#region ReceiptDiscPct
		public abstract class receiptDiscPct : PX.Data.BQL.BqlDecimal.Field<receiptDiscPct>
		{
		}
		protected Decimal? _ReceiptDiscPct;
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(POReceiptLine.discPct))]
		[PXUIField(DisplayName = "Discount Percent")]
		public virtual Decimal? ReceiptDiscPct
		{
			get
			{
				return this._ReceiptDiscPct;
			}
			set
			{
				this._ReceiptDiscPct = value;
			}
		}
		#endregion

		#region CuryOrderDiscAmt
		public abstract class curyOrderDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrderDiscAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.orderCuryInfoID), typeof(POReceiptLineS.orderDiscAmt), BqlField = typeof(POLine.curyDiscAmt))]
		public virtual decimal? CuryOrderDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region OrderDiscAmt
		public abstract class orderDiscAmt : PX.Data.BQL.BqlDecimal.Field<orderDiscAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.discAmt))]
		public virtual decimal? OrderDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryReceiptDiscAmt
		public abstract class curyReceiptDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyReceiptDiscAmt>
		{
		}
		protected Decimal? _CuryReceiptDiscAmt;
		[PXDBCurrency(typeof(POReceiptLineS.receiptCuryInfoID), typeof(POReceiptLineS.receiptDiscAmt), BqlField = typeof(POReceiptLine.curyDiscAmt))]
		public virtual decimal? CuryReceiptDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region ReceiptDiscAmt
		public abstract class receiptDiscAmt : PX.Data.BQL.BqlDecimal.Field<receiptDiscAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.discAmt))]
		public virtual decimal? ReceiptDiscAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryReceiptUnitCost
		public abstract class curyReceiptUnitCost : PX.Data.BQL.BqlDecimal.Field<curyReceiptUnitCost>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.curyUnitCost))]
		[PXUIField(DisplayName = "Receipt Unit Cost", Visibility = PXUIVisibility.SelectorVisible)] //TODO: check naming
		public virtual decimal? CuryReceiptUnitCost
		{
			get;
			set;
		}
		#endregion
		#region ReceiptUnitCost
		public abstract class receiptUnitCost : PX.Data.BQL.BqlDecimal.Field<receiptUnitCost>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.unitCost))]
		public virtual decimal? ReceiptUnitCost
		{
			get;
			set;
		}
		#endregion
		#region CuryReceiptExtCost
		public abstract class curyReceiptExtCost : PX.Data.BQL.BqlDecimal.Field<curyReceiptExtCost>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.curyExtCost))]
		public virtual decimal? CuryReceiptExtCost
		{
			get;
			set;
		}
		#endregion
		#region ReceiptExtCost
		public abstract class receiptExtCost : PX.Data.BQL.BqlDecimal.Field<receiptExtCost>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.extCost))]
		public virtual decimal? ReceiptExtCost
		{
			get;
			set;
		}
		#endregion

		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty>
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.unbilledQty))]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty>
		{
		}
		protected Decimal? _BaseUnbilledQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion
		
		#region OrderUOM
		public abstract class orderUOM : PX.Data.BQL.BqlString.Field<orderUOM>
		{
		}
		[INUnit(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POLine.uOM))]
		public virtual string OrderUOM
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty>
		{
		}
		[PXDBQuantity(typeof(POReceiptLineS.orderUOM), typeof(POReceiptLineS.baseOrderQty), BqlField = typeof(POLine.orderQty))]
		[PXUIField(DisplayName = "Ordered Qty.")]
		public virtual decimal? OrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOrderQty))]
		public virtual decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100, BqlField = typeof(POLine.retainagePct))]
		[PXUIField(DisplayName = "Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageAmt
		public abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.orderCuryInfoID), typeof(POReceiptLineS.retainageAmt), BqlField = typeof(POLine.curyRetainageAmt))]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmt
		public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.retainageAmt))]
		public virtual decimal? RetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt>
		{
		}
		[PXDBCurrency(typeof(orderCuryInfoID), typeof(unbilledAmt), BqlField = typeof(POLine.curyUnbilledAmt))]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual decimal? CuryUnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt>
		{
		}

		[PXDBBaseCury(BqlField = typeof(POLine.unbilledAmt))]
		public virtual decimal? UnbilledAmt
		{
			get;
			set;
		}
		#endregion

		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate>
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(POLine.groupDiscountRate))]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region DocumentDiscountRate
		public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate>
		{
		}
		protected Decimal? _DocumentDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(POLine.documentDiscountRate))]
		public virtual Decimal? DocumentDiscountRate
		{
			get
			{
				return this._DocumentDiscountRate;
			}
			set
			{
				this._DocumentDiscountRate = value;
			}
		}
		#endregion

		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID>
		{
		}
		protected String _TaxCategoryID;

		[PXDBString(TX.TaxCategory.taxCategoryID.Length, IsUnicode = true, BqlField = typeof(POLine.taxCategoryID))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID>
		{
		}
		protected Int32? _ExpenseAcctID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseAcctID))]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID>
		{
		}
		protected Int32? _ExpenseSubID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseSubID))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID>
		{
		}
		protected Int32? _POAccrualAcctID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualAcctID))]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID>
		{
		}
		protected Int32? _POAccrualSubID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualSubID))]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc>
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POReceiptLine.tranDesc))]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID>
		{
		}
		protected String _TaxID;
		[PXDBString(TX.Tax.taxID.Length, IsUnicode = true, BqlField = typeof(POLine.taxID))]
		[PXUIField(DisplayName = "Tax ID", Visible = false)]
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected int? _ProjectID;
		[PXDBInt(BqlField = typeof(POReceiptLine.projectID))]
		public virtual int? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID>
		{
		}
		protected int? _TaskID;
		[PXDBInt(BqlField = typeof(POReceiptLine.taskID))]
		public virtual int? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		[PXDBInt(BqlField = typeof(POReceiptLine.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualType
		public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType>
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(POReceiptLine.pOAccrualType))]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region POAccrualRefNoteID
		public abstract class pOAccrualRefNoteID : PX.Data.BQL.BqlGuid.Field<pOAccrualRefNoteID>
		{
		}
		[PXDBGuid(BqlField = typeof(POReceiptLine.pOAccrualRefNoteID))]
		public virtual Guid? POAccrualRefNoteID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualLineNbr
		public abstract class pOAccrualLineNbr : PX.Data.BQL.BqlInt.Field<pOAccrualLineNbr>
		{
		}
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualLineNbr))]
		public virtual int? POAccrualLineNbr
		{
			get;
			set;
		}
		#endregion

		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType>
		{
		}
		protected String _POType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr>
		{
		}
		protected Int32? _POLineNbr;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOLineNbr))]
		[PXUIField(DisplayName = "PO Line Nbr.")]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion

		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID>
		{
		}
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.discountID))]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
		public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID>
		{
		}
		protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.discountSequenceID))]
		public virtual String DiscountSequenceID
		{
			get
			{
				return this._DiscountSequenceID;
			}
			set
			{
				this._DiscountSequenceID = value;
			}
		}
		#endregion

		#region AllowEditUnitCost
		public abstract class allowEditUnitCost : PX.Data.BQL.BqlBool.Field<allowEditUnitCost>
		{
		}
		[PXDBBool(BqlField = typeof(POReceiptLine.allowEditUnitCost))]
		public virtual bool? AllowEditUnitCost
		{
			get;
			set;
		}
		#endregion

		#region DRTermStartDate
		public abstract class dRTermStartDate : PX.Data.BQL.BqlDateTime.Field<dRTermStartDate> { }

		protected DateTime? _DRTermStartDate;

		[PXDBDate(BqlField = typeof(POLine.dRTermStartDate))]
		public DateTime? DRTermStartDate
		{
			get { return _DRTermStartDate; }
			set { _DRTermStartDate = value; }
		}
		#endregion
		#region DRTermEndDate
		public abstract class dRTermEndDate : PX.Data.BQL.BqlDateTime.Field<dRTermEndDate> { }

		protected DateTime? _DRTermEndDate;

		[PXDBDate(BqlField = typeof(POLine.dRTermEndDate))]
		public DateTime? DRTermEndDate
		{
			get { return _DRTermEndDate; }
			set { _DRTermEndDate = value; }
		}
		#endregion

		#region DropshipExpenseRecording
		public abstract class dropshipExpenseRecording : PX.Data.BQL.BqlString.Field<dropshipExpenseRecording> { }
		[PXDBString(1, BqlField = typeof(POReceiptLine.dropshipExpenseRecording))]
		public virtual String DropshipExpenseRecording
		{
			get;
			set;
		}
		#endregion

		#region CostCenterID
		public abstract class costCenterID : PX.Data.BQL.BqlInt.Field<costCenterID>
		{
		}
		[PXDBInt(BqlField = typeof(POReceiptLine.costCenterID))]
		public virtual int? CostCenterID
		{
			get;
			set;
		}
		#endregion

		#region IAPTranSource Members

		protected bool IsDirectReceipt
		{
			get { return this.OrderQty == null; }
		}

		protected bool UseAmountsFromReceipt
		{
			get { return this.AllowEditUnitCost == true; }
		}

		string IAPTranSource.OrigUOM
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.UOM : this.OrderUOM;
			}
		}

		public virtual decimal? OrigQty
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptQty : this.OrderQty;
			}
		}

		public virtual decimal? BaseOrigQty
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.BaseReceiptQty : this.BaseOrderQty;
			}
		}

		decimal? IAPTranSource.BillQty
		{
			get
			{
				return this.UnbilledQty;
			}
		}

		decimal? IAPTranSource.BaseBillQty
		{
			get
			{
				return this.BaseUnbilledQty;
			}
		}

		Int64? IAPTranSource.CuryInfoID
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptCuryInfoID : this.OrderCuryInfoID;
			}
		}

		decimal? IAPTranSource.DiscPct
		{
			get
			{
				return UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptDiscPct : this.OrderDiscPct;
			}
		}

		decimal? IAPTranSource.CuryDiscAmt
		{
			get
			{
				return UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.CuryReceiptDiscAmt : this.CuryOrderDiscAmt;
			}
		}

		decimal? IAPTranSource.DiscAmt
		{
			get
			{
				return UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptDiscAmt : this.OrderDiscAmt;
			}
		}

		decimal? IAPTranSource.CuryLineAmt
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.CuryReceiptExtCost : this.CuryOrderLineAmt;
			}
		}

		decimal? IAPTranSource.LineAmt
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptExtCost : this.OrderLineAmt;
			}
		}

		decimal? IAPTranSource.CuryUnitCost
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.CuryReceiptUnitCost : this.CuryOrderUnitCost;
			}
		}

		decimal? IAPTranSource.UnitCost
		{
			get
			{
				return this.IsDirectReceipt || UseAmountsFromReceipt || this.ReturnInventoryCostMode == ReturnCostMode.OriginalCost ? this.ReceiptUnitCost : this.OrderUnitCost;
			}
		}

		bool IAPTranSource.IsReturn
		{
			get
			{
				return this.ReceiptType == POReceiptType.POReturn;
			}
		}

		bool IAPTranSource.IsPartiallyBilled
		{
			get
			{
				return this.BaseOrigQty != this.BaseUnbilledQty;
			}
		}

		bool IAPTranSource.AggregateWithExistingTran
		{
			get
			{
				return this.POAccrualType == Objects.PO.POAccrualType.Order;
			}
		}

		public virtual decimal? CuryBilledAmt
		{
			get;
			set;
		}

		public virtual decimal? BilledAmt
		{
			get;
			set;
		}

		public virtual bool CompareReferenceKey(APTran aTran)
		{
			return aTran.POAccrualType == this.POAccrualType
				&& aTran.POAccrualRefNoteID == this.POAccrualRefNoteID
				&& aTran.POAccrualLineNbr == this.POAccrualLineNbr;
		}
		public virtual void SetReferenceKeyTo(APTran aTran)
		{
			bool orderPOAccrual = (this.POAccrualType == Objects.PO.POAccrualType.Order);
			aTran.POAccrualType = this.POAccrualType;
			aTran.POAccrualRefNoteID = this.POAccrualRefNoteID;
			aTran.POAccrualLineNbr = this.POAccrualLineNbr;
			aTran.ReceiptType = orderPOAccrual ? null : this.ReceiptType;
			aTran.ReceiptNbr = orderPOAccrual ? null : this.ReceiptNbr;
			aTran.ReceiptLineNbr = orderPOAccrual ? null : this.LineNbr;
			//TODO: move to IAPTranSource, current implementation to preserve exact behavior when SubItem was shown from joined POReceiptLine
			aTran.SubItemID = orderPOAccrual ? null : this.SubItemID;
			aTran.POOrderType = this.POType;
			aTran.PONbr = this.PONbr;
			aTran.POLineNbr = this.POLineNbr;
		}

		#endregion
	}
}
