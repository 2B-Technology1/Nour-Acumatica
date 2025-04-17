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
    public class VoidCheckBeforeTranProcess : PXGraph<VoidCheckBeforeTranProcess>
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
            //[PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]

            public virtual DateTime? TransactionDate { get; set; }
        }

        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        public PXAction<ProcessFilter> ViewTransaction;

        [PXButton]
        protected virtual void viewProduct()
        {
            CAAdj row =(CAAdj) Records.Cache.Current;
            // Creating the instance of the graph
            CashTransferEntry graph = PXGraph.CreateInstance<CashTransferEntry>();
            // Setting the current product for the graph
            graph.Transfer.Current = graph.Transfer.Search<CAAdj.adjRefNbr>(
                                         row.AdjRefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (graph.Transfer.Current != null)
            {
                throw new PXRedirectRequiredException(graph, true, "Details");
            }
        }

        [PXFilterable]
        public PXFilteredProcessingJoin<CAAdj, ProcessFilter,
            InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CAAdj.cashAccountID>>,
                InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CAAdj.entryTypeID>>,
                    LeftJoin<Banks, On<CAAdjExt.usrBankName, Equal<Banks.bankID>>,
                        LeftJoin<BankCheck, On<BankCheck.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>>>>>,
            Where<CashAccountExt.usrCheckDispersant, Equal<True>,
                And<CAAdjExt.usrCheck, Equal<False>,
                    And<CAAdj.cashAccountID, Equal<BankCheck.checkAccountID>,
                        And<CAEntryType.drCr, Equal<CADrCr.cACredit>,
                            And<CAAdj.released, Equal<True>>>>>>> Records;

        public VoidCheckBeforeTranProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            DateTime? d = this.Filter.Current.TransactionDate;
            //Records.SetProcessDelegate(list => ReleaseDocs(list, cashAccountID,
            //                                                this.Filter.Current.TransactionDate));
        }

        //public static void ReleaseDocs(List<CAAdj> payments, int? cashAccount, DateTime? transactionDate)
        //{

        //    //    foreach (CAAdj payment in payments)
        //    //    {
        //    //        CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
        //    //        APPayment aPPayment = PXSelectReadonly<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>,
        //    //                                      And<APPayment.docType, Equal<Required<APRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
        //    //        CashAccount outAccount = PXSelect<CashAccount,
        //    //            Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(Transfer, aPPayment.CashAccountID);
        //    //        CashAccount inAccount = PXSelect<CashAccount,
        //    //            Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, aPPayment.GetExtension<APRegisterExt>().UsrCheckDispersant);
        //    //        Transfer.Transfer.Insert();
        //    //        Transfer.Transfer.Current.OutAccountID = cashAccount;// outAccount.CashAccountID;
        //    //        Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
        //    //        Transfer.Transfer.Current.InAccountID = cashAccount;
        //    //        Transfer.Transfer.Current.OutExtRefNbr = "Void Check Trans";
        //    //        Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrType = payment.DocType;
        //    //        Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrRefNbr = payment.RefNbr;
        //    //        Transfer.Transfer.Current.OutDate = transactionDate;
        //    //        Transfer.Transfer.Current.InDate = transactionDate;
        //    //        Transfer.Transfer.Current.Descr = payment.DocDesc;
        //    //        Transfer.Transfer.Update(Transfer.Transfer.Current);
        //    //        Transfer.Actions.PressSave();
        //    //        CATransfer transfer = Transfer.Transfer.Current;
        //    //        List<CARegister> list = new List<CARegister>();
        //    //        CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDIn);
        //    //        if (tran != null)
        //    //            list.Add(CATrxRelease.CARegister(transfer, tran));
        //    //        else
        //    //            throw new PXException("catrxrelease error");

        //    //        tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDOut);
        //    //        if (tran == null)
        //    //            throw new PXException("tran is null");

        //    //        PXLongOperation.StartOperation(Transfer, delegate () { CATrxRelease.GroupRelease(list, false); });

        //    //        APPaymentEntry entry = PXGraph.CreateInstance<APPaymentEntry>();
        //    //        entry.Document.Current = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>,
        //    //                                      And<APPayment.docType, Equal<Required<APRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
        //    //        entry.Document.Update(entry.Document.Current);

        //    //        entry.Document.Cache.SetValueExt<APRegisterExt.usrCheckDispersant>(entry.Document.Current, null);
        //    //        entry.Document.Cache.SetValueExt<APRegisterExt.usrChecked>(entry.Document.Current, true);
        //    //        entry.Document.Cache.SetValueExt<APRegisterExt.usrRefunded>(entry.Document.Current, true);
        //    //        entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
        //    //        entry.Document.Cache.IsDirty = true;
        //    //        entry.Document.Cache.Persist(PXDBOperation.Update);

        //    //        //entry.Actions.PressSave();

        //    //    }

        //    }
        }
}