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
	public class ProductBulkPricingRestDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<ProductsBulkPricingRules>>
	{
		public virtual IChildRestDataProvider<ProductsBulkPricingRules> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new ProductBulkPricingRestDataProvider(restClient);
		}
	}
	public class ProductBatchBulkRestDataProviderFactory : IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice>>
	{
		public virtual IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new ProductBatchBulkRestDataProvider(restClient);
		}
	}

	public class ProductBulkPricingRestDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsBulkPricingRules>
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/bulk-pricing-rules";

		protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/bulk-pricing-rules/{id}";

		public ProductBulkPricingRestDataProvider(IBigCommerceRestClient client)
		{
			_restClient = client;
		}
		public virtual ProductsBulkPricingRules Create(ProductsBulkPricingRules pricingRules, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			return Create<ProductsBulkPricingRules, BulkPricing>(pricingRules, segments).Data;
		}
		public virtual IEnumerable<ProductsBulkPricingRules> GetAll(string parentId, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(parentId);
			return GetAll<ProductsBulkPricingRules, BulkPricingList>(urlSegments: segments, cancellationToken: cancellationToken);
		}

		public virtual bool Delete(string id, string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return Delete(urlSegments: segments);
		}

		public virtual ProductsBulkPricingRules Update(ProductsBulkPricingRules productData, string id, string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			var result = Update<ProductsBulkPricingRules, BulkPricing>(productData, segments);
			return result.Data;
		}

		public virtual ProductsBulkPricingRules GetByID(string id, string parentId)
		{
			return GetAll(parentId).FirstOrDefault(item => item.Id.ToString() == id);
		}
	}	

	public class ProductBatchBulkRestDataProvider : ProductBulkPricingRestDataProvider, IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice>
	{
		protected override string GetListUrl { get; } = "v3/catalog/products?include=bulk_pricing_rules";
		public ProductBatchBulkRestDataProvider(IBigCommerceRestClient restClient) : base(restClient)
		{
		}

		public virtual void UpdateAll(List<BulkPricingWithSalesPrice> productDatas, Action<ItemProcessCallback<BulkPricingWithSalesPrice>> callback)
		{
			var product = new BulkPricingListWithSalesPrice { Data = productDatas };
			UpdateAll<BulkPricingWithSalesPrice, BulkPricingListWithSalesPrice>(product, new UrlSegments(), callback);
		}

		public IEnumerable<BulkPricingWithSalesPrice> GetVariants(string parentId, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
