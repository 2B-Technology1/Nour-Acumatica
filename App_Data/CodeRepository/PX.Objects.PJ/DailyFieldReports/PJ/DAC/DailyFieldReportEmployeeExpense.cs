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

using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Employee Expenses")]
    public class DailyFieldReportEmployeeExpense : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public virtual int? DailyFieldReportEmployeeExpenseId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBDefault(typeof(EPExpenseClaimDetails.claimDetailCD))]
        [PXDBString(15, IsUnicode = true)]
        [EmployeeExpenseSelector]
        [PXParent(typeof(SelectFrom<EPExpenseClaimDetails>
            .Where<EPExpenseClaimDetails.claimDetailCD.IsEqual<employeeExpenseId.FromCurrent>>))]
        [PXUIField(DisplayName = "Reference Number")]
        public virtual string EmployeeExpenseId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEmployeeExpensesId : BqlGuid.Field<dailyFieldReportEmployeeExpensesId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class employeeExpenseId : BqlString.Field<employeeExpenseId>
        {
        }
    }
}
