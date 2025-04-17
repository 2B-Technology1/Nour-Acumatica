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
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;
using PX.Objects.CR;
using PX.Data.BQL;

namespace PX.Objects.PM
{
	public class PMQuoteMaintExternalTax : ExternalTaxBase<PMQuoteMaint, PMQuote>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		[PXOverride]
		public void Persist()
		{
			if (Base.Quote.Current != null && IsExternalTax(Base.Quote.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Quote.Current.IsTaxValid != true)
			{
				if (PXLongOperation.GetCurrentItem() == null)
				{
					PXLongOperation.StartOperation(Base, delegate ()
					{
						CalculateExternalTax(Base.Quote.Current);
					});
				}
				else
				{
					CalculateExternalTax(Base.Quote.Current);
				}
			}
		}

		public override void SkipTaxCalcAndSave()
		{
			try
			{
				skipExternalTaxCalcOnSave = true;
				Base.Save.Press();
			}
			finally
			{
				skipExternalTaxCalcOnSave = false;
			}
		}

		protected virtual void _(Events.RowUpdated<PMQuote> e)
		{
			if (IsExternalTax(e.Row.TaxZoneID) && !e.Cache.ObjectsEqual<PMQuote.contactID, PMQuote.taxZoneID, PMQuote.branchID, PMQuote.locationID,
					PMQuote.curyAmount, PMQuote.shipAddressID>(e.Row, e.OldRow))
			{
				e.Row.IsTaxValid = false;
			}
		}

		protected virtual void _(Events.RowInserted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}
		protected virtual void _(Events.RowUpdated<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}
		protected virtual void _(Events.RowDeleted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}

		protected virtual void _(Events.RowInserted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}
		protected virtual void _(Events.RowUpdated<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}
		protected virtual void _(Events.RowDeleted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Quote.Current);
		}

		#region CRShippingAddress Events

