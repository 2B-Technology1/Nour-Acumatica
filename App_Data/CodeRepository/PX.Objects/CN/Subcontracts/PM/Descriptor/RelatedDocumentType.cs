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
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PM.Descriptor
{
	public class RelatedDocumentType
	{
		public const string AllCommitmentsType = "ALL";
		public const string PurchaseOrderType = POOrderType.RegularOrder + " + " + POOrderType.ProjectDropShip; //string.Join(" + ", new object[] { POOrderType.RegularOrder, POOrderType.ProjectDropShip })
		public const string SubcontractType = POOrderType.RegularSubcontract;

		public const string AllCommitmentsLabel = "All Commitments";
		public const string PurchaseOrderLabel = "Purchase Order";
		public const string SubcontractLabel = "Subcontract";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(AllCommitmentsType, AllCommitmentsLabel),
					Pair(PurchaseOrderType, PurchaseOrderLabel),
					Pair(SubcontractType, SubcontractLabel)
				})
			{ }
		}

		public class DetailListAttribute : PXStringListAttribute
		{
			public DetailListAttribute() : base(
				(POOrderType.RegularOrder, $"{PurchaseOrderLabel}, {PX.Objects.PO.Messages.RegularOrder}"),
				(POOrderType.RegularSubcontract, SubcontractLabel),
				(POOrderType.ProjectDropShip, $"{PurchaseOrderLabel}, {PX.Objects.PO.Messages.ProjectDropShip}"))
			{ }
		}

		public class all : PX.Data.BQL.BqlString.Constant<all>
		{
			public all() : base(AllCommitmentsType) { }
		}

		public class purchaseOrder : PX.Data.BQL.BqlString.Constant<purchaseOrder>
		{
			public purchaseOrder() : base(PurchaseOrderType) { }
		}

		public class subcontract : PX.Data.BQL.BqlString.Constant<subcontract>
		{
			public subcontract() : base(SubcontractType) { }
		}
	}
}
