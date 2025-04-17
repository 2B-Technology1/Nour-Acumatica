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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.PO.LandedCosts.Attributes
{
	/// <summary>
	/// This is a specialized version of the <see cref=VendorAttribute/>.<br/>
	/// Displays only Pay-to vendors and PO document vendor and allowed all vendors for transfer receipts<br/>
	/// </summary>
	[PXRestrictor(
		typeof(Where<Vendor.payToVendorID, IsNull,
			Or<Vendor.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>),
		AP.Messages.SuppliedByVendorNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.taxAgency, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>),
		AP.Messages.TaxAgencyNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.isLaborUnion, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>),
		AP.Messages.LaborUnionNotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[PXRestrictor(
		typeof(Where<Vendor.vendor1099, NotEqual<True>,
			Or<Vendor.bAccountID, Equal<Current<POLandedCostDoc.vendorID>>>>),
		AP.Messages.Vendor1099NotAllowedInPayTo,
		typeof(Vendor.acctCD))]
	[VerndorNonEmployeeOrOrganizationRestrictor]
	public class POLandedCostPayToVendorAttribute : BasePayToVendorAttribute
	{
		public POLandedCostPayToVendorAttribute()
			: base(typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>)) // TODO: remove fake Where after AC-101187
		{
		}
	}
}
