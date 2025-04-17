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

namespace PX.Objects.PO
{
	public class ReturnCostMode
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(OriginalCost, Messages.OriginalReceiptCost),
					Pair(CostByIssue, Messages.CostByIssueStategy),
					Pair(ManualCost, Messages.ManualCost)
				})
			{ }
		}

		public const string NotApplicable = "N";
		public const string OriginalCost = "O";
		public const string CostByIssue = "I";
		public const string ManualCost = "M";

		public class notApplicable : PX.Data.BQL.BqlString.Constant<notApplicable>
		{
			public notApplicable() : base(NotApplicable) {; }
		}

		public class originalCost : PX.Data.BQL.BqlString.Constant<originalCost>
		{
			public originalCost() : base(OriginalCost) {; }
		}

		public class costByIssue : PX.Data.BQL.BqlString.Constant<costByIssue>
		{
			public costByIssue() : base(CostByIssue) { }
		}

		public class manualCost : PX.Data.BQL.BqlString.Constant<manualCost>
		{
			public manualCost() : base(ManualCost) { }
		}
	}
}
