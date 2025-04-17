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
using System.Linq;
using PX.CS.Contracts.Interfaces;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.WeatherBitModels;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public class WeatherBitIntegrationStrategy : WeatherIntegrationBaseStrategy<WeatherBitResponse>
    {
        private const string IconUrl = "https://www.weatherbit.io/static/img/icons/{0}.png";

        protected override string BaseUrl => "https://api.weatherbit.io/v2.0/current";

        public override void AddRequiredParameters(WeatherIntegrationSetup weatherIntegrationSetup)
        {
            Request.AddQueryParameter("key", weatherIntegrationSetup.WeatherApiKey);
            Request.AddQueryParameter("units", WeatherIntegrationConstants.UnitOfMeasures.WeatherBitFahrenheit);
        }

        public override void AddCityAndCountryParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("city", location.City);
            Request.AddQueryParameter("country", location.CountryID);
        }

        public override void AddZipCodeAndCountryParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("postal_code", location.PostalCode);
            Request.AddQueryParameter("country", location.CountryID);
        }

        public override void AddGeographicLocationParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("lat", location.Latitude);
            Request.AddQueryParameter("lon", location.Longitude);
        }

        protected override DailyFieldReportWeather CreateDailyFieldReportWeather(WeatherBitResponse model)
        {
            var response = model.Data.First();
            return new DailyFieldReportWeather
            {
                Icon = string.Format(IconUrl, response.Weather.Icon),
                TimeObserved = DateTimeOffset.FromUnixTimeSeconds(response.TimeObserved).ToUniversalTime().DateTime,
                TemperatureLevel = response.TemperatureLevel,
                WindSpeed = response.WindSpeed,
                Cloudiness = response.Cloudiness,
                Humidity = response.Humidity ?? decimal.Zero,
                PrecipitationAmount = GetPrecipitationAmount(response),
                LocationCondition = response.Weather.SiteCondition
            };
        }

        private static decimal? GetPrecipitationAmount(Models.WeatherBitModels.Data response)
        {
            if (response.Rain == decimal.Zero && response.Snowfall != decimal.Zero)
            {
                return response.Snowfall;
            }
            if (response.Rain != decimal.Zero && response.Snowfall == decimal.Zero)
            {
                return response.Rain;
            }
            return response.Weather.Code
                .StartsWith(WeatherIntegrationConstants.WeatherBitServiceWeatherCodes.StartCodeOfSnowRange)
                ? response.Snowfall
                : response.Rain;
        }
    }
}
