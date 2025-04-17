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
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.PO;
using System;

namespace PX.Objects.AP
{
	public class VerndorNonEmployeeOrOrganizationRestrictorAttribute : PXRestrictorAttribute
	{
		public VerndorNonEmployeeOrOrganizationRestrictorAttribute() :
			base(typeof(Where<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationType, BAccountType.vendorType, BAccountType.combinedType>>),
			Messages.VendorNonEmployeeOrOrganization)
		{
		}

		public VerndorNonEmployeeOrOrganizationRestrictorAttribute(Type receiptType) :
			base(BqlTemplate.OfCondition<
				Where<Current<BqlPlaceholder.A>, NotEqual<POReceiptType.transferreceipt>,
						And<Vendor.vStatus, IsNotNull,
						And<BAccountR.type, In3<BAccountType.vendorType, BAccountType.combinedType>,
					Or<Current<BqlPlaceholder.A>, Equal<POReceiptType.transferreceipt>,
						And<Where<BAccountR.isBranch, Equal<True>,
						Or<BAccountR.type, In3<BAccountType.branchType, BAccountType.organizationType, BAccountType.combinedType>>>>>>>>>
				.Replace<BqlPlaceholder.A>(receiptType)
				.ToType(),
			Messages.VendorNonEmployeeOrOrganizationDependingOnReceiptType)
		{
		}
	}
}
