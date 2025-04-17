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
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class WeatherRequestParameters
    {
        public const string ZipCodeAndCountry = "Zip Code And Country";
        public const string CityAndCountry = "City And Country";
        public const string GeographicLocation = "Geographic Location";

        private static readonly string[] RequestParameters =
        {
            ZipCodeAndCountry,
            CityAndCountry,
            GeographicLocation
        };

        private static readonly string[] AccuWeatherParameters =
        {
            GeographicLocation
        };

        public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
        {
            public void RowSelected(PXCache cache, PXRowSelectedEventArgs args)
            {
                if (args.Row is WeatherIntegrationSetup weatherSetup)
                {
                    switch (weatherSetup.WeatherApiService)
                    {
                        case WeatherApiService.OpenWeatherMap:
                        case WeatherApiService.WeatherBit:
                            _AllowedLabels = RequestParameters;
                            _AllowedValues = RequestParameters;
                            break;
                        case WeatherApiService.AccuWeather:
                            _AllowedLabels = AccuWeatherParameters;
                            _AllowedValues = AccuWeatherParameters;
                            break;
                    }
                }
            }
        }
    }
}
