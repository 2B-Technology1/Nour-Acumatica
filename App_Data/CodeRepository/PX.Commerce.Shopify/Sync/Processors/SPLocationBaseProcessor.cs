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
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public abstract class SPLocationBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : LocationProcessorBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{
		protected SPLocationProcessor locationProcessor;

		protected ICustomerRestDataProvider<CustomerData> customerDataProvider;
		protected IChildRestDataProvider<CustomerAddressData> customerAddressDataProvider;
		protected List<Tuple<String, String, String>> formFieldsList;
		protected BCBinding currentBinding;
		protected SPRestClient client;

		#region Factories
		[InjectDependency]
		protected ISPRestDataProviderFactory<ICustomerRestDataProvider<CustomerData>> customerDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<CustomerAddressData>> customerAddressDataProviderFactory { get; set; }
		#endregion

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());
			customerAddressDataProvider = customerAddressDataProviderFactory.CreateInstance(client);
			customerDataProvider = customerDataProviderFactory.CreateInstance(client);
		}

		protected virtual CustomerLocation MapLocationImport(CustomerAddressData addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = new CustomerLocation();
			locationImpl.Custom = locationProcessor == null ? GetCustomFieldsForImport() : locationProcessor.GetCustomFieldsForImport();
			//locationImpl.Customer = customer.AcctCD?.Trim().ValueField();
			locationImpl.LocationName = (string.IsNullOrWhiteSpace(addressObj.Company) ? addressObj.Name : addressObj.Company).ValueField();
			locationImpl.ContactOverride = true.ValueField();
			locationImpl.AddressOverride = true.ValueField();

			//Contact
			Contact contactImpl = locationImpl.LocationContact = new Contact();
			contactImpl.CompanyName = addressObj.Company.ValueField();
			contactImpl.FirstName = addressObj.FirstName.ValueField();
			contactImpl.LastName = addressObj.LastName.ValueField();
			contactImpl.Attention = addressObj.Name.ValueField();
			contactImpl.Phone1 = addressObj.Phone.ValueField();

			//FullName equals to CompanyName field.
			contactImpl.FullName = addressObj.Company.ValueField();

			//Address
			Address addressImpl = contactImpl.Address = new Address();
			addressImpl.AddressLine1 = addressObj.Address1.ValueField() ?? string.Empty.ValueField();
			addressImpl.AddressLine2 = addressObj.Address2.ValueField() ?? string.Empty.ValueField();
			addressImpl.City = addressObj.City.ValueField();
			addressImpl.Country = addressObj.CountryCode.ValueField();
			if (!string.IsNullOrEmpty(addressObj.ProvinceCode))
			{
				addressImpl.State = GetHelper<SPHelper>().GetSubstituteLocalByExtern(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), addressObj.ProvinceCode, addressObj.ProvinceCode).ValueField();
			}
			else
				addressImpl.State = string.Empty.ValueField();
			addressImpl.PostalCode = addressObj.PostalCode?.ToUpperInvariant()?.ValueField();
			return locationImpl;
		}

		protected virtual CustomerAddressData MapLocationExport(MappedLocation addressObj, MappedCustomer customerObj)
		{
			CustomerLocation locationImpl = addressObj.Local;
			CustomerAddressData addressData = new CustomerAddressData();

			var result = PXSelectJoin<PX.Objects.CR.Location,
		InnerJoin<PX.Objects.CR.BAccount, On<PX.Objects.CR.Location.locationID, Equal<PX.Objects.CR.BAccount.defLocationID>>>,
		Where<PX.Objects.CR.BAccount.noteID, Equal<Required<PX.Objects.CR.BAccount.noteID>>>>.Select(this, customerObj.LocalID);


			//Contact
			Contact contactImpl = locationImpl.LocationContact;
			addressData.Company = contactImpl.FullName?.Value ?? locationImpl.LocationName?.Value;
			addressData.Name = contactImpl.Attention?.Value ?? string.Empty;

			addressData.Phone = contactImpl.Phone1?.Value ?? contactImpl.Phone2?.Value;

			//Address
			Address addressImpl = contactImpl.Address;
			addressData.Address1 = addressImpl.AddressLine1?.Value;
			addressData.Address2 = addressImpl.AddressLine2?.Value;
			addressData.City = addressImpl.City?.Value;
			addressData.CountryCode = addressImpl.Country?.Value;
			addressData.Province = GetHelper<SPHelper>().GetSubstituteExternByLocal(BCSubstitute.GetValue(Operation.ConnectorType, BCSubstitute.State), addressImpl.State?.Value, addressImpl.State?.Value);
			addressData.PostalCode = addressImpl.PostalCode?.Value;
			if ((result.FirstOrDefault().GetItem<PX.Objects.CR.Location>()).NoteID == locationImpl.NoteID.Value)
				addressData.Default = true;
			else
				addressData.Default = false;
			return addressData;
		}
	}
}
