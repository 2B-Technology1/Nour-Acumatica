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

/// <summary>
/// All the commented code is there just to compare future changes on Opportunity / Sales Order external tax calc changes
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.CS.Contracts.Interfaces;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.TaxProvider;

namespace PX.Objects.FS
{
    public class ServiceOrderEntryExternalTax : ExternalTax<ServiceOrderEntry, FSServiceOrder>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.avalaraTax>();
        }

        public override bool IsExternalTax(string taxZoneID)
        {
            if (Base.ServiceOrderTypeSelected.Current != null && Base.ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.PROJECTS)
                return false;

            return IsExternalTax(Base, taxZoneID);
        }

        public bool SkipExternalTaxCalcOnSave
        {
            get
            {
                return skipExternalTaxCalcOnSave;
            }
            set
            {
                skipExternalTaxCalcOnSave = value;
            }
        }

        public override FSServiceOrder CalculateExternalTax(FSServiceOrder order)
        {
            if (IsExternalTax(order.TaxZoneID) && skipExternalTaxCalcOnSave == false)
                return CalculateExternalTax(order, false);

            return order;
        }

        public virtual FSServiceOrder CalculateExternalTax(FSServiceOrder order, bool forceRecalculate)
        {
            var toAddress = GetToAddress(order);

            var service = TaxProviderFactory(Base, order.TaxZoneID);

            GetTaxRequest getRequest = null;
            GetTaxRequest getRequestOpen = null;
            GetTaxRequest getRequestUnbilled = null;
            GetTaxRequest getRequestFreight = null;

            bool isValidByDefault = false;

            FSSrvOrdType srvOrdType = PXSelect<FSSrvOrdType, Where<FSSrvOrdType.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>>>.Select(this.Base, order.SrvOrdType);

            if (/*srvOrdType.INDocType != INTranType.Transfer &&*/toAddress != null && !IsNonTaxable(toAddress))
            {
                if (order.IsTaxValid != true || forceRecalculate)
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

                /*if (order.IsOpenTaxValid != true || forceRecalculate)
                {
                    getRequestOpen = BuildGetTaxRequestOpen(order);
                    if (getRequestOpen.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequestOpen = null;
                    }
                }*/

                /*if (order.IsUnbilledTaxValid != true || forceRecalculate)
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
                }*/

                /*if (order.IsFreightTaxValid != true || forceRecalculate)
                {
                    getRequestFreight = BuildGetTaxRequestFreight(order);
                    if (getRequestFreight.CartItems.Count > 0)
                    {
                        isValidByDefault = false;
                    }
                    else
                    {
                        getRequestFreight = null;
                    }
                }*/
            }

            if (isValidByDefault)
            {
                order.CuryTaxTotal = 0;
                //order.CuryOpenTaxTotal = 0;
                //order.CuryUnbilledTaxTotal = 0;
                order.IsTaxValid = true;
                //order.IsOpenTaxValid = true;
                //order.IsUnbilledTaxValid = true;
                //order.IsFreightTaxValid = true;

                Base.CurrentServiceOrder.Update(order);

                foreach (FSServiceOrderTaxTran item in Base.Taxes.Select())
                {
                    Base.Taxes.Delete(item);
                }

                using (var ts = new PXTransactionScope())
                {
                    Base.Persist(typeof(FSServiceOrderTaxTran), PXDBOperation.Delete);
                    Base.Persist(typeof(FSServiceOrder), PXDBOperation.Update);
                    PXTimeStampScope.PutPersisted(Base.CurrentServiceOrder.Cache, order, PXDatabase.SelectTimeStamp());
                    ts.Complete();
                }
                return order;
            }

            GetTaxResult result = null;
            GetTaxResult resultOpen = null;
            GetTaxResult resultUnbilled = null;
            GetTaxResult resultFreight = null;

            bool getTaxFailed = false;
            if (getRequest != null)
            {
                result = service.GetTax(getRequest);

                if (!result.IsSuccess)
                {
                    getTaxFailed = true;
                }
            }
            if (getRequestOpen != null)
            {
                if (getRequest != null && IsSame(getRequest, getRequestOpen))
                {
                    resultOpen = result;
                }
                else
                {
                    resultOpen = service.GetTax(getRequestOpen);

                    if (!resultOpen.IsSuccess)
                    {
                        getTaxFailed = true;
                    }
                }
            }
            if (getRequestUnbilled != null)
            {
                if (getRequest != null && IsSame(getRequest, getRequestUnbilled))
                {
                    resultUnbilled = result;
                }
                else
                {
                    resultUnbilled = service.GetTax(getRequestUnbilled);

                    if (!resultUnbilled.IsSuccess)
                    {
                        getTaxFailed = true;
                    }
                }
            }
            if (getRequestFreight != null)
            {
                resultFreight = service.GetTax(getRequestFreight);

                if (!resultFreight.IsSuccess)
                {
                    getTaxFailed = true;
                }
            }

            if (!getTaxFailed)
            {
                try
                {
                    ApplyTax(order, result, resultOpen, resultUnbilled, resultFreight);
                }
                catch (PXOuterException ex)
                {
                    string msg = PX.Objects.TX.Messages.FailedToApplyTaxes;
                    foreach (string err in ex.InnerMessages)
                    {
                        msg += Environment.NewLine + err;
                    }

                    throw new PXException(ex, msg);
                }
                catch (Exception ex)
                {
                    string msg = PX.Objects.TX.Messages.FailedToApplyTaxes;
                    msg += Environment.NewLine + ex.Message;

                    throw new PXException(ex, msg);
                }
            }
            else
            {
                ResultBase taxResult = result ?? resultOpen ?? resultUnbilled ?? resultFreight;
                if (taxResult != null)
                    LogMessages(taxResult);

                throw new PXException(PX.Objects.TX.Messages.FailedToGetTaxes);
            }

            return order;
        }

        [PXOverride]
        public virtual void RecalculateExternalTaxes()
        {
            if (Base.CurrentServiceOrder.Current != null && IsExternalTax(Base.CurrentServiceOrder.Current.TaxZoneID) && !skipExternalTaxCalcOnSave && /*!Base.IsTransferOrder &&*/
                (Base.CurrentServiceOrder.Current.IsTaxValid != true /*|| Base.CurrentServiceOrder.Current.IsOpenTaxValid != true || Base.CurrentServiceOrder.Current.IsUnbilledTaxValid != true*/)
            )
            {
				FSServiceOrder doc = (FSServiceOrder)Base.ServiceOrderRecords.Cache.CreateCopy(Base.CurrentServiceOrder.Current);
				if (Base.RecalculateExternalTaxesSync)
                {
					ServiceOrderExternalTaxCalc.Process(doc);
                }
                else
                {
                    Debug.Print("{0} SOExternalTaxCalc.Process(doc) Async", DateTime.Now.TimeOfDay);
                    PXLongOperation.StartOperation(Base, delegate ()
                    {
                        Debug.Print("{0} Inside PXLongOperation.StartOperation", DateTime.Now.TimeOfDay);
                        ServiceOrderExternalTaxCalc.Process(doc);
                    });
                }
            }
        }

        public PXAction<FSServiceOrder> recalcExternalTax;
        [PXUIField(DisplayName = "Recalculate External Tax", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
        [PXButton()]
        public virtual IEnumerable RecalcExternalTax(PXAdapter adapter)
        {
            if (Base.CurrentServiceOrder.Current != null && IsExternalTax(Base.CurrentServiceOrder.Current.TaxZoneID))
            {
                var order = Base.CurrentServiceOrder.Current;
                CalculateExternalTax(Base.CurrentServiceOrder.Current, true);

                Base.Clear(PXClearOption.ClearAll);
                Base.ServiceOrderRecords.Current = Base.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(order.RefNbr, order.SrvOrdType);

                yield return Base.CurrentServiceOrder.Current;
            }
            else
            {
                foreach (var res in adapter.Get())
                {
                    yield return res;
                }
            }
        }

        protected virtual void _(Events.RowSelected<FSServiceOrder> e)
        {
            if (e.Row == null)
                return;

            var isExternalTax = IsExternalTax(e.Row.TaxZoneID);
            bool runningFromExternalControls = Base.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.WEB_METHOD);

            if (isExternalTax == true && ((FSServiceOrder)e.Row).IsTaxValid != true
                && runningFromExternalControls == false
            )
            {
                PXUIFieldAttribute.SetWarning<FSServiceOrder.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
            }/*
            else if (isExternalTax == true && ((FSServiceOrder)e.Row).IsFreightTaxValid != true && !Base.IsTransferOrder)
                PXUIFieldAttribute.SetWarning<FSServiceOrder.curyTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

            if (isExternalTax == true && ((FSServiceOrder)e.Row).IsOpenTaxValid != true && !Base.IsTransferOrder)
                PXUIFieldAttribute.SetWarning<FSServiceOrder.curyOpenTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);

            if (isExternalTax == true && ((FSServiceOrder)e.Row).IsUnbilledTaxValid != true && !Base.IsTransferOrder)
            {
                PXUIFieldAttribute.SetWarning<FSServiceOrder.curyUnbilledTaxTotal>(e.Cache, e.Row, AR.Messages.TaxIsNotUptodate);
                PXUIFieldAttribute.SetWarning<FSServiceOrder.curyUnbilledOrderTotal>(e.Cache, e.Row, PX.Objects.SO.Messages.UnbilledBalanceWithoutTaxTaxIsNotUptodate);
            }*/


            Base.Taxes.Cache.AllowInsert = !isExternalTax && e.Row.AllowInvoice == false;
            Base.Taxes.Cache.AllowUpdate = !isExternalTax && e.Row.AllowInvoice == false;
            Base.Taxes.Cache.AllowDelete = !isExternalTax && e.Row.AllowInvoice == false;
        }

        protected virtual void _(Events.RowUpdated<FSServiceOrder> e)
        {
            if (e.Row == null)
                return;


            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (IsExternalTax(e.Row.TaxZoneID)
                && (!e.Cache.ObjectsEqual<FSServiceOrder.branchLocationID,
                                          FSServiceOrder.orderDate,
                                          FSServiceOrder.taxZoneID,
                                          FSServiceOrder.billCustomerID,
                                          FSServiceOrder.serviceOrderAddressID,
                                          FSServiceOrder.branchID,
										  FSServiceOrder.curyDiscTot>(e.Row, e.OldRow)))
            {
                e.Row.IsTaxValid = false;
                //e.Row.IsOpenTaxValid = false;
                //e.Row.IsUnbilledTaxValid = false;

                /*if (!e.Cache.ObjectsEqual<FSSODet.openAmt>(e.Row, e.OldRow))
                {
                    e.Row.IsOpenTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSSODet.unbilledAmt>(e.Row, e.OldRow))
                {
                    e.Row.IsUnbilledTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSServiceOrder.curyFreightTot, FSServiceOrder.freightTaxCategoryID>(e.OldRow, e.Row))
                {
                    e.Row.IsFreightTaxValid = false;
                    e.Row.IsTaxValid = false;
                    e.Row.IsOpenTaxValid = false;
                    e.Row.IsUnbilledTaxValid = false;
                }*/
            }
        }

        protected virtual void _(Events.RowInserted<FSSODet> e)
        {
                InvalidateExternalTax(Base.CurrentServiceOrder.Current);
            }

        protected virtual void _(Events.RowUpdated<FSSODet> e)
        {
            //if any of the fields that was saved in avalara has changed mark doc as TaxInvalid.
            if (Base.CurrentServiceOrder.Current != null && IsExternalTax(Base.CurrentServiceOrder.Current.TaxZoneID))
            {
                if (!e.Cache.ObjectsEqual<
                        FSSODet.acctID,
                        FSSODet.inventoryID,
                        FSSODet.tranDesc,
                        FSSODet.curyBillableTranAmt,
                        FSSODet.tranDate,
                        FSSODet.taxCategoryID,
                        FSSODet.siteID
                    >(e.Row, e.OldRow) ||
                    (e.Row.POSource == INReplenishmentSource.DropShipToOrder) != (e.OldRow.POSource == INReplenishmentSource.DropShipToOrder))
                {
                    InvalidateExternalTax(Base.CurrentServiceOrder.Current);
                }

                /*if (!e.Cache.ObjectsEqual<FSSODet.openAmt>(e.Row, e.OldRow))
                {
                    Base.CurrentServiceOrder.Current.IsOpenTaxValid = false;
                }*/

                /*if (!e.Cache.ObjectsEqual<FSSODet.unbilledAmt>(e.Row, e.OldRow))
                {
                    Base.CurrentServiceOrder.Current.IsUnbilledTaxValid = false;
                }*/
            }
        }
        protected virtual void _(Events.RowDeleted<FSSODet> e)
        {
            InvalidateExternalTax(Base.CurrentServiceOrder.Current);
        }

        #region FSAddress Events
        protected virtual void _(Events.RowUpdated<FSAddress> e)
        {
            if (e.Row == null) return;
            if (e.Cache.ObjectsEqual<FSAddress.postalCode, FSAddress.countryID, FSAddress.state>(e.Row, e.OldRow) == false)
                InvalidateExternalTax(Base.CurrentServiceOrder.Current);
        }

        protected virtual void _(Events.RowInserted<FSAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.CurrentServiceOrder.Current);
        }

        protected virtual void _(Events.RowDeleted<FSAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.CurrentServiceOrder.Current);
        }

        protected virtual void _(Events.FieldUpdating<FSAddress, FSAddress.overrideAddress> e)
        {
            if (e.Row == null) return;
            InvalidateExternalTax(Base.CurrentServiceOrder.Current);
        }
        #endregion

        protected virtual GetTaxRequest BuildGetTaxRequest(FSServiceOrder order)
        {
            if (order == null)
                throw new PXArgumentException(nameof(order));


            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });
			TaxZone taxZone = (TaxZone)Base.TaxZone.View.SelectSingleBound(new object[] { order });

			IAddressLocation fromAddress = GetFromAddress(order);
            IAddressLocation toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetFromAddressCR);

            if (toAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetToAddressCR);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust?.AcctCD;
            request.BAccountClassID = cust?.ClassID;
			request.TaxRegistrationID = loc?.TaxRegistrationID;
			request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("SO.{0}.{1}", order.SrvOrdType, order.RefNbr);
            request.DocDate = order.OrderDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);
			request.APTaxType = taxZone.ExternalAPTaxType;

			Sign sign = Sign.Plus;

            request.CustomerUsageType = loc?.CAvalaraCustomerUsageType;
            if (!string.IsNullOrEmpty(loc?.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc?.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            /*if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                sign = Sign.Minus;

                PXSelectBase<FSSODet> selectLineWithInvoiceDate = new PXSelect<FSSODet,
                Where<FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>, And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                And<FSSODet.invoiceDate, IsNotNull>>>>(Base);

                FSSODet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = PX.Objects.SO.Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {*/
            request.DocType = TaxDocumentType.SalesOrder;
            /*}*/


            /* We need InnerJoin with InventoryItem instead of LeftJoin */
            /* because of instructions and comments lines */
            PXSelectBase<FSSODet> select = new PXSelectJoin<FSSODet,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSSODet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSSODet.acctID>>>>,
                Where<FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                    And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
                OrderBy<Asc<FSSODet.srvOrdType, Asc<FSSODet.refNbr, Asc<FSSODet.lineNbr>>>>>(Base);

            request.Discount = sign * order.CuryDiscTot.GetValueOrDefault();
            
            foreach (PXResult<FSSODet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
            {
                FSSODet tran = (FSSODet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;                

                var line = new TaxCartItem();
                line.Index = tran.LineNbr ?? 0;

                /*if (srvOrdType.DefaultOperation != tran.Operation)
                    line.Amount = Sign.Minus * sign * tran.CuryLineAmt.GetValueOrDefault();
                else
                    line.Amount = sign * tran.CuryLineAmt.GetValueOrDefault();*/
                line.UnitPrice = sign * tran.CuryUnitPrice.GetValueOrDefault();
                line.Amount = sign * tran.CuryBillableTranAmt.GetValueOrDefault();

                line.Description = tran.TranDesc;
                line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
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

				request.CartItems.Add(line);
            }

            return request;
        }

        /*protected virtual GetTaxRequest BuildGetTaxRequestOpen(FSServiceOrder order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFromAddressSO);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetToAddressSO);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("SO.{0}.{1}", order.SrvOrdType, order.RefNbr);
            request.DocDate = order.OrderDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(loc.CAvalaraCustomerUsageType))
            {
                request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;

                PXSelectBase<FSSODet> selectLineWithInvoiceDate = new PXSelect<FSSODet,
                Where<FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>, And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                And<FSSODet.invoiceDate, IsNotNull>>>>(Base);

                FSSODet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }
            request.DocType = TaxDocumentType.SalesOrder;


            PXSelectBase<FSSODet> select = new PXSelectJoin<FSSODet,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSSODet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSSODet.acctID>>>>,
                Where<FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                    And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
                OrderBy<Asc<FSSODet.srvOrdType, Asc<FSSODet.refNbr, Asc<FSSODet.lineNbr>>>>>(Base);

            request.Discount = order.CuryDiscTot.GetValueOrDefault();

            foreach (PXResult<FSSODet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
            {
                FSSODet tran = (FSSODet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                if (tran.OpenAmt >= 0)
                {
                    var line = new TaxCartItem();
                    line.Index = tran.LineNbr ?? 0;
                    if (srvOrdType.DefaultOperation != tran.Operation)
                        line.Amount = -1 * mult * tran.CuryOpenAmt.GetValueOrDefault();
                    else
                        line.Amount = mult * tran.CuryOpenAmt.GetValueOrDefault();
                    line.Description = tran.TranDesc;
                    line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                    line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
                    line.ItemCode = item.InventoryCD;
                    line.Quantity = Math.Abs(tran.OpenQty.GetValueOrDefault());
                    line.Discounted = request.Discount > 0;
                    line.RevAcct = salesAccount.AccountCD;

                    line.TaxCode = tran.TaxCategoryID;

                    request.CartItems.Add(line);
                }
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestOpen() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        /*protected virtual GetTaxRequest BuildGetTaxRequestUnbilled(FSServiceOrder order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(Messages.FailedGetFromAddressSO);

            if (toAddress == null)
                throw new PXException(Messages.FailedGetToAddressSO);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = string.Format("{0}.{1}.Open", order.SrvOrdType, order.RefNbr);
            request.DocDate = order.OrderDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(order.AvalaraCustomerUsageType))
            {
                request.CustomerUsageType = order.AvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.DefaultOperation == SOOperation.Receipt)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;

                PXSelectBase<FSSODet> selectLineWithInvoiceDate = new PXSelect<FSSODet,
                Where<FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>, And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                And<FSSODet.invoiceDate, IsNotNull>>>>(Base);

                FSSODet soLine = selectLineWithInvoiceDate.SelectSingle(order.SrvOrdType, order.RefNbr);
                if (soLine != null && soLine.TranDate != null)
                {
                    request.TaxOverride.Reason = Messages.ReturnReason;
                    request.TaxOverride.TaxDate = soLine.TranDate.Value;
                    request.TaxOverride.TaxOverrideType = TaxOverrideType.TaxDate;
                }

            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }


            PXSelectBase<FSSODet> select = new PXSelectJoin<FSSODet,
                LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<FSSODet.inventoryID>>,
                    LeftJoin<Account, On<Account.accountID, Equal<FSSODet.salesAcctID>>>>,
                Where<FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                    And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>,
                OrderBy<Asc<FSSODet.srvOrdType, Asc<FSSODet.refNbr, Asc<FSSODet.lineNbr>>>>>(Base);

            request.Discount = order.CuryDiscTot.GetValueOrDefault();

            foreach (PXResult<FSSODet, InventoryItem, Account> res in select.View.SelectMultiBound(new object[] { order }))
            {
                FSSODet tran = (FSSODet)res;
                InventoryItem item = (InventoryItem)res;
                Account salesAccount = (Account)res;

                if (tran.UnbilledAmt >= 0)
                {
                    var line = new TaxCartItem();
                    line.Index = tran.LineNbr ?? 0;
                    if (srvOrdType.DefaultOperation != tran.Operation)
                        line.Amount = -1 * mult * tran.CuryUnbilledAmt.GetValueOrDefault();
                    else
                        line.Amount = mult * tran.CuryUnbilledAmt.GetValueOrDefault();
                    line.Description = tran.TranDesc;
                    line.DestinationAddress = AddressConverter.ConvertTaxAddress(GetToAddress(order, tran));
                    line.OriginAddress = AddressConverter.ConvertTaxAddress(GetFromAddress(order, tran));
                    line.ItemCode = item.InventoryCD;
                    line.Quantity = Math.Abs(tran.UnbilledQty.GetValueOrDefault());
                    line.Discounted = request.Discount > 0;
                    line.RevAcct = salesAccount.AccountCD;

                    line.TaxCode = tran.TaxCategoryID;

                    request.CartItems.Add(line);
                }
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestUnbilled() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        /*protected virtual GetTaxRequest BuildGetTaxRequestFreight(FSServiceOrder order)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (order == null)
                throw new PXArgumentException(ErrorMessages.ArgumentNullException);

            Customer cust = (Customer)Base.TaxCustomer.View.SelectSingleBound(new object[] { order });
            Location loc = (Location)Base.TaxLocation.View.SelectSingleBound(new object[] { order });

            IAddressBase fromAddress = GetFromAddress(order);
            IAddressBase toAddress = GetToAddress(order);

            if (fromAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetFromAddressCR);

            if (toAddress == null)
                throw new PXException(PX.Objects.CR.Messages.FailedGetToAddressCR);

            GetTaxRequest request = new GetTaxRequest();
            request.CompanyCode = CompanyCodeFromBranch(order.TaxZoneID, order.BranchID);
            request.CurrencyCode = order.CuryID;
            request.CustomerCode = cust.AcctCD;
            request.OriginAddress = AddressConverter.ConvertTaxAddress(fromAddress);
            request.DestinationAddress = AddressConverter.ConvertTaxAddress(toAddress);
            request.DocCode = $"{order.SrvOrdType}.{order.RefNbr}.Freight";
            request.DocDate = order.OrderDate.GetValueOrDefault();
            request.LocationCode = GetExternalTaxProviderLocationCode(order);

            int mult = 1;

            if (!string.IsNullOrEmpty(loc.CAvalaraCustomerUsageType))
            {
                request.CustomerUsageType = loc.CAvalaraCustomerUsageType;
            }
            if (!string.IsNullOrEmpty(loc.CAvalaraExemptionNumber))
            {
                request.ExemptionNo = loc.CAvalaraExemptionNumber;
            }

            FSSrvOrdType srvOrdType = (FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order });

            if (srvOrdType.ARDocType == ARDocType.CreditMemo)
            {
                request.DocType = TaxDocumentType.ReturnOrder;
                mult = -1;
            }
            else
            {
                request.DocType = TaxDocumentType.SalesOrder;
            }

            if (order.CuryFreightTot > 0)
            {
                var line = new TaxCartItem();
                line.Index = short.MaxValue;
                line.Amount = mult * order.CuryFreightTot.GetValueOrDefault();
                line.Description = PXMessages.LocalizeNoPrefix(Messages.FreightDesc);
                line.DestinationAddress = request.DestinationAddress;
                line.OriginAddress = request.OriginAddress;
                line.ItemCode = "N/A";
                line.Discounted = false;
                line.TaxCode = order.FreightTaxCategoryID;

                request.CartItems.Add(line);
            }

            sw.Stop();
            Debug.Print("BuildGetTaxRequestFreight() in {0} millisec.", sw.ElapsedMilliseconds);

            return request;
        }
        */
        protected virtual void ApplyTax(FSServiceOrder order, GetTaxResult result, GetTaxResult resultOpen, GetTaxResult resultUnbilled, GetTaxResult resultFreight)
        {
			TaxZone taxZone = (TaxZone)Base.TaxZone.View.SelectSingleBound(new object[] { order });
			AP.Vendor vendor = GetTaxAgency(Base, taxZone);

			/*var sign = ((FSSrvOrdType)Base.ServiceOrderTypeSelected.View.SelectSingleBound(new object[] { order })).DefaultOperation == SOOperation.Receipt
                ? Sign.Minus
                : Sign.Plus;*/
			var sign = Sign.Plus;

			if (result != null)
			{
				//Clear all existing Tax transactions:
				foreach (PXResult<FSServiceOrderTaxTran, Tax> res in Base.Taxes.View.SelectMultiBound(new object[] { order }))
				{
					FSServiceOrderTaxTran taxTran = res;
					Base.Taxes.Delete(taxTran);
				}

				Base.Views.Caches.Add(typeof(Tax));

				decimal freightTax = 0;
				if (resultFreight != null)
					freightTax = sign * resultFreight.TotalTaxAmount;

				//bool requireControlTotal = Base.ServiceOrderTypeSelected.Current.RequireControlTotal == true;
				/*if (order.Hold != true)
                    Base.ServiceOrderTypeSelected.Current.RequireControlTotal = false;*/

				var taxDetails = new List<PX.TaxProvider.TaxDetail>();

				for (int i = 0; i < result.TaxSummary.Length; i++)
				{
					string taxID = result.TaxSummary[i].TaxName;

					if (string.IsNullOrEmpty(taxID))
						taxID = result.TaxSummary[i].JurisCode;

					if (string.IsNullOrEmpty(taxID))
					{
						PXTrace.WriteInformation(PX.Objects.SO.Messages.EmptyValuesFromExternalTaxProvider);
						continue;
					}

					taxDetails.Add(result.TaxSummary[i]);
				}

				if (resultFreight != null)
				{
					foreach (TaxProvider.TaxDetail tax in resultFreight.TaxSummary.OrderByDescending(e => e.TaxAmount))
					{
						if (tax.TaxAmount != 0
							|| taxDetails.Find(e => e.TaxName == tax.TaxName) == default(TaxProvider.TaxDetail))
						{
							taxDetails.Add(tax);
						}
					}
				}

				try
				{
					foreach (var taxDetail in taxDetails)
					{
						taxDetail.TaxType = CSTaxType.Sales;
						Tax tax = CreateTax(Base, taxZone, vendor, taxDetail);
						if (tax == null)
							continue;

						FSServiceOrderTaxTran taxTran = (FSServiceOrderTaxTran)Base.Taxes.Cache.CreateInstance();
						taxTran.TaxID = tax?.TaxID;
						taxTran.CuryTaxAmt = Math.Abs(taxDetail.TaxAmount);
						taxTran.CuryTaxableAmt = Math.Abs(taxDetail.TaxableAmount);
						taxTran.TaxRate = Convert.ToDecimal(taxDetail.Rate) * 100;
						taxTran.JurisType = taxDetail.JurisType;
						taxTran.JurisName = taxDetail.JurisName;
						taxTran.TaxZoneID = taxZone.TaxZoneID;

						Base.Taxes.Insert(taxTran);
					}

					Base.CurrentServiceOrder.SetValueExt<FSServiceOrder.curyTaxTotal>(order, sign * result.TotalTaxAmount + freightTax);

					decimal? CuryDocTotal = Base.GetCuryDocTotal(order.CuryBillableOrderTotal, order.CuryDiscTot, order.CuryTaxTotal, 0);
					Base.CurrentServiceOrder.SetValueExt<FSServiceOrder.curyDocTotal>(order, CuryDocTotal ?? 0m);
				}
				finally
				{
					//Base.ServiceOrderTypeSelected.Current.RequireControlTotal = requireControlTotal;
				}
			}


            /*if (resultUnbilled != null)
                Base.CurrentServiceOrder.SetValueExt<FSServiceOrder.curyUnbilledTaxTotal>(order, sign * resultUnbilled.TotalTaxAmount);

            if (resultOpen != null)
                Base.CurrentServiceOrder.SetValueExt<FSServiceOrder.curyOpenTaxTotal>(order, sign * resultOpen.TotalTaxAmount);*/

            order = (FSServiceOrder)Base.CurrentServiceOrder.Cache.CreateCopy(order);
            order.IsTaxValid = true;
            Base.CurrentServiceOrder.Cache.Update(order);

            if (Base.TimeStamp == null)
            {
                Base.SelectTimeStamp();
            }

            SkipTaxCalcAndSave();
        }

        protected virtual bool IsSame(GetTaxRequest x, GetTaxRequest y)
        {
            if (x.CartItems.Count != y.CartItems.Count)
                return false;

            for (int i = 0; i < x.CartItems.Count; i++)
            {
                if (x.CartItems[i].Amount != y.CartItems[i].Amount)
                    return false;
            }

            return true;
        }

		protected override string GetExternalTaxProviderLocationCode(FSServiceOrder order) => GetExternalTaxProviderLocationCode<FSSODet, FSSODet.FK.ServiceOrder.SameAsCurrent, FSSODet.siteID>(order);

		public virtual IAddressLocation GetFromAddress(FSServiceOrder order)
        {
            FSAddress returnAdrress = PXSelectJoin<FSAddress,
                                        InnerJoin<
                                            FSBranchLocation,
                                            On<FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                        Where<
                                            FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                            .Select(Base, order.BranchLocationID)
                                            .RowCast<FSAddress>()
                                            .FirstOrDefault();

            /*
            var branch =
                PXSelectJoin<Branch,
                InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Branch.bAccountID>>,
                InnerJoin<Address, On<Address.addressID, Equal<BAccountR.defAddressID>>>>,
                Where<Branch.branchID, Equal<Required<Branch.branchID>>>>
                .Select(Base, order.BranchID)
                .RowCast<Address>()
                .FirstOrDefault()
                .With(ValidAddressFrom<BAccountR.defAddressID>);*/

            return returnAdrress;
        }

        public virtual IAddressLocation GetFromAddress(FSServiceOrder order, FSSODet line)
        {
            /*Boolean isDropShip = line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder;
            IAddressBase vendorAddress = isDropShip
                ? PXSelectJoin<Address,
                    InnerJoin<Location, On<Location.defAddressID, Equal<Address.addressID>>,
                    InnerJoin<Vendor, On<Vendor.defLocationID, Equal<Location.locationID>>>>,
                    Where<Vendor.bAccountID, Equal<Current<FSSODet.vendorID>>>>
                    .SelectSingleBound(Base, new[] { line })
                    .RowCast<Address>()
                    .FirstOrDefault()
                    .With(ValidAddressFrom<Vendor.defLocationID>)
                : null;
            return vendorAddress
                ?? PXSelectJoin<Address,
                    InnerJoin<INSite, On<Address.addressID, Equal<INSite.addressID>>>,
                    Where<INSite.siteID, Equal<Current<FSSODet.siteID>>>>
                    .SelectSingleBound(Base, new[] { line })
                    .RowCast<Address>()
                    .FirstOrDefault()
                    .With(ValidAddressFrom<INSite.addressID>)
                ?? GetFromAddress(order);*/
            IAddressLocation returnAddress = null;

            if (line.SiteID != null)
            {
                returnAddress = PXSelectJoin<Address,
                                InnerJoin<INSite, On<Address.addressID, Equal<INSite.addressID>>>,
                                Where<
                                    INSite.siteID, Equal<Required<INSite.siteID>>>>
                                .Select(Base, line.SiteID)
                                .RowCast<Address>()
                                .FirstOrDefault();
            } 

            if (returnAddress == null)
            {
                returnAddress = GetFromAddress(order);
            }

            return returnAddress;

        }

        public virtual IAddressLocation GetToAddress(FSServiceOrder order)
        {
            /*if (order.WillCall == true)
                return GetFromAddress(order);
            else*/

            return (FSAddress.PK.Find(Base, order.ServiceOrderAddressID)).With(ValidAddressFrom<FSServiceOrder.serviceOrderAddressID>);
        }

        public virtual IAddressLocation GetToAddress(FSServiceOrder order, FSSODet line)
        {
            /*if (order.WillCall == true && line.SiteID != null && !(line.POCreate == true && line.POSource == INReplenishmentSource.DropShipToOrder))
                return GetFromAddress(order, line); // will call
            else*/
            return GetToAddress(order);
        }

        private IAddressLocation ValidAddressFrom<TFieldSource>(IAddressLocation address)
            where TFieldSource : IBqlField
        {
            if (!IsEmptyAddress(address)) return address;
            throw new PXException(PickAddressError<TFieldSource>(address));
        }

        private string PickAddressError<TFieldSource>(IAddressBase address)
            where TFieldSource : IBqlField
        {
            if (typeof(TFieldSource) == typeof(FSServiceOrder.serviceOrderAddressID))
                return PXSelectReadonly<FSServiceOrder, Where<FSServiceOrder.serviceOrderAddressID, Equal<Required<FSAddress.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((FSAddress)address).AddressID).First().GetItem<FSServiceOrder>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<FSServiceOrder>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(Vendor.defLocationID))
                return PXSelectReadonly<Vendor, Where<Vendor.defLocationID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<Vendor>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<Vendor>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(INSite.addressID))
                return PXSelectReadonly<INSite, Where<INSite.addressID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<INSite>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<INSite>(), new EntityHelper(Base).GetRowID(e)));

            if (typeof(TFieldSource) == typeof(BAccountR.defAddressID))
                return PXSelectReadonly<BAccountR, Where<BAccountR.defAddressID, Equal<Required<Address.addressID>>>>
                    .SelectWindowed(Base, 0, 1, ((Address)address).AddressID).First().GetItem<BAccountR>()
                    .With(e => PXMessages.LocalizeFormat(AR.Messages.AvalaraAddressSourceError, EntityHelper.GetFriendlyEntityName<BAccountR>(), new EntityHelper(Base).GetRowID(e)));

            throw new ArgumentOutOfRangeException("Unknown address source used");
        }

        protected virtual bool IsCommonCarrier(string carrierID)
        {
            if (string.IsNullOrEmpty(carrierID))
            {
                return false; //pickup;
            }
            else
            {
                Carrier carrier = PXSelect<Carrier, Where<Carrier.carrierID, Equal<Required<Carrier.carrierID>>>>.Select(Base, carrierID);
                if (carrier == null)
                {
                    return false;
                }
                else
                {
                    return carrier.IsCommonCarrier == true;
                }
            }
        }

        private void InvalidateExternalTax(FSServiceOrder order, bool keepFreight = false)
        {
            if (order == null || !IsExternalTax(order.TaxZoneID)) return;
            order.IsTaxValid = false;
            /*order.IsOpenTaxValid = false;
            order.IsUnbilledTaxValid = false;
            if (keepFreight == false)
                order.IsFreightTaxValid = false;*/
        }
    }
}
