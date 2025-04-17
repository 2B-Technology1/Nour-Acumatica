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

namespace PX.Commerce.BigCommerce.API.REST
{
	public class VariantImageDataProviderFactory : IBCRestDataProviderFactory<IVariantImageDataProvider>
	{
		public virtual IVariantImageDataProvider CreateInstance(IBigCommerceRestClient restClient) => new VariantImageDataProvider(restClient);
	}

	public class VariantImageDataProvider : RestDataProviderV3, IVariantImageDataProvider
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/variants/{id}/image";
		protected override string GetSingleUrl { get; } = string.Empty;

		public VariantImageDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public virtual ProductsImageData Create(ProductsImageData productsImageData, string parentId, string id)
		{
			var segments = MakeUrlSegments(id, parentId);
			var productsImage = new ProductsImage { Data = productsImageData };
			return Create<ProductsImageData, ProductsImage>(productsImage, segments).Data;
		}
	}
}
