using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CR;


namespace Maintenance
{
    public class VoidCheckBeforeProcess : PXGraph<VoidCheckProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector(typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrBank, Equal<True>>>),
                        new Type[]
            {
                  typeof(CashAccount.cashAccountCD),
              typeof(CashAccount.descr),
              typeof(CashAccount.curyID),
            },
                        DescriptionField = typeof(CashAccount.descr),
                SubstituteKey = typeof(CashAccount.cashAccountCD))]
            public virtual int? CashAccountID { get; set; }

            public abstract class transactionDate : PX.Data.IBqlField
            {
            }
            [PXDate()]
           // [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]
            public virtual DateTime? TransactionDate { get; set; }
        }

        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;
       
        [PXFilterable]
        public PXFilteredProcessingJoin<APRegister, ProcessFilter,
                                InnerJoin<APPayment, On<APRegister.refNbr, Equal<APPayment.refNbr>,
                                          And<APRegister.docType, Equal<APPayment.docType>>>,
                                InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<APPayment.cashAccountID>>,
                                InnerJoin<BAccount, On<APRegister.vendorID, Equal<BAccount.bAccountID>>,
                                    LeftJoin<CATransfer, On<CATransferExt.usrType, Equal<APPayment.docType>,
                                                   And<CATransferExt.usrRefNbr, Equal<APPayment.refNbr>>>>>>>,
                                    Where<APRegisterExt.usrChecked, Equal<False>,
                                    And<APRegisterExt.usrRefunded, Equal<False>,
                                    And<Where<APRegister.docType, Equal<APDocType.check>,
                                            Or<APRegister.docType, Equal<APDocType.prepayment>>>>>>> Records;

        public PXAction<ProcessFilter> ViewProduct;

        [PXButton]
        protected virtual void viewProduct()
        {
            APRegister row = (APRegister)Records.Cache.Current;
            // Creating the instance of the graph
            APPaymentEntry entry = PXGraph.CreateInstance<APPaymentEntry>();
            // Setting the current product for the graph
            entry.Document.Current = entry.Document.Search<APPayment.refNbr>(row.RefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (entry.Document.Current != null)
            {
                throw new PXRedirectRequiredException(entry, true, "Details");
            }
        }

        public VoidCheckBeforeProcess()
        {
            DateTime? d = this.Filter.Current.TransactionDate;
            Records.SetProcessDelegate(list => ReleaseDocs(list, d));
        }

        //public PXSelect<APRegister, Where<APRegister.released, Equal<True>>> APRecords;
        //[PXViewName(PX.Objects.AP.Messages.APAdjust)]
        //[PXCopyPasteHiddenView]
        //public PXSelectJoin<APAdjust, LeftJoin<APInvoice, 
        //    On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>, 
        //    Where<APAdjust.adjgDocType, Equal<Current<APPayment.docType>>, 
        //        And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>, 
        //            And<APAdjust.adjNbr, Equal<Current<APPayment.lineCntr>>>>>> Adjustments;

        //[PXCopyPasteHiddenView]
        //public PXSelectJoin<
        //    APAdjust,
        //    LeftJoin<APInvoice,
        //        On<APInvoice.docType, Equal<APAdjust.adjdDocType>,
        //        And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>,
        //    Where<
        //        APAdjust.adjgDocType, Equal<Current<APPayment.docType>>,
        //        And<APAdjust.adjgRefNbr, Equal<Current<APPayment.refNbr>>,
        //        And<APAdjust.adjNbr, Less<Current<APPayment.lineCntr>>>>>>
        //    Adjustments_History;
        //public void X()
        //{
        //    APRegister current = APRecords.Current;

        //    //if (current.Released != true)
        //    //    return adapter.Get();

        //    APRegister rec = (APRegister)APRecords.Cache.CreateCopy(APRecords.Current);
        //    rec.RefNbr = null;
        //    rec.Status = null;
        //    rec.Approved = null;
        //    rec.Hold = null;
        //    rec.Released = null;
        //    rec.NoteID = null;
        //    rec.OrigDocAmt *= -1;
        //    rec.ChargeAmt *= -1;
        //    rec.CuryOrigDiscAmt *= -1;
        //    rec.CuryOrigDocAmt *= -1;
        //    rec.CuryOrigWhTaxAmt *= -1;
        //    rec.OrigDiscAmt *= -1;
        //    rec.OrigWhTaxAmt *= -1;
        //    rec.RGOLAmt *= -1;
        //    rec.EmployeeID = null;
        //    rec.DocType = "REF";

        //    ////detail definition //Adjustments
        //    List<Tuple<APAdjust, APAdjust>> splits = new List<Tuple<APAdjust, APAdjust>>();
        //    foreach (APAdjust split in Adjustments.Select())
        //    {
        //        APAdjust newSplit = (APAdjust)Adjustments.Cache.CreateCopy(split);
        //        newSplit.DisplayRefNbr = null;
        //        newSplit.NoteID = null;
        //        newSplit.AdjAmt *= -1;
        //        newSplit.AdjDiscAmt *= -1;
        //        newSplit.AdjWhTaxAmt *= -1;
        //        newSplit.CuryAdjdAmt *= -1;
        //        newSplit.CuryAdjdDiscAmt *= -1;
        //        newSplit.DisplayCuryAmt *= -1;
        //        newSplit.RGOLAmt *= -1;
        //        splits.Add(new Tuple<APAdjust, APAdjust>(split, newSplit));
        //    }

        //    ////further processing //Adjustments_History
        //    //List<CATaxTran> taxes = new List<CATaxTran>();
        //    //foreach (CATaxTran taxTran in Taxes.Select())
        //    //{
        //    //    CATaxTran newTaxTran = new CATaxTran();
        //    //    newTaxTran.AccountID = taxTran.AccountID;
        //    //    newTaxTran.BranchID = taxTran.BranchID;
        //    //    newTaxTran.FinPeriodID = taxTran.FinPeriodID;
        //    //    newTaxTran.SubID = taxTran.SubID;
        //    //    newTaxTran.TaxBucketID = taxTran.TaxBucketID;
        //    //    newTaxTran.TaxID = taxTran.TaxID;
        //    //    newTaxTran.TaxType = taxTran.TaxType;
        //    //    newTaxTran.TaxZoneID = taxTran.TaxZoneID;
        //    //    newTaxTran.TranDate = taxTran.TranDate;
        //    //    newTaxTran.VendorID = taxTran.VendorID;
        //    //    newTaxTran.CuryID = taxTran.CuryID;
        //    //    newTaxTran.Description = taxTran.Description;
        //    //    newTaxTran.NonDeductibleTaxRate = taxTran.NonDeductibleTaxRate;
        //    //    newTaxTran.TaxRate = taxTran.TaxRate;
        //    //    newTaxTran.CuryTaxableAmt = -taxTran.CuryTaxableAmt;
        //    //    newTaxTran.CuryTaxAmt = -taxTran.CuryTaxAmt;
        //    //    newTaxTran.CuryExpenseAmt = -taxTran.CuryExpenseAmt;
        //    //    newTaxTran.TaxableAmt = -taxTran.TaxableAmt;
        //    //    newTaxTran.TaxAmt = -taxTran.TaxAmt;
        //    //    newTaxTran.ExpenseAmt = -taxTran.ExpenseAmt;

        //    //    taxes.Add(newTaxTran);
        //    //}
        //    CATranEntry catr = PXGraph.CreateInstance<CATranEntry>();

        //    catr.Clear();
        //    //reversingContext = true;
        //    APRegister insertedAdj = APRecords.Insert(rec);

        //    //PXNoteAttribute.CopyNoteAndFiles(APRecords.Cache, current, APRecords.Cache, insertedAdj);
        //    //foreach (Tuple<CASplit, CASplit> pair in splits)
        //    //{
        //    //    CASplit newSplit = pair.Item2;
        //    //    newSplit = CASplitRecords.Insert(newSplit);
        //    //    PXNoteAttribute.CopyNoteAndFiles(CASplitRecords.Cache, pair.Item1, CASplitRecords.Cache, newSplit);
        //    //}
        //    //reversingContext = false;
        //    //foreach (CATaxTran newTaxTran in taxes)
        //    //{
        //    //    Taxes.Insert(newTaxTran);
        //    //}
        //    ////We should reenter totals depending on taxes as TaxAttribute does not recalculate them if externalCall==false
        //    //APRecords.Cache.SetValue<CAAdj.taxRoundDiff>(insertedAdj, adj.TaxRoundDiff);
        //    //APRecords.Cache.SetValue<CAAdj.curyTaxRoundDiff>(insertedAdj, adj.CuryTaxRoundDiff);
        //    //APRecords.Cache.SetValue<CAAdj.taxTotal>(insertedAdj, adj.TaxAmt);
        //    //APRecords.Cache.SetValue<CAAdj.curyTaxTotal>(insertedAdj, adj.CuryTaxAmt);
        //    //APRecords.Cache.SetValue<CAAdj.tranAmt>(insertedAdj, adj.TranAmt);
        //    //APRecords.Cache.SetValue<CAAdj.curyTranAmt>(insertedAdj, adj.CuryTranAmt);
        //    //List<CAAdj> ret = new List<CAAdj> { insertedAdj };
        //    ////return ret;
        //}
        
        public static void ReleaseDocs(List<APRegister> payments, DateTime? transactionDate)
        {
            
        }



        //releaseDocs
        //    //APPaymentEntry apmt = PXGraph.CreateInstance<APPaymentEntry>();
        //    ////ARPayment arPayment = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
        //    ////                                  And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>
        //    ////                                  .Select(bankDeposit, payment.RefNbr, payment.DocType);
        //    //APRegister regResult = PXSelect<APRegister,
        //    //        Where<APRegister.docType, Equal<APDocType.refund>>,
        //    //        OrderBy<Desc<APRegister.refNbr>>>.Select(apmt);
        //    //APRegister pmtResult = PXSelect<APPayment,
        //    //        Where<APPayment.docType,Equal<APDocType.refund>>,
        //    //        OrderBy<Desc<APPayment.refNbr>>>.Select(apmt);
        //    //string lastNumber, newRef;
        //    //newRef = "000";
        //    //char[] ch;
        //    //if (pmtResult != null)
        //    //{
        //    //    lastNumber = pmtResult.RefNbr;
        //    //    ch = lastNumber.ToCharArray();
        //    //    for (int i = ch.Length - 1; i >= 0; i--)
        //    //    {
        //    //        if (!char.IsDigit(ch[i])) break;
        //    //        if (ch[i] < '9') { ch[i]++; break; }
        //    //        ch[i] = '0';

        //    //    }
        //    //    newRef = new string(ch);
        //    //}


        //    //apmt.ProviderInsert<APRegister>
        //    //    (
        //    //    new PXDataFieldAssign("CashAccountID", apmt.CashAcctDetail_AccountID),
        //    //    new PXDataFieldAssign("DocType", "REF"),
        //    //    /**/new PXDataFieldAssign("RefNbr", newRef.ToString()),
        //    //    new PXDataFieldAssign("VendorID", regResult.VendorID),//PaymentMethodID,CATranID,
        //    //    new PXDataFieldAssign("VendorLocationID", regResult.VendorLocationID),
        //    //    new PXDataFieldAssign("APAccountID", regResult.APAccountID),
        //    //    new PXDataFieldAssign("APSubID", regResult.APSubID),
        //    //    new PXDataFieldAssign("DocDesc", regResult.DocDesc),
        //    //    new PXDataFieldAssign("CuryInfoID", regResult.CuryInfoID),
        //    //    new PXDataFieldAssign("OrigDocAmt", regResult.OrigDocAmt),
        //    //    new PXDataFieldAssign("CuryDocBal", regResult.CuryDocBal),
        //    //    new PXDataFieldAssign("DocBal", regResult.DocBal),
        //    //    new PXDataFieldAssign("DiscTot", regResult.DiscTot),
        //    //    new PXDataFieldAssign("CuryDiscTot", regResult.CuryDiscTot),
        //    //    new PXDataFieldAssign("CuryDiscBal", regResult.CuryDiscBal),
        //    //    new PXDataFieldAssign("CuryDiscTaken", regResult.CuryDiscTaken),
        //    //    new PXDataFieldAssign("DiscTaken", regResult.DiscTaken),
        //    //    new PXDataFieldAssign("CuryChargeAmt", regResult.CuryChargeAmt),
        //    //    new PXDataFieldAssign("ChargeAmt", regResult.ChargeAmt),
        //    //    new PXDataFieldAssign("OpenDoc", regResult.OpenDoc),
        //    //    new PXDataFieldAssign("Released", regResult.Released),
        //    //    new PXDataFieldAssign("Hold", regResult.Hold),
        //    //    new PXDataFieldAssign("Status", regResult.Status),
        //    //    new PXDataFieldAssign("LineCntr", regResult.LineCntr),
        //    //    new PXDataFieldAssign("CuryID", regResult.CuryID),
        //    //    new PXDataFieldAssign("CreatedByID", apmt.UID),
        //    //    new PXDataFieldAssign("CreatedByScreenID", apmt.Document.Current.CreatedByScreenID),
        //    //    new PXDataFieldAssign("CreatedDateTime", DateTime.Now),
        //    //    new PXDataFieldAssign("LastModifiedByID", apmt.UID),
        //    //    new PXDataFieldAssign("LastModifiedByScreenID", apmt.Document.Current.CreatedByScreenID),
        //    //    new PXDataFieldAssign("LastModifiedDateTime", DateTime.Now),
        //    //    new PXDataFieldAssign("RGOLAmt", regResult.RGOLAmt),
        //    //    new PXDataFieldAssign("Voided", 1),
        //    //    new PXDataFieldAssign("Approved", 1),
        //    //    new PXDataFieldAssign("Rejected", 0),
        //    //    new PXDataFieldAssign("DocDate", DateTime.Now),
        //    //    new PXDataFieldAssign("OrigDocDate", regResult.OrigDocDate),
        //    //    new PXDataFieldAssign("AdjDate", DateTime.Now),
        //    //    new PXDataFieldAssign("AdjFinPeriodID", regResult.FinPeriodID),
        //    //    new PXDataFieldAssign("AdjTranPeriodID", regResult.TranPeriodID),
        //    //    new PXDataFieldAssign("ClosedFinPeriodID", regResult.ClosedFinPeriodID),
        //    //    new PXDataFieldAssign("ClosedTranPeriodID", regResult.ClosedTranPeriodID),
        //    //    new PXDataFieldAssign("Cleared", 1),
        //    //    new PXDataFieldAssign("ClearDate", DateTime.Now)
        //    //    );


        //    //apmt.ProviderInsert<APPayment>
        //    //    (
        //    //    new PXDataFieldAssign("CashAccountID", apmt.CashAcctDetail_AccountID),
        //    //    new PXDataFieldAssign("DocType", "REF"),
        //    //    /**/new PXDataFieldAssign("RefNbr", newRef.ToString()),
        //    //    new PXDataFieldAssign("ExtRefNbr", regResult.FinPeriodID),//CashAccountID,PaymentMethodID,CATranID,
        //    //    new PXDataFieldAssign("AdjDate", DateTime.Now),
        //    //    new PXDataFieldAssign("AdjFinPeriodID", apmt.FINPERIOD.FinPeriodID),//year+month of today 
        //    //    new PXDataFieldAssign("AdjTranPeriodID", apmt.FINPERIOD.FinPeriodID),//??
        //    //    new PXDataFieldAssign("RemitAddressID", 0),
        //    //    new PXDataFieldAssign("RemitContactID", 0),
        //    //    new PXDataFieldAssign("Cleared", 1),
        //    //    new PXDataFieldAssign("ClearDate", DateTime.Now)
        //    //    );
    }
}