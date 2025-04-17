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
    public class VoidCheckProcess : PXGraph<VoidCheckProcess>
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
        public PXFilteredProcessingJoin<APRegister, ProcessFilter,
                                InnerJoin<APPayment, On<APRegister.refNbr, Equal<APPayment.refNbr>,
                                          And<APRegister.docType, Equal<APPayment.docType>>>,
                                InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<APPayment.cashAccountID>>,
                                InnerJoin<BAccount, On<APRegister.vendorID, Equal<BAccount.bAccountID>>,
                                    LeftJoin<CATransfer, On<CATransferExt.usrType, Equal<APPayment.docType>,
                                                   And<CATransferExt.usrRefNbr, Equal<APPayment.refNbr>>>>>>>,
                                    Where<APRegisterExt.usrChecked, Equal<True>,
                                    And<APRegisterExt.usrRefunded, Equal<False>,
                                    And<Where<APRegister.docType, Equal<APDocType.check>,
                                            Or<APRegister.docType, Equal<APDocType.prepayment>>>>>>> Records; 

        public VoidCheckProcess()
        {
            DateTime? d = this.Filter.Current.TransactionDate;
            Records.SetProcessDelegate(list => ReleaseDocs(list,d));
        }

        public static void ReleaseDocs(List<APRegister> payments, DateTime? transactionDate)
        {

            foreach (APRegister payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                APPayment aPPayment = PXSelectReadonly<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>,
                                              And<APPayment.docType,Equal<Required<APRegister.docType>>>>>.Select(Transfer, payment.RefNbr,payment.DocType);
                CashAccount outAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(Transfer, aPPayment.CashAccountID);
                CashAccount inAccount = PXSelect<CashAccount, 
                    Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(Transfer, aPPayment.GetExtension<APRegisterExt>().UsrCheckDispersant);
                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = outAccount.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                Transfer.Transfer.Current.InAccountID = inAccount.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = "Void Check Dispersant";
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
                
                APPaymentEntry entry = PXGraph.CreateInstance<APPaymentEntry>();
                entry.Document.Current = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>,
                                              And<APPayment.docType, Equal<Required<APRegister.docType>>>>>.Select(Transfer, payment.RefNbr, payment.DocType);
                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }

                entry.Document.Cache.SetValueExt<APRegisterExt.usrCheckDispersant>(entry.Document.Current, null);
                entry.Document.Cache.SetValueExt<APRegisterExt.usrChecked>(entry.Document.Current, false);
               
                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
                entry.Document.Cache.IsDirty = true;
                entry.Document.Cache.Persist(PXDBOperation.Update);

                //entry.Actions.PressSave();

            }
             
        }
    }
}