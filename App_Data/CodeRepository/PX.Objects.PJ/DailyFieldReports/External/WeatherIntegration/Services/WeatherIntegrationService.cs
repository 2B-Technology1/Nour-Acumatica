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
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Api;
using PX.Data;
using PX.CS.Contracts.Interfaces;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public class WeatherIntegrationService : IWeatherIntegrationService
    {
        private readonly PXGraph graph;
        private readonly PXCache cache;

        private IAddressLocation location;
        private IWeatherIntegrationStrategy weatherIntegrationStrategy;

		private PXCache<WeatherIntegrationSetup> weatherIntegrationSetupCache;

		private WeatherIntegrationSetup weatherIntegrationSetup => (WeatherIntegrationSetup)weatherIntegrationSetupCache.Current;

		public WeatherIntegrationService(PXGraph graph)
		{
			this.graph = graph;
			weatherIntegrationSetupCache = graph.Caches<WeatherIntegrationSetup>();
			cache = graph.Caches<DailyFieldReport>();
		}

		public IWeatherIntegrationStrategy WeatherIntegrationStrategy
        {
            get
            {
                if (weatherIntegrationStrategy != null)
                {
                    return weatherIntegrationStrategy;
                }

                switch (weatherIntegrationSetup.WeatherApiService)
                {
					case WeatherApiService.OpenWeatherMap:
						weatherIntegrationStrategy=(IWeatherIntegrationStrategy) new OpenWeatherMapIntegrationStrategy();break;
					case WeatherApiService.WeatherBit:
						weatherIntegrationStrategy = new WeatherBitIntegrationStrategy();break;
					case WeatherApiService.AccuWeather:
						weatherIntegrationStrategy = CreateAccuWeatherIntegrationStrategy();break;
					default:
						throw new Exception(WeatherIntegrationMessages
							.ToLoadWeatherConditionsMustBeSpecifiedOnWeatherIntegrationSettingsTab);
				}

                return weatherIntegrationStrategy;
            }
            set => weatherIntegrationStrategy = value ?? throw new ArgumentNullException(nameof(value));
        }

        public DailyFieldReportWeather GetDailyFieldReportWeather()
        {
            location = (IAddressLocation) cache.Current;
            ValidateWeatherIntegrationSetup();
            WeatherIntegrationStrategy.InitializeRequest(graph, weatherIntegrationSetup);
            AddQueryParameters();
            return WeatherIntegrationStrategy.GetDailyFieldReportWeather();
        }

        private void AddQueryParameters()
        {
            switch (weatherIntegrationSetup.RequestParametersType)
            {
                case WeatherRequestParameters.CityAndCountry:
                    ValidateFields(nameof(location.City), nameof(location.CountryID));
                    WeatherIntegrationStrategy.AddCityAndCountryParameters(location);
                    break;
                case WeatherRequestParameters.ZipCodeAndCountry:
                    ValidateFields(nameof(location.PostalCode), nameof(location.CountryID));
                    WeatherIntegrationStrategy.AddZipCodeAndCountryParameters(location);
                    break;
                case WeatherRequestParameters.GeographicLocation:
                    ValidateFields(nameof(location.Latitude), nameof(location.Longitude));
                    WeatherIntegrationStrategy.AddGeographicLocationParameters(location);
                    break;
            }
            WeatherIntegrationStrategy.AddRequiredParameters(weatherIntegrationSetup);
        }

        private void ValidateWeatherIntegrationSetup()
        {
            if (weatherIntegrationSetup.RequestParametersType.IsNullOrEmpty() ||
                weatherIntegrationSetup.WeatherApiKey.IsNullOrEmpty())
            {
                throw new Exception(WeatherIntegrationMessages
                    .ToLoadWeatherConditionsMustBeSpecifiedOnWeatherIntegrationSettingsTab);
            }
        }

        private void ValidateFields(string fieldName1, string fieldName2)
        {
            if (cache.GetValue(location, fieldName1) == null ||
                cache.GetValue(location, fieldName2) == null)
            {
                var message = string.Format(WeatherIntegrationMessages
                        .ToLoadWeatherConditionsMustBeSpecifiedForDailyFieldReport,
                    PXUIFieldAttribute.GetDisplayName(cache, fieldName1).ToLower(),
                    PXUIFieldAttribute.GetDisplayName(cache, fieldName2).ToLower());
                throw new Exception(message);
            }
        }

        private IWeatherIntegrationStrategy CreateAccuWeatherIntegrationStrategy()
        {
            ValidateFields(nameof(location.Latitude), nameof(location.Longitude));
            return new AccuWeatherIntegrationStrategy(weatherIntegrationSetup, location, graph);
        }
    }
}