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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;

namespace PX.Objects.IN
{
	/// <summary>
	/// The DAC is used as a filter in Intercompany Goods in Transit Generic Inquiry
	/// </summary>
	[PXCacheName(Messages.IntercompanyGoodsInTransitFilter)]
	public class IntercompanyGoodsInTransitFilter : IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : Data.BQL.BqlInt.Field<inventoryID> { }
		[StockItem(DisplayName = "Inventory Item")]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region ShippedBefore
		public abstract class shippedBefore : Data.BQL.BqlDateTime.Field<shippedBefore> { }
		[PXDate]
		[PXUIField(DisplayName = "Shipped Before")]
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? ShippedBefore
		{
			get;
			set;
		}
		#endregion
		#region ShowOverdueItems
		public abstract class showOverdueItems : Data.BQL.BqlBool.Field<showOverdueItems> { }
		[PXBool]
		[PXUIField(DisplayName = "Show Only Overdue Items")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? ShowOverdueItems
		{
			get;
			set;
		}
		#endregion
		#region ShowItemsWithoutReceipt
		public abstract class showItemsWithoutReceipt : Data.BQL.BqlBool.Field<showItemsWithoutReceipt> { }
		[PXBool]
		[PXUIField(DisplayName = "Show Only Items Without Receipt")]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? ShowItemsWithoutReceipt
		{
			get;
			set;
		}
		#endregion

		#region SellingCompany 
		public abstract class sellingCompany : Data.BQL.BqlInt.Field<sellingCompany> { }
		[VendorActive(DisplayName = SO.Messages.SellingCompany, Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Vendor.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Vendor.isBranch, Equal<True>>), SO.Messages.VendorIsNotBranch, typeof(Vendor.acctCD))]
		public virtual int? SellingCompany
		{
			get;
			set;
		}
		#endregion
		#region SellingSiteID
		public abstract class sellingSiteID : Data.BQL.BqlInt.Field<sellingSiteID> { }
		[Site(DisplayName = "Selling Warehouse")]
		public virtual int? SellingSiteID
		{
			get;
			set;
		}
		#endregion

		#region PurchasingCompany
		public abstract class purchasingCompany : Data.BQL.BqlInt.Field<purchasingCompany> { }
		[CustomerActive(DisplayName = SO.Messages.PurchasingCompany, Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Customer.isBranch, Equal<True>>), SO.Messages.CustomerIsNotBranch, typeof(Customer.acctCD))]
		public virtual int? PurchasingCompany
		{
			get;
			set;
		}
		#endregion
		#region PurchasingSiteID
		public abstract class purchasingSiteID : Data.BQL.BqlInt.Field<purchasingSiteID> { }
		[Site(DisplayName = "Purchasing Warehouse")]
		public virtual int? PurchasingSiteID
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
}
