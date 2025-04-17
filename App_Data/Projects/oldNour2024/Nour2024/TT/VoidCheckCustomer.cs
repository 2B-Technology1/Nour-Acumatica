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
    public class VoidCheckCustomer : PXGraph<VoidCheckCustomer>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {

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


        [PXFilterable]
        //public PXFilteredProcessingJoin<ARRegister, ProcessFilter,
        //                        InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
        //                                  And<ARRegister.docType, Equal<ARPayment.docType>>>,
        //                        InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>,
        //                        InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>,
        //                            InnerJoin<CATransfer, On<CATransferExt.usrType, Equal<ARPayment.docType>,
        //                                           And<CATransferExt.usrRefNbr, Equal<ARPayment.refNbr>>>>>>>,
        //                            Where<ARRegisterExt.usrPayReturn, Equal<True>,
        //                            And<Where<ARRegister.docType, Equal<ARDocType.refund>>>>> Records;

        public PXFilteredProcessingJoin<ARRegister, ProcessFilter,
                                   InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
                                             And<ARRegister.docType, Equal<ARPayment.docType>>>,
                                   InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>,
                                   InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>>>>,
                                       //InnerJoin<CATransfer, On<CATransferExt.usrType, Equal<ARPayment.docType>,
                                       //               And<CATransferExt.usrRefNbr, Equal<ARPayment.refNbr>>>>>>>,
                                       Where<ARRegisterExt.usrPayReturn, Equal<True>,
                                       And<Where<ARRegister.docType, Equal<ARDocType.refund>>>>> Records;


        public VoidCheckCustomer()
        {
            DateTime? d = this.Filter.Current.TransactionDate;
            Records.SetProcessDelegate(list => ReleaseDocs(list,d));
        }

        public static void ReleaseDocs(List<ARRegister> payments, DateTime? transactionDate)
        {

            foreach (ARRegister payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                ARPayment aPPayment = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
                CashAccount outAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(Transfer, aPPayment.CashAccountID);
                CashAccount inAccount = PXSelect<CashAccount,
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, aPPayment.GetExtension<ARRegisterExt>().UsrReturnBankName);
                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = outAccount.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                Transfer.Transfer.Current.InAccountID = inAccount.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = "Void Check Customer";
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

                ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();
                entry.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }

                entry.Document.Cache.SetValueExt<ARRegisterExt.usrReturnBankName>(entry.Document.Current, null);
                entry.Document.Cache.SetValueExt<ARRegisterExt.usrPayReturn>(entry.Document.Current, false);
               
                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
                entry.Document.Cache.IsDirty = true;
                entry.Document.Cache.Persist(PXDBOperation.Update);

                //entry.Actions.PressSave();

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