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
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PXCacheName(Messages.SOForPurchaseReceiptFilter)]
	public partial class SOForPurchaseReceiptFilter : IBqlTable
	{
		#region DocDate
		public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
		[PXDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region PurchasingCompany
		public abstract class purchasingCompany : PX.Data.BQL.BqlInt.Field<purchasingCompany> { }
		[CustomerActive(DisplayName = SO.Messages.PurchasingCompany, Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Customer.isBranch, Equal<True>>), SO.Messages.CustomerIsNotBranch, typeof(Customer.acctCD))]
		public virtual int? PurchasingCompany { get; set; }
		#endregion

		#region SellingCompany 
		public abstract class sellingCompany : PX.Data.BQL.BqlInt.Field<sellingCompany> { }
		[VendorActive(DisplayName = SO.Messages.SellingCompany, Visibility = PXUIVisibility.SelectorVisible, Required = false, DescriptionField = typeof(Vendor.acctName), Filterable = true)]
		[PXRestrictor(typeof(Where<Vendor.isBranch, Equal<True>>), SO.Messages.VendorIsNotBranch, typeof(Vendor.acctCD))]
		public virtual int? SellingCompany { get; set; }
		#endregion

		#region PutReceiptsOnHold
		public abstract class putReceiptsOnHold : PX.Data.BQL.BqlBool.Field<putReceiptsOnHold> { }
		[PXBool()]
		[PXUIField(DisplayName = "Put Created Receipts on Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? PutReceiptsOnHold { get; set; }
		#endregion
	}
}
