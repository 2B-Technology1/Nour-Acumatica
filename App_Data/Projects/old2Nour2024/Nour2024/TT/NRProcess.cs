using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.GL;
using PX.Objects.AR;
using PX.Objects.CR;


namespace Maintenance
{
    public class NRProcess : PXGraph<NRProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            #region cashAccountID
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector
                (typeof(Search<CashAccount.cashAccountID, Where<CashAccountExt.usrNR, Equal<True>>>),
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
            public abstract class transactionDate : PX.Data.IBqlField
            {
            }
            [PXDate()]
            //[PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]

            public virtual DateTime? TransactionDate { get; set; }
            #endregion

            #region cashAccountFilter
            public abstract class cashAccountFilter : IBqlField { }
            [PXInt]
            [PXUIField(DisplayName = "Account Filter")]
            [PXSelector
                (typeof(Search2<CashAccount.cashAccountID,
                            InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<CashAccount.cashAccountID>>>,
                    Where<CashAccountExt.usrNR, Equal<True>, And<CashAccountAccess.userID, Equal<Current<AccessInfo.userID>>>>>),
                        new Type[]
                        {
                    typeof(CashAccount.cashAccountCD),
                    typeof(CashAccount.descr),
                    typeof(CashAccount.curyID),
                        },
                DescriptionField = typeof(CashAccount.descr),
                SubstituteKey = typeof(CashAccount.cashAccountCD))]
            public virtual int? CashAccountFilter { get; set; }
            #endregion

            #region totalAmount
            public abstract class totalAmount : PX.Data.IBqlField
            {
            }
            [PXDecimal]
            [PXUIField(DisplayName = "Total Amount",Enabled =false)]
            public virtual decimal? TotalAmount { get; set; }
            #endregion
        }
        
        public PXCancel<ProcessFilter> Cancel;
        public PXFilter<ProcessFilter> Filter;

        //[PXFilterable]
        public PXFilteredProcessingJoin<ARRegister,ProcessFilter,
            InnerJoin<ARPayment, On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
                And<ARRegister.docType, Equal<ARPayment.docType>>>,
                InnerJoin<CashAccount, On<ARRegisterExt.usrNRAccount, Equal<CashAccount.cashAccountCD>>//,
                    /*InnerJoin<CATransfer, On<CATransfer.inAccountID, Equal<CashAccount.cashAccountID>>*/,
                    LeftJoin<Banks, On<ARRegisterExt.usrBankName, Equal<Banks.bankID>>,
                        InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>,
                            InnerJoin<CashAccountAccess, On<CashAccountAccess.cashAccountID, Equal<ARPayment.cashAccountID>>>>>>>/*>*/,

            Where<ARRegisterExt.usrCUC, Equal<False>,
                And<CashAccountExt.usrNR, Equal<True>,
                And<CashAccountAccess.userID, Equal<Current<AccessInfo.userID>>,
                    And2<Where<ARRegister.docType, Equal<ARDocType.payment>, Or<ARRegister.docType, Equal<ARDocType.prepayment>>>,
                    And<ARRegisterExt.usrRefunded, Equal<False>,
                        And<Where<ARRegister.status, Equal<ARDocStatus.open>, Or<ARRegister.status, Equal<ARDocStatus.closed>>>>>>>>>> Records;

        protected virtual IEnumerable records()
        {

            ProcessFilter filter =Filter.Current;
            int startRow = PXView.StartRow;
            int totalRows = 0;

            BqlCommand query = Records.View.BqlSelect;

            FilterTransactions(ref query, filter);

            PXView acView = new PXView(this, true, query);
            var list = acView.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
                ref startRow, PXView.MaximumRows, ref totalRows);
            PXView.StartRow = 0;

            #region Delete Current User (Temp)
            //code here to delete the current user from the temp table
            UserCurrentEntry usr = new UserCurrentEntry();
            usr.ProviderDelete<UserCurrent>();
            #endregion

            return list;
        }

        private void FilterTransactions(ref BqlCommand query, ProcessFilter filter)
        {
            if (filter != null)
            {
                if (filter.CashAccountFilter != null)
                {
                    query = query.WhereAnd<Where<CashAccount.cashAccountID, Equal<Current<ProcessFilter.cashAccountFilter>>>>();
                   // string sql = query.GetText(this);//to try it in SQL Server
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

                if (tot == null) tot = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    object i1 = list[i];
                    var amt = (ARRegister)((PXResult)i1)[0];
                    tot += amt.CuryOrigDocAmt;
                }

                val = tot;
                tot = 0;
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
            val = 0;
        }

        public NRProcess()
        {
          
            
            int? cashAccountID = this.Filter.Current.CashAccountID;
            DateTime? transactionDate = this.Filter.Current.TransactionDate;

            Records.SetProcessDelegate(list => ReleaseDocs(list, cashAccountID, transactionDate));
        }

        public static void ReleaseDocs(List<ARRegister> payments, int? cashAccount, DateTime? transactionDate)
        {

            foreach (ARRegister payment in payments)
            {
                CADepositEntry bankDeposit = PXGraph.CreateInstance<CADepositEntry>();

                ARPayment arPayment = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>
                                              .Select(bankDeposit, payment.RefNbr, payment.DocType);

                bankDeposit.Document.Insert();
                bankDeposit.Document.Current.CashAccountID = cashAccount;
                bankDeposit.Document.Current.ExtRefNbr = "NR";
                bankDeposit.Document.Current.TranDesc = payment.DocDesc;
                //bankDeposit.Actions.PressSave();
                //bankDeposit.Document.Update(bankDeposit.Document.Current);
                bankDeposit.Details.Insert();
                bankDeposit.Details.Current.OrigModule = "AR";
                bankDeposit.Details.Current.OrigDocType = payment.DocType;
                bankDeposit.Details.Current.OrigRefNbr = payment.RefNbr;
                bankDeposit.Details.Current.AccountID = arPayment.CashAccountID;
                bankDeposit.Details.Current.CuryTranAmt = payment.CuryOrigDocAmt;
                bankDeposit.Details.Current.CuryOrigAmt = payment.CuryOrigDocAmt;
                bankDeposit.Details.Current.OrigAmt = payment.OrigDocAmt;
                bankDeposit.Document.Current.CuryDetailTotal = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.CuryTranAmt = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.ControlAmt = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.TranDate = transactionDate;
                bankDeposit.Document.Current.FinPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
                bankDeposit.Document.Current.TranPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");

                bankDeposit.Actions.PressSave();
                try
                {
                    PXLongOperation.StartOperation(bankDeposit, delegate() { CADepositEntry.ReleaseDoc(bankDeposit.Document.Current); });
                }
                catch { }

                ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();

                /*instead of arPayment, to wrkarnd arregister updated...*/
                entry.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>
                                              .Select(bankDeposit, payment.RefNbr, payment.DocType);
                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }

                CashAccount accountCD = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
                    Select(bankDeposit, cashAccount);

                entry.Document.Cache.SetValueExt<ARPaymentExt.usrNRAccount>(entry.Document.Current, accountCD.CashAccountCD);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrCUCAccount>(entry.Document.Current, accountCD.CashAccountCD);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrNR>(entry.Document.Current, true);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrNRRefNbr>(entry.Document.Current, bankDeposit.Document.Current.RefNbr);
                entry.Document.Cache.SetValue<ARPayment.extRefNbr>(entry.Document.Current, "000");

                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
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