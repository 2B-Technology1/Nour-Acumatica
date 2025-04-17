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
	public class ProductImagesDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<ProductsImageData>>
	{
		public virtual IChildRestDataProvider<ProductsImageData> CreateInstance(IBigCommerceRestClient restClient)
		{
			return new ProductImagesDataProvider(restClient); ;
		}
	}

	public class ProductImagesDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsImageData>
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/images";
		protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/images/{id}";

		public ProductImagesDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public virtual ProductsImageData Create(ProductsImageData productsImageData, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			return Create<ProductsImageData, ProductsImage>(productsImageData, segments)?.Data;
		}

		public virtual ProductsImageData Update(ProductsImageData productsImageData, string id,string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return Update<ProductsImageData, ProductsImage>(productsImageData, segments)?.Data;
		}
		public virtual bool Delete(string id, string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return base.Delete(segments);
		}

		public virtual IEnumerable<ProductsImageData> GetAll(string parentId, CancellationToken cancellationToken = default)
		{
			var segments = MakeParentUrlSegments(parentId);
			return GetAll<ProductsImageData, ProductsImageList>(null, segments, cancellationToken);
		}

		public virtual ProductsImageData GetByID(string id, string parentId)
		{
			var segments = MakeUrlSegments(id, parentId);
			return GetByID<ProductsImageData, ProductsImage>(segments).Data;
		}

		#region Not implemented 

		public virtual int Count(string parentId)
		{
			throw new System.NotImplementedException();
		}
		#endregion
	}
}
