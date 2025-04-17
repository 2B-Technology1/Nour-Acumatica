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

using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.DAC
{
    [PXCacheName("Daily Field Report Photo Log")]
    public class DailyFieldReportPhotoLog : IBqlTable
    {
        [PXDBIdentity(IsKey = true)]
        public int? DailyFieldReportPhotoLogId
        {
            get;
            set;
        }

        [PXDBInt]
        [PXDBDefault(typeof(DailyFieldReport.dailyFieldReportId))]
        [PXParent(typeof(SelectFrom<DailyFieldReport>
            .Where<DailyFieldReport.dailyFieldReportId.IsEqual<dailyFieldReportId.FromCurrent>>))]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        [PXDefault]
        [PXDBInt]
        [PXSelector(typeof(SearchFor<PhotoLog.photoLogId>
                .Where<PhotoLog.photoLogId
                    .IsNotInSubselect<SearchFor<photoLogId>
                        .Where<dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>>
                    .And<PhotoLog.projectId.IsEqual<DailyFieldReport.projectId.FromCurrent>>
                    .Or<PhotoLog.dailyFieldReportId.FromCurrent.IsNotNull>>),
            SubstituteKey = typeof(PhotoLog.photoLogCd))]
        [PXParent(typeof(SelectFrom<PhotoLog>
            .Where<PhotoLog.photoLogId.IsEqual<photoLogId.FromCurrent>>))]
        [PXUIField(DisplayName = "Photo Log ID", Required = true)]
        public int? PhotoLogId
        {
            get;
            set;
        }

        public abstract class dailyFieldReportPhotoLogId : BqlInt.Field<dailyFieldReportPhotoLogId>
        {
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }

        public abstract class photoLogId : BqlInt.Field<photoLogId>
        {
        }
    }
}
