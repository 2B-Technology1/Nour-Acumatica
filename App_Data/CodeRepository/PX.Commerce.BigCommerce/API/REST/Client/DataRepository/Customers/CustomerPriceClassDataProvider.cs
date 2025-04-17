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

using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class CustomerPriceClassRestDataProviderFactory : IBCRestDataProviderFactory<IParentRestDataProvider<CustomerGroupData>>
	{
		public virtual IParentRestDataProvider<CustomerGroupData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new CustomerPriceClassRestDataProvider(restClient);
		}
	}

	public class CustomerPriceClassRestDataProvider : RestDataProviderV2, IParentRestDataProvider<CustomerGroupData>
	{
		protected override string GetListUrl { get; } = "v2/customer_groups";

		protected override string GetSingleUrl { get; } = "v2/customer_groups/{id}";

		public CustomerPriceClassRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public virtual CustomerGroupData Create(CustomerGroupData group)
		{
			var newGroup = Create<CustomerGroupData>(group);
			return newGroup;
		}

		public virtual CustomerGroupData Update(CustomerGroupData group, string id)
		{
			var segments = MakeUrlSegments(id);
			return Update(group, segments);
		}

		public virtual IEnumerable<CustomerGroupData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll<CustomerGroupData>(filter, cancellationToken: cancellationToken);
		}

		public virtual CustomerGroupData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
			return GetByID<CustomerGroupData>(segments);
		}

		public virtual bool Delete(string id) => throw new System.NotImplementedException();

		public virtual bool Delete(string id, CustomerGroupData entity) => throw new System.NotImplementedException();
	}
}
