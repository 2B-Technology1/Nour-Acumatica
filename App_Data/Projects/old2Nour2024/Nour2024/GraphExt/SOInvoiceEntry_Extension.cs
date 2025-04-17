using System;
using System.Collections;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using Maintenance;
using MyMaintaince;
using System.Text;
using PX.Data.BQL;
using System.Linq;
using PX.Data.BQL.Fluent;

namespace PX.Objects.SO
{

    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class SOInvoiceEntry_Extension : PXGraphExtension<SOInvoiceEntry>
    {
       
        #region Views

        //public PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<ARRegister.docType, Equal<Required<ARRegister.docType>>>>> document;
        //public PXSelect<SOOrder, Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>, And<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>> order;
        
        //public SelectFrom<SOOrder>
        //    .Where<SOOrder.orderNbr.IsEqual<@P.AsString>
        //        .And<SOOrder.orderType.IsEqual<@P.AsString>>>
        //    .View order;
        #endregion

        public PXAction<ARInvoice> release;
        [PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable Release(PXAdapter adapter)
        {
            ARRegister row = Base.Document.Current;
            //ARRegisterExt itemExt = PXCache<ARRegister>.
            //GetExtension<ARRegisterExt>(row);

            ARRegisterExt itemExt = row.GetExtension<ARRegisterExt>();
            
            //try
            //{

            //if( !(row is null ) && row.DocType == "CRM")
            //{
            //    ARRegister doc = document.Select(row.OrigRefNbr, row.OrigDocType);
            //    ARRegisterExt docExt = doc.GetExtension<ARRegisterExt>();

            //    if (itemExt.UsrTaxRefNbr == docExt.UsrTaxRefNbr)
            //    {
            //        itemExt.UsrTaxRefNbr = GetSerial(row.DocType);
            //    }
            //    }
            //}
            //catch (Exception)
            //{

            //}
            
            if (itemExt.UsrCustAmount != null)
            {
                NumberstoAlpha alpha = new NumberstoAlpha(itemExt.UsrCustAmount.Value.ToString(), Base.Document.Current.CuryID);
                //PXDatabase.Update<ARRegister>(
                //  new PXDataFieldAssign("UsrCustReplicate", alpha.GetNumberAr()),
                //  new PXDataFieldRestrict("RefNbr", row.RefNbr));
                //  //PXDataFieldRestrict.OperationSwitchAllowed);

                var replicate = alpha.GetNumberAr();
                row.GetExtension<ARRegisterExt>().UsrCustReplicate = replicate;
                //row.GetExtension<ARRegisterExt>().Usrarword=replicate;
                Base.Actions.PressSave();
            }
            Base.Actions.PressSave();

            return Base.Release(adapter);
        }
        
        public virtual string GetSerial(string invoiceType)
        {
            Boolean T = true;
            PXSelectBase<SetUp> t = new PXSelectReadonly<SetUp, Where<SetUp.taxSerial, Equal<Required<SetUp.taxSerial>>>>(this.Base);
            t.Cache.ClearQueryCache();
            PXResultset<SetUp> resulttran = t.Select(T);
            SetUp tline = resulttran;
            string lastNumber;
            if (invoiceType == "CRM")
            {
                lastNumber = tline.TaxCmRefnbr;
            }
            else
            {
                lastNumber = tline.TaxInvRefnbr;
            }
            char[] symbols = lastNumber.ToCharArray();
            StringBuilder taxNumber = new StringBuilder();
            for (int i = symbols.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(symbols[i]))
                    break;
                if (symbols[i] < '9')
                {
                    symbols[i]++;
                    break;
                }
                symbols[i] = '0';
            }
            foreach(char symbol in symbols)
            {
                taxNumber.Append(symbol);

            }
            return taxNumber.ToString();
        }

        #region Event Handlers


        #region Document Row Selected

        protected void _(Events.RowSelected<ARInvoice> e)
        {

            ARRegister row = e.Row;
            
            ARRegisterExt rowExt = row.GetExtension<ARRegisterExt>();
            string refNbr = row.RefNbr;
            string tranType = row.DocType;

            Base.Document.Cache.SetValue<ARRegisterExt.usrCustAmount>(row, row.CuryOrigDocAmt);            
            ARTran tran = Base.Transactions.SelectSingle(refNbr, tranType);

            if (!(tran is null) && rowExt.UsrPurchaseOrderDescription is null && rowExt.UsrPurchaseOrderReference is null && rowExt.UsrSalesOrderReference is null)
            {

                string orderNbr = tran.SOOrderNbr;
                string orderType = tran.SOOrderType;

                SOOrder soOrder = SelectFrom<SOOrder>
                                     .Where<SOOrder.orderNbr.IsEqual<@P.AsString>
                                        .And<SOOrder.orderType.IsEqual<@P.AsString>>>
                                        .View.Select(Base,orderNbr, orderType);
                SOOrderExt orderExt = soOrder.GetExtension<SOOrderExt>();

                Base.Document.Cache.SetValue<ARRegisterExt.usrPurchaseOrderDescription>(row, orderExt.UsrPurchaseOrderDescription);
                Base.Document.Cache.SetValue<ARRegisterExt.usrPurchaseOrderReference>(row, orderExt.UsrPurchaseOrderReference);
                Base.Document.Cache.SetValue<ARRegisterExt.usrSalesOrderReference>(row, orderExt.UsrSalesOrderReference);
                

            }
        }


        #endregion

        protected void ARInvoice_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            try
            {
                var row = (ARInvoice)e.Row;
                BAccount cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this.Base, row.CustomerID);

                if (cust != null)
                {


                    row.GetExtension<ARRegisterExt>().UsrPtsBalance = cust.GetExtension<BAccountExt>().UsrLoyPoints; //null ref error.. will fix it...


                }
            }
            catch (Exception)
            {

                throw new PXException("customerid fieldupdated exception.");
            }
        }


        protected void ARInvoice_CuryOrigDocAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ARInvoice)e.Row;

            //--convert CuryOrigDocAmt to points
            row.GetExtension<ARRegisterExt>().UsrLoyPoints = (int)row.CuryOrigDocAmt;
        }


        protected void ARInvoice_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            /*var row = (ARInvoice)e.Row;
            SOInvoiceEntry SO = PXGraph.CreateInstance<SOInvoiceEntry>();
            AccessInfo Accessinfo =SO.Accessinfo;
            row.DocDate=Accessinfo.BusinessDate;
            SO.Actions.PressSave();*/
        }

        string noData = "No Data to print";

        protected void ARInvoice_UsrRedPts_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ARInvoice)e.Row;

            //--convert pts to cash(discount)
            row.GetExtension<ARRegisterExt>().UsrRedEqv = row.GetExtension<ARRegisterExt>().UsrRedPts / 100;

            //--convert CuryOrigDocAmt to points
            row.GetExtension<ARRegisterExt>().UsrLoyPoints = (int)row.CuryOrigDocAmt;
            //cache.SetValueExt<ARRegisterExt.usrLoyPoints>(row, (int)row.CuryOrigDocAmt);


            //--update the cash discount to the points equiv amt:
            cache.SetValue<ARRegister.curyOrigDiscAmt>(row, row.GetExtension<ARRegisterExt>().UsrRedEqv);
            row.CuryOrigDiscAmt = row.GetExtension<ARRegisterExt>().UsrRedEqv;

            if (row.GetExtension<ARRegisterExt>().UsrPtsBalance != 0)
            {
                if (row.GetExtension<ARRegisterExt>().UsrPtsBalance < row.GetExtension<ARRegisterExt>().UsrRedPts)
                {
                    cache.SetValueExt<ARRegisterExt.usrRedPts>(row, 0); //here no need to update UsrRedEqv,  CuryOrigDiscAmt
                    row.GetExtension<ARRegisterExt>().UsrRedPts = 0; //also need to update UsrRedEqv
                    row.GetExtension<ARRegisterExt>().UsrRedEqv = 0;
                    //throw new PXException ("Redemption points cannot exceed customer balance.");
                }
            }

            //this.Base.RecalculateDiscounts(this.Base.Transactions.Cache,row);
            //this.Base.Transcations.Update(row);
        }

        #endregion


        #region Print Buttons

        public PXAction<PX.Objects.AR.ARInvoice> PrintSalesInvoice;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Sales Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printSalesInvoice()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "MG000041", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }


        public PXAction<PX.Objects.AR.ARInvoice> PrintInvoicIncurance;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Incurance Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printInvoicIncurance()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "HO000041", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        public PXAction<PX.Objects.AR.ARInvoice> PrintTrafficLetter;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Traffic Letter", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printTrafficLetter()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "MG000039", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        public PXAction<PX.Objects.AR.ARInvoice> PrintTrafficLetterFinal;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Traffic Letter Final", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printTrafficLetterFinal()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "MG000036", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        public PXAction<PX.Objects.AR.ARInvoice> PrintBanCancel;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Ban Cancel", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printBanCancel()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "MG000037", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        public PXAction<PX.Objects.AR.ARInvoice> PrintLicenseRenew;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print License Renew", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected void printLicenseRenew()
        {
            ARInvoice inv = this.Base.Document.Current;
            if (inv.RefNbr != " <NEW>")
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();  //OrderType OrderNbr
                parameters["DocType"] = inv.DocType;
                parameters["RefNbr"] = inv.RefNbr;
                throw new PXReportRequiredException(parameters, "MG000038", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        #endregion

       
    }

}