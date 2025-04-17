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
	public class ProductsOptionRestDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<ProductsOptionData>>
	{
		public virtual IChildRestDataProvider<ProductsOptionData> CreateInstance(IBigCommerceRestClient restClient) => new ProductsOptionRestDataProvider(restClient);
	}

	public class ProductsOptionRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsOptionData>
    {
        protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/options";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/options/{id}";

        public ProductsOptionRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

		#region IChildRestDataProvider
		public virtual ProductsOptionData Create(ProductsOptionData productsOptionData, string parentId)
        {
            var segments = MakeParentUrlSegments(parentId);
            var productsOption = new ProductsOption { Data = productsOptionData };
            return Create<ProductsOptionData, ProductsOption>(productsOption, segments).Data;
        }

		public virtual ProductsOptionData Update(ProductsOptionData productsOptionData, string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return Update<ProductsOptionData, ProductsOption>(productsOptionData, segments).Data;
        }

		public virtual bool Delete(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return base.Delete(segments);
        }

        public virtual ProductsOptionData GetByID(string id, string parentId)
        {
            var segments = MakeUrlSegments(id, parentId);
            return GetByID<ProductsOptionData, ProductsOption>(segments).Data;
        }

		public virtual IEnumerable<ProductsOptionData> GetAll(string parentId, CancellationToken cancellationToken = default)
        {
            var segments = MakeParentUrlSegments(parentId);
            return GetAll<ProductsOptionData, ProductsOptionList>(null, segments, cancellationToken);
        }
        #endregion
    }
}
