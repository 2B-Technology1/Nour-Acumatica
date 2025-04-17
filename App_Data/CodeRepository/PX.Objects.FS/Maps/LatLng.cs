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
using System.Globalization;
using System.Xml;

namespace PX.Objects.FS
{
/// <summary>
/// Class representing a latitude/longitude pair.
/// </summary>
    public class LatLng
    {
        public LatLng()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLng"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public LatLng(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LatLng"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public LatLng(decimal? latitude, decimal? longitude)
        {
            this.latitude = Convert.ToDouble(latitude);
            this.longitude = Convert.ToDouble(longitude);
        }

        internal LatLng(XmlElement locationElement, XmlNamespaceManager nameSpace)
        {
            this.latitude = double.Parse(locationElement.SelectSingleNode(
            string.Format(".//{0}Latitude",ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText, CultureInfo.InvariantCulture);
            this.longitude = double.Parse(locationElement.SelectSingleNode(
            string.Format(".//{0}Longitude", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText, CultureInfo.InvariantCulture);
        }

        private double latitude;

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public double Latitude
        {
            get
            {
                return this.latitude;
            }
        }

        private double longitude;

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public double Longitude
        {
            get
            {
                return this.longitude;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.latitude.ToString() + ", " + this.longitude.ToString();
        }
    }
}
