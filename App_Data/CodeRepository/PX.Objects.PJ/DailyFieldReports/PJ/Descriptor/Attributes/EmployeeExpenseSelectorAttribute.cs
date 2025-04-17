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

using System.Collections;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Common;
using PX.Data;
using PX.Objects.EP;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class EmployeeExpenseSelectorAttribute : RelatedEntitiesBaseSelectorAttribute
    {
        public EmployeeExpenseSelectorAttribute()
            : base(typeof(EPExpenseClaimDetails.claimDetailCD),
                typeof(EPExpenseClaimDetails.claimDetailCD),
                typeof(EPExpenseClaimDetails.expenseDate),
                typeof(EPExpenseClaimDetails.expenseRefNbr),
                typeof(EPExpenseClaimDetails.employeeID),
                typeof(EPExpenseClaimDetails.branchID),
                typeof(EPExpenseClaimDetails.tranDesc),
                typeof(EPExpenseClaimDetails.curyTranAmtWithTaxes),
                typeof(EPExpenseClaimDetails.curyID),
                typeof(EPExpenseClaimDetails.status))
        {
        }

        public IEnumerable GetRecords()
        {
            var linkedEmployeeExpensesNumbers = _Graph.GetExtension<DailyFieldReportEntryEmployeeExpensesExtension>()
                .EmployeeExpenses.SelectMain().Select(ee => ee.EmployeeExpenseId);
            return GetRelatedEntities<EPExpenseClaimDetails, EPExpenseClaimDetails.contractID>()
                .Where(ec => ec.ClaimDetailCD.IsNotIn(linkedEmployeeExpensesNumbers));
        }

        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
		{
			if (args.NewValue == null || !ValidateValue || _BypassFieldVerifying.Value)
			{
				return;
			}

			bool isFound = SelectFrom<EPExpenseClaimDetails>
				.Where<EPExpenseClaimDetails.claimDetailCD.IsEqual<@P.AsString>>
				.View.Select(_Graph, args.NewValue.ToString()).Any();

			if (isFound)
			{
				return;
			}

			throw new PXSetPropertyException(SharedMessages.CannotBeFound, $"[{_FieldName}]");
		}
	}
}
