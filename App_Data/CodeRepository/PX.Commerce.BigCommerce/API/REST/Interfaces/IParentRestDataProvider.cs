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
	public interface IParentReadOnlyRestDataProvider<ReturnType> where ReturnType : class
	{
		IEnumerable<ReturnType> GetAll(IFilter filter = null, CancellationToken cancellationToken = default);

		ReturnType GetByID(string id);
	}

	public interface IParentRestDataProvider<ReturnType> : IParentReadOnlyRestDataProvider<ReturnType> where ReturnType : class
	{
		ReturnType Create(ReturnType entity);

		ReturnType Update(ReturnType entity, string id);

		bool Delete(string id);

		bool Delete(string id, ReturnType entity);
	}

	public interface IParentRestDataProviderV3<ReturnType, FilterType> : IParentRestDataProvider<ReturnType>
		where ReturnType : class
		where FilterType : Filter
	{
		ReturnType GetByID(string id, FilterType filter = null, CancellationToken cancellationToken = default);
	}	

	public interface IUpdateAllParentRestDataProvider<ReturnType> : IParentReadOnlyRestDataProvider<ReturnType> where ReturnType : Core.BCAPIEntity
	{
		List<ReturnType> UpdateAll(List<ReturnType> customersCustomFieldDataList);
	}

	public interface IStockRestDataProvider<ReturnType> : IParentRestDataProvider<ReturnType> where ReturnType : class
	{
		ReturnType GetByID(string id, IFilter filter = null);

		void UpdateAllQty(List<ProductQtyData> listOfProductData, Action<ItemProcessCallback<ProductQtyData>> callback);

		void UpdateAllRelations(List<RelatedProductsData> listOfProductData, Action<ItemProcessCallback<RelatedProductsData>> callback);
	}
}
