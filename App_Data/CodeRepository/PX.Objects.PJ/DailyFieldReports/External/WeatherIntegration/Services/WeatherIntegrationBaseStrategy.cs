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

using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Infrastructure;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.CS.Contracts.Interfaces;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public abstract class WeatherIntegrationBaseStrategy<TModel> : IWeatherIntegrationStrategy
        where TModel : class, new()
    {
        protected Request Request;

        protected abstract string BaseUrl
        {
            get;
        }

        public void InitializeRequest(PXGraph graph, WeatherIntegrationSetup weatherIntegrationSetup)
        {
            Request = weatherIntegrationSetup.IsWeatherProcessingLogEnabled == true
                ? new RequestWithWeatherProcessingLog(graph, BaseUrl)
                : new Request(BaseUrl);
        }

        public DailyFieldReportWeather GetDailyFieldReportWeather()
        {
            var model = Request.Execute<TModel>();
            return CreateDailyFieldReportWeather(model);
        }

        public abstract void AddRequiredParameters(WeatherIntegrationSetup weatherIntegrationSetup);

        public abstract void AddCityAndCountryParameters(IAddressLocation location);

        public abstract void AddZipCodeAndCountryParameters(IAddressLocation location);

        public abstract void AddGeographicLocationParameters(IAddressLocation location);

        protected abstract DailyFieldReportWeather CreateDailyFieldReportWeather(TModel model);
    }
}