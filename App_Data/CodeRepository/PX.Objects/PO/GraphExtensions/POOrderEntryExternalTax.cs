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
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;

namespace PX.Objects.PO
{
	public class POOrderEntryExternalTax : ExternalTax<POOrderEntry, POOrder>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		public virtual bool CalculateTaxesUsingExternalProvider(string taxZoneID)
		{
			bool isImportedTaxes = Base.Document.Current != null && Base.Document.Current.ExternalTaxesImportInProgress == true;
			return IsExternalTax(Base, taxZoneID) && !isImportedTaxes;
		}

		public override POOrder CalculateExternalTax(POOrder order)
		{
			if (order != null && CalculateTaxesUsingExternalProvider(order.TaxZoneID))
			{
				var toAddress = GetToAddress(order);
				bool isNonTaxable = IsNonTaxable(toAddress);
				bool applyEmptyTax = false;

				if (isNonTaxable)
				{
					applyEmptyTax = true;
				}
				else
				{
					var res = PXSelectJoin<TaxZone, LeftJoin<TaxPlugin, On<TaxPlugin.taxPluginID, Equal<TaxZone.taxPluginID>>>,
						Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>
						.SelectSingleBound(Base, null, order.TaxZoneID).Cast<PXResult<TaxZone, TaxPlugin>>().FirstOrDefault();

					TaxZone taxZone = res;
					TaxPlugin taxPlugin = res;
					if (taxZone?.ExternalAPTaxType == ExternalAPTaxTypes.Use && taxPlugin != null && taxPlugin.PluginTypeName.Contains("PX.TaxProvider.AvalaraRest.AvalaraRestTaxProvider"))
					{
						applyEmptyTax = true;
					}
				}

				if (applyEmptyTax)
				{
					ApplyTax(order, GetTaxResult.Empty, GetTaxResult.Empty);

					return order;
				}

				var service = TaxProviderFactory(Base, order.TaxZoneID);

				GetTaxRequest getRequest = null;
				GetTaxRequest getRequestUnbilled = null;

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

				if (order.IsUnbilledTaxValid != true)
				{
					getRequestUnbilled = BuildGetTaxRequestUnbilled(order);
					if (getRequestUnbilled.CartItems.Count > 0)
					{
						isValidByDefault = false;
					}
					else
					{
						getRequestUnbilled = null;
					}
				}

				if (isValidByDefault)
				{
				using (SuppressRequireControlTotalScope(order.Hold == true))
				{
					order.IsTaxValid = true;
					order.IsUnbilledTaxValid = true;

					order = Base.Document.Update(order);
					SkipTaxCalcAndSave();
					return order;
				}
			}

				GetTaxResult result = null;
				GetTaxResult resultUnbilled = null;

				bool getTaxFailed = false;
				if (getRequest != null)
				{
					result = service.GetTax(getRequest);
					if (!result.IsSuccess)
					{
						getTaxFailed = true;
					}
				}
				if (getRequestUnbilled != null)
				{
					resultUnbilled = service.GetTax(getRequestUnbilled);
					if (!resultUnbilled.IsSuccess)
					{
						getTaxFailed = true;
					}
				}

				if (!getTaxFailed)
				{
					ApplyExternalTaxes(order, result, resultUnbilled);
				}
				else
				{
					ResultBase taxResult = result ?? resultUnbilled;
					if (taxResult != null)
						LogMessages(taxResult);

					throw new PXException(TX.Messages.FailedToGetTaxes);
				}
			}

			return order;
		}

		public virtual void ApplyExternalTaxes(POOrder order, GetTaxResult result, GetTaxResult resultUnbilled)
		{
			try
			{
				ApplyTax(order, result, resultUnbilled);
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
				string msg = TX.Messages.FailedToApplyTaxes;
				msg += Environment.NewLine + ex.Message;

				throw new PXException(ex, msg);
			}
		}

