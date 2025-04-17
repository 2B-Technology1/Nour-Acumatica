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
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ProductRestDataProviderFactory : IBCRestDataProviderFactory<IStockRestDataProvider<ProductData>>
	{
		public virtual IStockRestDataProvider<ProductData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new ProductRestDataProvider(restClient);
		}
	}

	public class ProductRestDataProvider : RestDataProviderV3, IStockRestDataProvider<ProductData>
	{
		private const string id_string = "id";
		protected override string GetListUrl { get; } = "v3/catalog/products";
		//protected override string GetFullListUrl { get; } = "v3/catalog/products?include=variants,images,custom_fields,primary_image,bulk_pricing_rules";
		protected override string GetSingleUrl { get; } = "v3/catalog/products/{id}";

		public ProductRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		#region IParentRestDataProvider
		public virtual IEnumerable<ProductData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default)
		{
			return GetAll<ProductData, ProductList>(filter, cancellationToken: cancellationToken);
		}
		public virtual ProductData GetByID(string id)
		{
			return GetByID(id, null);
		}

		public virtual ProductData GetByID(string id, IFilter filter = null)
		{
			var segments = MakeUrlSegments(id);
			var result = GetByID<ProductData, Product>(segments, filter);
			return result.Data;
		}

		public virtual ProductData Create(ProductData productData)
		{
				var product = new Product { Data = productData };
				var result = base.Create<ProductData, Product>(product);
				return result.Data;
		}

		public virtual bool Delete(string id)
		{
			var segments = MakeUrlSegments(id.ToString());
			return Delete(segments);
		}

		public virtual bool Delete(string id, ProductData productData)
		{
			return Delete(id);
		}

		public virtual ProductData Update(ProductData productData, string id)
		{
			var segments = MakeUrlSegments(id);
			var result = Update<ProductData, Product>(productData, segments);
			return result.Data;
		}

		public virtual void UpdateAllQty(List<ProductQtyData> productDatas, Action<ItemProcessCallback<ProductQtyData>> callback)
		{
			var product = new ProductQtyList { Data = productDatas };
			UpdateAll<ProductQtyData, ProductQtyList>(product, new UrlSegments(), callback);
		}
		public virtual void UpdateAllRelations(List<RelatedProductsData> productDatas, Action<ItemProcessCallback<RelatedProductsData>> callback)
		{
			var product = new RelatedProductsList { Data = productDatas };
			UpdateAll<RelatedProductsData, RelatedProductsList>(product, new UrlSegments(), callback);
		}
		#endregion
	}
}
