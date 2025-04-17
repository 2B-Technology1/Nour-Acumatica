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

using PX.Common;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.TaxProvider;
using System;
using System.Linq;
using Location = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AP
{
	public class APInvoiceEntryExternalTax : ExternalTax<APInvoiceEntry, APInvoice>
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

		public override APInvoice CalculateExternalTax(APInvoice invoice)
        {
			try
			{
				if (CalculateTaxesUsingExternalProvider(invoice.TaxZoneID))
				{
					if (invoice.InstallmentNbr != null || invoice.IsChildRetainageDocument())
					{
						//do not calculate tax for installments and child retainage documents
						return invoice;
					}

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
						Base.Document.SetValueExt<APInvoice.nonTaxable>(invoice, false);
					}

					var service = TaxProviderFactory(Base, invoice.TaxZoneID);

					var taxRequest = BuildTaxRequest(invoice);

					if (taxRequest.CartItems.Count == 0)
					{
						ApplyTax(invoice, GetTaxResult.Empty); // Invoice without APTran. Clear APTax.
						invoice.IsTaxValid = true;
						invoice.IsTaxSaved = false;
						invoice = Base.Document.Update(invoice);

						SkipTaxCalcAndSave();

						return invoice;
					}

					var result = service.GetTax(taxRequest);
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

						var request = new PX.TaxProvider.PostTaxRequest();
						request.CompanyCode = taxRequest.CompanyCode;
						request.CustomerCode = taxRequest.CustomerCode;
						request.BAccountClassID = taxRequest.BAccountClassID;
						request.DocCode = taxRequest.DocCode;
						request.DocDate = taxRequest.DocDate;
						request.DocType = taxRequest.DocType;
						request.TotalAmount = result.TotalAmount;
						request.TotalTaxAmount = result.TotalTaxAmount;
						var postResult = service.PostTax(request);
						if (postResult.IsSuccess)
						{
							APInvoice copy = PXCache<APInvoice>.CreateCopy(invoice);
							copy.IsTaxValid = true;
							invoice = Base.Document.Update(copy);
							SkipTaxCalcAndSave();
						}

					}
					else
					{
						PXTrace.WriteError(String.Join(", ", result.Messages));

						throw new PXException(TX.Messages.FailedToGetTaxes);
					}
				}

				return invoice;
			}
			finally
			{
				// We need to remove external transaction if external taxes were calculated for scheduled document
				if (invoice.IsTaxSaved == true && invoice.Scheduled == true)
				{
					VoidScheduledDocument(invoice);
					SkipTaxCalcAndSave();
				}
			}
        }

        [PXOverride]
        public virtual void Persist()
        {
            if (Base.Document.Current != null &&
                IsExternalTax(Base.Document.Current.TaxZoneID) &&
                Base.Document.Current.InstallmentNbr == null &&
                Base.Document.Current.IsTaxValid != true &&
                !skipExternalTaxCalcOnSave &&
				Base.Document.Current.Released != true)
            {
                if (PXLongOperation.GetCurrentItem() == null)
                {
					APInvoice currentDoc = Base.Document.Current;
                    PXLongOperation.StartOperation(Base, delegate
                    {
                        APInvoiceEntry rg = PXGraph.CreateInstance<APInvoiceEntry>();
                        rg.Document.Current = PXSelect<APInvoice, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(rg, currentDoc.DocType, currentDoc.RefNbr);
                        rg.CalculateExternalTax(rg.Document.Current);
                    });
                }
                else
                {
                    Base.CalculateExternalTax(Base.Document.Current);
                }
            }
        }

        [PXOverride]
        public virtual APRegister OnBeforeRelease(APRegister doc)
        {
            skipExternalTaxCalcOnSave = true;

            return doc;
        }

        protected virtual void _(Events.RowSelected<APInvoice> e)
        {
            if (e.Row == null)
                return;

			bool isExternalTax = CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID);
			bool enableTaxes = !isExternalTax &&
				e.Row.IsRetainageReversing != true &&
				e.Row.IsRetainageDocument != true;

			Base.Taxes.Cache.AllowDelete = Base.Taxes.Cache.AllowDelete && enableTaxes;
			Base.Taxes.Cache.AllowInsert = Base.Taxes.Cache.AllowInsert && enableTaxes;
			Base.Taxes.Cache.AllowUpdate = Base.Taxes.Cache.AllowUpdate && enableTaxes;

			if (isExternalTax && e.Row.IsTaxValid != true)
                PXUIFieldAttribute.SetWarning<APInvoice.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
        }

        protected virtual void _(Events.RowPersisting<APInvoice> e)
        {
			if (e.Row.Released == true)
                return;

			if (e.Row.IsTaxSaved == true)
			{
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
            if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update) && ((!IsExternalTax(Base, e.Row.TaxZoneID) && e.Row.ExternalTaxesImportInProgress != true) || IsNonTaxable(GetToAddress(e.Row))))
			{
				CancelTax(e.Row, VoidReasonCode.DocDeleted);
			}
		}

			if (CalculateTaxesUsingExternalProvider(e.Row.TaxZoneID) && e.Row.IsOriginalRetainageDocument() && Base.APSetup.Current?.RetainTaxes == true)
			{
				throw new PXException(AP.Messages.APExternalRetainedTaxesNotSupported);
			}
		}

        protected virtual void _(Events.RowUpdated<APTran> e)
        {
            //if any of the fields that was saved in External Tax Provider has changed mark doc as TaxInvalid.
            if (IsDocumentExtTaxValid(Base.Document.Current) && !e.Cache.ObjectsEqual<APTran.accountID, APTran.inventoryID, APTran.tranAmt, APTran.tranDate, APTran.taxCategoryID>(e.Row, e.OldRow))
            {
                Base.Document.Current.IsTaxValid = false;
                Base.Document.Update(Base.Document.Current);
            }
        }

        public virtual bool IsDocumentExtTaxValid(APInvoice doc)
        {
            return doc != null && CalculateTaxesUsingExternalProvider(doc.TaxZoneID) && doc.InstallmentNbr == null;
        }


        protected virtual void _(Events.RowDeleted<APTran> e)
        {
            Base.Document.Current.IsTaxValid = !IsDocumentExtTaxValid(Base.Document.Current);
        }

        protected virtual void _(Events.RowInserted<APTran> e)
        {
            if (IsDocumentExtTaxValid(Base.Document.Current))
            {
                Base.Document.Current.IsTaxValid = false;
                Base.Document.Cache.MarkUpdated(Base.Document.Current);
            }
        }

        protected virtual void _(Events.RowUpdated<APInvoice> e)
        {
            //Recalculate taxes when document date changed
            if (e.Row.Released != true)
            {
                if (IsDocumentExtTaxValid(e.Row) && !e.Cache.ObjectsEqual<
					APInvoice.curyDiscountedTaxableTotal,
					APInvoice.docDate,
					APInvoice.branchID>(e.Row, e.OldRow))
                {
                    e.Row.IsTaxValid = false;
                }
            }
        }

        protected virtual GetTaxRequest BuildTaxRequest(APInvoice invoice) => BuildCommitTaxRequest(invoice);

		public virtual CommitTaxRequest BuildCommitTaxRequest(APInvoice invoice)
        {
            if (invoice == null) throw new PXArgumentException(nameof(invoice), ErrorMessages.ArgumentNullException);

            Vendor vend = (Vendor)Base.vendor.View.SelectSingleBound(new object[] { invoice });
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { invoice });

			var request = new PX.TaxProvider.CommitTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(invoice.TaxZoneID, invoice.BranchID);
            request.CurrencyCode = invoice.CuryID;
            request.CustomerCode = vend.AcctCD;
            request.BAccountClassID = vend.ClassID;
            IAddressLocation fromAddress = GetFromAddress(invoice);
            IAddressLocation toAddress = GetToAddress(invoice);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFrom);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetTo);

            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = $"AP.{invoice.DocType}.{invoice.RefNbr}";
            request.DocDate = invoice.DocDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(invoice);
			request.APTaxType = taxZone.ExternalAPTaxType;
			request.IsTaxSaved = invoice.IsTaxSaved == true;

			Location branchLoc = GetBranchLocation(invoice);

            if (branchLoc != null)
            {
                request.CustomerUsageType = branchLoc.CAvalaraCustomerUsageType;
                request.ExemptionNo = branchLoc.CAvalaraExemptionNumber;
            }

			request.DocType = GetTaxDocumentType(invoice);
			Sign sign = GetDocumentSign(invoice);

            PXSelectBase<APTran> select = new PXSelectJoin<APTran,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<APTran.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<APTran.accountID>>>>,
                Where<APTran.tranType, Equal<Current<APInvoice.docType>>,
                    And<APTran.refNbr, Equal<Current<APInvoice.refNbr>>,
                    And<APTran.lineType, NotEqual<SOLineType.discount>>>>,
                OrderBy<Asc<APTran.tranType, Asc<APTran.refNbr, Asc<APTran.lineNbr>>>>>(Base);

            request.Discount = sign * GetDocDiscount().GetValueOrDefault();
			bool applyRetainage = Base.APSetup.Current?.RetainTaxes != true && invoice.IsOriginalRetainageDocument();

            foreach (PXResult<APTran, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { invoice }))
            {
                APTran tran = (APTran)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

				bool lineIsDiscounted = request.Discount != 0m &&
					((tran.DocumentDiscountRate ?? 1m) != 1m || (tran.GroupDiscountRate ?? 1m) != 1m);

                var line = new TaxCartItem();
                line.Index = tran.LineNbr.GetValueOrDefault();
                line.UnitPrice = sign * tran.CuryUnitCost.GetValueOrDefault();
				line.Amount = sign * (tran.CuryTranAmt.GetValueOrDefault() + (applyRetainage ? tran.CuryRetainageAmt.GetValueOrDefault() : 0m));
                line.Description = tran.TranDesc;
                line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(invoice, tran));
                line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(invoice, tran));
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

			if (invoice.DocType == APDocType.DebitAdj && (invoice.OrigDocDate != null))
			{
				request.TaxOverride.Reason = Messages.DebitAdjustmentReason;
				request.TaxOverride.TaxDate = invoice.OrigDocDate.Value;
				request.TaxOverride.TaxOverrideType = PX.TaxProvider.TaxOverrideType.TaxDate;
			}

            return request;
		}

		public virtual TaxDocumentType GetTaxDocumentType(APInvoice invoice)
		{
			TaxZone taxZone = (TaxZone)Base.taxzone.View.SelectSingleBound(new object[] { invoice });

			switch (invoice.DrCr)
			{
				case DrCr.Debit:
					return TaxDocumentType.PurchaseInvoice;
				case DrCr.Credit:
					return taxZone?.ExternalAPTaxType.Equals(ExternalAPTaxTypes.Use) ?? false
						? TaxDocumentType.PurchaseInvoice
						: TaxDocumentType.ReturnInvoice;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
		}

		public virtual Sign GetDocumentSign(APInvoice invoice)
		{
			switch (invoice.DrCr)
			{
				case DrCr.Debit:
					return Sign.Plus;
				case DrCr.Credit:
					return Sign.Minus;

				default:
					throw new PXException(Messages.DocTypeNotSupported);
			}
        }

        protected virtual void ApplyTax(APInvoice invoice, GetTaxResult result)
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
            foreach (PXResult<APTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { invoice }))
            {
                Base.Taxes.Delete(res);
            }

            Base.Views.Caches.Add(typeof(Tax));

			foreach (APTax item in Base.Tax_Rows.View.SelectMultiBound(new object[] { invoice }))
			{
				Base.Tax_Rows.Delete(item);
			}

			TaxCalc oldTaxCalc = TaxBaseAttribute.GetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null);
			bool requireControlTotal = Base.APSetup.Current.RequireControlTotal == true;

			try
			{
				if (invoice.Hold != true)
					Base.APSetup.Current.RequireControlTotal = false;

				TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, TaxCalc.ManualCalc);

				for (int i = 0; i < result.TaxSummary.Length; i++)
				{
					Tax tax = CreateTax(Base, taxZone, vendor, result.TaxSummary[i]);
					if (tax == null)
						continue;

					APTaxTran taxTran = new APTaxTran
					{
						Module = BatchModule.AP,
						TranType = invoice.DocType,
						RefNbr = invoice.RefNbr,
						TaxID = tax?.TaxID,
						CuryTaxAmt = Math.Abs(result.TaxSummary[i].TaxAmount),
						CuryTaxableAmt = Math.Abs(result.TaxSummary[i].TaxableAmount),
						TaxRate = Convert.ToDecimal(result.TaxSummary[i].Rate) * 100,
						CuryID = invoice.CuryID,
						TaxType = result.TaxSummary[i].TaxType,
						TaxBucketID = 0,
						AccountID = tax?.SalesTaxAcctID ?? vendor.SalesTaxAcctID,
						SubID = tax?.SalesTaxSubID ?? vendor.SalesTaxSubID,
						JurisType = result.TaxSummary[i].JurisType,
						JurisName = result.TaxSummary[i].JurisName
					};

					Base.Taxes.Insert(taxTran);
				}

				InsertTaxDetails(invoice, result, taxZone, vendor);
			}
			finally
			{
				TaxBaseAttribute.SetTaxCalc<APTran.taxCategoryID>(Base.Transactions.Cache, null, oldTaxCalc);
				Base.APSetup.Current.RequireControlTotal = requireControlTotal;
			}

            Base.Document.Cache.SetValueExt<APInvoice.isTaxSaved>(invoice, true);
        }

		private void InsertTaxDetails(APInvoice invoice, GetTaxResult result, TaxZone taxZone, Vendor vendor)
		{
			foreach (TaxLine taxline in result.TaxLines)
			{
				foreach (PX.TaxProvider.TaxDetail taxDetail in taxline.TaxDetails)
				{
					Tax tax = CreateTax(Base, taxZone, vendor, taxDetail);

					if (tax == null)
						continue;

					Base.Tax_Rows.Insert(new APTax
					{
						TranType = invoice.DocType,
						RefNbr = invoice.RefNbr,
						LineNbr = taxline.Index,
						TaxID = tax.TaxID,
						CuryTaxAmt = Math.Abs(taxDetail.TaxAmount),
						CuryTaxableAmt = Math.Abs(taxDetail.TaxableAmount),
						TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100,
					});
				}
			}
		}

		public void ApplyExternalTaxes(APInvoice invoice, GetTaxResult result)
		{
			try
			{
				ApplyTax(invoice, result);
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

        protected virtual void CancelTax(APInvoice invoice, VoidReasonCode code)
        {
			string taxZoneID = APInvoice.PK.Find(Base, invoice)?.TaxZoneID ?? invoice.TaxZoneID;

            var request = new VoidTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(taxZoneID, invoice.BranchID);
            request.Code = code;
            request.DocCode = $"AP.{invoice.DocType}.{invoice.RefNbr}";
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

		public virtual void VoidScheduledDocument(APInvoice invoice)
		{
			bool isTaxValid = invoice.IsTaxValid ?? false;
			CancelTax(invoice, VoidReasonCode.DocDeleted);

			if (isTaxValid)
			{
				invoice.IsTaxValid = true;
				Base.Document.Cache.MarkUpdated(invoice);
			}
		}

		protected override string GetExternalTaxProviderLocationCode(APInvoice invoice) => GetExternalTaxProviderLocationCode<APTran, APTran.FK.Invoice.SameAsCurrent, APTran.siteID>(invoice);

        protected virtual IAddressLocation GetToAddress(APInvoice invoice)
        {
            return
                PXSelectJoin<Branch,
                InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
                    InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
                Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
                .Select(Base, invoice.BranchID)
                .RowCast<Address>()
                .FirstOrDefault();
        }

		protected virtual IAddressLocation GetToAddress(APInvoice invoice, APTran tran)
		{
			POShipAddress poAddress =
				PXSelectJoin<POShipAddress,
				InnerJoin<POOrder, On<POShipAddress.addressID, Equal<POOrder.shipAddressID>>>,
				Where<POOrder.orderType, Equal<Current<APTran.pOOrderType>>,
					And<POOrder.orderNbr, Equal<Current<APTran.pONbr>>>>>
				.SelectSingleBound(Base, new[] { tran });

			if (tran.POOrderType.IsIn(POOrderType.DropShip, POOrderType.ProjectDropShip))
				return poAddress;

			Address receiptLineAddress =
				PXSelectJoin<Address,
				InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
				InnerJoin<POReceiptLine, On<POReceiptLine.siteID, Equal<INSite.siteID>>>>,
				Where<POReceiptLine.receiptType, Equal<Current<APTran.receiptType>>,
					And<POReceiptLine.receiptNbr, Equal<Current<APTran.receiptNbr>>,
					And<POReceiptLine.lineNbr, Equal<Current<APTran.receiptLineNbr>>>>>>
				.SelectSingleBound(Base, new[] { tran });

			if (receiptLineAddress != null)
				return receiptLineAddress;

			return (Address)PXSelectJoin<Address,
				InnerJoin<INSite, On<INSite.addressID, Equal<Address.addressID>>,
				InnerJoin<POLine, On<POLine.siteID, Equal<INSite.siteID>>>>,
				Where<POLine.orderType, Equal<Current<APTran.pOOrderType>>,
					And<POLine.orderNbr, Equal<Current<APTran.pONbr>>,
					And<POLine.lineNbr, Equal<Current<APTran.pOLineNbr>>>>>>
				.SelectSingleBound(Base, new[] { tran })
				?? poAddress
				?? GetToAddress(invoice);
		}

        protected virtual Location GetBranchLocation(APInvoice invoice)
        {
            PXSelectBase<Branch> select = new PXSelectJoin
                <Branch, InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
                    InnerJoin<Location, On<Location.bAccountID, Equal<BAccountR.bAccountID>, And<Location.locationID, Equal<BAccountR.defLocationID>>>>>,
                    Where<Branch.branchID, Equal<Required<Branch.branchID>>>>(Base);

            foreach (PXResult<Branch, BAccountR, Location> res in select.Select(invoice.BranchID))
                return (Location)res;

            return null;
        }

        protected virtual IAddressLocation GetFromAddress(APInvoice invoice)
        {
            return
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>>,
                Where<Location.locationID, Equal<Required<Location.locationID>>>>
                .SelectWindowed(Base, 0, 1, new object[]
                {
                    PXAccess.FeatureInstalled<FeaturesSet.vendorRelations>()
                        ? invoice.SuppliedByVendorLocationID
                        : invoice.VendorLocationID
                })
                .RowCast<Address>()
                .FirstOrDefault();
        }

        protected virtual IAddressLocation GetFromAddress(APInvoice invoice, APTran tran)
        {
            return (IAddressLocation)
                PXSelectJoin<PORemitAddress,
                InnerJoin<POOrder, On<PORemitAddress.addressID, Equal<POOrder.remitAddressID>>>,
                Where<POOrder.orderType, Equal<Current<APTran.pOOrderType>>,
                    And<POOrder.orderNbr, Equal<Current<APTran.pONbr>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<PORemitAddress>()
                .FirstOrDefault()
                ??
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>,
                InnerJoin<POReceipt, On<POReceipt.vendorLocationID, Equal<Location.locationID>>,
                InnerJoin<POReceiptLine,
					On<POReceiptLine.FK.Receipt>>>>,
                Where<POReceiptLine.receiptType, Equal<Current<APTran.receiptType>>,
                    And<POReceiptLine.receiptNbr, Equal<Current<APTran.receiptNbr>>,
                    And<POReceiptLine.lineNbr, Equal<Current<APTran.receiptLineNbr>>>>>>
                .SelectSingleBound(Base, new[] { tran })
                .RowCast<Address>()
                .FirstOrDefault()
                ??
                PXSelectJoin<Address,
                InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>>,
                Where<Location.locationID, Equal<Current<APInvoice.vendorLocationID>>>>
                .SelectSingleBound(Base, new[] { invoice })
                .RowCast<Address>()
                .FirstOrDefault(); ;
        }

		protected override decimal? GetDocDiscount()
		{
			return Base.Document.Current?.CuryDiscTot ?? 0m;
		}
    }
}
