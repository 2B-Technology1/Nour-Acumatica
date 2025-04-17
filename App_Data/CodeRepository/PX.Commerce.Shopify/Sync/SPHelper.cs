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

using Newtonsoft.Json.Linq;
using PX.Api.ContractBased.Models;
using PX.Commerce.Core;
using PX.Commerce.Core.API;
using PX.Commerce.Objects;
using PX.Commerce.Shopify.API.GraphQL;
using PX.Commerce.Shopify.API.REST;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify
{
	public class SPHelper : CommerceHelper
	{
		#region Inventory
		public virtual string GetInventoryCDByExternID(String productID, String variantID, String sku, String description, bool? isGiftCard, out string uom, out string alternateID)
		{
			return GetInventoryCDByExternID(productID, variantID, sku, description, isGiftCard, out uom, out alternateID, out string itemStatus);
		}

		public virtual String GetInventoryCDByExternID(String productID, String variantID, String sku, String description, bool? isGiftCard, out string uom, out string alternateID, out string itemStatus)
		{
			return GetInventoryCDByExternID(productID, variantID, sku, description, isGiftCard, out uom, out alternateID, out itemStatus, out var _, out var _);
		}

		public virtual String GetInventoryCDByExternID(String productID, String variantID, String sku, String description, bool? isGiftCard, out string uom, out string alternateID, out string itemStatus, out bool? stkItem, out bool? nonStockShip)
		{
			alternateID = null;
			if (isGiftCard == true)
			{
				Int32? giftCertItem = _processor.GetBindingExt<BCBindingExt>().GiftCertificateItemID;
				PX.Objects.IN.InventoryItem inventory = giftCertItem != null ? PX.Objects.IN.InventoryItem.PK.Find(this, giftCertItem) : null;
				if (inventory?.InventoryCD == null)
					throw new PXException(ShopifyMessages.NoGiftCertificateItem);

				uom = inventory.SalesUnit?.Trim();
				itemStatus = inventory.ItemStatus?.Trim();
				stkItem = inventory.StkItem;
				nonStockShip = inventory.NonStockShip;
				return inventory.InventoryCD.Trim();
			}

			String priorityUOM = null;
			PX.Objects.IN.InventoryItem item = null;
			if (!string.IsNullOrEmpty(productID) && !string.IsNullOrEmpty(variantID))
			{
				item = PXSelectJoin<PX.Objects.IN.InventoryItem,
						InnerJoin<BCSyncDetail, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncDetail.localID>>,
						InnerJoin<BCSyncStatus, On<BCSyncStatus.syncID, Equal<BCSyncDetail.syncID>>>>,
							Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
								And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
								And2<Where<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
									Or<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>,
									Or<BCSyncStatus.entityType, Equal<Required<BCSyncStatus.entityType>>>>>,
								And<BCSyncStatus.externID, Equal<Required<BCSyncStatus.externID>>,
								And<BCSyncDetail.externID, Equal<Required<BCSyncDetail.externID>>,
								And<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>>>>>>
						.Select(this, _processor.Operation.ConnectorType, _processor.Operation.Binding, BCEntitiesAttribute.StockItem, BCEntitiesAttribute.NonStockItem, BCEntitiesAttribute.ProductWithVariant, productID, variantID, sku);

			}
			if (item == null) //Serch by SKU
			{
				item = PXSelect<PX.Objects.IN.InventoryItem,
							Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>
								.Select(this, string.IsNullOrEmpty(sku) ? description : sku);
			}

			string[] unacceptableStatuses = { InventoryItemStatus.Inactive, InventoryItemStatus.MarkedForDeletion, InventoryItemStatus.NoSales };

			if (item != null && (item.ItemStatus.IsIn(unacceptableStatuses)))
			{
				throw new PXException(BCMessages.InventoryInvalidStatus, string.IsNullOrEmpty(sku) ? description : sku, Processor.GetBinding().BindingName, item.InventoryCD.Trim());
			}


			if (item == null && (sku != null || description != null)) //Search by cross references
			{
				List<InventoryItem> matchActiveItems = new List<InventoryItem>();
				List<InventoryItem> matchOtherItems = new List<InventoryItem>();
				PX.Objects.IN.InventoryItem itemCandidate = null;
				PX.Objects.IN.INItemXRef crossrefCandidate = null;
				foreach (PXResult<PX.Objects.IN.INItemXRef, PX.Objects.IN.InventoryItem> result in PXSelectJoin<PX.Objects.IN.INItemXRef,
					InnerJoin<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.INItemXRef.inventoryID, Equal<PX.Objects.IN.InventoryItem.inventoryID>>>,
					Where<PX.Objects.IN.INItemXRef.alternateType, Equal<INAlternateType.global>,
						And<PX.Objects.IN.INItemXRef.alternateID, Equal<Required<PX.Objects.IN.INItemXRef.alternateID>>>>>.Select(this, string.IsNullOrEmpty(sku) ? description : sku))
				{
					var currentItem = result.GetItem<PX.Objects.IN.InventoryItem>();

					if (currentItem.ItemStatus.IsIn(unacceptableStatuses))
					{
						matchOtherItems.Add(currentItem);
					}
					else
					{
						matchActiveItems.Add(currentItem);

						if (matchActiveItems.Count == 1)
						{
							crossrefCandidate = result.GetItem<PX.Objects.IN.INItemXRef>();
							itemCandidate = currentItem;
						}
					}

				}

				if (matchActiveItems.Count > 1)
					throw new PXException(BCMessages.MultipleItemsSameCrossRefSP, sku, Processor.GetBinding().BindingName,
						string.Join(", ", matchActiveItems.Select(x => x.InventoryCD.Trim())));
				else if (matchActiveItems.Count == 0)
				{
					if (matchOtherItems.Count == 1)
						throw new PXException(BCMessages.SingleItemOtherStatusCrossRefSP, sku, Processor.GetBinding().BindingName,
							string.Join(", ", matchOtherItems.Select(x => x.InventoryCD.Trim())));
					else if (matchOtherItems.Count > 1)
						throw new PXException(BCMessages.MultipleOtherItemsSameCrossRefSP, sku, Processor.GetBinding().BindingName,
							string.Join(", ", matchOtherItems.Select(x => x.InventoryCD.Trim())));
				}

				item = itemCandidate;
				priorityUOM = crossrefCandidate?.UOM;
				alternateID = crossrefCandidate?.AlternateID;
			}

			if (item == null)
				throw new PXException(BCMessages.InventoryNotFound, string.IsNullOrEmpty(sku) ? description : sku, Processor.GetBinding().BindingName);

			uom = priorityUOM ?? item?.SalesUnit?.Trim();
			itemStatus = item.ItemStatus?.Trim();
			stkItem = item.StkItem;
			nonStockShip = item.NonStockShip;

			return item?.InventoryCD?.Trim();
		}

		public virtual String GetProductExternIDByProductCD(String inventoryCD, out string uom)
		{
			uom = null;
			string productID = null, variantID = null;
			BCBinding binding = _processor.GetBinding();
			foreach (PXResult status in PXSelectJoin<BCSyncStatus,
						InnerJoin<BCSyncDetail, On<BCSyncDetail.syncID, Equal<BCSyncStatus.syncID>>,
						InnerJoin<PX.Objects.IN.InventoryItem, On<PX.Objects.IN.InventoryItem.noteID, Equal<BCSyncDetail.localID>>>>,
						Where<BCSyncStatus.connectorType, Equal<Required<BCSyncStatus.connectorType>>,
							And<BCSyncStatus.bindingID, Equal<Required<BCSyncStatus.bindingID>>,
							And<BCSyncDetail.entityType, Equal<Required<BCSyncDetail.entityType>>,
							And<PX.Objects.IN.InventoryItem.inventoryCD, Equal<Required<PX.Objects.IN.InventoryItem.inventoryCD>>>>>>>
						.Select(this, binding.ConnectorType, binding.BindingID, BCEntitiesAttribute.Variant, inventoryCD))
			{
				PX.Objects.IN.InventoryItem item = status.GetItem<PX.Objects.IN.InventoryItem>();
				BCSyncDetail variantStatus = status.GetItem<BCSyncDetail>();
				BCSyncStatus parentStatus = status.GetItem<BCSyncStatus>();

				if (variantStatus.ExternID == null) throw new PXException(BCMessages.InvenotryNotSyncronized, inventoryCD);
				if (item?.ItemStatus == PX.Objects.IN.INItemStatus.Inactive) throw new PXException(BCMessages.InvenotryInactive, inventoryCD);

				uom = item?.SalesUnit?.Trim();

				if (variantStatus?.ExternID != null && parentStatus?.ExternID != null)
				{
					variantID = variantStatus.ExternID;
					productID = parentStatus.ExternID;
				}
			}

			if (productID == null)
			{
				var ExternProductVariantData = PXContext.GetSlot<List<ProductVariantData>>(nameof(ProductVariantData));
				var matchedFirstVariant = ExternProductVariantData?.FirstOrDefault(x => string.Equals(x.Sku, inventoryCD, StringComparison.OrdinalIgnoreCase));
				if (matchedFirstVariant != null)
				{
					productID = matchedFirstVariant.ProductId.ToString();
					variantID = matchedFirstVariant.Id.ToString();
				}
			}

			if (productID == null || variantID == null) throw new PXException(BCMessages.InvenotryNotSyncronized, inventoryCD);

			return new object[] { productID, variantID }.KeyCombine();
		}

		#endregion

		#region Taxes

		/// <summary>
		/// Checks tax ID substitution list and returns the substituted tax name if found.
		/// </summary>
		/// <param name="bindingExt">The binding of the store.</param>
		/// <param name="taxID">The tax ID to substitute.</param>
		/// <param name="countryCode">An optional country code to append to the tax name before substituting.</param>
		/// <param name="provinceCode">An optional province code to append to the tax name before substituting</param>
		/// <returns>The substituted tax ID if found or the original tax ID if not.</returns>
		public virtual string SubstituteTaxName(BCBindingExt bindingExt, string taxID, string countryCode = null, string provinceCode = null)
		{
			string taxNameWithLocation = taxID + (countryCode ?? string.Empty) + (provinceCode ?? String.Empty);
			string mappedTaxName = null;
			//Check substituion list for taxCodeWithLocation
			mappedTaxName = GetSubstituteLocalByExtern(bindingExt.TaxSubstitutionListID, taxNameWithLocation, null);
			if (mappedTaxName is null)
			{
				//If not found check taxCodes for taxCodeWithLocation
				TaxCodes.TryGetValue(taxNameWithLocation, out mappedTaxName);
			}
			if (mappedTaxName is null)
			{
				//If not found check substitution list for taxName
				mappedTaxName = GetSubstituteLocalByExtern(bindingExt.TaxSubstitutionListID, taxID, null);
			}
			if (mappedTaxName is null)
			{
				//if not found just use tax name
				mappedTaxName = taxID;
			}

			//Trim found tax name
			mappedTaxName = TrimAutomaticTaxNameForAvalara(mappedTaxName);
			//check substitution list for trimmed tax name, otherwise use trimmed tax name
			mappedTaxName = GetSubstituteLocalByExtern(bindingExt.TaxSubstitutionListID, mappedTaxName, mappedTaxName);

			return mappedTaxName;
		}

		#endregion

		#region Payment 
		public virtual BCPaymentMethods GetPaymentMethodMapping(string gateway, string currency, out string cashAccount, bool throwError = true)
		{
			cashAccount = null;
			BCPaymentMethods result = null;
			if (!PaymentMethods().Any(x =>
				string.Equals(currency, x.StoreCurrency, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(x.StorePaymentMethod, gateway, StringComparison.OrdinalIgnoreCase)))
			{
				PXCache cache = base.Caches[typeof(BCPaymentMethods)];
				BCPaymentMethods newMapping = new BCPaymentMethods()
				{
					BindingID = _processor.Operation.Binding,
					StoreCurrency = currency,
					StorePaymentMethod = gateway.ToUpper(),
					Active = true,
				};
				newMapping = (BCPaymentMethods)cache.Insert(newMapping);
				cache.Persist(newMapping, PXDBOperation.Insert);
				if (throwError)
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, gateway?.ToUpper(), null, currency);
			}
			else if (!PaymentMethods().Any(x =>
				string.Equals(currency, x.StoreCurrency, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(x.StorePaymentMethod, gateway, StringComparison.OrdinalIgnoreCase) && x.Active == true))
			{
				if (throwError)// note this parameter is passed as false for refunds
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, gateway?.ToUpper(), null, currency);
				return null;
			}
			else
			{
				result = PaymentMethods().FirstOrDefault(x =>
					string.Equals(currency, x.StoreCurrency, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(x.StorePaymentMethod, gateway, StringComparison.OrdinalIgnoreCase) && x.Active == true);

				CashAccount ca = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, result?.CashAccountID);

				cashAccount = ca?.CashAccountCD;

				if (cashAccount == null || result?.PaymentMethodID == null)
				{
					throw new PXException(BCMessages.OrderPaymentMethodIsMissing, gateway?.ToUpper(), null, currency);
				}
				else if (result.StorePaymentMethod == BCConstants.NoneGateway)
				{
					PX.Objects.CA.PaymentMethod pm = PX.Objects.CA.PaymentMethod.PK.Find(this, result.PaymentMethodID);

					if (pm?.PaymentType != PX.Objects.CA.PaymentMethodType.CashOrCheck)
						throw new PXException(BCMessages.NoneGatewayWithoutCashAccount);
				}
				IsCreditCardPaymentMethodValid(result);
			}
			return result;

		}

		public virtual string ParseTransactionNumber(OrderTransaction tran, out bool isCreditCardTran)
		{
			String paymentRef = tran?.Authorization;
			isCreditCardTran = tran?.Authorization != null;
			if (tran == null) return paymentRef;

			if (!String.IsNullOrWhiteSpace(paymentRef) && paymentRef.IndexOf("#") >= 0)
				paymentRef = paymentRef.Substring(0, paymentRef.IndexOf("#"));
			if (String.Equals(tran.Gateway, ShopifyConstants.Bogus, StringComparison.OrdinalIgnoreCase))// only for bogus gateway as transaction id is always same
			{
				paymentRef = $"{tran.Id}#{tran.Authorization ?? string.Empty}";
				isCreditCardTran = false;
			}
			if (String.Equals(tran.Gateway, ShopifyConstants.ShopifyPayments, StringComparison.OrdinalIgnoreCase))//Shopify Payments use shopify ID as primary transaction ID
			{
				paymentRef = tran.Id.ToString();
				isCreditCardTran = true;
			}

			if (String.IsNullOrEmpty(paymentRef))
				paymentRef = tran.Id.ToString();

			return paymentRef;
		}

		public virtual void AddCreditCardProcessingInfo(BCPaymentMethods methodMapping, Payment payment, string transactionType)
		{
			payment.IsNewCard = true.ValueField();
			payment.SaveCard = false.ValueField();
			payment.ProcessingCenterID = methodMapping?.ProcessingCenterID.ValueField();
			CreditCardTransactionDetail detail = new CreditCardTransactionDetail();
			detail.TranNbr = payment.PaymentRef;
			detail.TranDate = payment.ApplicationDate;
			detail.TranType = GetTransactionType(transactionType);

			payment.CreditCardTransactionInfo = new List<CreditCardTransactionDetail>(new[] { detail });
		}

		public virtual StringValue GetTransactionType(string transactionType)
		{
			switch (transactionType)
			{
				case TransactionType.EmvAuthorization:
				case TransactionType.Authorization:
					return CCTranTypeCode.Authorize.ValueField();
				case TransactionType.Capture:
					return CCTranTypeCode.PriorAuthorizedCapture.ValueField();
				case TransactionType.Sale:
					return CCTranTypeCode.AuthorizeAndCapture.ValueField();
				case TransactionType.Refund:
					return CCTranTypeCode.Credit.ValueField();
				case TransactionType.Void:
					return CCTranTypeCode.VoidTran.ValueField();
				default:
					return CCTranTypeCode.Unknown.ValueField();
			}
		}

		/// <summary>		
		/// Given one transaction <paramref name="data"/> in a list of <paramref name="transactions"/>
		/// this method returns the kind (Authorization, Capture, Sale.....) of the last successful transaction related to <paramref name="data"/> transaction.
		/// Using the last transaction in the chain:
		/// Populates the Amount of <paramref name="data"/> with the amount of the last successful transaction
		/// Populates the Action of <paramref name="data"/> with the last successful transaction kind 
		/// </summary>
		/// <param name="transactions"></param>
		/// <param name="data"></param>
		/// <returns>the kind of the last transaction in the list</returns>
		public virtual string PopulateAction(List<OrderTransaction> transactions, OrderTransaction data)
		{
			var lastTrans = transactions.LastOrDefault(x => x.ParentId == data.Id && x.Status == TransactionStatus.Success
		   				 && (x.Kind == TransactionType.Authorization || x.Kind == TransactionType.EmvAuthorization ||
							 x.Kind == TransactionType.Capture || x.Kind == TransactionType.Sale));

			var lastKind = data.Action = lastTrans?.Kind ?? data.Kind;
			data.Amount = lastTrans?.Amount ?? data.Amount;
			var voidTrans = transactions.FirstOrDefault(x => x.ParentId == data.Id && x.Status == TransactionStatus.Success && x.Kind == TransactionType.Void);
			if (voidTrans != null && (lastKind == TransactionType.Authorization || lastKind == TransactionType.EmvAuthorization))
			{
				data.Action = voidTrans.Kind;
			}

			return lastKind;
		}

		public virtual string GetGatewayDescr(OrderTransaction payment)
		{
			string gateWay = payment.Gateway.ReplaceEmptyString(BCConstants.NoneGateway);
			if (String.Equals(payment.Gateway, ShopifyConstants.GiftCard, StringComparison.OrdinalIgnoreCase) && payment.Receipt is Newtonsoft.Json.Linq.JObject)
			{
				Newtonsoft.Json.Linq.JObject receipt = (Newtonsoft.Json.Linq.JObject)payment.Receipt;
				string stringValue = receipt.ContainsKey(ShopifyConstants.GiftCardID) ? (string)receipt.GetValue(ShopifyConstants.GiftCardID) : null;
					if (!String.IsNullOrEmpty(stringValue)) gateWay += " #" + stringValue;
					else
					{
						stringValue = receipt.ContainsKey(ShopifyConstants.GiftCardLastCharacters) ? PXMessages.LocalizeFormat(ShopifyMessages.GiftcardGateway, ShopifyConstants.GiftCard, receipt.GetValue(ShopifyConstants.GiftCardLastCharacters)) : String.Empty;
						return String.IsNullOrEmpty(stringValue) ? gateWay : stringValue;
					}
			}
			return gateWay;
		}

		#endregion

		#region Filter
		public virtual void SetFilterMinDate(FilterOrders filter, DateTime? minDateTime, DateTime? syncOrdersFrom, int delaySecs)
		{
			if (minDateTime != null && (syncOrdersFrom == null || minDateTime > syncOrdersFrom))
			{
				filter.UpdatedAtMin = minDateTime.Value.ToLocalTime().AddSeconds(delaySecs);
			}
			else if (syncOrdersFrom != null)
			{
				filter.CreatedAtMin = syncOrdersFrom.Value.ToLocalTime().AddSeconds(delaySecs);
			}
		}

		public virtual string PrepareDateFilter(DateTime? minDateTime, DateTime? maxDateTime, DateTime? syncOrdersFrom, int delaySecs = 0)
		{
			//prepare filters
			var filtersList = new List<string>();
			if (minDateTime != null && (syncOrdersFrom == null || minDateTime > syncOrdersFrom))
			{
				filtersList.Add($"updated_at:>='{minDateTime.Value.ToLocalTime().AddSeconds(delaySecs):o}'");
			}
			else if (syncOrdersFrom != null)
			{
				filtersList.Add($"created_at:>='{syncOrdersFrom.Value.ToLocalTime().AddSeconds(delaySecs):o}'");
			}

			if (maxDateTime != null)
			{
				filtersList.Add($"updated_at:<='{maxDateTime.Value.ToLocalTime():o}'");
			}

			return filtersList.Any() ? string.Join(" AND ", filtersList) : null;
		}
		#endregion

		#region Utilities
		/// <summary>
		/// Returns the Shopify API default version.
		/// </summary>
		/// <remarks>If you want to use different API version, please overwrite in Web.Config or your detail DataProvider.</remarks>
		/// <returns>Shopify API Version.</returns>
		static public string GetAPIDefaultVersion()
		{
			return WebConfig.GetString(ShopifyConstants.ShopifyAPIVersion, ShopifyConstants.ApiVersion_202310);
		} 
		#endregion
	}
}
