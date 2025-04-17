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

namespace PX.Objects.FS
{
    public static class Haversine
    {
        public const double Rm = 3960; // means radius of the earth (miles) at 39 degrees from the equator
        public const double Rk = 6371; // means radius of the earth (km) at 39 degrees from the equator

        public enum DistanceUnit { Miles, Kilometers };

        public static double calculate(LatLng from, LatLng to, DistanceUnit unit)
        {
            return calculate(from.Latitude, from.Longitude, to.Latitude, to.Longitude, unit);
        }

        public static double calculate(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit)
        {
            double R = (unit == DistanceUnit.Miles) ? Rm : Rk;
            double dLat = toRadians(lat2 - lat1);
            double dLon = toRadians(lon2 - lon1);
            double rlat1 = toRadians(lat1);
            double rlat2 = toRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rlat1) * Math.Cos(rlat2);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }

        public static double toRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
