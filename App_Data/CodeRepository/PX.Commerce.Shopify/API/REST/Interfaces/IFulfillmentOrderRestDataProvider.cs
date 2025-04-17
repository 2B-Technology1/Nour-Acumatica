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
	public interface IFulfillmentOrderRestDataProvider
	{
		FulfillmentOrder ReleaseOpenFulfillment(string fulfillmentOrderId);

		FulfillmentOrder OpenFulfillmentOrder(string fulfillmentOrderId);

		FulfillmentOrder CancelFulfillmentOrder(string fulfillmentOrderId);

		FulfillmentOrder HoldFulfillmentOrder(FulfillmentOrder fulfillmentOrder, string fulfillmentOrderId);

		FulfillmentOrder MoveFulfillmentOrder(long? locationId, string fulfillmentOrderId);

		FulfillmentOrder CloseFulfillmentOrder(string fulfillmentOrderId, string message = null);

		//bool Delete(string fulfillmentId, string orderId = null);

		IEnumerable<FulfillmentOrder> GetAll(string orderId, IFilter filter = null, CancellationToken cancellationToken = default);

		//IEnumerable<FulfillmentOrder> GetAllWithoutParent(IFilter filter = null);

		FulfillmentOrder GetByID(string fulfillmentOrderId, string parentID = null);

		//FulfillmentOrder Update(FulfillmentOrder entity, string parentId, string id);
	}
}
