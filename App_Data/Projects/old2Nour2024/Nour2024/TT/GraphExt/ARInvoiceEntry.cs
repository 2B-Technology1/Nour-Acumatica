using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Common;
using PX.Objects.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.BQLConstants;
using PX.Objects.EP;
using PX.Objects.SO;
using PX.Objects.DR;
//using Avalara.AvaTax.Adapter;
//using Avalara.AvaTax.Adapter.TaxService;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOInvoiceEntry = PX.Objects.SO.SOInvoiceEntry;
//using AvaAddress = Avalara.AvaTax.Adapter.AddressService;
//using AvaMessage = Avalara.AvaTax.Adapter.Message;
using CRLocation = PX.Objects.CR.Standalone.Location;
using MyMaintaince;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;

namespace PX.Objects.AR
{

    public class ARInvoiceEntry_Extension : PXGraphExtension<ARInvoiceEntry>
    {
        #region Views

        //public PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<ARRegister.docType, Equal<Required<ARRegister.docType>>>>> searchDoc;
        //public PXSelect<ARTran, Where<ARTran.refNbr, Equal<Required<ARTran.refNbr>>, And<ARTran.tranType, Equal<Required<ARTran.tranType>>>>> searchLines;

        //public PXSelect<INRegister, Where<INRegisterExt.usrOrderID, Equal<Required<INRegisterExt.usrOrderID>>>> IN;
        //public PXSelect<INItemCost, Where<INItemCost.inventoryID, Equal<Required<INItemCost.inventoryID>>>> costs;



        //public SelectFrom<ARRegister>
        //        .Where<ARRegister.refNbr.IsEqual<@P.AsString>
        //            .And<ARRegister.docType.IsEqual<@P.AsString>>>
        //        .View searchDoc;

        //public SelectFrom<ARTran>
        //    .Where<ARTran.refNbr.IsEqual<@P.AsString>
        //        .And<ARTran.tranType.IsEqual<@P.AsString>>>
        //    .View searchLines;

        //public SelectFrom<INRegister>
        //    .Where<INRegisterExt.usrOrderID.IsEqual<@P.AsString>>
        //    .View IN;

        public SelectFrom<INItemCost>
            .Where<INItemCost.inventoryID.IsEqual<@P.AsInt>>
            .View costs;


        #endregion

        #region MyButtons (MMK)
        public PXSelect<ARInvoice> arinv;
        public PXAction<ARInvoice> PrintInvoice;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Print Invoice")]
        protected void printInvoice()
        {

            ARInvoice Av = this.arinv.Current;


            if (Av != null)
            {
                if (Av.Status != "C")
                {
                    throw new PXException(" Please Payment this invoice first");
                }
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters["RefNbr"] = Av.RefNbr;
                parameters["DocType"] = Av.DocType;
                throw new PXReportRequiredException(parameters, "NO700005", PXBaseRedirectException.WindowMode.New, null);

            }
            else
            {
                throw new PXException(" No Data Found To print");
            }
        }

