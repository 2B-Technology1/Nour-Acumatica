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
using System.Text.RegularExpressions;
using System.Xml;

namespace PX.Objects.FS
{
    /// <summary>
    /// Wrapper round the Google Maps geocoding service.
    /// </summary>
    public static class Geocoder
    {

        /// <summary>
        /// Reverses geocode the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Returns the address of the location.</returns>
        public static string ReverseGeocode(LatLng location, string apiKey)
        {
            string response;
            try
            {
                response = HttpWebService.MakeRequest(
                                            string.Format(
                                                "{0}Locations/{1},{2}?o=xml&key={3}",
                                                ID.MapsConsts.API_PREFIX,
                                                location.Latitude,
                                                location.Longitude,
                                                apiKey));
            }
            catch
            {
                throw new Exception(TX.Error.MAPS_CONNECTION_FAILED);
            }

            XmlDocument responseXml = new XmlDocument() { XmlResolver = null };
            responseXml.LoadXml(response);

            //The XML document comes with a schema. So, was needed load the schema used by the site
            XmlNamespaceManager nameSpace = new XmlNamespaceManager(responseXml.NameTable);
            SharedFunctions.GenerateXmlNameSpace(ref nameSpace);
            string directionResult = string.Empty;

            string[] resultType = { "Address", "PopulatedPlace"};

            foreach (string type in resultType)
            { 
                XmlNode result = responseXml.SelectSingleNode(
                                                string.Format(
                                                    "//{0}Location[{0}EntityType = '{1}']//{0}FormattedAddress",
                                                    ID.MapsConsts.XML_SCHEMA_TAG, 
                                                    type),
                                                nameSpace);
                if (result != null)
                {
                    directionResult = result.InnerText;
                    break;
                }
            }

            return directionResult;
        }

        /// <summary>
        /// Geocodes the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>An array of possible locations.</returns>
        public static GLocation[] Geocode(string rAddress, string apiKey)
        {
            string response;
            XmlDocument responseXml = new XmlDocument() { XmlResolver = null };

            try
            {
                string address = Regex.Replace(rAddress, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);

                response = HttpWebService.MakeRequest(
                                            string.Format(
                                                "{0}Locations/{1}?o=xml&key={2}", 
                                                ID.MapsConsts.API_PREFIX,
                                                address, 
                                                apiKey));
 
                responseXml.LoadXml(response);
            }
            catch
            {
                throw new Exception(TX.Error.MAPS_CONNECTION_FAILED);
            }
      
            //The XML document comes with a schema. So, was needed load the schema used by the site
            XmlNamespaceManager nameSpace = new XmlNamespaceManager(responseXml.NameTable);
            SharedFunctions.GenerateXmlNameSpace(ref nameSpace);

            XmlNodeList results = responseXml.SelectNodes(
                                                string.Format("//{0}Location", 
                                                    ID.MapsConsts.XML_SCHEMA_TAG), 
                                                nameSpace);

            List<GLocation> locations = new List<GLocation>();

            foreach (XmlElement result in results)
            {
                string formattedAddress = result.SelectSingleNode(
                                                        string.Format(".//{0}FormattedAddress", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace).InnerText;
                                                        XmlElement locationElement = (XmlElement)result.SelectSingleNode(
                                                            string.Format(".//{0}Point", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace);

                LatLng latLng = new LatLng(locationElement, nameSpace);
                GLocation location = new GLocation(latLng, formattedAddress);
                locations.Add(location);
            }

            return locations.ToArray();
        }

        public static string GetStatus(string rAddress, string apiKey)
        {
            string response;

            try
            {
                string address = Regex.Replace(rAddress, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

                response = HttpWebService.MakeRequest(
                                        string.Format("{0}Locations/{1}?o=xml&key={2}", 
                                                        ID.MapsConsts.API_PREFIX, 
                                                        address, 
                                                        apiKey));
            }
            catch
            {
                throw new Exception(TX.Error.MAPS_CONNECTION_FAILED);
            }
       
            XmlDocument responseXml = new XmlDocument() { XmlResolver = null };
            responseXml.LoadXml(response);

            XmlNamespaceManager nameSpace = new XmlNamespaceManager(responseXml.NameTable);

            SharedFunctions.GenerateXmlNameSpace(ref nameSpace);

            XmlElement result = (XmlElement)responseXml.SelectSingleNode(
                                                            string.Format("//{0}StatusDescription", ID.MapsConsts.XML_SCHEMA_TAG), nameSpace);
            string status = result.InnerText;
            return status;
        }
    }
}
