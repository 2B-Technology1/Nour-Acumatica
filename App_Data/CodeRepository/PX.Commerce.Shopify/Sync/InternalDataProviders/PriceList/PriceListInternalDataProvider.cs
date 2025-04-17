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

using PX.Commerce.Core;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data.SQLTree;
using Microsoft.Extensions.Logging;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Objects.Availability;
using InventoryItem = PX.Objects.IN.InventoryItem;

namespace PX.Commerce.Shopify
{
	///<inheritdoc/>
	public class PriceListInternalDataProvider : IPriceListInternalDataProvider
	{
		/// <summary>
		/// Retrieve All Price Class records with/without specified PriceClassIds
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassIds"></param>
		/// <returns></returns>
		public virtual IEnumerable<PriceListSalesPrice> GetPriceClasses(PXGraph graph, List<string> priceClassIds = null)
		{
			var queryResult = new SelectFrom<ARPriceClass>
							.LeftJoin<ARSalesPrice>.On<ARPriceClass.priceClassID.IsEqual<ARSalesPrice.custPriceClassID>
												   .And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>>
							.Where<P.AsString.IsNull.Or<ARPriceClass.priceClassID.IsIn<P.AsString>>>
							.AggregateTo<GroupBy<ARPriceClass.priceClassID, GroupBy<ARPriceClass.description, GroupBy<ARPriceClass.noteID, GroupBy<ARPriceClass.createdDateTime, GroupBy<ARPriceClass.lastModifiedDateTime>>>>>,
										 Max<ARSalesPrice.lastModifiedDateTime>, Count<ARSalesPrice.recordID>>.View(graph).Select(priceClassIds == null? null: (object)priceClassIds.ToArray());
			foreach (var item in queryResult)
			{
				var priceClass = item.GetItem<ARPriceClass>();
				var salesPrice = item.GetItem<ARSalesPrice>();
				var priceListSalesPrice = new PriceListSalesPrice()
				{
					PriceClassID = priceClass?.PriceClassID.Trim(),
					Description = priceClass?.Description?.Trim(),
					NoteID = priceClass?.NoteID,
					CreatedDateTime = priceClass?.CreatedDateTime,
					LastModifiedDateTime = priceClass?.LastModifiedDateTime,
					LastModifiedDateTimeForDetails = salesPrice?.LastModifiedDateTime,
					CountForDetails = salesPrice?.RecordID ?? 0
				};
				yield return priceListSalesPrice;
			}
		}

		///<inheritdoc/>
		public virtual PriceListSalesPrice GetPriceClass(PXGraph graph, Guid priceClassNoteId)
		{
			var queryResult = new SelectFrom<ARPriceClass>
							.LeftJoin<ARSalesPrice>.On<ARPriceClass.priceClassID.IsEqual<ARSalesPrice.custPriceClassID>
												   .And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>>
							.Where<ARPriceClass.noteID.IsEqual<P.AsGuid>>
							.AggregateTo<GroupBy<ARPriceClass.priceClassID, GroupBy<ARPriceClass.description, GroupBy<ARPriceClass.noteID, GroupBy<ARPriceClass.createdDateTime, GroupBy<ARPriceClass.lastModifiedDateTime>>>>>,
										 Max<ARSalesPrice.lastModifiedDateTime>, Count<ARSalesPrice.recordID>>.View(graph).Select(priceClassNoteId).FirstOrDefault();
			if(queryResult != null)
			{
				var priceClass = queryResult.GetItem<ARPriceClass>();
				var salesPrice = queryResult.GetItem<ARSalesPrice>();
				var priceListSalesPrice = new PriceListSalesPrice()
				{
					PriceClassID = priceClass?.PriceClassID.Trim(),
					Description = priceClass?.Description?.Trim(),
					NoteID = priceClass?.NoteID,
					CreatedDateTime = priceClass?.CreatedDateTime,
					LastModifiedDateTime = priceClass?.LastModifiedDateTime,
					LastModifiedDateTimeForDetails = salesPrice?.LastModifiedDateTime,
					CountForDetails = salesPrice?.RecordID ?? 0
				};
				return priceListSalesPrice;
			}

			return null;
		}

