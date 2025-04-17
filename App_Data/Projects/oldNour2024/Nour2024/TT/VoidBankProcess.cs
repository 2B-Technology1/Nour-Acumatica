using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CR;


namespace Maintenance
{
    public class VoidBankProcess : PXGraph<VoidBankProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {

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
        public PXFilteredProcessingJoin<ARRegister, ProcessFilter,
                                InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
                                                        And<ARRegister.docType, Equal<ARPayment.docType>>>,
                                InnerJoin<CashAccount, On<ARPayment.cashAccountID, Equal<CashAccount.cashAccountID>>,
                                LeftJoin<Banks, On<ARRegisterExt.usrBankName, Equal<Banks.bankID>>,
                                LeftJoin<CADeposit, On<CADeposit.refNbr, Equal<ARRegisterExt.usrBankDepNbr>>,
                                InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>>>>>>,
                                    Where<ARRegisterExt.usrBankDeposited, Equal<True>,
                                            And<ARRegisterExt.usrRefunded, Equal<False>,
                                            And<Where<ARRegister.docType, Equal<ARDocType.payment>,
                                                Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>>> Records;



        public VoidBankProcess()
        {
            DateTime? d = this.Filter.Current.TransactionDate;
            Records.SetProcessDelegate(list => ReleaseDocs(list, d));
        }

        public static void ReleaseDocs(List<ARRegister> payments, DateTime? transactionDate)
        {


            foreach (ARRegister payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();
                entry.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>,
                                                 And<ARPayment.docType, Equal<Required<ARRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }
                CashAccount outAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, payment.GetExtension<ARPaymentExt>().UsrBankAccount);
                CashAccount inAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, payment.GetExtension<ARPaymentExt>().UsrCUCAccount);
                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = outAccount.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                Transfer.Transfer.Current.InAccountID = inAccount.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = "Void Bank Payment";
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrType = payment.DocType;
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrRefNbr = payment.RefNbr;
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

                tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDOut);
                if (tran == null)
                    throw new PXException("tran is null");
                try
                {
                    PXLongOperation.StartOperation(Transfer, delegate() { CATrxRelease.GroupRelease(list, false); });
                }
                catch
                { }


                //entry.Document.SetValueExt<ARPaymentExt.usrCUC>(entry.Document.Current, true);
                entry.Document.SetValueExt<ARPaymentExt.usrBankAccount>(entry.Document.Current, "");
                entry.Document.SetValueExt<ARPaymentExt.usrBankDeposited>(entry.Document.Current, false);
                entry.Document.SetValueExt<ARPayment.extRefNbr>(entry.Document.Current, "000");

                entry.Document.Cache.IsDirty = true;
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
            entry.Document.Current = entry.Document.Search<ARRegister.refNbr>(row.RefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (entry.Document.Current != null)
            {
                throw new PXRedirectRequiredException(entry, true, "Details");
            }
        }
    }
}