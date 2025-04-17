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
using System.Linq;
using PX.Commerce.Shopify.API.REST;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.CR.CRMarketingListMaint_Extensions;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Api.ContractBased.Models;
using PX.Objects.SO;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public class SPShipmentEntityBucket : EntityBucketBase, IEntityBucket
	{
		public IMappedEntity Primary => Shipment;
		public IMappedEntity[] Entities => new IMappedEntity[] { Primary }.Concat(Orders).ToArray();

		public MappedShipment Shipment;
		public List<MappedOrder> Orders = new List<MappedOrder>();
	}

	public class SPShipmentsRestrictor : BCBaseRestrictor, IRestrictor
	{
		public virtual FilterResult RestrictExport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			#region Shipments
			return base.Restrict<MappedShipment>(mapped, delegate (MappedShipment obj)
			{
				if (obj.Local != null)
				{
					if (mode != FilterMode.Merge
						&& obj.Local.Confirmed?.Value == false)
					{
						return new FilterResult(FilterStatus.Invalid,
								PXMessages.Localize(BCMessages.LogShipmentSkippedNotConfirmed));
					}

					if (obj.Local.OrderNoteIds != null)
					{
						BCBindingExt binding = processor.GetBindingExt<BCBindingExt>();

						Boolean anyFound = false;
						foreach (var orderNoeId in obj.Local?.OrderNoteIds)
						{
							if (processor.SelectStatus(BCEntitiesAttribute.Order, orderNoeId) == null) continue;

							anyFound = true;
						}
						if (!anyFound)
						{
							return new FilterResult(FilterStatus.Ignore,
								PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.LogShipmentSkippedNoOrder, obj.Local.ShipmentNumber?.Value ?? obj.Local.SyncID.ToString()));
						}
					}
				}

				return null;
			});
			#endregion
		}

		public virtual FilterResult RestrictImport(IProcessor processor, IMappedEntity mapped, FilterMode mode)
		{
			return null;
		}
	}

	[BCProcessor(typeof(SPConnector), BCEntitiesAttribute.Shipment, BCCaptions.Shipment, 100,
		IsInternal = false,
		Direction = SyncDirection.Export,
		PrimaryDirection = SyncDirection.Export,
		PrimarySystem = PrimarySystem.Local,
		ExternTypes = new Type[] { typeof(ShipmentData) },
		LocalTypes = new Type[] { typeof(BCShipments) },
		GIScreenID = BCConstants.GenericInquiryShipmentDetails,
		GIResult = typeof(BCShipmentsResult),
		AcumaticaPrimaryType = typeof(PX.Objects.SO.SOShipment),
		AcumaticaPrimarySelect = typeof(PX.Objects.SO.SOShipment.shipmentNbr),
		URL = "orders/{0}",
		Requires = new string[] { BCEntitiesAttribute.Order }
	)]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ShipmentLine, EntityName = BCCaptions.ShipmentLine, AcumaticaType = typeof(PX.Objects.SO.SOShipLine))]
	[BCProcessorDetail(EntityType = BCEntitiesAttribute.ShipmentBoxLine, EntityName = BCCaptions.ShipmentLineBox, AcumaticaType = typeof(PX.Objects.SO.SOPackageDetailEx))]
	[BCProcessorRealtime(PushSupported = true, HookSupported = false,
		PushSources = new String[] { "BC-PUSH-Shipments" }, PushDestination = BCConstants.PushNotificationDestination)]
	public class SPShipmentProcessor : ShipmentProcessorBase<SPShipmentProcessor, SPShipmentEntityBucket, MappedShipment>
	{
		protected IOrderRestDataProvider orderDataProvider;
		protected IFulfillmentRestDataProvider fulfillmentDataProvider;
		protected IFulfillmentOrderRestDataProvider fulfillmentOrderDataProvider;
		protected IEnumerable<InventoryLocationData> inventoryLocations;
		protected List<BCShippingMappings> shippingMappings;
		protected BCBinding currentBinding;
		protected BCBindingExt currentBindingExt;
		protected BCBindingShopify currentShopifySettings;
		private long? defaultLocationId;

		#region Factories
		[InjectDependency]
		protected ISPRestDataProviderFactory<IOrderRestDataProvider> orderDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IFulfillmentRestDataProvider> fulfillmentDataProviderFactory { get; set; }
		[InjectDependency]
		protected ISPRestDataProviderFactory<IFulfillmentOrderRestDataProvider> fulfillmentOrderDataProviderFactory { get; set; }
		#endregion

		#region Constructor
		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);
			currentBinding = GetBinding();
			currentBindingExt = GetBindingExt<BCBindingExt>();
			currentShopifySettings = GetBindingExt<BCBindingShopify>();

			var client = SPConnector.GetRestClient(GetBindingExt<BCBindingShopify>());

			orderDataProvider = orderDataProviderFactory.CreateInstance(client);
			fulfillmentDataProvider = fulfillmentDataProviderFactory.CreateInstance(client);
			fulfillmentOrderDataProvider = fulfillmentOrderDataProviderFactory.CreateInstance(client);

			shippingMappings = PXSelectReadonly<BCShippingMappings,
				Where<BCShippingMappings.bindingID, Equal<Required<BCShippingMappings.bindingID>>>>
				.Select(this, Operation.Binding).Select(x => x.GetItem<BCShippingMappings>()).ToList();
			inventoryLocations = ConnectorHelper.GetConnector(currentBinding.ConnectorType)?.GetExternalInfo<InventoryLocationData>(BCObjectsConstants.BCInventoryLocation, currentBinding.BindingID)?.Where(x => x.Active == true);
			if (inventoryLocations == null || inventoryLocations.Count() == 0)
			{
				throw new PXException(ShopifyMessages.InventoryLocationNotFound);
			}
			else
				defaultLocationId = inventoryLocations.First().Id;
		}
		#endregion

		public override void NavigateLocal(IConnector connector, ISyncStatus status, ISyncDetail detail = null)
		{
			SOOrderShipment orderShipment = PXSelect<SOOrderShipment, Where<SOOrderShipment.shippingRefNoteID, Equal<Required<SOOrderShipment.shippingRefNoteID>>>>.Select(this, status?.LocalID);
			if (orderShipment.ShipmentType == SOShipmentType.DropShip)//dropshipment
			{
				POReceiptEntry extGraph = PXGraph.CreateInstance<POReceiptEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}
			if (orderShipment.ShipmentType == SOShipmentType.Issue && orderShipment.ShipmentNoteID == null) //Invoice
			{
				ARInvoiceEntry extGraph = PXGraph.CreateInstance<ARInvoiceEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}
			else//shipment
			{
				SOShipmentEntry extGraph = PXGraph.CreateInstance<SOShipmentEntry>();
				EntityHelper helper = new EntityHelper(extGraph);
				helper.NavigateToRow(extGraph.GetPrimaryCache().GetItemType().FullName, status.LocalID, PXRedirectHelper.WindowMode.NewWindow);

			}

		}
		public override bool ControlModification(IMappedEntity mapped, BCSyncStatus status, string operation, CancellationToken cancellationToken = default)
		{
			if (mapped is MappedShipment)
			{
				MappedShipment obj = mapped as MappedShipment;
				if (!obj.IsNew && obj.Local != null && obj.Local?.ExternalShipmentUpdated?.Value == true) // Update status only if order is unmodified
				{
					return false;
				}
			}
			return base.ControlModification(mapped, status, operation);
		}

		#region Pull
		public override async Task<MappedShipment> PullEntity(Guid? localID, Dictionary<string, object> externalInfo, CancellationToken cancellationToken = default)
		{
			BCShipments giResult = (new BCShipments()
			{
				ShippingNoteID = localID.ValueField()
			});
			giResult.Results = cbapi.GetGIResult<BCShipmentsResult>(giResult, BCConstants.GenericInquiryShipmentDetails, cancellationToken: cancellationToken).ToList();

			if (giResult?.Results == null || giResult.Results.Count == 0) return null;
			MapFilterFields(giResult?.Results, giResult);
			GetOrderShipment(giResult);
			if (giResult.Shipment == null && giResult.POReceipt == null) return null;
			MappedShipment obj = new MappedShipment(giResult, giResult.ShippingNoteID.Value, giResult.LastModified?.Value);
			return obj;


		}
		public override async Task<MappedShipment> PullEntity(String externID, String externalInfo, CancellationToken cancellationToken = default)
		{
			FulfillmentData data = fulfillmentDataProvider.GetByID(externID.KeySplit(0), externID.KeySplit(1));
			if (data == null) return null;

			MappedShipment obj = new MappedShipment(new ShipmentData() { FulfillmentDataList = new List<FulfillmentData>() { data } }, new Object[] { data.OrderId, data.Id }.KeyCombine(), null, data.DateModifiedAt.ToDate(false), data.CalculateHash());

			return obj;
		}
		#endregion

		#region Import
		public override async Task FetchBucketsForImport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
		}
		public override async Task<EntityStatus> GetBucketForImport(SPShipmentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			bucket.Shipment = bucket.Shipment.Set(new ShipmentData(), syncstatus.ExternID, String.Empty, syncstatus.ExternTS);

			return EntityStatus.None;
		}

		public override async Task MapBucketImport(SPShipmentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
		}
		public override async Task SaveBucketImport(SPShipmentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
		}
		#endregion

		#region Export
		public override async Task FetchBucketsForExport(DateTime? minDateTime, DateTime? maxDateTime, PXFilterRow[] filters, CancellationToken cancellationToken = default)
		{
			var giResult = cbapi.GetGIResult<BCShipmentsResult>(new BCShipments()
			{
				BindingID = currentBinding.BindingID.ValueField(),
				LastModified = minDateTime?.ValueField()
			}, BCConstants.GenericInquiryShipment, cancellationToken: cancellationToken);

			foreach (var result in giResult)
			{
				if (result.NoteID?.Value == null)
					continue;

				BCShipments bCShipments = new BCShipments() { ShippingNoteID = result.NoteID, LastModified = result.LastModifiedDateTime, ExternalShipmentUpdated = result.ExternalShipmentUpdated };
				MappedShipment obj = new MappedShipment(bCShipments, bCShipments.ShippingNoteID.Value, bCShipments.LastModified.Value);
				EntityStatus status = EnsureStatus(obj, SyncDirection.Export);
			}
		}
		public override async Task<EntityStatus> GetBucketForExport(SPShipmentEntityBucket bucket, BCSyncStatus syncstatus, CancellationToken cancellationToken = default)
		{
			SOOrderShipments impl = new SOOrderShipments();
			BCShipments giResult = new BCShipments()
			{
				ShippingNoteID = syncstatus.LocalID.ValueField()
			};
			giResult.Results = cbapi.GetGIResult<BCShipmentsResult>(giResult, BCConstants.GenericInquiryShipmentDetails, cancellationToken: cancellationToken).ToList();
			if (giResult?.Results == null || giResult?.Results?.Any() != true) return EntityStatus.None;

			MapFilterFields(giResult?.Results, giResult);
			if (giResult?.ShipmentType.Value == SOShipmentType.DropShip)
			{
				return GetDropShipment(bucket, giResult);
			}
			else if (giResult?.ShipmentType.Value == SOShipmentType.Invoice)
			{
				return GetInvoice(bucket, giResult);
			}
			else
			{
				return GetShipment(bucket, giResult);
			}
		}


		public override async Task MapBucketExport(SPShipmentEntityBucket bucket, IMappedEntity existing, CancellationToken cancellationToken = default)
		{
			MappedShipment obj = bucket.Shipment;
			if (obj.Local?.Confirmed?.Value == false) throw new PXException(BCMessages.ShipmentNotConfirmed);
			List<BCLocations> locationMappings = new List<BCLocations>();
			if (currentBindingExt.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse)
			{
				foreach (PXResult<BCLocations, INSite, INLocation> result in PXSelectJoin<BCLocations,
					InnerJoin<INSite, On<INSite.siteID, Equal<BCLocations.siteID>>,
					LeftJoin<INLocation, On<INLocation.siteID, Equal<BCLocations.siteID>, And<BCLocations.locationID, Equal<INLocation.locationID>>>>>,
					Where<BCLocations.bindingID, Equal<Required<BCLocations.bindingID>>, And<BCLocations.mappingDirection, Equal<BCMappingDirectionAttribute.export>>>,
					OrderBy<Desc<BCLocations.mappingDirection>>>.Select(this, currentBinding.BindingID))
				{
					var bl = (BCLocations)result;
					var site = (INSite)result;
					var iNLocation = (INLocation)result;
					bl.SiteCD = site.SiteCD.Trim();
					bl.LocationCD = bl.LocationID == null ? null : iNLocation.LocationCD.Trim();
					locationMappings.Add(bl);
				}
			}
			if (obj.Local.ShipmentType.Value == SOShipmentType.DropShip)
			{
				PurchaseReceipt impl = obj.Local.POReceipt;
				MapDropShipment(bucket, obj, impl, locationMappings);
			}
			else if (obj.Local.ShipmentType.Value == SOShipmentType.Issue)
			{
				Shipment impl = obj.Local.Shipment;
				MapShipment(bucket, obj, impl, locationMappings);
			}
			else
			{
				Shipment impl = obj.Local.Shipment;
				MapInvoice(bucket, obj, impl, locationMappings);
			}

			ValidateShipments(bucket, obj);
		}

		public override CustomField GetLocalCustomField(SPShipmentEntityBucket bucket, string viewName, string fieldName)
		{
			MappedShipment obj = bucket.Shipment;
			BCShipments impl = obj.Local;
			if (impl?.Results?.Count() > 0)
				return impl.Results[0].Custom?.Where(x => x.ViewName == viewName && x.FieldName == fieldName).FirstOrDefault();
			else return null;
		}

		public virtual void ValidateShipments(SPShipmentEntityBucket bucket, MappedShipment obj)
		{
			ShipmentData shipment = obj.Extern;
			//Validate all Shipments:
			//1. generate the removal list for these shipping lines have been removed from Shipment and no more shipping lines from the same order
			//2. match the existing external Shipment with current FulfillmentData
			//3. Validate the Shipping Qty, ensure "Shipping Qty" + "Shipped Qty in SPC" should be less or equal to order quantity.
			if (shipment.FulfillmentDataList.Any())
			{
				var existingDetailsForShipment = obj.Details.Where(d => d.EntityType == BCEntitiesAttribute.ShipmentLine || d.EntityType == BCEntitiesAttribute.ShipmentBoxLine);
				var existingDetailsForRemovedOrders = existingDetailsForShipment.Where(x => shipment.FulfillmentDataList.Any(osd => osd.OrderLocalID == x.LocalID) == false);
				//if detail existed but no more order in the Shipment, should delete the Shipment and update order status.
				//That means the Shipment exported to BC before, and then Shipment in AC changed, it removed previous Shipping items, and no more Shipping items represent to the order.
				//Scenario:
				//1. Shipment has itemA from orderA and itemB from orderB, sync the Shipment to SPC and it created sync details for both itemA and itemB.
				//2. Changed the Shipment, removed itemA and kept itemB in the Shipment, sync the Shipment again. System should cancel the itemA Shipment record in Shopify
				foreach (var detail in existingDetailsForRemovedOrders)
				{
					if (detail.ExternID.HasParent() && !string.IsNullOrEmpty(detail.ExternID.KeySplit(1)))
					{
						shipment.ExternShipmentsToRemove[detail.ExternID.KeySplit(1)] = detail.ExternID.KeySplit(0);
					}
				}

				//Group the shipmentData by Order, and then generate the removal list for the same existing shipments,
				//validate the Shipping Qty, ensure "Shipping Qty" + "Shipped Qty in SPC" should be less or equal to order quantity.
				foreach (var shipmentDataByOrder in shipment.FulfillmentDataList.GroupBy(x => new { x.OrderId, x.OrderLocalID }))
				{
					MappedOrder mappedOrder = bucket.Orders.FirstOrDefault(x => x.LocalID == shipmentDataByOrder.Key.OrderLocalID);

					string orderID = shipmentDataByOrder.Key.OrderId.ToString();

					//Get SyncDetails for current order
					var existingDetailsForOrder = existingDetailsForShipment.Where(x => x.LocalID == shipmentDataByOrder.Key.OrderLocalID);

					//Get extern Order, OrderShipments and OrderProducts from Shopify
					OrderData externOrder = orderDataProvider.GetByID(orderID, false, false, false);
					bucket.Orders.Add(new MappedOrder(externOrder, externOrder.Id.ToString(), externOrder.Name, null));
					List<FulfillmentData> existingFulfillments = externOrder.Fulfillments.Where(i => i.Status == FulfillmentStatus.Success).ToList();

					//qtyByItem for all items of order quantities and qtyUsedOnOrder is for the amount of objects processed by external shipments, refunds and used to prevent overshipping
					Dictionary<long, int> qtyByItem = externOrder.LineItems.Where(i => i.RequiresShipping == true).ToDictionary(x => (long)x.Id, x => x.Quantity ?? 0);
					Dictionary<long, int> qtyUsedOnOrder = qtyByItem.ToDictionary(item => (long)item.Key, item =>
						externOrder.Fulfillments.Where(i => i.Status == FulfillmentStatus.Success).SelectMany(i => i.LineItems).Where(i => i.Id == item.Key).Sum(i => i.Quantity ?? 0) +
						externOrder.Refunds.SelectMany(i => i.RefundLineItems).Where(i => i.LineItemId == item.Key && (i.RestockType == null || i.RestockType == RestockType.Cancel)).Sum(i => i.Quantity ?? 0));

					//If the order status in external order has been Canceled or Refunded, we should not create Shipment and throw error
					if (externOrder?.CancelledAt != null || externOrder?.FinancialStatus == OrderFinancialStatus.Refunded)
						throw new PXException(BCMessages.InvalidOrderStatusforShipment, externOrder?.Id, externOrder?.CancelledAt != null ? OrderStatus.Cancelled.ToString() : OrderFinancialStatus.Refunded.ToString());

					//Get all fulfillments that matching the externID in syncDetails
					var existingFulfillmentsForCurrent = existingFulfillments.Where(x => x != null && existingDetailsForOrder.Any(d => string.Equals(d.ExternID, $"{x.OrderId};{x.Id}", StringComparison.OrdinalIgnoreCase)));
					//If there is only one matching record, just need to assign existing fulfillmentID to current one. This should cover most of cases
					if (existingFulfillmentsForCurrent?.Count() == 1 && shipmentDataByOrder.Count() == 1)
					{
						var oneShipment = shipmentDataByOrder.First();
						AggregateSameLineItems(oneShipment);
						//if line items, quantity, and tracking info matched, there is no need to update
						if (existingFulfillmentsForCurrent.First().LineItems.All(i => oneShipment.LineItems.Any(x => x.Id == i.Id && x.Quantity == i.Quantity)) &&
									oneShipment.LineItems.All(i => existingFulfillmentsForCurrent.First().LineItems.Any(x => x.Id == i.Id && x.Quantity == i.Quantity)) &&
									(string.Equals(existingFulfillmentsForCurrent.First().TrackingNumbers?.FirstOrDefault(), oneShipment.TrackingNumbers.FirstOrDefault(), StringComparison.OrdinalIgnoreCase)) == true)
						{
							//Don't send duplicated message to customer
							oneShipment.NotifyCustomer = false;
							oneShipment.Id = existingFulfillmentsForCurrent.First().Id;
						}
						else
						{
							//Must cancel fulfillment because changing number of lines/item quantities impossible
							shipment.ExternShipmentsToRemove[existingFulfillmentsForCurrent.First().Id.ToString()] = orderID;

							//reduce the existing shipment qty and add current shipment qty to qtyUsedOnOrder dictionary in case to compare total qty later
							qtyByItem.Keys.ForEach(id => qtyUsedOnOrder[id] = qtyUsedOnOrder[id] - (existingFulfillmentsForCurrent.First()?.LineItems?.FirstOrDefault(i => i.Id == id)?.Quantity ?? 0)
														+ (oneShipment.LineItems.FirstOrDefault(i => i.Id == id)?.Quantity ?? 0));
						}
						existingFulfillments.Remove(existingFulfillmentsForCurrent.First());
					}
					else
					{
						//Validate Shipments, ensure all matching shipments link to existing shipment id and update data only.
						foreach (var oneShipment in shipmentDataByOrder)
						{
							//it is possible that we have the same line item twice for the same order in the same fulfillment.
							//If that is the case we have to join them otherwise it would not match with the external fulfillment. 
							AggregateSameLineItems(oneShipment);

							//find the matching fulfillment by tracking number
							FulfillmentData matchingFulfillment = existingFulfillments?.FirstOrDefault(i => !String.IsNullOrEmpty(i.TrackingNumbers.FirstOrDefault()) &&
								string.Equals(i.TrackingNumbers.FirstOrDefault(), oneShipment.TrackingNumbers.FirstOrDefault(), StringComparison.OrdinalIgnoreCase));
							//If the fulfillment is not matched by lines and quantities, but with tracking number, we cannot modify it externally and need to cancel it first
							if (matchingFulfillment != null)
							{
								existingFulfillments.Remove(matchingFulfillment);
								if (matchingFulfillment.LineItems.All(i => oneShipment.LineItems.Any(x => x.Id == i.Id && x.Quantity == i.Quantity)) &&
									oneShipment.LineItems.All(i => matchingFulfillment.LineItems.Any(x => x.Id == i.Id && x.Quantity == i.Quantity)) &&
									(string.Equals(matchingFulfillment.TrackingNumbers?.FirstOrDefault(), oneShipment.TrackingNumbers.FirstOrDefault(), StringComparison.OrdinalIgnoreCase)) == true)
								{
									oneShipment.NotifyCustomer = false;
									oneShipment.Id = matchingFulfillment.Id;
									continue;
								}
								//Must cancel fulfillment because changing number of lines/item quantities impossible
								shipment.ExternShipmentsToRemove[matchingFulfillment.Id.ToString()] = orderID;

								//reduce the existing shipment qty and add current shipment qty to qtyUsedOnOrder dictionary in case to compare total qty later
								qtyByItem.Keys.ForEach(id => qtyUsedOnOrder[id] = qtyUsedOnOrder[id] - (matchingFulfillment?.LineItems?.FirstOrDefault(i => i.Id == id)?.Quantity ?? 0)
															+ (oneShipment.LineItems.FirstOrDefault(i => i.Id == id)?.Quantity ?? 0));
							}
							else
							{
								//for items that cannot match by Tracking number, compare itemID and quantity to find the matching shipment
								var matchingFulfillments = existingFulfillments?.Where(i =>
									i.LineItems.All(x => oneShipment.LineItems.Any(item => item.Id == x.Id && item.Quantity == x.Quantity && x.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)) &&
									oneShipment.LineItems.All(x => i.LineItems.Any(item => item.Id == x.Id && item.Quantity == x.Quantity && item.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)));
								if (matchingFulfillments != null && matchingFulfillments.Count() > 1 && existingDetailsForOrder != null)
								{
									List<FulfillmentData> matches = new List<FulfillmentData>();
									//Find the relationship between sync detail record and external shipment
									foreach (var detail in existingDetailsForOrder)
									{
										matches.AddRange(matchingFulfillments.Where(mf => String.Equals(detail.ExternID, $"{mf.OrderId};{mf.Id}", StringComparison.OrdinalIgnoreCase))?.ToList() ?? new List<FulfillmentData>());
									}
									matchingFulfillments = matches;
								}
								if (matchingFulfillments.Any())
								{
									//for matching shipment, link it to existing one and only update.
									oneShipment.Id = matchingFulfillments.FirstOrDefault()?.Id;
									oneShipment.NotifyCustomer = false;
									existingFulfillments.Remove(matchingFulfillments.FirstOrDefault());
								}
								else
								{
									//If no matching shipment, add shipping qty to qtyUsedOnOrder dictionary in case to compare total qty later
									qtyByItem.Keys.ForEach(id => qtyUsedOnOrder[id] = qtyUsedOnOrder[id] + (oneShipment.LineItems.FirstOrDefault(i => i.Id == id)?.Quantity ?? 0));
								}
							}
						}
					}

					//If there is existing shipment sync details for current order, we should remove them in SPC and use the new data to create again.
					foreach (var detail in existingDetailsForOrder)
					{
						var externShipmentID = detail.ExternID.HasParent() ? detail.ExternID.KeySplit(1) : null;
						var existingShipment = existingFulfillments?.FirstOrDefault(x => x != null && x.Id.ToString() == externShipmentID);
						//if shipment has been created in SPC before, we should remove it later and then use new data to create again
						if (existingShipment != null)
						{
							shipment.ExternShipmentsToRemove[existingShipment.Id.ToString()] = orderID;
						}
					}

					//verify that after exporting we will not exceed item quantity on all fulfillments and also predict if order will be entirely fulfilled
					qtyUsedOnOrder.ForEach(x =>
					{
					if (x.Value > qtyByItem[x.Key])
						throw new PXException(BCMessages.ShipmentCannotBeExported, externOrder.LineItems.FirstOrDefault(i => i.Id == x.Key)?.Sku);
				});
				}
			}
		}

		/// <summary>
		/// Replace line items that has the same ID and Order ID by a new one with the sum of quantities.
		/// </summary>
		/// <param name="shipmentToExport"></param>
		public virtual void AggregateSameLineItems(FulfillmentData shipmentToExport)
		{
			var listOfRepeatedLineItems = shipmentToExport.LineItems
															.GroupBy(item => new { item.Id, item.OrderId })
															.Where(items => items.Count() > 1)
															.Select(group => new OrderLineItem()
															{
																Quantity = group.Sum(x => x.Quantity),
																OrderId = group.First().OrderId,
																Id = group.First().Id,
															});

			foreach (var repeatedLineItem in listOfRepeatedLineItems)
			{
				shipmentToExport.LineItems.RemoveAll(x => x.OrderId == repeatedLineItem.OrderId && x.Id == repeatedLineItem.Id);
				shipmentToExport.LineItems.Add(repeatedLineItem);
			}
		}

		public override async Task SaveBucketExport(SPShipmentEntityBucket bucket, IMappedEntity existing, String operation, CancellationToken cancellationToken = default)
		{
			MappedShipment obj = bucket.Shipment;

			if (obj.Extern.FulfillmentDataList.Any() == false)
			{
				SetInvalidStatus(obj, PXMessages.LocalizeFormatNoPrefixNLA(BCMessages.OrderShippingLineSyncronized, string.Empty, obj.Local.OrderNbr?.Value, bucket.Orders.FirstOrDefault()?.ExternID));
				return;
			}

			SetShouldBeExported(obj.Extern.FulfillmentDataList, bucket.Orders.Select(x => x.Extern).ToList());
			obj.ClearDetails();

			//Based on the validation result, cancel some shipments from Shopify
			foreach (var shipmentToRemove in obj.Extern.ExternShipmentsToRemove)
			{
				CancelFullfillment(bucket, shipmentToRemove.Value, shipmentToRemove.Key);
			}

			//Create all shipments for given order
			try
			{

				var externalOrderShipmentIds = new List<(string, string)>();
				var orderNumbers = new List<string>();
				foreach (var shipmentDataByOrder in obj.Extern.FulfillmentDataList.GroupBy(x => new { x.OrderId, x.OrderLocalID }))
				{
					MappedOrder mappedOrder = bucket.Orders.FirstOrDefault(x => x.LocalID == shipmentDataByOrder.Key.OrderLocalID);
					DateTime? lastModifiedOrderAt = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
					orderNumbers.Add(mappedOrder.ExternDescription);

					foreach (FulfillmentData shipmentData in shipmentDataByOrder)
					{
						//For Updating
						if (shipmentData.Id != null && shipmentData.ShouldBeExported == true)
							fulfillmentDataProvider.Delete(shipmentData.Id.ToString());
					}
					//For now we need to skip closed FulfillmentOrders because Shopify consider canceled FulfillmentOrders as closed,
					//So we don't know if its is closed because it is successfully completed or canceled.
					var externFulfilmentOrders = (shipmentDataByOrder.Any(x => x.ShouldBeExported == true)) ? //avoid unnecessary requests
													fulfillmentOrderDataProvider.GetAll(shipmentDataByOrder.Key.OrderId.ToString())
													.Where(x => x.Status != FulfillmentOrderStatus.Canceled && x.Status != FulfillmentOrderStatus.Closed)
													.ToArray() : null;

					foreach (FulfillmentData shipmentData in shipmentDataByOrder)
					{
						FulfillmentData data = null;

						if (shipmentData.ShouldBeExported == true)
						{
							//It is not possible to create a Fulfillment without informing a not closed/canceled FulfimentOrder in ItemsByFulfillmentOrder.
							SetItemsByFulfillmentOrder(shipmentData, externFulfilmentOrders);
							//If there are multiple fulfillmentOrders, we should separate the shipment by fulfillmentOrder to avoid "Multiple delivery method types" issue;
							if (shipmentData.ItemsByFulfillmentOrder?.Select(x => x.FulfillmentOrderId)?.Distinct()?.Count() > 1)
							{
								var groupedItems = shipmentData.ItemsByFulfillmentOrder.GroupBy(x => x.FulfillmentOrderId);
								foreach (var itemsByFulfillmentOrder in groupedItems)
								{
									shipmentData.ItemsByFulfillmentOrder = itemsByFulfillmentOrder.ToList();
									data = fulfillmentDataProvider.Create(shipmentData, shipmentData.OrderId.ToString());
								}
							}
							else
							data = fulfillmentDataProvider.Create(shipmentData, shipmentData.OrderId.ToString());
						}
						else
							data = shipmentData;

						externalOrderShipmentIds.Add((data.OrderId.ToString(), data.Id?.ToString()));

						if (lastModifiedOrderAt < data.DateModifiedAt)
							lastModifiedOrderAt = (DateTime)data.DateModifiedAt;

						obj.With(_ => { _.ExternID = null; return _; }).AddExtern(obj.Extern, new object[] { data.OrderId, data.Id }.KeyCombine(), mappedOrder.ExternDescription, data.DateModifiedAt.ToDate());
						obj.AddDetail(shipmentData.ShipmentType, shipmentData.OrderLocalID, new object[] { data.OrderId, data.Id }.KeyCombine());
					}

					//Get orderData to check the fulfillment status
					OrderData orderData = orderDataProvider.GetByID(shipmentDataByOrder.Key.OrderId.ToString(), false, false, false);
					if (orderData.FulfillmentStatus == OrderFulfillmentStatus.Fulfilled)
						lastModifiedOrderAt = orderData.DateModifiedAt;

					//Update order lastModifiedDate info in sync record
					mappedOrder.AddExtern(null, mappedOrder.ExternID, mappedOrder.Extern?.Name ?? mappedOrder.ExternDescription, lastModifiedOrderAt.ToDate(false));
					//Should keep the previous error message and status
					UpdateStatus(mappedOrder, null, mappedOrder.OriginalStatus.LastErrorMessage);
				}
				obj.ExternDescription = base.GetFormattedSyncDescriptionField(orderNumbers);
				obj.ExternID = base.GetFormtattedSyncExternalIdField(externalOrderShipmentIds);
				UpdateStatus(obj, operation);
			}
			catch (Exception ex)
			{
				UpdateStatus(obj, BCSyncOperationAttribute.ExternFailed, ex.InnerException?.Message ?? ex.Message);
			}

			#region Reset externalShipmentUpdated flag
			List<PXDataFieldParam> fieldParams = new List<PXDataFieldParam>();
			fieldParams.Add(new PXDataFieldAssign(typeof(BCSOShipmentExt.externalShipmentUpdated).Name, PXDbType.Bit, true));
			fieldParams.Add(new PXDataFieldRestrict(typeof(PX.Objects.SO.SOShipment.noteID).Name, PXDbType.UniqueIdentifier, obj.LocalID));
			PXDatabase.Update<PX.Objects.SO.SOShipment>(fieldParams.ToArray());
			#endregion
		}

		/// <summary>
		/// Sets in ShouldBeExported property if each FulfillmentData should be modified externally based on Tracking Company (ShipVia), LineItems id's and quantities, and Tracking info number of each fulfillment provided.
		/// </summary>
		/// <param name="localFulfilmentDatas">Fulfillments created locally to be compared</param>
		/// <param name="externOrderDatas">Order Data fetched externally</param>
		/// <returns></returns>
		public virtual void SetShouldBeExported(List<FulfillmentData> localFulfilmentDatas, List<OrderData> externOrderDatas)
		{
			localFulfilmentDatas.ForEach(x => AggregateSameLineItems(x));

			foreach (FulfillmentData localFulfillmentData in localFulfilmentDatas)
			{
				List<FulfillmentData> externFulfilments = externOrderDatas.FirstOrDefault(order => order?.Id == localFulfillmentData.OrderId)?.Fulfillments;
				var externFulfillment = externFulfilments.FirstOrDefault(fulfillment => localFulfillmentData.Id == fulfillment.Id);

				//Compares if there is an external Fulfillment exists, and if exists, compares its line items and tracking number with the local fulfillment, and vice-versa.
				localFulfillmentData.ShouldBeExported = externFulfillment == null
					|| !string.Equals(externFulfillment.TrackingCompany, localFulfillmentData.TrackingCompany, StringComparison.Ordinal)
					|| !(string.Equals(externFulfillment.TrackingNumbers?.FirstOrDefault(), localFulfillmentData.TrackingNumbers?.FirstOrDefault(), StringComparison.OrdinalIgnoreCase)
						&& localFulfillmentData.LineItems.All(localLineItem => externFulfillment.LineItems.Any(externLineItem => externLineItem.Id == localLineItem.Id && externLineItem.Quantity == localLineItem.Quantity))
						&& externFulfillment.LineItems.All(externLineItem => localFulfillmentData.LineItems.Any(localLineItem => localLineItem.Id == externLineItem.Id && localLineItem.Quantity == externLineItem.Quantity)));
			}
		}

		/// <summary>
		/// Sets Fulfillment Orders to the specified object.
		/// </summary>
		/// <param name="erpFulfilmentData">Local fulfillment to be set.</param>
		/// <param name="fulfillmentOrders">External fulfillmentOrders</param>
		public virtual void SetItemsByFulfillmentOrder(FulfillmentData erpFulfilmentData, IEnumerable<FulfillmentOrder> fulfillmentOrders = null)
		{
			erpFulfilmentData.ItemsByFulfillmentOrder = new List<LineItemsByFulfillmentOrder>();

			foreach (OrderLineItem localLine in erpFulfilmentData.LineItems)
			{
				bool matchFound = false;

				foreach (FulfillmentOrder shopifyFulfilmentOrder in fulfillmentOrders)
				{
					//Fulfillment Orders are divided by location and by type,
					//Because of that it is possible to have a fulfillment order without the localLine item, then we need to check.
					FulfillmentLineItem matchLineItem = shopifyFulfilmentOrder.LineItems.FirstOrDefault(shopifyItem => shopifyItem.LineItemId == localLine.Id);

					//Checks if the matchLineItem is fulfillable to the quantity shipped and validates the status of the FulfilmentOrder.
					if (matchLineItem == null || shopifyFulfilmentOrder.Status == FulfillmentOrderStatus.Closed || matchLineItem.FulfillableQuantity == 0) continue;
					matchFound = true;

					//Since the quantities are validated then we can consider that there are fulfilmentOrders enough for each line item.
					//it is possible that we are trying to create one fulfillment for different fulfillment orders.
					int? quantityToFulfill;
					bool isLocalLineItemFulfilled;
					if (matchLineItem.FulfillableQuantity < localLine.Quantity)
					{
						//if the quantity we want to fulfill is not satisfied, then we need to keep looping.
						quantityToFulfill = matchLineItem.FulfillableQuantity;
						localLine.Quantity -= matchLineItem.FulfillableQuantity;
						matchLineItem.FulfillableQuantity = 0;
						isLocalLineItemFulfilled = false;
					}
					else
					{
						matchLineItem.FulfillableQuantity -= localLine.Quantity;
						quantityToFulfill = localLine.Quantity;
						isLocalLineItemFulfilled = true;
					}

					var orderLineToFulfill = new OrderLineItem()
					{
						Id = matchLineItem.Id,
						Quantity = quantityToFulfill
					};

					//We have to check if there is already an ItemsByFulfillmentOrder created before,
					//Because one FulfilmentOrder could have more than one line item and we cannot overwrite if it is the case.
					LineItemsByFulfillmentOrder existentItemsByFulfillmentOrder = erpFulfilmentData.ItemsByFulfillmentOrder.FirstOrDefault(x => x.FulfillmentOrderId == shopifyFulfilmentOrder.Id);

					if (existentItemsByFulfillmentOrder == null)
					{
						erpFulfilmentData.ItemsByFulfillmentOrder.Add(new LineItemsByFulfillmentOrder()
						{
							FulfillmentOrderId = shopifyFulfilmentOrder.Id,
							FulfillmentOrderLineItems = new List<OrderLineItem>() { orderLineToFulfill }
						});
					}
					else
						existentItemsByFulfillmentOrder.FulfillmentOrderLineItems.Add(orderLineToFulfill);

					if (isLocalLineItemFulfilled) break;
				}
				//In case we do not find the order line in any valid fulfillment order, it is possible that this fulfillment order has been closed or canceled manually.
				if (!matchFound)
					throw new PXException(BCMessages.ShipmentWithoutFulfilmentOrder, localLine.Id);
			}
		}

		protected virtual void CancelFullfillment(SPShipmentEntityBucket bucket, String orderID, String fulfillmentID)
		{
			if (!string.IsNullOrEmpty(fulfillmentID))
			{
				try
				{
					fulfillmentDataProvider.CancelFulfillment(orderID, fulfillmentID);
				}
				catch (Exception ex)
				{
					Log(bucket?.Primary?.SyncID, SyncDirection.Export, ex);
				}
			}
		}
				
		#region ShipmentGetSection

		protected virtual void GetOrderShipment(BCShipments bCShipments)
		{
			if (bCShipments.ShipmentType?.Value == SOShipmentType.DropShip)
				GetDropShipmentByShipmentNbr(bCShipments);
			else if (bCShipments.ShipmentType.Value == SOShipmentType.Invoice)
				GetInvoiceByShipmentNbr(bCShipments);
			else
				bCShipments.Shipment = cbapi.GetByID<Shipment>(bCShipments.ShippingNoteID.Value,
					new Shipment()
					{
						ReturnBehavior = ReturnBehavior.OnlySpecified,
						Details = new List<ShipmentDetail>() { new ShipmentDetail() },
						Packages = new List<ShipmentPackage>() { new ShipmentPackage() },
						Orders = new List<ShipmentOrderDetail>() { new ShipmentOrderDetail() },
					});

		}

		protected virtual void GetInvoiceByShipmentNbr(BCShipments bCShipment)
		{
			bCShipment.Shipment = new Shipment();
			bCShipment.Shipment.Details = new List<ShipmentDetail>();

			foreach (PXResult<ARTran, SOOrder> item in PXSelectJoin<ARTran,
				InnerJoin<SOOrder, On<ARTran.sOOrderType, Equal<SOOrder.orderType>, And<ARTran.sOOrderNbr, Equal<SOOrder.orderNbr>>>>,
				Where<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>
			.Select(this, bCShipment.ShipmentNumber.Value))
			{
				ARTran line = item.GetItem<ARTran>();
				ShipmentDetail detail = new ShipmentDetail();
				detail.OrderNbr = line.SOOrderNbr.ValueField();
				detail.OrderLineNbr = line.SOOrderLineNbr.ValueField();
				detail.OrderType = line.SOOrderType.ValueField();
				detail.ShippedQty = line.Qty.ValueField();
				bCShipment.Shipment.Details.Add(detail);
			}
		}

		//
		// TODO: AC-287742 The SPShipmentProcessor::GetDropShipmentByShipmentNbr and BCShipmentProcessor:GetDropShipmentByShipmentNbr should be moved to the base class.
		//
		protected virtual void GetDropShipmentByShipmentNbr(BCShipments bCShipments)
		{
			bCShipments.POReceipt = new PurchaseReceipt();
			bCShipments.POReceipt.ShipmentNbr = bCShipments.ShipmentNumber;
			bCShipments.POReceipt.VendorRef = bCShipments.VendorRef;
			bCShipments.POReceipt.ReceiptNbr = bCShipments.ReceiptNbr;
			bCShipments.POReceipt.ReceiptDate = bCShipments.ReceiptDate;
			bCShipments.POReceipt.Details = new List<PurchaseReceiptDetail>();

			foreach (PXResult<SOLineSplit, POOrder, SOOrder> item in PXSelectJoin<SOLineSplit,
				LeftJoin<POOrder, On<POOrder.orderType, Equal<SOLineSplit.pOType>, And<POOrder.orderNbr, Equal<SOLineSplit.pONbr>>>,
				LeftJoin<SOOrder, On<SOOrder.orderType, Equal<SOLineSplit.orderType>, And<SOOrder.orderNbr, Equal<SOLineSplit.orderNbr>>>>>,
				Where< SOLineSplit.pOReceiptType, Equal<Required<SOLineSplit.pOReceiptType>>, And<SOLineSplit.pOReceiptNbr, Equal<Required<SOLineSplit.pOReceiptNbr>>>>>
			.Select(this, bCShipments.ReceiptType.Value, bCShipments.ShipmentNumber.Value))
			{
				SOLineSplit lineSplit = item.GetItem<SOLineSplit>();
				SOOrder line = item.GetItem<SOOrder>();
				POOrder poOrder = item.GetItem<POOrder>();
				PurchaseReceiptDetail detail = new PurchaseReceiptDetail();
				detail.SOOrderNbr = lineSplit.OrderNbr.ValueField();
				detail.SOLineNbr = lineSplit.LineNbr.ValueField();
				detail.SOOrderType = lineSplit.OrderType.ValueField();
				detail.ReceiptQty = lineSplit.ShippedQty.ValueField();
				detail.ShipVia = poOrder.ShipVia.ValueField();
				detail.SONoteID = line.NoteID.ValueField();
				bCShipments.POReceipt.Details.Add(detail);
				bCShipments.POReceipt.VendorID = poOrder.VendorID.ValueField();
			}
		}

		protected virtual EntityStatus GetDropShipment(SPShipmentEntityBucket bucket, BCShipments bCShipments)
		{
			if (bCShipments.ShipmentNumber == null) return EntityStatus.None;
			GetDropShipmentByShipmentNbr(bCShipments);
			if (bCShipments.POReceipt == null || bCShipments.POReceipt?.Details?.Count == 0)
				return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipments, bCShipments.ShippingNoteID.Value, bCShipments.LastModified.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<PurchaseReceiptDetail> lines = bCShipments.POReceipt.Details
				.GroupBy(r => new { OrderType = r.SOOrderType.Value, OrderNbr = r.SOOrderNbr.Value })
				.Select(r => r.First());
			foreach (PurchaseReceiptDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.SOOrderType.Value.SearchField(), OrderNbr = line.SOOrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipments.POReceipt.ShipmentNbr.Value);

				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}
		protected virtual EntityStatus GetShipment(SPShipmentEntityBucket bucket, BCShipments bCShipment)
		{
			if (bCShipment.ShippingNoteID == null || bCShipment.ShippingNoteID.Value == Guid.Empty) return EntityStatus.None;
			bCShipment.Shipment = cbapi.GetByID<Shipment>(bCShipment.ShippingNoteID.Value,
					new Shipment()
					{
						ReturnBehavior = ReturnBehavior.OnlySpecified,
						Details = new List<ShipmentDetail>() { new ShipmentDetail() },
						Packages = new List<ShipmentPackage>() { new ShipmentPackage() },
						Orders = new List<ShipmentOrderDetail>() { new ShipmentOrderDetail() },
					});
			if (bCShipment.Shipment == null || bCShipment.Shipment?.Details?.Count == 0)
				return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipment, bCShipment.ShippingNoteID.Value, bCShipment.LastModified.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<ShipmentDetail> lines = bCShipment.Shipment.Details
				.GroupBy(r => new { OrderType = r.OrderType.Value, OrderNbr = r.OrderNbr.Value })
				.Select(r => r.First());
			foreach (ShipmentDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.OrderType.Value.SearchField(), OrderNbr = line.OrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipment.Shipment.ShipmentNbr.Value);
				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}
		protected virtual EntityStatus GetInvoice(SPShipmentEntityBucket bucket, BCShipments bCShipment)
		{
			if (bCShipment.ShipmentNumber == null) return EntityStatus.None;
			GetInvoiceByShipmentNbr(bCShipment);
			if (bCShipment.Shipment?.Details?.Count == 0) return EntityStatus.None;

			MappedShipment obj = bucket.Shipment = bucket.Shipment.Set(bCShipment, bCShipment.ShippingNoteID.Value, bCShipment.LastModified.Value);
			EntityStatus status = EnsureStatus(obj, SyncDirection.Export);

			IEnumerable<ShipmentDetail> lines = bCShipment.Shipment.Details
				.GroupBy(r => new { OrderType = r.OrderType.Value, OrderNbr = r.OrderNbr.Value })
				.Select(r => r.First());
			foreach (ShipmentDetail line in lines)
			{
				SalesOrder orderImpl = cbapi.Get<SalesOrder>(new SalesOrder() { OrderType = line.OrderType.Value.SearchField(), OrderNbr = line.OrderNbr.Value.SearchField() });
				if (orderImpl == null) throw new PXException(BCMessages.OrderNotFound, bCShipment.Shipment.ShipmentNbr.Value);
				MappedOrder orderObj = new MappedOrder(orderImpl, orderImpl.SyncID, orderImpl.SyncTime);
				EntityStatus orderStatus = EnsureStatus(orderObj);

				if (orderObj.ExternID == null) throw new PXException(BCMessages.OrderNotSyncronized, orderImpl.OrderNbr.Value);

				bucket.Orders.Add(orderObj);
			}
			return status;
		}

		#endregion

		#region ShipmentMappingSection

		protected virtual void MapDropShipment(SPShipmentEntityBucket bucket, MappedShipment obj, PurchaseReceipt impl, List<BCLocations> locationMappings)
		{
			ShipmentData shipment = obj.Extern = new ShipmentData();

			foreach (MappedOrder order in bucket.Orders)
			{
				FulfillmentData shipmentData = new FulfillmentData();
				shipmentData.LineItems = new List<OrderLineItem>();

				shipmentData.OrderId = order.ExternID.ToLong();
				shipmentData.OrderLocalID = order.LocalID;
				shipmentData.LocationId = defaultLocationId;
				shipmentData.ShipmentType = BCEntitiesAttribute.ShipmentLine;

				var shipvia = impl.Details.FirstOrDefault(x => !string.IsNullOrEmpty(x.ShipVia?.Value))?.ShipVia?.Value ?? string.Empty;
				shipmentData.TrackingNumbers = new List<string>() { impl.VendorRef?.Value };
				shipmentData.TrackingInfo = new TrackingInfo()
				{
					Number = impl.VendorRef?.Value,
					Company = GetCarrierName(shipvia)
				};
				shipmentData.NotifyCustomer = true;

				foreach (PurchaseReceiptDetail line in impl.Details ?? new List<PurchaseReceiptDetail>())
				{
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.SOOrderType.Value && order.Local.OrderNbr.Value == line.SOOrderNbr.Value && d.LineNbr.Value == line.SOLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					if (lineInfo == null) lineInfo = MatchOrderLineFromExtern(order?.ExternID, orderLine.InventoryID.Value); //Try to fetch line data from external system in case item was extra added but not synced to ERP
					if (lineInfo == null) continue;

					OrderLineItem shipItem = new OrderLineItem();
					shipItem.Id = lineInfo.ExternID.ToLong();
					shipItem.Quantity = (int)line.ReceiptQty?.Value;
					shipItem.OrderId = order.ExternID.ToLong();

					shipmentData.LineItems.Add(shipItem);
				}

				//Add to Shipment only if ShipmentItems have value
				if (shipmentData.LineItems.Any())
					shipment.FulfillmentDataList.Add(shipmentData);
			}
		}

		protected virtual void MapInvoice(SPShipmentEntityBucket bucket, MappedShipment obj, Shipment impl, List<BCLocations> locationMappings)
		{
			ShipmentData shipment = obj.Extern = new ShipmentData();

			foreach (MappedOrder order in bucket.Orders)
			{
				FulfillmentData shipmentData = new FulfillmentData();
				shipmentData.LineItems = new List<OrderLineItem>();
				shipmentData.OrderId = order.ExternID.ToLong();
				shipmentData.OrderLocalID = order.LocalID;
				shipmentData.ShipmentType = BCEntitiesAttribute.ShipmentLine;
				shipmentData.LocationId = defaultLocationId;
				shipmentData.TrackingNumbers = new List<string>();
				shipmentData.TrackingInfo = new TrackingInfo();
				shipmentData.NotifyCustomer = true;

				foreach (ShipmentDetail line in impl.Details ?? new List<ShipmentDetail>())
				{
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.OrderType.Value && order.Local.OrderNbr.Value == line.OrderNbr.Value && d.LineNbr.Value == line.OrderLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					if (lineInfo == null) lineInfo = MatchOrderLineFromExtern(order?.ExternID, orderLine.InventoryID.Value); //Try to fetch line data from external system in case item was extra added but not synced to ERP
					if (lineInfo == null) continue;

					OrderLineItem shipItem = new OrderLineItem();
					shipItem.Id = lineInfo.ExternID.ToLong();
					shipItem.Quantity = (int)line.ShippedQty?.Value;
					shipItem.OrderId = order.ExternID.ToLong();
					shipmentData.LineItems.Add(shipItem);
				}

				//Add to Shipment only if ShipmentItems have value
				if (shipmentData.LineItems.Any())
					shipment.FulfillmentDataList.Add(shipmentData);
			}
		}

		protected virtual void MapShipment(SPShipmentEntityBucket bucket, MappedShipment obj, Shipment impl, List<BCLocations> locationMappings)
		{
			ShipmentData shipment = obj.Extern = new ShipmentData();

			//Get Package Details, there is only InventoryID in SOShipLineSplitPackage, in case to compare InventoryCD field with Shipping line item, get InventoryCD from InventoryItem and save it in the Tuple.
			List<Tuple<SOShipLineSplitPackage, InventoryItem>> PackageDetails = new List<Tuple<SOShipLineSplitPackage, InventoryItem>>();
			foreach (PXResult<SOShipLineSplitPackage, InventoryItem> item in PXSelectJoin<SOShipLineSplitPackage,
				InnerJoin<InventoryItem, On<SOShipLineSplitPackage.inventoryID, Equal<InventoryItem.inventoryID>>>,
				Where<SOShipLineSplitPackage.shipmentNbr, Equal<Required<SOShipLineSplitPackage.shipmentNbr>>>>.
				Select(this, impl.ShipmentNbr?.Value))
			{
				PackageDetails.Add(Tuple.Create(item.GetItem<SOShipLineSplitPackage>(), item.GetItem<InventoryItem>()));
			}

			var packages = new List<ShipmentPackage>();

			foreach (ShipmentPackage package in impl.Packages ?? new List<ShipmentPackage>())
			{
				//Check the Package whether has shipping items in it
				var detail = PackageDetails.Where(x => x.Item1.PackageLineNbr == package.LineNbr?.Value && x.Item1.PackedQty != 0);

				if (string.IsNullOrEmpty(package.TrackingNbr?.Value))
					continue; // if tracking number is empty then ignore

				//If there is not content in the package, that means it's a empty package, but we don't ship empty boxes
				//So later we will get all the rest items and add them to one shipment.
				//If it's not empty, add detail item info to ShipmentLineNbr, in case to compare with Shipping lines later
				foreach (var item in detail ?? Enumerable.Empty<Tuple<SOShipLineSplitPackage, InventoryItem>>())
				{
					SOShipLineSplitPackage soShipLine = item.Item1;
					InventoryItem inventoryItem = item.Item2;
					decimal packedQty = (string.Equals(soShipLine.UOM, inventoryItem.SalesUnit)) ?
						soShipLine.PackedQty ?? 0m:
						POItemCostManager.ConvertUOM(this, inventoryItem, inventoryItem.SalesUnit, soShipLine.PackedQty.Value, soShipLine.UOM);
						
					package.ShipmentLineNbr.Add(new Tuple<int?, decimal?>(soShipLine.ShipmentLineNbr, packedQty));
				}

				packages.Add(package);
			}

			foreach (MappedOrder order in bucket.Orders)
			{
				//get all line items for the current order in this Shipment
				Dictionary<ShipmentDetail, DetailInfo> ShippingLineDetails = new Dictionary<ShipmentDetail, DetailInfo>();
				foreach (ShipmentDetail line in impl.Details ?? new List<ShipmentDetail>())
				{
					SalesOrderDetail orderLine = order.Local.Details.FirstOrDefault(d =>
						order.Local.OrderType.Value == line.OrderType.Value && order.Local.OrderNbr.Value == line.OrderNbr.Value && d.LineNbr.Value == line.OrderLineNbr.Value);
					if (orderLine == null) continue; //skip shipment that is not from this order

					DetailInfo lineInfo = order.Details.FirstOrDefault(d => d.EntityType == BCEntitiesAttribute.OrderLine && d.LocalID == orderLine.NoteID.Value);
					//if no data found in sync detail, try to fetch line data from external system in case item was extra added but not synced to ERP
					if (lineInfo == null)
						lineInfo = MatchOrderLineFromExtern(order?.ExternID, orderLine.InventoryID.Value);
					if (lineInfo == null)
						continue;// if order line not present in external system then just skip

					ShippingLineDetails[line] = lineInfo;
				}

				if (!ShippingLineDetails.Any()) continue;

				//Lookup by packages first
				foreach (ShipmentPackage onePackage in packages)
				{
					//Get the line items in the package
					var shippingLinesInPackage = ShippingLineDetails.Keys.Where(x => onePackage.ShipmentLineNbr.Any() && onePackage.ShipmentLineNbr.Select(y => y.Item1).Contains(x.LineNbr?.Value));
					//if no lines in the package and package is not empty box, that means the package is not for this order, we should skip it.
					if (onePackage.ShipmentLineNbr.Any() && !shippingLinesInPackage.Any()) continue;

					FulfillmentData shipmentDataByPackage = new FulfillmentData();
					shipmentDataByPackage.LineItems = new List<OrderLineItem>();
					shipmentDataByPackage.ShipmentType = BCEntitiesAttribute.ShipmentLine;
					shipmentDataByPackage.OrderId = order.ExternID?.ToLong();
					shipmentDataByPackage.OrderLocalID = order.LocalID;
					shipmentDataByPackage.TrackingNumbers = new List<string>() { onePackage.TrackingNbr?.Value };
					shipmentDataByPackage.TrackingInfo = new TrackingInfo()
					{
						Number = onePackage.TrackingNbr?.Value,
						Company = GetCarrierName(impl.ShipVia?.Value ?? string.Empty)
					};
					shipmentDataByPackage.NotifyCustomer = true;

					foreach (ShipmentDetail line in shippingLinesInPackage)
					{
						//Use FulfillmentOrderId
						shipmentDataByPackage.LocationId = GetMappedExternalLocation(locationMappings, impl.WarehouseID?.Value, line?.LocationID?.Value);

						//Get the SyncDetail for current line
						DetailInfo lineInfo = ShippingLineDetails[line];
						int shippedQuantity;

						//Non-stock kits
						if (PackageDetails.Any(x => x.Item1.PackageLineNbr == onePackage.LineNbr.Value && x.Item1.ShipmentLineNbr == line.LineNbr.Value && x.Item2.InventoryCD.Trim() != line.InventoryID.Value.Trim()) == true)
						{
							//Skip shipping line if its ShippingQty is 0, because the non-stock kit has shipped in other package
							if (line.ShippedQty.Value == 0) continue;

							//Use ShippedQty of line item instead of packedQty of non-stock kits
							shippedQuantity = (int)(line.ShippedQty.Value ?? 0);
							//Reduce the ShippedQty if it has used.
							line.ShippedQty = 0m.ValueField();
						}
						else //normal items
						{
							//Qty should use the actual packedQty.
							decimal sumOfQuantities = onePackage?.ShipmentLineNbr.Where(x => x.Item1 == line.LineNbr?.Value)?.Sum(x => x.Item2) ?? 0;

							//if its decimal, we allocate all line items in one shipment.
							bool isDecimalValue = sumOfQuantities % 1 > 0;
							shippedQuantity = (isDecimalValue)?
								(int)(line.ShippedQty.Value ?? 0) : //Allocate all items
								(int)(sumOfQuantities); 
							if (shippedQuantity == 0) continue; //Skip shipping line if its ShippingQty is 0

							//Reduce the ShippedQty if it has used.
							line.ShippedQty = (isDecimalValue) ? 0m.ValueField() : (line.ShippedQty.Value - shippedQuantity).ValueField();
						}

						OrderLineItem shipItem = new OrderLineItem();
						shipItem.Id = lineInfo.ExternID.ToLong();
						shipItem.OrderId = order.ExternID.ToLong();
						shipItem.Quantity = shippedQuantity;
						shipmentDataByPackage.LineItems.Add(shipItem);
					}

					//We should not ship fulfillment without LineItems or if their quantities are 0.
					if (shipmentDataByPackage.LineItems.Any() && !shipmentDataByPackage.LineItems.Any(item => item.Quantity == 0))
						shipment.FulfillmentDataList.Add(shipmentDataByPackage);
				}

				//if shipping lines still have ShippedQty, that means there is no package for them. Put them in emptybox or virtual package without tracking number
				var restShippingLines = ShippingLineDetails.Keys.Where(x => x.ShippedQty?.Value > 0);
				if (restShippingLines.Any())
				{
					var trackingNumber = impl.Packages?.FirstOrDefault(p => p.ShipmentLineNbr.Any() == false && !string.IsNullOrEmpty(p.TrackingNbr?.Value))?.TrackingNbr.Value;//If no emptybox, tracking number should be empty
					FulfillmentData shipmentDataForRestLines;
					//If the shipment for empty box has been created, put all rest items to this shipment, otherwise create a new one
					if (!string.IsNullOrEmpty(trackingNumber) && shipment.FulfillmentDataList.Any(x => x.TrackingInfo.Number == trackingNumber && x.OrderLocalID == order.LocalID))
					{
						shipmentDataForRestLines = shipment.FulfillmentDataList.First(x => x.TrackingInfo.Number == trackingNumber && x.OrderLocalID == order.LocalID);
					}
					else
					{
						shipmentDataForRestLines = new FulfillmentData();
						shipmentDataForRestLines.LineItems = new List<OrderLineItem>();
						shipmentDataForRestLines.ShipmentType = BCEntitiesAttribute.ShipmentLine;
						shipmentDataForRestLines.OrderId = order.ExternID?.ToLong();
						shipmentDataForRestLines.OrderLocalID = order.LocalID;
						shipmentDataForRestLines.TrackingNumbers = new List<string>() { trackingNumber };
						shipmentDataForRestLines.TrackingInfo = new TrackingInfo()
						{
							Number = trackingNumber,
							Company = GetCarrierName(impl.ShipVia?.Value ?? string.Empty)
						};
						shipmentDataForRestLines.NotifyCustomer = true;
						shipment.FulfillmentDataList.Add(shipmentDataForRestLines);
					}

					foreach (ShipmentDetail line in restShippingLines)
					{
						if (line.ShippedQty.Value == null || line.ShippedQty.Value < 1) continue;

						//Get the SyncDetail for current line
						DetailInfo lineInfo = ShippingLineDetails[line];

						OrderLineItem shipItem = new OrderLineItem();
						shipItem.Id = lineInfo.ExternID.ToLong();
						shipItem.OrderId = order.ExternID.ToLong();
						shipItem.Quantity = (int)(line.ShippedQty.Value); //Use rest ShippedQty of line item
						shipmentDataForRestLines.LineItems.Add(shipItem);
					}
				}
			}
		}

		#endregion
		/// <summary>
		/// Searches for <paramref name="shipVia"/> in shipping carrier substitution list used in store settings.
		/// </summary>
		/// <param name="shipVia"></param>
		/// <returns>The found value or the provided <paramref name="shipVia"/> value.</returns>
		protected virtual string GetCarrierName(string shipVia)
		{
			if (string.IsNullOrEmpty(shipVia))	return string.Empty;
			string substitutionList = GetBindingExt<BCBindingExt>().ShippingCarrierListID;

			return GetHelper<SPHelper>().GetSubstituteLocalByExtern(substitutionList, shipVia, def: shipVia);
		}

		protected virtual DetailInfo MatchOrderLineFromExtern(string externalOrderId, string identifyKey)
		{
			DetailInfo lineInfo = null;
			if (string.IsNullOrEmpty(externalOrderId) || string.IsNullOrEmpty(identifyKey))
				return lineInfo;
			var orderLineDetails = orderDataProvider.GetByID(externalOrderId, includedMetafields: false, includedTransactions: false, includedCustomer: false, includedOrderRisk: false)?.LineItems;
			var matchedLine = orderLineDetails?.FirstOrDefault(x => string.Equals(x?.Sku, identifyKey, StringComparison.OrdinalIgnoreCase));
			if (matchedLine != null && matchedLine?.Id.HasValue == true)
			{
				lineInfo = new DetailInfo(BCEntitiesAttribute.OrderLine, null, matchedLine.Id.ToString());
			}
			return lineInfo;
		}

		protected virtual long? GetMappedExternalLocation(List<BCLocations> locationMappings, string siteCD, string locationCD)
		{
			if (locationMappings?.Count == 0 || string.IsNullOrEmpty(siteCD))
				return defaultLocationId;
			var matchedItem = locationMappings.FirstOrDefault(l => !string.IsNullOrEmpty(l.ExternalLocationID) && string.Equals(l.SiteCD, siteCD, StringComparison.OrdinalIgnoreCase) && (l.LocationID == null || (l.LocationID != null && string.Equals(l.LocationCD, locationCD, StringComparison.OrdinalIgnoreCase))));
			if (matchedItem != null)
				return inventoryLocations.Any(x => x.Id?.ToString() == matchedItem.ExternalLocationID) ? matchedItem.ExternalLocationID?.ToLong() : defaultLocationId;
			else
				return defaultLocationId;
		}
		#endregion
	}
}
