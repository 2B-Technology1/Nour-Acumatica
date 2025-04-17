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
	/// <summary>
	/// 
	/// </summary>
	public interface IPriceListRestDataProvider
	{
		/// <summary>
		/// Creates a price list in the external system
		/// </summary>
		/// <param name="priceList"></param>
		/// <returns></returns>
		PriceList Create(PriceList priceList);
		/// <summary>
		/// Deletes a price list in the external system
		/// </summary>
		/// <param name="priceListId"></param>
		/// <returns></returns>
		bool DeletePriceList(string priceListId);
		/// <summary>
		/// Get all the price lists that exist in the external system.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		IEnumerable<PriceList> GetAll(CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IPriceListRecordRestDataProvider : IPriceListRestDataProvider
	{
		/// <summary>
		/// Deletes a record in a price list id
		/// </summary>
		/// <param name="priceListId">Price list id in the external system.</param>
		/// <param name="id">product / variant id</param>
		/// <param name="currency">the currency of the price.</param>
		/// <returns></returns>
		bool DeleteRecords(string priceListId, string id, string currency);
		/// <summary>
		/// Gets all the prices that belong to the price list.
		/// </summary>
		/// <param name="priceListId"></param>
		/// <param name="filter"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		IEnumerable<PriceListRecord> GetAllRecords(string priceListId, IFilter filter = null, CancellationToken cancellationToken = default);
		/// <summary>
		/// Bulk insert of a price list.
		/// </summary>
		/// <param name="priceListRecords"></param>
		/// <param name="priceListId"></param>
		/// <param name="callback"></param>
		void Upsert(List<PriceListRecord> priceListRecords, string priceListId, Action<ItemProcessCallback<PriceListRecord>> callback);
	}
}
