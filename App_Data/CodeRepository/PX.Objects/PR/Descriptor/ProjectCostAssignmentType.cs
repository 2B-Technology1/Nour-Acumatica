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

namespace PX.Objects.PR.Standalone
{
	public class ProjectCostAssignmentType
	{
		public class noCostAssigned : PX.Data.BQL.BqlString.Constant<noCostAssigned>
		{
			public noCostAssigned() : base(NoCostAssigned) { }
		}

		public class wageCostAssigned : PX.Data.BQL.BqlString.Constant<wageCostAssigned>
		{
			public wageCostAssigned() : base(WageCostAssigned) { }
		}

		public class wageLaborBurdenAssigned : PX.Data.BQL.BqlString.Constant<wageLaborBurdenAssigned>
		{
			public wageLaborBurdenAssigned() : base(WageLaborBurdenAssigned) { }
		}

		public const string NoCostAssigned = "NCA";
		public const string WageCostAssigned = "WCA";
		public const string WageLaborBurdenAssigned = "WLB";
	}
}
