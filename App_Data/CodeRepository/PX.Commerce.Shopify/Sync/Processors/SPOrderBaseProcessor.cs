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
using PX.Commerce.Shopify.API.REST;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Threading.Tasks;

namespace PX.Commerce.Shopify
{
	public abstract class SPOrderBaseProcessor<TGraph, TEntityBucket, TPrimaryMapped> : OrderProcessorBase<TGraph, TEntityBucket, TPrimaryMapped>
		where TGraph : PXGraph
		where TEntityBucket : class, IEntityBucket, new()
		where TPrimaryMapped : class, IMappedEntity, new()
	{

		public SPHelper helper = PXGraph.CreateInstance<SPHelper>();
		protected InventoryItem refundItem;
		protected Lazy<InventoryItem> giftCertificateItem;

		public override async Task Initialise(IConnector iconnector, ConnectorOperation operation)
		{
			await base.Initialise(iconnector, operation);

			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			refundItem = bindingExt.RefundAmountItemID != null ? PX.Objects.IN.InventoryItem.PK.Find((PXGraph)this, bindingExt.RefundAmountItemID) : throw new PXException(ShopifyMessages.NoRefundItem);
			giftCertificateItem = new Lazy<InventoryItem>(delegate { return InventoryItem.PK.Find(this, bindingExt.GiftCertificateItemID); }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
		}

		#region Refunds
		public virtual SalesOrderDetail InsertRefundAmountItem(decimal amount, StringValue branch)
		{
			decimal quantity = amount < 0m ? -1 : 1;
			BCBindingExt bindingExt = GetBindingExt<BCBindingExt>();
			SalesOrderDetail detail = new SalesOrderDetail();
			detail.InventoryID = refundItem.InventoryCD?.TrimEnd().ValueField();
			detail.OrderQty = quantity.ValueField();
			detail.UOM = refundItem.BaseUnit.ValueField();
			detail.Branch = branch;
			//Unit price is not allowed to be negative. We take the absolute value and use -1 for quantity instead.
			detail.UnitPrice = Math.Abs(amount).ValueField();
			detail.ManualPrice = true.ValueField();
			detail.ReasonCode = bindingExt?.ReasonCode?.ValueField();
			return detail;
		}

		#endregion

		[Obsolete("Starting from 2022R2 the determination of taxes names are done in the scope of helpers. Please delete this method in 2024R1.")]
		protected string DetermineTaxName(OrderData data, OrderTaxLine tax)
		{
			string TaxName = tax.TaxName;
			OrderAddressData taxAddress = data.ShippingAddress ?? data.BillingAddress ?? new OrderAddressData();
			string taxNameWithLocation = TaxName + (taxAddress.CountryCode ?? String.Empty) + (taxAddress.ProvinceCode ?? String.Empty);
			string mappedTaxName = null;
			//Check substituion list for taxCodeWithLocation
			mappedTaxName = GetHelper<SPHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, taxNameWithLocation, null);
			if (mappedTaxName is null)
			{
				//If not found check taxCodes for taxCodeWithLocation
				GetHelper<SPHelper>().TaxCodes.TryGetValue(taxNameWithLocation, out mappedTaxName);
			}
			if (mappedTaxName is null)
			{
				//If not found check substitution list for taxName
				mappedTaxName = GetHelper<SPHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, TaxName, null);
			}
			if (mappedTaxName is null)
			{
				//if not found just use tax name
				mappedTaxName = TaxName;
			}
			//Trim found tax name
			mappedTaxName = GetHelper<SPHelper>().TrimAutomaticTaxNameForAvalara(mappedTaxName);
			//check substitution list for trimmed tax name, otherwise use trimmed tax name
			mappedTaxName = GetHelper<SPHelper>().GetSubstituteLocalByExtern(GetBindingExt<BCBindingExt>().TaxSubstitutionListID, mappedTaxName, mappedTaxName);
			return mappedTaxName;
		}
	}
}
