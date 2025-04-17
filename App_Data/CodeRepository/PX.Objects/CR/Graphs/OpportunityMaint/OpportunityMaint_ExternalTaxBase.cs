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
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;

namespace PX.Objects.CR
{
	public class OpportunityMaintExternalTax : ExternalTaxBase<OpportunityMaint, CROpportunity>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		[PXOverride]
		public void Persist()
		{
			if (Base.Opportunity.Current != null && IsExternalTax(Base.Opportunity.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Opportunity.Current.IsTaxValid != true)
			{
				if (!PXLongOperation.IsLongOperationContext())
				{
					var graph = Base.CloneGraphState();
					var ext = graph.GetProcessingExtension<OpportunityMaintExternalTax>();

					PXLongOperation.StartOperation(Base, delegate ()
					{
						ext.CalculateExternalTax(graph.Opportunity.Current);
					});
				}
				else
				{
					CalculateExternalTax(Base.Opportunity.Current);
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

		#region CROpportunity Events
		protected virtual void _(Events.RowSelected<CROpportunity> e)
		{
			if (e.Row == null)
				return;

			if (IsExternalTax(e.Row.TaxZoneID) && e.Row.IsTaxValid != true)
				PXUIFieldAttribute.SetWarning<CROpportunity.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
		}
		#endregion

		#region CROpportunityProducts Events
		protected virtual void _(Events.RowInserted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}

		protected virtual void _(Events.RowDeleted<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowUpdated<CROpportunityProducts> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}

		protected virtual void _(Events.RowUpdated<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowDeleted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		protected virtual void _(Events.RowInserted<CROpportunityDiscountDetail> e)
		{
			InvalidateExternalTax(Base.Opportunity.Current);
		}
		#endregion

		#region CRShippingAddress Events

		protected virtual void _(Events.RowUpdated<CRShippingAddress> e)
		{
			if (e.Row != null && e.Cache.ObjectsEqual<CRShippingAddress.postalCode, CRShippingAddress.countryID,
				CRShippingAddress.state, CRShippingAddress.latitude, CRShippingAddress.longitude>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		protected virtual void _(Events.RowInserted<CRShippingAddress> e)
		{
			if (e.Row != null && Base.Opportunity.Current != null)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<CRShippingAddress> e)
		{
			if (e.Row != null && Base.Opportunity.Current != null)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		protected virtual void _(Events.FieldUpdated<CRShippingAddress.overrideAddress> e)
		{
			if (e.Row != null && Base.Opportunity.Current != null)
			{
				InvalidateExternalTax(Base.Opportunity.Current);
			}
		}

		#endregion

		public virtual void InvalidateExternalTax(CROpportunity doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Opportunity.Cache.MarkUpdated(doc);
			}
		}

		public override CROpportunity CalculateExternalTax(CROpportunity order)
		{
			var toAddress = GetToAddress(order);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable || order.BAccountID == null)
			{
				ApplyTax(order, GetTaxResult.Empty);
				order.IsTaxValid = true;
				order = Base.Opportunity.Update(order);
				
				SkipTaxCalcAndSave();

				return order;
			}

			var service = TaxProviderFactory(Base, order.TaxZoneID);

			GetTaxRequest getRequest = null;
			bool isValidByDefault = true;

			if (order.IsTaxValid != true)
			{
				getRequest = BuildGetTaxRequest(order);

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
				order.IsTaxValid = true;
				order = Base.Opportunity.Update(order);
				SkipTaxCalcAndSave();
				
				return order;
			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.IsSuccess)
			{
				try
				{
					ApplyTax(order, result);

					order.IsTaxValid = true;
					order = Base.Opportunity.Update(order);
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

			return order;
		}

		protected virtual GetTaxRequest BuildGetTaxRequest(CROpportunity order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			BAccount cust = (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(Base, order.BAccountID);
			Location loc = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(Base, order.BAccountID, order.LocationID);
			TaxZone taxZone = (TaxZone)PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<CROpportunity.taxZoneID>>>>.Select(Base, order.TaxZoneID);

			IAddressLocation addressFrom = GetFromAddress(order);
			IAddressLocation addressTo = GetToAddress(order);

			if (addressFrom == null)
				throw new PXException(Messages.FailedGetFromAddressCR);

			if (addressTo == null)
				throw new PXException(Messages.FailedGetToAddressCR);

			int mult = 1;

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, Base.Accessinfo.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = cust?.AcctCD;
			request.BAccountClassID = cust?.ClassID;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(addressFrom);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(addressTo);
			request.DocCode = $"CR.{order.OpportunityID}";
			request.DocDate = order.CloseDate.GetValueOrDefault();
			request.Discount = mult * order.CuryLineDocDiscountTotal.GetValueOrDefault();
			request.APTaxType = taxZone.ExternalAPTaxType;

			request.CustomerUsageType = order.AvalaraCustomerUsageType;
			request.ExemptionNo = order.ExternalTaxExemptionNumber;

			request.DocType = TaxDocumentType.SalesOrder;

			PXSelectBase<CROpportunityProducts> select = new PXSelectJoin<CROpportunityProducts,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<CROpportunityProducts.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<CROpportunityProducts.quoteID, Equal<Current<CROpportunity.quoteNoteID>>>,
				OrderBy<Asc<CROpportunityProducts.lineNbr>>>(Base);

			foreach (PXResult<CROpportunityProducts, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				CROpportunityProducts tran = (CROpportunityProducts)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				bool lineIsDiscounted = request.Discount != 0m &&
					((tran.DocumentDiscountRate ?? 1m) != 1m || (tran.GroupDiscountRate ?? 1m) != 1m);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.UnitPrice = mult * tran.CuryUnitPrice.GetValueOrDefault();
				line.Amount = mult * tran.CuryAmount.GetValueOrDefault();
				line.Description = tran.Descr;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
				line.UOM = tran.UOM;
				line.Discounted = lineIsDiscounted;
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

		protected virtual void CalcCuryProductsAmount(CROpportunity order, ref decimal? curyProductsAmount) { }

		protected virtual void ApplyTax(CROpportunity order, GetTaxResult result)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;

			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)PXSetup<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<CROpportunity.taxZoneID>>>>.Select(Base, order.TaxZoneID);
				vendor = GetTaxAgency(Base, taxZone);
			}

			//Clear all existing Tax transactions:
			foreach (PXResult<CRTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
			{
				CRTaxTran taxTran = (CRTaxTran)res;
				Base.Taxes.Delete(taxTran);
			}

			Base.Views.Caches.Add(typeof(Tax));

			TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<CROpportunityProducts.taxCategoryID>(Base.Products.Cache, null);
			var row = Base.OpportunityCurrent.Cache.GetExtension<PX.Objects.Extensions.SalesTax.Document>(Base.OpportunityCurrent.Current);
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
					taxTran.QuoteID = order.QuoteNoteID;
					taxTran.TaxID = tax?.TaxID;
					taxTran.LineNbr = i + 1;
					taxTran.CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount);
					taxTran.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount);
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

			decimal? CuryProductsAmount =
				order.ManualTotalEntry == true
				? order.CuryAmount - order.CuryDiscTot
				: order.CuryLineTotal - order.CuryDiscTot + order.CuryTaxTotal;

			CalcCuryProductsAmount(order, ref CuryProductsAmount);
		}

		[Obsolete]
		protected IAddressLocation GetFromAddress()
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccount, Address> res in select.Select(Base.Accessinfo.BranchID))
				return (Address)res;

			return null;
		}

		protected IAddressLocation GetFromAddress(CROpportunity order)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccount, Address> res in select.Select(order.BranchID))
				return (Address)res;

			return null;
		}

		protected IAddressLocation GetToAddress(CROpportunity order)
		{
			var crShipAddress = (CRShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { order });

			if (crShipAddress != null)
				return crShipAddress;

			Address shipAddress = null;

			Location loc = (Location)PXSelect<Location,
				Where<Location.bAccountID, Equal<Required<Location.bAccountID>>, And<Location.locationID, Equal<Required<Location.locationID>>>>>.
				Select(Base, order.BAccountID, order.LocationID);
			if (loc != null)
			{
				shipAddress = PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, loc.DefAddressID);
			}

			return shipAddress;
		}
	}
}
