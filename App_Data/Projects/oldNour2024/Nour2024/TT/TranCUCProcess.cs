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
    public class TranCUCProcess : PXGraph<TranCUCProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector(typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrCUC, Equal<True>>>),
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


            public abstract class documentRef : PX.Data.IBqlField
            {
            }
            [PXString]
            [PXUIField(DisplayName = "Document Ref.")]
            
            public virtual string DocumentRef { get; set; }
        }
        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        //[PXFilterable]
        //public PXFilteredProcessingJoin<CAAdj, ProcessFilter,
        //                        InnerJoin<CashAccount, On<CAAdj.cashAccountID, Equal<CashAccount.cashAccountID>>,
        //                        LeftJoin<Banks, On<CAAdjExt.usrBankName, Equal<Banks.bankID>>,
        //                        InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CAAdj.entryTypeID>>>>>,
        //                            Where<CAEntryType.drCr, Equal<CADrCr.cADebit>,
        //                            And<CAAdjExt.usrCUC, Equal<False>,
        //                            And<CashAccountExt.usrNR, Equal<True>,
        //                            And<CAAdj.released, Equal<True>>>>>> Records;


        [PXFilterable]

        public PXFilteredProcessingJoin<CAAdj, ProcessFilter,

                                InnerJoin<CashAccount, On<CAAdj.cashAccountID, Equal<CashAccount.cashAccountID>>,

                                LeftJoin<Banks, On<CAAdjExt.usrBankName, Equal<Banks.bankID>>,

                                InnerJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<CAAdj.entryTypeID>>>>>,

                                    Where<CAEntryType.drCr, Equal<CADrCr.cADebit>,

                                    And<CAAdjExt.usrCUC, Equal<False>,

                                    And<CashAccountExt.usrNR, Equal<True>,

                                    And<CAAdj.released, Equal<True>, And<CAAdjExt.usrReversed, Equal<False>>>>>>> Records;


        public TranCUCProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            Records.SetProcessDelegate(list => ReleaseDocs(list, cashAccountID,
                                                           this.Filter.Current.TransactionDate,
                                                           this.Filter.Current.DocumentRef));
        }

        public static void ReleaseDocs(List<CAAdj> payments,int? cashAccount,DateTime? transactionDate,string documentRef)
        {
            
            foreach (CAAdj payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                Transfer.Transfer.Insert();
                CAAdj caAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>,
                                        And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(Transfer, payment.AdjRefNbr, payment.AdjTranType);
                Transfer.Transfer.Current.OutAccountID = caAdj.CashAccountID;
                Transfer.Transfer.Current.CuryTranOut = caAdj.CuryTranAmt;
                Transfer.Transfer.Current.InAccountID = cashAccount;
                Transfer.Transfer.Current.OutExtRefNbr = documentRef;
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrType = caAdj.DocType;
                Transfer.Transfer.Current.GetExtension<CATransferExt>().UsrRefNbr = caAdj.RefNbr;
                Transfer.Transfer.Current.OutDate = transactionDate;
                Transfer.Transfer.Current.InDate = transactionDate;
                Transfer.Transfer.Current.Descr = caAdj.TranDesc;
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
                catch { }

                CATranEntry entry = PXGraph.CreateInstance<CATranEntry>();
                entry.CurrentDocument.Current = caAdj;
                

                try
                {
                    entry.CurrentDocument.Update(entry.CurrentDocument.Current);
                }
                catch
                { }
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCUCAccount>(entry.CurrentDocument.Current, cashAccount);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCUC>(entry.CurrentDocument.Current, true);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrTransferRefNbr>(entry.CurrentDocument.Current, Transfer.Transfer.Current.TransferNbr);
                entry.CurrentDocument.Cache.SetValue<CAAdj.extRefNbr>(entry.CurrentDocument.Current, "00000");

                entry.CurrentDocument.Cache.IsDirty = true;
                entry.CurrentDocument.Cache.Persist(PXDBOperation.Update);
            }
             
        }
}
}