        string noData = "No Data to print";


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
                throw new PXReportRequiredException(parameters, "MG000040", PXBaseRedirectException.WindowMode.New, null);
            }
            else
            {
                throw new PXException(noData);
            }
        }

        #endregion


        //#region Release

        //public delegate ARRegister ReleaseDelegate(ARRegister doc);
        //[PXOverride]
        //public ARRegister Release(ARRegister doc, ReleaseDelegate baseMethod)
        //{

        //    ARInvoice inv = this.Base.Document.Current;
        //    var maint = PXGraph.CreateInstance<ARInvoiceEntry>();
        //    if (this.arinv.Current.GetExtension<ARRegisterExt>().UsrTaxPortal == true)
        //    {
        //        string Otype = inv.DocType;
        //        char[] symbols = GetSerial(Otype);
        //        this.arinv.Current.GetExtension<ARRegisterExt>().UsrTaxRefNbr = new string(symbols);
        //        if (Otype == "CRM")
        //        {
        //            maint.ProviderUpdate<SetUp>(new PXDataFieldAssign("taxCMRefnbr", this.arinv.Current.GetExtension<ARRegisterExt>().UsrTaxRefNbr), new PXDataFieldRestrict("TaxSerial", true));
        //        }
        //        else
        //        {
        //            maint.ProviderUpdate<SetUp>(new PXDataFieldAssign("taxINVRefnbr", this.arinv.Current.GetExtension<ARRegisterExt>().UsrTaxRefNbr), new PXDataFieldRestrict("TaxSerial", true));

        //        }
        //    }
        //    this.Base.Actions.PressSave();


        //    return baseMethod(doc);

        //}
        //#endregion
        //public delegate IEnumerable ReleaseDelegate(PXAdapter adapter);
        ////[PXUIField(DisplayName = "Prepare Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        //[PXOverride]
        //public override IEnumerable Release(PXAdapter adapter)
        //{


        //    return baseMethod(adapter);
        //}


        #region Event Handlers

        #region ARInvoice RowPersisting

        protected void ARInvoice_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            try
            {

                bool isInserting = (e.Operation & PXDBOperation.Insert) == PXDBOperation.Insert;
                bool isUpdating = (e.Operation & PXDBOperation.Update) == PXDBOperation.Update;

                ARRegister row = (ARInvoice)e.Row;
                ARRegisterExt rowExt = row.GetExtension<ARRegisterExt>();
                if (row.DocType == "CRM" && rowExt.UsrTaxPortal == true)
                {
                    ARRegister invoiceInfo = SelectFrom<ARRegister>
                                                .Where<ARRegister.refNbr.IsEqual<@P.AsString>
                                                    .And<ARRegister.docType.IsEqual<@P.AsString>>>
                                                .View.Select(Base,row.OrigRefNbr, row.OrigDocType);

                    ARRegisterExt invoiceInfoExt = invoiceInfo.GetExtension<ARRegisterExt>();

                    if (invoiceInfoExt.UsrTaxRefNbr == rowExt.UsrTaxRefNbr && invoiceInfoExt.UsrTaxPortal == true && rowExt.UsrTaxPortal == true)
                    {
                        cache.SetValueExt<ARRegisterExt.usrTaxRefNbr>(row, null);
                        if (isInserting)
                            cache.SetStatus(row, PXEntryStatus.Inserted);
                        else if (isUpdating)
                            cache.SetStatus(row, PXEntryStatus.Updated);
                    }
                }

            }
            catch (Exception)
            {

            }

        }

        #endregion

        #region Unit Price Field Updated

        protected void ARTran_CuryUnitPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

        }

        #endregion

        #endregion

        #region Invoice Row Selected

        protected void ARTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            try
            {

                ARTran row = (ARTran)e.Row;

                var doc = this.Base.Document.Current;

                var lines = SelectFrom<ARTran>
                                .Where<ARTran.refNbr.IsEqual<@P.AsString>
                                    .And<ARTran.tranType.IsEqual<@P.AsString>>>
                                .View.Select(Base,doc.RefNbr, doc.DocType);

                if (row.SOOrderNbr is null)
                {
                    foreach (ARTran line in lines)
                    {
                        INItemCost cost = SelectFrom<INItemCost>
                                            .Where<INItemCost.inventoryID.IsEqual<@P.AsInt>>
                                            .View.Select(Base,line.InventoryID);
                        if (!(cost is null) && (cost.TotalCost / cost.QtyOnHand) > line.UnitPrice)
                        {
                            PXUIFieldAttribute.SetWarning<ARTran.curyUnitPrice>(cache, line, "Minimum Gross Profit requirement is not satisfied.");

                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        public virtual char[] GetSerial(string orderType)
        {
            Boolean T = true;
            PXSelectBase<SetUp> t = new PXSelectReadonly<SetUp, Where<SetUp.taxSerial, Equal<Required<SetUp.taxSerial>>>>(this.Base);
            t.Cache.ClearQueryCache();
            PXResultset<SetUp> resulttran = t.Select(T);
            SetUp tline = resulttran;
            string lastNumber;
            if (orderType == "CRM")
            {
                lastNumber = tline.TaxCmRefnbr;
            }
            else
            {
                lastNumber = tline.TaxInvRefnbr;
            }
            char[] symbols = lastNumber.ToCharArray();
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
            return symbols;

        }

    }


}