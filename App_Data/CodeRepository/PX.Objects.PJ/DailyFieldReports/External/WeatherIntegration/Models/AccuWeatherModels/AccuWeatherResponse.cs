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

using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.AccuWeatherModels
{
    public class AccuWeatherResponse
    {
        [JsonProperty("WeatherIcon")]
        public string Icon
        {
            get;
            set;
        }

        [JsonProperty("PrecipitationSummary")]
        public PrecipitationSummary Precipitation
        {
            get;
            set;
        }

        public Wind Wind
        {
            get;
            set;
        }

        [JsonProperty("CloudCover")]
        public int? Cloudiness
        {
            get;
            set;
        }

        public Temperature Temperature
        {
            get;
            set;
        }

        [JsonProperty("EpochTime")]
        public long TimeObserved
        {
            get;
            set;
        }

        [JsonProperty("RelativeHumidity")]
        public int? Humidity
        {
            get;
            set;
        }

        [JsonProperty("WeatherText")]
        public string SiteCondition
        {
            get;
            set;
        }
    }
}
