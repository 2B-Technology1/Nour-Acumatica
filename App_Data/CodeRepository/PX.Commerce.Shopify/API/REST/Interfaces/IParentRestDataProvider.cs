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

namespace PX.Commerce.Shopify.API.REST
{
	public interface IParentRestDataProvider<T>
		where T : class
	{
		T Create(T entity);

		T Update(T entity, string id);

		bool Delete(string id);

		IEnumerable<T> GetAll(IFilter filter = null, CancellationToken cancellationToken = default);

		T GetByID(string id);
	}

	public interface ICustomerRestDataProvider<T> : IParentRestDataProvider<T>
		where T : class
	{
		bool ActivateAccount(string customerId);

		bool Delete(T entity, string customerId);

		List<CustomerAddressData> GetAddressesById(string id, CancellationToken cancellationToken = default);

		T GetByID(string customerId, bool includedMetafields = true, bool includeAllAddresses = false, CancellationToken cancellationToken = default);

		IEnumerable<T> GetByQuery(string fieldName, string value, bool includedMetafields = false, CancellationToken cancellationToken = default);

		List<MetafieldData> GetMetafieldsById(string id, CancellationToken cancellationToken = default);

		T Update(T entity);
	}

	public interface IProductRestDataProvider<T> : IParentRestDataProvider<T> where T : class
	{
		T GetByID(string productId, bool includedMetafields = false, CancellationToken cancellationToken = default);
	}

	public interface IInventoryLevelRestDataProvider<T> : IParentRestDataProvider<T> where T : class
	{
		T SetInventory(T entity);
	}
}
