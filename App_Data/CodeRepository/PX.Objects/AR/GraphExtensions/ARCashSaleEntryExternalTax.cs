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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AR.Standalone;
using PX.Objects.CR;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.TX.GraphExtensions;
using PX.TaxProvider;

namespace PX.Objects.AR
{
	public class ARCashSaleEntryExternalTax : ExternalTax<ARCashSaleEntry, ARCashSale>
	{
        private bool asynchronousProcess = true;
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
		}

		protected virtual void _(Events.RowUpdated<ARCashSale> e)
		{
			if (e.Row.Released != true && IsDocumentExtTaxValid(e.Row) && !e.Cache.ObjectsEqual<
				ARCashSale.externalTaxExemptionNumber,
				ARCashSale.avalaraCustomerUsageType, 
				ARCashSale.curyDiscountedTaxableTotal, 
				ARCashSale.adjDate, 
				ARCashSale.taxZoneID,
				ARCashSale.branchID>(e.Row, e.OldRow))
			{
				e.Row.IsTaxValid = false;
			}
		}

		protected virtual void _(Events.RowPersisting<ARCashSale> e)
		{
			if (e.Row.IsTaxSaved != true || e.Row.Released == true)
				return;

			//Cancel tax if document is deleted
			if (e.Operation.Command() == PXDBOperation.Delete)
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}

