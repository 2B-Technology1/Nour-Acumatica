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

using PX.Api.ContractBased.Models;
using PX.Commerce.BigCommerce.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce
{
	public abstract class BCLocationBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : LocationProcessorBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected BCLocationProcessor locationProcessor;		
		protected IParentRestDataProviderV3<CustomerData, FilterCustomers> customerDataProviderV3;		
		protected IParentRestDataProviderV3<CustomerAddressData, FilterAddresses> customerAddressDataProviderV3;		
		protected IUpdateAllParentRestDataProvider<CustomerFormFieldData> customerFormFieldRestDataProvider;		
		protected IRestDataReader<List<States>> statesProvider;		
		protected IRestDataReader<List<Countries>> countriesProvider;
		protected Lazy<List<Tuple<String, String, String>>> formFieldsList;
		protected Dictionary<string, List<States>> countriesAndStates;
		protected BCRestClient client;

		#region Factories
		[InjectDependency]
		protected IBCRestDataProviderFactory<IParentRestDataProviderV3<CustomerData, FilterCustomers>> customerDataProviderV3Factory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IParentRestDataProviderV3<CustomerAddressData, FilterAddresses>> customerAddressDataProviderV3Factory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IUpdateAllParentRestDataProvider<CustomerFormFieldData>> customerFormFieldRestDataProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IRestDataReader<List<States>>> statesProviderFactory { get; set; }
		[InjectDependency]
		protected IBCRestDataProviderFactory<IRestDataReader<List<Countries>>> countriesProviderFactory { get; set; }
		#endregion

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			client = BCConnector.GetRestClient(GetBindingExt<BCBindingBigCommerce>());
			customerDataProviderV3 = customerDataProviderV3Factory.CreateInstance(client);
			customerAddressDataProviderV3 = customerAddressDataProviderV3Factory.CreateInstance(client);
			customerFormFieldRestDataProvider = customerFormFieldRestDataProviderFactory.CreateInstance(client);
			statesProvider = statesProviderFactory.CreateInstance(client);
			countriesProvider = countriesProviderFactory.CreateInstance(client);
			formFieldsList = new Lazy<List<Tuple<String, String, String>>>(() =>
			{
				return ConnectorHelper.GetConnectorSchema(operation.ConnectorType, operation.Binding, operation.EntityType)?.FormFields ?? new List<Tuple<string, string, string>>();
			}, true);
			if (countriesAndStates == null)
			{
				var states = statesProvider.Get();
				countriesAndStates =
					countriesProvider.Get().ToDictionary(
						x => x.CountryCode,
						x => states.Where(i => i.CountryID == x.ID).ToList());
			}
		}

		protected virtual CustomerLocation MapLocationImport(CustomerAddressData addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = new CustomerLocation();
			locationImpl.Custom = locationProcessor == null ? GetCustomFieldsForImport() : locationProcessor.GetCustomFieldsForImport();

			//Location
			string firstLastName = CustomerNameResolver(addressObj.FirstName, addressObj.LastName, (int)customerObj.Extern.Id);
			locationImpl.LocationName = (String.IsNullOrEmpty(addressObj.Company) ? firstLastName : addressObj.Company).ValueField();
			locationImpl.ContactOverride = true.ValueField();
			locationImpl.AddressOverride = true.ValueField();
			//Contact
			Contact contactImpl = locationImpl.LocationContact = new Contact();
			contactImpl.FirstName = addressObj.FirstName.ValueField();
			contactImpl.LastName = addressObj.LastName.ValueField();
			contactImpl.Attention = firstLastName.ValueField();
			contactImpl.Phone1 = new StringValue { Value = addressObj.Phone };
			contactImpl.FullName = addressObj.Company?.ValueField();

			//Address
			Address addressImpl = contactImpl.Address = new Address();
			addressImpl.AddressLine1 = GetHelper<BCHelper>().CleanAddress(addressObj.Address1 ?? string.Empty).ValueField();
			addressImpl.AddressLine2 = GetHelper<BCHelper>().CleanAddress(addressObj.Address2 ?? string.Empty).ValueField();
			addressImpl.City = addressObj.City.ValueField();
			addressImpl.Country = addressObj.CountryCode.ValueField();
			if (!string.IsNullOrEmpty(addressObj.State))
			{
				var stateValue = GetExternalStateInfo(addressObj.CountryCode, addressObj.State);
				addressImpl.State = GetHelper<BCHelper>().GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), stateValue, stateValue).ValueField();
			}
			else
				addressImpl.State = string.Empty.ValueField();
			addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();
			return locationImpl;
		}
	
		protected virtual CustomerAddressData MapLocationExport(MappedLocation addressObj)
		{
			CustomerLocation locationImpl = addressObj.Local;
			CustomerAddressData addressData = new CustomerAddressData();

			
			//Contact
			Contact contactImpl = locationImpl.LocationContact;
			addressData.Company = contactImpl.FullName?.Value ?? locationImpl.LocationName?.Value;
			string fullName = new String[] {
				contactImpl.Attention?.Value,
				locationImpl.LocationName?.Value,
			 contactImpl.FullName?.Value
			}.FirstNotEmpty();
			if (string.IsNullOrWhiteSpace(fullName))
			{
				throw new PXException(BCMessages.CustomerLocationNameIsEmpty);
			}
			addressData.FirstName = fullName.FieldsSplit(0, fullName);
			addressData.LastName = fullName.FieldsSplit(1, fullName);
			addressData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;

			//Address
			Address addressImpl = contactImpl.Address;
			addressData.Address1 = addressImpl.AddressLine1?.Value;
			addressData.Address2 = addressImpl.AddressLine2?.Value;
			addressData.City = addressImpl.City?.Value;
			addressData.CountryCode = addressImpl.Country?.Value;
			if (!string.IsNullOrEmpty(addressImpl.State?.Value))
			{
				var stateValue = GetHelper<BCHelper>().GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), addressImpl.State.Value, addressImpl.State.Value);
				addressData.State = countriesAndStates.FirstOrDefault(i => i.Key.Equals(addressImpl.Country.Value)).Value.FirstOrDefault(
					i => string.Equals(i.StateID, stateValue, StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(i.State, stateValue, StringComparison.OrdinalIgnoreCase))?.State;
			}
			if (String.IsNullOrEmpty(addressData.State) && 
				countriesAndStates.FirstOrDefault(i => i.Key.Equals(addressImpl.Country.Value)).Value != null &&
				countriesAndStates.FirstOrDefault(i => i.Key.Equals(addressImpl.Country.Value)).Value.Count > 0)
				throw new PXException(BCMessages.NoValidStateForAddress, addressImpl.Country.Value, addressImpl.State?.Value);

			addressData.PostalCode = addressImpl.PostalCode?.Value;
			return addressData;
		}

		public virtual string CustomerNameResolver(string firstName, string lastName, int id)
		{
			if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
				throw new PXException(BCMessages.CustomerNameIsEmpty, id);
			if (firstName.Equals(lastName))
				return firstName;
			return String.Concat(firstName, " ", lastName);
		}

		public override List<(string fieldName, string fieldValue)> GetExternCustomFieldList(BCEntity entity, ExternCustomFieldInfo customFieldInfo)
		{
			List<(string fieldName, string fieldValue)> fieldsList = new List<(string fieldName, string fieldValue)>();
			SchemaInfo extEntitySchema = ConnectorHelper.GetConnectorSchema(entity.ConnectorType, entity.BindingID, entity.EntityType);
			foreach (var formField in extEntitySchema.FormFields?.Where(x => x.Item1 == entity.EntityType))
			{
				fieldsList.Add((formField.Item2, formField.Item2));
			}
			return fieldsList;
		}

		public object GetCustomerFormFields(IList<CustomerFormFieldData> customerFormFields, string fieldName)
		{
			if (customerFormFields?.Count > 0)
			{
				var formField = customerFormFields.Where(x => x.Name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
				if (formField.Value is Newtonsoft.Json.Linq.JArray)
				{
					var fieldValues = formField?.Value as Newtonsoft.Json.Linq.JArray;
					if (fieldValues == null || fieldValues.Count == 0) return null;
					return string.Join(",", fieldValues.ToObject<string[]>());
				}
				return formField?.Value;
			}
			return null;
		}

		public object SetCustomerFormFields(object targetData, string targetObject, string targetField, string sourceObject, object sourceValue)
		{
			dynamic value = null;
			if (sourceValue != null && !(sourceValue is string && string.IsNullOrWhiteSpace(sourceValue.ToString())))
			{
				var collectionType = sourceValue.GetType().IsGenericType &&
						(sourceValue.GetType().GetGenericTypeDefinition() == typeof(List<>) ||
						 sourceValue.GetType().GetGenericTypeDefinition() == typeof(IList<>));
				var formFieldType = formFieldsList.Value.Where(x => x.Item1 == Operation.EntityType && string.Equals(x.Item2, targetField?.Trim(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.Item3;
				if (collectionType && formFieldType == BCExternCustomFieldAttribute.JArray)
				{
					value = new List<string>();
					foreach (object item in (IList<object>)sourceValue)
					{
						((List<string>)value).Add(Convert.ToString(item));
					}
				}
				else if (!collectionType && formFieldType == BCExternCustomFieldAttribute.JArray)
				{
					var strValue = Convert.ToString(sourceValue);
					if (strValue.Length > 1 && strValue.Contains(","))
						value = strValue.Split(',');
					else if (strValue.Length > 1 && strValue.Contains(";"))
						value = strValue.Split(';');
					else
						value = strValue == null ? new List<string>() : new List<string> { strValue };
				}
				else if (collectionType && formFieldType == BCExternCustomFieldAttribute.Value)
				{
					value = string.Empty;
					foreach (object item in (IList<object>)sourceValue)
					{
						value = (string)value + (Convert.ToString(item));
					}
				}
				else
				{
					value = Convert.ToString(sourceValue);
				}
			}
			return value;
		}

		public override void ValidateExternCustomField(BCEntity entity, ExternCustomFieldInfo customFieldInfo, string sourceObject, string sourceField, string targetObject, string targetField, EntityOperationType direction)
		{
			if (!string.IsNullOrEmpty(sourceField) && sourceField.StartsWith("=") && direction == EntityOperationType.ImportMapping)
			{
				throw new PXException(BCMessages.InvalidSourceFieldFormFields);
			}
		}

		public virtual string GetExternalStateInfo(string countryCode, string stateName)
		{
			States seekenState = null;
			//Search State info by Country code first, otherwise search State info from all States with State name
			if (!string.IsNullOrEmpty(countryCode) && countriesAndStates.TryGetValue(countryCode, out List<States> statesList))
			{
				seekenState = statesList.FirstOrDefault(i => string.Equals(i.StateID, stateName, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(i.State, stateName, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				//Find State info without CountryCode, if there is only one record, return State. Otherwise return original state name.
				var availableStatesList = countriesAndStates.Values.SelectMany(x => x)?.Where(i => string.Equals(i.StateID, stateName, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(i.State, stateName, StringComparison.OrdinalIgnoreCase));
				if(availableStatesList?.Count() == 1)
				{
					seekenState = availableStatesList.First();
				}
			}

			return seekenState?.StateID ?? stateName;
		}
	}
}
