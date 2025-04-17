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
    public class VoidCUCProcess : PXGraph<VoidCUCProcess>
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
        public PXFilteredProcessingJoin<ARRegister, ProcessFilter,
                                InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
                                                        And<ARRegister.docType, Equal<ARPayment.docType>>>,
                                InnerJoin<CashAccount, On<ARPayment.cashAccountID, Equal<CashAccount.cashAccountID>>,
                                LeftJoin<Banks, On<ARRegisterExt.usrBankName, Equal<Banks.bankID>>,
                                LeftJoin<CADeposit, On<CADeposit.refNbr, Equal<ARRegisterExt.usrDepositRefNbr>>,
                                InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>>>>>>,
                                    Where<ARRegisterExt.usrCUC, Equal<True>,
                                            And<ARRegisterExt.usrBankDeposited, Equal<False>,
                                            And<ARRegisterExt.usrRefunded, Equal<False>,
                                            And<Where<ARRegister.docType, Equal<ARDocType.payment>,
                                                Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>>>> Records;

     


        public VoidCUCProcess()
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
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, payment.GetExtension<ARRegisterExt>().UsrCUCAccount); 
                CashAccount inAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, payment.GetExtension<ARRegisterExt>().UsrNRAccount);
                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = outAccount.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                if (inAccount == null)
                    Transfer.Transfer.Current.InAccountID = entry.Document.Current.CashAccountID;
                else
                    Transfer.Transfer.Current.InAccountID = inAccount.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = "Void CUC Payment";
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrType = payment.DocType;
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrRefNbr = payment.RefNbr;
                Transfer.Transfer.Current.OutDate = transactionDate;
                Transfer.Transfer.Current.InDate = transactionDate;
                Transfer.Transfer.Current.Descr = payment.DocDesc;
                Transfer.Transfer.Current.InBranchID = inAccount.BranchID;
                Transfer.Transfer.Current.OutBranchID = outAccount.BranchID;
                Transfer.Transfer.Current.InPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM"); ;
                Transfer.Transfer.Current.InTranPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
                Transfer.Transfer.Current.OutPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
                Transfer.Transfer.Current.OutTranPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
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

                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCUCAccount>(entry.CurrentDocument.Current, null);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCUC>(entry.CurrentDocument.Current, false);
                entry.CurrentDocument.Cache.SetValue<CAAdj.extRefNbr>(entry.CurrentDocument.Current, "0000");
                entry.Document.Cache.IsDirty = true;
                entry.Document.Cache.Persist(PXDBOperation.Update);

            }

        }
    }
}