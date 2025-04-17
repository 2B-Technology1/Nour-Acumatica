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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Employee Activity")]
    public class DailyFieldReportEmployeeActivity : IBqlTable
    {
        [PXDBIdentity]
        public virtual int? DailyFieldReportEmployeeActivityId
        {
            get;
            set;
        }

        [PXDBInt(IsKey = true)]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public virtual int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDBGuid(IsKey = true)]
        [PXParent(typeof(SelectFrom<EPActivityApprove>
            .Where<EPActivityApprove.noteID.IsEqual<employeeActivityId.FromCurrent>>))]
        public virtual Guid? EmployeeActivityId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportEmployeeActivityId : BqlInt.Field<dailyFieldReportEmployeeActivityId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class employeeActivityId : BqlGuid.Field<employeeActivityId>
        {
        }
    }
}