		/// <summary>
		/// Return the whole list of prices for a specified priceClass Id
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassNoteId"></param>
		/// <returns></returns>ConvertToStr
		public virtual IEnumerable<SalesPriceDetail> GetSalesPrices(PXGraph graph, Guid priceClassNoteId)
		{
			var totalRows = WebConfig.GetInt(BCConstants.CommerceInquiryPageSize, BCConstants.InquiryPageSize);
			int startRow = 0;
			var baseQuery = new SelectFrom<ARPriceClass>
							.InnerJoin<ARSalesPrice>.On<ARPriceClass.priceClassID.IsEqual<ARSalesPrice.custPriceClassID>
													.And<ARSalesPrice.priceType.IsEqual<PriceTypes.customerPriceClass>>>
							.InnerJoin<InventoryItem>.On<InventoryItem.inventoryID.IsEqual<ARSalesPrice.inventoryID>>
							.LeftJoin<ChildInventoryItem>.On<InventoryItem.isTemplate.IsEqual<True>
													.And<ChildInventoryItem.templateItemID.IsEqual<InventoryItem.inventoryID>>
													.And<ChildInventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>>>
							.Where<ARPriceClass.noteID.IsEqual<@P.AsGuid>
							.And<InventoryItem.itemStatus.IsNotInSequence<ExcludedInventoryItemStatusSequence>>
							.And<InventoryItem.exportToExternal.IsEqual<True>>>
							.OrderBy<ARSalesPrice.inventoryID.Asc, ARSalesPrice.curyID.Asc>.View(graph);

			while (true)
			{
				int resultCount = 0;

				using (new PXFieldScope(baseQuery.View, typeof(ARPriceClass.noteID), typeof(ARPriceClass.priceClassID), typeof(ARPriceClass.description), typeof(ARPriceClass.lastModifiedDateTime), typeof(ARSalesPrice.priceClassID)
				, typeof(ARSalesPrice.breakQty), typeof(ARSalesPrice.createdDateTime), typeof(ARSalesPrice.curyID), typeof(ARSalesPrice.customerCD), typeof(ARSalesPrice.customerID), typeof(ARSalesPrice.custPriceClassID), typeof(ARSalesPrice.description)
				, typeof(ARSalesPrice.discountable), typeof(ARSalesPrice.effectiveDate), typeof(ARSalesPrice.expirationDate), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.inventoryID), typeof(ARSalesPrice.isPromotionalPrice)
				, typeof(ARSalesPrice.lastModifiedDateTime), typeof(ARSalesPrice.noteID), typeof(ARSalesPrice.priceCode), typeof(ARSalesPrice.priceType), typeof(ARSalesPrice.salesPrice), typeof(ARSalesPrice.siteID), typeof(ARSalesPrice.taxCalcMode)
				, typeof(ARSalesPrice.uOM), typeof(InventoryItem.noteID), typeof(InventoryItem.itemStatus), typeof(InventoryItem.baseUnit), typeof(InventoryItem.salesUnit), typeof(InventoryItem.isTemplate), typeof(InventoryItem.templateItemID)
				, typeof(ChildInventoryItem.inventoryCD), typeof(ChildInventoryItem.inventoryID), typeof(ChildInventoryItem.noteID), typeof(ChildInventoryItem.itemStatus), typeof(ChildInventoryItem.baseUnit), typeof(ChildInventoryItem.salesUnit)))
				{
					foreach (var row in baseQuery.SelectWindowed(startRow, totalRows, priceClassNoteId))
					{
						resultCount++;

						InventoryItem item = row.GetItem<InventoryItem>();
						ChildInventoryItem childItem = row.GetItem<ChildInventoryItem>();
						ARPriceClass priceClass = row.GetItem<ARPriceClass>();
						ARSalesPrice salesPrice = row.GetItem<ARSalesPrice>();

						if (salesPrice != null)
						{
							var salesPriceDetail = new SalesPriceDetail()
							{
								NoteID = salesPrice.NoteID.ValueField(),
								PriceCode = salesPrice.PriceCode?.Trim().ValueField(),
								PriceClassNoteID = priceClass.NoteID.ValueField(),
								UOM = salesPrice.UOM.ValueField(),
								Warehouse = salesPrice.SiteID?.ToString()?.ValueField(),
								CurrencyID = salesPrice.CuryID.ValueField(),
								Promotion = salesPrice.IsPromotionalPrice.ValueField(),
								PriceType = salesPrice.PriceType.ValueField(),
								TaxCalculationMode = salesPrice.TaxCalcMode.ValueField(),
								InventoryID = item.InventoryCD?.Trim().ValueField(),
								LastModifiedDateTime = salesPrice.LastModifiedDateTime.ValueField(),
								EffectiveDate = salesPrice.EffectiveDate.ValueField(),
								ExpirationDate = salesPrice.ExpirationDate.ValueField(),
								Description = salesPrice.Description.ValueField(),
								BreakQty = salesPrice.BreakQty.ValueField(),
								Price = salesPrice.SalesPrice.ValueField(),
								Isvariant = childItem?.TemplateItemID != null,
								IsTemplate = item.IsTemplate,
								TemplateItemID = item.TemplateItemID,
								TemplateItemNoteID = childItem?.TemplateItemID != null ? item?.NoteID : null,
								//If SalesPriceItem is for TemplateItem, includes all matrix items with the same SalesPriceItem value, but NoteID will point to the matrix item instead,
								//The NoteID of TemplateItem will fill up in the TemplateItemNoteID field.
								InventoryNoteID = childItem?.NoteID == null ? item.NoteID : childItem.NoteID,
								BaseUnit = item.BaseUnit,
								SalesUnit = item.SalesUnit
							};

							yield return salesPriceDetail;
						}

					}

					startRow += resultCount;
					if (resultCount != totalRows)
						break;
				}
			}
			yield break;
		}

