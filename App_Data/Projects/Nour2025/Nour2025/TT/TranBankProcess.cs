using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CR;


namespace Maintenance
{
    public class TranBankProcess : PXGraph<TranBankProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            #region cashAccountID
            public abstract class cashAccountID : PX.Data.IBqlField {}
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector
                (typeof(Search2<CashAccount.cashAccountID,
                            InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CashAccount.cashAccountID>>>,
                    Where<CashAccountExt.usrBank, Equal<True>,
                    And<CashAccountAccess.userID, Equal<Current<AccessInfo.userID>>>>>),
                        new Type[]
                        {
                    typeof(CashAccount.cashAccountCD),
                    typeof(CashAccount.descr),
                    typeof(CashAccount.curyID),
                        },
                DescriptionField = typeof(CashAccount.descr),
                SubstituteKey = typeof(CashAccount.cashAccountCD))]
            public virtual int? CashAccountID { get; set; }
            #endregion

            #region transactionDate
            public abstract class transactionDate : PX.Data.IBqlField{}
            [PXDate()]
           // [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]

            public virtual DateTime? TransactionDate { get; set; }
            #endregion

            #region cashAccountFilter
            //public abstract class cashAccountFilter : IBqlField{}
            //[PXInt]
            //[PXUIField(DisplayName = "Account Filter")]
            //[PXSelector
            //    (typeof(Search5<CashAccount.cashAccountID,
            //                InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CashAccount.cashAccountID>>,
            //                    InnerJoin<UserCurrent, On<CashAccountAccess.userID, Equal<UserCurrent.userID>>>>,
            //        Where<CashAccountExt.usrCUC, Equal<True>, Or<CashAccountExt.usrBank, Equal<True>>>,
            //        Aggregate<GroupBy<CashAccount.cashAccountID>>>),
            //            new Type[]
            //            {
            //        typeof(CashAccount.cashAccountCD),
            //        typeof(CashAccount.descr),
            //        typeof(CashAccount.curyID),
            //            },
            //    DescriptionField = typeof(CashAccount.descr),
            //    SubstituteKey = typeof(CashAccount.cashAccountCD))]
            //public virtual int? CashAccountFilter { get; set; }
            #endregion

            #region totalAmount
            public abstract class totalAmount : PX.Data.IBqlField
            {
            }
            [PXDecimal]
            [PXUIField(DisplayName = "Total Amount", Enabled = false)]
            public virtual decimal? TotalAmount { get; set; }
            #endregion
        }
        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        //[PXFilterable]
        public PXFilteredProcessingJoin<CAAdj, ProcessFilter,
                                InnerJoin<CashAccount, On<CAAdj.cashAccountID, Equal<CashAccount.cashAccountID>>,
                                InnerJoin<BankCUC,On<BankCUC.cashAccountID,Equal<Current<ProcessFilter.cashAccountID>>>,
                                LeftJoin<Banks,On<CAAdjExt.usrBankName,Equal<Banks.bankID>>,
                                LeftJoin<CATransfer,On<CATransfer.transferNbr,Equal<CAAdjExt.usrTransferRefNbr>>,
                                InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CAAdj.cashAccountID>>>>>>>,
                                    Where<CAAdjExt.usrCUC, Equal<True>,
                                    And<CashAccountAccess.userID, Equal<Current<AccessInfo.userID>>,
                                            And<CAAdjExt.usrBankDeposited, Equal<False>,
                                            And<CAAdjExt.usrCUCAccount, Equal<BankCUC.cucaccountid>>>>>> Records;

        public TranBankProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            Records.SetProcessDelegate(list => ReleaseDocs(list,cashAccountID,
                                                            this.Filter.Current.TransactionDate)); 
        }

        public static void ReleaseDocs(List<CAAdj> payments, int? cashAccount, DateTime? transactionDate)
        {

            foreach (CAAdj payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                Transfer.Transfer.Insert();
                CAAdj caAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>,
                                        And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(Transfer, payment.AdjRefNbr, payment.AdjTranType);
                Transfer.Transfer.Current.OutAccountID = caAdj.GetExtension<CAAdjExt>().UsrCUCAccount;
                Transfer.Transfer.Current.CuryTranOut = caAdj.CuryTranAmt;
                Transfer.Transfer.Current.InAccountID = cashAccount;
                Transfer.Transfer.Current.OutExtRefNbr = "Transaction Bank Processing";
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
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrBankAccount>(entry.CurrentDocument.Current, cashAccount);
                entry.CurrentDocument.Cache.SetValueExt<CAAdjExt.usrBankDeposited>(entry.CurrentDocument.Current, true);
                entry.CurrentDocument.Cache.SetValue<CAAdj.extRefNbr>(entry.CurrentDocument.Current, "00");

                entry.CurrentDocument.Cache.IsDirty = true;
                entry.CurrentDocument.Cache.Persist(PXDBOperation.Update);

            }
             
        }

        private void FilterTransactions(ref BqlCommand query, ProcessFilter filter)
        {
            if (filter != null)
            {
                if (filter.CashAccountID != null)
                {
                    query = query.WhereAnd<Where<CAAdj.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>>();
                }
            }
        }

        public decimal? tot;

        protected void ProcessFilter_TotalAmount_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            ProcessFilter filter = Filter.Current;
            decimal? val = 0m;
            if (filter != null)
            {
                int startRow = PXView.StartRow;
                int totalRows = 0;

                BqlCommand query = Records.View.BqlSelect;
                FilterTransactions(ref query, filter);

                PXView acView = new PXView(this, true, query);
                var list = acView.
                   Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
                   ref startRow, PXView.MaximumRows, ref totalRows);
                //PXView.StartRow = 0;

                if (tot == null) tot = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    object i1 = list[i];
                    var amt = (CAAdj)((PXResult)i1)[0];
                    tot += amt.CuryOrigDocAmt;
                }

                val = tot;
                tot = 0;
                //}
            }
            e.ReturnValue = val; //5 times hit!!

            UserCurrentEntry usr = new UserCurrentEntry();

            if (val == 0 & e.Row == null)
            {
                #region Insert Current User (Temp)
                //code here to insert the current user to the temp table
                usr.ProviderInsert<UserCurrent>(new PXDataFieldAssign("UserID", Accessinfo.UserID));
                #endregion
            }
        }

        public PXAction<ProcessFilter> ViewProduct;

        [PXButton]
        protected virtual void viewProduct()
        {
            CAAdj row = (CAAdj)Records.Cache.Current;
            // Creating the instance of the graph
            CashTransferEntry entry = PXGraph.CreateInstance<CashTransferEntry>();
            // Setting the current product for the graph
            entry.Transfer.Current = entry.Transfer.Search<CATransfer.transferNbr>(row.GetExtension<CAAdjExt>().UsrTransferRefNbr);
            // If the product is found by its ID, throw an exception to open
            // a new window (tab) in the browser
            if (entry.Transfer.Current != null)
            {
                throw new PXRedirectRequiredException(entry, true, "Details");
            }
        }

    }
}