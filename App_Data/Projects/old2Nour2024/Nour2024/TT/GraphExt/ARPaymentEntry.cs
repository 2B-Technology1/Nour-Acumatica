using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CR;
//using PX.CCProcessing;
using SOOrderEntry = PX.Objects.SO.SOOrderEntry;
using SOOrder = PX.Objects.SO.SOOrder;
using SOAdjust = PX.Objects.SO.SOAdjust;
using SOOrderType = PX.Objects.SO.SOOrderType;
using PX.Objects;
using PX.Objects.AR;
using Maintenance;
namespace PX.Objects.AR
{
    [PXCustomization]
    public class Cst_ARPaymentEntry : ARPaymentEntry
        //ARPaymentEntry_Extension:PXGraphExtension<ARPaymentEntry>
    {

        public new PXAction<ARPayment> report;


        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.Report)]
        protected override IEnumerable Report(PXAdapter adapter,
            [PXString(8)]
        [PXStringList(new string[] { "AR610500", "AR622000", "AR641000","AR020715" },
            new string[] { "AR Edit Detailed", "AR Register Detailed", "Invoice/Memo Form","Payment Info" })]
        string reportID
            )
        {
            PXReportRequiredException ex = null;
            foreach (ARPayment doc in adapter.Get<ARPayment>())
            {
                var parameters = new Dictionary<string, string>();
                if (this.Caches[typeof(ARPayment)].GetStatus(doc) == PXEntryStatus.Notchanged)
                {
                    this.Caches[typeof(ARPayment)].SetStatus(doc, PXEntryStatus.Updated);
                }

                if (reportID == "AR641000")
                {
                    parameters["ARInvoice.DocType"] = doc.DocType;
                    parameters["ARInvoice.RefNbr"] = doc.RefNbr;
                }
                else if (reportID == "AR610500")
                {
                    parameters["DocType"] = doc.DocType;
                    parameters["RefNbr"] = doc.RefNbr;
                    parameters["BranchID"] = null;
                    parameters["PeriodTo"] = null;
                    parameters["PeriodFrom"] = null;
                }
                else if (reportID == "AR622000")
                {
                    parameters["DocType"] = doc.DocType;
                    parameters["RefNbr"] = doc.RefNbr;
                    parameters["BranchID"] = null;
                    parameters["LedgerID"] = null;
                    parameters["StartPeriodID"] = null;
                    parameters["EndPeriodID"] = null;
                }
                else if (reportID == "AR020715")
                {
                    parameters["DocType"] = doc.DocType;
                    parameters["RefNbr"] = doc.RefNbr;
                }

                ex = PXReportRequiredException.CombineReport(ex, GetCustomerReportID(reportID, doc), parameters);
            }

            this.Save.Press();
            if (ex != null) throw ex;

            return adapter.Get();
        }

        #region Event Handlers
        protected override void ARPayment_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {

            base.ARPayment_CashAccountID_FieldUpdated(sender, e);
            var row = (ARPayment)e.Row;
            row.GetExtension<ARPaymentExt>().UsrRefNbr = "";
            ARPaymentEntry pmt = PXGraph.CreateInstance<ARPaymentEntry>();
            if (e.Row != null)
            {
                PXSelectBase<CashAccount> cs = new PXSelectReadonly<CashAccount,
                    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(pmt);
                cs.Cache.Clear();
                CashAccount cacc = cs.Select(row.CashAccountID);
                if (row.CashAccountID != null)
                {
                    sender.SetValue<ARPaymentExt.usrNRAccount>(row, cacc.CashAccountCD);
                    sender.SetValue<ARPaymentExt.usrCUCAccount>(row, cacc.CashAccountCD);
                }

            }

    }

        protected void ARPayment_UsrBankRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {

            ARPayment row = (ARPayment)e.Row;
            if (e.Row != null && ((ARPayment)e.Row).DocType == ARDocType.VoidPayment)
            {
                //avoid webdialog in PaymentRef attribute
                e.Cancel = true;
            }
            else
            {
                if (string.IsNullOrEmpty((string)e.NewValue) == false && (row.Status == ARDocStatus.Balanced || row.Status == ARDocStatus.Hold) && row.DocType == ARDocType.Payment)
                {

                    ARPayment dup = null;

                    dup = PXSelectReadonly<ARPayment, Where<ARPayment.customerID, Equal<Current<ARPayment.customerID>>,
                                        And<ARPaymentExt.usrBankRefNbr, Equal<Required<ARPaymentExt.usrBankRefNbr>>,
                                        And<ARPayment.voided, Equal<boolFalse>,
                                        And<ARPayment.released, Equal<True>>>>>>.Select(this, e.NewValue);

                    if (dup != null)
                    {
                        sender.RaiseExceptionHandling<ARPaymentExt.usrBankRefNbr>(e.Row, e.NewValue, new PXSetPropertyException
                            (Messages.DuplicateCustomerPayment,
                            PXErrorLevel.RowError, dup.GetExtension<ARPaymentExt>().UsrBankRefNbr, dup.DocDate, dup.DocType, dup.RefNbr));
                    }
                }
            }

        }

        protected override void ARPayment_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            base.ARPayment_RowPersisting(sender, e);
            var row = (ARPayment)e.Row;

            if (string.IsNullOrEmpty(row.GetExtension<ARPaymentExt>().UsrRefNbr) == true)
            {
                AutoSequence sequence = PXSelect<AutoSequence, Where<AutoSequence.type, Equal<Current<ARPayment.docType>>,
                                                                            And<AutoSequence.cashAccountID, Equal<Current<ARPayment.cashAccountID>>,
                                                                            And<AutoSequence.vendor, Equal<False>>>>>.Select(this);
                if (sequence != null)
                    row.GetExtension<ARPaymentExt>().UsrRefNbr = AutoNumberAttribute.GetNextNumber(sender, row, sequence.Sequence, DateTime.Now);
            }


            if (string.IsNullOrEmpty(row.GetExtension<ARPaymentExt>().UsrBankRefNbr) == false && (row.Status == ARDocStatus.Balanced || row.Status == ARDocStatus.Hold) && row.DocType == ARDocType.Payment)
            {
                ARPayment dup = PXSelectReadonly<ARPayment, Where<ARPayment.customerID, Equal<Current<ARPayment.customerID>>,
                                                            And<ARPaymentExt.usrBankRefNbr, Equal<Required<ARPaymentExt.usrBankRefNbr>>,
                                                            And<ARPayment.voided, Equal<boolFalse>,
                                                            And<ARPayment.released, Equal<True>>>>>>.Select(this, row.GetExtension<ARPaymentExt>().UsrBankRefNbr);

                if (row.DocType == ARDocType.Payment && dup != null)
                    throw new PXRowPersistingException(typeof(ARPaymentExt.usrBankRefNbr).Name, null, "Duplicate Bank Ref. No", typeof(ARPaymentExt.usrBankRefNbr).Name);
            }
        }

        protected void ARAdjust_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {

            //var row = (ARAdjust)e.Row;
            //if (row.AdjgDocType == ARDocType.Refund)
            //{
            //    //ARAdjust adjustment = PXSelect<ARAdjust, Where<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>>>.Select(this, row.RefNbr);
            //    ARPayment adjustedPayment = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>.Select(this, row.AdjdRefNbr);
            //    ARPaymentEntry paymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            //    //paymentEntry.Document.Update(adjustedPayment);
            //    paymentEntry.Document.Current.GetExtension<ARPaymentExt>().UsrRefunded = true;
            //    paymentEntry.Actions.PressSave();
            //}
        }


        #endregion


    }
    
}