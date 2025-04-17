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
using System.Linq;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class CustomerRestDataProviderV3Factory : IBCRestDataProviderFactory<IParentRestDataProviderV3<CustomerData, FilterCustomers>>
	{
		public virtual IParentRestDataProviderV3<CustomerData, FilterCustomers> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new CustomerRestDataProviderV3(restClient);
		}
	}

	public class CustomerRestDataProviderV3 : RestDataProviderV3, IParentRestDataProviderV3<CustomerData, FilterCustomers>
	{
		protected override string GetListUrl { get; } = "v3/customers";

		protected override string GetSingleUrl { get; } = "v3/customers";

		private CustomerAddressRestDataProviderV3 customerAddressDataProviderV3;

		public CustomerRestDataProviderV3(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
			customerAddressDataProviderV3 = new CustomerAddressRestDataProviderV3(restClient);
		}

		public virtual IEnumerable<CustomerData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll<CustomerData, CustomerList>(filter, cancellationToken: cancellationToken);
		}

		public virtual CustomerData GetByID(string id) => GetByID(id, null);

		public virtual CustomerData GetByID(string id, FilterCustomers filter = null, CancellationToken cancellationToken = default)
		{
			if (filter == null) filter = new FilterCustomers { Include = "addresses,formfields" };

			filter.Id = id;

			var result = base.GetAll<CustomerData, CustomerList>(filter, cancellationToken: cancellationToken).FirstOrDefault();

			// if there are exactly 10 addresses returned included in the customer's data, send a separate request to get all addresses
			// of customer to make sure all addresses are available for further process as there's a limit of max 10 addresses returned
			// with customer's data
			if (result != null && filter.Include.Contains("addresses") && result.AddressCount > 10)
			{
				FilterAddresses addressFilter = new FilterAddresses { Include = "formfields", CustomerId = result.Id.ToString() };
				result.Addresses = customerAddressDataProviderV3.GetAll(addressFilter, cancellationToken: cancellationToken).ToList();
			}

			return result;
		}

		public virtual CustomerData Create(CustomerData customer)
		{
			CustomerList resonse = Create<CustomerData, CustomerList>(new CustomerData[] { customer }.ToList());
			return resonse?.Data?.FirstOrDefault();
		}

		public virtual CustomerData Update(CustomerData customer)
		{
			CustomerList resonse = Update<CustomerData, CustomerList>(new CustomerData[] { customer }.ToList());
			return resonse?.Data?.FirstOrDefault();
		}

		public virtual CustomerData Update(CustomerData customer, string id) => Update(customer);

		public virtual bool Delete(string id) => throw new System.NotImplementedException();

		public virtual bool Delete(string id, CustomerData entity) => throw new System.NotImplementedException();
	}
}
