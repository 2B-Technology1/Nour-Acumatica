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
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.AP
{
	/// exclude
	[PXUIField(DisplayName = "Pay-to Vendor", FieldClass = nameof(FeaturesSet.VendorRelations))]
	[PXRestrictor(
		typeof(Where<Vendor.vStatus, Equal<VendorStatus.active>,
			Or<Vendor.vStatus, Equal<VendorStatus.oneTime>,
			Or<Vendor.vStatus, Equal<VendorStatus.holdPayments>>>>),
		Messages.VendorIsInStatus,
		typeof(Vendor.vStatus))]
	[PXRestrictor(typeof(Where<Vendor.type, NotEqual<BAccountType.employeeType>>), Messages.VendorCannotBe,
		typeof(Vendor.type))]
	public class BasePayToVendorAttribute : VendorAttribute
	{
		public BasePayToVendorAttribute(){}
		public BasePayToVendorAttribute(Type search) : base(search) {}
	}

	/// <summary>
	/// This is a specialized version of the <see cref=VendorAttribute/>.<br/>
	/// Displays only Pay-to vendors for Vendors form<br/>
	/// </summary>
	[PXRestrictor(
		typeof(Where<Vendor.payToVendorID, IsNull>),
		Messages.SuppliedByVendorNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(typeof(Where<Vendor.taxAgency, NotEqual<True>>), Messages.TaxAgencyNotAllowedInPayTo, typeof(Vendor.acctCD))]
	[PXRestrictor(typeof(Where<Vendor.isLaborUnion, NotEqual<True>>), Messages.LaborUnionNotAllowedInPayTo, typeof(Vendor.acctCD))]
	[PXRestrictor(typeof(Where<Vendor.vendor1099, NotEqual<True>>), Messages.Vendor1099NotAllowedInPayTo, typeof(Vendor.acctCD))]
	[PXRestrictor(typeof(Where<Vendor.bAccountID, NotEqual<Optional<Vendor.bAccountID>>>), Messages.SameVendorNotAllowedInPayTo, typeof(Vendor.acctCD))]
	public class PayToVendorAttribute : BasePayToVendorAttribute
	{
		public PayToVendorAttribute() {}
		public PayToVendorAttribute(Type search) : base(search) {}
	}

	/// <summary>
	/// This is a specialized version of the <see cref=VendorAttribute/>.<br/>
	/// Displays only Pay-to vendors and PO document vendor and allowed all vendors for transfer receipts<br/>
	/// </summary>
	[PXRestrictor(
		typeof(Where<Vendor.payToVendorID, IsNull,
			Or<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>),
		Messages.SuppliedByVendorNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.taxAgency, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>),
		Messages.TaxAgencyNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.isLaborUnion, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>),
		Messages.LaborUnionNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.vendor1099, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POOrder.vendorID>>>>),
		Messages.Vendor1099NotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	public class POOrderPayToVendorAttribute : BasePayToVendorAttribute
	{
	}

	/// <summary>
	/// This is a specialized version of the <see cref=VendorAttribute/>.<br/>
	/// Displays only Pay-to vendors and PO document vendor and allowed all vendors for transfer receipts<br/>
	/// </summary>
	[PXRestrictor(
		typeof(Where<Vendor.payToVendorID, IsNull,
			Or<Vendor.bAccountID, Equal<Current<POReceipt.vendorID>>>>),
		Messages.SuppliedByVendorNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.taxAgency, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POReceipt.vendorID>>>>),
		Messages.TaxAgencyNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.isLaborUnion, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POReceipt.vendorID>>>>),
		Messages.LaborUnionNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.vendor1099, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POReceipt.vendorID>>>>),
		Messages.Vendor1099NotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[VerndorNonEmployeeOrOrganizationRestrictor]
	public class POReceiptPayToVendorAttribute : BasePayToVendorAttribute
	{
		public POReceiptPayToVendorAttribute() 
			: base(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>)) // TODO: remove fake Where after AC-101187
		{
		}
	}
}
