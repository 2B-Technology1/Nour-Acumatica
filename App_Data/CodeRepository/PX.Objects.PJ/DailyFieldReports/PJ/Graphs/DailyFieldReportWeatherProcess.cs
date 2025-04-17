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

using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Data;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
    public class DailyFieldReportWeatherProcess : PXGraph<DailyFieldReportWeatherProcess>
    {
        public PXFilter<DailyFieldReportWeatherFilter> Filter;

        public PXCancel<DailyFieldReportWeatherFilter> Cancel;

		public PXSetup<WeatherIntegrationSetup> WeatherIntegrationSetup;

        [PXFilterable]
        public PXFilteredProcessing<DailyFieldReport, DailyFieldReportWeatherFilter,
            Where<DailyFieldReport.hold.IsEqual<True>
                .And<DailyFieldReport.projectId.IsEqual<DailyFieldReportWeatherFilter.projectId.FromCurrent>
                    .Or<DailyFieldReportWeatherFilter.projectId.FromCurrent.IsNull>>
                .And<DailyFieldReport.date.IsEqual<AccessInfo.businessDate.FromCurrent>>>,
            OrderBy<Desc<DailyFieldReport.dailyFieldReportId>>> DailyFieldReports;

		public DailyFieldReportWeatherProcess()
		{
			if (WeatherIntegrationSetup.Current?.IsConfigurationEnabled != true)
			{
				throw new PXSetupNotEnteredException(WeatherIntegrationMessages.LoadWeatherConditionsIsAvailableIfSettingsAreEnabled,
					typeof(ProjectManagementSetup), CacheNames.ProjectManagementPreferences);
			}
		}

        public virtual void _(Events.RowSelected<DailyFieldReportWeatherFilter> args)
        {
            DailyFieldReports.SetProcessDelegate(CreateWeatherForDailyFieldReport);
        }

        private static void CreateWeatherForDailyFieldReport(DailyFieldReport dailyFieldReport)
        {
            var dailyFieldReportGraph = CreateInstance<DailyFieldReportEntry>();
            var weatherExtension = dailyFieldReportGraph
                .GetExtension<DailyFieldReportEntryWeatherExtension>();
            dailyFieldReportGraph.DailyFieldReport.Current = dailyFieldReport;
            weatherExtension.LoadWeatherConditions.Press();
            weatherExtension.Weather.Cache.Persist(PXDBOperation.Insert);
        }
    }
}
