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

using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using System;

namespace PX.Objects.PM
{
	public class MapService
    {
        private readonly PXGraph graph;

        public MapService(PXGraph graph)
        {
            this.graph = graph;
        }

        public virtual void viewAddressOnMap(IAddressLocation location)
        {
            var mapRedirector = SitePolicy.CurrentMapRedirector;
            if (mapRedirector == null)
            {
                return;
            }
            if (location.Latitude == null && location.Longitude == null)
            {
                ShowLocationByAddress(mapRedirector, location, location.AddressLine1);
            }
            else
            {
                ShowLocationByCoordinates(location.Latitude, location.Longitude);
            }
        }

        private void ShowLocationByAddress(MapRedirector mapRedirector, IAddressLocation location, string siteAddress)
        {
            var country = GetCountry(location.CountryID)?.Description ?? location.CountryID;
            mapRedirector.ShowAddress(country, location.State, location.City, location.PostalCode, siteAddress, null,
                null);
        }

        private Country GetCountry(string code)
        {
            return SelectFrom<Country>
                .Where<Country.countryID.IsEqual<P.AsString>>.View
                .Select(graph, code);
        }

        private static void ShowLocationByCoordinates(decimal? latitude, decimal? longitude)
        {
            new GoogleMapLatLongRedirector().ShowAddressByLocation(latitude, longitude);
        }

        public class GoogleMapLatLongRedirector : GoogleMapRedirector
        {
            #region Methods
            public void ShowAddressByLocation(decimal? latitude, decimal? longitude)
            {
                string latLong = string.Empty;
                string countryDummy = string.Empty;
                string stateDummy = string.Empty;
                string cityDummy = string.Empty;
                string postalCodeDummy = string.Empty;
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
}
