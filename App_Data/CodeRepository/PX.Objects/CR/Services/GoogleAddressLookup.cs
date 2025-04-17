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

using PX.AddressValidator;
using PX.CCProcessingBase.Attributes;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CR.Services;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Messages = PX.Objects.CR.Messages;

namespace PX.AddressLookup
{
	[PXDisplayTypeName("Google Maps", typeof(FeaturesSet.addressLookup))]
	public class GoogleAddressLookup : IAddressLookupService
	{
		/// <summary>
		/// https://developers.google.com/places/web-service/autocomplete#place_autocomplete_status_codes
		/// OK indicates that no errors occurred and at least one result was returned.
		/// ZERO_RESULTS indicates that the search was successful but returned no results.This may occur if the search was passed a bounds in a remote location.
		/// OVER_QUERY_LIMIT indicates that you are over your quota.
		/// REQUEST_DENIED indicates that your request was denied, generally because of lack of a valid key parameter.
		/// INVALID_REQUEST generally indicates that the input parameter is missing.
		/// UNKNOWN_ERROR indicates a server-side error; trying again may be successful
		/// </summary>
		public static class GoogleReponseStatus
		{
			public const string OK = "OK";
			public const string REQUEST_DENIED = "REQUEST_DENIED";
		}

		public static readonly string AddressValidatorID = "GoogleAddressLookup";
		#region Properties
		protected string ApiKey { get; set; }
		protected string Countries { get; set; }
		protected string Url { get; set; }
		protected string Path { get; set; }
		#endregion

		#region IAddressConnectedService
		public virtual void Initialize(IEnumerable<IAddressValidatorSetting> settings)
		{
			foreach (IAddressValidatorSetting setting in settings)
			{
				if ("API_KEY".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					ApiKey = setting.Value;
				}

				if ("COUNTRIES".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					if (string.IsNullOrEmpty(setting.Value))
					{
						Countries = "";
					}
					else
					{
						Countries = String.Join(",", from iso in setting.Value.Split(CountriesISOListHelper.DelimiterChars, StringSplitOptions.RemoveEmptyEntries) select String.Format("'{0}'", iso));
					}
				}

				if ("URL".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					Url = setting.Value;
				}
				if ("PATH".Equals(setting.SettingID, StringComparison.InvariantCultureIgnoreCase))
				{
					Path = setting.Value;
				}
			}
		}

		public virtual PingResult Ping()
		{
			List<string> messages = new List<string>();
			bool responseStatus = true;

			string url = string.Format("https://maps.googleapis.com/maps/api/place/autocomplete/xml?input=Acuma&types=address&key={0}", ApiKey);
			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(url);
			string status = xDoc.SelectSingleNode("AutocompletionResponse/status")?.InnerText;
			if (string.IsNullOrEmpty(status))
			{
				responseStatus = false;
				messages.Add(Messages.AutocompleteServiceTestFailed);
				messages.Add(url);
			}
			else if (!status.Equals(GoogleReponseStatus.OK))
			{
				responseStatus = false;
				messages.Add("");
				messages.Add(status);
				messages.Add(xDoc.SelectSingleNode("AutocompletionResponse/error_message")?.InnerText);
			}
			PingResult result = new PingResult
			{
				IsSuccess = responseStatus,
				Messages = messages.ToArray()
			};
			return result;
		}

		public virtual IAddressValidatorSetting[] DefaultSettings
		{
			get
			{
				IAddressValidatorSetting[] settings = new IAddressValidatorSetting[]
				{
					new AddressValidatorSetting(
							AddressValidatorID,
							"API_KEY",
							1,
							"API Key",
							"",
							AddressValidatorSettingControlType.Password),
					new AddressValidatorSetting(
							AddressValidatorID,
							"COUNTRIES",
							2,
							"Country restriction",
							"US, CA, GB, MX",
							AddressValidatorSettingControlType.CountriesISO),
					new AddressValidatorSetting(
							AddressValidatorID,
							"URL",
							3,
							"URL",
							"https://maps.googleapis.com/maps/api/js?key=${API_KEY}&libraries=places&language=EN",
							AddressValidatorSettingControlType.Text),
					new AddressValidatorSetting(
							AddressValidatorID,
							"PATH",
							4,
							"PATH",
							"./map-api/google-api",
							AddressValidatorSettingControlType.Text)
				};
				return settings;
			}
		}
		#endregion

		#region IAddressLookupService
		public string GetClientScript(PXGraph graph)
		{
			string language = "";
			if (string.IsNullOrEmpty(graph?.Culture?.TwoLetterISOLanguageName) == false)
			{
				language = graph.Culture.TwoLetterISOLanguageName;
			}
			string sSciprt = "";
			if (string.IsNullOrEmpty(Countries) == false)
			{
				sSciprt += string.Format("<script type='text/javascript'>  var componentRestrictions_country = [{0}];  </script>\n", Countries);
			}
			sSciprt += string.Format("<script type='text/javascript' src='https://maps.googleapis.com/maps/api/js?key={0}&libraries=places&language={1}' async defer></script>"
					, ApiKey
					, language) +
				@"<script type='text/javascript' src='..\..\Scripts\AddressLookup\GooglePlacesAPI.js' async defer></script>";
			return sSciprt;
		}
		#endregion
	}

	public class AddressValidatorPluginMaintGoogleAddressLookupGraphExt : PXGraphExtension<AddressValidatorPluginMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.addressLookup>();
		}

		protected virtual void _(Events.FieldVerifying<AddressValidatorPluginDetail, AddressValidatorPluginDetail.value> e)
		{
			AddressValidatorPluginDetail row = e.Row;
			if (row != null &&
				string.IsNullOrEmpty(e?.NewValue?.ToString()) == false)
			{
				AddressValidatorPlugin addressValidatorPlugin = 
					PXSelect<
						AddressValidatorPlugin, 
						Where<AddressValidatorPlugin.addressValidatorPluginID, Equal<Required<AddressValidatorPlugin.addressValidatorPluginID>>>>
						.SelectWindowed(Base, 0, 1, row.AddressValidatorPluginID);
				if (addressValidatorPlugin == null ||
					addressValidatorPlugin.PluginTypeName.Contains(GoogleAddressLookup.AddressValidatorID) == false)
				{
					return;
				}

				switch ((int)(row.ControlTypeValue ?? 0))
				{
					case (int)AddressValidatorSettingControlType.CountriesISO:
						int numCountries = e.NewValue.ToString().Split(CountriesISOListHelper.DelimiterChars, StringSplitOptions.RemoveEmptyEntries).Length;
						if (numCountries > 5)
						{
							throw new PXSetPropertyException(Messages.GoogleAddressLookupCountryLimit);
						}
						break;
					default:
						break;
				}
			}
		}
	}
}
