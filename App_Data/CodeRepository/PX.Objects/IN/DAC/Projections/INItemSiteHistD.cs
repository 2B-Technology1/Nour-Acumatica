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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CM;

namespace PX.Objects.IN
{
	[INItemSiteHistDProjection]
    [PXHidden]
    public class INItemSiteHistD : IBqlTable
    {
		#region Keys
		public class PK : PrimaryKeyOf<INItemSiteHistD>.By<siteID, inventoryID, subItemID, sDate>
		{
			public static INItemSiteHistD Find(PXGraph graph, int? siteID, int? inventoryID, int? subItemID, DateTime? sDate, PKFindOptions options = PKFindOptions.None)
				=> FindBy(graph, siteID, inventoryID, subItemID, sDate, options);
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(IsKey = true, BqlField = typeof(INItemSiteHistByCostCenterD.siteID))]
		[PXDefault()]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(IsKey = true, BqlField = typeof(INItemSiteHistByCostCenterD.inventoryID))]
        [PXDefault()]
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
        [SubItem(IsKey = true, BqlField = typeof(INItemSiteHistByCostCenterD.subItemID))]
        [PXDefault()]
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
        #region SDate
        public abstract class sDate : PX.Data.BQL.BqlDateTime.Field<sDate> { }
        protected DateTime? _SDate;
        [PXDBDate(IsKey = true, BqlField = typeof(INItemSiteHistByCostCenterD.sDate))]
        public virtual DateTime? SDate
        {
            get
            {
                return this._SDate;
            }
            set
            {
                this._SDate = value;
            }
        }
        #endregion
        #region SYear
        public abstract class sYear : PX.Data.BQL.BqlInt.Field<sYear> { }
        protected int? _SYear;
        [PXDBInt(BqlField = typeof(INItemSiteHistByCostCenterD.sYear))]
        public virtual int? SYear
        {
            get
            {
                return this._SYear;
            }
            set
            {
                this._SYear = value;
            }
        }
        #endregion
        #region SMonth
        public abstract class sMonth : PX.Data.BQL.BqlInt.Field<sMonth> { }
        protected int? _SMonth;
        [PXDBInt(BqlField = typeof(INItemSiteHistByCostCenterD.sMonth))]
        public virtual int? SMonth
        {
            get
            {
                return this._SMonth;
            }
            set
            {
                this._SMonth = value;
            }
        }
        #endregion
        #region SQuater
        public abstract class sQuater : PX.Data.BQL.BqlInt.Field<sQuater> { }
        protected int? _SQuater;
        [PXDBInt(BqlField = typeof(INItemSiteHistByCostCenterD.sQuater))]
        public virtual int? SQuater
        {
            get
            {
                return this._SQuater;
            }
            set
            {
                this._SQuater = value;
            }
        }
        #endregion
        #region SDay
        public abstract class sDay : PX.Data.BQL.BqlInt.Field<sDay> { }
        protected int? _SDay;
        [PXDBInt(BqlField = typeof(INItemSiteHistByCostCenterD.sDay))]
        public virtual int? SDay
        {
            get
            {
                return this._SDay;
            }
            set
            {
                this._SDay = value;
            }
        }
        #endregion
        #region SDayOfWeek
        public abstract class sDayOfWeek : PX.Data.BQL.BqlInt.Field<sDayOfWeek> { }
        protected int? _SDayOfWeek;
        [PXDBInt(BqlField = typeof(INItemSiteHistByCostCenterD.sDayOfWeek))]
        public virtual int? SDayOfWeek
        {
            get
            {
                return this._SDayOfWeek;
            }
            set
            {
                this._SDayOfWeek = value;
            }
        }
        #endregion
        #region QtyReceived
        public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }
        protected Decimal? _QtyReceived;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyReceived))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Received")]
        public virtual Decimal? QtyReceived
        {
            get
            {
                return this._QtyReceived;
            }
            set
            {
                this._QtyReceived = value;
            }
        }
        #endregion
        #region QtyIssued
        public abstract class qtyIssued : PX.Data.BQL.BqlDecimal.Field<qtyIssued> { }
        protected Decimal? _QtyIssued;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyIssued))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Issued")]
        public virtual Decimal? QtyIssued
        {
            get
            {
                return this._QtyIssued;
            }
            set
            {
                this._QtyIssued = value;
            }
        }
        #endregion
        #region QtySales
        public abstract class qtySales : PX.Data.BQL.BqlDecimal.Field<qtySales> { }
        protected Decimal? _QtySales;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtySales))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Sales")]
        public virtual Decimal? QtySales
        {
            get
            {
                return this._QtySales;
            }
            set
            {
                this._QtySales = value;
            }
        }
        #endregion
        #region QtyCreditMemos
        public abstract class qtyCreditMemos : PX.Data.BQL.BqlDecimal.Field<qtyCreditMemos> { }
        protected Decimal? _QtyCreditMemos;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyCreditMemos))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Credit Memos")]
        public virtual Decimal? QtyCreditMemos
        {
            get
            {
                return this._QtyCreditMemos;
            }
            set
            {
                this._QtyCreditMemos = value;
            }
        }
        #endregion
        #region QtyDropShipSales
        public abstract class qtyDropShipSales : PX.Data.BQL.BqlDecimal.Field<qtyDropShipSales> { }
        protected Decimal? _QtyDropShipSales;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyDropShipSales))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Drop Ship Sales")]
        public virtual Decimal? QtyDropShipSales
        {
            get
            {
                return this._QtyDropShipSales;
            }
            set
            {
                this._QtyDropShipSales = value;
            }
        }
        #endregion
        #region QtyTransferIn
        public abstract class qtyTransferIn : PX.Data.BQL.BqlDecimal.Field<qtyTransferIn> { }
        protected Decimal? _QtyTransferIn;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyTransferIn))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transfer In")]
        public virtual Decimal? QtyTransferIn
        {
            get
            {
                return this._QtyTransferIn;
            }
            set
            {
                this._QtyTransferIn = value;
            }
        }
        #endregion
        #region QtyTransferOut
        public abstract class qtyTransferOut : PX.Data.BQL.BqlDecimal.Field<qtyTransferOut> { }
        protected Decimal? _QtyTransferOut;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyTransferOut))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transfer Out")]
        public virtual Decimal? QtyTransferOut
        {
            get
            {
                return this._QtyTransferOut;
            }
            set
            {
                this._QtyTransferOut = value;
            }
        }
        #endregion
        #region QtyAssemblyIn
        public abstract class qtyAssemblyIn : PX.Data.BQL.BqlDecimal.Field<qtyAssemblyIn> { }
        protected Decimal? _QtyAssemblyIn;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyAssemblyIn))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Assembly In")]
        public virtual Decimal? QtyAssemblyIn
        {
            get
            {
                return this._QtyAssemblyIn;
            }
            set
            {
                this._QtyAssemblyIn = value;
            }
        }
        #endregion
        #region QtyAssemblyOut
        public abstract class qtyAssemblyOut : PX.Data.BQL.BqlDecimal.Field<qtyAssemblyOut> { }
        protected Decimal? _QtyAssemblyOut;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyAssemblyOut))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Assembly Out")]
        public virtual Decimal? QtyAssemblyOut
        {
            get
            {
                return this._QtyAssemblyOut;
            }
            set
            {
                this._QtyAssemblyOut = value;
            }
        }
        #endregion
        #region QtyAdjusted
        public abstract class qtyAdjusted : PX.Data.BQL.BqlDecimal.Field<qtyAdjusted> { }
        protected Decimal? _QtyAdjusted;
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyAdjusted))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Adjusted")]
        public virtual Decimal? QtyAdjusted
        {
            get
            {
                return this._QtyAdjusted;
            }
            set
            {
                this._QtyAdjusted = value;
            }
        }
        #endregion
        #region BegQty
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.begQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beginning Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? BegQty
        {
            get;
            set;
        }
        public abstract class begQty : PX.Data.BQL.BqlDecimal.Field<begQty> { }
        #endregion
        #region EndQty
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.endQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ending Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? EndQty
        {
            get;
            set;
        }
        public abstract class endQty : PX.Data.BQL.BqlDecimal.Field<endQty> { }
        #endregion
        #region QtyDebit
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyDebit))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Debit Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? QtyDebit
        {
            get;
            set;
        }
        public abstract class qtyDebit : PX.Data.BQL.BqlDecimal.Field<qtyDebit> { }
        #endregion
        #region QtyCredit
        [PXDBQuantity(BqlField = typeof(INItemSiteHistByCostCenterD.qtyCredit))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Credit Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? QtyCredit
        {
            get;
            set;
        }
        public abstract class qtyCredit : PX.Data.BQL.BqlDecimal.Field<qtyCredit> { }
        #endregion
        #region CostDebit
        [PXDBBaseCury(BqlField = typeof(INItemSiteHistByCostCenterD.costDebit))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Debit Cost", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? CostDebit
        {
            get;
            set;
        }
        public abstract class costDebit : PX.Data.BQL.BqlDecimal.Field<costDebit> { }
        #endregion
        #region CostCredit
        [PXDBBaseCury(BqlField = typeof(INItemSiteHistByCostCenterD.costCredit))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Credit Cost", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual decimal? CostCredit
        {
            get;
            set;
        }
        public abstract class costCredit : PX.Data.BQL.BqlDecimal.Field<costCredit> { }
        #endregion
    }
}