			//Cancel tax if last line in the document is deleted
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && !Base.Transactions.Any())
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}

			//Cancel tax if IsExternalTax has changed to False (Document was changed from External Tax Provider to Acumatica Tax Engine) or address has become NonTaxable.
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && (!IsExternalTax(e.Row.TaxZoneID) || IsNonTaxable(GetToAddress(e.Row))))
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}
		}

		protected virtual void _(Events.RowInserted<ARTran> e)
		{
			if (IsDocumentExtTaxValid(Base.Document.Current) && e.Row.Released != true)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowUpdated<ARTran> e)
		{
			//if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
			if (IsDocumentExtTaxValid(Base.Document.Current) &&
				!e.Cache.ObjectsEqual<ARTran.accountID, ARTran.inventoryID, ARTran.tranDesc, ARTran.tranAmt, ARTran.tranDate, ARTran.taxCategoryID>(e.Row, e.OldRow))
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<ARTran> e)
		{
			if (IsDocumentExtTaxValid(Base.Document.Current) && e.Row.Released != true)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#region ARShippingAddress Events

		protected virtual void _(Events.RowUpdated<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null && e.Cache.ObjectsEqual<ARShippingAddress.postalCode, ARShippingAddress.countryID,
				ARShippingAddress.state, ARShippingAddress.latitude, ARShippingAddress.longitude>(e.Row, e.OldRow) == false)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowInserted<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.RowDeleted<ARShippingAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		protected virtual void _(Events.FieldUpdating<ARShippingAddress, ARShippingAddress.overrideAddress> e)
		{
			if (e.Row != null && Base.Document.Current != null)
			{
				InvalidateExternalTax(Base.Document.Current);
			}
		}

		#endregion

		private void InvalidateExternalTax(ARCashSale doc)
		{
			if (IsExternalTax(doc.TaxZoneID))
			{
				doc.IsTaxValid = false;
				Base.Document.Cache.MarkUpdated(doc);
			}
		}

		public override ARCashSale CalculateExternalTax(ARCashSale invoice)
		{
			var toAddress = GetToAddress(invoice);
			bool isNonTaxable = IsNonTaxable(toAddress);

			if (isNonTaxable)
			{
				ApplyTax(invoice, GetTaxResult.Empty);
				invoice.IsTaxValid = true;
				invoice.NonTaxable = true;
				invoice.IsTaxSaved = false;
				invoice = Base.Document.Update(invoice);

				SkipTaxCalcAndSave();

				return invoice;
            }
            else if (invoice.NonTaxable == true)
			{
				Base.Document.SetValueExt<ARRegister.nonTaxable>(invoice, false);
			}

			var service = TaxProviderFactory(Base, invoice.TaxZoneID);

			GetTaxRequest getRequest = BuildGetTaxRequest(invoice);

			if (getRequest.CartItems.Count == 0)
			{
				ApplyTax(invoice, GetTaxResult.Empty);
				invoice.IsTaxValid = true;
				invoice.IsTaxSaved = false;
				invoice = Base.Document.Update(invoice);

				SkipTaxCalcAndSave();

				return invoice;
			}

			GetTaxResult result = service.GetTax(getRequest);
			if (result.IsSuccess)
			{
				try
				{
					ApplyTax(invoice, result);
					SkipTaxCalcAndSave();
				}
				catch (PXOuterException ex)
				{
					try
					{
						CancelTax(invoice, VoidReasonCode.Unspecified);
					}
					catch (Exception)
					{
						throw new PXException(new PXException(ex, TX.Messages.FailedToApplyTaxes), TX.Messages.FailedToCancelTaxes);
					}

					string msg = TX.Messages.FailedToApplyTaxes;
					foreach (string err in ex.InnerMessages)
					{
						msg += Environment.NewLine + err;
					}

					throw new PXException(ex, msg);
				}
				catch (Exception ex)
				{
					try
					{
						CancelTax(invoice, VoidReasonCode.Unspecified);
					}
					catch (Exception)
					{
						throw new PXException(new PXException(ex, TX.Messages.FailedToApplyTaxes), TX.Messages.FailedToCancelTaxes);
					}

					string msg = TX.Messages.FailedToApplyTaxes;
					msg += Environment.NewLine + ex.Message;

					throw new PXException(ex, msg);
				}

				PostTaxRequest request = new PostTaxRequest();
				request.CompanyCode = getRequest.CompanyCode;
				request.CustomerCode = getRequest.CustomerCode;
				request.BAccountClassID = getRequest.BAccountClassID;
				request.DocCode = getRequest.DocCode;
				request.DocDate = getRequest.DocDate;
				request.DocType = getRequest.DocType;
				request.TotalAmount = result.TotalAmount;
				request.TotalTaxAmount = result.TotalTaxAmount;
				PostTaxResult postResult = service.PostTax(request);
				if (postResult.IsSuccess)
				{
                    ARCashSale copy = PXCache<ARCashSale>.CreateCopy(invoice);
                    copy.IsTaxValid = true;
                    invoice = Base.Document.Update(copy);
					SkipTaxCalcAndSave();
				}
			}
			else
			{
				LogMessages(result);

				throw new PXException(TX.Messages.FailedToGetTaxes);
			}


			return invoice;
		}

		[PXOverride]
		public virtual void Persist()
		{
			if (Base.Document.Current != null && IsExternalTax(Base.Document.Current.TaxZoneID) && Base.Document.Current.IsTaxValid != true && !skipExternalTaxCalcOnSave)
			{
                if (PXLongOperation.GetCurrentItem() == null && asynchronousProcess)
				{
					ARCashSale currentDoc = Base.Document.Current;
					PXLongOperation.StartOperation(Base, delegate ()
					{
						ARCashSaleEntry rg = PXGraph.CreateInstance<ARCashSaleEntry>();
						rg.Document.Current = PXSelect<ARCashSale, Where<ARCashSale.docType, Equal<Required<ARCashSale.docType>>, And<ARCashSale.refNbr, Equal<Required<ARCashSale.refNbr>>>>>.Select(rg, currentDoc.DocType, currentDoc.RefNbr);
						rg.CalculateExternalTax(rg.Document.Current);
					});
				}
				else
				{
					Base.CalculateExternalTax(Base.Document.Current);
				}
			}
		}

        public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, ReleaseDelegate baseRelease)
        {
            List<object> cashSales = new List<object>();
            foreach (ARCashSale cashSale in adapter.Get<ARCashSale>())
            {
                cashSales.Add(cashSale);
            }
            asynchronousProcess = false;
            Base.Save.Press();
            asynchronousProcess = true;
            return baseRelease(new PXAdapter(new PXView.Dummy(Base, adapter.View.BqlSelect, cashSales)));
        }

		protected virtual GetTaxRequest BuildGetTaxRequest(ARCashSale invoice)
		{
			if (invoice == null) throw new PXArgumentException(nameof(invoice), ErrorMessages.ArgumentNullException);

			Customer cust = (Customer)Base.customer.View.SelectSingleBound(new object[] { invoice });
			Location loc = (Location)Base.location.View.SelectSingleBound(new object[] { invoice });
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { invoice });

			GetTaxRequest request = new GetTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(invoice.TaxZoneID, invoice.BranchID);
			request.CurrencyCode = invoice.CuryID;
			request.CustomerCode = cust.AcctCD;
			request.BAccountClassID = cust.ClassID;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.APTaxType = taxZone.ExternalAPTaxType;
			IAddressLocation fromAddress = GetFromAddress(invoice);
			IAddressLocation toAddress = GetToAddress(invoice);

			if (fromAddress == null)
				throw new PXException(Messages.FailedGetFrom);

			if (toAddress == null)
				throw new PXException(Messages.FailedGetTo);

			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
			request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
			request.DocCode = $"AR.{invoice.DocType}.{invoice.RefNbr}";
			request.DocDate = invoice.DocDate.GetValueOrDefault();
			request.LocationCode = GetExternalTaxProviderLocationCode(invoice);
			request.CustomerUsageType = invoice.AvalaraCustomerUsageType;
			request.IsTaxSaved = invoice.IsTaxSaved == true;

			if (!string.IsNullOrEmpty(invoice.ExternalTaxExemptionNumber))
			{
				request.ExemptionNo = invoice.ExternalTaxExemptionNumber;
			}

			request.DocType = GetTaxDocumentType(invoice);
			Sign sign = GetDocumentSign(invoice);

			PXSelectBase<ARTran> select = new PXSelectJoin<ARTran,
				LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<ARTran.inventoryID>>,
					LeftJoin<Account, On<Account.accountID, Equal<ARTran.accountID>>>>,
				Where<ARTran.tranType, Equal<Current<ARCashSale.docType>>,
					And<ARTran.refNbr, Equal<Current<ARCashSale.refNbr>>,
					And<Where<ARTran.lineType, NotEqual<SOLineType.discount>, Or<ARTran.lineType, IsNull>>>>>,
				OrderBy<Asc<ARTran.tranType, Asc<ARTran.refNbr, Asc<ARTran.lineNbr>>>>>(Base);

			request.Discount = sign * GetDocDiscount().GetValueOrDefault();
			DateTime? taxDate = invoice.OrigDocDate;

			foreach (PXResult<ARTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTran tran = (ARTran)res;
				InventoryItem item = (InventoryItem)res;
				Account salesAccount = (Account)res;

				var line = new TaxCartItem();
				line.Index = tran.LineNbr ?? 0;
				line.UnitPrice = sign * tran.CuryUnitPrice.GetValueOrDefault();
				line.Amount = sign * tran.CuryTranAmt.GetValueOrDefault();
				line.Description = tran.TranDesc;
				line.DestinationAddress = request.DestinationAddress;
				line.OriginAddress = request.OriginAddress;
				line.ItemCode = item.InventoryCD;
				line.Quantity = Math.Abs(tran.Qty.GetValueOrDefault());
				line.UOM = tran.UOM;
				line.Discounted = request.Discount != 0m;
				line.RevAcct = salesAccount.AccountCD;
				line.TaxCode = tran.TaxCategoryID;
				if (!string.IsNullOrEmpty(item.HSTariffCode))
				{
					line.CommodityCode = new CommodityCode(item.CommodityCodeType, item.HSTariffCode);
				}
				if (tran.OrigInvoiceDate != null)
					taxDate = tran.OrigInvoiceDate;

				request.CartItems.Add(line);
			}

			if (invoice.DocType == ARDocType.CashReturn && invoice.OrigDocDate != null)
			{
				request.TaxOverride.Reason = Messages.ReturnReason;
				request.TaxOverride.TaxDate = taxDate.Value;
				request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
			}

			return request;
		}

		public virtual TaxDocumentType GetTaxDocumentType(ARCashSale invoice)
		{
			switch (ARInvoiceType.DrCr(invoice.DocType))
			{
				case DrCr.Credit:
					return TaxDocumentType.SalesInvoice;
				case DrCr.Debit:
					return TaxDocumentType.ReturnInvoice;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		public virtual Sign GetDocumentSign(ARCashSale invoice)
		{
			switch (ARInvoiceType.DrCr(invoice.DocType))
			{
				case DrCr.Credit:
					return Sign.Plus;
				case DrCr.Debit:
					return Sign.Minus;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		protected virtual void ApplyTax(ARCashSale invoice, GetTaxResult result)
		{
			TaxZone taxZone = null;
			AP.Vendor vendor = null;
			invoice.CuryTaxTotal = 0;

			if (result.TaxSummary.Length > 0)
			{
				taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { invoice });
				vendor = GetTaxAgency(Base, taxZone, true);
			}
			//Clear all existing Tax transactions:
			foreach (PXResult<ARTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { invoice }))
			{
				ARTaxTran taxTran = (ARTaxTran)res;
				Base.Taxes.Delete(taxTran);
			}

			Base.Views.Caches.Add(typeof(Tax));

			TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null);
			try
			{
				TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

				for (int i = 0; i < result.TaxSummary.Length; i++)
				{
					result.TaxSummary[i].TaxType = CSTaxType.Sales;
					Tax tax = CreateTax(Base, taxZone, vendor, result.TaxSummary[i]);
					if (tax == null)
						continue;

					ARTaxTran taxTran = new ARTaxTran
					{
						Module = BatchModule.AR,
						TranType = invoice.DocType,
						RefNbr = invoice.RefNbr,
						TaxID = tax?.TaxID,
						CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount),
						CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount),
						TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100,
						TaxType = result.TaxSummary[i].TaxType,
						TaxBucketID = 0,
						AccountID = tax?.SalesTaxAcctID ?? vendor.SalesTaxAcctID,
						SubID = tax?.SalesTaxSubID ?? vendor.SalesTaxSubID,
						JurisType = result.TaxSummary[i].JurisType,
						JurisName = result.TaxSummary[i].JurisName
					};

					Base.Taxes.Insert(taxTran);
				}
			}
			finally
			{
				TaxBaseAttribute.SetTaxCalc<ARTran.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);
			}

			bool requireControlTotal = Base.arsetup.Current.RequireControlTotal == true;

			if (invoice.Hold != true)
				Base.arsetup.Current.RequireControlTotal = false;

			try
			{
				Base.Document.Cache.SetValueExt<ARCashSale.isTaxSaved>(invoice, true);
			}
			finally
			{
				Base.arsetup.Current.RequireControlTotal = requireControlTotal;
			}
		}

		protected virtual void CancelTax(ARCashSale invoice, VoidReasonCode code)
		{
			string taxZoneID = ARCashSale.PK.Find(Base, invoice)?.TaxZoneID ?? invoice.TaxZoneID;

			var request = new VoidTaxRequest();
			request.CompanyCode = CompanyCodeFromBranch(taxZoneID, invoice.BranchID);
			request.Code = code;
			request.DocCode = $"AR.{invoice.DocType}.{invoice.RefNbr}";
			request.DocType = GetTaxDocumentType(invoice);

			var service = TaxProviderFactory(Base, taxZoneID);
			if (service == null)
				return;

			var result = service.VoidTax(request);

			if (!result.IsSuccess)
			{
				LogMessages(result);
				throw new PXException(TX.Messages.FailedToDeleteFromExternalTaxProvider);
			}
			else
			{
				invoice.IsTaxSaved = false;
				invoice.IsTaxValid = false;
				if (Base.Document.Cache.GetStatus(invoice) == PXEntryStatus.Notchanged)
					Base.Document.Cache.SetStatus(invoice, PXEntryStatus.Updated);
			}
		}

		protected override string GetExternalTaxProviderLocationCode(ARCashSale invoice) => GetExternalTaxProviderLocationCode<ARTran, ARTran.FK.CashSale.SameAsCurrent, ARTran.siteID>(invoice);

		protected virtual IAddressLocation GetFromAddress(ARCashSale invoice)
		{
			PXSelectBase<Branch> select = new PXSelectJoin
				<Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
					InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

			foreach (PXResult<Branch, BAccountR, Address> res in select.Select(invoice.BranchID))
				return (Address)res;

			return null;
		}

		protected virtual IAddressLocation GetToAddress(ARCashSale invoice)
		{
			return (ARShippingAddress)Base.Shipping_Address.View.SelectSingleBound(new object[] { invoice });
		}

        public virtual bool IsDocumentExtTaxValid(ARCashSale doc)
        {
            return doc != null && IsExternalTax(doc.TaxZoneID) && doc.InstallmentNbr == null;
        }
	}
}
