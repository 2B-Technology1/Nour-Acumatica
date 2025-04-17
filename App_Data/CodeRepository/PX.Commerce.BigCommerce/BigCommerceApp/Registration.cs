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
using PX.Commerce.BigCommerce.API.REST;
using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce
{
	public class ServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			Client bigCommerceAppClient = BigCommerceAppConnectedApplication.BigCommerceAppClientForRegistration();

			builder
				.RegisterInstance(bigCommerceAppClient)
				.Keyed<Client>(Guid.Parse(BigCommerceAppConnectedApplication.BigCommerceAppClientId));

			#region Data Provider Factories Registration

			builder.RegisterType<CustomerAddressRestDataProviderV3Factory>().As<IBCRestDataProviderFactory<IParentRestDataProviderV3<CustomerAddressData, FilterAddresses>>>();
			builder.RegisterType<CustomerFormFieldRestDataProviderFactory>().As<IBCRestDataProviderFactory<IUpdateAllParentRestDataProvider<CustomerFormFieldData>>>();
			builder.RegisterType<CustomerPriceClassRestDataProviderFactory>().As<IBCRestDataProviderFactory<IParentRestDataProvider<CustomerGroupData>>>();
			builder.RegisterType<CustomerRestDataProviderV3Factory>().As<IBCRestDataProviderFactory<IParentRestDataProviderV3<CustomerData, FilterCustomers>>>();
			builder.RegisterType<OrderCouponsRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<OrdersCouponData>>>();
			builder.RegisterType<OrderMetaFieldRestDataProviderFactory>().As<IBCRestDataProviderFactory<IOrderMetaFieldRestDataProvider>>();
			builder.RegisterType<OrderProductsRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<OrdersProductData>>>();
			builder.RegisterType<OrderRefundsRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrderRefund>>>();
			builder.RegisterType<OrderRestDataProviderFactory>().As<IBCRestDataProviderFactory<IOrderRestDataProvider>>();
			builder.RegisterType<OrderShipmentsRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<OrdersShipmentData>>>();
			builder.RegisterType<OrderShippingAddressesRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<OrdersShippingAddressData>>>();
			builder.RegisterType<OrderTaxesRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<OrdersTaxData>>>();
			builder.RegisterType<OrderTransactionsRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildReadOnlyRestDataProvider<OrdersTransactionData>>>();
			builder.RegisterType<PriceListRecordRestDataProviderFactory>().As<IBCRestDataProviderFactory<IPriceListRecordRestDataProvider>>();
			builder.RegisterType<PriceListRestDataProviderFactory>().As<IBCRestDataProviderFactory<IPriceListRestDataProvider>>();
			builder.RegisterType<ProductBatchBulkRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<BulkPricingWithSalesPrice>>>();
			builder.RegisterType<ProductBulkPricingRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsBulkPricingRules>>>();
			builder.RegisterType<ProductCategoryRestDataProviderFactory>().As<IBCRestDataProviderFactory<IParentRestDataProvider<ProductCategoryData>>>();
			builder.RegisterType<ProductCustomFieldRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsCustomFieldData>>>();
			builder.RegisterType<ProductImagesDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsImageData>>>();
			builder.RegisterType<ProductOptionValueRestDataProviderFactory>().As<IBCRestDataProviderFactory<ISubChildRestDataProvider<ProductOptionValueData>>>();
			builder.RegisterType<ProductRestDataProviderFactory>().As<IBCRestDataProviderFactory<IStockRestDataProvider<ProductData>>>();
			builder.RegisterType<ProductsOptionRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsOptionData>>>();
			builder.RegisterType<ProductVariantBatchRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildUpdateAllRestDataProvider<ProductsVariantData>>>();
			builder.RegisterType<ProductVariantRestDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVariantData>>>();
			builder.RegisterType<ProductVideoDataProviderFactory>().As<IBCRestDataProviderFactory<IChildRestDataProvider<ProductsVideo>>>();
			builder.RegisterType<StoreCountriesProviderFactory>().As<IBCRestDataProviderFactory<IRestDataReader<List<Countries>>>>();
			builder.RegisterType<StoreCurrencyDataProviderFactory>().As<IBCRestDataProviderFactory<IRestDataReader<List<Currency>>>>();
			builder.RegisterType<StoreStatesProviderFactory>().As<IBCRestDataProviderFactory<IRestDataReader<List<States>>>>();
			builder.RegisterType<TaxDataProviderFactory>().As<IBCRestDataProviderFactory<IParentReadOnlyRestDataProvider<ProductsTaxData>>>();
			builder.RegisterType<VariantImageDataProviderFactory>().As<IBCRestDataProviderFactory<IVariantImageDataProvider>>();
			builder.RegisterType<BasePriceDataProvider>().As<IBasePriceDataProvider>();
			builder.RegisterType<CustomerClassPriceListDataProvider>().As<ICustomerClassPriceListDataProvider>();
			#endregion Data Provider Factories Registration
		}
	}
}
