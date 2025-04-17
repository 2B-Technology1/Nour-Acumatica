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
using System;
using System.Collections.Generic;
using System.Threading;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ProductVideoDataProviderFactory : IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVideo>>
	{
		public virtual IChildRestDataProvider<ProductsVideo> CreateInstance(IBigCommerceRestClient restClient) => new ProductVideoDataProvider(restClient);
	}
	public class ProductVideoDataProvider : RestDataProviderV3, IChildRestDataProvider<ProductsVideo>
	{
		protected override string GetListUrl { get; } = "v3/catalog/products/{parent_id}/videos";

		protected override string GetSingleUrl { get; } = "v3/catalog/products/{parent_id}/videos/{id}";

		public ProductVideoDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public virtual ProductsVideo Create(ProductsVideo productsVideo, string parentId)
		{
			var segments = MakeParentUrlSegments(parentId);
			var productVideo = new ProductVideoData { Data = productsVideo };
			return Create<ProductsVideo, ProductVideoData>(productVideo, segments).Data;
		}

		#region Not Implemented
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual ProductsVideo Update(ProductsVideo productsVideo, string id, string parentId)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual int Count(string parentId)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual bool Delete(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual ProductsVideo GetByID(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public virtual IEnumerable<ProductsVideo> GetAll(string externID, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
