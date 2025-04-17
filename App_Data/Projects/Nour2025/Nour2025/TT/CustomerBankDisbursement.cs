using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR;
using PX.Objects.CR;


namespace Maintenance
{
    public class CustomerBankDisbursement : PXGraph<CustomerBankDisbursement>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            //[PXSelector(typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrCheckDispersant, Equal<True>>>),
            //            new Type[]
            //{
            //      typeof(CashAccount.cashAccountCD),
            //  typeof(CashAccount.descr),
            //  typeof(CashAccount.curyID),
            //},

                //        DescriptionField = typeof(CashAccount.descr),
                //SubstituteKey = typeof(CashAccount.cashAccountCD))]
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
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]

            public virtual DateTime? TransactionDate { get; set; }
        }

        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        [PXFilterable]
        

        public PXFilteredProcessingJoin<ARPayment, ProcessFilter,
            InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>,
            InnerJoin<BankCheck, On<BankCheck.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>>>,
            Where<CashAccountExt.usrCheckDispersant, Equal<True>,
           And<ARPayment.docType, Equal<ARDocType.refund>,
            And<ARPayment.cashAccountID, Equal<BankCheck.checkAccountID>,
           And<ARRegisterExt.usrPayReturn,Equal<False>>>>>> Records;
                          
   

        public CustomerBankDisbursement()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            Records.SetProcessDelegate(list => ReleaseDocs(list, cashAccountID,
                                                            this.Filter.Current.TransactionDate));


        
        }

        public static void ReleaseDocs(List<ARPayment> payments, int? cashAccount, DateTime? transactionDate)
        {

            foreach (ARRegister payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                ARPayment aRPayment = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                                And<ARPayment.docType, Equal<Required<ARRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = cashAccount;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                Transfer.Transfer.Current.InAccountID = aRPayment.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = payment.GetExtension<ARRegisterExt>().UsrCheckNbr + "-Customer Disb.";
                Transfer.Transfer.SetValueExt<CATransferExt.usrType>(Transfer.Transfer.Current, payment.DocType);
                Transfer.Transfer.SetValueExt<CATransferExt.usrRefNbr>(Transfer.Transfer.Current, payment.RefNbr);
                Transfer.Transfer.Current.OutDate = transactionDate;
                Transfer.Transfer.Current.InDate = transactionDate;
                Transfer.Transfer.Current.Descr = payment.DocDesc;
                Transfer.Transfer.Update(Transfer.Transfer.Current);
                Transfer.Actions.PressSave();
                CATransfer transfer = Transfer.Transfer.Current;
                List<CARegister> list = new List<CARegister>();
                CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDIn);
                if (tran != null)
                    list.Add(CATrxRelease.CARegister(transfer, tran));
                else
                    throw new PXException("catrxrelease error");

                tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.
                    Select(Transfer, transfer.TranIDOut);
                if (tran == null)
                    throw new PXException("Tran is null");

                PXLongOperation.StartOperation(Transfer, delegate() { CATrxRelease.GroupRelease(list, false); });

                ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();

                entry.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>
                    , And<ARPayment.docType, Equal<Required<ARRegister.docType>>>>>
                    .Select(Transfer, payment.RefNbr, payment.DocType);

                entry.Document.Update(entry.Document.Current);

                CashAccount accountCD =
                    PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
                    Select(Transfer, cashAccount);

                entry.Document.Cache.SetValueExt<ARRegisterExt.usrReturnBankName>(entry.Document.Current, accountCD.CashAccountCD);
                entry.Document.Cache.SetValueExt<ARRegisterExt.usrPayReturn>(entry.Document.Current, true);
                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
                entry.Document.Cache.IsDirty = true;
                //entry.Document.Cache.PersistUpdated(entry.Document.Current);
                entry.Document.Cache.Persist(PXDBOperation.Update);

               

            }

        }
        public PXAction<ProcessFilter> ViewProduct;

        [PXButton]
        protected virtual void viewProduct()
        {
            ARRegister row = (ARRegister)Records.Cache.Current;
            // Creating the instance of the graph
            ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();
            // Setting the current product for the graph
            entry.Document.Current = entry.Document.Search<ARRegister.docType, ARRegister.refNbr>(row.DocType,row.RefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (entry.Document.Current != null)
            {
                throw new PXRedirectRequiredException(entry, true, "Details");
            }
        }
    }
}