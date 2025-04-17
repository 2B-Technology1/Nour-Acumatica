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

using Autofac;
using IdentityServer4.Models;
using PX.Commerce.Shopify.API.REST;
using System;
using PX.Commerce.Shopify.API.GraphQL;

namespace PX.Commerce.Shopify
{
	public class ServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			Client shopifyAppClient = ShopifyAppConnectedApplication.ShopifyAppClientForRegistration();

			builder
				.RegisterInstance(shopifyAppClient)
				.Keyed<Client>(Guid.Parse(ShopifyAppConnectedApplication.ShopifyAppClientId));

			#region Rest Data Providers Factories
			builder.RegisterType<CustomerAddressRestDataProviderFactory>().As<ISPRestDataProviderFactory<IChildRestDataProvider<CustomerAddressData>>>();
			builder.RegisterType<CustomerRestDataProviderFactory>().As<ISPRestDataProviderFactory<ICustomerRestDataProvider<CustomerData>>>();
			builder.RegisterType<FulfillmentRestDataProviderFactory>().As<ISPRestDataProviderFactory<IFulfillmentRestDataProvider>>();
			builder.RegisterType<FulfillmentOrderRestDataProviderFactory>().As<ISPRestDataProviderFactory<IFulfillmentOrderRestDataProvider>>();
			builder.RegisterType<InventoryLevelRestDataProviderFactory>().As<ISPRestDataProviderFactory<IInventoryLevelRestDataProvider<InventoryLevelData>>>();
			builder.RegisterType<InventoryLocationRestDataProviderFactory>().As<ISPRestDataProviderFactory<IParentRestDataProvider<InventoryLocationData>>>();
			builder.RegisterType<OrderRestDataProviderFactory>().As<ISPRestDataProviderFactory<IOrderRestDataProvider>>();
			builder.RegisterType<ProductImageRestDataProviderFactory>().As<ISPRestDataProviderFactory<IChildRestDataProvider<ProductImageData>>>();
			builder.RegisterType<ProductRestDataProviderFactory>().As<ISPRestDataProviderFactory<IProductRestDataProvider<ProductData>>>();
			builder.RegisterType<ProductVariantRestDataProviderFactory>().As<ISPRestDataProviderFactory<IChildRestDataProvider<ProductVariantData>>>();
			builder.RegisterType<StoreRestDataProviderFactory>().As<ISPRestDataProviderFactory<IStoreRestDataProvider>>();
			#endregion

			#region Graph Data Provider Factories

			builder.RegisterType<ProductGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<ProductGQLDataProvider>>();

			#endregion

			builder.RegisterType<SPGraphQLAPIClientFactory>().As<ISPGraphQLAPIClientFactory>().SingleInstance();
			builder.RegisterType<CompanyGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<CompanyGQLDataProvider>>();
			builder.RegisterType<MetafieldsGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<MetaFielsGQLDataProvider>>();
			builder.RegisterType<SPMetafieldsMappingServiceFactory>().As<ISPMetafieldsMappingServiceFactory>();
			builder.RegisterType<OrderGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<OrderGQLDataProvider>>();
			builder.RegisterType<CustomerGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<CustomerGQLDataProvider>>();
			builder.RegisterType<PriceListGQLDataProviderFactory>().As<ISPGraphQLDataProviderFactory<PriceListGQLDataProvider>>();
			builder.RegisterType<PriceListInternalDataProvider>().As<IPriceListInternalDataProvider>(); 
		}
	}
}