		[PXOverride]
		public virtual void Persist(Action baseImpl)
		{
			if (Base.Document.Current?.OrderType == POOrderType.DropShip
				&& CalculateTaxesUsingExternalProvider(SO.SOOrder.PK.Find(Base, Base.Document.Current.SOOrderType, Base.Document.Current.SOOrderNbr)?.TaxZoneID))
				GetToAddress(Base.Document.Current).With(ValidAddressFrom<POOrder.shipAddressID>);

			baseImpl();

			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && Base.Document.Current.IsTaxValid != true)
			{
				if (!PXLongOperation.IsLongOperationContext())
				{
					var doc = new POOrder
					{
						OrderType = Base.Document.Current.OrderType,
						OrderNbr = Base.Document.Current.OrderNbr,
					};
					PXLongOperation.StartOperation(Base, delegate ()
					{
						POExternalTaxCalc.Process(doc);
					});
				}
				else
				{
					CalculateExternalTax(Base.Document.Current);
				}
			}
		}

		protected virtual void _(Events.RowSelected<POOrder> e)
		{
			if (e.Row == null)
				return;

			if (CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID) && e.Row.IsTaxValid != true)
			{
				PXUIFieldAttribute.SetWarning<POOrder.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
			}
		}

		protected virtual void _(Events.RowUpdated<POOrder> e)
		{
			if (e.Row == null)
				return;

			if (CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID))
			{
				if (e.Cache.ObjectsEqual<POOrder.shipDestType,
					POOrder.shipToBAccountID,
					POOrder.shipToLocationID,
					POOrder.branchID>(e.Row, e.OldRow) == false)
				{
					InvalidateExternalTax(e.Row);
				}
			}
		}

		protected virtual void _(Events.RowInserted<POLine> e)
		{
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				Base.Document.Current.IsTaxValid = false;
				Base.Document.Update(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowUpdated<POLine> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID) &&
			    !e.Cache.ObjectsEqual<POLine.inventoryID, POLine.tranDesc, POLine.extCost, POLine.promisedDate, POLine.taxCategoryID>(e.Row, e.OldRow))
			{
				Base.Document.Current.IsTaxValid = false;
				Base.Document.Update(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<POLine> e)
		{
			if (Base.Document.Current != null && CalculateTaxesUsingExternalProvider(Base.Document.Current.TaxZoneID))
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#region POShipAddress handlers
		protected virtual void _(Events.RowUpdated<POShipAddress> e)
		{
			if (e.Row == null) return;
			if (e.Cache.ObjectsEqual<POShipAddress.postalCode, POShipAddress.countryID, POShipAddress.state>(e.Row, e.OldRow) == false)
				InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowInserted<POShipAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.RowDeleted<POShipAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}

		protected virtual void _(Events.FieldUpdating<POShipAddress, POShipAddress.overrideAddress> e)
		{
			if (e.Row == null) return;
			InvalidateExternalTax(Base.Document.Current);
		}
		#endregion

		protected virtual GetTaxRequest BuildGetTaxRequest(POOrder order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { order });
			Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { order });
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { order });

			IAddressLocation fromAddress = GetFromAddress(order);
			IAddressLocation toAddress = GetToAddress(order);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddressSO);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddressSO);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = vend.AcctCD;
			request.BAccountClassID = vend.ClassID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"PO.{order.OrderType}.{order.OrderNbr}";
			request.DocDate = order.ExpectedDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(order);
			request.APTaxType = taxZone.ExternalAPTaxType;

			int mult = 1;

			request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}

			request.DocType = TaxDocumentType.PurchaseOrder;

			PXSelectBase<POLine> select = new PXSelectJoin<POLine,
				LeftJoin<InventoryItem, On<POLine.FK.InventoryItem>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<POLine.orderType, Equal<Current<POOrder.orderType>>, And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLine.lineNbr>>>(Base);

			request.Discount = mult * order.CuryDiscTot.GetValueOrDefault();

			foreach (PXResult<POLine, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				POLine tran = (POLine)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;
				bool lineIsDiscounted = request.Discount != 0m &&
					((tran.DocumentDiscountRate ?? 1m) != 1m || (tran.GroupDiscountRate ?? 1m) != 1m);

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.UnitPrice = mult * tran.CuryUnitCost.GetValueOrDefault();
				line.Amount = mult * tran.CuryExtCost.GetValueOrDefault();
				line.Description = tran.TranDesc;
				line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
				line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.OrderQty.GetValueOrDefault());
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

		protected virtual GetTaxRequest BuildGetTaxRequestUnbilled(POOrder order)
		{
			if (order == null)
				throw new PXArgumentException(nameof(order));

			Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { order });
			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { order });
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { order });

			IAddressLocation fromAddress = GetFromAddress(order);
			IAddressLocation toAddress = GetToAddress(order);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFromAddressSO);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetToAddressSO);

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
			request.CurrencyCode = order.CuryID;
			request.CustomerCode = vend.AcctCD;
			request.BAccountClassID = vend.ClassID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = string.Format("PO.{0}.{1}", order.OrderType, order.OrderNbr);
			request.DocDate = order.OrderDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(order);
			request.Discount = 0m;
			request.APTaxType = taxZone.ExternalAPTaxType;

			int mult = 1;

			request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
			if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
			{
				request.ExemptionNo = loc.CAvalaraExemptionNumber;
			}


			request.DocType = TaxDocumentType.PurchaseOrder;

			PXSelectBase<POLine> select = new PXSelectJoin<POLine,
				LeftJoin<InventoryItem, On<POLine.FK.InventoryItem>,
					LeftJoin<Account, On<Account.accountID, Equal<InventoryItem.salesAcctID>>>>,
				Where<POLine.orderType, Equal<Current<POOrder.orderType>>, And<POLine.orderNbr, Equal<Current<POOrder.orderNbr>>>>,
				OrderBy<Asc<POLine.lineNbr>>>(Base);


			foreach (PXResult<POLine, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
			{
				POLine tran = (POLine)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				if (tran.UnbilledAmt > 0)
				{
					var line = new TaxCartItem();
					line.Index = tran.LineNbr ?? 0;
					line.UnitPrice = mult * tran.CuryUnitCost.GetValueOrDefault();
					line.Amount = mult * tran.CuryUnbilledAmt.GetValueOrDefault();
					line.Description = tran.TranDesc;
					line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
					line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
					line.ItemCode = item.InventoryCD;
					line.Quantity = tran.BaseUnbilledQty.GetValueOrDefault();
					line.UOM = tran.UOM;
					line.Discounted = false;
					line.RevAcct = salesAccount.AccountCD;

					line.TaxCode = tran.TaxCategoryID;
					if (!string.IsNullOrEmpty(item.HSTariffCode))
					{
						line.CommodityCode = new CommodityCode(item.CommodityCodeType, item.HSTariffCode);
					}

					request.CartItems.Add(line);
				}
			}

			return request;
		}

		protected virtual void ApplyTax(POOrder order, GetTaxResult result, GetTaxResult resultUnbilled)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;
			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { order });
				vendor = GetTaxAgency(Base, taxZone);
			}
			//Clear all existing Tax transactions:
			foreach (PXResult<POTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
			{
				POTaxTran taxTran = (POTaxTran)res;
				Base.Taxes.Delete(taxTran);
			}

			Base.Views.Caches.Add(typeof(Tax));

			TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<POLine.taxCategoryID>(Base.Transactions.Cache, null);
			try
			{
				TaxBaseAttribute.SetTaxCalc<POLine.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

				for (int i = 0; i < result.TaxSummary.Length; i++)
				{
					result.TaxSummary[i].TaxType = CSTaxType.Sales;
					Tax tax = CreateTax(Base, taxZone, vendor, result.TaxSummary[i]);
					if (tax == null)
						continue;

					POTaxTran taxTran = new POTaxTran();
					taxTran.OrderType = order.OrderType;
					taxTran.OrderNbr = order.OrderNbr;
					taxTran.TaxID = tax?.TaxID;
					taxTran.CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount);
					taxTran.CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount);
					taxTran.TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100;
					taxTran.JurisType = result.TaxSummary[i].JurisType;
					taxTran.JurisName = result.TaxSummary[i].JurisName;

					Base.Taxes.Insert(taxTran);
				}
			}
			finally
			{
				TaxBaseAttribute.SetTaxCalc<POLine.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);
			}

			using (SuppressRequireControlTotalScope(order.Hold == true))
			{
				if (resultUnbilled != null)
					Base.Document.SetValueExt<POOrder.curyUnbilledTaxTotal>(order, Math.Abs(resultUnbilled.TotalTaxAmount));

				order.IsTaxValid = true;
				order.IsUnbilledTaxValid = true;

				order = Base.Document.Update(order);
			}

			SkipTaxCalcAndSave();
		}

		protected override string GetExternalTaxProviderLocationCode(POOrder order) => GetExternalTaxProviderLocationCode<POLine, POLine.FK.Order.SameAsCurrent, POLine.siteID>(order);

		protected virtual IAddressLocation GetToAddress(POOrder order)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket,
					POOrderType.DropShip, POOrderType.ProjectDropShip))
				return (POShipAddress)PXSelect<POShipAddress,
					Where<POShipAddress.addressID, Equal<Required<POOrder.shipAddressID>>>>
					.Select(Base, order.ShipAddressID);

			return
				PXSelectJoin<Branch,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
				Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
				.Select(Base, order.BranchID)
				.RowCast<Address>()
				.FirstOrDefault();
		}

		protected virtual IAddressLocation GetToAddress(POOrder order, POLine line)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket))
			{
				return
					PXSelectJoin<Address,
					InnerJoin<INSite, On<INSite.FK.Address>>,
					Where<INSite.siteID, Equal<Current<POLine.siteID>>>>
					.SelectSingleBound(Base, new[] { line })
					.RowCast<Address>()
					.FirstOrDefault()
					?? GetToAddress(order);
			}

			return GetToAddress(order);
		}

		protected virtual CR.Standalone.Location GetBranchLocation(POOrder order)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<CR.Standalone.Location, On<CR.Standalone.Location.bAccountID, Equal<BAccountR.bAccountID>, And<CR.Standalone.Location.locationID, Equal<BAccountR.defLocationID>>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccountR, CR.Standalone.Location> res in select.Select(order.BranchID))
				return (CR.Standalone.Location)res;

			return null;
		}

		protected virtual IAddressLocation GetFromAddress(POOrder order)
		{
			if (order.OrderType.IsIn(POOrderType.RegularOrder, POOrderType.Blanket, POOrderType.StandardBlanket,
					POOrderType.DropShip, POOrderType.ProjectDropShip))
				return (PORemitAddress)PXSelect<PORemitAddress,
					Where<PORemitAddress.addressID, Equal<Required<POOrder.remitAddressID>>>>
					.Select(Base, order.RemitAddressID);

			return (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>
				.Select(Base, Base.vendor.Current.DefAddressID);
		}

		protected virtual IAddressLocation GetFromAddress(POOrder order, POLine line)
		{
			return GetFromAddress(order);
		}


		private void InvalidateExternalTax(POOrder order)
		{
			if (order == null || !CalculateTaxesUsingExternalProvider(order.TaxZoneID)) return;
			order.IsTaxValid = false;
			order.IsUnbilledTaxValid = false;
		}

		private IAddressBase ValidAddressFrom<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			IAddressLocation addressLocation = address as IAddressLocation;
			if (!IsEmptyAddress(addressLocation))
				return address;

			throw new PXException(PickAddressError<TFieldSource>(address));
		}

		private string PickAddressError<TFieldSource>(IAddressBase address)
			where TFieldSource : IBqlField
		{
			if (typeof(TFieldSource) == typeof(POOrder.shipAddressID))
				return PXSelect<POOrder, Where<POOrder.shipAddressID, Equal<Required<Address.addressID>>>>
					.SelectWindowed(Base, 0, 1, ((POAddress)address).AddressID).First().GetItem<POOrder>()
					.With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<POOrder>(), new EntityHelper(Base).GetRowID(e)));

			throw new ArgumentOutOfRangeException("Unknown address source used");
		}

		private IDisposable SuppressRequireControlTotalScope(bool hold)
		{
			bool requireBlanketControlTotal = Base.POSetup.Current.RequireBlanketControlTotal == true;
			bool requireDropShipControlTotal = Base.POSetup.Current.RequireDropShipControlTotal == true;
			bool requireProjectDropShipControlTotal = Base.POSetup.Current.RequireProjectDropShipControlTotal == true;
			bool requireOrderControlTotal = Base.POSetup.Current.RequireOrderControlTotal == true;

			if (hold != true)
			{
				Base.POSetup.Current.RequireBlanketControlTotal = false;
				Base.POSetup.Current.RequireDropShipControlTotal = false;
				Base.POSetup.Current.RequireProjectDropShipControlTotal = false;
				Base.POSetup.Current.RequireOrderControlTotal = false;
			}

			return Disposable.Create(() =>
			{
				Base.POSetup.Current.RequireBlanketControlTotal = requireBlanketControlTotal;
				Base.POSetup.Current.RequireDropShipControlTotal = requireDropShipControlTotal;
				Base.POSetup.Current.RequireProjectDropShipControlTotal = requireProjectDropShipControlTotal;
				Base.POSetup.Current.RequireOrderControlTotal = requireOrderControlTotal;
			});
		}
	}
}
