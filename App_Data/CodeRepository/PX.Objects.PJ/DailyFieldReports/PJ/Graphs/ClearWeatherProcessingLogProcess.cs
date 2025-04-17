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
using System.Collections;
using System.Collections.Generic;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
    public class ClearWeatherProcessingLogProcess : PXGraph<ClearWeatherProcessingLogProcess>
    {
        public PXSetup<WeatherIntegrationSetup> WeatherIntegrationSetup;

        public PXProcessingJoin<WeatherProcessingLog,
                InnerJoin<DailyFieldReport,
                    On<WeatherProcessingLog.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId>>>>
            WeatherProcessingLogs;

        public ClearWeatherProcessingLogProcess()
        {
	        var setup = WeatherIntegrationSetup.Current;

	        if (SiteMapExtension.IsClearDailyFieldReportWeatherProcessingLogScreen() &&
                (WeatherIntegrationSetup.Current?.IsConfigurationEnabled != true ||
                    WeatherIntegrationSetup.Current?.IsWeatherProcessingLogEnabled != true))
            {
                throw new PXSetupNotEnteredException(WeatherIntegrationMessages.ClearWeatherLogIsAvailableIfSettingsAreEnabled,
                    typeof(ProjectManagementSetup), CacheNames.ProjectManagementPreferences);
            }
        }

        public IEnumerable weatherProcessingLogs()
        {
            var logsLifeTime = GetLogsLifeTime();
            return SelectFrom<WeatherProcessingLog>
                .InnerJoin<DailyFieldReport>
                    .On<WeatherProcessingLog.dailyFieldReportId.IsEqual<DailyFieldReport.dailyFieldReportId>>
                .Where<WeatherProcessingLog.requestTime.IsLess<P.AsDateTime>>.View.Select(this, logsLifeTime);
        }

        public virtual void _(Events.RowSelected<WeatherProcessingLog> args)
        {
            WeatherProcessingLogs.SetProcessDelegate(ClearWeatherProcessingLog);
        }

        private DateTime? GetLogsLifeTime()
        {
            var days = WeatherIntegrationSetup.Current.WeatherProcessingLogTerm.GetValueOrDefault();
            var term = new TimeSpan(days - 1, 0, 0, 0);
            return Accessinfo.BusinessDate?.Subtract(term);
        }

        private static void ClearWeatherProcessingLog(List<WeatherProcessingLog> weatherProcessingLogs)
        {
            var graph = CreateInstance(typeof(PXGraph));
            graph.Caches<WeatherProcessingLog>().DeleteAll(weatherProcessingLogs);
            graph.Caches<WeatherProcessingLog>().Persist(PXDBOperation.Delete);
            throw new PXRefreshException();
        }
    }
}