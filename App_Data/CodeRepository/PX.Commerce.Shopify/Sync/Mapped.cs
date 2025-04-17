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

using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using System;

namespace PX.Commerce.Shopify
{
	#region SPMappedEntity
	public abstract class SPMappedEntity<ExternType, LocalType> : MappedEntity<ExternType, LocalType>
		where ExternType : BCAPIEntity, IExternEntity
		where LocalType : CBAPIEntity, ILocalEntity
	{
		public SPMappedEntity(String entType)
			: base(SPConnector.TYPE, entType)
		{ }
		public SPMappedEntity(BCSyncStatus status)
			: base(status)
		{
		}
		public SPMappedEntity(String entType, LocalType entity, Guid? id, DateTime? timestamp)
			: base(SPConnector.TYPE, entType, entity, id, timestamp)
		{
		}
		public SPMappedEntity(String entType, ExternType entity, String id, String description, DateTime? timestamp)
			: base(SPConnector.TYPE, entType, entity, id, description, timestamp)
		{
		}
		public SPMappedEntity(String entType, ExternType entity, String id, String description, String hash)
			: base(SPConnector.TYPE, entType, entity, id, description, hash)
		{
		}
	}
	#endregion
	#region MappedCustomer
	public class MappedCustomer : SPMappedEntity<CustomerData, Customer>
	{
		public const String TYPE = BCEntitiesAttribute.Customer;

		public MappedCustomer()
			: base(TYPE)
		{ }
		public MappedCustomer(Customer entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedCustomer(CustomerData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedCompany
	public class MappedCompany: SPMappedEntity<CompanyDataGQL, Customer>
	{
		public const String TYPE = BCEntitiesAttribute.Company;

		public MappedCompany()
			: base(TYPE)
		{ }
		public MappedCompany(Customer entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedCompany(CompanyDataGQL entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedLocation
	public class MappedLocation : SPMappedEntity<CustomerAddressData, CustomerLocation>
	{
		public const String TYPE = BCEntitiesAttribute.Address;

		public MappedLocation()
			: base(TYPE)
		{ }
		public MappedLocation(BCSyncStatus status)
			: base(status) { }
		public MappedLocation(CustomerLocation entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedLocation(CustomerAddressData entity, String id, String description, String hash, Int32? parent)
			: base(TYPE, entity, id, description, hash)
		{
			ParentID = parent;
		}
	}
	#endregion
	#region MappedStockItem
	public class MappedStockItem : SPMappedEntity<ProductData, StockItem>
	{
		public const String TYPE = BCEntitiesAttribute.StockItem;

		public MappedStockItem()
			: base(TYPE)
		{ }
		public MappedStockItem(StockItem entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedStockItem(ProductData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedNonStockItem
	public class MappedNonStockItem : SPMappedEntity<ProductData, NonStockItem>
	{
		public const String TYPE = BCEntitiesAttribute.NonStockItem;

		public MappedNonStockItem()
			: base(TYPE)
		{ }
		public MappedNonStockItem(NonStockItem entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedNonStockItem(ProductData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedTemplateItem
	public class MappedTemplateItem : SPMappedEntity<ProductData, TemplateItems>
	{
		public const String TYPE = BCEntitiesAttribute.ProductWithVariant;

		public MappedTemplateItem()
			: base(TYPE)
		{ }
		public MappedTemplateItem(TemplateItems entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedTemplateItem(ProductData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedAvailability
	public class MappedAvailability : SPMappedEntity<InventoryLevelData, StorageDetailsResult>
	{
		public const String TYPE = BCEntitiesAttribute.ProductAvailability;

		public MappedAvailability()
			: base(TYPE)
		{ }
		public MappedAvailability(StorageDetailsResult entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
			UpdateParentExternTS = true;
		}
		public MappedAvailability(InventoryLevelData entity, String id, String description, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, description, timestamp)
		{
			ParentID = parent;
			UpdateParentExternTS = true;
		}
	}
	#endregion
	#region MappedProductImage
	public class MappedProductImage : SPMappedEntity<ProductImageData, ItemImageDetails>
	{
		public const String TYPE = BCEntitiesAttribute.ProductImage;

		public MappedProductImage()
			: base(TYPE)
		{ }
		public MappedProductImage(ItemImageDetails entity, Guid? id, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, timestamp)
		{
			ParentID = parent;
		}
		public MappedProductImage(ProductImageData entity, String id, String description, DateTime? timestamp, Int32? parent)
			: base(TYPE, entity, id, description, timestamp)
		{
			ParentID = parent;
		}
	}
	#endregion
	#region MappedSalesOrder
	public class MappedOrder : SPMappedEntity<OrderData, SalesOrder>
	{
		public const String TYPE = BCEntitiesAttribute.Order;

		public MappedOrder()
			: base(TYPE)
		{ }
		public MappedOrder(SalesOrder entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedOrder(OrderData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedPayment
	public class MappedPayment : SPMappedEntity<OrderTransaction, Payment>
	{
		public const String TYPE = BCEntitiesAttribute.Payment;

		public MappedPayment()
			: base(TYPE)
		{ }
		public MappedPayment(Payment entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedPayment(OrderTransaction entity, String id, String description, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, description, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedShipment
	public class MappedShipment : SPMappedEntity<ShipmentData, BCShipments>
	{
		public const String TYPE = BCEntitiesAttribute.Shipment;

		public MappedShipment()
			: base(TYPE)
		{ }
		public MappedShipment(BCShipments entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedShipment(ShipmentData entity, String id, String description, DateTime? timestamp, String hashcode)
			: base(TYPE, entity, id, description, timestamp) { ExternHash = hashcode; }
	}
	#endregion
	#region MappedRefunds
	public class MappedRefunds : SPMappedEntity<OrderData, SalesOrder>
	{
		public const String TYPE = BCEntitiesAttribute.OrderRefunds;

		public MappedRefunds()
			: base(TYPE)
		{ }
		public MappedRefunds(SalesOrder entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp) { }
		public MappedRefunds(OrderData entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp) { }
	}
	#endregion
	#region MappedPriceList
	public class MappedPriceList : SPMappedEntity<PriceListGQL, PriceListSalesPrice>
	{
		public const String TYPE = BCEntitiesAttribute.PriceList;

		public MappedPriceList()
			: base(TYPE)
		{ }
		public MappedPriceList(PriceListSalesPrice entity, Guid? id, DateTime? timestamp)
			: base(TYPE, entity, id, timestamp)
		{ }
		public MappedPriceList(PriceListGQL entity, String id, String description, DateTime? timestamp)
			: base(TYPE, entity, id, description, timestamp)
		{ }
	}
	#endregion
}
