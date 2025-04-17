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

using PX.Commerce.Objects;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class TaxDataProviderFactory : IBCRestDataProviderFactory<IParentReadOnlyRestDataProvider<ProductsTaxData>>
	{
		public virtual IParentReadOnlyRestDataProvider<ProductsTaxData> CreateInstance(IBigCommerceRestClient restClient) => new TaxDataProvider(restClient);
	}

	public class TaxDataProvider : RestDataProviderV3, IParentReadOnlyRestDataProvider<ProductsTaxData>
	{
		protected override string GetListUrl { get; } = "v2/tax_classes";

		protected override string GetSingleUrl { get; } = "v2/tax_classes/{id}";

		public TaxDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}
		public virtual ProductsTaxData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
			var result = base.GetByID<ProductsTaxData, ProductsTax>(segments);
			return result.Data;
		}
		public virtual List<ProductsTaxData> GetAll()
		{
			var request = _restClient.MakeRequest(GetListUrl);
			var result = _restClient.Get<List<ProductsTaxData>>(request);
			result.Add(new ProductsTaxData() { Id = 0, Name = BCObjectsConstants.DefaultTaxClass });
			return result;
		}

		public virtual IEnumerable<ProductsTaxData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll();
		}
	}
}
