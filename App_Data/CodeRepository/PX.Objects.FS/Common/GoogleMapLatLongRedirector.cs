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

namespace PX.Data
{
    public class GoogleMapLatLongRedirector : GoogleMapRedirector
    {
        #region Methods
        public void ShowAddressByLocation(decimal? latitude, decimal? longitude)
        {
            string latLong           = string.Empty;
            string countryDummy      = string.Empty;
            string stateDummy        = string.Empty;
            string cityDummy         = string.Empty;
            string postalCodeDummy   = string.Empty;
            string addressLine1Dummy = string.Empty;
            string addressLine2Dummy = string.Empty;

            if (latitude == null)
            {
                latitude = 0;
            }

            if (longitude == null)
            {
                longitude = 0;
            }

            latLong += Convert.ToString(latitude) + "," + Convert.ToString(longitude);

            ShowAddress(countryDummy, stateDummy, cityDummy, postalCodeDummy, addressLine1Dummy, addressLine2Dummy, latLong);
        }
        #endregion
    }
}
