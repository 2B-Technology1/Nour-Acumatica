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
    public class BankProcess : PXGraph<BankProcess>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt]
            [PXUIField(DisplayName = "Cash Account")]
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
            //[PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = true)]

            public virtual DateTime? TransactionDate { get; set; }

            //public abstract class cashTotal : PX.Data.IBqlField
            //{
            //}
            //[PXDBDecimal(2)]
            //[PXDefault(TypeCode.Decimal, "0.0")]
            //[PXUIField(DisplayName = "Total Amount", Visibility = PXUIVisibility.Visible, Visible = true, Enabled =false)]
            ////Search<ARRegister.docBal, Aggregate<Sum<>>>)]
            //[PXFormula(null, typeof(SumCalc<ARRegister.docBal>))]
            //public virtual string CashTotal { get; set; }

        }
        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        [PXFilterable]
        //public PXFilteredProcessingJoin<ARRegister, ProcessFilter, InnerJoin<ARPayment, 
        //    On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
        //        And<ARRegister.docType,Equal<ARPayment.docType>>>,
        //    InnerJoin<BankCUC,On<BankCUC.cashAccountID,Equal<Current<ProcessFilter.cashAccountID>>>,
        //        LeftJoin<CADeposit,On<CADeposit.refNbr,Equal<ARRegisterExt.usrDepositRefNbr>>,
        //            InnerJoin<CashAccount,On<BankCUC.cucaccountid,Equal<CashAccount.cashAccountID>>,
        //                LeftJoin<Banks,On<ARRegisterExt.usrBankName,Equal<Banks.bankID>>,
        //                    InnerJoin<BAccount,On<ARRegister.customerID,Equal<BAccount.bAccountID>>>>>>>>,
        //    Where<ARRegisterExt.usrCUC, Equal<True>, 
        //        And<ARRegisterExt.usrBankDeposited, Equal<False>,
        //            And<ARPaymentExt.usrCUCAccount,Equal<CashAccount.cashAccountCD>,
        //                And<Where<ARRegister.docType, Equal<ARDocType.payment>,
        //                    Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>>>,OrderBy<Desc<ARRegister.docDate>>> Records;



        public PXFilteredProcessingJoin<ARRegister, ProcessFilter, InnerJoin<ARPayment,
           On<ARRegister.refNbr, Equal<ARPayment.refNbr>,
               And<ARRegister.docType, Equal<ARPayment.docType>>>,
           InnerJoin<BankCUC, On<BankCUC.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>,
               LeftJoin<CADeposit, On<CADeposit.refNbr, Equal<ARRegisterExt.usrDepositRefNbr>>,
                   InnerJoin<CashAccount, On<BankCUC.cucaccountid, Equal<CashAccount.cashAccountID>>,
                       LeftJoin<Banks, On<ARRegisterExt.usrBankName, Equal<Banks.bankID>>,
                           InnerJoin<BAccount, On<ARRegister.customerID, Equal<BAccount.bAccountID>>>>>>>>,
           Where<ARRegisterExt.usrCUC, Equal<True>,
             And<CADeposit.tranType, Equal<CATranType.cADeposit>,
               And<ARRegisterExt.usrBankDeposited, Equal<False>,
                   And<ARPaymentExt.usrCUCAccount, Equal<CashAccount.cashAccountCD>,
                       And<Where<ARRegister.docType, Equal<ARDocType.payment>,
                           Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>>>>, OrderBy<Desc<ARRegister.docDate>>> Records;

        public BankProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            Records.SetProcessDelegate(list => ReleaseDocs(list,cashAccountID,
                                                            this.Filter.Current.TransactionDate));
            
            
        }

        public static void ReleaseDocs(List<ARRegister> payments,int? cashAccount,DateTime? transactionDate)
        {
            
            foreach (ARRegister payment in payments)
            {
                CADepositEntry bankDeposit = PXGraph.CreateInstance<CADepositEntry>();
                ARPayment arPayment = PXSelectReadonly<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>.Select(bankDeposit, payment.RefNbr, payment.DocType);
                CashAccount CUCAccount = PXSelect<CashAccount,Where<CashAccount.cashAccountCD,Equal<Required<CashAccount.cashAccountCD>>>>.Select(bankDeposit,payment.GetExtension<ARRegisterExt>().UsrCUCAccount);
               

                bankDeposit.Document.Insert();
                bankDeposit.Document.Current.CashAccountID = cashAccount;
                bankDeposit.Document.Current.ExtRefNbr = "Bank Depositing";
                bankDeposit.Document.Current.TranDesc = payment.DocDesc;

                //bankDeposit.Actions.PressSave();
                //bankDeposit.Document.Update(bankDeposit.Document.Current);
                bankDeposit.Details.Insert();
                bankDeposit.Details.Current.OrigModule = "AR";
                bankDeposit.Details.Current.OrigDocType = payment.DocType;
                bankDeposit.Details.Current.OrigRefNbr = payment.RefNbr;
                bankDeposit.Details.Current.AccountID = CUCAccount.CashAccountID;
                bankDeposit.Details.Current.CuryTranAmt = payment.CuryOrigDocAmt;
                bankDeposit.Details.Current.CuryOrigAmt = payment.CuryOrigDocAmt;
                bankDeposit.Details.Current.OrigAmt = payment.OrigDocAmt;

                bankDeposit.Details.Current.OrigDrCr = "D";
                bankDeposit.Details.Current.OrigCuryInfoID = payment.CuryInfoID;

                bankDeposit.Document.Current.CuryDetailTotal = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.CuryTranAmt = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.ControlAmt = payment.CuryOrigDocAmt;
                bankDeposit.Document.Current.TranDate = transactionDate;
                bankDeposit.Document.Current.FinPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
                bankDeposit.Document.Current.TranPeriodID = transactionDate.Value.ToString("yyyy") + transactionDate.Value.ToString("MM");
                bankDeposit.Document.Current.BranchID = payment.BranchID;

                bankDeposit.Actions.PressSave();
                try
                {
                    PXLongOperation.StartOperation(bankDeposit, delegate() { CADepositEntry.ReleaseDoc(bankDeposit.Document.Current); });
                }
                catch { }
                ARPaymentEntry entry = PXGraph.CreateInstance<ARPaymentEntry>();

                entry.Document.Current = PXSelect<ARPayment, Where<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>,
                                              And<ARPayment.docType, Equal<Required<ARPayment.docType>>>>>.Select(bankDeposit, payment.RefNbr, payment.DocType);

                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }
                CashAccount accountCD = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(bankDeposit, cashAccount);

                entry.Document.Cache.SetValueExt<ARPaymentExt.usrBankAccount>(entry.Document. Current, accountCD.CashAccountCD);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrBankDeposited>(entry.Document.Current, true);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrRefunded>(entry.Document.Current, false);
                entry.Document.Cache.SetValueExt<ARPaymentExt.usrBankDepNbr>(entry.Document.Current, bankDeposit.Document.Current.RefNbr);
                entry.Document.Cache.SetValue<ARPayment.extRefNbr>(entry.Document.Current, "0");

                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
                entry.Document.Cache.IsDirty = true;
                entry.Document.Cache.Persist(PXDBOperation.Update);
                //entry.Actions.PressSave();




            }
             
        }
    }
}