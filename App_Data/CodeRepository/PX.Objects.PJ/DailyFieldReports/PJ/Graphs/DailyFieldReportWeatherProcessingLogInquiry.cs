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
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
    public class DailyFieldReportWeatherProcessingLogInquiry : PXGraph<DailyFieldReportWeatherProcessingLogInquiry>
    {
        public PXFilter<WeatherProcessingLogFilter> Filter;
        public PXCancel<WeatherProcessingLogFilter> Cancel;

        public PXSetup<WeatherIntegrationSetup> WeatherIntegrationSetup;

        [PXFilterable]
        public SelectFrom<WeatherProcessingLog>
            .InnerJoin<DailyFieldReport>
                .On<WeatherProcessingLog.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId>>
            .Where<Brackets<DailyFieldReport.projectId.IsEqual<WeatherProcessingLogFilter.projectId.FromCurrent>
                    .Or<WeatherProcessingLogFilter.projectId.FromCurrent.IsNull>>
                .And<WeatherProcessingLog.weatherService
                    .IsEqual<WeatherProcessingLogFilter.weatherApiService.FromCurrent>
                    .Or<WeatherProcessingLogFilter.weatherApiService.FromCurrent.IsNull>>
                .And<WeatherProcessingLog.requestTime
                    .IsGreaterEqual<WeatherProcessingLogFilter.requestDateFrom.FromCurrent>
                    .Or<WeatherProcessingLogFilter.requestDateFrom.FromCurrent.IsNull>>
                .And<WeatherProcessingLog.requestTime
                    .IsLessEqual<WeatherProcessingLogFilter.requestDateTo.FromCurrent>
                    .Or<WeatherProcessingLogFilter.requestDateTo.FromCurrent.IsNull>>
                .And<WeatherProcessingLog.requestStatusIcon
                    .IsEqual<WeatherIntegrationConstants.RequestStatusIcons.requestStatusFailIcon>
                    .Or<WeatherProcessingLogFilter.isShowErrorsOnly.FromCurrent.IsEqual<False>>>>
            .OrderBy<Desc<WeatherProcessingLog.requestTime>>.View.ReadOnly WeatherProcessingLogs;

        public DailyFieldReportWeatherProcessingLogInquiry()
        {
            if (WeatherIntegrationSetup.Current?.IsConfigurationEnabled != true ||
                WeatherIntegrationSetup.Current?.IsWeatherProcessingLogEnabled != true)
            {
                throw new PXSetupNotEnteredException(WeatherIntegrationMessages.WeatherLogIsAvailableIfSettingsAreEnabled, 
                    typeof(ProjectManagementSetup), CacheNames.ProjectManagementPreferences);
            }
        }

        public virtual void _(Events.FieldDefaulting<WeatherProcessingLogFilter,
            WeatherProcessingLogFilter.requestDateFrom> args)
        {
            args.NewValue = Accessinfo.BusinessDate?.AddDays(
                WeatherIntegrationConstants.ProcessingLogFilter.DefaultRequestDateFromDifference);
        }
    }
}