		/// <summary>
		/// Retrieve All Customer Locations associated with specified PriceClassId
		/// </summary>
		/// <param name="graph"></param>
		/// <param name="priceClassId"></param>
		/// <returns></returns>
		public virtual IEnumerable<CustomerLocation> GetCustomerLocationsWithPriceClass(PXGraph graph, string priceClassId)
		{
			var baseQuery = new SelectFrom<PX.Objects.CR.Location>
								.InnerJoin<PX.Objects.AR.Customer>.On<PX.Objects.AR.Customer.bAccountID.IsEqual<PX.Objects.CR.Location.bAccountID>>
								.Where<PX.Objects.AR.Customer.customerCategory.IsEqual<BCCustomerCategoryAttribute.organizationCategory>
									.And<PX.Objects.AR.Customer.status.IsNotEqual<CustomerStatus.inactive>>
									.And<PX.Objects.CR.Location.isActive.IsEqual<True>>
									.And<PX.Objects.CR.Location.cPriceClassID.IsEqual<P.AsString>>>.View(graph);

			using (new PXFieldScope(baseQuery.View, typeof(PX.Objects.AR.Customer.noteID), typeof(PX.Objects.AR.Customer.bAccountID), typeof(PX.Objects.AR.Customer.acctCD), typeof(PX.Objects.CR.BAccount.lastModifiedDateTime)
				, typeof(PX.Objects.CR.Location.noteID), typeof(PX.Objects.CR.Location.isActive), typeof(PX.Objects.CR.Location.status), typeof(PX.Objects.CR.Location.locationCD)
				, typeof(PX.Objects.CR.Location.locationID), typeof(PX.Objects.CR.Location.lastModifiedDateTime)))
			{
				foreach (var item in baseQuery.Select(priceClassId))
				{
					var location = item.GetItem<PX.Objects.CR.Location>();
					var customer = item.GetItem<PX.Objects.AR.Customer>();
					var customerLocation = new CustomerLocation()
					{
						CustomerNoteID = customer.NoteID,
						NoteID = location.NoteID.ValueField(),
						Active = location.IsActive.ValueField(),
						Status = location.Status.ValueField(),
						Customer = customer.AcctCD.ValueField(),
						LocationID = location.LocationCD.ValueField(),
						LastModifiedDateTime = location.LastModifiedDateTime.ValueField(),
					};
					yield return customerLocation;
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerable<BCSyncDetail> GetBCSyncDetailsForInventoryItem(PXGraph graph, ConnectorOperation operation, Guid?[] inventoryIDs)
		{
			if (inventoryIDs?.Length == 0)
				yield break;

			var baseQuery = new SelectFrom<BCSyncDetail>
							.InnerJoin<BCSyncStatus>.On<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>
							.Where<BCSyncStatus.connectorType.IsEqual<P.AsString>
								.And<BCSyncStatus.bindingID.IsEqual<P.AsInt>>
								.And<BCSyncStatus.entityType.IsInSequence<InventoryItemsEntityTypeSequence>>
								.And<BCSyncStatus.status.IsNotInSequence<InvalidSyncStatusSquence>>
								.And<BCSyncStatus.externID.IsNotNull>
								.And<BCSyncDetail.entityType.IsEqual<BCEntitiesAttribute.variant>>
								.And<BCSyncDetail.externID.IsNotNull>
								.And<BCSyncDetail.localID.IsIn<P.AsGuid>>>
							.View(graph);

			using (new PXFieldScope(baseQuery.View, typeof(BCSyncDetail.syncID), typeof(BCSyncDetail.entityType), typeof(BCSyncDetail.detailID), typeof(BCSyncDetail.localID), typeof(BCSyncDetail.externID)
				, typeof(BCSyncDetail.isHidden), typeof(BCSyncDetail.refNoteID)))
			{
				foreach (var item in baseQuery.Select(operation.ConnectorType, operation.Binding, (object)inventoryIDs))
				{
					var syncDetail = item.GetItem<BCSyncDetail>();
					if (syncDetail != null)
						yield return syncDetail;
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerable<BCSyncDetail> GetBCSyncDetailsForTemplateItem(PXGraph graph, ConnectorOperation operation, Guid?[] inventoryIDs)
		{
			if (inventoryIDs?.Length == 0)
				yield break;

			var baseQuery = new SelectFrom<BCSyncDetail>
							.InnerJoin<BCSyncStatus>.On<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>
							.Where<BCSyncStatus.connectorType.IsEqual<P.AsString>
								.And<BCSyncStatus.bindingID.IsEqual<P.AsInt>>
								.And<BCSyncStatus.entityType.IsEqual<BCEntitiesAttribute.productWithVariant>>
								.And<BCSyncStatus.status.IsNotInSequence<InvalidSyncStatusSquence>>
								.And<BCSyncStatus.externID.IsNotNull>
								.And<BCSyncStatus.localID.IsIn<P.AsGuid>>
								.And<BCSyncDetail.entityType.IsEqual<BCEntitiesAttribute.variant>>
								.And<BCSyncDetail.externID.IsNotNull>>
							.View(graph);

			using (new PXFieldScope(baseQuery.View, typeof(BCSyncDetail.syncID), typeof(BCSyncDetail.entityType), typeof(BCSyncDetail.detailID), typeof(BCSyncDetail.localID), typeof(BCSyncDetail.externID)
				, typeof(BCSyncDetail.isHidden), typeof(BCSyncDetail.refNoteID)))
			{
				foreach (var item in baseQuery.Select(operation.ConnectorType, operation.Binding, (object)inventoryIDs))
				{
					var syncDetail = item.GetItem<BCSyncDetail>();
					if (syncDetail != null)
						yield return syncDetail;
				}
			}
		}

		/// <inheritdoc/>
		public virtual IEnumerable<BCSyncDetail> GetBCSyncDetailsForLocations(PXGraph graph, ConnectorOperation operation, Guid?[] customerLocationIDs)
		{
			if (customerLocationIDs?.Length == 0)
				yield break;

			var baseQuery = new SelectFrom<BCSyncDetail>
							.InnerJoin<BCSyncStatus>.On<BCSyncStatus.syncID.IsEqual<BCSyncDetail.syncID>>
							.Where<BCSyncStatus.connectorType.IsEqual<P.AsString>
								.And<BCSyncStatus.bindingID.IsEqual<P.AsInt>>
								.And<BCSyncStatus.entityType.IsEqual<BCEntitiesAttribute.company>>
								.And<BCSyncStatus.status.IsNotInSequence<InvalidSyncStatusSquence>>
								.And<BCSyncStatus.externID.IsNotNull>
								.And<BCSyncDetail.entityType.IsEqual<BCEntitiesAttribute.companyLocation>>
								.And<BCSyncDetail.externID.IsNotNull>
								.And<BCSyncDetail.localID.IsIn<P.AsGuid>>>
							.View(graph);

			using (new PXFieldScope(baseQuery.View, typeof(BCSyncDetail.syncID), typeof(BCSyncDetail.entityType), typeof(BCSyncDetail.detailID), typeof(BCSyncDetail.localID), typeof(BCSyncDetail.externID)
				, typeof(BCSyncDetail.isHidden), typeof(BCSyncDetail.refNoteID)))
			{
				foreach (var item in baseQuery.Select(operation.ConnectorType, operation.Binding, (object)customerLocationIDs))
				{
					var syncDetail = item.GetItem<BCSyncDetail>();
					if (syncDetail != null)
						yield return syncDetail;
				}
			}
		}
	}
}
