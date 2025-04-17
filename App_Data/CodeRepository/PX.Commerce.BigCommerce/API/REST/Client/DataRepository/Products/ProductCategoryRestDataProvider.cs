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

using PX.Commerce.Core.Model;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ProductCategoryRestDataProviderFactory : IBCRestDataProviderFactory<IParentRestDataProvider<ProductCategoryData>>
	{
		public virtual IParentRestDataProvider<ProductCategoryData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new ProductCategoryRestDataProvider(restClient);
		}
	}

	public class ProductCategoryRestDataProvider : RestDataProviderV3, IParentRestDataProvider<ProductCategoryData>
    {
        private const string id_string = "id";

        protected override string GetListUrl { get; }   = "v3/catalog/categories";
        protected override string GetSingleUrl { get; } = "v3/catalog/categories/{id}";

        public ProductCategoryRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region  IParentRestDataProvider  
        public virtual IEnumerable<ProductCategoryData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
        {
            return GetAll<ProductCategoryData, ProductCategoryList>(filter);
        }

		public virtual ProductCategoryData GetByID(string id)
        {
            var segments = MakeUrlSegments(id);
            return GetByID<ProductCategoryData, ProductCategory>(segments).Data;
        }

        public virtual bool Delete(string id)
        {
            var segments = MakeUrlSegments(id.ToString());
            return base.Delete(segments);
        }
		public virtual bool Delete(string id, ProductCategoryData productCategoryData)
		{
			return Delete(id);
		}

		public virtual ProductCategoryData Create(ProductCategoryData category)
        {
            var productCategory  = new ProductCategory{Data = category};
            return Create<ProductCategoryData, ProductCategory>(productCategory).Data;
        }

		public virtual ProductCategoryData Update(ProductCategoryData category, string id)
        {
            var segments = MakeUrlSegments(id);
            return Update<ProductCategoryData, ProductCategory>(category, segments).Data;
        }

		#endregion
	}
}
