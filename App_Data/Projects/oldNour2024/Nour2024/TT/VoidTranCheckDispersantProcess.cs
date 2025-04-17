using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.AP;
using PX.Objects.CR;


namespace Maintenance
{
    public class VoidTranCheckDispersantProcess : PXGraph<VoidTranCheckDispersantProcess>
    {
         [Serializable]
        [PXHidden]
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
        public PXFilteredProcessingJoin<CAAdj, ProcessFilter,
                                InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CAAdj.cashAccountID>>,
                                InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CAAdj.entryTypeID>>,
                                LeftJoin<Banks, On<CAAdjExt.usrBankName, Equal<Banks.bankID>>,
                                LeftJoin<BankCheck, On<BankCheck.cashAccountID, Equal<CAAdjExt.usrCheckDispersant>>>>>>,
                                    Where<CashAccountExt.usrCheckDispersant, Equal<True>,
                                         And<CAAdjExt.usrCheck, Equal<True>,
                                         And<CAAdj.cashAccountID, Equal<BankCheck.checkAccountID>,
                                         And<CAEntryType.drCr, Equal<CADrCr.cACredit>,
                                         And<CAAdj.released, Equal<True>>>>>>> Records;


        public VoidTranCheckDispersantProcess()
        {
            DateTime? d = this.Filter.Current.TransactionDate;
            Records.SetProcessDelegate(list => ReleaseDocs(list, d));
        }

        public static void ReleaseDocs(List<CAAdj> payments, DateTime? transactionDate)
        {

            foreach (CAAdj payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                Transfer.Transfer.Insert();
                CAAdj caAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>,
                                        And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(Transfer, payment.AdjRefNbr, payment.AdjTranType);
                Transfer.Transfer.Current.OutAccountID = payment.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryTranAmt;
                Transfer.Transfer.Current.InAccountID = caAdj.GetExtension<CAAdjExt>().UsrCheckDispersant;
                Transfer.Transfer.Current.OutExtRefNbr = "Void Transaction Check Dispersant";
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrType = payment.DocType;
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrRefNbr = payment.RefNbr;
                Transfer.Transfer.Current.OutDate = transactionDate;
                Transfer.Transfer.Current.InDate = transactionDate;
                Transfer.Transfer.Current.Descr = payment.TranDesc;
                Transfer.Transfer.Update(Transfer.Transfer.Current);
                Transfer.Actions.PressSave();
                CATransfer transfer = Transfer.Transfer.Current;
                List<CARegister> list = new List<CARegister>();
                CATran tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDIn);
                if (tran != null)
                    list.Add(CATrxRelease.CARegister(transfer, tran));
                else
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("catrxrelease error");

                tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.Select(Transfer, transfer.TranIDOut);
                if (tran == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("tran is null");
                try
                {
                    PXLongOperation.StartOperation(Transfer, delegate() { CATrxRelease.GroupRelease(list, false); });
                }
                catch { }

                CATranEntry entry = PXGraph.CreateInstance<CATranEntry>();
                entry.CurrentDocument.Current = caAdj;
                try
                {
                    entry.CurrentDocument.Update(entry.CurrentDocument.Current);
                }
                catch { }
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCheckDispersant>(entry.CurrentDocument.Current, null);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCheck>(entry.CurrentDocument.Current, false);
                entry.CurrentDocument.Cache.SetValue<CAAdj.extRefNbr>(entry.CurrentDocument.Current, "0000");

                entry.CurrentDocument.Cache.IsDirty = true;
                entry.CurrentDocument.Cache.Persist(PXDBOperation.Update);


            }
             
        }
    }
}