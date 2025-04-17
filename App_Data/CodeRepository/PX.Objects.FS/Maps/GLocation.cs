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
using System.Collections.Generic;
using System.Text;

namespace PX.Objects.FS
{
    /// <summary>
    /// Class representing a location, defined by name and/or by latitude/longitude.
    /// </summary>
    public class GLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GLocation"/> class.
        /// </summary>
        /// <param name="locationName">Name of the location.</param>
        public GLocation(string locationName)
        {
            this.locationName = locationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GLocation"/> class.
        /// </summary>
        /// <param name="latLng">The latitude/longitude of the location.</param>
        public GLocation(LatLng latLng)
        {
            this.latLng = latLng;
        }

        internal GLocation(LatLng latLng, string locationName)
        {
            this.latLng = latLng;
            this.locationName = locationName;
        }

        private LatLng latLng;

        /// <summary>
        /// Gets the latitude/longitude of the location.
        /// </summary>
        public LatLng LatLng
        {
            get { return this.latLng; }
        }

        private string locationName;

        /// <summary>
        /// Gets the name/address of the location.
        /// </summary>
        /// <value>
        /// The name/address of the location.
        /// </value>
        public string LocationName
        {
            get { return this.locationName; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.locationName != null)
            {
                return this.locationName;
            }

            return this.latLng.ToString();
        }
    }
}
