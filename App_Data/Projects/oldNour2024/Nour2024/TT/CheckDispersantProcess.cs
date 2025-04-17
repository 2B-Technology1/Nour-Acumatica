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
    public class CheckDispersantProcess : PXGraph<CheckDispersantProcess>
    {
         [Serializable]
        [PXHidden]
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
        }

        public PXCancel<ProcessFilter> Cancel;

        public PXFilter<ProcessFilter> Filter;

        [PXFilterable]

        //public PXFilteredProcessingJoin<APRegister, ProcessFilter, 
        //    InnerJoin<APPayment, On<APRegister.refNbr, Equal<APPayment.refNbr>,And<APRegister.docType,Equal<APPayment.docType>>>,
        //        InnerJoin<CashAccount,On<CashAccount.cashAccountID,Equal<APPayment.cashAccountID>>,
        //            LeftJoin<BankCheck,On<BankCheck.cashAccountID,Equal<Current<ProcessFilter.cashAccountID>>>,
        //                InnerJoin<BAccount, On<APRegister.vendorID, Equal<BAccount.bAccountID>>>>>>,
        //    Where< CashAccountExt.usrCheckDispersant,Equal<True>,
        //    And<APRegisterExt.usrRefunded, Equal<False>,
        //        And<Where<APRegisterExt.usrChecked, Equal<False>,
        //            And<APPayment.cashAccountID, Equal<BankCheck.checkAccountID>,
        //                And<Where<APRegister.docType,Equal<APDocType.check>,
        //                    And<APRegister.status,Equal<APDocStatus.open>,
        //                        Or<APRegister.docType, Equal<APDocType.check>,
        //                            And<APRegister.status,Equal<APDocStatus.closed>,
        //                                Or<APRegister.docType, Equal<APDocType.prepayment>,
        //                                    And<APRegister.status,Equal<APDocStatus.open>,
        //                                        Or<APRegister.docType, Equal<APDocType.prepayment>,
        //                                            And<APRegister.status,Equal<APDocStatus.closed>
        //                                                >>>>>>>>>>>>>>> Records;



        public PXFilteredProcessingJoin<APRegister, ProcessFilter,
            InnerJoin<APPayment, On<APRegister.refNbr, Equal<APPayment.refNbr>, And<APRegister.docType, Equal<APPayment.docType>>>,
                InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<APPayment.cashAccountID>>,
                    LeftJoin<BankCheck, On<BankCheck.cashAccountID, Equal<Current<ProcessFilter.cashAccountID>>>,
                        InnerJoin<BAccount, On<APRegister.vendorID, Equal<BAccount.bAccountID>>>>>>,
            Where<CashAccountExt.usrCheckDispersant, Equal<True>,
                And<APRegisterExt.usrChecked, Equal<False>,
                    And<APPayment.cashAccountID, Equal<BankCheck.checkAccountID>,
                        And<APRegisterExt.usrRefunded, Equal<False>,
                            And<
                                Where2<Where<APRegister.docType, Equal<APDocType.check>, Or<APRegister.docType, Equal<APDocType.prepayment>>>,
                                    And<Where<APRegister.status, Equal<APDocStatus.open>, Or<APRegister.status, Equal<APDocStatus.closed>>>>>
                                    >>>>>> Records;
        
        
        public CheckDispersantProcess()
        {
            int? cashAccountID = this.Filter.Current.CashAccountID;
            // Acuminator disable once PX1008 LongOperationDelegateSynchronousExecution [Justification]
            Records.SetProcessDelegate(list => ReleaseDocs(list,cashAccountID,
                                                            this.Filter.Current.TransactionDate));
        }

        public static void ReleaseDocs(List<APRegister> payments, int? cashAccount, DateTime? transactionDate)
        {

            foreach (APRegister payment in payments)
            {
                CashTransferEntry Transfer = PXGraph.CreateInstance<CashTransferEntry>();
                APPayment aPPayment = PXSelectReadonly<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>,
                         
                       And<APPayment.docType,Equal<Required<APRegister.docType>>>>>.Select(Transfer, payment.RefNbr,payment.DocType);

               


                Transfer.Transfer.Insert();
                Transfer.Transfer.Current.OutAccountID = cashAccount;
                Transfer.Transfer.Current.CuryTranOut = payment.CuryOrigDocAmt;
                Transfer.Transfer.Current.InAccountID = aPPayment.CashAccountID;
                Transfer.Transfer.Current.OutExtRefNbr = payment.GetExtension<APRegisterExt>().UsrCheckNbr + "-Check Dispersant";
                Transfer.Transfer.SetValueExt<CATransferExt.usrType>(Transfer.Transfer.Current, payment.DocType);
                Transfer.Transfer.SetValueExt<CATransferExt.usrRefNbr>(Transfer.Transfer.Current, payment.RefNbr);
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
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("catrxrelease error");

                tran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATransfer.tranIn>>>>.
                    Select(Transfer, transfer.TranIDOut);
                if (tran == null)
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("tran is null");
                try
                {
                    PXLongOperation.StartOperation(Transfer, delegate() { CATrxRelease.GroupRelease(list, false); });
                }
                catch { }

                APPaymentEntry entry = PXGraph.CreateInstance<APPaymentEntry>();

                entry.Document.Current = PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APRegister.refNbr>>
                    , And<APPayment.docType, Equal<Required<APRegister.docType>>>>>
                    .Select(Transfer, payment.RefNbr, payment.DocType);

                try
                {
                    entry.Document.Update(entry.Document.Current);
                }
                catch
                { }

                CashAccount accountCD =
                    PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.
                    Select(Transfer, cashAccount);

                entry.Document.Cache.SetValueExt<APRegisterExt.usrCheckDispersant>(entry.Document.Current, accountCD.CashAccountCD);
                entry.Document.Cache.SetValueExt<APRegisterExt.usrChecked>(entry.Document.Current, true);
                entry.Document.Cache.SetStatus(entry.Document.Current, PXEntryStatus.Modified);
                entry.Document.Cache.IsDirty = true;
                entry.Document.Cache.Persist(PXDBOperation.Update);
                

            }

        }
    }
}