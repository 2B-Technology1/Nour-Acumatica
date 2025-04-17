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
using PX.Objects.CN.ProjectAccounting.Descriptor;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor
{
    public class ProjectTaskType
    {
        public const string Cost = "Cost";
        public const string Revenue = "Rev";
        public const string CostRevenue = "CostRev";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] TaskTypesValues =
            {
	            Cost,
	            Revenue,
				CostRevenue
			};

            private static readonly string[] TaskTypesLabels =
            {
	            ProjectAccountingLabels.CostTask,
	            ProjectAccountingLabels.RevenueTask,
				ProjectAccountingLabels.CostAndRevenueTask
			};

			public ListAttribute()
                : base(TaskTypesValues, TaskTypesLabels)
            {
            }
        }

        public class cost : BqlString.Constant<cost>
        {
            public cost()
                : base(Cost)
            {
            }
        }

        public class revenue : BqlString.Constant<revenue>
        {
            public revenue()
                : base(Revenue)
            {
            }
        }

        public class costRevenue : BqlString.Constant<costRevenue>
        {
            public costRevenue()
                : base(CostRevenue)
            {
            }
        }
    }
}