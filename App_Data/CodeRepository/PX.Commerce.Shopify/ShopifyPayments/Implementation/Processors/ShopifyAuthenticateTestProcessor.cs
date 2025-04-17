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

using PX.CCProcessingBase.Interfaces.V2;
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.ShopifyPayments
{
	public class ShopifyAuthenticateTestProcessor : ShopifyPaymentsDataProvider
	{		
		public ShopifyAuthenticateTestProcessor(IShopifyRestClient restClient) : base(restClient)
		{
		}

		public void TestCredentials()
		{
			try
			{
				StoreRestDataProvider restClient = new StoreRestDataProvider(ShopifyRestClient);
				StoreData store = null;

				try
				{
					store = restClient.Get();
				}
				catch (NullReferenceException)
				{
					// Do nothing
				}

				if (store == null || store.Id == null)
					throw new PXException(ShopifyMessages.TestConnectionStoreNotFound);
			}
			catch (Exception ex)
			{
				ErrorHandler(ex);
				throw;
			}
		}
	}
}