		protected virtual void _(Events.RowUpdated<CRShippingAddress> e)
		{
			if (e.Row != null && e.Cache.ObjectsEqual<CRShippingAddress.postalCode, CRShippingAddress.countryID,
				CRShippingAddress.state, CRShippingAddress.latitude, CRShippingAddress.longitude>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Quote.Current);
			}
		}

		protected virtual void _(Events.RowInserted<CRShippingAddress> e)
		{
			if (e.Row != null)
			{
				InvalidateExternalTax(Base.Quote.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<CRShippingAddress> e)
		{
			if (e.Row != null)
			{
				InvalidateExternalTax(Base.Quote.Current);
			}
		}

		#endregion

		private void InvalidateExternalTax(PMQuote quote)
		{
			if (IsExternalTax(quote.TaxZoneID))
			{
				quote.IsTaxValid = false;
				Base.Quote.Cache.MarkUpdated(quote, assertError: true);
			}
		}

		public override PMQuote CalculateExternalTax(PMQuote quote)
		{
			var toAddress = GetToAddress(quote);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable)
			{
				ApplyTax(quote, GetTaxResult.Empty);
				quote.IsTaxValid = true;
				quote = Base.Quote.Update(quote);

				SkipTaxCalcAndSave();

				return quote;
			}

			var service = TaxProviderFactory(Base, quote.TaxZoneID);

			GetTaxRequest getRequest = null;
			bool isValidByDefault = true;

			if (quote.IsTaxValid != true)
			{
				getRequest = BuildGetTaxRequest(quote);

				if (getRequest.CartItems.Count > 0)
				{
					isValidByDefault = false;
				}
				else
				{
					getRequest = null;
				}
			}

			if (isValidByDefault)
			{
				quote.IsTaxValid = true;
				quote = Base.Quote.Update(quote);
				SkipTaxCalcAndSave();
				return quote;
			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.IsSuccess)
			{
				try
				{
					ApplyTax(quote, result);
					quote.IsTaxValid = true;
					quote = Base.Quote.Update(quote);
					SkipTaxCalcAndSave();
				}
				catch (PXOuterException ex)
				{
					string msg = TX.Messages.FailedToApplyTaxes;
					foreach (string err in ex.InnerMessages)
					{
						msg += Environment.NewLine + err;
					}

					throw new PXException(ex, msg);
				}
				catch (Exception ex)
				{
					throw new PXException(ex, TX.Messages.FailedToApplyTaxes);
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}

			return quote;
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(PMQuote quote)
		{
			if (quote == null)
				throw new PXArgumentException(nameof(quote));

			BAccount cust = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(Base, quote.BAccountID);
			CR.Location loc = (CR.Location)PXSelect<CR.Location,
				Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.
				Select(Base, quote.BAccountID, quote.LocationID);
			TaxZone taxZone = (TaxZone)PXSetup<TaxZone>.Where<TaxZone.taxZoneID.IsEqual<@P.AsString>>.Select(Base, quote.TaxZoneID);

			IAddressLocation addressFrom = GetFromAddress(quote);
			IAddressLocation addressTo = GetToAddress(quote);

			if (addressFrom == null)
				throw new PXException(CR.Messages.FailedGetFromAddressCR);

			if (addressTo == null)
				throw new PXException(CR.Messages.FailedGetToAddressCR);

			int mult = 1;

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(quote.TaxZoneID, Base.Accessinfo.BranchID);
			request.CurrencyCode = quote.CuryID;
			request.CustomerCode = cust?.AcctCD;
			request.BAccountClassID = cust?.ClassID;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(addressFrom);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(addressTo);
			request.DocCode = string.Format("CR.{0}", quote.OpportunityID);
			request.DocDate = quote.DocumentDate.GetValueOrDefault();
			request.Discount = mult * quote.CuryLineDocDiscountTotal.GetValueOrDefault();
			request.APTaxType = taxZone.ExternalAPTaxType;

			request.CustomerUsageType = loc?.CAvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc?.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc?.CAvalaraExemptionNumber;
			}

			request.DocType = TaxDocumentType.SalesOrder;

			var select = PXSelectJoin<CROpportunityProducts,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<CROpportunityProducts.quoteID, Equal<Required<PMQuote.quoteID>>>,
				OrderBy<Asc<CROpportunityProducts.lineNbr>>>.Select(Base, quote.QuoteID);

			foreach (PXResult<CROpportunityProducts, InventoryItem, Account> res in select)
			{
				CROpportunityProducts tran = (CROpportunityProducts)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.UnitPrice = mult * tran.CuryUnitPrice.GetValueOrDefault();
				line.Amount = mult * tran.CuryAmount.GetValueOrDefault();
				line.Description = tran.Descr;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = item.InventoryCD;
				line.Quantity = tran.Qty.GetValueOrDefault();
				line.UOM = tran.UOM;
				line.Discounted = request.Discount != 0m;
				line.RevAcct = salesAccount.AccountCD;
				line.TaxCode = tran.TaxCategoryID;
				if (!string.IsNullOrEmpty(item.HSTariffCode))
				{
					line.CommodityCode = new CommodityCode(item.CommodityCodeType, item.HSTariffCode);
				}

				request.CartItems.Add(line);
			}

			return request;
		}

		protected void ApplyTax(PMQuote quote, GetTaxResult result)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;

			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)PXSetup<TaxZone>.Where<TaxZone.taxZoneID.IsEqual<@P.AsString>>.Select(Base, quote.TaxZoneID);
				vendor = GetTaxAgency(Base, taxZone);
			}

			//Clear all existing Tax transactions:
			foreach (PXResult<CRTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { quote }))
			{
				CRTaxTran taxTran = (CRTaxTran)res;
				Base.Taxes.Delete(taxTran);
			}

			Base.Views.Caches.Add(typeof(Tax));

			TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<CROpportunityProducts.taxCategoryID>(Base.Products.Cache, null);
			var row = Base.QuoteCurrent.Cache.GetExtension<PX.Objects.Extensions.SalesTax.Document>(Base.QuoteCurrent.Current);
			TaxCalc oldRTaxCalc = (TaxCalc)row.TaxCalc;

			try
			{
				TaxBaseAttribute.SetTaxCalc<CROpportunityProducts.taxCategoryID>(Base.Products.Cache, null, TaxCalc.ManualCalc);
				row.TaxCalc = TaxCalc.ManualCalc;

				for (int i = 0; i < result.TaxSummary.Length; i++)
				{
					result.TaxSummary[i].TaxType = CSTaxType.Sales;
					Tax tax = CreateTax(Base, taxZone, vendor, result.TaxSummary[i]);
					if (tax == null)
						continue;

					CRTaxTran taxTran = new CRTaxTran();
					taxTran.QuoteID = quote.QuoteID;
					taxTran.TaxID = tax?.TaxID;
					taxTran.LineNbr = i + 1;
					taxTran.CuryTaxAmt = result.TaxSummary[i].TaxAmount;
					taxTran.CuryTaxableAmt = result.TaxSummary[i].TaxableAmount;
					taxTran.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100;
					taxTran.JurisType = result.TaxSummary[i].JurisType;
					taxTran.JurisName = result.TaxSummary[i].JurisName;
					taxTran.TaxZoneID = taxZone.TaxZoneID;

					Base.Taxes.Insert(taxTran);
				}
			}
			finally
			{
				TaxBaseAttribute.SetTaxCalc<CROpportunityProducts.taxCategoryID>(Base.Products.Cache, null, oldTaxCalc);
				row.TaxCalc = oldRTaxCalc;
			}
		}

		protected IAddressLocation GetFromAddress(PMQuote quote)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
			<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>>>,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccount, Address> res in select.Select(quote.BranchID))
				return (Address)res;

			return null;
		}

		protected IAddressLocation GetToAddress(PMQuote quote)
		{
			var crShipAddress = (CRShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { quote });

			if (crShipAddress != null)
				return crShipAddress;

			Address shipAddress = null;

			CR.Location loc = (CR.Location)PXSelect<CR.Location,
					Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.
				Select(Base, quote.BAccountID, quote.LocationID);

			if (loc != null)
			{
				shipAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, loc.DefAddressID);
			}

			return shipAddress;
		}
	}
}
