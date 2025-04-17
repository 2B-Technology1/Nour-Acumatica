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
    public class TranCheckDispersantProcess : PXGraph<TranCheckDispersantProcess>
    {
         [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector(typeof(Search2<CashAccount.cashAccountID,
                            InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CashAccount.cashAccountID>>>,
                    Where<CashAccountExt.usrBank, Equal<True>,And<CashAccountAccess.userID, Equal<Current<AccessInfo.userID>>>>>),
                        new Type[]
            {
                  typeof(CashAccount.cashAccountCD),
              typeof(CashAccount.descr),
              typeof(CashAccount.curyID),
            },
                        DescriptionField = typeof(CashAccount.descr),
                SubstituteKey = typeof(CashAccount.cashAccountCD))]


            //[PXSelector(typeof(Search2<CashAccount.cashAccountID,
            //               InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CashAccount.cashAccountID>>,
            //                   InnerJoin<UserCurrent, On<CashAccountAccess.userID, Equal<UserCurrent.userID>>>>,
            //       Where<CashAccountExt.usrBank, Equal<True>>>),
            //           new Type[]
            //{
            //      typeof(CashAccount.cashAccountCD),
            //  typeof(CashAccount.descr),
            //  typeof(CashAccount.curyID),
            //},
            //           DescriptionField = typeof(CashAccount.descr),
            //   SubstituteKey = typeof(CashAccount.cashAccountCD))]


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
                            And<CAAdj.released, Equal<True>,And<CAAdjExt.usrReversed,Equal<False>>>>>>>> Records;


        public TranCheckDispersantProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            Records.SetProcessDelegate(list => ReleaseDocs(list, cashAccountID,
                                                            this.Filter.Current.TransactionDate));
        }

        public static void ReleaseDocs(List<CAAdj> payments, int? cashAccount, DateTime? transactionDate)
        {

            foreach (CAAdj payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                Transfer.Transfer.Insert();
                CAAdj caAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>,
                                        And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.
                                        Select(Transfer, payment.AdjRefNbr, payment.AdjTranType);

                Transfer.Transfer.Current.OutAccountID = cashAccount;
                Transfer.Transfer.Current.CuryTranOut = caAdj.CuryTranAmt;
                Transfer.Transfer.Current.InAccountID = caAdj.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = "Transaction Check Dispersant";
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
                catch { }
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCheckDispersant>(entry.CurrentDocument.Current, cashAccount);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrCheck>(entry.CurrentDocument.Current, true);
                entry.CurrentDocument.Cache.SetValue<CAAdj.extRefNbr>(entry.CurrentDocument.Current, "000000");

                entry.CurrentDocument.Cache.IsDirty = true;
                entry.CurrentDocument.Cache.Persist(PXDBOperation.Update);

            }

        }


        public PXAction<ProcessFilter> ViewProduct;

        [PXButton]
        protected virtual void viewProduct()
        {
            CAAdj row = (CAAdj)Records.Cache.Current;
            // Creating the instance of the graph
            CATranEntry entry = PXGraph.CreateInstance<CATranEntry>();
            // Setting the current product for the graph
            
            entry.CAAdjRecords.Current = entry.CAAdjRecords.Search<CAAdj.adjRefNbr>( row.AdjRefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (entry.CurrentDocument.Current != null)
            {
                throw new PXRedirectRequiredException(entry, true, "Details");
            }
        }
    }
}