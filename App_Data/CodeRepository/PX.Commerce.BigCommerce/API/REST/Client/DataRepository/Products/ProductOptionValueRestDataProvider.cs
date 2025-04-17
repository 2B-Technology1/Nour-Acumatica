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
	public class ProductOptionValueRestDataProviderFactory : IBCRestDataProviderFactory<ISubChildRestDataProvider<ProductOptionValueData>>
	{
		public virtual ISubChildRestDataProvider<ProductOptionValueData> CreateInstance(IBigCommerceRestClient restClient) => new ProductOptionValueRestDataProvider(restClient);
	}

	public class ProductOptionValueRestDataProvider : RestDataProviderV3, ISubChildRestDataProvider<ProductOptionValueData>
    {
        protected override string GetListUrl { get; }   = "v3/catalog/products/{product_id}/options/{option_id}/values";
        protected override string GetSingleUrl { get; } = "v3/catalog/products/{product_id}/options/{option_id}/values/{value_id}";

        private const string product_id = "product_id";
        private const string option_id  = "option_id";
        private const string value_id   = "value_id";

        public ProductOptionValueRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
            _restClient = restClient;
		}

        #region ISubChildRestDataProvider  
        public virtual IEnumerable<ProductOptionValueData> GetAll(string productId, string optionId, CancellationToken cancellationToken = default)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);

            return base.GetAll<ProductOptionValueData, ProductsOptionValueList>(null, segments, cancellationToken);
        }

		public virtual ProductOptionValueData GetByID(string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);

            return base.GetByID<ProductOptionValueData, ProductsOptionValue>(segments).Data;
        }

		public virtual ProductOptionValueData Create(ProductOptionValueData productsOptionValueData, string productId, string optionId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            var productsOptionValue = new ProductsOptionValue {Data = productsOptionValueData};
            
            return base.Create<ProductOptionValueData, ProductsOptionValue>(productsOptionValue, segments).Data;
        }

		public virtual ProductOptionValueData Update(ProductOptionValueData productsOptionValueData, string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);
            var productsOptionValue = new ProductsOptionValue {Data = productsOptionValueData};
            return Update<ProductOptionValueData, ProductsOptionValue>(productsOptionValue, segments).Data;
        }

		public virtual bool Delete(string productId, string optionId, string valueId)
        {
            var segments = new UrlSegments();
            segments.Add(product_id, productId);
            segments.Add(option_id, optionId);
            segments.Add(value_id, valueId);

            return base.Delete(segments);
        }
        #endregion
    }
}
