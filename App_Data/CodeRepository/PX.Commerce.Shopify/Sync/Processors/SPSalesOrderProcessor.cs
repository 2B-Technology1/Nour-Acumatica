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

using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public class SPSalesOrderBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary { get => Order; }
		public IMappedEntity[] Entities => new IMappedEntity[] { Order };

		public override IMappedEntity[] PreProcessors { get => (new IMappedEntity[] { Customer, Company }).ToArray(); }
		public override IMappedEntity[] PostProcessors { get => Enumerable.Empty<IMappedEntity>().Concat(Payments).Concat(Shipments).ToArray(); }

		public MappedOrder Order;
		public MappedCustomer Customer;
		public MappedCompany Company;
		public MappedLocation Location;
		public List<MappedPayment> Payments = new List<MappedPayment>();
		public List<MappedShipment> Shipments = new List<MappedShipment>();
	}

	public class SPSalesOrderRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region Orders
			return base.Restrict<MappedOrder>(mapped, delegate (MappedOrder obj)
			{
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();

				// skip order that was created before SyncOrdersFrom
				if (mode != FilterMode.Merge
					&& obj.Local != null && bindingExt.SyncOrdersFrom != null && obj.Local.CreatedDate.Value < bindingExt.SyncOrdersFrom)
				{
					return new FilterResult(FilterStatus.Ignore,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedCreatedBeforeSyncOrdersFrom, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(), bindingExt.SyncOrdersFrom.Value.Date.ToString("d")));
				}

				//skip order that has only gift certificate in order line
				var guestID = bindingExt.GuestCustomerID != null ? PX.Objects.AR.Customer.PK.Find((PXGraph)processor, bindingExt.GuestCustomerID)?.AcctCD : null;
				if (mode != FilterMode.Merge
					&& guestID != null && !string.IsNullOrEmpty(obj.Local?.CustomerID?.Value?.Trim()) && guestID.Trim().Equals(obj.Local?.CustomerID?.Value?.Trim()))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedGuestOrder, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString()));
				}

				var orderTypesArray = bindingExt.OtherSalesOrderTypes?.Split(',').Where(i => i != bindingExt.OrderType).ToArray();
				BCBindingShopify bindingShopify = processor.GetBindingExt<BCBindingShopify>();
				if (obj.Local?.OrderType != null && obj.Local?.OrderType?.Value != bindingShopify.POSDirectOrderType && obj.Local?.OrderType?.Value != bindingShopify.POSShippingOrderType &&
					obj.Local?.OrderType?.Value != bindingExt.OrderType && orderTypesArray?.Contains(obj.Local?.OrderType?.Value) == false)
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedTypeNotSupported, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(), obj.Local?.OrderType?.Value));
				}

				//skip order with following status: On-Hold, Credit Hold, Pending Approval, Canceled, Expired, and Risk Hold.
				if (mode != FilterMode.Merge
					&& obj.IsNew && obj.Local != null && obj.Local.Status != null)
				{
					if (obj.Local.Status.Value == PX.Objects.SO.Messages.Hold || obj.Local.Status.Value == PX.Objects.SO.Messages.CreditHold
						|| obj.Local.Status.Value == PX.Objects.SO.Messages.Cancelled || obj.Local.Status.Value == PX.Objects.SO.Messages.Expired
						|| obj.Local.Status.Value == BCObjectsMessages.RiskHold
						|| obj.Local.Status.Value == PX.Objects.SO.Messages.AwaitingPayment || obj.Local.Status.Value == PX.Objects.EP.Messages.Balanced
						|| obj.Local.Status.Value == PX.Objects.EP.Messages.Rejected || obj.Local.Status.Value == PX.Objects.SO.Messages.Expired)
					{
						return new FilterResult(FilterStatus.Filtered,
							PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedLocalStatusNotSupported,
								obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString(),
								obj.Local.Status.Value));
					}
				}

				if (mode != FilterMode.Merge
					&& obj.IsNew && !String.IsNullOrEmpty(obj.Local?.ExternalRef?.Value?.Trim()))
				{
					return new FilterResult(FilterStatus.Filtered,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedWithExternalRef, obj.Local.OrderNbr?.Value ?? obj.Local.SyncID.ToString()));
				}

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region Orders
			return base.Restrict<MappedOrder>(mapped, delegate (MappedOrder obj)
			{
				BCBindingShopify bindingShopify = processor.GetBindingExt<BCBindingShopify>();
				BCBindingExt bindingExt = processor.GetBindingExt<BCBindingExt>();

				// skip order that was created before SyncOrdersFrom
				if (mode != FilterMode.Merge
					&& obj.Extern != null && bindingExt.SyncOrdersFrom != null && obj.Extern.DateCreatedAt < bindingExt.SyncOrdersFrom)
				{
					return new FilterResult(FilterStatus.Ignore,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedCreatedBeforeSyncOrdersFrom, obj.Extern.Id, bindingExt.SyncOrdersFrom.Value.Date.ToString("d")));
				}

				if (mode != FilterMode.Merge
					&& obj.Extern != null && obj.IsNew
					&& string.Equals(obj.Extern.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase)
					&& (PXAccess.FeatureInstalled<FeaturesSet.shopifyPOS>() != true || bindingShopify.ShopifyPOS != true))
				{
					return new FilterResult(FilterStatus.Invalid,
						PXMessages.LocalizeFormatNoPrefixNLA(ShopifyMessages.POSOrderNotSupported, $"{obj.Extern.Name}({obj.Extern.Id})"));
				}

				if (mode != FilterMode.Merge
					&& obj.Extern != null && obj.IsNew
					&& obj.Extern.ClosedAt != null
					&& !string.Equals(obj.Extern.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase))
				{
					return new FilterResult(FilterStatus.Filtered,
						PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogOrderSkippedExternStatusNotSupported, $"{obj.Extern.Name}({obj.Extern.Id})",
							PXMessages.LocalizeNoPrefix(BCAPICaptions.Archived)));
				}

				return null;
			});
			#endregion
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Order, BCCaptions.Order, 80,
		IsInternal = false,
		Direction = SyncDirection.Bidirect,
		PrimaryDirection = SyncDirection.Import,
		PrimarySystem = PrimarySystem.Extern,
		PrimaryGraph = typeof(PX.Objects.SO.SOOrderEntry),
		ExternTypes = new Type[] { typeof(OrderData) },
		LocalTypes = new Type[] { typeof(SalesOrder) },
		DetailTypes = new String[] { BCEntitiesAttribute.OrderLine, BCCaptions.OrderLine, BCEntitiesAttribute.OrderAddress, BCCaptions.OrderAddress },
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOOrder),
		AcumaticaPrimarySelect = typeof(Search<PX.Objects.SO.SOOrder.orderNbr>),
		URL = "orders/{0}",
		RequiresOneOf = new string[] { BCEntitiesAttribute.Customer + "." + BCEntitiesAttribute.Company }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.OrderLine, EntityName = BCCaptions.OrderLine, AcumaticaType = typeof(PX.Objects.SO.SOLine))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.OrderAddress, EntityName = BCCaptions.OrderAddress, AcumaticaType = typeof(PX.Objects.SO.SOOrder))]
	[BCProcessorRealtime(PushSupported = true, HookSupported = true,
		PushSources = new String[] { "BC-PUSH-Orders" }, PushDestination = BCConstants.PushNotificationDestination,
		WebHookType = typeof(WebHookMessage),
		WebHooks = new String[]
		{
			"orders/create",
			"orders/cancelled",
			"orders/paid",
			"orders/updated"
		})]
	[BCProcessorExternCustomField(BCConstants.ShopifyMetaFields, ShopifyCaptions.Metafields, nameof(OrderData.Metafields), typeof(OrderData), new Type[] { typeof(SalesOrderDetail) })]
	[BCProcessorExternCustomField(BCConstants.OrderItemProperties, ShopifyCaptions.Properties, nameof(OrderLineItem.Properties), typeof(OrderLineItem), new Type[] { typeof(SalesOrderDetail) }, readAsCollection: true)]
	public class SPSalesOrderProcessor : SPOrderBaseProcessor<SPSalesOrderProcessor, SPSalesOrderBucket, MappedOrder>
	{
		protected SPPaymentProcessor paymentProcessor = PXGraph.CreateInstance<SPPaymentProcessor>();

		protected IProductRestDataProvider<ProductData> productDataProvider;
		protected IChildRestDataProvider<ProductVariantData> productVariantDataProvider;
		protected IOrderRestDataProvider orderDataProvider;
		protected IFulfillmentOrderRestDataProvider fulfillmentOrderRestDataProvider;
		protected IStoreRestDataProvider storeDataProvider;
		protected List<ShippingZoneData> storeShippingZones;
		protected IChildRestDataProvider<CustomerAddressData> customerAddressRestDataProvider;
		protected IParentRestDataProvider<InventoryLocationData> inventoryLocationRestDataProvider;
		protected IMetafieldsGQLDataProvider metafieldDataGQLProvider;
		protected IOrderGQLDataProvider orderDataGQLProvider;
		private ISPMetafieldsMappingService metafieldsMappingService;

		#region Factories
		[InjectDependency]
		protected ISPRestDataProviderFactory<IProductRestDataProvider<ProductData>> productDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<ProductVariantData>> productVariantDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IOrderRestDataProvider> orderDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IFulfillmentOrderRestDataProvider> fulfillmentOrderDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IStoreRestDataProvider> storeDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IChildRestDataProvider<CustomerAddressData>> customerAddressDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IParentRestDataProvider<InventoryLocationData>> locationDataProviderFactory { get; set; }
		[InjectDependency]
		public ISPGraphQLDataProviderFactory<MetaFielsGQLDataProvider> metafieldGrahQLDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPGraphQLDataProviderFactory<OrderGQLDataProvider> orderDataGQLProviderFactory { get; set; }
		[InjectDependency]
		public ISPGraphQLAPIClientFactory shopifyGraphQLClientFactory { get; set; }
		[InjectDependency]
		public ISPMetafieldsMappingServiceFactory spMetafieldsMappingServiceFactory { get; set; }
		#endregion

		#region Initialization
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			orderDataProvider = orderDataProviderFactory.CreateInstance(client);
			storeDataProvider = storeDataProviderFactory.CreateInstance(client);
			customerAddressRestDataProvider = customerAddressDataProviderFactory.CreateInstance(client);
			productDataProvider = productDataProviderFactory.CreateInstance(client);
			productVariantDataProvider = productVariantDataProviderFactory.CreateInstance(client);
			fulfillmentOrderRestDataProvider = fulfillmentOrderDataProviderFactory.CreateInstance(client);
			inventoryLocationRestDataProvider = locationDataProviderFactory.CreateInstance(client);

			storeShippingZones = storeDataProvider.GetShippingZones().ToList();

			if (GetEntity(BCEntitiesAttribute.Payment)?.IsActive == true)
			{
				await paymentProcessor.Initialise(iconnector, operation.Clone().With(_ => { _.EntityType = BCEntitiesAttribute.Payment; return _; }));
			}

			var graphQLClient = shopifyGraphQLClientFactory.GetClient(GetBindingExt<BCBindingShopify>());
			metafieldDataGQLProvider = metafieldGrahQLDataProviderFactory.GetProvider(graphQLClient);
			metafieldsMappingService = spMetafieldsMappingServiceFactory.GetInstance(metafieldDataGQLProvider);
			orderDataGQLProvider = orderDataGQLProviderFactory.GetProvider(graphQLClient);
		}
		#endregion

		#region Common
		public override List<(string fieldName, string fieldValue)> GetExternCustomFieldList(BCEntity entity, ExternCustomFieldInfo customFieldInfo)
		{
			if (customFieldInfo.Identifier == BCConstants.ShopifyMetaFields)
			{
				return new List<(string fieldName, string fieldValue)>() { (BCConstants.MetafieldFormat, BCConstants.MetafieldFormat) };
			}
			if (customFieldInfo.Identifier == BCConstants.OrderItemProperties)
			{
				return new List<(string fieldName, string fieldValue)>() { (nameof(NameValuePair.Name), nameof(NameValuePair.Name)), (nameof(NameValuePair.Value), nameof(NameValuePair.Value)) };
			}
			return new List<(string fieldName, string fieldValue)>();
		}
		public override void ValidateExternCustomField(BCEntity entity, ExternCustomFieldInfo customFieldInfo, string sourceObject, string sourceField, string targetObject, string targetField, EntityOperationType direction)
		{
			//For item properties, skip validation
			if (customFieldInfo.Identifier == BCConstants.OrderItemProperties)
				return;
			//Skip formula expression
			if (direction == EntityOperationType.ImportMapping && sourceField.StartsWith("="))
				return;
			//Validate the field format
			var fieldStrGroup = direction == EntityOperationType.ImportMapping ? sourceField?.Split('.') : targetField?.Split('.');
			if (fieldStrGroup.Length == 2)
			{
				var keyFieldName = fieldStrGroup[0].Replace("[", "").Replace("]", "").Replace(" ", "");
				if (!string.IsNullOrWhiteSpace(keyFieldName) && string.Equals(keyFieldName, BCConstants.MetafieldFormat, StringComparison.OrdinalIgnoreCase) == false)
					return;
			}
			throw new PXException(BCMessages.InvalidFilter, direction == EntityOperationType.ImportMapping ? "Source" : "Target", BCConstants.MetafieldFormat);
		}
		public override object GetExternCustomFieldValue(SPSalesOrderBucket entity, ExternCustomFieldInfo customFieldInfo, object sourceData, string sourceObject, string sourceField, out string displayName)
		{
			displayName = null;
			var formattedSourceObject = sourceObject.Replace(" -> ", ".");
			if (customFieldInfo.Identifier == BCConstants.ShopifyMetaFields)
			{
				var sourceinfo = sourceField.Split('.');
				var metafields = GetPropertyValue(sourceData, formattedSourceObject, out displayName);
				if (metafields != null && (metafields.GetType().IsGenericType && (metafields.GetType().GetGenericTypeDefinition() == typeof(List<>) || metafields.GetType().GetGenericTypeDefinition() == typeof(IList<>))))
				{
					var metafieldsList = (System.Collections.IList)metafields;
					if (sourceinfo.Length == 2)
					{
						var nameSpaceField = sourceinfo[0].Replace("[", "").Replace("]", "")?.Trim();
						var keyField = sourceinfo[1].Replace("[", "").Replace("]", "")?.Trim();
						foreach (object metaItem in metafieldsList)
						{
							if (metaItem is MetafieldData)
							{
								var metaField = metaItem as MetafieldData;
								if (metaField != null && string.Equals(metaField.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(metaField.Key, keyField, StringComparison.OrdinalIgnoreCase))
								{
									return metaField.Value;
								}
							}
						}

					}
				}
			}
			if (customFieldInfo.Identifier == BCConstants.OrderItemProperties)
			{
				if (string.IsNullOrWhiteSpace(sourceField))
				{
					return GetPropertyValue(sourceData, formattedSourceObject.Substring(0, formattedSourceObject.LastIndexOf($".{customFieldInfo.ObjectName}")), out displayName);
				}
				if (sourceData is OrderLineItem)
				{
					var propertiesList = (sourceData as OrderLineItem).Properties;
					if (propertiesList != null && propertiesList.Count > 0)
					{
						if (string.Equals(sourceField, nameof(NameValuePair.Name), StringComparison.OrdinalIgnoreCase))
							return propertiesList.FirstOrDefault()?.Name;
						else if (string.Equals(sourceField, nameof(NameValuePair.Value), StringComparison.OrdinalIgnoreCase))
							return propertiesList.FirstOrDefault()?.Value;
						else
							return propertiesList.FirstOrDefault(x => string.Equals(x.Name, sourceField, StringComparison.OrdinalIgnoreCase))?.Value;
					}
				}
			}
			return null;
		}

		public override void SetExternCustomFieldValue(SPSalesOrderBucket entity, ExternCustomFieldInfo customFieldInfo, object targetData, string targetObject, string targetField, string sourceObject, object value, IMappedEntity existing = null)
		{
			if (value != PXCache.NotSetValue && value != null)
			{
				if (customFieldInfo.Identifier == BCConstants.ShopifyMetaFields)
				{
					var targetinfo = targetField?.Split('.');
					if (targetinfo.Length == 2)
					{
						var nameSpaceField = targetinfo[0].Replace("[", "").Replace("]", "")?.Trim();
						var keyField = targetinfo[1].Replace("[", "").Replace("]", "")?.Trim();
						OrderData data = (OrderData)entity.Primary.Extern;
						OrderData existingOrder = existing?.Extern as OrderData;

						var entityType = ShopifyGraphQLConstants.OWNERTYPE_ORDER;
						var metafieldValue = metafieldsMappingService.GetFormattedMetafieldValue(entityType, nameSpaceField, keyField, Convert.ToString(value));

						var newMetaField = new MetafieldData()
						{
							Namespace = nameSpaceField,
							Key = keyField,
							Value = metafieldValue.Value,
							Type = metafieldValue.GetShopifyType()
						};
						if (customFieldInfo.ExternEntityType == typeof(OrderData))
						{
							var metaFieldList = data.Metafields = data.Metafields ?? new List<MetafieldData>();
							if (existingOrder != null && existingOrder.Metafields?.Count > 0)
							{
								var existedMetaField = existingOrder.Metafields.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
								newMetaField.Id = existedMetaField?.Id;
							}
							var matchedData = metaFieldList.FirstOrDefault(x => string.Equals(x.Namespace, nameSpaceField, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Key, keyField, StringComparison.OrdinalIgnoreCase));
							if (matchedData != null)
							{
								matchedData = newMetaField;
							}
							else
								metaFieldList.Add(newMetaField);
						}
					}
				}
			}
		}

		public override async Task<MappedOrder> PullEntity(Guid? localID, Dictionary<String, Object> fields, CancellationToken cancellationToken = default)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(localID);
			if (impl == null) return null;

			MappedOrder obj = new MappedOrder(impl, impl.SyncID, impl.SyncTime);

			return obj;
		}
		public override async Task<MappedOrder> PullEntity(string externID, string jsonObject, CancellationToken cancellationToken = default)
		{
			OrderData data = orderDataProvider.GetByID(externID);
			if (data == null) return null;

			if (data.ClosedAt != null && data?.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled && !string.Equals(data.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase) &&
						data?.FinancialStatus != OrderFinancialStatus.PartiallyRefunded && data?.FinancialStatus != OrderFinancialStatus.Refunded)
				return null;

			MappedOrder obj = new MappedOrder(data, data.Id?.ToString(), data.Name, data.DateModifiedAt.ToDate(false));

			return obj;
		}

		public override async Task<PullSimilarResult<MappedOrder>> PullSimilar(IExternEntity entity, CancellationToken cancellationToken = default)
		{
			string uniqueFieldValue = ((OrderData)entity)?.Id?.ToString();
			if (string.IsNullOrEmpty(uniqueFieldValue))
				return null;
			uniqueFieldValue = APIHelper.ReferenceMake(uniqueFieldValue, GetBinding().BindingName);
			List<MappedOrder> result = new List<MappedOrder>();
			List<string> orderTypes = new List<string>() { GetBindingExt<BCBindingExt>()?.OrderType };
			if (string.Equals(((OrderData)entity)?.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase))
			{
				BCBindingShopify bindingShopify = GetBindingExt<BCBindingShopify>();
				//Support POS order type searching
				if (!string.IsNullOrEmpty(bindingShopify.POSDirectOrderType) && !orderTypes.Contains(bindingShopify.POSDirectOrderType))
					orderTypes.Add(bindingShopify.POSDirectOrderType);
				if (!string.IsNullOrEmpty(bindingShopify.POSShippingOrderType) && !orderTypes.Contains(bindingShopify.POSShippingOrderType))
					orderTypes.Add(bindingShopify.POSShippingOrderType);
			}
			GetHelper<SPHelper>().TryGetCustomOrderTypeMappings(ref orderTypes);

			foreach (SOOrder item in GetHelper<SPHelper>().OrderByTypesAndCustomerRefNbr.Select(orderTypes.ToArray(), uniqueFieldValue))
			{
				SalesOrder data = new SalesOrder() { SyncID = item.NoteID, SyncTime = item.LastModifiedDateTime, ExternalRef = item.CustomerRefNbr?.ValueField() };
				result.Add(new MappedOrder(data, data.SyncID, data.SyncTime));
			}
			return new PullSimilarResult<MappedOrder>() { UniqueField = uniqueFieldValue, Entities = result };
		}

		public override async Task<PullSimilarResult<MappedOrder>> PullSimilar(ILocalEntity entity, CancellationToken cancellationToken = default)
		{
			string uniqueFieldValue = ((SalesOrder)entity)?.ExternalRef?.Value;
			if (string.IsNullOrEmpty(uniqueFieldValue))
				return null;

			uniqueFieldValue = APIHelper.ReferenceParse(uniqueFieldValue, GetBinding().BindingName);
			IEnumerable<OrderData> similarOrders = orderDataProvider.GetAll(new FilterOrders() { IDs = uniqueFieldValue }, cancellationToken);

			OrderData data = similarOrders?.FirstOrDefault();
			if (data == null) return null;

			List<MappedOrder> result = new List<MappedOrder>();
			result.Add(new MappedOrder(data, data.Id?.ToString(), data.Name, data.DateModifiedAt.ToDate(false)));
			return new PullSimilarResult<MappedOrder>() { UniqueField = uniqueFieldValue, Entities = result };
		}

		public override Boolean ControlModification(IMappedEntity mapped, BCSyncStatus status, string operation, CancellationToken cancellationToken = default)
		{
			if (mapped is MappedOrder)
			{
				MappedOrder order = mapped as MappedOrder;
				if (operation == BCSyncOperationAttribute.ExternChanged && !order.IsNew && order?.Extern != null && status?.PendingSync == false)
				{
					//We should prevent order from sync if it is updated by shipment
					if (order.Extern.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled || order.Extern.FulfillmentStatus == OrderFulfillmentStatus.Partial)
					{
						DateTime? orderdate = order.Extern.DateModifiedAt.ToDate(false);
						DateTime? shipmentDate = order.Extern.Fulfillments?.Max(x => x.DateModifiedAt)?.ToDate(false);

						if (orderdate != null && shipmentDate != null && Math.Abs((orderdate - shipmentDate).Value.TotalSeconds) < 5) //Modification withing 5 sec
							return false;
					}
				}
			}

			return base.ControlModification(mapped, status, operation);
		}
		public override bool ShouldFilter(SyncDirection direction, BCSyncStatus status)
		{
			bool filter = base.ShouldFilter(direction, status);

			if (filter && status?.EntityType == BCEntitiesAttribute.Order)
			{
				return (status.ExternID == null || status.LocalID == null);
			}

			return filter;
		}
		public override void ControlDirection(SPSalesOrderBucket bucket, BCSyncStatus status, ref bool shouldImport, ref bool shouldExport, ref bool skipSync, ref bool skipForce)
		{
			MappedOrder order = bucket.Order;

			if (order != null
				&& (shouldImport || Operation.SyncMethod == SyncMode.Force)
				&& order?.IsNew == false && order?.ExternID != null && order?.LocalID != null
				&& (order?.Local?.Status?.Value == PX.Objects.SO.Messages.Completed || order?.Local?.Status?.Value == PX.Objects.SO.Messages.Cancelled))
			{
				var newHash = order.Extern.CalculateHash();
				//If externHash is null and Acumatica order existing, that means BCSyncStatus record was deleted and re-created
				if (string.IsNullOrEmpty(status.ExternHash) || newHash == status.ExternHash)
				{
					skipForce = true;
					skipSync = true;
					status.LastOperation = BCSyncOperationAttribute.ExternChangedWithoutUpdateLocal;
					status.LastErrorMessage = null;
					UpdateStatus(order, status.LastOperation, status.LastErrorMessage);
					shouldImport = false;
				}
			}
		}
		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			BCBindingExt currentBindingExt = GetBindingExt<BCBindingExt>();

			var delaySecs = -GetBindingExt<BCBindingShopify>().ApiDelaySeconds ?? 0;
			FilterOrders filter = new FilterOrders { Status = OrderStatus.Any, Order = "updated_at asc" };
			filter.Fields = BCRestHelper.PrepareFilterFields(typeof(OrderData), filters, "id", "name", "source_name", "financial_status", "fulfillment_status", "updated_at", "created_at", "cancelled_at", "closed_at");

			GetHelper<SPHelper>().SetFilterMinDate(filter, minDateTime, currentBindingExt.SyncOrdersFrom, delaySecs);
			if (maxDateTime != null) filter.UpdatedAtMax = maxDateTime.Value.ToLocalTime();

			IEnumerable<OrderData> datas = orderDataProvider.GetAll(filter, cancellationToken: cancellationToken);

			int countNum = 0;
			List<IMappedEntity> mappedList = new List<IMappedEntity>();
			try
			{
				foreach (OrderData data in datas)
				{
					if (data.ClosedAt != null && data?.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled && !string.Equals(data.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase) &&
						data?.FinancialStatus != OrderFinancialStatus.PartiallyRefunded && data?.FinancialStatus != OrderFinancialStatus.Refunded)
						continue;

					IMappedEntity obj = new MappedOrder(data, data.Id?.ToString(), data.Name, data.DateModifiedAt.ToDate(false));

					mappedList.Add(obj);
					countNum++;
					if (countNum % BatchFetchCount == 0)
					{
						ProcessMappedListForImport(mappedList, true);

					}
				}
			}
			finally
			{
				if (mappedList.Any())
				{
					ProcessMappedListForImport(mappedList, true);
				}
			}
		}

		public override async Task<EntityStatus> GetBucketForImport(SPSalesOrderBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			bool? isPaymentActive = GetEntity(BCEntitiesAttribute.Payment)?.IsActive;
			OrderData data = orderDataProvider.GetByID(syncstatus.ExternID, true, isPaymentActive == true, true, GetBindingExt<BCBindingExt>()?.ImportOrderRisks ?? false);
			if (data == null) return EntityStatus.None;

			//Order item location mapping for all orders
			//We only fetch order item location data when user setup import location mapping in the Store setting page
			bool hasImportLocationMapping = BCLocationSlot.GetBCLocations(Operation.Binding)?.Any(x => x.MappingDirection == BCMappingDirectionAttribute.Import) == true;
			if (hasImportLocationMapping)
			{
				FetchOrderItemsLocationInfo(data);
			}

			MappedOrder obj = bucket.Order = bucket.Order.Set(data, data.Id?.ToString(), data.Name, data.DateModifiedAt.ToDate(false));
			EntityStatus status = EnsureStatus(obj, SyncDirection.Import);

			if (status != EntityStatus.Pending && status != EntityStatus.Synchronized && Operation.SyncMethod != SyncMode.Force)
				return status;

			if (data.Customer != null && data.Customer.Id > 0 && (!string.IsNullOrEmpty(data.Customer.Email) || !string.IsNullOrEmpty(data.Customer.Phone)))
			{
				if (string.IsNullOrEmpty(data.Customer.FirstName) && string.IsNullOrEmpty(data.Customer.LastName))
				{
					LogWarning(Operation.LogScope(), BCMessages.CustomerNameIsEmpty, data.Customer.Id);
				}
				else if (data.Company != null && data.Company.Id != null && data.Company.LocationId != null)
				{
					CompanyDataGQL company = new CompanyDataGQL() { Id = data.Company.Id.ToString().ConvertIdToGid(ShopifyGraphQLConstants.Objects.Company) };
					MappedCompany companyObj = bucket.Company = bucket.Company.Set(company, data.Company.Id.ToString(), null, data.DateCreatedAt.ToDate(false));
					EntityStatus companyStatus = EnsureStatus(companyObj, SyncDirection.Import);
				}
				else
				{
					MappedCustomer customerObj = bucket.Customer = bucket.Customer.Set(data.Customer, data.Customer.Id?.ToString(), data.Customer.Email ?? data.Customer.Phone, data.Customer.DateModifiedAt.ToDate(false));
					EntityStatus customerStatus = EnsureStatus(customerObj);

					if (data.ShippingAddress != null && GetEntity(BCEntitiesAttribute.Address)?.Direction != BCSyncDirectionAttribute.Export)
					{
						CustomerAddressData address = GetMatchingLocation(data);

						if (address != null)
						{
							bucket.Location = bucket.Location.Set(address, new Object[] { address.CustomerId, address.Id }.KeyCombine(), data.Customer.Email ?? address.Name, address.CalculateHash()).With(_ => { _.ParentID = customerObj.SyncID; return _; });
						}
						else
						{
							data.Customer.Addresses = customerAddressRestDataProvider.GetAll(data.Customer.Id.ToString(), cancellationToken: cancellationToken).ToList();// as  just recent 10 address are returned we need to make call to get all and retry
							address = GetMatchingLocation(data);
							if (address != null)
								bucket.Location = bucket.Location.Set(address, new Object[] { address.CustomerId, address.Id }.KeyCombine(), data.Customer.Email ?? address.Name, address.CalculateHash()).With(_ => { _.ParentID = customerObj.SyncID; return _; });
							else
								LogWarning(Operation.LogScope(syncstatus), BCMessages.LogOrderLocationUnidentified, $"{data.Name}|{data.Id}");
						}
					}
					BCExtensions.SetSharedSlot<CustomerData>(this.GetType(), data.Customer.Id?.ToString(), data.Customer);
				}
			}

			if (isPaymentActive == true && data.Transactions?.Any() == true)
			{
				foreach (OrderTransaction tranData in data.Transactions)
				{
					var hasBeenRefunded = data.Transactions.Any(x => x.ParentId == tranData.Id && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Refund && x.Amount == tranData.Amount &&
																x.ProcessedAt.HasValue && x.ProcessedAt.Value.Subtract(tranData.ProcessedAt.Value).TotalSeconds < 10);
				
					var hasSaleTransaction = data.Transactions.Any(x => x.Id == tranData.ParentId && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Sale && x.Amount == tranData.Amount &&
										tranData.ProcessedAt.HasValue && tranData.ProcessedAt.Value.Subtract(x.ProcessedAt.Value).TotalSeconds < 10);

					if ((tranData.Status == TransactionStatus.Success && tranData.Kind == TransactionType.Sale && hasBeenRefunded) ||
						(tranData.Status == TransactionStatus.Success && tranData.Kind == TransactionType.Refund && tranData.ParentId != null && hasSaleTransaction))
					{
						//Skip successful payment and its refund payment to avoid payment amount greater than order total amount issue if another payment is unsuccessful and system rolls back all payments
						continue;
					}

					var lastKind = GetHelper<SPHelper>().PopulateAction(data.Transactions, tranData);

					MappedPayment paymentObj = new MappedPayment(tranData, new Object[] { data.Id, tranData.Id }.KeyCombine(), data.Name ?? data.OrderNumber, tranData.DateModifiedAt.ToDate(false), tranData.CalculateHash()).With(_ => { _.ParentID = obj.SyncID; return _; });
					PXResult<BCSyncStatus> pxresult = GetBCSyncStatusResult(paymentObj.EntityType, paymentObj.SyncID, paymentObj.LocalID, paymentObj.ExternID);
					var currentPaymentStatus = pxresult == null ? null : pxresult.GetItem<BCSyncStatus>();

					var hasBeenVoidedTransaction = data.Transactions.Any(x => x.ParentId == tranData.Id && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Void
														  && x.Amount == tranData.Amount);

					//If Payment has been voided and never synced before, then we  skip it.
					if (tranData.Status == TransactionStatus.Success && (tranData.Kind == TransactionType.Void || hasBeenVoidedTransaction) && currentPaymentStatus == null)
					{
						continue;
					}

					EntityStatus paymentStatus = EnsureStatus(paymentObj, SyncDirection.Import);

					tranData.LastKind = lastKind;

					if (paymentStatus == EntityStatus.Pending)
					{
						bucket.Payments.Add(paymentObj);
					}
				}
			}
			BCExtensions.SetSharedSlot<OrderData>(this.GetType(), data.Id?.ToString(), data);
			return status;
		}

		public virtual void FetchOrderItemsLocationInfo(OrderData data)
		{
			//Get the Location info from FulfillmentOrder API
			IEnumerable<FulfillmentOrder> fulfillmentOrderList = fulfillmentOrderRestDataProvider.GetAll(data.Id.ToString());
			foreach (FulfillmentOrder item in fulfillmentOrderList ?? new List<FulfillmentOrder>())
			{
				//Should skip Canceled and Imcomplete status, use the new FulfillmentOrder instead.
				if (item.Status == FulfillmentOrderStatus.Canceled || item.Status == FulfillmentOrderStatus.Incomplete) continue;

				//In each assigned location, it includes the line items that should be fulfilled from this location.
				//We use the lineItemId to find the matched line item in order, and assign the locationId to associated order line item.
				foreach (FulfillmentLineItem lineDetail in item.LineItems ?? new List<FulfillmentLineItem>())
				{
					OrderLineItem matchedOrderLineItem = data.LineItems.FirstOrDefault(x => string.Equals(x.Id, lineDetail?.LineItemId));
					if (matchedOrderLineItem != null)
					{
						if (matchedOrderLineItem.OriginLocation != null)
							matchedOrderLineItem.OriginLocation.LocationId = item.AssignedLocationId;
						else
							matchedOrderLineItem.OriginLocation = new OrderItemLocation() { LocationId = item.AssignedLocationId };
					}
				}
			}
		}

		protected virtual CustomerAddressData GetMatchingLocation(OrderData data)
		{
			//Find proper location by all fields.
			return data.Customer.Addresses?.FirstOrDefault(x => String.Equals(x.City ?? string.Empty, data.ShippingAddress.City ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.Company ?? string.Empty, data.ShippingAddress.Company ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.CountryCode ?? string.Empty, data.ShippingAddress.CountryCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.FirstName ?? string.Empty, data.ShippingAddress.FirstName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.LastName ?? string.Empty, data.ShippingAddress.LastName ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.Phone ?? string.Empty, data.ShippingAddress.Phone ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.ProvinceCode ?? string.Empty, data.ShippingAddress.ProvinceCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.Address1 ?? string.Empty, data.ShippingAddress.Address1 ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.Address2 ?? string.Empty, data.ShippingAddress.Address2 ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)
			&& String.Equals(x.PostalCode ?? string.Empty, data.ShippingAddress.PostalCode ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));
		}


		/// <summary>
		/// Shopify does not provide an exchange rate but it does provide the total amount in the two currencies.		
		/// </summary>
		/// <param name="branchCurrency"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public virtual decimal? GetExchangeRate(string branchCurrency, OrderData data)
		{
			decimal? rate = null;

			//In case the presented currency is difference from the order currency, we need to set the reciprocal rate in case the Store currency is the same as the branch currency
			if (data.CurrencyPresentment != data.Currency)
			{
				//Shopify does not provide an exchange rate; however, it provides the total amount in the two currencies.
				if (data.Currency == branchCurrency &&
					data.CurrentSubTotalPriceSet?.PresentmentMoney != null &&
					data.CurrentTotalPrice.Value != data.CurrentTotalPriceSet?.PresentmentMoney?.Amount)
				{
					rate = data.CurrentTotalPriceSet?.PresentmentMoney?.Amount / data.CurrentTotalPrice.Value;
				}
			}
			return rate;
		}
		public override async Task MapBucketImport(SPSalesOrderBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			BCBinding binding = GetBinding();
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			BCBindingShopify bindingShopify = GetBindingExt<BCBindingShopify>();
			var branchCurrency = PX.Objects.GL.Branch.PK.Find(this, binding.BranchID).BaseCuryID;
			var storeDefaultCurrency = bindingExt.DefaultStoreCurrency;

			MappedOrder obj = bucket.Order;
			OrderData data = obj.Extern;
			SalesOrder impl = obj.Local = new SalesOrder();
			SalesOrder presented = existing?.Local as SalesOrder;
			// we can update only the order status with open/On hold/Back order
			if (presented != null && presented.Status?.Value != PX.Objects.SO.Messages.Open && presented.Status?.Value != PX.Objects.SO.Messages.Hold
				 && presented.Status?.Value != BCObjectsMessages.RiskHold && presented.Status?.Value != PX.Objects.SO.Messages.CreditHold && presented.Status?.Value != PX.Objects.SO.Messages.BackOrder)
			{
				throw new PXException(BCMessages.OrderStatusDoesNotAllowModification, presented.OrderNbr?.Value);
			}

			var branch = Branch.PK.Find(this, binding.BranchID);
			bool cancelledOrder = data?.CancelledAt != null;
			var description = PXMessages.LocalizeFormat(ShopifyMessages.OrderDescription, binding.BindingName, data.Name, data.FinancialStatus?.ToString());
			impl.Description = description.ValueField();
			impl.Custom = GetCustomFieldsForImport();

			#region SalesOrder
			//POS order
			if (string.Equals(data.SourceName, ShopifyConstants.POSSource, StringComparison.OrdinalIgnoreCase))
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.shopifyPOS>() != true || bindingShopify.ShopifyPOS != true)
					throw new PXException(ShopifyMessages.POSOrderNotSupported, $"{obj.Extern.Name}({obj.Extern.Id})");
				if (data.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)
					impl.OrderType = bindingShopify.POSDirectOrderType.ValueField();
				else if (data.FulfillmentStatus == null || data.FulfillmentStatus == OrderFulfillmentStatus.Null)
					impl.OrderType = bindingShopify.POSShippingOrderType.ValueField();
			}
			else
				impl.OrderType = bindingExt?.OrderType.ValueField();
			impl.ExternalOrderOrigin = binding.BindingName.ValueField();
			impl.ExternalOrderSource = data.SourceName.ValueField();
			var date = data.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(bindingExt?.OrderTimeZone));
			if (date.HasValue)
				impl.Date = (new DateTime(date.Value.Date.Ticks)).ValueField();
			impl.RequestedOn = impl.Date;

			impl.CurrencyID = data.CurrencyPresentment.ValueField();

			var rate = GetExchangeRate(branchCurrency, data);

			if (rate != null && rate != 1m)
			{
				LogInfo(Operation.LogScope(), BCMessages.LogExchangeRateOverride, data.CurrencyPresentment, branchCurrency, rate.Value, binding.BindingName, bucket.Order.ExternID);
				impl.ReciprocalRate = rate != null ? rate.ValueField() : null;
			}		
			//impl.CurrencyRate = data.CurrencyExchangeRate.ValueField();
			impl.CustomerOrder = GetCustomerOrderForImport(data, impl);

			impl.ExternalRef = APIHelper.ReferenceMake(data.Id, binding.BindingName).ValueField();
			impl.Note = data.Note;
			if (data.NoteAttributes != null)
			{
				var giftMessage = data.NoteAttributes.FirstOrDefault(x => string.Equals(x.Name, ShopifyConstants.GiftNote, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
				if (!string.IsNullOrEmpty(giftMessage))
					impl.Note = impl.Note + Environment.NewLine + PXMessages.LocalizeFormat(ShopifyMessages.GiftNote, giftMessage);
			}

			impl.ExternalOrderOriginal = true.ValueField();

			PX.Objects.CR.Address address = null;
			PX.Objects.CR.Contact contact = null;
			PX.Objects.CR.Location location = null;
			PX.Objects.AR.Customer customer = null;
			PXResult<PX.Objects.CR.Address, PX.Objects.CR.Contact> billingResult = null;
			PXResult<PX.Objects.CR.Location, PX.Objects.CR.Address, PX.Objects.CR.Contact> shippingResult = null;

			//If order is from individual customer, or is from guest customer without Customer and Company
			if (bucket.Customer != null || (bucket.Customer == null && bucket.Company == null))
			{
				//Customer ID
				if (bucket.Customer != null && data.Customer.Id > 0 && (!string.IsNullOrEmpty(data.Customer.Email) || !string.IsNullOrEmpty(data.Customer.Phone)) &&
					(!string.IsNullOrEmpty(data.Customer.FirstName) || !string.IsNullOrEmpty(data.Customer.LastName)))
				{
					var result = PXSelectJoin<PX.Objects.AR.Customer,
						LeftJoin<PX.Objects.CR.Address, On<PX.Objects.AR.Customer.defBillAddressID, Equal<PX.Objects.CR.Address.addressID>>,
						LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defBillContactID, Equal<PX.Objects.CR.Contact.contactID>>,
						LeftJoin<BCSyncStatus, On<PX.Objects.AR.Customer.noteID, Equal<BCSyncStatus.localID>>>>>,
						Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
							And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
							And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>.
							Select(this, BCEntitiesAttribute.Customer, data.Customer.Id).AsEnumerable().
							Cast<PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Address, PX.Objects.CR.Contact, BCSyncStatus>>().FirstOrDefault();
					customer = result?.GetItem<PX.Objects.AR.Customer>();
					address = result?.GetItem<PX.Objects.CR.Address>();
					if (customer == null) throw new PXException(BCMessages.CustomerNotSyncronized, data.Customer.Id);
					if (customer.Status != PX.Objects.AR.CustomerStatus.Active) throw new PXException(BCMessages.CustomerNotActive, data.Customer.Id);
					if (customer.CuryID == null)
					{
						string branchCuryID = branch?.BaseCuryID;
						if (branchCuryID != impl.CurrencyID.Value) throw new PXException(BCMessages.OrderCurrencyNotMatchBaseCurrency, impl.CurrencyID.Value, branchCuryID, customer.AcctCD);
					}
					else if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);
					impl.CustomerID = customer.AcctCD?.Trim().ValueField();
					billingResult = new PXResult<PX.Objects.CR.Address, PX.Objects.CR.Contact>(result?.GetItem<PX.Objects.CR.Address>(), result?.GetItem<PX.Objects.CR.Contact>());
				}
				else
				{
					PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Contact, PX.Objects.CR.Address> result = null;

					// If no customer check existing for customer
					if (!string.IsNullOrEmpty(presented?.CustomerID?.Value))
					{
						result = PXSelectJoin<PX.Objects.AR.Customer,
									LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defBillContactID, Equal<PX.Objects.CR.Contact.contactID>>,
										LeftJoin<PX.Objects.CR.Address, On<PX.Objects.AR.Customer.defBillAddressID, Equal<PX.Objects.CR.Address.addressID>>>>,
								Where<PX.Objects.AR.Customer.acctCD, Equal<Required<PX.Objects.AR.Customer.acctCD>>>>.
							Select(this, presented?.CustomerID?.Value).
							Cast<PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Contact, PX.Objects.CR.Address>>().FirstOrDefault();
						customer = result?.GetItem<PX.Objects.AR.Customer>();
						address = result?.GetItem<PX.Objects.CR.Address>();
					}

					// If no customer yet use email to find one.
					if (customer == null && !string.IsNullOrEmpty(data.Email))
					{
						result = PXSelectJoin<PX.Objects.AR.Customer,
						   LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defBillContactID, Equal<PX.Objects.CR.Contact.contactID>>,
						   LeftJoin<PX.Objects.CR.Address, On<PX.Objects.AR.Customer.defBillAddressID, Equal<PX.Objects.CR.Address.addressID>>>>,
						   Where<PX.Objects.CR.Contact.eMail, Equal<Required<PX.Objects.CR.Contact.eMail>>>>.
						   Select(this, data.Email).
						   Cast<PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Contact, PX.Objects.CR.Address>>().FirstOrDefault();
						customer = result?.GetItem<PX.Objects.AR.Customer>();
						address = result?.GetItem<PX.Objects.CR.Address>();
					}

					// If no customer yet use phone to find one.
					if (customer == null && !string.IsNullOrEmpty(data.Phone))
					{
						result = PXSelectJoin<PX.Objects.AR.Customer,
						   LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.defBillContactID, Equal<PX.Objects.CR.Contact.contactID>>,
						   LeftJoin<PX.Objects.CR.Address, On<PX.Objects.AR.Customer.defBillAddressID, Equal<PX.Objects.CR.Address.addressID>>>>,
						   Where<PX.Objects.CR.Contact.phone1, Equal<Required<PX.Objects.CR.Contact.phone1>>, Or<PX.Objects.CR.Contact.phone2, Equal<Required<PX.Objects.CR.Contact.phone2>>>>>.
						   Select(this, data.Phone, data.Phone).
						   Cast<PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Contact, PX.Objects.CR.Address>>().FirstOrDefault();
						customer = result?.GetItem<PX.Objects.AR.Customer>();
						address = result?.GetItem<PX.Objects.CR.Address>();
					}

					// If no customer yet use the guest customer
					if (customer == null)
					{
						customer = GuestCustomerService?.GetGuestCustomer(this, cbapi, GetBindingExt<BCBindingExt>());
						if (customer == null) throw new PXException(ShopifyMessages.NoGuestCustomer);
					}
					else
					{
						if (customer.CuryID == null)
						{
							string branchCuryID = branch?.BaseCuryID;
							if (branchCuryID != impl.CurrencyID.Value) throw new PXException(BCMessages.OrderCurrencyNotMatchBaseCurrency, impl.CurrencyID.Value, branchCuryID, customer.AcctCD);
						}
						else if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);
						billingResult = new PXResult<PX.Objects.CR.Address, PX.Objects.CR.Contact>(result?.GetItem<PX.Objects.CR.Address>(), result?.GetItem<PX.Objects.CR.Contact>());

					}
					if (customer.Status != PX.Objects.AR.CustomerStatus.Active) throw new PXException(BCMessages.CustomerNotActive, data.Customer.Id);
					impl.CustomerID = customer.AcctCD?.Trim().ValueField();
				}

				//Location ID
				if (customer != null && bucket.Location != null)
				{
					PXResult<PX.Objects.CR.Location> syncedLocation = PXSelectJoin<PX.Objects.CR.Location,
							InnerJoin<BCSyncStatus,
								On<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
									And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
									And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
									And<BCSyncStatus.localID, Equal<PX.Objects.CR.Location.noteID>,
									And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>>>>>>,
							LeftJoin<PX.Objects.CR.Address,
								On<PX.Objects.CR.Location.defAddressID, Equal<PX.Objects.CR.Address.addressID>>,
							LeftJoin<PX.Objects.CR.Contact,
								On<PX.Objects.CR.Location.defContactID, Equal<PX.Objects.CR.Contact.contactID>>
							>>>,
							Where<PX.Objects.CR.Location.bAccountID, Equal<Required<PX.Objects.CR.Location.bAccountID>>, And<BCSyncStatus.syncID, IsNotNull>>>
						.Select(this, BCEntitiesAttribute.Address, bucket?.Location?.ExternID, customer.BAccountID);
					if (syncedLocation != null)
					{
						location = syncedLocation?.GetItem<PX.Objects.CR.Location>();
						impl.LocationID = location.LocationCD?.Trim().ValueField();

						shippingResult = new PXResult<PX.Objects.CR.Location, PX.Objects.CR.Address, PX.Objects.CR.Contact>(
							syncedLocation?.GetItem<PX.Objects.CR.Location>(),
							syncedLocation?.GetItem<PX.Objects.CR.Address>(),
							syncedLocation?.GetItem<PX.Objects.CR.Contact>());
					}
					else
					{
						PXResult<PX.Objects.CR.Location> defaultLocaltion = PXSelectJoin<PX.Objects.CR.Location,
							InnerJoin<PX.Objects.CR.BAccount,
								On<PX.Objects.CR.Location.bAccountID, Equal<PX.Objects.CR.BAccount.bAccountID>,
									And<PX.Objects.CR.Location.locationID, Equal<PX.Objects.CR.BAccount.defLocationID>>>,
							LeftJoin<PX.Objects.CR.Address,
								On<PX.Objects.CR.Location.defAddressID, Equal<PX.Objects.CR.Address.addressID>>,
							LeftJoin<PX.Objects.CR.Contact,
								On<PX.Objects.CR.Location.defContactID, Equal<PX.Objects.CR.Contact.contactID>>
							>>>,
							Where<PX.Objects.CR.Location.bAccountID, Equal<Required<PX.Objects.CR.Location.bAccountID>>>>
							.Select(this, customer.BAccountID);
						if (defaultLocaltion != null)
						{
							location = defaultLocaltion?.GetItem<PX.Objects.CR.Location>();
							impl.LocationID = location.LocationCD?.Trim().ValueField();

							shippingResult = new PXResult<PX.Objects.CR.Location, PX.Objects.CR.Address, PX.Objects.CR.Contact>(
								defaultLocaltion?.GetItem<PX.Objects.CR.Location>(),
								defaultLocaltion?.GetItem<PX.Objects.CR.Address>(),
								defaultLocaltion?.GetItem<PX.Objects.CR.Contact>());
						}
					}
				}
			}
			else if (bucket.Company != null) //For Company customer order
			{
				Guid? companyLocalID = bucket.Company.LocalID;
				Guid? contactLocalID = bucket.Company.Details?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyContact && string.Equals(x.ExternID.KeySplit(0), data.Customer?.Id.ToString()))?.LocalID;
				Guid? locationLocalID = bucket.Company.Details?.FirstOrDefault(x => x.EntityType == BCEntitiesAttribute.CompanyLocation && string.Equals(x.ExternID, data.Company.LocationId.ToString()))?.LocalID;

				//If any companyLocalID,contactLocalID,locationLocalID is null, that means the company doesn't import correctly, should throw error to user.
				if (companyLocalID == null || locationLocalID == null)
				{
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyNotSyncronized, bucket.Company.ExternID));
				}

				if (contactLocalID == null)
				{
					var contactNameWithID = $"{data.Customer.FirstName} {data.Customer.LastName} ({data.Customer?.Id})";
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyContactNotSyncronized, contactNameWithID, data.Company.LocationId));
				}

				var result = PXSelectJoin<PX.Objects.AR.Customer,
						LeftJoin<PX.Objects.CR.Address, On<PX.Objects.AR.Customer.defBillAddressID, Equal<PX.Objects.CR.Address.addressID>>,
						LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.AR.Customer.bAccountID, Equal<PX.Objects.CR.Contact.bAccountID>>>>,
						Where<PX.Objects.AR.Customer.noteID, Equal<Required<PX.Objects.AR.Customer.noteID>>,
							And<PX.Objects.CR.Contact.noteID, Equal<Required<PX.Objects.CR.Contact.noteID>>,
							And<PX.Objects.AR.Customer.customerCategory, Equal<Required<PX.Objects.AR.Customer.customerCategory>>>>>>.
							Select(this, companyLocalID, contactLocalID, BCCustomerCategoryAttribute.OrganizationValue).AsEnumerable().
							Cast<PXResult<PX.Objects.AR.Customer, PX.Objects.CR.Address, PX.Objects.CR.Contact>>().FirstOrDefault();
				customer = result?.GetItem<PX.Objects.AR.Customer>();
				address = result?.GetItem<PX.Objects.CR.Address>();
				if (customer == null) throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyNotSyncronized, bucket.Company.ExternID));
				if (customer.Status != PX.Objects.AR.CustomerStatus.Active) throw new PXException(BCMessages.CustomerNotActive, customer.AcctCD);
				if (customer.CuryID == null)
				{
					string branchCuryID = branch?.BaseCuryID;
					if (branchCuryID != impl.CurrencyID.Value) throw new PXException(BCMessages.OrderCurrencyNotMatchBaseCurrency, impl.CurrencyID.Value, branchCuryID, customer.AcctCD);
				}
				else if (customer.CuryID != impl.CurrencyID.Value && !customer.AllowOverrideCury.Value) throw new PXException(BCMessages.OrderCurrencyNotMathced, impl.CurrencyID.Value, customer.CuryID);

				impl.CustomerID = customer.AcctCD?.Trim().ValueField();
				impl.ContactID = result?.GetItem<PX.Objects.CR.Contact>()?.ContactID?.ToString().ValueField();

				billingResult = new PXResult<PX.Objects.CR.Address, PX.Objects.CR.Contact>(result?.GetItem<PX.Objects.CR.Address>(), result?.GetItem<PX.Objects.CR.Contact>());

				//Location
				PXResult<PX.Objects.CR.Location> syncedLocation = PXSelectJoin<PX.Objects.CR.Location,
						LeftJoin<PX.Objects.CR.Address, On<PX.Objects.CR.Location.defAddressID, Equal<PX.Objects.CR.Address.addressID>>,
						LeftJoin<PX.Objects.CR.Contact, On<PX.Objects.CR.Location.defContactID, Equal<PX.Objects.CR.Contact.contactID>>>>,
						Where<PX.Objects.CR.Location.noteID, Equal<Required<PX.Objects.CR.Location.noteID>>>>
					.Select(this, locationLocalID);
				if (syncedLocation != null)
				{
					location = syncedLocation?.GetItem<PX.Objects.CR.Location>();
					impl.LocationID = location.LocationCD?.Trim().ValueField();

					shippingResult = new PXResult<PX.Objects.CR.Location, PX.Objects.CR.Address, PX.Objects.CR.Contact>(
						syncedLocation?.GetItem<PX.Objects.CR.Location>(),
						syncedLocation?.GetItem<PX.Objects.CR.Address>(),
						syncedLocation?.GetItem<PX.Objects.CR.Contact>());
				}
			}

			impl.FinancialSettings = new FinancialSettings();
			impl.FinancialSettings.Branch = branch?.BranchCD?.Trim().ValueField();
			#endregion

			#region ShippingSettings
			//Freight
			impl.Totals = new Totals();
			impl.Totals.OverrideFreightAmount = new BooleanValue() { Value = true };
			impl.Totals.OrderWeight = data.TotalWeightInGrams.ValueField();

			List<OrderAdjustment> refundOrderAdjustments = null;
			List<RefundLineItem> refundItems = null;
			refundOrderAdjustments = data.Refunds?.Count > 0 ? data.Refunds.SelectMany(x => x.OrderAdjustments)?.ToList() : null;
			refundItems = data.Refunds?.Count > 0 ? data.Refunds.SelectMany(x => x.RefundLineItems)?.ToList() : null;

			if (data.Refunds?.Count > 0)
				impl.ExternalRefundRef = string.Join(";", data.Refunds.Select(x => x.Id)).ValueField();
			decimal shippingrefundAmt = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.AmountPresentment) ?? 0m) ?? 0m;
			decimal shippingrefundAmtTax = refundOrderAdjustments?.Where(x => x.Kind == OrderAdjustmentType.ShippingRefund)?.Sum(x => (-x.TaxAmountPresentment) ?? 0m) ?? 0m;
			decimal shippingRefundTotal = (data.TaxesIncluded == true) ? shippingrefundAmt + shippingrefundAmtTax : shippingrefundAmt;
			//Included the shipping discount, if there is a free shipping discount applied, freight fee should be 0.
			//reduce the shipping refund amount from freight
			decimal shippingCost = data.ShippingLines.Sum(x => x.ShippingCostExcludingTaxPresentment) ?? 0m;
			decimal shipmentDiscounts = data.ShippingLines.SelectMany(x => x.DiscountAllocations)?.Sum(x => x?.DiscountAmountPresentment ?? 0m) ?? 0m;
			impl.Totals.Freight = (shippingCost - shipmentDiscounts - shippingRefundTotal).ValueField();
			if (impl.Totals.Freight.Value < 0)
				throw new PXException(BCMessages.ValueCannotBeLessThenZero, shipmentDiscounts, shippingCost);

			//ShippingSettings
			impl.ShippingSettings = new ShippingSettings();
			PXCache cache = base.Caches[typeof(BCShippingMappings)];

			var shippingLine = data.ShippingLines?.FirstOrDefault();
			bool hasMappingError = false;
			String zoneName = string.Empty;
			String shippingMethod = shippingLine?.Title?.Trim() ?? string.Empty;
			if (shippingLine != null)
			{
				storeShippingZones = storeShippingZones ?? storeDataProvider.GetShippingZones(cancellationToken).ToList();
				BCShippingMappings mappingValue = null;

				// for Delivery method Pickp Shipping address is not present in that case keep shippingzone as empty.
				if (data.ShippingAddress != null)
				{
					//Compare ShippingZones data to find ShippingMapping record first, and then search wtih ShippingLine data again if no record is found.
					var shippingZone = storeShippingZones.FirstOrDefault(x => x.Countries?.Count > 0 && x.Countries.Any(c => string.Equals(c.Code, data.ShippingAddress.CountryCode, StringComparison.InvariantCultureIgnoreCase))) ??
											storeShippingZones.FirstOrDefault(x => x.Countries?.Count > 0 && x.Countries.Any(c => c.Code == "*"));
					zoneName = shippingZone != null ? shippingZone.Name?.Trim() : shippingLine.Code ?? string.Empty;
				}
				mappingValue = GetHelper<SPHelper>().ShippingMethods().FirstOrDefault(m =>
						string.Equals(m.ShippingZone, zoneName, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(m.ShippingMethod, shippingMethod, StringComparison.OrdinalIgnoreCase));
				if (mappingValue != null)
				{
					if (mappingValue.Active == true && mappingValue.CarrierID == null)
					{
						hasMappingError = true;
					}
					else if (mappingValue.Active == true && mappingValue.CarrierID != null)
					{
						impl.ShipVia = impl.ShippingSettings.ShipVia = mappingValue.CarrierID.ValueField();
						impl.ShippingSettings.ShippingZone = mappingValue.ZoneID.ValueField();
						impl.ShippingSettings.ShippingTerms = mappingValue.ShipTermsID.ValueField();
					}
				}
				else
				{
					hasMappingError = true;
					BCShippingMappings inserted = new BCShippingMappings() { BindingID = Operation.Binding, ShippingZone = zoneName, ShippingMethod = shippingMethod, Active = true };
					cache.Insert(inserted);
				}
			}

			if (cache.Inserted.Count() > 0)
				cache.Persist(PXDBOperation.Insert);
			if (hasMappingError)
			{
				throw new PXException(BCMessages.OrderShippingMappingIsMissing, zoneName, shippingMethod);
			}

			#region Ship-To Address && Contact

			if (data.ShippingAddress != null)
			{

				impl.ShipToAddress = new Core.API.Address();
				impl.ShipToAddress.AddressLine1 = (data.ShippingAddress.Address1 ?? string.Empty).ValueField();
				impl.ShipToAddress.AddressLine2 = (data.ShippingAddress.Address2 ?? string.Empty).ValueField();
				impl.ShipToAddress.City = data.ShippingAddress.City.ValueField();
				impl.ShipToAddress.Country = data.ShippingAddress.CountryCode.ValueField();
				if (impl.ShipToAddress.Country?.Value != null && !string.IsNullOrEmpty(data.ShippingAddress.ProvinceCode))
				{
					impl.ShipToAddress.State = GetHelper<SPHelper>().SearchStateID(impl.ShipToAddress.Country?.Value, data.ShippingAddress.Province, data.ShippingAddress.ProvinceCode)?.ValueField();
				}
				else
					impl.ShipToAddress.State = string.Empty.ValueField();
				impl.ShipToAddress.PostalCode = data.ShippingAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.ShipToContact = new DocContact();
				impl.ShipToContact.Phone1 = data.ShippingAddress.Phone.ValueField();
				impl.ShipToContact.Email = data.Email.ValueField();
				impl.ShipToContact.Attention = data.ShippingAddress.Name.ValueField();
				impl.ShipToContact.BusinessName = data.ShippingAddress.Company.ValueField();

				impl.ShipToAddressOverride = true.ValueField();
				impl.ShipToContactOverride = true.ValueField();
				if (customer.BAccountID != GetBindingExt<BCBindingExt>()?.GuestCustomerID)
				{
					address = shippingResult?.GetItem<PX.Objects.CR.Address>();
					if (address != null && CompareAddress(impl.ShipToAddress, address))
						impl.ShipToAddressOverride = false.ValueField();

					contact = shippingResult?.GetItem<PX.Objects.CR.Contact>();
					location = shippingResult?.GetItem<PX.Objects.CR.Location>();
					if (contact != null && CompareContact(impl.ShipToContact, contact, location))
						impl.ShipToContactOverride = false.ValueField();
				}

			}
			else
			{
				impl.ShipToAddress = new Core.API.Address();
				impl.ShipToAddress.AddressLine1 = string.Empty.ValueField();
				impl.ShipToAddress.AddressLine2 = string.Empty.ValueField();
				impl.ShipToAddress.City = string.Empty.ValueField();
				impl.ShipToAddress.State = string.Empty.ValueField();
				impl.ShipToAddress.PostalCode = string.Empty.ValueField();
				impl.ShipToContact = new DocContact();
				impl.ShipToContact.Phone1 = string.Empty.ValueField();
				impl.ShipToContact.Email = data.Email.ValueField();
				impl.ShipToContact.Attention = string.Empty.ValueField();
				impl.ShipToContact.BusinessName = string.Empty.ValueField();
				impl.ShipToAddressOverride = true.ValueField();
				impl.ShipToContactOverride = true.ValueField();
				impl.ShipToAddress.Country = (data.Customer?.DefaultAddress?.CountryCode ?? data.Customer?.Addresses?.FirstOrDefault()?.CountryCode ?? address?.CountryID)?.ValueField();
			}
			#endregion

			#endregion

			#region	Bill-To Address && Contact

			impl.BillToAddress = new Core.API.Address();
			impl.BillToContact = new DocContact();
			if (data.BillingAddress == null && data.ShippingAddress != null)
			{
				impl.BillToAddress.AddressLine1 = impl.ShipToAddress.AddressLine1;
				impl.BillToAddress.AddressLine2 = impl.ShipToAddress.AddressLine2;
				impl.BillToAddress.City = impl.ShipToAddress.City;
				impl.BillToAddress.Country = impl.ShipToAddress.Country;
				impl.BillToAddress.State = impl.ShipToAddress.State;
				impl.BillToAddress.PostalCode = impl.ShipToAddress.PostalCode;

				impl.BillToContact.Phone1 = impl.ShipToContact.Phone1;
				impl.BillToContact.Email = impl.ShipToContact.Email;
				impl.BillToContact.BusinessName = impl.ShipToContact.BusinessName;
				impl.BillToContact.Attention = impl.ShipToContact.Attention;
			}
			else if (data.BillingAddress != null)
			{
				impl.BillToAddress.AddressLine1 = (data.BillingAddress.Address1 ?? string.Empty).ValueField();
				impl.BillToAddress.AddressLine2 = (data.BillingAddress.Address2 ?? string.Empty).ValueField();
				impl.BillToAddress.City = data.BillingAddress.City.ValueField();
				impl.BillToAddress.Country = data.BillingAddress.CountryCode.ValueField();
				if (!string.IsNullOrEmpty(data.BillingAddress.ProvinceCode) && data.BillingAddress.ProvinceCode.Equals(data.ShippingAddress?.ProvinceCode))
				{
					impl.BillToAddress.State = impl.ShipToAddress.State;
				}
				else if (impl.BillToAddress.Country?.Value != null && !string.IsNullOrEmpty(data.BillingAddress.ProvinceCode))
				{
					impl.BillToAddress.State = GetHelper<SPHelper>().SearchStateID(impl.BillToAddress.Country?.Value, data.BillingAddress.Province, data.BillingAddress.ProvinceCode)?.ValueField();
				}
				else
					impl.BillToAddress.State = string.Empty.ValueField();
				impl.BillToAddress.PostalCode = data.BillingAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.BillToContact.Phone1 = data.BillingAddress.Phone.ValueField();
				impl.BillToContact.Email = data.Email.ValueField();
				impl.BillToContact.BusinessName = data.BillingAddress.Company.ValueField();
				impl.BillToContact.Attention = data.BillingAddress.Name.ValueField();
			}
			else if (data.BillingAddress == null && data.Customer?.DefaultAddress != null)
			{
				impl.BillToAddress.AddressLine1 = (data.Customer?.DefaultAddress.Address1 ?? string.Empty).ValueField();
				impl.BillToAddress.AddressLine2 = (data.Customer?.DefaultAddress.Address2 ?? string.Empty).ValueField();
				impl.BillToAddress.City = data.Customer?.DefaultAddress.City.ValueField();
				impl.BillToAddress.Country = data.Customer?.DefaultAddress.CountryCode.ValueField();
				if (!string.IsNullOrEmpty(data.Customer.DefaultAddress.ProvinceCode) && data.Customer.DefaultAddress.ProvinceCode.Equals(data.ShippingAddress?.ProvinceCode))
				{
					impl.BillToAddress.State = impl.ShipToAddress.State;
				}
				if (impl.BillToAddress.Country?.Value != null && !string.IsNullOrEmpty(data.Customer?.DefaultAddress.ProvinceCode))
				{
					impl.BillToAddress.State = GetHelper<SPHelper>().SearchStateID(impl.BillToAddress.Country?.Value, data.Customer?.DefaultAddress.Province, data.Customer?.DefaultAddress.ProvinceCode)?.ValueField();
				}
				else
					impl.BillToAddress.State = string.Empty.ValueField();
				impl.BillToAddress.PostalCode = data.Customer?.DefaultAddress.PostalCode?.ToUpperInvariant()?.ValueField();

				impl.BillToContact.Phone1 = data.Customer?.DefaultAddress.Phone.ValueField();
				impl.BillToContact.Email = data.Email.ValueField();
				impl.BillToContact.BusinessName = data.Customer?.DefaultAddress.Company.ValueField();
				impl.BillToContact.Attention = data.Customer?.DefaultAddress.Name.ValueField();
			}
			impl.BillToContactOverride = true.ValueField();
			impl.BillToAddressOverride = true.ValueField();
			if (customer.BAccountID != GetBindingExt<BCBindingExt>()?.GuestCustomerID)
			{
				address = billingResult?.GetItem<PX.Objects.CR.Address>();
				if (address != null && CompareAddress(impl.BillToAddress, address))
					impl.BillToAddressOverride = false.ValueField();

				contact = billingResult?.GetItem<PX.Objects.CR.Contact>();
				if (contact != null && CompareContact(impl.BillToContact, contact))
					impl.BillToContactOverride = false.ValueField();
			}
			#endregion

			#region Products
			impl.Details = new List<SalesOrderDetail>();
			Decimal? totalDiscount = 0m;
			String orderLevelLocation = data.LocationId?.ToString();
			List<BCLocations> locationMappings = BCLocationSlot.GetBCLocations(binding.BindingID);
			foreach (var orderItem in data.LineItems)
			{
				decimal? quantity = orderItem.Quantity;
				decimal? subTotal = orderItem.PricePresentment * quantity;
				//Check refund data whether have this orderItem data
				List<RefundLineItem> matchedRefundItems = null;
				decimal? refundSubtotal = 0;
				decimal? refundQuantity = 0;
				decimal? itemDiscount = 0;
				SalesOrderDetail detail = new SalesOrderDetail();
				detail.DiscountAmount = 0m.ValueField();
				if (refundItems?.Count > 0 && refundItems.Any(x => x.LineItemId == orderItem.Id))
				{
					matchedRefundItems = refundItems.Where(x => x.LineItemId == orderItem.Id).ToList();
					//Should not change the original order quantity if RestockType is Return or NoRestock(We cannot support Unfulfilled item that set to NoRestock) because it's from fulfilled item, we should create RC order instead
					refundQuantity = matchedRefundItems.Where(x => x.RestockType != null && x.RestockType == RestockType.Cancel).Sum(x => x.Quantity);

					//If Admin modifies the item quantity and then changes back to original quantity, Shopify will keep the total quantity and use it to do the calculation in the item;
					//and add a new record to the refund item to keep the same amount. So we have to use this data to re-calculate the tax and discount if they applied.
					quantity = orderItem.Quantity - refundQuantity;
					subTotal = orderItem.PricePresentment * quantity;
					refundSubtotal = matchedRefundItems.Sum(x => x.SubTotalPresentment);
				}

				if (orderItem.DiscountAllocations?.Count > 0)
				{
					itemDiscount = orderItem.DiscountAllocations.Sum(x => x.DiscountAmountPresentment);
					if (refundSubtotal != 0)
					{
						//Manual calculate the item discount if there is a refund item
						itemDiscount = itemDiscount + refundSubtotal - (orderItem.PricePresentment * refundQuantity);
					}
					totalDiscount += itemDiscount;
					if (bindingExt?.PostDiscounts == BCPostDiscountAttribute.LineDiscount)
					{
						detail.DiscountAmount = itemDiscount.ValueField();
					}
				}
				//If there is an refund item, remove the discount applied to that item from discount total.
				if (refundSubtotal != 0 || quantity == 0)
				{
					totalDiscount -= itemDiscount;
				}

				//Sometimes Shopify API returns a product and variants with SKU, but without SKU for a line item, what causes an improper mapping. This is NOT expected of Shopify.
				//As a fix for the connector, we added the following condition to fetch the product in case SKU is empty. However, it is possible that one orderItem doesn't exist
				//externally ("Tip" as example), so we have to bypass here and try to get from the helper.
				if (string.IsNullOrEmpty(orderItem.Sku) && orderItem.ProductId != null)
				{
					if (orderItem.VariantId != null)
						orderItem.Sku = productVariantDataProvider.GetByID(orderItem.ProductId?.ToString(), orderItem.VariantId.ToString())?.Sku;
					else
					{
						var product = productDataProvider.GetByID(orderItem.ProductId?.ToString());
						orderItem.Sku = product?.Variants?.FirstOrDefault()?.Sku;
					}
				}

				String inventoryCD = GetHelper<SPHelper>().GetInventoryCDByExternID(
					orderItem.ProductId?.ToString(),
					orderItem.VariantId.ToString(),
					orderItem.Sku ?? string.Empty,
					orderItem.Name,
					orderItem.IsGiftCard,
					out string uom,
					out string alternateID,
					out string itemStatus,
					out bool? stkItem,
					out bool? nonStockShip);

				var unitPrice = orderItem.PricePresentment;
				HandleNegativePriceItem(data, orderItem, stkItem, nonStockShip, ref unitPrice, ref quantity, ref subTotal);

				detail.Branch = impl.FinancialSettings.Branch;
				detail.InventoryID = inventoryCD?.TrimEnd().ValueField();
				detail.OrderQty = quantity.ValueField();
				detail.UOM = uom.ValueField();
				detail.UnitPrice = unitPrice.ValueField();
				detail.LineDescription = orderItem.Name.ValueField();
				detail.ExtendedPrice = subTotal.ValueField();
				detail.FreeItem = (orderItem.PricePresentment == 0m).ValueField();
				detail.ManualPrice = true.ValueField();
				detail.ExternalRef = orderItem.Id.ToString().ValueField();
				detail.AlternateID = alternateID?.ValueField();

				//Warehouse and Location mapping
				//Find the mapping based on the following order
				//1. Find the location in Order line item first.
				//2. If not found, find the location in Order level
				BCLocations matchedMapping = locationMappings?.FirstOrDefault(x => x.MappingDirection == BCMappingDirectionAttribute.Import
												&& orderItem.OriginLocation?.LocationId != null && string.Equals(x.ExternalLocationID, orderItem.OriginLocation?.LocationId?.ToString())) ??
											 locationMappings?.FirstOrDefault(x => x.MappingDirection == BCMappingDirectionAttribute.Import
												&& !string.IsNullOrEmpty(orderLevelLocation) && string.Equals(x.ExternalLocationID, orderLevelLocation));
				if (matchedMapping != null)
				{
					if (matchedMapping.SiteID.HasValue && BCLocationSlot.GetWarehouses(binding.BindingID).TryGetValue(matchedMapping.SiteID.Value, out INSite warehouse))
						detail.WarehouseID = warehouse?.SiteCD?.ValueField();
					if (matchedMapping.LocationID.HasValue && BCLocationSlot.GetLocations(binding.BindingID).TryGetValue(matchedMapping.LocationID.Value, out INLocation loc))
						detail.Location = loc?.LocationCD?.ValueField();
				}

				//Check for existing
				DetailInfo matchedDetail = existing?.Details?.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && orderItem.Id.ToString() == d.ExternID);
				if (matchedDetail != null) detail.Id = matchedDetail.LocalID; //Search by Details

				if (presented?.Details != null && presented.Details.Any()) //Serach by Existing line
				{
					SalesOrderDetail matchedLine = presented?.Details?.FirstOrDefault(x =>
						(x.ExternalRef?.Value != null && x.ExternalRef?.Value == orderItem.Id.ToString())
						||
						(x.InventoryID?.Value?.Trim() == detail?.InventoryID?.Value?.Trim() && (detail?.UOM == null || detail?.UOM.Value == x.UOM?.Value)));

					if (matchedLine != null && !impl.Details.Any(i => i.Id == matchedLine.Id))
					{
						detail.Id = matchedLine.Id;
						//If order quantity changed because of fulfilled item, we should keep original quantity in SalesOrder because we have created a refund order for fulfilled item
						//Otherwise we should apply the changes to existing SalesOrder in AC
						bool hasDifferentFulfilledQty = (quantity - (decimal?)orderItem.FulfillableQuantity) != matchedLine.QtyOnShipments.Value;
						if (hasDifferentFulfilledQty)
						{
							//Fulfillable Qty (eStore) > Open Qty(ERP) => Raise an error
							if ((decimal?)orderItem.FulfillableQuantity > matchedLine.OpenQty.Value)
								throw new PXException(BCMessages.OrderFulfillableQtyNotMatch);

							decimal? newQty = (decimal?)orderItem.FulfillableQuantity + matchedLine.QtyOnShipments.Value;

							detail.OrderQty = newQty.ValueField();
							detail.ExtendedPrice = (orderItem.PricePresentment * newQty).ValueField();
						}
					}
				}

				if (detail.Id == null && (itemStatus == BCINItemStatus.Inactive || itemStatus == BCINItemStatus.NoSales || itemStatus == BCINItemStatus.ToDelete))
					throw new PXException(BCMessages.InvalidItemStatus, inventoryCD, BCINItemStatus.Convert(itemStatus));

				impl.Details.Add(detail);
			}
			#endregion

			#region Add RefundItem Line

			foreach (var oneRefund in data.Refunds ?? new List<OrderRefund>())
			{
				var refundAdjustments = oneRefund.OrderAdjustments?.Where(x => x != null && x.Kind == OrderAdjustmentType.RefundDiscrepancy);
				//Should ignore order adjustment if there is no transaction in the refund.
				//If there is no Refund Transaction it means the payment is not captured. So shouldn't import the Order Adjustment. Otherwise, it would be treated as double refund.
				//Should ignore order adjustment if it comes from the fulfilled items only, and import these adjustment in customer refund payment
				bool shouldSkipAdjustment = refundAdjustments?.Any() != true || oneRefund.Transactions?.Any() != true ||
					(oneRefund.RefundLineItems?.Any() == true && oneRefund.RefundLineItems.All(x => x.RestockType != RestockType.Cancel));
				if (shouldSkipAdjustment)
					continue;

				var adjustAmount = refundAdjustments.Sum(x => x.AmountPresentment ?? 0m);
				if (adjustAmount == 0m) continue;

				//The admustAmount from Shopify data has  the +/- symbol already, we should keep the symbol when order adjustment imported into Acumatica
				var detail = InsertRefundAmountItem(adjustAmount, impl.FinancialSettings.Branch);
				detail.ExternalRef = oneRefund.Id?.ToString().ValueField();
				//Put the refund adjustment reason to the Description.
				detail.LineDescription = refundAdjustments.FirstOrDefault()?.Reason.ValueField();
				if (presented != null && presented.Details?.Count > 0)
				{
					presented.Details.FirstOrDefault(x => string.Equals(x.InventoryID.Value, detail.InventoryID.Value) && string.Equals(x.ExternalRef?.Value, detail.ExternalRef?.Value))
						.With(e => detail.Id = e.Id);
				}
				impl.Details.Add(detail);
			}

			#endregion

			#region Taxes
			impl.TaxDetails = new List<TaxDetail>();
			bool isAutomaticTax = bindingExt.TaxSynchronization == true && bindingExt.DefaultTaxZoneID != null && bindingExt.UseAsPrimaryTaxZone == true;
			if (data.TaxLines?.Count > 0)
			{
				if (GetBindingExt<BCBindingExt>()?.TaxSynchronization == true)
				{
					impl.IsTaxValid = true.ValueField();
					string taxType = isAutomaticTax ? GetHelper<SPHelper>().DetermineTaxType(data.TaxLines.Select(i => i.TaxName).ToList()) : null;
					foreach (OrderTaxLine tax in data.TaxLines)
					{
						var order = bucket.Order.Extern;
						string countryCode = order.ShippingAddress?.CountryCode ?? order.BillingAddress?.CountryCode;
						string provinceCode = order.ShippingAddress?.ProvinceCode ?? order.BillingAddress?.ProvinceCode;

						if (countryCode == null && provinceCode == null)
						{
							if (order.LocationId != null)
							{
								var orderLocation = inventoryLocationRestDataProvider.GetByID(order.LocationId.ToString());
								countryCode = orderLocation?.CountryCode;
								provinceCode = orderLocation?.ProvinceCode;
							}
							else
							{
								var firstLineItem = order.LineItems?.FirstOrDefault();
								countryCode = firstLineItem?.OriginLocation?.CountryCode;
								provinceCode = firstLineItem?.OriginLocation?.ProvinceCode;
							}
						}
						string taxName = GetHelper<SPHelper>().SubstituteTaxName(GetBindingExt<BCBindingExt>(), tax.TaxName, countryCode, provinceCode);
						if (string.IsNullOrEmpty(taxName)) throw new PXException(BCObjectsMessages.TaxNameDoesntExist);
						decimal? taxable = 0m, taxableExcludeRefundItems = 0m;
						decimal? taxAmount = 0m;

						var lineItemsWithTax = data.LineItems.Where(x => x.TaxLines?.Count > 0 && x.TaxLines.Any(t => t.TaxAmountPresentment > 0m && t.TaxName == tax.TaxName));
						var shippingItemsWithTax = data.ShippingLines.Where(x => x.TaxLines?.Count > 0 && x.TaxLines.Any(t => t.TaxAmountPresentment > 0m && t.TaxName == tax.TaxName));
						taxable = lineItemsWithTax.Sum(x => (x?.PricePresentment * x?.Quantity) ?? 0m) - lineItemsWithTax.SelectMany(x => x.DiscountAllocations)?.Sum(x => x.DiscountAmountPresentment ?? 0m);
						taxable += shippingItemsWithTax.Sum(x => x.ShippingCostExcludingTaxPresentment ?? 0m) - shippingItemsWithTax.SelectMany(x => x.DiscountAllocations)?.Sum(x => x.DiscountAmountPresentment ?? 0m) - shippingrefundAmt;
						taxableExcludeRefundItems = taxable;
						taxAmount = tax.TaxAmountPresentment;

						// if a tax rate is not received, we need to calculate it
						bool NeedToCalculateTaxRate = tax.TaxRate == 0 && taxAmount != 0 && taxable != 0;
						if (NeedToCalculateTaxRate)
						{
							tax.TaxRate = taxAmount / taxable;
						}

						if (cancelledOrder == false && refundItems?.Count > 0)
						{
							//If the line item shows in the Refunds field, we have to calculate the Tax manually.
							var lineItemIds = lineItemsWithTax.Select(x => x.Id).Distinct();
							var refundItemsWithTax = refundItems.Where(x => x.RestockType == RestockType.Cancel && lineItemIds.Contains(x.LineItemId)).ToList();
							if (refundItemsWithTax?.Count > 0)
							{
								taxableExcludeRefundItems = taxable - refundItemsWithTax.Sum(x => x.SubTotalPresentment ?? 0m);
								taxAmount = GetHelper<SPHelper>().RoundToStoreSetting((decimal)(taxableExcludeRefundItems * tax.TaxRate), null);
							}
						}
						else if (cancelledOrder == true)
						{
							taxableExcludeRefundItems = 0m;
							taxAmount = 0m;
						}

						TaxDetail inserted = impl.TaxDetails.FirstOrDefault(i => i.TaxID.Value?.Equals(taxName, StringComparison.InvariantCultureIgnoreCase) == true);
						if (inserted == null)
						{
							impl.TaxDetails.Add(new TaxDetail()
							{
								TaxID = taxName.ValueField(),
								TaxAmount = taxAmount.ValueField(),
								TaxRate = (tax.TaxRate * 100).ValueField(),
								TaxableAmount = taxableExcludeRefundItems.ValueField()
							});
						}
						else if (inserted.TaxAmount != null && taxable == taxableExcludeRefundItems)
						{
							inserted.TaxAmount.Value += tax.TaxAmount;
						}
					}
				}
			}

			//Check for tax Ids with more than 30 characters
			String[] tooLongTaxIDs = ((impl.TaxDetails ?? new List<TaxDetail>()).Select(x => x.TaxID?.Value).Where(x => (x?.Length ?? 0) > PX.Objects.TX.Tax.taxID.Length).ToArray());
			if (tooLongTaxIDs != null && tooLongTaxIDs.Length > 0)
			{
				throw new PXException(PX.Commerce.Objects.BCObjectsMessages.CannotFindSaveTaxIDs, string.Join(",", tooLongTaxIDs), PX.Objects.TX.Tax.taxID.Length, bindingExt.TaxSubstitutionListID);
			}

			if (isAutomaticTax)
			{
				impl.FinancialSettings.OverrideTaxZone = presented?.FinancialSettings?.OverrideTaxZone ?? true.ValueField();
				impl.FinancialSettings.CustomerTaxZone = presented?.FinancialSettings?.CustomerTaxZone ?? GetBindingExt<BCBindingExt>()?.DefaultTaxZoneID.ValueField();
			}
			//Calculate tax mode
			impl.TaxCalcMode = data.TaxesIncluded == true ? PX.Objects.TX.TaxCalculationMode.Gross.ValueField() : PX.Objects.TX.TaxCalculationMode.Net.ValueField();
			#endregion

			#region Discounts
			impl.DisableAutomaticDiscountUpdate = true.ValueField();
			impl.DiscountDetails = new List<SalesOrdersDiscountDetails>();
			if (data.DiscountApplications?.Count > 0)
			{
				var tmpDiscountDetails = new List<SalesOrdersDiscountDetails>();
				SalesOrdersDiscountDetails itemDiscountDetail = null;
				var totalItemDiscounts = data.LineItems.SelectMany(x => x.DiscountAllocations).ToList();
				//If there is a shipping discount, it has been applied to the Freight fee calculation above.
				for (int i = 0; i < data.DiscountApplications.Count; i++)
				{
					var discountItem = data.DiscountApplications[i];
					SalesOrdersDiscountDetails detail = new SalesOrdersDiscountDetails();
					detail.Type = PX.Objects.Common.Discount.DiscountType.ExternalDocument.ValueField();
					detail.ExternalDiscountCode = discountItem.Type == DiscountApplicationType.DiscountCode ? (discountItem.Code ?? string.Empty).ValueField() : (discountItem.Title ?? discountItem.Description ?? string.Empty).ValueField();
					detail.Description = (discountItem.Description ?? detail.ExternalDiscountCode.Value).ValueField();
					if (discountItem.TargetType == DiscountTargetType.ShippingLine)
					{
						detail.Description = ShopifyMessages.DiscountAppliedToShippingItem.ValueField();
						detail.DiscountAmount = 0m.ValueField();
						tmpDiscountDetails.Add(detail);
					}
					else
					{
						var matchedDiscounts = totalItemDiscounts.Where(x => x.DiscountApplicationIndex == i);
						if (GetBindingExt<BCBindingExt>()?.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
						{
							detail.DiscountAmount = matchedDiscounts.Sum(x => x.DiscountAmountPresentment ?? 0m).ValueField();
						}
						else
						{
							detail.Description = ShopifyMessages.DiscountAppliedToLineItem.ValueField();
							detail.DiscountAmount = 0m.ValueField();
						}
						//If the refund items have discount, we cannot get the accurate discount amount, we have to combine all discounts to the order level.
						if (cancelledOrder == false && refundItems?.Count > 0 && refundItems.Any(x => x.OrderLineItem.DiscountAllocations?.Count > 0) && GetBindingExt<BCBindingExt>()?.PostDiscounts == BCPostDiscountAttribute.DocumentDiscount)
						{
							itemDiscountDetail = detail;
							itemDiscountDetail.ExternalDiscountCode = ShopifyMessages.RefundDiscount.ValueField();
							itemDiscountDetail.DiscountAmount = totalDiscount.ValueField();
							break;
						}
						else
						{
							tmpDiscountDetails.Add(detail);
						}
					}
				}
				// extra step to make sure all discounts are grouped by discount code before adding them to impl.DiscountDetails
				// as there are cases where SP sends multiple entries for a same discount code in the discount_applications field
				if (tmpDiscountDetails.Count > 0)
				{
					foreach (var discountDetail in tmpDiscountDetails)
					{
						// unlike in BC, in SP discount can be differentiated using Description where the original code will be stored
						// so we will group discounts based on the original codes
						if (!impl.DiscountDetails.Any(x => x.ExternalDiscountCode.Value == discountDetail.ExternalDiscountCode.Value))
							impl.DiscountDetails.Add(discountDetail);
						else
							impl.DiscountDetails.FirstOrDefault(x => x.ExternalDiscountCode.Value == discountDetail.ExternalDiscountCode.Value).DiscountAmount.Value += discountDetail.DiscountAmount.Value;
					}
				}
				if (itemDiscountDetail != null)
				{
					impl.DiscountDetails.Add(itemDiscountDetail);
				}
			}
			#endregion

			#region Payment
			//skip creation of payments if there are refunds as Applied to order amount will be greater than order total
			// Also skip payment creation with order if tax synchronization is false because even if taxes are not sent from connector they may or may not
			// be recalculated by ERP based on taxzone is external or not. Thus resulting in incorrect amount applied to the order(eg if tax is recalculated applied to order amount
			// would be less than order total).
			if (existing == null && GetEntity(BCEntitiesAttribute.Payment)?.IsActive == true && !paymentProcessor.ImportMappings.Select().Any()
				&& data.FinancialStatus != OrderFinancialStatus.PartiallyRefunded && data.FinancialStatus != OrderFinancialStatus.Refunded
				&& bindingExt.TaxSynchronization == true)
			{
				impl.Payments = new List<SalesOrderPayment>();
				var totalOrderAmount = data.CurrentTotalPrice;
				decimal totalAmountAppliedToOrder = 0;
				foreach (MappedPayment payment in bucket.Payments)
				{
					if (!payment.IsNew)
						continue;

					OrderTransaction dataPayment = payment.Extern;
					// skip if payment is Shopify payment because fees can only be handled properly in payment processor
					if (dataPayment.Gateway == ShopifyConstants.ShopifyPayments) continue;

					BCPaymentMethods methodMapping = GetHelper<SPHelper>().GetPaymentMethodMapping(dataPayment.Gateway.ReplaceEmptyString(BCConstants.NoneGateway), dataPayment.Currency, out string cashAcount);
					if (methodMapping.ReleasePayments ?? false) continue; //don't save payment with the order if the require release.

					SalesOrderPayment implPament = new SalesOrderPayment();
					implPament.ExternalID = payment.ExternID;

					//Product
					implPament.DocType = PX.Objects.AR.Messages.Prepayment.ValueField();
					implPament.Currency = impl.CurrencyID;
					var appDate = dataPayment.DateCreatedAt.ToDate(false, PXTimeZoneInfo.FindSystemTimeZoneById(GetBindingExt<BCBindingExt>()?.OrderTimeZone));
					if (appDate.HasValue)
						implPament.ApplicationDate = (new DateTime(appDate.Value.Date.Ticks)).ValueField();
					
					implPament.Hold = false.ValueField();

					if (totalAmountAppliedToOrder >= totalOrderAmount)
						implPament.AppliedToOrder = ((decimal)0).ValueField();
					else
					{
						var paymentAmount = (decimal)dataPayment.Amount;
						var toApply = paymentAmount > (totalOrderAmount - totalAmountAppliedToOrder) ? totalOrderAmount.Value - totalAmountAppliedToOrder : paymentAmount;
						totalAmountAppliedToOrder += toApply;
						implPament.AppliedToOrder = ((decimal)toApply).ValueField();
					}

					implPament.PaymentAmount = ((decimal)dataPayment.Amount).ValueField();

					implPament.PaymentMethod = methodMapping?.PaymentMethodID.ValueField();
					implPament.CashAccount = cashAcount?.Trim()?.ValueField();
					implPament.ExternalRef = dataPayment.Id.ToString().ValueField();
					implPament.PaymentRef = GetHelper<SPHelper>().ParseTransactionNumber(dataPayment, out bool isCreditCardTran).ValueField();

					PX.Objects.AR.ARRegister existingPayment = PXSelect<PX.Objects.AR.ARRegister,
						Where<PX.Objects.AR.ARRegister.externalRef, Equal<Required<PX.Objects.AR.ARRegister.externalRef>>>>.Select(this, implPament.ExternalRef.Value);
					if (existingPayment != null) continue; //skip if payment with same ref nbr exists already.

					TransactionStatus? lastStatus = data.Transactions.LastOrDefault(x => x.ParentId == data.Id && x.Status == TransactionStatus.Success)?.Status ?? dataPayment.Status;
					var paymentDesc = PXMessages.LocalizeFormat(ShopifyMessages.PaymentDescription,
						binding.BindingName,
						bucket.Order?.Extern?.Name,
						GetHelper<SPHelper>().FirstCharToUpper(dataPayment.Kind),
						lastStatus?.ToString(),
						GetHelper<SPHelper>().GetGatewayDescr(dataPayment));
					implPament.Description = paymentDesc.ValueField();

					//Credit Card:
					if (methodMapping?.ProcessingCenterID != null && isCreditCardTran)
					{
						implPament.NewCard = true.ValueField();
						implPament.SaveCard = false.ValueField();
						implPament.ProcessingCenterID = methodMapping?.ProcessingCenterID?.ValueField();

						SalesOrderCreditCardTransactionDetail creditCardDetail = new SalesOrderCreditCardTransactionDetail();
						creditCardDetail.TranNbr = implPament.PaymentRef;
						if (appDate != null && appDate.HasValue)
							creditCardDetail.TranDate = implPament.ApplicationDate;
						//creditCardDetail.ExtProfileId = dataPayment.PaymentInstrumentToken.ValueField();
						creditCardDetail.TranType = GetHelper<SPHelper>().GetTransactionType(dataPayment.LastKind);
						implPament.CreditCardTransactionInfo = new List<SalesOrderCreditCardTransactionDetail>(new[] { creditCardDetail });
					}

					impl.Payments.Add(implPament);
				}
			}
			#endregion

			#region Order Risks
			impl.OrderRisks = new List<OrderRisks>();
			impl.MaxRiskScore = new DecimalValue() { Value = null };
			obj.Local.OrderRisks?.AddRange(presented?.OrderRisks == null ? Enumerable.Empty<OrderRisks>()
				: presented.OrderRisks.Select(n => new OrderRisks() { Id = n.Id, Delete = true }));
			if (bindingExt.ImportOrderRisks == true && data.OrderRisks?.Count > 0)
			{
				foreach (var shopifyRisk in data.OrderRisks.OrderByDescending(x => x.Score))
				{
					var risk = new OrderRisks()
					{
						Message = shopifyRisk.Message.ValueField(),
						Recommendation = shopifyRisk.Recommendation.ToString().ValueField(),
						Score = (shopifyRisk.Score * 100).ValueField(),
					};
					impl.OrderRisks.Add(risk);

				}

				impl.MaxRiskScore = (impl.OrderRisks?.Max(x => x.Score?.Value) ?? 0).ValueField();

			}
			#endregion

			#region Adjust for Existing
			if (presented != null)
			{
				obj.Local.OrderType = presented.OrderType; //Keep the same order Type

				//remap entities if existing
				presented.DiscountDetails?.ForEach(e => obj.Local.DiscountDetails?.FirstOrDefault(n =>
					// if code has less than 15 characters then comparing the code itself is enough
					((n.ExternalDiscountCode?.Value != null && n.ExternalDiscountCode.Value.Length < 15 && n.ExternalDiscountCode.Value == e.ExternalDiscountCode.Value) ||
					// if code has 15 characters or more then also take into account discount's Description as it should contain the original code
					(n.ExternalDiscountCode?.Value != null && n.ExternalDiscountCode.Value.Length >= 15 && n.ExternalDiscountCode.Value.Contains(e.ExternalDiscountCode.Value) &&
					(e.Description.Value == string.Empty || n.Description.Value == e.Description.Value))) &&
					// as a preventive mesure, in case any scenario is missing, also make sure Id is null before mapping it
					n.Id == null).With(n => n.Id = e.Id));

				presented.Payments?.ForEach(e => obj.Local.Payments?.FirstOrDefault(n => n.PaymentRef.Value == e.PaymentRef.Value).With(n => n.Id = e.Id));

				//delete unnecessary entities
				obj.Local.Details?.AddRange(presented.Details == null ? Enumerable.Empty<SalesOrderDetail>()
					: presented.Details.Where(e => obj.Local.Details == null || (!string.IsNullOrEmpty(e.ExternalRef.Value) && !obj.Local.Details.Any(n => e.Id == n.Id))).Select(n => new SalesOrderDetail() { Id = n.Id, Delete = true }));

				obj.Local.DiscountDetails?.AddRange(presented.DiscountDetails == null ? Enumerable.Empty<SalesOrdersDiscountDetails>()
					: presented.DiscountDetails.Where(e => obj.Local.DiscountDetails == null || !obj.Local.DiscountDetails.Any(n => e.Id == n.Id)).Select(n => new SalesOrdersDiscountDetails() { Id = n.Id, Delete = true }));

				obj.Local.Payments?.AddRange(presented.Payments == null ? Enumerable.Empty<SalesOrderPayment>()
					: presented.Payments.Where(e => obj.Local.Payments == null || !obj.Local.Payments.Any(n => e.Id == n.Id)).Select(n => new SalesOrderPayment() { Id = n.Id, Delete = true }));
			}
			#endregion
		}


		/// <summary>
		/// Returns the customerOrder field value with PONumber in case of B2B and with the order name when not B2B
		/// </summary>
		/// <param name="orderData">External Order Entity</param>
		/// <param name="salesOrder">ERP Order Entity</param>
		public virtual StringValue GetCustomerOrderForImport(OrderData orderData, SalesOrder salesOrder)
		{
			if (orderData.Company == null)
				return orderData.Name.ValueField();

			if (string.IsNullOrWhiteSpace(orderData.PONumber))
				return new StringValue() { Value = null };

			return orderData.PONumber.ValueField();
		}

		/// <summary>
		/// When the connector imports SO that has line item with price is less than 0 and quantity is greater than 0, the following logic should be applied:
		/// 1. If order item is not a Non-stock item, or it's a Non-stock item but "Require Shipment" is checked, throw error message
		/// 2. If order total is less than 0, throw error message
		/// 3. Otherwise should convert SO line item with the Quantity to negative value, Ext.Price to Positive value.
		/// </summary>
		protected virtual void HandleNegativePriceItem(OrderData data, OrderLineItem orderItem, bool? stkItem, bool? nonStockShip, ref decimal? unitPrice, ref decimal? quantity, ref decimal? subTotal)
		{
			if (unitPrice < 0 && quantity > 0)
			{
				if (stkItem != false || nonStockShip == true)
					throw new PXException(BCMessages.NegativePriceOrderWithNotMatchItem, data?.Id, orderItem?.Id);
				else if (data?.OrderTotalPresentment < 0)
					throw new PXException(BCMessages.NegativePriceOrder, data?.Id);
				quantity = -quantity;
				unitPrice = -unitPrice;
				if(subTotal > 0)
				subTotal = -subTotal;
			}
		}

		protected bool CompareAddress(Core.API.Address mappedAddress, PX.Objects.CR.Address address)
		{
			return Compare(mappedAddress.City?.Value, address.City)
											&& Compare(mappedAddress.Country?.Value, address.CountryID)
											&& Compare(mappedAddress.State?.Value, address.State)
											&& Compare(mappedAddress.AddressLine1?.Value, address.AddressLine1)
											&& Compare(mappedAddress.AddressLine2?.Value, address.AddressLine2)
											&& Compare(mappedAddress.PostalCode?.Value, address.PostalCode);
		}
		protected bool CompareContact(DocContact mappedContact, PX.Objects.CR.Contact contact, PX.Objects.CR.Location location = null)
		{
			return (Compare(mappedContact.BusinessName?.Value, contact.FullName) || Compare(mappedContact.BusinessName?.Value, location?.Descr))
										&& Compare(mappedContact.Attention?.Value, contact.Attention)
										&& Compare(mappedContact.Email?.Value, contact.EMail)
										&& Compare(mappedContact.Phone1?.Value, contact.Phone1);
		}
		protected bool Compare(string value1, string value2)
		{
			return string.Equals(value1?.Trim() ?? string.Empty, value2?.Trim() ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
		}

		public override async Task SaveBucketImport(SPSalesOrderBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			MappedOrder obj = bucket.Order;
			SalesOrder local = obj.Local;
			SalesOrder presented = existing?.Local as SalesOrder;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();

			// If custom mapped orderType, this will prevent attempt to modify existing SO type and following error
			if (existing != null)
				obj.Local.OrderType = ((MappedOrder)existing).Local.OrderType;

			SalesOrder impl;

			DetailInfo[] oldDetails = obj.Details.ToArray();
			obj.ClearDetails();

			//If we need to cancel the order in Acumatica
			//sort solines by deleted =true first because of api bug  in case if lines are deleted
			obj.Local.Details = obj.Local.Details.OrderByDescending(o => o.Delete).ToList();
			obj.Local.DiscountDetails = obj.Local.DiscountDetails.OrderByDescending(o => o.Delete).ToList();

			if (bindingExt.TaxSynchronization == true)
			{
				obj.AddDetail(BCEntitiesAttribute.TaxSynchronization, null, BCObjectsConstants.BCSyncDetailTaxSynced, true);
			}

			#region Taxes
			GetHelper<SPHelper>().LogTaxDetails(obj.SyncID, obj.Local);
			#endregion

			impl = cbapi.Put<SalesOrder>(obj.Local, obj.LocalID);

			#region Taxes
			GetHelper<SPHelper>().ValidateTaxes(obj.SyncID, impl, obj.Local);
			#endregion

			//If we need to cancel the order in Acumatica
			bool criteriaForCancelOrder = (obj.Extern?.CancelledAt != null || obj.Extern.FinancialStatus == OrderFinancialStatus.Refunded) && (impl.Details?.Count == 0 || impl.Details?.All(x => x.QtyOnShipments.Value == 0m) == true);
			if (criteriaForCancelOrder)
			{
				impl = CancelSalesOrder(bucket, impl);
			}
			else
			{
				impl = UpdateOrderStatus(bucket, impl) ?? impl;
			}

			obj.ExternHash = obj.Extern.CalculateHash();
			obj.AddLocal(impl, impl.SyncID, impl.SyncTime);

			// Save Details
			if (bucket.Location?.Extern != null && bucket.Location?.ExternID != null)
			{
				obj.AddDetail(BCEntitiesAttribute.OrderAddress, impl.SyncID, bucket.Location.ExternID); //Shipment ID detail
			}

			#region Verify and Update Sync Details with Order Details Info

			UpdateSyncDetailsWithOrderDetails(bucket, impl, existing, oldDetails);

			#endregion

			#region Update Order To Shopify

			UpdateExternalOrderWithOrderData(bucket, impl, existing);

			#endregion

			UpdateStatus(obj, operation);

			#region Payments

			UpdateSyncDetailsWtihOrderPayments(bucket, impl, existing, operation);

			#endregion
		}

		public virtual SalesOrder CancelSalesOrder(SPSalesOrderBucket bucket, SalesOrder existingOrder, CancellationToken cancellationToken = default)
		{
			if (existingOrder?.SyncID == null)
				return null;

			return cbapi.Invoke<SalesOrder, CancelSalesOrder>(new SalesOrder() { Id = existingOrder?.Id }, existingOrder.SyncID);
		}

		public virtual SalesOrder UpdateOrderStatus(SPSalesOrderBucket bucket, SalesOrder existingOrder, CancellationToken cancellationToken = default)
		{
			SalesOrder impl = null;
			bool criteriaForUpdateSOStatus = existingOrder.Status.Value != PX.Objects.SO.Messages.Completed && existingOrder.Status.Value != PX.Objects.SO.Messages.Cancelled && existingOrder.Details?.All(x => x.OpenQty.Value == 0m) == true;
			if (criteriaForUpdateSOStatus) //Change order status if no item needs to ship
			{
				//if some items have shipped and no open qty in all line items, change the order status to Completed
				if (existingOrder.Details?.Any(x => x.QtyOnShipments.Value != 0m) == true)
				{
					impl = cbapi.Put<SalesOrder>(new SalesOrder()
					{
						Id = existingOrder.Id,
						Details = existingOrder.Details.Select(d => new SalesOrderDetail() { Id = d.Id, Completed = true.ValueField() }).ToList()
					}, existingOrder.SyncID);
				}
				else if (existingOrder.Details?.All(x => x.OrderQty.Value == 0m) == true)//if no item has shipped and no open qty in all line items, change the order status to Cancelled
				{
					impl = CancelSalesOrder(bucket, existingOrder);
				}
			}

			return impl;
		}

		public virtual void UpdateExternalOrderWithOrderData(SPSalesOrderBucket bucket, SalesOrder order, IMappedEntity existing)
		{
			if (GetBindingExt<BCBindingExt>().SyncOrderNbrToStore == true && bucket.Order.Extern?.Tags?.Contains(order.OrderNbr?.Value, StringComparison.OrdinalIgnoreCase) != true)
			{
				OrderData exportOrderData = new OrderData() { Id = bucket.Order.ExternID.ToLong() };
				if (string.IsNullOrEmpty(bucket.Order.Extern.Tags) || !bucket.Order.Extern.Tags.Contains(BCObjectsConstants.ImportedInAcumatica, StringComparison.OrdinalIgnoreCase))
					exportOrderData.Tags = string.IsNullOrEmpty(bucket.Order.Extern.Tags) ? BCObjectsConstants.ImportedInAcumatica : $"{bucket.Order.Extern.Tags},{BCObjectsConstants.ImportedInAcumatica}";
				else
					exportOrderData.Tags = bucket.Order.Extern.Tags;
				exportOrderData.Tags = string.IsNullOrEmpty(exportOrderData.Tags) ? order.OrderNbr.Value : $"{exportOrderData.Tags},{order.OrderNbr.Value}";
				var existedMetafield = bucket.Order.Extern.Metafields?.FirstOrDefault(x => x.Key == BCObjectsConstants.ImportedInAcumatica);
				if (existedMetafield == null)
				{
					existedMetafield = new MetafieldData()
					{
						Key = BCObjectsConstants.ImportedInAcumatica,
						Value = order.OrderNbr.Value,
						Type = ShopifyConstants.ValueType_SingleString,
						Namespace = BCObjectsConstants.Namespace_Global
					};
					exportOrderData.Metafields = new List<MetafieldData>() { existedMetafield };
				}
				else if (existedMetafield.Value?.Contains(order.OrderNbr.Value, StringComparison.OrdinalIgnoreCase) != true)
				{
					existedMetafield.Value = string.IsNullOrEmpty(existedMetafield.Value) ? order.OrderNbr.Value : $"{existedMetafield.Value},{order.OrderNbr.Value}";
					exportOrderData.Metafields = new List<MetafieldData>() { existedMetafield };
				}
				try
				{
					var returnOrderData = orderDataProvider.Update(exportOrderData, bucket.Order.ExternID);
					bucket.Order.ExternTimeStamp = returnOrderData?.DateModifiedAt.ToDate(false);
				}
				catch (Exception ex)
				{
					LogWarning(Operation.LogScope(), ex.Message);
				}
			}
		}

		public virtual void UpdateSyncDetailsWithOrderDetails(SPSalesOrderBucket bucket, SalesOrder order, IMappedEntity existing, DetailInfo[] existingDetails)
		{
			var refundItems = bucket.Order.Extern != null && bucket.Order.Extern.Refunds?.Count > 0 ? bucket.Order.Extern.Refunds.SelectMany(x => x.RefundLineItems)?.ToList() : null;
			foreach (var orderItem in bucket.Order.Extern.LineItems) //Line ID detail
			{
				SalesOrderDetail detail = null;
				detail = order.Details?.FirstOrDefault(x => x.NoteID.Value == existingDetails.FirstOrDefault(o => o.ExternID == orderItem.Id.ToString())?.LocalID);
				if (detail == null) detail = order.Details?.FirstOrDefault(x => x.ExternalRef?.Value != null && x.ExternalRef?.Value == orderItem.Id.ToString());
				if (detail == null)
				{
					String inventoryCD = GetHelper<SPHelper>().GetInventoryCDByExternID(
						orderItem.ProductId.ToString(),
						orderItem.VariantId.ToString(),
						orderItem.Sku ?? string.Empty,
						orderItem.Name,
						orderItem.IsGiftCard,
						out string uom,
						out string alternateID,
						out string itemStatus);
					detail = order.Details?.FirstOrDefault(x => !bucket.Order.Details.Any(o => x.NoteID.Value == o.LocalID) && x.InventoryID.Value == inventoryCD);
				}
				if (detail != null)
				{
					bucket.Order.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, orderItem.Id.ToString());
					continue;
				}

				if (refundItems != null && refundItems.Any(x => x.LineItemId == orderItem.Id))
				{ //Skip the refunded items
					var refundQuantity = refundItems.Where(x => x.LineItemId == orderItem.Id).Sum(q => q.Quantity) ?? 0;
					if (orderItem.Quantity - refundQuantity == 0)
						continue;
				}

				throw new PXException(BCMessages.CannotMapLines);
			}
		}

		public virtual void UpdateSyncDetailsWtihOrderPayments(SPSalesOrderBucket bucket, SalesOrder order, IMappedEntity existing, string operation)
		{
			if (existing == null && bucket.Order.Local.Payments != null && bucket.Payments != null)
			{
				for (int i = 0; i < bucket.Order.Local.Payments.Count; i++)
				{
					SalesOrderPayment sent = bucket.Order.Local.Payments[i];
					PX.Objects.AR.ARPayment payment = null;
					String docType = (new PX.Objects.AR.ARDocType()).ValueLabelPairs.First(p => p.Label == sent.DocType.Value).Value;
					string extRef = sent.PaymentRef.Value;
					payment = PXSelectJoin<PX.Objects.AR.ARPayment, InnerJoin<SOAdjust, On<SOAdjust.adjgRefNbr, Equal<PX.Objects.AR.ARPayment.refNbr>>>,
						Where<PX.Objects.AR.ARPayment.extRefNbr, Equal<Required<PX.Objects.AR.ARPayment.extRefNbr>>,
						And<PX.Objects.AR.ARPayment.docType, Equal<Required<PX.Objects.AR.ARPayment.docType>>,
						And<SOAdjust.adjdOrderNbr, Equal<Required<SOAdjust.adjdOrderNbr>>
						>>>>.Select(this, extRef, docType, order.OrderNbr.Value);
					if (payment == null) continue;

					MappedPayment objPayment = bucket.Payments.FirstOrDefault(x => x.ExternID == sent.ExternalID);
					if (objPayment == null) continue;

					objPayment.AddLocal(null, payment.NoteID, payment.LastModifiedDateTime);
					UpdateStatus(objPayment, operation);
				}
			}
		}

		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			var bindingExt = GetBindingExt<BCBindingExt>();
			var minDate = minDateTime == null || (minDateTime != null && bindingExt.SyncOrdersFrom != null && minDateTime < bindingExt.SyncOrdersFrom) ? bindingExt.SyncOrdersFrom : minDateTime;

			List<string> orderTypesArray = new List<string> { bindingExt.OrderType };
			if (PXAccess.FeatureInstalled<FeaturesSet.userDefinedOrderTypes>() && bindingExt.OtherSalesOrderTypes != null && bindingExt.OtherSalesOrderTypes?.Count() > 0)
				orderTypesArray.AddRange(bindingExt.OtherSalesOrderTypes.Split(',').Where(i => i != bindingExt.OrderType).ToList());

			foreach (var orderType in orderTypesArray)
			{
				if (String.IsNullOrEmpty(orderType)) continue;

				var res = cbapi.GetAll<SalesOrder>(
						new SalesOrder()
						{
							OrderType = orderType.SearchField(),
							OrderNbr = new StringReturn(),
							Status = new StringReturn(),
							CustomerID = new StringReturn(),
							ExternalRef = new StringReturn(),
							Details = new List<SalesOrderDetail>() { new SalesOrderDetail() {
							ReturnBehavior = ReturnBehavior.OnlySpecified,
							InventoryID = new StringReturn() } }
						},
						minDate, maxDateTime, filters, cancellationToken: cancellationToken);
				if (res != null && res.Any())
				{
					int countNum = 0;
					List<IMappedEntity> mappedList = new List<IMappedEntity>();
					foreach (SalesOrder impl in res)
					{
						MappedOrder obj = new MappedOrder(impl, impl.SyncID, impl.SyncTime);

						mappedList.Add(obj);
						countNum++;
						if (countNum % BatchFetchCount == 0)
						{
							ProcessMappedListForExport(mappedList);
						}
					}
					if (mappedList.Any())
					{
						ProcessMappedListForExport(mappedList);
					}
				}
			}
		}
		public override async Task<EntityStatus> GetBucketForExport(SPSalesOrderBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			SalesOrder impl = cbapi.GetByID<SalesOrder>(syncstatus.LocalID, GetCustomFieldsForExport());
			if (impl == null) return EntityStatus.None;

			MappedOrder obj = bucket.Order = bucket.Order.Set(impl, impl.SyncID, impl.SyncTime);
			EntityStatus status = EnsureStatus(bucket.Order, SyncDirection.Export);

			if (status != EntityStatus.Pending && status != EntityStatus.Synchronized)
				return status;

			if (GetEntity(BCEntitiesAttribute.Shipment)?.IsActive == true && impl.Shipments != null)
			{
				foreach (SalesOrderShipment orderShipmentImpl in impl.Shipments)
				{
					if (orderShipmentImpl.ShippingNoteID?.Value == null) continue;

					BCShipments shipmentImpl = new BCShipments();
					shipmentImpl.ShippingNoteID = orderShipmentImpl.ShippingNoteID;
					shipmentImpl.OrderNoteIds = new List<Guid?>() { syncstatus.LocalID };
					shipmentImpl.ShipmentNumber = orderShipmentImpl.ShipmentNbr;
					shipmentImpl.ShipmentType = orderShipmentImpl.ShipmentType;
					shipmentImpl.Confirmed = (orderShipmentImpl.Status?.Value == BCAPICaptions.Confirmed).ValueField();
					MappedShipment shipmentObj = new MappedShipment(shipmentImpl, shipmentImpl.ShippingNoteID.Value, orderShipmentImpl.LastModifiedDateTime.Value);
					EntityStatus shipmentStatus = EnsureStatus(shipmentObj, SyncDirection.Export);

					if (shipmentStatus == EntityStatus.Pending)
						bucket.Shipments.Add(shipmentObj);
				}
			}

			Core.API.Customer implCust = cbapi.Get<Core.API.Customer>(new Core.API.Customer() { CustomerID = new StringSearch() { Value = impl.CustomerID.Value } });
			if (implCust == null)
				throw new PXException(BCMessages.NoCustomerForOrder, obj.Local.OrderNbr.Value);

			//For individual customer order			
			if (BCCustomerCategoryAttribute.IsIndividual(implCust?.CustomerCategory?.Value))
			{
				BCSyncStatus customerStatus = CustomerStatus.Select(BCEntitiesAttribute.Customer, impl.CustomerID?.Value);
				if (customerStatus == null || string.IsNullOrEmpty(customerStatus.ExternID) || customerStatus.LocalID == null)
				{
					MappedCustomer objCust = new MappedCustomer(implCust, implCust.SyncID, implCust.SyncTime);
					EntityStatus custStatus = EnsureStatus(objCust, SyncDirection.Export);

					if (custStatus == EntityStatus.Pending)
						bucket.Customer = objCust;
				}
			}
			//For Company customer order			
			else if (BCCustomerCategoryAttribute.IsOrganization(implCust?.CustomerCategory?.Value))
			{
				//if there is no Contact added in the order.
				if (impl.ContactID?.Value == null)
					throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyOrderNoContact, obj.Local.OrderNbr.Value));

				//We cannot ensure all contacs/locations/roleAssignments belong to the Company have synced to Shopify in this moment, so we have to run Company sync first.
				MappedCompany objCust = bucket.Company = new MappedCompany(implCust, implCust.SyncID, implCust.SyncTime);
				EntityStatus custStatus = EnsureStatus(objCust, SyncDirection.Export);
			}
			return status;
		}

		public override async Task MapBucketExport(SPSalesOrderBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedOrder obj = bucket.Order;

			if (obj.Local.Details == null || obj.Local.Details.Count == 0) throw new PXException(ShopifyMessages.NoOrderDetails, obj.Local.OrderNbr.Value);
			if (!string.IsNullOrEmpty(obj.Local.ExternalRefundRef?.Value)) throw new PXException(ShopifyMessages.CannotExportOrder, obj.Local.OrderNbr.Value);

			SalesOrder impl = obj.Local;
			OrderData orderData = obj.Extern = new OrderData();
			OrderData existingExternOrder = existing?.Extern != null ? ((OrderData)existing.Extern) : null;

			obj.Extern.Id = APIHelper.ReferenceParse(impl.ExternalRef?.Value, GetBinding().BindingName).ToLong();

			if (existingExternOrder != null)
			{
				orderData.Id = ((OrderData)existing.Extern).Id;
			}

			//Check whether currency is supported in Shopify store.
			if (string.IsNullOrEmpty(impl.CurrencyID.Value) || GetBindingExt<BCBindingExt>().SupportedCurrencies.Contains(impl.CurrencyID.Value) == false)
			{
				throw new PXException(BCMessages.OrderCurrencyNotSupported, impl.CurrencyID.Value, ShopifyConstants.ShopifyName);
			}
			else
				orderData.Currency = impl.CurrencyID.Value;

			orderData.Email = impl.ShipToContact.Email?.Value ?? impl.BillToContact.Email?.Value;
			orderData.Note = impl.Note;
			orderData.InventoryBehaviour = OrderInventoryBehaviour.DecrementObeyingPolicy;
			#region Order Transaction
			MapOrderTransactionForExport(bucket, orderData, existing);
			#endregion


			#region Order customer
			//bucket.Customer may be null or have value if order is from individual customer; but if order is from company customer, bucket.Company must have value
			if (bucket.Company == null)
			{
				MapOrderCustomerForExport(bucket, orderData, existing);
			}
			else
			{
				MapOrderCompanyForExport(bucket, orderData, existing);
			}
			#endregion

			#region Order Totals & Shipping fees
			MapOrderShippingLinesForExport(bucket, orderData, existing);
			#endregion

			#region Order BillTo
			MapOrderBillToForExport(bucket, orderData, existing);
			#endregion

			#region Order ShipTo
			MapOrderShipToForExport(bucket, orderData, existing);
			#endregion

			#region Order Details
			MapOrderLinesForExport(bucket, orderData, existing);
			#endregion

			#region Order Tax
			MapOrderTaxesForExport(bucket, orderData, existing);
			#endregion

			#region Order Discount
			MapOrderDiscountsForExport(bucket, orderData, existing);
			#endregion

			#region Order Tags
			MapOrderTagsForExport(bucket, orderData, existing);
			#endregion

			#region Order Metafield data
			MapOrderMetafieldsForExport(bucket, orderData, existing);
			#endregion
		}

		#region Export Mapping

		#region Order Transaction
		protected virtual void MapOrderTransactionForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			//We don't export Payment transaction to Shopify, all export orders should be paid in ERP
			orderData.FinancialStatus = OrderFinancialStatus.Paid;
			//In the new restful Order API, we need to export the Transaction as well
			orderData.Transactions = new List<OrderTransaction>() { new OrderTransaction() {
				Kind = TransactionType.Sale,
				Status = TransactionStatus.Success,
				Amount = impl.OrderTotal.Value
			} };
		}
		#endregion

		#region Order customer
		protected virtual void MapOrderCustomerForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			BCSyncStatus customerStatus = CustomerStatus.Select(BCEntitiesAttribute.Customer, impl.CustomerID?.Value);

			if (customerStatus == null || string.IsNullOrEmpty(customerStatus.ExternID))
			{
				throw new PXException(BCMessages.CustomerNotSyncronized, impl.CustomerID?.Value);
			}
			else
			{
				orderData.Customer = new CustomerData() { Id = customerStatus.ExternID.ToLong() };
				orderData.Company = null;
			}
		}
		#endregion

		#region Order Company
		protected virtual void MapOrderCompanyForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			if (string.IsNullOrEmpty(impl.ContactID?.Value))
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyOrderNoContact, impl.OrderNbr.Value));
			}

			//int? BAccountID = null;
			int? contactID = null, locationID = null;
			string companyExternID = null, customerExternID = null, contactExternID = null, locationExternID = null, contactDisplayName = null;
			List<BCRoleAssignment> roleAssignmentsList = new List<BCRoleAssignment>();

			foreach (var syncResult in PXSelectJoin<BCSyncStatus,
				LeftJoin<BCSyncDetail, On<BCSyncDetail.syncID, Equal<BCSyncStatus.syncID>>,
				//LeftJoin<PX.Objects.CR.BAccount, On<PX.Objects.CR.BAccount.noteID, Equal<BCSyncStatus.localID>>,
				LeftJoin<PX.Objects.CR.Contact, On<BCSyncDetail.entityType, Equal<BCEntitiesAttribute.companyContact>, And<PX.Objects.CR.Contact.noteID, Equal<BCSyncDetail.localID>>>,
				LeftJoin<PX.Objects.CR.Location, On<BCSyncDetail.entityType, Equal<BCEntitiesAttribute.companyLocation>, And<PX.Objects.CR.Location.noteID, Equal<BCSyncDetail.localID>>>,
				LeftJoin<BCRoleAssignment, On<BCSyncDetail.entityType, Equal<BCEntitiesAttribute.companyRoleAssignment>, And<BCRoleAssignment.noteID, Equal<BCSyncDetail.localID>>>>>>>,
				Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
						And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
						And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>,
						And<BCSyncStatus.localID, Equal<Required<BCSyncStatus.localID>>>>>>>.
						Select(this, BCEntitiesAttribute.Company, bucket.Company.LocalID))
			{
				companyExternID = syncResult.GetItem<BCSyncStatus>()?.ExternID;
				var companySyncDetail = syncResult.GetItem<BCSyncDetail>();
				var companyContact = syncResult.GetItem<PX.Objects.CR.Contact>();
				var companyLocation = syncResult.GetItem<PX.Objects.CR.Location>();
				var companyRoleAssignment = syncResult.GetItem<BCRoleAssignment>();

				if (companySyncDetail.EntityType == BCEntitiesAttribute.CompanyContact && string.Equals(companyContact.ContactID?.ToString(), impl.ContactID?.Value))
				{
					contactID = companyContact.ContactID;
					contactDisplayName = companyContact.DisplayName?.Trim();
					orderData.Email = companyContact.EMail;
					contactExternID = companySyncDetail.ExternID.KeySplit(1, companySyncDetail.ExternID);
					customerExternID = companySyncDetail.ExternID.KeySplit(0);
				}

				if (companySyncDetail.EntityType == BCEntitiesAttribute.CompanyLocation && string.Equals(companyLocation.LocationCD.Trim(), impl.LocationID?.Value))
				{
					locationID = companyLocation.LocationID;
					locationExternID = companySyncDetail.ExternID;
				}

				if (companySyncDetail.EntityType == BCEntitiesAttribute.CompanyRoleAssignment && companyRoleAssignment != null)
				{
					roleAssignmentsList.Add(companyRoleAssignment);
				}

			}

			if (string.IsNullOrEmpty(companyExternID) || string.IsNullOrEmpty(contactExternID) || string.IsNullOrEmpty(locationExternID) || string.IsNullOrEmpty(customerExternID))
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyNotSyncronized, impl.CustomerID?.Value));
			}

			if (roleAssignmentsList.Any(x => x.ContactID == contactID && x.LocationID == locationID && !string.IsNullOrEmpty(x.Role)) != true)
			{
				throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.CompanyOrderNoRoleAssignment, contactDisplayName ?? impl.ContactID?.Value, impl.LocationID?.Value, impl.CustomerID?.Value));
			}

			orderData.Customer = new CustomerData() { Id = customerExternID.ToLong() };
			orderData.Company = new OrderCompany() { Id = companyExternID.ToLong(), LocationId = locationExternID.ToLong() };

			if (impl.CustomerOrder == null || string.IsNullOrWhiteSpace(impl.CustomerOrder?.Value))
				orderData.PONumber = bucket.Order.Extern.Id.HasValue ? "" : null;
			else
				orderData.PONumber = impl.CustomerOrder.Value;
		}
		#endregion

		#region Order ShipTo
		protected virtual void MapOrderShipToForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			string attention = !string.IsNullOrWhiteSpace(impl.BillToContact.Attention?.Value) ? impl.BillToContact.Attention?.Value : impl.BillToContact.BusinessName?.Value;
			string firstName = attention.FieldsSplit(0, attention);
			string lastName = attention.FieldsSplit(1, attention);

			orderData.ShippingAddress = new OrderAddressData();
			orderData.ShippingAddress.Address1 = impl.ShipToAddress.AddressLine1?.Value;
			orderData.ShippingAddress.Address2 = impl.ShipToAddress.AddressLine2?.Value;
			orderData.ShippingAddress.City = impl.ShipToAddress.City?.Value;
			orderData.ShippingAddress.PostalCode = impl.ShipToAddress.PostalCode?.Value;
			orderData.ShippingAddress.CountryCode = impl.ShipToAddress.Country?.Value;
			orderData.ShippingAddress.ProvinceCode = impl.ShipToAddress.State?.Value;
			orderData.ShippingAddress.Company = impl.ShipToContact.BusinessName?.Value;
			orderData.ShippingAddress.Phone = impl.ShipToContact.Phone1?.Value;
			orderData.ShippingAddress.FirstName = string.Equals(firstName, lastName) ? null : firstName;
			orderData.ShippingAddress.LastName = lastName;
		}
		#endregion

		#region Order BillTo
		protected virtual void MapOrderBillToForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			string attention = !string.IsNullOrWhiteSpace(impl.BillToContact.Attention?.Value) ? impl.BillToContact.Attention?.Value : impl.BillToContact.BusinessName?.Value;
			string firstName = attention.FieldsSplit(0, attention);
			string lastName = attention.FieldsSplit(1, attention);

			orderData.BillingAddress = new OrderAddressData();
			orderData.BillingAddress.Address1 = impl.BillToAddress.AddressLine1?.Value;
			orderData.BillingAddress.Address2 = impl.BillToAddress.AddressLine2?.Value;
			orderData.BillingAddress.City = impl.BillToAddress.City?.Value;
			orderData.BillingAddress.PostalCode = impl.BillToAddress.PostalCode?.Value;
			orderData.BillingAddress.CountryCode = impl.BillToAddress.Country?.Value;
			orderData.BillingAddress.ProvinceCode = impl.BillToAddress.State?.Value;
			orderData.BillingAddress.Company = impl.BillToContact.BusinessName?.Value;
			orderData.BillingAddress.Phone = impl.BillToContact.Phone1?.Value;
			orderData.BillingAddress.FirstName = string.Equals(firstName, lastName) ? null : firstName;
			orderData.BillingAddress.LastName = lastName;
		}
		#endregion

		#region Order Total and Shipping lines
		protected virtual void MapOrderShippingLinesForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			if (impl.Totals != null)
			{
				orderData.TotalWeightInGrams = impl.Totals.OrderWeight.Value;
				if (impl.Totals.Freight.Value > 0 || impl.Totals.PremiumFreight.Value > 0)
				{
					if (string.IsNullOrEmpty(impl.ShipVia.Value))
					{
						throw new PXException(BCMessages.OrderNotExportWithoutShipVia, impl.OrderNbr.Value);
					}
					var mappingValue = GetHelper<SPHelper>().ShippingMethods().FirstOrDefault(m => m.Active == true &&
						string.Equals(m.CarrierID, impl.ShipVia.Value, StringComparison.OrdinalIgnoreCase));
					if (mappingValue == null)
					{
						throw new PXException(BCMessages.OrderShippingMappingIsMissingForExport, impl.OrderNbr.Value);
					}
					orderData.ShippingLines = new List<OrderShippingLine>()
					{
						new OrderShippingLine()
						{
							Code = mappingValue != null? mappingValue.ShippingMethod ?? impl.ShipVia.Value : impl.ShipVia.Value,
							ShippingCostExcludingTax = (impl.Totals.Freight.Value?? 0) + (impl.Totals.PremiumFreight.Value ?? 0),
							SourceName = BCObjectsConstants.ImportedInAcumatica,
							Title = mappingValue != null? mappingValue.ShippingMethod ?? impl.ShipVia.Value : impl.ShipVia.Value
						}
					};
				}
			}
		}
		#endregion

		#region Order Line items
		protected virtual void MapOrderLinesForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			orderData.LineItems = new List<OrderLineItem>();

			foreach (SalesOrderDetail detail in impl.Details)
			{
				OrderLineItem product = new OrderLineItem();

				String key = GetHelper<SPHelper>().GetProductExternIDByProductCD(detail.InventoryID?.Value.Trim(), out string uom);
				if (key == null) throw new PXException(BCMessages.InvenotryNotSyncronized, detail.InventoryID?.Value);
				if (uom != detail?.UOM?.Value) throw new PXException(BCMessages.NotSalesUOMUsed, detail.InventoryID?.Value);
				if ((detail.OrderQty.Value ?? 0) % 1 != 0) throw new PXException(BCMessages.NotDecimalQtyUsed, detail.InventoryID?.Value);

				product.Id = bucket.Order.Details.Where(x => x.EntityType == BCEntitiesAttribute.OrderLine && x.LocalID == detail.NoteID.Value).FirstOrDefault()?.ExternID.ToLong();

				if (!String.IsNullOrEmpty(key.KeySplit(1, String.Empty)))
					product.VariantId = key.KeySplit(1, String.Empty).ToLong();
				product.Quantity = (int)detail.OrderQty.Value;
				//Discount in line level cannot be supported in current Shopify API, so we need to re-calculate the unit price.
				product.Price = product.Price = (detail.OrderQty.Value ?? 0) == 0 ? (detail.Amount.Value ?? 0) : (detail.Amount.Value ?? 0) / detail.OrderQty.Value;

				bool.TryParse(GetHelper<SPHelper>().GetSubstituteExternByLocal(GetBindingExt<BCBindingExt>().TaxCategorySubstitutionListID, detail.TaxCategory?.Value, String.Empty), out bool isTaxable);
				product.Taxable = isTaxable;

				orderData.LineItems.Add(product);
			}
		}
		#endregion

		#region Order Taxes
		protected virtual void MapOrderTaxesForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			//The connector will check the Tax Synchronization settings in the Taxes section of the Shopify Stores (BC201010) screen.
			//If the check box is unchecked, we will not be exporting taxes with the sales order since when the order is imported back to ERP, the recalculation will occur and taxes will be over-ridden.
			//If the check box is checked, the connector shall export the tax ID associated with the order and create the taxes
			SalesOrder impl = bucket.Order.Local;
			if (GetBindingExt<BCBindingExt>().TaxSynchronization == true && impl.TaxDetails != null && impl.TaxDetails.Any())
			{
				orderData.TaxLines = new List<OrderTaxLine>();
				foreach (var taxLine in impl.TaxDetails)
				{
					OrderTaxLine newTaxLine = new OrderTaxLine();
					newTaxLine.TaxName = GetHelper<SPHelper>().GetSubstituteExternByLocal(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, taxLine.TaxID.Value, taxLine.TaxID.Value);
					newTaxLine.TaxRate = taxLine.TaxRate.Value / 100;
					newTaxLine.TaxAmount = taxLine.TaxAmount.Value;
					orderData.TaxLines.Add(newTaxLine);
				}
				orderData.TotalTax = impl.TaxTotal.Value;
			}
		}
		#endregion

		#region Order Discount
		protected virtual void MapOrderDiscountsForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			if (impl.DiscountDetails != null && impl.DiscountDetails.Any())
			{
				orderData.DiscountCodes = new List<OrderDiscountCodes>();
				foreach (var discountDetail in impl.DiscountDetails)
				{
					bool isAmountDiscount = discountDetail.DiscountAmount.Value > 0 ? true : false;
					OrderDiscountCodes orderDiscount = new OrderDiscountCodes();
					//If no external discount code will use discount code instead, if no code in the discount, will use the default discount code "External Discount".
					orderDiscount.Code = string.IsNullOrEmpty(discountDetail.ExternalDiscountCode.Value) ? (discountDetail.DiscountCode.Value ?? ShopifyConstants.ExternalDiscount) : discountDetail.ExternalDiscountCode.Value;
					orderDiscount.Amount = isAmountDiscount ? discountDetail.DiscountAmount.Value : discountDetail.DiscountPercent.Value;
					orderDiscount.Type = isAmountDiscount ? DiscountType.FixedAmount : DiscountType.Percentage;

					orderData.DiscountCodes.Add(orderDiscount);
				}
			}
		}
		#endregion

		#region Order Tags
		protected virtual void MapOrderTagsForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			OrderData existingExternOrder = existing?.Extern != null ? ((OrderData)existing.Extern) : null;

			if (GetBindingExt<BCBindingExt>().SyncOrderNbrToStore == true && (existingExternOrder == null || existingExternOrder?.Tags?.Contains(impl.OrderNbr?.Value, StringComparison.OrdinalIgnoreCase) != true))
			{
				if (string.IsNullOrEmpty(existingExternOrder?.Tags) || !existingExternOrder.Tags.Contains(BCObjectsConstants.ImportedInAcumatica, StringComparison.OrdinalIgnoreCase))
					orderData.Tags = string.IsNullOrEmpty(existingExternOrder?.Tags) ? BCObjectsConstants.ImportedInAcumatica : $"{existingExternOrder?.Tags},{BCObjectsConstants.ImportedInAcumatica}";
				else
					orderData.Tags = existingExternOrder.Tags;

				orderData.Tags = string.IsNullOrEmpty(orderData.Tags) ? impl.OrderNbr.Value : $"{orderData.Tags},{impl.OrderNbr.Value}";
				var existedMetafield = existingExternOrder?.Metafields?.FirstOrDefault(x => x.Key == BCObjectsConstants.ImportedInAcumatica);

				if (orderData.Metafields == null)
					orderData.Metafields = new List<MetafieldData>();

				if (existedMetafield == null)
				{
					existedMetafield = new MetafieldData()
					{
						Key = BCObjectsConstants.ImportedInAcumatica,
						Value = impl.OrderNbr.Value,
						Type = ShopifyConstants.ValueType_SingleString,
						Namespace = BCObjectsConstants.Namespace_Global
					};
					orderData.Metafields.Add(existedMetafield);
				}
				else if (existedMetafield.Value?.Contains(impl.OrderNbr.Value, StringComparison.OrdinalIgnoreCase) != true)
				{
					existedMetafield.Value = string.IsNullOrEmpty(existedMetafield.Value) ? impl.OrderNbr.Value : $"{existedMetafield.Value},{impl.OrderNbr.Value}";
					orderData.Metafields.Add(existedMetafield);
				}
			}
		}
		#endregion

		#region Order Metafield Data
		protected virtual void MapOrderMetafieldsForExport(SPSalesOrderBucket bucket, OrderData orderData, IMappedEntity existing)
		{
			SalesOrder impl = bucket.Order.Local;
			OrderData existingExternOrder = existing?.Extern != null ? ((OrderData)existing.Extern) : null;

			if (existingExternOrder == null && orderData.Id == null)
			{
				MetafieldData meta = new MetafieldData()
				{
					Key = ShopifyConstants.Order,
					Value = impl.Id.Value.ToString(),
					Type = ShopifyConstants.ValueType_SingleString,
					Namespace = BCObjectsConstants.Namespace_Global
				};
				if (orderData.Metafields == null)
					orderData.Metafields = new List<MetafieldData>() { meta };
				else
					orderData.Metafields.Add(meta);
			}
		}
		#endregion

		#endregion

		public override async Task SaveBucketExport(SPSalesOrderBucket bucket, IMappedEntity existing, string operation, CancellationToken cancellationToken = default)
		{
			List<PXDataFieldParam> fieldParams = new List<PXDataFieldParam>();

			MappedOrder obj = bucket.Order;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			OrderData existingExternOrder = existing?.Extern != null ? ((OrderData)existing.Extern) : null;
			OrderData data = null;

			if (obj.Extern.Id == null)
			{
				data = orderDataProvider.Create(obj.Extern);

				#region Update ExternalRef

				string externalRef = APIHelper.ReferenceMake(data.Id?.ToString(), GetBinding().BindingName);
				if (externalRef.Length < 40 && obj.Local.SyncID != null)
				{
					fieldParams.Add(new PXDataFieldAssign(typeof(PX.Objects.SO.SOOrder.customerRefNbr).Name, PXDbType.NVarChar, externalRef));
				}

				if (obj.Extern.Company == null)
					fieldParams.Add(new PXDataFieldAssign(typeof(PX.Objects.SO.SOOrder.customerOrderNbr).Name, PXDbType.NVarChar, data.Name));

				//  Reset ExternalOrderOriginal flag to true
				fieldParams.Add(new PXDataFieldAssign(typeof(BCSOOrderExt.externalOrderOriginal).Name, PXDbType.Bit, true));

				if (fieldParams.Count > 0)
				{
					fieldParams.Add(new PXDataFieldRestrict(typeof(PX.Objects.SO.SOOrder.noteID).Name, PXDbType.UniqueIdentifier, obj.Local.SyncID.Value));
					PXDatabase.Update<PX.Objects.SO.SOOrder>(fieldParams.ToArray());
				}

				#endregion
			}
			else
			{
				//If order is archived in Shopify but it's not Completed Status in ERP, we should re-open order first.
				if (existingExternOrder != null && existingExternOrder?.ClosedAt != null && obj.Local.Status.Value != PX.Objects.SO.Messages.Completed)
				{
					data = orderDataProvider.ReopenOrder(obj.Extern.Id.ToString());
				}

				if (existingExternOrder != null && existingExternOrder?.CancelledAt == null && obj.Local.Status.Value == PX.Objects.SO.Messages.Cancelled &&
						existingExternOrder?.FulfillmentStatus != OrderFulfillmentStatus.Fulfilled && existingExternOrder?.FulfillmentStatus != OrderFulfillmentStatus.Partial)
				{
					data = orderDataProvider.CancelOrder(obj.Extern.Id.ToString());
				}
				else
					data = orderDataProvider.Update(obj.Extern, obj.Extern.Id.ToString());

				//if order in ERP is completed, and order is fullfilled but still opened in Shopify, we should close the order in Shopify after data updated.
				//if user re-open the completed order and add notes to order and we need to sync the changes to Shopify.
				//If user marked the order to Completed in AC later, we should manual close Shopify as well because there is not Shipment in this case.
				if (obj.Local.Status.Value == PX.Objects.SO.Messages.Completed && data.ClosedAt == null && data.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)
				{
					data = orderDataProvider.CloseOrder(data.Id.ToString());
				}
			}

			DetailInfo[] oldDetails = obj.Details.ToArray();
			obj.ClearDetails();

			if (bindingExt.TaxSynchronization == true)
			{
				obj.AddDetail(BCEntitiesAttribute.TaxSynchronization, null, BCObjectsConstants.BCSyncDetailTaxSynced, true);
			}

			foreach (var detail in obj.Local.Details) //Line ID detail
			{
				var product = data.LineItems.FirstOrDefault(x => x.Id.ToString() == oldDetails.FirstOrDefault(o => o.LocalID == detail.NoteID.Value)?.ExternID);
				if (product == null)
				{
					String externalID = GetHelper<SPHelper>().GetProductExternIDByProductCD(detail.InventoryID.Value, out string uom);
					product = data.LineItems.FirstOrDefault(x => !obj.Details.Any(o => x.Id.ToString() == o.ExternID && o.EntityType == BCEntitiesAttribute.OrderLine)
								&& x.VariantId.ToString() == externalID.KeySplit(1, String.Empty));
				}

				//Because Shopify doesn't allow adding/deleting/updating line items after order created through API, it may cause some line details in AC may not show in Shopify order, we should skip them only.
				if (product != null)
				{
					//Update externalRef for order detail
					fieldParams.Clear();
					fieldParams.Add(new PXDataFieldAssign(typeof(BCSOLineExt.externalRef).Name, PXDbType.NVarChar, product.Id.ToString()));
					fieldParams.Add(new PXDataFieldRestrict(typeof(PX.Objects.SO.SOLine.noteID).Name, PXDbType.UniqueIdentifier, detail.NoteID.Value));
					PXDatabase.Update<PX.Objects.SO.SOLine>(fieldParams.ToArray());

					obj.AddDetail(BCEntitiesAttribute.OrderLine, detail.NoteID.Value, product.Id.ToString());
				}
			}

			obj.AddExtern(data, data.Id.ToString(), data.Name, data.DateModifiedAt.ToDate(false));
			UpdateStatus(obj, operation);

		}
		#endregion

		#region Methods
		public override object GetSourceObjectImport(object currentTargetObject, IExternEntity data, BCEntityImportMapping mapping)
		{
			OrderData salesOrder = data as OrderData;
			if (mapping.SourceObject.Contains(BCConstants.LineItems) && mapping.TargetObject.Contains(BCConstants.Details))
			{
				var soline = (SalesOrderDetail)currentTargetObject;
				if (soline?.ExternalRef != null)
					return salesOrder?.LineItems?.FirstOrDefault(x => x.Id.ToString() == soline.ExternalRef.Value);
			}
			return base.GetSourceObjectImport(currentTargetObject, data, mapping);
		}

		#endregion
	}


}
