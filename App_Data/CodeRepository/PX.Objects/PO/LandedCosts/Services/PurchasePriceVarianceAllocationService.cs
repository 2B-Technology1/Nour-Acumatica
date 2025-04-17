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
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts
{
	public class PurchasePriceVarianceAllocationService : AllocationServiceBase
	{
		private static readonly Lazy<PurchasePriceVarianceAllocationService> _purchasePriceVarianceAllocationService = new Lazy<PurchasePriceVarianceAllocationService>(PXGraph.CreateInstance<PurchasePriceVarianceAllocationService>);

		public static PurchasePriceVarianceAllocationService Instance => _purchasePriceVarianceAllocationService.Value;

		public virtual decimal AllocateOverRCTLine(PXGraph graph, List<POReceiptLineAdjustment> result, POReceiptLine aLine, decimal toDistribute, Int32? branchID)
		{
			var allocationItem = new AllocationItem
			{
				LandedCostCode = new LandedCostCode(),
				ReceiptLine = aLine
			};

			allocationItem.LandedCostCode.AllocationMethod = LandedCostAllocationMethod.ByQuantity;

			var rest = AllocateOverRCTLine(graph, result, allocationItem, toDistribute, branchID);

			return rest;
		}
	}
}
