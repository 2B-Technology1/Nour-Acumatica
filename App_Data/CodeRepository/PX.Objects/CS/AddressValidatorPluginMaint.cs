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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Compilation;
using PX.AddressValidator;
using PX.SM;
using PX.Data;
using PX.Objects.TX;
using static PX.Objects.CS.PXAddressLookup;
using System.Linq;
using System.Globalization;

namespace PX.Objects.CS
{
	public class AddressValidatorPluginMaint : PXGraph<AddressValidatorPluginMaint, AddressValidatorPlugin>
	{
		public PXSelect<AddressValidatorPlugin> Plugin;
		public PXSelectOrderBy<AddressValidatorPluginDetail, OrderBy<Asc<AddressValidatorPluginDetail.sortOrder>>> Details;

		public PXSelect<AddressValidatorPluginDetail, Where<AddressValidatorPluginDetail.addressValidatorPluginID,
			Equal<Current<AddressValidatorPlugin.addressValidatorPluginID>>>, OrderBy<Asc<AddressValidatorPluginDetail.sortOrder>>> SelectDetails;

		protected IEnumerable details()
		{
			ImportSettings();

			return SelectDetails.Select();
		}

		public virtual void ImportSettings()
		{
			if (Plugin.Current != null)
			{
				var provider = CreateAddressValidator(this, Plugin.Current);
				if (provider != null)
				{
					var settings = provider.DefaultSettings;

					InsertDetails(settings);
				}
			}
		}

		public PXAction<AddressValidatorPlugin> test;
		[PXUIField(DisplayName = "Test connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual void Test()
		{
			if (Plugin.Current != null)
			{
				Save.Press();

				var provider = CreateAddressValidator(this, Plugin.Current);
				if (provider != null)
				{
					var result = provider.Ping();
					if (result.IsSuccess)
					{
						Plugin.Ask(Plugin.Current, Messages.ConnectionAddressValidatorAskSuccessHeader, Messages.ConnectionAddressValidatorAskSuccess, MessageButtons.OK, MessageIcon.Information);
					}
					else
					{
						StringBuilder errorMessages = new StringBuilder();

						foreach (var message in result.Messages)
						{
							errorMessages.AppendLine(message);
						}

						if (errorMessages.Length > 0)
						{
							throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.TestFailed, errorMessages.ToString().TrimEnd()));
						}
					}
				}
			}
		}

		private void InsertDetails(IList<IAddressValidatorSetting> list)
		{
			var existingRecords = new Dictionary<string, AddressValidatorPluginDetail>();

			foreach (AddressValidatorPluginDetail detail in SelectDetails.Select())
			{
				existingRecords.Add(detail.SettingID.ToUpper(), detail);
			}

			foreach (var item in list)
			{
				AddressValidatorPluginDetail existingRecord;
				if (existingRecords.TryGetValue(item.SettingID.ToUpper(), out existingRecord))
				{
					var copy = PXCache<AddressValidatorPluginDetail>.CreateCopy(existingRecord);

					if (!string.IsNullOrEmpty(item.Description))
						copy.Description = item.Description;
					copy.ControlType = item.ControlType;
					copy.ComboValues = item.ComboValues;
					copy.SortOrder = item.SortOrder;

					if (existingRecord.Description != copy.Description || existingRecord.ControlTypeValue != copy.ControlTypeValue ||
					    existingRecord.ComboValuesStr != copy.ComboValuesStr || existingRecord.SortOrder != copy.SortOrder)
						SelectDetails.Update(copy);
				}
				else
				{
					var addressValidatorPluginDetail = new AddressValidatorPluginDetail
					{
						SettingID = item.SettingID.ToUpper(),
						SortOrder = item.SortOrder,
						Description = item.Description,
						Value = item.Value,
						ControlType = item.ControlType,
						ComboValues = item.ComboValues
					};

					SelectDetails.Insert(addressValidatorPluginDetail);
				}
			}
		}

