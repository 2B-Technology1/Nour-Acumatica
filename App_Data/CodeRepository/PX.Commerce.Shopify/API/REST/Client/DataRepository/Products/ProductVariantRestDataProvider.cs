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

namespace PX.Commerce.Shopify.API.REST
{
	public class ProductVariantRestDataProviderFactory : ISPRestDataProviderFactory<IChildRestDataProvider<ProductVariantData>>
	{
		public virtual IChildRestDataProvider<ProductVariantData> CreateInstance(IShopifyRestClient restClient) => new ProductVariantRestDataProvider(restClient);
	}

	public class ProductVariantRestDataProvider : RestDataProviderBase, IChildRestDataProvider<ProductVariantData>
	{
		protected override string GetListUrl { get; } = "products/{parent_id}/variants.json";
		protected override string GetSingleUrl { get; } = "products/{parent_id}/variants/{id}.json"; //The same API url : variants/{id}.json
		protected string GetAllUrl { get; } = "variants.json";
		protected override string GetSearchUrl => throw new NotImplementedException();

		public ProductVariantRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public virtual ProductVariantData Create(ProductVariantData entity, string productId)
		{
			var segments = MakeParentUrlSegments(productId);
			return base.Create<ProductVariantData, ProductVariantResponse>(entity, segments);
		}

		public virtual ProductVariantData Update(ProductVariantData entity, string productId, string variantId)
		{
			var segments = MakeUrlSegments(variantId, productId);
			return Update<ProductVariantData, ProductVariantResponse>(entity, segments);
		}

		public virtual bool Delete(string productId, string variantId)
		{
			var segments = MakeUrlSegments(variantId, productId);
			return Delete(segments);
		}

		public virtual IEnumerable<ProductVariantData> GetAll(string productId, IFilter filter = null, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(productId);
			return GetAll<ProductVariantData, ProductVariantsResponse>(filter, segments, cancellationToken);
		}

		public virtual ProductVariantData GetByID(string productId, string variantId)
		{
			var segments = MakeUrlSegments(variantId, productId);
			return GetByID<ProductVariantData, ProductVariantResponse>(segments);
		}

		public virtual IEnumerable<ProductVariantData> GetAllWithoutParent(IFilter filter = null)
		{
			var request = BuildRequest(GetAllUrl, nameof(GetAllWithoutParent), null, filter);
			return ShopifyRestClient.GetAll<ProductVariantData, ProductVariantsResponse>(request);
		}
	}
}
