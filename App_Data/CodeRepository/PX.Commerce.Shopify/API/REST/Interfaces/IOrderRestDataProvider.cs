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
	public interface IOrderRestDataProvider
	{
		OrderData CancelOrder(string orderId);
		OrderData CloseOrder(string orderId);
		OrderData Create(OrderData entity);
		bool Delete(string id);
		IEnumerable<OrderData> GetAll(IFilter filter = null, CancellationToken cancellationToken = default);
		OrderData GetByID(string id);
		OrderData GetByID(string id, bool includedMetafields = false, bool includedTransactions = false, bool includedCustomer = true, bool includedOrderRisk = false, CancellationToken cancellationToken = default);
		IEnumerable<OrderData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null);
		List<MetafieldData> GetMetafieldsById(string id, CancellationToken cancellationToken = default);
		CustomerData GetOrderCustomer(string orderId);
		List<OrderRisk> GetOrderRisks(string orderId, CancellationToken cancellationToken = default);
		OrderTransaction GetOrderSingleTransaction(string orderId, string transactionId);
		List<OrderTransaction> GetOrderTransactions(string orderId, CancellationToken cancellationToken = default);
		OrderTransaction PostPaymentToCapture(OrderTransaction entity, string orderId);
		OrderData ReopenOrder(string orderId);
		OrderData Update(OrderData entity);
		OrderData Update(OrderData entity, string id);
	}
}