		public static IAddressConnectedService CreateAddressValidator(PXGraph graph, AddressValidatorPlugin plugin)
		{
			IAddressConnectedService service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type addressValidatorType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (IAddressConnectedService)Activator.CreateInstance(addressValidatorType);

					PXSelectBase<AddressValidatorPluginDetail> select = new PXSelect<AddressValidatorPluginDetail, Where<AddressValidatorPluginDetail.addressValidatorPluginID, Equal<Required<AddressValidatorPluginDetail.addressValidatorPluginID>>>>(graph);
					PXResultset<AddressValidatorPluginDetail> resultset = select.Select(plugin.AddressValidatorPluginID);
					var list = new List<IAddressValidatorSetting>(resultset.Count);

					foreach (AddressValidatorPluginDetail item in resultset)
					{
						list.Add(item);
					}

					service.Initialize(list);

				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateAddressValidatorPlugin, ex.Message);
				}
			}

			return service;
		}

		public static IReadOnlyList<string> GetAddressValidatorPluginAttributes(PXGraph graph, string addressValidatorPluginID)
		{
			AddressValidatorPlugin plugin = PXSelect<AddressValidatorPlugin,
				Where<AddressValidatorPlugin.addressValidatorPluginID, Equal<Required<AddressValidatorPlugin.addressValidatorPluginID>>>>.Select(graph, addressValidatorPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindAddressValidatorPlugin, addressValidatorPluginID);

			IAddressValidator validator = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				try
				{
					Type addressValidatorType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					validator = (IAddressValidator)Activator.CreateInstance(addressValidatorType);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCreateAddressValidatorPlugin, ex.Message);
				}
			}

			return validator == null ? new List<string>().AsReadOnly() : validator.Attributes;
		}

		public static IAddressValidator CreateAddressValidator(PXGraph graph, string addressValidatorPluginID)
		{
			AddressValidatorPlugin plugin = AddressValidatorPlugin.PK.Find(graph, addressValidatorPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindAddressValidatorPlugin, addressValidatorPluginID);

			return (IAddressValidator)CreateAddressValidator(graph, plugin);
		}

		public static bool IsActive(PXGraph graph, string countryID)
		{
			var country = Country.PK.Find(graph, countryID);
			AddressValidatorPlugin m = AddressValidatorPlugin.PK.Find(graph, country?.AddressValidatorPluginID);
			return m?.IsActive ?? false;
		}

		protected virtual void AddressValidatorPlugin_PluginTypeName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			AddressValidatorPlugin row = e.Row as AddressValidatorPlugin;
			if (row == null) return;

			foreach (AddressValidatorPluginDetail detail in SelectDetails.Select())
			{
				SelectDetails.Delete(detail);
			}
		}

		protected virtual bool IsCountryISOValid(string countryISOCode)
		{
			Country country = PXSelect<Country, Where<Country.countryID, Equal<Required<Country.countryID>>>>.SelectWindowed(this, 0, 1, countryISOCode);
			if (country != null)
			{
				return true;
			}

			try
			{
				RegionInfo info = new RegionInfo(countryISOCode);
			}
			catch (ArgumentException)
			{
				throw new PXSetPropertyException(Messages.CountryCodeIsNotValid, countryISOCode);
			}
			return true;
		}

		/// <exclude/>
		protected virtual void AddressValidatorPluginDetail_Value_FieldVerifying(
			PXCache sender,
			PXFieldVerifyingEventArgs e)
		{
			AddressValidatorPluginDetail detail = e.Row as AddressValidatorPluginDetail;
			string newValue = e.NewValue as string;
			if (detail == null || string.IsNullOrEmpty(newValue)) return;

			AddressValidatorPluginDetail copy = PXCache<AddressValidatorPluginDetail>.CreateCopy(detail);
			copy.Value = newValue;

			Type pluginType = PXBuildManager.GetType(Plugin.Current.PluginTypeName, true);
			var service = (IAddressConnectedService)Activator.CreateInstance(pluginType);
			service.Initialize(new List<AddressValidatorPluginDetail> { copy });

			switch ((int)(detail.ControlTypeValue ?? 0))
			{
				case (int)AddressValidatorSettingControlType.CountriesISO:
					foreach (var countryCode in e.NewValue.ToString().Split(CountriesISOListHelper.DelimiterChars, StringSplitOptions.RemoveEmptyEntries).ToList())
					{
						if (IsCountryISOValid(countryCode))
						{
							continue;
						}
					}
					break;
				case (int)AddressValidatorSettingControlType.SingleCountryISO:
					IsCountryISOValid(e.NewValue.ToString());
					break;
				default:
					break;
			}
		}

		protected virtual void AddressValidatorPluginDetail_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			AddressValidatorPluginDetail row = e.Row as AddressValidatorPluginDetail;
			if (row != null)
			{
				string fieldName = typeof(AddressValidatorPluginDetail.value).Name;

				switch (row.ControlTypeValue)
				{
					case AddressValidatorPluginDetail.Combo:
						List<string> labels = new List<string>();
						List<string> values = new List<string>();
						foreach (var kv in row.ComboValues)
						{
							values.Add(kv.Key);
							labels.Add(kv.Value);
						}
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, AddressValidatorPluginDetail.ValueFieldLength, null, fieldName, false, 1, null,
																values.ToArray(), labels.ToArray(), true, null);
						break;
					case AddressValidatorPluginDetail.CheckBox:
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						break;
					case AddressValidatorPluginDetail.Password:
						if (e.ReturnState != null)
						{
							string strValue = e.ReturnState.ToString();
							string encripted = new string('*', strValue.Length);

							e.ReturnState = PXFieldState.CreateInstance(encripted, typeof(string), false, null, -1, null, null, null, fieldName,
									null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						}
						break;
					case AddressValidatorPluginDetail.Text:
						if (e.ReturnState != null)
						{
							if (row.SettingID == "PATH" || row.SettingID == "URL")
								e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, false, null, null, PXUIVisibility.Undefined, null, null, null);
						}
						break;
					default:
						break;
				}
			}
		}

		protected virtual void AddressValidatorPlugin_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			AddressValidatorPlugin row = e.Row as AddressValidatorPlugin;
			if (row == null) return;

			Country country = PXSelect<Country, Where<Country.addressValidatorPluginID, Equal<Current<AddressValidatorPlugin.addressValidatorPluginID>>>>.SelectWindowed(this, 0, 1);
			if (country != null)
			{
				throw new PXException(Messages.CountryFK);
			}
			PreferencesGeneral sitePref = PXSelect<PreferencesGeneral, Where<PreferencesGeneral.addressLookupPluginID, Equal<Current<AddressValidatorPlugin.addressValidatorPluginID>>>>.SelectWindowed(this, 0, 1);
			if (sitePref != null)
			{
				throw new PXException(Messages.AddressLookupSitePrefFK);
			}
			PortalSetup portalPref = PXSelect<PortalSetup, Where<PortalSetup.addressLookupPluginID, Equal<Current<AddressValidatorPlugin.addressValidatorPluginID>>>>.SelectWindowed(this, 0, 1);
			if (portalPref != null)
			{
				throw new PXException(Messages.AddressLookupPortalPrefFK);
			}
		}
	}
}
