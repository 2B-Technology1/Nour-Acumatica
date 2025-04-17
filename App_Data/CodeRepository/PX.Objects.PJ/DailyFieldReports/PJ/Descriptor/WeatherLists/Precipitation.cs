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

using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists
{
    public class Precipitation
    {
        public const string None = "None";
        public const string Thunderstorm = "Thunderstorm";
        public const string Drizzle = "Drizzle";
        public const string LightRain = "Light Rain";
        public const string ModerateRain = "Moderate Rain";
        public const string HeavyRain = "Heavy Rain";
        public const string ShowerRain = "Shower Rain";
        public const string LightSnow = "Light Snow";
        public const string Snow = "Snow";
        public const string HeavySnow = "Heavy Snow";
        public const string MixSnowRain = "Mix Snow/Rain";

        private static readonly string[] AllowedValues =
        {
            None,
            Thunderstorm,
            Drizzle,
            LightRain,
            ModerateRain,
            HeavyRain,
            ShowerRain,
            LightSnow,
            Snow,
            HeavySnow,
            MixSnowRain
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }
    }
}
